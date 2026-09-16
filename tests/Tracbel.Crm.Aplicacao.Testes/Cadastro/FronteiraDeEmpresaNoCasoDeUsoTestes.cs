using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Aplicacao.Clientes;
using Tracbel.Crm.Aplicacao.Equipamentos;
using Tracbel.Crm.Dominio.Comum;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Aplicacao.Testes.Cadastro;

/// <summary>
/// A FRONTEIRA DE EMPRESA VISTA DE CIMA — pelo caminho que a API percorre de verdade.
///
/// <para><c>FronteiraDeEmpresaTestes</c> já prova que o filtro global existe e vira cláusula SQL.
/// Este arquivo prova a outra metade, que é a que interessa a quem consome: que o CASO DE USO,
/// com os repositórios de verdade no meio, não devolve por descuido o que o filtro barra. É a
/// diferença entre "o mecanismo está lá" e "o caminho inteiro respeita o mecanismo".</para>
///
/// <para>O último teste é uma PROVA NEGATIVA: ele desliga a proteção e confirma que, sem ela, o
/// mesmo caso de uso entrega o dado da outra filial. Sem esse teste, os outros provariam apenas
/// que o banco está vazio para o segundo usuário — que é uma explicação alternativa perfeitamente
/// compatível com todas as asserções acima dele.</para>
/// </summary>
[Trait("Categoria", "Autorizacao")]
public sealed class FronteiraDeEmpresaNoCasoDeUsoTestes(ITestOutputHelper saida) : IDisposable
{
    private readonly BancoDeTeste _banco = new();

    private static CancellationToken Ct => CancellationToken.None;

    public void Dispose() => _banco.Dispose();

    /// <summary>Cria um cliente em Ribeirão Preto e devolve a chave pública dele.</summary>
    private async Task<Guid> CriarEmRibeiraoAsync()
    {
        _banco.AgirComo(BancoDeTeste.UsuarioDeRibeirao, BancoDeTeste.RibeiraoPreto);

        var casos = _banco.CasosDeCliente();
        var criado = await casos.Criar.ExecutarAsync(
            new NovoCliente(
                NomeRazao: "Fazenda Só de Ribeirão Ltda",
                TipoDePessoa: "Juridica",
                Documento: "11.222.333/0001-81"),
            Ct);

        criado.EhSucesso.Should().BeTrue(criado.Erro);
        casos.Contexto.Dispose();

        return criado.Valor.Chave;
    }

    [Fact]
    public async Task Obter_no_contexto_de_outra_filial_devolve_nao_encontrado()
    {
        var chave = await CriarEmRibeiraoAsync();

        _banco.AgirComo(BancoDeTeste.UsuarioDeBarretos, BancoDeTeste.Barretos);

        var casos = _banco.CasosDeCliente();
        var resultado = await casos.Obter.ExecutarAsync(chave, Ct);

        resultado.EhSucesso.Should().BeFalse();

        // NÃO ENCONTRADO, e não "proibido", de propósito: distinguir os dois contaria a quem não
        // pode ver que o registro existe — vazamento de informação por código de status.
        resultado.Tipo.Should().Be(TipoDeFalha.NaoEncontrado);

        saida.WriteLine($"Barretos pedindo {chave}: {resultado.Tipo} — {resultado.Erro}");

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Listar_no_contexto_de_outra_filial_nao_traz_o_registro_nem_no_total()
    {
        await CriarEmRibeiraoAsync();

        _banco.AgirComo(BancoDeTeste.UsuarioDeBarretos, BancoDeTeste.Barretos);

        var casos = _banco.CasosDeCliente();
        var lista = await casos.Listar.ExecutarAsync(null, null, null, null, null, null, false, true, Ct);

        lista.EhSucesso.Should().BeTrue(lista.Erro);
        lista.Valor.Dados.Itens.Should().BeEmpty();

        // O TOTAL TAMBÉM É ZERO, e essa asserção não é redundante: um filtro aplicado só na
        // página, e não na contagem, entregaria uma lista vazia com "total: 1" — e a tela diria
        // que existe um registro que ela não pode mostrar. Isso já é vazamento.
        lista.Valor.Dados.Total.Should().Be(0);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task Alterar_e_inativar_de_outra_filial_tambem_batem_na_fronteira()
    {
        // A fronteira não pode valer só para a leitura: se ela valesse só lá, bastaria um PUT
        // com a chave certa para alterar o cadastro de outra filial sem nunca conseguir lê-lo.
        var chave = await CriarEmRibeiraoAsync();

        _banco.AgirComo(BancoDeTeste.UsuarioDeBarretos, BancoDeTeste.Barretos);

        var casos = _banco.CasosDeCliente();

        var alteracao = await casos.Alterar.ExecutarAsync(
            chave, new AlteracaoDeCliente(NomeRazao: "Sequestro de cadastro", TipoDePessoa: "Juridica"), Ct);

        alteracao.Tipo.Should().Be(TipoDeFalha.NaoEncontrado);

        var inativacao = await casos.Inativar.ExecutarAsync(
            chave, new InativacaoDeCliente(MotivoCodigo: "DUPLICADO"), Ct);

        inativacao.Tipo.Should().Be(TipoDeFalha.NaoEncontrado);

        casos.Contexto.Dispose();
    }

    [Fact]
    public async Task O_equipamento_de_outra_filial_tambem_nao_aparece()
    {
        _banco.AgirComo(BancoDeTeste.UsuarioDeRibeirao, BancoDeTeste.RibeiraoPreto);

        var cliente = await CriarEmRibeiraoAsync();

        var frota = _banco.CasosDeEquipamento();
        var maquina = await frota.Criar.ExecutarAsync(
            new NovoEquipamentoDeTeste(cliente).Requisicao, Ct);

        maquina.EhSucesso.Should().BeTrue(maquina.Erro);
        frota.Contexto.Dispose();

        _banco.AgirComo(BancoDeTeste.UsuarioDeBarretos, BancoDeTeste.Barretos);

        var outra = _banco.CasosDeEquipamento();
        var lista = await outra.Listar.ExecutarAsync(
            null, null, null, null, null, null, null, false, true, null, null, false, Ct);

        lista.EhSucesso.Should().BeTrue(lista.Erro);
        lista.Valor.Dados.Total.Should().Be(0, "a fronteira vale para toda entidade com EmpresaId");

        outra.Contexto.Dispose();
    }

    // =============================================================================================
    //  A PROVA NEGATIVA
    // =============================================================================================

    /// <summary>
    /// DESLIGA A PROTEÇÃO E CONFIRMA QUE O TESTE FALHARIA.
    ///
    /// <para>Os quatro testes acima afirmam que Barretos não enxerga o cliente de Ribeirão Preto.
    /// Sozinhos, eles são compatíveis com uma explicação bem mais chata: a de que o registro
    /// simplesmente não foi gravado, ou de que a consulta está errada e nunca devolve nada. Um
    /// teste que passa pelo motivo errado é pior do que teste nenhum, porque dá segurança falsa.</para>
    ///
    /// <para>Então aqui a MESMA consulta roda com <c>IgnoreQueryFilters</c> — a única forma de
    /// desligar o filtro global — e o registro APARECE. Isso prende os dois lados: o dado está lá,
    /// a consulta funciona, e o que separa um resultado do outro é exclusivamente a fronteira.</para>
    ///
    /// <para>É o mesmo formato de prova já usado em <c>FronteiraDeEmpresaTestes</c>, aplicado
    /// agora ao caminho do caso de uso.</para>
    /// </summary>
    [Fact]
    public async Task Prova_negativa_sem_o_filtro_global_o_registro_da_outra_filial_aparece()
    {
        var chave = await CriarEmRibeiraoAsync();

        _banco.AgirComo(BancoDeTeste.UsuarioDeBarretos, BancoDeTeste.Barretos);

        await using var contexto = _banco.NovoContexto();

        var comFronteira = await contexto.Clientes
            .Where(c => c.ChavePublica == chave)
            .Select(c => c.NomeRazao)
            .ToListAsync(Ct);

        var semFronteira = await contexto.Clientes
            .IgnoreQueryFilters()
            .Where(c => c.ChavePublica == chave)
            .Select(c => c.NomeRazao)
            .ToListAsync(Ct);

        saida.WriteLine("--- SQL da consulta COM a fronteira ---");
        saida.WriteLine(contexto.Clientes.Where(c => c.ChavePublica == chave).ToQueryString());
        saida.WriteLine(string.Empty);
        saida.WriteLine($"COM a fronteira, no contexto de Barretos: {comFronteira.Count} linha(s)");
        saida.WriteLine($"SEM a fronteira, a MESMA consulta:        {semFronteira.Count} linha(s) " +
                        $"→ {string.Join(", ", semFronteira)}");

        comFronteira.Should().BeEmpty("é a fronteira que barra");

        semFronteira.Should().ContainSingle(
            "o dado ESTÁ no banco e a consulta funciona — o que muda entre as duas linhas acima é " +
            "exclusivamente o filtro global. Se esta asserção falhar, os outros testes deste " +
            "arquivo estão passando pelo motivo errado.");
    }
}

/// <summary>Uma requisição de equipamento válida, montada num lugar só.</summary>
internal sealed class NovoEquipamentoDeTeste(Guid clienteChave)
{
    public Aplicacao.Equipamentos.NovoEquipamento Requisicao { get; } = new(
        Chassi: "1JD8R340ABC123456",
        ModeloCodigo: BancoDeTeste.ModeloSemeado,
        ClienteChave: clienteChave.ToString(),
        Situacao: "Ativo",
        Origem: "Crm",
        AnoFabricacao: "2023",
        AnoModelo: "2024");
}
