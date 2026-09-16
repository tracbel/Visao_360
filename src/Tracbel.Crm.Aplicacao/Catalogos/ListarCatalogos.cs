using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Catalogos;

/// <summary>
/// Entrega TODAS as listas que alimentam os campos de seleção do formulário.
///
/// É O ENDPOINT QUE TORNA A REGRA EXEQUÍVEL. O documento 16, seção 3.1, diz que campo com
/// catálogo não aceita digitação livre, e que a regra vale nos três momentos juntos: na tela, na
/// API e no banco. A tela só consegue cumprir a parte dela se tiver de onde puxar as opções —
/// e é isto aqui. Sem este endpoint, a alternativa real é o front embutir listas próprias, que
/// é exatamente como o legado chegou a ter FINALIZADO e FINALIZADA no mesmo catálogo.
///
/// DUAS FAMÍLIAS DE LISTA, e a diferença é declarada na resposta:
///
/// <list type="bullet">
///   <item><b>Catálogo de banco</b> — <c>metadado.Catalogo</c> e o catálogo de frota. Cresce sem
///   release: o negócio acrescenta item pela tela de Taxonomias. Vem com
///   <c>permiteItemNovo = true</c>.</item>
///   <item><b>Domínio fechado de código</b> — a situação do cliente, o tipo de pessoa, a situação
///   e a origem da máquina. São <c>enum</c> do domínio, com restrição de verificação no banco;
///   acrescentar item exige release E migração. Vem com <c>permiteItemNovo = false</c>, para o
///   front não oferecer um botão de "novo item" que não existe.</item>
/// </list>
/// </summary>
public sealed class ListarCatalogos(IRepositorioCatalogos repositorio, IRelogio relogio)
{
    /// <summary>O código do catálogo que lista as filiais da Tracbel.</summary>
    public const string CodigoDeEmpresa = "EMPRESA";

    /// <summary>O código do catálogo que lista os modelos de máquina.</summary>
    public const string CodigoDeModelo = "MODELO_EQUIPAMENTO";

    /// <summary>Executa a consulta.</summary>
    /// <param name="codigo">Um código de catálogo para trazer só ele; nulo traz todos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<CatalogoParaSelecao>>>> ExecutarAsync(
        string? codigo, CancellationToken ct)
    {
        var pedido = string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim().ToUpperInvariant();

        var doBanco = await repositorio.ListarAsync(pedido, ct);

        var todos = doBanco
            .Concat(DominiosFechados().Where(c => pedido is null || c.Codigo == pedido))
            .OrderBy(c => c.Codigo, StringComparer.Ordinal)
            .ToList();

        if (pedido is not null && todos.Count == 0)
            return Resultado<ComProcedencia<IReadOnlyList<CatalogoParaSelecao>>>.NaoEncontrado(
                $"Não existe catálogo com o código \"{pedido}\". " +
                "Consulte /api/v1/catalogos, sem código, para ver a lista completa.");

        return Resultado<ComProcedencia<IReadOnlyList<CatalogoParaSelecao>>>.Ok(
            ComProcedencia<IReadOnlyList<CatalogoParaSelecao>>.DoNossoBanco(
                todos, "metadado.Catalogo", relogio));
    }

    /// <summary>
    /// Os domínios que moram no CÓDIGO e não em tabela, expostos no mesmo formato dos outros.
    ///
    /// Eles aparecem aqui porque a tela precisa deles do mesmo jeito, e o formato ser o mesmo é
    /// o que permite o front ter UM componente de seleção em vez de dois caminhos.
    /// </summary>
    private static IEnumerable<CatalogoParaSelecao> DominiosFechados()
    {
        yield return DeEnum<SituacaoDoCliente>(
            "SITUACAO_CLIENTE", "Situação do cliente",
            "Em que ponto do relacionamento o cliente está. Domínio fechado no código e no banco.");

        yield return DeEnum<TipoDePessoa>(
            "TIPO_DE_PESSOA", "Tipo de pessoa",
            "Física (documento de 11 dígitos) ou jurídica (14 dígitos).");

        yield return DeEnum<SituacaoDoEquipamento>(
            "SITUACAO_EQUIPAMENTO", "Situação do equipamento",
            "Em que estado a máquina está.");

        yield return DeEnum<OrigemDoEquipamento>(
            "ORIGEM_EQUIPAMENTO", "Origem do equipamento",
            "Quem afirma que a máquina existe: o ERP (Protheus), o CEN pelo CRM, ou uma venda do ART.");

        yield return DeEnum<PorteDeMaquina>(
            "PORTE_DE_MAQUINA", "Porte da máquina",
            "O porte da classificação de produto — pequeno, médio, grande, ou não se aplica.");
    }

    private static CatalogoParaSelecao DeEnum<T>(string codigo, string nome, string descricao)
        where T : struct, Enum =>
        new(codigo, nome, descricao, PermiteItemNovo: false,
            [.. Enum.GetValues<T>()
                .Select((valor, ordem) => new ItemParaSelecao(
                    valor.ToString(), Rotular(valor.ToString()), (short)(ordem + 1), false))]);

    /// <summary>
    /// Transforma <c>ClienteInativo</c> em <c>Cliente inativo</c>.
    ///
    /// Não é enfeite: o código é o que vai para o histórico e não muda; o rótulo é o que a
    /// pessoa lê. Derivar o rótulo do nome do membro mantém os dois em sincronia sem uma segunda
    /// lista para alguém esquecer de atualizar.
    /// </summary>
    private static string Rotular(string nomeDoMembro)
    {
        var letras = nomeDoMembro
            .SelectMany((c, i) => i > 0 && char.IsUpper(c) ? [' ', char.ToLowerInvariant(c)] : new[] { c });

        return string.Concat(letras);
    }
}
