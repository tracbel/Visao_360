using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>
/// O FATURAMENTO E A CURVA ABC — o que dá letra ao cliente. <c>--somente-faturamento</c>, a rotina
/// <c>FATURAMENTO_PROTHEUS</c> do orquestrador.
///
/// <para><b>Código próprio, fora da carga do Vórtice.</b> Até 24/09/2026 esta etapa morava numa parte
/// de <see cref="CargaDeProcessoDoVortice"/>, a classe congelada na fase 1 (decisão D-12), só porque
/// nasceu lá. Ela não lê o Vórtice em lugar nenhum: a contraparte vem da <c>SD2</c> do Protheus, o
/// cliente vem do nosso cadastro e a classe sai do que acabou de ser gravado. Aqui a trilha de
/// auditoria diz a origem certa — o Protheus, e não o Vórtice.</para>
///
/// <para>Três etapas, nesta ordem, porque cada uma depende da anterior: grava o faturamento por
/// cliente, filial e mês; tira da janela o que a origem não tem mais; ordena os clientes por
/// faturamento e corta a curva em A, B e C. Quem não aparece no faturamento é D — e D aqui significa
/// "não comprou na janela", não "cliente ruim".</para>
///
/// <para><b>A curva é por FILIAL, e não da rede inteira.</b> Um cliente que responde por 3% do
/// faturamento de Votuporanga é grande em Votuporanga, e some no consolidado ao lado de Ribeirão
/// Preto. Como a carteira e o CEN são de uma filial, a classe tem de ser da mesma filial. A filial
/// é a que EMITIU a nota (<c>D2_FILIAL</c>) — medido em 24/09/2026, as dezesseis filiais faturam no
/// ERP, e não só a 010101 como se acreditava (ver <see cref="LeitorDeFaturamentoDoProtheus"/>).</para>
///
/// <para><b>O movimento promove a situação.</b> A carga de clientes da SA1 cria todo cliente como
/// <c>Suspect</c>, porque o cadastro diz que ele existe e não que comprou (ver
/// <see cref="CargaDeClientesDoProtheus"/>). Quem sabe que comprou é a nota: o cliente com venda na
/// janela passa de <c>Suspect</c> ou <c>Prospect</c> a <c>Cliente</c>. Ninguém é rebaixado aqui, e
/// <c>ClienteInativo</c> e <c>Encerrado</c> — decisões de pessoa — não são tocados.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="protheus">A leitura da SD2.</param>
/// <param name="usuarioResponsavelId">Quem responde pelas gravações.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDeFaturamentoDoProtheus(
    Func<CrmDbContext> abrirContexto,
    LeitorDeFaturamentoDoProtheus protheus,
    long usuarioResponsavelId,
    Action<string> relatar)
{
    private const int TamanhoDoBloco = 500;

    /// <summary>
    /// A maior fração da janela que uma execução pode remover. Nota cancelada e cliente que passou a
    /// existir mexem em poucos meses de cada vez; tirar um quinto da janela numa noite é leitura que
    /// veio pela metade, não o ERP mudando.
    /// </summary>
    public const double FracaoMaximaDeRemocao = 0.20;

    /// <summary>
    /// Abaixo disto a trava não se aplica: numa janela pequena (a primeira semana de uma filial, ou um
    /// teste) uma única nota cancelada já passa de 20%, e recusar seria falso alarme.
    /// </summary>
    public const int JanelaMinimaParaATrava = 1000;

    /// <summary>Se a remoção desta execução deve ser recusada como leitura parcial.</summary>
    public static bool RemocaoPassaDaTrava(int linhasNaJanela, int obsoletos) =>
        linhasNaJanela >= JanelaMinimaParaATrava && obsoletos > linhasNaJanela * FracaoMaximaDeRemocao;

    /// <summary>
    /// Quantos meses INTEIROS para trás a leitura e a curva ABC olham — três anos, mais o mês corrente.
    ///
    /// <para>A janela começa no dia 1: começar "hoje menos três anos" fazia o primeiro mês chegar pela
    /// metade e sobrescrever, com metade do valor, a competência que já estava inteira no banco.</para>
    /// </summary>
    internal const int MesesDeFaturamento = 36;

    /// <summary>Onde a classe A termina: os clientes que somam os primeiros 80% do faturamento.</summary>
    private const decimal CorteDaClasseA = 0.80m;

    /// <summary>Onde a classe B termina.</summary>
    private const decimal CorteDaClasseB = 0.95m;

    private readonly Dictionary<string, int> _decisoes = new(StringComparer.Ordinal);

    /// <summary>O sistema de origem, conhecido depois de garantido — a origem das gravações na trilha.</summary>
    private int? _sistemaId;

    /// <summary>O primeiro dia da janela: o dia 1 do mês, <see cref="MesesDeFaturamento"/> meses atrás.</summary>
    /// <param name="agoraUtc">O instante da carga.</param>
    internal static DateOnly InicioDaJanela(DateTime agoraUtc) =>
        new DateOnly(agoraUtc.Year, agoraUtc.Month, 1).AddMonths(-MesesDeFaturamento);

    /// <summary>Lê o faturamento do Protheus, grava e reapura a curva ABC.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ResumoDoFaturamento>> ExecutarAsync(CancellationToken ct)
    {
        relatar("Lendo o faturamento direto do banco do Protheus — três anos, para a curva ABC ter base…");

        var lido = await protheus.LerAsync(InicioDaJanela(DateTime.UtcNow), relatar, ct);
        if (!lido.EhSucesso) return Resultado<ResumoDoFaturamento>.Indisponivel(lido.Erro!);

        return Resultado<ResumoDoFaturamento>.Ok(await GravarAsync(lido.Valor, ct));
    }

    /// <summary>
    /// Grava um lote já lido — a carga inteira menos a leitura. É o que o teste exercita.
    /// </summary>
    /// <param name="lote">O que a leitura da SD2 produziu.</param>
    /// <param name="ct">Cancelamento.</param>
    internal async Task<ResumoDoFaturamento> GravarAsync(LoteDoProtheus lote, CancellationToken ct)
    {
        _sistemaId = await GarantirSistemaAsync(ct);

        RegistrarDescartesDaLeitura(lote);

        var gravacao = await GravarFaturamentoAsync(lote, ct);

        relatar(
            $"  {gravacao.Novos} mês(es) novo(s), {gravacao.Reapurados} reapurado(s) e {gravacao.Removidos} removido(s) " +
            $"da janela · nota mais recente: {lote.EmissaoMaisRecente:dd/MM/yyyy}.");

        var (curva, promovidos) = await ApurarCurvaAbcAsync(lote.Desde, ct);
        relatar(
            "  curva ABC: " +
            string.Join(" · ", curva.OrderBy(p => p.Key).Select(p => $"{p.Key} {p.Value}")) +
            $" · {promovidos} cliente(s) promovido(s) a Cliente pelo faturamento.");

        // AS DECISÕES SAEM JUNTO, e não só a contagem. Elas são o que diz quanto dinheiro ficou de fora
        // e por quê — sem isso o comando terminaria com "6.849 meses gravados" e o descarte voltaria a
        // ser invisível.
        return new ResumoDoFaturamento(
            gravacao.Novos, gravacao.Reapurados, gravacao.Removidos, promovidos, curva,
            new Dictionary<string, int>(_decisoes, StringComparer.Ordinal));
    }

    // =============================================================================================
    // A gravação
    // =============================================================================================

    private async Task<(int Novos, int Reapurados, int Removidos)> GravarFaturamentoAsync(
        LoteDoProtheus lote, CancellationToken ct)
    {
        var faturamento = lote.Faturamento;

        // A LEITURA VAZIA NÃO APAGA NADA. A janela é substituída pelo que a origem devolveu; uma
        // origem que devolve zero linha num ERP que emite nota todo dia é defeito de leitura, não
        // ausência de venda — e apagar três anos de faturamento por causa dele seria o pior desfecho.
        if (faturamento.Count == 0)
        {
            Decidir("Leitura do Protheus sem nenhuma venda na janela — nada foi gravado nem removido", 1);
            return (0, 0, 0);
        }

        var porDocumento = await MapaDeClientesPorDocumentoAsync(ct);
        var porCodigoDeEmpresa = await MapaDeEmpresasPorCodigoAsync(ct);

        int novos = 0, reapurados = 0;
        var documentosSemCliente = new HashSet<string>(StringComparer.Ordinal);

        // O DESCARTE TEM DE DIZER QUANTO DINHEIRO LEVOU JUNTO. Contar linha descartada não basta:
        // oitenta e quatro documentos parecem um resíduo, e eram mais de duzentos milhões de reais.
        var valorSemCliente = 0m;
        var filiaisSemEmpresa = new HashSet<string>(StringComparer.Ordinal);
        var valorSemEmpresa = 0m;

        // O QUE A LEITURA PRODUZIU, pela chave natural de cada tabela. É o que define a janela depois:
        // o que está no banco dentro dela e não está aqui não existe mais na origem.
        var produzidosComCliente = new HashSet<(long ClienteId, int EmpresaId, DateOnly Competencia)>();
        var produzidosSemCliente = new HashSet<(string Documento, int EmpresaId, DateOnly Competencia)>();

        foreach (var bloco in faturamento.Chunk(TamanhoDoBloco))
        {
            await using var contexto = AbrirContextoDaCarga();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            // A CHAVE NATURAL É (cliente, filial, competência), e é ela que torna a carga
            // reexecutável: quem já existe é reapurado, não duplicado.
            var resolvido = bloco
                .Select(f =>
                {
                    var (clienteId, empresaId) = Resolver(f, porDocumento, porCodigoDeEmpresa);
                    return (Linha: f, ClienteId: clienteId, EmpresaId: empresaId);
                })
                .ToList();

            foreach (var (linha, _, _) in resolvido.Where(x => x.ClienteId == 0))
            {
                documentosSemCliente.Add(linha.DocumentoDoCliente);
                valorSemCliente += linha.ValorLiquido;
            }

            foreach (var (linha, _, _) in resolvido.Where(x => x.EmpresaId == 0))
            {
                filiaisSemEmpresa.Add(linha.CodigoDaFilial ?? "(sem filial)");
                valorSemEmpresa += linha.ValorLiquido;
            }

            // QUEM NÃO TEM CLIENTE VAI PARA A OUTRA TABELA, e não para o lixo. Precisa da filial
            // resolvida — sem ela não há onde pendurar a linha.
            var semDono = resolvido.Where(x => x.ClienteId == 0 && x.EmpresaId != 0).ToList();
            foreach (var (linha, _, empresaId) in semDono)
                produzidosSemCliente.Add((linha.DocumentoDoCliente, empresaId, linha.Competencia));

            await GravarSemClienteAsync(contexto, semDono.Select(x => (x.Linha, x.EmpresaId)), lote.Desde, ct);

            var doBloco = resolvido.Where(x => x.ClienteId != 0 && x.EmpresaId != 0).ToList();
            if (doBloco.Count > 0)
            {
                var chaves = doBloco.Select(x => x.ClienteId).Distinct().ToList();
                var existentes = await contexto.FaturamentoDosClientes
                    .Where(f => chaves.Contains(f.ClienteId) && f.Competencia >= lote.Desde)
                    .ToDictionaryAsync(f => (f.ClienteId, f.EmpresaId, f.Competencia), ct);

                foreach (var (linha, clienteId, empresaId) in doBloco)
                {
                    var chave = (clienteId, empresaId, linha.Competencia);
                    produzidosComCliente.Add(chave);

                    if (existentes.TryGetValue(chave, out var jaExiste))
                    {
                        jaExiste.Reapurar(linha.ValorLiquido, linha.Notas, linha.Itens, linha.Quebra);
                        reapurados++;
                        continue;
                    }

                    contexto.FaturamentoDosClientes.Add(FaturamentoDoCliente.Criar(
                        empresaId, clienteId, linha.Competencia,
                        linha.ValorLiquido, linha.Notas, linha.Itens, linha.Quebra));
                    novos++;
                }
            }

            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        var removidos = await RemoverOQueSaiuDaOrigemAsync(lote.Desde, produzidosComCliente, produzidosSemCliente, ct);

        if (documentosSemCliente.Count > 0)
            Decidir(
                $"Documentos que faturam e NÃO existem no cadastro de clientes do CRM — " +
                $"{Reais(valorSemCliente)} de venda sem dono aqui dentro (gravados em FaturamentoSemCliente)",
                documentosSemCliente.Count);

        if (filiaisSemEmpresa.Count > 0)
            Decidir(
                $"Códigos de filial do faturamento sem empresa correspondente " +
                $"({string.Join(", ", filiaisSemEmpresa.Order(StringComparer.Ordinal))}) — {Reais(valorSemEmpresa)} descartados",
                filiaisSemEmpresa.Count);

        if (removidos > 0)
            Decidir(
                "Meses de faturamento removidos da janela porque a origem não os tem mais — nota cancelada, " +
                "ou cliente que passou a existir no CRM e saiu de FaturamentoSemCliente",
                removidos);

        return (novos, reapurados, removidos);
    }

    /// <summary>
    /// A JANELA É SUBSTITUÍDA, e não só acrescida — é isto que torna a carga idempotente por competência.
    ///
    /// <para>Sem esta etapa, a linha que mudou de lugar ficava nos dois. O caso que motivou: o
    /// faturamento roda antes da carga de clientes da SA1, e todo o dinheiro vai para
    /// <c>FaturamentoSemCliente</c>; no dia seguinte o cliente existe, a mesma venda entra em
    /// <c>FaturamentoDoCliente</c> — e a linha antiga continuava, contando o mesmo real duas vezes no
    /// cartão da diretoria. O mesmo vale para a nota cancelada no ERP.</para>
    ///
    /// <para><b>Exclusão física, de propósito.</b> Estas tabelas são a projeção de um fato que mora no
    /// ERP; a linha que a origem não tem mais não é histórico, é erro. É a mesma regra do crédito do
    /// SICOR, onde a operação reclassificada sai para não ser contada duas vezes. Nenhuma das duas
    /// tabelas está na política de auditoria. Só a janela lida é tocada: o que é anterior a ela fica
    /// como está.</para>
    /// </summary>
    private async Task<int> RemoverOQueSaiuDaOrigemAsync(
        DateOnly desde,
        HashSet<(long ClienteId, int EmpresaId, DateOnly Competencia)> comCliente,
        HashSet<(string Documento, int EmpresaId, DateOnly Competencia)> semCliente,
        CancellationToken ct)
    {
        await using var contexto = AbrirContextoDaCarga();

        var naJanelaComCliente = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.Competencia >= desde)
            .Select(f => new { f.Id, f.ClienteId, f.EmpresaId, f.Competencia })
            .ToListAsync(ct);
        var obsoletosComCliente = naJanelaComCliente
            .Where(f => !comCliente.Contains((f.ClienteId, f.EmpresaId, f.Competencia)))
            .Select(f => f.Id)
            .ToList();

        var naJanelaSemCliente = await contexto.FaturamentoSemClientes.AsNoTracking()
            .Where(f => f.Competencia >= desde)
            .Select(f => new { f.Id, f.Documento, f.EmpresaId, f.Competencia })
            .ToListAsync(ct);
        var obsoletosSemCliente = naJanelaSemCliente
            .Where(f => !semCliente.Contains((f.Documento, f.EmpresaId, f.Competencia)))
            .Select(f => f.Id)
            .ToList();

        if (obsoletosComCliente.Count == 0 && obsoletosSemCliente.Count == 0) return 0;

        // A LEITURA PARCIAL NÃO APAGA EM MASSA. A leitura vazia já está barrada lá em cima; esta é a
        // irmã dela que passa pela porta: uma filial que some da leitura por um problema passageiro, e
        // a janela dela inteira sairia do banco numa madrugada. Uma exclusão acima da fração máxima
        // é tratada como defeito de leitura — nada é removido, e a decisão fica no relatório.
        var naJanela = naJanelaComCliente.Count + naJanelaSemCliente.Count;
        var obsoletos = obsoletosComCliente.Count + obsoletosSemCliente.Count;
        if (RemocaoPassaDaTrava(naJanela, obsoletos))
        {
            Decidir(
                $"Remoção RECUSADA: a leitura deixaria de fora {obsoletos} de {naJanela} meses da janela " +
                $"(acima de {FracaoMaximaDeRemocao:P0}) — tratado como leitura parcial do Protheus; nada foi removido",
                obsoletos);
            return 0;
        }

        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);
        var removidos = 0;

        foreach (var bloco in obsoletosComCliente.Chunk(TamanhoDoBloco))
            removidos += await contexto.FaturamentoDosClientes.Where(f => bloco.Contains(f.Id)).ExecuteDeleteAsync(ct);

        foreach (var bloco in obsoletosSemCliente.Chunk(TamanhoDoBloco))
            removidos += await contexto.FaturamentoSemClientes.Where(f => bloco.Contains(f.Id)).ExecuteDeleteAsync(ct);

        await transacao.CommitAsync(ct);
        return removidos;
    }

    /// <summary>
    /// Grava o faturamento cuja contraparte não é cliente do CRM.
    ///
    /// <para>Não chama <c>SaveChanges</c>: entra na mesma transação do bloco de quem tem cliente,
    /// para que uma carga interrompida não deixe metade de um mês de um lado e metade do outro.</para>
    /// </summary>
    private static async Task GravarSemClienteAsync(
        CrmDbContext contexto,
        IEnumerable<(FaturamentoParaCarga Linha, int EmpresaId)> linhas,
        DateOnly desde,
        CancellationToken ct)
    {
        var doBloco = linhas.ToList();
        if (doBloco.Count == 0) return;

        var documentos = doBloco.Select(x => x.Linha.DocumentoDoCliente).Distinct().ToList();

        var existentes = await contexto.FaturamentoSemClientes
            .Where(f => documentos.Contains(f.Documento) && f.Competencia >= desde)
            .ToDictionaryAsync(f => (f.Documento, f.EmpresaId, f.Competencia), ct);

        foreach (var (linha, empresaId) in doBloco)
        {
            var natureza = ClassificarParceiro(linha.DocumentoDoCliente);
            var chave = (linha.DocumentoDoCliente, empresaId, linha.Competencia);

            if (existentes.TryGetValue(chave, out var jaExiste))
            {
                jaExiste.Reapurar(
                    linha.NomeNaOrigem, natureza,
                    linha.ValorLiquido, linha.Notas, linha.Itens, linha.Quebra);
                continue;
            }

            contexto.FaturamentoSemClientes.Add(FaturamentoSemCliente.Criar(
                empresaId, linha.Competencia, linha.DocumentoDoCliente, linha.NomeNaOrigem,
                natureza, linha.ValorLiquido, linha.Notas, linha.Itens, linha.Quebra));
        }
    }

    /// <summary>O que a leitura já deixou de fora, com o valor — antes de o CRM decidir qualquer coisa.</summary>
    private void RegistrarDescartesDaLeitura(LoteDoProtheus lote)
    {
        if (lote.CodigosSemDocumento > 0)
            Decidir(
                $"Códigos de cliente da nota sem CPF/CNPJ válido na SA1 — {Reais(lote.ValorSemDocumento)} " +
                $"de venda em {lote.ItensSemDocumento:N0} itens, sem a quem atribuir",
                lote.CodigosSemDocumento);

        if (lote.ItensQueNaoSaoVenda > 0)
            Decidir(
                "Itens de saída descartados por não serem venda — transferência entre filiais, remessa e " +
                $"retorno de demonstração, devolução e baixa de estoque ({Reais(lote.ValorQueNaoEhVenda)})",
                lote.ItensQueNaoSaoVenda);

        if (lote.ItensSemData > 0)
            Decidir("Itens de nota descartados por não ter data de emissão legível", lote.ItensSemData);
    }

    // =============================================================================================
    // A curva ABC e a situação
    // =============================================================================================

    /// <summary>
    /// Apura a curva ABC por filial, grava a classe em cada cliente e promove quem comprou.
    /// </summary>
    /// <param name="desde">O início da janela: só o faturamento dela conta.</param>
    /// <param name="ct">Cancelamento.</param>
    private async Task<(Dictionary<ClasseDeCliente, int> Curva, int Promovidos)> ApurarCurvaAbcAsync(
        DateOnly desde, CancellationToken ct)
    {
        var agora = DateTime.UtcNow;

        // A SOMA É EM MEMÓRIA: são três anos de linhas cliente × filial × mês, três colunas cada. E
        // A JANELA É A MESMA DA LEITURA — sem o filtro, a curva de daqui a um ano olharia quatro.
        List<(long ClienteId, int EmpresaId, decimal Faturado)> linhas;
        await using (var contexto = AbrirContextoDaCarga())
        {
            linhas = [.. (await contexto.FaturamentoDosClientes.AsNoTracking()
                    .Where(f => f.ExcluidoEm == null && f.Competencia >= desde)
                    .Select(f => new { f.ClienteId, f.EmpresaId, f.ValorLiquido })
                    .ToListAsync(ct))
                .Select(f => (f.ClienteId, f.EmpresaId, f.ValorLiquido))];
        }

        var classePorCliente = ClassificarCurva(linhas);
        var contagem = new Dictionary<ClasseDeCliente, int>();
        var promovidos = 0;

        foreach (var bloco in classePorCliente.Chunk(TamanhoDoBloco))
        {
            await using var contextoDoBloco = AbrirContextoDaCarga();
            var ids = bloco.Select(p => p.Key).ToList();
            var clientes = await contextoDoBloco.Clientes.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

            foreach (var cliente in clientes)
            {
                var (classe, faturado) = classePorCliente[cliente.Id];
                cliente.ApurarClasse(classe, faturado, agora, usuarioResponsavelId);
                contagem[classe] = contagem.GetValueOrDefault(classe) + 1;

                if (!cliente.EstaExcluido && PromoveAoFaturar(cliente.Situacao))
                {
                    cliente.MudarSituacao(SituacaoDoCliente.Cliente, usuarioResponsavelId);
                    promovidos++;
                }
            }

            await contextoDoBloco.SaveChangesAsync(ct);
        }

        // D É QUEM NÃO COMPROU, e ele é a maioria. Marcar explicitamente, em vez de deixar nulo, é o
        // que permite a tela dizer "3.100 clientes D sem visita há mais de 360 dias".
        List<long> semFaturamento;
        await using (var contexto = AbrirContextoDaCarga())
        {
            var comClasse = classePorCliente.Keys.ToHashSet();
            semFaturamento = [.. (await contexto.Clientes.AsNoTracking()
                    .Where(c => c.ExcluidoEm == null)
                    .Select(c => c.Id)
                    .ToListAsync(ct))
                .Where(id => !comClasse.Contains(id))];
        }

        foreach (var bloco in semFaturamento.Chunk(TamanhoDoBloco))
        {
            await using var contextoDoBloco = AbrirContextoDaCarga();
            var ids = bloco.ToList();
            var clientes = await contextoDoBloco.Clientes.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

            foreach (var cliente in clientes)
                cliente.ApurarClasse(ClasseDeCliente.D, 0m, agora, usuarioResponsavelId);

            contagem[ClasseDeCliente.D] = contagem.GetValueOrDefault(ClasseDeCliente.D) + clientes.Count;
            await contextoDoBloco.SaveChangesAsync(ct);
        }

        if (promovidos > 0)
            Decidir("Clientes promovidos de Suspect/Prospect a Cliente por terem venda na janela", promovidos);

        return (contagem, promovidos);
    }

    /// <summary>
    /// A curva ABC, pura: de (cliente, filial, faturado) para a classe de cada cliente.
    ///
    /// <para><b>Um cliente pode faturar em mais de uma filial.</b> A classe é dele, então a filial que
    /// decide é aquela onde ele mais comprou — a praça em que ele é cliente de verdade.</para>
    ///
    /// <para><b>O corte é pelo acumulado</b>, e não pelo valor do cliente: é isso que faz a curva ser
    /// ABC e não uma faixa de preço. O acumulado já inclui o próprio cliente — é a regra que a carga
    /// sempre aplicou. Consequência que o comentário antigo escondia ("o primeiro cliente é sempre A"):
    /// quem sozinho passa de 80% da filial é B, e de 95% é C. É decisão de negócio mudar isso.</para>
    /// </summary>
    /// <param name="linhas">O faturamento, uma linha por cliente, filial e mês (ou já somado).</param>
    internal static Dictionary<long, (ClasseDeCliente Classe, decimal Faturado)> ClassificarCurva(
        IEnumerable<(long ClienteId, int EmpresaId, decimal Faturado)> linhas)
    {
        var melhorFilialPorCliente = linhas
            .GroupBy(x => (x.ClienteId, x.EmpresaId))
            .Select(g => (g.Key.ClienteId, g.Key.EmpresaId, Faturado: g.Sum(x => x.Faturado)))
            .GroupBy(x => x.ClienteId)
            // O DESEMPATE PELA MENOR FILIAL faz duas apurações seguidas darem a mesma classe.
            .Select(g => g.OrderByDescending(x => x.Faturado).ThenBy(x => x.EmpresaId).First());

        var classePorCliente = new Dictionary<long, (ClasseDeCliente, decimal)>();

        foreach (var daFilial in melhorFilialPorCliente.GroupBy(x => x.EmpresaId))
        {
            var ordenados = daFilial.OrderByDescending(x => x.Faturado).ThenBy(x => x.ClienteId).ToList();
            var total = ordenados.Sum(x => x.Faturado);
            if (total <= 0) continue;

            var acumulado = 0m;
            foreach (var cliente in ordenados)
            {
                acumulado += cliente.Faturado;
                var fatia = acumulado / total;

                var classe = fatia <= CorteDaClasseA
                    ? ClasseDeCliente.A
                    : fatia <= CorteDaClasseB
                        ? ClasseDeCliente.B
                        : ClasseDeCliente.C;

                classePorCliente[cliente.ClienteId] = (classe, cliente.Faturado);
            }
        }

        return classePorCliente;
    }

    /// <summary>
    /// Se a venda muda a situação do cliente para <c>Cliente</c>.
    ///
    /// <para><c>Suspect</c> é "ainda não se sabe" e <c>Prospect</c> é "nunca comprou": a nota fiscal
    /// desmente os dois. <c>ClienteInativo</c> e <c>Encerrado</c> são decisões de uma pessoa sobre o
    /// relacionamento, e a carga não as desfaz — um encerrado que voltou a comprar é assunto da
    /// gerência, não de rotina noturna.</para>
    /// </summary>
    /// <param name="situacao">A situação atual.</param>
    internal static bool PromoveAoFaturar(SituacaoDoCliente situacao) =>
        situacao is SituacaoDoCliente.Suspect or SituacaoDoCliente.Prospect;

    // =============================================================================================
    // A natureza da contraparte sem cliente
    // =============================================================================================

    /// <summary>
    /// O que é a contraparte de uma nota que não achou cliente no CRM.
    ///
    /// <para>Só a fábrica e o grupo são reconhecidos, pela raiz do CNPJ — a lista mora em
    /// <see cref="ParceirosPorRaizDeCnpj"/>, que o parque de máquinas também usa. <b>Revenda não entra
    /// aqui</b>: não há lista confiável de concessionária. Quem não é fábrica nem grupo entra como
    /// cadastro faltando — que é o palpite certo na dúvida, porque é o que faz alguém olhar.</para>
    /// </summary>
    /// <param name="documento">CPF ou CNPJ, só dígitos.</param>
    internal static NaturezaDoParceiro ClassificarParceiro(string documento)
    {
        if (documento.Length == 0) return NaturezaDoParceiro.SemDocumento;

        return ParceirosPorRaizDeCnpj.PelaRaiz(documento) ?? NaturezaDoParceiro.ClienteNaoCadastrado;
    }

    // =============================================================================================
    // A SIMULAÇÃO — lê o CRM e o Protheus, e não grava nada
    // =============================================================================================

    /// <summary>
    /// <c>--somente-faturamento --simular</c>: o que a carga gravaria, sem gravar.
    ///
    /// <para><b>Nenhuma transação é aberta e nenhum <c>SaveChanges</c> é chamado</b> — não é a
    /// transação desfeita das outras simulações, é leitura pura, porque ela roda contra o banco de
    /// produção a partir de uma estação. O sistema de origem também não é criado.</para>
    ///
    /// <para><b>Duas visões.</b> "CRM de hoje" casa a venda com os clientes que o banco tem agora.
    /// "CRM + carga da SA1" casa também com os clientes que <c>--somente-clientes-protheus</c> criaria
    /// — que é o que importa enquanto a produção tiver o cadastro vazio.</para>
    /// </summary>
    /// <param name="cadastroDaSa1">A leitura da SA1, para a segunda visão; nula dispensa a visão.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<SimulacaoDoFaturamento>> SimularAsync(
        LeitorDeClientesDoProtheus? cadastroDaSa1, CancellationToken ct)
    {
        relatar("Lendo o faturamento direto do banco do Protheus — três anos (SIMULAÇÃO: nada será gravado)…");

        var lido = await protheus.LerAsync(InicioDaJanela(DateTime.UtcNow), relatar, ct);
        if (!lido.EhSucesso) return Resultado<SimulacaoDoFaturamento>.Indisponivel(lido.Erro!);

        IReadOnlySet<string>? daSa1 = null;
        if (cadastroDaSa1 is not null)
        {
            relatar("Lendo a SA1 para projetar os clientes que a carga de clientes criaria…");
            var projetados = await DocumentosQueACargaDaSa1CriariaAsync(cadastroDaSa1, ct);
            if (!projetados.EhSucesso) relatar("  a SA1 não foi lida: " + projetados.Erro + " — só a visão do CRM de hoje.");
            else daSa1 = projetados.Valor;
        }

        return Resultado<SimulacaoDoFaturamento>.Ok(await SimularAsync(lido.Valor, daSa1, ct));
    }

    /// <summary>A simulação sobre um lote já lido — é o que o teste exercita.</summary>
    /// <param name="lote">O lote.</param>
    /// <param name="documentosDaSa1">Os documentos que a carga da SA1 criaria; nulo dispensa a segunda visão.</param>
    /// <param name="ct">Cancelamento.</param>
    internal async Task<SimulacaoDoFaturamento> SimularAsync(
        LoteDoProtheus lote, IReadOnlySet<string>? documentosDaSa1, CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var clientes = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Documento != null)
            .Select(c => new { c.Id, Documento = c.Documento!.Value.Numero, c.Situacao })
            .ToListAsync(ct);

        var porDocumento = new Dictionary<string, long>(StringComparer.Ordinal);
        var situacaoPorId = new Dictionary<long, SituacaoDoCliente>();
        foreach (var cliente in clientes)
        {
            porDocumento.TryAdd(cliente.Documento, cliente.Id);
            situacaoPorId[cliente.Id] = cliente.Situacao;
        }

        var totalDeClientes = await contexto.Clientes.AsNoTracking().CountAsync(c => c.ExcluidoEm == null, ct);

        var empresas = await contexto.Empresas.AsNoTracking()
            .Select(e => new { e.Id, e.Codigo, e.Nome })
            .ToListAsync(ct);
        var porCodigoDeEmpresa = empresas.ToDictionary(e => e.Codigo, e => e.Id, StringComparer.Ordinal);
        var nomeDaEmpresa = empresas.ToDictionary(e => e.Id, e => e.Nome);

        // O QUE O BANCO TEM HOJE NA JANELA, para dizer quanto seria incluído, reapurado e removido.
        var existentesComCliente = (await contexto.FaturamentoDosClientes.AsNoTracking()
                .Where(f => f.Competencia >= lote.Desde)
                .Select(f => new { f.ClienteId, f.EmpresaId, f.Competencia })
                .ToListAsync(ct))
            .Select(f => (f.ClienteId, f.EmpresaId, f.Competencia))
            .ToHashSet();
        var existentesSemCliente = (await contexto.FaturamentoSemClientes.AsNoTracking()
                .Where(f => f.Competencia >= lote.Desde)
                .Select(f => new { f.Documento, f.EmpresaId, f.Competencia })
                .ToListAsync(ct))
            .Select(f => (f.Documento, f.EmpresaId, f.Competencia))
            .ToHashSet();

        var hoje = Visao("CRM de hoje", lote, porDocumento, situacaoPorId, totalDeClientes, porCodigoDeEmpresa, nomeDaEmpresa);

        // A DIFERENÇA CONTRA O BANCO é da visão de hoje: é o que a carga faria se rodasse agora.
        var produzidosComCliente = new HashSet<(long, int, DateOnly)>();
        var produzidosSemCliente = new HashSet<(string, int, DateOnly)>();
        foreach (var linha in lote.Faturamento)
        {
            var (clienteId, empresaId) = Resolver(linha, porDocumento, porCodigoDeEmpresa);
            if (empresaId == 0) continue;
            if (clienteId != 0) produzidosComCliente.Add((clienteId, empresaId, linha.Competencia));
            else produzidosSemCliente.Add((linha.DocumentoDoCliente, empresaId, linha.Competencia));
        }

        var reapuraria = produzidosComCliente.Count(existentesComCliente.Contains) + produzidosSemCliente.Count(existentesSemCliente.Contains);
        var incluiria = produzidosComCliente.Count + produzidosSemCliente.Count - reapuraria;
        var removeria = existentesComCliente.Count(k => !produzidosComCliente.Contains(k)) + existentesSemCliente.Count(k => !produzidosSemCliente.Contains(k));

        VisaoDaSimulacao? comSa1 = null;
        if (documentosDaSa1 is not null)
        {
            // OS CLIENTES QUE A SA1 CRIARIA ganham um identificador provisório, negativo, só para a
            // curva: nascem Suspect, que é o que a carga de clientes grava.
            var ampliado = new Dictionary<string, long>(porDocumento, StringComparer.Ordinal);
            var situacoes = new Dictionary<long, SituacaoDoCliente>(situacaoPorId);
            long provisorio = 0;
            foreach (var documento in documentosDaSa1.Order(StringComparer.Ordinal))
            {
                if (ampliado.ContainsKey(documento)) continue;
                ampliado[documento] = --provisorio;
                situacoes[provisorio] = SituacaoDoCliente.Suspect;
            }

            comSa1 = Visao("CRM + carga da SA1", lote, ampliado, situacoes, totalDeClientes - (int)provisorio,
                porCodigoDeEmpresa, nomeDaEmpresa);
        }

        return new SimulacaoDoFaturamento(lote, hoje, comSa1, incluiria, reapuraria, removeria);
    }

    /// <summary>Uma visão da simulação: o lote resolvido contra um cadastro de clientes.</summary>
    private static VisaoDaSimulacao Visao(
        string nome,
        LoteDoProtheus lote,
        IReadOnlyDictionary<string, long> porDocumento,
        IReadOnlyDictionary<long, SituacaoDoCliente> situacaoPorId,
        int totalDeClientes,
        IReadOnlyDictionary<string, int> porCodigoDeEmpresa,
        IReadOnlyDictionary<int, string> nomeDaEmpresa)
    {
        var porMes = new SortedDictionary<DateOnly, Acumulado>();
        var porFilial = new SortedDictionary<string, Acumulado>(StringComparer.Ordinal);
        var comCliente = new List<(long, int, decimal)>();
        var documentosComCliente = new HashSet<string>(StringComparer.Ordinal);
        var documentosSemCliente = new HashSet<string>(StringComparer.Ordinal);
        int linhasComCliente = 0, linhasSemCliente = 0, linhasSemFilial = 0;

        foreach (var linha in lote.Faturamento)
        {
            var (clienteId, empresaId) = Resolver(linha, porDocumento, porCodigoDeEmpresa);
            var codigo = linha.CodigoDaFilial ?? "(sem filial)";
            var rotuloDaFilial = empresaId != 0 && nomeDaEmpresa.TryGetValue(empresaId, out var n) ? $"{codigo} {n}" : $"{codigo} (sem empresa no CRM)";

            foreach (var acumulado in new[] { Obter(porMes, linha.Competencia), Obter(porFilial, rotuloDaFilial) })
            {
                acumulado.Linhas++;
                acumulado.Total += linha.ValorLiquido;
                acumulado.Maquina += linha.Quebra.Maquina;
                if (empresaId == 0) acumulado.SemFilial += linha.ValorLiquido;
                else if (clienteId != 0) acumulado.ComCliente += linha.ValorLiquido;
                else acumulado.SemCliente += linha.ValorLiquido;
            }

            if (empresaId == 0) { linhasSemFilial++; continue; }

            if (clienteId != 0)
            {
                linhasComCliente++;
                documentosComCliente.Add(linha.DocumentoDoCliente);
                comCliente.Add((clienteId, empresaId, linha.ValorLiquido));
            }
            else
            {
                linhasSemCliente++;
                documentosSemCliente.Add(linha.DocumentoDoCliente);
            }
        }

        var curva = ClassificarCurva(comCliente);
        var contagem = curva.Values.GroupBy(v => v.Classe).ToDictionary(g => g.Key, g => g.Count());
        contagem[ClasseDeCliente.D] = Math.Max(0, totalDeClientes - curva.Count);

        var promovidos = curva.Keys.Count(id => situacaoPorId.TryGetValue(id, out var s) && PromoveAoFaturar(s));

        return new VisaoDaSimulacao(
            nome,
            [.. porMes.Select(p => p.Value.Linha(p.Key.ToString("yyyy-MM", CultureInfo.InvariantCulture)))],
            [.. porFilial.Select(p => p.Value.Linha(p.Key))],
            linhasComCliente, linhasSemCliente, linhasSemFilial,
            documentosComCliente.Count, documentosSemCliente.Count, promovidos, contagem);

        static Acumulado Obter<TChave>(SortedDictionary<TChave, Acumulado> onde, TChave chave) where TChave : notnull
        {
            if (!onde.TryGetValue(chave, out var acumulado)) onde[chave] = acumulado = new Acumulado();
            return acumulado;
        }
    }

    private sealed class Acumulado
    {
        public int Linhas;
        public decimal Total;
        public decimal Maquina;
        public decimal ComCliente;
        public decimal SemCliente;
        public decimal SemFilial;

        public LinhaDaSimulacao Linha(string chave) => new(chave, Linhas, Total, Maquina, ComCliente, SemCliente, SemFilial);
    }

    /// <summary>
    /// Os documentos que <c>--somente-clientes-protheus</c> transformaria em cliente — só para a simulação.
    ///
    /// <para><b>A mesma sequência de <see cref="CargaDeClientesDoProtheus"/></b>, na mesma ordem: loja
    /// principal por documento (<see cref="CargaDeClientesDoProtheus.EscolherAPrincipal"/>, a mesma
    /// função), dígito verificador, UF conhecida, município no catálogo e filial responsável na área de
    /// atuação. Aquela carga só simula dentro de uma transação desfeita — que grava e desfaz —, e esta
    /// simulação roda contra a produção sem abrir transação nenhuma; por isso a projeção é refeita
    /// aqui, em leitura pura. Se a regra de lá mudar, esta visão precisa acompanhar.</para>
    /// </summary>
    private async Task<Resultado<IReadOnlySet<string>>> DocumentosQueACargaDaSa1CriariaAsync(
        LeitorDeClientesDoProtheus cadastroDaSa1, CancellationToken ct)
    {
        var lido = await cadastroDaSa1.LerAsync(ct);
        if (!lido.EhSucesso) return Resultado<IReadOnlySet<string>>.Indisponivel(lido.Erro!);

        await using var contexto = abrirContexto();

        var municipios = await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge != null)
            .Select(m => new { m.Id, CodigoIbge = m.CodigoIbge!.Value, m.Uf })
            .ToListAsync(ct);

        var municipioPorIbge = municipios.ToDictionary(m => m.CodigoIbge, m => m.Id);
        var prefixoDaUf = municipios
            .GroupBy(m => m.Uf, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().CodigoIbge / 100_000, StringComparer.OrdinalIgnoreCase);

        var comFilial = (await contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
                .Where(a => a.EncerradoEm == null && a.EmpresaResponsavelId != null)
                .Select(a => a.MunicipioId)
                .ToListAsync(ct))
            .ToHashSet();

        var documentos = new HashSet<string>(StringComparer.Ordinal);
        foreach (var grupo in lido.Valor.GroupBy(l => l.Documento, StringComparer.Ordinal))
        {
            var loja = CargaDeClientesDoProtheus.EscolherAPrincipal(grupo);

            if (!CpfCnpj.TentarCriar(grupo.Key, out _)) continue;
            if (string.IsNullOrWhiteSpace(loja.Uf) || !prefixoDaUf.TryGetValue(loja.Uf, out var prefixo)) continue;

            var codigoIbge = LeitorDeClientesDoProtheus.CodigoIbge(prefixo, loja.CodigoDoMunicipio);
            if (codigoIbge is null || !municipioPorIbge.TryGetValue(codigoIbge.Value, out var municipioId)) continue;
            if (!comFilial.Contains(municipioId)) continue;

            documentos.Add(grupo.Key);
        }

        relatar($"  {documentos.Count:N0} documento(s) da SA1 virariam cliente (área de atuação).");
        return Resultado<IReadOnlySet<string>>.Ok(documentos);
    }

    /// <summary>
    /// Imprime a simulação: por mês, por filial, e o que mudaria no banco.
    /// </summary>
    /// <param name="simulacao">A simulação.</param>
    /// <param name="escrever">Onde escrever.</param>
    internal static void Imprimir(SimulacaoDoFaturamento simulacao, Action<string> escrever)
    {
        var lote = simulacao.Lote;
        escrever(string.Empty);
        escrever($"Janela lida: desde {lote.Desde:dd/MM/yyyy} · nota mais recente {lote.EmissaoMaisRecente:dd/MM/yyyy} · " +
                 $"{lote.Faturamento.Count:N0} meses de faturamento por documento e filial · " +
                 $"filiais nas notas: {string.Join(", ", lote.FiliaisVistas)}.");
        escrever($"Fora da venda: {lote.ItensQueNaoSaoVenda:N0} itens que não são venda ({Reais(lote.ValorQueNaoEhVenda)}); " +
                 $"{lote.CodigosSemDocumento:N0} códigos sem documento na SA1 ({Reais(lote.ValorSemDocumento)}).");
        escrever($"No banco, se a carga rodasse agora (visão de hoje): incluiria {simulacao.Incluiria:N0}, reapuraria " +
                 $"{simulacao.Reapuraria:N0} e removeria {simulacao.Removeria:N0} linha(s).");

        foreach (var visao in new[] { simulacao.Hoje, simulacao.ComSa1 }.OfType<VisaoDaSimulacao>())
        {
            escrever(string.Empty);
            escrever($"=== {visao.Nome} ===");
            escrever($"  {visao.LinhasComCliente:N0} linha(s) em FaturamentoDoCliente ({visao.DocumentosComCliente:N0} documentos) · " +
                     $"{visao.LinhasSemCliente:N0} em FaturamentoSemCliente ({visao.DocumentosSemCliente:N0} documentos) · " +
                     $"{visao.LinhasSemFilial:N0} descartada(s) sem empresa.");
            escrever("  curva ABC: " + string.Join(" · ", visao.Curva.OrderBy(p => p.Key).Select(p => $"{p.Key} {p.Value:N0}")) +
                     $" · {visao.ClientesPromovidos:N0} promovido(s) a Cliente.");

            escrever(string.Empty);
            escrever($"  {"competência",-12}{"linhas",8}{"total R$ mi",14}{"máquina",10}{"c/ cliente",12}{"s/ cliente",12}{"s/ filial",11}");
            foreach (var linha in visao.PorMes) escrever("  " + Formatar(linha, 12));

            escrever(string.Empty);
            escrever($"  {"filial",-44}{"linhas",8}{"total R$ mi",14}{"máquina",10}{"c/ cliente",12}{"s/ cliente",12}{"s/ filial",11}");
            foreach (var linha in visao.PorFilial) escrever("  " + Formatar(linha, 44));
        }

        static string Formatar(LinhaDaSimulacao l, int largura) =>
            $"{l.Chave.PadRight(largura)}{l.Linhas,8:N0}{Mi(l.Total),14}{Mi(l.Maquina),10}{Mi(l.ComCliente),12}{Mi(l.SemCliente),12}{Mi(l.SemFilial),11}";

        static string Mi(decimal valor) => (valor / 1_000_000m).ToString("N2", CulturaDoRelatorio);
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>
    /// A filial pelo CÓDIGO de seis dígitos, que é o mesmo de <c>organizacao.Empresa</c> — sem de-para.
    /// O cliente pelo documento.
    /// </summary>
    private static (long ClienteId, int EmpresaId) Resolver(
        FaturamentoParaCarga linha,
        IReadOnlyDictionary<string, long> porDocumento,
        IReadOnlyDictionary<string, int> porCodigoDeEmpresa) =>
        (porDocumento.GetValueOrDefault(linha.DocumentoDoCliente),
         linha.CodigoDaFilial is { Length: > 0 } codigo ? porCodigoDeEmpresa.GetValueOrDefault(codigo) : 0);

    /// <summary>
    /// Abre um contexto que grava na trilha como integração do Protheus (documento 41, fase 2).
    /// </summary>
    private CrmDbContext AbrirContextoDaCarga()
    {
        var contexto = abrirContexto();
        if (_sistemaId is { } sistema) contexto.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistema);
        return contexto;
    }

    private async Task<int> GarantirSistemaAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var sistema = await contexto.Sistemas
            .FirstOrDefaultAsync(s => s.Codigo == LeitorDeClientesDoProtheus.CodigoDoSistema, ct);

        if (sistema is not null) return sistema.Id;

        sistema = Sistema.Criar(
            LeitorDeClientesDoProtheus.CodigoDoSistema, "Protheus (TOTVS) — ERP", "SQL Server, somente leitura");

        contexto.Sistemas.Add(sistema);
        await contexto.SaveChangesAsync(ct);

        return sistema.Id;
    }

    /// <summary>
    /// A filial em operação pelo código de seis dígitos — TODAS, inclusive as desativadas no CRM.
    ///
    /// <para>Guaíra, Ituverava e Monte Alto estão desativadas em <c>organizacao.Empresa</c> e seguem
    /// emitindo nota no ERP (medido em 24/09/2026). A venda delas é venda da empresa: descartá-la por
    /// um sinalizador do CRM faria o total da diretoria não fechar com o do ERP.</para>
    /// </summary>
    private async Task<Dictionary<string, int>> MapaDeEmpresasPorCodigoAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        return await contexto.Empresas.AsNoTracking()
            .ToDictionaryAsync(e => e.Codigo, e => e.Id, StringComparer.Ordinal, ct);
    }

    /// <summary>O identificador de cada cliente carregado, pelo documento sem máscara.</summary>
    private async Task<Dictionary<string, long>> MapaDeClientesPorDocumentoAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var comDocumento = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Documento != null)
            .Select(c => new { c.Id, Documento = c.Documento!.Value.Numero })
            .ToListAsync(ct);

        var mapa = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var cliente in comDocumento) mapa.TryAdd(cliente.Documento, cliente.Id);

        return mapa;
    }

    private void Decidir(string decisao, int quantas) =>
        _decisoes[decisao] = _decisoes.TryGetValue(decisao, out var atual) ? atual + quantas : quantas;

    /// <summary>
    /// O valor em reais cheios, para a linha de decisão da carga.
    ///
    /// <para>Sem centavos de propósito: quem lê o relatório da carga está decidindo se um descarte
    /// é resíduo ou é problema, e essa decisão se toma na ordem de grandeza.</para>
    /// </summary>
    private static string Reais(decimal valor) => valor.ToString("C0", CulturaDoRelatorio);

    private static readonly CultureInfo CulturaDoRelatorio = CultureInfo.GetCultureInfo("pt-BR");
}

/// <summary>
/// O que a atualização do faturamento fez.
/// </summary>
/// <param name="MesesNovos">Meses de cliente que entraram novos.</param>
/// <param name="MesesReapurados">Meses de cliente que já existiam e foram reapurados.</param>
/// <param name="MesesRemovidos">Linhas das duas tabelas que saíram da janela porque a origem não as tem mais.</param>
/// <param name="ClientesPromovidos">Clientes que passaram a <c>Cliente</c> por terem venda.</param>
/// <param name="Curva">Quantos clientes ficaram em cada classe.</param>
/// <param name="Decisoes">
/// O que foi descartado e por quê — com o valor em reais no próprio texto, porque a ordem de
/// grandeza é o que separa resíduo de problema.
/// </param>
internal sealed record ResumoDoFaturamento(
    int MesesNovos,
    int MesesReapurados,
    int MesesRemovidos,
    int ClientesPromovidos,
    IReadOnlyDictionary<ClasseDeCliente, int> Curva,
    IReadOnlyDictionary<string, int> Decisoes);

/// <summary>Uma linha da simulação — um mês ou uma filial.</summary>
/// <param name="Chave">A competência (<c>yyyy-MM</c>) ou a filial.</param>
/// <param name="Linhas">Meses de faturamento por documento.</param>
/// <param name="Total">O valor.</param>
/// <param name="Maquina">Quanto foi máquina.</param>
/// <param name="ComCliente">Quanto casou com cliente do CRM.</param>
/// <param name="SemCliente">Quanto iria para <c>FaturamentoSemCliente</c>.</param>
/// <param name="SemFilial">Quanto seria descartado por não ter empresa no CRM.</param>
internal sealed record LinhaDaSimulacao(
    string Chave, int Linhas, decimal Total, decimal Maquina, decimal ComCliente, decimal SemCliente, decimal SemFilial);

/// <summary>Uma visão da simulação: o lote casado contra um cadastro de clientes.</summary>
internal sealed record VisaoDaSimulacao(
    string Nome,
    IReadOnlyList<LinhaDaSimulacao> PorMes,
    IReadOnlyList<LinhaDaSimulacao> PorFilial,
    int LinhasComCliente,
    int LinhasSemCliente,
    int LinhasSemFilial,
    int DocumentosComCliente,
    int DocumentosSemCliente,
    int ClientesPromovidos,
    IReadOnlyDictionary<ClasseDeCliente, int> Curva);

/// <summary>O que <c>--somente-faturamento --simular</c> apurou.</summary>
/// <param name="Lote">O que a leitura trouxe.</param>
/// <param name="Hoje">A visão contra o cadastro de hoje.</param>
/// <param name="ComSa1">A visão com os clientes que a carga da SA1 criaria; nula sem a SA1.</param>
/// <param name="Incluiria">Linhas novas, na visão de hoje.</param>
/// <param name="Reapuraria">Linhas existentes que seriam reapuradas.</param>
/// <param name="Removeria">Linhas da janela que a origem não tem mais.</param>
internal sealed record SimulacaoDoFaturamento(
    LoteDoProtheus Lote,
    VisaoDaSimulacao Hoje,
    VisaoDaSimulacao? ComSa1,
    int Incluiria,
    int Reapuraria,
    int Removeria);
