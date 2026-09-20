using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>Um município como o IBGE o publica.</summary>
/// <param name="Codigo">O código de sete dígitos.</param>
/// <param name="Nome">O nome oficial, com acento e apóstrofo.</param>
/// <param name="Uf">A sigla da UF.</param>
public sealed record MunicipioDoIbge(int Codigo, string Nome, string Uf);

/// <summary>Uma linha do nosso catálogo de municípios, como a carga a lê.</summary>
/// <param name="Id">O identificador interno.</param>
/// <param name="Nome">O nome gravado hoje.</param>
/// <param name="Uf">A UF gravada.</param>
/// <param name="CodigoIbge">O código já reconhecido, quando houver.</param>
public sealed record MunicipioDoCatalogo(int Id, string Nome, string Uf, int? CodigoIbge);

/// <summary>Como a linha do catálogo foi reconhecida no IBGE — a ordem é a da confiança.</summary>
public enum FormaDeReconhecimento
{
    /// <summary>O nome normalizado é igual ao oficial.</summary>
    NomeIgual = 0,

    /// <summary>Igual depois de trocar o apóstrofo por espaço (<c>ESTRELA D OESTE</c>).</summary>
    GrafiaDoApostrofo = 1,

    /// <summary>Nome com a marca d'água de 20 caracteres, prefixo de um único município.</summary>
    NomeTruncadoNaOrigem = 2
}

/// <summary>Uma linha do catálogo reconhecida no IBGE.</summary>
/// <param name="MunicipioId">A linha do catálogo.</param>
/// <param name="NomeAnterior">O nome gravado antes do reconhecimento.</param>
/// <param name="Oficial">O município oficial.</param>
/// <param name="Forma">Como casou.</param>
public sealed record ReconhecimentoNoIbge(int MunicipioId, string NomeAnterior, MunicipioDoIbge Oficial, FormaDeReconhecimento Forma);

/// <summary>Uma linha que casaria com um município cujo código já pertence a outra linha.</summary>
/// <param name="MunicipioId">A linha que ficou sem código.</param>
/// <param name="Nome">O nome dela.</param>
/// <param name="Oficial">O município oficial com que ela casaria.</param>
/// <param name="Forma">
/// Por qual regra ela casaria — só a do apóstrofo ou a do nome cortado. É o que separa o que foi
/// autorizado a consolidar (nome cortado, documento 32, seção 4.6) do que continua pendente.
/// </param>
public sealed record VarianteDeGrafia(int MunicipioId, string Nome, MunicipioDoIbge Oficial, FormaDeReconhecimento Forma);

/// <summary>O resultado do reconhecimento do catálogo inteiro.</summary>
/// <param name="Reconhecidos">As linhas que recebem código e nome oficial.</param>
/// <param name="Variantes">As segundas grafias do mesmo município — ficam para revisão, sem fusão.</param>
/// <param name="SemCorrespondencia">As linhas que não são município do IBGE (distrito, texto de teste).</param>
/// <param name="AusentesNoCatalogo">Os municípios oficiais que nenhuma linha representa.</param>
public sealed record ResultadoDoReconhecimento(
    IReadOnlyList<ReconhecimentoNoIbge> Reconhecidos,
    IReadOnlyList<VarianteDeGrafia> Variantes,
    IReadOnlyList<MunicipioDoCatalogo> SemCorrespondencia,
    IReadOnlyList<MunicipioDoIbge> AusentesNoCatalogo);

/// <summary>
/// O SANEAMENTO DO TERRITÓRIO — as decisões da carga do documento 32, como funções puras.
///
/// <para><b>Nada aqui funde, completa ou escolhe por aproximação.</b> Casar é igualdade de texto
/// normalizado, na ordem de confiança escrita em <see cref="Reconhecer"/>; o que não casa por
/// igualdade fica contado e nomeado para revisão. É o critério do pedido: não unir nem completar
/// registro ambíguo automaticamente.</para>
/// </summary>
public static partial class SaneamentoDeTerritorio
{
    /// <summary>
    /// O tamanho em que o nome do município foi cortado na origem (documento 26, seção 4.5): 947
    /// das 10.214 linhas do catálogo do Vórtice têm exatamente 20 caracteres.
    /// </summary>
    public const int MarcaDeTruncamento = 20;

    /// <summary>
    /// A chave de comparação que PRESERVA o apóstrofo: maiúsculas, sem acento, hífen como espaço,
    /// espaços colapsados. É a que separa <c>ESTRELA D'OESTE</c> de <c>ESTRELA D OESTE</c>.
    /// </summary>
    /// <param name="nome">O nome como veio.</param>
    public static string ChaveExata(string nome)
    {
        var decomposto = nome.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        var texto = new StringBuilder(decomposto.Length);

        foreach (var caractere in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark) continue;
            texto.Append(caractere switch
            {
                '-' => ' ',
                '’' or '‘' or '`' or '´' => '\'',
                _ => caractere
            });
        }

        return Espacos().Replace(texto.ToString(), " ").Trim();
    }

    /// <summary>
    /// A chave que a colação do banco (<c>Latin1_General_CI_AI</c>) enxerga: só caixa e acento
    /// são ignorados. É com ela que a carga confere, antes de gravar, que o nome oficial não
    /// colide com o nome de OUTRA linha no índice único <c>UX_Municipio_Uf_Nome</c>.
    /// </summary>
    /// <param name="nome">O nome como veio.</param>
    public static string ChaveDaColacao(string nome)
    {
        var decomposto = nome.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
        return new string([.. decomposto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)]);
    }

    /// <summary>A mesma chave, com o apóstrofo virando espaço — a segunda tentativa de casamento.</summary>
    /// <param name="nome">O nome como veio.</param>
    public static string ChaveSemApostrofo(string nome) =>
        Espacos().Replace(ChaveExata(nome).Replace('\'', ' '), " ").Trim();

    /// <summary>
    /// Reconhece o catálogo no IBGE.
    ///
    /// <para><b>A ordem, e por que é esta.</b> (1) nome igual; (2) igual sem o apóstrofo, só se o
    /// código ainda estiver livre; (3) nome com a marca d'água de truncamento que é prefixo de UM
    /// único município da UF, com o código livre. Uma linha que casaria com um código já tomado
    /// NÃO é fundida à outra: vira <see cref="VarianteDeGrafia"/>, e os endereços que apontam para
    /// ela aparecem como "sem código IBGE" até alguém decidir.</para>
    ///
    /// <para><b>Determinístico.</b> As linhas são percorridas por <c>Id</c>; rodar duas vezes sobre o
    /// mesmo catálogo dá o mesmo resultado, e sobre o catálogo já reconhecido não reconhece nada.</para>
    /// </summary>
    /// <param name="catalogo">O nosso catálogo inteiro.</param>
    /// <param name="oficiais">A lista oficial do IBGE.</param>
    public static ResultadoDoReconhecimento Reconhecer(
        IEnumerable<MunicipioDoCatalogo> catalogo, IReadOnlyList<MunicipioDoIbge> oficiais)
    {
        var linhas = catalogo.OrderBy(m => m.Id).ToList();

        var porChaveExata = Agrupar(oficiais, o => ChaveExata(o.Nome));
        var porChaveSemApostrofo = Agrupar(oficiais, o => ChaveSemApostrofo(o.Nome));

        var tomados = linhas.Where(m => m.CodigoIbge is not null).Select(m => m.CodigoIbge!.Value).ToHashSet();
        var reconhecidos = new List<ReconhecimentoNoIbge>();
        var variantes = new List<VarianteDeGrafia>();
        var pendentes = linhas.Where(m => m.CodigoIbge is null).ToList();

        // (1) NOME IGUAL. Quem escreve o nome igual AO BANCO (caixa e acento à parte) passa na
        // frente: "ARCO-IRIS" e "ARCO IRIS" dão a mesma chave, mas só o primeiro pode receber o
        // nome oficial "Arco-Íris" sem esbarrar no índice único de nome — a colação do banco
        // ignora acento e caixa, não hífen nem espaço.
        pendentes =
        [
            .. pendentes
                .OrderBy(linha => Unico(porChaveExata, linha.Uf, ChaveExata(linha.Nome)) is { } oficial
                                  && ChaveDaColacao(oficial.Nome) == ChaveDaColacao(linha.Nome) ? 0 : 1)
                .ThenBy(linha => linha.Id)
                .Where(linha =>
            {
                if (Unico(porChaveExata, linha.Uf, ChaveExata(linha.Nome)) is not { } oficial
                    || !tomados.Add(oficial.Codigo))
                    return true;

                reconhecidos.Add(new ReconhecimentoNoIbge(linha.Id, linha.Nome, oficial, FormaDeReconhecimento.NomeIgual));
                return false;
            })
        ];

        // (2) SÓ O APÓSTROFO DIFERE. Código já tomado vira variante, e sai da fila do mesmo jeito.
        pendentes =
        [
            .. pendentes.Where(linha =>
            {
                if (Unico(porChaveSemApostrofo, linha.Uf, ChaveSemApostrofo(linha.Nome)) is not { } oficial)
                    return true;

                if (tomados.Add(oficial.Codigo))
                    reconhecidos.Add(new ReconhecimentoNoIbge(linha.Id, linha.Nome, oficial, FormaDeReconhecimento.GrafiaDoApostrofo));
                else
                    variantes.Add(new VarianteDeGrafia(linha.Id, linha.Nome, oficial, FormaDeReconhecimento.GrafiaDoApostrofo));

                return false;
            })
        ];

        // (3) NOME CORTADO EM 20.
        var semCorrespondencia = new List<MunicipioDoCatalogo>();
        foreach (var linha in pendentes)
        {
            var candidatos = linha.Nome.Trim().Length == MarcaDeTruncamento
                ? oficiais.Where(o => o.Uf == linha.Uf
                                      && ChaveSemApostrofo(o.Nome).StartsWith(ChaveSemApostrofo(linha.Nome), StringComparison.Ordinal))
                          .ToList()
                : [];

            if (candidatos.Count != 1)
            {
                semCorrespondencia.Add(linha);
                continue;
            }

            if (tomados.Add(candidatos[0].Codigo))
                reconhecidos.Add(new ReconhecimentoNoIbge(linha.Id, linha.Nome, candidatos[0], FormaDeReconhecimento.NomeTruncadoNaOrigem));
            else
                variantes.Add(new VarianteDeGrafia(linha.Id, linha.Nome, candidatos[0], FormaDeReconhecimento.NomeTruncadoNaOrigem));
        }

        return new ResultadoDoReconhecimento(
            reconhecidos,
            variantes,
            semCorrespondencia,
            [.. oficiais.Where(o => !tomados.Contains(o.Codigo)).OrderBy(o => o.Codigo)]);
    }

    /// <summary>
    /// AS GRAFIAS CORTADAS QUE O PRÓPRIO CATÁLOGO JÁ PROVA — sem chamar o IBGE.
    ///
    /// <para>Depois do reconhecimento, cada município oficial tem a sua linha com código e nome
    /// oficial. Uma linha sem código, com o nome cortado em 20 caracteres, que é prefixo de UM único
    /// município oficial da mesma UF, é a regra 3 de <see cref="Reconhecer"/> aplicada contra o catálogo
    /// reconhecido. É o que deixa a carga do sistema de origem conferir as grafias cortadas sem depender
    /// da API do IBGE. Catálogo que nunca foi reconhecido não tem código — e não devolve variante.</para>
    /// </summary>
    /// <param name="catalogo">O nosso catálogo inteiro.</param>
    public static IReadOnlyList<VarianteDeGrafia> VariantesCortadasDoCatalogo(IEnumerable<MunicipioDoCatalogo> catalogo)
    {
        var linhas = catalogo.ToList();
        var oficiais = linhas
            .Where(m => m.CodigoIbge is not null)
            .Select(m => new MunicipioDoIbge(m.CodigoIbge!.Value, m.Nome, m.Uf))
            .ToList();

        return [.. Reconhecer(linhas, oficiais).Variantes.Where(v => v.Forma == FormaDeReconhecimento.NomeTruncadoNaOrigem)];
    }

    /// <summary>
    /// Se o texto declara uma vaga em aberto, e não uma pessoa: "A Contratar 3", "CONTRATAR 4".
    /// </summary>
    /// <param name="nomeNaOrigem">A célula como veio.</param>
    public static bool EhVagaAContratar(string nomeNaOrigem) =>
        VagaAContratar().IsMatch(ChaveExata(nomeNaOrigem));

    /// <summary>
    /// A chave de uma pessoa: a chave exata com ponto e sublinhado virando espaço, e sem o domínio
    /// do e-mail. <c>FULANO.EXEMPLO</c>, <c>Fulano Exemplo</c> e <c>fulano.exemplo@…</c> dão a
    /// mesma chave.
    /// </summary>
    /// <param name="nome">O nome, o login ou o e-mail.</param>
    public static string ChaveDePessoa(string nome) => ResponsavelPeloMunicipio.ChaveDoNome(nome);

    /// <summary>
    /// Indexa os usuários pelas três formas de nome que as planilhas usam: o login (antes do
    /// arroba), o nome de exibição e o nome completo.
    /// </summary>
    /// <param name="usuarios">Os usuários que podem responder por município.</param>
    public static IReadOnlyDictionary<string, IReadOnlySet<long>> IndexarUsuarios(
        IEnumerable<(long Id, string NomePrincipal, string NomeExibicao, string NomeCompleto)> usuarios)
    {
        var indice = new Dictionary<string, HashSet<long>>(StringComparer.Ordinal);

        foreach (var (id, principal, exibicao, completo) in usuarios)
        {
            foreach (var forma in new[] { principal, exibicao, completo })
            {
                if (string.IsNullOrWhiteSpace(forma)) continue;
                var chave = ChaveDePessoa(forma);
                if (!indice.TryGetValue(chave, out var ids)) indice[chave] = ids = [];
                ids.Add(id);
            }
        }

        return indice.ToDictionary(p => p.Key, p => (IReadOnlySet<long>)p.Value, StringComparer.Ordinal);
    }

    /// <summary>
    /// O que se pode afirmar sobre o nome que a planilha escreveu.
    ///
    /// <para><b>Só igualdade, e só com um usuário.</b> "FULANO" não vira "FULANO.EXEMPLO" por
    /// semelhança, e um nome que casa com duas pessoas não escolhe nenhuma. Célula com duas
    /// pessoas ("X // Y") fica não identificada, com o texto inteiro preservado.</para>
    /// </summary>
    /// <param name="nomeNaOrigem">A célula como veio.</param>
    /// <param name="usuariosPorChave">O índice de <see cref="IndexarUsuarios"/>.</param>
    public static (SituacaoDoResponsavel Situacao, long? UsuarioId) IdentificarResponsavel(
        string nomeNaOrigem, IReadOnlyDictionary<string, IReadOnlySet<long>> usuariosPorChave)
    {
        if (EhVagaAContratar(nomeNaOrigem)) return (SituacaoDoResponsavel.VagaAContratar, null);

        if (nomeNaOrigem.Contains('/')) return (SituacaoDoResponsavel.NaoIdentificado, null);

        return usuariosPorChave.TryGetValue(ChaveDePessoa(nomeNaOrigem), out var ids) && ids.Count == 1
            ? (SituacaoDoResponsavel.UsuarioIdentificado, ids.Single())
            : (SituacaoDoResponsavel.NaoIdentificado, null);
    }

    /// <summary>Se a célula da planilha está vazia do jeito que as duas planilhas escrevem vazio.</summary>
    /// <param name="celula">O texto da célula.</param>
    public static bool CelulaVazia(string? celula) =>
        string.IsNullOrWhiteSpace(celula) || celula.Trim() == "-";

    /// <summary>
    /// Uma medida da PAM como o SIDRA a escreve — vale para área, quantidade e valor.
    ///
    /// <para><c>-</c> é zero absoluto no IBGE, e vira zero. <c>...</c>, <c>..</c> e <c>X</c> são "não
    /// disponível", "não se aplica" e "sigilo", e viram nulo — nunca zero. <b>Nulo na entrada</b>
    /// significa outra coisa: a variável não veio na resposta; também vira nulo. Qualquer outro
    /// símbolo para a carga: formato desconhecido não se adivinha.</para>
    /// </summary>
    /// <param name="valor">O campo <c>V</c> da resposta do SIDRA.</param>
    public static decimal? MedidaDoSidra(string? valor)
    {
        if (valor is null) return null;

        var texto = valor.Trim();

        if (texto == "-") return 0m;
        if (texto is "..." or ".." or "X") return null;

        if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var medida) && medida >= 0)
            return medida;

        throw new FormatException($"Valor do SIDRA fora do formato conhecido: \"{valor}\".");
    }

    private static Dictionary<(string Uf, string Chave), List<MunicipioDoIbge>> Agrupar(
        IEnumerable<MunicipioDoIbge> oficiais, Func<MunicipioDoIbge, string> chave) =>
        oficiais.GroupBy(o => (o.Uf, chave(o))).ToDictionary(g => g.Key, g => g.ToList());

    private static MunicipioDoIbge? Unico(
        Dictionary<(string Uf, string Chave), List<MunicipioDoIbge>> indice, string uf, string chave) =>
        indice.TryGetValue((uf, chave), out var lista) && lista.Count == 1 ? lista[0] : null;

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espacos();

    [GeneratedRegex(@"^(A )?CONTRATAR( \d+)?$")]
    private static partial Regex VagaAContratar();
}
