using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Clientes;

/// <summary>
/// Lista clientes do NOSSO banco, com paginação, filtro e ordenação.
///
/// O QUE ESTE CASO DE USO NÃO FAZ, e é o ponto mais importante dele: filtrar por empresa. A
/// fronteira de multiempresa é aplicada pelo filtro global do <c>CrmDbContext</c>, em TODA
/// consulta, e não existe caminho por aqui que a desligue — nem passando <c>EmpresaId</c> num
/// parâmetro, porque parâmetro de empresa não existe nesta consulta. [V] é a correção do achado
/// A-1 do documento 21, e é o que impede que uma listagem esquecida devolva as 13 filiais.
/// </summary>
public sealed class ListarClientes(IRepositorioClientes repositorio, IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="pagina">Página pedida, começando em 1.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="termo">Busca por razão social, nome fantasia ou documento.</param>
    /// <param name="situacao">Filtro por situação. Domínio fechado.</param>
    /// <param name="tipoDePessoa">Filtro por Fisica ou Juridica.</param>
    /// <param name="ordenarPor">Coluna de ordenação. Domínio fechado.</param>
    /// <param name="descendente">Ordem decrescente.</param>
    /// <param name="incluirInativos">Trazer também os inativados.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<ClienteResumo>>>> ExecutarAsync(
        int? pagina,
        int? tamanho,
        string? termo,
        string? situacao,
        string? tipoDePessoa,
        string? ordenarPor,
        bool descendente,
        bool incluirInativos,
        CancellationToken ct)
    {
        var erros = new ColetorDeErros();

        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<ClienteResumo>>>.FalhaDeValidacao(
                paginacao.Erro!, paginacao.Erros);

        var situacaoEscolhida = situacao is null
            ? null
            : erros.ItemDeDominio<SituacaoDoCliente>("situacao", situacao);

        var tipoEscolhido = tipoDePessoa is null
            ? null
            : erros.ItemDeDominio<TipoDePessoa>("tipoDePessoa", tipoDePessoa);

        var ordem = erros.ItemDeDominioOuPadrao("ordenarPor", ordenarPor, OrdemDeCliente.Nome);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<ClienteResumo>>>(
                "A consulta tem parâmetros que não valem.");

        var pagi = await repositorio.ListarAsync(
            new ConsultaDeClientes(
                paginacao.Valor,
                Termo: string.IsNullOrWhiteSpace(termo) ? null : termo.Trim(),
                Situacao: situacaoEscolhida,
                TipoDePessoa: tipoEscolhido,
                Ordem: ordem,
                Descendente: descendente,
                IncluirInativos: incluirInativos),
            ct);

        var resumo = new PaginaDe<ClienteResumo>(
            [.. pagi.Itens.Select(ClienteResumo.De)], pagi.Pagina, pagi.Tamanho, pagi.Total);

        return Resultado<ComProcedencia<PaginaDe<ClienteResumo>>>.Ok(
            ComProcedencia<PaginaDe<ClienteResumo>>.DoNossoBanco(resumo, "comercial.Cliente", relogio));
    }
}

/// <summary>
/// Traz a ficha de um cliente pela chave pública.
///
/// NÃO EXISTE, AQUI, DIFERENÇA ENTRE "não existe" E "não é seu": as duas coisas devolvem a mesma
/// recusa. Distinguir contaria a quem não pode ver que o registro existe — que é vazamento de
/// informação por status HTTP (documento 05).
/// </summary>
public sealed class ObterCliente(IRepositorioClientes repositorio, IRelogio relogio)
{
    /// <summary>Executa a consulta.</summary>
    /// <param name="chave">O GUID público do cliente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<ClienteDetalhe>>> ExecutarAsync(Guid chave, CancellationToken ct)
    {
        var cliente = await repositorio.ObterAsync(chave, incluirInativos: true, ct);

        if (cliente is null)
            return Resultado<ComProcedencia<ClienteDetalhe>>.NaoEncontrado(
                $"Não há cliente {chave} ao seu alcance.");

        return Resultado<ComProcedencia<ClienteDetalhe>>.Ok(
            ComProcedencia<ClienteDetalhe>.DoNossoBanco(
                ClienteDetalhe.De(cliente), "comercial.Cliente", relogio));
    }
}
