using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso à carteirização, sobre o <see cref="CrmDbContext"/> — o que sustenta a Cobertura.
///
/// <para><b>A data do último contato é lida, não calculada na hora.</b> É a única exceção
/// consciente à regra de não guardar dado derivado, e está registrada como tal na própria
/// entidade: a tela ordena centenas de clientes por essa data, e calculá-la a cada abertura
/// custaria varrer a tabela de interações. A carga a preenche a partir das interações que
/// efetivamente entraram, e o job diário a reconcilia.</para>
///
/// <para>[V] No sistema de origem a mesma coluna só é preenchida para desfechos marcados numa
/// tabela de configuração que quatro departamentos nunca povoaram — 31.556 clientes
/// carteirizados aparecem lá como "nunca contatados" por construção, e a procedure que a
/// atualizava parou em agosto de 2025. A nossa nasce do fato.</para>
/// </summary>
public sealed class RepositorioDeCarteiras(CrmDbContext contexto) : IRepositorioCarteiras
{
    /// <inheritdoc />
    public async Task<PaginaDe<LinhaDeCobertura>> ListarCoberturaAsync(
        ConsultaDeCobertura consulta, CancellationToken ct)
    {
        var linhas = Filtrar(consulta);

        var total = await linhas.CountAsync(ct);

        var itens = await Ordenar(linhas, consulta)
            .Skip(consulta.Paginacao.Saltar)
            .Take(consulta.Paginacao.Tamanho)
            .Select(v => new LinhaDeCobertura(
                contexto.Clientes.Where(c => c.Id == v.ClienteId)
                    .Select(c => c.ChavePublica).FirstOrDefault(),
                contexto.Clientes.Where(c => c.Id == v.ClienteId)
                    .Select(c => c.NomeRazao).FirstOrDefault()!,
                contexto.Carteiras.Where(k => k.Id == v.CarteiraId)
                    .Select(k => k.ChavePublica).FirstOrDefault(),
                contexto.Carteiras.Where(k => k.Id == v.CarteiraId)
                    .Select(k => k.Nome).FirstOrDefault()!,
                contexto.Carteiras.Where(k => k.Id == v.CarteiraId)
                    .SelectMany(k => contexto.LinhasDeNegocio
                        .Where(l => l.Id == k.LinhaDeNegocioId).Select(l => l.Nome))
                    .FirstOrDefault()!,
                v.Classe,
                v.UltimaInteracaoEm,
                v.DiasCicloContato,
                contexto.Carteiras.Where(k => k.Id == v.CarteiraId)
                    .SelectMany(k => contexto.Usuarios
                        .Where(u => u.Id == k.ResponsavelId).Select(u => u.NomeExibicao))
                    .FirstOrDefault()!))
            .ToListAsync(ct);

        return new PaginaDe<LinhaDeCobertura>(
            itens, consulta.Paginacao.Pagina, consulta.Paginacao.Tamanho, total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ResumoDeCobertura>> ResumirCoberturaAsync(
        DateTime agoraUtc, CancellationToken ct)
    {
        var trintaDias = agoraUtc.AddDays(-30);
        var noventaDias = agoraUtc.AddDays(-90);

        // O AGRUPAMENTO É DO BANCO. São 49 mil vínculos: contar "quantos tiveram contato nos
        // últimos 30 dias" na tela significaria trazer os 49 mil para contar cinco números por
        // carteira.
        var agrupado = await contexto.ClienteCarteiras
            .Where(v => v.DesvinculadoEm == null)
            .GroupBy(v => v.CarteiraId)
            .Select(g => new
            {
                CarteiraId = g.Key,
                Clientes = g.Count(),
                Em30 = g.Count(v => v.UltimaInteracaoEm >= trintaDias),
                Em90 = g.Count(v => v.UltimaInteracaoEm >= noventaDias),
                Nunca = g.Count(v => v.UltimaInteracaoEm == null),
                Ultimo = g.Max(v => v.UltimaInteracaoEm)
            })
            .ToListAsync(ct);

        var carteiras = await contexto.Carteiras.AsNoTracking()
            .Where(k => k.ExcluidoEm == null)
            .Select(k => new
            {
                k.Id,
                k.ChavePublica,
                k.Codigo,
                k.Nome,
                k.Natureza,
                LinhaDeNegocio = contexto.LinhasDeNegocio
                    .Where(l => l.Id == k.LinhaDeNegocioId).Select(l => l.Nome).FirstOrDefault(),
                Responsavel = contexto.Usuarios
                    .Where(u => u.Id == k.ResponsavelId).Select(u => u.NomeExibicao).FirstOrDefault(),
                // O QUE O DONO DA CARTEIRA É. Cinco das contas responsáveis são caixa de área —
                // INT.MERCADO responde por onze carteiras, uma delas com 5.000 clientes. Elas
                // continuam nos números; a tela é que passa a poder dizer que são área.
                NaturezaDoResponsavel = contexto.Usuarios
                    .Where(u => u.Id == k.ResponsavelId)
                    .Select(u => u.Natureza).FirstOrDefault()
            })
            .ToDictionaryAsync(k => k.Id, ct);

        return
        [
            .. agrupado
                .Where(a => carteiras.ContainsKey(a.CarteiraId))
                .Select(a => new ResumoDeCobertura(
                    carteiras[a.CarteiraId].ChavePublica,
                    carteiras[a.CarteiraId].Codigo,
                    carteiras[a.CarteiraId].Nome,
                    carteiras[a.CarteiraId].LinhaDeNegocio ?? string.Empty,
                    carteiras[a.CarteiraId].Responsavel ?? string.Empty,
                    a.Clientes,
                    a.Em30,
                    a.Em90,
                    a.Nunca,
                    a.Ultimo,
                    carteiras[a.CarteiraId].Natureza.ToString(),
                    carteiras[a.CarteiraId].NaturezaDoResponsavel.ToString()))
                .OrderByDescending(r => r.Clientes)
        ];
    }

    private IQueryable<ClienteCarteira> Filtrar(ConsultaDeCobertura consulta)
    {
        // A FRONTEIRA DE EMPRESA CHEGA AQUI PELA CARTEIRA, não por um Where escrito à mão: o
        // vínculo cliente × carteira não tem coluna de empresa (ele é a ponte entre duas
        // entidades que têm), e a junção com a carteira — que TEM o filtro global — é o que
        // fecha a fronteira. Sem esta junção, a listagem atravessaria filial.
        var linhas =
            from vinculo in contexto.ClienteCarteiras.Where(v => v.DesvinculadoEm == null)
            join carteira in contexto.Carteiras on vinculo.CarteiraId equals carteira.Id
            where carteira.ExcluidoEm == null
            select new { vinculo, carteira };

        if (consulta.CarteiraId is { } carteiraId)
            linhas = linhas.Where(x => x.vinculo.CarteiraId == carteiraId);

        if (consulta.ResponsavelId is { } responsavelId)
            linhas = linhas.Where(x => x.carteira.ResponsavelId == responsavelId);

        if (consulta.Classe is { } classe)
            linhas = linhas.Where(x => x.vinculo.Classe == classe);

        if (consulta.SomenteSemContato)
            linhas = linhas.Where(x => x.vinculo.UltimaInteracaoEm == null);

        if (consulta.DiasSemContato is { } dias)
        {
            var limite = DateTime.UtcNow.AddDays(-dias);
            linhas = linhas.Where(x =>
                x.vinculo.UltimaInteracaoEm == null || x.vinculo.UltimaInteracaoEm < limite);
        }

        return linhas.Select(x => x.vinculo);
    }

    private static IQueryable<ClienteCarteira> Ordenar(
        IQueryable<ClienteCarteira> linhas, ConsultaDeCobertura consulta) =>
        (consulta.Ordem, consulta.Descendente) switch
        {
            (OrdemDeCobertura.Classe, false) =>
                linhas.OrderBy(v => v.Classe).ThenBy(v => v.UltimaInteracaoEm).ThenBy(v => v.Id),
            (OrdemDeCobertura.Classe, true) =>
                linhas.OrderByDescending(v => v.Classe).ThenBy(v => v.UltimaInteracaoEm).ThenBy(v => v.Id),

            (OrdemDeCobertura.Nome, false) => linhas.OrderBy(v => v.ClienteId).ThenBy(v => v.Id),
            (OrdemDeCobertura.Nome, true) => linhas.OrderByDescending(v => v.ClienteId).ThenBy(v => v.Id),

            // O PADRÃO É "QUEM ESTÁ HÁ MAIS TEMPO SEM CONTATO PRIMEIRO", e o nunca-contatado vem
            // antes de todos: no SQL Server o nulo ordena primeiro na crescente, que é exatamente
            // onde ele deve aparecer numa tela de cobertura.
            (_, true) => linhas.OrderByDescending(v => v.UltimaInteracaoEm).ThenBy(v => v.Id),
            _ => linhas.OrderBy(v => v.UltimaInteracaoEm).ThenBy(v => v.Id)
        };
}
