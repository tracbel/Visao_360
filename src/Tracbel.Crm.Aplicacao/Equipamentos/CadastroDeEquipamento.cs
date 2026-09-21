using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Equipamentos;

/// <summary>Os campos de equipamento já conferidos e resolvidos contra os catálogos.</summary>
internal sealed record DadosDeEquipamentoConferidos(
    ModeloParaSelecao? Modelo,
    long? ClienteId,
    Guid? ClienteChave,
    string? ClienteNome,
    int? ClienteEmpresaId,
    SituacaoDoEquipamento Situacao,
    OrigemDoEquipamento Origem,
    short? AnoFabricacao,
    short? AnoModelo,
    string? NumeroSerie,
    string? Placa,
    string? LocalizacaoDescrita,
    LinhaDeProdutoParaSelecao? LinhaDeProduto);

/// <summary>
/// A conferência de entrada de equipamento, escrita uma vez para a criação e a alteração.
///
/// O MODELO, A CLASSIFICAÇÃO E O DONO SÃO RESOLVIDOS AQUI, contra o catálogo e contra o cadastro
/// de clientes — não são aceitos como número vindo do JSON. É a regra 3.1 do documento 16 aplicada
/// aos três: modelo e classificação são catálogo, e cliente é registro cujo alcance depende de quem
/// está pedindo.
/// </summary>
internal static class ConferenciaDeEquipamento
{
    public static async Task<DadosDeEquipamentoConferidos?> ConferirAsync(
        ColetorDeErros erros,
        IRepositorioCatalogos catalogos,
        IRepositorioClientes clientes,
        string? modeloCodigo,
        string? clienteChave,
        string? situacao,
        string? origem,
        string? anoFabricacao,
        string? anoModelo,
        string? numeroSerie,
        string? placa,
        string? localizacaoDescrita,
        string? linhaDeProdutoCodigo,
        bool modeloObrigatorio,
        CancellationToken ct)
    {
        // O MODELO SÓ É OPCIONAL na máquina que chegou por venda do ART sem correspondência segura de
        // produto: é o que evita que alguém precise inventar um modelo para confirmar o dono.
        var codigo = modeloObrigatorio
            ? erros.Obrigatorio("modeloCodigo", modeloCodigo, "o modelo da máquina")
            : modeloCodigo?.Trim();

        ModeloParaSelecao? modelo = null;
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            modelo = await catalogos.ObterModeloAsync(codigo, ct);
            if (modelo is null)
                erros.Registrar(
                    "modeloCodigo",
                    "Modelo fora do catálogo de frota. " +
                    "Consulte /api/v1/catalogos/MODELO_EQUIPAMENTO para ver as opções.",
                    modeloCodigo);
        }

        LinhaDeProdutoParaSelecao? linhaDeProduto = null;
        if (!string.IsNullOrWhiteSpace(linhaDeProdutoCodigo))
        {
            linhaDeProduto = await catalogos.ObterLinhaDeProdutoAsync(linhaDeProdutoCodigo.Trim(), ct);
            if (linhaDeProduto is null)
                erros.Registrar(
                    "linhaDeProdutoCodigo",
                    "Classificação fora do catálogo. Consulte /api/v1/catalogos/LINHA_DE_PRODUTO para ver as opções.",
                    linhaDeProdutoCodigo);
        }

        var situacaoEscolhida = erros.ItemDeDominioOuPadrao(
            "situacao", situacao, SituacaoDoEquipamento.Ativo);
        var origemEscolhida = erros.ItemDeDominioOuPadrao("origem", origem, OrigemDoEquipamento.Crm);

        long? clienteId = null;
        Guid? chaveDoDono = null;
        string? nomeDoDono = null;
        int? empresaDoDono = null;

        if (!string.IsNullOrWhiteSpace(clienteChave))
        {
            if (!Guid.TryParse(clienteChave.Trim(), out var chave))
            {
                erros.Registrar("clienteChave", "A chave do cliente precisa ser um GUID.", clienteChave);
            }
            else
            {
                var dono = await clientes.ObterAsync(chave, incluirInativos: false, ct);
                if (dono is null)
                    erros.Registrar(
                        "clienteChave",
                        "Não há cliente ativo com esta chave ao seu alcance.",
                        clienteChave);
                else
                {
                    clienteId = dono.Cliente.Id;
                    chaveDoDono = dono.Cliente.ChavePublica;
                    nomeDoDono = dono.Cliente.NomeRazao;
                    empresaDoDono = dono.Cliente.EmpresaId;
                }
            }
        }

        var fabricacao = LerAno(erros, "anoFabricacao", anoFabricacao);
        var doModelo = LerAno(erros, "anoModelo", anoModelo);

        if (erros.TemErro) return null;

        return new DadosDeEquipamentoConferidos(
            modelo,
            clienteId,
            chaveDoDono,
            nomeDoDono,
            empresaDoDono,
            situacaoEscolhida,
            origemEscolhida,
            fabricacao,
            doModelo,
            numeroSerie,
            placa,
            localizacaoDescrita,
            linhaDeProduto);
    }

    /// <summary>A classificação como a leitura a devolve.</summary>
    public static ClassificacaoDaMaquina? Classificacao(LinhaDeProdutoParaSelecao? linha) =>
        linha is null ? null : new ClassificacaoDaMaquina(linha.Codigo, linha.Nome, linha.Porte);

    /// <summary>
    /// Lê um ano que chegou como texto.
    ///
    /// A FAIXA É CONFERIDA PELA ENTIDADE, não aqui: aqui só se decide se aquilo É um número. A
    /// divisão importa — "2O16" com a letra O é erro de digitação e merece uma frase sobre
    /// formato; 1850 é um ano possível de escrever e a recusa dele é regra de negócio.
    /// </summary>
    private static short? LerAno(ColetorDeErros erros, string campo, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;

        if (short.TryParse(valor.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var ano))
            return ano;

        erros.Registrar(campo, "O ano precisa ser um número de quatro dígitos.", valor);
        return null;
    }

    /// <summary>
    /// A recusa de chassi repetido, com a máquina que já o tem.
    ///
    /// [V] O cadastro vivo de equipamento do legado não valida formato de chassi: ele é texto
    /// livre lá, o que impede usar a coluna como chave natural para casar venda, garantia e
    /// ordem de serviço da mesma máquina. Aqui o chassi é tipo de valor E é único.
    /// </summary>
    public static async Task<ErroDeCampo?> ProcurarChassiRepetidoAsync(
        IRepositorioEquipamentos repositorio, Chassi chassi, Guid? exceto, CancellationToken ct)
    {
        var dona = await repositorio.ObterPorChassiAsync(chassi, exceto, ct);

        return dona is null
            ? null
            : new ErroDeCampo(
                "chassi",
                "Este chassi já está cadastrado nesta filial. " +
                "Se for a mesma máquina, edite o cadastro existente em vez de criar outro.",
                chassi.Numero);
    }
}

/// <summary>Cadastra uma máquina no nosso banco.</summary>
public sealed class CriarEquipamento(
    IRepositorioEquipamentos repositorio,
    IRepositorioClientes clientes,
    IRepositorioCatalogos catalogos,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso)
{
    /// <summary>Executa o cadastro.</summary>
    /// <param name="entrada">O que veio na requisição.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<EquipamentoDetalhe>> ExecutarAsync(
        NovoEquipamento entrada, CancellationToken ct)
    {
        // A PERMISSÃO É CONFERIDA TAMBÉM AQUI, e não só na rota (fase 3 do documento 41): o caso de uso
        // pode ser chamado por outro caminho um dia, e a regra tem de ir junto com ele.
        if (!acesso.Atual.Tem(Permissoes.EquipamentoCriar))
            return Resultado<EquipamentoDetalhe>.SemPermissao(
                $"Falta a permissão '{Permissoes.EquipamentoCriar}' ({Permissoes.Catalogo[Permissoes.EquipamentoCriar]}).");

        // EM "TODAS AS FILIAIS" A MÁQUINA NÃO TERIA FILIAL ESCOLHIDA — mesma regra do cadastro de cliente.
        if (acesso.Atual.VeTodasAsFiliais)
            return Resultado<EquipamentoDetalhe>.Conflito(ContextoAcesso.MensagemDeCadastroEmTodasAsFiliais);

        var erros = new ColetorDeErros();

        Chassi chassi = default;
        var chassiInformado = erros.Obrigatorio("chassi", entrada.Chassi, "o chassi da máquina");
        if (!string.IsNullOrWhiteSpace(chassiInformado) && !Chassi.TentarCriar(chassiInformado, out chassi))
            erros.Registrar(
                "chassi",
                "O chassi precisa ter 17 caracteres, só letras e números, sem I, O nem Q.",
                entrada.Chassi);

        var dados = await ConferenciaDeEquipamento.ConferirAsync(
            erros, catalogos, clientes,
            entrada.ModeloCodigo, entrada.ClienteChave, entrada.Situacao, entrada.Origem,
            entrada.AnoFabricacao, entrada.AnoModelo, entrada.NumeroSerie, entrada.Placa,
            entrada.LocalizacaoDescrita, entrada.LinhaDeProdutoCodigo, modeloObrigatorio: true, ct);

        if (dados is null || erros.TemErro)
            return erros.Recusar<EquipamentoDetalhe>("O cadastro do equipamento tem campos a corrigir.");

        var repetido = await ConferenciaDeEquipamento.ProcurarChassiRepetidoAsync(
            repositorio, chassi, exceto: null, ct);

        if (repetido is not null)
            return Resultado<EquipamentoDetalhe>.Conflito(
                "Já existe máquina com este chassi.", [repetido]);

        var contexto = acesso.Atual;

        Equipamento equipamento;
        try
        {
            equipamento = Equipamento.Criar(
                empresaId: contexto.EmpresaId,
                modeloId: dados.Modelo!.Id,
                chassi: chassi,
                origem: dados.Origem,
                criadoPorId: contexto.UsuarioId,
                clienteId: dados.ClienteId,
                situacao: dados.Situacao,
                anoFabricacao: dados.AnoFabricacao,
                anoModelo: dados.AnoModelo,
                numeroSerie: dados.NumeroSerie,
                placa: dados.Placa,
                localizacaoDescrita: dados.LocalizacaoDescrita);

            if (dados.LinhaDeProduto is { } classificacao)
                equipamento.ClassificarSeAusente(classificacao.Id, contexto.UsuarioId);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<EquipamentoDetalhe>.Conflito(erro.Message);
        }

        await repositorio.AdicionarAsync(equipamento, ct);

        var gravou = await unidade.SalvarAsync(ct);
        return gravou.EhSucesso
            ? Resultado<EquipamentoDetalhe>.Ok(EquipamentoDetalhe.De(
                new EquipamentoComContexto(
                    equipamento, dados.ClienteChave, dados.ClienteNome, dados.Modelo,
                    ConferenciaDeEquipamento.Classificacao(dados.LinhaDeProduto))))
            : Resultado<EquipamentoDetalhe>.Conflito(gravou.Erro!);
    }
}

/// <summary>Altera o cadastro de uma máquina.</summary>
public sealed class AlterarEquipamento(
    IRepositorioEquipamentos repositorio,
    IRepositorioClientes clientes,
    IRepositorioCatalogos catalogos,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso)
{
    /// <summary>Executa a alteração.</summary>
    /// <param name="chave">O GUID público da máquina.</param>
    /// <param name="entrada">O que veio na requisição.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<EquipamentoDetalhe>> ExecutarAsync(
        Guid chave, AlteracaoDeEquipamento entrada, CancellationToken ct)
    {
        // A PERMISSÃO É CONFERIDA TAMBÉM AQUI, e não só na rota (fase 3 do documento 41): o caso de uso
        // pode ser chamado por outro caminho um dia, e a regra tem de ir junto com ele.
        if (!acesso.Atual.Tem(Permissoes.EquipamentoEditar))
            return Resultado<EquipamentoDetalhe>.SemPermissao(
                $"Falta a permissão '{Permissoes.EquipamentoEditar}' ({Permissoes.Catalogo[Permissoes.EquipamentoEditar]}).");

        var leitura = await repositorio.ObterAsync(chave, incluirInativos: false, ct);

        if (leitura is null)
            return Resultado<EquipamentoDetalhe>.NaoEncontrado(
                $"Não há equipamento ativo {chave} ao seu alcance.");

        var equipamento = leitura.Equipamento;
        var erros = new ColetorDeErros();
        var versao = erros.CarimboDeVersao("versao", entrada.Versao);

        // A CLASSIFICAÇÃO AUSENTE NO CORPO MANTÉM A ATUAL; texto vazio retira. É a diferença entre
        // "não mexi neste campo" e "tirei a classificação", que o PUT precisa distinguir.
        var classificacaoPedida = entrada.LinhaDeProdutoCodigo is null
            ? leitura.Classificacao?.Codigo
            : entrada.LinhaDeProdutoCodigo;

        var modeloObrigatorio = !(equipamento.Origem == OrigemDoEquipamento.Art && equipamento.ModeloId is null);

        var dados = await ConferenciaDeEquipamento.ConferirAsync(
            erros, catalogos, clientes,
            entrada.ModeloCodigo ?? leitura.Modelo?.Codigo, entrada.ClienteChave,
            entrada.Situacao ?? equipamento.Situacao.ToString(), equipamento.Origem.ToString(),
            entrada.AnoFabricacao, entrada.AnoModelo, entrada.NumeroSerie, entrada.Placa,
            entrada.LocalizacaoDescrita, classificacaoPedida, modeloObrigatorio, ct);

        if (dados is null || erros.TemErro)
            return erros.Recusar<EquipamentoDetalhe>("A alteração do equipamento tem campos a corrigir.");

        // EM "TODAS AS FILIAIS" O CLIENTE DE OUTRA FILIAL TAMBÉM ESTÁ AO ALCANCE. Numa filial só, o filtro
        // global já impedia ligar a máquina a ele — as 16 filiais são raiz, então o que se enxerga é a
        // filial da máquina. A regra fica escrita para não mudar só porque o alcance cresceu.
        if (acesso.Atual.VeTodasAsFiliais && dados.ClienteEmpresaId is { } filialDoCliente && filialDoCliente != equipamento.EmpresaId)
        {
            erros.Registrar(
                "clienteChave",
                "Este cliente é de outra filial. Em \"Todas as filiais\", a máquina só pode ser ligada a um cliente da filial dela.",
                entrada.ClienteChave);
            return erros.Recusar<EquipamentoDetalhe>("A alteração do equipamento tem campos a corrigir.");
        }

        if (!equipamento.VersaoConfere(versao))
            return Resultado<EquipamentoDetalhe>.Concorrencia(
                "Esta máquina foi alterada por outra pessoa depois que você abriu a tela. " +
                "Recarregue o registro e refaça a alteração.");

        try
        {
            equipamento.Alterar(
                modeloId: dados.Modelo?.Id,
                usuarioId: acesso.Atual.UsuarioId,
                clienteId: dados.ClienteId,
                situacao: dados.Situacao,
                anoFabricacao: dados.AnoFabricacao,
                anoModelo: dados.AnoModelo,
                numeroSerie: dados.NumeroSerie,
                placa: dados.Placa,
                localizacaoDescrita: dados.LocalizacaoDescrita);

            equipamento.Classificar(dados.LinhaDeProduto?.Id, acesso.Atual.UsuarioId);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<EquipamentoDetalhe>.Conflito(erro.Message);
        }

        var gravou = await unidade.SalvarAsync(ct);
        return gravou.EhSucesso
            ? Resultado<EquipamentoDetalhe>.Ok(EquipamentoDetalhe.De(
                new EquipamentoComContexto(
                    equipamento, dados.ClienteChave, dados.ClienteNome, dados.Modelo,
                    ConferenciaDeEquipamento.Classificacao(dados.LinhaDeProduto), leitura.UltimaVenda)))
            : Resultado<EquipamentoDetalhe>.Concorrencia(gravou.Erro!);
    }
}

/// <summary>Baixa uma máquina — exclusão lógica. A linha e o histórico de horímetro ficam.</summary>
public sealed class InativarEquipamento(
    IRepositorioEquipamentos repositorio,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso)
{
    /// <summary>Executa a baixa.</summary>
    /// <param name="chave">O GUID público da máquina.</param>
    /// <param name="entrada">A versão conhecida.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<EquipamentoDetalhe>> ExecutarAsync(
        Guid chave, BaixaDeEquipamento entrada, CancellationToken ct)
    {
        // A PERMISSÃO É CONFERIDA TAMBÉM AQUI, e não só na rota (fase 3 do documento 41): o caso de uso
        // pode ser chamado por outro caminho um dia, e a regra tem de ir junto com ele.
        if (!acesso.Atual.Tem(Permissoes.EquipamentoExcluir))
            return Resultado<EquipamentoDetalhe>.SemPermissao(
                $"Falta a permissão '{Permissoes.EquipamentoExcluir}' ({Permissoes.Catalogo[Permissoes.EquipamentoExcluir]}).");

        var leitura = await repositorio.ObterAsync(chave, incluirInativos: true, ct);

        if (leitura is null)
            return Resultado<EquipamentoDetalhe>.NaoEncontrado($"Não há equipamento {chave} ao seu alcance.");

        var equipamento = leitura.Equipamento;
        var erros = new ColetorDeErros();
        var versao = erros.CarimboDeVersao("versao", entrada.Versao);

        if (erros.TemErro)
            return erros.Recusar<EquipamentoDetalhe>("A baixa do equipamento tem campos a corrigir.");

        if (!equipamento.VersaoConfere(versao))
            return Resultado<EquipamentoDetalhe>.Concorrencia(
                "Esta máquina foi alterada por outra pessoa depois que você abriu a tela. " +
                "Recarregue o registro antes de baixar.");

        try
        {
            equipamento.Inativar(acesso.Atual.UsuarioId);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<EquipamentoDetalhe>.Conflito(erro.Message);
        }

        var gravou = await unidade.SalvarAsync(ct);
        return gravou.EhSucesso
            ? Resultado<EquipamentoDetalhe>.Ok(EquipamentoDetalhe.De(leitura))
            : Resultado<EquipamentoDetalhe>.Concorrencia(gravou.Erro!);
    }
}
