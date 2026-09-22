using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// A FROTA DE TRATORES DE UM MUNICÍPIO, numa faixa de potência — Censo Agropecuário do IBGE
/// (tabela SIDRA 6871, classificação 12605).
///
/// <para><b>Por que existe.</b> O potencial de mercado precisa saber quantas máquinas JÁ EXISTEM na
/// região, e não só quanta área há para mecanizar: o parque instalado é o que se renova. Nenhuma
/// fonte interna sabe disso — o CRM só enxerga o que a Tracbel vendeu.</para>
///
/// <para><b>O ano é 2017 e vai continuar sendo.</b> O Censo Agropecuário é decenal; o próximo
/// resultado sai em 2028. A coluna do ano existe para que o dado não minta sobre a própria idade, e
/// para que 2028 entre sem migração.</para>
///
/// <para><b>Cuidado ao somar:</b> a faixa "Total" é uma das três categorias, ao lado de "Menos de
/// 100 cv" e "De 100 cv e mais". Somar as três conta o parque duas vezes. E o Total <b>não</b> é
/// sempre a soma das outras duas: o IBGE divulga o total e oculta a parte quando o sigilo se aplica a
/// poucos estabelecimentos — por isso as três ficam guardadas.</para>
/// </summary>
public sealed class FrotaDeTratoresNoMunicipio
{
    private FrotaDeTratoresNoMunicipio() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O município.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>O ano do Censo.</summary>
    public short Ano { get; private set; }

    /// <summary>A categoria da classificação 12605. 113521 é o total, 113522 abaixo de 100 cv.</summary>
    public int PotenciaCodigoIbge { get; private set; }

    /// <summary>O rótulo oficial da faixa.</summary>
    public string PotenciaNome { get; private set; } = default!;

    /// <summary>Estabelecimentos que declararam ter trator. Nulo é sigilo ou não disponível.</summary>
    public int? EstabelecimentosComTrator { get; private set; }

    /// <summary>Tratores existentes. Nulo é sigilo ou não disponível.</summary>
    public int? Tratores { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra a linha de um município, ano e faixa de potência.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="ano">O ano do Censo.</param>
    /// <param name="potenciaCodigoIbge">A categoria da classificação 12605.</param>
    /// <param name="potenciaNome">O rótulo oficial.</param>
    /// <param name="estabelecimentosComTrator">Estabelecimentos com trator, ou nulo.</param>
    /// <param name="tratores">Tratores, ou nulo.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o ano, o código ou uma contagem não valem.</exception>
    public static FrotaDeTratoresNoMunicipio Registrar(
        int municipioId, short ano, int potenciaCodigoIbge, string potenciaNome,
        int? estabelecimentosComTrator, int? tratores, long importadoPorId, DateTime agoraUtc)
    {
        MedicaoDoIbge.ConferirAno(ano);
        MedicaoDoIbge.ConferirCategoria(potenciaCodigoIbge, potenciaNome);
        MedicaoDoIbge.ConferirContagem(estabelecimentosComTrator, nameof(estabelecimentosComTrator));
        MedicaoDoIbge.ConferirContagem(tratores, nameof(tratores));

        return new FrotaDeTratoresNoMunicipio
        {
            MunicipioId = municipioId,
            Ano = ano,
            PotenciaCodigoIbge = potenciaCodigoIbge,
            PotenciaNome = potenciaNome.Trim(),
            EstabelecimentosComTrator = estabelecimentosComTrator,
            Tratores = tratores,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>Confere a linha contra uma nova leitura e diz se alguma coisa mudou.</summary>
    /// <param name="potenciaNome">O rótulo da nova leitura.</param>
    /// <param name="estabelecimentosComTrator">Estabelecimentos da nova leitura.</param>
    /// <param name="tratores">Tratores da nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public bool Reapurar(
        string potenciaNome, int? estabelecimentosComTrator, int? tratores,
        long importadoPorId, DateTime agoraUtc)
    {
        MedicaoDoIbge.ConferirContagem(estabelecimentosComTrator, nameof(estabelecimentosComTrator));
        MedicaoDoIbge.ConferirContagem(tratores, nameof(tratores));

        var mudou = PotenciaNome != potenciaNome.Trim()
                    || EstabelecimentosComTrator != estabelecimentosComTrator
                    || Tratores != tratores;

        PotenciaNome = potenciaNome.Trim();
        EstabelecimentosComTrator = estabelecimentosComTrator;
        Tratores = tratores;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;

        return mudou;
    }
}

/// <summary>
/// OS ESTABELECIMENTOS AGROPECUÁRIOS DE UM MUNICÍPIO, num grupo de área total — Censo Agropecuário
/// (tabela SIDRA 6780, classificação 220).
///
/// <para><b>Por que existe.</b> Tamanho de propriedade é o que separa o trator de 75 cv do de 200:
/// a distribuição por faixa diz que máquina cabe naquele município. A planilha do comercial reagrupa
/// as faixas do IBGE em nove; aqui ficam as <b>18 originais</b> mais "produtor sem área" e o total,
/// para que qualquer reagrupamento seja refazível sem nova carga.</para>
///
/// <para><b>A faixa "mais de 2.500 ha" da planilha é a soma de DUAS</b> categorias do IBGE — "De
/// 2.500 a menos de 10.000 ha" e "De 10.000 ha e mais". Quem reagrupa soma; o dado bruto fica como o
/// IBGE publica.</para>
/// </summary>
public sealed class EstabelecimentosPorAreaNoMunicipio
{
    private EstabelecimentosPorAreaNoMunicipio() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O município.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>O ano do Censo.</summary>
    public short Ano { get; private set; }

    /// <summary>A categoria da classificação 220. 110085 é o total; 111560 é "produtor sem área".</summary>
    public int GrupoDeAreaCodigoIbge { get; private set; }

    /// <summary>O rótulo oficial da faixa.</summary>
    public string GrupoDeAreaNome { get; private set; } = default!;

    /// <summary>Estabelecimentos na faixa. Nulo é sigilo ou não disponível.</summary>
    public int? Estabelecimentos { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra a linha de um município, ano e grupo de área.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="ano">O ano do Censo.</param>
    /// <param name="grupoDeAreaCodigoIbge">A categoria da classificação 220.</param>
    /// <param name="grupoDeAreaNome">O rótulo oficial.</param>
    /// <param name="estabelecimentos">A contagem, ou nulo.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o ano, o código ou a contagem não valem.</exception>
    public static EstabelecimentosPorAreaNoMunicipio Registrar(
        int municipioId, short ano, int grupoDeAreaCodigoIbge, string grupoDeAreaNome,
        int? estabelecimentos, long importadoPorId, DateTime agoraUtc)
    {
        MedicaoDoIbge.ConferirAno(ano);
        MedicaoDoIbge.ConferirCategoria(grupoDeAreaCodigoIbge, grupoDeAreaNome);
        MedicaoDoIbge.ConferirContagem(estabelecimentos, nameof(estabelecimentos));

        return new EstabelecimentosPorAreaNoMunicipio
        {
            MunicipioId = municipioId,
            Ano = ano,
            GrupoDeAreaCodigoIbge = grupoDeAreaCodigoIbge,
            GrupoDeAreaNome = grupoDeAreaNome.Trim(),
            Estabelecimentos = estabelecimentos,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>Confere a linha contra uma nova leitura e diz se alguma coisa mudou.</summary>
    /// <param name="grupoDeAreaNome">O rótulo da nova leitura.</param>
    /// <param name="estabelecimentos">A contagem da nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public bool Reapurar(string grupoDeAreaNome, int? estabelecimentos, long importadoPorId, DateTime agoraUtc)
    {
        MedicaoDoIbge.ConferirContagem(estabelecimentos, nameof(estabelecimentos));

        var mudou = GrupoDeAreaNome != grupoDeAreaNome.Trim() || Estabelecimentos != estabelecimentos;

        GrupoDeAreaNome = grupoDeAreaNome.Trim();
        Estabelecimentos = estabelecimentos;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;

        return mudou;
    }
}

/// <summary>
/// O EFETIVO DE UM REBANHO NUM MUNICÍPIO — Pesquisa da Pecuária Municipal (tabela SIDRA 3939).
///
/// <para><b>Por que existe, e por que não veio junto do Censo.</b> Pecuária também mecaniza, e a
/// PPM é <b>anual</b>: ela sabe de 2024 enquanto o Censo ainda fala de 2017. Misturar as duas numa
/// tabela só faria a coluna do ano significar coisas diferentes na mesma linha.</para>
///
/// <para>A carga traz hoje só o bovino, que é o que a issue 65 pede. A tabela guarda o tipo de
/// rebanho, então incluir bubalino ou equino é mudar uma lista no leitor, não o modelo.</para>
/// </summary>
public sealed class RebanhoNoMunicipio
{
    private RebanhoNoMunicipio() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O município.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>O ano da pesquisa.</summary>
    public short Ano { get; private set; }

    /// <summary>A categoria da classificação 79. 2670 é bovino.</summary>
    public int RebanhoCodigoIbge { get; private set; }

    /// <summary>O rótulo oficial do tipo de rebanho.</summary>
    public string RebanhoNome { get; private set; } = default!;

    /// <summary>O efetivo, em cabeças. Nulo é sigilo ou não disponível.</summary>
    public int? Cabecas { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra o efetivo de um rebanho num município e ano.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    /// <param name="rebanhoCodigoIbge">A categoria da classificação 79.</param>
    /// <param name="rebanhoNome">O rótulo oficial.</param>
    /// <param name="cabecas">O efetivo, ou nulo.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o ano, o código ou o efetivo não valem.</exception>
    public static RebanhoNoMunicipio Registrar(
        int municipioId, short ano, int rebanhoCodigoIbge, string rebanhoNome,
        int? cabecas, long importadoPorId, DateTime agoraUtc)
    {
        MedicaoDoIbge.ConferirAno(ano);
        MedicaoDoIbge.ConferirCategoria(rebanhoCodigoIbge, rebanhoNome);
        MedicaoDoIbge.ConferirContagem(cabecas, nameof(cabecas));

        return new RebanhoNoMunicipio
        {
            MunicipioId = municipioId,
            Ano = ano,
            RebanhoCodigoIbge = rebanhoCodigoIbge,
            RebanhoNome = rebanhoNome.Trim(),
            Cabecas = cabecas,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>Confere a linha contra uma nova leitura e diz se alguma coisa mudou.</summary>
    /// <param name="rebanhoNome">O rótulo da nova leitura.</param>
    /// <param name="cabecas">O efetivo da nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public bool Reapurar(string rebanhoNome, int? cabecas, long importadoPorId, DateTime agoraUtc)
    {
        MedicaoDoIbge.ConferirContagem(cabecas, nameof(cabecas));

        var mudou = RebanhoNome != rebanhoNome.Trim() || Cabecas != cabecas;

        RebanhoNome = rebanhoNome.Trim();
        Cabecas = cabecas;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;

        return mudou;
    }
}

/// <summary>
/// A ÁREA TERRITORIAL DE UM MUNICÍPIO, em km² — IBGE (tabela SIDRA 4714, Censo 2022).
///
/// <para><b>Por que existe, se o município já está no catálogo.</b> Porque a área tem <b>ano</b>: o
/// IBGE revisa limites municipais, e a área de hoje não é a de 2010. Guardá-la como coluna do
/// catálogo faria a revisão do IBGE reescrever o passado em silêncio.</para>
///
/// <para>Ela é o denominador das medidas por densidade — área plantada sobre área do município,
/// tratores por mil km² — e o que permite dizer "esta cidade é pequena e concentrada" em vez de só
/// "esta cidade tem pouca área plantada".</para>
/// </summary>
public sealed class AreaTerritorialDoMunicipio
{
    private AreaTerritorialDoMunicipio() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O município.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>O ano da apuração.</summary>
    public short Ano { get; private set; }

    /// <summary>A área, em quilômetros quadrados, com os três decimais que o IBGE publica.</summary>
    public decimal? AreaKm2 { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra a área de um município num ano.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="ano">O ano da apuração.</param>
    /// <param name="areaKm2">A área em km², ou nulo.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o ano ou a área não valem.</exception>
    public static AreaTerritorialDoMunicipio Registrar(
        int municipioId, short ano, decimal? areaKm2, long importadoPorId, DateTime agoraUtc)
    {
        MedicaoDoIbge.ConferirAno(ano);

        // ÁREA ZERO NÃO EXISTE. Ao contrário de uma contagem — onde zero é uma medida legítima —, um
        // município sem área seria um erro de leitura, e é melhor recusar do que dividir por ele.
        if (areaKm2 is <= 0)
            throw new RegraDeNegocioViolada("A área territorial de um município é maior que zero.");

        return new AreaTerritorialDoMunicipio
        {
            MunicipioId = municipioId,
            Ano = ano,
            AreaKm2 = areaKm2,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>Confere a linha contra uma nova leitura e diz se a área mudou.</summary>
    /// <param name="areaKm2">A área da nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando a área não vale.</exception>
    public bool Reapurar(decimal? areaKm2, long importadoPorId, DateTime agoraUtc)
    {
        if (areaKm2 is <= 0)
            throw new RegraDeNegocioViolada("A área territorial de um município é maior que zero.");

        var mudou = AreaKm2 != areaKm2;

        AreaKm2 = areaKm2;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;

        return mudou;
    }
}

/// <summary>
/// UMA MEDIDA QUE O IBGE PUBLICA PARA O ESTADO INTEIRO — a célula do SIDRA no nível 3 (issue 155).
///
/// <para><b>Por que existe: o total publicado não é a soma dos municípios.</b> Onde poucos
/// estabelecimentos respondem, o IBGE divulga o total do estado e oculta a parcela municipal (o
/// <c>X</c> do sigilo). Somar os 645 municípios devolve um número menor que o oficial, e era isso que
/// a tela fazia com tratores e propriedades — enquanto a lavoura já comparava com a linha publicada
/// (<see cref="ProducaoAgricolaNoEstado"/>). Dois métodos na mesma linha de indicadores.</para>
///
/// <para><b>Por que UMA tabela para as quatro pesquisas.</b> A chave natural do SIDRA é a mesma em
/// todas — localidade, tabela, variável, período e categoria —, e é ela que está guardada aqui. Uma
/// tabela por pesquisa repetiria quatro vezes a mesma estrutura para guardar, no total, algumas
/// dezenas de linhas: o estado é UMA localidade, não 645.</para>
///
/// <para><b>Não substitui as tabelas municipais.</b> Elas continuam sendo a base do mapa e do
/// recorte por região; esta é só o denominador do "% de São Paulo".</para>
/// </summary>
public sealed class MedidaDoIbgeNoEstado
{
    private MedidaDoIbgeNoEstado() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O código da UF no IBGE. São Paulo é 35.</summary>
    public int EstadoCodigoIbge { get; private set; }

    /// <summary>A tabela do SIDRA: 6871 tratores, 6780 estabelecimentos, 3939 rebanho, 4714 área.</summary>
    public short TabelaDoSidra { get; private set; }

    /// <summary>A variável dentro da tabela — a 6871 tem duas, e cada uma é uma linha daqui.</summary>
    public short VariavelDoSidra { get; private set; }

    /// <summary>O ano da pesquisa, que difere entre elas: o Censo é 2017 e a PPM é anual.</summary>
    public short Ano { get; private set; }

    /// <summary>A categoria da classificação pedida, ou nulo na tabela que não tem nenhuma (4714).</summary>
    public int? CategoriaCodigoIbge { get; private set; }

    /// <summary>O rótulo oficial da categoria; nulo junto com o código.</summary>
    public string? CategoriaNome { get; private set; }

    /// <summary>O valor publicado. Nulo é sigilo ou não divulgado — nunca zero.</summary>
    public decimal? Valor { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra a medida publicada de uma UF, tabela, variável, ano e categoria.</summary>
    /// <param name="estadoCodigoIbge">O código da UF.</param>
    /// <param name="tabelaDoSidra">A tabela do SIDRA.</param>
    /// <param name="variavelDoSidra">A variável dentro da tabela.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    /// <param name="categoriaCodigoIbge">A categoria, ou nulo quando a tabela não tem classificação.</param>
    /// <param name="categoriaNome">O rótulo da categoria, ou nulo junto com o código.</param>
    /// <param name="valor">O valor publicado, ou nulo.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando a UF, o ano, a chave do SIDRA ou o valor não valem.</exception>
    public static MedidaDoIbgeNoEstado Registrar(
        int estadoCodigoIbge, short tabelaDoSidra, short variavelDoSidra, short ano,
        int? categoriaCodigoIbge, string? categoriaNome, decimal? valor, long importadoPorId, DateTime agoraUtc)
    {
        if (estadoCodigoIbge is < 11 or > 53)
            throw new RegraDeNegocioViolada($"O código de UF do IBGE vai de 11 a 53; {estadoCodigoIbge} não é um.");

        if (tabelaDoSidra <= 0 || variavelDoSidra <= 0)
            throw new RegraDeNegocioViolada("A medida do IBGE precisa da tabela e da variável do SIDRA de onde veio.");

        MedicaoDoIbge.ConferirAno(ano);
        ConferirCategoria(categoriaCodigoIbge, categoriaNome);
        ConferirValor(valor);

        return new MedidaDoIbgeNoEstado
        {
            EstadoCodigoIbge = estadoCodigoIbge,
            TabelaDoSidra = tabelaDoSidra,
            VariavelDoSidra = variavelDoSidra,
            Ano = ano,
            CategoriaCodigoIbge = categoriaCodigoIbge,
            CategoriaNome = string.IsNullOrWhiteSpace(categoriaNome) ? null : categoriaNome.Trim(),
            Valor = valor,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>Confere a linha contra uma nova leitura e diz se alguma coisa mudou.</summary>
    /// <param name="categoriaNome">O rótulo da nova leitura.</param>
    /// <param name="valor">O valor da nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o valor não vale.</exception>
    public bool Reapurar(string? categoriaNome, decimal? valor, long importadoPorId, DateTime agoraUtc)
    {
        ConferirValor(valor);

        var nome = string.IsNullOrWhiteSpace(categoriaNome) ? null : categoriaNome.Trim();
        var mudou = CategoriaNome != nome || Valor != valor;

        CategoriaNome = nome;
        Valor = valor;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;

        return mudou;
    }

    /// <summary>Código e rótulo andam juntos: um sem o outro é leitura pela metade.</summary>
    private static void ConferirCategoria(int? codigo, string? nome)
    {
        if (codigo is null && string.IsNullOrWhiteSpace(nome)) return;

        if (codigo is null || string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioViolada(
                "A categoria do IBGE vem com código e rótulo, ou sem os dois quando a tabela não tem classificação.");

        MedicaoDoIbge.ConferirCategoria(codigo.Value, nome);
    }

    /// <summary>
    /// Zero vale — um estado pode não ter um rebanho —, negativo não existe em contagem, cabeça nem
    /// área. Nulo é o sigilo, e continua sendo permitido.
    /// </summary>
    private static void ConferirValor(decimal? valor)
    {
        if (valor < 0)
            throw new RegraDeNegocioViolada("A medida publicada pelo IBGE não é negativa.");
    }
}

/// <summary>
/// AS CONFERÊNCIAS QUE AS QUATRO MEDIÇÕES DO IBGE COMPARTILHAM.
///
/// <para>Elas moram juntas porque são a mesma regra dita uma vez: um ano do IBGE, um código de
/// categoria com rótulo, e uma contagem que não é negativa. Repeti-las em quatro entidades faria
/// quatro lugares para consertar quando uma delas mudar.</para>
/// </summary>
internal static class MedicaoDoIbge
{
    /// <summary>O primeiro ano de qualquer das pesquisas usadas, e um teto que não chega.</summary>
    public static void ConferirAno(short ano)
    {
        if (ano is < 1920 or > 2100)
            throw new RegraDeNegocioViolada($"O ano {ano} está fora do intervalo das pesquisas do IBGE.");
    }

    /// <summary>A categoria da classificação: código positivo e rótulo com texto.</summary>
    public static void ConferirCategoria(int codigo, string nome)
    {
        if (codigo <= 0)
            throw new RegraDeNegocioViolada("O código de categoria do IBGE é positivo.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioViolada("A categoria do IBGE precisa do rótulo oficial dela.");
    }

    /// <summary>
    /// Uma contagem do IBGE: zero vale, negativo não existe.
    ///
    /// <para>Nulo também vale, e significa outra coisa: sigilo (<c>X</c>) ou não disponível — nunca
    /// zero. Somar um sigiloso como zero diria que o município não tem trator quando só não foi
    /// divulgado.</para>
    /// </summary>
    public static void ConferirContagem(int? valor, string oQue)
    {
        if (valor < 0)
            throw new RegraDeNegocioViolada($"A contagem \"{oQue}\" do IBGE não é negativa.");
    }
}
