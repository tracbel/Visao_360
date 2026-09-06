# dados-referencia — os catálogos que alimentam os campos de seleção do CRM

> Requisito do Ricardo: *"quando tiver que dar seguimento em um formulário do CEN, a maioria
> dos campos vão puxar dados de tabelas e não coisa manual escrita"*. Esta pasta é **o dado**
> desses catálogos — um arquivo JSON por tabela de referência, pronto para virar carga (`seed`
> ou migration) assim que a tabela existir no PostgreSQL. Esta frente não cria tabela nem faz
> carga — outras frentes fazem isso a partir destes arquivos.

Regras aplicadas a todo arquivo aqui (documento
[16-HIGIENIZACAO-DE-DADOS](../docs/projeto/16-HIGIENIZACAO-DE-DADOS.md) §3): código estável em
maiúsculo sem acento/espaço (é ele que vai para o histórico, nunca muda depois de usado),
descrição em português correto com o que aparece na tela, `ordem` explícita, `ativo` para poder
aposentar um item sem apagar, e nenhuma duplicata semântica sem registro em `observacao`.

## Como este arquivo foi gerado

Os 20 arquivos foram produzidos programaticamente a partir das fontes reais — nunca digitados à
mão — por um script de geração único (não versionado; a lógica de transformação de cada catálogo
está descrita na coluna "De onde veio" abaixo e, em detalhe, no campo `origem` de cada JSON). Isso
evita erro de transcrição, principalmente de acentuação. Todo achado de qualidade de dado
encontrado durante a geração (nome de tabela que não bate com o conteúdo, geografia fictícia,
código repetido no legado) está documentado no campo `observacao` do item afetado e, quando é uma
decisão pendente do negócio, em [PENDENTES.md](PENDENTES.md).

## Os 20 catálogos

| Arquivo | O que é | Itens | De onde veio |
|---|---|---:|---|
| `motivo_de_perda.json` | Por que uma oportunidade foi perdida | 8 | Protótipo (`config-motivos-perda.json`) |
| `tipo_de_tarefa.json` | Classificação da tarefa da Agenda do CEN | 4 | Protótipo (`tipo-meta.json`) |
| `categoria_de_interacao.json` | Canal pelo qual o contato aconteceu | 6 | Protótipo (`config-categorias-interacao-cfg.json` + cor/ícone de `config-categorias-interacao.json`) |
| `linha_de_negocio.json` | Frente de produto vendida (Tratores, Colheitadeiras…) | 6 | Protótipo (`linha-icon.json` + `catalogo-modelos.json`) |
| `classe_de_cliente.json` | Classificação ABCD, define frequência de visita | 4 | Protótipo (`meta-frequencia.json`) |
| `situacao_do_cliente.json` | Estágio do ciclo comercial do cliente | 5 | Decisão do doc 17 §3 + `GE_Pessoa.Status` do Vórtice |
| `marca.json` | Fabricante do equipamento | 5 | **Reconstruído por inferência** — ver PENDENTES |
| `familia.json` | Agrupamento de modelos dentro da linha | 0 | **Sem fonte** — ver PENDENTES |
| `modelo.json` | Modelo de equipamento vendável | 18 | Protótipo (`catalogo-modelos.json`) |
| `praca.json` | Praça de mercado (região) | 4 | Protótipo (`mercado-pracas.json`) — **geografia fictícia**, ver PENDENTES |
| `cultura.json` | Cultura agrícola da propriedade do cliente | 3 | Protótipo (`cliente-84391.json`) |
| `concorrente.json` | Fabricante concorrente | 2 | **Reconstruído** do texto de `config-motivos-perda.json` — ver PENDENTES |
| `condicao_de_pagamento.json` | Forma de pagamento/financiamento | 0 | **Sem fonte** — ver PENDENTES |
| `tipo_de_documento.json` | Tipo de arquivo anexável | 107 | Legado (`DMN_DocTp.csv`) |
| `fase.json` | Etapa do funil de oportunidade (kanban) | 6 | Protótipo (`pipeline-fases.json`) |
| `tipo_de_processo.json` | Modelo de fluxo que um processo segue | 62 | Legado (`IV_CodProcesso.csv`) |
| `empresa.json` | Filial da Tracbel | 18 | Legado (`GE_Empresa.csv`) |
| `papel_de_contato.json` | Papel do contato na decisão de compra | 3 | Legado (`GE_Contato.NivelDecisao`, via `dominios/valores.csv`) |
| `origem_de_lead.json` | Canal de origem do lead | 45 | Legado (`IV_Motivo.csv`) — ver achado em PENDENTES |
| `resultado.json` | Desfecho de uma tarefa | 0 | **Sem fonte utilizável** — ver PENDENTES |

**306 itens no total**, em 17 catálogos preenchidos e 3 vazios (`familia`, `condicao_de_pagamento`,
`resultado` — cada um com o motivo e a pergunta objetiva em [PENDENTES.md](PENDENTES.md)).

## Tabela de conferência — catálogo → tabela do modelo → tela que usa

Conforme [17-MODELO-UNIFICADO §6.1](../docs/projeto/17-MODELO-UNIFICADO.md#6-o-formulário-do-cen-puxa-de-tabela).

| Catálogo | Tabela no modelo unificado | Tela / campo do formulário |
|---|---|---|
| `motivo_de_perda` | `processo.MotivoDePerda` | Ficha de Oportunidade (marcar como perdida), Visão 360, Configurações › Taxonomias |
| `tipo_de_tarefa` | `processo.TipoTarefa` | Agenda do CEN, Nova tarefa |
| `resultado` | `processo.Resultado` | Registrar resultado de uma tarefa da Agenda |
| `categoria_de_interacao` | `processo.TipoTarefa.Categoria` | Drawer de registro de contato da Cobertura, timeline do cliente |
| `linha_de_negocio` | `organizacao.LinhaDeNegocio` | Nova Oportunidade (cascata Linha→Modelo), Visão 360 (mix por linha), presente em quase toda tela |
| `classe_de_cliente` | `comercial.ClienteCarteira.Classe` | Chips de filtro de Clientes e Cobertura |
| `situacao_do_cliente` | `comercial.Cliente.Situacao` (domínio `CHECK`) | Tela Clientes (filtro e cadastro) |
| `marca` | `frota.Marca` | Nova Oportunidade, Ficha do Equipamento |
| `familia` | `frota.Familia` | Catálogo em cascata da Nova Oportunidade (nível entre linha e modelo) |
| `modelo` | `frota.Modelo` | Nova Oportunidade (cascata Linha→Modelo), Ficha do Equipamento |
| `praca` | `organizacao.Praca` | Visão 360 (KPI "Conhecimento de mercado"), Cobertura Regional |
| `cultura` | `metadado.CatalogoItem` (`CULTURA`) | Ficha do Cliente, endereços/fazendas |
| `concorrente` | `metadado.CatalogoItem` (`CONCORRENTE`) | Registro de venda perdida, aba Competidores |
| `condicao_de_pagamento` | `metadado.CatalogoItem` (`CONDICAO_PAGAMENTO`) | Nova Oportunidade, item de proposta |
| `tipo_de_documento` | `metadado.CatalogoItem` (`TIPO_DOCUMENTO`) | Aba Documentação (cliente, processo, equipamento) |
| `fase` | `processo.Fase` | Pipeline de Vendas (kanban), timeline da oportunidade |
| `tipo_de_processo` | `processo.TipoProcesso` | Interno — define qual fluxo/fases um processo segue |
| `empresa` | `organizacao.Empresa` | Seletor de contexto (multiempresa/multifilial) |
| `papel_de_contato` | `metadado.CatalogoItem` (`PAPEL_CONTATO`) | Ficha do Cliente, aba Contatos |
| `origem_de_lead` | `metadado.CatalogoItem` (`ORIGEM_LEAD`) | Cadastro de Lead |

## Forma de cada arquivo

```json
{
  "catalogo": "motivo_de_perda",
  "descricao": "Por que uma oportunidade foi perdida",
  "origem": "de onde veio o dado, com achados relevantes",
  "itens": [
    { "codigo": "PRECO_ACIMA", "descricao": "Preço acima do concorrente", "ordem": 1, "ativo": true, "observacao": "" }
  ]
}
```

- **`codigo`** — estável, maiúsculo, sem acento e sem espaço. É o que vai para a coluna de FK e
  para o histórico; nunca muda depois que um registro transacional o referenciar.
- **`descricao`** — o rótulo em português correto, com acento — é o que aparece na tela.
- **`ordem`** — posição de exibição no `<select>`/picklist. Não precisa ser a ordem alfabética.
- **`ativo`** — `false` aposenta o item sem apagar (ver procedimento abaixo).
- **`observacao`** — nunca vazio por acaso: carrega a citação da fonte, o de-para de grafia
  quando duas fontes diziam a mesma coisa com palavras diferentes, ou o achado de qualidade de
  dado relevante para aquele item específico.

## Procedimento para acrescentar ou aposentar um item

Documento [16-HIGIENIZACAO-DE-DADOS §3.1](../docs/projeto/16-HIGIENIZACAO-DE-DADOS.md#31-campo-com-catálogo-não-aceita-digitação-livre).

**Para acrescentar:**
1. Alguém do negócio propõe o valor novo (mesmo rito do glossário, documento 15 §6).
2. O dono do catálogo aprova — catálogo comercial (motivo de perda, categoria de interação,
   classe de cliente): Gestor Regional ou Admin Comercial; catálogo de frota (marca, modelo):
   quem administra o cadastro de produto.
3. Acrescenta-se o item ao final da lista de `itens` do arquivo correspondente, com `codigo`
   novo (nunca reaproveita um código aposentado), `ordem` maior que o último item ativo, e
   `ativo: true`. Roda-se `node scripts/dados/validar-referencia.mjs` antes de commitar.
4. Só depois de existir no catálogo o valor pode ser referenciado por um registro transacional.

**Para aposentar** (nunca apagar — um valor usado no histórico não pode sumir):
1. Muda-se `ativo` de `true` para `false` no item. O item continua no arquivo.
2. Registra-se em `observacao` a data e o motivo da aposentadoria.
3. A tela para de oferecer o item em telas de criação; registros antigos que já o usam
   continuam lendo e exibindo normalmente.

**Nunca:**
- Remover um item da lista `itens`.
- Reescrever o `codigo` de um item que já existia (quebra todo registro histórico que aponta
  para ele).
- Adicionar item sem `ordem` e `ativo` explícitos — o validador barra isso.

## Verificação

```bash
node scripts/dados/validar-referencia.mjs
```

Confere forma do JSON, código duplicado, código com acento/espaço, ordem repetida, descrição
vazia e item sem `ativo`. Só relata — não corrige nada. Ver [PENDENTES.md](PENDENTES.md) para o
que ainda falta preencher.
