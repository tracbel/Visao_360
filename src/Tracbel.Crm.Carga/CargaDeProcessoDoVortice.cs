using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Protheus;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Carga;

/// <summary>
/// A CARGA DO RELACIONAMENTO — usuário, carteira, processo, tarefa e interação.
///
/// <para>É a segunda metade do ensaio da migração. A primeira (<see cref="CargaDoVortice"/>)
/// trouxe QUEM é o cliente; esta traz O QUE aconteceu com ele — e é ela que faz as telas de
/// Pipeline, Agenda, Cobertura e Visão 360 terem número em vez de exemplo.</para>
///
/// <para><b>A ordem não é arbitrária, é a das chaves estrangeiras.</b> Sem usuário não há dono de
/// carteira nem de tarefa; sem linha de negócio não há carteira; sem tipo de processo não há
/// fase; sem fase não há processo; sem tarefa não há interação com ponteiro de conclusão. Cada
/// etapa devolve o de-para que a seguinte consome, e nenhuma delas consulta o banco linha a
/// linha.</para>
///
/// <para><b>Idempotente pelo mesmo mecanismo:</b> uma linha em <c>integracao.ChaveExterna</c> por
/// registro, o recusado em <c>integracao.MensagemDescartada</c> e a marca em
/// <c>integracao.PontoDeSincronismo</c>. Rodar de novo reconcilia, não duplica.</para>
/// </summary>
internal sealed partial class CargaDeProcessoDoVortice(
    Func<CrmDbContext> abrirContexto,
    LeitorDeCargaDoVortice leitor,
    LeitorDeFaturamentoDoProtheus faturamentoDoProtheus,
    IReadOnlyDictionary<int, int> deParaDeFiliais,
    long usuarioResponsavelId,
    Action<string> relatar)
{
    private const int TamanhoDoBloco = 500;

    private const string FluxoDeUsuario = "VORTICE.CARGA.USUARIO";
    private const string FluxoDeCarteira = "VORTICE.CARGA.CARTEIRA";
    private const string FluxoDeVinculo = "VORTICE.CARGA.CLIENTE_CARTEIRA";
    private const string FluxoDeTerritorio = "VORTICE.CARGA.CARTEIRA_MUNICIPIO";
    private const string FluxoDeProcesso = "VORTICE.CARGA.PROCESSO";
    private const string FluxoDeTarefa = "VORTICE.CARGA.TAREFA";
    private const string FluxoDeInteracao = "VORTICE.CARGA.INTERACAO";
    private const string FluxoDeCatalogo = "VORTICE.CARGA.CATALOGO";

    /// <summary>
    /// O DOMÍNIO DO E-MAIL AUSENTE.
    ///
    /// <para>O cadastro de usuário do sistema de origem <b>não guarda e-mail</b>: zero dos 274
    /// usuários que esta carga precisa tem endereço preenchido, e a coluna de login corporativo
    /// também está vazia. A nossa coluna de e-mail é obrigatória, porque no CRM novo o usuário é
    /// espelho do Entra ID e o endereço é a identidade.</para>
    ///
    /// <para>Construir <c>login@tracbel.com.br</c> seria inventar um endereço que pode não
    /// existir — e que alguém, um dia, usaria para mandar mensagem. O sufixo <c>.invalid</c> é
    /// reservado pela RFC 2606 exatamente para isto: é um domínio que <b>não pode</b> existir,
    /// logo o valor não afirma nada. Ele é único por usuário (o índice exige), é legível na tela
    /// como "não informado", e some no dia em que o Entra ID entrar.</para>
    /// </summary>
    private const string DominioDeEmailAusente = "@sem-email.vortice.invalid";

    /// <summary>O tipo de tarefa que substitui a ação ausente na origem.</summary>
    private const string TipoDeTarefaNaoInformada = "NAO_INFORMADA";

    /// <summary>O motivo de perda que substitui o motivo que a origem nunca registrou.</summary>
    private const string MotivoDePerdaNaoInformado = "NAO_INFORMADO_NA_ORIGEM";

    private readonly Dictionary<string, ContagemDeSaneamento> _saneamento = new(StringComparer.Ordinal);
    private readonly List<LinhaRecusada> _recusas = [];
    private readonly Dictionary<string, int> _decisoes = new(StringComparer.Ordinal);

    /// <summary>Executa a carga inteira e devolve o que aconteceu.</summary>
    /// <param name="recorte">O recorte escolhido.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ResumoDoRelacionamento>> ExecutarAsync(
        RecorteDaCarga recorte, CancellationToken ct)
    {
        var sistemaId = await GarantirSistemaAsync(ct);

        // -----------------------------------------------------------------------------------------
        // 1. Usuário — sem ele nada tem dono.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo usuários do sistema de origem (sem senha, nunca)…");
        var usuarios = await leitor.LerUsuariosAsync(recorte, ct);
        if (!usuarios.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(usuarios.Erro!);
        Contabilizar(usuarios.Valor, u => (u.Correcoes, u.Rejeicoes));

        var (porChaveDeUsuario, porLogin) = await GravarUsuariosAsync(sistemaId, usuarios.Valor.Aceitos, ct);

        // -----------------------------------------------------------------------------------------
        // 2. Linha de negócio e carteira.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo linhas de negócio e carteiras…");
        var linhas = await leitor.LerLinhasDeNegocioAsync(recorte, ct);
        if (!linhas.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(linhas.Erro!);

        var porChaveDeLinha = await GravarLinhasDeNegocioAsync(sistemaId, linhas.Valor.Aceitos, ct);

        var carteiras = await leitor.LerCarteirasAsync(recorte, ct);
        if (!carteiras.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(carteiras.Erro!);
        Contabilizar(carteiras.Valor, c => (c.Correcoes, c.Rejeicoes));

        var porChaveDeCarteira = await GravarCarteirasAsync(
            sistemaId, carteiras.Valor.Aceitos, porChaveDeUsuario, porChaveDeLinha, ct);

        // -----------------------------------------------------------------------------------------
        // 2.1. O TERRITORIO DA CARTEIRA - quais cidades ela atende.
        //
        // E o agrupamento real, e ele desmente a "regional" do prototipo: IVS_Regional existe e
        // tem ZERO linhas. Entra aqui, logo depois da carteira, porque depende dela e do catalogo
        // de municipios que a carga de cadastro ja gravou.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo o territorio de cada carteira - as cidades que ela atende...");
        var territorio = await leitor.LerMunicipiosDeCarteiraAsync(recorte, ct);
        if (!territorio.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(territorio.Erro!);
        _recusas.AddRange(territorio.Valor.Recusadas);

        var (territorioGravado, carteirasComMunicipio) = await GravarMunicipiosDeCarteiraAsync(
            sistemaId, territorio.Valor.Aceitos, porChaveDeCarteira, ct);

        // -----------------------------------------------------------------------------------------
        // 3. Catálogos de processo e de tarefa.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo os catálogos que o dado de 2026 realmente usa…");

        var tiposDeProcesso = await leitor.LerTiposDeProcessoAsync(recorte, ct);
        if (!tiposDeProcesso.EhSucesso)
            return Resultado<ResumoDoRelacionamento>.Indisponivel(tiposDeProcesso.Erro!);

        var fases = await leitor.LerFasesAsync(recorte, ct);
        if (!fases.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(fases.Erro!);

        var tiposDeTarefa = await leitor.LerTiposDeTarefaAsync(recorte, ct);
        if (!tiposDeTarefa.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(tiposDeTarefa.Erro!);

        var resultados = await leitor.LerResultadosAsync(recorte, ct);
        if (!resultados.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(resultados.Erro!);
        _recusas.AddRange(resultados.Valor.Recusadas);

        var porChaveDeTipoDeProcesso = await GravarTiposDeProcessoAsync(
            sistemaId, tiposDeProcesso.Valor.Aceitos, ct);

        var porCodigoDeFase = await GravarFasesAsync(
            sistemaId, fases.Valor.Aceitos, porChaveDeTipoDeProcesso, ct);

        var porChaveDeTipoDeTarefa = await GravarTiposDeTarefaAsync(sistemaId, tiposDeTarefa.Valor.Aceitos, ct);

        var porChaveDeResultado = await GravarResultadosAsync(
            sistemaId, resultados.Valor.Aceitos, porChaveDeTipoDeTarefa, ct);

        relatar($"  catálogo: {porChaveDeTipoDeProcesso.Count} tipo(s) de processo, " +
                $"{porCodigoDeFase.Count} fase(s), {porChaveDeTipoDeTarefa.Count} tipo(s) de tarefa " +
                $"e {porChaveDeResultado.Count} desfecho(s) nasceram do dado real.");

        // -----------------------------------------------------------------------------------------
        // 4. O de-para de clientes que a primeira carga deixou pronto.
        // -----------------------------------------------------------------------------------------
        var porChaveDeCliente = await MapaDeChavesAsync(sistemaId, nameof(Cliente), ct);
        var empresaPorCliente = await MapaDeEmpresaPorClienteAsync(ct);

        // -----------------------------------------------------------------------------------------
        // 5. Carteirização.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo a carteirização…");
        var vinculos = await leitor.LerVinculosDeCarteiraAsync(recorte, ct);
        if (!vinculos.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(vinculos.Erro!);
        Contabilizar(vinculos.Valor, v => (v.Correcoes, v.Rejeicoes));

        var vinculosGravados = await GravarVinculosAsync(
            sistemaId, vinculos.Valor.Aceitos, porChaveDeCliente, porChaveDeCarteira, ct);

        // -----------------------------------------------------------------------------------------
        // 6. Processo.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo processos de 2026. A junção das duas tabelas de processo leva alguns minutos.");
        var processos = await leitor.LerProcessosAsync(recorte, ct);
        if (!processos.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(processos.Erro!);
        _recusas.AddRange(processos.Valor.Recusadas);
        Contabilizar(processos.Valor, p => (p.Correcoes, p.Rejeicoes));
        relatar($"  {processos.Valor.LinhasLidas} linha(s) lidas · {processos.Valor.Aceitos.Count} aceitas.");

        var motivoDePerdaId = await GarantirMotivoDePerdaAsync(ct);

        var porChaveDeProcesso = await GravarProcessosAsync(
            sistemaId, processos.Valor.Aceitos, porChaveDeCliente, porChaveDeTipoDeProcesso,
            porCodigoDeFase, porLogin, motivoDePerdaId, ct);

        // -----------------------------------------------------------------------------------------
        // 7. Tarefa.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo a agenda de 2026…");
        var tarefas = await leitor.LerTarefasAsync(recorte, ct);
        if (!tarefas.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(tarefas.Erro!);
        _recusas.AddRange(tarefas.Valor.Recusadas);
        Contabilizar(tarefas.Valor, t => (t.Correcoes, t.Rejeicoes));
        relatar($"  {tarefas.Valor.LinhasLidas} linha(s) lidas · {tarefas.Valor.Aceitos.Count} aceitas.");

        var tipoDeTarefaPadraoId = await GarantirTipoDeTarefaPadraoAsync(ct);

        var porChaveDeTarefa = await GravarTarefasAsync(
            sistemaId, tarefas.Valor.Aceitos, porChaveDeCliente, porChaveDeProcesso,
            porChaveDeTipoDeTarefa, porChaveDeResultado, porChaveDeUsuario, empresaPorCliente,
            tipoDeTarefaPadraoId, ct);

        // -----------------------------------------------------------------------------------------
        // 8. Interação.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo o histórico de 2026. É a maior leitura da carga.");
        var interacoes = await leitor.LerInteracoesAsync(recorte, ct);
        if (!interacoes.EhSucesso) return Resultado<ResumoDoRelacionamento>.Indisponivel(interacoes.Erro!);
        _recusas.AddRange(interacoes.Valor.Recusadas);
        Contabilizar(interacoes.Valor, i => (i.Correcoes, i.Rejeicoes));
        relatar($"  {interacoes.Valor.LinhasLidas} linha(s) lidas · {interacoes.Valor.Aceitos.Count} aceitas.");

        var interacoesGravadas = await GravarInteracoesAsync(
            sistemaId, interacoes.Valor.Aceitos, porChaveDeCliente, porChaveDeProcesso,
            porChaveDeTarefa, porChaveDeTipoDeTarefa, porChaveDeResultado, porChaveDeUsuario,
            empresaPorCliente, tipoDeTarefaPadraoId, ct);

        // -----------------------------------------------------------------------------------------
        // 9. POR QUE PERDEMOS — a resposta que estava no formulário, e não no processo.
        //
        // Entra depois do processo porque se pendura nele, e depois do cliente porque é o cliente
        // que dá a filial quando o processo ficou fora do recorte. É esta etapa que tira do ar a
        // frase "o legado não declara o motivo de nenhuma perda", que era falsa.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo as vendas perdidas — motivo, concorrente e diferença de preço…");
        var vendasPerdidas = await leitor.LerVendasPerdidasAsync(recorte, ct);
        if (!vendasPerdidas.EhSucesso)
            return Resultado<ResumoDoRelacionamento>.Indisponivel(vendasPerdidas.Erro!);
        _recusas.AddRange(vendasPerdidas.Valor.Recusadas);
        Contabilizar(vendasPerdidas.Valor, v => (v.Correcoes, v.Rejeicoes));
        relatar($"  {vendasPerdidas.Valor.LinhasLidas} resposta(s) lidas · " +
                $"{vendasPerdidas.Valor.Aceitos.Count} aceitas.");

        var vendasPerdidasGravadas = await GravarVendasPerdidasAsync(
            sistemaId, vendasPerdidas.Valor.Aceitos, porChaveDeProcesso, porChaveDeCliente,
            motivoDePerdaId, ct);

        relatar($"  {vendasPerdidasGravadas} venda(s) perdida(s) gravada(s).");

        // -----------------------------------------------------------------------------------------
        // 10. O FATURAMENTO, E A CLASSE QUE SAI DELE.
        //
        // Entra depois do cliente porque casa por documento, e antes dos derivados porque a
        // classe A/B/C/D é a chave de leitura de toda métrica de cobertura da gerência.
        // -----------------------------------------------------------------------------------------
        // O FATURAMENTO VEM DO PROTHEUS, E NÃO DA CÓPIA NO VÓRTICE.
        //
        // Até 06/09/2026 esta etapa lia `X_TOTVS_CRM_FATURAMENTO`, a tabela que o Vórtice RECEBE
        // do ERP — e que para em 11/04/2025. Era de lá que vinha a frase "o faturamento parou",
        // repetida em todas as telas por meses. Medido na origem: a SD2 tem nota emitida na
        // mesma semana. Morreu a integração, não o faturamento.
        //
        // A janela olha três anos para trás a partir de HOJE, porque agora o dado alcança hoje.
        relatar("Lendo o faturamento direto do Protheus — três anos, para a curva ABC ter base…");
        var desde = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-AnosDeFaturamento));
        var faturamento = await faturamentoDoProtheus.LerAsync(desde, relatar, ct);
        if (!faturamento.EhSucesso)
            return Resultado<ResumoDoRelacionamento>.Indisponivel(faturamento.Erro!);

        if (faturamento.Valor.ClientesSemCadastro > 0)
            Decidir(
                "Códigos de cliente na nota fiscal sem correspondência no cadastro do Protheus",
                faturamento.Valor.ClientesSemCadastro);

        if (faturamento.Valor.ItensSemData > 0)
            Decidir("Itens de nota descartados por não ter data de emissão legível",
                faturamento.Valor.ItensSemData);

        var (faturamentoGravado, _) = await GravarFaturamentoAsync(
            sistemaId, faturamento.Valor.Faturamento, ct);

        relatar(
            $"  {faturamentoGravado} mês(es) de faturamento gravado(s) · nota mais recente: " +
            $"{faturamento.Valor.EmissaoMaisRecente:dd/MM/yyyy}.");

        var curva = await ApurarCurvaAbcAsync(ct);
        relatar(
            "  curva ABC: " +
            string.Join(" · ", curva.OrderBy(p => p.Key).Select(p => $"{p.Key} {p.Value}")));

        // -----------------------------------------------------------------------------------------
        // 11. O que só se sabe DEPOIS: o duplo ponteiro e a data do último contato.
        // -----------------------------------------------------------------------------------------
        var (tarefasLigadas, carteirasComContato) = await ReconciliarDerivadosAsync(ct);

        relatar($"  duplo ponteiro: {tarefasLigadas} tarefa(s) ligadas à interação que as concluiu.");
        relatar($"  cobertura: {carteirasComContato} vínculo(s) de carteira com data de último contato " +
                "calculada das interações carregadas.");

        await GravarRecusasAsync(ct);

        await MarcarSincronismoAsync(sistemaId, FluxoDeUsuario, usuarios.Valor, porChaveDeUsuario.Count, ct);
        await MarcarSincronismoAsync(sistemaId, FluxoDeCarteira, carteiras.Valor, porChaveDeCarteira.Count, ct);
        await MarcarSincronismoAsync(sistemaId, FluxoDeVinculo, vinculos.Valor, vinculosGravados, ct);
        await MarcarSincronismoAsync(
            sistemaId, FluxoDeTerritorio, territorio.Valor, territorioGravado, ct);
        await MarcarSincronismoAsync(
            sistemaId, FluxoDeCatalogo, tiposDeTarefa.Valor, porChaveDeTipoDeTarefa.Count, ct);
        await MarcarSincronismoAsync(
            sistemaId, FluxoDeProcesso, processos.Valor, porChaveDeProcesso.Count, ct);
        await MarcarSincronismoAsync(sistemaId, FluxoDeTarefa, tarefas.Valor, porChaveDeTarefa.Count, ct);
        await MarcarSincronismoAsync(
            sistemaId, FluxoDeInteracao, interacoes.Valor, interacoesGravadas, ct);
        await MarcarSincronismoAsync(
            sistemaId, FluxoDeVendaPerdida, vendasPerdidas.Valor, vendasPerdidasGravadas, ct);

        return Resultado<ResumoDoRelacionamento>.Ok(new ResumoDoRelacionamento(
            UsuariosLidos: usuarios.Valor.LinhasLidas,
            UsuariosGravados: porChaveDeUsuario.Count,
            LinhasDeNegocioGravadas: porChaveDeLinha.Count,
            CarteirasLidas: carteiras.Valor.LinhasLidas,
            CarteirasGravadas: porChaveDeCarteira.Count,
            MunicipiosDeCarteiraLidos: territorio.Valor.LinhasLidas,
            MunicipiosDeCarteiraGravados: territorioGravado,
            CarteirasComMunicipio: carteirasComMunicipio,
            VinculosLidos: vinculos.Valor.LinhasLidas,
            VinculosGravados: vinculosGravados,
            TiposDeProcessoGravados: porChaveDeTipoDeProcesso.Count,
            FasesGravadas: porCodigoDeFase.Count,
            TiposDeTarefaGravados: porChaveDeTipoDeTarefa.Count,
            ResultadosGravados: porChaveDeResultado.Count,
            ProcessosLidos: processos.Valor.LinhasLidas,
            ProcessosGravados: porChaveDeProcesso.Count,
            TarefasLidas: tarefas.Valor.LinhasLidas,
            TarefasGravadas: porChaveDeTarefa.Count,
            InteracoesLidas: interacoes.Valor.LinhasLidas,
            InteracoesGravadas: interacoesGravadas,
            TarefasLigadasAInteracao: tarefasLigadas,
            VinculosComUltimoContato: carteirasComContato,
            VendasPerdidasLidas: vendasPerdidas.Valor.LinhasLidas,
            VendasPerdidasGravadas: vendasPerdidasGravadas,
            MesesDeFaturamentoGravados: faturamentoGravado,
            ClientesPorClasse: curva.ToDictionary(p => p.Key.ToString(), p => p.Value, StringComparer.Ordinal),
            Recusadas: _recusas.Count,
            Saneamento: _saneamento.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal),
            RecusasPorMotivo: _recusas
                .GroupBy(r => $"{r.Entidade} — {r.Categoria}", StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal),
            ExemplosDeRecusa: _recusas
                .GroupBy(r => $"{r.Entidade} — {r.Categoria}", StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => Resumir(g.First().Motivo), StringComparer.Ordinal),
            Decisoes: _decisoes.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal)));
    }

    // =============================================================================================
    // Usuário
    // =============================================================================================

    private async Task<(Dictionary<string, long> PorChave, Dictionary<string, long> PorLogin)>
        GravarUsuariosAsync(int sistemaId, IReadOnlyList<UsuarioParaCarga> usuarios, CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(Usuario), ct);
        var porLogin = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var principaisUsados = (await contexto.Usuarios.AsNoTracking()
                .Select(u => u.NomePrincipal)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var novos = new List<(string Chave, string Login, Usuario Entidade)>();

        foreach (var usuario in usuarios)
        {
            if (!deParaDeFiliais.TryGetValue(usuario.CodigoDaFilialNoLegado, out var empresaId))
            {
                Recusar(nameof(Usuario), "Filial da origem sem correspondência no CRM", usuario.ChaveDeOrigem,
                    $"A filial {usuario.CodigoDaFilialNoLegado} não corresponde a nenhuma filial em " +
                    "operação do CRM.", usuario);
                continue;
            }

            var principal = usuario.Login.ToLowerInvariant() + DominioDeEmailAusente;

            // NEM TODA CONTA DO LEGADO É GENTE. Departamento, servidor da origem, fornecedor e
            // resíduo de teste convivem com pessoa na mesma tabela, e somados como pessoa
            // distorcem qualquer ranking de CEN.
            var decisoesDaNatureza = new List<CorrecaoAplicada>();
            var natureza = ClassificacaoDeNatureza.DoUsuario(
                usuario.NomeCompleto, usuario.Login, decisoesDaNatureza);

            if (decisoesDaNatureza.Count > 0)
                Decidir($"Contas classificadas como {natureza} (não entram em ranking de pessoa)", 1);

            if (mapa.TryGetValue(usuario.ChaveDeOrigem, out var existenteId))
            {
                var entidade = await contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == existenteId, ct);

                if (entidade is { EstaExcluido: false })
                {
                    entidade.Alterar(
                        usuario.NomeCompleto, NomeCurto(usuario.NomeCompleto), empresaId,
                        usuario.UltimoAcessoEm, usuarioResponsavelId);

                    entidade.Classificar(natureza, usuarioResponsavelId);
                    porLogin[usuario.Login] = entidade.Id;
                }

                continue;
            }

            if (!principaisUsados.Add(principal))
            {
                Recusar(nameof(Usuario), "Login repetido no cadastro de origem", usuario.ChaveDeOrigem,
                    $"O login '{usuario.Login}' já pertence a outro usuário. O sistema de origem " +
                    "admite dois cadastros com o mesmo login; aqui o índice único não admite.",
                    usuario);
                continue;
            }

            // A IDENTIDADE EXTERNA É DERIVADA, E DECLARADAMENTE PROVISÓRIA. O usuário do CRM é
            // espelho do Entra ID, e o legado não tem o identificador de objeto do Entra. O GUID
            // nasce de um resumo do login: é ESTÁVEL entre execuções (o de-para reencontra a
            // mesma pessoa) e nunca colide com um GUID de verdade, porque não foi sorteado.
            var novo = Usuario.Criar(
                GuidEstavelDe($"VORTICE.USUARIO.{usuario.Login.ToUpperInvariant()}"),
                principal,
                usuario.NomeCompleto,
                NomeCurto(usuario.NomeCompleto),
                Email.Criar(principal),
                empresaId,
                usuarioResponsavelId,
                usuario.UltimoAcessoEm,
                natureza: natureza);

            contexto.Usuarios.Add(novo);
            novos.Add((usuario.ChaveDeOrigem, usuario.Login, novo));
        }

        await contexto.SaveChangesAsync(ct);

        foreach (var (chave, login, entidade) in novos)
        {
            contexto.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, nameof(Usuario), entidade.Id, chave));
            mapa[chave] = entidade.Id;
            porLogin[login] = entidade.Id;
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        Decidir("Usuários migrados sem e-mail (o cadastro de origem não guarda nenhum)", mapa.Count);
        relatar($"  usuários: {mapa.Count} no de-para, {novos.Count} novos nesta rodada.");

        return (mapa, porLogin);
    }

    // =============================================================================================
    // Linha de negócio e carteira
    // =============================================================================================

    private async Task<Dictionary<string, int>> GravarLinhasDeNegocioAsync(
        int sistemaId, IReadOnlyList<LinhaDeNegocioParaCarga> linhas, CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(LinhaDeNegocio), ct);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var codigosUsados = (await contexto.LinhasDeNegocio.AsNoTracking()
                .Select(l => l.Codigo)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var novos = new List<(string Chave, LinhaDeNegocio Entidade)>();

        // A CADÊNCIA ATRAVESSA MESMO QUANDO A LINHA JÁ EXISTE. Ela era lida da origem e
        // descartada aqui; quem já rodou a carga antes tem as linhas gravadas com os quatro
        // campos nulos, e sem uma segunda passagem eles ficariam nulos para sempre.
        var existentes = await contexto.LinhasDeNegocio.ToDictionaryAsync(l => l.Id, l => l, ct);
        var comCadencia = 0;

        foreach (var linha in linhas)
        {
            if (mapa.TryGetValue(linha.ChaveDeOrigem, out var jaGravada))
            {
                if (existentes.TryGetValue((int)jaGravada, out var entidade)
                    && DeclararCadencia(entidade, linha)) comCadencia++;

                continue;
            }

            if (!codigosUsados.Add(linha.Codigo)) continue;

            var nova = LinhaDeNegocio.Criar(linha.Codigo, linha.Nome);
            if (DeclararCadencia(nova, linha)) comCadencia++;

            contexto.LinhasDeNegocio.Add(nova);
            novos.Add((linha.ChaveDeOrigem, nova));
        }

        if (comCadencia > 0)
            Decidir("Linhas de negócio com cadência de visita declarada na origem", comCadencia);

        await contexto.SaveChangesAsync(ct);

        foreach (var (chave, entidade) in novos)
        {
            contexto.ChavesExternas.Add(
                ChaveExterna.Criar(sistemaId, nameof(LinhaDeNegocio), entidade.Id, chave));
            mapa[chave] = entidade.Id;
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        return mapa.ToDictionary(p => p.Key, p => (int)p.Value, StringComparer.Ordinal);
    }

    private async Task<Dictionary<string, long>> GravarCarteirasAsync(
        int sistemaId,
        IReadOnlyList<CarteiraParaCarga> carteiras,
        IReadOnlyDictionary<string, long> porChaveDeUsuario,
        IReadOnlyDictionary<string, int> porChaveDeLinha,
        CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(Carteira), ct);
        var linhaPadraoId = await GarantirLinhaDeNegocioPadraoAsync(ct);

        // OS NOMES QUE A CLASSIFICAÇÃO PRECISA LER. A natureza da carteira se decide pelo que a
        // ORIGEM declara — a linha de negócio "DADOS CADASTRAIS", a palavra TESTE no nome, o
        // fornecedor no responsável —, e nada disso está no identificador.
        var nomeDaLinha = await NomeDasLinhasDeNegocioAsync(ct);
        var nomeDoUsuario = await NomeDosUsuariosAsync(ct);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var codigosUsados = (await contexto.Carteiras.AsNoTracking()
                .Select(c => c.Codigo)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var novos = new List<(string Chave, Carteira Entidade)>();

        foreach (var carteira in carteiras)
        {
            if (!deParaDeFiliais.TryGetValue(carteira.CodigoDaFilialNoLegado, out var empresaId))
            {
                Recusar(nameof(Carteira), "Filial da origem sem correspondência no CRM",
                    carteira.ChaveDeOrigem,
                    $"A filial {carteira.CodigoDaFilialNoLegado} não corresponde a nenhuma filial em " +
                    "operação do CRM.", carteira);
                continue;
            }

            var responsavelId = carteira.ChaveDoResponsavelDeOrigem is { } dono
                                && porChaveDeUsuario.TryGetValue(dono, out var achado)
                ? achado
                : usuarioResponsavelId;

            if (responsavelId == usuarioResponsavelId)
                Decidir("Carteiras sem CEN na origem, atribuídas ao operador da carga", 1);

            var linhaId = carteira.ChaveDaLinhaDeNegocioDeOrigem is { } depto
                          && porChaveDeLinha.TryGetValue(depto, out var linha)
                ? linha
                : linhaPadraoId;

            var decisoesDaNatureza = new List<CorrecaoAplicada>();
            var natureza = ClassificacaoDeNatureza.DaCarteira(
                carteira.Nome,
                nomeDaLinha.GetValueOrDefault(linhaId),
                nomeDoUsuario.GetValueOrDefault(responsavelId),
                decisoesDaNatureza);

            if (decisoesDaNatureza.Count > 0)
                Decidir($"Carteiras classificadas como {natureza} (fora do número de gerência)", 1);

            if (mapa.TryGetValue(carteira.ChaveDeOrigem, out var existenteId))
            {
                var entidade = await contexto.Carteiras.FirstOrDefaultAsync(c => c.Id == existenteId, ct);
                if (entidade is { EstaExcluido: false })
                {
                    entidade.Alterar(carteira.Nome, responsavelId, linhaId, usuarioResponsavelId);
                    entidade.Classificar(natureza, usuarioResponsavelId);
                }

                continue;
            }

            if (!codigosUsados.Add(carteira.Codigo)) continue;

            var nova = Carteira.Criar(
                empresaId, linhaId, carteira.Codigo, carteira.Nome, responsavelId,
                usuarioResponsavelId, natureza: natureza);

            contexto.Carteiras.Add(nova);
            novos.Add((carteira.ChaveDeOrigem, nova));
        }

        await contexto.SaveChangesAsync(ct);

        foreach (var (chave, entidade) in novos)
        {
            contexto.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, nameof(Carteira), entidade.Id, chave));
            mapa[chave] = entidade.Id;
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        Decidir("Carteiras sem supervisor na origem (a coluna é nula em 100% delas)", carteiras.Count);
        relatar($"  carteiras: {mapa.Count} no de-para, {novos.Count} novas nesta rodada.");

        return mapa;
    }

    /// <summary>
    /// Grava o TERRITORIO DE CADA CARTEIRA - o par carteira x municipio.
    ///
    /// <para><b>O municipio vem do de-para que a carga de cadastro deixou pronto.</b> Ele nao e
    /// procurado por nome: a origem aponta para a cidade por sequencial, e o de-para de
    /// <c>integracao.ChaveExterna</c> traduz esse sequencial - inclusive quando ele e uma das
    /// linhas duplicadas do catalogo antigo, porque todas as chaves do grupo apontam para o mesmo
    /// municipio. Vinculo cujo municipio nao esta no de-para e RECUSADO e contado, nunca
    /// adivinhado.</para>
    ///
    /// <para><b>Idempotente e reversivel.</b> Rodar de novo reencontra a linha pelo par
    /// (carteira, municipio) e reabre o vinculo se a origem voltou a declara-lo. O par que a
    /// origem deixou de declarar NAO e apagado: e encerrado com data, porque tirar uma cidade de
    /// uma carteira e decisao comercial e o historico dela vale.</para>
    /// </summary>
    private async Task<(int Gravados, int CarteirasAlcancadas)> GravarMunicipiosDeCarteiraAsync(
        int sistemaId,
        IReadOnlyList<MunicipioDeCarteiraParaCarga> territorio,
        IReadOnlyDictionary<string, long> porChaveDeCarteira,
        CancellationToken ct)
    {
        var porChaveDeMunicipio = await MapaDeChavesAsync(sistemaId, nameof(Municipio), ct);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var existentes = (await contexto.CarteiraMunicipios.ToListAsync(ct))
            .ToDictionary(v => (v.CarteiraId, v.MunicipioId));

        var declarados = new HashSet<(long Carteira, int Municipio)>();
        var gravados = 0;

        foreach (var linha in territorio)
        {
            if (!porChaveDeCarteira.TryGetValue(linha.ChaveDaCarteiraDeOrigem, out var carteiraId))
            {
                Recusar(nameof(CarteiraMunicipio), "Carteira do territorio fora da carga",
                    linha.ChaveDeOrigem,
                    $"A carteira {linha.ChaveDaCarteiraDeOrigem} nao entrou na carga - ela nao " +
                    "tem cliente do recorte, ou e de filial que nao opera.", linha);
                continue;
            }

            if (!porChaveDeMunicipio.TryGetValue(linha.ChaveDoMunicipioDeOrigem, out var municipioId))
            {
                Recusar(nameof(CarteiraMunicipio), "Municipio do territorio fora do catalogo",
                    linha.ChaveDeOrigem,
                    $"A cidade {linha.ChaveDoMunicipioDeOrigem} nao esta no catalogo de " +
                    "municipios do CRM. Ou a carga de cadastro nao rodou, ou a cidade foi " +
                    "recusada por UF fora das 27.", linha);
                continue;
            }

            var par = (carteiraId, (int)municipioId);
            declarados.Add(par);

            if (existentes.TryGetValue(par, out var vigente))
            {
                vigente.Revincular();
                gravados++;
                continue;
            }

            contexto.CarteiraMunicipios.Add(
                CarteiraMunicipio.Criar(
                    carteiraId, (int)municipioId, usuarioResponsavelId, linha.AtualizadoEm));

            gravados++;
        }

        // O QUE A ORIGEM DEIXOU DE DECLARAR E ENCERRADO, NAO APAGADO - mesmo principio do
        // soft delete: perder o rastro de "esta cidade ja foi desta carteira" e perder a resposta
        // de por que um cliente daquela cidade tem o historico que tem.
        var encerrados = 0;

        foreach (var (par, linha) in existentes)
        {
            if (declarados.Contains(par) || linha.DesvinculadoEm is not null) continue;

            linha.Desvincular(DateTime.UtcNow);
            encerrados++;
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        var carteirasAlcancadas = declarados.Select(p => p.Carteira).Distinct().Count();

        if (encerrados > 0)
            Decidir("Municipios encerrados por a origem ter deixado de declara-los", encerrados);

        relatar($"  territorio: {gravados} vinculo(s) carteira x municipio vigentes, em " +
                $"{carteirasAlcancadas} carteira(s).");

        return (gravados, carteirasAlcancadas);
    }

    private async Task<int> GravarVinculosAsync(
        int sistemaId,
        IReadOnlyList<VinculoDeCarteiraParaCarga> vinculos,
        IReadOnlyDictionary<string, long> porChaveDeCliente,
        IReadOnlyDictionary<string, long> porChaveDeCarteira,
        CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(ClienteCarteira), ct);

        await using var leitura = abrirContexto();

        // O ÍNDICE ÚNICO É (cliente, carteira) VIGENTE, e a chave da origem é
        // (pessoa, departamento, carteira) — três colunas contra duas. Duas linhas da origem
        // podem cair no mesmo par, e é o que acontece quando a mesma carteira serve dois
        // departamentos. A conferência aqui existe para a segunda virar recusa com motivo, em
        // vez de derrubar o bloco no índice.
        var paresUsados = (await leitura.ClienteCarteiras.AsNoTracking()
                .Where(v => v.DesvinculadoEm == null)
                .Select(v => new { v.ClienteId, v.CarteiraId })
                .ToListAsync(ct))
            .Select(v => (v.ClienteId, v.CarteiraId))
            .ToHashSet();

        var gravados = 0;

        foreach (var bloco in vinculos.Chunk(TamanhoDoBloco))
        {
            await using var contexto = abrirContexto();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novos = new List<(string Chave, ClienteCarteira Entidade)>();
            var reconciliados = new List<string>();

            foreach (var vinculo in bloco)
            {
                if (mapa.ContainsKey(vinculo.ChaveDeOrigem))
                {
                    reconciliados.Add(vinculo.ChaveDeOrigem);
                    continue;
                }

                if (!porChaveDeCliente.TryGetValue(vinculo.ChaveDoClienteDeOrigem, out var clienteId))
                {
                    Recusar(nameof(ClienteCarteira), "Cliente do vínculo fora da carga",
                        vinculo.ChaveDeOrigem,
                        "O cliente deste vínculo de carteira não entrou na carga de cadastro.",
                        vinculo);
                    continue;
                }

                if (!porChaveDeCarteira.TryGetValue(vinculo.ChaveDaCarteiraDeOrigem, out var carteiraId))
                {
                    Recusar(nameof(ClienteCarteira), "Carteira do vínculo fora da carga",
                        vinculo.ChaveDeOrigem,
                        "A carteira deste vínculo não entrou na carga — ela pertence a uma filial " +
                        "fora das que estão em operação.", vinculo);
                    continue;
                }

                if (!paresUsados.Add((clienteId, carteiraId)))
                {
                    Recusar(nameof(ClienteCarteira), "Cliente já vinculado a esta carteira",
                        vinculo.ChaveDeOrigem,
                        "A chave do vínculo na origem tem três colunas (pessoa, departamento e " +
                        "carteira) e a nossa tem duas. A mesma pessoa aparece duas vezes na mesma " +
                        "carteira, por departamentos diferentes; o segundo vínculo foi recusado.",
                        vinculo);
                    continue;
                }

                var nova = ClienteCarteira.Criar(
                    clienteId, carteiraId, vinculo.Classe, usuarioResponsavelId,
                    vinculo.DiasCicloContato, vinculo.VinculadoEm);

                contexto.ClienteCarteiras.Add(nova);
                novos.Add((vinculo.ChaveDeOrigem, nova));
            }

            await contexto.SaveChangesAsync(ct);

            foreach (var (chave, entidade) in novos)
            {
                contexto.ChavesExternas.Add(
                    ChaveExterna.Criar(sistemaId, nameof(ClienteCarteira), entidade.Id, chave));
                mapa[chave] = entidade.Id;
            }

            await CarimbarConciliacaoAsync(contexto, sistemaId, nameof(ClienteCarteira), reconciliados, ct);
            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);

            gravados += novos.Count + reconciliados.Count;
            relatar($"  carteirização: {gravados}/{vinculos.Count}");
        }

        return gravados;
    }

    // =============================================================================================
    // Catálogos de processo e de tarefa
    // =============================================================================================

    private async Task<Dictionary<string, int>> GravarTiposDeProcessoAsync(
        int sistemaId, IReadOnlyList<TipoDeProcessoParaCarga> tipos, CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(TipoProcesso), ct);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var codigosUsados = (await contexto.TiposDeProcesso.AsNoTracking()
                .Select(t => t.Codigo)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var novos = new List<(string Chave, TipoProcesso Entidade)>();

        foreach (var tipo in tipos)
        {
            if (mapa.ContainsKey(tipo.ChaveDeOrigem)) continue;
            if (!codigosUsados.Add(tipo.Codigo)) continue;

            var novo = TipoProcesso.Criar(tipo.Codigo, tipo.Nome, estaAtivo: tipo.EstaAtivoNaOrigem);
            contexto.TiposDeProcesso.Add(novo);
            novos.Add((tipo.ChaveDeOrigem, novo));
        }

        await contexto.SaveChangesAsync(ct);

        foreach (var (chave, entidade) in novos)
        {
            contexto.ChavesExternas.Add(
                ChaveExterna.Criar(sistemaId, nameof(TipoProcesso), entidade.Id, chave));
            mapa[chave] = entidade.Id;
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        return mapa.ToDictionary(p => p.Key, p => (int)p.Value, StringComparer.Ordinal);
    }

    private async Task<Dictionary<(string Tipo, string Fase), int>> GravarFasesAsync(
        int sistemaId,
        IReadOnlyList<FaseParaCarga> fases,
        IReadOnlyDictionary<string, int> porChaveDeTipoDeProcesso,
        CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var existentes = (await contexto.Fases.AsNoTracking()
                .Select(f => new { f.Id, f.TipoProcessoId, f.Codigo })
                .ToListAsync(ct))
            .ToDictionary(f => (f.TipoProcessoId, f.Codigo), f => f.Id);

        var novos = new List<((string Tipo, string Fase) Chave, Fase Entidade)>();
        var mapa = new Dictionary<(string, string), int>();

        foreach (var fase in fases)
        {
            if (!porChaveDeTipoDeProcesso.TryGetValue(fase.ChaveDoTipoDeProcessoDeOrigem, out var tipoId))
                continue;

            if (existentes.TryGetValue((tipoId, fase.Codigo), out var achado))
            {
                mapa[(fase.ChaveDoTipoDeProcessoDeOrigem, fase.Codigo)] = achado;
                continue;
            }

            var nova = Fase.Criar(tipoId, fase.Codigo, fase.Nome, fase.Ordem, fase.EhFinal);
            contexto.Fases.Add(nova);
            existentes[(tipoId, fase.Codigo)] = 0;
            novos.Add(((fase.ChaveDoTipoDeProcessoDeOrigem, fase.Codigo), nova));
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        foreach (var (chave, entidade) in novos) mapa[chave] = entidade.Id;

        return mapa;
    }

    private async Task<Dictionary<string, int>> GravarTiposDeTarefaAsync(
        int sistemaId, IReadOnlyList<TipoDeTarefaParaCarga> tipos, CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(TipoTarefa), ct);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var codigosUsados = (await contexto.TiposDeTarefa.AsNoTracking()
                .Select(t => t.Codigo)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var novos = new List<(string Chave, TipoTarefa Entidade)>();
        var semPrazo = 0;

        foreach (var tipo in tipos)
        {
            if (tipo.PrazoDiasUteis == 0) semPrazo++;
            if (mapa.ContainsKey(tipo.ChaveDeOrigem)) continue;
            if (!codigosUsados.Add(tipo.Codigo)) continue;

            // A CATEGORIA NÃO VEM DA ORIGEM, e não é adivinhada aqui. O cadastro de ação do
            // legado tem uma coluna de classe com sete letras de uma posição, sem catálogo no
            // banco e com o significado dentro do cliente Gupta. "Monitorar Cliente" pode ser
            // visita, ligação ou WhatsApp — o dado não diz qual, e classificar por palavra do
            // nome seria afirmar o que ninguém sabe. Entra Interna, que é o valor que NÃO afirma
            // contato com o cliente, e a triagem das 178 ações fica como pergunta ao negócio.
            var novo = TipoTarefa.Criar(
                tipo.Codigo, tipo.Nome, CategoriaDeInteracao.Interna,
                tipo.PrazoDiasUteis, contaParaCobertura: false,
                estaAtivo: tipo.EstaAtivoNaOrigem, ultimoUsoEm: tipo.UltimoUsoEm);

            contexto.TiposDeTarefa.Add(novo);
            novos.Add((tipo.ChaveDeOrigem, novo));
        }

        await contexto.SaveChangesAsync(ct);

        foreach (var (chave, entidade) in novos)
        {
            contexto.ChavesExternas.Add(
                ChaveExterna.Criar(sistemaId, nameof(TipoTarefa), entidade.Id, chave));
            mapa[chave] = entidade.Id;
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        Decidir("Tipos de tarefa sem prazo declarado na origem", semPrazo);
        Decidir("Tipos de tarefa sem categoria de contato na origem", tipos.Count);

        return mapa.ToDictionary(p => p.Key, p => (int)p.Value, StringComparer.Ordinal);
    }

    private async Task<Dictionary<string, int>> GravarResultadosAsync(
        int sistemaId,
        IReadOnlyList<ResultadoParaCarga> resultados,
        IReadOnlyDictionary<string, int> porChaveDeTipoDeTarefa,
        CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(Dominio.Processo.Resultado), ct);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var existentes = (await contexto.Resultados.AsNoTracking()
                .Select(r => new { r.TipoTarefaId, r.Codigo })
                .ToListAsync(ct))
            .Select(r => (r.TipoTarefaId, r.Codigo))
            .ToHashSet();

        var novos = new List<(string Chave, Dominio.Processo.Resultado Entidade)>();
        var semMapeamento = 0;

        foreach (var resultado in resultados)
        {
            if (!resultado.ClasseVeioDoMapeamento) semMapeamento++;
            if (mapa.ContainsKey(resultado.ChaveDeOrigem)) continue;

            if (!porChaveDeTipoDeTarefa.TryGetValue(resultado.ChaveDoTipoDeTarefaDeOrigem, out var tipoId))
            {
                Recusar(nameof(Dominio.Processo.Resultado), "Ação dona do desfecho fora da carga",
                    resultado.ChaveDeOrigem,
                    "A ação a que este desfecho pertence não aparece no dado do período e não foi " +
                    "carregada. O desfecho ficaria sem tipo de tarefa.", resultado);
                continue;
            }

            if (!existentes.Add((tipoId, resultado.Codigo))) continue;

            var novo = Dominio.Processo.Resultado.Criar(
                tipoId, resultado.Codigo, resultado.Nome, resultado.Classe,
                estaAtivo: true, ultimoUsoEm: resultado.UltimoUsoEm);

            contexto.Resultados.Add(novo);
            novos.Add((resultado.ChaveDeOrigem, novo));
        }

        await contexto.SaveChangesAsync(ct);

        foreach (var (chave, entidade) in novos)
        {
            contexto.ChavesExternas.Add(
                ChaveExterna.Criar(sistemaId, nameof(Dominio.Processo.Resultado), entidade.Id, chave));
            mapa[chave] = entidade.Id;
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        Decidir("Desfechos sem mapeamento de fase nem de status na origem (entraram como Manutenção)",
            semMapeamento);

        return mapa.ToDictionary(p => p.Key, p => (int)p.Value, StringComparer.Ordinal);
    }

    // =============================================================================================
    // Processo
    // =============================================================================================

    private async Task<Dictionary<string, long>> GravarProcessosAsync(
        int sistemaId,
        IReadOnlyList<ProcessoParaCarga> processos,
        IReadOnlyDictionary<string, long> porChaveDeCliente,
        IReadOnlyDictionary<string, int> porChaveDeTipoDeProcesso,
        IReadOnlyDictionary<(string Tipo, string Fase), int> porCodigoDeFase,
        IReadOnlyDictionary<string, long> porLogin,
        int motivoDePerdaId,
        CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(Dominio.Processo.Processo), ct);
        var gravados = 0;
        var semTitulo = 0;
        var semResponsavel = 0;

        foreach (var bloco in processos.Chunk(TamanhoDoBloco))
        {
            await using var contexto = abrirContexto();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novos = new List<(string Chave, Dominio.Processo.Processo Entidade)>();
            var reconciliados = new List<string>();

            foreach (var processo in bloco)
            {
                if (!processo.TituloVeioDaOrigem) semTitulo++;

                if (!deParaDeFiliais.TryGetValue(processo.CodigoDaFilialNoLegado, out var empresaId))
                    continue;

                if (!porChaveDeCliente.TryGetValue(processo.ChaveDoClienteDeOrigem, out var clienteId))
                {
                    Recusar(nameof(Dominio.Processo.Processo), "Cliente do processo fora da carga",
                        processo.ChaveDeOrigem,
                        "O cliente deste processo não entrou na carga de cadastro — ou foi recusado, " +
                        "ou está fora do recorte. Processo sem cliente é exatamente o que o modelo " +
                        "novo existe para impedir.", processo);
                    continue;
                }

                if (!porChaveDeTipoDeProcesso.TryGetValue(
                        processo.ChaveDoTipoDeProcessoDeOrigem, out var tipoId))
                {
                    Recusar(nameof(Dominio.Processo.Processo), "Tipo de processo fora do catálogo",
                        processo.ChaveDeOrigem,
                        "O fluxo deste processo não está no catálogo carregado.", processo);
                    continue;
                }

                if (!porCodigoDeFase.TryGetValue(
                        (processo.ChaveDoTipoDeProcessoDeOrigem, processo.CodigoDaFase), out var faseId))
                {
                    Recusar(nameof(Dominio.Processo.Processo), "Fase fora do catálogo do fluxo",
                        processo.ChaveDeOrigem,
                        $"A fase '{processo.CodigoDaFase}' não existe no catálogo do fluxo carregado.",
                        processo);
                    continue;
                }

                var proprietarioId = processo.LoginDoResponsavelNaOrigem is { } login
                                     && porLogin.TryGetValue(login, out var dono)
                    ? dono
                    : usuarioResponsavelId;

                if (proprietarioId == usuarioResponsavelId) semResponsavel++;

                if (mapa.TryGetValue(processo.ChaveDeOrigem, out var existenteId))
                {
                    var entidade = await contexto.Processos.FirstOrDefaultAsync(p => p.Id == existenteId, ct);

                    if (entidade is { EstaExcluido: false })
                    {
                        entidade.ColocarNaFase(faseId, processo.FaseDesde, usuarioResponsavelId);
                        AplicarSituacao(entidade, processo, motivoDePerdaId);
                        reconciliados.Add(processo.ChaveDeOrigem);
                    }

                    continue;
                }

                var novo = Dominio.Processo.Processo.Abrir(
                    numero: long.TryParse(processo.ChaveDeOrigem, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out var numero)
                        ? numero
                        : 0,
                    empresaId: empresaId,
                    tipoProcessoId: tipoId,
                    clienteId: clienteId,
                    faseId: faseId,
                    titulo: processo.Titulo,
                    proprietarioId: proprietarioId,
                    criadoPorId: usuarioResponsavelId,
                    descricao: processo.Descricao,
                    // O VALOR PASSA PELO TIPO, E O TIPO PODE RECUSAR. Dinheiro exige duas casas
                    // decimais e valor não negativo; a coluna da origem é decimal livre. Um
                    // valor com três casas não é "quase dois centavos" — é dado corrompido, e
                    // arredondá-lo em silêncio seria o mesmo erro que a esteira de integração do
                    // legado comete. Ele sai vazio, com a recusa contada.
                    valorEstimado: ValorQuePodeEntrar(processo),
                    quantidade: processo.Quantidade,
                    previsaoConclusao: processo.PrevisaoConclusao,
                    previsaoConclusaoOriginal: processo.PrevisaoConclusaoOriginal,
                    abertoEmUtc: processo.AbertoEm);

                novo.ColocarNaFase(faseId, processo.FaseDesde, usuarioResponsavelId);
                AplicarSituacao(novo, processo, motivoDePerdaId);

                contexto.Processos.Add(novo);
                novos.Add((processo.ChaveDeOrigem, novo));
            }

            await contexto.SaveChangesAsync(ct);

            foreach (var (chave, entidade) in novos)
            {
                contexto.ChavesExternas.Add(
                    ChaveExterna.Criar(sistemaId, nameof(Dominio.Processo.Processo), entidade.Id, chave));
                mapa[chave] = entidade.Id;
            }

            await CarimbarConciliacaoAsync(
                contexto, sistemaId, nameof(Dominio.Processo.Processo), reconciliados, ct);
            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);

            gravados += novos.Count + reconciliados.Count;
            relatar($"  processos: {gravados}/{processos.Count}");
        }

        Decidir("Processos cujo título foi composto do número, por falta de resumo na origem", semTitulo);
        Decidir("Processos cujo login de responsável não existe no cadastro de usuários", semResponsavel);

        return mapa;
    }

    /// <summary>
    /// Converte o valor da origem pelo tipo de valor do domínio, contando a recusa.
    ///
    /// <para>O tipo exige duas casas decimais e valor não negativo. Quando a origem não cabe
    /// nele, o campo sai vazio — nunca arredondado. É o princípio 1.3 do documento 16 aplicado à
    /// coluna que sustenta o valor do funil.</para>
    /// </summary>
    private Dinheiro? ValorQuePodeEntrar(ProcessoParaCarga processo)
    {
        if (processo.ValorEstimado is not { } valor) return null;

        if (Dinheiro.TentarCriar(valor, out var dinheiro)) return dinheiro;

        Decidir("Processos cujo valor da origem não é um valor monetário válido (saiu vazio)", 1);
        return null;
    }

    /// <summary>
    /// Aplica a situação traduzida, resolvendo as duas exigências do modelo: encerrar tem data e
    /// perder tem motivo.
    ///
    /// <para>O motivo usado é um item declarado — <c>NÃO INFORMADO NA ORIGEM</c>. Não é
    /// contorno de restrição: é a resposta honesta a um dado que o legado não tem. 83% das saídas
    /// do funil de 2026 não declaram motivo de perda nenhum, e o relatório de perdas do CRM novo
    /// precisa mostrar exatamente isso, em vez de mostrar uma distribuição inventada.</para>
    /// </summary>
    private void AplicarSituacao(
        Dominio.Processo.Processo entidade, ProcessoParaCarga processo, int motivoDePerdaId)
    {
        var encerra = processo.Situacao is not (SituacaoDoProcesso.Aberto or SituacaoDoProcesso.Suspenso);

        entidade.RegistrarSituacao(
            processo.Situacao,
            processo.SituacaoDesde,
            usuarioResponsavelId,
            processo.Situacao is SituacaoDoProcesso.Perdido ? motivoDePerdaId : null,
            encerra ? processo.ConcluidoEm ?? processo.SituacaoDesde : null);

        if (processo.Situacao is SituacaoDoProcesso.Perdido)
            Decidir("Processos perdidos sem motivo de perda na origem", 1);
    }

    // =============================================================================================
    // Tarefa
    // =============================================================================================

    private async Task<Dictionary<string, long>> GravarTarefasAsync(
        int sistemaId,
        IReadOnlyList<TarefaParaCarga> tarefas,
        IReadOnlyDictionary<string, long> porChaveDeCliente,
        IReadOnlyDictionary<string, long> porChaveDeProcesso,
        IReadOnlyDictionary<string, int> porChaveDeTipoDeTarefa,
        IReadOnlyDictionary<string, int> porChaveDeResultado,
        IReadOnlyDictionary<string, long> porChaveDeUsuario,
        IReadOnlyDictionary<long, int> empresaPorCliente,
        int tipoDeTarefaPadraoId,
        CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(Tarefa), ct);
        var gravados = 0;
        var semPrazo = 0;
        var semTipo = 0;

        foreach (var bloco in tarefas.Chunk(TamanhoDoBloco))
        {
            await using var contexto = abrirContexto();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novos = new List<(string Chave, Tarefa Entidade)>();
            var reconciliadas = new List<string>();

            foreach (var tarefa in bloco)
            {
                if (tarefa.PrazoLimite is null) semPrazo++;

                if (mapa.TryGetValue(tarefa.ChaveDeOrigem, out var existenteId))
                {
                    // A AGENDA É A TABELA QUE MAIS MUDA ENTRE DUAS EXECUÇÕES: a tarefa de ontem
                    // foi concluída, ou foi empurrada para a semana que vem. Pular a linha só
                    // porque ela já existe deixaria a agenda migrada envelhecer em silêncio — e
                    // "rodei de novo e nada mudou" é o defeito, não o objetivo.
                    var entidade = await contexto.Tarefas.FirstOrDefaultAsync(t => t.Id == existenteId, ct);

                    if (entidade is { EstaExcluido: false })
                    {
                        entidade.Reprogramar(tarefa.AgendadaPara, tarefa.PrazoLimite, usuarioResponsavelId);

                        if (entidade.Situacao is not SituacaoDaTarefa.Concluida)
                            ConcluirSePossivel(entidade, tarefa, porChaveDeResultado, porChaveDeUsuario);

                        reconciliadas.Add(tarefa.ChaveDeOrigem);
                    }

                    continue;
                }

                if (!porChaveDeUsuario.TryGetValue(tarefa.ChaveDoResponsavelDeOrigem, out var responsavelId))
                {
                    Recusar(nameof(Tarefa), "Responsável fora do cadastro de usuários",
                        tarefa.ChaveDeOrigem,
                        "O dono desta tarefa não existe no cadastro de usuários do sistema de " +
                        "origem. A tarefa entraria sem ninguém para executá-la.", tarefa);
                    continue;
                }

                long? clienteId = tarefa.ChaveDoClienteDeOrigem is { } chaveDoCliente
                                  && porChaveDeCliente.TryGetValue(chaveDoCliente, out var achadoCliente)
                    ? achadoCliente
                    : null;

                long? processoId = tarefa.ChaveDoProcessoDeOrigem is { } chaveDoProcesso
                                   && porChaveDeProcesso.TryGetValue(chaveDoProcesso, out var achadoProcesso)
                    ? achadoProcesso
                    : null;

                if (clienteId is null && processoId is null)
                {
                    Recusar(nameof(Tarefa), "Tarefa sem cliente e sem processo no CRM",
                        tarefa.ChaveDeOrigem,
                        "Nem o cliente nem o processo desta tarefa entraram na carga. Ela apareceria " +
                        "na agenda sem dizer sobre o que é.", tarefa);
                    continue;
                }

                var empresaId = clienteId is { } dono && empresaPorCliente.TryGetValue(dono, out var daEmpresa)
                    ? daEmpresa
                    : deParaDeFiliais.TryGetValue(tarefa.CodigoDaFilialNoLegado, out var daFilial)
                        ? daFilial
                        : 0;

                if (empresaId == 0) continue;

                if (!porChaveDeTipoDeTarefa.TryGetValue(tarefa.ChaveDoTipoDeTarefaDeOrigem, out var tipoId))
                {
                    tipoId = tipoDeTarefaPadraoId;
                    semTipo++;
                }

                var nova = Tarefa.Agendar(
                    empresaId, tipoId, tarefa.Assunto, responsavelId, tarefa.AgendadaPara,
                    tarefa.OrigemAtribuicao, usuarioResponsavelId,
                    processoId, clienteId, contatoId: null, tarefa.Detalhe,
                    tarefa.PrazoLimite, tarefa.Prioridade, tarefa.CriadaEm);

                ConcluirSePossivel(nova, tarefa, porChaveDeResultado, porChaveDeUsuario);

                contexto.Tarefas.Add(nova);
                novos.Add((tarefa.ChaveDeOrigem, nova));
            }

            await contexto.SaveChangesAsync(ct);

            foreach (var (chave, entidade) in novos)
            {
                contexto.ChavesExternas.Add(ChaveExterna.Criar(sistemaId, nameof(Tarefa), entidade.Id, chave));
                mapa[chave] = entidade.Id;
            }

            await CarimbarConciliacaoAsync(contexto, sistemaId, nameof(Tarefa), reconciliadas, ct);
            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);

            gravados += novos.Count + reconciliadas.Count;
            relatar($"  tarefas: {mapa.Count}/{tarefas.Count}");
        }

        Decidir("Tarefas sem prazo limite (a origem não declara prazo nem na ação nem na agenda)", semPrazo);
        Decidir("Tarefas cuja ação a origem não declara (entraram no tipo 'Não informada')", semTipo);

        return mapa;
    }

    /// <summary>
    /// Conclui a tarefa quando — e só quando — a origem traz as três coisas que a conclusão
    /// exige: a data, o desfecho e quem concluiu.
    ///
    /// <para>Faltando qualquer uma delas a tarefa fica pendente. É o oposto do que o legado faz:
    /// lá a agenda é marcada como realizada e as três colunas ficam vazias, e o resultado é uma
    /// lista que ninguém consegue auditar.</para>
    /// </summary>
    private static void ConcluirSePossivel(
        Tarefa entidade,
        TarefaParaCarga tarefa,
        IReadOnlyDictionary<string, int> porChaveDeResultado,
        IReadOnlyDictionary<string, long> porChaveDeUsuario)
    {
        if (tarefa.Situacao is not SituacaoDaTarefa.Concluida) return;

        if (tarefa.ChaveDoResultadoDeOrigem is not { } chaveDoDesfecho
            || !porChaveDeResultado.TryGetValue(chaveDoDesfecho, out var desfechoId)) return;

        if (tarefa.ChaveDeQuemConcluiuDeOrigem is not { } chaveDeQuemConcluiu
            || !porChaveDeUsuario.TryGetValue(chaveDeQuemConcluiu, out var concluiuId)) return;

        entidade.Concluir(concluiuId, desfechoId, interacaoConclusaoId: null, tarefa.ConcluidaEm);
    }

    // =============================================================================================
    // Interação
    // =============================================================================================

    private async Task<int> GravarInteracoesAsync(
        int sistemaId,
        IReadOnlyList<InteracaoParaCarga> interacoes,
        IReadOnlyDictionary<string, long> porChaveDeCliente,
        IReadOnlyDictionary<string, long> porChaveDeProcesso,
        IReadOnlyDictionary<string, long> porChaveDeTarefa,
        IReadOnlyDictionary<string, int> porChaveDeTipoDeTarefa,
        IReadOnlyDictionary<string, int> porChaveDeResultado,
        IReadOnlyDictionary<string, long> porChaveDeUsuario,
        IReadOnlyDictionary<long, int> empresaPorCliente,
        int tipoDeTarefaPadraoId,
        CancellationToken ct)
    {
        var mapa = await MapaDeChavesAsync(sistemaId, nameof(Interacao), ct);
        var gravadas = 0;
        var deSistema = 0;

        foreach (var bloco in interacoes.Chunk(TamanhoDoBloco))
        {
            await using var contexto = abrirContexto();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novas = new List<(string Chave, Interacao Entidade)>();

            foreach (var interacao in bloco)
            {
                if (interacao.Natureza is NaturezaDaInteracao.Sistema) deSistema++;

                // INTERAÇÃO É SOMENTE-ACRESCENTAR. Numa reexecução ela não é atualizada: o fato
                // que aconteceu não muda de forma. O de-para reencontra a linha, e a rodada
                // apenas carimba a conciliação.
                if (mapa.ContainsKey(interacao.ChaveDeOrigem)) continue;

                if (!porChaveDeUsuario.TryGetValue(interacao.ChaveDoAutorDeOrigem, out var autorId))
                {
                    Recusar(nameof(Interacao), "Autor fora do cadastro de usuários",
                        interacao.ChaveDeOrigem,
                        "Quem registrou este andamento não existe no cadastro de usuários do sistema " +
                        "de origem. Uma interação sem autor entraria na linha do tempo do cliente " +
                        "sem ninguém para responder por ela.", interacao);
                    continue;
                }

                long? clienteId = interacao.ChaveDoClienteDeOrigem is { } chaveDoCliente
                                  && porChaveDeCliente.TryGetValue(chaveDoCliente, out var achadoCliente)
                    ? achadoCliente
                    : null;

                long? processoId = interacao.ChaveDoProcessoDeOrigem is { } chaveDoProcesso
                                   && porChaveDeProcesso.TryGetValue(chaveDoProcesso, out var achadoProcesso)
                    ? achadoProcesso
                    : null;

                if (clienteId is null && processoId is null)
                {
                    Recusar(nameof(Interacao), "Interação sem cliente e sem processo no CRM",
                        interacao.ChaveDeOrigem,
                        "Nem o cliente nem o processo deste andamento entraram na carga. Interação " +
                        "precisa se ligar a alguma coisa — nunca solta.", interacao);
                    continue;
                }

                var empresaId = clienteId is { } dono && empresaPorCliente.TryGetValue(dono, out var daEmpresa)
                    ? daEmpresa
                    : deParaDeFiliais.TryGetValue(interacao.CodigoDaFilialNoLegado, out var daFilial)
                        ? daFilial
                        : 0;

                if (empresaId == 0) continue;

                var tipoId = porChaveDeTipoDeTarefa.TryGetValue(
                    interacao.ChaveDoTipoDeTarefaDeOrigem, out var achadoTipo)
                    ? achadoTipo
                    : tipoDeTarefaPadraoId;

                int? resultadoId = interacao.ChaveDoResultadoDeOrigem is { } chaveDoDesfecho
                                   && porChaveDeResultado.TryGetValue(chaveDoDesfecho, out var achadoDesfecho)
                    ? achadoDesfecho
                    : null;

                long? tarefaId = interacao.ChaveDaTarefaDeOrigem is { } chaveDaTarefa
                                 && porChaveDeTarefa.TryGetValue(chaveDaTarefa, out var achadaTarefa)
                    ? achadaTarefa
                    : null;

                var nova = Interacao.Registrar(
                    empresaId, tipoId, interacao.Assunto,
                    DataHoraUtc.Criar(interacao.OcorridaEm), interacao.Natureza, autorId,
                    clienteId, processoId, leadId: null, contatoId: null, tarefaId, resultadoId,
                    interacao.ResultadoComplemento, interacao.Detalhe, interacao.DuracaoMinutos,
                    interacao.Latitude, interacao.Longitude);

                contexto.Interacoes.Add(nova);
                novas.Add((interacao.ChaveDeOrigem, nova));
            }

            await contexto.SaveChangesAsync(ct);

            foreach (var (chave, entidade) in novas)
            {
                contexto.ChavesExternas.Add(
                    ChaveExterna.Criar(sistemaId, nameof(Interacao), entidade.Id, chave));
                mapa[chave] = entidade.Id;
            }

            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);

            gravadas += novas.Count;
            relatar($"  interações: {mapa.Count}/{interacoes.Count}");
        }

        Decidir("Interações escritas pelo servidor da origem, não por gente", deSistema);

        return mapa.Count;
    }

    // =============================================================================================
    // O que só se sabe depois de tudo estar gravado
    // =============================================================================================

    /// <summary>
    /// Fecha o duplo ponteiro e calcula a data do último contato — as duas coisas que dependem de
    /// tarefa e interação já existirem.
    ///
    /// <para><b>É conjunto, e não linha a linha, de propósito.</b> São 124 mil interações contra
    /// 104 mil tarefas e 57 mil vínculos de carteira: percorrer isso pelo rastreador de mudanças
    /// levaria muito mais tempo do que a carga inteira, e o resultado seria o mesmo. Estas são as
    /// únicas duas escritas em conjunto da carga, e as duas gravam no NOSSO banco.</para>
    ///
    /// <para><b>Por que a data do último contato não é copiada da origem:</b> lá ela só é
    /// preenchida para desfechos marcados numa tabela de configuração que quatro departamentos
    /// nunca povoaram — 31.556 clientes carteirizados aparecem como "nunca contatados" por
    /// construção, e a procedure que a atualizava não roda desde agosto de 2025. A nossa nasce
    /// das interações que efetivamente entraram, e por isso ela é verificável.</para>
    /// </summary>
    private async Task<(int TarefasLigadas, int VinculosComContato)> ReconciliarDerivadosAsync(
        CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var tarefas = await contexto.Database.ExecuteSqlRawAsync(
            """
            UPDATE t
               SET t.InteracaoConclusaoId = u.Ultima
              FROM processo.Tarefa t
              JOIN (SELECT TarefaId, MAX(Id) AS Ultima
                      FROM processo.Interacao
                     WHERE TarefaId IS NOT NULL
                     GROUP BY TarefaId) u ON u.TarefaId = t.Id
             WHERE t.Situacao = 'Concluida'
               AND t.InteracaoConclusaoId IS NULL
            """, ct);

        var carteiras = await contexto.Database.ExecuteSqlRawAsync(
            """
            UPDATE cc
               SET cc.UltimaInteracaoEm = u.Ultima
              FROM comercial.ClienteCarteira cc
              JOIN (SELECT ClienteId, MAX(OcorridaEm) AS Ultima
                      FROM processo.Interacao
                     WHERE ClienteId IS NOT NULL
                     GROUP BY ClienteId) u ON u.ClienteId = cc.ClienteId
             WHERE cc.DesvinculadoEm IS NULL
               AND (cc.UltimaInteracaoEm IS NULL OR cc.UltimaInteracaoEm < u.Ultima)
            """, ct);

        return (tarefas, carteiras);
    }

    // =============================================================================================
    // Catálogo mínimo que a carga precisa garantir
    // =============================================================================================

    private async Task<int> GarantirLinhaDeNegocioPadraoAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var existente = await contexto.LinhasDeNegocio.FirstOrDefaultAsync(l => l.Codigo == "NAO_INFORMADA", ct);
        if (existente is not null) return existente.Id;

        var nova = LinhaDeNegocio.Criar("NAO_INFORMADA", "Não informada na origem", ordem: 900);
        contexto.LinhasDeNegocio.Add(nova);
        await contexto.SaveChangesAsync(ct);

        return nova.Id;
    }

    private async Task<int> GarantirTipoDeTarefaPadraoAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var existente = await contexto.TiposDeTarefa
            .FirstOrDefaultAsync(t => t.Codigo == TipoDeTarefaNaoInformada, ct);

        if (existente is not null) return existente.Id;

        var novo = TipoTarefa.Criar(
            TipoDeTarefaNaoInformada, "Não informada na origem", CategoriaDeInteracao.Interna,
            prazoDiasUteis: 0);

        contexto.TiposDeTarefa.Add(novo);
        await contexto.SaveChangesAsync(ct);

        return novo.Id;
    }

    private async Task<int> GarantirMotivoDePerdaAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var existente = await contexto.MotivosDePerda
            .FirstOrDefaultAsync(m => m.Codigo == MotivoDePerdaNaoInformado, ct);

        if (existente is not null) return existente.Id;

        var novo = MotivoDePerda.Criar(
            MotivoDePerdaNaoInformado, "Não informado na origem", CategoriaDeMotivoDePerda.Outro);

        contexto.MotivosDePerda.Add(novo);
        await contexto.SaveChangesAsync(ct);

        return novo.Id;
    }

    private async Task<int> GarantirSistemaAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var sistema = await contexto.Sistemas
            .FirstOrDefaultAsync(s => s.Codigo == LeitorDeCargaDoVortice.CodigoDoSistema, ct);

        if (sistema is not null) return sistema.Id;

        sistema = Sistema.Criar(
            LeitorDeCargaDoVortice.CodigoDoSistema, "Vórtice CRM (sistema legado)",
            "SQL Server, somente leitura");

        contexto.Sistemas.Add(sistema);
        await contexto.SaveChangesAsync(ct);

        return sistema.Id;
    }

    // =============================================================================================
    // Apoio: de-para, fila de descarte e marca de sincronismo
    // =============================================================================================

    /// <summary>
    /// Copia a cadência da origem para a linha, e diz se havia alguma para copiar.
    ///
    /// <para>Uma linha sem nenhum dos quatro dias não é erro: dezoito dos vinte e nove
    /// departamentos do legado não declaram cadência nenhuma. O que não se pode é inventar um
    /// número para elas — sem cadência declarada, "fora do ciclo" não existe.</para>
    /// </summary>
    private static bool DeclararCadencia(LinhaDeNegocio linha, LinhaDeNegocioParaCarga daOrigem)
    {
        linha.DeclararCadencia(
            daOrigem.DiasCicloClasseA, daOrigem.DiasCicloClasseB,
            daOrigem.DiasCicloClasseC, daOrigem.DiasCicloClasseD);

        return daOrigem.DiasCicloClasseA is not null || daOrigem.DiasCicloClasseB is not null
            || daOrigem.DiasCicloClasseC is not null || daOrigem.DiasCicloClasseD is not null;
    }

    /// <summary>O nome de cada linha de negócio já gravada, por identificador.</summary>
    private async Task<Dictionary<int, string>> NomeDasLinhasDeNegocioAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        return await contexto.LinhasDeNegocio.AsNoTracking()
            .ToDictionaryAsync(l => l.Id, l => l.Nome, ct);
    }

    /// <summary>O nome de exibição de cada usuário já gravado, por identificador.</summary>
    private async Task<Dictionary<long, string>> NomeDosUsuariosAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        return await contexto.Usuarios.AsNoTracking()
            .ToDictionaryAsync(u => u.Id, u => u.NomeExibicao, ct);
    }

    private async Task<Dictionary<string, long>> MapaDeChavesAsync(
        int sistemaId, string entidade, CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        return await contexto.ChavesExternas.AsNoTracking()
            .Where(c => c.SistemaId == sistemaId && c.Entidade == entidade)
            .ToDictionaryAsync(c => c.ChaveOrigem, c => c.RegistroId, StringComparer.Ordinal, ct);
    }

    private async Task<Dictionary<long, int>> MapaDeEmpresaPorClienteAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        return await contexto.Clientes.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.EmpresaId, ct);
    }

    private static async Task CarimbarConciliacaoAsync(
        CrmDbContext contexto, int sistemaId, string entidade, List<string> chaves, CancellationToken ct)
    {
        if (chaves.Count == 0) return;

        var linhas = await contexto.ChavesExternas
            .Where(c => c.SistemaId == sistemaId && c.Entidade == entidade && chaves.Contains(c.ChaveOrigem))
            .ToListAsync(ct);

        foreach (var linha in linhas) linha.MarcarSincronismo(DateTime.UtcNow);
    }

    private async Task GravarRecusasAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        await contexto.Database.ExecuteSqlRawAsync(
            "DELETE FROM integracao.MensagemDescartada WHERE Fluxo LIKE 'VORTICE.CARGA.%' " +
            "AND Fluxo NOT IN ('VORTICE.CARGA.CLIENTE','VORTICE.CARGA.ENDERECO'," +
            "'VORTICE.CARGA.CONTATO','VORTICE.CARGA.EQUIPAMENTO') AND TratadaEm IS NULL", ct);

        foreach (var bloco in _recusas.Chunk(TamanhoDoBloco))
        {
            foreach (var recusa in bloco)
                contexto.MensagensDescartadas.Add(MensagemDescartada.Criar(
                    Fluxo(recusa.Entidade), recusa.ConteudoJson, recusa.Motivo, tentativas: 1));

            await contexto.SaveChangesAsync(ct);
        }

        relatar($"  fila de descarte: {_recusas.Count} linha(s) recusadas, com motivo e conteúdo cru.");
    }

    private static string Fluxo(string entidade) => entidade switch
    {
        nameof(Usuario) => FluxoDeUsuario,
        nameof(Carteira) => FluxoDeCarteira,
        nameof(ClienteCarteira) => FluxoDeVinculo,
        nameof(CarteiraMunicipio) => FluxoDeTerritorio,
        nameof(Tarefa) => FluxoDeTarefa,
        nameof(Interacao) => FluxoDeInteracao,
        nameof(Dominio.Processo.Processo) => FluxoDeProcesso,
        _ => FluxoDeCatalogo
    };

    private async Task MarcarSincronismoAsync<T>(
        int sistemaId, string fluxo, LoteDaCarga<T> lote, int gravados, CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var ponto = await contexto.PontosDeSincronismo.FirstOrDefaultAsync(p => p.Fluxo == fluxo, ct);

        var ateOnde = lote.MaisRecenteNaOrigem?.ToString("O", CultureInfo.InvariantCulture)
                      ?? DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        if (ponto is null)
        {
            ponto = PontoDeSincronismo.Criar(sistemaId, fluxo, ateOnde);
            contexto.PontosDeSincronismo.Add(ponto);
        }

        ponto.RegistrarRodada(ateOnde, lote.LinhasLidas, gravados, lote.Recusadas.Count, DateTime.UtcNow);
        await contexto.SaveChangesAsync(ct);
    }

    // =============================================================================================
    // Contabilidade
    // =============================================================================================

    private void Contabilizar<T>(
        LoteDaCarga<T> lote,
        Func<T, (IReadOnlyList<CorrecaoAplicada> Correcoes, IReadOnlyList<CampoRejeitado> Rejeicoes)> trilha)
    {
        foreach (var linha in lote.Aceitos)
        {
            var (correcoes, rejeicoes) = trilha(linha);

            foreach (var correcao in correcoes)
                Acumular(correcao.Campo, corrigido: true, correcao.ValorOriginal, correcao.ValorNormalizado);

            foreach (var rejeicao in rejeicoes)
                Acumular(rejeicao.Campo, corrigido: false, rejeicao.ValorOriginal, null);
        }
    }

    private void Acumular(string campo, bool corrigido, string? original, string? resultado)
    {
        if (!_saneamento.TryGetValue(campo, out var contagem))
            _saneamento[campo] = contagem = new ContagemDeSaneamento();

        if (corrigido)
        {
            contagem.Corrigidos++;
            if (contagem.ExemploDeCorrecao is null && original is not null && resultado is not null)
                contagem.ExemploDeCorrecao = $"{Resumir(original)} → {Resumir(resultado)}";
        }
        else
        {
            contagem.Recusados++;
            contagem.ExemploDeRecusa ??= Resumir(original ?? "(vazio)");
        }
    }

    private void Decidir(string decisao, int quantas) =>
        _decisoes[decisao] = _decisoes.TryGetValue(decisao, out var atual) ? atual + quantas : quantas;

    private void Recusar(string entidade, string categoria, string chave, string motivo, object linha) =>
        _recusas.Add(new LinhaRecusada(entidade, categoria, chave, motivo, JsonSerializer.Serialize(new
        {
            chaveDeOrigem = chave,
            resumo = linha.ToString()
        })));

    private static string Resumir(string texto)
    {
        var numaLinha = string.Join(' ', texto.Split(
            (char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return numaLinha.Length <= 90 ? numaLinha : numaLinha[..90] + "…";
    }

    private static string NomeCurto(string nomeCompleto)
    {
        var curto = nomeCompleto.Trim();
        return curto.Length <= 80 ? curto : curto[..80].TrimEnd();
    }

    /// <summary>
    /// Um GUID DETERMINÍSTICO a partir de um texto — mesma entrada, mesmo identificador, sempre.
    ///
    /// <para>É o que faz a coluna de identidade externa poder existir antes do Entra ID: um GUID
    /// sorteado a cada execução quebraria a reexecução da carga no índice único. O resumo é MD5
    /// e não é criptografia — é só uma função estável de 128 bits, que é o tamanho de um GUID.
    /// Nada aqui protege nada, e por isso não há requisito de resistência a colisão adversária.
    /// </para>
    /// </summary>
    private static Guid GuidEstavelDe(string texto) => new(MD5.HashData(Encoding.UTF8.GetBytes(texto)));
}

/// <summary>O que a carga de relacionamento fez, em número — é o corpo do relatório do documento 25.</summary>
/// <param name="UsuariosLidos">Linhas de usuário lidas na origem.</param>
/// <param name="UsuariosGravados">Usuários no de-para.</param>
/// <param name="LinhasDeNegocioGravadas">Linhas de negócio nascidas dos departamentos com gente.</param>
/// <param name="CarteirasLidas">Linhas de carteira lidas.</param>
/// <param name="CarteirasGravadas">Carteiras no de-para.</param>
/// <param name="MunicipiosDeCarteiraLidos">Linhas de territorio lidas na origem.</param>
/// <param name="MunicipiosDeCarteiraGravados">Vinculos carteira x municipio vigentes.</param>
/// <param name="CarteirasComMunicipio">Quantas carteiras ficaram com pelo menos uma cidade.</param>
/// <param name="VinculosLidos">Linhas de carteirização lidas.</param>
/// <param name="VinculosGravados">Vínculos cliente × carteira gravados.</param>
/// <param name="TiposDeProcessoGravados">Modelos de fluxo nascidos do dado.</param>
/// <param name="FasesGravadas">Fases nascidas de onde os processos estão.</param>
/// <param name="TiposDeTarefaGravados">Tipos de tarefa nascidos das ações em uso.</param>
/// <param name="ResultadosGravados">Desfechos nascidos do que foi lançado.</param>
/// <param name="ProcessosLidos">Linhas de processo lidas.</param>
/// <param name="ProcessosGravados">Processos no de-para.</param>
/// <param name="TarefasLidas">Linhas de agenda lidas.</param>
/// <param name="TarefasGravadas">Tarefas no de-para.</param>
/// <param name="InteracoesLidas">Linhas de histórico lidas.</param>
/// <param name="InteracoesGravadas">Interações no de-para.</param>
/// <param name="TarefasLigadasAInteracao">Tarefas com o duplo ponteiro fechado.</param>
/// <param name="VinculosComUltimoContato">Vínculos de carteira com data de último contato calculada.</param>
/// <param name="VendasPerdidasLidas">Respostas de formulário de venda perdida lidas na origem.</param>
/// <param name="VendasPerdidasGravadas">Vendas perdidas gravadas, com motivo e concorrente.</param>
/// <param name="MesesDeFaturamentoGravados">Linhas de faturamento por cliente, filial e mês.</param>
/// <param name="ClientesPorClasse">Quantos clientes ficaram em cada classe da curva ABC.</param>
/// <param name="Recusadas">Total de linhas na fila de descarte.</param>
/// <param name="Saneamento">Contagem de correção e recusa por campo.</param>
/// <param name="RecusasPorMotivo">Contagem de recusa por regra que recusou.</param>
/// <param name="ExemplosDeRecusa">Um motivo real por regra.</param>
/// <param name="Decisoes">As decisões que a carga teve de tomar, com quantas linhas cada uma alcançou.</param>
internal sealed record ResumoDoRelacionamento(
    int UsuariosLidos,
    int UsuariosGravados,
    int LinhasDeNegocioGravadas,
    int CarteirasLidas,
    int CarteirasGravadas,
    int MunicipiosDeCarteiraLidos,
    int MunicipiosDeCarteiraGravados,
    int CarteirasComMunicipio,
    int VinculosLidos,
    int VinculosGravados,
    int TiposDeProcessoGravados,
    int FasesGravadas,
    int TiposDeTarefaGravados,
    int ResultadosGravados,
    int ProcessosLidos,
    int ProcessosGravados,
    int TarefasLidas,
    int TarefasGravadas,
    int InteracoesLidas,
    int InteracoesGravadas,
    int TarefasLigadasAInteracao,
    int VinculosComUltimoContato,
    int VendasPerdidasLidas,
    int VendasPerdidasGravadas,
    int MesesDeFaturamentoGravados,
    IReadOnlyDictionary<string, int> ClientesPorClasse,
    int Recusadas,
    IReadOnlyDictionary<string, ContagemDeSaneamento> Saneamento,
    IReadOnlyDictionary<string, int> RecusasPorMotivo,
    IReadOnlyDictionary<string, string> ExemplosDeRecusa,
    IReadOnlyDictionary<string, int> Decisoes);
