# Arquitetura de software — CRM Tracbel

> **Documento 03 de 07** · Versão 1.1 · 19/09/2026 — a pilha medida substitui a planejada (issue #84).
> Versão 1.0 · 30/08/2026.
> Stack **hoje, no código**: **.NET 10 (LTS) · C# 14 (`LangVersion latest`) · EF Core 10 · SQL Server ·
> React 19 + TypeScript 6 + Vite 8**. O texto abaixo foi escrito quando o alvo era .NET 8 e EF Core 8;
> onde ele cita a versão, vale esta linha.
> Este documento é o material de referência do time. Se um dev novo ler só um arquivo do projeto,
> que seja este.

---

## 1. As cinco camadas

```
Tracbel.Crm.sln
│
├── src/Tracbel.Crm.Dominio/          ← O CORAÇÃO. Zero dependência externa.
│   ├── Comum/                          entidades-base, tipos de valor, erros
│   ├── Crm/                            Conta, Contato, Lead
│   ├── Workflow/                       Processo, Tarefa, Atividade, Regra
│   ├── Seguranca/                      Usuario, Permissao, ContextoAcesso
│   └── Portas/                         INTERFACES que o domínio exige do mundo
│
├── src/Tracbel.Crm.Aplicacao/        ← Casos de uso. Orquestra o domínio.
│   ├── Leads/                          QualificarLead, ReceberLeadExterno
│   ├── Processos/                      DarAndamento, MoverEstagio
│   └── Comum/                          pipeline de comandos, validação, resultado
│
├── src/Tracbel.Crm.Infraestrutura/   ← O mundo real. Implementa as Portas.
│   ├── Persistencia/                   DbContext, configurações EF, repositórios
│   ├── Identidade/                     Entra ID, resolução do usuário
│   ├── Mensageria/                     outbox, jobs, e-mail
│   └── Observabilidade/                logs, métricas, alarmes
│
├── src/Tracbel.Crm.Integracao/       ← QUARENTENA do mundo legado.
│   └── Vortice/                        a ÚNICA pasta que conhece IV_Processo e SeqPessoa
│
├── src/Tracbel.Crm.Api/              ← ASP.NET Core Web API. Hospedada no IIS.
│
└── src/Tracbel.Crm.Web/              ← React + TypeScript (Vite). Consome só a Api.
```

### A regra de dependência

**As setas apontam sempre para dentro.** `Api` conhece `Aplicacao`; `Aplicacao` conhece `Dominio`;
**`Dominio` não conhece ninguém.** `Infraestrutura` implementa as interfaces do `Dominio`, mas o
`Dominio` não sabe que ela existe.

Isso não é purismo. É o que permite testar toda a regra de negócio **sem banco, sem HTTP e sem mock
complicado** — e é a razão de o portão de qualidade da fase (documento 06) ser exequível.

### Por que `Integracao` é uma camada separada

`[V]` O vocabulário do Vórtice é uma armadilha: `CodProcesso` **não é** o código do processo — é o tipo
de fluxo. `IV_Agenda.Vendedor` guarda um **login**, não um ID. `GE_Pessoa.Status` tem seis valores mais
o branco.

Se esses conceitos vazarem para o domínio, o CRM novo nasce contaminado e **nunca** conseguirá desligar
o Vórtice. Por isso: **só a pasta `Integracao/Vortice` pode pronunciar esses nomes.** Ela traduz na
fronteira e devolve objetos do nosso domínio. Quando o Vórtice morrer, apaga-se uma pasta.

---

## 2. Encapsulamento — entidade não é saco de propriedades

O erro mais comum em CRM é a *anemic domain model*: classes com `get; set;` público e toda a regra
espalhada em serviços. Aqui não.

### 2.1 A base de toda entidade

```csharp
namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Base de toda entidade do domínio.
///
/// Duas identidades, de propósito (ver documento 04, seção 1.3):
///  - <see cref="Id"/>: sequencial, interno, eficiente como índice clustered. Nunca sai daqui.
///  - <see cref="ChavePublica"/>: GUID, é o que aparece na URL e na API, para que ninguém
///    consiga enumerar registros nem inferir volume de negócio contando IDs.
/// </summary>
public abstract class EntidadeBase
{
    public long Id { get; protected set; }
    public Guid ChavePublica { get; protected set; } = Guid.NewGuid();

    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;
    public long CriadoPorId { get; protected set; }
    public DateTime? AlteradoEm { get; protected set; }
    public long? AlteradoPorId { get; protected set; }

    /// <summary>Soft delete. NULL = registro ativo.</summary>
    public DateTime? ExcluidoEm { get; protected set; }

    /// <summary>Controle de concorrência otimista, gerenciado pelo SQL Server (rowversion).</summary>
    public byte[] Versao { get; protected set; } = default!;

    /// <summary>
    /// Eventos de domínio acumulados nesta unidade de trabalho.
    /// Só são publicados DEPOIS do commit — ver seção 5.3. Publicar antes do commit é o erro
    /// que faz o sistema notificar sobre algo que a transação depois desfez.
    /// </summary>
    private readonly List<IEventoDominio> _eventos = new();
    public IReadOnlyCollection<IEventoDominio> Eventos => _eventos.AsReadOnly();

    protected void RegistrarEvento(IEventoDominio evento) => _eventos.Add(evento);
    public void LimparEventos() => _eventos.Clear();

    protected void MarcarAlteracao(long usuarioId)
    {
        AlteradoEm = DateTime.UtcNow;
        AlteradoPorId = usuarioId;
    }
}
```

Repare em `protected set`. **Ninguém fora da entidade muda o estado dela.** Isso não é burocracia: é o
que impede que uma tela, um job ou uma integração deixem o registro num estado que o negócio não admite —
que é exatamente como o Vórtice acumulou 345.535 órfãos e processos cancelados sem motivo.

### 2.2 A entidade de negócio expõe **comportamento**, não campos

```csharp
namespace Tracbel.Crm.Dominio.Crm;

/// <summary>
/// Lead — contato ainda não qualificado.
///
/// Modelagem herdada do Salesforce: tabela FLAT e autossuficiente (nome, empresa e contato no
/// mesmo registro), porque antes de qualificar não se sabe se existe conta, contato ou negócio.
///
/// Comportamento herdado do Dynamics: ao qualificar, o lead NÃO é apagado nem travado —
/// ele muda de situação e passa a apontar para o que gerou. Isso mantém a rastreabilidade
/// (de onde veio este cliente?) e permite auditar a decisão depois.
/// </summary>
public sealed class Lead : EntidadeBase
{
    // ---- Estado privado: o mundo externo lê, mas não escreve ----
    public int EmpresaId { get; private set; }
    public string NomeContato { get; private set; } = default!;
    public string? NomeEmpresa { get; private set; }
    public Email? Email { get; private set; }          // tipo de valor, valida no construtor
    public Telefone? Telefone { get; private set; }    // idem
    public SituacaoLead Situacao { get; private set; } = SituacaoLead.Novo;
    public long ProprietarioId { get; private set; }

    public DateTime? QualificadoEm { get; private set; }
    public long? QualificadoPorId { get; private set; }
    public long? ContaGeradaId { get; private set; }
    public long? ContatoGeradoId { get; private set; }
    public long? ProcessoGeradoId { get; private set; }

    /// <summary>
    /// O JSON exatamente como chegou do RD Station / site / chat.
    ///
    /// [V] No Vórtice, campo não mapeado na tela "Integração RD" é DESCARTADO — vira texto
    /// solto no detalhe do histórico e some. Aqui guardamos o payload íntegro SEMPRE, o que
    /// permite reprocessar um lead antigo quando o mapeamento for corrigido.
    /// </summary>
    public string? PayloadOriginal { get; private set; }

    // EF Core precisa de um construtor sem parâmetros. `private` para que ninguém mais use.
    private Lead() { }

    /// <summary>
    /// Único jeito de nascer um lead. Se os dados não servem, o objeto não existe —
    /// em vez de existir inválido e quebrar três camadas adiante.
    /// </summary>
    public static Lead Criar(
        int empresaId,
        string nomeContato,
        long proprietarioId,
        long criadoPorId,
        Email? email = null,
        Telefone? telefone = null,
        string? nomeEmpresa = null,
        string? payloadOriginal = null)
    {
        if (string.IsNullOrWhiteSpace(nomeContato))
            throw new RegraDeNegocioViolada("Lead precisa de um nome de contato.");

        // Um lead sem NENHUMA forma de contato é lixo: ninguém consegue trabalhá-lo.
        // Barrar na entrada é mais barato do que descobrir na carteira do vendedor.
        if (email is null && telefone is null)
            throw new RegraDeNegocioViolada("Lead precisa de e-mail ou telefone.");

        var lead = new Lead
        {
            EmpresaId = empresaId,
            NomeContato = nomeContato.Trim(),
            NomeEmpresa = nomeEmpresa?.Trim(),
            Email = email,
            Telefone = telefone,
            ProprietarioId = proprietarioId,
            CriadoPorId = criadoPorId,
            PayloadOriginal = payloadOriginal,
            Situacao = SituacaoLead.Novo
        };

        lead.RegistrarEvento(new LeadRecebido(lead.ChavePublica, empresaId));
        return lead;
    }

    /// <summary>
    /// Qualifica o lead, ligando-o aos registros criados.
    ///
    /// A transição é validada AQUI, dentro da entidade. É impossível qualificar duas vezes
    /// ou qualificar um lead descartado, porque não existe caminho de código que pule esta
    /// verificação — não há setter público de Situacao.
    /// </summary>
    public void Qualificar(long contaId, long contatoId, long? processoId, long usuarioId)
    {
        if (Situacao == SituacaoLead.Qualificado)
            throw new RegraDeNegocioViolada("Este lead já foi qualificado.");
        if (Situacao == SituacaoLead.Descartado)
            throw new RegraDeNegocioViolada("Lead descartado não pode ser qualificado. Reabra antes.");

        Situacao = SituacaoLead.Qualificado;
        QualificadoEm = DateTime.UtcNow;
        QualificadoPorId = usuarioId;
        ContaGeradaId = contaId;
        ContatoGeradoId = contatoId;
        ProcessoGeradoId = processoId;
        MarcarAlteracao(usuarioId);

        RegistrarEvento(new LeadQualificado(ChavePublica, contaId, processoId, usuarioId));
    }

    /// <summary>
    /// Descarta com motivo obrigatório.
    ///
    /// [V] No Vórtice, "Atividade Cancelada" cancelava o PROCESSO INTEIRO sem registrar
    /// motivo e sem desfazer. Aqui: motivo é obrigatório, e <see cref="Reabrir"/> existe.
    /// </summary>
    public void Descartar(int motivoId, string? observacao, long usuarioId)
    {
        if (Situacao == SituacaoLead.Qualificado)
            throw new RegraDeNegocioViolada("Lead já qualificado não pode ser descartado.");

        Situacao = SituacaoLead.Descartado;
        MotivoDescarteId = motivoId;
        MarcarAlteracao(usuarioId);
        RegistrarEvento(new LeadDescartado(ChavePublica, motivoId, observacao, usuarioId));
    }

    /// <summary>Desfazer existe. Sempre. Toda transição de estado é reversível ou justificada.</summary>
    public void Reabrir(string justificativa, long usuarioId)
    {
        if (Situacao != SituacaoLead.Descartado)
            throw new RegraDeNegocioViolada("Só lead descartado pode ser reaberto.");
        if (string.IsNullOrWhiteSpace(justificativa))
            throw new RegraDeNegocioViolada("Reabertura exige justificativa.");

        Situacao = SituacaoLead.Novo;
        MotivoDescarteId = null;
        MarcarAlteracao(usuarioId);
        RegistrarEvento(new LeadReaberto(ChavePublica, justificativa, usuarioId));
    }

    public int? MotivoDescarteId { get; private set; }
}
```

### 2.3 Tipos de valor — a validação mora no tipo

```csharp
namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Telefone brasileiro normalizado.
///
/// [V] `GE_Pessoa.FoneNro1` no Vórtice é `decimal(12)`. Telefone com DDI ou zero à esquerda
/// estoura o tipo e a API devolve `{"Message":"An error has occurred."}` — sem dizer o motivo.
/// Foram horas de investigação por causa de uma escolha de tipo.
///
/// Aqui o telefone é um TIPO. Quem tem um `Telefone` em mãos tem um telefone válido:
/// não existe caminho para construir um inválido.
/// </summary>
public readonly record struct Telefone
{
    /// <summary>Só dígitos, com DDD, sem DDI. Ex.: "17999990000".</summary>
    public string Numero { get; }

    private Telefone(string numero) => Numero = numero;

    public static Telefone Criar(string entrada)
    {
        if (!TentarCriar(entrada, out var telefone))
            throw new RegraDeNegocioViolada($"Telefone inválido: '{entrada}'.");
        return telefone;
    }

    /// <summary>
    /// Versão que não lança. Use na importação em massa, onde a linha ruim deve ser
    /// registrada e o lote deve continuar — nunca abortado inteiro.
    /// [V] o sync do mobile aborta a carga inteira numa única linha órfã.
    /// </summary>
    public static bool TentarCriar(string? entrada, out Telefone telefone)
    {
        telefone = default;
        if (string.IsNullOrWhiteSpace(entrada)) return false;

        // Tira tudo que não é dígito: "+55 (17) 99999-0000" -> "5517999990000"
        var digitos = new string(entrada.Where(char.IsDigit).ToArray());

        // Remove o DDI 55 quando o resto tem tamanho de número nacional.
        if (digitos.Length is 12 or 13 && digitos.StartsWith("55"))
            digitos = digitos[2..];

        // 10 = fixo com DDD; 11 = celular com DDD.
        if (digitos.Length is not (10 or 11)) return false;

        var ddd = int.Parse(digitos[..2]);
        if (ddd is < 11 or > 99) return false;

        telefone = new Telefone(digitos);
        return true;
    }

    /// <summary>Formatação para exibir. Guardamos cru; formatamos só na saída.</summary>
    public string Formatado() => Numero.Length == 11
        ? $"({Numero[..2]}) {Numero[2..7]}-{Numero[7..]}"
        : $"({Numero[..2]}) {Numero[2..6]}-{Numero[6..]}";

    public override string ToString() => Formatado();
}
```

---

## 3. Polimorfismo — onde ele paga

Polimorfismo por si só não é virtude; usado no lugar errado, esconde a lógica. Neste projeto ele resolve
exatamente **três** problemas.

### 3.1 Efeitos de regra — o `switch` que não existe

Uma regra de workflow pode criar tarefa, mover estágio, encerrar processo, notificar ou chamar webhook.
A implementação ingênua é um `switch` gigante que cresce a cada efeito novo. A implementação certa é uma
interface com uma implementação por efeito.

```csharp
namespace Tracbel.Crm.Dominio.Workflow.Efeitos;

/// <summary>
/// Contrato de um efeito de regra.
///
/// ESTE É O PONTO DE EXTENSÃO MAIS USADO DO SISTEMA. Para acrescentar uma capacidade nova
/// ao motor de workflow, um dev cria UMA classe que implementa esta interface e registra o
/// tipo na tabela `wf.Regra`. Não se toca em nenhum arquivo existente.
/// </summary>
public interface IEfeitoRegra
{
    /// <summary>Nome gravado em `wf.Regra.Efeito`. Precisa bater exatamente.</summary>
    string Nome { get; }

    /// <summary>
    /// Executa o efeito. NÃO lança exceção para caso de negócio: devolve
    /// <see cref="ResultadoEfeito"/>, porque "não disparou" é informação a registrar,
    /// não erro a engolir. É esta decisão que torna o defeito 899/900 impossível.
    /// </summary>
    Task<ResultadoEfeito> ExecutarAsync(ContextoRegra contexto, CancellationToken ct);
}

/// <summary>
/// Cria a próxima tarefa. É o efeito equivalente ao `IV_AcaoAuto` do Vórtice —
/// com as duas correções que faltavam lá.
/// </summary>
public sealed class EfeitoCriarTarefa : IEfeitoRegra
{
    private readonly IResolvedorDestinatario _resolvedor;
    private readonly ICalendarioUtil _calendario;

    public EfeitoCriarTarefa(IResolvedorDestinatario resolvedor, ICalendarioUtil calendario)
    {
        _resolvedor = resolvedor;
        _calendario = calendario;
    }

    public string Nome => "CriarTarefa";

    public async Task<ResultadoEfeito> ExecutarAsync(ContextoRegra ctx, CancellationToken ct)
    {
        // CORREÇÃO 1 — o destinatário é resolvido AGORA, por expressão.
        // [V] `IV_AcaoAuto.SeqUsuario` guarda um ID FIXO na linha da regra. É por isso que a
        // regra 9473 mandou 150 tarefas para o mesmo usuário desde abril, e por isso trocar o
        // líder de um CEN não corrige as aprovações já geradas: o Vórtice não reatribui.
        var destinatario = await _resolvedor.ResolverAsync(ctx.Regra.ExpressaoDestinatario, ctx, ct);

        if (destinatario is null)
        {
            // CORREÇÃO 2 — falhar é um RESULTADO REGISTRADO, nunca um silêncio.
            // No Vórtice isto simplesmente não geraria a tarefa, e ninguém saberia.
            return ResultadoEfeito.NaoAplicado(
                $"Não foi possível resolver o destinatário pela expressão " +
                $"'{ctx.Regra.ExpressaoDestinatario}'. Processo {ctx.Processo?.Numero}.");
        }

        var tipoTarefa = ctx.Regra.EfeitoTipoTarefaId!.Value;
        var prazo = _calendario.AdicionarDiasUteis(DateTime.UtcNow, ctx.PrazoDiasUteis);

        var tarefa = Tarefa.CriarPorRegra(
            empresaId: ctx.EmpresaId,
            processoId: ctx.Processo?.Id,
            contaId: ctx.Conta?.Id,
            tipoTarefaId: tipoTarefa,
            responsavelId: destinatario.UsuarioId,
            agendadaPara: prazo,
            regraId: ctx.Regra.Id,
            atividadeOrigemId: ctx.Atividade?.Id,
            origemAtribuicao: destinatario.Origem);

        ctx.RegistrarTarefaCriada(tarefa);
        return ResultadoEfeito.Aplicado($"Tarefa criada para {destinatario.NomeExibicao}.", tarefa.ChavePublica);
    }
}
```

Acrescentar o efeito "AtribuirCarteira" é criar `EfeitoAtribuirCarteira : IEfeitoRegra`. O motor
encontra por DI. **Nenhum arquivo existente muda.** É esse o significado prático de "qualquer dev do
time implementa coisa nova".

### 3.2 Resolução de destinatário — a estratégia

```csharp
namespace Tracbel.Crm.Dominio.Workflow.Destinatarios;

/// <summary>
/// Uma estratégia de resolução. Cada implementação entende UMA forma de dizer
/// "para quem vai a tarefa".
/// </summary>
public interface IEstrategiaDestinatario
{
    /// <summary>A expressão que esta estratégia sabe interpretar (ex.: "gestorDe(...)").</summary>
    bool Reconhece(string expressao);

    Task<Destinatario?> ResolverAsync(string expressao, ContextoRegra contexto, CancellationToken ct);
}

/// <summary>
/// "gestorDe(processo.Proprietario)" — resolve o gestor NO MOMENTO DA EXECUÇÃO.
///
/// [V] Este é o defeito 2.5 do Vórtice: a aprovação vai para o líder do CEN no momento em
/// que a agenda é GERADA, e fica gravada. Trocar o líder no cadastro corrige as aprovações
/// futuras, mas as já geradas continuam com o líder antigo — e o sistema não reatribui.
/// Como aqui a hierarquia é uma closure table (org.HierarquiaVendas) reconstruída na troca
/// de gestor, e a resolução acontece agora, o problema não existe.
/// </summary>
public sealed class EstrategiaGestorDe : IEstrategiaDestinatario
{
    private readonly IHierarquiaVendas _hierarquia;
    public EstrategiaGestorDe(IHierarquiaVendas hierarquia) => _hierarquia = hierarquia;

    public bool Reconhece(string expressao) =>
        expressao.StartsWith("gestorDe(", StringComparison.OrdinalIgnoreCase);

    public async Task<Destinatario?> ResolverAsync(string expressao, ContextoRegra ctx, CancellationToken ct)
    {
        var alvo = ExtrairArgumento(expressao);           // "processo.Proprietario"
        var usuarioId = ctx.ResolverReferenciaUsuario(alvo);
        if (usuarioId is null) return null;

        var gestor = await _hierarquia.ObterGestorDiretoAsync(usuarioId.Value, ct);
        return gestor is null
            ? null
            : new Destinatario(gestor.Id, gestor.NomeExibicao, OrigemAtribuicao.Hierarquia);
    }

    private static string ExtrairArgumento(string expressao)
    {
        var inicio = expressao.IndexOf('(') + 1;
        var fim = expressao.LastIndexOf(')');
        return expressao[inicio..fim].Trim();
    }
}
```

Estratégias previstas na fase 1: `processo.Proprietario` · `carteira.Responsavel` ·
`gestorDe(...)` · `equipe:CODIGO` · `quemExecutou` · `usuario:UPN` (o último desencorajado — é a forma
que produziu as 5.958 regras do Vórtice).

### 3.3 Herança: a atividade

```csharp
/// <summary>
/// Base de toda atividade. Inspirada no `ActivityPointer` do Dataverse, que é herança de
/// tabela REAL: a tabela base guarda toda atividade; a específica, só o extra.
///
/// Aqui usamos herança de UMA TABELA com discriminador (TPH — Table Per Hierarchy), porque
/// com um time de 3 pessoas a herança física custa mais em complexidade do que entrega em
/// pureza. O EF Core faz isso nativamente pela coluna `Tipo`.
///
/// Atividade é IMUTÁVEL: não há setter público nem método de alteração. Corrigir um
/// lançamento é registrar um estorno. [V] no Vórtice o histórico também é imutável — é um
/// dos acertos dele, e mantemos.
/// </summary>
public abstract class Atividade : EntidadeBase
{
    public int EmpresaId { get; protected set; }
    public long? ProcessoId { get; protected set; }
    public long? ContaId { get; protected set; }
    public long? TarefaId { get; protected set; }
    public string Assunto { get; protected set; } = default!;
    public DateTime OcorreuEm { get; protected set; }
    public NaturezaAtividade Natureza { get; protected set; }
    public long RegistradoPorId { get; protected set; }

    /// <summary>
    /// Cada tipo resume a si mesmo do seu jeito. É o que a linha do tempo exibe.
    /// Polimorfismo puro: a tela chama `Resumir()` e não sabe (nem precisa saber) o tipo.
    /// </summary>
    public abstract string Resumir();
}

/// <summary>Andamento de tarefa: o registro de que uma tarefa foi concluída com um desfecho.</summary>
public sealed class AtividadeAndamento : Atividade
{
    public int DesfechoId { get; private set; }
    public string? DesfechoComplemento { get; private set; }
    public string? Detalhe { get; private set; }

    public override string Resumir() =>
        $"{Assunto} — {DesfechoComplemento ?? "sem complemento"}";
}

/// <summary>Visita em campo. Guarda geolocalização, que a operação agrícola usa de verdade.</summary>
public sealed class AtividadeVisita : Atividade
{
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public int? DuracaoMinutos { get; private set; }

    public override string Resumir() =>
        $"Visita de {DuracaoMinutos ?? 0} min" +
        (Latitude.HasValue ? $" em {Latitude:F5}, {Longitude:F5}" : "");
}
```

---

## 4. O motor de workflow

### 4.1 O que ele faz, em ordem

```
Usuário conclui uma tarefa escolhendo um Desfecho
        │
        ▼
┌──────────────────────────────────────────────────────────────┐
│  MotorWorkflow.ProcessarAndamentoAsync()                      │
│  ─ tudo dentro de UMA transação ─                             │
│                                                               │
│  1. Valida: a tarefa é minha? está pendente? o desfecho vale? │
│  2. Grava a Atividade (fato imutável)                         │
│  3. Conclui a Tarefa, ligando-a à Atividade (duplo ponteiro)  │
│  4. Carrega as Regras cujo gatilho casa com este desfecho     │
│  5. PARA CADA regra:                                          │
│       a. avalia a Condição                                    │
│       b. se verdadeira, executa o Efeito                      │
│       c. GRAVA EM wf.RegraExecucao — disparou OU NÃO, e o     │
│          motivo em português. SEMPRE. Sem exceção.            │
│  6. Enfileira eventos no outbox (mesma transação)             │
│  7. COMMIT                                                    │
│  8. Só então publica os eventos                               │
└──────────────────────────────────────────────────────────────┘
```

O passo **5c** é o coração do projeto. É a diferença entre um sistema em que a taxa de geração de uma
tarefa cai de 68% para 3% ao longo de cinco meses sem ninguém notar, e um em que isso vira alarme no
dia seguinte.

### 4.2 O motor

```csharp
namespace Tracbel.Crm.Dominio.Workflow;

public sealed class MotorWorkflow
{
    private readonly IRepositorioRegras _regras;
    private readonly IAvaliadorCondicao _avaliador;
    private readonly IEnumerable<IEfeitoRegra> _efeitos;      // todas as implementações, via DI
    private readonly IRegistroExecucaoRegra _registro;
    private readonly IRelogio _relogio;

    public MotorWorkflow(
        IRepositorioRegras regras,
        IAvaliadorCondicao avaliador,
        IEnumerable<IEfeitoRegra> efeitos,
        IRegistroExecucaoRegra registro,
        IRelogio relogio)
    {
        _regras = regras;
        _avaliador = avaliador;
        _efeitos = efeitos;
        _registro = registro;
        _relogio = relogio;
    }

    /// <summary>
    /// Avalia todas as regras disparadas por um evento.
    ///
    /// CONTRATO: este método NUNCA lança por causa de regra que não disparou. Ele devolve o
    /// relatório do que aconteceu com cada uma. Só regra marcada como `EhCritica` aborta a
    /// transação — e mesmo assim, depois de registrar o motivo.
    /// </summary>
    public async Task<RelatorioExecucao> AvaliarAsync(ContextoRegra contexto, CancellationToken ct)
    {
        // Correlaciona tudo que aconteceu neste evento. É por este ID que o suporte
        // reconstrói "o que o sistema fez quando o vendedor clicou em concluir".
        var correlacaoId = Guid.NewGuid();
        var relatorio = new RelatorioExecucao(correlacaoId);

        var regras = await _regras.ObterPorGatilhoAsync(
            contexto.Evento, contexto.DesfechoId, contexto.TipoProcessoId, ct);

        foreach (var regra in regras.OrderBy(r => r.Ordem))
        {
            var inicio = _relogio.Agora;

            try
            {
                // --- Regra desligada ---
                if (!regra.EstaAtiva)
                {
                    await RegistrarAsync(regra, contexto, correlacaoId,
                        ResultadoRegra.RegraInativa,
                        "Regra está desativada (EstaAtiva = 0).", inicio, ct);
                    continue;
                }

                // --- Condição ---
                // [V] `IV_AcaoAutoCtrl` está VAZIA no Vórtice: a regra não tem condição.
                // Sem condição, a única saída é replicar a regra por filial — 5.958 regras
                // para 1.262 pares distintos. Uma expressão elimina essa explosão.
                if (!string.IsNullOrWhiteSpace(regra.Condicao))
                {
                    var avaliacao = await _avaliador.AvaliarAsync(regra.Condicao, contexto, ct);
                    if (!avaliacao.Verdadeira)
                    {
                        await RegistrarAsync(regra, contexto, correlacaoId,
                            ResultadoRegra.CondicaoFalsa,
                            // A explicação em português é o que o suporte vai ler.
                            // Ex.: "ValorEstimado (120000,00) não é maior que 500000,00"
                            avaliacao.Explicacao, inicio, ct);
                        continue;
                    }
                }

                // --- Efeito ---
                var efeito = _efeitos.FirstOrDefault(e => e.Nome == regra.Efeito);
                if (efeito is null)
                {
                    // Configuração aponta para efeito que não existe no código.
                    // Erro de operação, e precisa gritar.
                    await RegistrarAsync(regra, contexto, correlacaoId,
                        ResultadoRegra.Erro,
                        $"Efeito '{regra.Efeito}' não tem implementação registrada.", inicio, ct);
                    relatorio.RegistrarFalha(regra.Codigo, $"Efeito '{regra.Efeito}' inexistente.");
                    continue;
                }

                var resultado = await efeito.ExecutarAsync(contexto with { Regra = regra }, ct);

                await RegistrarAsync(regra, contexto, correlacaoId,
                    resultado.Aplicou ? ResultadoRegra.Disparou : ResultadoRegra.SemDestinatario,
                    resultado.Motivo, inicio, ct, resultado.TarefaCriadaId);

                relatorio.Registrar(regra.Codigo, resultado);
            }
            catch (Exception ex)
            {
                // Uma regra quebrada não derruba as outras. [V] no Vórtice as ações 899 e 900
                // caem JUNTAS enquanto 894 e 814 continuam — sinal de que o motor não isola
                // falha por regra. Aqui isola.
                await RegistrarAsync(regra, contexto, correlacaoId,
                    ResultadoRegra.Erro, $"Exceção: {ex.Message}", inicio, ct);

                relatorio.RegistrarFalha(regra.Codigo, ex.Message);
                if (regra.EhCritica) throw;   // crítica aborta — mas já ficou registrado
            }
        }

        return relatorio;
    }

    private async Task RegistrarAsync(
        Regra regra, ContextoRegra ctx, Guid correlacaoId, ResultadoRegra resultado,
        string? motivo, DateTime inicio, CancellationToken ct, Guid? tarefaCriada = null)
    {
        await _registro.GravarAsync(new ExecucaoRegra
        {
            RegraId = regra.Id,
            CorrelacaoId = correlacaoId,
            Evento = ctx.Evento,
            ProcessoId = ctx.Processo?.Id,
            TarefaId = ctx.Tarefa?.Id,
            AtividadeId = ctx.Atividade?.Id,
            Resultado = resultado,
            Motivo = motivo,
            DuracaoMs = (int)(_relogio.Agora - inicio).TotalMilliseconds
        }, ct);
    }
}
```

### 4.3 O alarme que fecha o ciclo

Registrar não basta — alguém precisa olhar. Um job diário compara a taxa de disparo de cada regra com
a média das quatro semanas anteriores:

```csharp
/// <summary>
/// Roda toda madrugada. Detecta regra que parou de disparar.
///
/// Este job é a resposta direta ao defeito mais caro do Vórtice: a ação 900 caiu de 68% (março)
/// para 3% (agosto) ao longo de cinco meses, e a queda só foi descoberta quando o comercial
/// reclamou de processos travados. Com este job, a queda de março teria aberto incidente em
/// abril.
/// </summary>
public sealed class MonitorSaudeRegras
{
    private const decimal QuedaQueAbreIncidente = 0.30m;   // 30%

    public async Task ExecutarAsync(CancellationToken ct)
    {
        var hoje = await _metricas.TaxaDisparoPorRegraAsync(dias: 7, ct);
        var base4Semanas = await _metricas.TaxaDisparoPorRegraAsync(dias: 28, offsetDias: 7, ct);

        foreach (var (regraId, taxaAtual) in hoje)
        {
            if (!base4Semanas.TryGetValue(regraId, out var taxaBase) || taxaBase == 0) continue;

            var queda = (taxaBase - taxaAtual) / taxaBase;
            if (queda < QuedaQueAbreIncidente) continue;

            await _incidentes.AbrirAsync(new Incidente(
                Severidade.Alta,
                $"Regra {regraId} caiu {queda:P0} na taxa de disparo",
                $"Média de 4 semanas: {taxaBase:P1}. Últimos 7 dias: {taxaAtual:P1}. " +
                $"Verifique wf.RegraExecucao filtrando por RegraId = {regraId} " +
                $"e agrupando por Resultado para ver o motivo predominante."), ct);
        }
    }
}
```

---

## 5. Pipeline de extensão

`[DYN]` O ponto de extensão é uma **linha de tabela**, não de código. Um plugin se registra em
`SdkMessageProcessingStep` — mensagem + tabela + estágio + modo + ordem — e o contrato é minúsculo.

### 5.1 O contrato

```csharp
namespace Tracbel.Crm.Dominio.Extensibilidade;

/// <summary>
/// Um manipulador de evento de domínio.
///
/// Para acrescentar comportamento ao sistema — validar algo, integrar com outro sistema,
/// disparar notificação — o dev cria uma classe que implementa esta interface e insere uma
/// linha em `meta.ManipuladorEvento`. Não se altera nenhum arquivo existente.
///
/// Três estágios, copiados do Dataverse:
///  - PreValidacao : antes da transação. Para validar e RECUSAR cedo, barato.
///  - PreOperacao  : dentro da transação, antes de gravar. Para MUTAR o próprio registro.
///  - PosOperacao  : dentro da transação, depois de gravar. Para EFEITOS COLATERAIS.
///
/// [SF] A regra de ouro que a Salesforce aprendeu com a Order of Execution: se você vai mutar
/// o próprio registro, faça ANTES do commit — mutar depois custa um segundo DML.
/// </summary>
public interface IManipuladorEvento<in TEvento> where TEvento : IEventoDominio
{
    EstagioExecucao Estagio { get; }

    /// <summary>Menor roda primeiro. Explícito, para não depender de ordem de descoberta.</summary>
    int Ordem => 100;

    Task ManipularAsync(TEvento evento, ContextoExecucao contexto, CancellationToken ct);
}
```

### 5.2 Um exemplo real

```csharp
/// <summary>
/// Quando um lead é qualificado, garante que a conta entra na carteira certa.
///
/// Exemplo didático: é assim que se acrescenta comportamento ao sistema. Uma classe nova,
/// uma linha em `meta.ManipuladorEvento`, zero alteração no núcleo.
/// </summary>
public sealed class VincularContaACarteiraAoQualificar : IManipuladorEvento<LeadQualificado>
{
    private readonly IRepositorioCarteiras _carteiras;
    private readonly IRepositorioContas _contas;
    private readonly ILogger<VincularContaACarteiraAoQualificar> _log;

    public VincularContaACarteiraAoQualificar(
        IRepositorioCarteiras carteiras,
        IRepositorioContas contas,
        ILogger<VincularContaACarteiraAoQualificar> log)
    {
        _carteiras = carteiras;
        _contas = contas;
        _log = log;
    }

    // PosOperacao: a conta já existe neste ponto, então podemos vinculá-la.
    public EstagioExecucao Estagio => EstagioExecucao.PosOperacao;
    public int Ordem => 50;   // roda antes das notificações (100)

    public async Task ManipularAsync(LeadQualificado evento, ContextoExecucao ctx, CancellationToken ct)
    {
        var conta = await _contas.ObterAsync(evento.ContaId, ct);
        if (conta is null) return;

        // Um cliente pertence a VÁRIAS carteiras, uma por linha de negócio.
        // [V] Este é o melhor ativo conceitual do Vórtice (IVS_Pes) e nem Salesforce nem
        // Dynamics fazem isso nativamente. Copiado de propósito.
        var carteira = await _carteiras.ObterPorCidadeELinhaAsync(
            conta.CidadeId, evento.LinhaNegocioId, ct);

        if (carteira is null)
        {
            // Não achar carteira NÃO é erro fatal: o lead foi qualificado e isso vale.
            // Mas precisa aparecer — senão vira o "processo órfão" do Vórtice.
            _log.LogWarning(
                "Conta {ContaId} qualificada sem carteira para a linha {Linha} na cidade {Cidade}. " +
                "Atribua manualmente.", conta.Id, evento.LinhaNegocioId, conta.CidadeId);
            return;
        }

        conta.VincularACarteira(carteira.Id, ctx.UsuarioId);
    }
}
```

### 5.3 Outbox — nunca há dado sem evento

```csharp
/// <summary>
/// Salva as alterações e o outbox na MESMA transação.
///
/// [V] A integração do Vórtice não tem transação: o `BEGIN TRANSACTION` está literalmente
/// comentado nas procedures. O resultado medido: 783.242 títulos (R$ 5,18 bi) presos em
/// staging desde maio de 2025 e 280.214 ordens de serviço que nunca foram promovidas.
///
/// Aqui, ou grava tudo, ou não grava nada. E o evento só é PUBLICADO depois do commit —
/// publicar antes é o erro que faz o sistema notificar sobre algo que a transação desfez.
/// </summary>
public async Task<int> SalvarComEventosAsync(CancellationToken ct)
{
    var entidades = ChangeTracker.Entries<EntidadeBase>()
        .Where(e => e.Entity.Eventos.Any())
        .Select(e => e.Entity)
        .ToList();

    var eventos = entidades.SelectMany(e => e.Eventos).ToList();

    // O evento vira linha de outbox DENTRO da transação do dado.
    foreach (var evento in eventos)
        MensagensSaida.Add(MensagemSaida.De(evento));

    entidades.ForEach(e => e.LimparEventos());

    var afetados = await base.SaveChangesAsync(ct);   // COMMIT

    // Só agora. Se o processo morrer aqui, o despachante do outbox entrega depois.
    await _despachante.PublicarAsync(eventos, ct);

    return afetados;
}
```

---

## 6. Segurança aplicada num ponto só

O detalhe está no documento 05. Aqui, a mecânica.

```csharp
/// <summary>
/// Filtro global de segurança por linha.
///
/// [V] O Vórtice NÃO tem permissão por registro: sem RLS no SQL Server, um único trigger no
/// banco inteiro, e toda a segurança feita no cliente Gupta. Acesso direto ao banco ignora
/// tudo e não deixa rastro.
///
/// Aqui a regra é aplicada pelo EF Core em TODA consulta da entidade, sempre. Não existe
/// caminho de leitura que escape — nem em relatório, nem em exportação, nem em job.
/// </summary>
protected override void OnModelCreating(ModelBuilder modelo)
{
    modelo.Entity<Processo>().HasQueryFilter(p =>
        // 1. Soft delete
        p.ExcluidoEm == null
        // 2. Escopo de empresa
        && _contexto.EmpresasVisiveis.Contains(p.EmpresaId)
        // 3. Escopo de registro, pela profundidade que o usuário tem em "Processo.Ler".
        //    [DYN] a matriz privilégio x profundidade resolve isso com colunas da própria
        //    linha (ProprietarioId, EmpresaId) — sem join, que é o que a torna barata.
        && (
               _contexto.Profundidade("Processo.Ler") >= Profundidade.Organizacao
            || (_contexto.Profundidade("Processo.Ler") >= Profundidade.Empresa
                && p.EmpresaId == _contexto.EmpresaId)
            || (_contexto.Profundidade("Processo.Ler") >= Profundidade.Equipe
                && _contexto.SubordinadosIds.Contains(p.ProprietarioId))
            || p.ProprietarioId == _contexto.UsuarioId
            // 4. Compartilhamento explícito
            || CompartilhamentosRegistro.Any(c =>
                   c.Entidade == "Processo"
                && c.RegistroId == p.Id
                && (c.UsuarioId == _contexto.UsuarioId
                    || _contexto.EquipesIds.Contains(c.EquipeId!.Value))
                && (c.ExpiraEm == null || c.ExpiraEm > DateTime.UtcNow))
        ));
}
```

**A regra do time:** consulta que precisa ignorar o filtro (job de sistema, migração) usa
`IgnoreQueryFilters()` **explicitamente**, e essa chamada é proibida fora de `Infraestrutura/Sistema/`
— verificado por teste de arquitetura no CI (seção 8).

---

## 7. API

- **REST**, versionada em `/api/v1/`, documentada por OpenAPI gerado do código.
- **Recursos identificados pela `ChavePublica`** (GUID), nunca pelo `Id` interno.
- **Autenticação:** JWT do Entra ID. Nenhuma senha é armazenada.
- **Regra de ouro:** `[V]` no Vórtice **não existe endpoint para criar usuário** — a tela faz, a API
  não. Aqui, **toda operação disponível na tela existe na API**, e a tela consome a mesma API. Não há
  caminho privilegiado.
- **Idempotência:** `POST` aceita header `Idempotency-Key`; a chave é gravada em `intg.ChaveExterna`.
- **Paginação por cursor**, nunca `OFFSET` em tabela grande.

```csharp
[ApiController]
[Route("api/v1/leads")]
[Authorize]
public sealed class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;
    public LeadsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Qualifica um lead: cria conta, contato e (opcionalmente) processo.</summary>
    /// <response code="200">Qualificado. Devolve as chaves dos registros criados.</response>
    /// <response code="409">Lead já qualificado ou descartado.</response>
    [HttpPost("{chave:guid}/qualificar")]
    [RequerPermissao("Lead.Qualificar")]
    [ProducesResponseType(typeof(LeadQualificadoResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Qualificar(
        Guid chave, [FromBody] QualificarLeadRequisicao requisicao, CancellationToken ct)
    {
        var resultado = await _mediator.Send(
            new QualificarLeadComando(chave, requisicao.CriarProcesso, requisicao.LinhaNegocioId), ct);

        // Erro de negócio NUNCA vira 500. Vira 409 com mensagem que o usuário entende.
        return resultado.EhSucesso
            ? Ok(resultado.Valor)
            : Conflict(resultado.ParaProblemDetails());
    }
}
```

---

## 8. Testes — o portão que define o fim da fase

O requisito é explícito: **uma fase só termina quando estiver 100% testada.** Traduzido em critérios
verificáveis:

| Camada | O que se testa | Ferramenta | Portão |
|---|---|---|---|
| Domínio | regra de negócio, transição de estado, tipos de valor | xUnit + FluentAssertions | **cobertura ≥ 90%** |
| Aplicação | caso de uso completo, com banco real | xUnit + Testcontainers (SQL Server) | **todo caso de uso tem teste** |
| Workflow | cada regra dispara e **cada regra NÃO dispara** pelo motivo certo | xUnit | **cobertura de regra = 100%** |
| Segurança | cada perfil × cada entidade × cada verbo | xUnit parametrizado | **matriz completa, sem exceção** |
| API | contrato, status codes, idempotência | WebApplicationFactory | **todo endpoint** |
| Arquitetura | as setas de dependência apontam para dentro | NetArchTest | **zero violação** |
| Frontend | componentes e fluxos principais | Vitest + Testing Library | **cobertura ≥ 70%** |
| E2E | os caminhos que o usuário percorre de verdade | Playwright | **os 5 fluxos da fase** |

```csharp
/// <summary>
/// Teste de arquitetura: impede que a regra de dependência seja violada por descuido.
/// Roda no CI. Falhou, não faz merge.
/// </summary>
[Fact]
public void Dominio_nao_pode_depender_de_infraestrutura()
{
    var resultado = Types.InAssembly(typeof(Lead).Assembly)
        .ShouldNot()
        .HaveDependencyOnAny(
            "Tracbel.Crm.Infraestrutura",
            "Tracbel.Crm.Api",
            "Microsoft.EntityFrameworkCore")
        .GetResult();

    resultado.IsSuccessful.Should().BeTrue(
        "o domínio precisa ser testável sem banco. Violações: {0}",
        string.Join(", ", resultado.FailingTypeNames ?? Array.Empty<string>()));
}

/// <summary>
/// O vocabulário do Vórtice não pode vazar da quarentena.
/// Se este teste falhar, alguém está construindo o próximo sistema legado.
/// </summary>
[Fact]
public void Vocabulario_do_Vortice_fica_confinado_na_camada_de_integracao()
{
    var proibidos = new[] { "SeqPessoa", "CodProcesso", "IV_", "GE_", "EXT_", "SeqAgenda" };

    var violacoes = Directory
        .EnumerateFiles("src/Tracbel.Crm.Dominio", "*.cs", SearchOption.AllDirectories)
        .Concat(Directory.EnumerateFiles("src/Tracbel.Crm.Aplicacao", "*.cs", SearchOption.AllDirectories))
        .Where(arquivo => proibidos.Any(p => File.ReadAllText(arquivo).Contains(p)))
        .ToList();

    violacoes.Should().BeEmpty(
        "termos do Vórtice só podem existir em Tracbel.Crm.Integracao");
}
```

---

## 9. Convenções de código

1. **Português no domínio.** `Conta`, `Processo`, `DarAndamento`. O negócio da Tracbel é discutido em
   português; traduzir para inglês só adiciona um passo de tradução em toda conversa.
2. **Inglês no técnico.** `Repository`, `Handler`, `Middleware`, `DbContext` — são termos da plataforma.
3. **Nada de abreviação.** `ProcessoRepositorio`, não `ProcRepo`. `[V]` `IV_ProcStatMonit` custou meia
   hora de investigação só para descobrir o que era.
4. **Comentário explica o PORQUÊ, não o quê.** `// incrementa o contador` é ruído. `// [V] o Vórtice
   trunca em 20 caracteres aqui; guardamos os 200` é conhecimento.
5. **Marcador `[V]`** em todo comentário que registre uma lição do legado. Torna rastreável a razão de
   cada decisão estranha — e evita que alguém "simplifique" de volta para o defeito.
6. **`async` sempre com `CancellationToken`**, propagado até o fim.
7. **Erro de negócio devolve `Resultado<T>`; erro de programação lança exceção.** Nunca o inverso.
8. **Um arquivo, um tipo.** Nome do arquivo = nome do tipo.
9. **Arquivo passou de 400 linhas?** É sinal de que faz coisa demais. Divida.

---

## 10. Infraestrutura de execução

| Item | Escolha | Por quê |
|---|---|---|
| Alvo do .NET | **net10.0** (LTS, suporte até 14/11/2028), declarado só no `Directory.Build.props`; SDK fixado em 10.0.401 pelo `global.json` | o .NET 8 e o 9 saem de suporte em 10/11/2026 (issue #84) |
| Runtime no servidor | **nenhum a instalar**: `publicar.ps1` gera pacote *self-contained* (`-r win-x64 --self-contained true`) | o servidor só tem os runtimes 6 e 8, e a publicação leva o dela — 131,8 MB no .NET 10, contra 123,0 MB no 9 |
| Hospedagem da API | ASP.NET Core no **IIS** (in-process) | é o que o TI da Tracbel opera hoje |
| Banco | **SQL Server 2019**, database `TRACBEL_CRM` | licença já existe; EF Core é maduro |
| Arquivos | Azure Blob ou file share dedicado | `[V]` o Doc Manager usa FTP com config no perfil do LocalSystem — inmanutenível |
| Jobs | **Hangfire** com storage no SQL Server | dashboard de execução e retry prontos; `[V]` o agendador do Vórtice não tem alarme |
| Logs | **Serilog** → arquivo + Seq (ou Application Insights) | log estruturado e consultável |
| Métricas | OpenTelemetry → Prometheus/Grafana | a taxa de disparo de regra é métrica de primeira classe |
| Frontend | React + Vite, servido pelo IIS como estático | sem SSR: não há necessidade e simplifica a operação |
| CI/CD | GitHub Actions → build, testes, migration, deploy | `[V]` o deploy do Vórtice é cópia de pasta, e copiar a pasta errada apaga as DLLs do ODBC |
| Ambientes | `dev` → `homolog` → `prod` | três, não quatro. `[SF]` a escada de 4 sandboxes é overhead de multi-tenant |

---

## 11. Como acrescentar uma funcionalidade — o guia de 6 passos

Este é o roteiro que responde ao seu requisito de "um CRM onde qualquer um consegue implementar
funcionalidades novas".

**Cenário:** "quando um lead de Grandes Contas for qualificado, notificar o gerente da regional."

1. **Evento já existe?** `LeadQualificado` existe. Não precisa mexer no núcleo.
2. **Crie a classe.**
   `src/Tracbel.Crm.Aplicacao/Leads/Manipuladores/NotificarGerenteRegionalGrandesContas.cs`,
   implementando `IManipuladorEvento<LeadQualificado>`.
3. **Escreva o teste primeiro.** Um caso que dispara, um que não dispara (lead que não é Grandes Contas),
   e um em que não há gerente definido.
4. **Registre.** Uma linha em `meta.ManipuladorEvento`: evento, tipo .NET, estágio `PosOperacao`, ordem.
5. **Rode a suíte.** Se o teste de arquitetura passar, você não violou a regra de dependência.
6. **Deploy.** A migration leva a linha de registro; o DI encontra a classe no boot.

**Arquivos existentes alterados: zero.**
