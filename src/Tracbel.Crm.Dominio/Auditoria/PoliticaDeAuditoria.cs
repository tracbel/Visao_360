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

        // IDENTIDADE E ACESSO (fase 3, achado C-8): quem mudou a filial de casa ou o gestor de alguém, quem
        // concedeu ou revogou um perfil, quem mexeu no que um perfil concede. É a pergunta "por que fulano
        // passou a ver isto?", e ela precisa de resposta com autor e data. A revogação é exclusão da linha
        // de UsuarioPerfil — e a trilha grava a exclusão com os valores anteriores.
        ["Usuario"] = ["EmpresaId", "GestorId", "EstaAtivo", "Natureza", "AguardandoLiberacaoDesde"],
        ["UsuarioPerfil"] = ["UsuarioId", "PerfilId", "EmpresaId", "ExpiraEm", "Justificativa", "RevogadaEm", "MotivoDaRevogacao"],
        ["Perfil"] = ["Codigo", "Nome", "EstaAtivo", "EhPadrao"],
        ["PerfilPermissao"] = ["PerfilId", "CodigoPermissao", "Profundidade"],

        // Preço de mercado e dólar: o valor. A série "só cresce" (issue 66), e a fonte que revisa um
        // mês já gravado muda um número que alimenta o potencial — a trilha guarda o anterior.
        ["CotacaoDeProduto"] = ["ValorEmReais"],
        ["CotacaoDoDolar"] = ["ReaisPorDolar"],

        // Custo de produção: as camadas que a rentabilidade usa. A CONAB revisa série antiga, e o
        // número que muda mexe na margem da issue 73 — a trilha guarda o anterior.
        ["CustoDeProducao"] = ["CustoOperacionalHa", "CustoTotalHa", "CustoOperacionalUnidade", "CustoTotalUnidade"],

        // Crédito rural: o valor. O Banco Central acrescenta contrato registrado com atraso aos meses
        // recentes, e o índice de crédito da issue 73 muda com isso — a trilha guarda o anterior.
        //
        // E A CHAVE INTEIRA (issue 153). O ano relido espelha a fonte: a linha que o Banco Central reclassificou
        // (a mesma operação com outra fonte de recurso, por exemplo) é apagada, senão seria contada duas vezes. Com só o
        // valor na trilha, a exclusão dizia "saiu R$ 250 mil" sem dizer de onde. A chave não muda numa alteração —
        // ela só aparece na trilha quando a linha sai, e aí diz qual linha era.
        ["CreditoRuralDeInvestimento"] =
        [
            "Valor", "Area", "MunicipioId", "CodigoMunicipioBcb", "Ano", "Mes", "CodigoProduto", "CodigoPrograma",
            "CodigoSubprograma", "CodigoFonte", "CodigoSeguro", "Atividade", "CodigoModalidade"
        ],

        // AS FONTES DO IBGE (issue 153): a revisão substitui o valor, e a trilha guarda o anterior. O IBGE revisa a PAM
        // do ano anterior quando publica a nova; sem a trilha, o potencial calculado em agosto mudava em outubro sem
        // rastro de por quê. A inclusão feita pela carga não entra (ver RegistraInclusao) — só o que ela muda depois.
        ["ProducaoAgricolaNoMunicipio"] =
            ["ProdutoNome", "AreaPlantadaHectares", "AreaColhidaHectares", "QuantidadeProduzida", "ValorDaProducaoMilReais"],
        ["ProducaoAgricolaNoEstado"] =
            ["ProdutoNome", "AreaPlantadaHectares", "AreaColhidaHectares", "QuantidadeProduzida", "ValorDaProducaoMilReais"],
        ["FrotaDeTratoresNoMunicipio"] = ["PotenciaNome", "Tratores", "EstabelecimentosComTrator"],
        ["EstabelecimentosPorAreaNoMunicipio"] = ["GrupoDeAreaNome", "Estabelecimentos"],
        ["RebanhoNoMunicipio"] = ["RebanhoNome", "Cabecas"],
        ["AreaTerritorialDoMunicipio"] = ["AreaKm2"],

        // O TOTAL PUBLICADO DO ESTADO (issue 155) é o denominador de todo "% de São Paulo": quando o
        // IBGE revisa a linha da UF, a fatia da região muda sem que nada tenha mudado na região. A
        // trilha guarda o valor anterior, como nas tabelas municipais da issue 153.
        ["MedidaDoIbgeNoEstado"] = ["Valor", "CategoriaNome"],

        // A USINA DA ANP (issue 153): o que a ANP corrige e a saída da lista. O mês de referência fica de fora — ele
        // anda todo mês em todas as usinas, e a trilha viraria o carimbo do Vórtice.
        ["UsinaDeEtanol"] =
            ["RazaoSocial", "MunicipioId", "CapacidadeDeAnidroM3Dia", "CapacidadeDeHidratadoM3Dia", "EncerradaEm"],

        // O DE-PARA DE MUNICÍPIO (issue 154): para onde a chave de uma fonte aponta. Um par errado leva a produção,
        // o crédito ou o custo de um município inteiro para o vizinho — e a pergunta "quem apontou o código 3549300
        // do SICOR para São José do Rio Preto, e quando?" precisa de resposta. A chave e a fonte não entram: são a
        // identidade do par, e mudar uma delas é outro par.
        ["CorrespondenciaDeMunicipio"] = ["MunicipioId", "Forma", "TextoNaFonte"],

        // PARÂMETROS DO POTENCIAL (issue 71): "parâmetro alterado gera trilha com o autor". A vigência nasce
        // pela mão de uma pessoa — a inclusão entra na trilha inteira — e depois só muda para ser revogada.
        // Parâmetro errado muda o potencial inteiro; a pergunta "quem pôs 20 ha no café, e quando?" tem resposta.
        ["RegraDePotencial"] =
        [
            "ProdutoCodigoIbge", "HectaresPorMaquina", "AnosDeRenovacao", "ModeloDeReferencia", "Situacao",
            "VigenteDesde", "Justificativa", "RevogadoEm", "MotivoDaRevogacao"
        ],
        ["ParametroDoPotencial"] =
        [
            "MesesDaJanela", "PesoDosContratosNoCredito", "LimiteDeRetracao", "LimiteDeAquecimento",
            "LimiteDeSuperaquecimento", "NomeDaFaixaIntermediaria", "LimiteDaPercepcao", "PesoDoIndicadorDePreco",
            "PesoDoIndicadorDeCredito", "PesoDoIndicadorComercial", "FatorMinimo", "FatorMaximo",
            "VigenteDesde", "Justificativa", "RevogadoEm", "MotivoDaRevogacao"
        ],
        ["PercepcaoDoGestor"] = ["MunicipioId", "Percentual", "VigenteDesde", "Justificativa", "RevogadoEm", "MotivoDaRevogacao"],

        // INTEGRAÇÕES (issue 136): quem mudou o endereço, o usuário, o monitoramento ou a agenda — e QUANDO a
        // credencial foi trocada. A senha protegida NÃO entra: a trilha guarda o antes e o depois em texto, e a
        // senha não pode estar em texto em lugar nenhum. O resultado dos testes e das execuções também não: é
        // rotina, e tem histórico próprio.
        ["Conexao"] =
        [
            "Endereco", "Porta", "Banco", "Objeto", "Usuario", "NomeDoCabecalho", "StatusEsperado", "MinutosEntreVerificacoes",
            "EstaAtiva", "SegredoAlteradoEm"
        ],
        ["Rotina"] = ["Cadencia", "Mes", "Dia", "Hora", "IntervaloMinutos", "EstaLigada"],

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
