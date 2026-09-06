using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso à linha do tempo, sobre o <see cref="CrmDbContext"/>.
///
/// <para>É a tabela de maior volume do modelo — 122 mil linhas só do ano corrente, e milhões na
/// migração completa. Por isso a listagem <b>sempre</b> desce pelo índice de
/// <c>(ClienteId, OcorridaEm DESC)</c>, que é o mais importante do banco, e o filtro por cliente
/// ou por processo é o caminho normal de uso: a Visão 360 e a Cobertura nunca pedem a linha do
/// tempo inteira.</para>
///
/// <para><b>DÍVIDA NOMEADA, e é a hora dela.</b> A paginação continua por <c>OFFSET</c>, e o
/// documento 23 já registrou que esta é a tabela que deve nascer por cursor. Com o filtro por
/// cliente o <c>OFFSET</c> percorre dezenas de linhas e não dói; sem filtro, ele percorre a
/// tabela — e é por isso que a página sem filtro tem o mesmo teto de 200 das demais.</para>
/// </summary>
public sealed class RepositorioDeInteracoes(CrmDbContext contexto) : IRepositorioInteracoes
{
    /// <inheritdoc />
    public async Task<PaginaDe<InteracaoComContexto>> ListarAsync(
        ConsultaDeInteracoes consulta, CancellationToken ct)
    {
        var linhas = Filtrar(contexto.Interacoes.AsQueryable(), consulta);

        var total = await linhas.CountAsync(ct);

        var itens = await linhas
            .OrderByDescending(i => i.OcorridaEm)
            .ThenByDescending(i => i.Id)
            .Skip(consulta.Paginacao.Saltar)
            .Take(consulta.Paginacao.Tamanho)
            .Select(ComOContexto())
            .ToListAsync(ct);

        return new PaginaDe<InteracaoComContexto>(
            itens, consulta.Paginacao.Pagina, consulta.Paginacao.Tamanho, total);
    }

    private System.Linq.Expressions.Expression<Func<Interacao, InteracaoComContexto>> ComOContexto() =>
        i => new InteracaoComContexto(
            i,
            contexto.Clientes.Where(c => c.Id == i.ClienteId).Select(c => (Guid?)c.ChavePublica).FirstOrDefault(),
            contexto.Clientes.Where(c => c.Id == i.ClienteId).Select(c => c.NomeRazao).FirstOrDefault(),
            contexto.Processos.Where(p => p.Id == i.ProcessoId).Select(p => (Guid?)p.ChavePublica).FirstOrDefault(),
            contexto.TiposDeTarefa.Where(t => t.Id == i.TipoTarefaId).Select(t => t.Nome).FirstOrDefault()!,
            contexto.Resultados.Where(r => r.Id == i.ResultadoId).Select(r => r.Nome).FirstOrDefault(),
            contexto.Usuarios.Where(u => u.Id == i.RegistradoPorId).Select(u => u.NomeExibicao).FirstOrDefault()!);

    private static IQueryable<Interacao> Filtrar(
        IQueryable<Interacao> linhas, ConsultaDeInteracoes consulta)
    {
        if (consulta.ClienteId is { } cliente) linhas = linhas.Where(i => i.ClienteId == cliente);
        if (consulta.ProcessoId is { } processo) linhas = linhas.Where(i => i.ProcessoId == processo);
        if (consulta.AutorId is { } autor) linhas = linhas.Where(i => i.RegistradoPorId == autor);
        if (consulta.Natureza is { } natureza) linhas = linhas.Where(i => i.Natureza == natureza);

        if (consulta.De is { } de)
        {
            var inicio = DataHoraUtc.Criar(
                DateTime.SpecifyKind(de.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc));

            linhas = linhas.Where(i => i.OcorridaEm >= inicio);
        }

        if (consulta.Ate is { } ate)
        {
            var fim = DataHoraUtc.Criar(
                DateTime.SpecifyKind(ate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc));

            linhas = linhas.Where(i => i.OcorridaEm < fim);
        }

        return linhas;
    }
}
