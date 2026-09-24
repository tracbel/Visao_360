using System.Collections.Frozen;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// O FATURAMENTO LIDO DA ORIGEM — a <c>SD2</c> do Protheus, direto no banco, em sessão de leitura.
///
/// ---------------------------------------------------------------------------------------------
/// POR QUE ESTE LEITOR SUBSTITUI O DO VÓRTICE. O CRM operou meses com "o faturamento parou em
/// 11/04/2025". A frase vinha de <c>X_TOTVS_CRM_FATURAMENTO</c>, a tabela que o Vórtice
/// <b>recebe</b> do Protheus — e essa de fato para naquela data. Medido na origem em 06/09/2026:
/// a <c>SD2</c> tem nota emitida na mesma semana. <b>O que morreu foi a integração, não o
/// faturamento.</b>
///
/// ---------------------------------------------------------------------------------------------
/// POR QUE O BANCO, E NÃO MAIS A API REST (24/09/2026). Até aqui a leitura passava pelo
/// <c>genericQuery</c> do AppServer (<see cref="PonteDoProtheus"/>): página de mil linhas, filial
/// por filial no cabeçalho <c>tenantId</c>, e a agregação feita aqui, linha a linha — 1,3 milhão de
/// itens de nota atravessando a rede para virar algumas dezenas de milhares de meses. Com o banco
/// direto (o mesmo acesso da carga de clientes, <see cref="LeitorDeClientesDoProtheus"/>), o
/// <c>GROUP BY</c> roda no próprio SQL Server do ERP e chega só o agregado: a leitura de três anos
/// levou <b>menos de dois segundos</b> na medição. A leitura REST de faturamento saiu; a
/// <see cref="PonteDoProtheus"/> continua existindo só para o botão "Testar" da conexão REST.
///
/// <para><b>SÓ LEITURA.</b> A conexão declara <c>ApplicationIntent=ReadOnly</c> e as consultas são
/// <c>SELECT</c> com <c>NOLOCK</c> — para não segurar trava num ERP em produção. Nada é escrito.</para>
///
/// ---------------------------------------------------------------------------------------------
/// A FILIAL DA VENDA É <c>D2_FILIAL</c> — e ela tem as dezesseis filiais, não uma.
///
/// <para>O comentário antigo da carga dizia que <c>D2_FILIAL</c> era <c>010101</c> em 100% das
/// notas. <b>Era efeito da leitura REST sem <c>tenantId</c></b>, que responde pela filial padrão do
/// nó — não um fato da origem. Medido no banco em 24/09/2026, venda (tipo <c>N</c> + CFOP de venda)
/// desde 24/09/2023, em R$ milhões: 010101 Ribeirão Preto 937,35 · 010102 Araraquara 65,83 · 010103
/// Barretos 108,34 · 010104 Guaíra 29,32 · 010105 Ituverava 23,18 · 010107 Orlândia 173,62 · 010109
/// Bebedouro 382,17 · 010110 Monte Alto 10,72 · 010111 Franca 115,52 · 010112 Itápolis 13,31 · 010113
/// São José do Rio Preto 184,78 · 010114 Catanduva 159,54 · 010115 Jales 84,61 · 010116 Votuporanga
/// 87,39 · 010117 Tupã 62,61 · 010118 Marília 63,40. As filiais 010113 a 010118 começam entre
/// novembro de 2024 e janeiro de 2025, quando entraram no ERP.</para>
///
/// <para><b>Por que <c>D2_FILIAL</c> e não outro campo.</b> <c>F2_FILIAL</c> é o mesmo valor (é a
/// chave que liga o cabeçalho ao item; os itens de venda têm cabeçalho em 100% dos casos menos um);
/// <c>F2_FILDEST</c> só existe na transferência; <c>VV0_FILIAL</c>, da saída de veículo, é igual à
/// <c>D2_FILIAL</c> da nota da máquina; <c>VV9_FILNEG</c> vem vazio; e o vendedor
/// (<c>F2_VEND1</c> → <c>SA3</c>) não tem filial: <c>A3_FILIAL</c> é <c>0101</c> para todos e
/// <c>A3_FILFUN</c> vem vazio. <b>A única filial que a venda tem no ERP é a que emitiu a nota.</b>
/// O extrator do BI (<c>(Extrator) QVDs Uteis.qvf</c>) usa a mesma coluna, com
/// <c>D2_FILIAL LIKE '0101%'</c>.</para>
///
/// <para><b>O código é o de <c>organizacao.Empresa</c>, sem de-para.</b> <c>D2_FILIAL</c> vem com
/// seis dígitos (<c>0101NN</c>), igual a <c>SYS_COMPANY.M0_CODFIL</c> e à view
/// <c>X_V_UTIL_FILIAIS.COD_FULL</c> do BI — as dezesseis conferidas nome a nome contra as empresas do
/// CRM em 24/09/2026. A <c>020101</c> ("TBA", outra raiz de CNPJ) aparece com cinco itens de venda,
/// o último em 03/2024: não tem empresa no CRM e sai no relatório da carga como descartada.</para>
///
/// <para><b>O que <c>D2_FILIAL</c> NÃO é: a filial do território do cliente.</b> A máquina é faturada
/// concentrada em 010101 e 010109: medido em três anos, 68,8% da máquina faturada por Ribeirão Preto
/// e 79,8% da faturada por Bebedouro foi para cliente de município de OUTRA filial da área de
/// atuação. Isso não é erro a corrigir aqui — é a diferença entre "filial da venda" e "filial do
/// cliente", que o CRM já trata como dois filtros independentes (documento 32, seção 8.5; a filial do
/// cliente é <c>Cliente.EmpresaId</c>, que vem da área de atuação).</para>
///
/// ---------------------------------------------------------------------------------------------
/// AS DUAS TABELAS, E POR QUE SÃO DUAS.
///
/// <c>SD2</c> é o item da nota fiscal de saída: filial, emissão, produto, grupo, cliente, valor.
/// Ela identifica o cliente por <b>código do Protheus</b> (<c>D2_CLIENTE</c> + <c>D2_LOJA</c>), e
/// o CRM identifica por CPF/CNPJ. Quem traduz é a <c>SA1</c>, o cadastro de clientes, onde
/// <c>A1_CGC</c> guarda o documento.
///
/// <para><b>A loja faz parte da chave, e não é detalhe.</b> Medido: o cliente <c>008096049</c>
/// tem as lojas 0001, 0004 e 0008 com <b>três CNPJs diferentes</b> — matriz e filiais do mesmo
/// grupo. Casar só por código atribuiria a nota da filial ao CNPJ da matriz.</para>
///
/// ---------------------------------------------------------------------------------------------
/// MÁQUINA OU PEÇA sai de <c>D2_GRUPO</c>, que viaja <b>na própria linha da nota</b> — não é
/// preciso juntar com a <c>SB1</c>, que tem 517 mil linhas. O catálogo dos grupos é a
/// <c>SBM</c>: <c>VEIC</c> é máquina, a faixa <c>1001..10xx</c> é peça por linha de produto,
/// <c>SRV</c> é serviço, <c>MO_O</c> é mão de obra de oficina.
///
/// <para><b>O vendedor fica para depois, e cabe aqui.</b> <c>SF2.F2_VEND1</c> tem o vendedor de
/// cada nota (preenchido em praticamente todo item de venda). Quando entrar, é mais uma coluna no
/// <c>GROUP BY</c> de <see cref="ConsultaDaVenda"/> — o grão continua sendo o mês.</para>
/// </summary>
/// <param name="opcoes">A configuração do banco do Protheus.</param>
public sealed class LeitorDeFaturamentoDoProtheus(OpcoesDoBancoDoProtheus opcoes)
{
    /// <summary>
    /// OS CFOPs QUE SÃO RECEITA DE VENDA. Tudo o que não estiver aqui fica de fora.
    ///
    /// <para><b>A lista mora só aqui.</b> As duas consultas (<see cref="ConsultaDaVenda"/> e o resumo
    /// do que ficou de fora) montam o filtro a partir dela, e <see cref="EhVenda"/> também: não há
    /// segunda cópia em SQL que possa discordar desta.</para>
    ///
    /// <para><b>Por que isto existe.</b> A SD2 guarda toda saída de mercadoria, e venda é só uma
    /// parte delas. Medido sobre 2024–2026: de R$ 1,27 bilhão que saiu pela porta, <b>menos de
    /// dois terços é venda</b>. O resto é transferência entre filiais, retorno de conserto,
    /// remessa para demonstração e baixa de estoque. Sem este filtro o painel da diretoria
    /// mostraria <b>meio bilhão de reais de faturamento que não existe</b>.</para>
    ///
    /// <para><b>É lista de inclusão, e não de exclusão</b>, de propósito. Com lista de exclusão,
    /// um CFOP novo que ninguém classificou entraria como venda e inflaria o número em silêncio.
    /// Com lista de inclusão ele fica de fora e o total de excluídos aparece no relatório da
    /// carga — o erro passa a ser para menos, e visível. Entre subestimar de forma visível e
    /// superestimar em silêncio, este projeto escolhe o primeiro.</para>
    ///
    /// <para>O que está aqui: venda de mercadoria (x102, x108, x117), venda com substituição
    /// tributária (x403, x405), venda de combustível e lubrificante (x656, x659) e prestação de
    /// serviço (x933). O primeiro dígito é 5 dentro do estado e 6 fora — os dois entram.</para>
    ///
    /// <para><b>A varredura completa, medida em 06/09/2026 sobre as 205.169 linhas emitidas desde
    /// 01/01/2024.</b> O que a lista aceita, por ordem de valor: 5102 R$ 552,1 mi (42.804 itens),
    /// 5405 R$ 117,1 mi (97.439 — é peça, muito item e pouco valor), 6933 R$ 63,2 mi, 5933
    /// R$ 45,3 mi, 6102 R$ 32,0 mi, 5656 R$ 7,0 mi, e o resto abaixo de R$ 3 mi.</para>
    ///
    /// <para>O que a lista recusa, e por quê: 5152 transferência entre filiais R$ 176,2 mi; 5916
    /// retorno de conserto R$ 172,7 mi (é máquina que entrou para reparo e voltou — nunca foi
    /// vendida); 5912 e 5914 remessa para demonstração e feira R$ 105,4 mi; 5409 transferência com
    /// ST R$ 30,1 mi; 5927 baixa de estoque R$ 8,8 mi; 5910 bonificação e brinde R$ 6,0 mi.</para>
    ///
    /// <para><b>O 5949 é uma pergunta em aberto, não uma decisão.</b> "Outra saída não
    /// especificada": 5.221 itens, R$ 38,1 mi. Fica fora pela mesma regra — na dúvida, para
    /// menos e visível —, e o valor sai no relatório da carga. <b>Precisa da resposta do fiscal
    /// antes de virar decisão definitiva.</b> E a regra inteira ainda não foi conferida contra a do
    /// BI: o extrator do BI não filtra CFOP nem TES — o que é venda, para ele, está nas medidas do
    /// aplicativo, que não foram lidas.</para>
    /// </summary>
    public static readonly FrozenSet<string> CfopsDeVenda = new[]
    {
        "5102", "6102", "5108", "6108", "5117", "6117",
        "5403", "6403", "5405", "6405",
        "5656", "6656", "5659", "6659",
        "5933", "6933"
    }.ToFrozenSet(StringComparer.Ordinal);

    /// <summary>
    /// O tipo de nota que é venda.
    ///
    /// <para><c>N</c> é nota normal. Medido em 2026: <c>B</c> (beneficiamento e devolução de
    /// compra) somou R$ 39 milhões em 107 itens, e <c>D</c> é devolução. Nenhum dos dois é
    /// receita de venda, e o valor alto do <c>B</c> mostra o estrago que ele faria no total.</para>
    /// </summary>
    public const string TipoDeVenda = "N";

    /// <summary>Se a linha da nota é venda: tipo <see cref="TipoDeVenda"/> e CFOP da lista.</summary>
    /// <param name="tipo">O <c>D2_TIPO</c>.</param>
    /// <param name="cfop">O <c>D2_CF</c>.</param>
    public static bool EhVenda(string? tipo, string? cfop) =>
        string.Equals(tipo?.Trim(), TipoDeVenda, StringComparison.Ordinal)
        && cfop is not null && CfopsDeVenda.Contains(cfop.Trim());

    /// <summary>
    /// O filtro de venda em SQL, montado de <see cref="CfopsDeVenda"/> e <see cref="TipoDeVenda"/>.
    ///
    /// <para>Os valores são constantes deste arquivo, nunca entrada de fora — por isso podem ir
    /// literais. <c>D2_CF</c> é <c>varchar(5)</c> e o SQL Server compara ignorando espaço à direita,
    /// então o <c>IN</c> dispensa <c>RTRIM</c> e o índice <c>SD2_SUGESTAO_02</c> continua
    /// servindo.</para>
    /// </summary>
    private static readonly string FiltroDeVenda =
        $"d.D2_TIPO = '{TipoDeVenda}' AND d.D2_CF IN ({string.Join(", ", CfopsDeVenda.Order(StringComparer.Ordinal).Select(c => $"'{c}'"))})";

    /// <summary>
    /// A VENDA AGREGADA NA ORIGEM: uma linha por filial, mês, código + loja do cliente e grupo.
    ///
    /// <para><b>Por que o grupo fica no <c>GROUP BY</c> e a classificação não vai para o SQL.</b> A
    /// regra "VEIC é máquina, 1xxx é peça" mora em <see cref="Classificar"/>; escrevê-la de novo num
    /// <c>CASE</c> seria a segunda cópia que um dia discorda. Por mês e cliente são poucos grupos, e a
    /// transferência continua pequena.</para>
    ///
    /// <para><b>As notas são contadas à parte</b> (<c>notas</c>), por filial, mês e cliente + loja:
    /// a mesma nota tem item de máquina e de peça, e contá-la dentro de cada grupo a contaria duas
    /// vezes. Entre lojas a soma é exata, porque cada nota tem um cliente e uma loja só.</para>
    ///
    /// <para><b>O valor é convertido item a item</b> para <c>decimal(18,2)</c> antes de somar:
    /// <c>D2_TOTAL</c> é <c>float</c> na origem, e somar ponto flutuante acumularia resíduo que a
    /// restrição "a quebra fecha com o total" do CRM recusaria.</para>
    ///
    /// <para>A <c>SA1</c> entra por <c>LEFT JOIN</c>: o código sem cadastro não some, chega sem
    /// documento e é contado — com o valor — no relatório da carga.</para>
    /// </summary>
    public static readonly string ConsultaDaVenda = $"""
        WITH venda AS (
            SELECT d.D2_FILIAL AS Filial,
                   LEFT(d.D2_EMISSAO, 6) AS Mes,
                   d.D2_CLIENTE AS Cliente,
                   d.D2_LOJA AS Loja,
                   d.D2_GRUPO AS Grupo,
                   CAST(d.D2_TOTAL AS decimal(18, 2)) AS Valor,
                   d.D2_SERIE + '|' + d.D2_DOC AS Nota
            FROM dbo.SD2010 d WITH (NOLOCK)
            WHERE d.D_E_L_E_T_ = ' ' AND d.D2_EMISSAO >= @desde AND {FiltroDeVenda}
        ),
        porGrupo AS (
            SELECT Filial, Mes, Cliente, Loja, Grupo, SUM(Valor) AS Valor, COUNT(*) AS Itens
            FROM venda
            GROUP BY Filial, Mes, Cliente, Loja, Grupo
        ),
        notas AS (
            SELECT Filial, Mes, Cliente, Loja, COUNT(DISTINCT Nota) AS Notas
            FROM venda
            GROUP BY Filial, Mes, Cliente, Loja
        )
        SELECT RTRIM(g.Filial), g.Mes, RTRIM(g.Cliente), RTRIM(g.Loja), RTRIM(a.A1_CGC), RTRIM(a.A1_NOME),
               RTRIM(g.Grupo), g.Valor, g.Itens, n.Notas
        FROM porGrupo g
        JOIN notas n ON n.Filial = g.Filial AND n.Mes = g.Mes AND n.Cliente = g.Cliente AND n.Loja = g.Loja
        LEFT JOIN dbo.SA1010 a WITH (NOLOCK)
               ON a.D_E_L_E_T_ = ' ' AND a.A1_COD = g.Cliente AND a.A1_LOJA = g.Loja
        """;

    /// <summary>
    /// O QUE SAIU PELA PORTA E NÃO É VENDA — para o relatório da carga dizer quanto dinheiro ficou de
    /// fora, e não só quantas linhas. E a nota mais recente, que é a marca de que a origem está viva.
    /// </summary>
    public static readonly string ConsultaDoResumo = $"""
        SELECT COUNT(*),
               SUM(CASE WHEN {FiltroDeVenda} THEN 0 ELSE 1 END),
               SUM(CASE WHEN {FiltroDeVenda} THEN 0 ELSE CAST(d.D2_TOTAL AS decimal(18, 2)) END),
               MAX(d.D2_EMISSAO)
        FROM dbo.SD2010 d WITH (NOLOCK)
        WHERE d.D_E_L_E_T_ = ' ' AND d.D2_EMISSAO >= @desde
        """;

    /// <summary>
    /// Máquina, peça ou serviço — pelo grupo de produto que viaja na linha da nota.
    ///
    /// <para>A pergunta do negócio era "qual das SB separa máquina de peça". A resposta medida é
    /// que <b>nenhuma separa</b>: quem separa é o grupo, catálogo <c>SBM</c> (273 grupos), e ele
    /// está no próprio <c>D2_GRUPO</c> da nota.</para>
    ///
    /// <para><c>VEIC</c> é máquina. A faixa <c>1001</c> a <c>10xx</c> é peça, uma por linha de
    /// produto (colhedora de cana, trator, colheitadeira de grãos, plantadeira, plataforma, gator,
    /// pulverizador, implemento). <c>SRV</c>, <c>MO_O</c> e <c>MO_T</c> são serviço e mão de obra.
    /// <c>AMS</c>, <c>FR</c> e <c>JD</c> existem e não se encaixam em nenhum dos três — vão para
    /// "outros", <b>que é o que impede a quebra de fechar com menos que o total</b>.</para>
    /// </summary>
    /// <param name="grupo">O <c>D2_GRUPO</c> da linha.</param>
    public static GrupoDaNota Classificar(string? grupo) => grupo?.Trim() switch
    {
        "VEIC" => GrupoDaNota.Maquina,
        "SRV" or "MO_O" or "MO_T" => GrupoDaNota.Servico,
        // A FAIXA DE PEÇA É NUMÉRICA e cresce: o negócio abre um grupo novo a cada linha de
        // produto que entra. Testar o formato em vez de listar os códigos é o que faz o grupo
        // 1042 de amanhã já entrar como peça, em vez de cair em "outros" sem ninguém notar.
        { Length: 4 } g when g.All(char.IsAsciiDigit) && g[0] == '1' => GrupoDaNota.Peca,
        _ => GrupoDaNota.Outros
    };

    /// <summary>
    /// Lê o faturamento a partir de uma data, agregado por cliente, filial e mês.
    /// </summary>
    /// <param name="desde">A primeira emissão a considerar — o dia 1 de um mês, para que nenhum mês
    /// da janela chegue pela metade.</param>
    /// <param name="relatar">Para dizer o que está acontecendo.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LoteDoProtheus>> LerAsync(DateOnly desde, Action<string> relatar, CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<LoteDoProtheus>.Indisponivel(
                "A leitura do faturamento do Protheus exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha " +
                "(ou a conexão \"Protheus — banco (leitura)\" configurada em Configurações > Integrações).");

        var construtor = new SqlConnectionStringBuilder
        {
            DataSource = opcoes.Servidor,
            InitialCatalog = opcoes.Banco,
            UserID = opcoes.Usuario,
            Password = opcoes.Senha,
            TrustServerCertificate = true,
            ApplicationIntent = ApplicationIntent.ReadOnly,
            ConnectTimeout = 15
        };

        relatar($"  lendo as notas de venda emitidas desde {desde:dd/MM/yyyy} (SD2 + SA1, agregado no banco do Protheus)…");

        try
        {
            await using var conexao = new SqlConnection(construtor.ConnectionString);
            await conexao.OpenAsync(ct);

            var linhas = new List<VendaAgregadaNoProtheus>(200_000);
            await using (var comando = new SqlCommand(ConsultaDaVenda, conexao) { CommandTimeout = 600 })
            {
                comando.Parameters.Add(ParametroDaData(desde));
                await using var leitor = await comando.ExecuteReaderAsync(ct);
                while (await leitor.ReadAsync(ct))
                {
                    string? Opcional(int i) => leitor.IsDBNull(i) ? null : leitor.GetString(i);

                    linhas.Add(new VendaAgregadaNoProtheus(
                        leitor.GetString(0),
                        leitor.GetString(1),
                        leitor.GetString(2),
                        leitor.GetString(3),
                        Opcional(4),
                        Opcional(5),
                        Opcional(6) ?? string.Empty,
                        leitor.GetDecimal(7),
                        leitor.GetInt32(8),
                        leitor.GetInt32(9)));
                }
            }

            ResumoDaSd2 resumo;
            await using (var comando = new SqlCommand(ConsultaDoResumo, conexao) { CommandTimeout = 600 })
            {
                comando.Parameters.Add(ParametroDaData(desde));
                await using var leitor = await comando.ExecuteReaderAsync(ct);
                await leitor.ReadAsync(ct);
                resumo = new ResumoDaSd2(
                    leitor.GetInt32(0),
                    leitor.IsDBNull(1) ? 0 : leitor.GetInt32(1),
                    leitor.IsDBNull(2) ? 0m : leitor.GetDecimal(2),
                    leitor.IsDBNull(3) ? null : Data(leitor.GetString(3)));
            }

            var lote = Agregar(desde, linhas, resumo);

            relatar(
                $"    {lote.ItensLidos:N0} itens de nota no período · {lote.ItensQueNaoSaoVenda:N0} descartados por não " +
                $"serem venda (R$ {lote.ValorQueNaoEhVenda:N2} em transferência, remessa e devolução).");
            relatar(
                $"    {lote.Faturamento.Count:N0} meses de faturamento por cliente, em {lote.FiliaisVistas.Count} filiais · " +
                $"{lote.CodigosSemDocumento:N0} código(s) de cliente sem documento válido na SA1 (R$ {lote.ValorSemDocumento:N2}).");

            return Resultado<LoteDoProtheus>.Ok(lote);
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código.
            return Resultado<LoteDoProtheus>.Indisponivel(
                $"O banco do Protheus não respondeu à leitura do faturamento (erro SQL {falha.Number}). Nada foi gravado.");
        }
    }

    /// <summary>
    /// Junta as linhas agregadas da origem no grão do CRM: documento, filial e mês.
    ///
    /// <para><b>Público e puro de propósito</b>: é aqui que mora toda a regra de agregação, e ela é
    /// testada sem banco.</para>
    /// </summary>
    /// <param name="desde">O primeiro dia lido.</param>
    /// <param name="linhas">As linhas de <see cref="ConsultaDaVenda"/>.</param>
    /// <param name="resumo">O que <see cref="ConsultaDoResumo"/> contou.</param>
    public static LoteDoProtheus Agregar(DateOnly desde, IEnumerable<VendaAgregadaNoProtheus> linhas, ResumoDaSd2 resumo)
    {
        var porChave = new Dictionary<(string Documento, string Filial, DateOnly Mes), Acumulado>();
        var nomePorDocumento = new Dictionary<string, (string Codigo, string Nome)>(StringComparer.Ordinal);
        var codigosSemDocumento = new HashSet<string>(StringComparer.Ordinal);
        int itensSemDocumento = 0, itensSemData = 0;
        var valorSemDocumento = 0m;

        foreach (var linha in linhas)
        {
            var mes = Mes(linha.Mes);
            if (mes is null) { itensSemData += linha.Itens; continue; }

            // SEM DOCUMENTO NÃO HÁ A QUEM ATRIBUIR. O CRM identifica cliente por CPF/CNPJ, e o
            // código sem cadastro na SA1 (ou com documento que não é CPF nem CNPJ) não casa com
            // nada. Fica de fora — com o valor contado, para o relatório dizer quanto.
            var documento = LeitorDeClientesDoProtheus.SoDigitos(linha.Documento);
            if (documento.Length is not (11 or 14))
            {
                codigosSemDocumento.Add($"{linha.Cliente}/{linha.Loja}");
                itensSemDocumento += linha.Itens;
                valorSemDocumento += linha.Valor;
                continue;
            }

            // O MESMO CNPJ APARECE EM VÁRIAS LOJAS com o nome escrito de jeitos diferentes. Fica o
            // da loja de menor código: qualquer um serve para uma pessoa reconhecer de quem se trata,
            // e escolher sempre o mesmo impede o nome de mudar de carga para carga sem motivo.
            var codigo = $"{linha.Cliente}/{linha.Loja}";
            if (!string.IsNullOrWhiteSpace(linha.Nome)
                && (!nomePorDocumento.TryGetValue(documento, out var atual) || string.CompareOrdinal(codigo, atual.Codigo) < 0))
                nomePorDocumento[documento] = (codigo, linha.Nome.Trim());

            var chave = (documento, linha.Filial, mes.Value);
            if (!porChave.TryGetValue(chave, out var acumulado))
                acumulado = porChave[chave] = new Acumulado();

            acumulado.Valor += linha.Valor;
            acumulado.Itens += linha.Itens;

            // A CONTAGEM DE NOTAS VEM REPETIDA em cada grupo da mesma loja no mês: soma uma vez só.
            if (acumulado.Lojas.Add(codigo)) acumulado.Notas += linha.Notas;

            switch (Classificar(linha.Grupo))
            {
                case GrupoDaNota.Maquina: acumulado.Maquina += linha.Valor; break;
                case GrupoDaNota.Peca: acumulado.Peca += linha.Valor; break;
                case GrupoDaNota.Servico: acumulado.Servico += linha.Valor; break;
                default: acumulado.Outros += linha.Valor; break;
            }
        }

        var agregado = porChave
            // VALOR ZERO OU NEGATIVO NÃO ENTRA. Um mês que fecha em zero para um cliente não é
            // faturamento — e o banco do CRM recusaria o negativo.
            .Where(p => p.Value.Valor > 0)
            .OrderBy(p => p.Key.Mes).ThenBy(p => p.Key.Filial, StringComparer.Ordinal).ThenBy(p => p.Key.Documento, StringComparer.Ordinal)
            .Select(p => new FaturamentoParaCarga(
                ChaveDeOrigem: $"{p.Key.Documento}/{p.Key.Filial}/{p.Key.Mes:yyyy-MM}",
                DocumentoDoCliente: p.Key.Documento,
                // A FILIAL VEM COMO CÓDIGO DE SEIS DÍGITOS ('010101'), que é exatamente o código
                // de `organizacao.Empresa`. O campo numérico do Vórtice fica em zero.
                CodigoDaFilialNoLegado: 0,
                CodigoDaFilial: p.Key.Filial,
                Competencia: p.Key.Mes,
                ValorLiquido: decimal.Round(p.Value.Valor, 2),
                Notas: p.Value.Notas,
                Itens: p.Value.Itens,
                NomeNaOrigem: nomePorDocumento.TryGetValue(p.Key.Documento, out var nome) ? nome.Nome : string.Empty,
                Quebra: p.Value.Quebrar()))
            .ToList();

        return new LoteDoProtheus(
            desde,
            agregado,
            [.. porChave.Keys.Select(k => k.Filial).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
            resumo.ItensLidos,
            codigosSemDocumento.Count,
            itensSemDocumento,
            decimal.Round(valorSemDocumento, 2),
            itensSemData,
            resumo.ItensQueNaoSaoVenda,
            decimal.Round(resumo.ValorQueNaoEhVenda, 2),
            resumo.EmissaoMaisRecente);
    }

    private sealed class Acumulado
    {
        public decimal Valor;
        public decimal Maquina;
        public decimal Peca;
        public decimal Servico;
        public decimal Outros;
        public int Itens;
        public int Notas;
        public readonly HashSet<string> Lojas = new(StringComparer.Ordinal);

        /// <summary>
        /// A quebra do mês, arredondada de forma a fechar com o total.
        ///
        /// <para>As três primeiras parcelas são arredondadas e a última recebe a diferença. Somar
        /// quatro valores arredondados independentemente erra até dois centavos contra o total —
        /// o bastante para a restrição do banco recusar a linha inteira.</para>
        /// </summary>
        public QuebraDoFaturamento Quebrar()
        {
            var maquina = decimal.Round(Maquina, 2);
            var peca = decimal.Round(Peca, 2);
            var servico = decimal.Round(Servico, 2);

            return new QuebraDoFaturamento(
                maquina, peca, servico,
                decimal.Round(Valor, 2) - maquina - peca - servico);
        }
    }

    private static SqlParameter ParametroDaData(DateOnly desde) =>
        // O MESMO TIPO DA COLUNA: `D2_EMISSAO` é varchar(8) 'yyyyMMdd'. Um nvarchar aqui faria o SQL
        // Server converter a coluna inteira em vez do parâmetro, e o índice por emissão deixaria de
        // servir.
        new("@desde", System.Data.SqlDbType.VarChar, 8) { Value = desde.ToString("yyyyMMdd", CultureInfo.InvariantCulture) };

    /// <summary>O mês <c>yyyyMM</c> da emissão; nulo quando a origem gravou algo que não é data.</summary>
    private static DateOnly? Mes(string bruto) =>
        DateOnly.TryParseExact(bruto + "01", "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes)
            ? mes
            : null;

    /// <summary>
    /// A data <c>yyyyMMdd</c> como o Protheus a grava. Uma data não reconhecida vira nulo — nunca
    /// <c>DateTime.Today</c>, que daria uma marca de sincronismo falsa.
    /// </summary>
    private static DateOnly? Data(string bruto) =>
        DateOnly.TryParseExact(bruto.Trim(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
            ? data
            : null;
}

/// <summary>O que a linha da nota vendeu, na divisão que o negócio usa.</summary>
public enum GrupoDaNota
{
    /// <summary>Grupo <c>VEIC</c>.</summary>
    Maquina,

    /// <summary>A faixa numérica <c>1001..10xx</c>.</summary>
    Peca,

    /// <summary><c>SRV</c> e mão de obra.</summary>
    Servico,

    /// <summary>Grupo que ainda não sabemos ler.</summary>
    Outros
}

/// <summary>Uma linha de <see cref="LeitorDeFaturamentoDoProtheus.ConsultaDaVenda"/>, como veio.</summary>
/// <param name="Filial">O <c>D2_FILIAL</c>, seis dígitos.</param>
/// <param name="Mes">O mês da emissão, <c>yyyyMM</c>.</param>
/// <param name="Cliente">O <c>D2_CLIENTE</c>.</param>
/// <param name="Loja">O <c>D2_LOJA</c>.</param>
/// <param name="Documento">O <c>A1_CGC</c>; nulo quando o código não está na SA1.</param>
/// <param name="Nome">O <c>A1_NOME</c>.</param>
/// <param name="Grupo">O <c>D2_GRUPO</c>.</param>
/// <param name="Valor">A soma do <c>D2_TOTAL</c>.</param>
/// <param name="Itens">Quantos itens de nota.</param>
/// <param name="Notas">Quantas notas distintas a loja teve no mês, na filial — repetido em cada grupo.</param>
public sealed record VendaAgregadaNoProtheus(
    string Filial,
    string Mes,
    string Cliente,
    string Loja,
    string? Documento,
    string? Nome,
    string Grupo,
    decimal Valor,
    int Itens,
    int Notas);

/// <summary>O que <see cref="LeitorDeFaturamentoDoProtheus.ConsultaDoResumo"/> contou na janela.</summary>
/// <param name="ItensLidos">Todos os itens de nota de saída emitidos na janela.</param>
/// <param name="ItensQueNaoSaoVenda">Os que o filtro de venda recusou.</param>
/// <param name="ValorQueNaoEhVenda">Quanto eles somam.</param>
/// <param name="EmissaoMaisRecente">A emissão mais recente vista.</param>
public sealed record ResumoDaSd2(int ItensLidos, int ItensQueNaoSaoVenda, decimal ValorQueNaoEhVenda, DateOnly? EmissaoMaisRecente);

/// <summary>
/// O que uma leitura de faturamento do Protheus produziu.
/// </summary>
/// <param name="Desde">O primeiro dia lido. As competências a partir dele vieram inteiras da origem — é
/// a janela que a carga substitui.</param>
/// <param name="Faturamento">O agregado por cliente, filial e mês.</param>
/// <param name="FiliaisVistas">Os códigos de filial encontrados nas notas de venda.</param>
/// <param name="ItensLidos">Quantos itens de nota a origem tem na janela, venda ou não.</param>
/// <param name="CodigosSemDocumento">Códigos de cliente (código/loja) da nota sem CPF/CNPJ válido na SA1.</param>
/// <param name="ItensSemDocumento">Itens de venda desses códigos.</param>
/// <param name="ValorSemDocumento">Quanto eles somam — venda que existe e não tem a quem ser atribuída.</param>
/// <param name="ItensSemData">Itens descartados por não ter data de emissão legível.</param>
/// <param name="ItensQueNaoSaoVenda">Itens de saída que não são venda — transferência, remessa,
/// devolução, baixa de estoque.</param>
/// <param name="ValorQueNaoEhVenda">Quanto esses itens somam. Vai para o relatório da carga
/// porque é grande: mais de um terço do que sai pela porta.</param>
/// <param name="EmissaoMaisRecente">A nota mais nova vista. Vira a marca de sincronismo.</param>
public sealed record LoteDoProtheus(
    DateOnly Desde,
    IReadOnlyList<FaturamentoParaCarga> Faturamento,
    IReadOnlyList<string> FiliaisVistas,
    int ItensLidos,
    int CodigosSemDocumento,
    int ItensSemDocumento,
    decimal ValorSemDocumento,
    int ItensSemData,
    int ItensQueNaoSaoVenda,
    decimal ValorQueNaoEhVenda,
    DateOnly? EmissaoMaisRecente);
