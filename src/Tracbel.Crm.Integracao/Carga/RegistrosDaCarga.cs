using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// O RECORTE da carga — o que se decidiu trazer, e nada além disso.
///
/// <para>O recorte é parâmetro e não constante de propósito: ele é a decisão que precisa ser
/// defendida por escrito (documento 24), e uma decisão defendida por escrito não pode ficar
/// escondida dentro de uma consulta.</para>
///
/// <para><b>A filial vem da atividade, não do cadastro.</b> O cadastro de pessoas do legado não
/// tem coluna de filial: quem tem é a agenda e o histórico. Uma pessoa fica na filial em que
/// mais interagiu no período — e uma pessoa sem nenhuma interação numa das filiais em operação
/// simplesmente não está no recorte, em vez de entrar numa filial arbitrada.</para>
/// </summary>
/// <param name="Ano">O ano-calendário de atividade que define quem entra.</param>
/// <param name="CodigosDeFilialNoLegado">As filiais em operação, no código numérico do legado.</param>
/// <param name="Limite">Teto de clientes, para ensaio. Nulo é sem teto.</param>
public sealed record RecorteDaCarga(int Ano, IReadOnlyList<int> CodigosDeFilialNoLegado, int? Limite = null);

/// <summary>
/// Um cliente do legado JÁ SANEADO — tipos de valor do domínio, mais a trilha do que mudou.
///
/// <para>Este tipo é o outro lado da quarentena: da linha crua do banco legado para cá, o
/// vocabulário é o nosso, e o formato de origem não atravessa. Quem consome isto não sabe — e
/// não pode saber — que o documento chegou partido em duas colunas numéricas.</para>
///
/// <para><b>As duas listas não são enfeite.</b> <see cref="Correcoes"/> cumpre o princípio 1.4
/// do documento 16 (toda correção automática fica registrada) e <see cref="Rejeicoes"/> cumpre
/// o 1.3 (rejeitar é melhor que corrigir em silêncio). Um cliente pode entrar com o e-mail
/// recusado: o registro é aceito, o campo não.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador da pessoa no sistema de origem, como texto.</param>
/// <param name="CodigoDaFilialNoLegado">A filial em que a pessoa mais interagiu no período.</param>
/// <param name="NomeRazao">Razão social ou nome, normalizado.</param>
/// <param name="NomeFantasia">Nome fantasia, normalizado.</param>
/// <param name="TipoDePessoa">Física ou jurídica.</param>
/// <param name="Documento">CPF ou CNPJ recomposto e conferido. Nulo quando a origem não tinha.</param>
/// <param name="Situacao">A situação traduzida do código de uma letra da origem.</param>
/// <param name="Email">E-mail normalizado, ou nulo.</param>
/// <param name="Telefone">Telefone normalizado, ou nulo.</param>
/// <param name="InscricaoEstadual">Inscrição estadual, ou nulo.</param>
/// <param name="AtividadeEconomica">Código CNAE, ou nulo.</param>
/// <param name="Endereco">O endereço principal, quando a origem tem os campos obrigatórios.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram, com o motivo.</param>
public sealed record ClienteParaCarga(
    string ChaveDeOrigem,
    int CodigoDaFilialNoLegado,
    string NomeRazao,
    string? NomeFantasia,
    TipoDePessoa TipoDePessoa,
    CpfCnpj? Documento,
    SituacaoDoCliente Situacao,
    string? Email,
    string? Telefone,
    string? InscricaoEstadual,
    string? AtividadeEconomica,
    EnderecoParaCarga? Endereco,
    DateTime? AtualizadoEm,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// O endereço principal do cliente, saneado.
///
/// <para>Só existe quando logradouro, município e UF sobreviveram ao saneamento — são os três
/// campos que a entidade <c>Endereco</c> exige. Endereço pela metade não entra: um logradouro
/// sem município não localiza ninguém e ocuparia a única vaga de endereço principal do
/// cliente.</para>
/// </summary>
/// <param name="Logradouro">Logradouro normalizado.</param>
/// <param name="Numero">Número, como texto — <c>S/N</c> é um número de endereço válido.</param>
/// <param name="Complemento">Complemento.</param>
/// <param name="Bairro">Bairro.</param>
/// <param name="Municipio">O município como TEXTO, do jeito que a origem o escreveu.</param>
/// <param name="ChaveDoMunicipioDeOrigem">
/// O identificador do município na origem — <c>GE_Pessoa.SeqCidade</c>, que aponta para
/// <c>GE_Cidade</c>. É por ele que a carga liga o endereço ao catálogo, e não pelo texto:
/// o ponteiro está preenchido em 96% dos cadastros e é o que a origem realmente usa. Nulo
/// quando a origem não aponta para cidade nenhuma — aí sobra o texto.
/// </param>
/// <param name="Uf">Unidade federativa, conferida contra as 27.</param>
/// <param name="Cep">CEP de oito dígitos, ou nulo.</param>
/// <param name="Latitude">Latitude em graus decimais, dentro da caixa do Brasil.</param>
/// <param name="Longitude">Longitude em graus decimais, dentro da caixa do Brasil.</param>
public sealed record EnderecoParaCarga(
    string Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string Municipio,
    string? ChaveDoMunicipioDeOrigem,
    string Uf,
    Cep? Cep,
    decimal? Latitude,
    decimal? Longitude);

/// <summary>
/// Um MUNICÍPIO do catálogo da origem (<c>GE_Cidade</c>), saneado.
///
/// <para><b>Sem código do IBGE, e é o achado a registrar.</b> A tabela de cidades da origem não
/// tem essa coluna: ela guarda um sequencial próprio, o nome, a UF, a região, a população, a
/// faixa de CEP e o DDD. O município é identificado por <b>nome mais UF</b>, e o risco disso
/// está escrito no documento 26, seção 4 — dois nomes iguais na mesma UF viram um município só
/// no CRM, e a origem tem 180 pares assim.</para>
///
/// <para><b>A UF é o campo que recusa a linha.</b> [V] Ela é texto livre na origem e guarda
/// hoje <c>**</c>, <c>EX</c>, <c>MI</c>, <c>PÁ</c> e o vazio em 229 das 10.214 linhas. Município
/// com UF que não é uma das 27 não entra — e a recusa fica registrada, com o valor.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O sequencial da cidade na origem.</param>
/// <param name="Nome">Nome do município, normalizado.</param>
/// <param name="Uf">Unidade federativa, conferida contra as 27.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record MunicipioParaCarga(
    string ChaveDeOrigem,
    string Nome,
    string Uf,
    DateTime? AtualizadoEm,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// O VÍNCULO CARTEIRA × MUNICÍPIO da origem (<c>IVS_CartCid</c>) — o agrupamento territorial
/// que existe de verdade.
///
/// <para>É o achado que substitui a "regional" do protótipo: <c>IVS_Regional</c> existe e tem
/// ZERO linhas, enquanto esta tabela tem 673 linhas em 91 carteiras. A carteira pertence a uma
/// filial e atende um conjunto de cidades — é assim que a Tracbel Agro se organiza no
/// território.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O par carteira+cidade da origem, como texto.</param>
/// <param name="ChaveDaCarteiraDeOrigem">A carteira, na origem.</param>
/// <param name="ChaveDoMunicipioDeOrigem">A cidade, na origem.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem.</param>
public sealed record MunicipioDeCarteiraParaCarga(
    string ChaveDeOrigem,
    string ChaveDaCarteiraDeOrigem,
    string ChaveDoMunicipioDeOrigem,
    DateTime? AtualizadoEm);

/// <summary>
/// Um contato do cliente, saneado.
///
/// <para>[V] No legado o contato é entidade fraca — chave composta com a pessoa, sem identidade
/// própria. A <see cref="ChaveDeOrigem"/> preserva o par para o de-para conseguir reencontrar a
/// mesma linha na recarga; do lado de cá, o contato passa a existir por si.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O par pessoa+contato da origem, como texto.</param>
/// <param name="ChaveDoClienteDeOrigem">A pessoa dona do contato, na origem.</param>
/// <param name="Nome">Primeiro nome, normalizado.</param>
/// <param name="Sobrenome">O que sobrou do nome, quando havia mais de uma palavra.</param>
/// <param name="Documento">CPF do contato, recomposto e conferido, ou nulo.</param>
/// <param name="Cargo">Cargo declarado.</param>
/// <param name="Email">E-mail normalizado.</param>
/// <param name="Telefone">Telefone normalizado.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record ContatoParaCarga(
    string ChaveDeOrigem,
    string ChaveDoClienteDeOrigem,
    string Nome,
    string? Sobrenome,
    CpfCnpj? Documento,
    string? Cargo,
    string? Email,
    string? Telefone,
    DateTime? AtualizadoEm,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// Uma máquina do parque, saneada e com o chassi já validado.
///
/// <para><b>Sem chassi não há máquina.</b> O chassi é a identidade da máquina agrícola e a chave
/// de deduplicação (documento 16, seção 4); a entidade <c>Equipamento</c> o exige. A linha do
/// legado que não tem um chassi de 17 posições válido é recusada e vai para a fila de descarte
/// com o motivo — não entra com chassi inventado.</para>
///
/// <para><b>Marca, família e modelo vêm do legado como TEXTO</b> e viram catálogo do lado de cá.
/// Não é invenção: é o valor que já estava lá, promovido a linha de catálogo, que é onde ele
/// deveria ter nascido.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador da linha de parque na origem.</param>
/// <param name="ChaveDoClienteDeOrigem">A pessoa dona da máquina, na origem.</param>
/// <param name="Chassi">O chassi validado.</param>
/// <param name="Marca">A marca declarada, normalizada.</param>
/// <param name="Categoria">A categoria do legado — vira família dentro da marca.</param>
/// <param name="Modelo">O modelo declarado, normalizado.</param>
/// <param name="Ano">Ano do modelo, quando o texto da origem era mesmo um ano.</param>
/// <param name="EstaAtivo">Se a origem considera a máquina ativa no parque.</param>
/// <param name="LocalizacaoDescrita">A observação de onde a máquina está, quando existe.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record EquipamentoParaCarga(
    string ChaveDeOrigem,
    string ChaveDoClienteDeOrigem,
    Chassi Chassi,
    string Marca,
    string Categoria,
    string Modelo,
    short? Ano,
    bool EstaAtivo,
    string? LocalizacaoDescrita,
    DateTime? AtualizadoEm,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// Uma linha da origem que NÃO passou no saneamento, e por quê.
///
/// <para>Recusar não é ignorar (documento 16, seção 8.3). Cada uma destas vira uma linha em
/// <c>integracao.MensagemDescartada</c>, com o conteúdo cru em JSON e o motivo em português —
/// consultável, contável e, no dia em que alguém decidir tratar, tratável. [V] É o oposto exato
/// da fila de e-mail do legado, com 22.512 falhas silenciosas e nenhuma nova tentativa.</para>
/// </summary>
/// <param name="Entidade">O que se tentou montar: Cliente, Contato ou Equipamento.</param>
/// <param name="Categoria">
/// A REGRA que recusou, curta e estável — é por ela que o relatório agrupa.
/// O motivo cita o valor da linha ("o chassi 'Nivaldo' não tem 17 posições") e por isso é
/// diferente em cada linha; agrupar por ele produziria mil grupos de um. A categoria é a mesma
/// para todas as linhas que caíram na mesma regra, que é o que se quer contar.
/// </param>
/// <param name="ChaveDeOrigem">O identificador da linha na origem.</param>
/// <param name="Motivo">Por que não entrou, em português, com o valor que causou a recusa.</param>
/// <param name="ConteudoJson">A linha como veio, em JSON, para conferência manual.</param>
public sealed record LinhaRecusada(
    string Entidade, string Categoria, string ChaveDeOrigem, string Motivo, string ConteudoJson);

/// <summary>
/// O resultado de uma leitura de lote: o que passou, o que não passou, e até onde se leu.
/// </summary>
/// <typeparam name="T">O tipo de registro saneado do lote.</typeparam>
/// <param name="Aceitos">As linhas que passaram no saneamento.</param>
/// <param name="Recusadas">As linhas que não passaram, com o motivo.</param>
/// <param name="LinhasLidas">Quantas linhas a origem devolveu.</param>
/// <param name="MaisRecenteNaOrigem">A alteração mais recente vista, que vira a marca de sincronismo.</param>
public sealed record LoteDaCarga<T>(
    IReadOnlyList<T> Aceitos,
    IReadOnlyList<LinhaRecusada> Recusadas,
    int LinhasLidas,
    DateTime? MaisRecenteNaOrigem);

/// <summary>
/// Uma venda perdida do legado JÁ SANEADA — o que o formulário respondeu, no nosso vocabulário.
///
/// <para>As chaves de origem chegam como texto e ainda não são identificadores nossos: quem
/// resolve o processo, o cliente e a filial é a carga, que tem o mapa de <c>ChaveExterna</c>.
/// Vir como texto aqui é o que permite a resposta entrar mesmo quando o processo dela ficou
/// fora do recorte — perder o motivo da perda por causa disso seria perder justamente o que se
/// foi buscar.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador da resposta do formulário na origem.</param>
/// <param name="ChaveDoProcessoNaOrigem">O processo perdido, quando a resposta aponta um.</param>
/// <param name="ChaveDoClienteNaOrigem">O cliente que comprou do concorrente.</param>
/// <param name="CodigoDaFilialNaOrigem">A filial, herdada do processo.</param>
/// <param name="RegistradaEm">Quando o CEN preencheu, já em UTC.</param>
/// <param name="OcorridaEm">Quando a venda foi perdida, quando a data é plausível.</param>
/// <param name="Motivo">Por que se perdeu, como o formulário oferece.</param>
/// <param name="TipoDoEquipamento">Tratores, colheitadeiras, implementos.</param>
/// <param name="MarcaDoConcorrente">Quem levou a venda.</param>
/// <param name="RevendaDoConcorrente">A revenda que fechou.</param>
/// <param name="ModeloDoConcorrente">O modelo que o concorrente vendeu.</param>
/// <param name="ModeloOfertado">O modelo que a Tracbel ofereceu.</param>
/// <param name="Quantidade">Quantas máquinas.</param>
/// <param name="PrecoDoConcorrente">O preço do concorrente, quando é preço.</param>
/// <param name="PrecoOfertado">O preço ofertado, quando é preço.</param>
/// <param name="ParticipamosDaNegociacao">Se a Tracbel participou. Nulo é "não se sabe".</param>
/// <param name="RegistradaPor">Quem preencheu, como a origem identifica.</param>
/// <param name="Correcoes">O que foi corrigido na entrada.</param>
/// <param name="Rejeicoes">Os campos recusados, com o registro aceito assim mesmo.</param>
public sealed record VendaPerdidaParaCarga(
    string ChaveDeOrigem,
    string? ChaveDoProcessoNaOrigem,
    string? ChaveDoClienteNaOrigem,
    int? CodigoDaFilialNaOrigem,
    DateTime RegistradaEm,
    DateOnly? OcorridaEm,
    string? Motivo,
    string? TipoDoEquipamento,
    string? MarcaDoConcorrente,
    string? RevendaDoConcorrente,
    string? ModeloDoConcorrente,
    string? ModeloOfertado,
    int Quantidade,
    decimal? PrecoDoConcorrente,
    decimal? PrecoOfertado,
    bool? ParticipamosDaNegociacao,
    string? RegistradaPor,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// O faturamento de um cliente numa filial e num mês, já agregado pela origem.
///
/// <para>O cliente vem como DOCUMENTO, e não como identificador: a tabela do ERP não conhece o
/// CRM. Quem traduz é a carga, que tem o cadastro carregado.</para>
/// </summary>
/// <param name="ChaveDeOrigem">Documento, filial e competência — o que identifica a linha.</param>
/// <param name="DocumentoDoCliente">CPF ou CNPJ, com 11 ou 14 dígitos.</param>
/// <param name="CodigoDaFilialNoLegado">A filial que faturou, no número da origem.</param>
/// <param name="Competencia">O mês, sempre no dia 1.</param>
/// <param name="ValorLiquido">O total líquido do mês.</param>
/// <param name="Notas">Notas fiscais distintas.</param>
/// <param name="Itens">Itens de nota.</param>
public sealed record FaturamentoParaCarga(
    string ChaveDeOrigem,
    string DocumentoDoCliente,
    int CodigoDaFilialNoLegado,
    DateOnly Competencia,
    decimal ValorLiquido,
    int Notas,
    int Itens);
