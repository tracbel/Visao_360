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
    IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="pagina">Página pedida, começando em 1.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="termo">Busca por chassi, número de série ou placa.</param>
    /// <param name="situacao">Filtro por situação. Domínio fechado.</param>
    /// <param name="origem">Filtro por Protheus ou Crm. Domínio fechado.</param>
    /// <param name="clienteChave">Filtro pelo dono — é o que a Visão 360 usa.</param>
    /// <param name="ordenarPor">Coluna de ordenação. Domínio fechado.</param>
    /// <param name="descendente">Ordem decrescente.</param>
    /// <param name="incluirInativos">Trazer também as máquinas baixadas.</param>
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

        var ordem = erros.ItemDeDominioOuPadrao("ordenarPor", ordenarPor, OrdemDeEquipamento.Chassi);

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
                IncluirInativos: incluirInativos),
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
