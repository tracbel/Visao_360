using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Crm;

/// <summary>
/// Eventos de domínio do Lead.
///
/// São os PONTOS DE EXTENSÃO do módulo. Para acrescentar comportamento — notificar alguém,
/// integrar com outro sistema, disparar uma regra — cria-se um manipulador para o evento
/// e registra-se uma linha em <c>meta.ManipuladorEvento</c>. Nenhum arquivo aqui muda.
///
/// São <c>record</c> imutáveis de propósito: um fato que já aconteceu não se altera.
/// </summary>
public sealed record LeadRecebido(
    Guid LeadChave,
    int EmpresaId,
    int OrigemId,
    DateTime OcorreuEm) : IEventoDominio;

/// <summary>O lead virou cliente, contato e possivelmente processo.</summary>
public sealed record LeadQualificado(
    Guid LeadChave,
    long ClienteId,
    long ContatoId,
    long? ProcessoId,
    int? LinhaNegocioId,
    long UsuarioId,
    DateTime OcorreuEm) : IEventoDominio;

/// <summary>O lead foi descartado, com motivo registrado.</summary>
public sealed record LeadDescartado(
    Guid LeadChave,
    int MotivoId,
    string? Observacao,
    long UsuarioId,
    DateTime OcorreuEm) : IEventoDominio;

/// <summary>Um descarte foi desfeito, com justificativa.</summary>
public sealed record LeadReaberto(
    Guid LeadChave,
    string Justificativa,
    long UsuarioId,
    DateTime OcorreuEm) : IEventoDominio;

/// <summary>O lead foi identificado como repetição de outro.</summary>
public sealed record LeadMarcadoDuplicado(
    Guid LeadChave,
    long LeadOriginalId,
    long UsuarioId,
    DateTime OcorreuEm) : IEventoDominio;
