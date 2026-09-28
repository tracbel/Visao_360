using System.Globalization;
using MySqlConnector;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;

namespace Tracbel.Crm.Integracao.OperationsCenter;

/// <summary>
/// A LEITURA DO OPERATIONS CENTER — a última leitura de horas e a última posição de cada máquina, do banco do BI
/// (decisão de 28/09/2026).
///
/// <para><b>Três consultas, e nenhuma na view.</b> A <c>view_last_machine_location</c> do BI faz o mesmo e passa de 45 s
/// por agregado. Aqui a última leitura de cada máquina sai pela chave primária das tabelas de histórico
/// (<c>MachineId</c> + data): uma busca por máquina, que leva de 76 s a 94 s nas 7.882 máquinas (medido em 27/09/2026).
/// Horas e posição vêm separadas porque têm datas diferentes — a máquina pode mandar posição sem ligar o motor.</para>
///
/// <para><b>As datas vêm como o BI guarda</b>, e o CRM as trata como UTC: são as da API do Operations Center, que as
/// devolve em UTC. O relógio do servidor do BI está em UTC−3, mas ele só carimba a carga, não a leitura.</para>
///
/// <para><b>Nada de nome.</b> A tabela de clientes e de organizações do BI tem nome de fazenda e de pessoa; esta leitura
/// não toca nelas. O que sai daqui é chassi, horas e coordenada.</para>
/// </summary>
/// <param name="opcoes">A configuração.</param>
public sealed class LeitorDoOperationsCenter(OpcoesDoOperationsCenter opcoes)
{
    /// <summary>O código do sistema em <c>integracao.Sistema</c> — o mesmo da conexão.</summary>
    public const string CodigoDoSistema = "OPERATIONS_CENTER";

    /// <summary>Lê as máquinas com a última leitura de cada coisa.</summary>
    /// <param name="relatar">Onde a leitura escreve o andamento — as consultas levam minutos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<IReadOnlyList<MaquinaNaTelemetria>>> LerAsync(Action<string> relatar, CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<IReadOnlyList<MaquinaNaTelemetria>>.Indisponivel(
                "A leitura do Operations Center exige OperationsCenter__Servidor, __Banco, __Usuario e __Senha — ou a conexão " +
                "\"Operations Center\" configurada em Configurações > Integrações.");

        // O NOME DA TABELA ENTRA NA CONSULTA, e por isso é conferido: só letra, número e sublinhado.
        var tabela = string.IsNullOrWhiteSpace(opcoes.Tabela) ? OpcoesDoOperationsCenter.TabelaDasMaquinasPadrao : opcoes.Tabela.Trim();
        if (!tabela.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))
            return Resultado<IReadOnlyList<MaquinaNaTelemetria>>.Indisponivel("O nome da tabela das máquinas tem caractere não aceito.");

        var construtor = new MySqlConnectionStringBuilder
        {
            Server = opcoes.Servidor,
            Port = opcoes.Porta,
            Database = opcoes.Banco,
            UserID = opcoes.Usuario,
            Password = opcoes.Senha,
            ConnectionTimeout = 15,
            DefaultCommandTimeout = (uint)opcoes.TempoLimiteSegundos,
            CharacterSet = "utf8mb4"
        };

        try
        {
            await using var conexao = new MySqlConnection(construtor.ConnectionString);
            await conexao.OpenAsync(ct);

            await using (var somenteLeitura = new MySqlCommand("SET SESSION TRANSACTION READ ONLY", conexao))
                await somenteLeitura.ExecuteNonQueryAsync(ct);

            // ---- 1. as máquinas ----
            var vins = new Dictionary<string, string?>(StringComparer.Ordinal);
            await using (var comando = new MySqlCommand($"SELECT MachineId, MachineVin FROM `{tabela}`", conexao))
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    if (leitor.IsDBNull(0)) continue;
                    vins[Convert.ToString(leitor.GetValue(0), CultureInfo.InvariantCulture)!] =
                        leitor.IsDBNull(1) ? null : Convert.ToString(leitor.GetValue(1), CultureInfo.InvariantCulture);
                }
            }

            relatar($"  {vins.Count:N0} máquinas no Operations Center; lendo a última leitura de horas de cada uma…");

            // ---- 2. a última leitura de horas, pela chave primária ----
            var horas = new Dictionary<string, (decimal Valor, DateTime Em)>(StringComparer.Ordinal);
            // A CONSULTA PARTE DAS MÁQUINAS, e não do histórico: a subconsulta acha a data mais nova de cada máquina de trás
            // para a frente na chave primária, e o JOIN pega a linha dela pela mesma chave. Partir do histórico varreria as
            // 87 milhões de linhas.
            const string UltimasHoras =
                "SELECT x.MachineId, e.EngineHoursDatetime, e.EngineHoursValue FROM " +
                "(SELECT m.MachineId, (SELECT u.EngineHoursDatetime FROM machineenginehours u WHERE u.MachineId = m.MachineId " +
                "ORDER BY u.EngineHoursDatetime DESC LIMIT 1) AS d FROM `{0}` m) x " +
                "JOIN machineenginehours e ON e.MachineId = x.MachineId AND e.EngineHoursDatetime = x.d";
            await using (var comando = new MySqlCommand(string.Format(CultureInfo.InvariantCulture, UltimasHoras, tabela), conexao))
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    if (leitor.IsDBNull(0) || leitor.IsDBNull(1) || leitor.IsDBNull(2)) continue;
                    horas[Convert.ToString(leitor.GetValue(0), CultureInfo.InvariantCulture)!] =
                        (Convert.ToDecimal(leitor.GetValue(2), CultureInfo.InvariantCulture), Utc(leitor.GetDateTime(1)));
                }
            }

            relatar($"  {horas.Count:N0} com horas; lendo a última posição…");

            // ---- 3. a última posição, pela chave primária ----
            var posicoes = new Dictionary<string, (double Lat, double Lon, DateTime Em)>(StringComparer.Ordinal);
            const string UltimasPosicoes =
                "SELECT x.MachineId, l.LocationHistoryDateTime, l.LocationHistoryLat, l.LocationHistoryLon FROM " +
                "(SELECT m.MachineId, (SELECT u.LocationHistoryDateTime FROM machinelocationhistory u WHERE u.MachineId = m.MachineId " +
                "ORDER BY u.LocationHistoryDateTime DESC LIMIT 1) AS d FROM `{0}` m) x " +
                "JOIN machinelocationhistory l ON l.MachineId = x.MachineId AND l.LocationHistoryDateTime = x.d";
            await using (var comando = new MySqlCommand(string.Format(CultureInfo.InvariantCulture, UltimasPosicoes, tabela), conexao))
            await using (var leitor = await comando.ExecuteReaderAsync(ct))
            {
                while (await leitor.ReadAsync(ct))
                {
                    if (leitor.IsDBNull(0) || leitor.IsDBNull(1) || leitor.IsDBNull(2) || leitor.IsDBNull(3)) continue;
                    posicoes[Convert.ToString(leitor.GetValue(0), CultureInfo.InvariantCulture)!] = (
                        Convert.ToDouble(leitor.GetValue(2), CultureInfo.InvariantCulture),
                        Convert.ToDouble(leitor.GetValue(3), CultureInfo.InvariantCulture),
                        Utc(leitor.GetDateTime(1)));
                }
            }

            relatar($"  {posicoes.Count:N0} com posição.");

            return Resultado<IReadOnlyList<MaquinaNaTelemetria>>.Ok(
            [
                .. vins.Select(v =>
                {
                    var temHoras = horas.TryGetValue(v.Key, out var h);
                    var temPosicao = posicoes.TryGetValue(v.Key, out var p);
                    return new MaquinaNaTelemetria(
                        v.Key, v.Value,
                        temHoras ? h.Valor : null, temHoras ? h.Em : null,
                        temPosicao ? p.Lat : null, temPosicao ? p.Lon : null, temPosicao ? p.Em : null);
                })
            ]);
        }
        catch (MySqlException falha)
        {
            // A MENSAGEM DO CONECTOR NÃO CITA A SENHA, mas pode citar servidor e usuário: sai só o código.
            return Resultado<IReadOnlyList<MaquinaNaTelemetria>>.Indisponivel(
                $"O Operations Center não respondeu à leitura (erro MySQL {falha.Number}). Nada foi gravado.");
        }
    }

    private static DateTime Utc(DateTime lida) => DateTime.SpecifyKind(lida, DateTimeKind.Utc);
}
