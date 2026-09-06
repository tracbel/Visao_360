using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Metadado;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// A chave estrangeira COMPOSTA que amarra uma coluna a UM catálogo — o mecanismo da seção
/// 8.11 do documento 17, escrito uma vez e reusado pelas nove colunas de papel.
///
/// O QUE ESTAVA ERRADO ANTES: a chave alternativa <c>AK_CatalogoItem_CatalogoId</c>
/// <c>(CatalogoId, Id)</c> existia, mas as chaves estrangeiras que apontavam para
/// <c>metadado.CatalogoItem</c> eram todas de coluna única, contra <c>CatalogoItem.Id</c>.
/// Na prática <c>ClienteContato.PapelId</c> aceitava um item do catálogo <c>CULTURA</c>, e
/// <c>Endereco.CulturaId</c> aceitava um <c>CONCORRENTE</c> — o domínio fechado que o
/// documento promete só existia no texto. E a existência da chave alternativa fazia o
/// mecanismo PARECER implementado para quem lê o catálogo do banco, o que é pior do que não
/// existir (auditoria do documento 21, achado I-1).
///
/// COMO FUNCIONA: a tabela que aponta ganha uma coluna de catálogo com valor CONSTANTE —
/// valor padrão no banco e restrição de verificação que só aceita aquele número — e a chave
/// estrangeira passa a ser <c>(CatalogoDoPapelId, PapelId)</c> contra
/// <c>(CatalogoItem.CatalogoId, CatalogoItem.Id)</c>. Gravar um item do catálogo errado deixa
/// de ser possível: o par não existe do outro lado.
///
/// A COLUNA É SOMBRA (shadow property): ela não aparece na entidade de domínio porque não é
/// informação de negócio — é a metade estrutural de uma chave estrangeira, e o valor dela
/// nunca varia. A aplicação não a escreve; o valor padrão do banco a preenche.
///
/// QUANDO A COLUNA DE ITEM É ANULÁVEL: o SQL Server não verifica chave estrangeira composta
/// quando qualquer coluna do par é nula, então <c>CulturaId IS NULL</c> continua sendo um
/// endereço sem cultura. A restrição só morde quando há item escolhido — que é exatamente
/// quando ela precisa morder.
/// </summary>
internal static class LigacaoDeCatalogo
{
    /// <summary>
    /// Liga <paramref name="colunaDoItem"/> ao catálogo <paramref name="catalogoId"/> por chave
    /// estrangeira composta, criando a coluna de catálogo constante
    /// <paramref name="colunaDoCatalogo"/>, o índice que cobre a chave e a restrição de
    /// verificação que prende o valor.
    /// </summary>
    /// <typeparam name="T">A entidade que aponta para o catálogo.</typeparam>
    /// <param name="b">O construtor da entidade. <c>ToTable</c> já precisa ter sido chamado.</param>
    /// <param name="colunaDoItem">A coluna de papel — <c>PapelId</c>, <c>CulturaId</c>...</param>
    /// <param name="colunaDoCatalogo">A coluna de catálogo constante — <c>CatalogoDoPapelId</c>...</param>
    /// <param name="catalogoId">Uma das constantes de <see cref="CatalogosDeSistema"/>.</param>
    public static EntityTypeBuilder<T> LigarAoCatalogoDeSistema<T>(
        this EntityTypeBuilder<T> b,
        string colunaDoItem,
        string colunaDoCatalogo,
        int catalogoId)
        where T : class
    {
        var tabela = b.Metadata.GetTableName()
            ?? throw new InvalidOperationException(
                $"Chame ToTable antes de LigarAoCatalogoDeSistema em {typeof(T).Name}.");

        b.Property<int>(colunaDoCatalogo).HasDefaultValue(catalogoId);

        // O índice que cobre a chave estrangeira composta (documento 14, seção 7). Substitui o
        // índice de coluna única que existia antes: com a coluna de catálogo constante, este
        // índice tem exatamente a mesma seletividade, quatro bytes mais largo.
        b.HasIndex(colunaDoCatalogo, colunaDoItem);

        b.HasOne<CatalogoItem>()
            .WithMany()
            .HasForeignKey(colunaDoCatalogo, colunaDoItem)
            .HasPrincipalKey(nameof(CatalogoItem.CatalogoId), nameof(CatalogoItem.Id))
            .OnDelete(DeleteBehavior.Restrict);

        // A CONSTANTE, dita ao banco. Sem isto, alguém poderia gravar outro número na coluna de
        // catálogo e a chave estrangeira passaria a aceitar item de qualquer catálogo de novo.
        b.ToTable(t => t.HasCheckConstraint(
            $"CK_{tabela}_{colunaDoCatalogo}", $"[{colunaDoCatalogo}] = {catalogoId}"));

        return b;
    }
}
