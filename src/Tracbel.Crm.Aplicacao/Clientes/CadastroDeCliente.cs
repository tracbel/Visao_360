using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Clientes;

/// <summary>
/// Os campos de cliente já saneados e conferidos — o que sobra depois que a entrada crua passou
/// pelos tipos de valor do domínio e pelos catálogos.
/// </summary>
internal sealed record DadosDeClienteConferidos(
    string NomeRazao,
    string? NomeFantasia,
    TipoDePessoa TipoDePessoa,
    CpfCnpj? Documento,
    string? InscricaoEstadual,
    string? AtividadeEconomica,
    SituacaoDoCliente Situacao,
    int? OrigemId,
    string? OrigemCodigo);

/// <summary>
/// A conferência de entrada de cliente, escrita UMA vez e usada pela criação e pela alteração.
///
/// Ela não valida CPF, telefone nem nada disso: ela CHAMA os tipos de valor do domínio, que já
/// sabem validar, e traduz a recusa deles em erro de campo. É a regra do documento 16, princípio
/// 1.2 — um só ponto de normalização por tipo — vista do lado de quem consome.
/// </summary>
internal static class ConferenciaDeCliente
{
    public static async Task<DadosDeClienteConferidos?> ConferirAsync(
        ColetorDeErros erros,
        IRepositorioCatalogos catalogos,
        string? nomeRazao,
        string? nomeFantasia,
        string? tipoDePessoa,
        string? documento,
        string? inscricaoEstadual,
        string? atividadeEconomica,
        string? situacao,
        string? origemCodigo,
        CancellationToken ct)
    {
        var nome = erros.Obrigatorio("nomeRazao", nomeRazao, "a razão social ou o nome do cliente");
        var tipo = erros.ItemDeDominio<TipoDePessoa>("tipoDePessoa", tipoDePessoa);
        var situacaoEscolhida = erros.ItemDeDominioOuPadrao("situacao", situacao, SituacaoDoCliente.Prospect);

        CpfCnpj? doc = null;
        if (!string.IsNullOrWhiteSpace(documento))
        {
            if (CpfCnpj.TentarCriar(documento, out var lido))
            {
                doc = lido;

                // A coerência entre documento e tipo de pessoa também é conferida pela entidade.
                // Aqui ela é conferida de novo só para que a recusa saia COM O NOME DO CAMPO —
                // a entidade recusa com uma frase, esta camada recusa apontando onde corrigir.
                if (tipo is { } t && lido.EhPessoaFisica != (t == TipoDePessoa.Fisica))
                    erros.Registrar(
                        "documento",
                        lido.EhPessoaFisica
                            ? "Um CPF pertence a pessoa física. Troque o tipo de pessoa ou informe um CNPJ."
                            : "Um CNPJ pertence a pessoa jurídica. Troque o tipo de pessoa ou informe um CPF.",
                        documento);
            }
            else
            {
                erros.Registrar(
                    "documento",
                    "CPF ou CNPJ inválido: confira os dígitos. Aceita com ou sem máscara.",
                    documento);
            }
        }

        int? origemId = null;
        if (!string.IsNullOrWhiteSpace(origemCodigo))
        {
            origemId = await catalogos.ResolverItemAsync(
                CatalogosDeSistema.OrigemDeLead, origemCodigo.Trim(), ct);

            // Campo com catálogo NÃO aceita texto livre — documento 16, seção 3.1, ponto 2:
            // a API recusa mesmo que a tela tenha sido contornada.
            if (origemId is null)
                erros.Registrar(
                    "origemCodigo",
                    "Origem fora do catálogo ORIGEM_LEAD. Consulte /api/v1/catalogos/ORIGEM_LEAD para ver as opções.",
                    origemCodigo);
        }

        if (erros.TemErro) return null;

        return new DadosDeClienteConferidos(
            nome,
            nomeFantasia,
            tipo!.Value,
            doc,
            inscricaoEstadual,
            atividadeEconomica,
            situacaoEscolhida,
            origemId,
            string.IsNullOrWhiteSpace(origemCodigo) ? null : origemCodigo.Trim());
    }

    /// <summary>
    /// A recusa de documento repetido, com o nome de quem já o tem.
    ///
    /// [V] O legado mede 116 CPFs repetidos entre clientes distintos e uma fila de deduplicação
    /// com 124 mil pares abandonados desde 2018. O índice único do banco impede a repetição; esta
    /// consulta existe para que a recusa diga QUEM já usa o documento, em vez de devolver uma
    /// violação de índice que ninguém consegue interpretar.
    /// </summary>
    public static async Task<ErroDeCampo?> ProcurarDocumentoRepetidoAsync(
        IRepositorioClientes repositorio, CpfCnpj? documento, int empresaId, Guid? exceto, CancellationToken ct)
    {
        if (documento is not { } doc) return null;

        var dono = await repositorio.ObterPorDocumentoAsync(doc, empresaId, exceto, ct);

        return dono is null
            ? null
            : new ErroDeCampo(
                "documento",
                $"Este documento já está no cliente \"{dono.NomeRazao}\" desta filial. " +
                "Se for o mesmo cliente, edite o cadastro existente em vez de criar outro.",
                doc.Formatado());
    }
}

/// <summary>Cria um cliente no nosso banco.</summary>
public sealed class CriarCliente(
    IRepositorioClientes repositorio,
    IRepositorioCatalogos catalogos,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso)
{
    /// <summary>Executa a criação.</summary>
    /// <param name="entrada">O que veio na requisição.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ClienteDetalhe>> ExecutarAsync(NovoCliente entrada, CancellationToken ct)
    {
        // A PERMISSÃO É CONFERIDA TAMBÉM AQUI, e não só na rota (fase 3 do documento 41): o caso de uso
        // pode ser chamado por outro caminho um dia, e a regra tem de ir junto com ele.
        if (!acesso.Atual.Tem(Permissoes.ClienteCriar))
            return Resultado<ClienteDetalhe>.SemPermissao(
                $"Falta a permissão '{Permissoes.ClienteCriar}' ({Permissoes.Catalogo[Permissoes.ClienteCriar]}).");

        // EM "TODAS AS FILIAIS" O CLIENTE NÃO TERIA FILIAL ESCOLHIDA: a do contexto é a de casa só porque
        // ele precisa de uma. Recusar é melhor do que cadastrar numa filial que ninguém escolheu.
        if (acesso.Atual.VeTodasAsFiliais)
            return Resultado<ClienteDetalhe>.Conflito(ContextoAcesso.MensagemDeCadastroEmTodasAsFiliais);

        var erros = new ColetorDeErros();

        var dados = await ConferenciaDeCliente.ConferirAsync(
            erros, catalogos,
            entrada.NomeRazao, entrada.NomeFantasia, entrada.TipoDePessoa, entrada.Documento,
            entrada.InscricaoEstadual, entrada.AtividadeEconomica, entrada.Situacao,
            entrada.OrigemCodigo, ct);

        if (dados is null)
            return erros.Recusar<ClienteDetalhe>("O cadastro do cliente tem campos a corrigir.");

        var repetido = await ConferenciaDeCliente.ProcurarDocumentoRepetidoAsync(
            repositorio, dados.Documento, acesso.Atual.EmpresaId, exceto: null, ct);

        if (repetido is not null)
            return Resultado<ClienteDetalhe>.Conflito(
                "Já existe cliente com este documento nesta filial.", [repetido]);

        var contexto = acesso.Atual;

        // A FILIAL E O DONO VÊM DO CONTEXTO DE ACESSO, nunca do corpo da requisição. Aceitar
        // EmpresaId no JSON seria abrir um caminho para gravar na filial de outro — a fronteira
        // do documento 05 valeria para a leitura e não para a escrita.
        Cliente cliente;
        try
        {
            cliente = Cliente.Criar(
                empresaId: contexto.EmpresaId,
                nomeRazao: dados.NomeRazao,
                tipoDePessoa: dados.TipoDePessoa,
                proprietarioId: contexto.UsuarioId,
                criadoPorId: contexto.UsuarioId,
                documento: dados.Documento,
                nomeFantasia: dados.NomeFantasia,
                situacao: dados.Situacao,
                inscricaoEstadual: dados.InscricaoEstadual,
                atividadeEconomica: dados.AtividadeEconomica,
                origemId: dados.OrigemId);
        }
        catch (RegraDeNegocioViolada erro)
        {
            // A entidade é a última palavra. Se ela recusou, a recusa vira resultado — não 500.
            return Resultado<ClienteDetalhe>.Conflito(erro.Message);
        }

        await repositorio.AdicionarAsync(cliente, ct);

        var gravou = await unidade.SalvarAsync(ct);
        return gravou.EhSucesso
            ? Resultado<ClienteDetalhe>.Ok(
                ClienteDetalhe.De(new ClienteComContexto(cliente, dados.OrigemCodigo, null)))
            : Resultado<ClienteDetalhe>.Conflito(gravou.Erro!);
    }
}

/// <summary>Altera um cliente já cadastrado.</summary>
public sealed class AlterarCliente(
    IRepositorioClientes repositorio,
    IRepositorioCatalogos catalogos,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso)
{
    /// <summary>Executa a alteração.</summary>
    /// <param name="chave">O GUID público do cliente.</param>
    /// <param name="entrada">O que veio na requisição.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ClienteDetalhe>> ExecutarAsync(
        Guid chave, AlteracaoDeCliente entrada, CancellationToken ct)
    {
        // A PERMISSÃO É CONFERIDA TAMBÉM AQUI, e não só na rota (fase 3 do documento 41): o caso de uso
        // pode ser chamado por outro caminho um dia, e a regra tem de ir junto com ele.
        if (!acesso.Atual.Tem(Permissoes.ClienteEditar))
            return Resultado<ClienteDetalhe>.SemPermissao(
                $"Falta a permissão '{Permissoes.ClienteEditar}' ({Permissoes.Catalogo[Permissoes.ClienteEditar]}).");

        var leitura = await repositorio.ObterAsync(chave, incluirInativos: false, ct);

        if (leitura is null)
            return Resultado<ClienteDetalhe>.NaoEncontrado(
                $"Não há cliente ativo {chave} ao seu alcance.");

        var cliente = leitura.Cliente;
        var erros = new ColetorDeErros();
        var versao = erros.CarimboDeVersao("versao", entrada.Versao);

        var dados = await ConferenciaDeCliente.ConferirAsync(
            erros, catalogos,
            entrada.NomeRazao, entrada.NomeFantasia, entrada.TipoDePessoa, entrada.Documento,
            entrada.InscricaoEstadual, entrada.AtividadeEconomica,
            entrada.Situacao ?? cliente.Situacao.ToString(), entrada.OrigemCodigo, ct);

        if (dados is null)
            return erros.Recusar<ClienteDetalhe>("A alteração do cliente tem campos a corrigir.");

        if (!cliente.VersaoConfere(versao))
            return Resultado<ClienteDetalhe>.Concorrencia(
                "Este cliente foi alterado por outra pessoa depois que você abriu a tela. " +
                "Recarregue o registro e refaça a alteração — assim nada do trabalho dela é perdido.");

        // NA FILIAL DO CLIENTE, e não na do contexto: em "Todas as filiais" o contexto alcança todas, e o
        // documento só é único dentro da filial (UX_Cliente_Empresa_Documento).
        var repetido = await ConferenciaDeCliente.ProcurarDocumentoRepetidoAsync(
            repositorio, dados.Documento, cliente.EmpresaId, exceto: chave, ct);

        if (repetido is not null)
            return Resultado<ClienteDetalhe>.Conflito(
                "Já existe outro cliente com este documento nesta filial.", [repetido]);

        try
        {
            cliente.Alterar(
                nomeRazao: dados.NomeRazao,
                tipoDePessoa: dados.TipoDePessoa,
                proprietarioId: cliente.ProprietarioId,
                usuarioId: acesso.Atual.UsuarioId,
                documento: dados.Documento,
                nomeFantasia: dados.NomeFantasia,
                inscricaoEstadual: dados.InscricaoEstadual,
                atividadeEconomica: dados.AtividadeEconomica,
                origemId: dados.OrigemId);

            cliente.MudarSituacao(dados.Situacao, acesso.Atual.UsuarioId);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<ClienteDetalhe>.Conflito(erro.Message);
        }

        var gravou = await unidade.SalvarAsync(ct);
        return gravou.EhSucesso
            ? Resultado<ClienteDetalhe>.Ok(
                ClienteDetalhe.De(leitura with { Cliente = cliente, OrigemCodigo = dados.OrigemCodigo }))
            : Resultado<ClienteDetalhe>.Concorrencia(gravou.Erro!);
    }
}

/// <summary>
/// Inativa um cliente — exclusão LÓGICA, com motivo de catálogo obrigatório.
///
/// Nada é apagado. [V] no legado, "Atividade Cancelada" cancelava o processo inteiro sem
/// registrar motivo e sem desfazer; e a fila de deduplicação abandonada mostra o que acontece
/// quando o cadastro só sabe crescer.
/// </summary>
public sealed class InativarCliente(
    IRepositorioClientes repositorio,
    IRepositorioCatalogos catalogos,
    IUnidadeDeTrabalho unidade,
    IProvedorContextoAcesso acesso)
{
    /// <summary>Executa a inativação.</summary>
    /// <param name="chave">O GUID público do cliente.</param>
    /// <param name="entrada">O motivo e a versão conhecida.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ClienteDetalhe>> ExecutarAsync(
        Guid chave, InativacaoDeCliente entrada, CancellationToken ct)
    {
        // A PERMISSÃO É CONFERIDA TAMBÉM AQUI, e não só na rota (fase 3 do documento 41): o caso de uso
        // pode ser chamado por outro caminho um dia, e a regra tem de ir junto com ele.
        if (!acesso.Atual.Tem(Permissoes.ClienteExcluir))
            return Resultado<ClienteDetalhe>.SemPermissao(
                $"Falta a permissão '{Permissoes.ClienteExcluir}' ({Permissoes.Catalogo[Permissoes.ClienteExcluir]}).");

        var leitura = await repositorio.ObterAsync(chave, incluirInativos: true, ct);

        if (leitura is null)
            return Resultado<ClienteDetalhe>.NaoEncontrado($"Não há cliente {chave} ao seu alcance.");

        var cliente = leitura.Cliente;
        var erros = new ColetorDeErros();
        var versao = erros.CarimboDeVersao("versao", entrada.Versao);
        var codigo = erros.Obrigatorio("motivoCodigo", entrada.MotivoCodigo, "o motivo da inativação");

        int? motivoId = null;
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            motivoId = await catalogos.ResolverItemAsync(CatalogosDeSistema.MotivoDeInativacao, codigo, ct);
            if (motivoId is null)
                erros.Registrar(
                    "motivoCodigo",
                    "Motivo fora do catálogo MOTIVO_INATIVACAO. " +
                    "Consulte /api/v1/catalogos/MOTIVO_INATIVACAO para ver as opções.",
                    codigo);
        }

        if (erros.TemErro)
            return erros.Recusar<ClienteDetalhe>("A inativação do cliente tem campos a corrigir.");

        if (!cliente.VersaoConfere(versao))
            return Resultado<ClienteDetalhe>.Concorrencia(
                "Este cliente foi alterado por outra pessoa depois que você abriu a tela. " +
                "Recarregue o registro antes de inativar.");

        try
        {
            cliente.Inativar(motivoId!.Value, acesso.Atual.UsuarioId);
        }
        catch (RegraDeNegocioViolada erro)
        {
            return Resultado<ClienteDetalhe>.Conflito(erro.Message);
        }

        var gravou = await unidade.SalvarAsync(ct);
        return gravou.EhSucesso
            ? Resultado<ClienteDetalhe>.Ok(
                ClienteDetalhe.De(leitura with { Cliente = cliente, MotivoInativacaoCodigo = codigo }))
            : Resultado<ClienteDetalhe>.Concorrencia(gravou.Erro!);
    }
}
