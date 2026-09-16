using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;

namespace Tracbel.Crm.Infraestrutura.Identidade;

/// <summary>
/// QUEM É A PESSOA, quando ela entrou pelo Microsoft Entra ID.
///
/// <para>É o ponto de troca que <see cref="ResolvedorDeContextoProvisorio"/> anunciava: a mesma
/// saída — um <see cref="ContextoAcesso"/> —, só que a identidade vem de um token que a Microsoft
/// assinou, e não de um cabeçalho que qualquer um escreve.</para>
///
/// ---------------------------------------------------------------------------------------------
/// O CASAMENTO DA CONTA, e por que ele tem três passos.
///
/// <para>O Entra entrega o identificador de objeto e <c>fulano.exemplo@tracbel.com.br</c>. O banco
/// tem, para quase todo mundo, <c>fulano.exemplo@sem-email.vortice.invalid</c> — o Vórtice não
/// guardava e-mail, e a carga inventou um endereço num domínio que não pode existir. O único dado em
/// comum é a parte antes do <c>@</c>.</para>
///
/// <list type="number">
///   <item><b>Pelo identificador de objeto.</b> É o caso normal depois do primeiro login, e o único
///   que não depende de nome.</item>
///   <item><b>Pelo nome principal exato.</b> Para contas criadas já com o e-mail real.</item>
///   <item><b>Pela parte antes do <c>@</c>, só em conta que nunca entrou.</b> Casa uma vez, grava o
///   identificador e troca o nome inventado pelo real — dali em diante vale o passo 1.</item>
/// </list>
///
/// <para><b>O passo 3 é restrito de propósito.</b> Só vale para conta com o sufixo inventado, e só
/// para conta de pessoa. Uma conta que já entrou tem nome real, então um segundo "joao.silva" nunca
/// herda a carteira do primeiro; e uma caixa de departamento (<c>INT.MERCADO</c>) nunca vira login de
/// alguém. Quando nada casa, a resposta é "sem cadastro" — nunca um palpite.</para>
/// </summary>
public sealed class ResolvedorDeContextoDoEntraId(
    DbContextOptions<CrmDbContext> opcoesDoBanco,
    IOptions<OpcoesDeContextoProvisorio> opcoes,
    ILogger<ResolvedorDeContextoDoEntraId> log)
{
    /// <summary>
    /// Resolve o contexto de acesso de quem entrou pelo Entra ID.
    ///
    /// <para>A consulta roda sob contexto de SISTEMA pela mesma razão do resolvedor provisório: o
    /// escopo é montado lendo o cadastro de usuário, então ainda não existe nesta hora.</para>
    /// </summary>
    /// <param name="identidade">O que o token afirmou.</param>
    /// <param name="empresaInformada">A filial escolhida na tela. Nula usa a filial de casa.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ContextoAcesso>> ResolverAsync(
        IdentidadeDoEntra identidade, string? empresaInformada, CancellationToken ct)
    {
        await using var banco = new CrmDbContext(opcoesDoBanco, ProvedorDeContextoDeSistema.Instancia);
        var agora = DateTime.UtcNow;

        var usuario = await banco.Usuarios
            .FirstOrDefaultAsync(u => u.IdentidadeExterna == identidade.IdentidadeExterna && u.ExcluidoEm == null, ct);

        if (usuario is null)
        {
            var achado = await CasarPrimeiroLoginAsync(banco, identidade, ct);
            if (!achado.EhSucesso) return Resultado<ContextoAcesso>.NaoEncontrado(achado.Erro!);

            usuario = achado.Valor;

            if (!usuario.EstaAtivo)
                return Resultado<ContextoAcesso>.NaoEncontrado(
                    "A conta Microsoft entrou, mas o usuário correspondente está desativado no CRM.");

            var anterior = usuario.NomePrincipal;
            var email = Email.TentarCriar(identidade.Email ?? identidade.NomePrincipal, out var real)
                ? real
                : usuario.Email;

            usuario.VincularAoEntraId(identidade.IdentidadeExterna, identidade.NomePrincipal, email, agora);

            try
            {
                await banco.SaveChangesAsync(ct);
            }
            catch (DbUpdateException erro)
            {
                // O nome principal é único, inclusive entre contas excluídas. Uma conta apagada com o
                // mesmo e-mail real barra o vínculo — e isso é caso de a TI olhar, não de o login
                // estourar com 500.
                log.LogError(erro, "VÍNCULO RECUSADO pelo banco: {Upn} → usuário {Id}.", identidade.NomePrincipal, usuario.Id);
                return Resultado<ContextoAcesso>.NaoEncontrado(
                    "A conta Microsoft entrou, mas não foi possível ligá-la ao cadastro do CRM. " +
                    "Existe outro usuário com este e-mail — peça à TI para conferir.");
            }

            // O VÍNCULO SAI NO LOG COMO AVISO, com o antes e o depois. É a decisão mais sensível
            // deste arquivo — ela entrega uma carteira a uma conta — e precisa ser auditável sem
            // depender de alguém lembrar que ela aconteceu.
            log.LogWarning(
                "VÍNCULO: a conta Microsoft {Upn} ({Oid}) foi ligada ao usuário {Id}, antes {Anterior}.",
                identidade.NomePrincipal, identidade.IdentidadeExterna, usuario.Id, anterior);
        }
        else
        {
            if (!usuario.EstaAtivo)
                return Resultado<ContextoAcesso>.NaoEncontrado(
                    "A conta Microsoft entrou, mas o usuário correspondente está desativado no CRM.");

            if (usuario.RegistrarAcesso(agora)) await banco.SaveChangesAsync(ct);
        }

        // IDENTIDADE PROVADA PELO TOKEN: a concessão explícita vale sempre neste caminho.
        return await EscopoDeAcesso.MontarAsync(
            banco, usuario.Id, usuario.NomeExibicao, empresaInformada, usuario.EmpresaId,
            opcoes.Value.CabecalhoDeEmpresa, honrarConcessoesExplicitas: true, ct);
    }

    /// <summary>
    /// Os passos 2 e 3 do casamento — só rodam quando o identificador de objeto ainda não é
    /// conhecido.
    /// </summary>
    private static async Task<Resultado<Usuario>> CasarPrimeiroLoginAsync(
        CrmDbContext banco, IdentidadeDoEntra identidade, CancellationToken ct)
    {
        var upn = identidade.NomePrincipal.Trim().ToLowerInvariant();
        var arroba = upn.IndexOf('@');

        if (arroba <= 0)
            return Resultado<Usuario>.NaoEncontrado(
                "A conta Microsoft não trouxe um nome principal no formato de e-mail.");

        var inventado = upn[..arroba] + Usuario.SufixoSemEmail;

        // O nome principal é único no banco, então há no máximo UM exato e UM inventado.
        var candidatos = await banco.Usuarios
            .Where(u => u.ExcluidoEm == null &&
                        (u.NomePrincipal.ToLower() == upn || u.NomePrincipal.ToLower() == inventado))
            .ToListAsync(ct);

        var exato = candidatos.FirstOrDefault(u => string.Equals(u.NomePrincipal, upn, StringComparison.OrdinalIgnoreCase));
        if (exato is not null) return Resultado<Usuario>.Ok(exato);

        var pelaCarga = candidatos.FirstOrDefault(u => u.AindaNaoEntrouPeloEntraId);

        if (pelaCarga is not null && pelaCarga.Natureza != NaturezaDoUsuario.Pessoa)
            return Resultado<Usuario>.NaoEncontrado(
                $"A conta Microsoft corresponde a um cadastro do tipo {pelaCarga.Natureza}, que não " +
                "é login de pessoa. Peça à TI para criar o seu usuário no CRM.");

        return pelaCarga is not null
            ? Resultado<Usuario>.Ok(pelaCarga)
            : Resultado<Usuario>.NaoEncontrado(
                "A conta Microsoft entrou, mas não tem cadastro no CRM. " +
                "Peça à TI para liberar o seu usuário.");
    }
}
