using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Configuracoes;

/// <summary>Mapeamento de <see cref="Catalogo"/> — schema <c>metadado</c>.</summary>
public sealed class CatalogoConfiguracao : IEntityTypeConfiguration<Catalogo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Catalogo> b)
    {
        b.ToTable("Catalogo", "metadado");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(c => c.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(c => c.PermiteItemNovo).IsRequired();
        b.Property(c => c.EstaAtivo).IsRequired();

        b.HasIndex(c => c.Codigo).IsUnique().HasDatabaseName("UX_Catalogo_Codigo");

        // OS CATÁLOGOS QUE O ESQUEMA REFERENCIA, com identificador fixo — ver
        // Dominio.Metadado.CatalogosDeSistema para o porquê de o Id ser constante aqui e
        // gerado pelo banco em todo o resto (documento 14, regra 8.3).
        //
        // Sem estas oito linhas, as chaves estrangeiras compostas de papel apontariam para um
        // catálogo que não existe, e nenhuma delas poderia ser satisfeita.
        b.HasData(CatalogosDeSistema.Todos.Select(c => new
        {
            c.Id,
            c.Codigo,
            c.Nome,
            Descricao = (string?)c.Descricao,
            PermiteItemNovo = true,
            EstaAtivo = true
        }));
    }
}

/// <summary>
/// Mapeamento de <see cref="CatalogoItem"/>.
///
/// A DESCRIÇÃO USA COLLATION NÃO DETERMINÍSTICA, e é aí que mora a garantia central deste
/// schema: com unicidade por catálogo sobre essa coluna, "Preço" e "PRECO" passam a ser o
/// mesmo item — o banco recusa o segundo. [V] é exatamente o defeito que fez FINALIZADO
/// (18.416 linhas) conviver com FINALIZADA (11.563) no Vórtice, que tem zero restrições de
/// verificação em 767 tabelas.
/// </summary>
public sealed class CatalogoItemConfiguracao : IEntityTypeConfiguration<CatalogoItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CatalogoItem> b)
    {
        b.ToTable("CatalogoItem", "metadado");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).ValueGeneratedOnAdd();

        b.Property(i => i.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();

        b.Property(i => i.Descricao)
            .HasMaxLength(200)
            .IsUnicode(true)
            .UseCollation(CrmDbContext.ColacaoSemCaixaNemAcento)
            .IsRequired();

        b.Property(i => i.Ordem).IsRequired();
        b.Property(i => i.ExigeObservacao).IsRequired();
        b.Property(i => i.EstaAtivo).IsRequired();
        b.Property(i => i.UltimoUsoEm).HasPrecision(3);

        b.HasIndex(i => new { i.CatalogoId, i.Codigo })
            .IsUnique()
            .HasDatabaseName("UX_CatalogoItem_Catalogo_Codigo");

        // A unicidade que fecha o domínio de verdade: dois rótulos que só diferem em caixa ou
        // acento são o MESMO rótulo, e o segundo não entra.
        b.HasIndex(i => new { i.CatalogoId, i.Descricao })
            .IsUnique()
            .HasDatabaseName("UX_CatalogoItem_Catalogo_Descricao");

        // Chave candidata que permite, mais adiante, a chave estrangeira COMPOSTA descrita na
        // seção 8.11 do documento 17 — a que amarra uma coluna a um catálogo específico.
        b.HasAlternateKey(i => new { i.CatalogoId, i.Id })
            .HasName("AK_CatalogoItem_CatalogoId");

        b.HasIndex(i => new { i.CatalogoId, i.Ordem }).HasFilter("[EstaAtivo] = 1");
        b.HasIndex(i => new { i.CatalogoId, i.ItemPaiId });

        b.HasOne<Catalogo>().WithMany().HasForeignKey(i => i.CatalogoId).OnDelete(DeleteBehavior.Restrict);

        // O PAI É DO MESMO CATÁLOGO. A chave estrangeira composta contra a mesma chave
        // alternativa usada pelas colunas de papel — de graça, pelo mesmo mecanismo: sem ela,
        // um item de CULTURA podia ter como pai um item de TIPO_DOCUMENTO.
        b.HasOne<CatalogoItem>()
            .WithMany()
            .HasForeignKey(i => new { i.CatalogoId, i.ItemPaiId })
            .HasPrincipalKey(i => new { i.CatalogoId, i.Id })
            .OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_CatalogoItem_Ordem", "[Ordem] >= 0"));
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CatalogoItem_Pai", "[ItemPaiId] IS NULL OR [ItemPaiId] <> [Id]"));
    }
}

/// <summary>Mapeamento de <see cref="CampoPersonalizado"/>.</summary>
public sealed class CampoPersonalizadoConfiguracao : IEntityTypeConfiguration<CampoPersonalizado>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CampoPersonalizado> b)
    {
        b.ToTable("CampoPersonalizado", "metadado");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).ValueGeneratedOnAdd();

        b.Property(c => c.Entidade).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(c => c.Campo).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(c => c.Rotulo).HasMaxLength(80).IsUnicode(true).IsRequired();
        b.Property(c => c.TipoDeCampo).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(c => c.Grupo).HasMaxLength(60).IsUnicode(true);
        b.Property(c => c.CondicaoVisibilidade).HasMaxLength(1000).IsUnicode(true);
        b.Property(c => c.Ordem).IsRequired();
        b.Property(c => c.EhPersonalizado).IsRequired();
        b.Property(c => c.EhObrigatorio).IsRequired();
        b.Property(c => c.NoResumo).IsRequired();
        b.Property(c => c.EstaAtivo).IsRequired();

        // As regras de validação variam por tipo de campo: faixa numérica, expressão, lista.
        // JSON em vez de dez colunas que só valem para um tipo cada — [V] o oposto das 41
        // colunas genéricas numa tabela de 51 do Vórtice.
        b.Property(c => c.Validacao).HasColumnType("nvarchar(max)");
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CampoPersonalizado_ValidacaoJson", "[Validacao] IS NULL OR ISJSON([Validacao]) = 1"));

        b.HasIndex(c => new { c.Entidade, c.Campo })
            .IsUnique()
            .HasDatabaseName("UX_CampoPersonalizado_Entidade_Campo");

        b.HasIndex(c => c.CatalogoId);

        b.HasOne<Catalogo>().WithMany().HasForeignKey(c => c.CatalogoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_CampoPersonalizado_Tipo",
            "[TipoDeCampo] IN ('Texto','Numero','Data','Booleano','Lista','Referencia')"));

        // Campo de lista sem catálogo é texto livre disfarçado — a regra da seção 6 do doc 17.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_CampoPersonalizado_Lista", "[TipoDeCampo] <> 'Lista' OR [CatalogoId] IS NOT NULL"));
    }
}

/// <summary>Mapeamento de <see cref="TratadorDeEvento"/>.</summary>
public sealed class TratadorDeEventoConfiguracao : IEntityTypeConfiguration<TratadorDeEvento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TratadorDeEvento> b)
    {
        b.ToTable("TratadorDeEvento", "metadado");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).ValueGeneratedOnAdd();

        b.Property(t => t.Evento).HasMaxLength(60).IsUnicode(false).IsRequired();
        b.Property(t => t.TipoImplementacao).HasMaxLength(200).IsUnicode(false).IsRequired();
        b.Property(t => t.Momento).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(t => t.CamposFiltro).HasMaxLength(400).IsUnicode(false);
        b.Property(t => t.Ordem).IsRequired();
        b.Property(t => t.EhAssincrono).IsRequired();
        b.Property(t => t.EstaAtivo).IsRequired();

        b.HasIndex(t => new { t.Evento, t.TipoImplementacao, t.Momento })
            .IsUnique()
            .HasDatabaseName("UX_TratadorDeEvento_Evento_Tipo_Momento");

        b.HasIndex(t => new { t.Evento, t.Momento, t.Ordem }).HasFilter("[EstaAtivo] = 1");

        b.ToTable(x => x.HasCheckConstraint(
            "CK_TratadorDeEvento_Momento", "[Momento] IN ('PreValidacao','PreOperacao','PosOperacao')"));

        // [DYN] assíncrono SÓ existe depois da operação — antes dela, rodar fora da transação
        // não faz sentido nenhum.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_TratadorDeEvento_Assincrono", "[EhAssincrono] = 0 OR [Momento] = 'PosOperacao'"));
    }
}

/// <summary>Mapeamento de <see cref="Formulario"/>.</summary>
public sealed class FormularioConfiguracao : IEntityTypeConfiguration<Formulario>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Formulario> b)
    {
        b.ToTable("Formulario", "metadado");
        b.HasKey(f => f.Id);
        b.Property(f => f.Id).ValueGeneratedOnAdd();

        b.Property(f => f.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(f => f.Nome).HasMaxLength(120).IsUnicode(true).IsRequired();
        b.Property(f => f.Descricao).HasMaxLength(400).IsUnicode(true);
        b.Property(f => f.VersaoPublicada).IsRequired();
        b.Property(f => f.EstaAtivo).IsRequired();

        b.HasIndex(f => f.Codigo).IsUnique().HasDatabaseName("UX_Formulario_Codigo");
        b.HasIndex(f => f.TipoProcessoId);

        b.HasOne<TipoProcesso>().WithMany().HasForeignKey(f => f.TipoProcessoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint("CK_Formulario_Versao", "[VersaoPublicada] >= 1"));
    }
}

/// <summary>Mapeamento de <see cref="Pergunta"/>.</summary>
public sealed class PerguntaConfiguracao : IEntityTypeConfiguration<Pergunta>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Pergunta> b)
    {
        b.ToTable("Pergunta", "metadado");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.Codigo).HasMaxLength(40).IsUnicode(false).IsRequired();
        b.Property(p => p.Texto).HasMaxLength(1000).IsUnicode(true).IsRequired();
        b.Property(p => p.TipoDeResposta).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(p => p.ValorCondicao).HasMaxLength(200).IsUnicode(true);
        b.Property(p => p.Ordem).IsRequired();
        b.Property(p => p.EhObrigatoria).IsRequired();
        b.Property(p => p.EstaAtiva).IsRequired();

        b.HasIndex(p => new { p.FormularioId, p.Codigo })
            .IsUnique()
            .HasDatabaseName("UX_Pergunta_Formulario_Codigo");

        b.HasIndex(p => new { p.FormularioId, p.Ordem });
        b.HasIndex(p => p.CatalogoId);
        b.HasIndex(p => p.PerguntaCondicaoId);

        b.HasOne<Formulario>().WithMany().HasForeignKey(p => p.FormularioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Catalogo>().WithMany().HasForeignKey(p => p.CatalogoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Pergunta>().WithMany().HasForeignKey(p => p.PerguntaCondicaoId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Pergunta_TipoDeResposta",
            "[TipoDeResposta] IN ('Texto','Numero','Data','Booleano','Catalogo','Cliente','Equipamento'," +
            "'CatalogoMultiplo')"));

        // Pergunta de catálogo diz de qual catálogo. Sem isso, a opção vira texto livre.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Pergunta_Catalogo",
            "[TipoDeResposta] NOT IN ('Catalogo','CatalogoMultiplo') OR [CatalogoId] IS NOT NULL"));

        // Pergunta não depende de si mesma.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Pergunta_Condicao", "[PerguntaCondicaoId] IS NULL OR [PerguntaCondicaoId] <> [Id]"));
    }
}

/// <summary>Mapeamento de <see cref="Preenchimento"/>.</summary>
public sealed class PreenchimentoConfiguracao : IEntityTypeConfiguration<Preenchimento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Preenchimento> b)
    {
        b.ToTable("Preenchimento", "metadado");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedOnAdd();

        b.Property(p => p.VersaoFormulario).IsRequired();
        b.Property(p => p.Situacao).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(p => p.PreenchidoEm).HasPrecision(3).IsRequired();

        b.HasIndex(p => new { p.FormularioId, p.PreenchidoEm }).IsDescending(false, true);
        b.HasIndex(p => p.ClienteId);
        b.HasIndex(p => p.ProcessoId);
        b.HasIndex(p => p.TarefaId);
        b.HasIndex(p => p.InteracaoId);
        b.HasIndex(p => p.PreenchidoPorId);
        b.HasIndex(p => p.EmpresaId);

        b.HasOne<Empresa>().WithMany().HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Formulario>().WithMany().HasForeignKey(p => p.FormularioId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Dominio.Processo.Processo>().WithMany().HasForeignKey(p => p.ProcessoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tarefa>().WithMany().HasForeignKey(p => p.TarefaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Interacao>().WithMany().HasForeignKey(p => p.InteracaoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Usuario>().WithMany().HasForeignKey(p => p.PreenchidoPorId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(x => x.HasCheckConstraint(
            "CK_Preenchimento_Situacao", "[Situacao] IN ('EmAndamento','Concluido','Cancelado')"));
        b.ToTable(x => x.HasCheckConstraint("CK_Preenchimento_Versao", "[VersaoFormulario] >= 1"));

        // Preenchimento se liga a alguma coisa: cliente, processo, tarefa ou interação.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Preenchimento_TemVinculo",
            "[ClienteId] IS NOT NULL OR [ProcessoId] IS NOT NULL " +
            "OR [TarefaId] IS NOT NULL OR [InteracaoId] IS NOT NULL"));
    }
}

/// <summary>
/// Mapeamento de <see cref="Resposta"/>.
///
/// É esta tabela que substitui as 175 tabelas físicas de formulário do Vórtice. A resposta é
/// TIPADA: uma coluna por tipo, e a restrição de verificação garante que exatamente uma
/// delas está preenchida. Sem isso, seria um campo de texto para tudo — o modelo que devolve
/// data como texto e não ordena.
/// </summary>
public sealed class RespostaConfiguracao : IEntityTypeConfiguration<Resposta>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Resposta> b)
    {
        b.ToTable("Resposta", "metadado");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).ValueGeneratedOnAdd();

        b.Property(r => r.ValorTexto).HasMaxLength(4000).IsUnicode(true);
        b.Property(r => r.ValorNumero).HasPrecision(18, 6);
        b.Property(r => r.ValorData).HasPrecision(3);
        b.Property(r => r.CriadoEm).HasPrecision(3).IsRequired();

        // O que não cabe numa coluna escalar: escolha múltipla, matriz, anexo com metadado.
        // É a alternativa a criar uma tabela filha por formato de resposta — o caminho que
        // produziu as 175 tabelas do legado.
        b.Property(r => r.ValorEstruturado).HasColumnType("nvarchar(max)");
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Resposta_ValorEstruturadoJson",
            "[ValorEstruturado] IS NULL OR ISJSON([ValorEstruturado]) = 1"));

        b.HasIndex(r => r.PreenchimentoId);

        // Os três índices que sustentam a pergunta transversal ("quantas vendas perdemos por
        // preço em 2026?"), cada um parcial sobre a coluna que aquele tipo de pergunta usa.
        b.HasIndex(r => new { r.PerguntaId, r.CatalogoItemId }).HasFilter("[CatalogoItemId] IS NOT NULL");
        b.HasIndex(r => new { r.PerguntaId, r.ValorNumero }).HasFilter("[ValorNumero] IS NOT NULL");
        b.HasIndex(r => new { r.PerguntaId, r.ValorTexto }).HasFilter("[ValorTexto] IS NOT NULL");

        b.HasIndex(r => r.ClienteId);
        b.HasIndex(r => r.EquipamentoId);

        b.HasIndex(r => new { r.PreenchimentoId, r.PerguntaId })
            .IsUnique()
            .HasDatabaseName("UX_Resposta_Preenchimento_Pergunta");

        b.HasOne<Preenchimento>().WithMany().HasForeignKey(r => r.PreenchimentoId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Pergunta>().WithMany().HasForeignKey(r => r.PerguntaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<CatalogoItem>().WithMany().HasForeignKey(r => r.CatalogoItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Cliente>().WithMany().HasForeignKey(r => r.ClienteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Equipamento>().WithMany().HasForeignKey(r => r.EquipamentoId).OnDelete(DeleteBehavior.Restrict);

        // EXATAMENTE UMA coluna de valor preenchida. É o que impede a resposta ambígua e o
        // que faz a consulta transversal ser confiável.
        b.ToTable(x => x.HasCheckConstraint(
            "CK_Resposta_UmValor",
            "(CASE WHEN [ValorTexto]        IS NOT NULL THEN 1 ELSE 0 END + " +
            " CASE WHEN [ValorNumero]       IS NOT NULL THEN 1 ELSE 0 END + " +
            " CASE WHEN [ValorData]         IS NOT NULL THEN 1 ELSE 0 END + " +
            " CASE WHEN [ValorBooleano]     IS NOT NULL THEN 1 ELSE 0 END + " +
            " CASE WHEN [CatalogoItemId]   IS NOT NULL THEN 1 ELSE 0 END + " +
            " CASE WHEN [ClienteId]         IS NOT NULL THEN 1 ELSE 0 END + " +
            " CASE WHEN [EquipamentoId]     IS NOT NULL THEN 1 ELSE 0 END + " +
            " CASE WHEN [ValorEstruturado]  IS NOT NULL THEN 1 ELSE 0 END) = 1"));
    }
}
