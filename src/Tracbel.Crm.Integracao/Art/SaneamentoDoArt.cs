using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Integracao.Art;

/// <summary>Em que estado o chassi chega do ART.</summary>
public enum SituacaoDoChassiNaOrigem
{
    /// <summary>17 letras e números — o VIN, que entra sozinho.</summary>
    Valido = 0,

    /// <summary>Campo vazio.</summary>
    Vazio = 1,

    /// <summary>Menos de 17 caracteres — número de série curto ou chassi cortado.</summary>
    Incompleto = 2,

    /// <summary>Com símbolo, ou tamanho acima de 17, sem ser uma lista de chassis.</summary>
    ForaDoPadrao = 3,

    /// <summary>Dois ou mais chassis válidos no mesmo campo — venda de lote.</summary>
    Multiplo = 4,

    /// <summary>
    /// Número de série curto que o cadastro de veículos do Protheus tem exatamente — a identidade confirmada
    /// (decisão 2 de 24/09/2026). Entra como o VIN.
    /// </summary>
    ConfirmadoPeloProtheus = 5
}

/// <summary>Os códigos de pendência que o saneamento e a carga atribuem a um registro do ART.</summary>
public static class MotivoDePendenciaDoArt
{
    /// <summary>Chassi vazio.</summary>
    public const string ChassiVazio = "CHASSI_VAZIO";

    /// <summary>Chassi com menos de 17 caracteres, sem confirmação no cadastro de veículos do Protheus.</summary>
    public const string ChassiIncompleto = "CHASSI_INCOMPLETO";

    /// <summary>
    /// O identificador curto está no cadastro de veículos do Protheus, mas como COMPONENTE (agricultura de precisão,
    /// motor, capota, kit) — não é a máquina, e não vira uma.
    /// </summary>
    public const string IdentificadorDeComponente = "IDENTIFICADOR_DE_COMPONENTE_NO_PROTHEUS";

    /// <summary>Chassi fora do padrão VIN.</summary>
    public const string ChassiForaDoPadrao = "CHASSI_FORA_DO_PADRAO";

    /// <summary>Mais de um chassi no mesmo campo.</summary>
    public const string ChassiMultiplo = "CHASSI_MULTIPLO";

    /// <summary>CPF ou CNPJ que não passa no dígito verificador.</summary>
    public const string DocumentoInvalido = "DOCUMENTO_INVALIDO";

    /// <summary>Linha ou produto vazios — a venda não se sustenta sem eles.</summary>
    public const string ProdutoVazio = "LINHA_OU_PRODUTO_VAZIO";

    /// <summary>Documento válido sem cliente no CRM.</summary>
    public const string CompradorAusente = "COMPRADOR_AUSENTE_NO_CRM";

    /// <summary>Documento com mais de um cliente no CRM e nenhum critério para escolher.</summary>
    public const string CompradorAmbiguo = "COMPRADOR_AMBIGUO_NO_CRM";

    /// <summary>Unidade sem filial correspondente.</summary>
    public const string UnidadeSemFilial = "UNIDADE_SEM_FILIAL";

    /// <summary>O chassi é de uma máquina baixada no CRM — não é reativada nem duplicada.</summary>
    public const string MaquinaBaixada = "MAQUINA_BAIXADA_NO_CRM";
}

/// <summary>Um registro do ART depois do saneamento — pronto para a carga decidir.</summary>
/// <param name="Codigo">O identificador do registro no ART.</param>
/// <param name="SituacaoDoChassi">Em que estado o chassi chegou.</param>
/// <param name="Chassi">O chassi, quando válido.</param>
/// <param name="ChassiNaOrigem">O chassi como o ART escreve, sem espaço nas pontas.</param>
/// <param name="Documento">O documento do comprador, quando válido.</param>
/// <param name="NomeDoComprador">O nome como o ART escreve — só para a fila de compradores.</param>
/// <param name="Linha">A linha, sem espaço nas pontas.</param>
/// <param name="Produto">O produto, sem espaço nas pontas.</param>
/// <param name="Empresa">A empresa do ART (Agro Norte, Agro Noroeste).</param>
/// <param name="Unidade">A unidade que vendeu.</param>
/// <param name="UnidadeDoFaturamento">A unidade que faturou.</param>
/// <param name="VendidaEm">A data da venda.</param>
/// <param name="FaturadaEm">A data do faturamento.</param>
/// <param name="EntregueEm">A data da entrega.</param>
/// <param name="AbertaEm">Quando a venda foi aberta no ART.</param>
/// <param name="Situacao">A situação como o ART escreve.</param>
/// <param name="NumeroDoPedido">O número do pedido.</param>
/// <param name="NumeroDaNotaFiscal">O número da nota de venda.</param>
/// <param name="Gestao">Varejo ou Grandes Contas, como o ART escreve.</param>
/// <param name="VendaDireta">Venda direta.</param>
/// <param name="RepasseDireto">Repasse direto.</param>
/// <param name="Quantidade">A quantidade declarada.</param>
/// <param name="Hash">O resumo do conteúdo lido.</param>
/// <param name="Transformacoes">O que o saneamento mudou, uma frase por mudança.</param>
/// <param name="Motivos">Os códigos de pendência que o saneamento já encontrou.</param>
public sealed record VendaDoArtSaneada(
    string Codigo,
    SituacaoDoChassiNaOrigem SituacaoDoChassi,
    Chassi? Chassi,
    string? ChassiNaOrigem,
    CpfCnpj? Documento,
    string? NomeDoComprador,
    string Linha,
    string Produto,
    string? Empresa,
    string? Unidade,
    string? UnidadeDoFaturamento,
    DateOnly? VendidaEm,
    DateOnly? FaturadaEm,
    DateOnly? EntregueEm,
    DateTime? AbertaEm,
    string? Situacao,
    string? NumeroDoPedido,
    string? NumeroDaNotaFiscal,
    string? Gestao,
    bool VendaDireta,
    bool RepasseDireto,
    int? Quantidade,
    string Hash,
    IReadOnlyList<string> Transformacoes,
    IReadOnlyList<string> Motivos)
{
    /// <summary>As transformações numa frase só, no tamanho da coluna — ou nulo quando não houve nenhuma.</summary>
    public string? TransformacoesEmTexto
    {
        get
        {
            if (Transformacoes.Count == 0) return null;

            // A trilha é resumo legível; a lista completa é recalculável a partir da origem.
            var texto = string.Join("; ", Transformacoes);
            return texto.Length <= 1000 ? texto : texto[..997] + "...";
        }
    }
}

/// <summary>
/// O SANEAMENTO DO ART — funções puras, sem banco e sem rede, testáveis linha a linha.
///
/// <para>Obedece ao princípio 1.3 do documento 16: valor que não passa sai <b>vazio com o motivo
/// escrito</b> — data zerada não vira "hoje", chassi cortado não é completado, texto maior que a
/// coluna não é cortado.</para>
/// </summary>
public static class SaneamentoDoArt
{
    /// <summary>Saneia um registro.</summary>
    /// <param name="registro">O registro como veio do ART.</param>
    public static VendaDoArtSaneada Sanear(RegistroDoArt registro)
    {
        var transformacoes = new List<string>();
        var motivos = new List<string>();

        var (situacaoDoChassi, chassi) = ClassificarChassi(registro.Chassis);
        switch (situacaoDoChassi)
        {
            case SituacaoDoChassiNaOrigem.Vazio: motivos.Add(MotivoDePendenciaDoArt.ChassiVazio); break;
            case SituacaoDoChassiNaOrigem.Incompleto: motivos.Add(MotivoDePendenciaDoArt.ChassiIncompleto); break;
            case SituacaoDoChassiNaOrigem.ForaDoPadrao: motivos.Add(MotivoDePendenciaDoArt.ChassiForaDoPadrao); break;
            case SituacaoDoChassiNaOrigem.Multiplo: motivos.Add(MotivoDePendenciaDoArt.ChassiMultiplo); break;
        }

        if (chassi is { } valido && !string.Equals(valido.Numero, registro.Chassis?.Trim(), StringComparison.Ordinal))
            transformacoes.Add("chassi: espaço interno ou letra minúscula normalizados");

        var digitos = new string([.. (registro.CpfCnpj ?? string.Empty).Where(char.IsAsciiDigit)]);
        CpfCnpj? documento = CpfCnpj.TentarCriar(digitos, out var doc) ? doc : null;
        if (documento is null) motivos.Add(MotivoDePendenciaDoArt.DocumentoInvalido);

        var linha = registro.Linha?.Trim() ?? string.Empty;
        var produto = registro.Produto?.Trim() ?? string.Empty;
        if (linha.Length == 0 || produto.Length == 0 || linha.Length > 60 || produto.Length > 60)
            motivos.Add(MotivoDePendenciaDoArt.ProdutoVazio);

        return new VendaDoArtSaneada(
            registro.Codigo.Trim(),
            situacaoDoChassi,
            chassi,
            Texto("chassi na origem", registro.Chassis, 200, transformacoes),
            documento,
            Texto("nome do comprador", registro.Cliente, 100, transformacoes),
            linha,
            produto,
            Texto("empresa", registro.Empresa, 60, transformacoes),
            Texto("unidade", registro.Unidade, 80, transformacoes),
            Texto("unidade do faturamento", registro.UnidadeDoFaturamento, 80, transformacoes),
            Data("data da venda", registro.DataDaVenda, transformacoes),
            Data("data do faturamento", registro.DataDoFaturamento, transformacoes),
            Data("data da entrega", registro.Entrega, transformacoes),
            DataHora("abertura", registro.AbertaEm, transformacoes),
            Texto("situação", registro.Situacao, 10, transformacoes),
            Texto("pedido", registro.NumeroDoPedido, 20, transformacoes),
            Texto("nota fiscal", registro.NumeroDaNotaFiscal, 60, transformacoes),
            Texto("gestão", registro.Gestao, 30, transformacoes),
            SimOuNao("venda direta", registro.VendaDireta, transformacoes),
            SimOuNao("repasse direto", registro.RepasseDireto, transformacoes),
            int.TryParse(registro.Quantidade, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantidade) ? quantidade : null,
            Resumir(registro),
            transformacoes,
            motivos);
    }

    /// <summary>
    /// Classifica o chassi. Só espaço é removido; qualquer outro símbolo deixa o chassi fora do padrão,
    /// porque adivinhar qual parte é o chassi seria inventar a identidade da máquina. Dezessete letras e
    /// números é VIN — inclusive com I, O e Q, como o prefixo <c>1CQ</c> da John Deere (decisão 4 de
    /// 24/09/2026).
    /// </summary>
    /// <param name="bruto">O campo como veio.</param>
    public static (SituacaoDoChassiNaOrigem Situacao, Chassi? Chassi) ClassificarChassi(string? bruto)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return (SituacaoDoChassiNaOrigem.Vazio, null);

        if (Chassi.TentarCriar(bruto, out var chassi)) return (SituacaoDoChassiNaOrigem.Valido, chassi);

        var semEspaco = new string([.. bruto.Where(c => !char.IsWhiteSpace(c))]).ToUpperInvariant();

        var pedacosValidos = bruto
            .ToUpperInvariant()
            .Split((char[])[' ', '/', ';', ',', '-', '\n', '\r', '\t', '|', '.', '+'], StringSplitOptions.RemoveEmptyEntries)
            .Count(p => Chassi.TentarCriar(p, out _));

        var emBlocosDe17 = semEspaco.Length >= 34 && semEspaco.Length % 17 == 0
                           && Enumerable.Range(0, semEspaco.Length / 17).All(i => Chassi.TentarCriar(semEspaco.Substring(i * 17, 17), out _));

        if (pedacosValidos >= 2 || emBlocosDe17) return (SituacaoDoChassiNaOrigem.Multiplo, null);

        return semEspaco.Length < 17
            ? (SituacaoDoChassiNaOrigem.Incompleto, null)
            : (SituacaoDoChassiNaOrigem.ForaDoPadrao, null);
    }

    /// <summary>
    /// O NÚMERO DE SÉRIE CURTO CONFIRMADO PELO PROTHEUS (decisão 2 de 24/09/2026).
    ///
    /// <para>Chassi com menos de 17 posições pode ser o número de série de um implemento ou de uma máquina antiga — ou
    /// um chassi cortado. O que separa os dois é o cadastro de veículos do Protheus: quando ele tem EXATAMENTE aquele
    /// identificador (normalizado do mesmo jeito, uma máquina só, que não é componente), o identificador é a
    /// identidade da máquina, e o registro deixa de estar pendente por chassi. O antes e o depois medidos estão no
    /// documento 48, seção 5.5.</para>
    ///
    /// <para>O que não confirma continua como estava — pendente, com o motivo. Nada é completado nem adivinhado.</para>
    /// </summary>
    /// <param name="venda">O registro saneado.</param>
    /// <param name="parque">O cadastro de veículos do Protheus.</param>
    public static VendaDoArtSaneada ConfirmarIdentificadorCurto(VendaDoArtSaneada venda, ParqueNoProtheus parque)
    {
        if (venda.SituacaoDoChassi != SituacaoDoChassiNaOrigem.Incompleto) return venda;

        var normalizado = Chassi.Normalizar(venda.ChassiNaOrigem);
        if (!parque.TentarAchar(normalizado, out var maquina)) return venda;

        var motivos = venda.Motivos.Where(m => m != MotivoDePendenciaDoArt.ChassiIncompleto).ToList();

        if (RegrasDoParque.EhComponente(maquina))
            return venda with { Motivos = [.. motivos, MotivoDePendenciaDoArt.IdentificadorDeComponente] };

        if (!Chassi.TentarCriarIdentificadorConfirmado(normalizado, out var chassi)) return venda;

        return venda with
        {
            SituacaoDoChassi = SituacaoDoChassiNaOrigem.ConfirmadoPeloProtheus,
            Chassi = chassi,
            Motivos = motivos,
            Transformacoes =
            [
                .. venda.Transformacoes,
                $"chassi: número de série de {chassi.Numero.Length} posições confirmado no cadastro de veículos do Protheus (VV1)"
            ]
        };
    }

    /// <summary>
    /// A data do ART. Zerada ou inválida sai vazia, com a transformação registrada — nunca uma data
    /// substituta.
    /// </summary>
    /// <param name="campo">O nome do campo, para a trilha.</param>
    /// <param name="bruto">A data como texto (<c>yyyy-MM-dd</c>).</param>
    /// <param name="transformacoes">Onde registrar.</param>
    public static DateOnly? Data(string campo, string? bruto, List<string> transformacoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        var texto = bruto.Trim();
        if (texto.StartsWith("0000", StringComparison.Ordinal))
        {
            transformacoes.Add($"{campo}: zerada na origem, ficou vazia");
            return null;
        }

        if (texto.Length >= 10
            && DateOnly.TryParseExact(texto[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
        {
            if (data.Year is >= 1990 and <= 2100) return data;
            transformacoes.Add($"{campo}: ano {data.Year} fora da faixa aceita, ficou vazia");
            return null;
        }

        transformacoes.Add($"{campo}: data inválida na origem, ficou vazia");
        return null;
    }

    private static DateTime? DataHora(string campo, string? bruto, List<string> transformacoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        var texto = bruto.Trim();
        if (DateTime.TryParseExact(texto, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var instante)
            && instante.Year is >= 1990 and <= 2100)
            return instante;

        var soData = Data(campo, texto, transformacoes);
        return soData?.ToDateTime(TimeOnly.MinValue);
    }

    private static string? Texto(string campo, string? bruto, int tamanhoMaximo, List<string> transformacoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        var texto = bruto.Trim();
        if (texto.Length <= tamanhoMaximo) return texto;

        transformacoes.Add($"{campo}: {texto.Length} caracteres, acima dos {tamanhoMaximo} da coluna — omitido, não cortado");
        return null;
    }

    private static bool SimOuNao(string campo, string? bruto, List<string> transformacoes)
    {
        var codigo = ClassificacaoDoArt.Codificar(bruto ?? string.Empty);
        switch (codigo)
        {
            case "SIM": return true;
            case "NAO": case "SEM_CODIGO": return false;
            default:
                transformacoes.Add($"{campo}: valor não reconhecido na origem, lido como Não");
                return false;
        }
    }

    /// <summary>
    /// O resumo do conteúdo lido — é o que a recarga compara para saber se o registro mudou. Entra
    /// tudo o que foi lido, e nada mais: o resumo não é reversível para um documento.
    /// </summary>
    private static string Resumir(RegistroDoArt r)
    {
        var conteudo = string.Join('',
            r.Codigo, r.Chassis, r.CpfCnpj, r.Cliente, r.Linha, r.Produto, r.Empresa, r.Unidade, r.UnidadeDoFaturamento,
            r.DataDaVenda, r.DataDoFaturamento, r.Entrega, r.AbertaEm, r.Situacao, r.NumeroDoPedido, r.NumeroDaNotaFiscal,
            r.Gestao, r.VendaDireta, r.RepasseDireto, r.Quantidade);

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(conteudo)));
    }
}
