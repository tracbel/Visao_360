using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A série anual da PAM no banco do CRM (issue 198).
///
/// <para><b>Nada é gravado.</b> O preço implícito é derivado de duas colunas que já existem desde a issue
/// 156; guardá-lo criaria uma terceira verdade para o mesmo número. Aqui só se lê e se soma.</para>
/// </summary>
public sealed class RepositorioDoPrecoImplicito(CrmDbContext contexto) : IRepositorioDoPrecoImplicito
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ProducaoAnualDoProduto>> LerAsync(int? municipioCodigoIbge, CancellationToken ct)
    {
        var producoes = contexto.ProducoesAgricolasNosMunicipios.AsNoTracking();

        // O RECORTE: um município, ou a ADR inteira. Sem filtro nenhum viria São Paulo todo, e a tela fala
        // da área de atuação — misturar os dois faria o preço "da região" ser o do estado.
        var doRecorte = municipioCodigoIbge is { } codigo
            ? from p in producoes
              join m in contexto.Municipios.AsNoTracking() on p.MunicipioId equals m.Id
              where m.CodigoIbge == codigo
              select p
            : from p in producoes
              join a in contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking() on p.MunicipioId equals a.MunicipioId
              where a.PertenceAAdr
              select p;

        // A SOMA VEM ANTES DA DIVISÃO — o preço do recorte é `Σ valor ÷ Σ quantidade`, a média ponderada
        // pela colheita de cada município. A média simples dos preços municipais daria o mesmo peso a quem
        // colheu dez toneladas e a quem colheu dez mil.
        //
        // `Sum` de coluna anulável no EF ignora os nulos e devolve nulo quando NENHUMA linha tem valor —
        // que é o que se quer: município sob sigilo não entra na soma, e sigilo de todos não vira zero.
        var somado = await doRecorte
            .GroupBy(p => new { p.ProdutoCodigoIbge, p.Ano })
            .Select(g => new
            {
                g.Key.ProdutoCodigoIbge,
                g.Key.Ano,
                Valor = g.Sum(p => p.ValorDaProducaoMilReais),
                Quantidade = g.Sum(p => p.QuantidadeProduzida),
                // QUANTOS MUNICÍPIOS SUSTENTAM A SOMA. Sem isso, "R$ 18.000 a tonelada na região" pode ser
                // de um município só, e a tela não teria como dizer.
                Municipios = g.Count(p => p.ValorDaProducaoMilReais != null && p.QuantidadeProduzida != null),
                Nome = g.Max(p => p.ProdutoNome),
            })
            .ToListAsync(ct);

        return
        [
            .. somado
                .OrderBy(s => s.Nome, StringComparer.Ordinal)
                .ThenByDescending(s => s.Ano)
                // `Max` sobre uma coluna de texto volta anulável pelo tipo, ainda que `ProdutoNome` seja
                // obrigatório na entidade: o grupo sempre tem pelo menos uma linha, e ela sempre tem nome.
                .Select(s => new ProducaoAnualDoProduto(
                    s.ProdutoCodigoIbge, s.Nome ?? string.Empty, s.Ano, s.Valor, s.Quantidade, s.Municipios))
        ];
    }
}
