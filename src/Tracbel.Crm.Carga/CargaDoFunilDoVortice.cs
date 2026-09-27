using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Saneamento;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Carga;

/// <summary>
/// Os códigos de pendência do funil e da venda perdida — gravados em <c>integracao.RegistroDeOrigem.Motivos</c>.
/// </summary>
internal static class MotivoDePendenciaDoFunil
{
    /// <summary>A filial do processo no Vórtice não é filial ativa do CRM: o processo não entra (decisão de 27/09/2026).</summary>
    public const string FilialDoProcessoForaDoCrm = "FILIAL_DO_PROCESSO_FORA_DO_CRM";

    /// <summary>O processo está na janela, mas não tem resultado aceito — nem próprio, nem herdado.</summary>
    public const string SemResultadoAceito = "SEM_RESULTADO_ACEITO";

    /// <summary>O processo tem filho DNA: sai, e o filho herda os resultados dele.</summary>
    public const string PaiSubstituidoPeloFilhoDna = "PAI_SUBSTITUIDO_PELO_FILHO_DNA";

    /// <summary>A resposta de venda perdida é de filial que não está no CRM, ou não tem filial nenhuma para situá-la.</summary>
    public const string FilialForaDoCrm = "FILIAL_FORA_DO_CRM";

    /// <summary>A resposta de venda perdida não tem data de preenchimento.</summary>
    public const string SemDataDeRegistro = "SEM_DATA_DE_REGISTRO";

    /// <summary>
    /// Toda data que abriria o processo (inclusão, primeiro andamento, primeiro resultado) é absurda — antes de 2000 ou
    /// depois de agora + 1 dia.
    /// </summary>
    public const string AberturaComDataInvalida = "ABERTURA_COM_DATA_INVALIDA";

    /// <summary>A data de preenchimento da resposta é absurda — antes de 2000 ou depois de agora + 1 dia.</summary>
    public const string DataDeRegistroInvalida = "DATA_DE_REGISTRO_INVALIDA";
}

/// <summary>O que a rotina do funil fez (ou faria, na simulação), em número — sem nome nem documento.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDoFunilDoVortice(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// O FUNIL E AS VENDAS PERDIDAS DO VÓRTICE — a rotina <c>PROCESSOS_VORTICE</c> (decisões de 27/09/2026, documento 52).
///
/// <para><b>O que faz.</b> Lê os processos 31/41/50 e o histórico deles, aplica a <see cref="RegraDoEstagio"/> e
/// mantém <c>processo.EstagioDoProcesso</c> — uma linha por processo por estágio, do prospect também. Lê os doze
/// formulários de venda perdida, decide o papel de cada resposta (<see cref="RegraDoPapelDaVendaPerdida"/>) e mantém
/// <c>processo.VendaPerdida</c>. O que não entra fica em <c>integracao.RegistroDeOrigem</c>, com o motivo.</para>
///
/// <para><b>O que NÃO faz, e é o que resolve a D-12 para este histórico</b> (errata de 27/09/2026 no documento 41): não
/// cria cliente, carteira nem usuário — liga ao que o CRM já tem, pelo documento, pelo de-para da carteira e pelo
/// login; não toca o faturamento; e não grava o último contato, que é da <c>CARTEIRAS_VORTICE</c>.</para>
///
/// <para><b>É SINCRONIA, relida inteira a cada rodada</b>, sem marca d'água: o funil custa 2–3 s e os formulários 1 s
/// (medido em 27/09/2026). Linha nova entra; linha que mudou na origem é atualizada; estágio que sumiu é removido —
/// e a trilha de auditoria guarda o que era. Rodar de novo sem mudança na origem não altera nada.</para>
///
/// <para><b>Planejar, depois gravar.</b> Tudo é calculado primeiro, SÓ COM LEITURA; a simulação para aí. A gravação é
/// em blocos (a primeira rodada são ~113 mil linhas), cada bloco na sua transação: uma rodada que cai no meio deixa o
/// funil a meio caminho, e a próxima converge.</para>
///
/// <para><b>A trava.</b> Leitura vazia, ou queda de mais de 5% no que o CRM já tem, ABORTA SEM APAGAR: é uma leitura a
/// conferir, e o custo de errar para esse lado é o funil inteiro.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="lerFunil">A leitura do funil, com os códigos de resultado da classificação.</param>
/// <param name="lerVendasPerdidas">A leitura dos formulários de venda perdida.</param>
/// <param name="deParaDeFiliais">A filial do Vórtice (<c>NN</c>) para a filial ativa do CRM (<c>0101NN</c>).</param>
/// <param name="usuarioId">Quem roda a rotina.</param>
/// <param name="relogio">O relógio (UTC).</param>
/// <param name="relatar">Onde a rotina escreve o andamento.</param>
/// <param name="tamanhoDoBloco">Linhas por bloco de gravação — o teste usa um pequeno para exercitar a queda no meio.</param>
internal sealed class CargaDoFunilDoVortice(
    Func<CrmDbContext> abrirContexto,
    Func<IReadOnlyCollection<int>, CancellationToken, Task<Resultado<LeituraDoFunilDoVortice>>> lerFunil,
    Func<CancellationToken, Task<Resultado<IReadOnlyList<RespostaDeVendaPerdidaNoVortice>>>> lerVendasPerdidas,
    IReadOnlyDictionary<int, int> deParaDeFiliais,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar,
    int tamanhoDoBloco = CargaDoFunilDoVortice.TamanhoDoBloco)
{
    /// <summary>O fluxo dos processos na trilha da origem — e o nome da trava da rotina.</summary>
    internal const string Fluxo = "VORTICE.FUNIL";

    /// <summary>O fluxo das respostas de venda perdida na trilha da origem.</summary>
    internal const string FluxoDaVendaPerdida = "VORTICE.VENDA_PERDIDA";

    /// <summary>
    /// A QUEDA QUE A ROTINA ACEITA sem parar: 5% do que o CRM já tem. Mais do que isso numa rodada não é o funil
    /// andando — é leitura parcial, e apagar em cima dela seria perder o funil.
    /// </summary>
    internal const double QuedaMaximaAceita = 0.05;

    /// <summary>
    /// Linhas por bloco de gravação — cada bloco, uma transação. O bloco fecha em processos inteiros: pode passar deste
    /// número pelos estágios do último processo (no máximo seis).
    /// </summary>
    internal const int TamanhoDoBloco = 2_000;

    /// <summary>
    /// QUANTOS IDENTIFICADORES VÃO NUMA CONSULTA POR LISTA. O SQL Server aceita 2.100 parâmetros por comando, e a lista
    /// de vendas perdidas do Vórtice já passa de 3 mil: a consulta por lista é feita em fatias, e nunca chega perto do
    /// limite, qualquer que seja a tradução que o EF escolha para o <c>Contains</c>.
    /// </summary>
    internal const int IdsPorConsulta = 1_000;

    /// <summary>A opção do terminal que passa por cima da trava de queda — nunca da rotina do orquestrador.</summary>
    internal const string OpcaoDeAceitarQueda = "--aceitar-queda";

    /// <summary>O motivo de perda que substitui o que a origem não registrou — o mesmo código da carga antiga.</summary>
    internal const string MotivoNaoInformado = "NAO_INFORMADO_NA_ORIGEM";

    /// <summary>Rótulo: processos lidos.</summary>
    internal const string RotuloDeProcessosLidos = "processos 31/41/50 lidos (todos, sem janela)";

    /// <summary>Rótulo: processos no funil.</summary>
    internal const string RotuloDeProcessosNoFunil = "processos no funil (ao menos um estágio)";

    /// <summary>Rótulo: processos pendentes.</summary>
    internal const string RotuloDeProcessosPendentes = "processos da janela pendentes (ao menos um motivo)";

    /// <summary>Rótulo: linhas incluídas.</summary>
    internal const string RotuloDeLinhasIncluidas = "linhas do funil incluídas";

    /// <summary>Rótulo: linhas alteradas.</summary>
    internal const string RotuloDeLinhasAlteradas = "linhas do funil alteradas pela origem";

    /// <summary>Rótulo: linhas removidas.</summary>
    internal const string RotuloDeLinhasRemovidas = "linhas do funil removidas (o estágio sumiu da origem)";

    /// <summary>Rótulo: linhas mantidas.</summary>
    internal const string RotuloDeLinhasMantidas = "linhas do funil mantidas sem alteração";

    /// <summary>Rótulo: respostas de venda perdida que entram.</summary>
    internal const string RotuloDeRespostasQueEntram = "respostas de venda perdida que entram";

    /// <summary>Rótulo: respostas pendentes.</summary>
    internal const string RotuloDeRespostasPendentes = "respostas de venda perdida pendentes";

    /// <summary>Rótulo: vendas perdidas incluídas.</summary>
    internal const string RotuloDeVendasPerdidasIncluidas = "vendas perdidas incluídas";

    /// <summary>Rótulo: vendas perdidas alteradas.</summary>
    internal const string RotuloDeVendasPerdidasAlteradas = "vendas perdidas alteradas pela origem";

    /// <summary>Rótulo: papéis alterados.</summary>
    internal const string RotuloDePapeisAlterados = "vendas perdidas com o papel alterado";

    /// <summary>Rótulo: vendas perdidas excluídas.</summary>
    internal const string RotuloDeVendasPerdidasExcluidas = "vendas perdidas excluídas (sumiram da origem ou ficaram pendentes)";

    /// <summary>Rótulo: motivos novos.</summary>
    internal const string RotuloDeMotivosIncluidos = "motivos de perda incluídos no catálogo";

    private const string EntidadeCarteira = nameof(Carteira);
    private const string EntidadeVendaPerdida = nameof(VendaPerdida);

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>Executa a rotina.</summary>
    /// <param name="simular">Só planeja e conta: não abre transação de escrita.</param>
    /// <param name="aceitarQueda">
    /// Passa por cima da trava de queda — só pelo terminal (<see cref="OpcaoDeAceitarQueda"/>), nunca pela rotina: a
    /// queda legítima (uma filial desativada tira ~7,7% do funil) não pode travar a rotina para sempre, e a queda por
    /// leitura parcial não pode passar sozinha. Quando aceita, fica escrita na execução e nas observações.
    /// </param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDoFunilDoVortice>> ExecutarAsync(bool simular, bool aceitarQueda, CancellationToken ct)
    {
        Dictionary<int, EstagioDoFunil> estagioPorResultado;
        await using (var banco = abrirContexto())
        {
            estagioPorResultado = await banco.ClassificacoesDeResultadoDoVortice.AsNoTracking()
                .Where(c => c.Estagio != null)
                .ToDictionaryAsync(c => c.CodigoNaOrigem, c => c.Estagio!.Value, ct);
        }

        if (estagioPorResultado.Count == 0)
            return Falha("A classificação dos resultados do Vórtice está vazia no CRM — sem ela nenhum estágio existe. " +
                         "Nada foi lido e nada foi gravado.");

        relatar($"Lendo o funil do Vórtice (processos 31/41/50 e {estagioPorResultado.Count} códigos de resultado, sessão somente leitura)…");
        var funil = await lerFunil(estagioPorResultado.Keys.ToList(), ct);
        if (!funil.EhSucesso) return Falha(funil.Erro!);

        relatar("Lendo os formulários de venda perdida do Vórtice (sessão somente leitura)…");
        var formularios = await lerVendasPerdidas(ct);
        if (!formularios.EhSucesso) return Falha(formularios.Erro!);

        // A ORIGEM VAZIA NÃO É UM FUNIL VAZIO: é uma leitura que falhou sem dizer. Sincronizar contra ela apagaria o
        // funil inteiro de uma vez.
        if (funil.Valor.Processos.Count == 0 || funil.Valor.Historico.Count == 0 || formularios.Valor.Count == 0)
            return Falha(
                $"O Vórtice devolveu {funil.Valor.Processos.Count} processo(s), {funil.Valor.Historico.Count} linha(s) de histórico e " +
                $"{formularios.Valor.Count} resposta(s) de venda perdida. Leitura vazia não é funil vazio, é leitura a conferir — " +
                "nada foi removido e nada foi gravado.");

        var agora = relogio();
        Plano plano;
        await using (var leituraDoCrm = abrirContexto())
        {
            plano = await PlanejarAsync(leituraDoCrm, funil.Valor, formularios.Valor, estagioPorResultado, agora, ct);
        }

        Relatar(funil.Valor, formularios.Valor, plano);

        var quedas = new List<string>();
        if (Queda(plano.LinhasExistentes, plano.Desejadas.Count) is { } quedaDoFunil)
            quedas.Add($"o funil cairia de {plano.LinhasExistentes:N0} para {plano.Desejadas.Count:N0} linhas ({quedaDoFunil:P1})");
        if (Queda(plano.VendasPerdidasAtivas, plano.Respostas.Count(r => r.Motivo is null)) is { } quedaDasPerdas)
            quedas.Add($"as vendas perdidas cairiam de {plano.VendasPerdidasAtivas:N0} para {plano.Respostas.Count(r => r.Motivo is null):N0} " +
                       $"({quedaDasPerdas:P1})");

        if (quedas.Count > 0)
        {
            var descricao = string.Join("; ", quedas);
            if (!aceitarQueda)
                return Falha(
                    $"{char.ToUpperInvariant(descricao[0])}{descricao[1..]} — mais do que os {QuedaMaximaAceita:P0} que uma rodada aceita. " +
                    "Isso costuma ser leitura a conferir, não o funil andando: nada foi removido e nada foi gravado. Se a queda é " +
                    $"legítima (uma filial desativada, por exemplo), rode no terminal com {OpcaoDeAceitarQueda}.");

            plano.QuedaAceita = $"Queda aceita por {OpcaoDeAceitarQueda}, no terminal: {descricao}.";
            _observacoes.Add(plano.QuedaAceita);
            relatar("  " + plano.QuedaAceita);
        }

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura, e nenhuma transação de escrita foi aberta.");
            return Resultado<RelatorioDoFunilDoVortice>.Ok(new RelatorioDoFunilDoVortice(true, _contagens, _observacoes));
        }

        var gravacao = await AplicarAsync(plano, agora, ct);
        return gravacao.EhSucesso
            ? Resultado<RelatorioDoFunilDoVortice>.Ok(new RelatorioDoFunilDoVortice(false, _contagens, _observacoes))
            : Falha(gravacao.Erro!);
    }

    private static Resultado<RelatorioDoFunilDoVortice> Falha(string mensagem) => Resultado<RelatorioDoFunilDoVortice>.Indisponivel(mensagem);

    /// <summary>A data crível: de 2000 até agora + 1 dia — a mesma faixa que a regra do estágio aceita na abertura.</summary>
    private static bool DataCrivel(DateTime utc, DateTime agora) =>
        utc >= RegraDoEstagio.MenorAberturaCrivel && utc <= agora + RegraDoEstagio.ToleranciaDeDataFutura;

    /// <summary>A queda relativa, quando passa do aceito; nula quando não passa (ou quando não havia nada antes).</summary>
    private static double? Queda(int antes, int depois)
    {
        if (antes == 0 || depois >= antes) return null;
        var queda = (antes - depois) / (double)antes;
        return queda > QuedaMaximaAceita ? queda : null;
    }

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private async Task<Plano> PlanejarAsync(
        CrmDbContext banco, LeituraDoFunilDoVortice funil, IReadOnlyList<RespostaDeVendaPerdidaNoVortice> respostas,
        IReadOnlyDictionary<int, EstagioDoFunil> estagioPorResultado, DateTime agora, CancellationToken ct)
    {
        var plano = new Plano();
        plano.SistemaId = await banco.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == LeitorDeCargaDoVortice.CodigoDoSistema)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(ct);

        var ligacoes = await LigacoesAsync(banco, plano.SistemaId, ct);

        // ---------------------------------------------------------------------------------------------
        // O funil: a regra, e o que o CRM já tem.
        // ---------------------------------------------------------------------------------------------
        var processos = funil.Processos.GroupBy(p => p.Numero).ToDictionary(g => g.Key, g => g.First());
        plano.Apuracoes = RegraDoEstagio.Apurar(
            processos.Values.Select(p => p.NaRegra), funil.Historico.Select(h => h.NaRegra), estagioPorResultado, agora);

        foreach (var apuracao in plano.Apuracoes)
        {
            if (apuracao.Situacao is SituacaoNaRegraDoEstagio.ForaDaJanela or SituacaoNaRegraDoEstagio.SemAbertura) continue;

            var processo = processos[apuracao.Numero];
            var motivo = processo.NroEmpresa is not { } nn || !deParaDeFiliais.ContainsKey(nn)
                ? MotivoDePendenciaDoFunil.FilialDoProcessoForaDoCrm
                : apuracao.Situacao switch
                {
                    SituacaoNaRegraDoEstagio.PaiSubstituidoPeloFilhoDna => MotivoDePendenciaDoFunil.PaiSubstituidoPeloFilhoDna,
                    SituacaoNaRegraDoEstagio.SemResultadoAceito => MotivoDePendenciaDoFunil.SemResultadoAceito,
                    SituacaoNaRegraDoEstagio.AberturaInvalida => MotivoDePendenciaDoFunil.AberturaComDataInvalida,
                    _ => null
                };

            if (motivo is null)
            {
                var ligacao = ligacoes.Do(processo);
                plano.Ligacoes.Add(ligacao);
                var situacao = SaneamentoDeProcesso.Situacao(processo.Status, []).Situacao;

                // O DESFECHO SÓ TEM DATA quando encerra e quando a data é crível — a 31/12/9999 do Vórtice não é data.
                var desfechoEm = situacao is SituacaoDoProcesso.Ganho or SituacaoDoProcesso.Perdido or SituacaoDoProcesso.Cancelado
                                 && processo.RealizadoEmUtc is { } realizado && DataCrivel(realizado, agora)
                    ? processo.RealizadoEmUtc
                    : null;

                foreach (var estagio in apuracao.Estagios)
                {
                    plano.Desejadas[(processo.Numero, estagio.Estagio)] = EstagioDoProcesso.Normalizar(new RetratoDoEstagio(
                        deParaDeFiliais[processo.NroEmpresa!.Value], processo.Numero, processo.Tipo, ligacao.ClienteId, ligacao.CarteiraId,
                        ligacao.ResponsavelId, apuracao.AbertoEm!.Value, apuracao.AberturaDeduzida, situacao, desfechoEm, estagio.Estagio,
                        estagio.AlcancadoEm, estagio.UltimaAcaoDaEtapaEm, estagio.NumeroDoProcessoDna, estagio.ResultadoQueAbriu,
                        estagio.PelaEntradaDigital));
                }
            }

            plano.RegistrosDoFunil.Add(new RegistroPlanejado(
                apuracao.Numero.ToString(CultureInfo.InvariantCulture),
                RetratoDoProcesso(processo, apuracao),
                motivo is null ? DecisaoDaIntegracao.Importado : DecisaoDaIntegracao.Pendente,
                motivo));
        }

        foreach (var linha in await banco.EstagiosDoProcesso.AsNoTracking().ToListAsync(ct))
        {
            plano.LinhasExistentes++;
            var chave = (linha.NumeroDoProcessoNaOrigem, linha.Estagio);
            if (!plano.Desejadas.TryGetValue(chave, out var desejada))
            {
                plano.ARemover.Add((linha.Id, linha.NumeroDoProcessoNaOrigem));
                if (processos.ContainsKey(linha.NumeroDoProcessoNaOrigem)) plano.RemovidasDeProcessoPresente++;
                continue;
            }

            plano.Existentes.Add(chave);
            if (linha.Retrato == desejada) plano.Mantidas++;
            else plano.AAlterar.Add((linha.Id, desejada));
        }

        plano.AIncluir.AddRange(plano.Desejadas.Where(d => !plano.Existentes.Contains(d.Key)).Select(d => d.Value));

        // ---------------------------------------------------------------------------------------------
        // A venda perdida.
        // ---------------------------------------------------------------------------------------------
        await PlanejarVendasPerdidasAsync(banco, plano, ligacoes, respostas, agora, ct);

        // ---------------------------------------------------------------------------------------------
        // A trilha da origem, planejada.
        // ---------------------------------------------------------------------------------------------
        if (plano.SistemaId is { } sistemaId)
        {
            plano.TrilhaDoFunil = await PlanejarTrilhaAsync(banco, sistemaId, Fluxo, plano.RegistrosDoFunil, ct);
            plano.TrilhaDaVendaPerdida = await PlanejarTrilhaAsync(banco, sistemaId, FluxoDaVendaPerdida, plano.RegistrosDaVendaPerdida, ct);
        }
        else
        {
            // SEM O SISTEMA NO CRM, nada foi lido antes: tudo é registro novo.
            plano.TrilhaDoFunil = new PlanoDaTrilha(plano.RegistrosDoFunil.Select(r => r.Chave).ToHashSet(StringComparer.Ordinal))
            {
                Novos = plano.RegistrosDoFunil.Count
            };
            plano.TrilhaDaVendaPerdida = new PlanoDaTrilha(plano.RegistrosDaVendaPerdida.Select(r => r.Chave).ToHashSet(StringComparer.Ordinal))
            {
                Novos = plano.Respostas.Count
            };
        }

        return plano;
    }

    private async Task PlanejarVendasPerdidasAsync(
        CrmDbContext banco, Plano plano, Ligacoes ligacoes, IReadOnlyList<RespostaDeVendaPerdidaNoVortice> respostas, DateTime agora,
        CancellationToken ct)
    {
        var motivos = await banco.MotivosDePerda.AsNoTracking().ToDictionaryAsync(m => m.Codigo, m => m.Id, StringComparer.Ordinal, ct);
        var itens = (await banco.CatalogoItens.AsNoTracking()
                .Where(i => i.CatalogoId == CatalogosDeSistema.Concorrente || i.CatalogoId == CatalogosDeSistema.TipoDeEquipamento
                                                                           || i.CatalogoId == CatalogosDeSistema.RevendaConcorrente)
                .Select(i => new { i.CatalogoId, i.Codigo, i.Id })
                .ToListAsync(ct))
            .ToDictionary(i => (i.CatalogoId, i.Codigo), i => i.Id);

        foreach (var resposta in respostas.GroupBy(r => r.Questionario).Select(g => g.First()))
        {
            var correcoes = new List<CorrecaoAplicada>();
            var item = new PlanoDaResposta { Origem = resposta, Correcoes = correcoes };
            plano.Respostas.Add(item);

            if (resposta.RegistradaEmUtc is not { } registrada)
            {
                item.Motivo = MotivoDePendenciaDoFunil.SemDataDeRegistro;
                continue;
            }

            if (!DataCrivel(registrada, agora))
            {
                item.Motivo = MotivoDePendenciaDoFunil.DataDeRegistroInvalida;
                continue;
            }

            // A FILIAL: a do processo; sem processo, a do histórico em que o formulário foi preenchido. Filial de fora do
            // CRM não entra (decisão de 27/09/2026). Sem nenhuma das duas, a do cliente do CRM, se houver.
            var clienteId = ligacoes.Cliente(resposta.Documento);
            int? empresaId = (resposta.EmpresaDoProcesso ?? resposta.EmpresaDoHistorico) is { } nn
                ? deParaDeFiliais.TryGetValue(nn, out var daFilial) ? daFilial : null
                : clienteId is { } dono && ligacoes.EmpresaDoCliente.TryGetValue(dono, out var doCliente) ? doCliente : null;

            if (empresaId is null)
            {
                item.Motivo = MotivoDePendenciaDoFunil.FilialForaDoCrm;
                continue;
            }

            item.EmpresaId = empresaId.Value;
            item.ClienteId = clienteId;
            item.Motivo = null;

            var motivo = Codificar(resposta.Motivo);
            item.MotivoCodigo = motivo.Length == 0 ? MotivoNaoInformado : motivo;
            item.MotivoNome = motivo.Length == 0 ? "Não informado na origem" : resposta.Motivo!;
            item.Tipo = (Codificar(resposta.TipoDoEquipamento), resposta.TipoDoEquipamento);
            item.Marca = (Codificar(resposta.Marca), resposta.Marca);
            item.Revenda = (Codificar(resposta.Revenda), resposta.Revenda);

            var registradaLocal = resposta.RegistradaEmUtc.Value.AddHours(-3);
            item.OcorridaEm = SaneamentoDeVendaPerdida.DataDaPerda(resposta.OcorridaEm, registradaLocal, correcoes);
            item.Quantidade = SaneamentoDeVendaPerdida.Quantidade(resposta.Quantidade, correcoes);
            item.PrecoDoConcorrente = SaneamentoDeVendaPerdida.Preco("precoDoConcorrente", resposta.PrecoDoConcorrente, correcoes);
            item.PrecoOfertado = SaneamentoDeVendaPerdida.Preco("precoOfertado", resposta.PrecoOfertado, correcoes);
            item.Participacao = SaneamentoDeVendaPerdida.SimNaoOuNada(resposta.Participamos);
            item.ModeloDoConcorrente = Limitar(resposta.ModeloDoConcorrente, 120);
            item.ModeloOfertado = Limitar(resposta.ModeloOfertado, 120);

            if (!motivos.ContainsKey(item.MotivoCodigo)) plano.MotivosNovos.TryAdd(item.MotivoCodigo, Limitar(item.MotivoNome, 120)!);
            Catalogar(plano, itens, CatalogosDeSistema.TipoDeEquipamento, item.Tipo);
            Catalogar(plano, itens, CatalogosDeSistema.Concorrente, item.Marca);
            Catalogar(plano, itens, CatalogosDeSistema.RevendaConcorrente, item.Revenda);
        }

        // O PAPEL, decidido só entre as respostas que entram: a duplicata cuja principal ficou pendente vira a principal.
        var queEntram = plano.Respostas.Where(r => r.Motivo is null).ToList();
        var papeis = RegraDoPapelDaVendaPerdida.Decidir(queEntram
            .Select(r => new RespostaNaRegraDoPapel(r.Origem.Questionario, r.Origem.Formulario, r.Origem.SeqPessoa ?? 0, r.Origem.Processo,
                r.Origem.RegistradaEmUtc!.Value))
            .ToList());
        foreach (var resposta in queEntram)
        {
            resposta.Papel = papeis[resposta.Origem.Questionario];
            if (resposta.Papel.ParticipacaoNegada) resposta.Participacao = ParticipacaoNaNegociacao.Nao;
        }

        // O QUE O CRM JÁ TEM desta rotina: as vendas perdidas com o de-para do Vórtice.
        if (plano.SistemaId is not { } sistemaId) return;

        var chaves = await banco.ChavesExternas.AsNoTracking()
            .Where(c => c.SistemaId == sistemaId && c.Entidade == EntidadeVendaPerdida)
            .Select(c => new { c.ChaveOrigem, c.RegistroId })
            .ToListAsync(ct);
        var existentes = new Dictionary<long, VendaPerdida>();
        foreach (var fatia in chaves.Select(c => c.RegistroId).Distinct().Chunk(IdsPorConsulta))
        {
            var ids = fatia.ToList();
            foreach (var venda in await banco.VendasPerdidas.AsNoTracking().Where(v => ids.Contains(v.Id)).ToListAsync(ct))
                existentes[venda.Id] = venda;
        }
        var porChave = chaves.Where(c => existentes.ContainsKey(c.RegistroId))
            .GroupBy(c => c.ChaveOrigem, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => existentes[g.First().RegistroId], StringComparer.Ordinal);

        plano.VendasPerdidasAtivas = porChave.Values.Count(v => !v.EstaExcluido);
        var idPorChave = porChave.ToDictionary(p => p.Key, p => p.Value.Id, StringComparer.Ordinal);

        foreach (var resposta in queEntram)
        {
            if (!porChave.TryGetValue(Chave(resposta.Origem), out var existente)) continue;

            resposta.VendaPerdidaId = existente.Id;
            if (existente.Conteudo != VendaPerdida.Normalizar(Conteudo(resposta, motivos, itens))) plano.VendasPerdidasAlteradas++;
            if (existente.EstaExcluido) plano.VendasPerdidasRestauradas++;

            long? principal = resposta.Papel!.ChaveDaPrincipal is { } chaveDaPrincipal
                ? idPorChave.GetValueOrDefault(chaveDaPrincipal.ToString(CultureInfo.InvariantCulture))
                : null;
            if (existente.Papel != resposta.Papel.Papel || existente.VendaPerdidaPrincipalId != principal) plano.PapeisAlterados++;
        }

        var entram = queEntram.Select(r => Chave(r.Origem)).ToHashSet(StringComparer.Ordinal);
        plano.AExcluir.AddRange(porChave.Where(p => !entram.Contains(p.Key) && !p.Value.EstaExcluido).Select(p => p.Value.Id));
    }

    private static void Catalogar(Plano plano, IReadOnlyDictionary<(int, string), int> itens, int catalogo, (string Codigo, string? Nome) valor)
    {
        if (valor.Codigo.Length == 0 || itens.ContainsKey((catalogo, valor.Codigo))) return;
        plano.ItensNovos.TryAdd((catalogo, valor.Codigo), Limitar(valor.Nome!.Trim(), 200)!);
    }

    private async Task<PlanoDaTrilha> PlanejarTrilhaAsync(
        CrmDbContext banco, int sistemaId, string fluxo, IReadOnlyList<RegistroPlanejado> planejados, CancellationToken ct)
    {
        var existentes = await banco.RegistrosDeOrigem.AsNoTracking()
            .Where(r => r.SistemaId == sistemaId && r.Fluxo == fluxo)
            .Select(r => new { r.ChaveOrigem, r.HashDoConteudo, r.Decisao, r.Motivos, r.AusenteNaOrigemDesde })
            .ToDictionaryAsync(r => r.ChaveOrigem, StringComparer.Ordinal, ct);

        var trilha = new PlanoDaTrilha(planejados.Select(r => r.Chave).ToHashSet(StringComparer.Ordinal));
        foreach (var registro in planejados)
        {
            if (!existentes.TryGetValue(registro.Chave, out var existente)) { trilha.Novos++; continue; }

            if (existente.HashDoConteudo == registro.Retrato.Hash && existente.Decisao == registro.Decisao
                && existente.Motivos == registro.Motivos && existente.AusenteNaOrigemDesde is null)
            {
                trilha.Iguais.Add(registro.Chave);
                continue;
            }

            if (existente.HashDoConteudo != registro.Retrato.Hash) trilha.Alterados++;
            if (existente.Decisao != registro.Decisao || existente.Motivos != registro.Motivos) trilha.ComDecisaoAlterada++;
        }

        trilha.QueSumiram.AddRange(existentes
            .Where(e => !trilha.Planejados.Contains(e.Key) && e.Value.AusenteNaOrigemDesde is null)
            .Select(e => e.Key));
        return trilha;
    }

    /// <summary>
    /// O QUE O CRM JÁ TEM PARA LIGAR — lido uma vez: o cliente pelo documento, a carteira pelo de-para da sincronia das
    /// carteiras e a conta pelo login. Nada disso é criado aqui.
    /// </summary>
    private static async Task<Ligacoes> LigacoesAsync(CrmDbContext banco, int? sistemaId, CancellationToken ct)
    {
        var clientes = await banco.Clientes.AsNoTracking()
            .Where(c => c.Documento != null)
            .Select(c => new { c.Id, c.Documento, c.EmpresaId, Excluido = c.ExcluidoEm != null })
            .ToListAsync(ct);
        var clientePorDocumento = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var grupo in clientes.Where(c => !c.Excluido).GroupBy(c => c.Documento!.Value.Numero, StringComparer.Ordinal))
            if (grupo.Count() == 1) clientePorDocumento[grupo.Key] = grupo.First().Id;

        var carteiras = await banco.Carteiras.AsNoTracking()
            .Where(k => k.ExcluidoEm == null)
            .Select(k => new { k.Id, k.ResponsavelId })
            .ToDictionaryAsync(k => k.Id, k => k.ResponsavelId, ct);

        var carteiraPorSeq = new Dictionary<int, long>();
        if (sistemaId is { } sistema)
        {
            foreach (var chave in await banco.ChavesExternas.AsNoTracking()
                         .Where(c => c.SistemaId == sistema && c.Entidade == EntidadeCarteira)
                         .Select(c => new { c.ChaveOrigem, c.RegistroId })
                         .ToListAsync(ct))
            {
                if (int.TryParse(chave.ChaveOrigem, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seq)
                    && carteiras.ContainsKey(chave.RegistroId))
                    carteiraPorSeq[seq] = chave.RegistroId;
            }
        }

        var contas = await banco.Usuarios.AsNoTracking()
            .Where(u => u.ExcluidoEm == null)
            .Select(u => new { u.Id, u.NomePrincipal })
            .ToListAsync(ct);
        var contaPorLogin = contas
            .GroupBy(u => ParteLocal(u.NomePrincipal), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        return new Ligacoes(
            clientePorDocumento, clientes.Where(c => !c.Excluido).GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First().EmpresaId),
            carteiraPorSeq, carteiras, contaPorLogin);
    }

    // =============================================================================================
    // A gravação — em blocos
    // =============================================================================================

    private async Task<Resultado<bool>> AplicarAsync(Plano plano, DateTime agora, CancellationToken ct)
    {
        int sistemaId;
        await using (var banco = abrirContexto())
        {
            sistemaId = await CargaDeTerritorio.SistemaAsync(
                banco, LeitorDeCargaDoVortice.CodigoDoSistema, "Vórtice CRM (sistema legado)", "SQL Server, somente leitura", ct);
        }

        CrmDbContext Abrir()
        {
            var banco = abrirContexto();

            // O QUE A ROTINA MUDA num registro que já existia vai para a trilha como integração do Vórtice; o que nasce
            // agora tem o rastro em integracao.RegistroDeOrigem.
            banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);
            return banco;
        }

        // A EXECUÇÃO, REGISTRADA: é o que a administração consulta por fluxo — quando rodou, o que fez, se a queda foi
        // aceita pelo terminal e, se caiu no meio, até onde gravou.
        long execucaoId;
        await using (var banco = Abrir())
        {
            var execucao = ExecucaoDeSincronizacao.Iniciar(sistemaId, Fluxo, Environment.MachineName, relogio());
            banco.ExecucoesDeSincronizacao.Add(execucao);
            await banco.SaveChangesAsync(ct);
            execucaoId = execucao.Id;
        }

        // 1. O funil, EM BLOCOS DE PROCESSOS INTEIROS: inclusão, alteração e remoção de um processo andam no mesmo bloco,
        // e o bloco é uma transação. Se um cair, nenhum processo fica com parte dos estágios de uma rodada e parte da outra.
        var blocos = BlocosPorProcesso(plano, tamanhoDoBloco);
        var gravados = 0;
        try
        {
            foreach (var bloco in blocos)
            {
                await using var banco = Abrir();
                banco.EstagiosDoProcesso.AddRange(bloco.Incluir.Select(EstagioDoProcesso.Registrar));

                var linhas = new Dictionary<long, EstagioDoProcesso>();
                foreach (var fatia in bloco.Alterar.Select(a => a.Id).Concat(bloco.Remover).Chunk(IdsPorConsulta))
                {
                    var ids = fatia.ToList();
                    foreach (var linha in await banco.EstagiosDoProcesso.Where(e => ids.Contains(e.Id)).ToListAsync(ct))
                        linhas[linha.Id] = linha;
                }

                foreach (var (id, retrato) in bloco.Alterar) linhas[id].AtualizarDaOrigem(retrato);

                // A REMOÇÃO PASSA PELO RASTREADOR, e não por um DELETE em massa, de propósito: é assim que a trilha de
                // auditoria guarda qual processo saiu de qual estágio, e desde quando ele estava lá.
                banco.EstagiosDoProcesso.RemoveRange(bloco.Remover.Select(id => linhas[id]));

                await banco.SaveChangesAsync(ct);
                gravados++;
            }

            // 2. A venda perdida: pequena (~3,2 mil respostas), numa transação só.
            await AplicarVendasPerdidasAsync(Abrir, plano, sistemaId, agora, ct);

            // 3. A trilha da origem.
            await AplicarTrilhaAsync(Abrir, sistemaId, Fluxo, plano.RegistrosDoFunil, plano.TrilhaDoFunil, agora, ct);
            await AplicarTrilhaAsync(Abrir, sistemaId, FluxoDaVendaPerdida, plano.RegistrosDaVendaPerdida, plano.TrilhaDaVendaPerdida, agora, ct);
        }
        catch (Exception falha) when (falha is DbUpdateException or InvalidOperationException or RegraDeNegocioViolada)
        {
            var onde = gravados < blocos.Count
                ? $"{gravados} de {blocos.Count} blocos do funil gravados; o bloco {gravados + 1} foi desfeito inteiro, sem deixar " +
                  "processo com parte dos estágios"
                : $"os {blocos.Count} blocos do funil gravados; a venda perdida ou a trilha da origem parou no meio";
            var mensagem = $"Gravação parcial: {onde}. A próxima rodada completa — ela relê tudo e grava só o que falta. " +
                           $"Causa: {falha.GetBaseException().Message}";

            await EncerrarAsync(Abrir, execucaoId, e => e.Falhar(1, mensagem, relogio()), ct);
            return Resultado<bool>.Indisponivel(mensagem);
        }

        var resumo =
            $"Funil: {plano.AIncluir.Count:N0} incluídas, {plano.AAlterar.Count:N0} alteradas, {plano.ARemover.Count:N0} removidas. " +
            $"Vendas perdidas: {plano.Respostas.Count(r => r.Motivo is null && r.VendaPerdidaId is null):N0} incluídas, " +
            $"{plano.VendasPerdidasAlteradas:N0} alteradas, {plano.AExcluir.Count:N0} excluídas." +
            (plano.QuedaAceita is null ? string.Empty : " " + plano.QuedaAceita);

        await EncerrarAsync(Abrir, execucaoId, e => e.Concluir(
            1,
            plano.RegistrosDoFunil.Count + plano.Respostas.Count,
            plano.AIncluir.Count + plano.Respostas.Count(r => r.Motivo is null && r.VendaPerdidaId is null),
            plano.AAlterar.Count + plano.ARemover.Count + plano.VendasPerdidasAlteradas + plano.PapeisAlterados + plano.AExcluir.Count,
            plano.RegistrosDoFunil.Count(r => r.Motivos is not null) + plano.Respostas.Count(r => r.Motivo is not null),
            resumo,
            relogio()), ct);

        relatar("Gravado. A rotina pode rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
        return Resultado<bool>.Ok(true);
    }

    private static async Task EncerrarAsync(
        Func<CrmDbContext> abrir, long execucaoId, Action<ExecucaoDeSincronizacao> encerrar, CancellationToken ct)
    {
        await using var banco = abrir();
        var execucao = await banco.ExecucoesDeSincronizacao.FirstAsync(e => e.Id == execucaoId, ct);
        encerrar(execucao);
        await banco.SaveChangesAsync(ct);
    }

    /// <summary>
    /// OS BLOCOS DE GRAVAÇÃO, POR PROCESSO: os estágios de um processo nunca ficam em blocos diferentes. O bloco fecha
    /// quando passa do tamanho — no máximo seis linhas além dele, as do último processo.
    /// </summary>
    private static List<BlocoDoFunil> BlocosPorProcesso(Plano plano, int tamanho)
    {
        var porProcesso = new SortedDictionary<long, BlocoDoFunil>();
        BlocoDoFunil Do(long numero)
        {
            if (!porProcesso.TryGetValue(numero, out var bloco)) porProcesso[numero] = bloco = new BlocoDoFunil();
            return bloco;
        }

        foreach (var retrato in plano.AIncluir) Do(retrato.NumeroDoProcessoNaOrigem).Incluir.Add(retrato);
        foreach (var alteracao in plano.AAlterar) Do(alteracao.Retrato.NumeroDoProcessoNaOrigem).Alterar.Add(alteracao);
        foreach (var (id, numero) in plano.ARemover) Do(numero).Remover.Add(id);

        var blocos = new List<BlocoDoFunil>();
        var atual = new BlocoDoFunil();
        foreach (var processo in porProcesso.Values)
        {
            atual.Incluir.AddRange(processo.Incluir);
            atual.Alterar.AddRange(processo.Alterar);
            atual.Remover.AddRange(processo.Remover);
            if (atual.Linhas < tamanho) continue;
            blocos.Add(atual);
            atual = new BlocoDoFunil();
        }

        if (atual.Linhas > 0) blocos.Add(atual);
        return blocos;
    }

    private async Task AplicarVendasPerdidasAsync(Func<CrmDbContext> abrir, Plano plano, int sistemaId, DateTime agora, CancellationToken ct)
    {
        await using var banco = abrir();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        // OS CATÁLOGOS, pela codificação da carga antiga: "Condição comercial" e "Condição Comercial" são um item só.
        foreach (var (codigo, nome) in plano.MotivosNovos)
            banco.MotivosDePerda.Add(MotivoDePerda.Criar(codigo, nome, SaneamentoDeVendaPerdida.CategoriaDoMotivo(codigo)));
        foreach (var ((catalogo, codigo), nome) in plano.ItensNovos)
            banco.CatalogoItens.Add(CatalogoItem.Criar(catalogo, codigo, nome));
        await banco.SaveChangesAsync(ct);

        var motivos = await banco.MotivosDePerda.AsNoTracking().ToDictionaryAsync(m => m.Codigo, m => m.Id, StringComparer.Ordinal, ct);
        var itens = (await banco.CatalogoItens.AsNoTracking()
                .Where(i => i.CatalogoId == CatalogosDeSistema.Concorrente || i.CatalogoId == CatalogosDeSistema.TipoDeEquipamento
                                                                           || i.CatalogoId == CatalogosDeSistema.RevendaConcorrente)
                .Select(i => new { i.CatalogoId, i.Codigo, i.Id })
                .ToListAsync(ct))
            .ToDictionary(i => (i.CatalogoId, i.Codigo), i => i.Id);

        var chaves = await banco.ChavesExternas
            .Where(c => c.SistemaId == sistemaId && c.Entidade == EntidadeVendaPerdida)
            .ToListAsync(ct);
        var geridas = new Dictionary<long, VendaPerdida>();
        foreach (var fatia in chaves.Select(c => c.RegistroId).Distinct().Chunk(IdsPorConsulta))
        {
            var idsGeridos = fatia.ToList();
            foreach (var venda in await banco.VendasPerdidas.Where(v => idsGeridos.Contains(v.Id)).ToListAsync(ct)) geridas[venda.Id] = venda;
        }
        var chavePorOrigem = chaves.GroupBy(c => c.ChaveOrigem, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        // DUAS PASSADAS, NA ORDEM DO PAPEL: primeiro as principais — que não apontam para ninguém —, depois o complemento e
        // a duplicata, que já nascem apontando para a principal. A nova nasce com o papel certo, e a trilha de auditoria
        // não ganha um "era principal, virou duplicata" de algo que nasceu agora.
        var vendaPorChave = new Dictionary<string, VendaPerdida>(StringComparer.Ordinal);
        var queEntram = plano.Respostas.Where(r => r.Motivo is null).ToList();

        foreach (var passada in new[]
                 {
                     queEntram.Where(r => r.Papel!.Papel == PapelDaVendaPerdida.Principal).ToList(),
                     queEntram.Where(r => r.Papel!.Papel != PapelDaVendaPerdida.Principal).ToList()
                 })
        {
            var novas = new List<(string Chave, VendaPerdida Venda)>();
            foreach (var resposta in passada)
            {
                var chave = Chave(resposta.Origem);
                var conteudo = Conteudo(resposta, motivos, itens);
                long? principal = resposta.Papel!.ChaveDaPrincipal is { } chaveDaPrincipal
                    ? vendaPorChave[chaveDaPrincipal.ToString(CultureInfo.InvariantCulture)].Id
                    : null;

                if (chavePorOrigem.TryGetValue(chave, out var deParaExistente) && geridas.TryGetValue(deParaExistente.RegistroId, out var existente))
                {
                    // A EXISTENTE acompanha a origem, volta se tinha sido excluída e muda de papel se o par mudou.
                    existente.AtualizarDaOrigem(conteudo, usuarioId);
                    existente.RestaurarDaOrigem(usuarioId);
                    existente.DefinirPapel(resposta.Papel.Papel, principal, usuarioId);
                    vendaPorChave[chave] = existente;
                    continue;
                }

                var nova = VendaPerdida.DaOrigem(conteudo, usuarioId, resposta.Papel.Papel, principal);
                banco.VendasPerdidas.Add(nova);
                novas.Add((chave, nova));
                vendaPorChave[chave] = nova;
            }

            await banco.SaveChangesAsync(ct);

            foreach (var (chave, venda) in novas)
            {
                if (chavePorOrigem.TryGetValue(chave, out var orfa)) orfa.ReapontarPara(venda.Id, agora);
                else banco.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, EntidadeVendaPerdida, venda.Id, chave));
            }
        }

        // O que sumiu da origem, ou ficou pendente, sai da conta — exclusão lógica, nunca apagada.
        foreach (var id in plano.AExcluir)
            if (geridas.TryGetValue(id, out var venda)) venda.Excluir(usuarioId);

        await banco.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);
    }

    /// <summary>
    /// A TRILHA DA ORIGEM: o registro novo nasce, o que mudou (conteúdo, decisão, ou voltou a aparecer) é atualizado, o
    /// que sumiu é marcado ausente — e o que ficou igual só ganha a leitura, num <c>UPDATE</c> por bloco, sem passar pelo
    /// rastreador: são dezenas de milhares de linhas iguais por dia, e carregar cada uma para mudar dois campos seria o
    /// grosso da rodada.
    /// </summary>
    private static async Task AplicarTrilhaAsync(
        Func<CrmDbContext> abrir, int sistemaId, string fluxo, IReadOnlyList<RegistroPlanejado> planejados, PlanoDaTrilha trilha,
        DateTime agora, CancellationToken ct)
    {
        var aTratar = planejados.Where(r => !trilha.Iguais.Contains(r.Chave)).ToList();

        foreach (var bloco in aTratar.Chunk(IdsPorConsulta))
        {
            await using var banco = abrir();
            var chaves = bloco.Select(r => r.Chave).ToList();
            var existentes = await banco.RegistrosDeOrigem
                .Where(r => r.SistemaId == sistemaId && r.Fluxo == fluxo && chaves.Contains(r.ChaveOrigem))
                .ToDictionaryAsync(r => r.ChaveOrigem, StringComparer.Ordinal, ct);

            foreach (var planejado in bloco)
            {
                if (!existentes.TryGetValue(planejado.Chave, out var registro))
                {
                    registro = RegistroDeOrigem.Registrar(sistemaId, fluxo, planejado.Chave, planejado.Retrato, agora);
                    banco.RegistrosDeOrigem.Add(registro);
                }
                else
                {
                    registro.RegistrarLeitura(planejado.Retrato, agora);
                }

                registro.Decidir(planejado.Decisao, planejado.Motivos, null);
            }

            await banco.SaveChangesAsync(ct);
        }

        foreach (var bloco in trilha.Iguais.Chunk(IdsPorConsulta))
        {
            await using var banco = abrir();
            var chaves = bloco.ToList();
            await banco.RegistrosDeOrigem
                .Where(r => r.SistemaId == sistemaId && r.Fluxo == fluxo && chaves.Contains(r.ChaveOrigem))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.UltimaLeituraEm, agora)
                    .SetProperty(r => r.Leituras, r => r.Leituras + 1), ct);
        }

        foreach (var bloco in trilha.QueSumiram.Chunk(IdsPorConsulta))
        {
            await using var banco = abrir();
            var chaves = bloco.ToList();
            foreach (var registro in await banco.RegistrosDeOrigem
                         .Where(r => r.SistemaId == sistemaId && r.Fluxo == fluxo && chaves.Contains(r.ChaveOrigem))
                         .ToListAsync(ct))
                registro.MarcarAusente(agora);
            await banco.SaveChangesAsync(ct);
        }
    }

    // =============================================================================================
    // O relatório
    // =============================================================================================

    private void Relatar(LeituraDoFunilDoVortice funil, IReadOnlyList<RespostaDeVendaPerdidaNoVortice> respostas, Plano plano)
    {
        const string etapaDaLeitura = "1. Leitura do Vórtice";
        Contar(etapaDaLeitura, RotuloDeProcessosLidos, funil.Processos.Count);
        foreach (var tipo in funil.Processos.GroupBy(p => p.Tipo).OrderBy(g => g.Key))
            Contar(etapaDaLeitura, $"  tipo {tipo.Key}", tipo.Count());
        Contar(etapaDaLeitura, "linhas do histórico com resultado classificado ou ação da etapa", funil.Historico.Count);
        Contar(etapaDaLeitura, "respostas de venda perdida lidas", respostas.Count);
        foreach (var formulario in respostas.GroupBy(r => r.Formulario).OrderBy(g => g.Key, StringComparer.Ordinal))
            Contar(etapaDaLeitura, $"  {formulario.Key}", formulario.Count());

        const string etapaDaRegra = "2. A regra do estágio";
        var apuracoes = plano.Apuracoes;
        Contar(etapaDaRegra, "processos abertos antes da janela (01/11/2023) — fora do universo", apuracoes.Count(a => a.Situacao == SituacaoNaRegraDoEstagio.ForaDaJanela));
        Contar(etapaDaRegra, "processos sem inclusão e sem histórico — fora do universo", apuracoes.Count(a => a.Situacao == SituacaoNaRegraDoEstagio.SemAbertura));
        Contar(etapaDaRegra, RotuloDeProcessosNoFunil, plano.RegistrosDoFunil.Count(r => r.Motivos is null));
        Contar(etapaDaRegra, RotuloDeProcessosPendentes, plano.RegistrosDoFunil.Count(r => r.Motivos is not null));
        foreach (var motivo in plano.RegistrosDoFunil.Where(r => r.Motivos is not null).GroupBy(r => r.Motivos!).OrderByDescending(g => g.Count()))
            Contar(etapaDaRegra, $"  pendência por motivo: {motivo.Key}", motivo.Count());
        Contar(etapaDaRegra, "abertura deduzida do primeiro andamento (inclusão nula)",
            plano.Desejadas.Values.Where(d => d.AberturaDeduzida).Select(d => d.NumeroDoProcessoNaOrigem).Distinct().Count());
        Contar(etapaDaRegra, "resultados recusados por data futura", apuracoes.Sum(a => a.ResultadosComDataFutura));
        Contar(etapaDaRegra, "processos com data de abertura absurda recusada (valeu a seguinte, ou ficou pendente)",
            apuracoes.Count(a => a.AberturaRecusada));
        Contar(etapaDaRegra, "processos com DNA em ciclo (tratados como sem pai)", apuracoes.Count(a => a.DnaEmCiclo));
        Contar(etapaDaRegra, "processos com ProcessoPai sem vínculo DNA (não herdam, como no BI — decisão aberta, documento 52 §3)",
            funil.Processos.Count(p => p.NumeroDoPai is { } pai && pai != p.NumeroDoDna));
        foreach (var estagio in plano.Desejadas.Values.GroupBy(d => d.Estagio).OrderBy(g => g.Key))
        {
            Contar(etapaDaRegra, $"  {estagio.Key}: processos", estagio.Count());
            Contar(etapaDaRegra, $"  {estagio.Key}: herdados do processo DNA", estagio.Count(e => e.NumeroDoProcessoDnaNaOrigem is not null));
            Contar(etapaDaRegra, $"  {estagio.Key}: pela entrada digital", estagio.Count(e => e.PelaEntradaDigital));
        }

        const string etapaDasLigacoes = "3. Ligações com o CRM (nada é criado)";
        Contar(etapaDasLigacoes, "processos com cliente do CRM (pelo documento)", plano.Ligacoes.Count(l => l.ClienteId is not null));
        Contar(etapaDasLigacoes, "processos de prospect (sem cliente no CRM)", plano.Ligacoes.Count(l => l.ClienteId is null));
        Contar(etapaDasLigacoes, "processos com a carteira MAQ_NOVOS no CRM", plano.Ligacoes.Count(l => l.CarteiraId is not null));
        Contar(etapaDasLigacoes, "responsável pelo login do Vórtice", plano.Ligacoes.Count(l => l.ResponsavelPeloLogin));
        Contar(etapaDasLigacoes, "responsável pelo dono da carteira", plano.Ligacoes.Count(l => l.ResponsavelId is not null && !l.ResponsavelPeloLogin));
        Contar(etapaDasLigacoes, "sem responsável", plano.Ligacoes.Count(l => l.ResponsavelId is null));

        const string etapaDoFunil = "4. Funil (processo.EstagioDoProcesso)";
        Contar(etapaDoFunil, "linhas que o CRM já tinha", plano.LinhasExistentes);
        Contar(etapaDoFunil, RotuloDeLinhasIncluidas, plano.AIncluir.Count);
        Contar(etapaDoFunil, RotuloDeLinhasAlteradas, plano.AAlterar.Count);
        Contar(etapaDoFunil, RotuloDeLinhasRemovidas, plano.ARemover.Count);
        Contar(etapaDoFunil, "  de processo ainda no Vórtice", plano.RemovidasDeProcessoPresente);
        Contar(etapaDoFunil, "  de processo que não existe mais em IV_PROCDADO", plano.ARemover.Count - plano.RemovidasDeProcessoPresente);
        Contar(etapaDoFunil, RotuloDeLinhasMantidas, plano.Mantidas);

        const string etapaDaVendaPerdida = "5. Venda perdida (processo.VendaPerdida)";
        var entram = plano.Respostas.Where(r => r.Motivo is null).ToList();
        Contar(etapaDaVendaPerdida, RotuloDeRespostasQueEntram, entram.Count);
        foreach (var papel in entram.GroupBy(r => r.Papel!.Papel).OrderBy(g => g.Key))
            Contar(etapaDaVendaPerdida, $"  {papel.Key}", papel.Count());
        Contar(etapaDaVendaPerdida, "  principais com participação \"Não\" pelo SEM_PARTICIPACAO", entram.Count(r => r.Papel!.ParticipacaoNegada));
        Contar(etapaDaVendaPerdida, RotuloDeRespostasPendentes, plano.Respostas.Count(r => r.Motivo is not null));
        foreach (var motivo in plano.Respostas.Where(r => r.Motivo is not null).GroupBy(r => r.Motivo!).OrderByDescending(g => g.Count()))
            Contar(etapaDaVendaPerdida, $"  pendência por motivo: {motivo.Key}", motivo.Count());
        Contar(etapaDaVendaPerdida, "com cliente do CRM", entram.Count(r => r.ClienteId is not null));
        Contar(etapaDaVendaPerdida, RotuloDeVendasPerdidasIncluidas, entram.Count(r => r.VendaPerdidaId is null));
        Contar(etapaDaVendaPerdida, RotuloDeVendasPerdidasAlteradas, plano.VendasPerdidasAlteradas);
        Contar(etapaDaVendaPerdida, RotuloDePapeisAlterados, plano.PapeisAlterados);
        Contar(etapaDaVendaPerdida, "vendas perdidas restauradas (voltaram à origem)", plano.VendasPerdidasRestauradas);
        Contar(etapaDaVendaPerdida, RotuloDeVendasPerdidasExcluidas, plano.AExcluir.Count);
        Contar(etapaDaVendaPerdida, RotuloDeMotivosIncluidos, plano.MotivosNovos.Count);
        Contar(etapaDaVendaPerdida, "itens de catálogo incluídos (concorrente, tipo, revenda)", plano.ItensNovos.Count);
        Contar(etapaDaVendaPerdida, "valores saneados na entrada (preço, data, quantidade)", entram.Sum(r => r.Correcoes.Count));

        const string etapaDaTrilha = "6. Trilha (integracao.RegistroDeOrigem)";
        foreach (var (nome, trilha) in new[] { ("processos", plano.TrilhaDoFunil), ("respostas", plano.TrilhaDaVendaPerdida) })
        {
            Contar(etapaDaTrilha, $"{nome}: registros lidos pela primeira vez", trilha.Novos);
            Contar(etapaDaTrilha, $"{nome}: registros com conteúdo alterado na origem", trilha.Alterados);
            Contar(etapaDaTrilha, $"{nome}: registros com decisão ou motivo diferente", trilha.ComDecisaoAlterada);
            Contar(etapaDaTrilha, $"{nome}: registros sem alteração", trilha.Iguais.Count);
            Contar(etapaDaTrilha, $"{nome}: registros que sumiram da origem (marcados ausentes)", trilha.QueSumiram.Count);
        }

        if (plano.SistemaId is null)
            _observacoes.Add("O CRM ainda não conhece o sistema VORTICE: sem o de-para da sincronia das carteiras, nenhum processo " +
                             "liga à carteira nesta rodada. Rode a CARTEIRAS_VORTICE antes, e esta rotina liga na seguinte.");
    }

    private void Contar(string etapa, string rotulo, int valor)
    {
        _contagens.Add((etapa, rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    private static string Chave(RespostaDeVendaPerdidaNoVortice resposta) => resposta.Questionario.ToString(CultureInfo.InvariantCulture);

    private static string Codificar(string? texto) => SaneamentoDeProcesso.Codificar(texto, 40);

    private static string? Limitar(string? texto, int tamanho) => texto is null || texto.Length <= tamanho ? texto : texto[..tamanho];

    /// <summary>O login da conta: a parte antes do <c>@</c> do nome principal.</summary>
    private static string ParteLocal(string nomePrincipal)
    {
        var arroba = nomePrincipal.IndexOf('@', StringComparison.Ordinal);
        return (arroba < 0 ? nomePrincipal : nomePrincipal[..arroba]).Trim().ToLowerInvariant();
    }

    /// <summary>O conteúdo da venda perdida com os identificadores do catálogo; o código ainda não catalogado vale zero.</summary>
    private static ConteudoDaVendaPerdida Conteudo(
        PlanoDaResposta r, IReadOnlyDictionary<string, int> motivos, IReadOnlyDictionary<(int, string), int> itens)
    {
        int? Item(int catalogo, string codigo) => codigo.Length == 0 ? null : itens.GetValueOrDefault((catalogo, codigo));

        return new ConteudoDaVendaPerdida(
            r.EmpresaId, r.Origem.RegistradaEmUtc!.Value, r.OcorridaEm, motivos.GetValueOrDefault(r.MotivoCodigo), r.ClienteId,
            Item(CatalogosDeSistema.TipoDeEquipamento, r.Tipo.Codigo), Item(CatalogosDeSistema.Concorrente, r.Marca.Codigo),
            Item(CatalogosDeSistema.RevendaConcorrente, r.Revenda.Codigo), r.ModeloDoConcorrente, r.ModeloOfertado, r.Quantidade,
            r.PrecoDoConcorrente, r.PrecoOfertado, r.Participacao, r.Origem.Formulario, r.Origem.Processo);
    }

    /// <summary>O retrato do processo na trilha — sem nome nem documento: o tipo, a filial e o que a regra concluiu.</summary>
    private static RetratoDoRegistroDeOrigem RetratoDoProcesso(ProcessoNoFunilDoVortice processo, ApuracaoDoProcesso apuracao)
    {
        var conteudo = string.Join('|', new[]
            {
                processo.Tipo.ToString(CultureInfo.InvariantCulture), processo.NroEmpresa?.ToString(CultureInfo.InvariantCulture),
                processo.NumeroDoDna.ToString(CultureInfo.InvariantCulture), apuracao.Situacao.ToString(), processo.Status,
                apuracao.AbertoEm?.ToString("O", CultureInfo.InvariantCulture), processo.RealizadoEmUtc?.ToString("O", CultureInfo.InvariantCulture),
                processo.NumeroDoPai?.ToString(CultureInfo.InvariantCulture), apuracao.AberturaRecusada.ToString(), apuracao.DnaEmCiclo.ToString()
            }
            .Concat(apuracao.Estagios.Select(e => string.Join(',', e.Estagio, e.AlcancadoEm.ToString("O", CultureInfo.InvariantCulture),
                e.ResultadoQueAbriu.ToString(CultureInfo.InvariantCulture), e.NumeroDoProcessoDna?.ToString(CultureInfo.InvariantCulture),
                e.PelaEntradaDigital, e.UltimaAcaoDaEtapaEm?.ToString("O", CultureInfo.InvariantCulture)))));

        var transformacoes = new List<string>(4);
        if (apuracao.AberturaRecusada)
            transformacoes.Add("abertura: data absurda recusada (antes de 2000 ou depois de agora + 1 dia)" +
                               (apuracao.AbertoEm is null ? ", e nenhuma outra é crível" : ", e valeu a seguinte"));
        else if (apuracao.AberturaDeduzida) transformacoes.Add("abertura: a inclusão é nula, e valeu o primeiro andamento (o COALESCE do BI)");
        if (apuracao.DnaEmCiclo) transformacoes.Add("DNA em ciclo: tratado como processo sem pai");
        if (processo.NumeroDoPai is { } pai && pai != processo.NumeroDoDna)
            transformacoes.Add($"filho do processo {pai.ToString(CultureInfo.InvariantCulture)} por ProcessoPai, sem vínculo DNA: não herda, como no BI");
        if (apuracao.ResultadosComDataFutura > 0)
            transformacoes.Add($"{apuracao.ResultadosComDataFutura} resultado(s) com data futura recusado(s)");

        return new RetratoDoRegistroDeOrigem(
            Resumo(conteudo), null, $"PROCESSO {processo.Tipo}", null,
            processo.NroEmpresa?.ToString("00", CultureInfo.InvariantCulture), null,
            transformacoes.Count == 0 ? null : string.Join("; ", transformacoes));
    }

    private static RetratoDoRegistroDeOrigem RetratoDaResposta(PlanoDaResposta r)
    {
        var o = r.Origem;
        var conteudo = string.Join('|',
            o.Formulario, o.Processo?.ToString(CultureInfo.InvariantCulture), o.SeqPessoa?.ToString(CultureInfo.InvariantCulture),
            o.EmpresaDoProcesso?.ToString(CultureInfo.InvariantCulture), o.EmpresaDoHistorico?.ToString(CultureInfo.InvariantCulture),
            o.RegistradaEmUtc?.ToString("O", CultureInfo.InvariantCulture), o.Documento.Situacao, o.Documento.Numero, o.TipoDoEquipamento,
            o.Marca, o.ModeloDoConcorrente, o.Revenda, o.ModeloOfertado, o.Quantidade?.ToString(CultureInfo.InvariantCulture),
            o.OcorridaEm?.ToString("O", CultureInfo.InvariantCulture), o.PrecoDoConcorrente?.ToString(CultureInfo.InvariantCulture),
            o.PrecoOfertado?.ToString(CultureInfo.InvariantCulture), o.Motivo, o.Participamos);

        var transformacoes = r.Correcoes.Count == 0
            ? null
            : Limitar(string.Join("; ", r.Correcoes.Select(c => $"{c.Campo}: {c.Motivo}")), 1000);

        return new RetratoDoRegistroDeOrigem(
            Resumo(conteudo), null, o.Formulario, null,
            (o.EmpresaDoProcesso ?? o.EmpresaDoHistorico)?.ToString("00", CultureInfo.InvariantCulture),
            r.OcorridaEm, transformacoes);
    }

    private static string Resumo(string conteudo) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(conteudo)));

    // =============================================================================================
    // As peças do plano
    // =============================================================================================

    /// <summary>Um bloco de gravação: processos inteiros, com o que incluir, alterar e remover de cada um.</summary>
    private sealed class BlocoDoFunil
    {
        public List<RetratoDoEstagio> Incluir { get; } = [];

        public List<(long Id, RetratoDoEstagio Retrato)> Alterar { get; } = [];

        public List<long> Remover { get; } = [];

        public int Linhas => Incluir.Count + Alterar.Count + Remover.Count;
    }

    /// <summary>O registro da origem que esta rodada quer deixar na trilha.</summary>
    private sealed record RegistroPlanejado(string Chave, RetratoDoRegistroDeOrigem Retrato, DecisaoDaIntegracao Decisao, string? Motivos);

    /// <summary>O que a trilha de um fluxo vai receber.</summary>
    private sealed class PlanoDaTrilha(HashSet<string> planejados)
    {
        public HashSet<string> Planejados { get; } = planejados;

        public List<string> Iguais { get; } = [];

        public int Novos { get; set; }

        public int Alterados { get; set; }

        public int ComDecisaoAlterada { get; set; }

        public List<string> QueSumiram { get; } = [];
    }

    /// <summary>A ligação de um processo com o que o CRM já tem.</summary>
    private sealed record Ligacao(long? ClienteId, long? CarteiraId, long? ResponsavelId, bool ResponsavelPeloLogin);

    private sealed record Ligacoes(
        IReadOnlyDictionary<string, long> ClientePorDocumento,
        IReadOnlyDictionary<long, int> EmpresaDoCliente,
        IReadOnlyDictionary<int, long> CarteiraPorSeq,
        IReadOnlyDictionary<long, long> DonoDaCarteira,
        IReadOnlyDictionary<string, long> ContaPorLogin)
    {
        public long? Cliente(DocumentoDoVortice documento) =>
            documento.Situacao == SituacaoDoDocumentoNoVortice.Valido && ClientePorDocumento.TryGetValue(documento.Numero!, out var id) ? id : null;

        /// <summary>
        /// O cliente pelo documento; a carteira pelo de-para gravado pela sincronia das carteiras; o responsável pelo
        /// login do Vórtice e, sem conta, pelo dono da carteira.
        /// </summary>
        public Ligacao Do(ProcessoNoFunilDoVortice processo)
        {
            long? carteira = processo.SeqCarteira is { } seq && CarteiraPorSeq.TryGetValue(seq, out var k) ? k : null;
            long? pelaConta = processo.LoginDoResponsavel is { } login && ContaPorLogin.TryGetValue(login.Trim().ToLowerInvariant(), out var u) ? u : null;
            long? peloDono = carteira is { } c && DonoDaCarteira.TryGetValue(c, out var dono) ? dono : null;
            return new Ligacao(Cliente(processo.Documento), carteira, pelaConta ?? peloDono, pelaConta is not null);
        }
    }

    private sealed class PlanoDaResposta
    {
        public required RespostaDeVendaPerdidaNoVortice Origem { get; init; }

        public required List<CorrecaoAplicada> Correcoes { get; init; }

        public string? Motivo { get; set; }

        public int EmpresaId { get; set; }

        public long? ClienteId { get; set; }

        public string MotivoCodigo { get; set; } = MotivoNaoInformado;

        public string MotivoNome { get; set; } = string.Empty;

        public (string Codigo, string? Nome) Tipo { get; set; } = (string.Empty, null);

        public (string Codigo, string? Nome) Marca { get; set; } = (string.Empty, null);

        public (string Codigo, string? Nome) Revenda { get; set; } = (string.Empty, null);

        public DateOnly? OcorridaEm { get; set; }

        public int Quantidade { get; set; } = 1;

        public decimal? PrecoDoConcorrente { get; set; }

        public decimal? PrecoOfertado { get; set; }

        public ParticipacaoNaNegociacao Participacao { get; set; }

        public string? ModeloDoConcorrente { get; set; }

        public string? ModeloOfertado { get; set; }

        public PapelDecidido? Papel { get; set; }

        public long? VendaPerdidaId { get; set; }
    }

    private sealed class Plano
    {
        public int? SistemaId { get; set; }

        public IReadOnlyList<ApuracaoDoProcesso> Apuracoes { get; set; } = [];

        public List<Ligacao> Ligacoes { get; } = [];

        public Dictionary<(long Numero, EstagioDoFunil Estagio), RetratoDoEstagio> Desejadas { get; } = [];

        public HashSet<(long Numero, EstagioDoFunil Estagio)> Existentes { get; } = [];

        public List<RetratoDoEstagio> AIncluir { get; } = [];

        public List<(long Id, RetratoDoEstagio Retrato)> AAlterar { get; } = [];

        public List<(long Id, long Numero)> ARemover { get; } = [];

        public string? QuedaAceita { get; set; }

        public int RemovidasDeProcessoPresente { get; set; }

        public int Mantidas { get; set; }

        public int LinhasExistentes { get; set; }

        public List<RegistroPlanejado> RegistrosDoFunil { get; } = [];

        public List<PlanoDaResposta> Respostas { get; } = [];

        public List<RegistroPlanejado> RegistrosDaVendaPerdida => Respostas
            .Select(r => new RegistroPlanejado(
                Chave(r.Origem), RetratoDaResposta(r),
                r.Motivo is null ? DecisaoDaIntegracao.Importado : DecisaoDaIntegracao.Pendente, r.Motivo))
            .ToList();

        public Dictionary<string, string> MotivosNovos { get; } = new(StringComparer.Ordinal);

        public Dictionary<(int Catalogo, string Codigo), string> ItensNovos { get; } = [];

        public int VendasPerdidasAtivas { get; set; }

        public int VendasPerdidasAlteradas { get; set; }

        public int VendasPerdidasRestauradas { get; set; }

        public int PapeisAlterados { get; set; }

        public List<long> AExcluir { get; } = [];

        public PlanoDaTrilha TrilhaDoFunil { get; set; } = new([]);

        public PlanoDaTrilha TrilhaDaVendaPerdida { get; set; } = new([]);
    }
}
