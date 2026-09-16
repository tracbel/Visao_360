using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Infraestrutura.Multiempresa;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Carga;

/// <summary>
/// O que toda carga precisa antes de gravar: o de-para das filiais e quem responde pelos registros.
/// Usado pela carga de console e pelo serviço de sincronização, que o refaz a cada ciclo — filial
/// desativada ou usuário novo valem na rodada seguinte, sem reiniciar o serviço.
/// </summary>
internal static class PreparacaoDaCarga
{
    /// <summary>
    /// Monta o DE-PARA DE FILIAIS a partir do banco, nao de um arquivo.
    ///
    /// <para>O codigo oficial da filial e 0101NN, e NN e o numero da filial no sistema de origem — a
    /// correspondencia esta registrada em dados-referencia/empresa.json e ja foi semeada em
    /// organizacao.Empresa. Ler daqui garante que a carga e a API concordem sobre quais filiais
    /// existem: se uma filial for desativada amanha, a carga para de trazer cliente para ela sem
    /// ninguem precisar lembrar de mexer aqui.</para>
    /// </summary>
    /// <param name="opcoes">O banco do CRM.</param>
    /// <param name="diario">O diário de alcance entre empresas.</param>
    /// <param name="ct">Cancelamento.</param>
    internal static async Task<(Dictionary<int, int> DePara, long UsuarioId, int EmpresaId, string? Erro)> PrepararAsync(
        DbContextOptions<CrmDbContext> opcoes, DiarioDeAlcanceEntreEmpresasEmLog diario, CancellationToken ct = default)
    {
        var provisorio = new ContextoDeCargaDeSistema(0, 0, new HashSet<int>());
        await using var contexto = new CrmDbContext(opcoes, provisorio, diario);

        var empresas = await contexto.Empresas.AsNoTracking()
            .Where(e => e.EstaAtiva)
            .Select(e => new { e.Id, e.Codigo })
            .ToListAsync(ct);

        var dePara = new Dictionary<int, int>();

        foreach (var empresa in empresas)
        {
            if (empresa.Codigo.Length != 6
                || !empresa.Codigo.StartsWith("0101", StringComparison.Ordinal)
                || !int.TryParse(empresa.Codigo.AsSpan(4), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var numero))
                continue;

            dePara[numero] = empresa.Id;
        }

        if (dePara.Count == 0)
            return ([], 0, 0,
                "Nenhuma filial ativa no padrão 0101NN foi encontrada em organizacao.Empresa. Rode o " +
                "seed de dados de referência antes da carga: ./scripts/banco/rodar-seed.ps1");

        var usuario = await contexto.Usuarios.AsNoTracking()
            .Where(u => u.EstaAtivo)
            .OrderBy(u => u.Id)
            .FirstOrDefaultAsync(ct);

        return usuario is null
            ? ([], 0, 0,
                "Nenhum usuário ativo em seguranca.Usuario. Todo registro criado precisa de um " +
                "responsável, e a coluna é chave estrangeira: rode o seed de usuários de " +
                "desenvolvimento antes da carga.")
            : (dePara, usuario.Id, usuario.EmpresaId, null);
    }
}
