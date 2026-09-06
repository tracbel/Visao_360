using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Crm;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Crm;

/// <summary>
/// Testes do ciclo de vida do Lead.
///
/// O valor não está nos testes de caminho feliz — está nos que provam que o sistema
/// RECUSA o que não pode acontecer.
/// </summary>
[Trait("Categoria", "Dominio")]
public class LeadTestes
{
    private const int Empresa = 1;
    private const int Origem = 10;
    private const long Vendedor = 100;
    private const long Sistema = 1;

    private static Lead CriarLeadValido() => Lead.Criar(
        empresaId: Empresa,
        nomeContato: "João da Silva",
        origemId: Origem,
        proprietarioId: Vendedor,
        criadoPorId: Sistema,
        email: Email.Criar("joao@fazendaboavista.com.br"),
        nomeEmpresa: "Fazenda Boa Vista");

    // ---------------------------------------------------------------- criação

    [Fact]
    public void Nasce_novo_e_registra_evento()
    {
        var lead = CriarLeadValido();

        lead.Situacao.Should().Be(SituacaoLead.Novo);
        lead.ChavePublica.Should().NotBe(Guid.Empty);
        lead.Eventos.Should().ContainSingle().Which.Should().BeOfType<LeadRecebido>();
    }

    [Fact]
    public void Recusa_lead_sem_nome()
    {
        var acao = () => Lead.Criar(Empresa, "  ", Origem, Vendedor, Sistema,
            email: Email.Criar("a@b.com"));

        acao.Should().Throw<RegraDeNegocioViolada>().WithMessage("*nome*");
    }

    [Fact]
    public void Recusa_lead_sem_nenhuma_forma_de_contato()
    {
        // [V] Os 489 processos gerados pelo conector do TALLOS Chat, de 99 pessoas,
        // ficaram 98% parados em "Apresentação / EM ABERTO" — lead que ninguém consegue
        // atender. Barrar na entrada é mais barato do que descobrir na carteira.
        var acao = () => Lead.Criar(Empresa, "João", Origem, Vendedor, Sistema);

        acao.Should().Throw<RegraDeNegocioViolada>().WithMessage("*e-mail ou telefone*");
    }

    [Fact]
    public void Guarda_o_payload_original_intacto()
    {
        // [V] No Vórtice, campo não mapeado na tela "Integração RD" é DESCARTADO.
        // Guardar o cru é o que permite reprocessar quando o mapeamento for corrigido.
        const string payload = """{"identificador":"lp-colheitadeira-2026","utm_source":"google"}""";

        var lead = Lead.Criar(Empresa, "Maria", Origem, Vendedor, Sistema,
            telefone: Telefone.Criar("17999990000"), payloadOriginal: payload);

        lead.PayloadOriginal.Should().Be(payload);
    }

    // ---------------------------------------------------------------- qualificação

    [Fact]
    public void Qualificar_liga_aos_registros_criados_e_mantem_o_lead_vivo()
    {
        // [DYN] O Salesforce trava o lead como somente leitura na conversão.
        // O Dynamics o mantém vivo, com ponteiro de volta. Escolhemos o segundo.
        var lead = CriarLeadValido();

        lead.Qualificar(clienteId: 500, contatoId: 600, processoId: 700, usuarioId: Vendedor);

        lead.Situacao.Should().Be(SituacaoLead.Qualificado);
        lead.ClienteGeradoId.Should().Be(500);
        lead.ContatoGeradoId.Should().Be(600);
        lead.ProcessoGeradoId.Should().Be(700);
        lead.QualificadoEm.Should().NotBeNull();
        lead.QualificadoPorId.Should().Be(Vendedor);
        lead.EstaExcluido.Should().BeFalse("o lead sobrevive à qualificação");
    }

    [Fact]
    public void Nao_qualifica_duas_vezes()
    {
        var lead = CriarLeadValido();
        lead.Qualificar(500, 600, 700, Vendedor);

        var acao = () => lead.Qualificar(501, 601, 701, Vendedor);

        acao.Should().Throw<RegraDeNegocioViolada>().WithMessage("*já foi qualificado*");
    }

    [Fact]
    public void Nao_qualifica_lead_descartado_sem_reabrir_antes()
    {
        var lead = CriarLeadValido();
        lead.Descartar(motivoId: 3, observacao: "sem perfil", usuarioId: Vendedor);

        var acao = () => lead.Qualificar(500, 600, null, Vendedor);

        acao.Should().Throw<RegraDeNegocioViolada>().WithMessage("*Reabra*");
    }

    // ---------------------------------------------------------------- descarte e reabertura

    [Fact]
    public void Descartar_exige_motivo_e_permite_desfazer()
    {
        // [V] "Atividade Cancelada" no Vórtice cancela o processo INTEIRO, sem motivo
        // registrado e SEM DESFAZER. A tela não tem "desfazer andamento".
        var lead = CriarLeadValido();

        lead.Descartar(motivoId: 7, observacao: "comprou do concorrente", usuarioId: Vendedor);
        lead.Situacao.Should().Be(SituacaoLead.Descartado);
        lead.MotivoDescarteId.Should().Be(7);

        lead.Reabrir("cliente voltou a procurar em agosto", Vendedor);
        lead.Situacao.Should().Be(SituacaoLead.Novo);
        lead.MotivoDescarteId.Should().BeNull();
    }

    [Fact]
    public void Reabertura_exige_justificativa()
    {
        var lead = CriarLeadValido();
        lead.Descartar(7, null, Vendedor);

        var acao = () => lead.Reabrir("   ", Vendedor);

        acao.Should().Throw<RegraDeNegocioViolada>().WithMessage("*justificativa*");
    }

    [Fact]
    public void Nao_reabre_lead_que_nao_esta_descartado()
    {
        var lead = CriarLeadValido();

        var acao = () => lead.Reabrir("qualquer coisa", Vendedor);

        acao.Should().Throw<RegraDeNegocioViolada>().WithMessage("*descartado*");
    }

    [Fact]
    public void Nao_descarta_lead_ja_qualificado()
    {
        var lead = CriarLeadValido();
        lead.Qualificar(500, 600, 700, Vendedor);

        var acao = () => lead.Descartar(7, null, Vendedor);

        acao.Should().Throw<RegraDeNegocioViolada>().WithMessage("*Encerre o processo*");
    }

    // ---------------------------------------------------------------- reconversão

    [Fact]
    public void Lead_conhecido_pode_ser_retrabalhado_sem_limite_vitalicio()
    {
        // [V] A ação 897 "Validar e Qualificar LEAD" tem QtdeMaxPessoa = 1, e o limite é
        // VITALÍCIO: em 193 casos medidos, 189 (98%) foram bloqueados por já ter havido uma
        // 897 antes — mas só 7 ainda estavam abertas. Reconversão de lead conhecido nunca
        // vira tarefa nova para o vendedor.
        //
        // Aqui não existe limite vitalício: a política de retrabalho é uma REGRA com
        // condição e janela temporal (wf.Regra.Condicao), não uma trava no catálogo.
        var lead = CriarLeadValido();

        lead.Descartar(7, "sem verba este ano", Vendedor);
        lead.Reabrir("safra nova, verba liberada", Vendedor);
        lead.Qualificar(500, 600, 700, Vendedor);

        lead.Situacao.Should().Be(SituacaoLead.Qualificado);
    }

    [Fact]
    public void MarcarEmContato_e_idempotente()
    {
        var lead = CriarLeadValido();

        lead.MarcarEmContato(Vendedor);
        lead.MarcarEmContato(Vendedor);

        lead.Situacao.Should().Be(SituacaoLead.EmContato);
    }
}
