using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>Pessoa física ou jurídica.</summary>
public enum TipoDePessoa
{
    /// <summary>Pessoa física — documento de 11 dígitos.</summary>
    Fisica = 0,

    /// <summary>Pessoa jurídica — documento de 14 dígitos.</summary>
    Juridica = 1
}

/// <summary>
/// Em que ponto do relacionamento o cliente está.
///
/// [V] O Vórtice mistura cliente, prospect, suspect e falecido num único caractere, com
/// 2.126 linhas em branco que a própria view do fornecedor devolve como interrogação. Aqui
/// o domínio é fechado e explícito.
/// </summary>
public enum SituacaoDoCliente
{
    /// <summary>Ainda não se sabe se há interesse.</summary>
    Suspect = 0,

    /// <summary>Há interesse identificado, mas nunca comprou.</summary>
    Prospect = 1,

    /// <summary>Comprou e continua ativo.</summary>
    Cliente = 2,

    /// <summary>Comprou no passado e está sem movimento.</summary>
    ClienteInativo = 3,

    /// <summary>Relacionamento encerrado.</summary>
    Encerrado = 4
}

/// <summary>
/// O cadastro do cliente — único.
///
/// Substitui as 29 cópias do cadastro de pessoa do Vórtice. A coluna de situação cobre
/// suspect, prospect, cliente, inativo e encerrado: não existem cinco tabelas para cinco
/// estágios do mesmo relacionamento.
///
/// O termo é Cliente, e não Conta: ninguém na Tracbel fala conta.
/// </summary>
public sealed class Cliente : EntidadeBase
{
    private Cliente() { }

    /// <summary>Filial dona do cadastro. Raiz do escopo de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>
    /// Razão social, ou o nome da pessoa física.
    ///
    /// A coluna nasce com a colação <c>Latin1_General_CI_AI</c>: comparação e busca ignoram
    /// caixa E acento sem nenhuma coluna derivada — "Jose" encontra "José", "SOJA" é o mesmo
    /// que "Soja". [V] mata a tabela de fonema do Vórtice, com 151.583 linhas mantidas por
    /// fora só para fazer isto à mão, e mata também a coluna computada que a versão
    /// PostgreSQL deste modelo precisava manter (documento 20, seção 4).
    /// </summary>
    public string NomeRazao { get; private set; } = default!;

    /// <summary>Nome fantasia, quando existe.</summary>
    public string? NomeFantasia { get; private set; }

    /// <summary>Se é pessoa física ou jurídica.</summary>
    public TipoDePessoa TipoDePessoa { get; private set; }

    /// <summary>CPF ou CNPJ, só dígitos, já validado.</summary>
    public CpfCnpj? Documento { get; private set; }

    /// <summary>Inscrição estadual.</summary>
    public string? InscricaoEstadual { get; private set; }

    /// <summary>Código de atividade econômica.</summary>
    public string? AtividadeEconomica { get; private set; }

    /// <summary>Em que ponto do relacionamento o cliente está.</summary>
    public SituacaoDoCliente Situacao { get; private set; } = SituacaoDoCliente.Prospect;

    /// <summary>Desde quando está nesta situação (UTC).</summary>
    public DateTime SituacaoDesde { get; private set; } = DateTime.UtcNow;

    /// <summary>Item de catálogo com o motivo da inativação.</summary>
    public int? MotivoInativacaoId { get; private set; }

    /// <summary>Quem responde pelo cliente. Âncora da segurança por registro.</summary>
    public long ProprietarioId { get; private set; }

    /// <summary>A matriz do grupo econômico. Nulo é não pertencer a grupo.</summary>
    public long? ClienteMatrizId { get; private set; }

    /// <summary>Item de catálogo com a origem do cadastro.</summary>
    public int? OrigemId { get; private set; }

    /// <summary>Cria um cliente.</summary>
    public static Cliente Criar(
        int empresaId,
        string nomeRazao,
        TipoDePessoa tipoDePessoa,
        long proprietarioId,
        long criadoPorId,
        CpfCnpj? documento = null,
        string? nomeFantasia = null,
        SituacaoDoCliente situacao = SituacaoDoCliente.Prospect,
        string? inscricaoEstadual = null,
        string? atividadeEconomica = null,
        int? origemId = null)
    {
        if (string.IsNullOrWhiteSpace(nomeRazao))
            throw new RegraDeNegocioViolada("Cliente sem razão social não existe.");

        ConferirDocumento(documento, tipoDePessoa);

        return new Cliente
        {
            EmpresaId = empresaId,
            NomeRazao = nomeRazao.Trim(),
            NomeFantasia = string.IsNullOrWhiteSpace(nomeFantasia) ? null : nomeFantasia.Trim(),
            TipoDePessoa = tipoDePessoa,
            Documento = documento,
            InscricaoEstadual = string.IsNullOrWhiteSpace(inscricaoEstadual) ? null : inscricaoEstadual.Trim(),
            AtividadeEconomica = string.IsNullOrWhiteSpace(atividadeEconomica) ? null : atividadeEconomica.Trim(),
            Situacao = situacao,
            OrigemId = origemId,
            ProprietarioId = proprietarioId,
            CriadoPorId = criadoPorId
        };
    }

    /// <summary>
    /// Altera o cadastro.
    ///
    /// A ENTIDADE É QUEM RECUSA, e é por isso que este método existe em vez de o caso de uso
    /// atribuir propriedade por propriedade: não há caminho de código que troque o tipo de
    /// pessoa sem que a coerência com o documento seja conferida aqui. É a mesma disciplina de
    /// <see cref="Criar"/> — o objeto nunca fica num estado que o negócio não admite.
    ///
    /// A EMPRESA NÃO ENTRA: a filial dona do cadastro é a fronteira de acesso, não um campo de
    /// formulário. Mover um cliente entre filiais é outra operação, com outra permissão, e ela
    /// não existe nesta fase.
    /// </summary>
    public void Alterar(
        string nomeRazao,
        TipoDePessoa tipoDePessoa,
        long proprietarioId,
        long usuarioId,
        CpfCnpj? documento = null,
        string? nomeFantasia = null,
        string? inscricaoEstadual = null,
        string? atividadeEconomica = null,
        int? origemId = null)
    {
        if (EstaExcluido)
            throw new RegraDeNegocioViolada("Cliente inativado não aceita alteração. Reative antes.");

        if (string.IsNullOrWhiteSpace(nomeRazao))
            throw new RegraDeNegocioViolada("Cliente sem razão social não existe.");

        ConferirDocumento(documento, tipoDePessoa);

        NomeRazao = nomeRazao.Trim();
        NomeFantasia = string.IsNullOrWhiteSpace(nomeFantasia) ? null : nomeFantasia.Trim();
        TipoDePessoa = tipoDePessoa;
        Documento = documento;
        InscricaoEstadual = string.IsNullOrWhiteSpace(inscricaoEstadual) ? null : inscricaoEstadual.Trim();
        AtividadeEconomica = string.IsNullOrWhiteSpace(atividadeEconomica) ? null : atividadeEconomica.Trim();
        OrigemId = origemId;
        ProprietarioId = proprietarioId;

        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// A classe do cliente na curva ABC do faturamento. Nula enquanto não houve apuração.
    ///
    /// <para>Ver <see cref="ClasseDeCliente"/> para o motivo de ela ser apurada e não declarada:
    /// as três colunas do legado que deveriam guardá-la estão vazias.</para>
    /// </summary>
    public ClasseDeCliente? Classe { get; private set; }

    /// <summary>O faturamento que produziu a classe, na janela apurada.</summary>
    public decimal? FaturamentoApurado { get; private set; }

    /// <summary>
    /// Quando a classe foi apurada.
    ///
    /// <para>Vem junto porque a classe é uma FOTOGRAFIA: ela muda quando o faturamento muda, e
    /// um "cliente A" sem a data da apuração não diz se é A hoje ou se era A em 2023.</para>
    /// </summary>
    public DateTime? ClasseApuradaEm { get; private set; }

    /// <summary>
    /// Grava a classe apurada da curva ABC.
    /// </summary>
    /// <param name="classe">A classe apurada.</param>
    /// <param name="faturamentoApurado">O faturamento da janela que a produziu.</param>
    /// <param name="apuradaEm">Quando a apuração rodou.</param>
    /// <param name="usuarioId">Quem apurou.</param>
    public void ApurarClasse(
        ClasseDeCliente classe, decimal faturamentoApurado, DateTime apuradaEm, long usuarioId)
    {
        if (Classe == classe && FaturamentoApurado == faturamentoApurado) return;

        Classe = classe;
        FaturamentoApurado = faturamentoApurado;
        ClasseApuradaEm = apuradaEm;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>Move o cliente para outra situação, carimbando desde quando.</summary>
    public void MudarSituacao(SituacaoDoCliente novaSituacao, long usuarioId)
    {
        if (Situacao == novaSituacao) return;
        Situacao = novaSituacao;
        SituacaoDesde = DateTime.UtcNow;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Inativa o cliente: exclusão LÓGICA, com motivo de catálogo obrigatório.
    ///
    /// Nada é apagado — a linha continua no banco, auditável, e o histórico que aponta para ela
    /// continua legível. [V] é o oposto do legado, onde "Atividade Cancelada" cancelava o
    /// processo inteiro sem registrar motivo e sem caminho de volta.
    /// </summary>
    /// <param name="motivoId">Item do catálogo <c>MOTIVO_INATIVACAO</c>.</param>
    /// <param name="usuarioId">Quem inativou.</param>
    public void Inativar(int motivoId, long usuarioId)
    {
        if (EstaExcluido)
            throw new RegraDeNegocioViolada("Este cliente já está inativado.");

        MotivoInativacaoId = motivoId;
        MudarSituacao(SituacaoDoCliente.ClienteInativo, usuarioId);
        Excluir(usuarioId);
    }

    private static void ConferirDocumento(CpfCnpj? documento, TipoDePessoa tipoDePessoa)
    {
        if (documento is not { } doc) return;

        var esperado = tipoDePessoa == TipoDePessoa.Fisica;
        if (doc.EhPessoaFisica != esperado)
            throw new RegraDeNegocioViolada(
                "O documento não corresponde ao tipo de pessoa informado: CPF para física, CNPJ para jurídica.");
    }
}
