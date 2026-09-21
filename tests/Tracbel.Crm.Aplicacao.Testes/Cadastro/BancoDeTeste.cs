using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Aplicacao.Catalogos;
using Tracbel.Crm.Aplicacao.Clientes;
using Tracbel.Crm.Aplicacao.Equipamentos;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

namespace Tracbel.Crm.Aplicacao.Testes.Cadastro;

/// <summary>
/// O banco de teste dos casos de uso: SQLite em memória, com os REPOSITÓRIOS DE VERDADE.
///
/// <para><b>Por que não dublê de repositório:</b> porque o que mais importa provar aqui é
/// justamente o que um dublê apagaria. A fronteira de empresa não está escrita no repositório —
/// está no filtro global do <see cref="CrmDbContext"/>, e só existe quando a consulta vira SQL
/// de verdade. Um <c>IRepositorioClientes</c> falso devolveria a lista que o teste mandasse, e o
/// teste passaria mesmo com a fronteira desligada. É o mesmo raciocínio já registrado em
/// <c>FronteiraDeEmpresaTestes</c>: filtro que não traduz é filtro que não existe.</para>
///
/// <para><b>O que este banco NÃO cobre, e por quê:</b> a concorrência otimista. O
/// <c>rowversion</c> é do SQL Server e sai do modelo no SQLite
/// (<c>CrmDbContext.ConfigurarConcorrenciaOtimista</c>), então <c>VersaoConfere</c> sempre aceita
/// aqui. A regra em si é testada sem banco em <c>Tracbel.Crm.Dominio.Testes</c>, e o
/// comportamento ponta a ponta foi exercitado contra o SQL Server real (documento 23).</para>
/// </summary>
public sealed class BancoDeTeste : IDisposable
{
    /// <summary>A filial de Ribeirão Preto — onde quase tudo deste teste acontece.</summary>
    public const int RibeiraoPreto = 1;

    /// <summary>A filial de Barretos — a "outra filial", que serve de prova da fronteira.</summary>
    public const int Barretos = 2;

    /// <summary>O identificador do usuário de Ribeirão Preto.</summary>
    public const long UsuarioDeRibeirao = 100;

    /// <summary>O identificador do usuário de Barretos.</summary>
    public const long UsuarioDeBarretos = 200;

    /// <summary>O código do modelo semeado, para o cadastro de equipamento.</summary>
    public const string ModeloSemeado = "8R_340";

    private readonly SqliteConnection _conexao;
    private readonly ProvedorMutavel _acesso = new();

    /// <summary>Monta o banco, cria o esquema e semeia o mínimo de referência.</summary>
    public BancoDeTeste()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();

        _acesso.Definir(ContextoDe(UsuarioDeRibeirao, RibeiraoPreto, ehSistema: true));

        using var criacao = NovoContexto();
        criacao.Database.EnsureCreated();
        Semear(criacao);

        _acesso.Definir(ContextoDe(UsuarioDeRibeirao, RibeiraoPreto));
    }

    /// <summary>O relógio, fixo, para a procedência ser conferível.</summary>
    public IRelogio Relogio { get; } = new RelogioFixo(new DateTime(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc));

    /// <summary>Troca quem está agindo — é o que permite provar a fronteira entre filiais.</summary>
    /// <param name="usuarioId">Quem passa a agir.</param>
    /// <param name="empresaId">Em qual filial.</param>
    public void AgirComo(long usuarioId, int empresaId) => _acesso.Definir(ContextoDe(usuarioId, empresaId));

    /// <summary>
    /// Age em "Todas as filiais" — o que o escopo monta para o administrador que escolhe
    /// <see cref="ContextoAcesso.CodigoDeTodasAsFiliais"/>: as duas filiais no alcance, e a de casa só porque
    /// o contexto precisa de uma.
    /// </summary>
    /// <param name="usuarioId">Quem passa a agir.</param>
    /// <param name="empresaDeCasa">A filial de casa dele.</param>
    public void AgirEmTodasAsFiliais(long usuarioId, int empresaDeCasa) =>
        _acesso.Definir(new ContextoAcesso(
            usuarioId,
            nomeExibicao: $"usuário {usuarioId}",
            empresaId: empresaDeCasa,
            empresasVisiveis: new HashSet<int> { RibeiraoPreto, Barretos },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            profundidades: Permissoes.Catalogo.Keys.ToDictionary(p => p, _ => Profundidade.Organizacao),
            todasAsFiliais: true));

    /// <summary>Um contexto de persistência novo, já com o escopo de acesso atual.</summary>
    public CrmDbContext NovoContexto() =>
        new(new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options, _acesso);

    /// <summary>Monta os casos de uso de cliente sobre um contexto novo.</summary>
    public (CriarCliente Criar, AlterarCliente Alterar, InativarCliente Inativar,
            ListarClientes Listar, ObterCliente Obter, CrmDbContext Contexto) CasosDeCliente()
    {
        var contexto = NovoContexto();
        var clientes = new RepositorioDeClientes(contexto);
        var catalogos = new RepositorioDeCatalogos(contexto);
        var unidade = new UnidadeDeTrabalho(contexto);

        return (
            new CriarCliente(clientes, catalogos, unidade, _acesso),
            new AlterarCliente(clientes, catalogos, unidade, _acesso),
            new InativarCliente(clientes, catalogos, unidade, _acesso),
            new ListarClientes(clientes, Relogio),
            new ObterCliente(clientes, Relogio),
            contexto);
    }

    /// <summary>Monta os casos de uso de equipamento sobre um contexto novo.</summary>
    public (CriarEquipamento Criar, AlterarEquipamento Alterar, InativarEquipamento Inativar,
            ListarEquipamentos Listar, CrmDbContext Contexto) CasosDeEquipamento()
    {
        var contexto = NovoContexto();
        var clientes = new RepositorioDeClientes(contexto);
        var equipamentos = new RepositorioDeEquipamentos(contexto);
        var catalogos = new RepositorioDeCatalogos(contexto);
        var unidade = new UnidadeDeTrabalho(contexto);

        return (
            new CriarEquipamento(equipamentos, clientes, catalogos, unidade, _acesso),
            new AlterarEquipamento(equipamentos, clientes, catalogos, unidade, _acesso),
            new InativarEquipamento(equipamentos, unidade, _acesso),
            new ListarEquipamentos(equipamentos, clientes, catalogos, Relogio),
            contexto);
    }

    /// <summary>Monta o caso de uso de catálogos sobre um contexto novo.</summary>
    public (ListarCatalogos Caso, CrmDbContext Contexto) CasoDeCatalogos()
    {
        var contexto = NovoContexto();
        return (new ListarCatalogos(new RepositorioDeCatalogos(contexto), Relogio), contexto);
    }

    /// <inheritdoc />
    public void Dispose() => _conexao.Dispose();

    private static ContextoAcesso ContextoDe(long usuarioId, int empresaId, bool ehSistema = false) =>
        new(usuarioId,
            nomeExibicao: $"usuário {usuarioId}",
            empresaId: empresaId,
            empresasVisiveis: new HashSet<int> { empresaId },
            subordinadosIds: new HashSet<long>(),
            equipesIds: new HashSet<long>(),
            // O OPERADOR DE CADASTRO: os testes daqui exercitam criar, alterar e inativar, e desde a fase 3 o
            // caso de uso confere a permissão de cada uma. A recusa sem ela é provada em PermissaoNoCasoDeUsoTestes.
            profundidades: new[]
                {
                    Permissoes.ClienteLer, Permissoes.ClienteCriar, Permissoes.ClienteEditar, Permissoes.ClienteExcluir,
                    Permissoes.EquipamentoLer, Permissoes.EquipamentoCriar, Permissoes.EquipamentoEditar, Permissoes.EquipamentoExcluir
                }
                .ToDictionary(p => p, _ => Profundidade.EmpresaEAbaixo),
            ehServicoDeSistema: ehSistema);

    private static void Semear(CrmDbContext db)
    {
        foreach (var (id, codigo, nome) in new[]
                 {
                     (RibeiraoPreto, "010101", "Tracbel Agro — Ribeirão Preto"),
                     (Barretos, "010103", "Tracbel Agro — Barretos")
                 })
        {
            var empresa = Empresa.Criar(codigo, nome);
            db.Entry(empresa).Property(e => e.Id).CurrentValue = id;
            db.Empresas.Add(empresa);
        }

        foreach (var (id, apelido, empresaId) in new[]
                 {
                     (UsuarioDeRibeirao, "ricardo", RibeiraoPreto),
                     (UsuarioDeBarretos, "barretos", Barretos)
                 })
        {
            var usuario = Usuario.Criar(
                identidadeExterna: Guid.NewGuid(),
                nomePrincipal: $"{apelido}@tracbel.com.br",
                nomeCompleto: apelido,
                nomeExibicao: apelido,
                email: Email.Criar($"{apelido}@tracbel.com.br"),
                empresaId: empresaId,
                criadoPorId: 1);

            db.Entry(usuario).Property(u => u.Id).CurrentValue = id;
            db.Usuarios.Add(usuario);
        }

        // Os oito catálogos de sistema já vêm do HasData do modelo. O que falta são os ITENS,
        // e são eles que a regra "campo com catálogo não aceita texto livre" precisa ter para
        // poder aceitar alguma coisa.
        db.CatalogoItens.AddRange(
            CatalogoItem.Criar(CatalogosDeSistema.OrigemDeLead, "INDICACAO", "Indicação", 10),
            CatalogoItem.Criar(CatalogosDeSistema.OrigemDeLead, "SITE", "Site", 20),
            CatalogoItem.Criar(CatalogosDeSistema.MotivoDeInativacao, "DUPLICADO", "Cadastro duplicado", 10),
            CatalogoItem.Criar(CatalogosDeSistema.Cultura, "SOJA", "Soja", 10));

        var marca = Marca.Criar("JOHN_DEERE", "John Deere", ehRepresentada: true);
        db.Marcas.Add(marca);
        db.SaveChanges();

        var familia = Familia.Criar(marca.Id, "A_CONFIRMAR", "A confirmar");
        db.Familias.Add(familia);
        db.SaveChanges();

        db.Modelos.Add(Modelo.Criar(familia.Id, ModeloSemeado, "8R 340"));
        db.SaveChanges();
    }

    /// <summary>O provedor de contexto que o teste consegue trocar no meio do caminho.</summary>
    private sealed class ProvedorMutavel : IProvedorContextoAcesso
    {
        private ContextoAcesso? _atual;

        public ContextoAcesso Atual => _atual!;

        public void Definir(ContextoAcesso contexto) => _atual = contexto;
    }

    private sealed class RelogioFixo(DateTime agora) : IRelogio
    {
        public DateTime Agora { get; } = agora;
    }
}
