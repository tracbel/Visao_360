# Auditoria independente de estrutura e nomenclatura do banco

> Documento 21 · 04/09/2026 · **Revisão externa.** Quem escreve não participou da construção do
> modelo nem da migração de 04/09/2026, e não alterou código nem documento nesta rodada.
> Verdade conferida, na ordem de precedência declarada: [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md),
> [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md) (vence sobre o 15),
> [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), [20-DECISAO-SQL-SERVER](20-DECISAO-SQL-SERVER.md).
> Estado real conferido em `docs/banco/catalogo.csv`, `docs/banco/esquema.json`,
> `src/Tracbel.Crm.Infraestrutura/Persistencia/`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/` e
> **no banco vivo do contêiner** (`TracbelCrm`, somente leitura).

---

## Veredito, em cinco linhas

1. **A conversão PostgreSQL para SQL Server não perdeu estrutura.** 63 tabelas, 745 colunas, 166
   chaves estrangeiras, 306 índices, 114 restrições de verificação, colação `Latin1_General_CI_AI`,
   as 4 tabelas particionadas com **todos** os índices alinhados. Nenhuma tabela sem chave primária,
   nenhuma FK sem índice, nenhum tipo proibido, todo dinheiro em `decimal(18,2)`.
2. **A nomenclatura está boa, e o medo do Vórtice não se repetiu.** Zero abreviação da lista
   proibida, zero `snake_case`, zero prefixo `IV_`/`GE_`/`EXT_`, zero token de backup, e **nenhuma
   das 63 é cópia disfarçada de outra** — a varredura por assinatura de colunas devolveu zero grupos
   idênticos. Há **uma** tabela de cliente.
3. **Mas há três defeitos estruturais que teste nenhum pega**, os três do mesmo tipo: o documento
   promete que **o banco garante**, e o banco não garante — a chave estrangeira composta do catálogo,
   o domínio fechado da coluna `Entidade`, e a colação em produção.
4. **E há um teste que passa pelo motivo errado, com consequência direta:** `EmpresaId` existe em 21
   tabelas e o filtro global de segurança está em **uma**. É a frase do doc 14, seção 5.2 — *"coluna
   que existe mas não é usada como fronteira não é fronteira nenhuma"* — agora do nosso lado.
5. **Não aprovo como está.** Nada aqui exige refazer o modelo: são correções cirúrgicas, quase todas
   de uma migração aditiva mais um teste. O modelo em si é bom e entrega o que se propôs.

### Contagem por gravidade

| Gravidade | Quantos |
|---|---:|
| **Impede** | **3** |
| **Atrapalha** | **19** |
| **Cosmético** | **8** |
| **Total** | **30** |

---

## 1. Impede

### I-1 · A chave estrangeira composta do catálogo foi prometida, metade dela foi construída, e a metade que restringe não existe

**Gravidade:** impede.

**Onde exatamente:** `metadado.CatalogoItem` e as **9 colunas de papel** que apontam para ela —
`comercial.ClienteContato.PapelId`, `comercial.Cliente.OrigemId`,
`comercial.Cliente.MotivoInativacaoId`, `comercial.Lead.OrigemId`, `comercial.Lead.MotivoDescarteId`,
`comercial.Endereco.CulturaId`, `documento.Documento.TipoDocumentoId`,
`processo.ItemDeProposta.CondicaoPagamentoId`, `processo.Processo.ConcorrenteId`. Definições em
`src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs` e nas
configurações de cada tabela referenciadora.

**Regra violada:** [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seção 8.11 — *"A integridade
referencial se mantém com `UNIQUE (CatalogoId, Id)` em `CatalogoItem` e **FK composta** na tabela que
aponta, com a coluna `CatalogoId` persistida como constante — assim `Contato.PapelId` só aceita item
do catálogo `PAPEL_CONTATO`, **verificado pelo banco**."*

**O que está errado.** A chave alternativa existe: `AK_CatalogoItem_CatalogoId (CatalogoId, Id)`,
confirmada em `sys.indexes` no banco vivo. **Nenhuma das 11 chaves estrangeiras que apontam para
`CatalogoItem` a usa.** Todas são de coluna única para `CatalogoItem.Id`, e nenhuma tabela
referenciadora tem a coluna `CatalogoId` persistida como constante. Conferido no catálogo do banco:

```
comercial.ClienteContato | FK_ClienteContato_CatalogoItem_PapelId    | PapelId         -> Id
comercial.Endereco       | FK_Endereco_CatalogoItem_CulturaId        | CulturaId       -> Id
documento.Documento      | FK_Documento_CatalogoItem_TipoDocumentoId | TipoDocumentoId -> Id
...  (11 no total, todas de coluna unica)
```

Na prática: `ClienteContato.PapelId` aceita hoje um item do catálogo `CULTURA`;
`Endereco.CulturaId` aceita um `CONCORRENTE`; `Documento.TipoDocumentoId` aceita um `ORIGEM_LEAD`.
Seis dos vinte catálogos de `dados-referencia/` moram nessa tabela compartilhada — é onde o risco se
concretiza primeiro. E a existência da `AK_` faz o mecanismo **parecer** implementado para quem lê o
catálogo do banco, o que é pior do que não existir.

**Por que é grave.** Foi exatamente para isto que as três tabelas do doc 04 (`PapelContato`,
`OrigemLead`, `TipoDocumento`) foram removidas em favor de `CatalogoItem` (doc 17, seção 9.3). A
troca só se paga se a restrição de catálogo for do banco. Sem ela, trocamos três domínios fechados
por um domínio aberto — o mesmo movimento que produziu FINALIZADO/FINALIZADA, um nível acima.

**Correção sugerida:** trocar cada uma das 9 chaves estrangeiras de papel por FK composta
`(CatalogoIdDoPapel, XId)` contra `AK_CatalogoItem_CatalogoId`, com `CatalogoIdDoPapel` como coluna
persistida de valor constante, e acrescentar um teste que exija FK composta para toda coluna que
aponte para `CatalogoItem`.

---

### I-2 · O `ALTER DATABASE ... COLLATE` — o único mecanismo que importa em produção — nunca foi exercido por teste nenhum

**Gravidade:** impede.

**Onde exatamente:** `src/Tracbel.Crm.Infraestrutura/Migrations/ModeloInicial.ColacaoDoBanco.cs`,
linhas 36 a 49; `infra/docker-compose.yml` linha 50 (`MSSQL_COLLATION`); teste
`tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs:175`
(`A_restricao_de_dominio_recusa_o_valor_invalido_no_banco`).

**Regra violada:** [20-DECISAO-SQL-SERVER](20-DECISAO-SQL-SERVER.md), seções 4.3 e 4.4 (a colação
substitui a coluna derivada, e *"é este que importa em produção"*), e
[14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), seções 14 e 15 (*"regra sem teste é recomendação, não
padrão"*).

**O que está errado.** Duas coisas, que se somam:

1. O bloco da migração é guardado por um `IF` que compara a colação corrente com
   `Latin1_General_CI_AI`. No contêiner de desenvolvimento, `MSSQL_COLLATION` fixa a colação do
   **servidor** e, por consequência, do `model` em `Latin1_General_CI_AI` — conferido no banco vivo:
   `SERVERPROPERTY('Collation')` e `DATABASEPROPERTYEX('model','Collation')` devolvem os dois
   `Latin1_General_CI_AI`. Como o banco de teste `TracbelCrmMigracaoTeste` nasce do `model`, **a
   condição do `IF` é sempre falsa e o `ALTER` nunca executa.** O bloco pode estar quebrado e todos
   os testes continuam verdes.
2. O teste que deveria provar o comportamento grava `Soja` e depois `SOJA` e espera violação de
   unicidade. Isso exercita **caixa**, não **acento** — e a colação do Vórtice
   (`SQL_Latin1_General_CP1_CI_AS`) também é case-insensitive, logo recusaria os dois igualmente. A
   propriedade que a decisão 20, seção 4.3, introduziu — *"Jose" encontra "José"* — **não é
   exercitada em lugar nenhum do conjunto de testes.**

Em produção o banco nasce dentro da instância do Vórtice, cuja colação padrão é accent-sensitive. Se
o bloco falhar, todas as colunas nascem com a colação errada, a busca por nome volta a se comportar
como no Vórtice, e `UX_CatalogoItem_Catalogo_Descricao` deixa de impedir `Preço` ao lado de `Preco`.
Nada avisa.

**O que está certo e vale registrar:** a **ordem** está correta — `DefinirColacaoDoBanco` é chamado
na linha 16 de `20260904120040_ModeloInicial.cs`, antes do primeiro `CreateTable`, na linha 48, de
modo que as colunas herdam a colação já corrigida.

**Correção sugerida:** um teste que crie o banco a partir de um `model` com colação accent-sensitive,
aplique a migração e afirme que `DATABASEPROPERTYEX(DB_NAME(),'Collation')` é
`Latin1_General_CI_AI`, mais um caso `Soja`/`Sója` no teste de domínio.

---
### I-3 · `Entidade` é texto livre em 10 colunas, sem domínio fechado e sem chave estrangeira

**Gravidade:** impede.

**Onde exatamente** (conferido no banco vivo — nenhuma tem restrição que enumere valores):

| Coluna | Tipo | Papel |
|---|---|---|
| `documento.Vinculo.Entidade` | `varchar(40) NOT NULL` | a que registro o documento está preso |
| `seguranca.CompartilhamentoDeRegistro.Entidade` | `varchar(40) NOT NULL` | a que registro o acesso foi concedido |
| `auditoria.CampoAuditado.Entidade` | `varchar(40) NOT NULL` | **o que se audita** |
| `auditoria.AlteracaoDeCampo.Entidade` | `varchar(40) NOT NULL` | o que foi alterado |
| `auditoria.EventoDeAcesso.Entidade` | `varchar(40) NULL` | o que foi visto |
| `integracao.ChaveExterna.Entidade` | `varchar(40) NOT NULL` | o de-para com o ERP |
| `integracao.Recepcao.Entidade` | `varchar(40) NOT NULL` | a área de pouso, particionada por esta coluna |
| `metadado.CampoPersonalizado.Entidade` | `varchar(40) NOT NULL` | onde o campo extra aparece |
| `seguranca.Permissao.Entidade` | `varchar(40) NOT NULL` | verbo vezes entidade |
| `relatorio.Fonte.EntidadeRaiz` | `varchar(40) NOT NULL` | a raiz da fonte curada |

A única restrição que cita `[Entidade]` em todo o banco é `CK_EventoDeAcesso_Registro`, e ela só
verifica co-nulabilidade com `RegistroId`, não o domínio. Em cinco dessas tabelas o par
`(Entidade, RegistroId)` é um ponteiro polimórfico e `RegistroId` **não tem FK** — inevitável no
desenho, o que torna o domínio de `Entidade` a única defesa que sobra. O mesmo vale para a coluna
`Campo` em `auditoria.AlteracaoDeCampo`, `auditoria.CampoAuditado`, `metadado.CampoPersonalizado` e
`relatorio.FonteCampo`.

**Regra violada:** [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seção 6.2, regra 2 (*"Todo domínio
fechado curto tem `CHECK`"*) e [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), seção 6, última linha
(*"Nenhuma coluna de texto livre onde existe (ou deveria existir) um catálogo"*).

**Por que é grave, com nome e sobrenome.** Três consequências concretas, cada uma citada pelos
próprios documentos como o defeito a não repetir:

- `documento.Vinculo` existe para corrigir os **5.302 documentos órfãos** do Vórtice (doc 17, seção
  8.6). Com `Entidade` livre e `RegistroId` sem FK, a mesma classe de órfão é estruturalmente
  reproduzível — só que com nomes melhores.
- `seguranca.CompartilhamentoDeRegistro` é *"a peça que falta no Vórtice"* (doc 17, seção 8.2). Ela
  concede acesso a uma **string**: `Cliente` e `Clientes` são registros diferentes para o banco, e
  renomear uma entidade orfana todos os compartilhamentos, em silêncio.
- `auditoria.CampoAuditado` é o que impede os 39 milhões de linhas de log (doc 17, seção 8.7): ela
  **decide o que se audita**, nomeando `(Entidade, Campo)` como texto. Um nome errado desliga a
  auditoria daquele campo sem produzir erro nenhum.

E, ao contrário do que o doc 14, seção 6, supõe ao marcar esta regra como *"Recomendação — não é
mecanicamente detectável"*, **para `Entidade` ela é detectável**: o conjunto de valores válidos é
exatamente o conjunto de entidades mapeadas no `CrmDbContext`, conhecido em tempo de teste.

**Correção sugerida:** gerar, a partir do próprio modelo, um `CHECK` enumerando os nomes de entidade
em cada uma das 10 colunas, e um teste que exija esse `CHECK` em toda coluna chamada
`Entidade`/`EntidadeRaiz`.

---

## 2. Atrapalha

### A-1 · `EmpresaId` está em 21 tabelas; o filtro que a torna fronteira está em uma

**Gravidade:** atrapalha.
**Onde:** `src/Tracbel.Crm.Infraestrutura/Persistencia/CrmDbContext.cs:345` — única ocorrência de
`HasQueryFilter` em todo o `src/`, sobre `Lead`. Teste
`tests/Tracbel.Crm.Arquitetura.Testes/Banco/AuditoriaTestes.cs:124`
(`Toda_tabela_transacional_declara_a_coluna_de_multiempresa`).
**Regra violada:** [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), seção 11.3.

As 21 tabelas com `EmpresaId` estão impecáveis do lado da coluna: todas `NOT NULL`, todas com FK para
`organizacao.Empresa`, todas indexadas (conferido no banco vivo). Mas 20 das 21 não têm o filtro
global. O teste afirma que a coluna existe e **não afirma que ela é usada** — e o próprio doc 14,
seção 5.2, escreve o critério que o teste não aplica: *"Uma coluna que existe mas não é usada como
fronteira de fato não é fronteira nenhuma."* É o exemplo mais claro de teste que passa pelo motivo
errado no conjunto: ele protege a metade barata da regra. O documento é honesto sobre a lacuna, mas
ela aparece numa seção cuja regra irmã está marcada **[Testado]** — quem lê a tabela conclui que a
multiempresa está coberta.

**Correção sugerida:** teste que exija `HasQueryFilter` sobre `EmpresaId` em toda entidade que declare
`EmpresaId`, com lista de exceções nomeadas.

---

### A-2 · Só 16 das 63 tabelas carregam o bloco de auditoria; 10 carregam um pedaço; nada define quais deveriam

**Gravidade:** atrapalha.
**Onde:** conferido em `docs/banco/catalogo.csv`. Bloco completo (7 colunas mais `Versao`): **16**.
Bloco **parcial**: **10** — `auditoria.AlteracaoDeCampo`, `comercial.CanalContato`,
`comercial.ConsentimentoComunicacao`, `frota.LeituraDeHorimetro`, `integracao.MensagemDeSaida`,
`metadado.Resposta`, `organizacao.Empresa`, `processo.Interacao`, `processo.Regra`,
`processo.TipoProcesso`. Sem bloco nenhum: **37**.
**Regra violada:** [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), seções 5.1 e 16 (*"A entidade nova
herda `EntidadeBase` (transacional) ou tem justificativa explícita para não herdar?"* — item de
revisão de PR, sem teste).

`AuditoriaTestes` só olha quem herda `EntidadeBase`. Uma tabela escapa das **quatro** regras da seção
5 — bloco de auditoria, concorrência otimista, autoria por ID e multiempresa — simplesmente **não
herdando**. Não existe teste que diga quais tabelas devem ser transacionais, e a decisão é invisível
na revisão porque não existe como lista em lugar nenhum. Casos que chamam atenção:
`organizacao.Empresa` tem `ChavePublica`, `CriadoEm` e `AlteradoEm` mas não
`CriadoPorId`/`AlteradoPorId` — quem criou uma filial não fica registrado; `processo.Regra` tem
quatro das sete e não tem `ExcluidoEm` nem `Versao`.

**Correção sugerida:** lista explícita, versionada e testada, de quais tabelas são transacionais e
quais são catálogo, com a justificativa de cada não-transacional — o mesmo mecanismo de
`CascadeJustificado`.

---

### A-3 · `comercial.ClienteCarteira` — a carteirização — não tem `EmpresaId`, `ChavePublica`, `ExcluidoEm` nem `Versao`

**Gravidade:** atrapalha.
**Onde:** `comercial.ClienteCarteira`, 10 colunas: `Id`, `ClienteId`, `CarteiraId`, `Classe`,
`DiasCicloContato`, `PotencialAnual`, `UltimaInteracaoEm`, `VinculadoEm`, `VinculadoPorId`,
`DesvinculadoEm`.
**Regra violada:** [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), seção 5.2, e o motivo escrito no
comentário do próprio teste: *"`EmpresaId` precisa estar na **PRÓPRIA LINHA**, para que a checagem de
acesso seja uma comparação de coluna, sem join."*

É a tabela que decide **qual CEN vê qual cliente** — descrita no doc 17, seção 8.3, como *"o melhor
ativo do legado"*. A empresa só é alcançável por join com `organizacao.Carteira`. O trio próprio
(`VinculadoEm`, `VinculadoPorId`, `DesvinculadoEm`) resolve o histórico do vínculo, mas **mudar a
classe A para C ou o ciclo de contato de um cliente não deixa rastro nenhum** — não há
`AlteradoEm`/`AlteradoPorId` — e, sem `Versao`, duas gravações simultâneas sobrescrevem-se em
silêncio.

**Correção sugerida:** acrescentar `EmpresaId NOT NULL` com FK e índice, e `Versao`; ou registrar a
exceção e o porquê na seção 5 do doc 14.

---

### A-4 · `processo.Regra` não tem coluna de empresa — e "a empresa é condição, não cópia" é a correção central do projeto

**Gravidade:** atrapalha.
**Onde:** `processo.Regra`, 22 colunas, nenhuma de empresa.
**Regra violada:** [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seção 8.4 — *"No Vórtice a mesma
regra é replicada para cada uma das 18 empresas: 1.429 globais mais cerca de 4.500 duplicadas. **Aqui
a empresa é condição, não cópia.**"*

Não há `EmpresaId` nem qualquer coluna de empresa. A "condição" só pode morar em
`Condicao nvarchar(2000)`, que é **anulável** e é texto de expressão. Ou seja: a correção mais
celebrada do modelo não tem coluna, não tem restrição e não tem teste — o banco não sabe distinguir
regra global de regra de filial, e a duplicação por empresa que o Vórtice faz por cópia de linha
continua possível aqui por cópia de linha com `Condicao` diferente.

**Correção sugerida:** `EmpresaId int NULL` (nulo igual a global) com FK e índice, mais um teste que
exija o uso da coluna em vez da expressão.

---
### A-5 · Dois nomes para a mesma referência: `LinhaNegocioId` contra `LinhaDeNegocioId`

**Gravidade:** atrapalha.
**Onde:** `comercial.Lead.LinhaNegocioId` contra `organizacao.Carteira.LinhaDeNegocioId`,
`organizacao.Meta.LinhaDeNegocioId`, `organizacao.Praca.LinhaDeNegocioId` e
`processo.TipoProcesso.LinhaDeNegocioId`. As cinco apontam para `organizacao.LinhaDeNegocio`.
**Regra violada:** [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md), seção 4 (`org.LinhaNegocio` vira
`organizacao.LinhaDeNegocio`, motivo declarado: *"palavra inteira"*) e seção 6 (*"é o que impede o
banco de ganhar três nomes para a mesma coisa"*).

**Agravante, e é o que faz este item não ser cosmético:** a heurística de
`IntegridadeReferencialTestes.Toda_referencia_a_entidade_mapeada_tem_foreign_key_declarada` só
reconhece a propriedade quando o nome bate **exatamente** com `<Entidade>Id`. `LinhaNegocioId` não
bate com a entidade `LinhaDeNegocio` — **a coluna é invisível para o teste**. Ela tem FK hoje; se
perdê-la, nada acusa.

**Correção sugerida:** renomear para `LinhaDeNegocioId` em `comercial.Lead`.

---

### A-6 · `ConjuntoPermissaoId` não bate com a entidade `ConjuntoDePermissao` — mesmo defeito, mesmo ponto cego

**Gravidade:** atrapalha.
**Onde:** `seguranca.ConjuntoDePermissaoItem.ConjuntoPermissaoId` e
`seguranca.UsuarioConjuntoDePermissao.ConjuntoPermissaoId`; o nome aparece também dentro da restrição
registrada em `tests/Tracbel.Crm.Arquitetura.Testes/Banco/IntegridadeReferencialTestes.cs:26`
(`FK_ConjuntoDePermissaoItem_ConjuntoDePermissao_ConjuntoPermissaoId`).
**Regra violada:** [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md), seção 4 (`seg.ConjuntoPermissao`
vira `seguranca.ConjuntoDePermissao`, motivo declarado: *"leitura"*), reafirmada pelo
[17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seção 9.1.

A tabela foi renomeada, a coluna não. Mesmo ponto cego do A-5: a heurística de FK não alcança nenhuma
das duas colunas.

**Correção sugerida:** renomear as duas colunas para `ConjuntoDePermissaoId`.

---

### A-7 · `processo.TipoProcesso.Versao int` colide com a coluna reservada de concorrência, e o mesmo conceito tem quatro nomes

**Gravidade:** atrapalha.
**Onde:**

| Coluna | Tipo | O que é |
|---|---|---|
| `Versao`, em 17 tabelas | `rowversion` | concorrência otimista (doc 14, seção 5.1) |
| `processo.TipoProcesso.Versao` | `int NOT NULL`, com `CK_TipoProcesso_Versao` | **número de versão de negócio** |
| `documento.Documento.NumeroVersao` | `int NOT NULL` | número de versão de negócio |
| `metadado.Formulario.VersaoPublicada` | `int NOT NULL` | idem |
| `metadado.Preenchimento.VersaoFormulario` | `int NOT NULL` | idem |

**Regra violada:** [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), seção 5.1 (`Versao` é a coluna
`rowversion` do bloco fixo) e [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md), seção 6.

`Versao` é palavra reservada do bloco de auditoria e está sendo usada para outra coisa numa tabela.
Funciona hoje porque `TipoProcesso` não herda `EntidadeBase`; no dia em que herdar, o choque é uma
migração destrutiva. E o mesmo conceito de negócio aparece com quatro grafias — o defeito do doc 15,
seção 6, em miniatura, com `documento.Documento` exibindo o nome certo ao lado do errado.

**Correção sugerida:** renomear `TipoProcesso.Versao` para `NumeroVersao` e padronizar as outras três
na mesma palavra.

---

### A-8 · CPF/CNPJ chama-se `Documento`, e `Documento` já é o arquivo anexado — e `Empresa` chama a mesma coisa de `Cnpj`

**Gravidade:** atrapalha.
**Onde:** `comercial.Cliente.Documento varchar(14)`, `comercial.Contato.Documento varchar(14)`,
`comercial.Lead.Documento varchar(14)`, contra `organizacao.Empresa.Cnpj`, contra a tabela
`documento.Documento` e o schema `documento`.
**Regra violada:** [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md), seção 3.3 — *"**Documento**:
arquivo anexado a um processo, `documento.Documento`"* — e seção 6 (um termo, um significado). O
[17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seção 5.2, chama o campo de **`CpfCnpj`**, e existe o
tipo de valor `Tracbel.Crm.Dominio.Comum.CpfCnpj` no domínio.

Um analista comercial que abrir o banco encontra `Cliente.Documento` e a tabela `documento.Documento`
significando coisas diferentes, e `Empresa.Cnpj` significando o mesmo que `Cliente.Documento`. É o
teste do doc 15, seção 1 — *"se um analista comercial não entende o nome, o nome está errado"* —
falhando por ambiguidade, não por abreviação.

**Correção sugerida:** renomear as três para `CpfCnpj`, e `Empresa.Cnpj` também.

---

### A-9 · `comercial.CanalContato.ValorNormalizado` é a segunda coluna derivada — e o doc 17 diz que só existe uma

**Gravidade:** atrapalha.
**Onde:** `comercial.CanalContato.ValorNormalizado varchar(200) NOT NULL`, com
`IX_CanalContato_ValorNormalizado` filtrado. Conferido no banco vivo: **não é coluna computada** —
`sys.computed_columns` está vazia no banco inteiro —, logo é mantida pela aplicação.
**Regra violada:** [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seção 9.5 —
*"`comercial.ClienteCarteira`: acrescentar `UltimaInteracaoEm` denormalizado, com job de
reconciliação — **única exceção consciente à regra 4**"* — e a regra 5 (*"Índice é índice"*), a mesma
pela qual `Cliente.NomeNormalizado` foi eliminada no doc 20, seção 4.3.

São duas colunas derivadas, não uma. A de `CanalContato` não está documentada em lugar nenhum, não
tem coluna computada que a garanta e não tem job de reconciliação declarado como o `UltimaInteracaoEm`
tem. Ela pode ser legítima — a colação `CI_AI` resolve caixa e acento mas **não** remove pontuação de
telefone, que é provavelmente o que a justifica. Isso precisa estar escrito, e não está.

**Correção sugerida:** transformar em coluna computada persistida, ou registrar a exceção na seção
9.5 do doc 17 com o motivo.

---

### A-10 · O catálogo de referência `categoria_de_interacao` não cabe no `CHECK` que deveria recebê-lo

**Gravidade:** atrapalha.
**Onde:** `dados-referencia/categoria_de_interacao.json` (6 itens: `VISITA`, `LIGACAO`, `WHATSAPP`,
`EMAIL`, `REMOTA`, **`FEIRA_EVENTO`**) contra `CK_TipoTarefa_Categoria`, que aceita `Visita`,
`Ligacao`, `WhatsApp`, `Email`, `Remota` e `Interna`.
**Regra violada:** [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seções 8.4 e 9.5
(`TipoTarefa.Categoria` absorve a categoria de interação do protótipo).

`FEIRA_EVENTO` **não pode ser gravado** — o banco recusa. E `Interna` existe no `CHECK` sem item
correspondente no catálogo. O próprio arquivo de semente registra a origem:
*"config-categorias-interacao-cfg.json (6 itens, tela Configurações, Taxonomias)"*.

**Correção sugerida:** acrescentar `FeiraEvento` ao enum e ao `CHECK`, ou registrar em
`dados-referencia/PENDENTES.md` a decisão de não suportá-lo.

---

### A-11 · `Encerrada` no documento e na semente, `Encerrado` no banco

**Gravidade:** atrapalha.
**Onde:** `CK_Cliente_Situacao` aceita `Encerrado`; [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md),
seção 3, regra 1, escreve *"(`Suspect`, `Prospect`, `Cliente`, `ClienteInativo`, **`Encerrada`**)"*;
`dados-referencia/situacao_do_cliente.json` traz `ENCERRADA`.
**Regra violada:** doc 17, seção 3, regra 1 (documento posterior, vence por precedência declarada).

É, literalmente, FINALIZADO/FINALIZADA — a divergência de gênero que este programa inteiro cita como o
defeito emblemático do Vórtice — entre o documento normativo, a semente e o código. Ninguém percebeu
porque nenhum teste compara o `CHECK` com o catálogo de referência.

**Correção sugerida:** teste que compare, para cada catálogo de `dados-referencia/` cujo destino é um
`CHECK`, os valores do `CHECK` com os códigos do arquivo.

---

### A-12 · O portão de tabela nova não existe: `docs/decisoes/` não foi criado

**Gravidade:** atrapalha.
**Onde:** `docs/decisoes/` — diretório ausente no repositório.
**Regra violada:** [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md), seção 10.2 — *"Toda tabela nova
exige decisão registrada. O registro fica em `docs/decisoes/` (ADR), e o pull request que cria a
migration referencia o ADR ou não passa."*

As 63 tabelas nasceram sem ADR nenhum, e o mecanismo que deve impedir a 64ª de nascer por conveniência
não está montado. A única barreira mecânica hoje é a contagem fixa (ver A-13), que é fácil de
atualizar e não pergunta nada.

**Correção sugerida:** criar `docs/decisoes/` com os ADRs retroativos das tabelas novas do doc 17,
seção 9.4, e verificação no pipeline que exija referência a ADR em PR que altere o número de tabelas.

---

### A-13 · `Os_dez_schemas...somam_sessenta_e_tres_tabelas` verifica uma lista fixa, não a regra

**Gravidade:** atrapalha.
**Onde:** `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs`, linhas 111 a 139.
**Regra que deveria verificar:** doc 14, seção 15.1 — *"o **portão** de 63 tabelas em 10 schemas"*.

O teste compara um dicionário literal de contagens. Não é um portão: é um contador. Passa se alguém
**trocar** uma tabela por outra dentro do mesmo schema, e a forma de adicionar uma tabela é editar o
literal — a mesma linha de código, sem nenhuma pergunta. O portão real (as 7 perguntas do ADR, seção
10.2) não tem verificação nenhuma.

**Correção sugerida:** manter a contagem e acrescentar a lista **nominal** das 63 tabelas esperadas,
para que qualquer troca também falhe.

---

### A-14 · Dois testes casam nome de coluna por substring e podem passar por citação de outra coluna

**Gravidade:** atrapalha.
**Onde:** `IntegridadeReferencialTestes.cs:155`
(`Coluna_baseada_em_enum_tem_check_constraint_de_dominio`) e `TiposDeColunaTestes.cs:373`
(`Coluna_de_caractere_fixo_unico_tem_check_constraint`). Ambos usam
`checks.Any(sql => sql.Contains(coluna, StringComparison.OrdinalIgnoreCase))`.

A verificação é *"o nome desta coluna aparece em algum lugar de alguma restrição desta tabela"*, não
*"existe uma restrição sobre esta coluna"*. Uma coluna `Situacao` seria considerada coberta por um
`CHECK` sobre `SituacaoDesde`; uma `Tipo`, por um sobre `TipoConteudo`; uma `Origem`, por um sobre
`OrigemAtribuicao` — e esses pares de nomes **já existem no modelo**, em tabelas diferentes. Hoje
nenhum caso real passa indevidamente; o mecanismo é que está frouxo. O teste irmão
`Toda_coluna_de_json_tem_check_de_json_valido` (`TiposDeColunaTestes.cs:274`) faz do jeito certo —
procura o nome da coluna entre colchetes.

**Correção sugerida:** casar por `"[" + coluna + "]"`, como o teste de JSON já faz.

---

### A-15 · A regra de abreviação proibida é verificada em coluna, nunca em nome de tabela

**Gravidade:** atrapalha.
**Onde:** `EsquemaENomenclaturaTestes.cs`, linhas 238 a 273 — o laço percorre `tabela.GetProperties()`
e nunca examina `tabela.GetTableName()`.
**Regra violada:** [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md), seção 5 — *"**Nome de tabela e de
coluna** usa a palavra inteira"*. O doc 14, seção 15.1, descreve o teste sem restringir a colunas.

Nenhum nome de tabela viola hoje. Mas `TipoProc`, `MotivoPerda` ou `CadCli` passariam sem alarme, e é
metade da regra que o glossário existe para proteger.

**Correção sugerida:** incluir o nome da tabela no mesmo laço.

---
### A-16 · Domínios fechados curtos gravados como texto livre, sem `CHECK` — inclusive o mesmo conceito em três tabelas

**Gravidade:** atrapalha.
**Onde** (conferido no banco vivo — nenhuma restrição cita a coluna):

| Coluna | Tipo | Observação |
|---|---|---|
| `processo.Regra.Evento` | `varchar(40) NOT NULL` | mesmo conceito, três tabelas, |
| `processo.RegraExecucao.Evento` | `varchar(40) NOT NULL` | três domínios abertos e independentes, |
| `metadado.TratadorDeEvento.Evento` | `varchar(60) NOT NULL` | e um deles com tamanho diferente |
| `integracao.Sistema.MeioDeAcesso` | `varchar(40) NOT NULL` | |
| `integracao.MensagemDeSaida.Tipo` | `varchar(60) NOT NULL` | |
| `integracao.MensagemDescartada.Fluxo` | `varchar(60) NOT NULL` | par com a de baixo, sem domínio comum |
| `integracao.PontoDeSincronismo.Fluxo` | `varchar(60) NOT NULL` | |
| `frota.LeituraDeHorimetro.Fonte` | `varchar NOT NULL` | quem declarou o horímetro |
| `comercial.ConsentimentoComunicacao.OrigemEvidencia` | `varchar NOT NULL` | prova de consentimento LGPD |

Na mesma tabela `processo.Regra`, `Efeito` **tem** `CHECK` e `Evento` não — a inconsistência está a
uma linha de código de distância.

**Regra violada:** doc 17, seção 6.2, regra 2; doc 14, seção 6, última linha.

`Regra.Evento` e `RegraExecucao.Evento` precisam concordar para que o log de execução se relacione com
a regra que o gerou — o mecanismo que o doc 17, seção 8.4, descreve como *"o log que torna o silêncio
impossível"*. Dois domínios abertos que precisam concordar é a definição de FINALIZADO/FINALIZADA.

**Correção sugerida:** um `enum` do C# por domínio (que já traz o `CHECK` pela regra testada da seção
6), compartilhado entre as tabelas que falam do mesmo conceito.

---

### A-17 · A tela aprovada mostra campos que não têm coluna

**Gravidade:** atrapalha.
**Onde:** `prototipo/dados-seed/cliente-84391.json` contra `comercial.Cliente`,
`comercial.CanalContato` e `comercial.ConsentimentoComunicacao`.

| Campo da tela | Onde deveria estar | Situação |
|---|---|---|
| `home_page` (site do cliente) | `comercial.CanalContato` | `CK_CanalContato_Tipo` só aceita `Email`, `Telefone`, `Celular`, `WhatsApp` — **o site não tem onde ser gravado** |
| `origem_alteracao` (`"Protheus-Sync · 22/08/2026 14:31"`) | `comercial.Cliente` | não existe; `AlteradoPorId` é id de usuário, não de sistema. É o campo que o doc 17, seção 5.1, cita como prova de que o dono do dado é informação de tela |
| `alteracoes_pendentes[]` (campo, valor proposto) | — | não existe estrutura. É o mecanismo do doc 17, seção 5.2, regra 2: *"Quem não é dono não sobrescreve, abre tarefa"* |
| `segmentacao.porte` | `comercial.Cliente` | não existe. O doc 17, regra 8, cita `Porte` nominalmente como exemplo de **coluna** |
| `segmentacao.rota`, `faturamento_declarado`, `hectares_totais` | `comercial.Cliente` | não existem (há `Endereco.Hectares` por fazenda, sem total) |
| `data_fundacao`, `ie_uf` | `comercial.Cliente` | não existem |
| `consentimento_lgpd.versao` | `comercial.ConsentimentoComunicacao` | não existe (há `ReferenciaEvidencia` e `OrigemEvidencia`) |

Três dos campos de `segmentacao` são candidatos legítimos a `metadado.CampoPersonalizado` pelo doc 17,
seção 10.1 — mas `porte` é citado pelo próprio doc 17 como coluna, e `home_page` não tem saída nenhuma
no desenho atual.

**Correção sugerida:** acrescentar `Site` ao `CHECK` de `CanalContato.Tipo` e decidir, campo a campo,
quais dos demais são coluna e quais são `CampoPersonalizado`, registrando na seção 9.5 do doc 17.

---

### A-18 · O doc 14, seção 4, afirma que uma regra é "vacuamente verdadeira"; ela não é

**Gravidade:** atrapalha.
**Onde:** [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), seção 4, linha 173 —
*"`Coluna_de_caractere_fixo_unico_tem_check_constraint` — **vacuamente verdadeiro hoje (nenhuma coluna
`char(1)` mapeada ainda)**"*.

`comercial.ClienteCarteira.Classe` é `char(1) NOT NULL` com `CK_ClienteCarteira_Classe`, e o
comentário do próprio teste (`TiposDeColunaTestes.cs:359`) diz corretamente *"Hoje a única é
`comercial.ClienteCarteira.Classe` (A a D), que TEM o CHECK."* O documento normativo e o código
discordam sobre um fato.

**Correção sugerida:** corrigir a linha 173 do doc 14.

---

### A-19 · O doc 14, seção 15, declara 35 métodos de teste contra o modelo; são 31

**Gravidade:** atrapalha.
**Onde:** [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md), seção 15.1, última linha.

Contagem real de `[Fact]` em `tests/Tracbel.Crm.Arquitetura.Testes/Banco/`:
`EsquemaENomenclaturaTestes` 11, `TiposDeColunaTestes` 10, `IntegridadeReferencialTestes` 6,
`AuditoriaTestes` 4, total **31**; mais 6 em `MigracaoNoContainerTestes`, que usa o atributo próprio
`[FatoSeHouverSqlServer]`.

**Correção sugerida:** corrigir o número, ou remover a contagem, que envelhece a cada PR.

---

## 3. Cosmético

**C-1 · Siglas e abreviações fora da lista fechada.** O doc 15, seção 1, fecha a lista em CEN, CNPJ,
CPF, NF, OS, ERP, CRM, e a seção 6 exige rito para termo novo. Entraram sem passar por ele: `Uf`
(`comercial.Endereco`, `organizacao.Praca`), `Cep` (`comercial.Endereco`), `EnderecoIp`
(`auditoria.EventoDeAcesso`, `comercial.ConsentimentoComunicacao`), `PotenciaCv` (`frota.Modelo`),
`DuracaoMs` (`processo.RegraExecucao`). `Cv` e `Ms` não são sequer sigla — são abreviação de
"cavalo-vapor" e "milissegundos", proibidas pela seção 5. *Correção: `PotenciaEmCavalos` e
`DuracaoEmMilissegundos`; registrar `Uf`, `Cep` e `Ip` no glossário.*

**C-2 · Booleanos que não afirmam o que afirmam.** `metadado.CampoPersonalizado.NoResumo` e
`comercial.ConsentimentoComunicacao.Concedido`, contra o doc 15, seção 5 (*"Começa com o que ela
afirma: `EstaAtivo`, `ExigeVisitaPresencial`, `FoiEstornada`"*). *Correção: `ApareceNoResumo` e
`FoiConcedido`.*

**C-3 · Variação de gênero no mesmo conceito.** `EstaAtivo` (15 ocorrências) contra `EstaAtiva` (11);
`EhObrigatorio` contra `EhObrigatoria`; `ProcessadoEm` contra `ProcessadaEm`. Gramaticalmente correto,
mas é o padrão que o doc 15, seção 6, cita como origem de FINALIZADO/FINALIZADA. *Correção: decidir
explicitamente concordar com o substantivo e registrar.*

**C-4 · Termos fora do glossário.** `processo.Fase.Marco` (ambíguo) e `seguranca.Permissao.Verbo`
(jargão de programador; um analista comercial não lê "verbo"). Nenhum dos dois está no glossário,
contra o doc 15, seção 6. *Correção: `RotuloDeEtapa`; `Acao` ou `Operacao` no lugar de `Verbo`.*

**C-5 · O critério de catálogo próprio contra `CatalogoItem` foi aplicado de forma assimétrica.**
`frota.Marca` (`Id`, `Codigo`, `Nome`, `EstaAtiva`, `EhRepresentada`) e `frota.Familia` (`Id`,
`Codigo`, `Nome`, `EstaAtiva`, `MarcaId`) são cascas de catálogo, o mesmo perfil das três tabelas
removidas em favor de `metadado.CatalogoItem` pelo doc 17, seção 8.11. Passam pelo critério 2
(relacionamento próprio), então a decisão é defensável — mas não está escrita. *Correção: registrar na
seção 8.11 qual critério cada uma satisfaz.*

**C-6 · Divergência interna do doc 17 sobre a classe do cliente.** A seção 6.1, linha 604, manda a
classe para `metadado.CatalogoItem (CLASSE_CLIENTE)`; a seção 8.3 e o doc 15, seção 3.1, dizem que é
coluna de `comercial.ClienteCarteira`. A implementação segue a seção 8.3 (`Classe char(1)` com
`CHECK`), que é a leitura correta. *Correção: corrigir a linha 604.*

**C-7 · "NomeRazao ganhou a colação" — nenhuma coluna tem colação explícita.** O doc 20, seção 4.3,
afirma que `Cliente.NomeRazao` recebeu a colação. No banco vivo, `sys.columns.collation_name` é
`Latin1_General_CI_AI` para **todas** as colunas de texto, herdada do banco; nenhuma tem colação
declarada. Funcionalmente idêntico — e o `COLLATE Latin1_General_BIN2` de `CK_Endereco_Uf` e
`CK_Documento_Resumo` está corretamente na expressão. *Correção: ajustar a frase do doc 20.*

**C-8 · Inconsistência do "De" nos nomes compostos.** `TipoProcesso` e `TipoTarefa` sem "De", contra
`MotivoDePerda`, `ItemDeProposta`, `LinhaDeNegocio`, `PassagemDeFase`, `LeituraDeHorimetro`,
`ConjuntoDePermissao`, `AlteracaoDeCampo`, `EventoDeAcesso`, `MensagemDeSaida`,
`CompartilhamentoDeRegistro` e `PontoDeSincronismo`. O mesmo dentro das colunas: `TipoDePessoa`,
`TipoDeDado`, `TipoDeCampo` e `TipoDeResposta` contra `TipoConteudo` e `TipoImplementacao`. Cada
escolha foi decidida individualmente no doc 15, seção 4; o conjunto não tem regra. *Correção: nenhuma
renomeação; uma frase no doc 15 dizendo quando o "De" entra.*

---
## 4. O que verifiquei e considerei correto

Isto também é resultado, e é a maior parte do trabalho.

**Contagem e organização**
- 63 tabelas em 10 schemas e 745 colunas — batem entre `catalogo.csv`, `esquema.json` e o banco vivo.
  O banco tem 64 objetos porque `__EFMigrationsHistory` também existe, e ela está em `metadado`,
  **não em `dbo`**, exatamente como o teste promete.
- A distribuição por schema bate linha a linha com o doc 17, seção 8.12: `organizacao` 6,
  `seguranca` 8, `comercial` 9, `processo` 13, `frota` 5, `documento` 2, `auditoria` 3,
  `integracao` 6, `metadado` 8, `relatorio` 3.

**Nomenclatura**
- **Zero** nomes de tabela ou coluna fora de PascalCase ASCII. Zero acento, zero cedilha, zero
  underscore, zero `snake_case` sobrevivente da rodada PostgreSQL.
- **Zero** prefixos `IV_`, `IVS_`, `GE_`, `GEP_`, `EXT_`, `IMP_`, `OUT_`, `DMN_`, `X_`.
- **Zero** tokens `BKP`, `OLD`, `TESTE`, `TMP`, `MIG`.
- **Zero** abreviações da lista do doc 15, seção 5 — conferi as 18 palavras por tokenização
  PascalCase sobre as 745 colunas, não por busca de substring. `Cor`, `Codigo`, `Descricao` e `Ordem`
  estão corretos e não são falsos positivos.
- **Zero** identificadores acima de 128 caracteres e nenhuma colisão de nome de índice por
  truncamento.

**Estrutura**
- **Nenhuma tabela sem chave primária** — conferido em `sys.key_constraints`, não só no modelo.
- **Nenhuma foreign key sem índice cobrindo as colunas** — as 166 FKs conferidas contra
  `sys.index_columns` no banco vivo, pelo prefixo das colunas.
- Prefixos de objeto: 306 índices, 166 FKs e 114 restrições de verificação, todos no padrão. Um único
  objeto fora da lista da seção 3 — `AK_CatalogoItem_CatalogoId`, um índice único com prefixo `AK_`,
  que a seção 15.1 permite e a seção 3 esquece de listar. É a seção 3 que está incompleta.
- `EmpresaId`, nas 21 tabelas em que existe: **sempre** `NOT NULL`, **sempre** com FK para
  `organizacao.Empresa`, **sempre** indexada. Sem uma exceção.

**Tipos**
- Todas as 7 colunas de dinheiro são `decimal(18,2)` — **inclusive as que não usam o tipo de valor
  `Dinheiro`** (`ClienteCarteira.PotencialAnual`, `Meta.Alvo`, `Praca.PotencialEstimado`,
  `ItemDeProposta.PrecoUnitario` e `ValorTotal`, `Processo.ValorEstimado` e `ValorFinal`). O teste só
  alcança as que usam o tipo de valor; a disciplina alcançou as outras.
- Todos os 19 `decimal` declaram precisão e escala. Nenhum `numeric(18,0)`.
- Nenhuma coluna resolve para `datetime`, `smalldatetime`, `money`, `float`, `real`, `text`, `ntext`,
  `image` ou `sql_variant`. Toda data e hora é `datetime2(3)`.
- As 8 colunas de JSON são `nvarchar(max)` e **todas as 8** têm o `CHECK` de `ISJSON`.
- Nenhum `HasColumnType` escrito à mão fora dos arquivos gerados pelo EF Core.

**Unificação — a pergunta do Ricardo**
- Rodei a varredura por assinatura de colunas do método de `scripts/banco/detectar-duplicatas.py`
  sobre as 63, sem alterar o script original. **Passada 1, conjunto de colunas idêntico: zero
  grupos.** Passada 2, Jaccard maior ou igual a 0,75: **um único par**, `metadado.Catalogo` contra
  `seguranca.ConjuntoDePermissao` (0,83), e é coincidência de casca de catálogo
  (`Codigo`, `Nome`, `Descricao`, `EstaAtivo`), não cópia — as duas têm papéis e filhas
  completamente diferentes.
- Repeti ignorando o bloco de auditoria, que infla a semelhança artificialmente: quatro pares acima de
  0,60, todos pelo mesmo motivo e todos justificados.
- **Nenhuma das 63 é cópia disfarçada de outra. Não há duas tabelas com o mesmo sentido e nomes
  diferentes. Há uma tabela de cliente.** Este era o medo declarado, e ele não se realizou.

**Migração PostgreSQL para SQL Server (doc 20, seção 4.5), ponto a ponto**
- `timestamptz(3)` para `datetime2(3)`: feito, verificado contra o tipo resolvido.
- `jsonb` para `nvarchar(max)` mais `ISJSON`: feito nas 8, e a lista virou código nomeado uma a uma —
  é mais forte do que era.
- `xmin` para `rowversion`: 17 colunas `Versao` do tipo `rowversion`, nas 16 tabelas transacionais.
- `gen_random_uuid()` para `NEWID()`: correto, e a escolha de **não** usar `NEWSEQUENTIALID()` está
  bem justificada — identificador previsível anularia o propósito de `ChavePublica`.
- Particionamento: as 4 tabelas do doc 20, seção 9, estão particionadas por esquema mensal, e **os 21
  índices delas estão todos alinhados** ao esquema de partição — que é o que permite `SWITCH` e
  `TRUNCATE ... WITH (PARTITIONS ...)`. Conferido em `sys.partition_schemes`. Nada depende de
  Enterprise.
- Índice parcial para índice filtrado: 35 índices filtrados, aplicados nos índices de **busca**. Os 74
  sem filtro são cobertura de FK e unicidade de `ChavePublica`, onde o filtro seria errado —
  `ChavePublica` precisa continuar única mesmo para registro excluído logicamente. A distinção foi
  feita com critério, não por descuido.
- Colação: banco em `Latin1_General_CI_AI`, e o `ALTER DATABASE` da migração inicial roda **antes** do
  primeiro `CreateTable` (linha 16 contra linha 48), de modo que as colunas herdam a colação certa. A
  ordem está correta; o que falta é o teste (I-2).
- Coluna computada `NomeNormalizado` corretamente **ausente** — a remoção prometida no doc 20, seção
  4.3, foi executada; o banco não tem nenhuma coluna computada.

**Colunas do doc 17, seção 9.5 — conferidas uma a uma, todas presentes**
`processo.Interacao.ClienteId` (com FK) · `comercial.ClienteCarteira.UltimaInteracaoEm` ·
`frota.Equipamento.Origem` (com `CK_Equipamento_Origem`) e `LocalizacaoDescrita` ·
`processo.TipoTarefa.Categoria`, `Cor` (com `CHECK` de hexadecimal), `ContaParaCobertura` e
`FormularioId` · `processo.Processo.MotivoDePerdaId` e `ObservacaoDaPerda` ·
`comercial.Cliente.NomeRazao`.

**Dados de referência**
- Os 20 catálogos de `dados-referencia/` têm destino no modelo — nenhum ficou sem lugar. Os três
  vazios (`familia`, `condicao_de_pagamento`, `resultado`) estão honestamente documentados em
  `dados-referencia/PENDENTES.md`, com a pergunta objetiva e de quem depende. É um bom padrão e merece
  continuar.
- `CK_ConsentimentoComunicacao_Canal` bate exatamente com os cinco canais de `preferencias_contato` do
  protótipo. `CK_ClienteCarteira_Classe` bate com `classe_de_cliente.json`. `CK_Cliente_Situacao` bate
  em 4 dos 5 (ver A-11).

---

## 5. O que não consegui verificar, e por quê

1. **Se o `ALTER DATABASE ... COLLATE` funciona de verdade.** Exigiria criar um banco a partir de um
   `model` com colação accent-sensitive, e esta auditoria é somente leitura. É a razão de I-2 ser
   "impede" e não "atrapalha": não é que esteja errado — é que **ninguém sabe**, e o lugar onde se
   descobre é a produção.
2. **Se os testes passam hoje.** Não executei `dotnet test`: outra frente está mexendo no código em
   paralelo, e um resultado tirado no meio de uma alteração não seria evidência de nada. Toda
   afirmação sobre teste neste documento vem da **leitura do código do teste**, não da execução. Pela
   mesma razão não conferi o total de 191 testes citado no doc 20, seção 12.
3. **Comportamento com dado real.** O banco do contêiner está vazio. Tudo o que afirmo sobre
   integridade é sobre a **estrutura** que a impõe, não sobre linhas que a violem.
4. **A cobertura do protótipo além da Ficha do Cliente.** Confrontei campo a campo
   `prototipo/dados-seed/cliente-84391.json` (A-17) e os 20 catálogos de referência. Os demais
   arquivos de `prototipo/dados-seed/` — pipeline, performance, pós-vendas, agenda, Visão 360,
   `oportunidade-1517613.json`, `equipamento-1RW7250PVMR123456.json` — **não foram percorridos campo a
   campo**, porque a auditoria foi encerrada antes. Uma segunda passada focada neles provavelmente
   encontra mais lacunas da mesma natureza que as de A-17.
5. **A cobertura de `CHECK` das colunas de texto longo e das colunas anuláveis.** Minha varredura de
   "coluna de domínio sem restrição" cobriu texto `NOT NULL` de até 60 caracteres e sem FK. Colunas
   anuláveis e colunas acima de 60 caracteres não passaram pelo mesmo filtro, então a lista de A-16
   pode estar incompleta para menos.
6. **A instância de produção.** Nada foi consultado em `10.150.14.65` — todo o trabalho aconteceu no
   contêiner local, como o doc 20, seção 6, determina.

---

## 6. O que eu faria primeiro

Na ordem, e nenhum destes é grande:

1. **I-1** — chave estrangeira composta do catálogo. É a única correção que muda o esquema de forma
   não trivial, e é a que fica mais barata agora, com o banco vazio.
2. **I-3** — `CHECK` de domínio gerado do modelo para as 10 colunas `Entidade`. Uma migração aditiva e
   um teste.
3. **I-2** — o teste da colação. Cobre o maior risco do dia do deploy.
4. **A-1** — filtro global de empresa nas 20 tabelas restantes, com o teste que o exige.
5. **A-5, A-6, A-7, A-8** — as quatro renomeações. Hoje são quatro linhas; depois da carga histórica
   da fase 6, não são.

Feitos esses cinco, eu aprovo.
---

## 7. Correções aplicadas em 04/09/2026

> Escrito por quem **implementou** a correção, não por quem auditou. As seções 1 a 6 acima ficam
> como estão — são o registro da auditoria, e reescrevê-las apagaria a evidência do defeito.
> Os 19 achados "atrapalha" e os 8 cosméticos **não foram tocados**, com uma exceção de brinde
> registrada no fim desta seção.

Tudo entrou por **uma migração aditiva**, `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade`,
gerada por `dotnet ef migrations add` sobre a migração inicial. `20260904120040_ModeloInicial` e
`ModeloInicial.ColacaoDoBanco.cs` **não foram alterados**.

### I-1 · A chave estrangeira composta do catálogo agora existe

**O que mudou.** As **nove colunas de papel** trocaram a chave estrangeira de coluna única por uma
**composta** contra `AK_CatalogoItem_CatalogoId (CatalogoId, Id)`. Cada tabela ganhou uma coluna de
catálogo **constante**, presa em dois lugares — valor padrão no banco e restrição de verificação:

| Coluna de papel | Coluna de catálogo constante | Catálogo | Restrição |
|---|---|---|---|
| `comercial.ClienteContato.PapelId` | `CatalogoDoPapelId` | `PAPEL_CONTATO` (1) | `CK_ClienteContato_CatalogoDoPapelId` |
| `comercial.Cliente.OrigemId` | `CatalogoDaOrigemId` | `ORIGEM_LEAD` (2) | `CK_Cliente_CatalogoDaOrigemId` |
| `comercial.Cliente.MotivoInativacaoId` | `CatalogoDoMotivoInativacaoId` | `MOTIVO_INATIVACAO` (3) | `CK_Cliente_CatalogoDoMotivoInativacaoId` |
| `comercial.Lead.OrigemId` | `CatalogoDaOrigemId` | `ORIGEM_LEAD` (2) | `CK_Lead_CatalogoDaOrigemId` |
| `comercial.Lead.MotivoDescarteId` | `CatalogoDoMotivoDescarteId` | `MOTIVO_DESCARTE` (4) | `CK_Lead_CatalogoDoMotivoDescarteId` |
| `comercial.Endereco.CulturaId` | `CatalogoDaCulturaId` | `CULTURA` (5) | `CK_Endereco_CatalogoDaCulturaId` |
| `documento.Documento.TipoDocumentoId` | `CatalogoDoTipoDocumentoId` | `TIPO_DOCUMENTO` (6) | `CK_Documento_CatalogoDoTipoDocumentoId` |
| `processo.ItemDeProposta.CondicaoPagamentoId` | `CatalogoDaCondicaoPagamentoId` | `CONDICAO_PAGAMENTO` (7) | `CK_ItemDeProposta_CatalogoDaCondicaoPagamentoId` |
| `processo.Processo.ConcorrenteId` | `CatalogoDoConcorrenteId` | `CONCORRENTE` (8) | `CK_Processo_CatalogoDoConcorrenteId` |

O índice de coluna única que existia só para cobrir a chave estrangeira antiga foi **substituído**
pelo composto `(CatalogoDo<Papel>Id, <Papel>Id)` — mesma seletividade, quatro bytes mais largo, e o
número de índices do banco não mudou. O mecanismo está escrito uma vez, em
`Persistencia/Configuracoes/LigacaoDeCatalogo.cs`, e usado nove vezes.

**A decisão que a correção obrigou, e ela mexe numa regra do doc 14.** "Constante", no SQL Server, é
um literal dentro de um `CHECK` — o identificador do catálogo teve que virar **esquema**, não dado.
Os oito catálogos que o modelo referencia passaram a ter `Id` fixo, declarado em
`Dominio.Metadado.CatalogosDeSistema` e semeado pela migração. Isso é uma exceção à regra 8.3 do
[14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md) (*"nunca com o `Id` interno fixado à mão"*), e foi
registrada lá como **regra 8.4**, fechada, nomeada e testada. Nada mais no banco pode fixar `Id`.

**De brinde, pelo mesmo mecanismo:** `metadado.CatalogoItem.ItemPaiId` virou composta
`(CatalogoId, ItemPaiId)` — o item pai agora tem de ser **do mesmo catálogo** que o filho. Não estava
em nenhum achado; saiu de graça e não custa índice novo.

**A única referência que continua de coluna única** é `metadado.Resposta.CatalogoItemId`, por
desenho: ela aponta para o item de *qualquer* catálogo, porque qual catálogo vale é decidido por
`Pergunta.CatalogoId` linha a linha, e `CK_Pergunta_Catalogo` já exige esse vínculo. Torná-la
composta obrigaria a copiar o catálogo da pergunta para dentro de `metadado.Resposta` — a
denormalização que a regra 4 do doc 17 proíbe. A exceção está **nomeada e justificada** em
`CatalogoDeSistemaTestes.ReferenciaGenericaJustificada`, no mesmo mecanismo de `CascadeJustificado`.

**O que impede a regressão** — `tests/Tracbel.Crm.Arquitetura.Testes/Banco/CatalogoDeSistemaTestes.cs`:

| Teste | O que ele impede |
|---|---|
| `Toda_coluna_que_aponta_para_item_de_catalogo_usa_chave_estrangeira_composta` | coluna nova apontando para `CatalogoItem` com chave de coluna única — o defeito exato do achado. Escapar exige entrada escrita em `ReferenciaGenericaJustificada` |
| `A_coluna_de_catalogo_da_chave_composta_e_uma_constante_presa_pelo_banco` | chave composta cuja coluna de catálogo não seja constante nos **dois** lugares (valor padrão e `CHECK`) — sem isso a composição não restringe nada |
| `Os_catalogos_de_sistema_nascem_semeados_com_o_identificador_que_o_esquema_usa` | o `CHECK` citar um catálogo que ninguém semeia. `[V]` é `IV_SegPerfil`, a tabela de perfis do Vórtice que o esquema pressupõe e que está vazia |

**Prova negativa, executada:** desfazendo só a ligação de `ClienteContato.PapelId`, o primeiro teste
falha com *"ClienteContato.PapelId (chave estrangeira de coluna única — aceita item de QUALQUER
catálogo)"*.

### I-2 · O `ALTER DATABASE ... COLLATE` agora é exercido

**O que mudou.** Nada na migração — ela estava certa, e o achado dizia justamente que **ninguém
sabia**. O que faltava era o teste, e ele existe:
`tests/Tracbel.Crm.Arquitetura.Testes/Banco/ColacaoNoContainerTestes.cs`, categoria `BancoReal`,
ignorado com a razão escrita quando não há servidor.

**Como o cenário é reproduzido, e a divergência da correção sugerida.** A auditoria pediu um banco
criado a partir de um `model` com colação accent-sensitive. O teste usa
`CREATE DATABASE ... COLLATE SQL_Latin1_General_CP1_CI_AS` — a colação da instância do Vórtice —
em vez de alterar o `model`. O estado que a migração enxerga é o mesmo (o `IF` do bloco lê a colação
do banco corrente, não a origem dela), e alterar o `model` exigiria acesso exclusivo a um banco de
sistema compartilhado por toda a instância, deixando-a suja se o teste caísse no meio. **O problema
apontado está resolvido: o bloco passou a executar de verdade em toda rodada de `dotnet test`.**

| Teste | O que ele impede |
|---|---|
| `A_migracao_corrige_a_colacao_do_banco_que_nasceu_com_a_colacao_do_Vortice` | o bloco quebrar em silêncio. Afirma a premissa (o banco nasceu `..._CI_AS`), aplica a migração, exige `DATABASEPROPERTYEX(DB_NAME(),'Collation') = Latin1_General_CI_AI` e exige que `metadado.Catalogo.Nome` — coluna **sem** colação declarada — tenha herdado a corrigida, o que também prova a **ordem** (o `ALTER` antes do primeiro `CreateTable`) |
| `No_banco_nascido_errado_e_corrigido_a_unicidade_ja_ignora_acento` | a prova de comportamento: nesse banco, `UX_Catalogo_Codigo` recusa `'SÉGMENTO'` ao lado de `'SEGMENTO'` |
| `A_restricao_de_dominio_recusa_o_rotulo_que_so_difere_por_acento` (novo, em `MigracaoNoContainerTestes`) | o caso de **acento** que faltava: grava `'Soja'` e depois `'Sója'`. O teste que existia media só **caixa**, e a colação do Vórtice recusaria a caixa igual — ele passaria sob a colação errada |

**Prova negativa, executada:** com o corpo de `DefinirColacaoDoBanco` desligado, os dois testes de
`ColacaoNoContainerTestes` falham — o primeiro com *"but `SQL_Latin1_General_CP1_CI_AS`"*, e o
segundo porque `'SEGMENTO'` e `'SÉGMENTO'` **entram os dois**, que é exatamente o comportamento do
Vórtice.

**Um efeito colateral registrado:** o teste de colação não usa o pool de conexões
(`Pooling=false`). `ALTER DATABASE ... COLLATE` muda o estado do banco por baixo de uma conexão
viva, e o `sp_reset_connection` do pool passa a falhar com o erro 4021. Vale só para esse arquivo.

### I-3 · `Entidade` tem domínio fechado, gerado do próprio modelo

**A escolha: restrição de verificação, não tabela de referência.** Pelo critério do
[14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md):

- **seção 14** — domínio fechado se garante com `CHECK`; `FOREIGN KEY` é o mecanismo de *"a linha
  filha só existe se a linha pai existir"*. Aqui **não há linha pai**: o valor válido é o nome de uma
  **classe do modelo**, não um registro de negócio;
- **seção 8.3** — dado de referência nasce de seed versionado. Uma tabela `metadado.Entidade` seria
  um seed que **copia o modelo**: duas fontes da verdade para a mesma lista, livres para divergir em
  silêncio. O `CHECK` é **gerado** do modelo a cada compilação, e não tem como divergir;
- **seções 10 e 13** — uma chave estrangeira nessas colunas obrigaria índice por `Entidade` nas três
  tabelas particionadas de log e de área de pouso, e uma consulta a mais por linha gravada, nas
  tabelas de maior volume do banco;
- **doc 17, seção 8.11** — não é catálogo de negócio: ninguém administra esta lista numa tela.

**Sobre "a lista muda quando o modelo cresce":** muda, e é por isso que o teste compara a lista do
`CHECK` com a lista do modelo em vez de guardar uma cópia. Entidade nova **quebra o `dotnet test`**
até que uma migração regenere as dez restrições. O custo é uma migração por entidade nova, cobrado
no lugar certo — e é menor que o de um seed que pode ficar velho sem ninguém notar. O item entrou no
checklist de PR da seção 16 do doc 14.

**O que mudou.** `CrmDbContext.FecharDominioDoPonteiroPolimorfico` roda depois de
`ApplyConfigurationsFromAssembly` e gera, para as **10** colunas `Entidade`/`EntidadeRaiz`, um
`CHECK` com os 63 nomes de entidade do modelo, em ordem estável, comparados com
`COLLATE Latin1_General_BIN2`. A colação binária é parte da regra: sob a colação do banco (`CI_AI`,
de propósito), `IN ('Cliente')` aceitaria `CLIENTE` e `cliénte` — e aqui o valor é identificador de
máquina, não texto de gente. É o mesmo caminho de `CK_Endereco_Uf` e `CK_Documento_Resumo`.

**`Campo` ficou com domínio de FORMA, e isto é uma divergência consciente da frase do achado.** As
quatro colunas `Campo` ganharam `CHECK` de identificador PascalCase ASCII — barra `nome_cliente`,
`Nome do cliente`, ` Nome` — mas **não** ganharam lista fechada. Dois motivos:

1. o conjunto válido seria as 754 colunas do modelo, o que faria **qualquer coluna nova em qualquer
   tabela** exigir migração que reescrevesse essas restrições. A relação custo/benefício é o oposto
   da de `Entidade`, onde o gatilho é entidade nova;
2. o domínio **não é uniforme entre as quatro**: `relatorio.FonteCampo.Campo` nomeia coluna de
   **visão**, não do modelo, e `metadado.CampoPersonalizado.Campo` nomeia a coluna que a migração de
   campo personalizado ainda vai criar (doc 14, regra 12.1). Fechar as quatro contra a mesma lista
   seria uma restrição que mente.

Fica registrado como **pendente**: fechar o par `(Entidade, Campo)` de `auditoria.CampoAuditado` —
a tabela que **decide o que se audita**, e onde um nome errado desliga a auditoria em silêncio — é
o caso que mais paga, e o candidato natural é uma chave estrangeira de `auditoria.AlteracaoDeCampo`
para `UX_CampoAuditado_Entidade_Campo`. Não entrou nesta rodada por ser mudança de escopo maior que
a corrigida aqui.

**O que impede a regressão** — `tests/Tracbel.Crm.Arquitetura.Testes/Banco/DominioDeEntidadeTestes.cs`:

| Teste | O que ele impede |
|---|---|
| `Toda_coluna_que_nomeia_entidade_tem_o_dominio_fechado_pela_lista_do_modelo` | coluna `Entidade` sem `CHECK`, `CHECK` sem colação binária, e — o principal — lista **divergente do modelo**, para mais ou para menos. Exige também que as colunas continuem sendo dez |
| `Toda_coluna_que_nomeia_campo_exige_identificador_do_padrao_de_nomenclatura` | coluna `Campo` aceitando qualquer texto. Exige que continuem sendo quatro |

**Prova negativa, executada:** removendo `Entidade` da geração, o primeiro teste falha listando as
nove restrições ausentes, uma a uma.

### A recusa acontecendo, no banco do contêiner

Contra `TracbelCrm` com as duas migrações aplicadas, dentro de uma transação desfeita no fim:

```
--- A) o papel CERTO (item do catalogo PAPEL_CONTATO) entra ---
ACEITO: papel = item 1 do catalogo PAPEL_CONTATO

--- B) o papel ERRADO (item do catalogo CULTURA) e RECUSADO ---
RECUSADO pelo banco.
  erro    : 547
  mensagem: The INSERT statement conflicted with the FOREIGN KEY constraint
            "FK_ClienteContato_CatalogoItem_CatalogoDoPapelId_PapelId".
            The conflict occurred in database "TracbelCrm", table "metadado.CatalogoItem".

--- C) burlar escrevendo outro catalogo na coluna constante tambem e RECUSADO ---
RECUSADO pelo banco.
  erro    : 547
  mensagem: The INSERT statement conflicted with the CHECK constraint
            "CK_ClienteContato_CatalogoDoPapelId". The conflict occurred in database
            "TracbelCrm", table "comercial.ClienteContato", column 'CatalogoDoPapelId'.

--- D) nome de entidade fora da lista fechada e RECUSADO (achado I-3) ---
RECUSADO pelo banco.
  erro    : 547
  mensagem: The INSERT statement conflicted with the CHECK constraint
            "CK_CompartilhamentoDeRegistro_Entidade". The conflict occurred in database
            "TracbelCrm", table "seguranca.CompartilhamentoDeRegistro", column 'Entidade'.

--- E) o nome CERTO da entidade entra ---
ACEITO: entidade = 'Cliente'
```

O caso **C** é o que fecha a porta dos fundos: não basta a chave composta existir, a coluna de
catálogo precisa ser constante — senão bastaria gravar outro número nela para a chave voltar a
aceitar item de qualquer lista. O caso **D** é o achado I-3 no banco: `Clientes`, com o "s" que a
auditoria citou, é recusado.

### O que ficou

- **63 tabelas em 10 schemas** — o número não mudou; a correção não criou tabela nenhuma.
- **754 colunas** (eram 745): as nove colunas de catálogo constante. `docs/banco/DICIONARIO.md`,
  `ERD.md`, `catalogo.csv` e `esquema.json` foram regerados.
- **`dotnet build`**: 0 avisos, 0 erros. **`dotnet test`**: 208 testes, 0 falhas, 0 ignorados —
  eram 200 antes (138 de domínio, 12 de aplicação, 58 de arquitetura).
- Documentos atualizados por terem ficado factualmente errados: **14** (seções 6, 8, 14, 15.1 e 16),
  **17** (seção 8.11) e **20** (seções 4.3 e 4.4). Este documento 21 não teve nenhuma linha das
  seções 1 a 6 alterada.
- **Corrigido de brinde:** `CatalogoItem.ItemPaiId` (hierarquia dentro do mesmo catálogo) e um teste
  que passava pelo motivo errado — `A_restricao_de_dominio_recusa_o_valor_invalido_no_banco` criava
  o catálogo `CULTURA` que agora nasce semeado, e a violação medida teria virado a de
  `UX_Catalogo_Codigo`. Nenhum dos 19 "atrapalha" nem dos 8 cosméticos foi tocado.

---

## 8. Correção do achado A-1 — a fronteira de multiempresa, aplicada em 04/09/2026

> Escrito por quem **implementou** a correção. As seções 1 a 6 continuam intactas — inclusive o
> texto do achado A-1, que é o registro do defeito. Dos 26 achados restantes, **nenhum foi
> tocado**: esta seção trata só do A-1.

**Nada mudou no banco.** O filtro global do EF Core é predicado de consulta, não coluna nem
restrição: as 21 colunas `EmpresaId` já estavam lá, `NOT NULL`, com FK e índice. Por isso **não há
migração nova** — `20260904120040_ModeloInicial` e
`20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade` não foram alterados, e nenhuma terceira
foi criada. O que faltava não era estrutura; era uso.

### 8.1 O que mudou

O filtro deixou de ser escrito entidade por entidade e passou a ser **varredura do modelo**, em
`CrmDbContext.AplicarFronteiraDeEmpresa`: *quem declara `EmpresaId` ganha o filtro*, resolvido em
tempo de construção do modelo. Vinte linhas escritas à mão seriam vinte linhas que a entidade nº 22
não teria — a varredura tira do desenvolvedor a chance de esquecer.

O predicado, na ordem do documento 05:

```csharp
linha => _ehSistema                                  // 1. job e integração, sob identidade explícita
      || _alcanceEntreEmpresas                       // 2. a via de escape, aberta e registrada
      || _empresasVisiveis.Contains(linha.EmpresaId) // 3. filial que o usuário alcança
      || linha.EmpresaId == _empresaId               // 4. a filial de casa dele
```

**18 entidades ganharam o filtro** (as que tinham a coluna e nenhuma proteção):

| Schema | Entidades |
|---|---|
| `comercial` | `Cliente`, `Contato`, `CanalContato`, `Endereco`, `ConsentimentoComunicacao`, `Alerta` |
| `processo` | `Processo`, `Tarefa`, `Interacao`, `ItemDeProposta` |
| `organizacao` | `Carteira`, `Meta` |
| `seguranca` | `Equipe` |
| `frota` | `Equipamento` |
| `documento` | `Documento` |
| `metadado` | `Preenchimento` |
| `relatorio` | `Relatorio` |
| `auditoria` | `AlteracaoDeCampo` |

**1 entidade já tinha filtro próprio** — `Lead`, que soma a fronteira de empresa à profundidade da
permissão (documento 05, seção 5). Ele continua sendo o modelo, e ganhou só o reconhecimento da via
de escape, na cláusula de empresa e **não** na de propriedade: quem abre o alcance entre filiais com
profundidade `Proprios` continua vendo só os leads dele, em todas as filiais que já alcançava. A
camada 3 não cai junto com a camada da empresa.

Nenhuma dessas 18 recebeu filtro de exclusão lógica junto: `ExcluidoEm` e a profundidade por dono
são das camadas 3 e 5 e entram no filtro próprio de cada entidade, como já acontece no `Lead`. A
fronteira de empresa é a **primeira** camada, e é a que não podia faltar em nenhuma.

### 8.2 As duas exceções — declaradas, e por quê

Vivem em `CrmDbContext.FronteiraDeEmpresaJustificada`, com o motivo escrito ao lado do nome. São
duas, e as duas são casos em que `EmpresaId` **não significa "a filial dona desta linha"**:

| Entidade | Por que não é filtrada |
|---|---|
| `seguranca.Usuario` | O `EmpresaId` aqui é a filial **de casa** do usuário — o dado que *define* o escopo dos outros filtros, não a linha protegida por ele. Filtrar seria circular: o `ContextoAcesso` é montado **lendo esta tabela**, antes de existir escopo nenhum, e a consulta de login não devolveria ninguém. Depois de montado, quebraria também o que a fronteira não deve quebrar — mostrar quem criou ou alterou um registro transferido entre filiais, e resolver a hierarquia comercial, que atravessa filial por desenho (documento 05, seção 5). O que limita a exposição do usuário é a camada 5, de campo sensível. |
| `seguranca.CompartilhamentoDeRegistro` | É a **camada 4** do documento 05, aditiva e desenhada exatamente para atravessar a fronteira: é o que deixa o gerente ver uma oportunidade de outra filial sem trocar o dono dela. O `EmpresaId` da linha é a filial **do registro compartilhado** — quase sempre a outra. Filtrar por empresa tornaria o compartilhamento entre filiais invisível para quem o recebeu, e faria o endpoint "por que eu vejo isto" (seção 6) mentir. **A fronteira devida aqui é o beneficiário** (`UsuarioId`/`EquipeId`), e ela entra junto com a implementação da camada 4 — fica registrado aqui como dívida nomeada, não como esquecimento. |

A lista é fechada dos dois lados: ela está no `CrmDbContext` **e** em `MultiempresaTestes`. Criar uma
exceção nova exige mexer nos dois arquivos e escrever o motivo aqui — que é o custo que uma exceção
de segurança deve ter. Silenciar o teste acrescentando uma linha só no código não funciona.

### 8.3 A via de escape — quem ignora a fronteira precisa dizer que está ignorando

O administrador que concilia duas filiais e a integração que exporta a base para o Protheus são usos
legítimos. Eles não podem ser impossíveis, e também não podem acontecer por descuido. Então a
fronteira cai por **declaração**:

```csharp
using (contexto.AbrirAlcanceEntreEmpresas("conciliação de carteira entre Ribeirão Preto e Bebedouro, chamado 4711"))
{
    // aqui, e só aqui, as consultas atravessam a fronteira
}
```

Quatro exigências, e nenhuma é dispensável:

1. **motivo escrito**, nunca em branco — é o que alguém vai ler depois;
2. **permissão**: `Empresa.AlcanceEntreFiliais` em profundidade `Organizacao`
   (`ContextoAcesso.PermissaoDeAlcanceEntreEmpresas`). O serviço de sistema já a tem por
   `EhServicoDeSistema`;
3. **diário**: sem um `IDiarioDeAlcanceEntreEmpresas` configurado, **não abre**. Escape sem rastro
   não é escape legítimo, é furo. A implementação de hoje é
   `Infraestrutura/Multiempresa/DiarioDeAlcanceEntreEmpresasEmLog`, que escreve abertura e
   fechamento no log em nível de **alerta** — se isso aparecer mil vezes por dia, alguma rotina está
   abrindo escape como se fosse consulta normal, e o painel precisa mostrar isso sem ninguém
   procurar. Quando a autenticação pelo Entra ID entrar, o mesmo evento vira linha em
   `auditoria.EventoDeAcesso`;
4. **escopo**: vale até o `Dispose`, e o padrão nunca é "aberto". **Ter a permissão de abrir não é
   estar aberto** — o administrador que não abrir enxerga as mesmas filiais de sempre.

### 8.4 O teste que impede a regressão

`tests/Tracbel.Crm.Arquitetura.Testes/Banco/MultiempresaTestes.cs`, quatro métodos contra o modelo:

| Teste | O que ele impede |
|---|---|
| `Toda_entidade_com_EmpresaId_tem_filtro_global_que_usa_a_coluna` | tabela nova com a coluna e sem filtro — e também filtro que existe mas **não cita `EmpresaId`**, que passaria numa checagem só de existência |
| `Todo_filtro_de_empresa_reconhece_a_via_de_escape_declarada` | escape pela metade: umas tabelas atravessando a fronteira e outras não, que é pior do que não ter escape, porque parece que funcionou |
| `As_excecoes_da_fronteira_sao_exatamente_as_acordadas_e_cada_uma_tem_motivo_escrito` | exceção silenciosa. A lista é fechada, e cada motivo precisa ter substância |
| `Nenhuma_excecao_da_fronteira_aponta_para_entidade_que_sumiu_ou_perdeu_a_coluna` | entrada morta, que sugere decisão deliberada onde não há nem entidade |

E `tests/Tracbel.Crm.Aplicacao.Testes/Persistencia/FronteiraDeEmpresaTestes.cs`, nove métodos de
**comportamento** contra SQLite em memória — porque filtro que não traduz para SQL é filtro que não
existe. Eles provam a fronteira sobre `Cliente`, entidade que nunca teve filtro; que o `Where` do
chamador e a busca pelo `Id` direto não a furam; que a cláusula sai no SQL; que ela vale para
`Equipamento`, `Tarefa`, `Interacao`, `Carteira` e `AlteracaoDeCampo`; e as cinco condições da via de
escape. As filiais do teste usam os códigos reais de `dados-referencia/empresa.json` — `010101`
Ribeirão Preto, `010102` Araraquara e `010109` Bebedouro, todas da lista de 13 em operação. As três
marcadas "a confirmar" não aparecem em teste nenhum.

**Prova negativa, executada.** Comentando a linha `AplicarFronteiraDeEmpresa(modelo);` e rodando
`dotnet test`:

```
Failed!  - Failed: 1, Passed: 52, Total: 53 - Tracbel.Crm.Arquitetura.Testes.dll
Failed!  - Failed: 4, Passed: 17, Total: 21 - Tracbel.Crm.Aplicacao.Testes.dll

  Failed MultiempresaTestes.Toda_entidade_com_EmpresaId_tem_filtro_global_que_usa_a_coluna
  Error Message:
   Expected semFiltro to be empty because toda entidade que declara EmpresaId recebe o filtro
   global de empresa (documento 14, regra 11.3) [...] Sem filtro: AlteracaoDeCampo, Alerta,
   CanalContato, Cliente, ConsentimentoComunicacao, Contato, Endereco, Documento, Equipamento,
   Preenchimento, Carteira, Meta, Interacao, ItemDeProposta, Processo, Tarefa, Relatorio, Equipe
```

O teste não só falha: ele **nomeia as 18**. A linha foi restaurada em seguida.

### 8.5 A fronteira acontecendo

Impresso pelo próprio teste, contra o banco em memória, para um usuário da filial `010101` que
alcança `010101` e `010102` — três clientes semeados, um em cada filial:

```
--- SQL gerado por db.Clientes, sem nenhum Where do chamador ---
.param set @__ef_filter__p_2 0
.param set @__ef_filter___empresaId_1 1

SELECT "c"."Id", ..., "c"."EmpresaId", ..., "c"."NomeRazao", ...
FROM "Cliente" AS "c"
WHERE @__ef_filter__p_2 OR "c"."EmpresaId" IN (1, 2) OR "c"."EmpresaId" = @__ef_filter___empresaId_1

--- COM a fronteira (usuario da filial 010101, alcanca 010101 e 010102) ---
  Fazenda da minha filial
  Fazenda da filial filha

--- SEM a fronteira (IgnoreQueryFilters), a MESMA consulta ---
  Fazenda da minha filial
  Fazenda da filial filha
  Fazenda de Bebedouro
```

O parâmetro `@__ef_filter__p_2` é a via de escape somada ao serviço de sistema — `0` aqui, porque
ninguém declarou nada. É ele que vira `1` dentro do `using` do administrador, e é por isso que a
contagem do teste da via de escape vai de **2 → 3 → 2** no mesmo objeto de contexto: aberto, usado,
fechado.

A diferença entre os dois blocos é o achado A-1 inteiro: **o dado da outra filial está no banco, e a
consulta que esquece o `WHERE` deixou de alcançá-lo**.

### 8.6 O que ficou

- **21 entidades com `EmpresaId`**: 18 ganharam o filtro, 1 (`Lead`) já tinha o seu, 2 são exceções
  declaradas e justificadas.
- **63 tabelas em 10 schemas, 754 colunas** — nada mudou no esquema, e não há migração nova.
- **`dotnet build`**: 0 avisos, 0 erros. **`dotnet test`**: **221 testes, 0 falhas, 0 ignorados** —
  eram 208 (138 de domínio, 12 de aplicação, 58 de arquitetura); agora são 138 de domínio, 21 de
  aplicação e 62 de arquitetura, com os 13 novos desta correção.
- **Arquivos tocados:** `CrmDbContext.cs`, `ContextoAcesso.cs`,
  `Portas/IDiarioDeAlcanceEntreEmpresas.cs` (novo),
  `Infraestrutura/Multiempresa/DiarioDeAlcanceEntreEmpresasEmLog.cs` (novo), `Api/Program.cs` (o
  registro do diário) e os dois arquivos de teste. **`src/Tracbel.Crm.Web/` não foi tocado.**
- **Dívida nomeada, para não virar esquecimento:** `CompartilhamentoDeRegistro` precisa do filtro
  por **beneficiário** quando a camada 4 do documento 05 for implementada. A regra 11.3 do documento
  14 — *"hoje só `Lead` tem o filtro implementado"* — e a lacuna admitida na seção 5.2 do mesmo
  documento ficaram desatualizadas por esta correção e pedem revisão na próxima rodada de documento.
