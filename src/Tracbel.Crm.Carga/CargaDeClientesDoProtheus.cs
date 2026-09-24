using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga de clientes fez, em números.</summary>
/// <param name="LojasLidas">Linhas da SA1 com documento válido.</param>
/// <param name="DocumentosDistintos">Clientes candidatos, depois de juntar as lojas.</param>
/// <param name="ClientesIncluidos">Clientes criados.</param>
/// <param name="ClientesAtualizados">Clientes que já existiam e mudaram.</param>
/// <param name="ClientesIguais">Clientes que já existiam e não mudaram.</param>
/// <param name="EnderecosIncluidos">Endereços principais criados.</param>
/// <param name="Pendentes">Documentos que não viraram cliente, com o motivo.</param>
/// <param name="PendentesPorMotivo">Quantos por motivo.</param>
/// <param name="Contagens">A medição, etapa a etapa.</param>
internal sealed record RelatorioDaCargaDeClientes(
    int LojasLidas,
    int DocumentosDistintos,
    int ClientesIncluidos,
    int ClientesAtualizados,
    int ClientesIguais,
    int EnderecosIncluidos,
    int Pendentes,
    IReadOnlyDictionary<string, int> PendentesPorMotivo,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens);

/// <summary>
/// A CARGA DO CADASTRO DE CLIENTES, da <c>SA1010</c> do Protheus para <c>comercial.Cliente</c>.
///
/// <para><b>Por que ela existe</b> (24/09/2026). O banco de produção estava com <b>zero</b> clientes
/// — efeito da sanitização de 15/09 sem recarga —, e é isso que fazia toda a Visão Diretoria sair
/// vazia: sem cliente não há endereço, sem endereço não há município, e as 4.183 vendas que o ART lê
/// ficavam pendentes com <c>COMPRADOR_AUSENTE_NO_CRM</c> em 100% delas. A carga de cadastro que
/// existia lê o Vórtice e foi aposentada; a SA1 é a fonte decidida.</para>
///
/// <para><b>UMA LOJA NÃO É UM CLIENTE.</b> A SA1 tem uma linha por código + loja, e o mesmo CNPJ
/// aparece em várias — 30.923 lojas são 28.116 clientes na área de atuação. O CRM identifica cliente
/// pelo DOCUMENTO, que é a chave que casa com o ART, com a nota fiscal e com o Vórtice. As lojas são
/// juntadas aqui, e a escolhida para o endereço é a mais completa.</para>
///
/// <para><b>A FILIAL DONA VEM DA ÁREA DE ATUAÇÃO, e não da origem.</b> Medido: <c>A1_FILIAL</c> vem
/// vazio nas 38.752 linhas, porque no Protheus o cadastro de cliente é compartilhado entre filiais.
/// Quem responde por um cliente é a filial responsável pelo MUNICÍPIO dele — e os 203 municípios da
/// ADR têm todos uma. Era isto que a fila de compradores do ART chamava de "definir filial
/// responsável": não era decisão pendente, era um cruzamento que ninguém tinha feito.</para>
///
/// <para><b>O ESCOPO SE DEFINE SOZINHO.</b> Só há filial responsável dentro da área de atuação;
/// cliente de município de fora não tem dono e fica pendente, com o motivo escrito. Isso não é uma
/// exclusão arbitrária — é a fronteira de acesso do CRM dizendo de quem o cadastro é.</para>
///
/// <para><b>Repetível.</b> O cliente é reencontrado pelo documento e o endereço pelo cliente.
/// Conteúdo igual não regrava. O que uma pessoa mudou no CRM não é desfeito: a carga só preenche o
/// que está ausente e corrige o que veio da origem.</para>
///
/// <para><b>Simulação:</b> a carga inteira roda numa transação, desfeita no fim quando simulada — os
/// números da simulação são, por construção, os da carga real.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="protheus">A leitura da SA1.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDeClientesDoProtheus(
    Func<CrmDbContext> abrirContexto,
    LeitorDeClientesDoProtheus protheus,
    long usuarioId,
    Action<string> relatar)
{
    /// <summary>Motivo: o município da SA1 não está no catálogo do IBGE do CRM.</summary>
    internal const string MunicipioNaoReconhecido = "MUNICIPIO_NAO_RECONHECIDO";

    /// <summary>Motivo: o município existe, mas está fora da área de atuação — ninguém responde por ele.</summary>
    internal const string ForaDaAreaDeAtuacao = "FORA_DA_AREA_DE_ATUACAO";

    /// <summary>Motivo: a SA1 não diz a UF, e sem ela o código do município não se monta.</summary>
    internal const string SemUf = "SEM_UF_NA_ORIGEM";

    /// <summary>
    /// Motivo: o documento tem 11 ou 14 dígitos mas não passa no dígito verificador.
    ///
    /// <para>Medido na primeira simulação contra a SA1 real: existe <c>55555555555</c> no cadastro.
    /// Um documento assim <b>não é erro da carga</b> — é dado que a origem aceitou e o CRM não
    /// aceita, e o lugar dele é a lista de pendências, não uma exceção que derruba a carga inteira
    /// e impede os outros 33 mil clientes de entrarem.</para>
    /// </summary>
    internal const string DocumentoInvalido = "DOCUMENTO_INVALIDO";

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];

    /// <summary>Executa a carga.</summary>
    /// <param name="simular">Desfaz tudo ao final.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDaCargaDeClientes>> ExecutarAsync(bool simular, CancellationToken ct)
    {
        relatar("Lendo o cadastro de clientes do Protheus (SA1010, sessão somente leitura)…");
        var lido = await protheus.LerAsync(ct);
        if (!lido.EhSucesso) return Resultado<RelatorioDaCargaDeClientes>.Indisponivel(lido.Erro!);

        var lojas = lido.Valor;
        const string etapaDaLeitura = "1. Leitura da SA1";
        Contar(etapaDaLeitura, "lojas com documento válido", lojas.Count);

        // UMA LOJA NÃO É UM CLIENTE: o agrupamento é por documento, e a loja escolhida para o
        // endereço é a mais completa — desempate por menor código, para que duas cargas seguidas
        // escolham a mesma e o endereço não fique oscilando entre lojas equivalentes.
        var porDocumento = lojas
            .GroupBy(l => l.Documento, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, EscolherAPrincipal, StringComparer.Ordinal);

        Contar(etapaDaLeitura, "documentos distintos (clientes candidatos)", porDocumento.Count);
        Contar(etapaDaLeitura, "  com mais de uma loja na SA1", lojas.Count - porDocumento.Count);

        await using var contexto = abrirContexto();
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);

        var territorio = await TerritorioDaCargaDeClientes.LerAsync(contexto, ct);

        const string etapaDoDePara = "2. De-para de município e filial";
        Contar(etapaDoDePara, "municípios no catálogo do CRM", territorio.MunicipioPorIbge.Count);
        Contar(etapaDoDePara, "municípios com filial responsável", territorio.EmpresaPorMunicipio.Count);

        var clientesPorDocumento = (await contexto.Clientes.Where(c => c.Documento != null).ToListAsync(ct))
            .ToDictionary(c => c.Documento!.Value.Numero, StringComparer.Ordinal);

        var enderecoPorCliente = await contexto.Enderecos
            .Where(e => e.EhPrincipal)
            .ToDictionaryAsync(e => e.ClienteId, ct);

        int incluidos = 0, atualizados = 0, iguais = 0, enderecos = 0, tipoDivergente = 0;
        var pendentesPorMotivo = new Dictionary<string, int>(StringComparer.Ordinal);
        var novos = new List<(Cliente Cliente, LojaDeClienteNoProtheus Loja, int MunicipioId)>();

        foreach (var (documento, loja) in porDocumento)
        {
            // A DECISÃO DE CADA DOCUMENTO mora em TerritorioDaCargaDeClientes.Situar, que a sincronia das
            // carteiras do Vórtice também usa para dizer por que um cliente de carteira não está no CRM.
            var (motivo, cpfCnpj, municipioId, empresaId) = territorio.Situar(documento, loja);
            if (motivo is not null)
            {
                Pendente(motivo);
                continue;
            }

            // O TIPO DE PESSOA SAI DO DOCUMENTO, e não de `A1_PESSOA`.
            //
            // Onze dígitos é CPF e catorze é CNPJ — isso é fato do documento, conferido no dígito
            // verificador. `A1_PESSOA` é um campo que alguém preencheu, e na SA1 real ele
            // discorda: a primeira simulação parou justamente aqui. Derivar do documento elimina a
            // classe inteira de erro em vez de tratá-la caso a caso; a discordância vira número,
            // para que ela seja vista e corrigida na origem.
            var tipo = cpfCnpj.EhPessoaFisica ? TipoDePessoa.Fisica : TipoDePessoa.Juridica;
            if (cpfCnpj.EhPessoaFisica != loja.PessoaFisica) tipoDivergente++;

            if (clientesPorDocumento.TryGetValue(documento, out var existente))
            {
                // SÓ CHAMA `Alterar` SE ALGO MUDOU DE FATO. `Alterar` marca a trilha de auditoria;
                // chamá-lo a cada carga encheria `auditoria.AlteracaoDeCampo` de linhas dizendo que
                // nada mudou, e a trilha deixaria de servir para achar o que mudou.
                var igual =
                    string.Equals(existente.NomeRazao, loja.NomeRazao.Trim(), StringComparison.Ordinal)
                    && string.Equals(existente.NomeFantasia, loja.NomeFantasia?.Trim(), StringComparison.Ordinal)
                    && existente.TipoDePessoa == tipo
                    && string.Equals(existente.InscricaoEstadual, loja.InscricaoEstadual?.Trim(), StringComparison.Ordinal);

                if (igual)
                {
                    iguais++;
                }
                else
                {
                    existente.Alterar(
                        loja.NomeRazao,
                        tipo,
                        existente.ProprietarioId,
                        usuarioId,
                        documento: existente.Documento,
                        nomeFantasia: loja.NomeFantasia,
                        inscricaoEstadual: loja.InscricaoEstadual,
                        atividadeEconomica: existente.AtividadeEconomica,
                        origemId: existente.OrigemId);
                    atualizados++;
                }

                continue;
            }

            // A SITUAÇÃO NÃO SAI DO CADASTRO. A SA1 diz que o cliente EXISTE, não que ele comprou —
            // e `Prospect` afirmaria "nunca comprou", `Cliente` afirmaria "comprou e está ativo".
            // Quem sabe disso é o movimento: o faturamento do Protheus e as vendas do ART. Entra
            // como `Suspect`, que é literalmente "ainda não se sabe", e o movimento promove.
            var cliente = Cliente.Criar(
                empresaId,
                loja.NomeRazao,
                tipo,
                proprietarioId: usuarioId,
                criadoPorId: usuarioId,
                documento: cpfCnpj,
                nomeFantasia: loja.NomeFantasia,
                situacao: SituacaoDoCliente.Suspect,
                inscricaoEstadual: loja.InscricaoEstadual);

            contexto.Clientes.Add(cliente);
            novos.Add((cliente, loja, municipioId));
            incluidos++;
        }

        // OS ENDEREÇOS SÓ DEPOIS DE O CLIENTE TER IDENTIDADE: o endereço é filho e precisa do Id.
        if (novos.Count > 0)
        {
            await contexto.SaveChangesAsync(ct);

            foreach (var (cliente, loja, municipioId) in novos)
            {
                if (enderecoPorCliente.ContainsKey(cliente.Id)) continue;
                if (string.IsNullOrWhiteSpace(loja.Logradouro)) continue;

                // CEP INVÁLIDO NÃO IMPEDE O ENDEREÇO: o que importa para a tela é o MUNICÍPIO, que
                // já veio do catálogo. Recusar o endereço inteiro por causa de oito dígitos mal
                // digitados perderia o município junto — e é o município que põe o cliente no mapa.
                Cep? cep = Cep.TentarCriar(loja.Cep, out var cepLido) ? cepLido : null;

                contexto.Enderecos.Add(Endereco.Criar(
                    cliente.EmpresaId,
                    cliente.Id,
                    TipoDeEndereco.Fiscal,
                    loja.Logradouro,
                    MunicipioDoEndereco.Selecionado(municipioId),
                    loja.Uf,
                    usuarioId,
                    bairro: loja.Bairro,
                    cep: cep,
                    ehPrincipal: true));

                enderecos++;
            }
        }

        await contexto.SaveChangesAsync(ct);

        const string etapaDaGravacao = "3. Gravação";
        Contar(etapaDaGravacao, "clientes incluídos", incluidos);
        Contar(etapaDaGravacao, "clientes já existentes e atualizados", atualizados);
        Contar(etapaDaGravacao, "clientes já existentes sem alteração", iguais);
        Contar(etapaDaGravacao, "endereços principais incluídos", enderecos);
        Contar(etapaDaGravacao, "A1_PESSOA discorda do documento (valeu o documento)", tipoDivergente);

        var pendentes = pendentesPorMotivo.Values.Sum();
        Contar(etapaDaGravacao, "documentos pendentes (nenhum cliente criado)", pendentes);
        foreach (var (motivo, quantidade) in pendentesPorMotivo.OrderByDescending(p => p.Value))
            Contar(etapaDaGravacao, $"  pendência por motivo: {motivo}", quantidade);

        if (simular)
        {
            await transacao.RollbackAsync(ct);
            relatar("SIMULAÇÃO: nada foi gravado. Os números acima são os da carga real.");
        }
        else
        {
            await transacao.CommitAsync(ct);
        }

        return Resultado<RelatorioDaCargaDeClientes>.Ok(new RelatorioDaCargaDeClientes(
            lojas.Count, porDocumento.Count, incluidos, atualizados, iguais, enderecos, pendentes,
            pendentesPorMotivo, _contagens));

        void Pendente(string motivo) => pendentesPorMotivo[motivo] = pendentesPorMotivo.GetValueOrDefault(motivo) + 1;
    }

    /// <summary>
    /// A loja que representa o cliente: a mais completa, desempatada pelo menor código.
    ///
    /// <para>O DESEMPATE NÃO É ENFEITE. Sem ele, duas cargas seguidas podem escolher lojas
    /// diferentes com a mesma completude e ficar trocando o endereço do cliente de uma para outra,
    /// gerando trilha de alteração sem que nada tenha mudado na origem.</para>
    /// </summary>
    internal static LojaDeClienteNoProtheus EscolherAPrincipal(IEnumerable<LojaDeClienteNoProtheus> lojas) =>
        lojas
            .OrderByDescending(l => !l.Bloqueado)
            .ThenByDescending(l => string.IsNullOrWhiteSpace(l.Logradouro) ? 0 : 1)
            .ThenByDescending(l => string.IsNullOrWhiteSpace(l.Cep) ? 0 : 1)
            .ThenByDescending(l => string.IsNullOrWhiteSpace(l.InscricaoEstadual) ? 0 : 1)
            .ThenBy(l => l.Codigo, StringComparer.Ordinal)
            .ThenBy(l => l.Loja, StringComparer.Ordinal)
            .First();

    private void Contar(string etapa, string rotulo, int valor)
    {
        _contagens.Add((etapa, rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }
}

/// <summary>
/// O QUE A CARGA DA SA1 PRECISA SABER DO TERRITÓRIO para decidir se um documento vira cliente: o prefixo IBGE de
/// cada UF, o município pelo código do IBGE e a filial responsável por município da área de atuação.
///
/// <para><b>Por que separado da carga.</b> A sincronia das carteiras do Vórtice (24/09/2026) precisa da MESMA
/// decisão para dizer por que um cliente de carteira não está no CRM — fora da área de atuação, município não
/// reconhecido — e, na simulação, para projetar o que a carga da SA1 criaria. Uma segunda cópia da regra seria
/// uma regra que um dia discorda da primeira.</para>
/// </summary>
/// <param name="PrefixoDaUf">O prefixo IBGE de cada UF, tirado do catálogo de municípios.</param>
/// <param name="MunicipioPorIbge">O município do CRM pelo código do IBGE.</param>
/// <param name="EmpresaPorMunicipio">A filial responsável por município, só nas linhas vigentes.</param>
internal sealed record TerritorioDaCargaDeClientes(
    IReadOnlyDictionary<string, int> PrefixoDaUf,
    IReadOnlyDictionary<int, int> MunicipioPorIbge,
    IReadOnlyDictionary<int, int> EmpresaPorMunicipio)
{
    /// <summary>Lê o território do banco do CRM. Só leitura.</summary>
    /// <param name="contexto">O contexto do CRM.</param>
    /// <param name="ct">Cancelamento.</param>
    internal static async Task<TerritorioDaCargaDeClientes> LerAsync(CrmDbContext contexto, CancellationToken ct)
    {
        // O PREFIXO DA UF SAI DO CATÁLOGO, e não de uma tabela escrita no código: uma segunda lista
        // de UFs é uma lista que um dia discorda da primeira.
        var municipios = await contexto.Municipios.AsNoTracking()
            .Where(m => m.CodigoIbge != null)
            .Select(m => new { m.Id, CodigoIbge = m.CodigoIbge!.Value, m.Uf })
            .ToListAsync(ct);

        var municipioPorIbge = municipios.ToDictionary(m => m.CodigoIbge, m => m.Id);
        var prefixoDaUf = municipios
            .GroupBy(m => m.Uf, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().CodigoIbge / 100_000, StringComparer.OrdinalIgnoreCase);

        // A FILIAL RESPONSÁVEL POR CADA MUNICÍPIO. Só as linhas vigentes: um município que saiu da
        // área de atuação deixa de dar filial, e o cliente dele passa a ficar pendente — que é o
        // comportamento certo, e não um dono herdado de uma divisão que não existe mais.
        var empresaPorMunicipio = await contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
            .Where(a => a.EncerradoEm == null && a.EmpresaResponsavelId != null)
            .Select(a => new { a.MunicipioId, EmpresaId = a.EmpresaResponsavelId!.Value })
            .ToDictionaryAsync(a => a.MunicipioId, a => a.EmpresaId, ct);

        return new TerritorioDaCargaDeClientes(prefixoDaUf, municipioPorIbge, empresaPorMunicipio);
    }

    /// <summary>
    /// Decide se o documento vira cliente: devolve o motivo da pendência, ou o documento conferido com o município
    /// e a filial responsável.
    /// </summary>
    /// <param name="documento">O documento, só dígitos.</param>
    /// <param name="loja">A loja escolhida para representar o cliente.</param>
    internal (string? Motivo, CpfCnpj Documento, int MunicipioId, int EmpresaId) Situar(string documento, LojaDeClienteNoProtheus loja)
    {
        // O DÍGITO VERIFICADOR É CONFERIDO ANTES DE TUDO: o CRM identifica cliente pelo
        // documento, e um documento que ele recusa não tem como ser reencontrado na carga
        // seguinte. Vale mais deixá-lo na lista de pendências, com o motivo, do que criar um
        // cliente que ninguém consegue casar com nota nem com venda.
        if (!CpfCnpj.TentarCriar(documento, out var cpfCnpj))
            return (CargaDeClientesDoProtheus.DocumentoInvalido, default, 0, 0);

        if (string.IsNullOrWhiteSpace(loja.Uf) || !PrefixoDaUf.TryGetValue(loja.Uf, out var prefixo))
            return (CargaDeClientesDoProtheus.SemUf, cpfCnpj, 0, 0);

        var codigoIbge = LeitorDeClientesDoProtheus.CodigoIbge(prefixo, loja.CodigoDoMunicipio);
        if (codigoIbge is null || !MunicipioPorIbge.TryGetValue(codigoIbge.Value, out var municipioId))
            return (CargaDeClientesDoProtheus.MunicipioNaoReconhecido, cpfCnpj, 0, 0);

        if (!EmpresaPorMunicipio.TryGetValue(municipioId, out var empresaId))
            return (CargaDeClientesDoProtheus.ForaDaAreaDeAtuacao, cpfCnpj, municipioId, 0);

        return (null, cpfCnpj, municipioId, empresaId);
    }
}
