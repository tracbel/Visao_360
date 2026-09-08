using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Carga;

/// <summary>
/// O FATURAMENTO E A CURVA ABC — o que dá letra ao cliente.
///
/// <para>Duas etapas, nesta ordem, porque a segunda depende da primeira: grava o faturamento por
/// cliente, filial e mês; depois ordena os clientes por faturamento e corta a curva em A, B e C.
/// Quem não aparece no faturamento é D — e D aqui significa "não comprou na janela", não "cliente
/// ruim".</para>
///
/// <para><b>A curva é por FILIAL, e não da rede inteira.</b> Um cliente que responde por 3% do
/// faturamento de Votuporanga é grande em Votuporanga, e some no consolidado ao lado de Ribeirão
/// Preto, que fatura vinte vezes mais. Como a carteira e o CEN são de uma filial, a classe tem de
/// ser da mesma filial, senão o CEN de uma praça pequena não teria cliente A nenhum para
/// visitar.</para>
///
/// <para><b>Só que hoje o Protheus não segmenta por filial.</b> Medido em 06/09/2026:
/// <c>D2_FILIAL</c> é <c>010101</c> em <b>100%</b> das 205.169 linhas de nota — Araraquara,
/// Barretos, Guaíra, Orlândia, Franca e Votuporanga têm zero itens. A Agro fatura toda por uma
/// filial só no ERP, exatamente como a <c>VV1</c> da frota. O agrupamento abaixo continua correto
/// e passa a valer sozinho no dia em que o ERP separar, mas <b>a curva de hoje é global</b>, e
/// dizer o contrário na tela seria mentira. A praça de cada cliente sai da carteira do CRM, que é
/// quem sabe de quem ele é.</para>
/// </summary>
internal sealed partial class CargaDeProcessoDoVortice
{
    /// <summary>Quantos anos para trás a curva ABC olha.</summary>
    private const int AnosDeFaturamento = 3;

    /// <summary>Onde a classe A termina: os clientes que somam os primeiros 80% do faturamento.</summary>
    private const decimal CorteDaClasseA = 0.80m;

    /// <summary>Onde a classe B termina.</summary>
    private const decimal CorteDaClasseB = 0.95m;

    /// <summary>
    /// Lê o faturamento do Protheus, grava e reapura a curva ABC — <b>sem tocar no Vórtice</b>.
    ///
    /// <para>É a única etapa da carga que não depende do sistema de origem: a contraparte vem da
    /// <c>SD2</c>, o cliente vem do nosso próprio cadastro e a classe sai do que acabou de ser
    /// gravado. Por isso ela roda sozinha por <c>--somente-faturamento</c> — reler o Vórtice
    /// inteiro para atualizar faturamento custa horas e não muda nada do que o Vórtice traz.</para>
    /// </summary>
    /// <param name="sistemaId">O sistema de origem, para a chave externa.</param>
    /// <param name="ct">Cancelamento.</param>
    private async Task<Resultado<FaturamentoCarregado>> CarregarFaturamentoAsync(
        int sistemaId, CancellationToken ct)
    {
        relatar("Lendo o faturamento direto do Protheus — três anos, para a curva ABC ter base…");

        var desde = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-AnosDeFaturamento));

        // AS FILIAIS SAEM DA NOSSA PRÓPRIA TABELA DE EMPRESAS, e não de uma lista no código.
        //
        // O código do Protheus é o mesmo de `organizacao.Empresa` — seis dígitos, `010107`. Filtrar
        // por esse formato deixa de fora `CEQU` e `CFRA_COLORADO_6`, que são empresas nossas sem
        // correspondente no ERP e que o Protheus recusaria com 403. Filial nova cadastrada aqui
        // passa a ser lida sozinha, sem alterar código.
        var filiais = await FiliaisDoProtheusAsync(ct);

        var faturamento = await faturamentoDoProtheus.LerAsync(desde, filiais, relatar, ct);
        if (!faturamento.EhSucesso)
            return Resultado<FaturamentoCarregado>.Indisponivel(faturamento.Erro!);

        if (faturamento.Valor.ClientesSemCadastro > 0)
            Decidir(
                "Códigos de cliente na nota fiscal sem correspondência no cadastro do Protheus",
                faturamento.Valor.ClientesSemCadastro);

        if (faturamento.Valor.ItensQueNaoSaoVenda > 0)
            Decidir(
                "Itens de saída descartados por não serem venda — transferência entre filiais, " +
                "remessa e retorno de demonstração, devolução e baixa de estoque",
                faturamento.Valor.ItensQueNaoSaoVenda);

        if (faturamento.Valor.ItensSemData > 0)
            Decidir("Itens de nota descartados por não ter data de emissão legível",
                faturamento.Valor.ItensSemData);

        var (gravado, _) = await GravarFaturamentoAsync(sistemaId, faturamento.Valor.Faturamento, ct);

        relatar(
            $"  {gravado} mês(es) de faturamento gravado(s) · nota mais recente: " +
            $"{faturamento.Valor.EmissaoMaisRecente:dd/MM/yyyy}.");

        var curva = await ApurarCurvaAbcAsync(ct);
        relatar(
            "  curva ABC: " +
            string.Join(" · ", curva.OrderBy(p => p.Key).Select(p => $"{p.Key} {p.Value}")));

        return Resultado<FaturamentoCarregado>.Ok(new FaturamentoCarregado(gravado, curva));
    }

    /// <summary>
    /// Executa <b>somente</b> a etapa de faturamento, sem ler o sistema de origem.
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ResumoDoFaturamento>> ExecutarSomenteFaturamentoAsync(CancellationToken ct)
    {
        var sistemaId = await GarantirSistemaAsync(ct);
        var gravado = await CarregarFaturamentoAsync(sistemaId, ct);

        if (!gravado.EhSucesso) return Resultado<ResumoDoFaturamento>.Indisponivel(gravado.Erro!);

        // AS DECISÕES SAEM JUNTO, e não só a contagem. Elas são o que diz quanto dinheiro ficou
        // de fora e por quê — sem isso o comando terminaria com "6.849 meses gravados" e o
        // descarte voltaria a ser invisível, que é o defeito que este trabalho existe para
        // corrigir.
        return Resultado<ResumoDoFaturamento>.Ok(new ResumoDoFaturamento(
            gravado.Valor.Gravados,
            _decisoes.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal)));
    }

    private async Task<(int Gravados, int SemCliente)> GravarFaturamentoAsync(
        int sistemaId,
        IReadOnlyList<FaturamentoParaCarga> faturamento,
        CancellationToken ct)
    {
        if (faturamento.Count == 0) return (0, 0);

        var porDocumento = await MapaDeClientesPorDocumentoAsync(ct);

        // A FILIAL VEM DE DOIS JEITOS, PORQUE HÁ DUAS ORIGENS. O Vórtice identifica por número
        // (`NroEmpresa`) e exige de-para; o Protheus identifica pelo CÓDIGO de seis dígitos, que
        // é o mesmo de `organizacao.Empresa` e dispensa tradução. Quem manda é o que veio
        // preenchido na linha.
        var porCodigoDeEmpresa = await MapaDeEmpresasPorCodigoAsync(ct);
        var gravados = 0;
        var semCliente = 0;
        var documentosSemCliente = new HashSet<string>(StringComparer.Ordinal);

        // O DESCARTE TEM DE DIZER QUANTO DINHEIRO LEVOU JUNTO.
        //
        // Este método já jogava fora, em silêncio, tudo que não casasse cliente ou filial — e foi
        // assim que 10.578 meses apurados viraram 6.849 gravados sem uma linha de log. Contar
        // linha descartada não basta: oitenta e quatro documentos parecem um resíduo, e eram mais
        // de duzentos milhões de reais. É o mesmo defeito que deixou "faturamento parado em
        // 11/04/2025" passar dezessete meses — número que some não grita.
        var valorSemCliente = 0m;
        var filiaisSemEmpresa = new HashSet<string>(StringComparer.Ordinal);
        var valorSemEmpresa = 0m;

        foreach (var bloco in faturamento.Chunk(TamanhoDoBloco))
        {
            await using var contexto = abrirContexto();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            // A CHAVE NATURAL É (cliente, filial, competência), e é ela que torna a carga
            // reexecutável: quem já existe é reapurado, não duplicado.
            var resolvido = bloco
                .Select(f => new
                {
                    Linha = f,
                    ClienteId = porDocumento.GetValueOrDefault(f.DocumentoDoCliente),
                    EmpresaId = f.CodigoDaFilial is { Length: > 0 } codigo
                        ? porCodigoDeEmpresa.GetValueOrDefault(codigo)
                        : deParaDeFiliais.GetValueOrDefault(f.CodigoDaFilialNoLegado)
                })
                .ToList();

            var doBloco = resolvido.Where(x => x.ClienteId != 0 && x.EmpresaId != 0).ToList();

            // QUEM NÃO TEM CLIENTE VAI PARA A OUTRA TABELA, e não para o lixo. Precisa da filial
            // resolvida — sem ela não há onde pendurar a linha —, e a filial resolve porque o
            // código vem do próprio ERP.
            var semDonoNoBloco = resolvido
                .Where(x => x.ClienteId == 0 && x.EmpresaId != 0)
                .ToList();

            foreach (var fora in resolvido.Where(x => x.ClienteId == 0))
            {
                if (documentosSemCliente.Add(fora.Linha.DocumentoDoCliente)) semCliente++;
                valorSemCliente += fora.Linha.ValorLiquido;
            }

            foreach (var fora in resolvido.Where(x => x.EmpresaId == 0))
            {
                filiaisSemEmpresa.Add(fora.Linha.CodigoDaFilial ?? fora.Linha.CodigoDaFilialNoLegado.ToString());
                valorSemEmpresa += fora.Linha.ValorLiquido;
            }

            await GravarSemClienteAsync(contexto, semDonoNoBloco.Select(x => (x.Linha, x.EmpresaId)), ct);

            if (doBloco.Count == 0)
            {
                await contexto.SaveChangesAsync(ct);
                await transacao.CommitAsync(ct);
                continue;
            }

            var chaves = doBloco.Select(x => x.ClienteId).Distinct().ToList();
            var existentes = await contexto.FaturamentoDosClientes
                .Where(f => chaves.Contains(f.ClienteId))
                .ToDictionaryAsync(f => (f.ClienteId, f.EmpresaId, f.Competencia), ct);

            foreach (var item in doBloco)
            {
                var chave = (item.ClienteId, item.EmpresaId, item.Linha.Competencia);

                if (existentes.TryGetValue(chave, out var jaExiste))
                {
                    jaExiste.Reapurar(
                        item.Linha.ValorLiquido, item.Linha.Notas, item.Linha.Itens, item.Linha.Quebra);
                    continue;
                }

                contexto.FaturamentoDosClientes.Add(FaturamentoDoCliente.Criar(
                    item.EmpresaId, item.ClienteId, item.Linha.Competencia,
                    item.Linha.ValorLiquido, item.Linha.Notas, item.Linha.Itens, item.Linha.Quebra));

                gravados++;
            }

            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        if (semCliente > 0)
            Decidir(
                $"Documentos que faturam e NÃO existem no cadastro de clientes do CRM — " +
                $"{Reais(valorSemCliente)} de venda sem dono aqui dentro", semCliente);

        if (filiaisSemEmpresa.Count > 0)
            Decidir(
                $"Códigos de filial do faturamento sem empresa correspondente " +
                $"({string.Join(", ", filiaisSemEmpresa.Order())}) — {Reais(valorSemEmpresa)} descartados",
                filiaisSemEmpresa.Count);

        return (gravados, semCliente);
    }

    /// <summary>
    /// Apura a curva ABC por filial e grava a classe em cada cliente.
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    private async Task<Dictionary<ClasseDeCliente, int>> ApurarCurvaAbcAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();
        var agora = DateTime.UtcNow;

        var porCliente = await contexto.FaturamentoDosClientes
            .Where(f => f.ExcluidoEm == null)
            .GroupBy(f => new { f.ClienteId, f.EmpresaId })
            .Select(g => new
            {
                g.Key.ClienteId,
                g.Key.EmpresaId,
                Faturado = g.Sum(f => f.ValorLiquido)
            })
            .ToListAsync(ct);

        // UM CLIENTE PODE FATURAR EM MAIS DE UMA FILIAL. A classe é dele, então a filial que
        // decide é aquela onde ele mais comprou — a praça em que ele é cliente de verdade.
        var melhorFilialPorCliente = porCliente
            .GroupBy(x => x.ClienteId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Faturado).First());

        var classePorCliente = new Dictionary<long, (ClasseDeCliente Classe, decimal Faturado)>();

        foreach (var daFilial in melhorFilialPorCliente.Values.GroupBy(x => x.EmpresaId))
        {
            var ordenados = daFilial.OrderByDescending(x => x.Faturado).ToList();
            var total = ordenados.Sum(x => x.Faturado);
            if (total <= 0) continue;

            var acumulado = 0m;
            foreach (var cliente in ordenados)
            {
                acumulado += cliente.Faturado;
                var fatia = acumulado / total;

                // O CORTE É PELO ACUMULADO, e não pelo valor do cliente: é isso que faz a curva
                // ser ABC e não uma faixa de preço. O primeiro cliente é sempre A, mesmo numa
                // filial pequena — e é o que a gerência daquela praça precisa ver.
                var classe = fatia <= CorteDaClasseA
                    ? ClasseDeCliente.A
                    : fatia <= CorteDaClasseB
                        ? ClasseDeCliente.B
                        : ClasseDeCliente.C;

                classePorCliente[cliente.ClienteId] = (classe, cliente.Faturado);
            }
        }

        var contagem = new Dictionary<ClasseDeCliente, int>();
        var comClasse = classePorCliente.Keys.ToHashSet();

        foreach (var bloco in classePorCliente.Chunk(TamanhoDoBloco))
        {
            await using var contextoDoBloco = abrirContexto();
            var ids = bloco.Select(p => p.Key).ToList();
            var clientes = await contextoDoBloco.Clientes.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

            foreach (var cliente in clientes)
            {
                var (classe, faturado) = classePorCliente[cliente.Id];
                cliente.ApurarClasse(classe, faturado, agora, usuarioResponsavelId);
                contagem[classe] = contagem.GetValueOrDefault(classe) + 1;
            }

            await contextoDoBloco.SaveChangesAsync(ct);
        }

        // D É QUEM NÃO COMPROU, e ele é a maioria. Marcar explicitamente, em vez de deixar nulo,
        // é o que permite a tela dizer "3.100 clientes D sem visita há mais de 360 dias" — com
        // nulo ela só poderia dizer "sem classe", que não é uma métrica.
        var semFaturamento = await contexto.Clientes
            .Where(c => c.ExcluidoEm == null && !comClasse.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(ct);

        foreach (var bloco in semFaturamento.Chunk(TamanhoDoBloco))
        {
            await using var contextoDoBloco = abrirContexto();
            var ids = bloco.ToList();
            var clientes = await contextoDoBloco.Clientes.Where(c => ids.Contains(c.Id)).ToListAsync(ct);

            foreach (var cliente in clientes)
                cliente.ApurarClasse(ClasseDeCliente.D, 0m, agora, usuarioResponsavelId);

            contagem[ClasseDeCliente.D] = contagem.GetValueOrDefault(ClasseDeCliente.D) + clientes.Count;
            await contextoDoBloco.SaveChangesAsync(ct);
        }

        return contagem;
    }

    /// <summary>
    /// As raízes de CNPJ que não são cliente.
    ///
    /// <para><b>A raiz, e não o nome.</b> "TRACBEL AGRO NOR.", "TRACBEL AGRO NORTE" e "TRACBEL
    /// AGRO NOROESTE" são a mesma empresa escrita de três jeitos; a raiz <c>09507371</c> é uma só.
    /// Classificar por nome é o tipo de regra que funciona no dia em que foi escrita e falha na
    /// primeira vez que alguém digita diferente.</para>
    ///
    /// <para>Medido em três anos: a fábrica levou R$ 96,6 milhões e a empresa irmã R$ 27,4
    /// milhões, <b>tudo com CFOP 5102 — idêntico a uma venda</b>. Só o documento separa. Sem esta
    /// tabela, R$ 124 milhões de transferência entrariam no faturamento comercial da diretoria e
    /// na meta de alguém.</para>
    /// </summary>
    private static readonly Dictionary<string, NaturezaDoParceiro> NaturezaPorRaizDeCnpj = new(StringComparer.Ordinal)
    {
        ["89674782"] = NaturezaDoParceiro.Fabrica,       // John Deere Brasil
        ["09507371"] = NaturezaDoParceiro.EmpresaDoGrupo, // Tracbel Agro Norte
        ["03258870"] = NaturezaDoParceiro.EmpresaDoGrupo  // Tracbel — quatro cadastros na mesma raiz
    };

    /// <summary>
    /// O que é a contraparte de uma nota que não achou cliente no CRM.
    ///
    /// <para>Só a fábrica e o grupo são reconhecidos por raiz. <b>Revenda não entra aqui</b>: não
    /// há lista confiável de concessionária, e adivinhar por "MÁQUINAS" no nome classificaria como
    /// revenda qualquer cliente cujo nome fantasia tenha a palavra. Quem não é fábrica nem grupo
    /// entra como cadastro faltando — que é o palpite certo na dúvida, porque é o que faz alguém
    /// olhar em vez de arquivar.</para>
    /// </summary>
    /// <param name="documento">CPF ou CNPJ, só dígitos.</param>
    private static NaturezaDoParceiro ClassificarParceiro(string documento)
    {
        if (documento.Length == 0) return NaturezaDoParceiro.SemDocumento;

        return documento.Length == 14
            && NaturezaPorRaizDeCnpj.TryGetValue(documento[..8], out var natureza)
                ? natureza
                : NaturezaDoParceiro.ClienteNaoCadastrado;
    }

    /// <summary>
    /// Grava o faturamento cuja contraparte não é cliente do CRM.
    ///
    /// <para>Não chama <c>SaveChanges</c>: entra na mesma transação do bloco de quem tem cliente,
    /// para que uma carga interrompida não deixe metade de um mês de um lado e metade do outro.
    /// </para>
    /// </summary>
    /// <param name="contexto">O contexto do bloco, já dentro da transação.</param>
    /// <param name="linhas">As linhas sem cliente, com a filial já resolvida.</param>
    /// <param name="ct">Cancelamento.</param>
    private static async Task GravarSemClienteAsync(
        CrmDbContext contexto,
        IEnumerable<(FaturamentoParaCarga Linha, int EmpresaId)> linhas,
        CancellationToken ct)
    {
        var doBloco = linhas.ToList();
        if (doBloco.Count == 0) return;

        var documentos = doBloco.Select(x => x.Linha.DocumentoDoCliente).Distinct().ToList();

        var existentes = await contexto.FaturamentoSemClientes
            .Where(f => documentos.Contains(f.Documento))
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

    /// <summary>
    /// O valor em reais cheios, para a linha de decisão da carga.
    ///
    /// <para>Sem centavos de propósito: quem lê o relatório da carga está decidindo se um descarte
    /// é resíduo ou é problema, e essa decisão se toma na ordem de grandeza.</para>
    /// </summary>
    /// <param name="valor">O valor a formatar.</param>
    private static string Reais(decimal valor) => valor.ToString("C0", CulturaDoRelatorio);

    private static readonly System.Globalization.CultureInfo CulturaDoRelatorio =
        System.Globalization.CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>
    /// Os códigos de filial que existem no Protheus, para a leitura ir uma a uma.
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    private async Task<IReadOnlyList<string>> FiliaisDoProtheusAsync(CancellationToken ct)
    {
        await using var contexto = abrirContexto();

        var codigos = await contexto.Empresas.AsNoTracking()
            .Select(e => e.Codigo)
            .ToListAsync(ct);

        return
        [
            .. codigos
                .Where(c => c.Length == 6 && c.All(char.IsAsciiDigit))
                .Order(StringComparer.Ordinal)
        ];
    }

    /// <summary>O identificador de cada filial em operação, pelo código de seis dígitos.</summary>
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
}

/// <summary>O que a etapa de faturamento produziu, para quem a chamou montar o resumo.</summary>
/// <param name="Gravados">Meses de cliente gravados novos.</param>
/// <param name="Curva">Quantos clientes ficaram em cada classe.</param>
internal sealed record FaturamentoCarregado(
    int Gravados,
    Dictionary<ClasseDeCliente, int> Curva);

/// <summary>
/// O que a atualização só-faturamento fez.
/// </summary>
/// <param name="MesesGravados">Quantos meses de cliente entraram novos.</param>
/// <param name="Decisoes">
/// O que foi descartado e por quê — com o valor em reais no próprio texto, porque a ordem de
/// grandeza é o que separa resíduo de problema.
/// </param>
internal sealed record ResumoDoFaturamento(
    int MesesGravados,
    IReadOnlyDictionary<string, int> Decisoes);
