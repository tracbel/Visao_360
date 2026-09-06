using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>
/// UMA MÉTRICA QUE A TELA PEDE E O DADO NÃO SUSTENTA — devolvida vazia, com o motivo.
///
/// <para>É a peça mais importante deste contrato, e ela existe por causa de um defeito medido no
/// sistema que estamos substituindo: lá, 17 meses de faturamento parado passaram por atual
/// porque a tela mostrava um número sem dizer de onde ele vinha. Uma tela que diz "sem dado"
/// está certa; uma tela que mostra um número construído sobre 0,8% de preenchimento está errada
/// — e não tem como o usuário perceber.</para>
///
/// <para>O motivo é medido, não escrito à mão: ele traz a contagem real que justifica a
/// ausência, apurada na mesma consulta que produziu o agregado.</para>
/// </summary>
/// <param name="Metrica">O nome da métrica, como a tela a chama.</param>
/// <param name="Motivo">Por que ela vem vazia, com o número que sustenta a afirmação.</param>
public sealed record MetricaSemDado(string Metrica, string Motivo);

/// <summary>Um agregado, junto da lista do que a tela pediu e não pôde ser respondido.</summary>
/// <typeparam name="T">O que o agregado devolve.</typeparam>
/// <param name="Itens">As linhas do agregado.</param>
/// <param name="MetricasSemDado">O que a tela pede e o dado não sustenta.</param>
public sealed record Agregado<T>(IReadOnlyList<T> Itens, IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>O processo como a listagem e o cartão do funil o mostram.</summary>
/// <param name="Chave">O GUID público do processo.</param>
/// <param name="Numero">O número legível — é o que o usuário fala ao telefone.</param>
/// <param name="Titulo">O que aparece no cartão.</param>
/// <param name="ClienteChave">O GUID público do cliente.</param>
/// <param name="ClienteNome">A razão social do cliente.</param>
/// <param name="TipoProcessoCodigo">O código do modelo de fluxo.</param>
/// <param name="TipoProcessoNome">O nome do modelo de fluxo.</param>
/// <param name="FaseCodigo">O código da fase atual.</param>
/// <param name="FaseNome">O nome da fase atual.</param>
/// <param name="FaseOrdem">A posição da fase na barra.</param>
/// <param name="FaseDesde">Desde quando o processo está nesta fase (UTC).</param>
/// <param name="DiasNaFase">Há quantos dias o processo está parado nesta fase.</param>
/// <param name="Situacao">Onde o processo está no funil.</param>
/// <param name="ValorEstimado">Valor estimado. Nulo quando a origem não declara.</param>
/// <param name="PrevisaoConclusao">Previsão de conclusão vigente.</param>
/// <param name="ProprietarioNome">Quem responde pelo processo.</param>
/// <param name="CriadoEm">Quando o processo foi aberto (UTC).</param>
public sealed record ProcessoResumo(
    Guid Chave,
    long Numero,
    string Titulo,
    Guid ClienteChave,
    string ClienteNome,
    string TipoProcessoCodigo,
    string TipoProcessoNome,
    string FaseCodigo,
    string FaseNome,
    short FaseOrdem,
    DateTime FaseDesde,
    int DiasNaFase,
    string Situacao,
    decimal? ValorEstimado,
    DateOnly? PrevisaoConclusao,
    string? ProprietarioNome,
    DateTime CriadoEm)
{
    /// <summary>Traduz a leitura para o cartão do funil.</summary>
    /// <param name="leitura">O processo com o contexto já resolvido.</param>
    /// <param name="agoraUtc">O instante de referência para o tempo de fase.</param>
    public static ProcessoResumo De(ProcessoComContexto leitura, DateTime agoraUtc)
    {
        var p = leitura.Processo;

        return new ProcessoResumo(
            p.ChavePublica,
            p.Numero,
            p.Titulo,
            leitura.ClienteChave,
            leitura.ClienteNome,
            leitura.TipoProcessoCodigo,
            leitura.TipoProcessoNome,
            leitura.FaseCodigo,
            leitura.FaseNome,
            leitura.FaseOrdem,
            p.FaseDesde,
            (int)Math.Max(0, (agoraUtc - p.FaseDesde).TotalDays),
            p.Situacao.ToString(),
            p.ValorEstimado?.Valor,
            p.PrevisaoConclusao,
            leitura.ProprietarioNome,
            p.CriadoEm);
    }
}

/// <summary>O processo como a ficha da oportunidade o mostra.</summary>
/// <param name="Resumo">Tudo o que o cartão já mostra.</param>
/// <param name="Descricao">Descrição livre.</param>
/// <param name="Quantidade">Quantidade negociada.</param>
/// <param name="ValorFinal">Valor efetivamente fechado.</param>
/// <param name="PrevisaoConclusaoOriginal">A primeira previsão registrada — é o que mede derrapagem.</param>
/// <param name="SituacaoDesde">Desde quando está nesta situação (UTC).</param>
/// <param name="ConcluidoEm">Quando encerrou (UTC).</param>
/// <param name="MotivoDePerdaCodigo">Por que não fechou.</param>
/// <param name="ObservacaoDaPerda">O texto que explica a perda.</param>
/// <param name="Versao">O carimbo de concorrência, em base64.</param>
public sealed record ProcessoDetalhe(
    ProcessoResumo Resumo,
    string? Descricao,
    decimal? Quantidade,
    decimal? ValorFinal,
    DateOnly? PrevisaoConclusaoOriginal,
    DateTime SituacaoDesde,
    DateTime? ConcluidoEm,
    string? MotivoDePerdaCodigo,
    string? ObservacaoDaPerda,
    string? Versao)
{
    /// <summary>Traduz a leitura para a ficha.</summary>
    /// <param name="leitura">O processo com o contexto já resolvido.</param>
    /// <param name="agoraUtc">O instante de referência para o tempo de fase.</param>
    public static ProcessoDetalhe De(ProcessoComContexto leitura, DateTime agoraUtc)
    {
        var p = leitura.Processo;

        return new ProcessoDetalhe(
            ProcessoResumo.De(leitura, agoraUtc),
            p.Descricao,
            p.Quantidade,
            p.ValorFinal?.Valor,
            p.PrevisaoConclusaoOriginal,
            p.SituacaoDesde,
            p.ConcluidoEm,
            leitura.MotivoDePerdaCodigo,
            p.ObservacaoDaPerda,
            p.Versao is null ? null : Convert.ToBase64String(p.Versao));
    }
}

/// <summary>Uma coluna do funil, como a tela de Pipeline a mostra.</summary>
/// <param name="TipoProcessoCodigo">O fluxo.</param>
/// <param name="TipoProcessoNome">O nome do fluxo.</param>
/// <param name="FaseCodigo">A fase.</param>
/// <param name="FaseNome">O nome da fase.</param>
/// <param name="FaseOrdem">A posição da fase na barra.</param>
/// <param name="Processos">Quantos processos abertos estão nesta fase.</param>
/// <param name="ProcessosComValor">Quantos deles declaram valor. É a medida da confiança do total.</param>
/// <param name="ValorTotal">A soma dos que declaram valor. Nulo quando nenhum declara.</param>
public sealed record FaseDoFunil(
    string TipoProcessoCodigo,
    string TipoProcessoNome,
    string FaseCodigo,
    string FaseNome,
    short FaseOrdem,
    int Processos,
    int ProcessosComValor,
    decimal? ValorTotal);

/// <summary>A tarefa como a agenda a mostra.</summary>
/// <param name="Chave">O GUID público da tarefa.</param>
/// <param name="Assunto">O que aparece na agenda.</param>
/// <param name="Detalhe">Detalhe livre.</param>
/// <param name="TipoTarefaCodigo">O código do tipo de tarefa.</param>
/// <param name="TipoTarefaNome">O nome do tipo de tarefa.</param>
/// <param name="ClienteChave">O GUID público do cliente.</param>
/// <param name="ClienteNome">A razão social do cliente.</param>
/// <param name="ProcessoChave">O GUID público do processo.</param>
/// <param name="ProcessoTitulo">O título do processo.</param>
/// <param name="ResponsavelNome">Quem tem que fazer.</param>
/// <param name="AgendadaPara">Quando está agendada (UTC).</param>
/// <param name="PrazoLimite">Prazo limite (UTC). Nulo quando a origem não declara prazo.</param>
/// <param name="Prioridade">De 1 (alta) a 5 (baixa).</param>
/// <param name="Situacao">Em que estado a tarefa está.</param>
/// <param name="EstaAtrasada">A data já passou e a tarefa não foi concluída.</param>
/// <param name="DiasDeAtraso">Há quantos dias ela está atrasada. Zero quando está em dia.</param>
/// <param name="ConcluidaEm">Quando foi concluída (UTC).</param>
/// <param name="ResultadoNome">O desfecho registrado na conclusão.</param>
public sealed record TarefaResumo(
    Guid Chave,
    string Assunto,
    string? Detalhe,
    string TipoTarefaCodigo,
    string TipoTarefaNome,
    Guid? ClienteChave,
    string? ClienteNome,
    Guid? ProcessoChave,
    string? ProcessoTitulo,
    string ResponsavelNome,
    DateTime AgendadaPara,
    DateTime? PrazoLimite,
    short Prioridade,
    string Situacao,
    bool EstaAtrasada,
    int DiasDeAtraso,
    DateTime? ConcluidaEm,
    string? ResultadoNome)
{
    /// <summary>Traduz a leitura para a linha da agenda.</summary>
    /// <param name="leitura">A tarefa com o contexto já resolvido.</param>
    /// <param name="agoraUtc">O instante de referência para o atraso.</param>
    public static TarefaResumo De(TarefaComContexto leitura, DateTime agoraUtc)
    {
        var t = leitura.Tarefa;

        // O ATRASO É MEDIDO CONTRA A DATA AGENDADA, e não contra o prazo limite. O prazo limite
        // existe em menos de um quinto das tarefas migradas porque o sistema de origem não
        // declara prazo em nenhuma das ações em uso; medir por ele faria a maior parte da agenda
        // parecer em dia por falta de dado, que é o pior tipo de indicador verde.
        var pendente = t.Situacao is Dominio.Processo.SituacaoDaTarefa.Pendente
            or Dominio.Processo.SituacaoDaTarefa.EmAndamento;

        var atrasada = pendente && t.AgendadaPara < agoraUtc;

        return new TarefaResumo(
            t.ChavePublica,
            t.Assunto,
            t.Detalhe,
            leitura.TipoTarefaCodigo,
            leitura.TipoTarefaNome,
            leitura.ClienteChave,
            leitura.ClienteNome,
            leitura.ProcessoChave,
            leitura.ProcessoTitulo,
            leitura.ResponsavelNome,
            t.AgendadaPara,
            t.PrazoLimite,
            t.Prioridade,
            t.Situacao.ToString(),
            atrasada,
            atrasada ? (int)Math.Max(0, (agoraUtc - t.AgendadaPara).TotalDays) : 0,
            t.ConcluidaEm,
            leitura.ResultadoNome);
    }
}

/// <summary>A interação como a linha do tempo a mostra.</summary>
/// <param name="Chave">O GUID público da interação.</param>
/// <param name="Assunto">Assunto.</param>
/// <param name="Detalhe">O relato do que aconteceu.</param>
/// <param name="TipoTarefaNome">Que tipo de contato foi este.</param>
/// <param name="ResultadoNome">O desfecho registrado.</param>
/// <param name="ResultadoComplemento">O complemento do desfecho.</param>
/// <param name="Natureza">Quem procurou quem: Ativa, Receptiva ou Sistema.</param>
/// <param name="OcorridaEm">Quando o contato aconteceu (UTC).</param>
/// <param name="DuracaoMinutos">Duração. Nulo quando a origem não registra.</param>
/// <param name="Latitude">Latitude do atendimento em campo.</param>
/// <param name="Longitude">Longitude do atendimento em campo.</param>
/// <param name="AutorNome">Quem registrou.</param>
/// <param name="ClienteChave">O GUID público do cliente.</param>
/// <param name="ClienteNome">A razão social do cliente.</param>
/// <param name="ProcessoChave">O GUID público do processo.</param>
public sealed record InteracaoResumo(
    Guid Chave,
    string Assunto,
    string? Detalhe,
    string TipoTarefaNome,
    string? ResultadoNome,
    string? ResultadoComplemento,
    string Natureza,
    DateTime OcorridaEm,
    int? DuracaoMinutos,
    decimal? Latitude,
    decimal? Longitude,
    string AutorNome,
    Guid? ClienteChave,
    string? ClienteNome,
    Guid? ProcessoChave)
{
    /// <summary>Traduz a leitura para a linha do tempo.</summary>
    /// <param name="leitura">A interação com o contexto já resolvido.</param>
    public static InteracaoResumo De(InteracaoComContexto leitura)
    {
        var i = leitura.Interacao;

        return new InteracaoResumo(
            i.ChavePublica,
            i.Assunto,
            i.Detalhe,
            leitura.TipoTarefaNome,
            leitura.ResultadoNome,
            i.ResultadoComplemento,
            i.Natureza.ToString(),
            i.OcorridaEm.Valor,
            i.DuracaoMinutos,
            i.Latitude,
            i.Longitude,
            leitura.AutorNome,
            leitura.ClienteChave,
            leitura.ClienteNome,
            leitura.ProcessoChave);
    }
}

/// <summary>Uma linha da tela de Cobertura.</summary>
/// <param name="ClienteChave">O GUID público do cliente.</param>
/// <param name="ClienteNome">A razão social.</param>
/// <param name="CarteiraChave">O GUID público da carteira.</param>
/// <param name="CarteiraNome">O nome da carteira.</param>
/// <param name="LinhaDeNegocioNome">A linha de negócio da carteira.</param>
/// <param name="Classe">A classe do cliente nesta carteira.</param>
/// <param name="UltimaInteracaoEm">Quando foi o último contato (UTC). Nulo é "nunca".</param>
/// <param name="DiasSemContato">Há quantos dias ninguém fala com ele. Nulo quando nunca houve contato.</param>
/// <param name="DiasCicloContato">A cadência esperada, quando declarada.</param>
/// <param name="EstaForaDoCiclo">Passou da cadência esperada. Nulo quando não há cadência declarada.</param>
/// <param name="ResponsavelNome">O CEN responsável pela carteira.</param>
public sealed record CoberturaResumo(
    Guid ClienteChave,
    string ClienteNome,
    Guid CarteiraChave,
    string CarteiraNome,
    string LinhaDeNegocioNome,
    string Classe,
    DateTime? UltimaInteracaoEm,
    int? DiasSemContato,
    short? DiasCicloContato,
    bool? EstaForaDoCiclo,
    string ResponsavelNome)
{
    /// <summary>Traduz a leitura para a linha da Cobertura.</summary>
    /// <param name="leitura">A linha de cobertura.</param>
    /// <param name="agoraUtc">O instante de referência.</param>
    public static CoberturaResumo De(LinhaDeCobertura leitura, DateTime agoraUtc)
    {
        var dias = leitura.UltimaInteracaoEm is { } ultima
            ? (int)Math.Max(0, (agoraUtc - ultima).TotalDays)
            : (int?)null;

        return new CoberturaResumo(
            leitura.ClienteChave,
            leitura.ClienteNome,
            leitura.CarteiraChave,
            leitura.CarteiraNome,
            leitura.LinhaDeNegocioNome,
            leitura.Classe.ToString(),
            leitura.UltimaInteracaoEm,
            dias,
            leitura.DiasCicloContato,

            // FORA DO CICLO É NULO, NÃO FALSO, quando não há cadência declarada. Falso diria
            // "está em dia", e não é isso que se sabe: não se sabe nada.
            leitura.DiasCicloContato is { } ciclo ? dias is null || dias > ciclo : null,
            leitura.ResponsavelNome);
    }
}
