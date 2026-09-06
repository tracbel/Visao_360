using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Crm;
using Tracbel.Crm.Dominio.Documento;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Relatorio;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Dominio.Workflow;

namespace Tracbel.Crm.Infraestrutura.Persistencia;

/// <summary>
/// O contexto de persistência do CRM, sobre SQL Server 2022.
///
/// AQUI MORA O ÚNICO PONTO DE APLICAÇÃO DA SEGURANÇA POR LINHA. Nenhuma consulta escapa:
/// nem tela, nem relatório, nem exportação, nem job, nem integração.
///
/// [V] No Vórtice não é assim. A segurança roda 100% no cliente Gupta, e por isso
/// **125 dos 134 relatórios não têm nenhum predicado de usuário** — o filtro da tela
/// simplesmente não vale para o relatório, e o picklist de "Carteiras" entrega a carteira
/// de todo mundo. Acesso direto ao banco ignora tudo e não deixa rastro.
/// </summary>
public class CrmDbContext : DbContext
{
    /// <summary>
    /// Comparação de texto que ignora caixa E acento.
    ///
    /// É a colação nativa do SQL Server: <c>CI</c> de *case-insensitive*, <c>AI</c> de
    /// *accent-insensitive*. "Preço", "PRECO" e "preco" passam a ser o MESMO valor para
    /// efeito de igualdade, de <c>LIKE</c> e de unicidade de índice — é o que impede
    /// FINALIZADO e FINALIZADA de conviverem no mesmo catálogo ([V] o defeito medido em
    /// 30 mil linhas do Vórtice) e é também o que faz "Jose" encontrar "José" na busca por
    /// nome, sem nenhuma coluna derivada.
    ///
    /// Vale para o BANCO INTEIRO (aplicada em <c>OnModelCreating</c>) e é repetida
    /// explicitamente nas duas colunas onde a garantia é estrutural — a razão social do
    /// cliente e o rótulo do item de catálogo —, para que a intenção sobreviva a um
    /// <c>restore</c> num servidor com outra colação padrão.
    ///
    /// [V] O Vórtice usa <c>SQL_Latin1_General_CP1_CI_AS</c>: *accent-SENSITIVE*. Por isso,
    /// lá, buscar "Jose" não encontra "José".
    /// </summary>
    public const string ColacaoSemCaixaNemAcento = "Latin1_General_CI_AI";

    /// <summary>
    /// Colação binária, usada só dentro de restrição de verificação que precisa distinguir
    /// caixa (ex.: a UF em duas maiúsculas, o resumo do documento em hexadecimal minúsculo).
    ///
    /// Sem ela, sob a colação do banco (que é CI_AI de propósito), <c>LIKE '[A-Z][A-Z]'</c>
    /// aceitaria "sp" e "sP" — o CHECK deixaria de checar o que diz checar.
    /// </summary>
    public const string ColacaoBinaria = "Latin1_General_BIN2";

    /// <summary>
    /// O nome da coluna que É a fronteira de multiempresa. Não existe segunda.
    ///
    /// Documento 14, regra 11.1: <c>EmpresaId</c> é a ÚNICA fronteira — proibido schema por
    /// filial, banco por filial ou tabela replicada por filial.
    /// </summary>
    public const string ColunaDeEmpresa = "EmpresaId";

    /// <summary>
    /// AS EXCEÇÕES DA FRONTEIRA DE EMPRESA — nomeadas, justificadas e contadas.
    ///
    /// Toda entidade que declara <see cref="ColunaDeEmpresa"/> recebe o filtro global de
    /// empresa em <see cref="AplicarFronteiraDeEmpresa"/>. Estas são as únicas que não
    /// recebem, e cada uma está aqui com o motivo escrito. É o mesmo mecanismo de
    /// <c>CascadeJustificado</c> e <c>TextoIlimitadoJustificado</c>: a exceção existe, mas
    /// não existe em silêncio.
    ///
    /// <c>MultiempresaTestes</c> vigia esta lista dos dois lados — entidade nova com
    /// <c>EmpresaId</c> e sem filtro reprova o build, e entrada nova aqui reprova até que a
    /// exceção seja registrada também no teste e no documento 21.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> FronteiraDeEmpresaJustificada =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // O EmpresaId do usuário NÃO é a filial dona da linha — é a filial DE CASA dele,
            // o dado que DEFINE o escopo dos outros filtros ("Empresa de casa do usuário:
            // define o escopo padrão dele"). Filtrar o cadastro de usuário por ele seria
            // circular: o ContextoAcesso é montado LENDO esta tabela, antes de existir
            // qualquer escopo — a consulta de login não devolveria ninguém. E, mesmo depois de
            // montado, quebraria o que a fronteira não deve quebrar: mostrar o nome de quem
            // criou ou alterou um registro transferido entre filiais, e resolver a hierarquia
            // comercial, que atravessa filial por desenho (documento 05, seção 5).
            [nameof(Usuario)] =
                "EmpresaId aqui é a filial DE CASA do usuário — o dado que define o escopo, não " +
                "a linha protegida por ele. O ContextoAcesso é montado lendo esta tabela, antes " +
                "de existir escopo; filtrá-la tornaria o login e a hierarquia entre filiais " +
                "impossíveis. O que limita a exposição do usuário é a camada 5, de campo sensível.",

            // A camada 4 do documento 05 é ADITIVA e existe justamente para atravessar a
            // fronteira: é o que deixa o gerente ver uma oportunidade de outra filial sem
            // trocar o dono dela. O EmpresaId da linha é a filial DO REGISTRO COMPARTILHADO —
            // por definição a outra. Filtrar por ela apagaria o compartilhamento entre filiais
            // do mapa e faria o endpoint "por que eu vejo isto" (seção 6) mentir.
            // A fronteira devida aqui é o BENEFICIÁRIO (UsuarioId/EquipeId), e ela entra junto
            // com a camada 4 — registrado no documento 21, seção de correções aplicadas.
            [nameof(CompartilhamentoDeRegistro)] =
                "é a camada 4 do documento 05, aditiva e desenhada para ATRAVESSAR a fronteira: " +
                "o EmpresaId da linha é a filial do registro compartilhado, quase sempre a outra. " +
                "Filtrar por empresa tornaria o compartilhamento entre filiais invisível para " +
                "quem o recebeu. A fronteira devida é o beneficiário (UsuarioId/EquipeId)."
        };

    /// <summary>
    /// A entidade que tem filtro PRÓPRIO, mais restritivo que a fronteira de empresa, e por
    /// isso não recebe o filtro genérico: o dela já contém a fronteira, somada à profundidade
    /// da permissão (documento 05, seção 5).
    /// </summary>
    private static readonly IReadOnlySet<string> FiltroProprio =
        new HashSet<string>(StringComparer.Ordinal) { nameof(Lead) };

    private readonly IProvedorContextoAcesso _provedorAcesso;
    private readonly IDiarioDeAlcanceEntreEmpresas? _diario;

    // ---------------------------------------------------------------------
    // Valores pré-computados por requisição.
    //
    // POR QUE PRÉ-COMPUTAR: o filtro global do EF Core vira árvore de expressão traduzida
    // para SQL. Não dá para chamar ContextoAcesso.AlcancaRegistro() dentro dele — o EF não
    // sabe traduzir um método. Então resolvemos a lógica de profundidade UMA VEZ, no
    // construtor, e o filtro fica sendo álgebra booleana simples, que o EF traduz sem esforço.
    //
    // O efeito colateral é bom: a decisão de profundidade não roda por linha.
    // ---------------------------------------------------------------------
    private readonly long _usuarioId;
    private readonly int _empresaId;
    private readonly bool _ehSistema;
    private readonly IReadOnlySet<int> _empresasVisiveis;
    private readonly IReadOnlySet<long> _subordinadosIds;

    private readonly bool _leadVeTudo;
    private readonly bool _leadVeEmpresaEAbaixo;
    private readonly bool _leadVeEmpresa;
    private readonly bool _leadVeEquipe;
    private readonly bool _leadVeProprios;

    // ---------------------------------------------------------------------
    // A ÚNICA coisa aqui que NÃO é pré-computada: a via de escape.
    //
    // Ela é mutável de propósito — muda quando alguém chama
    // AbrirAlcanceEntreEmpresas(motivo) e volta ao normal quando o escopo fecha. O filtro
    // global do EF lê este campo NO MOMENTO DA CONSULTA (o EF o transforma em parâmetro),
    // e é isso que faz o escape valer para as consultas dentro do escopo e para nenhuma
    // outra.
    // ---------------------------------------------------------------------
    private bool _alcanceEntreEmpresas;

    /// <summary>Cria o contexto com o escopo de acesso da requisição.</summary>
    /// <param name="opcoes">Opções do provedor.</param>
    /// <param name="provedorAcesso">Quem está agindo nesta requisição.</param>
    /// <param name="diario">
    /// Onde a abertura da via de escape é registrada. Sem ele,
    /// <see cref="AbrirAlcanceEntreEmpresas"/> se recusa a abrir: escape sem rastro não é
    /// escape legítimo. É opcional só para que o caminho de migração e os testes de esquema,
    /// que nunca abrem escape nenhum, não precisem montá-lo.
    /// </param>
    public CrmDbContext(
        DbContextOptions<CrmDbContext> opcoes,
        IProvedorContextoAcesso provedorAcesso,
        IDiarioDeAlcanceEntreEmpresas? diario = null)
        : base(opcoes)
    {
        _provedorAcesso = provedorAcesso;
        _diario = diario;
        var acesso = provedorAcesso.Atual;

        _usuarioId = acesso.UsuarioId;
        _empresaId = acesso.EmpresaId;
        _ehSistema = acesso.EhServicoDeSistema;
        _empresasVisiveis = acesso.EmpresasVisiveis;
        _subordinadosIds = acesso.SubordinadosIds;

        var lead = acesso.ProfundidadeDe("Lead.Ler");
        _leadVeTudo = lead >= Profundidade.Organizacao;
        _leadVeEmpresaEAbaixo = lead >= Profundidade.EmpresaEAbaixo;
        _leadVeEmpresa = lead >= Profundidade.Empresa;
        _leadVeEquipe = lead >= Profundidade.Equipe;
        _leadVeProprios = lead >= Profundidade.Proprios;
    }

    // ---- organizacao ----

    /// <summary>Filiais da Tracbel.</summary>
    public DbSet<Empresa> Empresas => Set<Empresa>();

    /// <summary>Linhas de negócio.</summary>
    public DbSet<LinhaDeNegocio> LinhasDeNegocio => Set<LinhaDeNegocio>();

    /// <summary>Carteiras.</summary>
    public DbSet<Carteira> Carteiras => Set<Carteira>();

    /// <summary>A hierarquia comercial materializada.</summary>
    public DbSet<HierarquiaComercial> HierarquiaComercial => Set<HierarquiaComercial>();

    /// <summary>Metas de faturamento, cobertura e frequência.</summary>
    public DbSet<Meta> Metas => Set<Meta>();

    /// <summary>Praças de mercado.</summary>
    public DbSet<Praca> Pracas => Set<Praca>();

    /// <summary>Municípios — o catálogo nacional que fecha o campo de município.</summary>
    public DbSet<Municipio> Municipios => Set<Municipio>();

    /// <summary>Os municípios que cada carteira atende.</summary>
    public DbSet<CarteiraMunicipio> CarteiraMunicipios => Set<CarteiraMunicipio>();

    // ---- seguranca ----

    /// <summary>Usuários.</summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    /// <summary>Equipes.</summary>
    public DbSet<Equipe> Equipes => Set<Equipe>();

    /// <summary>Composição das equipes.</summary>
    public DbSet<EquipeMembro> EquipeMembros => Set<EquipeMembro>();

    /// <summary>Catálogo de permissões.</summary>
    public DbSet<Permissao> Permissoes => Set<Permissao>();

    /// <summary>Conjuntos de permissão.</summary>
    public DbSet<ConjuntoPermissao> ConjuntosPermissao => Set<ConjuntoPermissao>();

    /// <summary>Concessões de conjunto a usuário.</summary>
    public DbSet<UsuarioConjuntoPermissao> ConcessoesPermissao => Set<UsuarioConjuntoPermissao>();

    /// <summary>Compartilhamentos pontuais de registro.</summary>
    public DbSet<CompartilhamentoDeRegistro> CompartilhamentosDeRegistro => Set<CompartilhamentoDeRegistro>();

    // ---- comercial ----

    /// <summary>Clientes.</summary>
    public DbSet<Cliente> Clientes => Set<Cliente>();

    /// <summary>Contatos.</summary>
    public DbSet<Contato> Contatos => Set<Contato>();

    /// <summary>Vínculos entre cliente e contato.</summary>
    public DbSet<ClienteContato> ClienteContatos => Set<ClienteContato>();

    /// <summary>Telefones, e-mails e WhatsApp.</summary>
    public DbSet<CanalContato> CanaisDeContato => Set<CanalContato>();

    /// <summary>Endereços e fazendas.</summary>
    public DbSet<Endereco> Enderecos => Set<Endereco>();

    /// <summary>Consentimentos de comunicação.</summary>
    public DbSet<ConsentimentoComunicacao> Consentimentos => Set<ConsentimentoComunicacao>();

    /// <summary>Carteirização de clientes.</summary>
    public DbSet<ClienteCarteira> ClienteCarteiras => Set<ClienteCarteira>();

    /// <summary>Leads.</summary>
    public DbSet<Lead> Leads => Set<Lead>();

    /// <summary>Alertas fixados no cliente.</summary>
    public DbSet<Alerta> Alertas => Set<Alerta>();

    // ---- processo ----

    /// <summary>Modelos de fluxo.</summary>
    public DbSet<TipoProcesso> TiposDeProcesso => Set<TipoProcesso>();

    /// <summary>Fases dos fluxos.</summary>
    public DbSet<Fase> Fases => Set<Fase>();

    /// <summary>Tipos de tarefa.</summary>
    public DbSet<TipoTarefa> TiposDeTarefa => Set<TipoTarefa>();

    /// <summary>Desfechos.</summary>
    public DbSet<Resultado> Resultados => Set<Resultado>();

    /// <summary>Processos.</summary>
    public DbSet<Dominio.Processo.Processo> Processos => Set<Dominio.Processo.Processo>();

    /// <summary>Passagens de fase.</summary>
    public DbSet<PassagemDeFase> PassagensDeFase => Set<PassagemDeFase>();

    /// <summary>Tarefas.</summary>
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    /// <summary>Interações.</summary>
    public DbSet<Interacao> Interacoes => Set<Interacao>();

    /// <summary>Participantes de interação.</summary>
    public DbSet<InteracaoParticipante> InteracaoParticipantes => Set<InteracaoParticipante>();

    /// <summary>Regras de automação.</summary>
    public DbSet<Regra> Regras => Set<Regra>();

    /// <summary>O log de execução de regra — a tabela que torna o silêncio impossível.</summary>
    public DbSet<ExecucaoRegra> ExecucoesRegra => Set<ExecucaoRegra>();

    /// <summary>Motivos de perda.</summary>
    public DbSet<MotivoDePerda> MotivosDePerda => Set<MotivoDePerda>();

    /// <summary>Itens de proposta.</summary>
    public DbSet<ItemDeProposta> ItensDeProposta => Set<ItemDeProposta>();

    /// <summary>
    /// Vendas perdidas para a concorrência — para quem, com que máquina e por quanto.
    ///
    /// Herda a fronteira de multiempresa sem uma linha a mais: o filtro global é aplicado a
    /// toda entidade que tenha <c>EmpresaId</c>, e esta tem.
    /// </summary>
    public DbSet<VendaPerdida> VendasPerdidas => Set<VendaPerdida>();

    /// <summary>
    /// O faturamento por cliente, filial e mês — a base da curva ABC e da série de doze meses.
    /// </summary>
    public DbSet<FaturamentoDoCliente> FaturamentoDosClientes => Set<FaturamentoDoCliente>();

    // ---- frota ----

    /// <summary>Marcas de máquina.</summary>
    public DbSet<Marca> Marcas => Set<Marca>();

    /// <summary>Famílias de máquina.</summary>
    public DbSet<Familia> Familias => Set<Familia>();

    /// <summary>Modelos de máquina.</summary>
    public DbSet<Modelo> Modelos => Set<Modelo>();

    /// <summary>Máquinas dos clientes.</summary>
    public DbSet<Equipamento> Equipamentos => Set<Equipamento>();

    /// <summary>Leituras de horímetro.</summary>
    public DbSet<LeituraDeHorimetro> LeiturasDeHorimetro => Set<LeituraDeHorimetro>();

    // ---- documento ----

    /// <summary>Arquivos anexados.</summary>
    public DbSet<Dominio.Documento.Documento> Documentos => Set<Dominio.Documento.Documento>();

    /// <summary>Vínculos de documento.</summary>
    public DbSet<Vinculo> VinculosDeDocumento => Set<Vinculo>();

    // ---- auditoria ----

    /// <summary>Campos auditados.</summary>
    public DbSet<CampoAuditado> CamposAuditados => Set<CampoAuditado>();

    /// <summary>Alterações de campo.</summary>
    public DbSet<AlteracaoDeCampo> AlteracoesDeCampo => Set<AlteracaoDeCampo>();

    /// <summary>Eventos de acesso.</summary>
    public DbSet<EventoDeAcesso> EventosDeAcesso => Set<EventoDeAcesso>();

    // ---- integracao ----

    /// <summary>Sistemas externos.</summary>
    public DbSet<Sistema> Sistemas => Set<Sistema>();

    /// <summary>De-para de identificadores.</summary>
    public DbSet<ChaveExterna> ChavesExternas => Set<ChaveExterna>();

    /// <summary>Pontos de sincronismo por fluxo.</summary>
    public DbSet<PontoDeSincronismo> PontosDeSincronismo => Set<PontoDeSincronismo>();

    /// <summary>Área de pouso efêmera da integração.</summary>
    public DbSet<Recepcao> Recepcoes => Set<Recepcao>();

    /// <summary>Fila de saída.</summary>
    public DbSet<MensagemDeSaida> MensagensDeSaida => Set<MensagemDeSaida>();

    /// <summary>Fila de descarte.</summary>
    public DbSet<MensagemDescartada> MensagensDescartadas => Set<MensagemDescartada>();

    // ---- metadado ----

    /// <summary>Campos personalizados declarados.</summary>
    public DbSet<CampoPersonalizado> CamposPersonalizados => Set<CampoPersonalizado>();

    /// <summary>Tratadores de evento declarados.</summary>
    public DbSet<TratadorDeEvento> TratadoresDeEvento => Set<TratadorDeEvento>();

    /// <summary>Catálogos.</summary>
    public DbSet<Catalogo> Catalogos => Set<Catalogo>();

    /// <summary>Itens de catálogo.</summary>
    public DbSet<CatalogoItem> CatalogoItens => Set<CatalogoItem>();

    /// <summary>Definições de formulário.</summary>
    public DbSet<Formulario> Formularios => Set<Formulario>();

    /// <summary>Perguntas de formulário.</summary>
    public DbSet<Pergunta> Perguntas => Set<Pergunta>();

    /// <summary>Preenchimentos de formulário.</summary>
    public DbSet<Preenchimento> Preenchimentos => Set<Preenchimento>();

    /// <summary>Respostas de formulário.</summary>
    public DbSet<Resposta> Respostas => Set<Resposta>();

    // ---- relatorio ----

    /// <summary>Fontes curadas de relatório.</summary>
    public DbSet<Fonte> FontesDeRelatorio => Set<Fonte>();

    /// <summary>Campos das fontes de relatório.</summary>
    public DbSet<FonteCampo> CamposDeFonte => Set<FonteCampo>();

    /// <summary>Relatórios salvos pelo usuário.</summary>
    public DbSet<Dominio.Relatorio.Relatorio> Relatorios => Set<Dominio.Relatorio.Relatorio>();

    /// <summary>Expõe o provedor para os repositórios que precisam do contexto completo.</summary>
    public ContextoAcesso Acesso => _provedorAcesso.Atual;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.HasDefaultSchema("comercial");

        // -----------------------------------------------------------------
        // A COLAÇÃO DO BANCO NASCE AQUI, no modelo versionado — documento 14, regra 9.5:
        // nível de compatibilidade e colação nascem num script versionado, nunca de uma
        // configuração feita à mão no servidor. A migração gera
        // `ALTER DATABASE CURRENT COLLATE Latin1_General_CI_AI`, e o
        // `scripts/banco/subir-banco.ps1` já cria o banco com ela.
        //
        // [V] é a lição direta do Vórtice, cujo banco está em nível de compatibilidade 100
        // (SQL 2008) rodando sobre SQL Server 2019 e ninguém sabe dizer quando isso foi
        // fixado assim.
        // -----------------------------------------------------------------
        if (Database.IsSqlServer()) modelo.UseCollation(ColacaoSemCaixaNemAcento);

        modelo.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);

        FecharDominioDoPonteiroPolimorfico(modelo);

        // -----------------------------------------------------------------
        // FILTRO GLOBAL DE SEGURANÇA
        //
        // Aplicado em TODA consulta da entidade, sempre. Não existe caminho de leitura
        // que escape — nem em relatório, nem em exportação, nem em job.
        //
        // A ordem das cláusulas segue a do documento 05: sistema, alcance por
        // profundidade, e por último o compartilhamento explícito.
        //
        // A PRIMEIRA fronteira — a de empresa — é aplicada por varredura do modelo, e não
        // uma entidade por vez: ver AplicarFronteiraDeEmpresa. Depois dela vêm os filtros
        // próprios, mais restritivos, que SUBSTITUEM o genérico na entidade em que existem
        // (o EF Core guarda um filtro por entidade — o último configurado vence). Por isso a
        // ordem aqui importa, e por isso a entidade com filtro próprio está nomeada em
        // FiltroProprio, para a varredura não gastar trabalho que seria descartado.
        // -----------------------------------------------------------------

        AplicarFronteiraDeEmpresa(modelo);

        modelo.Entity<Lead>().HasQueryFilter(l =>
            l.ExcluidoEm == null
            && (_ehSistema
                || _leadVeTudo
                || ((_leadVeEmpresaEAbaixo || _leadVeEmpresa) && _alcanceEntreEmpresas)
                || (_leadVeEmpresaEAbaixo && _empresasVisiveis.Contains(l.EmpresaId))
                || (_leadVeEmpresa && l.EmpresaId == _empresaId)
                || (_leadVeEquipe && _subordinadosIds.Contains(l.ProprietarioId))
                || (_leadVeProprios && l.ProprietarioId == _usuarioId)));

        ConfigurarConcorrenciaOtimista(modelo, Database.IsSqlServer());

        if (!Database.IsSqlServer()) ReduzirParaProvedorDeTeste(modelo);
    }

    /// <summary>
    /// A FRONTEIRA DE EMPRESA, APLICADA POR VARREDURA DO MODELO — não uma entidade por vez.
    ///
    /// O QUE ISTO CORRIGE (documento 21, achado A-1): <c>EmpresaId</c> existia em 21 tabelas e
    /// o filtro global existia em UMA. As outras 20 tinham a coluna e nenhuma proteção — bastava
    /// uma consulta esquecer o <c>WHERE</c> para devolver dado de todas as filiais. O critério
    /// que condena isso é do próprio documento 14, seção 5.2: *"uma coluna que existe mas não é
    /// usada como fronteira de fato não é fronteira nenhuma"*.
    ///
    /// POR QUE VARREDURA, E NÃO 20 LINHAS ESCRITAS À MÃO: vinte linhas escritas à mão são vinte
    /// linhas que a entidade nº 22 não vai ter, porque quem a criar não vai lembrar de escrever
    /// a vigésima primeira. Aqui a regra é "quem tem a coluna ganha o filtro", resolvida contra
    /// o modelo em tempo de construção — ESQUECER DEIXA DE SER POSSÍVEL. O que ainda dá para
    /// fazer é uma exceção, e ela exige entrada em <see cref="FronteiraDeEmpresaJustificada"/>,
    /// que <c>MultiempresaTestes</c> confere nome por nome.
    ///
    /// O PREDICADO, na ordem do documento 05:
    ///   1. serviço de sistema (job, integração) enxerga tudo — e sob identidade explícita;
    ///   2. alcance entre filiais ABERTO por quem declarou o motivo (a via de escape);
    ///   3. a linha é de uma filial que o usuário alcança (<c>EmpresasVisiveis</c>, já expandida
    ///      pela hierarquia de empresa);
    ///   4. a linha é da filial de casa dele.
    ///
    /// Nada aqui é avaliado por linha em memória: o EF traduz tudo para <c>WHERE</c>, e os
    /// campos deste contexto viram parâmetro da consulta. É o mesmo mecanismo do filtro do
    /// <c>Lead</c> — o modelo a repetir, como diz a regra 11.3 do documento 14.
    ///
    /// O QUE ESTE FILTRO **NÃO** FAZ: profundidade por dono (próprios/equipe) e exclusão
    /// lógica. Isso é a camada 3 e o <c>ExcluidoEm</c>, que entram com o filtro próprio de cada
    /// entidade, como já acontece no <c>Lead</c>. A fronteira de empresa é a PRIMEIRA camada, e
    /// é a que não pode faltar em nenhuma.
    /// </summary>
    private void AplicarFronteiraDeEmpresa(ModelBuilder modelo)
    {
        // Os campos deste contexto, referenciados por árvore de expressão. É exatamente o que
        // o compilador de C# gera para o filtro do Lead escrito como lambda — a diferença é
        // que aqui o tipo da entidade só se conhece em tempo de execução.
        var contexto = Expression.Constant(this);
        var ehSistema = Expression.Field(contexto, nameof(_ehSistema));
        var alcanceAberto = Expression.Field(contexto, nameof(_alcanceEntreEmpresas));
        var empresasVisiveis = Expression.Field(contexto, nameof(_empresasVisiveis));
        var empresaDeCasa = Expression.Field(contexto, nameof(_empresaId));

        var contem = typeof(IReadOnlySet<int>).GetMethod(nameof(IReadOnlySet<int>.Contains))!;

        foreach (var tipo in modelo.Model.GetEntityTypes().ToList())
        {
            if (tipo.FindProperty(ColunaDeEmpresa) is null) continue;

            var nome = tipo.ClrType.Name;
            if (FiltroProprio.Contains(nome)) continue;
            if (FronteiraDeEmpresaJustificada.ContainsKey(nome)) continue;

            var linha = Expression.Parameter(tipo.ClrType, "linha");
            var empresaDaLinha = Expression.Property(linha, ColunaDeEmpresa);

            var predicado = Expression.OrElse(
                Expression.OrElse(ehSistema, alcanceAberto),
                Expression.OrElse(
                    Expression.Call(empresasVisiveis, contem, empresaDaLinha),
                    Expression.Equal(empresaDaLinha, empresaDeCasa)));

            modelo.Entity(tipo.ClrType).HasQueryFilter(Expression.Lambda(predicado, linha));
        }
    }

    /// <summary>
    /// A VIA DE ESCAPE — explícita, permissionada e registrada.
    ///
    /// Existe uso legítimo de leitura entre filiais: o administrador que concilia duas filiais,
    /// a integração que exporta a base para o Protheus, a apuração de meta consolidada. A
    /// fronteira não pode tornar isso impossível — mas também não pode cair por descuido. Então
    /// ela cai por DECLARAÇÃO:
    ///
    /// <code>
    /// using (contexto.AbrirAlcanceEntreEmpresas("conciliação de carteira entre Franca e Barretos"))
    /// {
    ///     // aqui, e só aqui, as consultas atravessam a fronteira
    /// }
    /// </code>
    ///
    /// TRÊS EXIGÊNCIAS, e nenhuma é dispensável:
    ///   - o motivo é escrito por quem abre, e nunca em branco;
    ///   - quem abre tem <see cref="ContextoAcesso.PermissaoDeAlcanceEntreEmpresas"/> em
    ///     profundidade Organização (o serviço de sistema já a tem);
    ///   - existe um <see cref="IDiarioDeAlcanceEntreEmpresas"/> para registrar. Sem diário
    ///     configurado, NÃO ABRE — escape sem rastro não é escape legítimo, é furo.
    ///
    /// O escopo é do OBJETO DE CONTEXTO e acaba no <c>Dispose</c>. Quem esquecer de fechar
    /// perde o alcance junto com o contexto, no fim da requisição — o padrão nunca é "aberto".
    /// </summary>
    /// <param name="motivo">Por que a fronteira está sendo ignorada.</param>
    /// <returns>O escopo; fechá-lo devolve a fronteira ao lugar.</returns>
    public IDisposable AbrirAlcanceEntreEmpresas(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException(
                "Ignorar a fronteira de empresa exige motivo escrito — é o que vai ficar no " +
                "diário e é o que alguém vai ler depois.",
                nameof(motivo));

        var acesso = _provedorAcesso.Atual;

        if (!acesso.PodeAlcancarTodasAsEmpresas)
            throw new InvalidOperationException(
                $"{acesso.NomeExibicao} não pode abrir o alcance entre filiais: falta a " +
                $"permissão {ContextoAcesso.PermissaoDeAlcanceEntreEmpresas} em profundidade " +
                "Organização (documento 05, seção 8).");

        if (_diario is null)
            throw new InvalidOperationException(
                "O alcance entre filiais não abre sem um IDiarioDeAlcanceEntreEmpresas " +
                "configurado: quem ignora a fronteira precisa dizer que está ignorando, e isso " +
                "precisa ficar registrado.");

        if (_alcanceEntreEmpresas)
            throw new InvalidOperationException(
                "O alcance entre filiais já está aberto neste contexto. Escopo aninhado " +
                "esconderia quem abriu de verdade — feche o primeiro antes de abrir outro.");

        _diario.Abriu(acesso, motivo);
        _alcanceEntreEmpresas = true;

        return new EscopoDeAlcanceEntreEmpresas(this, acesso, motivo, _diario);
    }

    /// <summary>
    /// A fronteira de empresa está derrubada NESTE contexto, agora?
    ///
    /// Serve ao diagnóstico e ao teste. Nunca é "sim" por padrão: só depois de alguém chamar
    /// <see cref="AbrirAlcanceEntreEmpresas"/>.
    /// </summary>
    public bool EstaComAlcanceEntreEmpresas => _alcanceEntreEmpresas;

    /// <summary>O escopo aberto. Fechá-lo devolve a fronteira e registra a duração.</summary>
    private sealed class EscopoDeAlcanceEntreEmpresas(
        CrmDbContext contexto,
        ContextoAcesso acesso,
        string motivo,
        IDiarioDeAlcanceEntreEmpresas diario) : IDisposable
    {
        private readonly DateTime _abertoEm = DateTime.UtcNow;
        private bool _fechado;

        public void Dispose()
        {
            if (_fechado) return;
            _fechado = true;
            contexto._alcanceEntreEmpresas = false;
            diario.Fechou(acesso, motivo, DateTime.UtcNow - _abertoEm);
        }
    }

    /// <summary>
    /// FECHA O DOMÍNIO DAS COLUNAS <c>Entidade</c>, <c>EntidadeRaiz</c> e <c>Campo</c>.
    ///
    /// O QUE ESTAS COLUNAS SÃO: o par <c>(Entidade, RegistroId)</c> é um ponteiro polimórfico —
    /// <c>documento.Vinculo</c> prende o arquivo a um registro qualquer,
    /// <c>seguranca.CompartilhamentoDeRegistro</c> concede acesso a um registro qualquer,
    /// <c>auditoria.CampoAuditado</c> decide o que se audita. Em cinco das dez tabelas
    /// <c>RegistroId</c> NÃO TEM chave estrangeira, e não tem como ter: o alvo muda de tabela
    /// linha a linha. Isso faz do domínio de <c>Entidade</c> a única defesa que sobra.
    ///
    /// O QUE ACONTECIA SEM ISTO (documento 21, achado I-3): <c>Entidade</c> era
    /// <c>varchar(40)</c> livre. <c>Cliente</c> e <c>Clientes</c> eram registros diferentes para
    /// o banco; um nome errado em <c>CampoAuditado</c> desligava a auditoria daquele campo sem
    /// erro nenhum; e o documento órfão do Vórtice — 5.302 deles, 6,9% do total — voltava a ser
    /// estruturalmente possível, só que com nomes melhores.
    ///
    /// POR QUE RESTRIÇÃO DE VERIFICAÇÃO E NÃO TABELA DE REFERÊNCIA — o critério do documento 14:
    ///   - seção 14: domínio fechado se garante com <c>CHECK</c>; <c>FOREIGN KEY</c> é o
    ///     mecanismo de "a linha filha só existe se a linha pai existir". Aqui não há linha pai:
    ///     o valor válido é o nome de uma CLASSE do modelo, não um registro de negócio;
    ///   - seção 8.3: dado de referência nasce de seed versionado. Uma tabela
    ///     <c>metadado.Entidade</c> seria um seed que COPIA o modelo — duas fontes da verdade
    ///     para a mesma lista, livres para divergir em silêncio. O <c>CHECK</c> aqui é GERADO do
    ///     próprio modelo a cada compilação: não tem como divergir;
    ///   - seções 10 e 13: uma chave estrangeira nessas colunas obrigaria índice por
    ///     <c>Entidade</c> nas três tabelas particionadas de log e de área de pouso, e uma
    ///     consulta a mais por linha gravada, nas tabelas de maior volume do banco.
    ///
    /// QUANDO O MODELO CRESCE: a lista muda, e é para isso que existe
    /// <c>DominioDeEntidadeTestes</c> — ele compara a lista do <c>CHECK</c> com a lista do
    /// modelo e falha se elas divergirem. Entidade nova sem migração que regenere o
    /// <c>CHECK</c> não passa no <c>dotnet test</c>. É o preço, e ele é cobrado no lugar certo.
    ///
    /// <c>Campo</c> FICA COM DOMÍNIO DE FORMA, não de lista: o conjunto válido seria as 745
    /// colunas do modelo, o que faria QUALQUER coluna nova em QUALQUER tabela exigir migração
    /// aqui; e ele não é uniforme — <c>relatorio.FonteCampo.Campo</c> nomeia coluna de VISÃO, e
    /// <c>metadado.CampoPersonalizado.Campo</c> nomeia a coluna que a migração de campo
    /// personalizado ainda vai criar (documento 14, regra 12.1). O que dá para exigir sem
    /// mentir é que seja um identificador do padrão da seção 3 — PascalCase ASCII, sem espaço,
    /// sem pontuação, sem <c>snake_case</c>.
    ///
    /// A COLAÇÃO BINÁRIA É PARTE DA REGRA: sob a colação do banco (<c>CI_AI</c>, de propósito),
    /// <c>IN ('Cliente')</c> aceitaria <c>CLIENTE</c> e <c>cliénte</c>. Aqui o valor é
    /// identificador de máquina, não texto de gente — a comparação é exata, pelo mesmo caminho
    /// que <c>CK_Endereco_Uf</c> e <c>CK_Documento_Resumo</c> já usam (documento 14, seção 14).
    /// </summary>
    private static void FecharDominioDoPonteiroPolimorfico(ModelBuilder modelo)
    {
        var nomesDeEntidade = modelo.Model.GetEntityTypes()
            .Select(t => t.ClrType.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var lista = string.Join(", ", nomesDeEntidade.Select(n => $"'{n}'"));

        foreach (var tipo in modelo.Model.GetEntityTypes())
        {
            var tabela = tipo.GetTableName();
            if (tabela is null) continue;

            foreach (var propriedade in tipo.GetProperties())
            {
                var coluna = propriedade.GetColumnName();

                var sql = coluna switch
                {
                    "Entidade" or "EntidadeRaiz" =>
                        $"[{coluna}] COLLATE {ColacaoBinaria} IN ({lista})",

                    // Identificador do padrão da seção 3: começa por maiúscula e só tem letra e
                    // dígito ASCII. Barra 'nome_cliente', 'Nome do cliente' e ' Nome'.
                    "Campo" =>
                        $"[{coluna}] COLLATE {ColacaoBinaria} LIKE '[A-Z]%' " +
                        $"AND [{coluna}] COLLATE {ColacaoBinaria} NOT LIKE '%[^A-Za-z0-9]%'",

                    _ => null
                };

                if (sql is null) continue;

                if (propriedade.IsNullable) sql = $"[{coluna}] IS NULL OR ({sql})";

                tipo.AddCheckConstraint($"CK_{tabela}_{coluna}", sql);
            }
        }
    }

    /// <summary>
    /// Remove do modelo o que só o SQL Server sabe fazer, para os testes de COMPORTAMENTO que
    /// rodam sobre SQLite em memória.
    ///
    /// POR QUE ISTO EXISTE, e por que não é afrouxar regra nenhuma: o modelo é de SQL Server
    /// por decisão (documento 20) e usa recursos que só ele tem — <c>rowversion</c>,
    /// <c>NEWID()</c> como valor padrão do servidor, colação por coluna, <c>ISJSON</c> e
    /// <c>LIKE</c> com classe de caractere na restrição de verificação. Nada disso é criável
    /// no SQLite, e sem esta redução o <c>EnsureCreated()</c> do teste falharia num detalhe
    /// de infraestrutura que não tem relação nenhuma com o que ele testa (o filtro global de
    /// segurança).
    ///
    /// O que se perde aqui NÃO fica sem verificação: o esquema, os tipos, as restrições e o
    /// particionamento são verificados contra o modelo de SQL Server (pasta <c>Banco/</c> dos
    /// testes de arquitetura) e contra o banco de verdade (<c>MigracaoNoContainerTestes</c>).
    /// A redução vale só para o provedor de teste, e nunca para o banco real.
    /// </summary>
    private static void ReduzirParaProvedorDeTeste(ModelBuilder modelo)
    {
        foreach (var tipo in modelo.Model.GetEntityTypes())
        {
            foreach (var restricao in tipo.GetCheckConstraints().ToList())
                tipo.RemoveCheckConstraint(restricao.ModelName);

            foreach (var propriedade in tipo.GetProperties())
            {
                propriedade.SetDefaultValueSql(null);
                propriedade.SetComputedColumnSql(null);
                propriedade.SetColumnType(null);
                propriedade.SetCollation(null);
            }

            foreach (var indice in tipo.GetIndexes())
                indice.SetAnnotation("SqlServer:Include", null);
        }
    }

    /// <summary>
    /// Concorrência otimista pelo <c>rowversion</c> do SQL Server.
    ///
    /// <c>rowversion</c> é um contador de 8 bytes que o BANCO incrementa a cada gravação da
    /// linha — inclusive na gravação que não passa pela aplicação. O EF Core inclui o valor
    /// lido na cláusula <c>WHERE</c> do <c>UPDATE</c>; se ninguém tiver alterado a linha, uma
    /// linha é afetada, e se alguém tiver alterado, nenhuma é — e o EF lança
    /// <c>DbUpdateConcurrencyException</c>.
    ///
    /// É a garantia do documento 14, seção 5.1: se duas pessoas abrirem o mesmo registro e as
    /// duas salvarem, a segunda recebe erro em vez de sobrescrever em silêncio o trabalho da
    /// primeira.
    ///
    /// Fora do SQL Server (o SQLite dos testes de comportamento) a propriedade
    /// <c>Versao</c> sai do modelo: é um <c>byte[]</c> que só o <c>rowversion</c> sabe
    /// preencher, e o SQLite não tem equivalente.
    /// </summary>
    private static void ConfigurarConcorrenciaOtimista(ModelBuilder modelo, bool ehSqlServer)
    {
        foreach (var tipo in modelo.Model.GetEntityTypes()
                     .Where(t => typeof(EntidadeBase).IsAssignableFrom(t.ClrType))
                     .Select(t => t.ClrType)
                     .ToList())
        {
            if (ehSqlServer)
                modelo.Entity(tipo).Property(nameof(EntidadeBase.Versao)).IsRowVersion();
            else
                modelo.Entity(tipo).Ignore(nameof(EntidadeBase.Versao));
        }
    }

    /// <summary>
    /// Salva as alterações e o carimbo de auditoria.
    ///
    /// [V] A integração do Vórtice não tem transação — o <c>BEGIN TRANSACTION</c> está
    /// literalmente comentado nas procedures. O resultado medido: 783.242 títulos
    /// (R$ 5,18 bi) presos em staging desde maio de 2025. Aqui, ou grava tudo, ou não grava nada.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        CarimbarAuditoria();
        return base.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        CarimbarAuditoria();
        return base.SaveChanges();
    }

    private void CarimbarAuditoria()
    {
        // O EntidadeBase já carimba CriadoEm/AlteradoEm nos métodos de domínio.
        // Aqui garantimos que a autoria nunca fica em branco, mesmo em caminho
        // que tenha esquecido de passar o usuário.
        foreach (var entrada in ChangeTracker.Entries<EntidadeBase>())
        {
            if (entrada.State is EntityState.Added && entrada.Entity.CriadoPorId == 0)
                entrada.Property(nameof(EntidadeBase.CriadoPorId)).CurrentValue = _usuarioId;
        }
    }
}
