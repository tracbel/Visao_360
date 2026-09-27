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

    /// <summary>
    /// O formulário do Vórtice de onde a resposta veio (<see cref="FormulariosDaVendaPerdida"/>); nulo quando a venda
    /// perdida não veio do Vórtice.
    /// </summary>
    public string? FormularioDeOrigem { get; private set; }

    /// <summary>
    /// O PAPEL DA RESPOSTA (decisão de 27/09/2026). O mesmo negócio perdido aparece em mais de um formulário — o
    /// <c>_JDE</c> repete o antigo, o <c>SEM_PARTICIPACAO</c> repete o FY25, e os <c>VP_*</c> detalham o FY25. Só a
    /// principal conta; as outras ficam, apontando para ela, para ninguém somar a mesma perda duas vezes.
    /// </summary>
    public PapelDaVendaPerdida Papel { get; private set; } = PapelDaVendaPerdida.Principal;

    /// <summary>A principal de quem esta é complemento ou duplicata; nula na principal.</summary>
    public long? VendaPerdidaPrincipalId { get; private set; }

    /// <summary>
    /// O número do processo no Vórtice — liga a perda ao funil (<see cref="EstagioDoProcesso"/>) sem depender de
    /// <see cref="Processo"/>, que só a onda 2 vai carregar.
    /// </summary>
    public long? NumeroDoProcessoNaOrigem { get; private set; }

    /// <summary>Se esta perda entra na conta: só a principal, e só a que não foi excluída.</summary>
    public bool Conta => Papel == PapelDaVendaPerdida.Principal && !EstaExcluido;

    /// <summary>O conteúdo que a origem declara, no formato em que ela é comparada.</summary>
    public ConteudoDaVendaPerdida Conteudo => new(
        EmpresaId, RegistradaEm, OcorridaEm, MotivoDePerdaId, ClienteId, TipoDeEquipamentoId, ConcorrenteId,
        RevendaDoConcorrenteId, ModeloDoConcorrente, ModeloOfertado, Quantidade, PrecoDoConcorrente, PrecoOfertado,
        Participacao, FormularioDeOrigem, NumeroDoProcessoNaOrigem);

    /// <summary>Registra uma resposta de formulário do Vórtice, já com o papel dela.</summary>
    /// <param name="conteudo">O que a origem declara.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    /// <param name="papel">O papel; a principal é o padrão.</param>
    /// <param name="principalId">A principal, quando o papel não é principal.</param>
    public static VendaPerdida DaOrigem(
        ConteudoDaVendaPerdida conteudo, long criadoPorId, PapelDaVendaPerdida papel = PapelDaVendaPerdida.Principal, long? principalId = null)
    {
        ValidarPapel(papel, principalId, null);
        var venda = new VendaPerdida { CriadoPorId = criadoPorId, Papel = papel, VendaPerdidaPrincipalId = principalId };
        venda.Aplicar(conteudo);
        return venda;
    }

    /// <summary>Acompanha a origem. Devolve se o conteúdo mudou.</summary>
    /// <param name="conteudo">O que a origem declara nesta rodada.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public bool AtualizarDaOrigem(ConteudoDaVendaPerdida conteudo, long usuarioId)
    {
        var normalizado = Normalizar(conteudo);
        if (Conteudo == normalizado) return false;

        Aplicar(normalizado);
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// Define o papel da resposta. A principal não aponta para ninguém; o complemento e a duplicata apontam para a
    /// principal, e nunca para si mesmos. Devolve se mudou.
    /// </summary>
    /// <param name="papel">O papel.</param>
    /// <param name="principalId">A principal, quando o papel não é principal.</param>
    /// <param name="usuarioId">Quem decide.</param>
    public bool DefinirPapel(PapelDaVendaPerdida papel, long? principalId, long usuarioId)
    {
        ValidarPapel(papel, principalId, Id);

        if (Papel == papel && VendaPerdidaPrincipalId == principalId) return false;

        Papel = papel;
        VendaPerdidaPrincipalId = principalId;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// A resposta voltou a aparecer na origem depois de sumir: deixa de estar excluída. Devolve se mudou.
    /// </summary>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public bool RestaurarDaOrigem(long usuarioId)
    {
        if (!EstaExcluido) return false;
        ExcluidoEm = null;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// O conteúdo com a data no milissegundo — o que a coluna <c>datetime2(3)</c> guarda. Sem isso a data relida do
    /// Vórtice (<c>datetime</c>, passo de 1/300 s) nunca seria igual à gravada.
    /// </summary>
    /// <param name="c">O conteúdo como veio.</param>
    public static ConteudoDaVendaPerdida Normalizar(ConteudoDaVendaPerdida c) =>
        c with { RegistradaEm = EstagioDoProcesso.NoMilissegundo(c.RegistradaEm), Quantidade = c.Quantidade < 1 ? 1 : c.Quantidade };

    private static void ValidarPapel(PapelDaVendaPerdida papel, long? principalId, long? proprioId)
    {
        if (papel == PapelDaVendaPerdida.Principal && principalId is not null)
            throw new RegraDeNegocioViolada("A venda perdida principal não aponta para outra principal.");
        if (papel != PapelDaVendaPerdida.Principal && principalId is null)
            throw new RegraDeNegocioViolada("O complemento e a duplicata precisam apontar a venda perdida principal.");
        if (principalId is not null && principalId == proprioId)
            throw new RegraDeNegocioViolada("A venda perdida não é complemento nem duplicata de si mesma.");
    }

    private void Aplicar(ConteudoDaVendaPerdida conteudo)
    {
        var c = Normalizar(conteudo);
        if (c.FormularioDeOrigem is { } formulario && !FormulariosDaVendaPerdida.Todos.Contains(formulario, StringComparer.Ordinal))
            throw new RegraDeNegocioViolada($"O formulário {formulario} não é de venda perdida.");

        EmpresaId = c.EmpresaId;
        RegistradaEm = c.RegistradaEm;
        OcorridaEm = c.OcorridaEm;
        MotivoDePerdaId = c.MotivoDePerdaId;
        ClienteId = c.ClienteId;
        TipoDeEquipamentoId = c.TipoDeEquipamentoId;
        ConcorrenteId = c.ConcorrenteId;
        RevendaDoConcorrenteId = c.RevendaDoConcorrenteId;
        ModeloDoConcorrente = c.ModeloDoConcorrente;
        ModeloOfertado = c.ModeloOfertado;
        Quantidade = c.Quantidade;
        PrecoDoConcorrente = c.PrecoDoConcorrente;
        PrecoOfertado = c.PrecoOfertado;
        Participacao = c.Participacao;
        FormularioDeOrigem = c.FormularioDeOrigem;
        NumeroDoProcessoNaOrigem = c.NumeroDoProcessoNaOrigem;
    }

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

/// <summary>O que uma resposta de formulário do Vórtice declara sobre a venda perdida — o que a rotina compara.</summary>
/// <param name="EmpresaId">A filial.</param>
/// <param name="RegistradaEm">Quando o formulário foi preenchido (UTC).</param>
/// <param name="OcorridaEm">Quando a venda foi perdida, como o CEN declarou.</param>
/// <param name="MotivoDePerdaId">O motivo.</param>
/// <param name="ClienteId">O cliente do CRM; nulo para o prospect.</param>
/// <param name="TipoDeEquipamentoId">O tipo de equipamento.</param>
/// <param name="ConcorrenteId">Quem levou.</param>
/// <param name="RevendaDoConcorrenteId">A revenda que fechou.</param>
/// <param name="ModeloDoConcorrente">O modelo do concorrente.</param>
/// <param name="ModeloOfertado">O modelo que a Tracbel ofereceu.</param>
/// <param name="Quantidade">Quantas máquinas.</param>
/// <param name="PrecoDoConcorrente">O preço do concorrente.</param>
/// <param name="PrecoOfertado">O preço da Tracbel.</param>
/// <param name="Participacao">Se a Tracbel participou.</param>
/// <param name="FormularioDeOrigem">O formulário de onde veio.</param>
/// <param name="NumeroDoProcessoNaOrigem">O processo no Vórtice.</param>
public sealed record ConteudoDaVendaPerdida(
    int EmpresaId,
    DateTime RegistradaEm,
    DateOnly? OcorridaEm,
    int MotivoDePerdaId,
    long? ClienteId,
    int? TipoDeEquipamentoId,
    int? ConcorrenteId,
    int? RevendaDoConcorrenteId,
    string? ModeloDoConcorrente,
    string? ModeloOfertado,
    int Quantidade,
    decimal? PrecoDoConcorrente,
    decimal? PrecoOfertado,
    ParticipacaoNaNegociacao Participacao,
    string? FormularioDeOrigem,
    long? NumeroDoProcessoNaOrigem);

/// <summary>
/// O PAPEL DE UMA RESPOSTA DE VENDA PERDIDA (decisão de 27/09/2026). Só a principal conta.
/// </summary>
public enum PapelDaVendaPerdida
{
    /// <summary>A resposta que representa a perda — a única que entra nas contas.</summary>
    Principal = 0,

    /// <summary>Um formulário de detalhe (<c>VP_*</c>) que acompanha a principal: acrescenta, não repete.</summary>
    Complemento = 1,

    /// <summary>O mesmo negócio registrado outra vez em outro formulário: repete a principal.</summary>
    Duplicata = 2
}

/// <summary>
/// OS FORMULÁRIOS DE VENDA PERDIDA DO VÓRTICE QUE ENTRAM (decisão de 27/09/2026, documento 52 §4) — o nome da tabela
/// de respostas, como a origem o escreve. O <c>_MANITO</c> (Colorado) fica de fora.
/// </summary>
public static class FormulariosDaVendaPerdida
{
    /// <summary>O antigo, 2012–2023: 1.511 respostas, 1.001 com preço John Deere.</summary>
    public const string Antigo = "IV_Q_VENDA_PERDIDA";

    /// <summary>O atual, desde 07/2025.</summary>
    public const string Fy25 = "IV_Q_VENDA_PERDIDA_FY25";

    /// <summary>Máquinas e implementos, 2024–2025.</summary>
    public const string MaqImp = "IV_Q_VENDA_PERDIDA_MAQIMP";

    /// <summary>A perda em que a Tracbel não participou, desde 08/2025.</summary>
    public const string SemParticipacao = "IV_Q_VP_SEM_PARTICIPACAO";

    /// <summary>O gêmeo do antigo, 2012–2016.</summary>
    public const string Jde = "IV_Q_VENDA_PERDIDA_JDE";

    /// <summary>Produto, 2022–2024.</summary>
    public const string Prod = "IV_Q_VENDA_PERDIDA_PROD";

    /// <summary>Implementos.</summary>
    public const string Implem = "IV_Q_VENDA_PERDIDA_IMPLEM";

    /// <summary>Implementos, versão curta.</summary>
    public const string Impl = "IV_Q_VENDA_PERDIDA_IMPL";

    /// <summary>O detalhe do trator perdido — complemento do FY25.</summary>
    public const string VpTrator = "IV_Q_VP_TRATOR";

    /// <summary>O detalhe da colheitadeira perdida — complemento do FY25.</summary>
    public const string VpColheitadeira = "IV_Q_VP_COLHEITADEIRA";

    /// <summary>O detalhe da plantadeira perdida — complemento do FY25.</summary>
    public const string VpPlantadeira = "IV_Q_VP_PLANTADEIRA";

    /// <summary>O detalhe da colhedora perdida — complemento do FY25.</summary>
    public const string VpColhedora = "IV_Q_VP_COLHEDORA";

    /// <summary>Os doze, na ordem da restrição do banco.</summary>
    public static readonly IReadOnlyList<string> Todos =
        [Antigo, Fy25, MaqImp, SemParticipacao, Jde, Prod, Implem, Impl, VpTrator, VpColheitadeira, VpPlantadeira, VpColhedora];

    /// <summary>Os formulários de detalhe, que acompanham o FY25.</summary>
    public static readonly IReadOnlySet<string> Complementos =
        new HashSet<string>(StringComparer.Ordinal) { VpTrator, VpColheitadeira, VpPlantadeira, VpColhedora };
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
