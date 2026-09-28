using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Frota;

/// <summary>
/// Uma máquina do Operations Center da John Deere como a leitura a entrega: a identificação e a ÚLTIMA leitura de cada
/// coisa — o horímetro e a posição, cada um com a própria data.
/// </summary>
/// <param name="IdNaOrigem">O identificador da máquina no Operations Center.</param>
/// <param name="Vin">O chassi (VIN) como o Operations Center guarda; pode vir vazio.</param>
/// <param name="Horas">As horas de motor da última leitura.</param>
/// <param name="HorasEmUtc">Quando a última leitura de horas foi feita.</param>
/// <param name="Latitude">A latitude da última posição.</param>
/// <param name="Longitude">A longitude da última posição.</param>
/// <param name="PosicaoEmUtc">Quando a máquina estava na última posição.</param>
public sealed record MaquinaNaTelemetria(
    string IdNaOrigem,
    string? Vin,
    decimal? Horas,
    DateTime? HorasEmUtc,
    double? Latitude,
    double? Longitude,
    DateTime? PosicaoEmUtc)
{
    /// <summary>A leitura mais recente da máquina, de horas ou de posição; nula sem leitura nenhuma.</summary>
    public DateTime? LeituraMaisRecente =>
        HorasEmUtc is { } horas && PosicaoEmUtc is { } posicao ? (horas > posicao ? horas : posicao) : HorasEmUtc ?? PosicaoEmUtc;
}

/// <summary>Uma máquina do CRM, só com o que o casamento usa.</summary>
/// <param name="Id">O identificador do equipamento.</param>
/// <param name="Chassi">O chassi gravado.</param>
/// <param name="NumeroSerie">O número de série, quando há.</param>
public sealed record MaquinaDoCrmParaTelemetria(long Id, string Chassi, string? NumeroSerie);

/// <summary>O que aconteceu com cada máquina do Operations Center no casamento.</summary>
public enum DesfechoDaTelemetria
{
    /// <summary>Achou a máquina do CRM pelo chassi.</summary>
    CasadaPeloChassi = 0,

    /// <summary>Achou a máquina do CRM pelo número de série.</summary>
    CasadaPeloNumeroDeSerie = 1,

    /// <summary>O Operations Center não tem o chassi desta máquina.</summary>
    SemVin = 2,

    /// <summary>O chassi não está no parque do CRM.</summary>
    SemParNoCrm = 3,

    /// <summary>
    /// Outra máquina do Operations Center com o mesmo chassi, ou que caiu na mesma máquina do CRM, tem leitura mais nova
    /// — é o terminal trocado de máquina, e vale o que falou por último.
    /// </summary>
    Repetida = 4
}

/// <summary>
/// O CASAMENTO DA TELEMETRIA COM O PARQUE DO CRM (decisão de 28/09/2026) — pelo chassi, e só por ele.
///
/// <para><b>O chassi exato primeiro, o número de série depois.</b> O VIN do Operations Center é a plaqueta da máquina; no
/// CRM ele é o <see cref="Equipamento.Chassi"/>, normalizado do mesmo jeito (<see cref="Chassi.Normalizar"/>). O número de
/// série só entra quando o chassi não achou nada, e só quando ele aponta uma máquina só.</para>
///
/// <para><b>Nada por semelhança.</b> Pedaço de chassi, final igual e nome de modelo não casam: a leitura errada poria
/// o horímetro de uma máquina na outra, e isso ninguém vê na tela.</para>
///
/// <para><b>Uma leitura por máquina do CRM.</b> Quando duas máquinas do Operations Center caem na mesma — o mesmo VIN
/// em dois terminais, que é o terminal trocado —, vale a que tem a leitura mais nova.</para>
/// </summary>
public static class CasamentoDaTelemetria
{
    /// <summary>Casa as máquinas do Operations Center com as do CRM.</summary>
    /// <param name="maquinas">As máquinas do Operations Center.</param>
    /// <param name="doCrm">As máquinas do CRM, sem as baixadas.</param>
    /// <returns>As casadas, uma por máquina do CRM, e quantas deram cada desfecho.</returns>
    public static (IReadOnlyList<(long EquipamentoId, MaquinaNaTelemetria Maquina)> Casadas, IReadOnlyDictionary<DesfechoDaTelemetria, int> Desfechos)
        Casar(IReadOnlyList<MaquinaNaTelemetria> maquinas, IReadOnlyList<MaquinaDoCrmParaTelemetria> doCrm)
    {
        var porChassi = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var m in doCrm) porChassi.TryAdd(Chassi.Normalizar(m.Chassi), m.Id);

        // O NÚMERO DE SÉRIE SÓ VALE QUANDO APONTA UMA MÁQUINA SÓ: repetido em duas, ele não prova qual.
        var porSerie = doCrm
            .Where(m => !string.IsNullOrWhiteSpace(m.NumeroSerie))
            .GroupBy(m => Chassi.Normalizar(m.NumeroSerie), StringComparer.Ordinal)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.Single().Id, StringComparer.Ordinal);

        var desfechos = Enum.GetValues<DesfechoDaTelemetria>().ToDictionary(d => d, _ => 0);
        var escolhida = new Dictionary<long, (MaquinaNaTelemetria Maquina, DesfechoDaTelemetria Como)>();

        foreach (var maquina in maquinas)
        {
            var vin = Chassi.Normalizar(maquina.Vin);
            if (vin.Length == 0)
            {
                desfechos[DesfechoDaTelemetria.SemVin]++;
                continue;
            }

            (long Id, DesfechoDaTelemetria Como)? achada =
                porChassi.TryGetValue(vin, out var peloChassi) ? (peloChassi, DesfechoDaTelemetria.CasadaPeloChassi)
                : porSerie.TryGetValue(vin, out var pelaSerie) ? (pelaSerie, DesfechoDaTelemetria.CasadaPeloNumeroDeSerie)
                : null;

            if (achada is not { } a)
            {
                desfechos[DesfechoDaTelemetria.SemParNoCrm]++;
                continue;
            }

            if (escolhida.TryGetValue(a.Id, out var anterior))
            {
                desfechos[DesfechoDaTelemetria.Repetida]++;
                if (MaisNova(maquina, anterior.Maquina)) escolhida[a.Id] = (maquina, a.Como);
                continue;
            }

            escolhida[a.Id] = (maquina, a.Como);
        }

        foreach (var (_, (_, como)) in escolhida) desfechos[como]++;

        return ([.. escolhida.Select(e => (e.Key, e.Value.Maquina)).OrderBy(e => e.Key)], desfechos);
    }

    private static bool MaisNova(MaquinaNaTelemetria candidata, MaquinaNaTelemetria atual) =>
        (candidata.LeituraMaisRecente ?? DateTime.MinValue) > (atual.LeituraMaisRecente ?? DateTime.MinValue);
}
