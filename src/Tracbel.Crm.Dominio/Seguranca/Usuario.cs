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

    /// <summary>
    /// Gestor direto — a ÚNICA hierarquia de pessoas (documento 40). A profundidade <c>Equipe</c> alcança os
    /// subordinados calculados a partir daqui, na montagem do contexto de acesso.
    ///
    /// <para>O antigo <c>Papel</c>, texto livre que não autorizava nada, saiu na fase 3: o que a pessoa pode
    /// fazer vem dos perfis concedidos a ela, e só deles.</para>
    /// </summary>
    public long? GestorId { get; private set; }

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

    /// <summary>
    /// Desde quando a conta espera o administrador liberá-la (UTC). Nula: liberada.
    ///
    /// <para><b>A decisão de 21/09/2026:</b> o grupo do Entra ID é o portão. Quem passa por ele e ainda não
    /// tem cadastro ganha o usuário no primeiro login, mas NÃO enxerga dado nenhum até o administrador
    /// escolher a filial e liberar. Nenhum número aparece para quem ninguém olhou.</para>
    /// </summary>
    public DateTime? AguardandoLiberacaoDesde { get; private set; }

    /// <summary>Se a conta foi criada no primeiro login e ainda não foi liberada.</summary>
    public bool AguardaLiberacao => AguardandoLiberacaoDesde is not null;

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

    /// <summary>
    /// Cria, no primeiro login, a conta de quem passou pelo grupo do Entra ID e não tinha cadastro. Ela
    /// nasce AGUARDANDO LIBERAÇÃO: o login é recusado até o administrador escolher a filial e liberar.
    ///
    /// <para><b>A filial provisória não dá acesso a nada</b> — a conta não entra enquanto espera. Ela só
    /// existe porque a filial de casa é obrigatória; quem decide a de verdade é o administrador, em
    /// <see cref="Liberar"/>.</para>
    ///
    /// <para>O autor é o sistema (identificador 0): a pessoa ainda não tem identificador na hora do
    /// cadastro, e o que a criou foi o login dela, não alguém da TI.</para>
    /// </summary>
    /// <param name="identidadeExterna">O identificador de objeto (<c>oid</c>) do Entra ID.</param>
    /// <param name="nomePrincipal">O nome principal (<c>preferred_username</c>).</param>
    /// <param name="nome">O nome da pessoa no Entra (<c>name</c>), quando vier.</param>
    /// <param name="email">O e-mail corporativo.</param>
    /// <param name="filialProvisoriaId">A filial obrigatória enquanto a conta espera.</param>
    /// <param name="agoraUtc">O instante do login.</param>
    public static Usuario CriarNoPrimeiroLogin(
        Guid identidadeExterna, string nomePrincipal, string? nome, Email email, int filialProvisoriaId, DateTime agoraUtc)
    {
        if (identidadeExterna == Guid.Empty)
            throw new RegraDeNegocioViolada("O Entra ID não devolveu o identificador de objeto da conta.");

        if (string.IsNullOrWhiteSpace(nomePrincipal))
            throw new RegraDeNegocioViolada("O Entra ID não devolveu o nome principal da conta.");

        var principal = nomePrincipal.Trim();
        var completo = string.IsNullOrWhiteSpace(nome) ? principal.Split('@')[0] : nome.Trim();

        return new Usuario
        {
            IdentidadeExterna = identidadeExterna,
            NomePrincipal = principal,
            NomeCompleto = Cortar(completo, 200),
            NomeExibicao = Cortar(completo, 80),
            Email = email,
            EmpresaId = filialProvisoriaId,
            UltimoLoginEm = agoraUtc,
            AguardandoLiberacaoDesde = agoraUtc,
            Natureza = NaturezaDoUsuario.Pessoa,
            CriadoPorId = 0
        };

        static string Cortar(string texto, int limite) => texto.Length <= limite ? texto : texto[..limite];
    }

    /// <summary>
    /// O administrador libera a conta que esperava: escolhe a filial de casa, e a pessoa passa a entrar.
    /// </summary>
    /// <param name="filialId">A filial de casa escolhida.</param>
    /// <param name="usuarioId">O administrador que liberou.</param>
    public void Liberar(int filialId, long usuarioId)
    {
        if (!AguardaLiberacao)
            throw new RegraDeNegocioViolada("Esta conta não está aguardando liberação.");

        if (usuarioId == Id)
            throw new RegraDeNegocioViolada("Ninguém libera a própria conta.");

        EmpresaId = filialId;
        AguardandoLiberacaoDesde = null;
        MarcarAlteracao(usuarioId);
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

    /// <summary>
    /// O sufixo do nome principal que a carga inventou para quem não tinha e-mail.
    ///
    /// <para>O cadastro de usuário do Vórtice não guarda e-mail de ninguém, e a coluna daqui é
    /// obrigatória — então a carga gerou <c>login@sem-email.vortice.invalid</c>, num domínio que a
    /// RFC 2606 garante que não existe. <b>Esse sufixo é também o sinal de "ainda não entrou pelo
    /// Entra ID"</b>: o primeiro login troca o nome principal pelo real, e o sufixo some.</para>
    /// </summary>
    public const string SufixoSemEmail = "@sem-email.vortice.invalid";

    /// <summary>Se esta conta ainda carrega o nome principal inventado pela carga.</summary>
    public bool AindaNaoEntrouPeloEntraId =>
        NomePrincipal.EndsWith(SufixoSemEmail, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Liga a conta ao Microsoft Entra ID, no primeiro login.
    ///
    /// <para><b>Por que o vínculo é gravado, e não refeito a cada acesso:</b> o casamento do
    /// primeiro login é por nome — a parte antes do <c>@</c>, que é o único dado que o Vórtice e o
    /// Entra têm em comum. Nome muda (casamento, correção de grafia) e nome se repete. O
    /// identificador de objeto do Entra não muda nunca. Casar por nome UMA vez e passar a casar
    /// pelo identificador é o que impede que a troca de sobrenome de alguém, daqui a um ano,
    /// entregue a carteira dela a outra pessoa.</para>
    ///
    /// <para>O nome principal e o e-mail passam a ser os reais, e o <c>.invalid</c> some — é assim
    /// que a base se corrige sozinha, conta a conta, sem carga nenhuma.</para>
    /// </summary>
    /// <param name="identidadeExterna">O identificador de objeto (<c>oid</c>) do Entra ID.</param>
    /// <param name="nomePrincipal">O nome principal real (<c>preferred_username</c>).</param>
    /// <param name="email">O e-mail corporativo real.</param>
    /// <param name="agoraUtc">O instante do login.</param>
    public void VincularAoEntraId(Guid identidadeExterna, string nomePrincipal, Email email, DateTime agoraUtc)
    {
        if (identidadeExterna == Guid.Empty)
            throw new RegraDeNegocioViolada("O Entra ID não devolveu o identificador de objeto da conta.");

        if (string.IsNullOrWhiteSpace(nomePrincipal))
            throw new RegraDeNegocioViolada("O Entra ID não devolveu o nome principal da conta.");

        IdentidadeExterna = identidadeExterna;
        NomePrincipal = nomePrincipal.Trim();
        Email = email;
        UltimoLoginEm = agoraUtc;

        // Quem altera é a própria pessoa: o vínculo acontece no login dela, e a trilha de
        // auditoria precisa dizer isso — e não "sistema".
        MarcarAlteracao(Id);
    }

    /// <summary>
    /// Registra um acesso, sem gravar a cada requisição.
    ///
    /// <para>A identidade é resolvida em TODA chamada à API — a Visão 360 dispara dezenas por tela.
    /// Gravar o último acesso em cada uma transformaria leitura em escrita e poria o cadastro de
    /// usuário em disputa de bloqueio. Uma hora de resolução basta para a pergunta que este campo
    /// responde: "essa pessoa ainda usa o sistema?".</para>
    /// </summary>
    /// <param name="agoraUtc">O instante do acesso.</param>
    /// <returns>Verdadeiro quando houve mudança a gravar.</returns>
    public bool RegistrarAcesso(DateTime agoraUtc)
    {
        if (UltimoLoginEm is { } ultimo && agoraUtc - ultimo < TimeSpan.FromHours(1)) return false;

        UltimoLoginEm = agoraUtc;
        return true;
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
