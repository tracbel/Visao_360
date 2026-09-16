using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Equipamentos;

/// <summary>
/// Lista as máquinas do NOSSO banco.
///
/// Mesma disciplina da listagem de clientes: não existe parâmetro de empresa nesta consulta,
/// porque a fronteira de multiempresa é do filtro global do <c>CrmDbContext</c> e não é
/// negociável por quem chama.
/// </summary>
public sealed class ListarEquipamentos(
    IRepositorioEquipamentos repositorio,
    IRepositorioClientes clientes,
    IRepositorioCatalogos catalogos,
    IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="pagina">Página pedida, começando em 1.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="termo">Busca por chassi, número de série ou placa.</param>
    /// <param name="situacao">Filtro por situação. Domínio fechado.</param>
    /// <param name="origem">Filtro por Protheus, Crm ou Art. Domínio fechado.</param>
    /// <param name="clienteChave">Filtro pelo dono — é o que a Visão 360 usa.</param>
    /// <param name="ordenarPor">Coluna de ordenação. Domínio fechado.</param>
    /// <param name="descendente">Ordem decrescente.</param>
    /// <param name="incluirInativos">Trazer também as máquinas baixadas.</param>
    /// <param name="linhaDeProduto">Código da classificação, ou SEM_CLASSIFICACAO.</param>
    /// <param name="porte">Pequeno, Medio, Grande ou NaoSeAplica.</param>
    /// <param name="somenteComVenda">Só as máquinas com venda registrada.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<EquipamentoResumo>>>> ExecutarAsync(
        int? pagina,
        int? tamanho,
        string? termo,
        string? situacao,
        string? origem,
        Guid? clienteChave,
        string? ordenarPor,
        bool descendente,
        bool incluirInativos,
        string? linhaDeProduto,
        string? porte,
        bool somenteComVenda,
        CancellationToken ct)
    {
        var erros = new ColetorDeErros();

        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<EquipamentoResumo>>>.FalhaDeValidacao(
                paginacao.Erro!, paginacao.Erros);

        var situacaoEscolhida = situacao is null
            ? null
            : erros.ItemDeDominio<SituacaoDoEquipamento>("situacao", situacao);

        var origemEscolhida = origem is null
            ? null
            : erros.ItemDeDominio<OrigemDoEquipamento>("origem", origem);

        var porteEscolhido = porte is null
            ? null
            : erros.ItemDeDominio<PorteDeMaquina>("porte", porte);

        var ordem = erros.ItemDeDominioOuPadrao("ordenarPor", ordenarPor, OrdemDeEquipamento.Chassi);

        // A CLASSIFICAÇÃO É CATÁLOGO: código fora dele é recusado, e não devolve lista vazia — lista
        // vazia diria "não há máquina" quando o que houve foi um código errado.
        string? classificacao = null;
        if (!string.IsNullOrWhiteSpace(linhaDeProduto))
        {
            classificacao = linhaDeProduto.Trim().ToUpperInvariant();
            if (classificacao != ConsultaDeEquipamentos.SemClassificacao
                && await catalogos.ObterLinhaDeProdutoAsync(classificacao, ct) is null)
                erros.Registrar(
                    "linhaDeProduto",
                    "Classificação fora do catálogo. Consulte /api/v1/catalogos/LINHA_DE_PRODUTO.",
                    linhaDeProduto);
        }

        // O filtro por dono chega como CHAVE PÚBLICA e é traduzido aqui — de propósito. Traduzir
        // pelo repositório significa que um cliente fora do alcance de quem consulta não resolve,
        // e a listagem devolve vazio em vez de vazar a existência da máquina dele.
        long? clienteId = null;
        if (clienteChave is { } chave)
        {
            var dono = await clientes.ObterAsync(chave, incluirInativos: true, ct);
            if (dono is null)
                erros.Registrar("clienteChave", "Não há cliente com esta chave ao seu alcance.", chave.ToString());
            else
                clienteId = dono.Cliente.Id;
        }

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<EquipamentoResumo>>>(
                "A consulta tem parâmetros que não valem.");

        var pagi = await repositorio.ListarAsync(
            new ConsultaDeEquipamentos(
                paginacao.Valor,
                Termo: string.IsNullOrWhiteSpace(termo) ? null : termo.Trim(),
                Situacao: situacaoEscolhida,
                Origem: origemEscolhida,
                ClienteId: clienteId,
                Ordem: ordem,
                Descendente: descendente,
                IncluirInativos: incluirInativos,
                LinhaDeProdutoCodigo: classificacao,
                Porte: porteEscolhido,
                SomenteComVenda: somenteComVenda),
            ct);

        var resumo = new PaginaDe<EquipamentoResumo>(
            [.. pagi.Itens.Select(EquipamentoResumo.De)], pagi.Pagina, pagi.Tamanho, pagi.Total);

        return Resultado<ComProcedencia<PaginaDe<EquipamentoResumo>>>.Ok(
            ComProcedencia<PaginaDe<EquipamentoResumo>>.DoNossoBanco(resumo, "frota.Equipamento", relogio));
    }
}

/// <summary>Traz a ficha de uma máquina pela chave pública.</summary>
public sealed class ObterEquipamento(IRepositorioEquipamentos repositorio, IRelogio relogio)
{
    /// <summary>Executa a consulta.</summary>
    /// <param name="chave">O GUID público da máquina.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<EquipamentoDetalhe>>> ExecutarAsync(
        Guid chave, CancellationToken ct)
    {
        var leitura = await repositorio.ObterAsync(chave, incluirInativos: true, ct);

        if (leitura is null)
            return Resultado<ComProcedencia<EquipamentoDetalhe>>.NaoEncontrado(
                $"Não há equipamento {chave} ao seu alcance.");

        return Resultado<ComProcedencia<EquipamentoDetalhe>>.Ok(
            ComProcedencia<EquipamentoDetalhe>.DoNossoBanco(
                EquipamentoDetalhe.De(leitura), "frota.Equipamento", relogio));
    }
}

/// <summary>
/// AS MÁQUINAS QUE O CLIENTE COMPROU — pelo vínculo "comprador na venda", com a data da venda. Uma
/// compra não faz do cliente o dono atual: o campo <c>EhDonoAtual</c> diz quando ele também é.
/// </summary>
public sealed class ListarMaquinasCompradasPeloCliente(IRepositorioHistoricoComercial repositorio, IRelogio relogio)
{
    /// <summary>Executa a consulta.</summary>
    /// <param name="chave">O GUID público do cliente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<MaquinaCompradaPeloCliente>>>> ExecutarAsync(
        Guid chave, CancellationToken ct)
    {
        var maquinas = await repositorio.ListarMaquinasCompradasAsync(chave, ct);

        return maquinas is null
            ? Resultado<ComProcedencia<IReadOnlyList<MaquinaCompradaPeloCliente>>>.NaoEncontrado(
                $"Não há cliente {chave} ao seu alcance.")
            : Resultado<ComProcedencia<IReadOnlyList<MaquinaCompradaPeloCliente>>>.Ok(
                ComProcedencia<IReadOnlyList<MaquinaCompradaPeloCliente>>.DoNossoBanco(
                    maquinas, "frota.VinculoDeClienteComEquipamento", relogio));
    }
}

/// <summary>
/// O HISTÓRICO COMERCIAL de uma máquina: cada venda, o comprador NELA, a natureza do vínculo, a
/// filial, as datas e a trilha da origem (documento 35, seção 10).
///
/// <para>O comprador de uma venda não é apresentado como dono: a ficha mostra o dono em "Cliente
/// proprietário", e o comprador aqui, com a data da venda.</para>
/// </summary>
public sealed class ListarVendasDoEquipamento(IRepositorioHistoricoComercial repositorio, IRelogio relogio)
{
    /// <summary>Executa a consulta.</summary>
    /// <param name="chave">O GUID público da máquina.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<IReadOnlyList<VendaDaMaquinaComContexto>>>> ExecutarAsync(
        Guid chave, CancellationToken ct)
    {
        var vendas = await repositorio.ListarVendasAsync(chave, ct);

        return vendas is null
            ? Resultado<ComProcedencia<IReadOnlyList<VendaDaMaquinaComContexto>>>.NaoEncontrado(
                $"Não há equipamento {chave} ao seu alcance.")
            : Resultado<ComProcedencia<IReadOnlyList<VendaDaMaquinaComContexto>>>.Ok(
                ComProcedencia<IReadOnlyList<VendaDaMaquinaComContexto>>.DoNossoBanco(vendas, "frota.VendaDeMaquina", relogio));
    }
}
