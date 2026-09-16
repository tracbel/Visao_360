# Carga dos dados de 2026 do Vórtice — o ensaio da migração

> **Documento 24** · Versão 1.0 · 05/09/2026
> Cobre `src/Tracbel.Crm.Carga` (o comando) e `src/Tracbel.Crm.Integracao/Carga` (a quarentena).
> Executada ao vivo contra o banco de produção do Vórtice, **somente leitura**, com a VPN de pé.
> Todo número deste documento saiu de uma execução real, não de estimativa.

---

## 1. O recorte, e por que ele

**Entra no CRM a pessoa que teve interação em 2026 — agenda ou histórico — numa das treze filiais
em operação.** São **24.218 clientes**.

| Universo | Pessoas | Como foi medido |
|---|---:|---|
| `GE_Pessoa` inteira | 119.348 | volumetria de 03/09/2026 |
| Com atividade em 2026, em qualquer filial | 24.260 | `IV_Historico.DtaRealizacao` ∪ `IV_Agenda.DtaAgenda` em 2026 |
| **Com atividade em 2026, nas 13 filiais em operação** | **24.218** | o recorte desta carga |

### Por que este corte e não outro

**Atividade, e não cadastro.** Trazer as 119.348 pessoas encheria a tela de gente com quem
ninguém fala desde 2011 — e o cadastro do Vórtice não distingue as duas coisas. Interação no ano
corrente é a definição mais próxima de "cliente que existe" que o dado sustenta sem opinião.

**A filial vem da atividade, não do cadastro — porque o cadastro não tem filial.** `GE_Pessoa` não
tem coluna de empresa; quem tem é `IV_Agenda` e `IV_Historico`. Cada pessoa fica na filial em que
mais interagiu no ano, com empate resolvido pelo menor código para a carga ser determinística: sem
esse desempate, rodar de novo moveria clientes de filial sem nada ter mudado na origem.

**As 42 pessoas de fora.** Quarenta e duas só tiveram atividade em Guaíra, Ituverava ou Monte Alto
— as três filiais que o Vórtice ainda marca como ativas e que a lista oficial de 04/09/2026 não
confirma (`dados-referencia/empresa.json`). Trazê-las obrigaria a criar cliente numa filial que a
API não oferece no seletor. Ficam de fora **até o negócio decidir** se fecharam ou foram
incorporadas.

**O volume não precisou de teto.** 24 mil clientes e 19 mil linhas de parque cabem numa carga só —
a execução inteira leva **49 segundos**. O teto (`--limite`) existe para ensaio e não foi usado.

### O de-para de filiais

O código oficial é `0101NN`, em que `NN` é o número da filial no Vórtice. A carga **lê o de-para do
banco**, de `organizacao.Empresa`, e não de um arquivo: desativar uma filial ali faz a carga parar
de trazer cliente para ela sem ninguém precisar lembrar de mexer no código.

---

## 2. O que veio, por entidade

```
entidade            lidas  gravadas
Cliente             24218     23942
Endereco                —     19641
Contato             23669     23313
Equipamento         19147      3888
```

E por filial, contado no banco depois da carga:

| Código | Filial | Clientes | Contatos | Endereços | Equipamentos |
|---|---|---:|---:|---:|---:|
| 010101 | Ribeirão Preto | 10.793 | 8.746 | 8.227 | 2.142 |
| 010102 | Araraquara | 1.836 | 1.679 | 1.392 | 372 |
| 010107 | Orlândia | 1.521 | 1.377 | 1.425 | 276 |
| 010103 | Barretos | 1.376 | 1.412 | 1.326 | 255 |
| 010111 | Franca | 1.323 | 1.661 | 1.084 | 251 |
| 010116 | Votuporanga | 1.258 | 1.266 | 1.187 | 11 |
| 010113 | São José do Rio Preto | 1.010 | 1.334 | 866 | 266 |
| 010117 | Tupã | 992 | 833 | 924 | 5 |
| 010112 | Itápolis | 962 | 1.236 | 707 | 112 |
| 010114 | Catanduva | 929 | 1.244 | 787 | 1 |
| 010115 | Jales | 861 | 988 | 826 | 0 |
| 010109 | Bebedouro | 678 | 1.028 | 553 | 197 |
| 010118 | Marília | 403 | 509 | 337 | 0 |
| | **Total** | **23.942** | **23.313** | **19.641** | **3.888** |

**A coluna de equipamentos é o achado desta tabela.** Ribeirão Preto tem 2.142 máquinas e Jales e
Marília têm zero — não porque não haja máquina lá, mas porque **só algumas filiais preenchem o
chassi** no cadastro de parque do Vórtice. O campo é texto livre e opcional; onde a equipe não tem
o hábito de preenchê-lo, o parque inteiro fica sem identidade. Ver seção 5.

O catálogo de frota também cresceu, a partir do texto que já estava lá:

| Catálogo | Semeado (protótipo) | Nascido do dado real | Total |
|---|---:|---:|---:|
| `frota.Marca` | 7 | **6** | 13 |
| `frota.Familia` | 7 | **15** | 22 |
| `frota.Modelo` | 18 | **235** | 253 |

Não é invenção: marca e modelo **já existem no Vórtice**, em texto livre e sem catálogo por trás —
que é justamente o defeito. A carga promove o valor existente a linha de catálogo. As sete famílias
semeadas chamavam-se "A confirmar" porque ninguém sabia quais eram; agora são Trator, Colhedora de
Cana, Pulverizador, Implemento, Plataforma Adicional, Agricultura Precisão e Turf.

---

## 3. O que foi normalizado, por regra

**15.994 correções de campo**, cada uma registrada com valor de origem, valor entregue e motivo —
o princípio 1.4 do [documento 16](16-HIGIENIZACAO-DE-DADOS.md). Nada foi corrigido em silêncio.

| Campo | Corrigidos | Exemplo (origem → entregue; nome, contato e documento trocados por fictícios do mesmo formato) |
|---|---:|---|
| **documento** | **10.062** | `1933566000139` → `01933566000139` (documento fictício, mesmo formato) |
| nome (do contato) | 1.388 | `FULANO-EXEMPLO ` → `FULANO-EXEMPLO` |
| telefone | 1.296 | `1655550000` → `16955550000` |
| nomeRazao | 619 | `FULANO  DE TAL` → `FULANO DE TAL` |
| logradouro | 518 | `R ALAGOAS,  ` → `R ALAGOAS,` |
| situacao | 461 | `S` → `Suspect` |
| nomeFantasia | 424 | `FAZ. EXEMPLO  VELHA` → `FAZ. EXEMPLO VELHA` |
| inscricaoEstadual | 367 | `123456789 ` → `123456789` |
| municipio | 275 | `SANTA RITA DO  PASSA` → `SANTA RITA DO PASSA` |
| bairro | 259 | `ZONA  RURAL` → `ZONA RURAL` |
| email | 203 | `FULANO.EXEMPLO@EXEMPLO.COM` → `fulano.exemplo@exemplo.com` |
| localizacao | 63 | quebra de linha colapsada em espaço |
| complemento | 46 | espaço duplo colapsado |
| numero | 10 | `149, ` → `149,` |
| tipoLogradouro | 3 | `R ` → `R` |

### O número que importa: 10.062 documentos reconstituídos

**42% dos clientes carregados tiveram o CPF ou o CNPJ remontado** porque `GE_Pessoa` guarda o
documento em **duas colunas numéricas** (`NroCGCCPF` + `DigCGCCPF`) e número não preserva zero à
esquerda. A carga devolve os zeros ao lugar, **confere o dígito verificador** e só então aceita.

É o achado da [qualidade-dados-legado](../extracao-vortice/qualidade-dados-legado.md) deixando de
ser estatística e virando dado utilizável: `AGROPECUARIA EXEMPLO LTDA` (fictício) está no CRM como
`01.933.566/0001-39` (fictício, mesmo formato), com o zero da frente que a coluna numérica do Vórtice tinha comido.

**A reconstituição não é chute, e a prova é que ela pode falhar.** Quando o número remontado não
passa no dígito verificador, o cliente é **recusado** — 190 vezes nesta carga (seção 4).

### 7.328 campos recusados, sem derrubar a linha

Um campo ruim não derruba o registro inteiro: ele sai vazio, com o motivo registrado.

| Campo | Recusados | Por quê, com exemplo real |
|---|---:|---|
| endereco | 4.225 | falta logradouro, município ou UF — `(vazio) / TABATINGA / SP` |
| cep | 2.252 | não tem oito dígitos, ou é um dígito só repetido — `0` |
| telefone | 652 | não forma DDD + 8 ou 9 dígitos |
| atividadeEconomica | 97 | CNAE com mais dígitos do que a coluna aceita — `01156000` (8 dígitos) |
| email | 73 | sem `@`, sem ponto no domínio, ou com espaço |
| uf | 13 | fora das 27 unidades federativas — `**` |
| documento (do contato) | 12 | não reconstitui um CPF válido |
| ano | 4 | campo de texto livre que não é um ano — `> 1997` |

**Endereço pela metade não entra.** Logradouro sem município não localiza ninguém e ocuparia a
única vaga de endereço principal do cliente — a tela mostraria um endereço que não leva a lugar
nenhum, que é pior do que mostrar nenhum.

---

## 4. O que foi recusado, e por quê

**15.906 linhas na fila de descarte**, cada uma em `integracao.MensagemDescartada` com o conteúdo
cru em JSON e o motivo em português. Consultável:

```sql
SELECT Fluxo, Erro, COUNT(*) FROM integracao.MensagemDescartada
WHERE Fluxo LIKE 'VORTICE.CARGA.%' AND TratadaEm IS NULL
GROUP BY Fluxo, Erro ORDER BY 3 DESC;
```

| Linhas | Regra que recusou | Exemplo real |
|---:|---|---|
| **11.419** | Equipamento — máquina sem chassi na origem | coluna `Identificador` vazia |
| **3.367** | Equipamento — chassi fora do padrão VIN de 17 posições | um primeiro nome de pessoa, `Trator`, `DIVERSOS`, `1PO5425XAAT020373` |
| 464 | Equipamento — chassi repetido | `1BM7225JCDH002401` em duas linhas do parque |
| 356 | Contato — cliente dono fora da carga | contato de pessoa recusada ou fora do recorte |
| **190** | Cliente — documento não reconstituível | `F` + base `12345678901` + dígito `99` (fictício, mesmo formato) |
| 82 | Cliente — tipo de pessoa fora de `F`/`J` | 78 com a coluna **nula**, 4 com o valor `0` |
| 15 | Contato — CPF repetido na filial | entrou **sem** documento, não foi descartado |
| 9 | Equipamento — cliente dono fora da carga | máquina de pessoa recusada |
| 4 | Cliente — documento repetido na mesma filial | `12.345.678/0001-00` (fictício, com dígito inválido de propósito) em dois cadastros |

### Os 190 documentos recusados são um achado, não um erro da carga

Abrindo a fila de descarte:

| Situação na origem | Linhas |
|---|---:|
| `FisicaJuridica = 'F'` com base de **11 ou 12 dígitos** (ou seja, um **CNPJ**) | 184 |
| `FisicaJuridica = 'F'` com base de 9 ou 10 dígitos que não passa no dígito verificador | 6 |

Os nomes confirmam: `JOSE SOLERA E OUTROS`, `VALDECIR SONCINI E OUTRO`, `HENRIQUE HILDEBRAND`.
São **condomínios rurais e espólios com CNPJ, cadastrados como pessoa física**. O documento não
está errado — a *marcação de tipo de pessoa* está. A carga não conserta isso porque consertar
significaria decidir, para cada linha, que o dado certo é o número e não o tipo. É trabalho de
negócio, e agora ele tem uma lista de 184 linhas em vez de uma suspeita.

### Sem chassi não há máquina — e é aqui que dói

**Das 19.147 linhas de parque, 14.786 (77%) não entraram por causa do chassi.** O chassi é a
identidade da máquina e a chave de deduplicação ([documento 16](16-HIGIENIZACAO-DE-DADOS.md),
seção 4); a coluna `IV_ClientePropr.Identificador` é **texto livre e opcional**, e guarda de tudo:

- 11.419 vazias;
- 3.014 com um texto que não tem 17 posições — `Nivaldo`, `Romulo`, `Trator`, `TREINAMENTO`,
  `Batedeira de amendoim`, `MF275`;
- 106 com 17 posições mas com caractere fora de A–Z 0–9;
- **247 com 17 posições alfanuméricas, recusadas só por conterem `I`, `O` ou `Q`.**

**As 247 merecem uma decisão, e é a recomendação mais concreta deste documento.** Todas começam
com `1CQ` — `1CQ0319ATA0090434`, `1CQ0607CCD0090031`, `1CQ0620ACH0120252`. É prefixo legítimo de
PIN da John Deere (fábrica de Catalão), e **o padrão VIN de automóvel proíbe I/O/Q, mas o PIN de
máquina agrícola não segue esse padrão**. A regra do tipo `Chassi` está mais estreita do que a
realidade da frota. O mesmo dado prova a confusão que a regra existe para evitar: o parque tem
`1P05425XEAT020989` (com zero) e `1PO5425XAAT020373` (com a letra O) na mesma coluna.

> **Recomendação:** rever a regra de `Chassi` (documento 16, seção 2.11) para aceitar `I`, `O` e `Q`
> em PIN de máquina agrícola — mantendo as 17 posições. Sozinha, essa mudança recupera **247
> máquinas** desta carga e um múltiplo disso na migração completa. **Não foi feita aqui:** é
> decisão de produto sobre uma regra escrita e testada, não conserto de carga.

---

## 5. O que ficou vazio, e quantos

Campo que não existe no legado fica vazio. Nada foi preenchido por dedução.

| Campo | Vazios | De | Por quê |
|---|---:|---:|---|
| `Cliente.OrigemId` | 23.942 | 23.942 | `GE_Pessoa.Origem` é texto livre; mapeá-lo ao catálogo `ORIGEM_LEAD` seria adivinhar |
| `Cliente.AtividadeEconomica` | 23.923 | 23.942 | CNAE quase sempre vazio na origem, e 97 com mais dígitos que a coluna |
| `Cliente.InscricaoEstadual` | 17.159 | 23.942 | vazia na origem |
| `Cliente.NomeFantasia` | 12.183 | 23.942 | vazia na origem |
| `Cliente.Documento` | **8.419** | 23.942 | **sem documento nenhum na origem** — não confundir com os 190 recusados |
| Cliente sem endereço | 4.301 | 23.942 | endereço incompleto na origem (seção 3) |
| `Endereco.Latitude/Longitude` | 15.487 | 19.641 | sem coordenada na origem |
| `Endereco.Cep` | 7.754 | 19.641 | CEP ausente ou inválido |
| `Contato.Documento` | 22.186 | 23.313 | CPF do contato quase nunca preenchido |
| `Contato.Sobrenome` | 13.942 | 23.313 | o legado guarda o nome inteiro numa coluna só; sem espaço, não há corte |
| `Equipamento.AnoFabricacao` | 3.888 | 3.888 | **não existe** no cadastro de parque do legado |
| `Equipamento.Placa` | 3.888 | 3.888 | idem |
| `Equipamento.HorimetroAtual` | 3.888 | 3.888 | idem — o horímetro vive nas OS, e `EXT_OS` está parada |
| `Equipamento.AnoModelo` | 75 | 3.888 | campo de texto livre que não é ano |

Duas atribuições **provisórias e declaradas**, porque as colunas são obrigatórias:

1. **Proprietário.** Todo cliente, contato e máquina entrou sob um único usuário. O legado guarda o
   vendedor como **login em texto** (`IV_Agenda.Vendedor`, 487.038 linhas medidas), e nenhum desses
   logins existe em `seguranca.Usuario`, que hoje tem três usuários de desenvolvimento. A carteira
   real depende da carga de usuários, que é outra frente.
2. **Papel do contato.** Os 23.313 vínculos `ClienteContato` usam um item novo do catálogo
   `PAPEL_CONTATO`: **`NAO_INFORMADO` — "Não informado na origem"**. O legado guarda "tipo de
   contato" como texto livre e não diz se a pessoa decide ou influencia; mapeá-lo para `DECISOR`
   seria afirmar sobre o negócio algo que o dado não sustenta.

---

## 6. Como o comando funciona

**É um projeto de console, `src/Tracbel.Crm.Carga`, e não um endpoint da API.** Três razões:

1. **A leitura leva minutos.** O recorte varre `IV_Historico` (2,4 milhões de linhas) e `IV_Agenda`
   (932 mil). O tempo limite do servidor web, do balanceador e do navegador são todos menores — a
   carga morreria no meio sem ninguém saber onde parou.
2. **A carga precisa de alcance de organização.** Ela grava em treze filiais. Pôr esse poder atrás
   de uma rota HTTP significaria que existe uma URL capaz de atravessar a fronteira de multiempresa
   do [documento 05](05-SEGURANCA.md). Não deve existir. Aqui ele mora num processo com escopo,
   horário e operador conhecidos, num `ContextoAcesso` declarado como **serviço de sistema**.
3. **A migração vai rodar por agendamento, não por clique.** Escrever a carga já no formato em que
   ela vai viver evita reescrevê-la depois.

```bash
$env:Vortice__Conexao = "..."                                   # a credencial NUNCA é versionada
dotnet run --project src/Tracbel.Crm.Carga -- --somente-medir    # passo 1: conta e sai
dotnet run --project src/Tracbel.Crm.Carga -- --recomecar        # zera o cadastro e carrega
dotnet run --project src/Tracbel.Crm.Carga                       # reexecuta, reconciliando
```

### Somente leitura no Vórtice, por construção

Não existe `INSERT`, `UPDATE`, `SELECT INTO` nem chamada de procedimento em
`LeitorDeCargaDoVortice`. Toda consulta usa `WITH (NOLOCK)` e **toda consulta tem filtro** —
nenhuma varre tabela inteira. A lista de filiais entra em **parâmetros**, um por filial; o que é
concatenado no texto da consulta são os nomes dos parâmetros, nunca os valores.

A carga lê `GE_Pessoa`, `GE_Contato`, `IV_ClientePropr`, `IV_Propriedade`, `IV_Agenda` e
`IV_Historico` — todos vivos. **Não toca em `EXT_Veic`, `EXT_NFS`, `EXT_OS` nem `EXT_Titulo`**,
pelo motivo da seção 6.1 do [documento 23](23-API.md): estão paradas, respondem à consulta e
parecem disponíveis.

`GE_PessoaEnd` ficou de fora por medição: a alteração mais recente dela é de **29/07/2024**. O
endereço vem de `GE_Pessoa`, alterada hoje.

### O vocabulário do legado não vaza

A quarentena é `Tracbel.Crm.Integracao/Carga`. De lá para fora saem `ClienteParaCarga`,
`ContatoParaCarga` e `EquipamentoParaCarga` — tipos de valor do domínio (`CpfCnpj`, `Cep`,
`Chassi`) mais a trilha do que foi corrigido e do que foi recusado. Quem consome não sabe, e não
pode saber, que o documento chegou partido em duas colunas numéricas. O teste
`Vocabulario_do_Vortice_fica_confinado_na_camada_de_integracao` continua verde.

### Idempotência: rodar de novo atualiza, não duplica

Cada registro tem uma linha em `integracao.ChaveExterna` ligando a chave de origem ao
identificador do CRM — **70.784 linhas de de-para** (23.942 clientes, 23.313 contatos, 19.641
endereços, 3.888 máquinas). Rodar de novo encontra a linha, atualiza o que mudou e carimba
`SincronizadoEm`.

Prova, com a segunda execução feita **sem** `--recomecar`:

| Tabela | Depois da 1ª | Depois da 2ª |
|---|---:|---:|
| `comercial.Cliente` | 23.942 | **23.942** |
| `comercial.Contato` | 23.313 | **23.313** |
| `comercial.Endereco` | 19.641 | **19.641** |
| `frota.Equipamento` | 3.888 | **3.888** |
| `integracao.ChaveExterna` | 70.784 | **70.784** |
| chaves externas repetidas | 0 | **0** |
| chassis repetidos | 0 | **0** |
| documentos repetidos na filial | 0 | **0** |

> **A primeira reexecução falhou, e o defeito valeu o susto.** A conferência de documento repetido
> existia só no caminho de criação. Na reexecução, um contato que tinha entrado **sem** CPF (porque
> o CPF já era de outro) voltou pelo caminho de reconciliação, que gravava o documento sem
> conferir — e o índice único `UX_Contato_Empresa_Documento` derrubou a transação. Corrigido: a
> conferência agora sabe **quem é o dono** de cada documento, e não apenas se ele já foi usado; na
> reconciliação, o registro mantém o documento que já tinha. **Uma carga que nunca foi executada
> duas vezes não é idempotente — é não testada.**

### Transação por bloco, e progresso

Blocos de 500, cada um numa transação. Uma transação de 24 mil linhas seguraria bloqueio por
minutos e perderia tudo num erro no fim; um bloco que falha desfaz só ele, e a execução seguinte
retoma pelo de-para. O progresso sai a cada bloco.

### A marca de sincronismo

Quatro linhas em `integracao.PontoDeSincronismo` — a tabela que teria alarmado em 11/04/2025:

| Fluxo | Até onde leu | Lidos | Gravados | Recusados |
|---|---|---:|---:|---:|
| `VORTICE.CARGA.CLIENTE` | 2026-09-04T21:26:15 | 24.218 | 23.942 | 272 |
| `VORTICE.CARGA.ENDERECO` | 2026-09-04T21:26:15 | 24.218 | 19.641 | 272 |
| `VORTICE.CARGA.CONTATO` | 2026-09-03T11:56:24 | 23.669 | 23.313 | 0 |
| `VORTICE.CARGA.EQUIPAMENTO` | 2026-09-04T16:36:06 | 19.147 | 3.888 | 14.786 |

Os três contadores andam juntos de propósito. Gravar só o que entrou contaria metade da história —
que é a metade que faltava no Vórtice.

---

## 7. O que quebrou visualmente com o dado real

Capturado com Playwright, 1440×900, com a carga dentro. **Zero erro de console em todas as telas.**
Nenhum destes foi consertado nesta etapa.

| # | O que se vê | Onde | Diagnóstico |
|---|---|---|---|
| 1 | Máquinas **`Baixado`** aparecem com "Mostrar baixados" **desmarcado** | `/equipamentos` | 🔴 **Defeito de tela.** O filtro olha a exclusão lógica (`ExcluidoEm`); a coluna mostra a **situação**. A carga traz 490 máquinas `Baixado` sem excluí-las logicamente (o parque histórico é informação), e as duas ideias divergem na cara do usuário. São 326 das 2.142 de Ribeirão Preto. |
| 2 | O seletor **Modelo** virou uma lista plana de **253 itens** | `/equipamentos` | 🟡 Consequência direta do catálogo crescer do dado real. Sem agrupar por marca ou família, o campo fica impraticável — e na migração completa serão milhares. |
| 3 | Segunda linha do cliente repete o nome **cortado em 20 caracteres** | `/clientes` | 🟡 **Não é a tela: é o dado.** `AGROPECUARIA EXEMPLO DOCE LTDA` / `AGROPECUARIA EXEMPLO` (fictício, mesmo corte). 3.694 clientes carregados têm nome fantasia com exatamente 20 caracteres, e 1.399 têm fantasia idêntica ao nome. Lê-se como defeito de renderização. Ver seção 8. |
| 4 | Nomes que são **um documento** ou **`**INATIVO**`** | `/clientes` | 🟡 Dado real: `12.345.678/0001-00` e `12345678900` como razão social (fictícios, com dígito inválido de propósito). O CRM não tem regra que recuse (documento 16, seção 2.1, prevê "só dígito, sem letra" — este passa porque tem pontuação). |
| 5 | Coluna DOCUMENTO com **—** em 8.419 clientes | `/clientes` | 🟢 Correto e legível. É o vazio honesto da seção 5. |
| 6 | Nome longo do cliente **quebra em duas linhas** e a linha da tabela cresce | `/equipamentos` | 🟢 Não quebra o layout. `FULANO DE TAL EXEMPLO DA SILVA E OUTROS` (fictício; o nome real tem 40 caracteres, truncado na origem). |
| 7 | Paginação, busca e documento formatado | ambas | 🟢 **Funcionam.** `1–25 de 2142 equipamentos · Página 1 de 86`; busca por `agropecuaria` devolve 70 de 10.793; busca por chassi completo devolve 1 de 1; CPF sai `123.456.789-00` e CNPJ `12.345.678/0001-00` (fictícios, com dígito inválido de propósito). |
| 8 | `GET /saude/banco` responde **500** | API | 🔴 **Pré-existente, não relacionado ao dado.** O endpoint resolve o `CrmDbContext` antes do meio de campo de contexto rodar, e o `ContextoAcessoDaRequisicao` lança — como foi desenhado para lançar. É a ordem no `Program.cs`. Os demais endpoints respondem 200. |

---

## 8. O que isto ensina para a migração completa

**1. A reconstituição do documento funciona, e é o maior ganho isolado.** 10.062 documentos
remontados com dígito verificador conferido, 190 recusados. Extrapolando para as 119.348 pessoas do
Vórtice, são da ordem de **50 mil documentos recuperados** — e a recuperação é verificável, porque o
dígito verificador confere ou não confere.

**2. O tipo de pessoa é menos confiável que o documento.** As 184 pessoas físicas com CNPJ mostram
que, quando os dois discordam, o **número** costuma estar certo e a **marcação** errada. A migração
completa deve tratar "F com 12 dígitos de base" como *pessoa jurídica a confirmar*, e não como
documento inválido — hoje ela vira recusa.

**3. O parque de máquinas não migra pelo chassi sozinho.** 77% das linhas de parque não têm chassi
utilizável. Três decisões precisam vir antes da migração completa, nesta ordem de retorno:
   - **rever a regra de I/O/Q** (recupera 247 máquinas aqui, e o PIN John Deere é o padrão da frota);
   - decidir o que fazer com as **11.419 máquinas sem chassi** — entram com identidade sintética e
     marcadas, ou não entram? Hoje não entram, e isso significa que a tela de Cobertura enxerga
     menos parque do que o CEN declarou;
   - conciliar com o **Protheus** ([documento 18](18-INTEGRACAO-PROTHEUS.md)), que é dono do ativo
     faturado e tem o chassi certo do que a Tracbel vendeu.

**4. Existem duas marcas d'água de truncamento novas, e são grandes.** Medidas ao vivo em
`GE_Pessoa`:

| Coluna | Tipo | Linhas com o tamanho exato | Contexto |
|---|---|---:|---|
| `Fantasia` | `varchar(50)` | **13.787 com exatamente 20** | 16.904 na faixa de 19 a 21 — o balde de 20 sozinho tem 82% da vizinhança |
| `NomeRazao` | `varchar(100)` | **5.371 com exatamente 40** | apenas **251** linhas passam de 40 em todo o cadastro |

A coluna permitia 50 e 100; **quem cortou foi a aplicação**, em 20 e em 40. É o mesmo padrão do
achado 9.2 do [documento 01](01-DIAGNOSTICO-VORTICE.md) (`IV_Historico.ResultadoCmpl` cortado em 20
num campo de 150), agora medido em mais duas colunas do cadastro que a tela de Clientes exibe. **O
texto cortado não volta** — a migração completa deve migrar o que sobrou e marcar a linha, nunca
fingir que o nome sempre foi curto.

**5. A filial precisa vir da atividade, e isso tem custo.** Como `GE_Pessoa` não tem coluna de
empresa, cada carga paga uma varredura de `IV_Agenda` + `IV_Historico`. Funciona em 49 segundos
para um ano; para o cadastro inteiro será a operação mais cara da migração. Vale materializar o
de-para pessoa→filial uma vez, em vez de recalculá-lo a cada rodada.

**6. Catálogo tem de nascer antes do dado, não junto.** 235 modelos e 15 famílias nasceram durante
esta carga porque não havia de onde tirá-los. Deu certo e é determinístico, mas o resultado é um
catálogo com o vocabulário do legado dentro — `TR5060EN`, `JD 5055 E`, `MF283`. Antes da migração
completa, alguém do negócio precisa **revisar e consolidar** essa lista; adiar isso significa levar
a bagunça de nomenclatura do Vórtice inteira para o CRM novo.

**7. A carga só é idempotente porque foi executada duas vezes.** O defeito da seção 6 não apareceu
em teste nem em revisão: apareceu na segunda execução, e só na segunda. Toda migração parcial
daqui em diante deve ter a reexecução como passo obrigatório de verificação — não como cortesia.

---

## 9. Verificação, com saída real

| Verificação | Resultado |
|---|---|
| `dotnet build` | **0 aviso, 0 erro** |
| `dotnet test` | **277 testes, 277 passando** (143 domínio · 62 arquitetura · 44 aplicação · 15 API · 13 integração) |
| Carga completa | **49 s**, 24.218 lidas, 23.942 clientes gravados |
| Reexecução sem `--recomecar` | **contagens idênticas**, zero duplicata |
| Consulta no banco | contagem por tabela e por filial nas seções 2 e 6 |
| Telas com Playwright | 10 capturas, **zero erro de console**, achados na seção 7 |

O cliente de teste que existia antes (`Fazenda Verificacao 93152 Ltda`) foi removido por
`--recomecar`. **Tudo o que está no banco hoje veio do Vórtice.**
