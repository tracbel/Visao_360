using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Integracao;

/// <summary>O que uma correspondência da origem traduz.</summary>
public enum TipoDeCorrespondencia
{
    /// <summary>A linha da origem → a classificação de produto do CRM.</summary>
    LinhaDeProduto = 0,

    /// <summary>O produto da origem → o modelo do catálogo de frota.</summary>
    Produto = 1,

    /// <summary>A unidade da origem → a filial do CRM.</summary>
    Unidade = 2
}

/// <summary>Em que ponto está a correspondência.</summary>
public enum SituacaoDaCorrespondencia
{
    /// <summary>Correspondência por regra exata e escrita — utilizável sem revisão.</summary>
    CorrespondenciaExata = 0,

    /// <summary>Sem correspondência segura: fica para uma pessoa decidir.</summary>
    PendenteDeRevisao = 1,

    /// <summary>O valor da origem não é classificação de produto (ex.: "USADOS" é condição da venda).</summary>
    NaoEClassificacaoDeProduto = 2,

    /// <summary>Uma pessoa confirmou a correspondência.</summary>
    ConfirmadaPorRevisao = 3,

    /// <summary>Uma pessoa recusou a correspondência.</summary>
    RecusadaPorRevisao = 4
}

/// <summary>
/// O DE-PARA EXPLÍCITO de um valor da origem para o catálogo do CRM — linha, produto ou unidade
/// (documento 35, seção 10).
///
/// <para><b>O valor da origem fica como veio.</b> <see cref="TextoNaOrigem"/> guarda o texto exato,
/// e <see cref="CodigoNaOrigem"/> a chave normalizada que a recarga usa para reencontrar a linha. A
/// tradução é uma coluna, com o critério escrito ao lado — nunca uma troca silenciosa do valor.</para>
///
/// <para><b>O que uma pessoa revisou, a carga não mexe.</b> A carga só avalia a correspondência que
/// ninguém revisou; a revisada fica como está, em toda recarga.</para>
/// </summary>
public sealed class CorrespondenciaDaOrigem
{
    private CorrespondenciaDaOrigem() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O sistema de origem.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O que é traduzido.</summary>
    public TipoDeCorrespondencia Tipo { get; private set; }

    /// <summary>A chave normalizada do valor da origem — a identidade da linha na recarga.</summary>
    public string CodigoNaOrigem { get; private set; } = default!;

    /// <summary>O valor exatamente como a origem escreve.</summary>
    public string TextoNaOrigem { get; private set; } = default!;

    /// <summary>O contexto da origem — para o produto, a linha em que ele aparece.</summary>
    public string? ContextoNaOrigem { get; private set; }

    /// <summary>A classificação de produto correspondente.</summary>
    public int? LinhaDeProdutoId { get; private set; }

    /// <summary>O modelo do catálogo correspondente.</summary>
    public int? ModeloId { get; private set; }

    /// <summary>A filial correspondente.</summary>
    public int? EmpresaCorrespondenteId { get; private set; }

    /// <summary>Em que ponto está.</summary>
    public SituacaoDaCorrespondencia Situacao { get; private set; } = SituacaoDaCorrespondencia.PendenteDeRevisao;

    /// <summary>A regra aplicada, ou o motivo de estar pendente.</summary>
    public string Criterio { get; private set; } = default!;

    /// <summary>Quantas linhas da origem usaram este valor na última leitura.</summary>
    public int Ocorrencias { get; private set; }

    /// <summary>Quando o valor apareceu pela primeira vez (UTC).</summary>
    public DateTime PrimeiraLeituraEm { get; private set; }

    /// <summary>A última leitura em que o valor apareceu (UTC).</summary>
    public DateTime UltimaLeituraEm { get; private set; }

    /// <summary>Quando uma pessoa revisou (UTC). Nula enquanto ninguém revisou.</summary>
    public DateTime? RevisadaEm { get; private set; }

    /// <summary>Quem revisou.</summary>
    public long? RevisadaPorId { get; private set; }

    /// <summary>Se uma pessoa já revisou — o que torna a linha intocável pela carga.</summary>
    public bool FoiRevisada => RevisadaEm is not null;

    /// <summary>Se a correspondência pode ser usada para classificar ou apontar.</summary>
    public bool EhUtilizavel =>
        Situacao is SituacaoDaCorrespondencia.CorrespondenciaExata or SituacaoDaCorrespondencia.ConfirmadaPorRevisao;

    /// <summary>Registra um valor da origem visto pela primeira vez, ainda sem avaliação.</summary>
    /// <param name="sistemaId">O sistema.</param>
    /// <param name="tipo">O que é traduzido.</param>
    /// <param name="codigoNaOrigem">A chave normalizada.</param>
    /// <param name="textoNaOrigem">O texto exato.</param>
    /// <param name="contextoNaOrigem">O contexto.</param>
    /// <param name="lidaEm">O instante da leitura.</param>
    public static CorrespondenciaDaOrigem Registrar(
        int sistemaId, TipoDeCorrespondencia tipo, string codigoNaOrigem, string textoNaOrigem,
        string? contextoNaOrigem, DateTime lidaEm)
    {
        if (string.IsNullOrWhiteSpace(codigoNaOrigem))
            throw new RegraDeNegocioViolada("Correspondência precisa da chave do valor da origem.");

        return new CorrespondenciaDaOrigem
        {
            SistemaId = sistemaId,
            Tipo = tipo,
            CodigoNaOrigem = codigoNaOrigem,
            TextoNaOrigem = textoNaOrigem,
            ContextoNaOrigem = contextoNaOrigem,
            Criterio = "Ainda não avaliada.",
            PrimeiraLeituraEm = lidaEm,
            UltimaLeituraEm = lidaEm
        };
    }

    /// <summary>Carimba a leitura desta rodada.</summary>
    /// <param name="ocorrencias">Quantas linhas usaram o valor.</param>
    /// <param name="lidaEm">O instante.</param>
    public void RegistrarLeitura(int ocorrencias, DateTime lidaEm)
    {
        Ocorrencias = ocorrencias;
        UltimaLeituraEm = lidaEm;
    }

    /// <summary>
    /// Aplica a avaliação da carga. Não faz nada quando uma pessoa já revisou.
    /// </summary>
    /// <param name="situacao">A situação avaliada.</param>
    /// <param name="linhaDeProdutoId">A classificação.</param>
    /// <param name="modeloId">O modelo.</param>
    /// <param name="empresaCorrespondenteId">A filial.</param>
    /// <param name="criterio">A regra, ou o motivo da pendência.</param>
    /// <returns>Verdadeiro quando a avaliação mudou algo.</returns>
    public bool Avaliar(
        SituacaoDaCorrespondencia situacao, int? linhaDeProdutoId, int? modeloId, int? empresaCorrespondenteId, string criterio)
    {
        if (FoiRevisada) return false;
        if (situacao is SituacaoDaCorrespondencia.ConfirmadaPorRevisao or SituacaoDaCorrespondencia.RecusadaPorRevisao)
            throw new RegraDeNegocioViolada("Confirmar ou recusar é revisão de pessoa, não avaliação da carga.");

        var mudou = Situacao != situacao || LinhaDeProdutoId != linhaDeProdutoId || ModeloId != modeloId
                    || EmpresaCorrespondenteId != empresaCorrespondenteId || Criterio != criterio;

        Situacao = situacao;
        LinhaDeProdutoId = linhaDeProdutoId;
        ModeloId = modeloId;
        EmpresaCorrespondenteId = empresaCorrespondenteId;
        Criterio = criterio;
        return mudou;
    }

    /// <summary>A revisão de uma pessoa: confirma ou recusa, com o critério escrito.</summary>
    /// <param name="confirmada">Confirmar (verdadeiro) ou recusar.</param>
    /// <param name="linhaDeProdutoId">A classificação.</param>
    /// <param name="modeloId">O modelo.</param>
    /// <param name="empresaCorrespondenteId">A filial.</param>
    /// <param name="criterio">Por quê.</param>
    /// <param name="usuarioId">Quem revisou.</param>
    /// <param name="quando">O instante.</param>
    public void Revisar(
        bool confirmada, int? linhaDeProdutoId, int? modeloId, int? empresaCorrespondenteId, string criterio,
        long usuarioId, DateTime quando)
    {
        if (string.IsNullOrWhiteSpace(criterio))
            throw new RegraDeNegocioViolada("A revisão de uma correspondência precisa do critério escrito.");

        Situacao = confirmada ? SituacaoDaCorrespondencia.ConfirmadaPorRevisao : SituacaoDaCorrespondencia.RecusadaPorRevisao;
        LinhaDeProdutoId = confirmada ? linhaDeProdutoId : null;
        ModeloId = confirmada ? modeloId : null;
        EmpresaCorrespondenteId = confirmada ? empresaCorrespondenteId : null;
        Criterio = criterio.Trim();
        RevisadaEm = quando;
        RevisadaPorId = usuarioId;
    }
}

/// <summary>O que a integração decidiu sobre um registro da origem.</summary>
public enum DecisaoDaIntegracao
{
    /// <summary>Entrou: a venda existe no CRM.</summary>
    Importado = 0,

    /// <summary>Não entrou: os motivos estão no registro.</summary>
    Pendente = 1
}

/// <summary>O retrato de um registro da origem, sem dado pessoal nem valor financeiro.</summary>
/// <param name="Hash">O resumo do conteúdo lido.</param>
/// <param name="ChassiNaOrigem">O chassi como a origem escreve.</param>
/// <param name="LinhaNaOrigem">A linha como a origem escreve.</param>
/// <param name="ProdutoNaOrigem">O produto como a origem escreve.</param>
/// <param name="UnidadeNaOrigem">A unidade como a origem escreve.</param>
/// <param name="VendidaEm">A data da venda, saneada.</param>
/// <param name="Transformacoes">As transformações aplicadas na leitura.</param>
public sealed record RetratoDoRegistroDeOrigem(
    string Hash,
    string? ChassiNaOrigem,
    string? LinhaNaOrigem,
    string? ProdutoNaOrigem,
    string? UnidadeNaOrigem,
    DateOnly? VendidaEm,
    string? Transformacoes);

/// <summary>
/// A TRILHA de cada registro lido da origem — identificador, primeira e última leitura, mudança de
/// conteúdo, ausência e a decisão com os motivos (documento 35, seção 10).
///
/// <para>É o que torna a recarga repetível e verificável: o mesmo registro é reencontrado pela
/// chave, o conteúdo igual não regrava nada, o conteúdo diferente fica datado, e o registro que some
/// da origem é marcado ausente — nunca apagado.</para>
///
/// <para><b>Sem dado pessoal:</b> o documento e o nome do comprador não moram aqui. A fila de
/// compradores ausentes (<see cref="CompradorPendente"/>) é que os guarda, com a fronteira de
/// filial.</para>
/// </summary>
public sealed class RegistroDeOrigem
{
    private RegistroDeOrigem() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O sistema de origem.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O fluxo. Ex.: ART.VENDA_DE_MAQUINA.</summary>
    public string Fluxo { get; private set; } = default!;

    /// <summary>O identificador do registro na origem.</summary>
    public string ChaveOrigem { get; private set; } = default!;

    /// <summary>O resumo do conteúdo na última leitura.</summary>
    public string HashDoConteudo { get; private set; } = default!;

    /// <summary>O chassi como a origem escreve.</summary>
    public string? ChassiNaOrigem { get; private set; }

    /// <summary>A linha como a origem escreve.</summary>
    public string? LinhaNaOrigem { get; private set; }

    /// <summary>O produto como a origem escreve.</summary>
    public string? ProdutoNaOrigem { get; private set; }

    /// <summary>A unidade como a origem escreve.</summary>
    public string? UnidadeNaOrigem { get; private set; }

    /// <summary>A data da venda, saneada.</summary>
    public DateOnly? VendidaEm { get; private set; }

    /// <summary>As transformações aplicadas na leitura.</summary>
    public string? Transformacoes { get; private set; }

    /// <summary>A decisão da última leitura.</summary>
    public DecisaoDaIntegracao Decisao { get; private set; } = DecisaoDaIntegracao.Pendente;

    /// <summary>Os motivos, quando pendente — códigos separados por vírgula.</summary>
    public string? Motivos { get; private set; }

    /// <summary>A venda criada ou atualizada a partir deste registro.</summary>
    public long? VendaDeMaquinaId { get; private set; }

    /// <summary>A primeira leitura (UTC).</summary>
    public DateTime PrimeiraLeituraEm { get; private set; }

    /// <summary>A última leitura em que o registro existia na origem (UTC).</summary>
    public DateTime UltimaLeituraEm { get; private set; }

    /// <summary>A última vez que o conteúdo mudou na origem (UTC).</summary>
    public DateTime? ConteudoAlteradoEm { get; private set; }

    /// <summary>Desde quando o registro não aparece na origem (UTC). Nulo enquanto aparece.</summary>
    public DateTime? AusenteNaOrigemDesde { get; private set; }

    /// <summary>Quantas leituras encontraram o registro.</summary>
    public int Leituras { get; private set; }

    /// <summary>Registra a primeira leitura de um registro.</summary>
    /// <param name="sistemaId">O sistema.</param>
    /// <param name="fluxo">O fluxo.</param>
    /// <param name="chaveOrigem">O identificador na origem.</param>
    /// <param name="retrato">O conteúdo sem dado pessoal.</param>
    /// <param name="lidaEm">O instante.</param>
    public static RegistroDeOrigem Registrar(
        int sistemaId, string fluxo, string chaveOrigem, RetratoDoRegistroDeOrigem retrato, DateTime lidaEm)
    {
        if (string.IsNullOrWhiteSpace(chaveOrigem))
            throw new RegraDeNegocioViolada("Registro de origem sem identificador não é rastreável.");

        var registro = new RegistroDeOrigem
        {
            SistemaId = sistemaId,
            Fluxo = fluxo,
            ChaveOrigem = chaveOrigem.Trim(),
            PrimeiraLeituraEm = lidaEm
        };

        registro.Aplicar(retrato);
        registro.UltimaLeituraEm = lidaEm;
        registro.Leituras = 1;
        return registro;
    }

    /// <summary>Carimba uma nova leitura e diz se o conteúdo mudou.</summary>
    /// <param name="retrato">O conteúdo desta leitura.</param>
    /// <param name="lidaEm">O instante.</param>
    /// <returns>Verdadeiro quando o conteúdo é diferente do da leitura anterior.</returns>
    public bool RegistrarLeitura(RetratoDoRegistroDeOrigem retrato, DateTime lidaEm)
    {
        var mudou = retrato.Hash != HashDoConteudo;
        if (mudou) ConteudoAlteradoEm = lidaEm;

        Aplicar(retrato);
        UltimaLeituraEm = lidaEm;
        AusenteNaOrigemDesde = null;
        Leituras++;
        return mudou;
    }

    /// <summary>Registra a decisão desta leitura.</summary>
    /// <param name="decisao">Importado ou pendente.</param>
    /// <param name="motivos">Os motivos, quando pendente.</param>
    /// <param name="vendaDeMaquinaId">A venda, quando importado.</param>
    public void Decidir(DecisaoDaIntegracao decisao, string? motivos, long? vendaDeMaquinaId)
    {
        Decisao = decisao;
        Motivos = string.IsNullOrWhiteSpace(motivos) ? null : motivos;
        VendaDeMaquinaId = vendaDeMaquinaId ?? VendaDeMaquinaId;
    }

    /// <summary>Marca que o registro não apareceu nesta leitura da origem. Nada é apagado.</summary>
    /// <param name="quando">O instante da leitura.</param>
    public void MarcarAusente(DateTime quando) => AusenteNaOrigemDesde ??= quando;

    private void Aplicar(RetratoDoRegistroDeOrigem retrato)
    {
        HashDoConteudo = retrato.Hash;
        ChassiNaOrigem = retrato.ChassiNaOrigem;
        LinhaNaOrigem = retrato.LinhaNaOrigem;
        ProdutoNaOrigem = retrato.ProdutoNaOrigem;
        UnidadeNaOrigem = retrato.UnidadeNaOrigem;
        VendidaEm = retrato.VendidaEm;
        Transformacoes = retrato.Transformacoes;
    }
}

/// <summary>Onde o comprador ausente do CRM foi encontrado.</summary>
public enum GrupoDoCompradorPendente
{
    /// <summary>Tem nota de saída do Protheus carregada, sem cliente no CRM.</summary>
    ComNotaNoProtheus = 0,

    /// <summary>Não tem nota carregada nem cliente no CRM.</summary>
    SemNotaNoProtheus = 1
}

/// <summary>Em que ponto está o comprador pendente.</summary>
public enum SituacaoDoCompradorPendente
{
    /// <summary>Ainda não há cliente com este documento no CRM.</summary>
    AguardandoCadastro = 0,

    /// <summary>Um cliente com este documento passou a existir no CRM.</summary>
    Cadastrado = 1
}

/// <summary>O que o cadastro de clientes do Protheus (SA1) diz sobre o documento.</summary>
public enum SituacaoNoCadastroDoProtheus
{
    /// <summary>O cadastro do Protheus não foi consultado nesta rodada.</summary>
    NaoConferido = 0,

    /// <summary>Nenhum cadastro com o documento.</summary>
    Ausente = 1,

    /// <summary>Cadastro existente e ativo.</summary>
    Ativo = 2,

    /// <summary>Cadastro existente e bloqueado.</summary>
    Bloqueado = 3
}

/// <summary>O que a carga apurou sobre um comprador ausente do CRM.</summary>
/// <param name="EmpresaId">A filial da venda mais recente — a fronteira de acesso.</param>
/// <param name="NomeNaOrigem">O nome como a origem escreve.</param>
/// <param name="Grupo">Com ou sem nota no Protheus.</param>
/// <param name="Vendas">Quantas vendas da origem têm este comprador.</param>
/// <param name="VendasComChassiValido">Delas, quantas têm chassi completo e válido.</param>
/// <param name="PrimeiraVendaEm">A venda mais antiga.</param>
/// <param name="UltimaVendaEm">A venda mais recente.</param>
/// <param name="FiliaisDasVendas">Os códigos das filiais das vendas.</param>
/// <param name="NotasNoProtheus">Quantas linhas de nota sem cliente no CRM têm o documento.</param>
/// <param name="NaturezaNasNotas">A natureza da contraparte nas notas.</param>
/// <param name="PrimeiraNotaEm">A competência mais antiga dessas notas.</param>
/// <param name="UltimaNotaEm">A competência mais recente.</param>
/// <param name="NomeDaNotaCoincide">Se o nome nas notas é igual ao da origem, normalizado.</param>
/// <param name="SituacaoNoCadastroDoProtheus">O que a SA1 diz.</param>
/// <param name="TemEnderecoNoProtheus">Se a SA1 tem endereço.</param>
/// <param name="TemMunicipioNoProtheus">Se a SA1 tem código de município.</param>
/// <param name="TemInscricaoEstadualNoProtheus">Se a SA1 tem inscrição estadual.</param>
/// <param name="NomeDoCadastroCoincide">Se o nome na SA1 é igual ao da origem, normalizado.</param>
/// <param name="DadosQueFaltam">O que falta para um cadastro confiável, em texto.</param>
public sealed record ApuracaoDoCompradorPendente(
    int EmpresaId,
    string NomeNaOrigem,
    GrupoDoCompradorPendente Grupo,
    int Vendas,
    int VendasComChassiValido,
    DateOnly? PrimeiraVendaEm,
    DateOnly? UltimaVendaEm,
    string FiliaisDasVendas,
    int NotasNoProtheus,
    string? NaturezaNasNotas,
    DateOnly? PrimeiraNotaEm,
    DateOnly? UltimaNotaEm,
    bool NomeDaNotaCoincide,
    SituacaoNoCadastroDoProtheus SituacaoNoCadastroDoProtheus,
    bool TemEnderecoNoProtheus,
    bool TemMunicipioNoProtheus,
    bool TemInscricaoEstadualNoProtheus,
    bool NomeDoCadastroCoincide,
    string DadosQueFaltam);

/// <summary>
/// A FILA DE COMPRADORES AUSENTES do CRM — nenhum é criado automaticamente (documento 35, seção 10).
///
/// <para>Cada linha diz onde o documento foi encontrado (notas do Protheus, cadastro SA1) e o que
/// falta para um cadastro confiável. Quem cadastra é uma pessoa; a carga só marca
/// <see cref="SituacaoDoCompradorPendente.Cadastrado"/> quando o cliente passa a existir.</para>
/// </summary>
public sealed class CompradorPendente : EntidadeBase
{
    private CompradorPendente() { }

    /// <summary>A filial da venda mais recente — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O sistema de origem.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O documento do comprador.</summary>
    public CpfCnpj Documento { get; private set; }

    /// <summary>Física ou jurídica, pelo tamanho do documento.</summary>
    public TipoDePessoa TipoDePessoa { get; private set; }

    /// <summary>O nome como a origem escreve.</summary>
    public string NomeNaOrigem { get; private set; } = default!;

    /// <summary>Com ou sem nota no Protheus.</summary>
    public GrupoDoCompradorPendente Grupo { get; private set; }

    /// <summary>Em que ponto está.</summary>
    public SituacaoDoCompradorPendente Situacao { get; private set; } = SituacaoDoCompradorPendente.AguardandoCadastro;

    /// <summary>Quantas vendas da origem têm este comprador.</summary>
    public int Vendas { get; private set; }

    /// <summary>Delas, quantas têm chassi completo e válido.</summary>
    public int VendasComChassiValido { get; private set; }

    /// <summary>A venda mais antiga.</summary>
    public DateOnly? PrimeiraVendaEm { get; private set; }

    /// <summary>A venda mais recente.</summary>
    public DateOnly? UltimaVendaEm { get; private set; }

    /// <summary>Os códigos das filiais das vendas.</summary>
    public string FiliaisDasVendas { get; private set; } = default!;

    /// <summary>Linhas de nota sem cliente no CRM com este documento.</summary>
    public int NotasNoProtheus { get; private set; }

    /// <summary>A natureza da contraparte nas notas.</summary>
    public string? NaturezaNasNotas { get; private set; }

    /// <summary>A competência mais antiga das notas.</summary>
    public DateOnly? PrimeiraNotaEm { get; private set; }

    /// <summary>A competência mais recente das notas.</summary>
    public DateOnly? UltimaNotaEm { get; private set; }

    /// <summary>Se o nome nas notas é igual ao da origem, normalizado.</summary>
    public bool NomeDaNotaCoincide { get; private set; }

    /// <summary>O que a SA1 do Protheus diz.</summary>
    public SituacaoNoCadastroDoProtheus SituacaoNoCadastroDoProtheus { get; private set; }

    /// <summary>Se a SA1 tem endereço.</summary>
    public bool TemEnderecoNoProtheus { get; private set; }

    /// <summary>Se a SA1 tem código de município.</summary>
    public bool TemMunicipioNoProtheus { get; private set; }

    /// <summary>Se a SA1 tem inscrição estadual.</summary>
    public bool TemInscricaoEstadualNoProtheus { get; private set; }

    /// <summary>Se o nome na SA1 é igual ao da origem, normalizado.</summary>
    public bool NomeDoCadastroCoincide { get; private set; }

    /// <summary>O que falta para um cadastro confiável.</summary>
    public string DadosQueFaltam { get; private set; } = default!;

    /// <summary>A última apuração da carga (UTC).</summary>
    public DateTime ApuradoEm { get; private set; }

    /// <summary>Registra um comprador ausente.</summary>
    /// <param name="sistemaId">O sistema de origem.</param>
    /// <param name="documento">O documento válido.</param>
    /// <param name="apuracao">O que a carga apurou.</param>
    /// <param name="quando">O instante.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    public static CompradorPendente Registrar(
        int sistemaId, CpfCnpj documento, ApuracaoDoCompradorPendente apuracao, DateTime quando, long criadoPorId)
    {
        var comprador = new CompradorPendente
        {
            SistemaId = sistemaId,
            Documento = documento,
            TipoDePessoa = documento.EhPessoaFisica ? TipoDePessoa.Fisica : TipoDePessoa.Juridica,
            CriadoPorId = criadoPorId
        };

        comprador.Aplicar(apuracao, quando);
        return comprador;
    }

    /// <summary>Atualiza a apuração desta rodada.</summary>
    /// <param name="apuracao">O que a carga apurou.</param>
    /// <param name="quando">O instante.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public void Atualizar(ApuracaoDoCompradorPendente apuracao, DateTime quando, long usuarioId)
    {
        Aplicar(apuracao, quando);
        Situacao = SituacaoDoCompradorPendente.AguardandoCadastro;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>Marca que um cliente com o documento passou a existir no CRM.</summary>
    /// <param name="quando">O instante.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public void MarcarCadastrado(DateTime quando, long usuarioId)
    {
        if (Situacao == SituacaoDoCompradorPendente.Cadastrado) return;
        Situacao = SituacaoDoCompradorPendente.Cadastrado;
        ApuradoEm = quando;
        MarcarAlteracao(usuarioId);
    }

    private void Aplicar(ApuracaoDoCompradorPendente a, DateTime quando)
    {
        if (string.IsNullOrWhiteSpace(a.NomeNaOrigem))
            throw new RegraDeNegocioViolada("Comprador pendente precisa do nome como a origem escreve.");

        EmpresaId = a.EmpresaId;
        NomeNaOrigem = a.NomeNaOrigem;
        Grupo = a.Grupo;
        Vendas = a.Vendas;
        VendasComChassiValido = a.VendasComChassiValido;
        PrimeiraVendaEm = a.PrimeiraVendaEm;
        UltimaVendaEm = a.UltimaVendaEm;
        FiliaisDasVendas = a.FiliaisDasVendas;
        NotasNoProtheus = a.NotasNoProtheus;
        NaturezaNasNotas = a.NaturezaNasNotas;
        PrimeiraNotaEm = a.PrimeiraNotaEm;
        UltimaNotaEm = a.UltimaNotaEm;
        NomeDaNotaCoincide = a.NomeDaNotaCoincide;
        SituacaoNoCadastroDoProtheus = a.SituacaoNoCadastroDoProtheus;
        TemEnderecoNoProtheus = a.TemEnderecoNoProtheus;
        TemMunicipioNoProtheus = a.TemMunicipioNoProtheus;
        TemInscricaoEstadualNoProtheus = a.TemInscricaoEstadualNoProtheus;
        NomeDoCadastroCoincide = a.NomeDoCadastroCoincide;
        DadosQueFaltam = a.DadosQueFaltam;
        ApuradoEm = quando;
    }
}

/// <summary>O tipo de divergência entre as fontes.</summary>
public enum TipoDeDivergencia
{
    /// <summary>O comprador da venda não é o dono atual registrado no CRM.</summary>
    CompradorDiferenteDoProprietarioNoCrm = 0,

    /// <summary>O dono do chassi no Protheus não é o comprador da venda.</summary>
    ProprietarioNoProtheusDiferenteDoComprador = 1,

    /// <summary>A origem trocou o comprador de uma venda já importada.</summary>
    CompradorAlteradoNaOrigem = 2,

    /// <summary>A origem trocou o chassi de uma venda já importada.</summary>
    ChassiAlteradoNaOrigem = 3,

    /// <summary>Uma venda importada deixou de aparecer na origem.</summary>
    RegistroAusenteNaOrigem = 4
}

/// <summary>Em que ponto está a divergência.</summary>
public enum SituacaoDaDivergencia
{
    /// <summary>Detectada e sem tratamento.</summary>
    Aberta = 0,

    /// <summary>Uma pessoa tratou.</summary>
    Resolvida = 1,

    /// <summary>A carga deixou de encontrá-la.</summary>
    DeixouDeOcorrer = 2
}

/// <summary>
/// UMA DIVERGÊNCIA ENTRE FONTES — ART, CRM e Protheus —, registrada para revisão e nunca resolvida
/// por sobrescrita (documento 35, seção 10).
///
/// <para>Os valores guardam referência, não dado pessoal: a chave pública do cliente do CRM, e não o
/// documento dele.</para>
/// </summary>
public sealed class DivergenciaDeIntegracao : EntidadeBase
{
    private DivergenciaDeIntegracao() { }

    /// <summary>A filial da venda — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O sistema de origem.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O tipo.</summary>
    public TipoDeDivergencia Tipo { get; private set; }

    /// <summary>O registro da origem que a produziu.</summary>
    public string ChaveOrigem { get; private set; } = default!;

    /// <summary>A máquina envolvida.</summary>
    public long? EquipamentoId { get; private set; }

    /// <summary>A venda envolvida.</summary>
    public long? VendaDeMaquinaId { get; private set; }

    /// <summary>O que a divergência é, em texto.</summary>
    public string Descricao { get; private set; } = default!;

    /// <summary>O valor no CRM.</summary>
    public string? ValorNoCrm { get; private set; }

    /// <summary>O valor na origem.</summary>
    public string? ValorNaOrigem { get; private set; }

    /// <summary>O valor no Protheus.</summary>
    public string? ValorNoProtheus { get; private set; }

    /// <summary>Em que ponto está.</summary>
    public SituacaoDaDivergencia Situacao { get; private set; } = SituacaoDaDivergencia.Aberta;

    /// <summary>Quando foi detectada (UTC).</summary>
    public DateTime DetectadaEm { get; private set; }

    /// <summary>A última carga que a encontrou (UTC).</summary>
    public DateTime ConfirmadaEm { get; private set; }

    /// <summary>Quando foi resolvida ou deixou de ocorrer (UTC).</summary>
    public DateTime? EncerradaEm { get; private set; }

    /// <summary>Registra uma divergência.</summary>
    /// <param name="empresaId">A filial.</param>
    /// <param name="sistemaId">O sistema.</param>
    /// <param name="tipo">O tipo.</param>
    /// <param name="chaveOrigem">O registro da origem.</param>
    /// <param name="equipamentoId">A máquina.</param>
    /// <param name="vendaDeMaquinaId">A venda.</param>
    /// <param name="descricao">O que é.</param>
    /// <param name="valorNoCrm">O valor no CRM.</param>
    /// <param name="valorNaOrigem">O valor na origem.</param>
    /// <param name="valorNoProtheus">O valor no Protheus.</param>
    /// <param name="quando">O instante.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    public static DivergenciaDeIntegracao Registrar(
        int empresaId, int sistemaId, TipoDeDivergencia tipo, string chaveOrigem, long? equipamentoId,
        long? vendaDeMaquinaId, string descricao, string? valorNoCrm, string? valorNaOrigem, string? valorNoProtheus,
        DateTime quando, long criadoPorId) => new()
    {
        EmpresaId = empresaId,
        SistemaId = sistemaId,
        Tipo = tipo,
        ChaveOrigem = chaveOrigem,
        EquipamentoId = equipamentoId,
        VendaDeMaquinaId = vendaDeMaquinaId,
        Descricao = descricao,
        ValorNoCrm = valorNoCrm,
        ValorNaOrigem = valorNaOrigem,
        ValorNoProtheus = valorNoProtheus,
        DetectadaEm = quando,
        ConfirmadaEm = quando,
        CriadoPorId = criadoPorId
    };

    /// <summary>
    /// A carga encontrou a divergência de novo. Se os valores mudaram, ela reabre; se são os mesmos e
    /// uma pessoa já a resolveu, continua resolvida.
    /// </summary>
    /// <param name="descricao">O que é.</param>
    /// <param name="valorNoCrm">O valor no CRM.</param>
    /// <param name="valorNaOrigem">O valor na origem.</param>
    /// <param name="valorNoProtheus">O valor no Protheus.</param>
    /// <param name="quando">O instante.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public void Reconfirmar(
        string descricao, string? valorNoCrm, string? valorNaOrigem, string? valorNoProtheus, DateTime quando, long usuarioId)
    {
        var mudou = ValorNoCrm != valorNoCrm || ValorNaOrigem != valorNaOrigem || ValorNoProtheus != valorNoProtheus;
        ConfirmadaEm = quando;

        if (mudou || Situacao == SituacaoDaDivergencia.DeixouDeOcorrer)
        {
            Descricao = descricao;
            ValorNoCrm = valorNoCrm;
            ValorNaOrigem = valorNaOrigem;
            ValorNoProtheus = valorNoProtheus;
            Situacao = SituacaoDaDivergencia.Aberta;
            EncerradaEm = null;
            MarcarAlteracao(usuarioId);
        }
    }

    /// <summary>A carga não encontrou mais a divergência.</summary>
    /// <param name="quando">O instante.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public void MarcarQueDeixouDeOcorrer(DateTime quando, long usuarioId)
    {
        if (Situacao != SituacaoDaDivergencia.Aberta) return;
        Situacao = SituacaoDaDivergencia.DeixouDeOcorrer;
        EncerradaEm = quando;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>Uma pessoa tratou a divergência.</summary>
    /// <param name="quando">O instante.</param>
    /// <param name="usuarioId">Quem tratou.</param>
    public void Resolver(DateTime quando, long usuarioId)
    {
        Situacao = SituacaoDaDivergencia.Resolvida;
        EncerradaEm = quando;
        MarcarAlteracao(usuarioId);
    }
}

/// <summary>Como terminou uma execução de sincronização.</summary>
public enum ResultadoDaExecucao
{
    /// <summary>Começou e ainda não terminou — ou o processo caiu no meio.</summary>
    EmAndamento = 0,

    /// <summary>Terminou e gravou.</summary>
    Sucesso = 1,

    /// <summary>Esgotou as tentativas sem gravar; a mensagem diz por quê.</summary>
    Falha = 2,

    /// <summary>Não rodou porque outra execução do mesmo fluxo estava em andamento.</summary>
    Ignorada = 3
}

/// <summary>
/// UMA EXECUÇÃO DO SERVIÇO DE SINCRONIZAÇÃO — quando começou, quando terminou, em que máquina, com que
/// resultado e quanto leu e gravou (documento 35, seção 11).
///
/// <para>É o que responde "a integração está rodando?" sem abrir o servidor: a última execução, a
/// última com sucesso e a falha com a mensagem. A mensagem nunca carrega credencial — as leituras do
/// ART e do Protheus devolvem só o código do erro.</para>
/// </summary>
public sealed class ExecucaoDeSincronizacao
{
    private const int TamanhoDaMensagem = 1000;

    private ExecucaoDeSincronizacao() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O sistema de origem.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O fluxo. Ex.: ART.VENDA_DE_MAQUINA.</summary>
    public string Fluxo { get; private set; } = default!;

    /// <summary>A máquina que executou.</summary>
    public string Maquina { get; private set; } = default!;

    /// <summary>Quando começou (UTC).</summary>
    public DateTime IniciadaEm { get; private set; }

    /// <summary>Quando terminou (UTC).</summary>
    public DateTime? TerminadaEm { get; private set; }

    /// <summary>Como terminou.</summary>
    public ResultadoDaExecucao Resultado { get; private set; } = ResultadoDaExecucao.EmAndamento;

    /// <summary>Quantas tentativas foram feitas.</summary>
    public int Tentativas { get; private set; }

    /// <summary>Registros lidos da origem.</summary>
    public int RegistrosLidos { get; private set; }

    /// <summary>Vendas incluídas.</summary>
    public int Incluidos { get; private set; }

    /// <summary>Vendas atualizadas pela origem.</summary>
    public int Atualizados { get; private set; }

    /// <summary>Registros pendentes.</summary>
    public int Pendentes { get; private set; }

    /// <summary>O resumo ou o motivo da falha, sem credencial.</summary>
    public string? Mensagem { get; private set; }

    /// <summary>Registra o começo de uma execução.</summary>
    /// <param name="sistemaId">O sistema.</param>
    /// <param name="fluxo">O fluxo.</param>
    /// <param name="maquina">A máquina que executa.</param>
    /// <param name="quando">O instante.</param>
    public static ExecucaoDeSincronizacao Iniciar(int sistemaId, string fluxo, string maquina, DateTime quando) => new()
    {
        SistemaId = sistemaId,
        Fluxo = fluxo,
        Maquina = string.IsNullOrWhiteSpace(maquina) ? "?" : maquina[..Math.Min(60, maquina.Length)],
        IniciadaEm = quando
    };

    /// <summary>Encerra com sucesso.</summary>
    /// <param name="tentativas">Tentativas feitas.</param>
    /// <param name="lidos">Registros lidos.</param>
    /// <param name="incluidos">Vendas incluídas.</param>
    /// <param name="atualizados">Vendas atualizadas.</param>
    /// <param name="pendentes">Registros pendentes.</param>
    /// <param name="mensagem">O resumo.</param>
    /// <param name="quando">O instante.</param>
    public void Concluir(int tentativas, int lidos, int incluidos, int atualizados, int pendentes, string? mensagem, DateTime quando)
    {
        Resultado = ResultadoDaExecucao.Sucesso;
        Tentativas = tentativas;
        RegistrosLidos = lidos;
        Incluidos = incluidos;
        Atualizados = atualizados;
        Pendentes = pendentes;
        Mensagem = Cortar(mensagem);
        TerminadaEm = quando;
    }

    /// <summary>Encerra com falha, depois de esgotar as tentativas.</summary>
    /// <param name="tentativas">Tentativas feitas.</param>
    /// <param name="mensagem">O motivo, sem credencial.</param>
    /// <param name="quando">O instante.</param>
    public void Falhar(int tentativas, string mensagem, DateTime quando)
    {
        Resultado = ResultadoDaExecucao.Falha;
        Tentativas = tentativas;
        Mensagem = Cortar(mensagem);
        TerminadaEm = quando;
    }

    /// <summary>Encerra sem rodar — outra execução do fluxo estava em andamento.</summary>
    /// <param name="motivo">Por quê.</param>
    /// <param name="quando">O instante.</param>
    public void Ignorar(string motivo, DateTime quando)
    {
        Resultado = ResultadoDaExecucao.Ignorada;
        Mensagem = Cortar(motivo);
        TerminadaEm = quando;
    }

    private static string? Cortar(string? texto) =>
        texto is null ? null : texto.Length <= TamanhoDaMensagem ? texto : texto[..(TamanhoDaMensagem - 3)] + "...";
}
