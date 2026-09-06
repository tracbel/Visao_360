-- =============================================================================================
--  SEMENTE DE DADOS DE REFERENCIA — CRM Tracbel
--
--  GERADO a partir de dados-referencia/*.json. Nao edite este arquivo a mao: corrija o JSON de
--  origem e gere de novo. O criterio e o do documento 14, secao 8.3 — dado de referencia nasce
--  de seed versionado no Git, nunca de INSERT manual em ambiente.
--
--  IDEMPOTENTE (documento 14, regra 8.2): rodar duas vezes nao duplica linha nem lanca excecao.
--  Cada bloco e um MERGE pela chave natural (o Codigo estavel), que ATUALIZA o rotulo e a
--  situacao e INSERE o que faltava. Nada e apagado: item de catalogo que sai de uso vira
--  EstaAtivo = 0, porque apagar quebraria toda chave estrangeira que aponta para ele.
--
--  O QUE ESTE ARQUIVO **NAO** SEMEIA: os oito catalogos de sistema (metadado.Catalogo). Eles ja
--  nascem da migracao ModeloInicial, por HasData(CatalogosDeSistema.Todos), porque o Id deles e
--  parte do esquema — as chaves estrangeiras compostas de papel apontam para numeros fixos.
--  Aqui entram os ITENS deles, que sao dado de negocio e tem Id gerado pelo banco.
-- =============================================================================================

-- QUOTED_IDENTIFIER LIGADO EXPLICITAMENTE: o sqlcmd sobe com ele DESLIGADO por padrao, e o
-- MERGE recusa rodar assim em tabela com indice filtrado — que e o caso de quase toda tabela
-- deste modelo. Declarar aqui faz o arquivo funcionar em qualquer cliente, e nao so no que
-- passa a bandeira certa na linha de comando.
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- ---------------------------------------------------------------------------------------------
--  organizacao.Empresa — AS FILIAIS.
--
--  Sao 18 linhas, das quais 13 ATIVAS: a lista oficial de filiais em operacao, confirmada pelo
--  Ricardo em 04/09/2026. O codigo segue o padrao 0101NN, em que NN e o numero da empresa no
--  sistema legado — a correspondencia e direta.
--
--  AS DEMAIS ENTRAM COM EstaAtiva = 0. O legado marca algumas delas como ativas e o negocio nao
--  as confirmou; trata-las como operacionais faria a API oferecer, no seletor de filial, uma
--  filial onde ninguem trabalha. Elas ficam no banco (nao se apaga filial: o historico aponta
--  para ela) e fora da lista de selecao, que so devolve ativas.
--
--  HIERARQUIA PLANA de proposito: Caminho = '/' e Nivel = 0 em todas. As filiais sao irmas, nao
--  ha holding intermediaria modelada nesta fase, e por isso "esta empresa e as abaixo" resolve
--  para a propria — que e exatamente o isolamento que a fronteira de multiempresa deve dar hoje.
-- ---------------------------------------------------------------------------------------------
MERGE organizacao.Empresa AS destino
USING (VALUES
    ('010101', N'Tracbel Agro — Ribeirão Preto', 1),
    ('010102', N'Tracbel Agro — Araraquara', 1),
    ('010103', N'Tracbel Agro — Barretos', 1),
    ('010107', N'Tracbel Agro — Orlândia', 1),
    ('010109', N'Tracbel Agro — Bebedouro', 1),
    ('010111', N'Tracbel Agro — Franca', 1),
    ('010112', N'Tracbel Agro — Itápolis', 1),
    ('010113', N'Tracbel Agro — São José do Rio Preto', 1),
    ('010114', N'Tracbel Agro — Catanduva', 1),
    ('010115', N'Tracbel Agro — Jales', 1),
    ('010116', N'Tracbel Agro — Votuporanga', 1),
    ('010117', N'Tracbel Agro — Tupã', 1),
    ('010118', N'Tracbel Agro — Marília', 1),
    ('010104', N'Tracbel Agro — Guaíra', 0),
    ('010105', N'Tracbel Agro — Ituverava', 0),
    ('010110', N'Tracbel Agro — Monte Alto', 0),
    ('CFRA_COLORADO_6', N'Colorado Desativado', 0),
    ('CEQU', N'Colorado Equipamentos', 0)
) AS origem (Codigo, Nome, EstaAtiva)
ON destino.Codigo = origem.Codigo
WHEN MATCHED AND (destino.Nome <> origem.Nome OR destino.EstaAtiva <> origem.EstaAtiva)
    THEN UPDATE SET Nome = origem.Nome, EstaAtiva = origem.EstaAtiva, AlteradoEm = SYSUTCDATETIME()
WHEN NOT MATCHED BY TARGET
    THEN INSERT (ChavePublica, Codigo, Nome, Caminho, Nivel, EstaAtiva, CriadoEm)
         VALUES (NEWID(), origem.Codigo, origem.Nome, '/', 0, origem.EstaAtiva, SYSUTCDATETIME());

-- ---------------------------------------------------------------------------------------------
--  metadado.CatalogoItem — PAPEL_CONTATO (3 itens)
--  Origem: dados-referencia/papel_de_contato.json
-- ---------------------------------------------------------------------------------------------
MERGE metadado.CatalogoItem AS destino
USING (VALUES
    ('DECISOR', N'Decisor', 1, 1, 0),
    ('INFLUENCIADOR', N'Influenciador', 2, 1, 0),
    ('NAO_DECISOR', N'Não decisor', 3, 1, 0)
) AS origem (Codigo, Descricao, Ordem, EstaAtivo, ExigeObservacao)
ON destino.CatalogoId = 1 AND destino.Codigo = origem.Codigo
WHEN MATCHED AND (destino.Descricao <> origem.Descricao OR destino.EstaAtivo <> origem.EstaAtivo)
    THEN UPDATE SET Descricao = origem.Descricao, Ordem = origem.Ordem, EstaAtivo = origem.EstaAtivo
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
         VALUES (1, origem.Codigo, origem.Descricao, origem.Ordem, origem.ExigeObservacao, origem.EstaAtivo);

-- ---------------------------------------------------------------------------------------------
--  metadado.CatalogoItem — ORIGEM_LEAD (45 itens)
--  Origem: dados-referencia/origem_de_lead.json
-- ---------------------------------------------------------------------------------------------
MERGE metadado.CatalogoItem AS destino
USING (VALUES
    ('AGROFY', N'Agrofy', 1, 1, 0),
    ('ANUNCIOS', N'Anuncios', 2, 1, 0),
    ('BI_CNAE', N'BI CNAE', 3, 1, 0),
    ('CALL_NOW', N'MF Rural', 4, 1, 0),
    ('COLORADO_CONECTA', N'Colorado Conecta', 5, 1, 0),
    ('CONVITE', N'Convite', 6, 1, 0),
    ('EEMOVEL', N'EEmovel', 7, 1, 0),
    ('EMAIL_MKT', N'Email Mkt', 8, 1, 0),
    ('EVENTOS', N'Eventos', 9, 1, 0),
    ('EXTERNO', N'Externo', 10, 1, 0),
    ('FACEBOOK', N'Facebook', 11, 1, 0),
    ('FACEBOOK_ANUNCIO', N'Facebook Anúncio', 12, 1, 0),
    ('GOOGLE_ANUNCIO', N'Google anúncio', 13, 1, 0),
    ('HOTSITE', N'Hotsite', 14, 1, 0),
    ('INDICACAO', N'Indicacao', 15, 1, 0),
    ('INSTAGRAM', N'Instagram', 16, 1, 0),
    ('INSTAGRAM_ANUNCIO', N'Instagram Anúncio', 17, 1, 0),
    ('INSTAGRAM_ORGANICO', N'Instagram Orgânico', 18, 1, 0),
    ('JA_CONHECE', N'Ja Conhece', 19, 1, 0),
    ('JD_CONECTA', N'John Deere Conecta', 20, 1, 0),
    ('JOHN_DEERE', N'John Deere', 21, 1, 0),
    ('LANDING_PAGE', N'Landing Page', 22, 1, 0),
    ('LINKED_IN_ORGANICO', N'Linked in Orgânico', 23, 1, 0),
    ('META', N'Meta', 24, 1, 0),
    ('MF_RURAL', N'Call Now', 25, 1, 0),
    ('OLX', N'OLX', 26, 1, 0),
    ('PESQ_SATSFYD', N'Pesq Satsfyd', 27, 1, 0),
    ('POPAGRO', N'PopAgro', 28, 1, 0),
    ('PROSPECCAO_ATIVA', N'Prospecção Ativa', 29, 1, 0),
    ('RADIO', N'Rádio', 30, 1, 0),
    ('RANDON', N'Randon', 31, 1, 0),
    ('RD_TALLOS', N'RD Tallos', 32, 1, 0),
    ('SAC', N'SAC', 33, 1, 0),
    ('SHOW_ROOM', N'Show Room', 34, 1, 0),
    ('SITE', N'Site', 35, 1, 0),
    ('SITE_ARREMAQ', N'Site Arremaq', 36, 1, 0),
    ('SITE_PARCEIROS', N'Site Parceiros', 37, 1, 0),
    ('SITE_TR_E_CA', N'Site TR e CA', 38, 1, 0),
    ('SMS', N'SMS', 39, 1, 0),
    ('TV', N'TV', 40, 1, 0),
    ('VISITA', N'Visita', 41, 1, 0),
    ('WHATS_APP', N'Whats App', 42, 1, 0),
    ('WHATS_APP_PROSPECCAO', N'Whats App Prospecção', 43, 1, 0),
    ('YOUTUBE', N'Youtube', 44, 1, 0),
    ('YOUTUBE_ORGANICO', N'Youtube Orgânico', 45, 1, 0)
) AS origem (Codigo, Descricao, Ordem, EstaAtivo, ExigeObservacao)
ON destino.CatalogoId = 2 AND destino.Codigo = origem.Codigo
WHEN MATCHED AND (destino.Descricao <> origem.Descricao OR destino.EstaAtivo <> origem.EstaAtivo)
    THEN UPDATE SET Descricao = origem.Descricao, Ordem = origem.Ordem, EstaAtivo = origem.EstaAtivo
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
         VALUES (2, origem.Codigo, origem.Descricao, origem.Ordem, origem.ExigeObservacao, origem.EstaAtivo);

-- ---------------------------------------------------------------------------------------------
--  metadado.CatalogoItem — CULTURA (3 itens)
--  Origem: dados-referencia/cultura.json
-- ---------------------------------------------------------------------------------------------
MERGE metadado.CatalogoItem AS destino
USING (VALUES
    ('SOJA', N'Soja', 1, 1, 0),
    ('MILHO_SAFRINHA', N'Milho safrinha', 2, 1, 0),
    ('ALGODAO', N'Algodão', 3, 1, 0)
) AS origem (Codigo, Descricao, Ordem, EstaAtivo, ExigeObservacao)
ON destino.CatalogoId = 5 AND destino.Codigo = origem.Codigo
WHEN MATCHED AND (destino.Descricao <> origem.Descricao OR destino.EstaAtivo <> origem.EstaAtivo)
    THEN UPDATE SET Descricao = origem.Descricao, Ordem = origem.Ordem, EstaAtivo = origem.EstaAtivo
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
         VALUES (5, origem.Codigo, origem.Descricao, origem.Ordem, origem.ExigeObservacao, origem.EstaAtivo);

-- ---------------------------------------------------------------------------------------------
--  metadado.CatalogoItem — TIPO_DOCUMENTO (107 itens)
--  Origem: dados-referencia/tipo_de_documento.json
-- ---------------------------------------------------------------------------------------------
MERGE metadado.CatalogoItem AS destino
USING (VALUES
    ('EXPERIE_CLIENTE', N'Inquérito de Experiência do Cliente', 1, 1, 0),
    ('FINANCEIRO', N'Documentos Financeiro', 2, 1, 0),
    ('OUTROS', N'Documentos Diversos', 3, 1, 0),
    ('NF', N'NF', 4, 1, 0),
    ('NF_VEN_DIRETA', N'NF Venda Direta', 5, 1, 0),
    ('PEDIDO_VENDA', N'Pedido Venda', 6, 1, 0),
    ('NF_DE_REFATURAM', N'Nota Fiscal Refaturamento', 7, 1, 0),
    ('SERASA', N'Consulta de Serasa', 8, 1, 0),
    ('IBM', N'Inspeção Básica de Máquina', 9, 1, 0),
    ('CONTR_LOCACAO', N'Contrato de Locação', 10, 1, 0),
    ('AUTOR_AGREGA', N'Autorização de agrega e desagrega especial', 11, 1, 0),
    ('AVALIACAO_USADO', N'Avaliação Equipamento/Implemento Usado', 12, 1, 0),
    ('AUTOR_FATURA', N'Autorizacao de Faturamento', 13, 1, 0),
    ('DECLARACAO', N'Declaracao de Onus', 14, 1, 0),
    ('NOTA_FISCAL', N'Nota de Venda', 15, 1, 0),
    ('VALIDADO_AVALIA', N'Validação de Avaliação e Fotos', 16, 1, 0),
    ('AUTORIZACAO', N'Autorizacao entregar novo sem retirar usado', 17, 1, 0),
    ('NF_SERVICO_AMS', N'Nota Fiscal Faturamento Serviço AMS', 18, 1, 0),
    ('AVALI_LOGISTICA', N'Avaliação retirada logistica', 19, 1, 0),
    ('AVALI_OFICINA', N'Avaliação Receber Maquina Oficina', 20, 1, 0),
    ('NF_USADO_CLIENT', N'Nota Fiscal Usado Cliente', 21, 1, 0),
    ('FOR_USADOS', N'Formulario Venda Usado', 22, 1, 0),
    ('DECL_ONUS_IMP', N'Declaração de Onus IMP', 23, 1, 0),
    ('CALCULO_COMISSA', N'Calculo de comissão venda direta', 24, 1, 0),
    ('CONF_EQUIP', N'Configuração do Equiipamento', 25, 1, 0),
    ('ORCA_PECAS', N'Orcamento de Peças', 26, 1, 0),
    ('PEDIDO_JD_QUOTE', N'Pedido JD Quote (Assinado)', 27, 1, 0),
    ('AUTO_COMPRA_IMP', N'Autorização de Compra Implemento', 28, 1, 0),
    ('NF_JD', N'Nota Fiscal Venda Direta JD', 29, 1, 0),
    ('NF_COLORADO', N'Nota Fiscal Venda Direta Colorado', 30, 1, 0),
    ('NF_DEV_REFATURA', N'Nota Fiscal Devolução Refaturamento', 31, 1, 0),
    ('LIBER_PREP_MAQ', N'Liberação para preparação maquina', 32, 1, 0),
    ('PLANILHA_PRECO', N'Planilha Formação de Preço', 33, 1, 0),
    ('COMISSAO_IMP', N'Comissão de Implementos', 34, 1, 0),
    ('COMISSAO_AMS', N'Comissão AMS', 35, 1, 0),
    ('TERMO_DE_CESSAO', N'Termo de Cessão Equipamento', 36, 1, 0),
    ('PLANILHA_AMS', N'Planilha Formação de Preço AMS', 37, 1, 0),
    ('NF_ORIGEM_USADO', N'Nota Fiscal de Origem - Usado', 38, 1, 0),
    ('DEC_PERDA_NF_US', N'Declaração de Perda NF Usado', 39, 1, 0),
    ('PLANILHA_IMP', N'Planilha Formação de Preço Implemento', 40, 1, 0),
    ('ORC_AGREGA', N'Orçamento Agrega', 41, 1, 0),
    ('APOLICE_SEGURO', N'Apolice de seguro', 42, 1, 0),
    ('CHECKLIST_EMB', N'Check List Embarque', 43, 1, 0),
    ('CHECKLIST_DESEM', N'Check List Desembarque', 44, 1, 0),
    ('REMESSA', N'Remessa Análise Fluído', 45, 1, 0),
    ('RELATORIO_PECAS', N'RELATÓRIO PEÇAS BALCÃO', 46, 1, 0),
    ('PLANILHA_CORTES', N'PLANILHA DE CORTESIA', 47, 1, 0),
    ('PERFORMANCE', N'Relatório de Performance', 48, 1, 0),
    ('PERCEPCAO', N'Relatório de Percepção', 49, 1, 0),
    ('NF_DEMONSTRACAO', N'NF Demonstração', 50, 1, 0),
    ('NF_RETORNO_DEMO', N'NF Retorno Demonstração', 51, 1, 0),
    ('LAYOUT', N'Layout - Projeto Irrigação', 52, 1, 0),
    ('PROPOSTA', N'Proposta - Projeto Irrigação', 53, 1, 0),
    ('DCP', N'DCP', 54, 1, 0),
    ('NF_MAQ_IMPLEMEN', N'NF MAQ/IMPLEMENTO', 55, 1, 0),
    ('CONTRATO', N'CONTRATO', 56, 1, 0),
    ('VENDA_CONTRATO', N'VENDA CONTRATO', 57, 1, 0),
    ('AUT_FATURAMENT', N'AUTORIZAÇÃO FATURAMENTO - 61', 58, 1, 0),
    ('DC_NAO_AUT_PMP', N'DOC NÃO AUTORIZADO PMP', 59, 1, 0),
    ('FOTO_PMP', N'FOTO PMP', 60, 1, 0),
    ('DOC_LOCAL_DESC', N'DOC LOCALIZAÇÃO DESCONHECIDA PMP', 61, 1, 0),
    ('CHECK_LIST_PRE', N'CHECK LIST_PRÉ_ENTREGA', 62, 1, 0),
    ('CORR_CHECK_LIST', N'CORREÇÃO_CHECK_LIST_PRÉ_ENTREGA', 63, 1, 0),
    ('ORC_PECA_PRE', N'ORÇAMENTO PEÇAS PRÉ ENTREGA', 64, 1, 0),
    ('RGA', N'RGA', 65, 1, 0),
    ('TCAT_CTRE', N'TCAT_CTRE', 66, 1, 0),
    ('TCAT_CNH_MOT', N'TCAT_CNH_MOT', 67, 1, 0),
    ('TCAT_CAVALO', N'TCAT_CAVALO', 68, 1, 0),
    ('TCAT_CARRETA', N'TCAT_CARRETA', 69, 1, 0),
    ('TCAT_NF_EQUIP', N'TCAT_NF_EQUIP', 70, 1, 0),
    ('TCAT_CHECK_LIST', N'TCAT_CHECK-LIST', 71, 1, 0),
    ('TCAT_FOT_AVARIA', N'TCAT_FOT_AVARIA', 72, 1, 0),
    ('TCAT_ORC_REPARO', N'TCAT_ORÇ_REPARO', 73, 1, 0),
    ('FAT_EXTERN_DSI', N'FATURAMENTO EXTERNO DSI', 74, 1, 0),
    ('COMP_PAGTO_DSI', N'COMPROVANTE PAGAMENTO DSI', 75, 1, 0),
    ('NF_ASSINADA', N'NF ASSINADA', 76, 1, 0),
    ('PLAN_MANUT_ASSI', N'Plano Manutenção Ass. - PLAN MANUT ASSI - 80', 77, 1, 0),
    ('DC_ENTG_TEC_ASS', N'Doc Entrega Téc Assinado - DC ENTG TEC ASS - 81', 78, 1, 0),
    ('ORCAMENTO', N'ORÇAMENTO', 79, 1, 0),
    ('NF_VENDA_PECA', N'NF VENDA PEÇA', 80, 1, 0),
    ('PEDIDO_COMP_DSI', N'PEDIDO COMPRA DSI', 81, 1, 0),
    ('DOC_GARANTIA', N'Documento de Garantia', 82, 1, 0),
    ('CAPA_OS_FATURAD', N'CAPA OS FATURADA', 83, 1, 0),
    ('AUT_FAT_FY25', N'AUTORIZAÇÃO DE FATURAMENTO FY25', 84, 1, 0),
    ('VENCIMENTO_SEG', N'VENCIMENTO SEGURO', 85, 1, 0),
    ('AVALIACAO_SMNV', N'AVALIAÇÃO SEMINOVOS', 86, 1, 0),
    ('PLAN_CUSTO_SMNV', N'PLANILHA DE CUSTO SEMINOVOS', 87, 1, 0),
    ('DECL_ONUS_SMNV', N'DECLARAÇÃO DE ÔNUS SEMINOVOS', 88, 1, 0),
    ('CERT_ONUS_SMNV', N'CERTIDÃO NEGATIVA DE ÔNUS SEMINOVOS', 89, 1, 0),
    ('NFO_DPNF_SMNV', N'NF ORIGEM/DECLARAÇÃO PERDA NF SEMINOVOS', 90, 1, 0),
    ('PED_COMPRA_VD', N'PEDIDO DE COMPRA VENDA DIRETA', 91, 1, 0),
    ('PLAN_VENDA_VD', N'PLANILHA DE VENDA DIRETA', 92, 1, 0),
    ('JD_QUOTE', N'JD_QUOTE', 93, 1, 0),
    ('OPERATION_CENTE', N'Operation Center', 94, 1, 0),
    ('AGRONOMY_ANALYZ', N'Agronomy Analyzer', 95, 1, 0),
    ('VENDA_DIRETA', N'CONTRATO VENDA DIRETA', 96, 1, 0),
    ('COMP_DE_PAGTO', N'Comprovante de Pagamento', 97, 1, 0),
    ('EMAIL_APROV_CRE', N'E-mail de Aprovação Crédito', 98, 1, 0),
    ('PLANILHA_CUSTO', N'PLANILHA CUSTO GRANDES CONTAS', 99, 1, 0),
    ('PEDIDO_GC', N'PLANILHA PEDIDO GRANDES CONTAS', 100, 1, 0),
    ('CSC_CHASSI', N'Comprovante da Procedência', 101, 1, 0),
    ('ATIVACAO_LICENC', N'Ativação Licença', 102, 1, 0),
    ('LICENCA_ATIVADA', N'Licença Ativada', 103, 1, 0),
    ('COMUNICADO_ATIV', N'Comunicado Ativação', 104, 1, 0),
    ('FORM_ENTREGA', N'FORM ENTREGA', 105, 1, 0),
    ('FOTO_DEMO_PDF', N'FOTO_DEMO_PDF', 106, 1, 0),
    ('AUTO_SEMI_NOVOS', N'Autorização departamento Semi Novos', 107, 1, 0)
) AS origem (Codigo, Descricao, Ordem, EstaAtivo, ExigeObservacao)
ON destino.CatalogoId = 6 AND destino.Codigo = origem.Codigo
WHEN MATCHED AND (destino.Descricao <> origem.Descricao OR destino.EstaAtivo <> origem.EstaAtivo)
    THEN UPDATE SET Descricao = origem.Descricao, Ordem = origem.Ordem, EstaAtivo = origem.EstaAtivo
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
         VALUES (6, origem.Codigo, origem.Descricao, origem.Ordem, origem.ExigeObservacao, origem.EstaAtivo);

-- ---------------------------------------------------------------------------------------------
--  metadado.CatalogoItem — CONCORRENTE (2 itens)
--  Origem: dados-referencia/concorrente.json
-- ---------------------------------------------------------------------------------------------
MERGE metadado.CatalogoItem AS destino
USING (VALUES
    ('CASE_IH', N'Case IH', 1, 1, 0),
    ('NEW_HOLLAND', N'New Holland', 2, 1, 0)
) AS origem (Codigo, Descricao, Ordem, EstaAtivo, ExigeObservacao)
ON destino.CatalogoId = 8 AND destino.Codigo = origem.Codigo
WHEN MATCHED AND (destino.Descricao <> origem.Descricao OR destino.EstaAtivo <> origem.EstaAtivo)
    THEN UPDATE SET Descricao = origem.Descricao, Ordem = origem.Ordem, EstaAtivo = origem.EstaAtivo
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
         VALUES (8, origem.Codigo, origem.Descricao, origem.Ordem, origem.ExigeObservacao, origem.EstaAtivo);

-- ---------------------------------------------------------------------------------------------
--  metadado.CatalogoItem — MOTIVO_INATIVACAO
--
--  PROVISORIO, E ESTA MARCADO COMO TAL. Nao existe fonte para este catalogo: nem o legado nem o
--  prototipo tem a lista, e dados-referencia/ nao a entrega (ver PENDENTES.md). As tres linhas
--  abaixo sao o minimo estrutural para o fluxo de inativacao funcionar de ponta a ponta, e a
--  lista definitiva e uma pergunta aberta ao negocio — registrada em docs/projeto/23-API.md,
--  secao de divida.
--
--  O item OUTRO nasce com ExigeObservacao = 1: escolher "outro" sem escrever o motivo e como o
--  legado acumulou processos cancelados sem justificativa nenhuma.
-- ---------------------------------------------------------------------------------------------
MERGE metadado.CatalogoItem AS destino
USING (VALUES
    ('ENCERROU_ATIVIDADE', N'Encerrou atividade', 10, 0),
    ('DUPLICADO', N'Cadastro duplicado', 20, 0),
    ('OUTRO', N'Outro motivo', 90, 1)
) AS origem (Codigo, Descricao, Ordem, ExigeObservacao)
ON destino.CatalogoId = 3 AND destino.Codigo = origem.Codigo
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
         VALUES (3, origem.Codigo, origem.Descricao, origem.Ordem, origem.ExigeObservacao, 1);

-- ---------------------------------------------------------------------------------------------
--  metadado.CatalogoItem — MOTIVO_DESCARTE
--
--  PROVISORIO, E ESTA MARCADO COMO TAL. Nao existe fonte para este catalogo: nem o legado nem o
--  prototipo tem a lista, e dados-referencia/ nao a entrega (ver PENDENTES.md). As tres linhas
--  abaixo sao o minimo estrutural para o fluxo de inativacao funcionar de ponta a ponta, e a
--  lista definitiva e uma pergunta aberta ao negocio — registrada em docs/projeto/23-API.md,
--  secao de divida.
--
--  O item OUTRO nasce com ExigeObservacao = 1: escolher "outro" sem escrever o motivo e como o
--  legado acumulou processos cancelados sem justificativa nenhuma.
-- ---------------------------------------------------------------------------------------------
MERGE metadado.CatalogoItem AS destino
USING (VALUES
    ('SEM_PERFIL', N'Sem perfil de compra', 10, 0),
    ('SEM_CONTATO', N'Nao foi possivel contato', 20, 0),
    ('OUTRO', N'Outro motivo', 90, 1)
) AS origem (Codigo, Descricao, Ordem, ExigeObservacao)
ON destino.CatalogoId = 4 AND destino.Codigo = origem.Codigo
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CatalogoId, Codigo, Descricao, Ordem, ExigeObservacao, EstaAtivo)
         VALUES (4, origem.Codigo, origem.Descricao, origem.Ordem, origem.ExigeObservacao, 1);

-- ---------------------------------------------------------------------------------------------
--  frota.Marca — as marcas, REPRESENTADAS e CONCORRENTES na mesma tabela.
--
--  Os concorrentes entram aqui de proposito, com EhRepresentada = 0. E o que permite cadastrar a
--  maquina do concorrente no parque do cliente — que e justamente o dado que a tela de Cobertura
--  precisa e que nenhum ERP tem, porque ninguem fatura a maquina do concorrente.
-- ---------------------------------------------------------------------------------------------
MERGE frota.Marca AS destino
USING (VALUES
    ('JOHN_DEERE', N'John Deere', 1),
    ('MANITOU', N'Manitou', 1),
    ('HUSQVARNA', N'Husqvarna', 1),
    ('VALLEY', N'Valley (irrigação)', 1),
    ('COLORADO', N'Colorado', 1),
    ('CASE_IH', N'Case IH', 0),
    ('NEW_HOLLAND', N'New Holland', 0)
) AS origem (Codigo, Nome, EhRepresentada)
ON destino.Codigo = origem.Codigo
WHEN MATCHED AND destino.Nome <> origem.Nome THEN UPDATE SET Nome = origem.Nome
WHEN NOT MATCHED BY TARGET
    THEN INSERT (Codigo, Nome, EhRepresentada, EstaAtiva)
         VALUES (origem.Codigo, origem.Nome, origem.EhRepresentada, 1);

-- ---------------------------------------------------------------------------------------------
--  frota.Familia — UMA POR MARCA, e todas com o codigo A_CONFIRMAR.
--
--  ISTO E UMA LACUNA DECLARADA, nao um desenho. dados-referencia/familia.json esta VAZIO: nao ha
--  fonte de familia em lugar nenhum — o catalogo de modelos do legado virou dump (4.431.168
--  linhas) e o prototipo nao tem o nivel intermediario. Como frota.Modelo exige uma familia, a
--  alternativa a esta linha seria nao ter catalogo de modelo nenhum, e ai o cadastro de
--  equipamento nao existiria.
--
--  Entao a familia entra com o nome dizendo o que ela e: "A confirmar". Quando a frente de dados
--  de referencia entregar as familias de verdade, este bloco e substituido e os modelos sao
--  reapontados — sem migracao, porque e dado.
-- ---------------------------------------------------------------------------------------------
MERGE frota.Familia AS destino
USING (
    SELECT m.Id AS MarcaId, 'A_CONFIRMAR' AS Codigo, N'A confirmar' AS Nome FROM frota.Marca m
) AS origem
ON destino.MarcaId = origem.MarcaId AND destino.Codigo = origem.Codigo
WHEN NOT MATCHED BY TARGET
    THEN INSERT (MarcaId, Codigo, Nome, EstaAtiva) VALUES (origem.MarcaId, origem.Codigo, origem.Nome, 1);

-- ---------------------------------------------------------------------------------------------
--  frota.Modelo — os 18 modelos do catalogo do prototipo.
--
--  Todos sob a familia "A confirmar" da John Deere, pela lacuna descrita acima. O codigo do
--  modelo e estavel e e o que a API aceita no cadastro de equipamento — nunca texto livre.
-- ---------------------------------------------------------------------------------------------
MERGE frota.Modelo AS destino
USING (
    SELECT v.Codigo, v.Nome, f.Id AS FamiliaId
    FROM (VALUES
        ('5075E', N'5075E'),
        ('5090E', N'5090E'),
        ('6110J', N'6110J'),
        ('6135J', N'6135J'),
        ('7230J', N'7230J'),
        ('8R_250', N'8R 250'),
        ('8R_340', N'8R 340'),
        ('S770', N'S770'),
        ('S780', N'S780'),
        ('S680_SEMINOVA', N'S680 seminova'),
        ('4630', N'4630'),
        ('4730', N'4730'),
        ('DB40', N'DB40'),
        ('DB44', N'DB44'),
        ('DB50', N'DB50'),
        ('2730', N'2730'),
        ('ARADO_4_DISCOS', N'Arado 4 discos'),
        ('CONTRATO_ANUAL', N'Contrato anual')
    ) AS v (Codigo, Nome)
    CROSS JOIN (
        SELECT TOP 1 fa.Id FROM frota.Familia fa
        JOIN frota.Marca ma ON ma.Id = fa.MarcaId
        WHERE ma.Codigo = 'JOHN_DEERE' AND fa.Codigo = 'A_CONFIRMAR'
    ) AS f
) AS origem
ON destino.Codigo = origem.Codigo
WHEN MATCHED AND destino.Nome <> origem.Nome THEN UPDATE SET Nome = origem.Nome
WHEN NOT MATCHED BY TARGET
    THEN INSERT (FamiliaId, Codigo, Nome, EstaAtivo) VALUES (origem.FamiliaId, origem.Codigo, origem.Nome, 1);

COMMIT TRANSACTION;

PRINT 'Semente de referencia aplicada.';

SELECT 'organizacao.Empresa' AS Tabela, COUNT(*) AS Linhas, SUM(CAST(EstaAtiva AS int)) AS Ativas FROM organizacao.Empresa
UNION ALL SELECT 'metadado.CatalogoItem', COUNT(*), SUM(CAST(EstaAtivo AS int)) FROM metadado.CatalogoItem
UNION ALL SELECT 'frota.Marca', COUNT(*), SUM(CAST(EstaAtiva AS int)) FROM frota.Marca
UNION ALL SELECT 'frota.Familia', COUNT(*), SUM(CAST(EstaAtiva AS int)) FROM frota.Familia
UNION ALL SELECT 'frota.Modelo', COUNT(*), SUM(CAST(EstaAtivo AS int)) FROM frota.Modelo;
