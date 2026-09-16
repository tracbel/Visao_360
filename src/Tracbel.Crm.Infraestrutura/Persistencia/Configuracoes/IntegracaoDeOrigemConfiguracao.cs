using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="LinhaDeProduto"/> — schema <c>frota</c> (documento 35, seção 10).
/// </summary>
public sealed class LinhaDeProdutoConfiguracao : IEntityTypeConfiguration<LinhaDeProduto>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LinhaDeProduto> b)
    {
        b.ToTable("LinhaDeProduto", "frota");
        b.HasKey(l => l.Id);
        b.Property(l => l.Id).ValueGeneratedOnAdd();
        b.Property(l => l.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(l => l.Nome).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(l => l.Porte).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(l => l.EstaAtiva).IsRequired();

        b.HasIndex(l => l.Codigo).IsUnique().HasDatabaseName("UX_LinhaDeProduto_Codigo");
        b.HasIndex(l => l.FamiliaId);

        b.HasOne<Familia>().WithMany().HasForeignKey(l => l.FamiliaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_LinhaDeProduto_Porte", "[Porte] IN ('NaoSeAplica','Pequeno','Medio','Grande')"));
    }
}

/// <summary>
/// Mapeamento de <see cref="VendaDeMaquina"/>. A unicidade por sistema e chave de origem é o que
/// impede a recarga de duplicar a venda.
/// </summary>
public sealed class VendaDeMaquinaConfiguracao : IEntityTypeConfiguration<VendaDeMaquina>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VendaDeMaquina> b)
    {
        b.ToTable("VendaDeMaquina", "frota");
        b.HasKey(v => v.Id);
        b.Property(v => v.Id).ValueGeneratedOnAdd();

        b.Property(v => v.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(v => v.ChavePublica).IsUnique().HasDatabaseName("UX_VendaDeMaquina_ChavePublica");

        b.Property(v => v.ChaveOrigem).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(v => v.NumeroDoPedido).HasMaxLength(20).IsUnicode(false);
        b.Property(v => v.NumeroDaNotaFiscal).HasMaxLength(60).IsUnicode(false);
        b.Property(v => v.SituacaoNaOrigem).HasMaxLength(10).IsUnicode(false);
        b.Property(v => v.GestaoNaOrigem).HasMaxLength(30).IsUnicode(true);
        b.Property(v => v.LinhaNaOrigem).HasMaxLength(60).IsUnicode(true).IsRequired();
        b.Property(v => v.ProdutoNaOrigem).HasMaxLength(60).IsUnicode(true).IsRequired();
        b.Property(v => v.EmpresaNaOrigem).HasMaxLength(60).IsUnicode(true);
        b.Property(v => v.UnidadeNaOrigem).HasMaxLength(80).IsUnicode(true);
        b.Property(v => v.UnidadeDoFaturamentoNaOrigem).HasMaxLength(80).IsUnicode(true);
        b.Property(v => v.HashDaOrigem).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(v => v.Transformacoes).HasMaxLength(1000).IsUnicode(true);
        b.Property(v => v.VendaDireta).IsRequired();
        b.Property(v => v.RepasseDireto).IsRequired();

        b.Property(v => v.RegistradaNaOrigemEm).HasPrecision(3);
        b.Property(v => v.ImportadaEm).HasPrecision(3).IsRequired();
        b.Property(v => v.AtualizadaPelaOrigemEm).HasPrecision(3);
        b.Property(v => v.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(v => v.AlteradoEm).HasPrecision(3);
        b.Property(v => v.ExcluidoEm).HasPrecision(3);

        b.HasIndex(v => new { v.SistemaId, v.ChaveOrigem })
            .IsUnique()
            .HasDatabaseName("UX_VendaDeMaquina_Sistema_ChaveOrigem");

        b.HasIndex(v => new { v.EquipamentoId, v.VendidaEm });
        b.HasIndex(v => v.CompradorId);
        b.HasIndex(v => v.EmpresaId);
        b.HasIndex(v => v.EmpresaDoFaturamentoId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(v => v.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Empresa>().WithMany().HasForeignKey(v => v.EmpresaDoFaturamentoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipamento>().WithMany().HasForeignKey(v => v.EquipamentoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(v => v.CompradorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(v => v.SistemaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_VendaDeMaquina_Quantidade", "[Quantidade] IS NULL OR [Quantidade] >= 0"));

        b.Ignore(v => v.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="VinculoDeClienteComEquipamento"/>.</summary>
public sealed class VinculoDeClienteComEquipamentoConfiguracao : IEntityTypeConfiguration<VinculoDeClienteComEquipamento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VinculoDeClienteComEquipamento> b)
    {
        b.ToTable("VinculoDeClienteComEquipamento", "frota");
        b.HasKey(v => v.Id);
        b.Property(v => v.Id).ValueGeneratedOnAdd();

        b.Property(v => v.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(v => v.ChavePublica).IsUnique().HasDatabaseName("UX_VinculoDeClienteComEquipamento_ChavePublica");

        b.Property(v => v.Natureza).HasConversion<string>().HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(v => v.MotivoDoEncerramento).HasMaxLength(200).IsUnicode(true);
        b.Property(v => v.EncerradoEm).HasPrecision(3);
        b.Property(v => v.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(v => v.AlteradoEm).HasPrecision(3);
        b.Property(v => v.ExcluidoEm).HasPrecision(3);

        // Um comprador vigente por venda: a recarga reencontra o vínculo em vez de criar outro.
        b.HasIndex(v => new { v.VendaDeMaquinaId, v.ClienteId, v.Natureza })
            .IsUnique()
            .HasFilter("[VendaDeMaquinaId] IS NOT NULL AND [EncerradoEm] IS NULL")
            .HasDatabaseName("UX_VinculoDeClienteComEquipamento_Venda_Cliente_Natureza");

        b.HasIndex(v => new { v.EquipamentoId, v.Natureza });
        b.HasIndex(v => v.ClienteId);
        b.HasIndex(v => v.EmpresaId);
        b.HasIndex(v => v.SistemaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(v => v.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipamento>().WithMany().HasForeignKey(v => v.EquipamentoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<VendaDeMaquina>().WithMany().HasForeignKey(v => v.VendaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(v => v.SistemaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_VinculoDeClienteComEquipamento_Natureza", "[Natureza] IN ('CompradorNaVenda')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_VinculoDeClienteComEquipamento_Encerramento",
            "([EncerradoEm] IS NULL AND [MotivoDoEncerramento] IS NULL) OR ([EncerradoEm] IS NOT NULL AND [MotivoDoEncerramento] IS NOT NULL)"));

        b.Ignore(v => v.Eventos);
    }
}

/// <summary>
/// Mapeamento de <see cref="CorrespondenciaDaOrigem"/>. Uma linha por sistema, tipo e valor da origem.
/// </summary>
public sealed class CorrespondenciaDaOrigemConfiguracao : IEntityTypeConfiguration<CorrespondenciaDaOrigem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CorrespondenciaDaOrigem> b)
    {
        b.ToTable("CorrespondenciaDaOrigem", "integracao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.CodigoNaOrigem).HasMaxLength(80).IsUnicode(false).IsRequired();
        b.Property(c => c.TextoNaOrigem).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.ContextoNaOrigem).HasMaxLength(120).IsUnicode(true);
        b.Property(c => c.Situacao).HasConversion<string>().HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(c => c.Criterio).HasMaxLength(400).IsUnicode(true).IsRequired();
        b.Property(c => c.Ocorrencias).IsRequired();
        b.Property(c => c.PrimeiraLeituraEm).HasPrecision(3).IsRequired();
        b.Property(c => c.UltimaLeituraEm).HasPrecision(3).IsRequired();
        b.Property(c => c.RevisadaEm).HasPrecision(3);

        b.HasIndex(c => new { c.SistemaId, c.Tipo, c.CodigoNaOrigem })
            .IsUnique()
            .HasDatabaseName("UX_CorrespondenciaDaOrigem_Sistema_Tipo_Codigo");

        b.HasIndex(c => c.LinhaDeProdutoId);
        b.HasIndex(c => c.ModeloId);
        b.HasIndex(c => c.EmpresaCorrespondenteId);
        b.HasIndex(c => c.RevisadaPorId);

        b.HasOne<Sistema>().WithMany().HasForeignKey(c => c.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LinhaDeProduto>().WithMany().HasForeignKey(c => c.LinhaDeProdutoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Modelo>().WithMany().HasForeignKey(c => c.ModeloId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaCorrespondenteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.RevisadaPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_CorrespondenciaDaOrigem_Tipo", "[Tipo] IN ('LinhaDeProduto','Produto','Unidade')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CorrespondenciaDaOrigem_Situacao",
            "[Situacao] IN ('CorrespondenciaExata','PendenteDeRevisao','NaoEClassificacaoDeProduto','ConfirmadaPorRevisao','RecusadaPorRevisao')"));
        b.ToTable(x => x.HasCheckConstraint("CK_CorrespondenciaDaOrigem_Ocorrencias", "[Ocorrencias] >= 0"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CorrespondenciaDaOrigem_Revisao",
            "([RevisadaEm] IS NULL AND [RevisadaPorId] IS NULL) OR ([RevisadaEm] IS NOT NULL AND [RevisadaPorId] IS NOT NULL)"));
    }
}

/// <summary>Mapeamento de <see cref="RegistroDeOrigem"/> — a trilha de cada registro lido.</summary>
public sealed class RegistroDeOrigemConfiguracao : IEntityTypeConfiguration<RegistroDeOrigem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RegistroDeOrigem> b)
    {
        b.ToTable("RegistroDeOrigem", "integracao");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedOnAdd();

        b.Property(r => r.Fluxo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(r => r.ChaveOrigem).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(r => r.HashDoConteudo).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(r => r.ChassiNaOrigem).HasMaxLength(200).IsUnicode(true);
        b.Property(r => r.LinhaNaOrigem).HasMaxLength(60).IsUnicode(true);
        b.Property(r => r.ProdutoNaOrigem).HasMaxLength(60).IsUnicode(true);
        b.Property(r => r.UnidadeNaOrigem).HasMaxLength(80).IsUnicode(true);
        b.Property(r => r.Transformacoes).HasMaxLength(1000).IsUnicode(true);
        b.Property(r => r.Decisao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(r => r.Motivos).HasMaxLength(400).IsUnicode(false);
        b.Property(r => r.Leituras).IsRequired();
        b.Property(r => r.PrimeiraLeituraEm).HasPrecision(3).IsRequired();
        b.Property(r => r.UltimaLeituraEm).HasPrecision(3).IsRequired();
        b.Property(r => r.ConteudoAlteradoEm).HasPrecision(3);
        b.Property(r => r.AusenteNaOrigemDesde).HasPrecision(3);

        b.HasIndex(r => new { r.SistemaId, r.Fluxo, r.ChaveOrigem })
            .IsUnique()
            .HasDatabaseName("UX_RegistroDeOrigem_Sistema_Fluxo_Chave");

        b.HasIndex(r => r.VendaDeMaquinaId);
        b.HasIndex(r => new { r.Fluxo, r.Decisao });

        b.HasOne<Sistema>().WithMany().HasForeignKey(r => r.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<VendaDeMaquina>().WithMany().HasForeignKey(r => r.VendaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_RegistroDeOrigem_Decisao", "[Decisao] IN ('Importado','Pendente')"));
        b.ToTable(x => x.HasCheckConstraint("CK_RegistroDeOrigem_Leituras", "[Leituras] >= 1"));
    }
}

/// <summary>Mapeamento de <see cref="CompradorPendente"/> — a fila de compradores ausentes do CRM.</summary>
public sealed class CompradorPendenteConfiguracao : IEntityTypeConfiguration<CompradorPendente>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CompradorPendente> b)
    {
        b.ToTable("CompradorPendente", "integracao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(c => c.ChavePublica).IsUnique().HasDatabaseName("UX_CompradorPendente_ChavePublica");

        b.Property(c => c.Documento)
            .HasConversion(d => d.Numero, s => CpfCnpj.Criar(s))
            .HasMaxLength(14).IsUnicode(false).IsRequired();
        b.Property(c => c.TipoDePessoa).HasConversion<string>().HasMaxLength(10).IsUnicode(false).IsRequired();
        b.Property(c => c.NomeNaOrigem).HasMaxLength(100).IsUnicode(true).IsRequired();
        b.Property(c => c.Grupo).HasConversion<string>().HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(c => c.Situacao).HasConversion<string>().HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(c => c.FiliaisDasVendas).HasMaxLength(200).IsUnicode(false).IsRequired();
        b.Property(c => c.NaturezaNasNotas).HasMaxLength(40).IsUnicode(false);
        b.Property(c => c.SituacaoNoCadastroDoProtheus).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.DadosQueFaltam).HasMaxLength(400).IsUnicode(true).IsRequired();
        b.Property(c => c.ApuradoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.AlteradoEm).HasPrecision(3);
        b.Property(c => c.ExcluidoEm).HasPrecision(3);

        b.HasIndex(c => new { c.SistemaId, c.Documento })
            .IsUnique()
            .HasDatabaseName("UX_CompradorPendente_Sistema_Documento");
        b.HasIndex(c => new { c.EmpresaId, c.Grupo, c.Situacao });

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(c => c.SistemaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_CompradorPendente_TipoDePessoa", "[TipoDePessoa] IN ('Fisica','Juridica')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CompradorPendente_Documento",
            "([TipoDePessoa] = 'Fisica' AND LEN([Documento]) = 11) OR ([TipoDePessoa] = 'Juridica' AND LEN([Documento]) = 14)"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CompradorPendente_Grupo", "[Grupo] IN ('ComNotaNoProtheus','SemNotaNoProtheus')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CompradorPendente_Situacao", "[Situacao] IN ('AguardandoCadastro','Cadastrado')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CompradorPendente_SituacaoNoCadastroDoProtheus",
            "[SituacaoNoCadastroDoProtheus] IN ('NaoConferido','Ausente','Ativo','Bloqueado')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CompradorPendente_Contadores",
            "[Vendas] >= 0 AND [VendasComChassiValido] >= 0 AND [VendasComChassiValido] <= [Vendas] AND [NotasNoProtheus] >= 0"));

        b.Ignore(c => c.Eventos);
    }
}

/// <summary>
/// Mapeamento de <see cref="ExecucaoDeSincronizacao"/> — o registro de cada execução do serviço de
/// sincronização (documento 35, seção 11).
/// </summary>
public sealed class ExecucaoDeSincronizacaoConfiguracao : IEntityTypeConfiguration<ExecucaoDeSincronizacao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExecucaoDeSincronizacao> b)
    {
        b.ToTable("ExecucaoDeSincronizacao", "integracao");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.Fluxo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(e => e.Maquina).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(e => e.Resultado).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.Mensagem).HasMaxLength(1000).IsUnicode(true);
        b.Property(e => e.IniciadaEm).HasPrecision(3).IsRequired();
        b.Property(e => e.TerminadaEm).HasPrecision(3);

        // "A última execução deste fluxo" é a consulta da administração.
        b.HasIndex(e => new { e.SistemaId, e.Fluxo, e.IniciadaEm }).IsDescending(false, false, true);

        b.HasOne<Sistema>().WithMany().HasForeignKey(e => e.SistemaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_ExecucaoDeSincronizacao_Resultado", "[Resultado] IN ('EmAndamento','Sucesso','Falha','Ignorada')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_ExecucaoDeSincronizacao_Contadores",
            "[Tentativas] >= 0 AND [RegistrosLidos] >= 0 AND [Incluidos] >= 0 AND [Atualizados] >= 0 AND [Pendentes] >= 0"));
    }
}

/// <summary>Mapeamento de <see cref="DivergenciaDeIntegracao"/>.</summary>
public sealed class DivergenciaDeIntegracaoConfiguracao : IEntityTypeConfiguration<DivergenciaDeIntegracao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DivergenciaDeIntegracao> b)
    {
        b.ToTable("DivergenciaDeIntegracao", "integracao");
        b.HasKey(d => d.Id);
        b.Property(d => d.Id).ValueGeneratedOnAdd();

        b.Property(d => d.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(d => d.ChavePublica).IsUnique().HasDatabaseName("UX_DivergenciaDeIntegracao_ChavePublica");

        b.Property(d => d.Tipo).HasConversion<string>().HasMaxLength(50).IsUnicode(false).IsRequired();
        b.Property(d => d.ChaveOrigem).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(d => d.Descricao).HasMaxLength(400).IsUnicode(true).IsRequired();
        b.Property(d => d.ValorNoCrm).HasMaxLength(200).IsUnicode(true);
        b.Property(d => d.ValorNaOrigem).HasMaxLength(200).IsUnicode(true);
        b.Property(d => d.ValorNoProtheus).HasMaxLength(200).IsUnicode(true);
        b.Property(d => d.Situacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(d => d.DetectadaEm).HasPrecision(3).IsRequired();
        b.Property(d => d.ConfirmadaEm).HasPrecision(3).IsRequired();
        b.Property(d => d.EncerradaEm).HasPrecision(3);
        b.Property(d => d.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(d => d.AlteradoEm).HasPrecision(3);
        b.Property(d => d.ExcluidoEm).HasPrecision(3);

        b.HasIndex(d => new { d.SistemaId, d.Tipo, d.ChaveOrigem })
            .IsUnique()
            .HasDatabaseName("UX_DivergenciaDeIntegracao_Sistema_Tipo_Chave");
        b.HasIndex(d => new { d.EmpresaId, d.Situacao });
        b.HasIndex(d => d.EquipamentoId);
        b.HasIndex(d => d.VendaDeMaquinaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(d => d.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Sistema>().WithMany().HasForeignKey(d => d.SistemaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipamento>().WithMany().HasForeignKey(d => d.EquipamentoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<VendaDeMaquina>().WithMany().HasForeignKey(d => d.VendaDeMaquinaId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_DivergenciaDeIntegracao_Tipo",
            "[Tipo] IN ('CompradorDiferenteDoProprietarioNoCrm','ProprietarioNoProtheusDiferenteDoComprador'," +
            "'CompradorAlteradoNaOrigem','ChassiAlteradoNaOrigem','RegistroAusenteNaOrigem')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_DivergenciaDeIntegracao_Situacao", "[Situacao] IN ('Aberta','Resolvida','DeixouDeOcorrer')"));

        b.Ignore(d => d.Eventos);
    }
}
