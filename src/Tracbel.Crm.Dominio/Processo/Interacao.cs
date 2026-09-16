using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Processo;

/// <summary>Quem procurou quem.</summary>
public enum NaturezaDaInteracao
{
    /// <summary>Nós procuramos o cliente.</summary>
    Ativa = 0,

    /// <summary>O cliente nos procurou.</summary>
    Receptiva = 1,

    /// <summary>Registro gerado pelo próprio sistema.</summary>
    Sistema = 2
}

/// <summary>
/// O contato que aconteceu — fato imutável.
///
/// É somente-acrescentar por desenho: NÃO tem data de alteração nem exclusão lógica.
/// Corrigir um lançamento é registrar uma interação de estorno; a permissão de alteração e
/// remoção é negada no banco para o papel da aplicação.
///
/// Herda 2,4 milhões de linhas do Vórtice e substitui cinco tabelas. O cliente é coluna
/// DIRETA, não só via processo: interação sem processo existe, e a Visão 360 e a Cobertura
/// ordenam por ela. O índice de cliente mais data decrescente é o mais importante do modelo.
/// </summary>
public sealed class Interacao
{
    private Interacao() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O identificador que a API expõe.</summary>
    public Guid ChavePublica { get; private set; } = Guid.NewGuid();

    /// <summary>Filial dona do registro.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Que tipo de contato foi este. Dá a categoria: visita, ligação, WhatsApp.</summary>
    public int TipoTarefaId { get; private set; }

    /// <summary>O processo, quando o contato pertence a um.</summary>
    public long? ProcessoId { get; private set; }

    /// <summary>O cliente. Coluna direta, sempre que há cliente.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>O contato com quem se falou.</summary>
    public long? ContatoId { get; private set; }

    /// <summary>A tarefa que este contato concluiu.</summary>
    public long? TarefaId { get; private set; }

    /// <summary>Assunto.</summary>
    public string Assunto { get; private set; } = default!;

    /// <summary>O relato do que aconteceu.</summary>
    public string? Detalhe { get; private set; }

    /// <summary>Desfecho registrado.</summary>
    public int? ResultadoId { get; private set; }

    /// <summary>
    /// Complemento do desfecho. Guarda o que couber no tamanho declarado — [V] no Vórtice a
    /// coluna aceita 150 caracteres e a aplicação trunca em 20, em silêncio.
    /// </summary>
    public string? ResultadoComplemento { get; private set; }

    /// <summary>Quem procurou quem.</summary>
    public NaturezaDaInteracao Natureza { get; private set; } = NaturezaDaInteracao.Ativa;

    /// <summary>Quando o contato aconteceu (UTC).</summary>
    public DataHoraUtc OcorridaEm { get; private set; }

    /// <summary>Duração, em minutos.</summary>
    public int? DuracaoMinutos { get; private set; }

    /// <summary>Latitude do atendimento em campo.</summary>
    public decimal? Latitude { get; private set; }

    /// <summary>Longitude do atendimento em campo.</summary>
    public decimal? Longitude { get; private set; }

    /// <summary>Quem registrou.</summary>
    public long RegistradoPorId { get; private set; }

    /// <summary>Quando a linha foi gravada (UTC).</summary>
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>A coordenada validada do atendimento, quando as duas colunas estão preenchidas.</summary>
    public Coordenada? Localizacao =>
        Latitude is { } lat && Longitude is { } lon && Coordenada.TentarCriar((double)lat, (double)lon, out var c)
            ? c
            : null;

    /// <summary>Registra uma interação que já aconteceu.</summary>
    /// <param name="empresaId">Filial dona do registro.</param>
    /// <param name="tipoTarefaId">Que tipo de contato foi este.</param>
    /// <param name="assunto">Assunto.</param>
    /// <param name="ocorridaEm">Quando o contato aconteceu.</param>
    /// <param name="natureza">Quem procurou quem.</param>
    /// <param name="registradoPorId">Quem registrou.</param>
    /// <param name="clienteId">O cliente.</param>
    /// <param name="processoId">O processo, quando o contato pertence a um.</param>
    /// <param name="contatoId">O contato com quem se falou.</param>
    /// <param name="tarefaId">A tarefa que este contato concluiu.</param>
    /// <param name="resultadoId">Desfecho registrado.</param>
    /// <param name="resultadoComplemento">Complemento do desfecho.</param>
    /// <param name="detalhe">O relato do que aconteceu.</param>
    /// <param name="duracaoMinutos">Duração, em minutos.</param>
    /// <param name="latitude">Latitude do atendimento em campo.</param>
    /// <param name="longitude">Longitude do atendimento em campo.</param>
    public static Interacao Registrar(
        int empresaId,
        int tipoTarefaId,
        string assunto,
        DataHoraUtc ocorridaEm,
        NaturezaDaInteracao natureza,
        long registradoPorId,
        long? clienteId = null,
        long? processoId = null,
        long? contatoId = null,
        long? tarefaId = null,
        int? resultadoId = null,
        string? resultadoComplemento = null,
        string? detalhe = null,
        int? duracaoMinutos = null,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        if (clienteId is null && processoId is null && contatoId is null)
            throw new RegraDeNegocioViolada(
                "Interação precisa se ligar a alguma coisa: cliente, processo ou contato. Nunca solta.");

        return new Interacao
        {
            EmpresaId = empresaId,
            TipoTarefaId = tipoTarefaId,
            Assunto = assunto,
            Detalhe = detalhe,
            OcorridaEm = ocorridaEm,
            Natureza = natureza,
            RegistradoPorId = registradoPorId,
            ClienteId = clienteId,
            ProcessoId = processoId,
            ContatoId = contatoId,
            TarefaId = tarefaId,
            ResultadoId = resultadoId,
            ResultadoComplemento = resultadoComplemento,
            DuracaoMinutos = duracaoMinutos,
            Latitude = latitude,
            Longitude = longitude,

            // A LINHA NASCE COM A DATA DO FATO, não com a data da carga. Interação é registro
            // imutável: se a data de gravação virasse "hoje" para 124 mil linhas migradas, a
            // única coisa que a coluna diria é quando a migração rodou.
            CriadoEm = ocorridaEm.Valor
        };
    }
}

// O QUE SAIU DAQUI NA FASE 1 (documento 41): `InteracaoParticipante` e o enum `PapelNaInteracao`
// — quem mais esteve na reunião. Uma visita tem três pessoas do cliente e o Vórtice guarda uma;
// a tabela existia para resolver isso, mas nasceu com o modelo inicial, nunca recebeu uma linha e
// não havia tela nem carga que a preenchesse. O participante que o sistema de fato registra hoje é
// o `ContatoId` da própria interação, um só.
//
// A ideia não foi descartada: quando a tela de visita passar a perguntar quem participou, a tabela
// volta pelo desenho do documento 40, com o código que a alimenta escrito junto.
