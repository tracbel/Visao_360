using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Vortice;

/// <summary>
/// O ÚLTIMO CONTATO PELA REGRA DA BI DO VÓRTICE (decisão de 27/09/2026). O que estes testes prendem: a lista de
/// resultados é a da view <c>BI_CARTEIRA_VN</c>, código a código e na mesma ordem; a consulta agrega por pessoa no
/// Vórtice, sem trazer linha de histórico, sem filtro de departamento no histórico e sem a coluna de nomes.
/// </summary>
public sealed class UltimoContatoDoVorticeTestes
{
    private static readonly string Consulta = LeitorDeCarteirasDoVortice.ConsultaDosUltimosContatos;

    [Fact]
    public void A_lista_e_a_das_linhas_86_e_87_da_view_da_BI_codigo_a_codigo_e_na_mesma_ordem()
    {
        var view = File.ReadAllLines(Path.Combine(RaizDoRepositorio(), "docs", "extracao-vortice", "modulos", "views", "BI_CARTEIRA_VN.sql"));

        // As linhas 86 e 87 do arquivo (índices 85 e 86): o IN (...) do HIS.RESULTADO.
        var trecho = view[85] + " " + view[86];
        trecho.Should().Contain("HIS.RESULTADO IN", "a especificação mora nessas duas linhas; se a view foi reextraída, confira de novo");

        var codigos = Regex.Match(trecho, @"IN\s*\(([^)]*)\)").Groups[1].Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => int.Parse(c, CultureInfo.InvariantCulture))
            .ToList();

        LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato.Should().Equal(codigos);
    }

    [Fact]
    public void Sao_53_codigos_sem_repeticao()
    {
        LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato.Should().HaveCount(53);
        LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void A_consulta_usa_a_lista_inteira_e_nada_mais()
    {
        var emTexto = Regex.Match(Consulta, @"h\.Resultado IN \(([^)]*)\)").Groups[1].Value
            .Split(',', StringSplitOptions.TrimEntries)
            .Select(c => int.Parse(c, CultureInfo.InvariantCulture));

        emTexto.Should().Equal(LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato);
    }

    [Fact]
    public void A_consulta_agrega_por_pessoa_no_Vortice_e_so_devolve_pessoa_e_data()
    {
        Consulta.Should().Contain("SELECT h.SeqPessoa, MAX(h.DtaRealizacao) AS UltimoContatoEm");
        Consulta.Should().Contain("GROUP BY h.SeqPessoa", "nenhuma linha de histórico sai do Vórtice");
        Consulta.Should().Contain("IV_Historico h WITH (NOLOCK)").And.Contain("IVS_Pes p WITH (NOLOCK)",
            "como as outras consultas do leitor: não segura trava num sistema em produção");
    }

    [Fact]
    public void O_universo_e_o_dos_vinculos_mas_o_historico_nao_tem_filtro_de_departamento()
    {
        // A PESSOA é a da carteira do departamento — o mesmo universo da consulta dos vínculos.
        Consulta.Should().Contain("p.SeqDepto = @seqDepto AND p.SeqCarteira IS NOT NULL");

        // O HISTÓRICO não: contato de qualquer área conta, como na BI.
        Consulta.Should().NotContainAny("h.Departamento", "h.NroEmpresa", "h.SeqDepto");
    }

    [Fact]
    public void A_coluna_de_nomes_do_historico_nunca_aparece()
    {
        // IV_Historico.Contato guarda NOMES de pessoas: nem selecionada, nem agrupada, nem filtrada.
        Regex.IsMatch(Consulta, @"\bContato\b", RegexOptions.IgnoreCase).Should().BeFalse();
    }

    [Fact]
    public void Sem_leitura_do_historico_ninguem_tem_ultimo_contato()
    {
        var leitura = new LeituraDasCarteirasDoVortice(
            new DepartamentoNoVortice(2, "MAQ-NOVOS", null, 180, 180, 180, 360), [], new Dictionary<long, DonoNoVortice>(), []);

        leitura.UltimosContatos.Should().BeEmpty("ausência de contato é \"nunca contatado\", e não um valor inventado");
        leitura.TempoDaConsultaDosUltimosContatos.Should().BeNull();
    }

    private static string RaizDoRepositorio()
    {
        var atual = new DirectoryInfo(AppContext.BaseDirectory);
        while (atual is not null && !atual.EnumerateFiles("*.sln").Any()) atual = atual.Parent;
        return atual?.FullName ?? throw new InvalidOperationException("Não encontrei a raiz do repositório (nenhum .sln acima).");
    }
}
