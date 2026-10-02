using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O CENÁRIO ESCOLHIDO para a meta de um município (issue 263): conservador, moderado, otimista — o mercado ajustado do
/// momento com −5%, 0% ou +5% — ou um número digitado à mão.
/// </summary>
public enum CenarioDeMercado
{
    /// <summary>O mercado ajustado com −5%.</summary>
    Conservador = 1,

    /// <summary>O mercado ajustado como está — o padrão de quem ainda não escolheu.</summary>
    Moderado = 2,

    /// <summary>O mercado ajustado com +5%.</summary>
    Otimista = 3,

    /// <summary>Um número digitado pelo gestor.</summary>
    Manual = 4
}

/// <summary>
/// A META DE UM MUNICÍPIO NUM ANO FISCAL, para uma categoria de máquina — o planejamento do gestor com o CEN (issue 263, a
/// aba "Cenários de Mercado" do protótipo da pasta 360).
///
/// <para><b>Uma por município, categoria e ano fiscal</b> (decisão do Ricardo em 02/10/2026: a meta é por ano fiscal, de
/// novembro a outubro). Mudar de ideia altera a mesma linha — e a trilha guarda de quanto para quanto, quem e quando.</para>
///
/// <para><b>O número escolhido fica gravado</b> (<see cref="MetaNaEscolha"/>), e não só o cenário: o mercado ajustado muda
/// com o preço e o crédito, e "moderado" em março não é o mesmo número de "moderado" em agosto. A tela mostra o cenário de
/// hoje ao lado do que foi combinado.</para>
///
/// <para><b>Não é <c>EntidadeBase</c>, de propósito</b>, pelo mesmo motivo de <see cref="MunicipioDaAreaDeAtuacao"/>: toda
/// entidade-base é dona de uma filial (<c>EmpresaId</c>, a fronteira de multiempresa), e a meta não é. Ela é do município, e a
/// filial é atributo dele — a diretoria precisa ver a meta da ADR inteira. Quem grava em qual município é conferido no caso
/// de uso, pela filial responsável na área de atuação.</para>
///
/// <para><b>Quem grava é a Gerência e a Diretoria</b> (permissão <c>Planejamento.Gravar</c>, decisão de 02/10/2026); o CEN
/// vê. Aqui só há a regra do que a meta pode ser.</para>
/// </summary>
public sealed class MetaDoCenarioNoMunicipio
{
    private MetaDoCenarioNoMunicipio() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O município.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>A categoria de máquina — trator, colhedora de cana…</summary>
    public int CategoriaDeMaquinaId { get; private set; }

    /// <summary>O ano fiscal — o ano civil em que ele termina (2026 é nov/2025 a out/2026).</summary>
    public short AnoFiscal { get; private set; }

    /// <summary>O cenário escolhido.</summary>
    public CenarioDeMercado Cenario { get; private set; }

    /// <summary>O número digitado, no cenário manual; nulo nos outros.</summary>
    public decimal? ValorManual { get; private set; }

    /// <summary>A meta em máquinas no momento da escolha — o número combinado, que não muda quando o mercado muda.</summary>
    public decimal MetaNaEscolha { get; private set; }

    /// <summary>Quem escolheu primeiro.</summary>
    public long EscolhidaPorId { get; private set; }

    /// <summary>Quando (UTC).</summary>
    public DateTime EscolhidaEm { get; private set; }

    /// <summary>Quem mudou por último; nulo enquanto ninguém mudou.</summary>
    public long? AlteradaPorId { get; private set; }

    /// <summary>Quando (UTC).</summary>
    public DateTime? AlteradaEm { get; private set; }

    /// <summary>Quem gravou por último — o autor que a tela mostra.</summary>
    public long GravadaPorId => AlteradaPorId ?? EscolhidaPorId;

    /// <summary>Quando foi gravada por último.</summary>
    public DateTime GravadaEm => AlteradaEm ?? EscolhidaEm;

    /// <summary>Escolhe a meta de um município pela primeira vez.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="categoriaDeMaquinaId">A categoria.</param>
    /// <param name="anoFiscal">O ano fiscal.</param>
    /// <param name="cenario">O cenário.</param>
    /// <param name="valorManual">O número digitado, só no manual.</param>
    /// <param name="metaNaEscolha">A meta do cenário hoje — no manual, o próprio número.</param>
    /// <param name="usuarioId">Quem escolheu.</param>
    /// <param name="agoraUtc">Quando.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando a combinação não vale.</exception>
    public static MetaDoCenarioNoMunicipio Escolher(
        int municipioId, int categoriaDeMaquinaId, short anoFiscal, CenarioDeMercado cenario, decimal? valorManual, decimal metaNaEscolha,
        long usuarioId, DateTime agoraUtc)
    {
        if (municipioId <= 0) throw new RegraDeNegocioViolada("A meta é de um município.");
        if (categoriaDeMaquinaId <= 0) throw new RegraDeNegocioViolada("A meta é de uma categoria de máquina.");
        if (anoFiscal is < 2000 or > 2100) throw new RegraDeNegocioViolada($"O ano fiscal {anoFiscal} não existe no planejamento.");

        var meta = new MetaDoCenarioNoMunicipio
        {
            MunicipioId = municipioId,
            CategoriaDeMaquinaId = categoriaDeMaquinaId,
            AnoFiscal = anoFiscal,
            EscolhidaPorId = usuarioId,
            EscolhidaEm = agoraUtc
        };
        meta.Aplicar(cenario, valorManual, metaNaEscolha);
        return meta;
    }

    /// <summary>Muda a escolha — a trilha guarda o antes e o depois.</summary>
    /// <param name="cenario">O cenário novo.</param>
    /// <param name="valorManual">O número digitado, só no manual.</param>
    /// <param name="metaNaEscolha">A meta do cenário hoje.</param>
    /// <param name="usuarioId">Quem mudou.</param>
    /// <param name="agoraUtc">Quando.</param>
    /// <returns>Se algo mudou.</returns>
    public bool Alterar(CenarioDeMercado cenario, decimal? valorManual, decimal metaNaEscolha, long usuarioId, DateTime agoraUtc)
    {
        var antes = (Cenario, ValorManual, MetaNaEscolha);
        Aplicar(cenario, valorManual, metaNaEscolha);
        if (antes == (Cenario, ValorManual, MetaNaEscolha)) return false;
        AlteradaPorId = usuarioId;
        AlteradaEm = agoraUtc;
        return true;
    }

    private void Aplicar(CenarioDeMercado cenario, decimal? valorManual, decimal metaNaEscolha)
    {
        if (!Enum.IsDefined(cenario)) throw new RegraDeNegocioViolada("O cenário é conservador, moderado, otimista ou manual.");

        if (cenario == CenarioDeMercado.Manual)
        {
            if (valorManual is null) throw new RegraDeNegocioViolada("O cenário manual precisa do número da meta.");
            if (valorManual < 0) throw new RegraDeNegocioViolada("A meta não é negativa.");
            if (valorManual > 100_000) throw new RegraDeNegocioViolada("Meta acima de 100 mil máquinas num município não é um número possível.");
        }
        else if (valorManual is not null)
        {
            throw new RegraDeNegocioViolada("Só o cenário manual leva um número digitado.");
        }

        if (metaNaEscolha < 0) throw new RegraDeNegocioViolada("A meta não é negativa.");

        Cenario = cenario;
        ValorManual = cenario == CenarioDeMercado.Manual ? decimal.Round(valorManual!.Value, 2) : null;
        MetaNaEscolha = decimal.Round(cenario == CenarioDeMercado.Manual ? valorManual!.Value : metaNaEscolha, 2);
    }
}
