using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Clientes;

/// <summary>
/// O cliente como a LISTAGEM o mostra — o suficiente para a grade, e nada além.
///
/// A CHAVE É O GUID, nunca o identificador interno: quem enumera <c>/clientes/1</c>,
/// <c>/clientes/2</c> conta quantos clientes a Tracbel tem. O identificador sequencial existe
/// para ser índice, não para sair daqui (documento 04, seção 1.3).
/// </summary>
/// <param name="Chave">O GUID público do cliente.</param>
/// <param name="NomeRazao">Razão social ou nome da pessoa física.</param>
/// <param name="NomeFantasia">Nome fantasia, quando existe.</param>
/// <param name="TipoDePessoa">Fisica ou Juridica.</param>
/// <param name="Documento">CPF ou CNPJ formatado, quando cadastrado.</param>
/// <param name="Situacao">Onde o cliente está no ciclo comercial.</param>
/// <param name="CriadoEm">Quando o cadastro nasceu (UTC).</param>
/// <param name="AlteradoEm">Última alteração (UTC).</param>
/// <param name="EstaInativo">Verdadeiro quando o cadastro foi inativado logicamente.</param>
public sealed record ClienteResumo(
    Guid Chave,
    string NomeRazao,
    string? NomeFantasia,
    string TipoDePessoa,
    string? Documento,
    string Situacao,
    DateTime CriadoEm,
    DateTime? AlteradoEm,
    bool EstaInativo)
{
    /// <summary>Traduz a leitura para o que a listagem mostra.</summary>
    public static ClienteResumo De(ClienteComContexto leitura)
    {
        var c = leitura.Cliente;
        return new ClienteResumo(
            c.ChavePublica,
            c.NomeRazao,
            c.NomeFantasia,
            c.TipoDePessoa.ToString(),
            c.Documento?.Formatado(),
            c.Situacao.ToString(),
            c.CriadoEm,
            c.AlteradoEm,
            c.EstaExcluido);
    }
}

/// <summary>
/// O cliente como a FICHA o mostra. Acrescenta ao resumo o que a tela de edição precisa —
/// inclusive a <see cref="Versao"/>, que o <c>PUT</c> devolve para a conferência de
/// concorrência.
/// </summary>
/// <param name="Chave">O GUID público.</param>
/// <param name="NomeRazao">Razão social ou nome.</param>
/// <param name="NomeFantasia">Nome fantasia.</param>
/// <param name="TipoDePessoa">Fisica ou Juridica.</param>
/// <param name="Documento">CPF ou CNPJ formatado.</param>
/// <param name="DocumentoSemMascara">O mesmo documento, só dígitos — é o que a integração usa.</param>
/// <param name="InscricaoEstadual">Inscrição estadual.</param>
/// <param name="AtividadeEconomica">Código de atividade econômica.</param>
/// <param name="Situacao">Situação no ciclo comercial.</param>
/// <param name="SituacaoDesde">Desde quando está nesta situação (UTC).</param>
/// <param name="EmpresaId">A filial dona do cadastro — a fronteira de acesso.</param>
/// <param name="ProprietarioId">Quem responde pelo cliente.</param>
/// <param name="CriadoEm">Quando nasceu (UTC).</param>
/// <param name="AlteradoEm">Última alteração (UTC).</param>
/// <param name="EstaInativo">Se foi inativado logicamente.</param>
/// <param name="OrigemCodigo">Código do item de ORIGEM_LEAD — é o que o campo de seleção pré-seleciona.</param>
/// <param name="MotivoInativacaoCodigo">Código do item de MOTIVO_INATIVACAO, quando inativado.</param>
/// <param name="Versao">O carimbo de concorrência, em base64. Devolva-o no PUT.</param>
public sealed record ClienteDetalhe(
    Guid Chave,
    string NomeRazao,
    string? NomeFantasia,
    string TipoDePessoa,
    string? Documento,
    string? DocumentoSemMascara,
    string? InscricaoEstadual,
    string? AtividadeEconomica,
    string Situacao,
    DateTime SituacaoDesde,
    int EmpresaId,
    long ProprietarioId,
    DateTime CriadoEm,
    DateTime? AlteradoEm,
    bool EstaInativo,
    string? OrigemCodigo,
    string? MotivoInativacaoCodigo,
    string? Versao)
{
    /// <summary>Traduz a leitura para a ficha.</summary>
    public static ClienteDetalhe De(ClienteComContexto leitura)
    {
        var c = leitura.Cliente;
        return new ClienteDetalhe(
            c.ChavePublica,
            c.NomeRazao,
            c.NomeFantasia,
            c.TipoDePessoa.ToString(),
            c.Documento?.Formatado(),
            c.Documento?.Numero,
            c.InscricaoEstadual,
            c.AtividadeEconomica,
            c.Situacao.ToString(),
            c.SituacaoDesde,
            c.EmpresaId,
            c.ProprietarioId,
            c.CriadoEm,
            c.AlteradoEm,
            c.EstaExcluido,
            leitura.OrigemCodigo,
            leitura.MotivoInativacaoCodigo,
            c.Versao is null ? null : Convert.ToBase64String(c.Versao));
    }
}

/// <summary>
/// O que o <c>POST</c> de cliente aceita.
///
/// TUDO É <c>string?</c> DE PROPÓSITO, e não é preguiça: se o contrato tipasse os campos, o
/// desserializador recusaria a requisição antes de o nosso código rodar — e a resposta seria a
/// mensagem genérica do framework, não a nossa mensagem campo a campo. Aqui o texto entra cru e
/// é o caso de uso que decide o que dizer sobre cada campo (documento 16, princípio 1.3).
/// </summary>
/// <param name="NomeRazao">Razão social ou nome. Obrigatório.</param>
/// <param name="NomeFantasia">Nome fantasia.</param>
/// <param name="TipoDePessoa">Fisica ou Juridica. Obrigatório.</param>
/// <param name="Documento">CPF ou CNPJ, com ou sem máscara.</param>
/// <param name="InscricaoEstadual">Inscrição estadual.</param>
/// <param name="AtividadeEconomica">Código de atividade econômica.</param>
/// <param name="Situacao">Situação inicial. Padrão: Prospect.</param>
/// <param name="OrigemCodigo">Código do item do catálogo ORIGEM_LEAD. Nunca texto livre.</param>
public sealed record NovoCliente(
    string? NomeRazao = null,
    string? NomeFantasia = null,
    string? TipoDePessoa = null,
    string? Documento = null,
    string? InscricaoEstadual = null,
    string? AtividadeEconomica = null,
    string? Situacao = null,
    string? OrigemCodigo = null);

/// <summary>
/// O que o <c>PUT</c> de cliente aceita. A <see cref="Versao"/> é o que veio no <c>GET</c>.
/// </summary>
/// <param name="NomeRazao">Razão social ou nome. Obrigatório.</param>
/// <param name="NomeFantasia">Nome fantasia.</param>
/// <param name="TipoDePessoa">Fisica ou Juridica. Obrigatório.</param>
/// <param name="Documento">CPF ou CNPJ.</param>
/// <param name="InscricaoEstadual">Inscrição estadual.</param>
/// <param name="AtividadeEconomica">Código de atividade econômica.</param>
/// <param name="Situacao">Situação no ciclo comercial.</param>
/// <param name="OrigemCodigo">Código do item do catálogo ORIGEM_LEAD.</param>
/// <param name="Versao">O carimbo de concorrência lido no GET, em base64.</param>
public sealed record AlteracaoDeCliente(
    string? NomeRazao = null,
    string? NomeFantasia = null,
    string? TipoDePessoa = null,
    string? Documento = null,
    string? InscricaoEstadual = null,
    string? AtividadeEconomica = null,
    string? Situacao = null,
    string? OrigemCodigo = null,
    string? Versao = null);

/// <summary>O que o <c>DELETE</c> de cliente aceita: o motivo, que é de catálogo e é obrigatório.</summary>
/// <param name="MotivoCodigo">Código do item do catálogo MOTIVO_INATIVACAO.</param>
/// <param name="Versao">O carimbo de concorrência lido no GET, em base64.</param>
public sealed record InativacaoDeCliente(string? MotivoCodigo = null, string? Versao = null);
