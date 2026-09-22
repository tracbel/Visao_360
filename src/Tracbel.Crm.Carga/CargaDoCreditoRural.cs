using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.BancoCentral;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A CARGA DO CRÉDITO RURAL DE INVESTIMENTO DO SICOR — São Paulo inteiro, todos os produtos, desde 2013
/// (issue 68).
///
/// <para><b>Primeiro as tabelas auxiliares</b> (programa, subprograma, fonte e produto), depois os anos.</para>
///
/// <para><b>Um ano por transação.</b> São ~15 mil linhas por ano; um ano que falhe desfaz só a si mesmo,
/// e a próxima rodada o encontra faltando e o lê de novo.</para>
///
/// <para><b>Quais anos são lidos.</b> Sempre o ano corrente e o anterior — é neles que o Banco Central
/// acrescenta contrato registrado com atraso (medido em 21/09/2026: 2025 tinha 3 linhas de trator a
/// mais do que o arquivo que o comercial baixou meses antes). Os anos mais antigos só quando ainda não
/// estão no banco: eles não mudam, e relê-los todo mês seria baixar 200 mil linhas para não gravar
/// nenhuma.</para>
///
/// <para><b>O ano relido espelha o Banco Central.</b> Linha que saiu da fonte naquele ano é apagada: o
/// SICOR publica somas, e uma combinação que sumiu foi reclassificada — mantê-la contaria o mesmo
/// crédito duas vezes. O número de linhas apagadas sai no relatório da carga. Ao contrário da cotação
/// de preço, que é um fato de um mês, esta linha é um agregado que o próprio Banco Central refaz.</para>
///
/// <para><b>O município vem do de-para gravado</b> (issue 154). O código do Banco Central não é o do IBGE: na
/// primeira rodada o nome, sem acento, caixa e apóstrofo, casa os 637 municípios de SP que aparecem no SICOR
/// desde 2013 (medido em 21/09/2026), e o par vai para <c>organizacao.CorrespondenciaDeMunicipio</c>, pela chave
/// do Banco Central. Nas rodadas seguintes o par responde, e nenhum nome é comparado de novo. Código sem par
/// vira recusa, com o nome e o número de linhas.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="sicor">A leitura do SICOR.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDoCreditoRural(
    Func<CrmDbContext> abrirContexto,
    LeitorDoSicor sicor,
    long usuarioId,
    Action<string> relatar)
{
    private const string FluxoDosCatalogos = "BCB.SICOR_TABELAS";
    private const string FluxoDoInvestimento = "BCB.SICOR_INVESTIMENTO";
    private const string EtapaDosCatalogos = "Tabelas auxiliares do SICOR (Banco Central)";
    private const string EtapaDoInvestimento = "Crédito rural de investimento em SP (SICOR)";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];

    /// <summary>Executa a carga.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<(string Etapa, string Rotulo, int Valor)>> ExecutarAsync(CancellationToken ct)
    {
        await CarregarCatalogosAsync(ct);
        await CarregarInvestimentosAsync(ct);
        return _contagens;
    }

    // =============================================================================================
    // 1. Tabelas auxiliares
    // =============================================================================================

    private async Task CarregarCatalogosAsync(CancellationToken ct)
    {
        await using var trava = await TravaDeFluxo.TomarAsync(abrirContexto(), FluxoDosCatalogos, ct);

        relatar("Lendo as tabelas auxiliares do SICOR (programa, subprograma, fonte e produto)…");
        var lidos = new List<(string Tipo, ItemDoCatalogoDoSicor Item)>();
        foreach (var (catalogo, tipo) in new[]
                 {
                     (CatalogoDoSicor.Programa, "PROGRAMA"), (CatalogoDoSicor.Subprograma, "SUBPROGRAMA"),
                     (CatalogoDoSicor.Fonte, "FONTE"), (CatalogoDoSicor.Produto, "PRODUTO")
                 })
        {
            var itens = await sicor.LerCatalogoAsync(catalogo, ct);
            lidos.AddRange(itens.Select(i => (tipo, i)));
            _contagens.Add((EtapaDosCatalogos, $"itens lidos — {tipo.ToLowerInvariant()}", itens.Count));
        }

        var agora = DateTime.UtcNow;
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaAsync(contexto, ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var existentes = (await contexto.ItensDoSicor.ToListAsync(ct)).ToDictionary(i => (i.Tipo, i.CodigoPai, i.Codigo));
        var recusas = new List<(object Conteudo, string Motivo)>();
        var novos = new List<ItemDoSicor>();
        int revisados = 0;

        // CÓDIGO REPETIDO NA TABELA: o produto tem vigências sucessivas com o mesmo código. Vale a última
        // linha publicada, que é a vigente.
        foreach (var (tipo, item) in lidos.GroupBy(l => (l.Tipo, l.Item.CodigoPai, l.Item.Codigo)).Select(g => g.Last()))
        {
            try
            {
                if (existentes.TryGetValue((tipo, item.CodigoPai, item.Codigo), out var existente))
                {
                    if (existente.Revisar(item.Descricao, item.VigenteDe, item.VigenteAte, usuarioId, agora)) revisados++;
                }
                else
                {
                    novos.Add(ItemDoSicor.Registrar(tipo, item.Codigo, item.CodigoPai, item.Descricao, item.VigenteDe, item.VigenteAte, usuarioId, agora));
                }
            }
            catch (RegraDeNegocioViolada regra)
            {
                recusas.Add((item, regra.Message));
            }
        }

        contexto.ItensDoSicor.AddRange(novos);
        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDosCatalogos, recusas, ct);
        await contexto.SaveChangesAsync(ct);
        await CargaDeTerritorio.RegistrarRodadaAsync(contexto, sistemaId, FluxoDosCatalogos, lidos.Count, novos.Count + revisados, recusas.Count, ct, "tabelas auxiliares");
        await transacao.CommitAsync(ct);

        _contagens.Add((EtapaDosCatalogos, "itens novos", novos.Count));
        _contagens.Add((EtapaDosCatalogos, "itens revisados", revisados));
        _contagens.Add((EtapaDosCatalogos, "itens recusados", recusas.Count));
    }

    // =============================================================================================
    // 2. Investimento, ano a ano
    // =============================================================================================

    private async Task CarregarInvestimentosAsync(CancellationToken ct)
    {
        await using var trava = await TravaDeFluxo.TomarAsync(abrirContexto(), FluxoDoInvestimento, ct);

        int municipiosDeSaoPaulo;
        HashSet<short> anosNoBanco;
        await using (var leitura = abrirContexto())
        {
            municipiosDeSaoPaulo = await leitura.Municipios.AsNoTracking().CountAsync(m => m.Uf == "SP" && m.CodigoIbge != null, ct);
            anosNoBanco = [.. await leitura.CreditosRuraisDeInvestimento.Select(c => c.Ano).Distinct().ToListAsync(ct)];
        }

        if (municipiosDeSaoPaulo == 0)
            throw new InvalidOperationException(
                "Nenhum município de SP tem código do IBGE neste banco. O crédito rural não tem onde ser gravado: " +
                "rode a carga do território (--somente-territorio) antes.");

        var anoCorrente = (short)DateTime.UtcNow.Year;
        var anos = Enumerable.Range(LeitorDoSicor.PrimeiroAno, anoCorrente - LeitorDoSicor.PrimeiroAno + 1)
            .Select(a => (short)a)
            .Where(a => a >= anoCorrente - 1 || !anosNoBanco.Contains(a))
            .ToList();

        var recusasPorMunicipio = new Dictionary<int, (string Nome, int Linhas)>();
        var recusasDeValor = new List<(object Conteudo, string Motivo)>();
        int lidas = 0, novas = 0, revisadas = 0, mantidas = 0, apagadas = 0;
        int pelaCorrespondencia = 0, paresNovos = 0;
        var municipiosCasados = new HashSet<int>();

        foreach (var ano in anos)
        {
            relatar($"Lendo o crédito de investimento de SP em {ano} (SICOR)…");
            var linhas = await sicor.LerInvestimentosAsync(LeitorDoSicor.SaoPauloNoBancoCentral, ano, ct);
            lidas += linhas.Count;

            var agora = DateTime.UtcNow;
            await using var contexto = abrirContexto();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var sistemaId = await SistemaAsync(contexto, ct);
            contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

            // O MUNICÍPIO VEM DO DE-PARA GRAVADO (issue 154), pela chave que a fonte tem: o código do Banco
            // Central, que não é o do IBGE. Só a primeira rodada casa pelo nome — e grava o par.
            var dePara = await CorrespondenciaDeMunicipios.AbrirAsync(contexto, FluxoDoInvestimento, "SP", usuarioId, ct);

            var existentes = (await contexto.CreditosRuraisDeInvestimento.Where(c => c.Ano == ano).ToListAsync(ct))
                .ToDictionary(c => c.ChaveNatural);
            var vistas = new HashSet<CreditoRuralDeInvestimento.Chave>();
            var novasDoAno = new List<CreditoRuralDeInvestimento>();

            // O QUE A FONTE TROUXE, recusado ou não (issue 153). Uma linha recusada — município que não casou, valor
            // que não vale — continua existindo no Banco Central; se já estava no banco, fica como estava. Antes ela
            // caía em "saiu da fonte" e era apagada: um município com grafia nova perderia os anos relidos inteiros.
            var naFonte = new HashSet<CreditoRuralDeInvestimento.Chave>();

            foreach (var l in linhas)
            {
                var chave = new CreditoRuralDeInvestimento.Chave(
                    l.CodigoMunicipioBcb, l.Ano, l.Mes, l.CodigoProduto, l.CodigoPrograma,
                    l.CodigoSubprograma, l.CodigoFonte, l.CodigoSeguro, l.Atividade, l.CodigoModalidade);
                naFonte.Add(chave);

                if (!dePara.Resolver(l.CodigoMunicipioBcb.ToString(CultureInfo.InvariantCulture), l.Municipio, agora, out var municipioId))
                {
                    recusasPorMunicipio[l.CodigoMunicipioBcb] = (l.Municipio,
                        (recusasPorMunicipio.TryGetValue(l.CodigoMunicipioBcb, out var r) ? r.Linhas : 0) + 1);
                    continue;
                }

                municipiosCasados.Add(l.CodigoMunicipioBcb);

                if (!vistas.Add(chave))
                {
                    recusasDeValor.Add((l, "A mesma combinação apareceu duas vezes no mesmo ano. Vale a primeira."));
                    continue;
                }

                try
                {
                    if (existentes.TryGetValue(chave, out var existente))
                    {
                        if (existente.Revisar(l.Valor, l.Area, usuarioId, agora)) revisadas++;
                        else mantidas++;
                    }
                    else
                    {
                        novasDoAno.Add(CreditoRuralDeInvestimento.Registrar(chave, municipioId, l.Valor, l.Area, usuarioId, agora));
                    }
                }
                catch (RegraDeNegocioViolada regra)
                {
                    vistas.Remove(chave);
                    recusasDeValor.Add((l, regra.Message));
                }
            }

            // O ANO RELIDO ESPELHA A FONTE: o que saiu dela naquele ano sai daqui (ver o resumo da classe), e a trilha
            // guarda a linha inteira — valor e chave (issue 153). Só se a leitura trouxe alguma coisa — uma resposta
            // vazia por falha do serviço não pode apagar um ano inteiro —, e só o que a fonte de fato não trouxe.
            var sairam = linhas.Count == 0 ? [] : existentes.Values.Where(e => !naFonte.Contains(e.ChaveNatural)).ToList();
            contexto.CreditosRuraisDeInvestimento.RemoveRange(sairam);
            apagadas += sairam.Count;

            contexto.CreditosRuraisDeInvestimento.AddRange(novasDoAno);
            novas += novasDoAno.Count;

            dePara.Gravar(contexto);
            pelaCorrespondencia += dePara.CasadosPelaCorrespondencia;
            paresNovos += dePara.CasadosPorNome;

            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        // AS RECUSAS SÃO REGISTRADAS NO FIM, numa transação só: município que não casou vira UMA recusa,
        // com o número de linhas, e não quinze mil.
        await using (var contexto = abrirContexto())
        {
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);
            var sistemaId = await SistemaAsync(contexto, ct);
            contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

            var recusas = recusasPorMunicipio
                .Select(r => ((object)new { CodigoMunicipioBcb = r.Key, r.Value.Nome, r.Value.Linhas },
                    $"O município \"{r.Value.Nome}\" (código {r.Key} no Banco Central) não casou com o catálogo de SP: {r.Value.Linhas} linhas de fora."))
                .Concat(recusasDeValor)
                .ToList();

            await CargaDeTerritorio.SubstituirRecusasAsync(contexto, FluxoDoInvestimento, recusas, ct);
            await contexto.SaveChangesAsync(ct);

            var periodo = anos.Count == 0 ? "nenhum ano" : $"anos {anos.Min()} a {anos.Max()}";
            await CargaDeTerritorio.RegistrarRodadaAsync(
                contexto, sistemaId, FluxoDoInvestimento, lidas, novas + revisadas + apagadas, recusas.Count, ct, periodo);
            await transacao.CommitAsync(ct);

            Contar($"linhas lidas ({periodo})", lidas);
        }

        Contar("anos relidos", anos.Count);
        Contar("linhas novas", novas);
        Contar("linhas revisadas pelo Banco Central (valor anterior na trilha)", revisadas);
        Contar("linhas mantidas sem mudança", mantidas);
        Contar("linhas que saíram do SICOR no ano relido (apagadas)", apagadas);
        Contar("municípios do Banco Central casados com o catálogo", municipiosCasados.Count);
        Contar("linhas resolvidas pelo de-para já gravado", pelaCorrespondencia);
        Contar("correspondências novas gravadas (casadas por nome)", paresNovos);
        Contar("municípios sem casamento (recusados)", recusasPorMunicipio.Count);
        Contar("linhas recusadas por valor ou repetição", recusasDeValor.Count);
    }

    private static Task<int> SistemaAsync(CrmDbContext contexto, CancellationToken ct) =>
        CargaDeTerritorio.SistemaAsync(contexto, "BCB", "Banco Central — SGS e SICOR", "REST público, somente leitura", ct);

    private void Contar(string rotulo, int valor) => _contagens.Add((EtapaDoInvestimento, rotulo, valor));
}
