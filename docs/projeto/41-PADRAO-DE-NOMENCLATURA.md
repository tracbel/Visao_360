# 41 — Padrão de nomenclatura do CRM

> **Versão 1.0 · 15/09/2026 · decisão D-9 da Fase 2.** Documento de padrão: define **como nomear**.
> A lista do que muda, nome a nome, está em [41B — Matriz de nomenclatura](41B-MATRIZ-DE-NOMENCLATURA.md).
>
> **Nada foi renomeado.** Nenhuma migração, entidade, rota ou tela foi alterada. O padrão vale a partir
> da aprovação; a matriz é executada dentro das fases do
> [41 — Plano executivo](41-PLANO-EXECUTIVO-DA-REESTRUTURACAO.md).
>
> Base: [documento 39](39-AUDITORIA-ARQUITETURA-BANCO.md) (seção 17, achados de nomenclatura),
> [documento 40](40-ARQUITETURA-ALVO-DO-BANCO.md) (modelo alvo), documento 14 (padrão de banco) e
> documento 15 (glossário).

## 1. Por que agora

O banco operacional está vazio (documento 38). Renomear com tabela vazia custa uma migração e nenhuma
transformação de dado. Depois que o CRM voltar a ter dado, a regra 9.3 do documento 14 volta a valer —
migração é aditiva, e renomear passa a custar duas entregas.

**Regra de ouro desta janela:** ou se renomeia tudo o que a matriz 41B listar, ou não se renomeia nada.
Meio sistema com nome novo e meio com nome antigo é pior do que o estado atual.

## 2. Princípios

| # | Princípio | Consequência |
|---:|---|---|
| N1 | **Português do negócio, por extenso.** | Sem abreviação (`Qtd`, `Dt`, `Cod`), sem inglês no domínio. Exceções só onde o termo técnico é inglês consagrado (`Hash`, `Json`, `Token`). |
| N2 | **Um conceito, um nome, em toda a pilha.** | O mesmo conceito tem o mesmo nome na tabela, na entidade, no contrato, na rota e na tela. |
| N3 | **O nome do banco é o nome da entidade.** | `seguranca.Perfil` ↔ `Perfil`. Sem tradução no mapeamento. |
| N4 | **O nome é o do negócio, não o da origem.** | Nada de vocabulário herdado do Vórtice, do ART ou de planilha. |
| N5 | **Nome diz o que é, não como foi feito.** | `ClassificacaoDeProduto`, e não `LinhaDeProduto` (que veio do texto do ART). |
| N6 | **Sufixo com significado fixo.** | `...Id` é chave estrangeira; `...Em` é data ou instante; `Esta.../Eh...` é booleano. |
| N7 | **Sem sufixo de camada.** | Nada de `Dto`, `VM`, `Model`, `Entity`, `Helper`, `Manager`. |
| N8 | **O plural é só do `DbSet` e da rota.** | Tabela e entidade no singular. |

## 3. Regras por artefato

### 3.1 Schema

Minúsculo, singular, sem acento. Os do alvo: `organizacao`, `seguranca`, `comercial`, `processo`,
`frota`, `metadado`, `integracao`, `auditoria`. Um schema é um domínio; não se cria schema para agrupar
por tipo de objeto.

### 3.2 Tabela

- **PascalCase, singular, português, sem abreviação.**
- **Entidade composta usa preposição:** `VendaDeMaquina`, `MotivoDePerda`, `ClassificacaoDeProduto`,
  `RegistroDeOrigem`, `PendenciaDeCadastro`.
- **Tabela de vínculo (N:N) justapõe os dois nomes, do dono para o membro, sem preposição:**
  `ClienteCarteira`, `CarteiraMunicipio`, `UsuarioPerfil`, `PerfilPermissao`.
- **Sem prefixo de schema no nome** (`comercial.Cliente`, nunca `comercial.ComercialCliente`).

### 3.3 Coluna

| Tipo | Regra | Exemplo |
|---|---|---|
| Chave primária | `Id` | `Id` |
| Chave pública | `ChavePublica` (é o que a API expõe) | `ChavePublica` |
| Chave estrangeira | `<Entidade>Id`, com o **nome completo da entidade** | `LinhaDeNegocioId` (nunca `LinhaNegocioId`), `PerfilId` |
| Papel na relação | `<Papel>Id`, quando há mais de uma FK para a mesma entidade | `ResponsavelId`, `CompradorId`, `GestorId` |
| Instante ou data | `<particípio ou substantivo>Em` | `CriadoEm`, `OcorridaEm`, `VendidaEm`, `EncerradoEm` |
| Booleano de estado | `Esta<Adjetivo>` | `EstaAtivo` |
| Booleano de característica | `Eh<Adjetivo>` | `EhPrincipal`, `EhPadrao` |
| Booleano de regra | `Exige...`, `Permite...`, `Conta...`, `Tem...` | `ExigeObservacao`, `ContaParaCobertura` |
| Enumeração | o substantivo do enum | `Situacao`, `Natureza`, `Decisao` |
| Valor monetário | `Valor...` | `ValorLiquido`, `ValorEstimado` |
| Contagem | substantivo no plural | `Notas`, `Itens`, `Leituras` |
| Concorrência | `Versao` **reservada** para `rowversion` | `Versao` |

**Decisões de nomenclatura que encerram divergências de hoje:**

1. **`EstaAtivo` é invariável**, em qualquer tabela. Hoje há `EstaAtivo` em 16 tabelas e `EstaAtiva` em
   13 — duas grafias para a mesma coisa. A concordância de gênero é mais bonita em prosa e pior em
   consulta: quem escreve um filtro precisa lembrar o gênero de cada tabela.
2. **`Versao` só é concorrência.** Versão de negócio recebe nome próprio: `NumeroDaVersao`,
   `VersaoPublicada`. `TipoProcesso.Versao int` (hoje) é o caso a corrigir.
3. **CPF/CNPJ chama-se `CpfCnpj`.** `Documento` fica reservado para arquivo. Onde o domínio garante
   pessoa jurídica, `Cnpj` (caso de `Empresa`).
4. **`Natureza` só quando o enum é `NaturezaDo<Algo>` daquela tabela.** Onde o sentido é outro, o nome
   é específico: `NaturezaDaContraparte` no faturamento.
5. **Coluna que termina em `Id` é chave estrangeira.** Enum de desfecho não se chama `Resultado` ao
   lado de FKs `ResultadoId`: em `ExecucaoDeSincronizacao` passa a ser `Desfecho`.
6. **Nada de `...NaOrigem` em tabela de domínio** (princípio P4 do documento 40): esse vocabulário vive
   em `RegistroDeOrigem`.

### 3.4 Entidade, `DbSet` e configuração

- **Entidade = nome da tabela**, sem exceção. Hoje há quatro divergências (`ConjuntoPermissao` ↔
  `ConjuntoDePermissao`, `ItemConjuntoPermissao` ↔ `ConjuntoDePermissaoItem`, `UsuarioConjuntoPermissao`
  ↔ `UsuarioConjuntoDePermissao`, `ExecucaoRegra` ↔ `RegraExecucao`).
- **`DbSet` = plural da entidade, pluralizando o último termo:** `Clientes`, `VendasDeMaquina`
  (plural no primeiro termo quando a preposição liga um complemento fixo), `MotivosDePerda`,
  `ClienteCarteiras`, `UsuarioPerfis`, `Faturamentos`.
- **Configuração:** `<Schema>Configuracao.cs` ou `<Area>Configuracao.cs`, uma classe por entidade
  (`IEntityTypeConfiguration<T>`).

### 3.5 Enum

- Nome: `<Conceito>Do<Entidade>` / `<Conceito>Da<Entidade>` / `<Conceito>De<Coisa>` —
  `SituacaoDoCliente`, `NaturezaDaCarteira`, `ClasseDeCliente`. É o padrão já dominante (58 enums).
- Valores em PascalCase, sem prefixo (`Aberta`, `Ganha`, `Perdida`).
- **Persistido como texto**, com o mesmo nome do valor — nunca o número.
- Enum sem `Nenhum`/`Indefinido`, salvo quando "não informado" é um estado real do negócio.

### 3.6 Caso de uso (a camada de aplicação)

- Nome: **verbo no infinitivo + substantivo**: `ListarClientes`, `ObterCliente`, `CriarCliente`,
  `AlterarCliente`, `InativarCliente`, `ConcluirTarefa`, `EncerrarOportunidade`.
- Arquivo por área: `ConsultasDe<Area>.cs`, `CadastroDe<Area>.cs`, `Contratos De<Area>.cs`.
- **Não existe "service".** O que existe é caso de uso.

### 3.7 Contrato (o DTO)

| Papel | Padrão | Exemplo |
|---|---|---|
| Item de lista | `<Entidade>Resumo` | `ClienteResumo` |
| Ficha | `<Entidade>Detalhe` | `ClienteDetalhe` |
| Entrada de criação | `Novo<Entidade>` | `NovoCliente` |
| Entrada de alteração | `AlteracaoDe<Entidade>` | `AlteracaoDeCliente` |
| Entrada de baixa | `InativacaoDe<Entidade>` | `InativacaoDeCliente` (hoje há `BaixaDeEquipamento`) |
| Leitura composta | `Painel<Assunto>` / `<Assunto>Resumido` | `PainelDoCen`, `FaturamentoResumido` |

Sem sufixo `Dto`. O contrato é `record` selado.

### 3.8 Porta e repositório

- Porta (interface, no domínio): `IRepositorioDe<Plural>` — hoje é `IRepositorioClientes`, sem o "De".
- Implementação: `RepositorioDe<Plural>`.
- Um método por pergunta, com nome de pergunta: `ListarAsync`, `ObterPorChaveAsync`.

### 3.9 Rota

- `/api/v1/<recurso-no-plural-kebab>`; sub-recurso depois da chave:
  `/api/v1/clientes/{chave:guid}/maquinas-compradas`.
- Relatórios: `/api/v1/relatorios/<assunto-kebab>`.
- Administração: `/api/v1/admin/<recurso-kebab>`.
- **A URL usa `ChavePublica`**, nunca o `Id` interno.
- O verbo é o HTTP. Nada de `/obterCliente` ou `/clientes/listar`.

### 3.10 JSON da API

- `camelCase`, derivado do nome do contrato: `nomeRazao`, `ultimaInteracaoEm`.
- Instantes em ISO-8601 UTC; quando o campo puder ser confundido com hora local, sufixo `Utc`
  (`lidoEmUtc`).
- Enum viaja como texto, igual ao domínio.

### 3.11 Migração

- `yyyyMMddHHmmss_FraseEmPascalCase`, descrevendo **a mudança de negócio**:
  `RemocaoDeEstruturasSemUso`, `PerfilESubstituicaoDoPapel`, `FaturamentoUnico`.
- **Uma migração por fase** do plano executivo; nunca `Update1`, `Fix`, `Ajustes`.
- Renomear enquanto o banco está vazio é permitido nesta janela; depois volta a regra aditiva.

### 3.12 Teste

- Classe: `<Assunto>Testes` (`CadastroDeClienteTestes`, `EsquemaENomenclaturaTestes`).
- Método: frase em português com sublinhado, afirmando a regra —
  `Os_dez_schemas_do_modelo_unificado_existem_e_somam_oitenta_tabelas`.

### 3.13 Frontend

- Tela e componente: `PascalCase.tsx` (`ClienteCadastro.tsx`).
- Módulo de dados e utilitário: `camelCase.ts` (`relacionamento.ts`).
- Tipos em `tipos/`, com o mesmo nome do contrato da API, em PascalCase.
- Rota de tela em kebab (`/relatorios/cobertura`).

## 4. Vocabulário oficial

| Termo de negócio | Nome técnico (alvo) | Nome atual | Nunca usar |
|---|---|---|---|
| Oportunidade | `Oportunidade` | `Processo` | "processo" para negócio |
| Pipeline (funil) | `Pipeline` | `TipoProcesso` | "tipo de processo" |
| Etapa | `Etapa` | `Fase` | "fase" na tela |
| Atividade planejada | `Tarefa` | `Tarefa` | "agenda" como entidade |
| Atividade realizada | `Interacao` | `Interacao` | "histórico" como entidade |
| Tipo de atividade | `TipoDeAtividade` | `TipoTarefa` | "categoria de interação" como segundo conceito |
| Resultado da atividade | `ResultadoDeAtividade` | `Resultado` | — |
| Classificação de produto | `ClassificacaoDeProduto` | `LinhaDeProduto` | "linha de produto" (colide com linha de negócio) |
| Linha de negócio | `LinhaDeNegocio` | `LinhaDeNegocio` | "departamento", "segmento" |
| Perfil de acesso | `Perfil` | `ConjuntoDePermissao` | "papel" |
| Filial | `Empresa` | `Empresa` | "unidade", "loja" no modelo |
| Carteira | `Carteira` | `Carteira` | — |
| Classe do cliente | `Cliente.Classe` | duas colunas | "potencial" como classe |
| Cadência | `LinhaDeNegocio.DiasCicloClasse<A..D>` | duas fontes | "ciclo" solto |
| Parte não cadastrada | `PendenciaDeCadastro` | `CompradorPendente` | "comprador pendente" (é mais amplo) |
| Rastro de origem | `RegistroDeOrigem` | 5 mecanismos | `ChaveExterna`, `Recepcao` |

## 5. Nomes proibidos

1. **Vocabulário do Vórtice no domínio:** `Processo` (como oportunidade), `Potencial` (como classe),
   `Acao`/`AcaoGeradora`, `Depto`, `Pes`.
2. **Vocabulário do ART:** `LinhaNaOrigem`, `ProdutoNaOrigem`, `GestaoNaOrigem`, `VendaDireta`,
   `RepasseDireto` **em tabela de domínio** (no rastro, podem ficar).
3. **Vocabulário de planilha:** `ArquivoDeOrigem`, `LinhaNaOrigem`, `ChaveNaOrigem` em tabela de
   negócio.
4. **Genéricos sem dono:** `Tipo`, `Status`, `Flag`, `Dados`, `Info`, `Valor` sozinhos.
5. **Abreviação:** `Qtd`, `Dt`, `Nr`, `Cod`, `Desc`, `Obs`.
6. **Inglês no domínio:** `Status`, `Owner`, `Payload`, `Name` (o `PayloadOriginal` de `Lead` sai com a
   tabela).

## 6. Como o padrão é verificado

O projeto já tem testes de arquitetura de banco (`Arquitetura.Testes/Banco`). Depois da aprovação, eles
passam a cobrir:

| Regra | Verificação |
|---|---|
| Entidade = tabela | comparar `ToTable` com o nome do tipo |
| FK termina em `Id` e usa o nome completo da entidade | ler o modelo do EF |
| `Versao` só como `rowversion` | tipo da coluna |
| `EstaAtivo` invariável | nome das colunas booleanas de estado |
| Sem abreviação proibida | lista fechada, hoje verificada só em coluna — passa a valer para tabela |
| Enum persistido como texto | conversor no modelo |
| Rota em kebab e com `ChavePublica` | teste de rota na API |

## 7. O que vem depois

1. Aprovar este padrão.
2. Aprovar a [matriz 41B](41B-MATRIZ-DE-NOMENCLATURA.md), que aplica o padrão nome a nome.
3. Os renomes entram **dentro das fases** do plano executivo (fase 3 para identidade, fase 4 para
   configuração comercial, fases 6 e 7 para oportunidade, atividade e produto) — nunca numa fase
   separada só de renome, para não haver duas rodadas de quebra no frontend.
