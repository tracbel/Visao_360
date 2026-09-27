using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso às vendas perdidas, sobre o <see cref="CrmDbContext"/>.
///
/// <para>Como nos outros repositórios, <b>não há um <c>Where</c> de empresa neste arquivo</b>: a
/// fronteira de filial entra sozinha, pelo filtro global, porque <c>VendaPerdida</c> tem
/// <c>EmpresaId</c>. Vale também para os agregados abaixo — que é onde o legado mais vazava.</para>
///
/// <para><b>Os dois agrupamentos saem de um <c>GROUP BY</c> no banco</b>, e trazem o denominador
/// junto: a diferença média de preço só se calcula sobre as linhas que declararam os DOIS
/// preços, e <c>ComOsDoisPrecos</c> viaja com a média para a tela poder dizer sobre quantas ela
/// foi feita. Média sem denominador é exatamente o defeito que este CRM existe para não
/// repetir.</para>
///
/// <para><b>Só a principal conta</b> (decisão de 27/09/2026): o <c>_JDE</c> gêmeo do antigo e o <c>SEM_PARTICIPACAO</c>
/// gêmeo do FY25 repetem a mesma perda, e os <c>VP_*</c> a detalham. Contar os três faria a perda contar em dobro e o
/// preço do gêmeo entrar na média.</para>
/// </summary>
public sealed class RepositorioDeVendasPerdidas(CrmDbContext contexto) : IRepositorioVendasPerdidas
{
    /// <inheritdoc />
    public Task<int> ContarAsync(CancellationToken ct) =>
        contexto.VendasPerdidas.Where(v => v.ExcluidoEm == null && v.Papel == PapelDaVendaPerdida.Principal).CountAsync(ct);

    /// <summary>
    /// AS PRINCIPAIS DO RECORTE. O período é o da data em que o CEN preencheu o formulário (<c>RegistradaEm</c>), a única
    /// que toda resposta tem: a data da perda declarada (<c>OcorridaEm</c>) falta em boa parte do histórico, e filtrar por
    /// ela faria a perda sem data sumir de todo período. O responsável é o do processo no funil, ligado pelo número do
    /// processo no Vórtice — a resposta de formulário não tem dono próprio.
    /// </summary>
    private IQueryable<VendaPerdida> Principais(FiltroDeVendaPerdida filtro)
    {
        var consulta = contexto.VendasPerdidas.AsNoTracking()
            .Where(v => v.ExcluidoEm == null
                        && v.Papel == PapelDaVendaPerdida.Principal
                        && v.RegistradaEm >= filtro.DeUtc
                        && v.RegistradaEm < filtro.AteUtc);

        if (filtro.Formulario is { } formulario)
            consulta = consulta.Where(v => v.FormularioDeOrigem == formulario);

        if (filtro.Responsavel is { } chave)
            consulta = consulta.Where(v => contexto.EstagiosDoProcesso.Any(e =>
                e.NumeroDoProcessoNaOrigem == v.NumeroDoProcessoNaOrigem
                && e.Estagio == EstagioDoFunil.Lead
                && contexto.Usuarios.Any(u => u.Id == e.ResponsavelId && u.ChavePublica == chave)));

        return consulta;
    }

    /// <inheritdoc />
    public Task<int> ContarProcessosPerdidosAsync(FiltroDeVendaPerdida filtro, CancellationToken ct)
    {
        // A LINHA DO LEAD É O PROCESSO: o estágio é cumulativo, e todo processo do funil tem a dele — contar só ela
        // conta cada processo uma vez. A data é a do desfecho; sem ela, a da abertura.
        var consulta = contexto.EstagiosDoProcesso.AsNoTracking()
            .Where(e => e.Estagio == EstagioDoFunil.Lead
                        && e.Desfecho == SituacaoDoProcesso.Perdido
                        && (e.DesfechoEm ?? e.AbertoEm) >= filtro.DeUtc
                        && (e.DesfechoEm ?? e.AbertoEm) < filtro.AteUtc);

        if (filtro.Responsavel is { } chave)
            consulta = consulta.Where(e => contexto.Usuarios.Any(u => u.Id == e.ResponsavelId && u.ChavePublica == chave));

        return consulta.CountAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FatiaDeVendaPerdida>> ResumirPorMotivoAsync(FiltroDeVendaPerdida filtro, CancellationToken ct)
    {
        var agrupado = await Principais(filtro)
            .GroupBy(v => v.MotivoDePerdaId)
            .Select(g => new Bruto(
                g.Key,
                g.Count(),
                g.Sum(v => v.Quantidade),
                g.Count(v => v.PrecoDoConcorrente != null && v.PrecoOfertado != null),
                g.Average(v => v.PrecoDoConcorrente != null && v.PrecoOfertado != null
                    ? v.PrecoOfertado - v.PrecoDoConcorrente
                    : null)))
            .ToListAsync(ct);

        var motivos = await contexto.MotivosDePerda.AsNoTracking()
            .Select(m => new { m.Id, m.Codigo, m.Nome })
            .ToDictionaryAsync(m => m.Id, ct);

        return
        [
            .. agrupado
                .Select(a => new FatiaDeVendaPerdida(
                    motivos.TryGetValue(a.Chave, out var motivo) ? motivo.Codigo : "SEM_MOTIVO",
                    motivos.TryGetValue(a.Chave, out var nome) ? nome.Nome : "Sem motivo registrado",
                    a.Quantidade,
                    a.Maquinas,
                    a.ComOsDoisPrecos,
                    Arredondar(a.DiferencaMedia)))
                .OrderByDescending(f => f.Quantidade)
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FatiaDeVendaPerdida>> ResumirPorConcorrenteAsync(FiltroDeVendaPerdida filtro, CancellationToken ct)
    {
        // A LINHA SEM CONCORRENTE FICA DE FORA, e não vira "não informado": um ranking de para
        // quem se perdeu com uma fatia "não sei" no topo não responde nada. Quantas linhas não
        // declaram o concorrente é conta de outra pergunta, e a tela a faz pelo total.
        var agrupado = await Principais(filtro)
            .Where(v => v.ConcorrenteId != null)
            .GroupBy(v => v.ConcorrenteId!.Value)
            .Select(g => new Bruto(
                g.Key,
                g.Count(),
                g.Sum(v => v.Quantidade),
                g.Count(v => v.PrecoDoConcorrente != null && v.PrecoOfertado != null),
                g.Average(v => v.PrecoDoConcorrente != null && v.PrecoOfertado != null
                    ? v.PrecoOfertado - v.PrecoDoConcorrente
                    : null)))
            .ToListAsync(ct);

        var itens = await contexto.CatalogoItens.AsNoTracking()
            .Where(i => i.CatalogoId == CatalogosDeSistema.Concorrente)
            .Select(i => new { i.Id, i.Codigo, i.Descricao })
            .ToDictionaryAsync(i => i.Id, ct);

        return
        [
            .. agrupado
                .Select(a => new FatiaDeVendaPerdida(
                    itens.TryGetValue(a.Chave, out var item) ? item.Codigo : "DESCONHECIDO",
                    itens.TryGetValue(a.Chave, out var nome) ? nome.Descricao : "Concorrente não catalogado",
                    a.Quantidade,
                    a.Maquinas,
                    a.ComOsDoisPrecos,
                    Arredondar(a.DiferencaMedia)))
                .OrderByDescending(f => f.Quantidade)
        ];
    }

    /// <summary>Centavo não muda decisão de diretoria, e engorda o JSON.</summary>
    private static decimal? Arredondar(decimal? valor) =>
        valor is null ? null : decimal.Round(valor.Value, 2);

    /// <summary>O agrupamento como o banco o devolve, antes de ganhar nome.</summary>
    private sealed record Bruto(
        int Chave, int Quantidade, int Maquinas, int ComOsDoisPrecos, decimal? DiferencaMedia);
}
