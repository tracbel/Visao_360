using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso ao cadastro de clientes, sobre o <see cref="CrmDbContext"/>.
///
/// O QUE NÃO ESTÁ ESCRITO AQUI É O MAIS IMPORTANTE: não há nenhum <c>Where</c> de empresa em
/// lugar nenhum deste arquivo, e mesmo assim nenhuma consulta daqui atravessa a fronteira de
/// filial. O filtro global do <see cref="CrmDbContext"/> entra em TODA consulta da entidade,
/// antes de qualquer coisa que este repositório escreva.
///
/// Essa é a diferença entre "o repositório lembra de filtrar" e "não existe caminho que
/// esqueça" — e é a correção do achado A-1 do documento 21 sendo aproveitada em vez de
/// reimplementada. [V] no legado a segurança mora no cliente Gupta, e por isso 125 dos 134
/// relatórios não têm predicado de usuário nenhum: cada consulta nova precisava lembrar, e as
/// dos relatórios não lembraram.
/// </summary>
public sealed class RepositorioDeClientes(CrmDbContext contexto) : IRepositorioClientes
{
    /// <inheritdoc />
    public async Task<PaginaDe<ClienteComContexto>> ListarAsync(
        ConsultaDeClientes consulta, CancellationToken ct)
    {
        var linhas = Filtrar(contexto.Clientes.AsQueryable(), consulta);

        var total = await linhas.CountAsync(ct);

        var itens = await Ordenar(linhas, consulta)
            .Skip(consulta.Paginacao.Saltar)
            .Take(consulta.Paginacao.Tamanho)
            .Select(ComOsCodigosDeCatalogo())
            .ToListAsync(ct);

        return new PaginaDe<ClienteComContexto>(
            itens, consulta.Paginacao.Pagina, consulta.Paginacao.Tamanho, total);
    }

    /// <inheritdoc />
    public Task<ClienteComContexto?> ObterAsync(Guid chavePublica, bool incluirInativos, CancellationToken ct)
    {
        var linhas = contexto.Clientes.Where(c => c.ChavePublica == chavePublica);
        if (!incluirInativos) linhas = linhas.Where(c => c.ExcluidoEm == null);

        return linhas.Select(ComOsCodigosDeCatalogo()).FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task AdicionarAsync(Cliente cliente, CancellationToken ct) =>
        await contexto.Clientes.AddAsync(cliente, ct);

    /// <inheritdoc />
    public Task<Cliente?> ObterPorDocumentoAsync(CpfCnpj documento, int empresaId, Guid? exceto, CancellationToken ct)
    {
        // A conversão de CpfCnpj para coluna já está declarada no mapeamento; comparar pelo tipo
        // de valor é o que faz a consulta usar UX_Cliente_Empresa_Documento.
        var linhas = contexto.Clientes
            .Where(c => c.ExcluidoEm == null && c.EmpresaId == empresaId && c.Documento == documento);

        if (exceto is { } chave) linhas = linhas.Where(c => c.ChavePublica != chave);

        return linhas.FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Traz, junto do cliente, os CÓDIGOS dos itens de catálogo que ele aponta.
    ///
    /// São duas subconsultas correlacionadas, resolvidas pelo banco na mesma ida — em vez de N+1
    /// consultas depois, uma por linha da página, que é o modo silencioso de uma listagem de 25
    /// linhas virar 51 consultas.
    ///
    /// O par <c>(CatalogoId, Id)</c> na condição não é redundante: é a mesma amarração da chave
    /// estrangeira composta do documento 21, achado I-1. Ler pelo par garante que um
    /// <c>OrigemId</c> que porventura apontasse para item de outro catálogo apareça como nulo,
    /// e não como o rótulo errado.
    /// </summary>
    private System.Linq.Expressions.Expression<Func<Cliente, ClienteComContexto>> ComOsCodigosDeCatalogo() =>
        c => new ClienteComContexto(
            c,
            contexto.CatalogoItens
                .Where(i => i.CatalogoId == CatalogosDeSistema.OrigemDeLead && i.Id == c.OrigemId)
                .Select(i => i.Codigo)
                .FirstOrDefault(),
            contexto.CatalogoItens
                .Where(i => i.CatalogoId == CatalogosDeSistema.MotivoDeInativacao
                         && i.Id == c.MotivoInativacaoId)
                .Select(i => i.Codigo)
                .FirstOrDefault());

    private static IQueryable<Cliente> Filtrar(IQueryable<Cliente> linhas, ConsultaDeClientes consulta)
    {
        if (!consulta.IncluirInativos) linhas = linhas.Where(c => c.ExcluidoEm == null);

        if (consulta.Situacao is { } situacao) linhas = linhas.Where(c => c.Situacao == situacao);
        if (consulta.TipoDePessoa is { } tipo) linhas = linhas.Where(c => c.TipoDePessoa == tipo);
        if (consulta.ProprietarioId is { } dono) linhas = linhas.Where(c => c.ProprietarioId == dono);

        if (!string.IsNullOrWhiteSpace(consulta.Termo))
        {
            var termo = consulta.Termo.Trim();

            // O DOCUMENTO É COMPARADO POR VALOR INTEIRO; nome e nome fantasia, por trecho.
            //
            // Mesma razão do chassi em RepositorioDeEquipamentos: CpfCnpj é tipo de valor com
            // conversor, e o EF aplica o conversor da propriedade AO PARÂMETRO da comparação —
            // num LIKE isso vira uma tentativa de converter "%1234%" em CpfCnpj, e a consulta
            // estoura com "Invalid cast from 'System.String' to 'CpfCnpj'".
            //
            // Aceitar máscara continua valendo: quem cola "11.222.333/0001-81" da tela do ERP
            // encontra, porque o tipo de valor normaliza a entrada antes da comparação.
            //
            // DÍVIDA NOMEADA (documento 23): busca por PEDAÇO de documento — a raiz do CNPJ, por
            // exemplo — não funciona hoje. Vale a mesma correção aditiva do chassi.
            var achouDocumento = CpfCnpj.TentarCriar(termo, out var documentoProcurado);

            linhas = linhas.Where(c =>
                c.NomeRazao.Contains(termo)
                || (c.NomeFantasia != null && c.NomeFantasia.Contains(termo))
                || (achouDocumento && c.Documento == documentoProcurado));
        }

        return linhas;
    }

    private static IQueryable<Cliente> Ordenar(IQueryable<Cliente> linhas, ConsultaDeClientes consulta) =>
        (consulta.Ordem, consulta.Descendente) switch
        {
            (OrdemDeCliente.Nome, false) => linhas.OrderBy(c => c.NomeRazao).ThenBy(c => c.Id),
            (OrdemDeCliente.Nome, true) => linhas.OrderByDescending(c => c.NomeRazao).ThenBy(c => c.Id),

            (OrdemDeCliente.CriadoEm, false) => linhas.OrderBy(c => c.CriadoEm).ThenBy(c => c.Id),
            (OrdemDeCliente.CriadoEm, true) => linhas.OrderByDescending(c => c.CriadoEm).ThenBy(c => c.Id),

            (OrdemDeCliente.Situacao, false) => linhas.OrderBy(c => c.Situacao).ThenBy(c => c.NomeRazao),
            (OrdemDeCliente.Situacao, true) => linhas.OrderByDescending(c => c.Situacao).ThenBy(c => c.NomeRazao),

            (OrdemDeCliente.AlteradoEm, false) => linhas.OrderBy(c => c.AlteradoEm).ThenBy(c => c.Id),
            (OrdemDeCliente.AlteradoEm, true) => linhas.OrderByDescending(c => c.AlteradoEm).ThenBy(c => c.Id),

            // O domínio da ordenação é fechado por enum e conferido na entrada; se um valor novo
            // aparecer aqui, é entidade nova sem ordenação escrita, e a ordem estável por nome é
            // a resposta certa — nunca uma ordem indefinida, que muda de página para página.
            _ => linhas.OrderBy(c => c.NomeRazao).ThenBy(c => c.Id)
        };
}
