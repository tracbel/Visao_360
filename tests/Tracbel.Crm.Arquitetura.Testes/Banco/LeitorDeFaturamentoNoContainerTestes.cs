using FluentAssertions;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// A LEITURA DO FATURAMENTO CONTRA UM SQL SERVER DE VERDADE — a consulta que roda no banco do
/// Protheus, com as colunas do tipo que a origem tem (<c>D2_TOTAL</c> é <c>float</c>, <c>D2_CF</c> é
/// <c>varchar(5)</c> com espaço à direita, <c>D2_EMISSAO</c> é <c>varchar(8)</c>).
///
/// <para>POR QUE NO CONTÊINER, e não num dublê: o que pode quebrar aqui é o SQL — o <c>GROUP BY</c>, a
/// contagem de notas distintas, o <c>LEFT JOIN</c> da SA1, a comparação com espaço à direita, a
/// conversão do ponto flutuante. Um dublê em memória não exercitaria nada disso. As tabelas são as
/// mínimas, com os nomes e os tipos medidos na <c>SD2010</c> e na <c>SA1010</c> de produção em
/// 24/09/2026.</para>
///
/// <para>COMO RODAR: igual aos demais deste diretório — suba o contêiner e rode <c>dotnet test</c>.
/// Sem servidor, o teste é IGNORADO com a razão dita em voz alta.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class LeitorDeFaturamentoNoContainerTestes
{
    /// <summary>Banco próprio deste teste, apagado e recriado a cada execução.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmProtheusFaturamentoTeste";

    private static readonly DateOnly Desde = new(2026, 1, 1);

    [FatoSeHouverSqlServer]
    public async Task A_leitura_agrega_no_banco_so_a_venda_e_preserva_a_filial_da_nota()
    {
        RecriarBancoComAsTabelasDoProtheus();

        Executar("""
            INSERT INTO dbo.SA1010 (A1_COD, A1_LOJA, A1_CGC, A1_NOME, D_E_L_E_T_) VALUES
                ('000000001', '0001', '11222333000181', 'FAZENDA TESTE', ' '),
                ('000000009', '0001', '99999999000191', 'CADASTRO APAGADO', '*');

            INSERT INTO dbo.SD2010 (D2_FILIAL, D2_EMISSAO, D2_CLIENTE, D2_LOJA, D2_GRUPO, D2_TOTAL, D2_DOC, D2_SERIE, D2_CF, D2_TIPO, D_E_L_E_T_) VALUES
                -- A mesma nota com máquina e peça, e o CFOP com espaço à direita, como a origem grava.
                ('010101', '20260805', '000000001', '0001', 'VEIC', 1000000.10, '000000001', '1  ', '5102 ', 'N', ' '),
                ('010101', '20260806', '000000001', '0001', '1001', 199.95,     '000000001', '1  ', '5405 ', 'N', ' '),
                -- Mesmo número, OUTRA série: é outra nota.
                ('010101', '20260820', '000000001', '0001', 'SRV ', 10.00,      '000000001', '2  ', '5933 ', 'N', ' '),
                -- O mesmo cliente comprando em São José do Rio Preto: outra filial, outra linha.
                ('010113', '20260810', '000000001', '0001', '1001', 50.00,      '000000077', '1  ', '6102 ', 'N', ' '),
                -- Transferência e beneficiamento: saem pela porta e não são venda.
                ('010101', '20260811', '000000001', '0001', '1001', 30.00,      '000000002', '1  ', '5152 ', 'N', ' '),
                ('010101', '20260812', '000000001', '0001', '1001', 40.00,      '000000003', '1  ', '5102 ', 'B', ' '),
                -- Apagada no Protheus, e emitida antes da janela: não existem para a leitura.
                ('010101', '20260813', '000000001', '0001', '1001', 60.00,      '000000004', '1  ', '5102 ', 'N', '*'),
                ('010101', '20251231', '000000001', '0001', '1001', 70.00,      '000000005', '1  ', '5102 ', 'N', ' '),
                -- Cliente cujo cadastro está apagado na SA1: venda sem documento.
                ('010102', '20260815', '000000009', '0001', '1001', 25.00,      '000000006', '1  ', '5102 ', 'N', ' ');
            """);

        var resultado = await new LeitorDeFaturamentoDoProtheus(Opcoes()).LerAsync(Desde, _ => { }, CancellationToken.None);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro ?? string.Empty);
        var lote = resultado.Valor;

        var ribeirao = lote.Faturamento.Single(f => f.CodigoDaFilial == "010101");
        ribeirao.DocumentoDoCliente.Should().Be("11222333000181");
        ribeirao.Competencia.Should().Be(new DateOnly(2026, 8, 1));
        ribeirao.ValorLiquido.Should().Be(1_000_210.05m, "o float da origem vira decimal item a item, sem resíduo");
        ribeirao.Notas.Should().Be(2, "a nota 1 da série 1 (máquina e peça) e a nota 1 da série 2");
        ribeirao.Itens.Should().Be(3);
        ribeirao.Quebra.Maquina.Should().Be(1_000_000.10m);
        ribeirao.Quebra.Peca.Should().Be(199.95m);
        ribeirao.Quebra.Servico.Should().Be(10.00m);
        ribeirao.NomeNaOrigem.Should().Be("FAZENDA TESTE");

        lote.Faturamento.Single(f => f.CodigoDaFilial == "010113").ValorLiquido.Should().Be(50m,
            "a filial é a da nota: o mesmo cliente em outra praça é outra linha, não soma em 010101");
        lote.Faturamento.Should().HaveCount(2);
        lote.FiliaisVistas.Should().Equal("010101", "010113");

        lote.CodigosSemDocumento.Should().Be(1);
        lote.ValorSemDocumento.Should().Be(25m);

        lote.ItensLidos.Should().Be(7, "tudo o que não está apagado e foi emitido na janela, venda ou não");
        lote.ItensQueNaoSaoVenda.Should().Be(2);
        lote.ValorQueNaoEhVenda.Should().Be(70m);
        lote.EmissaoMaisRecente.Should().Be(new DateOnly(2026, 8, 20));
        lote.Desde.Should().Be(Desde);
    }

    [FatoSeHouverSqlServer]
    public async Task A_janela_sem_nota_nenhuma_devolve_lote_vazio_e_nao_erro()
    {
        RecriarBancoComAsTabelasDoProtheus();

        var resultado = await new LeitorDeFaturamentoDoProtheus(Opcoes()).LerAsync(Desde, _ => { }, CancellationToken.None);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro ?? string.Empty);
        resultado.Valor.Faturamento.Should().BeEmpty();
        resultado.Valor.ItensLidos.Should().Be(0);
        resultado.Valor.EmissaoMaisRecente.Should().BeNull();
    }

    // -----------------------------------------------------------------------------------------

    /// <summary>O acesso ao banco de teste no formato que o leitor recebe — o mesmo da conexão da tela.</summary>
    private static OpcoesDoBancoDoProtheus Opcoes()
    {
        var cadeia = new SqlConnectionStringBuilder(ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste));
        return new OpcoesDoBancoDoProtheus
        {
            Servidor = cadeia.DataSource,
            Banco = cadeia.InitialCatalog,
            Usuario = cadeia.UserID,
            Senha = cadeia.Password
        };
    }

    /// <summary>Apaga e recria o banco de teste com a SD2 e a SA1 mínimas, nos tipos da origem.</summary>
    private static void RecriarBancoComAsTabelasDoProtheus()
    {
        SqlConnection.ClearAllPools();

        using (var master = new SqlConnection(ConexaoDeSqlServer.MontarPara("master")))
        {
            master.Open();
            using var comando = master.CreateCommand();
            comando.CommandTimeout = 180;
            comando.CommandText = $"""
                IF DB_ID(N'{NomeDoBancoDeTeste}') IS NOT NULL
                BEGIN
                    ALTER DATABASE [{NomeDoBancoDeTeste}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{NomeDoBancoDeTeste}];
                END;
                CREATE DATABASE [{NomeDoBancoDeTeste}];
                """;
            comando.ExecuteNonQuery();
        }

        Executar("""
            CREATE TABLE dbo.SD2010 (
                D2_FILIAL varchar(6) NOT NULL, D2_EMISSAO varchar(8) NOT NULL, D2_CLIENTE varchar(9) NOT NULL,
                D2_LOJA varchar(4) NOT NULL, D2_GRUPO varchar(4) NOT NULL, D2_TOTAL float NOT NULL,
                D2_DOC varchar(9) NOT NULL, D2_SERIE varchar(3) NOT NULL, D2_CF varchar(5) NOT NULL,
                D2_TIPO varchar(1) NOT NULL, D_E_L_E_T_ varchar(1) NOT NULL);

            CREATE TABLE dbo.SA1010 (
                A1_COD varchar(9) NOT NULL, A1_LOJA varchar(4) NOT NULL, A1_CGC varchar(14) NOT NULL,
                A1_NOME varchar(60) NOT NULL, D_E_L_E_T_ varchar(1) NOT NULL);
            """);
    }

    private static void Executar(string sql)
    {
        using var conexao = new SqlConnection(ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste));
        conexao.Open();
        using var comando = conexao.CreateCommand();
        comando.CommandText = sql;
        comando.ExecuteNonQuery();
    }
}
