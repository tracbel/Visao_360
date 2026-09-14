"""Planilhas-modelo para coletar os dados que faltam ao território e à Visão 360 (documento 34).

Uso:
    python scripts/coleta/gerar-modelos-de-coleta.py [--entrada dados-locais/coleta/entrada] [--saida dados-locais/coleta]

- Sem --entrada (ou sem os CSVs), gera só os MODELOS, com exemplos fictícios. Não têm dado da empresa.
- Com os CSVs exportados por extrair-pre-preenchimento.ps1, gera também a versão PRÉ-PREENCHIDA com o
  que o CRM e a extração do Vórtice já têm — para pedir confirmação, e não o dado de novo. Essa versão
  tem nome de funcionário e número real: vai só para dados-locais/, que o Git ignora.

Toda planilha tem as abas: Instruções, Dados, Dicionário e Listas.
"""
import argparse
import csv
import os
import sys
from datetime import date

from openpyxl import Workbook
from openpyxl.styles import Alignment, Font, PatternFill
from openpyxl.utils import get_column_letter
from openpyxl.worksheet.datavalidation import DataValidation

sys.stdout.reconfigure(encoding='utf-8')
RAIZ = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))

CABECALHO = PatternFill('solid', fgColor='1F5E3B')
EXEMPLO = PatternFill('solid', fgColor='FFF4D6')
PRE = PatternFill('solid', fgColor='E8F1FB')
PEDIDO = PatternFill('solid', fgColor='FFFFFF')

# (campo, obrigatório, formato, exemplo fictício, de onde vem, lista de valores ou None)
MODELOS = {
    '01-usuarios-cargos-e-gestores': {
        'titulo': 'Usuários do comercial: cargo, gestor e alcance',
        'para_que': 'Montar as visões de CEN, gerente e diretor e decidir quem recebe a visão da empresa inteira (documento 32, P-10).',
        'ja_existe': 'O CRM já tem login, nome e filial de casa de 263 pessoas. Na extração do Vórtice, 186 têm supervisor e 233 têm gerente; '
                     'desses, 107 supervisores e 135 gerentes casam com um login do CRM e vêm como SUGESTÃO para confirmar.',
        'campos': [
            ('Login', 'sim', 'e-mail corporativo', 'fulano.exemplo@tracbel.com.br', 'CRM (pré-preenchido)', None),
            ('Nome', 'sim', 'texto', 'Fulano Exemplo', 'CRM (pré-preenchido)', None),
            ('FilialDeCasa', 'sim', 'código 0101NN', '010101', 'CRM (pré-preenchido)', 'filiais'),
            ('SupervisorSugeridoPeloVortice', 'não', 'login', 'beltrano.exemplo@tracbel.com.br', 'Vórtice (sugestão)', None),
            ('GerenteSugeridoPeloVortice', 'não', 'login', 'ciclano.exemplo@tracbel.com.br', 'Vórtice (sugestão)', None),
            ('Cargo', 'sim', 'lista', 'CEN', 'A INFORMAR', 'cargos'),
            ('GestorImediato', 'sim', 'login', 'beltrano.exemplo@tracbel.com.br', 'A INFORMAR (confirme ou corrija a sugestão)', None),
            ('LojasQueAtende', 'não', 'códigos separados por ;', '010101;010103', 'A INFORMAR', None),
            ('RecebeVisaoDaEmpresa', 'sim', 'Sim/Não', 'Não', 'A DECIDIR (diretoria)', 'simnao'),
            ('AtivoDesde', 'não', 'dd/mm/aaaa', '01/03/2024', 'A INFORMAR', None),
            ('Observacao', 'não', 'texto', '', 'livre', None),
        ],
    },
    '02-municipio-cen-e-gestor-vigentes': {
        'titulo': 'CEN e gestor vigentes por município da ADR',
        'para_que': 'Resolver as divergências entre as duas planilhas (82 municípios com CEN diferente e 46 com grafia diferente) e dar vigência (P-1).',
        'ja_existe': 'Os 203 municípios, a região, a loja e o que CADA planilha diz vêm pré-preenchidos, com a comparação. Só falta dizer qual vale e desde quando.',
        'campos': [
            ('CodigoIbge', 'sim', '7 dígitos', '3500000', 'IBGE (pré-preenchido)', None),
            ('Municipio', 'sim', 'texto', 'Município Exemplo', 'IBGE (pré-preenchido)', None),
            ('Regiao', 'sim', 'Norte/Noroeste', 'Norte', 'planilha de área de atuação (pré-preenchido)', None),
            ('Loja', 'sim', 'código 0101NN', '010101', 'planilha de área de atuação (pré-preenchido)', 'filiais'),
            ('CenNaPlanilhaAreaDeAtuacao', 'não', 'texto', 'FULANO.EXEMPLO', 'planilha (pré-preenchido)', None),
            ('CenNaPlanilhaCenEGestor', 'não', 'texto', 'Fulano Exemplo', 'planilha (pré-preenchido)', None),
            ('GestorNaPlanilhaCenEGestor', 'não', 'texto', 'Beltrano Exemplo', 'planilha (pré-preenchido)', None),
            ('Comparacao', 'não', 'lista', 'ProvavelMesmaPessoa', 'CRM (pré-preenchido)', None),
            ('CenVigente', 'sim', 'login', 'fulano.exemplo@tracbel.com.br', 'A INFORMAR', None),
            ('GestorVigente', 'sim', 'login', 'beltrano.exemplo@tracbel.com.br', 'A INFORMAR', None),
            ('VigenteDesde', 'sim', 'dd/mm/aaaa', '01/01/2026', 'A INFORMAR', None),
            ('Observacao', 'não', 'texto', '', 'livre', None),
        ],
    },
    '03-classificacao-sam-kam-varejo': {
        'titulo': 'Classificação de cliente: SAM, KAM e Varejo',
        'para_que': 'Ligar o filtro de tipo de cliente (P-6). Nenhuma fonte carregada classifica cliente.',
        'ja_existe': 'Não existe classificação por cliente. No Vórtice, SAM e KAM aparecem só como resposta de formulário por venda direta — não serve de cadastro.',
        'campos': [
            ('DocumentoDoCliente', 'sim', 'CPF ou CNPJ, só dígitos', '11111111000111', 'A INFORMAR', None),
            ('NomeDoCliente', 'não', 'texto', 'Agropecuária Exemplo Ltda', 'conferência', None),
            ('TipoDeCliente', 'sim', 'lista', 'KAM', 'A INFORMAR', 'tipos_de_cliente'),
            ('CriterioAplicado', 'sim', 'texto curto', 'faturamento anual acima do limite do KAM', 'A INFORMAR', None),
            ('VigenteDesde', 'sim', 'dd/mm/aaaa', '01/01/2026', 'A INFORMAR', None),
            ('ResponsavelPelaConta', 'não', 'login', 'fulano.exemplo@tracbel.com.br', 'A INFORMAR', None),
        ],
    },
    '04-metas': {
        'titulo': 'Metas por filial, linha e mês',
        'para_que': 'Previsão FY 2026 da Visão 360 ("ART ou Arquivo de Meta").',
        'ja_existe': 'A tabela de metas do CRM está vazia, e o Vórtice tem 1 linha de meta. Não há o que pré-preencher.',
        'campos': [
            ('AnoFiscal', 'sim', 'aaaa', '2026', 'A INFORMAR', None),
            ('InicioDoAnoFiscal', 'sim', 'dd/mm/aaaa', '01/11/2025', 'A INFORMAR (decisão P-4)', None),
            ('Filial', 'sim', 'código 0101NN', '010101', 'A INFORMAR', 'filiais'),
            ('LinhaDeNegocio', 'sim', 'código', 'MAQ_NOVOS', 'A INFORMAR', 'linhas'),
            ('Responsavel', 'não', 'login do CEN (vazio = filial)', 'fulano.exemplo@tracbel.com.br', 'A INFORMAR', None),
            ('Mes', 'sim', 'aaaa-mm', '2026-03', 'A INFORMAR', None),
            ('TipoDeMeta', 'sim', 'lista', 'Valor', 'A INFORMAR', 'tipos_de_meta'),
            ('Alvo', 'sim', 'número', '1500000', 'A INFORMAR', None),
        ],
    },
    '05-propriedades-e-culturas': {
        'titulo': 'Propriedades rurais, áreas e culturas (clientes e não clientes)',
        'para_que': 'Potencial de cliente e de não cliente (P-11). A área do IBGE é regional e não substitui este dado.',
        'ja_existe': 'Nenhum endereço de cliente tem área ou cultura (0 de 19.657). A fonte esperada é o ART, sem acesso desta rede. Prefira a EXPORTAÇÃO do ART a digitar.',
        'campos': [
            ('DocumentoDoProprietario', 'sim', 'CPF ou CNPJ, só dígitos', '11111111000111', 'ART ou A INFORMAR', None),
            ('EhClienteTracbel', 'sim', 'Sim/Não', 'Sim', 'A INFORMAR', 'simnao'),
            ('NomeDaPropriedade', 'não', 'texto', 'Fazenda Exemplo', 'ART ou A INFORMAR', None),
            ('CodigoIbgeDoMunicipio', 'sim', '7 dígitos', '3500000', 'ART ou A INFORMAR', None),
            ('AreaTotalHa', 'sim', 'número, hectares', '1500', 'ART ou A INFORMAR', None),
            ('AreaAgricultavelHa', 'sim', 'número ≤ área total', '1400', 'ART ou A INFORMAR', None),
            ('Cultura', 'sim', 'lista (código IBGE)', '40139', 'ART ou A INFORMAR', 'culturas'),
            ('AreaDaCulturaHa', 'sim', 'número', '900', 'ART ou A INFORMAR', None),
            ('Safra', 'sim', 'aaaa/aa', '2025/26', 'ART ou A INFORMAR', None),
            ('Latitude', 'não', 'graus decimais', '-20.1234567', 'ART ou A INFORMAR', None),
            ('Longitude', 'não', 'graus decimais', '-48.1234567', 'ART ou A INFORMAR', None),
        ],
    },
    '06-parque-de-maquinas': {
        'titulo': 'Parque de máquinas: ano, horímetro e entrega técnica',
        'para_que': 'Ciclo de troca e potencial ajustado (P-9).',
        'ja_existe': 'O CRM tem 3.888 máquinas com chassi, modelo e cliente, e nenhuma com ano, horímetro ou data de venda. Chassi e modelo vêm pré-preenchidos.',
        'campos': [
            ('Chassi', 'sim', '17 posições', '1BM5078EXAA000001', 'CRM (pré-preenchido)', None),
            ('Modelo', 'sim', 'texto', '5078E', 'CRM (pré-preenchido)', None),
            ('Filial', 'sim', 'código 0101NN', '010101', 'CRM (pré-preenchido)', 'filiais'),
            ('AnoDeFabricacao', 'sim', 'aaaa', '2019', 'ART / Operation Center ou A INFORMAR', None),
            ('HorimetroHoras', 'não', 'número', '5737', 'ART / Operation Center ou A INFORMAR', None),
            ('HorimetroLidoEm', 'não', 'dd/mm/aaaa', '15/08/2026', 'ART / Operation Center ou A INFORMAR', None),
            ('OrigemDaLeitura', 'não', 'lista', 'Operation Center', 'A INFORMAR', 'origens_horimetro'),
            ('DataDaEntregaTecnica', 'não', 'dd/mm/aaaa', '10/03/2019', 'A INFORMAR', None),
        ],
    },
    '07-taxonomia-de-produto': {
        'titulo': 'Taxonomia de produto: linha, série, porte e potência',
        'para_que': 'Filtros de tipo de produto e modelo (P-7) e regra de potencial por modelo.',
        'ja_existe': 'O CRM tem 253 modelos em 22 famílias e 13 marcas, nenhum com potência. A lista de modelos vem pré-preenchida para classificar.',
        'campos': [
            ('Marca', 'sim', 'texto', 'John Deere', 'CRM (pré-preenchido)', None),
            ('Familia', 'sim', 'texto', 'Trator', 'CRM (pré-preenchido)', None),
            ('Modelo', 'sim', 'texto', '5078E', 'CRM (pré-preenchido)', None),
            ('MaquinasNoParque', 'não', 'número', '12', 'CRM (pré-preenchido)', None),
            ('LinhaDeProduto', 'sim', 'lista', 'Tratores médios', 'A INFORMAR', 'linhas_de_produto'),
            ('Serie', 'não', 'texto', 'Série 5E', 'A INFORMAR', None),
            ('Porte', 'sim', 'lista', 'Médio', 'A INFORMAR', 'portes'),
            ('PotenciaCv', 'não', 'número', '78', 'A INFORMAR', None),
            ('ModeloEquivalenteDe', 'não', 'texto (quando o nome está sujo)', '5078E', 'A INFORMAR', None),
        ],
    },
    '08-regras-de-potencial': {
        'titulo': 'Regras de potencial por cultura',
        'para_que': 'Mapa de potencial (P-8). Hoje há uma única regra, informada como exemplo e não confirmada.',
        'ja_existe': 'Pré-preenchidas a regra do café (1 3036N a cada 10 ha, a confirmar) e as culturas de maior área plantada na ADR (IBGE), para cada uma receber a sua regra.',
        'campos': [
            ('CulturaCodigoIbge', 'sim', 'código IBGE', '40139', 'IBGE (pré-preenchido)', 'culturas'),
            ('Cultura', 'sim', 'texto', 'Café (em grão) Total', 'IBGE (pré-preenchido)', None),
            ('HectaresNaAdr', 'não', 'número', '52000', 'IBGE (pré-preenchido)', None),
            ('ModeloDeReferencia', 'sim', 'texto', '3036N', 'A INFORMAR', None),
            ('HectaresPorMaquina', 'sim', 'número > 0', '10', 'A INFORMAR', None),
            ('HorizonteEmAnos', 'não', 'número', '7', 'A INFORMAR', None),
            ('Arredondamento', 'não', 'lista', 'Para baixo', 'A INFORMAR', 'arredondamentos'),
            ('PrecoDeReferencia', 'não', 'R$', '350000', 'A INFORMAR', None),
            ('VigenteDesde', 'sim', 'dd/mm/aaaa', '01/01/2026', 'A INFORMAR', None),
            ('Confirmada', 'sim', 'Sim/Não', 'Não', 'A DECIDIR', 'simnao'),
        ],
    },
    '09-ciclo-de-troca': {
        'titulo': 'Ciclo de troca por categoria de máquina',
        'para_que': 'Potencial ajustado por idade do parque (P-9).',
        'ja_existe': 'Nenhuma regra de vida útil existe. A maquete cita 7 a 10 anos, sem fonte.',
        'campos': [
            ('Categoria', 'sim', 'lista', 'Trator médio', 'A INFORMAR', 'portes'),
            ('VidaUtilAnos', 'não', 'número', '8', 'A INFORMAR', None),
            ('VidaUtilHoras', 'não', 'número', '10000', 'A INFORMAR', None),
            ('Fonte', 'sim', 'texto', 'critério interno do comercial', 'A INFORMAR', None),
        ],
    },
    '10-definicao-de-visita': {
        'titulo': 'O que conta como visita, e a periodicidade',
        'para_que': 'Mapa de pendência de visita (P-2 e P-3). Hoje "visita" é qualquer interação registrada.',
        'ja_existe': 'Os tipos de tarefa com uso em 2026 vêm pré-preenchidos, com o total e quantas interações têm coordenada. Nenhum está marcado como '
                     '"conta para cobertura" no CRM. A cadência declarada por linha também vem pré-preenchida.',
        'campos': [
            ('TipoDeTarefa', 'sim', 'código', 'VISITA_CAMPO_EXEMPLO', 'CRM (pré-preenchido)', None),
            ('Nome', 'sim', 'texto', 'Visita a campo', 'CRM (pré-preenchido)', None),
            ('Interacoes2026', 'não', 'número', '45', 'CRM (pré-preenchido)', None),
            ('ComCoordenada', 'não', 'número', '40', 'CRM (pré-preenchido)', None),
            ('ContaComoVisita', 'sim', 'Sim/Não', 'Sim', 'A DECIDIR', 'simnao'),
            ('ExigeCoordenada', 'não', 'Sim/Não', 'Sim', 'A DECIDIR', 'simnao'),
            ('Observacao', 'não', 'texto', '', 'livre', None),
        ],
    },
    '11-composicao-do-pos-venda': {
        'titulo': 'O que compõe pós-venda',
        'para_que': 'Valor de pós-venda do painel. Hoje é peça + serviço pelo grupo do item da nota — composição provisória.',
        'ja_existe': 'A classificação do item da nota (máquina, peça, serviço, outros) já existe. As parcelas abaixo são PROPOSTA de recorte para marcar.',
        'campos': [
            ('Componente', 'sim', 'texto', 'Peça balcão', 'PROPOSTA (pré-preenchido)', None),
            ('EntraNoPosVenda', 'sim', 'Sim/Não', 'Sim', 'A DECIDIR', 'simnao'),
            ('Observacao', 'não', 'texto', '', 'livre', None),
        ],
    },
}

LISTAS_FIXAS = {
    'cargos': ['CEN', 'Gerente de loja', 'Gerente regional', 'Diretor', 'Pós-venda', 'Outro'],
    'simnao': ['Sim', 'Não'],
    'tipos_de_cliente': ['SAM', 'KAM', 'Varejo'],
    'tipos_de_meta': ['Valor', 'Quantidade de máquinas', 'Quantidade de visitas'],
    'origens_horimetro': ['Operation Center', 'Manualmente', 'Simova', 'Oficina'],
    'linhas_de_produto': ['Tratores compactos', 'Tratores médios', 'Tratores grandes', 'Colhedoras', 'Pulverizadores', 'Plantadeiras', 'Implementos', 'Agricultura de precisão'],
    'portes': ['Compacto', 'Médio', 'Grande', 'Colhedora', 'Pulverizador', 'Implemento'],
    'arredondamentos': ['Para baixo', 'Para cima', 'Mais próximo'],
    'componentes_pos_venda': ['Peça balcão', 'Peça oficina', 'Serviço de oficina', 'Mão de obra externa', 'AMS', 'Agricultura de precisão (PUK)', 'Garantia', 'Pneus', 'Seguro', 'Consórcio'],
}


# AS DECISÕES PARA O GERENTE, em ordem de prioridade (documento 34, seção 3). Mesma lista do documento.
# (prioridade, pergunta, opções, impacto, funcionalidades bloqueadas, o que o sistema já tem, planilha)
DECISOES = [
    ('1 · Alta', 'Qual das duas planilhas de CEN vale hoje, e desde quando?',
     'a) Área de Atuação · b) CEN e Gestor por Município · c) uma planilha nova e datada (modelo 02)',
     'Define o CEN e o gestor de cada um dos 203 municípios; hoje 82 têm CEN diferente e 46 têm grafia diferente.',
     'Filtro por CEN/gestor; responsável no detalhe do município; visão do CEN.',
     'As duas planilhas carregadas, lado a lado, com a comparação (modelo 02 pré-preenchido).', '02'),
    ('2 · Alta', 'O que conta como visita para a cobertura da carteira?',
     'a) só tipos de tarefa de visita (2 tipos, 47 interações em 2026) · b) interação com coordenada (29,5 mil de 122 mil) · '
     'c) resultados marcados como externos no Vórtice (22) · d) qualquer interação (regra provisória de hoje)',
     'Muda o numerador e o denominador do mapa de pendência de visita.',
     'Validação comercial do mapa A; meta de visita.',
     'Tipos de tarefa com uso em 2026 e contagem com coordenada (modelo 10 pré-preenchido).', '10'),
    ('3 · Alta', 'Qual periodicidade de visita vale, e ela se mede por cliente ou por vínculo com a carteira?',
     'a) a declarada no CRM (máquinas 180/180/180/360 dias; prospecção 120/120/120/180; peças e AMS 360) · '
     'b) 30/60/90/120 da maquete · c) o ciclo de visita por potencial do Vórtice',
     'Define quem está "fora da cadência".', 'Mapa A; alertas de visita vencida.',
     'Cadência declarada por linha e classe no CRM.', '10'),
    ('4 · Alta', 'Quem recebe a visão da empresa inteira, e quais são os cargos (CEN, gerente, diretor)?',
     'a) só diretoria · b) diretoria e gerência regional · c) outra regra', 'Libera a visão consolidada para usuários reais.',
     'Visão da empresa para qualquer pessoa (hoje só perfil de teste); visões por cargo.',
     '263 usuários com login e filial; sugestão de supervisor e gerente para parte deles vinda do Vórtice (modelo 01).', '01'),
    ('5 · Alta', 'O mapa de vendas usa o ano fiscal até a data (FYTD) ou os 12 meses fechados? Se FYTD, quando começa o ano fiscal?',
     'a) 12 meses fechados (hoje) · b) FYTD, com a data de início', 'Muda o período de todos os valores de venda.',
     'Leitura oficial dos valores do mapa B.', 'O filtro de período já existe.', '—'),
    ('6 · Alta', 'Devolução e cancelamento devem ser abatidos das vendas?',
     'a) sim, pela nota de entrada do Protheus · b) não', 'Reduz os valores de venda e de pós-venda.',
     'Valor líquido "comercial" das vendas.', 'As notas de saída já são lidas; a de entrada não.', '—'),
    ('7 · Alta', 'A regra "1 trator 3036N a cada 10 ha de café" vale? E quais são as regras das outras culturas?',
     'Confirmar ou corrigir o café; informar cana, soja, laranja, amendoim e milho', 'Transforma a estimativa regional em potencial com regra aprovada.',
     'Mapa C com regra confirmada; potencial em valor.', 'Área plantada do IBGE por município e cultura (modelo 08 pré-preenchido).', '08'),
    ('8 · Média', 'O que a diretoria considera pós-venda?', 'Marcar os componentes (modelo 11)',
     'Muda o valor de pós-venda do painel.', 'Pós-venda com composição aprovada.', 'Peça e serviço já separados no item da nota.', '11'),
    ('9 · Média', 'Como se define SAM, KAM e Varejo, e quais clientes são de cada tipo?', 'Critério e lista (modelo 03)',
     'Liga o filtro de tipo de cliente.', 'Filtro SAM/KAM/Varejo.', 'Nenhuma classificação por cliente nas fontes.', '03'),
    ('10 · Média', 'Como o comercial agrupa os produtos (linha, série, porte)?', 'Classificar os modelos (modelo 07)',
     'Liga os filtros de tipo de produto e modelo (com o item da nota).', 'Filtros de produto e modelo.', '253 modelos em 22 famílias (modelo 07 pré-preenchido).', '07'),
    ('11 · Média', 'A quem pertence o cliente que compra em mais de uma filial?',
     'a) à filial do cadastro (hoje) · b) à que mais vende para ele · c) às duas, com divisão', '1.706 clientes; R$ 321,6 mi em 12 meses com filial de cadastro diferente da que vendeu.',
     'Metas e carteira por filial.', 'A conciliação por filial já está pronta (documento 32, seção 8.6).', '—'),
    ('12 · Média', 'Guaíra, Ituverava e Monte Alto operam?', 'a) sim — reativar no CRM · b) não — definir quem atende os municípios',
     '11 municípios da ADR; as três filiais emitiram R$ 17,4 mi com cliente e R$ 4,7 mi sem cliente em 12 meses.', 'Visão de filial dessas lojas.', 'O faturamento delas já é lido.', '—'),
    ('13 · Média', 'O que são os 35 municípios fora da ADR na planilha de área de atuação?', 'a) prospecção · b) histórico · c) erro — remover',
     'Define se entram em algum mapa.', 'Tratamento desses municípios.', 'Estão carregados, marcados como fora da ADR.', '—'),
    ('14 · Média', 'Quem cadastra as contrapartes que compram e não têm cliente no CRM?', 'Responsável e prazo',
     'R$ 230,7 mi em 12 meses (13 filiais ativas) ficam fora dos municípios.', 'Vendas por município completas.', 'A lista por documento já existe no CRM.', '—'),
    ('15 · Baixa', 'Quem confere os 2 endereços cuja coordenada contradiz o município?', 'Responsável do cadastro',
     '2 clientes fora do mapa.', '—', 'Os dois estão na fila de revisão, com o motivo.', '—'),
    ('16 · Baixa', 'Qual o ciclo de troca por categoria, e há fonte de parque concorrente além das vendas perdidas?', 'Modelo 09',
     'Potencial ajustado por idade do parque.', 'Potencial ajustado.', '165 vendas perdidas com concorrente, 163 com modelo e 156 com preço.', '09'),
    ('17 · Baixa', 'De onde vêm as metas: ART ou arquivo?', 'a) ART · b) arquivo mensal (modelo 04)', 'Previsão FY 2026.', 'Cartão de previsão.', 'Metas vazias no CRM e no Vórtice.', '04'),
]


def escrever_decisoes(destino):
    livro = Workbook()
    aba = livro.active
    aba.title = 'Decisões'
    titulos = ['Prioridade', 'Pergunta', 'Opções', 'Impacto', 'Funcionalidades bloqueadas', 'O que o sistema já tem', 'Planilha', 'Resposta', 'Quem decidiu', 'Data']
    for coluna, titulo in enumerate(titulos, 1):
        celula = aba.cell(row=1, column=coluna, value=titulo)
        celula.font = Font(bold=True, color='FFFFFF')
        celula.fill = CABECALHO
    for r, decisao in enumerate(DECISOES, 2):
        for coluna, valor in enumerate(decisao, 1):
            aba.cell(row=r, column=coluna, value=valor).alignment = Alignment(wrap_text=True, vertical='top')
    for letra, largura in zip('ABCDEFGHIJ', (11, 44, 48, 42, 34, 40, 9, 34, 16, 12)):
        aba.column_dimensions[letra].width = largura
    aba.freeze_panes = 'C2'
    livro.save(destino)


def ler_csv(pasta, nome):
    caminho = os.path.join(pasta, nome) if pasta else ''
    if not pasta or not os.path.exists(caminho):
        return None
    with open(caminho, encoding='utf-8-sig', newline='') as f:
        return list(csv.DictReader(f, delimiter=';'))


def listas(entrada):
    resultado = dict(LISTAS_FIXAS)
    filiais = ler_csv(entrada, 'filiais.csv')
    resultado['filiais'] = [f"{f['Codigo']}" for f in filiais if f['EstaAtiva'] in ('1', 'True')] if filiais else ['010101', '010102', '010103']
    linhas = ler_csv(entrada, 'linhas-de-negocio.csv')
    resultado['linhas'] = [l['Codigo'] for l in linhas] if linhas else ['MAQ_NOVOS', 'MAQ_PECAS', 'MAQ_AMS', 'PROSP_MAQ']
    culturas = ler_csv(entrada, 'culturas-na-adr.csv')
    resultado['culturas'] = [c['ProdutoCodigoIbge'] for c in culturas] if culturas else ['40139']
    return resultado


def escrever(modelo_id, modelo, linhas_de_dados, listas_de_valores, destino, pre_preenchido):
    livro = Workbook()

    instrucoes = livro.active
    instrucoes.title = 'Instruções'
    texto = [
        (modelo['titulo'], Font(bold=True, size=14)),
        ('', None),
        (f"Para que serve: {modelo['para_que']}", None),
        (f"O que o sistema já tem: {modelo['ja_existe']}", None),
        ('', None),
        ('Como preencher:', Font(bold=True)),
        ('1. Preencha só as colunas marcadas como "A INFORMAR" ou "A DECIDIR" na aba Dicionário. As azuis vêm do sistema: corrija só se estiverem erradas, e diga na Observação.', None),
        ('2. Use os valores da aba Listas quando a coluna tiver lista. Não crie valor novo — se faltar, escreva na Observação.', None),
        ('3. Linhas amarelas são EXEMPLO FICTÍCIO: apague antes de devolver.', None),
        ('4. Datas no formato dd/mm/aaaa; números sem separador de milhar; documentos só com dígitos.', None),
        ('5. Não mande senha, dado bancário ou documento de pessoa que não precise estar aqui.', None),
        ('', None),
        (f"Gerado em {date.today():%d/%m/%Y} · documento 34 do projeto · {'versão PRÉ-PREENCHIDA (tem dado da empresa: não circule fora da Tracbel)' if pre_preenchido else 'modelo com exemplos fictícios'}", Font(italic=True, color='666666')),
    ]
    for i, (linha, fonte) in enumerate(texto, 1):
        celula = instrucoes.cell(row=i, column=1, value=linha)
        celula.alignment = Alignment(wrap_text=True, vertical='top')
        if fonte:
            celula.font = fonte
    instrucoes.column_dimensions['A'].width = 140

    dados = livro.create_sheet('Dados')
    for coluna, (campo, obrigatorio, formato, exemplo, origem, lista) in enumerate(modelo['campos'], 1):
        celula = dados.cell(row=1, column=coluna, value=campo)
        celula.font = Font(bold=True, color='FFFFFF')
        celula.fill = CABECALHO
        celula.alignment = Alignment(wrap_text=True)
        dados.column_dimensions[get_column_letter(coluna)].width = max(14, min(42, len(campo) + 6))

    if linhas_de_dados:
        for r, valores in enumerate(linhas_de_dados, 2):
            for coluna, (campo, _, _, _, origem, _) in enumerate(modelo['campos'], 1):
                celula = dados.cell(row=r, column=coluna, value=valores.get(campo, ''))
                celula.fill = PRE if 'pré-preenchido' in origem or 'sugestão' in origem else PEDIDO
        primeira_livre = len(linhas_de_dados) + 2
    else:
        for r in (2, 3):
            for coluna, (_, _, _, exemplo, _, _) in enumerate(modelo['campos'], 1):
                celula = dados.cell(row=r, column=coluna, value=exemplo)
                celula.fill = EXEMPLO
        primeira_livre = 4

    ultima = max(primeira_livre + 500, len(linhas_de_dados or []) + 2)
    for coluna, (_, _, _, _, _, lista) in enumerate(modelo['campos'], 1):
        if lista and listas_de_valores.get(lista):
            letra = get_column_letter(coluna)
            validacao = DataValidation(type='list', formula1=f"=Listas!${get_column_letter(list(listas_de_valores).index(lista) + 1)}$2:${get_column_letter(list(listas_de_valores).index(lista) + 1)}${len(listas_de_valores[lista]) + 1}", allow_blank=True)
            validacao.error = 'Use um valor da aba Listas.'
            validacao.showErrorMessage = True
            dados.add_data_validation(validacao)
            validacao.add(f'{letra}2:{letra}{ultima}')
    dados.freeze_panes = 'A2'

    dicionario = livro.create_sheet('Dicionário')
    for coluna, titulo in enumerate(['Campo', 'Obrigatório', 'Formato', 'Exemplo fictício', 'De onde vem / quem responde', 'Lista'], 1):
        celula = dicionario.cell(row=1, column=coluna, value=titulo)
        celula.font = Font(bold=True, color='FFFFFF')
        celula.fill = CABECALHO
    for r, campo in enumerate(modelo['campos'], 2):
        for coluna, valor in enumerate(campo, 1):
            dicionario.cell(row=r, column=coluna, value=valor or '')
    for letra, largura in zip('ABCDEF', (30, 12, 30, 34, 44, 20)):
        dicionario.column_dimensions[letra].width = largura

    lista_aba = livro.create_sheet('Listas')
    for coluna, (nome, valores) in enumerate(listas_de_valores.items(), 1):
        celula = lista_aba.cell(row=1, column=coluna, value=nome)
        celula.font = Font(bold=True)
        for r, valor in enumerate(valores, 2):
            lista_aba.cell(row=r, column=coluna, value=valor)
        lista_aba.column_dimensions[get_column_letter(coluna)].width = 26

    livro.save(destino)


def pre_preenchimento(modelo_id, entrada):
    if modelo_id == '01-usuarios-cargos-e-gestores':
        linhas = ler_csv(entrada, 'usuarios.csv')
        return [{'Login': l['Login'], 'Nome': l['Nome'], 'FilialDeCasa': l['FilialDeCasa'],
                 'SupervisorSugeridoPeloVortice': l.get('SupervisorSugerido', ''), 'GerenteSugeridoPeloVortice': l.get('GerenteSugerido', '')} for l in linhas] if linhas else None
    if modelo_id == '02-municipio-cen-e-gestor-vigentes':
        linhas = ler_csv(entrada, 'municipios-da-adr.csv')
        return [{'CodigoIbge': l['CodigoIbge'], 'Municipio': l['Municipio'], 'Regiao': l['Regiao'], 'Loja': l['Loja'],
                 'CenNaPlanilhaAreaDeAtuacao': l['CenAreaDeAtuacao'], 'CenNaPlanilhaCenEGestor': l['CenCenEGestor'],
                 'GestorNaPlanilhaCenEGestor': l['GestorCenEGestor'], 'Comparacao': l['Comparacao']} for l in linhas] if linhas else None
    if modelo_id == '06-parque-de-maquinas':
        linhas = ler_csv(entrada, 'parque.csv')
        return [{'Chassi': l['Chassi'], 'Modelo': l['Modelo'], 'Filial': l['Filial']} for l in linhas] if linhas else None
    if modelo_id == '07-taxonomia-de-produto':
        linhas = ler_csv(entrada, 'modelos.csv')
        return [{'Marca': l['Marca'], 'Familia': l['Familia'], 'Modelo': l['Modelo'], 'MaquinasNoParque': l['Maquinas']} for l in linhas] if linhas else None
    if modelo_id == '08-regras-de-potencial':
        linhas = ler_csv(entrada, 'culturas-na-adr.csv')
        if not linhas:
            return None
        return [{'CulturaCodigoIbge': l['ProdutoCodigoIbge'], 'Cultura': l['ProdutoNome'], 'HectaresNaAdr': l['HectaresNaAdr'],
                 **({'ModeloDeReferencia': '3036N', 'HectaresPorMaquina': '10', 'Confirmada': 'Não'} if l['ProdutoCodigoIbge'] == '40139' else {})} for l in linhas]
    if modelo_id == '10-definicao-de-visita':
        linhas = ler_csv(entrada, 'tipos-de-tarefa.csv')
        return [{'TipoDeTarefa': l['Codigo'], 'Nome': l['Nome'], 'Interacoes2026': l['Interacoes2026'], 'ComCoordenada': l['ComCoordenada']} for l in linhas] if linhas else None
    if modelo_id == '11-composicao-do-pos-venda':
        return [{'Componente': c} for c in LISTAS_FIXAS['componentes_pos_venda']]
    return None


def main():
    argumentos = argparse.ArgumentParser()
    argumentos.add_argument('--entrada', default=os.path.join(RAIZ, 'dados-locais', 'coleta', 'entrada'))
    argumentos.add_argument('--saida', default=os.path.join(RAIZ, 'dados-locais', 'coleta'))
    a = argumentos.parse_args()

    tem_entrada = os.path.isdir(a.entrada) and any(n.endswith('.csv') for n in os.listdir(a.entrada))
    valores = listas(a.entrada if tem_entrada else None)

    pasta_modelos = os.path.join(a.saida, 'modelos')
    os.makedirs(pasta_modelos, exist_ok=True)
    for modelo_id, modelo in MODELOS.items():
        escrever(modelo_id, modelo, pre_preenchimento(modelo_id, None) if modelo_id == '11-composicao-do-pos-venda' else None,
                 listas(None), os.path.join(pasta_modelos, f'{modelo_id}.xlsx'), pre_preenchido=False)
    escrever_decisoes(os.path.join(pasta_modelos, '00-decisoes-para-o-gerente.xlsx'))
    print(f'modelos (exemplos fictícios): {len(MODELOS)} planilhas + lista de {len(DECISOES)} decisões em {pasta_modelos}')

    if tem_entrada:
        pasta_pre = os.path.join(a.saida, 'pre-preenchidas')
        os.makedirs(pasta_pre, exist_ok=True)
        feitas = 0
        for modelo_id, modelo in MODELOS.items():
            linhas = pre_preenchimento(modelo_id, a.entrada)
            if linhas:
                escrever(modelo_id, modelo, linhas, valores, os.path.join(pasta_pre, f'{modelo_id}.xlsx'), pre_preenchido=True)
                feitas += 1
                print(f'  pré-preenchida: {modelo_id} ({len(linhas)} linhas)')
        print(f'pré-preenchidas (dado da empresa, fora do Git): {feitas} planilhas em {pasta_pre}')


if __name__ == '__main__':
    main()
