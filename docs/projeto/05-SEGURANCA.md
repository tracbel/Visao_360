# Segurança e permissões — CRM Tracbel

> **Documento 05 de 07** · Versão 1.1 · 16/09/2026 (§11A: segredos medidos, issue #1) · 1.0 em 30/08/2026
> Modelo derivado de `[DYN]` matriz privilégio × profundidade e `[SF]` permission sets + sharing,
> cortado ao tamanho de 130 usuários. `[V]` = defeito medido no Vórtice.

---

## 1. Por que este documento existe

O Vórtice tem **oito camadas de permissão empilhadas** e **nenhuma delas é escopo de registro
transacional**. Medições:

| Achado | Medida |
|---|---|
| Permissão por registro | **não existe** — sem RLS (`sys.security_policies` = 0), **1 trigger no banco inteiro** |
| Onde a segurança é aplicada | **100% no cliente Gupta.** Acesso direto ao banco ignora tudo e não deixa rastro |
| Papéis/perfis | **não existem.** 939 usuários → **370 combinações distintas** de 51 flags. A tabela que resolveria (`IV_SegPerfil`) está vazia |
| Senha | **sem salt.** 663 usuários com senha → **467 valores distintos**; **80 compartilham o mesmo valor** |
| Política de senha | **1 de 11 políticas**, força "Fraco", **sem expiração** |
| Auditoria de login | **114 eventos em 9 anos** |
| Escopo de visibilidade | resolvido em código por **4 rotas paralelas**, uma delas morta (`SeqUrSuperv` NULL em 655/655 carteiras) |
| Hierarquia | ponteiro de **um nível só**, sem closure, **sem reatribuição** |
| Multiempresa | 18 filiais, mas **340 de 409 usuários ativos acessam 17 empresas** |
| Desativar usuário | **destrói as permissões** — não há flag de ativo/inativo |
| Token da API | **texto claro** numa tabela de parâmetros |
| Permissão por campo | existe, amarrada ao nome do widget Gupta — **17 linhas no total** |
| Permissão por tela | cobre **10 de 184 telas**. As outras 174 não têm controle por política |
| Usuários sem política nenhuma | **710 de 1.386 — 51%** |
| Multiempresa na permissão | a coluna `NROEMPRESA` tem **um único valor distinto** |
| Coluna de inativação de usuário | **não existe no schema** — confirmado em `GE_Usuario` |

O resultado prático: **a Tracbel não consegue responder "quem viu o quê" numa auditoria.**

---

## 2. As cinco camadas — e só cinco

`[SF]` tem sete camadas; `[DYN]` tem seis mais compartilhamento linha-a-linha. Boa parte dessa
complexidade existe porque são **multi-tenant globais**. A Tracbel não é. Cinco camadas entregam
~90% do poder com ~15% da complexidade.

```
┌─ 1. AUTENTICAÇÃO ─────────────── Entra ID + MFA. Zero senha no CRM.
│
├─ 2. PERMISSÃO DE ENTIDADE ────── "pode ler Processo?"        ADITIVA (teto)
│      Permissao × ConjuntoPermissao × Usuario
│
├─ 3. PROFUNDIDADE ──────────────── "QUAIS processos?"          ADITIVA
│      0 Nenhum · 1 Próprios · 2 Equipe · 3 Empresa · 4 Empresa+abaixo · 5 Organização
│
├─ 4. COMPARTILHAMENTO ─────────── exceções explícitas          ADITIVA
│      por regra · por equipe · manual · delegação temporária
│
└─ 5. CAMPO SENSÍVEL ───────────── "pode ver a margem?"         SUBTRATIVA
       a única camada que TIRA acesso
```

### A ordem de avaliação — a regra que governa tudo

```
1. Autenticado?                          não → 401
2. Tem a permissão da entidade+verbo?    não → 403   ← TETO ABSOLUTO
3. A linha cabe na profundidade?          sim → PERMITE
4. Há compartilhamento explícito?         sim → PERMITE
                                          não → a linha não existe para você
5. Campo sensível sem autorização?        → devolve o registro SEM o campo
```

**Camadas 2, 3 e 4 são aditivas** (união; vence a mais permissiva). **Camada 5 é a única subtrativa.**
`[SF]` a permissão de objeto/campo é união de profile + permission sets. `[DYN]` é explícito:
**não existe DENY em lugar nenhum** — se você concedeu Organização em Contato, não dá para esconder
um registro depois.

> **Decisão consciente:** não implementamos regra de negação (`deny`). Ela parece útil e é a origem de
> metade dos bugs de autorização em sistemas grandes, porque a ordem de avaliação vira imprevisível.
> Quando alguém não pode ver algo, **reduza a profundidade** — não crie exceção negativa.

---

## 3. Camada 1 — autenticação

**Delegada ao Microsoft Entra ID.** O CRM **não armazena senha**, e essa é uma decisão de segurança,
não de conveniência.

| Item | Como |
|---|---|
| Login | OpenID Connect contra o tenant da Tracbel |
| MFA | política do tenant — já obrigatória |
| Provisionamento | usuário entra na Tracbel → tem conta. `[V]` no Vórtice criar usuário toca **~8 tabelas e ~110 linhas, sem transação e sem API** |
| Desprovisionamento | desligamento no AD → acesso cai. `[V]` no Vórtice **70% das contas nunca logaram** e 87% estão dormentes |
| Token da API | Entra ID (client credentials) para integrações. `[V]` token em texto claro numa tabela |
| Sessão | JWT de 60 min + refresh; revogação por `EstaAtivo = 0` |

**Consequência direta:** as 11 políticas de senha do Vórtice, a janela de horário de acesso e o
armazenamento de hash **deixam de ser problema do CRM**. É código que não escrevemos e superfície que
não defendemos.

---

## 4. Camada 2 — permissão de entidade

Permissão é nomeada, granular por **entidade + verbo**, e agrupada em **conjuntos** (`[SF]` permission set).

```
Permissao:  Lead.Ler · Lead.Criar · Lead.Editar · Lead.Excluir · Lead.Qualificar · Lead.Atribuir
            Processo.Ler · Processo.Editar · Processo.Encerrar · Processo.Reabrir
            Conta.Ler · Conta.Editar · Conta.VerMargem
            Regra.Ler · Regra.Editar · Regra.Ativar
            Relatorio.Criar · Relatorio.Exportar
```

**Não existe "usuário com permissão avulsa".** Toda concessão passa por um conjunto. `[V]` é
exatamente isso que produziu as 370 combinações distintas para 939 usuários — cada pessoa virou um
floco de neve, e ninguém consegue auditar.

### Conjuntos previstos na fase 1

| Conjunto | Para quem | Profundidade típica |
|---|---|---|
| `CEN` | consultor de vendas | Próprios |
| `ASSISTENTE_VENDAS` | apoio administrativo | Equipe |
| `GERENTE_VENDAS` | líder de CEN | Empresa (via hierarquia) |
| `ADM_VENDAS` | Kálida e time | Empresa |
| `MARKETING` | quem trabalha o lead | Empresa (só Lead) |
| `GESTOR_REGIONAL` | regional | Empresa + abaixo |
| `AUDITORIA` | leitura ampla, zero escrita | Organização (só `.Ler`) |
| `ADMINISTRADOR` | TI | Organização |

**Composição:** um gerente recebe `CEN` + `GERENTE_VENDAS`. União aditiva — vence a mais permissiva.
`[SF]` é exatamente o modelo de permission sets componíveis, e é o que evita a explosão combinatória.

**Concessão temporária:** `UsuarioConjuntoPermissao.ExpiraEm` cobre férias e substituição sem que alguém
esqueça de remover depois — que é como privilégio vira permanente.

---

## 5. Camada 3 — profundidade (o coração do modelo)

`[DYN]` A matriz **privilégio × profundidade** é o achado mais valioso da pesquisa. Ela substitui,
sozinha, o trio OWD + role hierarchy + sharing rules do Salesforce — **e é mais barata**, porque resolve
com colunas da própria linha (`ProprietarioId`, `EmpresaId`), **sem join**.

| Nível | Nome | O usuário vê |
|---|---|---|
| 0 | Nenhum | nada |
| 1 | **Próprios** | onde ele é o proprietário |
| 2 | **Equipe** | os próprios + de quem está abaixo dele na hierarquia de vendas |
| 3 | **Empresa** | tudo da empresa dele |
| 4 | **Empresa e abaixo** | a empresa dele e as filhas (usa `Empresa.Caminho`) |
| 5 | **Organização** | tudo |

A profundidade é gravada **por permissão**, não por usuário. Um CEN pode ter `Processo.Ler` = Equipe e
`Processo.Editar` = Próprios: **vê o do colega, mas não altera**. Essa granularidade é o que o Vórtice
nunca teve.

### Resolvendo o requisito da Tracbel

> *"O vendedor vê só a carteira dele; o líder vê a do CEN inteiro; o gerente regional vê a regional."*

```
CEN                → Processo.Ler = Próprios  (+ compartilhamento por carteira)
Líder de CEN       → Processo.Ler = Equipe    (closure table resolve qualquer profundidade)
Gerente regional   → Processo.Ler = Empresa e abaixo
```

`[V]` No Vórtice isso é resolvido em código por **quatro rotas paralelas** — `IV_VENDEDOR.SeqUsuarioLider`,
`IVS_Carteira.SeqUsrResp`, `IVS_Carteira.SeqUrSuperv` (morta), `IVS_DeptoEmpr.SeqUsrGerDepto` — e essas
rotas **só valem para o mobile**; no desktop não há equivalente configurável. É por isso que trocar o
usuário de uma gerente **esvazia o relatório de carteira**.

### A closure table resolve a reatribuição

`org.HierarquiaVendas` materializa todos os pares ancestral→descendente. Quando `Usuario.GestorId` muda,
a closure é **reconstruída na mesma transação**:

```csharp
/// <summary>
/// Move um usuário na hierarquia, levando TODOS os subordinados junto.
///
/// [V] No Vórtice o checklist de troca de usuário tem 4 passos manuais, e o passo 3 diz
/// literalmente que agendas pendentes "não se reatribuem sozinhas". Aqui o sistema reatribui.
/// </summary>
public async Task MoverNaHierarquiaAsync(long usuarioId, long? novoGestorId, CancellationToken ct)
{
    await using var tx = await _db.Database.BeginTransactionAsync(ct);

    // 1. Remove os vínculos antigos da subárvore inteira
    var subarvore = await _db.HierarquiaVendas
        .Where(h => h.AncestralId == usuarioId)
        .Select(h => h.DescendenteId).ToListAsync(ct);

    await _db.HierarquiaVendas
        .Where(h => subarvore.Contains(h.DescendenteId)
                 && !subarvore.Contains(h.AncestralId))
        .ExecuteDeleteAsync(ct);

    // 2. Reconstrói a partir do novo gestor
    if (novoGestorId is not null)
    {
        var ancestrais = await _db.HierarquiaVendas
            .Where(h => h.DescendenteId == novoGestorId).ToListAsync(ct);

        foreach (var a in ancestrais)
            foreach (var d in subarvore)
                _db.HierarquiaVendas.Add(new VinculoHierarquia(
                    a.AncestralId, d, a.Profundidade + 1));
    }

    // 3. Reatribui as tarefas pendentes de aprovação que estavam com o gestor antigo.
    //    É o passo que o Vórtice deixa manual — e que por isso ninguém faz.
    await _tarefas.ReatribuirAprovacoesPendentesAsync(usuarioId, novoGestorId, ct);

    await _db.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);
}
```

---

## 6. Camada 4 — compartilhamento explícito

Para o que a profundidade não cobre. `[SF]` tabela de share com **`RowCause` tipado** — e a coluna
`Motivo` é o que permite responder **"por que este usuário vê este registro"** sem investigação.

`[DYN]` A Microsoft precisou construir uma API inteira (`RetrieveAccessOrigin`) para responder essa
pergunta. Aqui está gravado desde a primeira linha.

| Motivo | Quem cria | Expira? |
|---|---|---|
| `Regra` | regra de workflow (ex.: entrar na carteira compartilha a conta) | não |
| `Equipe` | pertencer a uma equipe proprietária | não |
| `Manual` | um usuário compartilha com outro | opcional |
| `Delegacao` | cobertura de férias | **sim, obrigatório** |
| `Hierarquia` | materializado quando a closure muda | não |

### O endpoint "por que eu vejo isto"

```csharp
/// <summary>
/// GET /api/v1/processos/{chave}/acesso
///
/// Devolve TODAS as razões pelas quais o usuário atual enxerga este registro.
///
/// [V] O caso B do suporte do Vórtice ("estão aparecendo agendas que não são minhas") levou
/// horas de investigação em IV_AcaoAuto para descobrir que a regra 9067 devolve o follow-up
/// para quem lançou o andamento. Com este endpoint, seriam segundos.
/// </summary>
[HttpGet("{chave:guid}/acesso")]
public async Task<ExplicacaoAcesso> ExplicarAcesso(Guid chave, CancellationToken ct)
{
    // Ex.: [ "Você é o proprietário",
    //        "Compartilhado pela regra CARTEIRA_MAQ em 12/08/2026",
    //        "Sua permissão Processo.Ler tem profundidade Equipe e o proprietário
    //         (João Mendes) está subordinado a você" ]
    return await _explicador.ExplicarAsync("Processo", chave, _contexto.UsuarioId, ct);
}
```

---

## 7. Camada 5 — campo sensível

A única camada subtrativa. Para margem, custo, comissão e dado pessoal.

```csharp
[CampoSensivel("Processo.VerMargem")]
public decimal? MargemPercentual { get; private set; }
```

Sem a permissão, o campo é **omitido do JSON** — não vem `null`.

> `[DYN]` **Armadilha documentada:** o Dataverse devolve **NULL silencioso** para coluna sem permissão,
> o que quebra integrações de forma sutil (o consumidor acha que o valor é vazio, não que é proibido).
> Nós omitimos o campo e devolvemos a lista `camposOmitidos` no envelope da resposta. O consumidor
> **sabe** que existe algo que ele não pode ver.

---

## 8. Um ponto de aplicação — e só um

**A regra:** nenhuma consulta escapa do filtro. Nem relatório, nem exportação, nem job, nem integração.

`[V]` No Vórtice, **125 dos 134 relatórios não têm nenhum predicado de usuário**, e o picklist de
"Carteiras" oferece a carteira de todo mundo. A segurança da tela não vale para o relatório.

| Superfície | Aplicação |
|---|---|
| API / telas | `HasQueryFilter` global do EF Core (ver documento 03, seção 6) |
| Relatórios | a `FonteRelatorio` aponta para uma **view que já filtra**; o usuário nunca escreve SQL |
| Exportação | mesma consulta da tela + registro em `aud.EventoAcesso` como `ExportacaoDados` |
| Jobs | rodam sob identidade de sistema **explícita**, e o `IgnoreQueryFilters()` só é permitido em `Infraestrutura/Sistema/` — verificado por teste de arquitetura no CI |
| Banco | a role da aplicação **não tem** `DELETE` em `wf.Atividade` nem em `aud.*` |

---

## 9. LGPD

`[V]` **Dois defeitos graves medidos:**

1. As views `GE$PESSOA_LGPD`, `GE$CONTATO_LGPD`, `GE$EMAIL_LGPD`, `GE$PESSOAFONE_LGPD` mascaram a coluna
   **e republicam o valor cru na mesma view**, em `z_NOMERAZAO`, `z_NROCGCCPF`, `z_EMAIL`, `z_FONENRO1`.
   **A anonimização é contornável com um `SELECT`.**
2. O opt-in **não tem data, não tem origem e não tem prova** — só um flag. Isso não sustenta uma
   solicitação de titular.

### O que fazemos

| Direito do titular | Implementação |
|---|---|
| Consentimento | `crm.ConsentimentoComunicacao` **append-only**, com `DecididoEm`, `OrigemEvidencia`, `EvidenciaRef` e IP. Revogar é inserir linha negando |
| Acesso | endpoint que exporta tudo do titular em JSON |
| Correção | fluxo normal de edição, com trilha em `aud.AlteracaoCampo` |
| Eliminação | **anonimização real**: sobrescreve o dado pessoal e mantém o registro transacional (obrigação fiscal). Sem coluna `z_*`, sem cópia em lugar nenhum |
| Portabilidade | mesma exportação do acesso, em formato aberto |
| Registro de tratamento | `aud.EventoAcesso` com `LeituraDadoSensivel` |

**Minimização:** só integra do Vórtice o que a fase precisa. Não copiamos as 767 tabelas.

---

## 10. Auditoria

`[SF]` Field History Tracking: **20 campos por objeto, 18 meses**. Auditoria é **escopada**, não total.

`[V]` O Vórtice audita tudo e o resultado é: **44,8M de 85,5M linhas do banco (52,4%) são log**, das
quais **96,8% de `GE_LOG_PROCESSO` são re-carimbos** `(Atualizado em ...)` de um job de cobrança —
**617 linhas de log por processo**. E `GE_LgTb` tem 11,9M linhas **congeladas desde jun/2023**, nunca
expurgadas. Auditoria que ninguém consegue ler não é auditoria: é custo de disco.

| O que | Onde | Retenção |
|---|---|---|
| Alteração de campo (opt-in por campo) | `aud.AlteracaoCampo` | 18 meses online, arquivo depois |
| Login, falha de login, acesso negado | `aud.EventoAcesso` | 24 meses |
| Exportação de dados | `aud.EventoAcesso` | 24 meses |
| Mudança de permissão | `aud.AlteracaoCampo` sobre `seg.*` | **permanente** |
| Execução de regra | `wf.RegraExecucao` | 12 meses, partição mensal |

**Toda tabela de log tem política de expurgo desde o primeiro dia**, e o job de expurgo é escrito
**junto** com a tabela — nunca "depois".

---

## 11. Hardening

| Superfície | Controle |
|---|---|
| Injeção de SQL | EF Core parametrizado. `FromSqlRaw` **proibido** — teste de arquitetura no CI |
| Autorização quebrada | atributo `[RequerPermissao]` obrigatório; endpoint sem ele **falha o build** |
| Enumeração de recursos | a API só aceita `ChavePublica` (GUID). O `Id` sequencial nunca sai |
| Segredos | Azure Key Vault ou DPAPI. Nunca em `appsettings.json`. `[V]` token em texto claro no banco |
| Upload | valida magic bytes (não a extensão), limite de tamanho, antivírus, storage fora da webroot |
| Rate limit | por usuário e por IP, em toda a API |
| Cabeçalhos | HSTS, CSP, X-Content-Type-Options, sem `Server` |
| Dependências | Dependabot + `dotnet list package --vulnerable` no CI |
| Transporte | TLS 1.2+ obrigatório, inclusive na LAN |
| Backup | diário com **teste de restauração mensal**. `[V]` o terminal server do Vórtice está **sem backup** — a tarefa de System State está desabilitada |

---

## 11A. Segredos hoje — medido em 16/09/2026

> Issue #1 `[001]` do backlog mestre (doc 46A). A linha "Segredos" do §11 diz **para onde vamos**
> (Key Vault ou DPAPI). Esta seção diz **onde estamos**, com o comando que qualquer um repete.
> Nenhum valor aparece aqui — só nomes.

### Onde cada segredo mora

| Arquivo | No Git | Guarda | Nomes em |
|---|---|---|---|
| `.env` na raiz | não (`.gitignore`) | integrações: Protheus (REST e banco), ART, Entra ID, API Gestão de Negócios | `.env.exemplo` na raiz |
| `infra/.env` | não (`.gitignore`) | banco local do contêiner (`DB_*`) | `infra/.env.exemplo` |

No servidor, a aplicação lê as mesmas credenciais como variável de ambiente no formato de seção do
.NET (`Protheus__Senha`, `Entra__…`), nunca de `appsettings.json`. O que `publicar.ps1` ainda não
repassa — `TOTVS_API_*` — é a #18.

`git check-ignore` confirma que `.env`, `.env.*` (menos os `.env.exemplo`), `*.bak`, `publicacao/`
e `dados-locais/` continuam fora do Git.

### A chave da API Gestão de Negócios

- **Antes:** `API_TOKEN` — nome genérico, sem nenhum uso no código.
- **Agora:** `GESTAO_NEGOCIOS_API_URL` e `GESTAO_NEGOCIOS_API_TOKEN` no `.env` da raiz; no servidor,
  `GestaoDeNegocios__Base` e `GestaoDeNegocios__Chave`.
- **Ninguém usa a chave ainda.** O cliente HTTP é a #13, e só se escreve depois do contrato (#12) e
  da chave de cliente (#50). Até lá a chave fica guardada, sem chamada.

### A varredura

```powershell
./scripts/seguranca/varrer-segredos.ps1 -Relatorio dados-locais/varredura-segredos-AAAAMMDD.md
```

Lê os arquivos rastreados, os novos ainda não ignorados e **todo o histórico** (`git log --all -p`).
Procura **por padrão, nunca por valor**:

| Padrão | O que pega |
|---|---|
| `cadeia-de-conexao` | `Password=` e `Pwd=` com valor |
| `atribuicao-entre-aspas` | senha, password, secret, api key, token recebendo texto entre aspas |
| `variavel-de-ambiente` | `NOME_COM_SENHA/PASSWORD/SECRET/TOKEN/_KEY=valor` |
| `chave-privada` | bloco `BEGIN … PRIVATE KEY` |
| `token-do-github`, `jwt`, `segredo-de-aplicativo-entra`, `chave-aws` | formatos conhecidos |

Cada achado sai numa de três classes, e o valor sai sempre como `«oculto:N»` (N = tamanho):

- **REFERENCIA** — não é segredo: nome de variável, `***`/`REDIGIDO`, a própria definição de um padrão,
  ou texto curto demais para ser credencial.
- **TESTE** — valor inventado dentro de `tests/`, que existe para provar o mascaramento.
- **SUSPEITO** — o resto. **Um único suspeito faz o script sair com código 1.**

**Resultado de 16/09/2026** (HEAD `cb05b7c` + árvore de trabalho):

| Arquivos lidos | Commits | Achados | REFERENCIA | TESTE | SUSPEITO |
|---|---|---|---|---|---|
| 1460 | 18 | 82 | 76 | 6 | **0** |

**Prova de que o script detecta:** um arquivo temporário com oito segredos inventados, um por padrão,
deu oito SUSPEITO e código 1. O arquivo foi apagado em seguida.

**Limites:**

- É **por padrão**. Um segredo colado sem nome reconhecível ao lado passa.
- **Não lê arquivo ignorado** (`.env`, `.env.bak-*`), de propósito: quem roda a varredura não precisa
  ver valor nenhum.

### A mensagem de erro não carrega credencial

`Sigilo.Mascarar` (`Tracbel.Crm.Integracao/Sigilo.cs`) troca por `***` cada valor secreto informado
e todo trecho `Password=` ou `Pwd=` de cadeia de conexão. Antes ficava dentro da sincronização do ART; agora é compartilhado.

| Integração | O que sai na mensagem | Teste |
|---|---|---|
| ART | passa por `Sigilo.Mascarar` | `SincronizacaoDoArtTestes` (2) |
| Protheus REST | passa por `Sigilo.Mascarar` com usuário e senha, na autenticação e na leitura de página | `SigiloDaPonteDoProtheusTestes` (3) |
| Vórtice | texto genérico; o detalhe fica no log do servidor | sem teste de mascaramento; congelado (#20) |
| API Gestão de Negócios | não há cliente | nasce com a #13; teste na #41 |

Os testes do Protheus forçam o pior caso: uma exceção de rede que cita usuário, senha e
`password=` na URL. **Com o mascaramento desligado, dois dos três falham**: o teste foi rodado assim e o
código voltou. O terceiro impede que a senha volte para a URL do pedido de token.

### Pedido de rotação da chave da API Gestão de Negócios

**Situação:** redigido em 16/09/2026. **Quem envia:** o responsável pelo projeto, à equipe dona da
API (Inteligência de Mercado). **Envio:** data, canal e protocolo a preencher quando sair.

> **Assunto:** Rotação preventiva da chave da API Gestão de Negócios usada pelo CRM Tracbel (Visão 360)
>
> Pedimos a emissão de uma chave nova para o CRM Tracbel (Visão 360) e a revogação da chave atual.
>
> **Por quê:** a chave atual ficou guardada num arquivo local de configuração, com um nome genérico, antes
> de haver regra para ela. Uma varredura no repositório, incluindo todo o histórico, não achou a chave em
> arquivo versionado. A rotação é preventiva e acontece antes do primeiro uso.
>
> **O que pedimos:**
>
> 1. Uma chave nova, identificada como "CRM Tracbel — Visão 360", só com leitura.
> 2. Entrega por canal seguro, nunca em texto aberto por e-mail ou chat.
> 3. A data de revogação da chave atual.
> 4. A validade da chave nova e como se pede a próxima rotação.
> 5. Se as chamadas ficam registradas do lado da API, e por quanto tempo.
>
> O CRM ainda não chama a API. A chave nova fica guardada como variável de ambiente, fora do código e do
> Git, até o contrato de uso ser combinado.

### O que fica pendente

- **A Carga imprime a mensagem crua da exceção** no console de quem a roda. Risco baixo: só quem opera vê,
  na própria estação.
- **`.env.bak-20260909-174719` na raiz local.** Está fora do Git, mas pelo nome é uma cópia antiga do
  `.env`. Apagar depois da rotação.
- **Protheus em HTTP puro** (doc 28) e **`publicar.ps1` sem `TOTVS_API_*`** (#18).
- **Key Vault ou DPAPI** (§11) continua sendo o alvo. Hoje, o segredo fica em variável de ambiente.

---

## 12. A matriz de teste — o portão de segurança da fase

O requisito é 100% testado. Para segurança, isso significa a matriz completa:

```csharp
/// <summary>
/// Matriz de autorização: cada conjunto × cada entidade × cada verbo.
/// Nenhuma combinação fica sem teste. Se alguém acrescentar uma permissão e esquecer o
/// caso de teste, o teste de completude (abaixo) falha.
/// </summary>
[Theory]
[MemberData(nameof(MatrizCompleta))]
public async Task Autorizacao_respeita_conjunto_e_profundidade(
    string conjunto, string entidade, string verbo, Profundidade profundidade, bool esperaPermitir)
{
    var usuario = await _fixture.CriarUsuarioComAsync(conjunto);
    var registro = await _fixture.CriarRegistroAsync(entidade, proprietario: OutroUsuario);

    var permitido = await _autorizador.PodeAsync(usuario, entidade, verbo, registro.Id);

    permitido.Should().Be(esperaPermitir,
        "{0} com {1} deveria {2} poder {3} em {4} de outro usuário",
        conjunto, profundidade, esperaPermitir ? "" : "não", verbo, entidade);
}

/// <summary>Nenhuma permissão pode existir sem caso de teste.</summary>
[Fact]
public async Task Toda_permissao_cadastrada_tem_caso_na_matriz()
{
    var noBanco = await _db.Permissoes.Select(p => p.Codigo).ToListAsync();
    var naMatriz = MatrizCompleta().Select(d => $"{d[1]}.{d[2]}").Distinct();

    noBanco.Except(naMatriz).Should().BeEmpty("toda permissão precisa de teste");
}
```

**Complementos obrigatórios do portão:**

- **Teste de escape:** para cada entidade, um teste que tenta ler registro fora do escopo por
  API, relatório, exportação e busca. Todos devem devolver vazio — não erro.
- **Revisão de dependências** sem vulnerabilidade alta ou crítica.
- **Teste de restauração de backup** executado e documentado.

---

## 13. O que decidimos NÃO fazer

| Descartado | Por quê |
|---|---|
| Regra de negação (`deny`) | torna a ordem de avaliação imprevisível; `[DYN]` não tem, de propósito |
| Territory Management `[SF]` | potência alta, complexidade altíssima; carteira já resolve |
| Access Teams auto-criadas `[DYN]` | limite de 4 por tabela e custo escondido de linhas materializadas |
| Muting Permission Sets `[SF]` | a exceção subtrativa mais mal-entendida da plataforma |
| Sharing linha-a-linha para usuário individual em massa | `[DYN]` `PrincipalObjectAccess` é onde o compartilhamento vira dívida técnica |
| Position Hierarchy `[DYN]` | Manager hierarchy resolve; a segunda é redundante |
| Criptografia de coluna no banco | `[SF]` Shield **não é controle de acesso** e dificulta consulta. TDE cobre o disco |
| Janela de horário de acesso | `[V]` existe no Vórtice e tem **3 linhas**. Ninguém usa |
| Permissão por widget de tela | `[V]` existe e tem **17 linhas**, amarrada ao nome físico do controle Gupta |

---

## 14. Resumo

| Camada | Mecanismo | Inspiração | Fase |
|---|---|---|---|
| 1 Autenticação | Entra ID + MFA | — | 1 |
| 2 Entidade+verbo | `ConjuntoPermissao` aditivo | `[SF]` permission sets | 1 |
| 3 Profundidade | 6 níveis por permissão | `[DYN]` privilege × depth | 1 |
| 4 Compartilhamento | share com motivo tipado | `[SF]` RowCause | 1 |
| 5 Campo sensível | omissão do campo | `[DYN]` column security, sem o NULL silencioso | 2 |
| — Auditoria | escopada, com retenção | `[SF]` field history | 1 |
| — LGPD | consentimento com prova | correção do `[V]` | 1 |

**Cinco camadas, contra oito do Vórtice — e as cinco funcionam.**
