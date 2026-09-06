using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Identidade;

/// <summary>
/// O porta-contexto de UMA requisição: quem está agindo, definido uma vez e lido por todo o
/// resto do processamento.
///
/// POR QUE UM PORTA-CONTEÚDO E NÃO UM ADAPTADOR QUE LÊ O HTTP: porque
/// <see cref="Persistencia.CrmDbContext"/> pede <see cref="IProvedorContextoAcesso"/> no
/// CONSTRUTOR, e o construtor roda quando o contexto de banco é resolvido — que pode ser antes
/// ou depois de qualquer coisa. Separar "onde o contexto mora" de "quem o descobre" resolve a
/// ordem: o meio de campo da API descobre e chama <see cref="Definir"/>; o banco lê daqui.
///
/// O efeito colateral é bom e é de arquitetura: a Infraestrutura não precisa conhecer ASP.NET
/// Core para carregar a identidade da requisição. Quem conhece HTTP é a API, que é a camada
/// cujo trabalho é justamente esse.
///
/// LER ANTES DE DEFINIR É ERRO DE PROGRAMAÇÃO, e por isso lança em vez de devolver um contexto
/// vazio. Um contexto vazio silencioso é o pior desfecho possível aqui: as consultas passariam
/// a filtrar por empresa 0 e devolveriam nada, e alguém gastaria horas procurando o dado que
/// "sumiu" (documento 03, convenção 7).
/// </summary>
public sealed class ContextoAcessoDaRequisicao : IProvedorContextoAcesso
{
    private ContextoAcesso? _atual;

    /// <inheritdoc />
    public ContextoAcesso Atual =>
        _atual ?? throw new InvalidOperationException(
            "O contexto de acesso desta requisição ainda não foi definido. " +
            "O meio de campo de contexto de acesso precisa rodar antes de qualquer uso do " +
            "CrmDbContext — confira a ordem em Program.cs.");

    /// <summary>Já foi definido? Serve ao diagnóstico e ao meio de campo.</summary>
    public bool EstaDefinido => _atual is not null;

    /// <summary>Define quem está agindo nesta requisição. Chamado uma vez, pelo meio de campo.</summary>
    /// <param name="contexto">O contexto resolvido.</param>
    public void Definir(ContextoAcesso contexto) => _atual = contexto;
}
