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

/// <summary>
/// Mapeamento de <see cref="MunicipioDaAreaDeAtuacao"/> — o município na área de atuação e na ADR.
///
/// <para><b>A filial responsável é chave estrangeira, e não a coluna de multiempresa</b>
/// (documento 32, seção 7): o nome <c>EmpresaResponsavelId</c> é deliberado para que o filtro
/// global de filial NÃO se aplique. A ADR é um mapa da empresa inteira.</para>
/// </summary>
public sealed class MunicipioDaAreaDeAtuacaoConfiguracao : IEntityTypeConfiguration<MunicipioDaAreaDeAtuacao>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MunicipioDaAreaDeAtuacao> b)
    {
        b.ToTable("MunicipioDaAreaDeAtuacao", "organizacao");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();

        b.Property(m => m.PertenceAAdr).IsRequired();
        b.Property(m => m.Regiao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(m => m.ArquivoDeOrigem).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(m => m.LinhaNaOrigem).IsRequired();
        b.Property(m => m.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(m => m.EncerradoEm).HasPrecision(3);

        // UM MUNICÍPIO, UMA LINHA VIGENTE. Filtrado porque encerrar não apaga: o município que sai
        // numa revisão e volta na seguinte ganha uma segunda linha, e a primeira conta a história.
        b.HasIndex(m => m.MunicipioId)
            .IsUnique()
            .HasFilter("[EncerradoEm] IS NULL")
            .HasDatabaseName("UX_MunicipioDaAreaDeAtuacao_Municipio_Vigente");

        b.HasIndex(m => m.EmpresaResponsavelId);
        b.HasIndex(m => m.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(m => m.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Empresa>().WithMany().HasForeignKey(m => m.EmpresaResponsavelId).OnDelete(DeleteBehavior.Restrict);

        // QUEM RODOU A CARGA É UM USUÁRIO DE VERDADE, e o banco confere. Sem a chave, um ponteiro
        // para usuário apagado ou inventado passaria — e rastro que não aponta para ninguém não é rastro.
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(m => m.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_MunicipioDaAreaDeAtuacao_Regiao", "[Regiao] IN ('NaoInformada','Norte','Noroeste')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_MunicipioDaAreaDeAtuacao_LinhaNaOrigem", "[LinhaNaOrigem] >= 2"));
    }
}

/// <summary>
/// Mapeamento de <see cref="ResponsavelPeloMunicipio"/> — o que cada planilha afirma sobre quem
/// responde por cada município.
///
/// <para><b>A unicidade é por fonte</b>: o mesmo município tem, ao mesmo tempo, o CEN que a
/// planilha de área de atuação declara e o que a planilha de CEN declara. É isso que deixa a
/// divergência visível em vez de uma sobrescrever a outra.</para>
/// </summary>
public sealed class ResponsavelPeloMunicipioConfiguracao : IEntityTypeConfiguration<ResponsavelPeloMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ResponsavelPeloMunicipio> b)
    {
        b.ToTable("ResponsavelPeloMunicipio", "organizacao");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedOnAdd();

        b.Property(r => r.Papel).HasConversion<string>().HasMaxLength(10).IsUnicode(false).IsRequired();
        b.Property(r => r.Fonte).HasConversion<string>().HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(r => r.Situacao).HasConversion<string>().HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(r => r.NomeNaOrigem).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(r => r.ChaveNaOrigem).HasMaxLength(120).IsUnicode(true);
        b.Property(r => r.ArquivoDeOrigem).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(r => r.LinhaNaOrigem).IsRequired();
        b.Property(r => r.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(r => r.EncerradoEm).HasPrecision(3);

        b.HasIndex(r => new { r.MunicipioId, r.Papel, r.Fonte })
            .IsUnique()
            .HasFilter("[EncerradoEm] IS NULL")
            .HasDatabaseName("UX_ResponsavelPeloMunicipio_Municipio_Papel_Fonte_Vigente");

        b.HasIndex(r => r.UsuarioId);
        b.HasIndex(r => r.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(r => r.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(r => r.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(r => r.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_ResponsavelPeloMunicipio_Papel", "[Papel] IN ('Cen','Gestor')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ResponsavelPeloMunicipio_Fonte",
            "[Fonte] IN ('PlanilhaAreaDeAtuacao','PlanilhaCenEGestorPorMunicipio')"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ResponsavelPeloMunicipio_Situacao",
            "[Situacao] IN ('UsuarioIdentificado','VagaAContratar','NaoIdentificado')"));

        // O PAR SITUAÇÃO × USUÁRIO, preso no banco como o domínio o prende no código: só o
        // identificado aponta para usuário, e todo identificado aponta.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ResponsavelPeloMunicipio_UsuarioIdentificado",
            "([Situacao] = 'UsuarioIdentificado' AND [UsuarioId] IS NOT NULL) " +
            "OR ([Situacao] <> 'UsuarioIdentificado' AND [UsuarioId] IS NULL)"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ResponsavelPeloMunicipio_LinhaNaOrigem", "[LinhaNaOrigem] >= 2"));
    }
}

/// <summary>
/// Mapeamento de <see cref="ProducaoAgricolaNoMunicipio"/> — as quatro medidas da PAM/IBGE.
///
/// <para>Área é <c>decimal(14,2)</c>: a maior área de um produto num município paulista é de dezenas
/// de milhares de hectares, e o IBGE publica em hectares inteiros. Quantidade e valor vão a
/// <c>decimal(18,2)</c>: o valor é em MIL reais e, num município de cana, passa da casa do milhão.
/// Todas anuláveis, porque "não disponível" não é zero (ver a entidade).</para>
/// </summary>
public sealed class ProducaoAgricolaNoMunicipioConfiguracao : IEntityTypeConfiguration<ProducaoAgricolaNoMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProducaoAgricolaNoMunicipio> b)
    {
        b.ToTable("ProducaoAgricolaNoMunicipio", "organizacao");
        b.HasKey(a => a.Id);
        b.Property(a => a.Id).ValueGeneratedOnAdd();

        b.Property(a => a.Ano).IsRequired();
        b.Property(a => a.ProdutoCodigoIbge).IsRequired();
        b.Property(a => a.ProdutoNome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(a => a.AreaPlantadaHectares).HasPrecision(14, 2);
        b.Property(a => a.AreaColhidaHectares).HasPrecision(14, 2);
        b.Property(a => a.QuantidadeProduzidaToneladas).HasPrecision(18, 2);
        b.Property(a => a.ValorDaProducaoMilReais).HasPrecision(18, 2);
        b.Property(a => a.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(a => a.ImportadoPorId).IsRequired();

        b.HasIndex(a => new { a.MunicipioId, a.Ano, a.ProdutoCodigoIbge })
            .IsUnique()
            .HasDatabaseName("UX_ProducaoAgricolaNoMunicipio_Municipio_Ano_Produto");

        // A consulta do mapa: "a área deste produto, neste ano, em todos os municípios".
        b.HasIndex(a => new { a.ProdutoCodigoIbge, a.Ano });
        b.HasIndex(a => a.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(a => a.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(a => a.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_ProducaoAgricolaNoMunicipio_Ano", "[Ano] BETWEEN 1974 AND 2100"));
        b.ToTable(t => t.HasCheckConstraint("CK_ProducaoAgricolaNoMunicipio_Produto", "[ProdutoCodigoIbge] > 0"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ProducaoAgricolaNoMunicipio_Medidas",
            "([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0) " +
            "AND ([AreaColhidaHectares] IS NULL OR [AreaColhidaHectares] >= 0) " +
            "AND ([QuantidadeProduzidaToneladas] IS NULL OR [QuantidadeProduzidaToneladas] >= 0) " +
            "AND ([ValorDaProducaoMilReais] IS NULL OR [ValorDaProducaoMilReais] >= 0)"));
    }
}

/// <summary>
/// Mapeamento de <see cref="ProducaoAgricolaNoEstado"/> — a linha da UF inteira, que o IBGE publica
/// e que <b>não</b> é a soma dos municípios (ver a entidade).
/// </summary>
public sealed class ProducaoAgricolaNoEstadoConfiguracao : IEntityTypeConfiguration<ProducaoAgricolaNoEstado>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProducaoAgricolaNoEstado> b)
    {
        b.ToTable("ProducaoAgricolaNoEstado", "organizacao");
        b.HasKey(a => a.Id);
        b.Property(a => a.Id).ValueGeneratedOnAdd();

        b.Property(a => a.EstadoCodigoIbge).IsRequired();
        b.Property(a => a.Ano).IsRequired();
        b.Property(a => a.ProdutoCodigoIbge).IsRequired();
        b.Property(a => a.ProdutoNome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(a => a.AreaPlantadaHectares).HasPrecision(16, 2);
        b.Property(a => a.AreaColhidaHectares).HasPrecision(16, 2);
        b.Property(a => a.QuantidadeProduzidaToneladas).HasPrecision(20, 2);
        b.Property(a => a.ValorDaProducaoMilReais).HasPrecision(20, 2);
        b.Property(a => a.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(a => a.ImportadoPorId).IsRequired();

        b.HasIndex(a => new { a.EstadoCodigoIbge, a.Ano, a.ProdutoCodigoIbge })
            .IsUnique()
            .HasDatabaseName("UX_ProducaoAgricolaNoEstado_Estado_Ano_Produto");

        b.HasIndex(a => a.ImportadoPorId);

        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(a => a.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_ProducaoAgricolaNoEstado_Ano", "[Ano] BETWEEN 1974 AND 2100"));
        b.ToTable(t => t.HasCheckConstraint("CK_ProducaoAgricolaNoEstado_Estado", "[EstadoCodigoIbge] BETWEEN 11 AND 53"));
        b.ToTable(t => t.HasCheckConstraint("CK_ProducaoAgricolaNoEstado_Produto", "[ProdutoCodigoIbge] > 0"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_ProducaoAgricolaNoEstado_Medidas",
            "([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares] >= 0) " +
            "AND ([AreaColhidaHectares] IS NULL OR [AreaColhidaHectares] >= 0) " +
            "AND ([QuantidadeProduzidaToneladas] IS NULL OR [QuantidadeProduzidaToneladas] >= 0) " +
            "AND ([ValorDaProducaoMilReais] IS NULL OR [ValorDaProducaoMilReais] >= 0)"));
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
        b.HasOne<Empresa>().WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<LinhaDeNegocio>().WithMany().HasForeignKey(c => c.LinhaDeNegocioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.ResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Ignore(c => c.Eventos);
    }
}

