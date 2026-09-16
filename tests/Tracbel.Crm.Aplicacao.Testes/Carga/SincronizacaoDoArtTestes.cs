using FluentAssertions;
using Tracbel.Crm.Carga.Sincronizacao;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// O que o serviço de sincronização do ART decide sem banco: a configuração aceita, a espera entre
/// tentativas e a mensagem sem credencial (documento 35, seção 11). As credenciais aqui são inventadas.
/// </summary>
public sealed class SincronizacaoDoArtTestes
{
    [Fact]
    public void A_mensagem_nao_carrega_usuario_nem_senha_nem_trecho_de_cadeia_de_conexao()
    {
        var mensagem = Sigilo.Mascarar(
            "Login failed for user 'leitor_art'. Server=srv;User Id=leitor_art;Password=S3gr3d0!;Pwd = outra ; senha S3gr3d0!",
            ["leitor_art", "S3gr3d0!", null, ""]);

        mensagem.Should().NotContain("leitor_art").And.NotContain("S3gr3d0!").And.NotContain("outra");
        mensagem.Should().Contain("Login failed for user '***'").And.Contain("Password=***");
    }

    [Fact]
    public void Segredo_curto_demais_nao_apaga_a_mensagem_inteira()
    {
        Sigilo.Mascarar("erro SQL 18456", ["sa", "1"]).Should().Be("erro SQL 18456");
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 30)]
    [InlineData(3, 60)]
    [InlineData(4, 120)]
    [InlineData(9, 900)]
    public void A_espera_dobra_a_cada_tentativa_e_para_em_quinze_minutos(int tentativa, int segundos)
    {
        ExecutorDaSincronizacaoDoArt.EsperaAntesDa(tentativa, TimeSpan.FromSeconds(30))
            .Should().Be(TimeSpan.FromSeconds(segundos));
    }

    [Fact]
    public void A_configuracao_padrao_vale_e_a_fora_da_faixa_diz_qual_chave_corrigir()
    {
        new OpcoesDaSincronizacaoDoArt().Validar().Should().BeNull();
        new OpcoesDaSincronizacaoDoArt().Habilitada.Should().BeFalse("sem o arquivo do servidor, o serviço não grava");

        new OpcoesDaSincronizacaoDoArt { IntervaloMinutos = 1 }.Validar().Should().Contain("IntervaloMinutos");
        new OpcoesDaSincronizacaoDoArt { Tentativas = 0 }.Validar().Should().Contain("Tentativas");
        new OpcoesDaSincronizacaoDoArt { EsperaEntreTentativasSegundos = -1 }.Validar().Should().Contain("EsperaEntreTentativasSegundos");
    }
}
