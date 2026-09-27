using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Vortice;
using ProcessoDoCrm = Tracbel.Crm.Dominio.Processo.Processo;
using ResultadoDoCrm = Tracbel.Crm.Dominio.Processo.Resultado;

namespace Tracbel.Crm.Carga;

/// <summary>O que a onda 2 fez (ou faria, na simulação), em número — sem nome nem documento.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDasOportunidadesDoVortice(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// A ONDA 2 DOS PROCESSOS DO VÓRTICE — as oportunidades, a agenda e a linha do tempo dos clientes casados (documento
/// 52 §12; decisões do Ricardo de 27/09/2026). É o segundo modo da rotina <c>PROCESSOS_VORTICE</c>, depois do funil.
///
/// <para><b>O universo.</b> Os processos 31/41/50 abertos desde 01/11/2023, de filial que o CRM tem e de pessoa que casa
/// pelo CPF/CNPJ com UM cliente ativo — o prospect fica só no funil, que já o conta. Deles vêm
/// <c>processo.Processo</c>, <c>processo.Tarefa</c> e <c>processo.Interacao</c>, e a linha do funil ganha o processo do
/// CRM (<c>EstagioDoProcesso.ProcessoId</c>).</para>
///
/// <para><b>As decisões.</b> Só o <c>Resumo</c> entra de texto livre, como título (P1). Tarefa ou interação cujo
/// responsável não tem conta vai para o dono do processo, contada (P2). O perdido leva o motivo e o concorrente da venda
/// perdida principal do mesmo processo; sem formulário, "não informado na origem" (P8). Tarefa pendente só entra se o
/// processo está aberto (P10). Tarefa sem processo não entra (P6); o pai DNA entra como processo (P7).</para>
///
/// <para><b>O que NÃO faz.</b> Não cria cliente, carteira nem usuário, e não grava o último contato — isso é da
/// <c>CARTEIRAS_VORTICE</c>. Interação só é incluída: o que aconteceu não muda de forma.</para>
///
/// <para><b>Como o funil</b>: planeja só com leitura, aborta com leitura vazia ou queda de mais de 5%, simula sem
/// transação e grava em blocos, cada um a sua transação — a rodada que cai no meio converge na próxima.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="ler">A leitura da onda 2 no Vórtice.</param>
/// <param name="deParaDeFiliais">A filial do Vórtice (<c>NN</c>) para a filial ativa do CRM.</param>
/// <param name="usuarioId">Quem roda a rotina — e o dono de último recurso (P2).</param>
/// <param name="relogio">O relógio (UTC).</param>
/// <param name="relatar">Onde a rotina escreve o andamento.</param>
/// <param name="tamanhoDoBloco">Registros por bloco de gravação.</param>
internal sealed class CargaDasOportunidadesDoVortice(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<LeituraDasOportunidadesDoVortice>>> ler,
    IReadOnlyDictionary<int, int> deParaDeFiliais,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar,
    int tamanhoDoBloco = CargaDoFunilDoVortice.TamanhoDoBloco)
{
    /// <summary>O fluxo da execução — e o nome da trava.</summary>
    internal const string Fluxo = "VORTICE.OPORTUNIDADES";

    /// <summary>O tipo de tarefa da ação que a origem não declara — o mesmo código da carga antiga.</summary>
    internal const string TipoDeTarefaNaoInformada = "NAO_INFORMADA";

    /// <summary>O código de fase da fase em branco na origem — o mesmo da carga antiga.</summary>
    internal const string FaseNaoInformada = "NAO_INFORMADA";

    /// <summary>Rótulo: processos no universo.</summary>
    internal const string RotuloDeProcessosNoUniverso = "processos no universo (casados, na janela, de filial do CRM)";

    /// <summary>Rótulo: processos incluídos.</summary>
    internal const string RotuloDeProcessosIncluidos = "processos incluídos";

    /// <summary>Rótulo: processos alterados.</summary>
    internal const string RotuloDeProcessosAlterados = "processos alterados pela origem";

    /// <summary>Rótulo: processos excluídos.</summary>
    internal const string RotuloDeProcessosExcluidos = "processos excluídos (saíram do universo)";

    /// <summary>Rótulo: tarefas incluídas.</summary>
    internal const string RotuloDeTarefasIncluidas = "tarefas incluídas";

    /// <summary>Rótulo: tarefas alteradas.</summary>
    internal const string RotuloDeTarefasAlteradas = "tarefas alteradas pela origem";

    /// <summary>Rótulo: tarefas excluídas.</summary>
    internal const string RotuloDeTarefasExcluidas = "tarefas excluídas (saíram do universo)";

    /// <summary>Rótulo: interações incluídas.</summary>
    internal const string RotuloDeInteracoesIncluidas = "interações incluídas";

    // AS ENTIDADES DO DE-PARA, com os nomes da carga antiga (o CHECK de ChaveExterna aceita estes). Escritas por extenso:
    // nameof de um alias devolveria o nome do alias.
    private const string EntidadeProcesso = "Processo";
    private const string EntidadeTarefa = nameof(Tarefa);
    private const string EntidadeInteracao = nameof(Interacao);
    private const string EntidadeTipoProcesso = nameof(TipoProcesso);
    private const string EntidadeTipoTarefa = nameof(TipoTarefa);
    private const string EntidadeResultado = "Resultado";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>Executa a rotina.</summary>
    /// <param name="simular">Só planeja e conta: não abre transação de escrita.</param>
    /// <param name="aceitarQueda">Passa por cima da trava de queda — só pelo terminal, como no funil.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDasOportunidadesDoVortice>> ExecutarAsync(bool simular, bool aceitarQueda, CancellationToken ct)
    {
        relatar("Lendo processos, agenda e histórico 31/41/50 do Vórtice desde 01/11/2023 (sessão somente leitura)…");
        var leitura = await ler(ct);
        if (!leitura.EhSucesso) return Falha(leitura.Erro!);

        var origem = leitura.Valor;

        // A ORIGEM VAZIA NÃO É UMA AGENDA VAZIA: é leitura que falhou sem dizer. Sincronizar contra ela excluiria tudo.
        if (origem.Processos.Count == 0 || origem.Tarefas.Count == 0 || origem.Interacoes.Count == 0)
            return Falha(
                $"O Vórtice devolveu {origem.Processos.Count} processo(s), {origem.Tarefas.Count} linha(s) de agenda e " +
                $"{origem.Interacoes.Count} linha(s) de histórico. Leitura vazia não é carteira sem movimento, é leitura a " +
                "conferir — nada foi excluído e nada foi gravado.");

        var agora = relogio();
        Plano plano;
        await using (var banco = abrirContexto())
        {
            plano = await PlanejarAsync(banco, origem, agora, ct);
        }

        Relatar(origem, plano);

        var quedas = new List<string>();
        if (Queda(plano.ProcessosGeridosAtivos, plano.Processos.Count) is { } quedaDeProcessos)
            quedas.Add($"os processos cairiam de {plano.ProcessosGeridosAtivos:N0} para {plano.Processos.Count:N0} ({quedaDeProcessos:P1})");
        if (Queda(plano.TarefasGeridasAtivas, plano.Tarefas.Count) is { } quedaDeTarefas)
            quedas.Add($"as tarefas cairiam de {plano.TarefasGeridasAtivas:N0} para {plano.Tarefas.Count:N0} ({quedaDeTarefas:P1})");

        if (quedas.Count > 0)
        {
            var descricao = string.Join("; ", quedas);
            if (!aceitarQueda)
                return Falha(
                    $"{char.ToUpperInvariant(descricao[0])}{descricao[1..]} — mais do que os {CargaDoFunilDoVortice.QuedaMaximaAceita:P0} que " +
                    "uma rodada aceita. Isso costuma ser leitura a conferir: nada foi excluído e nada foi gravado. Se a queda é " +
                    $"legítima, rode no terminal com {CargaDoFunilDoVortice.OpcaoDeAceitarQueda}.");

            plano.QuedaAceita = $"Queda aceita por {CargaDoFunilDoVortice.OpcaoDeAceitarQueda}, no terminal: {descricao}.";
            _observacoes.Add(plano.QuedaAceita);
            relatar("  " + plano.QuedaAceita);
        }

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura, e nenhuma transação de escrita foi aberta.");
            return Resultado<RelatorioDasOportunidadesDoVortice>.Ok(new RelatorioDasOportunidadesDoVortice(true, _contagens, _observacoes));
        }

        var gravacao = await AplicarAsync(origem, plano, agora, ct);
        return gravacao.EhSucesso
            ? Resultado<RelatorioDasOportunidadesDoVortice>.Ok(new RelatorioDasOportunidadesDoVortice(false, _contagens, _observacoes))
            : Falha(gravacao.Erro!);
    }

    private static Resultado<RelatorioDasOportunidadesDoVortice> Falha(string mensagem) =>
        Resultado<RelatorioDasOportunidadesDoVortice>.Indisponivel(mensagem);

    private static double? Queda(int antes, int depois)
    {
        if (antes == 0 || depois >= antes) return null;
        var queda = (antes - depois) / (double)antes;
        return queda > CargaDoFunilDoVortice.QuedaMaximaAceita ? queda : null;
    }

    /// <summary>A data crível: de 2000 até agora + 1 dia — a mesma faixa da regra do estágio.</summary>
    private static bool Crivel(DateTime? utc, DateTime agora) =>
        utc is { } data && data >= RegraDoEstagio.MenorAberturaCrivel && data <= agora + RegraDoEstagio.ToleranciaDeDataFutura;

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private async Task<Plano> PlanejarAsync(CrmDbContext banco, LeituraDasOportunidadesDoVortice origem, DateTime agora, CancellationToken ct)
    {
        var plano = new Plano
        {
            SistemaId = await banco.Sistemas.AsNoTracking()
                .Where(s => s.Codigo == LeitorDeCargaDoVortice.CodigoDoSistema)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync(ct)
        };

        var ligacoes = await LigacoesComOCrm.LerAsync(banco, plano.SistemaId, ct);
        var mapas = await MapasAsync(banco, plano.SistemaId, origem, ct);
        var logins = origem.Logins.GroupBy(l => l.SeqUsuario).ToDictionary(g => g.Key, g => g.First().Login);

        // A VENDA PERDIDA PRINCIPAL DE CADA PROCESSO (P8): o motivo e o concorrente que o formulário declarou.
        var perdas = (await banco.VendasPerdidas.AsNoTracking()
                .Where(v => v.ExcluidoEm == null && v.Papel == PapelDaVendaPerdida.Principal && v.NumeroDoProcessoNaOrigem != null)
                .Select(v => new { Numero = v.NumeroDoProcessoNaOrigem!.Value, v.MotivoDePerdaId, v.ConcorrenteId, v.RegistradaEm })
                .ToListAsync(ct))
            .GroupBy(v => v.Numero)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(v => v.RegistradaEm).First());

        // ---------------------------------------------------------------------------------------------
        // O universo dos processos.
        // ---------------------------------------------------------------------------------------------
        foreach (var p in origem.Processos.GroupBy(p => p.Numero).Select(g => g.First()))
        {
            // A ABERTURA CRÍVEL: a inclusão, ou — nula ou absurda — o primeiro andamento. A regra do funil.
            DateTime? abertura = Crivel(p.IncluidoEmUtc, agora) ? p.IncluidoEmUtc
                : Crivel(p.PrimeiroAndamentoEmUtc, agora) ? p.PrimeiroAndamentoEmUtc
                : null;

            if (abertura is not { } aberto || aberto < RegraDoEstagio.InicioDaJanela) { plano.ForaDaJanela++; continue; }
            if (p.NroEmpresa is not { } nn || !deParaDeFiliais.TryGetValue(nn, out var empresaId)) { plano.FilialForaDoCrm++; continue; }

            var casamento = ligacoes.Casar(p.Documento);
            if (casamento.ClienteId is not { } clienteId)
            {
                plano.NaoCasados.TryGetValue(casamento.Motivo!, out var ja);
                plano.NaoCasados[casamento.Motivo!] = ja + 1;
                continue;
            }

            var ligacao = ligacoes.Do(p.Documento, p.SeqCarteira, p.LoginDoResponsavel);
            var dono = ligacao.ResponsavelId ?? usuarioId;
            if (ligacao.ResponsavelPeloLogin) plano.DonoPeloLogin++;
            else if (ligacao.ResponsavelId is not null) plano.DonoPelaCarteira++;
            else plano.DonoPeloOperador++;

            var situacao = SaneamentoDeProcesso.Situacao(p.Status, []).Situacao;
            var faseDesde = Crivel(p.FaseDesdeUtc, agora) ? p.FaseDesdeUtc!.Value : aberto;
            var situacaoDesde = Crivel(p.StatusDesdeUtc, agora) ? p.StatusDesdeUtc!.Value : aberto;
            var encerra = situacao is not (SituacaoDoProcesso.Aberto or SituacaoDoProcesso.Suspenso);

            // O ENCERRAMENTO TEM DATA (CK_Processo_Encerramento): a realização crível, senão desde quando está na situação.
            DateTime? concluidoEm = !encerra ? null : Crivel(p.RealizadoEmUtc, agora) ? p.RealizadoEmUtc : situacaoDesde;

            var perda = situacao is SituacaoDoProcesso.Perdido && perdas.TryGetValue(p.Numero, out var principal) ? principal : null;
            if (situacao is SituacaoDoProcesso.Perdido)
            {
                if (perda is null) plano.PerdidosSemFormulario++;
                else plano.PerdidosComMotivoDoFormulario++;
            }

            var fase = (p.Fase ?? string.Empty).Trim();
            var codigoDaFase = fase.Length == 0 ? FaseNaoInformada : SaneamentoDeProcesso.Codificar(fase, 40);
            if (codigoDaFase.Length == 0) codigoDaFase = FaseNaoInformada;

            var resumo = (p.Resumo ?? string.Empty).Trim();
            var titulo = resumo.Length > 0
                ? Limitar(resumo, 200)!
                : $"{mapas.NomeDoTipo(p.Tipo)} · nº {p.Numero.ToString(CultureInfo.InvariantCulture)}";
            if (resumo.Length == 0) plano.TitulosCompostos++;

            plano.Processos.Add(new ProcessoPlanejado(
                p, empresaId, clienteId, ligacao.CarteiraId, dono, aberto, faseDesde, situacao, situacaoDesde, concluidoEm,
                codigoDaFase, fase.Length == 0 ? "Não informada na origem" : Limitar(fase, 80)!, (short)Math.Clamp(p.FaseOrdem ?? 0, 0, short.MaxValue),
                perda?.MotivoDePerdaId, perda?.ConcorrenteId, titulo,
                p.Valor is > 0 && Dinheiro.TentarCriar(p.Valor.Value, out var valor) ? valor : null,
                p.Quantidade is > 0 ? p.Quantidade : null));
        }

        var porNumero = plano.Processos.ToDictionary(p => p.Origem.Numero);

        // ---------------------------------------------------------------------------------------------
        // As tarefas — só as de processo do universo; a pendente, só de processo aberto (P10).
        // ---------------------------------------------------------------------------------------------
        foreach (var t in origem.Tarefas.GroupBy(t => t.SeqAgenda).Select(g => g.First()))
        {
            if (!porNumero.TryGetValue(t.Processo, out var processo)) continue;
            if (t.AgendadaParaUtc is not { } agendada || !DataHoraUtc.TentarCriar(agendada, out _)) { plano.TarefasSemData++; continue; }

            var resultadoConhecido = t.Resultado is { } r && mapas.ResultadoConhecido(r);
            var situacao = SaneamentoDeProcesso.SituacaoDaTarefaDe(
                t.Realizada, t.RealizadaEmUtc is not null, resultadoConhecido, t.SeqUsuarioQueConcluiu is not null);

            if (situacao is SituacaoDaTarefa.Pendente && processo.Situacao is not (SituacaoDoProcesso.Aberto or SituacaoDoProcesso.Suspenso))
            {
                plano.PendentesDeProcessoEncerrado++;
                continue;
            }

            var responsavel = ligacoes.Conta(t.SeqUsuario, t.SeqUsuario is { } u ? logins.GetValueOrDefault(u) : null);
            if (responsavel is null) plano.TarefasComDono++;

            long? concluidaPor = null;
            if (situacao is SituacaoDaTarefa.Concluida)
            {
                concluidaPor = ligacoes.Conta(t.SeqUsuarioQueConcluiu, logins.GetValueOrDefault(t.SeqUsuarioQueConcluiu!.Value));
                if (concluidaPor is null) plano.ConclusoesComDono++;
                concluidaPor ??= processo.Dono;
            }

            plano.Tarefas.Add(new TarefaPlanejada(
                t, processo, responsavel ?? processo.Dono, situacao, concluidaPor,
                t.Prioridade is >= 1 and <= 5 ? (short)t.Prioridade.Value : (short)3,
                Crivel(t.PrazoLimiteUtc, agora.AddYears(5)) ? t.PrazoLimiteUtc : null));
        }

        // ---------------------------------------------------------------------------------------------
        // As interações — só as novas: o que aconteceu não muda de forma.
        // ---------------------------------------------------------------------------------------------
        var chaves = await ChavesAsync(banco, plano.SistemaId, ct);
        foreach (var i in origem.Interacoes.GroupBy(i => i.SeqHistorico).Select(g => g.First()))
        {
            if (!porNumero.TryGetValue(i.Processo, out var processo)) continue;
            if (chaves.Interacoes.ContainsKey(Texto(i.SeqHistorico))) { plano.InteracoesJaNoCrm++; continue; }
            if (i.RealizadaEmUtc is not { } ocorrida || !Crivel(ocorrida, agora) || !DataHoraUtc.TentarCriar(ocorrida, out _))
            {
                plano.InteracoesSemData++;
                continue;
            }

            var autor = ligacoes.Conta(i.SeqUsuario, i.SeqUsuario is { } u ? logins.GetValueOrDefault(u) : null);
            if (autor is null) plano.InteracoesComDono++;
            if (i.DeSistema) plano.InteracoesDeSistema++;

            plano.Interacoes.Add(new InteracaoPlanejada(i, processo, autor ?? processo.Dono));
        }

        // ---------------------------------------------------------------------------------------------
        // O que o CRM já tem: o que muda, o que volta e o que sai.
        // ---------------------------------------------------------------------------------------------
        var numerosDesejados = porNumero.Keys.Select(Texto).ToHashSet(StringComparer.Ordinal);
        var processosGeridos = await CarregarAsync(banco.Set<ProcessoDoCrm>(), chaves.Processos.Values, ct);
        plano.ProcessosGeridosAtivos = processosGeridos.Values.Count(p => !p.EstaExcluido);

        foreach (var planejado in plano.Processos)
        {
            if (!chaves.Processos.TryGetValue(Texto(planejado.Origem.Numero), out var id) || !processosGeridos.TryGetValue(id, out var existente))
            {
                plano.ProcessosAIncluir++;
                continue;
            }

            if (existente.EstaExcluido) plano.ProcessosARestaurar++;
            if (existente.RetratoDaOrigem != ProcessoDoCrm.Normalizar(planejado.Retrato(mapas))) plano.ProcessosAAlterar++;
        }

        plano.ProcessosAExcluir.AddRange(chaves.Processos
            .Where(c => !numerosDesejados.Contains(c.Key) && processosGeridos.TryGetValue(c.Value, out var p) && !p.EstaExcluido)
            .Select(c => c.Value));

        var agendasDesejadas = plano.Tarefas.Select(t => Texto(t.Origem.SeqAgenda)).ToHashSet(StringComparer.Ordinal);
        var tarefasGeridas = await CarregarAsync(banco.Tarefas, chaves.Tarefas.Values, ct);
        plano.TarefasGeridasAtivas = tarefasGeridas.Values.Count(t => !t.EstaExcluido);

        foreach (var planejada in plano.Tarefas)
        {
            if (!chaves.Tarefas.TryGetValue(Texto(planejada.Origem.SeqAgenda), out var id) || !tarefasGeridas.TryGetValue(id, out var existente))
            {
                plano.TarefasAIncluir++;
                continue;
            }

            if (existente.EstaExcluido) plano.TarefasARestaurar++;
            var processoId = chaves.Processos.GetValueOrDefault(Texto(planejada.Processo.Origem.Numero));
            if (existente.RetratoDaOrigem != Tarefa.Normalizar(planejada.Retrato(mapas, processoId))) plano.TarefasAAlterar++;
        }

        plano.TarefasAExcluir.AddRange(chaves.Tarefas
            .Where(c => !agendasDesejadas.Contains(c.Key) && tarefasGeridas.TryGetValue(c.Value, out var t) && !t.EstaExcluido)
            .Select(c => c.Value));

        plano.CatalogosNovos = mapas.QuantosFaltam(plano);
        return plano;
    }

    private static string Texto(long valor) => valor.ToString(CultureInfo.InvariantCulture);

    private static string? Limitar(string? texto, int tamanho) => texto is null || texto.Length <= tamanho ? texto : texto[..tamanho].TrimEnd();

    /// <summary>As entidades geridas, por id, lidas em fatias sem rastreador.</summary>
    private static async Task<Dictionary<long, T>> CarregarAsync<T>(IQueryable<T> conjunto, IEnumerable<long> ids, CancellationToken ct)
        where T : EntidadeBase
    {
        var porId = new Dictionary<long, T>();
        foreach (var fatia in ids.Distinct().Chunk(CargaDoFunilDoVortice.IdsPorConsulta))
        {
            var lista = fatia.ToList();
            foreach (var entidade in await conjunto.AsNoTracking().Where(e => lista.Contains(e.Id)).ToListAsync(ct))
                porId[entidade.Id] = entidade;
        }

        return porId;
    }

    private static async Task<Chaves> ChavesAsync(CrmDbContext banco, int? sistemaId, CancellationToken ct)
    {
        var chaves = new Chaves();
        if (sistemaId is not { } sistema) return chaves;

        foreach (var chave in await banco.ChavesExternas.AsNoTracking()
                     .Where(c => c.SistemaId == sistema
                                 && (c.Entidade == EntidadeProcesso || c.Entidade == EntidadeTarefa || c.Entidade == EntidadeInteracao))
                     .Select(c => new { c.Entidade, c.ChaveOrigem, c.RegistroId })
                     .ToListAsync(ct))
        {
            var destino = chave.Entidade switch
            {
                EntidadeProcesso => chaves.Processos,
                EntidadeTarefa => chaves.Tarefas,
                _ => chaves.Interacoes
            };
            destino[chave.ChaveOrigem] = chave.RegistroId;
        }

        return chaves;
    }

    // =============================================================================================
    // Os catálogos — lidos no plano, completados na gravação
    // =============================================================================================

    private static async Task<Mapas> MapasAsync(
        CrmDbContext banco, int? sistemaId, LeituraDasOportunidadesDoVortice origem, CancellationToken ct)
    {
        var mapas = new Mapas(origem);

        if (sistemaId is { } sistema)
        {
            foreach (var chave in await banco.ChavesExternas.AsNoTracking()
                         .Where(c => c.SistemaId == sistema
                                     && (c.Entidade == EntidadeTipoProcesso || c.Entidade == EntidadeTipoTarefa || c.Entidade == EntidadeResultado))
                         .Select(c => new { c.Entidade, c.ChaveOrigem, c.RegistroId })
                         .ToListAsync(ct))
            {
                if (!int.TryParse(chave.ChaveOrigem, NumberStyles.Integer, CultureInfo.InvariantCulture, out var codigo)) continue;
                var destino = chave.Entidade switch
                {
                    EntidadeTipoProcesso => mapas.TipoPorCodigo,
                    EntidadeTipoTarefa => mapas.TipoTarefaPorAcao,
                    _ => mapas.ResultadoPorCodigo
                };
                destino[codigo] = (int)chave.RegistroId;
            }
        }

        // A CHAVE ÓRFÃ NÃO VALE: a sanitização de 15/09 apagou os catálogos e pode ter deixado de-para apontando para o nada.
        var tipos = await banco.TiposDeProcesso.AsNoTracking().Select(t => t.Id).ToListAsync(ct);
        var tiposDeTarefa = await banco.TiposDeTarefa.AsNoTracking().Select(t => new { t.Id, t.Codigo }).ToListAsync(ct);
        var resultados = await banco.Resultados.AsNoTracking().Select(r => r.Id).ToListAsync(ct);
        Podar(mapas.TipoPorCodigo, tipos.ToHashSet());
        Podar(mapas.TipoTarefaPorAcao, tiposDeTarefa.Select(t => t.Id).ToHashSet());
        Podar(mapas.ResultadoPorCodigo, resultados.ToHashSet());

        foreach (var fase in await banco.Fases.AsNoTracking().Select(f => new { f.Id, f.TipoProcessoId, f.Codigo }).ToListAsync(ct))
            mapas.FasePorChave[(fase.TipoProcessoId, fase.Codigo)] = fase.Id;

        mapas.TipoTarefaNaoInformadaId = tiposDeTarefa.FirstOrDefault(t => t.Codigo == TipoDeTarefaNaoInformada)?.Id ?? 0;
        mapas.MotivoNaoInformadoId = await banco.MotivosDePerda.AsNoTracking()
            .Where(m => m.Codigo == CargaDoFunilDoVortice.MotivoNaoInformado)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(ct);

        return mapas;
    }

    private static void Podar(Dictionary<int, int> mapa, HashSet<int> existentes)
    {
        foreach (var codigo in mapa.Where(p => !existentes.Contains(p.Value)).Select(p => p.Key).ToList()) mapa.Remove(codigo);
    }

    /// <summary>
    /// OS CATÁLOGOS MÍNIMOS (P4): tipo de processo, fase, tipo de tarefa, desfecho e o motivo "não informado" — só o que a
    /// origem usa. Os códigos são os da carga antiga, para a mesma ação ser o mesmo tipo de tarefa. Um código que já existe
    /// sem de-para ganha o de-para, em vez de um segundo item. Os catálogos só crescem: nome que muda na origem não muda
    /// aqui.
    /// </summary>
    private async Task<Mapas> CompletarCatalogosAsync(
        Func<CrmDbContext> abrir, int sistemaId, LeituraDasOportunidadesDoVortice origem, Plano plano, CancellationToken ct)
    {
        await using (var banco = abrir())
        {
            await using var transacao = await banco.Database.BeginTransactionAsync(ct);
            var mapas = await MapasAsync(banco, sistemaId, origem, ct);

            // Os tipos de processo.
            var tiposPorCodigo = await banco.TiposDeProcesso.ToDictionaryAsync(t => t.Codigo, StringComparer.OrdinalIgnoreCase, ct);
            var tiposNovos = new List<(int Codigo, TipoProcesso Tipo)>();
            foreach (var tipo in origem.TiposDeProcesso.Where(t => !mapas.TipoPorCodigo.ContainsKey(t.Codigo)))
            {
                var codigo = CodigoComChave(tipo.Descricao ?? tipo.DescricaoReduzida, "FLUXO", tipo.Codigo);
                if (!tiposPorCodigo.TryGetValue(codigo, out var existente))
                {
                    existente = TipoProcesso.Criar(codigo, Limitar(Nome(tipo.Descricao, tipo.DescricaoReduzida, $"Fluxo {tipo.Codigo}"), 120)!, estaAtivo: tipo.EmUso);
                    banco.TiposDeProcesso.Add(existente);
                    tiposPorCodigo[codigo] = existente;
                }

                tiposNovos.Add((tipo.Codigo, existente));
            }

            // Os tipos de tarefa: cada ação usada, e o "não informada".
            var tiposDeTarefaPorCodigo = await banco.TiposDeTarefa.ToDictionaryAsync(t => t.Codigo, StringComparer.OrdinalIgnoreCase, ct);
            var acoesNovas = new List<(int Acao, TipoTarefa Tipo)>();
            foreach (var acao in origem.Acoes.Where(a => !mapas.TipoTarefaPorAcao.ContainsKey(a.Codigo)))
            {
                var codigo = CodigoComChave(acao.Descricao ?? acao.DescricaoReduzida, "ACAO", acao.Codigo);
                if (!tiposDeTarefaPorCodigo.TryGetValue(codigo, out var existente))
                {
                    // A CATEGORIA NÃO VEM DA ORIGEM (a mesma decisão da carga antiga): Interna, que não afirma contato.
                    existente = TipoTarefa.Criar(
                        codigo, Limitar(Nome(acao.Descricao, acao.DescricaoReduzida, $"Ação {acao.Codigo}"), 120)!, CategoriaDeInteracao.Interna,
                        acao.PrazoEmDias is > 0 and < short.MaxValue ? (short)acao.PrazoEmDias.Value : (short)0,
                        contaParaCobertura: false, estaAtivo: acao.EmUso);
                    banco.TiposDeTarefa.Add(existente);
                    tiposDeTarefaPorCodigo[codigo] = existente;
                }

                acoesNovas.Add((acao.Codigo, existente));
            }

            if (!tiposDeTarefaPorCodigo.ContainsKey(TipoDeTarefaNaoInformada))
                banco.TiposDeTarefa.Add(TipoTarefa.Criar(TipoDeTarefaNaoInformada, "Não informada na origem", CategoriaDeInteracao.Interna, prazoDiasUteis: 0));

            if (!await banco.MotivosDePerda.AnyAsync(m => m.Codigo == CargaDoFunilDoVortice.MotivoNaoInformado, ct))
                banco.MotivosDePerda.Add(MotivoDePerda.Criar(CargaDoFunilDoVortice.MotivoNaoInformado, "Não informado na origem", CategoriaDeMotivoDePerda.Outro));

            await banco.SaveChangesAsync(ct);

            foreach (var (codigo, tipo) in tiposNovos)
            {
                banco.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, EntidadeTipoProcesso, tipo.Id, Texto(codigo)));
                mapas.TipoPorCodigo[codigo] = tipo.Id;
            }

            foreach (var (acao, tipo) in acoesNovas)
            {
                banco.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, EntidadeTipoTarefa, tipo.Id, Texto(acao)));
                mapas.TipoTarefaPorAcao[acao] = tipo.Id;
            }

            // As fases — as que os processos do universo ocupam, com a ordem mínima e a heurística de final da carga antiga.
            foreach (var fase in plano.Processos
                         .GroupBy(p => (Tipo: mapas.TipoPorCodigo.GetValueOrDefault(p.Origem.Tipo), p.CodigoDaFase))
                         .Where(g => g.Key.Tipo != 0 && !mapas.FasePorChave.ContainsKey((g.Key.Tipo, g.Key.CodigoDaFase))))
            {
                var nova = Fase.Criar(
                    fase.Key.Tipo, fase.Key.CodigoDaFase, fase.First().NomeDaFase, fase.Min(p => p.OrdemDaFase),
                    fase.Key.CodigoDaFase.StartsWith("FINALIZ", StringComparison.Ordinal)
                    || fase.Key.CodigoDaFase.StartsWith("CANCELA", StringComparison.Ordinal)
                    || fase.Key.CodigoDaFase.StartsWith("CONCLU", StringComparison.Ordinal));
                banco.Fases.Add(nova);
            }

            // Os desfechos: cada um pertence a um tipo de tarefa, pela ação dona.
            var efeitos = origem.Efeitos.GroupBy(e => e.Resultado).ToDictionary(g => g.Key, g => g.First());
            var resultadosPorChave = await banco.Resultados.ToDictionaryAsync(r => (r.TipoTarefaId, r.Codigo), ct);
            var resultadosNovos = new List<(int Codigo, ResultadoDoCrm Resultado)>();
            foreach (var resultado in origem.Resultados.Where(r => !mapas.ResultadoPorCodigo.ContainsKey(r.Codigo)))
            {
                if (resultado.Acao is not { } acao || !mapas.TipoTarefaPorAcao.TryGetValue(acao, out var tipoId)) continue;

                var codigo = $"RES_{Texto(resultado.Codigo)}";
                if (!resultadosPorChave.TryGetValue((tipoId, codigo), out var existente))
                {
                    var efeito = efeitos.GetValueOrDefault(resultado.Codigo);
                    var (classe, _) = SaneamentoDeProcesso.ClasseDoResultado(efeito?.StatusDeDestino, efeito?.MoveFase ?? false);
                    existente = ResultadoDoCrm.Criar(tipoId, codigo, Limitar(Nome(resultado.Descricao, resultado.DescricaoReduzida, $"Desfecho {resultado.Codigo}"), 120)!, classe);
                    banco.Resultados.Add(existente);
                    resultadosPorChave[(tipoId, codigo)] = existente;
                }

                resultadosNovos.Add((resultado.Codigo, existente));
            }

            await banco.SaveChangesAsync(ct);

            foreach (var (codigo, resultado) in resultadosNovos)
                banco.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, EntidadeResultado, resultado.Id, Texto(codigo)));

            await banco.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        await using var releitura = abrir();
        return await MapasAsync(releitura, sistemaId, origem, ct);
    }

    private static string CodigoComChave(string? nome, string semNome, int chave)
    {
        var codigo = SaneamentoDeProcesso.Codificar(nome, 34);
        return $"{(codigo.Length == 0 ? semNome : codigo)}_{Texto(chave)}";
    }

    private static string Nome(string? descricao, string? reduzida, string padrao) =>
        !string.IsNullOrWhiteSpace(descricao) ? descricao.Trim() : !string.IsNullOrWhiteSpace(reduzida) ? reduzida.Trim() : padrao;

    // =============================================================================================
    // A gravação — em blocos
    // =============================================================================================

    private async Task<Resultado<bool>> AplicarAsync(LeituraDasOportunidadesDoVortice origem, Plano plano, DateTime agora, CancellationToken ct)
    {
        int sistemaId;
        await using (var banco = abrirContexto())
        {
            sistemaId = await CargaDeTerritorio.SistemaAsync(
                banco, LeitorDeCargaDoVortice.CodigoDoSistema, "Vórtice CRM (sistema legado)", "SQL Server, somente leitura", ct);
        }

        CrmDbContext Abrir()
        {
            var banco = abrirContexto();
            banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);
            return banco;
        }

        long execucaoId;
        await using (var banco = Abrir())
        {
            var execucao = ExecucaoDeSincronizacao.Iniciar(sistemaId, Fluxo, Environment.MachineName, relogio());
            banco.ExecucoesDeSincronizacao.Add(execucao);
            await banco.SaveChangesAsync(ct);
            execucaoId = execucao.Id;
        }

        var etapa = "os catálogos";
        try
        {
            var mapas = await CompletarCatalogosAsync(Abrir, sistemaId, origem, plano, ct);

            etapa = "os processos";
            var processoPorNumero = await GravarProcessosAsync(Abrir, sistemaId, plano, mapas, agora, ct);

            etapa = "as tarefas";
            var tarefaPorAgenda = await GravarTarefasAsync(Abrir, sistemaId, plano, mapas, processoPorNumero, agora, ct);

            etapa = "as interações";
            var interacaoPorHistorico = await GravarInteracoesAsync(Abrir, sistemaId, plano, mapas, processoPorNumero, tarefaPorAgenda, ct);

            etapa = "o duplo ponteiro das tarefas";
            await LigarPonteirosAsync(Abrir, plano, tarefaPorAgenda, interacaoPorHistorico, ct);

            etapa = "a linha do funil";
            await LigarFunilAsync(Abrir, processoPorNumero, ct);
        }
        catch (Exception falha) when (falha is DbUpdateException or InvalidOperationException or RegraDeNegocioViolada)
        {
            var mensagem = $"Gravação parcial: a rodada parou em {etapa}; os blocos anteriores ficaram, o bloco em curso foi desfeito. " +
                           $"A próxima rodada completa — ela relê tudo e grava só o que falta. Causa: {falha.GetBaseException().Message}";
            await EncerrarAsync(Abrir, execucaoId, e => e.Falhar(1, mensagem, relogio()), ct);
            return Resultado<bool>.Indisponivel(mensagem);
        }

        var resumo =
            $"Processos: {plano.ProcessosAIncluir:N0} incluídos, {plano.ProcessosAAlterar:N0} alterados, {plano.ProcessosAExcluir.Count:N0} excluídos. " +
            $"Tarefas: {plano.TarefasAIncluir:N0} incluídas, {plano.TarefasAAlterar:N0} alteradas, {plano.TarefasAExcluir.Count:N0} excluídas. " +
            $"Interações: {plano.Interacoes.Count:N0} incluídas." +
            (plano.QuedaAceita is null ? string.Empty : " " + plano.QuedaAceita);

        await EncerrarAsync(Abrir, execucaoId, e => e.Concluir(
            1,
            origem.Processos.Count + origem.Tarefas.Count + origem.Interacoes.Count,
            plano.ProcessosAIncluir + plano.TarefasAIncluir + plano.Interacoes.Count,
            plano.ProcessosAAlterar + plano.ProcessosAExcluir.Count + plano.TarefasAAlterar + plano.TarefasAExcluir.Count,
            0,
            resumo,
            relogio()), ct);

        relatar("Gravado. A rotina pode rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
        return Resultado<bool>.Ok(true);
    }

    private static async Task EncerrarAsync(
        Func<CrmDbContext> abrir, long execucaoId, Action<ExecucaoDeSincronizacao> encerrar, CancellationToken ct)
    {
        await using var banco = abrir();
        var execucao = await banco.ExecucoesDeSincronizacao.FirstAsync(e => e.Id == execucaoId, ct);
        encerrar(execucao);
        await banco.SaveChangesAsync(ct);
    }

    private static async Task<Dictionary<string, ChaveExterna>> ChavesRastreadasAsync(
        CrmDbContext banco, int sistemaId, string entidade, IReadOnlyCollection<string> chaves, CancellationToken ct)
    {
        var porChave = new Dictionary<string, ChaveExterna>(StringComparer.Ordinal);
        foreach (var fatia in chaves.Chunk(CargaDoFunilDoVortice.IdsPorConsulta))
        {
            var lista = fatia.ToList();
            foreach (var chave in await banco.ChavesExternas
                         .Where(c => c.SistemaId == sistemaId && c.Entidade == entidade && lista.Contains(c.ChaveOrigem))
                         .ToListAsync(ct))
                porChave[chave.ChaveOrigem] = chave;
        }

        return porChave;
    }

    /// <summary>Os processos, em blocos; devolve o processo do CRM de cada número do universo.</summary>
    private async Task<Dictionary<long, long>> GravarProcessosAsync(
        Func<CrmDbContext> abrir, int sistemaId, Plano plano, Mapas mapas, DateTime agora, CancellationToken ct)
    {
        var processoPorNumero = new Dictionary<long, long>();

        foreach (var bloco in plano.Processos.Chunk(tamanhoDoBloco))
        {
            await using var banco = abrir();
            await using var transacao = await banco.Database.BeginTransactionAsync(ct);

            var chaves = await ChavesRastreadasAsync(banco, sistemaId, EntidadeProcesso, [.. bloco.Select(p => Texto(p.Origem.Numero))], ct);
            var ids = chaves.Values.Select(c => c.RegistroId).ToList();
            var existentes = ids.Count == 0
                ? new Dictionary<long, ProcessoDoCrm>()
                : await banco.Set<ProcessoDoCrm>().Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

            var novos = new List<(string Chave, ProcessoDoCrm Processo)>();
            foreach (var planejado in bloco)
            {
                var chave = Texto(planejado.Origem.Numero);
                var retrato = planejado.Retrato(mapas);

                if (chaves.TryGetValue(chave, out var deParaExistente) && existentes.TryGetValue(deParaExistente.RegistroId, out var existente))
                {
                    existente.RestaurarDaOrigem(usuarioId);
                    existente.AtualizarDaOrigem(retrato, usuarioId);
                    processoPorNumero[planejado.Origem.Numero] = existente.Id;
                    continue;
                }

                var novo = ProcessoDoCrm.DaOrigem(retrato, usuarioId);
                banco.Set<ProcessoDoCrm>().Add(novo);
                novos.Add((chave, novo));
            }

            await banco.SaveChangesAsync(ct);

            foreach (var (chave, processo) in novos)
            {
                if (chaves.TryGetValue(chave, out var orfa)) orfa.ReapontarPara(processo.Id, agora);
                else banco.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, EntidadeProcesso, processo.Id, chave));
                processoPorNumero[long.Parse(chave, CultureInfo.InvariantCulture)] = processo.Id;
            }

            await banco.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        // O QUE SAIU DO UNIVERSO é excluído — lógico, nunca apagado. As tarefas dele saem na etapa das tarefas.
        foreach (var fatia in plano.ProcessosAExcluir.Chunk(tamanhoDoBloco))
        {
            await using var banco = abrir();
            var ids = fatia.ToList();
            foreach (var processo in await banco.Set<ProcessoDoCrm>().Where(p => ids.Contains(p.Id)).ToListAsync(ct)) processo.Excluir(usuarioId);
            await banco.SaveChangesAsync(ct);
        }

        relatar($"  processos: {processoPorNumero.Count:N0} no universo, {plano.ProcessosAExcluir.Count:N0} excluídos");
        return processoPorNumero;
    }

    /// <summary>As tarefas, em blocos; devolve a tarefa do CRM de cada agenda do universo.</summary>
    private async Task<Dictionary<long, long>> GravarTarefasAsync(
        Func<CrmDbContext> abrir, int sistemaId, Plano plano, Mapas mapas, IReadOnlyDictionary<long, long> processoPorNumero, DateTime agora,
        CancellationToken ct)
    {
        var tarefaPorAgenda = new Dictionary<long, long>();

        foreach (var bloco in plano.Tarefas.Chunk(tamanhoDoBloco))
        {
            await using var banco = abrir();
            await using var transacao = await banco.Database.BeginTransactionAsync(ct);

            var chaves = await ChavesRastreadasAsync(banco, sistemaId, EntidadeTarefa, [.. bloco.Select(t => Texto(t.Origem.SeqAgenda))], ct);
            var ids = chaves.Values.Select(c => c.RegistroId).ToList();
            var existentes = ids.Count == 0
                ? new Dictionary<long, Tarefa>()
                : await banco.Tarefas.Where(t => ids.Contains(t.Id)).ToDictionaryAsync(t => t.Id, ct);

            var novas = new List<(string Chave, Tarefa Tarefa)>();
            foreach (var planejada in bloco)
            {
                var chave = Texto(planejada.Origem.SeqAgenda);
                var retrato = planejada.Retrato(mapas, processoPorNumero[planejada.Processo.Origem.Numero]);

                if (chaves.TryGetValue(chave, out var deParaExistente) && existentes.TryGetValue(deParaExistente.RegistroId, out var existente))
                {
                    existente.RestaurarDaOrigem(usuarioId);
                    existente.AtualizarDaOrigem(retrato, usuarioId);
                    tarefaPorAgenda[planejada.Origem.SeqAgenda] = existente.Id;
                    continue;
                }

                var nova = Tarefa.DaOrigem(retrato, usuarioId);
                banco.Tarefas.Add(nova);
                novas.Add((chave, nova));
            }

            await banco.SaveChangesAsync(ct);

            foreach (var (chave, tarefa) in novas)
            {
                if (chaves.TryGetValue(chave, out var orfa)) orfa.ReapontarPara(tarefa.Id, agora);
                else banco.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, EntidadeTarefa, tarefa.Id, chave));
                tarefaPorAgenda[long.Parse(chave, CultureInfo.InvariantCulture)] = tarefa.Id;
            }

            await banco.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        foreach (var fatia in plano.TarefasAExcluir.Chunk(tamanhoDoBloco))
        {
            await using var banco = abrir();
            var ids = fatia.ToList();
            foreach (var tarefa in await banco.Tarefas.Where(t => ids.Contains(t.Id)).ToListAsync(ct)) tarefa.Excluir(usuarioId);
            await banco.SaveChangesAsync(ct);
        }

        relatar($"  tarefas: {tarefaPorAgenda.Count:N0} no universo, {plano.TarefasAExcluir.Count:N0} excluídas");
        return tarefaPorAgenda;
    }

    /// <summary>As interações novas, em blocos; devolve a interação do CRM de cada histórico — as que já estavam também.</summary>
    private async Task<Dictionary<long, long>> GravarInteracoesAsync(
        Func<CrmDbContext> abrir, int sistemaId, Plano plano, Mapas mapas, IReadOnlyDictionary<long, long> processoPorNumero,
        IReadOnlyDictionary<long, long> tarefaPorAgenda, CancellationToken ct)
    {
        long? TarefaDa(long? agenda) => agenda is { } a && tarefaPorAgenda.TryGetValue(a, out var id) ? id : null;

        foreach (var bloco in plano.Interacoes.Chunk(tamanhoDoBloco))
        {
            await using var banco = abrir();
            await using var transacao = await banco.Database.BeginTransactionAsync(ct);

            var novas = new List<(string Chave, Interacao Interacao)>();
            foreach (var planejada in bloco)
            {
                var i = planejada.Origem;
                var (latitude, longitude) = Coordenada(i.Latitude, i.Longitude);
                var interacao = Interacao.Registrar(
                    planejada.Processo.EmpresaId,
                    mapas.TipoDaAcao(i.AcaoGeradora),
                    mapas.NomeDaAcao(i.AcaoGeradora),
                    DataHoraUtc.Criar(EstagioDoProcesso.NoMilissegundo(i.RealizadaEmUtc!.Value)),
                    i.DeSistema ? NaturezaDaInteracao.Sistema : SaneamentoDeProcesso.Natureza(i.Natureza, null, new HashSet<string>(), []),
                    planejada.AutorId,
                    planejada.Processo.ClienteId,
                    processoPorNumero[planejada.Processo.Origem.Numero],
                    contatoId: null,
                    TarefaDa(i.SeqAgendaDeOrigem),
                    i.Resultado is { } r ? mapas.ResultadoDoCodigo(r) : null,
                    resultadoComplemento: null,
                    detalhe: null,
                    i.Duracao is > 0 and < int.MaxValue ? (int)i.Duracao.Value : null,
                    latitude,
                    longitude);

                banco.Interacoes.Add(interacao);
                novas.Add((Texto(i.SeqHistorico), interacao));
            }

            await banco.SaveChangesAsync(ct);
            foreach (var (chave, interacao) in novas)
                banco.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, EntidadeInteracao, interacao.Id, chave));
            await banco.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        // O MAPA INTEIRO, das novas e das que já estavam: o duplo ponteiro de uma tarefa pode apontar para qualquer uma.
        await using var leitura = abrir();
        var interacaoPorHistorico = new Dictionary<long, long>();
        foreach (var chave in await leitura.ChavesExternas.AsNoTracking()
                     .Where(c => c.SistemaId == sistemaId && c.Entidade == EntidadeInteracao)
                     .Select(c => new { c.ChaveOrigem, c.RegistroId })
                     .ToListAsync(ct))
            if (long.TryParse(chave.ChaveOrigem, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seq))
                interacaoPorHistorico[seq] = chave.RegistroId;

        relatar($"  interações: {plano.Interacoes.Count:N0} incluídas");
        return interacaoPorHistorico;
    }

    /// <summary>A coordenada só entra inteira e dentro do globo; zero-zero é "não registrado", e não o Golfo da Guiné.</summary>
    private static (decimal? Latitude, decimal? Longitude) Coordenada(decimal? latitude, decimal? longitude) =>
        latitude is { } la && longitude is { } lo && (la != 0 || lo != 0) && la is >= -90 and <= 90 && lo is >= -180 and <= 180
            ? (la, lo)
            : (null, null);

    /// <summary>
    /// O DUPLO PONTEIRO: a tarefa aponta a interação que a gerou e a que a concluiu, quando as duas estão no CRM. É a lógica
    /// da carga antiga que liga pelo que a origem declara — e nunca a que escreve o último contato do vínculo.
    /// </summary>
    private async Task LigarPonteirosAsync(
        Func<CrmDbContext> abrir, Plano plano, IReadOnlyDictionary<long, long> tarefaPorAgenda,
        IReadOnlyDictionary<long, long> interacaoPorHistorico, CancellationToken ct)
    {
        long? Interacao(long? seq) => seq is { } s && interacaoPorHistorico.TryGetValue(s, out var id) ? id : null;

        foreach (var bloco in plano.Tarefas.Where(t => tarefaPorAgenda.ContainsKey(t.Origem.SeqAgenda)).Chunk(tamanhoDoBloco))
        {
            await using var banco = abrir();
            var ids = bloco.Select(t => tarefaPorAgenda[t.Origem.SeqAgenda]).ToList();
            var tarefas = await banco.Tarefas.Where(t => ids.Contains(t.Id)).ToDictionaryAsync(t => t.Id, ct);

            foreach (var planejada in bloco)
                tarefas[tarefaPorAgenda[planejada.Origem.SeqAgenda]].LigarInteracoes(
                    Interacao(planejada.Origem.SeqHistoricoDeOrigem), Interacao(planejada.Origem.SeqHistoricoDeConclusao), usuarioId);

            await banco.SaveChangesAsync(ct);
        }
    }

    /// <summary>A linha do funil ganha o processo do CRM de mesmo número; a do processo que saiu do universo perde.</summary>
    private async Task LigarFunilAsync(Func<CrmDbContext> abrir, IReadOnlyDictionary<long, long> processoPorNumero, CancellationToken ct)
    {
        List<long> aLigar;
        await using (var leitura = abrir())
        {
            aLigar = (await leitura.EstagiosDoProcesso.AsNoTracking()
                    .Select(e => new { e.Id, e.NumeroDoProcessoNaOrigem, e.ProcessoId })
                    .ToListAsync(ct))
                .Where(e => (processoPorNumero.TryGetValue(e.NumeroDoProcessoNaOrigem, out var id) ? id : (long?)null) != e.ProcessoId)
                .Select(e => e.Id)
                .ToList();
        }

        foreach (var fatia in aLigar.Chunk(tamanhoDoBloco))
        {
            await using var banco = abrir();
            var ids = fatia.ToList();
            foreach (var linha in await banco.EstagiosDoProcesso.Where(e => ids.Contains(e.Id)).ToListAsync(ct))
                linha.LigarAoProcesso(processoPorNumero.TryGetValue(linha.NumeroDoProcessoNaOrigem, out var id) ? id : null);
            await banco.SaveChangesAsync(ct);
        }

        relatar($"  funil: {aLigar.Count:N0} linhas com o processo ligado ou desligado");
    }

    // =============================================================================================
    // O relatório
    // =============================================================================================

    private void Relatar(LeituraDasOportunidadesDoVortice origem, Plano plano)
    {
        const string leitura = "1. Leitura do Vórtice";
        Contar(leitura, "processos 31/41/50 lidos", origem.Processos.Count);
        Contar(leitura, "linhas de agenda lidas", origem.Tarefas.Count);
        Contar(leitura, "linhas de histórico lidas", origem.Interacoes.Count);

        const string universo = "2. O universo (P6, P7: só processo, pai DNA incluído)";
        Contar(universo, RotuloDeProcessosNoUniverso, plano.Processos.Count);
        Contar(universo, "fora da janela (abertura antes de 01/11/2023 ou sem data crível)", plano.ForaDaJanela);
        Contar(universo, "de filial fora do CRM", plano.FilialForaDoCrm);
        foreach (var (motivo, quantos) in plano.NaoCasados.OrderByDescending(m => m.Value))
            Contar(universo, $"  sem cliente casado: {motivo} (o prospect fica só no funil)", quantos);

        const string ligacoes = "3. Donos e decisões";
        Contar(ligacoes, "dono pelo login do responsável", plano.DonoPeloLogin);
        Contar(ligacoes, "dono pelo dono da carteira", plano.DonoPelaCarteira);
        Contar(ligacoes, "dono = quem roda a rotina (sem login nem carteira)", plano.DonoPeloOperador);
        Contar(ligacoes, "títulos compostos (sem Resumo na origem, P1)", plano.TitulosCompostos);
        Contar(ligacoes, "perdidos com o motivo da venda perdida (P8)", plano.PerdidosComMotivoDoFormulario);
        Contar(ligacoes, "perdidos sem formulário — não informado na origem (P8)", plano.PerdidosSemFormulario);
        Contar(ligacoes, "tarefas do dono do processo (responsável sem conta, P2)", plano.TarefasComDono);
        Contar(ligacoes, "conclusões atribuídas ao dono do processo (P2)", plano.ConclusoesComDono);
        Contar(ligacoes, "interações do dono do processo (autor sem conta, P2)", plano.InteracoesComDono);
        Contar(ligacoes, "tarefas pendentes de processo encerrado — fora (P10)", plano.PendentesDeProcessoEncerrado);

        const string processos = "4. Processos (processo.Processo)";
        Contar(processos, RotuloDeProcessosIncluidos, plano.ProcessosAIncluir);
        Contar(processos, RotuloDeProcessosAlterados, plano.ProcessosAAlterar);
        Contar(processos, "processos restaurados (voltaram ao universo)", plano.ProcessosARestaurar);
        Contar(processos, RotuloDeProcessosExcluidos, plano.ProcessosAExcluir.Count);

        const string tarefas = "5. Tarefas (processo.Tarefa)";
        Contar(tarefas, RotuloDeTarefasIncluidas, plano.TarefasAIncluir);
        Contar(tarefas, RotuloDeTarefasAlteradas, plano.TarefasAAlterar);
        Contar(tarefas, "tarefas restauradas", plano.TarefasARestaurar);
        Contar(tarefas, RotuloDeTarefasExcluidas, plano.TarefasAExcluir.Count);
        Contar(tarefas, "concluídas", plano.Tarefas.Count(t => t.Situacao is SituacaoDaTarefa.Concluida));
        Contar(tarefas, "pendentes", plano.Tarefas.Count(t => t.Situacao is SituacaoDaTarefa.Pendente));
        Contar(tarefas, "sem data de agenda — fora", plano.TarefasSemData);

        const string interacoes = "6. Interações (processo.Interacao — só inclusão)";
        Contar(interacoes, RotuloDeInteracoesIncluidas, plano.Interacoes.Count);
        Contar(interacoes, "já no CRM (não mudam)", plano.InteracoesJaNoCrm);
        Contar(interacoes, "escritas pelo servidor da origem", plano.InteracoesDeSistema);
        Contar(interacoes, "sem data crível — fora", plano.InteracoesSemData);

        Contar("7. Catálogos mínimos (P4)", "itens a incluir (tipos, fases, ações, desfechos, motivo)", plano.CatalogosNovos);

        if (plano.SistemaId is null)
            _observacoes.Add("O CRM ainda não conhece o sistema VORTICE: sem o de-para da sincronia das carteiras, nenhum processo " +
                             "liga à carteira nem acha a conta pelo SeqUsuario. Rode a CARTEIRAS_VORTICE antes.");
    }

    private void Contar(string etapa, string rotulo, int valor)
    {
        _contagens.Add((etapa, rotulo, valor));
        relatar($"  {valor,7:N0}  {rotulo}");
    }

    // =============================================================================================
    // As estruturas do plano
    // =============================================================================================

    /// <summary>O de-para de processo, tarefa e interação que o CRM já tem.</summary>
    private sealed class Chaves
    {
        public Dictionary<string, long> Processos { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, long> Tarefas { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, long> Interacoes { get; } = new(StringComparer.Ordinal);
    }

    /// <summary>
    /// OS CATÁLOGOS DO CRM pelo código da origem. No plano, o que ainda não existe vale zero — só o retrato de um registro
    /// que vai mudar de qualquer jeito fica diferente; na gravação, depois de completar os catálogos, tudo tem id.
    /// </summary>
    private sealed class Mapas(LeituraDasOportunidadesDoVortice origem)
    {
        private readonly Dictionary<int, string> _nomeDoTipo = origem.TiposDeProcesso.GroupBy(t => t.Codigo)
            .ToDictionary(g => g.Key, g => Nome(g.First().Descricao, g.First().DescricaoReduzida, $"Fluxo {g.Key}"));

        private readonly Dictionary<int, string> _nomeDaAcao = origem.Acoes.GroupBy(a => a.Codigo)
            .ToDictionary(g => g.Key, g => Nome(g.First().Descricao, g.First().DescricaoReduzida, $"Ação {g.Key}"));

        private readonly HashSet<int> _resultadosComDono = origem.Resultados
            .Where(r => r.Acao is { } a && origem.Acoes.Any(x => x.Codigo == a))
            .Select(r => r.Codigo)
            .ToHashSet();

        public Dictionary<int, int> TipoPorCodigo { get; } = [];
        public Dictionary<(int TipoId, string Codigo), int> FasePorChave { get; } = [];
        public Dictionary<int, int> TipoTarefaPorAcao { get; } = [];
        public Dictionary<int, int> ResultadoPorCodigo { get; } = [];
        public int TipoTarefaNaoInformadaId { get; set; }
        public int MotivoNaoInformadoId { get; set; }

        /// <summary>O nome do tipo de processo, para o título composto (P1).</summary>
        public string NomeDoTipo(int codigo) => _nomeDoTipo.GetValueOrDefault(codigo) ?? $"Fluxo {codigo}";

        /// <summary>O nome da ação — o assunto da tarefa e da interação: o texto livre da origem não entra (P1).</summary>
        public string NomeDaAcao(int? acao) =>
            Limitar(acao is { } a && _nomeDaAcao.TryGetValue(a, out var nome) ? nome : "Não informada na origem", 200)!;

        /// <summary>O tipo de tarefa da ação; sem ação conhecida, o "não informada".</summary>
        public int TipoDaAcao(int? acao) =>
            acao is { } a && TipoTarefaPorAcao.TryGetValue(a, out var id) ? id : TipoTarefaNaoInformadaId;

        /// <summary>O desfecho do CRM; nulo quando a origem não o tem em catálogo com ação dona.</summary>
        public int? ResultadoDoCodigo(int codigo) => ResultadoPorCodigo.TryGetValue(codigo, out var id) ? id : null;

        /// <summary>Se o desfecho existe ou vai existir — a conclusão da tarefa depende dele (CK_Tarefa_Conclusao).</summary>
        public bool ResultadoConhecido(int codigo) => ResultadoPorCodigo.ContainsKey(codigo) || _resultadosComDono.Contains(codigo);

        /// <summary>Quantos itens de catálogo a gravação inclui — a conta da simulação.</summary>
        public int QuantosFaltam(Plano plano) =>
            origem.TiposDeProcesso.Count(t => !TipoPorCodigo.ContainsKey(t.Codigo))
            + origem.Acoes.Count(a => !TipoTarefaPorAcao.ContainsKey(a.Codigo))
            + origem.Resultados.Count(r => !ResultadoPorCodigo.ContainsKey(r.Codigo) && _resultadosComDono.Contains(r.Codigo))
            + plano.Processos.Select(p => (p.Origem.Tipo, p.CodigoDaFase)).Distinct()
                .Count(f => !TipoPorCodigo.TryGetValue(f.Tipo, out var tipo) || !FasePorChave.ContainsKey((tipo, f.CodigoDaFase)))
            + (TipoTarefaNaoInformadaId == 0 ? 1 : 0)
            + (MotivoNaoInformadoId == 0 ? 1 : 0);
    }

    /// <summary>Um processo do universo, com tudo o que o retrato precisa além dos catálogos.</summary>
    private sealed record ProcessoPlanejado(
        ProcessoDaOportunidadeNoVortice Origem, int EmpresaId, long ClienteId, long? CarteiraId, long Dono, DateTime AbertoEm,
        DateTime FaseDesde, SituacaoDoProcesso Situacao, DateTime SituacaoDesde, DateTime? ConcluidoEm, string CodigoDaFase,
        string NomeDaFase, short OrdemDaFase, int? MotivoDaVendaPerdida, int? ConcorrenteDaVendaPerdida, string Titulo,
        Dinheiro? Valor, decimal? Quantidade)
    {
        public RetratoDoProcessoDaOrigem Retrato(Mapas mapas)
        {
            var tipoId = mapas.TipoPorCodigo.GetValueOrDefault(Origem.Tipo);
            var perdido = Situacao is SituacaoDoProcesso.Perdido;
            return new RetratoDoProcessoDaOrigem(
                Origem.Numero, EmpresaId, tipoId, ClienteId, CarteiraId, Titulo,
                mapas.FasePorChave.GetValueOrDefault((tipoId, CodigoDaFase)), FaseDesde, Situacao, SituacaoDesde, ConcluidoEm,
                perdido ? MotivoDaVendaPerdida ?? mapas.MotivoNaoInformadoId : null,
                perdido ? ConcorrenteDaVendaPerdida : null,
                Valor, Quantidade, Origem.PrevisaoConclusao, Origem.PrevisaoConclusaoOriginal, Dono, AbertoEm);
        }
    }

    /// <summary>Uma tarefa do universo.</summary>
    private sealed record TarefaPlanejada(
        TarefaDaOportunidadeNoVortice Origem, ProcessoPlanejado Processo, long ResponsavelId, SituacaoDaTarefa Situacao,
        long? ConcluidaPorId, short Prioridade, DateTime? PrazoLimite)
    {
        public RetratoDaTarefaDaOrigem Retrato(Mapas mapas, long processoId)
        {
            var concluida = Situacao is SituacaoDaTarefa.Concluida;
            return new RetratoDaTarefaDaOrigem(
                Processo.EmpresaId, processoId, Processo.ClienteId, mapas.TipoDaAcao(Origem.Acao), mapas.NomeDaAcao(Origem.Acao),
                ResponsavelId,
                (Origem.TipoAgendamento ?? string.Empty).Trim().ToUpperInvariant() is "A" ? OrigemDaAtribuicao.Regra : OrigemDaAtribuicao.Manual,
                Origem.AgendadaParaUtc!.Value, PrazoLimite, Prioridade, Situacao,
                concluida ? Origem.RealizadaEmUtc : null,
                concluida ? ConcluidaPorId : null,
                concluida && Origem.Resultado is { } r ? mapas.ResultadoDoCodigo(r) : null,
                Origem.GeradaEmUtc ?? Origem.AgendadaParaUtc!.Value);
        }
    }

    /// <summary>Uma interação nova do universo.</summary>
    private sealed record InteracaoPlanejada(InteracaoDaOportunidadeNoVortice Origem, ProcessoPlanejado Processo, long AutorId);

    private sealed class Plano
    {
        public int? SistemaId { get; init; }
        public List<ProcessoPlanejado> Processos { get; } = [];
        public List<TarefaPlanejada> Tarefas { get; } = [];
        public List<InteracaoPlanejada> Interacoes { get; } = [];
        public List<long> ProcessosAExcluir { get; } = [];
        public List<long> TarefasAExcluir { get; } = [];
        public Dictionary<string, int> NaoCasados { get; } = new(StringComparer.Ordinal);

        public int ForaDaJanela { get; set; }
        public int FilialForaDoCrm { get; set; }
        public int DonoPeloLogin { get; set; }
        public int DonoPelaCarteira { get; set; }
        public int DonoPeloOperador { get; set; }
        public int TitulosCompostos { get; set; }
        public int PerdidosComMotivoDoFormulario { get; set; }
        public int PerdidosSemFormulario { get; set; }
        public int TarefasComDono { get; set; }
        public int ConclusoesComDono { get; set; }
        public int InteracoesComDono { get; set; }
        public int PendentesDeProcessoEncerrado { get; set; }
        public int TarefasSemData { get; set; }
        public int InteracoesJaNoCrm { get; set; }
        public int InteracoesDeSistema { get; set; }
        public int InteracoesSemData { get; set; }
        public int ProcessosGeridosAtivos { get; set; }
        public int ProcessosAIncluir { get; set; }
        public int ProcessosAAlterar { get; set; }
        public int ProcessosARestaurar { get; set; }
        public int TarefasGeridasAtivas { get; set; }
        public int TarefasAIncluir { get; set; }
        public int TarefasAAlterar { get; set; }
        public int TarefasARestaurar { get; set; }
        public int CatalogosNovos { get; set; }
        public string? QuedaAceita { get; set; }
    }
}
