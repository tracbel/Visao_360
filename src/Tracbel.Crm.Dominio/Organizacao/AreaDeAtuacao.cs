using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// A região da área de atuação, como a planilha do comercial a declara.
///
/// <para><b>Domínio fechado com os dois valores que a fonte traz, e nenhum a mais.</b> A planilha
/// <c>Area de Atuação.xlsx</c> escreve "Noroeste" em 120 municípios e "Norte" em 83 — somam os 203
/// da ADR. Não é a "regional" do protótipo (MT Norte, GO, BA Oeste), que não tem lastro: é a
/// divisão que o próprio comercial usa para a ADR (documento 32, seção 4).</para>
/// </summary>
public enum RegiaoDaAreaDeAtuacao
{
    /// <summary>A fonte não declara região — é o caso dos municípios fora da ADR.</summary>
    NaoInformada = 0,

    /// <summary>Região Norte da ADR.</summary>
    Norte = 1,

    /// <summary>Região Noroeste da ADR.</summary>
    Noroeste = 2
}

/// <summary>
/// UM MUNICÍPIO NA ÁREA DE ATUAÇÃO DA TRACBEL AGRO — e se ele pertence à ADR.
///
/// <para><b>Por que é tabela, e não coluna em <see cref="Municipio"/>.</b> O município é catálogo
/// nacional; pertencer à área de atuação é um fato DA TRACBEL, com origem e ciclo de vida
/// próprios: entra por uma planilha datada, pode sair numa revisão, e precisa dizer de qual arquivo
/// e de qual linha veio. Três colunas no catálogo nacional perderiam exatamente isso — e o
/// documento 17 manda campo quando "quase serve"; aqui não serve, porque o ciclo de vida é outro.</para>
///
/// <para><b>A filial responsável não se chama <c>EmpresaId</c>, de propósito.</b> <c>EmpresaId</c>
/// é o nome da fronteira de multiempresa (documento 14, regra 11.1): quem declara a coluna é
/// FILTRADO por ela. Aqui a filial é ATRIBUTO do território, não dona da linha — o diretor precisa
/// ver a ADR inteira, e um CEN de Jales precisa saber que Ribeirão Preto existe e é de outra loja.
/// Filtrar este cadastro por filial esconderia o mapa de quem mais precisa dele.</para>
///
/// <para><b>Encerrar, nunca apagar.</b> Se uma revisão da planilha tirar o município da área, a
/// linha ganha <see cref="EncerradoEm"/> e continua explicando por que os números antigos daquele
/// município foram contados como ADR.</para>
/// </summary>
public sealed class MunicipioDaAreaDeAtuacao
{
    private MunicipioDaAreaDeAtuacao() { }

    /// <summary>Identificador interno. É <c>int</c>: o teto é o número de municípios do Brasil.</summary>
    public int Id { get; private set; }

    /// <summary>O município do catálogo nacional.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>
    /// Se o município pertence à ADR. A planilha lista 238 municípios: 203 marcados como
    /// "Área ADR" e 35 sem marcação nenhuma — os 35 continuam aqui, com falso, porque são o que a
    /// fonte diz e ninguém confirmou o que eles significam (documento 32, seção 10).
    /// </summary>
    public bool PertenceAAdr { get; private set; }

    /// <summary>A região que a fonte declara.</summary>
    public RegiaoDaAreaDeAtuacao Regiao { get; private set; }

    /// <summary>A filial (loja) responsável pelo município, quando a fonte a declara.</summary>
    public int? EmpresaResponsavelId { get; private set; }

    /// <summary>O nome do arquivo de onde a linha veio. É rastro, não campo de formulário.</summary>
    public string ArquivoDeOrigem { get; private set; } = default!;

    /// <summary>A linha da planilha, contando o cabeçalho como linha 1.</summary>
    public int LinhaNaOrigem { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha pela última vez (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Quando o município saiu da área de atuação. Nulo é vigente.</summary>
    public DateTime? EncerradoEm { get; private set; }

    /// <summary>Registra um município da área de atuação.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="pertenceAAdr">Se a fonte o marca como ADR.</param>
    /// <param name="regiao">A região declarada.</param>
    /// <param name="empresaResponsavelId">A filial responsável, quando declarada.</param>
    /// <param name="arquivoDeOrigem">O nome do arquivo lido.</param>
    /// <param name="linhaNaOrigem">A linha lida.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public static MunicipioDaAreaDeAtuacao Registrar(
        int municipioId,
        bool pertenceAAdr,
        RegiaoDaAreaDeAtuacao regiao,
        int? empresaResponsavelId,
        string arquivoDeOrigem,
        int linhaNaOrigem,
        long importadoPorId,
        DateTime agoraUtc)
    {
        ConferirRastro(arquivoDeOrigem, linhaNaOrigem);

        return new MunicipioDaAreaDeAtuacao
        {
            MunicipioId = municipioId,
            PertenceAAdr = pertenceAAdr,
            Regiao = regiao,
            EmpresaResponsavelId = empresaResponsavelId,
            ArquivoDeOrigem = arquivoDeOrigem.Trim(),
            LinhaNaOrigem = linhaNaOrigem,
            ImportadoPorId = importadoPorId,
            ImportadoEm = agoraUtc
        };
    }

    /// <summary>
    /// Confere a linha contra uma nova leitura da fonte.
    ///
    /// <para><b>Devolve se algo mudou</b>, para a carga contar o que de fato alterou em vez de
    /// relatar a planilha inteira como "atualizada" a cada execução. O carimbo de importação anda
    /// sempre: ele diz quando a linha foi conferida pela última vez, mesmo sem mudança.</para>
    /// </summary>
    /// <param name="pertenceAAdr">Se a fonte o marca como ADR.</param>
    /// <param name="regiao">A região declarada.</param>
    /// <param name="empresaResponsavelId">A filial responsável, quando declarada.</param>
    /// <param name="arquivoDeOrigem">O nome do arquivo lido.</param>
    /// <param name="linhaNaOrigem">A linha lida.</param>
    /// <param name="importadoPorId">Quem rodou esta conferência — passa a ser o dono do carimbo.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public bool Conferir(
        bool pertenceAAdr,
        RegiaoDaAreaDeAtuacao regiao,
        int? empresaResponsavelId,
        string arquivoDeOrigem,
        int linhaNaOrigem,
        long importadoPorId,
        DateTime agoraUtc)
    {
        ConferirRastro(arquivoDeOrigem, linhaNaOrigem);

        var mudou = PertenceAAdr != pertenceAAdr
                    || Regiao != regiao
                    || EmpresaResponsavelId != empresaResponsavelId
                    || EncerradoEm is not null;

        PertenceAAdr = pertenceAAdr;
        Regiao = regiao;
        EmpresaResponsavelId = empresaResponsavelId;
        ArquivoDeOrigem = arquivoDeOrigem.Trim();
        LinhaNaOrigem = linhaNaOrigem;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;
        EncerradoEm = null;

        return mudou;
    }

    /// <summary>Tira o município da área de atuação sem apagar a linha.</summary>
    /// <param name="agoraUtc">O instante do encerramento.</param>
    public void Encerrar(DateTime agoraUtc) => EncerradoEm ??= agoraUtc;

    private static void ConferirRastro(string arquivoDeOrigem, int linhaNaOrigem)
    {
        if (string.IsNullOrWhiteSpace(arquivoDeOrigem))
            throw new RegraDeNegocioViolada("Linha de área de atuação sem arquivo de origem não tem rastro.");

        if (linhaNaOrigem < 2)
            throw new RegraDeNegocioViolada(
                "A linha 1 da planilha é o cabeçalho: dado de município começa na linha 2.");
    }
}

/// <summary>O papel de uma pessoa diante do município.</summary>
public enum PapelNoMunicipio
{
    /// <summary>O CEN — quem atende os clientes do município.</summary>
    Cen = 0,

    /// <summary>O gestor (gerente) do território.</summary>
    Gestor = 1
}

/// <summary>
/// De onde veio a afirmação "esta pessoa responde por este município".
///
/// <para>As duas planilhas recebidas em 13/09/2026 são guardadas LADO A LADO, e não fundidas: elas
/// concordam sobre os 203 municípios e sobre a loja, e discordam sobre o CEN em 82 deles. Escolher
/// uma em silêncio seria decidir pelo comercial qual está atual — e a atualidade de nenhuma das
/// duas está confirmada.</para>
/// </summary>
public enum FonteDoResponsavel
{
    /// <summary><c>Area de Atuação.xlsx</c>, coluna <c>CEN</c>.</summary>
    PlanilhaAreaDeAtuacao = 0,

    /// <summary>
    /// <c>CEN e Gestor por Municipio.xlsx</c>, colunas <c>Vendedor_Territorio</c> e
    /// <c>Gerente_Territorio</c> — segundo o relato, extraída do CRM.
    /// </summary>
    PlanilhaCenEGestorPorMunicipio = 1
}

/// <summary>O que a carga conseguiu afirmar sobre o nome que a fonte escreveu.</summary>
public enum SituacaoDoResponsavel
{
    /// <summary>O nome casou, sem ambiguidade, com exatamente um usuário do CRM.</summary>
    UsuarioIdentificado = 0,

    /// <summary>A fonte declara a vaga em aberto ("A Contratar 3", "CONTRATAR 4").</summary>
    VagaAContratar = 1,

    /// <summary>
    /// O nome não casou com usuário nenhum, ou casou com mais de um, ou traz duas pessoas na mesma
    /// célula. Fica o nome como a fonte escreveu, para revisão — nunca um palpite.
    /// </summary>
    NaoIdentificado = 2
}

/// <summary>
/// QUEM A FONTE DIZ QUE RESPONDE POR UM MUNICÍPIO — com a fonte, a linha e o nome como veio.
///
/// <para><b>É uma afirmação de fonte, não uma atribuição do sistema.</b> A carteira do CRM
/// (<see cref="CarteiraMunicipio"/>) diz quais cidades o CEN atende no Vórtice; esta tabela guarda o
/// que as planilhas do comercial dizem, cada uma na sua linha. Quando o negócio confirmar a
/// vigente, a confirmação vira dado — até lá, a tela mostra as duas e marca a divergência.</para>
///
/// <para><b>O nome da origem é preservado mesmo quando o usuário foi identificado.</b> É o que
/// permite a qualquer um conferir a decisão da carga contra a planilha, linha a linha.</para>
///
/// <para><b>Vigência.</b> Nenhuma das duas planilhas declara desde quando o vínculo vale. Por isso
/// não há coluna de "vigente desde" inventada: há <see cref="ImportadoEm"/>, que é o fato que
/// existe, e <see cref="EncerradoEm"/>, que é quando uma leitura posterior deixou de afirmar o
/// vínculo.</para>
/// </summary>
public sealed class ResponsavelPeloMunicipio
{
    private ResponsavelPeloMunicipio() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O município.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>CEN ou gestor.</summary>
    public PapelNoMunicipio Papel { get; private set; }

    /// <summary>A planilha que fez a afirmação.</summary>
    public FonteDoResponsavel Fonte { get; private set; }

    /// <summary>O nome exatamente como a fonte escreveu. Rastro da carga, não digitação de tela.</summary>
    public string NomeNaOrigem { get; private set; } = default!;

    /// <summary>O que a carga conseguiu afirmar sobre esse nome.</summary>
    public SituacaoDoResponsavel Situacao { get; private set; }

    /// <summary>O usuário do CRM, só quando <see cref="Situacao"/> é identificado.</summary>
    public long? UsuarioId { get; private set; }

    /// <summary>A chave da linha na fonte, quando a fonte tem uma (ex.: <c>%ChaveTerritorio</c>).</summary>
    public string? ChaveNaOrigem { get; private set; }

    /// <summary>O nome do arquivo de onde a linha veio.</summary>
    public string ArquivoDeOrigem { get; private set; } = default!;

    /// <summary>A linha da planilha, contando o cabeçalho como linha 1.</summary>
    public int LinhaNaOrigem { get; private set; }

    /// <summary>Quando a carga gravou a afirmação (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Quando uma leitura posterior deixou de fazer esta afirmação. Nulo é vigente.</summary>
    public DateTime? EncerradoEm { get; private set; }

    /// <summary>Registra o que a fonte afirma.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="papel">CEN ou gestor.</param>
    /// <param name="fonte">A planilha.</param>
    /// <param name="nomeNaOrigem">O nome como a fonte escreveu.</param>
    /// <param name="situacao">O que a carga conseguiu afirmar sobre o nome.</param>
    /// <param name="usuarioId">O usuário, quando identificado.</param>
    /// <param name="chaveNaOrigem">A chave da linha na fonte, quando existe.</param>
    /// <param name="arquivoDeOrigem">O nome do arquivo.</param>
    /// <param name="linhaNaOrigem">A linha.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public static ResponsavelPeloMunicipio Registrar(
        int municipioId,
        PapelNoMunicipio papel,
        FonteDoResponsavel fonte,
        string nomeNaOrigem,
        SituacaoDoResponsavel situacao,
        long? usuarioId,
        string? chaveNaOrigem,
        string arquivoDeOrigem,
        int linhaNaOrigem,
        long importadoPorId,
        DateTime agoraUtc)
    {
        if (string.IsNullOrWhiteSpace(nomeNaOrigem))
            throw new RegraDeNegocioViolada(
                "Célula vazia não é responsável: a carga registra a ausência como recusa, não como vínculo.");

        // A SITUAÇÃO E O USUÁRIO ANDAM JUNTOS. "Identificado" sem usuário seria afirmar o que não
        // se sabe; usuário numa vaga a contratar seria inventar quem ocupa a vaga.
        if ((situacao == SituacaoDoResponsavel.UsuarioIdentificado) != (usuarioId is not null))
            throw new RegraDeNegocioViolada(
                "Só o responsável identificado aponta para um usuário, e todo identificado aponta.");

        if (string.IsNullOrWhiteSpace(arquivoDeOrigem) || linhaNaOrigem < 2)
            throw new RegraDeNegocioViolada("Responsável sem arquivo e linha de origem não tem rastro.");

        return new ResponsavelPeloMunicipio
        {
            MunicipioId = municipioId,
            Papel = papel,
            Fonte = fonte,
            NomeNaOrigem = nomeNaOrigem.Trim(),
            Situacao = situacao,
            UsuarioId = usuarioId,
            ChaveNaOrigem = string.IsNullOrWhiteSpace(chaveNaOrigem) ? null : chaveNaOrigem.Trim(),
            ArquivoDeOrigem = arquivoDeOrigem.Trim(),
            LinhaNaOrigem = linhaNaOrigem,
            ImportadoPorId = importadoPorId,
            ImportadoEm = agoraUtc
        };
    }

    /// <summary>
    /// A chave pela qual dois textos de planilha nomeiam a mesma pessoa: sem o domínio do e-mail,
    /// em maiúsculas, sem acento, com ponto, sublinhado e hífen virando espaço.
    /// <c>FULANO.DE.TAL</c>, <c>Fulano de Tal</c> e <c>fulano.de.tal@…</c> dão a mesma chave.
    ///
    /// <para><b>É igualdade, não semelhança.</b> "FULANO" e "FULANO.DE.TAL" dão chaves diferentes, e
    /// a tela os mostra como divergência — decidir que são a mesma pessoa é do comercial.</para>
    /// </summary>
    /// <param name="nome">O nome, o login ou o e-mail.</param>
    public static string ChaveDoNome(string nome)
    {
        var decomposto = nome.Split('@')[0].Trim().ToUpperInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var texto = new System.Text.StringBuilder(decomposto.Length);

        foreach (var caractere in decomposto)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(caractere)
                == System.Globalization.UnicodeCategory.NonSpacingMark) continue;

            texto.Append(caractere is '.' or '_' or '-' || char.IsWhiteSpace(caractere) ? ' ' : caractere);
        }

        return string.Join(' ', texto.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// COMO AS DUAS PLANILHAS NOMEIAM O CEN de um município — um rótulo para a tela, e só isso.
    ///
    /// <para><b>Não funde nada e não escolhe fonte.</b> "Provável mesma pessoa" diz que os dois textos
    /// diferem na grafia (mesmo primeiro nome, ou um contido no outro); decidir que é a mesma pessoa, e
    /// qual planilha vale, é do comercial (documento 32, seção 4.3). As duas afirmações continuam
    /// gravadas, cada uma com a sua fonte.</para>
    /// </summary>
    /// <param name="nomeNumaFonte">O nome numa planilha.</param>
    /// <param name="usuarioNumaFonte">O usuário identificado nela, quando houver.</param>
    /// <param name="nomeNaOutra">O nome na outra planilha.</param>
    /// <param name="usuarioNaOutra">O usuário identificado na outra, quando houver.</param>
    public static ComparacaoDoCen CompararCen(
        string nomeNumaFonte, long? usuarioNumaFonte, string nomeNaOutra, long? usuarioNaOutra)
    {
        if (usuarioNumaFonte is not null && usuarioNumaFonte == usuarioNaOutra) return ComparacaoDoCen.MesmoNome;

        var numa = ChaveDoNome(nomeNumaFonte);
        var outra = ChaveDoNome(nomeNaOutra);

        if (numa == outra) return ComparacaoDoCen.MesmoNome;

        return numa.Length > 0 && outra.Length > 0
               && (numa.Split(' ')[0] == outra.Split(' ')[0]
                   || numa.Contains(outra, StringComparison.Ordinal)
                   || outra.Contains(numa, StringComparison.Ordinal))
            ? ComparacaoDoCen.ProvavelMesmaPessoa
            : ComparacaoDoCen.NomesDiferentes;
    }

    /// <summary>
    /// Se uma nova leitura faz a MESMA afirmação — mesmo nome, mesma situação, mesmo usuário.
    ///
    /// <para>Mudar a linha da planilha não é mudar a afirmação: reordenar o arquivo não pode
    /// encerrar 203 vínculos e abrir outros 203 iguais.</para>
    /// </summary>
    /// <param name="nomeNaOrigem">O nome da nova leitura.</param>
    /// <param name="situacao">A situação da nova leitura.</param>
    /// <param name="usuarioId">O usuário da nova leitura.</param>
    public bool AfirmaOMesmo(string nomeNaOrigem, SituacaoDoResponsavel situacao, long? usuarioId) =>
        string.Equals(NomeNaOrigem, nomeNaOrigem.Trim(), StringComparison.Ordinal)
        && Situacao == situacao
        && UsuarioId == usuarioId;

    /// <summary>Encerra a afirmação sem apagar a linha.</summary>
    /// <param name="agoraUtc">O instante do encerramento.</param>
    public void Encerrar(DateTime agoraUtc) => EncerradoEm ??= agoraUtc;
}

/// <summary>Como as duas planilhas nomeiam o CEN de um município (documento 32, seção 4.3).</summary>
public enum ComparacaoDoCen
{
    /// <summary>Só uma planilha (ou nenhuma) declara CEN: não há o que comparar.</summary>
    UmaFonteSo = 0,

    /// <summary>O mesmo nome, ou o mesmo usuário do CRM identificado nas duas.</summary>
    MesmoNome = 1,

    /// <summary>Mesmo primeiro nome, ou um nome contido no outro — provável mesma pessoa, NÃO confirmado.</summary>
    ProvavelMesmaPessoa = 2,

    /// <summary>Nomes diferentes, inclusive vaga "a contratar" numa e nome na outra.</summary>
    NomesDiferentes = 3
}

/// <summary>
/// A ÁREA PLANTADA DE UM PRODUTO NUM MUNICÍPIO, num ano — da Produção Agrícola Municipal do IBGE.
///
/// <para><b>Por que existe.</b> O potencial por cultura precisa de área plantada, e nenhuma fonte
/// interna alcançável a tem: <c>comercial.Endereco.Hectares</c> e <c>CulturaId</c> estão vazios em
/// 100% das 19.641 linhas, e a base de propriedades do ART não responde desta rede (documento 31,
/// seção 2). A PAM é a fonte oficial e pública da área por município — é o que sustenta o
/// "potencial total da região", e só ele. Ela <b>não</b> separa cliente de não cliente.</para>
///
/// <para><b>Zero e "não disponível" são coisas diferentes, e as duas ficam.</b> O IBGE escreve
/// <c>-</c> para zero absoluto e <c>...</c> (ou <c>X</c>, sigilo) para dado não disponível. Aqui o
/// primeiro vira <c>0</c> e o segundo vira nulo — somar um "não disponível" como zero diria que o
/// município não planta o que só não foi divulgado.</para>
///
/// <para><b>O produto é o código da classificação oficial do IBGE</b> (tabela SIDRA 5457,
/// classificação 782), e o nome é o rótulo oficial dele — o mesmo papel do código IBGE do
/// município. Não é texto digitado.</para>
/// </summary>
public sealed class AreaPlantadaNoMunicipio
{
    private AreaPlantadaNoMunicipio() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O município.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>O ano da pesquisa.</summary>
    public short Ano { get; private set; }

    /// <summary>O código do produto na classificação 782 do IBGE. Ex.: 40139 é café (em grão) total.</summary>
    public int ProdutoCodigoIbge { get; private set; }

    /// <summary>O rótulo oficial do produto.</summary>
    public string ProdutoNome { get; private set; } = default!;

    /// <summary>Hectares plantados. Nulo é "não disponível" no IBGE; zero é zero.</summary>
    public decimal? AreaPlantadaHectares { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>
    /// Quem rodou a carga que gravou ou conferiu a linha. A fonte é pública, e mesmo assim o rastro
    /// é o mesmo das outras tabelas do território: quem trouxe o dado para dentro do CRM.
    /// </summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra a área de um produto.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    /// <param name="produtoCodigoIbge">O código do produto.</param>
    /// <param name="produtoNome">O rótulo oficial do produto.</param>
    /// <param name="areaPlantadaHectares">A área; nulo quando o IBGE não divulga.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public static AreaPlantadaNoMunicipio Registrar(
        int municipioId,
        short ano,
        int produtoCodigoIbge,
        string produtoNome,
        decimal? areaPlantadaHectares,
        long importadoPorId,
        DateTime agoraUtc)
    {
        if (ano is < 1974 or > 2100)
            throw new RegraDeNegocioViolada($"A Produção Agrícola Municipal começa em 1974; ano {ano} não existe nela.");

        if (produtoCodigoIbge <= 0)
            throw new RegraDeNegocioViolada("Produto sem código do IBGE não é produto da classificação oficial.");

        var registro = new AreaPlantadaNoMunicipio
        {
            MunicipioId = municipioId,
            Ano = ano,
            ProdutoCodigoIbge = produtoCodigoIbge
        };

        registro.Reapurar(produtoNome, areaPlantadaHectares, importadoPorId, agoraUtc);
        return registro;
    }

    /// <summary>
    /// Substitui a área pelo valor de uma nova leitura. Devolve se algo mudou.
    ///
    /// <para>SUBSTITUI, e não soma: o IBGE revisa a série, e a leitura nova é a verdade do ano.</para>
    /// </summary>
    /// <param name="produtoNome">O rótulo oficial do produto.</param>
    /// <param name="areaPlantadaHectares">A área; nulo quando o IBGE não divulga.</param>
    /// <param name="importadoPorId">Quem rodou esta leitura.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public bool Reapurar(string produtoNome, decimal? areaPlantadaHectares, long importadoPorId, DateTime agoraUtc)
    {
        if (string.IsNullOrWhiteSpace(produtoNome))
            throw new RegraDeNegocioViolada("Produto sem nome oficial não se mostra na tela.");

        if (areaPlantadaHectares < 0)
            throw new RegraDeNegocioViolada("Área plantada negativa não existe.");

        var mudou = !string.Equals(ProdutoNome, produtoNome.Trim(), StringComparison.Ordinal)
                    || AreaPlantadaHectares != areaPlantadaHectares;

        ProdutoNome = produtoNome.Trim();
        AreaPlantadaHectares = areaPlantadaHectares;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;
        return mudou;
    }
}

/// <summary>Se a regra foi confirmada pelo negócio ou ainda é um exemplo informado.</summary>
public enum SituacaoDaRegraDePotencial
{
    /// <summary>Informada como exemplo; o cálculo sai com o aviso de que a regra não foi confirmada.</summary>
    AConfirmar = 0,

    /// <summary>Confirmada pelo comercial, com vigência.</summary>
    Confirmada = 1
}

/// <summary>
/// UMA REGRA DE POTENCIAL POR ÁREA — "uma máquina de referência a cada N hectares de um produto".
///
/// <para><b>Hoje existe uma só, e ela não está confirmada.</b> É o exemplo do gerente comercial:
/// "na cultura de café, existe potencial de 1 trator 3036N a cada 10 hectares". A regra entra com
/// <see cref="SituacaoDaRegraDePotencial.AConfirmar"/>, e toda tela que a usa diz isso. Não há
/// generalização para outra cultura, outro modelo, horizonte, arredondamento ou preço — nada disso
/// foi informado (documento 32, seção 8.3).</para>
///
/// <para><b>O resultado é necessidade teórica de frota, não venda.</b> "1 trator a cada 10 ha" diz
/// quantas máquinas a área comporta; quantas serão compradas por ano depende de ciclo de troca,
/// parque instalado e concorrência — três dados que não existem hoje.</para>
///
/// <para><b>O modelo de referência é texto</b> porque o catálogo <c>frota.Modelo</c> não tem o
/// 3036N: dos 253 modelos carregados, nenhum é ele. Apontar para um modelo parecido seria inventar
/// a regra; quando o catálogo de modelos for saneado, este campo vira chave estrangeira.</para>
/// </summary>
public sealed class RegraDePotencial
{
    private RegraDePotencial() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O produto, pelo código da classificação 782 do IBGE.</summary>
    public int ProdutoCodigoIbge { get; private set; }

    /// <summary>O rótulo do produto como a regra o nomeia.</summary>
    public string ProdutoNome { get; private set; } = default!;

    /// <summary>Quantos hectares do produto correspondem a uma máquina de referência.</summary>
    public decimal HectaresPorMaquina { get; private set; }

    /// <summary>O modelo de referência, como o negócio o escreveu.</summary>
    public string ModeloDeReferencia { get; private set; } = default!;

    /// <summary>Se a regra é exemplo ou está confirmada.</summary>
    public SituacaoDaRegraDePotencial Situacao { get; private set; }

    /// <summary>Quem informou e onde — o rastro da regra.</summary>
    public string Origem { get; private set; } = default!;

    /// <summary>Quando a regra foi informada.</summary>
    public DateOnly InformadaEm { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Máquinas teóricas para uma área. Nulo quando a área não está disponível.</summary>
    /// <param name="areaHectares">A área plantada do produto.</param>
    public decimal? MaquinasTeoricas(decimal? areaHectares) =>
        areaHectares is { } area ? area / HectaresPorMaquina : null;

    /// <summary>Registra uma regra.</summary>
    /// <param name="produtoCodigoIbge">O código do produto.</param>
    /// <param name="produtoNome">O rótulo do produto.</param>
    /// <param name="hectaresPorMaquina">Hectares por máquina de referência.</param>
    /// <param name="modeloDeReferencia">O modelo de referência.</param>
    /// <param name="origem">Quem informou e onde.</param>
    /// <param name="informadaEm">Quando foi informada.</param>
    public static RegraDePotencial Informar(
        int produtoCodigoIbge,
        string produtoNome,
        decimal hectaresPorMaquina,
        string modeloDeReferencia,
        string origem,
        DateOnly informadaEm)
    {
        if (hectaresPorMaquina <= 0)
            throw new RegraDeNegocioViolada("Hectares por máquina precisa ser positivo.");

        if (string.IsNullOrWhiteSpace(modeloDeReferencia) || string.IsNullOrWhiteSpace(origem))
            throw new RegraDeNegocioViolada("Regra de potencial sem modelo de referência ou sem origem não tem rastro.");

        return new RegraDePotencial
        {
            ProdutoCodigoIbge = produtoCodigoIbge,
            ProdutoNome = produtoNome.Trim(),
            HectaresPorMaquina = hectaresPorMaquina,
            ModeloDeReferencia = modeloDeReferencia.Trim(),
            Origem = origem.Trim(),
            InformadaEm = informadaEm,
            Situacao = SituacaoDaRegraDePotencial.AConfirmar
        };
    }
}
