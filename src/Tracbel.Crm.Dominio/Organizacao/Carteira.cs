using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O que a carteira É — e nem toda carteira do legado é carteira de venda.
///
/// <para><b>Por que isto existe:</b> das 142 carteiras carregadas, 14 não são carteira comercial.
/// Há depósito de cadastro ("CADASTROS INATIVOS TRACBEL AGRO", "FORA ATUAÇÃO", "COL_FUNC"),
/// resíduo de teste de sistema ("TESTE APP MOBILE LITE", "TESTE_DSI", "TI_RPO" da consultoria) e
/// carteira de departamento. Misturadas com as de CEN, elas contaminam toda métrica de gerência:
/// a MAIOR carteira do sistema — 5.000 clientes — é da Inteligência de Mercado, e por causa dela
/// um departamento aparecia como o CEN número 1 do ranking da diretoria.</para>
///
/// <para><b>Classificar, e não apagar.</b> Os vínculos existem e o histórico é real; o que estava
/// errado era somá-los como se fossem carteira de venda. Com a natureza declarada, a tela de
/// gerência filtra e o número passa a significar o que ele diz — e o dado continua lá para quem
/// precisar dele.</para>
/// </summary>
public enum NaturezaDaCarteira
{
    /// <summary>Carteira de CEN: cliente atribuído a alguém para vender.</summary>
    Comercial = 0,

    /// <summary>Depósito de cadastro: inativos, fora de atuação, colaboradores.</summary>
    Administrativa = 1,

    /// <summary>Resíduo de teste de sistema ou de implantação do fornecedor.</summary>
    Teste = 2
}

/// <summary>
/// O conjunto de clientes de um CEN numa linha de negócio.
///
/// [V] A carteirização multi-linha é o melhor ativo do modelo antigo, e nem Salesforce nem
/// Dynamics fazem isso nativamente: um cliente pertence a várias carteiras ao mesmo tempo,
/// uma por linha de negócio, com classe diferente em cada.
/// </summary>
public sealed class Carteira : EntidadeBase
{
    private Carteira() { }

    /// <summary>Filial dona da carteira.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Linha de negócio da carteira.</summary>
    public int LinhaDeNegocioId { get; private set; }

    /// <summary>Praça de mercado da carteira, quando aplicável.</summary>
    public int? PracaId { get; private set; }

    /// <summary>Código estável. Ex.: <c>MAQ_13SJRP_01</c>.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>
    /// O CEN responsável. Aponta para o usuário, e ponto — [V] no Vórtice apontava para um
    /// cadastro paralelo de vendedor, fonte constante de confusão.
    /// </summary>
    public long ResponsavelId { get; private set; }

    /// <summary>Supervisor da carteira, quando existe.</summary>
    public long? SupervisorId { get; private set; }

    /// <summary>Equipe dona da carteira, quando a carteira é de time e não de pessoa.</summary>
    public long? EquipeId { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>
    /// Se esta carteira é de venda, de cadastro ou resíduo de teste.
    ///
    /// <para>Nasce classificada pela carga, com regra e evidência — nunca por lista escrita à
    /// mão. O negócio pode corrigir depois pela tela de administração de carteira: a
    /// classificação é uma leitura, e leitura se revê.</para>
    /// </summary>
    public NaturezaDaCarteira Natureza { get; private set; } = NaturezaDaCarteira.Comercial;

    /// <summary>Cria uma carteira.</summary>
    /// <param name="empresaId">Filial dona.</param>
    /// <param name="linhaDeNegocioId">Linha de negócio.</param>
    /// <param name="codigo">Código estável.</param>
    /// <param name="nome">Nome legível.</param>
    /// <param name="responsavelId">O CEN responsável.</param>
    /// <param name="criadoPorId">Quem criou.</param>
    /// <param name="estaAtiva">Se a carteira está em operação.</param>
    /// <param name="natureza">Se é carteira de venda, de cadastro ou de teste.</param>
    public static Carteira Criar(
        int empresaId,
        int linhaDeNegocioId,
        string codigo,
        string nome,
        long responsavelId,
        long criadoPorId,
        bool estaAtiva = true,
        NaturezaDaCarteira natureza = NaturezaDaCarteira.Comercial) => new()
    {
        Natureza = natureza,
        EmpresaId = empresaId,
        LinhaDeNegocioId = linhaDeNegocioId,
        Codigo = codigo,
        Nome = nome,
        ResponsavelId = responsavelId,
        EstaAtiva = estaAtiva,
        CriadoPorId = criadoPorId
    };

    /// <summary>
    /// Declara o que a carteira é — de venda, de cadastro ou de teste.
    ///
    /// <para>É método próprio, e não parâmetro de <see cref="Alterar"/>, porque é outra decisão:
    /// trocar o responsável de uma carteira é operação de rotina; dizer que ela não é carteira
    /// de venda muda de que números ela participa. Separado, aparece separado na auditoria.</para>
    /// </summary>
    /// <param name="natureza">O que a carteira é.</param>
    /// <param name="usuarioId">Quem classificou.</param>
    public void Classificar(NaturezaDaCarteira natureza, long usuarioId)
    {
        if (Natureza == natureza) return;

        Natureza = natureza;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>Troca o responsável e o nome da carteira, na reconciliação da carga.</summary>
    /// <param name="nome">Nome legível.</param>
    /// <param name="responsavelId">O CEN responsável.</param>
    /// <param name="linhaDeNegocioId">Linha de negócio.</param>
    /// <param name="usuarioId">Quem alterou.</param>
    public void Alterar(string nome, long responsavelId, int linhaDeNegocioId, long usuarioId)
    {
        Nome = nome;
        ResponsavelId = responsavelId;
        LinhaDeNegocioId = linhaDeNegocioId;
        MarcarAlteracao(usuarioId);
    }
}
