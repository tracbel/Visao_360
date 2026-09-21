namespace Tracbel.Crm.Infraestrutura.Identidade;

/// <summary>
/// O que acontece com quem entra pelo Entra ID pela primeira vez e não tem cadastro no CRM.
///
/// <para><b>A decisão de 21/09/2026:</b> o grupo do Entra ID é o portão. Quem passa por ele ganha o
/// usuário no banco, AGUARDANDO LIBERAÇÃO — sem ver dado nenhum até o administrador escolher a filial.</para>
///
/// <para><b>Só com o grupo configurado.</b> Sem <c>Entra:GrupoPermitido</c>, qualquer conta do locatário
/// passaria pelo login; criar usuário para cada uma encheria a lista do administrador de gente que
/// ninguém convidou. Nesse caso vale o de antes: "sem cadastro no CRM".</para>
/// </summary>
public sealed class OpcoesDoPrimeiroLogin
{
    /// <summary>Criar o usuário, aguardando liberação, quando a conta não casa com cadastro nenhum.</summary>
    public bool CriarUsuarioAguardandoLiberacao { get; set; }
}
