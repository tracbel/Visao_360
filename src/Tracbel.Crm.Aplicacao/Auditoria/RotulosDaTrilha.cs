using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Auditoria;

/// <summary>
/// COMO A TELA DE AUDITORIA FALA (issue 135): o nome de cada entidade e de cada campo auditado, e a tabela a que
/// cada identificador gravado aponta.
///
/// <para><b>Anda junto com <see cref="PoliticaDeAuditoria"/>.</b> Quem acrescenta um campo à política acrescenta o
/// rótulo aqui — e o teste <c>RotulosDaTrilhaTestes</c> recusa campo auditado sem rótulo, para a tela nunca mostrar
/// "EmpresaDoFaturamentoId" a quem procura quem mudou uma venda.</para>
/// </summary>
public static class RotulosDaTrilha
{
    private static readonly Dictionary<string, string> Entidades = new(StringComparer.Ordinal)
    {
        ["Cliente"] = "Cliente",
        ["Equipamento"] = "Equipamento",
        ["Endereco"] = "Endereço",
        ["Municipio"] = "Município",
        ["Usuario"] = "Conta de usuário",
        ["UsuarioPerfil"] = "Concessão de perfil",
        ["Perfil"] = "Perfil",
        ["PerfilPermissao"] = "Permissão de perfil",
        ["CotacaoDeProduto"] = "Cotação de produto",
        ["CotacaoDoDolar"] = "Cotação do dólar",
        ["CustoDeProducao"] = "Custo de produção",
        ["CreditoRuralDeInvestimento"] = "Crédito rural",
        ["ProducaoAgricolaNoMunicipio"] = "Produção agrícola no município (PAM)",
        ["ProducaoAgricolaNoEstado"] = "Produção agrícola no estado (PAM)",
        ["FrotaDeTratoresNoMunicipio"] = "Tratores no município (Censo)",
        ["EstabelecimentosPorAreaNoMunicipio"] = "Propriedades por tamanho (Censo)",
        ["RebanhoNoMunicipio"] = "Rebanho no município (PPM)",
        ["AreaTerritorialDoMunicipio"] = "Área territorial do município",
    ["MedidaDoIbgeNoEstado"] = "Total publicado do estado (IBGE)",
        ["UsinaDeEtanol"] = "Usina de etanol (ANP)",
        ["CorrespondenciaDeMunicipio"] = "Município da fonte (de-para)",
        ["RegraDePotencial"] = "Regra do potencial por cultura",
        ["ParametroDoPotencial"] = "Parâmetros gerais do potencial",
        ["PercepcaoDoGestor"] = "Percepção do gestor",
        ["VendaDeMaquina"] = "Venda de máquina",
        ["Conexao"] = "Conexão de integração",
        ["Rotina"] = "Rotina do servidor"
    };

    // Os campos que se repetem em várias entidades, com o mesmo sentido.
    private static readonly Dictionary<string, string> Comuns = new(StringComparer.Ordinal)
    {
        [PoliticaDeAuditoria.CampoDeExclusaoLogica] = "Excluído em",
        ["Situacao"] = "Situação",
        ["Nome"] = "Nome",
        ["EstaAtivo"] = "Ativo",
        ["Justificativa"] = "Justificativa",
        ["VigenteDesde"] = "Vigente desde",
        ["RevogadoEm"] = "Revogado em",
        ["MotivoDaRevogacao"] = "Motivo da revogação"
    };

    private static readonly Dictionary<string, Dictionary<string, string>> Campos = new(StringComparer.Ordinal)
    {
        ["Cliente"] = new(StringComparer.Ordinal)
        {
            ["NomeRazao"] = "Razão social", ["NomeFantasia"] = "Nome fantasia", ["TipoDePessoa"] = "Tipo de pessoa",
            ["Documento"] = "CPF/CNPJ", ["InscricaoEstadual"] = "Inscrição estadual", ["AtividadeEconomica"] = "Atividade econômica",
            ["MotivoInativacaoId"] = "Motivo da inativação", ["ProprietarioId"] = "Responsável", ["ClienteMatrizId"] = "Matriz",
            ["OrigemId"] = "Origem do cadastro"
        },
        ["Equipamento"] = new(StringComparer.Ordinal)
        {
            ["ClienteId"] = "Cliente", ["ModeloId"] = "Modelo", ["LinhaDeProdutoId"] = "Linha de produto", ["Chassi"] = "Chassi",
            ["NumeroSerie"] = "Número de série", ["Placa"] = "Placa", ["AnoFabricacao"] = "Ano de fabricação", ["AnoModelo"] = "Ano do modelo",
            ["EnderecoId"] = "Endereço", ["EquipamentoPaiId"] = "Equipamento principal", ["EquipamentoSubstitutoId"] = "Substituído por",
            ["VendidoEm"] = "Vendido em", ["GarantiaAte"] = "Garantia até"
        },
        ["Endereco"] = new(StringComparer.Ordinal) { ["MunicipioId"] = "Município" },
        ["Municipio"] = new(StringComparer.Ordinal) { ["CodigoIbge"] = "Código IBGE" },
        ["Usuario"] = new(StringComparer.Ordinal)
        {
            ["EmpresaId"] = "Filial de casa", ["GestorId"] = "Gestor", ["Natureza"] = "Natureza da conta",
            ["AguardandoLiberacaoDesde"] = "Aguardando liberação desde"
        },
        ["UsuarioPerfil"] = new(StringComparer.Ordinal)
        {
            ["UsuarioId"] = "Pessoa", ["PerfilId"] = "Perfil", ["EmpresaId"] = "Filial em que vale", ["ExpiraEm"] = "Vale até",
            ["RevogadaEm"] = "Revogada em"
        },
        ["Perfil"] = new(StringComparer.Ordinal) { ["Codigo"] = "Código", ["EhPadrao"] = "É o perfil padrão" },
        ["PerfilPermissao"] = new(StringComparer.Ordinal) { ["PerfilId"] = "Perfil", ["CodigoPermissao"] = "Permissão", ["Profundidade"] = "Onde vale" },
        ["CotacaoDeProduto"] = new(StringComparer.Ordinal) { ["ValorEmReais"] = "Valor (R$)" },
        ["CotacaoDoDolar"] = new(StringComparer.Ordinal) { ["ReaisPorDolar"] = "Reais por dólar" },
        ["CustoDeProducao"] = new(StringComparer.Ordinal)
        {
            ["CustoOperacionalHa"] = "Custo operacional por hectare", ["CustoTotalHa"] = "Custo total por hectare",
            ["CustoOperacionalUnidade"] = "Custo operacional por unidade", ["CustoTotalUnidade"] = "Custo total por unidade"
        },
        ["CreditoRuralDeInvestimento"] = new(StringComparer.Ordinal)
        {
            ["Valor"] = "Valor (R$)", ["Area"] = "Área financiada (ha)", ["MunicipioId"] = "Município",
            ["CodigoMunicipioBcb"] = "Município no Banco Central", ["Ano"] = "Ano", ["Mes"] = "Mês", ["CodigoProduto"] = "Produto",
            ["CodigoPrograma"] = "Programa", ["CodigoSubprograma"] = "Subprograma", ["CodigoFonte"] = "Fonte de recurso",
            ["CodigoSeguro"] = "Seguro", ["Atividade"] = "Atividade", ["CodigoModalidade"] = "Modalidade"
        },
        ["ProducaoAgricolaNoMunicipio"] = new(StringComparer.Ordinal)
        {
            ["ProdutoNome"] = "Produto", ["AreaPlantadaHectares"] = "Área plantada (ha)", ["AreaColhidaHectares"] = "Área colhida (ha)",
            ["QuantidadeProduzida"] = "Quantidade produzida", ["ValorDaProducaoMilReais"] = "Valor da produção (mil R$)"
        },
        ["ProducaoAgricolaNoEstado"] = new(StringComparer.Ordinal)
        {
            ["ProdutoNome"] = "Produto", ["AreaPlantadaHectares"] = "Área plantada (ha)", ["AreaColhidaHectares"] = "Área colhida (ha)",
            ["QuantidadeProduzida"] = "Quantidade produzida", ["ValorDaProducaoMilReais"] = "Valor da produção (mil R$)"
        },
        ["FrotaDeTratoresNoMunicipio"] = new(StringComparer.Ordinal)
        {
            ["PotenciaNome"] = "Faixa de potência", ["Tratores"] = "Tratores", ["EstabelecimentosComTrator"] = "Estabelecimentos com trator"
        },
        ["EstabelecimentosPorAreaNoMunicipio"] = new(StringComparer.Ordinal)
        {
            ["GrupoDeAreaNome"] = "Grupo de área", ["Estabelecimentos"] = "Estabelecimentos"
        },
        ["RebanhoNoMunicipio"] = new(StringComparer.Ordinal) { ["RebanhoNome"] = "Rebanho", ["Cabecas"] = "Cabeças" },
        ["AreaTerritorialDoMunicipio"] = new(StringComparer.Ordinal) { ["AreaKm2"] = "Área (km²)" },
    ["MedidaDoIbgeNoEstado"] = new(StringComparer.Ordinal) { ["Valor"] = "Valor publicado", ["CategoriaNome"] = "Categoria do IBGE" },
        ["UsinaDeEtanol"] = new(StringComparer.Ordinal)
        {
            ["RazaoSocial"] = "Razão social", ["MunicipioId"] = "Município", ["CapacidadeDeAnidroM3Dia"] = "Capacidade de anidro (m³/dia)",
            ["CapacidadeDeHidratadoM3Dia"] = "Capacidade de hidratado (m³/dia)", ["EncerradaEm"] = "Saiu da lista da ANP em"
        },
        ["CorrespondenciaDeMunicipio"] = new(StringComparer.Ordinal)
        {
            ["MunicipioId"] = "Município do catálogo", ["Forma"] = "Como a correspondência foi feita",
            ["TextoNaFonte"] = "Nome como a fonte publica"
        },
        ["RegraDePotencial"] = new(StringComparer.Ordinal)
        {
            ["ProdutoCodigoIbge"] = "Cultura (código IBGE)", ["HectaresPorMaquina"] = "Hectares por máquina",
            ["AnosDeRenovacao"] = "Anos de renovação", ["ModeloDeReferencia"] = "Modelo de referência"
        },
        ["ParametroDoPotencial"] = new(StringComparer.Ordinal)
        {
            ["MesesDaJanela"] = "Meses da janela", ["PesoDosContratosNoCredito"] = "Peso dos contratos no crédito",
            ["LimiteDeRetracao"] = "Limite de retração", ["LimiteDeAquecimento"] = "Limite de aquecimento",
            ["LimiteDeSuperaquecimento"] = "Limite de superaquecimento", ["NomeDaFaixaIntermediaria"] = "Nome da faixa intermediária",
            ["LimiteDaPercepcao"] = "Limite da percepção", ["PesoDoIndicadorDePreco"] = "Peso do indicador de preço",
            ["PesoDoIndicadorDeCredito"] = "Peso do indicador de crédito", ["PesoDoIndicadorComercial"] = "Peso do indicador comercial",
            ["FatorMinimo"] = "Fator mínimo", ["FatorMaximo"] = "Fator máximo"
        },
        ["PercepcaoDoGestor"] = new(StringComparer.Ordinal) { ["MunicipioId"] = "Município", ["Percentual"] = "Percentual" },
        ["VendaDeMaquina"] = new(StringComparer.Ordinal)
        {
            ["EmpresaId"] = "Filial", ["EmpresaDoFaturamentoId"] = "Filial do faturamento", ["VendidaEm"] = "Vendida em",
            ["FaturadaEm"] = "Faturada em", ["EntregueEm"] = "Entregue em", ["NumeroDoPedido"] = "Número do pedido",
            ["NumeroDaNotaFiscal"] = "Nota fiscal", ["SituacaoNaOrigem"] = "Situação na origem", ["GestaoNaOrigem"] = "Gestão na origem",
            ["VendaDireta"] = "Venda direta", ["RepasseDireto"] = "Repasse direto", ["Quantidade"] = "Quantidade",
            ["LinhaNaOrigem"] = "Linha na origem", ["ProdutoNaOrigem"] = "Produto na origem", ["UnidadeNaOrigem"] = "Unidade na origem",
            ["UnidadeDoFaturamentoNaOrigem"] = "Unidade do faturamento na origem"
        },
        ["Conexao"] = new(StringComparer.Ordinal)
        {
            ["Endereco"] = "Endereço", ["Porta"] = "Porta", ["Banco"] = "Banco", ["Objeto"] = "Visão lida", ["Usuario"] = "Usuário",
            ["NomeDoCabecalho"] = "Cabeçalho do segredo", ["StatusEsperado"] = "Status esperado", ["MinutosEntreVerificacoes"] = "Minutos entre verificações", ["EstaAtiva"] = "Monitorada",
            ["SegredoAlteradoEm"] = "Credencial trocada em"
        },
        ["Rotina"] = new(StringComparer.Ordinal)
        {
            ["Cadencia"] = "Cadência", ["Mes"] = "Mês", ["Dia"] = "Dia", ["Hora"] = "Hora", ["IntervaloMinutos"] = "Intervalo (minutos)",
            ["EstaLigada"] = "Ligada"
        }
    };

    // O IDENTIFICADOR QUE APONTA PARA OUTRA TABELA e que a tela troca pelo nome. O que não está aqui (modelo,
    // catálogo, endereço) aparece como número — são tabelas de apoio, e o número basta para achar.
    private static readonly Dictionary<(string Entidade, string Campo), TipoDeReferencia> Referencias = new()
    {
        [("Usuario", "EmpresaId")] = TipoDeReferencia.Empresa,
        [("Usuario", "GestorId")] = TipoDeReferencia.Usuario,
        [("UsuarioPerfil", "UsuarioId")] = TipoDeReferencia.Usuario,
        [("UsuarioPerfil", "PerfilId")] = TipoDeReferencia.Perfil,
        [("UsuarioPerfil", "EmpresaId")] = TipoDeReferencia.Empresa,
        [("PerfilPermissao", "PerfilId")] = TipoDeReferencia.Perfil,
        [("Cliente", "ProprietarioId")] = TipoDeReferencia.Usuario,
        [("Cliente", "ClienteMatrizId")] = TipoDeReferencia.Cliente,
        [("Equipamento", "ClienteId")] = TipoDeReferencia.Cliente,
        [("Endereco", "MunicipioId")] = TipoDeReferencia.Municipio,
        [("PercepcaoDoGestor", "MunicipioId")] = TipoDeReferencia.Municipio,
        [("CreditoRuralDeInvestimento", "MunicipioId")] = TipoDeReferencia.Municipio,
        [("UsinaDeEtanol", "MunicipioId")] = TipoDeReferencia.Municipio,
        [("CorrespondenciaDeMunicipio", "MunicipioId")] = TipoDeReferencia.Municipio,
        [("VendaDeMaquina", "EmpresaId")] = TipoDeReferencia.Empresa,
        [("VendaDeMaquina", "EmpresaDoFaturamentoId")] = TipoDeReferencia.Empresa
    };

    /// <summary>A profundidade como a pessoa entende — o mesmo texto da tela de perfis.</summary>
    private static readonly Dictionary<string, string> Alcance = new(StringComparer.Ordinal)
    {
        [nameof(Profundidade.Organizacao)] = "toda a organização",
        [nameof(Profundidade.EmpresaEAbaixo)] = "a filial escolhida e as que estão abaixo dela",
        [nameof(Profundidade.Empresa)] = "só a filial escolhida",
        [nameof(Profundidade.Equipe)] = "você e a sua equipe",
        [nameof(Profundidade.Proprios)] = "só os seus registros"
    };

    /// <summary>O nome da entidade; o próprio código quando falta rótulo.</summary>
    public static string DaEntidade(string entidade) => Entidades.GetValueOrDefault(entidade, entidade);

    /// <summary>Se a entidade tem rótulo.</summary>
    public static bool TemRotuloDaEntidade(string entidade) => Entidades.ContainsKey(entidade);

    /// <summary>O nome do campo; o próprio código quando falta rótulo.</summary>
    public static string DoCampo(string entidade, string campo) =>
        Campos.TryGetValue(entidade, out var daEntidade) && daEntidade.TryGetValue(campo, out var rotulo) ? rotulo
        : Comuns.GetValueOrDefault(campo, campo);

    /// <summary>Se o campo tem rótulo próprio ou comum.</summary>
    public static bool TemRotuloDoCampo(string entidade, string campo) =>
        (Campos.TryGetValue(entidade, out var daEntidade) && daEntidade.ContainsKey(campo)) || Comuns.ContainsKey(campo);

    /// <summary>Os campos cujo identificador a tela troca pelo nome.</summary>
    public static IReadOnlyCollection<(string Entidade, string Campo)> CamposQueApontam => Referencias.Keys;

    /// <summary>A tabela a que o campo aponta, quando aponta para uma que a tela sabe nomear.</summary>
    public static TipoDeReferencia? ReferenciaDe(string entidade, string campo) =>
        Referencias.TryGetValue((entidade, campo), out var tipo) ? tipo : null;

    /// <summary>
    /// O valor como a pessoa lê: o nome no lugar do identificador, "sim"/"não" no lugar de true/false, a
    /// profundidade e a permissão por extenso. Data continua em ISO — quem mostra é a tela, no fuso de quem vê.
    /// </summary>
    public static string? Formatar(
        string entidade, string campo, string? valor, IReadOnlyDictionary<(TipoDeReferencia Tipo, long Id), string> nomes)
    {
        if (valor is null) return null;

        if (ReferenciaDe(entidade, campo) is { } tipo)
            return long.TryParse(valor, out var id) && nomes.TryGetValue((tipo, id), out var nome) ? nome : $"nº {valor}";

        if (campo == "Profundidade" && Alcance.TryGetValue(valor, out var alcance)) return alcance;
        if (campo == "CodigoPermissao" && Permissoes.Catalogo.TryGetValue(valor, out var descricao)) return $"{descricao} ({valor})";

        return valor switch
        {
            "true" => "sim",
            "false" => "não",
            _ => valor
        };
    }
}
