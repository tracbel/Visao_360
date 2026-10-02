using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Frota;

// =================================================================================================
// AS ORDENS DE SERVIÇO DA OFICINA, DO PROTHEUS (pedido do Ricardo em 02/10/2026: "trazer os dados que faltam").
//
// Até aqui o CRM consultava a ordem de serviço mais recente de cada máquina só para decidir o dono atual (a evidência
// "ordem de serviço" do parque) e não a guardava. A ficha do cliente dizia "o CRM ainda não carrega as ordens de serviço
// da oficina", e a aba Pós-vendas mostrava o exemplo fixo do protótipo.
//
// A FONTE É A MESMA DO BI: as views X_V_BI_SERVICOS_CAPA_E_ITENS_OS (a OS item a item, com a capa repetida) e
// X_V_BI_SERVICOS_SRV_EXECUTADO_OS (o serviço executado, VO4) do banco do Protheus — as do extrator "Pós Vendas
// Serviços" do Qlik. O valor de peça e o de serviço seguem a régua do painel "Pós-Venda (Serviços)" do BI.
//
// O QUE NÃO ENTRA, de propósito: o nome, o documento, o endereço, o e-mail e o telefone do cliente (a view traz; o
// documento serve só para casar com o cadastro do CRM e é descartado), o técnico e o consultor, a placa e a cor.
// =================================================================================================

/// <summary>A situação da ordem de serviço, como o Protheus a escreve na capa (<c>VO1_STATUS</c>).</summary>
public enum SituacaoDaOrdemDeServico
{
    /// <summary><c>A</c> — aberta, em andamento na oficina.</summary>
    Aberta,

    /// <summary><c>L</c> (e o <c>D</c> antigo, que o BI trata como <c>L</c>) — liberada, esperando o fechamento.</summary>
    Liberada,

    /// <summary><c>F</c> — fechada.</summary>
    Fechada,

    /// <summary><c>C</c> — cancelada.</summary>
    Cancelada
}

/// <summary>O que uma ordem de serviço traz da origem, já saneado e casado com o CRM.</summary>
/// <param name="EmpresaId">A filial da OS — a fronteira de acesso.</param>
/// <param name="Numero">O número da OS na filial (<c>VO1_NUMOSV</c>).</param>
/// <param name="ClienteId">O cliente do CRM, casado pelo CPF/CNPJ do proprietário; nulo quando não casa.</param>
/// <param name="EquipamentoId">A máquina do CRM, casada pelo chassi; nula quando não casa.</param>
/// <param name="Chassi">O chassi como o Protheus escreve.</param>
/// <param name="Modelo">O modelo como o Protheus escreve.</param>
/// <param name="Horimetro">O horímetro informado na abertura.</param>
/// <param name="Situacao">A situação da capa.</param>
/// <param name="TipoDeAtendimento">O tipo de atendimento da capa (oficina, campo…), como o Protheus escreve.</param>
/// <param name="AbertaEm">A abertura.</param>
/// <param name="LiberadaEm">A liberação.</param>
/// <param name="FechadaEm">O fechamento.</param>
/// <param name="CanceladaEm">O cancelamento.</param>
/// <param name="ValorDePecas">As peças, já sem desconto — a régua do BI.</param>
/// <param name="ValorDeServicos">Os serviços, já sem desconto — a régua do BI.</param>
/// <param name="ItensDePeca">As peças requisitadas (linhas distintas).</param>
/// <param name="ItensDeServico">Os serviços executados (linhas distintas).</param>
/// <param name="HashDaOrigem">O resumo do conteúdo lido.</param>
public sealed record DadosDaOrdemDeServico(
    int EmpresaId,
    string Numero,
    long? ClienteId,
    long? EquipamentoId,
    string? Chassi,
    string? Modelo,
    decimal? Horimetro,
    SituacaoDaOrdemDeServico Situacao,
    string? TipoDeAtendimento,
    DateOnly AbertaEm,
    DateOnly? LiberadaEm,
    DateOnly? FechadaEm,
    DateOnly? CanceladaEm,
    decimal ValorDePecas,
    decimal ValorDeServicos,
    int ItensDePeca,
    int ItensDeServico,
    string HashDaOrigem);

/// <summary>
/// UMA ORDEM DE SERVIÇO DA OFICINA — a capa da OS do Protheus, com o total de peças e de serviços.
///
/// <para><b>A chave é a filial e o número</b> (<c>FILIAL|NUMERO_OS</c>, o <c>ID_OS</c> do BI): o número se repete entre
/// filiais. É espelho do Protheus: o CRM não edita; a OS que some da origem é excluída logicamente e a que volta é
/// reativada na mesma linha.</para>
///
/// <para><b>O grão é a OS, e não o item.</b> A pergunta da ficha do cliente e da máquina é "quantas OS, de quando, de
/// quanto, quantas ainda abertas e há quantos dias" — o item a item fica no ERP.</para>
/// </summary>
public sealed class OrdemDeServico : EntidadeBase
{
    /// <summary>O tamanho da chave na origem (<c>FILIAL|NUMERO</c>).</summary>
    public const int TamanhoDaChave = 40;

    /// <summary>O tamanho do número da OS.</summary>
    public const int TamanhoDoNumero = 20;

    /// <summary>O tamanho do chassi.</summary>
    public const int TamanhoDoChassi = 40;

    /// <summary>O tamanho do modelo e do tipo de atendimento.</summary>
    public const int TamanhoDoTexto = 120;

    /// <summary>O maior horímetro aceito — acima disso é digitação errada, e o campo fica vazio.</summary>
    public const decimal HorimetroMaximo = 999_999m;

    /// <summary>O fluxo da carga — o ponto de sincronismo (o frescor).</summary>
    public const string FluxoDaCarga = "PROTHEUS.ORDENS_DE_SERVICO";

    private OrdemDeServico() { }

    /// <summary>A filial da OS — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O sistema de onde veio.</summary>
    public int SistemaId { get; private set; }

    /// <summary>A chave natural: <c>FILIAL|NUMERO_OS</c>, como o BI a monta.</summary>
    public string ChaveNaOrigem { get; private set; } = default!;

    /// <summary>O número da OS na filial.</summary>
    public string Numero { get; private set; } = default!;

    /// <summary>O cliente do CRM, casado pelo documento do proprietário; nulo quando não casa.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>A máquina do CRM, casada pelo chassi; nula quando não casa.</summary>
    public long? EquipamentoId { get; private set; }

    /// <summary>O chassi como o Protheus escreve.</summary>
    public string? Chassi { get; private set; }

    /// <summary>O modelo como o Protheus escreve.</summary>
    public string? Modelo { get; private set; }

    /// <summary>O horímetro informado na abertura.</summary>
    public decimal? Horimetro { get; private set; }

    /// <summary>A situação da capa.</summary>
    public SituacaoDaOrdemDeServico Situacao { get; private set; }

    /// <summary>O tipo de atendimento da capa, como o Protheus escreve.</summary>
    public string? TipoDeAtendimento { get; private set; }

    /// <summary>A abertura.</summary>
    public DateOnly AbertaEm { get; private set; }

    /// <summary>A liberação.</summary>
    public DateOnly? LiberadaEm { get; private set; }

    /// <summary>O fechamento.</summary>
    public DateOnly? FechadaEm { get; private set; }

    /// <summary>O cancelamento.</summary>
    public DateOnly? CanceladaEm { get; private set; }

    /// <summary>As peças, já sem desconto.</summary>
    public decimal ValorDePecas { get; private set; }

    /// <summary>Os serviços, já sem desconto.</summary>
    public decimal ValorDeServicos { get; private set; }

    /// <summary>As peças requisitadas.</summary>
    public int ItensDePeca { get; private set; }

    /// <summary>Os serviços executados.</summary>
    public int ItensDeServico { get; private set; }

    /// <summary>O resumo do conteúdo lido.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>A leitura que trouxe o conteúdo atual (UTC).</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>Peças mais serviços.</summary>
    public decimal ValorTotal => ValorDePecas + ValorDeServicos;

    /// <summary>Se a OS ainda está na oficina — aberta ou liberada, como o "empenhado" do BI.</summary>
    public bool EstaEmAberto => EstaEmAbertoNa(Situacao);

    /// <summary>Se a situação é de OS ainda na oficina.</summary>
    /// <param name="situacao">A situação.</param>
    public static bool EstaEmAbertoNa(SituacaoDaOrdemDeServico situacao) =>
        situacao is SituacaoDaOrdemDeServico.Aberta or SituacaoDaOrdemDeServico.Liberada;

    /// <summary>
    /// A situação pela letra da capa. O <c>D</c> é o <c>L</c> antigo — o BI troca um pelo outro na carga. Letra
    /// desconhecida é nula: a carga a conta como formato ilegível, em vez de adivinhar.
    /// </summary>
    /// <param name="letra">O <c>STATUS_CAPA_OS</c>.</param>
    public static SituacaoDaOrdemDeServico? SituacaoPelaLetra(string? letra) => letra?.Trim().ToUpperInvariant() switch
    {
        "A" => SituacaoDaOrdemDeServico.Aberta,
        "L" or "D" => SituacaoDaOrdemDeServico.Liberada,
        "F" => SituacaoDaOrdemDeServico.Fechada,
        "C" => SituacaoDaOrdemDeServico.Cancelada,
        _ => null
    };

    /// <summary>A chave natural: a filial e o número, aparados.</summary>
    /// <param name="filial">O código da filial no Protheus.</param>
    /// <param name="numero">O número da OS.</param>
    public static string ChaveDe(string filial, string numero) => $"{filial.Trim()}|{numero.Trim()}";

    /// <summary>Os dias desde a abertura, para a OS ainda na oficina; nulo para a fechada ou cancelada.</summary>
    /// <param name="hoje">A data de referência.</param>
    public int? DiasEmAberto(DateOnly hoje) => EstaEmAberto ? Math.Max(0, hoje.DayNumber - AbertaEm.DayNumber) : null;

    /// <summary>Registra uma OS lida da origem.</summary>
    /// <param name="sistemaId">O sistema de origem.</param>
    /// <param name="chaveNaOrigem">A chave <c>FILIAL|NUMERO</c>.</param>
    /// <param name="dados">O conteúdo.</param>
    /// <param name="lidaEm">A leitura (UTC).</param>
    /// <param name="criadoPorId">Quem rodou a carga.</param>
    public static OrdemDeServico Registrar(int sistemaId, string chaveNaOrigem, DadosDaOrdemDeServico dados, DateTime lidaEm, long criadoPorId)
    {
        var chave = chaveNaOrigem?.Trim() ?? string.Empty;
        if (chave.Length is 0 or > TamanhoDaChave)
            throw new RegraDeNegocioViolada($"A ordem de serviço precisa da chave da origem (filial e número), de 1 a {TamanhoDaChave} caracteres.");

        var ordem = new OrdemDeServico { SistemaId = sistemaId, ChaveNaOrigem = chave, CriadoPorId = criadoPorId };
        ordem.Aplicar(dados, lidaEm);
        return ordem;
    }

    /// <summary>Aplica uma nova leitura; nada muda quando o resumo, a filial e os casamentos são os mesmos.</summary>
    /// <param name="dados">O conteúdo.</param>
    /// <param name="lidaEm">A leitura (UTC).</param>
    /// <param name="usuarioId">Quem rodou a carga.</param>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AtualizarDaOrigem(DadosDaOrdemDeServico dados, DateTime lidaEm, long usuarioId)
    {
        // O CASAMENTO ENTRA NA COMPARAÇÃO: o cliente cadastrado depois da OS, ou a máquina que o parque trouxe, mudam a OS
        // sem que nada tenha mudado no Protheus.
        if (dados.HashDaOrigem == HashDaOrigem && dados.EmpresaId == EmpresaId
            && dados.ClienteId == ClienteId && dados.EquipamentoId == EquipamentoId)
            return false;

        Aplicar(dados, lidaEm);
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>Traz de volta a OS que tinha sumido da origem.</summary>
    /// <param name="usuarioId">Quem rodou a carga.</param>
    public bool Reativar(long usuarioId)
    {
        if (!EstaExcluido) return false;
        ExcluidoEm = null;
        MarcarAlteracao(usuarioId);
        return true;
    }

    private void Aplicar(DadosDaOrdemDeServico dados, DateTime lidaEm)
    {
        if (string.IsNullOrWhiteSpace(dados.HashDaOrigem))
            throw new RegraDeNegocioViolada("A ordem de serviço guarda o resumo do conteúdo lido.");
        if (dados.ItensDePeca < 0 || dados.ItensDeServico < 0)
            throw new RegraDeNegocioViolada("A ordem de serviço não tem quantidade negativa de itens.");
        if (dados.Horimetro is < 0 or > HorimetroMaximo)
            throw new RegraDeNegocioViolada($"O horímetro da ordem de serviço vai de 0 a {HorimetroMaximo:N0} horas.");

        var numero = dados.Numero?.Trim() ?? string.Empty;
        if (numero.Length is 0 or > TamanhoDoNumero)
            throw new RegraDeNegocioViolada($"A ordem de serviço preserva o número da origem, de 1 a {TamanhoDoNumero} caracteres.");

        EmpresaId = dados.EmpresaId;
        Numero = numero;
        ClienteId = dados.ClienteId;
        EquipamentoId = dados.EquipamentoId;
        Chassi = Opcional(dados.Chassi, TamanhoDoChassi, "o chassi");
        Modelo = Opcional(dados.Modelo, TamanhoDoTexto, "o modelo");
        Horimetro = dados.Horimetro;
        Situacao = dados.Situacao;
        TipoDeAtendimento = Opcional(dados.TipoDeAtendimento, TamanhoDoTexto, "o tipo de atendimento");
        AbertaEm = dados.AbertaEm;
        LiberadaEm = dados.LiberadaEm;
        FechadaEm = dados.FechadaEm;
        CanceladaEm = dados.CanceladaEm;
        ValorDePecas = decimal.Round(dados.ValorDePecas, 2);
        ValorDeServicos = decimal.Round(dados.ValorDeServicos, 2);
        ItensDePeca = dados.ItensDePeca;
        ItensDeServico = dados.ItensDeServico;
        HashDaOrigem = dados.HashDaOrigem;
        LidaEm = lidaEm;
    }

    private static string? Opcional(string? texto, int tamanho, string oQue)
    {
        var limpo = texto?.Trim();
        if (string.IsNullOrEmpty(limpo)) return null;
        if (limpo.Length > tamanho)
            throw new RegraDeNegocioViolada($"A ordem de serviço preserva {oQue} da origem, até {tamanho} caracteres.");
        return limpo;
    }
}
