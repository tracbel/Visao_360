using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento de <see cref="FrotaDeTratoresNoMunicipio"/> — o parque de tratores do Censo
/// Agropecuário, por faixa de potência.
///
/// <para>As contagens são <c>int</c> anulável: o maior município de São Paulo tem alguns milhares de
/// tratores, e nulo é o sigilo do IBGE, que não é zero.</para>
/// </summary>
public sealed class FrotaDeTratoresNoMunicipioConfiguracao : IEntityTypeConfiguration<FrotaDeTratoresNoMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<FrotaDeTratoresNoMunicipio> b)
    {
        b.ToTable("FrotaDeTratoresNoMunicipio", "organizacao");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();

        b.Property(f => f.MunicipioId).IsRequired();
        b.Property(f => f.Ano).IsRequired();
        b.Property(f => f.PotenciaCodigoIbge).IsRequired();
        b.Property(f => f.PotenciaNome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(f => f.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(f => f.ImportadoPorId).IsRequired();

        b.HasIndex(f => new { f.MunicipioId, f.Ano, f.PotenciaCodigoIbge })
            .IsUnique()
            .HasDatabaseName("UX_FrotaDeTratoresNoMunicipio_Municipio_Ano_Potencia");

        b.HasIndex(f => f.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(f => f.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(f => f.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_FrotaDeTratoresNoMunicipio_Ano", "[Ano] BETWEEN 1920 AND 2100"));
        b.ToTable(t => t.HasCheckConstraint("CK_FrotaDeTratoresNoMunicipio_Potencia", "[PotenciaCodigoIbge] > 0"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_FrotaDeTratoresNoMunicipio_Contagens",
            "([EstabelecimentosComTrator] IS NULL OR [EstabelecimentosComTrator] >= 0) " +
            "AND ([Tratores] IS NULL OR [Tratores] >= 0)"));
    }
}

/// <summary>
/// Mapeamento de <see cref="EstabelecimentosPorAreaNoMunicipio"/> — os estabelecimentos por grupo de
/// área total do Censo Agropecuário.
/// </summary>
public sealed class EstabelecimentosPorAreaNoMunicipioConfiguracao
    : IEntityTypeConfiguration<EstabelecimentosPorAreaNoMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EstabelecimentosPorAreaNoMunicipio> b)
    {
        b.ToTable("EstabelecimentosPorAreaNoMunicipio", "organizacao");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();

        b.Property(e => e.MunicipioId).IsRequired();
        b.Property(e => e.Ano).IsRequired();
        b.Property(e => e.GrupoDeAreaCodigoIbge).IsRequired();
        b.Property(e => e.GrupoDeAreaNome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(e => e.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(e => e.ImportadoPorId).IsRequired();

        b.HasIndex(e => new { e.MunicipioId, e.Ano, e.GrupoDeAreaCodigoIbge })
            .IsUnique()
            .HasDatabaseName("UX_EstabelecimentosPorAreaNoMunicipio_Municipio_Ano_Grupo");

        b.HasIndex(e => e.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(e => e.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(e => e.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_EstabelecimentosPorAreaNoMunicipio_Ano", "[Ano] BETWEEN 1920 AND 2100"));
        b.ToTable(t => t.HasCheckConstraint("CK_EstabelecimentosPorAreaNoMunicipio_Grupo", "[GrupoDeAreaCodigoIbge] > 0"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_EstabelecimentosPorAreaNoMunicipio_Contagem",
            "[Estabelecimentos] IS NULL OR [Estabelecimentos] >= 0"));
    }
}

/// <summary>
/// Mapeamento de <see cref="RebanhoNoMunicipio"/> — o efetivo dos rebanhos da Pesquisa da Pecuária
/// Municipal, que é anual.
/// </summary>
public sealed class RebanhoNoMunicipioConfiguracao : IEntityTypeConfiguration<RebanhoNoMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RebanhoNoMunicipio> b)
    {
        b.ToTable("RebanhoNoMunicipio", "organizacao");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedOnAdd();

        b.Property(r => r.MunicipioId).IsRequired();
        b.Property(r => r.Ano).IsRequired();
        b.Property(r => r.RebanhoCodigoIbge).IsRequired();
        b.Property(r => r.RebanhoNome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(r => r.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(r => r.ImportadoPorId).IsRequired();

        b.HasIndex(r => new { r.MunicipioId, r.Ano, r.RebanhoCodigoIbge })
            .IsUnique()
            .HasDatabaseName("UX_RebanhoNoMunicipio_Municipio_Ano_Rebanho");

        // A consulta do mapa: "o rebanho bovino deste ano, em todos os municípios".
        b.HasIndex(r => new { r.RebanhoCodigoIbge, r.Ano });
        b.HasIndex(r => r.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(r => r.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(r => r.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_RebanhoNoMunicipio_Ano", "[Ano] BETWEEN 1920 AND 2100"));
        b.ToTable(t => t.HasCheckConstraint("CK_RebanhoNoMunicipio_Rebanho", "[RebanhoCodigoIbge] > 0"));
        b.ToTable(t => t.HasCheckConstraint("CK_RebanhoNoMunicipio_Cabecas", "[Cabecas] IS NULL OR [Cabecas] >= 0"));
    }
}

/// <summary>
/// Mapeamento de <see cref="AreaTerritorialDoMunicipio"/> — a área em km², com os três decimais que o
/// IBGE publica.
///
/// <para><c>decimal(11,3)</c> comporta 99.999.999,999 km²: o maior município do Brasil (Altamira)
/// tem 159.533,255 km², e o Brasil inteiro tem 8,5 milhões. Guardar isso como <c>float</c> faria a
/// soma de 645 municípios variar no terceiro decimal conforme a ordem das parcelas.</para>
/// </summary>
public sealed class AreaTerritorialDoMunicipioConfiguracao : IEntityTypeConfiguration<AreaTerritorialDoMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AreaTerritorialDoMunicipio> b)
    {
        b.ToTable("AreaTerritorialDoMunicipio", "organizacao");
        b.HasKey(a => a.Id);
        b.Property(a => a.Id).ValueGeneratedOnAdd();

        b.Property(a => a.MunicipioId).IsRequired();
        b.Property(a => a.Ano).IsRequired();
        b.Property(a => a.AreaKm2).HasPrecision(11, 3);
        b.Property(a => a.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(a => a.ImportadoPorId).IsRequired();

        b.HasIndex(a => new { a.MunicipioId, a.Ano })
            .IsUnique()
            .HasDatabaseName("UX_AreaTerritorialDoMunicipio_Municipio_Ano");

        b.HasIndex(a => a.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(a => a.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(a => a.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_AreaTerritorialDoMunicipio_Ano", "[Ano] BETWEEN 1920 AND 2100"));

        // ÁREA ZERO NÃO EXISTE, ao contrário de uma contagem. A regra é a mesma do domínio, dita
        // também aqui: o banco é a última linha de defesa de quem grava por fora da aplicação.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_AreaTerritorialDoMunicipio_Area", "[AreaKm2] IS NULL OR [AreaKm2] > 0"));
    }
}

/// <summary>
/// Mapeamento de <see cref="MedidaDoIbgeNoEstado"/> — o total que o IBGE publica para o estado inteiro
/// (issue 155), que não é a soma dos municípios.
///
/// <para>Não tem chave estrangeira para município, de propósito: o recorte é a UF, e o código dela é
/// o que o SIDRA devolve. <c>decimal(18,3)</c> comporta desde a área em km² com três decimais até o
/// efetivo de um rebanho em cabeças.</para>
/// </summary>
public sealed class MedidaDoIbgeNoEstadoConfiguracao : IEntityTypeConfiguration<MedidaDoIbgeNoEstado>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MedidaDoIbgeNoEstado> b)
    {
        b.ToTable("MedidaDoIbgeNoEstado", "organizacao");
        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedOnAdd();

        b.Property(m => m.EstadoCodigoIbge).IsRequired();
        b.Property(m => m.TabelaDoSidra).IsRequired();
        b.Property(m => m.VariavelDoSidra).IsRequired();
        b.Property(m => m.Ano).IsRequired();
        b.Property(m => m.CategoriaNome).HasMaxLength(120).IsUnicode(true);
        b.Property(m => m.Valor).HasPrecision(18, 3);
        b.Property(m => m.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(m => m.ImportadoPorId).IsRequired();

        // A CHAVE NATURAL DO SIDRA, SEM FILTRO. O EF filtra índice único com coluna anulável por
        // padrão ("WHERE [CategoriaCodigoIbge] IS NOT NULL"), e isso deixaria a tabela SEM
        // classificação — a da área territorial — duplicar à vontade, que é justamente a linha que
        // não pode ter duas versões. Sem filtro, o SQL Server trata dois nulos como iguais e garante
        // uma linha por (estado, tabela, variável, ano).
        b.HasIndex(m => new { m.EstadoCodigoIbge, m.TabelaDoSidra, m.VariavelDoSidra, m.Ano, m.CategoriaCodigoIbge })
            .IsUnique()
            .HasFilter(null)
            .HasDatabaseName("UX_MedidaDoIbgeNoEstado_Sidra");

        b.HasIndex(m => m.ImportadoPorId);

        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(m => m.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_MedidaDoIbgeNoEstado_Ano", "[Ano] BETWEEN 1920 AND 2100"));
        b.ToTable(t => t.HasCheckConstraint("CK_MedidaDoIbgeNoEstado_Uf", "[EstadoCodigoIbge] BETWEEN 11 AND 53"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_MedidaDoIbgeNoEstado_Sidra", "[TabelaDoSidra] > 0 AND [VariavelDoSidra] > 0"));

        // Código e rótulo andam juntos — a mesma regra do domínio, dita também no banco.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_MedidaDoIbgeNoEstado_Categoria",
            "([CategoriaCodigoIbge] IS NULL AND [CategoriaNome] IS NULL) " +
            "OR ([CategoriaCodigoIbge] > 0 AND [CategoriaNome] IS NOT NULL)"));

        b.ToTable(t => t.HasCheckConstraint("CK_MedidaDoIbgeNoEstado_Valor", "[Valor] IS NULL OR [Valor] >= 0"));
    }
}

/// <summary>
/// Mapeamento de <see cref="CorrespondenciaDeMunicipio"/> — o de-para entre a chave de cada fonte e o município
/// do catálogo (issue 154). Fica em <c>organizacao</c>, junto do município e das fontes que a usam.
/// </summary>
public sealed class CorrespondenciaDeMunicipioConfiguracao : IEntityTypeConfiguration<CorrespondenciaDeMunicipio>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CorrespondenciaDeMunicipio> b)
    {
        b.ToTable("CorrespondenciaDeMunicipio", "organizacao");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Fonte).HasMaxLength(40).IsUnicode(false).IsRequired();

        // A CHAVE ESCAPA DA COLAÇÃO SEM CAIXA NEM ACENTO: ela já vem normalizada, e "SAO PAULO" e "São Paulo"
        // precisam ser chaves diferentes se a fonte publicar as duas — o par é conferido, não adivinhado.
        b.Property(c => c.ChaveNaFonte).HasMaxLength(CorrespondenciaDeMunicipio.TamanhoDoTexto).IsUnicode(true).IsRequired()
            .UseCollation("Latin1_General_BIN2");

        b.Property(c => c.TextoNaFonte).HasMaxLength(CorrespondenciaDeMunicipio.TamanhoDoTexto).IsUnicode(true).IsRequired();
        b.Property(c => c.MunicipioId).IsRequired();
        b.Property(c => c.Forma).HasConversion<string>().HasMaxLength(30).IsUnicode(false).IsRequired();
        b.Property(c => c.CasadaEm).HasPrecision(3).IsRequired();
        b.Property(c => c.CasadaPorId).IsRequired();

        b.HasIndex(c => new { c.Fonte, c.ChaveNaFonte })
            .IsUnique()
            .HasDatabaseName("UX_CorrespondenciaDeMunicipio_Fonte_Chave");

        b.HasIndex(c => c.MunicipioId);
        b.HasIndex(c => c.CasadaPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(c => c.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(c => c.CasadaPorId).OnDelete(DeleteBehavior.Restrict);

        // O DOMÍNIO FECHADO NO BANCO, e não só no compilador (documento 14, seções 4 e 6): a forma diz de onde
        // veio a confiança do par, e ninguém corrige um par à mão gravando uma forma que o CRM não conhece.
        b.ToTable(t => t.HasCheckConstraint(
            "CK_CorrespondenciaDeMunicipio_Forma", "[Forma] IN ('NomeUnicoNaUf','CodigoDoIbgeNaFonte','Manual')"));
    }
}

/// <summary>
/// Mapeamento de <see cref="UsinaDeEtanol"/> — as usinas autorizadas pela ANP.
///
/// <para>A chave natural é o <b>CNPJ do estabelecimento</b>, e o índice único é sobre ele: a ANP
/// publica uma linha por estabelecimento e mês, e o CRM guarda só o mês mais recente. Sem esse índice
/// a recarga empilharia a mesma usina a cada rodada.</para>
/// </summary>
public sealed class UsinaDeEtanolConfiguracao : IEntityTypeConfiguration<UsinaDeEtanol>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UsinaDeEtanol> b)
    {
        b.ToTable("UsinaDeEtanol", "organizacao");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).ValueGeneratedOnAdd();

        // O CNPJ É COMPARADO DÍGITO A DÍGITO, e por isso escapa da colação sem caixa nem acento do
        // banco: ele não é texto de gente, é identificador.
        b.Property(u => u.Cnpj).HasMaxLength(14).IsUnicode(false).IsRequired()
            .UseCollation("Latin1_General_BIN2");

        b.Property(u => u.RazaoSocial).HasMaxLength(200).IsUnicode(true).IsRequired();
        b.Property(u => u.MunicipioId).IsRequired();
        b.Property(u => u.MesDeReferencia).IsRequired();
        b.Property(u => u.ImportadoEm).HasPrecision(3).IsRequired();
        b.Property(u => u.ImportadoPorId).IsRequired();

        // A usina que sai da lista da ANP fica encerrada, não apagada (issue 153).
        b.Property(u => u.EncerradaEm).HasPrecision(3);

        // A capacidade total é calculada em memória: guardá-la seria uma terceira fonte para a mesma
        // verdade, e o modelo já recusou isso no rendimento médio da PAM.
        b.Ignore(u => u.CapacidadeTotalM3Dia);
        b.Ignore(u => u.EstaVigente);

        b.HasIndex(u => u.Cnpj).IsUnique().HasDatabaseName("UX_UsinaDeEtanol_Cnpj");

        // A consulta do mapa: "as usinas deste município".
        b.HasIndex(u => u.MunicipioId);
        b.HasIndex(u => u.ImportadoPorId);

        b.HasOne<Municipio>().WithMany().HasForeignKey(u => u.MunicipioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Seguranca.Usuario>().WithMany().HasForeignKey(u => u.ImportadoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint("CK_UsinaDeEtanol_Cnpj", "LEN([Cnpj]) = 14"));
        b.ToTable(t => t.HasCheckConstraint(
            "CK_UsinaDeEtanol_Capacidade",
            "([CapacidadeDeAnidroM3Dia] IS NULL OR [CapacidadeDeAnidroM3Dia] >= 0) " +
            "AND ([CapacidadeDeHidratadoM3Dia] IS NULL OR [CapacidadeDeHidratadoM3Dia] >= 0)"));
    }
}
