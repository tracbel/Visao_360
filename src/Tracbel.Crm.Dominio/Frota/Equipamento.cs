using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Frota;

/// <summary>Em que estado a máquina está.</summary>
public enum SituacaoDoEquipamento
{
    /// <summary>Em estoque, ainda não vendida.</summary>
    Estoque = 0,

    /// <summary>Em operação no cliente.</summary>
    Ativo = 1,

    /// <summary>Vendida e transferida.</summary>
    Vendido = 2,

    /// <summary>Baixada, fora de operação.</summary>
    Baixado = 3
}

/// <summary>Quem afirma que esta máquina existe.</summary>
public enum OrigemDoEquipamento
{
    /// <summary>Veio do ERP: é ativo faturado pela Tracbel.</summary>
    Protheus = 0,

    /// <summary>Foi declarada pelo CEN no CRM — inclusive a máquina do concorrente.</summary>
    Crm = 1
}

/// <summary>
/// A máquina do cliente, identificada pelo chassi.
///
/// Substitui 23 tabelas do Vórtice. Guarda também a máquina do CONCORRENTE, que é a que mais
/// interessa para a tela de Cobertura: a coluna de origem é o que permite à tela dizer
/// quantos ativos vêm do ERP e quantos o CEN declarou.
/// </summary>
public sealed class Equipamento : EntidadeBase
{
    private Equipamento() { }

    /// <summary>Filial dona do registro.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O dono atual. Nulo é máquina em estoque.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>O modelo de catálogo.</summary>
    public int ModeloId { get; private set; }

    /// <summary>O chassi. É a identidade de verdade da máquina agrícola.</summary>
    public Chassi Chassi { get; private set; }

    /// <summary>Número de série, quando diferente do chassi.</summary>
    public string? NumeroSerie { get; private set; }

    /// <summary>Placa, quando a máquina é emplacada.</summary>
    public string? Placa { get; private set; }

    /// <summary>Ano de fabricação.</summary>
    public short? AnoFabricacao { get; private set; }

    /// <summary>Ano do modelo.</summary>
    public short? AnoModelo { get; private set; }

    /// <summary>Horímetro mais recente conhecido.</summary>
    public decimal? HorimetroAtual { get; private set; }

    /// <summary>Quando o horímetro foi atualizado (UTC).</summary>
    public DateTime? HorimetroAtualizadoEm { get; private set; }

    /// <summary>Onde a máquina está, na descrição do CEN. Ex.: nome da fazenda ou do talhão.</summary>
    public string? LocalizacaoDescrita { get; private set; }

    /// <summary>O endereço do cliente onde a máquina opera.</summary>
    public long? EnderecoId { get; private set; }

    /// <summary>[DYN] O implemento acoplado a um trator aponta para ele aqui.</summary>
    public long? EquipamentoPaiId { get; private set; }

    /// <summary>[DYN] A máquina que substituiu esta.</summary>
    public long? EquipamentoSubstitutoId { get; private set; }

    /// <summary>Em que estado a máquina está.</summary>
    public SituacaoDoEquipamento Situacao { get; private set; } = SituacaoDoEquipamento.Ativo;

    /// <summary>Quem afirma que esta máquina existe.</summary>
    public OrigemDoEquipamento Origem { get; private set; } = OrigemDoEquipamento.Crm;

    /// <summary>Quando foi vendida.</summary>
    public DateOnly? VendidoEm { get; private set; }

    /// <summary>Até quando vale a garantia.</summary>
    public DateOnly? GarantiaAte { get; private set; }

    /// <summary>Cadastra uma máquina.</summary>
    public static Equipamento Criar(
        int empresaId,
        int modeloId,
        Chassi chassi,
        OrigemDoEquipamento origem,
        long criadoPorId,
        long? clienteId = null,
        SituacaoDoEquipamento situacao = SituacaoDoEquipamento.Ativo,
        short? anoFabricacao = null,
        short? anoModelo = null,
        string? numeroSerie = null,
        string? placa = null,
        string? localizacaoDescrita = null)
    {
        ConferirAnos(anoFabricacao, anoModelo);
        ConferirPosse(situacao, clienteId);

        return new Equipamento
        {
            EmpresaId = empresaId,
            ModeloId = modeloId,
            Chassi = chassi,
            Origem = origem,
            CriadoPorId = criadoPorId,
            ClienteId = clienteId,
            Situacao = situacao,
            AnoFabricacao = anoFabricacao,
            AnoModelo = anoModelo,
            NumeroSerie = string.IsNullOrWhiteSpace(numeroSerie) ? null : numeroSerie.Trim(),
            Placa = string.IsNullOrWhiteSpace(placa) ? null : placa.Trim().ToUpperInvariant(),
            LocalizacaoDescrita = string.IsNullOrWhiteSpace(localizacaoDescrita) ? null : localizacaoDescrita.Trim()
        };
    }

    /// <summary>
    /// Altera o cadastro da máquina.
    ///
    /// A ORIGEM NÃO ENTRA, e é a decisão que mais importa aqui: quem afirma que a máquina
    /// existe — o ERP ou o CEN — não muda por edição de tela (documento 18: o Protheus é dono
    /// do ativo faturado; o CRM é dono da máquina declarada, inclusive a do concorrente).
    /// Trocar a origem apagaria justamente a informação que a tela de Cobertura usa.
    ///
    /// O CHASSI TAMBÉM NÃO: ele é a identidade da máquina e a chave de deduplicação
    /// (documento 16, seção 4). Chassi digitado errado se corrige inativando o registro e
    /// cadastrando o certo, para que o histórico não mude de dono em silêncio.
    /// </summary>
    public void Alterar(
        int modeloId,
        long usuarioId,
        long? clienteId = null,
        SituacaoDoEquipamento situacao = SituacaoDoEquipamento.Ativo,
        short? anoFabricacao = null,
        short? anoModelo = null,
        string? numeroSerie = null,
        string? placa = null,
        string? localizacaoDescrita = null)
    {
        if (EstaExcluido)
            throw new RegraDeNegocioViolada("Equipamento baixado não aceita alteração.");

        ConferirAnos(anoFabricacao, anoModelo);
        ConferirPosse(situacao, clienteId);

        ModeloId = modeloId;
        ClienteId = clienteId;
        Situacao = situacao;
        AnoFabricacao = anoFabricacao;
        AnoModelo = anoModelo;
        NumeroSerie = string.IsNullOrWhiteSpace(numeroSerie) ? null : numeroSerie.Trim();
        Placa = string.IsNullOrWhiteSpace(placa) ? null : placa.Trim().ToUpperInvariant();
        LocalizacaoDescrita = string.IsNullOrWhiteSpace(localizacaoDescrita) ? null : localizacaoDescrita.Trim();

        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Baixa a máquina: exclusão LÓGICA. A linha fica, o histórico de horímetro fica, e a
    /// máquina some das listagens de parque ativo.
    /// </summary>
    public void Inativar(long usuarioId)
    {
        if (EstaExcluido)
            throw new RegraDeNegocioViolada("Este equipamento já está baixado.");

        Situacao = SituacaoDoEquipamento.Baixado;
        Excluir(usuarioId);
    }

    /// <summary>
    /// Máquina em operação tem dono; máquina em estoque, não.
    ///
    /// [V] O legado deixa as duas coisas soltas, e é por isso que o parque de máquinas de lá
    /// não responde "quantas máquinas este cliente tem" sem ressalva.
    /// </summary>
    private static void ConferirPosse(SituacaoDoEquipamento situacao, long? clienteId)
    {
        if (situacao == SituacaoDoEquipamento.Estoque && clienteId is not null)
            throw new RegraDeNegocioViolada(
                "Máquina em estoque não tem dono. Informe a situação Ativo ou Vendido para vinculá-la a um cliente.");

        if (situacao is SituacaoDoEquipamento.Ativo or SituacaoDoEquipamento.Vendido && clienteId is null)
            throw new RegraDeNegocioViolada(
                "Máquina em operação precisa de cliente. Informe o cliente ou use a situação Estoque.");
    }

    private static void ConferirAnos(short? anoFabricacao, short? anoModelo)
    {
        // A restrição de verificação CK_Equipamento_Ano diz a mesma coisa no banco. Aqui a
        // recusa vem com a frase que o usuário lê, em vez de um erro de constraint.
        var limite = (short)(DateTime.UtcNow.Year + 1);

        if (anoFabricacao is { } fabricacao && (fabricacao < 1900 || fabricacao > limite))
            throw new RegraDeNegocioViolada(
                $"Ano de fabricação fora da faixa aceita (1900 a {limite}): {fabricacao}.");

        if (anoModelo is { } modelo && (modelo < 1900 || modelo > limite))
            throw new RegraDeNegocioViolada(
                $"Ano do modelo fora da faixa aceita (1900 a {limite}): {modelo}.");
    }
}

/// <summary>
/// Horas de operação da máquina ao longo do tempo.
///
/// A Ficha do Equipamento aprovada mostra a série histórica, não só o número atual. É série
/// temporal com cardinalidade maior que um e volume próprio, logo é tabela.
/// </summary>
public sealed class LeituraDeHorimetro
{
    private LeituraDeHorimetro() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A máquina.</summary>
    public long EquipamentoId { get; private set; }

    /// <summary>Quando a leitura foi feita (UTC).</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>Horas de operação acumuladas.</summary>
    public decimal Horas { get; private set; }

    /// <summary>De onde veio a leitura: ordem de serviço, telemetria, informação do cliente.</summary>
    public string Fonte { get; private set; } = default!;

    /// <summary>Quem registrou. Nulo quando veio de telemetria.</summary>
    public long? RegistradoPorId { get; private set; }

    /// <summary>Quando a linha foi gravada (UTC).</summary>
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Registra uma leitura.</summary>
    public static LeituraDeHorimetro Registrar(long equipamentoId, DateTime lidaEmUtc, decimal horas, string fonte)
    {
        if (horas < 0)
            throw new RegraDeNegocioViolada("Horímetro não anda para trás: leitura negativa não existe.");

        return new LeituraDeHorimetro
        {
            EquipamentoId = equipamentoId,
            LidaEm = lidaEmUtc,
            Horas = horas,
            Fonte = fonte
        };
    }
}
