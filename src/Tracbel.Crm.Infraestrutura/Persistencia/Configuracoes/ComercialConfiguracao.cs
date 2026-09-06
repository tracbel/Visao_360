using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="Cliente"/> — schema <c>comercial</c>.</summary>
public sealed class ClienteConfiguracao : IEntityTypeConfiguration<Cliente>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Cliente", "comercial");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(c => c.ChavePublica).IsUnique().HasDatabaseName("UX_Cliente_ChavePublica");

        // A BUSCA POR NOME SEM ACENTO É DA COLAÇÃO, não de uma coluna derivada.
        // Latin1_General_CI_AI ignora caixa E acento na comparação e no índice: "Jose"
        // encontra "José" e "JOSÉ" com o mesmo índice, sem coluna computada nenhuma.
        // [V] mata a tabela de fonema do Vórtice (151.583 linhas mantidas por fora só para
        // buscar nome sem acento) — regra 5 do documento 17: o que é derivável de outra
        // coluna nunca vira tabela. Ver documento 20, seção 4.
        b.Property(c => c.NomeRazao)
            .HasMaxLength(200)
            .IsUnicode(true)
            .UseCollation(CrmDbContext.ColacaoSemCaixaNemAcento)
            .IsRequired();

        b.Property(c => c.NomeFantasia).HasMaxLength(200).IsUnicode(true);
        b.Property(c => c.InscricaoEstadual).HasMaxLength(20).IsUnicode(false);
        b.Property(c => c.AtividadeEconomica).HasMaxLength(7).IsUnicode(false);

        b.Property(c => c.TipoDePessoa).HasConversion<string>().HasMaxLength(10).IsUnicode(false).IsRequired();
        b.Property(c => c.Situacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        // A CLASSE APURADA. Texto, e não número, para a tabela ser legível sem o enum do lado —
        // e nula enquanto a curva ABC não rodou, porque "sem apuração" não é a mesma coisa que
        // "classe D". D é quem não comprou; nulo é quem ninguém mediu ainda.
        b.Property(c => c.Classe).HasConversion<string>().HasMaxLength(1).IsUnicode(false);
        b.Property(c => c.FaturamentoApurado).HasPrecision(18, 2);
        b.Property(c => c.ClasseApuradaEm).HasPrecision(3);

        b.HasIndex(c => new { c.EmpresaId, c.Classe }).HasFilter("[ExcluidoEm] IS NULL");

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Cliente_Classe", "[Classe] IS NULL OR [Classe] IN ('A','B','C','D')"));
        b.Property(c => c.SituacaoDesde).HasPrecision(3).IsRequired();

        b.Property(c => c.Documento)
            .HasConversion(d => d!.Value.Numero, s => CpfCnpj.Criar(s))
            .HasMaxLength(14).IsUnicode(false);

        b.Property(c => c.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.AlteradoEm).HasPrecision(3);
        b.Property(c => c.ExcluidoEm).HasPrecision(3);

        // Unicidade de documento POR EMPRESA, ignorando excluídos.
        // [V] medido no Vórtice: 116 CPFs repetidos entre clientes distintos.
        b.HasIndex(c => new { c.EmpresaId, c.Documento })
            .IsUnique()
            .HasFilter("[Documento] IS NOT NULL AND [ExcluidoEm] IS NULL")
            .HasDatabaseName("UX_Cliente_Empresa_Documento");

        b.HasIndex(c => c.ProprietarioId).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => new { c.EmpresaId, c.Situacao }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => c.NomeRazao).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => c.ClienteMatrizId);
        b.HasIndex(c => c.ProprietarioEquipeId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.ProprietarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipe>().WithMany().HasForeignKey(c => c.ProprietarioEquipeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(c => c.ClienteMatrizId).OnDelete(DeleteBehavior.Restrict);
        // Origem e motivo de inativação vêm de catálogo, e de UM catálogo — chave estrangeira
        // COMPOSTA contra AK_CatalogoItem_CatalogoId. Antes eram de coluna única, e por isso
        // Cliente.OrigemId aceitava um item do catálogo CULTURA (documento 21, achado I-1).
        b.LigarAoCatalogoDeSistema(
            nameof(Cliente.OrigemId), "CatalogoDaOrigemId", CatalogosDeSistema.OrigemDeLead);
        b.LigarAoCatalogoDeSistema(
            nameof(Cliente.MotivoInativacaoId), "CatalogoDoMotivoInativacaoId",
            CatalogosDeSistema.MotivoDeInativacao);

        b.ToTable(t => t.HasCheckConstraint("CK_Cliente_TipoDePessoa", "[TipoDePessoa] IN ('Fisica','Juridica')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Cliente_Situacao",
            "[Situacao] IN ('Suspect','Prospect','Cliente','ClienteInativo','Encerrado')"));

        // O documento tem o tamanho certo para o tipo de pessoa — no banco, não só na tela.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Cliente_Documento",
            "[Documento] IS NULL " +
            "OR ([TipoDePessoa] = 'Fisica' AND LEN([Documento]) = 11) " +
            "OR ([TipoDePessoa] = 'Juridica' AND LEN([Documento]) = 14)"));

        b.Ignore(c => c.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="Contato"/>.</summary>
public sealed class ContatoConfiguracao : IEntityTypeConfiguration<Contato>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Contato> b)
    {
        b.ToTable("Contato", "comercial");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(c => c.ChavePublica).IsUnique().HasDatabaseName("UX_Contato_ChavePublica");

        b.Property(c => c.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.Sobrenome).HasMaxLength(120).IsUnicode(true);
        b.Property(c => c.Cargo).HasMaxLength(80).IsUnicode(true);

        b.Property(c => c.Documento)
            .HasConversion(d => d!.Value.Numero, s => CpfCnpj.Criar(s))
            .HasMaxLength(14).IsUnicode(false);

        b.Property(c => c.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.AlteradoEm).HasPrecision(3);
        b.Property(c => c.ExcluidoEm).HasPrecision(3);

        b.HasIndex(c => new { c.EmpresaId, c.Documento })
            .IsUnique()
            .HasFilter("[Documento] IS NOT NULL AND [ExcluidoEm] IS NULL")
            .HasDatabaseName("UX_Contato_Empresa_Documento");

        b.HasIndex(c => c.ProprietarioId).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => c.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(c => c.ProprietarioId).OnDelete(DeleteBehavior.Restrict);

        // Contato é pessoa física: se informar documento, é CPF.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Contato_Documento", "[Documento] IS NULL OR LEN([Documento]) = 11"));

        b.Ignore(c => c.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="ClienteContato"/>.</summary>
public sealed class ClienteContatoConfiguracao : IEntityTypeConfiguration<ClienteContato>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClienteContato> b)
    {
        b.ToTable("ClienteContato", "comercial");
        b.HasKey(v => v.Id);
        b.Property(v => v.Id).ValueGeneratedOnAdd();
        b.Property(v => v.EhPrincipal).IsRequired();

        b.HasIndex(v => new { v.ClienteId, v.ContatoId, v.PapelId })
            .IsUnique()
            .HasDatabaseName("UX_ClienteContato_Cliente_Contato_Papel");

        // No máximo um contato principal por cliente — índice único parcial.
        b.HasIndex(v => v.ClienteId)
            .IsUnique()
            .HasFilter("[EhPrincipal] = 1")
            .HasDatabaseName("UX_ClienteContato_Principal");

        b.HasIndex(v => v.ContatoId);

        b.HasOne<Cliente>().WithMany().HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Contato>().WithMany().HasForeignKey(v => v.ContatoId).OnDelete(DeleteBehavior.Restrict);

        // O papel vem do catálogo PAPEL_CONTATO — e SÓ dele. É a chave estrangeira composta
        // (CatalogoDoPapelId, PapelId) que faz valer o "verificado pelo banco" da seção 8.11 do
        // documento 17: sem ela, este campo aceitava um item do catálogo de culturas.
        b.LigarAoCatalogoDeSistema(
            nameof(ClienteContato.PapelId), "CatalogoDoPapelId", CatalogosDeSistema.PapelDeContato);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_ClienteContato_Periodo", "[EncerrouEm] IS NULL OR [IniciouEm] IS NULL OR [EncerrouEm] >= [IniciouEm]"));
    }
}

/// <summary>Mapeamento de <see cref="CanalContato"/>.</summary>
public sealed class CanalContatoConfiguracao : IEntityTypeConfiguration<CanalContato>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CanalContato> b)
    {
        b.ToTable("CanalContato", "comercial");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.Valor).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(c => c.ValorNormalizado).HasMaxLength(200).IsUnicode(false).IsRequired();
        b.Property(c => c.Rotulo).HasMaxLength(40).IsUnicode(true);
        b.Property(c => c.EhPrincipal).IsRequired();
        b.Property(c => c.EhValido).IsRequired();
        b.Property(c => c.ValidadoEm).HasPrecision(3);
        b.Property(c => c.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.ExcluidoEm).HasPrecision(3);

        b.HasIndex(c => c.ValorNormalizado).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => c.ClienteId).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => c.ContatoId).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => c.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(c => c.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Contato>().WithMany().HasForeignKey(c => c.ContatoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_CanalContato_Tipo", "[Tipo] IN ('Email','Telefone','Celular','WhatsApp')"));

        // Exatamente um dono. É o que impede o registro órfão que o Vórtice acumula.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_CanalContato_UmDono",
            "([ClienteId] IS NOT NULL AND [ContatoId] IS NULL) OR ([ClienteId] IS NULL AND [ContatoId] IS NOT NULL)"));
    }
}

/// <summary>Mapeamento de <see cref="Endereco"/>.</summary>
public sealed class EnderecoConfiguracao : IEntityTypeConfiguration<Endereco>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Endereco> b)
    {
        b.ToTable("Endereco", "comercial");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(e => e.ChavePublica).IsUnique().HasDatabaseName("UX_Endereco_ChavePublica");

        b.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.Identificacao).HasMaxLength(120).IsUnicode(true);
        b.Property(e => e.Logradouro).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(e => e.Numero).HasMaxLength(20).IsUnicode(false);
        b.Property(e => e.Complemento).HasMaxLength(120).IsUnicode(true);
        b.Property(e => e.Bairro).HasMaxLength(120).IsUnicode(true);
        // RESÍDUO DE MIGRAÇÃO. Esta coluna deixou de ser obrigatória: endereço novo preenche
        // MunicipioId, nunca ela. O nome dela NÃO mudou de propósito — renomear coluna é o que a
        // regra 9.3 do documento 14 proíbe, e o que a marca como resíduo é a restrição
        // CK_Endereco_Municipio mais o documento 26, seção 7, que conta quantas linhas ainda
        // dependem dela e diz o que fecha essa conta. O destino dela é sair, não ser renomeada.
        b.Property(e => e.Municipio).HasMaxLength(120).IsUnicode(true);
        b.Property(e => e.Uf).HasMaxLength(2).IsUnicode(false).IsFixedLength().IsRequired();
        b.Property(e => e.EhPrincipal).IsRequired();

        b.Property(e => e.Cep)
            .HasConversion(c => c!.Value.Numero, s => Dominio.Comum.Cep.Criar(s))
            .HasMaxLength(8).IsUnicode(false).IsFixedLength();

        b.Property(e => e.Hectares).HasPrecision(12, 2);

        // Grau decimal com sete casas: precisão de cerca de um centímetro, que é mais do que
        // o GPS de campo entrega. A coordenada é o que o mapa da Cobertura desenha.
        b.Property(e => e.Latitude).HasPrecision(10, 7);
        b.Property(e => e.Longitude).HasPrecision(10, 7);

        b.Property(e => e.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(e => e.AlteradoEm).HasPrecision(3);
        b.Property(e => e.ExcluidoEm).HasPrecision(3);

        b.HasIndex(e => e.ClienteId).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(e => e.ClienteId)
            .IsUnique()
            .HasFilter("[EhPrincipal] = 1 AND [ExcluidoEm] IS NULL")
            .HasDatabaseName("UX_Endereco_Principal");
        b.HasIndex(e => new { e.Uf, e.Municipio }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(e => e.EmpresaId);

        // O caminho de leitura da Cobertura territorial: "quais clientes há neste município?".
        b.HasIndex(e => e.MunicipioId).HasFilter("[ExcluidoEm] IS NULL");

        b.HasOne<Empresa>().WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(e => e.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Organizacao.Municipio>().WithMany()
            .HasForeignKey(e => e.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.LigarAoCatalogoDeSistema(
            nameof(Endereco.CulturaId), "CatalogoDaCulturaId", CatalogosDeSistema.Cultura);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_Endereco_Tipo", "[Tipo] IN ('Fiscal','Entrega','Cobranca','Fazenda')"));
        b.ToTable(t => t.HasCheckConstraint("CK_Endereco_Uf", "[Uf] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z]'"));

        // O MUNICÍPIO É SELEÇÃO, E O TEXTO É RESÍDUO — uma coisa OU a outra, nunca as duas nem
        // nenhuma. É esta restrição que impede o texto livre de voltar pela porta dos fundos: um
        // INSERT que preencha os dois é recusado pelo banco, e o que preenche só o texto está
        // declarando, no próprio dado, que aquela linha é dívida de migração — o documento 26,
        // seção 7, conta quantas são e o que fecha a conta.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Endereco_Municipio",
            "([MunicipioId] IS NOT NULL AND [Municipio] IS NULL) " +
            "OR ([MunicipioId] IS NULL AND [Municipio] IS NOT NULL)"));
        b.ToTable(t => t.HasCheckConstraint("CK_Endereco_Hectares", "[Hectares] IS NULL OR [Hectares] > 0"));

        // Coordenada é par: ou vêm as duas, ou não vem nenhuma. E cada uma dentro da faixa
        // que existe no planeta — o mesmo que o tipo de valor Coordenada já garante no C#.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Endereco_Coordenada",
            "([Latitude] IS NULL AND [Longitude] IS NULL) " +
            "OR ([Latitude] BETWEEN -90 AND 90 AND [Longitude] BETWEEN -180 AND 180)"));

        b.Ignore(e => e.Eventos);
        b.Ignore(e => e.Localizacao);
    }
}

/// <summary>Mapeamento de <see cref="ConsentimentoComunicacao"/>.</summary>
public sealed class ConsentimentoComunicacaoConfiguracao : IEntityTypeConfiguration<ConsentimentoComunicacao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ConsentimentoComunicacao> b)
    {
        b.ToTable("ConsentimentoComunicacao", "comercial");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Canal).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.Finalidade).HasConversion<string>().HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Concedido).IsRequired();
        b.Property(c => c.OrigemEvidencia).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(c => c.ReferenciaEvidencia).HasMaxLength(400).IsUnicode(true);
        b.Property(c => c.EnderecoIp).HasMaxLength(45).IsUnicode(false);
        b.Property(c => c.CriadoEm).HasPrecision(3).IsRequired();

        // O tipo de valor garante que a data da decisão está em UTC — sem isso, "quando o
        // titular autorizou" depende do fuso de quem gravou.
        b.Property(c => c.DecididaEm)
            .HasConversion(d => d.Valor, v => DataHoraUtc.Criar(v))
            .HasPrecision(3).IsRequired();

        // A consulta quente: "posso mandar e-mail de marketing para este contato?" — a
        // resposta é a linha mais recente do trio.
        b.HasIndex(c => new { c.ContatoId, c.Canal, c.Finalidade, c.DecididaEm });
        b.HasIndex(c => new { c.ClienteId, c.Canal, c.Finalidade, c.DecididaEm });
        b.HasIndex(c => c.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(c => c.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Contato>().WithMany().HasForeignKey(c => c.ContatoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_ConsentimentoComunicacao_Canal",
            "[Canal] IN ('Email','Sms','WhatsApp','Telefone','Correspondencia')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ConsentimentoComunicacao_Finalidade",
            "[Finalidade] IN ('Marketing','Transacional','Pesquisa','Cobranca')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ConsentimentoComunicacao_UmTitular",
            "([ClienteId] IS NOT NULL AND [ContatoId] IS NULL) OR ([ClienteId] IS NULL AND [ContatoId] IS NOT NULL)"));
    }
}

/// <summary>Mapeamento de <see cref="ClienteCarteira"/>.</summary>
public sealed class ClienteCarteiraConfiguracao : IEntityTypeConfiguration<ClienteCarteira>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClienteCarteira> b)
    {
        b.ToTable("ClienteCarteira", "comercial");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Classe).HasConversion<string>().HasMaxLength(1).IsUnicode(false).IsFixedLength().IsRequired();

        b.Property(c => c.PotencialAnual)
            .HasConversion(d => d!.Value.Valor, v => Dinheiro.Criar(v))
            .HasPrecision(18, 2);

        b.Property(c => c.UltimaInteracaoEm).HasPrecision(3);
        b.Property(c => c.VinculadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.DesvinculadoEm).HasPrecision(3);

        b.HasIndex(c => new { c.ClienteId, c.CarteiraId })
            .IsUnique()
            .HasFilter("[DesvinculadoEm] IS NULL")
            .HasDatabaseName("UX_ClienteCarteira_Cliente_Carteira_Vigente");

        // O índice da tela de Cobertura: a carteira ordenada por quem está há mais tempo
        // sem contato.
        b.HasIndex(c => new { c.CarteiraId, c.Classe, c.UltimaInteracaoEm })
            .HasFilter("[DesvinculadoEm] IS NULL");

        b.HasOne<Cliente>().WithMany().HasForeignKey(c => c.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Carteira>().WithMany().HasForeignKey(c => c.CarteiraId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_ClienteCarteira_Classe", "[Classe] IN ('A','B','C','D')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ClienteCarteira_Ciclo", "[DiasCicloContato] IS NULL OR [DiasCicloContato] > 0"));
    }
}

/// <summary>Mapeamento de <see cref="Alerta"/>.</summary>
public sealed class AlertaConfiguracao : IEntityTypeConfiguration<Alerta>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Alerta> b)
    {
        b.ToTable("Alerta", "comercial");
        b.HasKey(a => a.Id);
        b.Property(a => a.Id).ValueGeneratedOnAdd();

        b.Property(a => a.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(a => a.ChavePublica).IsUnique().HasDatabaseName("UX_Alerta_ChavePublica");

        b.Property(a => a.Severidade).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(a => a.Titulo).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(a => a.Detalhe).HasMaxLength(2000).IsUnicode(true);
        b.Property(a => a.EstaAtivo).IsRequired();

        b.Property(a => a.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(a => a.AlteradoEm).HasPrecision(3);
        b.Property(a => a.ExcluidoEm).HasPrecision(3);

        b.HasIndex(a => a.ClienteId).HasFilter("[EstaAtivo] = 1 AND [ExcluidoEm] IS NULL");
        b.HasIndex(a => a.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(a => a.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(a => a.ClienteId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_Alerta_Severidade", "[Severidade] IN ('Informativo','Atencao','Critico')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Alerta_Vigencia", "[VigenteAte] IS NULL OR [VigenteAte] >= [VigenteDe]"));

        b.Ignore(a => a.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="FaturamentoDoCliente"/> — schema <c>comercial</c>.</summary>
public sealed class FaturamentoDoClienteConfiguracao : IEntityTypeConfiguration<FaturamentoDoCliente>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FaturamentoDoCliente> b)
    {
        b.ToTable("FaturamentoDoCliente", "comercial");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();

        b.Property(f => f.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(f => f.ChavePublica).IsUnique().HasDatabaseName("UX_FaturamentoDoCliente_ChavePublica");

        b.Property(f => f.Competencia).IsRequired();
        b.Property(f => f.ValorLiquido).HasPrecision(18, 2).IsRequired();
        b.Property(f => f.Notas).IsRequired();
        b.Property(f => f.Itens).IsRequired();

        b.Property(f => f.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(f => f.AlteradoEm).HasPrecision(3);
        b.Property(f => f.ExcluidoEm).HasPrecision(3);

        // UM MÊS POR CLIENTE POR FILIAL, e o índice único é quem garante. Sem ele, uma segunda
        // execução da carga criaria a linha de novo em vez de reapurar a existente.
        b.HasIndex(f => new { f.ClienteId, f.EmpresaId, f.Competencia })
            .IsUnique()
            .HasDatabaseName("UX_FaturamentoDoCliente_Cliente_Empresa_Competencia");

        // A série de doze meses da diretoria: filial e competência, nesta ordem.
        b.HasIndex(f => new { f.EmpresaId, f.Competencia }).HasFilter("[ExcluidoEm] IS NULL");

        b.HasOne<Empresa>().WithMany().HasForeignKey(f => f.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(f => f.ClienteId).OnDelete(DeleteBehavior.Restrict);

        // Nota fiscal não tem valor negativo, e devolução vem como nota própria na origem.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_FaturamentoDoCliente_Valor", "[ValorLiquido] >= 0"));

        b.ToTable(x => x.HasCheckConstraint(
            "CK_FaturamentoDoCliente_Competencia", "DAY([Competencia]) = 1"));

        b.Ignore(f => f.Eventos);
    }
}
