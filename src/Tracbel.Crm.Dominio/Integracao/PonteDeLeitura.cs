using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Integracao;

/// <summary>
/// Uma correção que a ponte aplicou ao ler — o mesmo compromisso do documento 16, princípio 4:
/// toda correção automática fica registrada, nunca acontece em silêncio.
///
/// POR QUE ESTE TIPO NÃO É O <c>CorrecaoAplicada</c> DA CAMADA DE INTEGRAÇÃO: porque o domínio
/// não pode conhecer a camada de integração — as setas apontam para dentro. O adaptador traduz
/// o dele neste, na fronteira. É o preço de uma linha da camada anticorrupção, e é o preço
/// certo: quando a ponte for desligada, apaga-se a pasta e este tipo some junto.
/// </summary>
/// <param name="Campo">O campo corrigido, no nome que a resposta usa.</param>
/// <param name="ValorNoLegado">Como estava lá.</param>
/// <param name="ValorEntregue">Como está sendo entregue.</param>
/// <param name="Motivo">Por que foi corrigido.</param>
public sealed record AjusteNaLeitura(string Campo, string? ValorNoLegado, string ValorEntregue, string Motivo);

/// <summary>
/// O que a ponte devolve: o dado E a procedência, sempre juntos, num tipo só.
///
/// Estarem juntos NO TIPO é o que impede o carimbo de se perder no caminho: não existe
/// assinatura na ponte que devolva dado sem procedência.
/// </summary>
/// <typeparam name="T">O que foi lido.</typeparam>
/// <param name="Dados">O conteúdo.</param>
/// <param name="Procedencia">De onde veio e quando.</param>
public sealed record LeituraDoLegado<T>(T Dados, Procedencia Procedencia);

/// <summary>
/// Um cliente como ele existe no sistema legado, já traduzido para o nosso vocabulário.
///
/// NÃO É UMA ENTIDADE <see cref="Cliente"/>, e a diferença importa: isto é uma FOTOGRAFIA de
/// leitura, sem identidade nossa, sem proprietário, sem filial dona, e que não entra no nosso
/// banco por acidente. Para virar cliente nosso, alguém precisa cadastrar — e aí o dado passa
/// pelas nossas regras.
/// </summary>
/// <param name="IdentificadorNoLegado">A chave da linha no sistema de origem, para o front conseguir voltar nela.</param>
/// <param name="NomeRazao">Razão social ou nome.</param>
/// <param name="NomeFantasia">Nome fantasia, quando existe.</param>
/// <param name="TipoDePessoa">Física ou jurídica.</param>
/// <param name="Documento">CPF ou CNPJ já recomposto e formatado. Nulo quando não deu para recompor.</param>
/// <param name="DocumentoConfere">Verdadeiro quando o documento recomposto passa no dígito verificador.</param>
/// <param name="Cidade">Cidade.</param>
/// <param name="Uf">Unidade federativa.</param>
/// <param name="Email">E-mail.</param>
/// <param name="Telefone">Telefone já normalizado, quando deu.</param>
/// <param name="SituacaoNoLegado">A situação como o legado a guarda, sem tradução inventada.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem (UTC).</param>
/// <param name="Ajustes">O que a ponte corrigiu nesta linha.</param>
public sealed record ClienteNoLegado(
    long IdentificadorNoLegado,
    string NomeRazao,
    string? NomeFantasia,
    TipoDePessoa TipoDePessoa,
    string? Documento,
    bool DocumentoConfere,
    string? Cidade,
    string? Uf,
    string? Email,
    string? Telefone,
    string SituacaoNoLegado,
    DateTime? AtualizadoEm,
    IReadOnlyList<AjusteNaLeitura> Ajustes);

/// <summary>
/// Uma máquina do parque do cliente, como o legado a declara.
///
/// A CATEGORIA VEM DE LÁ e não é traduzida para o nosso catálogo de propósito: o legado guarda
/// o parque como texto por categoria (trator, colheitadeira, pulverizador...), sem chassi e sem
/// modelo de catálogo. Inventar aqui uma correspondência com <c>frota.Modelo</c> seria adivinhar
/// — e adivinhação numa ponte de leitura é como o dado errado entra sem ninguém assinar embaixo.
/// Quem quiser trazer a máquina para o nosso cadastro faz isso pelo cadastro, com chassi.
/// </summary>
/// <param name="IdentificadorNoLegado">A chave da linha na origem.</param>
/// <param name="Categoria">O tipo de máquina, como o legado o nomeia. Ex.: Trator, Pulverizador.</param>
/// <param name="Marca">A marca declarada.</param>
/// <param name="Modelo">O modelo declarado, em texto livre.</param>
/// <param name="Detalhe">A informação extra que a categoria carrega. Ex.: faixa de potência.</param>
/// <param name="Ano">Ano declarado, quando o texto da origem era mesmo um ano.</param>
/// <param name="EstaAtivo">Se a origem considera a máquina ativa no parque.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem (UTC).</param>
/// <param name="Ajustes">O que a ponte corrigiu nesta linha.</param>
public sealed record EquipamentoNoLegado(
    long IdentificadorNoLegado,
    string Categoria,
    string? Marca,
    string? Modelo,
    string? Detalhe,
    short? Ano,
    bool EstaAtivo,
    DateTime? AtualizadoEm,
    IReadOnlyList<AjusteNaLeitura> Ajustes);

/// <summary>O que a verificação da ponte responde: se dá para ler, e o que a origem disse.</summary>
/// <param name="Alcancavel">Verdadeiro quando a consulta de prova respondeu.</param>
/// <param name="Detalhe">O que aconteceu, em português.</param>
/// <param name="MilissegundosDeResposta">Quanto tempo a consulta de prova levou.</param>
public sealed record SaudeDaPonte(bool Alcancavel, string Detalhe, long MilissegundosDeResposta);
