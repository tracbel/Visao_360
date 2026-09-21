using Tracbel.Crm.Dominio.Auditoria;

namespace Tracbel.Crm.Dominio.Seguranca;

/// <summary>
/// Quem está usando o sistema agora, e o que ele alcança.
///
/// É montado uma vez por requisição, a partir do token do Entra ID, e carregado por todo o
/// processamento. Toda decisão de acesso consulta este objeto — não o banco.
///
/// [V] No Vórtice, o escopo de visibilidade é resolvido em código a partir de QUATRO ROTAS
/// PARALELAS (IV_VENDEDOR.SeqUsuarioLider, IVS_Carteira.SeqUsrResp, IVS_Carteira.SeqUrSuperv,
/// IVS_DeptoEmpr.SeqUsrGerDepto), uma delas morta (SeqUrSuperv é NULL em 655 de 655 carteiras) —
/// e essas rotas só valem para o mobile. No desktop não há equivalente configurável.
/// Aqui existe UMA fonte de verdade, e ela é este objeto.
/// </summary>
public sealed class ContextoAcesso
{
    private readonly Dictionary<string, Profundidade> _profundidades;

    /// <summary>Monta o contexto. Chamado pela infraestrutura, uma vez por requisição.</summary>
    /// <param name="usuarioId">Quem é.</param>
    /// <param name="nomeExibicao">Nome curto, para mensagens.</param>
    /// <param name="empresaId">A empresa "de casa" — define o escopo padrão.</param>
    /// <param name="empresasVisiveis">Empresas que ele alcança, já expandidas pela hierarquia.</param>
    /// <param name="subordinadosIds">Todos os subordinados, em qualquer profundidade.</param>
    /// <param name="equipesIds">Equipes de que participa.</param>
    /// <param name="profundidades">Permissão → profundidade, já consolidada dos conjuntos.</param>
    /// <param name="ehServicoDeSistema">Job ou integração rodando sem usuário humano.</param>
    /// <param name="origem">
    /// De onde vêm as gravações feitas sob este contexto. Quando omitida: <see cref="OrigemDaOperacao.Sistema"/>
    /// para serviço de sistema e <see cref="OrigemDaOperacao.Usuario"/> para pessoa.
    /// </param>
    /// <param name="sistemaId">O sistema externo, quando a origem é integração ou importação.</param>
    /// <param name="correlacaoId">
    /// O que liga as linhas da trilha à requisição ou à execução que as causou. Quando omitido, nasce
    /// um novo — e como o contexto é montado uma vez por requisição, é um por requisição.
    /// </param>
    public ContextoAcesso(
        long usuarioId,
        string nomeExibicao,
        int empresaId,
        IReadOnlySet<int> empresasVisiveis,
        IReadOnlySet<long> subordinadosIds,
        IReadOnlySet<long> equipesIds,
        IReadOnlyDictionary<string, Profundidade> profundidades,
        bool ehServicoDeSistema = false,
        OrigemDaOperacao? origem = null,
        int? sistemaId = null,
        Guid? correlacaoId = null)
    {
        UsuarioId = usuarioId;
        NomeExibicao = nomeExibicao;
        EmpresaId = empresaId;
        EmpresasVisiveis = empresasVisiveis;
        SubordinadosIds = subordinadosIds;
        EquipesIds = equipesIds;
        EhServicoDeSistema = ehServicoDeSistema;
        Origem = origem ?? (ehServicoDeSistema ? OrigemDaOperacao.Sistema : OrigemDaOperacao.Usuario);
        SistemaId = sistemaId;
        CorrelacaoId = correlacaoId ?? Guid.NewGuid();
        _profundidades = new Dictionary<string, Profundidade>(profundidades, StringComparer.Ordinal);
    }

    /// <summary>
    /// De onde vêm as gravações feitas sob este contexto — o que vai para
    /// <c>auditoria.AlteracaoDeCampo.Origem</c>.
    /// </summary>
    public OrigemDaOperacao Origem { get; }

    /// <summary>O sistema externo das gravações, quando há um.</summary>
    public int? SistemaId { get; }

    /// <summary>Identifica a requisição ou a execução; vai para <c>auditoria.AlteracaoDeCampo.CorrelacaoId</c>.</summary>
    public Guid CorrelacaoId { get; }

    /// <summary>Quem está agindo.</summary>
    public long UsuarioId { get; }

    /// <summary>Nome curto, para log e mensagem.</summary>
    public string NomeExibicao { get; }

    /// <summary>Empresa de origem do usuário.</summary>
    public int EmpresaId { get; }

    /// <summary>Empresas que ele alcança.</summary>
    public IReadOnlySet<int> EmpresasVisiveis { get; }

    /// <summary>Subordinados, em qualquer profundidade da hierarquia.</summary>
    public IReadOnlySet<long> SubordinadosIds { get; }

    /// <summary>Equipes de que participa.</summary>
    public IReadOnlySet<long> EquipesIds { get; }

    /// <summary>
    /// Job ou integração. Enxerga tudo, e o que grava entra na trilha com a <see cref="Origem"/>
    /// declarada — nunca como se fosse uma pessoa.
    /// </summary>
    public bool EhServicoDeSistema { get; }

    /// <summary>
    /// A permissão que autoriza ABRIR a via de escape da fronteira de empresa.
    ///
    /// Não é uma permissão de leitura de nada: é a permissão de DIZER que se está ignorando a
    /// fronteira. Quem a tem em <see cref="Profundidade.Organizacao"/> pode chamar
    /// <c>CrmDbContext.AbrirAlcanceEntreEmpresas(motivo)</c> — e cada abertura é registrada
    /// com o motivo escrito por quem abriu (documento 05, seção 8; documento 14, seção 11.3).
    /// </summary>
    public const string PermissaoDeAlcanceEntreEmpresas = "Empresa.AlcanceEntreFiliais";

    /// <summary>
    /// Este contexto pode abrir o alcance entre filiais?
    ///
    /// Vale para o administrador que recebeu <see cref="PermissaoDeAlcanceEntreEmpresas"/> em
    /// profundidade <see cref="Profundidade.Organizacao"/> e para o serviço de sistema (job e
    /// integração), que já enxerga tudo por <see cref="EhServicoDeSistema"/>.
    ///
    /// PODER ABRIR NÃO É ESTAR ABERTO: enquanto ninguém chamar
    /// <c>AbrirAlcanceEntreEmpresas</c>, o administrador continua vendo só as filiais dele.
    /// A fronteira só cai quando alguém declara, por escrito, que está derrubando.
    /// </summary>
    public bool PodeAlcancarTodasAsEmpresas =>
        ProfundidadeDe(PermissaoDeAlcanceEntreEmpresas) >= Profundidade.Organizacao;

    /// <summary>
    /// Todas as permissões deste contexto e a profundidade de cada uma — o que a rota de escopo efetivo
    /// mostra ("o que eu posso fazer aqui?"). O serviço de sistema não tem lista: ele alcança tudo.
    /// </summary>
    public IReadOnlyDictionary<string, Profundidade> Profundidades => _profundidades;

    /// <summary>
    /// Até onde este usuário alcança numa permissão.
    ///
    /// A consolidação é ADITIVA: se ele tem a mesma permissão por dois conjuntos, com
    /// profundidades diferentes, vence a MAIOR. `[SF]` é o "most permissive wins" dos
    /// permission sets. Permissão que ele não tem devolve <see cref="Profundidade.Nenhum"/>.
    /// </summary>
    public Profundidade ProfundidadeDe(string codigoPermissao)
    {
        if (EhServicoDeSistema) return Profundidade.Organizacao;
        return _profundidades.GetValueOrDefault(codigoPermissao, Profundidade.Nenhum);
    }

    /// <summary>
    /// Camada 2: ele tem a permissão, em qualquer profundidade?
    /// É o TETO ABSOLUTO — sem isso, nem se avalia a linha.
    /// </summary>
    public bool Tem(string codigoPermissao) => ProfundidadeDe(codigoPermissao) > Profundidade.Nenhum;

    /// <summary>
    /// Camada 3: esta linha cabe na profundidade que ele tem nesta permissão?
    ///
    /// Esta é a decisão mais executada do sistema inteiro — roda em toda consulta e em toda
    /// gravação. Por isso resolve com colunas da própria linha, sem consultar o banco.
    /// </summary>
    /// <param name="codigoPermissao">Ex.: <c>Processo.Ler</c>.</param>
    /// <param name="proprietarioId">Dono do registro.</param>
    /// <param name="empresaIdDoRegistro">Empresa do registro.</param>
    public bool AlcancaRegistro(string codigoPermissao, long proprietarioId, int empresaIdDoRegistro)
    {
        var profundidade = ProfundidadeDe(codigoPermissao);

        return profundidade switch
        {
            Profundidade.Nenhum => false,
            Profundidade.Organizacao => true,

            // A empresa dele e as filhas já vêm expandidas em EmpresasVisiveis.
            Profundidade.EmpresaEAbaixo => EmpresasVisiveis.Contains(empresaIdDoRegistro),

            Profundidade.Empresa => empresaIdDoRegistro == EmpresaId,

            // Equipe = os próprios MAIS os de quem está abaixo. A closure table já
            // materializou a subárvore inteira, então não há recursão aqui.
            Profundidade.Equipe => proprietarioId == UsuarioId
                                || SubordinadosIds.Contains(proprietarioId),

            Profundidade.Proprios => proprietarioId == UsuarioId,

            _ => false
        };
    }
}
