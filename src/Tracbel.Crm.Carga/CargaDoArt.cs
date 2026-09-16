using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;

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
/// desfeita no fim. Os números da simulação são, por construção, os da carga real.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="art">A leitura do ART.</param>
/// <param name="protheus">A leitura do cadastro do Protheus, quando configurada.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDoArt(
    Func<CrmDbContext> abrirContexto,
    LeitorDoArt art,
    LeitorDoCadastroDoProtheus? protheus,
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

    /// <summary>As divergências que descrevem um ESTADO — e que, por isso, deixam de ocorrer sozinhas.</summary>
    private static readonly HashSet<TipoDeDivergencia> DivergenciasDeEstado =
    [
        TipoDeDivergencia.CompradorDiferenteDoProprietarioNoCrm,
        TipoDeDivergencia.ProprietarioNoProtheusDiferenteDoComprador,
        TipoDeDivergencia.RegistroAusenteNaOrigem
    ];

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    private sealed record Importavel(VendaDoArtSaneada Venda, RegistroDeOrigem Registro, int EmpresaId, int? EmpresaDoFaturamentoId, long CompradorId);

    /// <summary>Executa a carga.</summary>
    /// <param name="simular">Desfaz tudo ao final.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDaCargaDoArt>> ExecutarAsync(bool simular, CancellationToken ct)
    {
        relatar("Lendo a view de vendas do ART (sessão somente leitura)…");
        var lidos = await art.LerVendasAsync(ct);
        if (!lidos.EhSucesso) return Resultado<RelatorioDaCargaDoArt>.Indisponivel(lidos.Erro!);

        var vendas = lidos.Valor.Select(SaneamentoDoArt.Sanear).ToList();

        var codigoRepetido = vendas.GroupBy(v => v.Codigo, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
        if (codigoRepetido is not null)
            return Resultado<RelatorioDaCargaDoArt>.Indisponivel(
                "A view do ART devolveu o mesmo código de venda em mais de uma linha; sem identificador único a " +
                "recarga duplicaria. Nada foi gravado.");

        ContarLeitura(vendas);

        var (donos, cadastros, protheusLido) = await LerProtheusAsync(vendas, ct);

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
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        int registrosNovos = 0, registrosAlterados = 0, registrosIguais = 0, importadasQueFicaramPendentes = 0;

        foreach (var s in vendas.OrderByDescending(v => v.VendidaEm).ThenByDescending(v => long.TryParse(v.Codigo, out var n) ? n : 0))
        {
            vistos.Add(s.Codigo);
            var motivos = new List<string>(s.Motivos);

            int? empresaId = s.Unidade is { } u && unidadePorTexto.TryGetValue(u, out var cu) && cu.EhUtilizavel ? cu.EmpresaCorrespondenteId : null;
            if (empresaId is null) motivos.Add(MotivoDePendenciaDoArt.UnidadeSemFilial);

            int? empresaDoFaturamentoId = s.UnidadeDoFaturamento is { } uf && unidadePorTexto.TryGetValue(uf, out var cf) && cf.EhUtilizavel
                ? cf.EmpresaCorrespondenteId
                : null;

            long? compradorId = null;
            if (s.Documento is { } documento)
            {
                if (!clientesPorDocumento.TryGetValue(documento.Numero, out var candidatos))
                {
                    motivos.Add(MotivoDePendenciaDoArt.CompradorAusente);
                    semCompradorNoCrm.Add((s, empresaId));
                }
                else if (candidatos.Count == 1)
                {
                    compradorId = candidatos[0].Id;
                }
                else
                {
                    // O MESMO DOCUMENTO EM MAIS DE UMA FILIAL: vale o cliente da filial da venda, quando é um só.
                    var naFilial = candidatos.Where(c => c.EmpresaId == empresaId).ToList();
                    if (naFilial.Count == 1) compradorId = naFilial[0].Id;
                    else motivos.Add(MotivoDePendenciaDoArt.CompradorAmbiguo);
                }
            }

            if (s.Chassi is { } chassiValido && !ativosPorChassi.ContainsKey(chassiValido.Numero) && baixados.Contains(chassiValido.Numero))
                motivos.Add(MotivoDePendenciaDoArt.MaquinaBaixada);

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
                registro.Decidir(DecisaoDaIntegracao.Pendente, string.Join(",", motivos.Distinct(StringComparer.Ordinal)), null);
                foreach (var motivo in motivos.Distinct(StringComparer.Ordinal))
                    pendentesPorMotivo[motivo] = pendentesPorMotivo.GetValueOrDefault(motivo) + 1;
                continue;
            }

            importaveis.Add(new Importavel(s, registro, empresaId!.Value, empresaDoFaturamentoId, compradorId!.Value));
        }

        Contar(etapaDaDecisao, "registros lidos pela primeira vez", registrosNovos);
        Contar(etapaDaDecisao, "registros já conhecidos com conteúdo alterado na origem", registrosAlterados);
        Contar(etapaDaDecisao, "registros já conhecidos sem alteração", registrosIguais);
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
                venda = VendaDeMaquina.Registrar(sistemaId, s.Codigo, equipamento.Id, i.CompradorId, dados, agora, usuarioId);
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

            if (venda.CompradorId != i.CompradorId)
            {
                Divergir(TipoDeDivergencia.CompradorAlteradoNaOrigem, s.Codigo, i.EmpresaId, equipamento.Id, venda.Id,
                    "O ART trocou o comprador de uma venda já importada. O vínculo do comprador anterior foi encerrado.",
                    Cliente(venda.CompradorId), Cliente(i.CompradorId), null);
                EncerrarVinculo(venda.Id, "O ART trocou o comprador desta venda.");
                venda.TrocarComprador(i.CompradorId, usuarioId);
            }

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
        int noProtheus = 0, donoIgualNoProtheus = 0, donoAmbiguoNoProtheus = 0;

        foreach (var grupo in importaveis.GroupBy(i => i.Venda.Chassi!.Value.Numero, StringComparer.Ordinal))
        {
            var maisRecente = grupo.First();
            var venda = vendasPorChave[maisRecente.Venda.Codigo];
            var equipamento = ativosPorChassi[grupo.Key];

            if (equipamento.ClienteId is { } dono && dono != venda.CompradorId)
                Divergir(TipoDeDivergencia.CompradorDiferenteDoProprietarioNoCrm, venda.ChaveOrigem, venda.EmpresaId, equipamento.Id, venda.Id,
                    "O comprador da venda mais recente no ART não é o dono registrado no CRM. O dono não foi alterado.",
                    Cliente(dono), Cliente(venda.CompradorId), null);

            if (!protheusLido || !donos.TryGetValue(grupo.Key, out var donoNoProtheus)) continue;

            noProtheus++;
            if (donoNoProtheus.Ambiguo) { donoAmbiguoNoProtheus++; continue; }
            if (donoNoProtheus.Documento is not { } documentoNoProtheus) continue;
            if (documentoNoProtheus == maisRecente.Venda.Documento!.Value.Numero) { donoIgualNoProtheus++; continue; }

            var clienteNoProtheus = clientesPorDocumento.TryGetValue(documentoNoProtheus, out var doProtheus) && doProtheus.Count == 1
                ? Cliente(doProtheus[0].Id)
                : "documento sem cliente único no CRM";

            Divergir(TipoDeDivergencia.ProprietarioNoProtheusDiferenteDoComprador, venda.ChaveOrigem, venda.EmpresaId, equipamento.Id, venda.Id,
                "O dono do chassi no cadastro de veículos do Protheus (VV1) não é o comprador da venda mais recente no ART.",
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
        Contar(etapaDasDivergencias, "  com dono igual ao comprador da venda mais recente", donoIgualNoProtheus);
        Contar(etapaDasDivergencias, "  com dono ambíguo no Protheus (código com lojas de documentos diferentes)", donoAmbiguoNoProtheus);

        await banco.SaveChangesAsync(ct);

        // -----------------------------------------------------------------------------------------
        // 6. A fila dos compradores ausentes do CRM — nenhum cliente é criado.
        // -----------------------------------------------------------------------------------------
        await AtualizarFilaDeCompradoresAsync(banco, sistemaId, semCompradorNoCrm, clientesPorDocumento.Keys.ToHashSet(StringComparer.Ordinal),
            cadastros, protheusLido, agora, ct);

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

    private async Task<(IReadOnlyDictionary<string, DonoNoProtheus>, IReadOnlyDictionary<string, CadastroNoProtheus>, bool)> LerProtheusAsync(
        List<VendaDoArtSaneada> vendas, CancellationToken ct)
    {
        if (protheus is null)
        {
            _observacoes.Add("Protheus não configurado: a divergência de dono no VV1 e a completude do cadastro (SA1) não foram conferidas.");
            return (new Dictionary<string, DonoNoProtheus>(), new Dictionary<string, CadastroNoProtheus>(), false);
        }

        relatar("Lendo o dono dos chassis (VV1010) e o cadastro dos compradores (SA1010) no Protheus — somente leitura…");
        var documentos = vendas.Where(v => v.Documento is not null).Select(v => v.Documento!.Value.Numero).ToHashSet(StringComparer.Ordinal);
        var lido = await protheus.LerAsync(documentos, ct);

        if (lido.EhSucesso) return (lido.Valor.Donos, lido.Valor.Cadastros, true);

        _observacoes.Add("Protheus não lido nesta rodada: " + lido.Erro);
        return (new Dictionary<string, DonoNoProtheus>(), new Dictionary<string, CadastroNoProtheus>(), false);
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

        foreach (var pendente in fila.Where(c => c.Situacao == SituacaoDoCompradorPendente.AguardandoCadastro && documentosComCliente.Contains(c.Documento.Numero)))
        {
            pendente.MarcarCadastrado(agora, usuarioId);
            cadastrados++;
        }

        Contar(etapa, "compradores distintos na fila nesta leitura", comNota + semNota);
        Contar(etapa, "  com nota de saída do Protheus sem cliente no CRM", comNota);
        Contar(etapa, "  sem nota do Protheus carregada", semNota);
        Contar(etapa, "  com cadastro na SA1 do Protheus", noSa1);
        Contar(etapa, "  com cadastro bloqueado na SA1", bloqueados);
        Contar(etapa, "  com endereço, município, inscrição e nome conferidos na SA1 (falta só a decisão)", completos);
        Contar(etapa, "incluídos na fila agora", incluidos);
        Contar(etapa, "já na fila e reapurados", atualizados);
        Contar(etapa, "marcados como cadastrados (o cliente passou a existir no CRM)", cadastrados);
        Contar(etapa, "compradores sem filial em nenhuma venda (fora da fila)", semFilial);
    }

    private void Contar(string etapa, string rotulo, int valor) => _contagens.Add((etapa, rotulo, valor));

    private static string? Limitar(string? texto, int tamanho) =>
        texto is null ? null : texto.Length <= tamanho ? texto : texto[..tamanho];
}
