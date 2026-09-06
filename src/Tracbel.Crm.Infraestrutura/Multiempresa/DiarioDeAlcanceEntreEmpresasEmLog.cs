using Microsoft.Extensions.Logging;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Infraestrutura.Multiempresa;

/// <summary>
/// O diário da via de escape, escrito no log da aplicação.
///
/// POR QUE <c>Warning</c> E NÃO <c>Information</c>: derrubar a fronteira de multiempresa é
/// evento raro e sempre deliberado. Se ele aparecer mil vezes por dia, alguma rotina está
/// abrindo escape como se fosse consulta normal — e é exatamente isso que o nível de alerta
/// precisa deixar visível no painel, sem ninguém procurar.
///
/// [V] O contraexemplo é do Vórtice: uma falha crítica de integração é registrada como
/// "Warning" e ninguém olha; e o acesso a dado pessoal não é registrado de forma nenhuma —
/// 114 eventos de login em nove anos.
///
/// ONDE ISTO AINDA VAI CRESCER: o mesmo evento deve virar linha em <c>auditoria.EventoDeAcesso</c>
/// quando a autenticação pelo Entra ID entrar e existir um usuário de verdade para carimbar.
/// O log é o registro que dá para ter hoje, e ele já cumpre a regra — o escape não acontece
/// em silêncio.
/// </summary>
/// <param name="log">O log da aplicação.</param>
public sealed class DiarioDeAlcanceEntreEmpresasEmLog(
    ILogger<DiarioDeAlcanceEntreEmpresasEmLog> log) : IDiarioDeAlcanceEntreEmpresas
{
    /// <inheritdoc />
    public void Abriu(ContextoAcesso acesso, string motivo) =>
        log.LogWarning(
            "Fronteira de empresa IGNORADA a pedido: {Usuario} (id {UsuarioId}, filial de casa " +
            "{EmpresaId}, servico de sistema: {EhSistema}) abriu alcance entre filiais. Motivo: {Motivo}",
            acesso.NomeExibicao, acesso.UsuarioId, acesso.EmpresaId, acesso.EhServicoDeSistema, motivo);

    /// <inheritdoc />
    public void Fechou(ContextoAcesso acesso, string motivo, TimeSpan duracao) =>
        log.LogWarning(
            "Fronteira de empresa RESTABELECIDA: {Usuario} (id {UsuarioId}) fechou o alcance " +
            "entre filiais depois de {Milissegundos} ms. Motivo declarado na abertura: {Motivo}",
            acesso.NomeExibicao, acesso.UsuarioId, (long)duracao.TotalMilliseconds, motivo);
}
