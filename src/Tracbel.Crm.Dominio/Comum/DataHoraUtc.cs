namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Instante no tempo, sempre em UTC e dentro de uma faixa plausível.
///
/// [V] LIÇÃO DO VÓRTICE — sem CHECK de faixa em nenhuma coluna de data, o Vórtice tem
/// vencimento gravado em <b>08/05/5024</b> e, mesmo entre os títulos em aberto, em
/// <b>10/08/2223</b> — a maior data já encontrada no banco é <b>20/08/5173</b> (documento
/// 01, achado 3.8; documento 10). A causa é conhecida: o Protheus usa a sentinela
/// <c>1900-01-01</c> para "sem data", tratada em só 2 das 5 colunas de data de título; o
/// que escapa dessa coalescência incompleta vira lixo de faixa (pesquisa 02, achado 9). A
/// própria pesquisa já recomenda a correção: validar faixa no adaptador, uma vez, para
/// toda coluna de data — não repetir o mesmo <c>CASE</c> em cada rotina de carga.
/// </summary>
public readonly record struct DataHoraUtc : IComparable<DataHoraUtc>
{
    // 1900 cobre nascimento e fundação de empresa antiga; ano atual + 30 cobre previsão de
    // garantia e vencimento de contrato longo (ex.: PowerGard, financiamento BNDES de até
    // 84 meses). Campos com regra mais estreita (ex.: vencimento de título) aplicam faixa
    // adicional no catálogo do documento 16 — este tipo é o piso comum a todo o sistema.
    private const int AnoMinimo = 1900;
    private const int AnosNoFuturo = 30;

    private DataHoraUtc(DateTime valor) => Valor = valor;

    /// <summary>O instante, sempre com <see cref="DateTime.Kind"/> igual a <see cref="DateTimeKind.Utc"/>.</summary>
    public DateTime Valor { get; }

    /// <summary>
    /// Cria a partir de um <see cref="DateTime"/>. Lança se o ano estiver fora da faixa
    /// plausível.
    ///
    /// Um <see cref="DateTimeKind.Unspecified"/> é tratado como já estando em UTC — quem
    /// tem um horário local deve converter ANTES de chamar aqui; este tipo não adivinha
    /// fuso horário.
    /// </summary>
    /// <exception cref="RegraDeNegocioViolada">Ano fora da faixa plausível.</exception>
    public static DataHoraUtc Criar(DateTime valor) =>
        TentarCriar(valor, out var dataHora)
            ? dataHora
            : throw new RegraDeNegocioViolada(
                $"Data fora da faixa plausível: '{valor:O}'. " +
                $"Esperado entre {AnoMinimo} e {DateTime.UtcNow.Year + AnosNoFuturo}.");

    /// <summary>
    /// Versão que não lança. Use em importação em massa — inclusive na migração do Vórtice,
    /// onde sentinela de "sem data" já deve ter sido convertida em <c>null</c> ANTES de
    /// chegar aqui (este tipo valida faixa, não reconhece sentinela de origem).
    /// </summary>
    public static bool TentarCriar(DateTime valor, out DataHoraUtc dataHora)
    {
        dataHora = default;

        if (valor.Year < AnoMinimo || valor.Year > DateTime.UtcNow.Year + AnosNoFuturo) return false;

        var utc = valor.Kind switch
        {
            DateTimeKind.Utc => valor,
            DateTimeKind.Local => valor.ToUniversalTime(),
            _ => DateTime.SpecifyKind(valor, DateTimeKind.Utc)
        };

        dataHora = new DataHoraUtc(utc);
        return true;
    }

    /// <summary>
    /// Ordena dois instantes.
    ///
    /// <para>Existe porque toda leitura de linha do tempo é uma FAIXA — "as interações deste
    /// cliente entre março e maio" — e uma faixa precisa de comparação. Sem os operadores, a
    /// consulta teria de comparar o <see cref="Valor"/> por dentro do tipo, e acesso a membro de
    /// um valor convertido não traduz para SQL: é o defeito nº 2 do documento 23, encontrado
    /// exercitando a API.</para>
    /// </summary>
    /// <param name="outro">O instante a comparar.</param>
    public int CompareTo(DataHoraUtc outro) => Valor.CompareTo(outro.Valor);

    /// <summary>Este instante é anterior ao outro?</summary>
    /// <param name="esquerda">O primeiro instante.</param>
    /// <param name="direita">O segundo instante.</param>
    public static bool operator <(DataHoraUtc esquerda, DataHoraUtc direita) => esquerda.Valor < direita.Valor;

    /// <summary>Este instante é posterior ao outro?</summary>
    /// <param name="esquerda">O primeiro instante.</param>
    /// <param name="direita">O segundo instante.</param>
    public static bool operator >(DataHoraUtc esquerda, DataHoraUtc direita) => esquerda.Valor > direita.Valor;

    /// <summary>Este instante é anterior ou igual ao outro?</summary>
    /// <param name="esquerda">O primeiro instante.</param>
    /// <param name="direita">O segundo instante.</param>
    public static bool operator <=(DataHoraUtc esquerda, DataHoraUtc direita) => esquerda.Valor <= direita.Valor;

    /// <summary>Este instante é posterior ou igual ao outro?</summary>
    /// <param name="esquerda">O primeiro instante.</param>
    /// <param name="direita">O segundo instante.</param>
    public static bool operator >=(DataHoraUtc esquerda, DataHoraUtc direita) => esquerda.Valor >= direita.Valor;

    /// <inheritdoc />
    public override string ToString() => Valor.ToString("O");
}
