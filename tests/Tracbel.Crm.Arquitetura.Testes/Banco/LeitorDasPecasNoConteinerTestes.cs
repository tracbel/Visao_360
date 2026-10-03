using FluentAssertions;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// A LEITURA DAS PEÇAS CONTRA UM SQL SERVER DE VERDADE — as duas consultas que rodam nas views do BI. O que pode quebrar
/// aqui é o SQL: a soma por mês no próprio banco (o <c>GROUP BY</c> sobre a data convertida), a janela pela emissão, o
/// orçamento aberto que entra de qualquer data, e a coluna <c>data_orçamento_orc</c>, com "ç" no nome.
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class LeitorDasPecasNoConteinerTestes
{
    private const string NomeDoBancoDeTeste = "TracbelCrmProtheusPecasTeste";

    [FatoSeHouverSqlServer]
    public async Task A_leitura_soma_o_faturamento_por_mes_no_banco_e_traz_os_orcamentos_da_janela_e_os_abertos()
    {
        RecriarBancoComAsViews();

        Executar("""
            INSERT INTO dbo.X_V_BI_FATURAMENTO_PECAS (filial, data_emissao_nf, seq_pessoa, setor, origem, nome_linha_produto, cod_familia_produto,
                id_vendedor, nome_vendedor, tp_cortesia, tp_ordem, nome_operacao, qtde, vlr_liquido_item, vlr_desconto, vlr_tabela, custo_medio) VALUES
                ('01', '20260903', '11444777000161', 'BALCAO', 'FAT_PECAS', 'PECAS JD', '9999', '000123', 'VENDEDOR A', 'NAO', 'VEN', 'VENDA', 2, 100.50, 5, 110, 70),
                ('01', '20260915', '11444777000161', 'BALCAO', 'FAT_PECAS', 'PECAS JD', '9999', '000123', 'VENDEDOR A', 'NAO', 'VEN', 'VENDA', 1, 49.50, 0, 50, 30),
                ('01', '20260810', '11444777000161', 'BALCAO', 'FAT_PECAS', 'PECAS JD', '9999', '000123', 'VENDEDOR A', 'NAO', 'VEN', 'VENDA', 1, 10, 0, 10, 5),
                ('01', '20200101', '11444777000161', 'BALCAO', 'FAT_PECAS', 'PECAS JD', '9999', '000123', 'VENDEDOR A', 'NAO', 'VEN', 'VENDA', 1, 999, 0, 999, 5);

            INSERT INTO dbo.X_V_BI_POSICAO_ORC_PECAS (filial_orc, nro_orc, STATUS_ORC, Situacao_orc, status_reserva_orc, tipo_atendimento_orc,
                tipo_orcamento_orc, cpf_cnpj_orc, [data_orçamento_orc], data_validade_orc, data_alteracao_orc, cod_vededor_orc, nome_vededor_orc,
                cod_item_orc, vlr_total_orc, vlr_desc_orc, vlr_margem_lucro_orc) VALUES
                ('01', '000777', 'Aberto', 'NO PRAZO', 'RESERVADO', 'BALCAO', 'VENDA', '11.444.777/0001-61', '2026-09-20', '2026-10-20', NULL, '000123', 'VENDEDOR A', 'PECA-1', 300, 10, 99),
                ('01', '000700', 'Parcialmente Atendido', 'VENCIDO', 'NAO RESERVADO', 'BALCAO', 'VENDA', '11444777000161', '2021-01-10', '2021-02-10', NULL, '000123', 'VENDEDOR A', 'PECA-2', 50, 0, 1),
                ('01', '000600', 'Encerrado', 'ATENDIDO', 'RESERVADO', 'BALCAO', 'VENDA', '11444777000161', '2021-01-10', '2021-02-10', NULL, '000123', 'VENDEDOR A', 'PECA-3', 70, 0, 1);
            """);

        var opcoes = Opcoes();
        var resultado = await new LeitorDasPecasDoProtheus(opcoes).LerAsync(new DateOnly(2023, 10, 1), new DateOnly(2024, 10, 1), null, CancellationToken.None);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro ?? string.Empty);
        var leitura = resultado.Valor;

        leitura.Faturamento.Should().HaveCount(2, "setembro soma as duas notas no banco; a nota de 2020 está fora da janela");
        var setembro = leitura.Faturamento.Single(f => f.Mes == new DateOnly(2026, 9, 1));
        (setembro.Quantidade, setembro.ValorLiquido, setembro.ValorDeDesconto, setembro.ValorDeTabela, setembro.Itens, setembro.Documento)
            .Should().Be((3m, 150m, 5m, 160m, 2, "11444777000161"));

        leitura.Orcamentos.Select(o => o.Numero).Should().BeEquivalentTo(["000777", "000700"], "o encerrado de 2021 está fora; o aberto entra de qualquer data");
        var aberto = leitura.Orcamentos.Single(o => o.Numero == "000777");
        (aberto.OrcadoEm, aberto.ValidoAte, aberto.Documento, aberto.ValorTotal).Should().Be(
            ((DateOnly?)new DateOnly(2026, 9, 20), (DateOnly?)new DateOnly(2026, 10, 20), "11444777000161", (decimal?)300m));
    }

    [FatoSeHouverSqlServer]
    public async Task A_leitura_curta_traz_o_orcamento_antigo_alterado_ha_pouco_so_quando_pede_os_alterados()
    {
        RecriarBancoComAsViews();

        Executar("""
            INSERT INTO dbo.X_V_BI_POSICAO_ORC_PECAS (filial_orc, nro_orc, STATUS_ORC, Situacao_orc, status_reserva_orc, tipo_atendimento_orc,
                tipo_orcamento_orc, cpf_cnpj_orc, [data_orçamento_orc], data_validade_orc, data_alteracao_orc, cod_vededor_orc, nome_vededor_orc,
                cod_item_orc, vlr_total_orc, vlr_desc_orc, vlr_margem_lucro_orc) VALUES
                -- Orçado em 2024, encerrado e alterado em setembro de 2026: só a leitura curta, pelos alterados, o traz.
                ('01', '000500', 'Encerrado', 'ATENDIDO', 'RESERVADO', 'BALCAO', 'VENDA', '11444777000161', '2024-01-10', '2024-02-10', '2026-09-20', '000123', 'VENDEDOR A', 'PECA-5', 80, 0, 1),
                -- Orçado e alterado em 2024: fora das duas.
                ('01', '000501', 'Encerrado', 'ATENDIDO', 'RESERVADO', 'BALCAO', 'VENDA', '11444777000161', '2024-01-10', '2024-02-10', '2024-01-11', '000123', 'VENDEDOR A', 'PECA-6', 90, 0, 1);
            """);

        var leitor = new LeitorDasPecasDoProtheus(Opcoes());
        var curta = (await leitor.LerAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 1), CancellationToken.None)).Valor;
        var semOsAlterados = (await leitor.LerAsync(new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 1), null, CancellationToken.None)).Valor;

        curta.Orcamentos.Select(o => o.Numero).Should().BeEquivalentTo(["000500"], "alterado em 20/09/2026, depois do início curto");
        semOsAlterados.Orcamentos.Should().BeEmpty("sem os alterados, a consulta é a de sempre: orçado na janela ou aberto");
    }

    // -----------------------------------------------------------------------------------------

    private static OpcoesDoBancoDoProtheus Opcoes()
    {
        var cadeia = new SqlConnectionStringBuilder(ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste));
        return new OpcoesDoBancoDoProtheus { Servidor = cadeia.DataSource, Banco = cadeia.InitialCatalog, Usuario = cadeia.UserID, Senha = cadeia.Password };
    }

    /// <summary>As duas "views" como tabelas com o mesmo nome — a data do faturamento como texto <c>AAAAMMDD</c>, a do orçamento como data.</summary>
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
            CREATE TABLE dbo.X_V_BI_FATURAMENTO_PECAS (
                filial varchar(2) NULL, data_emissao_nf varchar(8) NULL, seq_pessoa varchar(14) NULL, setor varchar(30) NULL, origem varchar(20) NULL,
                nome_linha_produto varchar(40) NULL, cod_familia_produto varchar(6) NULL, id_vendedor varchar(6) NULL, nome_vendedor varchar(40) NULL,
                tp_cortesia varchar(3) NULL, tp_ordem varchar(4) NULL, nome_operacao varchar(40) NULL, qtde numeric(12, 2) NULL,
                vlr_liquido_item numeric(14, 2) NULL, vlr_desconto numeric(14, 2) NULL, vlr_tabela numeric(14, 2) NULL, custo_medio numeric(14, 2) NULL);
            CREATE TABLE dbo.X_V_BI_POSICAO_ORC_PECAS (
                filial_orc varchar(2) NULL, nro_orc varchar(8) NULL, STATUS_ORC varchar(30) NULL, Situacao_orc varchar(20) NULL,
                status_reserva_orc varchar(30) NULL, tipo_atendimento_orc varchar(30) NULL, tipo_orcamento_orc varchar(30) NULL,
                cpf_cnpj_orc varchar(20) NULL, [data_orçamento_orc] date NULL, data_validade_orc date NULL, data_alteracao_orc date NULL,
                cod_vededor_orc varchar(6) NULL, nome_vededor_orc varchar(40) NULL, cod_item_orc varchar(30) NULL,
                vlr_total_orc numeric(14, 2) NULL, vlr_desc_orc numeric(14, 2) NULL, vlr_margem_lucro_orc numeric(14, 2) NULL);
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
