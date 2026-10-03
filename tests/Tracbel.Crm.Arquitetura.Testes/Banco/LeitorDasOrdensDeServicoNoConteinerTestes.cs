using FluentAssertions;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// A LEITURA DAS ORDENS DE SERVIÇO CONTRA UM SQL SERVER DE VERDADE — as duas consultas que rodam nas views do BI do
/// Protheus. A definição das views não vem no arquivo do Qlik, e o tipo das colunas de data não se vê daqui: por isso a
/// view dos itens tem as datas como <c>datetime</c>, e a dos serviços como texto <c>DD/MM/AAAA</c> com vazio — os dois
/// caminhos da conversão da janela no banco.
///
/// <para>O que pode quebrar aqui é o SQL: a janela pela abertura, a OS ainda aberta que entra de qualquer data, o serviço da
/// OS fechada antiga que fica de fora, o espaço à direita, o número e o documento.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class LeitorDasOrdensDeServicoNoConteinerTestes
{
    /// <summary>Banco próprio deste teste, apagado e recriado a cada execução.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmProtheusOrdensTeste";

    private const string CnpjDoProprietario = "11444777000161";

    [FatoSeHouverSqlServer]
    public async Task A_leitura_traz_a_janela_e_as_abertas_antigas_e_deixa_de_fora_a_fechada_antiga()
    {
        RecriarBancoComAsViews();

        Executar($"""
            INSERT INTO dbo.X_V_BI_SERVICOS_CAPA_E_ITENS_OS (FILIAL, NUMERO_OS, STATUS_CAPA_OS, DESC_TIPO_ATEND_CAPA, DATA_ABER, DATA_LIBE,
                DATA_CANC, DATA_FECH, CHASSI, MODELO, HORIME, CPF_CNPJ, PEC_OU_SRV, TIPO_TEMPO, COD_ITEM, NOSS_NUM_REQ, QTDADE, VLR_UNIT,
                VLR_DESC, FORMULA, GRUPO, PROD_REQ, ORIGI_PARALE) VALUES
                -- Recente e aberta: entra, com o espaço à direita aparado e o documento só com dígitos.
                ('010101', '00012345', 'A ', 'OFICINA   ', '2026-09-01', NULL, NULL, NULL, '1PY6155MCSS000001   ', 'TRATOR 6155M  ', 1234.5,
                 '11.444.777/0001-61', 'PEÇ', 'C', 'PECA-1    ', '00000011', 2.00, 100.00, 30.00, '      ', '1001', '      ', '      '),
                -- Antiga, ainda liberada (o D antigo): entra de qualquer data.
                ('010101', '00000777', 'D', 'CAMPO', '2021-03-10', '2021-03-15', NULL, NULL, 'PY6110J012345', 'TRATOR 6110J', NULL,
                 '{CnpjDoProprietario}', 'SRV', 'C', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),
                -- Antiga e fechada: fora da janela.
                ('010105', '00000888', 'F', 'OFICINA', '2021-03-10', NULL, NULL, '2021-03-20', 'PY6110J099999', 'TRATOR 6110J', NULL,
                 '{CnpjDoProprietario}', 'PEÇ', 'C', 'PECA-9', '00000001', 1, 10, 0, NULL, NULL, NULL, NULL);

            INSERT INTO dbo.X_V_BI_SERVICOS_SRV_EXECUTADO_OS (VO4_FILIAL, VO4_NUMOSV, COD_PROD, ABERTURA, FECHAMENTO, CANCELAMENTO, TpTpo,
                Desc_TpTpo, TpServico, GruServico, CodServico, DEP_INT_OS, DEP_GAR_OS, VOK_INCMOB, VOK_PREKIL, VOI_SITTPO, VO4_TIPTEM,
                TEMPAD, TEMTRA, TEMCOB, TEMVEN, TEMPAD_TOTAL, TEMTRA_TOTAL, TEMCOB_TOTAL, TEMVEN_TOTAL, VZ1_VALDES, VSC_VALDES, VO4_VALDES,
                VO4_PREKIL, VO4_KILROD, VSC_KILROD, VO4_VALINT, VO4_VALHOR, VO4_VALVEN, VZ1_VALUNI, VZ1_VALBRU, VSC_VALBRU) VALUES
                ('010101', '00012345', 'TEC01 ', '01/09/2026', '          ', '          ', 'C', 'CLIENTE', 'MO', 'AG', 'REVISAO', NULL, NULL,
                 '1', 0, '1', 'C', 2, 2, 2, 2, 4, 4, 4, 4, 0, 0, 30, 0, 0, 0, 80, 150, 0, 0, 0, 0),
                -- O serviço da OS antiga ainda aberta: sem fechamento nem cancelamento, entra.
                ('010101', '00000777', 'TEC02', '10/03/2021', '', '', 'C', 'CLIENTE', 'MO', 'AG', 'REVISAO', NULL, NULL,
                 '1', 0, '1', 'C', 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 80, 150, 0, 0, 0, 0),
                -- O serviço da OS antiga fechada: fora.
                ('010105', '00000888', 'TEC03', '10/03/2021', '20/03/2021', '', 'C', 'CLIENTE', 'MO', 'AG', 'REVISAO', NULL, NULL,
                 '1', 0, '1', 'C', 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 80, 150, 0, 0, 0, 0);
            """);

        var resultado = await new LeitorDasOrdensDeServicoDoProtheus(Opcoes()).LerAsync(new DateOnly(2023, 10, 1), null, CancellationToken.None);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro ?? string.Empty);
        var leitura = resultado.Valor;

        leitura.Itens.Select(i => i.NumeroOs).Should().BeEquivalentTo(["00012345", "00000777"], "a fechada de 2021 está fora da janela");
        var recente = leitura.Itens.Single(i => i.NumeroOs == "00012345");
        recente.Should().BeEquivalentTo(new
        {
            Filial = "010101",
            StatusDaCapa = "A",
            TipoDeAtendimento = "OFICINA",
            AbertaEm = (DateOnly?)new DateOnly(2026, 9, 1),
            Chassi = "1PY6155MCSS000001",
            Modelo = "TRATOR 6155M",
            Horimetro = (decimal?)1234.5m,
            DocumentoDoProprietario = CnpjDoProprietario,
            PecaOuServico = "PEÇ",
            CodigoDoItem = "PECA-1",
            Requisicao = "00000011",
            Quantidade = (decimal?)2m,
            ValorUnitario = (decimal?)100m,
            ValorDoDesconto = (decimal?)30m,
            Formula = (string?)null
        }, o => o.ExcludingMissingMembers());
        leitura.Itens.Single(i => i.NumeroOs == "00000777").LiberadaEm.Should().Be(new DateOnly(2021, 3, 15));

        leitura.Servicos.Select(s => s.NumeroOs).Should().BeEquivalentTo(["00012345", "00000777"]);
        var servico = leitura.Servicos.Single(s => s.NumeroOs == "00012345");
        (servico.CodProd, servico.Abertura, servico.Vo4ValHor, servico.TemCobTotal).Should().Be(("TEC01", (DateOnly?)new DateOnly(2026, 9, 1), (decimal?)150m, (decimal?)4m));
        RegrasDoValorDaOrdemDeServico.ValorDoServico(servico).Should().Be(285m);
    }

    [FatoSeHouverSqlServer]
    public async Task A_leitura_curta_traz_a_OS_antiga_fechada_ha_pouco_so_quando_pede_as_mudancas()
    {
        RecriarBancoComAsViews();

        Executar($"""
            INSERT INTO dbo.X_V_BI_SERVICOS_CAPA_E_ITENS_OS (FILIAL, NUMERO_OS, STATUS_CAPA_OS, DESC_TIPO_ATEND_CAPA, DATA_ABER, DATA_LIBE,
                DATA_CANC, DATA_FECH, CHASSI, MODELO, HORIME, CPF_CNPJ, PEC_OU_SRV, TIPO_TEMPO, COD_ITEM, NOSS_NUM_REQ, QTDADE, VLR_UNIT,
                VLR_DESC, FORMULA, GRUPO, PROD_REQ, ORIGI_PARALE) VALUES
                -- Aberta em 2021 e fechada em setembro de 2026: só a leitura curta, pelas mudanças, a traz.
                ('010101', '00000999', 'F', 'OFICINA', '2021-03-10', NULL, NULL, '2026-09-20', 'PY6110J055555', 'TRATOR 6110J', NULL,
                 '{CnpjDoProprietario}', 'PEÇ', 'C', 'PECA-5', '00000005', 1, 10, 0, NULL, NULL, NULL, NULL),
                -- Aberta em 2021 e cancelada em 2022: fora das duas.
                ('010105', '00000888', 'C', 'OFICINA', '2021-03-10', NULL, '2022-01-05', NULL, 'PY6110J099999', 'TRATOR 6110J', NULL,
                 '{CnpjDoProprietario}', 'PEÇ', 'C', 'PECA-9', '00000001', 1, 10, 0, NULL, NULL, NULL, NULL);

            INSERT INTO dbo.X_V_BI_SERVICOS_SRV_EXECUTADO_OS (VO4_FILIAL, VO4_NUMOSV, COD_PROD, ABERTURA, FECHAMENTO, CANCELAMENTO, TpTpo,
                Desc_TpTpo, TpServico, GruServico, CodServico, DEP_INT_OS, DEP_GAR_OS, VOK_INCMOB, VOK_PREKIL, VOI_SITTPO, VO4_TIPTEM,
                TEMPAD, TEMTRA, TEMCOB, TEMVEN, TEMPAD_TOTAL, TEMTRA_TOTAL, TEMCOB_TOTAL, TEMVEN_TOTAL, VZ1_VALDES, VSC_VALDES, VO4_VALDES,
                VO4_PREKIL, VO4_KILROD, VSC_KILROD, VO4_VALINT, VO4_VALHOR, VO4_VALVEN, VZ1_VALUNI, VZ1_VALBRU, VSC_VALBRU) VALUES
                ('010101', '00000999', 'TEC05', '10/03/2021', '20/09/2026', '', 'C', 'CLIENTE', 'MO', 'AG', 'REVISAO', NULL, NULL,
                 '1', 0, '1', 'C', 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 80, 150, 0, 0, 0, 0),
                ('010105', '00000888', 'TEC03', '10/03/2021', '', '05/01/2022', 'C', 'CLIENTE', 'MO', 'AG', 'REVISAO', NULL, NULL,
                 '1', 0, '1', 'C', 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 80, 150, 0, 0, 0, 0);
            """);

        var leitor = new LeitorDasOrdensDeServicoDoProtheus(Opcoes());
        var curta = (await leitor.LerAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 1), CancellationToken.None)).Valor;
        var semAsMudancas = (await leitor.LerAsync(new DateOnly(2026, 9, 1), null, CancellationToken.None)).Valor;

        curta.Itens.Select(i => i.NumeroOs).Should().BeEquivalentTo(["00000999"], "fechada em 20/09/2026, depois do início curto");
        curta.Servicos.Select(s => s.NumeroOs).Should().BeEquivalentTo(["00000999"]);
        semAsMudancas.Itens.Should().BeEmpty("sem as mudanças, a consulta é a de sempre: abertura ou situação em aberto");
        semAsMudancas.Servicos.Should().BeEmpty();
    }

    [FatoSeHouverSqlServer]
    public async Task A_view_que_nao_existe_diz_o_motivo_sem_citar_servidor_nem_usuario()
    {
        RecriarBancoComAsViews();
        Executar("DROP TABLE dbo.X_V_BI_SERVICOS_SRV_EXECUTADO_OS;");

        var resultado = await new LeitorDasOrdensDeServicoDoProtheus(Opcoes()).LerAsync(new DateOnly(2023, 10, 1), null, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("erro SQL 208").And.Contain("não existe neste banco").And.NotContain(Opcoes().Usuario!);
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

    /// <summary>
    /// Apaga e recria o banco de teste com as duas "views" — aqui, tabelas com o mesmo nome e as colunas que o script do BI
    /// seleciona; a leitura não distingue uma da outra.
    /// </summary>
    private static void RecriarBancoComAsViews()
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
            CREATE TABLE dbo.X_V_BI_SERVICOS_CAPA_E_ITENS_OS (
                FILIAL varchar(6) NOT NULL, NUMERO_OS varchar(8) NOT NULL, STATUS_CAPA_OS varchar(2) NULL, DESC_TIPO_ATEND_CAPA varchar(30) NULL,
                DATA_ABER datetime NULL, DATA_LIBE datetime NULL, DATA_CANC datetime NULL, DATA_FECH datetime NULL,
                CHASSI varchar(25) NULL, MODELO varchar(30) NULL, HORIME numeric(10, 1) NULL, CPF_CNPJ varchar(20) NULL,
                PEC_OU_SRV varchar(3) NULL, TIPO_TEMPO varchar(4) NULL, COD_ITEM varchar(30) NULL, NOSS_NUM_REQ varchar(8) NULL,
                QTDADE numeric(12, 2) NULL, VLR_UNIT numeric(14, 2) NULL, VLR_DESC numeric(14, 2) NULL,
                FORMULA varchar(6) NULL, GRUPO varchar(4) NULL, PROD_REQ varchar(6) NULL, ORIGI_PARALE varchar(6) NULL);
            CREATE TABLE dbo.X_V_BI_SERVICOS_SRV_EXECUTADO_OS (
                VO4_FILIAL varchar(6) NOT NULL, VO4_NUMOSV varchar(8) NOT NULL, COD_PROD varchar(6) NULL,
                ABERTURA varchar(10) NULL, FECHAMENTO varchar(10) NULL, CANCELAMENTO varchar(10) NULL,
                TpTpo varchar(4) NULL, Desc_TpTpo varchar(30) NULL, TpServico varchar(3) NULL, GruServico varchar(2) NULL,
                CodServico varchar(30) NULL, DEP_INT_OS varchar(6) NULL, DEP_GAR_OS varchar(6) NULL, VOK_INCMOB varchar(1) NULL,
                VOK_PREKIL numeric(12, 2) NULL, VOI_SITTPO varchar(1) NULL, VO4_TIPTEM varchar(4) NULL,
                TEMPAD numeric(10, 2) NULL, TEMTRA numeric(10, 2) NULL, TEMCOB numeric(10, 2) NULL, TEMVEN numeric(10, 2) NULL,
                TEMPAD_TOTAL numeric(10, 2) NULL, TEMTRA_TOTAL numeric(10, 2) NULL, TEMCOB_TOTAL numeric(10, 2) NULL, TEMVEN_TOTAL numeric(10, 2) NULL,
                VZ1_VALDES numeric(14, 2) NULL, VSC_VALDES numeric(14, 2) NULL, VO4_VALDES numeric(14, 2) NULL, VO4_PREKIL numeric(12, 2) NULL,
                VO4_KILROD numeric(12, 2) NULL, VSC_KILROD numeric(12, 2) NULL, VO4_VALINT numeric(14, 2) NULL, VO4_VALHOR numeric(14, 2) NULL,
                VO4_VALVEN numeric(14, 2) NULL, VZ1_VALUNI numeric(14, 2) NULL, VZ1_VALBRU numeric(14, 2) NULL, VSC_VALBRU numeric(14, 2) NULL);
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
