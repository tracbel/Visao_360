using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CriterioDasVendasDoArt;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

// =============================================================================================================
// OS LEITORES DO RECORTE (plano 2 do documento 54) — o que passa pelo filtro de filial, lido na hora: cada um uma
// natureza do dado, com as mesmas leituras que a apuração fazia. A conta fica na acumulação, em memória.
//
// CADA CONSULTA É A DE ANTES, com o tipo anônimo de antes: o registro nasce depois de materializar, para o SQL que
// vai ao banco ser exatamente o mesmo — o número igual começa na consulta igual.
// =============================================================================================================

/// <summary>Um cliente ao alcance, com o município do endereço principal (o de menor Id vem primeiro).</summary>
internal sealed record ClienteNoRecorte(long Id, int EmpresaId, ClasseDeCliente? Classe, int? MunicipioId);

/// <summary>Os clientes ao alcance, os inativados e, com o filtro do CEN, os clientes das carteiras dele.</summary>
internal sealed record ClientesDoRecorte(
    IReadOnlyList<ClienteNoRecorte> Clientes, IReadOnlyDictionary<long, int> Inativados, IReadOnlySet<long>? DoResponsavel);

/// <summary>A cadência da linha de negócio de uma carteira, em dias, por classe de cliente.</summary>
internal sealed record CadenciaDaCarteira(short? DiasCicloClasseA, short? DiasCicloClasseB, short? DiasCicloClasseC, short? DiasCicloClasseD);

/// <summary>Uma carteira comercial ao alcance.</summary>
internal sealed record CarteiraNoRecorte(long Id, long ResponsavelId, CadenciaDaCarteira? Cadencia);

/// <summary>Um vínculo vigente de cliente em carteira comercial.</summary>
internal sealed record VinculoNoRecorte(long CarteiraId, long ClienteId, DateTime? UltimaInteracaoEm);

/// <summary>O dono de uma carteira, pelo cadastro de usuário.</summary>
internal sealed record ResponsavelNoRecorte(string NomeExibicao, NaturezaDoUsuario Natureza);

/// <summary>As carteiras comerciais, os vínculos e os responsáveis ao alcance.</summary>
internal sealed record CoberturaDoRecorte(
    IReadOnlyList<CarteiraNoRecorte> Carteiras, IReadOnlyList<VinculoNoRecorte> Vinculos, IReadOnlyDictionary<long, ResponsavelNoRecorte> Responsaveis);

/// <summary>Uma linha mensal do faturamento de um cliente.</summary>
internal sealed record LinhaDeFaturamentoNoRecorte(
    long ClienteId, DateOnly Competencia, decimal ValorLiquido, decimal ValorEmMaquina, decimal ValorEmPeca, decimal ValorEmServico, decimal ValorEmOutros);

/// <summary>Uma nota para quem não tem cadastro de cliente no CRM.</summary>
internal sealed record NotaSemClienteNoRecorte(
    string Documento, NaturezaDoParceiro Natureza, decimal ValorLiquido, decimal ValorEmMaquina, decimal ValorEmPeca, decimal ValorEmServico, decimal ValorEmOutros);

/// <summary>O faturamento do recorte: com cliente nas duas janelas, desde quando há carga, e as notas sem cliente.</summary>
internal sealed record FaturamentoDoRecorte(
    IReadOnlyList<LinhaDeFaturamentoNoRecorte> ComCliente, DateOnly? PrimeiraCompetencia, IReadOnlyList<NotaSemClienteNoRecorte> SemCliente);

/// <summary>Uma venda de máquina do ART, com as três datas.</summary>
internal sealed record VendaDoArtNoRecorte(long CompradorId, int? LinhaDeProdutoId, DateOnly? VendidaEm, DateOnly? FaturadaEm, DateOnly? EntregueEm);

/// <summary>As vendas do ART ao alcance, com o de-para de categoria e as marcas da carga.</summary>
internal sealed record VendasDoArtNoRecorte(
    IReadOnlyDictionary<int, string> CodigoDaLinha,
    IReadOnlyDictionary<string, (string Codigo, string Nome, short Ordem)> CategoriaDaLinha,
    int VendasSemAData,
    DateOnly? VendaMaisRecente,
    DateOnly? PrimeiroMes,
    DateTime? CarregadoAte,
    IReadOnlyList<VendaDoArtNoRecorte> Vendas);

/// <summary>OS CLIENTES DO RECORTE — com o endereço principal, os inativados e o filtro "CEN / gestor".</summary>
/// <param name="contexto">O contexto do banco.</param>
internal sealed class LeitorDosClientesDoRecorte(CrmDbContext contexto)
{
    /// <summary>Lê os clientes.</summary>
    /// <param name="responsavelId">O CEN do filtro; nulo é todos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<ClientesDoRecorte> LerAsync(long? responsavelId, CancellationToken ct)
    {
        var clientes = await (
                from cliente in contexto.Clientes.AsNoTracking().Where(c => c.ExcluidoEm == null)
                join endereco in contexto.Enderecos.AsNoTracking().Where(e => e.EhPrincipal && e.ExcluidoEm == null)
                    on cliente.Id equals endereco.ClienteId into enderecos
                from endereco in enderecos.DefaultIfEmpty()
                orderby cliente.Id, endereco.Id
                select new { cliente.Id, cliente.EmpresaId, cliente.Classe, endereco.MunicipioId })
            .ToListAsync(ct);

        // O CLIENTE INATIVADO AO ALCANCE não some: a venda e o vínculo dele vão para um grupo
        // próprio. Sem isso, cairiam em "cliente de outra filial" — um rótulo errado para dinheiro
        // que é desta filial.
        var inativados = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm != null)
            .ToDictionaryAsync(c => c.Id, c => c.EmpresaId, ct);

        // O FILTRO "CEN / GESTOR": os clientes das carteiras comerciais do responsável escolhido. Quem não
        // está numa delas sai do recorte — do mapa e de fora dele —, como no filtro da filial do cliente.
        HashSet<long>? doResponsavel = null;
        if (responsavelId is { } responsavelEscolhido)
            doResponsavel = (await (
                    from vinculo in contexto.ClienteCarteiras.AsNoTracking().Where(v => v.DesvinculadoEm == null)
                    join carteira in contexto.Carteiras.AsNoTracking() on vinculo.CarteiraId equals carteira.Id
                    where carteira.ExcluidoEm == null
                          && carteira.Natureza == NaturezaDaCarteira.Comercial
                          && carteira.ResponsavelId == responsavelEscolhido
                    select vinculo.ClienteId)
                .ToListAsync(ct))
                .ToHashSet();

        return new ClientesDoRecorte(
            [.. clientes.Select(c => new ClienteNoRecorte(c.Id, c.EmpresaId, c.Classe, c.MunicipioId))], inativados, doResponsavel);
    }
}

/// <summary>A COBERTURA DO RECORTE — o vínculo em carteira comercial, contra a cadência da linha de negócio.</summary>
/// <param name="contexto">O contexto do banco.</param>
internal sealed class LeitorDaCoberturaDoRecorte(CrmDbContext contexto)
{
    /// <summary>Lê as carteiras, os vínculos e os responsáveis.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<CoberturaDoRecorte> LerAsync(CancellationToken ct)
    {
        var carteirasComerciais = await contexto.Carteiras.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Natureza == NaturezaDaCarteira.Comercial)
            .Select(c => new
            {
                c.Id,
                c.ResponsavelId,
                Cadencia = contexto.LinhasDeNegocio
                    .Where(l => l.Id == c.LinhaDeNegocioId)
                    .Select(l => new { l.DiasCicloClasseA, l.DiasCicloClasseB, l.DiasCicloClasseC, l.DiasCicloClasseD })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var idsDeCarteira = carteirasComerciais.Select(c => c.Id).ToList();

        var vinculos = await contexto.ClienteCarteiras.AsNoTracking()
            .Where(v => v.DesvinculadoEm == null && idsDeCarteira.Contains(v.CarteiraId))
            .Select(v => new { v.CarteiraId, v.ClienteId, v.UltimaInteracaoEm })
            .ToListAsync(ct);

        // OS RESPONSÁVEIS REAIS SÃO OS DAS CARTEIRAS, e não nomes digitados na tela: o nome sai do cadastro
        // de usuário, pelo mesmo filtro de alcance de todo o resto.
        var idsDeResponsavel = carteirasComerciais.Select(c => c.ResponsavelId).Distinct().ToList();
        var responsaveis = await contexto.Usuarios.AsNoTracking()
            .Where(u => idsDeResponsavel.Contains(u.Id))
            .Select(u => new { u.Id, u.NomeExibicao, u.Natureza })
            .ToDictionaryAsync(u => u.Id, ct);

        return new CoberturaDoRecorte(
            [
                .. carteirasComerciais.Select(c => new CarteiraNoRecorte(
                    c.Id,
                    c.ResponsavelId,
                    c.Cadencia is null
                        ? null
                        : new CadenciaDaCarteira(c.Cadencia.DiasCicloClasseA, c.Cadencia.DiasCicloClasseB, c.Cadencia.DiasCicloClasseC, c.Cadencia.DiasCicloClasseD)))
            ],
            [.. vinculos.Select(v => new VinculoNoRecorte(v.CarteiraId, v.ClienteId, v.UltimaInteracaoEm))],
            responsaveis.ToDictionary(r => r.Key, r => new ResponsavelNoRecorte(r.Value.NomeExibicao, r.Value.Natureza)));
    }
}

/// <summary>O FATURAMENTO DO RECORTE — o Protheus, em reais, com e sem cliente no CRM.</summary>
/// <param name="contexto">O contexto do banco.</param>
internal sealed class LeitorDoFaturamentoDoRecorte(CrmDbContext contexto)
{
    /// <summary>Lê o faturamento.</summary>
    /// <param name="consulta">O período e os filtros.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<FaturamentoDoRecorte> LerAsync(ConsultaDeIndicadoresTerritoriais consulta, CancellationToken ct)
    {
        var janelaAnterior = consulta.JanelaAnterior;

        // AS DUAS JANELAS NUMA LEITURA SÓ: a pedida e o mesmo trecho do ano anterior. Os meses entre as duas —
        // setembro e outubro, no ano fiscal até agosto — não são lidos.
        var faturamento = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null
                        && ((f.Competencia >= consulta.CompetenciaInicial && f.Competencia <= consulta.CompetenciaFinal)
                            || (f.Competencia >= janelaAnterior.Inicial && f.Competencia <= janelaAnterior.Final))
                        && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .Select(f => new { f.ClienteId, f.Competencia, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros })
            .ToListAsync(ct);

        // DESDE QUANDO HÁ FATURAMENTO AO ALCANCE: é o que diz se o ano anterior pode ser comparado. Com e sem
        // cliente, porque a carga grava os dois juntos.
        var primeiraComCliente = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .MinAsync(f => (DateOnly?)f.Competencia, ct);
        var primeiraSemCliente = await contexto.FaturamentoSemClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
            .MinAsync(f => (DateOnly?)f.Competencia, ct);
        DateOnly? primeiraCompetencia =
            primeiraComCliente is null ? primeiraSemCliente
            : primeiraSemCliente is null ? primeiraComCliente
            : primeiraComCliente < primeiraSemCliente ? primeiraComCliente : primeiraSemCliente;

        // A NOTA SEM CLIENTE NO CRM TAMBÉM É NOTA DESTA FILIAL. Sem ela, o total da tela seria só o
        // faturamento com cliente — R$ 320,6 mi a menos em 12 meses, medido em 13/09/2026 — e a
        // conferência "mapa + fora do mapa = o que a filial faturou" nunca fecharia. Ela não tem
        // cadastro, logo não tem filial de cadastro: o filtro pela filial do cliente a exclui — e o do CEN
        // também, porque sem cadastro ela não está em carteira nenhuma.
        List<NotaSemClienteNoRecorte> semCliente = [];
        if (consulta.FilialDoClienteId is null && consulta.ResponsavelId is null)
            semCliente =
            [
                .. (await contexto.FaturamentoSemClientes.AsNoTracking()
                        .Where(f => f.ExcluidoEm == null
                                    && f.Competencia >= consulta.CompetenciaInicial
                                    && f.Competencia <= consulta.CompetenciaFinal
                                    && (consulta.FilialDaVendaId == null || f.EmpresaId == consulta.FilialDaVendaId))
                        .Select(f => new { f.Documento, f.Natureza, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros })
                        .ToListAsync(ct))
                    .Select(f => new NotaSemClienteNoRecorte(
                        f.Documento, f.Natureza, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros))
            ];

        return new FaturamentoDoRecorte(
            [
                .. faturamento.Select(f => new LinhaDeFaturamentoNoRecorte(
                    f.ClienteId, f.Competencia, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros))
            ],
            primeiraCompetencia,
            semCliente);
    }
}

/// <summary>
/// AS VENDAS DE MÁQUINA DO RECORTE, EM UNIDADES — o ART (issue 69; D-P08 decidida em 24/09/2026). Nulas quando o ART não
/// trouxe venda ao alcance: ausência de carga não é ausência de venda.
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
internal sealed class LeitorDasVendasDoArtNoRecorte(CrmDbContext contexto)
{
    /// <summary>Lê as vendas do ART.</summary>
    /// <param name="consulta">O período e os filtros.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<VendasDoArtNoRecorte?> LerAsync(ConsultaDeIndicadoresTerritoriais consulta, CancellationToken ct)
    {
        var vendasDeMaquina = contexto.VendasDeMaquina.AsNoTracking()
            .Where(v => v.ExcluidoEm == null
                        && (consulta.FilialDaVendaId == null || v.EmpresaId == consulta.FilialDaVendaId));

        var oArtTrouxeVenda = await contexto.VendasDeMaquina.AsNoTracking().AnyAsync(v => v.ExcluidoEm == null, ct);
        if (!oArtTrouxeVenda) return null;

        // O DE-PARA É CRUZADO EM MEMÓRIA, DE PROPÓSITO. `CodigoDaLinha` é texto ASCII com colação
        // binária e `LinhaDeProduto.Codigo` é Unicode com a colação do banco: um JOIN entre os dois
        // no SQL Server dá conflito de colação. As duas tabelas têm dezenas de linhas.
        var codigoDaLinha = await contexto.LinhasDeProduto.AsNoTracking()
            .ToDictionaryAsync(l => l.Id, l => l.Codigo, ct);

        var categoriaDaLinha = new Dictionary<string, (string Codigo, string Nome, short Ordem)>(StringComparer.Ordinal);
        foreach (var ligacao in await (
                     from ligacao in contexto.LinhasDeProdutoNasCategorias.AsNoTracking()
                     join categoria in contexto.CategoriasDeMaquina.AsNoTracking()
                         on ligacao.CategoriaDeMaquinaId equals categoria.Id
                     select new { ligacao.CodigoDaLinha, categoria.Codigo, categoria.Nome, categoria.Ordem })
                 .ToListAsync(ct))
            categoriaDaLinha[ligacao.CodigoDaLinha] = (ligacao.Codigo, ligacao.Nome, ligacao.Ordem);

        var datas = DataDoCriterio(vendasDeMaquina, Criterio);
        var vendasSemAData = await datas.CountAsync(d => d == null, ct);
        var vendaMaisRecente = await datas.MaxAsync(ct);
        var carregadoAte = await vendasDeMaquina.MaxAsync(v => (DateTime?)v.ImportadaEm, ct);

        // DESDE QUANDO O ART TRAZ VENDA, pelo mês da data do critério — é o que diz se o ano anterior
        // pode ser comparado em unidades. O ART começou depois do Protheus, e cada fonte tem a sua.
        DateOnly? primeiroMes = null;
        if (await datas.MinAsync(ct) is { } primeiraVenda)
            primeiroMes = new DateOnly(primeiraVenda.Year, primeiraVenda.Month, 1);

        // AS DUAS JANELAS NUMA LEITURA SÓ, e a data de cada venda vem junto: a janela de cada uma — a
        // pedida ou a do ano anterior — é decidida na acumulação, pela data do critério.
        var vendas = await (
                from venda in NoPeriodo(
                    vendasDeMaquina, Criterio, consulta.JanelaAnterior.Inicial, consulta.CompetenciaFinal.AddMonths(1))
                // A MÁQUINA ENTRA POR FORA (junção à esquerda): ela tem filial própria, e a de uma
                // máquina reaproveitada de outra filial pode estar fora do alcance de quem lê. Numa
                // junção comum a venda sumiria inteira; assim ela conta, e só a categoria fica sem saber.
                join maquina in contexto.Equipamentos.AsNoTracking().Where(e => e.ExcluidoEm == null)
                    on venda.EquipamentoId equals maquina.Id into maquinas
                from maquina in maquinas.DefaultIfEmpty()
                select new
                {
                    venda.CompradorId,
                    LinhaDeProdutoId = maquina == null ? null : maquina.LinhaDeProdutoId,
                    venda.VendidaEm,
                    venda.FaturadaEm,
                    venda.EntregueEm
                })
            .ToListAsync(ct);

        return new VendasDoArtNoRecorte(
            codigoDaLinha,
            categoriaDaLinha,
            vendasSemAData,
            vendaMaisRecente,
            primeiroMes,
            carregadoAte,
            [.. vendas.Select(v => new VendaDoArtNoRecorte(v.CompradorId, v.LinhaDeProdutoId, v.VendidaEm, v.FaturadaEm, v.EntregueEm))]);
    }
}

/// <summary>
/// AS MÁQUINAS CONECTADAS DE CADA MUNICÍPIO (telemetria do Operations Center, 28/09/2026) — pelo município da última
/// posição, e pelo filtro de filial do equipamento: quem consulta conta as máquinas ao seu alcance.
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
internal sealed class LeitorDoParqueConectado(CrmDbContext contexto)
{
    /// <summary>Lê o parque conectado.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyDictionary<int, ParqueConectadoNoMunicipio>> LerAsync(CancellationToken ct)
    {
        var linhas = await contexto.Equipamentos.AsNoTracking()
            .Where(e => e.ExcluidoEm == null && e.MunicipioDaPosicaoId != null && e.PosicaoEm != null)
            .Join(contexto.Municipios.AsNoTracking().Where(m => m.CodigoIbge != null),
                e => e.MunicipioDaPosicaoId, m => (int?)m.Id,
                (e, m) => new { Codigo = m.CodigoIbge!.Value, e.HorimetroAtual, e.HorimetroAtualizadoEm, PosicaoEm = e.PosicaoEm!.Value })
            .ToListAsync(ct);

        return ParqueConectadoNoMunicipio.PorMunicipio(
            [.. linhas.Select(l => new MaquinaConectada(l.Codigo, l.HorimetroAtual, l.HorimetroAtualizadoEm, l.PosicaoEm))]);
    }
}
