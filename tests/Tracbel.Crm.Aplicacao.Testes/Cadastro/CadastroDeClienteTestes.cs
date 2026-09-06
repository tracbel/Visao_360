using FluentAssertions;
using Tracbel.Crm.Aplicacao.Clientes;
using Tracbel.Crm.Dominio.Comum;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Aplicacao.Testes.Cadastro;

/// <summary>
/// Os casos de uso do cadastro de cliente, contra banco de verdade.
///
/// Cada caminho feliz vem acompanhado do seu caminho de recusa, porque é a recusa que carrega a
/// promessa deste projeto: a mensagem diz O QUE CORRIGIR, campo a campo. [V] "An error has
/// occurred" é o que a API do legado devolve para tudo, e é por isso que ninguém consegue
/// diagnosticar nada lá.
/// </summary>
[Trait("Categoria", "CasoDeUso")]
public sealed class CadastroDeClienteTestes(ITestOutputHelper saida) : IDisposable
{
    private readonly BancoDeTeste _banco = new();

    /// <summary>Não há cancelamento a exercitar nestes testes; o token existe só para propagar.</summary>
    private static CancellationToken Ct => CancellationToken.None;

    public void Dispose() => _banco.Dispose();

    private static NovoCliente ClienteValido(string nome = "Fazenda Santa Luzia Ltda", string? documento = null) =>
        new(NomeRazao: nome,
            TipoDePessoa: "Juridica",
            Documento: documento,
            Situacao: "Prospect",
            OrigemCodigo: "INDICACAO");

    // ------------------------------------------------------------------ criar

    [Fact]
    public async Task Criar_grava_o_cliente_na_filial_do_contexto_de_acesso()
    {
        var casos = _banco.CasosDeCliente();

        var resultado = await casos.Criar.ExecutarAsync(
            ClienteValido(documento: "11.222.333/0001-81"), Ct);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro);

        // A FILIAL NÃO VEIO DO CORPO DA REQUISIÇÃO — veio do contexto. Aceitar EmpresaId no JSON
        // abriria um caminho para gravar na filial de outro, e a fronteira do documento 05
        // valeria só para a leitura.
        resultado.Valor.EmpresaId.Should().Be(BancoDeTeste.RibeiraoPreto);
        resultado.Valor.ProprietarioId.Should().Be(BancoDeTeste.UsuarioDeRibeirao);
        resultado.Valor.Documento.Should().Be("11.222.333/0001-81");
        resultado.Valor.OrigemCodigo.Should().Be("INDICACAO");

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Criar_recusa_dizendo_o_que_corrigir_em_CADA_campo_errado()
    {
        var casos = _banco.CasosDeCliente();

        var resultado = await casos.Criar.ExecutarAsync(
            new NovoCliente(
                NomeRazao: "   ",
                TipoDePessoa: "Juridica",
                Documento: "529.982.247-25",       // CPF válido, mas o tipo diz jurídica
                OrigemCodigo: "VEIO_DO_NADA"),     // não existe no catálogo
            Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoDeFalha.Validacao);

        foreach (var erro in resultado.Erros)
            saida.WriteLine($"{erro.Campo}: {erro.Mensagem} (recebido: {erro.ValorRecebido})");

        // TRÊS ERROS DE UMA VEZ, e não o primeiro. Recusar no primeiro campo faz o usuário
        // corrigir, reenviar, descobrir o segundo, corrigir, reenviar — o mesmo trabalho
        // repartido em três viagens.
        resultado.Erros.Select(e => e.Campo)
            .Should().BeEquivalentTo(["nomeRazao", "documento", "origemCodigo"]);

        resultado.Erros.Should().AllSatisfy(e =>
            e.Mensagem.Should().NotBeNullOrWhiteSpace("a mensagem precisa dizer o que fazer"));

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Criar_recusa_documento_repetido_dizendo_QUEM_ja_o_tem()
    {
        var casos = _banco.CasosDeCliente();

        await casos.Criar.ExecutarAsync(
            ClienteValido("Fazenda Primeira Ltda", "11.222.333/0001-81"),
            Ct);

        var segundo = await casos.Criar.ExecutarAsync(
            ClienteValido("Fazenda Segunda Ltda", "11222333000181"),
            Ct);

        segundo.EhSucesso.Should().BeFalse();
        segundo.Tipo.Should().Be(TipoDeFalha.Conflito);

        // [V] O legado mede 116 CPFs repetidos entre clientes distintos e 124 mil pares na fila
        // de deduplicação. A recusa aqui não é só "já existe": ela diz o nome de quem já tem.
        segundo.Erros.Should().ContainSingle()
            .Which.Mensagem.Should().Contain("Fazenda Primeira Ltda");

        saida.WriteLine(segundo.Erros[0].Mensagem);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Criar_recusa_codigo_de_catalogo_que_nao_existe_mesmo_sem_passar_pela_tela()
    {
        // Documento 16, seção 3.1, ponto 2: a API recusa o código fora do catálogo MESMO que a
        // tela tenha sido contornada — chamada direta, integração, script.
        var casos = _banco.CasosDeCliente();

        var resultado = await casos.Criar.ExecutarAsync(
            ClienteValido() with { OrigemCodigo = "SOJA" },   // existe, mas no catálogo CULTURA
            Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erros.Should().ContainSingle().Which.Campo.Should().Be("origemCodigo");

        // O ITEM EXISTE — só que noutro catálogo. É exatamente o defeito I-1 do documento 21,
        // em que Cliente.OrigemId aceitava um item de CULTURA, agora recusado na aplicação além
        // da chave estrangeira composta no banco.
        saida.WriteLine(resultado.Erros[0].Mensagem);

        casos.Contexto.Dispose();
    }

    // ------------------------------------------------------------------ alterar e inativar

    [Fact]
    public async Task Alterar_muda_o_cadastro_e_a_situacao()
    {
        var casos = _banco.CasosDeCliente();

        var criado = await casos.Criar.ExecutarAsync(ClienteValido(), Ct);

        var alterado = await casos.Alterar.ExecutarAsync(
            criado.Valor.Chave,
            new AlteracaoDeCliente(
                NomeRazao: "Fazenda Santa Luzia Agropecuária Ltda",
                TipoDePessoa: "Juridica",
                Situacao: "Cliente",
                OrigemCodigo: "SITE"),
            Ct);

        alterado.EhSucesso.Should().BeTrue(alterado.Erro);
        alterado.Valor.NomeRazao.Should().Be("Fazenda Santa Luzia Agropecuária Ltda");
        alterado.Valor.Situacao.Should().Be("Cliente");
        alterado.Valor.OrigemCodigo.Should().Be("SITE");
        alterado.Valor.AlteradoEm.Should().NotBeNull();

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Alterar_registro_que_nao_existe_devolve_nao_encontrado_e_nao_excecao()
    {
        var casos = _banco.CasosDeCliente();

        var resultado = await casos.Alterar.ExecutarAsync(
            Guid.NewGuid(),
            new AlteracaoDeCliente(NomeRazao: "Qualquer", TipoDePessoa: "Juridica"),
            Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoDeFalha.NaoEncontrado);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Inativar_e_exclusao_LOGICA_com_motivo_de_catalogo_obrigatorio()
    {
        var casos = _banco.CasosDeCliente();
        var criado = await casos.Criar.ExecutarAsync(ClienteValido(), Ct);

        // Sem motivo, recusa.
        var semMotivo = await casos.Inativar.ExecutarAsync(
            criado.Valor.Chave, new InativacaoDeCliente(), Ct);

        semMotivo.EhSucesso.Should().BeFalse();
        semMotivo.Erros.Should().ContainSingle().Which.Campo.Should().Be("motivoCodigo");

        // Com motivo de catálogo, inativa.
        var inativado = await casos.Inativar.ExecutarAsync(
            criado.Valor.Chave,
            new InativacaoDeCliente(MotivoCodigo: "DUPLICADO"),
            Ct);

        inativado.EhSucesso.Should().BeTrue(inativado.Erro);
        inativado.Valor.EstaInativo.Should().BeTrue();
        inativado.Valor.Situacao.Should().Be("ClienteInativo");
        inativado.Valor.MotivoInativacaoCodigo.Should().Be("DUPLICADO");

        casos.Contexto.Dispose();

        // NADA FOI APAGADO: a linha continua no banco, e é isso que "exclusão lógica" quer dizer.
        var conferencia = _banco.CasosDeCliente();
        var listaPadrao = await conferencia.Listar.ExecutarAsync(
            null, null, null, null, null, null, false, false, Ct);
        listaPadrao.Valor.Dados.Total.Should().Be(0, "a listagem padrão não traz inativado");

        var listaCompleta = await conferencia.Listar.ExecutarAsync(
            null, null, null, null, null, null, false, true, Ct);
        listaCompleta.Valor.Dados.Total.Should().Be(1, "o registro continua existindo e auditável");
        listaCompleta.Valor.Dados.Itens[0].EstaInativo.Should().BeTrue();

        conferencia.Contexto.Dispose();
    }

    // ------------------------------------------------------------------ listagem

    [Fact]
    public async Task Listar_recusa_pagina_maior_que_o_teto_em_vez_de_aceitar_em_silencio()
    {
        var casos = _banco.CasosDeCliente();

        var resultado = await casos.Listar.ExecutarAsync(
            1, 100_000, null, null, null, null, false, false, Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erros.Should().ContainSingle().Which.Campo.Should().Be("tamanho");

        // Sem o teto, um ?tamanho=100000 feito por engano transforma a listagem numa exportação
        // da base inteira — que é o que os 125 relatórios sem predicado do legado permitem hoje.
        saida.WriteLine(resultado.Erros[0].Mensagem);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Listar_recusa_ordenacao_fora_do_dominio_e_diz_quais_valem()
    {
        var casos = _banco.CasosDeCliente();

        var resultado = await casos.Listar.ExecutarAsync(
            null, null, null, null, null, "; DROP TABLE Cliente --", false, false,
            Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erros.Should().ContainSingle().Which.Campo.Should().Be("ordenarPor");
        resultado.Erros[0].Mensagem.Should().Contain("Nome");

        saida.WriteLine(resultado.Erros[0].Mensagem);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Listar_pagina_ordena_e_marca_a_procedencia()
    {
        var casos = _banco.CasosDeCliente();

        foreach (var nome in new[] { "Fazenda Carlos", "Fazenda Ana", "Fazenda Bruno" })
            await casos.Criar.ExecutarAsync(ClienteValido(nome), Ct);

        var pagina = await casos.Listar.ExecutarAsync(
            1, 2, null, null, null, "Nome", false, false, Ct);

        pagina.EhSucesso.Should().BeTrue(pagina.Erro);
        pagina.Valor.Dados.Total.Should().Be(3);
        pagina.Valor.Dados.Itens.Should().HaveCount(2);
        pagina.Valor.Dados.TemProxima.Should().BeTrue();
        pagina.Valor.Dados.Itens.Select(i => i.NomeRazao)
            .Should().ContainInOrder("Fazenda Ana", "Fazenda Bruno");

        // O CARIMBO DE ORIGEM VALE TAMBÉM PARA O NOSSO BANCO. A tela mistura, na mesma página, o
        // cadastro nosso e a leitura da ponte do legado; se só o legado viesse carimbado, o front
        // teria de deduzir a origem do resto pela ausência de carimbo.
        pagina.Valor.Procedencia.Sistema.Should().Be(Procedencias.SistemaProprio);
        pagina.Valor.Procedencia.Objeto.Should().Be("comercial.Cliente");
        pagina.Valor.Procedencia.LidoEmUtc.Should().Be(_banco.Relogio.Agora);
        pagina.Valor.Procedencia.EstaDesatualizado.Should().BeFalse();

        saida.WriteLine($"procedência: {pagina.Valor.Procedencia}");

        casos.Contexto.Dispose();
    }
}
