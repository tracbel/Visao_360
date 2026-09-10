using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Processo;

/// <summary>
/// A venda que o concorrente levou — com quem levou, por quanto e por quê.
///
/// ---------------------------------------------------------------------------------------------
/// POR QUE ESTA TABELA EXISTE, e por que ela não é uma coluna em <see cref="Processo"/>:
///
/// O processo já tem <c>MotivoDePerdaId</c>, e ele responde "por que não fechou". Esta tabela
/// responde a pergunta seguinte, que é a que a diretoria faz: **para quem perdemos, com que
/// máquina, e por quanta diferença de preço**. São nove atributos que só existem quando houve
/// concorrente, e pendurá-los no processo deixaria nove colunas nulas em 45 mil linhas.
///
/// ---------------------------------------------------------------------------------------------
/// DE ONDE O DADO VEM. Durante meses as telas disseram "os processos perdidos existem, mas o
/// legado não declara o motivo de nenhum deles". Estava errado, e o erro era de busca: o motivo
/// não é coluna do processo no Vórtice — é **resposta de formulário**. O Vórtice tem um motor de
/// questionário (<c>IV_Questionario</c>, 88.087 respostas) com 147 formulários tipados, um por
/// assunto, e a venda perdida é um deles.
///
/// O formulário em uso hoje é <c>IV_Q_VENDA_PERDIDA_FY25</c>, com **165 respostas em 2026** e
/// ligação direta ao processo. Antes dele houve <c>IV_Q_VENDA_PERDIDA</c> (1.511, até 2023) e
/// variantes por linha de negócio. Todas descrevem a mesma coisa e cabem aqui.
///
/// ---------------------------------------------------------------------------------------------
/// O QUE FOI SANEADO NA ENTRADA, e por quê — as regras estão em <c>SaneamentoDeVendaPerdida</c>:
///
///  - **Preço irrisório vira nulo.** A origem guarda <c>0,01</c> e <c>90,00</c> como preço de
///    trator. Não é preço: é tecla presa. Zero não é "de graça", é "não informado", e a média de
///    diferença de preço com esses valores dentro seria mentira.
///  - **Data fora do calendário vira nula.** Há venda perdida declarada em 2103 e 2104.
///  - **Texto em branco vira nulo**, e nunca string vazia — as duas coisas parecem iguais na tela
///    e se comportam diferente em toda consulta.
/// </summary>
public sealed class VendaPerdida : EntidadeBase
{
    private VendaPerdida() { }

    /// <summary>Filial dona do registro. É a fronteira de acesso, como em todo o modelo.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>
    /// O processo que se perdeu.
    ///
    /// Nulo é legítimo: a resposta do formulário aponta para um processo que pode estar fora do
    /// recorte carregado. Perder a resposta por causa disso seria perder o motivo da perda.
    /// </summary>
    public long? ProcessoId { get; private set; }

    /// <summary>O cliente que comprou do concorrente. Nulo pela mesma razão do processo.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>Quando o CEN preencheu o formulário.</summary>
    public DateTime RegistradaEm { get; private set; }

    /// <summary>Quando a venda foi perdida, como o CEN declarou. Nula quando ele não declarou.</summary>
    public DateOnly? OcorridaEm { get; private set; }

    /// <summary>Por que não fechou.</summary>
    public int MotivoDePerdaId { get; private set; }

    /// <summary>
    /// Tratores, colheitadeiras, implementos — a categoria da máquina disputada.
    ///
    /// Item do catálogo de sistema <c>TIPO_EQUIPAMENTO</c>, e não de <c>frota.Familia</c>:
    /// família pertence a uma marca, e o que o formulário declara vale para qualquer fabricante.
    /// </summary>
    public int? TipoDeEquipamentoId { get; private set; }

    /// <summary>
    /// O fabricante que levou a venda.
    ///
    /// É o MESMO catálogo que <c>Processo.ConcorrenteId</c> usa — o de sistema
    /// <c>CONCORRENTE</c>. Criar um segundo lugar para "quem é o concorrente" seria repetir o
    /// defeito que este projeto existe para corrigir.
    /// </summary>
    public int? ConcorrenteId { get; private set; }

    /// <summary>A revenda que fechou o negócio. Item do catálogo <c>REVENDA_CONCORRENTE</c>.</summary>
    public int? RevendaDoConcorrenteId { get; private set; }

    /// <summary>O modelo que o concorrente vendeu, como a origem escreveu.</summary>
    public string? ModeloDoConcorrente { get; private set; }

    /// <summary>O modelo que a Tracbel ofereceu.</summary>
    public string? ModeloOfertado { get; private set; }

    /// <summary>Quantas máquinas o negócio tinha. Um, quando a origem não diz.</summary>
    public int Quantidade { get; private set; } = 1;

    /// <summary>O preço do concorrente. Nulo quando não foi declarado ou não é preço.</summary>
    public decimal? PrecoDoConcorrente { get; private set; }

    /// <summary>O preço que a Tracbel ofereceu. Nulo pela mesma razão.</summary>
    public decimal? PrecoOfertado { get; private set; }

    /// <summary>
    /// Se a Tracbel chegou a participar da negociação.
    ///
    /// <para><b>São três respostas, e não duas.</b> "Não se sabe" é frequente — o formulário atual
    /// não faz essa pergunta e o anterior a deixava em branco —, e é afirmação diferente de
    /// "ficamos de fora". Este campo já foi <c>bool?</c>, e o nulo carregava a terceira resposta
    /// sem nome: quem lesse a coluna precisava saber, de cabeça, que <c>NULL</c> ali significava
    /// "não informado" e não "erro de carga". Com o valor nomeado, a soma por participação fecha
    /// sozinha e ninguém precisa adivinhar.</para>
    /// </summary>
    public ParticipacaoNaNegociacao Participacao { get; private set; }

    /// <summary>Quem preencheu, como a origem identifica. Só para rastrear.</summary>
    public string? RegistradaPor { get; private set; }

    /// <summary>Cria o registro de uma venda perdida.</summary>
    public static VendaPerdida Criar(
        int empresaId,
        DateTime registradaEm,
        int motivoDePerdaId,
        long? processoId = null,
        long? clienteId = null,
        DateOnly? ocorridaEm = null,
        int? tipoDeEquipamentoId = null,
        int? concorrenteId = null,
        int? revendaDoConcorrenteId = null,
        string? modeloDoConcorrente = null,
        string? modeloOfertado = null,
        int quantidade = 1,
        decimal? precoDoConcorrente = null,
        decimal? precoOfertado = null,
        ParticipacaoNaNegociacao participacao = ParticipacaoNaNegociacao.NaoInformado,
        string? registradaPor = null) => new()
        {
            EmpresaId = empresaId,
            RegistradaEm = registradaEm,
            MotivoDePerdaId = motivoDePerdaId,
            ProcessoId = processoId,
            ClienteId = clienteId,
            OcorridaEm = ocorridaEm,
            TipoDeEquipamentoId = tipoDeEquipamentoId,
            ConcorrenteId = concorrenteId,
            RevendaDoConcorrenteId = revendaDoConcorrenteId,
            ModeloDoConcorrente = modeloDoConcorrente,
            ModeloOfertado = modeloOfertado,
            Quantidade = quantidade < 1 ? 1 : quantidade,
            PrecoDoConcorrente = precoDoConcorrente,
            PrecoOfertado = precoOfertado,
            Participacao = participacao,
            RegistradaPor = registradaPor
        };
}

/// <summary>
/// Se a Tracbel participou da negociação que foi perdida.
///
/// <para>É seleção, e não booleano anulável: a diferença entre "ficamos de fora" e "ninguém
/// registrou" muda a leitura da derrota. A primeira é uma perda de cobertura — o concorrente
/// chegou e nós nem soubemos. A segunda é uma falha de preenchimento. Tratar as duas como o mesmo
/// <c>NULL</c> apagaria justamente o que a diretoria precisa distinguir.</para>
/// </summary>
public enum ParticipacaoNaNegociacao
{
    /// <summary>Ninguém registrou. É a maioria, e não é o mesmo que "não participamos".</summary>
    NaoInformado = 0,

    /// <summary>Disputamos e perdemos.</summary>
    Sim = 1,

    /// <summary>Não fomos chamados — perda de cobertura, não de proposta.</summary>
    Nao = 2
}
