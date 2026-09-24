# De onde vem cada dado — arquivos, tabelas e consultas

> Documento 29 · Versão 1.0 · 08/09/2026
> Responde a três perguntas: **qual arquivo** faz a leitura, **de quais tabelas** ela puxa, e
> **qual é a consulta**. Serve para auditar um número da tela até a origem dele.
> Convenção: `[medido]` = valor obtido ao vivo contra a produção em 08/09/2026.

---

## O caminho, em uma frase

Dois sistemas de origem alimentam o CRM. O **Vórtice** (SQL Server, leitura direta) traz cliente,
carteira, processo, agenda e o formulário de venda perdida. O **Protheus** (API REST) traz o
faturamento. Nada é escrito em nenhum dos dois — **toda leitura é somente leitura**, e no Protheus
apenas `GET`.

---

## 1. Protheus — o faturamento

**Arquivo:** `src/Tracbel.Crm.Integracao/Protheus/LeitorDeFaturamentoDoProtheus.cs`
**Carga:** `src/Tracbel.Crm.Carga/CargaDeFaturamentoDoProtheus.cs` (`--somente-faturamento [--simular]`)

> **Atualização de 24/09/2026 — a leitura saiu da API REST e vai direto ao banco do Protheus**
> (SQL Server, `ApplicationIntent=ReadOnly`, `SELECT ... WITH (NOLOCK)`, conexão "Protheus — banco
> (leitura)"). O `GROUP BY` por filial, mês, cliente e grupo roda no próprio banco do ERP, e chegam só
> os agregados: três anos em menos de um minuto. As regras abaixo (tipo `N`, lista de CFOP, grupo)
> são as mesmas; as consultas exatas estão em `LeitorDeFaturamentoDoProtheus.ConsultaDaVenda` e
> `ConsultaDoResumo`. O que segue sobre `genericQuery` e `tenantId` é o histórico da leitura REST.
> Filial: `D2_FILIAL`, confirmada no banco com as dezesseis filiais (ver o comentário da classe).

### 1.1 `SA1` — cadastro de clientes

Traduz o código do Protheus para CNPJ. Lida **uma vez**, sem filtro.

```
tables=SA1
fields=A1_COD,A1_LOJA,A1_CGC,A1_NOME
```

`[medido]` 38.682 linhas, das quais **38.674 com documento válido**.

**A chave é código + loja, não só o código.** O cliente `008096049` tem as lojas 0001, 0004 e 0008
com **três CNPJs diferentes** — matriz e filiais do mesmo grupo. Casar só por código atribuiria a
nota da filial ao CNPJ da matriz.

**A `SA1` é compartilhada entre as filiais** `[medido]`: `A1_FILIAL` vem vazio e o total é idêntico
em 010101, 010107 e 010113. Por isso é lida uma vez, e não dezesseis.

### 1.2 `SD2` — itens de nota fiscal de saída

É o faturamento. Lida **uma vez por filial**.

```
tables=SD2
fields=D2_FILIAL,D2_EMISSAO,D2_CLIENTE,D2_LOJA,D2_GRUPO,D2_TOTAL,D2_DOC,D2_CF,D2_TIPO
where=D2_EMISSAO >= 'AAAAMMDD'          -- três anos para trás
cabeçalho tenantId: 01,<filial>          -- 010101, 010102, … 010118
```

**O `tenantId` não é opcional.** Sem ele o AppServer responde pela filial padrão do nó, e esse
padrão **muda sozinho** `[medido]`: numa manhã devolvia 010101 (Ribeirão Preto) e à tarde 010116
(Votuporanga) — mesma consulta, mesma credencial, empresa diferente, sem erro nenhum. Foi assim que
este projeto concluiu, por um tempo, que a Agro faturava por uma filial só.

`[medido]` 16 filiais, **1.336.589 itens** desde 08/09/2023.

**Três filtros são aplicados sobre cada linha, e é aqui que mora a diferença entre "saiu pela
porta" e "é venda":**

| Filtro | Regra | O que corta `[medido]` |
|---|---|---|
| Tipo | `D2_TIPO = 'N'` | Devolução (`D`) e beneficiamento (`B`) |
| CFOP | lista de **inclusão** — x102, x108, x117, x403, x405, x656, x659, x933 | 395.516 itens, **R$ 2,79 bi** em transferência entre filiais, remessa de demonstração, retorno de conserto e baixa de estoque |
| Valor | mês do cliente tem de fechar `> 0` | Encontro de venda com devolução |

A lista de CFOP é **de inclusão, não de exclusão**, de propósito: um CFOP novo que ninguém
classificou fica de fora e aparece no relatório da carga, em vez de entrar como venda e inflar o
número em silêncio. O erro passa a ser **para menos, e visível**.

> Pendência: o CFOP **5949** ("outra saída não especificada") são R$ 38,1 mi que ficam de fora
> **sem sabermos o que são**. Precisa da resposta do fiscal.

### 1.3 A quebra máquina × peça × serviço

Sai de **`D2_GRUPO`**, que viaja na própria linha da nota e aponta para o catálogo **`SBM`** (273
grupos). Não é preciso juntar com a `SB1`, que tem 517.855 linhas.

| Grupo | É |
|---|---|
| `VEIC` | Máquina |
| `1001` … `10xx` | Peça, uma faixa por linha de produto |
| `SRV`, `MO_O`, `MO_T` | Serviço e mão de obra |
| o resto | "Outros" — ver ressalva |

**Ressalva aberta.** A varredura dos grupos mostrou que o CFOP de serviço carrega muita coisa que
não é serviço de oficina: `COM` COMISSOES VENDA DIRETA, `ICV` INCENTIVOS SOBRE VENDAS, `P4P` BONUS
PERFORMANCE, `LOC` LOCACAO, `ATV` ATIVACOES/LICENSAS. **Isso é receita da fábrica, não venda ao
cliente**, e hoje cai em "outros". É a razão de a John Deere aparecer como se fosse cliente.

### 1.4 Tabelas do Protheus lidas só para investigar

Não entram na carga; existem nos scripts de `scripts/protheus/`.

| Tabela | Para quê |
|---|---|
| `SX2` | Lista de tabelas do dicionário, com descrição |
| `SX3` | Campos de cada tabela — evita chutar nome de coluna |
| `SBM` | Catálogo dos grupos de produto |
| `VV1` | Frota — 38.360 chassis `[medido]` |

---

## 2. Vórtice — cadastro, carteira, processo e agenda

**Arquivos:**

| Arquivo | O que lê |
|---|---|
| `Carga/LeitorDeCargaDoVortice.cs` | Cliente, endereço, contato, equipamento, usuário, carteira, município |
| `Carga/LeitorDeCargaDoVortice.Processo.cs` | Processo, tarefa, interação, fase, resultado |
| `Carga/LeitorDeCargaDoVortice.VendaPerdida.cs` | O formulário de venda perdida |

Dezessete métodos de leitura, um por assunto: `LerClientesAsync`, `LerContatosAsync`,
`LerEnderecos…`, `LerEquipamentosAsync`, `LerUsuariosAsync`, `LerCarteirasAsync`,
`LerVinculosDeCarteiraAsync`, `LerMunicipiosDeCarteiraAsync`, `LerLinhasDeNegocioAsync`,
`LerProcessosAsync`, `LerTarefasAsync`, `LerInteracoesAsync`, `LerFasesAsync`,
`LerResultadosAsync`, `LerTiposDeProcessoAsync`, `LerTiposDeTarefaAsync`,
`LerVendasPerdidasAsync`.

### 2.1 As tabelas de origem

| Tabela | O que é | Alimenta |
|---|---|---|
| `GE_Pessoa` | Pessoa — cliente, contato e prospect na mesma tabela | `comercial.Cliente` |
| `GE_Contato` | Contato da pessoa | `comercial.ClienteContato` |
| `GE_Cidade` | Município | `organizacao.Municipio` |
| `GE_Usuario` | Usuário do sistema | `seguranca.Usuario` |
| `IVS_Carteira` | Carteira comercial | `organizacao.Carteira` |
| `IVS_CartCid` | Cidades de cada carteira | `organizacao.CarteiraMunicipio` |
| `IVS_Pes` | Vínculo pessoa × carteira | `comercial.ClienteCarteira` |
| `IVS_Depto` | Departamento — vira linha de negócio, **e traz a cadência** | `organizacao.LinhaDeNegocio` |
| `IV_Processo` | Oportunidade | `processo.Processo` |
| `IV_CodProcesso` | Tipo de processo | `processo.TipoDeProcesso` |
| `IV_Acao` | Tarefa | `processo.Tarefa` |
| `IV_Agenda` | Agenda | `processo.Tarefa` |
| `IV_Historico` | Interação registrada | `processo.Interacao` |
| `IV_Resultado` / `IV_ProcResultado` | Resultado e desfecho | `processo.Resultado` |
| `IV_ProcDado` | Dados do processo | `processo.Processo` |
| `IV_Propriedade` / `IV_ClientePropr` | Propriedade rural do cliente | `comercial.Cliente` |
| `IV_VENDEDOR` | Vendedor | `seguranca.Usuario` |
| `IV_Questionario` | Cabeçalho do formulário | `processo.VendaPerdida` |
| `IV_Q_VENDA_PERDIDA` | Formulário de venda perdida — geração antiga | `processo.VendaPerdida` |
| `IV_Q_VENDA_PERDIDA_FY25` | Mesma coisa, geração FY25 | `processo.VendaPerdida` |
| `IV_Q_VENDA_PERDIDA_MAQIMP` | Mesma coisa, máquina importada — **nomes de coluna diferentes** | `processo.VendaPerdida` |

**A cadência estava sendo lida e jogada fora.** `IVS_Depto` traz `CicloA`..`CicloD` — os dias entre
visitas por classe de cliente. Os valores reais: Venda de Máquinas 180/180/180/360, Prospecção
120/120/120/180, AMS e Peças 360 nos quatro.

**Por que a venda perdida são três tabelas.** O motivo da perda **não está no processo** — está no
motor de questionário, e cada geração do formulário criou uma tabela com nomes de coluna próprios
(`REVENDA_VP`, `QUANTIDADE_VP`, `MODELO_JOHN_DEERE_VP`…). A consulta é uma `UNION` das três, unida
ao `IV_Questionario`.

### 2.2 As CTEs — nomes que parecem tabela e não são

Aparecem nas consultas e **não existem no banco de origem**: `Recorte`, `Consolidado`,
`Classificada`, `Predominante`, `Posse`, `Efeito`, `Atividade`, `Conclusao`, `Presenca`,
`Respostas`, `Usados`, `Usos`.

`Recorte` é a mais importante: é ela que aplica o corte da carga — atividade no ano escolhido e
filial dentro da lista de empresas em operação.

---

## 3. O que a carga escreve

Trinta tabelas do nosso banco, agrupadas pelo que respondem:

| Assunto | Tabelas |
|---|---|
| Cliente | `Clientes`, `Enderecos`, `Contatos`, `ClienteContatos`, `Equipamentos` |
| Organização | `Carteiras`, `ClienteCarteiras`, `CarteiraMunicipios`, `Municipios`, `LinhasDeNegocio` |
| Processo | `Processos`, `Tarefas`, `Interacoes`, `Fases`, `Resultados`, `TiposDeProcesso`, `TiposDeTarefa`, `MotivosDePerda`, `VendasPerdidas` |
| Faturamento | `FaturamentoDosClientes`, `FaturamentoSemClientes` |
| Catálogo | `Catalogos`, `CatalogoItens`, `Marcas`, `Modelos`, `Familias` |
| Rastreabilidade | `Sistemas`, `ChavesExternas`, `PontosDeSincronismo`, `MensagensDescartadas` |
| Segurança | `Usuarios` |

**`ChavesExternas` é o que torna a carga reexecutável:** ela guarda a chave do registro na origem,
então rodar de novo **reapura** em vez de duplicar.

---

## 4. As duas tabelas novas de faturamento

### `comercial.FaturamentoDoCliente`

Grão: **cliente × filial × mês**. Além do total, guarda a quebra — `ValorEmMaquina`, `ValorEmPeca`,
`ValorEmServico`, `ValorEmOutros`. Uma restrição do banco (`CK_FaturamentoDoCliente_QuebraFecha`)
exige que as quatro parcelas somem o total, para que um grupo novo do ERP não suma da quebra em
silêncio.

`[medido]` R$ 1.696,7 mi · 3.805 clientes · 16 filiais · set/2023 a set/2026.
Máquina R$ 897,4 mi · peça R$ 698,9 mi · serviço R$ 78,8 mi · outros R$ 21,3 mi.

### `comercial.FaturamentoSemCliente`

O faturamento que **não achou cliente no CRM** — antes descartado em silêncio.

`[medido]` R$ 798,6 mi em 4.789 CNPJs:

| Natureza | CNPJs | Valor |
|---|---:|---:|
| `ClienteNaoCadastrado` | 4.766 | R$ 573,4 mi |
| `Fabrica` (John Deere) | 7 | R$ 180,8 mi |
| `EmpresaDoGrupo` (Tracbel) | 16 | R$ 44,4 mi |

A natureza é classificada **pela raiz do CNPJ**, não pelo nome: "TRACBEL AGRO NOR." e "TRACBEL AGRO
NORTE" são a mesma empresa escrita de dois jeitos, a raiz `09507371` é uma só.

**O que isso permite dizer:** o CRM gerencia **68%** do que a empresa vende. Sem esta tabela, a
tela mostraria R$ 1,70 bi como se fosse o total.

---

## 5. Consultas que a tela faz

Não vão à origem — leem o nosso banco.

| Arquivo | Responde |
|---|---|
| `Persistencia/Repositorios/RepositorioDeFaturamento.cs` | Série de 12 meses, top clientes, competência mais recente |
| `Persistencia/Repositorios/RepositorioDeCarteiras.cs` | Cobertura por faixa de dias, CENs, mix por linha |
| `Persistencia/Repositorios/RepositorioDoPainelDoCen.cs` | Cobertura **pela cadência declarada**, por responsável |
| `Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs` | Orquestra as anteriores para os endpoints |

**Nenhum repositório escreve um `Where` de empresa.** A fronteira de filial entra sozinha, por
filtro global, em toda entidade que tenha `EmpresaId` — é o que impede um vazamento entre filiais
passar despercebido numa consulta nova.

**Divergência conhecida:** a rosca "Status da cobertura" usa faixas de **30 e 90 dias**, que são
arbitrárias, enquanto o painel do CEN usa a **cadência declarada** (180/120/360). As duas telas
respondem diferente para o mesmo cliente. A rosca é a que está errada.
