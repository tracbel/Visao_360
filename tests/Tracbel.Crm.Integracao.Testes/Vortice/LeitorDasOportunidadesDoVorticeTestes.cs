using System.Data;
using System.Text.RegularExpressions;
using FluentAssertions;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Vortice;

/// <summary>
/// O LEITOR DA ONDA 2 (documento 52 §12). O que estes testes prendem, sem Vórtice nenhum:
/// <list type="bullet">
/// <item>SÓ O RESUMO ENTRA (decisão P1): nenhuma consulta pronuncia a <c>Descricao</c> do processo, o <c>Assunto</c>, o
/// <c>AssuntoCmpl</c>, o <c>Detalhe</c>, o <c>ResultadoCmpl</c>, nem as colunas de nome de pessoa (<c>Contato</c>,
/// <c>Vendedor</c>) — e o <c>USUINCLUSAO</c> só aparece dentro do <c>CASE</c> que diz se a linha é do sistema;</item>
/// <item>toda tabela é lida com <c>NOLOCK</c>, e cada coluna citada existe na tabela, no DDL da extração do Vórtice;</item>
/// <item>a janela (01/11/2023) e os tipos 31/41/50 estão em processo, agenda e histórico; a agenda é lida pelo processo;</item>
/// <item>a tradução das linhas: data para UTC, código zero como "não há", documento recomposto.</item>
/// </list>
/// </summary>
public sealed class LeitorDasOportunidadesDoVorticeTestes
{
    private static readonly string[] ConsultasDoProcesso =
        [LeitorDasOportunidadesDoVortice.ConsultaDosProcessos, LeitorDasOportunidadesDoVortice.ConsultaDaAgenda, LeitorDasOportunidadesDoVortice.ConsultaDoHistorico];

    private static IEnumerable<string> TodasAsConsultas => ConsultasDoProcesso.Concat(
    [
        LeitorDasOportunidadesDoVortice.ConsultaDosTiposDeProcesso, LeitorDasOportunidadesDoVortice.ConsultaDasAcoes([609, 50]),
        LeitorDasOportunidadesDoVortice.ConsultaDosResultados([3239]), LeitorDasOportunidadesDoVortice.ConsultaDosEfeitos([3239]),
        LeitorDasOportunidadesDoVortice.ConsultaDosLogins([7L, 9L])
    ]);

    /// <summary>As expressões de tabela da própria consulta — não são tabelas do Vórtice.</summary>
    private static readonly HashSet<string> Cte = new(StringComparer.OrdinalIgnoreCase) { "Conclusao" };

    [Fact]
    public void So_o_resumo_entra_e_nenhuma_consulta_pronuncia_texto_livre_nem_nome_de_pessoa()
    {
        foreach (var consulta in TodasAsConsultas)
        {
            foreach (var coluna in new[] { "Assunto", "AssuntoCmpl", "Detalhe", "ResultadoCmpl", "StatusDesc", "Contato", "Vendedor" })
                Regex.IsMatch(consulta, $@"\b{coluna}\b", RegexOptions.IgnoreCase).Should().BeFalse($"{coluna} é texto livre ou nome de pessoa (decisão P1)");

            consulta.Should().NotContain("*", "as colunas são nomeadas uma a uma — nenhuma entra por acidente");
        }

        foreach (var consulta in ConsultasDoProcesso)
            Regex.IsMatch(consulta, @"\bDescricao\b", RegexOptions.IgnoreCase).Should().BeFalse("a descrição do processo não entra (decisão P1)");

        LeitorDasOportunidadesDoVortice.ConsultaDosProcessos.Should().Contain("p.Resumo", "o resumo é o único texto livre que entra, como título");

        // O NOME DE QUEM INCLUIU só pode aparecer dentro do CASE que responde "foi o sistema?", com os nomes em parâmetro.
        foreach (var consulta in TodasAsConsultas)
        {
            var mencoes = Regex.Matches(consulta, "USUINCLUSAO", RegexOptions.IgnoreCase).Count;
            mencoes.Should().Be(consulta == LeitorDasOportunidadesDoVortice.ConsultaDoHistorico ? 1 : 0);
        }

        LeitorDasOportunidadesDoVortice.ConsultaDoHistorico.Should()
            .Contain("CASE WHEN LTRIM(RTRIM(h.USUINCLUSAO)) IN (@autor0, @autor1, @autor2, @autor3, @autor4) THEN 1 ELSE 0 END AS DeSistema");
        LeitorDasOportunidadesDoVortice.AutoresDeSistema.Should().HaveCount(5);

        // O LOGIN SAI SÓ NA CONSULTA POR LISTA DE USUÁRIOS, para achar a conta — em nenhuma outra.
        foreach (var consulta in ConsultasDoProcesso)
            consulta.Should().NotContainEquivalentOf("CodUsuario");
    }

    [Fact]
    public void Toda_tabela_lida_do_Vortice_e_lida_com_NOLOCK()
    {
        foreach (var consulta in TodasAsConsultas)
        {
            var semNolock = Regex.Matches(consulta, @"(?:FROM|JOIN)\s+(\w+)\s+(\w+)(?!\s+WITH \(NOLOCK\))", RegexOptions.IgnoreCase)
                .Select(m => m.Groups[1].Value)
                .Where(t => !Cte.Contains(t))
                .Where(t => !Regex.IsMatch(consulta, $@"\b{t}\s+\w+\s+WITH \(NOLOCK\)", RegexOptions.IgnoreCase))
                .ToList();

            semNolock.Should().BeEmpty("o Vórtice é um sistema em produção, e a leitura não segura trava nele");
        }
    }

    [Fact]
    public void A_janela_e_os_tipos_31_41_e_50_estao_em_processo_agenda_e_historico()
    {
        LeitorDasOportunidadesDoVortice.ConsultaDosProcessos.Should().Contain("WHERE d.CodProcesso IN (31, 41, 50)")
            .And.Contain("(p.DtaInclusao >= @janela OR a.PrimeiroAndamento >= @janela)")
            .And.Contain("t.Depto = @depto");

        // A AGENDA É LIDA PELO PROCESSO: tarefa avulsa não entra (decisão P6).
        LeitorDasOportunidadesDoVortice.ConsultaDaAgenda.Should().Contain("JOIN IV_PROCDADO d WITH (NOLOCK) ON d.Processo = a.Processo")
            .And.Contain("WHERE d.CodProcesso IN (31, 41, 50) AND a.DtaAgenda >= @janela");

        LeitorDasOportunidadesDoVortice.ConsultaDoHistorico.Should()
            .Contain("WHERE h.CodProcesso IN (31, 41, 50) AND h.Processo IS NOT NULL AND h.DtaRealizacao >= @janela");
        LeitorDasOportunidadesDoVortice.ConsultaDosEfeitos([3239, 250]).Should().Contain("pr.CodProcesso IN (31, 41, 50) AND pr.Resultado IN (250, 3239)");

        LeitorDasOportunidadesDoVortice.JanelaNaOrigem.Should().Be(new DateTime(2023, 11, 1, 0, 0, 0), "01/11/2023, 00:00 de São Paulo — como o Vórtice grava");
        FluentActions.Invoking(() => LeitorDasOportunidadesDoVortice.ConsultaDasAcoes([])).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cada_coluna_citada_existe_na_tabela_do_DDL_do_Vortice()
    {
        var ddl = string.Join('\n', new[] { "IV.sql", "GE.sql", "IVS.sql" }
            .Select(f => File.ReadAllText(Path.Combine(RaizDoRepositorio(), "docs", "extracao-vortice", "ddl", f))));
        var faltando = new List<string>();

        foreach (var consulta in TodasAsConsultas)
        {
            var tabelaDoApelido = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (Match m in Regex.Matches(consulta, @"(?:FROM|JOIN)\s+(\w+)\s+(\w+)", RegexOptions.IgnoreCase))
                if (!Cte.Contains(m.Groups[1].Value)) tabelaDoApelido[m.Groups[2].Value] = m.Groups[1].Value;

            foreach (var (apelido, tabela) in tabelaDoApelido)
            {
                var criacao = Regex.Match(ddl, $@"CREATE TABLE \[dbo\]\.\[{tabela}\] \((.*?)\n\)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                criacao.Success.Should().BeTrue($"a tabela {tabela} existe no Vórtice");

                var colunas = Regex.Matches(criacao.Groups[1].Value, @"^\s*\[(\w+)\]", RegexOptions.Multiline)
                    .Select(c => c.Groups[1].Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

                faltando.AddRange(Regex.Matches(consulta, $@"\b{apelido}\.(\w+)\b")
                    .Select(c => c.Groups[1].Value)
                    .Where(c => !colunas.Contains(c))
                    .Select(c => $"{tabela}.{c}"));
            }
        }

        // O APELIDO DE SUBCONSULTA (o "a" do primeiro andamento, o "c" da carteira) não é tabela, e fica de fora da conferência.
        faltando.Should().BeEmpty("o nome de coluna errado só apareceria na primeira rodada no servidor");
    }

    [Fact]
    public void A_linha_do_processo_e_traduzida_com_data_em_UTC_e_documento_recomposto()
    {
        var tabela = new DataTable();
        foreach (var (nome, tipo) in new[]
                 {
                     ("Processo", typeof(decimal)), ("CodProcesso", typeof(decimal)), ("NroEmpresa", typeof(decimal)), ("SeqPessoa", typeof(decimal)),
                     ("FisicaJuridica", typeof(string)), ("NroCGCCPF", typeof(decimal)), ("DigCGCCPF", typeof(decimal)), ("SeqCarteira", typeof(decimal)),
                     ("UsuResponsavel", typeof(string)), ("DtaInclusao", typeof(DateTime)), ("Status", typeof(string)), ("DtaRealizacao", typeof(DateTime)),
                     ("Fase", typeof(string)), ("FaseOrdem", typeof(decimal)), ("DtaFase", typeof(DateTime)), ("DtaStatus", typeof(DateTime)),
                     ("Resumo", typeof(string)), ("Valor", typeof(decimal)), ("Qtde", typeof(decimal)), ("DtaPrevConclusao", typeof(DateTime)),
                     ("DtaPrevConcOrig", typeof(DateTime)), ("PrimeiroAndamento", typeof(DateTime))
                 })
            tabela.Columns.Add(nome, tipo);

        tabela.Rows.Add(123456m, 41m, 13m, 777m, "F", 529982247m, 25m, 18m, " MARIA.SOUZA ", new DateTime(2024, 1, 5, 9, 30, 0), "EM ABERTO",
            DBNull.Value, " NEGOCIACAO ", 3m, new DateTime(2024, 2, 1, 8, 0, 0), new DateTime(2024, 2, 2, 8, 0, 0), "  Trator 6M  ",
            480000.50m, 1m, new DateTime(2024, 6, 30, 23, 0, 0), new DateTime(2024, 5, 31), new DateTime(2024, 1, 6, 10, 0, 0));

        using var leitor = tabela.CreateDataReader();
        leitor.Read().Should().BeTrue();
        var processo = LeitorDasOportunidadesDoVortice.MapearProcesso(leitor);

        processo.Should().BeEquivalentTo(new
        {
            Numero = 123456L,
            Tipo = (short)41,
            NroEmpresa = (int?)13,
            SeqCarteira = (int?)18,
            LoginDoResponsavel = "MARIA.SOUZA",
            IncluidoEmUtc = (DateTime?)new DateTime(2024, 1, 5, 12, 30, 0, DateTimeKind.Utc),
            PrimeiroAndamentoEmUtc = (DateTime?)new DateTime(2024, 1, 6, 13, 0, 0, DateTimeKind.Utc),
            RealizadoEmUtc = (DateTime?)null,
            Fase = "NEGOCIACAO",
            FaseOrdem = (int?)3,
            FaseDesdeUtc = (DateTime?)new DateTime(2024, 2, 1, 11, 0, 0, DateTimeKind.Utc),
            Resumo = "Trator 6M",
            Valor = (decimal?)480000.50m,
            PrevisaoConclusao = (DateOnly?)new DateOnly(2024, 6, 30),
            PrevisaoConclusaoOriginal = (DateOnly?)new DateOnly(2024, 5, 31)
        }, o => o.ExcludingMissingMembers());
        processo.Documento.Numero.Should().Be("52998224725");
    }

    [Fact]
    public void A_linha_da_agenda_e_a_do_historico_sao_traduzidas_e_o_codigo_zero_e_nenhum()
    {
        var agenda = new DataTable();
        foreach (var nome in new[] { "SeqAgenda", "Processo", "SeqUsuario", "Acao", "Prioridade", "UltResultado", "UltHistorico", "HistoricoOrigem", "UsuarioQueConcluiu" })
            agenda.Columns.Add(nome, typeof(decimal));
        foreach (var nome in new[] { "Realizada", "TipoAgendamento" }) agenda.Columns.Add(nome, typeof(string));
        foreach (var nome in new[] { "DtaAgenda", "DtaLimiteExecucao", "DtaRealizacao", "DtaGeracao" }) agenda.Columns.Add(nome, typeof(DateTime));

        var linha = agenda.NewRow();
        linha["SeqAgenda"] = 90m; linha["Processo"] = 123456m; linha["SeqUsuario"] = 7m; linha["Acao"] = 0m; linha["Prioridade"] = 2m;
        linha["UltResultado"] = 3239m; linha["UltHistorico"] = 500m; linha["HistoricoOrigem"] = 0m; linha["UsuarioQueConcluiu"] = 9m;
        linha["Realizada"] = "S"; linha["TipoAgendamento"] = "A";
        linha["DtaAgenda"] = new DateTime(2024, 3, 1, 9, 0, 0); linha["DtaRealizacao"] = new DateTime(2024, 3, 2, 10, 0, 0);
        agenda.Rows.Add(linha);

        using (var leitor = agenda.CreateDataReader())
        {
            leitor.Read();
            LeitorDasOportunidadesDoVortice.MapearTarefa(leitor).Should().BeEquivalentTo(new
            {
                SeqAgenda = 90L, Processo = 123456L, SeqUsuario = (long?)7, Acao = (int?)null, Prioridade = (decimal?)2m, Realizada = "S",
                AgendadaParaUtc = (DateTime?)new DateTime(2024, 3, 1, 12, 0, 0, DateTimeKind.Utc),
                RealizadaEmUtc = (DateTime?)new DateTime(2024, 3, 2, 13, 0, 0, DateTimeKind.Utc),
                Resultado = (int?)3239, SeqHistoricoDeConclusao = (long?)500, SeqHistoricoDeOrigem = (long?)null, SeqUsuarioQueConcluiu = (long?)9,
                TipoAgendamento = "A", GeradaEmUtc = (DateTime?)null
            }, o => o.ExcludingMissingMembers(), "ação zero e histórico de origem zero são 'não há'");
        }

        var historico = new DataTable();
        foreach (var nome in new[] { "SeqHistorico", "Processo", "SeqUsuario", "AcaoGeradora", "Resultado", "AgendaOrigem", "Duracao", "Latitude", "Longitude", "DeSistema" })
            historico.Columns.Add(nome, typeof(decimal));
        historico.Columns.Add("Natureza", typeof(string));
        historico.Columns.Add("DtaRealizacao", typeof(DateTime));
        historico.Rows.Add(500m, 123456m, 9m, 50m, 3239m, 90m, 0m, -21.1m, -47.8m, 1m, "A", new DateTime(2024, 3, 2, 10, 0, 0));

        using (var leitor = historico.CreateDataReader())
        {
            leitor.Read();
            LeitorDasOportunidadesDoVortice.MapearInteracao(leitor).Should().Be(new InteracaoDaOportunidadeNoVortice(
                500, 123456, 9, 50, 3239, 90, "A", true, new DateTime(2024, 3, 2, 13, 0, 0, DateTimeKind.Utc), 0m, -21.1m, -47.8m));
        }
    }

    [Fact]
    public async Task A_cadeia_malformada_e_dita_como_credencial_e_so_ela()
    {
        var leitura = await new LeitorDasOportunidadesDoVortice(new OpcoesDoVortice { Conexao = "isto não é=uma;cadeia==de conexão" })
            .LerAsync(CancellationToken.None);
        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("malformada");

        var semCredencial = await new LeitorDasOportunidadesDoVortice(new OpcoesDoVortice()).LerAsync(CancellationToken.None);
        semCredencial.Erro.Should().Contain("credencial do Vórtice");
    }

    private static string RaizDoRepositorio()
    {
        var atual = new DirectoryInfo(AppContext.BaseDirectory);
        while (atual is not null && !atual.EnumerateFiles("*.sln").Any()) atual = atual.Parent;
        return atual?.FullName ?? throw new InvalidOperationException("Não encontrei a raiz do repositório (nenhum .sln acima).");
    }
}
