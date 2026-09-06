using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso ao cadastro de máquinas. Mesma observação do repositório de clientes: a fronteira de
/// filial não aparece aqui porque ela é do filtro global, e por isso não há como esquecê-la.
/// </summary>
public sealed class RepositorioDeEquipamentos(CrmDbContext contexto) : IRepositorioEquipamentos
{
    /// <inheritdoc />
    public async Task<PaginaDe<EquipamentoComContexto>> ListarAsync(
        ConsultaDeEquipamentos consulta, CancellationToken ct)
    {
        var linhas = Filtrar(contexto.Equipamentos.AsQueryable(), consulta);

        var total = await linhas.CountAsync(ct);

        var itens = await Ordenar(linhas, consulta)
            .Skip(consulta.Paginacao.Saltar)
            .Take(consulta.Paginacao.Tamanho)
            .Select(ComDonoEModelo())
            .ToListAsync(ct);

        return new PaginaDe<EquipamentoComContexto>(
            itens, consulta.Paginacao.Pagina, consulta.Paginacao.Tamanho, total);
    }

    /// <inheritdoc />
    public Task<EquipamentoComContexto?> ObterAsync(
        Guid chavePublica, bool incluirInativos, CancellationToken ct)
    {
        var linhas = contexto.Equipamentos.Where(e => e.ChavePublica == chavePublica);
        if (!incluirInativos) linhas = linhas.Where(e => e.ExcluidoEm == null);

        return linhas.Select(ComDonoEModelo()).FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc />
    public async Task AdicionarAsync(Equipamento equipamento, CancellationToken ct) =>
        await contexto.Equipamentos.AddAsync(equipamento, ct);

    /// <inheritdoc />
    public Task<Equipamento?> ObterPorChassiAsync(Chassi chassi, Guid? exceto, CancellationToken ct)
    {
        var linhas = contexto.Equipamentos
            .Where(e => e.ExcluidoEm == null && e.Chassi == chassi);

        if (exceto is { } chave) linhas = linhas.Where(e => e.ChavePublica != chave);

        return linhas.FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Traz, junto da máquina, a chave pública e o nome do dono e o modelo resolvido com família
    /// e marca — tudo na mesma ida ao banco.
    ///
    /// O DONO PASSA PELO MESMO FILTRO GLOBAL: se a máquina é da minha filial mas o cliente não
    /// está ao meu alcance, o nome vem nulo em vez de vazar. É o filtro do <c>Cliente</c>
    /// atuando dentro da subconsulta, e não uma regra escrita duas vezes.
    /// </summary>
    private System.Linq.Expressions.Expression<Func<Equipamento, EquipamentoComContexto>> ComDonoEModelo() =>
        e => new EquipamentoComContexto(
            e,
            contexto.Clientes.Where(c => c.Id == e.ClienteId).Select(c => (Guid?)c.ChavePublica).FirstOrDefault(),
            contexto.Clientes.Where(c => c.Id == e.ClienteId).Select(c => c.NomeRazao).FirstOrDefault(),
            contexto.Modelos
                .Where(m => m.Id == e.ModeloId)
                .Select(m => new ModeloParaSelecao(
                    m.Id,
                    m.Codigo,
                    m.Nome,
                    contexto.Familias.Where(f => f.Id == m.FamiliaId).Select(f => f.Nome).FirstOrDefault()!,
                    contexto.Familias
                        .Where(f => f.Id == m.FamiliaId)
                        .SelectMany(f => contexto.Marcas.Where(ma => ma.Id == f.MarcaId).Select(ma => ma.Nome))
                        .FirstOrDefault()!,
                    contexto.Familias
                        .Where(f => f.Id == m.FamiliaId)
                        .SelectMany(f => contexto.Marcas.Where(ma => ma.Id == f.MarcaId)
                            .Select(ma => ma.EhRepresentada))
                        .FirstOrDefault()))
                .FirstOrDefault());

    private static IQueryable<Equipamento> Filtrar(
        IQueryable<Equipamento> linhas, ConsultaDeEquipamentos consulta)
    {
        if (!consulta.IncluirInativos) linhas = linhas.Where(e => e.ExcluidoEm == null);

        if (consulta.Situacao is { } situacao) linhas = linhas.Where(e => e.Situacao == situacao);
        if (consulta.Origem is { } origem) linhas = linhas.Where(e => e.Origem == origem);
        if (consulta.ClienteId is { } dono) linhas = linhas.Where(e => e.ClienteId == dono);
        if (consulta.ModeloId is { } modelo) linhas = linhas.Where(e => e.ModeloId == modelo);

        if (!string.IsNullOrWhiteSpace(consulta.Termo))
        {
            var termo = consulta.Termo.Trim();

            // O CHASSI É COMPARADO POR VALOR INTEIRO; número de série e placa, por trecho.
            //
            // POR QUE NÃO POR TRECHO TAMBÉM NO CHASSI: Chassi é tipo de valor com conversor, e o
            // EF aplica o conversor DA PROPRIEDADE ao parâmetro da comparação. Num LIKE isso
            // significa tentar converter "%987654%" — um pedaço de texto — para um Chassi de 17
            // caracteres, e a consulta estoura em tempo de execução com
            // "Invalid cast from 'System.String' to 'Chassi'". Vale tanto para .Contains quanto
            // para EF.Functions.Like: os dois foram exercitados e falham igual.
            //
            // A comparação por valor inteiro funciona porque aí o parâmetro JÁ é um Chassi, e o
            // conversor tem o que converter. Quem cola o chassi da plaqueta encontra a máquina.
            //
            // DÍVIDA NOMEADA (documento 23): busca por PEDAÇO de chassi — o CEN que digita só os
            // últimos dígitos — não funciona hoje. A correção é aditiva e não é para esta rodada:
            // uma coluna de busca em texto puro, alimentada do mesmo valor, com índice próprio.
            var achouChassi = Chassi.TentarCriar(termo, out var chassiProcurado);

            linhas = linhas.Where(e =>
                (achouChassi && e.Chassi == chassiProcurado)
                || (e.NumeroSerie != null && e.NumeroSerie.Contains(termo))
                || (e.Placa != null && e.Placa.Contains(termo)));
        }

        return linhas;
    }

    private static IQueryable<Equipamento> Ordenar(
        IQueryable<Equipamento> linhas, ConsultaDeEquipamentos consulta) =>
        (consulta.Ordem, consulta.Descendente) switch
        {
            (OrdemDeEquipamento.Chassi, false) => linhas.OrderBy(e => EF.Property<string>(e, nameof(Equipamento.Chassi))),
            (OrdemDeEquipamento.Chassi, true) => linhas.OrderByDescending(e => EF.Property<string>(e, nameof(Equipamento.Chassi))),

            (OrdemDeEquipamento.CriadoEm, false) => linhas.OrderBy(e => e.CriadoEm).ThenBy(e => e.Id),
            (OrdemDeEquipamento.CriadoEm, true) => linhas.OrderByDescending(e => e.CriadoEm).ThenBy(e => e.Id),

            (OrdemDeEquipamento.Situacao, false) => linhas.OrderBy(e => e.Situacao).ThenBy(e => EF.Property<string>(e, nameof(Equipamento.Chassi))),
            (OrdemDeEquipamento.Situacao, true) => linhas.OrderByDescending(e => e.Situacao).ThenBy(e => EF.Property<string>(e, nameof(Equipamento.Chassi))),

            (OrdemDeEquipamento.AnoModelo, false) => linhas.OrderBy(e => e.AnoModelo).ThenBy(e => EF.Property<string>(e, nameof(Equipamento.Chassi))),
            (OrdemDeEquipamento.AnoModelo, true) => linhas.OrderByDescending(e => e.AnoModelo).ThenBy(e => EF.Property<string>(e, nameof(Equipamento.Chassi))),

            _ => linhas.OrderBy(e => EF.Property<string>(e, nameof(Equipamento.Chassi)))
        };
}
