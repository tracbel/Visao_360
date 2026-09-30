using System.Globalization;
using FluentAssertions;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Relacionamento;

/// <summary>
/// O MOTIVO DO FUNIL EM PORTUGUÊS, qualquer que seja a máquina (30/09/2026, a caixa laranja do Pipeline na maquete): "Só
/// 653 de 13.838 processos abertos declaram valor (4,7%)" — com o ponto do milhar e a vírgula do decimal, e não na cultura
/// do servidor.
///
/// <para><b>Por que um dublê do repositório aqui</b>: o que está sob prova é o TEXTO que o caso de uso escreve a partir
/// das contagens, e não a consulta. O agrupamento no banco tem teste de ponta a ponta em
/// <c>EndpointsDeRelacionamentoTestes</c>.</para>
/// </summary>
public sealed class MotivoDoFunilTestes
{
    private sealed class RelogioFixo(DateTime agora) : IRelogio
    {
        public DateTime Agora { get; } = agora;
    }

    /// <summary>Devolve as fatias que recebeu, e só isso.</summary>
    private sealed class RepositorioDeFatias(IReadOnlyList<FatiaDoFunil> fatias) : IRepositorioProcessos
    {
        public Task<IReadOnlyList<FatiaDoFunil>> ResumirFunilAsync(int? tipoProcessoId, CancellationToken ct) =>
            Task.FromResult(fatias);

        public Task<PaginaDe<ProcessoComContexto>> ListarAsync(ConsultaDeProcessos consulta, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<ProcessoComContexto?> ObterAsync(Guid chavePublica, CancellationToken ct) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<ContagemPorRotulo>> ResumirPerdasAsync(CancellationToken ct) =>
            throw new NotSupportedException();
    }

    private static async Task<string> MotivoAsync(params FatiaDoFunil[] fatias)
    {
        var caso = new ObterFunil(new RepositorioDeFatias(fatias), new RelogioFixo(new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc)));
        var resultado = await caso.ExecutarAsync(CancellationToken.None);
        return resultado.Valor!.Dados.MetricasSemDado.Single().Motivo;
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("pt-BR")]
    [InlineData("")]
    public async Task O_motivo_escreve_os_numeros_em_portugues_em_qualquer_cultura_da_maquina(string cultura)
    {
        var antes = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);
        try
        {
            var motivo = await MotivoAsync(
                new FatiaDoFunil("VENDA", "Venda", "APRESENTACAO", "Apresentação", 1, 13_185, 600, 9_000_000m),
                new FatiaDoFunil("VENDA", "Venda", "NEGOCIACAO", "Negociação", 2, 653, 53, 1_000_000m));

            motivo.Should().StartWith("Só 653 de 13.838 processos abertos declaram valor (4,7%).");
        }
        finally
        {
            CultureInfo.CurrentCulture = antes;
        }
    }

    [Fact]
    public async Task Sem_valor_nenhum_o_total_tambem_vem_com_o_milhar()
    {
        var motivo = await MotivoAsync(new FatiaDoFunil("VENDA", "Venda", "APRESENTACAO", "Apresentação", 1, 2_500, 0, null));

        motivo.Should().StartWith("Nenhum dos 2.500 processos abertos declara valor.");
    }
}
