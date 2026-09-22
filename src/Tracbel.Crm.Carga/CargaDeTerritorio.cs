using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.Data.SqlClient;
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

    /// <summary>
    /// O fluxo da PAM. Mudou de <c>IBGE.AREA_PLANTADA</c> para <c>IBGE.PRODUCAO_AGRICOLA</c> em
    /// 20/09/2026 (issue 64), quando a carga passou a trazer as quatro medidas — o nome antigo
    /// descrevia uma coluna. O ponto de sincronismo antigo fica no banco, com a história dele.
    /// </summary>
    private const string FluxoDaProducaoAgricola = "IBGE.PRODUCAO_AGRICOLA";

    /// <summary>
    /// Quantos anos da PAM a carga traz, contando do último publicado para trás.
    ///
    /// <para><b>Por que mais de um.</b> O ano de referência ainda é decisão de negócio (D-P10, issue
    /// 63), e a planilha do comercial mistura anos. Guardar a série curta deixa a decisão em aberto
    /// sem nova carga.</para>
    ///
    /// <para><b>Por que TRÊS, e não dois [medido em 20/09/2026].</b> A conferência da issue 64 contra
    /// o protótipo só fecha com três. Os números que o documento 48, §3.8, rotula como "área plantada
    /// 2024" são, na verdade, os da PAM de **2023** — a ADR bate ao hectare (3.715.896) — e os
    /// rotulados "2025 preliminar" são os de **2024** (3.706.356 na ADR, 9.155.949 em SP, também ao
    /// hectare). Só a coluna de VALOR está com o ano certo: os R$ 50.465.781 mil da ADR e os
    /// R$ 118.021.202 mil de SP são mesmo de 2024, ao milhar. Ou seja, as colunas de área da planilha
    /// estão um ano adiantadas. Com dois anos a conferência ficaria dependendo de uma consulta
    /// avulsa; com três, quem duvidar confere no banco.</para>
    /// </summary>
    private const int AnosDaProducaoAgricola = 3;
    /// <summary>
    /// "Milho (em grão)" na classificação 782 da PAM — conferido nos metadados do IBGE em 22/09/2026.
    ///
    /// <para>É o milho INTEIRO, o mesmo que a 839 reparte em 1ª e 2ª safra. Não confundir com
    /// "Milho verde" (83394), que é outro produto.</para>
    /// </summary>
    private const int MilhoNaPam = 40122;

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

    /// <summary>
    /// SÓ A PRODUÇÃO AGRÍCOLA — a etapa 4, sozinha, sem as planilhas do comercial.
    ///
    /// <para><b>Por que existe separada</b> (issue 64). A PAM muda uma vez por ano, e é a única etapa
    /// que o servidor consegue rodar sozinho: as outras três dependem de duas planilhas que trazem
    /// nome de funcionário por município e ficam fora do repositório. Fazer a rotina anual carregar
    /// essas planilhas até o servidor para atualizar o IBGE seria levar dado pessoal onde ele não
    /// precisa estar.</para>
    ///
    /// <para>Ela conta com o catálogo de municípios já reconhecido — que é o que a
    /// <see cref="ExecutarAsync"/> faz e não muda de ano para ano. Município do IBGE que ainda não
    /// tenha linha no catálogo aparece como recusa, com o código, em vez de sumir.</para>
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    /// <returns>As contagens da etapa.</returns>
    public async Task<IReadOnlyList<(string Etapa, string Rotulo, int Valor)>> ExecutarSoAProducaoAgricolaAsync(
        CancellationToken ct)
    {
        var municipioPorCodigo = await MunicipiosPorCodigoAsync(ct);

        if (municipioPorCodigo.Count == 0)
            throw new InvalidOperationException(
                "Nenhum município tem código do IBGE neste banco. A produção agrícola não tem onde " +
                "ser gravada: rode a carga do território inteira (--somente-territorio) uma vez antes.");

        await LerEGravarProducaoAgricolaAsync(municipioPorCodigo, ct);
        return _contagens;
    }

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

        await LerEGravarProducaoAgricolaAsync(municipioPorCodigo, ct);

        return _contagens;
    }

    /// <summary>
    /// A PAM, ANO A ANO — a série longa da issue 156.
    ///
    /// <para>Cada ano é uma rodada de consultas ao SIDRA (o teto de tamanho não deixa pedir tudo de
    /// uma vez, issue 95), e o município, o total do estado e o milho por safra vêm na mesma passada.
    /// <b>Cada ano é gravado na própria transação</b>, e não todos no fim: quinze anos de 645
    /// municípios × 85 produtos não cabem na memória de uma vez, e o ano que já está gravado precisa
    /// ficar gravado quando a leitura do seguinte falhar.</para>
    ///
    /// <para><b>A rotina busca só o que falta.</b> O ano que já tem linha no banco não é relido —
    /// e ele só tem linha porque a transação DAQUELE ano terminou, de modo que "tem linha" é o mesmo
    /// que "está completo". A exceção é a janela recente, que o IBGE ainda revisa: essa volta a cada
    /// rodada. É o que faz a segunda rodada não gravar nada sem deixar de perceber uma revisão.</para>
    /// </summary>
    private async Task LerEGravarProducaoAgricolaAsync(
        IReadOnlyDictionary<int, int> municipioPorCodigo, CancellationToken ct)
    {
        // A TRAVA VEM ANTES DA LEITURA, e não antes da gravação. Ler a série no SIDRA leva minutos;
        // descobrir só no fim que outra rodada está em curso desperdiçaria a leitura inteira e diria
        // "parou" a quem já esperou. Aqui a segunda rodada para em um segundo.
        await using var trava = await TravaDeFluxo.TomarAsync(abrirContexto(), FluxoDaProducaoAgricola, ct);

        var ultimoAno = await ibge.LerUltimoAnoDaPamAsync(ct);
        var anoInicial = await AnoInicialDaSerieAsync(ultimoAno, ct);
        var (primeiroDoMilho, ultimoDoMilho) = await ibge.LerPeriodoDoMilhoPorSafraAsync(ct);
        var jaCarregados = await AnosJaCarregadosAsync(ct);

        var conta = new ContagemDaPam();
        var aLer = AnosASeremLidos(anoInicial, ultimoAno, jaCarregados);
        conta.AnosJaCompletos = ultimoAno - anoInicial + 1 - aLer.Count;

        foreach (var ano in aLer)
        {
            relatar($"Lendo a produção agrícola da PAM (SIDRA 5457) de {ano}, nos municípios de São Paulo…");
            var nosMunicipios = await ibge.LerProducaoNosMunicipiosAsync(CodigoDeSaoPaulo, ano, ct);

            relatar($"Lendo a produção agrícola de {ano} no total do estado de São Paulo…");
            var noEstado = await ibge.LerProducaoNoEstadoAsync(CodigoDeSaoPaulo, ano, ct);

            // A 839 COMEÇA EM 2003, e a PAM em 1974: fora do período dela não há o que pedir.
            IReadOnlyList<LinhaDaProducaoAgricola> milho = [];
            if (ano >= primeiroDoMilho && ano <= ultimoDoMilho)
            {
                relatar($"Lendo o milho de 1ª e 2ª safra de {ano} (SIDRA 839)…");
                milho = await ibge.LerMilhoPorSafraAsync(CodigoDeSaoPaulo, ano, ct);
            }

            await CarregarProducaoAgricolaAsync(ano, nosMunicipios, noEstado, milho, municipioPorCodigo, conta, ct);
        }

        await FecharRodadaDaProducaoAgricolaAsync(conta, anoInicial, ultimoAno, ct);
    }

    /// <summary>
    /// QUAIS ANOS ESTA RODADA LÊ — do mais recente para o mais antigo (issue 156).
    ///
    /// <para>Regra inteira, num lugar só: entra o ano que ainda não tem linha no banco, e entram
    /// sempre os <see cref="AnosDaProducaoAgricola"/> mais recentes, que o IBGE ainda revisa. É o que
    /// faz a <b>segunda rodada não gravar nada</b> sem deixar de perceber uma revisão — e o que
    /// impede a rotina anual de rebaixar quinze anos toda vez.</para>
    /// </summary>
    /// <param name="anoInicial">O primeiro ano da série pedida.</param>
    /// <param name="ultimoAno">O último ano que a PAM publica.</param>
    /// <param name="jaCarregados">Os anos que já têm linha no banco.</param>
    internal static IReadOnlyList<short> AnosASeremLidos(
        short anoInicial, short ultimoAno, IReadOnlySet<short> jaCarregados)
    {
        var anos = new List<short>();

        for (var ano = ultimoAno; ano >= anoInicial; ano--)
        {
            var naJanelaDeRevisao = ano > ultimoAno - AnosDaProducaoAgricola;
            if (naJanelaDeRevisao || !jaCarregados.Contains(ano)) anos.Add(ano);
        }

        return anos;
    }

    /// <summary>
    /// O PRIMEIRO ANO DA SÉRIE, como o parâmetro da rotina o define (issue 156).
    ///
    /// <para><b>O parâmetro só faz a série ficar mais longa.</b> A janela que o IBGE ainda revisa é
    /// lida sempre: um parâmetro que a encurtasse faria a carga deixar de perceber a revisão do ano
    /// passado — o contrário do que a issue 153 garantiu.</para>
    ///
    /// <para>Rotina sem parâmetro (ou banco sem a rotina, como num teste) fica com a janela curta, que
    /// é o comportamento anterior a esta issue.</para>
    /// </summary>
    private async Task<short> AnoInicialDaSerieAsync(short ultimoAno, CancellationToken ct)
    {
        var janelaCurta = (short)(ultimoAno - AnosDaProducaoAgricola + 1);

        await using var contexto = abrirContexto();
        var doParametro = await contexto.Rotinas.AsNoTracking()
            .Where(r => r.Codigo == RotinasDoSistema.FontesAnuais)
            .Select(r => r.AnoInicialDoHistorico)
            .FirstOrDefaultAsync(ct);

        return doParametro is { } pedido && pedido < janelaCurta ? pedido : janelaCurta;
    }

    /// <summary>
    /// Os anos que já têm linha da PAM no banco.
    ///
    /// <para>"Tem linha" é o mesmo que "está completo" porque cada ano é gravado na transação dele:
    /// um ano interrompido no meio não deixa linha nenhuma para trás.</para>
    /// </summary>
    private async Task<HashSet<short>> AnosJaCarregadosAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        var anos = await contexto.ProducoesAgricolasNosMunicipios.AsNoTracking()
            .Select(p => p.Ano).Distinct().ToListAsync(ct);

        return [.. anos];
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
    // 4. A produção agrícola (PAM): as quatro medidas, no município e no estado
    // =============================================================================================

    /// <summary>O que a rodada inteira da PAM contou, somado ano a ano (issue 156).</summary>
    internal sealed class ContagemDaPam
    {
        public int LidasNosMunicipios, LidasNoEstado, LidasDeMilho;
        public int Novas, NovasNoEstado, NovasDeMilho;
        public int Alteradas, Mantidas, Zeros, NaoDisponiveis;
        public int AnosJaCompletos, MilhoConferidoComAPam, MilhoDivergenteDaPam;
        public readonly List<(object Conteudo, string Motivo)> Recusas = [];
        public readonly List<short> AnosLidos = [];
    }

    /// <summary>UM ANO da PAM, na transação dele — município, total do estado e milho por safra.</summary>
    private async Task CarregarProducaoAgricolaAsync(
        short ano,
        IReadOnlyList<LinhaDaProducaoAgricola> nosMunicipios,
        IReadOnlyList<LinhaDaProducaoAgricola> noEstado,
        IReadOnlyList<LinhaDaProducaoAgricola> milho,
        IReadOnlyDictionary<int, int> municipioPorCodigo,
        ContagemDaPam conta,
        CancellationToken ct)
    {
        var agora = DateTime.UtcNow;

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaAsync(contexto, "IBGE", "IBGE — localidades e SIDRA", "REST público, somente leitura", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var existentes = (await contexto.ProducoesAgricolasNosMunicipios.Where(a => a.Ano == ano).ToListAsync(ct))
            .ToDictionary(a => (a.MunicipioId, a.ProdutoCodigoIbge));
        var existentesNoEstado = (await contexto.ProducoesAgricolasNosEstados.Where(a => a.Ano == ano).ToListAsync(ct))
            .ToDictionary(a => (a.EstadoCodigoIbge, a.ProdutoCodigoIbge));
        var existentesDeMilho = (await contexto.ProducoesDeMilhoPorSafra.Where(m => m.Ano == ano).ToListAsync(ct))
            .ToDictionary(m => (m.MunicipioId, m.SafraCodigoIbge));

        var novas = new List<ProducaoAgricolaNoMunicipio>();
        var novasNoEstado = new List<ProducaoAgricolaNoEstado>();
        var novasDeMilho = new List<ProducaoDeMilhoPorSafraNoMunicipio>();

        foreach (var linha in nosMunicipios)
        {
            if (!municipioPorCodigo.TryGetValue(linha.CodigoDoRecorte, out var municipioId))
            {
                conta.Recusas.Add((linha, $"O município {linha.CodigoDoRecorte} não está reconhecido no catálogo."));
                continue;
            }

            MedidasDaProducaoAgricola medidas;
            try
            {
                medidas = Medir(linha);
            }
            catch (FormatException formato)
            {
                conta.Recusas.Add((linha, formato.Message));
                continue;
            }

            // As contagens seguem a ÁREA PLANTADA, que é a medida que o mapa C usa: é ela que
            // precisa ter zero e "não disponível" separados, e é dela que se fala no relatório.
            if (medidas.AreaPlantadaHectares is null) conta.NaoDisponiveis++;
            else if (medidas.AreaPlantadaHectares == 0) conta.Zeros++;

            if (existentes.TryGetValue((municipioId, linha.ProdutoCodigo), out var existente))
            {
                if (existente.Reapurar(linha.ProdutoNome, medidas, usuarioId, agora)) conta.Alteradas++;
                else conta.Mantidas++;
            }
            else
            {
                novas.Add(ProducaoAgricolaNoMunicipio.Registrar(
                    municipioId, linha.Ano, linha.ProdutoCodigo, linha.ProdutoNome, medidas, usuarioId, agora));
            }
        }

        foreach (var linha in noEstado)
        {
            MedidasDaProducaoAgricola medidas;
            try
            {
                medidas = Medir(linha);
            }
            catch (FormatException formato)
            {
                conta.Recusas.Add((linha, formato.Message));
                continue;
            }

            if (existentesNoEstado.TryGetValue((linha.CodigoDoRecorte, linha.ProdutoCodigo), out var existente))
            {
                if (existente.Reapurar(linha.ProdutoNome, medidas, usuarioId, agora)) conta.Alteradas++;
                else conta.Mantidas++;
            }
            else
            {
                novasNoEstado.Add(ProducaoAgricolaNoEstado.Registrar(
                    linha.CodigoDoRecorte, linha.Ano, linha.ProdutoCodigo, linha.ProdutoNome, medidas, usuarioId, agora));
            }
        }

        foreach (var linha in milho)
        {
            if (!municipioPorCodigo.TryGetValue(linha.CodigoDoRecorte, out var municipioId))
            {
                conta.Recusas.Add((linha, $"O município {linha.CodigoDoRecorte} não está reconhecido no catálogo."));
                continue;
            }

            MedidasDaProducaoAgricola medidas;
            try
            {
                medidas = Medir(linha);
            }
            catch (FormatException formato)
            {
                conta.Recusas.Add((linha, formato.Message));
                continue;
            }

            if (existentesDeMilho.TryGetValue((municipioId, linha.ProdutoCodigo), out var existente))
            {
                if (existente.Reapurar(linha.ProdutoNome, medidas, usuarioId, agora)) conta.Alteradas++;
                else conta.Mantidas++;
            }
            else
            {
                novasDeMilho.Add(ProducaoDeMilhoPorSafraNoMunicipio.Registrar(
                    municipioId, linha.Ano, linha.ProdutoCodigo, linha.ProdutoNome, medidas, usuarioId, agora));
            }
        }

        ConferirOMilhoContraAPam(nosMunicipios, milho, municipioPorCodigo, conta);

        contexto.ProducoesAgricolasNosMunicipios.AddRange(novas);
        contexto.ProducoesAgricolasNosEstados.AddRange(novasNoEstado);
        contexto.ProducoesDeMilhoPorSafra.AddRange(novasDeMilho);
        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        conta.AnosLidos.Add(ano);
        conta.LidasNosMunicipios += nosMunicipios.Count;
        conta.LidasNoEstado += noEstado.Count;
        conta.LidasDeMilho += milho.Count;
        conta.Novas += novas.Count;
        conta.NovasNoEstado += novasNoEstado.Count;
        conta.NovasDeMilho += novasDeMilho.Count;
    }

    /// <summary>
    /// A CONFERÊNCIA QUE A ISSUE 156 PEDE: 1ª safra + 2ª safra = o milho da PAM, no mesmo município.
    ///
    /// <para>As duas tabelas são pesquisas diferentes do mesmo instituto, e a 839 é publicada com
    /// arredondamento próprio. Divergência de até um hectare é arredondamento; acima disso é aviso —
    /// e aviso, aqui, é <b>recusa registrada</b>, não linha descartada: o dado entra, e a conferência
    /// fica no painel de fontes para alguém olhar.</para>
    /// </summary>
    internal static void ConferirOMilhoContraAPam(
        IReadOnlyList<LinhaDaProducaoAgricola> nosMunicipios,
        IReadOnlyList<LinhaDaProducaoAgricola> milho,
        IReadOnlyDictionary<int, int> municipioPorCodigo,
        ContagemDaPam conta)
    {
        if (milho.Count == 0) return;

        var daPam = new Dictionary<int, decimal>();
        foreach (var linha in nosMunicipios.Where(l => l.ProdutoCodigo == MilhoNaPam))
        {
            var area = SaneamentoDeTerritorio.MedidaDoSidra(linha.AreaPlantadaBruta);
            if (area is not null) daPam[linha.CodigoDoRecorte] = area.Value;
        }

        foreach (var porMunicipio in milho.GroupBy(l => l.CodigoDoRecorte))
        {
            if (!municipioPorCodigo.ContainsKey(porMunicipio.Key)) continue;
            if (!daPam.TryGetValue(porMunicipio.Key, out var naPam)) continue;

            var areas = porMunicipio
                .Select(l => SaneamentoDeTerritorio.MedidaDoSidra(l.AreaPlantadaBruta))
                .Where(a => a is not null)
                .ToList();

            if (areas.Count == 0) continue;

            var somaDasSafras = areas.Sum(a => a!.Value);

            if (Math.Abs(somaDasSafras - naPam) <= 1m)
            {
                conta.MilhoConferidoComAPam++;
                continue;
            }

            conta.MilhoDivergenteDaPam++;
            conta.Recusas.Add((porMunicipio.First(),
                $"A 1ª + 2ª safra de milho somam {somaDasSafras:0.##} ha e a PAM publica {naPam:0.##} ha para o " +
                "mesmo município e ano. As duas leituras ficaram gravadas; a diferença é para conferência."));
        }
    }

    /// <summary>
    /// Fecha a rodada da PAM: as recusas de TODOS os anos de uma vez, o ponto de sincronismo e o
    /// relatório.
    ///
    /// <para>As recusas são substituídas no fim, e não ano a ano, porque <c>SubstituirRecusasAsync</c>
    /// troca a lista inteira do fluxo: chamada dentro do laço, o último ano apagaria a recusa dos
    /// anteriores.</para>
    /// </summary>
    private async Task FecharRodadaDaProducaoAgricolaAsync(
        ContagemDaPam conta, short anoInicial, short ultimoAno, CancellationToken ct)
    {
        const string etapa = "Produção agrícola (PAM/IBGE)";

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await SistemaAsync(contexto, "IBGE", "IBGE — localidades e SIDRA", "REST público, somente leitura", ct);
        contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        await SubstituirRecusasAsync(contexto, FluxoDaProducaoAgricola, conta.Recusas, ct);
        await contexto.SaveChangesAsync(ct);

        var periodo = conta.AnosLidos.Count == 0
            ? $"série {anoInicial}–{ultimoAno} já completa"
            : $"anos {string.Join(", ", conta.AnosLidos.Order())}";

        await RegistrarRodadaAsync(
            contexto, sistemaId, FluxoDaProducaoAgricola,
            conta.LidasNosMunicipios + conta.LidasNoEstado + conta.LidasDeMilho,
            conta.Novas + conta.NovasNoEstado + conta.NovasDeMilho + conta.Alteradas,
            conta.Recusas.Count, ct, periodo);
        await transacao.CommitAsync(ct);

        Contar(etapa, $"série pedida: de {anoInicial} a {ultimoAno} (anos)", ultimoAno - anoInicial + 1);
        Contar(etapa, "anos já completos no banco, não relidos", conta.AnosJaCompletos);
        Contar(etapa, "anos lidos nesta rodada", conta.AnosLidos.Count);
        Contar(etapa, $"linhas lidas nos municípios ({periodo})", conta.LidasNosMunicipios);
        Contar(etapa, "linhas lidas no total do estado", conta.LidasNoEstado);
        Contar(etapa, "linhas lidas do milho por safra (SIDRA 839)", conta.LidasDeMilho);
        Contar(etapa, "linhas novas (município)", conta.Novas);
        Contar(etapa, "linhas novas (estado)", conta.NovasNoEstado);
        Contar(etapa, "linhas novas (milho por safra)", conta.NovasDeMilho);
        Contar(etapa, "linhas reapuradas", conta.Alteradas);
        Contar(etapa, "linhas mantidas sem mudança", conta.Mantidas);
        Contar(etapa, "municípios em que 1ª + 2ª safra bate com o milho da PAM", conta.MilhoConferidoComAPam);
        Contar(etapa, "municípios em que a soma das safras diverge da PAM", conta.MilhoDivergenteDaPam);
        Contar(etapa, "área plantada zero (\"-\" no IBGE)", conta.Zeros);
        Contar(etapa, "área plantada não disponível (\"...\" ou \"X\")", conta.NaoDisponiveis);
        Contar(etapa, "linhas recusadas", conta.Recusas.Count);
    }

    /// <summary>As quatro medidas de uma linha, cada uma pelo saneamento que separa zero de ausência.</summary>
    private static MedidasDaProducaoAgricola Medir(LinhaDaProducaoAgricola linha) => new(
        SaneamentoDeTerritorio.MedidaDoSidra(linha.AreaPlantadaBruta),
        SaneamentoDeTerritorio.MedidaDoSidra(linha.AreaColhidaBruta),
        SaneamentoDeTerritorio.MedidaDoSidra(linha.QuantidadeProduzidaBruta),
        SaneamentoDeTerritorio.MedidaDoSidra(linha.ValorDaProducaoBruto));

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
