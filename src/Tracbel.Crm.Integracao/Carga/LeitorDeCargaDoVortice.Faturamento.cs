using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// A leitura do FATURAMENTO que o Vórtice recebe do Protheus.
///
/// <para><b>A janela não é o recorte da carga, e nem o calendário.</b> Todo o resto desta carga
/// lê o ano corrente; aqui a leitura são os três anos <b>anteriores à última nota que existe na
/// origem</b>. Duas razões, e as duas foram medidas:</para>
///
/// <para>1. <b>Três anos, e não um.</b> Um cliente que comprou uma colheitadeira em 2023 e nada
/// em 2024 é cliente grande, e um recorte de doze meses o classificaria como se não existisse.</para>
///
/// <para>2. <b>Ancorada no dado, e não em 2026.</b> Ancorar no ano da carga jogaria a janela em
/// 2024-2027, e a origem para em 11/04/2025: sobrariam R$ 952 milhões e 5.104 clientes, contra
/// R$ 2,35 bilhões e 6.998 clientes da janela certa. Dois terços do faturamento ficariam de fora.</para>
///
/// <para><b>A origem para em 11/04/2025</b>, e nada aqui esconde isso: a data máxima lida vira a
/// marca de sincronismo, e é ela que faz a tela escrever o período em vez de dizer "faturamento
/// do mês".</para>
///
/// <para><b>O cliente casa pelo documento.</b> A tabela do ERP não tem o identificador do CRM —
/// tem CPF/CNPJ. Numa amostra dos 300 maiores clientes por faturamento, 204 casaram com o nosso
/// cadastro; os outros são de filiais fora do recorte das treze da Agro. Quem não casa não entra,
/// e a contagem do que não casou sai no relatório da carga.</para>
/// </summary>
public sealed partial class LeitorDeCargaDoVortice
{
    /// <summary>Quantos anos para trás a curva ABC olha.</summary>
    private const int AnosDeFaturamento = 3;

    /// <summary>
    /// Lê o faturamento agregado por cliente, filial e mês.
    /// </summary>
    /// <param name="recorte">O recorte da carga — dele vem só a lista de filiais.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<FaturamentoParaCarga>>> LerFaturamentoAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "X_TOTVS_CRM_FATURAMENTO",
            recorte,
            $$"""
             SELECT f.CPF_CNPJ                                        AS Documento,
                    f.FILIAL                                          AS NroEmpresa,
                    DATEFROMPARTS(YEAR(f.DATA_EMISSAO_NF), MONTH(f.DATA_EMISSAO_NF), 1) AS Competencia,
                    SUM(f.VLR_LIQUIDO_ITEM)                           AS ValorLiquido,
                    COUNT(DISTINCT f.NRO_NF)                          AS Notas,
                    COUNT(*)                                          AS Itens
             FROM X_TOTVS_CRM_FATURAMENTO f WITH (NOLOCK)
             CROSS JOIN (
                 -- A JANELA ANCORA NA ÚLTIMA NOTA, E NÃO NO ANO DA CARGA.
                 --
                 -- Ancorada no recorte (2026), a janela de três anos cairia em 2024-2027 — e a
                 -- origem para em 11/04/2025. O resultado medido: R$ 952 milhões e 5.104
                 -- clientes, contra R$ 2,35 bilhões e 6.998 clientes quando ancorada na última
                 -- nota. Dois terços do faturamento ficavam de fora, e com eles a classe de
                 -- quase dois mil clientes.
                 --
                 -- Ancorar no dado tem uma segunda virtude: no dia em que a integração com o
                 -- Protheus voltar, a janela anda sozinha, sem ninguém trocar constante nenhuma.
                 SELECT MAX(u.DATA_EMISSAO_NF) AS UltimaNota
                 FROM X_TOTVS_CRM_FATURAMENTO u WITH (NOLOCK)
                 WHERE u.DELETADO <> 'S'
             ) AS janela
             WHERE f.DELETADO <> 'S'
               AND f.FILIAL IN ({FILIAIS})
               AND f.DATA_EMISSAO_NF > DATEADD(year, -{{AnosDeFaturamento}}, janela.UltimaNota)
               AND f.DATA_EMISSAO_NF <= janela.UltimaNota
               -- SÓ DOCUMENTO COM TAMANHO DE DOCUMENTO. A coluna aceita qualquer texto, e o que
               -- não tem 11 nem 14 dígitos não casa com cliente nenhum: entraria como linha
               -- órfã e sairia do outro lado como faturamento sem dono.
               AND LEN(LTRIM(RTRIM(f.CPF_CNPJ))) IN (11, 14)
             GROUP BY f.CPF_CNPJ, f.FILIAL,
                      DATEFROMPARTS(YEAR(f.DATA_EMISSAO_NF), MONTH(f.DATA_EMISSAO_NF), 1)
             HAVING SUM(f.VLR_LIQUIDO_ITEM) > 0
             ORDER BY 3
             """,
            LerFaturamento,
            ct);

    private static (FaturamentoParaCarga?, LinhaRecusada?) LerFaturamento(
        SqlDataReader leitor, string objeto)
    {
        var documento = Texto(leitor, "Documento")?.Trim() ?? string.Empty;
        var competencia = Data(leitor, "Competencia", opcional: false);

        if (competencia is null)
        {
            return (null, new LinhaRecusada(
                "FaturamentoDoCliente", "SemCompetencia", documento,
                "A nota não tem data de emissão, e sem ela o valor não pertence a mês nenhum.",
                $"{{\"documento\":\"{documento}\"}}"));
        }

        return (new FaturamentoParaCarga(
            ChaveDeOrigem: $"{documento}/{(int?)Numero(leitor, "NroEmpresa")}/{competencia:yyyy-MM}",
            DocumentoDoCliente: documento,
            CodigoDaFilialNoLegado: (int)(Numero(leitor, "NroEmpresa") ?? 0),
            Competencia: DateOnly.FromDateTime(competencia.Value),
            ValorLiquido: decimal.Round(Numero(leitor, "ValorLiquido") ?? 0m, 2),
            Notas: (int)(Numero(leitor, "Notas") ?? 0),
            Itens: (int)(Numero(leitor, "Itens") ?? 0)), null);
    }
}
