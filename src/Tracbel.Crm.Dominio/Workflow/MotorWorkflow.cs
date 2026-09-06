using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Dominio.Workflow;

/// <summary>
/// Contrato de um efeito de regra.
///
/// ESTE É O PONTO DE EXTENSÃO MAIS USADO DO SISTEMA. Para acrescentar uma capacidade nova
/// ao motor de workflow, um dev cria UMA classe que implementa esta interface e registra o
/// nome dela em <c>wf.Regra.Efeito</c>. Não se toca em nenhum arquivo existente —
/// nem no motor, nem nos outros efeitos.
/// </summary>
public interface IEfeitoRegra
{
    /// <summary>Nome gravado em <c>wf.Regra.Efeito</c>. Precisa bater exatamente.</summary>
    string Nome { get; }

    /// <summary>
    /// Executa o efeito.
    ///
    /// NÃO lança exceção para caso de negócio: devolve <see cref="ResultadoEfeito"/>, porque
    /// "não disparou" é informação a registrar, não erro a engolir.
    /// </summary>
    Task<ResultadoEfeito> ExecutarAsync(ContextoRegra contexto, CancellationToken ct);
}

/// <summary>
/// Avalia todas as regras disparadas por um evento e registra o que aconteceu com cada uma.
///
/// CONTRATO: este motor NUNCA lança por causa de regra que não disparou. Ele devolve o
/// relatório. Só regra marcada como <see cref="Regra.EhCritica"/> aborta a transação — e
/// mesmo assim, depois de registrar o motivo.
/// </summary>
public sealed class MotorWorkflow(
    IRepositorioRegras regras,
    IAvaliadorCondicao avaliador,
    IEnumerable<IEfeitoRegra> efeitos,
    IRegistroExecucaoRegra registro,
    IRelogio relogio)
{
    private readonly IRepositorioRegras _regras = regras;
    private readonly IAvaliadorCondicao _avaliador = avaliador;
    private readonly IReadOnlyList<IEfeitoRegra> _efeitos = efeitos.ToList();
    private readonly IRegistroExecucaoRegra _registro = registro;
    private readonly IRelogio _relogio = relogio;

    /// <summary>
    /// Processa um evento: carrega as regras do gatilho, avalia cada uma na ordem, e grava
    /// o resultado de TODAS — inclusive as que não dispararam.
    /// </summary>
    public async Task<RelatorioExecucao> AvaliarAsync(ContextoRegra contexto, CancellationToken ct = default)
    {
        // Correlaciona tudo que aconteceu neste evento. É por este identificador que o
        // suporte reconstrói "o que o sistema fez quando o vendedor clicou em concluir".
        var correlacaoId = Guid.NewGuid();
        var relatorio = new RelatorioExecucao(correlacaoId);

        var candidatas = await _regras.ObterPorGatilhoAsync(
            contexto.Evento, contexto.ResultadoId, contexto.TipoProcessoId, ct);

        foreach (var regra in candidatas.OrderBy(r => r.Ordem).ThenBy(r => r.Id))
        {
            var inicio = _relogio.Agora;

            try
            {
                // ---- 1. Regra desligada ----
                if (!regra.EstaAtiva)
                {
                    await GravarAsync(regra, contexto, correlacaoId, ResultadoRegra.RegraInativa,
                        "Regra está desativada.", inicio, ct);
                    relatorio.RegistrarSemEfeito();
                    continue;
                }

                // ---- 2. Condição ----
                // [V] IV_AcaoAutoCtrl está VAZIA no Vórtice: a regra não tem condição.
                // Sem condição, a única saída é replicar a regra por filial — 5.958 regras
                // para 1.262 pares distintos. Uma expressão elimina essa explosão.
                if (!string.IsNullOrWhiteSpace(regra.Condicao))
                {
                    var avaliacao = await _avaliador.AvaliarAsync(regra.Condicao, contexto, ct);
                    if (!avaliacao.Verdadeira)
                    {
                        await GravarAsync(regra, contexto, correlacaoId, ResultadoRegra.CondicaoFalsa,
                            avaliacao.Explicacao, inicio, ct);
                        relatorio.RegistrarSemEfeito();
                        continue;
                    }
                }

                // ---- 3. Efeito ----
                var efeito = _efeitos.FirstOrDefault(e =>
                    string.Equals(e.Nome, regra.Efeito, StringComparison.Ordinal));

                if (efeito is null)
                {
                    // A configuração aponta para um efeito que não existe no código.
                    // É erro de operação, e precisa gritar.
                    var motivo = $"Efeito '{regra.Efeito}' não tem implementação registrada.";
                    await GravarAsync(regra, contexto, correlacaoId, ResultadoRegra.Erro, motivo, inicio, ct);
                    relatorio.RegistrarFalha(regra.Codigo, motivo);
                    continue;
                }

                var resultado = await efeito.ExecutarAsync(contexto with { Regra = regra }, ct);

                await GravarAsync(regra, contexto, correlacaoId,
                    resultado.Aplicou ? ResultadoRegra.Disparou : ResultadoRegra.SemDestinatario,
                    resultado.Motivo, inicio, ct, resultado.TarefaCriadaId, resultado.DestinatarioId);

                relatorio.Registrar(regra.Codigo, resultado);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Uma regra quebrada NÃO derruba as outras.
                // [V] no Vórtice as ações 899 e 900 caem JUNTAS enquanto 894 e 814 continuam
                // gerando normalmente — sinal de que o motor não isola falha por regra.
                await GravarAsync(regra, contexto, correlacaoId, ResultadoRegra.Erro,
                    $"Exceção: {ex.Message}", inicio, ct);

                relatorio.RegistrarFalha(regra.Codigo, ex.Message);

                if (regra.EhCritica) throw;   // crítica aborta — mas já ficou registrado
            }
        }

        return relatorio;
    }

    private Task GravarAsync(
        Regra regra, ContextoRegra ctx, Guid correlacaoId, ResultadoRegra resultado,
        string? motivo, DateTime inicio, CancellationToken ct,
        long? tarefaCriadaId = null, long? destinatarioId = null)
        => _registro.GravarAsync(new ExecucaoRegra
        {
            RegraId = regra.Id,
            CorrelacaoId = correlacaoId,
            Evento = ctx.Evento,
            ProcessoId = ctx.ProcessoId,
            TarefaId = ctx.TarefaId,
            InteracaoId = ctx.InteracaoId,
            Resultado = resultado,
            Motivo = motivo,
            TarefaCriadaId = tarefaCriadaId,
            DestinatarioResolvidoId = destinatarioId,
            DuracaoMs = (int)(_relogio.Agora - inicio).TotalMilliseconds,
            ExecutadoEm = _relogio.Agora
        }, ct);
}
