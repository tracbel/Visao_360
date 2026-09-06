using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Tracbel.Crm.Dominio.Metadado;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Padrão de banco — a chave estrangeira COMPOSTA do catálogo genérico.
///
/// Cobre a seção 8.11 do documento 17: *"A integridade referencial se mantém com
/// `UNIQUE (CatalogoId, Id)` em `CatalogoItem` e FK composta na tabela que aponta, com a
/// coluna `CatalogoId` persistida como constante — assim `Contato.PapelId` só aceita item do
/// catálogo `PAPEL_CONTATO`, verificado pelo banco."*
///
/// POR QUE ESTE ARQUIVO EXISTE: a auditoria do documento 21 (achado I-1, gravidade "impede")
/// encontrou a chave alternativa criada e **nenhuma** das onze chaves estrangeiras usando-a —
/// todas de coluna única. `ClienteContato.PapelId` aceitava um item do catálogo `CULTURA`;
/// `Endereco.CulturaId` aceitava um `CONCORRENTE`. O pior detalhe do achado é que a chave
/// alternativa existente fazia o mecanismo **parecer** implementado para quem lê o catálogo
/// do banco. Este teste é o que impede a distância entre o documento e o banco de reabrir:
/// coluna nova que aponte para `metadado.CatalogoItem` sem chave composta não passa.
/// </summary>
[Trait("Categoria", "PadraoDeBanco")]
public sealed class CatalogoDeSistemaTestes
{
    /// <summary>
    /// As referências a <c>metadado.CatalogoItem</c> que são GENÉRICAS por desenho — o
    /// catálogo não é constante da coluna, e por isso não há constante para persistir.
    ///
    /// Mesmo mecanismo de <c>IntegridadeReferencialTestes.CascadeJustificado</c>: a exceção
    /// existe, tem nome e tem motivo escrito. Se alguém acrescentar uma coluna de papel e
    /// tentar escapar por aqui, a justificativa fica no diff, para ser discutida no PR.
    /// </summary>
    private static readonly Dictionary<(string Tabela, string Coluna), string> ReferenciaGenericaJustificada = new()
    {
        [("Resposta", "CatalogoItemId")] =
            "a resposta de formulário aponta para o item de QUALQUER catálogo: qual catálogo " +
            "vale é decidido por Pergunta.CatalogoId, linha a linha, e o banco já exige esse " +
            "vínculo em CK_Pergunta_Catalogo. Não existe constante para persistir aqui; " +
            "torná-la composta obrigaria a copiar o catálogo da pergunta para dentro de " +
            "metadado.Resposta, que é justamente a denormalização que a regra 4 do documento " +
            "17 proíbe"
    };

    [Fact]
    public void Toda_coluna_que_aponta_para_item_de_catalogo_usa_chave_estrangeira_composta()
    {
        var foraDoPadrao = new List<string>();

        foreach (var (tabela, fk) in ReferenciasAoItemDeCatalogo())
        {
            var colunas = fk.Properties.Select(p => p.GetColumnName()).ToList();

            if (fk.Properties.Count == 1)
            {
                var coluna = colunas[0];
                if (ReferenciaGenericaJustificada.ContainsKey((tabela.GetTableName()!, coluna))) continue;

                foraDoPadrao.Add(
                    $"{tabela.GetTableName()}.{coluna} (chave estrangeira de coluna única — " +
                    "aceita item de QUALQUER catálogo)");
                continue;
            }

            var chavePrincipal = fk.PrincipalKey.Properties.Select(p => p.Name).ToList();
            if (!chavePrincipal.SequenceEqual([nameof(CatalogoItem.CatalogoId), nameof(CatalogoItem.Id)]))
                foraDoPadrao.Add(
                    $"{tabela.GetTableName()}.{string.Join("+", colunas)} aponta para " +
                    $"({string.Join(", ", chavePrincipal)}) — o esperado é " +
                    "(CatalogoId, Id), a chave alternativa AK_CatalogoItem_CatalogoId");
        }

        foraDoPadrao.Should().BeEmpty(
            "toda coluna que aponta para metadado.CatalogoItem precisa de chave estrangeira " +
            "COMPOSTA (CatalogoDoPapelId, PapelId) contra AK_CatalogoItem_CatalogoId — é o " +
            "mecanismo da seção 8.11 do documento 17, e é o que impede o campo de papel do " +
            "contato de aceitar um item do catálogo de culturas. Exceção genérica só com " +
            "entrada em ReferenciaGenericaJustificada. Fora do padrão: {0}",
            string.Join("; ", foraDoPadrao));
    }

    [Fact]
    public void A_coluna_de_catalogo_da_chave_composta_e_uma_constante_presa_pelo_banco()
    {
        // Chave composta sem constante não restringe nada: bastaria gravar outro número na
        // coluna de catálogo para a chave estrangeira voltar a aceitar item de qualquer lista.
        // A constante mora em DOIS lugares do banco — valor padrão (para quem não escreve a
        // coluna) e restrição de verificação (para quem escreve) — e nos dois tem que ser o
        // MESMO número de CatalogosDeSistema.
        var identificadoresValidos = CatalogosDeSistema.Todos.Select(c => c.Id).ToHashSet();
        var problemas = new List<string>();

        foreach (var (tabela, fk) in ReferenciasAoItemDeCatalogo())
        {
            if (fk.Properties.Count != 2) continue;

            var colunaDeCatalogo = fk.Properties[0];
            var nomeDaTabela = tabela.GetTableName()!;
            var nomeDaColuna = colunaDeCatalogo.GetColumnName();

            // A hierarquia do próprio catálogo é o caso em que a coluna de catálogo NÃO é
            // constante: o pai de um item mora no mesmo catálogo que o filho, seja ele qual for.
            if (tabela.ClrType == typeof(CatalogoItem) && nomeDaColuna == nameof(CatalogoItem.CatalogoId))
                continue;

            var padrao = colunaDeCatalogo.GetDefaultValue();
            if (padrao is not int constante || !identificadoresValidos.Contains(constante))
            {
                problemas.Add(
                    $"{nomeDaTabela}.{nomeDaColuna}: valor padrão '{padrao ?? "(nenhum)"}' não é " +
                    "uma das constantes de CatalogosDeSistema");
                continue;
            }

            var esperado = $"[{nomeDaColuna}] = {constante}";
            var temCheck = tabela.GetCheckConstraints()
                .Any(c => c.Sql.Replace(" ", string.Empty, StringComparison.Ordinal)
                    == esperado.Replace(" ", string.Empty, StringComparison.Ordinal));

            if (!temCheck)
                problemas.Add($"{nomeDaTabela}.{nomeDaColuna}: falta a restrição '{esperado}'");
        }

        problemas.Should().BeEmpty(
            "a coluna de catálogo de uma chave estrangeira composta é CONSTANTE: valor padrão " +
            "no banco mais restrição de verificação que só aceita aquele número, e o número " +
            "vem de Dominio.Metadado.CatalogosDeSistema (documento 17, seção 8.11). " +
            "Problemas: {0}",
            string.Join("; ", problemas));
    }

    [Fact]
    public void Os_catalogos_de_sistema_nascem_semeados_com_o_identificador_que_o_esquema_usa()
    {
        // A constante da restrição de verificação aponta para uma LINHA de metadado.Catalogo.
        // Se essa linha não nascer com a migração, nenhuma das nove chaves compostas pode ser
        // satisfeita — o campo de papel do contato ficaria impossível de preencher.
        //
        // [V] é a lição de IV_SegPerfil, a tabela que resolveria os perfis de permissão do
        // Vórtice e que está VAZIA: catálogo que o esquema pressupõe e ninguém semeia.
        var semeados = ModeloBanco.Modelo
            .FindEntityType(typeof(Catalogo))!
            .GetSeedData()
            .Select(linha => (
                Id: Convert.ToInt32(linha[nameof(Catalogo.Id)], System.Globalization.CultureInfo.InvariantCulture),
                Codigo: (string)linha[nameof(Catalogo.Codigo)]!))
            .OrderBy(c => c.Id)
            .ToList();

        var esperados = CatalogosDeSistema.Todos
            .Select(c => (c.Id, c.Codigo))
            .OrderBy(c => c.Id)
            .ToList();

        semeados.Should().BeEquivalentTo(esperados,
            "os oito catálogos de sistema nascem com a migração, com o identificador fixo que " +
            "as restrições de verificação das nove colunas de papel citam literalmente " +
            "(Dominio.Metadado.CatalogosDeSistema)");
    }

    private static IEnumerable<(IEntityType Tabela, IForeignKey Fk)> ReferenciasAoItemDeCatalogo()
        => from tabela in ModeloBanco.Modelo.GetEntityTypes()
           from fk in tabela.GetForeignKeys()
           where fk.PrincipalEntityType.ClrType == typeof(CatalogoItem)
           select (tabela, fk);
}
