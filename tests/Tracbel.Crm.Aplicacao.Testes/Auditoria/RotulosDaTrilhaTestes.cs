using FluentAssertions;
using Tracbel.Crm.Aplicacao.Auditoria;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Auditoria;

/// <summary>
/// OS RÓTULOS ANDAM JUNTO COM A POLÍTICA (issue 135). Quem acrescenta um campo a <see cref="PoliticaDeAuditoria"/> e
/// esquece o nome dele aqui faz a tela de Auditoria mostrar o código da propriedade — este teste recusa antes.
/// </summary>
public sealed class RotulosDaTrilhaTestes
{
    [Fact]
    public void Toda_entidade_e_todo_campo_auditado_tem_nome_para_a_tela()
    {
        var semNome = PoliticaDeAuditoria.Entidades
            .Where(e => !RotulosDaTrilha.TemRotuloDaEntidade(e))
            .Concat(PoliticaDeAuditoria.Entidades
                .SelectMany(e => PoliticaDeAuditoria.CamposDe(e).Select(c => (Entidade: e, Campo: c)))
                .Where(x => !RotulosDaTrilha.TemRotuloDoCampo(x.Entidade, x.Campo))
                .Select(x => $"{x.Entidade}.{x.Campo}"))
            .ToList();

        semNome.Should().BeEmpty("a política e os rótulos mudam no mesmo commit");
    }

    [Fact]
    public void O_identificador_vira_nome_e_o_resto_fica_legivel()
    {
        var nomes = new Dictionary<(TipoDeReferencia Tipo, long Id), string> { [(TipoDeReferencia.Perfil, 6)] = "Gerência" };

        RotulosDaTrilha.Formatar("UsuarioPerfil", "PerfilId", "6", nomes).Should().Be("Gerência");
        RotulosDaTrilha.Formatar("UsuarioPerfil", "PerfilId", "99", nomes).Should().Be("nº 99", "o que não se acha continua achável pelo número");
        RotulosDaTrilha.Formatar("Usuario", "EstaAtivo", "false", nomes).Should().Be("não");
        RotulosDaTrilha.Formatar("PerfilPermissao", "Profundidade", "Organizacao", nomes).Should().Be("toda a organização");
        RotulosDaTrilha.Formatar("PerfilPermissao", "CodigoPermissao", "Auditoria.Ler", nomes).Should().Contain("trilha de auditoria").And.EndWith("(Auditoria.Ler)");
        RotulosDaTrilha.Formatar("Cliente", "NomeRazao", "Fazenda Boa Vista", nomes).Should().Be("Fazenda Boa Vista");
        RotulosDaTrilha.Formatar("Cliente", "NomeRazao", null, nomes).Should().BeNull();
    }

    [Fact]
    public void So_se_traduz_identificador_de_campo_que_a_politica_audita()
    {
        // Uma referência para um campo que a política não grava nunca seria usada — e esconderia um erro de digitação.
        RotulosDaTrilha.CamposQueApontam
            .Where(x => !PoliticaDeAuditoria.Audita(x.Entidade, x.Campo))
            .Should().BeEmpty();

        RotulosDaTrilha.ReferenciaDe("UsuarioPerfil", "UsuarioId").Should().Be(TipoDeReferencia.Usuario);
        RotulosDaTrilha.ReferenciaDe("Usuario", "EmpresaId").Should().Be(TipoDeReferencia.Empresa);
        RotulosDaTrilha.ReferenciaDe("Cliente", "NomeRazao").Should().BeNull();
    }
}
