namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>Como o dono atual no Protheus se compara com o comprador da venda mais recente no ART.</summary>
public enum DesfechoDaComparacaoComOArt
{
    /// <summary>O Protheus não diz nada: não tem a máquina, tem o chassi repetido com donos diferentes, ou o dono não
    /// tem documento. Vale o comprador do ART, sem divergência — não há o que contrariar.</summary>
    ProtheusSemDono = 0,

    /// <summary>O dono atual no Protheus É o comprador do ART — as duas fontes confirmam o dono.</summary>
    MesmoDono = 1,

    /// <summary>Outro documento, mesma raiz de CNPJ: é a mesma empresa (outra filial dela). Vale o Protheus.</summary>
    ProtheusPrevalecePelaMesmaRaiz = 2,

    /// <summary>Há nota de venda ou ordem de serviço DO DONO ATUAL depois da venda no ART: a máquina mudou de mão
    /// depois dela. Vale o Protheus.</summary>
    ProtheusPrevalecePorEvidenciaPosterior = 3,

    /// <summary>Donos diferentes e nada no Protheus depois da venda no ART. Vale o comprador do ART, e a divergência
    /// fica registrada.</summary>
    ArtPrevaleceSemEvidencia = 4,

    /// <summary>O Protheus diz que a dona é a própria Tracbel — a máquina voltou, ou o cadastro nunca foi atualizado.
    /// Vale o comprador do ART, e a divergência fica registrada.</summary>
    ArtPrevalecePorqueOProtheusDizTracbel = 5
}

/// <summary>
/// AS REGRAS DO PARQUE PELO PROPRIETÁRIO ATUAL — funções puras, sem banco, testáveis linha a linha. São as mesmas
/// para a sincronia do parque e para o ciclo do ART: quem decide quem tem a máquina é um lugar só.
/// </summary>
public static class RegrasDoParque
{
    /// <summary>
    /// OS GRUPOS DE MODELO QUE SÃO COMPONENTE, e não máquina (decisão de 24/09/2026): <c>AP</c> (agricultura de
    /// precisão — AutoTrac, StarFire), <c>MOT</c> (motor), <c>CAPOTA</c> e <c>KIT</c>. Eles têm número de série e dono
    /// no Protheus porque são vendidos à parte, mas não são o parque do cliente. Ficam de fora — o modelo tem
    /// <c>EquipamentoPaiId</c>, mas o Protheus não diz em que máquina cada componente está montado, e pendurá-lo
    /// numa máquina seria adivinhar.
    /// </summary>
    public static readonly IReadOnlySet<string> GruposDeComponente = new HashSet<string>(StringComparer.Ordinal) { "AP", "MOT", "CAPOTA", "KIT" };

    /// <summary>Se a máquina do Protheus é, na verdade, um componente.</summary>
    /// <param name="maquina">A máquina.</param>
    public static bool EhComponente(MaquinaNoProtheus maquina) =>
        maquina.GrupoDoModelo is { } grupo && GruposDeComponente.Contains(grupo);

    /// <summary>Se o documento é de uma empresa do grupo — pela raiz do CNPJ, nunca pelo nome.</summary>
    /// <param name="documento">O documento, só dígitos.</param>
    /// <param name="raizesDoGrupo">As raízes do grupo configuradas.</param>
    public static bool EhDoGrupo(string? documento, IReadOnlySet<string> raizesDoGrupo) =>
        ParceirosPorRaizDeCnpj.Raiz(documento) is { } raiz && raizesDoGrupo.Contains(raiz);

    /// <summary>
    /// A DECISÃO 1 DE 24/09/2026 — quem prevalece quando o Protheus e o ART discordam do dono.
    ///
    /// <para>O PROTHEUS PREVALECE QUANDO HÁ EVIDÊNCIA: nota de venda ou ordem de serviço do dono atual depois da data
    /// da venda no ART, ou a mesma raiz de CNPJ (a mesma empresa). SEM EVIDÊNCIA, ou quando o Protheus diz que a dona
    /// é a própria Tracbel, prevalece o comprador do ART, e o caso fica registrado como divergência. O comprador do
    /// ART continua, sempre, como histórico da venda.</para>
    ///
    /// <para><b>A data da venda no ART é a do faturamento</b> (<c>data_fat</c>, decisão D-P08.1), e a da venda
    /// quando o faturamento falta.</para>
    /// </summary>
    /// <param name="noProtheus">A máquina no Protheus; nula quando o chassi não está lá ou é ambíguo.</param>
    /// <param name="documentoDoComprador">O documento do comprador da venda mais recente no ART.</param>
    /// <param name="dataDaVendaNoArt">O faturamento da venda no ART, ou a data da venda.</param>
    /// <param name="raizesDoGrupo">As raízes de CNPJ do grupo.</param>
    public static DesfechoDaComparacaoComOArt Comparar(
        MaquinaNoProtheus? noProtheus, string documentoDoComprador, DateOnly? dataDaVendaNoArt, IReadOnlySet<string> raizesDoGrupo)
    {
        if (noProtheus?.DocumentoDoDono is not { } dono) return DesfechoDaComparacaoComOArt.ProtheusSemDono;
        if (string.Equals(dono, documentoDoComprador, StringComparison.Ordinal)) return DesfechoDaComparacaoComOArt.MesmoDono;
        if (EhDoGrupo(dono, raizesDoGrupo)) return DesfechoDaComparacaoComOArt.ArtPrevalecePorqueOProtheusDizTracbel;

        if (ParceirosPorRaizDeCnpj.Raiz(dono) is { } raiz && raiz == ParceirosPorRaizDeCnpj.Raiz(documentoDoComprador))
            return DesfechoDaComparacaoComOArt.ProtheusPrevalecePelaMesmaRaiz;

        return dataDaVendaNoArt is { } data && noProtheus.TemEvidenciaDoDonoDepoisDe(data)
            ? DesfechoDaComparacaoComOArt.ProtheusPrevalecePorEvidenciaPosterior
            : DesfechoDaComparacaoComOArt.ArtPrevaleceSemEvidencia;
    }

    /// <summary>Se o desfecho fica registrado como divergência para revisão.</summary>
    /// <param name="desfecho">O desfecho.</param>
    public static bool RegistraDivergencia(DesfechoDaComparacaoComOArt desfecho) =>
        desfecho is DesfechoDaComparacaoComOArt.ArtPrevaleceSemEvidencia or DesfechoDaComparacaoComOArt.ArtPrevalecePorqueOProtheusDizTracbel;

    /// <summary>Se o dono atual é o do Protheus (e não o comprador do ART).</summary>
    /// <param name="desfecho">O desfecho.</param>
    public static bool ProtheusPrevalece(DesfechoDaComparacaoComOArt desfecho) =>
        desfecho is DesfechoDaComparacaoComOArt.MesmoDono
            or DesfechoDaComparacaoComOArt.ProtheusPrevalecePelaMesmaRaiz
            or DesfechoDaComparacaoComOArt.ProtheusPrevalecePorEvidenciaPosterior;
}
