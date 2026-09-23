using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// TODO TEXTO SEMEADO CABE NA COLUNA QUE O RECEBE.
///
/// <para><b>O defeito que este teste impede, e que já aconteceu (23/09/2026, issue 74):</b> a semente de
/// uma vigência de parâmetro trazia uma justificativa de 439 caracteres numa coluna de 400. A suíte
/// inteira passou na estação e a migração quebrou no CI, com
/// <c>String or binary data would be truncated</c>.</para>
///
/// <para><b>Por que o local não pegava.</b> Os testes de aplicação e de API rodam em <b>SQLite</b>, que
/// ignora o tamanho declarado de <c>varchar</c> — ele aceita qualquer texto em qualquer coluna. Só o
/// SQL Server recusa, e no CI ele só é exercitado pelos testes de contêiner, que levam minutos e
/// dependem do contêiner subir.</para>
///
/// <para><b>Este teste lê o MODELO</b>, sem banco nenhum: ele compara cada valor semeado por
/// <c>HasData</c> com o <c>MaxLength</c> da propriedade. Roda em milissegundos e falha na hora de
/// escrever a semente, e não vinte minutos depois.</para>
/// </summary>
[Trait("Categoria", "Arquitetura")]
public sealed class SementeCabeNaColunaTestes
{
    [Fact]
    public void Todo_texto_semeado_cabe_no_tamanho_declarado_da_coluna()
    {
        var estouros = new List<string>();
        var conferidos = 0;

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var limites = tabela.GetProperties()
                .Where(p => p.ClrType == typeof(string) && p.GetMaxLength() is > 0)
                .ToDictionary(p => p.Name, p => p.GetMaxLength()!.Value);

            if (limites.Count == 0) continue;

            foreach (var linha in tabela.GetSeedData())
            {
                foreach (var (coluna, limite) in limites)
                {
                    if (!linha.TryGetValue(coluna, out var valor) || valor is not string texto) continue;

                    conferidos++;
                    if (texto.Length <= limite) continue;

                    estouros.Add(
                        $"{tabela.GetSchema()}.{tabela.GetTableName()}.{coluna}: " +
                        $"{texto.Length} caracteres numa coluna de {limite} — \"{texto[..Math.Min(60, texto.Length)]}…\"");
                }
            }
        }

        estouros.Should().BeEmpty(
            "texto semeado maior que a coluna passa no SQLite e quebra a migração no SQL Server, com " +
            "'String or binary data would be truncated'. Encurte a semente — o detalhe longo mora no documento.");

        // UM TESTE QUE NÃO OLHOU NADA É PIOR QUE TESTE NENHUM: ele fica verde e dá a impressão de guardar
        // alguma coisa. Se a leitura da semente parar de funcionar — API do EF mudando, configuração
        // movendo —, é aqui que se descobre.
        conferidos.Should().BeGreaterThan(50,
            "o modelo tem dezenas de textos semeados (perfis, permissões, catálogo de culturas, parâmetros); " +
            "zero ou pouquíssimos significa que este teste deixou de enxergar a semente");
    }
}
