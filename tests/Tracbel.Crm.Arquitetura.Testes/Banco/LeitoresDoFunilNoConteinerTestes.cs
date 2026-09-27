using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// OS LEITORES DO FUNIL E DAS VENDAS PERDIDAS CONTRA UM SQL SERVER DE VERDADE — as consultas que rodam no Vórtice, com
/// as colunas nos tipos da origem (<c>numeric</c>, <c>decimal</c>, <c>datetime</c>, <c>varchar</c> com espaço, o
/// <c>SeqHistorico</c> do questionário em <c>float</c>).
///
/// <para><b>Por que no contêiner:</b> a estação não lê o Vórtice (o classificador pode negar), e o que pode quebrar
/// aqui é o SQL — a UNION dos doze formulários com os CASTs, o <c>OUTER APPLY</c> da carteira, o <c>MIN</c> do primeiro
/// andamento, o <c>JOIN</c> do histórico pelo <c>float</c>. As doze tabelas de resposta são criadas com o DDL da
/// extração do Vórtice (<c>docs/extracao-vortice/ddl/IV.sql</c>), tal como estão lá; as outras, só com as colunas lidas,
/// nos mesmos tipos.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class LeitoresDoFunilNoConteinerTestes
{
    /// <summary>Banco próprio deste teste, apagado e recriado a cada execução.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmVorticeFunilTeste";

    [FatoSeHouverSqlServer]
    public async Task O_funil_le_os_processos_31_41_e_50_com_carteira_primeiro_andamento_e_so_o_historico_que_importa()
    {
        RecriarBancoComAsTabelasDoVortice();

        Executar("""
            INSERT INTO dbo.IVS_Depto (SeqDepto, Depto) VALUES (2, 'MAQ-NOVOS'), (7, 'PECAS');
            INSERT INTO dbo.GE_Pessoa (SeqPessoa, FisicaJuridica, NroCGCCPF, DigCGCCPF) VALUES (777, 'F', 529982247, 25), (778, 'J', 0, 0);
            INSERT INTO dbo.IVS_Pes (SeqPessoa, SeqDepto, SeqPesDepto, SeqCarteira) VALUES (777, 7, 1, 5), (777, 2, 2, 18), (777, 2, 3, 19);

            INSERT INTO dbo.IV_ProcDado (Processo, NroEmpresa, ProcessoDNA, CodProcesso, SeqPessoa) VALUES
                (1001, 1, 1001, 41, 777), (1002, 3, 1001, 50, 778), (1003, 1, 1003, 12, 777), (1004, 1, NULL, 31, NULL);
            INSERT INTO dbo.IV_Processo (Processo, UsuResponsavel, DtaInclusao, Status, DtaRealizacao) VALUES
                (1001, 'MARIA.SOUZA         ', '2024-01-05 09:30', 'EM ABERTO', NULL),
                (1002, NULL, NULL, 'FINALIZADA', '2024-06-01 10:00'),
                (1003, NULL, '2024-01-05', NULL, NULL),
                (1004, NULL, '2024-02-01', NULL, NULL);

            INSERT INTO dbo.IV_Historico (SeqHistorico, SeqPessoa, Contato, NroEmpresa, AcaoGeradora, Resultado, DtaRealizacao, Processo, CodProcesso) VALUES
                (1, 777, 'NOME DE PESSOA', 1, 50, 250, '2024-01-10 08:00', 1001, 41),
                (2, 777, NULL, 1, 609, 9999, '2024-02-10 08:00', 1001, 41),     -- só a ação da etapa
                (3, 777, NULL, 1, NULL, 9999, '2024-01-06 08:00', 1001, 41),    -- nem uma nem outra: só conta no primeiro andamento
                (4, 778, NULL, 3, NULL, 3239, '2024-03-01 08:00', 1002, 50),
                (5, 777, NULL, 1, NULL, 3239, '2024-03-01 08:00', 1003, 12);    -- tipo fora do funil
            """);

        var leitura = await new LeitorDoFunilDoVortice(Opcoes()).LerAsync([250, 3239], CancellationToken.None);
        leitura.EhSucesso.Should().BeTrue(leitura.Erro ?? string.Empty);

        var processos = leitura.Valor.Processos.ToDictionary(p => p.Numero);
        processos.Keys.Should().BeEquivalentTo([1001L, 1002L, 1004L], "o 1003 é do tipo 12");

        processos[1001].Should().BeEquivalentTo(new
        {
            Tipo = (short)41,
            NumeroDoDna = 1001L,
            NroEmpresa = (int?)1,
            SeqCarteira = (int?)18,
            LoginDoResponsavel = "MARIA.SOUZA",
            IncluidoEmUtc = (DateTime?)new DateTime(2024, 1, 5, 12, 30, 0, DateTimeKind.Utc),
            PrimeiroAndamentoEmUtc = (DateTime?)new DateTime(2024, 1, 6, 11, 0, 0, DateTimeKind.Utc),
            Status = "EM ABERTO"
        }, o => o.ExcludingMissingMembers());
        processos[1001].Documento.Numero.Should().Be("52998224725");

        processos[1002].NumeroDoDna.Should().Be(1001, "o filho DNA aponta o pai");
        processos[1002].IncluidoEmUtc.Should().BeNull("a inclusão nula chega nula — o COALESCE é da regra");
        processos[1002].SeqCarteira.Should().BeNull();
        processos[1002].Documento.Situacao.Should().Be(SituacaoDoDocumentoNoVortice.Zerado);
        processos[1004].NumeroDoDna.Should().Be(1004, "ProcessoDNA nulo é o próprio processo");

        leitura.Valor.Historico.Select(h => (h.SeqHistorico, h.Resultado, h.AcaoGeradora)).Should().BeEquivalentTo(
            new[] { (1L, 250, (int?)50), (2L, 9999, (int?)609), (4L, 3239, (int?)null) },
            "só o resultado classificado ou a ação da etapa, e só dos tipos do funil");

        // E A REGRA RODA SOBRE A LEITURA: o filho herda a venda aprovada do pai? Não — é o pai que tem filho, e sai.
        var apuracoes = RegraDoEstagio.Apurar(
            leitura.Valor.Processos.Select(p => p.NaRegra), leitura.Valor.Historico.Select(h => h.NaRegra),
            new Dictionary<int, EstagioDoFunil> { [250] = EstagioDoFunil.Cobertura, [3239] = EstagioDoFunil.Pedido },
            new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc));
        apuracoes.Single(a => a.Numero == 1001).Situacao.Should().Be(SituacaoNaRegraDoEstagio.PaiSubstituidoPeloFilhoDna);
        apuracoes.Single(a => a.Numero == 1002).Estagios.Max(e => e.Estagio).Should().Be(EstagioDoFunil.Pedido);
    }

    [FatoSeHouverSqlServer]
    public async Task A_venda_perdida_le_os_doze_formularios_com_a_filial_do_processo_ou_a_do_historico()
    {
        RecriarBancoComAsTabelasDoVortice();

        Executar("""
            INSERT INTO dbo.GE_Pessoa (SeqPessoa, FisicaJuridica, NroCGCCPF, DigCGCCPF) VALUES (777, 'F', 529982247, 25);
            INSERT INTO dbo.IV_ProcDado (Processo, NroEmpresa, ProcessoDNA, CodProcesso, SeqPessoa) VALUES (5000, 13, 5000, 41, 777);
            INSERT INTO dbo.IV_Historico (SeqHistorico, SeqPessoa, NroEmpresa, Resultado, DtaRealizacao, Processo, CodProcesso) VALUES
                (900, 777, 3, 1, '2014-05-10 10:00', NULL, NULL);

            INSERT INTO dbo.IV_Questionario (SeqQuestionario, SeqPessoa, SeqFormulario, SeqHistorico, DtaRealizacao, UsuInclusao, Processo) VALUES
                (1, 777, 10, 900, '2014-05-10 10:00', 'ALGUEM', NULL),
                (2, 777, 11, NULL, '2025-09-01 10:00', 'ALGUEM', 5000),
                (3, 777, 12, NULL, '2025-09-01 11:00', 'ALGUEM', 5000),
                (4, 777, 13, NULL, '2025-09-01 12:00', 'ALGUEM', 5000),
                (5, 777, 14, 900, NULL, 'ALGUEM', NULL);

            INSERT INTO dbo.IV_Q_VENDA_PERDIDA (SEQQUESTIONARIO, MARCA_VP, REVENDA, MODELO_VP, DATA_DA_VENDA, QUANTIDADE, PRECO_CONCORRENTE,
                PRECO_JOHN_DEERE, MODELO_JOHN_DEER, MOTIVO, TIPO_DE_EQUIPAMENTO, PARTICIPAMOS_DA_NEGO)
                VALUES (1, 'NEW HOLLAND', 'REVENDA X', 'T7', '2014-05-01', 2, 500000, 480000, '7230J', 'PRECO', 'TRATOR', 'SIM');
            INSERT INTO dbo.IV_Q_VENDA_PERDIDA_FY25 (SEQQUESTIONARIO, VP_TIPO_EQUIP_CONCOR, VP_MARCA_EQUIP_CONCO, VP_QUANTIDADE, VP_MOTIVO_VP)
                VALUES (2, 'TRATOR', 'CASE', 1, 'PRAZO DE ENTREGA');
            INSERT INTO dbo.IV_Q_VP_SEM_PARTICIPACAO (SEQQUESTIONARIO, MARCA_CONCORRENTE) VALUES (3, 'CASE');
            INSERT INTO dbo.IV_Q_VP_TRATOR (SEQQUESTIONARIO, VP_MARCA_CONCORRENT, VP_QUANTIDADE_EQUIP, VP_PRECO_CONCORRENTE)
                VALUES (4, 'CASE', ' 02', 610000.50);
            INSERT INTO dbo.IV_Q_VENDA_PERDIDA_JDE (SEQQUESTIONARIO, MARCA_VP) VALUES (5, 'NEW HOLLAND');
            INSERT INTO dbo.IV_Q_VENDA_PERDIDA_MANITO (SEQQUESTIONARIO, MOD_EQUIP) VALUES (6, 'NAO LIDO');
            """);

        var leitura = await new LeitorDeVendasPerdidasDoVortice(Opcoes()).LerAsync(CancellationToken.None);
        leitura.EhSucesso.Should().BeTrue(leitura.Erro ?? string.Empty);

        var respostas = leitura.Valor.ToDictionary(r => r.Questionario);
        respostas.Keys.Should().BeEquivalentTo([1L, 2L, 3L, 4L, 5L], "o _MANITO não é lido");

        respostas[1].Should().BeEquivalentTo(new
        {
            Formulario = FormulariosDaVendaPerdida.Antigo,
            Processo = (long?)null,
            EmpresaDoProcesso = (int?)null,
            EmpresaDoHistorico = (int?)3,
            Marca = "NEW HOLLAND",
            Revenda = "REVENDA X",
            ModeloDoConcorrente = "T7",
            ModeloOfertado = "7230J",
            Quantidade = (decimal?)2m,
            PrecoDoConcorrente = (decimal?)500000m,
            PrecoOfertado = (decimal?)480000m,
            Motivo = "PRECO",
            TipoDoEquipamento = "TRATOR",
            Participamos = "SIM",
            OcorridaEm = (DateTime?)new DateTime(2014, 5, 1),
            RegistradaEmUtc = (DateTime?)new DateTime(2014, 5, 10, 13, 0, 0, DateTimeKind.Utc)
        }, o => o.ExcludingMissingMembers());
        respostas[1].Documento.Numero.Should().Be("52998224725");

        respostas[2].Should().BeEquivalentTo(new { Processo = (long?)5000, EmpresaDoProcesso = (int?)13, Motivo = "PRAZO DE ENTREGA" },
            o => o.ExcludingMissingMembers());
        respostas[3].Participamos.Should().Be("NAO", "o formulário SEM_PARTICIPACAO afirma que a Tracbel não participou");
        respostas[4].Quantidade.Should().Be(2m, "no VP_TRATOR a quantidade é texto");
        respostas[4].PrecoDoConcorrente.Should().Be(610000.50m);
        respostas[5].RegistradaEmUtc.Should().BeNull("a resposta sem data chega sem data — quem decide é a rotina");
    }

    // -----------------------------------------------------------------------------------------

    private static OpcoesDoVortice Opcoes() => new() { Conexao = ConexaoDeSqlServer.MontarPara(NomeDoBancoDeTeste) };

    /// <summary>Apaga e recria o banco de teste com as tabelas do Vórtice que os dois leitores tocam.</summary>
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

        // AS SÓ-COM-O-QUE-SE-LÊ, nos tipos do Vórtice (docs/extracao-vortice/ddl). A coluna de nomes do histórico
        // existe — é justamente ela que a leitura não pode tocar.
        Executar("""
            CREATE TABLE dbo.IV_ProcDado (Processo numeric(18,0) NOT NULL PRIMARY KEY, NroEmpresa numeric(6,0) NULL, ProcessoPai numeric(18,0) NULL,
                ProcessoDNA numeric(18,0) NULL, CodProcesso decimal(4,0) NULL, SeqPessoa decimal(10,0) NULL);
            CREATE TABLE dbo.IV_Processo (Processo numeric(18,0) NOT NULL PRIMARY KEY, UsuResponsavel varchar(20) NULL,
                DtaInclusao datetime NULL, Status varchar(20) NULL, DtaRealizacao datetime NULL);
            CREATE TABLE dbo.IV_Historico (SeqHistorico numeric(18,0) NOT NULL PRIMARY KEY, SeqPessoa numeric(10,0) NOT NULL,
                Contato varchar(20) NULL, NroEmpresa numeric(6,0) NULL, AcaoGeradora numeric(6,0) NULL, Resultado decimal(6,0) NOT NULL,
                DtaRealizacao datetime NOT NULL, Processo numeric(18,0) NULL, CodProcesso decimal(4,0) NULL);
            CREATE TABLE dbo.IV_Questionario (SeqQuestionario numeric(18,0) NOT NULL PRIMARY KEY, SeqPessoa numeric(8,0) NOT NULL,
                SeqFormulario numeric(18,0) NOT NULL, SeqHistorico float NULL, DtaRealizacao datetime NULL, UsuInclusao varchar(20) NULL,
                Processo decimal(15,0) NULL);
            CREATE TABLE dbo.GE_Pessoa (SeqPessoa numeric(10,0) NOT NULL PRIMARY KEY, FisicaJuridica char(1) NULL,
                NroCGCCPF decimal(13,0) NULL, DigCGCCPF decimal(2,0) NULL);
            CREATE TABLE dbo.IVS_Depto (SeqDepto decimal(4,0) NOT NULL PRIMARY KEY, Depto varchar(12) NULL);
            CREATE TABLE dbo.IVS_Pes (SeqPessoa numeric(10,0) NOT NULL, SeqDepto decimal(4,0) NOT NULL, SeqPesDepto numeric(10,0) NOT NULL,
                SeqCarteira decimal(4,0) NOT NULL, PRIMARY KEY (SeqPessoa, SeqDepto, SeqPesDepto));
            """);

        // AS DOZE DE RESPOSTA (e a do _MANITO, que existe lá e não é lida), com o DDL da extração, tal como está.
        var ddl = File.ReadAllText(Path.Combine(ArquiteturaTestes.LocalizarRaizDoRepositorio(), "docs", "extracao-vortice", "ddl", "IV.sql"));
        foreach (var tabela in FormulariosDaVendaPerdida.Todos.Append("IV_Q_VENDA_PERDIDA_MANITO"))
        {
            var criacao = Regex.Match(ddl, $@"CREATE TABLE \[dbo\]\.\[{tabela}\] \(.*?\n\)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            criacao.Success.Should().BeTrue($"o DDL de {tabela} está na extração");
            Executar(criacao.Value);
        }
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
