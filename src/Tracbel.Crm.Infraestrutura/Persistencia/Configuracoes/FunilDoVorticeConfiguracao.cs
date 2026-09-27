using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="EstagioDoProcesso"/> — schema <c>processo</c> (decisões de 27/09/2026, documento 52 §2.1).
///
/// <para><b>O índice único é (processo, estágio)</b>, sem a filial: o número do processo é único no Vórtice inteiro, e
/// uma linha por processo por estágio é a regra. Os dois índices por filial e estágio são os dois recortes do funil — a
/// coorte (pela abertura) e o fluxo (pelo alcance).</para>
/// </summary>
public sealed class EstagioDoProcessoConfiguracao : IEntityTypeConfiguration<EstagioDoProcesso>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EstagioDoProcesso> b)
    {
        b.ToTable("EstagioDoProcesso", "processo");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.NumeroDoProcessoNaOrigem).IsRequired();
        b.Property(e => e.TipoDeProcessoNaOrigem).IsRequired();
        b.Property(e => e.Estagio).HasConversion<string>().HasMaxLength(12).IsUnicode(false).IsRequired();
        b.Property(e => e.Desfecho).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.AbertoEm).HasPrecision(3).IsRequired();
        b.Property(e => e.DesfechoEm).HasPrecision(3);
        b.Property(e => e.AlcancadoEm).HasPrecision(3).IsRequired();
        b.Property(e => e.UltimaAcaoDaEtapaEm).HasPrecision(3);

        b.HasIndex(e => new { e.NumeroDoProcessoNaOrigem, e.Estagio }).IsUnique().HasDatabaseName("UX_EstagioDoProcesso_Numero_Estagio");
        b.HasIndex(e => new { e.EmpresaId, e.Estagio, e.AbertoEm });
        b.HasIndex(e => new { e.EmpresaId, e.Estagio, e.AlcancadoEm });
        b.HasIndex(e => e.ResponsavelId);
        b.HasIndex(e => e.CarteiraId);
        b.HasIndex(e => e.ClienteId);
        b.HasIndex(e => e.ProcessoId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Processo>().WithMany().HasForeignKey(e => e.ProcessoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(e => e.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Carteira>().WithMany().HasForeignKey(e => e.CarteiraId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.ResponsavelId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_EstagioDoProcesso_Estagio",
            "[Estagio] IN ('Lead','Qualificado','Cobertura','Negociacao','Pedido','Faturamento')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_EstagioDoProcesso_Desfecho", "[Desfecho] IN ('Aberto','Suspenso','Ganho','Perdido','Cancelado')"));

        // O FUNIL É DOS PROCESSOS 31, 41 E 50 — os três tipos do extrator do BI. Processo de outro tipo aqui seria outra
        // pergunta respondida com a mesma tabela.
        b.ToTable(x => x.HasCheckConstraint("CK_EstagioDoProcesso_Tipo", "[TipoDeProcessoNaOrigem] IN (31, 41, 50)"));

        // HERDADO SEMPRE DIZ DE QUEM: o selo "herdado do processo nº X (DNA)" não pode ficar sem o X.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_EstagioDoProcesso_Heranca",
            "([HerdadoDoProcessoDna] = 1 AND [NumeroDoProcessoDnaNaOrigem] IS NOT NULL) " +
            "OR ([HerdadoDoProcessoDna] = 0 AND [NumeroDoProcessoDnaNaOrigem] IS NULL)"));
    }
}

/// <summary>
/// Mapeamento de <see cref="ClassificacaoDeResultadoDoVortice"/> — schema <c>integracao</c> (documento 52 §2.2).
///
/// <para><b>Semeada pela migração</b>, uma linha por item de <see cref="ClassificacaoDeResultadoDoVortice.Semente"/>,
/// com o identificador na ordem da semente — código novo entra no fim, como nas rotinas.</para>
/// </summary>
public sealed class ClassificacaoDeResultadoDoVorticeConfiguracao : IEntityTypeConfiguration<ClassificacaoDeResultadoDoVortice>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClassificacaoDeResultadoDoVortice> b)
    {
        b.ToTable("ClassificacaoDeResultadoDoVortice", "integracao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.CodigoNaOrigem).IsRequired();
        b.Property(c => c.Estagio).HasConversion<string>().HasMaxLength(12).IsUnicode(false);
        b.Property(c => c.ContaComoContato).IsRequired();
        b.Property(c => c.Fonte).HasMaxLength(200).IsUnicode(true).IsRequired();

        b.HasIndex(c => c.CodigoNaOrigem).IsUnique().HasDatabaseName("UX_ClassificacaoDeResultadoDoVortice_Codigo");

        b.ToTable(x => x.HasCheckConstraint(
            "CK_ClassificacaoDeResultadoDoVortice_Estagio",
            "[Estagio] IS NULL OR [Estagio] IN ('Lead','Qualificado','Cobertura','Negociacao','Pedido','Faturamento')"));
        b.ToTable(x => x.HasCheckConstraint("CK_ClassificacaoDeResultadoDoVortice_Codigo", "[CodigoNaOrigem] > 0"));

        // O CÓDIGO QUE NÃO PROVA ESTÁGIO NEM CONTA COMO CONTATO não tem por que estar aqui.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_ClassificacaoDeResultadoDoVortice_Utilidade", "[Estagio] IS NOT NULL OR [ContaComoContato] = 1"));

        b.HasData(ClassificacaoDeResultadoDoVortice.Semente.Select((item, posicao) => new
        {
            Id = posicao + 1,
            CodigoNaOrigem = item.Codigo,
            item.Estagio,
            item.ContaComoContato,
            item.Fonte
        }));
    }
}
