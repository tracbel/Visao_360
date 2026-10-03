using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Portas;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CodigosDoIbge;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.CriterioDasVendasDoArt;
using static Tracbel.Crm.Infraestrutura.Persistencia.Repositorios.GruposDoRecorte;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>O recorte somado: o período pedido, o mesmo trecho do ano anterior e o mês a mês de cada município.</summary>
/// <param name="Acumuladores">Cada grupo no período pedido — é ele que abre linha no mapa ou fora dele.</param>
/// <param name="Anteriores">Cada grupo no mesmo trecho do ano anterior, à parte.</param>
/// <param name="Mensal">As vendas de cada município em cada mês das duas janelas.</param>
internal sealed record RecorteAcumulado(
    IReadOnlyDictionary<int, Acumulador> Acumuladores,
    IReadOnlyDictionary<int, Acumulador> Anteriores,
    IReadOnlyDictionary<(int Grupo, DateOnly Mes), VendasDoMes> Mensal);

/// <summary>
/// A ACUMULAÇÃO DO RECORTE (plano 2 do documento 54): cada cliente, vínculo, nota e venda do ART cai em exatamente um
/// grupo — um município de São Paulo ou um dos grupos fora do mapa —, e é somado ali. Conta em memória: nenhuma leitura
/// de banco passa por aqui.
/// </summary>
internal static class AcumulacaoDoRecorte
{
    /// <summary>Soma o recorte lido pelos leitores.</summary>
    /// <param name="consulta">O período e os filtros da tela.</param>
    /// <param name="agoraUtc">O instante da leitura — a régua da cadência.</param>
    /// <param name="municipios">O catálogo de municípios, pelo identificador interno.</param>
    /// <param name="clientes">Os clientes ao alcance, os inativados e o filtro do CEN.</param>
    /// <param name="cobertura">As carteiras comerciais, os vínculos e os responsáveis.</param>
    /// <param name="faturamento">O faturamento com cliente nas duas janelas e as notas sem cliente.</param>
    /// <param name="art">As vendas do ART; nulas quando o ART não trouxe nada ao alcance.</param>
    internal static RecorteAcumulado Acumular(
        ConsultaDeIndicadoresTerritoriais consulta,
        DateTime agoraUtc,
        IReadOnlyDictionary<int, MunicipioDoCatalogo> municipios,
        ClientesDoRecorte clientes,
        CoberturaDoRecorte cobertura,
        FaturamentoDoRecorte faturamento,
        VendasDoArtNoRecorte? art)
    {
        var inativados = clientes.Inativados;
        var clientesDoResponsavel = clientes.DoResponsavel;
        var cadencias = cobertura.Carteiras.ToDictionary(c => c.Id, c => c.Cadencia);
        var responsavelDaCarteira = cobertura.Carteiras.ToDictionary(c => c.Id, c => c.ResponsavelId);

        var grupoDoCliente = new Dictionary<long, int>();
        var classeDoCliente = new Dictionary<long, ClasseDeCliente?>();
        var acumuladores = new Dictionary<int, Acumulador>();

        Acumulador Do(int grupo) =>
            acumuladores.TryGetValue(grupo, out var existente) ? existente : acumuladores[grupo] = new Acumulador();

        // O MESMO TRECHO DO ANO ANTERIOR (decisão de 27/09/2026) acumula À PARTE, num dicionário próprio: ele
        // não pode criar grupo no mapa nem fora dele — um grupo que só vendeu no ano passado apareceria hoje
        // com R$ 0, e a conferência "mapa + fora do mapa = total" ganharia uma linha que não é deste período.
        var janela = consulta.Janela;
        var janelaAnterior = consulta.JanelaAnterior;
        var anteriores = new Dictionary<int, Acumulador>();

        Acumulador DoAnterior(int grupo) =>
            anteriores.TryGetValue(grupo, out var existente) ? existente : anteriores[grupo] = new Acumulador();

        // O MÊS A MÊS DE CADA MUNICÍPIO, nas duas janelas — é dele que sai o mini-gráfico dos cartões.
        var mensal = new Dictionary<(int Grupo, DateOnly Mes), VendasDoMes>();

        VendasDoMes NoMes(int grupo, DateOnly mes) =>
            mensal.TryGetValue((grupo, mes), out var existente) ? existente : mensal[(grupo, mes)] = new VendasDoMes();

        // -----------------------------------------------------------------------------------------
        // O grupo de cada cliente: um município de SP, ou um dos grupos fora do mapa.
        // -----------------------------------------------------------------------------------------
        foreach (var cliente in clientes.Clientes)
        {
            // MAIS DE UM ENDEREÇO PRINCIPAL: vale o de menor Id, e a ordenação da consulta é o que
            // torna a escolha a mesma em toda execução.
            if (grupoDoCliente.ContainsKey(cliente.Id)) continue;

            classeDoCliente[cliente.Id] = cliente.Classe;

            if ((consulta.FilialDoClienteId is { } filialDoCliente && cliente.EmpresaId != filialDoCliente)
                || (clientesDoResponsavel is not null && !clientesDoResponsavel.Contains(cliente.Id)))
            {
                grupoDoCliente[cliente.Id] = ForaDoFiltro;
                continue;
            }

            var grupo = cliente.MunicipioId is not { } municipioId || !municipios.TryGetValue(municipioId, out var municipio)
                ? SemMunicipio
                : municipio.CodigoIbge is null
                    ? MunicipioSemCodigoIbge
                    : municipio.Uf != SaoPaulo ? OutraUf : municipio.CodigoIbge.Value;

            grupoDoCliente[cliente.Id] = grupo;
            var doGrupo = Do(grupo);
            doGrupo.Clientes++;
            doGrupo.ContarClasse(cliente.Classe);
        }

        // CADA LINHA CAI EM EXATAMENTE UM GRUPO, ou em nenhum quando o filtro a exclui. É o que
        // impede dupla contagem: não existe caminho que some a mesma venda em dois lugares.
        int GrupoDe(long clienteId)
        {
            if (grupoDoCliente.TryGetValue(clienteId, out var grupo)) return grupo;

            if (clientesDoResponsavel is not null && !clientesDoResponsavel.Contains(clienteId)) return ForaDoFiltro;

            if (inativados.TryGetValue(clienteId, out var filialDoInativado))
                return consulta.FilialDoClienteId is { } filtro && filialDoInativado != filtro ? ForaDoFiltro : ClienteInativado;

            // Cliente que não está ao alcance só existe na visão da filial: é cadastro de outra.
            // Com filtro de filial do cliente, o caso de uso já garantiu que essa filial está ao
            // alcance — logo, cliente fora do alcance está fora do filtro.
            return consulta.FilialDoClienteId is null ? ClienteDeOutraFilial : ForaDoFiltro;
        }

        // -----------------------------------------------------------------------------------------
        // Cobertura: vínculo em carteira comercial, contra a cadência da linha de negócio.
        // -----------------------------------------------------------------------------------------

        foreach (var vinculo in cobertura.Vinculos)
        {
            var grupoDoVinculo = GrupoDe(vinculo.ClienteId);
            if (grupoDoVinculo == ForaDoFiltro) continue;

            // COM O FILTRO DO CEN, a cobertura é a das carteiras DELE: o mesmo cliente numa carteira de outro
            // responsável tem outro dono e outra cadência a cumprir.
            if (consulta.ResponsavelId is { } doCen && responsavelDaCarteira[vinculo.CarteiraId] != doCen) continue;

            var acumulador = Do(grupoDoVinculo);
            var cadencia = cadencias[vinculo.CarteiraId];

            // SEM CLASSE APURADA, O VÍNCULO CONTA COMO D — a mesma regra do painel do CEN.
            short? dias = (classeDoCliente.GetValueOrDefault(vinculo.ClienteId) ?? ClasseDeCliente.D) switch
            {
                ClasseDeCliente.A => cadencia?.DiasCicloClasseA,
                ClasseDeCliente.B => cadencia?.DiasCicloClasseB,
                ClasseDeCliente.C => cadencia?.DiasCicloClasseC,
                _ => cadencia?.DiasCicloClasseD
            };

            acumulador.Vinculos++;
            acumulador.ClientesComVinculo.Add(vinculo.ClienteId);

            var responsavel = responsavelDaCarteira[vinculo.CarteiraId];
            acumulador.VinculosPorResponsavel[responsavel] = acumulador.VinculosPorResponsavel.GetValueOrDefault(responsavel) + 1;
            if (!acumulador.CarteirasPorResponsavel.TryGetValue(responsavel, out var carteirasDoResponsavel))
                acumulador.CarteirasPorResponsavel[responsavel] = carteirasDoResponsavel = [];
            carteirasDoResponsavel.Add(vinculo.CarteiraId);

            if (dias is null) acumulador.SemCadencia++;
            else if (vinculo.UltimaInteracaoEm is null) acumulador.NuncaContatados++;
            else if (vinculo.UltimaInteracaoEm >= agoraUtc.AddDays(-dias.Value)) acumulador.Cobertos++;
            else acumulador.ForaDaCadencia++;
        }

        // -----------------------------------------------------------------------------------------
        // Vendas: as linhas mensais do período, somadas aqui.
        //
        // A SOMA É EM MEMÓRIA, e não um GroupBy no banco, por uma razão de portabilidade medida: o
        // SQLite dos testes de API não agrega decimal, e a rota ficaria sem teste de ponta a ponta.
        // O volume é o de linhas cliente × mês de um período de até 36 meses ao alcance da filial —
        // dezenas de milhares, não milhões.
        // -----------------------------------------------------------------------------------------

        foreach (var cliente in faturamento.ComCliente.GroupBy(f => f.ClienteId))
        {
            var grupoDaVenda = GrupoDe(cliente.Key);
            if (grupoDaVenda == ForaDoFiltro) continue;

            // O GRUPO SÓ NASCE COM VENDA DO PERÍODO: um cliente que só comprou no ano anterior não pode abrir,
            // hoje, uma linha de R$ 0 no mapa ou fora dele.
            var doPeriodo = cliente.Where(f => janela.Contem(f.Competencia)).ToList();
            if (doPeriodo.Count > 0)
            {
                var acumulador = Do(grupoDaVenda);
                var liquido = doPeriodo.Sum(f => f.ValorLiquido);

                acumulador.ValorLiquido += liquido;
                acumulador.Maquina += doPeriodo.Sum(f => f.ValorEmMaquina);
                acumulador.Peca += doPeriodo.Sum(f => f.ValorEmPeca);
                acumulador.Servico += doPeriodo.Sum(f => f.ValorEmServico);
                acumulador.Outros += doPeriodo.Sum(f => f.ValorEmOutros);
                if (liquido != 0) acumulador.ClientesQueCompraram++;
            }

            var doAnterior = cliente.Where(f => janelaAnterior.Contem(f.Competencia)).ToList();
            if (doAnterior.Count > 0)
            {
                var acumulador = DoAnterior(grupoDaVenda);
                var liquido = doAnterior.Sum(f => f.ValorLiquido);

                acumulador.ValorLiquido += liquido;
                acumulador.Maquina += doAnterior.Sum(f => f.ValorEmMaquina);
                acumulador.Peca += doAnterior.Sum(f => f.ValorEmPeca);
                acumulador.Servico += doAnterior.Sum(f => f.ValorEmServico);
                acumulador.Outros += doAnterior.Sum(f => f.ValorEmOutros);
                if (liquido != 0) acumulador.ClientesQueCompraram++;
            }

            // O MÊS A MÊS SÓ DOS MUNICÍPIOS — é o que os cartões da ADR desenham.
            if (grupoDaVenda > 0)
                foreach (var linha in cliente)
                {
                    var mes = NoMes(grupoDaVenda, linha.Competencia);
                    mes.ValorLiquido += linha.ValorLiquido;
                    mes.Maquina += linha.ValorEmMaquina;
                    mes.PosVenda += linha.ValorEmPeca + linha.ValorEmServico;
                }
        }

        // A NOTA SEM CLIENTE NO CRM TAMBÉM É NOTA DESTA FILIAL — o leitor a traz vazia quando o filtro da filial do
        // cliente ou o do CEN a excluem (sem cadastro, ela não tem filial de cadastro nem carteira).
        foreach (var contraparte in faturamento.SemCliente.GroupBy(f => (Grupo: GrupoDaNatureza(f.Natureza), f.Documento)))
        {
            var acumulador = Do(contraparte.Key.Grupo);
            var liquido = contraparte.Sum(f => f.ValorLiquido);

            acumulador.ValorLiquido += liquido;
            acumulador.Maquina += contraparte.Sum(f => f.ValorEmMaquina);
            acumulador.Peca += contraparte.Sum(f => f.ValorEmPeca);
            acumulador.Servico += contraparte.Sum(f => f.ValorEmServico);
            acumulador.Outros += contraparte.Sum(f => f.ValorEmOutros);
            if (liquido != 0) acumulador.ClientesQueCompraram++;
        }

        // -----------------------------------------------------------------------------------------
        // AS VENDAS DE MÁQUINA EM UNIDADES — o ART (issue 69; D-P08 decidida em 24/09/2026).
        //
        // ELAS NÃO SÃO O FATURAMENTO ACIMA. Aquele é o Protheus, em REAIS; este é o ART, em MÁQUINAS.
        // A captura é uma razão de unidades sobre demanda estimada em unidades, e reais no numerador a
        // tornariam incomparável — os dois convivem, e nada aqui os soma.
        //
        // UMA VENDA É UMA MÁQUINA: cada linha do ART tem um chassi e vira uma venda. Somar a coluna de
        // quantidade da origem contaria duas vezes a máquina que ela lançasse em lote.
        //
        // AUSÊNCIA DE CARGA NÃO É AUSÊNCIA DE VENDA: com a tabela vazia ao alcance desta consulta, o
        // bloco inteiro sai NULO, e a captura sai vazia com o motivo. Zero é medida — a filial existe,
        // o ART trouxe dado e ela não vendeu máquina no período.
        // -----------------------------------------------------------------------------------------

        if (art is not null)
        {
            var codigoDaLinha = art.CodigoDaLinha;
            var categoriaDaLinha = art.CategoriaDaLinha;

            foreach (var venda in art.Vendas)
            {
                var data = DataPeloCriterio(Criterio, venda.VendidaEm, venda.FaturadaEm, venda.EntregueEm);
                if (data is not { } dia) continue;

                var doPeriodo = janela.Contem(dia);
                if (!doPeriodo && !janelaAnterior.Contem(dia)) continue;

                var grupoDaMaquina = GrupoDe(venda.CompradorId);
                if (grupoDaMaquina == ForaDoFiltro) continue;

                // A CATEGORIA DA MÁQUINA, pelo de-para da linha de produto — ou por que ela não tem.
                string? codigoDaCategoria = null;
                var linhaDaMaquina = venda.LinhaDeProdutoId is { } linhaId && codigoDaLinha.TryGetValue(linhaId, out var achada)
                    ? achada
                    : null;
                var semClassificacao = linhaDaMaquina is null;
                var emLinhaSemCategoria = false;
                if (linhaDaMaquina is not null)
                {
                    // LINHA SEM CATEGORIA CONTA NO TOTAL E SOME DA QUEBRA. Hoje é a plataforma de corte, que
                    // ficou sem categoria de propósito: é acessório de colheitadeira, julgamento do comercial,
                    // não omissão (documento 48, §5.3). A colhedora de cana ganhou categoria própria em 27/09/2026.
                    if (categoriaDaLinha.TryGetValue(linhaDaMaquina, out var categoria))
                        codigoDaCategoria = categoria.Codigo;
                    else
                        emLinhaSemCategoria = true;
                }

                // O FILTRO "TIPO DE PRODUTO" (a categoria): só a máquina que se SABE ser da categoria entra. A de
                // linha sem categoria ou sem classificação fica de fora — não se sabe de que tipo ela é.
                if (consulta.CategoriaDeMaquina is { } filtroDeCategoria
                    && !string.Equals(codigoDaCategoria, filtroDeCategoria, StringComparison.Ordinal))
                    continue;

                // O GRUPO SÓ NASCE COM VENDA DO PERÍODO, como no faturamento: o ano anterior acumula à parte.
                //
                // DOIS "SE" INDEPENDENTES, e não um ou-outro (revisão de 27/09/2026): numa janela de mais de doze
                // meses, os meses do meio são das DUAS janelas — o fim da anterior é o começo da pedida —, e a
                // venda deles conta nas duas, como já contava no faturamento em reais.
                void Contar(Acumulador acumulador)
                {
                    acumulador.MaquinasVendidas++;
                    if (semClassificacao) acumulador.MaquinasSemClassificacao++;
                    else if (emLinhaSemCategoria) acumulador.MaquinasEmLinhaSemCategoria++;
                    else
                        acumulador.MaquinasPorCategoria[codigoDaCategoria!] =
                            acumulador.MaquinasPorCategoria.GetValueOrDefault(codigoDaCategoria!) + 1;
                }

                if (doPeriodo) Contar(Do(grupoDaMaquina));
                if (janelaAnterior.Contem(dia)) Contar(DoAnterior(grupoDaMaquina));

                // O MÊS A MÊS É POR MÊS, e cada mês uma vez só: as duas séries leem do mesmo dicionário.
                if (grupoDaMaquina > 0)
                    NoMes(grupoDaMaquina, new DateOnly(dia.Year, dia.Month, 1)).MaquinasVendidas++;
            }
        }

        return new RecorteAcumulado(acumuladores, anteriores, mensal);
    }
}
