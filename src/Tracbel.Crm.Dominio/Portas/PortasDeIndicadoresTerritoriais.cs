using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// De onde o painel olha: a filial escolhida no cabeçalho, ou a empresa inteira.
///
/// <para><b>A empresa inteira é permissão, não preferência.</b> Ela abre o alcance entre filiais
/// (<c>CrmDbContext.AbrirAlcanceEntreEmpresas</c>), e só quem tem
/// <c>Empresa.AlcanceEntreFiliais</c> em profundidade Organização pode pedir — cada abertura fica
/// no diário com o motivo.</para>
/// </summary>
public enum VisaoTerritorial
{
    /// <summary>A filial do contexto de acesso e as abaixo dela.</summary>
    Filial = 0,

    /// <summary>Todas as filiais, cada venda no município do cliente.</summary>
    Empresa = 1
}

/// <summary>O que o painel geográfico pede.</summary>
/// <param name="CompetenciaInicial">O primeiro mês das vendas, sempre no dia 1.</param>
/// <param name="CompetenciaFinal">O último mês das vendas, sempre no dia 1, inclusive.</param>
/// <param name="Regiao">Filtro pela região da ADR. Nulo é a área inteira.</param>
/// <param name="LojaCodigo">Filtro pela loja responsável, pelo código da filial. Nulo é todas.</param>
/// <param name="Visao">Filial ou empresa inteira.</param>
/// <param name="FilialDaVendaId">
/// Só as notas emitidas por esta filial. Vale para as vendas; não se aplica a cobertura nem a
/// potencial.
/// </param>
/// <param name="FilialDoClienteId">
/// Só os clientes cadastrados nesta filial. Vale para cobertura e vendas.
/// </param>
public sealed record ConsultaDeIndicadoresTerritoriais(
    DateOnly CompetenciaInicial,
    DateOnly CompetenciaFinal,
    RegiaoDaAreaDeAtuacao? Regiao,
    string? LojaCodigo,
    VisaoTerritorial Visao = VisaoTerritorial.Filial,
    int? FilialDaVendaId = null,
    int? FilialDoClienteId = null);

/// <summary>Uma afirmação de responsável, como a tela a mostra — com a fonte, sempre.</summary>
/// <param name="Papel">CEN ou gestor.</param>
/// <param name="Fonte">A planilha que fez a afirmação.</param>
/// <param name="NomeNaOrigem">O nome como a planilha escreveu.</param>
/// <param name="Situacao">O que a carga conseguiu afirmar sobre o nome.</param>
/// <param name="UsuarioNome">O nome do usuário do CRM, quando identificado.</param>
/// <param name="ImportadoEm">Quando a afirmação foi lida (UTC). As planilhas não declaram vigência.</param>
public sealed record ResponsavelDeclarado(
    string Papel, string Fonte, string NomeNaOrigem, string Situacao, string? UsuarioNome, DateTime ImportadoEm);

/// <summary>
/// A cobertura de visita de um recorte territorial, contada por VÍNCULO cliente × carteira comercial
/// (documento 27, seção 4; documento 32, seção 8.1).
///
/// <para><b>Os estados não se sobrepõem, e a ordem é esta:</b> vínculo em linha de negócio sem
/// cadência declarada é <see cref="SemCadencia"/>, qualquer que seja o contato; entre os que têm
/// cadência, sem interação nenhuma é <see cref="NuncaContatados"/>, dentro do prazo é
/// <see cref="Cobertos"/>, fora dele é <see cref="ForaDaCadencia"/>. Assim o denominador do
/// percentual só contém quem tem prazo contra o que ser medido.</para>
/// </summary>
/// <param name="Clientes">Clientes distintos com endereço principal no recorte.</param>
/// <param name="Vinculos">Vínculos em carteira comercial desses clientes.</param>
/// <param name="VinculosComCadencia">Os que estão em linha de negócio com cadência declarada — os elegíveis.</param>
/// <param name="Cobertos">Última interação dentro da cadência da classe.</param>
/// <param name="ForaDaCadencia">Com interação, mas mais antiga que o prazo.</param>
/// <param name="NuncaContatados">Elegíveis sem interação nenhuma.</param>
/// <param name="SemCadencia">Em linha de negócio que não declara prazo — fora da conta.</param>
public sealed record CoberturaTerritorial(
    int Clientes, int Vinculos, int VinculosComCadencia, int Cobertos, int ForaDaCadencia, int NuncaContatados, int SemCadencia)
{
    /// <summary>Fora da cadência mais nunca contatados.</summary>
    public int Pendentes => ForaDaCadencia + NuncaContatados;

    /// <summary>
    /// Pendentes sobre elegíveis, em pontos percentuais com uma casa. Nulo quando não há elegível —
    /// "sem dado", nunca zero.
    /// </summary>
    public decimal? PercentualPendente =>
        VinculosComCadencia == 0 ? null : decimal.Round(100m * Pendentes / VinculosComCadencia, 1);
}

/// <summary>
/// As vendas de um recorte, pelo endereço principal do cliente, com a quebra do grupo do item.
///
/// <para><b>Sem dupla contagem por construção:</b> as quatro parcelas somam o valor líquido, e o
/// pós-venda é peça mais serviço — máquina e "outros" ficam fora dele.</para>
/// </summary>
/// <param name="ClientesQueCompraram">Clientes com faturamento diferente de zero no período.</param>
/// <param name="ValorLiquido">O total do período.</param>
/// <param name="Maquina">Grupo VEIC.</param>
/// <param name="Peca">Faixa de grupos de peça.</param>
/// <param name="Servico">Serviço e mão de obra.</param>
/// <param name="Outros">Repasse de fábrica e grupos não classificados.</param>
public sealed record VendasTerritoriais(
    int ClientesQueCompraram, decimal ValorLiquido, decimal Maquina, decimal Peca, decimal Servico, decimal Outros)
{
    /// <summary>Peça mais serviço.</summary>
    public decimal PosVenda => Peca + Servico;
}

/// <summary>O potencial teórico de um produto num município, por uma regra.</summary>
/// <param name="ProdutoCodigoIbge">O produto da regra.</param>
/// <param name="AreaPlantadaHectares">A área plantada; nulo quando o IBGE não divulga ou não foi carregada.</param>
/// <param name="MaquinasTeoricas">Área dividida pelos hectares por máquina; nulo quando a área é nula.</param>
public sealed record PotencialTerritorial(int ProdutoCodigoIbge, decimal? AreaPlantadaHectares, decimal? MaquinasTeoricas);

/// <summary>Os três indicadores de um município, com o território e os responsáveis.</summary>
/// <param name="CodigoIbge">O código IBGE — é por ele que o mapa encontra o polígono.</param>
/// <param name="Nome">O nome oficial.</param>
/// <param name="PertenceAAdr">Se a planilha de área de atuação o marca como ADR.</param>
/// <param name="ListadoNaAreaDeAtuacao">Se ele está na planilha, dentro ou fora da ADR.</param>
/// <param name="Regiao">A região da ADR.</param>
/// <param name="LojaCodigo">O código da loja responsável.</param>
/// <param name="LojaNome">O nome da loja responsável.</param>
/// <param name="LojaAtivaNoCrm">Se a filial está ativa no CRM; nulo quando não há loja.</param>
/// <param name="Responsaveis">O que cada planilha diz sobre CEN e gestor.</param>
/// <param name="CenDivergenteEntreFontes">Se as duas planilhas nomeiam o CEN de jeitos diferentes (grafia ou pessoa).</param>
/// <param name="ComparacaoDoCen">
/// Como as duas planilhas nomeiam o CEN: uma fonte só, mesmo nome, provável mesma pessoa ou nomes
/// diferentes. É rótulo: as duas afirmações continuam em <see cref="Responsaveis"/>.
/// </param>
/// <param name="Cobertura">A cobertura de visita.</param>
/// <param name="Vendas">As vendas no período.</param>
/// <param name="Potencial">O potencial teórico, uma linha por regra ativa.</param>
public sealed record IndicadoresDoMunicipio(
    int CodigoIbge,
    string Nome,
    bool PertenceAAdr,
    bool ListadoNaAreaDeAtuacao,
    string Regiao,
    string? LojaCodigo,
    string? LojaNome,
    bool? LojaAtivaNoCrm,
    IReadOnlyList<ResponsavelDeclarado> Responsaveis,
    bool CenDivergenteEntreFontes,
    string ComparacaoDoCen,
    CoberturaTerritorial Cobertura,
    VendasTerritoriais Vendas,
    IReadOnlyList<PotencialTerritorial> Potencial);

/// <summary>
/// O que não tem lugar no mapa, somado à parte — é o que faz o total da tela fechar com o banco.
/// </summary>
/// <param name="Grupo">Código estável do grupo.</param>
/// <param name="Descricao">O que o grupo é, em português.</param>
/// <param name="Cobertura">A cobertura do grupo.</param>
/// <param name="Vendas">As vendas do grupo.</param>
public sealed record IndicadoresForaDoMapa(string Grupo, string Descricao, CoberturaTerritorial Cobertura, VendasTerritoriais Vendas);

/// <summary>Uma regra de potencial, como a tela a cita junto do mapa.</summary>
/// <param name="ProdutoCodigoIbge">O produto.</param>
/// <param name="ProdutoNome">O rótulo do produto.</param>
/// <param name="HectaresPorMaquina">Hectares por máquina de referência.</param>
/// <param name="ModeloDeReferencia">O modelo de referência.</param>
/// <param name="Situacao">A confirmar ou confirmada.</param>
/// <param name="Origem">Quem informou e onde.</param>
public sealed record RegraDePotencialAplicada(
    int ProdutoCodigoIbge, string ProdutoNome, decimal HectaresPorMaquina, string ModeloDeReferencia, string Situacao, string Origem);

/// <summary>O painel geográfico inteiro.</summary>
/// <param name="CompetenciaInicial">O primeiro mês das vendas.</param>
/// <param name="CompetenciaFinal">O último mês das vendas, inclusive.</param>
/// <param name="ReferenciaDaCobertura">O instante contra o qual a cadência foi medida (UTC).</param>
/// <param name="InteracaoMaisRecente">A interação mais recente carregada — diz a idade do dado de visita.</param>
/// <param name="AnoDaAreaPlantada">O ano da PAM usado no potencial.</param>
/// <param name="Regras">As regras de potencial ativas.</param>
/// <param name="Municipios">Os municípios de SP da área de atuação ou com cliente, depois dos filtros.</param>
/// <param name="ForaDoMapa">Os grupos sem polígono: sem município, sem código IBGE, outra UF.</param>
/// <param name="Enderecos">Endereços de cliente ativos ao alcance.</param>
/// <param name="EnderecosComArea">Deles, quantos têm área e cultura.</param>
/// <param name="Visao">A visão aplicada — filial ou empresa.</param>
public sealed record IndicadoresTerritoriais(
    DateOnly CompetenciaInicial,
    DateOnly CompetenciaFinal,
    DateTime ReferenciaDaCobertura,
    DateTime? InteracaoMaisRecente,
    short? AnoDaAreaPlantada,
    IReadOnlyList<RegraDePotencialAplicada> Regras,
    IReadOnlyList<IndicadoresDoMunicipio> Municipios,
    IReadOnlyList<IndicadoresForaDoMapa> ForaDoMapa,
    int Enderecos,
    int EnderecosComArea,
    string Visao);

/// <summary>
/// O acesso aos INDICADORES TERRITORIAIS — a leitura que alimenta os três mapas.
///
/// <para><b>Porta própria, separada de <see cref="IRepositorioTerritorio"/></b>: aquela lista o
/// território; esta cruza o território com cliente, carteira, faturamento e área plantada. Juntas
/// dariam uma porta com duas razões para mudar.</para>
///
/// <para><b>A fronteira de filial continua valendo</b> para o que tem dono — cliente, carteira e
/// faturamento entram filtrados pelo contexto de acesso. A área de atuação, os responsáveis e a
/// área plantada são mapa da empresa e não são filtrados.</para>
/// </summary>
public interface IRepositorioIndicadoresTerritoriais
{
    /// <summary>Apura os indicadores.</summary>
    /// <param name="consulta">O período e os filtros, já validados.</param>
    /// <param name="agoraUtc">O instante contra o qual a cadência é medida.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IndicadoresTerritoriais> ApurarAsync(
        ConsultaDeIndicadoresTerritoriais consulta, DateTime agoraUtc, CancellationToken ct);

    /// <summary>
    /// As filiais pelo código — ativas e inativas —, para validar os filtros de filial da venda e
    /// de cadastro do cliente antes de apurar.
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyDictionary<string, int>> ListarFiliaisAsync(CancellationToken ct);
}
