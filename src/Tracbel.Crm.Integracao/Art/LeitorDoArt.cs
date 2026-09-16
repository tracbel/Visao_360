using System.Globalization;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.Art;

/// <summary>
/// Uma linha da view de vendas de máquina do ART, como veio — tudo texto, antes do saneamento.
///
/// <para><b>O que não está aqui, de propósito:</b> valor de venda e de compra, ICMS, custo, frete,
/// comissão, bônus, lucro, margem e resultado contábil — a view tem 40 colunas assim, e nenhuma
/// entra na integração. O vendedor e o usuário também não: são nomes de pessoa sem vínculo com
/// usuário do CRM. A consulta nem os seleciona, então eles não chegam à memória deste processo.</para>
///
/// <para><b>Datas como texto</b> porque o ART guarda data zerada (<c>0000-00-00</c>), que o
/// conector recusaria ou converteria em silêncio. Lida como texto, a transformação fica explícita
/// no saneamento, e registrada.</para>
/// </summary>
public sealed record RegistroDoArt(
    string Codigo,
    string? Chassis,
    string? CpfCnpj,
    string? Cliente,
    string? Linha,
    string? Produto,
    string? Empresa,
    string? Unidade,
    string? UnidadeDoFaturamento,
    string? DataDaVenda,
    string? DataDoFaturamento,
    string? Entrega,
    string? AbertaEm,
    string? Situacao,
    string? NumeroDoPedido,
    string? NumeroDaNotaFiscal,
    string? Gestao,
    string? VendaDireta,
    string? RepasseDireto,
    string? Quantidade);

/// <summary>
/// A LEITURA DO ART — uma consulta à view de vendas, em sessão somente leitura.
/// </summary>
/// <param name="opcoes">A configuração, vinda de variável de ambiente.</param>
public sealed class LeitorDoArt(IOptions<OpcoesDoArt> opcoes)
{
    /// <summary>O código do sistema em <c>integracao.Sistema</c>.</summary>
    public const string CodigoDoSistema = "ART";

    /// <summary>
    /// As colunas lidas. Nenhuma financeira: ver <see cref="RegistroDoArt"/>.
    /// </summary>
    private const string Colunas =
        "CAST(codigo AS CHAR), chassis, cpf_cnpj, cliente, linha, produto, empresa, unidade, unidade_fat, " +
        "CAST(data_vda AS CHAR), CAST(data_fat AS CHAR), CAST(entrega AS CHAR), CAST(dt_abertura AS CHAR), " +
        "situacao, num_ped, CAST(num_nfe_venda AS CHAR), gestao, venda_direta, repasse_direto, CAST(qte AS CHAR)";

    /// <summary>Lê a view inteira.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<IReadOnlyList<RegistroDoArt>>> LerVendasAsync(CancellationToken ct)
    {
        var config = opcoes.Value;
        if (!config.EstaConfigurada)
            return Resultado<IReadOnlyList<RegistroDoArt>>.Indisponivel(
                "A leitura do ART exige Art__Servidor, Art__Banco, Art__Usuario, Art__Senha e Art__Visao.");

        // O NOME DA VIEW ENTRA NA CONSULTA, e por isso é conferido: só letra, número e sublinhado.
        if (!config.Visao!.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))
            return Resultado<IReadOnlyList<RegistroDoArt>>.Indisponivel("O nome da view do ART tem caractere não aceito.");

        var construtor = new MySqlConnectionStringBuilder
        {
            Server = config.Servidor,
            Port = config.Porta,
            Database = config.Banco,
            UserID = config.Usuario,
            Password = config.Senha,
            ConnectionTimeout = 15,
            DefaultCommandTimeout = (uint)config.TempoLimiteSegundos,
            CharacterSet = "utf8mb4"
        };

        try
        {
            await using var conexao = new MySqlConnection(construtor.ConnectionString);
            await conexao.OpenAsync(ct);

            await using (var somenteLeitura = new MySqlCommand("SET SESSION TRANSACTION READ ONLY", conexao))
                await somenteLeitura.ExecuteNonQueryAsync(ct);

            await using var comando = new MySqlCommand($"SELECT {Colunas} FROM `{config.Visao}`", conexao);
            await using var leitor = await comando.ExecuteReaderAsync(ct);

            var registros = new List<RegistroDoArt>();
            while (await leitor.ReadAsync(ct))
            {
                string? Texto(int i) => leitor.IsDBNull(i) ? null : Convert.ToString(leitor.GetValue(i), CultureInfo.InvariantCulture);

                registros.Add(new RegistroDoArt(
                    Texto(0) ?? string.Empty, Texto(1), Texto(2), Texto(3), Texto(4), Texto(5), Texto(6), Texto(7),
                    Texto(8), Texto(9), Texto(10), Texto(11), Texto(12), Texto(13), Texto(14), Texto(15), Texto(16),
                    Texto(17), Texto(18), Texto(19)));
            }

            return Resultado<IReadOnlyList<RegistroDoArt>>.Ok(registros);
        }
        catch (MySqlException falha)
        {
            // A MENSAGEM DO CONECTOR NÃO CITA A SENHA, mas pode citar servidor e usuário: sai só o código.
            return Resultado<IReadOnlyList<RegistroDoArt>>.Indisponivel(
                $"O ART não respondeu à leitura (erro MySQL {falha.Number}). Nada foi gravado.");
        }
    }
}
