using System.Globalization;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// DE QUE PLANEJAMENTO A META VEIO, como a API Gestão de Negócios escreve.
///
/// <para><b>Consórcio é à parte</b> (D-M4, 27/09/2026): a meta de consórcio é em COTAS, e o CRM não mede cota vendida —
/// o realizado dela é "não medido pelo CRM". Ela não soma com a meta de máquinas.</para>
/// </summary>
public enum OrigemDaMeta
{
    /// <summary>A campanha anual de máquinas — a planilha "Divisão Time - Campanha Anual", hoje o cadastro da GN.</summary>
    Campanha = 0,

    /// <summary>O planejamento de consórcio, em cotas.</summary>
    Consorcio = 1
}

/// <summary>O que a meta traz da origem, já saneado e casado com o CRM.</summary>
/// <param name="EmpresaId">A filial da meta (<c>0101NN</c>) — a fronteira de acesso.</param>
/// <param name="Competencia">O mês, no dia 1.</param>
/// <param name="LinhaNaOrigem">A linha como a GN escreve.</param>
/// <param name="CodigoDaLinha">O código estável da linha — o mesmo algoritmo do ART.</param>
/// <param name="LinhaDeProdutoId">A classificação de produto do CRM; nula em CONSÓRCIO e USADOS, que não são categoria.</param>
/// <param name="ConsultorNaOrigem">O consultor como a GN escreve, em maiúsculas.</param>
/// <param name="ConsultorUsuarioId">A conta do CRM cujo login é o consultor; nula quando não há conta.</param>
/// <param name="VendaDireta">Tipo Direta; falso é Concessão.</param>
/// <param name="Origem">Campanha ou Consórcio.</param>
/// <param name="Quantidade">A meta em unidades (cotas, no consórcio).</param>
/// <param name="ValorUnitario">O valor unitário planejado.</param>
/// <param name="Margem">A margem planejada.</param>
/// <param name="HashDaOrigem">O resumo do conteúdo lido — é o que a recarga compara.</param>
public sealed record DadosDaMetaNaOrigem(
    int EmpresaId,
    DateOnly Competencia,
    string LinhaNaOrigem,
    string CodigoDaLinha,
    int? LinhaDeProdutoId,
    string ConsultorNaOrigem,
    long? ConsultorUsuarioId,
    bool VendaDireta,
    OrigemDaMeta Origem,
    int Quantidade,
    decimal? ValorUnitario,
    decimal? Margem,
    string HashDaOrigem);

/// <summary>A leitura que trouxe o conteúdo — o frescor da linha.</summary>
/// <param name="LidaEm">Quando o CRM leu (UTC).</param>
/// <param name="GeradaNaOrigemEm">Quando a API gerou a resposta (UTC), se disse.</param>
/// <param name="IdadeNaOrigemSegundos">A idade do espelho, quando a rota é espelho; nula no cadastro de metas.</param>
public sealed record LeituraDaMeta(DateTime LidaEm, DateTime? GeradaNaOrigemEm, int? IdadeNaOrigemSegundos);

/// <summary>
/// UMA META DE VENDA — a cota da API Gestão de Negócios, em unidades, por consultor, linha, mês e filial (decisão do
/// Ricardo de 27/09/2026, issue 138).
///
/// <para><b>O grão é o <c>id</c> da GN</b>: uma linha aqui por linha lá. A chave de negócio (mês, filial, linha, consultor,
/// tipo, origem) NÃO é única na origem — 83 grupos têm 187 linhas que se somam —, então as duplicatas de negócio só se
/// SOMAM na leitura; aqui nenhuma é descartada nem fundida.</para>
///
/// <para><b>O CRM não edita a meta.</b> Ela é espelho de um cadastro de outra equipe: muda quando a GN muda, e a trilha
/// de auditoria guarda o antes e o depois de cada revisão. A linha que some da origem é EXCLUÍDA logicamente, nunca
/// apagada; se voltar, é reativada — o histórico da trilha continua na mesma linha.</para>
///
/// <para><b>O que a meta não guarda:</b> o nome completo do consultor nem dado pessoal além do login da GN, que é o
/// que liga a meta à conta do CRM.</para>
/// </summary>
public sealed class MetaDeVenda : EntidadeBase
{
    /// <summary>O tamanho da coluna da linha.</summary>
    public const int TamanhoDaLinha = 60;

    /// <summary>O tamanho da coluna do consultor.</summary>
    public const int TamanhoDoConsultor = 80;

    /// <summary>
    /// O fluxo da carga das metas — a trava, o ponto de sincronismo (o frescor que a tela mostra) e a fila de descarte.
    /// Mora aqui porque a carga grava e a leitura lê o mesmo ponto.
    /// </summary>
    public const string FluxoDaCarga = "GESTAO_NEGOCIOS.METAS";

    /// <summary>
    /// O fluxo das vendas do ART na trilha da origem — o mesmo da carga do ART. As pendentes dele são a lacuna do realizado
    /// (D-M3): vendas que o ART tem e o CRM ainda não, por cadastro ou chassi.
    /// </summary>
    public const string FluxoDasVendasDoArt = "ART.VENDA_DE_MAQUINA";

    /// <summary>O código da linha de consórcio.</summary>
    public const string CodigoDaLinhaDeConsorcio = "CONSORCIO";

    private MetaDeVenda() { }

    /// <summary>
    /// A META DE CONSÓRCIO fica à parte (D-M4): pela origem ou pela linha CONSÓRCIO. As duas marcas vieram juntas nas
    /// 266 linhas medidas; a regra aceita qualquer uma, para uma linha de consórcio nunca cair na soma de máquinas. Uma regra
    /// só, para a carga e a leitura.
    /// </summary>
    /// <param name="origem">A origem da meta.</param>
    /// <param name="codigoDaLinha">O código da linha.</param>
    public static bool EhConsorcio(OrigemDaMeta origem, string codigoDaLinha) =>
        origem == OrigemDaMeta.Consorcio || codigoDaLinha == CodigoDaLinhaDeConsorcio;

    /// <summary>A filial da meta — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O sistema de onde a meta veio (<c>GESTAO_NEGOCIOS</c>).</summary>
    public int SistemaId { get; private set; }

    /// <summary>O <c>id</c> da linha na GN — a chave natural.</summary>
    public int IdNaOrigem { get; private set; }

    /// <summary>O mês da meta, no dia 1.</summary>
    public DateOnly Competencia { get; private set; }

    /// <summary>A linha como a GN escreve.</summary>
    public string LinhaNaOrigem { get; private set; } = default!;

    /// <summary>O código estável da linha — o mesmo algoritmo do ART, para o realizado casar por código.</summary>
    public string CodigoDaLinha { get; private set; } = default!;

    /// <summary>A classificação de produto do CRM; nula em CONSÓRCIO e USADOS.</summary>
    public int? LinhaDeProdutoId { get; private set; }

    /// <summary>O consultor como a GN escreve (<c>NOME.SOBRENOME</c>).</summary>
    public string ConsultorNaOrigem { get; private set; } = default!;

    /// <summary>A conta do CRM do consultor (login = o consultor em minúsculas); nula quando ele não tem conta.</summary>
    public long? ConsultorUsuarioId { get; private set; }

    /// <summary>Tipo Direta; falso é Concessão.</summary>
    public bool VendaDireta { get; private set; }

    /// <summary>Campanha ou Consórcio.</summary>
    public OrigemDaMeta Origem { get; private set; }

    /// <summary>A meta em unidades — cotas, no consórcio.</summary>
    public int Quantidade { get; private set; }

    /// <summary>O valor unitário planejado.</summary>
    public decimal? ValorUnitario { get; private set; }

    /// <summary>A margem planejada.</summary>
    public decimal? Margem { get; private set; }

    /// <summary>O resumo do conteúdo da última leitura que mudou a meta.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>Quando a meta entrou no CRM (UTC).</summary>
    public DateTime ImportadaEm { get; private set; }

    /// <summary>A última vez que a origem mudou esta meta (UTC).</summary>
    public DateTime? AtualizadaPelaOrigemEm { get; private set; }

    /// <summary>A leitura que trouxe o conteúdo atual (UTC). A leitura sem mudança não regrava a linha.</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>Quando a API gerou a resposta dessa leitura (UTC).</summary>
    public DateTime? GeradaNaOrigemEm { get; private set; }

    /// <summary>A idade do espelho nessa leitura; nula no cadastro de metas, que não é espelho.</summary>
    public int? IdadeNaOrigemSegundos { get; private set; }

    /// <summary>Registra uma meta lida da origem.</summary>
    /// <param name="sistemaId">O sistema de origem.</param>
    /// <param name="idNaOrigem">O id na GN.</param>
    /// <param name="dados">O conteúdo saneado e casado.</param>
    /// <param name="leitura">A leitura que o trouxe.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando a competência não é dia 1, a quantidade é negativa ou falta linha ou consultor.</exception>
    public static MetaDeVenda Registrar(int sistemaId, int idNaOrigem, DadosDaMetaNaOrigem dados, LeituraDaMeta leitura, long criadoPorId)
    {
        if (idNaOrigem <= 0) throw new RegraDeNegocioViolada("A meta de origem precisa do id da origem, positivo.");

        var meta = new MetaDeVenda
        {
            SistemaId = sistemaId,
            IdNaOrigem = idNaOrigem,
            ImportadaEm = leitura.LidaEm,
            CriadoPorId = criadoPorId
        };

        meta.Aplicar(dados, leitura);
        return meta;
    }

    /// <summary>
    /// Aplica uma nova leitura. Nada muda quando o resumo do conteúdo é o mesmo — é isso que faz a segunda leitura igual
    /// gravar zero. Cada campo mudado vai para a trilha no <c>SaveChanges</c>.
    /// </summary>
    /// <param name="dados">O conteúdo da nova leitura.</param>
    /// <param name="leitura">A leitura.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AtualizarDaOrigem(DadosDaMetaNaOrigem dados, LeituraDaMeta leitura, long usuarioId)
    {
        if (dados.HashDaOrigem == HashDaOrigem && dados.EmpresaId == EmpresaId && dados.LinhaDeProdutoId == LinhaDeProdutoId
            && dados.ConsultorUsuarioId == ConsultorUsuarioId)
            return false;

        Aplicar(dados, leitura);
        AtualizadaPelaOrigemEm = leitura.LidaEm;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// Traz de volta a meta que tinha sumido da origem e reapareceu com o mesmo id. A linha é a mesma — a trilha da
    /// exclusão e da volta fica junto.
    /// </summary>
    /// <param name="usuarioId">Quem roda a integração.</param>
    /// <returns>Verdadeiro quando estava excluída.</returns>
    public bool Reativar(long usuarioId)
    {
        if (!EstaExcluido) return false;
        ExcluidoEm = null;
        MarcarAlteracao(usuarioId);
        return true;
    }

    private void Aplicar(DadosDaMetaNaOrigem dados, LeituraDaMeta leitura)
    {
        if (dados.Competencia.Day != 1)
            throw new RegraDeNegocioViolada(
                $"A competência da meta é o mês, no dia 1 — veio {dados.Competencia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.");
        if (dados.Quantidade < 0) throw new RegraDeNegocioViolada("A meta não é negativa.");
        if (string.IsNullOrWhiteSpace(dados.LinhaNaOrigem) || dados.LinhaNaOrigem.Length > TamanhoDaLinha
            || string.IsNullOrWhiteSpace(dados.CodigoDaLinha) || dados.CodigoDaLinha.Length > TamanhoDaLinha)
            throw new RegraDeNegocioViolada("A meta preserva a linha da origem, de 1 a 60 caracteres.");
        if (string.IsNullOrWhiteSpace(dados.ConsultorNaOrigem) || dados.ConsultorNaOrigem.Length > TamanhoDoConsultor)
            throw new RegraDeNegocioViolada("A meta preserva o consultor da origem, de 1 a 80 caracteres.");
        if (string.IsNullOrWhiteSpace(dados.HashDaOrigem)) throw new RegraDeNegocioViolada("A meta guarda o resumo do conteúdo lido.");

        EmpresaId = dados.EmpresaId;
        Competencia = dados.Competencia;
        LinhaNaOrigem = dados.LinhaNaOrigem;
        CodigoDaLinha = dados.CodigoDaLinha;
        LinhaDeProdutoId = dados.LinhaDeProdutoId;
        ConsultorNaOrigem = dados.ConsultorNaOrigem;
        ConsultorUsuarioId = dados.ConsultorUsuarioId;
        VendaDireta = dados.VendaDireta;
        Origem = dados.Origem;
        Quantidade = dados.Quantidade;
        ValorUnitario = dados.ValorUnitario;
        Margem = dados.Margem;
        HashDaOrigem = dados.HashDaOrigem;
        LidaEm = leitura.LidaEm;
        GeradaNaOrigemEm = leitura.GeradaNaOrigemEm;
        IdadeNaOrigemSegundos = leitura.IdadeNaOrigemSegundos;
    }
}
