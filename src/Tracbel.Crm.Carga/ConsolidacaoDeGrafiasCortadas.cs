using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Ibge;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A CONFERÊNCIA DOS ENDEREÇOS EM GRAFIA CORTADA — uma peça só, chamada pelas duas cargas
/// (documento 32, seção 4.6).
///
/// <para><b>Por que é uma peça própria.</b> A correção nasceu dentro da carga do território, e a
/// recarga do sistema de origem a desfazia: o ponteiro de cidade da origem continua apontando para a
/// linha cortada. Depender de alguém lembrar de rodar a carga do território depois da do cadastro é o
/// tipo de passo que se esquece. Agora a carga do cadastro (a) não desfaz a correção cuja evidência não
/// mudou — ver <see cref="Endereco.CorrecaoDeGrafiaCortadaSeMantem"/> — e (b) roda esta conferência ao
/// fim, para o endereço novo ou alterado. A carga do território roda a mesma.</para>
///
/// <para><b>A regra de correção é a autorizada em 13/09/2026, e nenhuma outra:</b> a linha cortada é
/// prefixo de UM único município oficial da mesma UF, já reconhecido no catálogo com código IBGE; a UF
/// do endereço é a do município; e, havendo coordenada, ela cai dentro do contorno oficial. Endereço
/// nunca é associado pelo texto escrito nele. Quando uma evidência discorda, ele fica onde está e o
/// motivo vai para a fila de revisão — a cada rodada, enquanto a discordância durar.</para>
///
/// <para><b>Sem o IBGE, a carga não para.</b> As grafias cortadas saem do próprio catálogo reconhecido
/// (<see cref="SaneamentoDeTerritorio.VariantesCortadasDoCatalogo"/>); só o contorno vem da API. Se ela
/// não responder, o endereço sem coordenada ainda é corrigido pelas outras condições, e o que tem
/// coordenada fica pendente, com o motivo dito.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="lerMalhaDaUf">Lê a malha municipal oficial de uma UF, pelo código IBGE da UF.</param>
/// <param name="usuarioId">Quem roda a carga — vai para a trilha de auditoria.</param>
internal sealed class ConsolidacaoDeGrafiasCortadas(
    Func<CrmDbContext> abrirContexto,
    Func<int, CancellationToken, Task<IReadOnlyDictionary<int, PoligonoMunicipal>>> lerMalhaDaUf,
    long usuarioId)
{
    /// <summary>O fluxo da fila de revisão e do ponto de sincronismo.</summary>
    public const string Fluxo = "IBGE.GRAFIA_CORTADA";

    /// <summary>
    /// Quanto a leitura do contorno de uma UF pode esperar — menos que o cliente HTTP. O contorno só
    /// serve de conferência: numa rede que descarta pacotes, a carga do cadastro não fica minutos
    /// parada por ele, e o endereço com coordenada fica pendente para a próxima rodada.
    /// </summary>
    private static readonly TimeSpan PrazoPorUf = TimeSpan.FromSeconds(90);

    /// <summary>
    /// Para cada linha cortada do catálogo, a linha do município oficial — o que a carga do cadastro
    /// consulta para não desfazer uma correção.
    /// </summary>
    /// <param name="contexto">Um contexto aberto.</param>
    /// <param name="ct">Cancelamento.</param>
    public static async Task<IReadOnlyDictionary<int, int>> OficialDeCadaGrafiaAsync(CrmDbContext contexto, CancellationToken ct)
    {
        var (cortadas, linhaOficial) = await LerCatalogoAsync(contexto, ct);

        return cortadas
            .Where(v => linhaOficial.ContainsKey(v.Oficial.Codigo))
            .ToDictionary(v => v.MunicipioId, v => linhaOficial[v.Oficial.Codigo]);
    }

    /// <summary>Confere os endereços que estão nas grafias cortadas e corrige os inequívocos.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<ResultadoDaConsolidacao> ExecutarAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var (cortadas, linhaOficial) = await LerCatalogoAsync(contexto, ct);

        // ENDEREÇO EXCLUÍDO NÃO ENTRA: registro inativado não é corrigido, e contá-lo como pendente a
        // cada rodada incharia a fila de revisão com o que ninguém vai conferir.
        var idsDasGrafias = cortadas.Select(v => v.MunicipioId).ToList();
        var enderecosPorGrafia = (await contexto.Enderecos
                .Where(e => e.ExcluidoEm == null && e.MunicipioId != null && idsDasGrafias.Contains(e.MunicipioId.Value))
                .OrderBy(e => e.Id)
                .ToListAsync(ct))
            .ToLookup(e => e.MunicipioId!.Value);

        // O CONTORNO SÓ É LIDO QUANDO HÁ O QUE CONFERIR — endereço com coordenada. Sem isso, toda recarga
        // do sistema de origem esperaria a API do IBGE à toa.
        var ufsParaConferir = cortadas
            .Where(v => enderecosPorGrafia[v.MunicipioId].Any(e => e.Latitude is not null && e.Longitude is not null))
            .Select(v => v.Oficial.Codigo / 100_000);

        var (malha, contornoDisponivel) = await LerContornosAsync(ufsParaConferir, ct);

        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            contexto, "IBGE", "IBGE — localidades e SIDRA", "REST público, somente leitura", ct);

        var recusas = new List<(object Conteudo, string Motivo)>();
        int lidos = 0, reapontados = 0, pendentes = 0;

        foreach (var variante in cortadas)
        {
            if (!linhaOficial.TryGetValue(variante.Oficial.Codigo, out var oficialId))
            {
                recusas.Add((variante, "O município oficial não tem linha reconhecida no catálogo; nada foi reapontado."));
                continue;
            }

            var enderecos = enderecosPorGrafia[variante.MunicipioId].ToList();
            lidos += enderecos.Count;

            foreach (var endereco in enderecos)
            {
                var motivo = MotivoParaNaoCorrigir(endereco, variante.Oficial, malha, contornoDisponivel);

                if (motivo is not null)
                {
                    pendentes++;
                    recusas.Add((new
                    {
                        EnderecoId = endereco.Id,
                        GrafiaCortadaId = variante.MunicipioId,
                        variante.Nome,
                        CodigoIbge = variante.Oficial.Codigo
                    }, motivo));
                    continue;
                }

                contexto.AlteracoesDeCampo.Add(AlteracaoDeCampo.Registrar(
                    endereco.EmpresaId, nameof(Endereco), endereco.Id, nameof(Endereco.MunicipioId),
                    $"{variante.MunicipioId} ({variante.Nome})",
                    $"{oficialId} ({variante.Oficial.Nome}, IBGE {variante.Oficial.Codigo})",
                    usuarioId));

                endereco.ReapontarMunicipioDoCatalogo(oficialId, usuarioId);
                reapontados++;
            }
        }

        await CargaDeTerritorio.SubstituirRecusasAsync(contexto, Fluxo, recusas, ct);
        await contexto.SaveChangesAsync(ct);
        await CargaDeTerritorio.RegistrarRodadaAsync(contexto, sistemaId, Fluxo, lidos, reapontados, pendentes, ct);
        await transacao.CommitAsync(ct);

        return new ResultadoDaConsolidacao(cortadas.Count, lidos, reapontados, pendentes, contornoDisponivel);
    }

    private static async Task<(IReadOnlyList<VarianteDeGrafia> Cortadas, IReadOnlyDictionary<int, int> LinhaOficial)> LerCatalogoAsync(
        CrmDbContext contexto, CancellationToken ct)
    {
        var catalogo = await contexto.Municipios.AsNoTracking()
            .Select(m => new MunicipioDoCatalogo(m.Id, m.Nome, m.Uf, m.CodigoIbge))
            .ToListAsync(ct);

        var linhaOficial = catalogo
            .Where(m => m.CodigoIbge is not null)
            .GroupBy(m => m.CodigoIbge!.Value)
            .ToDictionary(g => g.Key, g => g.Min(m => m.Id));

        return (SaneamentoDeTerritorio.VariantesCortadasDoCatalogo(catalogo), linhaOficial);
    }

    private async Task<(IReadOnlyDictionary<int, PoligonoMunicipal> Malha, bool Disponivel)> LerContornosAsync(
        IEnumerable<int> ufs, CancellationToken ct)
    {
        var malha = new Dictionary<int, PoligonoMunicipal>();
        var disponivel = true;

        // O CÓDIGO DA UF SÃO OS DOIS PRIMEIROS DÍGITOS do código IBGE do município (3549805 → 35).
        foreach (var uf in ufs.Distinct().Order())
        {
            using var prazo = CancellationTokenSource.CreateLinkedTokenSource(ct);
            prazo.CancelAfter(PrazoPorUf);

            try
            {
                foreach (var (codigo, poligono) in await lerMalhaDaUf(uf, prazo.Token))
                    malha[codigo] = poligono;
            }
            catch (Exception falha) when (!ct.IsCancellationRequested
                                          && falha is HttpRequestException or OperationCanceledException
                                              or JsonException or InvalidDataException)
            {
                disponivel = false;
            }
        }

        return (malha, disponivel);
    }

    /// <summary>
    /// Por que um endereço NÃO pode ir para o município oficial — ou nulo quando nada discorda.
    /// </summary>
    private static string? MotivoParaNaoCorrigir(
        Endereco endereco,
        MunicipioDoIbge oficial,
        IReadOnlyDictionary<int, PoligonoMunicipal> malha,
        bool contornoDisponivel)
    {
        if (endereco.EstaExcluido)
            return "Endereço excluído: registro inativado não é corrigido.";

        if (!string.Equals(endereco.Uf, oficial.Uf, StringComparison.OrdinalIgnoreCase))
            return $"A UF do endereço ({endereco.Uf}) não é a do município oficial ({oficial.Uf}).";

        if (endereco.Latitude is not { } latitude || endereco.Longitude is not { } longitude)
            return null;

        if (!malha.TryGetValue(oficial.Codigo, out var contorno))
            return contornoDisponivel
                ? "O endereço tem coordenada e não há contorno oficial carregado para conferir o município."
                : "O endereço tem coordenada e o contorno oficial do IBGE não pôde ser lido nesta rodada: a conferência fica para a próxima.";

        if (contorno.Contem((double)longitude, (double)latitude))
            return null;

        var ondeCai = malha.FirstOrDefault(m => m.Value.Contem((double)longitude, (double)latitude)).Key;
        return ondeCai == 0
            ? $"A coordenada do endereço cai fora da UF carregada, e não em {oficial.Nome}."
            : $"A coordenada do endereço cai no município IBGE {ondeCai}, e não em {oficial.Nome} ({oficial.Codigo}).";
    }
}

/// <summary>O que a conferência das grafias cortadas fez.</summary>
/// <param name="Grafias">Linhas cortadas do catálogo com município oficial reconhecido.</param>
/// <param name="EnderecosLidos">Endereços encontrados nessas linhas nesta rodada.</param>
/// <param name="Reapontados">Endereços corrigidos para o município oficial, com trilha.</param>
/// <param name="Pendentes">Endereços que ficaram onde estavam, com o motivo na fila de revisão.</param>
/// <param name="ContornoDisponivel">
/// Se o contorno oficial que a rodada precisava foi lido — verdadeiro também quando nenhum endereço com
/// coordenada precisou dele.
/// </param>
internal sealed record ResultadoDaConsolidacao(
    int Grafias, int EnderecosLidos, int Reapontados, int Pendentes, bool ContornoDisponivel);
