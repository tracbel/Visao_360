using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O SEGMENTO DE UMA CULTURA — o agrupamento que o comercial usa para ler o mapa (issue 165).
///
/// <para>Os nove vieram do anexo 49C, e a classificação de cada produto da PAM está lá, marcada como
/// proposta a confirmar. O segmento não é botânica: ele agrupa o que compartilha <b>sistema de
/// máquinas</b> — por isso as oleaginosas ficam em <see cref="Graos"/>.</para>
/// </summary>
public enum SegmentoDaCultura
{
    /// <summary>Grãos e oleaginosas: soja, milho, amendoim, feijão, girassol, canola…</summary>
    Graos = 0,

    /// <summary>Cana-de-açúcar e cana para forragem.</summary>
    Cana = 1,

    /// <summary>Laranja, limão e tangerina.</summary>
    Citros = 2,

    /// <summary>Café, nas três linhas da classificação 782.</summary>
    Cafe = 3,

    /// <summary>Fruticultura, do abacate à uva.</summary>
    Fruticultura = 4,

    /// <summary>Olericultura: as hortaliças e as raízes de mesa.</summary>
    Olericultura = 5,

    /// <summary>Algodão herbáceo e arbóreo.</summary>
    Algodao = 6,

    /// <summary>Borracha, nos dois látices.</summary>
    Borracha = 7,

    /// <summary>Fibras, folhas, mandioca e temperos — o que não forma grupo de máquina próprio.</summary>
    Outros = 8
}

/// <summary>
/// UMA CULTURA DO CATÁLOGO — o que liga o vocabulário das cinco fontes (issue 165, anexo 49C).
///
/// <para><b>Por que existe.</b> "Café" é uma palavra escrita de cinco jeitos em cinco lugares: a PAM usa
/// o código da classificação 782, a CONAB de preços usa um <c>id_produto</c> próprio, a CONAB de custos
/// usa o texto do link na página, o SICOR usa outro código, e o front tinha lista fixa no código. Nenhum
/// deles é chave de nada sozinho — e é por isso que cultura nova exigia publicação.</para>
///
/// <para><b>A chave é o código da PAM, e ela mora na tabela de vínculo</b>
/// (<see cref="ProdutoDaPamNaCultura"/>): uma cultura pode ter mais de um produto — o café tem três, e só
/// o "Total" entra na soma da lavoura. É um para muitos, e a marca de quem entra na soma vive no
/// vínculo.</para>
///
/// <para><b>Vínculo ausente é ausência, não erro.</b> Nem toda cultura tem preço na CONAB ou série de
/// custo; a tela diz "sem preço da CONAB para esta cultura" em vez de mostrar vazio sem motivo.</para>
/// </summary>
public sealed class Cultura
{
    /// <summary>O tamanho dos textos livres do catálogo.</summary>
    public const int TamanhoDoTexto = 80;

    private Cultura() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O código estável, em caixa alta e sem acento — é por ele que a tela pede a cultura.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>O nome como a tela escreve.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>O segmento do comercial.</summary>
    public SegmentoDaCultura Segmento { get; private set; }

    /// <summary>A unidade em que o mercado negocia ("saca de 60 kg", "caixa de 40,8 kg", "tonelada").</summary>
    public string UnidadeComercial { get; private set; } = default!;

    /// <summary>
    /// Quantos quilos tem a unidade comercial.
    ///
    /// <para>É o fator que transforma o R$/kg da CONAB no preço da saca, e a tonelada da PAM na unidade de
    /// quem negocia. Guardá-lo aqui, e não no código, é o que faz cultura nova entrar pela tela.</para>
    /// </summary>
    public decimal QuilosPorUnidade { get; private set; }

    /// <summary>A fonte do preço: <c>CONAB</c> ou <c>SOCICANA</c>. Nulo quando nenhuma publica.</summary>
    public string? FonteDoPreco { get; private set; }

    /// <summary>O identificador do produto na fonte de preço (o <c>id_produto</c> da CONAB).</summary>
    public string? ProdutoDoPreco { get; private set; }

    /// <summary>O rótulo da série de custo da CONAB ("CAFÉ ARÁBICA"). Nulo quando não há série.</summary>
    public string? SerieDeCusto { get; private set; }

    /// <summary>Se a cultura aparece nas telas. Desligar não apaga: o histórico continua explicando o passado.</summary>
    public bool EstaAtiva { get; private set; }

    /// <summary>
    /// O LOCAL DA CONAB QUE É A REFERÊNCIA DE SÃO PAULO para esta cultura (D-P07, issue 159).
    ///
    /// <para><b>Em aberto.</b> A CONAB publica o custo em vários locais — o café em Franca, a cana em
    /// Piracicaba e em Penápolis —, e a margem muda com a escolha. Ninguém decidiu qual é "a referência",
    /// e por isso o campo nasce nulo: a margem sai <b>vazia com o motivo</b>, em vez de escolher um local
    /// por conta própria.</para>
    /// </summary>
    public string? LocalDeReferenciaDoCusto { get; private set; }

    /// <summary>
    /// A CAMADA DE CUSTO QUE A MARGEM USA (D-P07, issue 159); nula enquanto ninguém decide.
    ///
    /// <para>Operacional responde "a safra se paga?"; total responde "o negócio remunera o patrimônio?".
    /// São perguntas diferentes, e o número muda muito entre elas — por isso não há padrão.</para>
    /// </summary>
    public Mercado.CamadaDoCusto? CamadaDeCustoDaMargem { get; private set; }

    /// <summary>Registra uma cultura no catálogo.</summary>
    /// <param name="codigo">O código estável.</param>
    /// <param name="nome">O nome de exibição.</param>
    /// <param name="segmento">O segmento do comercial.</param>
    /// <param name="unidadeComercial">A unidade em que o mercado negocia.</param>
    /// <param name="quilosPorUnidade">Quantos quilos tem a unidade.</param>
    /// <param name="fonteDoPreco">CONAB, SOCICANA ou nulo.</param>
    /// <param name="produtoDoPreco">O identificador do produto na fonte de preço.</param>
    /// <param name="serieDeCusto">O rótulo da série de custo, quando existe.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando falta código, nome, unidade ou o par da fonte de preço.</exception>
    public static Cultura Registrar(
        string codigo,
        string nome,
        SegmentoDaCultura segmento,
        string unidadeComercial,
        decimal quilosPorUnidade,
        string? fonteDoPreco = null,
        string? produtoDoPreco = null,
        string? serieDeCusto = null)
    {
        var cultura = new Cultura { Codigo = ConferirCodigo(codigo), EstaAtiva = true };
        cultura.Redefinir(nome, segmento, unidadeComercial, quilosPorUnidade, fonteDoPreco, produtoDoPreco, serieDeCusto);
        return cultura;
    }

    /// <summary>Troca o que a tela edita. Devolve se alguma coisa mudou.</summary>
    /// <param name="nome">O nome de exibição.</param>
    /// <param name="segmento">O segmento.</param>
    /// <param name="unidadeComercial">A unidade comercial.</param>
    /// <param name="quilosPorUnidade">Quantos quilos tem a unidade.</param>
    /// <param name="fonteDoPreco">CONAB, SOCICANA ou nulo.</param>
    /// <param name="produtoDoPreco">O produto na fonte de preço.</param>
    /// <param name="serieDeCusto">A série de custo.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando falta nome ou unidade, ou o par da fonte de preço vem pela metade.</exception>
    public bool Redefinir(
        string nome,
        SegmentoDaCultura segmento,
        string unidadeComercial,
        decimal quilosPorUnidade,
        string? fonteDoPreco,
        string? produtoDoPreco,
        string? serieDeCusto)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioViolada("A cultura precisa de um nome para aparecer na tela.");

        if (string.IsNullOrWhiteSpace(unidadeComercial))
            throw new RegraDeNegocioViolada("A cultura precisa da unidade em que o mercado a negocia.");

        // ZERO OU NEGATIVO DIVIDIRIA O PREÇO POR NADA. O fator é o que transforma R$/kg em preço da saca.
        if (quilosPorUnidade <= 0)
            throw new RegraDeNegocioViolada("A unidade comercial tem peso maior que zero em quilos.");

        // FONTE E PRODUTO ANDAM JUNTOS: uma fonte sem produto não acha nada, e um produto sem fonte não
        // diz onde procurar. Os dois vazios significam "esta cultura não tem preço publicado".
        if (string.IsNullOrWhiteSpace(fonteDoPreco) != string.IsNullOrWhiteSpace(produtoDoPreco))
            throw new RegraDeNegocioViolada(
                "A fonte do preço e o produto dela andam juntos: informe os dois, ou nenhum dos dois.");

        var mudou = Nome != nome.Trim()
                    || Segmento != segmento
                    || UnidadeComercial != unidadeComercial.Trim()
                    || QuilosPorUnidade != quilosPorUnidade
                    || FonteDoPreco != Limpar(fonteDoPreco)
                    || ProdutoDoPreco != Limpar(produtoDoPreco)
                    || SerieDeCusto != Limpar(serieDeCusto);

        Nome = nome.Trim();
        Segmento = segmento;
        UnidadeComercial = unidadeComercial.Trim();
        QuilosPorUnidade = quilosPorUnidade;
        FonteDoPreco = Limpar(fonteDoPreco)?.ToUpperInvariant();
        ProdutoDoPreco = Limpar(produtoDoPreco);
        SerieDeCusto = Limpar(serieDeCusto);

        return mudou;
    }

    /// <summary>Liga ou desliga a cultura nas telas. Devolve se mudou.</summary>
    /// <param name="ativa">Se passa a aparecer.</param>
    public bool DefinirAtiva(bool ativa)
    {
        if (EstaAtiva == ativa) return false;
        EstaAtiva = ativa;
        return true;
    }

    /// <summary>
    /// Define a referência do custo desta cultura — o local da CONAB e a camada (D-P07, issue 159).
    ///
    /// <para><b>Os dois andam juntos.</b> Um local sem camada não diz qual número comparar, e uma camada
    /// sem local não diz de onde tirá-lo. Os dois nulos significam "ninguém decidiu ainda", e é assim que
    /// a cultura nasce.</para>
    /// </summary>
    /// <param name="local">O local de referência da CONAB, ou nulo.</param>
    /// <param name="camada">A camada de custo, ou nula.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando vem um sem o outro.</exception>
    public bool DefinirReferenciaDoCusto(string? local, Mercado.CamadaDoCusto? camada)
    {
        var limpo = Limpar(local);

        if ((limpo is null) != (camada is null))
            throw new RegraDeNegocioViolada(
                "O local de referência e a camada de custo andam juntos: informe os dois, ou nenhum dos dois.");

        if (LocalDeReferenciaDoCusto == limpo && CamadaDeCustoDaMargem == camada) return false;

        LocalDeReferenciaDoCusto = limpo;
        CamadaDeCustoDaMargem = camada;
        return true;
    }

    private static string ConferirCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new RegraDeNegocioViolada("A cultura precisa de um código estável.");

        return codigo.Trim().ToUpperInvariant();
    }

    private static string? Limpar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}

/// <summary>
/// UM PRODUTO DA PAM DENTRO DE UMA CULTURA (issue 165).
///
/// <para><b>Por que é tabela, e não coluna.</b> Uma cultura pode ter mais de um produto da classificação
/// 782: o café tem "Total", "Arábica" e "Canephora", e os três chegam na mesma resposta do SIDRA. Uma
/// coluna só guardaria um deles.</para>
///
/// <para><b><see cref="EntraNaSomaDaLavoura"/> é o que impede contar duas vezes.</b> O "Total" do café já
/// contém Arábica e Canephora; somar os três dobraria a área do café. A marca vive aqui, no vínculo, e
/// não numa lista de exceções escrita no código.</para>
///
/// <para><b>Um produto pertence a uma cultura só.</b> O índice único é sobre o código do produto, e não
/// sobre o par: o mesmo produto em duas culturas somaria a mesma terra duas vezes.</para>
/// </summary>
public sealed class ProdutoDaPamNaCultura
{
    private ProdutoDaPamNaCultura() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>A cultura.</summary>
    public int CulturaId { get; private set; }

    /// <summary>O código do produto na classificação 782.</summary>
    public int ProdutoCodigoIbge { get; private set; }

    /// <summary>O rótulo oficial do produto, como o IBGE o escreve.</summary>
    public string ProdutoNome { get; private set; } = default!;

    /// <summary>Se este produto entra na soma da lavoura — falso nos detalhados de um total.</summary>
    public bool EntraNaSomaDaLavoura { get; private set; }

    /// <summary>Liga um produto da PAM a uma cultura.</summary>
    /// <param name="culturaId">A cultura.</param>
    /// <param name="produtoCodigoIbge">O código na classificação 782.</param>
    /// <param name="produtoNome">O rótulo oficial.</param>
    /// <param name="entraNaSomaDaLavoura">Se entra na soma.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o código ou o rótulo não valem.</exception>
    public static ProdutoDaPamNaCultura Ligar(
        int culturaId, int produtoCodigoIbge, string produtoNome, bool entraNaSomaDaLavoura)
    {
        if (produtoCodigoIbge <= 0)
            throw new RegraDeNegocioViolada("Produto sem código do IBGE não é produto da classificação oficial.");

        if (string.IsNullOrWhiteSpace(produtoNome))
            throw new RegraDeNegocioViolada("O produto da PAM precisa do rótulo oficial dele.");

        return new ProdutoDaPamNaCultura
        {
            CulturaId = culturaId,
            ProdutoCodigoIbge = produtoCodigoIbge,
            ProdutoNome = produtoNome.Trim(),
            EntraNaSomaDaLavoura = entraNaSomaDaLavoura
        };
    }
}

/// <summary>
/// UMA CATEGORIA DE MÁQUINA (issue 165, D-IM-06).
///
/// <para><b>Por que existe.</b> O potencial é por cultura <b>e por categoria</b>: a mesma lavoura pede um
/// trator a cada tantos hectares e uma colheitadeira a cada outros tantos. Até aqui havia "máquinas
/// teóricas", sem dizer de quê.</para>
///
/// <para><b>Nem toda categoria existe no SICOR</b> (anexo 49C): o investimento do Banco Central tem
/// trator, "máquinas e implementos" e colheitadeiras, e <b>não tem</b> plantadeira nem pulverizador. A
/// categoria sem produto do SICOR existe do mesmo jeito — ela só não aparece no crédito.</para>
/// </summary>
public sealed class CategoriaDeMaquina
{
    private CategoriaDeMaquina() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O código estável, em caixa alta e sem acento.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>O nome como a tela escreve.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>A ordem de exibição — a lista tem sentido de leitura, e não é alfabética.</summary>
    public short Ordem { get; private set; }

    /// <summary>Se aparece nas telas.</summary>
    public bool EstaAtiva { get; private set; }

    /// <summary>Registra uma categoria.</summary>
    /// <param name="codigo">O código estável.</param>
    /// <param name="nome">O nome de exibição.</param>
    /// <param name="ordem">A ordem de exibição.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando falta código ou nome.</exception>
    public static CategoriaDeMaquina Registrar(string codigo, string nome, short ordem)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new RegraDeNegocioViolada("A categoria de máquina precisa de um código estável.");

        var categoria = new CategoriaDeMaquina { Codigo = codigo.Trim().ToUpperInvariant(), EstaAtiva = true };
        categoria.Redefinir(nome, ordem);
        return categoria;
    }

    /// <summary>Troca o que a tela edita. Devolve se mudou.</summary>
    /// <param name="nome">O nome de exibição.</param>
    /// <param name="ordem">A ordem.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando falta o nome.</exception>
    public bool Redefinir(string nome, short ordem)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioViolada("A categoria de máquina precisa de um nome para aparecer na tela.");

        var mudou = Nome != nome.Trim() || Ordem != ordem;
        Nome = nome.Trim();
        Ordem = ordem;
        return mudou;
    }

    /// <summary>Liga ou desliga a categoria. Devolve se mudou.</summary>
    /// <param name="ativa">Se passa a aparecer.</param>
    public bool DefinirAtiva(bool ativa)
    {
        if (EstaAtiva == ativa) return false;
        EstaAtiva = ativa;
        return true;
    }
}

/// <summary>
/// UM PRODUTO DO SICOR DENTRO DE UMA CATEGORIA DE MÁQUINA (issue 165).
///
/// <para>É o que tira a lista <c>[7080, 4860, 2700]</c> do código, onde a issue 157 a deixou de passagem:
/// "o que conta como máquina" passa a ser cadastro, com autor e trilha.</para>
///
/// <para><b>Um produto do SICOR pertence a uma categoria só</b> — o índice único é sobre o código do
/// produto. O mesmo produto em duas categorias contaria o mesmo financiamento duas vezes.</para>
/// </summary>
public sealed class ProdutoDoSicorNaCategoria
{
    private ProdutoDoSicorNaCategoria() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>A categoria de máquina.</summary>
    public int CategoriaDeMaquinaId { get; private set; }

    /// <summary>O código do produto no SICOR (7080 é trator).</summary>
    public int CodigoProduto { get; private set; }

    /// <summary>A descrição do produto, como o Banco Central a publica.</summary>
    public string Descricao { get; private set; } = default!;

    /// <summary>Liga um produto do SICOR a uma categoria.</summary>
    /// <param name="categoriaDeMaquinaId">A categoria.</param>
    /// <param name="codigoProduto">O código no SICOR.</param>
    /// <param name="descricao">A descrição do Banco Central.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o código ou a descrição não valem.</exception>
    public static ProdutoDoSicorNaCategoria Ligar(int categoriaDeMaquinaId, int codigoProduto, string descricao)
    {
        if (codigoProduto <= 0)
            throw new RegraDeNegocioViolada("Produto sem código do SICOR não é produto da tabela oficial.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new RegraDeNegocioViolada("O produto do SICOR precisa da descrição do Banco Central.");

        return new ProdutoDoSicorNaCategoria
        {
            CategoriaDeMaquinaId = categoriaDeMaquinaId,
            CodigoProduto = codigoProduto,
            Descricao = descricao.Trim()
        };
    }
}

/// <summary>
/// UMA CLASSIFICAÇÃO DE PRODUTO DO CRM DENTRO DE UMA CATEGORIA DE MÁQUINA (issue 69, D-P08).
///
/// <para><b>É o último elo entre a venda e a categoria.</b> A cadeia já existia inteira e parava aqui:</para>
///
/// <code>
/// ART (linha) → ClassificacaoDoArt → frota.LinhaDeProduto → ??? → organizacao.CategoriaDeMaquina
/// </code>
///
/// <para>O <c>ClassificacaoDoArt</c> já traduz as catorze linhas do ART para a classificação de produto do
/// CRM — <c>TRATOR_MEDIO</c>, <c>COLHEDORA_DE_CANA</c>, <c>PLANTADEIRA</c>… —, e o <c>CargaDoArt</c> já
/// grava isso em cada equipamento. O que faltava era dizer a qual <b>categoria de mercado</b> cada uma
/// pertence, e sem isso a captura não podia ser lida por categoria (acréscimo ao aceite da issue 69).</para>
///
/// <para><b>A chave é o CÓDIGO da linha, e não o id</b>, pelo mesmo motivo do de-para do SICOR: as linhas
/// de produto nascem da carga, com o código derivado do texto da origem, e o id só existe depois que ela
/// roda. Semear por id seria semear um número que ainda não existe.</para>
///
/// <para><b>Uma linha pertence a uma categoria só.</b> A mesma linha em duas categorias contaria a mesma
/// venda duas vezes na captura.</para>
///
/// <para><b>Linha sem categoria não é erro, é pendência</b>: a venda continua gravada e contada no total,
/// e some apenas da leitura POR categoria — com o nome da linha dito na tela, para alguém decidir.</para>
/// </summary>
public sealed class LinhaDeProdutoNaCategoria
{
    private LinhaDeProdutoNaCategoria() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>A categoria de máquina.</summary>
    public int CategoriaDeMaquinaId { get; private set; }

    /// <summary>O código da classificação de produto do CRM — <c>TRATOR_MEDIO</c>, <c>PLANTADEIRA</c>…</summary>
    public string CodigoDaLinha { get; private set; } = default!;

    /// <summary>O nome da linha, como a tela a mostra.</summary>
    public string Descricao { get; private set; } = default!;

    /// <summary>Liga uma classificação de produto a uma categoria de máquina.</summary>
    /// <param name="categoriaDeMaquinaId">A categoria.</param>
    /// <param name="codigoDaLinha">O código da classificação de produto do CRM.</param>
    /// <param name="descricao">O nome da linha.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o código ou a descrição não valem.</exception>
    public static LinhaDeProdutoNaCategoria Ligar(int categoriaDeMaquinaId, string codigoDaLinha, string descricao)
    {
        if (string.IsNullOrWhiteSpace(codigoDaLinha))
            throw new RegraDeNegocioViolada("A ligação precisa do código da classificação de produto do CRM.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new RegraDeNegocioViolada("A ligação precisa do nome da linha.");

        return new LinhaDeProdutoNaCategoria
        {
            CategoriaDeMaquinaId = categoriaDeMaquinaId,
            CodigoDaLinha = codigoDaLinha.Trim().ToUpperInvariant(),
            Descricao = descricao.Trim()
        };
    }
}

/// <summary>
/// UM GRUPO DE CULTURAS QUE COMPARTILHAM A MESMA TERRA E A MESMA MÁQUINA (issue 160, D-IM-01).
///
/// <para><b>Por que existe.</b> O milho safrinha é plantado depois da soja, no mesmo talhão, e boa parte
/// do amendoim entra em reforma de canavial. Somar as áreas conta terra que não existe, e o parque
/// teórico sai inflado justamente onde a rotação é mais comum.</para>
///
/// <para><b>O grupo é por CATEGORIA DE MÁQUINA.</b> Soja e milho dividem a plantadeira e o trator, e não
/// a colheitadeira — que é outra máquina em cada cultura. Um grupo único por cultura misturaria as duas
/// coisas.</para>
///
/// <para><b>Nasce vazio, de propósito.</b> Quais culturas compartilham é a decisão D-IM-01, e ela não
/// saiu. Sem grupo configurado, cada cultura soma a área dela — exatamente como era antes desta issue.</para>
/// </summary>
public sealed class GrupoDeCompartilhamento
{
    private GrupoDeCompartilhamento() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O código estável do grupo.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>O nome que a tela mostra ("Soja e milho safrinha").</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>A categoria de máquina a que o compartilhamento se aplica.</summary>
    public int CategoriaDeMaquinaId { get; private set; }

    /// <summary>Se o grupo vale. Desligado, as culturas dele voltam a somar separadas.</summary>
    public bool EstaAtivo { get; private set; }

    /// <summary>Registra um grupo.</summary>
    /// <param name="codigo">O código estável.</param>
    /// <param name="nome">O nome de exibição.</param>
    /// <param name="categoriaDeMaquinaId">A categoria a que ele se aplica.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando falta código ou nome.</exception>
    public static GrupoDeCompartilhamento Registrar(string codigo, string nome, int categoriaDeMaquinaId)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new RegraDeNegocioViolada("O grupo de compartilhamento precisa de um código estável.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioViolada("O grupo de compartilhamento precisa de um nome para a tela.");

        return new GrupoDeCompartilhamento
        {
            Codigo = codigo.Trim().ToUpperInvariant(),
            Nome = nome.Trim(),
            CategoriaDeMaquinaId = categoriaDeMaquinaId,
            EstaAtivo = true
        };
    }

    /// <summary>Liga ou desliga o grupo. Devolve se mudou.</summary>
    /// <param name="ativo">Se passa a valer.</param>
    public bool DefinirAtivo(bool ativo)
    {
        if (EstaAtivo == ativo) return false;
        EstaAtivo = ativo;
        return true;
    }
}

/// <summary>
/// UMA CULTURA DENTRO DE UM GRUPO DE COMPARTILHAMENTO (issue 160).
///
/// <para><b>Uma cultura entra em um grupo por categoria</b> — o índice único é sobre o par grupo ×
/// cultura, e a regra de "uma cultura num grupo só por categoria" é conferida na gravação, porque
/// depende da categoria do grupo.</para>
/// </summary>
public sealed class CulturaNoGrupoDeCompartilhamento
{
    private CulturaNoGrupoDeCompartilhamento() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O grupo.</summary>
    public int GrupoDeCompartilhamentoId { get; private set; }

    /// <summary>A cultura.</summary>
    public int CulturaId { get; private set; }

    /// <summary>Põe uma cultura num grupo.</summary>
    /// <param name="grupoId">O grupo.</param>
    /// <param name="culturaId">A cultura.</param>
    public static CulturaNoGrupoDeCompartilhamento Ligar(int grupoId, int culturaId) => new()
    {
        GrupoDeCompartilhamentoId = grupoId,
        CulturaId = culturaId
    };
}

/// <summary>
/// AS SEIS CULTURAS E AS SEIS CATEGORIAS QUE A MIGRAÇÃO SEMEIA — o conteúdo do anexo 49C, medido nas
/// fontes em 22/09/2026 (issue 165).
///
/// <para><b>Por que semear, e não deixar o administrador cadastrar do zero.</b> As seis já estão nas
/// telas de preço e de custo, com lista fixa no código; a semente é o que permite tirar a lista de lá sem
/// a tela ficar vazia no dia da publicação. A partir daqui, cultura nova entra pela tela.</para>
/// </summary>
public static class CatalogoSemeado
{
    /// <summary>Uma cultura da semente, com os produtos da PAM que a compõem.</summary>
    /// <param name="Codigo">O código estável.</param>
    /// <param name="Nome">O nome de exibição.</param>
    /// <param name="Segmento">O segmento.</param>
    /// <param name="UnidadeComercial">A unidade em que o mercado negocia.</param>
    /// <param name="QuilosPorUnidade">Quantos quilos tem a unidade.</param>
    /// <param name="FonteDoPreco">CONAB ou SOCICANA.</param>
    /// <param name="ProdutoDoPreco">O identificador na fonte de preço.</param>
    /// <param name="SerieDeCusto">O rótulo da série de custo da CONAB.</param>
    /// <param name="Produtos">Os produtos da PAM: código, nome e se entra na soma.</param>
    public sealed record CulturaSemeada(
        string Codigo,
        string Nome,
        SegmentoDaCultura Segmento,
        string UnidadeComercial,
        decimal QuilosPorUnidade,
        string? FonteDoPreco,
        string? ProdutoDoPreco,
        string? SerieDeCusto,
        IReadOnlyList<(int Codigo, string Nome, bool NaSoma)> Produtos);

    /// <summary>Uma categoria da semente, com os produtos do SICOR que ela agrupa.</summary>
    /// <param name="Codigo">O código estável.</param>
    /// <param name="Nome">O nome de exibição.</param>
    /// <param name="Ordem">A ordem de exibição.</param>
    /// <param name="ProdutosDoSicor">Os produtos do SICOR: código e descrição.</param>
    public sealed record CategoriaSemeada(
        string Codigo,
        string Nome,
        short Ordem,
        IReadOnlyList<(int Codigo, string Descricao)> ProdutosDoSicor);

    /// <summary>
    /// As seis culturas do pedido, com o vínculo de cada fonte medido no anexo 49C.
    ///
    /// <para><b>O café entra na soma só pelo "Total" (40139)</b>: Arábica e Canephora ficam no catálogo
    /// para quem quiser o detalhe, com <c>NaSoma</c> falso — somar os três dobraria a área do café.</para>
    /// </summary>
    public static readonly IReadOnlyList<CulturaSemeada> Culturas =
    [
        new("CAFE", "Café", SegmentoDaCultura.Cafe, "saca de 60 kg", 60m, "CONAB", "11195", "CAFÉ ARÁBICA",
        [
            (40139, "Café (em grão) Total", true),
            (40140, "Café (em grão) Arábica", false),
            (40141, "Café (em grão) Canephora", false)
        ]),
        new("CANA", "Cana-de-açúcar", SegmentoDaCultura.Cana, "tonelada", 1_000m, "CONAB", "4238", "CANA DE AÇÚCAR",
        [
            (40106, "Cana-de-açúcar", true)
        ]),
        new("SOJA", "Soja", SegmentoDaCultura.Graos, "saca de 60 kg", 60m, "CONAB", "4744", "SOJA",
        [
            (40124, "Soja (em grão)", true)
        ]),
        new("MILHO", "Milho", SegmentoDaCultura.Graos, "saca de 60 kg", 60m, "CONAB", "4742", "MILHO",
        [
            (40122, "Milho (em grão)", true)
        ]),
        new("LARANJA", "Laranja", SegmentoDaCultura.Citros, "caixa de 40,8 kg", 40.8m, "CONAB", "12290", "LARANJA",
        [
            (40151, "Laranja", true)
        ]),
        new("AMENDOIM", "Amendoim", SegmentoDaCultura.Graos, "saca de 25 kg", 25m, "CONAB", "4674", "AMENDOIM",
        [
            (40101, "Amendoim (em casca)", true)
        ])
    ];

    /// <summary>
    /// As seis categorias de máquina (D-IM-06), com os produtos do SICOR que existem para cada uma.
    ///
    /// <para><b>Três categorias nascem sem produto do SICOR</b> — plantadeira, pulverizador e agricultura
    /// de precisão —, porque o investimento do Banco Central não as separa (anexo 49C). Elas existem do
    /// mesmo jeito: o parque e a demanda são por categoria, e só o crédito é que não as enxerga.</para>
    /// </summary>
    public static readonly IReadOnlyList<CategoriaSemeada> Categorias =
    [
        new("TRATOR", "Trator", 1, [(7080, "TRATOR")]),
        new("PLANTADEIRA", "Plantadeira", 2, []),
        new("COLHEITADEIRA", "Colheitadeira", 3, [(2700, "COLHEITADEIRAS, COLHEDEIRAS E ARRANCADEIRAS")]),
        new("PULVERIZADOR", "Pulverizador", 4, []),
        new("IMPLEMENTO", "Implemento", 5, [(4860, "MÁQUINAS E IMPLEMENTOS")]),
        new("PRECISAO", "Agricultura de precisão", 6, [])
    ];
}
