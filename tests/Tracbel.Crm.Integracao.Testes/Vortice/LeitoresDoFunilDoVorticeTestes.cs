using System.Data;
using System.Text.RegularExpressions;
using FluentAssertions;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Vortice;

/// <summary>
/// OS LEITORES DO FUNIL E DAS VENDAS PERDIDAS (documento 52). O que estes testes prendem, sem Vórtice nenhum:
/// <list type="bullet">
/// <item>nenhuma consulta pronuncia a coluna de nomes do histórico (<c>IV_HISTORICO.Contato</c>) nem quem preencheu o
/// formulário (<c>UsuInclusao</c>), e toda tabela é lida com <c>NOLOCK</c>;</item>
/// <item>o funil é dos tipos 31/41/50, e o histórico traz os resultados da classificação e as ações da etapa;</item>
/// <item>cada coluna que o mapeamento dos doze formulários cita existe na tabela, no DDL da extração do Vórtice;</item>
/// <item>a tradução das linhas: número de qualquer tipo, data para UTC, documento recomposto, texto vazio como nulo, a
/// quantidade escrita como texto nos <c>VP_*</c>.</item>
/// </list>
/// </summary>
public sealed class LeitoresDoFunilDoVorticeTestes
{
    private static readonly string ConsultaDoHistorico = LeitorDoFunilDoVortice.ConsultaDoHistorico([3239, 250, 1278, 250]);

    private static IEnumerable<string> TodasAsConsultas =>
        [LeitorDoFunilDoVortice.ConsultaDosProcessos, ConsultaDoHistorico, LeitorDeVendasPerdidasDoVortice.Consulta];

    [Fact]
    public void Nenhuma_consulta_pronuncia_a_coluna_de_nomes_do_historico_nem_quem_preencheu()
    {
        foreach (var consulta in TodasAsConsultas)
        {
            Regex.IsMatch(consulta, @"\bContato\b", RegexOptions.IgnoreCase).Should().BeFalse("IV_HISTORICO.Contato guarda NOMES de pessoas");
            consulta.Should().NotContainEquivalentOf("UsuInclusao", "quem preencheu é login de pessoa, e a pergunta da perda não precisa dele");
            consulta.Should().NotContainEquivalentOf("Detalhe", "o detalhe do histórico é texto livre");
            consulta.Should().NotContain("*", "as colunas são nomeadas uma a uma — nenhuma entra por acidente");
        }
    }

    [Fact]
    public void Toda_tabela_lida_do_Vortice_e_lida_com_NOLOCK()
    {
        foreach (var consulta in TodasAsConsultas)
        {
            var semNolock = Regex.Matches(consulta, @"(?:FROM|JOIN)\s+(\w+)\s+(\w+)(?!\s+WITH \(NOLOCK\))", RegexOptions.IgnoreCase)
                .Select(m => m.Groups[1].Value)
                .Where(t => t is not "Respostas")
                .Where(t => !Regex.IsMatch(consulta, $@"\b{t}\s+\w+\s+WITH \(NOLOCK\)", RegexOptions.IgnoreCase))
                .ToList();

            semNolock.Should().BeEmpty("o Vórtice é um sistema em produção, e a leitura não segura trava nele");
        }
    }

    [Fact]
    public void O_funil_e_dos_tipos_31_41_e_50_e_o_historico_traz_a_classificacao_e_as_acoes_da_etapa()
    {
        LeitorDoFunilDoVortice.ConsultaDosProcessos.Should().Contain("WHERE d.CodProcesso IN (31, 41, 50)");
        LeitorDoFunilDoVortice.ConsultaDosProcessos.Should().Contain("t.Depto = @depto", "a carteira é a do departamento MAQ-NOVOS");

        ConsultaDoHistorico.Should().Contain("h.CodProcesso IN (31, 41, 50)");
        ConsultaDoHistorico.Should().Contain("h.Resultado IN (250, 1278, 3239)", "sem repetição, em ordem");
        ConsultaDoHistorico.Should().Contain("h.AcaoGeradora IN (50, 54, 597, 608, 609, 614, 767, 768, 769, 808, 823, 841)");

        FluentActions.Invoking(() => LeitorDoFunilDoVortice.ConsultaDoHistorico([])).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void A_venda_perdida_le_os_doze_formularios_e_nao_le_o_MANITO()
    {
        var consulta = LeitorDeVendasPerdidasDoVortice.Consulta;

        LeitorDeVendasPerdidasDoVortice.Formularios.Select(f => f.Tabela).Should().Equal(FormulariosDaVendaPerdida.Todos);
        foreach (var formulario in FormulariosDaVendaPerdida.Todos)
            consulta.Should().Contain($"FROM {formulario} v WITH (NOLOCK)");

        Regex.Matches(consulta, "UNION ALL").Count.Should().Be(11);
        consulta.Should().NotContain("MANITO", "o formulário da Colorado fica fora — decisão de 27/09/2026");
        consulta.Should().Contain("CAST('NAO' AS varchar(3)) AS Participamos", "o SEM_PARTICIPACAO afirma que não participamos");
    }

    [Fact]
    public void Cada_coluna_que_o_mapeamento_cita_existe_na_tabela_do_DDL_do_Vortice()
    {
        var ddl = File.ReadAllText(Path.Combine(RaizDoRepositorio(), "docs", "extracao-vortice", "ddl", "IV.sql"));
        var faltando = new List<string>();

        foreach (var formulario in LeitorDeVendasPerdidasDoVortice.Formularios)
        {
            var tabela = Regex.Match(ddl, $@"CREATE TABLE \[dbo\]\.\[{formulario.Tabela}\] \((.*?)\n\)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            tabela.Success.Should().BeTrue($"a tabela {formulario.Tabela} existe no Vórtice");

            var colunas = Regex.Matches(tabela.Groups[1].Value, @"^\s*\[(\w+)\]", RegexOptions.Multiline)
                .Select(m => m.Groups[1].Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            colunas.Should().Contain("SEQQUESTIONARIO");

            faltando.AddRange(formulario.Colunas.Where(c => !colunas.Contains(c)).Select(c => $"{formulario.Tabela}.{c}"));
        }

        faltando.Should().BeEmpty("o nome de coluna errado só apareceria na primeira rodada no servidor");
    }

    [Fact]
    public void A_linha_do_processo_e_traduzida_com_data_em_UTC_documento_recomposto_e_DNA_proprio_quando_nulo()
    {
        var tabela = new DataTable();
        foreach (var (nome, tipo) in new[]
                 {
                     ("Processo", typeof(decimal)), ("CodProcesso", typeof(decimal)), ("ProcessoDNA", typeof(decimal)),
                     ("NroEmpresa", typeof(decimal)), ("SeqPessoa", typeof(decimal)), ("FisicaJuridica", typeof(string)),
                     ("NroCGCCPF", typeof(decimal)), ("DigCGCCPF", typeof(decimal)), ("SeqCarteira", typeof(decimal)),
                     ("UsuResponsavel", typeof(string)), ("DtaInclusao", typeof(DateTime)), ("Status", typeof(string)),
                     ("DtaRealizacao", typeof(DateTime)), ("PrimeiroAndamento", typeof(DateTime))
                 })
            tabela.Columns.Add(nome, tipo);

        // CPF 529.982.247-25: a base numérica sem o zero à esquerda não se aplica aqui, mas o dígito vem à parte.
        tabela.Rows.Add(123456m, 41m, DBNull.Value, 13m, 777m, "F", 529982247m, 25m, 18m, "  MARIA.SOUZA ",
            new DateTime(2024, 1, 5, 9, 30, 0), "   ", DBNull.Value, new DateTime(2024, 1, 6, 10, 0, 0));

        using var leitor = tabela.CreateDataReader();
        leitor.Read().Should().BeTrue();
        var processo = LeitorDoFunilDoVortice.MapearProcesso(leitor);

        processo.Should().BeEquivalentTo(new
        {
            Numero = 123456L,
            Tipo = (short)41,
            NumeroDoDna = 123456L,
            NroEmpresa = (int?)13,
            SeqPessoa = (long?)777,
            SeqCarteira = (int?)18,
            LoginDoResponsavel = "MARIA.SOUZA",
            IncluidoEmUtc = (DateTime?)new DateTime(2024, 1, 5, 12, 30, 0, DateTimeKind.Utc),
            PrimeiroAndamentoEmUtc = (DateTime?)new DateTime(2024, 1, 6, 13, 0, 0, DateTimeKind.Utc),
            Status = (string?)null,
            RealizadoEmUtc = (DateTime?)null
        }, o => o.ExcludingMissingMembers());
        processo.Documento.Numero.Should().Be("52998224725");
        processo.NaRegra.Should().Be(new ProcessoNaRegraDoEstagio(123456, 41, 123456, processo.IncluidoEmUtc, processo.PrimeiroAndamentoEmUtc));
    }

    [Fact]
    public void A_linha_do_historico_e_traduzida()
    {
        var tabela = new DataTable();
        tabela.Columns.Add("SeqHistorico", typeof(decimal));
        tabela.Columns.Add("Processo", typeof(decimal));
        tabela.Columns.Add("Resultado", typeof(decimal));
        tabela.Columns.Add("DtaRealizacao", typeof(DateTime));
        tabela.Columns.Add("AcaoGeradora", typeof(decimal));
        tabela.Rows.Add(9m, 123456m, 3239m, new DateTime(2024, 3, 1, 10, 0, 0), DBNull.Value);

        using var leitor = tabela.CreateDataReader();
        leitor.Read();
        LeitorDoFunilDoVortice.MapearHistorico(leitor).Should().Be(
            new LinhaDoHistoricoDoFunil(9, 123456, 3239, new DateTime(2024, 3, 1, 13, 0, 0, DateTimeKind.Utc), null));
    }

    [Fact]
    public void A_resposta_da_venda_perdida_e_traduzida_e_o_processo_zero_e_nenhum()
    {
        var tabela = new DataTable();
        foreach (var nome in new[] { "Formulario", "TipoDoEquipamento", "Marca", "ModeloDoConcorrente", "Revenda", "ModeloOfertado",
                     "Quantidade", "Motivo", "Participamos", "FisicaJuridica" })
            tabela.Columns.Add(nome, typeof(string));
        foreach (var nome in new[] { "Questionario", "PrecoDoConcorrente", "PrecoOfertado", "Processo", "SeqPessoa", "EmpresaDoProcesso",
                     "EmpresaDoHistorico", "NroCGCCPF", "DigCGCCPF" })
            tabela.Columns.Add(nome, typeof(decimal));
        tabela.Columns.Add("OcorridaEm", typeof(DateTime));
        tabela.Columns.Add("DtaRealizacao", typeof(DateTime));

        var linha = tabela.NewRow();
        linha["Questionario"] = 5001m;
        linha["Formulario"] = FormulariosDaVendaPerdida.VpTrator;
        linha["Marca"] = " NEW HOLLAND ";
        linha["Quantidade"] = "02";
        linha["PrecoDoConcorrente"] = 610000.5m;
        linha["Processo"] = 0m;
        linha["SeqPessoa"] = 777m;
        linha["EmpresaDoHistorico"] = 3m;
        linha["DtaRealizacao"] = new DateTime(2025, 10, 1, 9, 0, 0);
        tabela.Rows.Add(linha);

        using var leitor = tabela.CreateDataReader();
        leitor.Read();
        var resposta = LeitorDeVendasPerdidasDoVortice.Mapear(leitor);

        resposta.Should().BeEquivalentTo(new
        {
            Questionario = 5001L,
            Formulario = FormulariosDaVendaPerdida.VpTrator,
            Marca = "NEW HOLLAND",
            Quantidade = (decimal?)2m,
            PrecoDoConcorrente = (decimal?)610000.5m,
            Processo = (long?)null,
            EmpresaDoProcesso = (int?)null,
            EmpresaDoHistorico = (int?)3,
            RegistradaEmUtc = (DateTime?)new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc),
            Motivo = (string?)null
        }, o => o.ExcludingMissingMembers());

        LeitorDeVendasPerdidasDoVortice.Quantidade("2,0").Should().Be(2m);
        LeitorDeVendasPerdidasDoVortice.Quantidade("duas").Should().BeNull("o que não é número é 'não declarada'");
    }

    private static string RaizDoRepositorio()
    {
        var atual = new DirectoryInfo(AppContext.BaseDirectory);
        while (atual is not null && !atual.EnumerateFiles("*.sln").Any()) atual = atual.Parent;
        return atual?.FullName ?? throw new InvalidOperationException("Não encontrei a raiz do repositório (nenhum .sln acima).");
    }
}
