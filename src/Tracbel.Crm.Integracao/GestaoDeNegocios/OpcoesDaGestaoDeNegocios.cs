namespace Tracbel.Crm.Integracao.GestaoDeNegocios;

/// <summary>
/// A CONFIGURAÇÃO DO CLIENTE DA API GESTÃO DE NEGÓCIOS (GN) — o sistema interno da Inteligência de Mercado, que
/// publica o cadastro de metas de venda e os painéis comerciais.
///
/// <para><b>Os nomes são os da issue [001]</b> (<c>.env.exemplo</c> e documento 05): na estação,
/// <c>GESTAO_NEGOCIOS_API_URL</c> e <c>GESTAO_NEGOCIOS_API_TOKEN</c>; no servidor, <c>GestaoDeNegocios__Base</c> e
/// <c>GestaoDeNegocios__Chave</c>. O caminho principal, desde a issue 136, é a tela: Configurações › Integrações grava
/// o endereço e a chave, protegida; as variáveis de ambiente continuam valendo como reserva.</para>
///
/// <para><b>O endereço é o NOME, com a validação do certificado inteira</b> (decisão D-M1 do Ricardo, 27/09/2026). O
/// certificado do servidor é o curinga <c>*.tracbel.com.br</c> de uma autoridade pública (GeoTrust/DigiCert), válido até
/// 04/04/2027; o único erro que aparecia era o de NOME, porque a chamada usava o IP. Chamado por
/// <c>https://negocios-agro.tracbel.com.br:5001</c>, ele passa na validação padrão e a renovação é transparente. Por
/// isso não há aqui impressão digital fixada nem "aceitar qualquer certificado": a chave vale para a API inteira, e
/// desligar a validação a entregaria a quem se pusesse no meio do caminho.</para>
/// </summary>
public sealed class OpcoesDaGestaoDeNegocios
{
    /// <summary>O nome da seção na configuração (<c>GestaoDeNegocios__Base</c>, <c>GestaoDeNegocios__Chave</c>).</summary>
    public const string Secao = "GestaoDeNegocios";

    /// <summary>
    /// O maior tamanho de página que a API aceita (<c>por_pagina</c> vai até 5000, medido em 27/09/2026). Acima disso a
    /// API recusaria o pedido — e o cliente recusa antes, com a frase certa.
    /// </summary>
    public const int TamanhoMaximoDaPagina = 5000;

    /// <summary>A base da API, sem barra no fim. Ex.: <c>https://negocios-agro.tracbel.com.br:5001</c>.</summary>
    public string? Base { get; set; }

    /// <summary>A chave (Bearer). Vem da tela ou de <c>GestaoDeNegocios__Chave</c>, nunca de arquivo versionado.</summary>
    public string? Chave { get; set; }

    /// <summary>Quanto esperar por uma página. A API responde em segundos; um minuto é folga para o servidor ocupado.</summary>
    public int TempoLimiteSegundos { get; set; } = 60;

    /// <summary>Quantas linhas por página. 500 é o padrão da própria API.</summary>
    public int TamanhoDaPagina { get; set; } = 500;

    /// <summary>Se a configuração tem o mínimo para tentar uma leitura.</summary>
    public bool EstaConfigurada => !string.IsNullOrWhiteSpace(Base) && !string.IsNullOrWhiteSpace(Chave);

    /// <summary>
    /// O que impede a leitura, em português — ou nulo quando a configuração serve. HTTP puro é recusado: a chave iria
    /// em claro pela rede.
    /// </summary>
    public string? Problema()
    {
        if (!EstaConfigurada)
            return "A API Gestão de Negócios não está configurada: grave o endereço e a chave em Configurações › Integrações, " +
                   "ou defina GestaoDeNegocios__Base e GestaoDeNegocios__Chave no servidor.";

        if (!Uri.TryCreate(Base!.Trim(), UriKind.Absolute, out var endereco) || endereco.Scheme != Uri.UriSchemeHttps)
            return "O endereço da API Gestão de Negócios precisa ser https:// — a chave não vai em claro pela rede.";

        if (TamanhoDaPagina is < 1 or > TamanhoMaximoDaPagina)
            return $"O tamanho da página vai de 1 a {TamanhoMaximoDaPagina}.";

        return null;
    }
}
