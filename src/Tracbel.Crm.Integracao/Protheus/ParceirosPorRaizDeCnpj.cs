using Tracbel.Crm.Dominio.Comercial;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// AS RAÍZES DE CNPJ QUE NÃO SÃO CLIENTE — a fábrica e as empresas do próprio grupo.
///
/// <para><b>A raiz, e não o nome.</b> "TRACBEL AGRO NOR.", "TRACBEL AGRO NORTE" e "TRACBEL AGRO NOROESTE" são a
/// mesma empresa escrita de três jeitos; a raiz <c>09507371</c> é uma só.</para>
///
/// <para><b>Uma lista só, para o faturamento e o parque.</b> Até 24/09/2026 ela morava dentro da carga do
/// faturamento, que classifica a contraparte da nota sem cliente. O parque de máquinas precisa da mesma resposta —
/// a máquina cujo dono atual no Protheus é a própria Tracbel voltou para a revenda, e não é parque de cliente — e
/// uma segunda lista escrita à mão é uma lista que um dia discorda da primeira.</para>
///
/// <para>Medido em três anos de nota: a fábrica levou R$ 96,6 milhões e a empresa irmã R$ 27,4 milhões, <b>tudo com
/// CFOP 5102 — idêntico a uma venda</b>. Só o documento separa. No cadastro de veículos do Protheus (24/09/2026), as
/// duas raízes do grupo são donas de 3.215 máquinas — 2.952 e 263 —, a maioria estoque e pedido de fábrica.</para>
/// </summary>
public static class ParceirosPorRaizDeCnpj
{
    /// <summary>
    /// A chave da configuração que substitui as raízes do grupo — a variável de ambiente
    /// <c>ParqueProtheus__RaizesDoGrupo</c> no servidor.
    /// </summary>
    public const string ChaveDaConfiguracao = "ParqueProtheus:RaizesDoGrupo";

    private static readonly Dictionary<string, NaturezaDoParceiro> NaturezaPorRaiz = new(StringComparer.Ordinal)
    {
        ["89674782"] = NaturezaDoParceiro.Fabrica,        // John Deere Brasil
        ["09507371"] = NaturezaDoParceiro.EmpresaDoGrupo, // Tracbel Agro Norte
        ["03258870"] = NaturezaDoParceiro.EmpresaDoGrupo  // Tracbel — quinze cadastros na mesma raiz
    };

    /// <summary>As raízes das empresas do grupo — o padrão que a configuração do parque pode substituir.</summary>
    public static IReadOnlySet<string> RaizesDoGrupo { get; } =
        NaturezaPorRaiz.Where(p => p.Value == NaturezaDoParceiro.EmpresaDoGrupo).Select(p => p.Key).ToHashSet(StringComparer.Ordinal);

    /// <summary>A raiz do CNPJ — os oito primeiros dígitos —, ou nula para CPF e documento incompleto.</summary>
    /// <param name="documento">O documento, só dígitos.</param>
    public static string? Raiz(string? documento) => documento is { Length: 14 } ? documento[..8] : null;

    /// <summary>
    /// A natureza pela raiz: fábrica ou empresa do grupo; nula quando a raiz não está na lista. Revenda não entra
    /// aqui — não há lista confiável de concessionária.
    /// </summary>
    /// <param name="documento">CPF ou CNPJ, só dígitos.</param>
    public static NaturezaDoParceiro? PelaRaiz(string? documento) =>
        Raiz(documento) is { } raiz && NaturezaPorRaiz.TryGetValue(raiz, out var natureza) ? natureza : null;

    /// <summary>
    /// A LISTA DO GRUPO, configurável: as raízes que a configuração declara (<c>ParqueProtheus__RaizesDoGrupo</c>,
    /// oito dígitos separados por vírgula), ou o padrão quando ela não declara nenhuma. O que não tem oito dígitos é
    /// ignorado — uma raiz mal digitada não pode tirar do parque a máquina de um cliente.
    /// </summary>
    /// <param name="configuradas">O texto da configuração.</param>
    public static IReadOnlySet<string> RaizesDoGrupoConfiguradas(string? configuradas)
    {
        var lidas = (configuradas ?? string.Empty)
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(r => new string([.. r.Where(char.IsAsciiDigit)]))
            .Where(r => r.Length == 8)
            .ToHashSet(StringComparer.Ordinal);

        return lidas.Count > 0 ? lidas : RaizesDoGrupo;
    }
}
