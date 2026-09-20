using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;

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
                "impossíveis. O que limita a exposição do usuário é a camada 5, de campo sensível."

            // A SEGUNDA EXCEÇÃO ERA `CompartilhamentoDeRegistro`, a camada 4 do documento 05 —
            // aditiva e desenhada para ATRAVESSAR a fronteira. A tabela saiu na fase 1 da
            // reestruturação (documento 41) por nunca ter tido uma linha; quando o
            // compartilhamento pontual voltar, a exceção volta com ele, escrita aqui.
        };

    /// <summary>
    /// As entidades que têm filtro PRÓPRIO, mais restritivo que a fronteira de empresa, e por
    /// isso não recebem o filtro genérico: o delas já contém a fronteira, somada à profundidade
    /// da permissão (documento 05, seção 5).
    ///
    /// <para>Está vazia desde a fase 1 da reestruturação (documento 41): a única entidade com
    /// filtro próprio era <c>Lead</c>, e a tabela saiu. O mecanismo continua — quem voltar a
    /// precisar escreve o filtro em <c>OnModelCreating</c> e nomeia a entidade aqui.</para>
    /// </summary>
    private static readonly IReadOnlySet<string> FiltroProprio =
        new HashSet<string>(StringComparer.Ordinal);

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

    // ---------------------------------------------------------------------
    // DE ONDE VÊM AS GRAVAÇÕES DESTE CONTEXTO (documento 41, fase 2).
    //
    // Nascem do contexto de acesso — pessoa na API, sistema na carga — e podem ser DECLARADAS por
    // quem grava, com DeclararOrigemDasGravacoes. A carga precisa disso: o processo é um só, mas a
    // leitura do IBGE, a das planilhas e a do ART são origens diferentes, e cada uma só descobre o
    // identificador do seu sistema depois de abrir o contexto.
    // ---------------------------------------------------------------------
    private readonly Guid _correlacaoId;
    private OrigemDaOperacao _origem;
    private int? _sistemaId;

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
        _origem = acesso.Origem;
        _sistemaId = acesso.SistemaId;
        _correlacaoId = acesso.CorrelacaoId;
    }

    /// <summary>
    /// Declara de onde vêm as gravações feitas por este contexto a partir de agora.
    ///
    /// <para>Serve à carga e à integração: o contexto de acesso delas é de sistema, mas a trilha
    /// precisa dizer QUAL leitura mudou o dado — integração do IBGE, importação das planilhas,
    /// integração do ART. Uma pessoa na API não chama isto: a origem dela já é
    /// <see cref="OrigemDaOperacao.Usuario"/>.</para>
    /// </summary>
    /// <param name="origem">A origem das gravações.</param>
    /// <param name="sistemaId">O sistema externo, quando há um.</param>
    public void DeclararOrigemDasGravacoes(OrigemDaOperacao origem, int? sistemaId)
    {
        _origem = origem;
        _sistemaId = sistemaId;
    }

    // ---- organizacao ----

    /// <summary>Filiais da Tracbel.</summary>
    public DbSet<Empresa> Empresas => Set<Empresa>();

    /// <summary>Linhas de negócio.</summary>
    public DbSet<LinhaDeNegocio> LinhasDeNegocio => Set<LinhaDeNegocio>();

    /// <summary>Carteiras.</summary>
    public DbSet<Carteira> Carteiras => Set<Carteira>();

    /// <summary>Municípios — o catálogo nacional que fecha o campo de município.</summary>
    public DbSet<Municipio> Municipios => Set<Municipio>();

    /// <summary>Os municípios que cada carteira atende.</summary>
    public DbSet<CarteiraMunicipio> CarteiraMunicipios => Set<CarteiraMunicipio>();

    /// <summary>Os municípios da área de atuação, e se cada um pertence à ADR (documento 32).</summary>
    public DbSet<MunicipioDaAreaDeAtuacao> MunicipiosDaAreaDeAtuacao => Set<MunicipioDaAreaDeAtuacao>();

    /// <summary>Quem cada planilha do comercial diz que responde por cada município.</summary>
    public DbSet<ResponsavelPeloMunicipio> ResponsaveisPelosMunicipios => Set<ResponsavelPeloMunicipio>();

    /// <summary>
    /// As quatro medidas da Produção Agrícola Municipal do IBGE, por produto e município: área
    /// plantada, área colhida, quantidade produzida e valor da produção.
    /// </summary>
    public DbSet<ProducaoAgricolaNoMunicipio> ProducoesAgricolasNosMunicipios => Set<ProducaoAgricolaNoMunicipio>();

    /// <summary>As mesmas quatro medidas no total da UF — que não é a soma dos municípios.</summary>
    public DbSet<ProducaoAgricolaNoEstado> ProducoesAgricolasNosEstados => Set<ProducaoAgricolaNoEstado>();

    /// <summary>As regras de potencial por área — hoje, um exemplo a confirmar.</summary>
    public DbSet<RegraDePotencial> RegrasDePotencial => Set<RegraDePotencial>();

    // ---- seguranca ----

    /// <summary>Usuários.</summary>
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    /// <summary>Conjuntos de permissão.</summary>
    public DbSet<ConjuntoPermissao> ConjuntosPermissao => Set<ConjuntoPermissao>();

    /// <summary>Concessões de conjunto a usuário.</summary>
    public DbSet<UsuarioConjuntoPermissao> ConcessoesPermissao => Set<UsuarioConjuntoPermissao>();

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

    /// <summary>Carteirização de clientes.</summary>
    public DbSet<ClienteCarteira> ClienteCarteiras => Set<ClienteCarteira>();

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

    /// <summary>Tarefas.</summary>
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    /// <summary>Interações.</summary>
    public DbSet<Interacao> Interacoes => Set<Interacao>();

    /// <summary>Motivos de perda.</summary>
    public DbSet<MotivoDePerda> MotivosDePerda => Set<MotivoDePerda>();

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

    /// <summary>
    /// O faturamento que não achou cliente no CRM — o denominador que faltava.
    ///
    /// <para>Sem esta, a tela mostrava R$ 620 milhões como se fosse tudo o que a empresa vende, e
    /// os R$ 213 milhões que não casaram cadastro sumiam num contador da carga.</para>
    /// </summary>
    public DbSet<FaturamentoSemCliente> FaturamentoSemClientes => Set<FaturamentoSemCliente>();

    // ---- frota ----

    /// <summary>Marcas de máquina.</summary>
    public DbSet<Marca> Marcas => Set<Marca>();

    /// <summary>Famílias de máquina.</summary>
    public DbSet<Familia> Familias => Set<Familia>();

    /// <summary>Modelos de máquina.</summary>
    public DbSet<Modelo> Modelos => Set<Modelo>();

    /// <summary>Máquinas dos clientes.</summary>
    public DbSet<Equipamento> Equipamentos => Set<Equipamento>();

    /// <summary>A classificação de produto do CRM — trator pequeno, colhedora de cana… (documento 35, seção 10).</summary>
    public DbSet<LinhaDeProduto> LinhasDeProduto => Set<LinhaDeProduto>();

    /// <summary>As vendas de máquina lidas da origem, sem valor financeiro.</summary>
    public DbSet<VendaDeMaquina> VendasDeMaquina => Set<VendaDeMaquina>();

    /// <summary>A ligação entre cliente e máquina, com natureza, origem e data.</summary>
    public DbSet<VinculoDeClienteComEquipamento> VinculosComEquipamento => Set<VinculoDeClienteComEquipamento>();

    // ---- documento ----

    // ---- auditoria ----

    /// <summary>Alterações de campo.</summary>
    public DbSet<AlteracaoDeCampo> AlteracoesDeCampo => Set<AlteracaoDeCampo>();

    // ---- integracao ----

    /// <summary>Sistemas externos.</summary>
    public DbSet<Sistema> Sistemas => Set<Sistema>();

    /// <summary>De-para de identificadores.</summary>
    public DbSet<ChaveExterna> ChavesExternas => Set<ChaveExterna>();

    /// <summary>Pontos de sincronismo por fluxo.</summary>
    public DbSet<PontoDeSincronismo> PontosDeSincronismo => Set<PontoDeSincronismo>();

    /// <summary>Fila de descarte.</summary>
    public DbSet<MensagemDescartada> MensagensDescartadas => Set<MensagemDescartada>();

    /// <summary>O de-para explícito de linha, produto e unidade da origem para o catálogo do CRM.</summary>
    public DbSet<CorrespondenciaDaOrigem> CorrespondenciasDaOrigem => Set<CorrespondenciaDaOrigem>();

    /// <summary>A trilha de cada registro lido da origem.</summary>
    public DbSet<RegistroDeOrigem> RegistrosDeOrigem => Set<RegistroDeOrigem>();

    /// <summary>A fila de compradores ausentes do CRM — nenhum é criado automaticamente.</summary>
    public DbSet<CompradorPendente> CompradoresPendentes => Set<CompradorPendente>();

    /// <summary>As divergências entre fontes, registradas para revisão.</summary>
    public DbSet<DivergenciaDeIntegracao> DivergenciasDeIntegracao => Set<DivergenciaDeIntegracao>();

    /// <summary>Cada execução do serviço de sincronização, com resultado e contagens.</summary>
    public DbSet<ExecucaoDeSincronizacao> ExecucoesDeSincronizacao => Set<ExecucaoDeSincronizacao>();

    // ---- metadado ----

    /// <summary>Catálogos.</summary>
    public DbSet<Catalogo> Catalogos => Set<Catalogo>();

    /// <summary>Itens de catálogo.</summary>
    public DbSet<CatalogoItem> CatalogoItens => Set<CatalogoItem>();

    // ---- relatorio ----

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

        // NENHUMA ENTIDADE TEM FILTRO PRÓPRIO NESTA FASE. O único era o de `Lead`, removido na
        // fase 1 da reestruturação (documento 41) junto com a tabela. O mecanismo continua de pé:
        // quem voltar a precisar de um filtro mais restritivo que a fronteira de empresa escreve
        // aqui e nomeia a entidade em `FiltroProprio`.

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
    /// Salva as alterações, o carimbo de autoria e a trilha de auditoria — na mesma transação.
    ///
    /// [V] A integração do Vórtice não tem transação — o <c>BEGIN TRANSACTION</c> está
    /// literalmente comentado nas procedures. O resultado medido: 783.242 títulos
    /// (R$ 5,18 bi) presos em staging desde maio de 2025. Aqui, ou grava tudo, ou não grava nada.
    ///
    /// <para><b>A TRILHA (documento 41, fase 2).</b> Sem campo auditado na gravação, nada muda: é o
    /// <c>SaveChanges</c> de sempre, sem transação a mais. Com campo auditado, há dois caminhos:</para>
    /// <list type="bullet">
    ///   <item>quem chamou já abriu transação (a carga abre uma por bloco): salva, grava a trilha
    ///   dentro dela e deixa o commit para quem abriu;</item>
    ///   <item>ninguém abriu (a API): abre uma, dentro da estratégia de execução — que na API tem
    ///   retentativa em falha transitória —, salva sem aceitar as mudanças, grava a trilha, faz o
    ///   commit e só então aceita. Se a transação cair no meio, a retentativa repete o bloco inteiro
    ///   com o rastreador intacto. É o padrão que o EF Core documenta para transação com
    ///   retentativa.</item>
    /// </list>
    /// </summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        CarimbarAuditoria();

        var pendencias = TrilhaDeAuditoria.Capturar(ChangeTracker, _origem);
        if (pendencias.Count == 0)
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);

        int gravados;
        if (Database.CurrentTransaction is not null)
        {
            gravados = await base.SaveChangesAsync(acceptAllChangesOnSuccess: false, ct);
            await TrilhaDeAuditoria.GravarAsync(this, pendencias, TrilhaAtual(), ct);
        }
        else
        {
            gravados = await Database.CreateExecutionStrategy().ExecuteAsync(async token =>
            {
                await using var transacao = await Database.BeginTransactionAsync(token);
                var n = await base.SaveChangesAsync(acceptAllChangesOnSuccess: false, token);
                await TrilhaDeAuditoria.GravarAsync(this, pendencias, TrilhaAtual(), token);
                await transacao.CommitAsync(token);
                return n;
            }, ct);
        }

        if (acceptAllChangesOnSuccess) ChangeTracker.AcceptAllChanges();
        return gravados;
    }

    /// <inheritdoc cref="SaveChangesAsync(bool, CancellationToken)" />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        CarimbarAuditoria();

        var pendencias = TrilhaDeAuditoria.Capturar(ChangeTracker, _origem);
        if (pendencias.Count == 0)
            return base.SaveChanges(acceptAllChangesOnSuccess);

        int gravados;
        if (Database.CurrentTransaction is not null)
        {
            gravados = base.SaveChanges(acceptAllChangesOnSuccess: false);
            TrilhaDeAuditoria.Gravar(this, pendencias, TrilhaAtual());
        }
        else
        {
            gravados = Database.CreateExecutionStrategy().Execute(() =>
            {
                using var transacao = Database.BeginTransaction();
                var n = base.SaveChanges(acceptAllChangesOnSuccess: false);
                TrilhaDeAuditoria.Gravar(this, pendencias, TrilhaAtual());
                transacao.Commit();
                return n;
            });
        }

        if (acceptAllChangesOnSuccess) ChangeTracker.AcceptAllChanges();
        return gravados;
    }

    private TrilhaDeAuditoria.TrilhaDoContexto TrilhaAtual() =>
        new(_origem, _sistemaId, _correlacaoId, _usuarioId, _empresaId);

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
