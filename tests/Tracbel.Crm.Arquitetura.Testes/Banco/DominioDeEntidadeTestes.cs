using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Padrão de banco — o domínio do PONTEIRO POLIMÓRFICO.
///
/// Cobre a regra 2 da seção 6.2 do documento 17 (*"todo domínio fechado curto tem `CHECK`"*) e
/// a última linha da seção 6 do documento 14 (*"nenhuma coluna de texto livre onde existe (ou
/// deveria existir) um catálogo"*).
///
/// POR QUE ESTE ARQUIVO EXISTE: a auditoria do documento 21 (achado I-3, gravidade "impede")
/// encontrou `Entidade` como `varchar(40)` livre em dez colunas, e em cinco delas
/// `RegistroId` não tem — nem pode ter — chave estrangeira, porque o alvo muda de tabela linha
/// a linha. Com o domínio aberto, `Cliente` e `Clientes` eram registros diferentes para o
/// banco: o documento órfão do Vórtice (5.302, 6,9% do total) e o compartilhamento perdido
/// voltavam a ser estruturalmente possíveis, só que com nomes melhores.
///
/// O documento 14, seção 6, marcava esta regra como *"não é mecanicamente detectável"*. Para
/// `Entidade` ela É: o conjunto válido é exatamente o conjunto de entidades mapeadas no
/// `CrmDbContext`, e é daí que a restrição é gerada. Este teste é a outra ponta — ele compara
/// a lista que está no banco com a lista que está no modelo, e falha quando divergirem. É por
/// ele que entidade nova sem migração que regenere o `CHECK` não passa no `dotnet test`.
/// </summary>
[Trait("Categoria", "PadraoDeBanco")]
public sealed class DominioDeEntidadeTestes
{
    /// <summary>As colunas que nomeiam uma entidade do modelo.</summary>
    private static readonly string[] ColunasDeEntidade = ["Entidade", "EntidadeRaiz"];

    [Fact]
    public void Toda_coluna_que_nomeia_entidade_tem_o_dominio_fechado_pela_lista_do_modelo()
    {
        var esperado = ModeloBanco.Modelo.GetEntityTypes()
            .Select(t => t.ClrType.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var problemas = new List<string>();
        var encontradas = 0;

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var checks = tabela.GetCheckConstraints().ToList();

            foreach (var propriedade in tabela.GetProperties())
            {
                var coluna = propriedade.GetColumnName();
                if (!ColunasDeEntidade.Contains(coluna, StringComparer.Ordinal)) continue;

                encontradas++;
                var nomeEsperado = $"CK_{tabela.GetTableName()}_{coluna}";
                var check = checks.SingleOrDefault(c => c.Name == nomeEsperado);

                if (check is null)
                {
                    problemas.Add($"{tabela.GetTableName()}.{coluna}: falta a restrição {nomeEsperado}");
                    continue;
                }

                // A comparação PRECISA ser binária: sob a colação do banco (CI_AI, de
                // propósito), IN ('Cliente') aceitaria 'CLIENTE' e 'cliénte'. Aqui o valor é
                // identificador de máquina, não texto de gente.
                if (!check.Sql.Contains(CrmDbContext.ColacaoBinaria, StringComparison.Ordinal))
                    problemas.Add(
                        $"{tabela.GetTableName()}.{coluna}: a restrição não compara com " +
                        $"COLLATE {CrmDbContext.ColacaoBinaria}");

                var listados = Regex.Matches(check.Sql, "'([^']*)'")
                    .Select(m => m.Groups[1].Value)
                    .OrderBy(n => n, StringComparer.Ordinal)
                    .ToList();

                if (!listados.SequenceEqual(esperado, StringComparer.Ordinal))
                {
                    var faltando = esperado.Except(listados, StringComparer.Ordinal).ToList();
                    var sobrando = listados.Except(esperado, StringComparer.Ordinal).ToList();
                    problemas.Add(
                        $"{tabela.GetTableName()}.{coluna}: lista divergente do modelo " +
                        $"(faltando: {string.Join(", ", faltando)}; " +
                        $"sobrando: {string.Join(", ", sobrando)})");
                }
            }
        }

        encontradas.Should().Be(2,
            "são DUAS as colunas que nomeiam entidade depois da fase 1 (documento 41): " +
            "auditoria.AlteracaoDeCampo.Entidade e integracao.ChaveExterna.Entidade. Eram dez — " +
            "as outras oito estavam em tabelas vazias que saíram (documento 21, achado I-3). Se " +
            "este número mudou, a coluna nova precisa entrar na conta, não passar despercebida");

        problemas.Should().BeEmpty(
            "toda coluna que nomeia uma entidade tem restrição de verificação com a lista " +
            "FECHADA das entidades do modelo, comparada sem tolerância a caixa nem acento — é " +
            "a única defesa que sobra num ponteiro polimórfico, onde RegistroId não pode ter " +
            "chave estrangeira. Problemas: {0}",
            string.Join("; ", problemas));
    }

    [Fact]
    public void Toda_coluna_que_nomeia_campo_exige_identificador_do_padrao_de_nomenclatura()
    {
        // `Campo` NÃO ganha lista fechada, e o motivo está escrito em
        // CrmDbContext.FecharDominioDoPonteiroPolimorfico: o conjunto válido seria toda coluna do
        // modelo, o que faria qualquer coluna nova em qualquer tabela exigir migração aqui.
        //
        // O que dá para exigir sem mentir é a FORMA: identificador PascalCase ASCII, o padrão
        // da seção 3 do documento 14. Barra 'nome_cliente', 'Nome do cliente' e ' Nome'.
        var problemas = new List<string>();
        var encontradas = 0;

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            foreach (var propriedade in tabela.GetProperties())
            {
                if (propriedade.GetColumnName() != "Campo") continue;

                encontradas++;
                var nomeEsperado = $"CK_{tabela.GetTableName()}_Campo";
                var check = tabela.GetCheckConstraints().SingleOrDefault(c => c.Name == nomeEsperado);

                if (check is null)
                {
                    problemas.Add($"{tabela.GetTableName()}.Campo: falta a restrição {nomeEsperado}");
                    continue;
                }

                if (!check.Sql.Contains(CrmDbContext.ColacaoBinaria, StringComparison.Ordinal)
                    || !check.Sql.Contains("[A-Z]", StringComparison.Ordinal)
                    || !check.Sql.Contains("%[^A-Za-z0-9]%", StringComparison.Ordinal))
                    problemas.Add(
                        $"{tabela.GetTableName()}.Campo: a restrição não exige identificador " +
                        $"PascalCase ASCII comparado com COLLATE {CrmDbContext.ColacaoBinaria} " +
                        $"(está: {check.Sql})");
            }
        }

        encontradas.Should().Be(1,
            "sobrou UMA coluna chamada 'Campo' depois da fase 1 (documento 41): " +
            "auditoria.AlteracaoDeCampo. As outras três — auditoria.CampoAuditado, " +
            "metadado.CampoPersonalizado e relatorio.FonteCampo — estavam em tabelas vazias que " +
            "saíram (documento 21, achado I-3)");

        problemas.Should().BeEmpty(
            "coluna que nomeia campo aceita identificador do padrão de nomenclatura, e nada " +
            "além disso. Problemas: {0}",
            string.Join("; ", problemas));
    }
}
