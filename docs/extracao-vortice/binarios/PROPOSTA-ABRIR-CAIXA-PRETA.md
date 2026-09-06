# Proposta — como abrir a caixa-preta do cliente Vórtice

> Documento de **proposta**. Nada aqui foi executado. Descreve o procedimento, quem precisa
> autorizar, o que se espera aprender e os riscos.
>
> Contexto: o inventário de binários ([INVENTARIO-BINARIOS.md](INVENTARIO-BINARIOS.md)) mostrou que
> o cliente desktop do Vórtice — os 31 executáveis em `C:\Vortice\TDev\App` — é compilado em
> **Gupta/OpenText Team Developer 7.3.4**. Isso muda completamente a estratégia de descoberta.

---

## 1. Por que o cliente desktop não se abre por descompilação

O Team Developer não gera IL nem código nativo legível. Ele compila a linguagem **SAL** para um
**p-code proprietário da OpenText**, empacotado dentro de um `.exe` que é essencialmente um
carregador: o executável de 15 MB do `vCRM Atendente.exe` é o runtime (`cdlli73.dll` e companhia)
mais um blob de p-code.

Consequências práticas:

| | |
|---|---|
| **Não existe descompilador público** | Ao contrário de .NET (ILSpy/dnSpy) ou Java, não há ferramenta de mercado que reconstrua SAL a partir do p-code |
| **Strings dão pouco** | Um `strings` no executável devolve fragmentos de SQL e rótulos de tela, sem estrutura nem ordem de execução |
| **O formato é instável entre versões** | Mesmo um esforço de engenharia reversa valeria só para a build atual |
| **O runtime é a documentação** | Toda a lógica que interessa vira, no fim, **SQL enviado ao SQL Server** |

**Conclusão:** a leitura correta do cliente Gupta não é estática, é **dinâmica** — observar o SQL
que ele emite. É mais barato, mais confiável e não depende de licença de descompilação.

> Os **27 executáveis .NET em `C:\Vortice\NT`** (Vortico Server, Jupiter Service, Bi Integrador…)
> são um caso diferente e estão tratados na seção 5.

---

## 2. O procedimento proposto: capturar o SQL do cliente

Duas técnicas, complementares. A primeira é a principal.

### 2.1 Extended Events no SQL Server (recomendada)

Captura no **servidor de banco** (`COLWCRM`), filtrando pela sessão do usuário-cobaia. Não toca no
terminal server, não altera o cliente, e é a única que enxerga **todas** as máquinas de uma vez.

**Passo a passo**

1. **Escolher a cobaia.** Um usuário do CRM operando em uma sessão RDP identificável.
   O filtro mais estável é a combinação `nt_username` + `client_hostname` — o login SQL é
   compartilhado (`crm`), então filtrar por `server_principal_name` sozinho **não serve**.

2. **Criar a sessão de eventos** (o DBA executa; o agente não tem permissão nem deve ter):

   ```sql
   CREATE EVENT SESSION [vtc_captura_cliente] ON SERVER
   ADD EVENT sqlserver.rpc_completed (
       ACTION (sqlserver.client_app_name, sqlserver.client_hostname,
               sqlserver.database_name, sqlserver.nt_username,
               sqlserver.session_id, sqlserver.sql_text, sqlserver.username)
       WHERE  sqlserver.client_hostname = N'<MAQUINA_DA_COBAIA>'
   ),
   ADD EVENT sqlserver.sql_batch_completed (
       ACTION (sqlserver.client_app_name, sqlserver.client_hostname,
               sqlserver.database_name, sqlserver.nt_username,
               sqlserver.session_id, sqlserver.sql_text, sqlserver.username)
       WHERE  sqlserver.client_hostname = N'<MAQUINA_DA_COBAIA>'
   )
   ADD TARGET package0.event_file (
       SET filename = N'<PASTA_DE_TRACE>\vtc_captura.xel',
           max_file_size = 256,          -- MB
           max_rollover_files = 8         -- teto de 2 GB
   )
   WITH (MAX_MEMORY = 8MB, EVENT_RETENTION_MODE = ALLOW_SINGLE_EVENT_LOSS,
         MAX_DISPATCH_LATENCY = 10 SECONDS, STARTUP_STATE = OFF);
   ```

   ⚠️ **`STARTUP_STATE = OFF` e teto de arquivo são obrigatórios.** Sessão sem limite enche o disco
   do servidor de banco — e o histórico deste ambiente (`PENDENCIAS.md`, seção I) mostra que disco
   cheio já derrubou uma migração aqui.

3. **Rodar o roteiro de telas.** A cobaia percorre um roteiro escrito, **anotando o horário de cada
   passo** — é isso que permite correlacionar depois. Exemplo:

   | hh:mm:ss | Tela | Ação |
   |---|---|---|
   | 09:12:04 | Atendimento | abrir pessoa 123456 |
   | 09:12:40 | Atendimento | gravar histórico |
   | 09:13:15 | Processo | avançar fase |

4. **Parar e ler.** `ALTER EVENT SESSION [vtc_captura_cliente] ON SERVER STATE = STOP;` e depois
   `DROP EVENT SESSION`. A leitura do `.xel` é offline, com
   `sys.fn_xe_file_target_read_file`, e pode ser feita pelo agente em modo somente leitura.

**Campos a capturar e por quê**

| Campo | Serve para |
|---|---|
| `sql_text` / `statement` | **o objetivo**: a query ou o `EXEC` que a tela dispara |
| `client_hostname` | isolar a cobaia (o filtro) |
| `nt_username` | identificar quem operou, quando o hostname é compartilhado |
| `client_app_name` | separar `vCRM Atendente` de `Vortico Server` e do Jupiter |
| `session_id` | agrupar tudo o que pertence à mesma sessão do cliente |
| `database_name` | confirmar que é `CRM` e não `CRM_HOMO` |
| `timestamp` | **correlacionar com o roteiro** — é a chave de tudo |
| `duration` / `logical_reads` | bônus: já entrega o mapa das telas lentas |

**Permissões que o DBA precisa conceder (a quem for executar):**

- `ALTER ANY EVENT SESSION` — criar, iniciar e parar a sessão;
- `VIEW SERVER STATE` — ler os DMVs e o alvo de arquivo;
- permissão de escrita **na pasta de trace**, para a conta de serviço do SQL Server.

> Nenhuma dessas permissões deve ir para a conta `CRM_Leitura` usada pelo agente. A captura é uma
> operação pontual, feita pelo DBA, com hora marcada.

### 2.2 Trace de ODBC no terminal server (complementar)

O runtime Gupta tem trace próprio, ligado no `SQL.INI`. É o mesmo mecanismo já usado no
diagnóstico do erro 401 (memória `vortice-erro-401-dlls-odbc-app`).

Em `C:\Vortice\TDev\SQL.INI`, seção `[odbcrtr]`:

```ini
odbctrace=on
odbctracefile=C:\temp\vtc_trace.log
```

Vantagem: mostra a chamada **do lado do cliente**, incluindo o que ele monta antes de enviar.
Desvantagens que tornam essa técnica secundária:

- ⚠️ **É um arquivo por máquina, e o dono do arquivo é quem rodou primeiro.** Se um administrador
  testar antes do usuário comum, o usuário recebe `Unable to open ODBC trace file: Permission denied`
  **e não consegue conectar**. Está documentado no `RUNBOOK-novo-terminal-server.md`, Fase 5.
- Degrada o desempenho de todo mundo naquele servidor.
- **Exige editar um arquivo de configuração em produção** — e lembrar de desligar depois.

**Recomendação:** usar apenas se os Extended Events não bastarem, fora do horário comercial, com
janela e reversão combinadas.

---

## 3. O que se espera aprender

| Pergunta hoje sem resposta | O que a captura entrega |
|---|---|
| Quais tabelas cada tela realmente lê e grava | O conjunto exato, sem depender do que a documentação diz |
| Onde mora a regra de negócio | Se o SQL é `SELECT`/`UPDATE` direto, a regra está no cliente; se é `EXEC`, está numa procedure — e essa **está legível no banco** |
| Ordem das operações numa transação | Sequência e escopo do `BEGIN TRAN`, para replicar a consistência no CRM novo |
| Colunas de verdade usadas | Separa as colunas vivas das centenas de legado em cada tabela |
| Quais telas custam caro | `duration` e `logical_reads` já apontam os gargalos |
| Como o BPM decide a próxima fase | O SQL do motor de workflow ao avançar um processo |

Isso alimenta diretamente o `docs/projeto/04-MODELO-DADOS.md` e o
`docs/projeto/11-MODULOS-TELAS-PERMISSOES.md` do CRM novo: deixa de ser inferência sobre nome de
tabela e passa a ser observação.

**O atalho já foi medido — e fecha a porta.** A contagem de objetos do banco `CRM`, feita em
02/09/2026 em somente leitura, mostra a proporção:

| Objeto | Quantidade |
|---|---:|
| Tabelas | 767 |
| Views | 411 |
| **Stored procedures** | **85** |
| Funções escalares | 51 |
| Triggers | **1** |

Oitenta e cinco procedures para 767 tabelas, e **um único trigger** em todo o banco. A regra de
negócio **não está no servidor de dados** — está dentro dos executáveis Gupta. É exatamente por
isso que a captura de SQL deixa de ser um atalho e passa a ser o caminho principal: não há um
corpo de código legível no banco que sirva de substituto.

(Ainda assim, vale ler as 85 procedures e as 51 funções — é material gratuito, disponível hoje via
`sys.sql_modules`, e provavelmente concentra o que há de mais crítico em integração e cálculo.)

---

## 4. Riscos e como conter

| Risco | Gravidade | Contenção |
|---|---|---|
| Trace enche o disco do servidor de banco | **Alta** | `max_file_size` + `max_rollover_files` (teto de 2 GB); pasta em volume que não seja o dos dados; conferir espaço antes |
| Overhead de captura em produção | Média | XEvents filtrado por hostname tem custo baixo; janela curta; nunca capturar `statement_completed` sem filtro |
| Sessão de eventos esquecida ligada | Média | `STARTUP_STATE = OFF` + `DROP EVENT SESSION` no roteiro de encerramento + lembrete no calendário |
| **Dados pessoais no `sql_text`** | **Alta (LGPD)** | O SQL capturado contém CPF, nome, telefone de clientes reais. Ver seção 6 |
| Trace de ODBC trava o login do usuário | Média | Não usar em horário comercial; conferir a permissão do arquivo; desligar e validar o login logo depois |
| Roteiro mal anotado inutiliza a captura | Média | Duas pessoas: uma opera, outra cronometra; ou gravar a tela com relógio visível |

---

## 5. Sobre descompilar os assemblies .NET — recomendação

Os 27 executáveis de `C:\Vortice\NT` e as DLLs que os acompanham **são** .NET Framework 4.8 e
**seriam** descompiláveis com ILSpy ou dnSpy. Tecnicamente é trivial. A questão não é técnica.

**Recomendação: não descompilar sem análise jurídica prévia.** A ordem sugerida:

1. **Ler o contrato e a licença primeiro.** Contratos de software proprietário costumam trazer
   cláusula expressa de proibição de engenharia reversa. O `TDBin\TDClickwrap.pdf` (a licença do
   Team Developer, presente na instalação) já sinaliza esse tipo de cláusula para o runtime; a
   licença do **Vórtice** é um documento separado, que precisa ser lido.
2. **Pedir ao fornecedor.** É o caminho mais rápido e sem risco. A Vórtice
   (`vortice@vortice.inf.br`, (16) 2138-1700) pode fornecer, sob NDA, o dicionário de dados, o
   modelo de dados e a documentação das integrações. Vale como pedido formal, por escrito,
   com prazo. **Um "não" documentado também é útil** — ele justifica o esforço de descoberta por
   observação.
3. **Só então, se o contrato permitir e o fornecedor recusar,** avaliar a descompilação — com
   parecer jurídico escrito, escopo restrito ao que for necessário para a interoperabilidade, e
   registro do que foi examinado.

**Sobre a base legal:** a discussão de descompilação é **contratual e de direito autoral**
(Lei 9.610/98 e Lei 9.609/98, a Lei do Software), **não** de LGPD. A LGPD entra em outro ponto — no
tratamento dos **dados pessoais** que aparecem na captura de SQL, tratado abaixo. São dois assuntos
distintos e não devem ser misturados no mesmo pedido de autorização.

> Vale notar: a análise de metadados feita neste inventário (versão, referências de assembly,
> hash) **não é engenharia reversa** — é leitura do manifesto público do arquivo, equivalente a ler
> a etiqueta. Nenhum IL foi lido ou reconstruído.

---

## 6. LGPD — o ponto que costuma passar batido

A captura de SQL grava **dados pessoais de clientes reais** em texto claro: CPF, nome, telefone,
endereço, valores. O arquivo `.xel` é, para todos os efeitos, um banco de dados pessoais.

Antes de capturar:

- **Base legal:** legítimo interesse (art. 7º, IX) para manutenção e evolução de sistema próprio, ou
  execução de contrato. Registrar a escolha por escrito.
- **Minimização:** capturar o **mínimo** de tempo necessário e, se possível, com um cliente de teste
  em vez de carteira real.
- **Retenção:** definir prazo curto (dias, não meses) e **apagar o `.xel` ao terminar a análise**.
- **Guarda:** arquivo em pasta de acesso restrito, nunca em repositório de código, nunca anexado a
  chamado ou e-mail.
- **Registro:** anotar a operação no inventário de tratamento de dados, junto ao DPO/encarregado.
- **Preferência forte por `CRM_HOMO`:** o servidor já tem essa base. Se o roteiro puder ser
  executado nela, o problema de LGPD praticamente desaparece — e essa deve ser a **primeira**
  alternativa avaliada.

---

## 7. Roteiro sugerido (ordem de execução)

| # | Passo | Quem | Pré-requisito |
|---|---|---|---|
| 1 | Pedir documentação técnica ao fornecedor, por escrito | Gestor do contrato | — |
| 2 | Medir quanto da lógica está em stored procedures (`sys.sql_modules`) | Agente (somente leitura) | acesso atual já basta |
| 3 | Avaliar se o roteiro roda em `CRM_HOMO` | DBA + área de negócio | — |
| 4 | Escrever o roteiro de telas com marcação de horário | Analista + usuário-chave | — |
| 5 | Criar a sessão de XEvents filtrada, com teto de arquivo | DBA | `ALTER ANY EVENT SESSION` |
| 6 | Executar o roteiro | Usuário-cobaia + cronometrista | sessão ativa |
| 7 | Parar, dropar a sessão, copiar o `.xel` para pasta restrita | DBA | — |
| 8 | Correlacionar SQL × tela × ação; publicar o mapa | Agente | — |
| 9 | **Apagar o `.xel`** | DBA | análise concluída |

O passo 2 é o de melhor relação custo/benefício e pode ser feito **hoje**, sem autorização nova.
