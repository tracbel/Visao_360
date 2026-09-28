using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso à carteirização, sobre o <see cref="CrmDbContext"/> — o que sustenta a Cobertura.
///
/// <para><b>A data do último contato é lida, não calculada na hora.</b> É a única exceção
/// consciente à regra de não guardar dado derivado, e está registrada como tal na própria
/// entidade: a tela ordena centenas de clientes por essa data, e calculá-la a cada abertura
/// custaria varrer o histórico inteiro do Vórtice. Desde 27/09/2026 ela vem da regra da BI de
/// carteiras (<c>BI_CARTEIRA_VN</c> — 53 resultados que contam como contato, em qualquer canal e
/// em qualquer departamento), apurada todo dia pela rotina <c>CARTEIRAS_VORTICE</c> sobre o
/// histórico INTEIRO — e não mais só das interações que a carga já tinha trazido para este banco.
/// A data só anda para a frente.</para>
///
/// <para>[V] No sistema de origem a mesma coluna só é preenchida para desfechos marcados numa
/// tabela de configuração que quatro departamentos nunca povoaram — 31.556 clientes
/// carteirizados apareciam lá como "nunca contatados" por construção, e a procedure que a
/// atualizava parou em agosto de 2025. A nossa lê o histórico inteiro, pela regra da BI.</para>
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

    /// <inheritdoc />
    /// <remarks>
    /// A fronteira chega pela CARTEIRA, pela mesma razão de <see cref="Filtrar"/>: o vínculo não tem coluna de empresa.
    /// Medido em 27/09/2026, 4.455 dos 8.537 vínculos ativos estão numa carteira de filial diferente da filial do
    /// cadastro do cliente — quem não alcança a filial da carteira não a vê na ficha, e o cliente continua visível.
    /// </remarks>
    public async Task<CarteirasLidasDoCliente?> ListarDoClienteAsync(Guid chaveDoCliente, CancellationToken ct)
    {
        var cliente = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ChavePublica == chaveDoCliente)
            .Select(c => new { c.Id, c.Classe, c.ClasseApuradaEm })
            .FirstOrDefaultAsync(ct);

        if (cliente is null) return null;

        var lidos = await (
                from vinculo in contexto.ClienteCarteiras.AsNoTracking()
                join carteira in contexto.Carteiras.AsNoTracking() on vinculo.CarteiraId equals carteira.Id
                join filial in contexto.Empresas.AsNoTracking() on carteira.EmpresaId equals filial.Id
                join linha in contexto.LinhasDeNegocio.AsNoTracking() on carteira.LinhaDeNegocioId equals linha.Id
                where vinculo.ClienteId == cliente.Id && vinculo.DesvinculadoEm == null && carteira.ExcluidoEm == null
                select new
                {
                    carteira.ChavePublica,
                    carteira.Codigo,
                    carteira.Nome,
                    carteira.Natureza,
                    Linha = linha.Nome,
                    linha.DiasCicloClasseA,
                    linha.DiasCicloClasseB,
                    linha.DiasCicloClasseC,
                    linha.DiasCicloClasseD,
                    Responsavel = contexto.Usuarios
                        .Where(u => u.Id == carteira.ResponsavelId).Select(u => u.NomeExibicao).FirstOrDefault(),
                    // O QUE O DONO DA CARTEIRA É: pessoa ou caixa de área, como na Cobertura — a enumeração vem crua e
                    // vira texto em memória.
                    NaturezaDoResponsavel = contexto.Usuarios
                        .Where(u => u.Id == carteira.ResponsavelId).Select(u => (NaturezaDoUsuario?)u.Natureza).FirstOrDefault(),
                    FilialCodigo = filial.Codigo,
                    FilialNome = filial.Nome,
                    vinculo.VinculadoEm,
                    vinculo.UltimaInteracaoEm
                })
            .ToListAsync(ct);

        return new CarteirasLidasDoCliente(
            cliente.Classe,
            cliente.ClasseApuradaEm,
            [.. lidos
                .OrderBy(v => v.Natureza == NaturezaDaCarteira.Comercial ? 0 : 1)
                .ThenBy(v => v.FilialCodigo, StringComparer.Ordinal)
                .ThenBy(v => v.Codigo, StringComparer.Ordinal)
                .Select(v => new VinculoDoClienteComCarteira(
                    v.ChavePublica,
                    v.Codigo,
                    v.Nome,
                    v.Natureza.ToString(),
                    v.Linha,
                    v.DiasCicloClasseA,
                    v.DiasCicloClasseB,
                    v.DiasCicloClasseC,
                    v.DiasCicloClasseD,
                    v.Responsavel,
                    v.NaturezaDoResponsavel?.ToString(),
                    v.FilialCodigo,
                    v.FilialNome,
                    v.VinculadoEm,
                    v.UltimaInteracaoEm))]);
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

        // A CLASSE DO FILTRO É A DO CLIENTE (curva ABC apurada do faturamento), e não a do vínculo:
        // ClienteCarteira.Classe é a coluna do legado, e continua entrando como C por assunção na
        // maioria dos vínculos (IVS_Pes.Potencial é varchar(3) sem catálogo) — filtrar por ela
        // recriaria a mesma distorção que tirou a ordenação por classe desta tela.
        if (consulta.Classe is { } classe)
            linhas = linhas.Where(x =>
                contexto.Clientes.Any(c => c.Id == x.vinculo.ClienteId && c.Classe == classe));

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
