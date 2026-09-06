using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Dominio.Workflow;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Catálogo de permissões. Fechado e versionado no seed.</summary>
public sealed class PermissaoConfiguracao : IEntityTypeConfiguration<Permissao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Permissao> b)
    {
        b.ToTable("Permissao", "seguranca");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();
        b.Property(p => p.Codigo).HasMaxLength(80).IsUnicode(false).IsRequired();
        b.Property(p => p.Entidade).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(p => p.Verbo).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(p => p.Descricao).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.HasIndex(p => p.Codigo).IsUnique().HasDatabaseName("UX_Permissao_Codigo");

        // O verbo é domínio fechado — os mesmos seis de Dominio.Seguranca.Verbos.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Permissao_Verbo",
            "[Verbo] IN ('Ler','Criar','Editar','Excluir','Atribuir','Compartilhar')"));
    }
}

/// <summary>Conjuntos de permissão e seus itens.</summary>
public sealed class ConjuntoPermissaoConfiguracao : IEntityTypeConfiguration<ConjuntoPermissao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ConjuntoPermissao> b)
    {
        b.ToTable("ConjuntoDePermissao", "seguranca");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();
        b.Property(c => c.Codigo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(c => c.EstaAtivo).IsRequired();
        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_ConjuntoDePermissao_Codigo");

        // A coleção é privada no domínio (encapsulamento). O EF acessa pelo campo.
        b.Metadata
            .FindNavigation(nameof(ConjuntoPermissao.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.ConjuntoPermissaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Uma permissão dentro de um conjunto, com sua profundidade.</summary>
public sealed class ItemConjuntoPermissaoConfiguracao : IEntityTypeConfiguration<ItemConjuntoPermissao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ItemConjuntoPermissao> b)
    {
        b.ToTable("ConjuntoDePermissaoItem", "seguranca");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).ValueGeneratedOnAdd();
        b.Property(i => i.CodigoPermissao).HasMaxLength(80).IsUnicode(false).IsRequired();

        // Gravada como string: o banco fica legível e inserir valor novo no enum
        // não reordena o que já existe.
        b.Property(i => i.Profundidade).HasConversion<string>()
            .HasMaxLength(20).IsUnicode(false).IsRequired();

        b.HasIndex(i => new { i.ConjuntoPermissaoId, i.CodigoPermissao })
            .IsUnique()
            .HasDatabaseName("UX_ConjuntoDePermissaoItem_Conjunto_Codigo");

        // Domínio fechado no banco, não só no enum do C#. "Nenhum" fica de fora de propósito:
        // ConjuntoPermissao.Conceder() já recusa conceder profundidade Nenhum — não faz
        // sentido existir uma linha dizendo "conceda isto com profundidade nenhuma". O CHECK
        // reflete a MESMA regra de negócio, não só a lista de valores do enum.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ConjuntoDePermissaoItem_Profundidade",
            "[Profundidade] IN ('Proprios','Equipe','Empresa','EmpresaEAbaixo','Organizacao')"));
    }
}

/// <summary>Concessão de conjunto a usuário, com expiração opcional.</summary>
public sealed class UsuarioConjuntoPermissaoConfiguracao : IEntityTypeConfiguration<UsuarioConjuntoPermissao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UsuarioConjuntoPermissao> b)
    {
        b.ToTable("UsuarioConjuntoDePermissao", "seguranca");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).ValueGeneratedOnAdd();
        b.Property(u => u.ConcedidoEm).HasPrecision(3).IsRequired();
        b.Property(u => u.ExpiraEm).HasPrecision(3);
        b.HasIndex(u => new { u.UsuarioId, u.ConjuntoPermissaoId })
            .IsUnique()
            .HasDatabaseName("UX_UsuarioConjuntoDePermissao_Usuario_Conjunto");

        // [V] "IV_Agenda, IV_Historico e IV_ProcDado não têm FK para IV_Processo — 345.535
        // linhas órfãs" (documento 04, seção 1.4). ConjuntoPermissaoId apontava para
        // seguranca.conjunto_de_permissao sem FK declarada — a mesma lacuna, em miniatura.
        // Restrict, não Cascade: apagar um conjunto de permissão NUNCA deve arrastar
        // silenciosamente o histórico de concessão de um usuário.
        b.HasOne<ConjuntoPermissao>()
            .WithMany()
            .HasForeignKey(u => u.ConjuntoPermissaoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Agora que seguranca.Usuario É uma entidade mapeada, a mesma regra vale para ela —
        // e a heurística de FK por nome do documento 14, seção 6, passa a exigir esta linha.
        b.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(u => u.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>Regras de automação.</summary>
public sealed class RegraConfiguracao : IEntityTypeConfiguration<Regra>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Regra> b)
    {
        b.ToTable("Regra", "processo");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedOnAdd();
        b.Property(r => r.Codigo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(r => r.Nome).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(r => r.Descricao).HasMaxLength(1000).IsUnicode(true);
        b.Property(r => r.Evento).HasMaxLength(40).IsUnicode(false).IsRequired();

        // As duas colunas que corrigem o defeito central do Vórtice.
        // [V] IV_AcaoAutoCtrl (a tabela de condições) está VAZIA lá, e o destinatário é um
        // ID fixo na linha — daí 5.958 regras para 1.262 pares distintos.
        b.Property(r => r.Condicao).HasMaxLength(2000).IsUnicode(true);
        b.Property(r => r.ExpressaoDestinatario).HasMaxLength(400).IsUnicode(true).IsRequired();

        b.Property(r => r.Efeito).HasMaxLength(40).IsUnicode(false).IsRequired();

        // JSON, e não texto solto: a forma do parâmetro varia por efeito, e o banco precisa
        // conseguir consultar dentro dela sem que ninguém escreva um analisador de texto.
        b.Property(r => r.EfeitoParametros).HasColumnType("nvarchar(max)");
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Regra_EfeitoParametrosJson",
            "[EfeitoParametros] IS NULL OR ISJSON([EfeitoParametros]) = 1"));

        b.Property(r => r.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(r => r.AlteradoEm).HasPrecision(3);

        b.HasIndex(r => r.Codigo).IsUnique().HasDatabaseName("UX_Regra_Codigo");

        // O índice que sustenta a carga de regras a cada evento.
        b.HasIndex(r => new { r.Evento, r.ResultadoId, r.Ordem }).HasFilter("[EstaAtiva] = 1");

        b.ToTable(t => t.HasCheckConstraint(
            "CK_Regra_Efeito",
            "[Efeito] IN ('CriarTarefa','MoverFase','EncerrarProcesso','Notificar','ChamarWebhook','AtribuirCarteira')"));

        // O gatilho e o efeito apontam para catálogos do schema processo. Quando estas quatro
        // colunas foram escritas, aquelas tabelas ainda não existiam no modelo e ficaram sem
        // chave estrangeira — a MESMA lacuna que produziu 345.535 linhas órfãs no Vórtice, e
        // que a heurística do documento 14, seção 6, pegou assim que as entidades entraram.
        b.HasOne<Dominio.Processo.TipoProcesso>().WithMany().HasForeignKey(r => r.TipoProcessoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.TipoTarefa>().WithMany().HasForeignKey(r => r.TipoTarefaId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Resultado>().WithMany().HasForeignKey(r => r.ResultadoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.TipoTarefa>().WithMany().HasForeignKey(r => r.EfeitoTipoTarefaId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Fase>().WithMany().HasForeignKey(r => r.EfeitoFaseId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => r.TipoProcessoId);
        b.HasIndex(r => r.TipoTarefaId);
        b.HasIndex(r => r.EfeitoTipoTarefaId);
        b.HasIndex(r => r.EfeitoFaseId);
    }
}

/// <summary>
/// O log de execução de regra.
///
/// [V] É a tabela que torna impossível repetir o defeito 899/900: no Vórtice, a taxa de
/// geração de uma tarefa crítica caiu de 68% para 3% ao longo de cinco meses e ninguém
/// percebeu, porque regra que não dispara não deixa rastro.
///
/// É PARTICIONADA POR MÊS (ver a migração inicial): 12 meses disponíveis, arquivamento
/// depois. Por isso a chave primária inclui a coluna de particionamento — exigência do
/// SQL Server para todo índice único de tabela particionada, e a única exceção à regra de
/// chave primária de coluna única.
/// </summary>
public sealed class ExecucaoRegraConfiguracao : IEntityTypeConfiguration<ExecucaoRegra>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ExecucaoRegra> b)
    {
        b.ToTable("RegraExecucao", "processo");
        b.HasKey(e => new { e.Id, e.ExecutadoEm });
        b.Property(e => e.Id).ValueGeneratedOnAdd();
        b.Property(e => e.Evento).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(e => e.Resultado).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        // A explicação em português. É o que o suporte lê para diagnosticar em segundos.
        b.Property(e => e.Motivo).HasMaxLength(1000).IsUnicode(true);

        b.Property(e => e.ExecutadoEm).HasPrecision(3).IsRequired();

        // Reconstrói "o que o sistema fez quando o vendedor clicou em concluir".
        b.HasIndex(e => e.CorrelacaoId);

        // Alimenta o monitor de saúde das regras: taxa de disparo por regra, por período.
        b.HasIndex(e => new { e.RegraId, e.ExecutadoEm, e.Resultado });

        b.HasIndex(e => new { e.ProcessoId, e.ExecutadoEm });

        // [V] mesma lacuna do documento 04, seção 1.4 (FK que falta é FK que gera órfão):
        // RegraId apontava para processo.regra sem FK declarada. Restrict, não Cascade —
        // apagar uma regra NUNCA pode arrastar o log que prova que ela disparou (ou não).
        b.HasOne<Regra>()
            .WithMany()
            .HasForeignKey(e => e.RegraId)
            .OnDelete(DeleteBehavior.Restrict);

        // Domínio fechado no banco. Aqui, ao contrário de ConjuntoPermissaoItem.Profundidade,
        // TODOS os valores do enum são estados legítimos — inclusive "Erro" e "CondicaoFalsa",
        // que são o comportamento normal e esperado deste log, não uma falha do log em si.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_RegraExecucao_Resultado",
            "[Resultado] IN ('Disparou','CondicaoFalsa','RegraInativa','SemDestinatario','Erro','Suprimida')"));

        // O log diz sobre O QUE a regra foi avaliada. Sem chave estrangeira, o log sobrevive
        // ao registro que ele descreve e vira linha órfã — que é o defeito que este log existe
        // para NÃO repetir.
        b.HasOne<Dominio.Processo.Processo>().WithMany().HasForeignKey(e => e.ProcessoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Tarefa>().WithMany().HasForeignKey(e => e.TarefaId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Interacao>().WithMany().HasForeignKey(e => e.InteracaoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Tarefa>().WithMany().HasForeignKey(e => e.TarefaCriadaId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(e => e.DestinatarioResolvidoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(e => e.TarefaId);
        b.HasIndex(e => e.InteracaoId);
        b.HasIndex(e => e.TarefaCriadaId);
        b.HasIndex(e => e.DestinatarioResolvidoId);
    }
}
