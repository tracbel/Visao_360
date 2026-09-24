using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Multiempresa;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDeCarteirasDoVortice;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A SINCRONIA DAS CARTEIRAS MAQ_NOVOS DO VÓRTICE (decisão de 24/09/2026). O que estes testes prendem:
/// <list type="bullet">
/// <item>a primeira rodada cria a linha com a cadência do Vórtice, as carteiras, as contas dos donos AGUARDANDO
/// LIBERAÇÃO e os vínculos que casam pelo documento — e deixa na trilha, com o motivo, os que não casam;</item>
/// <item>o pool do INT.MERCADO, a gaveta "sem potencial" e o digital se distinguem pela regra da carga antiga;</item>
/// <item>é sincronia: rodar de novo sem mudança não grava vínculo; o que sumiu é encerrado; quem mudou de carteira sai
/// de uma e entra na outra; nome e dono da carteira acompanham a origem;</item>
/// <item>a conta do dono é a que o Entra ID entrega à pessoa quando ela entra — depois de liberada;</item>
/// <item>a simulação não grava nada.</item>
/// </list>
/// <para>SQLite em memória com o modelo de verdade, como as outras cargas.</para>
/// </summary>
public sealed class CargaDeCarteirasDoVorticeTestes : IDisposable
{
    private static readonly DateTime Agora = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);

    private readonly SqliteConnection _conexao = new("Filename=:memory:");
    private readonly DbContextOptions<CrmDbContext> _opcoes;
    private readonly Semente _semente;

    public CargaDeCarteirasDoVorticeTestes()
    {
        _conexao.Open();
        _conexao.CreateCollation("Latin1_General_BIN2", (a, b) => string.CompareOrdinal(a, b));
        _opcoes = new DbContextOptionsBuilder<CrmDbContext>().UseSqlite(_conexao).Options;

        using var db = Sistema();
        db.Database.EnsureCreated();
        _semente = Semear(db, forcarIdentificadores: true);
    }

    public void Dispose() => _conexao.Dispose();

    private CrmDbContext Sistema() => new(_opcoes, ProvedorDeContextoDeSistema.Instancia);

    private CrmDbContext DaCarga() => new(_opcoes, new ContextoDeCargaDeSistema(_semente.Operador, _semente.RibeiraoPreto, new HashSet<int> { _semente.RibeiraoPreto, _semente.Barretos }));

    private async Task<RelatorioDaSincroniaDeCarteiras> Sincronizar(LeituraDasCarteirasDoVortice? leitura = null, bool simular = false, DateTime? quando = null)
    {
        var resultado = await Sincronia(DaCarga, leitura ?? Leitura(), quando ?? Agora, _semente).ExecutarAsync(simular, CancellationToken.None);
        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        return resultado.Valor;
    }

    private static string Principal(string login) => login + Usuario.SufixoSemEmail;

    // =============================================================================================
    // A primeira rodada
    // =============================================================================================

    [Fact]
    public async Task A_primeira_rodada_cria_a_linha_as_carteiras_os_donos_e_os_vinculos_que_casam()
    {
        var relatorio = await Sincronizar();

        await using var db = Sistema();

        var linha = await db.LinhasDeNegocio.SingleAsync();
        linha.Codigo.Should().Be("MAQ_NOVOS", "no Vórtice é MAQ-NOVOS, com hífen; no CRM, sublinhado");
        (linha.DiasCicloClasseA, linha.DiasCicloClasseB, linha.DiasCicloClasseC, linha.DiasCicloClasseD)
            .Should().Be(((short?)180, (short?)180, (short?)180, (short?)360), "a cadência vem de IVS_Depto, não do vínculo");

        var carteiras = await db.Carteiras.AsNoTracking().ToDictionaryAsync(c => c.Codigo);
        carteiras.Keys.Should().BeEquivalentTo(
            ["MAQ_01RIB_02_18", "MAQ_03BAR_02_19", "TBA_FILIAIS_708", "TBA_S_POTENCIAL_717", "DGT_01RIB_752"],
            "a carteira de filial fora do CRM, a de vendedor desligado e a de teste não entram");
        carteiras.Values.Should().OnlyContain(c => c.LinhaDeNegocioId == linha.Id);
        carteiras["MAQ_03BAR_02_19"].EmpresaId.Should().Be(_semente.Barretos, "o NroEmpresa 3 é a filial 010103");
        carteiras["MAQ_03BAR_02_19"].ResponsavelId.Should().Be(_semente.JoaoNoCrm, "o vendedor já tinha conta no CRM: casou pelo login");

        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeCarteirasCriadas).Should().Be(5);
        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeDonosCriados).Should().Be(3, "Maria, INT.MERCADO e Ana; João já existe e Pedro é de carteira desligada");

        var vigentes = await db.ClienteCarteiras.AsNoTracking().Where(v => v.DesvinculadoEm == null).ToListAsync();
        vigentes.Should().HaveCount(5);
        vigentes.Should().OnlyContain(v => v.DiasCicloContato == null && v.Classe == ClasseDeCliente.C,
            "a classe e o ciclo do vínculo saem no cenário A da issue 53: a sincronia não os decide");
        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosCriados).Should().Be(5);
    }

    [Fact]
    public async Task O_pool_do_INT_MERCADO_a_gaveta_e_o_digital_se_distinguem_pela_regra_da_carga_antiga()
    {
        await Sincronizar();

        await using var db = Sistema();
        var carteiras = await db.Carteiras.AsNoTracking().ToDictionaryAsync(c => c.Codigo);
        var usuarios = await db.Usuarios.AsNoTracking().ToDictionaryAsync(u => u.Id);

        // MAQ_: vendedor de campo, pessoa.
        carteiras["MAQ_01RIB_02_18"].Natureza.Should().Be(NaturezaDaCarteira.Comercial);
        usuarios[carteiras["MAQ_01RIB_02_18"].ResponsavelId].Natureza.Should().Be(NaturezaDoUsuario.Pessoa);

        // TBA_: o pool continua comercial — a carteira é real —, mas o dono é uma ÁREA.
        carteiras["TBA_FILIAIS_708"].Natureza.Should().Be(NaturezaDaCarteira.Comercial);
        usuarios[carteiras["TBA_FILIAIS_708"].ResponsavelId].Natureza.Should().Be(NaturezaDoUsuario.Departamento);

        // A gaveta que se declara no nome.
        carteiras["TBA_S_POTENCIAL_717"].Natureza.Should().Be(NaturezaDaCarteira.Administrativa);

        // DGT_: comercial, de pessoa — o que a distingue do campo é o código de origem, que fica no código da carteira.
        carteiras["DGT_01RIB_752"].Natureza.Should().Be(NaturezaDaCarteira.Comercial);
        usuarios[carteiras["DGT_01RIB_752"].ResponsavelId].Natureza.Should().Be(NaturezaDoUsuario.Pessoa);
        CargaDeCarteirasDoVortice.Prefixo("DGT_01RIB").Should().Be("DGT_");
        CargaDeCarteirasDoVortice.Prefixo("TBA_FILIAIS").Should().Be("TBA_");
        CargaDeCarteirasDoVortice.Prefixo("MAQ_01RIB_02").Should().Be("MAQ_");
        CargaDeCarteirasDoVortice.Prefixo("CONTA_CHAVE_01").Should().Be("outras");
    }

    [Fact]
    public async Task O_dono_sem_conta_ganha_uma_aguardando_liberacao_sem_perfil_e_o_que_tem_e_reaproveitado()
    {
        await Sincronizar();

        await using var db = Sistema();
        var maria = await db.Usuarios.AsNoTracking().SingleAsync(u => u.NomePrincipal == Principal("maria.souza"));

        maria.AguardaLiberacao.Should().BeTrue();
        maria.EstaAtivo.Should().BeTrue();
        maria.AindaNaoEntrouPeloEntraId.Should().BeTrue();
        maria.EmpresaId.Should().Be(_semente.RibeiraoPreto, "a filial provisória é a da carteira dela");
        maria.NomeExibicao.Should().Be("MARIA SOUZA FICTICIA");
        (await db.UsuariosPerfis.AsNoTracking().AnyAsync(c => c.UsuarioId == maria.Id)).Should().BeFalse("nasce sem perfil nenhum");

        (await db.Usuarios.CountAsync(u => u.NomePrincipal.StartsWith("joao.silva"))).Should().Be(1, "João já tinha conta: não se cria outra");
        (await db.Usuarios.AnyAsync(u => u.NomePrincipal == Principal("pedro.saiu"))).Should().BeFalse(
            "a carteira dele é de vendedor desligado e não entrou — não há por que criar a conta");
        (await db.Usuarios.AnyAsync(u => u.NomePrincipal == Principal("robo.app"))).Should().BeFalse(
            "o dono-robô só responde por carteira de teste, que não entra");
        (await db.Carteiras.AnyAsync(c => c.Codigo.StartsWith("TESTE_"))).Should().BeFalse();

        // O DE-PARA do dono fica gravado: a próxima rodada o reencontra pelo identificador do Vórtice.
        (await db.ChavesExternas.CountAsync(c => c.Entidade == nameof(Usuario))).Should().Be(4);
        (await db.ChavesExternas.CountAsync(c => c.Entidade == nameof(Carteira))).Should().Be(5);
    }

    [Fact]
    public async Task O_operador_da_carga_nunca_e_uma_conta_que_aguarda_liberacao()
    {
        // Uma conta que espera, com identificador MENOR que o do operador: sem o filtro, seria ela a escolhida.
        await using (var db = Sistema())
        {
            var esperando = Usuario.CriarAguardandoLiberacao(Guid.NewGuid(), "alguem.esperando", "ALGUEM", NaturezaDoUsuario.Pessoa, _semente.RibeiraoPreto, 1, Agora);
            db.Entry(esperando).Property(u => u.Id).CurrentValue = 50L;
            db.Usuarios.Add(esperando);
            await db.SaveChangesAsync();
        }

        var (_, usuarioId, _, erro) = await PreparacaoDaCarga.PrepararAsync(
            _opcoes, new DiarioDeAlcanceEntreEmpresasEmLog(NullLogger<DiarioDeAlcanceEntreEmpresasEmLog>.Instance));

        erro.Should().BeNull();
        usuarioId.Should().Be(_semente.Operador, "quem ainda não foi liberado não responde por registro nenhum");
    }

    [Fact]
    public async Task O_que_nao_casa_fica_na_trilha_com_o_motivo_e_nunca_e_descartado()
    {
        await Sincronizar();

        await using var db = Sistema();
        var registros = await db.RegistrosDeOrigem.AsNoTracking()
            .Where(r => r.Fluxo == CargaDeCarteirasDoVortice.Fluxo)
            .ToDictionaryAsync(r => r.ChaveOrigem);

        registros.Should().HaveCount(11, "um registro por vínculo lido — os que entram e os que não");
        registros.Values.Count(r => r.Decisao == DecisaoDaIntegracao.Importado).Should().Be(5);

        registros["5/18"].Motivos.Should().Be(MotivoDePendenciaDaCarteira.SemDocumento);
        registros["6/18"].Motivos.Should().Be(MotivoDePendenciaDaCarteira.DocumentoZerado);
        registros["7/18"].Motivos.Should().Be(MotivoDePendenciaDaCarteira.ClienteAusenteDoCrm,
            "sem o cadastro do Protheus não há como dizer por que o cliente não está no CRM");
        registros["8/714"].Motivos.Should().Be(MotivoDePendenciaDaCarteira.FilialDaCarteiraForaDoCrm);
        registros["9/900"].Motivos.Should().Be(MotivoDePendenciaDaCarteira.CarteiraDeVendedorDesligado);
        registros["11/724"].Motivos.Should().Be(MotivoDePendenciaDaCarteira.CarteiraDeTesteNaoCarregada,
            "o resíduo de teste não entra (decisão de 24/09/2026), mesmo com cliente que casa");

        registros["1/18"].LinhaNaOrigem.Should().Be("MAQ-NOVOS");
        registros["1/18"].UnidadeNaOrigem.Should().Be("MAQ_01RIB_02", "o registro diz de que carteira da origem ele é");
        registros["3/708"].Transformacoes.Should().Contain("zeros à esquerda", "o CPF 012… perdeu o zero na coluna numérica");

        (await db.Clientes.CountAsync()).Should().Be(4, "a sincronia nunca cria cliente");
    }

    // =============================================================================================
    // É sincronia
    // =============================================================================================

    [Fact]
    public async Task Rodar_de_novo_sem_mudanca_na_origem_nao_grava_nada_novo()
    {
        await Sincronizar();
        int vinculos, usuarios, carteiras;
        await using (var db = Sistema())
        {
            vinculos = await db.ClienteCarteiras.CountAsync();
            usuarios = await db.Usuarios.CountAsync();
            carteiras = await db.Carteiras.CountAsync();
        }

        var segunda = await Sincronizar(quando: Agora.AddDays(1));

        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosCriados).Should().Be(0);
        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosEncerrados).Should().Be(0);
        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosMantidos).Should().Be(5);
        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeDonosCriados).Should().Be(0);
        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeCarteirasCriadas).Should().Be(0);
        segunda.Valor(CargaDeCarteirasDoVortice.RotuloDeCarteirasAlteradas).Should().Be(0);
        segunda.Valor("registros já conhecidos sem alteração").Should().Be(11);

        await using var depois = Sistema();
        (await depois.ClienteCarteiras.CountAsync()).Should().Be(vinculos);
        (await depois.Usuarios.CountAsync()).Should().Be(usuarios);
        (await depois.Carteiras.CountAsync()).Should().Be(carteiras);
        (await depois.RegistrosDeOrigem.CountAsync(r => r.Fluxo == CargaDeCarteirasDoVortice.Fluxo)).Should().Be(11);
        (await depois.RegistrosDeOrigem.Where(r => r.ChaveOrigem == "1/18").Select(r => r.Leituras).SingleAsync()).Should().Be(2);
    }

    [Fact]
    public async Task O_vinculo_que_sumiu_e_encerrado_e_quem_mudou_de_carteira_sai_de_uma_e_entra_na_outra()
    {
        await Sincronizar();

        // NA ORIGEM, no dia seguinte: o cliente 2 saiu da gaveta "sem potencial", e o cliente 1 foi para Barretos.
        var vinculos = Vinculos().Where(v => v.Chave is not ("10/717" or "1/18")).ToList();
        vinculos.Add(Com(1, 19, CpfDoCliente1));
        var amanha = Agora.AddDays(1);

        var relatorio = await Sincronizar(Leitura(vinculos: vinculos), quando: amanha);

        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosEncerrados).Should().Be(2);
        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosCriados).Should().Be(1);
        relatorio.Valor("clientes que mudaram de carteira (encerrado numa, incluído noutra)").Should().Be(1);

        await using var db = Sistema();
        var carteiras = await db.Carteiras.AsNoTracking().ToDictionaryAsync(c => c.Codigo, c => c.Id);
        var cliente1 = ClienteId(db, CpfDoCliente1);
        var cliente2 = ClienteId(db, CnpjDoCliente2);

        var doCliente1 = await db.ClienteCarteiras.AsNoTracking().Where(v => v.ClienteId == cliente1).ToListAsync();
        doCliente1.Should().HaveCount(2, "a saída não apaga: a linha antiga fica, encerrada");
        doCliente1.Single(v => v.CarteiraId == carteiras["MAQ_01RIB_02_18"]).DesvinculadoEm.Should().Be(amanha);
        doCliente1.Single(v => v.CarteiraId == carteiras["MAQ_03BAR_02_19"]).DesvinculadoEm.Should().BeNull();

        (await db.ClienteCarteiras.AsNoTracking().SingleAsync(v => v.ClienteId == cliente2 && v.CarteiraId == carteiras["TBA_S_POTENCIAL_717"]))
            .DesvinculadoEm.Should().Be(amanha);

        var ausente = await db.RegistrosDeOrigem.AsNoTracking().SingleAsync(r => r.ChaveOrigem == "10/717");
        ausente.AusenteNaOrigemDesde.Should().Be(amanha, "o registro que some da origem é marcado, nunca apagado");

        // E A RODADA SEGUINTE, sem mudança, não mexe em mais nada.
        var terceira = await Sincronizar(Leitura(vinculos: vinculos), quando: amanha.AddDays(1));
        terceira.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosCriados).Should().Be(0);
        terceira.Valor(CargaDeCarteirasDoVortice.RotuloDeVinculosEncerrados).Should().Be(0);
    }

    [Fact]
    public async Task O_cliente_que_volta_entra_numa_linha_nova_e_o_intervalo_fora_continua_visivel()
    {
        await Sincronizar();
        var semO1 = Vinculos().Where(v => v.Chave != "1/18").ToList();
        await Sincronizar(Leitura(vinculos: semO1), quando: Agora.AddDays(1));
        await Sincronizar(quando: Agora.AddDays(2));

        await using var db = Sistema();
        var cliente1 = ClienteId(db, CpfDoCliente1);
        var linhas = await db.ClienteCarteiras.AsNoTracking().Where(v => v.ClienteId == cliente1).OrderBy(v => v.Id).ToListAsync();

        linhas.Should().HaveCount(2);
        linhas[0].DesvinculadoEm.Should().Be(Agora.AddDays(1));
        linhas[1].DesvinculadoEm.Should().BeNull();
    }

    [Fact]
    public async Task O_nome_e_o_dono_da_carteira_acompanham_a_origem()
    {
        await Sincronizar();

        // A carteira 18 passou para o João, com outro nome.
        var carteiras = Carteiras();
        carteiras[0] = ComVendedor(18, 1, "MAQ_01RIB_02", 1002, "RIBEIRAO PRETO 2");

        var relatorio = await Sincronizar(Leitura(carteiras: carteiras), quando: Agora.AddDays(1));

        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeCarteirasAlteradas).Should().Be(1);

        await using var db = Sistema();
        var carteira = await db.Carteiras.AsNoTracking().SingleAsync(c => c.Codigo == "MAQ_01RIB_02_18");
        carteira.Nome.Should().Be("RIBEIRAO PRETO 2");
        carteira.ResponsavelId.Should().Be(_semente.JoaoNoCrm);
        carteira.Codigo.Should().Be("MAQ_01RIB_02_18", "o código é estável: é ele que a tela e a trilha citam");
    }

    [Fact]
    public async Task A_carteira_cujo_vendedor_foi_desligado_tem_os_vinculos_encerrados()
    {
        await Sincronizar();

        var carteiras = Carteiras();
        carteiras[1] = ComVendedor(19, 3, "MAQ_03BAR_02", 1002, desligado: true);

        await Sincronizar(Leitura(carteiras: carteiras), quando: Agora.AddDays(1));

        await using var db = Sistema();
        var id = await db.Carteiras.Where(c => c.Codigo == "MAQ_03BAR_02_19").Select(c => c.Id).SingleAsync();
        (await db.ClienteCarteiras.AnyAsync(v => v.CarteiraId == id && v.DesvinculadoEm == null)).Should().BeFalse(
            "o BI exclui a carteira de vendedor desligado, e o vínculo sai daqui também — encerrado, não apagado");
        (await db.RegistrosDeOrigem.SingleAsync(r => r.ChaveOrigem == "2/19")).Motivos
            .Should().Be(MotivoDePendenciaDaCarteira.CarteiraDeVendedorDesligado);
    }

    [Fact]
    public async Task A_leitura_vazia_nao_encerra_nada()
    {
        await Sincronizar();

        var resultado = await Sincronia(DaCarga, Leitura(vinculos: []), Agora.AddDays(1), _semente).ExecutarAsync(false, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse("origem sem vínculo nenhum é leitura a conferir, não carteira vazia");
        await using var db = Sistema();
        (await db.ClienteCarteiras.CountAsync(v => v.DesvinculadoEm == null)).Should().Be(5);
    }

    // =============================================================================================
    // A conta do dono é a que o Entra ID entrega
    // =============================================================================================

    [Fact]
    public async Task Quando_a_pessoa_entra_pelo_Entra_ela_recebe_a_conta_da_carteira_depois_de_liberada()
    {
        await Sincronizar();

        var resolvedor = new ResolvedorDeContextoDoEntraId(
            _opcoes,
            Options.Create(new OpcoesDeContextoProvisorio()),
            Options.Create(new OpcoesDoPrimeiroLogin { CriarUsuarioAguardandoLiberacao = true }),
            NullLogger<ResolvedorDeContextoDoEntraId>.Instance);

        var oid = Guid.NewGuid();
        var entra = new IdentidadeDoEntra(oid, "Maria.Souza@tracbel.com.br", "maria.souza@tracbel.com.br", "Maria Souza");
        int contas;
        await using (var db = Sistema()) contas = await db.Usuarios.CountAsync();

        // ANTES DA LIBERAÇÃO: o login casa com a conta da carteira e é recusado com a frase de espera — sem criar outra.
        var antes = await resolvedor.ResolverAsync(entra, null, CancellationToken.None);
        antes.EhSucesso.Should().BeFalse();
        antes.Erro.Should().Be(ResolvedorDeContextoDoEntraId.MensagemAguardandoLiberacao);
        await using (var db = Sistema()) (await db.Usuarios.CountAsync()).Should().Be(contas, "a pessoa não ganha uma segunda conta, sem carteira");

        // O ADMINISTRADOR LIBERA.
        long mariaId;
        await using (var db = Sistema())
        {
            var maria = await db.Usuarios.SingleAsync(u => u.NomePrincipal == Principal("maria.souza"));
            maria.Liberar(_semente.RibeiraoPreto, _semente.Operador);
            await db.SaveChangesAsync();
            mariaId = maria.Id;
        }

        var depois = await resolvedor.ResolverAsync(entra, null, CancellationToken.None);
        depois.EhSucesso.Should().BeTrue(depois.Erro);
        depois.Valor.UsuarioId.Should().Be(mariaId, "é ESTA a conta que a pessoa recebe — a que já é dona da carteira");

        await using var leitura = Sistema();
        var vinculada = await leitura.Usuarios.AsNoTracking().SingleAsync(u => u.Id == mariaId);
        vinculada.IdentidadeExterna.Should().Be(oid);
        vinculada.NomePrincipal.Should().Be("Maria.Souza@tracbel.com.br");
        (await leitura.Carteiras.AsNoTracking().SingleAsync(c => c.Codigo == "MAQ_01RIB_02_18")).ResponsavelId.Should().Be(mariaId);

        // E A SINCRONIA SEGUINTE reencontra a mesma conta pelo de-para, mesmo com o nome principal já trocado.
        var seguinte = await Sincronizar(quando: Agora.AddDays(1));
        seguinte.Valor(CargaDeCarteirasDoVortice.RotuloDeDonosCriados).Should().Be(0);
        seguinte.Valor(CargaDeCarteirasDoVortice.RotuloDeCarteirasAlteradas).Should().Be(0);
    }

    [Fact]
    public async Task A_caixa_de_departamento_nao_vira_login_de_ninguem()
    {
        await Sincronizar();

        var resolvedor = new ResolvedorDeContextoDoEntraId(
            _opcoes, Options.Create(new OpcoesDeContextoProvisorio()), Options.Create(new OpcoesDoPrimeiroLogin()),
            NullLogger<ResolvedorDeContextoDoEntraId>.Instance);

        var resultado = await resolvedor.ResolverAsync(
            new IdentidadeDoEntra(Guid.NewGuid(), "int.mercado@tracbel.com.br", null, null), null, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("Departamento", "a conta do pool do INT.MERCADO é área, e área não é login de pessoa");
    }

    // =============================================================================================
    // A simulação
    // =============================================================================================

    [Fact]
    public async Task A_simulacao_conta_o_que_faria_e_nao_grava_nada()
    {
        var relatorio = await Sincronizar(simular: true);

        relatorio.Simulada.Should().BeTrue();
        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeCasados).Should().Be(5);
        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeDonosCriados).Should().Be(3);
        relatorio.Valor(CargaDeCarteirasDoVortice.RotuloDeCarteirasCriadas).Should().Be(5);

        await using var db = Sistema();
        (await db.LinhasDeNegocio.CountAsync()).Should().Be(0);
        (await db.Carteiras.CountAsync()).Should().Be(0);
        (await db.ClienteCarteiras.CountAsync()).Should().Be(0);
        (await db.Usuarios.CountAsync()).Should().Be(2);
        (await db.RegistrosDeOrigem.CountAsync()).Should().Be(0);
        (await db.ChavesExternas.CountAsync()).Should().Be(0);
        (await db.Sistemas.CountAsync()).Should().Be(0, "nem o sistema de origem é registrado na simulação");
    }
}
