using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Frota;

/// <summary>
/// O DONO ATUAL DA MÁQUINA (decisões de 24/09/2026): o vínculo com a evidência, a confirmação do dono quando o Protheus e
/// o ART concordam, e o que a sincronia só faz quando há o que mudar — para a trilha de auditoria não ganhar linha
/// "nada mudou".
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class ProprietarioAtualTestes
{
    private static readonly Chassi UmChassi = Chassi.Criar("1CQ7250PVMR123456");
    private static readonly DateOnly Hoje = new(2026, 9, 24);

    private static Equipamento DoProtheus() =>
        Equipamento.RegistrarPelaIntegracao(1, UmChassi, OrigemDoEquipamento.Protheus, 100, anoFabricacao: 2021, anoModelo: 2022);

    [Fact]
    public void A_maquina_do_Protheus_nasce_sem_modelo_e_sem_dono_confirmado()
    {
        var maquina = DoProtheus();

        maquina.Origem.Should().Be(OrigemDoEquipamento.Protheus);
        maquina.ModeloId.Should().BeNull("o modelo do Protheus só aponta o catálogo por código idêntico");
        maquina.ClienteId.Should().BeNull();
        maquina.Situacao.Should().Be(SituacaoDoEquipamento.ProprietarioNaoConfirmado);
        (maquina.AnoFabricacao, maquina.AnoModelo).Should().Be(((short?)2021, (short?)2022));
    }

    [Fact]
    public void A_maquina_do_CRM_nao_nasce_pela_integracao()
    {
        var criar = () => Equipamento.RegistrarPelaIntegracao(1, UmChassi, OrigemDoEquipamento.Crm, 100);
        criar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Confirmar_o_dono_poe_a_maquina_em_operacao_e_repetir_nao_muda_nada()
    {
        var maquina = DoProtheus();

        maquina.ConfirmarProprietario(42, 100).Should().BeTrue();
        maquina.ClienteId.Should().Be(42);
        maquina.Situacao.Should().Be(SituacaoDoEquipamento.Ativo);

        var alteradaEm = maquina.AlteradoEm;
        maquina.ConfirmarProprietario(42, 100).Should().BeFalse("o mesmo dono de novo não é mudança");
        maquina.AlteradoEm.Should().Be(alteradaEm);
    }

    [Fact]
    public void Desfazer_a_confirmacao_devolve_a_maquina_ao_dono_nao_confirmado()
    {
        var maquina = DoProtheus();
        maquina.ConfirmarProprietario(42, 100);

        maquina.DesfazerConfirmacaoDoProprietario(100).Should().BeTrue();
        maquina.ClienteId.Should().BeNull();
        maquina.Situacao.Should().Be(SituacaoDoEquipamento.ProprietarioNaoConfirmado);
        maquina.DesfazerConfirmacaoDoProprietario(100).Should().BeFalse();
    }

    [Fact]
    public void A_maquina_em_estoque_nao_recebe_dono_pela_sincronia()
    {
        var maquina = Equipamento.Criar(1, 7, UmChassi, OrigemDoEquipamento.Crm, 100, situacao: SituacaoDoEquipamento.Estoque);

        var confirmar = () => maquina.ConfirmarProprietario(42, 100);
        confirmar.Should().Throw<RegraDeNegocioViolada>("estoque é decisão de quem cuida da máquina, não da sincronia");
    }

    [Fact]
    public void O_ano_so_e_preenchido_quando_vazio_e_quando_e_ano_de_verdade()
    {
        var maquina = Equipamento.RegistrarPelaIntegracao(1, UmChassi, OrigemDoEquipamento.Protheus, 100, anoModelo: 2020);

        maquina.DefinirAnosSeAusentes(0, 2023, 100).Should().BeFalse("0 não é ano, e o ano do modelo já estava informado");
        maquina.DefinirAnosSeAusentes(2019, 2023, 100).Should().BeTrue();
        (maquina.AnoFabricacao, maquina.AnoModelo).Should().Be(((short?)2019, (short?)2020));
        maquina.DefinirAnosSeAusentes(2018, 2024, 100).Should().BeFalse();
    }

    [Fact]
    public void A_maquina_do_Protheus_sem_modelo_pode_ser_editada_sem_modelo()
    {
        var maquina = DoProtheus();

        var editar = () => maquina.Alterar(null, 100, situacao: SituacaoDoEquipamento.ProprietarioNaoConfirmado);
        editar.Should().NotThrow("como a do ART, ela espera a correspondência segura com o catálogo");
    }

    [Fact]
    public void O_dono_atual_tem_evidencia_e_nao_aponta_venda()
    {
        var vinculo = VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(3, 42, 9, 1, Hoje, EvidenciaDoProprietario.OrdemDeServico, 100);

        vinculo.Natureza.Should().Be(NaturezaDoVinculoComEquipamento.ProprietarioAtual);
        vinculo.Evidencia.Should().Be(EvidenciaDoProprietario.OrdemDeServico);
        vinculo.VendaDeMaquinaId.Should().BeNull("o comprador de cada venda continua no vínculo dele");
        vinculo.ReferenciaEm.Should().Be(Hoje);
    }

    [Fact]
    public void Acompanhar_o_dono_atual_so_muda_quando_ha_diferenca()
    {
        var vinculo = VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(3, 42, 9, 1, Hoje, EvidenciaDoProprietario.CadastroAntigo, 100);

        vinculo.AcompanharProprietario(3, 1, Hoje, EvidenciaDoProprietario.CadastroAntigo, 100).Should().BeFalse();
        vinculo.AlteradoEm.Should().BeNull("rodar de novo sem mudança não é alteração");

        vinculo.AcompanharProprietario(3, 1, Hoje.AddDays(10), EvidenciaDoProprietario.OrdemDeServico, 100).Should().BeTrue();
        vinculo.Evidencia.Should().Be(EvidenciaDoProprietario.OrdemDeServico);
    }

    [Fact]
    public void O_comprador_na_venda_nao_acompanha_evidencia()
    {
        var comprador = VinculoDeClienteComEquipamento.RegistrarCompradorNaVenda(3, 42, 9, 77, 2, Hoje, 100);

        comprador.Evidencia.Should().BeNull();
        var acompanhar = () => comprador.AcompanharProprietario(3, 1, Hoje, EvidenciaDoProprietario.NotaDeVenda, 100);
        acompanhar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void O_comprador_resolvido_pelo_dono_do_Protheus_sai_da_fila_sem_fingir_cadastro()
    {
        var pendente = CompradorPendente.Registrar(2, CpfCnpj.Criar("52998224725"), Apuracao(), DateTime.UtcNow, 100);

        pendente.MarcarResolvidoPeloDonoNoProtheus(DateTime.UtcNow, 100).Should().BeTrue();
        pendente.Situacao.Should().Be(SituacaoDoCompradorPendente.ResolvidoPeloDonoNoProtheus);
        pendente.MarcarResolvidoPeloDonoNoProtheus(DateTime.UtcNow, 100).Should().BeFalse();

        pendente.MarcarCadastrado(DateTime.UtcNow, 100);
        pendente.Situacao.Should().Be(SituacaoDoCompradorPendente.Cadastrado, "se ele passar a existir no CRM, está cadastrado");
    }

    [Fact]
    public void A_venda_guarda_quando_o_comprador_e_o_dono_do_Protheus_no_lugar_do_comprador()
    {
        var venda = VendaDeMaquina.Registrar(2, "1001", 9, 42, Dados(), DateTime.UtcNow, 100, compradorPeloDonoNoProtheus: true);
        venda.CompradorPeloDonoNoProtheus.Should().BeTrue();

        venda.TrocarComprador(77, 100);
        venda.CompradorId.Should().Be(77);
        venda.CompradorPeloDonoNoProtheus.Should().BeFalse("o comprador verdadeiro passou a existir no CRM");
    }

    private static ApuracaoDoCompradorPendente Apuracao() => new(
        1, "FICTICIO", GrupoDoCompradorPendente.SemNotaNoProtheus, 1, 1, Hoje, Hoje, "010101", 0, null, null, null, false,
        SituacaoNoCadastroDoProtheus.Ausente, false, false, false, false, "definir filial e carteira responsáveis");

    private static DadosDaVendaNaOrigem Dados() => new(
        1, null, Hoje, Hoje, null, null, null, null, null, null, false, false, 1, "TRATOR MEDIO", "6110J", null, null, null, "hash", null);
}
