namespace Tracbel.Crm.Dominio.Auditoria;

/// <summary>
/// O QUE SE AUDITA — a lista de adesão explícita, em código e versionada com as entidades.
///
/// <para><b>Por que em código, e não numa tabela.</b> A tabela <c>auditoria.CampoAuditado</c>
/// existia para isso e nunca teve uma linha: uma política que mora num lugar que ninguém preenche
/// é, na prática, "não auditar nada". Aqui a política vai junto com a entidade no mesmo commit —
/// quem acrescenta um campo de negócio vê a lista ao lado e decide, e a revisão do PR vê a decisão
/// (documento 41, fase 2).</para>
///
/// <para><b>Por que lista fechada, e não "tudo".</b> [V] O Vórtice audita tudo, e por isso o log
/// ocupa 42% do banco; 96,8% de uma das tabelas de log são recarimbos de um job. Entram aqui os
/// campos que alguém pode querer contestar ("quem mudou o dono deste cliente?"). Ficam de fora os
/// carimbos (<c>AlteradoEm</c>, <c>Versao</c>), o que é recalculado por rotina (classe ABC,
/// faturamento apurado) e o que chega por telemetria (horímetro).</para>
///
/// <para><b>O que esta lista NÃO decide:</b> o prazo de retenção. Ele é a decisão D-10 do
/// documento 41, de Ricardo com o jurídico, e está pendente.</para>
/// </summary>
public static class PoliticaDeAuditoria
{
    /// <summary>O campo de exclusão lógica — auditado em toda entidade da lista que o tenha.</summary>
    public const string CampoDeExclusaoLogica = "ExcluidoEm";

    private static readonly Dictionary<string, HashSet<string>> Campos = new(StringComparer.Ordinal)
    {
        // Cadastro de cliente: o que a tela de cliente altera, mais o dono e a matriz. A classe
        // ABC fica de fora — é recalculada pela curva, e cada rodada mudaria milhares de linhas.
        ["Cliente"] =
        [
            "NomeRazao", "NomeFantasia", "TipoDePessoa", "Documento", "InscricaoEstadual",
            "AtividadeEconomica", "Situacao", "MotivoInativacaoId", "ProprietarioId",
            "ClienteMatrizId", "OrigemId", CampoDeExclusaoLogica
        ],

        // Máquina do cliente: identificação, dono, classificação e ciclo de vida. O horímetro fica
        // de fora — é leitura, não decisão.
        ["Equipamento"] =
        [
            "ClienteId", "ModeloId", "LinhaDeProdutoId", "Chassi", "NumeroSerie", "Placa",
            "AnoFabricacao", "AnoModelo", "EnderecoId", "EquipamentoPaiId", "EquipamentoSubstitutoId",
            "Situacao", "VendidoEm", "GarantiaAte", CampoDeExclusaoLogica
        ],

        // Endereço: só o município do catálogo — é o que as correções de grafia cortada mudam, e
        // era o que a carga já registrava à mão.
        ["Endereco"] = ["MunicipioId"],

        // Município: o reconhecimento no IBGE muda o código e, às vezes, a grafia do nome.
        ["Municipio"] = ["CodigoIbge", "Nome"],

        // Preço de mercado e dólar: o valor. A série "só cresce" (issue 66), e a fonte que revisa um
        // mês já gravado muda um número que alimenta o potencial — a trilha guarda o anterior.
        ["CotacaoDeProduto"] = ["ValorEmReais"],
        ["CotacaoDoDolar"] = ["ReaisPorDolar"],

        // Custo de produção: as camadas que a rentabilidade usa. A CONAB revisa série antiga, e o
        // número que muda mexe na margem da issue 73 — a trilha guarda o anterior.
        ["CustoDeProducao"] = ["CustoOperacionalHa", "CustoTotalHa", "CustoOperacionalUnidade", "CustoTotalUnidade"],

        // Crédito rural: o valor. O Banco Central acrescenta contrato registrado com atraso aos meses
        // recentes, e o índice de crédito da issue 73 muda com isso — a trilha guarda o anterior.
        ["CreditoRuralDeInvestimento"] = ["Valor"],

        // Venda de máquina: o retrato que a origem pode reescrever a cada leitura
        // (VendaDeMaquina.AtualizarDaOrigem), campo por campo.
        ["VendaDeMaquina"] =
        [
            "EmpresaId", "EmpresaDoFaturamentoId", "VendidaEm", "FaturadaEm", "EntregueEm",
            "NumeroDoPedido", "NumeroDaNotaFiscal", "SituacaoNaOrigem", "GestaoNaOrigem",
            "VendaDireta", "RepasseDireto", "Quantidade", "LinhaNaOrigem", "ProdutoNaOrigem",
            "UnidadeNaOrigem", "UnidadeDoFaturamentoNaOrigem"
        ]
    };

    /// <summary>As entidades auditadas, por nome de tipo.</summary>
    public static IReadOnlyCollection<string> Entidades => Campos.Keys;

    /// <summary>Os campos auditados de uma entidade; vazio quando ela não é auditada.</summary>
    /// <param name="entidade">O nome do tipo da entidade.</param>
    public static IReadOnlySet<string> CamposDe(string entidade) =>
        Campos.TryGetValue(entidade, out var campos) ? campos : new HashSet<string>();

    /// <summary>Este campo desta entidade entra na trilha?</summary>
    /// <param name="entidade">O nome do tipo da entidade.</param>
    /// <param name="campo">O nome da propriedade.</param>
    public static bool Audita(string entidade, string campo) =>
        Campos.TryGetValue(entidade, out var campos) && campos.Contains(campo);

    /// <summary>
    /// O NASCIMENTO DO REGISTRO ENTRA NA TRILHA?
    ///
    /// <para>Só quando é uma pessoa que cria. O registro que nasce de integração ou de importação já
    /// tem o rastro da origem gravado ao lado dele (<c>integracao.RegistroDeOrigem</c> e
    /// <c>integracao.ChaveExterna</c>, com o valor como veio). Repetir cada campo na trilha só
    /// multiplicaria o volume — a primeira leitura do ART criaria dezenas de milhares de linhas
    /// dizendo "era nada, passou a ser o que a origem mandou". O que a integração MUDA depois, esse
    /// sim, entra.</para>
    ///
    /// <para>É a mesma regra que as cargas já seguiam quando gravavam a trilha à mão: só registravam
    /// a alteração do que já existia.</para>
    /// </summary>
    /// <param name="origem">De onde veio a gravação.</param>
    public static bool RegistraInclusao(OrigemDaOperacao origem) => origem == OrigemDaOperacao.Usuario;
}
