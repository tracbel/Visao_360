# API `wsVorticeCrmApi` - endpoints completos (extraidos do Swagger)

- **Spec:** `http://10.150.6.230/wsVorticeCrmApi/swagger/docs/v2` (Swagger 2.0, Swashbuckle) - copia em `swagger-wsVorticeCrmApi.json`
- **Titulo:** Documentação API 4.04.01-3
- **basePath:** `/wsVorticeCrmApi` - **schemes:** http
- **Rotas:** 112 - **Operacoes:** 116 - **Definicoes (modelos):** 91
- **securityDefinitions:** **nenhuma** - o spec nao declara esquema de autenticacao
- Coletado em 2026-09-02 23:27.

## Resumo por familia

| Familia | Operacoes |
|---|---:|
| acordo | 4 |
| ApiInfo | 1 |
| ativmob | 1 |
| cartao | 26 |
| certiface | 6 |
| chat | 2 |
| crm (nucleo) | 21 |
| crm/imp (importacao ERP) | 18 |
| facebook | 1 |
| followize | 4 |
| infobip | 2 |
| mercanet | 1 |
| neurotech | 1 |
| pabx | 3 |
| portal | 15 |
| rd | 2 |
| vorticeadmin | 1 |
| vtcsaas | 3 |
| webhook | 3 |
| wifire | 1 |

## Endpoints

### acordo

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/acordo/auth` | Médoto de autenticação. Chame este médoto duas vezes. Na primeira informe apenas o Cpf e receberá o JSON { "codigo": "", "two_factor_auth": true, "tipo_codigo_acesso": "", "codigo_acesso_descricao": "", "telefone_truncado": "", "seqpessoa": 0 } como resposta. Peça ao usuário o código e depois chame este método informando o Cpf e o código de acesso. | bodyRequest [body]*; Authorization [header] |
| POST | `/api/acordo/boleto/get` | Método para realizar o download do pdf com os boletos do acordo | bodyRequest [body]*; Authorization [header] |
| POST | `/api/acordo/get` | Método para retornar os acordos. | bodyRequest [body]*; Authorization [header] |
| GET | `/api/acordo/nroprocesso` | Gera um novo número de processo | Authorization [header] |

### ApiInfo

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| GET | `/api/ApiInfo` |  | Authorization [header] |

### ativmob

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/ativmob/event` | End-Point para recebimento de eventos de rastreio provenientes da Ativmob | voEvento [body]*; Authorization [header] |

### cartao

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/cartao/analisecredito` | Realiza análise de crédito. Será gerado um evento com a origem "NEUROTECH" e o código do evento será o valor do campo Resultado devolvido pela Neurotech. | Authorization [header] |
| POST | `/api/cartao/auth` | Inicializa o processo do fluxo da solicitação da proposta de cartão. Este é o primeiro método que deve ser chamado e é onde o token será gerado para utilizar nos demais métodos | data [body]*; Authorization [header] |
| POST | `/api/cartao/celular/validcheck/{numero}/{codigo}` | Valida o código enviado por SMS para o número de celular informado. Será gerado um evento com a origem "CARTAO" e os possíveis código do evento são: "CELULAR VALIDADO", "CELULAR NAO VALIDADO" | numero [path]*; codigo [path]*; Authorization [header] |
| POST | `/api/cartao/celular/validsend/{numero}` | Envia um código via SMS para validar o número do celular. Será gerado um evento com a origem CARTAO e o código de evento CELULAR EM VALIDACAO | numero [path]*; Authorization [header] |
| POST | `/api/cartao/certiface/captcha` | Envia os desafios e obtem o resultado. Será gerado um evento com a origem "CERTIFACE" e os possíveis código do evento são: "BIOMETRIA APROVADA", "BIOMETRIA NEGADA" e no resultado complementar o valor do campo codID devolvido pela Certiface | data [body]*; Authorization [header] |
| POST | `/api/cartao/certiface/challenge` | Obtem os desafios da prova de vida. | data [body]*; Authorization [header] |
| POST | `/api/cartao/codigovalidacaocheck/{subcontexto}/{codigo}` | Valida um código enviado ao celular | subcontexto [path]*; codigo [path]*; Authorization [header] |
| POST | `/api/cartao/codigovalidacaosend/{subcontexto}` | Envia um código de validação para o celular informado para validar o contexto informado no parâmetro psSubContexto | subcontexto [path]*; Authorization [header] |
| POST | `/api/cartao/conductor/cadexiste` | Verifica se o cadastro de um Cpf já existe na Conductor. Será gerado evento com a origem "CONDUCTOR" e os possíveis código do evento são: "CADASTRO_EXISTE", "CADASTRO_NAO_EXISTE" | Authorization [header] |
| POST | `/api/cartao/conductor/criaconta` | Cria a conta na Conductor. Será gerado evento com a Origem "CONDUCTOR" e os possíveis código de evento são: "CONTA CRIADA", "CONTA NAO CRIADA" | Authorization [header] |
| GET | `/api/cartao/dadosfinanceiros` | Retorna as informações financeiras da pessoa | Authorization [header] |
| POST | `/api/cartao/dadosfinanceiros` | Recebe as informações financeiras da pessoa | data [body]*; Authorization [header] |
| POST | `/api/cartao/documentos` | Recebe documentos | data [body]*; Authorization [header] |
| POST | `/api/cartao/evento` | Gera o evento recebido nas tabelas de importação do CRM. | data [body]*; Authorization [header] |
| GET | `/api/cartao/formulario/comprovanteendereco` | Retorna a lista dos comprovantes de endereço possíveis de serem usados | Authorization [header] |
| GET | `/api/cartao/formulario/comprovanterenda` | Retorna a lista dos comprovantes de renda possíveis de serem usados | Authorization [header] |
| GET | `/api/cartao/paramlista` | Retorna uma lista de parametros lista | data.seq [query]; data.parametro [query]; data.nroEmpresa [query]; Authorization [header] |
| POST | `/api/cartao/pessoa/foto` | Recebe a foto da pessoa em base64 | fotoBase64 [body]*; Authorization [header] |
| POST | `/api/cartao/prerequisitos` | Verifica os pré-requisitos para continuidade do processo de cartão. Será gerado um evento com a origem "CARTAO" e os possíveis código do evento são: "PRE-REQUISITO NEGADO", "PRE-REQUISITO APROVADO" | Authorization [header] |
| GET | `/api/cartao/promotor` | Obtém o promotor vinculado ao codusuario recebido | codusuario [query]*; Authorization [header] |
| GET | `/api/cartao/promotores` | Obtém a lista de promotores | psSearchText [query]*; Authorization [header] |
| GET | `/api/cartao/promotores/{psSearchText}` | Obtém a lista de promotores | psSearchText [path]*; Authorization [header] |
| GET | `/api/cartao/questionario` | Retorna os questionários respondidos. | Authorization [header] |
| POST | `/api/cartao/questionario` | Recebe um formulário respondido para gravar as respostas no CRM. Na lista de Perguntas deve-se enviar o campo NOMECOLUNA. A lista de perguntas e respotas devem estar ordenadas. Ou seja, a resposta de índice 1 corresponde à pergunta do mesmo índice e assim por diante. | data [body]*; Authorization [header] |
| GET | `/api/cartao/termo/adesao` | Retorna o termo de adesão do cliente logado. | Authorization [header] |
| GET | `/api/cartao/view/{nomeview}` | Busca informação em uma view. A view deve possuir a coluna SEQPESSOA ou PROCESSO que será usado na condição WHERE utilizando os dados do token | nomeview [path]*; Authorization [header] |

### certiface

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/certiface/facecaptcha/appkey` | Método para retornar a API Key usando a conta padrão da Certiface configurada no CRM. | data [body]*; Authorization [header] |
| POST | `/api/certiface/facecaptcha/captcha` | Método para enviar os desafios cumpridos. | data [body]*; Authorization [header] |
| POST | `/api/certiface/facecaptcha/challenge` | Método para retornar os desafios da appkey recebida. | appkey [body]*; Authorization [header] |
| POST | `/api/certiface/facecaptcha/credencial` | Obtem a credencial padrão da certiface configurada no CRM | Authorization [header] |
| POST | `/api/certiface/facecaptcha/result` | Método para obter o resultado do desafio de uma appkey. | appkey [body]*; Authorization [header] |
| POST | `/api/certiface/webhook` |  | request [body]*; Authorization [header] |

### chat

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/chat/carteirizacao` | Obtém o usuário do CRM para qual o chat deve ser encaminhado. | bodyRequest [body]*; Authorization [header] |
| GET | `/api/chat/departamento` | Obtém os departamentos disponíveis na view de carteirização de chat | Authorization [header] |

### crm (nucleo)

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| GET | `/api/crm/atributofixo/{Parametro}` | Retorna uma lista de atributos fixo | Parametro [path]*; Authorization [header] |
| GET | `/api/crm/autocep/cidade/{UF}` | Obtem a lista de cidades normalizadas pelo AutoCep | UF [path]*; Authorization [header] |
| GET | `/api/crm/cidade/{UF}` | Obtem a lista de cidade pertencentes à UF recebida | UF [path]*; Authorization [header] |
| POST | `/api/crm/documento` | Método para anexar documentos ao processo | Authorization [header] |
| GET | `/api/crm/empresas` | Retorna a lista de empresas ativas | Authorization [header] |
| POST | `/api/crm/empresas/modulo/perm` | Obtém a lista de empresas que o usuário possuí permissão no módulo informado. | data [body]*; Authorization [header] |
| GET | `/api/crm/formulario/schema/{psSeqFormulario}` | Neste método toda a estrutura de um formulário será retornada, contendo o cabeçalho, as perguntas e as opções de respostas. | psSeqFormulario [path]*; Authorization [header] |
| POST | `/api/crm/login` | Utilize este método para validar o login do CRM | data [body]*; Authorization [header] |
| POST | `/api/crm/optout/email` | Recebe uma lista de Optout de e-mails | data [body]*; Authorization [header] |
| POST | `/api/crm/optout/fone` | Recebe uma lista de Optout de telefones | data [body]*; Authorization [header] |
| GET | `/api/crm/paramlista` | Retorna uma lista de parametros lista | data.seq [query]; data.parametro [query]; data.nroEmpresa [query]; Authorization [header] |
| GET | `/api/crm/paramlista/{nroempresa}/{parametro}` | Busca um item na lista com base na empresa, no parâmetro e na query. | nroempresa [path]*; parametro [path]*; q [query]*; Authorization [header] |
| POST | `/api/crm/pessoa/search` | Busca uma pessoa utilizando os parâmetros informados. Será retornado a pessoa apenas se encontrar um único registro. É opcional a inclusão da carteirização de chat. Exemplo retorno: { "success": true, "seqpessoa": 123456, "nome": "Fulano de Tal", "carteirizacaoChat": "joao.silva" } | data [body]*; Authorization [header] |
| POST | `/api/crm/pessoa/searchlist` | Busca pessoas conforme os parâmetros passados e retorna uma lista com SeqPessoas encontrados. Caso encontre mais de um cadastro, irá retornar cadastros em ordem de alterações mais recentes primeiro. É opcional a inclusão da carteirização de chat. Exemplo retorno: { "success": true, "Pessoas": [ { "seqpessoa": 999, "nome": "Fulano de tal", "cpfcnpj": "12345678910", "nascimento": "13/12/1989", "carteirizacaoChat": "fulano.vendedor" } { "seqpessoa": 998, "nome": "Tal de Fulano", "cpfcnpj": "12345678910", "nascimento": "13/12/1989", "carteirizacaoChat": "fulano.supervisor" } ] } | data [body]*; Authorization [header] |
| POST | `/api/crm/pessoalink` | Grava um link de uma pessoa | data [body]*; Authorization [header] |
| GET | `/api/crm/pessoalink/{origem}/{pessoalink}` | Retorna um seqpessoa com base no link recebido | origem [path]*; pessoalink [path]*; Authorization [header] |
| GET | `/api/crm/processo/gera-numero` | Gera um novo número de processo | Authorization [header] |
| POST | `/api/crm/processo/search` | Retorna a lista de processos de acordo com os parâmetros de busca Caso seja informada uma ação, serão considerados apenas processos que tenham agenda em aberto daquela ação. | data [body]*; Authorization [header] |
| POST | `/api/crm/questionario` | Recebe um formulário respondido para gravar as respostas no CRM. Na lista de Perguntas deve-se enviar o campo NOMECOLUNA. A lista de perguntas e respotas devem estar ordenadas. Ou seja, a resposta de índice 1 corresponde à pergunta do mesmo índice e assim por diante. | data [body]*; Authorization [header] |
| GET | `/api/crm/textopadrao/{seq}` | Obtem o texto padrão informado | seq [path]*; Authorization [header] |
| POST | `/api/crm/view/{nomeview}` | Retorna os dados da view especificada na URL. | nomeview [path]*; where [body]*; Authorization [header] |

### crm/imp (importacao ERP)

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/crm/imp/email` | Recebe um email. | email [body]*; Authorization [header] |
| POST | `/api/crm/imp/evento` | Método para gerar um evento no CRM | poEvento [body]*; Authorization [header] |
| POST | `/api/crm/imp/formapgto` | Recebe uma forma de pagamento | formaPgto [body]*; Authorization [header] |
| POST | `/api/crm/imp/nfs` | Recebe uma nota fiscal | nfs [body]*; Authorization [header] |
| POST | `/api/crm/imp/nfscmpl` | Recebe um complemento de uma nota fiscal | nfsCmpl [body]*; Authorization [header] |
| POST | `/api/crm/imp/nfsitem` | Recebe um item de uma nota | nfsItem [body]*; Authorization [header] |
| POST | `/api/crm/imp/os` | Recebe uma ordem de serviço | os [body]*; Authorization [header] |
| POST | `/api/crm/imp/ositem` | Recebe um item de ordem de serviço | osItem [body]*; Authorization [header] |
| POST | `/api/crm/imp/ossolic` | Recebe uma solicitação de ordem de serviço | osSolic [body]*; Authorization [header] |
| POST | `/api/crm/imp/pedido` | Recebe um pedido | pedido [body]*; Authorization [header] |
| POST | `/api/crm/imp/pedidoitem` | Recebe um item de um pedido | pedidoItem [body]*; Authorization [header] |
| POST | `/api/crm/imp/pessoa` | Recebe os dados de uma pessoa | pessoa [body]*; Authorization [header] |
| POST | `/api/crm/imp/pessoa/contato` | Recebe um contato de uma pessoa | contato [body]*; Authorization [header] |
| POST | `/api/crm/imp/pessoa/fone` | Método para inserir/alterar/excluir um telefone de uma pessoa. | poEnt [body]*; Authorization [header] |
| POST | `/api/crm/imp/pessoa/propriedade` | Recebe uma propriedade de pessoa | propriedade [body]*; Authorization [header] |
| POST | `/api/crm/imp/produto` | Recebe um produto | produto [body]*; Authorization [header] |
| POST | `/api/crm/imp/titulo` | Recebe um título de cobrança | titulo [body]*; Authorization [header] |
| POST | `/api/crm/imp/veiculo` | Recebe um veículo | veiculo [body]*; Authorization [header] |

### facebook

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/facebook/webhook/lead` | Recebe um webhook do Facebook Leads Ads | data [body]*; Authorization [header] |

### followize

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/followize/lead/annotation` | Recebe um webhook de anotação do Lead e gera o evento "LEAD ANNOTATION" para a origem "FOLLOWIZE" | data [body]*; Authorization [header] |
| POST | `/api/followize/lead/create` | Recebe um webhook de criação do Lead e gera o evento "LEAD CREATE" para a origem "FOLLOWIZE" | data [body]*; Authorization [header] |
| POST | `/api/followize/lead/finalization` | Recebe um webhook de finalização do Lead e gera os eventos "LEAD FINALIZATION SALED", "LEAD FINALIZATION NOTSALED" para a origem "FOLLOWIZE" | data [body]*; Authorization [header] |
| POST | `/api/followize/lead/schedule` | Recebe um webhook de agendamento do Lead e gera o evento "LEAD SCHEDULE" para a origem "FOLLOWIZE" | data [body]*; Authorization [header] |

### infobip

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/infobip/IncomingMessage` | Recebe mensagens encaminhadas pela Infobip via webhook | Authorization [header] |
| POST | `/api/infobip/smsReceive` | Webhook para receber mensagens de SMS encaminhada pela Infobip utilizando o método Foward. Maiores informações em: https://dev.infobip.com/receive-sms/forward-method | bodyRequest [body]*; Authorization [header] |

### mercanet

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| GET | `/api/mercanet/event` |  | codusuario [query]*; origem [query]*; pessoalink [query]*; evento [query]*; id [query]*; url [query]*; Authorization [header] |

### neurotech

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| GET | `/api/neurotech/painel/{codOperacao}` |  | codOperacao [path]*; Authorization [header] |

### pabx

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/pabx/campanha/popup` | Recebe dados de uma chamada e grava os controles/agenda no módulo de call center (Discador Power) | data [body]*; Authorization [header] |
| GET | `/api/pabx/carteirizacao/{telefone}/{DTMF}` | Utilize este método para obter a carteirização do cliente. | telefone [path]*; DTMF [path]*; Authorization [header] |
| POST | `/api/pabx/chamada/incomingcall` | Webhook para receber os dados de uma chamada que foi entregue em um ramal. | data [body]*; Authorization [header] |

### portal

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| GET | `/api/portal/agenda` | Obtém a lista de agendas | concluidas [query]; dtaInicio [query]; dtaFim [query]; Authorization [header] |
| GET | `/api/portal/agenda/{seqAgenda}` | Retorna uma agenda | seqAgenda [path]*; Authorization [header] |
| GET | `/api/portal/ciencia` | Método para retornar a lista de ciências | Authorization [header] |
| POST | `/api/portal/ciencia` | Dá ciente em uma ciencia | data [body]*; Authorization [header] |
| GET | `/api/portal/docto/{seqdocto}` | Disponibiliza um documento para download | seqdocto [path]*; Authorization [header] |
| POST | `/api/portal/files/upload` | Método para upload de arquivos usando o form-data. Envie na key files um array de arquivos. Envie também a key processo com o número do processo para víniculo. | Authorization [header] |
| GET | `/api/portal/historico` | Traz a lista de históricos. Informe o pageNumber e o pageSize no header | processo [query]*; dna [query]*; Authorization [header] |
| POST | `/api/portal/historico` |  | data [body]*; Authorization [header] |
| POST | `/api/portal/password` | Método para alterar a senha do usuário | data [body]*; Authorization [header] |
| GET | `/api/portal/processo/{processo}` | Obtém os dados do processo informado | processo [path]*; Authorization [header] |
| GET | `/api/portal/processo/{processo}/documento` | Obtém a lista de documentos do processo | processo [path]*; Authorization [header] |
| GET | `/api/portal/resultado/{acao}` | Retorna a lista de resultados receptivos ou da ação informada | acao [path]*; Authorization [header] |
| POST | `/api/portal/senharecupera` | Método para recuperação de senha | data [body]*; Authorization [header] |
| POST | `/api/portal/signin` | Método para realizar o login do usuário | data [body]*; Authorization [header] |
| POST | `/api/portal/signup` | Realiza um novo cadastro | data [body]*; Authorization [header] |

### rd

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/rd/lead/{nroempresa}/{formulario}/{propriedade}` | Recebe um Lead do webhook da RD Station | nroempresa [path]*; formulario [path]*; propriedade [path]*; request [body]*; Authorization [header] |
| POST | `/api/rd/v2/lead/{nroempresa}/{formulario}/{propriedade}` |  | nroempresa [path]*; formulario [path]*; propriedade [path]*; request [body]*; Authorization [header] |

### vorticeadmin

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/vorticeadmin/status/send` | End-Point para recebimento de informações sobre o monitoramento dos integradores da Vórtice. | poEnt [body]*; Authorization [header] |

### vtcsaas

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/vtcsaas/evento` | Recebe um evento do SAAS e gera no CRM. | voChat [body]*; Authorization [header] |
| POST | `/api/vtcsaas/pesquisa` | Recebe uma pesquisa respondida pelo Fidelidade da Vórtice. | data [body]*; Authorization [header] |
| POST | `/api/vtcsaas/pessoa/set` | Recebe uma pessoa do SAAS e grava no CRM, nas tabelas de importação (IMP) | voPessoa [body]*; Authorization [header] |

### webhook

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/webhook/google/agenda/{cliente}/{calendarId}` | End-Point para recebimento de estimulos do Google Agenda (Google to Vortice). *Usado somente pela Vortice. | cliente [path]*; calendarId [path]*; Authorization [header] |
| POST | `/api/webhook/google/vrct/agenda/{calendarId}/{dtaBase}` | End-Point para recebimento de estimulos do Google Agenda (Vortice =&gt; Cliente). *Usado somente pelo cliente. | calendarId [path]*; dtaBase [path]*; Authorization [header] |
| POST | `/api/webhook/pessoa` |  | data [body]*; Authorization [header] |

### wifire

| Metodo | Rota | Resumo | Parametros |
|---|---|---|---|
| POST | `/api/wifire/webhook` | Webhook para receber insights do Wifire. | request [body]*; Authorization [header] |

## Diferenca em relacao a `docs/API-wsVorticeCrmApi.md` do agente

Referencia comparada: `c:\projetos\vortice-crm-agent\docs\API-wsVorticeCrmApi.md`

### Rotas presentes no Swagger e AUSENTES na doc (22)

| Metodo | Rota | Resumo |
|---|---|---|
| POST | `/api/cartao/celular/validcheck/{numero}/{codigo}` | Valida o código enviado por SMS para o número de celular informado. Será gerado um evento com a origem "CARTAO" e os possíveis código do evento são: "CELULAR VALIDADO", "CELULAR NAO VALIDADO" |
| POST | `/api/cartao/certiface/captcha` | Envia os desafios e obtem o resultado. Será gerado um evento com a origem "CERTIFACE" e os possíveis código do evento são: "BIOMETRIA APROVADA", "BIOMETRIA NEGADA" e no resultado complementar o valor do campo codID devolvido pela Certiface |
| POST | `/api/cartao/codigovalidacaocheck/{subcontexto}/{codigo}` | Valida um código enviado ao celular |
| POST | `/api/cartao/conductor/criaconta` | Cria a conta na Conductor. Será gerado evento com a Origem "CONDUCTOR" e os possíveis código de evento são: "CONTA CRIADA", "CONTA NAO CRIADA" |
| GET | `/api/cartao/formulario/comprovanteendereco` | Retorna a lista dos comprovantes de endereço possíveis de serem usados |
| POST | `/api/cartao/pessoa/foto` | Recebe a foto da pessoa em base64 |
| GET | `/api/cartao/promotor` | Obtém o promotor vinculado ao codusuario recebido |
| GET | `/api/cartao/promotores/{psSearchText}` | Obtém a lista de promotores |
| POST | `/api/certiface/facecaptcha/appkey` | Método para retornar a API Key usando a conta padrão da Certiface configurada no CRM. |
| POST | `/api/certiface/facecaptcha/captcha` | Método para enviar os desafios cumpridos. |
| POST | `/api/certiface/facecaptcha/result` | Método para obter o resultado do desafio de uma appkey. |
| POST | `/api/followize/lead/annotation` | Recebe um webhook de anotação do Lead e gera o evento "LEAD ANNOTATION" para a origem "FOLLOWIZE" |
| POST | `/api/followize/lead/create` | Recebe um webhook de criação do Lead e gera o evento "LEAD CREATE" para a origem "FOLLOWIZE" |
| POST | `/api/followize/lead/finalization` | Recebe um webhook de finalização do Lead e gera os eventos "LEAD FINALIZATION SALED", "LEAD FINALIZATION NOTSALED" para a origem "FOLLOWIZE" |
| POST | `/api/followize/lead/schedule` | Recebe um webhook de agendamento do Lead e gera o evento "LEAD SCHEDULE" para a origem "FOLLOWIZE" |
| POST | `/api/infobip/IncomingMessage` | Recebe mensagens encaminhadas pela Infobip via webhook |
| GET | `/api/portal/processo/{processo}/documento` | Obtém a lista de documentos do processo |
| POST | `/api/portal/senharecupera` | Método para recuperação de senha |
| POST | `/api/portal/signup` | Realiza um novo cadastro |
| POST | `/api/rd/lead/{nroempresa}/{formulario}/{propriedade}` | Recebe um Lead do webhook da RD Station |
| POST | `/api/vtcsaas/evento` | Recebe um evento do SAAS e gera no CRM. |
| POST | `/api/vtcsaas/pesquisa` | Recebe uma pesquisa respondida pelo Fidelidade da Vórtice. |

### Rotas citadas na doc e AUSENTES no Swagger (4)

- `/api/`
- `/api/crm/`
- `/api/followize/lead/{create`
- `/api/rd/lead/`

## Modelos (definitions)

- `AcordoAuthRequest` - `AcordoAuthResponse` - `AcordoBoletoRequest` - `AcordoProcessoGet` - `AcordoRequest` - `AcordoResponse` - `Activity` - `Address` - `AgendaDTO` - `Annotation` - `AtivMobEventoRastreio` - `Atributo` - `Attendant` - `AuthRequest` - `CertifaceAppkeyResquest` - `CertifaceCaptchaResquest` - `CertifaceWebhookRequest` - `ChatMensagem` - `CienciaPost` - `Company` - `Contact` - `contents` - `DadosFinanceirosRequest` - `DoctoDTO` - `Documento` - `Email` - `EmpModuloPerm` - `Endereco` - `Establishment` - `Evento` - `EventoRequest` - `File` - `Finalization` - `first` - `FollowizeLeadRequest` - `Form` - `GT_PessoaLog_ENT` - `HistoricoDTO` - `ImpEmail` - `ImpFormaPgto` - `ImpNfs` - `ImpNfsItem` - `ImpNsfCmpl` - `ImpOs` - `ImpOsItem` - `ImpOsSolic` - `ImpPedido` - `ImpPedidoItem` - `ImpPessoa` - `ImpPessoaContato` - `ImpPessoaFone` - `ImpProduto` - `ImpTitulo` - `ImpVeiculo` - `IncomingCall` - `Inights` - `Interests` - `last` - `Lead` - `LeadRequest` - `LinkAdicional` - `Login` - `Message` - `OptOutEmail` - `OptOutFone` - `origin` - `ParamListaRequest` - `Pessoa` - `PessoaLink` - `PessoaSearchRequest` - `PortalCadastro` - `PortalLogin` - `PortalSenhaAltera` - `Price` - `ProcessoDTO` - `ProcessoSearchRequest` - `Propriedade_ENT` - `QuestionarioPost` - `Questoes` - `Reason` - `Sale_Not_Performed_Reason` - `Salesman` - `Schedule` - `SenhaRecuperacao` - `SmsReceiveRequest` - `Team` - `Telefone` - `Tracking` - `User` - `VtcSaasEvento` - `VtcSaasPessoa`


