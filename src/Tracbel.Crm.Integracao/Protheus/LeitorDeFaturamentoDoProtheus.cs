using System.Globalization;
using System.Text.Json;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// O FATURAMENTO LIDO DA ORIGEM — e não da cópia que morreu.
///
/// ---------------------------------------------------------------------------------------------
/// POR QUE ESTE LEITOR SUBSTITUI O DO VÓRTICE. O CRM operou meses com "o faturamento parou em
/// 11/04/2025". A frase vinha de <c>X_TOTVS_CRM_FATURAMENTO</c>, a tabela que o Vórtice
/// <b>recebe</b> do Protheus — e essa de fato para naquela data. Medido na origem em 06/09/2026:
/// a <c>SD2</c> tem nota emitida na mesma semana. <b>O que morreu foi a integração, não o
/// faturamento.</b>
///
/// Números do recorte de 2026, medidos: <b>54.158 itens de nota</b>, <b>R$ 263.369.076,97</b>,
/// dos quais <b>68,2% em máquinas</b>.
///
/// ---------------------------------------------------------------------------------------------
/// AS DUAS TABELAS, E POR QUE SÃO DUAS.
///
/// <c>SD2</c> é o item da nota fiscal de saída: filial, emissão, produto, grupo, cliente, valor.
/// Ela identifica o cliente por <b>código do Protheus</b> (<c>D2_CLIENTE</c> + <c>D2_LOJA</c>), e
/// o CRM identifica por CPF/CNPJ. Quem traduz é a <c>SA1</c>, o cadastro de clientes, onde
/// <c>A1_CGC</c> guarda o documento.
///
/// <para><b>A loja faz parte da chave, e não é detalhe.</b> Medido: o cliente <c>008096049</c>
/// tem as lojas 0001, 0004 e 0008 com <b>três CNPJs diferentes</b> — matriz e filiais do mesmo
/// grupo. Casar só por código atribuiria a nota da filial ao CNPJ da matriz.</para>
///
/// ---------------------------------------------------------------------------------------------
/// MÁQUINA OU PEÇA sai de <c>D2_GRUPO</c>, que viaja <b>na própria linha da nota</b> — não é
/// preciso juntar com a <c>SB1</c>, que tem 517 mil linhas. O catálogo dos grupos é a
/// <c>SBM</c>: <c>VEIC</c> é máquina, a faixa <c>1001..10xx</c> é peça por linha de produto,
/// <c>SRV</c> é serviço, <c>MO_O</c> é mão de obra de oficina.
/// </summary>
/// <param name="ponte">A ponte de leitura do Protheus.</param>
public sealed class LeitorDeFaturamentoDoProtheus(PonteDoProtheus ponte)
{
    /// <summary>Os campos da nota que o CRM usa. Nenhum a mais: cada um custa banda.</summary>
    private const string CamposDaNota =
        "D2_FILIAL,D2_EMISSAO,D2_CLIENTE,D2_LOJA,D2_GRUPO,D2_TOTAL,D2_DOC,D2_CF,D2_TIPO";

    /// <summary>
    /// OS CFOPs QUE SÃO RECEITA DE VENDA. Tudo o que não estiver aqui fica de fora.
    ///
    /// <para><b>Por que isto existe.</b> A SD2 guarda toda saída de mercadoria, e venda é só uma
    /// parte delas. Medido sobre 2024–2026: de R$ 1,27 bilhão que saiu pela porta, <b>menos de
    /// dois terços é venda</b>. O resto é transferência entre filiais, retorno de conserto,
    /// remessa para demonstração e baixa de estoque. Sem este filtro o painel da diretoria
    /// mostraria <b>meio bilhão de reais de faturamento que não existe</b>.</para>
    ///
    /// <para><b>É lista de inclusão, e não de exclusão</b>, de propósito. Com lista de exclusão,
    /// um CFOP novo que ninguém classificou entraria como venda e inflaria o número em silêncio.
    /// Com lista de inclusão ele fica de fora e o total de excluídos aparece no relatório da
    /// carga — o erro passa a ser para menos, e visível. Entre subestimar de forma visível e
    /// superestimar em silêncio, este projeto escolhe o primeiro.</para>
    ///
    /// <para>O que está aqui: venda de mercadoria (x102, x108, x117), venda com substituição
    /// tributária (x403, x405), venda de combustível e lubrificante (x656, x659) e prestação de
    /// serviço (x933). O primeiro dígito é 5 dentro do estado e 6 fora — os dois entram.</para>
    ///
    /// <para><b>A varredura completa, medida em 06/09/2026 sobre as 205.169 linhas emitidas desde
    /// 01/01/2024.</b> O que a lista aceita, por ordem de valor: 5102 R$ 552,1 mi (42.804 itens),
    /// 5405 R$ 117,1 mi (97.439 — é peça, muito item e pouco valor), 6933 R$ 63,2 mi, 5933
    /// R$ 45,3 mi, 6102 R$ 32,0 mi, 5656 R$ 7,0 mi, e o resto abaixo de R$ 3 mi. Total aceito:
    /// <b>R$ 821,1 mi</b>.</para>
    ///
    /// <para>O que a lista recusa, e por quê: 5152 transferência entre filiais R$ 176,2 mi; 5916
    /// retorno de conserto R$ 172,7 mi (é máquina que entrou para reparo e voltou — nunca foi
    /// vendida); 5912 e 5914 remessa para demonstração e feira R$ 105,4 mi; 5409 transferência com
    /// ST R$ 30,1 mi; 5927 baixa de estoque R$ 8,8 mi; 5910 bonificação e brinde R$ 6,0 mi. Somam
    /// mais de <b>R$ 500 milhões que não são receita</b> e que, sem este filtro, iriam para o
    /// painel da diretoria como se fossem.</para>
    ///
    /// <para><b>O 5949 é uma pergunta em aberto, não uma decisão.</b> "Outra saída não
    /// especificada": 5.221 itens, R$ 38,1 mi. É o único CFOP relevante que a lista deixa de fora
    /// sem saber o que ele é. Fica fora pela mesma regra do parágrafo acima — na dúvida, para
    /// menos e visível —, e o valor sai no relatório da carga. <b>Precisa da resposta do fiscal
    /// antes de virar decisão definitiva.</b></para>
    /// </summary>
    private static readonly HashSet<string> CfopsDeVenda = new(StringComparer.Ordinal)
    {
        "5102", "6102", "5108", "6108", "5117", "6117",
        "5403", "6403", "5405", "6405",
        "5656", "6656", "5659", "6659",
        "5933", "6933"
    };

    /// <summary>
    /// O tipo de nota que é venda.
    ///
    /// <para><c>N</c> é nota normal. Medido em 2026: <c>B</c> (beneficiamento e devolução de
    /// compra) somou R$ 39 milhões em 107 itens, e <c>D</c> é devolução. Nenhum dos dois é
    /// receita de venda, e o valor alto do <c>B</c> mostra o estrago que ele faria no total.</para>
    /// </summary>
    private const string TipoDeVenda = "N";

    /// <summary>Os campos do cadastro de cliente que traduzem o código para documento e nome.</summary>
    private const string CamposDoCliente = "A1_COD,A1_LOJA,A1_CGC,A1_NOME";

    /// <summary>
    /// Máquina, peça ou serviço — pelo grupo de produto que viaja na linha da nota.
    ///
    /// <para>A pergunta do negócio era "qual das SB separa máquina de peça". A resposta medida é
    /// que <b>nenhuma separa</b>: quem separa é o grupo, catálogo <c>SBM</c> (273 grupos), e ele
    /// está no próprio <c>D2_GRUPO</c> da nota. Isso torna a quebra barata — não é preciso juntar
    /// com a <c>SB1</c>, que tem 517.855 linhas.</para>
    ///
    /// <para><c>VEIC</c> é máquina. A faixa <c>1001</c> a <c>10xx</c> é peça, uma por linha de
    /// produto (colhedora de cana, trator, colheitadeira de grãos, plantadeira, plataforma, gator,
    /// pulverizador, implemento). <c>SRV</c>, <c>MO_O</c> e <c>MO_T</c> são serviço e mão de obra.
    /// <c>AMS</c>, <c>FR</c> e <c>JD</c> existem e não se encaixam em nenhum dos três — vão para
    /// "outros", <b>que é o que impede a quebra de fechar com menos que o total</b>.</para>
    /// </summary>
    /// <param name="grupo">O <c>D2_GRUPO</c> da linha.</param>
    private static GrupoDaNota Classificar(string grupo) => grupo switch
    {
        "VEIC" => GrupoDaNota.Maquina,
        "SRV" or "MO_O" or "MO_T" => GrupoDaNota.Servico,
        // A FAIXA DE PEÇA É NUMÉRICA e cresce: o negócio abre um grupo novo a cada linha de
        // produto que entra. Testar o formato em vez de listar os códigos é o que faz o grupo
        // 1042 de amanhã já entrar como peça, em vez de cair em "outros" sem ninguém notar.
        _ when grupo.Length == 4 && grupo.All(char.IsAsciiDigit) && grupo[0] == '1' => GrupoDaNota.Peca,
        _ => GrupoDaNota.Outros
    };

    /// <summary>O que a linha da nota vendeu, na divisão que o negócio usa.</summary>
    private enum GrupoDaNota
    {
        /// <summary>Grupo <c>VEIC</c>.</summary>
        Maquina,

        /// <summary>A faixa numérica <c>1001..10xx</c>.</summary>
        Peca,

        /// <summary><c>SRV</c> e mão de obra.</summary>
        Servico,

        /// <summary>Grupo que ainda não sabemos ler.</summary>
        Outros
    }

    /// <summary>
    /// Lê o faturamento a partir de uma data, agregado por cliente, filial e mês.
    /// </summary>
    /// <param name="desde">A primeira emissão a considerar.</param>
    /// <param name="filiais">
    /// As filiais do Protheus a ler, uma a uma.
    ///
    /// <para><b>Sem esta lista a leitura traz UMA filial, e não a empresa.</b> Ver
    /// <see cref="PonteDoProtheus.LerPaginaAsync"/>: o AppServer responde pela filial padrão do nó
    /// quando o <c>tenantId</c> não vai no cabeçalho, e esse padrão muda sozinho. Medido em
    /// 08/09/2026, as dezesseis filiais somam <b>1.340.505</b> itens de nota desde setembro de
    /// 2023 — seis vezes o que uma leitura sem filial trazia.</para>
    /// </param>
    /// <param name="relatar">Para dizer o que está acontecendo numa leitura de minutos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LoteDoProtheus>> LerAsync(
        DateOnly desde,
        IReadOnlyList<string> filiais,
        Action<string> relatar,
        CancellationToken ct)
    {
        if (filiais.Count == 0)
            return Resultado<LoteDoProtheus>.Indisponivel(
                "Nenhuma filial do Protheus foi informada. Ler sem filial devolveria os números de " +
                "UMA filial escolhida pelo servidor, apresentados como se fossem os da empresa.");

        relatar("  lendo o cadastro de clientes do Protheus (SA1), para traduzir código em CNPJ…");

        // A SA1 É COMPARTILHADA ENTRE AS FILIAIS — medido: `A1_FILIAL` vem vazio e o total é o
        // mesmo (38.682) em 010101, 010107 e 010113. Lê-la uma vez por filial seria multiplicar por
        // dezesseis uma leitura idêntica. A SD2, essa sim, é por filial.
        var clientes = await ponte.LerTudoAsync(
            "SA1", CamposDoCliente, null,
            (lidos, total) => { if (lidos % 10_000 == 0) relatar($"    SA1: {lidos:N0} de {total:N0}"); },
            null,
            ct);

        if (!clientes.EhSucesso) return Resultado<LoteDoProtheus>.Indisponivel(clientes.Erro!);

        // A CHAVE É CÓDIGO + LOJA. Ver o comentário da classe: o mesmo código com lojas
        // diferentes tem CNPJs diferentes.
        var documentoPorCliente = new Dictionary<string, string>(StringComparer.Ordinal);
        var nomePorDocumento = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var linha in clientes.Valor)
        {
            var documento = SoDigitos(Texto(linha, "a1_cgc"));
            if (documento.Length is not (11 or 14)) continue;

            documentoPorCliente[$"{Texto(linha, "a1_cod")}/{Texto(linha, "a1_loja")}"] = documento;

            // O MESMO CNPJ APARECE EM VÁRIAS LOJAS com o nome escrito de jeitos diferentes. Fica
            // o primeiro: qualquer um serve para uma pessoa reconhecer de quem se trata, e
            // trocá-lo a cada linha só faria o nome mudar de carga para carga sem motivo.
            nomePorDocumento.TryAdd(documento, Texto(linha, "a1_nome"));
        }

        relatar($"    {documentoPorCliente.Count:N0} clientes com documento válido.");
        relatar(
            $"  lendo as notas emitidas desde {desde:dd/MM/yyyy} (SD2), " +
            $"filial a filial — {filiais.Count} filiais…");

        var porChave = new Dictionary<(string Documento, string Filial, DateOnly Mes), Acumulado>();
        var semCliente = new HashSet<string>(StringComparer.Ordinal);
        var semData = 0;
        var naoEhVenda = 0;
        var valorNaoEhVenda = 0m;
        var itensLidos = 0;
        DateOnly? maisRecente = null;

        foreach (var filial in filiais)
        {
            var daFilial = await ponte.LerTudoAsync(
                "SD2", CamposDaNota, $"D2_EMISSAO >= '{desde:yyyyMMdd}'",
                (lidos, total) =>
                {
                    if (lidos % 20_000 == 0) relatar($"    SD2 {filial}: {lidos:N0} de {total:N0}");
                },
                filial,
                ct);

            // UMA FILIAL QUE FALHA INTERROMPE TUDO, de propósito. Seguir em frente gravaria um
            // total parcial com cara de completo — e o painel da diretoria mostraria queda de
            // faturamento onde houve falha de leitura.
            if (!daFilial.EhSucesso) return Resultado<LoteDoProtheus>.Indisponivel(daFilial.Erro!);

            itensLidos += daFilial.Valor.Count;
            relatar($"    filial {filial}: {daFilial.Valor.Count:N0} itens.");

            AcumularNotas(
                daFilial.Valor, documentoPorCliente, porChave, semCliente,
                ref semData, ref naoEhVenda, ref valorNaoEhVenda, ref maisRecente);
        }

        return Montar(
            porChave, nomePorDocumento, relatar,
            itensLidos, semCliente, semData, naoEhVenda, valorNaoEhVenda, maisRecente);
    }

    /// <summary>
    /// Soma as linhas de uma filial no acumulado que atravessa todas elas.
    ///
    /// <para>O acumulado é comum às dezesseis porque a chave é (documento, filial, mês): o mesmo
    /// cliente compra em mais de uma praça, e cada praça é uma linha própria — que é justamente o
    /// que permite a curva ABC ser por filial.</para>
    /// </summary>
    private static void AcumularNotas(
        IReadOnlyList<Dictionary<string, JsonElement>> notas,
        Dictionary<string, string> documentoPorCliente,
        Dictionary<(string Documento, string Filial, DateOnly Mes), Acumulado> porChave,
        HashSet<string> semCliente,
        ref int semData,
        ref int naoEhVenda,
        ref decimal valorNaoEhVenda,
        ref DateOnly? maisRecente)
    {
        foreach (var linha in notas)
        {
            var emissao = Data(Texto(linha, "d2_emissao"));
            if (emissao is null) { semData++; continue; }

            if (maisRecente is null || emissao > maisRecente) maisRecente = emissao;

            var chaveDoCliente = $"{Texto(linha, "d2_cliente")}/{Texto(linha, "d2_loja")}";
            if (!documentoPorCliente.TryGetValue(chaveDoCliente, out var documento))
            {
                semCliente.Add(chaveDoCliente);
                continue;
            }

            // SÓ VENDA ENTRA. O CFOP e o tipo da nota são a evidência FISCAL do que a saída
            // é — e são muito melhores que qualquer heurística de nome ou de grupo de produto.
            if (Texto(linha, "d2_tipo") != TipoDeVenda || !CfopsDeVenda.Contains(Texto(linha, "d2_cf")))
            {
                naoEhVenda++;
                valorNaoEhVenda += Decimal(Texto(linha, "d2_total"));
                continue;
            }

            var mes = new DateOnly(emissao.Value.Year, emissao.Value.Month, 1);
            var chave = (documento, Texto(linha, "d2_filial"), mes);

            if (!porChave.TryGetValue(chave, out var acumulado))
                acumulado = porChave[chave] = new Acumulado();

            var valor = Decimal(Texto(linha, "d2_total"));
            acumulado.Valor += valor;
            acumulado.Itens++;
            acumulado.Notas.Add(Texto(linha, "d2_doc"));

            switch (Classificar(Texto(linha, "d2_grupo")))
            {
                case GrupoDaNota.Maquina: acumulado.Maquina += valor; break;
                case GrupoDaNota.Peca: acumulado.Peca += valor; break;
                case GrupoDaNota.Servico: acumulado.Servico += valor; break;
                default: acumulado.Outros += valor; break;
            }
        }
    }

    /// <summary>
    /// Monta o lote a partir do acumulado das dezesseis filiais.
    /// </summary>
    private static Resultado<LoteDoProtheus> Montar(
        Dictionary<(string Documento, string Filial, DateOnly Mes), Acumulado> porChave,
        Dictionary<string, string> nomePorDocumento,
        Action<string> relatar,
        int itensLidos,
        HashSet<string> semCliente,
        int semData,
        int naoEhVenda,
        decimal valorNaoEhVenda,
        DateOnly? maisRecente)
    {
        var agregado = porChave
            // VALOR ZERO OU NEGATIVO NÃO ENTRA. A SD2 guarda devolução e ajuste como linha
            // própria, e um mês que fecha em zero para um cliente não é faturamento — é o
            // encontro de contas de uma venda com a devolução dela.
            .Where(p => p.Value.Valor > 0)
            .Select(p => new FaturamentoParaCarga(
                ChaveDeOrigem: $"{p.Key.Documento}/{p.Key.Filial}/{p.Key.Mes:yyyy-MM}",
                DocumentoDoCliente: p.Key.Documento,
                // A FILIAL VEM COMO CÓDIGO DE SEIS DÍGITOS ('010101'), que é exatamente o código
                // de `organizacao.Empresa` — melhor que o `NroEmpresa` do Vórtice, que exigia
                // de-para. O campo numérico do registro fica em zero e o código vai no lugar dele.
                CodigoDaFilialNoLegado: 0,
                CodigoDaFilial: p.Key.Filial,
                Competencia: p.Key.Mes,
                ValorLiquido: decimal.Round(p.Value.Valor, 2),
                Notas: p.Value.Notas.Count,
                Itens: p.Value.Itens,
                NomeNaOrigem: nomePorDocumento.GetValueOrDefault(p.Key.Documento, string.Empty),
                Quebra: p.Value.Quebrar()))
            .ToList();

        relatar(
            $"    {itensLidos:N0} itens de nota lidos · {naoEhVenda:N0} descartados por não " +
            $"serem venda (R$ {valorNaoEhVenda:N2} em transferência, remessa e devolução).");
        relatar(
            $"    {agregado.Count:N0} meses de faturamento por cliente · " +
            $"{semCliente.Count:N0} código(s) de cliente sem cadastro.");

        return Resultado<LoteDoProtheus>.Ok(new LoteDoProtheus(
            agregado,
            [.. porChave.Keys.Select(k => k.Filial).Distinct()],
            itensLidos,
            semCliente.Count,
            semData,
            naoEhVenda,
            decimal.Round(valorNaoEhVenda, 2),
            maisRecente));
    }

    private sealed class Acumulado
    {
        public decimal Valor;
        public decimal Maquina;
        public decimal Peca;
        public decimal Servico;
        public decimal Outros;
        public int Itens;
        public readonly HashSet<string> Notas = new(StringComparer.Ordinal);

        /// <summary>
        /// A quebra do mês, arredondada de forma a fechar com o total.
        ///
        /// <para>As três primeiras parcelas são arredondadas e a última recebe a diferença. Somar
        /// quatro valores arredondados independentemente erra até dois centavos contra o total —
        /// o bastante para a restrição do banco recusar a linha inteira.</para>
        /// </summary>
        public QuebraDoFaturamento Quebrar()
        {
            var maquina = decimal.Round(Maquina, 2);
            var peca = decimal.Round(Peca, 2);
            var servico = decimal.Round(Servico, 2);

            return new QuebraDoFaturamento(
                maquina, peca, servico,
                decimal.Round(Valor, 2) - maquina - peca - servico);
        }
    }

    private static string Texto(Dictionary<string, JsonElement> linha, string campo) =>
        linha.TryGetValue(campo, out var valor)
            ? (valor.ValueKind == JsonValueKind.String ? valor.GetString() ?? string.Empty : valor.ToString()).Trim()
            : string.Empty;

    private static string SoDigitos(string texto) =>
        new([.. texto.Where(char.IsAsciiDigit)]);

    /// <summary>
    /// A data como o Protheus a devolve.
    ///
    /// <para>Ele alterna entre <c>yyyy-MM-dd</c> e <c>yyyyMMdd</c> conforme o campo, e uma data
    /// não reconhecida vira nulo — nunca <c>DateTime.Today</c>, que jogaria a nota no mês errado
    /// em silêncio.</para>
    /// </summary>
    private static DateOnly? Data(string bruto)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        return DateOnly.TryParseExact(bruto, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                   DateTimeStyles.None, out var comTraco)
            ? comTraco
            : DateOnly.TryParseExact(bruto, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var semTraco)
                ? semTraco
                : null;
    }

    /// <summary>
    /// O valor como o Protheus o devolve: texto, com PONTO decimal.
    ///
    /// <para>Ler com a cultura da máquina faria "674.45" virar 67.445 numa estação em pt-BR — um
    /// erro de cem vezes que nenhum total denunciaria.</para>
    /// </summary>
    private static decimal Decimal(string bruto) =>
        decimal.TryParse(bruto, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : 0m;
}

/// <summary>
/// O que uma leitura de faturamento do Protheus produziu.
/// </summary>
/// <param name="Faturamento">O agregado por cliente, filial e mês.</param>
/// <param name="FiliaisVistas">Os códigos de filial encontrados nas notas.</param>
/// <param name="ItensLidos">Quantos itens de nota a origem devolveu.</param>
/// <param name="ClientesSemCadastro">Códigos de cliente da nota que não estão na SA1.</param>
/// <param name="ItensSemData">Itens descartados por não ter data de emissão legível.</param>
/// <param name="ItensQueNaoSaoVenda">Itens de saída que não são venda — transferência, remessa,
/// devolução, baixa de estoque.</param>
/// <param name="ValorQueNaoEhVenda">Quanto esses itens somam. Vai para o relatório da carga
/// porque é grande: em 2026 foram R$ 135 milhões, mais da metade do que sai pela porta.</param>
/// <param name="EmissaoMaisRecente">A nota mais nova vista. Vira a marca de sincronismo.</param>
public sealed record LoteDoProtheus(
    IReadOnlyList<FaturamentoParaCarga> Faturamento,
    IReadOnlyList<string> FiliaisVistas,
    int ItensLidos,
    int ClientesSemCadastro,
    int ItensSemData,
    int ItensQueNaoSaoVenda,
    decimal ValorQueNaoEhVenda,
    DateOnly? EmissaoMaisRecente);
