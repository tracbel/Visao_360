using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="Conexao"/> (issue 136).
///
/// <para><b>As do sistema vêm da semente</b>, uma por item de <see cref="ConexoesDoSistema.Todas"/>, com o
/// identificador na ordem do catálogo. As cadastradas pela tela começam em 1000, para a semente crescer sem colidir
/// — a mesma solução dos perfis próprios.</para>
///
/// <para><b>A senha é <c>varbinary</c></b>: o que <c>IProtetorDeSegredos</c> devolveu. Não existe coluna de
/// senha em texto.</para>
/// </summary>
public sealed class ConexaoConfiguracao : IEntityTypeConfiguration<Conexao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Conexao> b)
    {
        b.ToTable("Conexao", "integracao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd().UseIdentityColumn(1000, 1);

        b.Property(c => c.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.Endereco).HasMaxLength(400).IsUnicode(false);
        b.Property(c => c.Banco).HasMaxLength(128).IsUnicode(false);
        b.Property(c => c.Objeto).HasMaxLength(128).IsUnicode(false);
        b.Property(c => c.Usuario).HasMaxLength(128).IsUnicode(true);
        b.Property(c => c.NomeDoCabecalho).HasMaxLength(80).IsUnicode(false);
        b.Property(c => c.SegredoProtegido).HasMaxLength(4000);
        b.Property(c => c.SegredoAlteradoEm).HasPrecision(3);
        b.Property(c => c.UltimaVerificacaoEm).HasPrecision(3);
        b.Property(c => c.UltimaVerificacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.UltimaVerificacaoResumo).HasMaxLength(Conexao.TamanhoDoResumo).IsUnicode(true);

        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_Conexao_Codigo");
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.SegredoAlteradoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Conexao_Tipo", "[Tipo] IN ('ApiRest','SqlServer','MySql','FontePublica','Monitorada')"));
        b.ToTable(x => x.HasCheckConstraint("CK_Conexao_UltimaVerificacao", "[UltimaVerificacao] IN ('NuncaVerificada','NoAr','ComFalha')"));
        b.ToTable(x => x.HasCheckConstraint("CK_Conexao_StatusEsperado", "[StatusEsperado] BETWEEN 100 AND 599"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Conexao_MinutosEntreVerificacoes", "[MinutosEntreVerificacoes] IS NULL OR [MinutosEntreVerificacoes] BETWEEN 15 AND 1440"));

        b.HasData(ConexoesDoSistema.Todas.Select((c, posicao) => new
        {
            Id = posicao + 1,
            c.Codigo,
            c.Nome,
            Descricao = (string?)c.Descricao,
            c.Tipo,
            EhDoSistema = true,
            Endereco = c.Endereco,
            StatusEsperado = 200,
            EstaAtiva = true,
            UltimaVerificacao = SituacaoDaVerificacao.NuncaVerificada
        }));
    }
}

/// <summary>Mapeamento de <see cref="VerificacaoDeConexao"/> — o histórico do botão "Testar".</summary>
public sealed class VerificacaoDeConexaoConfiguracao : IEntityTypeConfiguration<VerificacaoDeConexao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VerificacaoDeConexao> b)
    {
        b.ToTable("VerificacaoDeConexao", "integracao");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedOnAdd();

        b.Property(t => t.VerificadaEm).HasPrecision(3).IsRequired();
        b.Property(t => t.Resumo).HasMaxLength(Conexao.TamanhoDoResumo).IsUnicode(true).IsRequired();

        // "Os últimos testes desta conexão" é a consulta da tela.
        b.HasIndex(t => new { t.ConexaoId, t.VerificadaEm }).IsDescending(false, true);

        b.HasOne<Conexao>().WithMany().HasForeignKey(t => t.ConexaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(t => t.VerificadaPorId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Mapeamento de <see cref="Rotina"/> (issue 136). Uma linha por item de <see cref="RotinasDoSistema.Todas"/>,
/// com a agenda padrão, valendo desde <see cref="RotinasDoSistema.AgendaSemeadaDesde"/>.
/// </summary>
public sealed class RotinaConfiguracao : IEntityTypeConfiguration<Rotina>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Rotina> b)
    {
        b.ToTable("Rotina", "integracao");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedOnAdd();

        b.Property(r => r.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(r => r.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(r => r.Cadencia).HasConversion<string>().HasMaxLength(12).IsUnicode(false).IsRequired();
        b.Property(r => r.AgendaVigenteDesde).HasPrecision(3).IsRequired();
        b.Property(r => r.ExecucaoPedidaEm).HasPrecision(3);
        b.Property(r => r.UltimaExecucaoIniciadaEm).HasPrecision(3);
        b.Property(r => r.UltimaExecucaoTerminadaEm).HasPrecision(3);
        b.Property(r => r.UltimoResultado).HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        b.Property(r => r.UltimaMensagem).HasMaxLength(1000).IsUnicode(true);

        b.HasIndex(r => r.Codigo).IsUnique().HasDatabaseName("UX_Rotina_Codigo");
        b.HasOne<Usuario>().WithMany().HasForeignKey(r => r.ExecucaoPedidaPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_Rotina_Cadencia", "[Cadencia] IN ('Anual','Mensal','Diaria','Intervalo')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Rotina_UltimoResultado", "[UltimoResultado] IS NULL OR [UltimoResultado] IN ('EmAndamento','Sucesso','Falha','Ignorada')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Rotina_Agenda",
            "([Mes] IS NULL OR [Mes] BETWEEN 1 AND 12) AND ([Dia] IS NULL OR [Dia] BETWEEN 1 AND 28) " +
            "AND ([IntervaloMinutos] IS NULL OR [IntervaloMinutos] BETWEEN 15 AND 1440)"));

        b.HasData(RotinasDoSistema.Todas.Select((r, posicao) => new
        {
            Id = posicao + 1,
            r.Codigo,
            r.Nome,
            r.AgendaPadrao.Cadencia,
            r.AgendaPadrao.Mes,
            r.AgendaPadrao.Dia,
            r.AgendaPadrao.Hora,
            r.AgendaPadrao.IntervaloMinutos,
            EstaLigada = r.LigadaPorPadrao,
            AgendaVigenteDesde = RotinasDoSistema.AgendaSemeadaDesde
        }));
    }
}

/// <summary>Mapeamento de <see cref="ExecucaoDeRotina"/> — o histórico do orquestrador.</summary>
public sealed class ExecucaoDeRotinaConfiguracao : IEntityTypeConfiguration<ExecucaoDeRotina>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExecucaoDeRotina> b)
    {
        b.ToTable("ExecucaoDeRotina", "integracao");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.Motivo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.Maquina).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(e => e.IniciadaEm).HasPrecision(3).IsRequired();
        b.Property(e => e.TerminadaEm).HasPrecision(3);
        b.Property(e => e.Resultado).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.Mensagem).HasMaxLength(1000).IsUnicode(true);

        b.HasIndex(e => new { e.RotinaId, e.IniciadaEm }).IsDescending(false, true);

        b.HasOne<Rotina>().WithMany().HasForeignKey(e => e.RotinaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.PedidaPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_ExecucaoDeRotina_Motivo", "[Motivo] IN ('Agenda','Pedido','PrimeiraCarga')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_ExecucaoDeRotina_Resultado", "[Resultado] IN ('EmAndamento','Sucesso','Falha','Ignorada')"));
    }
}
