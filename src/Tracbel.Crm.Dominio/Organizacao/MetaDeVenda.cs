namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// DE QUE PLANEJAMENTO A META VEIO, como a API Gestão de Negócios escreve.
///
/// <para><b>Consórcio é à parte</b> (D-M4, 27/09/2026): a meta de consórcio é em COTAS, e o CRM não mede cota vendida —
/// o realizado dela é "não medido pelo CRM". Ela não soma com a meta de máquinas.</para>
/// </summary>
public enum OrigemDaMeta
{
    /// <summary>A campanha anual de máquinas — a planilha "Divisão Time - Campanha Anual", hoje o cadastro da GN.</summary>
    Campanha = 0,

    /// <summary>O planejamento de consórcio, em cotas.</summary>
    Consorcio = 1
}
