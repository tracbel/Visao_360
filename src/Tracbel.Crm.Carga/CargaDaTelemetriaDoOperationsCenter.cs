using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Ibge;
using Tracbel.Crm.Integracao.OperationsCenter;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga da telemetria fez, contado.</summary>
/// <param name="Simulada">Se nada foi gravado.</param>
/// <param name="Contagens">Cada número, com o rótulo que o relatório imprime.</param>
/// <param name="Observacoes">O que o relatório precisa dizer além dos números.</param>
internal sealed record RelatorioDaTelemetria(bool Simulada, IReadOnlyList<(string Rotulo, int Valor)> Contagens, IReadOnlyList<string> Observacoes)
{
    /// <summary>O número de um rótulo; zero quando não houve.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public int Valor(string rotulo) => Contagens.FirstOrDefault(c => c.Rotulo == rotulo).Valor;
}

/// <summary>
/// A CARGA DA TELEMETRIA DO OPERATIONS CENTER (decisão de 28/09/2026) — a rotina <c>TELEMETRIA_OPERATIONS_CENTER</c>.
///
/// <para><b>O que ela grava</b>, em cada máquina do CRM que tem o mesmo chassi de uma máquina conectada: o horímetro
/// (<see cref="Equipamento.HorimetroAtual"/>, que existia desde o modelo inicial e nunca teve fonte), e a última posição,
/// com o município onde ela cai pela malha oficial do IBGE. Só a leitura mais nova entra — a regra é do
/// <see cref="Equipamento"/>.</para>
///
/// <para><b>O que ela NÃO faz</b>: criar máquina. A máquina conectada que não está no parque do CRM fica contada no
/// relatório, e só — quem cria máquina é o parque do Protheus e o ART, que dizem de quem ela é.</para>
///
/// <para><b>Sem a malha do IBGE, só o horímetro.</b> A posição nova sem o município iria para o banco dizendo "fora de
/// São Paulo" sem ser verdade; nessa rodada ela não é gravada, e o relatório diz por quê.</para>
///
/// <para><b>Reexecutável.</b> Relê tudo a cada rodada; a leitura igual à gravada não muda nada.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="lerMaquinas">A leitura do Operations Center.</param>
/// <param name="lerMalha">A malha municipal de São Paulo; nula quando o IBGE não respondeu.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
/// <remarks>
/// Não há usuário aqui, de propósito: a telemetria não carimba a alteração do cadastro (é leitura, não decisão), e quem
/// grava fica no registro da rodada, com a integração como origem.
/// </remarks>
internal sealed class CargaDaTelemetriaDoOperationsCenter(
    Func<CrmDbContext> abrirContexto,
    Func<Action<string>, CancellationToken, Task<Resultado<IReadOnlyList<MaquinaNaTelemetria>>>> lerMaquinas,
    Func<CancellationToken, Task<LocalizadorDeMunicipio?>> lerMalha,
    Action<string> relatar)
{
    /// <summary>O fluxo da trava e do registro de rodada.</summary>
    public const string Fluxo = "OPERATIONS_CENTER.TELEMETRIA";

    /// <summary>Rótulo: máquinas lidas do Operations Center.</summary>
    public const string MaquinasLidas = "máquinas no Operations Center";

    /// <summary>Rótulo: máquinas do CRM consideradas.</summary>
    public const string MaquinasDoCrm = "máquinas no parque do CRM";

    /// <summary>Rótulo: horímetros gravados.</summary>
    public const string HorimetrosGravados = "horímetros gravados (leitura mais nova)";

    /// <summary>Rótulo: horímetros iguais ao gravado.</summary>
    public const string HorimetrosMantidos = "horímetros sem leitura nova";

    /// <summary>Rótulo: horímetros negativos ou acima do plausível.</summary>
    public const string HorimetrosImplausiveis = "horímetros negativos ou acima de 100 mil horas, ignorados";

    /// <summary>Rótulo: posições gravadas.</summary>
    public const string PosicoesGravadas = "posições gravadas (leitura mais nova)";

    /// <summary>Rótulo: posições iguais à gravada.</summary>
    public const string PosicoesMantidas = "posições sem leitura nova";

    /// <summary>Rótulo: posições fora do Brasil.</summary>
    public const string PosicoesInvalidas = "posições inválidas ou fora do Brasil, ignoradas";

    /// <summary>Rótulo: posições fora de São Paulo.</summary>
    public const string PosicoesForaDeSp = "posições gravadas fora de São Paulo (sem município)";

    /// <summary>Rótulo: posições não gravadas porque a malha não veio.</summary>
    public const string PosicoesSemMalha = "posições não gravadas: a malha do IBGE não respondeu";

    /// <summary>O rótulo de cada desfecho do casamento, na ordem do relatório.</summary>
    public static readonly IReadOnlyDictionary<DesfechoDaTelemetria, string> RotuloDoDesfecho = new Dictionary<DesfechoDaTelemetria, string>
    {
        [DesfechoDaTelemetria.CasadaPeloChassi] = "casadas pelo chassi",
        [DesfechoDaTelemetria.CasadaPeloNumeroDeSerie] = "casadas pelo número de série",
        [DesfechoDaTelemetria.SemVin] = "sem chassi no Operations Center",
        [DesfechoDaTelemetria.SemParNoCrm] = "chassi fora do parque do CRM",
        [DesfechoDaTelemetria.Repetida] = "repetidas (vale a leitura mais nova)"
    };

    /// <summary>Executa a carga.</summary>
    /// <param name="simular">Lê, casa e conta, e não grava nada.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDaTelemetria>> ExecutarAsync(bool simular, CancellationToken ct)
    {
        var contagens = new List<(string, int)>();
        var observacoes = new List<string>();

        // ---- 1. o Operations Center ----
        relatar("  lendo o Operations Center (a leitura leva uns três minutos)…");
        var lidas = await lerMaquinas(relatar, ct);
        if (!lidas.EhSucesso) return Resultado<RelatorioDaTelemetria>.Indisponivel(lidas.Erro!);
        var maquinas = lidas.Valor;
        contagens.Add((MaquinasLidas, maquinas.Count));

        // ---- 2. a malha de São Paulo ----
        var localizador = await lerMalha(ct);
        if (localizador is null)
            observacoes.Add("A malha municipal do IBGE não respondeu: nesta rodada só o horímetro foi gravado. A posição volta na próxima.");

        // ---- 3. o parque do CRM e o casamento ----
        await using var banco = abrirContexto();
        await using var transacao = simular ? null : await banco.Database.BeginTransactionAsync(ct);

        var consulta = banco.Equipamentos.Where(e => e.ExcluidoEm == null);
        var equipamentos = simular ? await consulta.AsNoTracking().ToListAsync(ct) : await consulta.ToListAsync(ct);
        contagens.Add((MaquinasDoCrm, equipamentos.Count));

        var (casadas, desfechos) = CasamentoDaTelemetria.Casar(
            maquinas, [.. equipamentos.Select(e => new MaquinaDoCrmParaTelemetria(e.Id, e.Chassi.Numero, e.NumeroSerie))]);
        foreach (var (desfecho, rotulo) in RotuloDoDesfecho)
            contagens.Add((rotulo, desfechos.GetValueOrDefault(desfecho)));

        // O MUNICÍPIO DO CATÁLOGO PELO CÓDIGO IBGE: é a chave natural, e a malha fala em código.
        var municipioPorIbge = (await banco.Municipios.AsNoTracking()
                .Where(m => m.CodigoIbge != null)
                .Select(m => new { Codigo = m.CodigoIbge!.Value, m.Id })
                .ToListAsync(ct))
            .GroupBy(m => m.Codigo)
            .ToDictionary(g => g.Key, g => g.Min(m => m.Id));

        // ---- 4. a leitura de cada máquina casada — a regra de "só a mais nova" é do equipamento ----
        var porId = equipamentos.ToDictionary(e => e.Id);
        int horimetros = 0, horimetrosMantidos = 0, implausiveis = 0;
        int posicoes = 0, posicoesMantidas = 0, invalidas = 0, foraDeSp = 0, semMalha = 0;

        foreach (var (id, maquina) in casadas)
        {
            var equipamento = porId[id];

            if (maquina.Horas is { } horas && maquina.HorasEmUtc is { } horasEm)
            {
                if (!Equipamento.HorimetroPlausivel(horas)) implausiveis++;
                else if (equipamento.RegistrarHorimetro(horas, horasEm)) horimetros++;
                else horimetrosMantidos++;
            }

            if (maquina.Latitude is not { } latitude || maquina.Longitude is not { } longitude || maquina.PosicaoEmUtc is not { } posicaoEm)
                continue;

            if (!Coordenada.TentarCriar(latitude, longitude, out var coordenada))
            {
                invalidas++;
                continue;
            }

            if (equipamento.PosicaoEm is { } gravada && posicaoEm <= gravada)
            {
                posicoesMantidas++;
                continue;
            }

            if (localizador is null)
            {
                semMalha++;
                continue;
            }

            // O PONTO FORA DA MALHA NÃO GANHA MUNICÍPIO: é fora de São Paulo, e não o vizinho mais perto.
            var codigo = localizador.CodigoIbgeDe(longitude, latitude);
            int? municipioId = codigo is { } c && municipioPorIbge.TryGetValue(c, out var doCatalogo) ? doCatalogo : null;
            if (municipioId is null) foraDeSp++;

            equipamento.RegistrarPosicao(coordenada, posicaoEm, municipioId);
            posicoes++;
        }

        contagens.Add((HorimetrosGravados, horimetros));
        contagens.Add((HorimetrosMantidos, horimetrosMantidos));
        contagens.Add((HorimetrosImplausiveis, implausiveis));
        contagens.Add((PosicoesGravadas, posicoes));
        contagens.Add((PosicoesMantidas, posicoesMantidas));
        contagens.Add((PosicoesInvalidas, invalidas));
        contagens.Add((PosicoesForaDeSp, foraDeSp));
        if (semMalha > 0) contagens.Add((PosicoesSemMalha, semMalha));

        if (simular || transacao is null)
            return Resultado<RelatorioDaTelemetria>.Ok(new RelatorioDaTelemetria(true, contagens, observacoes));

        // ---- 5. a gravação, na transação aberta antes da leitura do parque ----
        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDoOperationsCenter.CodigoDoSistema, "Operations Center — telemetria John Deere",
            "MySQL do BI, somente leitura", ct);
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        await banco.SaveChangesAsync(ct);

        var leituraMaisNova = maquinas.Select(m => m.LeituraMaisRecente).Max();
        await CargaDeTerritorio.RegistrarRodadaAsync(
            banco, sistemaId, Fluxo, maquinas.Count, horimetros + posicoes, implausiveis + invalidas, ct,
            leituraMaisNova?.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        await transacao.CommitAsync(ct);

        return Resultado<RelatorioDaTelemetria>.Ok(new RelatorioDaTelemetria(false, contagens, observacoes));
    }
}
