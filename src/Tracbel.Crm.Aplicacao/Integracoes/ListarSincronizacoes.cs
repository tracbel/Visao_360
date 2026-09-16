using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Integracoes;

/// <summary>
/// O registro das sincronizações para a administração (Configurações › Admin TI › Integrações): a
/// última execução de cada fluxo, o último sucesso e as execuções recentes (documento 35, seção 11).
/// </summary>
public sealed class ListarSincronizacoes(IRepositorioSincronizacoes repositorio, IRelogio relogio)
{
    private const int PadraoDeExecucoes = 10;
    private const int TetoDeExecucoes = 50;

    /// <summary>Executa a consulta.</summary>
    /// <param name="execucoes">Quantas execuções trazer por fluxo. Padrão 10, teto 50.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<SituacaoDaSincronizacao>>>> ExecutarAsync(int? execucoes, CancellationToken ct)
    {
        if (execucoes is < 1 or > TetoDeExecucoes)
        {
            var erros = new ColetorDeErros();
            erros.Registrar("execucoes", $"Informe de 1 a {TetoDeExecucoes}.", execucoes?.ToString());
            return erros.Recusar<ComProcedencia<IReadOnlyList<SituacaoDaSincronizacao>>>("A consulta tem parâmetros que não valem.");
        }

        var situacoes = await repositorio.ListarAsync(execucoes ?? PadraoDeExecucoes, ct);

        return Resultado<ComProcedencia<IReadOnlyList<SituacaoDaSincronizacao>>>.Ok(
            ComProcedencia<IReadOnlyList<SituacaoDaSincronizacao>>.DoNossoBanco(situacoes, "integracao.ExecucaoDeSincronizacao", relogio));
    }
}
