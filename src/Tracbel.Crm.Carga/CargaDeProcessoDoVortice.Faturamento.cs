using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Integracao;
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
/// </summary>
internal sealed partial class CargaDeProcessoDoVortice
{
    private const string FluxoDeFaturamento = "VORTICE.CARGA.FATURAMENTO";

    /// <summary>Onde a classe A termina: os clientes que somam os primeiros 80% do faturamento.</summary>
    private const decimal CorteDaClasseA = 0.80m;

    /// <summary>Onde a classe B termina.</summary>
    private const decimal CorteDaClasseB = 0.95m;

    private async Task<(int Gravados, int SemCliente)> GravarFaturamentoAsync(
        int sistemaId,
        IReadOnlyList<FaturamentoParaCarga> faturamento,
        CancellationToken ct)
    {
        if (faturamento.Count == 0) return (0, 0);

        var porDocumento = await MapaDeClientesPorDocumentoAsync(ct);
        var gravados = 0;
        var semCliente = 0;
        var documentosSemCliente = new HashSet<string>(StringComparer.Ordinal);

        foreach (var bloco in faturamento.Chunk(TamanhoDoBloco))
        {
            await using var contexto = abrirContexto();
            await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

            // A CHAVE NATURAL É (cliente, filial, competência), e é ela que torna a carga
            // reexecutável: quem já existe é reapurado, não duplicado.
            var doBloco = bloco
                .Select(f => new
                {
                    Linha = f,
                    ClienteId = porDocumento.GetValueOrDefault(f.DocumentoDoCliente),
                    EmpresaId = deParaDeFiliais.GetValueOrDefault(f.CodigoDaFilialNoLegado)
                })
                .Where(x => x.ClienteId != 0 && x.EmpresaId != 0)
                .ToList();

            foreach (var fora in bloco.Where(f => !porDocumento.ContainsKey(f.DocumentoDoCliente)))
            {
                if (documentosSemCliente.Add(fora.DocumentoDoCliente)) semCliente++;
            }

            if (doBloco.Count == 0)
            {
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
                    jaExiste.Reapurar(item.Linha.ValorLiquido, item.Linha.Notas, item.Linha.Itens);
                    continue;
                }

                contexto.FaturamentoDosClientes.Add(FaturamentoDoCliente.Criar(
                    item.EmpresaId, item.ClienteId, item.Linha.Competencia,
                    item.Linha.ValorLiquido, item.Linha.Notas, item.Linha.Itens));

                gravados++;
            }

            await contexto.SaveChangesAsync(ct);
            await transacao.CommitAsync(ct);
        }

        if (semCliente > 0)
            Decidir(
                "Documentos com faturamento que não casaram com cliente carregado " +
                "(são de filiais fora do recorte da Agro)", semCliente);

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
