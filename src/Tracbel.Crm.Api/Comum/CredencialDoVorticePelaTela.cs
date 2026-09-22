using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Api.Comum;

/// <summary>
/// A PONTE DO VÓRTICE USA A CREDENCIAL DA TELA, QUANDO HÁ (issue 136).
///
/// <para>A ponte lê <see cref="OpcoesDoVortice"/>; esta pós-configuração troca a cadeia de conexão pela gravada em
/// Configurações › Integrações. Roda por requisição (a ponte recebe <c>IOptionsSnapshot</c>): trocar a senha na tela
/// vale na próxima busca, sem reiniciar a API. Sem credencial na tela — ou se ela não abrir nesta máquina —, fica a
/// variável de ambiente <c>Vortice__Conexao</c>, como antes.</para>
/// </summary>
/// <param name="contexto">O contexto do banco.</param>
/// <param name="resolvedor">Quem decide e abre a credencial.</param>
public sealed class CredencialDoVorticePelaTela(CrmDbContext contexto, IResolvedorDeConexoes resolvedor) : IPostConfigureOptions<OpcoesDoVortice>
{
    /// <inheritdoc />
    public void PostConfigure(string? name, OpcoesDoVortice options)
    {
        var conexao = contexto.Conexoes.AsNoTracking().FirstOrDefault(c => c.Codigo == ConexoesDoSistema.Vortice);
        if (conexao is null || resolvedor.OrigemDe(conexao) != OrigemDaCredencial.Tela) return;

        try
        {
            options.Conexao = resolvedor.Resolver(conexao).CadeiaDeConexao;
        }
        catch (InvalidOperationException)
        {
            // A credencial da tela não abre nesta máquina: a ponte segue com a do ambiente. O botão "Testar" é que diz
            // o motivo, com a frase do protetor.
        }
    }
}
