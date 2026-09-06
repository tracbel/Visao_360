using FluentAssertions;
using Tracbel.Crm.Aplicacao.Catalogos;
using Tracbel.Crm.Aplicacao.Clientes;
using Tracbel.Crm.Aplicacao.Equipamentos;
using Tracbel.Crm.Dominio.Comum;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Aplicacao.Testes.Cadastro;

/// <summary>
/// Os casos de uso do cadastro de máquina e do catálogo que os alimenta.
/// </summary>
[Trait("Categoria", "CasoDeUso")]
public sealed class CadastroDeEquipamentoTestes(ITestOutputHelper saida) : IDisposable
{
    private readonly BancoDeTeste _banco = new();

    private static CancellationToken Ct => CancellationToken.None;

    public void Dispose() => _banco.Dispose();

    private async Task<Guid> ClienteAsync()
    {
        var casos = _banco.CasosDeCliente();
        var criado = await casos.Criar.ExecutarAsync(
            new NovoCliente(NomeRazao: "Fazenda do Parque Ltda", TipoDePessoa: "Juridica"), Ct);
        casos.Contexto.Dispose();
        return criado.Valor.Chave;
    }

    [Fact]
    public async Task Criar_cadastra_a_maquina_com_o_modelo_resolvido_pelo_catalogo()
    {
        var cliente = await ClienteAsync();
        var casos = _banco.CasosDeEquipamento();

        var resultado = await casos.Criar.ExecutarAsync(
            new NovoEquipamento(
                Chassi: "1jd8r340abc123456",          // minúsculo de propósito: o tipo normaliza
                ModeloCodigo: BancoDeTeste.ModeloSemeado,
                ClienteChave: cliente.ToString(),
                Situacao: "Ativo",
                Origem: "Crm",
                AnoModelo: "2024"),
            Ct);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        resultado.Valor.Chassi.Should().Be("1JD8R340ABC123456", "o tipo de valor normaliza a caixa");
        resultado.Valor.ModeloCodigo.Should().Be(BancoDeTeste.ModeloSemeado);
        resultado.Valor.Marca.Should().Be("John Deere");
        resultado.Valor.ClienteChave.Should().Be(cliente);
        resultado.Valor.EmpresaId.Should().Be(BancoDeTeste.RibeiraoPreto);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Criar_recusa_chassi_que_nao_tem_forma_de_chassi()
    {
        var cliente = await ClienteAsync();
        var casos = _banco.CasosDeEquipamento();

        var resultado = await casos.Criar.ExecutarAsync(
            new NovoEquipamento(
                Chassi: "ABC123",                     // curto demais
                ModeloCodigo: BancoDeTeste.ModeloSemeado,
                ClienteChave: cliente.ToString()),
            Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erros.Should().Contain(e => e.Campo == "chassi");

        // [V] O cadastro vivo de equipamento do legado guarda chassi como texto livre, e por isso
        // a coluna não serve de chave natural para casar venda, garantia e ordem de serviço da
        // mesma máquina. Aqui não existe caminho para gravar um chassi sem forma.
        saida.WriteLine(resultado.Erros.First(e => e.Campo == "chassi").Mensagem);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Criar_recusa_modelo_fora_do_catalogo_de_frota()
    {
        var cliente = await ClienteAsync();
        var casos = _banco.CasosDeEquipamento();

        var resultado = await casos.Criar.ExecutarAsync(
            new NovoEquipamento(
                Chassi: "1JD8R340ABC123456",
                ModeloCodigo: "TRATOR_QUALQUER",
                ClienteChave: cliente.ToString()),
            Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erros.Should().ContainSingle().Which.Campo.Should().Be("modeloCodigo");

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Criar_recusa_maquina_em_operacao_sem_dono_e_maquina_em_estoque_com_dono()
    {
        // A regra é da ENTIDADE, e chega aqui como conflito — não como exceção nem como 500.
        var cliente = await ClienteAsync();
        var casos = _banco.CasosDeEquipamento();

        var semDono = await casos.Criar.ExecutarAsync(
            new NovoEquipamento(
                Chassi: "1JD8R340ABC111111",
                ModeloCodigo: BancoDeTeste.ModeloSemeado,
                Situacao: "Ativo"),
            Ct);

        semDono.EhSucesso.Should().BeFalse();
        semDono.Tipo.Should().Be(TipoDeFalha.Conflito);
        saida.WriteLine(semDono.Erro!);

        var estoqueComDono = await casos.Criar.ExecutarAsync(
            new NovoEquipamento(
                Chassi: "1JD8R340ABC222222",
                ModeloCodigo: BancoDeTeste.ModeloSemeado,
                ClienteChave: cliente.ToString(),
                Situacao: "Estoque"),
            Ct);

        estoqueComDono.EhSucesso.Should().BeFalse();
        estoqueComDono.Tipo.Should().Be(TipoDeFalha.Conflito);
        saida.WriteLine(estoqueComDono.Erro!);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Criar_recusa_chassi_repetido()
    {
        var cliente = await ClienteAsync();
        var casos = _banco.CasosDeEquipamento();

        var requisicao = new NovoEquipamento(
            Chassi: "1JD8R340ABC123456",
            ModeloCodigo: BancoDeTeste.ModeloSemeado,
            ClienteChave: cliente.ToString());

        (await casos.Criar.ExecutarAsync(requisicao, Ct)).EhSucesso.Should().BeTrue();

        var repetido = await casos.Criar.ExecutarAsync(requisicao, Ct);

        repetido.EhSucesso.Should().BeFalse();
        repetido.Tipo.Should().Be(TipoDeFalha.Conflito);
        repetido.Erros.Should().ContainSingle().Which.Campo.Should().Be("chassi");

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Inativar_e_baixa_LOGICA_e_a_maquina_some_da_listagem_padrao()
    {
        var cliente = await ClienteAsync();
        var casos = _banco.CasosDeEquipamento();

        var criado = await casos.Criar.ExecutarAsync(
            new NovoEquipamento(
                Chassi: "1JD8R340ABC123456",
                ModeloCodigo: BancoDeTeste.ModeloSemeado,
                ClienteChave: cliente.ToString()),
            Ct);

        var baixado = await casos.Inativar.ExecutarAsync(
            criado.Valor.Chave, new BaixaDeEquipamento(), Ct);

        baixado.EhSucesso.Should().BeTrue(baixado.Erro);
        baixado.Valor.Situacao.Should().Be("Baixado");
        baixado.Valor.EstaInativo.Should().BeTrue();

        casos.Contexto.Dispose();

        var conferencia = _banco.CasosDeEquipamento();

        var padrao = await conferencia.Listar.ExecutarAsync(
            null, null, null, null, null, null, null, false, false, Ct);
        padrao.Valor.Dados.Total.Should().Be(0);

        var completa = await conferencia.Listar.ExecutarAsync(
            null, null, null, null, null, null, null, false, true, Ct);
        completa.Valor.Dados.Total.Should().Be(1, "baixar não apaga: a linha e o histórico ficam");

        conferencia.Contexto.Dispose();
    }

    // ------------------------------------------------------------------ catálogos

    [Fact]
    public async Task Os_catalogos_trazem_as_listas_de_banco_E_os_dominios_fechados_do_codigo()
    {
        var (caso, contexto) = _banco.CasoDeCatalogos();

        var todos = await caso.ExecutarAsync(null, Ct);

        todos.EhSucesso.Should().BeTrue(todos.Erro);

        var codigos = todos.Valor.Dados.Select(c => c.Codigo).ToList();
        saida.WriteLine("catálogos devolvidos: " + string.Join(", ", codigos));

        codigos.Should().Contain("ORIGEM_LEAD", "é catálogo de banco, administrado pelo negócio");
        codigos.Should().Contain(ListarCatalogos.CodigoDeModelo, "o modelo tem tabela própria");
        codigos.Should().Contain(ListarCatalogos.CodigoDeEmpresa, "a filial alimenta o seletor de contexto");
        codigos.Should().Contain("SITUACAO_CLIENTE", "é domínio fechado no código, e a tela precisa dele igual");

        // A DIFERENÇA ENTRE OS DOIS SAI NA RESPOSTA: o que cresce sem release e o que exige
        // migração. Sem isso, a tela ofereceria um botão de "novo item" que não leva a lugar
        // nenhum num domínio de enum.
        todos.Valor.Dados.First(c => c.Codigo == "ORIGEM_LEAD").PermiteItemNovo.Should().BeTrue();
        todos.Valor.Dados.First(c => c.Codigo == "SITUACAO_CLIENTE").PermiteItemNovo.Should().BeFalse();

        var situacoes = todos.Valor.Dados.First(c => c.Codigo == "SITUACAO_CLIENTE");
        situacoes.Itens.Select(i => i.Codigo).Should().Contain("ClienteInativo");
        situacoes.Itens.First(i => i.Codigo == "ClienteInativo").Descricao.Should().Be("Cliente inativo");

        contexto.Dispose();
    }

    [Fact]
    public async Task Catalogo_que_nao_existe_devolve_nao_encontrado_e_diz_onde_ver_a_lista()
    {
        var (caso, contexto) = _banco.CasoDeCatalogos();

        var resultado = await caso.ExecutarAsync("CATALOGO_INVENTADO", Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoDeFalha.NaoEncontrado);
        resultado.Erro.Should().Contain("/api/v1/catalogos");

        saida.WriteLine(resultado.Erro!);

        contexto.Dispose();
    }
}
