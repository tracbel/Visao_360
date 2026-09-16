# 37 — Solicitação de acesso somente leitura a dados do ART

| Campo | Valor |
|---|---|
| Para | Responsável técnico pelo ART (banco MySQL) |
| De | Projeto CRM Tracbel |
| Tipo | Pedido de acesso **somente leitura**, com escopo mínimo |
| Situação | Rascunho pronto para envio — **ainda não enviado** |
| Relacionado | Documento 35, seção 10 (integração das vendas de máquina) |

---

## 1. Contexto

O CRM já lê, em modo somente leitura, a view de vendas de máquina do ART que foi liberada para o
projeto. Com ela o CRM registra a máquina (pelo chassi), a venda e o cliente comprador naquela venda.
Valores, custos, comissões e margens **não são lidos**: a consulta nem seleciona essas colunas.

Quatro funcionalidades do CRM dependem de dados que essa view não traz. Elas estão marcadas como
pendentes e **nada é estimado no lugar desses dados** enquanto eles não estiverem acessíveis.

## 2. O que pedimos

Acesso de **leitura** às tabelas ou views que contenham as informações abaixo. Se o ART já tiver views
de consulta, preferimos elas às tabelas. Se for mais simples, uma view nova com só estas colunas atende.

| # | Informação | Para que o CRM usa | Identificadores necessários |
|---|---|---|---|
| 1 | **Propriedades rurais** e o vínculo de cada propriedade com o cliente (proprietário, arrendatário ou outro papel, se existir) | Mostrar as propriedades do cliente na Visão 360 e ligar a máquina ao lugar onde ela opera | Código da propriedade; CPF/CNPJ do cliente (ou o código do cliente no ART e a tabela que o traduz para CPF/CNPJ); papel do vínculo; município (código IBGE ou nome + UF) |
| 2 | **Área total** da propriedade e **área por cultura** | Calcular o potencial por cliente e por município, hoje estimado só pela área plantada pública do IBGE | Código da propriedade; área total (unidade: hectare?); cultura; área da cultura; safra ou data de referência |
| 3 | **Culturas** (catálogo) | Segmentar clientes por cultura e planejar a oferta por safra | Código e nome da cultura; se é ativa |
| 4 | **Horímetro** e **data da medição** | Programar revisão preventiva e identificar máquina com uso alto, candidata a troca | Chassi (17 caracteres) ou o código da máquina no ART e a tabela que o traduz para chassi; horas; data e hora da medição; origem da leitura (telemetria, revisão, informado pelo cliente), se houver |
| 5 | **Identificadores de ligação** | Relacionar 1–4 às máquinas e aos clientes que o CRM já tem | A tabela de clientes do ART com código e CPF/CNPJ; a tabela de máquinas com código e chassi — só as colunas de identificação |

## 3. O que **não** pedimos

- Escrita, alteração ou exclusão de qualquer dado.
- Acesso ao banco inteiro ou a esquemas que não contenham as informações da seção 2.
- Valores de venda e de compra, custos, impostos, comissões, bônus, lucro, margem ou resultado contábil.
- Dados de funcionários, vendedores ou usuários do ART.
- Senhas, tokens ou qualquer credencial por e-mail ou mensagem: pedimos que a credencial seja entregue pelo canal que a TI usa para isso.

## 4. Condições de acesso que propomos

1. Um usuário próprio para a integração, **só com SELECT** nas tabelas ou views da seção 2.
2. A mesma origem de rede já liberada para a view de vendas.
3. Consultas feitas fora do horário de pico, se houver restrição — nos informem a janela.
4. Nenhuma cópia integral: o CRM lê só as colunas listadas e grava o resultado no próprio banco, com a
   origem e a data de cada leitura registradas.

## 5. Pedimos também

1. **O dicionário de dados** disponível para essas tabelas: significado de cada coluna, domínios dos
   códigos (por exemplo, os valores de situação), unidade das medidas e chaves entre as tabelas.
2. A **frequência de atualização** de cada informação e se há histórico (por exemplo, todas as leituras
   de horímetro ou só a última).
3. Como o ART marca registro **excluído ou cancelado** (exclusão física, marcação lógica ou situação).
4. O **fuso horário** das datas e horas gravadas.

## 6. Observações sobre a view de vendas já liberada

Registramos, só em números agregados, o que a leitura atual encontrou. Não é pedido de correção: são
pontos em que a orientação do responsável pelo ART ajudaria a interpretar o dado.

| Observação | Quantidade (leitura de 14/09/2026) |
|---|---|
| Registros de venda na view | 4.144 |
| Chassi com menos de 17 caracteres (número de série curto de implemento?) | 741 |
| Chassi com 17 ou mais caracteres fora do padrão VIN | 153 |
| Campo de chassi com mais de um chassi (venda em lote?) | 5 |
| Chassi vazio | 3 |
| Data de entrega zerada (`0000-00-00`) | 69 |
| Data de comissão zerada | 4.137 |
| Unidade sem código de unidade (só o nome) | 50 |
| Mesmo chassi em duas vendas (em todos os casos, uma delas na linha USADOS) | 5 chassis |
| Significado dos códigos de situação `P`, `F`, `PU`, `PF` | a confirmar |

## 7. Contato

Responsável pelo pedido no projeto: _preencher antes de enviar_.
