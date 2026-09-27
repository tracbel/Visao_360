using FluentAssertions;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// O LEITOR DA ONDA 2 CONTRA UM SQL SERVER DE VERDADE (documento 52 §12) — as consultas que rodam no Vórtice, com as
/// colunas nos tipos da origem (<c>numeric</c>, <c>decimal</c>, <c>datetime</c>, <c>char</c>, <c>varchar</c> com espaço).
///
/// <para><b>O que pode quebrar aqui é o SQL:</b> a janela pela inclusão OU pelo primeiro andamento, a agenda pelo processo
/// (a avulsa fica fora), a conclusão procurada nos dois sentidos (o <c>CTE</c> e o <c>COALESCE</c> do ponteiro de ida), o
/// <c>CASE</c> sobre quem incluiu com os nomes em parâmetro, e os catálogos por lista. As tabelas têm também as colunas de
/// texto livre — é justamente elas que a leitura não pode tocar (decisão P1).</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class LeitorDasOportunidadesNoConteinerTestes
{
    /// <summary>Banco próprio deste teste, apagado e recriado a cada execução.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmVorticeOportunidadesTeste";

    [FatoSeHouverSqlServer]
    public async Task A_onda_2_le_os_processos_da_janela_a_agenda_pelo_processo_e_o_historico_sem_texto_livre()
    {
        RecriarBancoComAsTabelasDoVortice();

        Executar("""
            INSERT INTO dbo.IVS_Depto (SeqDepto, Depto) VALUES (2, 'MAQ-NOVOS');
            INSERT INTO dbo.GE_Pessoa (SeqPessoa, FisicaJuridica, NroCGCCPF, DigCGCCPF) VALUES (777, 'F', 529982247, 25);
            INSERT INTO dbo.IVS_Pes (SeqPessoa, SeqDepto, SeqPesDepto, SeqCarteira) VALUES (777, 2, 1, 18);

            INSERT INTO dbo.IV_ProcDado (Processo, NroEmpresa, CodProcesso, SeqPessoa) VALUES
                (1001, 1, 41, 777), (1002, 1, 31, 777), (1003, 1, 41, 777), (1004, 1, 12, 777);
            INSERT INTO dbo.IV_Processo (Processo, UsuResponsavel, DtaInclusao, Status, Fase, FaseOrdem, DtaFase, Resumo, Descricao, Valor, Qtde,
                DtaPrevConclusao) VALUES
                (1001, 'MARIA.SOUZA         ', '2024-01-05 09:30', 'EM ABERTO', 'NEGOCIACAO', 3, '2024-02-01', 'Trator 6M', 'TEXTO LIVRE', 480000.50, 1,
                 '2024-06-30'),
                (1002, NULL, '2022-05-01', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),  -- inclusão antiga, andamento na janela
                (1003, NULL, '2022-05-01', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL),  -- tudo antes da janela
                (1004, NULL, '2024-01-05', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);  -- tipo fora do funil

            INSERT INTO dbo.IV_Historico (SeqHistorico, SeqPessoa, Contato, NroEmpresa, SeqUsuario, AcaoGeradora, Resultado, AgendaOrigem,
                DtaRealizacao, Detalhe, Natureza, Processo, CodProcesso, USUINCLUSAO) VALUES
                (500, 777, 'NOME DE PESSOA', 1, 7, 50, 3239, 90, '2024-03-02 10:00', 'TEXTO LIVRE', 'A', 1001, 41, 'MARIA.SOUZA'),
                (501, 777, NULL, 1, 9, NULL, 250, NULL, '2024-03-03 10:00', NULL, 'A', 1001, 41, 'VRTCSERVER'),
                (502, 777, NULL, 1, 9, NULL, 250, NULL, '2024-02-01 10:00', NULL, 'R', 1002, 31, 'JOAO'),
                (503, 777, NULL, 1, 9, NULL, 250, NULL, '2023-10-01 10:00', NULL, 'A', 1001, 41, 'JOAO'),   -- antes da janela
                (504, 777, NULL, 1, 9, NULL, 250, NULL, '2024-03-01 10:00', NULL, 'A', NULL, NULL, 'JOAO');  -- sem processo

            INSERT INTO dbo.IV_Agenda (SeqAgenda, SeqUsuario, Acao, Assunto, Detalhe, DtaAgenda, Prioridade, HistoricoOrigem, TipoAgendamento,
                Realizada, DtaRealizacao, UltResultado, UltHistorico, Processo) VALUES
                (90, 7, 609, 'ASSUNTO LIVRE', 'DETALHE LIVRE', '2024-03-01 09:00', 2, 0, 'A', 'S', NULL, 0, 0, 1001),
                (91, 9, 609, NULL, NULL, '2024-03-05 09:00', 3, 501, 'M', 'N', NULL, 0, 0, 1003),
                (92, 9, 609, NULL, NULL, '2024-03-05 09:00', 3, 0, 'M', 'N', NULL, 0, 0, NULL),    -- avulsa
                (93, 9, 609, NULL, NULL, '2023-10-01 09:00', 3, 0, 'M', 'N', NULL, 0, 0, 1001);    -- antes da janela

            INSERT INTO dbo.IV_CodProcesso (CodProcesso, Descricao, DescrRed, EmUso) VALUES
                (31, 'PROSPECCAO', 'PROSP', 1), (41, 'VENDA DE MAQUINAS', 'VENDA', 1), (50, 'VENDA DE EQUIPAMENTO', 'EQUIP', 0), (12, 'OUTRO', 'OUTRO', 1);
            INSERT INTO dbo.IV_Acao (Acao, Descricao, DescReduzida, EmUso, PrazoRealizacao) VALUES
                (50, 'VISITA', 'VISITA', 'S', 0), (609, 'NEGOCIACAO', 'NEGOC', 'S', 2), (700, 'NAO USADA', 'NAO', 'S', 0);
            INSERT INTO dbo.IV_Resultado (Resultado, Acao, Descricao, DescReduzida) VALUES
                (3239, 609, 'VENDA APROVADA', 'VENDA APROV'), (250, 50, 'VISITA REALIZADA', 'VISITA OK');
            INSERT INTO dbo.IV_ProcResultado (CodProcesso, Resultado, Fase, Status, FaseSeguinte) VALUES
                (41, 3239, NULL, 'VENDA REALIZADA', 'PEDIDO'), (12, 250, NULL, 'CANCELADO', NULL);
            INSERT INTO dbo.GE_Usuario (SeqUsuario, CodUsuario) VALUES (7, 'MARIA.SOUZA'), (9, 'JOAO'), (11, 'NINGUEM');
            """);

        var leitura = await new LeitorDasOportunidadesDoVortice(new OpcoesDoVortice { Conexao = ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste) })
            .LerAsync(CancellationToken.None);
        leitura.EhSucesso.Should().BeTrue(leitura.Erro ?? string.Empty);
        var l = leitura.Valor;

        l.Processos.Select(p => p.Numero).Should().BeEquivalentTo([1001L, 1002L], "o 1003 é todo de antes da janela e o 1004 é do tipo 12");
        var p1001 = l.Processos.Single(p => p.Numero == 1001);
        p1001.Should().BeEquivalentTo(new
        {
            SeqCarteira = (int?)18, LoginDoResponsavel = "MARIA.SOUZA", Resumo = "Trator 6M", Fase = "NEGOCIACAO", FaseOrdem = (int?)3,
            Valor = (decimal?)480000.50m, PrevisaoConclusao = (DateOnly?)new DateOnly(2024, 6, 30)
        }, o => o.ExcludingMissingMembers());
        p1001.Documento.Numero.Should().Be("52998224725");

        l.Tarefas.Select(t => t.SeqAgenda).Should().BeEquivalentTo([90L, 91L], "a avulsa e a de antes da janela ficam fora");
        l.Tarefas.Single(t => t.SeqAgenda == 90).Should().BeEquivalentTo(new
        {
            Resultado = (int?)3239, SeqHistoricoDeConclusao = (long?)500, SeqUsuarioQueConcluiu = (long?)7,
            RealizadaEmUtc = (DateTime?)new DateTime(2024, 3, 2, 13, 0, 0, DateTimeKind.Utc)
        }, o => o.ExcludingMissingMembers(), "a conclusão vem pelo ponteiro de volta (AgendaOrigem) quando o de ida é zero");
        l.Tarefas.Single(t => t.SeqAgenda == 91).SeqHistoricoDeOrigem.Should().Be(501);

        l.Interacoes.Select(i => i.SeqHistorico).Should().BeEquivalentTo([500L, 501L, 502L]);
        l.Interacoes.Single(i => i.SeqHistorico == 501).DeSistema.Should().BeTrue("VRTCSERVER é autor de sistema");
        l.Interacoes.Single(i => i.SeqHistorico == 500).DeSistema.Should().BeFalse();

        l.TiposDeProcesso.Select(t => t.Codigo).Should().BeEquivalentTo([31, 41, 50]);
        l.Resultados.Select(r => (r.Codigo, r.Acao)).Should().BeEquivalentTo([(3239, (int?)609), (250, (int?)50)]);
        l.Acoes.Select(a => a.Codigo).Should().BeEquivalentTo([50, 609], "só as ações usadas");
        l.Efeitos.Should().ContainSingle().Which.Should().Be(new EfeitoDoResultadoNoVortice(3239, true, "VENDA REALIZADA"),
            "o efeito é o dos tipos 31/41/50 — o do tipo 12 não conta");
        l.Logins.Select(u => (u.SeqUsuario, u.Login)).Should().BeEquivalentTo([(7L, "MARIA.SOUZA"), (9L, "JOAO")]);
    }

    // -----------------------------------------------------------------------------------------

    /// <summary>Apaga e recria o banco de teste com as tabelas do Vórtice que o leitor toca, nos tipos da origem.</summary>
    private static void RecriarBancoComAsTabelasDoVortice()
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

        // AS COLUNAS LIDAS, nos tipos do Vórtice (docs/extracao-vortice/ddl) — e as de texto livre, que a leitura não toca.
        Executar("""
            CREATE TABLE dbo.IV_ProcDado (Processo numeric(18,0) NOT NULL PRIMARY KEY, NroEmpresa numeric(6,0) NULL,
                CodProcesso decimal(4,0) NULL, SeqPessoa decimal(10,0) NULL);
            CREATE TABLE dbo.IV_Processo (Processo numeric(18,0) NOT NULL PRIMARY KEY, Resumo varchar(100) NULL, Descricao varchar(4000) NULL,
                UsuResponsavel varchar(20) NULL, DtaInclusao datetime NULL, DtaRealizacao datetime NULL, DtaFase datetime NULL,
                DtaStatus datetime NULL, Fase varchar(20) NULL, FaseOrdem decimal(2,0) NULL, Status varchar(20) NULL,
                DtaPrevConclusao datetime NULL, Valor decimal(15,2) NULL, Qtde decimal(10,2) NULL, DtaPrevConcOrig datetime NULL);
            CREATE TABLE dbo.IV_Historico (SeqHistorico numeric(18,0) NOT NULL PRIMARY KEY, SeqPessoa numeric(10,0) NOT NULL,
                Contato varchar(20) NULL, NroEmpresa numeric(6,0) NULL, SeqUsuario int NULL, AcaoGeradora numeric(6,0) NULL,
                Resultado decimal(6,0) NOT NULL, AgendaOrigem int NULL, DtaRealizacao datetime NOT NULL, Detalhe varchar(4000) NULL,
                Natureza char(1) NULL, Processo numeric(18,0) NULL, CodProcesso decimal(4,0) NULL, Duracao decimal(4,0) NULL,
                Latitude numeric(14,11) NULL, Longitude numeric(14,11) NULL, USUINCLUSAO varchar(20) NULL);
            CREATE TABLE dbo.IV_Agenda (SeqAgenda numeric(18,0) NOT NULL PRIMARY KEY, SeqUsuario numeric(18,0) NULL, Acao decimal(6,0) NULL,
                Assunto varchar(50) NULL, DtaAgenda datetime NULL, DtaLimiteExecucao datetime NULL, Prioridade decimal(1,0) NULL,
                Detalhe varchar(250) NULL, HistoricoOrigem numeric(18,0) NULL, DtaGeracao datetime NULL, TipoAgendamento char(1) NULL,
                Realizada char(1) NULL, DtaRealizacao datetime NULL, UltResultado numeric(18,0) NULL, UltHistorico numeric(18,0) NULL,
                Processo numeric(18,0) NULL);
            CREATE TABLE dbo.GE_Pessoa (SeqPessoa numeric(10,0) NOT NULL PRIMARY KEY, FisicaJuridica char(1) NULL,
                NroCGCCPF decimal(13,0) NULL, DigCGCCPF decimal(2,0) NULL);
            CREATE TABLE dbo.IVS_Depto (SeqDepto decimal(4,0) NOT NULL PRIMARY KEY, Depto varchar(12) NULL);
            CREATE TABLE dbo.IVS_Pes (SeqPessoa numeric(10,0) NOT NULL, SeqDepto decimal(4,0) NOT NULL, SeqPesDepto numeric(10,0) NOT NULL,
                SeqCarteira decimal(4,0) NOT NULL, PRIMARY KEY (SeqPessoa, SeqDepto, SeqPesDepto));
            CREATE TABLE dbo.IV_CodProcesso (CodProcesso decimal(4,0) NOT NULL PRIMARY KEY, Descricao varchar(30) NULL,
                DescrRed varchar(15) NULL, EmUso numeric(1,0) NULL);
            CREATE TABLE dbo.IV_Acao (Acao decimal(6,0) NOT NULL PRIMARY KEY, DescReduzida varchar(20) NULL, Descricao varchar(40) NULL,
                EmUso char(1) NULL, PrazoRealizacao decimal(6,0) NULL);
            CREATE TABLE dbo.IV_Resultado (Resultado decimal(6,0) NOT NULL PRIMARY KEY, Acao decimal(6,0) NULL,
                DescReduzida varchar(20) NULL, Descricao varchar(40) NULL);
            CREATE TABLE dbo.IV_ProcResultado (CodProcesso decimal(4,0) NOT NULL, Resultado decimal(6,0) NOT NULL, Fase varchar(20) NULL,
                Status varchar(20) NULL, FaseSeguinte varchar(20) NULL);
            CREATE TABLE dbo.GE_Usuario (SeqUsuario numeric(18,0) NOT NULL PRIMARY KEY, CodUsuario varchar(20) NOT NULL);
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
