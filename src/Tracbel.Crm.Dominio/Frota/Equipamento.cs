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
    Baixado = 3,

    /// <summary>
    /// Vendida pela Tracbel, e o dono atual não foi confirmado.
    ///
    /// <para>É a situação da máquina que chega por uma venda histórica (documento 35, seção 10): o
    /// comprador daquela venda está no vínculo com a data e a origem, mas quem comprou em 2024 não
    /// prova quem tem a máquina hoje. Por isso ela não tem <see cref="Equipamento.ClienteId"/> até
    /// alguém confirmar.</para>
    /// </summary>
    ProprietarioNaoConfirmado = 4
}

/// <summary>Quem afirma que esta máquina existe.</summary>
public enum OrigemDoEquipamento
{
    /// <summary>
    /// Veio do ERP: o cadastro de veículos do Protheus (VV1) — máquina que a Tracbel vendeu, nova ou usada, ou que a
    /// oficina atendeu.
    /// </summary>
    Protheus = 0,

    /// <summary>Foi declarada pelo CEN no CRM — inclusive a máquina do concorrente.</summary>
    Crm = 1,

    /// <summary>Veio de uma venda de máquina registrada no ART, o sistema comercial de vendas.</summary>
    Art = 2
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

    /// <summary>
    /// O modelo de catálogo. Nulo só na máquina que veio de integração com o produto ainda sem
    /// correspondência segura no catálogo — o produto da origem fica na venda, e a correspondência
    /// fica pendente de revisão, em vez de a máquina ganhar um modelo por semelhança de nome.
    /// </summary>
    public int? ModeloId { get; private set; }

    /// <summary>A classificação de produto do CRM (trator pequeno, colhedora de cana…).</summary>
    public int? LinhaDeProdutoId { get; private set; }

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
    /// Registra a máquina que chegou por uma VENDA da origem — sem dono atual.
    ///
    /// <para>O comprador da venda não vira <see cref="ClienteId"/>: ele entra no vínculo de comprador,
    /// com a data e a origem. O modelo só entra quando a correspondência do produto é segura.</para>
    /// </summary>
    /// <param name="empresaId">A filial da venda.</param>
    /// <param name="chassi">O chassi, já validado.</param>
    /// <param name="origem">Quem afirma que a máquina existe.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    /// <param name="modeloId">O modelo, quando a correspondência do produto é segura.</param>
    /// <param name="linhaDeProdutoId">A classificação de produto, quando existe.</param>
    /// <param name="anoFabricacao">O ano de fabricação, quando a origem o declara.</param>
    /// <param name="anoModelo">O ano do modelo, quando a origem o declara.</param>
    public static Equipamento RegistrarPelaIntegracao(
        int empresaId,
        Chassi chassi,
        OrigemDoEquipamento origem,
        long criadoPorId,
        int? modeloId = null,
        int? linhaDeProdutoId = null,
        short? anoFabricacao = null,
        short? anoModelo = null)
    {
        // A MÁQUINA DE INTEGRAÇÃO SEM MODELO é a do ART e a do cadastro de veículos do Protheus: o produto da
        // origem fica na origem, e a correspondência com o catálogo espera uma regra segura.
        if (origem == OrigemDoEquipamento.Crm)
            throw new RegraDeNegocioViolada("A máquina declarada no CRM é cadastrada pela tela, com modelo — não pela integração.");

        ConferirAnos(anoFabricacao, anoModelo);

        return new Equipamento
        {
            EmpresaId = empresaId,
            Chassi = chassi,
            Origem = origem,
            CriadoPorId = criadoPorId,
            ModeloId = modeloId,
            LinhaDeProdutoId = linhaDeProdutoId,
            AnoFabricacao = anoFabricacao,
            AnoModelo = anoModelo,
            Situacao = SituacaoDoEquipamento.ProprietarioNaoConfirmado
        };
    }

    /// <summary>
    /// CONFIRMA O DONO: a máquina deixa de ser "proprietário não confirmado" e passa a ser do cliente, em operação.
    ///
    /// <para>Quem confirma pela integração é a sincronia do parque (decisão de 24/09/2026), e só quando duas fontes
    /// independentes concordam: o dono atual no cadastro de veículos do Protheus é o comprador da venda mais recente no
    /// ART. Uma pessoa confirma pela tela, com <see cref="Alterar"/>.</para>
    /// </summary>
    /// <param name="clienteId">O dono confirmado.</param>
    /// <param name="usuarioId">Quem confirma.</param>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool ConfirmarProprietario(long clienteId, long usuarioId)
    {
        if (EstaExcluido)
            throw new RegraDeNegocioViolada("Equipamento baixado não aceita alteração.");
        if (Situacao is not (SituacaoDoEquipamento.ProprietarioNaoConfirmado or SituacaoDoEquipamento.Ativo))
            throw new RegraDeNegocioViolada(
                $"Só a máquina em operação, ou com o dono ainda não confirmado, recebe dono confirmado (situação atual: {Situacao}).");

        if (Situacao == SituacaoDoEquipamento.Ativo && ClienteId == clienteId) return false;

        ClienteId = clienteId;
        Situacao = SituacaoDoEquipamento.Ativo;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// DESFAZ A CONFIRMAÇÃO: as duas fontes deixaram de concordar — a máquina foi revendida, e o dono atual no
    /// Protheus já não é o comprador do ART. Ela volta a "proprietário não confirmado", sem dono; quem é o dono atual
    /// segundo o Protheus continua no vínculo <see cref="NaturezaDoVinculoComEquipamento.ProprietarioAtual"/>.
    /// </summary>
    /// <param name="usuarioId">Quem desfaz.</param>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool DesfazerConfirmacaoDoProprietario(long usuarioId)
    {
        if (EstaExcluido || Situacao != SituacaoDoEquipamento.Ativo) return false;

        ClienteId = null;
        Situacao = SituacaoDoEquipamento.ProprietarioNaoConfirmado;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// Aponta os anos de fabricação e de modelo que ainda estão vazios. Ano já informado — pela carga ou por uma
    /// pessoa — não é trocado; ano fora da faixa aceita é ignorado, e não gravado.
    /// </summary>
    /// <param name="anoFabricacao">O ano de fabricação da origem.</param>
    /// <param name="anoModelo">O ano do modelo da origem.</param>
    /// <param name="usuarioId">Quem aponta.</param>
    /// <returns>Verdadeiro quando algum ano foi apontado agora.</returns>
    public bool DefinirAnosSeAusentes(short? anoFabricacao, short? anoModelo, long usuarioId)
    {
        if (EstaExcluido) return false;

        var fabricacao = AnoFabricacao is null && AnoAceito(anoFabricacao) ? anoFabricacao : null;
        var modelo = AnoModelo is null && AnoAceito(anoModelo) ? anoModelo : null;
        if (fabricacao is null && modelo is null) return false;

        AnoFabricacao ??= fabricacao;
        AnoModelo ??= modelo;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>Se o ano cabe na faixa que a restrição <c>CK_Equipamento_Ano</c> e a tela aceitam.</summary>
    /// <param name="ano">O ano.</param>
    public static bool AnoAceito(short? ano) => ano is { } a && a >= 1900 && a <= DateTime.UtcNow.Year + 1;

    /// <summary>
    /// Aponta o modelo quando a máquina ainda não tem nenhum. Modelo já definido — pela carga ou por
    /// uma pessoa — não é trocado: é a correção que a recarga precisa preservar.
    /// </summary>
    /// <param name="modeloId">O modelo da correspondência segura.</param>
    /// <param name="usuarioId">Quem aponta.</param>
    /// <returns>Verdadeiro quando o modelo foi apontado agora.</returns>
    public bool DefinirModeloSeAusente(int modeloId, long usuarioId)
    {
        if (ModeloId is not null || EstaExcluido) return false;
        ModeloId = modeloId;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// Classifica a máquina quando ela ainda não tem classificação. A classificação existente não é
    /// trocada pela carga.
    /// </summary>
    /// <param name="linhaDeProdutoId">A classificação.</param>
    /// <param name="usuarioId">Quem classifica.</param>
    /// <returns>Verdadeiro quando a classificação foi apontada agora.</returns>
    public bool ClassificarSeAusente(int linhaDeProdutoId, long usuarioId)
    {
        if (LinhaDeProdutoId is not null || EstaExcluido) return false;
        LinhaDeProdutoId = linhaDeProdutoId;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>Classifica — ou tira a classificação — por decisão de quem edita o cadastro.</summary>
    /// <param name="linhaDeProdutoId">A classificação, ou nula.</param>
    /// <param name="usuarioId">Quem classifica.</param>
    public void Classificar(int? linhaDeProdutoId, long usuarioId)
    {
        if (EstaExcluido)
            throw new RegraDeNegocioViolada("Equipamento baixado não aceita alteração.");

        if (LinhaDeProdutoId == linhaDeProdutoId) return;
        LinhaDeProdutoId = linhaDeProdutoId;
        MarcarAlteracao(usuarioId);
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
        int? modeloId,
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

        // O MODELO SÓ FICA VAZIO na máquina que veio de integração sem correspondência segura — o ART e o
        // cadastro de veículos do Protheus — e, uma vez escolhido, não volta a ficar vazio por edição.
        if (modeloId is null && (Origem == OrigemDoEquipamento.Crm || ModeloId is not null))
            throw new RegraDeNegocioViolada(
                "Informe o modelo do catálogo: só a máquina vinda do ART ou do Protheus ainda sem modelo pode ficar sem ele.");

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

        if (situacao == SituacaoDoEquipamento.ProprietarioNaoConfirmado && clienteId is not null)
            throw new RegraDeNegocioViolada(
                "Com o dono informado, a situação é Ativo ou Vendido: 'proprietário não confirmado' é a máquina sem dono confirmado.");

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

// O QUE SAIU DAQUI NA FASE 1 (documento 41): `LeituraDeHorimetro`, a série histórica de horas de
// operação da máquina. A tabela nasceu com o modelo inicial e nunca recebeu uma linha: nem a carga
// do Vórtice nem a do ART trazem horímetro, e a Ficha do Equipamento mostra hoje o número atual que
// está no próprio equipamento.
//
// A série histórica continua no desenho do documento 40, na fase de equipamentos e rastro: ela volta
// quando existir a origem que a alimenta — ordem de serviço ou telemetria.
