using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O painel por responsável — quem cobriu o quê, e o que deixou passar.
///
/// ---------------------------------------------------------------------------------------------
/// O QUE "COBERTO" SIGNIFICA AQUI, e por que não é um número redondo escolhido por alguém.
///
/// Um cliente está coberto quando a última interação dele cabe dentro da <b>cadência que o
/// negócio declarou para a classe dele naquela linha de negócio</b>. Os números são do legado, e
/// são estes: Venda de Máquinas e Implemento visita A, B e C a cada 180 dias e D a cada 360;
/// Prospecção usa 120/120/120/180; AMS e Peças usam 360 nos quatro.
///
/// <para>Onde a linha <b>não declara</b> cadência — dezoito dos vinte e nove departamentos —, o
/// cliente não entra nem como coberto nem como atrasado. Ele sai numa terceira coluna, que é a
/// resposta honesta: não há prazo contra o que medi-lo. Jogá-lo para qualquer um dos dois lados
/// inventaria uma meta que ninguém definiu.</para>
///
/// <para><b>Três estados, e não dois:</b> coberto, fora da cadência e nunca contatado. Somar os
/// dois últimos esconderia a diferença mais acionável da tela — quem o CEN conhece e deixou
/// vencer não é o mesmo problema que quem ele nunca procurou.</para>
///
/// <para>Como em todo repositório, <b>não há um <c>Where</c> de empresa neste arquivo</b>: a
/// fronteira de filial entra sozinha pelo filtro global.</para>
/// </summary>
public sealed class RepositorioDoPainelDoCen(CrmDbContext contexto) : IRepositorioPainelDoCen
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<(Guid Chave, string Nome, string Natureza, int Carteiras)>>
        ListarResponsaveisAsync(CancellationToken ct)
    {
        // SÓ QUEM TEM CARTEIRA COMERCIAL entra no seletor. Um seletor com 276 nomes, dos quais
        // 245 não respondem por carteira nenhuma, não é um seletor — é uma lista telefônica.
        var responsaveis = await contexto.Carteiras.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Natureza == NaturezaDaCarteira.Comercial)
            .GroupBy(c => c.ResponsavelId)
            .Select(g => new { ResponsavelId = g.Key, Carteiras = g.Count() })
            .ToListAsync(ct);

        var ids = responsaveis.Select(r => r.ResponsavelId).ToList();

        var pessoas = await contexto.Usuarios.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.ChavePublica, u.NomeExibicao, u.Natureza })
            .ToDictionaryAsync(u => u.Id, ct);

        return
        [
            .. responsaveis
                .Where(r => pessoas.ContainsKey(r.ResponsavelId))
                .Select(r => (
                    pessoas[r.ResponsavelId].ChavePublica,
                    pessoas[r.ResponsavelId].NomeExibicao,
                    pessoas[r.ResponsavelId].Natureza.ToString(),
                    r.Carteiras))
                .OrderBy(r => r.NomeExibicao, StringComparer.CurrentCultureIgnoreCase)
        ];
    }

    /// <inheritdoc />
    public async Task<PainelDoResponsavel?> ObterPainelAsync(
        Guid? responsavelChave, DateTime agoraUtc, CancellationToken ct)
    {
        long? responsavelId = null;
        var nome = "Todos os responsáveis";
        var natureza = "Consolidado";
        var chave = responsavelChave ?? Guid.Empty;

        if (responsavelChave is { } procurada)
        {
            var pessoa = await contexto.Usuarios.AsNoTracking()
                .Where(u => u.ChavePublica == procurada)
                .Select(u => new { u.Id, u.NomeExibicao, u.Natureza })
                .FirstOrDefaultAsync(ct);

            if (pessoa is null) return null;

            responsavelId = pessoa.Id;
            nome = pessoa.NomeExibicao;
            natureza = pessoa.Natureza.ToString();
        }

        // As carteiras COMERCIAIS do responsável, com a cadência de cada classe já junto: é ela
        // que decide se um cliente está coberto, e ela vem da linha de negócio da carteira.
        var carteiras = await contexto.Carteiras.AsNoTracking()
            .Where(c => c.ExcluidoEm == null
                        && c.Natureza == NaturezaDaCarteira.Comercial
                        && (responsavelId == null || c.ResponsavelId == responsavelId))
            .Select(c => new
            {
                c.Id,
                Cadencia = contexto.LinhasDeNegocio
                    .Where(l => l.Id == c.LinhaDeNegocioId)
                    .Select(l => new
                    {
                        l.DiasCicloClasseA,
                        l.DiasCicloClasseB,
                        l.DiasCicloClasseC,
                        l.DiasCicloClasseD
                    })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        if (carteiras.Count == 0)
        {
            return new PainelDoResponsavel(
                chave, nome, natureza, 0, 0, [], 0, 0, 0, 0, 0m);
        }

        var idsDeCarteira = carteiras.Select(c => c.Id).ToList();

        // O VÍNCULO É A UNIDADE, e não o cliente: o mesmo cliente está em várias carteiras, e
        // cada uma tem a sua cadência. Contar cliente distinto aqui esconderia que ele está
        // coberto em Peças e vencido em Máquinas.
        var vinculos = await contexto.ClienteCarteiras.AsNoTracking()
            .Where(v => v.DesvinculadoEm == null && idsDeCarteira.Contains(v.CarteiraId))
            .Select(v => new
            {
                v.CarteiraId,
                v.ClienteId,
                v.UltimaInteracaoEm,
                Classe = contexto.Clientes.Where(c => c.Id == v.ClienteId).Select(c => c.Classe).FirstOrDefault()
            })
            .ToListAsync(ct);

        var cadenciaPorCarteira = carteiras.ToDictionary(c => c.Id, c => c.Cadencia);

        var porClasse = new Dictionary<ClasseDeCliente, Acumulador>();

        foreach (var vinculo in vinculos)
        {
            // SEM CLASSE APURADA, O VÍNCULO CONTA COMO D. A curva ABC marca D quem não comprou,
            // e cliente sem apuração nenhuma é justamente quem nunca apareceu no faturamento.
            var classe = vinculo.Classe ?? ClasseDeCliente.D;
            var cadencia = cadenciaPorCarteira.GetValueOrDefault(vinculo.CarteiraId);

            short? dias = classe switch
            {
                ClasseDeCliente.A => cadencia?.DiasCicloClasseA,
                ClasseDeCliente.B => cadencia?.DiasCicloClasseB,
                ClasseDeCliente.C => cadencia?.DiasCicloClasseC,
                _ => cadencia?.DiasCicloClasseD
            };

            var acumulador = porClasse.TryGetValue(classe, out var jaExiste)
                ? jaExiste
                : porClasse[classe] = new Acumulador();

            acumulador.Clientes++;
            // GUARDA TODAS AS CADÊNCIAS VISTAS, e não a primeira. A mesma classe aparece em
            // carteiras de linhas diferentes — 180 dias em Máquinas, 360 em Peças —, e mostrar
            // "classe A · 360 dias" quando metade dela é medida contra 180 seria afirmar um
            // prazo que não vale para todo mundo daquela linha.
            if (dias is { } declarada) acumulador.Cadencias.Add(declarada);

            if (vinculo.UltimaInteracaoEm is null) acumulador.NuncaContatados++;
            else if (dias is null) acumulador.SemCadenciaDeclarada++;
            else if (vinculo.UltimaInteracaoEm >= agoraUtc.AddDays(-dias.Value)) acumulador.Cobertos++;
            else acumulador.ForaDaCadencia++;
        }

        var clientesDoResponsavel = vinculos.Select(v => v.ClienteId).Distinct().ToList();

        var processos = await contexto.Processos.AsNoTracking()
            .Where(p => p.ExcluidoEm == null && clientesDoResponsavel.Contains(p.ClienteId))
            .GroupBy(p => p.Situacao)
            .Select(g => new { Situacao = g.Key, Quantidade = g.Count() })
            .ToListAsync(ct);

        var vendasPerdidas = await contexto.VendasPerdidas.AsNoTracking()
            .CountAsync(v => v.ExcluidoEm == null
                             && v.ClienteId != null
                             && clientesDoResponsavel.Contains(v.ClienteId.Value), ct);

        var faturamento = await contexto.FaturamentoDosClientes.AsNoTracking()
            .Where(f => f.ExcluidoEm == null && clientesDoResponsavel.Contains(f.ClienteId))
            .SumAsync(f => (decimal?)f.ValorLiquido, ct) ?? 0m;

        return new PainelDoResponsavel(
            chave,
            nome,
            natureza,
            carteiras.Count,
            vinculos.Count,
            [
                .. porClasse
                    .OrderBy(p => p.Key)
                    .Select(p => new CoberturaPorClasse(
                        p.Key.ToString(),
                        p.Value.Clientes,
                        p.Value.Cobertos,
                        p.Value.ForaDaCadencia,
                        p.Value.NuncaContatados,
                        p.Value.SemCadenciaDeclarada,
                        // Uma cadência só: mostra o número. Mais de uma: nulo, e a tela escreve
                        // que varia por linha de negócio.
                        p.Value.Cadencias.Count == 1 ? p.Value.Cadencias.Single() : null))
            ],
            processos.FirstOrDefault(p => p.Situacao == SituacaoDoProcesso.Ganho)?.Quantidade ?? 0,
            processos.FirstOrDefault(p => p.Situacao == SituacaoDoProcesso.Perdido)?.Quantidade ?? 0,
            processos.FirstOrDefault(p => p.Situacao == SituacaoDoProcesso.Aberto)?.Quantidade ?? 0,
            vendasPerdidas,
            decimal.Round(faturamento, 2));
    }

    /// <summary>O contador de uma classe enquanto os vínculos são percorridos.</summary>
    private sealed class Acumulador
    {
        public int Clientes;
        public int Cobertos;
        public int ForaDaCadencia;
        public int NuncaContatados;
        public int SemCadenciaDeclarada;
        public readonly HashSet<short> Cadencias = [];
    }
}
