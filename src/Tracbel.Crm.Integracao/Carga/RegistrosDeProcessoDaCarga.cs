using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Integracao.Saneamento;

namespace Tracbel.Crm.Integracao.Carga;

/// <summary>
/// Um usuário do sistema de origem, saneado — <b>sem senha, nunca</b>.
///
/// <para>[V] A coluna de senha do legado tem 30 bytes sem sal e 80 usuários compartilham
/// exatamente o mesmo valor armazenado. Ela não é lida por esta carga: nem para conferir, nem
/// para migrar, nem para contar. O CRM novo delega a autenticação ao Entra ID, e um espelho de
/// senha herdada seria um passivo de segurança criado de propósito.</para>
///
/// <para><b>O que vem, e é só o necessário para os vínculos terem dono:</b> o login, o nome, a
/// filial em que a pessoa mais trabalhou no período e o último acesso. Nada mais.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador do usuário no sistema de origem.</param>
/// <param name="Login">O login, como a origem o guarda. É o que a agenda referencia por texto.</param>
/// <param name="NomeCompleto">Nome completo.</param>
/// <param name="CodigoDaFilialNoLegado">A filial em que mais trabalhou no período.</param>
/// <param name="UltimoAcessoEm">Último acesso conhecido, ou nulo para quem nunca entrou.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record UsuarioParaCarga(
    string ChaveDeOrigem,
    string Login,
    string NomeCompleto,
    int CodigoDaFilialNoLegado,
    DateTime? UltimoAcessoEm,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// Uma linha de negócio do sistema de origem, com a cadência de contato que ela declara.
///
/// <para>[V] O departamento do legado é o que mais se aproxima da linha de negócio: são 29
/// cadastrados, 13 com alguma pessoa e 16 vazios. Só os que têm gente do recorte vêm — catálogo
/// morto migrado continua morto, e ocupa o seletor da tela.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador do departamento na origem.</param>
/// <param name="Codigo">Código estável.</param>
/// <param name="Nome">Nome legível.</param>
/// <param name="DiasCicloClasseA">Cadência esperada para cliente classe A, em dias.</param>
/// <param name="DiasCicloClasseB">Cadência esperada para classe B.</param>
/// <param name="DiasCicloClasseC">Cadência esperada para classe C.</param>
/// <param name="DiasCicloClasseD">Cadência esperada para classe D.</param>
public sealed record LinhaDeNegocioParaCarga(
    string ChaveDeOrigem,
    string Codigo,
    string Nome,
    short? DiasCicloClasseA,
    short? DiasCicloClasseB,
    short? DiasCicloClasseC,
    short? DiasCicloClasseD);

/// <summary>
/// Uma carteira do sistema de origem, com o responsável já resolvido.
///
/// <para>[V] A carteira do legado aponta para DOIS cadastros de dono: o usuário do CRM
/// (<c>SeqUsrResp</c>) e o vendedor comercial (<c>SeqVendedor</c>), que pode não ser usuário do
/// sistema. Aqui os dois viram um: o responsável é sempre um usuário, e a resolução do vendedor
/// para usuário acontece na leitura, não na tela.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador da carteira na origem.</param>
/// <param name="CodigoDaFilialNoLegado">A filial dona.</param>
/// <param name="Codigo">Código estável.</param>
/// <param name="Nome">Nome legível.</param>
/// <param name="ChaveDoResponsavelDeOrigem">O usuário responsável, já resolvido.</param>
/// <param name="ChaveDaLinhaDeNegocioDeOrigem">O departamento predominante da carteira.</param>
/// <param name="TemSupervisorNaOrigem">Se a origem declara supervisor. Mede o vazio.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record CarteiraParaCarga(
    string ChaveDeOrigem,
    int CodigoDaFilialNoLegado,
    string Codigo,
    string Nome,
    string? ChaveDoResponsavelDeOrigem,
    string? ChaveDaLinhaDeNegocioDeOrigem,
    bool TemSupervisorNaOrigem,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// O vínculo cliente × carteira — a carteirização, que é o melhor ativo do modelo antigo.
///
/// <para><b>A data do último contato NÃO vem daqui.</b> A coluna equivalente do legado só é
/// preenchida para desfechos que alguém marcou, numa tabela de configuração esquecida, e por
/// isso 31.556 clientes carteirizados têm "nunca contatado" por construção. A nossa data nasce
/// das interações efetivamente carregadas — o que a torna verificável.</para>
/// </summary>
/// <param name="ChaveDeOrigem">A chave composta do vínculo na origem.</param>
/// <param name="ChaveDoClienteDeOrigem">A pessoa.</param>
/// <param name="ChaveDaCarteiraDeOrigem">A carteira.</param>
/// <param name="Classe">A classe do cliente na carteira.</param>
/// <param name="ClasseVeioDaOrigem">Se a classe foi lida ou assumida.</param>
/// <param name="DiasCicloContato">Cadência declarada no vínculo, quando existe.</param>
/// <param name="VinculadoEm">Quando o vínculo nasceu na origem.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record VinculoDeCarteiraParaCarga(
    string ChaveDeOrigem,
    string ChaveDoClienteDeOrigem,
    string ChaveDaCarteiraDeOrigem,
    ClasseDeCliente Classe,
    bool ClasseVeioDaOrigem,
    short? DiasCicloContato,
    DateTime? VinculadoEm,
    DateTime? AtualizadoEm,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>Um modelo de fluxo do sistema de origem que aparece no dado do período.</summary>
/// <param name="ChaveDeOrigem">O identificador do fluxo na origem.</param>
/// <param name="Codigo">Código estável.</param>
/// <param name="Nome">Nome legível.</param>
/// <param name="EstaAtivoNaOrigem">Se a origem marca o fluxo como em uso.</param>
/// <param name="Processos">Quantos processos do período usam este fluxo.</param>
public sealed record TipoDeProcessoParaCarga(
    string ChaveDeOrigem, string Codigo, string Nome, bool EstaAtivoNaOrigem, int Processos);

/// <summary>
/// Uma fase, como o dado do período a mostra — e não como o catálogo do fornecedor a declara.
///
/// <para>A fase vem de <b>onde os processos realmente estão</b>. É a diferença entre migrar as
/// 273 fases cadastradas e migrar as que existem: uma fase sem nenhum processo dentro é uma
/// coluna vazia no funil.</para>
/// </summary>
/// <param name="ChaveDoTipoDeProcessoDeOrigem">O fluxo dono da fase.</param>
/// <param name="Codigo">Código estável dentro do fluxo.</param>
/// <param name="Nome">Nome legível, como a origem o escreve.</param>
/// <param name="Ordem">Posição na barra de fases, como a origem a numera.</param>
/// <param name="EhFinal">Se é fase de encerramento.</param>
/// <param name="Processos">Quantos processos do período estão nesta fase.</param>
public sealed record FaseParaCarga(
    string ChaveDoTipoDeProcessoDeOrigem,
    string Codigo,
    string Nome,
    short Ordem,
    bool EhFinal,
    int Processos);

/// <summary>
/// Um tipo de tarefa vindo do catálogo de ações do sistema de origem.
///
/// <para><b>O prazo é zero porque a origem não tem prazo.</b> [V] Nenhuma das ações em uso
/// preenche <c>PrazoRealizacao</c> — o produto tem cinco colunas de prazo no cadastro de ação e
/// todas estão vazias. Zero aqui significa "a origem não declara prazo", e é por isso que a
/// tarefa migrada só ganha prazo limite quando a linha da agenda traz um.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador da ação na origem.</param>
/// <param name="Codigo">Código estável.</param>
/// <param name="Nome">Nome legível.</param>
/// <param name="PrazoDiasUteis">Prazo declarado pela origem. Zero é ausência declarada.</param>
/// <param name="EstaAtivoNaOrigem">Se a origem marca a ação como em uso.</param>
/// <param name="UltimoUsoEm">Quando a ação foi usada pela última vez no período.</param>
/// <param name="Usos">Quantas linhas do período usam esta ação.</param>
public sealed record TipoDeTarefaParaCarga(
    string ChaveDeOrigem,
    string Codigo,
    string Nome,
    short PrazoDiasUteis,
    bool EstaAtivoNaOrigem,
    DateTime? UltimoUsoEm,
    int Usos);

/// <summary>
/// Um desfecho do catálogo de resultados do sistema de origem, com a classe já deduzida.
///
/// <para>A classe sai do que o desfecho FAZ com o processo, declarado na tabela de mapeamento do
/// legado: quem grava um status de cancelamento é cancelamento, quem grava um status de perda é
/// perda, quem aponta fase seguinte é avanço, e o resto é manutenção. [V] 84% dos desfechos
/// mapeados do fluxo de vendas não declaram para onde o processo vai — e é exatamente por isso
/// que existem processos com quarenta andamentos parados na mesma fase.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador do resultado na origem.</param>
/// <param name="ChaveDoTipoDeTarefaDeOrigem">A ação a que o desfecho pertence.</param>
/// <param name="Codigo">Código estável dentro do tipo de tarefa.</param>
/// <param name="Nome">Nome legível.</param>
/// <param name="Classe">O que o desfecho faz com o funil.</param>
/// <param name="ClasseVeioDoMapeamento">Se a classe foi deduzida do mapeamento ou assumida.</param>
/// <param name="UltimoUsoEm">Quando o desfecho foi usado pela última vez no período.</param>
/// <param name="Usos">Quantos lançamentos do período usam este desfecho.</param>
public sealed record ResultadoParaCarga(
    string ChaveDeOrigem,
    string ChaveDoTipoDeTarefaDeOrigem,
    string Codigo,
    string Nome,
    ClasseDeResultado Classe,
    bool ClasseVeioDoMapeamento,
    DateTime? UltimoUsoEm,
    int Usos);

/// <summary>
/// Um processo do sistema de origem, saneado — a oportunidade do funil.
///
/// <para>[V] O legado guarda o processo em DUAS tabelas sem chave estrangeira entre elas: o
/// estado numa, o contexto na outra. A que tem o estado não sabe de quem é o processo nem de que
/// tipo ele é. Daqui para fora as duas já vieram juntas, e o cliente é obrigatório.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O número do processo na origem.</param>
/// <param name="CodigoDaFilialNoLegado">A filial dona.</param>
/// <param name="ChaveDoClienteDeOrigem">A pessoa dona do processo.</param>
/// <param name="ChaveDoTipoDeProcessoDeOrigem">O fluxo.</param>
/// <param name="CodigoDaFase">A fase em que o processo está.</param>
/// <param name="Titulo">O que aparece no cartão do funil.</param>
/// <param name="TituloVeioDaOrigem">Se o título foi lido ou composto.</param>
/// <param name="Descricao">Descrição livre.</param>
/// <param name="Situacao">A situação traduzida para o domínio fechado.</param>
/// <param name="SituacaoNaOrigem">O texto de status como a origem o guarda. É a trilha da tradução.</param>
/// <param name="ValorEstimado">Valor estimado, quando a origem o declara.</param>
/// <param name="Quantidade">Quantidade negociada.</param>
/// <param name="PrevisaoConclusao">Previsão vigente.</param>
/// <param name="PrevisaoConclusaoOriginal">A primeira previsão registrada.</param>
/// <param name="AbertoEm">Quando o processo nasceu na origem.</param>
/// <param name="FaseDesde">Desde quando está nesta fase.</param>
/// <param name="SituacaoDesde">Desde quando está nesta situação.</param>
/// <param name="ConcluidoEm">Quando encerrou, quando encerrou.</param>
/// <param name="LoginDoResponsavelNaOrigem">O login de quem responde pelo processo.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record ProcessoParaCarga(
    string ChaveDeOrigem,
    int CodigoDaFilialNoLegado,
    string ChaveDoClienteDeOrigem,
    string ChaveDoTipoDeProcessoDeOrigem,
    string CodigoDaFase,
    string Titulo,
    bool TituloVeioDaOrigem,
    string? Descricao,
    SituacaoDoProcesso Situacao,
    string SituacaoNaOrigem,
    decimal? ValorEstimado,
    decimal? Quantidade,
    DateOnly? PrevisaoConclusao,
    DateOnly? PrevisaoConclusaoOriginal,
    DateTime AbertoEm,
    DateTime FaseDesde,
    DateTime SituacaoDesde,
    DateTime? ConcluidoEm,
    string? LoginDoResponsavelNaOrigem,
    DateTime? AtualizadoEm,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// Uma tarefa da agenda do sistema de origem, saneada.
///
/// <para>[V] A agenda do legado guarda o vendedor como LOGIN num campo de texto, não como
/// identificador. Aqui o responsável já vem resolvido para a chave do usuário — e a linha cujo
/// dono não existe no cadastro é recusada em vez de entrar órfã.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador da agenda na origem.</param>
/// <param name="CodigoDaFilialNoLegado">A filial dona.</param>
/// <param name="ChaveDoClienteDeOrigem">A pessoa, quando há.</param>
/// <param name="ChaveDoProcessoDeOrigem">O processo, quando há.</param>
/// <param name="ChaveDoTipoDeTarefaDeOrigem">A ação. Vazia quando a origem não declara nenhuma.</param>
/// <param name="Assunto">O que aparece na agenda.</param>
/// <param name="Detalhe">Detalhe livre.</param>
/// <param name="ChaveDoResponsavelDeOrigem">Quem tem que fazer.</param>
/// <param name="AgendadaPara">Quando está agendada.</param>
/// <param name="PrazoLimite">Prazo limite, só quando a origem declara um.</param>
/// <param name="Prioridade">Prioridade de 1 a 5.</param>
/// <param name="Situacao">A situação traduzida para o domínio fechado.</param>
/// <param name="ConcluidaEm">Quando foi concluída.</param>
/// <param name="ChaveDeQuemConcluiuDeOrigem">Quem concluiu.</param>
/// <param name="ChaveDoResultadoDeOrigem">O desfecho registrado na conclusão.</param>
/// <param name="ChaveDaInteracaoDeConclusaoDeOrigem">O andamento que a concluiu.</param>
/// <param name="ChaveDaInteracaoDeOrigemDeOrigem">O andamento que a gerou.</param>
/// <param name="OrigemAtribuicao">Por que a tarefa é dessa pessoa.</param>
/// <param name="CriadaEm">Quando a tarefa nasceu na origem.</param>
/// <param name="AtualizadoEm">Última alteração conhecida na origem.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record TarefaParaCarga(
    string ChaveDeOrigem,
    int CodigoDaFilialNoLegado,
    string? ChaveDoClienteDeOrigem,
    string? ChaveDoProcessoDeOrigem,
    string ChaveDoTipoDeTarefaDeOrigem,
    string Assunto,
    string? Detalhe,
    string ChaveDoResponsavelDeOrigem,
    DateTime AgendadaPara,
    DateTime? PrazoLimite,
    short Prioridade,
    SituacaoDaTarefa Situacao,
    DateTime? ConcluidaEm,
    string? ChaveDeQuemConcluiuDeOrigem,
    string? ChaveDoResultadoDeOrigem,
    string? ChaveDaInteracaoDeConclusaoDeOrigem,
    string? ChaveDaInteracaoDeOrigemDeOrigem,
    OrigemDaAtribuicao OrigemAtribuicao,
    DateTime CriadaEm,
    DateTime? AtualizadoEm,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);

/// <summary>
/// Uma interação do histórico do sistema de origem, saneada — o fato que aconteceu.
///
/// <para>A natureza separa três coisas que o legado mistura na mesma linha do tempo: nós
/// procuramos o cliente, o cliente nos procurou, e o servidor carimbou um evento. [V] Um quarto
/// do "histórico de relacionamento" de 2026 foi escrito por um processo automático, e a tela do
/// legado mostra tudo junto sem distinção nenhuma.</para>
/// </summary>
/// <param name="ChaveDeOrigem">O identificador do histórico na origem.</param>
/// <param name="CodigoDaFilialNoLegado">A filial dona.</param>
/// <param name="ChaveDoClienteDeOrigem">A pessoa.</param>
/// <param name="ChaveDoProcessoDeOrigem">O processo, quando há.</param>
/// <param name="ChaveDoTipoDeTarefaDeOrigem">A ação geradora. Vazia quando a origem não declara.</param>
/// <param name="ChaveDoResultadoDeOrigem">O desfecho registrado.</param>
/// <param name="ChaveDaTarefaDeOrigem">A agenda que este andamento concluiu.</param>
/// <param name="Assunto">Assunto, composto do desfecho quando a origem não tem um.</param>
/// <param name="Detalhe">O relato do que aconteceu.</param>
/// <param name="ResultadoComplemento">Complemento do desfecho.</param>
/// <param name="Natureza">Quem procurou quem.</param>
/// <param name="OcorridaEm">Quando o contato aconteceu.</param>
/// <param name="DuracaoMinutos">Duração, quando a origem a registra.</param>
/// <param name="Latitude">Latitude do atendimento em campo.</param>
/// <param name="Longitude">Longitude do atendimento em campo.</param>
/// <param name="ChaveDoAutorDeOrigem">Quem registrou.</param>
/// <param name="Correcoes">O que foi normalizado nesta linha.</param>
/// <param name="Rejeicoes">Os campos que não passaram.</param>
public sealed record InteracaoParaCarga(
    string ChaveDeOrigem,
    int CodigoDaFilialNoLegado,
    string? ChaveDoClienteDeOrigem,
    string? ChaveDoProcessoDeOrigem,
    string ChaveDoTipoDeTarefaDeOrigem,
    string? ChaveDoResultadoDeOrigem,
    string? ChaveDaTarefaDeOrigem,
    string Assunto,
    string? Detalhe,
    string? ResultadoComplemento,
    NaturezaDaInteracao Natureza,
    DateTime OcorridaEm,
    int? DuracaoMinutos,
    decimal? Latitude,
    decimal? Longitude,
    string ChaveDoAutorDeOrigem,
    IReadOnlyList<CorrecaoAplicada> Correcoes,
    IReadOnlyList<CampoRejeitado> Rejeicoes);
