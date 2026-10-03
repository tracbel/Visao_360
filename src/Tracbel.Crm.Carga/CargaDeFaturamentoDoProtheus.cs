using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>
/// O FATURAMENTO E A CURVA ABC — o que dá letra ao cliente. <c>--somente-faturamento</c>, a rotina
/// <c>FATURAMENTO_PROTHEUS</c> do orquestrador.
///
/// <para><b>Código próprio, fora da carga do Vórtice.</b> Até 24/09/2026 esta etapa morava numa parte
/// de <see cref="CargaDeProcessoDoVortice"/>, a classe congelada na fase 1 (decisão D-12), só porque
/// nasceu lá. Ela não lê o Vórtice em lugar nenhum: a contraparte vem da <c>SD2</c> do Protheus, o
/// cliente vem do nosso cadastro e a classe sai do que acabou de ser gravado. Aqui a trilha de
/// auditoria diz a origem certa — o Protheus, e não o Vórtice.</para>
///
/// <para>Três etapas, nesta ordem, porque cada uma depende da anterior: grava o faturamento por
/// cliente, filial e mês; tira da janela o que a origem não tem mais; ordena os clientes por
/// faturamento e corta a curva em A, B e C. Quem não aparece no faturamento é D — e D aqui significa
/// "não comprou na janela", não "cliente ruim".</para>
///
/// <para><b>A curva é por FILIAL, e não da rede inteira.</b> Um cliente que responde por 3% do
/// faturamento de Votuporanga é grande em Votuporanga, e some no consolidado ao lado de Ribeirão
/// Preto. Como a carteira e o CEN são de uma filial, a classe tem de ser da mesma filial. A filial
/// é a que EMITIU a nota (<c>D2_FILIAL</c>) — medido em 24/09/2026, as dezesseis filiais faturam no
/// ERP, e não só a 010101 como se acreditava (ver <see cref="LeitorDeFaturamentoDoProtheus"/>).</para>
///
/// <para><b>O movimento promove a situação.</b> A carga de clientes da SA1 cria todo cliente como
/// <c>Suspect</c>, porque o cadastro diz que ele existe e não que comprou (ver
/// <see cref="CargaDeClientesDoProtheus"/>). Quem sabe que comprou é a nota: o cliente com venda na
/// janela passa de <c>Suspect</c> ou <c>Prospect</c> a <c>Cliente</c>. Ninguém é rebaixado aqui, e
/// <c>ClienteInativo</c> e <c>Encerrado</c> — decisões de pessoa — não são tocados.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="protheus">A leitura da SD2.</param>
/// <param name="usuarioResponsavelId">Quem responde pelas gravações.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed partial class CargaDeFaturamentoDoProtheus(
    Func<CrmDbContext> abrirContexto,
    LeitorDeFaturamentoDoProtheus protheus,
    long usuarioResponsavelId,
    Action<string> relatar)
{
    private const int TamanhoDoBloco = 500;

    /// <summary>
    /// A maior fração da janela que uma execução pode remover. Nota cancelada e cliente que passou a
    /// existir mexem em poucos meses de cada vez; tirar um quinto da janela numa noite é leitura que
    /// veio pela metade, não o ERP mudando.
    /// </summary>
    public const double FracaoMaximaDeRemocao = 0.20;

    /// <summary>
    /// Abaixo disto a trava não se aplica: numa janela pequena (a primeira semana de uma filial, ou um
    /// teste) uma única nota cancelada já passa de 20%, e recusar seria falso alarme.
    /// </summary>
    public const int JanelaMinimaParaATrava = 1000;

    /// <summary>Se a remoção desta execução deve ser recusada como leitura parcial.</summary>
    public static bool RemocaoPassaDaTrava(int linhasNaJanela, int obsoletos) =>
        linhasNaJanela >= JanelaMinimaParaATrava && obsoletos > linhasNaJanela * FracaoMaximaDeRemocao;

    /// <summary>
    /// Quantos meses INTEIROS para trás a leitura e a curva ABC olham — três anos, mais o mês corrente.
    ///
    /// <para>A janela começa no dia 1: começar "hoje menos três anos" fazia o primeiro mês chegar pela
    /// metade e sobrescrever, com metade do valor, a competência que já estava inteira no banco.</para>
    /// </summary>
    internal const int MesesDeFaturamento = 36;

    /// <summary>Onde a classe A termina: os clientes que somam os primeiros 80% do faturamento.</summary>
    private const decimal CorteDaClasseA = 0.80m;

    /// <summary>Onde a classe B termina.</summary>
    private const decimal CorteDaClasseB = 0.95m;

    private readonly Dictionary<string, int> _decisoes = new(StringComparer.Ordinal);

    /// <summary>O sistema de origem, conhecido depois de garantido — a origem das gravações na trilha.</summary>
    private int? _sistemaId;

    /// <summary>O primeiro dia da janela: o dia 1 do mês, <see cref="MesesDeFaturamento"/> meses atrás.</summary>
    /// <param name="agoraUtc">O instante da carga.</param>
    internal static DateOnly InicioDaJanela(DateTime agoraUtc) =>
        new DateOnly(agoraUtc.Year, agoraUtc.Month, 1).AddMonths(-MesesDeFaturamento);

    /// <summary>A leitura curta, registrada como execução de sincronização — a tela de Integrações a lista por fluxo.</summary>
    internal const string FluxoDaLeituraCurta = "PROTHEUS.FATURAMENTO";

    /// <summary>A leitura completa — a última com sucesso decide quando a próxima é completa.</summary>
    internal const string FluxoDaLeituraCompleta = "PROTHEUS.FATURAMENTO_COMPLETO";

    /// <summary>Lê o faturamento do Protheus, grava e reapura a curva ABC — curta ou completa pela agenda da semana.</summary>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<ResumoDoFaturamento>> ExecutarAsync(CancellationToken ct) => ExecutarAsync(completaPedida: false, ct);

    /// <summary>Lê, grava e reapura; <paramref name="completaPedida"/> força os 36 meses (<c>--completa</c>).</summary>
    /// <param name="completaPedida">Se a leitura deve ser completa em qualquer dia.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<ResumoDoFaturamento>> ExecutarAsync(bool completaPedida, CancellationToken ct) =>
        ExecutarAsync((desde, c) => protheus.LerAsync(desde, relatar, c), completaPedida, () => DateTime.UtcNow, ct);

    /// <summary>A rodada inteira com a leitura e o relógio de fora — é o que o teste exercita.</summary>
    /// <param name="ler">A leitura da SD2 a partir de uma data.</param>
    /// <param name="completaPedida">Se a leitura deve ser completa em qualquer dia.</param>
    /// <param name="relogio">O relógio: decide o alcance e carimba a execução registrada.</param>
    /// <param name="ct">Cancelamento.</param>
    internal async Task<Resultado<ResumoDoFaturamento>> ExecutarAsync(
        Func<DateOnly, CancellationToken, Task<Resultado<LoteDoProtheus>>> ler, bool completaPedida, Func<DateTime> relogio, CancellationToken ct)
    {
        var agoraUtc = relogio();
        _sistemaId = await GarantirSistemaAsync(ct);
        var alcance = AlcanceDaLeituraDoFaturamento.Decidir(agoraUtc, await UltimaLeituraCompletaAsync(ct), completaPedida);
        var completa = alcance.Modo == ModoDaLeituraDoFaturamento.Completa;

        relatar(completa
            ? $"Leitura COMPLETA do faturamento — três anos, desde {alcance.Desde:dd/MM/yyyy} ({alcance.Motivo})…"
            : $"Leitura curta do faturamento — desde {alcance.Desde:dd/MM/yyyy} ({alcance.Motivo}); a completa roda no domingo…");

        // A RODADA, REGISTRADA POR FLUXO: a tela de Integrações lista as duas, e a última completa com sucesso decide quando a
        // próxima é completa. A que cai no meio fica como falha — e não conta.
        var execucaoId = await IniciarExecucaoAsync(completa ? FluxoDaLeituraCompleta : FluxoDaLeituraCurta, relogio(), ct);
        try
        {
            var lido = await ler(alcance.Desde, ct);
            if (!lido.EhSucesso)
            {
                await EncerrarExecucaoAsync(execucaoId, e => e.Falhar(1, lido.Erro!, relogio()), ct);
                return Resultado<ResumoDoFaturamento>.Indisponivel(lido.Erro!);
            }

            var resumo = await GravarAsync(lido.Valor, InicioDaJanela(agoraUtc), completa ? alcance.InicioDaJanelaCurta : null, ct) with
            {
                Modo = alcance.Modo,
                Desde = alcance.Desde
            };

            await EncerrarExecucaoAsync(execucaoId, e => e.Concluir(
                1, lido.Valor.Faturamento.Count, resumo.MesesNovos, resumo.MesesReapurados + resumo.MesesRemovidos, 0,
                LinhaDoResumo(resumo), relogio()), ct);
            return Resultado<ResumoDoFaturamento>.Ok(resumo);
        }
        catch (Exception falha) when (falha is DbUpdateException or RegraDeNegocioViolada or InvalidOperationException)
        {
            await EncerrarExecucaoAsync(
                execucaoId, e => e.Falhar(1, $"A carga parou no meio: {falha.GetBaseException().Message}", relogio()), ct);
            throw;
        }
    }

    /// <summary>
    /// A LINHA DA RODADA — o que o orquestrador guarda na execução da rotina e a tela de Integrações mostra: o modo, desde
    /// quando leu e, na completa, o que ela corrigiu antes da janela curta.
    ///
    /// <para>O valor sai como "R$" + número, e não pelo formato de moeda: o do pt-BR põe um espaço não separável depois do
    /// símbolo, que a tela e o teste leriam diferente.</para>
    /// </summary>
    /// <param name="resumo">O resumo da rodada.</param>
    internal static string LinhaDoResumo(ResumoDoFaturamento resumo)
    {
        var modo = resumo.Modo == ModoDaLeituraDoFaturamento.Completa ? "leitura COMPLETA" : "leitura curta";
        var desde = resumo.Desde is { } d ? string.Create(CulturaDoRelatorio, $" desde {d:dd/MM/yyyy}") : string.Empty;
        var linha = string.Create(CulturaDoRelatorio,
            $"{modo}{desde}: {resumo.MesesNovos:N0} mês(es) novo(s), {resumo.MesesReapurados:N0} reapurado(s), {resumo.MesesRemovidos:N0} removido(s)");

        return resumo.Conferencia is not { } conferencia
            ? linha
            : linha + string.Create(CulturaDoRelatorio,
                $" · antes de {conferencia.Antes:MM/yyyy}, {conferencia.Meses:N0} mês(es) corrigido(s) (R$ {conferencia.Valor:N0}) que a leitura curta não teria visto");
    }

    /// <summary>O fim da última leitura completa com sucesso; nulo quando nunca houve.</summary>
    private async Task<DateTime?> UltimaLeituraCompletaAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        return await contexto.ExecucoesDeSincronizacao.AsNoTracking()
            .Where(e => e.Fluxo == FluxoDaLeituraCompleta && e.Resultado == ResultadoDaExecucao.Sucesso)
            .MaxAsync(e => e.TerminadaEm, ct);
    }

    private async Task<long> IniciarExecucaoAsync(string fluxo, DateTime quando, CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        var execucao = ExecucaoDeSincronizacao.Iniciar(_sistemaId!.Value, fluxo, Environment.MachineName, quando);
        contexto.ExecucoesDeSincronizacao.Add(execucao);
        await contexto.SaveChangesAsync(ct);
        return execucao.Id;
    }

    private async Task EncerrarExecucaoAsync(long execucaoId, Action<ExecucaoDeSincronizacao> encerrar, CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        encerrar(await contexto.ExecucoesDeSincronizacao.FirstAsync(e => e.Id == execucaoId, ct));
        await contexto.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Grava um lote já lido — a carga inteira menos a leitura. É o que o teste exercita.
    /// </summary>
    /// <param name="lote">O que a leitura da SD2 produziu.</param>
    /// <param name="ct">Cancelamento.</param>
    internal Task<ResumoDoFaturamento> GravarAsync(LoteDoProtheus lote, CancellationToken ct) =>
        GravarAsync(lote, InicioDaJanela(DateTime.UtcNow), conferirAntesDe: null, ct);

    /// <summary>Grava um lote já lido, com a janela da curva e a fronteira da conferência explícitas.</summary>
    /// <param name="lote">O que a leitura da SD2 produziu — curta ou completa.</param>
    /// <param name="inicioDaCurva">O início dos 36 meses da curva ABC, o mesmo nos dois modos.</param>
    /// <param name="conferirAntesDe">Na leitura completa, onde a janela curta começa: o que mudar antes disso é contado.</param>
    /// <param name="ct">Cancelamento.</param>
    internal async Task<ResumoDoFaturamento> GravarAsync(
        LoteDoProtheus lote, DateOnly inicioDaCurva, DateOnly? conferirAntesDe, CancellationToken ct)
    {
        _sistemaId = await GarantirSistemaAsync(ct);
        var conferidos = conferirAntesDe is { } antes ? new Conferidos(antes) : null;

        RegistrarDescartesDaLeitura(lote);

        var gravacao = await GravarFaturamentoAsync(lote, conferidos, ct);

        relatar(
            $"  {gravacao.Novos} mês(es) novo(s), {gravacao.Reapurados} reapurado(s) e {gravacao.Removidos} removido(s) " +
            $"da janela · nota mais recente: {lote.EmissaoMaisRecente:dd/MM/yyyy}.");

        // A CURVA OLHA OS 36 MESES NOS DOIS MODOS (plano 3 do documento 54): com a janela da leitura, a rodada curta
        // rebaixaria a D o cliente que comprou muito em março e nada desde então.
        var (curva, promovidos) = await ApurarCurvaAbcAsync(inicioDaCurva, ct);
        relatar(
            "  curva ABC: " +
            string.Join(" · ", curva.OrderBy(p => p.Key).Select(p => $"{p.Key} {p.Value}")) +
            $" · {promovidos} cliente(s) promovido(s) a Cliente pelo faturamento.");

        // A CONFERÊNCIA SAI ATÉ QUANDO É ZERO: "nenhum mês corrigido" é justamente a confirmação de que a leitura curta basta.
        var conferencia = conferidos?.Fechar();
        if (conferencia is not null)
            Decidir(
                $"Conferência semanal: meses ANTES da janela curta ({conferencia.Antes:MM/yyyy}) corrigidos pela leitura " +
                $"completa — {Reais(conferencia.Valor)} que a leitura curta não teria visto",
                conferencia.Meses);

        // AS DECISÕES SAEM JUNTO, e não só a contagem. Elas são o que diz quanto dinheiro ficou de fora
        // e por quê — sem isso o comando terminaria com "6.849 meses gravados" e o descarte voltaria a
        // ser invisível.
        return new ResumoDoFaturamento(
            gravacao.Novos, gravacao.Reapurados, gravacao.Removidos, promovidos, curva,
            new Dictionary<string, int>(_decisoes, StringComparer.Ordinal), conferencia);
    }

    // =============================================================================================
    // A gravação
    // =============================================================================================

    private async Task<(int Novos, int Reapurados, int Removidos)> GravarFaturamentoAsync(
        LoteDoProtheus lote, Conferidos? conferidos, CancellationToken ct)
    {
        var faturamento = lote.Faturamento;

        // A LEITURA VAZIA NÃO APAGA NADA. A janela é substituída pelo que a origem devolveu; uma
        // origem que devolve zero linha num ERP que emite nota todo dia é defeito de leitura, não
        // ausência de venda — e apagar três anos de faturamento por causa dele seria o pior desfecho.
        if (faturamento.Count == 0)
        {
            Decidir("Leitura do Protheus sem nenhuma venda na janela — nada foi gravado nem removido", 1);
            return (0, 0, 0);
        }

        var porDocumento = await MapaDeClientesPorDocumentoAsync(ct);
        var porCodigoDeEmpresa = await MapaDeEmpresasPorCodigoAsync(ct);

        int novos = 0, reapurados = 0;
        var documentosSemCliente = new HashSet<string>(StringComparer.Ordinal);

        // O DESCARTE TEM DE DIZER QUANTO DINHEIRO LEVOU JUNTO. Contar linha descartada não basta:
        // oitenta e quatro documentos parecem um resíduo, e eram mais de duzentos milhões de reais.
        var valorSemCliente = 0m;
        var filiaisSemEmpresa = new HashSet<string>(StringComparer.Ordinal);
        var valorSemEmpresa = 0m;

        // O QUE A LEITURA PRODUZIU, pela chave natural de cada tabela. É o que define a janela depois:
        // o que está no banco dentro dela e não está aqui não existe mais na origem.
        var produzidosComCliente = new HashSet<(long ClienteId, int EmpresaId, DateOnly Competencia)>();
        var produzidosSemCliente = new HashSet<(string Documento, int EmpresaId, DateOnly Competencia)>();

        foreach (var bloco in faturamento.Chunk(TamanhoDoBloco))
        {
            await using var contexto = AbrirContextoDaCarga();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            // A CHAVE NATURAL É (cliente, filial, competência), e é ela que torna a carga
            // reexecutável: quem já existe é reapurado, não duplicado.
            var resolvido = bloco
                .Select(f =>
                {
                    var (clienteId, empresaId) = Resolver(f, porDocumento, porCodigoDeEmpresa);
                    return (Linha: f, ClienteId: clienteId, EmpresaId: empresaId);
                })
                .ToList();

            foreach (var (linha, _, _) in resolvido.Where(x => x.ClienteId == 0))
            {
                documentosSemCliente.Add(linha.DocumentoDoCliente);
                valorSemCliente += linha.ValorLiquido;
            }

            foreach (var (linha, _, _) in resolvido.Where(x => x.EmpresaId == 0))
            {
                filiaisSemEmpresa.Add(linha.CodigoDaFilial ?? "(sem filial)");
                valorSemEmpresa += linha.ValorLiquido;
            }

            // QUEM NÃO TEM CLIENTE VAI PARA A OUTRA TABELA, e não para o lixo. Precisa da filial
            // resolvida — sem ela não há onde pendurar a linha.
            var semDono = resolvido.Where(x => x.ClienteId == 0 && x.EmpresaId != 0).ToList();
            foreach (var (linha, _, empresaId) in semDono)
                produzidosSemCliente.Add((linha.DocumentoDoCliente, empresaId, linha.Competencia));

            await GravarSemClienteAsync(contexto, semDono.Select(x => (x.Linha, x.EmpresaId)), lote.Desde, conferidos, ct);

            var doBloco = resolvido.Where(x => x.ClienteId != 0 && x.EmpresaId != 0).ToList();
            if (doBloco.Count > 0)
            {
                var chaves = doBloco.Select(x => x.ClienteId).Distinct().ToList();
                var existentes = await contexto.FaturamentoDosClientes
                    .Where(f => chaves.Contains(f.ClienteId) && f.Competencia >= lote.Desde)
                    .ToDictionaryAsync(f => (f.ClienteId, f.EmpresaId, f.Competencia), ct);

                foreach (var (linha, clienteId, empresaId) in doBloco)
                {
                    var chave = (clienteId, empresaId, linha.Competencia);
                    produzidosComCliente.Add(chave);

                    if (existentes.TryGetValue(chave, out var jaExiste))
                    {
                        conferidos?.Contar(linha.Competencia, linha.ValorLiquido - jaExiste.ValorLiquido);
                        jaExiste.Reapurar(linha.ValorLiquido, linha.Notas, linha.Itens, linha.Quebra);
                        reapurados++;
                        continue;
                    }

                    conferidos?.Contar(linha.Competencia, linha.ValorLiquido);
                    contexto.FaturamentoDosClientes.Add(FaturamentoDoCliente.Criar(
                        empresaId, clienteId, linha.Competencia,
                        linha.ValorLiquido, linha.Notas, linha.Itens, linha.Quebra));
                    novos++;
                }
            }

            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        var removidos = await RemoverOQueSaiuDaOrigemAsync(lote.Desde, produzidosComCliente, produzidosSemCliente, conferidos, ct);

        if (documentosSemCliente.Count > 0)
            Decidir(
                $"Documentos que faturam e NÃO existem no cadastro de clientes do CRM — " +
                $"{Reais(valorSemCliente)} de venda sem dono aqui dentro (gravados em FaturamentoSemCliente)",
                documentosSemCliente.Count);

        if (filiaisSemEmpresa.Count > 0)
            Decidir(
                $"Códigos de filial do faturamento sem empresa correspondente " +
                $"({string.Join(", ", filiaisSemEmpresa.Order(StringComparer.Ordinal))}) — {Reais(valorSemEmpresa)} descartados",
                filiaisSemEmpresa.Count);

        if (removidos > 0)
            Decidir(
                "Meses de faturamento removidos da janela porque a origem não os tem mais — nota cancelada, " +
                "ou cliente que passou a existir no CRM e saiu de FaturamentoSemCliente",
                removidos);

        return (novos, reapurados, removidos);
    }

    /// <summary>
    /// A JANELA É SUBSTITUÍDA, e não só acrescida — é isto que torna a carga idempotente por competência.
    ///
    /// <para>Sem esta etapa, a linha que mudou de lugar ficava nos dois. O caso que motivou: o
    /// faturamento roda antes da carga de clientes da SA1, e todo o dinheiro vai para
    /// <c>FaturamentoSemCliente</c>; no dia seguinte o cliente existe, a mesma venda entra em
    /// <c>FaturamentoDoCliente</c> — e a linha antiga continuava, contando o mesmo real duas vezes no
    /// cartão da diretoria. O mesmo vale para a nota cancelada no ERP.</para>
    ///
    /// <para><b>Exclusão física, de propósito.</b> Estas tabelas são a projeção de um fato que mora no
    /// ERP; a linha que a origem não tem mais não é histórico, é erro. É a mesma regra do crédito do
    /// SICOR, onde a operação reclassificada sai para não ser contada duas vezes. Nenhuma das duas
    /// tabelas está na política de auditoria. Só a janela lida é tocada: o que é anterior a ela fica
    /// como está.</para>
    /// </summary>
    private async Task<int> RemoverOQueSaiuDaOrigemAsync(
        DateOnly desde,
        HashSet<(long ClienteId, int EmpresaId, DateOnly Competencia)> comCliente,
        HashSet<(string Documento, int EmpresaId, DateOnly Competencia)> semCliente,
        Conferidos? conferidos,
        CancellationToken ct)
    {
        await using var contexto = AbrirContextoDaCarga();

        var naJanelaComCliente = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.Competencia >= desde)
            .Select(f => new { f.Id, f.ClienteId, f.EmpresaId, f.Competencia, f.ValorLiquido })
            .ToListAsync(ct);
        var obsoletosComCliente = naJanelaComCliente
            .Where(f => !comCliente.Contains((f.ClienteId, f.EmpresaId, f.Competencia)))
            .Select(f => (f.Id, f.Competencia, f.ValorLiquido))
            .ToList();

        var naJanelaSemCliente = await contexto.FaturamentoSemClientes.AsNoTracking()
            .Where(f => f.Competencia >= desde)
            .Select(f => new { f.Id, f.Documento, f.EmpresaId, f.Competencia, f.ValorLiquido })
            .ToListAsync(ct);
        var obsoletosSemCliente = naJanelaSemCliente
            .Where(f => !semCliente.Contains((f.Documento, f.EmpresaId, f.Competencia)))
            .Select(f => (f.Id, f.Competencia, f.ValorLiquido))
            .ToList();

        if (obsoletosComCliente.Count == 0 && obsoletosSemCliente.Count == 0) return 0;

        // A LEITURA PARCIAL NÃO APAGA EM MASSA. A leitura vazia já está barrada lá em cima; esta é a
        // irmã dela que passa pela porta: uma filial que some da leitura por um problema passageiro, e
        // a janela dela inteira sairia do banco numa madrugada. Uma exclusão acima da fração máxima
        // é tratada como defeito de leitura — nada é removido, e a decisão fica no relatório.
        var naJanela = naJanelaComCliente.Count + naJanelaSemCliente.Count;
        var obsoletos = obsoletosComCliente.Count + obsoletosSemCliente.Count;
        if (RemocaoPassaDaTrava(naJanela, obsoletos))
        {
            Decidir(
                $"Remoção RECUSADA: a leitura deixaria de fora {obsoletos} de {naJanela} meses da janela " +
                $"(acima de {FracaoMaximaDeRemocao:P0}) — tratado como leitura parcial do Protheus; nada foi removido",
                obsoletos);
            return 0;
        }

        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);
        var removidos = 0;

        foreach (var bloco in obsoletosComCliente.Select(f => f.Id).Chunk(TamanhoDoBloco))
            removidos += await contexto.FaturamentoDosClientes.Where(f => bloco.Contains(f.Id)).ExecuteDeleteAsync(ct);

        foreach (var bloco in obsoletosSemCliente.Select(f => f.Id).Chunk(TamanhoDoBloco))
            removidos += await contexto.FaturamentoSemClientes.Where(f => bloco.Contains(f.Id)).ExecuteDeleteAsync(ct);

        await transacao.CommitAsync(ct);

        // O MÊS QUE SAIU TAMBÉM É CORREÇÃO: a nota de junho cancelada em setembro só a leitura completa vê.
        foreach (var (_, competencia, valor) in obsoletosComCliente.Concat(obsoletosSemCliente))
            conferidos?.Contar(competencia, valor);

        return removidos;
    }

    /// <summary>
    /// Grava o faturamento cuja contraparte não é cliente do CRM.
    ///
    /// <para>Não chama <c>SaveChanges</c>: entra na mesma transação do bloco de quem tem cliente,
    /// para que uma carga interrompida não deixe metade de um mês de um lado e metade do outro.</para>
    /// </summary>
    private static async Task GravarSemClienteAsync(
        CrmDbContext contexto,
        IEnumerable<(FaturamentoParaCarga Linha, int EmpresaId)> linhas,
        DateOnly desde,
        Conferidos? conferidos,
        CancellationToken ct)
    {
        var doBloco = linhas.ToList();
        if (doBloco.Count == 0) return;

        var documentos = doBloco.Select(x => x.Linha.DocumentoDoCliente).Distinct().ToList();

        var existentes = await contexto.FaturamentoSemClientes
            .Where(f => documentos.Contains(f.Documento) && f.Competencia >= desde)
            .ToDictionaryAsync(f => (f.Documento, f.EmpresaId, f.Competencia), ct);

        foreach (var (linha, empresaId) in doBloco)
        {
            var natureza = ClassificarParceiro(linha.DocumentoDoCliente);
            var chave = (linha.DocumentoDoCliente, empresaId, linha.Competencia);

            if (existentes.TryGetValue(chave, out var jaExiste))
            {
                conferidos?.Contar(linha.Competencia, linha.ValorLiquido - jaExiste.ValorLiquido);
                jaExiste.Reapurar(
                    linha.NomeNaOrigem, natureza,
                    linha.ValorLiquido, linha.Notas, linha.Itens, linha.Quebra);
                continue;
            }

            conferidos?.Contar(linha.Competencia, linha.ValorLiquido);
            contexto.FaturamentoSemClientes.Add(FaturamentoSemCliente.Criar(
                empresaId, linha.Competencia, linha.DocumentoDoCliente, linha.NomeNaOrigem,
                natureza, linha.ValorLiquido, linha.Notas, linha.Itens, linha.Quebra));
        }
    }

    /// <summary>O que a leitura já deixou de fora, com o valor — antes de o CRM decidir qualquer coisa.</summary>
    private void RegistrarDescartesDaLeitura(LoteDoProtheus lote)
    {
        if (lote.CodigosSemDocumento > 0)
            Decidir(
                $"Códigos de cliente da nota sem CPF/CNPJ válido na SA1 — {Reais(lote.ValorSemDocumento)} " +
                $"de venda em {lote.ItensSemDocumento:N0} itens, sem a quem atribuir",
                lote.CodigosSemDocumento);

        if (lote.ItensQueNaoSaoVenda > 0)
            Decidir(
                "Itens de saída descartados por não serem venda — transferência entre filiais, remessa e " +
                $"retorno de demonstração, devolução e baixa de estoque ({Reais(lote.ValorQueNaoEhVenda)})",
                lote.ItensQueNaoSaoVenda);

        if (lote.ItensSemData > 0)
            Decidir("Itens de nota descartados por não ter data de emissão legível", lote.ItensSemData);
    }

    // =============================================================================================
    // A curva ABC e a situação
    // =============================================================================================

    /// <summary>
    /// Apura a curva ABC por filial, grava a classe em cada cliente e promove quem comprou.
    /// </summary>
    /// <param name="desde">O início da janela: só o faturamento dela conta.</param>
    /// <param name="ct">Cancelamento.</param>
    private async Task<(Dictionary<ClasseDeCliente, int> Curva, int Promovidos)> ApurarCurvaAbcAsync(
        DateOnly desde, CancellationToken ct)
    {
        var agora = DateTime.UtcNow;

        // A SOMA É EM MEMÓRIA: são três anos de linhas cliente × filial × mês, três colunas cada. E
        // A JANELA É A MESMA DA LEITURA — sem o filtro, a curva de daqui a um ano olharia quatro.
        List<(long ClienteId, int EmpresaId, decimal Faturado)> linhas;
        await using (var contexto = AbrirContextoDaCarga())
        {
            linhas = [.. (await contexto.FaturamentoDosClientes.AsNoTracking()
                    .Where(f => f.ExcluidoEm == null && f.Competencia >= desde)
                    .Select(f => new { f.ClienteId, f.EmpresaId, f.ValorLiquido })
                    .ToListAsync(ct))
                .Select(f => (f.ClienteId, f.EmpresaId, f.ValorLiquido))];
        }

        var classePorCliente = ClassificarCurva(linhas);
        var contagem = new Dictionary<ClasseDeCliente, int>();
        var promovidos = 0;

        foreach (var bloco in classePorCliente.Chunk(TamanhoDoBloco))
        {
            await using var contextoDoBloco = AbrirContextoDaCarga();
            var ids = bloco.Select(p => p.Key).ToList();
            var clientes = await contextoDoBloco.Clientes.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

            foreach (var cliente in clientes)
            {
                var (classe, faturado) = classePorCliente[cliente.Id];
                cliente.ApurarClasse(classe, faturado, agora, usuarioResponsavelId);
                contagem[classe] = contagem.GetValueOrDefault(classe) + 1;

                if (!cliente.EstaExcluido && PromoveAoFaturar(cliente.Situacao))
                {
                    cliente.MudarSituacao(SituacaoDoCliente.Cliente, usuarioResponsavelId);
                    promovidos++;
                }
            }

            await contextoDoBloco.SaveChangesAsync(ct);
        }

        // D É QUEM NÃO COMPROU, e ele é a maioria. Marcar explicitamente, em vez de deixar nulo, é o
        // que permite a tela dizer "3.100 clientes D sem visita há mais de 360 dias".
        List<long> semFaturamento;
        await using (var contexto = AbrirContextoDaCarga())
        {
            var comClasse = classePorCliente.Keys.ToHashSet();
            semFaturamento = [.. (await contexto.Clientes.AsNoTracking()
                    .Where(c => c.ExcluidoEm == null)
                    .Select(c => c.Id)
                    .ToListAsync(ct))
                .Where(id => !comClasse.Contains(id))];
        }

        foreach (var bloco in semFaturamento.Chunk(TamanhoDoBloco))
        {
            await using var contextoDoBloco = AbrirContextoDaCarga();
            var ids = bloco.ToList();
            var clientes = await contextoDoBloco.Clientes.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

            foreach (var cliente in clientes)
                cliente.ApurarClasse(ClasseDeCliente.D, 0m, agora, usuarioResponsavelId);

            contagem[ClasseDeCliente.D] = contagem.GetValueOrDefault(ClasseDeCliente.D) + clientes.Count;
            await contextoDoBloco.SaveChangesAsync(ct);
        }

        if (promovidos > 0)
            Decidir("Clientes promovidos de Suspect/Prospect a Cliente por terem venda na janela", promovidos);

        return (contagem, promovidos);
    }

    /// <summary>
    /// A curva ABC, pura: de (cliente, filial, faturado) para a classe de cada cliente.
    ///
    /// <para><b>Um cliente pode faturar em mais de uma filial.</b> A classe é dele, então a filial que
    /// decide é aquela onde ele mais comprou — a praça em que ele é cliente de verdade.</para>
    ///
    /// <para><b>O corte é pelo acumulado</b>, e não pelo valor do cliente: é isso que faz a curva ser
    /// ABC e não uma faixa de preço. O acumulado já inclui o próprio cliente — é a regra que a carga
    /// sempre aplicou. Consequência que o comentário antigo escondia ("o primeiro cliente é sempre A"):
    /// quem sozinho passa de 80% da filial é B, e de 95% é C. É decisão de negócio mudar isso.</para>
    /// </summary>
    /// <param name="linhas">O faturamento, uma linha por cliente, filial e mês (ou já somado).</param>
    internal static Dictionary<long, (ClasseDeCliente Classe, decimal Faturado)> ClassificarCurva(
        IEnumerable<(long ClienteId, int EmpresaId, decimal Faturado)> linhas)
    {
        var melhorFilialPorCliente = linhas
            .GroupBy(x => (x.ClienteId, x.EmpresaId))
            .Select(g => (g.Key.ClienteId, g.Key.EmpresaId, Faturado: g.Sum(x => x.Faturado)))
            .GroupBy(x => x.ClienteId)
            // O DESEMPATE PELA MENOR FILIAL faz duas apurações seguidas darem a mesma classe.
            .Select(g => g.OrderByDescending(x => x.Faturado).ThenBy(x => x.EmpresaId).First());

        var classePorCliente = new Dictionary<long, (ClasseDeCliente, decimal)>();

        foreach (var daFilial in melhorFilialPorCliente.GroupBy(x => x.EmpresaId))
        {
            var ordenados = daFilial.OrderByDescending(x => x.Faturado).ThenBy(x => x.ClienteId).ToList();
            var total = ordenados.Sum(x => x.Faturado);
            if (total <= 0) continue;

            var acumulado = 0m;
            foreach (var cliente in ordenados)
            {
                acumulado += cliente.Faturado;
                var fatia = acumulado / total;

                var classe = fatia <= CorteDaClasseA
                    ? ClasseDeCliente.A
                    : fatia <= CorteDaClasseB
                        ? ClasseDeCliente.B
                        : ClasseDeCliente.C;

                classePorCliente[cliente.ClienteId] = (classe, cliente.Faturado);
            }
        }

        return classePorCliente;
    }

    /// <summary>
    /// Se a venda muda a situação do cliente para <c>Cliente</c>.
    ///
    /// <para><c>Suspect</c> é "ainda não se sabe" e <c>Prospect</c> é "nunca comprou": a nota fiscal
    /// desmente os dois. <c>ClienteInativo</c> e <c>Encerrado</c> são decisões de uma pessoa sobre o
    /// relacionamento, e a carga não as desfaz — um encerrado que voltou a comprar é assunto da
    /// gerência, não de rotina noturna.</para>
    /// </summary>
    /// <param name="situacao">A situação atual.</param>
    internal static bool PromoveAoFaturar(SituacaoDoCliente situacao) =>
        situacao is SituacaoDoCliente.Suspect or SituacaoDoCliente.Prospect;

    // =============================================================================================
    // A natureza da contraparte sem cliente
    // =============================================================================================

    /// <summary>
    /// O que é a contraparte de uma nota que não achou cliente no CRM.
    ///
    /// <para>Só a fábrica e o grupo são reconhecidos, pela raiz do CNPJ — a lista mora em
    /// <see cref="ParceirosPorRaizDeCnpj"/>, que o parque de máquinas também usa. <b>Revenda não entra
    /// aqui</b>: não há lista confiável de concessionária. Quem não é fábrica nem grupo entra como
    /// cadastro faltando — que é o palpite certo na dúvida, porque é o que faz alguém olhar.</para>
    /// </summary>
    /// <param name="documento">CPF ou CNPJ, só dígitos.</param>
    internal static NaturezaDoParceiro ClassificarParceiro(string documento)
    {
        if (documento.Length == 0) return NaturezaDoParceiro.SemDocumento;

        return ParceirosPorRaizDeCnpj.PelaRaiz(documento) ?? NaturezaDoParceiro.ClienteNaoCadastrado;
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>
    /// A filial pelo CÓDIGO de seis dígitos, que é o mesmo de <c>organizacao.Empresa</c> — sem de-para.
    /// O cliente pelo documento.
    /// </summary>
    private static (long ClienteId, int EmpresaId) Resolver(
        FaturamentoParaCarga linha,
        IReadOnlyDictionary<string, long> porDocumento,
        IReadOnlyDictionary<string, int> porCodigoDeEmpresa) =>
        (porDocumento.GetValueOrDefault(linha.DocumentoDoCliente),
         linha.CodigoDaFilial is { Length: > 0 } codigo ? porCodigoDeEmpresa.GetValueOrDefault(codigo) : 0);

    /// <summary>
    /// Abre um contexto que grava na trilha como integração do Protheus (documento 41, fase 2).
    /// </summary>
    private CrmDbContext AbrirContextoDaCarga()
    {
        var contexto = abrirContexto();
        if (_sistemaId is { } sistema) contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistema);
        return contexto;
    }

    private async Task<int> GarantirSistemaAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var sistema = await contexto.Sistemas
            .FirstOrDefaultAsync(s => s.Codigo == LeitorDeClientesDoProtheus.CodigoDoSistema, ct);

        if (sistema is not null) return sistema.Id;

        sistema = Sistema.Criar(
            LeitorDeClientesDoProtheus.CodigoDoSistema, "Protheus (TOTVS) — ERP", "SQL Server, somente leitura");

        contexto.Sistemas.Add(sistema);
        await contexto.SaveChangesAsync(ct);

        return sistema.Id;
    }

    /// <summary>
    /// A filial em operação pelo código de seis dígitos — TODAS, inclusive as desativadas no CRM.
    ///
    /// <para>Guaíra, Ituverava e Monte Alto estão desativadas em <c>organizacao.Empresa</c> e seguem
    /// emitindo nota no ERP (medido em 24/09/2026). A venda delas é venda da empresa: descartá-la por
    /// um sinalizador do CRM faria o total da diretoria não fechar com o do ERP.</para>
    /// </summary>
    private async Task<Dictionary<string, int>> MapaDeEmpresasPorCodigoAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        return await contexto.Empresas.AsNoTracking()
            .ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);
    }

    /// <summary>O identificador de cada cliente carregado, pelo documento sem máscara.</summary>
    private async Task<Dictionary<string, long>> MapaDeClientesPorDocumentoAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var comDocumento = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Documento != null)
            .Select(c => new { c.Id, Documento = c.Documento!.Value.Numero })
            .ToListAsync(ct);

        var mapa = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var cliente in comDocumento) mapa.TryAdd(cliente.Documento, cliente.Id);

        return mapa;
    }

    /// <summary>
    /// O QUE A LEITURA COMPLETA CORRIGE ANTES DA JANELA CURTA, enquanto a gravação passa: mês novo, mês com valor mudado e mês
    /// removido. Mês igual não conta — e mês dentro da janela curta também não, porque a leitura curta já o veria.
    /// </summary>
    /// <param name="antes">Onde a janela curta começa.</param>
    private sealed class Conferidos(DateOnly antes)
    {
        private int _meses;
        private decimal _valor;

        /// <summary>Conta um mês, quando ele é anterior à janela curta e o valor mudou.</summary>
        /// <param name="competencia">O mês.</param>
        /// <param name="diferenca">O valor novo menos o antigo; o valor inteiro no mês novo ou removido.</param>
        public void Contar(DateOnly competencia, decimal diferenca)
        {
            if (competencia >= antes || diferenca == 0) return;
            _meses++;
            _valor += Math.Abs(diferenca);
        }

        /// <summary>O total da rodada.</summary>
        public ConferenciaDaJanelaCurta Fechar() => new(antes, _meses, decimal.Round(_valor, 2));
    }

    private void Decidir(string decisao, int quantas) =>
        _decisoes[decisao] = _decisoes.TryGetValue(decisao, out var atual) ? atual + quantas : quantas;

    /// <summary>
    /// O valor em reais cheios, para a linha de decisão da carga.
    ///
    /// <para>Sem centavos de propósito: quem lê o relatório da carga está decidindo se um descarte
    /// é resíduo ou é problema, e essa decisão se toma na ordem de grandeza.</para>
    /// </summary>
    private static string Reais(decimal valor) => valor.ToString("C0", CulturaDoRelatorio);

    private static readonly CultureInfo CulturaDoRelatorio = CultureInfo.GetCultureInfo("pt-BR");
}

/// <summary>
/// O que a atualização do faturamento fez.
/// </summary>
/// <param name="MesesNovos">Meses de cliente que entraram novos.</param>
/// <param name="MesesReapurados">Meses de cliente que já existiam e foram reapurados.</param>
/// <param name="MesesRemovidos">Linhas das duas tabelas que saíram da janela porque a origem não as tem mais.</param>
/// <param name="ClientesPromovidos">Clientes que passaram a <c>Cliente</c> por terem venda.</param>
/// <param name="Curva">Quantos clientes ficaram em cada classe.</param>
/// <param name="Decisoes">
/// O que foi descartado e por quê — com o valor em reais no próprio texto, porque a ordem de
/// grandeza é o que separa resíduo de problema.
/// </param>
/// <param name="Conferencia">Na leitura completa, o que ela corrigiu antes da janela curta; nula na curta.</param>
/// <param name="Modo">Curta ou completa.</param>
/// <param name="Desde">O primeiro dia lido; nulo quando o lote foi gravado sem passar pela rodada.</param>
internal sealed record ResumoDoFaturamento(
    int MesesNovos,
    int MesesReapurados,
    int MesesRemovidos,
    int ClientesPromovidos,
    IReadOnlyDictionary<ClasseDeCliente, int> Curva,
    IReadOnlyDictionary<string, int> Decisoes,
    ConferenciaDaJanelaCurta? Conferencia = null,
    ModoDaLeituraDoFaturamento Modo = ModoDaLeituraDoFaturamento.Completa,
    DateOnly? Desde = null);

/// <summary>
/// O QUE A LEITURA COMPLETA CORRIGIU ANTES DA JANELA CURTA (documento 54, passo 6) — mês novo, mês com valor mudado e mês
/// removido, com e sem cliente. É a medida de quanto a leitura curta, sozinha, teria deixado de ver.
/// </summary>
/// <param name="Antes">Onde a janela curta começa: só o que é anterior a esta competência conta.</param>
/// <param name="Meses">Quantos meses (por cliente ou documento e filial) mudaram.</param>
/// <param name="Valor">Quanto, em reais: o valor do mês novo ou removido, e a diferença do mês corrigido.</param>
internal sealed record ConferenciaDaJanelaCurta(DateOnly Antes, int Meses, decimal Valor);
