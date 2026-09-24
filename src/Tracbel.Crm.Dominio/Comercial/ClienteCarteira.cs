using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>
/// A CLASSE DO CLIENTE — apurada do faturamento, e não digitada por alguém.
///
/// ---------------------------------------------------------------------------------------------
/// POR QUE ELA É APURADA. A classe A/B/C/D é o eixo de toda métrica de cobertura que a gerência
/// pede: quantos clientes A estão sem visita, quantos D o CEN atendeu. O protótipo mostrava essa
/// classe e o legado parecia tê-la — mas não tem. Medido no Vórtice:
///
///  - <c>IVS_Pes.Potencial</c>: 134.757 de 139.072 vínculos em branco. Dos preenchidos, a maioria
///    guarda <c>64</c>, <c>43</c>, <c>22</c> e <c>85</c> — resquício de outro domínio. Só 487
///    vínculos têm de fato uma letra, e 353 deles são "C".
///  - <c>IVS_Pes.Classe</c>: <b>100% em branco</b>, nas 139.072 linhas.
///  - <c>GE_Pessoa.Porte</c>: em branco, "." ou a palavra literal "string".
///
/// Três colunas para a mesma coisa, e nenhuma preenchida. Inventar a letra seria repetir o
/// defeito que este CRM existe para corrigir; deixar a métrica de fora seria não responder a
/// pergunta que a diretoria faz.
///
/// <para><b>A saída é apurar</b> — do faturamento real, que existe: R$ 2,49 bilhões em 7.252
/// clientes entre 2022 e 11/04/2025. Curva ABC clássica, o método que qualquer concessionária
/// usa, e que tem a vantagem de ser reproduzível: dois auditores chegam ao mesmo resultado.</para>
///
/// <para><b>A cadência de cada classe é declarada pelo negócio</b>, e essa parte o legado tem:
/// Venda de Máquinas e Implemento visita A, B e C a cada 180 dias e D a cada 360; Prospecção usa
/// 120/120/120/180. Ver <c>LinhaDeNegocio.DeclararCadencia</c>.</para>
/// </summary>
public enum ClasseDeCliente
{
    /// <summary>Os clientes que, somados, respondem pelos primeiros 80% do faturamento.</summary>
    A = 0,

    /// <summary>Os que completam de 80% a 95%.</summary>
    B = 1,

    /// <summary>Os que completam de 95% a 100%.</summary>
    C = 2,

    /// <summary>
    /// Comprou nada na janela apurada.
    ///
    /// <para>Não é "cliente ruim": é cliente sem compra no período. Boa parte da carteira está
    /// aqui, e é exatamente essa a fatia que a cobertura existe para atacar.</para>
    /// </summary>
    D = 3
}

/// <summary>
/// A carteirização: em que carteira o cliente está, com que classe, ciclo e potencial.
///
/// Um cliente está em várias carteiras ao mesmo tempo — uma por linha de negócio — com
/// classe diferente em cada. Substitui dez tabelas do Vórtice.
/// </summary>
public sealed class ClienteCarteira
{
    private ClienteCarteira() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O cliente.</summary>
    public long ClienteId { get; private set; }

    /// <summary>A carteira.</summary>
    public long CarteiraId { get; private set; }

    /// <summary>A classe do cliente nesta carteira.</summary>
    public ClasseDeCliente Classe { get; private set; } = ClasseDeCliente.C;

    /// <summary>Potencial anual estimado nesta linha de negócio.</summary>
    public Dinheiro? PotencialAnual { get; private set; }

    /// <summary>Cadência esperada de contato, em dias.</summary>
    public short? DiasCicloContato { get; private set; }

    /// <summary>
    /// Quando foi a última interação com este cliente nesta carteira (UTC).
    ///
    /// É a ÚNICA exceção consciente à regra de não copiar dado derivado, e está registrada
    /// como tal: a tela de Cobertura ordena centenas de clientes por esta data, e calcular na
    /// hora custaria varrer a tabela de interações, que herda 2,4 milhões de linhas. É
    /// mantido por regra na gravação da interação, com reconciliação diária por job.
    /// </summary>
    public DateTime? UltimaInteracaoEm { get; private set; }

    /// <summary>Quando o cliente entrou na carteira (UTC).</summary>
    public DateTime VinculadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quem colocou o cliente na carteira.</summary>
    public long VinculadoPorId { get; private set; }

    /// <summary>Quando o cliente saiu da carteira. Nulo é vínculo vigente.</summary>
    public DateTime? DesvinculadoEm { get; private set; }

    /// <summary>Coloca um cliente numa carteira.</summary>
    /// <param name="clienteId">O cliente.</param>
    /// <param name="carteiraId">A carteira.</param>
    /// <param name="classe">A classe do cliente nesta carteira.</param>
    /// <param name="vinculadoPorId">Quem vinculou.</param>
    /// <param name="diasCicloContato">Cadência esperada de contato, em dias.</param>
    /// <param name="vinculadoEmUtc">Quando o vínculo nasceu na origem.</param>
    public static ClienteCarteira Criar(
        long clienteId,
        long carteiraId,
        ClasseDeCliente classe,
        long vinculadoPorId,
        short? diasCicloContato = null,
        DateTime? vinculadoEmUtc = null)
    {
        if (diasCicloContato is <= 0)
            throw new RegraDeNegocioViolada("Ciclo de contato em dias não é zero nem negativo.");

        return new ClienteCarteira
        {
            ClienteId = clienteId,
            CarteiraId = carteiraId,
            Classe = classe,
            DiasCicloContato = diasCicloContato,
            VinculadoEm = vinculadoEmUtc ?? DateTime.UtcNow,
            VinculadoPorId = vinculadoPorId
        };
    }

    /// <summary>
    /// Tira o cliente da carteira SEM apagar a linha — a mesma regra do território da carteira.
    ///
    /// <para>"Este cliente já foi desta carteira, de tal a tal data" é o que responde por que ele tem o
    /// histórico que tem. Se ele voltar, entra uma linha nova: o intervalo em que esteve fora continua
    /// visível, em vez de ser apagado por uma reabertura.</para>
    /// </summary>
    /// <param name="quandoUtc">O instante do encerramento.</param>
    public void Desvincular(DateTime quandoUtc) => DesvinculadoEm ??= quandoUtc;

    /// <summary>Carimba a data da última interação. Chamado pela regra de gravação de interação.</summary>
    public void RegistrarInteracao(DateTime ocorridaEmUtc)
    {
        if (UltimaInteracaoEm is null || ocorridaEmUtc > UltimaInteracaoEm)
            UltimaInteracaoEm = ocorridaEmUtc;
    }
}
