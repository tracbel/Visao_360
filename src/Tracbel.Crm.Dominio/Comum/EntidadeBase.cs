namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Base de toda entidade do domínio.
///
/// Duas identidades, de propósito:
/// <list type="bullet">
///   <item><see cref="Id"/> — sequencial, interno, eficiente como índice clustered. Nunca sai daqui.</item>
///   <item><see cref="ChavePublica"/> — GUID, é o que aparece na URL e na API, para que ninguém
///   consiga enumerar registros nem inferir volume de negócio contando IDs.</item>
/// </list>
///
/// Todo estado tem <c>protected set</c>: ninguém fora da entidade muda o que ela guarda.
/// Isso não é burocracia — é o que impede que uma tela, um job ou uma integração deixem o
/// registro num estado que o negócio não admite. [V] foi assim que o Vórtice acumulou
/// 345.535 linhas órfãs e processos cancelados sem motivo registrado.
/// </summary>
public abstract class EntidadeBase
{
    private readonly List<IEventoDominio> _eventos = [];

    /// <summary>Identificador interno, sequencial. Chave primária no banco.</summary>
    public long Id { get; protected set; }

    /// <summary>Identificador exposto pela API e pela URL.</summary>
    public Guid ChavePublica { get; protected set; } = Guid.NewGuid();

    /// <summary>Quando o registro nasceu (UTC).</summary>
    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;

    /// <summary>Quem criou o registro.</summary>
    public long CriadoPorId { get; protected set; }

    /// <summary>Última alteração (UTC). Nulo enquanto o registro nunca foi alterado.</summary>
    public DateTime? AlteradoEm { get; protected set; }

    /// <summary>Quem fez a última alteração.</summary>
    public long? AlteradoPorId { get; protected set; }

    /// <summary>Exclusão lógica. Nulo significa registro ativo.</summary>
    public DateTime? ExcluidoEm { get; protected set; }

    /// <summary>
    /// Controle de concorrência otimista.
    ///
    /// É uma coluna <c>rowversion</c> do SQL Server, incrementada PELO BANCO a cada gravação
    /// da linha — inclusive na gravação que não passa pela aplicação. Se duas pessoas abrirem
    /// o mesmo registro e as duas salvarem, a segunda recebe
    /// <c>DbUpdateConcurrencyException</c> em vez de sobrescrever em silêncio o trabalho da
    /// primeira (documento 14, seção 5.1).
    ///
    /// O mapeamento é condicionado ao provedor em
    /// <c>CrmDbContext.ConfigurarConcorrenciaOtimista</c>: no SQL Server a propriedade vira
    /// o token de concorrência; no SQLite dos testes de comportamento ela sai do modelo,
    /// porque lá não existe rowversion. Daí o tipo ser anulável.
    /// </summary>
    public byte[]? Versao { get; protected set; }

    /// <summary>Verdadeiro quando o registro foi excluído logicamente.</summary>
    public bool EstaExcluido => ExcluidoEm is not null;

    /// <summary>
    /// Eventos de domínio acumulados nesta unidade de trabalho.
    ///
    /// Só são publicados DEPOIS do commit. Publicar antes é o erro que faz o sistema
    /// notificar sobre algo que a transação depois desfez.
    /// </summary>
    public IReadOnlyCollection<IEventoDominio> Eventos => _eventos.AsReadOnly();

    /// <summary>Acumula um evento para publicação após o commit.</summary>
    protected void RegistrarEvento(IEventoDominio evento) => _eventos.Add(evento);

    /// <summary>Descarta os eventos já publicados. Chamado pela unidade de trabalho.</summary>
    public void LimparEventos() => _eventos.Clear();

    /// <summary>Carimba autoria e data da alteração.</summary>
    protected void MarcarAlteracao(long usuarioId)
    {
        AlteradoEm = DateTime.UtcNow;
        AlteradoPorId = usuarioId;
    }

    /// <summary>
    /// A versão que quem vai alterar leu é a mesma que está aqui agora?
    ///
    /// POR QUE ISTO EXISTE ALÉM DO <c>rowversion</c>: o <c>rowversion</c> protege a janela entre
    /// a leitura e a gravação DENTRO de uma requisição. Numa API sem estado, a janela que
    /// interessa é outra e é muito maior — o usuário abre a tela às 9h, o colega salva às 9h05,
    /// e o primeiro salva às 9h10 por cima. Só o cliente sabe qual versão ele leu, então ele
    /// devolve essa versão no <c>PUT</c> e a comparação acontece aqui.
    ///
    /// As duas camadas somam: esta recusa a alteração cega do usuário; o <c>rowversion</c>
    /// recusa a corrida entre duas requisições simultâneas.
    ///
    /// QUANDO NÃO HÁ VERSÃO CONHECIDA a comparação é dispensada — de propósito, e é a única
    /// concessão: o provedor de teste (SQLite) não tem <c>rowversion</c>, e um cliente de
    /// integração que não guarda versão precisa continuar conseguindo gravar. O que ele perde é
    /// a proteção, e ele perde porque escolheu não mandar a versão.
    /// </summary>
    /// <param name="versaoConhecida">A versão que quem está alterando leu. Nula dispensa a conferência.</param>
    public bool VersaoConfere(byte[]? versaoConhecida) =>
        versaoConhecida is null
        || Versao is null
        || versaoConhecida.AsSpan().SequenceEqual(Versao);

    /// <summary>Exclui logicamente. O registro continua existindo e auditável.</summary>
    public void Excluir(long usuarioId)
    {
        if (EstaExcluido) return;
        ExcluidoEm = DateTime.UtcNow;
        MarcarAlteracao(usuarioId);
    }
}
