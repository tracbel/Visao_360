using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>Para que serve este endereço.</summary>
public enum TipoDeEndereco
{
    /// <summary>Endereço fiscal, o da nota.</summary>
    Fiscal = 0,

    /// <summary>Endereço de entrega.</summary>
    Entrega = 1,

    /// <summary>Endereço de cobrança.</summary>
    Cobranca = 2,

    /// <summary>Fazenda ou unidade produtiva do cliente.</summary>
    Fazenda = 3
}

/// <summary>
/// COMO O ENDEREÇO DIZ EM QUE MUNICÍPIO ELE ESTÁ — por seleção, ou pelo resíduo da migração.
///
/// <para><b>Por que um tipo, e não dois parâmetros.</b> A regra é "ou uma coisa, ou a outra,
/// nunca as duas nem nenhuma", e ela precisa valer em toda porta de entrada: o formulário, a
/// API e a carga. Escrita como dois parâmetros opcionais, ela viraria um <c>if</c> repetido em
/// cada chamador — e é assim que a exceção vira regra. Escrita como este tipo, a única forma de
/// construir um valor é escolher um dos dois caminhos, e o caminho do texto tem no nome o que
/// ele é.</para>
///
/// <para>O banco repete a mesma exigência em <c>CK_Endereco_Municipio</c>, porque o que a
/// aplicação garante o banco também garante — é a regra do documento 16, seção 3: campo com
/// catálogo não aceita digitação, e a garantia vale nos três momentos juntos.</para>
/// </summary>
public readonly record struct MunicipioDoEndereco
{
    private MunicipioDoEndereco(int? id, string? textoDoLegado)
    {
        Id = id;
        TextoNaoIdentificado = textoDoLegado;
    }

    /// <summary>O município escolhido no catálogo. Nulo só no resíduo da migração.</summary>
    public int? Id { get; }

    /// <summary>
    /// O texto que veio do sistema legado e não casou com nenhum município do catálogo.
    ///
    /// <para>É RESÍDUO DE MIGRAÇÃO, com prazo: o documento 26, seção 7, conta quantos endereços
    /// ainda estão assim e o que fecha essa conta. Quando ela chegar a zero, esta propriedade e
    /// a coluna que a sustenta saem — um deploy depois de o código parar de usá-las, como manda
    /// a regra 9.3 do documento 14.</para>
    /// </summary>
    public string? TextoNaoIdentificado { get; }

    /// <summary>O município veio do catálogo. É este o caminho de todo endereço novo.</summary>
    /// <param name="municipioId">O município escolhido.</param>
    public static MunicipioDoEndereco Selecionado(int municipioId) =>
        municipioId <= 0
            ? throw new RegraDeNegocioViolada("Município selecionado precisa de um identificador válido.")
            : new MunicipioDoEndereco(municipioId, null);

    /// <summary>
    /// O município NÃO foi identificado na carga — só o texto do legado sobrou.
    ///
    /// <para>O nome é longo de propósito. Este é o único caminho pelo qual um município entra
    /// como texto, e ele existe para não perder o que já veio do sistema antigo — não para uma
    /// tela oferecer digitação livre.</para>
    /// </summary>
    /// <param name="textoDoLegado">O município como o sistema de origem o escreveu.</param>
    public static MunicipioDoEndereco NaoIdentificadoNaCarga(string textoDoLegado) =>
        string.IsNullOrWhiteSpace(textoDoLegado)
            ? throw new RegraDeNegocioViolada(
                "Endereço sem município: nem seleção do catálogo, nem texto do legado.")
            : new MunicipioDoEndereco(null, textoDoLegado.Trim());
}

/// <summary>
/// Onde o cliente está — endereço fiscal, de entrega e fazenda.
///
/// O documento 04 não modelou endereço nenhum, e a Ficha do Cliente aprovada tem endereço
/// principal mais três fazendas, com hectares, cultura e coordenada. O mapa da tela de
/// Cobertura precisa de latitude e longitude.
/// </summary>
public sealed class Endereco : EntidadeBase
{
    private Endereco() { }

    /// <summary>Filial dona do registro.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O cliente dono do endereço.</summary>
    public long ClienteId { get; private set; }

    /// <summary>Para que serve este endereço.</summary>
    public TipoDeEndereco Tipo { get; private set; }

    /// <summary>Nome da fazenda ou da unidade, quando tem.</summary>
    public string? Identificacao { get; private set; }

    /// <summary>Logradouro.</summary>
    public string Logradouro { get; private set; } = default!;

    /// <summary>Número. É texto porque existe s/n e existe número com letra.</summary>
    public string? Numero { get; private set; }

    /// <summary>Complemento.</summary>
    public string? Complemento { get; private set; }

    /// <summary>Bairro.</summary>
    public string? Bairro { get; private set; }

    /// <summary>
    /// O município do catálogo <c>organizacao.Municipio</c> — a forma certa de dizer onde é.
    ///
    /// <para>É anulável no banco por uma razão só: os endereços que a carga trouxe do sistema
    /// legado e cujo município não casou com nenhuma linha do catálogo. Endereço NOVO nunca
    /// entra assim — o único caminho que deixa esta coluna nula é
    /// <see cref="MunicipioDoEndereco.NaoIdentificadoNaCarga"/>, e o banco cobra a exclusividade
    /// em <c>CK_Endereco_Municipio</c>.</para>
    /// </summary>
    public int? MunicipioId { get; private set; }

    /// <summary>
    /// RESÍDUO DE MIGRAÇÃO: o município como texto, do jeito que o legado o escreveu.
    ///
    /// <para>Preenchido apenas quando <see cref="MunicipioId"/> é nulo, e nunca junto com ele —
    /// a restrição <c>CK_Endereco_Municipio</c> cobra isso no banco. [V] Esta coluna nasceu como
    /// texto livre OBRIGATÓRIO e é o defeito que o documento 26 corrige: a carga normalizou 275
    /// municípios com espaço duplo (<c>SANTA RITA DO  PASSA</c>) porque o campo aceitava
    /// qualquer coisa.</para>
    ///
    /// <para><b>O nome dela continua sendo <c>Municipio</c> de propósito.</b> Um nome que se
    /// autodeclarasse resíduo seria melhor de ler, mas renomear coluna é exatamente o que a
    /// regra 9.3 do documento 14 proíbe — migração é aditiva. O que a marca como resíduo é a
    /// restrição de verificação, este comentário e a contagem do documento 26, seção 7. O
    /// caminho de saída é a REMOÇÃO, um deploy depois de a contagem chegar a zero.</para>
    /// </summary>
    public string? Municipio { get; private set; }

    /// <summary>
    /// Unidade federativa, duas letras.
    ///
    /// <para>Continua obrigatória porque o resíduo da migração depende dela para localizar
    /// qualquer coisa. Onde <see cref="MunicipioId"/> está preenchido, ela é DERIVADA do
    /// município e existe por herança — está registrada no documento 26, seção 8, como candidata
    /// a sair junto com <see cref="Municipio"/>.</para>
    /// </summary>
    public string Uf { get; private set; } = default!;

    /// <summary>CEP, oito dígitos, já validado.</summary>
    public Cep? Cep { get; private set; }

    /// <summary>Área em hectares, quando é fazenda.</summary>
    public decimal? Hectares { get; private set; }

    /// <summary>Item do catálogo CULTURA — nunca texto livre.</summary>
    public int? CulturaId { get; private set; }

    /// <summary>Latitude em graus decimais.</summary>
    public decimal? Latitude { get; private set; }

    /// <summary>Longitude em graus decimais.</summary>
    public decimal? Longitude { get; private set; }

    /// <summary>Se é o endereço principal do cliente. No máximo um por cliente.</summary>
    public bool EhPrincipal { get; private set; }

    /// <summary>
    /// A coordenada validada, quando as duas colunas estão preenchidas.
    ///
    /// Não é coluna: latitude e longitude são gravadas separadas porque o banco precisa
    /// ordenar e indexar cada uma. O tipo de valor entra aqui para que quem lê no domínio
    /// receba um par já validado, e não dois números soltos.
    /// </summary>
    public Coordenada? Localizacao =>
        Latitude is { } lat && Longitude is { } lon && Coordenada.TentarCriar((double)lat, (double)lon, out var c)
            ? c
            : null;

    /// <summary>Cria um endereço.</summary>
    /// <param name="empresaId">Filial dona do registro.</param>
    /// <param name="clienteId">O cliente dono do endereço.</param>
    /// <param name="tipo">Para que serve este endereço.</param>
    /// <param name="logradouro">Logradouro.</param>
    /// <param name="municipio">
    /// O município — do catálogo, ou o resíduo da carga. Ver <see cref="MunicipioDoEndereco"/>.
    /// </param>
    /// <param name="uf">Unidade federativa, duas letras.</param>
    /// <param name="criadoPorId">Quem criou.</param>
    /// <param name="numero">Número.</param>
    /// <param name="complemento">Complemento.</param>
    /// <param name="bairro">Bairro.</param>
    /// <param name="cep">CEP validado.</param>
    /// <param name="identificacao">Nome da fazenda ou da unidade.</param>
    /// <param name="ehPrincipal">Se é o endereço principal do cliente.</param>
    /// <param name="latitude">Latitude em graus decimais.</param>
    /// <param name="longitude">Longitude em graus decimais.</param>
    public static Endereco Criar(
        int empresaId,
        long clienteId,
        TipoDeEndereco tipo,
        string logradouro,
        MunicipioDoEndereco municipio,
        string uf,
        long criadoPorId,
        string? numero = null,
        string? complemento = null,
        string? bairro = null,
        Cep? cep = null,
        string? identificacao = null,
        bool ehPrincipal = false,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        if (uf.Length != 2)
            throw new RegraDeNegocioViolada($"UF inválida: '{uf}'. São exatamente duas letras.");

        var endereco = new Endereco
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            Tipo = tipo,
            Logradouro = logradouro,
            MunicipioId = municipio.Id,
            Municipio = municipio.TextoNaoIdentificado,
            Uf = uf.ToUpperInvariant(),
            EhPrincipal = ehPrincipal,
            CriadoPorId = criadoPorId
        };

        endereco.AtribuirDetalhes(numero, complemento, bairro, cep, identificacao, latitude, longitude);
        return endereco;
    }

    /// <summary>
    /// Altera o endereço.
    ///
    /// NEM A EMPRESA NEM O CLIENTE ENTRAM: mudar o dono de um endereço é mudar de quem é o
    /// registro, e isso não é edição de formulário — é outra operação, com outra permissão.
    /// </summary>
    /// <param name="tipo">Para que serve este endereço.</param>
    /// <param name="logradouro">Logradouro.</param>
    /// <param name="municipio">
    /// O município — do catálogo, ou o resíduo da carga. Ver <see cref="MunicipioDoEndereco"/>.
    /// </param>
    /// <param name="uf">Unidade federativa, duas letras.</param>
    /// <param name="usuarioId">Quem alterou.</param>
    /// <param name="numero">Número.</param>
    /// <param name="complemento">Complemento.</param>
    /// <param name="bairro">Bairro.</param>
    /// <param name="cep">CEP validado.</param>
    /// <param name="identificacao">Nome da fazenda ou da unidade.</param>
    /// <param name="latitude">Latitude em graus decimais.</param>
    /// <param name="longitude">Longitude em graus decimais.</param>
    public void Alterar(
        TipoDeEndereco tipo,
        string logradouro,
        MunicipioDoEndereco municipio,
        string uf,
        long usuarioId,
        string? numero = null,
        string? complemento = null,
        string? bairro = null,
        Cep? cep = null,
        string? identificacao = null,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        if (EstaExcluido)
            throw new RegraDeNegocioViolada("Endereço excluído não aceita alteração.");

        if (uf.Length != 2)
            throw new RegraDeNegocioViolada($"UF inválida: '{uf}'. São exatamente duas letras.");

        Tipo = tipo;
        Logradouro = logradouro;
        MunicipioId = municipio.Id;
        Municipio = municipio.TextoNaoIdentificado;
        Uf = uf.ToUpperInvariant();

        AtribuirDetalhes(numero, complemento, bairro, cep, identificacao, latitude, longitude);
        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Os campos opcionais, num lugar só — inclusive a regra do PAR de coordenada.
    ///
    /// Latitude sem longitude não localiza nada, e a restrição <c>CK_Endereco_Coordenada</c>
    /// recusa a linha no banco. Aqui as duas entram juntas ou nenhuma entra, para a recusa não
    /// chegar como erro de constraint.
    /// </summary>
    private void AtribuirDetalhes(
        string? numero,
        string? complemento,
        string? bairro,
        Cep? cep,
        string? identificacao,
        decimal? latitude,
        decimal? longitude)
    {
        Numero = string.IsNullOrWhiteSpace(numero) ? null : numero.Trim();
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = string.IsNullOrWhiteSpace(bairro) ? null : bairro.Trim();
        Identificacao = string.IsNullOrWhiteSpace(identificacao) ? null : identificacao.Trim();
        Cep = cep;

        var temPar = latitude is not null && longitude is not null;
        Latitude = temPar ? latitude : null;
        Longitude = temPar ? longitude : null;
    }
}
