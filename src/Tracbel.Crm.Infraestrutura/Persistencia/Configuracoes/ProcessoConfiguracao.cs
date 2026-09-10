using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="TipoProcesso"/> — schema <c>processo</c>.</summary>
public sealed class TipoProcessoConfiguracao : IEntityTypeConfiguration<TipoProcesso>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TipoProcesso> b)
    {
        b.ToTable("TipoProcesso", "processo");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedOnAdd();
        b.Property(t => t.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(t => t.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(t => t.Versao).IsRequired();
        b.Property(t => t.EstaAtivo).IsRequired();

        b.HasIndex(t => new { t.Codigo, t.Versao }).IsUnique().HasDatabaseName("UX_TipoProcesso_Codigo_Versao");
        b.HasIndex(t => t.LinhaDeNegocioId);

        b.HasOne<LinhaDeNegocio>().WithMany().HasForeignKey(t => t.LinhaDeNegocioId)
            .OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_TipoProcesso_Versao", "[Versao] >= 1"));
    }
}

/// <summary>Mapeamento de <see cref="Fase"/>.</summary>
public sealed class FaseConfiguracao : IEntityTypeConfiguration<Fase>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Fase> b)
    {
        b.ToTable("Fase", "processo");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();
        b.Property(f => f.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(f => f.Nome).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(f => f.Marco).HasMaxLength(60).IsUnicode(true);
        b.Property(f => f.Ordem).IsRequired();
        b.Property(f => f.ExigeCamposObrigatorios).IsRequired();
        b.Property(f => f.EhFinal).IsRequired();

        b.HasIndex(f => new { f.TipoProcessoId, f.Codigo }).IsUnique().HasDatabaseName("UX_Fase_Tipo_Codigo");
        b.HasIndex(f => new { f.TipoProcessoId, f.Ordem });

        b.HasOne<TipoProcesso>().WithMany().HasForeignKey(f => f.TipoProcessoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Fase_Probabilidade",
            "[ProbabilidadePercentual] IS NULL OR [ProbabilidadePercentual] BETWEEN 0 AND 100"));
    }
}

/// <summary>Mapeamento de <see cref="TipoTarefa"/>.</summary>
public sealed class TipoTarefaConfiguracao : IEntityTypeConfiguration<TipoTarefa>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TipoTarefa> b)
    {
        b.ToTable("TipoTarefa", "processo");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedOnAdd();
        b.Property(t => t.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(t => t.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(t => t.Categoria).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(t => t.Cor).HasMaxLength(7).IsUnicode(false);
        b.Property(t => t.PrazoDiasUteis).IsRequired();
        b.Property(t => t.EhAprovacao).IsRequired();
        b.Property(t => t.ExigeGeorreferencia).IsRequired();
        b.Property(t => t.ContaParaCobertura).IsRequired();
        b.Property(t => t.EstaAtivo).IsRequired();
        b.Property(t => t.UltimoUsoEm).HasPrecision(3);

        b.HasIndex(t => t.Codigo).IsUnique().HasDatabaseName("UX_TipoTarefa_Codigo");
        b.HasIndex(t => t.TipoProcessoId);
        b.HasIndex(t => t.FormularioId);

        b.HasOne<TipoProcesso>().WithMany().HasForeignKey(t => t.TipoProcessoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Formulario>().WithMany().HasForeignKey(t => t.FormularioId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_TipoTarefa_Categoria",
            "[Categoria] IN ('Visita','Ligacao','WhatsApp','Email','Remota','Interna')"));
        b.ToTable(x => x.HasCheckConstraint("CK_TipoTarefa_Prazo", "[PrazoDiasUteis] >= 0"));

        // Cor é hexadecimal de seis dígitos com cerquilha, ou nada. É a cor da agenda.
        b.ToTable(x => x.HasCheckConstraint("CK_TipoTarefa_Cor", "[Cor] IS NULL OR ([Cor] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND LEN([Cor]) = 7)"));
    }
}

/// <summary>Mapeamento de <see cref="Resultado"/>.</summary>
public sealed class ResultadoConfiguracao : IEntityTypeConfiguration<Resultado>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Resultado> b)
    {
        b.ToTable("Resultado", "processo");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedOnAdd();
        b.Property(r => r.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(r => r.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(r => r.Classe).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(r => r.ExigeJustificativa).IsRequired();
        b.Property(r => r.EstaAtivo).IsRequired();
        b.Property(r => r.UltimoUsoEm).HasPrecision(3);

        b.HasIndex(r => new { r.TipoTarefaId, r.Codigo }).IsUnique().HasDatabaseName("UX_Resultado_TipoTarefa_Codigo");
        b.HasIndex(r => r.FaseDestinoId);

        b.HasOne<TipoTarefa>().WithMany().HasForeignKey(r => r.TipoTarefaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Fase>().WithMany().HasForeignKey(r => r.FaseDestinoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Resultado_Classe", "[Classe] IN ('Avanco','Manutencao','Perda','Cancelamento')"));
    }
}

/// <summary>Mapeamento de <see cref="MotivoDePerda"/>.</summary>
public sealed class MotivoDePerdaConfiguracao : IEntityTypeConfiguration<MotivoDePerda>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MotivoDePerda> b)
    {
        b.ToTable("MotivoDePerda", "processo");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();
        b.Property(m => m.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(m => m.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(m => m.Categoria).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(m => m.ExigeConcorrente).IsRequired();
        b.Property(m => m.ExigeObservacao).IsRequired();
        b.Property(m => m.Ordem).IsRequired();
        b.Property(m => m.EstaAtivo).IsRequired();

        b.HasIndex(m => m.Codigo).IsUnique().HasDatabaseName("UX_MotivoDePerda_Codigo");

        b.ToTable(x => x.HasCheckConstraint(
            "CK_MotivoDePerda_Categoria",
            "[Categoria] IN ('Preco','Prazo','Produto','Financiamento','Desistencia','Concorrencia','Outro')"));
    }
}

/// <summary>Mapeamento de <see cref="Dominio.Processo.Processo"/> — a segunda entidade central.</summary>
public sealed class ProcessoConfiguracao : IEntityTypeConfiguration<Dominio.Processo.Processo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Dominio.Processo.Processo> b)
    {
        b.ToTable("Processo", "processo");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(p => p.ChavePublica).IsUnique().HasDatabaseName("UX_Processo_ChavePublica");

        b.Property(p => p.Numero).IsRequired();
        b.Property(p => p.Titulo).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(p => p.Descricao).HasMaxLength(4000).IsUnicode(true);
        b.Property(p => p.ObservacaoDaPerda).HasMaxLength(1000).IsUnicode(true);

        b.Property(p => p.Situacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(p => p.SituacaoDesde).HasPrecision(3).IsRequired();
        b.Property(p => p.FaseDesde).HasPrecision(3).IsRequired();
        b.Property(p => p.ConcluidoEm).HasPrecision(3);

        b.Property(p => p.ValorEstimado)
            .HasConversion(d => d!.Value.Valor, v => Dinheiro.Criar(v))
            .HasPrecision(18, 2);

        b.Property(p => p.ValorFinal)
            .HasConversion(d => d!.Value.Valor, v => Dinheiro.Criar(v))
            .HasPrecision(18, 2);

        b.Property(p => p.Quantidade).HasPrecision(12, 3);

        b.Property(p => p.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(p => p.AlteradoEm).HasPrecision(3);
        b.Property(p => p.ExcluidoEm).HasPrecision(3);

        b.HasIndex(p => new { p.EmpresaId, p.Numero }).IsUnique().HasDatabaseName("UX_Processo_Empresa_Numero");

        // Os três caminhos de leitura da tela: a ficha do cliente, o funil e a minha lista.
        b.HasIndex(p => p.ClienteId).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(p => new { p.EmpresaId, p.FaseId }).HasFilter("[Situacao] = 'Aberto' AND [ExcluidoEm] IS NULL");
        b.HasIndex(p => new { p.ProprietarioId, p.Situacao }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(p => p.TipoProcessoId);
        b.HasIndex(p => p.ContatoId);
        b.HasIndex(p => p.CarteiraId);
        b.HasIndex(p => p.FaseId);
        b.HasIndex(p => p.MotivoDePerdaId);
        b.HasIndex(p => p.ProprietarioEquipeId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TipoProcesso>().WithMany().HasForeignKey(p => p.TipoProcessoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Contato>().WithMany().HasForeignKey(p => p.ContatoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Carteira>().WithMany().HasForeignKey(p => p.CarteiraId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Fase>().WithMany().HasForeignKey(p => p.FaseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<MotivoDePerda>().WithMany().HasForeignKey(p => p.MotivoDePerdaId).OnDelete(DeleteBehavior.Restrict);
        b.LigarAoCatalogoDeSistema(
            nameof(Dominio.Processo.Processo.ConcorrenteId), "CatalogoDoConcorrenteId",
            CatalogosDeSistema.Concorrente);
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.ProprietarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipe>().WithMany().HasForeignKey(p => p.ProprietarioEquipeId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Processo_Situacao", "[Situacao] IN ('Aberto','Suspenso','Ganho','Perdido','Cancelado')"));

        // Encerrou? Então tem data. [V] "Atividade Cancelada" cancelava o processo sem
        // registrar motivo nem permitir desfazer.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Processo_Encerramento",
            "[Situacao] IN ('Aberto','Suspenso') OR [ConcluidoEm] IS NOT NULL"));

        // Perdeu? Então tem motivo. É o que faz a Visão 360 conseguir responder "quanto
        // perdemos por preço".
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Processo_MotivoDePerda",
            "[Situacao] <> 'Perdido' OR [MotivoDePerdaId] IS NOT NULL"));

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Processo_Valores",
            "([ValorEstimado] IS NULL OR [ValorEstimado] >= 0) AND ([ValorFinal] IS NULL OR [ValorFinal] >= 0)"));

        b.Ignore(p => p.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="PassagemDeFase"/>.</summary>
public sealed class PassagemDeFaseConfiguracao : IEntityTypeConfiguration<PassagemDeFase>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PassagemDeFase> b)
    {
        b.ToTable("PassagemDeFase", "processo");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.Ordem).IsRequired();
        b.Property(p => p.EntrouEm).HasPrecision(3).IsRequired();
        b.Property(p => p.SaiuEm).HasPrecision(3);
        b.Property(p => p.HorasUteis).HasPrecision(10, 2);

        b.HasIndex(p => new { p.ProcessoId, p.Ordem })
            .IsUnique()
            .HasDatabaseName("UX_PassagemDeFase_Processo_Ordem");

        // "Em que fase o processo está agora": índice parcial sobre a única linha aberta.
        b.HasIndex(p => p.ProcessoId).HasFilter("[SaiuEm] IS NULL");
        b.HasIndex(p => p.FaseId);
        b.HasIndex(p => p.InteracaoOrigemId);
        b.HasIndex(p => p.RegraOrigemId);
        b.HasIndex(p => p.EntrouPorId);

        b.HasOne<Dominio.Processo.Processo>().WithMany().HasForeignKey(p => p.ProcessoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Fase>().WithMany().HasForeignKey(p => p.FaseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Interacao>().WithMany().HasForeignKey(p => p.InteracaoOrigemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Workflow.Regra>().WithMany().HasForeignKey(p => p.RegraOrigemId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.EntrouPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_PassagemDeFase_Periodo", "[SaiuEm] IS NULL OR [SaiuEm] >= [EntrouEm]"));
        b.ToTable(x => x.HasCheckConstraint("CK_PassagemDeFase_Ordem", "[Ordem] >= 1"));
    }
}

/// <summary>Mapeamento de <see cref="Tarefa"/>.</summary>
public sealed class TarefaConfiguracao : IEntityTypeConfiguration<Tarefa>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Tarefa> b)
    {
        b.ToTable("Tarefa", "processo");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedOnAdd();

        b.Property(t => t.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(t => t.ChavePublica).IsUnique().HasDatabaseName("UX_Tarefa_ChavePublica");

        b.Property(t => t.Assunto).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(t => t.Detalhe).HasMaxLength(4000).IsUnicode(true);
        b.Property(t => t.OrigemAtribuicao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(t => t.Situacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(t => t.Prioridade).IsRequired();

        b.Property(t => t.AgendadaPara).HasPrecision(3).IsRequired();
        b.Property(t => t.PrazoLimite).HasPrecision(3);
        b.Property(t => t.ConcluidaEm).HasPrecision(3);
        b.Property(t => t.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(t => t.AlteradoEm).HasPrecision(3);
        b.Property(t => t.ExcluidoEm).HasPrecision(3);

        // O índice que sustenta a tela principal do usuário: minhas tarefas pendentes.
        b.HasIndex(t => new { t.ResponsavelId, t.AgendadaPara })
            .HasFilter("[Situacao] IN ('Pendente','EmAndamento') AND [ExcluidoEm] IS NULL")
            .IncludeProperties(t => new { t.Assunto, t.Prioridade, t.ProcessoId, t.ClienteId });

        b.HasIndex(t => t.ProcessoId).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(t => t.ClienteId).HasFilter("[Situacao] <> 'Concluida' AND [ExcluidoEm] IS NULL");
        b.HasIndex(t => t.TipoTarefaId);
        b.HasIndex(t => t.ContatoId);
        b.HasIndex(t => t.ResultadoId);
        b.HasIndex(t => t.ResponsavelEquipeId);
        b.HasIndex(t => t.ConcluidaPorId);
        b.HasIndex(t => t.InteracaoConclusaoId);
        b.HasIndex(t => t.InteracaoOrigemId);
        b.HasIndex(t => t.CriadaPorRegraId);
        b.HasIndex(t => t.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(t => t.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Processo>().WithMany().HasForeignKey(t => t.ProcessoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(t => t.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Contato>().WithMany().HasForeignKey(t => t.ContatoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TipoTarefa>().WithMany().HasForeignKey(t => t.TipoTarefaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Resultado>().WithMany().HasForeignKey(t => t.ResultadoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Interacao>().WithMany().HasForeignKey(t => t.InteracaoConclusaoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Interacao>().WithMany().HasForeignKey(t => t.InteracaoOrigemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Workflow.Regra>().WithMany().HasForeignKey(t => t.CriadaPorRegraId)
            .OnDelete(DeleteBehavior.Restrict);

        // [V] IV_Agenda.Vendedor guarda o LOGIN, não o identificador. Aqui é chave
        // estrangeira, sempre.
        b.HasOne<Usuario>().WithMany().HasForeignKey(t => t.ResponsavelId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(t => t.ConcluidaPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipe>().WithMany().HasForeignKey(t => t.ResponsavelEquipeId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Tarefa_Situacao",
            "[Situacao] IN ('Pendente','EmAndamento','Concluida','Cancelada','Reatribuida')"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Tarefa_OrigemAtribuicao",
            "[OrigemAtribuicao] IN ('Regra','Manual','Hierarquia','Carteira','Rodizio')"));
        b.ToTable(x => x.HasCheckConstraint("CK_Tarefa_Prioridade", "[Prioridade] BETWEEN 1 AND 5"));

        // Concluiu? Tem data, quem e desfecho. Sem exceção.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Tarefa_Conclusao",
            "[Situacao] <> 'Concluida' " +
            "OR ([ConcluidaEm] IS NOT NULL AND [ConcluidaPorId] IS NOT NULL AND [ResultadoId] IS NOT NULL)"));

        b.Ignore(t => t.Eventos);
    }
}

/// <summary>
/// Mapeamento de <see cref="Interacao"/> — a tabela de maior volume do modelo.
///
/// Somente-acrescentar por desenho: não tem coluna de alteração nem de exclusão lógica.
/// </summary>
public sealed class InteracaoConfiguracao : IEntityTypeConfiguration<Interacao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Interacao> b)
    {
        b.ToTable("Interacao", "processo");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).ValueGeneratedOnAdd();

        b.Property(i => i.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(i => i.ChavePublica).IsUnique().HasDatabaseName("UX_Interacao_ChavePublica");

        b.Property(i => i.Assunto).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(i => i.Detalhe).HasMaxLength(4000).IsUnicode(true);
        b.Property(i => i.ResultadoComplemento).HasMaxLength(200).IsUnicode(true);
        b.Property(i => i.Natureza).HasConversion<string>().HasMaxLength(12).IsUnicode(false).IsRequired();
        b.Property(i => i.CriadoEm).HasPrecision(3).IsRequired();

        b.Property(i => i.OcorridaEm)
            .HasConversion(d => d.Valor, v => DataHoraUtc.Criar(v))
            .HasPrecision(3).IsRequired();

        b.Property(i => i.Latitude).HasPrecision(10, 7);
        b.Property(i => i.Longitude).HasPrecision(10, 7);

        // O ÍNDICE MAIS IMPORTANTE DO MODELO: a Visão 360 e a Cobertura leem as últimas
        // interações do cliente sobre uma tabela que herda 2,4 milhões de linhas.
        b.HasIndex(i => new { i.ClienteId, i.OcorridaEm }).IsDescending(false, true);

        b.HasIndex(i => new { i.ProcessoId, i.OcorridaEm }).IsDescending(false, true);
        b.HasIndex(i => i.ContatoId);
        b.HasIndex(i => i.LeadId);
        b.HasIndex(i => i.TarefaId);
        b.HasIndex(i => i.TipoTarefaId);
        b.HasIndex(i => i.ResultadoId);
        b.HasIndex(i => i.RegistradoPorId);
        b.HasIndex(i => i.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(i => i.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<TipoTarefa>().WithMany().HasForeignKey(i => i.TipoTarefaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Processo>().WithMany().HasForeignKey(i => i.ProcessoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(i => i.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Contato>().WithMany().HasForeignKey(i => i.ContatoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Crm.Lead>().WithMany().HasForeignKey(i => i.LeadId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tarefa>().WithMany().HasForeignKey(i => i.TarefaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Resultado>().WithMany().HasForeignKey(i => i.ResultadoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(i => i.RegistradoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Interacao_Natureza", "[Natureza] IN ('Ativa','Receptiva','Sistema')"));

        // Toda interação se liga a alguma coisa. Nunca solta.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Interacao_TemVinculo",
            "[ProcessoId] IS NOT NULL OR [ClienteId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [LeadId] IS NOT NULL"));

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Interacao_Duracao", "[DuracaoMinutos] IS NULL OR [DuracaoMinutos] >= 0"));

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Interacao_Coordenada",
            "([Latitude] IS NULL AND [Longitude] IS NULL) " +
            "OR ([Latitude] BETWEEN -90 AND 90 AND [Longitude] BETWEEN -180 AND 180)"));

        b.Ignore(i => i.Localizacao);
    }
}

/// <summary>Mapeamento de <see cref="InteracaoParticipante"/>.</summary>
public sealed class InteracaoParticipanteConfiguracao : IEntityTypeConfiguration<InteracaoParticipante>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InteracaoParticipante> b)
    {
        b.ToTable("InteracaoParticipante", "processo");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.Papel).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(p => p.NomeExterno).HasMaxLength(200).IsUnicode(true);

        b.HasIndex(p => p.InteracaoId);
        b.HasIndex(p => p.UsuarioId);
        b.HasIndex(p => p.ContatoId);

        b.HasOne<Interacao>().WithMany().HasForeignKey(p => p.InteracaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Contato>().WithMany().HasForeignKey(p => p.ContatoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_InteracaoParticipante_Papel", "[Papel] IN ('Autor','Destinatario','Copia','Participante')"));

        // Participante precisa ser alguém: usuário, contato ou ao menos um nome escrito.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_InteracaoParticipante_Identificado",
            "[UsuarioId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [NomeExterno] IS NOT NULL"));
    }
}

/// <summary>Mapeamento de <see cref="ItemDeProposta"/>.</summary>
public sealed class ItemDePropostaConfiguracao : IEntityTypeConfiguration<ItemDeProposta>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ItemDeProposta> b)
    {
        b.ToTable("ItemDeProposta", "processo");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).ValueGeneratedOnAdd();

        b.Property(i => i.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(i => i.ChavePublica).IsUnique().HasDatabaseName("UX_ItemDeProposta_ChavePublica");

        b.Property(i => i.Descricao).HasMaxLength(400).IsUnicode(true).IsRequired();
        b.Property(i => i.Ordem).IsRequired();
        b.Property(i => i.Quantidade).HasPrecision(12, 3).IsRequired();
        b.Property(i => i.DescontoPercentual).HasPrecision(5, 2);

        b.Property(i => i.PrecoUnitario)
            .HasConversion(d => d!.Value.Valor, v => Dinheiro.Criar(v))
            .HasPrecision(18, 2);

        b.Property(i => i.ValorTotal)
            .HasConversion(d => d.Valor, v => Dinheiro.Criar(v))
            .HasPrecision(18, 2).IsRequired();

        b.Property(i => i.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(i => i.AlteradoEm).HasPrecision(3);
        b.Property(i => i.ExcluidoEm).HasPrecision(3);

        b.HasIndex(i => new { i.ProcessoId, i.Ordem })
            .IsUnique()
            .HasFilter("[ExcluidoEm] IS NULL")
            .HasDatabaseName("UX_ItemDeProposta_Processo_Ordem");

        b.HasIndex(i => i.ModeloId);
        b.HasIndex(i => i.EquipamentoId);
        b.HasIndex(i => i.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(i => i.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Processo>().WithMany().HasForeignKey(i => i.ProcessoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Frota.Modelo>().WithMany().HasForeignKey(i => i.ModeloId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Frota.Equipamento>().WithMany().HasForeignKey(i => i.EquipamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.LigarAoCatalogoDeSistema(
            nameof(ItemDeProposta.CondicaoPagamentoId), "CatalogoDaCondicaoPagamentoId",
            CatalogosDeSistema.CondicaoDePagamento);

        b.ToTable(x => x.HasCheckConstraint("CK_ItemDeProposta_Quantidade", "[Quantidade] > 0"));
        b.ToTable(x => x.HasCheckConstraint("CK_ItemDeProposta_ValorTotal", "[ValorTotal] >= 0"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_ItemDeProposta_Desconto",
            "[DescontoPercentual] IS NULL OR [DescontoPercentual] BETWEEN 0 AND 100"));

        b.Ignore(i => i.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="VendaPerdida"/> — schema <c>processo</c>.</summary>
public sealed class VendaPerdidaConfiguracao : IEntityTypeConfiguration<VendaPerdida>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<VendaPerdida> b)
    {
        b.ToTable("VendaPerdida", "processo");
        b.HasKey(v => v.Id);
        b.Property(v => v.Id).ValueGeneratedOnAdd();

        b.Property(v => v.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(v => v.ChavePublica).IsUnique().HasDatabaseName("UX_VendaPerdida_ChavePublica");

        b.Property(v => v.RegistradaEm).HasPrecision(3).IsRequired();
        b.Property(v => v.OcorridaEm);
        b.Property(v => v.ModeloDoConcorrente).HasMaxLength(120).IsUnicode(true);
        b.Property(v => v.ModeloOfertado).HasMaxLength(120).IsUnicode(true);
        b.Property(v => v.RegistradaPor).HasMaxLength(60).IsUnicode(true);
        b.Property(v => v.Quantidade).IsRequired();
        b.Property(v => v.PrecoDoConcorrente).HasPrecision(18, 2);
        b.Property(v => v.PrecoOfertado).HasPrecision(18, 2);

        // GRAVADO COMO TEXTO, e com a restrição no banco. Este campo já foi um `bit` anulável, e
        // o `NULL` guardava a terceira resposta sem nome — quem abrisse a tabela precisava saber
        // de cabeça que ali "não informado" e "não participamos" eram coisas diferentes. O nome
        // vai junto com o dado, e o banco recusa qualquer valor fora dos três.
        b.Property(v => v.Participacao)
            .HasConversion<string>().HasMaxLength(16).IsUnicode(false).IsRequired()
            .HasDefaultValue(ParticipacaoNaNegociacao.NaoInformado);

        b.Property(v => v.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(v => v.AlteradoEm).HasPrecision(3);
        b.Property(v => v.ExcluidoEm).HasPrecision(3);

        // Os três recortes que as telas pedem: por filial e período, por motivo, por concorrente.
        b.HasIndex(v => new { v.EmpresaId, v.RegistradaEm }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(v => v.MotivoDePerdaId);
        b.HasIndex(v => v.ConcorrenteId);
        b.HasIndex(v => v.ProcessoId);
        b.HasIndex(v => v.ClienteId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(v => v.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<MotivoDePerda>().WithMany().HasForeignKey(v => v.MotivoDePerdaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Processo>().WithMany().HasForeignKey(v => v.ProcessoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
        // OS TRÊS CATÁLOGOS SÃO OS DE SISTEMA, e o concorrente é o MESMO que o processo usa.
        // A ligação composta amarra cada coluna ao seu catálogo: sem ela, a chave estrangeira
        // aceitaria item de qualquer lista, e "New Holland" poderia acabar no campo de revenda.
        b.LigarAoCatalogoDeSistema(
            nameof(VendaPerdida.ConcorrenteId), "CatalogoDoConcorrenteId",
            CatalogosDeSistema.Concorrente);
        b.LigarAoCatalogoDeSistema(
            nameof(VendaPerdida.TipoDeEquipamentoId), "CatalogoDoTipoDeEquipamentoId",
            CatalogosDeSistema.TipoDeEquipamento);
        b.LigarAoCatalogoDeSistema(
            nameof(VendaPerdida.RevendaDoConcorrenteId), "CatalogoDaRevendaId",
            CatalogosDeSistema.RevendaConcorrente);

        // PREÇO NEGATIVO NÃO EXISTE, e zero também não é preço — a origem guarda 0,01 como
        // preço de trator, e o saneamento troca isso por nulo antes de chegar aqui. A restrição
        // é a rede embaixo: se algum caminho futuro escrever direto, ela avisa.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_VendaPerdida_Precos",
            "([PrecoDoConcorrente] IS NULL OR [PrecoDoConcorrente] > 0) AND " +
            "([PrecoOfertado] IS NULL OR [PrecoOfertado] > 0)"));

        b.ToTable(x => x.HasCheckConstraint("CK_VendaPerdida_Quantidade", "[Quantidade] >= 1"));

        b.ToTable(x => x.HasCheckConstraint(
            "CK_VendaPerdida_Participacao",
            "[Participacao] IN ('NaoInformado','Sim','Nao')"));

        b.Ignore(v => v.Eventos);
    }
}
