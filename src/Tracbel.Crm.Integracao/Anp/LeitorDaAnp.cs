using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace Tracbel.Crm.Integracao.Anp;

/// <summary>
/// UMA USINA DE ETANOL AUTORIZADA PELA ANP, num mês — como o arquivo de dados abertos a escreve.
///
/// <para><b>O município vem em caixa alta e sem acento</b> (<c>MONTE APRAZIVEL</c>), exatamente como
/// nas planilhas do comercial. Quem reconhece o município oficial é o saneamento da carga, pela mesma
/// chave de colação — o leitor não adivinha.</para>
/// </summary>
/// <param name="MesDeReferencia">O mês da autorização, como <c>MM/aaaa</c>.</param>
/// <param name="RazaoSocial">A razão social do estabelecimento.</param>
/// <param name="Cnpj">O CNPJ do estabelecimento, com 14 dígitos e sem pontuação.</param>
/// <param name="Uf">O nome do estado por extenso, como a ANP escreve.</param>
/// <param name="Municipio">O nome do município, em caixa alta e sem acento.</param>
/// <param name="CapacidadeDeAnidroBruta">Capacidade de produção de etanol anidro, em m³/dia.</param>
/// <param name="CapacidadeDeHidratadoBruta">Capacidade de produção de etanol hidratado, em m³/dia.</param>
public sealed record LinhaDaUsinaDeEtanol(
    string MesDeReferencia,
    string RazaoSocial,
    string Cnpj,
    string Uf,
    string Municipio,
    string? CapacidadeDeAnidroBruta,
    string? CapacidadeDeHidratadoBruta);

/// <summary>
/// A LEITURA DOS DADOS ABERTOS DA ANP — as usinas de etanol autorizadas, por município.
///
/// <para><b>Por que a ANP, e não o cadastro do MAPA.</b> A issue 65 pedia a lista de municípios com
/// usina, que na pasta do comercial é um arquivo de 68 linhas, sem fonte nem data. O cadastro
/// equivalente do MAPA (SAPCana) existe e é diário, mas o download dele **exige CAPTCHA** — medido em
/// 20/09/2026 —, e contornar um controle de acesso deliberado não é caminho. A ANP publica a base do
/// painel de produtores de etanol como dado aberto, sem barreira: 145 usinas em 120 municípios de São
/// Paulo no mês 07/2026, com razão social, CNPJ e <b>capacidade de produção</b>. É mais do que a
/// planilha tinha, e tem procedência.</para>
///
/// <para><b>O que esta fonte NÃO enxerga:</b> usina que produz só açúcar, sem etanol, não é
/// autorizada pela ANP e não aparece aqui. Na prática quase toda usina paulista é mista, mas a
/// ausência de um município nesta lista não prova ausência de usina — só ausência de usina de
/// etanol.</para>
///
/// <para><b>O arquivo é um ZIP com quatro CSVs</b>, e o que interessa é o de capacidade, que tem a
/// série mensal. O de tancagem, publicado em outra página, está parado em 08/2023 e não serve.</para>
///
/// <para><b>Só leitura, só GET, só dado público.</b> Nenhuma credencial é usada nem existe.</para>
/// </summary>
/// <param name="http">O cliente HTTP.</param>
public sealed class LeitorDaAnp(HttpClient http)
{
    /// <summary>
    /// A base do Painel Dinâmico de Produtores de Etanol, em ZIP.
    ///
    /// <para>Atualizada mensalmente pela ANP; a versão lida em 20/09/2026 era de 18/08/2026 e
    /// terminava no mês 07/2026.</para>
    /// </summary>
    public const string EnderecoDaBaseDeEtanol =
        "https://www.gov.br/anp/pt-br/assuntos/producao-e-fornecimento-de-biocombustiveis/etanol/arquivos-etanol/pb-da-etanol.zip";

    /// <summary>O CSV de dentro do ZIP que traz a série mensal de capacidade por estabelecimento.</summary>
    public const string PrefixoDoArquivoDeCapacidade = "Etanol_Capacidade";

    /// <summary>O nome do estado de São Paulo, como a ANP o escreve na coluna <c>Estado</c>.</summary>
    public const string SaoPauloNaAnp = "São Paulo";

    /// <summary>
    /// As usinas de etanol de uma UF no mês mais recente publicado.
    ///
    /// <para><b>Só o mês mais recente.</b> A série mensal desde 2021 existe no arquivo, e o CRM não
    /// tem pergunta que a use: o que a Visão 360 precisa saber é quais usinas estão autorizadas
    /// HOJE e de que tamanho. Guardar 67 meses de 145 usinas seria 9.715 linhas para responder uma
    /// pergunta que 145 respondem.</para>
    /// </summary>
    /// <param name="uf">O nome do estado, como a ANP escreve. Ver <see cref="SaoPauloNaAnp"/>.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<LinhaDaUsinaDeEtanol>> LerUsinasDeEtanolAsync(
        string uf, CancellationToken ct)
    {
        var todas = await LerCapacidadeAsync(ct);

        var daUf = todas.Where(l => string.Equals(l.Uf, uf, StringComparison.OrdinalIgnoreCase)).ToList();
        if (daUf.Count == 0) return [];

        var maisRecente = daUf.Max(l => ComoData(l.MesDeReferencia));

        return [.. daUf.Where(l => ComoData(l.MesDeReferencia) == maisRecente)];
    }

    /// <summary>
    /// O mês <c>MM/aaaa</c> como data, para ordenar.
    ///
    /// <para><b>Comparar o texto não funciona:</b> "07/2026" vem antes de "12/2021" em ordem
    /// alfabética, e a carga escolheria o mês errado como "o mais recente".</para>
    /// </summary>
    /// <param name="mesDeReferencia">O mês no formato da ANP.</param>
    public static DateOnly ComoData(string mesDeReferencia) =>
        DateOnly.ParseExact(mesDeReferencia.Trim(), "MM/yyyy", CultureInfo.InvariantCulture);

    /// <summary>
    /// Baixa o ZIP da ANP e lê o CSV de capacidade inteiro, sem filtrar por UF.
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    /// <exception cref="InvalidDataException">Quando o ZIP não traz o CSV de capacidade.</exception>
    private async Task<IReadOnlyList<LinhaDaUsinaDeEtanol>> LerCapacidadeAsync(CancellationToken ct)
    {
        // O ZIP É LIDO DA MEMÓRIA, e não de um arquivo temporário: são menos de 1 MB, e um arquivo em
        // disco na máquina que roda a carga seria mais uma coisa para alguém limpar depois.
        var bytes = await http.GetByteArrayAsync(EnderecoDaBaseDeEtanol, ct);
        using var pacote = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);

        // MAIS DE UM ARQUIVO DE CAPACIDADE: a ANP separa a série em "2018-2020" e "2021-2026". Os dois
        // têm o mesmo cabeçalho, e o leitor junta — quem escolhe o mês é quem chama.
        var arquivos = pacote.Entries
            .Where(e => e.Name.StartsWith(PrefixoDoArquivoDeCapacidade, StringComparison.OrdinalIgnoreCase)
                        && e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (arquivos.Count == 0)
            throw new InvalidDataException(
                $"O pacote da ANP não traz nenhum CSV começando com \"{PrefixoDoArquivoDeCapacidade}\". " +
                $"Encontrados: {string.Join(", ", pacote.Entries.Select(e => e.Name))}.");

        var linhas = new List<LinhaDaUsinaDeEtanol>();

        foreach (var arquivo in arquivos)
        {
            await using var conteudo = arquivo.Open();

            // UTF-8 COM BOM. O arquivo abre com o marcador de ordem de bytes, e lê-lo como ISO-8859-1
            // deixaria "São Paulo" ilegível — e o filtro por UF, que compara com o nome por extenso,
            // não acharia nada.
            using var leitor = new StreamReader(conteudo, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);

            var cabecalho = await leitor.ReadLineAsync(ct);
            if (cabecalho is null) continue;

            var colunas = SepararCsv(cabecalho);
            int Coluna(string parte) => colunas.FindIndex(c => c.Contains(parte, StringComparison.OrdinalIgnoreCase));

            var iMes = Coluna("Mês/Ano");
            var iRazao = Coluna("Razão Social");
            var iCnpj = Coluna("CNPJ");
            var iUf = Coluna("Estado");
            var iMunicipio = Coluna("Município");
            var iAnidro = Coluna("Anidro");
            var iHidratado = Coluna("Hidratado");

            if (iMes < 0 || iRazao < 0 || iCnpj < 0 || iUf < 0 || iMunicipio < 0)
                throw new InvalidDataException(
                    $"O CSV \"{arquivo.Name}\" da ANP não tem as colunas esperadas. Cabeçalho lido: {cabecalho}");

            while (await leitor.ReadLineAsync(ct) is { } linha)
            {
                if (string.IsNullOrWhiteSpace(linha)) continue;

                var campos = SepararCsv(linha);
                if (campos.Count <= iMunicipio) continue;

                string? Campo(int i) => i >= 0 && i < campos.Count && campos[i].Length > 0 ? campos[i] : null;

                linhas.Add(new LinhaDaUsinaDeEtanol(
                    campos[iMes], campos[iRazao], campos[iCnpj], campos[iUf], campos[iMunicipio],
                    Campo(iAnidro), Campo(iHidratado)));
            }
        }

        return linhas;
    }

    /// <summary>
    /// Separa uma linha de CSV por vírgula, respeitando o campo entre aspas.
    ///
    /// <para><b>Um <c>Split(',')</c> não serve:</b> a razão social traz vírgula ("EMPRESA X, S/A"), e
    /// partir no bruto jogaria o CNPJ para a coluna do estado sem dar erro — a linha entraria errada,
    /// em silêncio, que é a pior forma de errar.</para>
    /// </summary>
    /// <param name="linha">A linha do arquivo.</param>
    public static List<string> SepararCsv(string linha)
    {
        var campos = new List<string>();
        var atual = new StringBuilder();
        var entreAspas = false;

        for (var i = 0; i < linha.Length; i++)
        {
            var c = linha[i];

            if (entreAspas && c == '"' && i + 1 < linha.Length && linha[i + 1] == '"')
            {
                // Aspa dobrada dentro de um campo entre aspas: é uma aspa literal.
                atual.Append('"');
                i++;
            }
            else if (c == '"')
            {
                entreAspas = !entreAspas;
            }
            else if (c == ',' && !entreAspas)
            {
                campos.Add(atual.ToString().Trim());
                atual.Clear();
            }
            else
            {
                atual.Append(c);
            }
        }

        campos.Add(atual.ToString().Trim());
        return campos;
    }
}
