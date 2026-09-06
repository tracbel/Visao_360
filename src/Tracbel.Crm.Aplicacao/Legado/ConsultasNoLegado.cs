using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Legado;

/// <summary>
/// Busca clientes NO SISTEMA LEGADO, para a tela mostrar o dado real enquanto o cadastro novo
/// ainda está sendo construído.
///
/// ISTO NÃO GRAVA NADA. O que volta daqui é leitura carimbada com origem e data; virar cliente
/// nosso exige alguém cadastrar pelo <c>POST /api/v1/clientes</c>, e aí o dado passa pelas
/// nossas regras. Misturar as duas coisas — ler de lá e gravar aqui automaticamente — é como se
/// importa a sujeira do legado junto com o dado.
/// </summary>
public sealed class BuscarClientesNoLegado(IPonteDeLeituraDoVortice ponte)
{
    /// <summary>O mínimo de caracteres para buscar. Menos que isso varre a base inteira.</summary>
    public const int TamanhoMinimoDoTermo = 3;

    /// <summary>O teto de linhas por busca.</summary>
    public const int LimiteMaximo = 100;

    /// <summary>Executa a busca.</summary>
    /// <param name="termo">Nome, nome fantasia ou documento.</param>
    /// <param name="limite">Quantas linhas no máximo.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LeituraDoLegado<IReadOnlyList<ClienteNoLegado>>>> ExecutarAsync(
        string? termo, int? limite, CancellationToken ct)
    {
        var erros = new ColetorDeErros();

        var texto = erros.Obrigatorio("termo", termo, "o que procurar");

        if (texto.Length is > 0 and < TamanhoMinimoDoTermo)
            erros.Registrar(
                "termo",
                $"Informe pelo menos {TamanhoMinimoDoTermo} caracteres. " +
                "Uma busca mais curta varreria o cadastro inteiro do sistema legado.",
                termo);

        var quantas = limite ?? 25;
        if (quantas is < 1 or > LimiteMaximo)
            erros.Registrar("limite", $"O limite vai de 1 a {LimiteMaximo}.", limite?.ToString());

        if (erros.TemErro)
            return Task.FromResult(
                erros.Recusar<LeituraDoLegado<IReadOnlyList<ClienteNoLegado>>>(
                    "A busca no sistema legado tem parâmetros a corrigir."));

        return ponte.BuscarClientesAsync(texto, quantas, ct);
    }
}

/// <summary>
/// Lê o parque de máquinas de um cliente NO SISTEMA LEGADO.
///
/// É a lista que o CEN mantém lá e que continua sendo atualizada hoje — diferente das tabelas de
/// frota vindas do ERP, que estão paradas desde 2024 e por isso NÃO são lidas por esta ponte.
/// </summary>
public sealed class ListarParqueNoLegado(IPonteDeLeituraDoVortice ponte)
{
    /// <summary>Executa a leitura.</summary>
    /// <param name="identificadorNoLegado">A chave da pessoa no sistema de origem.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LeituraDoLegado<IReadOnlyList<EquipamentoNoLegado>>>> ExecutarAsync(
        long identificadorNoLegado, CancellationToken ct)
    {
        if (identificadorNoLegado <= 0)
            return Task.FromResult(
                Resultado<LeituraDoLegado<IReadOnlyList<EquipamentoNoLegado>>>.FalhaDeValidacao(
                    "O identificador do cliente no sistema legado não vale.",
                    [new ErroDeCampo(
                        "identificador",
                        "O identificador é um número inteiro positivo, devolvido pela busca de clientes no legado.",
                        identificadorNoLegado.ToString())]));

        return ponte.ListarParqueDeMaquinasAsync(identificadorNoLegado, ct);
    }
}

/// <summary>
/// Verifica se a ponte está de pé.
///
/// A tela consulta isto antes de oferecer as abas do legado — assim ela mostra "sistema legado
/// indisponível" no lugar certo, em vez de deixar o usuário clicar e receber erro.
/// </summary>
public sealed class VerificarPonteDoLegado(IPonteDeLeituraDoVortice ponte)
{
    /// <summary>Executa a verificação.</summary>
    public Task<Resultado<LeituraDoLegado<SaudeDaPonte>>> ExecutarAsync(CancellationToken ct) =>
        ponte.VerificarAsync(ct);
}
