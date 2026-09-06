using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.Saneamento;

/// <summary>
/// Pipeline de saneamento de dado vindo de fora — Vórtice, ERP (Protheus/TOTVS), planilha,
/// formulário externo, RD Station. Ver documento 16, princípio 1: dado sujo não entra.
///
/// Esta pasta é parte da quarentena descrita no documento 03 ("Por que Integracao é uma
/// camada separada"): aqui, e só aqui, dado bruto — texto solto, tipo frouxo, formato de
/// origem externa — é traduzido para os tipos de valor de <c>Tracbel.Crm.Dominio.Comum</c>
/// (<see cref="CpfCnpj"/>, <see cref="Email"/>, <see cref="Telefone"/> e os demais) antes
/// de qualquer entidade de domínio ser construída. Uma vez saneado, o dado passa a ser só
/// o tipo de valor: nada rio abaixo volta a tocar o formato de origem.
///
/// Dono da fronteira com o ERP: o que vem do Protheus (TOTVS) é saneado AQUI, na entrada,
/// uma única vez — o CRM lê o resultado já saneado e não regrava por cima do que pertence
/// ao ERP. O desenho da integração Protheus em si (watermark, filas, retentativa) é de
/// outra frente (documento 04, seção 8; documento 14).
/// </summary>
public interface ISanitizador<in TBruto, TLimpo>
{
    /// <summary>
    /// Aplica as regras de saneamento e devolve o resultado auditável — nunca lança para
    /// dado de negócio inválido. Uma linha ruim é registrada, não aborta o lote (documento
    /// 16, princípio 3; a mesma razão de <see cref="Telefone.TentarCriar"/> não lançar).
    /// </summary>
    ResultadoSaneamento<TLimpo> Sanear(TBruto bruto);
}

/// <summary>
/// Uma correção automática aplicada a um campo durante o saneamento.
///
/// Existe para que TODA correção automática fique registrada (documento 16, princípio 4)
/// — nunca silenciosa como o truncamento de <c>IV_Historico.ResultadoCmpl</c> no Vórtice.
/// </summary>
public sealed record CorrecaoAplicada(string Campo, string? ValorOriginal, string ValorNormalizado, string Motivo);

/// <summary>Um campo que não passou no saneamento e por isso não entra no resultado.</summary>
public sealed record CampoRejeitado(string Campo, string? ValorOriginal, string Motivo);

/// <summary>
/// Resultado de uma execução de saneamento.
///
/// Sucesso/valor reaproveita o <see cref="Resultado{T}"/> que já existe no domínio — não
/// duplicamos esse conceito. O que este tipo acrescenta é a trilha exigida pelo documento
/// 16: o que foi normalizado e o que foi rejeitado, campo a campo, com o motivo de cada um.
/// </summary>
public sealed class ResultadoSaneamento<T>
{
    private ResultadoSaneamento(
        Resultado<T> resultado,
        IReadOnlyList<CorrecaoAplicada> correcoes,
        IReadOnlyList<CampoRejeitado> rejeicoes)
    {
        Resultado = resultado;
        Correcoes = correcoes;
        Rejeicoes = rejeicoes;
    }

    /// <summary>O resultado de sucesso/falha e o valor produzido.</summary>
    public Resultado<T> Resultado { get; }

    /// <summary>Verdadeiro quando há um valor utilizável ao final do saneamento.</summary>
    public bool Aceito => Resultado.EhSucesso;

    /// <summary>O valor saneado. Só acesse depois de conferir <see cref="Aceito"/>.</summary>
    public T Valor => Resultado.Valor;

    /// <summary>Correções automáticas aplicadas campo a campo (ex.: e-mail em minúsculo, espaço colapsado).</summary>
    public IReadOnlyList<CorrecaoAplicada> Correcoes { get; }

    /// <summary>Campos que não passaram no saneamento, com o motivo — mesmo quando o resultado geral é aceito.</summary>
    public IReadOnlyList<CampoRejeitado> Rejeicoes { get; }

    /// <summary>Sucesso: há um valor, com zero ou mais correções e rejeições de campo isolado registradas.</summary>
    public static ResultadoSaneamento<T> Aceitar(
        T valor,
        IReadOnlyList<CorrecaoAplicada>? correcoes = null,
        IReadOnlyList<CampoRejeitado>? rejeicoesDeCampo = null) =>
        new(Resultado<T>.Ok(valor), correcoes ?? [], rejeicoesDeCampo ?? []);

    /// <summary>Falha: o conjunto de rejeições inviabiliza o registro inteiro (ex.: faltou o campo obrigatório).</summary>
    public static ResultadoSaneamento<T> Rejeitar(string motivoGeral, IReadOnlyList<CampoRejeitado> rejeicoes) =>
        new(Resultado<T>.Falha(motivoGeral), [], rejeicoes);

    /// <summary>
    /// Produz o registro auditável desta execução — o que persiste como trilha de uma
    /// linha de importação (documento 16, princípio 4: toda correção automática fica
    /// registrada).
    /// </summary>
    public RegistroSaneamento ParaRegistro(string fonte, string entidade, IRelogio relogio) =>
        new(fonte, entidade, relogio.Agora, Aceito, Correcoes, Rejeicoes);
}

/// <summary>
/// Registro auditável de uma execução de saneamento: o que entrou, o que foi corrigido, o
/// que foi rejeitado e quando. É o oposto do Vórtice, que registra falha crítica de
/// integração como <c>Warning</c> (documento 01, achado 3.4) — aqui toda decisão do
/// saneamento é um fato gravável.
/// </summary>
public sealed record RegistroSaneamento(
    string Fonte,
    string Entidade,
    DateTime ProcessadoEm,
    bool Aceito,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);
