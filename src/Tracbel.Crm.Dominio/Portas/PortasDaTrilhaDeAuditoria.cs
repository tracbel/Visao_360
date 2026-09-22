using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>O que a tela de Auditoria pede (issue 135).</summary>
/// <param name="DeUtc">O início do período, incluso.</param>
/// <param name="AteUtc">O fim do período, excluso.</param>
/// <param name="Entidade">Só esta entidade (o nome do tipo, <c>UsuarioPerfil</c>); nulo, todas.</param>
/// <param name="RegistroId">Só este registro da entidade; nulo, todos.</param>
/// <param name="Autor">Trecho do nome ou do e-mail de quem alterou; nulo, qualquer um.</param>
/// <param name="Origem">Só gravações desta origem; nulo, todas.</param>
/// <param name="Operacao">Só inclusões, alterações ou exclusões; nulo, todas.</param>
/// <param name="Paginacao">A página, em eventos.</param>
public sealed record FiltroDaTrilha(
    DateTime DeUtc, DateTime AteUtc, string? Entidade, long? RegistroId, string? Autor,
    OrigemDaOperacao? Origem, OperacaoAuditada? Operacao, Paginacao Paginacao);

/// <summary>Um campo que mudou, como o banco guardou: identificador é número, enum é nome.</summary>
/// <param name="Campo">O nome da propriedade.</param>
/// <param name="Antes">Como estava; nulo na inclusão.</param>
/// <param name="Depois">Como ficou; nulo na exclusão.</param>
public sealed record CampoNaTrilha(string Campo, string? Antes, string? Depois);

/// <summary>
/// UM EVENTO DA TRILHA: o que uma gravação fez com um registro. As linhas de <c>AlteracaoDeCampo</c> gravadas no
/// mesmo <c>SaveChanges</c>, para o mesmo registro, pelo mesmo autor, têm o mesmo instante — e viram um evento só.
/// </summary>
/// <param name="Quando">O instante da gravação (UTC).</param>
/// <param name="Entidade">O nome do tipo alterado.</param>
/// <param name="RegistroId">O identificador interno do registro.</param>
/// <param name="Operacao">Inclusão, alteração ou exclusão.</param>
/// <param name="Origem">Pessoa, integração, importação, sistema ou rotina.</param>
/// <param name="Sistema">O sistema externo, quando a gravação veio de um.</param>
/// <param name="AutorId">Quem gravou.</param>
/// <param name="Autor">O nome de quem gravou.</param>
/// <param name="AutorLogin">O nome principal (e-mail) de quem gravou.</param>
/// <param name="FilialCodigo">A filial da linha da trilha.</param>
/// <param name="FilialNome">O nome da filial.</param>
/// <param name="Correlacao">A requisição ou a execução que causou a gravação.</param>
/// <param name="Campos">Os campos que mudaram.</param>
public sealed record EventoNaTrilha(
    DateTime Quando, string Entidade, long RegistroId, OperacaoAuditada Operacao, OrigemDaOperacao Origem,
    string? Sistema, long AutorId, string Autor, string AutorLogin, string FilialCodigo, string FilialNome,
    Guid? Correlacao, IReadOnlyList<CampoNaTrilha> Campos);

/// <summary>A que tabela um identificador gravado na trilha aponta, para a tela mostrar o nome.</summary>
public enum TipoDeReferencia
{
    /// <summary>Um usuário — pelo nome.</summary>
    Usuario,

    /// <summary>Uma filial — código e nome.</summary>
    Empresa,

    /// <summary>Um perfil — pelo nome.</summary>
    Perfil,

    /// <summary>Um município — nome e UF.</summary>
    Municipio,

    /// <summary>Um cliente — pela razão social.</summary>
    Cliente
}

/// <summary>
/// A LEITURA DA TRILHA DE AUDITORIA (issue 135). Só leitura: a trilha nasce do <c>SaveChanges</c> e ninguém a
/// altera. A fronteira de filial vale aqui como em todo dado com filial — é o filtro global do contexto.
/// </summary>
public interface IRepositorioDaTrilhaDeAuditoria
{
    /// <summary>Os eventos do filtro, do mais recente para trás, paginados por evento.</summary>
    Task<PaginaDe<EventoNaTrilha>> ListarAsync(FiltroDaTrilha filtro, CancellationToken ct);

    /// <summary>
    /// Como se chama cada registro, para a tela não mostrar "UsuarioPerfil 42": o nome do usuário, a razão social,
    /// "perfil → pessoa" na concessão. Registro apagado ou de entidade sem descrição fica de fora.
    /// </summary>
    Task<IReadOnlyDictionary<(string Entidade, long Id), string>> DescreverRegistrosAsync(
        IReadOnlyCollection<(string Entidade, long Id)> registros, CancellationToken ct);

    /// <summary>O nome de cada identificador gravado num campo que aponta para outra tabela.</summary>
    Task<IReadOnlyDictionary<(TipoDeReferencia Tipo, long Id), string>> ResolverReferenciasAsync(
        IReadOnlyCollection<(TipoDeReferencia Tipo, long Id)> referencias, CancellationToken ct);
}
