using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Carga;

/// <summary>
/// <b>LEGADO / SOMENTE REFERÊNCIA — CONGELADO NA FASE 1 (decisão D-12, documento 41).</b>
///
/// <para>Este arquivo NÃO é fonte de dado novo. Ele fica porque é a documentação executável de como
/// o dado do Vórtice foi lido, saneado e interpretado — cada regra aqui responde por um achado do
/// sistema de origem, e jogar isso fora seria jogar fora a única explicação que existe para o
/// formato do que já está gravado. O que se perdeu foi o direito de rodá-lo por rotina: o
/// <c>Program</c> recusa qualquer modo que leia o Vórtice sem uma declaração explícita na linha de
/// comando. O destino definitivo do código é decidido na FASE 8.</para>
///
/// <para>O que continua operacional e não passa por aqui: o faturamento do Protheus
/// (<c>--somente-faturamento</c>), o território (<c>--somente-territorio</c>) e o ART
/// (<c>--somente-art</c>).</para>
///
/// A CARGA — lê o recorte do sistema legado pela quarentena e grava no cadastro do CRM.
///
/// <para><b>Idempotente por construção.</b> Nada aqui usa "já rodei?" como pergunta: cada
/// registro do legado tem uma linha em <c>integracao.ChaveExterna</c> ligando a chave de origem
/// ao identificador do CRM. Rodar de novo encontra a linha, atualiza o que mudou e carimba a
/// conciliação. É a diferença entre uma carga e uma bagunça — e é a mesma peça que a integração
/// contínua vai usar depois, o que faz desta carga o ensaio dela, e não um script de uma vez.</para>
///
/// <para><b>Recusar é um resultado gravado.</b> Toda linha que não passa no saneamento vira uma
/// linha em <c>integracao.MensagemDescartada</c>, com a linha crua em JSON e o motivo em
/// português. [V] A fila de e-mail do legado tem 22.512 falhas que ninguém nunca viu.</para>
///
/// <para><b>Transação por bloco, não por carga inteira.</b> Uma transação de 24 mil linhas
/// seguraria bloqueio por minutos e perderia tudo num erro no fim. Um bloco que falha desfaz só
/// ele, e a execução seguinte retoma pelo de-para sem duplicar nada.</para>
/// </summary>
internal sealed class CargaDoVortice(
    Func<CrmDbContext> abrirContexto,
    LeitorDeCargaDoVortice leitor,
    IReadOnlyDictionary<int, int> deParaDeFiliais,
    long usuarioResponsavelId,
    Action<string> relatar,
    ConsolidacaoDeGrafiasCortadas consolidacaoDeGrafias)
{
    private const int TamanhoDoBloco = 500;

    /// <summary>O sistema do legado, conhecido depois de garantido — a origem das gravações na trilha.</summary>
    private int? _sistemaDoLegado;

    /// <summary>
    /// Abre um contexto que grava na trilha como integração do Vórtice (documento 41, fase 2), assim
    /// que o sistema for conhecido. Antes disso a origem é a do contexto de acesso: sistema.
    /// </summary>
    private CrmDbContext AbrirContextoDaCarga()
    {
        var contexto = abrirContexto();
        if (_sistemaDoLegado is { } sistema) contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistema);
        return contexto;
    }

    private const string FluxoDeMunicipio = "VORTICE.CARGA.MUNICIPIO";
    private const string FluxoDeCliente = "VORTICE.CARGA.CLIENTE";
    private const string FluxoDeEndereco = "VORTICE.CARGA.ENDERECO";
    private const string FluxoDeContato = "VORTICE.CARGA.CONTATO";
    private const string FluxoDeEquipamento = "VORTICE.CARGA.EQUIPAMENTO";

    /// <summary>
    /// O papel com que o contato é ligado ao cliente.
    ///
    /// [V] O legado guarda "tipo de contato" como texto livre e não diz se a pessoa decide, se
    /// influencia ou se nenhum dos dois. Mapear esse texto para DECISOR ou INFLUENCIADOR seria
    /// afirmar sobre o negócio uma coisa que o dado não sustenta. O catálogo ganha um item que
    /// diz exatamente o que se sabe — nada — e quem for qualificar o contato depois qualifica a
    /// partir de um estado honesto, não de um chute herdado.
    /// </summary>
    private const string PapelNaoInformado = "NAO_INFORMADO";

    private readonly Dictionary<string, ContagemDeSaneamento> _saneamento = new(StringComparer.Ordinal);
    private readonly List<LinhaRecusada> _recusas = [];

    /// <summary>Executa a carga inteira e devolve o que aconteceu.</summary>
    /// <param name="recorte">O recorte escolhido.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ResumoDaCarga>> ExecutarAsync(RecorteDaCarga recorte, CancellationToken ct)
    {
        var sistemaId = await GarantirSistemaAsync(ct);
        _sistemaDoLegado = sistemaId;
        var papelId = await GarantirPapelAsync(ct);

        // -----------------------------------------------------------------------------------------
        // Passo 0 — O CATÁLOGO DE MUNICÍPIOS, ANTES DE TUDO.
        //
        // Ele vem primeiro porque o endereço aponta para ele: sem o catálogo, todo endereço da
        // carga entraria como resíduo de texto, que é exatamente o defeito que o documento 26
        // corrige. É também o passo mais barato da carga inteira — dez mil linhas de referência.
        // -----------------------------------------------------------------------------------------
        relatar("Lendo o catálogo de municípios do sistema de origem…");
        var municipios = await leitor.LerMunicipiosAsync(recorte, ct);
        if (!municipios.EhSucesso) return Resultado<ResumoDaCarga>.Indisponivel(municipios.Erro!);
        relatar($"  {municipios.Valor.LinhasLidas} linha(s) lidas · " +
                $"{municipios.Valor.Aceitos.Count} aceitas · {municipios.Valor.Recusadas.Count} recusadas.");

        _recusas.AddRange(municipios.Valor.Recusadas);
        ContarSaneamento(municipios.Valor.Aceitos.Select(m => (m.Correcoes, m.Rejeicoes)));

        var catalogoDeMunicipios = await GravarMunicipiosAsync(sistemaId, municipios.Valor.Aceitos, ct);

        relatar($"Lendo clientes do recorte de {recorte.Ano}. A varredura de agenda e histórico " +
                "leva alguns minutos.");

        var clientes = await leitor.LerClientesAsync(recorte, ct);
        if (!clientes.EhSucesso) return Resultado<ResumoDaCarga>.Indisponivel(clientes.Erro!);
        relatar($"  {clientes.Valor.LinhasLidas} linha(s) lidas · {clientes.Valor.Aceitos.Count} aceitas · " +
                $"{clientes.Valor.Recusadas.Count} recusadas.");

        var contatos = await leitor.LerContatosAsync(recorte, ct);
        if (!contatos.EhSucesso) return Resultado<ResumoDaCarga>.Indisponivel(contatos.Erro!);
        relatar($"Contatos: {contatos.Valor.LinhasLidas} lida(s) · {contatos.Valor.Aceitos.Count} aceitas · " +
                $"{contatos.Valor.Recusadas.Count} recusadas.");

        var equipamentos = await leitor.LerEquipamentosAsync(recorte, ct);
        if (!equipamentos.EhSucesso) return Resultado<ResumoDaCarga>.Indisponivel(equipamentos.Erro!);
        relatar($"Equipamentos: {equipamentos.Valor.LinhasLidas} lida(s) · " +
                $"{equipamentos.Valor.Aceitos.Count} aceitas · {equipamentos.Valor.Recusadas.Count} recusadas.");

        _recusas.AddRange(clientes.Valor.Recusadas);
        _recusas.AddRange(contatos.Valor.Recusadas);
        _recusas.AddRange(equipamentos.Valor.Recusadas);

        ContarSaneamento(clientes.Valor.Aceitos.Select(c => (c.Correcoes, c.Rejeicoes)));
        ContarSaneamento(contatos.Valor.Aceitos.Select(c => (c.Correcoes, c.Rejeicoes)));
        ContarSaneamento(equipamentos.Valor.Aceitos.Select(c => (c.Correcoes, c.Rejeicoes)));

        var chavesDeCliente = await GravarClientesAsync(sistemaId, clientes.Valor.Aceitos, ct);
        var enderecos = await GravarEnderecosAsync(
            sistemaId, clientes.Valor.Aceitos, chavesDeCliente, catalogoDeMunicipios, ct);

        // AS GRAFIAS CORTADAS SE CONFEREM NA MESMA EXECUÇÃO (documento 32, seção 4.6). A gravação
        // acima já não desfez a correção cuja evidência não mudou; esta conferência cuida do endereço
        // novo ou alterado. Ninguém precisa lembrar de rodar a carga do território depois desta.
        relatar("Conferindo os endereços em grafia cortada do catálogo (documento 32, seção 4.6)…");
        var grafias = await consolidacaoDeGrafias.ExecutarAsync(ct);
        relatar($"  {grafias.Reapontados} reapontado(s) para o município oficial · " +
                $"{grafias.Pendentes} pendente(s) de conferência, com o motivo na fila de revisão" +
                (grafias.ContornoDisponivel ? "." : " · contorno oficial do IBGE indisponível nesta rodada."));
        var gravadosDeContato = await GravarContatosAsync(
            sistemaId, papelId, contatos.Valor.Aceitos, chavesDeCliente, ct);
        var gravadosDeEquipamento = await GravarEquipamentosAsync(
            sistemaId, equipamentos.Valor.Aceitos, chavesDeCliente, ct);

        await GravarRecusasAsync(ct);

        await MarcarSincronismoAsync(
            sistemaId, FluxoDeMunicipio, municipios.Valor, catalogoDeMunicipios.PorChaveDeOrigem.Count, ct);
        await MarcarSincronismoAsync(sistemaId, FluxoDeCliente, clientes.Valor, chavesDeCliente.Count, ct);
        await MarcarSincronismoAsync(sistemaId, FluxoDeEndereco, clientes.Valor, enderecos.Gravados, ct);
        await MarcarSincronismoAsync(sistemaId, FluxoDeContato, contatos.Valor, gravadosDeContato, ct);
        await MarcarSincronismoAsync(
            sistemaId, FluxoDeEquipamento, equipamentos.Valor, gravadosDeEquipamento, ct);

        return Resultado<ResumoDaCarga>.Ok(new ResumoDaCarga(
            MunicipiosLidos: municipios.Valor.LinhasLidas,
            MunicipiosGravados: catalogoDeMunicipios.PorNomeEUf.Values.Distinct().Count(),
            MunicipiosDuplicadosNaOrigem: catalogoDeMunicipios.DuplicadosNaOrigem,
            ClientesLidos: clientes.Valor.LinhasLidas,
            ClientesGravados: chavesDeCliente.Count,
            EnderecosGravados: enderecos.Gravados,
            EnderecosComMunicipioPeloPonteiro: enderecos.PeloPonteiro,
            EnderecosComMunicipioPeloNome: enderecos.PeloNome,
            EnderecosSemMunicipio: enderecos.SemMunicipio,
            MunicipiosReconhecidosPelaChave: catalogoDeMunicipios.ReconhecidosPelaChave,
            CorrecoesDeGrafiaMantidas: enderecos.CorrecoesDeGrafiaMantidas,
            GrafiasCortadasReapontadas: grafias.Reapontados,
            GrafiasCortadasPendentes: grafias.Pendentes,
            ContornoOficialDisponivel: grafias.ContornoDisponivel,
            ContatosLidos: contatos.Valor.LinhasLidas,
            ContatosGravados: gravadosDeContato,
            EquipamentosLidos: equipamentos.Valor.LinhasLidas,
            EquipamentosGravados: gravadosDeEquipamento,
            Recusadas: _recusas.Count,
            Saneamento: _saneamento.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal),
            RecusasPorMotivo: _recusas
                .GroupBy(r => $"{r.Entidade} — {r.Categoria}", StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal),
            ExemplosDeRecusa: _recusas
                .GroupBy(r => $"{r.Entidade} — {r.Categoria}", StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => Resumir(g.First().Motivo), StringComparer.Ordinal)));
    }

    // =============================================================================================
    // Cliente
    // =============================================================================================

    private async Task<Dictionary<string, long>> GravarClientesAsync(
        int sistemaId, IReadOnlyList<ClienteParaCarga> clientes, CancellationToken ct)
    {
        await using var leitura = AbrirContextoDaCarga();

        var mapa = await MapaDeChavesAsync(leitura, sistemaId, nameof(Cliente), ct);

        // QUEM E O DONO DE CADA DOCUMENTO, e nao so "este documento ja foi usado".
        //
        // Guardar apenas o conjunto de documentos usados basta para recusar uma criacao e NAO
        // basta para a reconciliacao: na segunda execucao o cliente ja existe, e perguntar "este
        // CNPJ ja esta em uso?" responderia sim - por causa dele mesmo. E preciso saber se o dono
        // e ele ou outro. Foi exatamente assim que a primeira reexecucao desta carga estourou o
        // indice unico, e e por isso que a reexecucao faz parte da verificacao.
        var donoDoDocumento = (await leitura.Clientes.AsNoTracking()
                .Where(c => c.Documento != null)
                .Select(c => new { c.Id, c.EmpresaId, c.Documento })
                .ToListAsync(ct))
            .ToDictionary(x => (x.EmpresaId, x.Documento!.Value.Numero), x => x.Id);

        var gravados = 0;

        foreach (var bloco in clientes.Chunk(TamanhoDoBloco))
        {
            await using var contexto = AbrirContextoDaCarga();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novos = new List<(string Chave, Cliente Entidade)>();
            var reconciliados = new List<string>();

            foreach (var cliente in bloco)
            {
                if (!deParaDeFiliais.TryGetValue(cliente.CodigoDaFilialNoLegado, out var empresaId))
                {
                    Recusar(nameof(Cliente), "Filial da origem sem correspondência no CRM", cliente.ChaveDeOrigem,
                        $"A filial {cliente.CodigoDaFilialNoLegado} do sistema de origem não " +
                        "corresponde a nenhuma filial em operação do CRM.", cliente);
                    continue;
                }

                if (mapa.TryGetValue(cliente.ChaveDeOrigem, out var existenteId))
                {
                    var entidade = await contexto.Clientes.FirstOrDefaultAsync(c => c.Id == existenteId, ct);

                    if (entidade is { EstaExcluido: false })
                    {
                        entidade.Alterar(
                            cliente.NomeRazao, cliente.TipoDePessoa, usuarioResponsavelId,
                            usuarioResponsavelId,
                            DocumentoQuePodeFicar(
                                cliente.Documento, entidade.Documento, entidade.Id, empresaId,
                                donoDoDocumento, nameof(Cliente), cliente.ChaveDeOrigem, cliente),
                            cliente.NomeFantasia,
                            cliente.InscricaoEstadual, cliente.AtividadeEconomica);

                        entidade.MudarSituacao(cliente.Situacao, usuarioResponsavelId);
                        reconciliados.Add(cliente.ChaveDeOrigem);
                    }

                    continue;
                }

                // A UNICIDADE DE DOCUMENTO POR FILIAL é índice no banco. Conferir aqui não é
                // duplicar a regra: é o que faz a recusa chegar como "este CNPJ já está em outro
                // cliente desta filial", com a linha crua guardada, em vez de uma violação de
                // índice que derruba o bloco inteiro e não diz de quem era a culpa.
                if (cliente.Documento is { } documento
                    && !donoDoDocumento.TryAdd((empresaId, documento.Numero), ReservadoNestaRodada))
                {
                    Recusar(nameof(Cliente), "Documento repetido na mesma filial", cliente.ChaveDeOrigem,
                        $"O documento {documento.Formatado()} já está em outro cliente desta " +
                        "filial. O sistema de origem não valida documento na entrada e admite a " +
                        "mesma pessoa cadastrada duas vezes; aqui o índice único não admite.",
                        cliente);
                    continue;
                }

                var novo = Cliente.Criar(
                    empresaId, cliente.NomeRazao, cliente.TipoDePessoa, usuarioResponsavelId,
                    usuarioResponsavelId, cliente.Documento, cliente.NomeFantasia, cliente.Situacao,
                    cliente.InscricaoEstadual, cliente.AtividadeEconomica);

                contexto.Clientes.Add(novo);
                novos.Add((cliente.ChaveDeOrigem, novo));
            }

            await contexto.SaveChangesAsync(ct);

            foreach (var (chave, entidade) in novos)
            {
                contexto.ChavesExternas.Add(
                    ChaveExterna.Criar(sistemaId, nameof(Cliente), entidade.Id, chave));
                mapa[chave] = entidade.Id;
            }

            await CarimbarConciliacaoAsync(contexto, sistemaId, nameof(Cliente), reconciliados, ct);
            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);

            gravados += novos.Count + reconciliados.Count;
            relatar($"  clientes: {gravados}/{clientes.Count}");
        }

        return mapa;
    }

    // =============================================================================================
    // Município — o catálogo nacional que fecha o campo do endereço
    // =============================================================================================

    /// <summary>
    /// Grava o catálogo de municípios e devolve as duas formas de encontrá-lo depois.
    ///
    /// <para><b>A DEDUPLICAÇÃO É O CORAÇÃO DESTE MÉTODO.</b> A origem tem 180 pares
    /// (nome, UF) repetidos, somando 415 linhas — <c>CATANDUVA/SP</c> aparece mais de uma vez com
    /// sequenciais diferentes. Do lado de cá o índice <c>UX_Municipio_Uf_Nome</c> não admite
    /// isso, e é bom que não admita: duas linhas para a mesma cidade são duas verdades. A carga
    /// escolhe a de MENOR sequencial como canônica — a consulta já vem ordenada assim, o que
    /// torna a escolha determinística entre execuções — e aponta TODAS as chaves de origem do
    /// grupo para ela no de-para. Nenhum vínculo se perde: um endereço que apontava para a
    /// segunda linha chega no mesmo município.</para>
    ///
    /// <para>Idempotente pelo mesmo mecanismo de sempre: uma linha em
    /// <c>integracao.ChaveExterna</c> por chave de origem, e o município já existente é
    /// reencontrado por nome mais UF quando o de-para ainda não o conhece — que é o caso da
    /// primeira execução depois de um seed manual.</para>
    /// </summary>
    private async Task<CatalogoDeMunicipios> GravarMunicipiosAsync(
        int sistemaId, IReadOnlyList<MunicipioParaCarga> municipios, CancellationToken ct)
    {
        await using var contexto = AbrirContextoDaCarga();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var porChave = await MapaDeChavesAsync(contexto, sistemaId, nameof(Municipio), ct);

        var porNomeEUf = (await contexto.Municipios.AsNoTracking()
                .Select(m => new { m.Id, m.Nome, m.Uf })
                .ToListAsync(ct))
            .GroupBy(m => ChaveDeMunicipio(m.Uf, m.Nome), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.Ordinal);

        var novos = new List<(string Chave, Municipio Entidade)>();
        var chavesDeGrupo = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var duplicados = 0;
        var reconhecidosPelaChave = 0;

        foreach (var municipio in municipios)
        {
            var identidade = ChaveDeMunicipio(municipio.Uf, municipio.Nome);

            // A SEGUNDA LINHA DO GRUPO NÃO CRIA MUNICÍPIO — só entra no de-para apontando para o
            // mesmo. É aqui que os 180 pares repetidos da origem (BOA VISTA/AL entre eles) viram
            // uma linha só. Sem esta guarda, a checagem seguinte não bastaria: `porNomeEUf` só
            // conhece o que JÁ ESTÁ no banco, e as duas linhas do mesmo grupo chegariam juntas ao
            // mesmo SaveChanges, onde o índice único as recusaria no meio do lote.
            var jaVistaNestaRodada = chavesDeGrupo.TryGetValue(identidade, out var doGrupo);

            if (!jaVistaNestaRodada) chavesDeGrupo[identidade] = doGrupo = [];

            doGrupo!.Add(municipio.ChaveDeOrigem);

            if (jaVistaNestaRodada)
            {
                duplicados++;
                continue;
            }

            if (porNomeEUf.ContainsKey(identidade)) continue;

            // A LINHA QUE A ORIGEM JÁ CONHECE PELA CHAVE NÃO É RECRIADA. O reconhecimento do IBGE
            // renomeia a linha do catálogo ("SANTA CRUZ DA ESPERA" vira "Santa Cruz da Esperança"), e a
            // busca por nome deixa de achá-la; sem esta guarda, a recarga criaria de novo a linha com o
            // nome cortado, sem código, e o catálogo ganharia uma duplicata por município renomeado. A
            // chave de origem é a identidade do próprio registro — não é casamento por semelhança.
            if (porChave.TryGetValue(municipio.ChaveDeOrigem, out var conhecidoPelaChave))
            {
                porNomeEUf[identidade] = (int)conhecidoPelaChave;
                reconhecidosPelaChave++;
                continue;
            }

            var novo = Municipio.Criar(municipio.Nome, municipio.Uf);
            contexto.Municipios.Add(novo);
            novos.Add((identidade, novo));
        }

        await contexto.SaveChangesAsync(ct);

        foreach (var (identidade, entidade) in novos) porNomeEUf[identidade] = entidade.Id;

        // O DE-PARA GANHA UMA LINHA POR CHAVE DE ORIGEM, inclusive as duplicadas — é isso que
        // faz um endereço apontando para a segunda linha de CATANDUVA/SP chegar no mesmo
        // município que o que aponta para a primeira.
        foreach (var (identidade, chaves) in chavesDeGrupo)
        {
            if (!porNomeEUf.TryGetValue(identidade, out var municipioId)) continue;

            foreach (var chave in chaves.Where(c => !porChave.ContainsKey(c)))
            {
                contexto.ChavesExternas.Add(
                    ChaveExterna.Criar(sistemaId, nameof(Municipio), municipioId, chave));
                porChave[chave] = municipioId;
            }
        }

        await contexto.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        var oficialDaGrafiaCortada = await ConsolidacaoDeGrafiasCortadas.OficialDeCadaGrafiaAsync(contexto, ct);

        relatar($"  municípios: {porNomeEUf.Values.Distinct().Count()} no catálogo, {novos.Count} novos nesta rodada, " +
                $"{duplicados} chave(s) de origem apontando para município que já existia, " +
                $"{reconhecidosPelaChave} reencontrado(s) pela chave depois de renomeado(s) pelo IBGE, " +
                $"{oficialDaGrafiaCortada.Count} grafia(s) cortada(s) com município oficial.");

        return new CatalogoDeMunicipios(
            porChave.ToDictionary(p => p.Key, p => (int)p.Value, StringComparer.Ordinal),
            porNomeEUf,
            duplicados,
            reconhecidosPelaChave,
            oficialDaGrafiaCortada);
    }

    /// <summary>
    /// A IDENTIDADE DE UM MUNICÍPIO em memória, do mesmo jeito que o banco a enxerga.
    ///
    /// <para>O índice <c>UX_Municipio_Uf_Nome</c> vive sob a colação <c>Latin1_General_CI_AI</c>,
    /// que ignora caixa E acento: para o banco, <c>Cajuru</c>, <c>CAJURU</c> e <c>Cajurú</c> são
    /// o mesmo valor. Se a deduplicação em memória usasse comparação comum, a carga tentaria
    /// gravar os três e receberia violação de índice único no meio do lote. Esta função reproduz
    /// a regra do banco — maiúsculas, sem acento, espaço colapsado — para que a decisão seja
    /// tomada aqui, antes de o banco precisar recusar.</para>
    /// </summary>
    internal static string ChaveDeMunicipio(string uf, string nome)
    {
        var decomposto = nome.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);

        var semAcento = new StringBuilder(decomposto.Length);
        var espacoPendente = false;

        foreach (var letra in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(letra) == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsWhiteSpace(letra))
            {
                espacoPendente = semAcento.Length > 0;
                continue;
            }

            if (espacoPendente)
            {
                semAcento.Append(' ');
                espacoPendente = false;
            }

            semAcento.Append(letra);
        }

        return $"{uf.Trim().ToUpperInvariant()}/{semAcento.ToString().Normalize(NormalizationForm.FormC)}";
    }

    // =============================================================================================
    // Endereço
    // =============================================================================================

    /// <summary>
    /// Grava os endereços, LIGANDO CADA UM AO MUNICÍPIO DO CATÁLOGO.
    ///
    /// <para><b>Duas tentativas, nesta ordem, e a ordem é a da confiança.</b> Primeiro o
    /// ponteiro que a origem já mantém (<c>GE_Pessoa.SeqCidade</c> → <c>GE_Cidade</c>), que
    /// está preenchido em 96% dos cadastros e é o que o sistema de origem de fato usa. Só quando
    /// ele não resolve — nulo, zero, ou apontando para uma cidade que a carga recusou por UF
    /// inválida — a carga tenta casar pelo TEXTO, por nome mais UF, contra o mesmo catálogo.
    /// </para>
    ///
    /// <para><b>O que não casa não é inventado nem descartado:</b> o endereço entra com o texto
    /// do legado em <c>Municipio</c> e <c>MunicipioId</c> nulo, que é o resíduo de migração que a
    /// restrição <c>CK_Endereco_Municipio</c> admite e o documento 26, seção 7, conta. As três
    /// contagens devolvidas por este método são exatamente o que aquela seção publica.</para>
    /// </summary>
    private async Task<ResultadoDeEnderecos> GravarEnderecosAsync(
        int sistemaId,
        IReadOnlyList<ClienteParaCarga> clientes,
        IReadOnlyDictionary<string, long> chavesDeCliente,
        CatalogoDeMunicipios catalogo,
        CancellationToken ct)
    {
        var comEndereco = clientes.Where(c => c.Endereco is not null).ToList();

        await using var leitura = AbrirContextoDaCarga();
        var mapa = await MapaDeChavesAsync(leitura, sistemaId, nameof(Endereco), ct);

        var gravados = 0;
        var peloPonteiro = 0;
        var peloNome = 0;
        var semMunicipio = 0;
        var correcoesMantidas = 0;
        var correcoesDesfeitas = 0;

        foreach (var bloco in comEndereco.Chunk(TamanhoDoBloco))
        {
            await using var contexto = AbrirContextoDaCarga();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novos = new List<(string Chave, Endereco Entidade)>();
            var reconciliados = new List<string>();

            foreach (var cliente in bloco)
            {
                if (!chavesDeCliente.TryGetValue(cliente.ChaveDeOrigem, out var clienteId)) continue;
                if (!deParaDeFiliais.TryGetValue(cliente.CodigoDaFilialNoLegado, out var empresaId)) continue;

                var dado = cliente.Endereco!;

                var municipioId = ResolverMunicipio(dado, catalogo, out var comoResolveu);

                switch (comoResolveu)
                {
                    case FormaDeResolucao.Ponteiro: peloPonteiro++; break;
                    case FormaDeResolucao.Nome: peloNome++; break;
                    default: semMunicipio++; break;
                }

                var municipio = municipioId is { } achado
                    ? MunicipioDoEndereco.Selecionado(achado)
                    : MunicipioDoEndereco.NaoIdentificadoNaCarga(dado.Municipio);

                if (mapa.TryGetValue(cliente.ChaveDeOrigem, out var existenteId))
                {
                    var entidade = await contexto.Enderecos.FirstOrDefaultAsync(e => e.Id == existenteId, ct);

                    if (entidade is { EstaExcluido: false })
                    {
                        // A CORREÇÃO DE GRAFIA CORTADA NÃO SE DESFAZ NA RECARGA quando a evidência que a
                        // sustentou é a mesma: a origem ainda aponta para a linha cortada, e o endereço já
                        // está no município oficial dela, com a mesma UF e a mesma coordenada. Qualquer
                        // diferença grava o que a origem diz, e a conferência ao fim da carga reexamina.
                        var municipioGravado = municipio;

                        if (municipioId is { } daOrigem
                            && catalogo.OficialDaGrafiaCortada.TryGetValue(daOrigem, out var oficial))
                        {
                            if (entidade.CorrecaoDeGrafiaCortadaSeMantem(oficial, dado.Uf, dado.Latitude, dado.Longitude))
                            {
                                municipioGravado = MunicipioDoEndereco.Selecionado(oficial);
                                correcoesMantidas++;
                            }
                            else if (entidade.MunicipioId == oficial)
                            {
                                // A CORREÇÃO QUE SE DESFAZ TAMBÉM VAI PARA A TRILHA — agora pelo SaveChanges
                                // (documento 41, fase 2): a origem mudou a UF ou a coordenada, o endereço volta
                                // para a linha que ela aponta, e a mudança de MunicipioId é auditada.
                                correcoesDesfeitas++;
                            }
                        }

                        entidade.Alterar(
                            TipoDeEndereco.Fiscal, dado.Logradouro, municipioGravado, dado.Uf,
                            usuarioResponsavelId, dado.Numero, dado.Complemento, dado.Bairro,
                            dado.Cep, identificacao: null, dado.Latitude, dado.Longitude);

                        reconciliados.Add(cliente.ChaveDeOrigem);
                    }

                    continue;
                }

                var novo = Endereco.Criar(
                    empresaId, clienteId, TipoDeEndereco.Fiscal, dado.Logradouro, municipio,
                    dado.Uf, usuarioResponsavelId, dado.Numero, dado.Complemento, dado.Bairro,
                    dado.Cep, identificacao: null, ehPrincipal: true, dado.Latitude, dado.Longitude);

                contexto.Enderecos.Add(novo);
                novos.Add((cliente.ChaveDeOrigem, novo));
            }

            await contexto.SaveChangesAsync(ct);

            foreach (var (chave, entidade) in novos)
                contexto.ChavesExternas.Add(
                    ChaveExterna.Criar(sistemaId, nameof(Endereco), entidade.Id, chave));

            await CarimbarConciliacaoAsync(contexto, sistemaId, nameof(Endereco), reconciliados, ct);
            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);

            gravados += novos.Count + reconciliados.Count;
            relatar($"  endereços: {gravados}/{comEndereco.Count}");
        }

        relatar($"  município do endereço: {peloPonteiro} pelo ponteiro da origem, {peloNome} " +
                $"pelo nome mais UF, {semMunicipio} sem casar (ficaram com o texto do legado), " +
                $"{correcoesMantidas} com a correção de grafia cortada mantida, {correcoesDesfeitas} com a " +
                "correção desfeita porque a origem mudou (com trilha).");

        return new ResultadoDeEnderecos(gravados, peloPonteiro, peloNome, semMunicipio, correcoesMantidas);
    }

    /// <summary>Como o município do endereço foi encontrado — ou não foi.</summary>
    private enum FormaDeResolucao
    {
        /// <summary>Não casou com município nenhum: sobrou o texto do legado.</summary>
        NaoCasou = 0,

        /// <summary>Pelo ponteiro que a origem já mantém — o caminho de confiança.</summary>
        Ponteiro = 1,

        /// <summary>Pelo nome mais UF, quando o ponteiro não resolveu.</summary>
        Nome = 2
    }

    private static int? ResolverMunicipio(
        EnderecoParaCarga endereco, CatalogoDeMunicipios catalogo, out FormaDeResolucao forma)
    {
        if (endereco.ChaveDoMunicipioDeOrigem is { } chave
            && catalogo.PorChaveDeOrigem.TryGetValue(chave, out var peloPonteiro))
        {
            forma = FormaDeResolucao.Ponteiro;
            return peloPonteiro;
        }

        if (catalogo.PorNomeEUf.TryGetValue(
                ChaveDeMunicipio(endereco.Uf, endereco.Municipio), out var peloNome))
        {
            forma = FormaDeResolucao.Nome;
            return peloNome;
        }

        forma = FormaDeResolucao.NaoCasou;
        return null;
    }

    // =============================================================================================
    // Contato
    // =============================================================================================

    private async Task<int> GravarContatosAsync(
        int sistemaId,
        int papelId,
        IReadOnlyList<ContatoParaCarga> contatos,
        IReadOnlyDictionary<string, long> chavesDeCliente,
        CancellationToken ct)
    {
        await using var leitura = AbrirContextoDaCarga();

        var mapa = await MapaDeChavesAsync(leitura, sistemaId, nameof(Contato), ct);

        var donoDoDocumento = (await leitura.Contatos.AsNoTracking()
                .Where(c => c.Documento != null)
                .Select(c => new { c.Id, c.EmpresaId, c.Documento })
                .ToListAsync(ct))
            .ToDictionary(x => (x.EmpresaId, x.Documento!.Value.Numero), x => x.Id);

        var empresaPorCliente = await MapaDeEmpresaPorClienteAsync(leitura, ct);

        var vinculos = (await leitura.ClienteContatos.AsNoTracking()
                .Select(v => new { v.ClienteId, v.ContatoId })
                .ToListAsync(ct))
            .Select(v => (v.ClienteId, v.ContatoId))
            .ToHashSet();

        var gravados = 0;

        foreach (var bloco in contatos.Chunk(TamanhoDoBloco))
        {
            await using var contexto = AbrirContextoDaCarga();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novos = new List<(string Chave, long ClienteId, Contato Entidade)>();
            var reconciliados = new List<string>();

            foreach (var contato in bloco)
            {
                if (!chavesDeCliente.TryGetValue(contato.ChaveDoClienteDeOrigem, out var clienteId))
                {
                    Recusar(nameof(Contato), "Cliente dono fora da carga", contato.ChaveDeOrigem,
                        "O cliente dono deste contato não entrou na carga — ou foi recusado, ou " +
                        "está fora do recorte. Contato sem cliente ficaria órfão, que é " +
                        "exatamente o defeito medido no sistema de origem.", contato);
                    continue;
                }

                var empresaId = empresaPorCliente[clienteId];

                if (mapa.TryGetValue(contato.ChaveDeOrigem, out var existenteId))
                {
                    var entidade = await contexto.Contatos.FirstOrDefaultAsync(c => c.Id == existenteId, ct);

                    if (entidade is { EstaExcluido: false })
                    {
                        entidade.Alterar(
                            contato.Nome, usuarioResponsavelId, contato.Sobrenome,
                            DocumentoQuePodeFicar(
                                contato.Documento, entidade.Documento, entidade.Id, empresaId,
                                donoDoDocumento, nameof(Contato), contato.ChaveDeOrigem, contato),
                            contato.Cargo);

                        reconciliados.Add(contato.ChaveDeOrigem);
                    }

                    continue;
                }

                var documento = contato.Documento;
                if (documento is { } cpf
                    && !donoDoDocumento.TryAdd((empresaId, cpf.Numero), ReservadoNestaRodada))
                {
                    // O CONTATO ENTRA MESMO ASSIM, sem o CPF. Diferente do cliente, o documento
                    // não é o que identifica o contato — o nome e o vínculo são. Perder a pessoa
                    // porque o CPF dela já aparece noutro cadastro seria jogar fora mais do que
                    // se ganha; o campo sai vazio e a recusa fica escrita.
                    documento = null;
                    Recusar(nameof(Contato), "CPF repetido na mesma filial (contato entrou sem documento)",
                        contato.ChaveDeOrigem, $"O CPF {cpf.Formatado()} já está em outro contato desta filial. O " +
                        "contato entrou sem documento em vez de ser descartado.", contato);
                }

                var novo = Contato.Criar(
                    empresaId, contato.Nome, usuarioResponsavelId, usuarioResponsavelId,
                    contato.Sobrenome, documento, contato.Cargo);

                contexto.Contatos.Add(novo);
                novos.Add((contato.ChaveDeOrigem, clienteId, novo));
            }

            await contexto.SaveChangesAsync(ct);

            foreach (var (chave, clienteId, entidade) in novos)
            {
                contexto.ChavesExternas.Add(
                    ChaveExterna.Criar(sistemaId, nameof(Contato), entidade.Id, chave));

                if (vinculos.Add((clienteId, entidade.Id)))
                    contexto.ClienteContatos.Add(
                        ClienteContato.Criar(clienteId, entidade.Id, papelId));

                mapa[chave] = entidade.Id;
            }

            await CarimbarConciliacaoAsync(contexto, sistemaId, nameof(Contato), reconciliados, ct);
            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);

            gravados += novos.Count + reconciliados.Count;
            relatar($"  contatos: {gravados}/{contatos.Count}");
        }

        return gravados;
    }

    // =============================================================================================
    // Equipamento
    // =============================================================================================

    private async Task<int> GravarEquipamentosAsync(
        int sistemaId,
        IReadOnlyList<EquipamentoParaCarga> equipamentos,
        IReadOnlyDictionary<string, long> chavesDeCliente,
        CancellationToken ct)
    {
        var catalogo = new CatalogoDeFrotaDaCarga();

        await using (var contextoDoCatalogo = AbrirContextoDaCarga())
        {
            await catalogo.CarregarAsync(contextoDoCatalogo, ct);

            // O CATÁLOGO É RESOLVIDO ANTES, e de uma vez: resolver marca, família e modelo dentro
            // do laço de gravação misturaria linhas de catálogo na transação das máquinas, e um
            // bloco desfeito levaria junto um catálogo que os blocos seguintes já esperavam achar.
            foreach (var trio in equipamentos
                         .Select(e => (e.Marca, e.Categoria, e.Modelo))
                         .Distinct())
                await catalogo.ResolverModeloAsync(
                    contextoDoCatalogo, trio.Marca, trio.Categoria, trio.Modelo, ct);
        }

        relatar($"  catálogo de frota: {catalogo.MarcasCriadas} marca(s), " +
                $"{catalogo.FamiliasCriadas} família(s) e {catalogo.ModelosCriados} modelo(s) " +
                "nasceram do dado real.");

        await using var leitura = AbrirContextoDaCarga();

        var mapa = await MapaDeChavesAsync(leitura, sistemaId, nameof(Equipamento), ct);

        var empresaPorCliente = await MapaDeEmpresaPorClienteAsync(leitura, ct);

        var chassisUsados = (await leitura.Equipamentos.AsNoTracking()
                .Select(e => e.Chassi)
                .ToListAsync(ct))
            .Select(c => c.Numero)
            .ToHashSet(StringComparer.Ordinal);

        var modelos = new Dictionary<(string, string, string), int>();
        var gravados = 0;

        foreach (var bloco in equipamentos.Chunk(TamanhoDoBloco))
        {
            await using var contexto = AbrirContextoDaCarga();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            var novos = new List<(string Chave, Equipamento Entidade)>();
            var reconciliados = new List<string>();

            foreach (var maquina in bloco)
            {
                if (!chavesDeCliente.TryGetValue(maquina.ChaveDoClienteDeOrigem, out var clienteId))
                {
                    Recusar(nameof(Equipamento), "Cliente dono fora da carga", maquina.ChaveDeOrigem,
                        "O cliente dono desta máquina não entrou na carga — ou foi recusado, ou " +
                        "está fora do recorte. Máquina em operação sem dono não existe.", maquina);
                    continue;
                }

                var empresaId = empresaPorCliente[clienteId];

                var chaveDoModelo = (maquina.Marca, maquina.Categoria, maquina.Modelo);
                if (!modelos.TryGetValue(chaveDoModelo, out var modeloId))
                {
                    modeloId = await catalogo.ResolverModeloAsync(
                        contexto, maquina.Marca, maquina.Categoria, maquina.Modelo, ct);
                    modelos[chaveDoModelo] = modeloId;
                }

                // Máquina que a origem não considera ativa entra BAIXADA, não some: o parque
                // histórico é informação, e a tela distingue as duas pela situação.
                var situacao = maquina.EstaAtivo
                    ? SituacaoDoEquipamento.Ativo
                    : SituacaoDoEquipamento.Baixado;

                if (mapa.TryGetValue(maquina.ChaveDeOrigem, out var existenteId))
                {
                    var entidade = await contexto.Equipamentos
                        .FirstOrDefaultAsync(e => e.Id == existenteId, ct);

                    if (entidade is { EstaExcluido: false })
                    {
                        entidade.Alterar(
                            modeloId, usuarioResponsavelId, clienteId, situacao,
                            anoFabricacao: null, maquina.Ano, numeroSerie: null, placa: null,
                            maquina.LocalizacaoDescrita);

                        reconciliados.Add(maquina.ChaveDeOrigem);
                    }

                    continue;
                }

                // O CHASSI É A CHAVE DE DEDUPLICAÇÃO (documento 16, seção 4). Duas linhas do
                // legado com o mesmo chassi são a mesma máquina física cadastrada duas vezes —
                // e é justamente isso que o índice único do banco novo existe para impedir.
                if (!chassisUsados.Add(maquina.Chassi.Numero))
                {
                    Recusar(nameof(Equipamento), "Chassi repetido", maquina.ChaveDeOrigem,
                        $"O chassi {maquina.Chassi.Numero} já está em outra máquina do CRM. " +
                        "Chassi repetido é a mesma máquina física cadastrada duas vezes na origem.",
                        maquina);
                    continue;
                }

                var novo = Equipamento.Criar(
                    empresaId, modeloId, maquina.Chassi, OrigemDoEquipamento.Crm,
                    usuarioResponsavelId, clienteId, situacao, anoFabricacao: null,
                    anoModelo: maquina.Ano, numeroSerie: null, placa: null,
                    localizacaoDescrita: maquina.LocalizacaoDescrita);

                contexto.Equipamentos.Add(novo);
                novos.Add((maquina.ChaveDeOrigem, novo));
            }

            await contexto.SaveChangesAsync(ct);

            foreach (var (chave, entidade) in novos)
            {
                contexto.ChavesExternas.Add(
                    ChaveExterna.Criar(sistemaId, nameof(Equipamento), entidade.Id, chave));
                mapa[chave] = entidade.Id;
            }

            await CarimbarConciliacaoAsync(contexto, sistemaId, nameof(Equipamento), reconciliados, ct);
            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);

            gravados += novos.Count + reconciliados.Count;
            relatar($"  equipamentos: {gravados}/{equipamentos.Count}");
        }

        return gravados;
    }

    // =============================================================================================
    // A fila de descarte, o de-para e a marca de sincronismo
    // =============================================================================================

    private async Task GravarRecusasAsync(CancellationToken ct)
    {
        await using var contexto = AbrirContextoDaCarga();

        // A FILA DE DESCARTE É O RETRATO DA ÚLTIMA RODADA. As linhas ainda não tratadas do fluxo
        // saem antes das novas entrarem, senão rodar de novo empilharia a mesma recusa duas
        // vezes e a contagem deixaria de significar alguma coisa. O que alguém JÁ TRATOU fica —
        // apagar o trabalho de quem tratou seria pior do que a duplicata.
        await contexto.Database.ExecuteSqlRawAsync(
            "DELETE FROM integracao.MensagemDescartada WHERE Fluxo LIKE 'VORTICE.CARGA.%' " +
            "AND TratadaEm IS NULL", ct);

        foreach (var bloco in _recusas.Chunk(TamanhoDoBloco))
        {
            foreach (var recusa in bloco)
                contexto.MensagensDescartadas.Add(MensagemDescartada.Criar(
                    Fluxo(recusa.Entidade),
                    recusa.ConteudoJson,
                    recusa.Motivo,
                    tentativas: 1));

            await contexto.SaveChangesAsync(ct);
        }

        relatar($"  fila de descarte: {_recusas.Count} linha(s) recusadas, com motivo e conteúdo cru.");
    }

    private static string Fluxo(string entidade) => entidade switch
    {
        nameof(Municipio) => FluxoDeMunicipio,
        nameof(Cliente) => FluxoDeCliente,
        nameof(Endereco) => FluxoDeEndereco,
        nameof(Equipamento) => FluxoDeEquipamento,
        _ => FluxoDeContato
    };

    private async Task MarcarSincronismoAsync<T>(
        int sistemaId, string fluxo, LoteDaCarga<T> lote, int gravados, CancellationToken ct)
    {
        await using var contexto = AbrirContextoDaCarga();

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

    /// <summary>
    /// A filial de cada cliente, num mapa só.
    ///
    /// Perguntar ao banco a filial de cada contato e de cada máquina, uma linha por vez, seriam
    /// mais de quarenta mil viagens de ida e volta para responder algo que cabe inteiro na
    /// memória — e o tempo de carga passaria de minutos a horas. É a mesma lição do N+1 que a
    /// listagem da API resolve com junção.
    /// </summary>
    /// <summary>
    /// Marca de "reservado nesta rodada, identificador ainda nao gerado".
    ///
    /// Um registro criado no bloco atual so ganha identificador depois da gravacao, e ate la o
    /// documento dele ja precisa estar reservado - senao duas linhas do mesmo bloco com o mesmo
    /// documento passariam as duas e o indice unico derrubaria a transacao inteira.
    /// </summary>
    private const long ReservadoNestaRodada = 0;

    /// <summary>
    /// Decide qual documento pode ficar no registro que esta sendo RECONCILIADO.
    ///
    /// <para>Quando o documento que vem da origem ja pertence a OUTRO registro da mesma filial, o
    /// registro fica com o documento que ja tinha e a recusa e gravada. A alternativa - gravar
    /// assim mesmo - nao e uma alternativa: o indice unico derruba a transacao e o bloco inteiro
    /// se perde por causa de uma linha.</para>
    /// </summary>
    private CpfCnpj? DocumentoQuePodeFicar(
        CpfCnpj? daOrigem,
        CpfCnpj? oQueJaTem,
        long registroId,
        int empresaId,
        Dictionary<(int, string), long> donoDoDocumento,
        string entidade,
        string chave,
        object linha)
    {
        if (daOrigem is not { } documento) return oQueJaTem;

        var chaveDoDocumento = (empresaId, documento.Numero);

        if (!donoDoDocumento.TryGetValue(chaveDoDocumento, out var dono))
        {
            donoDoDocumento[chaveDoDocumento] = registroId;
            return documento;
        }

        if (dono == registroId) return documento;

        Recusar(entidade, "Documento repetido na mesma filial (registro manteve o que ja tinha)",
            chave,
            $"O documento {documento.Formatado()} ja esta em outro registro desta filial. A " +
            "reconciliacao manteve o documento que este registro ja tinha em vez de derrubar o " +
            "bloco no indice unico.", linha);

        return oQueJaTem;
    }

    private static async Task<Dictionary<long, int>> MapaDeEmpresaPorClienteAsync(
        CrmDbContext contexto, CancellationToken ct) =>
        await contexto.Clientes.AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.EmpresaId, ct);

    private static async Task<Dictionary<string, long>> MapaDeChavesAsync(
        CrmDbContext contexto, int sistemaId, string entidade, CancellationToken ct) =>
        await contexto.ChavesExternas.AsNoTracking()
            .Where(c => c.SistemaId == sistemaId && c.Entidade == entidade)
            .ToDictionaryAsync(c => c.ChaveOrigem, c => c.RegistroId, StringComparer.Ordinal, ct);

    private static async Task CarimbarConciliacaoAsync(
        CrmDbContext contexto, int sistemaId, string entidade, List<string> chaves, CancellationToken ct)
    {
        if (chaves.Count == 0) return;

        var linhas = await contexto.ChavesExternas
            .Where(c => c.SistemaId == sistemaId && c.Entidade == entidade && chaves.Contains(c.ChaveOrigem))
            .ToListAsync(ct);

        foreach (var linha in linhas) linha.MarcarSincronismo(DateTime.UtcNow);
    }

    private async Task<int> GarantirSistemaAsync(CancellationToken ct)
    {
        await using var contexto = AbrirContextoDaCarga();

        var sistema = await contexto.Sistemas
            .FirstOrDefaultAsync(s => s.Codigo == LeitorDeCargaDoVortice.CodigoDoSistema, ct);

        if (sistema is not null) return sistema.Id;

        sistema = Sistema.Criar(
            LeitorDeCargaDoVortice.CodigoDoSistema,
            "Vórtice CRM (sistema legado)",
            "SQL Server, somente leitura");

        contexto.Sistemas.Add(sistema);
        await contexto.SaveChangesAsync(ct);

        return sistema.Id;
    }

    private async Task<int> GarantirPapelAsync(CancellationToken ct)
    {
        await using var contexto = AbrirContextoDaCarga();

        var item = await contexto.CatalogoItens.FirstOrDefaultAsync(
            i => i.CatalogoId == CatalogosDeSistema.PapelDeContato && i.Codigo == PapelNaoInformado, ct);

        if (item is not null) return item.Id;

        item = CatalogoItem.Criar(
            CatalogosDeSistema.PapelDeContato, PapelNaoInformado,
            "Não informado na origem", ordem: 900);

        contexto.CatalogoItens.Add(item);
        await contexto.SaveChangesAsync(ct);

        return item.Id;
    }

    // =============================================================================================
    // Contabilidade do saneamento
    // =============================================================================================

    private void ContarSaneamento(
        IEnumerable<(IReadOnlyList<CorrecaoAplicada> Correcoes,
                     IReadOnlyList<CampoRejeitado> Rejeicoes)> linhas)
    {
        foreach (var (correcoes, rejeicoes) in linhas)
        {
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

    private void Recusar(string entidade, string categoria, string chave, string motivo, object linha)
    {
        _recusas.Add(new LinhaRecusada(entidade, categoria, chave, motivo, JsonSerializer.Serialize(new
        {
            chaveDeOrigem = chave,
            resumo = linha.ToString()
        })));
    }

    /// <summary>
    /// Encurta para caber numa linha de relatório — e COLAPSA a quebra de linha antes de cortar.
    ///
    /// Sem isso, um campo do legado com quebra de linha dentro (existem aos milhares) quebraria a
    /// tabela do relatório ao meio e o número ao lado ficaria órfão da própria linha.
    /// </summary>
    private static string Resumir(string texto)
    {
        var numaLinha = string.Join(' ', texto.Split(
            (char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return numaLinha.Length <= 90 ? numaLinha : numaLinha[..90] + "…";
    }
}

/// <summary>
/// O catálogo de municípios como a carga precisa dele: as duas formas de encontrar um.
/// </summary>
/// <param name="PorChaveDeOrigem">
/// Do sequencial da cidade na origem para o município do CRM. Chaves duplicadas da origem
/// apontam para o MESMO município — é o que preserva os vínculos das linhas repetidas.
/// </param>
/// <param name="PorNomeEUf">
/// Da identidade de negócio (UF mais nome, sem caixa nem acento) para o município do CRM. É o
/// caminho de reserva, para o endereço cujo ponteiro de cidade não resolve.
/// </param>
/// <param name="DuplicadosNaOrigem">
/// Quantas chaves de origem caíram num município que já existia — a medida da duplicação do
/// catálogo do sistema antigo.
/// </param>
/// <param name="ReconhecidosPelaChave">
/// Municípios que o reconhecimento do IBGE renomeou e que a carga reencontrou pela chave de origem, em
/// vez de recriar a linha com o nome cortado.
/// </param>
/// <param name="OficialDaGrafiaCortada">
/// Para cada linha cortada do catálogo, a linha do município oficial (documento 32, seção 4.6).
/// </param>
internal sealed record CatalogoDeMunicipios(
    IReadOnlyDictionary<string, int> PorChaveDeOrigem,
    IReadOnlyDictionary<string, int> PorNomeEUf,
    int DuplicadosNaOrigem,
    int ReconhecidosPelaChave,
    IReadOnlyDictionary<int, int> OficialDaGrafiaCortada);

/// <summary>O que a gravação de endereços fez, separando como o município foi resolvido.</summary>
/// <param name="Gravados">Endereços criados ou reconciliados.</param>
/// <param name="PeloPonteiro">Quantos acharam o município pelo ponteiro da origem.</param>
/// <param name="PeloNome">Quantos acharam o município pelo nome mais UF.</param>
/// <param name="SemMunicipio">Quantos não acharam e ficaram com o texto do legado.</param>
/// <param name="CorrecoesDeGrafiaMantidas">Quantos mantiveram a correção de grafia cortada.</param>
internal sealed record ResultadoDeEnderecos(
    int Gravados, int PeloPonteiro, int PeloNome, int SemMunicipio, int CorrecoesDeGrafiaMantidas);

/// <summary>Quantas vezes um campo foi corrigido e quantas foi recusado, com um exemplo de cada.</summary>
internal sealed class ContagemDeSaneamento
{
    /// <summary>Quantas linhas tiveram este campo normalizado.</summary>
    public int Corrigidos { get; set; }

    /// <summary>Quantas linhas tiveram este campo recusado.</summary>
    public int Recusados { get; set; }

    /// <summary>Um exemplo real da correção, para o relatório não ficar abstrato.</summary>
    public string? ExemploDeCorrecao { get; set; }

    /// <summary>Um exemplo real da recusa.</summary>
    public string? ExemploDeRecusa { get; set; }
}

/// <summary>O que a carga fez, em número — é o corpo do relatório do documento 24.</summary>
/// <param name="MunicipiosLidos">Linhas do catálogo de cidades lidas na origem.</param>
/// <param name="MunicipiosGravados">Municípios distintos no catálogo do CRM.</param>
/// <param name="MunicipiosDuplicadosNaOrigem">Chaves de origem que caíram num município já existente.</param>
/// <param name="ClientesLidos">Linhas de cadastro lidas na origem.</param>
/// <param name="ClientesGravados">Clientes criados ou reconciliados.</param>
/// <param name="EnderecosGravados">Endereços principais criados ou reconciliados.</param>
/// <param name="EnderecosComMunicipioPeloPonteiro">Endereços ligados ao município pelo ponteiro da origem.</param>
/// <param name="EnderecosComMunicipioPeloNome">Endereços ligados ao município por nome mais UF.</param>
/// <param name="EnderecosSemMunicipio">Endereços que ficaram com o texto do legado — o resíduo a zerar.</param>
/// <param name="MunicipiosReconhecidosPelaChave">Municípios renomeados pelo IBGE e reencontrados pela chave.</param>
/// <param name="CorrecoesDeGrafiaMantidas">Endereços que mantiveram a correção de grafia cortada.</param>
/// <param name="GrafiasCortadasReapontadas">Endereços corrigidos nesta rodada para o município oficial.</param>
/// <param name="GrafiasCortadasPendentes">Endereços em grafia cortada que ficaram para conferência.</param>
/// <param name="ContornoOficialDisponivel">Se a malha oficial do IBGE foi lida na conferência.</param>
/// <param name="ContatosLidos">Linhas de contato lidas na origem.</param>
/// <param name="ContatosGravados">Contatos criados ou reconciliados.</param>
/// <param name="EquipamentosLidos">Linhas de parque lidas na origem.</param>
/// <param name="EquipamentosGravados">Máquinas criadas ou reconciliadas.</param>
/// <param name="Recusadas">Total de linhas na fila de descarte.</param>
/// <param name="Saneamento">Contagem de correção e recusa por campo.</param>
/// <param name="RecusasPorMotivo">Contagem de recusa por regra que recusou.</param>
/// <param name="ExemplosDeRecusa">Um motivo real, com o valor que causou a recusa, por regra.</param>
internal sealed record ResumoDaCarga(
    int MunicipiosLidos,
    int MunicipiosGravados,
    int MunicipiosDuplicadosNaOrigem,
    int ClientesLidos,
    int ClientesGravados,
    int EnderecosGravados,
    int EnderecosComMunicipioPeloPonteiro,
    int EnderecosComMunicipioPeloNome,
    int EnderecosSemMunicipio,
    int MunicipiosReconhecidosPelaChave,
    int CorrecoesDeGrafiaMantidas,
    int GrafiasCortadasReapontadas,
    int GrafiasCortadasPendentes,
    bool ContornoOficialDisponivel,
    int ContatosLidos,
    int ContatosGravados,
    int EquipamentosLidos,
    int EquipamentosGravados,
    int Recusadas,
    IReadOnlyDictionary<string, ContagemDeSaneamento> Saneamento,
    IReadOnlyDictionary<string, int> RecusasPorMotivo,
    IReadOnlyDictionary<string, string> ExemplosDeRecusa);
