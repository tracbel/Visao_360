namespace Tracbel.Crm.Dominio.Seguranca;

/// <summary>
/// O CATÁLOGO DE PERMISSÕES — em código, versionado com as rotas que as exigem (decisão D-3,
/// 21/09/2026).
///
/// <para><b>Por que em código, e não numa tabela.</b> Uma permissão só existe porque alguma rota ou
/// caso de uso a confere; ela nasce no mesmo commit que o código que a usa, e o compilador pega o nome
/// errado. Uma tabela <c>Permissao</c> ao lado seria uma segunda lista, livre para divergir — foi o que
/// aconteceu com a antiga, que nunca teve uma linha (documento 41, fase 1). Os <b>perfis</b>, que
/// agrupam permissões, continuam em tabela e são editáveis.</para>
///
/// <para><b>Uma permissão é entidade + verbo</b> (<c>Cliente.Editar</c>). O que um perfil concede é a
/// permissão e a <see cref="Profundidade"/> em que ela vale.</para>
/// </summary>
public static class Permissoes
{
    /// <summary>Ler clientes.</summary>
    public const string ClienteLer = "Cliente.Ler";

    /// <summary>Cadastrar cliente.</summary>
    public const string ClienteCriar = "Cliente.Criar";

    /// <summary>Alterar cliente.</summary>
    public const string ClienteEditar = "Cliente.Editar";

    /// <summary>Excluir (inativar) cliente.</summary>
    public const string ClienteExcluir = "Cliente.Excluir";

    /// <summary>Ler o parque de máquinas.</summary>
    public const string EquipamentoLer = "Equipamento.Ler";

    /// <summary>Cadastrar equipamento.</summary>
    public const string EquipamentoCriar = "Equipamento.Criar";

    /// <summary>Alterar equipamento.</summary>
    public const string EquipamentoEditar = "Equipamento.Editar";

    /// <summary>Dar baixa em equipamento.</summary>
    public const string EquipamentoExcluir = "Equipamento.Excluir";

    /// <summary>Ler catálogos (listas de seleção) e o catálogo de municípios.</summary>
    public const string CatalogoLer = "Catalogo.Ler";

    /// <summary>Ler processos (oportunidades).</summary>
    public const string ProcessoLer = "Processo.Ler";

    /// <summary>Ler tarefas e a agenda.</summary>
    public const string TarefaLer = "Tarefa.Ler";

    /// <summary>Ler interações.</summary>
    public const string InteracaoLer = "Interacao.Ler";

    /// <summary>Ler a cobertura de carteira.</summary>
    public const string CoberturaLer = "Cobertura.Ler";

    /// <summary>Ler os relatórios comerciais: funil, perdas, vendas perdidas e painel do CEN.</summary>
    public const string RelatorioLer = "Relatorio.Ler";

    /// <summary>Ler o faturamento e os indicadores executivos.</summary>
    public const string FaturamentoLer = "Faturamento.Ler";

    /// <summary>Ler os indicadores geográficos e as fontes públicas (preços, custos, crédito).</summary>
    public const string TerritorioLer = "Territorio.Ler";

    /// <summary>Ler a situação das integrações.</summary>
    public const string IntegracaoLer = "Integracao.Ler";

    /// <summary>Ler a ponte do sistema legado (congelado).</summary>
    public const string LegadoLer = "Legado.Ler";

    /// <summary>Abrir, com motivo registrado, o alcance entre filiais. Ver <see cref="ContextoAcesso.PermissaoDeAlcanceEntreEmpresas"/>.</summary>
    public const string EmpresaAlcanceEntreFiliais = ContextoAcesso.PermissaoDeAlcanceEntreEmpresas;

    /// <summary>Administrar perfis: criar, editar, ativar e desativar.</summary>
    public const string PerfilAdministrar = "Perfil.Administrar";

    /// <summary>Administrar usuários e as concessões de perfil.</summary>
    public const string UsuarioAdministrar = "Usuario.Administrar";

    /// <summary>Ler os parâmetros do potencial de mercado e o histórico das vigências (issue 71).</summary>
    public const string ParametroDoPotencialLer = "ParametroDoPotencial.Ler";

    /// <summary>
    /// Registrar e revogar vigências da regra por cultura e dos parâmetros gerais do potencial (issue 71).
    /// Parâmetro errado muda o potencial inteiro — por isso é permissão de administrador.
    /// </summary>
    public const string ParametroDoPotencialAdministrar = "ParametroDoPotencial.Administrar";

    /// <summary>Informar e revogar a percepção do gestor comercial por município (issue 71, D-P04).</summary>
    public const string PercepcaoDoGestorInformar = "PercepcaoDoGestor.Informar";

    /// <summary>
    /// Todas as permissões que existem, com o que cada uma deixa fazer. É a lista que o perfil aceita:
    /// conceder um código fora dela é recusado.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> Catalogo = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [ClienteLer] = "Ler clientes",
        [ClienteCriar] = "Cadastrar cliente",
        [ClienteEditar] = "Alterar cliente",
        [ClienteExcluir] = "Excluir cliente",
        [EquipamentoLer] = "Ler o parque de máquinas",
        [EquipamentoCriar] = "Cadastrar equipamento",
        [EquipamentoEditar] = "Alterar equipamento",
        [EquipamentoExcluir] = "Dar baixa em equipamento",
        [CatalogoLer] = "Ler catálogos e municípios",
        [ProcessoLer] = "Ler processos",
        [TarefaLer] = "Ler tarefas e a agenda",
        [InteracaoLer] = "Ler interações",
        [CoberturaLer] = "Ler a cobertura de carteira",
        [RelatorioLer] = "Ler os relatórios comerciais",
        [FaturamentoLer] = "Ler o faturamento e os indicadores executivos",
        [TerritorioLer] = "Ler os indicadores geográficos e as fontes públicas",
        [IntegracaoLer] = "Ler a situação das integrações",
        [LegadoLer] = "Ler a ponte do sistema legado",
        [EmpresaAlcanceEntreFiliais] = "Abrir o alcance entre filiais, com motivo registrado",
        [PerfilAdministrar] = "Administrar perfis",
        [UsuarioAdministrar] = "Administrar usuários e concessões",
        [ParametroDoPotencialLer] = "Ler os parâmetros do potencial de mercado",
        [ParametroDoPotencialAdministrar] = "Alterar os parâmetros do potencial de mercado, com vigência",
        [PercepcaoDoGestorInformar] = "Informar a percepção do gestor por município"
    };

    /// <summary>A permissão existe no catálogo?</summary>
    /// <param name="codigo">O código.</param>
    public static bool Existe(string codigo) => Catalogo.ContainsKey(codigo);
}

/// <summary>
/// OS PERFIS QUE O SISTEMA SEMEIA — o ponto de partida, editável depois pelo administrador.
///
/// <para><b>O perfil padrão (Q-P2, 21/09/2026) é o mínimo, sem excluir.</b> Até a fase 3, uma lista fixa
/// em código dava a TODO usuário criar, editar e <b>excluir</b> cliente e equipamento, sem ninguém ter
/// decidido isso. Agora todo usuário recebe ler, criar e editar cliente e equipamento e ler as telas;
/// excluir e a visão entre filiais são perfis próprios, concedidos a quem precisar, com registro.</para>
///
/// <para><b>Profundidade <see cref="Profundidade.EmpresaEAbaixo"/></b>: a filial escolhida e as que estão
/// abaixo dela — o mesmo alcance de antes. O administrador é a exceção: tudo em
/// <see cref="Profundidade.Organizacao"/>.</para>
/// </summary>
public static class PerfisDeSistema
{
    /// <summary>O código do perfil que todo usuário recebe.</summary>
    public const string Padrao = "PADRAO";

    /// <summary>O código do perfil que acrescenta excluir cliente e equipamento.</summary>
    public const string ExclusaoDeCadastro = "EXCLUSAO_DE_CADASTRO";

    /// <summary>O código do perfil que permite abrir o alcance entre filiais.</summary>
    public const string VisaoEntreFiliais = "VISAO_ENTRE_FILIAIS";

    /// <summary>O código do perfil de administração.</summary>
    public const string Administrador = "ADMINISTRADOR";

    /// <summary>O código do perfil do gestor comercial, que informa a percepção por município (issue 71).</summary>
    public const string GestorComercial = "GESTOR_COMERCIAL";

    /// <summary>Um perfil semeado.</summary>
    /// <param name="Id">O identificador fixo da semente.</param>
    /// <param name="Codigo">O código estável.</param>
    /// <param name="Nome">O nome legível.</param>
    /// <param name="Descricao">Para que serve.</param>
    /// <param name="EhPadrao">Se é o perfil que todo usuário recebe.</param>
    /// <param name="Permissoes">As permissões e a profundidade de cada uma.</param>
    public sealed record Semente(
        int Id, string Codigo, string Nome, string Descricao, bool EhPadrao,
        IReadOnlyList<(string Codigo, Profundidade Profundidade)> Permissoes);

    private static readonly string[] LeituraDasTelas =
    [
        Seguranca.Permissoes.ClienteLer, Seguranca.Permissoes.EquipamentoLer, Seguranca.Permissoes.CatalogoLer,
        Seguranca.Permissoes.ProcessoLer, Seguranca.Permissoes.TarefaLer, Seguranca.Permissoes.InteracaoLer,
        Seguranca.Permissoes.CoberturaLer, Seguranca.Permissoes.RelatorioLer, Seguranca.Permissoes.FaturamentoLer,
        Seguranca.Permissoes.TerritorioLer, Seguranca.Permissoes.IntegracaoLer, Seguranca.Permissoes.LegadoLer
    ];

    /// <summary>
    /// AS PERMISSÕES QUE NASCERAM DEPOIS DA PRIMEIRA SEMENTE (fase 3, 21/09/2026). Entram sempre no FIM da
    /// lista de cada perfil: o identificador semeado de <c>PerfilPermissao</c> é 100 × perfil + ordem, e uma
    /// permissão inserida no meio renumeraria as seguintes — a migração reescreveria linhas que já existem no
    /// banco, em vez de só acrescentar.
    /// </summary>
    private static readonly string[] AcrescentadasDepoisDaSemente =
    [
        Seguranca.Permissoes.ParametroDoPotencialLer,
        Seguranca.Permissoes.ParametroDoPotencialAdministrar,
        Seguranca.Permissoes.PercepcaoDoGestorInformar
    ];

    /// <summary>Os perfis semeados, na ordem dos identificadores.</summary>
    public static readonly IReadOnlyList<Semente> Todos =
    [
        new(1, Padrao, "Padrão",
            "O que todo usuário recebe: ler as telas e cadastrar e alterar cliente e equipamento. Sem excluir e sem visão entre filiais.",
            EhPadrao: true,
            [
                .. LeituraDasTelas.Select(p => (p, Profundidade.EmpresaEAbaixo)),
                (Seguranca.Permissoes.ClienteCriar, Profundidade.EmpresaEAbaixo),
                (Seguranca.Permissoes.ClienteEditar, Profundidade.EmpresaEAbaixo),
                (Seguranca.Permissoes.EquipamentoCriar, Profundidade.EmpresaEAbaixo),
                (Seguranca.Permissoes.EquipamentoEditar, Profundidade.EmpresaEAbaixo),

                // Issue 71: todo número do potencial sai com o parâmetro que o gerou, e quem vê o número pode
                // ver o parâmetro. Alterar é do administrador.
                (Seguranca.Permissoes.ParametroDoPotencialLer, Profundidade.EmpresaEAbaixo)
            ]),

        new(2, ExclusaoDeCadastro, "Exclusão de cadastro",
            "Acrescenta excluir cliente e dar baixa em equipamento.",
            EhPadrao: false,
            [
                (Seguranca.Permissoes.ClienteExcluir, Profundidade.EmpresaEAbaixo),
                (Seguranca.Permissoes.EquipamentoExcluir, Profundidade.EmpresaEAbaixo)
            ]),

        new(3, VisaoEntreFiliais, "Visão entre filiais",
            "Permite abrir, com motivo registrado, o alcance entre filiais (visão da empresa).",
            EhPadrao: false,
            [(Seguranca.Permissoes.EmpresaAlcanceEntreFiliais, Profundidade.Organizacao)]),

        // TODAS AS PERMISSÕES, EM TODA A ORGANIZAÇÃO (decisão de 21/09/2026: "o perfil admin tem todas as
        // filiais e todos os recursos"). Até então a maioria valia em EmpresaEAbaixo — com as 16 filiais
        // todas raiz, isso era "só a filial escolhida". A ordem não muda: só a profundidade, e a migração
        // atualiza as linhas que já existem em vez de apagar e recriar.
        new(4, Administrador, "Administrador",
            "Todas as permissões, em todas as filiais: tudo o que o padrão dá, mais excluir, a visão de todas as filiais, a administração de perfis e usuários e os parâmetros do potencial.",
            EhPadrao: false,
            [
                .. Seguranca.Permissoes.Catalogo.Keys
                    .Where(p => p is not Seguranca.Permissoes.EmpresaAlcanceEntreFiliais
                        and not Seguranca.Permissoes.PerfilAdministrar
                        and not Seguranca.Permissoes.UsuarioAdministrar
                        && !AcrescentadasDepoisDaSemente.Contains(p))
                    .Select(p => (p, Profundidade.Organizacao)),
                (Seguranca.Permissoes.EmpresaAlcanceEntreFiliais, Profundidade.Organizacao),
                (Seguranca.Permissoes.PerfilAdministrar, Profundidade.Organizacao),
                (Seguranca.Permissoes.UsuarioAdministrar, Profundidade.Organizacao),

                // Depois da primeira semente — no fim, para não renumerar.
                (Seguranca.Permissoes.ParametroDoPotencialLer, Profundidade.Organizacao),
                (Seguranca.Permissoes.ParametroDoPotencialAdministrar, Profundidade.Organizacao),
                (Seguranca.Permissoes.PercepcaoDoGestorInformar, Profundidade.Organizacao)
            ]),

        new(5, GestorComercial, "Gestor comercial",
            "Acrescenta informar a percepção do gestor sobre cada município, dentro do limite dos parâmetros gerais (issue 71, D-P04). Concedido a quem responde pelo território.",
            EhPadrao: false,
            [(Seguranca.Permissoes.PercepcaoDoGestorInformar, Profundidade.Organizacao)])
    ];
}
