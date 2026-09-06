# Modelo de dados do Dynamics 365 CE / Dataverse — tabelas padrão, relacionamentos, qualificação de Lead, modelo polimórfico de Atividade, cascading, alternate keys, rollup/calculated, choices, elastic tables e Field Service, com mapeamento Vórtice → Dynamics → CRM Tracbel

> Pesquisa automatizada - workflow `crm-tracbel-pesquisa-profunda`, 30/08/2026.

## Resumo

O Dataverse resolve por METADADOS quase tudo que a Tracbel pensaria em codificar. Herança de tabela: ActivityPointer é uma tabela-base real que guarda TODA atividade (email, task, phonecall, appointment, custom); a tabela específica só guarda colunas extras. Participação polimórfica: ActivityParty com 13 tipos de ParticipationTypeMask. Lookups polimórficos de verdade: Customer (account|contact), regardingobjectid, Annotation.objectid. Integridade declarativa: 6 ações de cascata x 6 CascadeType por relacionamento, sem código. A qualificação de Lead é mais simples que a do Salesforce: QualifyLead recebe 3 booleanos (CreateAccount/CreateContact/CreateOpportunity), o Lead vai para statecode=1/statuscode=3 e continua existindo, e os registros criados apontam de volta via originatingleadid; a experiência nova permite até 5 oportunidades por lead. Os limites numéricos são o que mais vale copiar: 10 alternate keys/tabela (900 bytes, 16 colunas), 200 rollups/ambiente e 50/tabela (job de 1h, sem rollup de rollup, sem N:N), 5 calculated columns encadeadas atravessando no máximo 2 tabelas, 3.000 tabelas custom/ambiente, 20 GB por partição em elastic table, plug-in com timeout de 2 minutos. Equipamento/ativo: msdyn_customerasset tem DUAS hierarquias (ParentAsset e MasterAsset) mais FunctionalLocation, e a Work Order é cabeçalho com linhas de Incident/Product/Service/ServiceTask, agendada via Resource Requirement -> Bookable Resource Booking. Para a Tracbel: copiar ActivityPointer+ActivityParty, Customer lookup, alternate keys e cascading declarativo; simplificar rollups/calculated (fazer com view/coluna materializada); descartar elastic tables e Territory como estão.

## Entidades / objetos mapeados

### `Lead`

**Funcao:** Prospect não qualificado. Não é filho de Account/Contact; carrega dados de pessoa E de empresa no mesmo registro (firstname/lastname + companyname).

**Campos-chave:** leadid; statecode (0=Open,1=Qualified,2=Disqualified); statuscode (1=New,2=Contacted,3=Qualified,4=Lost,5=Cannot Contact,6=No Longer Interested,7=Canceled); subject; companyname; parentaccountid; parentcontactid; qualifyingopportunityid; originatingcaseid; campaignid; leadqualitycode; qualificationcomments; customerid (account|contact)

**Relacoes:** N:1 campaign, incident, account, contact, opportunity (qualifyingopportunityid); 1:N para account/contact/opportunity via originatingleadid do lado deles

### `Account`

**Funcao:** Empresa/organização cliente. Hierárquica.

**Campos-chave:** accountid; name; accountnumber (alternate key OOB confirmada na doc); parentaccountid; primarycontactid; defaultpricelevelid; websiteurl; address1_*; ownerid; owningbusinessunit

**Relacoes:** 1:N contact, opportunity, incident, annotation, activitypointer (regardingobjectid); auto-referência parentaccountid; N:1 pricelevel

### `Contact`

**Funcao:** Pessoa física. Pode ser cliente final direto (customerid aponta para contact) ou pessoa de contato de um Account.

**Campos-chave:** contactid; firstname; lastname; jobtitle; emailaddress1; mobilephone; parentcustomerid (account|contact); defaultpricelevelid; address1_*

**Relacoes:** N:1 account (parentcustomerid, polimórfico); 1:N annotation/activity/opportunity

### `Opportunity`

**Funcao:** Negociação em andamento. Núcleo do pipeline.

**Campos-chave:** opportunityid; name; statecode (0=Open,1=Won,2=Lost); statuscode (1=In Progress,2=On Hold,3=Won,4=Canceled,5=Out-Sold); customerid (account|contact); parentaccountid; parentcontactid; originatingleadid; pricelevelid; campaignid; estimatedvalue; actualvalue; totalamount; totallineitemamount; closeprobability; budgetamount; isrevenuesystemcalculated

**Relacoes:** 1:N opportunityproduct, quote; N:N competitor (intersect opportunitycompetitors); N:1 pricelevel, campaign, lead; mensagens WinOpportunity/LoseOpportunity

### `OpportunityProduct`

**Funcao:** Linha de produto da oportunidade. Suporta produto do catálogo OU write-in (texto livre).

**Campos-chave:** opportunityproductid; opportunityid (SystemRequired); productid (nullable, Targets=product); productdescription (Write-In Product, 500 chars); isproductoverridden (True=Write-In, False=Existing); ispriceoverridden; uomid; quantity (precision 5); priceperunit; baseamount; volumediscountamount (read-only); manualdiscountamount; tax; extendedamount; lineitemnumber (0..1.000.000.000); parentbundleidref (auto-referência p/ bundles); producttypecode (1=Product,2=Bundle,3=Required Bundle Product,4=Optional Bundle Product,5=Project-based Service); pricingerrorcode (39 valores 0..38); skippricecalculation (0..3)

**Relacoes:** N:1 opportunity (Delete: Cascade), product (Delete: Restrict), uom (Delete: Restrict); auto-referência de bundle (Delete: Cascade)

### `Quote`

**Funcao:** Proposta/orçamento versionado. Só quotes em Draft podem ser editadas.

**Campos-chave:** quoteid; quotenumber; revisionnumber (SystemRequired, 0..2147483647); name; customerid (Customer: account|contact, ApplicationRequired); opportunityid; pricelevelid; statecode (0=Draft,1=Active,2=Won,3=Closed); statuscode (1=In Progress[state0], 2=In Progress[state1], 3=Open, 4=Won, 5=Lost, 6=Canceled, 7=Revised); effectivefrom/effectiveto; totalamount

**Relacoes:** 1:N quotedetail; mensagens: ReviseQuote, CloseQuote, WinQuote, ConvertQuoteToSalesOrder, GenerateQuoteFromOpportunity, GetQuoteProductsFromOpportunity

### `SalesOrder / Invoice`

**Funcao:** Pedido e fatura, com linhas próprias (salesorderdetail / invoicedetail), gerados a partir de Quote.

**Campos-chave:** pricelevelid (confirmado pelos relacionamentos price_level_orders e price_level_invoices); customerid do tipo Customer

**Relacoes:** Quote -> SalesOrder via ConvertQuoteToSalesOrder; ambos N:1 pricelevel

### `Product / PriceLevel / ProductPriceLevel / UoM`

**Funcao:** Catálogo. PriceLevel (Price List) é a lista de preços com vigência; ProductPriceLevel é o item da lista.

**Campos-chave:** pricelevel: pricelevelid, name (100), begindate, enddate, transactioncurrencyid (ApplicationRequired), statecode (0=Active,1=Inactive), statuscode (100001=Active,100002=Inactive), OwnershipType=OrganizationOwned; product: pricelevelid (lista default)

**Relacoes:** pricelevel 1:N productpricelevel, opportunity, quote, salesorder, invoice, product, account.defaultpricelevelid, contact.defaultpricelevelid, campaign.pricelistid

### `ActivityPointer (Activity)`

**Funcao:** TABELA-BASE de toda atividade. Toda criação de email/task/phonecall/appointment/atividade custom gera 1 linha aqui. É a view unificada da timeline.

**Campos-chave:** activityid; activitytypecode (tipo EntityName, READ-ONLY, SystemRequired); subject; description (guarda inclusive o corpo do e-mail); regardingobjectid (lookup polimórfico); statecode/statuscode; scheduledstart/scheduledend; actualstart/actualend; ownerid; allparties (tipo PartyList, read-only, Targets: account, contact, queue, systemuser, team); isregularactivity; instancetypecode

**Relacoes:** 1:1 com cada tabela de atividade (activity_pointer_email, _task, _phonecall, _appointment, _fax, _letter, _chat, _socialactivity, _recurringappointmentmaster); N:1 regardingobjectid para account, contact, knowledgearticle, businessunit e demais tabelas com HasActivities

### `ActivityParty`

**Funcao:** Participante de uma atividade. Modela N:N TIPADO entre atividade e (account|contact|systemuser|team|queue).

**Campos-chave:** activitypartyid; activityid; partyid; participationtypemask (1=Sender, 2=ToRecipient, 3=CCRecipient, 4=BccRecipient, 5=RequiredAttendee, 6=OptionalAttendee, 7=Organizer, 8=Regarding, 9=Owner, 10=Resource, 11=Customer, 12=ChatParticipant, 13=Related); addressused

**Relacoes:** N:1 activitypointer (activitypointer_activity_parties). Ao deletar a atividade, as parties são deletadas; ao deletar um contact, as parties NÃO são deletadas (preserva histórico)

### `Annotation (Note)`

**Funcao:** Nota/anexo anexado polimorficamente a um registro.

**Campos-chave:** annotationid; subject (500); notetext (Memo, RichText, MaxLength 100000); isdocument; documentbody (String, MaxLength 1.073.741.823); filename (255); filesize (Integer, max 1.000.000.000); mimetype (256); objectid (lookup polimórfico); objectidtypecode; ownerid (systemuser|team)

**Relacoes:** objectid Targets = lista FECHADA e curada (account, contact, appointment, email, task, phonecall, letter, fax, knowledgearticle, kbarticle, goal, sla, chat, mailbox, workflow, sharepointdocument, etc.). Cascade Delete: Cascade a partir de account/contact

### `Connection / ConnectionRole`

**Funcao:** Relacionamento INFORMAL entre dois registros quaisquer, com papel de cada lado. Alternativa a criar relacionamento formal.

**Campos-chave:** connection: connectionid; record1id e record2id (lookups com ~25 Targets incluindo account, contact, systemuser, team, territory, opportunity via activitypointer, position, goal, socialprofile); record1roleid/record2roleid (-> connectionrole); relatedconnectionid (conexão recíproca, ApplicationRequired); ismaster (SystemRequired, read-only). connectionrole: name (100), category (1=Business, 2=Family, 3=Social, 4=Sales, 5=Other, 1000=Stakeholder, 1001=Sales Team, 1002=Service), OwnershipType=OrganizationOwned

**Relacoes:** connectionrole tem N:N auto-referente connectionroleassociation_association (papéis recíprocos permitidos)

### `Territory`

**Funcao:** Região de vendas. Hierárquica.

**Campos-chave:** territoryid; name (200); managerid (-> systemuser); parentterritoryid (auto-referência, Delete=RemoveLink); OwnershipType=OrganizationOwned

**Relacoes:** 1:N systemuser (systemuser.territoryid); auto-referência hierárquica. Na tabela base do Dataverse NÃO aparece relacionamento direto com account — esse vínculo vem da solução Sales

### `SystemUser / Team / BusinessUnit`

**Funcao:** Modelo de segurança. Todo registro user-owned tem ownerid (systemuser|team), owninguser, owningteam e owningbusinessunit.

**Campos-chave:** ownerid tipo Owner com Targets=systemuser,team; owningbusinessunit; systemuser.territoryid; businessunit é hierárquica com uma única raiz por ambiente

**Relacoes:** BU 1:N team, systemuser; team pode ser Owning Team (possui registros) ou Access Team (só recebe compartilhamento, não possui nem tem security role)

### `Incident (Case)`

**Funcao:** Chamado/ocorrência de serviço.

**Campos-chave:** incidentid; title (200, obrigatório); ticketnumber (100, não editável); customerid (Customer: account|contact, obrigatório); primarycontactid; parentcaseid; masterid (caso primário de merge); casetypecode (1=Question,2=Problem,3=Request); caseorigincode (1=Phone,2=Email,3=Web,2483=Facebook,3986=Twitter,700610000=IoT); prioritycode (1=High,2=Normal,3=Low); statecode (0=Active,1=Resolved,2=Cancelled); statuscode (1=In Progress,2=On Hold,3=Waiting for Details,4=Researching,5=Problem Solved,6=Cancelled,1000=Information Provided,2000=Merged); entitlementid; slaid; productid; productserialnumber

**Relacoes:** N:1 account/contact (Customer), entitlement, contract, sla, product, subject, kbarticle; auto-referência parentcaseid e masterid; mensagens CloseIncident, Merge, ApplyRoutingRule, CalculateTotalTimeIncident

### `Campaign / Competitor`

**Funcao:** Campanha de marketing e concorrente.

**Campos-chave:** campaign.pricelistid -> pricelevel; competitor ligado a opportunity por N:N

**Relacoes:** opportunity N:N competitor (intersect opportunitycompetitors); competitor 1:N opportunityclose; campaign 1:N campaignactivity / campaignresponse (ambas são ATIVIDADES, herdam de ActivityPointer)

### `msdyn_customerasset (Customer Asset)`

**Funcao:** Equipamento/ativo do cliente instalado em campo. É o análogo direto do equipamento John Deere na base da Tracbel.

**Campos-chave:** msdyn_customerassetid; msdyn_name; msdyn_Account (-> account, 'Parent Customer of this Asset'); msdyn_ParentAsset (-> msdyn_customerasset); msdyn_MasterAsset (-> msdyn_customerasset, DisplayName 'Top-Level Asset'); msdyn_Product (-> product); msdyn_FunctionalLocation (-> msdyn_functionallocation); msdyn_CustomerAssetCategory (-> msdyn_customerassetcategory); msdyn_AssetTag (100); msdyn_ManufacturingDate; msdyn_Latitude/msdyn_Longitude; msdyn_DeviceId (IoT); msdyn_RegistrationStatus; msdyn_WorkOrderProduct (ativo auto-criado a partir de uma linha de produto de OS); statecode/statuscode; ownerid

**Relacoes:** DUAS hierarquias simultâneas (ParentAsset = pai imediato; MasterAsset = topo da árvore, evita recursão em query). 1:N para msdyn_workorder, msdyn_workorderincident, msdyn_workorderproduct, msdyn_workorderservice, msdyn_workorderservicetask, msdyn_workorderresolution, msdyn_rmaproduct, msdyn_inspectioninstance, msdyn_entitlementapplication e as 4 tabelas de agreementbooking*

### `msdyn_workorder (Work Order)`

**Funcao:** Ordem de serviço: cabeçalho do trabalho a executar.

**Campos-chave:** work order type, status/substatus, duration, priority, Service Account (-> account), Billing Account, msdyn_customerasset, functional location, price list/tax

**Relacoes:** 1:N msdyn_workorderincident (pacote de trabalho), msdyn_workorderproduct (peças), msdyn_workorderservice (mão de obra), msdyn_workorderservicetask (checklist). Gera automaticamente msdyn_resourcerequirement, que ao ser agendado cria bookableresourcebooking. Fechamento gera invoice + actuals + consumo de estoque

### `msdyn_incidenttype (Incident Type)`

**Funcao:** TEMPLATE de trabalho: pacote de service tasks, produtos, serviços e characteristics/skills recomendados.

**Campos-chave:** aplicado a uma Work Order via msdyn_workorderincident, que explode as linhas filhas

**Relacoes:** 1:N msdyn_workorderincident

### `bookableresource / bookableresourcebooking / msdyn_resourcerequirement`

**Funcao:** Agendamento genérico (não só de OS: serve para case, opportunity e tabelas custom).

**Campos-chave:** bookableresource = pessoa, contratado, EQUIPAMENTO ou instalação; booking = slot de tempo de um recurso; requirement = o que precisa ser agendado; booking status; booking timestamps -> booking journals (base do custo de mão de obra)

**Relacoes:** requirement 1:N booking (um requirement pode ser agendado várias vezes / para vários recursos)

### `msdyn_agreement (Service Agreement)`

**Funcao:** Contrato de manutenção que gera ordens de serviço recorrentes.

**Campos-chave:** vinculado a UM service account; as OS geradas herdam esse account

**Relacoes:** 1:N msdyn_agreementbookingincident/product/service/servicetask, todas com lookup para msdyn_customerasset

## Achados

### 1. Qualificação de Lead no Dynamics: 3 booleanos, o Lead SOBREVIVE, e o link é originatingleadid

A mensagem QualifyLead (SDK: QualifyLeadRequest; Web API: action QualifyLead) tem exatamente estas propriedades: LeadId (obrigatório), CreateAccount (bool, obrigatório), CreateContact (bool, obrigatório), CreateOpportunity (bool, obrigatório), OpportunityCurrencyId (obrigatório), OpportunityCustomerId (account ou contact, obrigatório), SourceCampaignId (obrigatório), Status (statuscode do lead; 3 = Qualified) e ProcessInstanceId (opcional — a instância do business process flow do Lead que deve ser transferida para a Opportunity). O Lead passa a statecode=1 (Qualified) / statuscode=3 (Qualified) e CONTINUA EXISTINDO: aparece na view 'Closed Leads' e pode ser reativado. Os registros criados apontam de volta para ele por originatingleadid (opportunity.originatingleadid é lookup para lead), e o lead aponta para a oportunidade por qualifyingopportunityid. Regra de negócio documentada: se CreateAccount=true E CreateContact=true, o account criado vira o pai do contact (contact.parentcustomerid) e o contact vira account.primarycontactid. Desqualificar usa statecode=2 com statuscode 4=Lost, 5=Cannot Contact, 6=No Longer Interested, 7=Canceled — e só é permitido se NÃO houver oportunidade associada. Notas e anexos do lead passam a ser exibidos no registro de Opportunity para não se perder informação.

**Evidencia:** https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.qualifyleadrequest ; https://learn.microsoft.com/en-us/dynamics365/developer/reference/entities/lead ; https://learn.microsoft.com/en-us/dynamics365/sales/faq-lead

**Licao para a Tracbel:** COPIAR o desenho. No CRM Tracbel, 'Qualificar' deve ser um comando de aplicação (um endpoint POST /leads/{id}/qualify com um DTO de 3-4 flags), não uma tela que o vendedor preenche à mão. Mantenha o Lead vivo com status Qualificado — é o registro de auditoria de origem (essencial para medir conversão por canal/campanha, coisa que hoje o Vórtice não entrega). Grave originatingLeadId em Cliente, Contato e Oportunidade: isso resolve rastreabilidade de ponta a ponta com 3 colunas e sem tabela de log.

### 2. Diferenças concretas Dynamics x Salesforce na conversão de Lead

(a) O Salesforce faz Lead Conversion e o Lead vira read-only/IsConverted, com ConvertedAccountId/ConvertedContactId/ConvertedOpportunityId. O Dynamics usa statecode/statuscode normais e mantém o registro editável após reativar. (b) O Dynamics permite criar ATÉ 5 OPORTUNIDADES a partir de um único lead (setting 'Let seller create up to 5 opportunities from a newly qualified lead'), coisa que o Salesforce não faz nativamente — mas o business process flow só acompanha a oportunidade PRIMÁRIA; para mudar qual é a primária o vendedor tem de voltar no estágio Qualify e usar Set Active/Next Stage. (c) O admin escolhe, por TIPO DE REGISTRO, quem cria: 'Automatic' (sistema) ou 'Seller' (vendedor escolhe criar novo, usar existente, ou não criar). (d) O motor de duplicidade do Sales é diferente do da Power Platform: a Power Platform compara matchcode; o Dynamics 365 Sales usa um modelo de IA com lógica fuzzy (nome do lead + nome da empresa parecidos; nome do lead + mesmo domínio de e-mail) ALÉM do matchcode (e-mail, telefone). (e) Se houver regra de duplicidade publicada e existir duplicata, a qualificação FALHA com erro — a mitigação documentada é marcar 'Exclude inactive matching records' na regra.

**Evidencia:** https://learn.microsoft.com/en-us/dynamics365/sales/qualify-lead-convert-opportunity-sales ; https://learn.microsoft.com/en-us/dynamics365/sales/faq-lead ; https://learn.microsoft.com/en-us/dynamics365/sales/define-lead-qualification-experience

**Licao para a Tracbel:** COPIAR o 'até N oportunidades por lead' — na venda de máquina agrícola um mesmo lead vira facilmente 2-3 negócios (trator + plantadeira + implemento) e forçar 1:1 empurra o vendedor para o campo texto. DESCARTAR o acoplamento fluxo-oportunidade-primária: se você fizer fluxo por oportunidade desde o início, não tem esse problema. COPIAR a ideia de deixar a política de criação configurável por tipo de registro (tabela de configuração, não if no código). ATENÇÃO: nunca deixe a regra de duplicidade BLOQUEAR a qualificação — no Dynamics isso é fonte conhecida de chamado; no CRM Tracbel a duplicidade deve avisar e oferecer merge, nunca abortar.

### 3. Field maps Lead → Account/Contact/Opportunity: pequenos, fixos e com um bug histórico

O mapeamento OOB é surpreendentemente enxuto. Para Opportunity só 3 campos: subject→name, parentcontactid→parentcontactid, parentaccountid→parentaccountid. Para Account 10 campos: companyname→name, websiteurl→websiteurl, telephone1→address1_telephone1, address1_line1/2/3, address1_city, address1_stateorprovince, address1_postalcode, address1_country. Para Contact 13: firstname, lastname, jobtitle, telephone1→managerphone (sic — o Business Phone do lead cai no campo 'Manager Phone' do contato, mapeamento OOB documentado), mobilephone, emailaddress1 e os 7 de endereço. Desde março/2024 NÃO é mais possível deletar field maps OOB; o override oficial é um plug-in post-operation no Create de Opportunity que checa se originatingleadid está preenchido e reescreve os campos.

**Evidencia:** https://learn.microsoft.com/en-us/dynamics365/sales/define-lead-qualification-experience (seção 'Field mappings to other entities')

**Licao para a Tracbel:** COPIAR o conceito de field map como DADO (uma tabela SourceEntity/SourceField/TargetEntity/TargetField), não como código — é literalmente o requisito 'extensibilidade sem release': o time adiciona um campo novo no lead e mapeia por tela. EVITAR o erro da Microsoft: se o map é dado, ele tem de ser editável/deletável pelo admin; travar map OOB e mandar escrever plug-in para sobrescrever é dívida técnica pura. Note também que 90% do valor está em ~13 campos — não gaste sprint construindo mapeador universal.

### 4. ActivityPointer é HERANÇA DE TABELA REAL, não uma view

Toda atividade grava fisicamente em ActivityPointerBase (a tabela-base) MAIS uma tabela de extensão dedicada por tipo (EmailBase, TaskBase, AppointmentBase, PhoneCallBase...). A doc é explícita: 'most of these columns are stored in the base table (ActivityPointerBase), and only custom columns, if any, will be stored in the EmailBase table'. A tabela de extensão só tem linhas se existir coluna customizada naquele tipo. O corpo do e-mail fica na coluna description do ActivityPointerBase (mesma coluna usada como descrição de um appointment) — por isso ActivityPointerBase costuma ser o maior consumidor de storage do ambiente. activitytypecode é do tipo EntityName, SystemRequired e READ-ONLY (IsValidForCreate=false e IsValidForUpdate=false): você nunca escreve o discriminador, o motor escreve. A coluna allparties é do tipo PartyList com Targets = account, contact, queue, systemuser, team. A segurança das atividades é herdada do ActivityPointer: privilégio é definido uma vez para 'Activity' e vale para todos os tipos.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/activities-data-model-storage ; https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/activitypointer ; https://learn.microsoft.com/en-us/power-apps/developer/data-platform/activity-entities

**Licao para a Tracbel:** COPIAR direto em EF Core: use Table-Per-Type (TPT) ou Table-Per-Hierarchy com discriminador para Atividade. Uma tabela Atividade (Id, TipoAtividadeId, Assunto, Descricao, RegardingTipo, RegardingId, DataAgendada, DataRealizada, Status, ResponsavelId, ...) + tabelas filhas Ligacao/Visita/Email/Tarefa. Isso dá a Timeline unificada de graça — a coisa que o Vórtice não tem e que os 130 usuários mais vão perceber. LIÇÃO NEGATIVA: NÃO ponha o corpo do e-mail/texto longo na tabela base (é o erro da Microsoft com description). Coloque texto grande numa tabela satélite AtividadeConteudo(AtividadeId, Corpo) ou coluna com armazenamento fora de linha; a tabela base tem de ser magra porque ela é lida em toda listagem.

### 5. ActivityParty: 13 tipos de participação resolvem N:N tipado e polimórfico de uma vez

ActivityParty.ParticipationTypeMask: 1=Sender, 2=ToRecipient, 3=CCRecipient, 4=BccRecipient, 5=RequiredAttendee, 6=OptionalAttendee, 7=Organizer, 8=Regarding, 9=Owner, 10=Resource, 11=Customer, 12=ChatParticipant, 13=Related. Cada tipo de atividade só aceita um subconjunto — Appointment aceita 5/6/7; Email aceita 1/2/3/4/13; PhoneCall e Fax aceitam só 1/2; Letter aceita 1/2/4; ServiceAppointment aceita 11 e 10; CampaignResponse aceita 11; Chat aceita 12. UMA ATIVIDADE CUSTOM ACEITA TODOS OS 13 automaticamente. A coluna AddressUsed guarda qual endereço de e-mail daquele party foi de fato usado. Exemplo de cardinalidade real citado na doc: um Appointment com 1 organizer, 1 owner, 2 required, 1 optional e 1 regarding gera 6 linhas em ActivityPartyBase. Regra de integridade importante: deletar um Contact NÃO deleta as ActivityParty que o referenciam (preserva quem participou), mas deletar a Atividade deleta todas as suas parties.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/activityparty-entity ; https://learn.microsoft.com/en-us/power-apps/developer/data-platform/activities-data-model-storage

**Licao para a Tracbel:** COPIAR o padrão inteiro: AtividadeParticipante(AtividadeId, TipoParticipacao, ParticipanteTipo, ParticipanteId, EnderecoUsado). Uma única tabela substitui 'de', 'para', 'cc', 'participantes', 'responsável', 'recurso' — e em revenda de máquina isso é essencial porque uma visita técnica tem vendedor + técnico + cliente + representante da fábrica. SIMPLIFICAR: comece com 6 tipos (Remetente, Destinatario, Copia, Participante, Responsavel, Cliente) e deixe o enum extensível por tabela de domínio; não replique os 13 sem necessidade. COPIAR TAMBÉM a regra de não deletar participante quando a pessoa é deletada — é auditoria.

### 6. regardingobjectid: o lookup polimórfico que constrói a Timeline

ActivityPointer.regardingobjectid é um lookup cujos Targets são todas as tabelas que declaram HasActivities=true (account, contact, knowledgearticle, knowledgebaserecord, businessunit, e todas as custom habilitadas). Cada target é implementado como um relacionamento 1:N SEPARADO com sua própria navigation property (regardingobjectid_account, regardingobjectid_contact, ...). Ou seja: por baixo não existe 'um' relacionamento polimórfico, existem N relacionamentos 1:N compartilhando a MESMA coluna física. Isso é o que permite consultar 'todas as atividades deste cliente' com FK real e índice, em vez de scan por objectTypeCode+objectId. A doc de relacionamentos deixa explícito que 'any activity table has a similar set of parental table relationships for tables that can be associated using the regarding lookup column' — cada um desses é um relacionamento PARENTAL.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/activitypointer (seção Many-to-One relationships) ; https://learn.microsoft.com/en-us/power-apps/maker/data-platform/create-edit-entity-relationships

**Licao para a Tracbel:** COPIAR com adaptação para SQL Server + EF Core: em vez de N FKs, use o par (RegardingTipo tinyint/smallint, RegardingId uniqueidentifier) com índice composto (RegardingTipo, RegardingId, DataAgendada DESC) — é a query da Timeline. Se quiser FK real, crie uma tabela RegardingRegistro(Id, Tipo) como 'supertipo' e aponte todas as FKs para ela. DESCARTAR a abordagem de N navigation properties no EF: com 2-3 devs isso vira inferno de manutenção; use um discriminador + resolver no repositório e exponha a Timeline por um endpoint único GET /timeline?tipo=X&id=Y.

### 7. Atividades customizadas: o que se ganha de graça e o que é irreversível

Criar uma atividade custom = criar tabela com EntityMetadata.IsActivity=true. Ao fazer isso você herda TODAS as propriedades e privilégios do activitypointer e TODOS os 13 activity party types. Restrições rígidas documentadas: (1) a primary column é obrigatoriamente chamada 'Subject' — você não escolhe; (2) IsAvailableOffline deve ser true; (3) IsMailMergeEnabled deve ser false; (4) OwnershipType só UserOwned ou TeamOwned (nunca OrganizationOwned); (5) HasActivities deve ser false (uma atividade não tem atividades filhas); (6) HasNotes deve ser true; (7) ActivityTypeMask (0=None, 1=Communication Activity) só pode ser setado NA CRIAÇÃO e NUNCA MAIS pode ser alterado — 0 esconde do menu de atividades, 1 (default) mostra.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/custom-activities

**Licao para a Tracbel:** COPIAR a ideia de 'tipo de atividade cadastrável' — é exatamente o requisito de extensibilidade sem release: o time cria 'Demonstração de Campo', 'Entrega Técnica', 'Revisão de 250h' como TIPOS DE ATIVIDADE em tabela, com um JSON/schema de campos extras, sem recompilar. LIÇÃO NEGATIVA CLARA: não replique metadados imutáveis (ActivityTypeMask fixo na criação). Toda propriedade de configuração no CRM Tracbel deve ser alterável; propriedade imutável obriga recriar registro, e recriar registro perde histórico.

### 8. Só existem DOIS tipos de relacionamento: 1:N e N:N — N:1 é ponto de vista

A doc é explícita: 'When you view relationships in Power Apps, you might think that there are three types of table relationships. Actually there are only two'. N:1 é apenas o 1:N visto do lado da tabela referenciadora. Adicionar uma coluna lookup JÁ CRIA um relacionamento 1:N. N:N usa uma intersect table (relationship table) que só guarda as duas chaves — e a doc afirma explicitamente: 'You can't add custom columns to a relationship table and it's never visible in the user interface'. Exemplo OOB real: opportunity N:N competitor. A doc também orienta QUANDO NÃO usar relacionamento: para vínculos informais (contato é casado com outro contato; contato já trabalhou em outra conta) use CONNECTION, porque criar relacionamento formal para cada nuance polui o modelo.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/maker/data-platform/create-edit-entity-relationships ; https://learn.microsoft.com/en-us/power-apps/maker/data-platform/create-edit-nn-relationships

**Licao para a Tracbel:** COPIAR a disciplina: relacionamento formal só para o que gera relatório e regra de negócio. DESCARTAR o N:N nativo sem colunas — no CRM Tracbel SEMPRE modele o N:N como entidade explícita (ex.: ClienteEquipamento com DataInstalacao, Situacao, NumeroSerie), porque 100% das vezes o negócio vai pedir um atributo no vínculo. O erro de projeto da Microsoft (intersect table imutável e invisível) obriga migração dolorosa quando o requisito chega. Em EF Core: sempre entidade de junção com chave própria, nunca skip navigation pura.

### 9. Cascading behaviors: 6 ações × 6 CascadeType, com opções válidas restritas por ação

CascadeConfiguration tem as propriedades Assign, Delete, Merge, Reparent, Share, Unshare (mais RollupView e Archive nas definições geradas). Os CascadeType são: Active (Cascade Active — só registros com statecode ativo), Cascade (Cascade All), NoCascade (Cascade None), RemoveLink (limpa a FK dos filhos), Restrict (impede deletar o pai se houver filhos), UserOwned (Cascade User Owned — só filhos do mesmo dono). Opções VÁLIDAS por ação: Assign/Reparent/Share/Unshare aceitam Active|Cascade|NoCascade|UserOwned; DELETE aceita SOMENTE Cascade|RemoveLink|Restrict; MERGE aceita somente Cascade|NoCascade. Existe uma tabela oficial de quais statecodes contam como 'Active' para cascata: 0 para Account, Contact, Email, Fax, Incident, Invoice, Lead, Letter, Opportunity, PhoneCall, SalesOrder, Task, CampaignResponse, BulkOperation, IncidentResolution, OpportunityClose, OrderClose e TODAS as tabelas custom; 1 para Quote; 2 para Contract; 3 para Appointment, ServiceAppointment e RecurringAppointmentMaster. A ação Reparent é a irmã do Share só que para direitos HERDADOS: ao mudar a FK de um relacionamento parental, os direitos ReadAccess, WriteAccess, DeleteAccess, AssignAccess, ShareAccess, AppendAccess e AppendToAccess podem mudar de escopo — CreateAccess não muda. Cascatas que falham têm hooks: mensagens cascadeAsync_FailureAPI e cascadeAsync_SuccessAPI, nas quais se registra plug-in assíncrono post-operation.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/configure-entity-relationship-cascading-behavior

**Licao para a Tracbel:** COPIAR como METADADO, não como código: uma tabela RelacionamentoComportamento(RelacionamentoId, Acao, Comportamento) lida pelo repositório antes de Delete/Assign/Share. Isso mata dezenas de if espalhados e é auditável. SIMPLIFICAR forte: implemente só 3 ações (Excluir, Transferir/Assign, Compartilhar) e 4 comportamentos (Cascatear, CascatearAtivos, RemoverVinculo, Restringir). DESCARTAR Merge/Unshare/RollupView no v1. COPIAR também os hooks de falha de cascata assíncrona — cascata em massa numa base de 130 usuários com anos de histórico VAI falhar em algum ponto e você precisa de log e reprocessamento, não de silêncio.

### 10. A regra do relacionamento parental único e as limitações que ela impõe

Um relacionamento é PARENTAL se qualquer uma destas condições for verdadeira: Assign/Reparent/Share/Unshare ∈ {Cascade All, Cascade User-owned, Cascade Active} OU Delete = Cascade All. É NÃO-parental se Assign/Reparent/Share/Unshare = Cascade None e Delete ∈ {RemoveLink, Restrict}. Consequências documentadas: (1) 'usually only one of those relationships can be considered a parental table relationship' entre um par de tabelas; (2) 'A custom table can't be the primary table in a relationship with a related system table that cascades' — tabela custom não pode ser pai cascateante de tabela do sistema; (3) 'No new relationship can have any action set to Cascade All, Cascade Active, or Cascade User-Owned if the related table in that relationship already exists as a related table in another relationship that has any action set to Cascade All/Active/User-Owned' — ou seja, o motor PROÍBE multi-parent cascateante. A exceção elegante: um lookup Customer (account|contact) conta como DOIS relacionamentos parentais 1:N separados, porque cada target é seu próprio relacionamento. Quando você troca Reparent/Share de cascata para No Cascade, roda um system job de 'Inherited access rights cleanup' que revoga só o acesso herdado, preservando quem tinha acesso direto.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/maker/data-platform/create-edit-entity-relationships (seções 'Parental table relationships' e 'Limitations on behaviors you can set')

**Licao para a Tracbel:** COPIAR a REGRA, não a implementação: no CRM Tracbel, declare no modelo qual é o 'dono hierárquico' de cada entidade filha e proíba dois pais cascateantes — sem isso, cascata vira grafo com ciclo e delete de um cliente derruba o banco. Escreva isso como TESTE (um teste de arquitetura que varre o metamodelo e falha se houver dois relacionamentos parentais para a mesma entidade filha). COPIAR o cleanup de acesso herdado: se você implementar herança de permissão por hierarquia (carteira/CEN), precisa de um job que limpe permissões herdadas quando o vínculo muda — é exatamente o problema real 'gerente não vê carteira de pedidos' já documentado na Tracbel.

### 11. Customer lookup e multi-table lookup: como o Dataverse faz polimorfismo de verdade

Customer é um tipo estático de multi-table lookup que aponta para account OU contact. Aparece em opportunity.customerid, quote.customerid (ApplicationRequired), incident.customerid (obrigatório), salesorder, invoice, lead.customerid. Mecânica: cada target é um relacionamento 1:N separado com navigation property própria; ao ler, você pede _campo_value e liga as annotations OData: FormattedValue (nome do registro), associatednavigationproperty (qual navigation) e Microsoft.Dynamics.CRM.lookuplogicalname (o LOGICAL NAME da tabela do registro apontado). Ao gravar, usa-se @odata.bind com o nome da navigation property, que é CASE SENSITIVE. Multi-table lookups customizados existem: action CreatePolymorphicLookupAttribute (SDK: CreatePolymorphicLookupAttributeRequest), que recebe um array OneToManyRelationships (um por tabela alvo, cada um com sua CascadeConfiguration própria) e a definição do Lookup. Limitação de UI declarada: 'You can create and modify custom multi-table lookups through the SDK for .NET or Dataverse Web API. Interactive user interface support will be available in a future release.' Limitação séria em calculated columns: 'You can't access multi-table lookup columns like Customer, which can be Account or Contact' — daí existirem parentaccountid e parentcontactid separados ao lado de customerid.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/webapi/multitable-lookup ; https://learn.microsoft.com/en-us/dynamics365/developer/reference/entities/quote ; https://learn.microsoft.com/en-us/power-apps/maker/data-platform/define-calculated-fields

**Licao para a Tracbel:** COPIAR o tipo Customer: em revenda agrícola o cliente é ora CNPJ (fazenda/grupo), ora CPF (produtor pessoa física) — o Vórtice resolve isso com uma tabela de pessoa única (IVS_Pes), e essa é na verdade uma solução MELHOR que a do Dynamics. RECOMENDAÇÃO: adote o modelo do Vórtice (uma entidade Pessoa com TipoPessoa PF/PJ) em vez do par Account/Contact do Dynamics — você elimina o polimorfismo Customer inteiro e todas as suas limitações (não funciona em calculated column, exige duas FKs redundantes). Guarde o polimorfismo genuíno só onde ele é inevitável: Atividade.Regarding, Anexo.Regarding e Conexão.

### 12. Alternate Keys: os limites exatos (10 chaves, 900 bytes, 16 colunas) e os caracteres proibidos

Limites oficiais: até DEZ alternate key definitions por tabela; o índice é validado contra as restrições do SQL — 900 BYTES por chave e 16 COLUNAS por chave. Tipos de coluna permitidos: Decimal, Whole Number (Integer), Single line of text (String), DateTime, Lookup e Option Set (Picklist). Colunas com field-level security NÃO podem entrar. Alternate keys NÃO são suportadas em virtual tables (o sistema não consegue garantir unicidade em dados externos). Armadilha documentada: se o valor de uma coluna da chave contiver /, <, >, *, %, &, :, \\, ? ou +, então GET/update/upsert (PATCH) via API NÃO FUNCIONAM — a unicidade vale, mas a chave fica inutilizável para integração. O índice é criado por job assíncrono (EntityKeyMetadata.AsyncJob) com EntityKeyIndexStatus em Pending/In Progress/Active/Failed, e há a ação ReactivateEntityKey para retentar. Exemplo OOB real citado na doc: Account tem alternate key em accountnumber.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/define-alternate-keys-entity

**Licao para a Tracbel:** COPIAR INTEGRALMENTE — este é o recurso mais valioso da lista para a Tracbel. É exatamente o que resolve integração com TOTVS/JDE e com o legado Vórtice sem tabela De/Para: crie unique index em (CodigoTotvs), (CodigoJde), (CnpjCpf), (CodigoVortice) nas entidades centrais e exponha UPSERT por chave alternativa nas APIs (PUT /clientes/by-cnpj/{cnpj}). Sem isso, todo integrador vai precisar de um SELECT prévio para achar o Id. ADOTAR a regra de sanitização: proibir na chave de negócio qualquer caractere que quebre URL — a Microsoft documenta a dor exata que você evitaria. Em .NET 8/EF Core isso é HasAlternateKey + índice único filtrado.

### 13. Rollup columns: números duros e uma lista longa de coisas que NÃO funcionam

Funções: SUM, COUNT, MIN, MAX, AVG. Limites default: 200 rollup columns POR AMBIENTE e 50 POR TABELA (ajustáveis nas colunas MaxRollupFieldsPerOrg e MaxRollupFieldsPerEntity da tabela Organization, com teto nos mesmos 200/50). Aviso explícito da Microsoft: 'Having more than 100 rollup columns for an environment might result in degraded performance'. Cada rollup cria DUAS colunas acessórias: <nome>_date (DateTime) e <nome>_state (Integer) com valores 0=NotCalculated, 1=Calculated, 2=OverflowError, 3=OtherError, 4=RetryLimitExceeded, 5=HierarchicalRecursionLimitReached, 6=LoopDetected, 7=CurrencyMissing. Dois jobs: 'Mass Calculate Rollup Field' (por coluna, roda 12 HORAS após criar/alterar a coluna, e se nunca mais alterarem, só roda de novo em 10 ANOS — comportamento by design) e 'Calculate Rollup Field' (um por TABELA, incremental, recorrência mínima default de 1 HORA). Recálculo manual no formulário: máximo 50.000 linhas relacionadas e profundidade de hierarquia máxima 10 (esses dois limites NÃO se aplicam ao job automático). NÃO funciona: rollup sobre rollup; rollup referenciando calculated column que usa outra calculated column; rollup sobre N:N; rollup sobre o 1:N de Activity ou de ActivityParty; workflow disparado por atualização de rollup; wait condition em rollup. O rollup é sempre calculado no contexto de system user — TODOS veem o mesmo número (controla-se só por column-level security). Precisão é ARREDONDADA ANTES de agregar (exemplo oficial: 1000,0041 + 2000,0044 com rollup de 2 casas = 3000,00). E há a pegadinha do grid: o grid de Cases no formulário de Account mostra casos ligados ao account E ao contato dele; o rollup só conta os do relacionamento declarado — os números não batem e isso é by design.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/maker/data-platform/define-rollup-fields

**Licao para a Tracbel:** NÃO COPIAR o mecanismo. Com SQL Server + EF Core e 2-3 devs, rollup assíncrono de 1 hora é o pior dos mundos: número errado na tela e ninguém sabe por quê. FAÇA: (a) agregados em tempo real via view indexada ou consulta com índice adequado quando o volume permite; (b) para o que for pesado, coluna materializada atualizada por trigger/handler de domínio no MESMO commit, com job noturno de reconciliação. COPIAR SIM duas ideias: a coluna acompanhante de status de cálculo (_state/_date) para diagnosticar divergência, e o alerta de que agregado calculado no contexto do sistema IGNORA permissão — decida conscientemente se 'total da carteira' que o vendedor vê inclui o que ele não pode abrir.

### 14. Calculated columns: 5 encadeadas, 2 tabelas de alcance, sem lookup polimórfico

Tipos suportados: Text, Choice, Yes/No, Whole Number, Decimal, Currency, DateTime. Funções disponíveis: ADDHOURS/DAYS/WEEKS/MONTHS/YEARS, SUBTRACTHOURS/DAYS/WEEKS/MONTHS/YEARS, DIFFINDAYS/HOURS/MINUTES/MONTHS/WEEKS/YEARS, CONCAT, TRIMLEFT, TRIMRIGHT. Limites: máximo de 5 CALCULATED COLUMNS ENCADEADAS; saved queries, gráficos e visualizações suportam no máximo 50 calculated columns únicas; a coluna só atravessa DUAS TABELAS (a atual + a pai via lookup, sintaxe ParentAccountId.AccountNumber) e é proibido encadear calculated de terceira tabela. Não pode referenciar a si mesma nem formar ciclo. Não acessa lookup multi-tabela (Customer). Não dispara workflow nem plug-in. Não pode converter coluna simples existente em calculada. Duplicate detection não roda sobre calculated. Ordenação é DESABILITADA quando a calculated usa coluna de registro pai, coluna lógica (endereço), outra calculated, ou Now(). Bug de UX documentado: se você trocar um operador OR por AND numa cláusula com múltiplas condições, TODOS os operadores da cláusula mudam junto. Em moeda, o cálculo usa sempre a coluna _Base convertida pela taxa do registro atual.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/maker/data-platform/define-calculated-fields

**Licao para a Tracbel:** SIMPLIFICAR. Em .NET 8 você tem computed columns do SQL Server (PERSISTED, indexáveis) e propriedades calculadas no domínio — ambas superiores a esse motor. USE: computed column PERSISTED quando precisa filtrar/ordenar/indexar; propriedade C# quando é só apresentação. COPIAR SÓ o limite de profundidade: proíba por convenção que uma coluna calculada dependa de outra calculada com mais de 2 níveis, e proíba atravessar mais de uma FK — é o que impede a explosão de JOIN que nenhum dev júnior vai enxergar. E anote a lição de UX: coluna calculada não ordenável é reclamação garantida do usuário final; se o campo aparece em grid, ele precisa ser persistido.

### 15. Choice/Choices global vs local: quando substituem tabela de domínio e quando NÃO

Choice (picklist single-select) e Choices (multi-select) podem usar um conjunto definido LOCALMENTE (só naquela coluna) ou GLOBALMENTE (reutilizável por várias colunas, e no nível do ambiente). A orientação oficial é bem direta nos dois sentidos: 'Global choice columns are useful when you have a standard set of categories that can apply to more than one column. Maintaining two separate choice options with the same values is difficult and if they aren't synchronized you can see errors, especially if you are mapping table columns in a one-to-many table relationship' — MAS também 'If you define every choice as a global choice your list of global choice columns will grow and could be difficult to manage. If you know that the set of options will only be used in one place, use a local choice.' Os valores das opções são INTEIROS, e é justamente por isso que aparecem valores 'esquisitos' nas tabelas OOB (incident.caseorigincode: 1=Phone, 2=Email, 3=Web, 2483=Facebook, 3986=Twitter, 700610000=IoT — os altos vêm de prefixo de solução). Choice pode ser usada em alternate key e em calculated column.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/maker/data-platform/create-edit-global-option-sets ; https://learn.microsoft.com/en-us/dynamics365/developer/reference/entities/incident

**Licao para a Tracbel:** REGRA PRÁTICA para o CRM Tracbel: use ENUM/choice quando o conjunto é (a) pequeno, (b) conhecido pelo código (há if/switch sobre ele) e (c) muda em release; use TABELA DE DOMÍNIO quando o usuário-chave precisa incluir item sem release, quando o item tem atributos próprios (ordem, cor, ativo/inativo, vigência) ou quando é referenciado em relatório. Como o requisito declarado é 'extensibilidade sem release', a Tracbel deve pender para TABELA DE DOMÍNIO na maioria dos casos — mas com valores estáveis e imutáveis (nunca reaproveite código de item excluído). COPIAR a disciplina de valor inteiro estável: números de status/estado NUNCA mudam de significado, senão você quebra todo relatório histórico e integração. Isso já mordeu a Tracbel no Vórtice (tipo de processo 41 vs 50).

### 16. Elastic tables: Cosmos DB por baixo, 20 GB por partição, e uma lista de coisas que somem

Elastic tables rodam sobre Azure Cosmos DB. Toda elastic table tem a coluna de sistema PartitionId (logical name partitionid, tipo string): a identidade única de uma linha é a COMBINAÇÃO chave primária + partitionid — com partitionid custom, o PK sozinho não tem constraint de unicidade. Regras da partition key: valor imutável após criação, alta cardinalidade, distribuição uniforme, no máximo 1.024 BYTES, e nenhum dos caracteres / < > * % & : \\ ? + (mesma lista das alternate keys). CADA PARTIÇÃO LÓGICA ARMAZENA NO MÁXIMO 20 GB; não há limite de número de partições. Se você não setar partitionid, o Dataverse usa o PK como partitionid, e o valor NÃO pode ser alterado depois. Coluna TTL automática: TTLInSeconds (logical name ttlinseconds) — segundos a partir da última modificação; se não setar, a linha persiste indefinidamente. Consistência: só é FORTE dentro de uma 'logical session' — é preciso propagar o token x-ms-session-token (SDK: parâmetro opcional SessionToken; Web API: header MSCRM.SessionToken) em todo request subsequente, senão a leitura pode não ver a própria escrita. O que NÃO funciona: transações multi-registro (ExecuteTransactionRequest e changeset de $batch 'succeed but aren't atomic' — hoje sem erro, no futuro com erro); deep insert ('Cannot create related entities. Create has to be called individually for each entity'); relacionamentos N:N; e plug-in síncrono PostOperation que lança exceção NÃO faz rollback do registro criado. O parâmetro opcional partitionId só está disponível hoje em Retrieve e Delete — para Update/Upsert é preciso usar alternate key. E: linhas expiradas por TTL que já foram sincronizadas para o data lake via Synapse Link NÃO são removidas de lá.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/elastic-tables

**Licao para a Tracbel:** DESCARTAR para o CRM Tracbel. Você tem SQL Server, 130 usuários e requisito explícito de forte consistência relacional (pedido, faturamento, título) — os três casos de uso oficiais de elastic table (dados semiestruturados, escala horizontal, carga imprevisível) não se aplicam. COPIAR APENAS a ideia de TTL: para tabelas de log, telemetria, integração e webhook (o CRM Tracbel vai ter, e vai crescer), defina retenção explícita no schema desde o dia 1 e um job de purge — a doc é o lembrete de que dado com TTL sincronizado para fora NÃO é apagado lá, ou seja, retenção precisa ser tratada em CADA destino, não só na origem.

### 17. Limites de tabelas/colunas e de anexo — o que é oficial e o que não é

OFICIAL: em fev/2023 o limite de tabelas CUSTOM por ambiente subiu de 1.500 para 3.000, e virtual tables deixaram de contar nesse limite. OFICIAL sobre anexo (tabela Annotation): notetext é Memo com MaxLength 100.000; documentbody é String com MaxLength 1.073.741.823 (aprox. 1 GB de base64); filesize é Integer com MaxValue 1.000.000.000 (aprox. 1 GB); filename 255; mimetype 256; subject 500. Colunas de imagem OOB (EntityImage) têm MaxSizeInKB 10.240 (10 MB) e MaxHeight/MaxWidth 144. Já o LIMITE DE COLUNAS POR TABELA eu NÃO consegui confirmar em documentação oficial da Microsoft — só em fontes de comunidade, que dizem que o teto é o do SQL Server (1.024 colunas) mas que o limite prático é bem menor porque colunas OOB e tipos compostos (money gera _Base, lookup gera name/type, rollup gera _date/_state) consomem várias colunas físicas por coluna lógica.

**Evidencia:** https://learn.microsoft.com/en-us/previous-versions/power-platform-release-plan/2022wave2/data-platform/table-limits-are-extended ; https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/annotation

**Licao para a Tracbel:** LIÇÃO DE MODELAGEM, não de limite: repare que no Dataverse UMA coluna lógica pode virar VÁRIAS colunas físicas (money -> valor + valor_Base; lookup -> id + name + type; rollup -> valor + _date + _state). Se a Tracbel copiar esse padrão sem pensar, tabelas centrais estouram e ficam lentas. RECOMENDAÇÃO: no CRM Tracbel, mantenha as tabelas centrais (Cliente, Oportunidade, Equipamento) abaixo de ~80 colunas e empurre o resto para tabelas satélite ou para um campo de atributos estendidos (JSON com schema versionado no SQL Server, indexável por computed column) — isso atende 'extensibilidade sem release' sem inchar o núcleo. Para anexo: NÃO guarde binário na tabela (o Dataverse guarda em documentbody e ele mesmo admite que isso explode storage); use blob/filesystem com ponteiro, exatamente como a Microsoft acabou fazendo com filepointer/storagepointer/prefix.

### 18. Modelo de segurança: BU + role + team + share, tudo aditivo e sem 'negar'

Peças: (1) Business Unit — toda base tem UMA raiz imutável; BUs filhas são criadas para segmentar; todo usuário pertence a uma BU; todo registro tem Owning Business Unit, que por default é a BU do criador. (2) Security role com 8 privilégios por tabela: Create, Read, Write, Delete, Append, Append To, Assign, Share. (3) Cada privilégio tem NÍVEL DE ACESSO em 4 degraus para tabelas user/team-owned: User (só os próprios), Business Unit, Business Unit and Child Business Units, Organization. Tabela ORGANIZATION-OWNED só tem 'pode' ou 'não pode' — e o tipo de ownership é escolhido NA CRIAÇÃO e NÃO PODE SER MUDADO. (4) Teams: Owning Team (possui registros e pode ter role — todo usuário da equipe herda) vs ACCESS TEAM (não possui registro nem tem role; recebe o registro por compartilhamento, e é mais performático). Toda BU tem um default team automático, gerenciado pelo sistema, cujos membros não podem ser adicionados/removidos à mão. (5) Sharing individual existe mas a doc alerta: 'It should be an exception, though, because it's a less performant way of controlling access. Sharing is tougher to troubleshoot'. (6) Column-level security: perfis de segurança de coluna controlam Create/Update/Read por coluna, mas SÓ para quem já tem acesso ao registro. A frase-chave: 'all privilege grants are accumulative with the greatest amount of access prevailing. If you gave broad organization level read access to all contact records, you can't go back and hide a single record' — NÃO EXISTE DENY. Modernized Business Units ('Record ownership across business units') permite atribuir roles de BUs diferentes ao mesmo usuário e setar explicitamente o Owning Business Unit; depende dos settings EnableOwnershipAcrossBusinessUnits, RecomputeOwnershipAcrossBusinessUnits e AlwaysMoveRecordToOwnerBusinessUnit. Armadilha documentada: com AlwaysMoveRecordToOwnerBusinessUnit=false, o privilégio do dono do PAI é validado mas o dos FILHOS não — a cascata pode fazer o dono do filho PERDER acesso ao próprio registro.

**Evidencia:** https://learn.microsoft.com/en-us/power-platform/admin/wp-security-cds

**Licao para a Tracbel:** COPIAR a espinha dorsal, que é exatamente o requisito 'níveis diversos de permissão': Papel -> Privilegio(Entidade, Acao, Escopo) com escopo em {Proprio, Unidade, UnidadeEDescendentes, Organizacao}, aditivo, sem deny. Isso cobre vendedor / líder de CEN / gerente regional / diretoria com UMA tabela. COPIAR a distinção Owning Team x Access Team — é o que resolve 'time de negócio compartilhado' sem duplicar dono. EVITAR: (a) compartilhamento individual como mecanismo padrão — na Tracbel isso viraria a solução de todo problema e ninguém mais entenderia quem vê o quê; permita, mas com expiração e auditoria; (b) ownership imutável — permita migrar entidade de organization-owned para user-owned, porque VAI ser preciso. E note o problema real já vivido na Tracbel ('gerente não vê carteira de pedidos porque a hierarquia ficou presa no usuário antigo'): a resposta do Dataverse é o nível 'Business Unit and Child Business Units' + cascade Assign — implemente hierarquia como ÁRVORE consultável (HierarchyId ou closure table), não como ponteiro de gerente.

### 19. Territory é organization-owned e hierárquica — e NÃO tem vínculo direto com Account na base

territory: OwnershipType = OrganizationOwned; PrimaryNameAttribute = name (200 chars); managerid é lookup para systemuser; parentterritoryid é auto-referência com Delete = RemoveLink (deletar o pai só limpa o ponteiro, não apaga filhos). Na definição da tabela Territory no Dataverse base, os relacionamentos 1:N listados são: territory_system_users (systemuser.territoryid — ou seja, o USUÁRIO é que aponta para o território), territory_parent_territory (a hierarquia), territory_connections1/2 (Connection) e os utilitários (asyncoperation, syncerror, processsession, mailboxtrackingfolder, principalobjectattributeaccess). Não há, nessa definição base, relacionamento direto territory→account; esse vínculo vem da solução Dynamics 365 Sales.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/territory

**Licao para a Tracbel:** ADAPTAR, não copiar. O Territory do Dynamics é fraco: é hierarquia + gerente, e a atribuição é pelo USUÁRIO, não pelo cliente. A Tracbel já tem um modelo MELHOR e mais rico no Vórtice (IVS_Carteira ligando cliente a vendedor via SeqVendedor, permitindo o mesmo cliente em várias carteiras com uma linha por carteira). RECOMENDAÇÃO: no CRM Tracbel modele Carteira/CEN como ENTIDADE DE VÍNCULO explícita — Carteira(Id, Nome, ResponsavelId, CarteiraPaiId) + CarteiraCliente(CarteiraId, ClienteId, Vigencia) — e derive a permissão da carteira, não do usuário. Isso resolve de uma vez os dois problemas conhecidos da Tracbel: 'cliente em várias carteiras' e 'aprovação vai para o líder do CEN no momento da geração'. COPIAR do Dynamics apenas: Delete=RemoveLink na auto-referência (nunca cascatear delete numa hierarquia organizacional) e o campo manager separado do parent.

### 20. Connection/ConnectionRole: relacionamento informal genérico com papel recíproco

connection.record1id e record2id são lookups com um conjunto grande e explícito de Targets (account, contact, systemuser, team, territory, activitypointer, appointment, email, fax, letter, phonecall, task, socialactivity, recurringappointmentmaster, goal, knowledgearticle, knowledgebaserecord, socialprofile, position, processsession, channelaccessprofilerule e outros). record1roleid/record2roleid apontam para connectionrole. relatedconnectionid é lookup para a própria connection (a conexão RECÍPROCA, ApplicationRequired) e ismaster (Boolean, SystemRequired, read-only) marca qual das duas é a original — ou seja, o Dataverse materializa AS DUAS DIREÇÕES do vínculo como dois registros ligados. connectionrole é OrganizationOwned, name até 100 chars, com category: 1=Business, 2=Family, 3=Social, 4=Sales, 5=Other, 1000=Stakeholder, 1001=Sales Team, 1002=Service; e tem N:N AUTO-REFERENTE connectionroleassociation_association, que define quais papéis podem ser recíprocos de quais. A doc de relacionamentos orienta explicitamente usar Connection em vez de relacionamento formal quando 'Most businesses won't generate reports using this kind of information'.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/connection ; https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/connectionrole ; https://learn.microsoft.com/en-us/power-apps/maker/data-platform/create-edit-entity-relationships

**Licao para a Tracbel:** COPIAR com moderação — mas é MUITO útil para a Tracbel: 'quem influencia a compra nessa fazenda', 'esse produtor é sócio daquele grupo', 'esse consultor agronômico atende esses 4 clientes', 'esse mecânico é o contato técnico desse equipamento'. Uma única tabela Vinculo(OrigemTipo, OrigemId, PapelOrigemId, DestinoTipo, DestinoId, PapelDestinoId, Vigencia) resolve tudo isso sem criar 8 entidades. SIMPLIFICAR: NÃO materialize as duas direções em dois registros (o ismaster/relatedconnectionid do Dynamics é fonte de bug de sincronia); grave UM registro e resolva a direção na consulta. COPIAR SIM a tabela de papéis com categoria e a restrição de reciprocidade permitida — sem ela o usuário cria 'Pai' recíproco de 'Pai'.

### 21. Annotation (nota/anexo): polimórfico, mas com lista FECHADA de alvos

annotation.objectid é lookup polimórfico com uma lista explícita e curada de Targets — account, contact, appointment, email, task, phonecall, letter, fax, chat, socialactivity, recurringappointmentmaster, goal, kbarticle, knowledgearticle, knowledgebaserecord, sla, mailbox, emailserverprofile, calendar, workflow, duplicaterule, convertrule, routingrule, sharepointdocument, msdyn_ai*, adx_* etc. Ou seja: NÃO é 'anexa em qualquer coisa'; a tabela precisa ter HasNotes=true. Os cascades a partir de account e contact são fortes: Assign=Cascade, Delete=Cascade, Merge=Cascade, Reparent=Cascade, Share=Cascade, Unshare=Cascade (deletar cliente apaga suas notas; compartilhar cliente compartilha suas notas). A nota é UserOwned (tem ownerid, owninguser, owningteam, owningbusinessunit). O anexo mora em documentbody (base64) e há colunas internas filepointer/storagepointer/prefix indicando migração para blob storage.

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/annotation

**Licao para a Tracbel:** COPIAR o padrão (Anexo com Regarding polimórfico + dono próprio + cascata de compartilhamento a partir do registro pai), MAS armazene o binário fora do banco desde o dia 1 (Azure Blob / file share, com hash e antivírus) — a Microsoft começou dentro do banco (documentbody) e teve que migrar para ponteiro; não repita o caminho. ATENÇÃO ESPECIAL: a cascata Share=Cascade significa que ao compartilhar um cliente você compartilha TODOS os anexos dele. Numa revenda, anexo pode ter proposta comercial de terceiro, análise de crédito, laudo — defina no CRM Tracbel a possibilidade de marcar anexo como restrito e NÃO cascatear compartilhamento cegamente.

### 22. Catálogo e precificação: PriceLevel com vigência, write-in product e 39 códigos de erro de preço

PriceLevel (Price List) é OrganizationOwned, com begindate/enddate (vigência), transactioncurrencyid ApplicationRequired e statecode 0=Active/1=Inactive (statuscode 100001/100002). É referenciada por opportunity, quote, salesorder, invoice, product (lista default do produto), account.defaultpricelevelid, contact.defaultpricelevelid e campaign.pricelistid. A linha de negócio (OpportunityProduct/QuoteDetail/SalesOrderDetail/InvoiceDetail) tem o padrão do WRITE-IN: productid é NULLABLE e existe isproductoverridden (labels: True='Write-In', False='Existing') com productdescription (500 chars) para o item fora de catálogo; ispriceoverridden (labels 'Override Price'/'Use Default') separa preço negociado de preço de tabela. A cadeia de valores é explícita nas descrições: priceperunit (vem da price list do pai) -> baseamount ('total price, based on the price per unit, volume discount, and quantity') -> extendedamount ('Amount value minus the Manual Discount amount'), com volumediscountamount READ-ONLY (vem da configuração de desconto por volume) e manualdiscountamount editável. producttypecode tipifica a linha: 1=Product, 2=Bundle, 3=Required Bundle Product, 4=Optional Bundle Product, 5=Project-based Service; e parentbundleidref é auto-referência com Delete=Cascade (apagar o bundle apaga os itens). pricingerrorcode tem 39 valores (0=None até 38), incluindo 2=Missing Price Level, 11=Product Not In Price Level, 34=Invalid Price Level Currency, 25=Price Calculation Error. skippricecalculation (0=DoPriceCalcAlways, 1=SkipPriceCalcOnCreate, 2=SkipPriceCalcOnUpdate, 3=SkipPriceCalcOnUpSert) é a válvula de escape para integração em massa. Toda coluna Money tem gêmea _Base na moeda da organização, com Precision 4 no _Base e 2 no valor transacional, e exchangerate por registro.

**Evidencia:** https://learn.microsoft.com/en-us/dynamics365/developer/reference/entities/opportunityproduct ; https://learn.microsoft.com/en-us/dynamics365/developer/reference/entities/pricelevel

**Licao para a Tracbel:** COPIAR quatro coisas, todas diretamente aplicáveis a máquina agrícola: (1) WRITE-IN — o vendedor SEMPRE vai precisar lançar item fora de catálogo (frete, implemento de terceiro, serviço); dar um campo texto na linha evita cadastro sujo no produto. (2) A separação priceUnitario / descontoVolume(sistema) / descontoManual(negociado) / valorFinal — nunca uma coluna só de 'valor', porque a diretoria vai querer medir desconto concedido. (3) Lista de preço com VIGÊNCIA e MOEDA, e default por cliente — reajuste John Deere e câmbio são realidade. (4) O flag skipPriceCalculation para as cargas de integração (TOTVS/JDE) não dispararem o motor de preço. SIMPLIFICAR: 39 códigos de erro de preço é excesso; comece com ~8 e deixe extensível. COPIAR TAMBÉM o par valor/valorBase com taxa de câmbio gravada NO REGISTRO — sem isso, relatório histórico em BRL muda quando o câmbio muda.

### 23. Quote: máquina de estados de 4 estados com REVISÃO versionada

quote.statecode: 0=Draft, 1=Active, 2=Won, 3=Closed. quote.statuscode: 1=In Progress (state 0), 2=In Progress (state 1), 3=Open (state 1), 4=Won (state 2), 5=Lost (state 3), 6=Canceled (state 3), 7=Revised (state 3). Regra de negócio embutida na própria descrição da coluna: 'Only draft quotes can be edited'. revisionnumber é SystemRequired (0..2147483647) e é o contador de versões. As mensagens dedicadas (ou seja, transições de estado modeladas como OPERAÇÕES, não como update de campo) são: GenerateQuoteFromOpportunity, GetQuoteProductsFromOpportunity, ReviseQuote, WinQuote, CloseQuote, ConvertQuoteToSalesOrder — além de SetState, Assign, GrantAccess/RevokeAccess/ModifyAccess e Rollup. customerid é tipo Customer (account|contact), ApplicationRequired.

**Evidencia:** https://learn.microsoft.com/en-us/dynamics365/developer/reference/entities/quote

**Licao para a Tracbel:** COPIAR INTEIRO — é o padrão que a Tracbel precisa e é onde CRM caseiro costuma errar. Três lições: (1) O par estado/motivo (statecode/statuscode) com o motivo TIPADO por estado é superior a um único campo 'situação' — permite 'Fechada' com motivos Perdida/Cancelada/Revisada sem confundir relatório. (2) Transição como MENSAGEM/comando com nome de negócio (RevisarProposta, GanharProposta, ConverterEmPedido), NUNCA um PATCH em statecode — isso te dá validação, autorização, log e evento de domínio num lugar só, e é a base de 'fluxos interativos' e 'segurança alta'. Em .NET 8: um endpoint por transição + um State Machine no domínio. (3) Revisão VERSIONADA e imutabilidade fora de Draft: proposta enviada ao cliente não pode ser editada; revisar cria versão nova. Isso resolve a auditoria de 'que preço foi prometido' que é crítica em venda de máquina.

### 24. Field Service: como o Dynamics modela EQUIPAMENTO DO CLIENTE (o caso Tracbel)

msdyn_customerasset é o ativo/equipamento instalado no cliente. Colunas graváveis: msdyn_name, msdyn_Account (Targets=account; descrição 'Parent Customer of this Asset'), msdyn_Product (Targets=product — liga o ativo ao item do catálogo), msdyn_ParentAsset (Targets=msdyn_customerasset), msdyn_MasterAsset (Targets=msdyn_customerasset; no Field Service o DisplayName é 'Top-Level Asset' e a descrição é 'Top-Level Asset, (if this asset is a sub asset)'), msdyn_FunctionalLocation (Targets=msdyn_functionallocation), msdyn_CustomerAssetCategory (Targets=msdyn_customerassetcategory), msdyn_AssetTag (100 chars), msdyn_ManufacturingDate, msdyn_Latitude/msdyn_Longitude, msdyn_DeviceId (registro no provedor IoT), msdyn_RegistrationStatus, msdyn_WorkOrderProduct ('link to the Work Order Product from where this Asset was auto created by the system' — instalar uma peça numa OS CRIA o sub-ativo automaticamente), statecode/statuscode e ownerid (systemuser|team). O ativo é referenciado por: msdyn_workorder, msdyn_workorderincident, msdyn_workorderproduct, msdyn_workorderservice, msdyn_workorderservicetask, msdyn_workorderresolution, msdyn_rmaproduct (devolução), msdyn_inspectioninstance (inspeção), msdyn_entitlementapplication e as quatro tabelas msdyn_agreementbooking* (contrato de manutenção). O detalhe de design mais importante: DUAS hierarquias simultâneas — ParentAsset (pai imediato) e MasterAsset (raiz da árvore), justamente para não precisar de CTE recursiva para responder 'todos os componentes desta máquina'.

**Evidencia:** https://learn.microsoft.com/en-us/dynamics365/field-service/developer/reference/entities/msdyn_customerasset ; https://learn.microsoft.com/en-us/dynamics365/developer/reference/entities/msdyn_customerasset

**Licao para a Tracbel:** COPIAR QUASE INTEGRALMENTE — este é o achado mais aplicável ao negócio da Tracbel. Equipamento John Deere é exatamente isso: máquina (ativo raiz) com componentes/implementos (sub-ativos), número de série, ligação ao item do catálogo, localização (fazenda/talhão = functional location), telemetria (JDLink ~ msdyn_DeviceId) e horímetro. COPIAR: (a) a dupla hierarquia ParentAsset + MasterAsset (denormalização deliberada da raiz — é barata e mata a recursão nas listagens e relatórios); (b) msdyn_FunctionalLocation separado do cliente — a máquina fica na FAZENDA, e fazenda não é igual a CNPJ do comprador; (c) a criação automática do sub-ativo a partir da peça instalada na OS — é o que mantém o 'as-built' do equipamento vivo sem digitação. ADICIONAR o que o Dynamics não tem OOB e a Tracbel precisa: horímetro/odômetro com histórico, garantia (início/fim/tipo), e proprietário ATUAL vs histórico de propriedade (máquina agrícola é revendida — o ativo sobrevive à troca de dono, e msdyn_Account sozinho perde esse histórico).

### 25. Field Service: Work Order é cabeçalho + linhas + template, e o agendamento é uma camada genérica

Fluxo documentado em 4 fases: criada -> agendada -> executada -> revisada/fechada. (1) msdyn_workorder guarda work order type, status, substatus, duration e priority; liga-se a account por SERVICE ACCOUNT, do qual herda endereço de serviço, território, defaults de faturamento, impostos e instruções; o Service Account pode ser configurado como obrigatório ou não POR TIPO DE OS. Billing account é separado do service account. (2) msdyn_incidenttype é o TEMPLATE: 'a defined package of recommended service tasks, products, services, and characteristics, or skills' — aplicar um Incident Type na OS explode automaticamente as linhas de service task, produto, serviço e skills necessárias. (3) Agendamento: a OS gera automaticamente um msdyn_resourcerequirement e o sistema mantém OS e requirement primário sincronizados (requirements criados manualmente NÃO são sincronizados). O framework de requirement é genérico: 'partly enables scheduling any entity, such as cases, opportunities, or custom entities'. Um requirement pode ser reservado várias vezes, gerando múltiplos bookableresourcebooking (mesmo recurso em horários diferentes, ou vários recursos). (4) Bookable resource = 'an employee, contractor, EQUIPMENT, facility, or anything else that needs to be scheduled'. Mudanças de status do técnico no app viram Booking Timestamps e depois Booking Journals, que são a base do cálculo de tempo e custo de mão de obra. (5) Fechar a OS dispara consumo de estoque, criação de invoice e actuals. msdyn_agreement (service agreement) gera OS recorrentes e está amarrado a UM service account.

**Evidencia:** https://learn.microsoft.com/en-us/dynamics365/field-service/field-service-architecture

**Licao para a Tracbel:** COPIAR três decisões de arquitetura: (1) TEMPLATE DE SERVIÇO (Incident Type) — 'Revisão 500h da colheitadeira X' vira um pacote cadastrável de tarefas + peças + serviços + competências; é literalmente 'extensibilidade sem release' aplicada à operação, e o pós-venda da Tracbel vive disso. (2) SEPARAR o que precisa ser feito (Requirement) de quem/quando faz (Booking) — assim o mesmo motor de agenda serve OS, visita comercial e demonstração de campo, em vez de três agendas paralelas. (3) SERVICE ACCOUNT ≠ BILLING ACCOUNT — no agro isso é regra, não exceção (fazenda opera, grupo/holding paga). SIMPLIFICAR: no v1 não faça Booking Journals nem otimização de rota; faça Requirement + Booking + timestamps de status, e derive o tempo depois. DESCARTAR por ora: RMA, inspection instance e agreement booking — só entram quando o pós-venda pedir.

### 26. Extensibilidade sem release no Dataverse: pipeline de eventos e o teto de 2 minutos

O motor executa toda operação de dados por um pipeline de estágios: PreValidation (stage 10, FORA da transação — é onde a doc manda colocar validação, porque só aí o erro impede a operação de começar), PreOperation (stage 20, DENTRO da transação, antes da escrita — é onde se altera o Target), Main/Core Operation e PostOperation (stage 40, dentro da transação, depois da escrita e antes do commit). Plug-ins rodam em sandbox isolada e estouram com System.TimeoutException aos 2 MINUTOS; custom workflow activities dentro de real-time workflow têm o mesmo teto. Low-code plug-ins (Power Fx) participam do MESMO pipeline — muda o modelo de autoria, não o runtime. Alerta importante em elastic tables: lançar InvalidPluginExecutionException em PreOperation/PostOperation síncrono NÃO faz rollback (a operação de dados já ocorreu), enquanto em PreValidation faz. Além do pipeline há: business rules (declarativas, rodam no cliente e/ou servidor), custom actions/custom process actions (mensagens de negócio nomeadas), workflows e Power Automate, tudo empacotado e movido entre ambientes por SOLUTIONS (security roles e column security profiles vão na solution; business units e teams NÃO — têm que ser recriados em cada ambiente).

**Evidencia:** https://learn.microsoft.com/en-us/power-apps/developer/data-platform/event-framework ; https://learn.microsoft.com/en-us/power-apps/developer/data-platform/elastic-tables (seção Transactional behavior) ; https://learn.microsoft.com/en-us/power-platform/admin/wp-security-cds (seção Managing security across multiple environments)

**Licao para a Tracbel:** COPIAR o pipeline como padrão de arquitetura em .NET 8 — é o núcleo do requisito 'OO forte + extensibilidade sem release'. Implemente com MediatR/pipeline behaviors ou interceptors do EF Core, com 4 estágios explícitos: Validar (fora da transação de negócio), AntesDeGravar (dentro), Gravar, DepoisDeGravar (dentro), e um 5º Assíncrono (fora, via outbox). Registre handlers por (Entidade, Operacao, Estagio, Ordem) em TABELA, para o time habilitar/desabilitar regra sem deploy. COPIAR o timeout: coloque um teto explícito (60-90 s) em qualquer handler síncrono, com cancelamento — sem isso, uma regra mal escrita trava o CRM inteiro para 130 usuários. COPIAR o conceito de SOLUTION: desde o dia 1, tudo que é configuração (papéis, tipos de atividade, templates de serviço, regras, field maps) precisa ser EXPORTÁVEL/IMPORTÁVEL entre DEV/HOM/PROD como artefato versionado — sem isso, 'extensibilidade sem release' vira 'configuração manual em produção'. E aprenda com a falha da Microsoft: business unit e team NÃO são transportáveis; no CRM Tracbel, torne a estrutura organizacional transportável (ou pelo menos seedável por script).

### 27. MAPEAMENTO PROPOSTO: Vórtice -> Dynamics/Dataverse -> CRM Tracbel (entidades centrais)

PESSOA/CLIENTE: Vórtice IVS_Pes (pessoa única PF/PJ) -> Dynamics Account + Contact + lookup polimórfico Customer -> Tracbel: MANTER o modelo do Vórtice, uma entidade Pessoa(TipoPessoa PF/PJ, CnpjCpf com alternate key) + Papel (Cliente, Prospect, Fornecedor, Contato), eliminando o polimorfismo Customer. VENDEDOR/HIERARQUIA: Vórtice IV_VENDEDOR + IV_VENDEDOREMPR -> Dynamics SystemUser + BusinessUnit + Territory -> Tracbel: Usuario + UnidadeOrganizacional hierárquica (com HierarchyId/closure table), separando IDENTIDADE (Usuario, via Entra ID) de POSIÇÃO (lotação/cargo) — foi a mistura das duas que causou o problema real 'gerente não vê carteira ao trocar de usuário'. CARTEIRA/CEN: Vórtice IVS_Carteira (SeqVendedor; cliente pode estar em várias carteiras, 1 linha por carteira) -> Dynamics Territory (fraco) + Access Team -> Tracbel: Carteira + CarteiraCliente com vigência; permissão DERIVADA da carteira. PROCESSO/OPORTUNIDADE: Vórtice processo (tipo 41/50, Fase + Status) -> Dynamics Opportunity (statecode 0/1/2 + statuscode) + Business Process Flow -> Tracbel: Oportunidade com Estado + MotivoEstado + FluxoDefinicaoId/EtapaAtualId, onde o FLUXO é dado versionado (nunca troque o tipo de um processo em andamento — é o bug 41 vs 50 já vivido). HISTÓRICO/AGENDA/AÇÃO: Vórtice ações (897, 899, 900) -> Dynamics ActivityPointer + ActivityParty + regardingobjectid -> Tracbel: Atividade (TPT) + AtividadeParticipante + TipoAtividade cadastrável com regras (limite por pessoa, gatilho, SLA) em DADOS — a regra 'ação 897 tem limite vitalício de 1 por pessoa' tem que ser configuração, não if. PROPOSTA/PEDIDO/FATURA: Vórtice pedido/faturamento -> Dynamics Quote(rev) -> SalesOrder -> Invoice, com mensagens ReviseQuote/ConvertQuoteToSalesOrder -> Tracbel: Proposta versionada -> Pedido -> (integração TOTVS/JDE para faturamento real, com alternate key). EQUIPAMENTO: Vórtice equipamentos -> Dynamics msdyn_customerasset (ParentAsset+MasterAsset+FunctionalLocation+Product) -> Tracbel: Equipamento com dupla hierarquia, NumeroSerie como alternate key, Local(Fazenda/Talhão), Garantia, Horimetro histórico e Propriedade histórica. OS/SERVIÇO: Vórtice OS -> Dynamics msdyn_workorder + incidenttype + resourcerequirement + booking -> Tracbel: OrdemServico + TemplateServico + Requisicao + Agendamento. DOCUMENTO/ANEXO: Vórtice IV_ProcDocto/DMN_Doc + Doc Manager FTP -> Dynamics Annotation(objectid) -> Tracbel: Anexo com Regarding polimórfico + blob externo + FK íntegra (o problema real dos 5.302 vínculos órfãos em IV_ProcDocto sem DMN_Doc é exatamente ausência de FK/cascata declarada). PRODUTO/PREÇO: -> Dynamics Product + PriceLevel + ProductPriceLevel -> Tracbel: Produto + TabelaPreco(vigência, moeda) + TabelaPrecoItem, com write-in nas linhas.

**Evidencia:** Síntese das páginas oficiais já citadas (activitypointer, activityparty, opportunity, quote, opportunityproduct, pricelevel, msdyn_customerasset, field-service-architecture, wp-security-cds, define-alternate-keys-entity) cruzada com o conhecimento do schema Vórtice registrado na memória do projeto (IVS_Pes, IV_VENDEDOR, IV_VENDEDOREMPR, IVS_Carteira/SeqVendedor, IV_ProcDocto/DMN_Doc, tipo de processo 41 vs 50, ações 897/899/900)

**Licao para a Tracbel:** Ordem de construção sugerida para um time de 2-3 pessoas: (1) Núcleo = Pessoa + Usuario/UnidadeOrganizacional + Papel/Privilegio + Carteira; (2) Atividade + AtividadeParticipante + Timeline (é o que o usuário final percebe primeiro e o que o Vórtice não entrega); (3) Oportunidade com máquina de estados por comando + fluxo em dados; (4) Proposta/Pedido com versionamento e write-in; (5) Equipamento com dupla hierarquia; (6) OS/Field Service. Fundação transversal desde o dia 1, não depois: alternate keys em todas as chaves de negócio (CNPJ/CPF, série, códigos TOTVS/JDE), pipeline de 4 estágios com handlers registrados em tabela, comportamento de cascata declarado em metadado, e tudo que é configuração exportável entre ambientes.

## Lacunas declaradas

- LIMITE DE COLUNAS POR TABELA no Dataverse: NÃO consegui confirmar em fonte oficial da Microsoft. As únicas fontes encontradas foram comunidade/LinkedIn afirmando que o teto é o do SQL Server (1.024 colunas) e que o limite prático é menor. Não use esse número como fato.
- QuoteDetail, SalesOrderDetail e InvoiceDetail: NÃO abri as páginas de referência dessas três tabelas. A existência delas é certa (as mensagens ConvertQuoteToSalesOrder e os relacionamentos price_level_quotes/orders/invoices confirmam), mas a lista de colunas que descrevi para 'linha de negócio' foi verificada apenas em OpportunityProduct — assumi por analogia que o padrão write-in/pricing se repete, o que é altamente provável mas não confirmado coluna a coluna.
- Campaign, CampaignActivity e CampaignResponse: só verifiquei indiretamente (CampaignActivity e CampaignResponse aparecem na tabela de activity party types, e campaign.pricelistid aparece nos relacionamentos de PriceLevel). Não abri a referência da tabela campaign — colunas, statecode/statuscode e o modelo de lista de marketing (list/marketinglist) não foram apurados.
- Competitor: confirmei pela documentação de maker que existe N:N entre opportunity e competitor, mas o NOME da intersect table ('opportunitycompetitors') veio de fonte de comunidade, não da referência oficial da tabela. Também não abri a referência de competitor (colunas como strengths, weaknesses, winpercentage não foram verificadas).
- QualifyLead — parâmetro SuppressDuplicateDetection: pedi explicitamente e ele NÃO aparece na lista de Properties de QualifyLeadRequest na documentação .NET. Os parâmetros confirmados são LeadId, CreateAccount, CreateContact, CreateOpportunity, OpportunityCurrencyId, OpportunityCustomerId, SourceCampaignId, Status e ProcessInstanceId. Também não confirmei o formato exato do retorno QualifyLeadResponse (a propriedade CreatedEntities não foi vista na página).
- Choices (multi-select): NÃO encontrei documentado o número máximo de opções selecionáveis numa coluna Choices, nem o esquema de prefixo de valor inteiro por publisher (os valores altos como 700610000 são evidência de prefixo, mas não achei a regra formal documentada).
- msdyn_workorder: descrevi o modelo pela página de ARQUITETURA (conceitual), não pela referência de tabela. Nomes de colunas específicas da work order (msdyn_serviceaccount, msdyn_billingaccount, msdyn_systemstatus, msdyn_substatus) não foram confirmados um a um na referência da entidade.
- Booking statuses e work order system statuses: a página 'work-order-status-booking-status' apareceu na busca mas eu NÃO a abri — os valores numéricos dos status de OS e de booking não estão confirmados.
- Territory x Account: a referência base do Dataverse de Territory NÃO lista relacionamento com account. Inferi que o vínculo territory/account vem da solução Dynamics 365 Sales, mas não confirmei isso abrindo a versão Sales da tabela territory nem a referência de account.
- Schema real do Vórtice: esta pesquisa foi 100% web. Os nomes Vórtice usados no mapeamento (IVS_Pes, IV_VENDEDOR, IV_VENDEDOREMPR, IVS_Carteira/SeqVendedor, IV_ProcDocto, DMN_Doc, tipo de processo 41/50, ações 897/899/900) vêm da memória do projeto, não foram re-verificados contra o banco nesta tarefa. Confirme no SQL Server antes de usar o mapeamento como especificação.
- Elastic tables: não confirmei se rollup columns, calculated columns e auditoria funcionam nelas (a doc lista o que NÃO funciona em transação, deep insert e N:N, mas não faz uma lista exaustiva de features indisponíveis). Também não apurei limite de tamanho de linha nem page size de consulta.
- Business Process Flow: não abri documentação específica sobre a tabela de BPF nem sobre como a instância do processo é persistida (só sei, pela doc de QualifyLead, que existe ProcessInstanceId transferível do Lead para a Opportunity, e que a BPF só acompanha a oportunidade primária). O modelo de dados de 'fluxos interativos' do Dynamics ficou pouco coberto.
