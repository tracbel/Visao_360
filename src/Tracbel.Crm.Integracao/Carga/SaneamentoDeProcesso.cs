using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// AS TRADUÇÕES DE DOMÍNIO ABERTO PARA DOMÍNIO FECHADO — processo, tarefa, desfecho e classe.
///
/// <para>Estão numa classe própria, pública e sem dependência de banco, por um motivo só: são as
/// decisões mais discutíveis desta carga, e decisão discutível precisa poder ser exercitada por
/// teste linha a linha. Cada função devolve, junto do valor, se ele foi <b>lido</b> ou
/// <b>assumido</b> — quem chama conta as duas coisas separadamente, e o relatório mostra as
/// duas.</para>
///
/// <para>[V] O sistema de origem guarda status de processo em <c>varchar(20)</c> livre: 36
/// valores distintos, <c>FINALIZADO</c> ao lado de <c>FINALIZADA</c>, <c>CANCELADO</c> ao lado
/// de <c>CANCELADA</c>, e 480.428 linhas em branco. Não existe tradução sem perda; existe
/// tradução <b>declarada</b>.</para>
/// </summary>
public static class SaneamentoDeProcesso
{
    /// <summary>Os status da origem que significam negócio fechado com venda.</summary>
    private static readonly HashSet<string> Ganhos = new(StringComparer.OrdinalIgnoreCase)
    {
        "FATURADO", "VENDA REALIZADA", "FAT AUTORIZADO", "SERVIÇO REALIZADO", "SERVICO REALIZADO",
        "COMISSÃO RECEBIDA", "COMISSAO RECEBIDA", "RECEBIDO FINANC"
    };

    /// <summary>Os status da origem que significam negócio encerrado sem venda.</summary>
    private static readonly HashSet<string> Perdas = new(StringComparer.OrdinalIgnoreCase)
    {
        "VENDA PERDIDA", "DESISTIU DA COMPRA", "DESISTENCIA COMPRA", "DESISTÊNCIA COMPRA",
        "CREDITO NAO APROVADO", "CRÉDITO NÃO APROVADO", "PEDIDO NÃO APROVADO", "PEDIDO NAO APROVADO",
        "DEVOLVIDO", "FINALIZADA S INTERES"
    };

    /// <summary>Os status da origem que significam cancelamento.</summary>
    private static readonly HashSet<string> Cancelamentos = new(StringComparer.OrdinalIgnoreCase)
    {
        "CANCELADO", "CANCELADA"
    };

    /// <summary>Os status da origem que significam processo parado sem estar encerrado.</summary>
    private static readonly HashSet<string> Suspensoes = new(StringComparer.OrdinalIgnoreCase)
    {
        "PARADO", "PENDENTE", "DOC PENDENTE"
    };

    /// <summary>
    /// Traduz o status de texto livre do processo para o domínio fechado do CRM.
    ///
    /// <para><b>A decisão mais delicada desta carga está aqui, e é o que NÃO se faz.</b> Um
    /// processo que a origem marca como realizado mas cujo status não declara desfecho comercial
    /// — <c>FINALIZADO</c>, <c>CONCLUIDO</c>, ou simplesmente em branco — <b>continua aberto</b>.
    /// Empurrá-lo para Ganho inventaria uma venda; para Perdido, uma perda; para Cancelado,
    /// repetiria o defeito do legado, onde o cancelamento virou a lixeira que engoliu 5.991
    /// negociações de 2026 sem motivo nenhum. O nosso domínio fechado não tem "encerrado sem
    /// desfecho comercial", e essa ausência é um achado a registrar — não um buraco a tapar com
    /// o valor mais próximo.</para>
    /// </summary>
    /// <param name="statusNaOrigem">O texto do status como a origem o guarda.</param>
    /// <param name="correcoes">Onde registrar a tradução assumida.</param>
    /// <returns>A situação no domínio fechado e se ela foi lida do status ou assumida.</returns>
    public static (SituacaoDoProcesso Situacao, bool VeioDaOrigem) Situacao(
        string? statusNaOrigem, List<CorrecaoAplicada> correcoes)
    {
        var texto = (statusNaOrigem ?? string.Empty).Trim();

        if (Ganhos.Contains(texto)) return (SituacaoDoProcesso.Ganho, true);
        if (Perdas.Contains(texto)) return (SituacaoDoProcesso.Perdido, true);
        if (Cancelamentos.Contains(texto)) return (SituacaoDoProcesso.Cancelado, true);
        if (Suspensoes.Contains(texto)) return (SituacaoDoProcesso.Suspenso, true);

        correcoes.Add(new CorrecaoAplicada(
            "situacaoDoProcesso", texto.Length == 0 ? null : texto, nameof(SituacaoDoProcesso.Aberto),
            texto.Length == 0
                ? "O status do processo está em branco na origem. Entrou como Aberto: fechá-lo " +
                  "exigiria afirmar um desfecho comercial que o dado não tem."
                : $"O status '{texto}' não declara desfecho comercial no sistema de origem. Entrou " +
                  "como Aberto em vez de ser empurrado para Ganho, Perdido ou Cancelado."));

        return (SituacaoDoProcesso.Aberto, false);
    }

    /// <summary>
    /// Traduz o par realizada/status da agenda para a situação da tarefa.
    ///
    /// <para>Concluir exige as três coisas que a restrição de verificação do banco cobra: data,
    /// quem concluiu e desfecho. Faltando qualquer uma, a tarefa entra <b>pendente</b> — o que é
    /// verdade sobre o que se sabe, e mantém a linha visível na agenda em vez de escondê-la num
    /// estado fechado que ninguém consegue justificar.</para>
    /// </summary>
    /// <param name="realizadaNaOrigem">A letra que a origem usa para "realizada".</param>
    /// <param name="temDataDeConclusao">Se a origem traz a data da realização.</param>
    /// <param name="temDesfecho">Se a origem traz o desfecho registrado.</param>
    /// <param name="temQuemConcluiu">Se a origem traz quem concluiu.</param>
    /// <returns>A situação no domínio fechado.</returns>
    public static SituacaoDaTarefa SituacaoDaTarefaDe(
        string? realizadaNaOrigem, bool temDataDeConclusao, bool temDesfecho, bool temQuemConcluiu)
    {
        var realizada = (realizadaNaOrigem ?? string.Empty).Trim().ToUpperInvariant() is "S";

        return realizada && temDataDeConclusao && temDesfecho && temQuemConcluiu
            ? SituacaoDaTarefa.Concluida
            : SituacaoDaTarefa.Pendente;
    }

    /// <summary>
    /// Deduz o que um desfecho FAZ com o funil, a partir do mapeamento declarado na origem.
    ///
    /// <para>A ordem importa: cancelar vence perder, perder vence avançar, e só sobra manutenção
    /// para quem não declara nada. [V] É o caso da maioria — 84% dos desfechos mapeados do fluxo
    /// de vendas não dizem para onde o processo vai, e por isso o desfecho mais lançado do CRM
    /// inteiro é "Contato Em Andamento", que não move nada.</para>
    /// </summary>
    /// <param name="statusDeDestino">O status que o mapeamento manda gravar, quando há.</param>
    /// <param name="declaraFaseSeguinte">Se o mapeamento aponta uma fase de destino.</param>
    /// <returns>A classe do desfecho e se ela foi deduzida do mapeamento ou assumida.</returns>
    public static (ClasseDeResultado Classe, bool VeioDoMapeamento) ClasseDoResultado(
        string? statusDeDestino, bool declaraFaseSeguinte)
    {
        var texto = (statusDeDestino ?? string.Empty).Trim();

        if (Cancelamentos.Contains(texto)) return (ClasseDeResultado.Cancelamento, true);
        if (Perdas.Contains(texto)) return (ClasseDeResultado.Perda, true);
        if (Ganhos.Contains(texto)) return (ClasseDeResultado.Avanco, true);
        if (declaraFaseSeguinte) return (ClasseDeResultado.Avanco, true);

        return (ClasseDeResultado.Manutencao, false);
    }

    /// <summary>
    /// Separa três coisas que a linha do tempo do legado mostra misturadas.
    ///
    /// <para>O invariante da origem é limpo e vale copiar: histórico ativo sempre nasce de uma
    /// tarefa, histórico receptivo nunca nasce. O que o legado <b>não</b> separa é o terceiro
    /// caso — o registro que o servidor escreveu sozinho, um quarto do volume de 2026. Aqui ele
    /// tem nome próprio.</para>
    /// </summary>
    /// <param name="naturezaNaOrigem">A letra de natureza da origem.</param>
    /// <param name="usuarioDeInclusao">O nome gravado como autor da inclusão.</param>
    /// <param name="autoresDeSistema">Os nomes que identificam o processo automático.</param>
    /// <param name="correcoes">Onde registrar a tradução assumida.</param>
    /// <returns>A natureza no domínio fechado.</returns>
    public static NaturezaDaInteracao Natureza(
        string? naturezaNaOrigem,
        string? usuarioDeInclusao,
        IReadOnlySet<string> autoresDeSistema,
        List<CorrecaoAplicada> correcoes)
    {
        var letra = (naturezaNaOrigem ?? string.Empty).Trim().ToUpperInvariant();
        var autor = (usuarioDeInclusao ?? string.Empty).Trim();

        if (autoresDeSistema.Contains(autor)) return NaturezaDaInteracao.Sistema;

        switch (letra)
        {
            case "A": return NaturezaDaInteracao.Ativa;
            case "R": return NaturezaDaInteracao.Receptiva;
            default:
                correcoes.Add(new CorrecaoAplicada(
                    "natureza", naturezaNaOrigem, nameof(NaturezaDaInteracao.Ativa),
                    letra.Length == 0
                        ? "A natureza está em branco na origem. Entrou como Ativa, que é o caso de " +
                          "99% do histórico do período."
                        : $"A letra '{letra}' não tem significado documentado no sistema de origem. " +
                          "Entrou como Ativa em vez de ser adivinhada."));
                return NaturezaDaInteracao.Ativa;
        }
    }

    /// <summary>
    /// Traduz o potencial de texto livre do vínculo de carteira para a classe A/B/C/D.
    ///
    /// <para>[V] A coluna é <c>varchar(3)</c> sem catálogo e o domínio real mistura letra com
    /// número: 4.553 linhas trazem <c>64</c>, <c>43</c>, <c>22</c> ou <c>85</c> onde deveria
    /// haver uma classe — resquício de outro domínio gravado no mesmo campo. O que não é A, B, C
    /// ou D entra como C e a suposição fica escrita.</para>
    /// </summary>
    /// <param name="potencialNaOrigem">O texto de potencial como a origem o guarda.</param>
    /// <param name="correcoes">Onde registrar a tradução assumida.</param>
    /// <returns>A classe e se ela foi lida da origem ou assumida.</returns>
    public static (ClasseDeCliente Classe, bool VeioDaOrigem) Classe(
        string? potencialNaOrigem, List<CorrecaoAplicada> correcoes)
    {
        var texto = (potencialNaOrigem ?? string.Empty).Trim().ToUpperInvariant();

        switch (texto)
        {
            case "A": return (ClasseDeCliente.A, true);
            case "B": return (ClasseDeCliente.B, true);
            case "C": return (ClasseDeCliente.C, true);
            case "D": return (ClasseDeCliente.D, true);
            default:
                correcoes.Add(new CorrecaoAplicada(
                    "classeNaCarteira", texto.Length == 0 ? null : texto, nameof(ClasseDeCliente.C),
                    texto.Length == 0
                        ? "O potencial está em branco na origem. Entrou como C — a classe do meio — " +
                          "em vez de ser adivinhado."
                        : $"O potencial '{texto}' não é uma classe A, B, C ou D. A coluna da origem é " +
                          "texto livre e guarda resquício de outro domínio. Entrou como C."));
                return (ClasseDeCliente.C, false);
        }
    }

    /// <summary>
    /// Transforma um nome de origem em código estável de catálogo — maiúsculas, sem acento e
    /// sem pontuação.
    ///
    /// <para>O código é o contrato público do catálogo (documento 16, seção 3): ele não pode
    /// mudar quando alguém corrigir a grafia do nome na tela. Por isso ele nasce do <b>número</b>
    /// da origem sempre que há um, e do texto só quando não há.</para>
    /// </summary>
    /// <param name="texto">O texto a transformar.</param>
    /// <param name="tamanhoMaximo">O tamanho da coluna de código.</param>
    /// <returns>O código, ou vazio quando não sobra nada.</returns>
    public static string Codificar(string? texto, int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var semAcento = texto.Trim().Normalize(System.Text.NormalizationForm.FormD);
        var construtor = new System.Text.StringBuilder(semAcento.Length);

        foreach (var caractere in semAcento)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(caractere)
                is System.Globalization.UnicodeCategory.NonSpacingMark)
                continue;

            construtor.Append(char.IsAsciiLetterOrDigit(caractere) ? char.ToUpperInvariant(caractere) : '_');
        }

        var bruto = construtor.ToString();

        while (bruto.Contains("__", StringComparison.Ordinal))
            bruto = bruto.Replace("__", "_", StringComparison.Ordinal);

        bruto = bruto.Trim('_');

        return bruto.Length <= tamanhoMaximo ? bruto : bruto[..tamanhoMaximo].TrimEnd('_');
    }
}
