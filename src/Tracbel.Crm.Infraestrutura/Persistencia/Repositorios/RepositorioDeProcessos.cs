using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso a processos, sobre o <see cref="CrmDbContext"/>.
///
/// <para>Vale aqui a mesma coisa que vale no cadastro: <b>não há um único <c>Where</c> de empresa
/// neste arquivo</b>, e mesmo assim nenhuma consulta atravessa a fronteira de filial. O filtro
/// global entra em toda consulta da entidade, antes de qualquer coisa que este repositório
/// escreva — e entra também nos AGREGADOS, que é o ponto onde o legado mais vaza: [V] dos 134
/// relatórios do sistema de origem, 125 não têm predicado de usuário nenhum.</para>
///
/// <para><b>O agregado é do banco.</b> O funil e as perdas saem de um <c>GROUP BY</c>, e não de
/// uma lista trazida para somar na memória. A diferença entre as duas coisas, num funil de 45
/// mil processos, é entre uma consulta e quarenta e cinco mil linhas atravessando a rede para
/// virar quinze números.</para>
/// </summary>
public sealed class RepositorioDeProcessos(CrmDbContext contexto) : IRepositorioProcessos
{
    /// <inheritdoc />
    public async Task<PaginaDe<ProcessoComContexto>> ListarAsync(
        ConsultaDeProcessos consulta, CancellationToken ct)
    {
        var linhas = Filtrar(contexto.Processos.AsQueryable(), consulta);

        var total = await linhas.CountAsync(ct);

        var itens = await Ordenar(linhas, consulta)
            .Skip(consulta.Paginacao.Saltar)
            .Take(consulta.Paginacao.Tamanho)
            .Select(ComOContexto())
            .ToListAsync(ct);

        return new PaginaDe<ProcessoComContexto>(
            itens, consulta.Paginacao.Pagina, consulta.Paginacao.Tamanho, total);
    }

    /// <inheritdoc />
    public Task<ProcessoComContexto?> ObterAsync(Guid chavePublica, CancellationToken ct) =>
        contexto.Processos
            .Where(p => p.ChavePublica == chavePublica)
            .Select(ComOContexto())
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<FatiaDoFunil>> ResumirFunilAsync(
        int? tipoProcessoId, CancellationToken ct)
    {
        var linhas = contexto.Processos.Where(p => p.ExcluidoEm == null && p.Situacao == SituacaoDoProcesso.Aberto);

        if (tipoProcessoId is { } tipo) linhas = linhas.Where(p => p.TipoProcessoId == tipo);

        // O AGRUPAMENTO É DO BANCO, e as duas contagens andam juntas: quantos processos estão na
        // fase e quantos deles declaram valor. Sem a segunda, a soma da primeira parece o valor
        // do funil — e não é.
        var agrupado = await linhas
            .GroupBy(p => new { p.TipoProcessoId, p.FaseId })
            .Select(g => new
            {
                g.Key.TipoProcessoId,
                g.Key.FaseId,
                Processos = g.Count(),
                ProcessosComValor = g.Count(p => p.ValorEstimado != null)
            })
            .ToListAsync(ct);

        // A SOMA É UMA SEGUNDA CONSULTA, e a razão é a dívida D-2 do documento 23 aparecendo de
        // novo: `ValorEstimado` é um tipo de valor com conversor, e `SUM` sobre ele exige que o
        // EF entenda `.Value.Valor` — acesso a membro dentro de um valor convertido, que não
        // traduz para SQL. A alternativa não é somar na tela: é o BANCO devolver só as linhas que
        // TÊM valor, que são as únicas que a soma alcança. Aqui isso é 0,8% dos processos, e a
        // contagem acima já disse exatamente quantas linhas isso é — não é um `ToList()` cego.
        var comValor = await linhas
            .Where(p => p.ValorEstimado != null)
            .Select(p => new { p.TipoProcessoId, p.FaseId, p.ValorEstimado })
            .ToListAsync(ct);

        var somaPorFase = comValor
            .GroupBy(v => (v.TipoProcessoId, v.FaseId))
            .ToDictionary(g => g.Key, g => g.Sum(v => v.ValorEstimado!.Value.Valor));

        var tipos = await contexto.TiposDeProcesso.AsNoTracking()
            .Select(t => new { t.Id, t.Codigo, t.Nome })
            .ToDictionaryAsync(t => t.Id, ct);

        var fases = await contexto.Fases.AsNoTracking()
            .Select(f => new { f.Id, f.Codigo, f.Nome, f.Ordem })
            .ToDictionaryAsync(f => f.Id, ct);

        return
        [
            .. agrupado
                .Where(a => tipos.ContainsKey(a.TipoProcessoId) && fases.ContainsKey(a.FaseId))
                .Select(a => new FatiaDoFunil(
                    tipos[a.TipoProcessoId].Codigo,
                    tipos[a.TipoProcessoId].Nome,
                    fases[a.FaseId].Codigo,
                    fases[a.FaseId].Nome,
                    fases[a.FaseId].Ordem,
                    a.Processos,
                    a.ProcessosComValor,
                    somaPorFase.TryGetValue((a.TipoProcessoId, a.FaseId), out var soma) ? soma : null))
                .OrderBy(f => f.TipoProcessoCodigo)
                .ThenBy(f => f.FaseOrdem)
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ContagemPorRotulo>> ResumirPerdasAsync(CancellationToken ct)
    {
        var agrupado = await contexto.Processos
            .Where(p => p.ExcluidoEm == null && p.Situacao == SituacaoDoProcesso.Perdido)
            .GroupBy(p => p.MotivoDePerdaId)
            .Select(g => new { MotivoId = g.Key, Quantidade = g.Count() })
            .ToListAsync(ct);

        var motivos = await contexto.MotivosDePerda.AsNoTracking()
            .Select(m => new { m.Id, m.Codigo, m.Nome })
            .ToDictionaryAsync(m => m.Id, ct);

        return
        [
            .. agrupado
                .Select(a => new ContagemPorRotulo(
                    a.MotivoId is { } id && motivos.ContainsKey(id) ? motivos[id].Codigo : "SEM_MOTIVO",
                    a.MotivoId is { } id2 && motivos.ContainsKey(id2)
                        ? motivos[id2].Nome
                        : "Sem motivo registrado",
                    a.Quantidade))
                .OrderByDescending(c => c.Quantidade)
        ];
    }

    /// <summary>
    /// Traz, junto do processo, o que a tela mostra ao lado dele.
    ///
    /// São subconsultas correlacionadas resolvidas na mesma ida ao banco. A alternativa é o
    /// N+1 silencioso: uma listagem de 25 cartões vira 126 consultas.
    /// </summary>
    private System.Linq.Expressions.Expression<Func<Processo, ProcessoComContexto>> ComOContexto() =>
        p => new ProcessoComContexto(
            p,
            contexto.Clientes.Where(c => c.Id == p.ClienteId).Select(c => c.ChavePublica).FirstOrDefault(),
            contexto.Clientes.Where(c => c.Id == p.ClienteId).Select(c => c.NomeRazao).FirstOrDefault()!,
            contexto.TiposDeProcesso.Where(t => t.Id == p.TipoProcessoId).Select(t => t.Codigo).FirstOrDefault()!,
            contexto.TiposDeProcesso.Where(t => t.Id == p.TipoProcessoId).Select(t => t.Nome).FirstOrDefault()!,
            contexto.Fases.Where(f => f.Id == p.FaseId).Select(f => f.Codigo).FirstOrDefault()!,
            contexto.Fases.Where(f => f.Id == p.FaseId).Select(f => f.Nome).FirstOrDefault()!,
            contexto.Fases.Where(f => f.Id == p.FaseId).Select(f => f.Ordem).FirstOrDefault(),
            contexto.Usuarios.Where(u => u.Id == p.ProprietarioId).Select(u => u.NomeExibicao).FirstOrDefault(),
            contexto.MotivosDePerda.Where(m => m.Id == p.MotivoDePerdaId).Select(m => m.Codigo).FirstOrDefault());

    private static IQueryable<Processo> Filtrar(IQueryable<Processo> linhas, ConsultaDeProcessos consulta)
    {
        linhas = linhas.Where(p => p.ExcluidoEm == null);

        if (!consulta.IncluirEncerrados && consulta.Situacao is null)
            linhas = linhas.Where(p =>
                p.Situacao == SituacaoDoProcesso.Aberto || p.Situacao == SituacaoDoProcesso.Suspenso);

        if (consulta.Situacao is { } situacao) linhas = linhas.Where(p => p.Situacao == situacao);
        if (consulta.TipoProcessoId is { } tipo) linhas = linhas.Where(p => p.TipoProcessoId == tipo);
        if (consulta.FaseId is { } fase) linhas = linhas.Where(p => p.FaseId == fase);
        if (consulta.ClienteId is { } cliente) linhas = linhas.Where(p => p.ClienteId == cliente);
        if (consulta.ProprietarioId is { } dono) linhas = linhas.Where(p => p.ProprietarioId == dono);

        if (consulta.AbertoDe is { } de)
            linhas = linhas.Where(p => p.CriadoEm >= de.ToDateTime(TimeOnly.MinValue));

        if (consulta.AbertoAte is { } ate)
            linhas = linhas.Where(p => p.CriadoEm < ate.AddDays(1).ToDateTime(TimeOnly.MinValue));

        if (!string.IsNullOrWhiteSpace(consulta.Termo))
        {
            var termo = consulta.Termo.Trim();

            // O NÚMERO É COMPARADO POR IGUALDADE, o título por trecho. Buscar "1517613" numa
            // coluna numérica com LIKE forçaria conversão em toda linha e desligaria o índice.
            var ehNumero = long.TryParse(termo, out var numero);

            linhas = linhas.Where(p => p.Titulo.Contains(termo) || (ehNumero && p.Numero == numero));
        }

        return linhas;
    }

    private static IQueryable<Processo> Ordenar(IQueryable<Processo> linhas, ConsultaDeProcessos consulta) =>
        (consulta.Ordem, consulta.Descendente) switch
        {
            (OrdemDeProcesso.Numero, false) => linhas.OrderBy(p => p.Numero),
            (OrdemDeProcesso.Numero, true) => linhas.OrderByDescending(p => p.Numero),

            (OrdemDeProcesso.Titulo, false) => linhas.OrderBy(p => p.Titulo).ThenBy(p => p.Id),
            (OrdemDeProcesso.Titulo, true) => linhas.OrderByDescending(p => p.Titulo).ThenBy(p => p.Id),

            (OrdemDeProcesso.ValorEstimado, false) => linhas.OrderBy(p => p.ValorEstimado).ThenBy(p => p.Id),
            (OrdemDeProcesso.ValorEstimado, true) =>
                linhas.OrderByDescending(p => p.ValorEstimado).ThenBy(p => p.Id),

            (OrdemDeProcesso.PrevisaoConclusao, false) =>
                linhas.OrderBy(p => p.PrevisaoConclusao).ThenBy(p => p.Id),
            (OrdemDeProcesso.PrevisaoConclusao, true) =>
                linhas.OrderByDescending(p => p.PrevisaoConclusao).ThenBy(p => p.Id),

            (OrdemDeProcesso.CriadoEm, false) => linhas.OrderBy(p => p.CriadoEm).ThenBy(p => p.Id),
            (OrdemDeProcesso.CriadoEm, true) => linhas.OrderByDescending(p => p.CriadoEm).ThenBy(p => p.Id),

            (_, true) => linhas.OrderByDescending(p => p.FaseDesde).ThenBy(p => p.Id),
            _ => linhas.OrderBy(p => p.FaseDesde).ThenBy(p => p.Id)
        };
}
