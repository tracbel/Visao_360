using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>
/// Lista processos — o que sustenta o Pipeline, o Funil e a aba de oportunidades da Visão 360.
///
/// <para>O QUE ESTE CASO DE USO NÃO FAZ, e vale repetir: filtrar por empresa. A fronteira é
/// aplicada pelo filtro global do contexto de persistência, e não existe parâmetro de empresa
/// nesta consulta — nem para quem quiser passar um.</para>
/// </summary>
public sealed class ListarProcessos(
    IRepositorioProcessos repositorio, IRepositorioClientes clientes, IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="pagina">Página pedida, começando em 1.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="termo">Busca por título ou número.</param>
    /// <param name="situacao">Filtro por situação. Domínio fechado.</param>
    /// <param name="clienteChave">Filtro pelo cliente. É o que a Visão 360 usa.</param>
    /// <param name="faseCodigo">Filtro pela fase — a coluna do kanban.</param>
    /// <param name="tipoProcessoCodigo">Filtro pelo modelo de fluxo.</param>
    /// <param name="ordenarPor">Coluna de ordenação. Domínio fechado.</param>
    /// <param name="descendente">Ordem decrescente.</param>
    /// <param name="incluirEncerrados">Trazer também os processos já encerrados.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<ProcessoResumo>>>> ExecutarAsync(
        int? pagina,
        int? tamanho,
        string? termo,
        string? situacao,
        Guid? clienteChave,
        string? faseCodigo,
        string? tipoProcessoCodigo,
        string? ordenarPor,
        bool descendente,
        bool incluirEncerrados,
        CancellationToken ct)
    {
        var erros = new ColetorDeErros();

        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<ProcessoResumo>>>.FalhaDeValidacao(
                paginacao.Erro!, paginacao.Erros);

        var situacaoEscolhida = situacao is null
            ? null
            : erros.ItemDeDominio<SituacaoDoProcesso>("situacao", situacao);

        var ordem = erros.ItemDeDominioOuPadrao("ordenarPor", ordenarPor, OrdemDeProcesso.FaseDesde);

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<ProcessoResumo>>>(
                "A consulta tem parâmetros que não valem.");

        long? clienteId = null;

        if (clienteChave is { } chave)
        {
            var cliente = await clientes.ObterAsync(chave, incluirInativos: true, ct);

            // NÃO EXISTE E NÃO É SEU dão a mesma resposta, aqui como no cadastro. Devolver lista
            // vazia contaria a quem não pode ver que o cliente existe noutra filial.
            if (cliente is null)
                return Resultado<ComProcedencia<PaginaDe<ProcessoResumo>>>.NaoEncontrado(
                    $"Não há cliente {chave} ao seu alcance.");

            clienteId = cliente.Cliente.Id;
        }

        var pagi = await repositorio.ListarAsync(
            new ConsultaDeProcessos(
                paginacao.Valor,
                Termo: string.IsNullOrWhiteSpace(termo) ? null : termo.Trim(),
                Situacao: situacaoEscolhida,
                ClienteId: clienteId,
                Ordem: ordem,
                Descendente: descendente,
                IncluirEncerrados: incluirEncerrados),
            ct);

        var agora = relogio.Agora;

        // O FILTRO POR CÓDIGO DE FASE E DE FLUXO ACONTECE AQUI, e não no repositório, porque o
        // que a API expõe é o CÓDIGO estável do catálogo e não o identificador interno — mesma
        // regra do cadastro. Ele é aplicado sobre a página já paginada pelo banco apenas quando
        // vem preenchido, e a tela de kanban usa a rota de funil, que agrupa no banco.
        var itens = pagi.Itens
            .Where(p => faseCodigo is null
                        || string.Equals(p.FaseCodigo, faseCodigo, StringComparison.OrdinalIgnoreCase))
            .Where(p => tipoProcessoCodigo is null
                        || string.Equals(p.TipoProcessoCodigo, tipoProcessoCodigo,
                            StringComparison.OrdinalIgnoreCase))
            .Select(p => ProcessoResumo.De(p, agora))
            .ToList();

        var resumo = new PaginaDe<ProcessoResumo>(
            itens, pagi.Pagina, pagi.Tamanho, pagi.Total);

        return Resultado<ComProcedencia<PaginaDe<ProcessoResumo>>>.Ok(
            ComProcedencia<PaginaDe<ProcessoResumo>>.DoNossoBanco(resumo, "processo.Processo", relogio));
    }
}

/// <summary>Traz a ficha de um processo pela chave pública.</summary>
public sealed class ObterProcesso(IRepositorioProcessos repositorio, IRelogio relogio)
{
    /// <summary>Executa a consulta.</summary>
    /// <param name="chave">O GUID público do processo.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<ProcessoDetalhe>>> ExecutarAsync(
        Guid chave, CancellationToken ct)
    {
        var processo = await repositorio.ObterAsync(chave, ct);

        if (processo is null)
            return Resultado<ComProcedencia<ProcessoDetalhe>>.NaoEncontrado(
                $"Não há processo {chave} ao seu alcance.");

        return Resultado<ComProcedencia<ProcessoDetalhe>>.Ok(
            ComProcedencia<ProcessoDetalhe>.DoNossoBanco(
                ProcessoDetalhe.De(processo, relogio.Agora), "processo.Processo", relogio));
    }
}

/// <summary>
/// O funil por fase, contado no banco — e a declaração do que ele NÃO consegue responder.
///
/// <para><b>É aqui que a regra do valor mora.</b> O valor do negócio é preenchido em menos de 1%
/// dos processos migrados; somar essa coluna e chamar o resultado de "valor do funil" seria
/// mostrar um número que representa uma fração da realidade sem dizer isso. O agregado devolve o
/// total só quando ele existe, devolve <b>quantos processos o sustentam</b> sempre, e devolve a
/// métrica ausente com o motivo medido.</para>
/// </summary>
public sealed class ObterFunil(IRepositorioProcessos repositorio, IRelogio relogio)
{
    /// <summary>Executa o agregado.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<Agregado<FaseDoFunil>>>> ExecutarAsync(CancellationToken ct)
    {
        var fatias = await repositorio.ResumirFunilAsync(tipoProcessoId: null, ct);

        var processos = fatias.Sum(f => f.Processos);
        var comValor = fatias.Sum(f => f.ProcessosComValor);

        var itens = fatias
            .Select(f => new FaseDoFunil(
                f.TipoProcessoCodigo, f.TipoProcessoNome, f.FaseCodigo, f.FaseNome, f.FaseOrdem,
                f.Processos, f.ProcessosComValor, f.ValorTotal))
            .ToList();

        var ausentes = new List<MetricaSemDado>();

        if (comValor == 0)
            ausentes.Add(new MetricaSemDado(
                "valorDoFunil",
                $"Nenhum dos {processos} processos abertos declara valor. O sistema de origem tem a " +
                "coluna e praticamente não a preenche; somar zeros e apresentar o total como valor " +
                "do funil mostraria um número que não representa negócio nenhum."));
        else if (processos > 0 && comValor * 100 / processos < 10)
            ausentes.Add(new MetricaSemDado(
                "valorDoFunilConfiavel",
                $"Só {comValor} de {processos} processos abertos declaram valor " +
                $"({comValor * 100.0 / processos:F1}%). O total vem preenchido, mas ele representa " +
                "essa fração — não o funil inteiro."));

        if (processos == 0)
            ausentes.Add(new MetricaSemDado(
                "funil",
                "Não há processo aberto ao alcance deste contexto de acesso."));

        return Resultado<ComProcedencia<Agregado<FaseDoFunil>>>.Ok(
            ComProcedencia<Agregado<FaseDoFunil>>.DoNossoBanco(
                new Agregado<FaseDoFunil>(itens, ausentes), "processo.Processo", relogio));
    }
}

/// <summary>As perdas por motivo, contadas no banco.</summary>
public sealed class ObterPerdas(IRepositorioProcessos repositorio, IRelogio relogio)
{
    /// <summary>Executa o agregado.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<Agregado<ContagemPorRotulo>>>> ExecutarAsync(
        CancellationToken ct)
    {
        var perdas = await repositorio.ResumirPerdasAsync(ct);

        var total = perdas.Sum(p => p.Quantidade);

        var semMotivoReal = perdas
            .Where(p => p.Codigo is "SEM_MOTIVO" or "NAO_INFORMADO_NA_ORIGEM")
            .Sum(p => p.Quantidade);

        var ausentes = new List<MetricaSemDado>();

        if (total == 0)
            ausentes.Add(new MetricaSemDado(
                "perdasPorMotivo",
                "Não há processo perdido ao alcance deste contexto de acesso."));
        else if (semMotivoReal == total)
            ausentes.Add(new MetricaSemDado(
                "perdasPorMotivo",
                $"Os {total} processos perdidos não têm motivo declarado na origem. O sistema " +
                "legado encerra o processo sem exigir motivo, e por isso não existe distribuição " +
                "de perda para mostrar — só o total."));
        else if (semMotivoReal > 0)
            ausentes.Add(new MetricaSemDado(
                "perdasPorMotivoCompleto",
                $"{semMotivoReal} de {total} processos perdidos não têm motivo declarado na origem. " +
                "A distribuição vem preenchida, mas essa fatia não é um motivo — é a ausência dele."));

        return Resultado<ComProcedencia<Agregado<ContagemPorRotulo>>>.Ok(
            ComProcedencia<Agregado<ContagemPorRotulo>>.DoNossoBanco(
                new Agregado<ContagemPorRotulo>(perdas, ausentes), "processo.Processo", relogio));
    }
}

/// <summary>
/// Lista tarefas — o que sustenta a Agenda do CEN.
/// </summary>
public sealed class ListarTarefas(
    IRepositorioTarefas repositorio,
    IRepositorioClientes clientes,
    IProvedorContextoAcesso contexto,
    IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="pagina">Página pedida.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="minhas">Só as tarefas de quem está pedindo.</param>
    /// <param name="situacao">Filtro por situação. Domínio fechado.</param>
    /// <param name="clienteChave">Filtro pelo cliente.</param>
    /// <param name="de">Data mínima de agendamento.</param>
    /// <param name="ate">Data máxima de agendamento.</param>
    /// <param name="somenteAtrasadas">Só o que já passou da data e não foi concluído.</param>
    /// <param name="ordenarPor">Coluna de ordenação. Domínio fechado.</param>
    /// <param name="descendente">Ordem decrescente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<TarefaResumo>>>> ExecutarAsync(
        int? pagina,
        int? tamanho,
        bool minhas,
        string? situacao,
        Guid? clienteChave,
        DateOnly? de,
        DateOnly? ate,
        bool somenteAtrasadas,
        string? ordenarPor,
        bool descendente,
        CancellationToken ct)
    {
        var erros = new ColetorDeErros();

        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<TarefaResumo>>>.FalhaDeValidacao(
                paginacao.Erro!, paginacao.Erros);

        var situacaoEscolhida = situacao is null
            ? null
            : erros.ItemDeDominio<SituacaoDaTarefa>("situacao", situacao);

        var ordem = erros.ItemDeDominioOuPadrao("ordenarPor", ordenarPor, OrdemDeTarefa.AgendadaPara);

        if (de is { } inicio && ate is { } fim && fim < inicio)
            erros.Registrar("ate", "A data final não pode ser anterior à inicial.", ate.ToString());

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<TarefaResumo>>>(
                "A consulta tem parâmetros que não valem.");

        long? clienteId = null;

        if (clienteChave is { } chave)
        {
            var cliente = await clientes.ObterAsync(chave, incluirInativos: true, ct);

            if (cliente is null)
                return Resultado<ComProcedencia<PaginaDe<TarefaResumo>>>.NaoEncontrado(
                    $"Não há cliente {chave} ao seu alcance.");

            clienteId = cliente.Cliente.Id;
        }

        var pagi = await repositorio.ListarAsync(
            new ConsultaDeTarefas(
                paginacao.Valor,
                ResponsavelId: minhas ? contexto.Atual.UsuarioId : null,
                Situacao: situacaoEscolhida,
                ClienteId: clienteId,
                De: de,
                Ate: ate,
                SomenteAtrasadas: somenteAtrasadas,
                Ordem: ordem,
                Descendente: descendente),
            ct);

        var agora = relogio.Agora;

        var resumo = new PaginaDe<TarefaResumo>(
            [.. pagi.Itens.Select(t => TarefaResumo.De(t, agora))], pagi.Pagina, pagi.Tamanho, pagi.Total);

        return Resultado<ComProcedencia<PaginaDe<TarefaResumo>>>.Ok(
            ComProcedencia<PaginaDe<TarefaResumo>>.DoNossoBanco(resumo, "processo.Tarefa", relogio));
    }
}

/// <summary>O painel do CEN — os números da agenda, contados no banco.</summary>
public sealed class ObterPainelDaAgenda(
    IRepositorioTarefas repositorio, IProvedorContextoAcesso contexto, IRelogio relogio)
{
    /// <summary>Executa o agregado.</summary>
    /// <param name="minhas">Só a agenda de quem está pedindo; falso conta a filial inteira.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<Agregado<PainelDaAgenda>>>> ExecutarAsync(
        bool minhas, CancellationToken ct)
    {
        var agora = relogio.Agora;

        var painel = await repositorio.ResumirAgendaAsync(
            minhas ? contexto.Atual.UsuarioId : null, agora, ct);

        var ausentes = new List<MetricaSemDado>();

        if (painel.Pendentes == 0 && painel.ConcluidasNosUltimosTrintaDias == 0)
            ausentes.Add(new MetricaSemDado(
                "agenda", "Não há tarefa ao alcance deste contexto de acesso."));

        if (painel.Pendentes > 0 && painel.SemPrazoLimite == painel.Pendentes)
            ausentes.Add(new MetricaSemDado(
                "cumprimentoDePrazo",
                $"Nenhuma das {painel.Pendentes} tarefas pendentes tem prazo limite declarado. O " +
                "sistema de origem não preenche prazo em nenhuma das ações em uso, então não " +
                "existe prazo contra o qual medir cumprimento — só a data que alguém escolheu."));
        else if (painel.SemPrazoLimite > 0)
            ausentes.Add(new MetricaSemDado(
                "cumprimentoDePrazoCompleto",
                $"{painel.SemPrazoLimite} de {painel.Pendentes} tarefas pendentes não têm prazo " +
                "limite declarado. O indicador cobre só o restante."));

        return Resultado<ComProcedencia<Agregado<PainelDaAgenda>>>.Ok(
            ComProcedencia<Agregado<PainelDaAgenda>>.DoNossoBanco(
                new Agregado<PainelDaAgenda>([painel], ausentes), "processo.Tarefa", relogio));
    }
}

/// <summary>
/// Lista interações — o que sustenta a linha do tempo da Visão 360.
/// </summary>
public sealed class ListarInteracoes(
    IRepositorioInteracoes repositorio, IRepositorioClientes clientes, IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="pagina">Página pedida.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="clienteChave">Filtro pelo cliente. É o caminho normal de uso.</param>
    /// <param name="natureza">Filtro por quem procurou quem. Domínio fechado.</param>
    /// <param name="de">Data mínima de ocorrência.</param>
    /// <param name="ate">Data máxima de ocorrência.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<InteracaoResumo>>>> ExecutarAsync(
        int? pagina,
        int? tamanho,
        Guid? clienteChave,
        string? natureza,
        DateOnly? de,
        DateOnly? ate,
        CancellationToken ct)
    {
        var erros = new ColetorDeErros();

        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<InteracaoResumo>>>.FalhaDeValidacao(
                paginacao.Erro!, paginacao.Erros);

        var naturezaEscolhida = natureza is null
            ? null
            : erros.ItemDeDominio<NaturezaDaInteracao>("natureza", natureza);

        if (de is { } inicio && ate is { } fim && fim < inicio)
            erros.Registrar("ate", "A data final não pode ser anterior à inicial.", ate.ToString());

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<InteracaoResumo>>>(
                "A consulta tem parâmetros que não valem.");

        long? clienteId = null;

        if (clienteChave is { } chave)
        {
            var cliente = await clientes.ObterAsync(chave, incluirInativos: true, ct);

            if (cliente is null)
                return Resultado<ComProcedencia<PaginaDe<InteracaoResumo>>>.NaoEncontrado(
                    $"Não há cliente {chave} ao seu alcance.");

            clienteId = cliente.Cliente.Id;
        }

        var pagi = await repositorio.ListarAsync(
            new ConsultaDeInteracoes(
                paginacao.Valor,
                ClienteId: clienteId,
                Natureza: naturezaEscolhida,
                De: de,
                Ate: ate),
            ct);

        var resumo = new PaginaDe<InteracaoResumo>(
            [.. pagi.Itens.Select(InteracaoResumo.De)], pagi.Pagina, pagi.Tamanho, pagi.Total);

        return Resultado<ComProcedencia<PaginaDe<InteracaoResumo>>>.Ok(
            ComProcedencia<PaginaDe<InteracaoResumo>>.DoNossoBanco(
                resumo, "processo.Interacao", relogio));
    }
}

/// <summary>
/// A cobertura de carteira, cliente a cliente — a tela que responde "com quem eu não falo há
/// tempo demais".
/// </summary>
public sealed class ListarCobertura(IRepositorioCarteiras repositorio, IRelogio relogio)
{
    /// <summary>Executa a listagem.</summary>
    /// <param name="pagina">Página pedida.</param>
    /// <param name="tamanho">Linhas por página.</param>
    /// <param name="classe">Filtro pela classe do cliente na carteira. Domínio fechado.</param>
    /// <param name="diasSemContato">Só quem está há mais dias que isto sem contato.</param>
    /// <param name="somenteSemContato">Só quem nunca foi contatado.</param>
    /// <param name="ordenarPor">Coluna de ordenação. Domínio fechado.</param>
    /// <param name="descendente">Ordem decrescente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<CoberturaResumo>>>> ExecutarAsync(
        int? pagina,
        int? tamanho,
        string? classe,
        int? diasSemContato,
        bool somenteSemContato,
        string? ordenarPor,
        bool descendente,
        CancellationToken ct)
    {
        var erros = new ColetorDeErros();

        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<CoberturaResumo>>>.FalhaDeValidacao(
                paginacao.Erro!, paginacao.Erros);

        var classeEscolhida = classe is null
            ? null
            : erros.ItemDeDominio<ClasseDeCliente>("classe", classe);

        var ordem = erros.ItemDeDominioOuPadrao(
            "ordenarPor", ordenarPor, OrdemDeCobertura.UltimaInteracaoEm);

        if (diasSemContato is < 0)
            erros.Registrar("diasSemContato", "O número de dias começa em zero.", diasSemContato.ToString());

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<CoberturaResumo>>>(
                "A consulta tem parâmetros que não valem.");

        var pagi = await repositorio.ListarCoberturaAsync(
            new ConsultaDeCobertura(
                paginacao.Valor,
                Classe: classeEscolhida,
                DiasSemContato: diasSemContato,
                SomenteSemContato: somenteSemContato,
                Ordem: ordem,
                Descendente: descendente),
            ct);

        var agora = relogio.Agora;

        var resumo = new PaginaDe<CoberturaResumo>(
            [.. pagi.Itens.Select(c => CoberturaResumo.De(c, agora))],
            pagi.Pagina, pagi.Tamanho, pagi.Total);

        return Resultado<ComProcedencia<PaginaDe<CoberturaResumo>>>.Ok(
            ComProcedencia<PaginaDe<CoberturaResumo>>.DoNossoBanco(
                resumo, "comercial.ClienteCarteira", relogio));
    }
}

/// <summary>A cobertura por carteira, contada no banco.</summary>
public sealed class ObterResumoDeCobertura(IRepositorioCarteiras repositorio, IRelogio relogio)
{
    /// <summary>Executa o agregado.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<Agregado<ResumoDeCobertura>>>> ExecutarAsync(
        CancellationToken ct)
    {
        var agora = relogio.Agora;
        var carteiras = await repositorio.ResumirCoberturaAsync(agora, ct);

        var clientes = carteiras.Sum(c => c.Clientes);
        var nunca = carteiras.Sum(c => c.NuncaContatados);

        var ausentes = new List<MetricaSemDado>();

        if (clientes == 0)
            ausentes.Add(new MetricaSemDado(
                "cobertura", "Não há carteira com cliente ao alcance deste contexto de acesso."));
        else if (nunca == clientes)
            ausentes.Add(new MetricaSemDado(
                "coberturaDeCarteira",
                $"Nenhum dos {clientes} clientes carteirizados tem a data do último contato. Ela vem do histórico do " +
                "Vórtice, pela rotina das carteiras; sem ela não existe último contato, e o indicador de " +
                "cobertura mede exatamente isso."));

        return Resultado<ComProcedencia<Agregado<ResumoDeCobertura>>>.Ok(
            ComProcedencia<Agregado<ResumoDeCobertura>>.DoNossoBanco(
                new Agregado<ResumoDeCobertura>(carteiras, ausentes),
                "comercial.ClienteCarteira", relogio));
    }
}

/// <summary>
/// As vendas perdidas — por motivo, para quem, e a que distância de preço.
///
/// <para><b>São duas populações, e a tela precisa das duas.</b> Os PROCESSOS do funil do Vórtice que terminaram
/// perdidos, e os FORMULÁRIOS que o CEN preencheu sobre a derrota. A diferença entre os dois números é a informação mais
/// acionável desta consulta: quantas derrotas ninguém registrou. Ela vai como métrica sem dado, junto do agregado.</para>
///
/// <para><b>O recorte (27/09/2026, documento 52 §4):</b> o período é o da data em que o formulário foi preenchido — o
/// padrão é o ano fiscal até o último mês fechado —, o formulário de origem é filtro opcional, e só a resposta
/// PRINCIPAL conta: a duplicata e o complemento repetem a mesma perda.</para>
/// </summary>
/// <param name="vendas">O acesso às vendas perdidas e aos processos perdidos do funil.</param>
/// <param name="relogio">O relógio, para o período padrão e a procedência.</param>
public sealed class ObterVendasPerdidas(
    IRepositorioVendasPerdidas vendas,
    IRelogio relogio)
{
    /// <summary>Executa o agregado.</summary>
    /// <param name="de">O primeiro dia; vazio com o último vazio é o ano fiscal até o último mês fechado.</param>
    /// <param name="ate">O último dia, inclusive.</param>
    /// <param name="formulario">O formulário de origem (<see cref="FormulariosDaVendaPerdida"/>); vazio para todos.</param>
    /// <param name="responsavel">O responsável pelo processo no funil, pela chave pública; vazio para todos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<VendasPerdidasResumidas>>> ExecutarAsync(
        DateOnly? de, DateOnly? ate, string? formulario, Guid? responsavel, CancellationToken ct)
    {
        var erros = new ColetorDeErros();
        var periodo = PeriodoDoRelatorio.Resolver(de, ate, relogio.Agora, erros);

        var doFormulario = string.IsNullOrWhiteSpace(formulario) ? null : formulario.Trim().ToUpperInvariant();
        if (doFormulario is not null && !FormulariosDaVendaPerdida.Todos.Contains(doFormulario, StringComparer.Ordinal))
            erros.Registrar("formulario", $"Formulário fora da lista. Opções: {string.Join(", ", FormulariosDaVendaPerdida.Todos)}.", formulario);

        if (erros.TemErro || periodo is null)
            return erros.Recusar<ComProcedencia<VendasPerdidasResumidas>>("A consulta tem parâmetros que não valem.");

        var filtro = new FiltroDeVendaPerdida(periodo.DeUtc, periodo.AteUtc, doFormulario, responsavel);
        var porMotivo = await vendas.ResumirPorMotivoAsync(filtro, ct);
        var porConcorrente = await vendas.ResumirPorConcorrenteAsync(filtro, ct);
        var registradas = porMotivo.Sum(f => f.Quantidade);

        // O PROCESSO PERDIDO VEM DO FUNIL DO VÓRTICE (27/09/2026): processo.Processo só a onda 2 carrega, e contar ali
        // dizia "0 processos perdidos" com centenas de formulários preenchidos.
        var perdidosNoProcesso = await vendas.ContarProcessosPerdidosAsync(filtro, ct);

        var ausentes = new List<MetricaSemDado>();

        if (registradas == 0 && perdidosNoProcesso > 0)
            ausentes.Add(new MetricaSemDado(
                "vendasPerdidasRegistradas",
                $"Há {perdidosNoProcesso} processo(s) do Vórtice perdido(s) em {periodo.Texto} e nenhum formulário de " +
                "venda perdida preenchido no período. O motivo, o concorrente e a diferença de preço só " +
                "existem quando o CEN preenche o formulário."));
        else if (perdidosNoProcesso > registradas)
            ausentes.Add(new MetricaSemDado(
                "vendasPerdidasSemFormulario",
                $"{perdidosNoProcesso - registradas} de {perdidosNoProcesso} processos do Vórtice perdidos em {periodo.Texto} " +
                "não têm formulário de venda perdida no período. A distribuição abaixo é das " +
                $"{registradas} derrotas que foram registradas, e não de todas."));

        var semConcorrente = registradas - porConcorrente.Sum(f => f.Quantidade);
        if (semConcorrente > 0)
            ausentes.Add(new MetricaSemDado(
                "concorrenteDaVendaPerdida",
                $"{semConcorrente} de {registradas} formulários não declaram para qual fabricante " +
                "a venda foi perdida. O ranking de concorrentes deixa essas linhas de fora, em " +
                "vez de criar uma fatia \"não informado\" no topo dele."));

        return Resultado<ComProcedencia<VendasPerdidasResumidas>>.Ok(
            ComProcedencia<VendasPerdidasResumidas>.DoNossoBanco(
                new VendasPerdidasResumidas(
                    registradas, perdidosNoProcesso, porMotivo, porConcorrente, ausentes, periodo, doFormulario),
                "processo.VendaPerdida · processo.EstagioDoProcesso",
                relogio));
    }
}

/// <summary>
/// O relatório de vendas perdidas como a tela o consome.
/// </summary>
/// <param name="Registradas">Quantos formulários de venda perdida (principais) foram preenchidos no período.</param>
/// <param name="ProcessosPerdidos">Quantos processos do funil do Vórtice terminaram perdidos no período.</param>
/// <param name="PorMotivo">A distribuição por motivo.</param>
/// <param name="PorConcorrente">O ranking de para quem se perdeu.</param>
/// <param name="MetricasSemDado">O que estes números não dizem.</param>
/// <param name="Periodo">O período, escrito.</param>
/// <param name="Formulario">O formulário filtrado; nulo para todos.</param>
public sealed record VendasPerdidasResumidas(
    int Registradas,
    int ProcessosPerdidos,
    IReadOnlyList<FatiaDeVendaPerdida> PorMotivo,
    IReadOnlyList<FatiaDeVendaPerdida> PorConcorrente,
    IReadOnlyList<MetricaSemDado> MetricasSemDado,
    PeriodoDoRelatorio Periodo,
    string? Formulario);

/// <summary>
/// O painel de um CEN — o filtro que a gerência pediu: escolhe a pessoa e vê a carteira dela.
///
/// <para><b>A pergunta que esta consulta responde</b> é a que se faz numa reunião de resultado:
/// deste CEN, quantos clientes A estão cobertos e quantos venceram o prazo; quantos ele nunca
/// procurou; quantos negócios ele ganhou e perdeu. Tudo com o mesmo dado que as outras telas
/// usam, para o número da reunião bater com o número do painel.</para>
/// </summary>
/// <param name="repositorio">O acesso ao painel.</param>
/// <param name="relogio">O relógio, que também mede a cadência.</param>
public sealed class ObterPainelDoCen(IRepositorioPainelDoCen repositorio, IRelogio relogio)
{
    /// <summary>Executa o painel.</summary>
    /// <param name="responsavelChave">O CEN escolhido; nulo traz o consolidado.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelDoCenResumido>>> ExecutarAsync(
        Guid? responsavelChave, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var painel = await repositorio.ObterPainelAsync(responsavelChave, agora, ct);

        if (painel is null)
            return Resultado<ComProcedencia<PainelDoCenResumido>>.NaoEncontrado(
                "Não há responsável com essa chave ao alcance deste contexto de acesso.");

        var responsaveis = await repositorio.ListarResponsaveisAsync(ct);

        var semCadencia = painel.PorClasse.Sum(c => c.SemCadenciaDeclarada);
        var ausentes = new List<MetricaSemDado>();

        if (semCadencia > 0)
            ausentes.Add(new MetricaSemDado(
                "cadenciaDeVisita",
                $"{semCadencia} vínculos estão em linha de negócio que não declara de quantos em " +
                "quantos dias visitar. Eles não entram como cobertos nem como vencidos — não há " +
                "prazo contra o que medi-los, e escolher um número aqui seria inventar a meta."));

        // A RESSALVA DO ÚLTIMO CONTATO. A frase antiga dizia que ele vinha do "recorte carregado, o ano corrente" — era
        // a carga legada. Desde 27/09/2026 a data vem do histórico INTEIRO do Vórtice, pela regra da BI de carteiras
        // (os resultados que contam como contato), gravada pela rotina das carteiras (CargaDeCarteirasDoVortice). O que
        // continua valendo dizer é o que "contato" é: qualquer resultado da lista, e não só visita.
        if (painel.Clientes > 0)
            ausentes.Add(new MetricaSemDado(
                "historicoDeContato",
                "\"Nunca contatado\" quer dizer que o histórico do Vórtice não tem contato com o cliente pela regra da BI de " +
                "carteiras — qualquer um dos resultados que ela conta como contato, e não só visita. A data vem do histórico " +
                "inteiro, lida pela rotina das carteiras do Vórtice, e só anda para a frente."));

        // A FRASE ANTIGA DIZIA QUE O FATURAMENTO "PARA EM 11/04/2025" — era a cópia que o Vórtice recebia, e não o
        // faturamento. O daqui é o da SD2 do Protheus, lido direto pela rotina FATURAMENTO_PROTHEUS (#233). O que falta
        // de verdade é o recorte por vendedor: a carga agrega a nota por cliente, filial e mês, e não lê o vendedor
        // dela (SF2.F2_VEND1) — ver LeitorDeFaturamentoDoProtheus.
        if (painel.FaturamentoDaCarteira > 0)
            ausentes.Add(new MetricaSemDado(
                "faturamentoDaCarteira",
                "O faturamento é dos CLIENTES da carteira, e não das vendas desta pessoa: a carga do " +
                "Protheus agrega a nota por cliente, filial e mês e ainda não lê o vendedor da nota " +
                "(F2_VEND1). Entram as notas que a rotina de faturamento carregou — a janela de três " +
                "anos que ela mantém — nas filiais ao seu alcance."));

        return Resultado<ComProcedencia<PainelDoCenResumido>>.Ok(
            ComProcedencia<PainelDoCenResumido>.DoNossoBanco(
                new PainelDoCenResumido(
                    painel,
                    [.. responsaveis.Select(r => new ResponsavelParaSelecao(r.Chave, r.Nome, r.Natureza, r.Carteiras))],
                    ausentes),
                "comercial.ClienteCarteira",
                relogio));
    }
}

/// <summary>O painel do CEN como a tela o consome.</summary>
/// <param name="Painel">Os números do responsável escolhido.</param>
/// <param name="Responsaveis">Todos os que têm carteira comercial, para o seletor.</param>
/// <param name="MetricasSemDado">O que estes números não dizem.</param>
public sealed record PainelDoCenResumido(
    PainelDoResponsavel Painel,
    IReadOnlyList<ResponsavelParaSelecao> Responsaveis,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>Um responsável no seletor da tela.</summary>
/// <param name="Chave">A chave pública.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="Natureza">Se é <c>Pessoa</c> ou <c>Departamento</c>.</param>
/// <param name="Carteiras">Quantas carteiras comerciais ele responde.</param>
public sealed record ResponsavelParaSelecao(Guid Chave, string Nome, string Natureza, int Carteiras);

/// <summary>
/// O faturamento — a série de doze meses e o ranking de clientes, lidos do ERP.
///
/// <para><b>Esta consulta existe porque uma premissa caiu.</b> Até 06/09/2026 o CRM afirmava que
/// o faturamento tinha parado em 11/04/2025, e três cartões do painel executivo mostravam "sem
/// dado" por causa disso. A frase vinha da tabela que o Vórtice RECEBE do Protheus; na origem,
/// medida, há nota emitida na mesma semana. O que morreu foi a integração.</para>
///
/// <para><b>O período vai junto do número, sempre.</b> Não é zelo: é a defesa contra a repetição
/// do defeito. Se a carga do ERP parar de novo, a competência mais recente para de avançar e a
/// tela diz isso — em vez de mostrar um total plausível e velho.</para>
/// </summary>
/// <param name="repositorio">O acesso ao faturamento.</param>
/// <param name="relogio">O relógio, para a procedência e para medir o atraso.</param>
public sealed class ObterFaturamento(IRepositorioFaturamento repositorio, IRelogio relogio)
{
    /// <summary>Quantos meses a série traz.</summary>
    private const int MesesDaSerie = 12;

    /// <summary>Quantos clientes o ranking traz.</summary>
    private const int ClientesNoRanking = 5;

    /// <summary>
    /// A partir de quantos dias de atraso a tela avisa que o dado envelheceu.
    ///
    /// <para>Sessenta dias porque a competência é mensal: um mês fechado só aparece completo
    /// depois de virar o mês, então trinta dias de "atraso" é operação normal. Sessenta já é
    /// sinal de que alguma coisa parou — e foi exatamente esse sinal que faltou por dezessete
    /// meses.</para>
    /// </summary>
    private const int DiasParaAvisarAtraso = 60;

    /// <summary>Executa a leitura.</summary>
    /// <param name="anoFiscal">
    /// O ANO FISCAL ESCOLHIDO NA TELA (30/09/2026, #313): a série termina no fim dele (ou no último mês carregado, no ano
    /// que corre), e o ranking é o faturamento dele. Nulo é o de antes: os doze meses até o último carregado e o ranking
    /// acumulado.
    /// </param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<FaturamentoResumido>>> ExecutarAsync(int? anoFiscal, CancellationToken ct)
    {
        var ultima = await repositorio.CompetenciaMaisRecenteAsync(ct);
        var ausentes = new List<MetricaSemDado>();

        if (ultima is null)
        {
            ausentes.Add(new MetricaSemDado(
                "faturamento",
                "Não há faturamento carregado para esta filial. Ele vem da SD2 do Protheus, pela " +
                "carga — se ela nunca rodou com a ponte configurada, não há o que mostrar."));

            return Resultado<ComProcedencia<FaturamentoResumido>>.Ok(
                ComProcedencia<FaturamentoResumido>.DoNossoBanco(
                    new FaturamentoResumido(null, 0m, false, [], [], ausentes),
                    "comercial.FaturamentoDoCliente", relogio));
        }

        var doAno = anoFiscal is { } ano ? AnoFiscal.Inteiro(ano) : null;
        var serie = await repositorio.SerieMensalAsync(MesesDaSerie, doAno?.Final, ct);
        var top = await repositorio.TopClientesAsync(ClientesNoRanking, doAno?.Inicial, doAno?.Final, ct);

        // O ÚLTIMO MÊS DA SÉRIE, e ele pode estar ABERTO. Medido em 06/09/2026: setembro tinha
        // R$ 1,7 milhão contra R$ 15,9 milhões de agosto — não porque a empresa parou de vender,
        // mas porque o mês tinha seis dias. Mostrar esse número como "faturamento do mês" ao
        // lado dos onze meses cheios do gráfico erraria por 90%, e é exatamente o tipo de
        // comparação que faz uma diretoria decidir errado.
        var doUltimoMes = serie.LastOrDefault()?.ValorLiquido ?? 0m;
        var hoje = DateOnly.FromDateTime(relogio.Agora);
        // O ÚLTIMO MÊS É O DA SÉRIE: num ano fiscal fechado ele é outubro, e não está em curso.
        var ultimoDaSerie = serie.LastOrDefault()?.Competencia ?? ultima.Value;
        var mesAindaAberto = ultimoDaSerie.Year == hoje.Year && ultimoDaSerie.Month == hoje.Month;

        if (mesAindaAberto)
            ausentes.Add(new MetricaSemDado(
                "mesEmCurso",
                $"{ultimoDaSerie:MM/yyyy} ainda está em curso — o valor é do que já foi faturado " +
                $"até {hoje:dd/MM}, e não do mês inteiro. Compará-lo com um mês fechado " +
                "subestima o mês corrente."));

        var fimDaCompetencia = ultima.Value.AddMonths(1).AddDays(-1);
        var atraso = DateOnly.FromDateTime(relogio.Agora).DayNumber - fimDaCompetencia.DayNumber;

        if (atraso > DiasParaAvisarAtraso)
            ausentes.Add(new MetricaSemDado(
                "faturamentoDesatualizado",
                $"O faturamento mais recente é de {ultima.Value:MM/yyyy} — {atraso} dias atrás. " +
                "A carga lê a SD2 do Protheus; um atraso deste tamanho quer dizer que ela parou " +
                "de rodar, e não que a empresa deixou de vender."));

        return Resultado<ComProcedencia<FaturamentoResumido>>.Ok(
            ComProcedencia<FaturamentoResumido>.DoNossoBanco(
                new FaturamentoResumido(ultimoDaSerie, doUltimoMes, mesAindaAberto, serie, top, ausentes),
                "comercial.FaturamentoDoCliente", relogio));
    }
}

/// <summary>
/// O FATURAMENTO DE UM CLIENTE — a série de doze meses, o total, a quebra por grupo de item da nota e a filial que
/// faturou, lidos da SD2 do Protheus pela rotina <c>FATURAMENTO_PROTHEUS</c> (#233).
///
/// <para><b>Esta consulta troca uma frase falsa.</b> A ficha do cliente dizia que o faturamento tinha parado em
/// 11/04/2025 e que "a ponte não lê" — era a cópia que o Vórtice recebia. Em 27/09/2026 a tabela tinha 53.977 meses de
/// 7.505 clientes, em 16 filiais, de 09/2023 a 09/2026, e nenhuma tela a lia por cliente.</para>
///
/// <para><b>A janela ancora na carga, e não em hoje.</b> Os doze meses terminam na competência mais recente que a carga
/// trouxe — a mesma âncora da série da diretoria. Com a rotina desligada, a tela diz até quando o dado vai (a data da
/// carga), em vez de mostrar meses zerados no fim como se o cliente tivesse parado de comprar.</para>
///
/// <para><b>Reais e unidades não se somam (D-P08).</b> Os reais vêm do Protheus; as máquinas em unidades vêm do ART e
/// aparecem na frota. A parcela "máquina" daqui é em reais, pelo grupo VEIC da própria linha da nota.</para>
/// </summary>
/// <param name="repositorio">O acesso ao faturamento.</param>
/// <param name="relogio">O relógio, para a procedência e para medir o atraso.</param>
public sealed class ObterFaturamentoDoCliente(IRepositorioFaturamento repositorio, IRelogio relogio)
{
    /// <summary>Quantos meses a série e o total trazem.</summary>
    private const int MesesDaJanela = 12;

    /// <summary>A partir de quantos dias sem competência nova a tela avisa — a régua de <see cref="ObterFaturamento"/>.</summary>
    private const int DiasParaAvisarAtraso = 60;

    /// <summary>Executa a leitura.</summary>
    /// <param name="chave">O GUID público do cliente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<FaturamentoDoClienteResumido>>> ExecutarAsync(Guid chave, CancellationToken ct)
    {
        var lido = await repositorio.DoClienteAsync(chave, ct);

        // NÃO EXISTE E NÃO É SEU dão a mesma resposta, como no cadastro.
        if (lido is null)
            return Resultado<ComProcedencia<FaturamentoDoClienteResumido>>.NaoEncontrado(
                $"Não há cliente {chave} ao seu alcance.");

        var ausentes = new List<MetricaSemDado>();

        if (lido.CompetenciaMaisRecenteDaCarga is not { } ultima)
        {
            ausentes.Add(new MetricaSemDado(
                "faturamento",
                "Não há faturamento carregado nas filiais ao seu alcance. Ele vem da SD2 do Protheus, pela " +
                "rotina FATURAMENTO_PROTHEUS — sem nenhuma execução dela, não há o que mostrar."));

            return Resultado<ComProcedencia<FaturamentoDoClienteResumido>>.Ok(
                ComProcedencia<FaturamentoDoClienteResumido>.DoNossoBanco(
                    new FaturamentoDoClienteResumido(null, null, null, false, null, null, null, [], [], ausentes),
                    "comercial.FaturamentoDoCliente", relogio));
        }

        var inicio = ultima.AddMonths(-(MesesDaJanela - 1));
        var primeira = lido.PrimeiraCompetenciaDaCarga ?? ultima;
        var daJanela = lido.Meses.Where(m => m.Competencia >= inicio && m.Competencia <= ultima).ToList();

        // O MÊS SEM NOTA ENTRA COM ZERO, e não some da série: um mês ausente faria a linha pular de agosto para
        // outubro como se setembro não tivesse existido — a mesma regra da série da diretoria.
        var serie = new List<MesDeFaturamentoDoCliente>(MesesDaJanela);
        for (var mes = inicio; mes <= ultima; mes = mes.AddMonths(1))
        {
            var doMes = daJanela.Where(m => m.Competencia == mes).ToList();
            serie.Add(new MesDeFaturamentoDoCliente(
                mes, doMes.Sum(m => m.ValorLiquido), doMes.Sum(m => m.Maquina), doMes.Sum(m => m.Notas)));
        }

        // A FILIAL É A QUE EMITIU A NOTA. A que faturou só antes da janela fica na lista, com zero nos doze meses e a
        // data da última nota — é o que diz "comprava em Barretos e parou".
        var porFilial = lido.Meses
            .GroupBy(m => (m.FilialCodigo, m.FilialNome))
            .Select(g =>
            {
                var doze = g.Where(m => m.Competencia >= inicio && m.Competencia <= ultima).ToList();
                return new FaturamentoDoClienteNaFilial(
                    g.Key.FilialCodigo,
                    g.Key.FilialNome,
                    doze.Sum(m => m.ValorLiquido),
                    doze.Sum(m => m.Maquina),
                    doze.Sum(m => m.Notas),
                    g.Sum(m => m.ValorLiquido),
                    g.Max(m => m.Competencia));
            })
            .OrderByDescending(f => f.DozeMeses)
            .ThenByDescending(f => f.UltimaNotaEm)
            .ThenBy(f => f.FilialCodigo, StringComparer.Ordinal)
            .ToList();

        DateOnly? ultimaCompra = lido.Meses.Count == 0 ? null : lido.Meses.Max(m => m.Competencia);

        // O MÊS MAIS RECENTE ESTÁ INTEIRO SÓ QUANDO A CARGA RODOU DEPOIS QUE ELE ACABOU. Medir pelo calendário ("é o
        // mês corrente?") erra com a rotina desligada: a competência 09/2026 da carga de 24/09 continuaria parcial em
        // outubro, e a tela passaria a tratá-la como mês fechado.
        var inicioDoMesSeguinte = ultima.AddMonths(1).ToDateTime(TimeOnly.MinValue);
        var ultimoMesIncompleto = lido.CarregadoEm is not { } carregadoEm || carregadoEm < inicioDoMesSeguinte;

        if (lido.Meses.Count == 0)
            ausentes.Add(new MetricaSemDado(
                "faturamentoDoCliente",
                $"Nenhuma nota de venda deste cliente nas filiais ao seu alcance, de {primeira:MM/yyyy} a " +
                $"{ultima:MM/yyyy} — a janela de três anos que a carga do Protheus mantém. A fronteira é a filial " +
                "que emitiu a nota: a nota de uma filial fora do seu alcance não aparece aqui."));
        else if (daJanela.Count == 0)
            ausentes.Add(new MetricaSemDado(
                "faturamentoNosDozeMeses",
                $"Nenhuma nota deste cliente de {inicio:MM/yyyy} a {ultima:MM/yyyy} nas filiais ao seu alcance. " +
                $"A mais recente é de {ultimaCompra:MM/yyyy}."));

        if (ultimoMesIncompleto)
            ausentes.Add(new MetricaSemDado(
                "mesIncompleto",
                $"{ultima:MM/yyyy} não está inteiro: a carga gravou esse mês até " +
                $"{lido.CarregadoEm:dd/MM/yyyy HH:mm} (UTC). O valor dele é parcial, e compará-lo com um mês " +
                "fechado subestima o mês."));

        var fimDaCompetencia = ultima.AddMonths(1).AddDays(-1);
        var atraso = DateOnly.FromDateTime(relogio.Agora).DayNumber - fimDaCompetencia.DayNumber;
        if (atraso > DiasParaAvisarAtraso)
            ausentes.Add(new MetricaSemDado(
                "faturamentoDesatualizado",
                $"O faturamento mais recente carregado é de {ultima:MM/yyyy} — {atraso} dias atrás. A rotina lê a " +
                "SD2 do Protheus; um atraso deste tamanho quer dizer que ela parou de rodar, e não que o cliente " +
                "deixou de comprar."));

        return Resultado<ComProcedencia<FaturamentoDoClienteResumido>>.Ok(
            ComProcedencia<FaturamentoDoClienteResumido>.DoNossoBanco(
                new FaturamentoDoClienteResumido(
                    primeira,
                    ultima,
                    lido.CarregadoEm,
                    ultimoMesIncompleto,
                    ultimaCompra,
                    Periodo(inicio, ultima, daJanela),
                    Periodo(primeira, ultima, lido.Meses),
                    serie,
                    porFilial,
                    ausentes),
                "comercial.FaturamentoDoCliente",
                relogio));
    }

    private static PeriodoDeFaturamentoDoCliente Periodo(DateOnly de, DateOnly ate, IReadOnlyCollection<MesDoClienteNaFilial> meses) =>
        new(de, ate,
            meses.Sum(m => m.ValorLiquido),
            meses.Sum(m => m.Maquina),
            meses.Sum(m => m.Peca),
            meses.Sum(m => m.Servico),
            meses.Sum(m => m.Outros),
            meses.Sum(m => m.Notas));
}

/// <summary>O faturamento de um cliente como a ficha o consome.</summary>
/// <param name="PrimeiraCompetenciaDaCarga">O começo da janela carregada ao alcance. Nulo quando não há carga.</param>
/// <param name="CompetenciaMaisRecente">A competência mais recente da carga ao alcance — a âncora dos doze meses.</param>
/// <param name="CarregadoEm">Quando a carga gravou essa competência (UTC) — o "até a carga de" da tela.</param>
/// <param name="UltimoMesEstaIncompleto">
/// Se a carga gravou a competência mais recente antes de ela acabar. Verdadeiro, o último mês da série é parcial.
/// </param>
/// <param name="UltimaCompraEm">A competência da nota mais recente do cliente ao alcance.</param>
/// <param name="DozeMeses">O total dos doze meses até a competência mais recente, com a quebra.</param>
/// <param name="JanelaCarregada">O total da janela inteira que a carga mantém, com a quebra.</param>
/// <param name="Serie">Os doze meses, do mais antigo para o mais novo, com zero no mês sem nota.</param>
/// <param name="PorFilial">A filial que emitiu a nota, com os doze meses e a janela.</param>
/// <param name="MetricasSemDado">O que estes números não dizem.</param>
public sealed record FaturamentoDoClienteResumido(
    DateOnly? PrimeiraCompetenciaDaCarga,
    DateOnly? CompetenciaMaisRecente,
    DateTime? CarregadoEm,
    bool UltimoMesEstaIncompleto,
    DateOnly? UltimaCompraEm,
    PeriodoDeFaturamentoDoCliente? DozeMeses,
    PeriodoDeFaturamentoDoCliente? JanelaCarregada,
    IReadOnlyList<MesDeFaturamentoDoCliente> Serie,
    IReadOnlyList<FaturamentoDoClienteNaFilial> PorFilial,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>
/// Um período de faturamento do cliente, com a quebra por grupo de item da nota — as quatro parcelas somam o total.
/// </summary>
/// <param name="De">A primeira competência do período.</param>
/// <param name="Ate">A última.</param>
/// <param name="ValorLiquido">O total.</param>
/// <param name="Maquina">Máquina (grupo VEIC), em reais.</param>
/// <param name="Peca">Peça.</param>
/// <param name="Servico">Serviço e mão de obra.</param>
/// <param name="Outros">Grupo que ainda não se sabe ler.</param>
/// <param name="Notas">Notas distintas, somadas mês a mês.</param>
public sealed record PeriodoDeFaturamentoDoCliente(
    DateOnly De, DateOnly Ate, decimal ValorLiquido, decimal Maquina, decimal Peca, decimal Servico, decimal Outros, int Notas);

/// <summary>Um mês da série do cliente.</summary>
/// <param name="Competencia">O primeiro dia do mês.</param>
/// <param name="ValorLiquido">O que foi faturado, somadas as filiais ao alcance.</param>
/// <param name="Maquina">Quanto disso foi máquina.</param>
/// <param name="Notas">Notas distintas.</param>
public sealed record MesDeFaturamentoDoCliente(DateOnly Competencia, decimal ValorLiquido, decimal Maquina, int Notas);

/// <summary>O faturamento do cliente numa filial — a que emitiu a nota.</summary>
/// <param name="FilialCodigo">O código da filial.</param>
/// <param name="FilialNome">O nome dela.</param>
/// <param name="DozeMeses">O total dos doze meses.</param>
/// <param name="MaquinaNosDozeMeses">Quanto disso foi máquina.</param>
/// <param name="NotasNosDozeMeses">Notas nos doze meses.</param>
/// <param name="NaJanela">O total da janela inteira carregada.</param>
/// <param name="UltimaNotaEm">A competência da nota mais recente nesta filial.</param>
public sealed record FaturamentoDoClienteNaFilial(
    string FilialCodigo,
    string FilialNome,
    decimal DozeMeses,
    decimal MaquinaNosDozeMeses,
    int NotasNosDozeMeses,
    decimal NaJanela,
    DateOnly UltimaNotaEm);

/// <summary>
/// AS CARTEIRAS DO CLIENTE E O CEN DE CADA UMA — o bloco da ficha que dizia "Falta rota".
///
/// <para><b>A classe é a do cadastro do cliente</b> (curva ABC apurada do faturamento), e não a do vínculo: medido em
/// 27/09/2026, os 8.537 vínculos ativos têm a classe C que a carga das carteiras grava por padrão, e os clientes
/// deles se dividem em A, B, C e D pela apuração. A cadência também não é a do vínculo (nula em todos): é a da linha
/// de negócio da carteira para a classe do cliente, D quando ela não foi apurada — a mesma regra do cartão de cobertura
/// da Visão 360 e do painel do CEN.</para>
///
/// <para><b>O último contato vem vazio hoje, e o motivo vai junto</b>: nenhum vínculo tem a data, porque ela só é
/// gravada quando uma interação é registrada no CRM, a carga das interações do Vórtice está congelada (D-12) e a
/// rotina das carteiras não traz o último contato. Vazio aqui é "sem registro", e não "nunca contatado".</para>
/// </summary>
/// <param name="repositorio">O acesso à carteirização.</param>
/// <param name="relogio">O relógio, para a procedência e para os dias sem contato.</param>
public sealed class ListarCarteirasDoCliente(IRepositorioCarteiras repositorio, IRelogio relogio)
{
    /// <summary>Executa a leitura.</summary>
    /// <param name="chave">O GUID público do cliente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<CarteirasDoClienteResumidas>>> ExecutarAsync(Guid chave, CancellationToken ct)
    {
        var lido = await repositorio.ListarDoClienteAsync(chave, ct);

        if (lido is null)
            return Resultado<ComProcedencia<CarteirasDoClienteResumidas>>.NaoEncontrado(
                $"Não há cliente {chave} ao seu alcance.");

        var agora = relogio.Agora;
        var classeDaCadencia = lido.Classe ?? ClasseDeCliente.D;
        var carteiras = lido.Vinculos.Select(v => CarteiraDoCliente.De(v, classeDaCadencia, agora)).ToList();

        var ausentes = new List<MetricaSemDado>();

        if (carteiras.Count == 0)
            ausentes.Add(new MetricaSemDado(
                "carteiras",
                "Este cliente não tem vínculo ativo em nenhuma carteira ao seu alcance. A fronteira é a filial da " +
                "carteira: a carteira de uma filial fora do seu alcance não aparece aqui."));
        else if (carteiras.All(c => c.UltimaInteracaoEm is null))
            ausentes.Add(new MetricaSemDado(
                "ultimoContato",
                $"Nenhum dos {carteiras.Count} vínculos deste cliente tem data de último contato. Ela só é gravada " +
                "quando uma interação é registrada no CRM; a carga das interações do Vórtice está congelada (decisão " +
                "D-12) e a rotina das carteiras (CARTEIRAS_VORTICE) não traz o último contato. Vazio aqui quer dizer " +
                "\"sem registro no CRM\", e não \"nunca contatado\"."));

        if (lido.Classe is null)
            ausentes.Add(new MetricaSemDado(
                "classe",
                "A classe deste cliente ainda não foi apurada: ela sai da curva ABC do faturamento, na carga do " +
                "Protheus. A cadência abaixo usa a da classe D, a regra para quem não tem classe."));

        return Resultado<ComProcedencia<CarteirasDoClienteResumidas>>.Ok(
            ComProcedencia<CarteirasDoClienteResumidas>.DoNossoBanco(
                new CarteirasDoClienteResumidas(lido.Classe?.ToString(), lido.ClasseApuradaEm, carteiras, ausentes),
                "comercial.ClienteCarteira",
                relogio));
    }
}

/// <summary>As carteiras do cliente como a ficha as consome.</summary>
/// <param name="Classe">A classe da curva ABC no cadastro do cliente: A, B, C ou D. Nula antes da apuração.</param>
/// <param name="ClasseApuradaEm">Quando a classe foi apurada (UTC).</param>
/// <param name="Carteiras">As carteiras em que ele está, ao alcance de quem consulta — as comerciais primeiro.</param>
/// <param name="MetricasSemDado">O que estes números não dizem.</param>
public sealed record CarteirasDoClienteResumidas(
    string? Classe,
    DateTime? ClasseApuradaEm,
    IReadOnlyList<CarteiraDoCliente> Carteiras,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>Uma carteira do cliente, com o CEN, a filial e a cadência.</summary>
/// <param name="CarteiraChave">O GUID público da carteira.</param>
/// <param name="CarteiraCodigo">O código da carteira.</param>
/// <param name="CarteiraNome">O nome.</param>
/// <param name="NaturezaDaCarteira">Comercial, Administrativa ou Teste.</param>
/// <param name="LinhaDeNegocioNome">A linha de negócio.</param>
/// <param name="ResponsavelNome">O CEN responsável pela carteira.</param>
/// <param name="NaturezaDoResponsavel">Pessoa ou caixa de área (Departamento), entre outras.</param>
/// <param name="FilialCodigo">O código da filial dona da carteira.</param>
/// <param name="FilialNome">O nome dela.</param>
/// <param name="DiasDeCadencia">
/// De quantos em quantos dias a linha de negócio visita a classe do cliente. Nulo quando a linha não declara.
/// </param>
/// <param name="VinculadoEm">Quando o cliente entrou na carteira (UTC).</param>
/// <param name="UltimaInteracaoEm">O último contato registrado (UTC). Nulo é "sem registro".</param>
/// <param name="DiasSemContato">Há quantos dias foi o último contato. Nulo sem registro.</param>
/// <param name="EstaForaDaCadencia">
/// Se o último contato passou da cadência. Nulo sem registro ou sem cadência — não se sabe, e "falso" diria que está em
/// dia.
/// </param>
public sealed record CarteiraDoCliente(
    Guid CarteiraChave,
    string CarteiraCodigo,
    string CarteiraNome,
    string NaturezaDaCarteira,
    string LinhaDeNegocioNome,
    string? ResponsavelNome,
    string? NaturezaDoResponsavel,
    string FilialCodigo,
    string FilialNome,
    short? DiasDeCadencia,
    DateTime VinculadoEm,
    DateTime? UltimaInteracaoEm,
    int? DiasSemContato,
    bool? EstaForaDaCadencia)
{
    /// <summary>Traduz o vínculo lido, com a cadência da classe do cliente.</summary>
    /// <param name="vinculo">O vínculo com a carteira.</param>
    /// <param name="classe">A classe que escolhe a cadência — a do cadastro, ou D sem apuração.</param>
    /// <param name="agoraUtc">O instante de referência dos dias sem contato.</param>
    public static CarteiraDoCliente De(VinculoDoClienteComCarteira vinculo, ClasseDeCliente classe, DateTime agoraUtc)
    {
        var cadencia = classe switch
        {
            ClasseDeCliente.A => vinculo.CadenciaDaClasseA,
            ClasseDeCliente.B => vinculo.CadenciaDaClasseB,
            ClasseDeCliente.C => vinculo.CadenciaDaClasseC,
            _ => vinculo.CadenciaDaClasseD
        };

        int? dias = vinculo.UltimaInteracaoEm is { } ultima ? (int)Math.Max(0, (agoraUtc - ultima).TotalDays) : null;

        return new CarteiraDoCliente(
            vinculo.CarteiraChave,
            vinculo.CarteiraCodigo,
            vinculo.CarteiraNome,
            vinculo.NaturezaDaCarteira,
            vinculo.LinhaDeNegocioNome,
            vinculo.ResponsavelNome,
            vinculo.NaturezaDoResponsavel,
            vinculo.FilialCodigo,
            vinculo.FilialNome,
            cadencia,
            vinculo.VinculadoEm,
            vinculo.UltimaInteracaoEm,
            dias,
            dias is { } d && cadencia is { } c ? d > c : null);
    }
}

/// <summary>O faturamento como a tela o consome.</summary>
/// <param name="CompetenciaMaisRecente">O mês mais recente com movimento. Nulo quando não há dado.</param>
/// <param name="ValorDoUltimoMes">O faturamento do mês mais recente da série.</param>
/// <param name="UltimoMesEstaAberto">
/// Se o mês mais recente ainda está em curso. Quando verdadeiro, o valor é parcial — e a tela
/// precisa dizer isso, senão o número aparece ao lado de meses cheios como se fosse um deles.
/// </param>
/// <param name="Serie">Os últimos doze meses, do mais antigo para o mais novo.</param>
/// <param name="TopClientes">Os maiores clientes por faturamento acumulado.</param>
/// <param name="MetricasSemDado">O que estes números não dizem.</param>
public sealed record FaturamentoResumido(
    DateOnly? CompetenciaMaisRecente,
    decimal ValorDoUltimoMes,
    bool UltimoMesEstaAberto,
    IReadOnlyList<MesDeFaturamento> Serie,
    IReadOnlyList<ClienteNoRanking> TopClientes,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);
