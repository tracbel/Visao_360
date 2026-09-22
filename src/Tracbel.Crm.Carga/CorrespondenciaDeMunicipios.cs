using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Carga;

/// <summary>
/// O DE-PARA DE MUNICÍPIO DE UMA FONTE, durante uma carga (issue 154).
///
/// <para><b>A correspondência é feita uma vez e gravada.</b> Na primeira rodada, a chave da fonte — o código
/// dela, quando tem; o nome normalizado, quando não tem — é casada com o catálogo pelo nome sem acento, caixa
/// e apóstrofo, e o par vai para <c>organizacao.CorrespondenciaDeMunicipio</c>. Nas rodadas seguintes o par
/// gravado responde, e nenhum nome é comparado de novo: grafia nova na fonte não desfaz o que já estava
/// casado, e nome que passe a existir em dois municípios não muda o resultado em silêncio.</para>
///
/// <para><b>Sem par, é recusa.</b> Nome que casa com dois municípios da UF, ou com nenhum, não vira
/// adivinhação: a linha é recusada com o motivo e aparece no painel de fontes, para alguém resolver à mão.</para>
///
/// <para><b>Os contadores são a prova.</b> <see cref="CasadosPelaCorrespondencia"/> e
/// <see cref="CasadosPorNome"/> saem no relatório da carga: na segunda rodada, o segundo tem de ser zero.</para>
/// </summary>
internal sealed class CorrespondenciaDeMunicipios
{
    private readonly string _fonte;
    private readonly long _usuarioId;
    private readonly Dictionary<string, CorrespondenciaDeMunicipio> _gravadas;
    private readonly Dictionary<string, int> _municipioPorNome;
    private readonly List<CorrespondenciaDeMunicipio> _novas = [];

    private CorrespondenciaDeMunicipios(
        string fonte, long usuarioId, Dictionary<string, CorrespondenciaDeMunicipio> gravadas, Dictionary<string, int> municipioPorNome)
    {
        _fonte = fonte;
        _usuarioId = usuarioId;
        _gravadas = gravadas;
        _municipioPorNome = municipioPorNome;
    }

    /// <summary>Quantas linhas foram resolvidas pelo par já gravado.</summary>
    public int CasadosPelaCorrespondencia { get; private set; }

    /// <summary>Quantos pares novos esta rodada estabeleceu pelo nome.</summary>
    public int CasadosPorNome => _novas.Count;

    /// <summary>Quantas chaves a fonte trouxe e o catálogo não resolveu.</summary>
    public int SemPar { get; private set; }

    /// <summary>Quantos municípios do catálogo a UF tem com nome único — o denominador do casamento por nome.</summary>
    public int MunicipiosNoCatalogo => _municipioPorNome.Count;

    /// <summary>
    /// Abre o de-para de uma fonte: os pares já gravados e o catálogo da UF por nome normalizado.
    /// </summary>
    /// <param name="contexto">O contexto da carga.</param>
    /// <param name="fonte">O fluxo da fonte.</param>
    /// <param name="uf">A UF do catálogo (hoje, sempre SP).</param>
    /// <param name="usuarioId">Quem roda a carga.</param>
    /// <param name="ct">Cancelamento.</param>
    public static async Task<CorrespondenciaDeMunicipios> AbrirAsync(
        CrmDbContext contexto, string fonte, string uf, long usuarioId, CancellationToken ct)
    {
        var gravadas = (await contexto.CorrespondenciasDeMunicipios.Where(c => c.Fonte == fonte).ToListAsync(ct))
            .ToDictionary(c => c.ChaveNaFonte, StringComparer.Ordinal);

        // NOME QUE APARECE EM DOIS MUNICÍPIOS DA UF FICA DE FORA: o casamento por nome só vale quando é único.
        var municipioPorNome = (await contexto.Municipios.AsNoTracking()
                .Where(m => m.Uf == uf && m.CodigoIbge != null)
                .Select(m => new { m.Id, m.Nome })
                .ToListAsync(ct))
            .GroupBy(m => SaneamentoDeTerritorio.ChaveSemApostrofo(m.Nome))
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.Single().Id, StringComparer.Ordinal);

        return new CorrespondenciaDeMunicipios(fonte, usuarioId, gravadas, municipioPorNome);
    }

    /// <summary>
    /// Resolve o município de uma linha da fonte.
    /// </summary>
    /// <param name="chaveNaFonte">O código da fonte, ou o nome quando ela não tem código.</param>
    /// <param name="nome">O nome do município como a fonte o publica.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <param name="municipioId">O município do catálogo, quando há par.</param>
    /// <returns>Verdadeiro quando há par; falso quando a linha precisa ser recusada.</returns>
    public bool Resolver(string chaveNaFonte, string nome, DateTime agoraUtc, out int municipioId)
    {
        var chave = SaneamentoDeTerritorio.ChaveSemApostrofo(chaveNaFonte);

        if (_gravadas.TryGetValue(chave, out var gravada))
        {
            municipioId = gravada.MunicipioId;
            CasadosPelaCorrespondencia++;
            return true;
        }

        if (!_municipioPorNome.TryGetValue(SaneamentoDeTerritorio.ChaveSemApostrofo(nome), out municipioId))
        {
            municipioId = 0;
            SemPar++;
            return false;
        }

        var nova = CorrespondenciaDeMunicipio.Registrar(
            _fonte, chave, nome, municipioId, FormaDaCorrespondencia.NomeUnicoNaUf, _usuarioId, agoraUtc);

        _gravadas[chave] = nova;
        _novas.Add(nova);
        return true;
    }

    /// <summary>A frase da recusa de uma chave sem par — a mesma em todas as fontes.</summary>
    /// <param name="nome">O nome como a fonte o publica.</param>
    public static string MotivoSemPar(string nome) =>
        $"\"{nome}\" não é, sem ambiguidade, um município do catálogo. A correspondência precisa ser resolvida à mão.";

    /// <summary>Põe os pares novos na transação da carga, para serem gravados com o resto.</summary>
    /// <param name="contexto">O contexto da carga.</param>
    public void Gravar(CrmDbContext contexto) => contexto.CorrespondenciasDeMunicipios.AddRange(_novas);
}
