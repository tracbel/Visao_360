using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga do ART fez, em número — sem nome, documento ou valor.</summary>
/// <param name="Simulada">Verdadeiro quando tudo foi desfeito ao final.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Unidades">O mapeamento das unidades para as filiais.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDaCargaDoArt(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<(string Unidade, string Filial, string Situacao, int Ocorrencias, string Criterio)> Unidades,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo, em qualquer etapa.</summary>
    /// <param name="rotulo">Um dos rótulos públicos de <see cref="CargaDoArt"/>.</param>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// A CARGA DAS VENDAS DE MÁQUINA DO ART (documento 35, seção 10).
///
/// <para><b>Máquina, venda e vínculo são três coisas.</b> A máquina é o chassi
/// (<c>frota.Equipamento</c>); a venda é o evento com data, filial e comprador
/// (<c>frota.VendaDeMaquina</c>); o vínculo diz que aquele cliente foi COMPRADOR NAQUELA VENDA
/// (<c>frota.VinculoDeClienteComEquipamento</c>). O dono atual da máquina não é tocado.</para>
///
/// <para><b>O que entra:</b> o registro com chassi completo e válido, comprador identificado sem
/// ambiguidade entre os clientes do CRM e unidade com filial correspondente. Todo o resto fica em
/// <c>integracao.RegistroDeOrigem</c> como pendente, com os motivos.</para>
///
/// <para><b>Repetível.</b> A venda é reencontrada pelo código do ART, a máquina pelo chassi, o vínculo
/// pela venda. Conteúdo igual não regrava; conteúdo diferente atualiza a venda e deixa a trilha em
/// <c>auditoria.AlteracaoDeCampo</c>; o que uma pessoa corrigiu (modelo, classificação, dono,
/// correspondência revisada, divergência resolvida) não é desfeito.</para>
///
/// <para><b>Simulação:</b> a carga inteira roda numa transação, e na simulação a transação é
/// desfeita no fim. Os números da simulação são, por construção, os da carga real. A
/// <b>projeção</b> (<see cref="ProjetarAsync"/>) é a irmã que não abre transação nenhuma: só a
/// decisão de cada registro, com a mesma função da carga — é a que roda contra a produção.</para>
///
/// <para><b>O Protheus decide três coisas desde 24/09/2026</b> (decisões 1, 2 e 5 do dono):
/// o número de série curto que o cadastro de veículos tem exatamente vira identidade da máquina; o
/// comprador ausente do CRM dá lugar ao dono atual da máquina quando ESTE é cliente; e a divergência
/// entre o dono no Protheus e o comprador do ART só fica registrada quando o ART prevalece — sem
/// evidência posterior no Protheus, ou com o Protheus apontando a própria Tracbel
/// (<see cref="RegrasDoParque.Comparar"/>). O VIN com I, O e Q entra pela regra do chassi.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="lerArt">A leitura do ART.</param>
/// <param name="lerProtheus">A leitura do cadastro do Protheus, quando configurada.</param>
/// <param name="raizesDoGrupo">As raízes de CNPJ das empresas do grupo.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDoArt(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<IReadOnlyList<RegistroDoArt>>>> lerArt,
    Func<IReadOnlySet<string>, CancellationToken, Task<Resultado<(ParqueNoProtheus Parque, IReadOnlyDictionary<string, CadastroNoProtheus> Cadastros)>>>? lerProtheus,
    IReadOnlySet<string> raizesDoGrupo,
    long usuarioId,
    Action<string> relatar)
{
    private const string Fluxo = "ART.VENDA_DE_MAQUINA";

    /// <summary>Rótulo: registros lidos da view.</summary>
    internal const string RotuloDeLidos = "registros (vendas) lidos da view";

    /// <summary>Rótulo: registros pendentes.</summary>
    internal const string RotuloDePendentes = "registros pendentes (ao menos um motivo)";

    /// <summary>Rótulo: máquinas incluídas.</summary>
    internal const string RotuloDeMaquinasIncluidas = "máquinas incluídas (chassi novo no CRM, sem dono atual)";

    /// <summary>Rótulo: vendas incluídas.</summary>
    internal const string RotuloDeVendasIncluidas = "vendas incluídas";

    /// <summary>Rótulo: vendas atualizadas pela origem.</summary>
    internal const string RotuloDeVendasAtualizadas = "vendas já importadas e atualizadas pela origem";

    /// <summary>Rótulo: chassis curtos confirmados pelo Protheus.</summary>
    internal const string RotuloDeSeriesConfirmadas = "chassi curto confirmado no cadastro de veículos do Protheus (vira a identidade da máquina)";

    /// <summary>Rótulo: compradores ausentes resolvidos pelo dono atual no Protheus.</summary>
    internal const string RotuloDeCompradoresPeloProtheus = "comprador ausente do CRM: a venda entra com o dono atual no Protheus, que é cliente";

    /// <summary>As divergências que descrevem um ESTADO — e que, por isso, deixam de ocorrer sozinhas.</summary>
    private static readonly HashSet<TipoDeDivergencia> DivergenciasDeEstado =
    [
        TipoDeDivergencia.CompradorDiferenteDoProprietarioNoCrm,
        TipoDeDivergencia.ProprietarioNoProtheusDiferenteDoComprador,
        TipoDeDivergencia.RegistroAusenteNaOrigem
    ];

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    private sealed record Importavel(
        VendaDoArtSaneada Venda, RegistroDeOrigem Registro, int EmpresaId, int? EmpresaDoFaturamentoId, long CompradorId, bool CompradorPeloProtheus);

    /// <summary>
    /// O que se decidiu sobre um registro do ART — a MESMA decisão na carga e na projeção.
    /// </summary>
    /// <param name="Venda">O registro, com a transformação do comprador anotada quando houve.</param>
    /// <param name="CompradorId">O cliente comprador, quando decidido.</param>
    /// <param name="CompradorPeloProtheus">Se o comprador é o dono atual no Protheus no lugar do comprador do ART.</param>
    /// <param name="Motivos">Os motivos de pendência; vazio quando o registro entra.</param>
    /// <param name="CompradorAusente">Se o comprador do ART não é cliente do CRM e nada o substituiu.</param>
    internal sealed record DecisaoDoRegistro(
        VendaDoArtSaneada Venda, long? CompradorId, bool CompradorPeloProtheus, IReadOnlyList<string> Motivos, bool CompradorAusente);

    /// <summary>
    /// DECIDE UM REGISTRO — função pura, sem banco: a carga e a projeção chamam esta mesma.
    ///
    /// <para><b>O comprador ausente do CRM dá lugar ao dono atual no Protheus</b> (decisão 5 de 24/09/2026) quando a
    /// máquina está no cadastro de veículos com um dono só, esse dono não é empresa do grupo e é cliente do CRM sem
    /// ambiguidade. O comprador do ART continua no registro e na fila de compradores enquanto houver venda dele
    /// que não se resolveu assim.</para>
    /// </summary>
    /// <param name="s">O registro saneado (e já confirmado pelo Protheus, quando o chassi era curto).</param>
    /// <param name="empresaId">A filial da unidade vendedora, quando a correspondência é utilizável.</param>
    /// <param name="clientesPorDocumento">Os clientes do CRM pelo documento.</param>
    /// <param name="chassisAtivos">Os chassis das máquinas ativas no CRM.</param>
    /// <param name="chassisBaixados">Os chassis das máquinas baixadas no CRM.</param>
    /// <param name="parque">O cadastro de veículos do Protheus, quando lido.</param>
    /// <param name="raizesDoGrupo">As raízes de CNPJ do grupo.</param>
    internal static DecisaoDoRegistro Decidir(
        VendaDoArtSaneada s,
        int? empresaId,
        IReadOnlyDictionary<string, List<(long Id, int EmpresaId)>> clientesPorDocumento,
        IReadOnlySet<string> chassisAtivos,
        IReadOnlySet<string> chassisBaixados,
        ParqueNoProtheus? parque,
        IReadOnlySet<string> raizesDoGrupo)
    {
        var motivos = new List<string>(s.Motivos);
        if (empresaId is null) motivos.Add(MotivoDePendenciaDoArt.UnidadeSemFilial);

        long? Unico(List<(long Id, int EmpresaId)> candidatos)
        {
            if (candidatos.Count == 1) return candidatos[0].Id;

            // O MESMO DOCUMENTO EM MAIS DE UMA FILIAL: vale o cliente da filial da venda, quando é um só.
            var naFilial = candidatos.Where(c => c.EmpresaId == empresaId).ToList();
            return naFilial.Count == 1 ? naFilial[0].Id : null;
        }

        long? compradorId = null;
        var peloProtheus = false;
        var ausente = false;
        if (s.Documento is { } documento)
        {
            if (clientesPorDocumento.TryGetValue(documento.Numero, out var candidatos))
            {
                compradorId = Unico(candidatos);
                if (compradorId is null) motivos.Add(MotivoDePendenciaDoArt.CompradorAmbiguo);
            }
            else if (s.Chassi is { } chassi
                     && parque is not null
                     && parque.TentarAchar(chassi.Numero, out var noProtheus)
                     && noProtheus.DocumentoDoDono is { } dono
                     && !RegrasDoParque.EhDoGrupo(dono, raizesDoGrupo)
                     && clientesPorDocumento.TryGetValue(dono, out var donos)
                     && Unico(donos) is { } substituto)
            {
                compradorId = substituto;
                peloProtheus = true;
                s = s with
                {
                    Transformacoes =
                    [
                        .. s.Transformacoes,
                        "comprador: o documento do ART não é cliente do CRM; a venda entrou com o dono atual da máquina no Protheus (VV1), que é"
                    ]
                };
            }
            else
            {
                motivos.Add(MotivoDePendenciaDoArt.CompradorAusente);
                ausente = true;
            }
        }

        if (s.Chassi is { } valido && !chassisAtivos.Contains(valido.Numero) && chassisBaixados.Contains(valido.Numero))
            motivos.Add(MotivoDePendenciaDoArt.MaquinaBaixada);

        var distintos = motivos.Distinct(StringComparer.Ordinal).ToList();
        return new DecisaoDoRegistro(s, distintos.Count == 0 ? compradorId : null, peloProtheus, distintos, ausente);
    }

    /// <summary>Executa a carga.</summary>
    /// <param name="simular">Desfaz tudo ao final.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDaCargaDoArt>> ExecutarAsync(bool simular, CancellationToken ct)
    {
        var lidas = await LerESanearAsync(ct);
        if (!lidas.EhSucesso) return Resultado<RelatorioDaCargaDoArt>.Indisponivel(lidas.Erro!);

        var (vendas, parque, cadastros, protheusLido) = lidas.Valor;

        var agora = DateTime.UtcNow;
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDoArt.CodigoDoSistema, "ART — vendas de máquina", "MySQL, view somente leitura", ct);

        // A TRILHA É AUTOMÁTICA (documento 41, fase 2): o que a leitura do ART muda num equipamento ou
        // numa venda que já existia vai para auditoria.AlteracaoDeCampo no SaveChanges, como integração
        // do ART. O que nasce agora não entra — o rastro dele é integracao.RegistroDeOrigem.
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var linhasDeProduto = await banco.LinhasDeProduto.AsNoTracking().ToDictionaryAsync(l => l.Codigo, StringComparer.Ordinal, ct);
        if (linhasDeProduto.Count == 0)
            return Resultado<RelatorioDaCargaDoArt>.Indisponivel(
                "frota.LinhaDeProduto está vazia. Rode o seed (./scripts/banco/rodar-seed.ps1) antes da carga do ART.");

        foreach (var semFamilia in linhasDeProduto.Values.Where(l => l.FamiliaId is null).OrderBy(l => l.Codigo))
            _observacoes.Add($"Classificação {semFamilia.Codigo} sem família do catálogo apontada (não há família equivalente confirmada).");

        var (linhaPorTexto, produtoPorTexto, unidadePorTexto, unidades) =
            await CorresponderAsync(banco, sistemaId, vendas, linhasDeProduto, agora, ct);

        var clientes = await banco.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm == null)
            .Select(c => new { c.Id, c.EmpresaId, c.ChavePublica, c.Documento })
            .ToListAsync(ct);
        var chaveDoCliente = clientes.ToDictionary(c => c.Id, c => c.ChavePublica);
        var clientesPorDocumento = clientes
            .Where(c => c.Documento is not null)
            .GroupBy(c => c.Documento!.Value.Numero, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(c => (c.Id, c.EmpresaId)).ToList(), StringComparer.Ordinal);

        var equipamentos = await banco.Equipamentos.ToListAsync(ct);
        var ativosPorChassi = equipamentos.Where(e => !e.EstaExcluido).ToDictionary(e => e.Chassi.Numero, StringComparer.Ordinal);
        var baixados = equipamentos.Where(e => e.EstaExcluido).Select(e => e.Chassi.Numero).ToHashSet(StringComparer.Ordinal);

        var registros = await banco.RegistrosDeOrigem.Where(r => r.SistemaId == sistemaId && r.Fluxo == Fluxo)
            .ToDictionaryAsync(r => r.ChaveOrigem, StringComparer.Ordinal, ct);
        var vendasPorChave = await banco.VendasDeMaquina.Where(v => v.SistemaId == sistemaId)
            .ToDictionaryAsync(v => v.ChaveOrigem, StringComparer.Ordinal, ct);
        var vinculoPorVenda = await banco.VinculosComEquipamento
            .Where(v => v.SistemaId == sistemaId && v.EncerradoEm == null && v.VendaDeMaquinaId != null)
            .ToDictionaryAsync(v => v.VendaDeMaquinaId!.Value, ct);
        var divergencias = await banco.DivergenciasDeIntegracao.Where(d => d.SistemaId == sistemaId)
            .ToDictionaryAsync(d => (d.Tipo, d.ChaveOrigem), ct);
        var encontradas = new HashSet<(TipoDeDivergencia, string)>();
        var divergenciasNovas = new Dictionary<TipoDeDivergencia, int>();
        var divergenciasReconfirmadas = new Dictionary<TipoDeDivergencia, int>();

        void Divergir(TipoDeDivergencia tipo, string chave, int empresaId, long? equipamentoId, long? vendaId,
            string descricao, string? noCrm, string? naOrigem, string? noProtheus)
        {
            encontradas.Add((tipo, chave));
            if (divergencias.TryGetValue((tipo, chave), out var existente))
            {
                existente.Reconfirmar(descricao, noCrm, naOrigem, noProtheus, agora, usuarioId);
                divergenciasReconfirmadas[tipo] = divergenciasReconfirmadas.GetValueOrDefault(tipo) + 1;
                return;
            }

            var nova = DivergenciaDeIntegracao.Registrar(
                empresaId, sistemaId, tipo, chave, equipamentoId, vendaId, descricao, noCrm, naOrigem, noProtheus, agora, usuarioId);
            banco.DivergenciasDeIntegracao.Add(nova);
            divergencias[(tipo, chave)] = nova;
            divergenciasNovas[tipo] = divergenciasNovas.GetValueOrDefault(tipo) + 1;
        }

        string Cliente(long id) => $"cliente {chaveDoCliente.GetValueOrDefault(id)}";

        // -----------------------------------------------------------------------------------------
        // 1. Decidir cada registro. A venda mais recente vem primeiro: é ela que dá a filial à máquina nova.
        // -----------------------------------------------------------------------------------------
        const string etapaDaDecisao = "2. Decisão por registro do ART";
        var importaveis = new List<Importavel>();
        var pendentesPorMotivo = new Dictionary<string, int>(StringComparer.Ordinal);
        var semCompradorNoCrm = new List<(VendaDoArtSaneada Venda, int? EmpresaId)>();
        var resolvidosPeloProtheus = new HashSet<string>(StringComparer.Ordinal);
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        var ativos = ativosPorChassi.Keys.ToHashSet(StringComparer.Ordinal);
        int registrosNovos = 0, registrosAlterados = 0, registrosIguais = 0, importadasQueFicaramPendentes = 0, compradoresPeloProtheus = 0;

        foreach (var lida in vendas.OrderByDescending(v => v.VendidaEm).ThenByDescending(v => long.TryParse(v.Codigo, out var n) ? n : 0))
        {
            vistos.Add(lida.Codigo);

            int? empresaId = lida.Unidade is { } u && unidadePorTexto.TryGetValue(u, out var cu) && cu.EhUtilizavel ? cu.EmpresaCorrespondenteId : null;
            int? empresaDoFaturamentoId = lida.UnidadeDoFaturamento is { } uf && unidadePorTexto.TryGetValue(uf, out var cf) && cf.EhUtilizavel
                ? cf.EmpresaCorrespondenteId
                : null;

            var decisao = Decidir(lida, empresaId, clientesPorDocumento, ativos, baixados, protheusLido ? parque : null, raizesDoGrupo);
            var s = decisao.Venda;
            var motivos = decisao.Motivos;

            if (decisao.CompradorAusente) semCompradorNoCrm.Add((s, empresaId));
            if (decisao.CompradorPeloProtheus)
            {
                resolvidosPeloProtheus.Add(s.Documento!.Value.Numero);
                compradoresPeloProtheus++;
            }

            var retrato = new RetratoDoRegistroDeOrigem(
                s.Hash, s.ChassiNaOrigem, Limitar(s.Linha, 60), Limitar(s.Produto, 60), s.Unidade, s.VendidaEm, s.TransformacoesEmTexto);

            if (!registros.TryGetValue(s.Codigo, out var registro))
            {
                registro = RegistroDeOrigem.Registrar(sistemaId, Fluxo, s.Codigo, retrato, agora);
                banco.RegistrosDeOrigem.Add(registro);
                registros[s.Codigo] = registro;
                registrosNovos++;
            }
            else if (registro.RegistrarLeitura(retrato, agora)) registrosAlterados++;
            else registrosIguais++;

            if (motivos.Count > 0)
            {
                if (registro.VendaDeMaquinaId is not null) importadasQueFicaramPendentes++;
                registro.Decidir(DecisaoDaIntegracao.Pendente, string.Join(",", motivos), null);
                foreach (var motivo in motivos)
                    pendentesPorMotivo[motivo] = pendentesPorMotivo.GetValueOrDefault(motivo) + 1;
                continue;
            }

            importaveis.Add(new Importavel(s, registro, empresaId!.Value, empresaDoFaturamentoId, decisao.CompradorId!.Value, decisao.CompradorPeloProtheus));
        }

        Contar(etapaDaDecisao, "registros lidos pela primeira vez", registrosNovos);
        Contar(etapaDaDecisao, "registros já conhecidos com conteúdo alterado na origem", registrosAlterados);
        Contar(etapaDaDecisao, "registros já conhecidos sem alteração", registrosIguais);
        Contar(etapaDaDecisao, RotuloDeCompradoresPeloProtheus, compradoresPeloProtheus);
        Contar(etapaDaDecisao, "registros importáveis (chassi válido + comprador único no CRM + filial)", importaveis.Count);
        Contar(etapaDaDecisao, RotuloDePendentes, vendas.Count - importaveis.Count);
        foreach (var (motivo, quantidade) in pendentesPorMotivo.OrderByDescending(p => p.Value))
            Contar(etapaDaDecisao, $"  pendência por motivo: {motivo}", quantidade);
        Contar(etapaDaDecisao, "vendas já importadas cujo registro ficou pendente nesta leitura (venda mantida)", importadasQueFicaramPendentes);

        // -----------------------------------------------------------------------------------------
        // 2. Máquinas.
        // -----------------------------------------------------------------------------------------
        const string etapaDasMaquinas = "3. Máquinas (chassi)";
        var criadasAgora = new HashSet<string>(StringComparer.Ordinal);
        var jaImportadas = new HashSet<string>(StringComparer.Ordinal);
        var existentesNoCrm = new HashSet<string>(StringComparer.Ordinal);
        var classificacaoRecusada = new HashSet<string>(StringComparer.Ordinal);
        int modelosApontados = 0, classificacoesApontadas = 0;

        var familiaDoModelo = await banco.Modelos.AsNoTracking().ToDictionaryAsync(m => m.Id, m => m.FamiliaId, ct);
        var familiaDaClassificacao = linhasDeProduto.Values.ToDictionary(l => l.Id, l => l.FamiliaId);

        // A classificação da linha da venda só entra quando não contradiz a família do modelo da máquina.
        bool Compativel(int? modelo, int classificacao) =>
            ClassificacaoDoArt.ClassificacaoCompativelComOModelo(
                modelo is { } m && familiaDoModelo.TryGetValue(m, out var familia) ? familia : null,
                familiaDaClassificacao.GetValueOrDefault(classificacao));

        foreach (var i in importaveis)
        {
            var chassi = i.Venda.Chassi!.Value;
            var linha = linhaPorTexto[i.Venda.Linha];
            var produto = produtoPorTexto[i.Venda.Produto];
            int? linhaDeProdutoId = linha.EhUtilizavel ? linha.LinhaDeProdutoId : null;
            int? modeloId = produto.EhUtilizavel ? produto.ModeloId : null;

            if (!ativosPorChassi.TryGetValue(chassi.Numero, out var equipamento))
            {
                if (linhaDeProdutoId is { } proposta && !Compativel(modeloId, proposta))
                {
                    classificacaoRecusada.Add(chassi.Numero);
                    linhaDeProdutoId = null;
                }

                equipamento = Equipamento.RegistrarPelaIntegracao(i.EmpresaId, chassi, OrigemDoEquipamento.Art, usuarioId, modeloId, linhaDeProdutoId);
                banco.Equipamentos.Add(equipamento);
                ativosPorChassi[chassi.Numero] = equipamento;
                criadasAgora.Add(chassi.Numero);
                continue;
            }

            if (!criadasAgora.Contains(chassi.Numero))
                (equipamento.Origem == OrigemDoEquipamento.Art ? jaImportadas : existentesNoCrm).Add(chassi.Numero);

            // SÓ PREENCHE O QUE ESTÁ VAZIO — o modelo e a classificação existentes podem ter sido corrigidos.
            if (modeloId is { } modelo && equipamento.DefinirModeloSeAusente(modelo, usuarioId))
                modelosApontados++;

            if (linhaDeProdutoId is { } recusada && equipamento.LinhaDeProdutoId is null && !Compativel(equipamento.ModeloId, recusada))
                classificacaoRecusada.Add(chassi.Numero);
            else if (linhaDeProdutoId is { } classificacao && equipamento.ClassificarSeAusente(classificacao, usuarioId))
                classificacoesApontadas++;
        }

        await banco.SaveChangesAsync(ct);

        Contar(etapaDasMaquinas, RotuloDeMaquinasIncluidas, criadasAgora.Count);
        Contar(etapaDasMaquinas, "  delas, com modelo do catálogo (correspondência exata do produto)",
            criadasAgora.Count(c => ativosPorChassi[c].ModeloId is not null));
        Contar(etapaDasMaquinas, "  delas, com classificação de produto",
            criadasAgora.Count(c => ativosPorChassi[c].LinhaDeProdutoId is not null));
        Contar(etapaDasMaquinas, "máquinas já existentes no CRM reaproveitadas (dono atual preservado)", existentesNoCrm.Count);
        Contar(etapaDasMaquinas, "máquinas já importadas do ART em carga anterior", jaImportadas.Count);
        Contar(etapaDasMaquinas, "modelos apontados em máquina sem modelo (correspondência exata)", modelosApontados);
        Contar(etapaDasMaquinas, "classificações apontadas em máquina sem classificação", classificacoesApontadas);
        Contar(etapaDasMaquinas, "classificações NÃO aplicadas: a família da classificação contradiz a do modelo", classificacaoRecusada.Count);

        // -----------------------------------------------------------------------------------------
        // 3. Vendas.
        // -----------------------------------------------------------------------------------------
        const string etapaDasVendas = "4. Vendas";
        int vendasNovas = 0, vendasAtualizadas = 0, vendasIguais = 0, camposAuditados = 0, vinculosEncerrados = 0;

        foreach (var i in importaveis)
        {
            var s = i.Venda;
            var equipamento = ativosPorChassi[s.Chassi!.Value.Numero];
            var dados = new DadosDaVendaNaOrigem(
                i.EmpresaId, i.EmpresaDoFaturamentoId, s.VendidaEm, s.FaturadaEm, s.EntregueEm, s.AbertaEm,
                s.NumeroDoPedido, s.NumeroDaNotaFiscal, s.Situacao, s.Gestao, s.VendaDireta, s.RepasseDireto, s.Quantidade,
                s.Linha, s.Produto, s.Empresa, s.Unidade, s.UnidadeDoFaturamento, s.Hash, s.TransformacoesEmTexto);

            if (!vendasPorChave.TryGetValue(s.Codigo, out var venda))
            {
                venda = VendaDeMaquina.Registrar(sistemaId, s.Codigo, equipamento.Id, i.CompradorId, dados, agora, usuarioId, i.CompradorPeloProtheus);
                banco.VendasDeMaquina.Add(venda);
                vendasPorChave[s.Codigo] = venda;
                vendasNovas++;
                continue;
            }

            // A ORIGEM CORRIGIU O CHASSI OU O COMPRADOR: a venda acompanha a origem (é o fato dela), o
            // vínculo antigo é ENCERRADO com o motivo — nunca apagado — e a troca vira divergência para revisão.
            if (venda.EquipamentoId != equipamento.Id)
            {
                Divergir(TipoDeDivergencia.ChassiAlteradoNaOrigem, s.Codigo, i.EmpresaId, equipamento.Id, venda.Id,
                    "O ART trocou o chassi de uma venda já importada. A venda passou para a máquina do chassi novo.",
                    $"máquina {venda.EquipamentoId}", $"máquina {equipamento.Id}", null);
                EncerrarVinculo(venda.Id, "O ART trocou o chassi desta venda.");
                venda.TrocarEquipamento(equipamento.Id, usuarioId);
            }

            // QUEM ESTAVA NO LUGAR DO COMPRADOR ERA O DONO DO PROTHEUS (decisão 5): a troca não é o ART mudando de
            // ideia — é o comprador verdadeiro que passou a existir no CRM, ou o dono no Protheus que mudou. Sem
            // divergência; o vínculo anterior é encerrado com o motivo certo.
            if (venda.CompradorId != i.CompradorId)
            {
                if (!venda.CompradorPeloDonoNoProtheus)
                    Divergir(TipoDeDivergencia.CompradorAlteradoNaOrigem, s.Codigo, i.EmpresaId, equipamento.Id, venda.Id,
                        "O ART trocou o comprador de uma venda já importada. O vínculo do comprador anterior foi encerrado.",
                        Cliente(venda.CompradorId), Cliente(i.CompradorId), null);

                EncerrarVinculo(venda.Id,
                    !venda.CompradorPeloDonoNoProtheus ? "O ART trocou o comprador desta venda."
                    : i.CompradorPeloProtheus ? "O dono atual da máquina no Protheus mudou; o comprador do ART continua fora do CRM."
                    : "O comprador do ART passou a existir no CRM e tomou o lugar do dono atual no Protheus.");
            }

            venda.TrocarComprador(i.CompradorId, usuarioId, i.CompradorPeloProtheus);

            var mudancas = venda.AtualizarDaOrigem(dados, agora, usuarioId);
            if (mudancas.Count == 0)
            {
                vendasIguais++;
                continue;
            }

            // Cada campo mudado vai para a trilha no SaveChanges; aqui só se conta.
            vendasAtualizadas++;
            camposAuditados += mudancas.Count;
        }

        void EncerrarVinculo(long vendaId, string motivo)
        {
            if (!vinculoPorVenda.Remove(vendaId, out var vinculo)) return;
            vinculo.Encerrar(motivo, agora, usuarioId);
            vinculosEncerrados++;
        }

        await banco.SaveChangesAsync(ct);

        Contar(etapaDasVendas, RotuloDeVendasIncluidas, vendasNovas);
        Contar(etapaDasVendas, RotuloDeVendasAtualizadas, vendasAtualizadas);
        Contar(etapaDasVendas, "vendas já importadas sem alteração (nada regravado)", vendasIguais);
        Contar(etapaDasVendas, "campos de venda com a mudança registrada na auditoria", camposAuditados);

        foreach (var registro in importaveis.Select(i => (i.Registro, Venda: vendasPorChave[i.Venda.Codigo])))
            registro.Registro.Decidir(DecisaoDaIntegracao.Importado, null, registro.Venda.Id);

        // -----------------------------------------------------------------------------------------
        // 4. Vínculos — o comprador naquela venda.
        // -----------------------------------------------------------------------------------------
        const string etapaDosVinculos = "5. Vínculos cliente × máquina";
        int vinculosNovos = 0, vinculosMantidos = 0;

        foreach (var venda in importaveis.Select(i => vendasPorChave[i.Venda.Codigo]))
        {
            if (vinculoPorVenda.TryGetValue(venda.Id, out var vinculo))
            {
                vinculo.AcompanharVenda(venda.EmpresaId, venda.VendidaEm, usuarioId);
                vinculosMantidos++;
                continue;
            }

            vinculo = VinculoDeClienteComEquipamento.RegistrarCompradorNaVenda(
                venda.EmpresaId, venda.CompradorId, venda.EquipamentoId, venda.Id, sistemaId, venda.VendidaEm, usuarioId);
            banco.VinculosComEquipamento.Add(vinculo);
            vinculoPorVenda[venda.Id] = vinculo;
            vinculosNovos++;
        }

        Contar(etapaDosVinculos, "vínculos 'comprador na venda' incluídos", vinculosNovos);
        Contar(etapaDosVinculos, "vínculos já existentes mantidos", vinculosMantidos);
        Contar(etapaDosVinculos, "vínculos encerrados porque a origem trocou chassi ou comprador", vinculosEncerrados);

        // -----------------------------------------------------------------------------------------
        // 5. Divergências entre ART, CRM e Protheus — pela venda MAIS RECENTE de cada chassi.
        // -----------------------------------------------------------------------------------------
        const string etapaDasDivergencias = "6. Divergências (registradas para revisão, nada sobrescrito)";
        int noProtheus = 0, donoAmbiguoNoProtheus = 0, compradorEraODonoNoProtheus = 0;
        var desfechos = new Dictionary<DesfechoDaComparacaoComOArt, int>();

        foreach (var grupo in importaveis.GroupBy(i => i.Venda.Chassi!.Value.Numero, StringComparer.Ordinal))
        {
            var maisRecente = grupo.First();
            var venda = vendasPorChave[maisRecente.Venda.Codigo];
            var equipamento = ativosPorChassi[grupo.Key];

            if (equipamento.ClienteId is { } dono && dono != venda.CompradorId)
                Divergir(TipoDeDivergencia.CompradorDiferenteDoProprietarioNoCrm, venda.ChaveOrigem, venda.EmpresaId, equipamento.Id, venda.Id,
                    "O comprador da venda mais recente no ART não é o dono registrado no CRM. O dono não foi alterado.",
                    Cliente(dono), Cliente(venda.CompradorId), null);

            if (!protheusLido) continue;
            if (parque.Ambiguos.Contains(grupo.Key)) { donoAmbiguoNoProtheus++; continue; }
            if (!parque.TentarAchar(grupo.Key, out var noProtheusDaMaquina)) continue;

            noProtheus++;

            // A VENDA QUE ENTROU COM O DONO DO PROTHEUS no lugar do comprador não tem o que comparar: o comprador
            // do ART não está no CRM, e quem está na venda é o próprio dono do Protheus.
            if (maisRecente.CompradorPeloProtheus) { compradorEraODonoNoProtheus++; continue; }

            // A DECISÃO 1 DE 24/09/2026, com a data do FATURAMENTO do ART (D-P08.1).
            var desfecho = RegrasDoParque.Comparar(
                noProtheusDaMaquina, maisRecente.Venda.Documento!.Value.Numero,
                maisRecente.Venda.FaturadaEm ?? maisRecente.Venda.VendidaEm, raizesDoGrupo);
            desfechos[desfecho] = desfechos.GetValueOrDefault(desfecho) + 1;
            if (!RegrasDoParque.RegistraDivergencia(desfecho)) continue;

            var documentoNoProtheus = noProtheusDaMaquina.DocumentoDoDono!;
            var clienteNoProtheus = desfecho == DesfechoDaComparacaoComOArt.ArtPrevalecePorqueOProtheusDizTracbel
                ? "a própria Tracbel (raiz de CNPJ do grupo)"
                : clientesPorDocumento.TryGetValue(documentoNoProtheus, out var doProtheus) && doProtheus.Count == 1
                    ? Cliente(doProtheus[0].Id)
                    : "documento sem cliente único no CRM";

            Divergir(TipoDeDivergencia.ProprietarioNoProtheusDiferenteDoComprador, venda.ChaveOrigem, venda.EmpresaId, equipamento.Id, venda.Id,
                desfecho == DesfechoDaComparacaoComOArt.ArtPrevalecePorqueOProtheusDizTracbel
                    ? "O cadastro de veículos do Protheus (VV1) diz que a dona da máquina é a própria Tracbel — a máquina voltou, " +
                      "ou o cadastro não foi atualizado. Vale o comprador da venda mais recente no ART."
                    : "O dono atual no cadastro de veículos do Protheus (VV1) não é o comprador da venda mais recente no ART, e o " +
                      "Protheus não tem nota de venda nem ordem de serviço dele depois dessa venda. Vale o comprador do ART.",
                equipamento.ClienteId is { } donoNoCrm ? Cliente(donoNoCrm) : null, Cliente(venda.CompradorId), clienteNoProtheus);
        }

        var vendasPorId = vendasPorChave.Values.Where(v => v.Id > 0).ToDictionary(v => v.Id);
        var ausentes = 0;
        foreach (var registro in registros.Values.Where(r => !vistos.Contains(r.ChaveOrigem)))
        {
            ausentes++;
            registro.MarcarAusente(agora);
            if (registro.VendaDeMaquinaId is { } vendaId && vendasPorId.TryGetValue(vendaId, out var vendaAusente))
                Divergir(TipoDeDivergencia.RegistroAusenteNaOrigem, registro.ChaveOrigem, vendaAusente.EmpresaId, vendaAusente.EquipamentoId, vendaId,
                    "Uma venda importada deixou de aparecer na view do ART. A venda e o vínculo foram mantidos.", null, null, null);
        }

        var deixaramDeOcorrer = 0;
        foreach (var ((tipo, chave), divergencia) in divergencias)
        {
            if (!DivergenciasDeEstado.Contains(tipo) || encontradas.Contains((tipo, chave))) continue;
            if (divergencia.Situacao != SituacaoDaDivergencia.Aberta) continue;

            // SEM O PROTHEUS NESTA RODADA, a divergência com ele não foi procurada — não encontrá-la não quer dizer
            // que ela deixou de existir.
            if (tipo == TipoDeDivergencia.ProprietarioNoProtheusDiferenteDoComprador && !protheusLido) continue;

            divergencia.MarcarQueDeixouDeOcorrer(agora, usuarioId);
            deixaramDeOcorrer++;
        }

        foreach (var tipo in Enum.GetValues<TipoDeDivergencia>())
        {
            Contar(etapaDasDivergencias, $"{tipo}: novas", divergenciasNovas.GetValueOrDefault(tipo));
            Contar(etapaDasDivergencias, $"{tipo}: já registradas e encontradas de novo", divergenciasReconfirmadas.GetValueOrDefault(tipo));
        }

        Contar(etapaDasDivergencias, "divergências de estado que deixaram de ocorrer", deixaramDeOcorrer);
        Contar(etapaDasDivergencias, "registros conhecidos que não vieram nesta leitura (marcados ausentes)", ausentes);
        Contar(etapaDasDivergencias, "Protheus lido nesta rodada (1 = sim)", protheusLido ? 1 : 0);
        Contar(etapaDasDivergencias, "chassis importáveis presentes no VV1 do Protheus", noProtheus);
        foreach (var desfecho in Enum.GetValues<DesfechoDaComparacaoComOArt>())
            Contar(etapaDasDivergencias, $"  dono no Protheus × comprador do ART: {desfecho}", desfechos.GetValueOrDefault(desfecho));
        Contar(etapaDasDivergencias, "  venda que entrou com o dono do Protheus no lugar do comprador (nada a comparar)", compradorEraODonoNoProtheus);
        Contar(etapaDasDivergencias, "chassis importáveis repetidos no Protheus com donos diferentes (ambíguos, não comparados)", donoAmbiguoNoProtheus);

        await banco.SaveChangesAsync(ct);

        // -----------------------------------------------------------------------------------------
        // 6. A fila dos compradores ausentes do CRM — nenhum cliente é criado.
        // -----------------------------------------------------------------------------------------
        await AtualizarFilaDeCompradoresAsync(banco, sistemaId, semCompradorNoCrm, clientesPorDocumento.Keys.ToHashSet(StringComparer.Ordinal),
            resolvidosPeloProtheus, cadastros, protheusLido, agora, ct);

        await banco.SaveChangesAsync(ct);

        if (simular)
        {
            await transacao.RollbackAsync(ct);
            _observacoes.Add("SIMULAÇÃO: tudo o que está acima foi executado numa transação e DESFEITO. O banco não mudou.");
        }
        else
        {
            await transacao.CommitAsync(ct);
        }

        return Resultado<RelatorioDaCargaDoArt>.Ok(new RelatorioDaCargaDoArt(simular, _contagens, unidades, _observacoes));
    }

    private void ContarLeitura(List<VendaDoArtSaneada> vendas)
    {
        const string etapa = "1. Leitura do ART";
        Contar(etapa, RotuloDeLidos, vendas.Count);

        foreach (var situacao in Enum.GetValues<SituacaoDoChassiNaOrigem>())
            Contar(etapa, $"chassi {situacao}", vendas.Count(v => v.SituacaoDoChassi == situacao));

        var chaves = vendas.Where(v => v.ChassiNaOrigem is not null)
            .GroupBy(v => v.Chassi?.Numero ?? new string([.. v.ChassiNaOrigem!.Where(c => !char.IsWhiteSpace(c))]).ToUpperInvariant(), StringComparer.Ordinal)
            .ToList();
        var repetidos = chaves.Where(g => g.Count() > 1).ToList();

        Contar(etapa, "chassis distintos (texto sem espaço)", chaves.Count);
        Contar(etapa, "chassis em mais de uma venda", repetidos.Count);
        Contar(etapa, "  linhas desses chassis", repetidos.Sum(g => g.Count()));
        Contar(etapa, "  casos com uma das vendas na linha USADOS (revenda)", repetidos.Count(g => g.Any(v => ClassificacaoDoArt.Codificar(v.Linha) == "USADOS")));
        Contar(etapa, "  casos com o mesmo comprador nas duas vendas", repetidos.Count(g => g.Select(v => v.Documento?.Numero).Distinct().Count() == 1));
        Contar(etapa, "  casos com o mesmo código de venda (duplicata real)", repetidos.Count(g => g.Select(v => v.Codigo).Distinct().Count() < g.Count()));
        Contar(etapa, "documentos de comprador válidos distintos", vendas.Where(v => v.Documento is not null).Select(v => v.Documento!.Value.Numero).Distinct().Count());
        Contar(etapa, "registros com alguma data normalizada (zerada ou inválida → vazia)", vendas.Count(v => v.Transformacoes.Any(t => t.Contains("ficou vazia", StringComparison.Ordinal))));

        foreach (var gestao in vendas.GroupBy(v => v.Gestao ?? "(vazio)").OrderByDescending(g => g.Count()))
            Contar(etapa, $"gestão da venda (preservada como veio): {gestao.Key}", gestao.Count());
    }

    /// <summary>
    /// A LEITURA DE UMA RODADA: o ART saneado, o Protheus e a confirmação dos chassis curtos — o que a carga e a
    /// projeção têm em comum, antes de qualquer decisão.
    /// </summary>
    private async Task<Resultado<(List<VendaDoArtSaneada> Vendas, ParqueNoProtheus Parque, IReadOnlyDictionary<string, CadastroNoProtheus> Cadastros, bool ProtheusLido)>>
        LerESanearAsync(CancellationToken ct)
    {
        relatar("Lendo a view de vendas do ART (sessão somente leitura)…");
        var lidos = await lerArt(ct);
        if (!lidos.EhSucesso)
            return Resultado<(List<VendaDoArtSaneada>, ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>, bool)>.Indisponivel(lidos.Erro!);

        var vendas = lidos.Valor.Select(SaneamentoDoArt.Sanear).ToList();

        var codigoRepetido = vendas.GroupBy(v => v.Codigo, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
        if (codigoRepetido is not null)
            return Resultado<(List<VendaDoArtSaneada>, ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>, bool)>.Indisponivel(
                "A view do ART devolveu o mesmo código de venda em mais de uma linha; sem identificador único a " +
                "recarga duplicaria. Nada foi gravado.");

        var parque = new ParqueNoProtheus([]);
        IReadOnlyDictionary<string, CadastroNoProtheus> cadastros = new Dictionary<string, CadastroNoProtheus>();
        var protheusLido = false;

        if (lerProtheus is null)
        {
            _observacoes.Add("Protheus não configurado: o dono no cadastro de veículos (VV1), os chassis curtos e a completude do " +
                             "cadastro (SA1) não foram conferidos.");
        }
        else
        {
            relatar("Lendo o parque com o dono atual (VV1010) e o cadastro dos compradores (SA1010) no Protheus — somente leitura…");
            var documentos = vendas.Where(v => v.Documento is not null).Select(v => v.Documento!.Value.Numero).ToHashSet(StringComparer.Ordinal);
            var lido = await lerProtheus(documentos, ct);

            if (lido.EhSucesso)
            {
                (parque, cadastros, protheusLido) = (lido.Valor.Parque, lido.Valor.Cadastros, true);
                vendas = [.. vendas.Select(v => SaneamentoDoArt.ConfirmarIdentificadorCurto(v, parque))];
            }
            else
            {
                _observacoes.Add("Protheus não lido nesta rodada: " + lido.Erro);
            }
        }

        ContarLeitura(vendas);
        Contar("1. Leitura do ART", RotuloDeSeriesConfirmadas, vendas.Count(v => v.SituacaoDoChassi == SituacaoDoChassiNaOrigem.ConfirmadoPeloProtheus));
        Contar("1. Leitura do ART", "identificador curto que é COMPONENTE no Protheus (continua pendente)",
            vendas.Count(v => v.Motivos.Contains(MotivoDePendenciaDoArt.IdentificadorDeComponente)));

        return Resultado<(List<VendaDoArtSaneada>, ParqueNoProtheus, IReadOnlyDictionary<string, CadastroNoProtheus>, bool)>.Ok(
            (vendas, parque, cadastros, protheusLido));
    }

    /// <summary>
    /// A PROJEÇÃO — o que o próximo ciclo decidiria, registro a registro, SEM ABRIR TRANSAÇÃO NENHUMA. É a que roda da
    /// estação contra o banco de produção, que daqui só se lê (<c>--somente-art --projetar</c>).
    ///
    /// <para><b>A decisão é a MESMA da carga</b> (<see cref="Decidir"/>). O que a projeção refaz em leitura é só a
    /// filial da unidade: a correspondência revisada por pessoa vale como está, e a outra é reavaliada pela regra —
    /// exatamente o que a carga faz ao gravar. O "antes" é o que <c>integracao.RegistroDeOrigem</c> tem hoje.</para>
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDaCargaDoArt>> ProjetarAsync(CancellationToken ct)
    {
        var lidas = await LerESanearAsync(ct);
        if (!lidas.EhSucesso) return Resultado<RelatorioDaCargaDoArt>.Indisponivel(lidas.Erro!);

        var (vendas, parque, _, protheusLido) = lidas.Valor;

        await using var banco = abrirContexto();

        var sistemaId = await banco.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == LeitorDoArt.CodigoDoSistema).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);

        var filiais = await banco.Empresas.AsNoTracking().Select(e => new FilialDoCrm(e.Id, e.Codigo, e.Nome, e.EstaAtiva)).ToListAsync(ct);
        var revisadas = sistemaId is null
            ? []
            : await banco.CorrespondenciasDaOrigem.AsNoTracking()
                .Where(c => c.SistemaId == sistemaId && c.Tipo == TipoDeCorrespondencia.Unidade && c.RevisadaEm != null)
                .ToDictionaryAsync(c => c.CodigoNaOrigem, StringComparer.Ordinal, ct);

        int? Filial(string? texto)
        {
            if (texto is null) return null;
            if (revisadas.TryGetValue(ClassificacaoDoArt.Codificar(texto), out var revisada))
                return revisada.EhUtilizavel ? revisada.EmpresaCorrespondenteId : null;

            var avaliacao = ClassificacaoDoArt.ClassificarUnidade(texto, filiais);
            return avaliacao.Situacao == SituacaoDaCorrespondencia.CorrespondenciaExata
                ? int.Parse(avaliacao.Destino!, CultureInfo.InvariantCulture)
                : null;
        }

        var clientesPorDocumento = (await banco.Clientes.AsNoTracking()
                .Where(c => c.ExcluidoEm == null && c.Documento != null)
                .Select(c => new { c.Id, c.EmpresaId, c.Documento })
                .ToListAsync(ct))
            .GroupBy(c => c.Documento!.Value.Numero, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(c => (c.Id, c.EmpresaId)).ToList(), StringComparer.Ordinal);

        var maquinas = await banco.Equipamentos.AsNoTracking().Select(e => new { e.Chassi, e.ExcluidoEm }).ToListAsync(ct);
        var ativos = maquinas.Where(m => m.ExcluidoEm == null).Select(m => m.Chassi.Numero).ToHashSet(StringComparer.Ordinal);
        var baixados = maquinas.Where(m => m.ExcluidoEm != null).Select(m => m.Chassi.Numero).ToHashSet(StringComparer.Ordinal);

        var antes = sistemaId is null
            ? []
            : await banco.RegistrosDeOrigem.AsNoTracking()
                .Where(r => r.SistemaId == sistemaId && r.Fluxo == Fluxo)
                .ToDictionaryAsync(r => r.ChaveOrigem, r => (r.Decisao, r.Motivos), StringComparer.Ordinal, ct);

        const string etapaDoAntes = "2. Hoje no CRM (integracao.RegistroDeOrigem)";
        var pendentesHoje = antes.Values.Where(r => r.Decisao == DecisaoDaIntegracao.Pendente).ToList();
        Contar(etapaDoAntes, "registros pendentes hoje", pendentesHoje.Count);
        foreach (var motivo in pendentesHoje.SelectMany(r => (r.Motivos ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
                     .GroupBy(m => m, StringComparer.Ordinal).OrderByDescending(g => g.Count()))
            Contar(etapaDoAntes, $"  hoje, por motivo: {motivo.Key}", motivo.Count());

        const string etapaDoDepois = "3. Projeção do próximo ciclo (a mesma decisão da carga)";
        var pendentesPorMotivo = new Dictionary<string, int>(StringComparer.Ordinal);
        var liberadosPorRazao = new Dictionary<string, int>(StringComparer.Ordinal);
        var importaveis = new List<(VendaDoArtSaneada Venda, long CompradorId, bool PeloProtheus)>();
        var resolvidos = new HashSet<string>(StringComparer.Ordinal);
        var ausentes = new HashSet<string>(StringComparer.Ordinal);
        var liberados = 0;

        foreach (var lida in vendas.OrderByDescending(v => v.VendidaEm).ThenByDescending(v => long.TryParse(v.Codigo, out var n) ? n : 0))
        {
            var decisao = Decidir(lida, Filial(lida.Unidade), clientesPorDocumento, ativos, baixados, protheusLido ? parque : null, raizesDoGrupo);
            if (decisao.CompradorPeloProtheus) resolvidos.Add(decisao.Venda.Documento!.Value.Numero);
            if (decisao.CompradorAusente) ausentes.Add(decisao.Venda.Documento!.Value.Numero);

            if (decisao.Motivos.Count > 0)
            {
                foreach (var motivo in decisao.Motivos) pendentesPorMotivo[motivo] = pendentesPorMotivo.GetValueOrDefault(motivo) + 1;
                continue;
            }

            importaveis.Add((decisao.Venda, decisao.CompradorId!.Value, decisao.CompradorPeloProtheus));

            // O QUE A REGRA NOVA LIBERA: o registro que hoje está pendente e entraria no próximo ciclo, pelo motivo
            // que o prendia.
            if (!antes.TryGetValue(lida.Codigo, out var hoje) || hoje.Decisao != DecisaoDaIntegracao.Pendente) continue;
            liberados++;
            var motivosDeHoje = (hoje.Motivos ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
            foreach (var motivo in motivosDeHoje.Order(StringComparer.Ordinal))
                liberadosPorRazao[motivo] = liberadosPorRazao.GetValueOrDefault(motivo) + 1;
        }

        Contar(etapaDoDepois, "registros importáveis", importaveis.Count);
        Contar(etapaDoDepois, RotuloDePendentes, vendas.Count - importaveis.Count);
        foreach (var (motivo, quantidade) in pendentesPorMotivo.OrderByDescending(p => p.Value))
            Contar(etapaDoDepois, $"  pendência por motivo: {motivo}", quantidade);
        Contar(etapaDoDepois, "registros pendentes hoje que entrariam", liberados);
        foreach (var (motivo, quantidade) in liberadosPorRazao.OrderByDescending(p => p.Value))
            Contar(etapaDoDepois, $"  liberados que hoje estão pendentes por: {motivo}", quantidade);
        Contar(etapaDoDepois, "  delas, com o chassi curto confirmado pelo Protheus",
            importaveis.Count(i => i.Venda.SituacaoDoChassi == SituacaoDoChassiNaOrigem.ConfirmadoPeloProtheus && antes.GetValueOrDefault(i.Venda.Codigo).Decisao == DecisaoDaIntegracao.Pendente));
        Contar(etapaDoDepois, "  delas, com o comprador substituído pelo dono atual no Protheus",
            importaveis.Count(i => i.PeloProtheus && antes.GetValueOrDefault(i.Venda.Codigo).Decisao == DecisaoDaIntegracao.Pendente));
        Contar(etapaDoDepois, RotuloDeCompradoresPeloProtheus, importaveis.Count(i => i.PeloProtheus));
        Contar(etapaDoDepois, "máquinas novas no CRM (chassi importável que ainda não existe)",
            importaveis.Select(i => i.Venda.Chassi!.Value.Numero).Distinct(StringComparer.Ordinal).Count(c => !ativos.Contains(c)));
        Contar(etapaDoDepois, "compradores que sairiam da fila (resolvidos pelo dono do Protheus)", resolvidos.Count(d => !ausentes.Contains(d)));

        const string etapaDasDivergencias = "4. Dono no Protheus × comprador do ART (decisão 1), pela venda mais recente de cada chassi";
        var desfechos = new Dictionary<DesfechoDaComparacaoComOArt, int>();
        foreach (var grupo in importaveis.GroupBy(i => i.Venda.Chassi!.Value.Numero, StringComparer.Ordinal))
        {
            var maisRecente = grupo.First();
            if (!protheusLido || maisRecente.PeloProtheus || !parque.TentarAchar(grupo.Key, out var noProtheus)) continue;

            var desfecho = RegrasDoParque.Comparar(
                noProtheus, maisRecente.Venda.Documento!.Value.Numero, maisRecente.Venda.FaturadaEm ?? maisRecente.Venda.VendidaEm, raizesDoGrupo);
            desfechos[desfecho] = desfechos.GetValueOrDefault(desfecho) + 1;
        }

        foreach (var desfecho in Enum.GetValues<DesfechoDaComparacaoComOArt>())
            Contar(etapaDasDivergencias, desfecho.ToString(), desfechos.GetValueOrDefault(desfecho));
        Contar(etapaDasDivergencias, "divergências que ficariam registradas (o ART prevalece)",
            desfechos.Where(p => RegrasDoParque.RegistraDivergencia(p.Key)).Sum(p => p.Value));
        Contar(etapaDasDivergencias, "divergências abertas hoje (ProprietarioNoProtheusDiferenteDoComprador)",
            sistemaId is null ? 0 : await banco.DivergenciasDeIntegracao.AsNoTracking().CountAsync(
                d => d.SistemaId == sistemaId && d.Tipo == TipoDeDivergencia.ProprietarioNoProtheusDiferenteDoComprador && d.Situacao == SituacaoDaDivergencia.Aberta, ct));

        _observacoes.Add("PROJEÇÃO: só leitura — nenhuma transação foi aberta, e o banco não mudou.");
        return Resultado<RelatorioDaCargaDoArt>.Ok(new RelatorioDaCargaDoArt(true, _contagens, [], _observacoes));
    }

    private async Task<(
        Dictionary<string, CorrespondenciaDaOrigem> Linhas,
        Dictionary<string, CorrespondenciaDaOrigem> Produtos,
        Dictionary<string, CorrespondenciaDaOrigem> Unidades,
        List<(string Unidade, string Filial, string Situacao, int Ocorrencias, string Criterio)> Relatorio)>
        CorresponderAsync(
            CrmDbContext banco, int sistemaId, List<VendaDoArtSaneada> vendas,
            Dictionary<string, LinhaDeProduto> linhasDeProduto, DateTime agora, CancellationToken ct)
    {
        const string etapa = "0. Correspondências da origem → catálogo do CRM";

        var modelos = await banco.Modelos.AsNoTracking().Where(m => m.EstaAtivo)
            .Select(m => new ModeloDoCatalogo(m.Id, m.Codigo)).ToListAsync(ct);
        var filiais = await banco.Empresas.AsNoTracking()
            .Select(e => new FilialDoCrm(e.Id, e.Codigo, e.Nome, e.EstaAtiva)).ToListAsync(ct);

        var existentes = await banco.CorrespondenciasDaOrigem.Where(c => c.SistemaId == sistemaId)
            .ToDictionaryAsync(c => (c.Tipo, c.CodigoNaOrigem), ct);

        int novas = 0, reavaliadas = 0, revisadasPreservadas = 0;

        CorrespondenciaDaOrigem Registrar(TipoDeCorrespondencia tipo, string texto, string? contexto, int ocorrencias,
            AvaliacaoDeCorrespondencia avaliacao, int? linhaId, int? modeloId, int? empresaId)
        {
            var codigo = ClassificacaoDoArt.Codificar(texto);
            if (!existentes.TryGetValue((tipo, codigo), out var correspondencia))
            {
                correspondencia = CorrespondenciaDaOrigem.Registrar(sistemaId, tipo, codigo, Limitar(texto, 120)!, Limitar(contexto, 120), agora);
                banco.CorrespondenciasDaOrigem.Add(correspondencia);
                existentes[(tipo, codigo)] = correspondencia;
                novas++;
            }

            correspondencia.RegistrarLeitura(ocorrencias, agora);
            if (correspondencia.FoiRevisada) revisadasPreservadas++;
            else if (correspondencia.Avaliar(avaliacao.Situacao, linhaId, modeloId, empresaId, avaliacao.Criterio)) reavaliadas++;
            return correspondencia;
        }

        var linhas = new Dictionary<string, CorrespondenciaDaOrigem>(StringComparer.Ordinal);
        foreach (var grupo in vendas.Where(v => v.Linha.Length > 0).GroupBy(v => v.Linha, StringComparer.Ordinal))
        {
            var avaliacao = ClassificacaoDoArt.ClassificarLinha(grupo.Key);
            int? linhaId = null;
            if (avaliacao.Destino is { } codigoDaClassificacao)
            {
                if (linhasDeProduto.TryGetValue(codigoDaClassificacao, out var classificacao)) linhaId = classificacao.Id;
                else avaliacao = new(SituacaoDaCorrespondencia.PendenteDeRevisao, null, $"A classificação {codigoDaClassificacao} não está no seed.");
            }

            linhas[grupo.Key] = Registrar(TipoDeCorrespondencia.LinhaDeProduto, grupo.Key, null, grupo.Count(), avaliacao, linhaId, null, null);
        }

        var produtos = new Dictionary<string, CorrespondenciaDaOrigem>(StringComparer.Ordinal);
        foreach (var grupo in vendas.Where(v => v.Produto.Length > 0).GroupBy(v => v.Produto, StringComparer.Ordinal))
        {
            var avaliacao = ClassificacaoDoArt.ClassificarProduto(grupo.Key, modelos);
            int? modeloId = avaliacao.Destino is { } id ? int.Parse(id, CultureInfo.InvariantCulture) : null;
            var contexto = string.Join(" / ", grupo.Select(v => v.Linha).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
            produtos[grupo.Key] = Registrar(TipoDeCorrespondencia.Produto, grupo.Key, contexto, grupo.Count(), avaliacao, null, modeloId, null);
        }

        var unidades = new Dictionary<string, CorrespondenciaDaOrigem>(StringComparer.Ordinal);
        var relatorio = new List<(string, string, string, int, string)>();
        var textos = vendas.SelectMany(v => new[] { v.Unidade, v.UnidadeDoFaturamento }).OfType<string>().Distinct(StringComparer.Ordinal);
        foreach (var texto in textos.Order(StringComparer.Ordinal))
        {
            var avaliacao = ClassificacaoDoArt.ClassificarUnidade(texto, filiais);
            int? empresaId = avaliacao.Destino is { } id ? int.Parse(id, CultureInfo.InvariantCulture) : null;
            var comoVendedora = vendas.Count(v => v.Unidade == texto);
            var comoFaturadora = vendas.Count(v => v.UnidadeDoFaturamento == texto);
            var empresas = vendas.Where(v => v.Unidade == texto && v.Empresa is not null).Select(v => v.Empresa!).Distinct(StringComparer.Ordinal);

            var correspondencia = Registrar(TipoDeCorrespondencia.Unidade, texto, Limitar(string.Join(" / ", empresas), 120),
                comoVendedora + comoFaturadora, avaliacao, null, null, empresaId);
            unidades[texto] = correspondencia;

            var filial = filiais.FirstOrDefault(f => f.Id == correspondencia.EmpresaCorrespondenteId);
            relatorio.Add((
                texto,
                filial is null ? "—" : $"{filial.Codigo} ({(filial.EstaAtiva ? "ativa" : "inativa")})",
                correspondencia.Situacao.ToString(),
                comoVendedora,
                correspondencia.Criterio));
        }

        foreach (var tipo in Enum.GetValues<TipoDeCorrespondencia>())
            foreach (var situacao in existentes.Values.Where(c => c.Tipo == tipo).GroupBy(c => c.Situacao).OrderBy(g => g.Key))
                Contar(etapa, $"{tipo}: {situacao.Key}", situacao.Count());

        Contar(etapa, "correspondências registradas pela primeira vez", novas);
        Contar(etapa, "correspondências reavaliadas pela regra", reavaliadas);
        Contar(etapa, "correspondências revisadas por pessoa e preservadas", revisadasPreservadas);
        Contar(etapa, "unidades distintas como unidade vendedora", vendas.Select(v => v.Unidade).OfType<string>().Distinct(StringComparer.Ordinal).Count());

        await banco.SaveChangesAsync(ct);
        return (linhas, produtos, unidades, relatorio);
    }

    private async Task AtualizarFilaDeCompradoresAsync(
        CrmDbContext banco,
        int sistemaId,
        List<(VendaDoArtSaneada Venda, int? EmpresaId)> semCompradorNoCrm,
        HashSet<string> documentosComCliente,
        HashSet<string> resolvidosPeloProtheus,
        IReadOnlyDictionary<string, CadastroNoProtheus> cadastros,
        bool protheusLido,
        DateTime agora,
        CancellationToken ct)
    {
        const string etapa = "7. Fila de compradores ausentes do CRM (nenhum cliente criado)";

        var fila = await banco.CompradoresPendentes.Where(c => c.SistemaId == sistemaId).ToListAsync(ct);
        var filaPorDocumento = fila.ToDictionary(c => c.Documento.Numero, StringComparer.Ordinal);

        var documentos = semCompradorNoCrm.Select(p => p.Venda.Documento!.Value.Numero).ToHashSet(StringComparer.Ordinal);

        var notas = (await banco.FaturamentoSemClientes.AsNoTracking()
                .Where(f => f.ExcluidoEm == null)
                .Select(f => new { f.Documento, f.Nome, f.Natureza, f.Competencia, f.Notas })
                .ToListAsync(ct))
            .Where(f => documentos.Contains(f.Documento))
            .GroupBy(f => f.Documento, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var codigoDaFilial = await banco.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Id, e => e.Codigo, ct);

        int incluidos = 0, atualizados = 0, comNota = 0, semNota = 0, semFilial = 0, cadastrados = 0;
        int noSa1 = 0, bloqueados = 0, completos = 0;

        foreach (var grupo in semCompradorNoCrm.GroupBy(p => p.Venda.Documento!.Value.Numero, StringComparer.Ordinal))
        {
            var ordenadas = grupo.OrderByDescending(p => p.Venda.VendidaEm).ToList();
            var empresaId = ordenadas.Select(p => p.EmpresaId).FirstOrDefault(e => e is not null);
            if (empresaId is null) { semFilial++; continue; }

            var documento = ordenadas[0].Venda.Documento!.Value;
            var nome = ordenadas.Select(p => p.Venda.NomeDoComprador).FirstOrDefault(n => n is not null) ?? "(sem nome no ART)";
            var nomeNormalizado = ClassificacaoDoArt.NormalizarParaComparar(nome);

            var notasDoDocumento = notas.GetValueOrDefault(grupo.Key) ?? [];
            var cadastro = cadastros.GetValueOrDefault(grupo.Key);

            var situacaoNoProtheus = !protheusLido
                ? SituacaoNoCadastroDoProtheus.NaoConferido
                : cadastro is null ? SituacaoNoCadastroDoProtheus.Ausente
                : cadastro.Bloqueado ? SituacaoNoCadastroDoProtheus.Bloqueado
                : SituacaoNoCadastroDoProtheus.Ativo;

            var faltam = new List<string>();
            if (situacaoNoProtheus == SituacaoNoCadastroDoProtheus.NaoConferido) faltam.Add("conferir o cadastro no Protheus (SA1)");
            if (situacaoNoProtheus == SituacaoNoCadastroDoProtheus.Ausente) faltam.Add("cadastro no Protheus (SA1) — nenhuma fonte com endereço e município");
            if (situacaoNoProtheus == SituacaoNoCadastroDoProtheus.Bloqueado) faltam.Add("confirmar o cadastro bloqueado no Protheus");
            if (cadastro is not null && !cadastro.TemEndereco) faltam.Add("endereço");
            if (cadastro is not null && !cadastro.TemMunicipio) faltam.Add("município (código IBGE)");
            if (cadastro is not null && !cadastro.TemInscricaoEstadual) faltam.Add("inscrição estadual");
            if (cadastro is not null && cadastro.NomeNormalizado != nomeNormalizado) faltam.Add("conferir o nome (ART e Protheus diferem)");
            faltam.Add("definir filial e carteira responsáveis");

            if (cadastro is not null) noSa1++;
            if (situacaoNoProtheus == SituacaoNoCadastroDoProtheus.Bloqueado) bloqueados++;
            if (faltam.Count == 1) completos++;

            var apuracao = new ApuracaoDoCompradorPendente(
                empresaId.Value,
                Limitar(nome, 100)!,
                notasDoDocumento.Count > 0 ? GrupoDoCompradorPendente.ComNotaNoProtheus : GrupoDoCompradorPendente.SemNotaNoProtheus,
                ordenadas.Count,
                ordenadas.Count(p => p.Venda.SituacaoDoChassi == SituacaoDoChassiNaOrigem.Valido),
                ordenadas.Min(p => p.Venda.VendidaEm),
                ordenadas.Max(p => p.Venda.VendidaEm),
                Limitar(string.Join(",", ordenadas.Select(p => p.EmpresaId).OfType<int>().Distinct().Select(id => codigoDaFilial[id]).Order(StringComparer.Ordinal)), 200)!,
                notasDoDocumento.Sum(n => n.Notas),
                notasDoDocumento.Count == 0 ? null : Limitar(string.Join(",", notasDoDocumento.Select(n => n.Natureza.ToString()).Distinct().Order(StringComparer.Ordinal)), 40),
                notasDoDocumento.Count == 0 ? null : notasDoDocumento.Min(n => n.Competencia),
                notasDoDocumento.Count == 0 ? null : notasDoDocumento.Max(n => n.Competencia),
                notasDoDocumento.Any(n => ClassificacaoDoArt.NormalizarParaComparar(n.Nome) == nomeNormalizado),
                situacaoNoProtheus,
                cadastro?.TemEndereco ?? false,
                cadastro?.TemMunicipio ?? false,
                cadastro?.TemInscricaoEstadual ?? false,
                cadastro is not null && cadastro.NomeNormalizado == nomeNormalizado,
                Limitar(string.Join("; ", faltam), 400)!);

            if (apuracao.Grupo == GrupoDoCompradorPendente.ComNotaNoProtheus) comNota++; else semNota++;

            if (filaPorDocumento.TryGetValue(grupo.Key, out var existente))
            {
                existente.Atualizar(apuracao, agora, usuarioId);
                atualizados++;
            }
            else
            {
                var novo = CompradorPendente.Registrar(sistemaId, documento, apuracao, agora, usuarioId);
                banco.CompradoresPendentes.Add(novo);
                filaPorDocumento[grupo.Key] = novo;
                incluidos++;
            }
        }

        foreach (var pendente in fila.Where(c => c.Situacao != SituacaoDoCompradorPendente.Cadastrado && documentosComCliente.Contains(c.Documento.Numero)))
        {
            pendente.MarcarCadastrado(agora, usuarioId);
            cadastrados++;
        }

        // A FILA ESVAZIA PARA QUEM O DONO DO PROTHEUS RESOLVEU (decisão 5 de 24/09/2026): todas as vendas dele entraram
        // com o dono atual da máquina, e o cadastro dele deixou de travar venda nenhuma. Quem ainda tem venda presa
        // continua na fila, reapurado acima.
        var resolvidos = 0;
        foreach (var pendente in fila.Where(c => resolvidosPeloProtheus.Contains(c.Documento.Numero) && !documentos.Contains(c.Documento.Numero)))
            if (pendente.MarcarResolvidoPeloDonoNoProtheus(agora, usuarioId)) resolvidos++;

        Contar(etapa, "compradores distintos na fila nesta leitura", comNota + semNota);
        Contar(etapa, "  com nota de saída do Protheus sem cliente no CRM", comNota);
        Contar(etapa, "  sem nota do Protheus carregada", semNota);
        Contar(etapa, "  com cadastro na SA1 do Protheus", noSa1);
        Contar(etapa, "  com cadastro bloqueado na SA1", bloqueados);
        Contar(etapa, "  com endereço, município, inscrição e nome conferidos na SA1 (falta só a decisão)", completos);
        Contar(etapa, "incluídos na fila agora", incluidos);
        Contar(etapa, "já na fila e reapurados", atualizados);
        Contar(etapa, "marcados como cadastrados (o cliente passou a existir no CRM)", cadastrados);
        Contar(etapa, "saíram da fila: as vendas entraram com o dono atual no Protheus", resolvidos);
        Contar(etapa, "compradores sem filial em nenhuma venda (fora da fila)", semFilial);
    }

    private void Contar(string etapa, string rotulo, int valor) => _contagens.Add((etapa, rotulo, valor));

    private static string? Limitar(string? texto, int tamanho) =>
        texto is null ? null : texto.Length <= tamanho ? texto : texto[..tamanho];
}
