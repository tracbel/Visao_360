using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// AS ROTAS DE LEITURA DO RELACIONAMENTO, exercitadas por HTTP com a aplicação inteira de pé —
/// processo, tarefa, interação, cobertura e os quatro agregados de relatório.
///
/// <para>Dois grupos de teste, e os dois importam pelo mesmo motivo. O primeiro prova que a
/// <b>fronteira de multiempresa vale nas rotas novas</b>: a mesma requisição responde 200 numa
/// filial e 404 na outra, e a listagem da outra filial não traz o registro nem no total. O
/// segundo prova que <b>o agregado diz quando não tem dado</b>, em vez de mostrar um número
/// construído sobre nada — que é o defeito do legado que este projeto existe para corrigir.</para>
/// </summary>
[Trait("Categoria", "Relacionamento")]
public sealed class EndpointsDeRelacionamentoTestes(ApiEmMemoria api, ITestOutputHelper saida)
    : IClassFixture<ApiEmMemoria>
{
    private static async Task<JsonElement> CorpoAsync(HttpResponseMessage resposta) =>
        JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.Clone();

    /// <summary>
    /// Semeia um relacionamento completo nas DUAS filiais: cliente, carteira, vínculo, processo,
    /// tarefa e interação — o mínimo para as sete rotas terem o que devolver e para a fronteira
    /// ter os dois lados.
    /// </summary>
    private async Task<(Guid ProcessoDeRibeirao, Guid ClienteDeRibeirao)> SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.Processos.AnyAsync())
        {
            var jaExiste = await db.Processos.OrderBy(p => p.Id).FirstAsync();
            var donoExistente = await db.Clientes.FirstAsync(c => c.Id == jaExiste.ClienteId);
            return (jaExiste.ChavePublica, donoExistente.ChavePublica);
        }

        var linha = LinhaDeNegocio.Criar("MAQ", "Máquinas");
        db.LinhasDeNegocio.Add(linha);

        var tipo = TipoProcesso.Criar("VENDA_EQUIPAMENTO", "Venda de Equipamento");
        db.TiposDeProcesso.Add(tipo);

        var acao = TipoTarefa.Criar("VISITAR", "Visitar cliente", CategoriaDeInteracao.Interna, prazoDiasUteis: 0);
        db.TiposDeTarefa.Add(acao);
        await db.SaveChangesAsync();

        var fase = Fase.Criar(tipo.Id, "APRESENTACAO", "Apresentação", ordem: 5);
        db.Fases.Add(fase);

        var desfecho = Dominio.Processo.Resultado.Criar(
            acao.Id, "RES_1", "Contato em andamento", ClasseDeResultado.Manutencao);

        db.Resultados.Add(desfecho);
        await db.SaveChangesAsync();

        Guid processoDeRibeirao = default;
        Guid clienteDeRibeirao = default;

        foreach (var (empresaId, usuarioId, sufixo) in new[] { (1, 100L, "Ribeirão"), (2, 200L, "Barretos") })
        {
            var cliente = Cliente.Criar(
                empresaId, $"Fazenda do Relacionamento {sufixo} Ltda", TipoDePessoa.Juridica,
                usuarioId, usuarioId);

            db.Clientes.Add(cliente);

            var carteira = Carteira.Criar(
                empresaId, linha.Id, $"CART_{sufixo.ToUpperInvariant()}", $"Carteira {sufixo}",
                usuarioId, usuarioId);

            db.Carteiras.Add(carteira);
            await db.SaveChangesAsync();

            db.ClienteCarteiras.Add(ClienteCarteira.Criar(
                cliente.Id, carteira.Id, ClasseDeCliente.A, usuarioId, diasCicloContato: 30));

            var processo = Dominio.Processo.Processo.Abrir(
                numero: empresaId * 1000L + 1,
                empresaId: empresaId,
                tipoProcessoId: tipo.Id,
                clienteId: cliente.Id,
                faseId: fase.Id,
                titulo: $"Oportunidade de {sufixo}",
                proprietarioId: usuarioId,
                criadoPorId: usuarioId,
                abertoEmUtc: new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));

            db.Processos.Add(processo);
            await db.SaveChangesAsync();

            var tarefa = Tarefa.Agendar(
                empresaId, acao.Id, $"Visitar {sufixo}", usuarioId,
                new DateTime(2026, 3, 2, 12, 0, 0, DateTimeKind.Utc),
                OrigemDaAtribuicao.Manual, usuarioId,
                processoId: processo.Id, clienteId: cliente.Id);

            db.Tarefas.Add(tarefa);

            db.Interacoes.Add(Interacao.Registrar(
                empresaId, acao.Id, $"Visita em {sufixo}",
                DataHoraUtc.Criar(new DateTime(2026, 3, 3, 12, 0, 0, DateTimeKind.Utc)),
                NaturezaDaInteracao.Ativa, usuarioId,
                clienteId: cliente.Id, processoId: processo.Id, resultadoId: desfecho.Id));

            await db.SaveChangesAsync();

            if (empresaId != 1) continue;

            processoDeRibeirao = processo.ChavePublica;
            clienteDeRibeirao = cliente.ChavePublica;
        }

        return (processoDeRibeirao, clienteDeRibeirao);
    }

    // =============================================================================================
    // A fronteira de multiempresa nas rotas novas
    // =============================================================================================

    [Fact]
    public async Task A_MESMA_requisicao_de_processo_devolve_200_numa_filial_e_404_na_outra()
    {
        var (processo, _) = await SemearAsync();
        var rota = $"/api/v1/processos/{processo}";

        var emRibeirao = await api.ClienteDeRibeirao().GetAsync(rota);
        var emBarretos = await api.ClienteDeBarretos().GetAsync(rota);

        saida.WriteLine($"GET {rota}");
        saida.WriteLine($"  X-Tracbel-Empresa: {ApiEmMemoria.FilialDeRibeirao} → {(int)emRibeirao.StatusCode}");
        saida.WriteLine($"  X-Tracbel-Empresa: {ApiEmMemoria.FilialDeBarretos} → {(int)emBarretos.StatusCode}");

        emRibeirao.StatusCode.Should().Be(HttpStatusCode.OK);
        emBarretos.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_listagem_de_processo_da_outra_filial_nao_traz_o_registro_nem_no_total()
    {
        await SemearAsync();

        var resposta = await api.ClienteDeBarretos().GetAsync("/api/v1/processos?termo=Ribeirão");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);

        var dados = (await CorpoAsync(resposta)).GetProperty("dados");
        dados.GetProperty("itens").GetArrayLength().Should().Be(0);
        dados.GetProperty("total").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task A_agenda_a_linha_do_tempo_e_a_cobertura_so_enxergam_a_propria_filial()
    {
        await SemearAsync();

        foreach (var rota in new[] { "/api/v1/tarefas", "/api/v1/interacoes", "/api/v1/cobertura" })
        {
            var deRibeirao = (await CorpoAsync(await api.ClienteDeRibeirao().GetAsync(rota)))
                .GetProperty("dados").GetProperty("total").GetInt32();

            var deBarretos = (await CorpoAsync(await api.ClienteDeBarretos().GetAsync(rota)))
                .GetProperty("dados").GetProperty("total").GetInt32();

            saida.WriteLine($"GET {rota} → Ribeirão: {deRibeirao} · Barretos: {deBarretos}");

            // CADA FILIAL VÊ EXATAMENTE UMA LINHA, a sua. Se a fronteira não valesse, as duas
            // veriam duas — e o teste passaria por igualdade, não por isolamento.
            deRibeirao.Should().Be(1);
            deBarretos.Should().Be(1);
        }
    }

    [Fact]
    public async Task A_linha_do_tempo_de_um_cliente_de_outra_filial_devolve_nao_encontrado()
    {
        var (_, cliente) = await SemearAsync();

        var resposta = await api.ClienteDeBarretos()
            .GetAsync($"/api/v1/interacoes?clienteChave={cliente}");

        // 404 E NÃO LISTA VAZIA: devolver 200 com zero itens contaria que a chave é válida em
        // algum lugar. A resposta é a mesma de "esse cliente não existe".
        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task O_agregado_tambem_respeita_a_fronteira_e_nao_soma_a_filial_do_vizinho()
    {
        await SemearAsync();

        foreach (var rota in new[] { "/api/v1/relatorios/funil", "/api/v1/relatorios/cobertura" })
        {
            var itens = (await CorpoAsync(await api.ClienteDeRibeirao().GetAsync(rota)))
                .GetProperty("dados").GetProperty("itens");

            saida.WriteLine($"GET {rota} → {itens.GetArrayLength()} linha(s) para Ribeirão Preto");

            // O AGREGADO É ONDE O LEGADO MAIS VAZA: 125 dos 134 relatórios dele não têm predicado
            // de usuário nenhum. Aqui o GROUP BY roda dentro do filtro global, e uma filial só
            // produz uma linha por fase e uma linha por carteira.
            itens.GetArrayLength().Should().Be(1);
        }
    }

    // =============================================================================================
    // O agregado que diz quando não tem dado
    // =============================================================================================

    [Fact]
    public async Task O_funil_declara_que_o_valor_nao_tem_dado_quando_nenhum_processo_tem_valor()
    {
        await SemearAsync();

        var dados = (await CorpoAsync(await api.ClienteDeRibeirao().GetAsync("/api/v1/relatorios/funil")))
            .GetProperty("dados");

        var fase = dados.GetProperty("itens")[0];
        fase.GetProperty("processos").GetInt32().Should().Be(1);
        fase.GetProperty("processosComValor").GetInt32().Should().Be(0);

        // O TOTAL VEM NULO, e não zero: zero diria "o funil vale nada", e o que se sabe é outra
        // coisa — que ninguém preencheu o valor.
        fase.GetProperty("valorTotal").ValueKind.Should().Be(JsonValueKind.Null);

        var ausentes = dados.GetProperty("metricasSemDado");
        ausentes.GetArrayLength().Should().BeGreaterThan(0);
        ausentes[0].GetProperty("metrica").GetString().Should().Be("valorDoFunil");

        saida.WriteLine(ausentes[0].GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task O_painel_da_agenda_declara_que_nao_ha_prazo_contra_o_qual_medir_cumprimento()
    {
        await SemearAsync();

        var dados = (await CorpoAsync(await api.ClienteDeRibeirao().GetAsync("/api/v1/relatorios/agenda")))
            .GetProperty("dados");

        var painel = dados.GetProperty("itens")[0];
        painel.GetProperty("pendentes").GetInt32().Should().Be(1);
        painel.GetProperty("semPrazoLimite").GetInt32().Should().Be(1);

        var motivos = dados.GetProperty("metricasSemDado")
            .EnumerateArray()
            .Select(m => m.GetProperty("metrica").GetString())
            .ToList();

        motivos.Should().Contain("cumprimentoDePrazo");
        saida.WriteLine(string.Join(" · ", motivos));
    }

    [Fact]
    public async Task A_tarefa_traz_o_atraso_calculado_e_a_cobertura_traz_os_dias_sem_contato()
    {
        await SemearAsync();

        var tarefa = (await CorpoAsync(await api.ClienteDeRibeirao().GetAsync("/api/v1/tarefas")))
            .GetProperty("dados").GetProperty("itens")[0];

        tarefa.GetProperty("estaAtrasada").GetBoolean().Should().BeTrue(
            "a tarefa foi agendada para março de 2026 e continua pendente");

        tarefa.GetProperty("diasDeAtraso").GetInt32().Should().BeGreaterThan(0);

        // O PRAZO LIMITE VEM NULO, e é o dado real: a origem não declara prazo em nenhuma das
        // ações em uso. Por isso o atraso é medido contra a data agendada.
        tarefa.GetProperty("prazoLimite").ValueKind.Should().Be(JsonValueKind.Null);

        var cobertura = (await CorpoAsync(await api.ClienteDeRibeirao().GetAsync("/api/v1/cobertura")))
            .GetProperty("dados").GetProperty("itens")[0];

        cobertura.GetProperty("diasCicloContato").GetInt16().Should().Be(30);

        // FORA DO CICLO É VERDADE PORQUE HÁ CICLO DECLARADO. Sem ciclo, o campo seria nulo — e
        // nulo é "não se sabe", não "está em dia".
        cobertura.GetProperty("estaForaDoCiclo").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Toda_leitura_do_relacionamento_carrega_procedencia()
    {
        await SemearAsync();

        foreach (var rota in new[]
                 {
                     "/api/v1/processos", "/api/v1/tarefas", "/api/v1/interacoes", "/api/v1/cobertura",
                     "/api/v1/relatorios/funil", "/api/v1/relatorios/perdas",
                     "/api/v1/relatorios/agenda", "/api/v1/relatorios/cobertura"
                 })
        {
            var corpo = await CorpoAsync(await api.ClienteDeRibeirao().GetAsync(rota));

            var procedencia = corpo.GetProperty("procedencia");
            procedencia.GetProperty("sistema").GetString().Should().Be("CRM Tracbel");
            procedencia.GetProperty("objeto").GetString().Should().NotBeNullOrWhiteSpace();

            saida.WriteLine($"{rota} → {procedencia.GetProperty("objeto").GetString()}");
        }
    }

    [Fact]
    public async Task Parametro_fora_do_dominio_fechado_e_recusado_com_as_opcoes_na_mensagem()
    {
        await SemearAsync();

        var resposta = await api.ClienteDeRibeirao().GetAsync("/api/v1/processos?situacao=QUASE_LA");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var erro = (await CorpoAsync(resposta)).GetProperty("erros")[0];
        erro.GetProperty("campo").GetString().Should().Be("situacao");
        erro.GetProperty("mensagem").GetString().Should().Contain("Aberto");
    }
}
