using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Aplicacao.Testes.Carga;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Persistencia;

/// <summary>
/// A TRILHA NA CONCORRÊNCIA, NO SQL SERVER DE VERDADE (issue 40, tarefa 3) — duas pessoas alteram o MESMO cliente
/// auditado: a primeira grava, a segunda recebe o aviso de concorrência e NENHUMA linha de trilha dela fica.
///
/// <para><b>Por que só no motor de verdade:</b> o <c>rowversion</c> é do SQL Server e sai do modelo no SQLite, e a trilha
/// é gravada na MESMA transação da alteração (fase 2). O que se prova é o par: a colisão desfaz a transação inteira, e
/// uma trilha que dissesse "a segunda pessoa mudou a situação" seria mentira gravada.</para>
/// </summary>
[Trait("Categoria", "BancoReal")]
public sealed class TrilhaNaConcorrenciaNoConteinerTestes
{
    /// <summary>Banco PRÓPRIO deste teste, apagado e recriado a cada execução — nunca o de desenvolvimento.</summary>
    private const string NomeDoBancoDeTeste = "TracbelCrmTrilhaNaConcorrenciaTeste";

    private static DbContextOptions<CrmDbContext> Opcoes() => new DbContextOptionsBuilder<CrmDbContext>()
        .UseSqlServer(
            SqlServerDoConteiner.MontarPara(NomeDoBancoDeTeste),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "metadado").CommandTimeout(180))
        .Options;

    [FatoSeHouverSqlServer]
    public async Task A_gravacao_recusada_pela_concorrencia_nao_deixa_linha_na_trilha()
    {
        var (filial, primeiraPessoa, segundaPessoa, clienteId) = await SemearAsync();

        await using var daPrimeira = new CrmDbContext(Opcoes(), Acesso(primeiraPessoa, filial));
        await using var daSegunda = new CrmDbContext(Opcoes(), Acesso(segundaPessoa, filial));

        // AS DUAS LEEM A MESMA VERSÃO da linha, cada uma no seu contexto.
        var vistoPelaPrimeira = await daPrimeira.Clientes.SingleAsync(c => c.Id == clienteId);
        var vistoPelaSegunda = await daSegunda.Clientes.SingleAsync(c => c.Id == clienteId);

        vistoPelaPrimeira.MudarSituacao(SituacaoDoCliente.Cliente, primeiraPessoa);
        var primeira = await new UnidadeDeTrabalho(daPrimeira).SalvarAsync(CancellationToken.None);
        primeira.EhSucesso.Should().BeTrue(primeira.Erro);

        vistoPelaSegunda.MudarSituacao(SituacaoDoCliente.ClienteInativo, segundaPessoa);
        var segunda = await new UnidadeDeTrabalho(daSegunda).SalvarAsync(CancellationToken.None);

        segunda.EhSucesso.Should().BeFalse("a segunda gravou em cima de uma versão que já não existia");
        segunda.Tipo.Should().Be(TipoDeFalha.Concorrencia, "é o aviso de concorrência, e não outro erro qualquer");

        await using var leitura = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        var trilha = await leitura.AlteracoesDeCampo
            .Where(a => a.Entidade == nameof(Cliente) && a.RegistroId == clienteId && a.Campo == "Situacao")
            .OrderBy(a => a.Id)
            .ToListAsync();

        // A INCLUSÃO E A PRIMEIRA ALTERAÇÃO — e nada da segunda pessoa.
        trilha.Should().NotContain(a => a.AlteradoPorId == segundaPessoa, "a transação recusada leva a trilha junto");
        trilha.Should().Contain(a => a.AlteradoPorId == primeiraPessoa && a.Operacao == OperacaoAuditada.Alteracao
                                     && a.ValorNovo == nameof(SituacaoDoCliente.Cliente));
        (await leitura.Clientes.SingleAsync(c => c.Id == clienteId)).Situacao.Should().Be(SituacaoDoCliente.Cliente,
            "vale a primeira gravação, e só ela");
    }

    [FatoSeHouverSqlServer]
    public async Task A_retencao_decidida_esta_no_catalogo_do_banco()
    {
        // D-10 (20/09/2026): 18 meses. Quem abre a tabela pelo SSMS lê a decisão, com a data e o jeito de expurgar.
        await SemearAsync();
        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);

        var descricao = await db.Database.SqlQueryRaw<string>(
                "SELECT CAST(value AS nvarchar(4000)) AS Value FROM sys.fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'auditoria', N'TABLE', N'AlteracaoDeCampo', NULL, NULL)")
            .SingleAsync();

        descricao.Should().Contain("DECIDIDA (D-10, 20/09/2026): 18 meses").And.Contain("nunca por DELETE");
    }

    private static async Task<(int Filial, long Primeira, long Segunda, long ClienteId)> SemearAsync()
    {
        SqlConnection.ClearAllPools();
        await using var db = new CrmDbContext(Opcoes(), ProvedorDeContextoDeSistema.Instancia);
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        var filial = Empresa.Criar("010101", "Tracbel Agro — Ribeirão Preto");
        db.Empresas.Add(filial);
        await db.SaveChangesAsync();

        var ana = Usuario.Criar(Guid.NewGuid(), "ana.trilha@tracbel.com.br", "Ana Trilha", "Ana",
            Email.Criar("ana.trilha@tracbel.com.br"), filial.Id, criadoPorId: 1);
        var bruno = Usuario.Criar(Guid.NewGuid(), "bruno.trilha@tracbel.com.br", "Bruno Trilha", "Bruno",
            Email.Criar("bruno.trilha@tracbel.com.br"), filial.Id, criadoPorId: 1);
        db.Usuarios.AddRange(ana, bruno);
        await db.SaveChangesAsync();

        // Documento fictício, com dígito verificador válido.
        var cliente = Cliente.Criar(filial.Id, "CLIENTE DA TRILHA FICTICIO LTDA", TipoDePessoa.Juridica, ana.Id, ana.Id,
            documento: CpfCnpj.Criar("11222333000181"), situacao: SituacaoDoCliente.Prospect);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        return (filial.Id, ana.Id, bruno.Id, cliente.Id);
    }

    private static ProvedorFixo Acesso(long usuario, int filial) => new(new ContextoAcesso(
        usuario,
        nomeExibicao: $"usuário {usuario}",
        empresaId: filial,
        empresasVisiveis: new HashSet<int> { filial },
        subordinadosIds: new HashSet<long>(),
        equipesIds: new HashSet<long>(),
        profundidades: Permissoes.Catalogo.Keys.ToDictionary(p => p, _ => Profundidade.EmpresaEAbaixo)));

    private sealed class ProvedorFixo(ContextoAcesso atual) : IProvedorContextoAcesso
    {
        public ContextoAcesso Atual { get; } = atual;
    }
}
