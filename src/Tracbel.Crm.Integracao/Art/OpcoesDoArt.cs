namespace Tracbel.Crm.Integracao.Art;

/// <summary>
/// A configuração da leitura do ART, o sistema comercial de vendas de máquina (documento 35, seção 10).
///
/// <para><b>A CREDENCIAL NUNCA MORA EM ARQUIVO DO PROJETO.</b> Servidor, usuário e senha chegam por
/// variável de ambiente — <c>Art__Servidor</c>, <c>Art__Usuario</c>, <c>Art__Senha</c>… — e não têm
/// valor padrão em lugar nenhum do repositório.</para>
///
/// <para><b>SOMENTE LEITURA, em duas camadas.</b> O usuário concedido pelo responsável do ART é de
/// leitura, e a sessão ainda se declara <c>READ ONLY</c> antes da primeira consulta: se um dia a
/// credencial ganhar escrita por engano, esta leitura continua sem conseguir gravar.</para>
/// </summary>
public sealed class OpcoesDoArt
{
    /// <summary>O nome da seção no arquivo de configuração.</summary>
    public const string Secao = "Art";

    /// <summary>O servidor MySQL do ART.</summary>
    public string? Servidor { get; set; }

    /// <summary>A porta. Padrão do MySQL.</summary>
    public uint Porta { get; set; } = 3306;

    /// <summary>O banco.</summary>
    public string? Banco { get; set; }

    /// <summary>O usuário de leitura.</summary>
    public string? Usuario { get; set; }

    /// <summary>A senha. Vem de <c>Art__Senha</c>, nunca de arquivo versionado.</summary>
    public string? Senha { get; set; }

    /// <summary>A view de vendas de máquina liberada para a integração.</summary>
    public string? Visao { get; set; }

    /// <summary>Quanto tempo esperar a consulta. A view inteira volta em segundos.</summary>
    public int TempoLimiteSegundos { get; set; } = 300;

    /// <summary>Se a configuração tem o mínimo para tentar uma conexão.</summary>
    public bool EstaConfigurada =>
        !string.IsNullOrWhiteSpace(Servidor)
        && !string.IsNullOrWhiteSpace(Banco)
        && !string.IsNullOrWhiteSpace(Usuario)
        && !string.IsNullOrWhiteSpace(Senha)
        && !string.IsNullOrWhiteSpace(Visao);
}
