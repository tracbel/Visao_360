using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Seguranca;

/// <summary>
/// O que a conta É — e nem toda conta do legado é gente.
///
/// <para><b>Por que isto existe:</b> o cadastro de usuário do Vórtice mistura pessoa, caixa de
/// departamento (<c>INT.MERCADO</c>, <c>CALL.CENTER</c>, <c>EXPERIENCIA.CLIENTE</c>), conta do
/// próprio servidor (<c>VRTCSERVER</c>, que assina 30.755 interações), conta do fornecedor
/// (<c>VORTICE CONSULTORIA</c>) e resíduo de teste (<c>TESTE JUNO</c>, <c>NETO PRADO TESTE
/// APP</c>). Somadas como pessoas, elas entram no ranking de CEN e no cálculo de produtividade —
/// e um departamento com 11 carteiras vence qualquer vendedor de verdade.</para>
///
/// <para><b>Classificar, e não apagar.</b> Essas contas assinam registro real: apagá-las tiraria
/// o autor de milhares de interações. O que muda é que a tela de gerência passa a saber que elas
/// não são vendedor.</para>
/// </summary>
public enum NaturezaDoUsuario
{
    /// <summary>Gente. É a única natureza que entra em ranking de pessoa.</summary>
    Pessoa = 0,

    /// <summary>Caixa de área ou departamento, com mais de um humano atrás.</summary>
    Departamento = 1,

    /// <summary>Conta do próprio sistema, que registra sem ninguém digitar.</summary>
    Sistema = 2,

    /// <summary>Conta de fornecedor ou consultoria externa.</summary>
    Fornecedor = 3,

    /// <summary>Resíduo de teste de implantação.</summary>
    Teste = 4
}

/// <summary>
/// O usuário do CRM — espelho do Microsoft Entra ID.
///
/// O CRM NÃO guarda senha. [V] No Vórtice a senha tem 30 bytes sem sal e 80 usuários
/// compartilham exatamente o mesmo valor armazenado. Aqui a autenticação é delegada ao
/// Entra ID, que a Tracbel já opera com MFA.
///
/// Esta tabela unifica os três cadastros de vendedor do legado num só. Ancora a propriedade
/// de registro: é o proprietário de cliente, processo, tarefa e lead.
/// </summary>
public sealed class Usuario : EntidadeBase
{
    private Usuario() { }

    /// <summary>O identificador de objeto do Entra ID. É a chave real de login.</summary>
    public Guid IdentidadeExterna { get; private set; }

    /// <summary>Nome principal de usuário, no formato de e-mail corporativo.</summary>
    public string NomePrincipal { get; private set; } = default!;

    /// <summary>Nome completo, como no cadastro corporativo.</summary>
    public string NomeCompleto { get; private set; } = default!;

    /// <summary>Nome curto, o que aparece na tela.</summary>
    public string NomeExibicao { get; private set; } = default!;

    /// <summary>E-mail corporativo.</summary>
    public Email Email { get; private set; }

    /// <summary>Empresa de casa do usuário: define o escopo padrão dele.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Gestor direto. Sustenta a segurança por hierarquia.</summary>
    public long? GestorId { get; private set; }

    /// <summary>Papel principal, para leitura rápida na tela. A permissão real vem do conjunto.</summary>
    public string? Papel { get; private set; }

    /// <summary>
    /// Desativar nunca apaga. [V] desativar um usuário no Vórtice destrói as permissões
    /// porque a coluna de ativo/inativo simplesmente não existe.
    /// </summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>
    /// Se esta conta é uma pessoa, um departamento, o sistema, um fornecedor ou um teste.
    ///
    /// <para>Nasce classificada pela carga, com regra e evidência. O negócio corrige pela tela de
    /// usuários quando a leitura estiver errada — e ela vai estar, em algum caso: nome de gente
    /// e nome de caixa às vezes se parecem.</para>
    /// </summary>
    public NaturezaDoUsuario Natureza { get; private set; } = NaturezaDoUsuario.Pessoa;

    /// <summary>Quando o usuário foi desativado (UTC).</summary>
    public DateTime? DesativadoEm { get; private set; }

    /// <summary>Último acesso bem-sucedido (UTC).</summary>
    public DateTime? UltimoLoginEm { get; private set; }

    /// <summary>Cria um usuário a partir da identidade do Entra ID.</summary>
    /// <param name="identidadeExterna">O identificador de objeto do Entra ID.</param>
    /// <param name="nomePrincipal">Nome principal de usuário.</param>
    /// <param name="nomeCompleto">Nome completo.</param>
    /// <param name="nomeExibicao">Nome curto, o que aparece na tela.</param>
    /// <param name="email">E-mail corporativo.</param>
    /// <param name="empresaId">Empresa de casa.</param>
    /// <param name="criadoPorId">Quem criou.</param>
    /// <param name="ultimoLoginEm">Último acesso conhecido.</param>
    /// <param name="estaAtivo">Se o usuário continua ativo.</param>
    /// <param name="desativadoEm">Quando foi desativado. Obrigatório quando não está ativo.</param>
    /// <param name="natureza">Se a conta é pessoa, departamento, sistema, fornecedor ou teste.</param>
    public static Usuario Criar(
        Guid identidadeExterna,
        string nomePrincipal,
        string nomeCompleto,
        string nomeExibicao,
        Email email,
        int empresaId,
        long criadoPorId,
        DateTime? ultimoLoginEm = null,
        bool estaAtivo = true,
        DateTime? desativadoEm = null,
        NaturezaDoUsuario natureza = NaturezaDoUsuario.Pessoa)
    {
        if (!estaAtivo && desativadoEm is null)
            throw new RegraDeNegocioViolada(
                "Usuário desativado precisa da data de desativação: 'inativo desde quando?' é " +
                "pergunta que tem de ter resposta.");

        return new Usuario
        {
            IdentidadeExterna = identidadeExterna,
            NomePrincipal = nomePrincipal,
            NomeCompleto = nomeCompleto,
            NomeExibicao = nomeExibicao,
            Email = email,
            EmpresaId = empresaId,
            UltimoLoginEm = ultimoLoginEm,
            EstaAtivo = estaAtivo,
            DesativadoEm = desativadoEm,
            Natureza = natureza,
            CriadoPorId = criadoPorId
        };
    }

    /// <summary>Atualiza o espelho do cadastro de origem, sem tocar na identidade.</summary>
    /// <param name="nomeCompleto">Nome completo.</param>
    /// <param name="nomeExibicao">Nome curto.</param>
    /// <param name="empresaId">Empresa de casa.</param>
    /// <param name="ultimoLoginEm">Último acesso conhecido.</param>
    /// <param name="usuarioId">Quem alterou.</param>
    public void Alterar(
        string nomeCompleto, string nomeExibicao, int empresaId, DateTime? ultimoLoginEm, long usuarioId)
    {
        NomeCompleto = nomeCompleto;
        NomeExibicao = nomeExibicao;
        EmpresaId = empresaId;
        UltimoLoginEm = ultimoLoginEm;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Declara o que a conta é — pessoa, departamento, sistema, fornecedor ou teste.
    ///
    /// <para>Método próprio pela mesma razão da carteira: é decisão de outra natureza que trocar
    /// nome ou e-mail, e muda de que números a conta participa.</para>
    /// </summary>
    /// <param name="natureza">O que a conta é.</param>
    /// <param name="usuarioId">Quem classificou.</param>
    public void Classificar(NaturezaDoUsuario natureza, long usuarioId)
    {
        if (Natureza == natureza) return;

        Natureza = natureza;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>Desativa o usuário sem apagar nada.</summary>
    public void Desativar(long usuarioId)
    {
        if (!EstaAtivo) return;
        EstaAtivo = false;
        DesativadoEm = DateTime.UtcNow;
        MarcarAlteracao(usuarioId);
    }
}
