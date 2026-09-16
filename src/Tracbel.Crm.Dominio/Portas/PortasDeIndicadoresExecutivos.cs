namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O faturamento de UMA competência, com a nota sem cliente no CRM separada por natureza da
/// contraparte (documento 36, cartão A).
///
/// <para><b>Fonte única: as notas de saída do Protheus</b> (SD2), já agregadas pela carga em
/// <c>comercial.FaturamentoDoCliente</c> e <c>comercial.FaturamentoSemCliente</c>. O ART não entra
/// aqui: ele registra a venda de máquina (pedido), não a nota — somar os dois contaria a mesma
/// máquina duas vezes.</para>
///
/// <para><b>Sem dupla contagem por construção:</b> cada linha de faturamento cai em exatamente uma
/// parcela (com cliente, ou uma das quatro naturezas), e a quebra por grupo de item (máquina, peça,
/// serviço, outros) soma o mesmo total por outro corte.</para>
/// </summary>
/// <param name="Competencia">O mês, sempre no dia 1.</param>
/// <param name="ComCliente">Notas para cliente cadastrado no CRM.</param>
/// <param name="ContraparteSemCadastro">Cliente não cadastrado, nota sem documento ou contraparte não classificada.</param>
/// <param name="RepasseDeFabrica">Notas para a fábrica — CFOP de venda, mas não é cliente final.</param>
/// <param name="EmpresaDoGrupo">Notas para outra empresa do grupo.</param>
/// <param name="OutraRevenda">Notas para outra concessionária.</param>
/// <param name="Maquina">Grupo do item máquina, com e sem cliente.</param>
/// <param name="Peca">Grupo do item peça, com e sem cliente.</param>
/// <param name="Servico">Serviço e mão de obra, com e sem cliente.</param>
/// <param name="Outros">Grupos não classificados, com e sem cliente.</param>
/// <param name="Notas">Notas fiscais distintas (uma nota tem uma contraparte só).</param>
/// <param name="CarregadoEm">Quando a carga gravou a linha mais recente desta competência (UTC).</param>
public sealed record FaturamentoDaCompetencia(
    DateOnly Competencia,
    decimal ComCliente,
    decimal ContraparteSemCadastro,
    decimal RepasseDeFabrica,
    decimal EmpresaDoGrupo,
    decimal OutraRevenda,
    decimal Maquina,
    decimal Peca,
    decimal Servico,
    decimal Outros,
    int Notas,
    DateTime? CarregadoEm)
{
    /// <summary>As quatro naturezas da nota sem cliente no CRM.</summary>
    public decimal SemCliente => ContraparteSemCadastro + RepasseDeFabrica + EmpresaDoGrupo + OutraRevenda;

    /// <summary>Tudo o que foi emitido na competência.</summary>
    public decimal Total => ComCliente + SemCliente;
}

/// <summary>
/// O faturamento do ANO CIVIL pedido, ao lado da meta de faturamento cadastrada — e nunca de uma
/// previsão (documento 36, cartão B).
///
/// <para><b>Ano civil, e não fiscal:</b> o calendário fiscal não foi confirmado (documento 32, P-4).
/// <b>Meta só soma no nível da filial</b> (sem carteira, usuário nem linha de negócio) e com o
/// período inteiro dentro do ano: somar a meta da filial com a das carteiras dela contaria o mesmo
/// alvo duas vezes, e ratear uma meta que cruza o ano seria inventar a parte de cada mês.</para>
/// </summary>
/// <param name="Ano">O ano pedido.</param>
/// <param name="PrimeiraCompetencia">O primeiro mês com faturamento no ano.</param>
/// <param name="UltimaCompetencia">O último mês com faturamento no ano.</param>
/// <param name="MesesComFaturamento">Quantos meses do ano têm linha de faturamento.</param>
/// <param name="ComCliente">Realizado com cliente no CRM.</param>
/// <param name="SemCliente">Realizado sem cliente no CRM, todas as naturezas.</param>
/// <param name="MetasDaFilial">Metas de faturamento da filial com o período dentro do ano.</param>
/// <param name="AlvoDaFilial">A soma dessas metas; nulo quando não há nenhuma — sem meta, e não meta zero.</param>
/// <param name="MetasDetalhadas">Metas de carteira, usuário ou linha no ano — contadas, não somadas.</param>
/// <param name="MetasQueCruzamOAno">Metas que começam ou terminam fora do ano — contadas, não rateadas.</param>
public sealed record FaturamentoDoAno(
    int Ano,
    DateOnly? PrimeiraCompetencia,
    DateOnly? UltimaCompetencia,
    int MesesComFaturamento,
    decimal ComCliente,
    decimal SemCliente,
    int MetasDaFilial,
    decimal? AlvoDaFilial,
    int MetasDetalhadas,
    int MetasQueCruzamOAno)
{
    /// <summary>Tudo o que foi emitido no ano, até a última competência carregada.</summary>
    public decimal Total => ComCliente + SemCliente;
}

/// <summary>
/// Os clientes em carteira, contados de dois jeitos que respondem a perguntas diferentes (documento
/// 36, cartão C).
///
/// <para><b>Clientes cadastrados com vínculo</b> é a PARTIÇÃO pela filial de cadastro: cada cliente
/// tem uma filial de cadastro só, então a soma das filiais é o total de clientes únicos, sem contar
/// ninguém duas vezes. <b>Clientes nas carteiras da filial</b> é o que a filial enxerga na carteira
/// dela — inclusive cliente cadastrado em outra filial — e NÃO se soma entre filiais.</para>
/// </summary>
/// <param name="ClientesCadastradosComVinculo">Clientes cadastrados nesta filial com vínculo ativo em qualquer carteira.</param>
/// <param name="Clientes">Deles, na situação cadastral Cliente.</param>
/// <param name="Prospects">Deles, Prospect.</param>
/// <param name="Suspects">Deles, Suspect.</param>
/// <param name="OutrasSituacoes">Deles, cliente inativo ou relacionamento encerrado.</param>
/// <param name="SemDocumento">Deles, sem CPF/CNPJ — identidade que não se confere com o Protheus.</param>
/// <param name="ClientesNasCarteirasDaFilial">Clientes distintos nas carteiras comerciais desta filial.</param>
/// <param name="Vinculos">Vínculos ativos nas carteiras desta filial, de qualquer natureza.</param>
/// <param name="VinculosComerciais">Deles, em carteira comercial.</param>
/// <param name="Carteiras">Carteiras desta filial.</param>
/// <param name="CarteirasComerciais">Deles, comerciais.</param>
public sealed record CarteiraDaFilial(
    int ClientesCadastradosComVinculo,
    int Clientes,
    int Prospects,
    int Suspects,
    int OutrasSituacoes,
    int SemDocumento,
    int ClientesNasCarteirasDaFilial,
    int Vinculos,
    int VinculosComerciais,
    int Carteiras,
    int CarteirasComerciais);

/// <summary>
/// A cobertura por VÍNCULO em carteira comercial, contra a cadência declarada da linha de negócio —
/// a mesma regra do mapa de cobertura e do painel do CEN (documento 27, seção 4; documento 36,
/// cartão D). Nenhum prazo fixo de 30 dias.
/// </summary>
/// <param name="VinculosComerciais">Vínculos ativos em carteira comercial desta filial.</param>
/// <param name="Elegiveis">Os que estão em linha com cadência declarada — o denominador.</param>
/// <param name="Cobertos">Último contato dentro da cadência da classe — o numerador.</param>
/// <param name="ForaDaCadencia">Com contato, mais antigo que o prazo.</param>
/// <param name="NuncaContatados">Elegíveis sem contato nenhum.</param>
/// <param name="SemCadencia">Em linha sem cadência declarada — fora da conta.</param>
/// <param name="ContatoMaisRecente">O último contato registrado nos vínculos da filial (UTC) — a idade do dado.</param>
/// <param name="TiposDeAtividade">Tipos de atividade cadastrados.</param>
/// <param name="TiposMarcadosComoVisita">Deles, marcados para contar como visita (<c>ContaParaCobertura</c>).</param>
public sealed record CoberturaDaFilial(
    int VinculosComerciais,
    int Elegiveis,
    int Cobertos,
    int ForaDaCadencia,
    int NuncaContatados,
    int SemCadencia,
    DateTime? ContatoMaisRecente,
    int TiposDeAtividade,
    int TiposMarcadosComoVisita)
{
    /// <summary>Fora da cadência mais nunca contatados.</summary>
    public int Pendentes => ForaDaCadencia + NuncaContatados;
}

/// <summary>
/// O que o CRM sabe do mercado: as vendas perdidas registradas no formulário (documento 36, cartão
/// E). Não há percentual aqui — participação de mercado exige emplacamento, que não está carregado.
/// </summary>
/// <param name="VendasPerdidasRegistradas">Formulários de venda perdida.</param>
/// <param name="ComConcorrente">Deles, com o fabricante concorrente declarado.</param>
/// <param name="ComModeloDoConcorrente">Deles, com o modelo do concorrente.</param>
/// <param name="ComOsDoisPrecos">Deles, com o preço do concorrente e o nosso.</param>
/// <param name="Unidades">Máquinas somadas nos formulários.</param>
/// <param name="PrimeiraEm">A perda mais antiga (data da perda, ou do registro quando ela falta).</param>
/// <param name="UltimaEm">A perda mais recente.</param>
public sealed record MercadoDaFilial(
    int VendasPerdidasRegistradas,
    int ComConcorrente,
    int ComModeloDoConcorrente,
    int ComOsDoisPrecos,
    int Unidades,
    DateOnly? PrimeiraEm,
    DateOnly? UltimaEm);

/// <summary>Os cinco indicadores executivos de uma filial.</summary>
/// <param name="ReferenciaUtc">O instante da apuração — contra ele a cadência e o mês em curso são medidos.</param>
/// <param name="FaturamentoDoMes">A competência mais recente carregada; nulo quando não há faturamento.</param>
/// <param name="Ano">O ano civil pedido.</param>
/// <param name="Carteira">Os clientes em carteira.</param>
/// <param name="Cobertura">A cobertura pela cadência.</param>
/// <param name="Mercado">As vendas perdidas registradas.</param>
public sealed record IndicadoresExecutivosDaFilial(
    DateTime ReferenciaUtc,
    FaturamentoDaCompetencia? FaturamentoDoMes,
    FaturamentoDoAno Ano,
    CarteiraDaFilial Carteira,
    CoberturaDaFilial Cobertura,
    MercadoDaFilial Mercado);

/// <summary>
/// O acesso aos INDICADORES EXECUTIVOS — os cinco cartões da Visão 360.
///
/// <para><b>Uma filial por leitura, e todo número somável entre filiais</b> — exceto
/// <see cref="CarteiraDaFilial.ClientesNasCarteirasDaFilial"/>, que diz na própria documentação que
/// não se soma. Faturamento é da filial que emitiu, vínculo é da filial da carteira, cliente único é
/// da filial de cadastro, meta e venda perdida são da filial dona. É o que deixa o consolidado da
/// tela somar as treze filiais sem contar nada duas vezes.</para>
/// </summary>
public interface IRepositorioIndicadoresExecutivos
{
    /// <summary>Apura os indicadores da filial do contexto de acesso.</summary>
    /// <param name="ano">O ano civil do cartão de meta × realizado.</param>
    /// <param name="agoraUtc">O instante contra o qual a cadência e o mês em curso são medidos.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IndicadoresExecutivosDaFilial> ApurarAsync(int ano, DateTime agoraUtc, CancellationToken ct);
}
