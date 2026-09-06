namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// A configuração da ponte de leitura do TOTVS Protheus.
///
/// <para><b>A CREDENCIAL NUNCA MORA EM ARQUIVO DO PROJETO.</b> Usuário e senha chegam por
/// variável de ambiente — <c>Protheus__Usuario</c> e <c>Protheus__Senha</c> — e não têm valor
/// padrão em lugar nenhum do repositório. Sem elas, a ponte responde "não configurada" e o resto
/// da aplicação continua de pé.</para>
///
/// <para><b>A senha viaja na QUERY STRING.</b> Não é escolha nossa: é assim que o endpoint de
/// token do Protheus funciona. A consequência é que qualquer mensagem de erro que cite a URL
/// expõe a credencial em log e em tela — e por isso <c>PonteDoProtheus</c> passa toda mensagem
/// por um filtro antes de deixá-la sair.</para>
///
/// <para><b>É HTTP puro, e isso é um problema conhecido.</b> O AppServer de produção não expõe
/// TLS na porta 5891. Enquanto for assim, a senha trafega em claro dentro da rede da Tracbel.
/// Está registrado como pendência de infraestrutura no documento 28.</para>
/// </summary>
public sealed class OpcoesDoProtheus
{
    /// <summary>O nome da seção no arquivo de configuração.</summary>
    public const string Secao = "Protheus";

    /// <summary>
    /// A base da API REST, sem barra no fim. Ex.: <c>http://10.100.10.98:5891/rest</c>.
    /// </summary>
    public string? Base { get; set; }

    /// <summary>O usuário da API. Vem de <c>Protheus__Usuario</c>.</summary>
    public string? Usuario { get; set; }

    /// <summary>A senha da API. Vem de <c>Protheus__Senha</c>, nunca de arquivo versionado.</summary>
    public string? Senha { get; set; }

    /// <summary>
    /// Quanto tempo esperar por uma página antes de desistir.
    ///
    /// <para>Generoso de propósito, ao contrário do Vórtice: aqui cada requisição traz mil linhas
    /// de um ERP em produção, e o AppServer leva segundos para responder quando está ocupado.
    /// Desistir cedo faria a carga falhar no meio por lentidão, não por erro.</para>
    /// </summary>
    public int TempoLimiteSegundos { get; set; } = 180;

    /// <summary>Quantas linhas por página. Mil é o que o AppServer aguenta sem degradar.</summary>
    public int TamanhoDaPagina { get; set; } = 1000;

    /// <summary>Se a configuração tem o mínimo para tentar uma conexão.</summary>
    public bool EstaConfigurada =>
        !string.IsNullOrWhiteSpace(Base)
        && !string.IsNullOrWhiteSpace(Usuario)
        && !string.IsNullOrWhiteSpace(Senha);
}
