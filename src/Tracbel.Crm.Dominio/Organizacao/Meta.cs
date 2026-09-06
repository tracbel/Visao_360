using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>O que a meta mede.</summary>
public enum TipoDeMeta
{
    /// <summary>Faturamento no período, em reais.</summary>
    Faturamento = 0,

    /// <summary>Percentual da carteira visitada no período.</summary>
    Cobertura = 1,

    /// <summary>Quantidade de interações no período.</summary>
    Frequencia = 2,

    /// <summary>Quantidade de máquinas vendidas no período.</summary>
    Volume = 3
}

/// <summary>
/// O alvo de faturamento, de cobertura ou de frequência, por CEN, linha e período.
///
/// As telas de Performance, Configurações e Visão 360 dependem dela. Hoje os alvos são
/// constante no código do protótipo (<c>config-metas.json</c>, <c>meta-frequencia.json</c>);
/// no Vórtice, <c>IVS_UsrMeta</c> tem uma única linha.
/// </summary>
public sealed class Meta : EntidadeBase
{
    private Meta() { }

    /// <summary>Filial a que a meta pertence.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Linha de negócio da meta. Nulo vale para todas.</summary>
    public int? LinhaDeNegocioId { get; private set; }

    /// <summary>Carteira da meta. Nulo quando a meta é do usuário ou da empresa.</summary>
    public long? CarteiraId { get; private set; }

    /// <summary>Usuário responsável pela meta. Nulo quando a meta é coletiva.</summary>
    public long? UsuarioId { get; private set; }

    /// <summary>O que a meta mede.</summary>
    public TipoDeMeta Tipo { get; private set; }

    /// <summary>Primeiro dia do período de apuração.</summary>
    public DateOnly PeriodoInicio { get; private set; }

    /// <summary>Último dia do período de apuração.</summary>
    public DateOnly PeriodoFim { get; private set; }

    /// <summary>
    /// O alvo. Guardado como número com escala explícita porque serve tanto a reais
    /// (faturamento) quanto a percentual (cobertura) e a contagem (frequência).
    /// </summary>
    public decimal Alvo { get; private set; }

    /// <summary>Observação de quem cadastrou a meta.</summary>
    public string? Observacao { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Cria uma meta.</summary>
    public static Meta Criar(
        int empresaId, TipoDeMeta tipo, DateOnly periodoInicio, DateOnly periodoFim, decimal alvo, long criadoPorId)
    {
        if (periodoFim < periodoInicio)
            throw new RegraDeNegocioViolada("O fim do período da meta não pode ser anterior ao início.");

        return new Meta
        {
            EmpresaId = empresaId,
            Tipo = tipo,
            PeriodoInicio = periodoInicio,
            PeriodoFim = periodoFim,
            Alvo = alvo,
            CriadoPorId = criadoPorId
        };
    }
}
