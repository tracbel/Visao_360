using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Comum;

/// <summary>
/// Testes dos tipos de valor.
///
/// Cada caso aqui corresponde a um defeito real medido no Vórtice. Não são testes
/// decorativos: são a prova de que o defeito não pode se repetir.
/// </summary>
[Trait("Categoria", "Dominio")]
public class TelefoneTestes
{
    [Theory]
    // [V] O caso que quebrava a API do Vórtice: FoneNro1 é decimal(12) e estoura com DDI.
    [InlineData("+55 (17) 99999-0000", "17999990000")]
    [InlineData("5517999990000", "17999990000")]
    [InlineData("(17) 99999-0000", "17999990000")]
    [InlineData("17999990000", "17999990000")]
    [InlineData("17 3222-1000", "1732221000")]      // fixo, 10 dígitos
    [InlineData("  11987654321  ", "11987654321")]
    // Celular antigo (pré-migração de 2016), sem o nono dígito: o número local de 8
    // dígitos começando em 6-9 é celular, nunca fixo — o tipo completa o "9".
    [InlineData("(17) 8888-0000", "17988880000")]
    [InlineData("17 6543-2100", "17965432100")]
    public void Aceita_e_normaliza_formatos_reais(string entrada, string esperado)
    {
        Telefone.Criar(entrada).Numero.Should().Be(esperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("999999999999999")]
    [InlineData("0199999999")]        // DDD 01 não existe
    [InlineData("17899990000")]       // celular precisa começar com 9 depois do DDD
    [InlineData("abc")]
    public void Recusa_entrada_invalida(string entrada)
    {
        Telefone.TentarCriar(entrada, out _).Should().BeFalse();
        var acao = () => Telefone.Criar(entrada);
        acao.Should().Throw<RegraDeNegocioViolada>();
    }

    [Theory]
    // Números estrangeiros que, só por coincidência de dígitos, teriam DDD "válido" e
    // "nono dígito" 9 se fossem tratados como nacionais. O DDI explícito precisa bastar
    // para rejeitar, sem depender dessa coincidência.
    [InlineData("+1 397 123 4567")]
    [InlineData("+1 (305) 555-0100")]
    [InlineData("+351 912 345 678")]
    public void Recusa_numero_internacional_mesmo_quando_coincide_em_tamanho(string entrada)
    {
        Telefone.TentarCriar(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void TentarCriar_nao_lanca_para_permitir_importacao_em_lote()
    {
        // [V] O sincronismo do Vórtico Mobile aborta a carga INTEIRA numa única linha ruim.
        // A versão que não lança é o que permite registrar a linha e seguir com o lote.
        var entradas = new[] { "17999990000", "lixo", "11987654321" };

        var validos = entradas
            .Select(e => Telefone.TentarCriar(e, out var t) ? t : (Telefone?)null)
            .Where(t => t is not null)
            .ToList();

        validos.Should().HaveCount(2, "a linha ruim é descartada sem derrubar as boas");
    }

    [Theory]
    [InlineData("17999990000", "(17) 99999-0000")]
    [InlineData("1732221000", "(17) 3222-1000")]
    public void Formata_para_exibicao(string numero, string esperado)
    {
        Telefone.Criar(numero).Formatado().Should().Be(esperado);
    }

    [Fact]
    public void Distingue_celular_de_fixo()
    {
        Telefone.Criar("17999990000").EhCelular.Should().BeTrue();
        Telefone.Criar("1732221000").EhCelular.Should().BeFalse();
    }
}

[Trait("Categoria", "Dominio")]
public class EmailTestes
{
    [Theory]
    [InlineData("Pessoa.Exemplo@Tracbel.com.br", "pessoa.exemplo@tracbel.com.br")]
    [InlineData("  contato@fazenda.agr.br  ", "contato@fazenda.agr.br")]
    // E-mail inteiro em caixa alta — comum em planilha de importação digitada com Caps Lock.
    [InlineData("JOAO.SILVA@TRACBEL.COM.BR", "joao.silva@tracbel.com.br")]
    public void Normaliza_para_minusculo_e_sem_espaco(string entrada, string esperado)
    {
        // [V] A normalização na construção é o que torna a deduplicação confiável.
        // No Vórtice existem TRÊS modelos de e-mail concorrentes e nenhum normaliza.
        Email.Criar(entrada).Endereco.Should().Be(esperado);
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("@semlocal.com")]
    [InlineData("sem@dominio")]
    [InlineData("com espaco@teste.com")]
    [InlineData("")]
    public void Recusa_entrada_invalida(string entrada)
    {
        Email.TentarCriar(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void Extrai_o_dominio()
    {
        Email.Criar("joao@tracbel.com.br").Dominio.Should().Be("tracbel.com.br");
    }
}

[Trait("Categoria", "Dominio")]
public class CpfCnpjTestes
{
    [Theory]
    [InlineData("529.982.247-25")]     // CPF válido conhecido
    [InlineData("52998224725")]
    public void Aceita_cpf_valido(string entrada)
    {
        var doc = CpfCnpj.Criar(entrada);
        doc.EhPessoaFisica.Should().BeTrue();
        doc.Numero.Should().Be("52998224725");
    }

    [Theory]
    [InlineData("11.222.333/0001-81")] // CNPJ válido conhecido
    [InlineData("11222333000181")]
    public void Aceita_cnpj_valido(string entrada)
    {
        var doc = CpfCnpj.Criar(entrada);
        doc.EhPessoaFisica.Should().BeFalse();
        doc.Numero.Should().Be("11222333000181");
    }

    [Theory]
    [InlineData("52998224724")]        // dígito verificador errado
    [InlineData("11111111111")]        // sequência repetida
    [InlineData("00000000000")]
    [InlineData("11222333000182")]     // CNPJ com dígito errado
    [InlineData("123")]
    [InlineData("")]
    public void Recusa_documento_invalido(string entrada)
    {
        // [V] Foram medidos 116 CPFs repetidos entre clientes distintos no Vórtice,
        // porque a validação nunca aconteceu na entrada.
        CpfCnpj.TentarCriar(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void Formata_conforme_o_tipo()
    {
        CpfCnpj.Criar("52998224725").Formatado().Should().Be("529.982.247-25");
        CpfCnpj.Criar("11222333000181").Formatado().Should().Be("11.222.333/0001-81");
    }
}

[Trait("Categoria", "Dominio")]
public class CepTestes
{
    [Theory]
    [InlineData("78455-000", "78455000")]
    [InlineData("78455000", "78455000")]
    [InlineData("  78455-000  ", "78455000")]
    public void Aceita_e_normaliza_formatos_reais(string entrada, string esperado)
    {
        Cep.Criar(entrada).Numero.Should().Be(esperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567")]      // 7 dígitos
    [InlineData("123456789")]    // 9 dígitos
    [InlineData("00000000")]     // sequência de erro de preenchimento
    [InlineData("11111111")]
    [InlineData("abcde-123")]
    public void Recusa_entrada_invalida(string entrada)
    {
        Cep.TentarCriar(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void Formata_com_hifen()
    {
        Cep.Criar("78455000").Formatado().Should().Be("78455-000");
    }
}

[Trait("Categoria", "Dominio")]
public class TextoNormalizadoTestes
{
    [Fact]
    public void Colapsa_espaco_duplo()
    {
        // [V] Defeito real: textos livres do Vórtice (ex.: IV_Historico.ResultadoCmpl) não
        // normalizam espaço — cada variação de espaçamento vira uma string "diferente".
        TextoNormalizado.Criar("Cliente  pediu   revisão").Valor.Should().Be("Cliente pediu revisão");
    }

    [Fact]
    public void Remove_espaco_nas_pontas()
    {
        TextoNormalizado.Criar("  Fazenda Santa Fé  ").Valor.Should().Be("Fazenda Santa Fé");
    }

    [Fact]
    public void Remove_caractere_invisivel()
    {
        // Espaço de largura zero (U+200B) no meio do texto: invisível ao olho, mas faz
        // duas strings "iguais" na tela compararem como diferentes.
        var comInvisivel = "Fazenda" + "\u200B" + " Santa Fé"; // literal, via escape, para ficar visível no código-fonte
        TextoNormalizado.Criar(comInvisivel).Valor.Should().Be("Fazenda Santa Fé");
    }

    [Fact]
    public void Converte_nbsp_em_espaco_comum()
    {
        var comNbsp = "Fazenda" + "\u00A0" + "Santa Fé"; // NBSP via escape, para ficar visível no código-fonte
        TextoNormalizado.Criar(comNbsp).Valor.Should().Be("Fazenda Santa Fé");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\u200B\u200B")]
    public void Recusa_vazio_ou_so_invisivel(string entrada)
    {
        TextoNormalizado.TentarCriar(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void Recusa_nulo()
    {
        TextoNormalizado.TentarCriar(null, out _).Should().BeFalse();
    }

    [Fact]
    public void Rejeita_em_vez_de_truncar_em_silencio()
    {
        // [V] IV_Historico.ResultadoCmpl é varchar(150) mas a aplicação trunca em 20 —
        // silenciosamente. Aqui o texto que excede o tamanho máximo é REJEITADO, não cortado.
        TextoNormalizado.TentarCriar("0123456789", out _, tamanhoMaximo: 5).Should().BeFalse();
    }
}

[Trait("Categoria", "Dominio")]
public class DinheiroTestes
{
    [Theory]
    [InlineData(1234.5)]
    [InlineData(1234.56)]
    [InlineData(0)]
    public void Aceita_ate_duas_casas(decimal entrada)
    {
        Dinheiro.Criar(entrada).Valor.Should().Be(entrada);
    }

    [Fact]
    public void Recusa_mais_de_duas_casas()
    {
        // [V] o documento 04 proíbe float/money no banco pelo arredondamento silencioso;
        // aqui o mesmo cuidado vale na entrada — 1234,567 não é "quase 1234,57", é dado
        // corrompido, e corrigir em silêncio é o mesmo erro do Vórtice (documento 01, 3.8).
        Dinheiro.TentarCriar(1234.567m, out _).Should().BeFalse();
        var acao = () => Dinheiro.Criar(1234.567m);
        acao.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Recusa_valor_negativo()
    {
        Dinheiro.TentarCriar(-10m, out _).Should().BeFalse();
    }
}

[Trait("Categoria", "Dominio")]
public class ChassiTestes
{
    [Theory]
    [InlineData("1RW7250PVMR123456", "1RW7250PVMR123456")]
    [InlineData("1rw7250pvmr123456", "1RW7250PVMR123456")]
    [InlineData(" 1RW7250PVMR123456 ", "1RW7250PVMR123456")]
    public void Aceita_e_normaliza(string entrada, string esperado)
    {
        Chassi.Criar(entrada).Numero.Should().Be(esperado);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1RW7250PVMR12345")]     // 16 caracteres
    [InlineData("1RW7250PVMR1234567")]   // 18 caracteres
    [InlineData("1RW7250PVMR-23456")]    // símbolo
    [InlineData("1RW7250PVMR_23456")]    // sublinhado
    [InlineData("1RW7250PVMRÉ23456")]    // letra fora do ASCII
    public void Recusa_entrada_invalida(string entrada)
    {
        // [V] EXT_Veic está morto e o cadastro vivo de equipamento não valida chassi como
        // formato — aqui a forma fixa é o que permite usar o chassi como chave natural.
        Chassi.TentarCriar(entrada, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("1CQ7250PVMR123456")]    // o prefixo da John Deere (decisão 4 de 24/09/2026)
    [InlineData("1RW7250PVMRI23456")]
    [InlineData("1RW7250PVMRO23456")]
    [InlineData("1rw7250pvmrq23456")]
    public void Aceita_I_O_e_Q_porque_a_plaqueta_real_tem(string entrada)
    {
        // O padrão VIN internacional proíbe as três letras, mas o chassi é o que a plaqueta diz: 1.211 máquinas John
        // Deere do Protheus começam em 1CQ. A letra é guardada como está — nunca trocada por 1 ou 0.
        var chassi = Chassi.Criar(entrada);

        chassi.Numero.Should().Be(entrada.ToUpperInvariant());
        chassi.EhVin.Should().BeTrue();
    }

    [Fact]
    public void Todo_VIN_valido_pela_regra_antiga_continua_valido_e_com_a_mesma_chave()
    {
        // O PEDIDO DA SESSÃO DO ART (24/09/2026): mudar a regra não pode reescrever chave de máquina já gravada — se
        // reescrevesse, seria migração de dados, e a recarga duplicaria a máquina. Toda forma aceita antes sai idêntica.
        foreach (var antigo in new[] { "1RW7250PVMR123456", "9ZXY876WVUT543210", "1ABCD23EFGH456789", "  1rw7250pvmr 123456 " })
        {
            Chassi.TentarCriar(antigo, out var chassi).Should().BeTrue();
            chassi.Numero.Should().Be(Chassi.Normalizar(antigo));
            Chassi.Restaurar(chassi.Numero).Should().Be(chassi);
        }
    }

    [Theory]
    [InlineData("12345", "12345")]
    [InlineData(" 0012 3456 ", "00123456")]
    [InlineData("ch570-12345", "CH570-12345")]
    [InlineData("PY6110J/0123", "PY6110J/0123")]
    [InlineData("1BM6110JCHD1", "1BM6110JCHD1")]
    public void O_identificador_curto_confirmado_tem_forma_propria(string entrada, string esperado)
    {
        Chassi.TentarCriarIdentificadorConfirmado(entrada, out var chassi).Should().BeTrue();

        chassi.Numero.Should().Be(esperado);
        chassi.EhVin.Should().BeFalse();
        Chassi.Restaurar(chassi.Numero).Should().Be(chassi, "o banco devolve o que gravou");
    }

    [Theory]
    [InlineData("1234")]                 // curto demais — número de quatro dígitos se repete entre marcas
    [InlineData("SEMCHASSI")]            // sem dígito: é texto, não número de série
    [InlineData("-12345")]               // começa com símbolo
    [InlineData("12345/")]               // termina com símbolo
    [InlineData("123\"45")]              // aspas não são de número de série
    [InlineData("1RW7250PVMR123456")]    // 17 é VIN, não identificador curto
    public void O_identificador_curto_recusa_o_que_nao_e_numero_de_serie(string entrada) =>
        Chassi.TentarCriarIdentificadorConfirmado(entrada, out _).Should().BeFalse();

    [Fact]
    public void O_identificador_curto_nao_entra_pela_porta_da_tela()
    {
        // A TELA E AS IMPORTAÇÕES usam TentarCriar: o número curto só vira identidade com a confirmação do Protheus.
        Chassi.TentarCriar("12345678", out _).Should().BeFalse();
        var recusa = () => Chassi.Restaurar("12-");
        recusa.Should().Throw<RegraDeNegocioViolada>();
    }
}

[Trait("Categoria", "Dominio")]
public class CoordenadaTestes
{
    [Fact]
    public void Aceita_coordenada_real_do_brasil()
    {
        var coordenada = Coordenada.Criar(-13.0489, -55.9142);   // Lucas do Rio Verde/MT
        coordenada.Latitude.Should().Be(-13.0489);
        coordenada.Longitude.Should().Be(-55.9142);
    }

    [Theory]
    [InlineData(91, -55)]              // latitude matematicamente impossível
    [InlineData(-13, -181)]            // longitude matematicamente impossível
    [InlineData(40.7128, -74.0060)]    // Nova York — fora da área de atuação da Tracbel
    [InlineData(-13.05, 55.91)]        // sinal de longitude trocado (cairia no Índico)
    public void Recusa_fora_da_faixa_plausivel(double latitude, double longitude)
    {
        Coordenada.TentarCriar(latitude, longitude, out _).Should().BeFalse();
    }
}

[Trait("Categoria", "Dominio")]
public class DataHoraUtcTestes
{
    [Fact]
    public void Aceita_data_plausivel_e_marca_como_utc()
    {
        var data = DataHoraUtc.Criar(new DateTime(2026, 8, 22, 10, 15, 0, DateTimeKind.Unspecified));
        data.Valor.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData(5173, 8, 20)]   // [V] a maior data já encontrada no banco do Vórtice (documento 10)
    [InlineData(2223, 8, 10)]   // [V] EXT_Titulo.DtaVencto entre títulos em aberto (documento 01, 3.8)
    [InlineData(5024, 5, 8)]    // [V] EXT_Titulo.DtaVencto, o maior valor da tabela (documento 01, 3.8)
    [InlineData(1, 1, 1)]       // ano 1 -- claramente corrompido
    public void Recusa_data_fora_da_faixa_plausivel(int ano, int mes, int dia)
    {
        var data = new DateTime(ano, mes, dia);
        DataHoraUtc.TentarCriar(data, out _).Should().BeFalse();
        var acao = () => DataHoraUtc.Criar(data);
        acao.Should().Throw<RegraDeNegocioViolada>();
    }
}
