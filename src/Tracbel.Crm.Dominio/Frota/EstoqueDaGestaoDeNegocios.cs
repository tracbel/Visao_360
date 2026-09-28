using System.Globalization;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Frota;

// =================================================================================================
// O ESTOQUE DE MÁQUINAS E A COBERTURA, PELA API GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026).
//
//   EquipamentoEmEstoque — o painel "Estoque & Pedidos" do TOTVS, máquina a máquina: o que está no pátio (estoque,
//                          remessa, consignado, em transferência) e o que vem da fábrica (PEDIDO), com a reserva, o
//                          pagamento e a chegada prevista. É de uma filial.
//   CoberturaDoEstoque   — quantos meses o estoque dura no ritmo de venda, por mês e por grupo de máquina. É da
//                          organização: a GN a calcula para a empresa inteira.
//
// Os dois são ESPELHO do TOTVS, lido pela GN: o CRM não edita, o que some da origem é excluído logicamente e o que volta
// é reativado na mesma linha.
//
// O QUE NÃO ENTRA, de propósito: o custo e o valor de compra (é custo, como o lucro e a margem do ART, que o CRM também não
// lê), o cliente do atendimento (nome de cliente), a nota, o FIDC e os opcionais. Os dias em estoque também não: mudam
// todo dia, e a tela os calcula da data de entrada.
// =================================================================================================

/// <summary>O que uma máquina do estoque traz da origem, já saneado.</summary>
/// <param name="EmpresaId">A filial — a fronteira de acesso.</param>
/// <param name="Chassi">O chassi; nulo no pedido de fábrica ainda sem chassi.</param>
/// <param name="Pedido">O pedido de compra à fábrica.</param>
/// <param name="Comar">O COMAR da John Deere.</param>
/// <param name="Situacao">A situação, como o TOTVS escreve (Estoque, PEDIDO, Remessa, Consignado, Em Transferência).</param>
/// <param name="Grupo">O grupo de máquina (os 13 da GN).</param>
/// <param name="Descricao">O modelo, como o TOTVS escreve.</param>
/// <param name="Configuracao">A configuração.</param>
/// <param name="Tipo">Máquina, AMS ou Implemento.</param>
/// <param name="EhUsado">Máquina usada.</param>
/// <param name="AnoModelo">Ano de fabricação e do modelo (<c>2025/2025</c>).</param>
/// <param name="EntradaEm">A entrada no estoque.</param>
/// <param name="ChegadaPrevistaEm">A chegada prevista da fábrica (FDD), no pedido.</param>
/// <param name="FaturamentoPrevistoEm">O faturamento previsto ao cliente.</param>
/// <param name="Pago">A máquina já foi paga à fábrica.</param>
/// <param name="Reservado">A máquina está reservada para uma venda.</param>
/// <param name="SituacaoNaFabrica">A situação do pedido na fábrica (Confirmado, Congelado, Faturado).</param>
/// <param name="HashDaOrigem">O resumo do conteúdo lido.</param>
public sealed record DadosDoEquipamentoEmEstoque(
    int EmpresaId,
    string? Chassi,
    string? Pedido,
    string? Comar,
    string Situacao,
    string Grupo,
    string Descricao,
    string? Configuracao,
    string Tipo,
    bool EhUsado,
    string? AnoModelo,
    DateOnly? EntradaEm,
    DateOnly? ChegadaPrevistaEm,
    DateOnly? FaturamentoPrevistoEm,
    bool Pago,
    bool Reservado,
    string? SituacaoNaFabrica,
    string HashDaOrigem);

/// <summary>
/// UMA MÁQUINA DO ESTOQUE OU DO PEDIDO À FÁBRICA — uma linha do painel "Estoque &amp; Pedidos" da API Gestão de Negócios
/// (<c>/api/v1/paineis/estoque-pedidos</c>), lido do TOTVS.
///
/// <para><b>A chave é o chassi interno do TOTVS</b> (<c>chaint</c>): 690 linhas, 690 distintos, medido em 28/09/2026. O
/// chassi da John Deere falta no pedido que ainda não saiu da fábrica.</para>
///
/// <para><b>O pedido à fábrica é a situação <c>PEDIDO</c></b>: não está no pátio, mas é disponibilidade futura, com a
/// chegada prevista (FDD).</para>
/// </summary>
public sealed class EquipamentoEmEstoque : EntidadeBase
{
    /// <summary>A situação do pedido à fábrica.</summary>
    public const string SituacaoDePedido = "PEDIDO";

    /// <summary>O tamanho da coluna do chassi interno.</summary>
    public const int TamanhoDoChaint = 30;

    /// <summary>O tamanho das colunas de código (chassi, pedido, COMAR).</summary>
    public const int TamanhoDoCodigo = 40;

    /// <summary>O tamanho das colunas curtas de texto (situação, grupo, tipo, situação na fábrica).</summary>
    public const int TamanhoDoTextoCurto = 60;

    /// <summary>O tamanho da descrição e da configuração.</summary>
    public const int TamanhoDaDescricao = 160;

    /// <summary>O fluxo da carga do estoque — o ponto de sincronismo (o frescor).</summary>
    public const string FluxoDaCarga = "GESTAO_NEGOCIOS.ESTOQUE";

    private EquipamentoEmEstoque() { }

    /// <summary>A filial — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O sistema de onde veio.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O chassi interno do TOTVS — a chave natural.</summary>
    public string Chaint { get; private set; } = default!;

    /// <summary>O chassi; nulo no pedido ainda sem chassi.</summary>
    public string? Chassi { get; private set; }

    /// <summary>O pedido à fábrica.</summary>
    public string? Pedido { get; private set; }

    /// <summary>O COMAR da John Deere.</summary>
    public string? Comar { get; private set; }

    /// <summary>A situação, como o TOTVS escreve.</summary>
    public string Situacao { get; private set; } = default!;

    /// <summary>O grupo de máquina.</summary>
    public string Grupo { get; private set; } = default!;

    /// <summary>O modelo, como o TOTVS escreve.</summary>
    public string Descricao { get; private set; } = default!;

    /// <summary>A configuração.</summary>
    public string? Configuracao { get; private set; }

    /// <summary>Máquina, AMS ou Implemento.</summary>
    public string Tipo { get; private set; } = default!;

    /// <summary>Máquina usada.</summary>
    public bool EhUsado { get; private set; }

    /// <summary>Ano de fabricação e do modelo.</summary>
    public string? AnoModelo { get; private set; }

    /// <summary>A entrada no estoque.</summary>
    public DateOnly? EntradaEm { get; private set; }

    /// <summary>A chegada prevista da fábrica (FDD).</summary>
    public DateOnly? ChegadaPrevistaEm { get; private set; }

    /// <summary>O faturamento previsto ao cliente.</summary>
    public DateOnly? FaturamentoPrevistoEm { get; private set; }

    /// <summary>Já paga à fábrica.</summary>
    public bool Pago { get; private set; }

    /// <summary>Reservada para uma venda.</summary>
    public bool Reservado { get; private set; }

    /// <summary>A situação do pedido na fábrica.</summary>
    public string? SituacaoNaFabrica { get; private set; }

    /// <summary>O resumo do conteúdo lido.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>A leitura que trouxe o conteúdo atual (UTC).</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>É pedido à fábrica, e não máquina no pátio.</summary>
    public bool EhPedidoDeFabrica => EhPedido(Situacao);

    /// <summary>Se a situação é a do pedido à fábrica.</summary>
    public static bool EhPedido(string situacao) => string.Equals(situacao?.Trim(), SituacaoDePedido, StringComparison.OrdinalIgnoreCase);

    /// <summary>Registra uma máquina lida da origem.</summary>
    public static EquipamentoEmEstoque Registrar(int sistemaId, string chaint, DadosDoEquipamentoEmEstoque dados, DateTime lidaEm, long criadoPorId)
    {
        var limpo = chaint?.Trim() ?? string.Empty;
        if (limpo.Length is 0 or > TamanhoDoChaint)
            throw new RegraDeNegocioViolada($"A máquina do estoque precisa do chassi interno do TOTVS, de 1 a {TamanhoDoChaint} caracteres.");
        var linha = new EquipamentoEmEstoque { SistemaId = sistemaId, Chaint = limpo, CriadoPorId = criadoPorId };
        linha.Aplicar(dados, lidaEm);
        return linha;
    }

    /// <summary>Aplica uma nova leitura; nada muda quando o resumo e a filial são os mesmos.</summary>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AtualizarDaOrigem(DadosDoEquipamentoEmEstoque dados, DateTime lidaEm, long usuarioId)
    {
        if (dados.HashDaOrigem == HashDaOrigem && dados.EmpresaId == EmpresaId) return false;
        Aplicar(dados, lidaEm);
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>Traz de volta a máquina que tinha sumido da origem.</summary>
    public bool Reativar(long usuarioId)
    {
        if (!EstaExcluido) return false;
        ExcluidoEm = null;
        MarcarAlteracao(usuarioId);
        return true;
    }

    private void Aplicar(DadosDoEquipamentoEmEstoque dados, DateTime lidaEm)
    {
        if (string.IsNullOrWhiteSpace(dados.HashDaOrigem)) throw new RegraDeNegocioViolada("A máquina do estoque guarda o resumo do conteúdo lido.");
        EmpresaId = dados.EmpresaId;
        Chassi = Opcional(dados.Chassi, TamanhoDoCodigo, "o chassi");
        Pedido = Opcional(dados.Pedido, TamanhoDoCodigo, "o pedido");
        Comar = Opcional(dados.Comar, TamanhoDoCodigo, "o COMAR");
        Situacao = Obrigatorio(dados.Situacao, TamanhoDoTextoCurto, "a situação");
        Grupo = Obrigatorio(dados.Grupo, TamanhoDoTextoCurto, "o grupo");
        Descricao = Obrigatorio(dados.Descricao, TamanhoDaDescricao, "a descrição");
        Configuracao = Opcional(dados.Configuracao, TamanhoDaDescricao, "a configuração");
        Tipo = Obrigatorio(dados.Tipo, TamanhoDoTextoCurto, "o tipo");
        EhUsado = dados.EhUsado;
        AnoModelo = Opcional(dados.AnoModelo, 9, "o ano");
        EntradaEm = dados.EntradaEm;
        ChegadaPrevistaEm = dados.ChegadaPrevistaEm;
        FaturamentoPrevistoEm = dados.FaturamentoPrevistoEm;
        Pago = dados.Pago;
        Reservado = dados.Reservado;
        SituacaoNaFabrica = Opcional(dados.SituacaoNaFabrica, TamanhoDoTextoCurto, "a situação na fábrica");
        HashDaOrigem = dados.HashDaOrigem;
        LidaEm = lidaEm;
    }

    private static string Obrigatorio(string texto, int tamanho, string oQue)
    {
        var limpo = texto?.Trim() ?? string.Empty;
        if (limpo.Length == 0 || limpo.Length > tamanho)
            throw new RegraDeNegocioViolada($"A máquina do estoque preserva {oQue} da origem, de 1 a {tamanho} caracteres.");
        return limpo;
    }

    private static string? Opcional(string? texto, int tamanho, string oQue)
    {
        var limpo = texto?.Trim();
        if (string.IsNullOrEmpty(limpo)) return null;
        if (limpo.Length > tamanho) throw new RegraDeNegocioViolada($"A máquina do estoque preserva {oQue} da origem, até {tamanho} caracteres.");
        return limpo;
    }
}

/// <summary>
/// A COBERTURA DO ESTOQUE — quantos meses o estoque dura no ritmo de venda, pela API Gestão de Negócios
/// (<c>/api/v1/cobertura</c>): o estoque dividido pelas saídas, em QUANTIDADE (o estoque está a custo e a venda a preço).
/// As saídas são as vendas da VVA010 do TOTVS, que cobre os treze grupos — inclusive AMS, que não passa pelo ART.
///
/// <para><b>Dois recortes</b>: por mês (os 13 meses fechados, no ritmo de venda de cada mês) e por grupo (o período
/// inteiro). A média que a GN mostra é a média simples dos itens de cada recorte — a tela a refaz.</para>
///
/// <para><b>É da organização</b>, e não de uma filial: não é <see cref="EntidadeBase"/>, que exige <c>EmpresaId</c>.
/// Segue o molde do dado de organização, com o próprio carimbo de importação e a própria exclusão lógica.</para>
/// </summary>
public sealed class CoberturaDoEstoque
{
    /// <summary>O recorte por mês.</summary>
    public const string RecortePorMes = "MES";

    /// <summary>O recorte por grupo de máquina.</summary>
    public const string RecortePorGrupo = "GRUPO";

    /// <summary>O tamanho da chave (o mês <c>AAAA-MM</c> ou o nome do grupo).</summary>
    public const int TamanhoDaChave = 60;

    /// <summary>O maior número de meses aceito — muito acima de qualquer cobertura real.</summary>
    public const decimal MesesMaximos = 999m;

    /// <summary>O fluxo da carga da cobertura.</summary>
    public const string FluxoDaCarga = "GESTAO_NEGOCIOS.COBERTURA";

    private CoberturaDoEstoque() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O sistema de onde veio.</summary>
    public int SistemaId { get; private set; }

    /// <summary><see cref="RecortePorMes"/> ou <see cref="RecortePorGrupo"/>.</summary>
    public string Recorte { get; private set; } = default!;

    /// <summary>O mês (<c>AAAA-MM</c>) ou o grupo.</summary>
    public string Chave { get; private set; } = default!;

    /// <summary>O mês, no dia 1, quando o recorte é por mês.</summary>
    public DateOnly? Competencia { get; private set; }

    /// <summary>Os meses de estoque.</summary>
    public decimal MesesDeEstoque { get; private set; }

    /// <summary>As vendas (saídas) que a conta usou, em quantidade.</summary>
    public int Vendas { get; private set; }

    /// <summary>O resumo do conteúdo lido.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>Quando a linha entrou no CRM (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga que a trouxe.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>A leitura que trouxe o conteúdo atual (UTC).</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>Quando a API calculou a cobertura (UTC), se disse.</summary>
    public DateTime? GeradaNaOrigemEm { get; private set; }

    /// <summary>Quando a linha sumiu da origem (UTC); nula enquanto está lá.</summary>
    public DateTime? ExcluidoEm { get; private set; }

    /// <summary>A chave natural de um item: o recorte e a chave.</summary>
    public static string ChaveNatural(string recorte, string chave) => $"{recorte}|{chave}";

    /// <summary>A chave de um mês: <c>AAAA-MM</c>.</summary>
    public static string ChaveDoMes(DateOnly competencia) => competencia.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    /// <summary>Registra um item lido da origem.</summary>
    public static CoberturaDoEstoque Registrar(
        int sistemaId, string recorte, string chave, DateOnly? competencia, decimal meses, int vendas, string hash, DateTime lidaEm,
        DateTime? geradaNaOrigemEm, long importadoPorId)
    {
        if (recorte is not (RecortePorMes or RecortePorGrupo)) throw new RegraDeNegocioViolada("A cobertura é por mês ou por grupo.");
        var limpa = chave?.Trim() ?? string.Empty;
        if (limpa.Length is 0 or > TamanhoDaChave) throw new RegraDeNegocioViolada($"A cobertura preserva a chave da origem, de 1 a {TamanhoDaChave} caracteres.");
        if (recorte == RecortePorMes && competencia is not { Day: 1 }) throw new RegraDeNegocioViolada("A cobertura por mês guarda o mês, no dia 1.");

        var linha = new CoberturaDoEstoque
        {
            SistemaId = sistemaId, Recorte = recorte, Chave = limpa, Competencia = recorte == RecortePorMes ? competencia : null,
            ImportadoEm = lidaEm, ImportadoPorId = importadoPorId
        };
        linha.Aplicar(meses, vendas, hash, lidaEm, geradaNaOrigemEm);
        return linha;
    }

    /// <summary>Aplica uma nova leitura; nada muda quando o resumo é o mesmo.</summary>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AtualizarDaOrigem(decimal meses, int vendas, string hash, DateTime lidaEm, DateTime? geradaNaOrigemEm)
    {
        if (hash == HashDaOrigem) return false;
        Aplicar(meses, vendas, hash, lidaEm, geradaNaOrigemEm);
        return true;
    }

    /// <summary>O item sumiu da origem: sai sem ser apagado.</summary>
    public void Excluir(DateTime agoraUtc) => ExcluidoEm ??= agoraUtc;

    /// <summary>Traz de volta o item que tinha sumido da origem.</summary>
    public bool Reativar()
    {
        if (ExcluidoEm is null) return false;
        ExcluidoEm = null;
        return true;
    }

    private void Aplicar(decimal meses, int vendas, string hash, DateTime lidaEm, DateTime? geradaNaOrigemEm)
    {
        if (meses is < 0 or > MesesMaximos) throw new RegraDeNegocioViolada($"A cobertura vai de 0 a {MesesMaximos} meses.");
        if (vendas < 0) throw new RegraDeNegocioViolada("As vendas da cobertura não são negativas.");
        if (string.IsNullOrWhiteSpace(hash)) throw new RegraDeNegocioViolada("A cobertura guarda o resumo do conteúdo lido.");
        MesesDeEstoque = decimal.Round(meses, 2);
        Vendas = vendas;
        HashDaOrigem = hash;
        LidaEm = lidaEm;
        GeradaNaOrigemEm = geradaNaOrigemEm;
    }
}
