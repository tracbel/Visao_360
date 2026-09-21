using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Potencial;

/// <summary>Um município da ADR, como a lista de escolha da percepção o mostra.</summary>
/// <param name="CodigoIbge">O código de sete dígitos — o que o formulário envia.</param>
/// <param name="Nome">O nome.</param>
public sealed record MunicipioDaAdrOpcao(int CodigoIbge, string Nome);

/// <summary>As listas de escolha dos formulários de parâmetros.</summary>
/// <param name="Produtos">Os produtos da PAM, com a área plantada na ADR — os mais plantados primeiro.</param>
/// <param name="Municipios">Os municípios vigentes da ADR, em ordem alfabética.</param>
public sealed record OpcoesDosParametros(IReadOnlyList<ProdutoDaPam> Produtos, IReadOnlyList<MunicipioDaAdrOpcao> Municipios);

/// <summary>
/// AS OPÇÕES DOS FORMULÁRIOS DE PARÂMETROS (issue 77). A tela oferece a lista, e não um campo de código: a
/// regra só aceita produto que está na PAM carregada, e a percepção é por município da ADR (D-P04, D-P14).
/// </summary>
public sealed class ListarOpcoesDosParametros(IRepositorioDeOpcoesDosParametros repositorio, IRelogio relogio)
{
    /// <summary>Lê as opções.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<OpcoesDosParametros>>> ExecutarAsync(CancellationToken ct)
    {
        var produtos = await repositorio.ListarProdutosDaPamAsync(ct);
        var municipios = await repositorio.ListarMunicipiosDaAdrAsync(ct);

        return Resultado<ComProcedencia<OpcoesDosParametros>>.Ok(
            ComProcedencia<OpcoesDosParametros>.DoNossoBanco(
                new OpcoesDosParametros(produtos, [.. municipios.Select(m => new MunicipioDaAdrOpcao(m.CodigoIbge, m.Nome))]),
                "organizacao.ProducaoAgricolaNoMunicipio · organizacao.MunicipioDaAreaDeAtuacao",
                relogio));
    }
}
