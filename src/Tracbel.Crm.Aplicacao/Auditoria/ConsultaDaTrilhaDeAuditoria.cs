using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Auditoria;

/// <summary>Um campo que mudou, como a tela mostra.</summary>
/// <param name="Campo">O nome da propriedade (<c>PerfilId</c>).</param>
/// <param name="Rotulo">O nome para a pessoa (Perfil).</param>
/// <param name="Antes">Como estava, já legível; nulo na inclusão.</param>
/// <param name="Depois">Como ficou, já legível; nulo na exclusão.</param>
public sealed record CampoDaAuditoria(string Campo, string Rotulo, string? Antes, string? Depois);

/// <summary>Um evento da trilha, como a tela mostra (issue 135).</summary>
/// <param name="Quando">O instante (UTC).</param>
/// <param name="Entidade">O código da entidade (<c>UsuarioPerfil</c>).</param>
/// <param name="EntidadeRotulo">O nome dela (Concessão de perfil).</param>
/// <param name="RegistroId">O identificador interno do registro.</param>
/// <param name="Registro">Como o registro se chama (Gerência para Maria Souza).</param>
/// <param name="Operacao">Inclusao, Alteracao ou Exclusao.</param>
/// <param name="Origem">Usuario, Integracao, Importacao, Sistema ou Job.</param>
/// <param name="Sistema">O sistema externo, quando veio de um.</param>
/// <param name="Autor">Quem gravou.</param>
/// <param name="AutorLogin">O e-mail de quem gravou.</param>
/// <param name="FilialCodigo">A filial da linha.</param>
/// <param name="FilialNome">O nome da filial.</param>
/// <param name="Correlacao">A requisição ou execução que causou a gravação.</param>
/// <param name="Campos">O que mudou.</param>
public sealed record EventoDaAuditoria(
    DateTime Quando, string Entidade, string EntidadeRotulo, long RegistroId, string Registro, string Operacao, string Origem,
    string? Sistema, string Autor, string AutorLogin, string FilialCodigo, string FilialNome, Guid? Correlacao,
    IReadOnlyList<CampoDaAuditoria> Campos);

/// <summary>Uma entidade que a trilha registra, para o filtro da tela.</summary>
/// <param name="Codigo">O código (<c>UsuarioPerfil</c>).</param>
/// <param name="Rotulo">O nome (Concessão de perfil).</param>
public sealed record EntidadeAuditada(string Codigo, string Rotulo);

/// <summary>
/// A TRILHA DE AUDITORIA NA TELA (issue 135): quem mudou o quê, quando, de quanto para quanto.
///
/// <para><b>O período é em dias de São Paulo</b> (<c>de</c> e <c>ate</c> em aaaa-mm-dd, os dois inclusos): é o dia que
/// a pessoa tem na cabeça quando procura "o que mudou ontem". Sem período, os últimos 30 dias; no máximo um ano por
/// consulta, para a tela não virar uma varredura da tabela inteira.</para>
///
/// <para><b>A fronteira é a de filial</b>, a mesma de todo dado com filial: a tela mostra a filial escolhida, e
/// "Todas as filiais" mostra todas. O que não tem filial (perfil, município) fica na filial de casa de quem
/// alterou.</para>
/// </summary>
public sealed class ConsultarTrilhaDeAuditoria(IRepositorioDaTrilhaDeAuditoria repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    private static readonly TimeSpan FusoDeSaoPaulo = TimeSpan.FromHours(-3);
    private const int DiasPorPadrao = 30;
    private const int DiasNoMaximo = 366;

    /// <summary>Executa a leitura.</summary>
    /// <param name="de">O primeiro dia (aaaa-mm-dd).</param>
    /// <param name="ate">O último dia (aaaa-mm-dd).</param>
    /// <param name="entidade">Só esta entidade.</param>
    /// <param name="registro">Só este registro da entidade.</param>
    /// <param name="autor">Trecho do nome ou do e-mail de quem alterou.</param>
    /// <param name="origem">Usuario, Integracao, Importacao, Sistema ou Job.</param>
    /// <param name="operacao">Inclusao, Alteracao ou Exclusao.</param>
    /// <param name="pagina">A página, a partir de 1.</param>
    /// <param name="tamanho">Eventos por página.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PaginaDe<EventoDaAuditoria>>>> ExecutarAsync(
        string? de, string? ate, string? entidade, long? registro, string? autor, string? origem, string? operacao,
        int? pagina, int? tamanho, CancellationToken ct)
    {
        if (!acesso.Atual.Tem(Permissoes.AuditoriaLer))
            return Resultado<ComProcedencia<PaginaDe<EventoDaAuditoria>>>.SemPermissao(
                $"Falta a permissão '{Permissoes.AuditoriaLer}' ({Permissoes.Catalogo[Permissoes.AuditoriaLer]}).");

        var erros = new ColetorDeErros();
        var hoje = DateOnly.FromDateTime(relogio.Agora + FusoDeSaoPaulo);
        var ultimoDia = LerDia(erros, "ate", ate) ?? hoje;
        var primeiroDia = LerDia(erros, "de", de) ?? ultimoDia.AddDays(-(DiasPorPadrao - 1));

        if (primeiroDia > ultimoDia)
            erros.Registrar("de", "O primeiro dia vem antes do último.", de);
        else if (ultimoDia.DayNumber - primeiroDia.DayNumber >= DiasNoMaximo)
            erros.Registrar("de", "Escolha um período de até um ano.", de);

        string? entidadeEscolhida = null;
        if (!string.IsNullOrWhiteSpace(entidade))
        {
            entidadeEscolhida = PoliticaDeAuditoria.Entidades.FirstOrDefault(e => string.Equals(e, entidade.Trim(), StringComparison.OrdinalIgnoreCase));
            if (entidadeEscolhida is null)
                erros.Registrar("entidade", $"A trilha não registra esta entidade. Opções: {string.Join(", ", PoliticaDeAuditoria.Entidades.Order(StringComparer.Ordinal))}.", entidade);
        }

        OrigemDaOperacao? origemEscolhida = string.IsNullOrWhiteSpace(origem) ? null : erros.ItemDeDominio<OrigemDaOperacao>("origem", origem);
        OperacaoAuditada? operacaoEscolhida = string.IsNullOrWhiteSpace(operacao) ? null : erros.ItemDeDominio<OperacaoAuditada>("operacao", operacao);

        var paginacao = Paginacao.Criar(pagina, tamanho);
        if (!paginacao.EhSucesso)
            return Resultado<ComProcedencia<PaginaDe<EventoDaAuditoria>>>.FalhaDeValidacao(paginacao.Erro!, paginacao.Erros);
        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PaginaDe<EventoDaAuditoria>>>("O filtro da trilha tem campos a corrigir.");

        var filtro = new FiltroDaTrilha(
            InicioDoDiaEmUtc(primeiroDia), InicioDoDiaEmUtc(ultimoDia.AddDays(1)), entidadeEscolhida, registro,
            string.IsNullOrWhiteSpace(autor) ? null : autor.Trim(), origemEscolhida, operacaoEscolhida, paginacao.Valor);

        var eventos = await repositorio.ListarAsync(filtro, ct);

        var referencias = eventos.Itens
            .SelectMany(e => e.Campos.SelectMany(c => new[] { c.Antes, c.Depois }.Select(valor => (e.Entidade, c.Campo, Valor: valor))))
            .Where(r => r.Valor is not null && RotulosDaTrilha.ReferenciaDe(r.Entidade, r.Campo) is not null)
            .Select(r => (Tipo: RotulosDaTrilha.ReferenciaDe(r.Entidade, r.Campo)!.Value, Id: long.TryParse(r.Valor, out var id) ? id : 0))
            .Where(r => r.Id > 0)
            .Distinct()
            .ToList();

        var nomes = await repositorio.ResolverReferenciasAsync(referencias, ct);
        var descricoes = await repositorio.DescreverRegistrosAsync(eventos.Itens.Select(e => (e.Entidade, e.RegistroId)).Distinct().ToList(), ct);

        var itens = eventos.Itens.Select(e => Montar(e, nomes, descricoes)).ToList();

        return Resultado<ComProcedencia<PaginaDe<EventoDaAuditoria>>>.Ok(
            ComProcedencia<PaginaDe<EventoDaAuditoria>>.DoNossoBanco(
                new PaginaDe<EventoDaAuditoria>(itens, eventos.Pagina, eventos.Tamanho, eventos.Total), "auditoria.AlteracaoDeCampo", relogio));
    }

    private static EventoDaAuditoria Montar(
        EventoNaTrilha e,
        IReadOnlyDictionary<(TipoDeReferencia Tipo, long Id), string> nomes,
        IReadOnlyDictionary<(string Entidade, long Id), string> descricoes)
    {
        var campos = e.Campos
            .Select(c => new CampoDaAuditoria(
                c.Campo, RotulosDaTrilha.DoCampo(e.Entidade, c.Campo),
                RotulosDaTrilha.Formatar(e.Entidade, c.Campo, c.Antes, nomes),
                RotulosDaTrilha.Formatar(e.Entidade, c.Campo, c.Depois, nomes)))
            .ToList();

        return new EventoDaAuditoria(
            e.Quando, e.Entidade, RotulosDaTrilha.DaEntidade(e.Entidade), e.RegistroId,
            descricoes.TryGetValue((e.Entidade, e.RegistroId), out var descricao) ? descricao : DescreverPelosCampos(e, nomes),
            e.Operacao.ToString(), e.Origem.ToString(), e.Sistema, e.Autor, e.AutorLogin, e.FilialCodigo, e.FilialNome, e.Correlacao, campos);
    }

    /// <summary>
    /// O REGISTRO QUE JÁ NÃO EXISTE se descreve pelo que a própria trilha guardou: a permissão tirada de um perfil
    /// apaga a linha, mas a exclusão gravou o perfil e o código.
    /// </summary>
    private static string DescreverPelosCampos(EventoNaTrilha e, IReadOnlyDictionary<(TipoDeReferencia Tipo, long Id), string> nomes)
    {
        string? Bruto(string campo) => e.Campos.FirstOrDefault(c => c.Campo == campo) is { } c ? c.Depois ?? c.Antes : null;
        string? Nome(string campo) => RotulosDaTrilha.Formatar(e.Entidade, campo, Bruto(campo), nomes);

        return e.Entidade switch
        {
            "PerfilPermissao" when Bruto("CodigoPermissao") is { } codigo => Nome("PerfilId") is { } perfil ? $"{codigo} em {perfil}" : codigo,
            "UsuarioPerfil" when Nome("PerfilId") is { } perfil && Nome("UsuarioId") is { } pessoa => $"{perfil} para {pessoa}",
            _ => $"nº {e.RegistroId.ToString(CultureInfo.InvariantCulture)}"
        };
    }

    private static DateTime InicioDoDiaEmUtc(DateOnly dia) =>
        DateTime.SpecifyKind(dia.ToDateTime(TimeOnly.MinValue) - FusoDeSaoPaulo, DateTimeKind.Utc);

    private static DateOnly? LerDia(ColetorDeErros erros, string campo, string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        if (DateOnly.TryParseExact(texto.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dia)) return dia;

        erros.Registrar(campo, "Use a data no formato aaaa-mm-dd, por exemplo 2026-09-22.", texto);
        return null;
    }
}

/// <summary>As entidades que a trilha registra, com o nome de cada uma, para o filtro da tela.</summary>
public sealed class ListarEntidadesAuditadas(IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>Executa a leitura.</summary>
    public Resultado<ComProcedencia<IReadOnlyList<EntidadeAuditada>>> Executar()
    {
        if (!acesso.Atual.Tem(Permissoes.AuditoriaLer))
            return Resultado<ComProcedencia<IReadOnlyList<EntidadeAuditada>>>.SemPermissao(
                $"Falta a permissão '{Permissoes.AuditoriaLer}' ({Permissoes.Catalogo[Permissoes.AuditoriaLer]}).");

        IReadOnlyList<EntidadeAuditada> entidades = PoliticaDeAuditoria.Entidades
            .Select(e => new EntidadeAuditada(e, RotulosDaTrilha.DaEntidade(e)))
            .OrderBy(e => e.Rotulo, StringComparer.Create(new CultureInfo("pt-BR"), ignoreCase: true))
            .ToList();

        return Resultado<ComProcedencia<IReadOnlyList<EntidadeAuditada>>>.Ok(
            ComProcedencia<IReadOnlyList<EntidadeAuditada>>.DoNossoBanco(entidades, "PoliticaDeAuditoria (código)", relogio));
    }
}
