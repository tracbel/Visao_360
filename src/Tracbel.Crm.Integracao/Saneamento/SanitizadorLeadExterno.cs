using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.Saneamento;

/// <summary>
/// Um lead exatamente como chegou de fora — site, RD Station, WhatsApp Business, planilha
/// de evento. Tudo texto solto, porque é assim que a origem entrega.
/// </summary>
public sealed record LeadExternoBruto(
    string? NomeContato,
    string? NomeEmpresa,
    string? Email,
    string? Telefone,
    string? CpfCnpj,
    string? Interesse);

/// <summary>
/// O mesmo lead depois do saneamento: só tipos de valor do domínio, prontos para
/// <c>Lead.Criar</c>. A regra de negócio "precisa de e-mail OU telefone" pertence à
/// entidade <c>Lead</c>, não a este pipeline — aqui cada campo é saneado por si; a
/// composição entre campos é decisão de domínio (documento 03, seção 2.2).
/// </summary>
public sealed record LeadExternoSaneado(
    string NomeContato,
    string? NomeEmpresa,
    Email? Email,
    Telefone? Telefone,
    CpfCnpj? Documento,
    string? Interesse);

/// <summary>
/// Saneia um lead recebido de fora antes de ele alcançar <c>Lead.Criar</c>.
///
/// Exemplo de referência do pipeline do documento 16: cada campo passa pelo tipo de valor
/// correspondente — o mesmo que qualquer entidade do domínio usa — e o que não valida vira
/// uma <see cref="CampoRejeitado"/>, nunca uma exceção que aborta o lote inteiro.
///
/// [V] É o oposto do sincronismo do Vórtice Mobile, que aborta a carga inteira numa única
/// linha ruim (5.302 vínculos quebrados — documento 01, achado 9.11).
/// </summary>
public sealed class SanitizadorLeadExterno : ISanitizador<LeadExternoBruto, LeadExternoSaneado>
{
    /// <inheritdoc />
    public ResultadoSaneamento<LeadExternoSaneado> Sanear(LeadExternoBruto bruto)
    {
        var rejeicoes = new List<CampoRejeitado>();
        var correcoes = new List<CorrecaoAplicada>();

        // Nome do contato é o único campo sem o qual o lead não tem dono de conversa —
        // sem ele, nem vale a pena avaliar o restante.
        if (!TextoNormalizado.TentarCriar(bruto.NomeContato, out var nome, tamanhoMaximo: 200))
        {
            rejeicoes.Add(new CampoRejeitado(nameof(bruto.NomeContato), bruto.NomeContato,
                "Nome do contato vazio ou ausente depois de normalizado."));
            return ResultadoSaneamento<LeadExternoSaneado>.Rejeitar(
                "Lead sem nome de contato utilizável.", rejeicoes);
        }
        if (bruto.NomeContato != nome.Valor)
            correcoes.Add(new CorrecaoAplicada(nameof(bruto.NomeContato), bruto.NomeContato, nome.Valor,
                "Espaço nas pontas, espaço duplo ou caractere invisível removido."));

        string? nomeEmpresa = null;
        if (TextoNormalizado.TentarCriar(bruto.NomeEmpresa, out var empresa, tamanhoMaximo: 200))
        {
            nomeEmpresa = empresa.Valor;
            if (bruto.NomeEmpresa != empresa.Valor)
                correcoes.Add(new CorrecaoAplicada(nameof(bruto.NomeEmpresa), bruto.NomeEmpresa, empresa.Valor,
                    "Espaço nas pontas, espaço duplo ou caractere invisível removido."));
        }
        else if (!string.IsNullOrWhiteSpace(bruto.NomeEmpresa))
        {
            rejeicoes.Add(new CampoRejeitado(nameof(bruto.NomeEmpresa), bruto.NomeEmpresa,
                "Vazio depois de normalizado."));
        }

        Email? email = null;
        if (Email.TentarCriar(bruto.Email, out var emailValido))
        {
            email = emailValido;
            if (!string.Equals(bruto.Email?.Trim(), emailValido.Endereco, StringComparison.Ordinal))
                correcoes.Add(new CorrecaoAplicada(nameof(bruto.Email), bruto.Email, emailValido.Endereco,
                    "Normalizado para minúsculo, sem espaço nas pontas."));
        }
        else if (!string.IsNullOrWhiteSpace(bruto.Email))
        {
            rejeicoes.Add(new CampoRejeitado(nameof(bruto.Email), bruto.Email, "Formato de e-mail inválido."));
        }

        Telefone? telefone = null;
        if (Telefone.TentarCriar(bruto.Telefone, out var telefoneValido))
        {
            telefone = telefoneValido;
            if (bruto.Telefone != telefoneValido.Numero)
                correcoes.Add(new CorrecaoAplicada(nameof(bruto.Telefone), bruto.Telefone, telefoneValido.Numero,
                    "Máscara removida, DDI nacional removido ou nono dígito completado."));
        }
        else if (!string.IsNullOrWhiteSpace(bruto.Telefone))
        {
            rejeicoes.Add(new CampoRejeitado(nameof(bruto.Telefone), bruto.Telefone,
                "Não é um telefone brasileiro válido (10 ou 11 dígitos com DDD)."));
        }

        CpfCnpj? documento = null;
        if (CpfCnpj.TentarCriar(bruto.CpfCnpj, out var documentoValido))
        {
            documento = documentoValido;
            if (bruto.CpfCnpj != documentoValido.Numero)
                correcoes.Add(new CorrecaoAplicada(nameof(bruto.CpfCnpj), bruto.CpfCnpj, documentoValido.Numero,
                    "Máscara removida."));
        }
        else if (!string.IsNullOrWhiteSpace(bruto.CpfCnpj))
        {
            rejeicoes.Add(new CampoRejeitado(nameof(bruto.CpfCnpj), bruto.CpfCnpj,
                "Dígito verificador de CPF/CNPJ não confere."));
        }

        string? interesse = null;
        if (TextoNormalizado.TentarCriar(bruto.Interesse, out var interesseValido, tamanhoMaximo: 400))
        {
            interesse = interesseValido.Valor;
            if (bruto.Interesse != interesseValido.Valor)
                correcoes.Add(new CorrecaoAplicada(nameof(bruto.Interesse), bruto.Interesse, interesseValido.Valor,
                    "Espaço nas pontas, espaço duplo ou caractere invisível removido."));
        }

        var saneado = new LeadExternoSaneado(nome.Valor, nomeEmpresa, email, telefone, documento, interesse);
        return ResultadoSaneamento<LeadExternoSaneado>.Aceitar(saneado, correcoes, rejeicoes);
    }
}
