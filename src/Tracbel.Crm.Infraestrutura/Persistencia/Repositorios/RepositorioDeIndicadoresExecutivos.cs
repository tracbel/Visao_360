using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

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
    public async Task<IndicadoresExecutivosDaFilial> ApurarAsync(int ano, DateTime agoraUtc, CancellationToken ct)
    {
        var mesCorrente = new DateOnly(agoraUtc.Year, agoraUtc.Month, 1);
        var primeiroMesDoAno = new DateOnly(ano, 1, 1);
        var ultimoMesDoAno = new DateOnly(ano, 12, 1);

        // -----------------------------------------------------------------------------------------
        // Faturamento: a competência mais recente carregada, e o ano civil pedido.
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
        // META DE FATURAMENTO: NÃO HÁ FONTE NESTA FASE.
        //
        // A tabela `organizacao.Meta` saiu na fase 1 da reestruturação (documento 41): nunca teve uma
        // linha, e nenhuma tela a preenchia — a Visão 360 já mostrava "nenhuma meta cadastrada" para
        // todas as filiais. O cartão continua com o realizado e com a mesma lacuna declarada; a
        // contagem e o alvo voltam a ser lidos aqui quando a administração de metas existir.
        // -----------------------------------------------------------------------------------------
        var anoApurado = new FaturamentoDoAno(
            ano,
            mesesDoAno.Count == 0 ? null : mesesDoAno[0],
            mesesDoAno.Count == 0 ? null : mesesDoAno[^1],
            mesesDoAno.Count,
            doAno.Where(l => l.Natureza is null).Sum(l => l.ValorLiquido),
            doAno.Where(l => l.Natureza is not null).Sum(l => l.ValorLiquido),
            0,
            null,
            0,
            0);

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
        var perdas = await contexto.VendasPerdidas.AsNoTracking()
            .Where(v => v.ExcluidoEm == null)
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

        return new IndicadoresExecutivosDaFilial(agoraUtc, doMes, anoApurado, carteiraDaFilial, cobertura, mercado);
    }
}
