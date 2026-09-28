namespace Tracbel.Crm.Integracao.OperationsCenter;

/// <summary>
/// A configuração da leitura do Operations Center da John Deere — o banco MySQL do BI (<c>johndeere_prd</c>), que o BI
/// carrega do Operations Center todo dia (decisão de 28/09/2026).
///
/// <para><b>A CREDENCIAL NUNCA MORA EM ARQUIVO DO PROJETO.</b> Chega pela tela (Configurações › Integrações, protegida)
/// ou pelas variáveis de ambiente do servidor — <c>OperationsCenter__Servidor</c>, <c>OperationsCenter__Senha</c>… —, e
/// não tem valor padrão em lugar nenhum do repositório. Na estação, o <c>.env</c> tem as mesmas coisas como
/// <c>OPCENTER_DB_*</c>.</para>
///
/// <para><b>SOMENTE LEITURA, em duas camadas</b>, como o ART: o usuário do BI é de leitura, e a sessão ainda se declara
/// <c>READ ONLY</c> antes da primeira consulta.</para>
/// </summary>
public sealed class OpcoesDoOperationsCenter
{
    /// <summary>O nome da seção no arquivo de configuração.</summary>
    public const string Secao = "OperationsCenter";

    /// <summary>A tabela das máquinas, que a tela chama de "visão lida".</summary>
    public const string TabelaDasMaquinasPadrao = "machines";

    /// <summary>O servidor MySQL.</summary>
    public string? Servidor { get; set; }

    /// <summary>A porta. Padrão do MySQL.</summary>
    public uint Porta { get; set; } = 3306;

    /// <summary>O banco.</summary>
    public string? Banco { get; set; }

    /// <summary>O usuário de leitura.</summary>
    public string? Usuario { get; set; }

    /// <summary>A senha. Vem da tela ou de <c>OperationsCenter__Senha</c>, nunca de arquivo versionado.</summary>
    public string? Senha { get; set; }

    /// <summary>A tabela das máquinas — por onde a leitura começa.</summary>
    public string? Tabela { get; set; } = TabelaDasMaquinasPadrao;

    /// <summary>
    /// Quanto tempo esperar cada consulta. As duas leituras de histórico levaram 76 s e 94 s em 27/09/2026 — o servidor do
    /// BI é lento no disco —, e a folga é para o dia em que ele estiver ocupado com a carga dele.
    /// </summary>
    public int TempoLimiteSegundos { get; set; } = 900;

    /// <summary>Se a configuração tem o mínimo para tentar uma conexão.</summary>
    public bool EstaConfigurada =>
        !string.IsNullOrWhiteSpace(Servidor)
        && !string.IsNullOrWhiteSpace(Banco)
        && !string.IsNullOrWhiteSpace(Usuario)
        && !string.IsNullOrWhiteSpace(Senha);
}
