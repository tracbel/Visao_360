using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Integracao.Testes.Vortice;

/// <summary>
/// A PONTE CAINDO — o comportamento que precisa valer justamente quando nada mais funciona.
///
/// <para>A ponte depende de VPN e de um servidor que não é nosso. O requisito é explícito: se o
/// legado não responder, o endpoint devolve erro claro e <b>o resto da API segue de pé</b>. Estes
/// testes exercitam os dois caminhos de indisponibilidade sem precisar de rede nenhuma — o que é
/// o ponto, porque um teste que só roda com VPN é um teste que não roda no CI.</para>
///
/// <para>O caminho de SUCESSO da ponte não é testável aqui: ele exige o banco do legado, que não
/// existe em ambiente de teste e não deve ser dublado com um esquema falso — dublê de banco
/// legado seria um segundo modelo do legado para manter, que é exatamente o que a camada de
/// quarentena existe para evitar. Ele é verificado por exercício manual contra a base real, e o
/// resultado fica registrado no documento 23.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class PonteDeLeituraDoVorticeTestes(ITestOutputHelper saida)
{
    private static CancellationToken Ct => CancellationToken.None;

    private static PonteDeLeituraDoVortice Ponte(string? conexao, int tempoLimite = 2) =>
        new(Options.Create(new OpcoesDoVortice
            {
                Conexao = conexao,
                TempoLimiteSegundos = tempoLimite
            }),
            new RelogioFixo(),
            NullLogger<PonteDeLeituraDoVortice>.Instance);

    [Fact]
    public async Task Sem_a_variavel_de_ambiente_a_ponte_diz_que_NAO_ESTA_CONFIGURADA_e_como_configurar()
    {
        var resultado = await Ponte(conexao: null).VerificarAsync(Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoDeFalha.DependenciaIndisponivel);

        // A frase diz O QUE FAZER, e diz também que o resto continua funcionando — que é a
        // informação que evita o chamado "o CRM caiu".
        resultado.Erro.Should().Contain("Vortice__Conexao");
        resultado.Erro.Should().Contain("continua funcionando");

        saida.WriteLine(resultado.Erro!);
    }

    [Fact]
    public async Task Servidor_inalcancavel_vira_DEPENDENCIA_INDISPONIVEL_e_nao_excecao()
    {
        // Endereço reservado para documentação (RFC 5737): nunca responde, em máquina nenhuma.
        // É o que simula a VPN fora do ar de forma determinística.
        var ponte = Ponte("Server=192.0.2.1,1433;Database=crm;User Id=x;Password=y;" +
                          "TrustServerCertificate=True;Encrypt=False;Connect Timeout=2");

        var resultado = await ponte.VerificarAsync(Ct);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Tipo.Should().Be(TipoDeFalha.DependenciaIndisponivel);

        // A MENSAGEM APONTA A VPN, que é a causa em quase todos os casos, e NÃO expõe nome de
        // servidor nem topologia de rede — o detalhe técnico vai para o log, não para a resposta.
        resultado.Erro.Should().Contain("VPN");
        resultado.Erro.Should().NotContain("192.0.2.1");

        saida.WriteLine(resultado.Erro!);
    }

    [Fact]
    public async Task Os_tres_metodos_da_ponte_recusam_do_mesmo_jeito_quando_o_legado_nao_responde()
    {
        // Se um dos três lançasse em vez de devolver resultado, a API viraria 500 naquele
        // endpoint — e a promessa "a ponte caída não derruba o resto" valeria só nos outros dois.
        var ponte = Ponte(conexao: null);

        var saude = await ponte.VerificarAsync(Ct);
        var clientes = await ponte.BuscarClientesAsync("santa", 10, Ct);
        var parque = await ponte.ListarParqueDeMaquinasAsync(26192, Ct);

        saude.Tipo.Should().Be(TipoDeFalha.DependenciaIndisponivel);
        clientes.Tipo.Should().Be(TipoDeFalha.DependenciaIndisponivel);
        parque.Tipo.Should().Be(TipoDeFalha.DependenciaIndisponivel);
    }

    [Fact]
    public void A_ponte_declara_quais_objetos_do_legado_estao_VIVOS_e_quais_estao_PARADOS()
    {
        // ESTE TESTE GUARDA CONHECIMENTO, não comportamento. As tabelas paradas do legado
        // respondem à consulta, têm centenas de milhares de linhas e PARECEM disponíveis. Foi
        // assim que 17 meses de dado velho passaram por atual. Se alguém apagar uma destas linhas
        // achando que é comentário morto, o teste avisa.
        var vivos = PonteDeLeituraDoVortice.ObjetosDoLegado.Where(o => o.Vivo).Select(o => o.Objeto).ToList();
        var parados = PonteDeLeituraDoVortice.ObjetosDoLegado.Where(o => !o.Vivo).ToList();

        vivos.Should().Contain(["GE_Pessoa", "IV_ClientePropr", "IV_Processo", "IV_Agenda", "IV_Historico"]);

        parados.Select(o => o.Objeto)
            .Should().Contain(["EXT_Veic", "EXT_NFS", "EXT_OS", "EXT_Titulo"],
                "as tabelas de frota do ERP, faturamento, ordem de serviço e título estão congeladas");

        // Cada objeto parado carrega o PORQUÊ, e não só a marcação.
        parados.Should().AllSatisfy(o =>
            o.Situacao.Should().NotBeNullOrWhiteSpace("dizer que parou sem dizer quando não ajuda ninguém"));

        foreach (var (objeto, _, situacao) in PonteDeLeituraDoVortice.ObjetosDoLegado)
            saida.WriteLine($"{objeto,-18} {situacao}");
    }

    private sealed class RelogioFixo : IRelogio
    {
        public DateTime Agora { get; } = new(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc);
    }
}
