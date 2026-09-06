using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Integracao;

/// <summary>
/// O sistema externo com que o CRM conversa: Protheus, John Deere, Vórtice.
///
/// Sem ele, a matriz de propriedade do dado não tem chave, e a tela de integrações não tem
/// fonte — hoje é um arquivo de configuração com sete registros.
/// </summary>
public sealed class Sistema
{
    private Sistema() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável. Ex.: PROTHEUS, JOHNDEERE, VORTICE, RDSTATION.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Como se fala com ele: linked server, REST, arquivo, fila.</summary>
    public string MeioDeAcesso { get; private set; } = default!;

    /// <summary>Quem responde pelo sistema do lado da Tracbel.</summary>
    public string? ResponsavelTecnico { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Cadastra um sistema externo.</summary>
    public static Sistema Criar(string codigo, string nome, string meioDeAcesso) => new()
    {
        Codigo = codigo,
        Nome = nome,
        MeioDeAcesso = meioDeAcesso
    };
}

/// <summary>
/// O de-para de identificador entre o CRM e o sistema externo.
///
/// [SF] É o identificador externo que torna a gravação idempotente. Cinco cópias do de-para
/// do Vórtice viram uma. A restrição de unicidade por sistema é o que impede a duplicação
/// que o legado sofre.
/// </summary>
public sealed class ChaveExterna
{
    private ChaveExterna() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O sistema externo.</summary>
    public int SistemaId { get; private set; }

    /// <summary>Nome da entidade do CRM.</summary>
    public string Entidade { get; private set; } = default!;

    /// <summary>Identificador interno do registro do CRM.</summary>
    public long RegistroId { get; private set; }

    /// <summary>O identificador que o sistema externo usa.</summary>
    public string ChaveOrigem { get; private set; } = default!;

    /// <summary>Quando os dois lados foram conciliados pela última vez (UTC).</summary>
    public DateTime SincronizadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Cria um de-para.</summary>
    public static ChaveExterna Criar(int sistemaId, string entidade, long registroId, string chaveOrigem) => new()
    {
        SistemaId = sistemaId,
        Entidade = entidade,
        RegistroId = registroId,
        ChaveOrigem = chaveOrigem
    };

    /// <summary>
    /// Carimba a conciliação desta rodada.
    ///
    /// É O QUE TORNA A RECARGA IDEMPOTENTE E LEGÍVEL: sem este carimbo, rodar de novo atualiza
    /// o registro e não deixa rastro de quando os dois lados foram conferidos pela última vez —
    /// e "quando foi a última vez que isto bateu?" é a pergunta que ninguém conseguiu responder
    /// nos 17 meses em que o faturamento do legado ficou parado.
    /// </summary>
    /// <param name="quandoUtc">O instante da conciliação.</param>
    public void MarcarSincronismo(DateTime quandoUtc) => SincronizadoEm = quandoUtc;
}

/// <summary>
/// Até onde cada fluxo de integração já leu.
///
/// É a tabela que teria alarmado em 11/04/2025, quando a integração de faturamento do
/// Vórtice parou e ninguém soube por 17 meses. Guarda o contador de lidos, gravados e com
/// erro de cada rodada, e é sobre ela que o alarme de atraso é escrito.
/// </summary>
public sealed class PontoDeSincronismo
{
    private PontoDeSincronismo() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O sistema externo.</summary>
    public int SistemaId { get; private set; }

    /// <summary>Nome do fluxo. Ex.: PROTHEUS.TITULO.</summary>
    public string Fluxo { get; private set; } = default!;

    /// <summary>Até onde leu: carimbo de tempo ou identificador, conforme o fluxo.</summary>
    public string UltimoValor { get; private set; } = default!;

    /// <summary>Quando a última rodada terminou (UTC).</summary>
    public DateTime ProcessadoEm { get; private set; }

    /// <summary>Quantas linhas foram lidas na última rodada.</summary>
    public int RegistrosLidos { get; private set; }

    /// <summary>Quantas linhas foram gravadas na última rodada.</summary>
    public int RegistrosGravados { get; private set; }

    /// <summary>Quantas linhas falharam na última rodada.</summary>
    public int RegistrosErro { get; private set; }

    /// <summary>
    /// Em quantos minutos sem rodada o alarme dispara. É o que transforma silêncio em
    /// incidente.
    /// </summary>
    public int MinutosParaAlarme { get; private set; } = 60;

    /// <summary>Cria o ponto de sincronismo de um fluxo.</summary>
    public static PontoDeSincronismo Criar(int sistemaId, string fluxo, string ultimoValor) => new()
    {
        SistemaId = sistemaId,
        Fluxo = fluxo,
        UltimoValor = ultimoValor,
        ProcessadoEm = DateTime.UtcNow
    };

    /// <summary>
    /// Fecha uma rodada: até onde leu, quantas linhas leu, gravou e recusou.
    ///
    /// OS TRÊS CONTADORES ANDAM JUNTOS de propósito. Gravar só o que entrou contaria metade da
    /// história — é exatamente a metade que faltava no legado, onde 22.512 falhas de fila
    /// ficaram invisíveis porque ninguém contava o que NÃO passou. A restrição
    /// <c>CK_PontoDeSincronismo_Contadores</c> recusa contador negativo no banco; aqui a recusa
    /// vem antes, com a frase que quem chamou entende.
    /// </summary>
    /// <param name="ultimoValor">Até onde a rodada leu: carimbo de tempo ou identificador.</param>
    /// <param name="lidos">Linhas lidas na origem.</param>
    /// <param name="gravados">Linhas gravadas no CRM.</param>
    /// <param name="recusados">Linhas recusadas pelo saneamento.</param>
    /// <param name="quandoUtc">Quando a rodada terminou.</param>
    public void RegistrarRodada(string ultimoValor, int lidos, int gravados, int recusados, DateTime quandoUtc)
    {
        if (string.IsNullOrWhiteSpace(ultimoValor))
            throw new RegraDeNegocioViolada(
                "Um ponto de sincronismo sem 'até onde li' não serve para continuar de onde parou.");

        if (lidos < 0 || gravados < 0 || recusados < 0)
            throw new RegraDeNegocioViolada("Contador de rodada não é negativo.");

        UltimoValor = ultimoValor;
        RegistrosLidos = lidos;
        RegistrosGravados = gravados;
        RegistrosErro = recusados;
        ProcessadoEm = quandoUtc;
    }
}

/// <summary>Em que ponto do processamento a linha recebida está.</summary>
public enum SituacaoDaRecepcao
{
    /// <summary>Chegou e ainda não foi processada.</summary>
    Recebida = 0,

    /// <summary>Foi processada com sucesso.</summary>
    Processada = 1,

    /// <summary>Falhou no processamento.</summary>
    Falhou = 2,

    /// <summary>Foi ignorada por regra, sem erro.</summary>
    Ignorada = 3
}

/// <summary>
/// A área de pouso EFÊMERA da integração.
///
/// [V] O Vórtice tem 77 tabelas de staging permanentes, com 19,4 milhões de linhas que nunca
/// saem de lá — entre elas 783.242 títulos, R$ 5,18 bilhões, presos desde maio de 2025.
/// Aqui existe uma tabela só, particionada por data e com data de expurgo em cada linha: o
/// que entrou e foi processado sai, por desenho.
/// </summary>
public sealed class Recepcao
{
    private Recepcao() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O sistema que enviou.</summary>
    public int SistemaId { get; private set; }

    /// <summary>Nome da entidade recebida.</summary>
    public string Entidade { get; private set; } = default!;

    /// <summary>O identificador que o sistema externo usa.</summary>
    public string ChaveOrigem { get; private set; } = default!;

    /// <summary>O conteúdo como chegou, íntegro. Guardado como JSON binário.</summary>
    public string Conteudo { get; private set; } = default!;

    /// <summary>Em que ponto do processamento está.</summary>
    public SituacaoDaRecepcao Situacao { get; private set; } = SituacaoDaRecepcao.Recebida;

    /// <summary>O erro, quando falhou.</summary>
    public string? Erro { get; private set; }

    /// <summary>Quando chegou (UTC). É a coluna de particionamento.</summary>
    public DateTime RecebidaEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quando foi processada (UTC).</summary>
    public DateTime? ProcessadaEm { get; private set; }

    /// <summary>A partir de quando a linha pode ser apagada. Sem isto, staging vira acervo.</summary>
    public DateTime ExpurgarApos { get; private set; }

    /// <summary>Recebe uma linha do sistema externo.</summary>
    public static Recepcao Receber(
        int sistemaId, string entidade, string chaveOrigem, string conteudo, int diasParaExpurgo = 30) => new()
    {
        SistemaId = sistemaId,
        Entidade = entidade,
        ChaveOrigem = chaveOrigem,
        Conteudo = conteudo,
        ExpurgarApos = DateTime.UtcNow.AddDays(diasParaExpurgo)
    };
}

/// <summary>Em que ponto da entrega a mensagem está.</summary>
public enum SituacaoDaMensagem
{
    /// <summary>Ainda não entregue.</summary>
    Pendente = 0,

    /// <summary>Entregue com sucesso.</summary>
    Entregue = 1,

    /// <summary>Falhou e será tentada de novo.</summary>
    Falhou = 2,

    /// <summary>Falhou o número máximo de vezes e foi para a fila de descarte.</summary>
    DescartadaAposLimite = 3
}

/// <summary>
/// A fila de saída, gravada na MESMA transação do dado.
///
/// É o que garante que nunca existe dado sem evento nem evento sem dado. [V] A integração do
/// Vórtice não tem transação: o comando de início de transação está literalmente comentado
/// nas procedures.
/// </summary>
public sealed class MensagemDeSaida
{
    private MensagemDeSaida() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>Sistema de destino.</summary>
    public int SistemaId { get; private set; }

    /// <summary>Tipo da mensagem.</summary>
    public string Tipo { get; private set; } = default!;

    /// <summary>O conteúdo, como JSON binário.</summary>
    public string Conteudo { get; private set; } = default!;

    /// <summary>Correlaciona com a operação que gerou a mensagem.</summary>
    public Guid CorrelacaoId { get; private set; }

    /// <summary>Em que ponto da entrega está.</summary>
    public SituacaoDaMensagem Situacao { get; private set; } = SituacaoDaMensagem.Pendente;

    /// <summary>Quantas vezes já se tentou entregar.</summary>
    public short Tentativas { get; private set; }

    /// <summary>Quando tentar de novo (UTC).</summary>
    public DateTime? ProximaTentativaEm { get; private set; }

    /// <summary>O erro da última tentativa.</summary>
    public string? UltimoErro { get; private set; }

    /// <summary>Quando a mensagem foi enfileirada (UTC).</summary>
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quando foi entregue (UTC).</summary>
    public DateTime? EntregueEm { get; private set; }

    /// <summary>Enfileira uma mensagem de saída.</summary>
    public static MensagemDeSaida Enfileirar(int sistemaId, string tipo, string conteudo, Guid correlacaoId) => new()
    {
        SistemaId = sistemaId,
        Tipo = tipo,
        Conteudo = conteudo,
        CorrelacaoId = correlacaoId,
        ProximaTentativaEm = DateTime.UtcNow
    };
}

/// <summary>
/// O que foi rejeitado, e por quê — a fila de descarte.
///
/// [V] Sem ela, mensagem que falha some: a fila de e-mail do Vórtice tem 22.512 falhas
/// silenciosas e nenhuma nova tentativa. Aqui nada morre em silêncio, e quem tratou fica
/// gravado — uma fila de descarte que ninguém olha é igual a não ter fila de descarte.
/// </summary>
public sealed class MensagemDescartada
{
    private MensagemDescartada() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A mensagem de saída que falhou, quando a origem é a fila de saída.</summary>
    public long? MensagemDeSaidaId { get; private set; }

    /// <summary>Nome do fluxo que produziu a mensagem.</summary>
    public string Fluxo { get; private set; } = default!;

    /// <summary>O conteúdo, como JSON binário.</summary>
    public string Conteudo { get; private set; } = default!;

    /// <summary>O erro que causou o descarte.</summary>
    public string Erro { get; private set; } = default!;

    /// <summary>Quantas tentativas foram feitas antes de desistir.</summary>
    public short Tentativas { get; private set; }

    /// <summary>Quando foi descartada (UTC).</summary>
    public DateTime DescartadaEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quando alguém tratou (UTC).</summary>
    public DateTime? TratadaEm { get; private set; }

    /// <summary>Quem tratou.</summary>
    public long? TratadaPorId { get; private set; }

    /// <summary>O que foi feito.</summary>
    public string? Tratativa { get; private set; }

    /// <summary>Descarta uma mensagem que falhou definitivamente.</summary>
    public static MensagemDescartada Criar(string fluxo, string conteudo, string erro, short tentativas) => new()
    {
        Fluxo = fluxo,
        Conteudo = conteudo,
        Erro = erro,
        Tentativas = tentativas
    };
}
