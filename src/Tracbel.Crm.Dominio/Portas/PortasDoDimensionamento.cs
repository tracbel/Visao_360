using Tracbel.Crm.Dominio.Comercial;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O DIMENSIONAMENTO DA ADR — o tamanho do território atendido dentro de São Paulo, loja a loja e município a município,
/// e a carteira que o cobre (issue 259, a aba "Dimensionamento ADR" do protótipo da pasta 360).
///
/// <para><b>Porta própria, e não um membro a mais da dos indicadores.</b> Os Indicadores leem UM ano da PAM, o das regras do
/// potencial, e só os municípios da área de atuação ou com cliente; o Dimensionamento lê dois anos (o ano-base e o anterior)
/// dos 645 municípios do estado, cultura a cultura — é o que o mapa de São Paulo e a fatia "Tracbel no Estado" pedem. E a
/// carteira daqui conta faixas de dias desde o último contato, que a cadência dos Indicadores não conta.</para>
/// </summary>
public interface IRepositorioDoDimensionamento
{
    /// <summary>Os anos da PAM carregados para os municípios de São Paulo, do mais novo para o mais antigo.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<short>> AnosDaPamAsync(CancellationToken ct);

    /// <summary>
    /// A produção agrícola dos municípios de São Paulo e do estado nos anos pedidos, já somada por cultura do catálogo (o
    /// que não é de cultura nenhuma vai para <see cref="CulturaDoDimensionamento.Outras"/>), com a área de atuação de cada
    /// município.
    /// </summary>
    /// <param name="anos">Os anos — o ano-base e o anterior.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<ProducaoParaODimensionamento> LerProducaoAsync(IReadOnlyCollection<short> anos, CancellationToken ct);

    /// <summary>
    /// A carteira comercial ao alcance, por município do cliente e por responsável, com as faixas de dias desde o último
    /// contato.
    /// </summary>
    /// <param name="consulta">A visão e os filtros da carteira.</param>
    /// <param name="agoraUtc">O instante contra o qual os dias são contados.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<CarteiraParaODimensionamento> LerCarteiraAsync(
        ConsultaDaCarteiraNoDimensionamento consulta, DateTime agoraUtc, CancellationToken ct);
}

/// <summary>Uma cultura do Dimensionamento — uma do catálogo, ou as outras.</summary>
/// <param name="Codigo">O código da cultura no catálogo, ou <see cref="Outras"/>.</param>
/// <param name="Nome">O nome de exibição.</param>
public sealed record CulturaDoDimensionamento(string Codigo, string Nome)
{
    /// <summary>
    /// O código do que não é de cultura nenhuma do catálogo: todos os outros produtos da PAM que entram na soma da lavoura.
    /// </summary>
    public const string Outras = "OUTRAS";
}

/// <summary>Um município de São Paulo, com o que a área de atuação diz dele.</summary>
/// <param name="CodigoIbge">O código IBGE.</param>
/// <param name="Nome">O nome oficial.</param>
/// <param name="PertenceAAdr">Se é da ADR — os 203 municípios da Tracbel.</param>
/// <param name="Regiao">A sub-região da ADR; nula fora da área de atuação.</param>
/// <param name="LojaCodigo">A filial responsável; nula sem loja.</param>
/// <param name="LojaNome">O nome dela.</param>
/// <param name="Usinas">As usinas de etanol vigentes na ANP que ficam aqui.</param>
public sealed record MunicipioNoDimensionamento(
    int CodigoIbge,
    string Nome,
    bool PertenceAAdr,
    string? Regiao,
    string? LojaCodigo,
    string? LojaNome,
    int Usinas);

/// <summary>
/// A PRODUÇÃO DE UMA CULTURA num município ou no estado, num ano.
///
/// <para><b>A quantidade é só a que o IBGE publica em toneladas.</b> Abacaxi e coco-da-baía vêm em mil frutos
/// (<see cref="Organizacao.UnidadesDaPam"/>), e somá-los às toneladas daria um número sem unidade: eles entram na área e no
/// valor das outras culturas, e ficam fora da quantidade.</para>
///
/// <para><b>Nulo é o IBGE não ter divulgado</b> (sigilo, ou a cultura não existe ali) — nunca zero.</para>
/// </summary>
/// <param name="CodigoIbge">O município; nulo na linha do estado.</param>
/// <param name="Ano">O ano da PAM.</param>
/// <param name="Cultura">O código da cultura, ou <see cref="CulturaDoDimensionamento.Outras"/>.</param>
/// <param name="AreaPlantadaHectares">A área plantada.</param>
/// <param name="AreaColhidaHectares">A área colhida — o denominador da produtividade, como o rendimento médio do IBGE.</param>
/// <param name="QuantidadeToneladas">A quantidade produzida, só dos produtos em toneladas.</param>
/// <param name="ValorMilReais">O valor da produção, em MIL reais.</param>
public sealed record ProducaoDaCultura(
    int? CodigoIbge,
    short Ano,
    string Cultura,
    decimal? AreaPlantadaHectares,
    decimal? AreaColhidaHectares,
    decimal? QuantidadeToneladas,
    decimal? ValorMilReais);

/// <summary>A produção lida para o Dimensionamento.</summary>
/// <param name="Culturas">As culturas do catálogo, na ordem do catálogo, e as outras no fim.</param>
/// <param name="Municipios">Os municípios de São Paulo com código IBGE.</param>
/// <param name="NosMunicipios">A produção de cada cultura em cada município, nos anos pedidos.</param>
/// <param name="NoEstado">
/// O total de São Paulo como o IBGE o publica (tabela 5457, nível UF), nos anos pedidos. Vazio quando o total do estado não
/// foi carregado — e aí a fatia "no Estado" sai vazia com o motivo, em vez de ser a soma dos municípios com outro nome.
/// </param>
public sealed record ProducaoParaODimensionamento(
    IReadOnlyList<CulturaDoDimensionamento> Culturas,
    IReadOnlyList<MunicipioNoDimensionamento> Municipios,
    IReadOnlyList<ProducaoDaCultura> NosMunicipios,
    IReadOnlyList<ProducaoDaCultura> NoEstado);

/// <summary>O que a leitura da carteira pede.</summary>
/// <param name="Visao">Filial (o alcance do cabeçalho) ou a empresa inteira.</param>
/// <param name="ResponsavelId">Só as carteiras comerciais deste responsável — o CEN.</param>
/// <param name="Classe">Só os clientes desta classe da curva ABC.</param>
/// <param name="Municipios">
/// Só os clientes com endereço principal nestes municípios (o recorte de região, loja ou usina). Nulo é a carteira
/// inteira — e só aí existe "fora da região".
/// </param>
public sealed record ConsultaDaCarteiraNoDimensionamento(
    VisaoTerritorial Visao,
    long? ResponsavelId = null,
    ClasseDeCliente? Classe = null,
    IReadOnlySet<int>? Municipios = null);

/// <summary>
/// AS FAIXAS DE DIAS DESDE O ÚLTIMO CONTATO — "cobertos há menos de 30, 60, 90 e 120 dias", como o protótipo.
///
/// <para><b>As faixas são acumuladas</b>: quem foi contatado há 20 dias está em todas as quatro. É o que faz cada uma ser
/// "que fatia da carteira foi coberta nesse prazo", e a de 90 dias ser a cobertura do ranking de prioridade.
/// <see cref="Sem120"/> é o resto: contato há 120 dias ou mais, ou nenhum.</para>
/// </summary>
/// <param name="Ate30">Contato há menos de 30 dias.</param>
/// <param name="Ate60">Há menos de 60.</param>
/// <param name="Ate90">Há menos de 90.</param>
/// <param name="Ate120">Há menos de 120.</param>
/// <param name="Sem120">Sem contato nos últimos 120 dias, ou nunca contatados.</param>
public sealed record FaixasDoUltimoContato(int Ate30, int Ate60, int Ate90, int Ate120, int Sem120);

/// <summary>Clientes distintos de um recorte da carteira.</summary>
/// <param name="Clientes">Clientes distintos com vínculo em carteira comercial.</param>
/// <param name="A">Classe A da curva do faturamento.</param>
/// <param name="B">Classe B.</param>
/// <param name="C">Classe C.</param>
/// <param name="D">Classe D.</param>
/// <param name="SemClasse">Sem classe apurada — o cliente sem faturamento; contado à parte, e não como D.</param>
/// <param name="Faixas">As faixas de dias desde o último contato.</param>
/// <param name="ABSemContato">Clientes A e B sem contato nos últimos 120 dias — o "A/B em risco" do protótipo.</param>
/// <param name="UltimoContatoEm">O contato mais recente do recorte (UTC); nulo quando nenhum.</param>
public sealed record ClientesDaCarteira(
    int Clientes,
    int A,
    int B,
    int C,
    int D,
    int SemClasse,
    FaixasDoUltimoContato Faixas,
    int ABSemContato,
    DateTime? UltimoContatoEm)
{
    /// <summary>A carteira vazia.</summary>
    public static readonly ClientesDaCarteira Nenhum = new(0, 0, 0, 0, 0, 0, new FaixasDoUltimoContato(0, 0, 0, 0, 0), 0, null);

    /// <summary>
    /// Conta clientes distintos — cada um com a classe dele e o último contato já resolvido (o mais recente dos vínculos
    /// que valem para o recorte).
    ///
    /// <para><b>"Há menos de N dias" inclui o dia N</b>, como a cadência dos Indicadores: o contato de exatamente 30 dias
    /// atrás está dentro dos 30.</para>
    /// </summary>
    /// <param name="clientes">A classe e o último contato de cada cliente.</param>
    /// <param name="agoraUtc">O instante contra o qual os dias são contados.</param>
    public static ClientesDaCarteira Contar(IEnumerable<(ClasseDeCliente? Classe, DateTime? UltimoContatoEm)> clientes, DateTime agoraUtc)
    {
        int total = 0, a = 0, b = 0, c = 0, d = 0, sem = 0, ate30 = 0, ate60 = 0, ate90 = 0, ate120 = 0, sem120 = 0, abSemContato = 0;
        DateTime? maisRecente = null;

        foreach (var (classe, ultimo) in clientes)
        {
            total++;
            switch (classe)
            {
                case ClasseDeCliente.A: a++; break;
                case ClasseDeCliente.B: b++; break;
                case ClasseDeCliente.C: c++; break;
                case ClasseDeCliente.D: d++; break;
                default: sem++; break;
            }

            if (ultimo is { } u && u >= agoraUtc.AddDays(-120))
            {
                ate120++;
                if (u >= agoraUtc.AddDays(-90)) ate90++;
                if (u >= agoraUtc.AddDays(-60)) ate60++;
                if (u >= agoraUtc.AddDays(-30)) ate30++;
            }
            else
            {
                sem120++;
                if (classe is ClasseDeCliente.A or ClasseDeCliente.B) abSemContato++;
            }

            if (ultimo is { } recente && (maisRecente is null || recente > maisRecente)) maisRecente = recente;
        }

        return new ClientesDaCarteira(
            total, a, b, c, d, sem, new FaixasDoUltimoContato(ate30, ate60, ate90, ate120, sem120), abSemContato, maisRecente);
    }
}

/// <summary>A carteira dos clientes de um município.</summary>
/// <param name="CodigoIbge">O município do endereço principal do cliente.</param>
/// <param name="Clientes">Os clientes dele.</param>
/// <param name="ResponsavelPrincipal">O responsável com mais vínculos aqui — o "vendedor" do município.</param>
public sealed record CarteiraNoMunicipio(int CodigoIbge, ClientesDaCarteira Clientes, string? ResponsavelPrincipal);

/// <summary>A carteira de um responsável.</summary>
/// <param name="ResponsavelId">O usuário dono das carteiras.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="Natureza">Pessoa, departamento… — a carteira de área aparece como área.</param>
/// <param name="NaRegiao">Dos clientes dele, os com endereço principal num município da ADR.</param>
/// <param name="Fora">Os de fora — outro município, outro estado ou sem endereço.</param>
/// <param name="Clientes">Os clientes dele, contados pelos vínculos DELE.</param>
public sealed record CarteiraDoResponsavel(
    long ResponsavelId,
    string Nome,
    string Natureza,
    int NaRegiao,
    int Fora,
    ClientesDaCarteira Clientes);

/// <summary>
/// A CARTEIRA LIDA PARA O DIMENSIONAMENTO.
///
/// <para><b>O total é de clientes distintos</b>, e a tabela por responsável conta cada um pelos vínculos do responsável:
/// o mesmo cliente em carteiras de dois CENs aparece nas duas linhas, e a soma da tabela pode passar do total — a tela
/// diz isso.</para>
/// </summary>
/// <param name="Total">Todos os clientes distintos ao alcance.</param>
/// <param name="NaRegiao">Deles, os com endereço principal num município da ADR.</param>
/// <param name="Fora">Os de fora.</param>
/// <param name="PorMunicipio">Os clientes por município de São Paulo.</param>
/// <param name="PorResponsavel">Os clientes por responsável.</param>
public sealed record CarteiraParaODimensionamento(
    ClientesDaCarteira Total,
    int NaRegiao,
    int Fora,
    IReadOnlyList<CarteiraNoMunicipio> PorMunicipio,
    IReadOnlyList<CarteiraDoResponsavel> PorResponsavel);
