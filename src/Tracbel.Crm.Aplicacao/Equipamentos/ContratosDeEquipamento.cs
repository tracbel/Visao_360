using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Equipamentos;

/// <summary>A máquina como a listagem a mostra.</summary>
/// <param name="Chave">O GUID público da máquina.</param>
/// <param name="Chassi">O chassi — a identidade de verdade do equipamento agrícola.</param>
/// <param name="ModeloCodigo">Código do modelo no catálogo de frota.</param>
/// <param name="ModeloNome">Nome do modelo. Ex.: 7250R.</param>
/// <param name="Marca">Marca do modelo.</param>
/// <param name="MarcaRepresentada">Falso quando é máquina de concorrente — o que a Cobertura usa.</param>
/// <param name="Situacao">Estoque, Ativo, Vendido, Baixado ou ProprietarioNaoConfirmado.</param>
/// <param name="Origem">Protheus (ativo faturado), Crm (declarado pelo CEN) ou Art (venda registrada no ART).</param>
/// <param name="AnoModelo">Ano do modelo.</param>
/// <param name="ClienteChave">O GUID do dono, quando a máquina tem dono.</param>
/// <param name="ClienteNome">A razão social do dono.</param>
/// <param name="CriadoEm">Quando o cadastro nasceu (UTC).</param>
/// <param name="AlteradoEm">Última alteração (UTC).</param>
/// <param name="EstaInativo">Se a máquina foi baixada.</param>
/// <param name="ClassificacaoCodigo">A classificação de produto. Ex.: TRATOR_MEDIO.</param>
/// <param name="ClassificacaoNome">O nome da classificação. Ex.: Trator médio.</param>
/// <param name="Porte">O porte da classificação.</param>
/// <param name="Vendas">Quantas vendas a máquina tem registradas.</param>
/// <param name="UltimaVendaEm">A data da venda mais recente.</param>
/// <param name="CompradorNaUltimaVendaChave">O comprador NA venda mais recente — não é o dono atual.</param>
/// <param name="CompradorNaUltimaVendaNome">O nome desse comprador.</param>
/// <param name="NaturezaDoVinculo">A natureza do vínculo do comprador: CompradorNaVenda.</param>
/// <param name="ProdutoNaOrigem">O produto como a origem da venda escreve.</param>
/// <param name="SistemaDaVenda">O sistema de onde a venda veio.</param>
public sealed record EquipamentoResumo(
    Guid Chave,
    string Chassi,
    string? ModeloCodigo,
    string? ModeloNome,
    string? Marca,
    bool? MarcaRepresentada,
    string Situacao,
    string Origem,
    short? AnoModelo,
    Guid? ClienteChave,
    string? ClienteNome,
    DateTime CriadoEm,
    DateTime? AlteradoEm,
    bool EstaInativo,
    string? ClassificacaoCodigo,
    string? ClassificacaoNome,
    string? Porte,
    int Vendas,
    DateOnly? UltimaVendaEm,
    Guid? CompradorNaUltimaVendaChave,
    string? CompradorNaUltimaVendaNome,
    string? NaturezaDoVinculo,
    string? ProdutoNaOrigem,
    string? SistemaDaVenda)
{
    /// <summary>Traduz a leitura para o que a listagem mostra.</summary>
    public static EquipamentoResumo De(EquipamentoComContexto leitura)
    {
        var e = leitura.Equipamento;
        return new EquipamentoResumo(
            e.ChavePublica,
            e.Chassi.Numero,
            leitura.Modelo?.Codigo,
            leitura.Modelo?.Nome,
            leitura.Modelo?.Marca,
            leitura.Modelo?.MarcaRepresentada,
            e.Situacao.ToString(),
            e.Origem.ToString(),
            e.AnoModelo,
            leitura.ClienteChave,
            leitura.ClienteNome,
            e.CriadoEm,
            e.AlteradoEm,
            e.EstaExcluido,
            leitura.Classificacao?.Codigo,
            leitura.Classificacao?.Nome,
            leitura.Classificacao?.Porte.ToString(),
            leitura.UltimaVenda?.Vendas ?? 0,
            leitura.UltimaVenda?.VendidaEm,
            leitura.UltimaVenda?.CompradorChave,
            leitura.UltimaVenda?.CompradorNome,
            leitura.UltimaVenda is null ? null : nameof(NaturezaDoVinculoComEquipamento.CompradorNaVenda),
            leitura.UltimaVenda?.ProdutoNaOrigem,
            leitura.UltimaVenda?.SistemaCodigo);
    }
}

/// <summary>A máquina como a ficha a mostra, com o carimbo de concorrência.</summary>
/// <param name="Chave">O GUID público.</param>
/// <param name="Chassi">O chassi.</param>
/// <param name="NumeroSerie">Número de série, quando diferente do chassi.</param>
/// <param name="Placa">Placa, quando a máquina é emplacada.</param>
/// <param name="ModeloCodigo">Código do modelo no catálogo de frota.</param>
/// <param name="ModeloNome">Nome do modelo.</param>
/// <param name="Familia">Família a que o modelo pertence.</param>
/// <param name="Marca">Marca do modelo.</param>
/// <param name="MarcaRepresentada">Falso quando é máquina de concorrente.</param>
/// <param name="Situacao">Situação da máquina.</param>
/// <param name="Origem">Quem afirma que a máquina existe.</param>
/// <param name="AnoFabricacao">Ano de fabricação.</param>
/// <param name="AnoModelo">Ano do modelo.</param>
/// <param name="HorimetroAtual">Horímetro mais recente conhecido.</param>
/// <param name="LocalizacaoDescrita">Onde a máquina está, na descrição do CEN.</param>
/// <param name="ClienteChave">O GUID do dono.</param>
/// <param name="ClienteNome">A razão social do dono.</param>
/// <param name="EmpresaId">A filial dona do registro — a fronteira de acesso.</param>
/// <param name="CriadoEm">Quando nasceu (UTC).</param>
/// <param name="AlteradoEm">Última alteração (UTC).</param>
/// <param name="EstaInativo">Se foi baixada.</param>
/// <param name="Versao">O carimbo de concorrência, em base64. Devolva-o no PUT.</param>
/// <param name="ClassificacaoCodigo">A classificação de produto.</param>
/// <param name="ClassificacaoNome">O nome da classificação.</param>
/// <param name="Porte">O porte.</param>
/// <param name="Vendas">Quantas vendas a máquina tem.</param>
/// <param name="UltimaVendaEm">A data da venda mais recente.</param>
/// <param name="CompradorNaUltimaVendaChave">O comprador na venda mais recente.</param>
/// <param name="CompradorNaUltimaVendaNome">O nome dele.</param>
/// <param name="NaturezaDoVinculo">CompradorNaVenda, quando há venda.</param>
/// <param name="ProdutoNaOrigem">O produto como a origem escreve.</param>
/// <param name="SistemaDaVenda">O sistema de origem da venda.</param>
/// <param name="DivergenciasAbertas">As divergências abertas entre ART, CRM e Protheus sobre esta máquina.</param>
public sealed record EquipamentoDetalhe(
    Guid Chave,
    string Chassi,
    string? NumeroSerie,
    string? Placa,
    string? ModeloCodigo,
    string? ModeloNome,
    string? Familia,
    string? Marca,
    bool? MarcaRepresentada,
    string Situacao,
    string Origem,
    short? AnoFabricacao,
    short? AnoModelo,
    decimal? HorimetroAtual,
    string? LocalizacaoDescrita,
    Guid? ClienteChave,
    string? ClienteNome,
    int EmpresaId,
    DateTime CriadoEm,
    DateTime? AlteradoEm,
    bool EstaInativo,
    string? Versao,
    string? ClassificacaoCodigo,
    string? ClassificacaoNome,
    string? Porte,
    int Vendas,
    DateOnly? UltimaVendaEm,
    Guid? CompradorNaUltimaVendaChave,
    string? CompradorNaUltimaVendaNome,
    string? NaturezaDoVinculo,
    string? ProdutoNaOrigem,
    string? SistemaDaVenda,
    IReadOnlyList<DivergenciaDaMaquina> DivergenciasAbertas)
{
    /// <summary>Traduz a leitura para a ficha.</summary>
    public static EquipamentoDetalhe De(EquipamentoComContexto leitura)
    {
        var e = leitura.Equipamento;
        return new EquipamentoDetalhe(
            e.ChavePublica,
            e.Chassi.Numero,
            e.NumeroSerie,
            e.Placa,
            leitura.Modelo?.Codigo,
            leitura.Modelo?.Nome,
            leitura.Modelo?.Familia,
            leitura.Modelo?.Marca,
            leitura.Modelo?.MarcaRepresentada,
            e.Situacao.ToString(),
            e.Origem.ToString(),
            e.AnoFabricacao,
            e.AnoModelo,
            e.HorimetroAtual,
            e.LocalizacaoDescrita,
            leitura.ClienteChave,
            leitura.ClienteNome,
            e.EmpresaId,
            e.CriadoEm,
            e.AlteradoEm,
            e.EstaExcluido,
            e.Versao is null ? null : Convert.ToBase64String(e.Versao),
            leitura.Classificacao?.Codigo,
            leitura.Classificacao?.Nome,
            leitura.Classificacao?.Porte.ToString(),
            leitura.UltimaVenda?.Vendas ?? 0,
            leitura.UltimaVenda?.VendidaEm,
            leitura.UltimaVenda?.CompradorChave,
            leitura.UltimaVenda?.CompradorNome,
            leitura.UltimaVenda is null ? null : nameof(NaturezaDoVinculoComEquipamento.CompradorNaVenda),
            leitura.UltimaVenda?.ProdutoNaOrigem,
            leitura.UltimaVenda?.SistemaCodigo,
            leitura.Divergencias ?? []);
    }
}

/// <summary>
/// O que o <c>POST</c> de equipamento aceita. Tudo texto, pela mesma razão dos contratos de
/// cliente: a recusa é nossa, campo a campo, e não a do desserializador.
/// </summary>
/// <param name="Chassi">17 letras e números (I, O e Q inclusive, como a plaqueta da John Deere). Obrigatório e único.</param>
/// <param name="ModeloCodigo">Código do modelo no catálogo de frota. Nunca texto livre.</param>
/// <param name="ClienteChave">O GUID do dono. Obrigatório quando a situação é Ativo ou Vendido.</param>
/// <param name="Situacao">Estoque, Ativo, Vendido ou Baixado. Padrão: Ativo.</param>
/// <param name="Origem">Protheus ou Crm. Padrão: Crm.</param>
/// <param name="AnoFabricacao">Ano de fabricação.</param>
/// <param name="AnoModelo">Ano do modelo.</param>
/// <param name="NumeroSerie">Número de série.</param>
/// <param name="Placa">Placa.</param>
/// <param name="LocalizacaoDescrita">Onde a máquina opera, na descrição do CEN.</param>
/// <param name="LinhaDeProdutoCodigo">A classificação de produto, do catálogo LINHA_DE_PRODUTO. Opcional.</param>
public sealed record NovoEquipamento(
    string? Chassi = null,
    string? ModeloCodigo = null,
    string? ClienteChave = null,
    string? Situacao = null,
    string? Origem = null,
    string? AnoFabricacao = null,
    string? AnoModelo = null,
    string? NumeroSerie = null,
    string? Placa = null,
    string? LocalizacaoDescrita = null,
    string? LinhaDeProdutoCodigo = null);

/// <summary>
/// O que o <c>PUT</c> de equipamento aceita.
///
/// CHASSI E ORIGEM NÃO ESTÃO AQUI de propósito — a razão está escrita em
/// <see cref="Equipamento.Alterar"/>: o chassi é a identidade da máquina e a origem diz quem
/// afirma que ela existe. Nenhum dos dois muda por edição de tela.
/// </summary>
/// <param name="ModeloCodigo">Código do modelo no catálogo de frota.</param>
/// <param name="ClienteChave">O GUID do dono.</param>
/// <param name="Situacao">Situação da máquina.</param>
/// <param name="AnoFabricacao">Ano de fabricação.</param>
/// <param name="AnoModelo">Ano do modelo.</param>
/// <param name="NumeroSerie">Número de série.</param>
/// <param name="Placa">Placa.</param>
/// <param name="LocalizacaoDescrita">Onde a máquina opera.</param>
/// <param name="Versao">O carimbo de concorrência lido no GET, em base64.</param>
/// <param name="LinhaDeProdutoCodigo">
/// A classificação de produto. Ausente mantém a atual; texto vazio retira a classificação.
/// </param>
public sealed record AlteracaoDeEquipamento(
    string? ModeloCodigo = null,
    string? ClienteChave = null,
    string? Situacao = null,
    string? AnoFabricacao = null,
    string? AnoModelo = null,
    string? NumeroSerie = null,
    string? Placa = null,
    string? LocalizacaoDescrita = null,
    string? Versao = null,
    string? LinhaDeProdutoCodigo = null);

/// <summary>O que o <c>DELETE</c> de equipamento aceita.</summary>
/// <param name="Versao">O carimbo de concorrência lido no GET, em base64.</param>
public sealed record BaixaDeEquipamento(string? Versao = null);
