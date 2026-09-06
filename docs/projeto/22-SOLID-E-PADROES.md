# SOLID e padrões de projeto — CRM Tracbel

> Documento normativo. Estende o [03-ARQUITETURA](03-ARQUITETURA.md) — não o substitui: aquele
> descreve **as camadas, o motor de workflow e as convenções**; este descreve **por que cada peça
> está onde está**, em termos dos cinco princípios, e qual teste impede que ela saia do lugar.
> O vocabulário é o do [15-GLOSSARIO-E-NOMES](15-GLOSSARIO-E-NOMES.md); o modelo de dados, o do
> [17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md); a disciplina de banco, a do
> [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md).
>
> **Formato, igual ao do documento 14: princípio → o que significa aqui → como se aplica no nosso
> código, com trecho real → o defeito do Vórtice que ele evita, com número → como se verifica.**
> Princípio sem teste automatizado é **recomendação**, não padrão, e está marcado como tal.
>
> Os testes que aplicam este documento vivem em
> [`tests/Tracbel.Crm.Arquitetura.Testes/SolidTestes.cs`](../../tests/Tracbel.Crm.Arquitetura.Testes/SolidTestes.cs)
> e em [`ArquiteturaTestes.cs`](../../tests/Tracbel.Crm.Arquitetura.Testes/ArquiteturaTestes.cs).
> Eles **citam a seção exata** deste documento nas mensagens de falha, então os números de seção
> daqui são contrato com aquele código: não renumere uma seção sem atualizar as strings de
> `Should().BeEmpty(...)`, e vice-versa.

---

## 1. Por que este documento existe

O pedido é direto: o CRM novo tem de nascer sobre SOLID. A resposta honesta é que **ele já
nasceu** — a arquitetura do documento 03 é uma aplicação dos cinco princípios, e quatro testes já
barram violação hoje. O que faltava era três coisas:

1. **Tornar explícito** onde cada princípio está no *nosso* código. O artigo que originou este
   pedido usa NestJS e TypeScript; nosso backend é .NET com C#. Os princípios são idênticos, a
   mecânica não. Um documento que repita o `OrderRepository` do artigo não serve para nada —
   este mostra `IRepositorioRegras`, `MotorWorkflow` e `SanitizadorLeadExterno`.
2. **Achar onde o princípio é violado na prática.** Está na seção 11, com arquivo, linha e
   gravidade.
3. **Fechar as lacunas com teste.** Nove testes novos, na seção 12.

### 1.1 O argumento, em números

Cada princípio abaixo tem um preço medido no legado. A extração ao vivo de 02/09/2026
([relatório](../extracao-vortice/00-RELATORIO-EXTRACAO.md)) e a pesquisa
[16](../pesquisa/16-vortice-ponta-a-ponta-ciclo-de-vida-pessoa-processo-agenda-historico.md)
mediram, num banco de 767 tabelas e 86,3 milhões de linhas:

| Princípio | O que o Vórtice paga por não tê-lo | Seção |
|---|---|---|
| Responsabilidade única | a mesma regra mora em **três lugares**: p-code Gupta (sem descompilador público), **196 SQLs** dentro de `GE_ObjDinamico` e **70 das 85** procedures que escrevem em tabela | 3 |
| Aberto/fechado | **5.958 regras** para **1.262 pares distintos**, replicadas por **18 filiais**: mudar um passo de processo exige 18 edições | 4 |
| Substituição de Liskov | `IVS_Carteira.SeqUrSuperv` é NULL em **655 de 655** carteiras — uma rota de visibilidade que o sistema declara ter e não tem | 5 |
| Segregação de interface | `IV_AcaoAuto` tem **31 colunas, 9 delas 100% nulas**: toda regra paga o preço do que não usa | 6 |
| Inversão de dependência | **zero** ferramenta enxerga a dependência entre p-code, SQL em coluna e procedure — ninguém consegue estimar o efeito de mudar nada | 7 |

---

## 2. Como ler este documento

Cada seção declara a regra, mostra o trecho real do nosso código, dá o número do Vórtice que
justifica a regra, e diz como ela é verificada — por `dotnet test`, por revisão de PR, ou por
nada (e nesse caso diz que é recomendação).

Três marcadores, os mesmos do resto da documentação:

- `[V]` — lição medida no Vórtice.
- `[SF]` — lição do Salesforce (documento 02).
- `[DYN]` — lição do Dynamics 365 / Dataverse (documento 02).

E uma advertência que vale para o documento inteiro: **dividir demais é um defeito, não uma
virtude.** A seção 10 é sobre isso, com exemplo nosso.

---

## 3. Responsabilidade única

> Uma classe tem **uma razão para mudar**. No nosso caso a formulação útil é: *uma peça responde
> a um interlocutor*. A entidade responde ao negócio. O caso de uso responde ao fluxo. O
> repositório responde ao banco. O sanitizador responde à origem do dado. Quando duas dessas
> pessoas pedem mudança no mesmo arquivo, o arquivo está errado.

### 3.1 A separação em camadas é o princípio na escala do projeto

As cinco camadas do documento 03, seção 1, são responsabilidade única aplicada ao repositório
inteiro:

| Projeto | A única razão para mudar | Tipos hoje |
|---|---|---|
| `Tracbel.Crm.Dominio` | uma regra de negócio da Tracbel mudou | 51 arquivos |
| `Tracbel.Crm.Aplicacao` | um fluxo de trabalho mudou | **0** — ver seção 11.5 |
| `Tracbel.Crm.Infraestrutura` | a tecnologia mudou (banco, fila, identidade) | 12 arquivos |
| `Tracbel.Crm.Integracao` | um sistema externo mudou (Vórtice, Protheus, RD Station) | 2 arquivos |
| `Tracbel.Crm.Api` | o contrato HTTP mudou | 1 arquivo |

> **Por quê:** `[V]` no Vórtice não há camada. A regra de negócio está simultaneamente em três
> lugares, e nenhum deles é testável:
>
> - **No p-code Gupta.** O compilador do OpenText Team Developer 7.3.4 gera bytecode
>   proprietário empacotado em 28 executáveis. **Não existe descompilador público**
>   ([inventário de binários](../extracao-vortice/binarios/INVENTARIO-BINARIOS.md), seção 4.1) —
>   o maior deles tem 14,7 MB de p-code, e quem o executa é uma DLL de 8,9 MB.
> - **Em 196 comandos SQL gravados dentro do banco**, na coluna de `GE_ObjDinamico` (o maior
>   com 1.788 caracteres, **168 do tipo `TESTE`**), com nome de tabela resolvido em tempo de
>   execução — **125 dos 196 dependem de tabelas físicas geradas por formulário**
>   (relatório de extração, achado 12).
> - **Em 70 das 85 stored procedures**, que escrevem em tabela. As 51 functions escalares não
>   escrevem — a assimetria é o que sobra de disciplina.
>
> O efeito composto: **551 módulos programáveis diferentes**, dos quais **129 existem apenas em
> produção** e **372 têm texto divergente** entre ambientes (364 views, 6 procedures, 1 function
> e o único trigger do banco). Não é possível saber o que roda em produção lendo o código de
> homologação.
>
> **Como se verifica:** `ArquiteturaTestes.Dominio_nao_depende_de_infraestrutura_nem_de_banco`
> e `SolidTestes.As_referencias_de_projeto_apontam_sempre_para_dentro` (seção 7.2).

### 3.2 A entidade responde ao negócio, e só

`Lead` (`src/Tracbel.Crm.Dominio/Crm/Lead.cs`) não sabe o que é HTTP, banco, JSON ou fila. Ela
sabe o que é um lead válido e quais transições existem:

```csharp
public void Qualificar(long clienteId, long contatoId, long? processoId, long usuarioId)
{
    if (Situacao == SituacaoLead.Qualificado)
        throw new RegraDeNegocioViolada("Este lead já foi qualificado.");

    if (Situacao == SituacaoLead.Descartado)
        throw new RegraDeNegocioViolada(
            "Lead descartado não pode ser qualificado. Reabra o lead antes.");

    Situacao = SituacaoLead.Qualificado;
    // ... carimba autoria, liga aos registros gerados ...

    RegistrarEvento(new LeadQualificado(
        ChavePublica, clienteId, contatoId, processoId, LinhaNegocioId, usuarioId, DateTime.UtcNow));
}
```

Não existe caminho de código que pule essa verificação, porque **não existe setter público de
`Situacao`**. A tela, o job e a integração passam todos por aqui.

> **Por quê:** `[V]` sem esse fechamento, o legado acumulou **345.535 linhas órfãs** e processos
> cancelados sem motivo registrado, e a explosão de vocabulário que o documento 14 mede:
> `FINALIZADO` (18.416 linhas) convivendo com `FINALIZADA` (11.563), `CANCELADO` (41.390) com
> `CANCELADA` (2.412), e **437.694 processos com status em branco** — em 767 tabelas com
> **zero `CHECK CONSTRAINT`**. Quando qualquer código pode escrever qualquer coisa, o domínio de
> valores é o que a digitação livre deixou.
>
> **Como se verifica:** `ArquiteturaTestes.Entidades_nao_expoem_setter_publico`.

### 3.3 O que a entidade devolve também é fechado

Propriedade sem setter não basta se o que ela devolve é uma `List` que qualquer um pode limpar.
`EntidadeBase` é o padrão a copiar:

```csharp
private readonly List<IEventoDominio> _eventos = [];
public IReadOnlyCollection<IEventoDominio> Eventos => _eventos.AsReadOnly();
protected void RegistrarEvento(IEventoDominio evento) => _eventos.Add(evento);
```

> **Como se verifica:** `SolidTestes.Entidades_nao_expoem_colecao_mutavel`. Hoje passa em toda a
> base — o teste existe para o dia em que alguém acrescentar `public List<Contato> Contatos`
> numa entidade nova.

### 3.4 A API recebe, delega e traduz

O endpoint não decide nada. Ele converte HTTP em comando e `Resultado<T>` em código de status:

```csharp
var resultado = await _mediator.Send(new QualificarLeadComando(...), ct);

// Erro de negócio NUNCA vira 500. Vira 409 com mensagem que o usuário entende.
return resultado.EhSucesso ? Ok(resultado.Valor) : Conflict(resultado.ParaProblemDetails());
```

> **Por quê:** `[V]` a API do Vórtice devolve `{"Message":"An error has occurred."}` tanto para
> telefone com DDI quanto para falha real de infraestrutura. Ninguém consegue diagnosticar nada,
> porque a camada que responde é a mesma que decide, e ela não distingue os dois casos.
>
> **Como se verifica:** `SolidTestes.A_api_nao_decide_regra_de_negocio` — parcialmente. O teste
> barra os três sintomas inequívocos (a API declarar `RegraDeNegocioViolada`, instanciar
> `MotorWorkflow`/`Autorizador` à mão, ou chamar `RegistrarEvento`). Ele **não** mede "quanta
> regra existe num endpoint", porque para isso não há heurística honesta — ver seção 11.4.

---

## 4. Aberto para extensão, fechado para modificação

> Acrescentar comportamento **não deve exigir alterar código existente**. No nosso caso a forma
> concreta disso é: o ponto de extensão é uma **linha de tabela** mais uma classe nova.

### 4.1 O motor de workflow é declarativo por decisão, não por elegância

`MotorWorkflow` (`src/Tracbel.Crm.Dominio/Workflow/MotorWorkflow.cs`) não conhece nenhum efeito.
Ele recebe **todos** por injeção e escolhe pelo nome gravado na linha da regra:

```csharp
public sealed class MotorWorkflow(
    IRepositorioRegras regras,
    IAvaliadorCondicao avaliador,
    IEnumerable<IEfeitoRegra> efeitos,     // todas as implementações, via DI
    IRegistroExecucaoRegra registro,
    IRelogio relogio)
```

```csharp
var efeito = _efeitos.FirstOrDefault(e => string.Equals(e.Nome, regra.Efeito, StringComparison.Ordinal));

if (efeito is null)
{
    // A configuração aponta para efeito que não existe no código. É erro de operação, e grita.
    var motivo = $"Efeito '{regra.Efeito}' não tem implementação registrada.";
    await GravarAsync(regra, contexto, correlacaoId, ResultadoRegra.Erro, motivo, inicio, ct);
    relatorio.RegistrarFalha(regra.Codigo, motivo);
    continue;
}
```

Repare no que **não** existe ali: um `switch (regra.Efeito)`. Acrescentar o efeito
"AtribuirCarteira" é criar `EfeitoAtribuirCarteira : IEfeitoRegra` e inserir uma linha em
`processo.Regra`. **Arquivos existentes alterados: zero.** É o guia de 6 passos do documento 03,
seção 11.

O mesmo desenho aparece três vezes:

| Ponto de extensão | Contrato | Registro |
|---|---|---|
| Efeito de regra | `IEfeitoRegra` | `processo.Regra.Efeito` |
| Estratégia de destinatário | `IEstrategiaDestinatario` | expressão em `processo.Regra.ExpressaoDestinatario` |
| Gancho de evento | `TratadorDeEvento` (`metadado/`) | linha em `metadado.TratadorDeEvento` |
| Campo novo numa entidade | `CampoPersonalizado` (`metadado/`) | linha em `metadado.CampoPersonalizado` |

> **Por quê:** `[V]` este é o defeito mais caro do legado, e ele tem duas metades.
>
> **Metade um — a regra não tem condição.** `IV_AcaoAuto` tem 31 colunas, das quais 9 estão 100%
> nulas, e a tabela que guardaria as condições (`IV_AcaoAutoCtrl`) está **VAZIA**, com
> `UsaObjDyn = 0` em 100% das linhas. Sem linguagem de condição, o único discriminante é o par
> (Resultado, NroEmpresa).
>
> **Metade dois — sem condição, a única saída é copiar.** Medido: **5.958 regras para apenas
> 1.262 pares (Resultado, Ação) distintos**, sendo 1.429 globais e cerca de 4.500 replicadas
> entre as **18 empresas** (239 a 354 regras por empresa). As 18 são todas filiais da Tracbel
> Agro, 17 delas em SP — **não há razão de negócio para a duplicação**. `IV_ResMsgPapel` repete o
> padrão com mais 1.338 regras, 100% no canal e-mail. **Mudar um passo de processo exige 18
> edições** (relatório de extração, achado 6).
>
> Duas colunas resolvem os dois problemas — `Condicao` e `ExpressaoDestinatario` — e é por isso
> que elas existem em `Regra`.
>
> **Como se verifica:** `MotorWorkflowTestes` (10 testes em
> `tests/Tracbel.Crm.Dominio.Testes/Workflow/`), com o caso mais importante sendo
> `Registra_no_log_quando_a_condicao_e_falsa`. Não há teste de arquitetura que prove "fechado
> para modificação" — isso é revisão de PR: *seu PR alterou um arquivo existente para
> acrescentar um comportamento? Então o ponto de extensão está errado.*

### 4.2 O aberto/fechado só vale se o fechamento for observável

Um motor extensível que engole falha em silêncio é pior do que um `switch`. Por isso o motor
grava **toda** avaliação — disparou, não disparou, e o motivo em português:

```csharp
await GravarAsync(regra, contexto, correlacaoId, ResultadoRegra.CondicaoFalsa,
    avaliacao.Explicacao, inicio, ct);
```

> **Por quê:** `[V]` a taxa de geração da ação 900 caiu de **68% (março) para 3% (agosto/2026)**
> ao longo de cinco meses e ninguém percebeu, porque regra que não dispara não deixa rastro. O
> passivo correlato: **35.662 tarefas pendentes, 44% delas em conta de usuário desativado**
> (pesquisa 16, §4.5).

---

## 5. Substituição de Liskov

> Quem recebe um tipo pode receber qualquer subtipo dele **sem verificar qual é** e sem que o
> comportamento observável quebre. Na prática, no nosso código, isso se manifesta em três
> lugares: os tipos de valor, os eventos de domínio e a única hierarquia real que temos.

### 5.1 Onde há herança, e por que ela respeita o contrato

O domínio tem exatamente **uma** hierarquia de classes: `EntidadeBase` e as 16 entidades que a
herdam (`Cliente`, `Processo`, `Tarefa`, `Lead`, `Equipamento`, `Carteira`, `Meta`, `Contato`,
`Endereco`, `Alerta`, `Documento`, `Relatorio`, `Usuario`, `Equipe`, `CompartilhamentoDeRegistro`,
`ItemDeProposta`). **Todas as folhas são `sealed`**, e `EntidadeBase` não tem nenhum método
virtual. Não há como uma subclasse enfraquecer um contrato que não existe para ser sobrescrito —
é Liskov obtido por construção, não por disciplina.

A única abstração polimórfica prevista é `Atividade.Resumir()` (documento 03, seção 3.3), que
tem contrato trivial: devolve uma string não nula. Nenhuma implementação pode enfraquecê-lo.

> **Como se verifica:** revisão de PR. Não há teste, e não deveria haver um genérico: "o subtipo
> respeita o contrato" não é decidível por reflexão. O que é verificável são os dois sintomas
> abaixo.

### 5.2 O sintoma verificável: `NotImplementedException`

É a violação de Liskov mais comum e a mais barata de detectar. O tipo declara que cumpre o
contrato, o compilador acredita, e quem chama descobre em produção que não cumpre.

**A regra:** implementação que ainda não sabe fazer algo devolve `Resultado<T>.Falha` ou
`ResultadoEfeito.NaoAplicado` com o motivo em português — que é informação registrada, não
surpresa:

```csharp
/// O efeito não se aplicou — e isso é INFORMAÇÃO A REGISTRAR, não erro a engolir.
public static ResultadoEfeito NaoAplicado(string motivo) => new() { Aplicou = false, Motivo = motivo };
```

> **Por quê:** `[V]` `IVS_Carteira.SeqUrSuperv` é uma das quatro rotas paralelas pelas quais o
> Vórtice resolve visibilidade — e é **NULL em 655 de 655 carteiras**. O código a consulta, o
> modelo a documenta, e ela nunca respondeu nada. Uma rota inteira que o sistema declara ter e
> não tem. Pior: as quatro rotas só valem para o mobile; no desktop não há equivalente
> configurável.
>
> **Como se verifica:** `SolidTestes.Nenhuma_implementacao_lanca_NotImplementedException`.
> Hoje: zero ocorrências em toda a `src/`.

### 5.3 Os tipos de valor: substituíveis porque são valores

`Comum/` tem nove tipos de valor, todos `readonly record struct`: `Cep`, `Chassi`, `Coordenada`,
`CpfCnpj`, `DataHoraUtc`, `Dinheiro`, `Email`, `Telefone`, `TextoNormalizado`.

Cada um segue o mesmo desenho, e é a repetição desse desenho que faz a substituição funcionar:

```csharp
public readonly record struct Telefone
{
    public string Numero { get; }                 // sem setter
    private Telefone(string numero) => Numero = numero;   // construtor privado

    public static Telefone Criar(string entrada);         // lança: entrada de usuário
    public static bool TentarCriar(string? entrada, out Telefone telefone);  // não lança: lote
    public string Formatado();
}
```

Três propriedades que juntas dão Liskov:

1. **Imutável.** `readonly struct` — nada muda depois de criado, então nenhum caminho de código
   observa um valor diferente do que recebeu.
2. **Igualdade por valor.** `record struct` gera `Equals`/`op_Equality` estruturais: dois
   telefones com o mesmo número **são** o mesmo telefone, em qualquer contexto.
3. **Não existe instância inválida.** Construtor privado + fábrica: quem tem um `Telefone` em
   mãos tem um telefone válido, sempre. Não há "telefone quase válido" que quebre o terceiro
   chamador da cadeia.

O par `Criar`/`TentarCriar` é deliberado, e a razão está no comentário de `Telefone`:

> `[V]` a versão que não lança existe para a importação em massa, onde a linha ruim deve ser
> registrada e **o lote deve continuar** — nunca abortado inteiro. O sincronismo do Vórtice
> Mobile aborta a carga inteira numa única linha órfã, e o saldo medido são **5.302 vínculos
> quebrados** (documento 01, achado 9.11).

> **Por quê (o resto do número):** `[V]` `GE_Pessoa.FoneNro1` é `decimal(12)` — telefone com DDI
> ou zero à esquerda estoura o tipo e a API responde `{"Message":"An error has occurred."}`.
> E `CpfCnpj` existe porque foram medidos **116 CPFs repetidos entre clientes distintos**, com
> uma fila de deduplicação de **124 mil pares abandonados desde 2018**: o documento nunca foi um
> tipo, sempre foi texto que cada tela comparava do seu jeito.
>
> **Como se verifica:** `SolidTestes.Tipos_de_valor_do_dominio_sao_imutaveis_e_comparados_por_valor`
> (os três requisitos, para todo tipo de valor de `Comum/`) e os 138 testes de
> `tests/Tracbel.Crm.Dominio.Testes/`.

### 5.4 Os eventos de domínio: fatos, não objetos

Evento de domínio é um `record` com propriedades `init`-only. Um fato que já aconteceu não se
altera — e por isso qualquer manipulador pode receber qualquer evento sem defender-se de mutação
por outro manipulador que rodou antes.

> **Como se verifica:** `ArquiteturaTestes.Eventos_de_dominio_sao_imutaveis`.

---

## 6. Segregação de interface

> Ninguém deve ser obrigado a implementar método que não usa. A pergunta prática: *existe alguma
> porta gorda no domínio?*

### 6.1 A resposta medida: não

As dez portas do domínio e o tamanho de cada uma:

| Porta | Onde | Membros |
|---|---|---:|
| `IRepositorioRegras` | `Portas/Portas.cs` | 1 |
| `IAvaliadorCondicao` | `Portas/Portas.cs` | 1 |
| `IRegistroExecucaoRegra` | `Portas/Portas.cs` | 1 |
| `IResolvedorDestinatario` | `Portas/Portas.cs` | 1 |
| `IProvedorContextoAcesso` | `Portas/IProvedorContextoAcesso.cs` | 1 |
| `IEventoDominio` | `Comum/Fundamentos.cs` (marcador) | 1 |
| `IRelogio` | `Comum/Fundamentos.cs` | 1 |
| `IEstrategiaDestinatario` | `Portas/Portas.cs` | 2 |
| `IHierarquiaVendas` | `Portas/Portas.cs` | 2 |
| `ICalendarioUtil` | `Portas/Portas.cs` | 2 |
| `IEfeitoRegra` | `Workflow/MotorWorkflow.cs` | 2 |

**Máximo: 2.** Nenhuma porta obriga ninguém a escrever método que não usa. A prova disso está
nos dublês de `tests/Tracbel.Crm.Dominio.Testes/Workflow/MotorWorkflowTestes.cs`: cada falso tem
duas ou três linhas, porque não há o que preencher à toa.

O caso mais instrutivo é `IEstrategiaDestinatario`, que **poderia** ter sido um método a mais em
`IResolvedorDestinatario` e não foi:

```csharp
public interface IResolvedorDestinatario           // o motor usa este
{
    Task<Destinatario?> ResolverAsync(string expressao, ContextoRegra contexto, CancellationToken ct);
}

public interface IEstrategiaDestinatario           // cada forma de dizer "para quem" usa este
{
    bool Reconhece(string expressao);
    Task<Destinatario?> ResolverAsync(string expressao, ContextoRegra contexto, CancellationToken ct);
}
```

O motor não sabe que estratégias existem — ele só resolve. Quem escreve uma estratégia nova não
precisa saber que existe um resolvedor. São dois interlocutores, duas interfaces.

### 6.2 A regra escrita

**Regra 6.2 — [Testado]** Nenhuma porta do domínio passa de **quatro membros**, ou registra
exceção com justificativa.

> **Por quê:** quatro é o dobro da maior porta que temos hoje — margem para crescer sem esconder
> o dia em que uma porta virou um serviço. `[V]` o contra-exemplo é `IV_AcaoAuto`: **31 colunas,
> 9 delas 100% nulas**. Toda regra do sistema paga o preço das colunas que não usa. E
> `IV_Resultado` leva o padrão ao extremo com **35 colunas de flag `CTRL*`** em 4.209 linhas.
>
> **Como se verifica:** `SolidTestes.Nenhuma_porta_do_dominio_passa_de_quatro_membros`. O
> dicionário de exceções do teste está **vazio hoje**, de propósito: uma exceção exige linha
> escrita, e linha escrita exige justificativa lida em PR.

---

## 7. Inversão de dependência

> O domínio declara **o que precisa** (a porta). A infraestrutura fornece **como** (o adaptador).
> A injeção de dependência liga os dois no boot. É o `OrderRepository` do artigo, no nosso
> vocabulário — com uma diferença de ênfase: aqui a porta é declarada **pelo consumidor**, dentro
> do domínio, não pelo repositório.

### 7.1 O exemplo, ponta a ponta

```csharp
// 1. O DOMÍNIO declara o que exige do mundo. Nenhum using de EF Core, HTTP ou SQL.
//    src/Tracbel.Crm.Dominio/Portas/Portas.cs
public interface IRepositorioRegras
{
    Task<IReadOnlyList<Regra>> ObterPorGatilhoAsync(
        string evento, int? resultadoId, int? tipoProcessoId, CancellationToken ct);
}

// 2. O DOMÍNIO consome a interface, nunca a implementação.
//    src/Tracbel.Crm.Dominio/Workflow/MotorWorkflow.cs
public sealed class MotorWorkflow(IRepositorioRegras regras, ...)

// 3. A INFRAESTRUTURA implementa. É a única que conhece EF Core.
//    src/Tracbel.Crm.Infraestrutura/Persistencia/RepositorioRegras.cs   (fase 1)

// 4. A INJEÇÃO liga. É a única linha do sistema que conhece os dois lados.
//    src/Tracbel.Crm.Api/Program.cs
builder.Services.AddScoped<IProvedorContextoAcesso, ProvedorContextoAcessoDeSistema>();
```

O ganho não é filosófico: é que os 138 testes de `Tracbel.Crm.Dominio.Testes` rodam em **menos de meio segundo**,
sem banco, sem HTTP e sem framework de mock — os dublês são classes de três linhas. É essa
velocidade que torna a exigência de 90% de cobertura do domínio (documento 03, seção 8)
exequível, e não uma boa intenção.

> **Por quê:** `[V]` no legado nada disso existe. A dependência entre p-code, os 196 SQLs em
> coluna e as 85 procedures é invisível para qualquer ferramenta, porque **o nome da tabela é
> resolvido em tempo de execução**. É por isso que ninguém consegue estimar o efeito de mudar
> nada — e por isso a estimativa vira "não mexe".

### 7.2 A regra estrutural: as setas apontam para dentro

**Regra 7.2 — [Testado]** O grafo de referências entre projetos é fechado e exatamente este:

| Projeto | Referencia |
|---|---|
| `Tracbel.Crm.Dominio` | *nada* |
| `Tracbel.Crm.Aplicacao` | `Dominio` |
| `Tracbel.Crm.Integracao` | `Dominio` |
| `Tracbel.Crm.Infraestrutura` | `Dominio`, `Aplicacao` |
| `Tracbel.Crm.Api` | `Aplicacao`, `Infraestrutura` |

> **Por quê:** é a inversão de dependência na sua forma mais barata e mais dura. Uma referência
> de projeto errada quebra o build de todo mundo antes de qualquer código ser escrito. Se você
> precisou de uma seta nova, quase sempre a peça está na camada errada.
>
> **Como se verifica:** `SolidTestes.As_referencias_de_projeto_apontam_sempre_para_dentro`, que
> lê os `.csproj`. Complementar ao
> `ArquiteturaTestes.Dominio_nao_depende_de_infraestrutura_nem_de_banco`, que pega o vazamento
> por pacote NuGet.

### 7.3 A regra de tipo: caso de uso só conhece porta

**Regra 7.3 — [Testado, hoje sem alcance]** Nenhum tipo de `Tracbel.Crm.Aplicacao` depende de
tipo concreto de infraestrutura.

> **Honestidade sobre o alcance:** `Tracbel.Crm.Aplicacao` ainda não tem nenhum tipo — a fase 1
> do documento 06 é que os cria. O teste hoje não reprova nada. Ele está armado desde já porque
> o primeiro caso de uso é exatamente o momento em que a tentação de injetar o `CrmDbContext`
> aparece, e é mais fácil o teste já existir do que lembrar de escrevê-lo naquele PR. A garantia
> que vale **hoje** é a da regra 7.2, sobre o `.csproj`.
>
> **Como se verifica:** `SolidTestes.Nenhum_tipo_da_aplicacao_depende_de_tipo_concreto_de_infraestrutura`.

### 7.4 A regra de organização: porta mora em `Portas/`

**Regra 7.4 — [Testado]** Toda interface pública do domínio mora em
`Tracbel.Crm.Dominio.Portas`, ou está na lista de exceções do teste **com justificativa
escrita**.

> **Por quê:** não é arrumação. Uma porta é o que o domínio exige do mundo, e a lista dessas
> exigências é o que um dev novo precisa conseguir ler num lugar só para saber o que a
> infraestrutura tem de entregar. Hoje há três exceções registradas — `IEventoDominio`,
> `IRelogio` e `IEfeitoRegra` — e a justificativa de cada uma está na seção 11.
>
> **Como se verifica:** `SolidTestes.Toda_porta_do_dominio_mora_no_namespace_Portas_ou_tem_justificativa_registrada`.

### 7.5 A regra de contrato: `async` sempre com `CancellationToken`

**Regra 7.5 — [Testado]** Todo método assíncrono público de `Dominio` e `Integracao` recebe
`CancellationToken`, e o propaga até o fim.

> **Por quê:** é a convenção 6 do documento 03, seção 9, e ela é de inversão de dependência —
> quem inicia a operação é quem decide desistir dela. Sem o token, o cancelamento vira
> responsabilidade da implementação, e cada implementação decide diferente. `[V]` o sincronismo
> do Vórtice Mobile não tem como ser interrompido: uma linha ruim aborta a carga inteira e não há
> caminho para parar antes.
>
> **Como se verifica:** `SolidTestes.Todo_metodo_assincrono_publico_recebe_CancellationToken`.

---

## 8. Os padrões de projeto que já usamos

Nomeados, com onde estão e por que foram escolhidos. A lista é curta de propósito — ver seção 10.

### 8.1 Repositório (*Repository*)

**Onde:** `IRepositorioRegras` em `Portas/Portas.cs`; a implementação chega na fase 1, em
`Infraestrutura/Persistencia/`.
**Por quê:** é a porta que permite testar o motor de workflow sem banco. Note a assinatura:

```csharp
/// Devolve as regras candidatas a um gatilho, já filtradas pelo que é indexável.
/// A condição fina é avaliada pelo motor, não aqui.
Task<IReadOnlyList<Regra>> ObterPorGatilhoAsync(string evento, int? resultadoId, int? tipoProcessoId, CancellationToken ct);
```

A divisão de trabalho está escrita no comentário: **o repositório filtra o que o índice resolve;
o motor avalia o que é regra.** É o que impede o repositório de virar o lugar onde a lógica se
esconde — o defeito exato das 70 procedures do Vórtice que escrevem em tabela.

### 8.2 Unidade de trabalho (*Unit of Work*)

**Onde:** `CrmDbContext` (`Infraestrutura/Persistencia/CrmDbContext.cs`), com o `SalvarComEventosAsync`
descrito no documento 03, seção 5.3.
**Por quê:** o dado e o evento gravam na **mesma transação**; o evento só é *publicado* depois do
commit.

> `[V]` A integração do Vórtice não tem transação — o `BEGIN TRANSACTION` está literalmente
> comentado nas procedures. O saldo medido: **783.242 títulos (R$ 5,18 bilhões) presos em
> staging desde maio de 2025** e **280.214 ordens de serviço nunca promovidas**, com o
> faturamento parado há **17 meses** sem ninguém notar. Publicar antes do commit é o erro
> simétrico: notificar sobre algo que a transação desfez.

### 8.3 Resultado em vez de exceção para erro de negócio

**Onde:** `Resultado<T>` e `RegraDeNegocioViolada` em `Comum/Fundamentos.cs`; `ResultadoEfeito` e
`AvaliacaoCondicao` em `Workflow/ContextoRegra.cs`; `ResultadoSaneamento<T>` em
`Integracao/Saneamento/Saneamento.cs`.
**Por quê:** a distinção é a convenção 7 do documento 03, e está escrita no próprio tipo:

> Erro de **negócio** ("lead já qualificado") devolve `Resultado<T>` ou lança
> `RegraDeNegocioViolada` dentro da entidade; vira 409/422. Erro de **programação** (nulo
> inesperado) lança as exceções normais do .NET; vira 500. **Nunca o inverso.**

O caso do motor de workflow é o mais importante: `ResultadoEfeito.NaoAplicado(motivo)` existe
porque *"não disparou"* é informação a registrar, não erro a engolir — e é essa decisão de
desenho que torna o defeito 899/900 estruturalmente impossível.

### 8.4 Evento de domínio (*Domain Event*)

**Onde:** `IEventoDominio` e a lista privada em `Comum/EntidadeBase.cs`; os eventos concretos em
`Crm/EventosLead.cs`.
**Por quê:** é o ponto de extensão que permite acrescentar comportamento sem tocar em quem
originou o fato — o `VincularContaACarteiraAoQualificar` do documento 03, seção 5.2. Os eventos
são acumulados na entidade e publicados pela unidade de trabalho, nunca pela entidade.

### 8.5 Camada anticorrupção (*Anti-Corruption Layer*)

**Onde:** `Tracbel.Crm.Integracao` inteira, hoje `Saneamento/`.
**Por quê:** o vocabulário do legado é uma armadilha (`CodProcesso` não é o código do processo;
`IV_Agenda.Vendedor` guarda um login, não um ID). O contrato genérico é minúsculo:

```csharp
public interface ISanitizador<in TBruto, TLimpo>
{
    ResultadoSaneamento<TLimpo> Sanear(TBruto bruto);
}
```

E `SanitizadorLeadExterno` mostra a fronteira funcionando: entra `LeadExternoBruto` (tudo `string?`,
porque é assim que a origem entrega), sai `LeadExternoSaneado` (só tipos de valor do domínio).
Cada correção vira um `CorrecaoAplicada`; cada campo ruim, um `CampoRejeitado`; o lote nunca
aborta.

O comentário do próprio arquivo registra a divisão de responsabilidade, e ela é exemplar:

> A regra de negócio "precisa de e-mail OU telefone" pertence à entidade `Lead`, não a este
> pipeline — aqui cada campo é saneado por si; a composição entre campos é decisão de domínio.

**Quando o Vórtice morrer, apaga-se uma pasta.** É esse o teste real da camada anticorrupção.

> **Como se verifica:** `ArquiteturaTestes.Vocabulario_do_Vortice_fica_confinado_na_camada_de_integracao`.

### 8.6 Estratégia (*Strategy*)

**Onde:** `IEstrategiaDestinatario` em `Portas/Portas.cs`.
**Por quê:** cada forma de dizer "para quem vai a tarefa" é uma classe. O par `Reconhece` +
`ResolverAsync` é o que permite acrescentar uma forma nova sem tocar no resolvedor.

> `[V]` `IV_AcaoAuto.SeqUsuario` guarda um **ID fixo** na linha da regra: a atribuição é decidida
> quando a regra é escrita, não quando ela roda. É por isso que trocar o líder de um CEN não
> corrige as aprovações já geradas — o sistema não reatribui.

### 8.7 Fábrica (*Factory*) — como método estático

**Onde:** em toda parte, e sempre na mesma forma. `Lead.Criar(...)`, `Tarefa.CriarPorRegra(...)`,
`CampoPersonalizado.Criar(...)`, `TratadorDeEvento.Criar(...)`, `Telefone.Criar`/`TentarCriar` e
os oito outros tipos de valor, `Resultado<T>.Ok`/`Falha`, `ResultadoEfeito.Aplicado`/`NaoAplicado`,
`DecisaoAcesso.Permite`/`Nega`, `ResultadoSaneamento<T>.Aceitar`/`Rejeitar`.
**Por quê:** construtor público não consegue recusar. Fábrica consegue, e o nome dela documenta a
intenção. **Não há classe-fábrica separada em lugar nenhum**, e não deve haver: seria a divisão
excessiva de que trata a seção 10.

### 8.8 Especificação de acesso, com explicação

**Onde:** `Autorizador` e `DecisaoAcesso` em `Seguranca/Autorizador.cs`.
**Por quê:** a decisão **e o motivo** saem juntos, desde a primeira linha de código:

```csharp
public sealed record DecisaoAcesso(bool Permitido, string Motivo, int Camada);
```

> `[DYN]` A Microsoft precisou construir uma API inteira (`RetrieveAccessOrigin`) só para
> responder "por que este usuário vê isto". `[V]` No Vórtice não há permissão por registro,
> nenhum RLS no SQL Server, um único trigger no banco inteiro, e toda a segurança feita no
> cliente Gupta — acesso direto ao banco ignora tudo e não deixa rastro.

### 8.9 Marcador de fase (*Outbox*) e injeção de relógio

`MensagemSaida`/outbox (documento 03, seção 5.3) e `IRelogio`/`RelogioDoSistema`. O segundo é
pequeno e decisivo: chamar `DateTime.UtcNow` dentro da regra torna o comportamento impossível de
testar de forma determinística.

---

## 9. SOLID no frontend — React e TypeScript

O frontend é React 18 + TypeScript + Vite (`src/Tracbel.Crm.Web/`). Os princípios são os mesmos;
a mecânica muda: onde no C# há interface e injeção, no TypeScript há **módulo e tipo**.

### 9.1 Inversão de dependência: a camada `src/dados/`

É o exemplo mais importante do frontend, e ele já existe. O comentário do próprio arquivo declara
o contrato:

```ts
/**
 * Carregador dos dados do protótipo.
 *
 * Hoje lê os JSONs extraídos do protótipo original (`public/dados/*.json`).
 * Quando a API existir, só esta camada muda: as telas continuam consumindo as mesmas funções.
 */
export async function carregar<T>(nome: string): Promise<T> { ... }
```

Nenhuma tela chama `fetch`. Toda tela chama `useDados<T>(nome)`, que chama `carregar<T>`, que
hoje lê JSON e amanhã chamará `/api/v1/...`. **A troca de JSON por API não toca em nenhum
componente** — é exatamente a inversão de dependência do backend, com módulo no lugar de
interface.

O tipo é o contrato. `src/tipos/` (12 arquivos) declara a forma do dado; a tela depende do tipo,
não da origem.

> **Como se verifica:** revisão de PR — *seu componente chamou `fetch` direto?* Não há teste
> automatizado hoje; o portão de 70% de cobertura com Vitest (documento 03, seção 8) é da fase 1.

### 9.2 Responsabilidade única: o hook separa buscar de mostrar

`useDados` (`src/dados/useDados.ts`, 22 linhas) faz **uma** coisa: transforma "nome de um recurso"
em `{ dados, erro, carregando }`. Ele não formata, não decide layout, não conhece nenhuma tela.

E a regra pura mora fora do React. `src/dados/painel360.ts` é o melhor exemplo: prazo de tarefa,
situação do dado vindo do Protheus e classificação de cobertura, todos **puros — sem React, sem
DOM**, como o cabeçalho do arquivo declara. É a mesma separação que no backend põe a regra na
entidade e a orquestração no caso de uso: aqui, a regra no módulo puro e a apresentação no
componente.

**Onde isso está violado hoje:** seção 11.1 e 11.2.

### 9.3 Aberto/fechado: composição, não `if` de variante

`AbasFicha` recebe uma lista de abas; `BlocoFicha` recebe conteúdo; `Layout` recebe a página.
Acrescentar uma aba é acrescentar um item ao array, não um `else if` dentro do componente. É o
mesmo desenho do `IEfeitoRegra`: o consumidor não conhece as variantes.

O sintoma da violação é a **propriedade que só serve para um caso** — `<Grafico eDaTelaDeFunil />`.
Quando ela aparece, o componente não é mais um; são dois disfarçados de um. A saída certa é
extrair o pedaço variável para `children` ou para uma prop de renderização.

### 9.4 Substituição de Liskov: união discriminada em vez de campo opcional

O equivalente frontend do tipo de valor é a **união discriminada**. Um estado que pode ser três
coisas é três formas, não uma forma com campos opcionais:

```ts
// Ruim: obriga todo consumidor a verificar qual combinação é válida.
type Estado<T> = { dados: T | null; erro: Error | null; carregando: boolean };

// Bom: o compilador garante que só um caso vale por vez.
type Estado<T> =
  | { situacao: 'carregando' }
  | { situacao: 'erro'; erro: Error }
  | { situacao: 'pronto'; dados: T };
```

O `Estado<T>` de `useDados.ts` hoje é a **primeira** forma. Não é defeito grave — o hook tem 22
linhas e a combinação inválida nunca é produzida — mas é a migração recomendada quando a camada
de dados passar a falar com a API, porque aí os estados se multiplicam (revalidando, obsoleto,
parcial). Registrado na seção 11.6.

### 9.5 Segregação de interface: a prop que ninguém usa

A tradução direta: um componente não deve exigir prop que aquele uso não precisa. O sintoma é
`<TabelaClientes clientes={...} filtro={undefined} aoOrdenar={() => {}} />` — três props passadas
só para calar o TypeScript. Quando isso aparece, a tabela é duas tabelas.

O padrão certo no nosso código é `TelaEmConstrucao`, que recebe o mínimo e não finge ser genérico.

---

## 10. Quando **não** aplicar

O artigo que originou este pedido é honesto nisso, e este documento precisa ser também.
**Dividir demais é um defeito, não uma virtude** — e num time de três pessoas o custo da divisão
excessiva é maior do que o da duplicação ocasional.

### 10.1 Não crie interface para o que tem uma implementação e sempre terá

`RelogioDoSistema` implementa `IRelogio`, e a interface se paga: o teste precisa fixar "agora".
Mas `Autorizador` **não** tem `IAutorizador`, e está certo assim — ele é lógica pura sobre
`ContextoAcesso`, o teste o instancia direto (`AutorizadorTestes`), e uma interface ali só
acrescentaria um arquivo e um salto de indireção.

**A pergunta que decide:** *existe um segundo implementador, real ou de teste, que eu preciso
substituir?* Se a resposta é não, não há interface.

### 10.2 Não divida a entidade que o negócio pensa junto

`Cliente` tem muitos campos. A tentação SRP é dividir em `DadosCadastraisDoCliente`,
`ClassificacaoDoCliente`, `ConsentimentosDoCliente`. **Não faça.** O comercial pensa "cliente"
como uma coisa só; três agregados exigiriam três transações, três consultas e três lugares para
esquecer de atualizar.

`[V]` O legado tem o defeito **na direção oposta e na mesma origem**: **90 tabelas em 34 grupos**
com conjunto de colunas idêntico, quase todas cópias manuais (`GE_Pessoa`, `GE_Pessoa_BKPJUN`,
`GE_Pessoa_BKP28052025`; `EXT_Titulo` com quatro variantes). Fragmentar sem critério e copiar sem
critério produzem o mesmo resultado: ninguém sabe onde está a verdade. O
[17-MODELO-UNIFICADO](17-MODELO-UNIFICADO.md) reduz as 767 tabelas a 63 exatamente por isso.

### 10.3 Não abstraia o ponto de extensão que ainda não tem segundo caso

Temos um único sanitizador (`SanitizadorLeadExterno`) e já existe `ISanitizador<TBruto, TLimpo>`.
Isso está no limite, e se justifica por duas razões concretas: o documento 16 já enumera os
próximos (Protheus, Vórtice, planilha, RD Station), e o contrato tem **um método**. Se tivesse
quatro, a decisão certa seria esperar o segundo caso.

**A regra do time:** a segunda ocorrência é que autoriza a abstração. A primeira é código; a
terceira é padrão. Extrair na primeira é adivinhação.

### 10.4 Não faça o motor de workflow resolver o que a linguagem resolve

O motor é declarativo porque há **5.958 regras** de evidência de que precisa ser. Uma regra que
existe uma vez e nunca vai variar por filial não precisa virar linha em `processo.Regra` — pode
ser um `if` dentro do método da entidade, onde o teste a alcança direto. Configurabilidade tem
custo: cada regra em tabela é uma regra que não está no compilador.

### 10.5 Não divida arquivo por contagem de tipos

A convenção 8 do documento 03 diz "um arquivo, um tipo". Na prática o código agrupa por conceito:
`Portas/Portas.cs` tem sete interfaces relacionadas; `Comum/Fundamentos.cs` tem
`IEventoDominio`, `RegraDeNegocioViolada`, `Resultado<T>`, `IRelogio` e `RelogioDoSistema`.
**Isso é melhor do que sete e cinco arquivos**, e é por isso que não há teste de "um arquivo, um
tipo". Ver seção 11.7 — a convenção é que precisa ser corrigida, não o código.

---

## 11. As violações reais, hoje

Levantamento de 04/09/2026 sobre o código como está. **Nada aqui foi consertado em silêncio.**
O que foi consertado está marcado; o resto é lista para decidir.

### 11.1 [Média] Formatadores duplicados em 10 arquivos do frontend

`src/Tracbel.Crm.Web/src/dados/formatadores.ts` exporta `formatarBRL`, `formatarBRLCompleto`,
`formatarNumero` e `formatarBRLCompacto`. Eles são **reimplementados localmente**, com corpo
idêntico ou quase, em:

| Arquivo | Linhas |
|---|---|
| `componentes/AbaPosVendas.tsx` | 26, 30, 36 |
| `componentes/painel360/PainelExecutivo.tsx` | 49 |
| `componentes/TabelaClientes.tsx` | 27 |
| `dados/performanceCen.ts` | 24 |
| `telas/Agenda.tsx` | 58, 63 |
| `telas/ClienteFicha.tsx` | 26, 29, 32 |
| `telas/Clientes.tsx` | 49 |
| `telas/CoberturaCarteira.tsx` | 28 |
| `telas/CoberturaRegional.tsx` | 20, 24 |
| `telas/EquipamentoFicha.tsx` | 22, 25, 28 |
| `telas/OportunidadeFicha.tsx` | 27, 30, 33 |

**Por que importa:** o dia em que a diretoria pedir "mostre centavos no funil", a mudança será em
onze lugares — e um deles vai passar despercebido, que é como o legado chegou a `FINALIZADO`
convivendo com `FINALIZADA`.

**Correção sugerida:** importar de `dados/formatadores.ts` e apagar as cópias; acrescentar
`formatarDataHora` e `formatarDataCurta` ao módulo, que hoje não existem lá e são duplicados
quatro vezes.

**Não corrigido aqui** porque toca onze arquivos de tela em porte pixel a pixel sob revisão de
outra frente (documento 08). É um PR próprio, pequeno e mecânico.

### 11.2 [Média] Componentes que fazem demais

O documento 03, seção 9, convenção 9, diz: *"arquivo passou de 400 linhas? é sinal de que faz
coisa demais."* Onze arquivos do frontend passam:

| Arquivo | Linhas |
|---|---:|
| `componentes/painel360/PainelExecutivo.tsx` | 816 |
| `componentes/painel360/Cliente360.tsx` | 786 |
| `telas/ClienteFicha.tsx` | 763 |
| `telas/OportunidadeFicha.tsx` | 736 |
| `telas/EquipamentoFicha.tsx` | 680 |
| `telas/CoberturaCarteira.tsx` | 669 |
| `componentes/painel360/PainelCen.tsx` | 567 |
| `telas/Agenda.tsx` | 559 |
| `telas/Pipeline.tsx` | 558 |
| `componentes/AbaPosVendas.tsx` | 534 |
| `telas/NovaOportunidade.tsx` | 504 |

Cada um mistura, no mesmo arquivo, formatação, agregação de dados e marcação. `PainelExecutivo.tsx`
declara dez funções locais antes do componente.

**Contexto que atenua:** são portes deliberados e literais do protótipo do gerente, com paridade
visual como requisito — os cabeçalhos citam a linha exata de `prototipo/referencia/assets/app.js`
que cada um reproduz. Dividir agora arriscaria a paridade que é o critério de aceite atual.

**Correção sugerida:** quando a tela passar a consumir a API (fase 1), extrair primeiro a
agregação para `src/dados/` — que é onde `painel360.ts` já mostra o padrão certo — e só depois
partir a marcação.

**Não corrigido aqui**, e a divisão não deve acontecer antes da troca de fonte de dados.

### 11.3 [Média] Nenhuma porta tem implementação de produção

Das dez portas do domínio, **apenas `IProvedorContextoAcesso` tem implementação fora de teste** —
e ela é um stub (ver 11.4). As outras nove existem só com dublê em
`tests/Tracbel.Crm.Dominio.Testes/Workflow/MotorWorkflowTestes.cs`.

**Por que importa:** a inversão de dependência ainda não foi exercitada de ponta a ponta. É
possível que uma assinatura de porta se revele impraticável quando o EF Core a implementar de
verdade — o risco clássico é `ObterPorGatilhoAsync` devolver regras demais.

**Não é defeito de desenho; é o estado da fase 0.** Registrado para que a fase 1 comece pelas
implementações e não por mais portas.

### 11.4 [Baixa] Adaptador de identidade mora na API, não na Infraestrutura

`ProvedorContextoAcessoDeSistema` está em `src/Tracbel.Crm.Api/Program.cs:66`. É um adaptador de
porta, e adaptador mora em `Infraestrutura/` (documento 03, seção 1) — a pasta `Identidade/`
existe no desenho justamente para ele.

**Contexto:** é stub declarado, temporário até o Entra ID entrar, e serve também ao design time
do `dotnet ef`.

**Correção sugerida:** quando a autenticação real entrar, o adaptador nasce em
`Infraestrutura/Identidade/`, e o `Program.cs` fica só com a linha de registro.

**Não corrigido aqui:** mover um stub que será substituído inteiro é churn sem ganho.

### 11.5 [Baixa] `Tracbel.Crm.Aplicacao` está vazio

Zero tipos. É o previsto para a fase 0 (documento 06), e é a razão de a regra 7.3 não ter alcance
hoje. Registrado para que a leitura de "191 testes passando" não seja confundida com "a camada de
aplicação está coberta" — ela não existe.

### 11.6 [Baixa] Três decisões de organização registradas como exceção

Nenhuma é defeito; todas são escolhas que o teste da regra 7.4 obriga a declarar por escrito:

| Tipo | Onde | Justificativa |
|---|---|---|
| `IEventoDominio` | `Comum/Fundamentos.cs:8` | marcador, não porta — não exige nada do mundo externo |
| `IRelogio` | `Comum/Fundamentos.cs:76` | é porta, mas primitiva usada por todo o domínio; mora junto de `RelogioDoSistema` |
| `IEfeitoRegra` | `Workflow/MotorWorkflow.cs:14` | é porta, mas é o ponto de extensão **do motor**; colocada junto de quem a consome |

O caso de `IRelogio` tem um detalhe: `RelogioDoSistema` (`Comum/Fundamentos.cs:83`) é um
**adaptador dentro do domínio**. Aceito, porque não depende de nada externo — mas é a única
exceção do tipo, e não deve haver uma segunda.

Somam-se a essas o `Estado<T>` de `useDados.ts` (seção 9.4), a migrar para união discriminada
quando a camada de dados falar com a API, e o endpoint `/saude/banco` de `Program.cs:47`, que
conversa direto com o `CrmDbContext` — aceito para verificação de vida, anotado para não virar
precedente.

### 11.7 [Baixa] A convenção "um arquivo, um tipo" está errada, não o código

O documento 03, seção 9, convenção 8, exige um tipo por arquivo. O código não cumpre, em
**33 dos 51 arquivos do domínio**, e **os maiores casos são bons agrupamentos**:

| Arquivo | Tipos públicos |
|---|---:|
| `Processo/Catalogos.cs` | 8 |
| `Integracao/Integracao.cs` | 8 |
| `Portas/Portas.cs` | 7 |
| `Workflow/ContextoRegra.cs` | 6 |
| `Metadado/Formulario.cs` | 6 |
| `Crm/EventosLead.cs` | 5 |
| `Comum/Fundamentos.cs` | 5 |

**Recomendação:** corrigir a convenção, não o código — trocar "um arquivo, um tipo" por
**"um arquivo, um conceito coeso; o nome do arquivo nomeia o conceito"**. Não há teste para essa
regra, e não deveria haver a que está escrita hoje: ela reprovaria 33 arquivos que ninguém quer
dividir.

**Decisão pendente**, e ela pertence ao documento 03.

### 11.8 O que foi consertado

Uma coisa só, e é de teste:

- `tests/Tracbel.Crm.Arquitetura.Testes/ArquiteturaTestes.cs:140` — `LocalizarRaizDoRepositorio()`
  passou de `private` para `internal static`, para ser reusada por `SolidTestes.cs` em vez de
  duplicada. Nenhuma mudança de comportamento.

**Nada em `src/` foi alterado.** As violações acima são todas de decisão, não de erro óbvio.

---

## 12. Os testes, e o que cada um impede

Índice reverso: dado um teste que falhou, qual regra e qual seção ele protege.

| Teste | Princípio | Seção | O que ele impede |
|---|---|---|---|
| `Dominio_nao_depende_de_infraestrutura_nem_de_banco` * | DIP | 3.1, 7.2 | o domínio importar EF Core, ASP.NET ou SqlClient |
| `Vocabulario_do_Vortice_fica_confinado_na_camada_de_integracao` * | SRP | 8.5 | `SeqPessoa`, `CodProcesso` e afins vazarem da quarentena |
| `Entidades_nao_expoem_setter_publico` * | SRP | 3.2 | estado de entidade mudar por atribuição em vez de método com regra |
| `Eventos_de_dominio_sao_imutaveis` * | LSP | 5.4 | alterar um fato que já aconteceu |
| `As_referencias_de_projeto_apontam_sempre_para_dentro` | DIP | 7.2 | uma seta de referência nova entre camadas, sem decisão |
| `Nenhum_tipo_da_aplicacao_depende_de_tipo_concreto_de_infraestrutura` | DIP | 7.3 | o caso de uso injetar `CrmDbContext` em vez da porta |
| `Toda_porta_do_dominio_mora_no_namespace_Portas_ou_tem_justificativa_registrada` | DIP | 7.4 | porta nova espalhada, sem justificativa escrita |
| `Todo_metodo_assincrono_publico_recebe_CancellationToken` | DIP | 7.5 | operação assíncrona que não pode ser interrompida |
| `Nenhuma_porta_do_dominio_passa_de_quatro_membros` | ISP | 6.2 | porta gorda que obriga a implementar o que não se usa |
| `Nenhuma_implementacao_lanca_NotImplementedException` | LSP | 5.2 | contrato declarado e não cumprido, descoberto em produção |
| `Tipos_de_valor_do_dominio_sao_imutaveis_e_comparados_por_valor` | LSP | 5.3 | tipo de valor mutável, com setter, ou comparado por referência |
| `Entidades_nao_expoem_colecao_mutavel` | SRP | 3.3 | `public List<T>` numa entidade, que qualquer um pode limpar |
| `A_api_nao_decide_regra_de_negocio` | SRP | 3.4 | regra de negócio migrar para o endpoint |

\* já existiam antes deste documento, em `ArquiteturaTestes.cs`.

Cada um dos nove novos foi verificado das duas maneiras: **passa** no código como está, e
**falha** quando a violação correspondente é introduzida de propósito.

### 12.1 Os testes que foram descartados, e por quê

O pedido original listava candidatos. Estes ficaram de fora, com o motivo:

**"Nenhum caso de uso tem mais de uma razão para mudar."** Descartado por **não haver heurística
honesta**. As candidatas usuais — contar dependências injetadas, contar métodos públicos, contar
linhas — medem **acoplamento** ou **tamanho**, não "razões para mudar". Um caso de uso com seis
dependências e uma responsabilidade passaria a reprovar; um com duas dependências e três
responsabilidades passaria. Um teste que reprova o certo e aprova o errado é pior do que teste
nenhum, porque ensina o time a contornar a métrica. **Fica como item de revisão de PR**, com a
pergunta escrita: *quem pediria esta mudança? se as respostas forem duas pessoas diferentes, são
dois casos de uso.* (E, hoje, o teste seria vazio de qualquer forma — ver 11.5.)

**"Cada `IEfeitoRegra` tem `Nome` único."** Descartado por ser **vazio hoje**: não há nenhuma
implementação de produção (11.3). Deve entrar junto com o primeiro efeito real, e é um teste de
três linhas.

**"Controlador não contém regra de negócio", na forma plena.** Substituído pelo proxy estrutural
de 3.4, que barra os três sintomas inequívocos. Medir "quanta regra existe num endpoint" cai no
mesmo problema do primeiro item.

**"Arquivo com no máximo 400 linhas."** Descartado porque reprovaria onze arquivos do frontend
que **não devem ser divididos agora** (11.2) e porque a convenção correlata do documento 03 já
precisa de revisão (11.7). Vira critério de revisão de PR para arquivo **novo**.

---

## 13. Verificação

```
dotnet build   → sem erro e sem aviso
dotnet test    → 200 testes, 0 falhas
                 Tracbel.Crm.Dominio.Testes       138
                 Tracbel.Crm.Aplicacao.Testes      12
                 Tracbel.Crm.Arquitetura.Testes    50   (41 anteriores + 9 deste documento)
```

O portão de qualidade do documento 03, seção 8, continua valendo: **arquitetura, zero violação**.
Falha nos testes desta pasta bloqueia o merge.
