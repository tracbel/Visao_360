using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;

using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// A leitura das VENDAS PERDIDAS do Vórtice — a parte da carga que existe porque a pergunta
/// "por que perdemos" estava sendo respondida com "o legado não registra", e isso era falso.
///
/// <para><b>O motivo da perda não é coluna do processo.</b> Procurar por ele em
/// <c>IV_Processo</c> não encontra nada, e foi essa busca que produziu a conclusão errada. O
/// Vórtice tem um motor de formulário — <c>IV_Questionario</c>, com 88.087 respostas e 147
/// tabelas de resposta tipadas, uma por formulário — e a venda perdida é um desses formulários.
/// O dado sempre esteve lá, num lugar que ninguém tinha olhado.</para>
///
/// <para><b>São três gerações do mesmo formulário</b>, e as três são lidas: o atual
/// (<c>IV_Q_VENDA_PERDIDA_FY25</c>, 165 respostas em 2026), o de máquinas e implementos
/// (<c>IV_Q_VENDA_PERDIDA_MAQIMP</c>, até 2025) e o antigo (<c>IV_Q_VENDA_PERDIDA</c>, até
/// 2023). Os nomes de coluna mudam entre eles; o que descrevem é o mesmo, e a UNION os
/// normaliza uma vez, aqui, para que nada além deste arquivo precise saber que houve três.</para>
///
/// <para><b>A filial vem do processo</b>, e não do formulário: a resposta não carrega empresa.
/// É o mesmo desempate usado no resto da carga — a linha pertence à filial do registro a que
/// ela se refere.</para>
/// </summary>
public sealed partial class LeitorDeCargaDoVortice
{
    /// <summary>
    /// Lê as vendas perdidas registradas no período, das três gerações do formulário.
    /// </summary>
    /// <param name="recorte">O que trazer.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<Resultado<LoteDaCarga<VendaPerdidaParaCarga>>> LerVendasPerdidasAsync(
        RecorteDaCarga recorte, CancellationToken ct) =>
        LerAsync(
            "IV_Q_VENDA_PERDIDA*",
            recorte,
            """
            WITH Respostas AS (
                SELECT v.SEQQUESTIONARIO           AS Questionario,
                       v.VP_TIPO_EQUIP_CONCOR      AS TipoDoEquipamento,
                       v.VP_MARCA_EQUIP_CONCO      AS MarcaDoConcorrente,
                       v.VP_MODELO_EQUIP_CONC      AS ModeloDoConcorrente,
                       v.VP_REVENDA                AS RevendaDoConcorrente,
                       v.VP_MODELO_JD              AS ModeloOfertado,
                       v.VP_QUANTIDADE             AS Quantidade,
                       v.VP_DATA_VP                AS OcorridaEm,
                       v.VP_PRECO_CONCORRENTE      AS PrecoDoConcorrente,
                       v.VP_PRECO_JD               AS PrecoOfertado,
                       v.VP_MOTIVO_VP              AS Motivo,
                       CAST(NULL AS varchar(3))    AS Participamos
                FROM IV_Q_VENDA_PERDIDA_FY25 v WITH (NOLOCK)

                UNION ALL

                SELECT v.SEQQUESTIONARIO, v.TIPO_DE_EQUIPAMENTO, v.MARCA_VP, v.MODELO_VP,
                       v.REVENDA, v.MODELO_JOHN_DEER, v.QUANTIDADE, v.DATA_DA_VENDA,
                       v.PRECO_CONCORRENTE, v.PRECO_JOHN_DEERE, v.MOTIVO,
                       v.PARTICIPAMOS_DA_NEGO
                FROM IV_Q_VENDA_PERDIDA v WITH (NOLOCK)

                UNION ALL

                -- O formulário de máquinas e implementos tem OS MESMOS CAMPOS COM OUTROS
                -- NOMES: cada geração do formulário renomeou tudo (REVENDA_VP,
                -- QUANTIDADE_VP, MODELO_JOHN_DEERE_VP). Não é variação de significado, é
                -- variação de digitação de quem criou o formulário — e é exatamente o tipo de
                -- ruído que morre aqui e não atravessa para o modelo novo.
                SELECT v.SEQQUESTIONARIO, v.TIPO_EQUIP_VP, v.MARCA_VP, v.MODELO_VP,
                       v.REVENDA_VP, v.MODELO_JOHN_DEERE_VP, v.QUANTIDADE_VP, v.DATA_VENDA_VP,
                       v.PRECO_CONCORRENTE_VP, v.PRECO_JOHN_DEERE_VP, v.MOTIVO_VP,
                       CAST(NULL AS varchar(3))
                FROM IV_Q_VENDA_PERDIDA_MAQIMP v WITH (NOLOCK)
            )
            SELECT r.Questionario, r.TipoDoEquipamento, r.MarcaDoConcorrente,
                   r.ModeloDoConcorrente, r.RevendaDoConcorrente, r.ModeloOfertado,
                   r.Quantidade, r.OcorridaEm, r.PrecoDoConcorrente, r.PrecoOfertado,
                   r.Motivo, r.Participamos,
                   q.DtaRealizacao, q.UsuInclusao, q.Processo, q.SeqPessoa,
                   d.NroEmpresa
            FROM Respostas r
            JOIN IV_Questionario q WITH (NOLOCK) ON q.SeqQuestionario = r.Questionario
            LEFT JOIN IV_ProcDado d WITH (NOLOCK) ON d.Processo = q.Processo
            WHERE q.DtaRealizacao >= @inicio AND q.DtaRealizacao < @fim
              AND (d.NroEmpresa IS NULL OR d.NroEmpresa IN ({FILIAIS}))
            ORDER BY q.DtaRealizacao
            """,
            LerVendaPerdida,
            ct);

    private static (VendaPerdidaParaCarga?, LinhaRecusada?) LerVendaPerdida(
        SqlDataReader leitor, string objeto)
    {
        var chave = Chave(leitor, "Questionario");
        var correcoes = new List<CorrecaoAplicada>();
        var rejeicoes = new List<CampoRejeitado>();

        var registradaEm = Data(leitor, "DtaRealizacao", opcional: false);
        if (registradaEm is null)
        {
            return (null, new LinhaRecusada(
                "VendaPerdida", "SemDataDeRegistro", chave,
                "A resposta do formulário não tem data de preenchimento, e sem ela não há como " +
                "situá-la em período nenhum.",
                $"{{\"questionario\":\"{chave}\"}}"));
        }

        return (new VendaPerdidaParaCarga(
            ChaveDeOrigem: chave,
            ChaveDoProcessoNaOrigem: Chave(leitor, "Processo") is { Length: > 0 } p && p != "0" ? p : null,
            ChaveDoClienteNaOrigem: Chave(leitor, "SeqPessoa") is { Length: > 0 } c && c != "0" ? c : null,
            CodigoDaFilialNaOrigem: (int?)Numero(leitor, "NroEmpresa"),
            RegistradaEm: registradaEm.Value.AddHours(HorasDeDiferencaParaUtc),
            OcorridaEm: SaneamentoDeVendaPerdida.DataDaPerda(
                Data(leitor, "OcorridaEm", opcional: true), registradaEm.Value, correcoes),
            Motivo: SaneamentoDaCarga.Texto("motivoDaPerda", Texto(leitor, "Motivo"), 40, correcoes, rejeicoes),
            TipoDoEquipamento: SaneamentoDaCarga.Texto(
                "tipoDoEquipamento", Texto(leitor, "TipoDoEquipamento"), 40, correcoes, rejeicoes),
            MarcaDoConcorrente: SaneamentoDaCarga.Texto(
                "marcaDoConcorrente", Texto(leitor, "MarcaDoConcorrente"), 40, correcoes, rejeicoes),
            RevendaDoConcorrente: SaneamentoDaCarga.Texto(
                "revendaDoConcorrente", Texto(leitor, "RevendaDoConcorrente"), 60, correcoes, rejeicoes),
            ModeloDoConcorrente: SaneamentoDaCarga.Texto(
                "modeloDoConcorrente", Texto(leitor, "ModeloDoConcorrente"), 120, correcoes, rejeicoes),
            ModeloOfertado: SaneamentoDaCarga.Texto(
                "modeloOfertado", Texto(leitor, "ModeloOfertado"), 120, correcoes, rejeicoes),
            Quantidade: SaneamentoDeVendaPerdida.Quantidade(Numero(leitor, "Quantidade"), correcoes),
            PrecoDoConcorrente: SaneamentoDeVendaPerdida.Preco(
                "precoDoConcorrente", Numero(leitor, "PrecoDoConcorrente"), correcoes),
            PrecoOfertado: SaneamentoDeVendaPerdida.Preco(
                "precoOfertado", Numero(leitor, "PrecoOfertado"), correcoes),
            ParticipamosDaNegociacao: SaneamentoDeVendaPerdida.SimNaoOuNada(Texto(leitor, "Participamos")),
            RegistradaPor: SaneamentoDaCarga.Texto(
                "registradaPor", Texto(leitor, "UsuInclusao"), 60, correcoes, rejeicoes),
            Correcoes: correcoes,
            Rejeicoes: rejeicoes), null);
    }
}
