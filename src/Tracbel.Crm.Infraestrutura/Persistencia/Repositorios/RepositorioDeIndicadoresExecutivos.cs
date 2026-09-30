using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// OS CINCO CARTÕES DA VISÃO 360, apurados para a filial do contexto de acesso (documento 36).
///
/// <para><b>Como em todo repositório, não há <c>Where</c> de empresa aqui.</b> Faturamento, meta,
/// carteira, cliente e venda perdida entram pelo filtro global. O vínculo cliente × carteira não tem
/// coluna de empresa: ele é lido pelas carteiras da filial (cobertura) ou pelos clientes cadastrados
/// nela (clientes únicos) — e é essa escolha que torna cada número somável entre filiais.</para>
///
/// <para><b>A cobertura repete a regra do mapa</b> (<see cref="RepositorioDeIndicadoresTerritoriais"/>):
/// vínculo em carteira comercial, cadência da linha de negócio pela classe do cliente, classe ausente
/// conta como D. Se as duas divergirem, o cartão e o mapa deixam de conversar — a regra está escrita
/// nos dois lugares, e o teste de API de cada rota a fixa.</para>
///
/// <para><b>Somas em memória</b>, pela mesma razão do repositório territorial: o SQLite dos testes de
/// API não agrega decimal. O volume é o de um ano de linhas cliente × mês de uma filial.</para>
/// </summary>
public sealed class RepositorioDeIndicadoresExecutivos(CrmDbContext contexto) : IRepositorioIndicadoresExecutivos
{
    /// <summary>As naturezas que são contraparte sem cadastro — a mesma leitura do mapa de vendas.</summary>
    private static readonly NaturezaDoParceiro[] ContraparteSemCadastro =
        [NaturezaDoParceiro.ClienteNaoCadastrado, NaturezaDoParceiro.SemDocumento, NaturezaDoParceiro.Indefinida];

    private sealed record LinhaDeFaturamento(
        DateOnly Competencia,
        NaturezaDoParceiro? Natureza,
        decimal ValorLiquido,
        decimal Maquina,
        decimal Peca,
        decimal Servico,
        decimal Outros,
        int Notas,
        DateTime CarregadoEm);

    /// <inheritdoc />
    public async Task<IndicadoresExecutivosDaFilial> ApurarAsync(
        int ano, CalendarioDoAno calendario, Dominio.Comum.JanelaDeCompetencia meses, DateTime agoraUtc, CancellationToken ct)
    {
        // O MÊS DE SÃO PAULO, e não o do UTC: às 22h do último dia do mês o UTC já virou o mês.
        var mesCorrente = Dominio.Comum.AnoFiscal.MesCorrenteEmSaoPaulo(agoraUtc);

        // OS MESES DO ANO VÊM PRONTOS, no calendário pedido — o fiscal (novembro a outubro) é o padrão desde
        // 27/09/2026 —, e o ano que ainda corre vem até o último mês fechado. Quem os calcula é a aplicação,
        // uma vez: novembro e dezembro são os meses em que o ano fiscal vai à frente do civil, e uma segunda
        // conta aqui é como as duas passariam a discordar.
        var primeiroMesDoAno = meses.Inicial;
        var ultimoMesDoAno = meses.Final;

        // -----------------------------------------------------------------------------------------
        // Faturamento: a competência mais recente carregada, e o ano pedido.
        //
        // A COMPETÊNCIA DO CARTÃO É A MAIS RECENTE QUE EXISTE até o mês corrente, com ou sem cliente —
        // e não "hoje". Se a carga parar, o cartão mostra o último mês carregado e diz qual é, em vez
        // de um mês corrente zerado.
        // -----------------------------------------------------------------------------------------
        var ultimaComCliente = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && f.Competencia <= mesCorrente)
            .MaxAsync(f => (DateOnly?)f.Competencia, ct);

        var ultimaSemCliente = await contexto.FaturamentoSemClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && f.Competencia <= mesCorrente)
            .MaxAsync(f => (DateOnly?)f.Competencia, ct);

        DateOnly? competenciaDoMes = ultimaComCliente is null ? ultimaSemCliente
            : ultimaSemCliente is null ? ultimaComCliente
            : ultimaComCliente > ultimaSemCliente ? ultimaComCliente : ultimaSemCliente;

        var mesDoCartao = competenciaDoMes ?? DateOnly.MinValue;

        var comCliente = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null
                        && ((f.Competencia >= primeiroMesDoAno && f.Competencia <= ultimoMesDoAno) || f.Competencia == mesDoCartao))
            .Select(f => new LinhaDeFaturamento(
                f.Competencia, null, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros,
                f.Notas, f.AlteradoEm ?? f.CriadoEm))
            .ToListAsync(ct);

        var semCliente = await contexto.FaturamentoSemClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null
                        && ((f.Competencia >= primeiroMesDoAno && f.Competencia <= ultimoMesDoAno) || f.Competencia == mesDoCartao))
            .Select(f => new LinhaDeFaturamento(
                f.Competencia, f.Natureza, f.ValorLiquido, f.ValorEmMaquina, f.ValorEmPeca, f.ValorEmServico, f.ValorEmOutros,
                f.Notas, f.AlteradoEm ?? f.CriadoEm))
            .ToListAsync(ct);

        var linhas = comCliente.Concat(semCliente).ToList();

        FaturamentoDaCompetencia? doMes = null;
        if (competenciaDoMes is { } competencia)
        {
            var doCartao = linhas.Where(l => l.Competencia == competencia).ToList();

            decimal DaNatureza(Func<NaturezaDoParceiro, bool> filtro) =>
                doCartao.Where(l => l.Natureza is { } natureza && filtro(natureza)).Sum(l => l.ValorLiquido);

            doMes = new FaturamentoDaCompetencia(
                competencia,
                doCartao.Where(l => l.Natureza is null).Sum(l => l.ValorLiquido),
                DaNatureza(n => ContraparteSemCadastro.Contains(n)),
                DaNatureza(n => n == NaturezaDoParceiro.Fabrica),
                DaNatureza(n => n == NaturezaDoParceiro.EmpresaDoGrupo),
                DaNatureza(n => n == NaturezaDoParceiro.OutraRevenda),
                doCartao.Sum(l => l.Maquina),
                doCartao.Sum(l => l.Peca),
                doCartao.Sum(l => l.Servico),
                doCartao.Sum(l => l.Outros),
                doCartao.Sum(l => l.Notas),
                doCartao.Count == 0 ? null : doCartao.Max(l => l.CarregadoEm));
        }

        var doAno = linhas.Where(l => l.Competencia >= primeiroMesDoAno && l.Competencia <= ultimoMesDoAno).ToList();
        var mesesDoAno = doAno.Select(l => l.Competencia).Distinct().Order().ToList();

        // -----------------------------------------------------------------------------------------
        // SÓ O FATURAMENTO. A meta decidida em 27/09/2026 é a de VENDA, em unidades, da API Gestão de
        // Negócios, e é lida pela rota própria (RepositorioDeMetas); a de faturamento, que nunca teve fonte,
        // saiu deste cartão (#138).
        // -----------------------------------------------------------------------------------------
        var anoApurado = new FaturamentoDoAno(
            ano,
            mesesDoAno.Count == 0 ? null : mesesDoAno[0],
            mesesDoAno.Count == 0 ? null : mesesDoAno[^1],
            mesesDoAno.Count,
            doAno.Where(l => l.Natureza is null).Sum(l => l.ValorLiquido),
            doAno.Where(l => l.Natureza is not null).Sum(l => l.ValorLiquido),
            calendario.ToString(),
            primeiroMesDoAno,
            ultimoMesDoAno);

        // -----------------------------------------------------------------------------------------
        // Carteira e cobertura: os vínculos das carteiras desta filial.
        // -----------------------------------------------------------------------------------------
        var carteiras = await contexto.Carteiras.AsNoTracking()
            .Where(k => k.ExcluidoEm == null)
            .Select(k => new
            {
                k.Id,
                k.Natureza,
                Cadencia = contexto.LinhasDeNegocio
                    .Where(l => l.Id == k.LinhaDeNegocioId)
                    .Select(l => new { l.DiasCicloClasseA, l.DiasCicloClasseB, l.DiasCicloClasseC, l.DiasCicloClasseD })
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(k => k.Id, ct);

        var idsDasCarteiras = carteiras.Keys.ToList();

        var vinculos = await contexto.ClienteCarteiras.AsNoTracking()
            .Where(v => v.DesvinculadoEm == null && idsDasCarteiras.Contains(v.CarteiraId))
            .Select(v => new { v.CarteiraId, v.ClienteId, v.UltimaInteracaoEm })
            .ToListAsync(ct);

        // A CLASSE É A DO CADASTRO DO CLIENTE (curva ABC apurada do faturamento), e não a do vínculo:
        // no legado, 47 mil dos 49 mil vínculos vieram com a classe C por padrão (documento 36, §D).
        var classeDoCliente = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm == null)
            .Select(c => new { c.Id, c.Classe })
            .ToDictionaryAsync(c => c.Id, c => c.Classe, ct);

        int vinculosComerciais = 0, elegiveis = 0, cobertos = 0, foraDaCadencia = 0, nuncaContatados = 0, semCadencia = 0;
        DateTime? contatoMaisRecente = null;
        var clientesNasCarteiras = new HashSet<long>();

        foreach (var vinculo in vinculos)
        {
            var carteira = carteiras[vinculo.CarteiraId];
            if (carteira.Natureza != NaturezaDaCarteira.Comercial) continue;

            vinculosComerciais++;
            clientesNasCarteiras.Add(vinculo.ClienteId);

            if (vinculo.UltimaInteracaoEm is { } ultima && (contatoMaisRecente is null || ultima > contatoMaisRecente))
                contatoMaisRecente = ultima;

            // SEM CLASSE APURADA — ou com o cliente cadastrado em outra filial, fora do alcance —, o
            // vínculo conta como D. É a regra do painel do CEN e do mapa.
            short? dias = (classeDoCliente.GetValueOrDefault(vinculo.ClienteId) ?? ClasseDeCliente.D) switch
            {
                ClasseDeCliente.A => carteira.Cadencia?.DiasCicloClasseA,
                ClasseDeCliente.B => carteira.Cadencia?.DiasCicloClasseB,
                ClasseDeCliente.C => carteira.Cadencia?.DiasCicloClasseC,
                _ => carteira.Cadencia?.DiasCicloClasseD
            };

            if (dias is null) semCadencia++;
            else if (vinculo.UltimaInteracaoEm is null) { elegiveis++; nuncaContatados++; }
            else if (vinculo.UltimaInteracaoEm >= agoraUtc.AddDays(-dias.Value)) { elegiveis++; cobertos++; }
            else { elegiveis++; foraDaCadencia++; }
        }

        // OS CLIENTES ÚNICOS SÃO A PARTIÇÃO PELA FILIAL DE CADASTRO: cliente desta filial com vínculo
        // ativo em qualquer carteira, desta ou de outra filial. Somadas as filiais, cada cliente aparece
        // uma vez só. O documento fica em memória porque é objeto de valor.
        var cadastrados = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm == null
                        && contexto.ClienteCarteiras.Any(v => v.ClienteId == c.Id && v.DesvinculadoEm == null))
            .Select(c => new { c.Situacao, c.Documento })
            .ToListAsync(ct);

        var carteiraDaFilial = new CarteiraDaFilial(
            cadastrados.Count,
            cadastrados.Count(c => c.Situacao == SituacaoDoCliente.Cliente),
            cadastrados.Count(c => c.Situacao == SituacaoDoCliente.Prospect),
            cadastrados.Count(c => c.Situacao == SituacaoDoCliente.Suspect),
            cadastrados.Count(c => c.Situacao is SituacaoDoCliente.ClienteInativo or SituacaoDoCliente.Encerrado),
            cadastrados.Count(c => c.Documento is null),
            clientesNasCarteiras.Count,
            vinculos.Count,
            vinculosComerciais,
            carteiras.Count,
            carteiras.Values.Count(k => k.Natureza == NaturezaDaCarteira.Comercial));

        var cobertura = new CoberturaDaFilial(
            vinculosComerciais,
            elegiveis,
            cobertos,
            foraDaCadencia,
            nuncaContatados,
            semCadencia,
            contatoMaisRecente,
            await contexto.TiposDeTarefa.AsNoTracking().CountAsync(ct),
            await contexto.TiposDeTarefa.AsNoTracking().CountAsync(t => t.ContaParaCobertura, ct));

        // -----------------------------------------------------------------------------------------
        // Mercado: as vendas perdidas registradas no formulário.
        // -----------------------------------------------------------------------------------------
        // SÓ A PRINCIPAL CONTA (decisão de 27/09/2026): a duplicata e o complemento repetem a mesma perda.
        var perdas = await contexto.VendasPerdidas.AsNoTracking()
            .Where(v => v.ExcluidoEm == null && v.Papel == PapelDaVendaPerdida.Principal)
            .Select(v => new
            {
                v.ConcorrenteId,
                v.ModeloDoConcorrente,
                v.PrecoDoConcorrente,
                v.PrecoOfertado,
                v.Quantidade,
                v.OcorridaEm,
                v.RegistradaEm
            })
            .ToListAsync(ct);

        var datasDasPerdas = perdas.Select(p => p.OcorridaEm ?? DateOnly.FromDateTime(p.RegistradaEm)).ToList();

        var mercado = new MercadoDaFilial(
            perdas.Count,
            perdas.Count(p => p.ConcorrenteId != null),
            perdas.Count(p => !string.IsNullOrWhiteSpace(p.ModeloDoConcorrente)),
            perdas.Count(p => p.PrecoDoConcorrente != null && p.PrecoOfertado != null),
            perdas.Sum(p => p.Quantidade),
            datasDasPerdas.Count == 0 ? null : datasDasPerdas.Min(),
            datasDasPerdas.Count == 0 ? null : datasDasPerdas.Max());

        // -----------------------------------------------------------------------------------------
        // O faturamento pelo ART (29/09/2026): as máquinas entregues, nos mesmos meses do ano.
        // -----------------------------------------------------------------------------------------
        var (entreguesNoAno, entreguesNoAnterior, entreguesNoMes, entreguesPorMes) =
            await EntreguesNoArtAsync(primeiroMesDoAno, ultimoMesDoAno, mesCorrente, ct);

        return new IndicadoresExecutivosDaFilial(
            agoraUtc, doMes, anoApurado, carteiraDaFilial, cobertura, mercado, entreguesNoAno, entreguesNoAnterior, entreguesNoMes,
            entreguesPorMes);
    }

    /// <summary>Quantos meses a série do "Faturamento — 12 meses" leva, terminando no mês em curso ou no fim do ano fechado.</summary>
    private const int MesesDaSerie = 12;

    /// <summary>
    /// AS MÁQUINAS ENTREGUES NO ART (decisão do Ricardo de 29/09/2026: "o faturamento real do ano fiscal vem do ART, das
    /// máquinas entregues"), no ano, no mesmo trecho do ano anterior e no mês em curso.
    ///
    /// <para><b>Lê o retrato de cada registro do ART</b> (<c>integracao.RegistroDeOrigem</c>), e não só
    /// <c>frota.VendaDeMaquina</c>: a venda cujo comprador ainda não está no CRM só existe ali, e sem ela o ano fica ~16%
    /// abaixo do que a Gestão de Negócios mostra (1.109 × 1.322 no FY26, documento 36, §3.2). O registro que sumiu da
    /// origem não conta.</para>
    ///
    /// <para><b>A filial é a da unidade que vendeu</b>, pela correspondência que a carga do ART usa para a venda — o
    /// retrato não tem coluna de empresa, e por isso o filtro global não o alcança: a fronteira é refeita aqui, com o
    /// mesmo predicado do filtro. A unidade sem filial não é de filial nenhuma, e fica fora.</para>
    ///
    /// <para><b>A série mês a mês</b> (29/09/2026) sai da mesma leitura: os doze meses que terminam no mês em curso — ou,
    /// num ano fiscal fechado, em outubro dele (30/09/2026, #313) —, com zero no mês sem entrega.</para>
    /// </summary>
    private async Task<(MaquinasEntreguesNoArt Ano, MaquinasEntreguesNoArt Anterior, MaquinasEntreguesNoArt MesEmCurso,
        IReadOnlyList<MaquinasEntreguesNoArt> PorMes)> EntreguesNoArtAsync(
        DateOnly inicio, DateOnly fim, DateOnly mesCorrente, CancellationToken ct)
    {
        var inicioDoAnterior = inicio.AddMonths(-12);
        var fimDoAnterior = fim.AddMonths(-12);
        // A SÉRIE SEGUE O ANO ESCOLHIDO (30/09/2026, #313): no ano que corre — o último mês fechado é o de ontem —, ela vai
        // até o mês em curso; num ano fiscal fechado, termina em outubro dele.
        var fimDaSerie = fim.AddMonths(1) >= mesCorrente ? mesCorrente : fim;
        var inicioDaSerie = fimDaSerie.AddMonths(-(MesesDaSerie - 1));
        var mesesDaSerie = Enumerable.Range(0, MesesDaSerie).Select(i => inicioDaSerie.AddMonths(i)).ToList();

        static MaquinasEntreguesNoArt Nenhuma(DateOnly de, DateOnly a) => new(de, a, 0, 0m, 0, 0);

        var art = await contexto.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == ConexoesDoSistema.Art).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);
        if (art is not { } sistemaDoArt)
            return (Nenhuma(inicio, fim), Nenhuma(inicioDoAnterior, fimDoAnterior), Nenhuma(mesCorrente, mesCorrente),
                mesesDaSerie.Select(m => Nenhuma(m, m)).ToList());

        // UMA LEITURA SÓ, da janela que cobre as quatro contas: o ano, o mesmo trecho do anterior, o mês em curso e os doze
        // meses da série — que, com um ano passado escolhido, ficam fora da janela do ano.
        var desde = inicioDoAnterior < inicioDaSerie ? inicioDoAnterior : inicioDaSerie;
        var ateDoAno = fim.AddMonths(1);
        var ateDaSerie = fimDaSerie.AddMonths(1);
        var fimDoMes = mesCorrente.AddMonths(1);

        var registros = await contexto.RegistrosDeOrigem.AsNoTracking()
            .Where(r => r.SistemaId == sistemaDoArt && r.Fluxo == MetaDeVenda.FluxoDasVendasDoArt && r.AusenteNaOrigemDesde == null
                        && r.EntregueEm != null
                        && ((r.EntregueEm >= desde && r.EntregueEm < ateDoAno)
                            || (r.EntregueEm >= inicioDaSerie && r.EntregueEm < ateDaSerie)
                            || (r.EntregueEm >= mesCorrente && r.EntregueEm < fimDoMes)))
            .Select(r => new { EntregueEm = r.EntregueEm!.Value, r.UnidadeNaOrigem, r.ValorDaVenda, r.VendaDeMaquinaId })
            .ToListAsync(ct);

        var filialDaUnidade = await contexto.CorrespondenciasDaOrigem.AsNoTracking()
            .Where(c => c.SistemaId == sistemaDoArt && c.Tipo == TipoDeCorrespondencia.Unidade && c.EmpresaCorrespondenteId != null
                        && (c.Situacao == SituacaoDaCorrespondencia.CorrespondenciaExata || c.Situacao == SituacaoDaCorrespondencia.ConfirmadaPorRevisao))
            .Select(c => new { c.TextoNaOrigem, EmpresaId = c.EmpresaCorrespondenteId!.Value })
            .ToListAsync(ct);

        var porTexto = filialDaUnidade
            .GroupBy(c => c.TextoNaOrigem, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().EmpresaId, StringComparer.OrdinalIgnoreCase);

        // O MESMO PREDICADO DO FILTRO GLOBAL (CrmDbContext.AplicarFronteiraDeEmpresa): serviço de sistema, alcance aberto
        // com motivo, filial alcançada, filial de casa.
        var acesso = contexto.Acesso;
        bool Alcanca(int empresa) =>
            acesso.EhServicoDeSistema || contexto.EstaComAlcanceEntreEmpresas
            || acesso.EmpresasVisiveis.Contains(empresa) || acesso.EmpresaId == empresa;

        var daFilial = registros
            .Where(r => r.UnidadeNaOrigem is { } unidade && porTexto.TryGetValue(unidade, out var empresa) && Alcanca(empresa))
            .Select(r => (Mes: new DateOnly(r.EntregueEm.Year, r.EntregueEm.Month, 1), r.ValorDaVenda, NoCrm: r.VendaDeMaquinaId is not null))
            .ToList();

        MaquinasEntreguesNoArt Somar(DateOnly de, DateOnly a)
        {
            var naJanela = daFilial.Where(r => r.Mes >= de && r.Mes <= a).ToList();
            return new MaquinasEntreguesNoArt(
                de, a,
                naJanela.Count,
                naJanela.Sum(r => r.ValorDaVenda ?? 0m),
                naJanela.Count(r => r.ValorDaVenda is null),
                naJanela.Count(r => !r.NoCrm));
        }

        return (Somar(inicio, fim), Somar(inicioDoAnterior, fimDoAnterior), Somar(mesCorrente, mesCorrente),
            mesesDaSerie.Select(m => Somar(m, m)).ToList());
    }
}
