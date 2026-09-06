namespace Tracbel.Crm.Infraestrutura.Identidade;

/// <summary>
/// A configuração da ponte de identidade provisória — a seção <c>ContextoProvisorio</c> do
/// <c>appsettings</c>.
///
/// Os valores padrão de <see cref="UsuarioPadrao"/> e <see cref="EmpresaPadrao"/> só têm efeito
/// quando <see cref="PermitirPadrao"/> é verdadeiro, e o <c>Program.cs</c> só liga isso em
/// Desenvolvimento. É a amarra nº 1 descrita em <see cref="ResolvedorDeContextoProvisorio"/>.
/// </summary>
public sealed class OpcoesDeContextoProvisorio
{
    /// <summary>O nome da seção no arquivo de configuração.</summary>
    public const string Secao = "ContextoProvisorio";

    /// <summary>O cabeçalho que carrega o nome principal do usuário.</summary>
    public string CabecalhoDeUsuario { get; set; } = "X-Tracbel-Usuario";

    /// <summary>O cabeçalho que carrega o código da filial.</summary>
    public string CabecalhoDeEmpresa { get; set; } = "X-Tracbel-Empresa";

    /// <summary>O usuário assumido quando o cabeçalho falta. Só vale em Desenvolvimento.</summary>
    public string? UsuarioPadrao { get; set; }

    /// <summary>A filial assumida quando o cabeçalho falta. Só vale em Desenvolvimento.</summary>
    public string? EmpresaPadrao { get; set; }

    /// <summary>
    /// Se a API pode assumir usuário e filial quando os cabeçalhos faltam.
    ///
    /// Ligado só em Desenvolvimento, pelo <c>Program.cs</c>. Em homologação e produção, requisição
    /// sem cabeçalho é recusada — a API nunca "assume alguém".
    /// </summary>
    public bool PermitirPadrao { get; set; }
}
