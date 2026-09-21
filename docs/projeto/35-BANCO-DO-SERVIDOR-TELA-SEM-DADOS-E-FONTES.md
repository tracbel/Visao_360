# O banco do servidor, a tela sem dados e o caminho dos dados até lá

> **Versão 1.6 · 14/09/2026 — o ambiente real da VM:** levantamento só de leitura, feito no próprio servidor.
> O servidor é uma VM OpenStack com Windows Server 2022 Datacenter, **membro do domínio**. A VM não expõe as
> extensões de virtualização, e por isso não roda WSL 2 nem contêiner Linux. Há um Docker Engine do Windows
> em execução, com **0 contêineres**, e um Docker Desktop instalado e parado. API, integração do ART e SQL
> Server 2025 Express são **serviços nativos do Windows**, e nenhum deles usa Docker. A arquitetura foi
> mantida. Os pontos para decisão (backup e log de transação, backups no mesmo disco, Docker ocioso) estão
> em **§12**.
>
> **Versão 1.5 · 14/09/2026 — a integração do ART como serviço no servidor:** o serviço do Windows
> `TracbelCrmSincronizacaoArt` roda no AGRO-SISTEMAS-W e grava no banco central `TracbelCrm` (SQL Server
> nativo; o Docker do servidor não tem contêiner). O primeiro ciclo gravou 2.109 vendas, 2.076 máquinas e 0
> duplicidades. A página exclusiva do ART saiu, e os dados foram para as fichas de máquina e de cliente e
> para Configurações › Integrações. A publicação levou três tentativas, e na segunda a API ficou parada por
> alguns minutos. Tudo em **§11**.
>
> **Versão 1.4 · 14/09/2026 — as vendas do ART no banco local:** classificação de produto modelada
> (`frota.LinhaDeProduto`), máquina, venda e vínculo separados, carga com cópia de segurança, simulação,
> carga real e segunda rodada sem duplicação, fila de 851 compradores ausentes (795 com nota no Protheus,
> 56 sem), 47 divergências registradas, 16 unidades mapeadas e a solicitação de acesso aos dados que
> faltam (documento 37). Tudo em **§10**. Nada foi feito no servidor.
>
> **Versão 1.3 · 14/09/2026 — uso do ART nos cartões:** o ART não entra no faturamento da Visão 360 — a
> venda de máquina registrada nele não é a nota do Protheus (razão mensal de 0,75 a 4,4 nos 12 meses
> fechados) — e não traz meta nem mercado total. Detalhe e conferências no **documento 36**.
>
> **Versão 1.2 · 14/09/2026 — o ART conecta:** autenticação, base e leitura funcionam; o usuário só lê a
> view de vendas de máquinas; perfil e cruzamento com o CRM e o Protheus em §7.4 a §7.8. A carga do
> território rodou no servidor às 12:49 (§5).
>
> **Versão 1.1 · 14/09/2026 — correção:** o ART e o Protheus **têm credencial configurada** no `.env` da
> raiz; o ART está bloqueado pela rede, e o banco do Protheus foi testado e lê (§7 e §8). A recusa da
> carga no servidor foi da revisão automática de aprovação (§5).
>
> **Documento 35** · Versão 1.0 · 14/09/2026 — a causa da tela de Indicadores Geográficos vazia no
> servidor, o ensaio da correção numa cópia idêntica do banco, o que falta rodar, o estado do ART e
> qual fonte sustenta cada informação.
> Convenção: **[medido]** = obtido em 14/09/2026 com consulta somente leitura ao banco do servidor, à
> cópia de ensaio ou à rede. Nenhuma senha foi lida, gravada ou exibida: o banco do servidor foi
> acessado com a conta do Windows de quem estava na estação.

---

## 0. Resumo

1. **A tela ficou vazia porque o território nunca foi carregado no banco do servidor.** Lá há 0
   municípios com código IBGE, 0 linhas de área de atuação, 0 responsáveis e 0 linhas de área
   plantada. O resto está no servidor: 23.945 clientes, faturamento até 09/2026 e 122.002 interações
   [medido]. **Não era falha de conexão nem de permissão**: a API respondia 200, sem erro no console.
2. **O ensaio numa cópia idêntica do banco do servidor corrigiu o problema.** A carga do território
   levou 28 s e trouxe 203 municípios da ADR para os mapas. O total da tela bateu com o SQL de
   conciliação ao centavo, e a segunda rodada não gravou nada (§3).
3. **A tela passou a dizer quando o território não foi carregado.** Antes mostrava "0 municípios" e
   "R$ 0", como se fosse resultado; falta de permissão e sessão expirada também ganharam mensagem
   própria (§4). **Não publicado** — a restrição de publicação continua.
4. **A carga no servidor não foi executada pela sessão de trabalho**: a política de permissões recusou
   a gravação no banco do servidor. O script está pronto e ensaiado:
   `scripts/deploy/carregar-territorio-no-servidor.ps1` (§5).
5. **O ART conecta.** A credencial está no `.env` da raiz do repositório — a versão 1.0 dizia que não
   havia, e **estava errada**. Na primeira tentativa o nome resolvia para 10.235.0.59 e a porta não
   abriu; na segunda resolveu para 10.100.5.134, e rede, autenticação, base e leitura funcionaram. O
   usuário só lê a view `bi_art_veiculos`: 4.144 vendas de máquinas de 2024 a 2026, com chassi,
   comprador, linha e produto — sem propriedade, área, cultura nem horímetro (§7).
6. **Para parar de levar dados desta estação para o servidor**, as cargas passam a gravar direto no
   banco do servidor — já possível hoje, com o script do §5 — e o próximo passo é agendá-las no
   próprio servidor (§6).

---

## 1. Onde está o banco [medido]

| | |
|---|---|
| Máquina | `AGRO-SISTEMAS-W`, 10.150.4.249 (a mesma que roda a aplicação); VM OpenStack Nova com Windows Server 2022 Datacenter, membro do domínio `tracbel.com.br` (§12) |
| Motor | SQL Server 2025 **Express** (17.0.1000.7), instância padrão `MSSQLSERVER`, porta 1433 |
| Banco | `TracbelCrm`, colação `Latin1_General_CI_AI` |
| Arquivos | `C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER\MSSQL\DATA\TracbelCrm.mdf` (520 MB) e `TracbelCrm_log.ldf` (1.032 MB) |
| Modo de recuperação | `FULL` |
| Como nasceu | restaurado em **09/09/2026 13:23** a partir do backup do banco **desta estação**, feito em 08/09/2026 17:33 (`instalar-no-servidor.ps1`) |
| Conta da aplicação | `TracbelCrm` (leitura, escrita e DDL). A senha está só no `appsettings.Production.json` do servidor, restrito a administradores |
| Acesso administrativo | pelo Windows, para quem é administrador do servidor |
| Cópias de segurança | **nenhuma rotina.** Além do backup de origem, só existe a cópia de 14/09/2026 08:42 (`...\MSSQL\Backup\TracbelCrm-copia-antes-do-territorio-20260914.bak`, 507 MB, conferida com `RESTORE VERIFYONLY`) |
| Agendador do banco | não existe: a edição Express não tem SQL Agent |

**Risco que isto mostra:** o banco está em recuperação `FULL` sem backup de log — o arquivo de log só
cresce (já tem 1 GB) — e não há backup rotineiro do banco que passou a ser o oficial (§6.3).

---

## 2. Por que a tela ficou vazia

### 2.1 Como os dados chegaram ao servidor

| Data | O que aconteceu | Onde |
|---|---|---|
| 08/09/2026 | backup do banco de desenvolvimento | esta estação |
| 09/09/2026 | restauração desse backup no servidor | servidor |
| 13/09/2026 | carga do território (IBGE, ADR, responsáveis, área plantada) e correção das grafias | **só nesta estação** |
| 13/09/2026 | publicação da aplicação: a migração criou as quatro tabelas do território, **vazias** | servidor |

O `publicar.ps1` leva aplicação e migração, **não leva dado**. O território nunca foi para o servidor.

### 2.2 O estado do banco do servidor em 14/09/2026 [medido]

| Medida | Servidor |
|---|---:|
| Última migração | `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial` |
| Clientes | 23.945 |
| Faturamento (linhas · última competência) | 37.867 · 09/2026 |
| Interações (última) | 122.002 · 06/09/2026 |
| Municípios com código IBGE | **0** |
| Área de atuação (ADR) | **0** |
| Responsáveis por município | **0** |
| Área plantada | **0** |
| Usuários pessoa ativos / que já entraram pelo Entra ID | 263 / 4 |
| Concessões de permissão | 0 |

### 2.3 O fluxo rastreado, camada por camada

| Camada | O que acontece com o território vazio | Evidência |
|---|---|---|
| Banco | nenhum município tem código IBGE nem está marcado como ADR | §2.2 |
| Consulta | o endereço do cliente aponta para município sem código IBGE, e a venda cai no grupo `MunicipioSemCodigoIbge`, fora do mapa | resposta da API: R$ 83,2 mi nesse grupo, na filial 010101 |
| API | responde **200** com 0 municípios, 6 grupos fora do mapa e a lacuna `areaDeAtuacao` | chamada direta à API sobre a cópia do servidor |
| Front | os mapas desenham os 645 municípios de SP sem nenhum na ADR; indicadores e total mostram 0 e R$ 0; a explicação só aparece no rodapé | captura `servidor-antes-da-carga-1-geral.png` |

**O que foi descartado com evidência:** conexão entre front e API (200, sem erro no console);
permissão (o usuário consulta a própria filial; o 403 existe só para a visão da empresa); período (os
12 meses fechados, set/2025–ago/2026, têm faturamento); filial (a 010101 tem R$ 217,1 mi no período).

---

## 3. O ensaio numa cópia idêntica do banco do servidor [medido]

**Como.** Cópia de segurança `COPY_ONLY` no servidor → trazida para `dados-locais/bancos/` (fora do
Git) → restaurada num SQL Server **2025** local (o contêiner de desenvolvimento é 2022 e não restaura
backup de versão mais nova) → aplicação local apontada para a cópia → captura → carga do território →
captura → segunda carga.

| Etapa | Resultado |
|---|---|
| Cópia restaurada | 23.945 clientes, 0 ADR, 0 municípios com código IBGE, mesma migração do servidor |
| Tela antes | API 200 com 0 municípios; 645 polígonos, 0 na ADR; total "0 municípios · R$ 0"; nenhum erro |
| Carga do território (28 s) | 5.571 municípios do IBGE lidos; **5.458 reconhecidos** e **113 criados**; **192 endereços** corrigidos das grafias cortadas e **2 pendentes** com o motivo; **203 municípios da ADR** e 35 fora; **203 + 406** afirmações de responsável; **45.582** linhas de área plantada (PAM 2024) |
| Tela depois | 203 municípios da ADR em cada um dos três mapas; pendência de visita 34,2%; vendas na ADR R$ 77,1 mi; 9.932 tratores teóricos; 82 municípios com CEN diferente; detalhe de Ribeirão Preto abre; nenhum erro |
| Conferência independente | total da filial 010101 na API **R$ 217.106.275,76** = SQL de conciliação (F1 + F2 + F4) **R$ 217.106.275,76**; Ribeirão Preto 446 clientes, vendas R$ 7.714.547,18, pós-venda R$ 2.892.701,03, 420 elegíveis e 317 pendentes — os mesmos números validados no banco de desenvolvimento (documento 32, §11.3) |
| Segunda carga | **grava zero**: 0 municípios, 0 endereços, 203 e 406 afirmações mantidas, 45.582 linhas mantidas |

As capturas estão em `dados-locais/capturas/` (fora do Git, têm número real):
`servidor-antes-da-carga-*`, `servidor-depois-da-carga-*` e `servidor-sem-carga-aviso-novo-*`.

---

## 4. O que mudou na tela

| Situação | Antes | Agora |
|---|---|---|
| Território não carregado (consulta sem filtro, nenhum município da ADR) | "0 municípios", "R$ 0", mapas cinzas | aviso **"O território ainda não foi carregado neste banco"**, dizendo que não é falta de permissão, não é falha e não é zero, com o valor que ficou fora do mapa; indicadores com "—" e o motivo; mapas não desenhados; linha de total explicando |
| Filtro de região ou loja sem município | igual ao acima | continua mostrando o resultado do filtro — é consulta vazia, não carga ausente |
| Sem permissão (403) | título com a frase da API | **"Sem permissão para esta consulta"**, com a permissão que falta |
| Sessão expirada (401) | título genérico | **"A sessão expirou"** |
| A API não responde | "A API do CRM não respondeu" | sem mudança |

**Verificação:** teste de API `Sem_municipio_da_adr_na_consulta_a_resposta_declara_a_lacuna_em_vez_de_mostrar_zero`
(72 testes da API passando); `tsc -b` e `oxlint` sem erro; captura com a cópia vazia
(`servidor-sem-carga-aviso-novo-1-geral.png`). O front não tem suíte de teste de componente
configurada (o Vitest está instalado, sem configuração) — a prova da tela é a captura.

**Não publicado.** No servidor, os mapas aparecem com a carga do §5, sem publicar; o aviso novo só
aparece depois da próxima publicação.

---

## 5. Levar o território ao servidor — o que você roda

**Na estação de desenvolvimento (JQ3G294), não no servidor**, numa janela do **PowerShell** (não no
Prompt de Comando), com a VPN, na conta que entra no SQL Server do servidor como administrador. O
servidor não tem o código-fonte da carga, o SDK do .NET 9 nem as planilhas; é a carga, rodando na
estação, que grava no banco do servidor pela rede. Rodado no servidor ou fora do repositório, o script
para logo no começo e diz isso.

```powershell
Set-Location C:\projetos\tracbel-crm
powershell -ExecutionPolicy Bypass -File .\scripts\deploy\carregar-territorio-no-servidor.ps1
```

O script:

1. confere que o banco do servidor responde e que a conta é administradora;
2. faz uma **cópia de segurança conferida** no próprio servidor;
3. mostra o estado antes;
4. roda a mesma carga do ensaio, gravando **direto no banco do servidor** (lê as duas planilhas da raiz
   do repositório e o IBGE; não usa o banco desta estação nem senha em arquivo);
5. mostra o estado depois e **para com erro se não chegar aos 203 municípios da ADR**.

**Resultado esperado:** os números da linha "Carga do território" do §3. A tela de Indicadores
Geográficos passa a mostrar os mapas **sem precisar republicar**. Reexecutar não grava nada. Para voltar
atrás, restaure a cópia do passo 2.

### 5.1 As fontes públicas já não precisam de você (20/09/2026, #64 e #65)

A quarta etapa — a PAM — saiu deste roteiro, e a estrutura agropecuária nasceu já fora dele. Nenhuma
das duas usa planilha, as duas mudam uma vez por ano e as duas rodam **no próprio servidor**, na
tarefa anual `TracbelCrmFontesPublicas`, que o
`scripts/deploy/agendar-fontes-publicas-no-servidor.ps1` instala uma vez. Detalhe no documento 32,
§8.3.3.

**Ficou medido que o servidor alcança a internet:** a primeira rodada leu 163.965 linhas do SIDRA de
dentro dele, sem proxy nem configuração especial. Isso vale para as próximas fontes externas do
documento 48 (#66, #67, #68).

**21/09/2026 (#66):** os preços das culturas entraram com rotina própria, **mensal** —
`TracbelCrmPrecos`, todo dia 20, instalada pelo mesmo script. CONAB, Socicana e dólar PTAX, todos
abertos.

O script acima continua sendo o caminho das **três primeiras** etapas, que dependem das duas
planilhas do comercial — e continuam saindo da estação enquanto as planilhas chegarem por e-mail.

**O caminho está certo.** O repositório nesta estação é `C:\projetos\tracbel-crm` (o projeto não usa as
unidades E: nem F:). O script encontra a raiz do repositório pela pasta onde ele mesmo está.

**Por que não foi rodado pela sessão.** A ação recusada foi a execução de
`dotnet run --project src\Tracbel.Crm.Carga -- --somente-territorio ...` com a conexão apontada para o
banco `TracbelCrm` em 10.150.4.249 (autenticação do Windows). **Foi uma rejeição da revisão automática de
aprovação** — o classificador do modo automático do Claude Code —, não uma recusa sua nem do SQL Server,
com o motivo **"Modify Shared Resources"** (modificação de recurso compartilhado). As consultas somente
leitura e a cópia de segurança no mesmo servidor não foram recusadas. Repetir a autorização não muda
essa revisão, e ela não foi contornada: a execução é sua. A mesma revisão recusou antes a publicação pelo
`publicar.ps1` ("Production Deploy") e a leitura de tokens das contas do GitHub ("Credential
Exploration").

---

## 6. Parar de levar dados desta estação para o servidor

### 6.1 O princípio

- **O banco do servidor é o oficial.** Nenhum backup desta estação volta a ser restaurado por cima dele
  (o `-RestaurarBanco` do instalador fica só para recuperação de desastre).
- **Toda carga grava direto nele**, lendo as fontes somente leitura — Vórtice, Protheus, IBGE e
  planilhas —, com a mesma idempotência e a mesma trilha de auditoria de hoje.

### 6.2 Em duas etapas

| Etapa | Como | O que precisa |
|---|---|---|
| **Agora** | as cargas rodam desta estação, apontadas para o banco do servidor pela conta do Windows — o script do §5 já faz isso para o território; o faturamento (`--somente-faturamento`) e a recarga do Vórtice seguem o mesmo caminho | a VPN; as variáveis de acesso do Protheus e do Vórtice na sessão de quem roda (nunca em arquivo) |
| **Próxima** | a carga instalada **no próprio servidor**, publicada junto com a aplicação, com as credenciais na configuração restrita do servidor, e agendada no **Agendador de Tarefas do Windows** do servidor: faturamento diário, Vórtice semanal, território quando a planilha mudar | rota do servidor até o Vórtice (10.150.14.65:1433), a API do Protheus e a internet (IBGE); uma pasta no servidor para as planilhas; alguém com administrador **no servidor** para criar as tarefas (a criação remota por `schtasks` está vetada) |

### 6.3 A rotina de cópia de segurança

Hoje não há. A proposta é uma tarefa noturna no Agendador do servidor com `BACKUP DATABASE` completo,
guardado fora do servidor — **não** no compartilhamento `bkpbd` de 10.150.6.230 —, e trocar o modo de
recuperação para `SIMPLE` (ou passar a fazer backup de log), para o log parar de crescer. Destino e
retenção são decisão sua.

---

## 7. O ART [medido]

### 7.1 A configuração existente (corrigido na versão 1.1)

| | |
|---|---|
| Onde | `.env` na raiz do repositório `C:\projetos\tracbel-crm` — ignorado pelo Git, como deve |
| Chaves do ART | `ART_DB_TIPO` (mysql), `ART_DB_SERVER` (`aftracbel.tracbel.com.br`), `ART_DB_PORT` (3306), `ART_DB_DATABASE` (`aftba`), `ART_VIEW` (`bi_art_veiculos`), `ART_DB_USER` e `ART_DB_PASSWORD` preenchidos |
| Quem lê este `.env` | **nenhum código do projeto**. A API e a carga leem `appsettings.json` e variáveis de ambiente com outros nomes (`ConnectionStrings__Crm`, `Vortice__Conexao`, `Protheus__*`); não há leitor de `.env` nem de `ART_*` |
| Por que a versão 1.0 não achou | a busca usada ignora os arquivos do `.gitignore`, e o `.env` está nele; as variáveis de ambiente e as credenciais do Windows foram olhadas, o `.env` não |

### 7.2 O teste com essa configuração [medido]

Feito da estação **JQ3G294** (endereço 172.16.17.133, interface Ethernet 4), com os valores do `.env`
carregados só dentro do processo:

| Etapa | Resultado |
|---|---|
| Resolução do nome | `aftracbel.tracbel.com.br` → **10.235.0.59** |
| Rede (TCP 3306) | **falha** — a conexão não abre; o ping também não responde |
| Rota | 172.16.17.251 → 172.17.146.1 → **143.208.144.33** → sem resposta: o tráfego segue a rota padrão para fora, e nenhuma rota leva à rede 10.235 |
| Tabela de rotas e VPN | só a rota padrão (0.0.0.0/0 via 172.16.17.251, Ethernet 4) cobre 10.235.0.59; o adaptador da **VPN Fortinet** existe e estava **desconectado** ("Not Present"). O documento 30 registrou que, com a VPN ligada, a faixa era roteada e o host não respondia |
| Autenticação | **não testável** sem conexão TCP |
| Acesso à base `aftba` | **não testável** |
| Permissão de leitura | **não testável** |

**Credencial existente e ausência de rota são problemas diferentes, e o que falta é a rota.** O teste de
autenticação está pronto para rodar assim que a porta abrir: cliente `mysql` 8.0 em contêiner, com a
senha passada por variável de ambiente, conferindo `CURRENT_USER()`, a base em uso, `SHOW GRANTS` e a
contagem de objetos da base.

**O caminho pelo Fluig** — o dataset `dts_art` repassa SQL ao `dts_sql` na fonte `bd_art` — não é
impedimento nem alternativa necessária: a conexão direta está configurada.

### 7.3 O que se sabe do esquema

Só a view **`bi_art_veiculos`** da base `aftba`, pelo SQL do dataset: `codigo`, `chassis`, `data_vda`,
`vendedor`, `id_unidade`, `unidade`, `unidade_fat`, `cpf_cnpj`, `cliente`, `linha`, `produto`,
`num_ped`, `data_fat`, `num_nfe_venda`, `entrega`. As tabelas de propriedade, cultura, área e horímetro
**não foram vistas** — não há como inspecioná-las sem acesso.

### 7.4 A segunda tentativa: conecta [medido]

Feita da estação **HZYLRK4** (172.16.17.133, Ethernet 4), com a mesma configuração do `.env`, pelo
cliente `pymysql`, com a sessão em modo somente leitura:

| Etapa | Resultado |
|---|---|
| Resolução do nome | `aftracbel.tracbel.com.br` → **10.100.5.134** (na primeira tentativa, 10.235.0.59) |
| Rede (TCP 3306) | **abre** |
| Autenticação | **OK** — MySQL 8.0.45 |
| Base | `aftba` em uso; `transaction_read_only = 1` |
| Permissões | `USAGE` global e `SELECT` **só** na view `bi_art_veiculos`, em `aftba` e em `tracbel-app` |
| Leitura | 4.144 linhas |
| Esquema visível ao usuário | 0 tabelas e 1 view |

### 7.5 A view `bi_art_veiculos` [medido, só agregados]

**72 colunas**, em seis grupos:

| Grupo | Colunas | O que o dado mostra |
|---|---|---|
| Venda | `codigo`, `num_ped`, `situacao`, `num_nfe_venda`, `venda_direta`, `repasse_direto` | situação P 4.090 · F 37 · PU 11 · PF 6 (códigos sem significado documentado); venda direta em 848 |
| Máquina | `chassis`, `linha`, `produto`, `possui_config`, `label_config`, `dt_lib_chassi` | 4.136 chassis distintos, 3.352 com 17 posições, 3 sem chassi, 5 repetidos; 14 linhas; 126 produtos |
| Comprador | `cpf_cnpj`, `cliente` | 182 CPF e 3.962 CNPJ; 1.778 compradores distintos |
| Canal | `empresa`, `unidade`, `unidade_fat`, `vendedor`, `gestao`, `usuario` | Agro Norte 2.585 · Agro Noroeste 1.559; 16 unidades; `gestao` Varejo 3.235 · Grandes Contas 909 |
| Datas | `data_vda`, `data_fat`, `entrega`, `data_com`, `data_cpra`, `dt_abertura` | vendas de 28/01/2024 a 14/09/2026 (2024: 1.282 · 2025: 1.802 · 2026: 1.060); faturamento em 4.112; `entrega` e `data_com` com data zerada; `data_cpra` com ano inválido |
| Usado na troca e financeiro | `chassis_usado`, `placa_usado`, `vr_usado`, valores de venda, compra, impostos, comissões, lucro e margem | máquina usada na troca em 257 vendas; **o financeiro não é necessário ao projeto e não será trazido** |

**Linhas:** TRATOR PEQUENO 1.381 · TRATOR GRANDE 771 · TRATOR MÉDIO 391 · IMPLEMENTOS OM 391 · COLHEDORA
CANA CH 570 370 · IMPLEMENTOS JD 346 · USADOS 302 · PLANTADEIRA 57 · PULVERIZADOR 53 · COLHEDORA CANA CH
950 36 · PLATAFORMA CORTE 18 · COLHEITADEIRA 17 · COLHEDORA CANA 9 · COLHEDORA CANA CH 750 2.

**O que a view não tem:** propriedade, área, cultura, horímetro e ano de fabricação.

### 7.6 O cruzamento com o CRM e o Protheus [medido]

O documento do comprador foi comparado só em hash SHA-256; o arquivo temporário foi apagado ao fim.

| Comparação | Resultado |
|---|---|
| Chassis do ART no CRM (`frota.Equipamento`, banco do servidor) | **42 de 4.136** — o parque do CRM veio do Vórtice; o ART traz as vendas de 2024 a 2026 |
| Nessas 42: comprador do ART × cliente do CRM | igual 4 · diferente 35 · cliente do CRM sem documento 3 |
| Nessas 42: data de venda e ano de fabricação no CRM | vazios nas 42 |
| Chassis do ART no Protheus (`VV1010`) | 3.938 de 4.136 |
| Nesses: dono no Protheus × comprador do ART | igual 1.721 · dono vazio no Protheus 1.835 · diferente 382 |
| Máquinas do CRM no Protheus | 3.736 de 3.888 |
| Compradores do ART que são clientes do CRM | 927 de 1.778 |
| …sem cliente no CRM, mas com nota no Protheus | 795 |
| …em nenhum dos dois | 56 |
| Compradores do ART com cadastro na `SA1010` | 1.741 de 1.778 |
| Produtos do ART com modelo equivalente no catálogo do CRM | 38 de 126 |
| Unidades do ART → filiais do CRM | 16 de 16; Ituverava, Guaíra e Monte Alto, inativas no CRM, têm 15, 9 e 2 vendas |

### 7.7 O que o ART sustenta, e o mapeamento proposto

| Requisito | O que o ART traz | Destino proposto | Regra |
|---|---|---|---|
| Parque vendido (O-02) | chassi, produto, linha, comprador, datas de venda, faturamento e entrega | `frota.Equipamento`, com a chave de origem do ART | cria só máquina com chassi de 17 posições e comprador com cliente no CRM; o resto vai para a fila de revisão com o motivo; dono divergente não é sobrescrito — fica sinalizado |
| Tipo de produto e modelo (O-20) | `linha` (14) e `produto` (126) | catálogo de linha de produto e `frota.Modelo` | a linha não tem lugar no modelo de dados hoje — decisão de modelagem antes da carga |
| Data de venda e entrega técnica | `data_vda`, `data_fat`, `entrega` | `Equipamento.VendidoEm` e a entrega técnica | data zerada ou inválida fica vazia, registrada |
| SAM, KAM e Varejo (O-19) | `gestao` Varejo / Grandes Contas, **por venda** | nenhum por enquanto | é atributo da venda, não do cliente; insumo para a decisão 9 do documento 34 |
| Filial | `unidade` e `unidade_fat` | `organizacao.Empresa` pelo nome | 16 de 16 casam |
| Financeiro | margem, custo, comissão | **não trazer** | fora do escopo e sensível |
| Propriedade, área, cultura, horímetro, ano (O-22, O-23, O-25, O-26) | ausentes da view | — | continuam bloqueados |

### 7.8 O que falta

1. **Propriedade, área, cultura e horímetro:** o dono do ART dizer em que tabelas ou visões esses dados
   estão e conceder leitura ao usuário — hoje ele só enxerga a view de vendas. **O pedido está pronto
   para envio no documento 37** (não enviado).
2. **Sincronização agendada:** confirmar a rota do servidor 10.150.4.249 até 10.100.5.134:3306 (não
   testada).
3. ~~**Linha de produto:** decidir onde ela entra no modelo de dados antes da carga do parque (O-20).~~
   **Resolvido na versão 1.4:** `frota.LinhaDeProduto`, com o de-para explícito do ART (§10.1).

**Continua bloqueado sem as tabelas de propriedade e parque:** cadastro de cliente conforme a planilha
de exemplo (O-01), horímetro e ano do parque (O-02), potencial de cliente e de não cliente (O-22, O-23),
cultura e área por propriedade (O-25) e ciclo de troca (O-26).

---

## 8. Qual fonte sustenta cada informação

Fontes somente leitura; toda correção acontece no banco do projeto, com o valor original preservado
(`integracao.ChaveExterna`, `integracao.MensagemDescartada` e `auditoria.AlteracaoDeCampo`).

| Informação | Fonte que sustenta | Base de conferência | Situação medida | Tratamento |
|---|---|---|---|---|
| Cliente: cadastro, carteira, contato | Vórtice (`GE_Pessoa`) | Protheus (`SA1010`) | 23.945 clientes; 8.422 sem documento (35%), dos quais só 698 casam por nome com a SA1; 387 CNPJs que faturam existem no CRM só por nome (R$ 110,8 mi); 326 raízes de CNPJ em 726 cadastros; 4 documentos repetidos (documento 31) | sinalizados, sem fusão. A credencial do banco do Protheus **existe** no `.env` da raiz (`TOTVS_DB_*`) e foi testada em 14/09/2026: autentica, abre a base `TMPRD` e lê a `SA1010` (38.701 clientes ativos). O usuário é **dono do banco** (`db_owner`); a integração só lê. Completar o documento pela SA1 é a próxima correção, ainda não implementada |
| CPF/CNPJ | Vórtice, reconstituído com dígito verificador | Protheus `A1_CGC` | 10.062 reconstituídos; 190 recusados (documento 24) | trilha por campo |
| Endereço e município | Vórtice + catálogo oficial do IBGE | IBGE (nome, UF, contorno) | 5.458 municípios reconhecidos; 192 endereços corrigidos; 2 com coordenada conflitante sinalizados; 4.282 linhas sem município do IBGE (distrito, "A CADASTRAR") | só correspondência inequívoca; idempotente (§3) |
| Faturamento | Protheus (SD2, notas de saída) | SQL de conciliação | R$ 897,5 mi em 12 meses, conferido ao centavo (documento 32, §8.6) | devolução não abatida (P-5) |
| Interação, cobertura de carteira | Vórtice | — | 122.002 interações até 06/09/2026 | regra de visita provisória (P-2) |
| ADR, loja, região | planilha Área de Atuação | planilha CEN e Gestor + IBGE | 203 de 203 | vigência a confirmar (P-1) |
| CEN e gestor | as duas planilhas | cadastro de usuários | 82 municípios com CEN diferente | as duas fontes preservadas |
| Área plantada | IBGE (PAM 2024) | — | 45.582 linhas | estimativa regional |
| Parque de máquinas | Vórtice (3.888 máquinas) | Protheus `VV1010` (38.369 chassis; 9% com dono; ano e horímetro vazios) | nenhuma das duas tem ano, horímetro ou entrega técnica | o **ART** traz as vendas de 2024 a 2026 com chassi, linha, produto, comprador e datas de venda, faturamento e entrega (4.136 chassis; só 42 já estão no CRM), sem ano e horímetro (§7.5 a §7.7) |
| Propriedade, cultura e área por cliente | ART, se existir lá | — | 0 de 19.657 endereços com área | não está na view liberada ao usuário do ART; depende do dono do ART (§7.8) |
| Metas | nenhuma fonte | — | 0 linhas | planilha 04 do documento 34 |
| SAM, KAM e Varejo | nenhuma fonte | — | não existe classificação por cliente | planilha 03 do documento 34 |

---

## 9. O que ficou montado nesta estação

- Contêiner `tracbel-crm-ensaio` (SQL Server 2025, porta 14334) com `TracbelCrmServidor` (a cópia já
  carregada) e `TracbelCrmServidorVazio` (a cópia como está no servidor).
- A aplicação local em `http://localhost:5199`, apontada para a cópia carregada.
- A cópia do backup em `dados-locais/bancos/` (fora do Git). Apagar depois do prazo de conferência.

---

## 10. As vendas do ART no banco local [medido]

Executado em 14/09/2026 no banco **local** de desenvolvimento (`TracbelCrm`, contêiner `tracbel-crm-db`,
porta 1433). O ART foi lido em sessão `READ ONLY`; o Protheus, só com `SELECT` e intenção de leitura.
**Nada foi feito no servidor.** Nenhum nome, documento ou valor aparece aqui: só contagens, códigos de
produto, linha e unidade — que são catálogo comercial.

### 10.1 A modelagem: por que três tabelas de frota e quatro de integração

| Tabela | O que guarda | Por que existe |
|---|---|---|
| `frota.LinhaDeProduto` | a classificação comercial — trator pequeno, médio, grande, colhedora de cana… — com porte e, quando existe, a família compatível | a família do catálogo é "Trator"; o comercial filtra por porte. É uma classificação **ao lado** da família, e não uma família nova: 10 classificações, 7 apontando para a família John Deere ou "Outras" já existente |
| `frota.VendaDeMaquina` | o evento de venda: filial, datas, pedido, nota, gestão, linha e produto **como o ART escreve** | uma máquina tem quantas vendas tiver — nova e revenda como usada. Sem valor, custo, comissão nem margem |
| `frota.VinculoDeClienteComEquipamento` | a ligação cliente × máquina com **natureza** (`CompradorNaVenda`), origem e data | o comprador de 2024 não prova quem tem a máquina hoje; `Equipamento.ClienteId` continua sendo o dono atual, e só uma pessoa o confirma |
| `integracao.CorrespondenciaDaOrigem` | o de-para explícito de linha, produto e unidade, com o critério escrito e a situação | o valor da origem fica preservado; a correspondência revisada por uma pessoa não é tocada pela recarga |
| `integracao.RegistroDeOrigem` | a trilha de cada linha lida: chave, resumo do conteúdo, transformações, decisão e motivos | é o que torna a recarga repetível e verificável |
| `integracao.CompradorPendente` | a fila dos compradores sem cliente no CRM, com o que falta para cadastrar | nenhum cliente é criado automaticamente |
| `integracao.DivergenciaDeIntegracao` | a divergência entre ART, CRM e Protheus | nada é resolvido por sobrescrita |

`frota.Equipamento` ganhou `LinhaDeProdutoId`, a origem `Art` e a situação `ProprietarioNaoConfirmado`;
`ModeloId` passou a aceitar vazio **só** na máquina de integração sem correspondência segura
(`CK_Equipamento_ModeloPendente`). Migração `IntegracaoDoArtLinhaDeProdutoVendaEVinculo`; o total do
modelo passou a 79 tabelas (documento 14, §2.1). O catálogo `LINHA_DE_PRODUTO` nasce do seed versionado.

### 10.2 As regras de entrada

| Regra | Como |
|---|---|
| Chassi | só entra o de 17 caracteres no padrão VIN; espaço é removido, **nenhum outro símbolo é descartado** e nenhum pedaço é escolhido. Vazio, incompleto, fora do padrão e múltiplo ficam pendentes com o motivo |
| Comprador | CPF/CNPJ com dígito verificador válido e **um único cliente** no CRM; havendo mais de um, vale o da filial da venda, se for um só |
| Filial | a unidade do ART corresponde à filial pelo nome **idêntico**, sem acento e caixa |
| Produto → modelo | só por código **idêntico** sem espaço e pontuação, com candidato único; "TR 6135M" não casa com "6135M" |
| Linha → classificação | tabela explícita das 14 linhas; "USADOS" é condição da venda e não classifica a máquina |
| Datas | zerada ou inválida fica vazia, e a transformação fica escrita no registro e na venda |
| Dono existente | não é sobrescrito; se o comprador da venda mais recente for outro, vira divergência |
| Gestão | `Varejo` e `Grandes Contas` ficam na venda, como vieram — não viram SAM/KAM nem classificação do cliente |
| Financeiro | nenhuma coluna de valor, custo, imposto, comissão ou margem é lida |

### 10.3 As repetições: 4.144 linhas e 4.136 chassis

| Medida | Resultado |
|---|---|
| Chassis em mais de uma venda | 5 (10 linhas) |
| …com uma das vendas na linha USADOS (revenda) | 5 |
| …com o mesmo comprador nas duas vendas | 0 |
| …com o mesmo código de venda (duplicata real) | 0 |

**Nenhuma é duplicata.** São cinco máquinas vendidas e depois revendidas como usadas, para outro
comprador. As duas vendas entram; a máquina é uma. Duas delas têm as duas vendas importáveis.

### 10.4 Cópia de segurança e simulação

- Cópia `COPY_ONLY` com `CHECKSUM` do `TracbelCrm` local às 14:05 (502 MB), conferida com
  `RESTORE VERIFYONLY`. Fica no volume do contêiner, fora do Git.
- A simulação roda a carga inteira numa transação **desfeita** no fim
  (`scripts/integracao/rodar-carga-do-art.ps1 -Simular`). A primeira tentativa parou por tamanho de coluna
  (`Situacao` com 20 caracteres, `ProprietarioNaoConfirmado` tem 25) e **nada foi gravado**; a migração,
  ainda não commitada, foi desfeita no banco local — as tabelas novas estavam vazias —, corrigida para 30
  e aplicada de novo. A segunda simulação terminou e os números dela são os da carga real (§10.5).

### 10.5 A carga real e a repetição

| Etapa | 1ª rodada | 2ª rodada |
|---|---|---|
| Registros lidos do ART | 4.144 novos | 4.144 já conhecidos, sem alteração |
| Importáveis / pendentes | 2.108 / 2.036 | 2.108 / 2.036 |
| Máquinas incluídas (sem dono atual) | **2.075** (1.086 com modelo, 1.991 com classificação) | 0 |
| Máquinas do CRM reaproveitadas (dono preservado) | 31 (7 ganharam classificação) | 31 (nada alterado) |
| Vendas | **2.108** incluídas | 0 incluídas · 2.108 sem alteração |
| Vínculos "comprador na venda" | **2.108** incluídos | 2.108 mantidos |
| Correspondências | 156 registradas | 0 novas · 0 reavaliadas |
| Divergências | 47 novas | 47 encontradas de novo, nenhuma nova |
| Fila de compradores | 851 incluídos | 851 reapurados, nenhum novo |

Pendências por motivo (um registro pode ter mais de um): comprador ausente do CRM 1.536 · chassi
incompleto 741 · fora do padrão 153 · múltiplo 5 · vazio 3. Documento inválido e unidade sem filial: 0.

**Conferência por SQL independente, depois da segunda rodada:** 0 chassi ativo repetido, 0 venda
repetida, 0 vínculo ativo repetido, 0 registro repetido, 0 máquina do ART com dono, 0 venda sem vínculo;
2.075 máquinas `Art`, 2.108 vendas, 2.108 vínculos ativos, 4.144 registros com 2 leituras cada.

**As regras da recarga:** o conteúdo igual não regrava (resumo SHA-256 do que foi lido); o conteúdo
diferente atualiza a venda e grava cada campo em `auditoria.AlteracaoDeCampo`; chassi ou comprador
trocado na origem encerra o vínculo antigo com o motivo, cria o novo e abre divergência; registro que
some da origem é marcado ausente e, se tinha venda, abre divergência; modelo, classificação e dono
existentes nunca são trocados; correspondência revisada e divergência resolvida são preservadas.

#### 10.5.1 A classificação que contradizia o modelo — achada na validação e corrigida

A validação na tela mostrou uma máquina que **já existia no CRM como trator John Deere** com a
classificação "implemento de outras marcas": a linha da venda no ART dizia "IMPLEMENTOS OM", e a carga
preenchia a classificação vazia sem conferir o modelo. Medido: **1** caso entre as 6 máquinas antigas
classificadas, e **0** entre as 1.086 máquinas do ART com modelo e classificação.

- **Regra nova:** a classificação só entra quando a família dela e a família do modelo da máquina não se
  contradizem (`ClassificacaoDoArt.ClassificacaoCompativelComOModelo`, com teste). Vale para a máquina
  nova e para a existente; a recusa é contada no relatório.
- **Correção no banco local:** a classificação daquela máquina foi retirada, com a linha em
  `auditoria.AlteracaoDeCampo` (antes `4`, depois vazio) — é a "correção de uma pessoa" que a recarga
  precisa preservar.

**3ª rodada, depois da correção:** 0 máquinas, 0 vendas, 0 vínculos, 0 divergências e 0 compradores
novos; **1 classificação recusada** (a corrigida não voltou); 4.144 registros com 3 leituras cada. A
rodada também encontrou **1 registro alterado no ART entre as leituras** — uma venda com data de
14/09/2026 editada durante a sessão, pendente por chassi incompleto e comprador ausente: a mudança ficou
datada no registro (`ConteudoAlteradoEm`) e nenhuma venda importada mudou.

### 10.6 As 16 unidades → filiais do CRM

Todas por nome idêntico. Vendas = linhas em que a unidade é a vendedora.

| Unidade | Vendas | Filial | Situação no CRM |
|---|---|---|---|
| Ribeirão Preto | 1.150 | 010101 | ativa |
| Franca | 421 | 010111 | ativa |
| Bebedouro | 350 | 010109 | ativa |
| São José do Rio Preto | 324 | 010113 | ativa |
| Jales | 307 | 010115 | ativa |
| Barretos | 296 | 010103 | ativa |
| Catanduva | 296 | 010114 | ativa |
| Votuporanga | 258 | 010116 | ativa |
| Tupã | 220 | 010117 | ativa |
| Orlândia | 168 | 010107 | ativa |
| Marília | 154 | 010118 | ativa |
| Araraquara | 124 | 010102 | ativa |
| Itápolis | 50 | 010112 | ativa (no ART, sem código de unidade) |
| Ituverava | 15 | 010105 | **inativa** |
| Guaíra | 9 | 010104 | **inativa** |
| Monte Alto | 2 | 010110 | **inativa** |

As vendas importadas nas três filiais inativas somam 15 (010104: 6, 010105: 7, 010110: 2). Elas não
aparecem na tela de nenhuma filial em operação, porque as inativas não estão no seletor (P-34).

### 10.7 Classificação e produto

| Correspondência | Situação | Quantidade |
|---|---|---|
| Linha → classificação | exata | 13 |
| Linha → classificação | não é classificação ("USADOS") | 1 |
| Produto → modelo | exata (código idêntico, candidato único) | 32 |
| Produto → modelo | pendente de revisão | 91 (inclui "CH 570", com dois modelos de mesmo código no catálogo) |
| Produto → modelo | descrição genérica ("Implementos", "Usado Outras Marcas") | 3 |

Máquinas com venda do ART por classificação: trator pequeno 823 · trator grande 527 · colhedora de cana
353 · trator médio 227 · pulverizador 43 · implemento John Deere 13 · implemento de outras marcas 10 ·
sem classificação 110 (as da linha USADOS, as que já existiam no CRM e a corrigida em §10.5.1).

### 10.8 A fila dos compradores ausentes do CRM

| Grupo | Cadastro na SA1 do Protheus | Compradores |
|---|---|---|
| Com nota de saída no Protheus (sem cliente no CRM) | ativo | 791 |
| Com nota de saída no Protheus | bloqueado | 4 |
| Sem nota carregada | ativo | 30 |
| Sem nota carregada | ausente | 24 |
| Sem nota carregada | bloqueado | 2 |
| **Total** | | **851** (795 com nota, 56 sem) |

**O que permitiria um cadastro confiável**, e a fila diz por comprador: cadastro na SA1 (falta em 24),
endereço, município com código IBGE e inscrição estadual; nome igual entre ART e Protheus; e, em todos,
a decisão de filial e carteira responsáveis. **705** compradores já têm na SA1 endereço, município,
inscrição e o mesmo nome — falta só a decisão. Os 1.536 registros pendentes por comprador ausente entram
quando o cliente existir no CRM: a recarga os reencontra e a fila marca o comprador como cadastrado.

### 10.9 As divergências

| Tipo | Abertas |
|---|---|
| Comprador da venda mais recente ≠ dono registrado no CRM | 27 |
| Dono do chassi no Protheus (VV1) ≠ comprador da venda mais recente | 20 |

Dos 2.106 chassis importáveis, 2.106 estão no VV1 do Protheus: 684 com dono igual ao comprador, 20
diferentes, **245 com dono ambíguo** — o VV1 guarda o código do cliente sem a loja, e o código tem lojas
com documentos diferentes — e os demais sem dono no Protheus. O ambíguo não vira divergência.

### 10.10 Pendências desta integração

| # | Pendência | Efeito enquanto não resolvida | Quem decide |
|---|---|---|---|
| P-31 | Regra comercial para "Grandes Contas" × SAM/KAM | o valor fica na venda, sem classificar o cliente | comercial |
| P-32 | Revisar os 91 produtos sem correspondência e o "CH 570" duplicado no catálogo | 989 máquinas incluídas sem modelo | comercial + cadastro |
| P-33 | Cadastrar os 851 compradores ausentes (705 com dados completos na SA1) | 1.536 vendas pendentes | comercial (cadastro) |
| P-34 | Filiais inativas com vendas no ART (Guaíra, Ituverava, Monte Alto) | 15 vendas fora da tela de qualquer filial em operação | comercial + TI |
| P-35 | Revisar as 47 divergências de dono | nenhuma posse alterada | comercial |
| P-36 | Chassi curto (741) e fora do padrão (153): implemento com número de série? | não importados | responsável pelo ART (documento 37, §6) |
| P-37 | Família do catálogo para colheitadeira, plantadeira e plataforma de corte | classificação sem família | cadastro |
| P-38 | Acesso a propriedade, área, cultura e horímetro | funcionalidades bloqueadas | responsável pelo ART (documento 37) |
| P-39 | A tela de revisão só lê: confirmar ou recusar correspondência e resolver divergência ainda não têm tela | a revisão é feita no banco | TI |
| P-40 | Máquina revendida por outra filial: a venda de uma filial não aparece no histórico da máquina de outra | o histórico comercial de uma filial é parcial para 2 chassis; na revisão de divergências da filial 010101, 10 de 20 linhas mostram a máquina como "—" porque ela é de outra filial | TI + diretoria (com P-8/P-20) |

### 10.11 Validação na aplicação local [medido]

API local (`http://localhost:5145`, apontada para o banco local) e tela em `http://localhost:5199`,
capturadas com `scripts/prototipo/capturar-integracao-art.mjs` — os exemplos entram por arquivo em
`dados-locais/` e as capturas ficam em `dados-locais/capturas/`, as duas pastas fora do Git. Zero erro
de console e zero resposta de erro da API.

**Tela × banco × relatório da carga, filial 010101:**

| Número | Tela | SQL independente | Relatório da carga |
|---|---|---|---|
| Equipamentos com venda registrada (filtro "Só com venda") | 625 | 625 | — |
| Classificação "Trator médio" | 87 | 87 | — |
| Porte "Grande" | 187 | 187 | — |
| Origem ART | 616 | 616 | — |
| Vendas importadas | 632 | 632 | — |
| Vínculos "comprador na venda" ativos | 632 | 632 | — |
| Máquinas do ART sem dono confirmado | 616 | 616 | — |
| Compradores ausentes: com nota / sem nota | 201 / 7 | 201 / 7 | — |
| Divergências abertas | 20 | 20 | — |

**Soma das 13 filiais em operação (comparação entre filiais na tela) × relatório:**

| Número | Soma na tela | Filiais inativas (SQL) | Total | Relatório |
|---|---|---|---|---|
| Vendas importadas | 2.093 | 15 | 2.108 | 2.108 |
| Vínculos ativos | 2.093 | 15 | 2.108 | 2.108 |
| Máquinas do ART sem dono confirmado | 2.060 | 15 | 2.075 | 2.075 |
| Compradores com nota | 791 | 4 | 795 | 795 |
| Compradores sem nota | 55 | 1 | 56 | 56 |

**Os exemplos reais** (chassi e código são de catálogo; nomes ficam só nas capturas locais):

| Caso | O que a tela mostra |
|---|---|
| Inclusão | máquina de origem ART, modelo e classificação da correspondência exata, situação "dono não confirmado" com o aviso, uma venda com o comprador marcado **comprador na venda** |
| Máquina que já existia | dono do CRM preservado na coluna Cliente; a venda do ART aparece no histórico; depois da correção de §10.5.1, sem a classificação que contradizia o modelo |
| Venda repetida | os dois chassis com duas vendas importáveis têm as vendas em **filiais diferentes**; cada filial vê só a sua venda e o histórico respeita a fronteira (P-40). O teste de API cobre as duas vendas na mesma filial |
| Proprietário divergente | dono no CRM ≠ comprador da venda mais recente; a divergência aparece na revisão com o link para a máquina, e a ficha continua com o dono do CRM |
| Produto sem correspondência | "sem modelo — produto do ART pendente de revisão", com o produto original ("TR 6115J") na ajuda do campo; a classificação "Trator médio" vem da linha |

### 10.12 Revisão independente e o que ela mudou

Um revisor separado leu o código, os testes e este documento, sem editar nada. **Nenhum problema
bloqueante.** Confirmado sem achado sério: comprador não vira dono, recarga sem duplicar, fronteira de
filial, proibição de semelhança de nome, datas e chassi, leitura só do ART e do Protheus, credencial fora
de log, dado pessoal fora da trilha, simulação desfeita e rótulo "comprador na venda" na tela.

| Achado | Gravidade | O que foi feito |
|---|---|---|
| A fila de compradores e as divergências da tela de revisão não filtravam pelo sistema ART | média | filtradas por `SistemaId` do ART (`RepositorioDaIntegracaoDoArt`) |
| A lista de equipamentos repetia subconsultas correlacionadas por máquina | média | **medido antes:** 0,6–0,7 s para 25 linhas e 5,5–7,5 s para 200 (a leitura do filtro de frota). A classificação e a venda mais recente passaram a vir em consultas pelo conjunto de máquinas da página. **Medido depois:** 50–130 ms para 25 linhas e 70–180 ms para 200, com os mesmos totais (625, 87, 187) |
| `CK_Equipamento_ModeloPendente` aceitava modelo vazio também na origem Protheus | baixa | apertada para `[ModeloId] IS NOT NULL OR [Origem] = 'Art'` — migração `ModeloPendenteSoNaOrigemArt`, aplicada no banco local |

### 10.13 Testes executados

| Suíte | Resultado |
|---|---|
| Domínio | 183 aprovados |
| Aplicação | 56 aprovados |
| Integração (inclui 32 do saneamento, da classificação e da regra de família do ART) | 133 aprovados |
| API (inclui 7 da integração do ART por HTTP) — depois das correções da revisão | 89 aprovados |
| Arquitetura (esquema, nomenclatura, 79 tabelas) | 62 aprovados |
| Front (`tsc -b` e `vite build`) | sem erro |

### 10.14 Como rodar de novo

```powershell
./scripts/integracao/rodar-carga-do-art.ps1 -Simular   # mostra os números, desfaz tudo
./scripts/integracao/rodar-carga-do-art.ps1            # grava no banco local
```

O script monta a conexão só na memória do processo, recusa outro servidor que não o local e não imprime
credencial.

> **Superado pela §11 para o servidor.** O script continua valendo só para o banco local de
> desenvolvimento. No servidor, quem sincroniza é o serviço do Windows `TracbelCrmSincronizacaoArt`, e a
> tela de revisão citada em §10.11 e P-39 saiu da aplicação (§11.5).

---

## 11. A integração como serviço do Windows e o banco central [medido]

Pedido de 14/09/2026: a sincronização com o ART precisa rodar sem terminal, navegador nem sessão aberta.
O banco da aplicação publicada é o único banco operacional, e os dados do ART aparecem nas telas que já
existem, não numa página exclusiva.

### 11.1 A arquitetura confirmada

```text
ART (MySQL, view de vendas,        Protheus (SQL Server,
sessão READ ONLY)                  só SELECT, ApplicationIntent=ReadOnly)
          \                               /
           TracbelCrmSincronizacaoArt  ── serviço do Windows ── AGRO-SISTEMAS-W
                         │
           SQL Server MSSQLSERVER · banco TracbelCrm ── serviço do Windows ── AGRO-SISTEMAS-W
                         │
           TracbelCrmApi (porta 5443, Entra ID) ── serviço do Windows ── AGRO-SISTEMAS-W
                         │
           telas: Equipamentos (lista e ficha) · Clientes (ficha) · Configurações › Integrações
```

| Componente | Onde roda | Como roda | Evidência |
|---|---|---|---|
| ART | servidor MySQL do ART (fora do CRM) | leitura da view com `SET SESSION TRANSACTION READ ONLY` | `LeitorDoArt` |
| Protheus | banco do Protheus (fora do CRM) | só `SELECT` em VV1010 e SA1010 | `LeitorDoCadastroDoProtheus` |
| Serviço de integração | **AGRO-SISTEMAS-W** (10.150.4.249) | serviço `TracbelCrmSincronizacaoArt`, `C:\aplicacoes\tracbel-crm-sincronizacao\Tracbel.Crm.Carga.exe --servico-art` | §11.9 |
| Banco central | **AGRO-SISTEMAS-W** | SQL Server 2025 Express, instância `MSSQLSERVER`, **serviço nativo do Windows**, banco `TracbelCrm` | §1 |
| API e telas | **AGRO-SISTEMAS-W** | serviço `TracbelCrmApi`, `C:\aplicacoes\tracbel-crm`, conexão `localhost` / `TracbelCrm` | §11.9 |
| DBeaver | AGRO-SISTEMAS-W | cliente de administração; não executa banco | — |

**O servidor alcança as duas bases** (teste de porta para o ART e o Protheus a partir do próprio
servidor, por tarefa agendada temporária), e por isso o serviço roda nele. Esta estação também
alcança, mas não participa da operação: nenhum dado sai daqui para o servidor.

### 11.2 O Docker do servidor e o desta estação [medido]

**Servidor AGRO-SISTEMAS-W:** Docker Engine 26.1.4 do Windows em execução (serviço `docker`), **0 contêineres e 0 imagens**; o Docker
Desktop está parado. Não há contêiner a descrever: nenhum nome, imagem, porta, volume ou política de
reinicialização. Nem a aplicação nem o banco dependem do Docker, e **nada foi parado nem removido**. O levantamento completo, com as três camadas do Docker e a limitação de virtualização da VM, está em §12.

**Esta estação (HZYLRK4)** tem dois contêineres do projeto, e nenhum deles é operacional:

| Contêiner | Imagem e versão | Estado e reinício | Porta | Volume | Finalidade |
|---|---|---|---|---|---|
| `tracbel-crm-db` | `mcr.microsoft.com/mssql/server:2022-latest` (16.0.4265.3), compose `tracbel-crm` | em execução, `unless-stopped` | 1433 | `tracbel_crm_dados` → `/var/opt/mssql` | banco de **desenvolvimento** (`TracbelCrm` local) |
| `tracbel-crm-ensaio` | `mcr.microsoft.com/mssql/server:2025-latest` (17.0.4085.5), sem compose | em execução, sem reinício automático | 14334 | nenhum (dados na camada do contêiner) | **cópias** do banco do servidor para ensaio: `TracbelCrmServidor`, `TracbelCrmServidorVazio` e `TracbelCrmEnsaioArt` |

### 11.3 O serviço de sincronização

**O mesmo executável da carga.** `Tracbel.Crm.Carga.exe --servico-art` sobe uma hospedagem de serviço
do Windows (`Sincronizacao/ServicoDeSincronizacaoDoArt.cs`), e cada ciclo chama a mesma `CargaDoArt` que
foi conferida no banco local (§10). Nenhuma regra de saneamento, correspondência ou gravação foi
reescrita.

| Exigência | Como está feito |
|---|---|
| Inicia com o Windows | `start= delayed-auto`, dependente de `MSSQLSERVER`; espera inicial de 60 s |
| Frequência configurável | `Sincronizacao:Art:IntervaloMinutos` (5 a 1.440; padrão 60), contado do fim de um ciclo ao começo do seguinte |
| Registra execução, resultado e falha | uma linha por ciclo em `integracao.ExecucaoDeSincronizacao`: início, fim, máquina, resultado (`Sucesso`, `Falha`, `Ignorada`), tentativas, lidos, incluídos, atualizados, pendentes e o resumo ou o motivo |
| Última sincronização bem-sucedida | `integracao.PontoDeSincronismo` do fluxo `ART.VENDA_DE_MAQUINA`, atualizado só no sucesso; também aparece na tela (§11.5) |
| Recupera-se de falha temporária | até `Tentativas` (padrão 3) por ciclo, com espera de 30 s que dobra a cada tentativa, até 15 min. Cada tentativa é uma carga inteira numa transação: a que falha é desfeita. Se o banco do CRM não responde nem para começar, o próximo ciclo vem em até 5 min. Se o processo cair, o Windows o reinicia (1, 2 e 5 min) |
| Evita duplicação | a carga é repetível (§10.5): venda pelo código do ART, máquina pelo chassi, vínculo pela venda. Além disso, a trava `sp_getapplock` impede dois ciclos ao mesmo tempo, inclusive o serviço e a carga manual |
| Preserva correções | as mesmas regras de §10: dono, modelo, classificação, correspondência revisada e divergência resolvida não são desfeitos |
| ART só em leitura | sessão `READ ONLY` no MySQL; Protheus só `SELECT` |
| Credenciais fora dos logs | as leituras devolvem só o código do erro; toda mensagem de exceção passa por `Sigilo.Mascarar`, que troca usuário e senha das três conexões e qualquer `Password=` por `***` |
| Parada limpa | a parada pelo Windows cancela o ciclo; a transação é desfeita e a execução fica como `Falha` com "interrompida pela parada do serviço" |

**Configuração no servidor** — `C:\aplicacoes\tracbel-crm-sincronizacao\appsettings.Production.json`,
restrito a administradores e SYSTEM:

| Seção | Chaves | De onde vem |
|---|---|---|
| `ConnectionStrings:Crm` | cadeia do banco central | copiada do `appsettings.Production.json` da API no servidor — é o mesmo destino da aplicação publicada |
| `Art` | `Servidor`, `Porta`, `Banco`, `Usuario`, `Senha`, `Visao` | `.env` da estação que publica (`ART_DB_*`, `ART_VIEW`) |
| `ProtheusBanco` | `Servidor`, `Banco`, `Usuario`, `Senha` | `.env` (`TOTVS_DB_*`) |
| `Sincronizacao:Art` | `Habilitada`, `IntervaloMinutos`, `EsperaInicialSegundos`, `Tentativas`, `EsperaEntreTentativasSegundos` | parâmetros do `publicar.ps1` |

`Habilitada` só existe nesse arquivo: com a configuração de desenvolvimento, o serviço registra o motivo
no Log de Aplicativo e não grava em banco nenhum.

### 11.4 O banco central

- **Destino confirmado:** a API publicada lê `localhost` / `TracbelCrm` no próprio servidor (arquivo de
  produção da API), e o serviço usa **a mesma cadeia**, copiada desse arquivo pelo `publicar.ps1`, sem
  passar por tela nem log.
- **Nenhuma cópia local é operacional:** `tracbel-crm-db` é desenvolvimento, e `tracbel-crm-ensaio` guarda
  cópias para ensaio (§11.2).
- **Antes de alterar o banco central**, o `publicar.ps1` faz uma cópia `COPY_ONLY` com `CHECKSUM` no
  diretório de backup da instância e só depois para os serviços. As migrações rodam na subida da API.
- **Catálogo de classificação:** as dez classificações de `frota.LinhaDeProduto` entram pela migração
  `SincronizacaoComoServicoECatalogoDeClassificacao`, com o mesmo `MERGE` do seed. O servidor não recebe
  o seed de desenvolvimento, e sem esse catálogo a carga recusa rodar.
- **Tabela nova:** `integracao.ExecucaoDeSincronizacao`, e o modelo passa a ter **80 tabelas** (documento
  14, §2.1).

### 11.5 Os dados do ART nas telas existentes

**Saiu da aplicação:** a rota e o item de menu "Integração ART", a tela, as rotas
`/api/v1/integracoes/art/{resumo,correspondencias,compradores-pendentes,divergencias}`, seus casos de uso,
a porta, o repositório e o script de captura dessa tela. Os dados continuam no banco.

| Tela | O que mostra do ART | Rota |
|---|---|---|
| Equipamentos — lista | classificação, porte, "só com venda", última venda e o comprador nela (§10) | `GET /api/v1/equipamentos` |
| Equipamentos — ficha | histórico comercial (§10) e **novo cartão "Divergências abertas"**: tipo, descrição e data de cada divergência da máquina | `GET /api/v1/equipamentos/{chave}` e `/vendas` |
| Clientes — ficha | **novo cartão "Máquinas compradas"**: data da venda, chassi, modelo e classificação, filial, se o comprador é também o dono atual, e a origem | `GET /api/v1/clientes/{chave}/maquinas-compradas` |
| Catálogos e filtros | `LINHA_DE_PRODUTO` e `PORTE_DE_MAQUINA` | `GET /api/v1/catalogos/...` |
| Configurações › TI e Integrações › Integrações | **novo cartão "Sincronizações do servidor"**: última execução, último sucesso e as dez execuções recentes, com tentativas, contagens e motivo | `GET /api/v1/integracoes/sincronizacoes` |

**Não aparecem em tela,** por não terem uso definido: as correspondências de produto e linha, a fila de
compradores ausentes e os campos brutos do ART. O que continua pendente de decisão está em P-33 e P-39.

### 11.6 Instalar, configurar, atualizar e verificar

| Ação | Como |
|---|---|
| **Instalar** (primeira vez) e **atualizar** | nesta estação, `.\scripts\deploy\publicar.ps1`. O script testa, gera os dois pacotes, faz a cópia do banco central, para os serviços, copia, grava a configuração da sincronização se ainda não existe, cria o serviço se ainda não existe, sobe a API (migrações), confere a saúde, sobe a sincronização e confere os arquivos por hash. Ninguém entra no servidor |
| **Mudar a frequência** | `.\scripts\deploy\publicar.ps1 -ReconfigurarSincronizacao -IntervaloMinutos 30`, ou, no servidor, editar `IntervaloMinutos` no arquivo de configuração e reiniciar o serviço |
| **Trocar credencial do ART ou do Protheus** | atualizar o `.env` e rodar `publicar.ps1 -ReconfigurarSincronizacao` |
| **Verificar na aplicação** | Configurações › TI e Integrações › Integrações |
| **Verificar pelo banco** | `.\scripts\deploy\verificar-sincronizacao.ps1`, que mostra estado do serviço, Log de Aplicativo, execuções, ponto de sincronismo, contagens e duplicidades; as mesmas consultas servem no DBeaver do servidor |
| **Verificar no servidor** | Visualizador de Eventos › Logs do Windows › Aplicativo, origem `TracbelCrmSincronizacaoArt` |
| **Parar ou desligar** | `sc.exe \\10.150.4.249 stop TracbelCrmSincronizacaoArt`; para não voltar no próximo reinício, `sc.exe \\10.150.4.249 config TracbelCrmSincronizacaoArt start= disabled` |

### 11.7 Validação local [medido]

| O que | Resultado |
|---|---|
| Testes | domínio 186 · integração 133 · API 90 (dados do ART nas telas por HTTP, rota antiga 404, execuções na administração) · aplicação 64 (sigilo, espera entre tentativas, configuração) · arquitetura 62 (80 tabelas) · front `tsc -b` e `vite build` sem erro |
| Migração nova no banco local | aplicada |
| Duas execuções reais seguidas no banco local | 19:48:07–19:48:15 e 19:48:15–19:48:22 UTC; 4.144 lidos, 0 vendas incluídas, 0 atualizadas, 2.036 pendentes em cada: **a repetição não grava nada** |
| Trava | com a primeira instância já lendo o ART, a segunda foi **recusada em 3,6 s** ("trava sp_getapplock ocupada", código 3) e ficou registrada como `Ignorada`; a primeira terminou normalmente em 9,2 s |
| API local, filial 010101 | rota antiga 404; administração com `Ignorada` e `Sucesso`; 8 das 200 primeiras máquinas com venda têm divergência aberta na ficha; 40 compradores somam 76 máquinas compradas, e em 1 delas o comprador é também o dono atual |

### 11.8 Ensaio numa cópia do banco do servidor [medido]

**Cópia usada:** `TracbelCrmEnsaioArt`, restaurada em `tracbel-crm-ensaio` a partir de `TracbelCrmServidor`
(§3). Tem 23.945 clientes, 3.888 máquinas e a mesma última migração do servidor antes da publicação
(`AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial`). As cópias de §3 não foram alteradas.

| Etapa | Resultado |
|---|---|
| As três migrações pendentes do servidor | aplicadas em **4,5 s**, sem erro; nenhuma máquina com situação fora do domínio |
| Catálogo de classificação pela migração | 10 classificações; família resolvida para trator (3), colhedora de cana, pulverizador e os dois implementos; colheitadeira, plantadeira e plataforma sem família, como esperado |
| **Um ciclo do serviço** (`--servico-art`, a mesma hospedagem do Windows, configurado por variável de ambiente) | `Sucesso` em **8 s** (19:57:08–19:57:16 UTC), 1 tentativa |
| O que o ciclo gravou | 4.144 lidos; **2.108 vendas** e 2.108 vínculos; **2.075 máquinas novas** e 31 existentes reaproveitadas; 2.036 pendentes; **851** compradores na fila; **47** divergências abertas |
| Duplicidade | 0 vendas repetidas pelo código da origem; 0 chassis repetidos |
| Ponto de sincronismo | `ART.VENDA_DE_MAQUINA`, 4.144 lidos, 2.108 gravados, 2.036 pendentes |

Os números são os mesmos da carga no banco local (§10.5).

**Um desvio nesta etapa, corrigido.** A primeira tentativa de ensaio apontou para o banco `TracbelCrm` do
contêiner de ensaio, que não existia: o nome da cópia é outro. A ferramenta de migração **criou um banco
vazio** com esse nome às 19:51:21 e aplicou nele a cadeia inteira de migrações. Isso comprovou que as
migrações rodam do zero num SQL Server 2025, mas não testou os dados do servidor. O banco foi conferido
(0 clientes, 0 máquinas, só as 10 classificações da migração) e removido às 19:54, e o ensaio foi refeito na
cópia correta. Nenhum banco existente foi alterado.

### 11.9 Publicação e primeira sincronização no servidor [medido]

Horários do servidor (UTC−3), exceto quando indicado.

**A publicação levou três tentativas.** As duas primeiras pararam por defeito do próprio `publicar.ps1`,
ambos corrigidos:

| Tentativa | Onde parou | Efeito no servidor | Correção |
|---|---|---|---|
| 1 | passo 3, antes de parar qualquer serviço | nenhum: API no ar, nada copiado | os dois blocos do script remoto de cópia de segurança eram concatenados sem quebra de linha, e o `New-Object` recebia o resto do script como argumento |
| 2 | passo 6, **depois** de parar a API e copiar | cópia `TracbelCrm-antes-da-publicacao-20260914-175518.bak` (501,8 MB) feita; **a API ficou parada** por alguns minutos; o arquivo de configuração da sincronização ficou criado **vazio** (0 byte, sem credencial) | a permissão do arquivo usava o nome `BUILTIN\Administrators`, que não se traduz no Windows em português desta estação; passou a usar os SIDs (S-1-5-32-544 e S-1-5-18). A API foi religada com `sc start`, voltou `RUNNING` com o banco conectado e **as três migrações aplicadas** |
| 3 (`-ReconfigurarSincronizacao`) | concluída | os 10 passos passaram | — |

**Estado depois da tentativa 3:**

| Item | Resultado |
|---|---|
| Cópia de segurança antes da parada | `TracbelCrm-antes-da-publicacao-20260914-175751.bak`, 502,2 MB, `BACKUP ... WITH CHECKSUM`. O `RESTORE VERIFYONLY` não é permitido à conta da aplicação; o `CHECKSUM` do backup conferiu as páginas |
| API | `RUNNING`; `/saude/banco` conectado, nenhuma migração pendente; 444 arquivos idênticos ao pacote por hash |
| Serviço `TracbelCrmSincronizacaoArt` | `AUTO_START (DELAYED)`, conta `LocalSystem`, dependente de `MSSQLSERVER`, reinício após 1, 2 e 5 min, `RUNNING`; 279 arquivos idênticos ao pacote |
| Configuração do serviço | 836 bytes, herança desligada, só `AUTORIDADE NT\SISTEMA` e `BUILTIN\Administradores` |
| Log de Aplicativo | 17:58:28 "Serviço TracbelCrmSincronizacaoArt iniciado na máquina AGRO-SISTEMAS-W. Destino: localhost / TracbelCrm. Intervalo: 60 min; tentativas por ciclo: 3"; 17:59:53 "Sincronização do ART concluída" |

**Primeiro ciclo no banco central** (`verificar-sincronizacao.ps1`):

| Medida | Servidor | Ensaio (§11.8) |
|---|---:|---:|
| Execução | 20:59:31–20:59:53 UTC, `Sucesso`, 1 tentativa | 8 s, `Sucesso` |
| Registros lidos do ART | 4.144 | 4.144 |
| Vendas incluídas e vínculos "comprador na venda" ativos | **2.109** e 2.109 | 2.108 e 2.108 |
| Máquinas de origem ART | **2.076** | 2.075 |
| Pendentes | 2.035 | 2.036 |
| Compradores na fila | 851 | 851 |
| Divergências abertas | 47 | 47 |
| Classificações de produto | 10 | 10 |
| Vendas repetidas pelo código da origem / chassis repetidos | **0 / 0** | 0 / 0 |
| Ponto de sincronismo | `ART.VENDA_DE_MAQUINA`, 20:59:53 UTC, 4.144 lidos, 2.109 gravados, 2.035 pendentes | — |

Um registro a mais foi importado no servidor do que no ensaio, e um a menos ficou pendente. A cópia de
ensaio é da manhã, e o banco central mudou desde então; a causa desse registro não foi investigada.

**Exibição.** As telas do servidor leem esse mesmo banco pela API publicada. Elas exigem o login do Entra
ID, e por isso a conferência visual no servidor **não foi feita pela sessão de trabalho**: fica com quem tem
acesso, em Configurações › TI e Integrações › Integrações, na ficha de uma máquina vendida e na ficha de
um comprador. A mesma camada foi conferida por HTTP nos testes da API e na API local (§11.7).

**Próximo ciclo:** 60 min depois do fim do primeiro. A repetição sem gravação foi comprovada no banco local
(§11.7). No servidor, confere-se pela segunda linha de `ULTIMAS EXECUCOES`, com 0 vendas incluídas.

---

## 12. O ambiente real da VM do servidor [medido]

Levantamento de 14/09/2026, 18:25 no horário do servidor, feito com `scripts/deploy/diagnosticar-servidor.ps1`.
O script roda **no próprio servidor**, com a conta SYSTEM (a mesma do serviço de sincronização), e **só lê**:
nada foi parado, reiniciado, instalado ou removido. Os contêineres desta estação não entram neste
levantamento.

### 12.1 A máquina

| Item | Valor |
|---|---|
| Nome | `AGRO-SISTEMAS-W` |
| Domínio | **membro de `tracbel.com.br`**. Os comentários de `publicar.ps1` e `_remoto.ps1` diziam que não era; foram corrigidos |
| Plataforma | máquina virtual **OpenStack Nova** (BIOS SeaBIOS); hypervisor presente |
| Windows | **Windows Server 2022 Datacenter**, 21H2, build 20348.5622, 64 bits, com interface gráfica |
| CPU | Intel Xeon Gold 6278C, 2 núcleos e 4 processadores lógicos |
| Memória | 16 GB (4,9 GB livres no momento) |
| Discos | C: 126,5 GB (32,3 GB livres) · D: 32 GB (28 GB livres, sem uso pelo CRM) |
| Último boot | 09/09/2026 09:29 |
| Sessões de usuário | 2 abertas, **ambas desconectadas** |

### 12.2 A VM consegue rodar contêiner?

Os recursos do Windows estão **habilitados**: Hyper-V, Containers, Virtual Machine Platform e o Subsistema do
Windows para Linux. Mas **a VM não expõe as extensões de virtualização à CPU**
(`VMMonitorModeExtensions=False`, `VirtualizationFirmwareEnabled=False`). Em todo boot, o Windows registra no
log de Sistema o evento 41: *"Hypervisor launch failed; Either VMX not present or not enabled in BIOS"*
(09/09/2026 às 03:27, 03:29 e 09:29).

**Consequência:** o hypervisor do Windows não sobe. Por isso **não funciona nesta VM** nada que precise de uma
máquina virtual dentro dela: WSL 2, o motor Linux do Docker Desktop e contêineres com isolamento Hyper-V. O
que funciona é contêiner **Windows com isolamento de processo**, que é o modo do Docker Engine instalado
(§12.3).

### 12.3 Docker, em três camadas

| Camada | O que existe na VM | Estado |
|---|---|---|
| **Cliente** | `docker.exe` 29.5.3, do Docker Desktop (o primeiro no PATH), e `docker.exe` 26.1.4, do Docker Engine | instalados |
| **Docker Desktop** 4.78.0 (arquivos de 12/06/2026) | serviço `com.docker.service` | **parado**, início manual; nenhum processo; o pipe `dockerDesktopLinuxEngine` não existe; o disco WSL do Docker Desktop tem 0,1 GB. Não consegue rodar nesta VM (§12.2) |
| **Docker Engine** 26.1.4 (Moby, `C:\Program Files\DockerEngine`, arquivos de 05/06/2024) | serviço `docker` (`dockerd.exe --run-service`) | **em execução** desde o boot, início automático, LocalSystem, 62 MB de memória; tipo `windows`, isolamento `process`, raiz `C:\ProgramData\docker`, sem `daemon.json` |
| **Contexto ativo** | `DOCKER_HOST` da máquina = `npipe:////./pipe/docker_engine` | sobrepõe o contexto gravado `desktop-linux` do único perfil com pasta `.docker`: **qualquer cliente docker na VM fala com o Docker Engine do Windows desta VM** |
| **Contêineres** | `docker ps -a`, `docker images`, `docker volume ls` | **0 contêineres, 0 imagens, 0 volumes** |
| WSL e Hyper-V | WSL 2.7.10 instalado; `WSLService`, `vmms`, `vmcompute` e `hns` em execução; `LxssManager` parado | serviços no ar, sem máquina virtual possível |

**Não confundir com esta estação (HZYLRK4):** `tracbel-crm-db` e `tracbel-crm-ensaio` são contêineres **da
estação** (§11.2). Nela há o cliente 29.7.2, com o contexto `desktop-linux`, e no momento do levantamento o
motor estava parado.

### 12.4 Quem executa cada componente

| Serviço | Estado | Início | Conta | Executável | Usa Docker? |
|---|---|---|---|---|---|
| `TracbelCrmApi` | em execução desde 14/09 17:58:21, sessão 0 | automático | LocalSystem | `C:\aplicacoes\tracbel-crm\Tracbel.Crm.Api.exe` | não |
| `TracbelCrmSincronizacaoArt` | em execução desde 14/09 17:58:26, sessão 0 | automático (atrasado) | LocalSystem | `C:\aplicacoes\tracbel-crm-sincronizacao\Tracbel.Crm.Carga.exe --servico-art` | não |
| `MSSQLSERVER` | em execução desde 09/09 13:22, sessão 0 | automático (atrasado) | `NT AUTHORITY\NETWORKSERVICE` | `...\MSSQL17.MSSQLSERVER\MSSQL\Binn\sqlservr.exe -sMSSQLSERVER` | **não** — o próprio SQL Server informa `host_platform = Windows`, `Windows Server 2022 Datacenter` |
| `SQLSERVERAGENT` / `SQLBrowser` | parados | desabilitados | — | — | — |
| `SQLWriter`, `SQLTELEMETRY` | em execução | automático | — | componentes do SQL Server | não |
| `docker` | em execução | automático | LocalSystem | `dockerd.exe` | **nenhum componente do CRM usa** |

Também roda na VM o serviço `TracbelVerificacao`, feito com NSSM: uma aplicação Python de outro projeto, na
pasta de perfil de um usuário. Ele não faz parte do CRM e não foi avaliado.

**Nenhuma dependência de Docker foi encontrada.** API, integração e banco são serviços nativos do Windows.

### 12.5 Banco e integração

| Item | Valor |
|---|---|
| Instância | `AGRO-SISTEMAS-W`, instância padrão `MSSQLSERVER`, **SQL Server 2025 Express** (64 bits) 17.0.1000.7 |
| Banco da aplicação | `TracbelCrm`, recuperação `FULL`, colação `Latin1_General_CI_AI`, criado em 09/09/2026 13:23 |
| Destino da API | `localhost` / `TracbelCrm`, login SQL (seções `Kestrel`, `Entra`, `ConnectionStrings`) |
| Destino da integração | `localhost` / `TracbelCrm`, com **exatamente a mesma cadeia de conexão da API** |
| Último sucesso | 20:59:31–20:59:53 UTC (17:59 no servidor), 1 tentativa; 4.144 lidos, 2.109 vendas incluídas, 0 atualizadas, 2.035 pendentes, 2.076 máquinas incluídas |
| Ponto de sincronismo | `ART.VENDA_DE_MAQUINA`, `2026-09-14T20:59:53Z`, 4.144 lidos, 2.109 gravados, 2.035 pendentes |

O DBeaver instalado no servidor é **cliente**: conecta nessa instância para consultar e administrar. Quem
executa o banco é o serviço `MSSQLSERVER`.

### 12.6 Onde ficam dados, logs, backups e configurações

| O quê | Onde |
|---|---|
| Dados | `C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER\MSSQL\DATA\TracbelCrm.mdf` (520 MB) |
| Log de transação | `...\MSSQL\DATA\TracbelCrm_log.ldf` (**1.032 MB**) |
| Backups | `...\MSSQL\Backup`: 4 arquivos, 1,93 GB — `antes-da-publicacao` de 14/09 17:57 (502,2 MB) e 17:55 (501,8 MB), `antes-do-territorio` de 14/09 09:46 (484,3 MB) e `copia-antes-do-territorio` de 14/09 08:42 (484,3 MB). O msdb registra também a origem do banco: o backup de 08/09/2026 17:33, feito na estação. Todos são `COPY_ONLY` manuais |
| Log do SQL Server | `...\MSSQL\Log` (4 arquivos `ERRORLOG`) |
| Log da API | **Log de Aplicativo do Windows**, origens `Tracbel.Crm.Api` e `TracbelCrmApi`. Não existe pasta `logs` na aplicação |
| Log da integração | Log de Aplicativo, origem `TracbelCrmSincronizacaoArt`, e a tabela `integracao.ExecucaoDeSincronizacao` |
| Configurações | `C:\aplicacoes\tracbel-crm\appsettings.Production.json` e `C:\aplicacoes\tracbel-crm-sincronizacao\appsettings.Production.json` (este restrito a SYSTEM e Administradores, sem herança) |

### 12.7 Autonomia do serviço de integração

| Exigência | Evidência |
|---|---|
| Não depende de terminal ou sessão | processo na **sessão 0** (serviços), conta LocalSystem. As duas sessões de usuário estavam desconectadas, e mesmo assim o ciclo rodou e registrou no banco e no Log de Aplicativo |
| Início automático | `AUTO_START (DELAYED)`, dependente de `MSSQLSERVER` |
| Recuperação de falha | reinicia após 60 s, 120 s e 300 s; a contagem zera em 1 dia; as ações valem também para falha sem queda (`FAILURE_ACTIONS_ON_NONCRASH_FAILURES: TRUE`) |
| Acesso às configurações pela conta do serviço | o levantamento rodou como SYSTEM e leu os dois arquivos: seções presentes, `Habilitada=True`, intervalo de 60 min, 3 tentativas, as seis chaves do ART e a seção do Protheus preenchidas |

Para comparação: a API reinicia após 5, 15 e 60 s, mas só quando o processo cai
(`NONCRASH_FAILURES: FALSE`). O SQL Server está com a configuração padrão, sem ação de recuperação.

### 12.8 Há ajuste necessário?

**Na arquitetura, não.** Nada depende de Docker. A integração funciona como serviço do Windows e grava no SQL
Server central. Instalar Docker ou migrar o banco para contêiner não resolveria nenhum problema encontrado,
e contêiner Linux nem é possível nesta VM (§12.2). A estrutura foi mantida.

Pontos encontrados. Todos dependem de decisão de quem administra a VM, e **nenhum foi alterado**:

| Ponto | Evidência | Por que importa |
|---|---|---|
| Banco sem rotina de backup, com log de transação crescendo | recuperação `FULL`, `.ldf` de 1.032 MB (o dobro do `.mdf`), só cópias `COPY_ONLY` manuais, SQL Agent desabilitado (Express) | o log cresce sem limite, e não há ponto de restauração recente automático (já apontado em §1 e §6.3) |
| Backups no mesmo disco dos dados | `.mdf`, `.ldf` e `.bak` no C:, que tem 32,3 GB livres; o D: tem 28 GB livres e não é usado | perder o disco C: leva o banco e as cópias juntos |
| Docker Engine em execução sem uso | serviço `docker` automático, 62 MB, 0 contêineres | consumo pequeno; parar ou manter cabe ao dono da VM, porque pode ser de outro projeto |
| Docker Desktop, WSL e Hyper-V instalados sem poder funcionar | evento 41 do hypervisor em todo boot | ruído no log e componentes para atualizar sem uso |
| A API não reinicia em falha sem queda | `NONCRASH_FAILURES: FALSE` | menor; o serviço de integração já tem essa opção ligada |
