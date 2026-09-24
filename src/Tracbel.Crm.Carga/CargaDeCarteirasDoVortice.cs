using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Carga;
using Tracbel.Crm.Integracao.Protheus;
using Tracbel.Crm.Integracao.Saneamento;
using Tracbel.Crm.Integracao.Vortice;

namespace Tracbel.Crm.Carga;

/// <summary>
/// Os códigos de pendência de um vínculo de carteira do Vórtice que não entrou — gravados em
/// <c>integracao.RegistroDeOrigem.Motivos</c>, separados por vírgula, como os do ART.
/// </summary>
internal static class MotivoDePendenciaDaCarteira
{
    /// <summary>O cliente não tem documento no Vórtice (<c>NroCGCCPF</c> nulo).</summary>
    public const string SemDocumento = "CLIENTE_SEM_DOCUMENTO_NO_VORTICE";

    /// <summary>O documento do cliente no Vórtice é zero.</summary>
    public const string DocumentoZerado = "DOCUMENTO_ZERADO_NO_VORTICE";

    /// <summary>O documento não se recompõe num CPF/CNPJ com dígito verificador válido.</summary>
    public const string DocumentoInvalido = "DOCUMENTO_INVALIDO_NO_VORTICE";

    /// <summary>Documento válido sem cliente no CRM — sem o cadastro do Protheus para dizer por quê.</summary>
    public const string ClienteAusenteDoCrm = "CLIENTE_AUSENTE_DO_CRM";

    /// <summary>Documento válido que não existe na SA1 do Protheus: não é cliente cadastrado na Tracbel.</summary>
    public const string ClienteAusenteDaSa1 = "CLIENTE_AUSENTE_DA_SA1";

    /// <summary>O cliente existe na SA1, mas o município dele está fora da área de atuação.</summary>
    public const string ClienteForaDaAreaDeAtuacao = "CLIENTE_FORA_DA_AREA_DE_ATUACAO";

    /// <summary>O cliente existe na SA1, mas o município dele não é reconhecido no catálogo do IBGE.</summary>
    public const string ClienteComMunicipioNaoReconhecido = "CLIENTE_COM_MUNICIPIO_NAO_RECONHECIDO_NA_SA1";

    /// <summary>O cliente existe na SA1, mas sem UF.</summary>
    public const string ClienteSemUfNaSa1 = "CLIENTE_SEM_UF_NA_SA1";

    /// <summary>A carga da SA1 criaria o cliente, mas ainda não rodou (ou rodou antes de ele entrar na SA1).</summary>
    public const string ClienteDaSa1AindaNaoCarregado = "CLIENTE_DA_SA1_AINDA_NAO_CARREGADO";

    /// <summary>O único cliente do CRM com o documento está excluído.</summary>
    public const string ClienteExcluidoNoCrm = "CLIENTE_EXCLUIDO_NO_CRM";

    /// <summary>Mais de um cliente do CRM com o documento.</summary>
    public const string ClienteAmbiguoNoCrm = "CLIENTE_AMBIGUO_NO_CRM";

    /// <summary>A carteira é de vendedor <c>DESLIGADO</c> — o BI a exclui, e aqui ela também não entra.</summary>
    public const string CarteiraDeVendedorDesligado = "CARTEIRA_DE_VENDEDOR_DESLIGADO";

    /// <summary>A filial da carteira no Vórtice não é uma filial ativa do CRM.</summary>
    public const string FilialDaCarteiraForaDoCrm = "FILIAL_DA_CARTEIRA_FORA_DO_CRM";

    /// <summary>A carteira não declara vendedor com usuário nem usuário responsável.</summary>
    public const string CarteiraSemResponsavel = "CARTEIRA_SEM_RESPONSAVEL";

    /// <summary>O login do dono não forma um nome principal aceitável.</summary>
    public const string ResponsavelComLoginInvalido = "RESPONSAVEL_COM_LOGIN_INVALIDO";

    /// <summary>A conta do dono no CRM foi excluída — a carga não a ressuscita.</summary>
    public const string ResponsavelExcluidoNoCrm = "RESPONSAVEL_EXCLUIDO_NO_CRM";

    /// <summary>Mais de uma conta do CRM casa com o login do dono.</summary>
    public const string ResponsavelAmbiguoNoCrm = "RESPONSAVEL_AMBIGUO_NO_CRM";

    /// <summary>A carteira foi excluída no CRM — a carga não a ressuscita.</summary>
    public const string CarteiraExcluidaNoCrm = "CARTEIRA_EXCLUIDA_NO_CRM";

    /// <summary>O vínculo aponta para uma carteira que o próprio Vórtice não tem.</summary>
    public const string CarteiraInexistenteNoVortice = "CARTEIRA_INEXISTENTE_NO_VORTICE";

    /// <summary>
    /// A carteira é resíduo de teste (<c>TESTE_*</c>, <c>TI_RPO</c> da consultoria) — decisão de 24/09/2026: não
    /// entra, e o dono dela não ganha conta.
    /// </summary>
    public const string CarteiraDeTesteNaoCarregada = "CARTEIRA_DE_TESTE_NAO_CARREGADA";
}

/// <summary>O que a sincronia das carteiras fez (ou faria, na simulação), em número — sem nome nem documento.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDaSincroniaDeCarteiras(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo, em qualquer etapa.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// A SINCRONIA DAS CARTEIRAS MAQ_NOVOS DO VÓRTICE (decisão de 24/09/2026).
///
/// <para><b>O que entra.</b> Todas as carteiras do departamento <c>MAQ-NOVOS</c> — o vendedor de campo
/// (<c>MAQ_*</c>), o pool da Inteligência de Mercado (<c>TBA_*</c>) e o digital (<c>DGT_*</c>). A única carteira que
/// fica de fora por ser o que é, é o resíduo de TESTE (decisão de 24/09/2026) — pendente, com o motivo, e sem conta
/// para o dono-robô. O que distingue as outras são as noções que o modelo já tem, pela MESMA regra da carga antiga
/// (<see cref="ClassificacaoDeNatureza"/>): a natureza da carteira (comercial, administrativa, teste) e a natureza
/// do dono (pessoa, departamento…). O pool do INT.MERCADO aparece como carteira de ÁREA; o código de origem
/// (<c>MAQ_</c>, <c>TBA_</c>, <c>DGT_</c>) continua no código da carteira.</para>
///
/// <para><b>O dono é o vendedor</b>, como no BI: o usuário do vendedor da carteira; sem vendedor, o usuário
/// responsável. Quem não tem conta no CRM ganha uma AGUARDANDO LIBERAÇÃO, sem perfil — o mesmo estado da issue 128
/// — que casa com o login do Entra ID pelo <c>nome.sobrenome</c>.</para>
///
/// <para><b>O cliente é casado pelo documento</b>, e só por ele: CPF/CNPJ recomposto do Vórtice contra o
/// <c>comercial.Cliente</c> que a carga da SA1 criou. O de-para antigo de <c>integracao.ChaveExterna</c> foi
/// apagado na sanitização e não é usado para cliente. Esta carga NÃO cria cliente: o vínculo que não casa fica
/// pendente em <c>integracao.RegistroDeOrigem</c>, com o motivo — nunca descartado em silêncio.</para>
///
/// <para><b>É SINCRONIA.</b> O Vórtice muda todo dia. Cada rodada compara o que a origem declara com o que o CRM
/// tem nas carteiras que esta sincronia administra: vínculo novo entra; vínculo que sumiu, ou cujo cliente mudou
/// de carteira, é ENCERRADO (<c>DesvinculadoEm</c>), nunca apagado; nome e dono da carteira acompanham a origem.
/// Rodar de novo sem mudança na origem não grava vínculo nenhum.</para>
///
/// <para><b>Planejar, depois gravar.</b> A rodada inteira é calculada primeiro, SÓ COM LEITURA — do Vórtice, do
/// CRM e, se houver, da SA1. A simulação para aí: não abre transação de escrita, e os números que ela mostra são
/// os do plano que a gravação executaria.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="lerVortice">A leitura das carteiras do Vórtice.</param>
/// <param name="lerSa1">A leitura da SA1, quando o banco do Protheus está configurado; nula quando não está.</param>
/// <param name="deParaDeFiliais">A filial do Vórtice (<c>NN</c>) para a filial ativa do CRM (<c>0101NN</c>).</param>
/// <param name="usuarioId">Quem roda a sincronia.</param>
/// <param name="relogio">O relógio (UTC).</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDeCarteirasDoVortice(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<LeituraDasCarteirasDoVortice>>> lerVortice,
    Func<CancellationToken, Task<Resultado<IReadOnlyList<LojaDeClienteNoProtheus>>>>? lerSa1,
    IReadOnlyDictionary<int, int> deParaDeFiliais,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>O fluxo na trilha de <c>integracao.RegistroDeOrigem</c> — um registro por vínculo da origem.</summary>
    internal const string Fluxo = "VORTICE.CARTEIRA_MAQ_NOVOS";

    /// <summary>O código da linha de negócio no CRM — o <c>MAQ-NOVOS</c> do Vórtice, com sublinhado.</summary>
    internal static readonly string CodigoDaLinha = SaneamentoDeProcesso.Codificar(LeitorDeCarteirasDoVortice.DepartamentoDeMaquinasNovas, 20);

    /// <summary>Rótulo: vínculos que entraram (ou entrariam) em carteira.</summary>
    internal const string RotuloDeCasados = "vínculos casados com cliente do CRM";

    /// <summary>Rótulo: vínculos pendentes.</summary>
    internal const string RotuloDePendentes = "vínculos pendentes (ao menos um motivo)";

    /// <summary>Rótulo: vínculos cliente × carteira a criar.</summary>
    internal const string RotuloDeVinculosCriados = "vínculos cliente × carteira incluídos";

    /// <summary>Rótulo: vínculos cliente × carteira encerrados.</summary>
    internal const string RotuloDeVinculosEncerrados = "vínculos cliente × carteira encerrados (DesvinculadoEm)";

    /// <summary>Rótulo: vínculos mantidos.</summary>
    internal const string RotuloDeVinculosMantidos = "vínculos cliente × carteira mantidos sem alteração";

    /// <summary>Rótulo: contas criadas aguardando liberação.</summary>
    internal const string RotuloDeDonosCriados = "donos sem conta no CRM: conta criada AGUARDANDO LIBERAÇÃO, sem perfil";

    /// <summary>Rótulo: carteiras criadas.</summary>
    internal const string RotuloDeCarteirasCriadas = "carteiras incluídas";

    /// <summary>Rótulo: carteiras atualizadas.</summary>
    internal const string RotuloDeCarteirasAlteradas = "carteiras já existentes com nome ou dono atualizados";

    private const string EntidadeCarteira = nameof(Carteira);
    private const string EntidadeUsuario = nameof(Usuario);

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>Executa a sincronia.</summary>
    /// <param name="simular">Só planeja e conta: não abre transação de escrita.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDaSincroniaDeCarteiras>> ExecutarAsync(bool simular, CancellationToken ct)
    {
        relatar($"Lendo as carteiras {LeitorDeCarteirasDoVortice.DepartamentoDeMaquinasNovas} do Vórtice (sessão somente leitura)…");
        var lido = await lerVortice(ct);
        if (!lido.EhSucesso) return Resultado<RelatorioDaSincroniaDeCarteiras>.Indisponivel(lido.Erro!);

        var leitura = lido.Valor;

        // A ORIGEM VAZIA NÃO É UMA ORIGEM SEM CLIENTES: é uma leitura que falhou sem dizer. Sincronizar contra ela
        // encerraria todos os vínculos do CRM de uma vez — e o custo de errar para esse lado é a carteira inteira.
        if (leitura.Vinculos.Count == 0)
            return Resultado<RelatorioDaSincroniaDeCarteiras>.Indisponivel(
                $"O Vórtice devolveu o departamento {leitura.Departamento.Depto} sem vínculo nenhum. Isso não é uma " +
                "carteira vazia, é uma leitura a conferir — nada foi encerrado e nada foi gravado.");

        IReadOnlyDictionary<string, LojaDeClienteNoProtheus>? sa1 = null;
        if (lerSa1 is not null)
        {
            relatar("Lendo o cadastro de clientes do Protheus (SA1010, sessão somente leitura) para explicar as pendências…");
            var lojas = await lerSa1(ct);
            if (lojas.EhSucesso)
            {
                sa1 = lojas.Valor
                    .GroupBy(l => l.Documento, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, CargaDeClientesDoProtheus.EscolherAPrincipal, StringComparer.Ordinal);
            }
            else
            {
                // O PROTHEUS É OPCIONAL AQUI: ele só refina o motivo da pendência. Parar a sincronia porque o ERP
                // não respondeu deixaria a carteira de ontem no ar por um detalhe de explicação.
                _observacoes.Add("O cadastro do Protheus não respondeu; o vínculo sem cliente no CRM fica com o motivo " +
                                 $"genérico {MotivoDePendenciaDaCarteira.ClienteAusenteDoCrm} nesta rodada. ({lojas.Erro})");
            }
        }

        var agora = relogio();
        Plano plano;
        await using (var leituraDoCrm = abrirContexto())
        {
            plano = await PlanejarAsync(leituraDoCrm, leitura, sa1, ct);
        }

        Relatar(leitura, plano, sa1 is not null);

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura, e nenhuma transação de escrita foi aberta.");
            return Resultado<RelatorioDaSincroniaDeCarteiras>.Ok(new RelatorioDaSincroniaDeCarteiras(true, _contagens, _observacoes));
        }

        await AplicarAsync(leitura, plano, agora, ct);
        return Resultado<RelatorioDaSincroniaDeCarteiras>.Ok(new RelatorioDaSincroniaDeCarteiras(false, _contagens, _observacoes));
    }

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private async Task<Plano> PlanejarAsync(
        CrmDbContext banco, LeituraDasCarteirasDoVortice leitura, IReadOnlyDictionary<string, LojaDeClienteNoProtheus>? sa1,
        CancellationToken ct)
    {
        var plano = new Plano();
        var departamento = leitura.Departamento;

        plano.SistemaId = await banco.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == LeitorDeCargaDoVortice.CodigoDoSistema)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(ct);

        // ---------------------------------------------------------------------------------------------
        // A linha de negócio, com a cadência que o Vórtice declara.
        // ---------------------------------------------------------------------------------------------
        var linha = await banco.LinhasDeNegocio.AsNoTracking().FirstOrDefaultAsync(l => l.Codigo == CodigoDaLinha, ct);
        plano.LinhaId = linha?.Id;
        plano.NomeDaLinha = linha?.Nome ?? departamento.Descricao ?? departamento.Depto;
        plano.DeclararCadencia = linha is null
            || linha.DiasCicloClasseA != departamento.CicloA || linha.DiasCicloClasseB != departamento.CicloB
            || linha.DiasCicloClasseC != departamento.CicloC || linha.DiasCicloClasseD != departamento.CicloD;

        // ---------------------------------------------------------------------------------------------
        // O que o CRM já tem.
        // ---------------------------------------------------------------------------------------------
        var chavesDeCarteira = new Dictionary<string, long>(StringComparer.Ordinal);
        var chavesDeUsuario = new Dictionary<string, long>(StringComparer.Ordinal);
        var registros = new Dictionary<string, (string Hash, DecisaoDaIntegracao Decisao, string? Motivos, DateTime? Ausente)>(StringComparer.Ordinal);

        if (plano.SistemaId is { } sistemaId)
        {
            foreach (var c in await banco.ChavesExternas.AsNoTracking()
                         .Where(c => c.SistemaId == sistemaId && (c.Entidade == EntidadeCarteira || c.Entidade == EntidadeUsuario))
                         .Select(c => new { c.Entidade, c.ChaveOrigem, c.RegistroId })
                         .ToListAsync(ct))
                (c.Entidade == EntidadeCarteira ? chavesDeCarteira : chavesDeUsuario)[c.ChaveOrigem] = c.RegistroId;

            foreach (var r in await banco.RegistrosDeOrigem.AsNoTracking()
                         .Where(r => r.SistemaId == sistemaId && r.Fluxo == Fluxo)
                         .Select(r => new { r.ChaveOrigem, r.HashDoConteudo, r.Decisao, r.Motivos, r.AusenteNaOrigemDesde })
                         .ToListAsync(ct))
                registros[r.ChaveOrigem] = (r.HashDoConteudo, r.Decisao, r.Motivos, r.AusenteNaOrigemDesde);
        }

        var usuarios = await banco.Usuarios.AsNoTracking()
            .Select(u => new ContaNoCrm(u.Id, u.NomePrincipal, u.ExcluidoEm != null))
            .ToListAsync(ct);
        var contaPorId = usuarios.ToDictionary(u => u.Id);
        var contasPorLogin = usuarios
            .GroupBy(u => ParteLocal(u.NomePrincipal), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var carteirasDoCrm = await banco.Carteiras.AsNoTracking()
            .Select(k => new CarteiraNoCrm(k.Id, k.Codigo, k.Nome, k.EmpresaId, k.LinhaDeNegocioId, k.ResponsavelId, k.ExcluidoEm != null))
            .ToListAsync(ct);
        var carteiraPorId = carteirasDoCrm.ToDictionary(k => k.Id);
        var carteiraPorCodigo = carteirasDoCrm.ToDictionary(k => k.Codigo, StringComparer.OrdinalIgnoreCase);

        // ---------------------------------------------------------------------------------------------
        // As carteiras, e o dono de cada uma.
        // ---------------------------------------------------------------------------------------------
        foreach (var origem in leitura.Carteiras)
        {
            var sigla = SaneamentoDeProcesso.Codificar(origem.Codigo, 30);

            // O CÓDIGO É O DA CARGA ANTIGA (sigla + identificador da origem): estável, único mesmo quando duas
            // carteiras têm a mesma sigla, e legível — MAQ_13SJRP_01_282 diz de onde veio.
            var carteira = new PlanoDeCarteira
            {
                Origem = origem,
                Codigo = $"{(sigla.Length == 0 ? "CART" : sigla)}_{origem.SeqCarteira.ToString(CultureInfo.InvariantCulture)}",
                Nome = origem.Descricao ?? origem.Codigo,
                Prefixo = Prefixo(origem.Codigo)
            };
            plano.Carteiras[origem.SeqCarteira] = carteira;

            // A CARTEIRA É REENCONTRADA pelo de-para gravado (o identificador do Vórtice) e, sem ele, pelo código — o
            // caso de um banco em que o de-para foi apagado e a carteira não.
            var chave = origem.SeqCarteira.ToString(CultureInfo.InvariantCulture);
            var existente = chavesDeCarteira.TryGetValue(chave, out var porChave) && carteiraPorId.TryGetValue(porChave, out var achada)
                ? achada
                : carteiraPorCodigo.GetValueOrDefault(carteira.Codigo);

            if (existente is not null)
            {
                carteira.CarteiraId = existente.Id;
                plano.CarteirasGeridas.Add(existente.Id);
            }

            // O QUE TIRA A CARTEIRA INTEIRA, na ordem em que a pergunta se faz.
            if (origem.VendedorDesligado)
            {
                carteira.Motivo = MotivoDePendenciaDaCarteira.CarteiraDeVendedorDesligado;
                continue;
            }

            if (!deParaDeFiliais.TryGetValue(origem.NroEmpresa, out var empresaId))
            {
                carteira.Motivo = MotivoDePendenciaDaCarteira.FilialDaCarteiraForaDoCrm;
                continue;
            }

            carteira.EmpresaId = empresaId;

            if (existente is { Excluida: true })
            {
                carteira.Motivo = MotivoDePendenciaDaCarteira.CarteiraExcluidaNoCrm;
                continue;
            }

            if (existente is not null && existente.EmpresaId != empresaId) plano.CarteirasQueMudaramDeFilial++;

            if (origem.SeqUsuarioDono is not { } seqDono || !leitura.Donos.TryGetValue(seqDono, out var donoNaOrigem))
            {
                carteira.Motivo = MotivoDePendenciaDaCarteira.CarteiraSemResponsavel;
                continue;
            }

            // A NATUREZA PELA REGRA DA CARGA ANTIGA. Ela é gravada só no nascimento — depois disso é leitura que o
            // negócio corrige pela tela, e a sincronia não desfaz a correção de ninguém.
            var decisoes = new List<CorrecaoAplicada>();
            carteira.Natureza = ClassificacaoDeNatureza.DaCarteira(carteira.Nome, plano.NomeDaLinha, donoNaOrigem.Nome, decisoes);

            // O RESÍDUO DE TESTE NÃO ENTRA (decisão de 24/09/2026), e isso vem ANTES do dono: a conta de um robô de
            // teste (NETO.APP, VORTICE.APP) ou da consultoria que só responde por teste não tem por que existir.
            if (carteira.Natureza == NaturezaDaCarteira.Teste)
            {
                carteira.Motivo = MotivoDePendenciaDaCarteira.CarteiraDeTesteNaoCarregada;
                continue;
            }

            if (!plano.Donos.TryGetValue(seqDono, out var dono))
            {
                dono = ResolverDono(donoNaOrigem, chavesDeUsuario, contaPorId, contasPorLogin);
                plano.Donos[seqDono] = dono;
            }

            if (dono.Motivo is { } motivoDoDono)
            {
                carteira.Motivo = motivoDoDono;
                continue;
            }

            carteira.Dono = dono;
            dono.Carteiras.Add(carteira);

            if (existente is not null)
            {
                carteira.Alterar = !string.Equals(existente.Nome, carteira.Nome, StringComparison.Ordinal)
                                   || dono.UsuarioId != existente.ResponsavelId
                                   || plano.LinhaId != existente.LinhaDeNegocioId;
            }
        }

        // A FILIAL PROVISÓRIA DE QUEM AINDA NÃO TEM CONTA é a das carteiras dele — a mais frequente, desempatada pela
        // menor. Não dá acesso a nada (a conta espera liberação); só diz ao administrador de onde a pessoa é.
        foreach (var dono in plano.Donos.Values.Where(d => d.Motivo is null && d.UsuarioId is null))
        {
            dono.FilialProvisoriaId = dono.Carteiras
                .GroupBy(c => c.EmpresaId)
                .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
                .First().Key;
        }

        // ---------------------------------------------------------------------------------------------
        // Os clientes do CRM, pelo documento.
        // ---------------------------------------------------------------------------------------------
        var clientes = await banco.Clientes.AsNoTracking()
            .Where(c => c.Documento != null)
            .Select(c => new { c.Id, c.Documento, Excluido = c.ExcluidoEm != null })
            .ToListAsync(ct);
        var clientesPorDocumento = clientes
            .GroupBy(c => c.Documento!.Value.Numero, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(c => (c.Id, c.Excluido)).ToList(), StringComparer.Ordinal);
        plano.ClientesNoCrm = clientes.Count(c => !c.Excluido);

        TerritorioDaCargaDeClientes? territorio = sa1 is null ? null : await TerritorioDaCargaDeClientes.LerAsync(banco, ct);

        // ---------------------------------------------------------------------------------------------
        // Cada vínculo da origem: casa, ou fica pendente com os motivos.
        // ---------------------------------------------------------------------------------------------
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        foreach (var vinculo in leitura.Vinculos)
        {
            if (!vistos.Add(vinculo.Chave)) continue;

            plano.Carteiras.TryGetValue(vinculo.SeqCarteira, out var carteira);
            var motivos = new List<string>(2);
            if (carteira is null) motivos.Add(MotivoDePendenciaDaCarteira.CarteiraInexistenteNoVortice);
            else if (carteira.Motivo is { } motivoDaCarteira) motivos.Add(motivoDaCarteira);

            long? clienteId = null;
            var casariaDepoisDaSa1 = false;
            switch (vinculo.Documento.Situacao)
            {
                case SituacaoDoDocumentoNoVortice.SemDocumento: motivos.Add(MotivoDePendenciaDaCarteira.SemDocumento); break;
                case SituacaoDoDocumentoNoVortice.Zerado: motivos.Add(MotivoDePendenciaDaCarteira.DocumentoZerado); break;
                case SituacaoDoDocumentoNoVortice.Invalido: motivos.Add(MotivoDePendenciaDaCarteira.DocumentoInvalido); break;
                default:
                {
                    var documento = vinculo.Documento.Numero!;
                    var ativos = clientesPorDocumento.TryGetValue(documento, out var comDocumento)
                        ? comDocumento.Where(c => !c.Excluido).ToList()
                        : [];

                    if (ativos.Count == 1) clienteId = ativos[0].Id;
                    else if (ativos.Count > 1) motivos.Add(MotivoDePendenciaDaCarteira.ClienteAmbiguoNoCrm);
                    else if (comDocumento is { Count: > 0 }) motivos.Add(MotivoDePendenciaDaCarteira.ClienteExcluidoNoCrm);
                    else
                    {
                        var motivo = PorQueNaoEstaNoCrm(documento, sa1, territorio);
                        casariaDepoisDaSa1 = motivo == MotivoDePendenciaDaCarteira.ClienteDaSa1AindaNaoCarregado;
                        motivos.Add(motivo);
                    }

                    break;
                }
            }

            var decisao = new DecisaoDoVinculo(vinculo, carteira, motivos.Count == 0 ? clienteId : null, motivos,
                clienteId is not null || casariaDepoisDaSa1);
            plano.Vinculos.Add(decisao);

            if (decisao.ClienteId is { } casado)
            {
                var par = (casado, vinculo.SeqCarteira);
                if (plano.Desejados.TryGetValue(par, out var desde))
                {
                    plano.ParesRepetidosNaOrigem++;
                    if (vinculo.IncluidoEmUtc < desde) plano.Desejados[par] = vinculo.IncluidoEmUtc;
                }
                else
                {
                    plano.Desejados[par] = vinculo.IncluidoEmUtc;
                }
            }

            // A TRILHA DA ORIGEM, planejada: o que é novo, o que mudou, o que ficou igual.
            var hash = Resumo(vinculo);
            if (!registros.TryGetValue(vinculo.Chave, out var registro)) plano.RegistrosNovos++;
            else if (!string.Equals(registro.Hash, hash, StringComparison.Ordinal)) plano.RegistrosAlterados++;
            else plano.RegistrosIguais++;

            var decisaoNova = decisao.ClienteId is null ? DecisaoDaIntegracao.Pendente : DecisaoDaIntegracao.Importado;
            if (registros.ContainsKey(vinculo.Chave) && (registro.Decisao != decisaoNova || registro.Motivos != decisao.MotivosEmTexto))
                plano.RegistrosComDecisaoAlterada++;
        }

        plano.RegistrosQueSumiram = registros.Count(r => !vistos.Contains(r.Key) && r.Value.Ausente is null);

        // ---------------------------------------------------------------------------------------------
        // O que muda na carteirização: o desejado contra o vigente, só nas carteiras que esta sincronia administra.
        // ---------------------------------------------------------------------------------------------
        var gerida = plano.CarteirasGeridas.ToList();
        var vigentes = await banco.ClienteCarteiras.AsNoTracking()
            .Where(v => v.DesvinculadoEm == null && gerida.Contains(v.CarteiraId))
            .Select(v => new { v.Id, v.ClienteId, v.CarteiraId })
            .ToListAsync(ct);

        var seqPorCarteiraId = plano.Carteiras.Values.Where(c => c.CarteiraId is not null)
            .ToDictionary(c => c.CarteiraId!.Value, c => c.Origem.SeqCarteira);
        var vigentesPorPar = new HashSet<(long, int)>();

        foreach (var v in vigentes)
        {
            // CARTEIRA ADMINISTRADA QUE SAIU DA LEITURA (perdeu o departamento e os clientes): todos os vínculos dela
            // saem, porque a origem deixou de declará-los.
            if (!seqPorCarteiraId.TryGetValue(v.CarteiraId, out var seq) || !plano.Desejados.ContainsKey((v.ClienteId, seq)))
            {
                plano.AEncerrar.Add(v.Id);
                plano.ClientesQueSairam.Add(v.ClienteId);
                continue;
            }

            vigentesPorPar.Add((v.ClienteId, seq));
        }

        foreach (var (par, desde) in plano.Desejados)
        {
            if (vigentesPorPar.Contains(par)) { plano.Mantidos++; continue; }
            plano.ACriar.Add((par.ClienteId, par.SeqCarteira, desde));
        }

        plano.ClientesQueMudaramDeCarteira = plano.ACriar.Select(a => a.ClienteId).Distinct().Count(plano.ClientesQueSairam.Contains);
        return plano;
    }

    /// <summary>
    /// ACHA A CONTA DO DONO NO CRM, ou decide criá-la. A ordem é a do vínculo mais firme para o mais frágil:
    /// o de-para gravado (o identificador do Vórtice), depois o login — a parte antes do <c>@</c>, que é o mesmo
    /// casamento que o Entra ID faz no primeiro login.
    /// </summary>
    private static PlanoDeDono ResolverDono(
        DonoNoVortice origem, IReadOnlyDictionary<string, long> chavesDeUsuario,
        IReadOnlyDictionary<long, ContaNoCrm> contaPorId, IReadOnlyDictionary<string, List<ContaNoCrm>> contasPorLogin)
    {
        var decisoes = new List<CorrecaoAplicada>();
        var dono = new PlanoDeDono
        {
            Origem = origem,
            Natureza = ClassificacaoDeNatureza.DoUsuario(origem.Nome, origem.Login, decisoes)
        };

        var login = origem.Login.Trim().ToLowerInvariant();
        if (login.Length == 0 || login.Contains('@', StringComparison.Ordinal)
            || !Email.TentarCriar(login + Usuario.SufixoSemEmail, out _) || (login + Usuario.SufixoSemEmail).Length > 200)
        {
            dono.Motivo = MotivoDePendenciaDaCarteira.ResponsavelComLoginInvalido;
            return dono;
        }

        if (chavesDeUsuario.TryGetValue(origem.SeqUsuario.ToString(CultureInfo.InvariantCulture), out var porChave)
            && contaPorId.TryGetValue(porChave, out var pelaChave))
        {
            if (pelaChave.Excluida) dono.Motivo = MotivoDePendenciaDaCarteira.ResponsavelExcluidoNoCrm;
            else dono.UsuarioId = pelaChave.Id;
            dono.ComoAchou = "de-para";
            return dono;
        }

        if (!contasPorLogin.TryGetValue(login, out var candidatas) || candidatas.Count == 0)
        {
            dono.ComoAchou = "nova";
            return dono;
        }

        // O NOME PRINCIPAL É ÚNICO, inclusive entre excluídas — por isso a excluída com o mesmo login barra a
        // criação: a conta nova bateria no índice. Quem decide é uma pessoa, não a carga.
        var ativas = candidatas.Where(c => !c.Excluida).ToList();
        if (ativas.Count == 1)
        {
            dono.UsuarioId = ativas[0].Id;
            dono.ComoAchou = "login";
        }
        else if (ativas.Count > 1) dono.Motivo = MotivoDePendenciaDaCarteira.ResponsavelAmbiguoNoCrm;
        else dono.Motivo = MotivoDePendenciaDaCarteira.ResponsavelExcluidoNoCrm;

        return dono;
    }

    /// <summary>
    /// POR QUE UM DOCUMENTO VÁLIDO NÃO TEM CLIENTE NO CRM. Com a SA1 à mão, a resposta é a da própria carga da SA1
    /// (<see cref="TerritorioDaCargaDeClientes.Situar"/>): fora da área de atuação, município não reconhecido, sem
    /// UF — ou "a carga criaria, mas ainda não rodou".
    /// </summary>
    private static string PorQueNaoEstaNoCrm(
        string documento, IReadOnlyDictionary<string, LojaDeClienteNoProtheus>? sa1, TerritorioDaCargaDeClientes? territorio)
    {
        if (sa1 is null || territorio is null) return MotivoDePendenciaDaCarteira.ClienteAusenteDoCrm;
        if (!sa1.TryGetValue(documento, out var loja)) return MotivoDePendenciaDaCarteira.ClienteAusenteDaSa1;

        return territorio.Situar(documento, loja).Motivo switch
        {
            null => MotivoDePendenciaDaCarteira.ClienteDaSa1AindaNaoCarregado,
            CargaDeClientesDoProtheus.ForaDaAreaDeAtuacao => MotivoDePendenciaDaCarteira.ClienteForaDaAreaDeAtuacao,
            CargaDeClientesDoProtheus.MunicipioNaoReconhecido => MotivoDePendenciaDaCarteira.ClienteComMunicipioNaoReconhecido,
            CargaDeClientesDoProtheus.SemUf => MotivoDePendenciaDaCarteira.ClienteSemUfNaSa1,
            _ => MotivoDePendenciaDaCarteira.ClienteAusenteDoCrm
        };
    }

    // =============================================================================================
    // A gravação — uma transação, executando o plano
    // =============================================================================================

    private async Task AplicarAsync(LeituraDasCarteirasDoVortice leitura, Plano plano, DateTime agora, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeCargaDoVortice.CodigoDoSistema, "Vórtice CRM (sistema legado)", "SQL Server, somente leitura", ct);

        // O QUE A SINCRONIA MUDA NUM REGISTRO QUE JÁ EXISTIA vai para a trilha como integração do Vórtice; o que
        // nasce agora tem o rastro em integracao.RegistroDeOrigem e integracao.ChaveExterna.
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        // 1. A linha de negócio.
        var departamento = leitura.Departamento;
        var linha = plano.LinhaId is { } linhaId
            ? await banco.LinhasDeNegocio.FirstAsync(l => l.Id == linhaId, ct)
            : null;
        if (linha is null)
        {
            linha = LinhaDeNegocio.Criar(CodigoDaLinha, Limitar(departamento.Descricao ?? departamento.Depto, 80));
            banco.LinhasDeNegocio.Add(linha);
        }

        if (plano.DeclararCadencia)
            linha.DeclararCadencia(departamento.CicloA, departamento.CicloB, departamento.CicloC, departamento.CicloD);

        await banco.SaveChangesAsync(ct);

        // 2. As contas dos donos que ainda não existem — aguardando liberação, sem perfil.
        foreach (var dono in plano.Donos.Values.Where(d => d.Motivo is null && d.UsuarioId is null))
        {
            dono.Criada = Usuario.CriarAguardandoLiberacao(
                CargaDeProcessoDoVortice.GuidEstavelDe($"VORTICE.USUARIO.{dono.Origem.Login.Trim().ToUpperInvariant()}"),
                dono.Origem.Login, dono.Origem.Nome ?? dono.Origem.Login, dono.Natureza, dono.FilialProvisoriaId, usuarioId, agora);
            banco.Usuarios.Add(dono.Criada);
        }

        await banco.SaveChangesAsync(ct);

        // O DE-PARA do dono e da carteira: criado quando falta, reapontado quando aponta para registro que não existe
        // mais, e carimbado com a conciliação desta rodada.
        var chaves = (await banco.ChavesExternas
                .Where(c => c.SistemaId == sistemaId && (c.Entidade == EntidadeCarteira || c.Entidade == EntidadeUsuario))
                .ToListAsync(ct))
            .ToDictionary(c => (c.Entidade, c.ChaveOrigem));

        void GarantirChave(string entidade, long chaveNaOrigem, long registroId)
        {
            var chave = chaveNaOrigem.ToString(CultureInfo.InvariantCulture);
            if (chaves.TryGetValue((entidade, chave), out var existente))
            {
                if (existente.RegistroId != registroId) existente.ReapontarPara(registroId, agora);
                else existente.MarcarSincronismo(agora);
                return;
            }

            var nova = ChaveExterna.Criar(sistemaId, entidade, registroId, chave);
            banco.ChavesExternas.Add(nova);
            chaves[(entidade, chave)] = nova;
        }

        foreach (var dono in plano.Donos.Values.Where(d => d.Motivo is null))
            GarantirChave(EntidadeUsuario, dono.Origem.SeqUsuario, dono.IdFinal);

        // 3. As carteiras.
        var paraAlterar = plano.Carteiras.Values.Where(c => c.Motivo is null && c.CarteiraId is not null && c.Alterar)
            .Select(c => c.CarteiraId!.Value).ToList();
        var rastreadas = await banco.Carteiras.Where(k => paraAlterar.Contains(k.Id)).ToDictionaryAsync(k => k.Id, ct);

        foreach (var carteira in plano.Carteiras.Values.Where(c => c.Motivo is null))
        {
            if (carteira.CarteiraId is { } existenteId)
            {
                if (carteira.Alterar)
                    rastreadas[existenteId].Alterar(carteira.Nome, carteira.Dono!.IdFinal, linha.Id, usuarioId);
                continue;
            }

            carteira.Criada = Carteira.Criar(
                carteira.EmpresaId, linha.Id, carteira.Codigo, Limitar(carteira.Nome, 120), carteira.Dono!.IdFinal, usuarioId,
                natureza: carteira.Natureza);
            banco.Carteiras.Add(carteira.Criada);
        }

        await banco.SaveChangesAsync(ct);

        foreach (var carteira in plano.Carteiras.Values.Where(c => c.IdFinal is not null))
            GarantirChave(EntidadeCarteira, carteira.Origem.SeqCarteira, carteira.IdFinal!.Value);

        // 4. A carteirização: encerra o que a origem deixou de declarar, inclui o que ela passou a declarar.
        var aEncerrar = plano.AEncerrar.ToHashSet();
        var gerida = plano.CarteirasGeridas.ToList();
        foreach (var vinculo in await banco.ClienteCarteiras
                     .Where(v => v.DesvinculadoEm == null && gerida.Contains(v.CarteiraId))
                     .ToListAsync(ct))
        {
            if (aEncerrar.Contains(vinculo.Id)) vinculo.Desvincular(agora);
        }

        foreach (var (clienteId, seqCarteira, desde) in plano.ACriar)
        {
            // A CLASSE NÃO É DECIDIDA AQUI (issue 53): ela é apurada da curva ABC do faturamento, no cliente, e a
            // coluna do vínculo sai no cenário A. O valor é o padrão do modelo, só porque a coluna é obrigatória;
            // o ciclo de contato vem da LINHA de negócio, e o do vínculo fica vazio.
            banco.ClienteCarteiras.Add(ClienteCarteira.Criar(
                clienteId, plano.Carteiras[seqCarteira].IdFinal!.Value, ClasseDeCliente.C, usuarioId, vinculadoEmUtc: desde ?? agora));
        }

        // 5. A trilha da origem: um registro por vínculo, com a decisão e os motivos.
        var registros = await banco.RegistrosDeOrigem.Where(r => r.SistemaId == sistemaId && r.Fluxo == Fluxo)
            .ToDictionaryAsync(r => r.ChaveOrigem, StringComparer.Ordinal, ct);

        foreach (var decisao in plano.Vinculos)
        {
            var retrato = Retrato(decisao, leitura.Departamento.Depto);
            if (!registros.TryGetValue(decisao.Origem.Chave, out var registro))
            {
                registro = RegistroDeOrigem.Registrar(sistemaId, Fluxo, decisao.Origem.Chave, retrato, agora);
                banco.RegistrosDeOrigem.Add(registro);
                registros[decisao.Origem.Chave] = registro;
            }
            else
            {
                registro.RegistrarLeitura(retrato, agora);
            }

            registro.Decidir(
                decisao.ClienteId is null ? DecisaoDaIntegracao.Pendente : DecisaoDaIntegracao.Importado,
                decisao.MotivosEmTexto, null);
        }

        var vistos = plano.Vinculos.Select(v => v.Origem.Chave).ToHashSet(StringComparer.Ordinal);
        foreach (var (chave, registro) in registros)
            if (!vistos.Contains(chave)) registro.MarcarAusente(agora);

        await banco.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        relatar("Gravado. A sincronia pode rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
    }

    // =============================================================================================
    // O relatório
    // =============================================================================================

    private void Relatar(LeituraDasCarteirasDoVortice leitura, Plano plano, bool comSa1)
    {
        const string etapaDaLeitura = "1. Leitura do Vórtice";
        Contar(etapaDaLeitura, "vínculos cliente × carteira lidos", leitura.Vinculos.Count);
        Contar(etapaDaLeitura, "carteiras do departamento", leitura.Carteiras.Count);
        Contar(etapaDaLeitura, "  com cliente", leitura.Vinculos.Select(v => v.SeqCarteira).Distinct().Count());
        Contar(etapaDaLeitura, "donos distintos (vendedor, senão responsável)",
            leitura.Carteiras.Where(c => c.SeqUsuarioDono is not null).Select(c => c.SeqUsuarioDono).Distinct().Count());
        Contar(etapaDaLeitura, "filiais distintas", leitura.Carteiras.Select(c => c.NroEmpresa).Distinct().Count());
        Contar(etapaDaLeitura, "carteiras com vendedor cujo usuário não é o responsável", leitura.Carteiras.Count(c => c.VendedorDiferenteDoResponsavel));
        Contar(etapaDaLeitura, "carteiras de vendedor DESLIGADO", leitura.Carteiras.Count(c => c.VendedorDesligado));
        foreach (var grupo in plano.Carteiras.Values.GroupBy(c => c.Prefixo).OrderBy(g => OrdemDoPrefixo(g.Key)))
        {
            var seqs = grupo.Select(c => c.Origem.SeqCarteira).ToHashSet();
            Contar(etapaDaLeitura, $"  {grupo.Key}: carteiras", grupo.Count());
            Contar(etapaDaLeitura, $"  {grupo.Key}: vínculos", leitura.Vinculos.Count(v => seqs.Contains(v.SeqCarteira)));
        }

        Contar(etapaDaLeitura, "documento válido (recomposto e conferido)", leitura.Vinculos.Count(v => v.Documento.Situacao == SituacaoDoDocumentoNoVortice.Valido));
        Contar(etapaDaLeitura, "  com zeros à esquerda devolvidos", leitura.Vinculos.Count(v => v.Documento.ZerosDevolvidos));
        Contar(etapaDaLeitura, "  FisicaJuridica discorda do documento (valeu o documento)", leitura.Vinculos.Count(v => v.Documento.TipoDeclaradoDiscorda));
        Contar(etapaDaLeitura, "sem documento", leitura.Vinculos.Count(v => v.Documento.Situacao == SituacaoDoDocumentoNoVortice.SemDocumento));
        Contar(etapaDaLeitura, "documento zerado", leitura.Vinculos.Count(v => v.Documento.Situacao == SituacaoDoDocumentoNoVortice.Zerado));
        Contar(etapaDaLeitura, "documento inválido", leitura.Vinculos.Count(v => v.Documento.Situacao == SituacaoDoDocumentoNoVortice.Invalido));

        const string etapaDaLinha = "2. Linha de negócio";
        Contar(etapaDaLinha, $"linha {CodigoDaLinha} a criar", plano.LinhaId is null ? 1 : 0);
        Contar(etapaDaLinha, $"cadência declarada (A/B/C/D = {leitura.Departamento.CicloA}/{leitura.Departamento.CicloB}/{leitura.Departamento.CicloC}/{leitura.Departamento.CicloD} dias)",
            plano.DeclararCadencia ? 1 : 0);

        const string etapaDosDonos = "3. Donos (seguranca.Usuario)";
        var donos = plano.Donos.Values.ToList();
        Contar(etapaDosDonos, "donos das carteiras que entram", donos.Count(d => d.Motivo is null));
        Contar(etapaDosDonos, "  já com conta no CRM, pelo de-para", donos.Count(d => d.Motivo is null && d.ComoAchou == "de-para"));
        Contar(etapaDosDonos, "  já com conta no CRM, pelo login", donos.Count(d => d.Motivo is null && d.ComoAchou == "login"));
        Contar(etapaDosDonos, RotuloDeDonosCriados, donos.Count(d => d.Motivo is null && d.UsuarioId is null));
        foreach (var natureza in donos.Where(d => d.Motivo is null && d.UsuarioId is null).GroupBy(d => d.Natureza).OrderBy(g => g.Key))
            Contar(etapaDosDonos, $"    natureza {natureza.Key}", natureza.Count());
        foreach (var motivo in donos.Where(d => d.Motivo is not null).GroupBy(d => d.Motivo!).OrderBy(g => g.Key))
            Contar(etapaDosDonos, $"  dono pendente: {motivo.Key}", motivo.Count());

        const string etapaDasCarteiras = "4. Carteiras (organizacao.Carteira)";
        var carteiras = plano.Carteiras.Values.ToList();
        Contar(etapaDasCarteiras, RotuloDeCarteirasCriadas, carteiras.Count(c => c.Motivo is null && c.CarteiraId is null));
        Contar(etapaDasCarteiras, RotuloDeCarteirasAlteradas, carteiras.Count(c => c.Motivo is null && c.CarteiraId is not null && c.Alterar));
        Contar(etapaDasCarteiras, "carteiras já existentes sem alteração", carteiras.Count(c => c.Motivo is null && c.CarteiraId is not null && !c.Alterar));
        foreach (var natureza in carteiras.Where(c => c.Motivo is null).GroupBy(c => (c.Natureza, c.Dono!.Natureza)).OrderBy(g => g.Key))
            Contar(etapaDasCarteiras, $"  natureza {natureza.Key.Item1}, dono {natureza.Key.Item2}", natureza.Count());
        foreach (var motivo in carteiras.Where(c => c.Motivo is not null).GroupBy(c => c.Motivo!).OrderBy(g => g.Key))
            Contar(etapaDasCarteiras, $"carteira pendente: {motivo.Key}", motivo.Count());
        Contar(etapaDasCarteiras, "carteiras que mudaram de filial na origem (a filial no CRM é mantida)", plano.CarteirasQueMudaramDeFilial);

        const string etapaDosVinculos = "5. Vínculos da origem";
        Contar(etapaDosVinculos, RotuloDeCasados, plano.Vinculos.Count(v => v.ClienteId is not null));
        foreach (var grupo in plano.Vinculos.Where(v => v.ClienteId is not null).GroupBy(v => v.Carteira!.Prefixo).OrderBy(g => OrdemDoPrefixo(g.Key)))
            Contar(etapaDosVinculos, $"  {grupo.Key}", grupo.Count());
        Contar(etapaDosVinculos, RotuloDePendentes, plano.Vinculos.Count(v => v.ClienteId is null));
        foreach (var motivo in plano.Vinculos.SelectMany(v => v.Motivos).GroupBy(m => m).OrderByDescending(g => g.Count()))
            Contar(etapaDosVinculos, $"  pendência por motivo: {motivo.Key}", motivo.Count());
        foreach (var grupo in plano.Vinculos.Where(v => v.ClienteId is null).GroupBy(v => v.Carteira?.Prefixo ?? "?").OrderBy(g => OrdemDoPrefixo(g.Key)))
        {
            Contar(etapaDosVinculos, $"  pendentes {grupo.Key}", grupo.Count());
            foreach (var motivo in grupo.SelectMany(v => v.Motivos).GroupBy(m => m).OrderByDescending(g => g.Count()))
                Contar(etapaDosVinculos, $"    {grupo.Key} por motivo: {motivo.Key}", motivo.Count());
        }

        if (comSa1)
        {
            // A PROJEÇÃO: o que casaria se a carga da SA1 já tivesse rodado no banco que se está lendo. Com o banco
            // de produção ainda sem clientes, é este o número que diz como a carteira vai ficar.
            Contar(etapaDosVinculos, "casariam depois da carga da SA1 (casados + CLIENTE_DA_SA1_AINDA_NAO_CARREGADO)",
                plano.Vinculos.Count(v => v.CasariaDepoisDaSa1));
            foreach (var grupo in plano.Vinculos.Where(v => v.CasariaDepoisDaSa1).GroupBy(v => v.Carteira!.Prefixo).OrderBy(g => OrdemDoPrefixo(g.Key)))
                Contar(etapaDosVinculos, $"  casariam {grupo.Key}", grupo.Count());

            // E SE A CARTEIRA NÃO FOSSE O PROBLEMA: quantos clientes casam em cada tipo de carteira, inclusive nas que
            // ficaram de fora pela filial. É o número que mede o custo de cada decisão sobre essas carteiras.
            Contar(etapaDosVinculos, "cliente casaria, em qualquer carteira (inclusive as pendentes)", plano.Vinculos.Count(v => v.ClienteCasaria));
            foreach (var grupo in plano.Vinculos.Where(v => v.ClienteCasaria).GroupBy(v => v.Carteira?.Prefixo ?? "?").OrderBy(g => OrdemDoPrefixo(g.Key)))
                Contar(etapaDosVinculos, $"  cliente casaria {grupo.Key}", grupo.Count());
        }

        const string etapaDaCarteirizacao = "6. Carteirização (comercial.ClienteCarteira)";
        Contar(etapaDaCarteirizacao, "clientes no CRM com documento", plano.ClientesNoCrm);
        Contar(etapaDaCarteirizacao, RotuloDeVinculosCriados, plano.ACriar.Count);
        Contar(etapaDaCarteirizacao, RotuloDeVinculosEncerrados, plano.AEncerrar.Count);
        Contar(etapaDaCarteirizacao, RotuloDeVinculosMantidos, plano.Mantidos);
        Contar(etapaDaCarteirizacao, "clientes que mudaram de carteira (encerrado numa, incluído noutra)", plano.ClientesQueMudaramDeCarteira);
        Contar(etapaDaCarteirizacao, "vínculos da origem que caem no mesmo par (cadastro repetido no Vórtice)", plano.ParesRepetidosNaOrigem);

        const string etapaDaTrilha = "7. Trilha (integracao.RegistroDeOrigem)";
        Contar(etapaDaTrilha, "registros lidos pela primeira vez", plano.RegistrosNovos);
        Contar(etapaDaTrilha, "registros já conhecidos com conteúdo alterado na origem", plano.RegistrosAlterados);
        Contar(etapaDaTrilha, "registros já conhecidos sem alteração", plano.RegistrosIguais);
        Contar(etapaDaTrilha, "registros já conhecidos com decisão ou motivo diferente", plano.RegistrosComDecisaoAlterada);
        Contar(etapaDaTrilha, "registros que sumiram da origem (marcados ausentes)", plano.RegistrosQueSumiram);
    }

    private void Contar(string etapa, string rotulo, int valor)
    {
        _contagens.Add((etapa, rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>
    /// O TIPO DE CARTEIRA NA ORIGEM, pelo começo do código: <c>MAQ</c> (vendedor de campo), <c>TBA</c> (pool da
    /// Inteligência de Mercado), <c>DGT</c> (digital); o resto (conta-chave, fora de atuação, teste) sai como
    /// "outras". Só para contar — a carga não decide nada por ele.
    /// </summary>
    /// <param name="codigo">O código da carteira no Vórtice.</param>
    internal static string Prefixo(string codigo)
    {
        var inicio = codigo.Split('_', 2)[0].Trim().ToUpperInvariant();
        return inicio is "MAQ" or "TBA" or "DGT" ? inicio + "_" : "outras";
    }

    private static int OrdemDoPrefixo(string prefixo) => prefixo switch { "MAQ_" => 0, "TBA_" => 1, "DGT_" => 2, _ => 3 };

    /// <summary>O login da conta: a parte antes do <c>@</c> do nome principal.</summary>
    private static string ParteLocal(string nomePrincipal)
    {
        var arroba = nomePrincipal.IndexOf('@', StringComparison.Ordinal);
        return (arroba < 0 ? nomePrincipal : nomePrincipal[..arroba]).Trim().ToLowerInvariant();
    }

    private static string Limitar(string texto, int tamanho) => texto.Length <= tamanho ? texto : texto[..tamanho];

    /// <summary>O resumo do conteúdo do vínculo — mudou o documento, a data ou a carteira, muda o resumo.</summary>
    private static string Resumo(VinculoNoVortice v) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|',
            v.SeqPessoa.ToString(CultureInfo.InvariantCulture), v.SeqCarteira.ToString(CultureInfo.InvariantCulture),
            v.Documento.Situacao, v.Documento.Numero, v.IncluidoEmUtc?.ToString("O", CultureInfo.InvariantCulture)))));

    /// <summary>O retrato do vínculo na trilha — sem nome nem documento: só o departamento, a carteira e o que mudou.</summary>
    private static RetratoDoRegistroDeOrigem Retrato(DecisaoDoVinculo decisao, string departamento)
    {
        var transformacoes = new List<string>(2);
        if (decisao.Origem.Documento.ZerosDevolvidos)
            transformacoes.Add("documento: zeros à esquerda devolvidos (a coluna numérica do Vórtice os perde)");
        if (decisao.Origem.Documento.TipoDeclaradoDiscorda)
            transformacoes.Add("tipo de pessoa: FisicaJuridica discorda do documento, e valeu o documento");

        return new RetratoDoRegistroDeOrigem(
            Resumo(decisao.Origem), null, Limitar(departamento, 60), null,
            decisao.Carteira is { } carteira ? Limitar(carteira.Origem.Codigo, 80) : null, null,
            transformacoes.Count == 0 ? null : string.Join("; ", transformacoes));
    }

    // =============================================================================================
    // As peças do plano
    // =============================================================================================

    private sealed record ContaNoCrm(long Id, string NomePrincipal, bool Excluida);

    private sealed record CarteiraNoCrm(long Id, string Codigo, string Nome, int EmpresaId, int LinhaDeNegocioId, long ResponsavelId, bool Excluida);

    private sealed class PlanoDeDono
    {
        public required DonoNoVortice Origem { get; init; }

        public NaturezaDoUsuario Natureza { get; init; }

        public long? UsuarioId { get; set; }

        public string? Motivo { get; set; }

        public string ComoAchou { get; set; } = "nova";

        public int FilialProvisoriaId { get; set; }

        public List<PlanoDeCarteira> Carteiras { get; } = [];

        public Usuario? Criada { get; set; }

        public long IdFinal => UsuarioId ?? Criada!.Id;
    }

    private sealed class PlanoDeCarteira
    {
        public required CarteiraNoVortice Origem { get; init; }

        public required string Codigo { get; init; }

        public required string Nome { get; init; }

        public required string Prefixo { get; init; }

        public string? Motivo { get; set; }

        public long? CarteiraId { get; set; }

        public int EmpresaId { get; set; }

        public NaturezaDaCarteira Natureza { get; set; }

        public PlanoDeDono? Dono { get; set; }

        public bool Alterar { get; set; }

        public Carteira? Criada { get; set; }

        public long? IdFinal => CarteiraId ?? Criada?.Id;
    }

    /// <summary>O que se decidiu sobre um vínculo da origem.</summary>
    /// <param name="Origem">O vínculo como veio.</param>
    /// <param name="Carteira">A carteira dele no plano; nula quando o Vórtice não a tem.</param>
    /// <param name="ClienteId">O cliente do CRM, quando o vínculo entra.</param>
    /// <param name="Motivos">Por que não entra, quando não entra.</param>
    /// <param name="ClienteCasaria">
    /// Se o CLIENTE casa — já existe no CRM, ou a carga da SA1 o criaria —, olhando só o cliente e não a carteira.
    /// </param>
    private sealed record DecisaoDoVinculo(
        VinculoNoVortice Origem, PlanoDeCarteira? Carteira, long? ClienteId, IReadOnlyList<string> Motivos, bool ClienteCasaria)
    {
        public string? MotivosEmTexto => Motivos.Count == 0 ? null : string.Join(",", Motivos.Distinct(StringComparer.Ordinal));

        /// <summary>Se o vínculo entraria com a carga da SA1 já aplicada: o cliente casaria e a carteira entra.</summary>
        public bool CasariaDepoisDaSa1 => ClienteCasaria && Carteira is { Motivo: null };
    }

    private sealed class Plano
    {
        public int? SistemaId { get; set; }

        public int? LinhaId { get; set; }

        public string NomeDaLinha { get; set; } = string.Empty;

        public bool DeclararCadencia { get; set; }

        public Dictionary<long, PlanoDeDono> Donos { get; } = [];

        public Dictionary<int, PlanoDeCarteira> Carteiras { get; } = [];

        public HashSet<long> CarteirasGeridas { get; } = [];

        public List<DecisaoDoVinculo> Vinculos { get; } = [];

        public Dictionary<(long ClienteId, int SeqCarteira), DateTime?> Desejados { get; } = [];

        public List<long> AEncerrar { get; } = [];

        public HashSet<long> ClientesQueSairam { get; } = [];

        public List<(long ClienteId, int SeqCarteira, DateTime? Desde)> ACriar { get; } = [];

        public int Mantidos { get; set; }

        public int ClientesQueMudaramDeCarteira { get; set; }

        public int ParesRepetidosNaOrigem { get; set; }

        public int CarteirasQueMudaramDeFilial { get; set; }

        public int ClientesNoCrm { get; set; }

        public int RegistrosNovos { get; set; }

        public int RegistrosAlterados { get; set; }

        public int RegistrosIguais { get; set; }

        public int RegistrosComDecisaoAlterada { get; set; }

        public int RegistrosQueSumiram { get; set; }
    }
}
