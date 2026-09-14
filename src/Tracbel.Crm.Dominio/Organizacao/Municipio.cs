using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O MUNICÍPIO — o catálogo nacional que substitui o município digitado à mão.
///
/// <para><b>Por que existe.</b> [V] O sistema de origem tem a tabela <c>GE_Cidade</c> com 10.214
/// linhas e a usa de verdade — <c>GE_Pessoa.SeqCidade</c> aponta para ela em 96% dos cadastros —,
/// mas o cadastro de pessoa guarda TAMBÉM o nome do município como texto livre ao lado, e os dois
/// divergem em 217 linhas. O nosso <c>comercial.Endereco.Municipio</c> herdou o pior dos dois
/// mundos: só o texto. Este catálogo é o que fecha esse campo — daqui para frente município é
/// seleção, e o texto sobra apenas como resíduo do que a carga não conseguiu casar.</para>
///
/// <para><b>Não tem coluna de empresa, e é decisão, não esquecimento.</b> Município é dado
/// nacional: Cajuru é Cajuru para a filial de Ribeirão Preto e para a de Marília. Copiar
/// <c>EmpresaId</c> aqui criaria 13 cópias do mesmo Brasil e transformaria a fronteira de
/// multiempresa numa fronteira de catálogo — exatamente o antipadrão que o documento 14, regra
/// 11.1, proíbe (banco por filial, tabela replicada por filial). A fronteira de multiempresa
/// alcança o município pela CARTEIRA, que tem <c>EmpresaId</c>, e por meio de
/// <see cref="CarteiraMunicipio"/>.</para>
///
/// <para><b>Também não herda <c>EntidadeBase</c>.</b> Pelo mesmo motivo de
/// <see cref="LinhaDeNegocio"/> e de <see cref="Praca"/>: é dado de referência, não registro
/// transacional. Ninguém "cria um município" no CRM — a lista existe antes do primeiro cliente e
/// muda por decreto federal, não por operação comercial. O bloco de auditoria de oito colunas
/// (documento 14, seção 5.1) existe para responder "quem mexeu neste registro de negócio", e a
/// resposta aqui é sempre "a carga". O que precisa de trilha é o VÍNCULO da carteira com o
/// município, e é lá que ela está.</para>
/// </summary>
public sealed class Municipio
{
    private Municipio() { }

    /// <summary>Identificador interno. É <c>int</c>: o Brasil tem 5.570 municípios.</summary>
    public int Id { get; private set; }

    /// <summary>
    /// O código do IBGE de sete dígitos — a chave natural do município no Brasil.
    ///
    /// <para><b>É anulável porque a origem não tem esse código.</b> <c>GE_Cidade</c> não tem
    /// coluna de IBGE: as colunas dela são <c>SeqCidade</c> (sequencial próprio),
    /// <c>Cidade</c>, <c>Uf</c>, <c>Regiao</c>, <c>Populacao</c>, faixa de CEP e DDD. A carga
    /// identifica o município por <b>nome mais UF</b>, e o risco disso está escrito no documento
    /// 26, seção 4. Quando o código do IBGE chegar — por carga de uma tabela oficial ou pelo
    /// Protheus —, ele entra aqui e passa a ser a chave natural sem nenhuma outra mudança de
    /// esquema: o índice único já existe, filtrado para os que têm o código.</para>
    /// </summary>
    public int? CodigoIbge { get; private set; }

    /// <summary>Nome do município.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>
    /// Unidade federativa, duas letras maiúsculas.
    ///
    /// <para>Domínio fechado pela restrição <c>CK_Municipio_Uf</c>, com as 27 unidades
    /// enumeradas e comparadas em <c>Latin1_General_BIN2</c> — a colação do banco ignora caixa e
    /// acento, e sem a colação binária a restrição aceitaria <c>'sp'</c> e <c>'PÁ'</c>. [V] a UF
    /// de <c>GE_Cidade</c> é texto livre e guarda hoje <c>**</c>, <c>EX</c>, <c>MI</c>,
    /// <c>PÁ</c> e o vazio.</para>
    /// </summary>
    public string Uf { get; private set; } = default!;

    /// <summary>Desligar sem apagar — município que deixou de existir por fusão ou emancipação.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Cadastra um município.</summary>
    /// <param name="nome">Nome do município.</param>
    /// <param name="uf">Unidade federativa, duas letras.</param>
    /// <param name="codigoIbge">O código do IBGE, quando a origem o declara.</param>
    public static Municipio Criar(string nome, string uf, int? codigoIbge = null) => new()
    {
        Nome = nome.Trim(),
        Uf = uf.Trim().ToUpperInvariant(),
        CodigoIbge = codigoIbge
    };

    /// <summary>
    /// Reconhece o município no cadastro oficial do IBGE: recebe o código e passa a usar o NOME
    /// OFICIAL.
    ///
    /// <para><b>O nome muda porque o da origem está errado de três jeitos medidos</b> (documento
    /// 26, seção 4.5): cortado em 20 caracteres (<c>SANTA CRUZ DA ESPERA</c>), sem acento e em
    /// caixa alta, e com o apóstrofo trocado por espaço. A carga que chama este método grava o nome
    /// anterior em <c>auditoria.AlteracaoDeCampo</c> — o valor da origem continua recuperável.</para>
    ///
    /// <para><b>Um código já reconhecido não troca.</b> Se a mesma linha casar com outro código
    /// numa rodada futura, é defeito de casamento, e defeito tem de parar a carga — trocar em
    /// silêncio mudaria o município de todos os endereços que apontam para esta linha.</para>
    /// </summary>
    /// <param name="codigoIbge">O código de sete dígitos.</param>
    /// <param name="nomeOficial">O nome como o IBGE o publica.</param>
    /// <returns>Se algo mudou.</returns>
    public bool ReconhecerNoIbge(int codigoIbge, string nomeOficial)
    {
        if (codigoIbge is < 1000000 or > 5999999)
            throw new RegraDeNegocioViolada($"Código do IBGE tem sete dígitos, de 1000000 a 5999999: {codigoIbge}.");

        if (string.IsNullOrWhiteSpace(nomeOficial))
            throw new RegraDeNegocioViolada("O IBGE não publica município sem nome.");

        if (CodigoIbge is { } atual && atual != codigoIbge)
            throw new RegraDeNegocioViolada(
                $"O município {Id} já foi reconhecido como {atual} no IBGE e agora casou com {codigoIbge}. " +
                "Isso é defeito de casamento, e não troca de código.");

        var mudou = CodigoIbge != codigoIbge || !string.Equals(Nome, nomeOficial.Trim(), StringComparison.Ordinal);

        CodigoIbge = codigoIbge;
        Nome = nomeOficial.Trim();
        return mudou;
    }
}

/// <summary>
/// O MUNICÍPIO QUE UMA CARTEIRA ATENDE — o agrupamento territorial que existe de verdade.
///
/// <para><b>O achado que motiva esta tabela.</b> A premissa de que a Tracbel Agro se organiza por
/// "regional" (MT Norte, GO, BA Oeste) veio do protótipo e <b>não tem lastro no sistema atual</b>:
/// a tabela <c>IVS_Regional</c> existe e tem ZERO linhas. O agrupamento real é outro, e está
/// preenchido: a carteira pertence a uma filial (<c>IVS_Carteira.NroEmpresa</c>) e atende um
/// conjunto de municípios (<c>IVS_CartCid</c>, 673 linhas em 91 carteiras). A carteira 6, por
/// exemplo, atende Cajuru, Cássia dos Coqueiros, Santa Cruz da Esperança e Santo Antônio da
/// Alegria, em SP.</para>
///
/// <para><b>Não tem coluna de empresa, e é decisão.</b> A fronteira de multiempresa chega aqui
/// pela carteira, que tem <c>EmpresaId</c> e o filtro global — exatamente como em
/// <c>comercial.ClienteCarteira</c>, que é a ponte entre duas entidades que têm a coluna e por
/// isso não a repete. Copiar <c>EmpresaId</c> aqui criaria uma segunda verdade sobre de quem é a
/// linha, e uma segunda verdade que pode divergir da primeira é pior do que nenhuma.</para>
///
/// <para><b>Não herda <c>EntidadeBase</c>, mas tem trilha.</b> Mesma forma de
/// <c>ClienteCarteira</c>: quem vinculou, quando, e quando desvinculou. Tirar um município de uma
/// carteira é uma decisão comercial que muda a quem o cliente daquela cidade pertence — e ela
/// nunca apaga a linha, só a encerra.</para>
/// </summary>
public sealed class CarteiraMunicipio
{
    private CarteiraMunicipio() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A carteira. É por ela que a fronteira de filial chega a esta linha.</summary>
    public long CarteiraId { get; private set; }

    /// <summary>O município atendido.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>Quando o município entrou na carteira (UTC).</summary>
    public DateTime VinculadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quem colocou o município na carteira.</summary>
    public long VinculadoPorId { get; private set; }

    /// <summary>Quando o município saiu da carteira. Nulo é vínculo vigente.</summary>
    public DateTime? DesvinculadoEm { get; private set; }

    /// <summary>Coloca um município na carteira.</summary>
    /// <param name="carteiraId">A carteira.</param>
    /// <param name="municipioId">O município.</param>
    /// <param name="vinculadoPorId">Quem vinculou.</param>
    /// <param name="vinculadoEmUtc">Quando o vínculo nasceu na origem.</param>
    public static CarteiraMunicipio Criar(
        long carteiraId, int municipioId, long vinculadoPorId, DateTime? vinculadoEmUtc = null) => new()
    {
        CarteiraId = carteiraId,
        MunicipioId = municipioId,
        VinculadoEm = vinculadoEmUtc ?? DateTime.UtcNow,
        VinculadoPorId = vinculadoPorId
    };

    /// <summary>Encerra o vínculo sem apagar a linha.</summary>
    /// <param name="quandoUtc">O instante do encerramento.</param>
    public void Desvincular(DateTime quandoUtc) => DesvinculadoEm ??= quandoUtc;

    /// <summary>Reabre um vínculo encerrado, quando a origem volta a declará-lo.</summary>
    public void Revincular() => DesvinculadoEm = null;
}
