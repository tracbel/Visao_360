using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Crm;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento do <see cref="Lead"/>. Segue os documentos 04 e 17 coluna por coluna.
///
/// CONVENÇÃO DO PROJETO — use <c>HasMaxLength</c>, <c>IsUnicode</c> e <c>HasPrecision</c>,
/// nunca <c>HasColumnType("...")</c> escrito à mão, com uma única exceção justificada: o JSON,
/// que no SQL Server é <c>nvarchar(max)</c> com <c>CHECK (ISJSON(...) = 1)</c> e não tem API
/// agnóstica de provedor. A lista de exceções vive no teste <c>TiposDeColunaTestes</c>, e
/// entrar nela é decisão revisada em PR.
///
/// O nome da coluna é o PascalCase do C#: <c>NomeContato</c> em C# é <c>NomeContato</c> no
/// banco. Por isso todo filtro de índice e toda restrição de verificação escrevem o nome da
/// coluna ENTRE COLCHETES — é a sintaxe do SQL Server, e é o que impede uma coluna com nome
/// de palavra reservada de quebrar a migração.
/// </summary>
public sealed class LeadConfiguracao : IEntityTypeConfiguration<Lead>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Lead> b)
    {
        b.ToTable("Lead", "comercial");
        b.HasKey(l => l.Id);
        b.Property(l => l.Id).ValueGeneratedOnAdd();

        // O identificador que a API expõe. Indexado porque toda busca vem por ele.
        b.Property(l => l.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(l => l.ChavePublica).IsUnique().HasDatabaseName("UX_Lead_ChavePublica");

        b.Property(l => l.EmpresaId).IsRequired();

        // Unicode: nome de gente tem acento.
        b.Property(l => l.NomeContato).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(l => l.NomeEmpresa).HasMaxLength(200).IsUnicode(true);
        b.Property(l => l.Interesse).HasMaxLength(400).IsUnicode(true);

        // ---- Tipos de valor, gravados como coluna simples ----
        // O conversor mantém o domínio limpo (Lead tem Email, não string) sem tabela extra.
        b.Property(l => l.Email)
            .HasConversion(e => e!.Value.Endereco, s => Dominio.Comum.Email.Criar(s))
            .HasMaxLength(200).IsUnicode(true);

        // Texto, NUNCA numérico.
        // [V] GE_Pessoa.FoneNro1 é decimal(12): telefone com DDI ou zero à esquerda estoura
        // o tipo, e a API devolve uma mensagem genérica sem dizer o motivo.
        b.Property(l => l.Telefone)
            .HasConversion(t => t!.Value.Numero, s => Dominio.Comum.Telefone.Criar(s))
            .HasMaxLength(20).IsUnicode(false);

        b.Property(l => l.Documento)
            .HasConversion(d => d!.Value.Numero, s => CpfCnpj.Criar(s))
            .HasMaxLength(14).IsUnicode(false);

        // ---- Ciclo de vida ----
        // String, não int: o banco fica legível, e inserir um valor novo no enum não
        // reordena o que já está gravado.
        b.Property(l => l.Situacao)
            .HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        // [V] "IV_Processo.Status tem mais de 20 valores de texto livre com duplicatas
        // semânticas: FINALIZADO convive com FINALIZADA, e 437.694 linhas estão em branco"
        // (extração de 02/09/2026, achado 7) — porque o Vórtice tem ZERO CHECK constraints em
        // 767 tabelas. O enum já fecha o domínio no C#; o CHECK fecha o domínio no banco
        // também, para quem gravar fora da aplicação (documento 14, seções 4 e 6).
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Lead_Situacao",
            "[Situacao] IN ('Novo','EmContato','Qualificado','Descartado','Duplicado')"));

        // Se qualificou, tem que ter data, quem e o que gerou. Regra no banco, não só no
        // código — é a mesma constraint do documento 04, seção 4.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Lead_Qualificacao",
            "[Situacao] <> 'Qualificado' " +
            "OR ([QualificadoEm] IS NOT NULL AND [QualificadoPorId] IS NOT NULL AND [ClienteGeradoId] IS NOT NULL)"));

        b.Property(l => l.OrigemId).IsRequired();
        b.Property(l => l.ProprietarioId).IsRequired();

        b.Property(l => l.QualificadoEm).HasPrecision(3);

        // O payload cru do RD Station, guardado SEMPRE — como JSON validado, não como texto
        // qualquer. [V] no Vórtice, campo não mapeado na tela de Integração RD é DESCARTADO:
        // vira texto solto no histórico e some. Guardar o cru permite reprocessar depois, e o
        // CHECK de ISJSON garante que o que foi guardado é documento, não sujeira.
        b.Property(l => l.PayloadOriginal).HasColumnType("nvarchar(max)");
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Lead_PayloadOriginalJson", "[PayloadOriginal] IS NULL OR ISJSON([PayloadOriginal]) = 1"));

        // ---- Auditoria ----
        b.Property(l => l.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(l => l.AlteradoEm).HasPrecision(3);
        b.Property(l => l.ExcluidoEm).HasPrecision(3);
        // A concorrência otimista é configurada no CrmDbContext: a coluna Versao, um
        // rowversion que o SQL Server incrementa a cada gravação da linha.

        // ---- Índices ----
        // Filtro parcial: o registro excluído logicamente não custa espaço de índice nem
        // aparece em varredura de intervalo.
        b.HasIndex(l => new { l.EmpresaId, l.Situacao, l.CriadoEm })
            .HasFilter("[ExcluidoEm] IS NULL");

        b.HasIndex(l => new { l.ProprietarioId, l.Situacao })
            .HasFilter("[ExcluidoEm] IS NULL");

        // Busca por contato: alimenta a deduplicação.
        b.HasIndex(l => l.Email).HasFilter("[Email] IS NOT NULL AND [ExcluidoEm] IS NULL");
        b.HasIndex(l => l.Documento).HasFilter("[Documento] IS NOT NULL AND [ExcluidoEm] IS NULL");

        // ---- Integridade referencial ----
        // [V] a FK que falta é a que dói: 345.535 linhas órfãs no Vórtice porque a agenda, o
        // histórico e o dado do processo não apontam para o processo.
        b.HasOne<Empresa>().WithMany().HasForeignKey(l => l.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(l => l.ProprietarioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(l => l.QualificadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LinhaDeNegocio>().WithMany().HasForeignKey(l => l.LinhaNegocioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(l => l.ClienteGeradoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Contato>().WithMany().HasForeignKey(l => l.ContatoGeradoId).OnDelete(DeleteBehavior.Restrict);

        // Nome completo do tipo: dentro deste arquivo, "Processo" sozinho seria ambíguo com o
        // namespace Tracbel.Crm.Dominio.Processo.
        b.HasOne<Dominio.Processo.Processo>()
            .WithMany()
            .HasForeignKey(l => l.ProcessoGeradoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(l => l.ProcessoGeradoId);

        // Origem e motivo de descarte vêm do catálogo — nunca texto livre, e nunca do catálogo
        // errado. [V] a coluna "Origem" do Vórtice virou texto livre, com duplicatas e valores de
        // teste em produção. A chave estrangeira COMPOSTA (documento 17, seção 8.11) é o que
        // impede que a origem de um lead seja um item do catálogo de tipos de documento.
        b.LigarAoCatalogoDeSistema(
            nameof(Lead.OrigemId), "CatalogoDaOrigemId", CatalogosDeSistema.OrigemDeLead);
        b.LigarAoCatalogoDeSistema(
            nameof(Lead.MotivoDescarteId), "CatalogoDoMotivoDescarteId",
            CatalogosDeSistema.MotivoDeDescarte);

        b.HasIndex(l => l.LinhaNegocioId);
        b.HasIndex(l => l.ClienteGeradoId);
        b.HasIndex(l => l.ContatoGeradoId);
        b.HasIndex(l => l.QualificadoPorId);

        // Eventos de domínio não são persistidos — vivem só na unidade de trabalho.
        b.Ignore(l => l.Eventos);
    }
}
