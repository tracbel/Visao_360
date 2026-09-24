using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Carga;

/// <summary>
/// Os códigos de pendência de uma máquina do cadastro de veículos do Protheus que não entrou no parque — gravados em
/// <c>integracao.RegistroDeOrigem.Motivos</c>, como os do ART e os das carteiras.
/// </summary>
internal static class MotivoDePendenciaDoParque
{
    /// <summary>O chassi aparece em mais de uma linha da VV1, com donos diferentes — nenhum é escolhido.</summary>
    public const string ChassiRepetidoComDonosDiferentes = "CHASSI_REPETIDO_COM_DONOS_DIFERENTES";

    /// <summary>A mesma máquina em outra linha da VV1, com o mesmo dono: a outra linha a representa.</summary>
    public const string LinhaRepetidaDaMesmaMaquina = "LINHA_REPETIDA_DA_MESMA_MAQUINA";

    /// <summary>Nem VIN de 17 posições nem identificador curto aceitável (símbolo, curto demais, longo demais, sem dígito).</summary>
    public const string IdentificadorForaDoPadrao = "IDENTIFICADOR_FORA_DO_PADRAO";

    /// <summary>O grupo do modelo é de componente (agricultura de precisão, motor, capota, kit).</summary>
    public const string Componente = "COMPONENTE_NAO_E_MAQUINA";

    /// <summary>A situação diz que a máquina está com a Tracbel: estoque, pedido, remessa, consignação, trânsito…</summary>
    public const string SituacaoForaDoCliente = "SITUACAO_FORA_DO_CLIENTE";

    /// <summary>Sem proprietário atual, ou o proprietário não tem CPF/CNPJ na SA1.</summary>
    public const string SemDonoNoProtheus = "SEM_DONO_NO_PROTHEUS";

    /// <summary>O proprietário atual é uma empresa do próprio grupo, pela raiz do CNPJ.</summary>
    public const string DonoEAPropriaTracbel = "DONO_E_A_PROPRIA_TRACBEL";

    /// <summary>O proprietário atual não é cliente do CRM.</summary>
    public const string DonoForaDoCrm = "DONO_FORA_DO_CRM";

    /// <summary>O documento do proprietário é de mais de um cliente do CRM.</summary>
    public const string DonoAmbiguoNoCrm = "DONO_AMBIGUO_NO_CRM";

    /// <summary>O chassi é de uma máquina baixada no CRM — não é reativada nem duplicada.</summary>
    public const string MaquinaBaixadaNoCrm = "MAQUINA_BAIXADA_NO_CRM";
}

/// <summary>O que a sincronia do parque fez (ou faria, na simulação), em número — sem nome nem documento.</summary>
/// <param name="Simulada">Verdadeiro quando nada foi gravado.</param>
/// <param name="Contagens">As contagens por etapa.</param>
/// <param name="Observacoes">O que merece leitura humana.</param>
internal sealed record RelatorioDoParqueDoProtheus(
    bool Simulada,
    IReadOnlyList<(string Etapa, string Rotulo, int Valor)> Contagens,
    IReadOnlyList<string> Observacoes)
{
    /// <summary>A soma das contagens com este rótulo, em qualquer etapa.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// O PARQUE DE MÁQUINAS PELO PROPRIETÁRIO ATUAL DO PROTHEUS (decisões de 24/09/2026) — <c>--somente-parque-protheus</c>,
/// a rotina <c>PARQUE_PROTHEUS</c> do orquestrador.
///
/// <para><b>O que entra.</b> Toda máquina do cadastro de veículos do Protheus (VV1) cujo dono atual — o
/// <c>VV1_PROATU + VV1_LJPATU</c>, casado com a SA1 — é cliente do CRM: nova e usada, John Deere e outras marcas. Fica
/// de fora, com o motivo na trilha: o componente (agricultura de precisão, motor, capota, kit), a máquina que está com a
/// Tracbel (estoque, pedido, remessa, consignação…), a que tem a própria Tracbel como dona (pela raiz do CNPJ, nunca
/// pelo nome), o chassi repetido com donos diferentes e o identificador que não é VIN nem número de série
/// aceitável.</para>
///
/// <para><b>Máquina, dono e evidência.</b> A máquina é o chassi (<c>frota.Equipamento</c>, origem Protheus quando
/// nasce aqui). O dono atual é um vínculo <see cref="NaturezaDoVinculoComEquipamento.ProprietarioAtual"/>, UM vigente
/// por máquina, com a evidência que o sustenta — nota de venda, ordem de serviço, só o cadastro antigo, ou a venda no
/// ART — e a data dela. O comprador de cada venda do ART continua no vínculo dele, como histórico.</para>
///
/// <para><b>Com o ART, vale a decisão 1</b> (<see cref="RegrasDoParque.Comparar"/>): para a máquina que tem venda no
/// ART, o dono do Protheus prevalece quando há evidência (nota ou ordem de serviço dele depois da venda, ou a mesma
/// raiz de CNPJ); sem evidência, ou com o Protheus dizendo que a dona é a Tracbel, o dono atual é o comprador do ART. A
/// divergência fica registrada pelo ciclo do ART, que é quem já as registra. Quando o Protheus e o ART concordam, a
/// máquina deixa de ser "proprietário não confirmado".</para>
///
/// <para><b>É SINCRONIA, e é repetível.</b> Rodar de novo sem mudança na origem não grava nada — cada método do
/// domínio compara antes de mudar, e a trilha de auditoria não ganha linha "nada mudou". Quando o dono muda, o vínculo
/// anterior é ENCERRADO com o motivo, nunca apagado. Leitura vazia não muda nada; e uma rodada que encerraria mais de
/// <see cref="FracaoMaximaDeEncerramento"/> dos vínculos vigentes é tratada como leitura parcial — os encerramentos
/// são recusados e a decisão fica no relatório, como na trava do faturamento.</para>
///
/// <para><b>Planejar, depois gravar.</b> A rodada inteira é calculada primeiro, SÓ COM LEITURA. A simulação para aí: não
/// abre transação de escrita, e os números que ela mostra são os do plano que a gravação executaria.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="lerParque">A leitura do cadastro de veículos do Protheus.</param>
/// <param name="raizesDoGrupo">As raízes de CNPJ das empresas do grupo.</param>
/// <param name="usuarioId">Quem roda a sincronia.</param>
/// <param name="relogio">O relógio (UTC).</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDoParqueDoProtheus(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<ParqueNoProtheus>>> lerParque,
    IReadOnlySet<string> raizesDoGrupo,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>O fluxo na trilha de <c>integracao.RegistroDeOrigem</c> — um registro por linha da VV1 com chassi.</summary>
    internal const string Fluxo = "PROTHEUS.PARQUE_VV1";

    /// <summary>
    /// A maior fração dos vínculos de dono atual vigentes que uma rodada pode encerrar. Dono de máquina muda devagar —
    /// revenda, troca na oficina —; encerrar um quinto do parque numa madrugada é leitura que veio pela metade, não o
    /// mercado mudando. O mesmo número da trava do faturamento.
    /// </summary>
    internal const double FracaoMaximaDeEncerramento = 0.20;

    /// <summary>Abaixo disto a trava não se aplica: num parque pequeno (a primeira semana, um teste), meia dúzia de trocas já passa de 20%.</summary>
    internal const int ParqueMinimoParaATrava = 1000;

    /// <summary>Rótulo: máquinas com dono atual cliente do CRM.</summary>
    internal const string RotuloDeMaquinasComDono = "máquinas com dono atual cliente do CRM";

    /// <summary>Rótulo: máquinas a criar.</summary>
    internal const string RotuloDeMaquinasCriadas = "máquinas novas no CRM (origem Protheus, sem dono confirmado)";

    /// <summary>Rótulo: vínculos de dono atual abertos.</summary>
    internal const string RotuloDeVinculosAbertos = "vínculos de dono atual abertos";

    /// <summary>Rótulo: vínculos de dono atual encerrados.</summary>
    internal const string RotuloDeVinculosEncerrados = "vínculos de dono atual encerrados (EncerradoEm, nunca apagados)";

    /// <summary>Rótulo: vínculos mantidos.</summary>
    internal const string RotuloDeVinculosMantidos = "vínculos de dono atual mantidos sem alteração";

    /// <summary>Rótulo: vínculos com evidência ou data atualizada.</summary>
    internal const string RotuloDeVinculosAtualizados = "vínculos de dono atual com evidência, data ou filial atualizada";

    /// <summary>Rótulo: donos confirmados.</summary>
    internal const string RotuloDeConfirmados = "máquinas com o dono confirmado agora (Protheus = comprador do ART)";

    /// <summary>Se a rodada deve ter os encerramentos recusados como leitura parcial.</summary>
    /// <param name="vigentes">Os vínculos de dono atual vigentes antes da rodada.</param>
    /// <param name="aEncerrar">Os que a rodada encerraria.</param>
    internal static bool EncerramentoPassaDaTrava(int vigentes, int aEncerrar) =>
        vigentes >= ParqueMinimoParaATrava && aEncerrar > vigentes * FracaoMaximaDeEncerramento;

    private readonly List<(string Etapa, string Rotulo, int Valor)> _contagens = [];
    private readonly List<string> _observacoes = [];

    /// <summary>Executa a sincronia.</summary>
    /// <param name="simular">Só planeja e conta: não abre transação de escrita.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDoParqueDoProtheus>> ExecutarAsync(bool simular, CancellationToken ct)
    {
        relatar("Lendo o cadastro de veículos do Protheus (VV1010, com SA1, VV0/VVA e VO1 — sessão somente leitura)…");
        var lido = await lerParque(ct);
        if (!lido.EhSucesso) return Resultado<RelatorioDoParqueDoProtheus>.Indisponivel(lido.Erro!);

        var parque = lido.Valor;

        // A ORIGEM VAZIA NÃO É UM PARQUE SEM MÁQUINAS: é uma leitura que falhou sem dizer. Sincronizar contra ela
        // encerraria todos os donos do CRM de uma vez.
        if (parque.Maquinas.Count(m => m.Chassi.Length > 0) == 0)
            return Resultado<RelatorioDoParqueDoProtheus>.Indisponivel(
                "O cadastro de veículos do Protheus veio sem máquina nenhuma com chassi. Isso não é um parque vazio, é uma " +
                "leitura a conferir — nada foi encerrado e nada foi gravado.");

        Plano plano;
        await using (var leituraDoCrm = abrirContexto())
        {
            plano = await PlanejarAsync(leituraDoCrm, parque, ct);
        }

        Relatar(parque, plano);

        if (!plano.EsquemaDoParque && !simular)
            return Resultado<RelatorioDoParqueDoProtheus>.Indisponivel(
                "O banco ainda não tem a migração ParquePeloProprietarioAtual (o vínculo de dono atual e a evidência). " +
                "Publique a versão com ela antes de ligar a rotina — nada foi gravado.");

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado — o plano foi calculado só com leitura, e nenhuma transação de escrita foi aberta.");
            return Resultado<RelatorioDoParqueDoProtheus>.Ok(new RelatorioDoParqueDoProtheus(true, _contagens, _observacoes));
        }

        await AplicarAsync(parque, plano, relogio(), ct);
        return Resultado<RelatorioDoParqueDoProtheus>.Ok(new RelatorioDoParqueDoProtheus(false, _contagens, _observacoes));
    }

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private async Task<Plano> PlanejarAsync(CrmDbContext banco, ParqueNoProtheus parque, CancellationToken ct)
    {
        var plano = new Plano { EsquemaDoParque = await EsquemaDoParqueExisteAsync(banco, ct) };
        if (!plano.EsquemaDoParque)
            _observacoes.Add("O banco lido ainda não tem a migração ParquePeloProprietarioAtual: não há dono atual nem venda com o dono " +
                             "do Protheus no lugar do comprador gravados — o plano parte de nenhum dono atual vigente, que é o que o " +
                             "banco terá logo depois da migração.");

        plano.SistemaDoArtId = await banco.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == LeitorDoArt.CodigoDoSistema).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);
        plano.SistemaDoProtheusId = await banco.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == LeitorDeClientesDoProtheus.CodigoDoSistema).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);

        // ---------------------------------------------------------------------------------------------
        // O que o CRM já tem.
        // ---------------------------------------------------------------------------------------------
        var clientes = await banco.Clientes.AsNoTracking()
            .Where(c => c.ExcluidoEm == null && c.Documento != null)
            .Select(c => new { c.Id, c.EmpresaId, c.Documento })
            .ToListAsync(ct);
        var clientesPorDocumento = clientes
            .GroupBy(c => c.Documento!.Value.Numero, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(c => (c.Id, c.EmpresaId)).ToList(), StringComparer.Ordinal);
        var empresaDoCliente = clientes.ToDictionary(c => c.Id, c => c.EmpresaId);
        var documentoDoCliente = clientes.ToDictionary(c => c.Id, c => c.Documento!.Value.Numero);

        var maquinas = await banco.Equipamentos.AsNoTracking()
            .Select(e => new MaquinaNoCrm(e.Id, e.Chassi, e.ExcluidoEm != null, e.ClienteId, e.Situacao, e.ModeloId, e.AnoFabricacao, e.AnoModelo))
            .ToListAsync(ct);
        var ativasPorChassi = maquinas.Where(m => !m.Baixada).ToDictionary(m => m.Chassi.Numero, StringComparer.Ordinal);
        var baixadas = maquinas.Where(m => m.Baixada).Select(m => m.Chassi.Numero).ToHashSet(StringComparer.Ordinal);
        var maquinaPorId = maquinas.ToDictionary(m => m.Id);

        // A REGRA NOVA DO CHASSI NÃO REESCREVE NENHUMA CHAVE (pedido da sessão do ART, 24/09/2026): o que o banco guarda
        // tem de se normalizar em si mesmo. Um só que não se normalize é migração de dados, e não regra nova.
        plano.ChavesQueMudariam = maquinas.Count(m => !SeNormalizaEmSiMesma(m.Chassi.Numero));
        plano.MaquinasNoCrm = maquinas.Count(m => !m.Baixada);

        var ultimaVendaNoArt = plano.SistemaDoArtId is { } art
            ? (plano.EsquemaDoParque
                    ? await banco.VendasDeMaquina.AsNoTracking()
                        .Where(v => v.SistemaId == art)
                        .Select(v => new VendaNoArt(v.Id, v.EquipamentoId, v.CompradorId, v.VendidaEm, v.FaturadaEm, v.CompradorPeloDonoNoProtheus))
                        .ToListAsync(ct)
                    : await banco.VendasDeMaquina.AsNoTracking()
                        .Where(v => v.SistemaId == art)
                        .Select(v => new VendaNoArt(v.Id, v.EquipamentoId, v.CompradorId, v.VendidaEm, v.FaturadaEm, false))
                        .ToListAsync(ct))
                .GroupBy(v => v.EquipamentoId)
                .ToDictionary(g => g.Key, g => g
                    .OrderByDescending(v => v.FaturadaEm ?? v.VendidaEm)
                    .ThenByDescending(v => v.VendidaEm)
                    .ThenByDescending(v => v.Id)
                    .First())
            : [];

        plano.VigentesPorMaquina = plano.EsquemaDoParque
            ? (await banco.VinculosComEquipamento.AsNoTracking()
                    .Where(v => v.Natureza == NaturezaDoVinculoComEquipamento.ProprietarioAtual && v.EncerradoEm == null)
                    .Select(v => new VinculoVigente(v.Id, v.EquipamentoId, v.ClienteId, v.EmpresaId, v.SistemaId, v.ReferenciaEm, v.Evidencia))
                    .ToListAsync(ct))
                .ToDictionary(v => v.EquipamentoId)
            : [];

        var modelos = await banco.Modelos.AsNoTracking().Where(m => m.EstaAtivo)
            .Select(m => new ModeloDoCatalogo(m.Id, m.Codigo)).ToListAsync(ct);
        var modeloPorCodigo = new Dictionary<string, int?>(StringComparer.Ordinal);
        int? Modelo(string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;
            if (modeloPorCodigo.TryGetValue(codigo, out var achado)) return achado;

            // SÓ POR CÓDIGO IDÊNTICO E CANDIDATO ÚNICO — a mesma regra do produto do ART. Semelhança de nome não aponta
            // modelo.
            var avaliacao = ClassificacaoDoArt.ClassificarProduto(codigo, modelos);
            return modeloPorCodigo[codigo] = avaliacao.Situacao == SituacaoDaCorrespondencia.CorrespondenciaExata
                ? int.Parse(avaliacao.Destino!, CultureInfo.InvariantCulture)
                : null;
        }

        (long Id, int EmpresaId)? Cliente(string documento, out string? motivo)
        {
            motivo = null;
            if (!clientesPorDocumento.TryGetValue(documento, out var candidatos)) { motivo = MotivoDePendenciaDoParque.DonoForaDoCrm; return null; }
            if (candidatos.Count == 1) return candidatos[0];
            motivo = MotivoDePendenciaDoParque.DonoAmbiguoNoCrm;
            return null;
        }

        // ---------------------------------------------------------------------------------------------
        // 1. A máquina que tem venda no ART: a decisão 1 escolhe o dono atual.
        // ---------------------------------------------------------------------------------------------
        var tratados = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (equipamentoId, venda) in ultimaVendaNoArt)
        {
            if (!maquinaPorId.TryGetValue(equipamentoId, out var maquina) || maquina.Baixada) continue;

            var chassi = maquina.Chassi.Numero;

            // A VENDA QUE ENTROU COM O DONO DO PROTHEUS no lugar do comprador não tem comprador do ART para comparar: a
            // máquina segue a regra do Protheus sozinho, abaixo.
            if (venda.CompradorPeloDonoNoProtheus) continue;
            if (!documentoDoCliente.TryGetValue(venda.CompradorId, out var documentoDoComprador)) continue;

            tratados.Add(chassi);
            var noProtheus = parque.TentarAchar(chassi, out var achada) ? achada : null;
            var dataNoArt = venda.FaturadaEm ?? venda.VendidaEm;
            var desfecho = RegrasDoParque.Comparar(noProtheus, documentoDoComprador, dataNoArt, raizesDoGrupo);
            plano.Desfechos[desfecho] = plano.Desfechos.GetValueOrDefault(desfecho) + 1;

            Alvo? alvo = null;
            if (RegrasDoParque.ProtheusPrevalece(desfecho) && Cliente(noProtheus!.DocumentoDoDono!, out _) is { } donoNoProtheus)
            {
                // NO MESMO DONO, a evidência mais forte que o só-cadastro é a própria venda do ART.
                var evidencia = desfecho == DesfechoDaComparacaoComOArt.MesmoDono && noProtheus.Evidencia == EvidenciaDoProprietario.CadastroAntigo
                    ? EvidenciaDoProprietario.VendaNoArt
                    : noProtheus.Evidencia;
                var referencia = desfecho == DesfechoDaComparacaoComOArt.MesmoDono
                    ? new[] { noProtheus.DataDaEvidencia, dataNoArt }.Max()
                    : noProtheus.DataDaEvidencia;

                alvo = new Alvo(chassi, noProtheus, donoNoProtheus.Id, donoNoProtheus.EmpresaId, evidencia, referencia,
                    DoArt: evidencia == EvidenciaDoProprietario.VendaNoArt, Confirmar: desfecho == DesfechoDaComparacaoComOArt.MesmoDono, desfecho);
            }
            else if (desfecho == DesfechoDaComparacaoComOArt.ProtheusPrevalecePorEvidenciaPosterior)
            {
                // O PROTHEUS PREVALECE, mas o dono dele não é cliente do CRM: a máquina está com alguém de fora, e o
                // comprador do ART já não a tem. Nenhum dono atual.
                plano.MotivoPorChassi[chassi] = MotivoDePendenciaDoParque.DonoForaDoCrm;
                plano.ArtPrevaleceriaMasDonoForaDoCrm++;
            }
            else
            {
                // VALE O COMPRADOR DO ART: sem evidência, com o Protheus dizendo Tracbel, sem o Protheus — ou a mesma
                // empresa com o CNPJ do Protheus fora do CRM.
                alvo = new Alvo(chassi, noProtheus, venda.CompradorId, empresaDoCliente[venda.CompradorId], EvidenciaDoProprietario.VendaNoArt,
                    dataNoArt, DoArt: true, Confirmar: false, desfecho);
            }

            if (alvo is not null) plano.Alvos[chassi] = alvo with { EquipamentoId = equipamentoId };
        }

        // ---------------------------------------------------------------------------------------------
        // 2. A máquina só do Protheus: o dono atual, quando é cliente do CRM.
        // ---------------------------------------------------------------------------------------------
        foreach (var (chassi, noProtheus) in parque.PorChassi)
        {
            if (tratados.Contains(chassi)) continue;

            string? Motivo()
            {
                if (!Chassi.TentarCriar(chassi, out _) && !Chassi.TentarCriarIdentificadorConfirmado(chassi, out _))
                    return MotivoDePendenciaDoParque.IdentificadorForaDoPadrao;
                if (RegrasDoParque.EhComponente(noProtheus)) return MotivoDePendenciaDoParque.Componente;
                if (!noProtheus.EstaNoCliente) return MotivoDePendenciaDoParque.SituacaoForaDoCliente;
                if (noProtheus.DocumentoDoDono is null) return MotivoDePendenciaDoParque.SemDonoNoProtheus;
                if (RegrasDoParque.EhDoGrupo(noProtheus.DocumentoDoDono, raizesDoGrupo)) return MotivoDePendenciaDoParque.DonoEAPropriaTracbel;
                if (!ativasPorChassi.ContainsKey(chassi) && baixadas.Contains(chassi)) return MotivoDePendenciaDoParque.MaquinaBaixadaNoCrm;
                return null;
            }

            if (Motivo() is { } motivo)
            {
                plano.MotivoPorChassi[chassi] = motivo;
                continue;
            }

            if (Cliente(noProtheus.DocumentoDoDono!, out var motivoDoCliente) is not { } dono)
            {
                plano.MotivoPorChassi[chassi] = motivoDoCliente!;
                continue;
            }

            plano.Alvos[chassi] = new Alvo(chassi, noProtheus, dono.Id, dono.EmpresaId, noProtheus.Evidencia, noProtheus.DataDaEvidencia,
                DoArt: false, Confirmar: false, null)
            {
                EquipamentoId = ativasPorChassi.TryGetValue(chassi, out var existente) ? existente.Id : null
            };
        }

        foreach (var ambiguo in parque.Ambiguos.Where(c => !tratados.Contains(c)))
            plano.MotivoPorChassi[ambiguo] = MotivoDePendenciaDoParque.ChassiRepetidoComDonosDiferentes;

        // ---------------------------------------------------------------------------------------------
        // 3. O que muda: máquinas, vínculos de dono atual e confirmações.
        // ---------------------------------------------------------------------------------------------
        foreach (var alvo in plano.Alvos.Values)
        {
            var existente = alvo.EquipamentoId is not null ? ativasPorChassi[alvo.Chassi] : null;
            var noProtheus = alvo.NoProtheus;

            if (existente is null)
            {
                var chassi = Chassi.TentarCriar(alvo.Chassi, out var vin) ? vin : Chassi.TentarCriarIdentificadorConfirmado(alvo.Chassi, out var curto) ? curto : default;
                plano.MaquinasACriar.Add(new MaquinaACriar(chassi, alvo.EmpresaDoCliente, Modelo(noProtheus?.CodigoDoModelo),
                    noProtheus?.AnoFabricacao, noProtheus?.AnoModelo));
            }
            else if (noProtheus is not null)
            {
                // SÓ O QUE ESTÁ VAZIO: modelo e anos preenchidos — pela carga ou por uma pessoa — não são trocados.
                var modelo = existente.ModeloId is null ? Modelo(noProtheus.CodigoDoModelo) : null;
                var fabricacao = existente.AnoFabricacao is null && Equipamento.AnoAceito(noProtheus.AnoFabricacao) ? noProtheus.AnoFabricacao : null;
                var anoModelo = existente.AnoModelo is null && Equipamento.AnoAceito(noProtheus.AnoModelo) ? noProtheus.AnoModelo : null;
                if (modelo is not null || fabricacao is not null || anoModelo is not null)
                    plano.MaquinasAComplementar[existente.Id] = (modelo, fabricacao, anoModelo);
            }

            plano.VigentesPorMaquina.TryGetValue(existente?.Id ?? 0, out var vigente);
            if (vigente is null) plano.VinculosAAbrir.Add(alvo);
            else if (vigente.ClienteId != alvo.ClienteId)
            {
                plano.Trocas.Add((vigente, alvo));
            }
            else if (vigente.EmpresaId != alvo.EmpresaDoCliente || vigente.ReferenciaEm != alvo.ReferenciaEm || vigente.Evidencia != alvo.Evidencia
                     || vigente.SistemaId != (alvo.DoArt ? plano.SistemaDoArtId : plano.SistemaDoProtheusId))
            {
                plano.VinculosAAcompanhar.Add((vigente, alvo));
            }
            else plano.VinculosMantidos++;

            if (existente is null) continue;

            // A CONFIRMAÇÃO É DA SINCRONIA só enquanto ninguém mexeu no dono: a máquina ainda sem dono, ou com o dono que
            // a própria sincronia confirmou (o do vínculo vigente). O dono que uma pessoa pôs fica, e é contado.
            var confirmadoPelaSincronia = existente.Situacao == SituacaoDoEquipamento.Ativo && existente.ClienteId is { } atual && atual == vigente?.ClienteId;
            if (alvo.Confirmar)
            {
                if (existente.Situacao == SituacaoDoEquipamento.ProprietarioNaoConfirmado && existente.ClienteId is null)
                    plano.AConfirmar[existente.Id] = alvo.ClienteId;
                else if (confirmadoPelaSincronia && existente.ClienteId != alvo.ClienteId)
                    plano.AConfirmar[existente.Id] = alvo.ClienteId;
                else if (existente.ClienteId != alvo.ClienteId)
                    plano.DonosDaTelaPreservados++;
            }
            else if (alvo.Desfecho is not null && confirmadoPelaSincronia)
            {
                // A MÁQUINA DO ART CUJAS FONTES DEIXARAM DE CONCORDAR volta a "não confirmado" — mesmo quando o dono atual
                // continua o comprador do ART: confirmado é quando as duas dizem o mesmo. A máquina só do Protheus nunca
                // foi confirmada pela sincronia, e o dono dela, se houver, é de uma pessoa.
                plano.ADesconfirmar.Add(existente.Id);
            }
            else if (existente.ClienteId is { } daTela && daTela != alvo.ClienteId)
            {
                plano.DonosDaTelaPreservados++;
            }
        }

        // O dono vigente cuja máquina saiu do parque: o Protheus deixou de apontar para um cliente do CRM.
        var comAlvo = plano.Alvos.Values.Where(a => a.EquipamentoId is not null).Select(a => a.EquipamentoId!.Value).ToHashSet();
        var chassiPorId = maquinas.ToDictionary(m => m.Id, m => m.Chassi.Numero);
        foreach (var vigente in plano.VigentesPorMaquina.Values.Where(v => !comAlvo.Contains(v.EquipamentoId)))
        {
            var motivo = chassiPorId.TryGetValue(vigente.EquipamentoId, out var chassi) && plano.MotivoPorChassi.TryGetValue(chassi, out var porque)
                ? porque
                : "MAQUINA_FORA_DO_CADASTRO_DE_VEICULOS";
            plano.Saidas.Add((vigente, motivo));
        }

        // A TRAVA DE ENCERRAMENTO EM MASSA: a troca de dono também encerra.
        var aEncerrar = plano.Trocas.Count + plano.Saidas.Count;
        if (EncerramentoPassaDaTrava(plano.VigentesPorMaquina.Count, aEncerrar))
        {
            plano.EncerramentoRecusado = aEncerrar;
            _observacoes.Add(
                $"ENCERRAMENTO RECUSADO: a leitura encerraria {aEncerrar:N0} de {plano.VigentesPorMaquina.Count:N0} vínculos de dono atual " +
                $"(acima de {FracaoMaximaDeEncerramento:P0}). Tratado como leitura parcial do Protheus: nenhum dono foi trocado nem " +
                "encerrado nesta rodada; o que é novo entrou. Confira a leitura antes de rodar de novo.");
            foreach (var (vigente, _) in plano.Trocas) plano.AConfirmar.Remove(vigente.EquipamentoId);
            plano.ADesconfirmar.Clear();
            plano.Trocas.Clear();
            plano.Saidas.Clear();
        }

        // ---------------------------------------------------------------------------------------------
        // 4. A trilha da origem: uma linha por linha da VV1 com chassi.
        // ---------------------------------------------------------------------------------------------
        var registros = plano.SistemaDoProtheusId is { } protheus
            ? await banco.RegistrosDeOrigem.AsNoTracking()
                .Where(r => r.SistemaId == protheus && r.Fluxo == Fluxo)
                .Select(r => new { r.ChaveOrigem, r.HashDoConteudo, r.AusenteNaOrigemDesde })
                .ToDictionaryAsync(r => r.ChaveOrigem, r => (r.HashDoConteudo, r.AusenteNaOrigemDesde), StringComparer.Ordinal, ct)
            : [];

        var vistos = new HashSet<string>(StringComparer.Ordinal);
        foreach (var linha in parque.Maquinas.Where(m => m.Chassi.Length > 0 && m.ChaveInterna.Length > 0))
        {
            if (!vistos.Add(linha.ChaveInterna)) continue;

            string? motivo;
            if (parque.Ambiguos.Contains(linha.Chassi) && !plano.Alvos.ContainsKey(linha.Chassi))
                motivo = MotivoDePendenciaDoParque.ChassiRepetidoComDonosDiferentes;
            else if (parque.PorChassi.TryGetValue(linha.Chassi, out var escolhida) && !ReferenceEquals(escolhida, linha))
                motivo = MotivoDePendenciaDoParque.LinhaRepetidaDaMesmaMaquina;
            else
                motivo = plano.Alvos.ContainsKey(linha.Chassi) ? null : plano.MotivoPorChassi.GetValueOrDefault(linha.Chassi, MotivoDePendenciaDoParque.SemDonoNoProtheus);

            plano.Registros.Add(new DecisaoDaLinha(linha, motivo));

            var hash = Resumo(linha);
            if (!registros.TryGetValue(linha.ChaveInterna, out var registro)) plano.RegistrosNovos++;
            else if (!string.Equals(registro.HashDoConteudo, hash, StringComparison.Ordinal)) plano.RegistrosAlterados++;
            else plano.RegistrosIguais++;
        }

        plano.RegistrosQueSumiram = registros.Count(r => !vistos.Contains(r.Key) && r.Value.AusenteNaOrigemDesde is null);
        return plano;
    }

    // =============================================================================================
    // A gravação — uma transação, executando o plano
    // =============================================================================================

    private async Task AplicarAsync(ParqueNoProtheus parque, Plano plano, DateTime agora, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaDoProtheus = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeClientesDoProtheus.CodigoDoSistema, "Protheus (TOTVS) — ERP", "SQL Server, somente leitura", ct);

        // O QUE A SINCRONIA MUDA NUMA MÁQUINA OU NUM VÍNCULO QUE JÁ EXISTIA vai para a trilha como integração do Protheus;
        // o que nasce agora tem o rastro em integracao.RegistroDeOrigem.
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaDoProtheus);

        int Sistema(Alvo alvo) => alvo.DoArt && plano.SistemaDoArtId is { } art ? art : sistemaDoProtheus;

        // 1. As máquinas novas.
        var criadas = new Dictionary<string, Equipamento>(StringComparer.Ordinal);
        foreach (var nova in plano.MaquinasACriar)
        {
            var equipamento = Equipamento.RegistrarPelaIntegracao(
                nova.EmpresaId, nova.Chassi, OrigemDoEquipamento.Protheus, usuarioId, nova.ModeloId,
                anoFabricacao: nova.AnoFabricacao, anoModelo: nova.AnoModelo);
            banco.Equipamentos.Add(equipamento);
            criadas[nova.Chassi.Numero] = equipamento;
        }

        await banco.SaveChangesAsync(ct);
        relatar($"  {criadas.Count:N0} máquina(s) nova(s) gravada(s).");

        // 2. As máquinas que já existiam: o que estava vazio, e a confirmação do dono.
        var aAlterar = plano.MaquinasAComplementar.Keys.Concat(plano.AConfirmar.Keys).Concat(plano.ADesconfirmar).Distinct().ToList();
        var rastreadas = new Dictionary<long, Equipamento>();
        foreach (var bloco in aAlterar.Chunk(2000))
            foreach (var e in await banco.Equipamentos.Where(e => bloco.Contains(e.Id)).ToListAsync(ct))
                rastreadas[e.Id] = e;

        foreach (var (id, (modelo, fabricacao, anoModelo)) in plano.MaquinasAComplementar)
        {
            var equipamento = rastreadas[id];
            if (modelo is { } m) equipamento.DefinirModeloSeAusente(m, usuarioId);
            equipamento.DefinirAnosSeAusentes(fabricacao, anoModelo, usuarioId);
        }

        foreach (var (id, clienteId) in plano.AConfirmar) rastreadas[id].ConfirmarProprietario(clienteId, usuarioId);
        foreach (var id in plano.ADesconfirmar) rastreadas[id].DesfazerConfirmacaoDoProprietario(usuarioId);

        // 3. Os vínculos de dono atual: encerra o que saiu ou trocou de dono ANTES de abrir o novo — o índice único
        // aceita um vigente por máquina.
        var idsAEncerrar = plano.Trocas.Select(t => t.Vigente.Id).Concat(plano.Saidas.Select(s => s.Vigente.Id))
            .Concat(plano.VinculosAAcompanhar.Select(a => a.Vigente.Id)).ToHashSet();
        var vinculos = new Dictionary<long, VinculoDeClienteComEquipamento>();
        foreach (var bloco in idsAEncerrar.Chunk(2000))
            foreach (var v in await banco.VinculosComEquipamento.Where(v => bloco.Contains(v.Id)).ToListAsync(ct))
                vinculos[v.Id] = v;

        foreach (var (vigente, alvo) in plano.Trocas)
            vinculos[vigente.Id].Encerrar(
                $"O dono atual mudou ({Descrever(alvo)}).", agora, usuarioId);
        foreach (var (vigente, motivo) in plano.Saidas)
            vinculos[vigente.Id].Encerrar(
                $"O Protheus deixou de apontar um dono cliente do CRM para esta máquina ({motivo}).", agora, usuarioId);

        await banco.SaveChangesAsync(ct);

        foreach (var (vigente, alvo) in plano.VinculosAAcompanhar)
            vinculos[vigente.Id].AcompanharProprietario(alvo.EmpresaDoCliente, Sistema(alvo), alvo.ReferenciaEm, alvo.Evidencia, usuarioId);

        foreach (var alvo in plano.VinculosAAbrir.Concat(plano.Trocas.Select(t => t.Alvo)))
        {
            var equipamentoId = alvo.EquipamentoId ?? criadas[alvo.Chassi].Id;
            banco.VinculosComEquipamento.Add(VinculoDeClienteComEquipamento.RegistrarProprietarioAtual(
                alvo.EmpresaDoCliente, alvo.ClienteId, equipamentoId, Sistema(alvo), alvo.ReferenciaEm, alvo.Evidencia, usuarioId));
        }

        await banco.SaveChangesAsync(ct);

        // 4. A trilha da origem.
        var registros = await banco.RegistrosDeOrigem.Where(r => r.SistemaId == sistemaDoProtheus && r.Fluxo == Fluxo)
            .ToDictionaryAsync(r => r.ChaveOrigem, StringComparer.Ordinal, ct);

        foreach (var decisao in plano.Registros)
        {
            var retrato = Retrato(decisao.Linha);
            if (!registros.TryGetValue(decisao.Linha.ChaveInterna, out var registro))
            {
                registro = RegistroDeOrigem.Registrar(sistemaDoProtheus, Fluxo, decisao.Linha.ChaveInterna, retrato, agora);
                banco.RegistrosDeOrigem.Add(registro);
                registros[decisao.Linha.ChaveInterna] = registro;
            }
            else
            {
                registro.RegistrarLeitura(retrato, agora);
            }

            registro.Decidir(decisao.Motivo is null ? DecisaoDaIntegracao.Importado : DecisaoDaIntegracao.Pendente, decisao.Motivo, null);
        }

        var vistos = plano.Registros.Select(r => r.Linha.ChaveInterna).ToHashSet(StringComparer.Ordinal);
        foreach (var (chave, registro) in registros)
            if (!vistos.Contains(chave)) registro.MarcarAusente(agora);

        await banco.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        relatar("Gravado. A sincronia pode rodar de novo a qualquer hora: sem mudança na origem, nada muda aqui.");
    }

    // =============================================================================================
    // O relatório
    // =============================================================================================

    private void Relatar(ParqueNoProtheus parque, Plano plano)
    {
        const string etapaDaLeitura = "1. Leitura do cadastro de veículos (VV1)";
        var comChassi = parque.Maquinas.Where(m => m.Chassi.Length > 0).ToList();
        Contar(etapaDaLeitura, "linhas lidas", parque.Maquinas.Count);
        Contar(etapaDaLeitura, "  sem chassi (pedido de fábrica, sobretudo)", parque.Maquinas.Count - comChassi.Count);
        Contar(etapaDaLeitura, "chassis distintos (normalizados)", parque.PorChassi.Count + parque.Ambiguos.Count);
        Contar(etapaDaLeitura, "  VIN de 17 posições", parque.PorChassi.Keys.Count(c => Chassi.TentarCriar(c, out _)));
        Contar(etapaDaLeitura, "  identificador curto aceito (número de série)", parque.PorChassi.Keys.Count(c => Chassi.TentarCriarIdentificadorConfirmado(c, out _)));
        Contar(etapaDaLeitura, "  repetidos com donos diferentes (ambíguos)", parque.Ambiguos.Count);
        Contar(etapaDaLeitura, "com dono atual e CPF/CNPJ na SA1", parque.PorChassi.Values.Count(m => m.DocumentoDoDono is not null));
        foreach (var situacao in parque.PorChassi.Values.GroupBy(m => m.Situacao).OrderBy(g => g.Key, StringComparer.Ordinal))
            Contar(etapaDaLeitura, $"  situação '{(situacao.Key.Length == 0 ? "vazia (carga antiga)" : situacao.Key)}'", situacao.Count());

        const string etapaDoArt = "2. Máquinas com venda no ART: dono no Protheus × comprador do ART (decisão 1)";
        foreach (var desfecho in Enum.GetValues<DesfechoDaComparacaoComOArt>())
            Contar(etapaDoArt, desfecho.ToString(), plano.Desfechos.GetValueOrDefault(desfecho));
        Contar(etapaDoArt, "o Protheus prevaleceria, mas o dono dele não é cliente do CRM (fica sem dono atual)", plano.ArtPrevaleceriaMasDonoForaDoCrm);

        const string etapaDosDonos = "3. Dono atual por máquina";
        var alvos = plano.Alvos.Values.ToList();
        Contar(etapaDosDonos, RotuloDeMaquinasComDono, alvos.Count);
        foreach (var evidencia in Enum.GetValues<EvidenciaDoProprietario>())
            Contar(etapaDosDonos, $"  evidência: {evidencia}", alvos.Count(a => a.Evidencia == evidencia));
        Contar(etapaDosDonos, "  novas", alvos.Count(a => a.NoProtheus?.Nova == true));
        Contar(etapaDosDonos, "  usadas", alvos.Count(a => a.NoProtheus?.Nova == false));
        Contar(etapaDosDonos, "  sem estado novo/usado (ou fora do Protheus)", alvos.Count(a => a.NoProtheus?.Nova is null));
        foreach (var marca in alvos.GroupBy(a => a.NoProtheus?.Marca ?? "(sem marca / fora do Protheus)").OrderByDescending(g => g.Count()).Take(12))
            Contar(etapaDosDonos, $"  marca: {marca.Key}", marca.Count());
        Contar(etapaDosDonos, "clientes com máquina no parque", alvos.Select(a => a.ClienteId).Distinct().Count());
        foreach (var motivo in plano.MotivoPorChassi.Values.GroupBy(m => m, StringComparer.Ordinal).OrderByDescending(g => g.Count()))
            Contar(etapaDosDonos, $"máquinas fora do parque: {motivo.Key}", motivo.Count());

        const string etapaDasMaquinas = "4. Máquinas (frota.Equipamento)";
        Contar(etapaDasMaquinas, "máquinas ativas no CRM antes da rodada", plano.MaquinasNoCrm);
        Contar(etapaDasMaquinas, "chaves de máquina do CRM que a regra nova do chassi reescreveria (tem de ser 0)", plano.ChavesQueMudariam);
        Contar(etapaDasMaquinas, RotuloDeMaquinasCriadas, plano.MaquinasACriar.Count);
        Contar(etapaDasMaquinas, "  delas, com identificador curto (número de série)", plano.MaquinasACriar.Count(m => !m.Chassi.EhVin));
        Contar(etapaDasMaquinas, "  delas, com modelo do catálogo (código idêntico)", plano.MaquinasACriar.Count(m => m.ModeloId is not null));
        Contar(etapaDasMaquinas, "  delas, com ano de fabricação", plano.MaquinasACriar.Count(m => m.AnoFabricacao is not null));
        Contar(etapaDasMaquinas, "máquinas que já existiam no CRM, casadas pelo chassi", alvos.Count(a => a.EquipamentoId is not null));
        Contar(etapaDasMaquinas, "  com modelo ou ano vazio preenchido agora", plano.MaquinasAComplementar.Count);

        const string etapaDosVinculos = "5. Vínculos de dono atual (frota.VinculoDeClienteComEquipamento)";
        Contar(etapaDosVinculos, "vigentes antes da rodada", plano.VigentesPorMaquina.Count);
        Contar(etapaDosVinculos, RotuloDeVinculosAbertos, plano.VinculosAAbrir.Count + plano.Trocas.Count);
        Contar(etapaDosVinculos, "  em máquina sem dono atual", plano.VinculosAAbrir.Count);
        Contar(etapaDosVinculos, "  por troca de dono (o anterior é encerrado)", plano.Trocas.Count);
        Contar(etapaDosVinculos, RotuloDeVinculosEncerrados, plano.Trocas.Count + plano.Saidas.Count);
        Contar(etapaDosVinculos, "  porque a máquina saiu do parque", plano.Saidas.Count);
        Contar(etapaDosVinculos, RotuloDeVinculosAtualizados, plano.VinculosAAcompanhar.Count);
        Contar(etapaDosVinculos, RotuloDeVinculosMantidos, plano.VinculosMantidos);
        Contar(etapaDosVinculos, "encerramentos RECUSADOS pela trava de leitura parcial", plano.EncerramentoRecusado);
        Contar(etapaDosVinculos, "clientes que ganham máquina nesta rodada",
            plano.VinculosAAbrir.Concat(plano.Trocas.Select(t => t.Alvo)).Select(a => a.ClienteId).Distinct().Count());

        const string etapaDaConfirmacao = "6. Dono confirmado (frota.Equipamento.ClienteId)";
        Contar(etapaDaConfirmacao, RotuloDeConfirmados, plano.AConfirmar.Count);
        Contar(etapaDaConfirmacao, "máquinas que voltam a 'proprietário não confirmado' (o Protheus e o ART deixaram de concordar)", plano.ADesconfirmar.Count);
        Contar(etapaDaConfirmacao, "máquinas com dono posto por pessoa diferente do dono atual (preservado)", plano.DonosDaTelaPreservados);

        const string etapaDaTrilha = "7. Trilha (integracao.RegistroDeOrigem)";
        Contar(etapaDaTrilha, "linhas da VV1 com chassi na trilha", plano.Registros.Count);
        Contar(etapaDaTrilha, "  importadas (a máquina tem dono atual no CRM)", plano.Registros.Count(r => r.Motivo is null));
        foreach (var motivo in plano.Registros.Where(r => r.Motivo is not null).GroupBy(r => r.Motivo!, StringComparer.Ordinal).OrderByDescending(g => g.Count()))
            Contar(etapaDaTrilha, $"  pendentes: {motivo.Key}", motivo.Count());
        Contar(etapaDaTrilha, "registros lidos pela primeira vez", plano.RegistrosNovos);
        Contar(etapaDaTrilha, "registros já conhecidos com conteúdo alterado na origem", plano.RegistrosAlterados);
        Contar(etapaDaTrilha, "registros já conhecidos sem alteração", plano.RegistrosIguais);
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
    /// SE O BANCO JÁ TEM A MIGRAÇÃO DO PARQUE. A simulação roda da estação contra a produção ANTES da publicação que
    /// traz a migração — é assim que se mede o que a rotina fará —, e lá as colunas novas ainda não existem. Antes
    /// dela não há dono atual nem venda com o dono do Protheus no lugar do comprador, e o plano parte desse vazio.
    /// No SQLite dos testes o modelo é sempre o de hoje.
    /// </summary>
    private static async Task<bool> EsquemaDoParqueExisteAsync(CrmDbContext banco, CancellationToken ct) =>
        !banco.Database.IsSqlServer()
        || await banco.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.columns WHERE object_id = OBJECT_ID(N'frota.VinculoDeClienteComEquipamento') AND name = N'Evidencia'")
            .SingleAsync(ct) > 0;

    /// <summary>Se o chassi guardado sai igual da normalização e da regra de hoje — VIN ou identificador curto.</summary>
    /// <param name="numero">O chassi como o banco guarda.</param>
    internal static bool SeNormalizaEmSiMesma(string numero) =>
        Chassi.Normalizar(numero) == numero
        && ((Chassi.TentarCriar(numero, out var vin) && vin.Numero == numero)
            || (Chassi.TentarCriarIdentificadorConfirmado(numero, out var curto) && curto.Numero == numero));

    private static string Descrever(Alvo alvo) =>
        alvo.Desfecho is { } desfecho ? $"{desfecho}; evidência {alvo.Evidencia}" : $"evidência {alvo.Evidencia}";

    private static string? Limitar(string? texto, int tamanho) =>
        texto is null ? null : texto.Length <= tamanho ? texto : texto[..tamanho];

    /// <summary>
    /// O resumo do conteúdo da linha — mudou o dono, a situação, a evidência ou o modelo, muda o resumo. O documento
    /// entra no cálculo e não sai dele: o resumo não é reversível.
    /// </summary>
    private static string Resumo(MaquinaNoProtheus m) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('|',
            m.ChaveInterna, m.ChassiNaOrigem, m.CodigoDaMarca, m.CodigoDoModelo, m.GrupoDoModelo, m.Nova, m.Situacao,
            m.AnoFabricacao, m.AnoModelo, m.DocumentoDoDono, m.VendidaEm, m.UltimaVendaEm, m.UltimaVendaEDoDono,
            m.UltimaOrdemEm, m.UltimaOrdemEDoDono))));

    /// <summary>O retrato da linha na trilha — sem nome nem documento: o chassi, o grupo, o modelo e a marca.</summary>
    private static RetratoDoRegistroDeOrigem Retrato(MaquinaNoProtheus m) => new(
        Resumo(m),
        Limitar(m.ChassiNaOrigem, 200),
        Limitar(string.Join(" · ", new[] { m.GrupoDoModelo, m.DescricaoDoGrupo }.OfType<string>()), 60),
        Limitar(string.Join(" · ", new[] { m.CodigoDoModelo, m.Modelo }.OfType<string>()), 60),
        Limitar(m.Marca, 80),
        m.UltimaVendaEm,
        m.Chassi != m.ChassiNaOrigem ? "chassi: espaço interno ou letra minúscula normalizados" : null);

    // =============================================================================================
    // As peças do plano
    // =============================================================================================

    private sealed record MaquinaNoCrm(
        long Id, Chassi Chassi, bool Baixada, long? ClienteId, SituacaoDoEquipamento Situacao, int? ModeloId, short? AnoFabricacao, short? AnoModelo);

    private sealed record VendaNoArt(long Id, long EquipamentoId, long CompradorId, DateOnly? VendidaEm, DateOnly? FaturadaEm, bool CompradorPeloDonoNoProtheus);

    private sealed record VinculoVigente(
        long Id, long EquipamentoId, long ClienteId, int EmpresaId, int? SistemaId, DateOnly? ReferenciaEm, EvidenciaDoProprietario? Evidencia);

    /// <summary>O dono atual decidido para uma máquina.</summary>
    private sealed record Alvo(
        string Chassi,
        MaquinaNoProtheus? NoProtheus,
        long ClienteId,
        int EmpresaDoCliente,
        EvidenciaDoProprietario Evidencia,
        DateOnly? ReferenciaEm,
        bool DoArt,
        bool Confirmar,
        DesfechoDaComparacaoComOArt? Desfecho)
    {
        public long? EquipamentoId { get; init; }
    }

    private sealed record MaquinaACriar(Chassi Chassi, int EmpresaId, int? ModeloId, short? AnoFabricacao, short? AnoModelo);

    private sealed record DecisaoDaLinha(MaquinaNoProtheus Linha, string? Motivo);

    private sealed class Plano
    {
        public bool EsquemaDoParque { get; init; }

        public int? SistemaDoArtId { get; set; }

        public int? SistemaDoProtheusId { get; set; }

        public int MaquinasNoCrm { get; set; }

        public int ChavesQueMudariam { get; set; }

        public Dictionary<string, Alvo> Alvos { get; } = new(StringComparer.Ordinal);

        public Dictionary<string, string> MotivoPorChassi { get; } = new(StringComparer.Ordinal);

        public Dictionary<DesfechoDaComparacaoComOArt, int> Desfechos { get; } = [];

        public int ArtPrevaleceriaMasDonoForaDoCrm { get; set; }

        public Dictionary<long, VinculoVigente> VigentesPorMaquina { get; set; } = [];

        public List<MaquinaACriar> MaquinasACriar { get; } = [];

        public Dictionary<long, (int? ModeloId, short? AnoFabricacao, short? AnoModelo)> MaquinasAComplementar { get; } = [];

        public List<Alvo> VinculosAAbrir { get; } = [];

        public List<(VinculoVigente Vigente, Alvo Alvo)> Trocas { get; } = [];

        public List<(VinculoVigente Vigente, string Motivo)> Saidas { get; } = [];

        public List<(VinculoVigente Vigente, Alvo Alvo)> VinculosAAcompanhar { get; } = [];

        public int VinculosMantidos { get; set; }

        public int EncerramentoRecusado { get; set; }

        public Dictionary<long, long> AConfirmar { get; } = [];

        public List<long> ADesconfirmar { get; } = [];

        public int DonosDaTelaPreservados { get; set; }

        public List<DecisaoDaLinha> Registros { get; } = [];

        public int RegistrosNovos { get; set; }

        public int RegistrosAlterados { get; set; }

        public int RegistrosIguais { get; set; }

        public int RegistrosQueSumiram { get; set; }
    }
}
