using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="Empresa"/> — schema <c>organizacao</c>.
///
/// CONVENÇÃO DO PROJETO — use <c>HasMaxLength</c>, <c>IsUnicode</c> e <c>HasPrecision</c>,
/// nunca <c>HasColumnType("...")</c> escrito à mão, salvo o JSON, que não tem API agnóstica
/// (hoje só <c>nvarchar(max)</c> com <c>CHECK (ISJSON(...) = 1)</c>) e está na lista de
/// exceções justificadas do teste <c>TiposDeColunaTestes</c>. O nome da coluna é o PascalCase
/// do C#, sem conversão nenhuma: em C# é PascalCase, no banco é
/// <c>ChavePublica</c>, letra por letra igual — é o vocabulário do documento 15 chegando
/// intacto ao banco.
/// </summary>
public sealed class EmpresaConfiguracao : IEntityTypeConfiguration<Empresa>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Empresa> b)
    {
        b.ToTable("Empresa", "organizacao");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(e => e.ChavePublica).IsUnique().HasDatabaseName("UX_Empresa_ChavePublica");

        b.Property(e => e.Codigo).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(e => e.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();

        b.Property(e => e.Cnpj)
            .HasConversion(c => c!.Value.Numero, s => CpfCnpj.Criar(s))
            .HasMaxLength(14).IsUnicode(false);

        b.Property(e => e.Caminho).HasMaxLength(400).IsUnicode(false).IsRequired();
        b.Property(e => e.Nivel).IsRequired();
        b.Property(e => e.EstaAtiva).IsRequired();
        b.Property(e => e.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(e => e.AlteradoEm).HasPrecision(3);

        b.HasIndex(e => e.Codigo).IsUnique().HasDatabaseName("UX_Empresa_Codigo");

        // O caminho materializado só serve para "esta empresa e todas abaixo": índice parcial
        // sobre as ativas, que é a única faixa que a consulta de escopo percorre.
        b.HasIndex(e => e.Caminho).HasFilter("[EstaAtiva] = 1");

        b.HasOne<Empresa>()
            .WithMany()
            .HasForeignKey(e => e.EmpresaPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(e => e.EmpresaPaiId);

        b.ToTable(t => t.HasCheckConstraint("CK_Empresa_Nivel", "[Nivel] >= 0"));
    }
}

/// <summary>Mapeamento de <see cref="LinhaDeNegocio"/>.</summary>
public sealed class LinhaDeNegocioConfiguracao : IEntityTypeConfiguration<LinhaDeNegocio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<LinhaDeNegocio> b)
    {
        b.ToTable("LinhaDeNegocio", "organizacao");
        b.HasKey(l => l.Id);
        b.Property(l => l.Id).ValueGeneratedOnAdd();

        b.Property(l => l.Codigo).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(l => l.Nome).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(l => l.Ordem).IsRequired();
        b.Property(l => l.EstaAtiva).IsRequired();

        b.HasIndex(l => l.Codigo).IsUnique().HasDatabaseName("UX_LinhaDeNegocio_Codigo");

        // O ciclo de contato é em dias e nunca é negativo — se a linha declara um ciclo, ele
        // vale para a tela de Cobertura decidir se o cliente está atrasado.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_LinhaDeNegocio_Ciclo",
            "COALESCE([DiasCicloClasseA], 1) > 0 AND COALESCE([DiasCicloClasseB], 1) > 0 " +
            "AND COALESCE([DiasCicloClasseC], 1) > 0 AND COALESCE([DiasCicloClasseD], 1) > 0"));
    }
}

/// <summary>Mapeamento de <see cref="Praca"/>.</summary>
public sealed class PracaConfiguracao : IEntityTypeConfiguration<Praca>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Praca> b)
    {
        b.ToTable("Praca", "organizacao");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.Codigo).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(p => p.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(p => p.Uf).HasMaxLength(2).IsUnicode(false).IsFixedLength();
        b.Property(p => p.AnoReferencia).IsRequired();
        b.Property(p => p.EstaAtiva).IsRequired();

        b.Property(p => p.PotencialEstimado)
            .HasConversion(d => d!.Value.Valor, v => Dinheiro.Criar(v))
            .HasPrecision(18, 2);

        b.HasIndex(p => new { p.Codigo, p.LinhaDeNegocioId, p.AnoReferencia })
            .IsUnique()
            .HasDatabaseName("UX_Praca_Codigo_Linha_Ano");

        b.HasOne<LinhaDeNegocio>()
            .WithMany()
            .HasForeignKey(p => p.LinhaDeNegocioId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(p => p.LinhaDeNegocioId);

        // Duas letras maiúsculas, e só. [V] a UF do Vórtice é texto livre.
        b.ToTable(t => t.HasCheckConstraint("CK_Praca_Uf", "[Uf] IS NULL OR [Uf] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z]'"));
        b.ToTable(t => t.HasCheckConstraint("CK_Praca_AnoReferencia", "[AnoReferencia] BETWEEN 2000 AND 2100"));
    }
}

/// <summary>
/// Mapeamento de <see cref="Municipio"/> — o catálogo nacional que fecha o campo de município.
///
/// <para><b>Sem <c>EmpresaId</c>, e é decisão registrada</b> (documento 26, seção 5.2): município
/// é dado nacional e não pertence a filial nenhuma. A regra 11.1 do documento 14 proíbe
/// justamente a tabela replicada por filial, e o <c>EmpresaId</c> aqui seria isso com outro
/// nome.</para>
///
/// <para><b>Duas unicidades, e as duas são de negócio.</b> O código do IBGE é a chave natural
/// quando existe — o índice é filtrado porque a origem não tem esse código e a maior parte das
/// linhas nasce sem ele. Nome mais UF é a unicidade que vale sempre, e sob a colação
/// <c>Latin1_General_CI_AI</c> ela também recusa <c>CAJURU</c> ao lado de <c>Cajuru</c> e de
/// <c>Cajurú</c>.</para>
/// </summary>
public sealed class MunicipioConfiguracao : IEntityTypeConfiguration<Municipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Municipio> b)
    {
        b.ToTable("Municipio", "organizacao");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();

        b.Property(m => m.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(m => m.Uf).HasMaxLength(2).IsUnicode(false).IsFixedLength().IsRequired();
        b.Property(m => m.EstaAtivo).IsRequired();

        b.HasIndex(m => new { m.Uf, m.Nome })
            .IsUnique()
            .HasDatabaseName("UX_Municipio_Uf_Nome");

        // O código do IBGE é único QUANDO EXISTE. O filtro é o que permite as duas coisas ao
        // mesmo tempo: a chave natural presa pelo banco, e a carga podendo trazer município sem
        // o código — que é o caso de hoje, porque GE_Cidade não tem essa coluna.
        b.HasIndex(m => m.CodigoIbge)
            .IsUnique()
            .HasFilter("[CodigoIbge] IS NOT NULL")
            .HasDatabaseName("UX_Municipio_CodigoIbge");

        // A busca da tela: "digite o município" filtrando por estado.
        b.HasIndex(m => m.Nome).HasFilter("[EstaAtivo] = 1");

        // DOMÍNIO FECHADO, as 27 unidades enumeradas. A colação binária é obrigatória aqui: a do
        // banco ignora caixa e acento, e sem ela a restrição aceitaria 'sp' e 'PÁ' — que são
        // literalmente dois dos valores que a UF de texto livre do legado guarda hoje.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Municipio_Uf",
            "[Uf] COLLATE Latin1_General_BIN2 IN ('AC','AL','AP','AM','BA','CE','DF','ES','GO'," +
            "'MA','MT','MS','MG','PA','PB','PR','PE','PI','RJ','RN','RS','RO','RR','SC','SP'," +
            "'SE','TO')"));

        // Sete dígitos: o código do IBGE vai de 1100015 (Alta Floresta d'Oeste) a 5300108
        // (Brasília). A faixa é a do próprio padrão, não um palpite de tamanho.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_Municipio_CodigoIbge",
            "[CodigoIbge] IS NULL OR [CodigoIbge] BETWEEN 1000000 AND 5999999"));
    }
}

/// <summary>
/// Mapeamento de <see cref="CarteiraMunicipio"/> — quais cidades cada carteira atende.
///
/// <para><b>Sem <c>EmpresaId</c>, e é decisão registrada</b> (documento 26, seção 5.2): esta
/// linha é a ponte entre a carteira (que tem a coluna e o filtro global de multiempresa) e o
/// município (que é nacional). É a mesma forma de <c>comercial.ClienteCarteira</c>, e pelo mesmo
/// motivo: uma segunda cópia de "de quem é esta linha" pode divergir da primeira.</para>
/// </summary>
public sealed class CarteiraMunicipioConfiguracao : IEntityTypeConfiguration<CarteiraMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CarteiraMunicipio> b)
    {
        b.ToTable("CarteiraMunicipio", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.VinculadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.DesvinculadoEm).HasPrecision(3);

        // A UNICIDADE DE NEGÓCIO: o par não se repete enquanto vigente. Filtrado porque
        // desvincular não apaga a linha — e o mesmo município pode voltar para a mesma carteira
        // depois, o que criaria uma segunda linha legítima.
        b.HasIndex(c => new { c.CarteiraId, c.MunicipioId })
            .IsUnique()
            .HasFilter("[DesvinculadoEm] IS NULL")
            .HasDatabaseName("UX_CarteiraMunicipio_Carteira_Municipio_Vigente");

        // A consulta da Cobertura no sentido inverso: "que carteira atende esta cidade?".
        b.HasIndex(c => c.MunicipioId).HasFilter("[DesvinculadoEm] IS NULL");

        b.HasOne<Carteira>().WithMany().HasForeignKey(c => c.CarteiraId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Municipio>().WithMany().HasForeignKey(c => c.MunicipioId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>Mapeamento de <see cref="HierarquiaComercial"/>.</summary>
public sealed class HierarquiaComercialConfiguracao : IEntityTypeConfiguration<HierarquiaComercial>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<HierarquiaComercial> b)
    {
        b.ToTable("HierarquiaComercial", "organizacao");
        b.HasKey(h => h.Id);
        b.Property(h => h.Id).ValueGeneratedOnAdd();

        b.Property(h => h.Profundidade).IsRequired();

        b.HasIndex(h => new { h.AncestralId, h.DescendenteId })
            .IsUnique()
            .HasDatabaseName("UX_HierarquiaComercial_Ancestral_Descendente");

        // A consulta quente é a inversa: "quem está acima de mim", para resolver a
        // profundidade de permissão sem varrer.
        b.HasIndex(h => new { h.DescendenteId, h.Profundidade });

        b.HasOne<Dominio.Seguranca.Usuario>()
            .WithMany()
            .HasForeignKey(h => h.AncestralId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne<Dominio.Seguranca.Usuario>()
            .WithMany()
            .HasForeignKey(h => h.DescendenteId)
            .OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_HierarquiaComercial_Profundidade", "[Profundidade] >= 0"));
    }
}

/// <summary>Mapeamento de <see cref="Carteira"/>.</summary>
public sealed class CarteiraConfiguracao : IEntityTypeConfiguration<Carteira>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Carteira> b)
    {
        b.ToTable("Carteira", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(c => c.ChavePublica).IsUnique().HasDatabaseName("UX_Carteira_ChavePublica");

        b.Property(c => c.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.EstaAtiva).IsRequired();

        // A NATUREZA VAI COMO TEXTO, e não como número: quem abrir a tabela no DBeaver lê
        // "Administrativa" e entende, em vez de ler "1" e ter de procurar o enum no código.
        // Vale para todo enum do modelo (documento 14).
        b.Property(c => c.Natureza)
            .HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        // O índice que a gerência usa: as carteiras COMERCIAIS de uma filial. Sem ele, filtrar
        // por natureza custaria uma varredura em toda consulta de cobertura.
        b.HasIndex(c => new { c.EmpresaId, c.Natureza }).HasFilter("[ExcluidoEm] IS NULL");

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Carteira_Natureza", "[Natureza] IN ('Comercial','Administrativa','Teste')"));

        b.Property(c => c.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(c => c.AlteradoEm).HasPrecision(3);
        b.Property(c => c.ExcluidoEm).HasPrecision(3);

        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_Carteira_Codigo");

        // O caminho de leitura diário: "minhas carteiras".
        b.HasIndex(c => c.ResponsavelId).HasFilter("[EstaAtiva] = 1 AND [ExcluidoEm] IS NULL");
        b.HasIndex(c => new { c.EmpresaId, c.LinhaDeNegocioId }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(c => c.SupervisorId);
        b.HasIndex(c => c.EquipeId);
        b.HasIndex(c => c.PracaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LinhaDeNegocio>().WithMany().HasForeignKey(c => c.LinhaDeNegocioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Praca>().WithMany().HasForeignKey(c => c.PracaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.ResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Equipe>().WithMany().HasForeignKey(c => c.EquipeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Ignore(c => c.Eventos);
    }
}

/// <summary>Mapeamento de <see cref="Meta"/>.</summary>
public sealed class MetaConfiguracao : IEntityTypeConfiguration<Meta>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Meta> b)
    {
        b.ToTable("Meta", "organizacao");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();

        b.Property(m => m.ChavePublica).IsRequired().HasDefaultValueSql("NEWID()");
        b.HasIndex(m => m.ChavePublica).IsUnique().HasDatabaseName("UX_Meta_ChavePublica");

        b.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(m => m.Alvo).HasPrecision(18, 2).IsRequired();
        b.Property(m => m.Observacao).HasMaxLength(400).IsUnicode(true);
        b.Property(m => m.EstaAtiva).IsRequired();

        b.Property(m => m.CriadoEm).HasPrecision(3).IsRequired();
        b.Property(m => m.AlteradoEm).HasPrecision(3);
        b.Property(m => m.ExcluidoEm).HasPrecision(3);

        b.HasIndex(m => new { m.EmpresaId, m.Tipo, m.PeriodoInicio }).HasFilter("[ExcluidoEm] IS NULL");
        b.HasIndex(m => m.CarteiraId);
        b.HasIndex(m => m.UsuarioId);
        b.HasIndex(m => m.LinhaDeNegocioId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(m => m.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LinhaDeNegocio>().WithMany().HasForeignKey(m => m.LinhaDeNegocioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Carteira>().WithMany().HasForeignKey(m => m.CarteiraId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_Meta_Tipo",
            "[Tipo] IN ('Faturamento','Cobertura','Frequencia','Volume')"));

        b.ToTable(t => t.HasCheckConstraint("CK_Meta_Periodo", "[PeriodoFim] >= [PeriodoInicio]"));
        b.ToTable(t => t.HasCheckConstraint("CK_Meta_Alvo", "[Alvo] >= 0"));

        b.Ignore(m => m.Eventos);
    }
}
