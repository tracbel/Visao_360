using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Ibge;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A CARGA DO TERRITÓRIO — município oficial, área de atuação, responsáveis e área plantada
/// (documento 32).
///
/// <para><b>Quatro etapas, cada uma na sua transação, nesta ordem:</b></para>
/// <list type="number">
///   <item>reconhecer o catálogo de municípios no cadastro oficial do IBGE (código e nome);</item>
///   <item>a planilha de área de atuação — ADR, região, loja e o CEN que ela declara;</item>
///   <item>a planilha de CEN e gestor — o CEN e o gestor que ela declara;</item>
///   <item>a área plantada da PAM para os municípios de São Paulo.</item>
/// </list>
///
/// <para><b>As planilhas são lidas antes de tudo</b>: coluna faltando para a carga sem ter tocado
/// no banco. Uma etapa que falha desfaz só a si mesma; as anteriores ficam, e a reexecução as
/// encontra prontas.</para>
///
/// <para><b>Reexecutável.</b> Rodar de novo sobre as mesmas fontes não grava nada novo: o
/// município reconhecido não é reconhecido de novo, a afirmação igual é mantida, a área igual não
/// é reapurada. O que muda na fonte encerra a linha antiga e abre a nova — nunca apaga.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="ibge">A leitura das APIs públicas do IBGE.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDeTerritorio(
    Func<CrmDbContext> abrirContexto,
    LeitorDoIbge ibge,
    long usuarioId,
    Action<string> relatar)
{
    private const int CodigoDeSaoPaulo = 35;

    private const string FluxoDoCatalogo = "IBGE.MUNICIPIO";
    private const string FluxoDaAreaPlantada = "IBGE.AREA_PLANTADA";
    private const string FluxoDaAreaDeAtuacao = "PLANILHA.AREA_DE_ATUACAO";
    private const string FluxoDoCenEGestor = "PLANILHA.CEN_E_GESTOR";

    /// <summary>As colunas que a carga usa da planilha de área de atuação.</summary>
    private static readonly string[] ColunasDaAreaDeAtuacao =
        ["%ChvMunicipio", "CEN", "FlgADR", "Loja (Responsável)", "Município", "Região", "UF"];

    /// <summary>As colunas que a carga usa da planilha de CEN e gestor.</summary>
    private static readonly string[] ColunasDoCenEGestor =
        ["%ChaveTerritorio", "Gerente_Territorio", "Loja_Territorio", "Municipio_Territorio", "Vendedor_Territorio"];

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];

    /// <summary>Uma afirmação de responsável lida de uma planilha.</summary>
    private sealed record Afirmacao(int MunicipioId, PapelNoMunicipio Papel, string Nome, string? Chave, int Linha);

    /// <summary>Executa as quatro etapas.</summary>
    /// <param name="caminhoDaAreaDeAtuacao">O caminho de <c>Area de Atuação.xlsx</c>.</param>
    /// <param name="caminhoDoCenEGestor">O caminho de <c>CEN e Gestor por Municipio.xlsx</c>.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <returns>As contagens de cada etapa, na ordem em que aconteceram.</returns>
    public async Task<IReadOnlyList<(string Etapa, string Rotulo, int Valor)>> ExecutarAsync(
        string caminhoDaAreaDeAtuacao, string caminhoDoCenEGestor, CancellationToken ct)
    {
        var areaDeAtuacao = PlanilhaDoComercial.Ler(caminhoDaAreaDeAtuacao, ColunasDaAreaDeAtuacao);
        var cenEGestor = PlanilhaDoComercial.Ler(caminhoDoCenEGestor, ColunasDoCenEGestor);

        relatar("Lendo o cadastro oficial de municípios do IBGE…");
        var oficiais = await ibge.LerMunicipiosAsync(ct);
        await ReconhecerCatalogoAsync(oficiais, ct);

        // A MESMA CONFERÊNCIA QUE A CARGA DO CADASTRO FAZ AO FIM de cada recarga do sistema de origem
        // (documento 32, seção 4.6): as duas cargas usam a mesma peça, e a ordem entre elas deixa de
        // importar.
        relatar("Conferindo os endereços das grafias cortadas contra a malha municipal oficial (IBGE)…");
        var consolidacao = await new ConsolidacaoDeGrafiasCortadas(abrirContexto, ibge.LerMalhaMunicipalAsync, usuarioId)
            .ExecutarAsync(ct);

        const string etapaDasGrafias = "Grafias cortadas → município oficial (endereços)";
        Contar(etapaDasGrafias, "grafias cortadas com município oficial reconhecido", consolidacao.Grafias);
        Contar(etapaDasGrafias, "endereços encontrados nessas grafias nesta rodada", consolidacao.EnderecosLidos);
        Contar(etapaDasGrafias, "endereços reapontados para o município oficial", consolidacao.Reapontados);
        Contar(etapaDasGrafias, "endereços pendentes de conferência (motivo na fila de revisão)", consolidacao.Pendentes);
        Contar(etapaDasGrafias, "contorno oficial do IBGE lido nesta rodada (1 = sim)", consolidacao.ContornoDisponivel ? 1 : 0);

        var oficiaisDeSaoPaulo = oficiais.Where(o => o.Uf == "SP").ToList();
        var municipioPorCodigo = await MunicipiosPorCodigoAsync(ct);
        var usuarios = await IndiceDeUsuariosAsync(ct);

        relatar($"Lendo {areaDeAtuacao.NomeDoArquivo}…");
        await CarregarAreaDeAtuacaoAsync(areaDeAtuacao, oficiaisDeSaoPaulo, municipioPorCodigo, usuarios, ct);

        relatar($"Lendo {cenEGestor.NomeDoArquivo}…");
        await CarregarCenEGestorAsync(cenEGestor, oficiaisDeSaoPaulo, municipioPorCodigo, usuarios, ct);

        relatar("Lendo a área plantada da PAM (SIDRA 5457) dos municípios de São Paulo…");
        var areaPlantada = await ibge.LerAreaPlantadaAsync(CodigoDeSaoPaulo, ct);
        await CarregarAreaPlantadaAsync(areaPlantada, municipioPorCodigo, ct);

        return _contagens;
    }

    // =============================================================================================
    // 1. O catálogo de municípios no IBGE
    // =============================================================================================

    private async Task ReconhecerCatalogoAsync(IReadOnlyList<MunicipioDoIbge> oficiais, CancellationToken ct)
    {
        const string etapa = "Catálogo de municípios × IBGE";

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaAsync(
            contexto, "IBGE", "IBGE — localidades e SIDRA", "REST público, somente leitura", ct);

        // A TRILHA É AUTOMÁTICA (documento 41, fase 2): o código e a grafia reconhecidos vão para
        // auditoria.AlteracaoDeCampo no SaveChanges, como integração do IBGE. A regra que a carga
        // aplicava à mão — só entra o que o banco reconhece como mudança, comparando sem caixa nem
        // acento — agora é a da própria trilha.
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var catalogo = await contexto.Municipios.AsNoTracking()
            .Select(m => new MunicipioDoCatalogo(m.Id, m.Nome, m.Uf, m.CodigoIbge))
            .ToListAsync(ct);

        var resultado = SaneamentoDeTerritorio.Reconhecer(catalogo, oficiais);

        // O ÍNDICE ÚNICO DE NOME, VISTO COMO O BANCO O VÊ. Renomear uma linha para o nome oficial
        // não pode esbarrar no nome de OUTRA linha — a checagem é feita aqui, antes, para a recusa
        // sair com motivo em vez de um erro de restrição que desfaria a etapa inteira.
        var nomesEmUso = catalogo
            .GroupBy(m => (m.Uf, Chave: SaneamentoDeTerritorio.ChaveDaColacao(m.Nome)))
            .ToDictionary(g => g.Key, g => g.Select(m => m.Id).ToHashSet());

        var recusas = new List<(object Conteudo, string Motivo)>();
        var entidades = new Dictionary<int, Municipio>();

        foreach (var bloco in resultado.Reconhecidos.Select(r => r.MunicipioId).Chunk(1000))
            foreach (var municipio in await contexto.Municipios.Where(m => bloco.Contains(m.Id)).ToListAsync(ct))
                entidades[municipio.Id] = municipio;

        var reconhecidos = 0;

        foreach (var reconhecimento in resultado.Reconhecidos)
        {
            var municipio = entidades[reconhecimento.MunicipioId];
            var chaveOficial = (municipio.Uf, SaneamentoDeTerritorio.ChaveDaColacao(reconhecimento.Oficial.Nome));

            if (nomesEmUso.TryGetValue(chaveOficial, out var donos) && donos.Any(id => id != municipio.Id))
            {
                recusas.Add((reconhecimento, "O nome oficial já é o nome de outra linha do catálogo. Não renomeado nem fundido: revisar as duas linhas."));
                continue;
            }

            if (!municipio.ReconhecerNoIbge(reconhecimento.Oficial.Codigo, reconhecimento.Oficial.Nome)) continue;

            // O código e a grafia vão para a trilha no SaveChanges. A grafia em caixa alta sem acento
            // que o banco não reconhece como mudança não entra, e continua recuperável pela chave de
            // origem do Vórtice em integracao.ChaveExterna.
            reconhecidos++;
        }

        var criados = 0;
        foreach (var oficial in resultado.AusentesNoCatalogo)
        {
            if (nomesEmUso.ContainsKey((oficial.Uf, SaneamentoDeTerritorio.ChaveDaColacao(oficial.Nome))))
            {
                recusas.Add((oficial, "Município oficial ausente do catálogo, mas o nome dele já é usado por outra linha. Não criado: revisar."));
                continue;
            }

            contexto.Municipios.Add(Municipio.Criar(oficial.Nome, oficial.Uf, oficial.Codigo));
            criados++;
        }

        foreach (var variante in resultado.Variantes)
            recusas.Add((variante, $"Segunda grafia de {variante.Oficial.Nome}/{variante.Oficial.Uf}: o código já pertence a outra linha. Não fundida — os endereços que apontam para ela ficam sem código IBGE até a revisão."));

        foreach (var linha in resultado.SemCorrespondencia)
            recusas.Add((linha, "Não corresponde a nenhum município do IBGE nesta UF (distrito, grafia sem equivalente ou texto que não é município)."));

        await SubstituirRecusasAsync(contexto, FluxoDoCatalogo, recusas, ct);
        await contexto.SaveChangesAsync(ct);
        await RegistrarRodadaAsync(contexto, sistemaId, FluxoDoCatalogo, oficiais.Count, reconhecidos + criados, recusas.Count, ct);
        await transacao.CommitAsync(ct);

        Contar(etapa, "municípios oficiais lidos", oficiais.Count);
        Contar(etapa, "linhas já reconhecidas antes desta rodada", catalogo.Count(m => m.CodigoIbge is not null));
        Contar(etapa, "reconhecidas por nome igual", resultado.Reconhecidos.Count(r => r.Forma == FormaDeReconhecimento.NomeIgual));
        Contar(etapa, "reconhecidas pela grafia do apóstrofo", resultado.Reconhecidos.Count(r => r.Forma == FormaDeReconhecimento.GrafiaDoApostrofo));
        Contar(etapa, "reconhecidas por nome cortado em 20 caracteres", resultado.Reconhecidos.Count(r => r.Forma == FormaDeReconhecimento.NomeTruncadoNaOrigem));
        Contar(etapa, "gravadas nesta rodada (código e nome oficial)", reconhecidos);
        Contar(etapa, "municípios oficiais criados (não havia linha)", criados);
        Contar(etapa, "segundas grafias, não fundidas", resultado.Variantes.Count);
        Contar(etapa, "linhas sem município do IBGE", resultado.SemCorrespondencia.Count);
    }

    // =============================================================================================
    // 2. A área de atuação
    // =============================================================================================

    private async Task CarregarAreaDeAtuacaoAsync(
        PlanilhaDoComercial planilha,
        IReadOnlyList<MunicipioDoIbge> oficiaisDeSaoPaulo,
        IReadOnlyDictionary<int, int> municipioPorCodigo,
        IReadOnlyDictionary<string, IReadOnlySet<long>> usuarios,
        CancellationToken ct)
    {
        const string etapa = "Área de atuação (planilha)";
        var agora = DateTime.UtcNow;

        var oficialPorNome = oficiaisDeSaoPaulo
            .GroupBy(o => SaneamentoDeTerritorio.ChaveSemApostrofo(o.Nome))
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaAsync(contexto, "PLANILHA", "Planilhas do comercial", "Arquivo xlsx lido pela carga", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Importacao, sistemaId);
        var lojas = await LojasAsync(contexto, ct);

        // A LINHA MAIS RECENTE DE CADA MUNICÍPIO, vigente ou não: um município que saiu e voltou
        // reabre a linha dele em vez de ganhar uma segunda.
        var existentes = (await contexto.MunicipiosDaAreaDeAtuacao.ToListAsync(ct))
            .GroupBy(m => m.MunicipioId)
            .ToDictionary(g => g.Key, g => g.OrderBy(m => m.EncerradoEm is null ? 0 : 1).ThenByDescending(m => m.Id).First());

        var recusas = new List<(object Conteudo, string Motivo)>();
        var afirmacoes = new List<Afirmacao>();
        var vistos = new HashSet<int>();
        int novas = 0, alteradas = 0, mantidas = 0, naAdr = 0, lojaSemFilial = 0;

        foreach (var linha in planilha.Linhas)
        {
            var nome = linha["Município"];

            if (SaneamentoDeTerritorio.CelulaVazia(nome))
            {
                recusas.Add((linha, $"Linha {linha.Numero} sem município."));
                continue;
            }

            if (!string.Equals(linha["UF"]?.Trim(), "SP", StringComparison.OrdinalIgnoreCase))
            {
                recusas.Add((linha, $"Linha {linha.Numero}: UF \"{linha["UF"]}\" — a área de atuação é de São Paulo."));
                continue;
            }

            if (!oficialPorNome.TryGetValue(SaneamentoDeTerritorio.ChaveSemApostrofo(nome!), out var candidatos)
                || candidatos.Count != 1
                || !municipioPorCodigo.TryGetValue(candidatos[0].Codigo, out var municipioId))
            {
                recusas.Add((linha, $"Linha {linha.Numero}: \"{nome}\" não é, sem ambiguidade, um município de SP no IBGE."));
                continue;
            }

            if (!vistos.Add(municipioId))
            {
                recusas.Add((linha, $"Linha {linha.Numero}: {candidatos[0].Nome} aparece mais de uma vez na planilha. Vale a primeira."));
                continue;
            }

            if (!TentarRegiao(linha["Região"], out var regiao))
            {
                recusas.Add((linha, $"Linha {linha.Numero}: região \"{linha["Região"]}\" fora do domínio (Norte, Noroeste ou vazio)."));
                vistos.Remove(municipioId);
                continue;
            }

            var pertenceAAdr = SaneamentoDeTerritorio.ChaveExata(linha["FlgADR"] ?? string.Empty) == "AREA ADR";
            if (pertenceAAdr) naAdr++;

            int? empresaId = null;
            var loja = linha["Loja (Responsável)"];
            if (!SaneamentoDeTerritorio.CelulaVazia(loja))
            {
                if (lojas.TryGetValue(SaneamentoDeTerritorio.ChaveExata(loja!), out var id)) empresaId = id;
                else lojaSemFilial++;
            }

            if (existentes.TryGetValue(municipioId, out var existente))
            {
                if (existente.Conferir(pertenceAAdr, regiao, empresaId, planilha.NomeDoArquivo, linha.Numero, usuarioId, agora)) alteradas++;
                else mantidas++;
            }
            else
            {
                contexto.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                    municipioId, pertenceAAdr, regiao, empresaId, planilha.NomeDoArquivo, linha.Numero, usuarioId, agora));
                novas++;
            }

            var cen = linha["CEN"];
            if (!SaneamentoDeTerritorio.CelulaVazia(cen))
                afirmacoes.Add(new Afirmacao(municipioId, PapelNoMunicipio.Cen, cen!, linha["%ChvMunicipio"], linha.Numero));
        }

        var encerradas = 0;
        foreach (var existente in existentes.Values.Where(e => e.EncerradoEm is null && !vistos.Contains(e.MunicipioId)))
        {
            existente.Encerrar(agora);
            encerradas++;
        }

        await contexto.SaveChangesAsync(ct);
        await GravarAfirmacoesAsync(contexto, etapa, FonteDoResponsavel.PlanilhaAreaDeAtuacao, afirmacoes, planilha.NomeDoArquivo, usuarios, agora, ct);
        await SubstituirRecusasAsync(contexto, FluxoDaAreaDeAtuacao, recusas, ct);
        await contexto.SaveChangesAsync(ct);
        await RegistrarRodadaAsync(contexto, sistemaId, FluxoDaAreaDeAtuacao, planilha.Linhas.Count, novas + alteradas, recusas.Count, ct);
        await transacao.CommitAsync(ct);

        Contar(etapa, "linhas de dado lidas", planilha.Linhas.Count);
        Contar(etapa, "municípios marcados como ADR", naAdr);
        Contar(etapa, "municípios fora da ADR", vistos.Count - naAdr);
        Contar(etapa, "linhas novas", novas);
        Contar(etapa, "linhas alteradas", alteradas);
        Contar(etapa, "linhas mantidas sem mudança", mantidas);
        Contar(etapa, "municípios que saíram da planilha (encerrados)", encerradas);
        Contar(etapa, "lojas sem filial correspondente no CRM", lojaSemFilial);
        Contar(etapa, "linhas recusadas", recusas.Count);
    }

    // =============================================================================================
    // 3. CEN e gestor por município
    // =============================================================================================

    private async Task CarregarCenEGestorAsync(
        PlanilhaDoComercial planilha,
        IReadOnlyList<MunicipioDoIbge> oficiaisDeSaoPaulo,
        IReadOnlyDictionary<int, int> municipioPorCodigo,
        IReadOnlyDictionary<string, IReadOnlySet<long>> usuarios,
        CancellationToken ct)
    {
        const string etapa = "CEN e gestor por município (planilha)";
        var agora = DateTime.UtcNow;
        var oficialPorCodigo = oficiaisDeSaoPaulo.ToDictionary(o => o.Codigo);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaAsync(contexto, "PLANILHA", "Planilhas do comercial", "Arquivo xlsx lido pela carga", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Importacao, sistemaId);

        var recusas = new List<(object Conteudo, string Motivo)>();
        var afirmacoes = new List<Afirmacao>();
        var vistos = new HashSet<int>();

        foreach (var linha in planilha.Linhas)
        {
            var chave = linha["%ChaveTerritorio"]?.Trim();
            var nome = linha["Municipio_Territorio"];

            if (SaneamentoDeTerritorio.CelulaVazia(nome))
            {
                recusas.Add((linha, $"Linha {linha.Numero}: a chave {chave} não traz município nem responsável."));
                continue;
            }

            // A CHAVE É O CÓDIGO IBGE SEM O "35" — verificado em 203 de 203 linhas (documento 32,
            // seção 4). E o nome tem de concordar com o código: chave e nome discordando é linha
            // que não se sabe de qual município é.
            if (chave is not { Length: 5 }
                || !int.TryParse($"{CodigoDeSaoPaulo}{chave}", NumberStyles.None, CultureInfo.InvariantCulture, out var codigo)
                || !oficialPorCodigo.TryGetValue(codigo, out var oficial))
            {
                recusas.Add((linha, $"Linha {linha.Numero}: a chave \"{chave}\" não é código IBGE de município de SP."));
                continue;
            }

            if (SaneamentoDeTerritorio.ChaveSemApostrofo(oficial.Nome) != SaneamentoDeTerritorio.ChaveSemApostrofo(nome!))
            {
                recusas.Add((linha, $"Linha {linha.Numero}: o código {codigo} é {oficial.Nome}, e a linha diz \"{nome}\"."));
                continue;
            }

            if (!municipioPorCodigo.TryGetValue(codigo, out var municipioId) || !vistos.Add(municipioId))
            {
                recusas.Add((linha, $"Linha {linha.Numero}: {oficial.Nome} sem linha no catálogo ou repetido na planilha."));
                continue;
            }

            var vendedor = linha["Vendedor_Territorio"];
            if (!SaneamentoDeTerritorio.CelulaVazia(vendedor))
                afirmacoes.Add(new Afirmacao(municipioId, PapelNoMunicipio.Cen, vendedor!, chave, linha.Numero));

            var gerente = linha["Gerente_Territorio"];
            if (!SaneamentoDeTerritorio.CelulaVazia(gerente))
                afirmacoes.Add(new Afirmacao(municipioId, PapelNoMunicipio.Gestor, gerente!, chave, linha.Numero));
        }

        await GravarAfirmacoesAsync(contexto, etapa, FonteDoResponsavel.PlanilhaCenEGestorPorMunicipio, afirmacoes, planilha.NomeDoArquivo, usuarios, agora, ct);
        await SubstituirRecusasAsync(contexto, FluxoDoCenEGestor, recusas, ct);
        await contexto.SaveChangesAsync(ct);
        await RegistrarRodadaAsync(contexto, sistemaId, FluxoDoCenEGestor, planilha.Linhas.Count, afirmacoes.Count, recusas.Count, ct);
        await transacao.CommitAsync(ct);

        Contar(etapa, "linhas de dado lidas", planilha.Linhas.Count);
        Contar(etapa, "municípios com responsável", vistos.Count);
        Contar(etapa, "linhas recusadas", recusas.Count);
    }

    /// <summary>
    /// Grava as afirmações de UMA fonte: mantém a igual, encerra a que mudou ou sumiu, abre a nova.
    /// </summary>
    private async Task GravarAfirmacoesAsync(
        CrmDbContext contexto,
        string etapa,
        FonteDoResponsavel fonte,
        IReadOnlyList<Afirmacao> afirmacoes,
        string arquivo,
        IReadOnlyDictionary<string, IReadOnlySet<long>> usuarios,
        DateTime agora,
        CancellationToken ct)
    {
        var vigentes = await contexto.ResponsaveisPelosMunicipios
            .Where(r => r.Fonte == fonte && r.EncerradoEm == null)
            .ToListAsync(ct);

        var porChave = vigentes.ToDictionary(r => (r.MunicipioId, r.Papel));
        var afirmadas = new HashSet<(int, PapelNoMunicipio)>();
        var novas = new List<ResponsavelPeloMunicipio>();
        var porSituacao = new Dictionary<(PapelNoMunicipio, SituacaoDoResponsavel), int>();
        int mantidas = 0, substituidas = 0;

        foreach (var afirmacao in afirmacoes)
        {
            afirmadas.Add((afirmacao.MunicipioId, afirmacao.Papel));

            var (situacao, usuarioIdentificado) = SaneamentoDeTerritorio.IdentificarResponsavel(afirmacao.Nome, usuarios);
            porSituacao[(afirmacao.Papel, situacao)] = porSituacao.GetValueOrDefault((afirmacao.Papel, situacao)) + 1;

            if (porChave.TryGetValue((afirmacao.MunicipioId, afirmacao.Papel), out var atual))
            {
                if (atual.AfirmaOMesmo(afirmacao.Nome, situacao, usuarioIdentificado))
                {
                    mantidas++;
                    continue;
                }

                atual.Encerrar(agora);
                substituidas++;
            }

            novas.Add(ResponsavelPeloMunicipio.Registrar(
                afirmacao.MunicipioId, afirmacao.Papel, fonte, afirmacao.Nome, situacao, usuarioIdentificado,
                afirmacao.Chave, arquivo, afirmacao.Linha, usuarioId, agora));
        }

        var encerradas = 0;
        foreach (var vigente in vigentes.Where(v => !afirmadas.Contains((v.MunicipioId, v.Papel))))
        {
            vigente.Encerrar(agora);
            encerradas++;
        }

        // ENCERRA ANTES DE ABRIR, em duas gravações: o índice único filtrado não admite duas
        // afirmações vigentes para o mesmo município, papel e fonte nem por um instante, e a
        // ordem dos comandos dentro de uma gravação só não é garantida.
        await contexto.SaveChangesAsync(ct);
        contexto.ResponsaveisPelosMunicipios.AddRange(novas);
        await contexto.SaveChangesAsync(ct);

        foreach (var ((papel, situacao), quantas) in porSituacao.OrderBy(p => p.Key))
            Contar(etapa, $"{papel}: {situacao}", quantas);

        Contar(etapa, "afirmações novas", novas.Count - substituidas);
        Contar(etapa, "afirmações mantidas sem mudança", mantidas);
        Contar(etapa, "afirmações que mudaram (antiga encerrada, nova aberta)", substituidas);
        Contar(etapa, "afirmações que a planilha deixou de fazer (encerradas)", encerradas);
    }

    // =============================================================================================
    // 4. A área plantada
    // =============================================================================================

    private async Task CarregarAreaPlantadaAsync(
        IReadOnlyList<LinhaDaAreaPlantada> linhas, IReadOnlyDictionary<int, int> municipioPorCodigo, CancellationToken ct)
    {
        const string etapa = "Área plantada (PAM/IBGE)";
        var agora = DateTime.UtcNow;

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaAsync(contexto, "IBGE", "IBGE — localidades e SIDRA", "REST público, somente leitura", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var anos = linhas.Select(l => l.Ano).Distinct().ToList();
        var existentes = (await contexto.AreasPlantadasNosMunicipios.Where(a => anos.Contains(a.Ano)).ToListAsync(ct))
            .ToDictionary(a => (a.MunicipioId, a.Ano, a.ProdutoCodigoIbge));

        var recusas = new List<(object Conteudo, string Motivo)>();
        var novas = new List<AreaPlantadaNoMunicipio>();
        int alteradas = 0, mantidas = 0, naoDisponiveis = 0, zeros = 0;

        foreach (var linha in linhas)
        {
            if (!municipioPorCodigo.TryGetValue(linha.CodigoDoMunicipio, out var municipioId))
            {
                recusas.Add((linha, $"O município {linha.CodigoDoMunicipio} não está reconhecido no catálogo."));
                continue;
            }

            decimal? area;
            try
            {
                area = SaneamentoDeTerritorio.AreaDoSidra(linha.ValorBruto);
            }
            catch (FormatException formato)
            {
                recusas.Add((linha, formato.Message));
                continue;
            }

            if (area is null) naoDisponiveis++;
            else if (area == 0) zeros++;

            if (existentes.TryGetValue((municipioId, linha.Ano, linha.ProdutoCodigo), out var existente))
            {
                if (existente.Reapurar(linha.ProdutoNome, area, usuarioId, agora)) alteradas++;
                else mantidas++;
            }
            else
            {
                novas.Add(AreaPlantadaNoMunicipio.Registrar(municipioId, linha.Ano, linha.ProdutoCodigo, linha.ProdutoNome, area, usuarioId, agora));
            }
        }

        contexto.AreasPlantadasNosMunicipios.AddRange(novas);
        await SubstituirRecusasAsync(contexto, FluxoDaAreaPlantada, recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var ultimoAno = anos.Count == 0 ? "sem linhas" : $"ano {anos.Max()}";
        await RegistrarRodadaAsync(contexto, sistemaId, FluxoDaAreaPlantada, linhas.Count, novas.Count + alteradas, recusas.Count, ct, ultimoAno);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"linhas lidas ({ultimoAno})", linhas.Count);
        Contar(etapa, "linhas novas", novas.Count);
        Contar(etapa, "linhas reapuradas", alteradas);
        Contar(etapa, "linhas mantidas sem mudança", mantidas);
        Contar(etapa, "valores zero (\"-\" no IBGE)", zeros);
        Contar(etapa, "valores não disponíveis (\"...\" ou \"X\" no IBGE)", naoDisponiveis);
        Contar(etapa, "linhas recusadas", recusas.Count);
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    private void Contar(string etapa, string rotulo, int valor) => _contagens.Add((etapa, rotulo, valor));

    private static bool TentarRegiao(string? celula, out RegiaoDaAreaDeAtuacao regiao)
    {
        regiao = RegiaoDaAreaDeAtuacao.NaoInformada;
        if (SaneamentoDeTerritorio.CelulaVazia(celula)) return true;

        switch (SaneamentoDeTerritorio.ChaveExata(celula!))
        {
            case "NORTE":
                regiao = RegiaoDaAreaDeAtuacao.Norte;
                return true;
            case "NOROESTE":
                regiao = RegiaoDaAreaDeAtuacao.Noroeste;
                return true;
            default:
                return false;
        }
    }

    private async Task<IReadOnlyDictionary<int, int>> MunicipiosPorCodigoAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        return await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge != null)
            .ToDictionaryAsync(m => m.CodigoIbge!.Value, m => m.Id, ct);
    }

    /// <summary>
    /// Só usuário de natureza PESSOA responde por município. Conta de departamento, de teste ou de
    /// sistema com o mesmo primeiro nome de alguém não pode virar o CEN de uma cidade.
    /// </summary>
    private async Task<IReadOnlyDictionary<string, IReadOnlySet<long>>> IndiceDeUsuariosAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        var pessoas = await contexto.Usuarios.AsNoTracking()
            .Where(u => u.Natureza == NaturezaDoUsuario.Pessoa)
            .Select(u => new { u.Id, u.NomePrincipal, u.NomeExibicao, u.NomeCompleto })
            .ToListAsync(ct);

        return SaneamentoDeTerritorio.IndexarUsuarios(
            pessoas.Select(p => (p.Id, p.NomePrincipal, p.NomeExibicao, p.NomeCompleto)));
    }

    /// <summary>
    /// A loja da planilha → a filial do CRM, pelo nome da cidade depois do travessão
    /// ("Tracbel Agro — Ribeirão Preto" → RIBEIRAO PRETO). Nome que aparece em duas filiais fica de
    /// fora: ambiguidade não se resolve por ordem.
    /// </summary>
    private static async Task<IReadOnlyDictionary<string, int>> LojasAsync(CrmDbContext contexto, CancellationToken ct)
    {
        var empresas = await contexto.Empresas.AsNoTracking().Select(e => new { e.Id, e.Nome }).ToListAsync(ct);

        return empresas
            .GroupBy(e => SaneamentoDeTerritorio.ChaveExata(e.Nome.Split('—').Last()))
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.Single().Id, StringComparer.Ordinal);
    }

    internal static async Task<int> SistemaAsync(
        CrmDbContext contexto, string codigo, string nome, string meioDeAcesso, CancellationToken ct)
    {
        var existente = await contexto.Sistemas.FirstOrDefaultAsync(s => s.Codigo == codigo, ct);
        if (existente is not null) return existente.Id;

        var sistema = Sistema.Criar(codigo, nome, meioDeAcesso);
        contexto.Sistemas.Add(sistema);
        await contexto.SaveChangesAsync(ct);
        return sistema.Id;
    }

    /// <summary>
    /// As recusas da rodada SUBSTITUEM as da rodada anterior do mesmo fluxo.
    ///
    /// <para>Estas fontes são relidas por inteiro a cada execução. Acumular as recusas faria a fila
    /// de descarte crescer com a mesma linha a cada rodada, e a contagem de "o que falta revisar"
    /// deixaria de significar alguma coisa. O histórico de quantas houve fica no ponto de
    /// sincronismo.</para>
    /// </summary>
    internal static async Task SubstituirRecusasAsync(
        CrmDbContext contexto, string fluxo, IReadOnlyList<(object Conteudo, string Motivo)> recusas, CancellationToken ct)
    {
        await contexto.MensagensDescartadas.Where(m => m.Fluxo == fluxo).ExecuteDeleteAsync(ct);

        foreach (var (conteudo, motivo) in recusas)
            contexto.MensagensDescartadas.Add(MensagemDescartada.Criar(
                fluxo, JsonSerializer.Serialize(conteudo), motivo, tentativas: 1));
    }

    internal static async Task RegistrarRodadaAsync(
        CrmDbContext contexto, int sistemaId, string fluxo, int lidos, int gravados, int recusados,
        CancellationToken ct, string? ultimoValor = null)
    {
        var agora = DateTime.UtcNow;
        var ateOnde = ultimoValor ?? agora.ToString("O", CultureInfo.InvariantCulture);

        var ponto = await contexto.PontosDeSincronismo.FirstOrDefaultAsync(p => p.Fluxo == fluxo, ct);
        if (ponto is null)
        {
            ponto = PontoDeSincronismo.Criar(sistemaId, fluxo, ateOnde);
            contexto.PontosDeSincronismo.Add(ponto);
        }

        ponto.RegistrarRodada(ateOnde, lidos, gravados, recusados, agora);
        await contexto.SaveChangesAsync(ct);
    }
}
