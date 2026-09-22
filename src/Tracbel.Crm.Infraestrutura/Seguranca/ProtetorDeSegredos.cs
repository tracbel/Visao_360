using System.Security.Cryptography;
using System.Text;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Seguranca;

/// <summary>
/// A PROTEÇÃO DA CREDENCIAL DAS CONEXÕES (issue 136).
///
/// <para><b>No Windows — o servidor e a estação —, a proteção é a do sistema para a máquina (DPAPI,
/// <see cref="DataProtectionScope.LocalMachine"/>).</b> A API, o orquestrador e a carga rodam como serviço e tarefa
/// do Windows na mesma máquina, com contas diferentes; a proteção da máquina é a que todos abrem, e nenhum outro
/// computador abre. Uma cópia do banco levada para outra máquina chega sem as senhas: quem administra digita de
/// novo. Não há chave em arquivo para guardar, girar ou perder.</para>
///
/// <para><b>Fora do Windows — o CI, no Linux —, AES-GCM com uma chave sorteada a cada processo.</b> Serve ao teste:
/// o que foi protegido abre enquanto o processo vive, e só. Nenhum servidor do CRM roda fora do Windows.</para>
///
/// <para><b>O primeiro byte diz quem protegeu</b> (1 = DPAPI, 2 = AES do processo), para a mensagem de "não abre"
/// dizer por quê.</para>
/// </summary>
public sealed class ProtetorDeSegredos : IProtetorDeSegredos
{
    private const byte MarcaDpapi = 1;
    private const byte MarcaAesDoProcesso = 2;

    // A ENTROPIA AMARRA O SEGREDO A ESTE USO: um blob DPAPI de outro programa da máquina não abre como credencial de
    // conexão, e o contrário também não.
    private static readonly byte[] Entropia = "TracbelCrm.Conexao.v1"u8.ToArray();

    private static readonly byte[] ChaveDoProcesso = RandomNumberGenerator.GetBytes(32);

    /// <inheritdoc />
    public byte[] Proteger(string segredo)
    {
        ArgumentException.ThrowIfNullOrEmpty(segredo);
        var aberto = Encoding.UTF8.GetBytes(segredo);

        try
        {
            if (OperatingSystem.IsWindows())
                return [MarcaDpapi, .. ProtectedData.Protect(aberto, Entropia, DataProtectionScope.LocalMachine)];

            var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
            var tag = new byte[AesGcm.TagByteSizes.MaxSize];
            var cifrado = new byte[aberto.Length];
            using (var aes = new AesGcm(ChaveDoProcesso, tag.Length))
                aes.Encrypt(nonce, aberto, cifrado, tag, Entropia);

            return [MarcaAesDoProcesso, .. nonce, .. tag, .. cifrado];
        }
        finally
        {
            CryptographicOperations.ZeroMemory(aberto);
        }
    }

    /// <inheritdoc />
    public string Revelar(byte[] protegido)
    {
        if (protegido is not { Length: > 1 }) throw new InvalidOperationException("A credencial gravada está vazia.");

        try
        {
            switch (protegido[0])
            {
                case MarcaDpapi when OperatingSystem.IsWindows():
                    return Encoding.UTF8.GetString(ProtectedData.Unprotect(protegido.AsSpan(1).ToArray(), Entropia, DataProtectionScope.LocalMachine));

                case MarcaDpapi:
                    throw new InvalidOperationException("A credencial foi protegida pelo Windows e só abre no Windows da máquina que a gravou.");

                case MarcaAesDoProcesso:
                    var tamanhoDoNonce = AesGcm.NonceByteSizes.MaxSize;
                    var tamanhoDaTag = AesGcm.TagByteSizes.MaxSize;
                    var nonce = protegido.AsSpan(1, tamanhoDoNonce);
                    var tag = protegido.AsSpan(1 + tamanhoDoNonce, tamanhoDaTag);
                    var cifrado = protegido.AsSpan(1 + tamanhoDoNonce + tamanhoDaTag);
                    var aberto = new byte[cifrado.Length];
                    using (var aes = new AesGcm(ChaveDoProcesso, tamanhoDaTag))
                        aes.Decrypt(nonce, cifrado, tag, aberto, Entropia);
                    return Encoding.UTF8.GetString(aberto);

                default:
                    throw new InvalidOperationException("A credencial gravada não tem uma marca de proteção conhecida.");
            }
        }
        catch (CryptographicException)
        {
            // A MENSAGEM NÃO CITA O CONTEÚDO — só o motivo provável, que é o que quem administra precisa para agir.
            throw new InvalidOperationException(
                "A credencial gravada pela tela não abre nesta máquina — o banco provavelmente veio de uma cópia de outro servidor. " +
                "Digite a senha de novo em Configurações › Integrações.");
        }
    }
}
