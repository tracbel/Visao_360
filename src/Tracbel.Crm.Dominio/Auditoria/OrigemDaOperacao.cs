namespace Tracbel.Crm.Dominio.Auditoria;

/// <summary>
/// DE ONDE VEIO A GRAVAÇÃO — a pergunta que a trilha precisa responder antes de "quem".
///
/// <para>Uma alteração de município feita por uma pessoa na tela e a mesma alteração feita pela
/// leitura do IBGE têm o mesmo antes, o mesmo depois e, na carga, até o mesmo usuário responsável.
/// Sem a origem, a trilha não distingue correção humana de sincronização — e é justamente essa a
/// distinção que alguém procura quando um dado "mudou sozinho" (documento 41, fase 2).</para>
/// </summary>
public enum OrigemDaOperacao
{
    /// <summary>Uma pessoa, por uma rota da API.</summary>
    Usuario = 0,

    /// <summary>A leitura de um sistema externo — ART, Protheus, IBGE, Vórtice.</summary>
    Integracao = 1,

    /// <summary>Um arquivo trazido por alguém — as planilhas do comercial.</summary>
    Importacao = 2,

    /// <summary>O próprio CRM, sem pessoa nem sistema externo por trás.</summary>
    Sistema = 3,

    /// <summary>Uma rotina agendada.</summary>
    Job = 4
}

/// <summary>O que aconteceu com o registro na gravação que gerou a linha da trilha.</summary>
public enum OperacaoAuditada
{
    /// <summary>O registro nasceu.</summary>
    Inclusao = 0,

    /// <summary>O registro mudou.</summary>
    Alteracao = 1,

    /// <summary>
    /// O registro saiu — por exclusão lógica (<c>ExcluidoEm</c> passou a ter valor) ou, onde
    /// ela existe, física.
    /// </summary>
    Exclusao = 2
}
