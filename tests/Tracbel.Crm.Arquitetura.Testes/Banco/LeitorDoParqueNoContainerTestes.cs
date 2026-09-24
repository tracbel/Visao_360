using FluentAssertions;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// A LEITURA DO CADASTRO DE VEÍCULOS CONTRA UM SQL SERVER DE VERDADE — a consulta que roda no banco do Protheus, com
/// as colunas do tipo que a origem tem (tudo <c>varchar</c> de largura fixa, com espaço à direita; datas como
/// <c>AAAAMMDD</c>; o apagado como <c>D_E_L_E_T_ = '*'</c>).
///
/// <para>POR QUE NO CONTÊINER, e não num dublê: o que pode quebrar aqui é o SQL — o casamento do proprietário atual com
/// a SA1 por código E loja, o <c>ROW_NUMBER</c> da última nota e da última ordem de serviço, o filtro da nota válida e
/// da ordem cancelada, a comparação com espaço à direita. As tabelas são as mínimas, com os nomes e os tamanhos
/// medidos na produção em 24/09/2026.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class LeitorDoParqueNoContainerTestes
{
    /// <summary>Banco próprio deste teste, apagado e recriado a cada execução.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmProtheusParqueTeste";

    private const string CpfDoDono = "52998224725";
    private const string CnpjDaLoja1 = "11222333000181";
    private const string CnpjDaLoja2 = "11222333000262";
    private const string CpfDaOficina = "01234567890";

    [FatoSeHouverSqlServer]
    public async Task A_leitura_casa_o_dono_atual_por_codigo_e_loja_e_traz_a_evidencia()
    {
        RecriarBancoComAsTabelasDoProtheus();

        Executar($"""
            INSERT INTO dbo.SA1010 (A1_FILIAL, A1_COD, A1_LOJA, A1_CGC, A1_NOME, D_E_L_E_T_) VALUES
                ('      ', '000000001', '0001', '{CpfDoDono}   ', 'DONO FICTICIO', ' '),
                ('      ', '000000002', '0001', '{CnpjDaLoja1}', 'EMPRESA FICTICIA MATRIZ', ' '),
                ('      ', '000000002', '0002', '{CnpjDaLoja2}', 'EMPRESA FICTICIA FILIAL', ' '),
                ('      ', '000000003', '0001', '{CpfDaOficina}   ', 'OUTRO FICTICIO', ' ');

            INSERT INTO dbo.VE1010 (VE1_FILIAL, VE1_CODMAR, VE1_DESMAR, D_E_L_E_T_) VALUES ('0101  ', 'JD ', 'JOHN DEERE', ' ');
            INSERT INTO dbo.VV2010 (VV2_FILIAL, VV2_CODMAR, VV2_MODVEI, VV2_SEGMOD, VV2_DESMOD, VV2_GRUMOD, D_E_L_E_T_, R_E_C_N_O_) VALUES
                ('0101  ', 'JD ', '6110J', '          ', 'TRATOR 6110J', 'TR 6  ', ' ', 1),
                ('0101  ', 'JD ', 'SF6000', '          ', 'RECEPTOR STARFIRE', 'AP    ', ' ', 2);
            INSERT INTO dbo.VVR010 (VVR_FILIAL, VVR_CODMAR, VVR_GRUMOD, VVR_DESCRI, D_E_L_E_T_) VALUES
                ('0101  ', 'JD ', 'TR 6  ', 'TRATOR LINHA 6000', ' ');

            INSERT INTO dbo.VV1010 (VV1_FILIAL, VV1_CHAINT, VV1_CHASSI, VV1_CODMAR, VV1_MODVEI, VV1_SEGMOD, VV1_FABMOD, VV1_ESTVEI,
                                    VV1_SITVEI, VV1_PROATU, VV1_LJPATU, VV1_CLIULV, VV1_DATVEN, VV1_DTUVEN, D_E_L_E_T_) VALUES
                -- O chassi com espaço no meio e minúscula; nota do dono, e depois uma ordem de outro cliente.
                ('0101  ', '000001', '1cq6110j amr000001', 'JD ', '6110J', '          ', '20212022', '0', '1', '000000001', '0001', '         ', '20220301', '        ', ' '),
                -- O código com duas lojas de CNPJs diferentes: vale a loja do proprietário atual.
                ('0101  ', '000002', 'PY6110J012345', 'JD ', '6110J', '          ', '00000000', '1', ' ', '000000002', '0002', '000000002', '        ', '        ', ' '),
                -- Apagada no Protheus: não existe para a leitura.
                ('0101  ', '000003', '1RW6110JCMR000003', 'JD ', '6110J', '          ', '20202020', '0', '1', '000000001', '0001', '         ', '        ', '        ', '*'),
                -- Nota cancelada e ordem cancelada não são evidência; o componente vem com o grupo dele.
                ('0101  ', '000004', 'PCGU12345', 'JD ', 'SF6000', '          ', '        ', '0', '1', '000000003', '0001', '         ', '        ', '        ', ' '),
                -- Pedido de fábrica: sem chassi e sem dono.
                ('0101  ', '000005', '                         ', 'JD ', '6110J', '          ', '        ', '0', '8', '         ', '    ', '         ', '        ', '        ', ' ');

            INSERT INTO dbo.VV0010 (VV0_FILIAL, VV0_NUMTRA, VV0_OPEMOV, VV0_SITNFI, VV0_DATMOV, VV0_CODCLI, VV0_LOJA, D_E_L_E_T_) VALUES
                ('010101', '0000000001', '0', '1', '20220301', '000000001', '0001', ' '),
                ('010101', '0000000002', '0', '2', '20240101', '000000003', '0001', ' '),
                ('010113', '0000000003', '0', '1', '20230615', '000000002', '0002', ' ');
            INSERT INTO dbo.VVA010 (VVA_FILIAL, VVA_NUMTRA, VVA_CHAINT, D_E_L_E_T_) VALUES
                ('010101', '0000000001', '000001', ' '),
                ('010101', '0000000002', '000004', ' '),
                ('010113', '0000000003', '000002', ' ');

            INSERT INTO dbo.VO1010 (VO1_FILIAL, VO1_NUMOSV, VO1_CHAINT, VO1_PROVEI, VO1_LOJPRO, VO1_DATABE, VO1_STATUS, D_E_L_E_T_) VALUES
                ('010101', '00000001', '000001', '000000003', '0001', '20250110', 'F', ' '),
                ('010101', '00000002', '000004', '000000003', '0001', '20250110', 'C', ' '),
                ('010113', '00000003', '000002', '000000002', '0002', '20250501', 'F', ' ');
            """);

        var resultado = await new LeitorDoParqueDoProtheus(Opcoes()).LerAsync(CancellationToken.None);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro ?? string.Empty);
        var parque = resultado.Valor;
        parque.Maquinas.Should().HaveCount(4, "a apagada não é lida; a sem chassi é, e quem a descarta é a sincronia");

        parque.TentarAchar("1CQ6110JAMR000001", out var tratores).Should().BeTrue("o chassi é normalizado em C#, pela função do domínio");
        tratores.Should().BeEquivalentTo(new
        {
            ChaveInterna = "000001",
            ChassiNaOrigem = "1cq6110j amr000001",
            Marca = "JOHN DEERE",
            CodigoDoModelo = "6110J",
            GrupoDoModelo = "TR 6",
            DescricaoDoGrupo = "TRATOR LINHA 6000",
            Nova = (bool?)true,
            Situacao = "1",
            AnoFabricacao = (short?)2021,
            AnoModelo = (short?)2022,
            DocumentoDoDono = CpfDoDono,
            VendidaEm = (DateOnly?)new DateOnly(2022, 3, 1),
            UltimaVendaEm = (DateOnly?)new DateOnly(2022, 3, 1),
            UltimaVendaEDoDono = true,
            UltimaOrdemEm = (DateOnly?)new DateOnly(2025, 1, 10),
            UltimaOrdemEDoDono = false
        }, o => o.ExcludingMissingMembers());
        tratores.Evidencia.Should().Be(EvidenciaDoProprietario.NotaDeVenda);

        parque.TentarAchar("PY6110J012345", out var daFilial).Should().BeTrue();
        daFilial.DocumentoDoDono.Should().Be(CnpjDaLoja2, "o código tem duas lojas de CNPJs diferentes, e a loja do dono atual decide");
        daFilial.Situacao.Should().Be(string.Empty, "a carga antiga, sem situação");
        (daFilial.AnoFabricacao, daFilial.AnoModelo).Should().Be(((short?)null, (short?)null), "00000000 não é ano");
        daFilial.UltimaVendaEDoDono.Should().BeTrue("a nota de 010113 é para o mesmo código e loja");
        daFilial.UltimaOrdemEDoDono.Should().BeTrue();
        daFilial.DataDaEvidencia.Should().Be(new DateOnly(2025, 5, 1));

        parque.TentarAchar("PCGU12345", out var componente).Should().BeTrue();
        componente.GrupoDoModelo.Should().Be("AP");
        RegrasDoParque.EhComponente(componente).Should().BeTrue();
        componente.UltimaVendaEm.Should().BeNull("a nota cancelada não é venda");
        componente.UltimaOrdemEm.Should().BeNull("a ordem cancelada não é evidência");

        var pedido = parque.Maquinas.Single(m => m.ChaveInterna == "000005");
        pedido.Chassi.Should().BeEmpty();
        pedido.DocumentoDoDono.Should().BeNull();
    }

    // -----------------------------------------------------------------------------------------

    /// <summary>O acesso ao banco de teste no formato que o leitor recebe.</summary>
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

    /// <summary>Apaga e recria o banco de teste com as tabelas mínimas do Protheus, nos tipos da origem.</summary>
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
            CREATE TABLE dbo.SA1010 (A1_FILIAL varchar(6) NOT NULL, A1_COD varchar(9) NOT NULL, A1_LOJA varchar(4) NOT NULL,
                A1_CGC varchar(14) NOT NULL, A1_NOME varchar(60) NOT NULL, D_E_L_E_T_ varchar(1) NOT NULL);
            CREATE TABLE dbo.VE1010 (VE1_FILIAL varchar(6) NOT NULL, VE1_CODMAR varchar(3) NOT NULL, VE1_DESMAR varchar(30) NOT NULL,
                D_E_L_E_T_ varchar(1) NOT NULL);
            CREATE TABLE dbo.VV2010 (VV2_FILIAL varchar(6) NOT NULL, VV2_CODMAR varchar(3) NOT NULL, VV2_MODVEI varchar(30) NOT NULL,
                VV2_SEGMOD varchar(10) NOT NULL, VV2_DESMOD varchar(60) NOT NULL, VV2_GRUMOD varchar(6) NOT NULL,
                D_E_L_E_T_ varchar(1) NOT NULL, R_E_C_N_O_ int NOT NULL);
            CREATE TABLE dbo.VVR010 (VVR_FILIAL varchar(6) NOT NULL, VVR_CODMAR varchar(3) NOT NULL, VVR_GRUMOD varchar(6) NOT NULL,
                VVR_DESCRI varchar(20) NOT NULL, D_E_L_E_T_ varchar(1) NOT NULL);
            CREATE TABLE dbo.VV1010 (VV1_FILIAL varchar(6) NOT NULL, VV1_CHAINT varchar(6) NOT NULL, VV1_CHASSI varchar(25) NOT NULL,
                VV1_CODMAR varchar(3) NOT NULL, VV1_MODVEI varchar(30) NOT NULL, VV1_SEGMOD varchar(10) NOT NULL,
                VV1_FABMOD varchar(8) NOT NULL, VV1_ESTVEI varchar(1) NOT NULL, VV1_SITVEI varchar(1) NOT NULL,
                VV1_PROATU varchar(9) NOT NULL, VV1_LJPATU varchar(4) NOT NULL, VV1_CLIULV varchar(9) NOT NULL,
                VV1_DATVEN varchar(8) NOT NULL, VV1_DTUVEN varchar(8) NOT NULL, D_E_L_E_T_ varchar(1) NOT NULL);
            CREATE TABLE dbo.VV0010 (VV0_FILIAL varchar(6) NOT NULL, VV0_NUMTRA varchar(10) NOT NULL, VV0_OPEMOV varchar(1) NOT NULL,
                VV0_SITNFI varchar(1) NOT NULL, VV0_DATMOV varchar(8) NOT NULL, VV0_CODCLI varchar(9) NOT NULL,
                VV0_LOJA varchar(4) NOT NULL, D_E_L_E_T_ varchar(1) NOT NULL);
            CREATE TABLE dbo.VVA010 (VVA_FILIAL varchar(6) NOT NULL, VVA_NUMTRA varchar(10) NOT NULL, VVA_CHAINT varchar(6) NOT NULL,
                D_E_L_E_T_ varchar(1) NOT NULL);
            CREATE TABLE dbo.VO1010 (VO1_FILIAL varchar(6) NOT NULL, VO1_NUMOSV varchar(8) NOT NULL, VO1_CHAINT varchar(6) NOT NULL,
                VO1_PROVEI varchar(9) NOT NULL, VO1_LOJPRO varchar(4) NOT NULL, VO1_DATABE varchar(8) NOT NULL,
                VO1_STATUS varchar(1) NOT NULL, D_E_L_E_T_ varchar(1) NOT NULL);
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
