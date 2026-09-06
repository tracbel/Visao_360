namespace Tracbel.Crm.Integracao.Vortice;

/// <summary>
/// A configuração da ponte de leitura do sistema legado.
///
/// <para><b>A CREDENCIAL NUNCA MORA EM ARQUIVO DO PROJETO.</b> <see cref="Conexao"/> chega por
/// variável de ambiente — <c>Vortice__Conexao</c> — e não tem valor padrão em lugar nenhum do
/// repositório. Sem ela, a ponte responde "não configurada" e o resto da API continua de pé;
/// nunca há uma senha versionada esperando alguém notar.</para>
///
/// <para>A conta de leitura do legado é somente-leitura no servidor SQL. Isso é a defesa de
/// fora; a de dentro é esta ponte não ter um único caminho de escrita.</para>
/// </summary>
public sealed class OpcoesDoVortice
{
    /// <summary>O nome da seção no arquivo de configuração.</summary>
    public const string Secao = "Vortice";

    /// <summary>
    /// A cadeia de conexão do banco do sistema legado. Vem de <c>Vortice__Conexao</c>,
    /// nunca de arquivo versionado.
    /// </summary>
    public string? Conexao { get; set; }

    /// <summary>
    /// Quanto tempo esperar antes de desistir.
    ///
    /// CURTO DE PROPÓSITO: a ponte depende de VPN, e VPN fora do ar não devolve erro — ela
    /// simplesmente não responde. Um tempo limite generoso aqui vira uma tela travada lá.
    /// Melhor desistir em 15 segundos e dizer o que houve.
    /// </summary>
    public int TempoLimiteSegundos { get; set; } = 15;

    /// <summary>
    /// A partir de quantos dias sem alteração o dado lido sai marcado como desatualizado.
    ///
    /// [V] Noventa dias é uma escolha conservadora contra um histórico ruim: o faturamento do
    /// legado ficou 17 MESES parado sem ninguém notar. O aviso na tela é o que impede a segunda
    /// vez.
    /// </summary>
    public int DiasParaConsiderarDesatualizado { get; set; } = 90;
}
