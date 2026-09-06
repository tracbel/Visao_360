using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Carga;

/// <summary>
/// O CATÁLOGO DE FROTA CRESCENDO A PARTIR DO DADO REAL — marca, família e modelo.
///
/// <para><b>O problema:</b> <c>frota.Equipamento.ModeloId</c> é chave estrangeira obrigatória, e
/// o catálogo semeado tem 18 modelos vindos do protótipo. O sistema legado guarda marca e modelo
/// como <b>texto livre</b>, e o recorte traz mais de mil textos distintos de modelo. Sem
/// catálogo, nenhuma máquina entra.</para>
///
/// <para><b>Isto não é inventar dado.</b> A marca e o modelo já existem lá — em texto, sem
/// catálogo por trás, que é justamente o defeito. O que a carga faz é <b>promover o valor
/// existente a linha de catálogo</b>, que é onde ele deveria ter nascido. Nada é criado sem uma
/// linha do legado por trás, e o relatório conta quantas linhas de catálogo nasceram assim.</para>
///
/// <para><b>A família vem da categoria do legado</b> — Trator, Colheitadeira Grãos, Pulverizador
/// —, que é exatamente o nível intermediário que <c>frota.Familia</c> descreve. As sete famílias
/// semeadas chamam-se "A confirmar" porque ninguém sabia quais eram; agora se sabe, e elas
/// entram com o nome que o negócio já usa.</para>
///
/// <para><b>Reaproveitar antes de criar.</b> Um modelo cujo código já existe no catálogo, na
/// mesma marca, é reusado — é o que faz os 18 modelos do protótipo continuarem valendo em vez de
/// ganharem um irmão gêmeo. O código é derivado do texto de forma determinística, então rodar de
/// novo reencontra a mesma linha em vez de criar outra.</para>
/// </summary>
internal sealed class CatalogoDeFrotaDaCarga
{
    private readonly Dictionary<string, int> _marcas = new(StringComparer.Ordinal);
    private readonly Dictionary<(int Marca, string Codigo), int> _familias = [];
    private readonly Dictionary<string, int> _modelos = new(StringComparer.Ordinal);
    private readonly Dictionary<int, int> _marcaDaFamilia = [];

    /// <summary>
    /// O trio marca/categoria/modelo do legado já resolvido.
    ///
    /// A carga resolve o catálogo inteiro numa passada antes de gravar máquina nenhuma; sem este
    /// cache, cada uma das dezenas de milhares de máquinas voltaria a perguntar ao banco por um
    /// modelo que já se sabe qual é.
    /// </summary>
    private readonly Dictionary<(string Marca, string Categoria, string Modelo), int> _porTrio = [];

    /// <summary>Quantas marcas a carga precisou criar.</summary>
    public int MarcasCriadas { get; private set; }

    /// <summary>Quantas famílias a carga precisou criar.</summary>
    public int FamiliasCriadas { get; private set; }

    /// <summary>Quantos modelos a carga precisou criar.</summary>
    public int ModelosCriados { get; private set; }

    /// <summary>Carrega o catálogo que já existe, para reusar antes de criar.</summary>
    /// <param name="contexto">O contexto do banco do CRM.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task CarregarAsync(CrmDbContext contexto, CancellationToken ct)
    {
        foreach (var marca in await contexto.Marcas.AsNoTracking().ToListAsync(ct))
            _marcas[marca.Codigo] = marca.Id;

        foreach (var familia in await contexto.Familias.AsNoTracking().ToListAsync(ct))
        {
            _familias[(familia.MarcaId, familia.Codigo)] = familia.Id;
            _marcaDaFamilia[familia.Id] = familia.MarcaId;
        }

        foreach (var modelo in await contexto.Modelos.AsNoTracking().ToListAsync(ct))
            _modelos[modelo.Codigo] = modelo.Id;
    }

    /// <summary>
    /// Devolve o modelo correspondente ao trio marca/categoria/modelo do legado, criando o que
    /// faltar. As criações são gravadas junto com a máquina, na mesma transação.
    /// </summary>
    /// <param name="contexto">O contexto do banco do CRM.</param>
    /// <param name="marca">A marca declarada no legado.</param>
    /// <param name="categoria">A categoria do legado, que vira família.</param>
    /// <param name="modelo">O modelo declarado no legado.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <returns>O identificador do modelo no catálogo.</returns>
    public async Task<int> ResolverModeloAsync(
        CrmDbContext contexto, string marca, string categoria, string modelo, CancellationToken ct)
    {
        if (_porTrio.TryGetValue((marca, categoria, modelo), out var jaResolvido)) return jaResolvido;

        var marcaId = await ResolverMarcaAsync(contexto, marca, ct);
        var familiaId = await ResolverFamiliaAsync(contexto, marcaId, categoria, ct);

        var codigo = Codificar(modelo, 40);

        // Um código que já existe NA MESMA MARCA é o mesmo modelo — reusa. Num modelo de outra
        // marca, o código ganha o prefixo da marca: "5075E" da John Deere e "5075E" de outra
        // fabricante são máquinas diferentes, e uma coluna de código única no catálogo inteiro
        // não pode fundir as duas em silêncio.
        if (_modelos.TryGetValue(codigo, out var existente))
        {
            var modeloExistente = await contexto.Modelos.AsNoTracking()
                .FirstAsync(m => m.Id == existente, ct);

            if (_marcaDaFamilia.TryGetValue(modeloExistente.FamiliaId, out var marcaDoExistente)
                && marcaDoExistente == marcaId)
                return _porTrio[(marca, categoria, modelo)] = existente;

            codigo = Codificar($"{marca}-{modelo}", 40);
            if (_modelos.TryGetValue(codigo, out var jaPrefixado))
                return _porTrio[(marca, categoria, modelo)] = jaPrefixado;
        }

        var novo = Modelo.Criar(familiaId, codigo, Encurtar(modelo, 120));
        contexto.Modelos.Add(novo);
        await contexto.SaveChangesAsync(ct);

        _modelos[codigo] = novo.Id;
        _porTrio[(marca, categoria, modelo)] = novo.Id;
        ModelosCriados++;
        return novo.Id;
    }

    private async Task<int> ResolverMarcaAsync(CrmDbContext contexto, string marca, CancellationToken ct)
    {
        var codigo = Codificar(marca, 20);
        if (_marcas.TryGetValue(codigo, out var id)) return id;

        var nova = Marca.Criar(codigo, Encurtar(marca, 80));
        contexto.Marcas.Add(nova);
        await contexto.SaveChangesAsync(ct);

        _marcas[codigo] = nova.Id;
        MarcasCriadas++;
        return nova.Id;
    }

    private async Task<int> ResolverFamiliaAsync(
        CrmDbContext contexto, int marcaId, string categoria, CancellationToken ct)
    {
        var codigo = Codificar(categoria, 20);
        if (_familias.TryGetValue((marcaId, codigo), out var id)) return id;

        var nova = Familia.Criar(marcaId, codigo, Encurtar(categoria, 80));
        contexto.Familias.Add(nova);
        await contexto.SaveChangesAsync(ct);

        _familias[(marcaId, codigo)] = nova.Id;
        _marcaDaFamilia[nova.Id] = marcaId;
        FamiliasCriadas++;
        return nova.Id;
    }

    /// <summary>
    /// Deriva o CÓDIGO ESTÁVEL a partir do texto: sem acento, maiúsculo, só letra, número e
    /// sublinhado.
    ///
    /// <para>Precisa ser determinístico porque é ele que faz a recarga reencontrar a mesma linha
    /// em vez de criar outra. "Colheitadeira Grãos" e "COLHEITADEIRA GRAOS" produzem o mesmo
    /// código, que é o comportamento certo: são o mesmo conceito escrito de dois jeitos — o
    /// defeito que fez <c>Afericao</c> e <c>Aferição</c> conviverem no legado.</para>
    /// </summary>
    private static string Codificar(string texto, int tamanhoMaximo)
    {
        var semAcento = texto.Normalize(NormalizationForm.FormD);
        var construtor = new StringBuilder(semAcento.Length);

        foreach (var caractere in semAcento)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark) continue;

            construtor.Append(char.IsAsciiLetterOrDigit(caractere)
                ? char.ToUpperInvariant(caractere)
                : '_');
        }

        var codigo = construtor.ToString();

        while (codigo.Contains("__", StringComparison.Ordinal))
            codigo = codigo.Replace("__", "_", StringComparison.Ordinal);

        codigo = codigo.Trim('_');
        if (codigo.Length > tamanhoMaximo) codigo = codigo[..tamanhoMaximo].TrimEnd('_');

        return codigo.Length == 0 ? "SEM_CODIGO" : codigo;
    }

    private static string Encurtar(string texto, int tamanhoMaximo) =>
        texto.Length <= tamanhoMaximo ? texto : texto[..tamanhoMaximo];
}
