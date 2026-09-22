using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Integracoes;

/// <summary>Um item do painel, como a API o devolve: a situação em texto, como nos outros contratos.</summary>
/// <param name="Codigo">Chave estável do item.</param>
/// <param name="Nome">O que se mede.</param>
/// <param name="Unidade">O que se conta, no plural.</param>
/// <param name="Total">Quantos registros deviam ter o dado.</param>
/// <param name="Cobertos">Quantos têm.</param>
/// <param name="Percentual">Cobertos sobre o total; nulo quando não há total.</param>
/// <param name="Situacao">Completa, Parcial ou Vazia.</param>
/// <param name="Motivo">A frase que diz o que falta e para quê.</param>
/// <param name="ParaQue">O que o motor calcula com o dado.</param>
/// <param name="PeriodoInicial">O primeiro período com dado.</param>
/// <param name="PeriodoFinal">O último período com dado.</param>
/// <param name="Detalhe">O que ajuda a ler o número.</param>
public sealed record ItemDaCoberturaDoMotor(
    string Codigo,
    string Nome,
    string Unidade,
    int Total,
    int Cobertos,
    decimal? Percentual,
    string Situacao,
    string Motivo,
    string ParaQue,
    string? PeriodoInicial,
    string? PeriodoFinal,
    string? Detalhe);

/// <summary>Um bloco do painel: uma fonte e os itens dela.</summary>
/// <param name="Codigo">Chave estável do bloco.</param>
/// <param name="Nome">O nome do bloco.</param>
/// <param name="Fonte">De onde o dado vem.</param>
/// <param name="EhInterno">Se é dado da Tracbel, sujeito à fronteira de filial.</param>
/// <param name="Incompletos">Quantos itens não estão completos.</param>
/// <param name="Itens">Os itens.</param>
public sealed record BlocoDaCoberturaDoMotor(
    string Codigo,
    string Nome,
    string Fonte,
    bool EhInterno,
    int Incompletos,
    IReadOnlyList<ItemDaCoberturaDoMotor> Itens);

/// <summary>O painel de cobertura dos dados do motor.</summary>
/// <param name="MunicipiosDaAdr">O denominador dos itens por município.</param>
/// <param name="FiliaisNoAlcance">Quantas filiais o dado interno contou — a fronteira de filial de quem pergunta.</param>
/// <param name="TodasAsFiliais">Se quem pergunta está vendo todas as filiais (o dado interno é o da empresa inteira).</param>
/// <param name="Incompletos">Quantos itens não estão completos, somando todos os blocos.</param>
/// <param name="Grupos">Os blocos, na ordem das camadas do motor.</param>
public sealed record PainelDeCoberturaDoMotor(
    int MunicipiosDaAdr,
    int FiliaisNoAlcance,
    bool TodasAsFiliais,
    int Incompletos,
    IReadOnlyList<BlocoDaCoberturaDoMotor> Grupos);

/// <summary>
/// A COBERTURA DOS DADOS DO MOTOR (issue 150): o que cada fonte cobre, por cultura, mês e campo, medido pelo
/// servidor a cada leitura — para que ninguém programe o motor de mercado sobre dado que não existe, e para que
/// a falta fique visível para sempre, e não medida uma vez numa planilha.
///
/// <para><b>O dado interno respeita a fronteira de filial.</b> A resposta diz quantas filiais entraram na conta:
/// o mesmo painel lido por quem vê uma filial e por quem vê todas dá números diferentes, e a tela precisa dizer
/// qual é qual.</para>
/// </summary>
public sealed class ObterCoberturaDoMotor(IRepositorioDeCoberturaDoMotor repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Mede e devolve o painel.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelDeCoberturaDoMotor>>> ExecutarAsync(CancellationToken ct)
    {
        var contexto = acesso.Atual;
        var cobertura = await repositorio.MedirAsync(ct);

        var grupos = cobertura.Grupos
            .Select(g => new BlocoDaCoberturaDoMotor(
                g.Codigo, g.Nome, g.Fonte, g.EhInterno, g.Incompletos,
                [
                    .. g.Itens.Select(i => new ItemDaCoberturaDoMotor(
                        i.Codigo, i.Nome, i.Unidade, i.Total, i.Cobertos, i.Percentual, i.Situacao.ToString(), i.Motivo,
                        i.ParaQue, i.PeriodoInicial, i.PeriodoFinal, i.Detalhe))
                ]))
            .ToList();

        var painel = new PainelDeCoberturaDoMotor(
            cobertura.MunicipiosDaAdr,
            contexto.EmpresasVisiveis.Count,
            contexto.VeTodasAsFiliais,
            grupos.Sum(g => g.Incompletos),
            grupos);

        return Resultado<ComProcedencia<PainelDeCoberturaDoMotor>>.Ok(
            ComProcedencia<PainelDeCoberturaDoMotor>.DoNossoBanco(
                painel,
                "organizacao.ProducaoAgricolaNoMunicipio · organizacao.CotacaoDeProduto · organizacao.CustoDeProducao · " +
                "organizacao.CreditoRuralDeInvestimento · frota.VendaDeMaquina · frota.Equipamento · processo.VendaPerdida · comercial.Endereco",
                relogio));
    }
}
