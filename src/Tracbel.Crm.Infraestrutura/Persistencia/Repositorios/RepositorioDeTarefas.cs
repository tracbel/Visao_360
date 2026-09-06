using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso à agenda, sobre o <see cref="CrmDbContext"/>.
///
/// <para>A listagem usa o índice parcial de <c>(ResponsavelId, AgendadaPara)</c> filtrado em
/// pendente — é a consulta mais quente do sistema, porque é a tela que o CEN abre de manhã.</para>
///
/// <para><b>O painel é uma consulta só.</b> Sete números, sete <c>COUNT</c> condicionais numa
/// projeção — e não sete idas ao banco. [V] a tela equivalente do legado é uma lista sem
/// agregado nenhum, e é por isso que ninguém lá sabe dizer o tamanho do próprio passivo sem
/// pedir consulta a alguém.</para>
/// </summary>
public sealed class RepositorioDeTarefas(CrmDbContext contexto) : IRepositorioTarefas
{
    /// <inheritdoc />
    public async Task<PaginaDe<TarefaComContexto>> ListarAsync(
        ConsultaDeTarefas consulta, CancellationToken ct)
    {
        var linhas = Filtrar(contexto.Tarefas.AsQueryable(), consulta);

        var total = await linhas.CountAsync(ct);

        var itens = await Ordenar(linhas, consulta)
            .Skip(consulta.Paginacao.Saltar)
            .Take(consulta.Paginacao.Tamanho)
            .Select(ComOContexto())
            .ToListAsync(ct);

        return new PaginaDe<TarefaComContexto>(
            itens, consulta.Paginacao.Pagina, consulta.Paginacao.Tamanho, total);
    }

    /// <inheritdoc />
    public async Task<PainelDaAgenda> ResumirAgendaAsync(
        long? responsavelId, DateTime agoraUtc, CancellationToken ct)
    {
        var linhas = contexto.Tarefas.Where(t => t.ExcluidoEm == null);

        if (responsavelId is { } dono) linhas = linhas.Where(t => t.ResponsavelId == dono);

        var inicioDeHoje = agoraUtc.Date;
        var fimDeHoje = inicioDeHoje.AddDays(1);
        var seteDias = inicioDeHoje.AddDays(8);
        var trintaDiasAtras = inicioDeHoje.AddDays(-30);

        var painel = await linhas
            .GroupBy(_ => 1)
            .Select(g => new PainelDaAgenda(
                g.Count(t => t.Situacao == SituacaoDaTarefa.Pendente
                             || t.Situacao == SituacaoDaTarefa.EmAndamento),
                g.Count(t => (t.Situacao == SituacaoDaTarefa.Pendente
                              || t.Situacao == SituacaoDaTarefa.EmAndamento)
                             && t.AgendadaPara < inicioDeHoje),
                g.Count(t => (t.Situacao == SituacaoDaTarefa.Pendente
                              || t.Situacao == SituacaoDaTarefa.EmAndamento)
                             && t.AgendadaPara >= inicioDeHoje && t.AgendadaPara < fimDeHoje),
                g.Count(t => (t.Situacao == SituacaoDaTarefa.Pendente
                              || t.Situacao == SituacaoDaTarefa.EmAndamento)
                             && t.AgendadaPara >= fimDeHoje && t.AgendadaPara < seteDias),
                g.Count(t => t.Situacao == SituacaoDaTarefa.Concluida && t.ConcluidaEm >= trintaDiasAtras),
                g.Count(t => (t.Situacao == SituacaoDaTarefa.Pendente
                              || t.Situacao == SituacaoDaTarefa.EmAndamento)
                             && t.PrazoLimite == null),
                g.Min(t => t.Situacao == SituacaoDaTarefa.Pendente
                           || t.Situacao == SituacaoDaTarefa.EmAndamento
                    ? (DateTime?)t.AgendadaPara
                    : null)))
            .FirstOrDefaultAsync(ct);

        return painel ?? new PainelDaAgenda(0, 0, 0, 0, 0, 0, null);
    }

    private System.Linq.Expressions.Expression<Func<Tarefa, TarefaComContexto>> ComOContexto() =>
        t => new TarefaComContexto(
            t,
            contexto.Clientes.Where(c => c.Id == t.ClienteId).Select(c => (Guid?)c.ChavePublica).FirstOrDefault(),
            contexto.Clientes.Where(c => c.Id == t.ClienteId).Select(c => c.NomeRazao).FirstOrDefault(),
            contexto.Processos.Where(p => p.Id == t.ProcessoId).Select(p => (Guid?)p.ChavePublica).FirstOrDefault(),
            contexto.Processos.Where(p => p.Id == t.ProcessoId).Select(p => p.Titulo).FirstOrDefault(),
            contexto.TiposDeTarefa.Where(x => x.Id == t.TipoTarefaId).Select(x => x.Codigo).FirstOrDefault()!,
            contexto.TiposDeTarefa.Where(x => x.Id == t.TipoTarefaId).Select(x => x.Nome).FirstOrDefault()!,
            contexto.Usuarios.Where(u => u.Id == t.ResponsavelId).Select(u => u.NomeExibicao).FirstOrDefault()!,
            contexto.Resultados.Where(r => r.Id == t.ResultadoId).Select(r => r.Nome).FirstOrDefault());

    private static IQueryable<Tarefa> Filtrar(IQueryable<Tarefa> linhas, ConsultaDeTarefas consulta)
    {
        linhas = linhas.Where(t => t.ExcluidoEm == null);

        if (consulta.ResponsavelId is { } dono) linhas = linhas.Where(t => t.ResponsavelId == dono);
        if (consulta.Situacao is { } situacao) linhas = linhas.Where(t => t.Situacao == situacao);
        if (consulta.ClienteId is { } cliente) linhas = linhas.Where(t => t.ClienteId == cliente);
        if (consulta.ProcessoId is { } processo) linhas = linhas.Where(t => t.ProcessoId == processo);

        if (consulta.De is { } de)
            linhas = linhas.Where(t => t.AgendadaPara >= de.ToDateTime(TimeOnly.MinValue));

        if (consulta.Ate is { } ate)
            linhas = linhas.Where(t => t.AgendadaPara < ate.AddDays(1).ToDateTime(TimeOnly.MinValue));

        // ATRASADA É "PASSOU DA DATA E NÃO FOI CONCLUÍDA", e não "passou do prazo limite": o
        // prazo limite existe em menos de um quinto das tarefas migradas, porque a origem não
        // declara prazo em nenhuma das ações em uso. Medir atraso pelo prazo faria 80% da agenda
        // parecer em dia por falta de dado.
        if (consulta.SomenteAtrasadas)
        {
            var agora = DateTime.UtcNow;
            linhas = linhas.Where(t =>
                (t.Situacao == SituacaoDaTarefa.Pendente || t.Situacao == SituacaoDaTarefa.EmAndamento)
                && t.AgendadaPara < agora);
        }

        return linhas;
    }

    private static IQueryable<Tarefa> Ordenar(IQueryable<Tarefa> linhas, ConsultaDeTarefas consulta) =>
        (consulta.Ordem, consulta.Descendente) switch
        {
            (OrdemDeTarefa.Prioridade, false) =>
                linhas.OrderBy(t => t.Prioridade).ThenBy(t => t.AgendadaPara),
            (OrdemDeTarefa.Prioridade, true) =>
                linhas.OrderByDescending(t => t.Prioridade).ThenBy(t => t.AgendadaPara),

            (OrdemDeTarefa.PrazoLimite, false) => linhas.OrderBy(t => t.PrazoLimite).ThenBy(t => t.Id),
            (OrdemDeTarefa.PrazoLimite, true) =>
                linhas.OrderByDescending(t => t.PrazoLimite).ThenBy(t => t.Id),

            (OrdemDeTarefa.Assunto, false) => linhas.OrderBy(t => t.Assunto).ThenBy(t => t.Id),
            (OrdemDeTarefa.Assunto, true) => linhas.OrderByDescending(t => t.Assunto).ThenBy(t => t.Id),

            (_, true) => linhas.OrderByDescending(t => t.AgendadaPara).ThenBy(t => t.Id),
            _ => linhas.OrderBy(t => t.AgendadaPara).ThenBy(t => t.Id)
        };
}
