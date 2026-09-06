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
                $"Nenhum dos {clientes} clientes carteirizados tem interação registrada no recorte " +
                "carregado. Sem interação não existe data de último contato, e o indicador de " +
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
/// <para><b>São duas populações, e a tela precisa das duas.</b> <see cref="ObterPerdas"/> conta
/// PROCESSOS marcados como perdidos; esta conta os FORMULÁRIOS que o CEN preencheu sobre a
/// derrota. A diferença entre os dois números é a informação mais acionável desta consulta:
/// quantas derrotas ninguém registrou. Ela vai como métrica sem dado, junto do agregado.</para>
/// </summary>
/// <param name="vendas">O acesso às vendas perdidas.</param>
/// <param name="processos">O acesso a processos, só para o total de perdidos.</param>
/// <param name="relogio">O relógio, para a procedência.</param>
public sealed class ObterVendasPerdidas(
    IRepositorioVendasPerdidas vendas,
    IRepositorioProcessos processos,
    IRelogio relogio)
{
    /// <summary>Executa o agregado.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<VendasPerdidasResumidas>>> ExecutarAsync(
        CancellationToken ct)
    {
        var porMotivo = await vendas.ResumirPorMotivoAsync(ct);
        var porConcorrente = await vendas.ResumirPorConcorrenteAsync(ct);
        var registradas = porMotivo.Sum(f => f.Quantidade);

        var perdidosNoProcesso = (await processos.ResumirPerdasAsync(ct)).Sum(p => p.Quantidade);

        var ausentes = new List<MetricaSemDado>();

        if (registradas == 0 && perdidosNoProcesso > 0)
            ausentes.Add(new MetricaSemDado(
                "vendasPerdidasRegistradas",
                $"Há {perdidosNoProcesso} processo(s) perdido(s) no período e nenhum formulário de " +
                "venda perdida preenchido. O motivo, o concorrente e a diferença de preço só " +
                "existem quando o CEN preenche o formulário."));
        else if (perdidosNoProcesso > registradas)
            ausentes.Add(new MetricaSemDado(
                "vendasPerdidasSemFormulario",
                $"{perdidosNoProcesso - registradas} de {perdidosNoProcesso} processos perdidos não " +
                "têm formulário de venda perdida preenchido. A distribuição abaixo é das " +
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
                    registradas, perdidosNoProcesso, porMotivo, porConcorrente, ausentes),
                "processo.VendaPerdida",
                relogio));
    }
}

/// <summary>
/// O relatório de vendas perdidas como a tela o consome.
/// </summary>
/// <param name="Registradas">Quantos formulários de venda perdida foram preenchidos.</param>
/// <param name="ProcessosPerdidos">Quantos processos estão marcados como perdidos no período.</param>
/// <param name="PorMotivo">A distribuição por motivo.</param>
/// <param name="PorConcorrente">O ranking de para quem se perdeu.</param>
/// <param name="MetricasSemDado">O que estes números não dizem.</param>
public sealed record VendasPerdidasResumidas(
    int Registradas,
    int ProcessosPerdidos,
    IReadOnlyList<FatiaDeVendaPerdida> PorMotivo,
    IReadOnlyList<FatiaDeVendaPerdida> PorConcorrente,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

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

        // A RESSALVA MAIS IMPORTANTE DESTA TELA, e ela precede as outras: a data do último
        // contato é calculada das interações CARREGADAS, e a carga é o recorte do ano corrente.
        // Com cadência de 360 dias, isso muda a leitura de "nunca contatado": quem foi visitado
        // em 2025 e não em 2026 aparece aqui como se nunca tivesse sido procurado.
        if (painel.Clientes > 0)
            ausentes.Add(new MetricaSemDado(
                "historicoDeContato",
                "\"Nunca contatado\" quer dizer \"sem interação no recorte carregado\", que é o " +
                "ano corrente. Um cliente visitado em 2025 e ainda não em 2026 aparece nessa " +
                "coluna. Onde a cadência é de 360 dias, essa diferença muda a leitura — e ela " +
                "some quando a carga passar a trazer mais de um ano de histórico."));

        if (painel.FaturamentoDaCarteira > 0)
            ausentes.Add(new MetricaSemDado(
                "faturamentoDaCarteira",
                "O faturamento é dos CLIENTES da carteira, e não das vendas desta pessoa: a " +
                "origem não diz quem vendeu cada nota. Ele também para em 11/04/2025, quando a " +
                "integração com o ERP morreu."));

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
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<FaturamentoResumido>>> ExecutarAsync(CancellationToken ct)
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

        var serie = await repositorio.SerieMensalAsync(MesesDaSerie, ct);
        var top = await repositorio.TopClientesAsync(ClientesNoRanking, ct);

        // O ÚLTIMO MÊS DA SÉRIE, e ele pode estar ABERTO. Medido em 06/09/2026: setembro tinha
        // R$ 1,7 milhão contra R$ 15,9 milhões de agosto — não porque a empresa parou de vender,
        // mas porque o mês tinha seis dias. Mostrar esse número como "faturamento do mês" ao
        // lado dos onze meses cheios do gráfico erraria por 90%, e é exatamente o tipo de
        // comparação que faz uma diretoria decidir errado.
        var doUltimoMes = serie.LastOrDefault()?.ValorLiquido ?? 0m;
        var hoje = DateOnly.FromDateTime(relogio.Agora);
        var mesAindaAberto = ultima.Value.Year == hoje.Year && ultima.Value.Month == hoje.Month;

        if (mesAindaAberto)
            ausentes.Add(new MetricaSemDado(
                "mesEmCurso",
                $"{ultima.Value:MM/yyyy} ainda está em curso — o valor é do que já foi faturado " +
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
                new FaturamentoResumido(ultima, doUltimoMes, mesAindaAberto, serie, top, ausentes),
                "comercial.FaturamentoDoCliente", relogio));
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
