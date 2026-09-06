# Modelo de dados padrão do Salesforce Sales Cloud — objetos, relacionamentos, Lead Conversion, Record Types, segurança por registro e Asset; com mapeamento Vórtice → Salesforce → CRM Tracbel

> Pesquisa automatizada - workflow `crm-tracbel-pesquisa-profunda`, 30/08/2026.

## Resumo

O Sales Cloud organiza a venda em duas metades deliberadamente separadas: o "pré-relacionamento" (Lead, uma tabela flat e autossuficiente com nome+empresa+contato no mesmo registro) e o "pós-relacionamento" (Account/Contact/Opportunity, normalizado). A ponte é a Lead Conversion, uma operação de sentido único: cria/reaproveita Account+Contact+Opportunity, copia campos por um mapa configurável (campos não mapeados são perdidos), reancora atividades e campaign members, marca IsConverted=true e trava o Lead como read-only (erro CANNOT_UPDATE_CONVERTED_LEAD). Sobre esse esqueleto, o Salesforce empilha três mecanismos que a Tracbel precisa entender antes de copiar: (1) Record Type + BusinessProcess, que troca layout, subconjunto de picklist e ciclo de vida por tipo de negócio — mas que a própria doc proíbe usar como controle de acesso; (2) três tipos de relacionamento (Lookup, Master-Detail, Hierarchical) com semânticas de cascata, ownership e sharing radicalmente diferentes — Master-Detail elimina o OwnerId do filho e faz herdar sharing do pai; (3) o OwnerId como âncora única da segurança por registro, complementado por tabelas de share (AccountShare/OpportunityShare) com RowCause tipado (Owner, Rule, Team, Manual, ImplicitParent). Para revenda de máquinas, o objeto-chave é Asset: frota instalada do cliente, com hierarquia de até 10.000 ativos, SerialNumber, InstallDate, UsageEndDate, Status e AssetRelationship (troca/upgrade) — é o análogo direto de EXT_Veic. A recomendação central: copiar a separação Lead/Account, o modelo Owner+Share e o Asset; simplificar drasticamente Pricebook/Quote/Order; e descartar Record Type como camada de segurança e Master-Detail como mecanismo de sharing.

## Entidades / objetos mapeados

### `Lead`

**Funcao:** Prospect não qualificado. Tabela FLAT e autossuficiente: guarda dados de pessoa (FirstName/LastName/Email/Phone/Title) E de empresa (Company, Industry, NumberOfEmployees, AnnualRevenue) no MESMO registro, sem FK para Account. É o único objeto do núcleo que não exige relacionamento pai.

**Campos-chave:** Id, OwnerId (User OU Queue), Status (picklist governada por LeadStatus), Company (obrigatório), LeadSource, Rating, RecordTypeId, IsUnreadByOwner, IsConverted, ConvertedDate, ConvertedAccountId, ConvertedContactId, ConvertedOpportunityId, MasterRecordId (merge)

**Relacoes:** ConvertedAccountId→Account (Lookup), ConvertedContactId→Contact (Lookup), ConvertedOpportunityId→Opportunity (Lookup) — todos Create/Filter/Group/Nillable/Sort, SEM propriedade Update. Lead NÃO pode ser o lado 'master' de um master-detail. CampaignMember.LeadId aponta para cá.

### `Account`

**Funcao:** Organização (cliente, concorrente, parceiro). Em orgs com Person Accounts habilitadas, também representa pessoa física — o registro Account carrega campos Person* e mantém um Contact filho oculto.

**Campos-chave:** Id, Name, OwnerId, ParentId (auto-lookup = hierarquia), RecordTypeId, Type, Industry, IsPersonAccount, PersonContactId, PersonEmail, PersonTitle (máx 80 chars), Billing/Shipping Address (compound), MasterRecordId

**Relacoes:** ParentId→Account (self-lookup, hierarquia); Contact.AccountId→Account; Opportunity.AccountId→Account; Case.AccountId; Contract.AccountId (obrigatório); Order.AccountId (obrigatório); Asset.AccountId; Entitlement.AccountId. AccountShare guarda os acessos; AccountTeamMember o time.

### `Contact`

**Funcao:** Pessoa física ligada a uma empresa. AccountId é Nillable → existe 'private contact' sem empresa.

**Campos-chave:** Id, AccountId, OwnerId, FirstName/LastName, Email, Phone, Title, ReportsToId, RecordTypeId, MailingAddress

**Relacoes:** AccountId→Account (Lookup, nillable, updateable); ReportsToId→Contact (self-lookup, organograma); AccountContactRelation permite N:N contato↔contas; OpportunityContactRole liga a Opportunity; CampaignMember.ContactId; Asset.ContactId.

### `AccountContactRelation`

**Funcao:** Objeto de junção que permite um mesmo Contact se relacionar com várias Accounts (feature 'Contacts to Multiple Accounts'). Disponível desde API 37.0 e suporta Person Accounts.

**Campos-chave:** AccountId, ContactId (ambos imutáveis após criação), Roles (multipicklist), IsDirect, IsActive, StartDate, EndDate

**Relacoes:** N:N entre Account e Contact, com vigência temporal (StartDate/EndDate) — histórico de vínculo, não só o estado atual.

### `Opportunity`

**Funcao:** Negócio/venda em andamento. Motor do pipeline e do forecast.

**Campos-chave:** Id, AccountId, OwnerId, Name, StageName (obrigatório), CloseDate (obrigatório), Amount, Probability, ExpectedRevenue (=Amount×Probability, read-only), ForecastCategory / ForecastCategoryName, IsClosed, IsWon, Type, LeadSource, RecordTypeId, Pricebook2Id, SyncedQuoteId, CampaignId, ContractId, ContactId (read-only, derivado do OpportunityContactRole primário), HasOpportunityLineItem, Territory2Id, Description (limite 32.000 chars)

**Relacoes:** AccountId→Account; CampaignId→Campaign; ContractId→Contract; Pricebook2Id→Pricebook2; SyncedQuoteId→Quote (sincronização bidirecional); OpportunityLineItem (filhos); OpportunityContactRole (papéis de contato); OpportunityTeamMember; OpportunityCompetitor; OpportunityShare.

### `OpportunityLineItem`

**Funcao:** Item de produto da oportunidade ('Opportunity Product' na UI). Só existe se a Opportunity tiver um Pricebook2.

**Campos-chave:** OpportunityId (obrigatório, Create/Filter/Group/Sort — SEM Nillable e SEM Update), PricebookEntryId (obrigatório), Product2Id (read-only, derivado), Quantity, UnitPrice, TotalPrice, ListPrice (espelha o UnitPrice do PricebookEntry), ProductCode, Name (read-only), Description

**Relacoes:** OpportunityId→Opportunity; PricebookEntryId→PricebookEntry. NÃO aponta direto para Product2 — o preço só existe via PricebookEntry. Criar um item incrementa Opportunity.Amount e ExpectedRevenue; a partir daí Amount vira read-only.

### `Product2`

**Funcao:** Catálogo de produtos/serviços que a empresa vende. É apenas o item — sem preço.

**Campos-chave:** Id, Name, ProductCode, Family, IsActive, StockKeepingUnit, Description

**Relacoes:** PricebookEntry liga Product2 a Pricebook2; Asset.Product2Id aponta o produto vendido; OpportunityLineItem/QuoteLineItem/OrderItem chegam via PricebookEntry. Product2 NÃO pode ser o master de um master-detail.

### `Pricebook2`

**Funcao:** Lista de preços. Toda org tem exatamente UM 'standard price book' (IsStandard=true) que define o preço de lista; price books customizados servem para canal, desconto, mercado ou contas específicas.

**Campos-chave:** Id, Name, IsActive, IsStandard (read-only), IsArchived, Description

**Relacoes:** 1:N para PricebookEntry. Opportunity.Pricebook2Id, Quote.Pricebook2Id, Order.Pricebook2Id. Pricebook2 NÃO pode ser master de master-detail. A app cliente só pode ATUALIZAR o standard price book — não criar/excluir.

### `PricebookEntry`

**Funcao:** Junção Produto × Lista de Preço × Moeda. É onde mora o PREÇO. Uma linha por combinação produto+pricebook+moeda.

**Campos-chave:** Pricebook2Id (obrigatório, imutável), Product2Id (obrigatório, imutável), UnitPrice (label 'List Price'), UseStandardPrice, IsActive, IsArchived, ProductCode/Name (read-only, do Product2), CurrencyIsoCode

**Relacoes:** Master do OpportunityLineItem/QuoteLineItem/OrderItem via PricebookEntryId. Se UseStandardPrice=true o UnitPrice é read-only e herda do PricebookEntry do standard price book. PricebookEntry NUNCA é deletado de fato — vira IsArchived e some da API; registros deletados não são recuperáveis.

### `Quote`

**Funcao:** Proposta comercial com preços propostos, gerada a partir de uma Opportunity e enviável como PDF. Disponível desde API 18.0.

**Campos-chave:** Id, OpportunityId, Pricebook2Id, ContractId, ContactId, QuoteNumber, Status, ExpirationDate, IsSyncing, Subtotal, Discount, GrandTotal, LineItemCount, QuoteToAddress/ShippingAddress

**Relacoes:** OpportunityId→Opportunity; a sincronização é dirigida pelo campo Opportunity.SyncedQuoteId (setar liga o sync, limpar desliga; o quote precisa ser filho daquela opportunity). CurrencyIsoCode é copiado da Opportunity e não pode mudar. QuoteLineGroup é filho de Quote por MASTER-DETAIL real.

### `QuoteLineItem`

**Funcao:** Item de produto da proposta.

**Campos-chave:** QuoteId, PricebookEntryId (obrigatório), Product2Id (obrigatório), OpportunityLineItemId (preenchido pela API na criação, não editável, API 40.0+), Quantity, UnitPrice, Discount, TotalPrice, ListPrice, SortOrder

**Relacoes:** QuoteId→Quote; OpportunityLineItemId→OpportunityLineItem (rastreia a origem do item quando o quote foi gerado da opportunity). QuoteLineItem NÃO pode ser master de master-detail.

### `Order`

**Funcao:** Pedido associado a um Contract ou a uma Account. Tem máquina de estados própria (Draft → Activated).

**Campos-chave:** Id, AccountId (obrigatório, só editável enquanto StatusCode=Draft), ContractId, OpportunityId, QuoteId, Pricebook2Id, OrderNumber, Status / StatusCode, EffectiveDate, EndDate, Type, OriginalOrderId (reduction/change orders), ActivatedById, ActivatedDate, TotalAmount, Billing/Shipping Address

**Relacoes:** OrderItem (filhos); OrderChangeLog é filho por MASTER-DETAIL real (RelatedOrderId). Order NÃO pode ser deletado quando Status=Activated; só em Draft. Voltar de Activated para Draft só é permitido se não houver reduction order products filhos.

### `OrderItem`

**Funcao:** Item de produto do pedido ('Order Product').

**Campos-chave:** OrderId, PricebookEntryId, Product2Id, Quantity, UnitPrice, ListPrice, TotalPrice, ServiceDate, EndDate, OriginalOrderItemId, AvailableQuantity

**Relacoes:** OrderId→Order; PricebookEntryId→PricebookEntry. Exige permissão Edit em Order para criar/alterar/excluir o item. OrderItem NÃO pode ser master de master-detail.

### `Contract`

**Funcao:** Acordo comercial ligado a uma Account. Base de renovação e de entitlement de serviço.

**Campos-chave:** AccountId (obrigatório), ContractNumber, Status/StatusCode, StartDate, ContractTerm (meses), EndDate (READ-ONLY: calculado = StartDate + ContractTerm; editável só se 'Auto-calculate Contract End Date' estiver desligado), OwnerExpirationNotice, CustomerSignedId/Date/Title, CompanySignedId/Date, ActivatedById/Date, SpecialTerms

**Relacoes:** AccountId→Account; Order.ContractId; Opportunity.ContractId; ContractContactRole; ContractLineItem → Entitlement.

### `Campaign`

**Funcao:** Ação de marketing (mala direta, webinar, feira). Tem hierarquia própria (campos *InHierarchy fazem roll-up da árvore).

**Campos-chave:** Id, Name, Type, Status, StartDate, EndDate, IsActive, ParentId, BudgetedCost, ActualCost, ExpectedRevenue, NumberOfLeads, NumberOfConvertedLeads, NumberOfOpportunities, AmountAllOpportunities, CampaignMemberRecordTypeId

**Relacoes:** ParentId→Campaign (hierarquia); CampaignMember (junção); Opportunity.CampaignId; CampaignInfluence (associação campanha↔oportunidade em Customizable Campaign Influence). Portal users não acessam este objeto.

### `CampaignMember`

**Funcao:** Junção Campanha × (Lead OU Contact OU Account). Carrega o status de resposta do membro naquela campanha.

**Campos-chave:** CampaignId (obrigatório), LeadId, ContactId, AccountId, Status (máx 40 chars, validado contra CampaignMemberStatus da campanha), HasResponded (READ-ONLY, derivado do Status), FirstRespondedDate, Type (indica se é lead, contact ou account)

**Relacoes:** Regra dura: um registro deve ter ContactId OU LeadId, NUNCA os dois — se enviar os dois, o insert 'passa' mas só o ContactId é gravado. Exceção documentada: para rastrear membros originados de lead que viraram contato, preencha AMBOS. RecordTypeId não é setável aqui — usa-se Campaign.CampaignMemberRecordTypeId.

### `Case`

**Funcao:** Chamado/atendimento (Service Cloud). Também usado como incidente pós-venda ligado ao equipamento.

**Campos-chave:** AccountId, ContactId, AssetId, EntitlementId, ParentId (self-lookup para hierarquia de casos), CaseNumber, Status, IsClosed (controlado pelo Status, não setável), IsClosedOnCreate, Priority, Origin, Reason, IsEscalated, BusinessHoursId, ServiceContractId, SlaStartDate/SlaExitDate, MasterRecordId

**Relacoes:** AccountId→Account; ContactId→Contact; AssetId→Asset (liga o chamado ao equipamento); EntitlementId→Entitlement (define o SLA aplicável); ParentId→Case.

### `Task`

**Funcao:** Tarefa (atividade a fazer / registro de contato realizado). Junto com Event forma 'Activities'.

**Campos-chave:** Id, OwnerId, Subject, Status, Priority, ActivityDate, WhoId (polimórfico), WhatId (polimórfico), IsClosed, IsRecurrence, CallDurationInSeconds, CallType, TaskSubtype, AccountId (derivado)

**Relacoes:** WhoId → Contact ou Lead (a PESSOA); WhatId → objeto não-humano (Account, Opportunity, Case, Campaign, Contract, Order, Product2, Asset, custom objects…). Com Shared Activities, TaskWhoIds é um JunctionIdList (N:N com contatos/leads).

### `Event`

**Funcao:** Compromisso de calendário (reunião, visita).

**Campos-chave:** Id, OwnerId, Subject, StartDateTime, EndDateTime, IsAllDayEvent, WhoId, WhatId, WhoCount, WhatCount, GroupEventType, Location, IsRecurrence2

**Relacoes:** Mesma dupla WhoId/WhatId da Task. EventWhoIds (JunctionIdList) cria N:N com contatos/leads quando Shared Activities está ligado — o primeiro da lista vira o WhoId primário. Zerar o JunctionIdList APAGA todos os registros de junção e é irreversível.

### `ActivityHistory / OpenActivity`

**Funcao:** Objetos READ-ONLY (só describeSObjects()) que materializam as related lists de 'Atividades Passadas' e 'Atividades Abertas' de um registro. Não são tabelas — são views agregadas.

**Campos-chave:** AccountId (derivado: conta do WhatId, senão conta do WhoId, senão null), ActivityDate, ActivityDateTime, WhoId, WhatId, Subject, Status, IsTask

**Relacoes:** Agregam Tasks fechadas + Events passados (ActivityHistory) e Tasks abertas + Events futuros (OpenActivity) de TODOS os contatos relacionados ao registro pai.

### `ContentDocument / ContentVersion / ContentDocumentLink`

**Funcao:** Modelo moderno de arquivos (Salesforce Files). ContentDocument é o arquivo lógico, ContentVersion cada versão, ContentDocumentLink é a JUNÇÃO polimórfica que compartilha o arquivo com registros, usuários, grupos e bibliotecas.

**Campos-chave:** ContentDocumentLink: ContentDocumentId, LinkedEntityId (polimórfico), ShareType, Visibility

**Relacoes:** LinkedEntityId aponta para praticamente qualquer objeto que suporte feed tracking (inclusive custom). Um MESMO arquivo pode estar ligado a N registros — diferente do antigo Attachment/Note, que tinha ParentId 1:1.

### `Note (clássico)`

**Funcao:** Nota de texto simples anexada a um registro. Modelo legado, substituído por ContentNote/Files.

**Campos-chave:** ParentId, Title, Body (limitado a 32 KB), IsPrivate, OwnerId

**Relacoes:** ParentId aponta para UM único registro pai — não é compartilhável entre registros. Por isso o Salesforce migrou para ContentDocumentLink.

### `Asset`

**Funcao:** Item de valor comercial que o cliente COMPROU e possui — produto próprio ou de concorrente. É a base instalada / frota do cliente. Central para revenda de equipamentos.

**Campos-chave:** Name (obrigatório), AccountId (obrigatório se ContactId vazio), ContactId (obrigatório se AccountId vazio), Product2Id, SerialNumber, InstallDate, PurchaseDate, ManufactureDate, UsageEndDate, Status (picklist padrão: Purchased, Shipped, Installed, Registered, Obsolete), StatusReason (Not Ready/Off/Offline/Online/Paused/Standby), Price, Quantity, ParentId, RootAssetId, AssetLevel (raiz=1, filhos=2, …), AssetProvidedById, AssetServicedById, LocationId, Address (compound), IsCompetitorProduct, IsInternal, ConsequenceOfFailure (Insignificant/Minor/Moderate/Major/Critical), AveragetimeBetweenFailure, AveragetimetoRepair, AverageUptimePerDay, Availability, SumDowntime, SumUnplannedDowntime, UptimeRecordStart/End, DigitalAssetStatus (On/Off/Warning/Error), Uuid, ExternalIdentifier, OwnerId, RecordTypeId

**Relacoes:** AccountId→Account (dono), ContactId→Contact, Product2Id→Product2 (o que é), ParentId→Asset (hierarquia de até 10.000 ativos), AssetProvidedById→Account (fabricante), AssetServicedById→Account (quem faz a manutenção), LocationId→Location, SalesStoreId→RetailStore|WebStore (polimórfico). AssetRelationship modela relação NÃO-hierárquica (troca, upgrade, bundle). Case.AssetId e Entitlement.AssetId apontam para cá. Tem AssetShare, AssetHistory, AssetFeed, AssetOwnerSharingRule, AssetChangeEvent.

### `AssetRelationship`

**Funcao:** Relação não-hierárquica entre dois Assets, por modificação: substituição, upgrade, ou agrupamento em bundle. API 41.0+.

**Campos-chave:** AssetId (o ativo NOVO, que toma o lugar), RelatedAssetId (o antigo), AssetRelationshipNumber (autonumber), AssetRole, RelationshipType, FromDate, ToDate

**Relacoes:** Aparece nas related lists 'Primary Assets' e 'Related Assets'. Cuidado documentado: via REST getRelatedListInfo, a resposta de PrimaryAssets vem rotulada 'Related Assets' e vice-versa.

### `Entitlement`

**Funcao:** Direito de suporte que uma conta/contato tem. Base do SLA. Pode ser ancorado em asset, produto ou contrato de serviço.

**Campos-chave:** AccountId, AssetId, AssetWarrantyID, BusinessHoursId (obrigatório), ContractLineItemId, ServiceContractId, SlaProcessId, Type, StartDate, EndDate, IsPerIncident, CasesPerEntitlement, RemainingCases, LocationID

**Relacoes:** AccountId→Account; AssetId→Asset (garantia/contrato POR EQUIPAMENTO); ContractLineItemId→ContractLineItem; Case.EntitlementId consome o entitlement e dispara os Milestones do SLA.

### `Territory2 (Enterprise Territory Management)`

**Funcao:** Território de vendas. Não é apenas rótulo: é um mecanismo de CONCESSÃO DE ACESSO paralelo à hierarquia de papéis.

**Campos-chave:** Name, DeveloperName, ParentTerritory2Id, Territory2ModelId, Territory2TypeId, AccountAccessLevel (Read Only/Read Write/Owner), OpportunityAccessLevel, CaseAccessLevel (Private/Read Only/Read Write), ContactAccessLevel, ForecastUserId

**Relacoes:** Territory2Model.State: Planning, Activating, Activation Failed, Active, Archiving, Archiving Failed, Archived, Deleting, Deletion Failed — só UM modelo Active por vez. ObjectTerritory2Association liga território↔Account (API 30.0+) e território↔Lead (API 55.0+), com AssociationCause = Territory2AssignmentRule (regra) ou Territory2Manual. UserTerritory2Association liga território↔User com RoleInTerritory2 = Owner, Administrator ou Sales Rep. Opportunity.Territory2Id guarda o território do negócio.

### `RecordType`

**Funcao:** Metadado que define uma 'variante' de um objeto: qual layout, qual subconjunto de picklist e qual ciclo de vida se aplicam. Atribuído por Profile/Permission Set.

**Campos-chave:** Name (label, máx 80 chars), DeveloperName (API name, único), Description (máx 255 chars), IsActive, IsPersonType, BusinessProcessId, SobjectType, NamespacePrefix

**Relacoes:** BusinessProcessId→BusinessProcess é OBRIGATÓRIO para record types de Opportunity e Lead (API 17.0+). RecordTypeId aparece na maioria dos objetos. Exceção: CampaignMember.RecordTypeId não é setável — usa-se Campaign.CampaignMemberRecordTypeId.

### `BusinessProcess`

**Funcao:** Subconjunto ordenado de valores de picklist de ciclo de vida — Opportunity.StageName, Lead.Status, Case.Status, Solution.Status. É o que faz o Record Type mudar o PROCESSO, não só o formulário.

**Campos-chave:** fullName (formato 'Opportunity.Bulk Orders'), description, isActive, values (PicklistValue[])

**Relacoes:** Referenciado por RecordType.BusinessProcessId. Definido dentro da definição do CustomObject/StandardObject no Metadata API.

### `AccountShare / OpportunityShare (tabelas de compartilhamento)`

**Funcao:** Materialização das concessões de acesso por registro. Uma linha por (registro, usuário-ou-grupo, motivo).

**Campos-chave:** AccountId/OpportunityId, UserOrGroupId, AccountAccessLevel (Read | Edit | All — 'All' não é válido em create/update), OpportunityAccessLevel, CaseAccessLevel, ContactAccessLevel, RowCause

**Relacoes:** RowCause (picklist restrita, imutável após criação): Manual (único valor permitido ao criar via API), Owner, Team (AccountTeamMember/OpportunityTeamMember), Rule (sharing rule), GuestRule, ImplicitParent (o usuário tem acesso à conta porque é dono/tem acesso a um filho — opportunity, case, contact, contract, order), GuestParentImplicit. A doc alerta explicitamente para NÃO construir customizações que dependam da existência desses registros de share.

### `OpportunityContactRole`

**Funcao:** Junção Opportunity × Contact com o papel do contato no negócio (ex.: Decision Maker, Business User).

**Campos-chave:** OpportunityId, ContactId, Role, IsPrimary

**Relacoes:** Opportunity.ContactId é READ-ONLY e derivado do OpportunityContactRole marcado como IsPrimary — para trocar, muda-se a flag IsPrimary, não o campo. Alerta da doc: a API aplica o direito de acesso da Opportunity, mas NÃO o do Contact — a query pode retornar ContactIds que o usuário não tem direito de ver ou que já foram deletados.

## Achados

### 1. Lead Conversion: o que exatamente acontece, e o que é irreversível

A conversão cria (ou reaproveita) Account + Contact + opcionalmente Opportunity, e atualiza o Lead. Os Change Data Capture events disparam nesta ordem: (1) criação da Account, (2) criação do Contact ligado à Account, (3) criação da Opportunity (opcional), (4) update do Lead. No update do Lead são gravados: Status (para um valor de status convertido), IsConverted=true, ConvertedDate (DATA, sem hora), ConvertedAccountId, ConvertedContactId, ConvertedOpportunityId. Detalhe importante: o change event do update do Lead NÃO inclui LastModifiedDate.

O que é IRREVERSÍVEL: a Object Reference é literal — 'After a lead has been converted, it's read only. However, you can query converted lead records. Only users with the View and Edit Converted Leads permission can update converted lead records.' Em Apex, qualquer tentativa de update em outros campos de um lead convertido devolve CANNOT_UPDATE_CONVERTED_LEAD. Os três campos Converted*Id têm propriedades 'Create, Filter, Group, Nillable, Sort' — NÃO têm 'Update': depois de gravados, nem a API os altera.

Não existe operação 'unconvert' padrão. Os registros gerados (Account/Contact/Opportunity) continuam existindo se você deletar depois — o Lead não volta.

Outros efeitos confirmados: todas as atividades (Tasks e Events, abertas e fechadas) do Lead são reancoradas — Task.WhoId passa a apontar o novo Contact e Task.WhatId a Opportunity (ou fica em branco se nenhuma Opportunity foi criada). O CampaignMember do Lead é associado ao novo Contact, preservando a atribuição de origem de marketing. Campos customizados só sobrevivem se houver mapeamento explícito em Lead Convert Settings — campo customizado não mapeado PERDE o valor.

A API não permite converter mudando Status: 'You can't convert a lead via the API by changing Status to one of the converted lead status values.' É preciso chamar convertLead() / Database.convertLead(). O objeto LeadStatus expõe o flag IsConverted por valor de picklist, e vários valores podem representar 'convertido'.

A classe Database.LeadConvert controla a operação: setLeadId, setConvertedStatus (obrigatório), setAccountId (mescla em conta existente), setContactId (mescla em contato existente), setRelatedPersonAccountId (converte para person account), setOpportunityName (máx 80 caracteres), setOpportunityId, setDoNotCreateOpportunity, setOwnerId, setSendNotificationEmail, setOverwriteLeadSource. Retorna LeadConvertResult com os IDs gerados.

**Evidencia:** resources.docs.salesforce.com/latest/latest/en-us/sfdc/pdf/object_reference.pdf — seção 'Lead' > 'Converted Leads' e campos ConvertedAccountId/ConvertedContactId/ConvertedOpportunityId (extraído localmente em object_reference.txt, linhas 172425-176500); developer.salesforce.com/docs/atlas.en-us.change_data_capture.meta/change_data_capture/cdc_standard_objects_lead_conversion.htm; developer.salesforce.com/docs/atlas.en-us.apexref.meta/apexref/apex_dml_convertLead.htm; help.salesforce.com artigo 000384333 (CANNOT_UPDATE_CONVERTED_LEAD)

**Licao para a Tracbel:** COPIAR o conceito com uma correção. Implemente `ConverterLead` como um comando transacional único que grava, no Lead, os três FKs de destino + IsConverted + DataConversao, e trave o Lead via interceptor do EF Core (SaveChangesInterceptor) para rejeitar update em qualquer propriedade que não seja da whitelist de conversão. MAS: não copie a irreversibilidade absoluta — ela é a reclamação nº 1 de admins Salesforce. Guarde um `LeadConversaoLog` com o snapshot JSON do Lead pré-conversão e implemente `DesfazerConversao` restrita a um papel administrativo, que reverte os FKs e reabre o Lead. Custo: uma tabela e ~80 linhas. Ganho: elimina a classe inteira de chamados 'converti errado'. E, crítico: o mapa Lead→Account/Contact/Opportunity deve ser CONFIGURÁVEL em banco (tabela `MapeamentoConversao`), não hardcoded — é exatamente o tipo de mudança que a Tracbel precisa fazer sem release.

### 2. Por que o Salesforce separa Lead de Account/Contact/Opportunity — a decisão de modelagem e o preço dela

O Lead é uma tabela deliberadamente DESNORMALIZADA e autossuficiente: guarda no mesmo registro os dados da pessoa (FirstName, LastName, Email, Phone, Title) e os da empresa (Company — obrigatório —, Industry, NumberOfEmployees, AnnualRevenue, Rating). Ele é o único objeto do núcleo comercial que não exige nenhum relacionamento pai para existir.

A razão é operacional, não conceitual: dado de prospect entra sujo, duplicado e incompleto (formulário web, lista comprada, feira). Se você o gravasse já normalizado em Account+Contact, sujaria a base mestra de clientes — a mesma base usada por faturamento, cobrança, pós-venda e ERP. O Lead funciona como uma ANTECÂMARA (staging com UI): fica separado, tem seu próprio dono, sua própria fila (OwnerId de Lead e Case pode ser um Queue, não só um User — privilégio que a maioria dos objetos não tem), suas próprias regras de atribuição (AssignmentRule com RuleType='leadAssignment', aplicada via AssignmentRuleHeader no create/update), seu próprio ciclo (LeadStatus) e seu próprio flag de 'ainda não trabalhado' (IsUnreadByOwner).

A consequência que raramente é dita: você paga com DUPLICAÇÃO DE MODELO. Todo campo que importa nas duas fases (segmento, região, origem, produto de interesse) precisa existir duas vezes — no Lead e no Account/Contact/Opportunity — e precisa de mapeamento. Todo relatório de funil precisa unir duas tabelas com semânticas diferentes. Toda automação (validação, trigger, fluxo) tende a ser escrita duas vezes. Por isso o mercado criou um contramodelo — 'account-based', em que tudo nasce como Account/Contact e o Lead é abolido — que funciona bem justamente em cenários B2B com universo de clientes finito e conhecido.

A Tracbel é exatamente esse cenário: ~130 usuários, revenda regional John Deere, universo de produtores rurais/frotistas grande mas mapeável, com CNPJ/CPF como chave natural forte, e ERP (TOTVS/JDE) já sendo a fonte da verdade do cliente faturado.

**Evidencia:** object_reference.pdf, seção Lead (campos Company obrigatório, IsUnreadByOwner, AssignmentRuleHeader com assignmentRuleId/useDefaultRule); trailhead.salesforce.com/content/learn/modules/leads_opportunities_lightning_experience/create-and-convert-leads-lightning; salesforceben.com/salesforce-best-practices-lead-object/

**Licao para a Tracbel:** ADAPTAR, não copiar literalmente. Recomendação: UMA tabela `Pessoa` (equivalente ao GE_Pessoa que já existe, com 118k linhas e 76 colunas) com um discriminador de estágio — `Status` já faz isso hoje no Vórtice ('P'≈75k prospects, 'A'≈39k ativos). Ou seja: a Tracbel JÁ opera no modelo account-based sem Lead separado, e isso não é um defeito. Mantenha. O que você deve COPIAR do Lead do Salesforce são as três coisas que realmente agregam e que o Vórtice não tem bem resolvidas: (1) OwnerId que aceita FILA além de usuário — leads de campanha caem numa fila de pré-venda até alguém puxar; (2) o flag `NaoLidoPeloDono` (IsUnreadByOwner) que mostra ao gestor o que foi distribuído e não foi tocado; (3) as regras de atribuição declarativas em tabela. Se, mesmo assim, marketing exigir um pátio separado, use uma tabela `LeadBruto` como staging PURO — sem UI de gestão, sem pipeline, sem relatório — cujo único destino é virar Pessoa. Isso te dá a limpeza da antecâmara sem pagar a duplicação de modelo.

### 3. Conversão para Person Account: o gatilho é o campo Company estar NULO

Regra literal da doc no campo Lead.Company: 'Required. The lead's company. If person account record types have been enabled, and if the value of Company is null, the lead converts to a person account.' Ou seja, o mesmo campo que é marcado 'Required' é o switch que decide se o resultado da conversão é uma empresa (business account + contact separado) ou uma pessoa física (person account, que é um Account com um Contact filho embutido).

Person Account é um Account com IsPersonAccount=true que carrega campos Person* (PersonContactId, PersonEmail, PersonTitle, PersonBirthDate, PersonDepartment, PersonEmailBouncedDate, FirstName, LastName, Salutation, Suffix). PersonContactId (Filter/Nillable/Update) guarda o ID do Contact filho gerado automaticamente. A doc avisa: 'If the values in the IsPersonAccount Fields are not null, you can't change IsPersonAccount to false or an error occurs' — ou seja, uma vez person account, sempre person account.

Armadilha concreta e específica documentada: PersonTitle tem máximo de 80 caracteres, e 'When converting a lead to a person account, the conversion fails if the lead's Title field contains more than 80 characters'. Person accounts também não entram em Account Hierarchies e não têm relacionamento direto entre si (só via AccountContactRelation).

O RecordType do Account resultante também não é escolhido explicitamente: se o usuário tem mais de um business record type disponível, o Salesforce usa o marcado como Default em Profile > Record Type Settings.

**Evidencia:** object_reference.pdf, campo Lead.Company (linha ~172597 do extrato) e seção 'IsPersonAccount Fields' de Account (linhas 13330-13720); campo Account.PersonTitle; help.salesforce.com artigo 000389598 'How Account Record Type Is Determined During Lead Conversion'

**Licao para a Tracbel:** DESCARTAR o mecanismo, COPIAR o problema que ele resolve. Person Account é uma gambiarra arquitetural do Salesforce para contornar o fato de que Account é obrigatoriamente uma organização — a Tracbel não tem esse problema porque GE_Pessoa já resolve pessoa física e jurídica na MESMA tabela via `FisicaJuridica` ('F'/'J') + `NroCGCCPF`. Isso é estritamente melhor. Mantenha uma única entidade `Pessoa` com discriminador de natureza jurídica. O que a Tracbel deve importar é a LIÇÃO NEGATIVA: nunca deixe uma regra de negócio importante (pessoa física vs jurídica) ser inferida implicitamente de um campo estar nulo. Torne explícito: `TipoPessoa` como enum, validado, obrigatório. E copie a validação defensiva: valide comprimento de campo ANTES de executar a conversão/importação, devolvendo erro de domínio nomeado, e não deixe a operação falhar no meio.

### 4. Record Types + BusinessProcess: como um mesmo objeto vira vários formulários, picklists e ciclos de vida

Record Type é o mecanismo de variação por tipo de negócio. Um Record Type amarra três coisas a um mesmo objeto:
1. PAGE LAYOUT — a combinação (Profile do usuário × RecordType do registro) determina qual layout é renderizado. É por isso que um mesmo objeto Opportunity pode ter formulário de 'Máquina Nova', 'Usado' e 'Peças/Serviço'.
2. SUBCONJUNTO DE PICKLIST — a master picklist tem todos os valores; cada Record Type publica um subconjunto. Um vendedor de peças não vê estágios de financiamento de máquina.
3. BUSINESS PROCESS — e aqui está o ponto que quase todo mundo perde: RecordType.BusinessProcessId é OBRIGATÓRIO para record types de Opportunity e Lead (API 17.0+). O BusinessProcess é um subconjunto ORDENADO dos valores do picklist de ciclo de vida (Opportunity.StageName, Lead.Status, Case.Status, Solution.Status). É o BusinessProcess que faz o Record Type mudar o PROCESSO, não só a aparência. Metadata: fullName no formato 'Opportunity.Bulk Orders', com isActive e values (PicklistValue[]).

Campos do RecordType: Name (label, máx 80 chars), DeveloperName (API name único, só alfanumérico e underscore, não pode começar com número nem ter underscores consecutivos), Description (máx 255 chars), IsActive ('Only active record types can be applied to records'), IsPersonType, SobjectType, NamespacePrefix.

O efeito em cascata sobre a Opportunity: StageName é o campo mestre. A doc: 'If the StageName is updated, then the ForecastCategoryName, IsClosed, IsWon, and Probability are automatically updated based on the stage-category mapping.' Esse mapeamento vive no objeto OpportunityStage. IsClosed e IsWon são explicitamente não-setáveis: 'You can query and filter on this field, but you can't directly set it in a create, upsert, or update request. It can only be set via StageName.' ForecastCategory e Probability são implicados mas PODEM ser sobrescritos.

**Evidencia:** object_reference.pdf, objeto RecordType (linha 260024 do extrato) e campo Opportunity.StageName; api_meta.pdf (Metadata API Developer Guide), tipo BusinessProcess (linha 41793 do extrato); help.salesforce.com/s/articleView?id=customize_recordtype.htm

**Licao para a Tracbel:** COPIAR o conceito, SIMPLIFICAR a implementação — e é aqui que mora o requisito de 'extensibilidade sem release'. O Vórtice já tem o equivalente disfarçado: `IV_CodProcesso` é o catálogo de tipos de processo, e a memória do projeto registra que trocar o tipo 41 para 50 'troca o fluxo BPM inteiro' — exatamente o comportamento Record Type + BusinessProcess. Formalize isso em três tabelas de metadados, lidas em runtime pelo .NET 8 e servidas ao React como JSON: (a) `TipoRegistro` (Entidade, Nome, Ativo); (b) `EtapaFluxo` (TipoRegistroId, Codigo, Ordem, EhFechado, EhGanho, ProbabilidadePadrao, CategoriaPrevisao) — este é o BusinessProcess/OpportunityStage e é o que permite criar um novo funil sem deploy; (c) `LayoutFormulario` (TipoRegistroId, PerfilId, DefinicaoJson) para os campos/seções. Adote a regra de ouro do Salesforce: campos DERIVADOS do estágio (EstaFechado, FoiGanho) são calculados pelo domínio a partir da EtapaFluxo e NUNCA aceitos do cliente — no EF Core, mapeie-os com `.ValueGeneratedOnAddOrUpdate()` ou simplesmente como propriedades sem setter público. Com 2-3 devs isso é uma semana de trabalho e elimina anos de deploys por mudança de funil.

### 5. AVISO EXPLÍCITO DA DOCUMENTAÇÃO: Record Type e Business Process NÃO são mecanismo de controle de acesso

O Salesforce coloca este aviso em destaque, com a mesma redação, em DOIS lugares diferentes da documentação oficial — o que indica que é um erro recorrente e caro em campo.

Na Object Reference, objeto RecordType: 'Important: Don't use record types as an access control mechanism. Profile assignment governs create and edit access for an object but doesn't govern read access. For example, a user assigned to a profile that isn't enabled for a particular record type can't create records with that record type, but can access records associated with that record type. Users with access to an object can read all record type information for that object. We strongly recommend against storing sensitive information in the record type description, name, or label. Instead, store sensitive information in a separate object or fields to which you've applied appropriate access controls.'

No Metadata API Guide, tipo BusinessProcess, o aviso gêmeo: 'Don't use business processes as an access control mechanism. Profile assignment governs create and edit access for business process but doesn't govern read access. […] Don't store sensitive information in the business process description, name, or picklist values.'

Traduzindo o mecanismo: o Record Type filtra o que o usuário pode CRIAR e EDITAR, e filtra a UI. Ele não filtra o que o usuário pode LER. Um usuário sem o Record Type 'Governo' habilitado no perfil não consegue criar uma oportunidade desse tipo — mas consegue ler uma oportunidade desse tipo que caia no seu escopo de sharing, e consegue ler a definição do próprio Record Type.

**Evidencia:** resources.docs.salesforce.com/latest/latest/en-us/sfdc/pdf/object_reference.pdf, objeto RecordType, bloco 'Important' (linha ~260031 do extrato); resources.docs.salesforce.com/latest/latest/en-us/sfdc/pdf/api_meta.pdf, tipo BusinessProcess, bloco 'Important' (linha ~41797 do extrato)

**Licao para a Tracbel:** DESCARTAR frontalmente — e escreva isto no CLAUDE.md/ADR do projeto. É a armadilha mais provável de um time de 2-3 devs construindo CRM próprio: usar o `TipoRegistro` para esconder dados ('vendedor de peças não vê oportunidade de máquina') porque é o caminho mais barato na UI. Isso produz um buraco de segurança silencioso: a API REST, o export de relatório e qualquer query direta ignoram o filtro de UI. Regra a adotar: `TipoRegistro` decide APENAS forma (layout, picklist, fluxo, campos obrigatórios). Visibilidade de linha é decidida EXCLUSIVAMENTE pela camada Owner+Compartilhamento, aplicada como um EF Core Global Query Filter no DbContext, para que TODA consulta — API, relatório, job — passe pelo mesmo funil por construção. Um teste de integração obrigatório: para cada endpoint, um usuário sem direito recebe 404/lista vazia, não a linha filtrada só no front.

### 6. Os três tipos de relacionamento: Lookup, Master-Detail e Hierarchical — semântica oficial

A Object Reference define com precisão, no capítulo 'Relationships Among Standard Objects and Fields':

MASTER-DETAIL (1:n) — 'A parent-child relationship where the master object controls certain behaviors of the detail object.' Quatro consequências, textuais:
• 'When a record of the master object is deleted, its related detail records are also deleted.' (cascade delete)
• 'The Owner field on the detail object isn't available and is automatically set to the owner of its associated master record. Custom objects on the detail side of a master-detail relationship can't have sharing rules, manual sharing, or queues, because these elements require the Owner field.' (o filho PERDE o OwnerId)
• 'The detail record inherits the sharing and security settings of its master record.'
• 'The master-detail relationship field is required on the page layout of the detail record.'
• 'By default, records can't be reparented in master-detail relationships' — só com a opção Allow reparenting (atributo reparentableMasterDetail no Metadata API).

LOOKUP (1:n) — 'This type of relationship links two objects, but has no effect on deletion or security. Unlike master-detail fields, lookup fields aren't automatically required.' O comportamento de deleção é configurável via DeleteConstraint: Cascade (deleta o filho), Restrict (impede deletar o pai), SetNull (default — limpa o campo).

HIERARCHICAL — tipo de campo próprio no Metadata API (FieldType 'Hierarchy'), e o Trailhead é explícito: é um lookup especial 'only available on the User object', usado para cadeia de gestão (ex.: ManagerId). Não confundir com auto-lookup: Account.ParentId, Contact.ReportsToId, Asset.ParentId, Case.ParentId e Campaign.ParentId são LOOKUPS que apontam para o próprio objeto — isso é auto-relacionamento, não 'hierarchical relationship' no sentido do metadado.

MANY-TO-MANY — se faz com um junction object que tem DOIS master-detail. O atributo relationshipOrder define qual pai é primário (0) e qual é secundário (1): 'The definition of primary or secondary affects delete behavior and inheritance of look and feel, and record ownership for junction objects.'

E o atributo que quase ninguém conhece, writeRequiresMasterRead: 'Sets the minimum sharing access level required on the primary record to create, edit, or delete child records. true = usuários com acesso Read no pai podem criar/editar/excluir filhos (menos restritivo). false = exige Read/Write no pai (mais restritivo, e é o DEFAULT). For junction objects, the most restrictive access from the two parents is enforced.'

**Evidencia:** object_reference.pdf, cap. 1, seção 'Relationships Among Standard Objects and Fields' (linhas 3145-3220 do extrato); api_meta.pdf, tipo CustomField — atributos deleteConstraint (linha ~42165), relationshipOrder (~42400), reparentableMasterDetail (~42424), writeRequiresMasterRead (~42552), e enum FieldType (~44469); trailhead.salesforce.com/content/learn/modules/data_modeling/object_relationships

**Licao para a Tracbel:** COPIAR a distinção conceitual, DESCARTAR a implementação. Em EF Core você não precisa de dois 'tipos de relacionamento' — precisa de duas DECISÕES explícitas por FK, e o erro clássico é deixá-las implícitas no default do ORM. Padronize no projeto: para cada relação, declare (a) comportamento de deleção via `OnDelete(DeleteBehavior.Cascade | Restrict | SetNull)` — que é literalmente o DeleteConstraint do Salesforce, então documente cada escolha; e (b) se o filho tem DONO PRÓPRIO ou herda o do pai. Regra prática derivada do Salesforce: `ItemOportunidade`, `ItemPedido`, `ItemProposta` são 'detail' — sem OwnerId, cascade delete, e a visibilidade deriva 100% do pai (implemente o Global Query Filter do filho como um EXISTS sobre o filtro do pai, nunca como uma regra independente que pode divergir). `Equipamento` (Asset), `Contrato`, `Chamado` são 'lookup' — têm dono próprio e sobrevivem ao pai. E copie o writeRequiresMasterRead como decisão consciente: para editar um item, exigir permissão de ESCRITA no pai (o default mais restritivo do Salesforce), não apenas leitura.

### 7. Objetos padrão que NÃO podem ser o lado 'master' — e a assimetria padrão vs. customizado

Restrição textual da Object Reference: 'You can define master-detail relationships between custom objects or between a custom object and a standard object. But the standard object can't be on the detail side of a relationship with a custom object. And you can't create a master-detail relationship where these standard objects are the primary object: BusinessHours, Idea, Lead, OrderItem, PriceBook2, Product2, QuoteLineItem, User.'

Duas regras embutidas, ambas com consequência prática séria:
1. Um objeto PADRÃO nunca pode ser filho (detail) de um objeto CUSTOMIZADO. Você não pode fazer sua Opportunity ser detail do seu objeto customizado 'Projeto'. Só o inverso.
2. Oito objetos padrão são proibidos até de serem PAI. Note quem está na lista: Lead, Product2, Pricebook2, OrderItem, QuoteLineItem — ou seja, você não pode pendurar um filho com herança de sharing e cascade delete num Lead nem num Produto.

O efeito colateral disso no mundo real é que integradores acabam usando Lookup + trigger Apex para simular cascade delete e herança de sharing nesses casos — código customizado replicando o que a plataforma faria, com todos os bugs que isso implica.

Observe também o que a modelagem padrão REALMENTE faz: OpportunityLineItem.OpportunityId, QuoteLineItem.QuoteId e OrderItem.OrderId são declarados 'Relationship Type: Lookup' no metadado — não Master-detail. Mas OpportunityId tem propriedades 'Create, Filter, Group, Sort' sem Nillable e sem Update, e a descrição diz 'Required'. Isto é: nos objetos padrão o Salesforce hardcoda o comportamento (obrigatório, imutável, cascade) em vez de usar o tipo Master-Detail, que é um recurso da camada de customização. Master-Detail de verdade só aparece em objetos mais novos, como QuoteLineGroup.QuoteId, OrderChangeLog.RelatedOrderId e PriceProtectExecLineItem.PriceProtectionExecutionId, todos marcados 'Refers To: X (the master object)'.

**Evidencia:** object_reference.pdf, seção 'Relationships Among Standard Objects and Fields'; e campos OpportunityLineItem.OpportunityId (linha ~205520), QuoteLineGroup.QuoteId (~255729), OrderChangeLog.RelatedOrderId (~209668) do extrato

**Licao para a Tracbel:** DESCARTAR — essas restrições são cicatriz de uma plataforma multi-tenant que tem que proteger o schema compartilhado de milhões de orgs. A Tracbel tem SQL Server próprio e não herda nenhuma dessas amarras. Mas extraia duas lições reais: (1) A assimetria 'padrão não pode ser filho de customizado' é um sintoma de plataforma fechada — é justamente o que a Tracbel está fugindo ao construir CRM próprio, e é o argumento a registrar no ADR do projeto. (2) Note o que a própria Salesforce FEZ na prática nos objetos de linha: campo pai obrigatório, imutável após criação, com cascade. Adote isso como padrão explícito para todas as linhas de item no CRM Tracbel: `OportunidadeId` como `required`, sem setter público após a criação (defina no construtor da entidade), com `OnDelete(DeleteBehavior.Cascade)`. Reparenting de item entre pedidos deve ser uma operação de domínio explícita e auditada, não um `entity.OportunidadeId = x` solto no serviço.

### 8. Limites numéricos de relacionamento e campos — o que é oficial e o que é folclore

CONFIRMADO na doc oficial de limites (Salesforce Developer Limits and Allocations Quick Reference, PDF): 'No more than 55 child-to-parent relationships can be specified in a query. A custom object allows up to 40 relationships, so you can reference all the child-to-parent relationships for a custom object in one query.' — o teto de 40 relacionamentos por objeto customizado está afirmado ali, de forma indireta mas oficial.

Outros limites confirmados no mesmo PDF, relevantes para quem projeta queries:
• Relationship queries retornam no máximo 2.000 resultados totais (API 28.0+), incluindo resultados de objetos filhos; versões anteriores retornavam 200.
• No máximo 20 relacionamentos parent-to-child por query.
• No máximo 5 níveis em um caminho child-to-parent (ex.: Contact.Account.Owner.FirstName são 3 níveis).
• Até API 57.0, só 2 níveis de parent-to-child por query; da API 58.0 em diante, até 5 níveis (não suportado em big objects, external objects, Bulk API 1.0/2.0).
• Uma query em campo polimórfico pode contar VÁRIAS vezes contra o limite de child-to-parent.

Amplamente documentado mas confirmado apenas em fontes secundárias e artigo de help (id=000386135 'Increase the maximum relationships (master-detail plus lookup) allowed per object'):
• 2 relacionamentos master-detail por objeto (máximo).
• 40 relacionamentos totais (master-detail + lookup) por objeto — o help article existe justamente para pedir aumento desse teto.
• Boa prática: não passar de 10.000 registros filhos por relação master-detail.
• Campos customizados por objeto: 500 (Enterprise) / 800 (Unlimited e Performance).

CONFIRMADO na doc: hierarquia de Assets suporta até 10.000 ativos (ver achado específico de Asset).

**Evidencia:** resources.docs.salesforce.com/latest/latest/en-us/sfdc/pdf/salesforce_app_limits_cheatsheet.pdf, seção 'Relationship queries / Relationship query limits' (linhas 1140-1180 do extrato local limits.txt); help.salesforce.com/s/articleView?id=000386135; help.salesforce.com/s/articleView?id=platform.custom_field_allocations.htm

**Licao para a Tracbel:** DESCARTAR os limites, COPIAR a disciplina que eles impõem. Nenhum desses tetos existe em SQL Server + EF Core — mas eles funcionaram como um governador de complexidade que a Tracbel não terá de graça. Dois riscos concretos a mitigar: (1) O limite de 2.000 linhas em relationship query é o que impede o Salesforce de servir uma tela que carrega tudo. Adote a mesma disciplina: proíba `.Include()` encadeado em listagem, use projeção para DTO (`.Select()`), e paginação obrigatória em toda coleção — inclusive nas 'abas de detalhe'. O Vórtice tem tabelas com 2,44M linhas (IV_Historico) e 1,17M (IV_Processo): sem essa disciplina desde o dia 1, a tela de cliente vai travar. (2) O teto de 5 níveis de navegação é um bom limite AUTOIMPOSTO para legibilidade — se uma regra precisa de `a.B.C.D.E.F`, o modelo está errado. Adicione um lint/revisão de código para isso. Para o limite de 40 relacionamentos: use-o como métrica de alerta — uma entidade com mais de ~30 FKs quase certamente está fazendo trabalho demais e deve ser quebrada.

### 9. Campos de auditoria padrão: a lista exata, a semântica de cada um, e a pegadinha do SystemModstamp

A Object Reference define um capítulo 'System Fields' — campos read-only presentes na maioria dos objetos, atualizados automaticamente:

Identidade / estado:
• Id (tipo ID) — 'Globally unique string that identifies a record… analogous to a primary key'. Contém um código de 3 caracteres que identifica o TIPO do objeto. Não pode ser atualizado.
• IsDeleted (boolean) — soft delete: indica se foi movido para a Lixeira. É o que permite undelete().
• LastReferencedDate / LastViewedDate (dateTime) — por usuário corrente; 'This field doesn't exist on records for a custom object unless a custom object tab is created for that object'.

Audit Fields (assim nomeados na doc):
• CreatedById (reference → User) + CreatedDate (dateTime)
• LastModifiedById (reference → User) + LastModifiedDate (dateTime) — 'Date and time when a USER last modified this record'
• SystemModstamp (dateTime) — 'Date and time when a user OR AUTOMATED PROCESS (such as a trigger) last modified this record.'

A distinção LastModifiedDate vs SystemModstamp é a razão de existir dois campos: o primeiro é 'quando um humano mexeu', o segundo é 'quando a linha mudou por qualquer motivo' — é o campo correto para sincronização incremental / CDC. E há um alerta explícito: 'SystemModstamp doesn't capture every field change. For example, if object A retrieves values from object B, then the changes to field values in records on object B are reflected in the SystemModstamp field for records on object B, but not on object A.' Ou seja, campos calculados/roll-up que mudam por causa do filho NÃO mexem no SystemModstamp do pai.

Faixa de datas válidas dos audit fields: 'the earliest valid date is 1970-01-01T00:00:00Z GMT… The latest valid date is 4000-12-31T00:00:00Z GMT' — diferente dos outros campos dateTime.

Migração: por padrão os audit fields são preenchidos pelo sistema, mas com a permissão 'Set Audit Fields upon Record Creation' você pode gravá-los na criação via API, preservando a auditoria do sistema de origem. Lista exata de objetos que suportam isso: Account, ArticleVersion, Attachment, CampaignMember, Case, CaseComment, Contact, ContentVersion, Contract, Event, Idea, IdeaComment, Lead, Opportunity, Question, Task, Vote e objetos customizados. 'The only audit field you cannot set a value for is systemModstamp.'

IDs: 15 caracteres base-62 case-SENSITIVE, ou 18 caracteres case-SAFE (os 3 extras codificam o case dos 15 primeiros). A doc recomenda sempre usar 18. 'Don't use 15-character IDs in case-insensitive applications like Microsoft Access.'

**Evidencia:** resources.docs.salesforce.com/latest/latest/en-us/sfdc/pdf/object_reference.pdf, cap.1 seções 'System Fields' (linhas 2419-2500 do extrato) e 'ID Field Type' (linhas 2216-2250)

**Licao para a Tracbel:** COPIAR quase integralmente — é o achado mais diretamente aplicável deste relatório. Defina uma classe base `EntidadeAuditavel` com CriadoPorId, CriadoEm, AlteradoPorId, AlteradoEm, `Versao` (rowversion/timestamp do SQL Server para concorrência otimista) e `Excluido` (soft delete). Preencha via `SaveChangesInterceptor` no EF Core, nunca manualmente no serviço — o Vórtice tem exatamente esse padrão hoje (`DtaInclusao`/`UsuInclusao`, `DtaAlteracao`/`UsuAlteracao`, `DtaInativacao`), então mantenha os nomes para facilitar migração. TRÊS coisas a copiar especificamente: (1) Separe 'alterado por humano' de 'alterado por processo' — crie `AlteradoEm` (usuário) e `SincronizadoEm`/rowversion (qualquer motivo, inclusive job de integração TOTVS/JDE). Sem isso, o job de sync noturno vai poluir a auditoria e você perde a capacidade de responder 'quem mexeu nisso'. (2) Soft delete via `Excluido` + Global Query Filter — permite desfazer, e é obrigatório num CRM onde vendedor apaga coisa por engano. (3) Habilite a gravação de audit fields na CARGA INICIAL de migração do Vórtice: sem isso, 118k pessoas e 1,17M processos chegam todos 'criados hoje por sistema' e você perde a história inteira. Planeje esse caminho de importação desde o começo — no Salesforce é uma permissão especial justamente porque foi esquecido por muita gente.

### 10. OwnerId: a âncora única da segurança por registro — e os efeitos colaterais que ninguém espera

A doc é explícita sobre o peso do campo: 'Objects have an ownerId field that is a reference to the user who owns that object. Ownership is an important concept that affects the security model and has other implications throughout the system.'

As regras exatas de escrita:
• 'For most users and most objects, this field can't be set directly upon insert. It is implicitly set to the current user when inserting an object.'
• 'When creating or updating a Case or Lead, a client application (logged in with sufficient permissions to transfer a record) can set this field to any valid User in the organization or to ANY VALID QUEUE of the appropriate type.' — Case e Lead são os únicos que aceitam FILA como dono.
• 'To update the ownerId field, the user must have the "Transfer Record" permission and Read access to the new owner.'

Dois efeitos colaterais que causam incidentes reais em produção:
1. 'Updating this field via the API changes ONLY the owner of that record. The change of ownership does NOT cascade to associated records as it does when you transfer record ownership in the Salesforce user interface.' — a UI e a API se comportam DIFERENTE. Trocar o dono de uma conta pela UI arrasta os filhos; pela API, não.
2. 'Updating this field on an account DELETES the existing sharing information and reapplies the organization-wide sharing defaults and sharing rules.' — trocar o dono de uma conta DESTRÓI os compartilhamentos manuais existentes.
E o reforço no capítulo de acesso: 'Ownership changes to a record do not automatically cascade to related records. For example, if ownership changes for a given Account, ownership does not then automatically change for any Contract associated with that Account — each ownership change must be made separately and explicitly by the client application.'

O Salesforce empilha sobre o OwnerId as camadas: Organization-Wide Defaults (a linha de base — 'the access the most restricted user should have'), Role Hierarchy (gestor vê o que o subordinado vê), Sharing Rules (exceções automáticas por critério ou por dono), Manual Sharing, Teams (AccountTeamMember/OpportunityTeamMember), Territory2 e Apex Sharing.

Acima de tudo isso, as permissões que ATROPELAM sharing: View All Records / Modify All Records (por objeto) e View All Data / Modify All Data (globais). E a advertência: 'give the logged-in user only the permissions needed'.

**Evidencia:** object_reference.pdf, cap.1 seção 'Frequently Occurring Fields' > OwnerId (linhas 2508-2530 do extrato) e seção 'Factors that Affect Data Access' (linhas 3222-3290); developer.salesforce.com/blogs/developer-relations/2017/04/salesforce-data-security-model-explained-visually; trailhead.salesforce.com/content/learn/modules/data_security/data_security_records

**Licao para a Tracbel:** COPIAR o modelo, com uma correção decisiva. Adote `DonoId` em todas as entidades de negócio (Pessoa, Oportunidade, Chamado, Contrato, Equipamento) — o Vórtice já tem os embriões (`GE_Pessoa.CODVENDEDOR`, `IV_Processo.UsuResponsavel`, `IV_Agenda.SeqUsuario`) mas dispersos e sem semântica unificada; unifique. Implemente a hierarquia via um `HierarquiaUsuario` com CAMINHO MATERIALIZADO (ex.: '/1/17/93/') em vez de auto-lookup recursivo: um `LIKE '/1/17/%'` resolve 'tudo do gestor e dos subordinados' em um índice, sem CTE recursiva a cada request. Isso resolve diretamente o problema já registrado na memória do projeto ('gerente não vê carteira de pedidos = hierarquia presa no usuário antigo'). Copie também as camadas: Padrão da organização por entidade → Hierarquia → Regras de compartilhamento → Compartilhamento manual → Equipe. E copie as duas ADVERTÊNCIAS como requisitos: (1) transferência de titularidade deve ser uma OPERAÇÃO DE DOMÍNIO explícita (`TransferirCarteira(deId, paraId, incluirFilhos: bool)`), nunca um `entity.DonoId = x`, e deve se comportar IGUAL na API e na UI — o bug do Salesforce de UI≠API é exatamente o que quebra automação; (2) essa operação nunca deve destruir compartilhamentos silenciosamente — recalcule e registre no log o que foi removido.

### 11. Tabelas de compartilhamento e RowCause: como o Salesforce materializa 'por que este usuário vê este registro'

Cada objeto compartilhável tem uma tabela de share (AccountShare, OpportunityShare, CaseShare, AssetShare, LeadShare…). Estrutura: o ID do registro, UserOrGroupId (usuário OU grupo), o nível de acesso, e — o campo mais interessante — RowCause: 'Reason that this sharing entry exists.'

Valores de RowCause em AccountShare: Manual, Owner, Team, Rule, GuestRule, ImplicitParent, GuestParentImplicit.
• Manual — 'a User with "All" access manually shared the Account'. É o ÚNICO valor permitido ao criar via API: 'If you're creating a sharing entry, the only permitted value is Manual… All other RowCause values are read-only. After the sharing entry is created, this field can't be edited.'
• Owner — o usuário é o dono.
• Team — é um AccountTeamMember.
• Rule — veio de uma sharing rule.
• ImplicitParent — 'The User or Group has access because they're the owner of or have sharing access to records related to the account, such as opportunities, cases, contacts, contracts, or orders.' Este é o IMPLICIT SHARING: quem tem acesso a uma oportunidade ganha acesso de leitura à conta dela, automaticamente.

Níveis de acesso em AccountShare: AccountAccessLevel = Read | Edit | All, sendo que 'All' não é válido em create/update; e níveis separados por objeto filho: CaseAccessLevel, ContactAccessLevel, OpportunityAccessLevel — ou seja, o compartilhamento da conta carrega junto, e de forma independente, o nível de acesso concedido aos filhos.

E o aviso arquitetural mais importante da seção: 'While Salesforce currently maintains read-only sharing entries for multiple sharing mechanisms, it's possible that we'll stop storing certain share records to improve performance. As a best practice, don't create customizations that rely on the availability of these sharing entries.' Traduzindo: a materialização é um detalhe de implementação, e é cara — a Salesforce reserva o direito de parar de materializar.

**Evidencia:** object_reference.pdf, objeto AccountShare, campos RowCause e AccountAccessLevel (linhas 17486-17800 do extrato)

**Licao para a Tracbel:** COPIAR o RowCause, SIMPLIFICAR a materialização. Duas decisões específicas. (1) COPIE o RowCause. Uma tabela `CompartilhamentoRegistro(Entidade, RegistroId, UsuarioOuGrupoId, Nivel, Motivo, CriadoEm)` onde `Motivo` é um enum tipado (Dono, Hierarquia, Regra, Manual, Equipe, Territorio, PaiImplicito). Isso resolve dois problemas de uma vez: responde à pergunta de suporte 'por que o Fulano vê isso?' — que hoje na Tracbel exige arqueologia em SQL — e permite recalcular só um motivo sem apagar os outros (o bug do Salesforce em que trocar o dono da conta apaga os compartilhamentos manuais existe porque a recalculação não é seletiva). (2) NÃO materialize tudo. Com ~130 usuários e volumes de 1-2M linhas, materializar o produto cartesiano usuário×registro é overengineering caro. Materialize APENAS os compartilhamentos explícitos (Manual, Equipe, Regra), e resolva Dono + Hierarquia + PaiImplicito em tempo de query, dentro do Global Query Filter, usando o caminho materializado da hierarquia. Você fica com uma tabela pequena e um filtro de 3 predicados indexáveis. (3) COPIE o implicit sharing como regra explícita: quem enxerga a Oportunidade enxerga a Pessoa dela (senão a tela quebra com 'cliente não encontrado'), mas o inverso NÃO vale.

### 12. Opportunity: StageName como campo mestre, e Amount que vira read-only quando há itens

A Opportunity concentra várias regras derivadas que valem estudo porque são exatamente o tipo de coisa que times reimplementam errado.

Cadeia do StageName (obrigatório, picklist): 'The StageName field controls several other fields on an opportunity… If the StageName is updated, then the ForecastCategoryName, IsClosed, IsWon, and Probability are automatically updated based on the stage-category mapping.' O mapeamento vive no objeto OpportunityStage. IsClosed e IsWon são 'Defaulted on create, Filter, Group, Sort' — não têm Create nem Update: 'You can query and filter on this field, but you can't directly set it in a create, upsert, or update request. It can only be set via StageName.' Já ForecastCategory e Probability são 'implied, but not directly controlled' — podem ser sobrescritos.
Valores de ForecastCategory (picklist restrita, read-only na prática desde a v12.0): BestCase, Closed, Forecast, MostLikely, Omitted, Pipeline. Valores de ForecastCategoryName (o editável): Best Case, Closed, Commit, Most Likely, Omitted, Pipeline.

Regra do Amount vs itens — textual: 'Estimated total sale amount. For opportunities with products, the amount is the sum of the related products. Any attempt to update this field, if the record has products, will be ignored. The update call will not be rejected, and other fields will be updated as specified, but the Amount will be unchanged.' Note a semântica perversa: a atualização é SILENCIOSAMENTE ignorada, não rejeitada.
Na seção 'Effects on Opportunities': criar um OpportunityLineItem incrementa Amount pelo TotalPrice do item e incrementa ExpectedRevenue por TotalPrice × Probability. 'You can't update the PricebookId field or the CurrencyIsoCode field on the opportunity if line items exist. The API REJECTS any attempt' — aqui rejeita.
ExpectedRevenue: 'Read-only field that is equal to the product of the opportunity Amount field and the Probability.'

Outros campos derivados: ContactId é read-only e derivado do OpportunityContactRole com IsPrimary=true — 'To update the value in this field, change the IsPrimary flag on the OpportunityContactRole'. HasOpportunityLineItem, HasOpenActivity, HasOverdueTask são read-only. Description tem limite de 32.000 caracteres.

Quote sync: Opportunity.SyncedQuoteID — 'Read only in an Apex trigger. The ID of the Quote that syncs with the opportunity. Setting this field lets you start and stop syncing… The ID has to be for a quote that is a child of the opportunity.'

**Evidencia:** object_reference.pdf, objeto Opportunity — campos StageName, Amount, ExpectedRevenue, IsClosed, ForecastCategory, ContactId, SyncedQuoteID (linhas 203556-204560 do extrato); e objeto OpportunityLineItem, seção 'Effects on Opportunities' (~205900)

**Licao para a Tracbel:** COPIAR o padrão de campo derivado, mas com falha ALTA em vez de silenciosa. Três aplicações diretas: (1) Modele `EstaFechada` e `FoiGanha` como propriedades CALCULADAS a partir da `EtapaFluxo` (tabela de metadados), com `private set` ou sem setter — jamais como colunas que a API aceita. Isso mata a classe de bug 'oportunidade marcada como ganha mas ainda na fase de negociação' que aparece em todo CRM feito à mão. Persista-as (colunas computadas ou preenchidas no domínio) para poder indexar e filtrar. (2) Copie a regra 'Valor vira derivado quando há itens' — é a decisão certa e resolve divergência entre cabeçalho e itens. MAS não copie o comportamento de ignorar em silêncio: lance uma exceção de domínio (`ValorNaoEditavelComItensException`) e devolva 409/422 na API. Ignorar update silenciosamente é a origem de tickets insolúveis do tipo 'salvei e não gravou'. (3) Copie a proibição de trocar a lista de preço quando já há itens (o Salesforce REJEITA nesse caso) — no CRM Tracbel isso vira: `TrocarTabelaDePreco` só é permitido em proposta sem itens, ou exige recálculo explícito de todos os itens com confirmação do usuário. (4) Note que Opportunity.ContactId derivar do papel primário é elegante: no CRM Tracbel, `ContatoPrincipalId` da oportunidade deve derivar de `PapelContatoOportunidade.EhPrincipal`, garantindo unicidade por índice filtrado único no SQL Server.

### 13. Product2 → Pricebook2 → PricebookEntry: por que o preço não mora no produto

O Salesforce separa deliberadamente três coisas: O QUE se vende (Product2), a LISTA em que se vende (Pricebook2) e o PREÇO naquela lista (PricebookEntry). Product2 não tem campo de preço.

Pricebook2: 'Each org has ONE standard price book that defines the standard or generic list price for each product or service that it sells. An org can have multiple custom price books to use for specialized purposes, such as for discounts, different channels or markets, or select accounts or opportunities.' Restrição: 'your client application can create, delete, and update custom price books, [but] your client application can only UPDATE the standard price book' — o padrão não pode ser criado nem deletado.

PricebookEntry é a junção: 'Create one PricebookEntry record for each standard or custom price and currency combination for a product in a Pricebook2.' A chave lógica é (Pricebook2Id, Product2Id, CurrencyIsoCode) — MOEDA faz parte da granularidade. Pricebook2Id e Product2Id são obrigatórios e IMUTÁVEIS: 'Once these records are created, your client application can't update these IDs.'

O campo UseStandardPrice é o mecanismo de herança de preço: 'If set to true, then the UnitPrice field is read-only, and the value is the same as the UnitPrice value in the corresponding PricebookEntry in the standard price book (that is, the PricebookEntry record whose Pricebook2Id refers to the standard price book and whose Product2Id and CurrencyIsoCode are the same as this record). For PricebookEntry records associated with the standard Pricebook2 record, this field must be set to true.'

Regra de exclusão que é uma lição de integridade histórica: 'If you delete a PriceBookEntry that is referenced by a line item, the line item is unaffected, but the PriceBookEntry is ARCHIVED and unavailable from the API. Deleted PriceBookEntry records can't be recovered.' Também: 'Although you can never delete PricebookEntry records, your client application can set [IsActive] to false.'

E o encadeamento completo: OpportunityLineItem / QuoteLineItem / OrderItem apontam para PricebookEntryId — NUNCA direto para Product2. Product2Id nesses objetos é read-only e derivado (em OpportunityLineItem, 'This is a read-only field available in API version 30.0 and later'). O campo ListPrice do item 'Corresponds to the UnitPrice on the PricebookEntry… A client application can use this information to show whether the unit price (or sales price) of the line item differs from the price book entry list price' — ou seja, guarda-se o preço de tabela vigente ao lado do preço praticado, para calcular desconto.

**Evidencia:** object_reference.pdf, objetos Pricebook2 (linha ~236285 do extrato, seção Usage), PricebookEntry (~236622, campos UseStandardPrice/UnitPrice/IsActive e seção Usage) e OpportunityLineItem (campos PricebookEntryId, Product2Id, ListPrice); developer.salesforce.com/docs/platform/data-models/guide/product-price-book.html

**Licao para a Tracbel:** SIMPLIFICAR agressivamente — copiar a estrutura inteira aqui seria um erro para um time de 2-3 pessoas. O que COPIAR (2 coisas, baratas e de alto valor): (1) O princípio de que preço não é atributo do produto: `Produto` (catálogo, vem do TOTVS/JDE/EXT_Produto) separado de `PrecoProduto(ProdutoId, TabelaId, VigenciaInicio, VigenciaFim, Valor)`. A Tracbel vende máquina nova, usada, peça e serviço com lógicas de preço diferentes; amarrar preço ao produto vai travar em 6 meses. (2) O padrão ListPrice ao lado de UnitPrice no ITEM: grave no item da proposta/pedido tanto `ValorTabela` (snapshot do preço vigente no momento) quanto `ValorPraticado`. Isso torna o desconto auditável e imune a mudança futura de tabela — é a diferença entre conseguir e não conseguir responder 'que desconto demos ao cliente X em março'. O que DESCARTAR: a obrigatoriedade de a linha apontar para PricebookEntry em vez de Produto (complica toda query sem ganho fora de multi-moeda), o conceito de 'standard price book' único, e o UseStandardPrice (herança implícita de preço é fonte de bug). O que ADAPTAR: a regra 'nunca delete, arquive' — para preço isso é obrigatório (VigenciaFim em vez de DELETE), senão você perde a capacidade de reconstituir uma proposta antiga.

### 14. Task e Event: WhoId/WhatId polimórficos — o padrão de 'atividade se liga a qualquer coisa'

Atividades no Salesforce (Task = tarefa, Event = compromisso de calendário) usam DOIS campos polimórficos com semântica distinta:

• WhoId — a PESSOA. 'Refers To: Contact, Lead'. Label na UI: 'Name'.
• WhatId — o OBJETO DE NEGÓCIO. Textual: 'The WhatId represents nonhuman objects such as accounts, opportunities, campaigns, cases, or custom objects. WhatIds are polymorphic. Polymorphic means a WhatId is equivalent to the ID of a related object. The label is Related To ID.' A lista de 'Refers To' é enorme e inclui Account, Opportunity, Order, Product2, Contract, Case, Campaign, Asset, Invoice, MemberPlan e dezenas de objetos de indústria.

Ambos são 'Create, Filter, Group, Nillable, Sort, Update' e o Relationship Type é Lookup (polimórfico).

Com a configuração 'Shared Activities' ligada, o modelo vira N:N: TaskWhoIds e EventWhoIds são campos do tipo JunctionIdList — 'A string array of contact or lead IDs used to create many-to-many relationships with a shared event. The first contact or lead ID in the list becomes the primary WhoId if you don't specify a primary WhoId.' Alerta explícito: 'Warning: Adding a JunctionIdList field name to the fieldsToNull property DELETES ALL related junction records. This action can't be undone.' Também WhoCount e WhatCount contam os relacionamentos, e 'The count of the WhatId must be 1 or less' — a atividade se liga a N pessoas mas a 1 só objeto de negócio.

Acima disso existem dois objetos READ-ONLY que só suportam describeSObjects(): ActivityHistory e OpenActivity. ActivityHistory: 'This read-only object is displayed in a related list of closed activities — past events and closed tasks — related to an object. It INCLUDES ACTIVITIES FOR ALL CONTACTS RELATED TO THE OBJECT.' O AccountId dele é derivado por uma cascata: 'The account associated with the WhatId, if it exists; or the account associated with the WhoId, if it exists; otherwise null.'

**Evidencia:** object_reference.pdf, objeto Event — campos WhatId, WhoId, EventWhoIds, WhoCount/WhatCount (linhas 118284-119400 do extrato); objeto ActivityHistory (linha 23776); objeto Task (~24394)

**Licao para a Tracbel:** ADAPTAR com cuidado — o polimorfismo é sedutor e caro. O Vórtice já resolve isso melhor em um aspecto: `IV_Historico` (2,44M linhas) e `IV_Agenda` (932k) têm FKs TIPADAS — `SeqPessoa`, `Processo`/`CodProcesso`, `SeqUsuario` — em vez de um WhatId genérico. Isso permite FK real, índice eficiente e integridade referencial no banco, coisas que WhatId não permite. RECOMENDAÇÃO: mantenha o modelo do Vórtice como base — `Atividade` com `PessoaId` (nullable) + `OportunidadeId` (nullable) + `ChamadoId` (nullable) + `EquipamentoId` (nullable), cada uma FK de verdade, com um CHECK constraint garantindo que ao menos uma esteja preenchida. Você ganha integridade e performance, e perde só a extensibilidade a entidades futuras — que você pode resolver depois adicionando uma coluna, com custo baixo. Se, e SÓ se, a necessidade de ligar atividade a entidades arbitrárias se concretizar, adicione um par `(TipoEntidade, EntidadeId)` como via secundária, sem FK, com índice composto. O que COPIAR de verdade: (1) a separação PESSOA vs OBJETO DE NEGÓCIO — uma atividade tem um 'com quem' e um 'sobre o quê', e conflacionar os dois é erro comum; (2) `ActivityHistory` como VIEW read-only (no SQL Server, uma view indexada ou uma query dedicada) que agrega as atividades de todos os contatos da conta — é o que faz a tela de cliente ser útil e evita N queries no front; (3) o alerta do JunctionIdList: qualquer operação que apague relacionamentos em lote precisa de confirmação explícita e log.

### 15. Asset: o objeto que modela a frota instalada do cliente — e por que ele é o coração de uma revenda de equipamentos

Definição: 'Represents an item of commercial value, such as a product sold by your company OR A COMPETITOR, that a customer has purchased.' Ou seja, Asset é a BASE INSTALADA — o que existe no pátio do cliente, independente de quem vendeu.

Usage textual, que é praticamente o business case da Tracbel: 'Use this object to track products sold to customers. With asset tracking, a client application can quickly determine which products were previously sold or are currently installed at a specific account. You can also create HIERARCHIES OF UP TO 10,000 ASSETS. For example, suppose that your company wants to renew and upsell opportunities on products sold in the past. Similarly, your company can track COMPETITIVE PRODUCTS in a customer environment where products can be replaced or swapped out. Asset tracking is also useful for product support… For example, the PurchaseDate or SerialNumber can indicate whether a given product has certain maintenance requirements, INCLUDING PRODUCT RECALLS. Similarly, the UsageEndDate can indicate when the asset was removed from service or when a license or warranty expires.'

Regra de criação: 'If an application creates an Asset record, it must specify a Name and either an AccountId, ContactId, or both.'

Campos organizados por eixo:
• Identidade física: Name (obrigatório), SerialNumber, Uuid ('The unique ID for the asset', API 49.0+), ExternalIdentifier ('The ID of the matching record in an external system', API 49.0+), StockKeepingUnit, Product2Id, ProductCode/ProductDescription/ProductFamily (read-only, do produto).
• Quem: AccountId (dono), ContactId, OwnerId ('By default, the asset owner is the user who created the asset record'), AssetProvidedById → Account ('The account that provided the asset, typically a MANUFACTURER'), AssetServicedById → Account ('The account in charge of SERVICING the asset').
• Ciclo de vida: PurchaseDate, InstallDate, ManufactureDate (API 49.0+), UsageEndDate, Status (picklist customizável, default: Purchased, Shipped, Installed, Registered, Obsolete), StatusReason (Not Ready, Off, Offline, Online, Paused, Standby — API 49.0+), Price, Quantity.
• Hierarquia: ParentId → Asset, RootAssetId, AssetLevel — 'If the asset has no parent or child assets, its level is 1. Assets that belong to a hierarchy have a level of 1 for the root asset, 2 for the child assets of the root asset, 3 for their children, and so forth.' Campo calculado e atualizado automaticamente.
• Localização: LocationId → Location ('Typically, this location is the place where the asset is stored, such as a warehouse or van'), Address compound (Street/City/State/PostalCode/Country/Latitude/Longitude/GeocodeAccuracy).
• Confiabilidade / IoT — o bloco mais interessante para maquinário: ConsequenceOfFailure (picklist: Insignificant, Minor, Moderate, Major, Critical — 'the business impact associated with the asset's failure'), AveragetimeBetweenFailure ('number of hours that typically elapses before the asset is likely to fail again'), AveragetimetoRepair ('number of hours it typically takes to repair an asset after a failure'), AverageUptimePerDay, Availability (% de uptime disponível), Reliability, SumDowntime, SumUnplannedDowntime, UptimeRecordStart/UptimeRecordEnd, DigitalAssetStatus (On/Off/Warning/Error).
• Flags: IsCompetitorProduct (UI label 'Competitor Asset'), IsInternal, HasLifecycleManagement ('You can't switch an asset to a lifecycle-managed asset or the reverse').

AssetRelationship (API 41.0+) modela relação NÃO-hierárquica: 'a non-hierarchical relationship between assets due to an asset modification; for example, a REPLACEMENT, UPGRADE, or other circumstance.' AssetId é 'the unique identifier of the NEW asset, which is the asset that is taking the place of the existing asset'. Tem AssetRelationshipNumber (autonumber), AssetRole, RelationshipType, FromDate/ToDate.

Asset tem infraestrutura completa: AssetShare (sharing por registro), AssetOwnerSharingRule, AssetHistory (histórico de campos rastreados), AssetFeed, AssetChangeEvent (CDC, API 44.0+), RecordTypeId.

E ele é o ponto de ancoragem do serviço: Case.AssetId liga o chamado ao equipamento; Entitlement.AssetId liga o direito de suporte/garantia ao equipamento específico (com BusinessHoursId, SlaProcessId, StartDate/EndDate, IsPerIncident, CasesPerEntitlement, RemainingCases).

**Evidencia:** resources.docs.salesforce.com/latest/latest/en-us/sfdc/pdf/object_reference.pdf, objeto Asset completo incluindo seção Usage (linhas 39948-41000 do extrato); objeto AssetRelationship (~42564); objeto Entitlement (~116967); objeto Case, campos AssetId/EntitlementId (~65458)

**Licao para a Tracbel:** COPIAR — este é o achado de maior valor direto para a Tracbel, e a boa notícia é que `EXT_Veic` já é 80% de um Asset. Mapeamento e lacunas concretas: EXT_Veic já tem IdVeic, SeqPessoa (=AccountId), Chassi (=SerialNumber, e é a chave natural de máquina agrícola), Placa, NroMOTOR, KMAtual, AnoModelo/AnoFabric, DTAPRIMVENDA (≈PurchaseDate), SeqPlanoMAN, e a hierarquia de catálogo EXT_VeicMarca→EXT_VeicFam→EXT_VeicModelo. O que FALTA e vale importar do Asset, em ordem de retorno: (1) `Status` do ciclo de vida do equipamento (Comprado/Entregue/Instalado/Ativo/Baixado) + `DataFimDeUso` — sem isso não dá para separar frota ativa de frota histórica, e todo relatório de base instalada fica errado; (2) `ParentId`/`EquipamentoPaiId` com `NivelHierarquia` — máquina agrícola tem implemento, plataforma, cabeçote, kit de agricultura de precisão; hoje isso não é modelável e cada acessório vira máquina solta; (3) `FornecidoPorId` (John Deere como fabricante) e `AtendidoPorId` (qual filial/oficina Tracbel presta serviço) — resolve roteamento de pós-venda; (4) `EhProdutoConcorrente` — este é o campo estratégico: cadastrar a máquina Case/New Holland do cliente é o que permite gerar oportunidade de conversão de frota, e o Salesforce trata isso como caso de uso de primeira classe; (5) `AssetRelationship` para troca/upgrade — é exatamente o ciclo de trade-in de máquina usada da Tracbel, e permite rastrear 'esta colheitadeira substituiu aquela'; (6) o bloco de confiabilidade (ConsequenceOfFailure, tempo médio entre falhas, disponibilidade) casa com os dados de `EXT_OS` que a Tracbel já tem — dá para calcular MTBF por modelo e virar argumento de venda de contrato de manutenção; (7) `Entitlement` ligado ao equipamento é o modelo certo para garantia de fábrica e contrato de manutenção por máquina, com SLA — hoje isso não existe estruturado. Implemente `Equipamento` com `DonoId` próprio e compartilhamento próprio (Lookup, não Master-Detail): a máquina sobrevive à troca de dono do cliente e precisa ser visível para pós-venda mesmo quando a venda pertence a outro vendedor.

### 16. Territory2: território como mecanismo de CONCESSÃO DE ACESSO, não como rótulo

Enterprise Territory Management é uma camada de segurança paralela à hierarquia de papéis, e o detalhe que a diferencia de um simples campo 'região' está nos campos de acesso do próprio Territory2:

• AccountAccessLevel — 'Represents the default account record access levels for users that are assigned to the territory. Values are: Read Only, Read/Write, Owner.'
• OpportunityAccessLevel, CaseAccessLevel (Private / Read Only / Read/Write), ContactAccessLevel (Private / Read Only / Read/Write).

Ou seja: ao atribuir um usuário a um território, ele ganha acesso às contas daquele território COM O NÍVEL DEFINIDO NO TERRITÓRIO — independentemente de quem é o dono do registro e independentemente da hierarquia de papéis. Note que os níveis são POR TIPO DE OBJETO FILHO: pode-se dar Read/Write na conta mas Private no caso.

Estrutura: Territory2 (com ParentTerritory2Id — territórios são hierárquicos, e ForecastUserId), pertence a um Territory2Type e a um Territory2Model. O modelo é VERSIONADO por estado: Territory2Model.State = Planning, Activating, Activation Failed, Active, Archiving, Archiving Failed, Archived, Deleting, Deletion Failed. Isso permite desenhar a próxima territorialização em paralelo à vigente e ativar de uma vez.

As atribuições são dois objetos de junção:
• ObjectTerritory2Association — território ↔ registro. ObjectId é polimórfico e 'Refers To: Account, Lead' (Account desde API 30.0, Lead desde API 55.0). AssociationCause diz COMO veio: Territory2AssignmentRule (regra automática) ou Territory2Manual. Detalhe operacional: 'If you delete associations, you can query them for up to 12 hours. Keep in mind that deleted associations BYPASS THE RECYCLE BIN.'
• UserTerritory2Association — território ↔ usuário. RoleInTerritory2: Owner, Administrator, Sales Rep. IsActive. Nota: 'UserTerritory2Association doesn't support adding custom fields.'

E Opportunity.Territory2Id guarda o território do negócio.

Visibilidade dos próprios modelos: 'If a territory model is in Active state, ANY standard or partner user can view that model, including its territories and assignment rules… Users cannot view territory models in other states (such as Planning or Archived).'

**Evidencia:** object_reference.pdf, objetos Territory2 (linha 305824 do extrato), Territory2Model (306114, campo State), ObjectTerritory2Association (201995) e UserTerritory2Association (323854)

**Licao para a Tracbel:** COPIAR o conceito, ADAPTAR à realidade da carteira. Este é o achado que mais fala com um problema já documentado na memória do projeto Tracbel: 'Carteira e CEN' (`IVS_Carteira.SeqVendedor` → `IV_VENDEDOR`, vínculo por `IV_VENDEDOREMPR`), 'roteamento de aprovação vai para o LÍDER do CEN no momento da geração' e 'gerente não vê carteira de pedidos = hierarquia presa no usuário antigo'. Esses são exatamente os sintomas de território implementado como campo solto em vez de mecanismo de acesso. RECOMENDAÇÃO: modele `Carteira` (o CEN) como uma entidade de primeira classe com (a) hierarquia própria via caminho materializado; (b) `NivelAcessoPessoa` e `NivelAcessoOportunidade` no próprio registro da carteira — copie literalmente essa ideia, é o que evita ter que criar regra de compartilhamento nova a cada carteira; (c) `AtribuicaoCarteiraUsuario(CarteiraId, UsuarioId, Papel, Ativo, VigenciaInicio, VigenciaFim)` com Papel = Dono/Administrador/Vendedor; (d) `AtribuicaoCarteiraPessoa(CarteiraId, PessoaId, Origem: Regra|Manual)` — o Vórtice já faz isso em `IVS_Pes` com PK composta (SeqPessoa+SeqDepto+SeqCarteira), permitindo um cliente em várias carteiras. Mantenha. E COPIE especialmente duas coisas: (1) o VERSIONAMENTO por estado (Planejamento/Ativo/Arquivado) — a Tracbel redesenha carteira todo ano fiscal (a memória registra 'fluxo FY25'), e sem versionamento a virada é um evento de risco com downtime; com versionamento é um toggle. (2) A VIGÊNCIA nas atribuições — o problema 'agenda antiga não se reatribui' e 'hierarquia presa no usuário antigo' se resolve quando a atribuição tem data e o sistema sabe qual era a carteira vigente NA DATA do registro. Guarde no registro (oportunidade/pedido) o `CarteiraId` vigente no momento da criação, como o Salesforce faz com Opportunity.Territory2Id.

### 17. Campaign e CampaignMember: a regra 'Lead XOR Contact' e o Status que dirige o HasResponded

CampaignMember é a junção Campanha × Membro. Definição: 'The CampaignMember object represents the relationship between a campaign and either a LEAD or a CONTACT. If the Accounts as Campaign Members setting is enabled in an org, CampaignMember can also represent the relationship between a campaign and an account.'

A regra dura, na seção Usage: 'Each record has a unique ID, and must contain either a ContactId or a LeadId, but CAN'T CONTAIN BOTH. Any attempt to create a single record with both results in a SUCCESSFUL INSERT but only the ContactId is inserted. However, you can create two separate records on a Campaign — one for the Lead and one for the Contact.' E a exceção documentada logo abaixo: 'Note: If you want to track lead-based campaign members you convert to contacts, provide BOTH a ContactId and a LeadId. Otherwise, only use one ID type.' — as duas frases se contradizem na aparência; a leitura correta é que o par completo é o mecanismo de rastreio de conversão.

Status e HasResponded: 'Controls the HasResponded flag on this object. You can't directly set the HasResponded flag, as it's read-only. You can set it indirectly by setting this field… Each predefined value implies a HasResponded flag value… For each Status field value, they can also select which values to count as "Responded"… The limit is 40 characters. When you create or update campaign members, use the TEXT VALUE for Status instead of the ID from the CampaignMemberStatus object.'
Comportamento de validação: se o Status enviado é válido para aquela campanha, grava e ajusta HasResponded; se é inválido, a API atribui o status DEFAULT da campanha (não rejeita!); e se a campanha não tem default, grava o valor enviado com HasResponded=false.

Outra pegadinha: campos padrão do lead/contato (Email, Phone, City…) aparecem no CampaignMember mas 'you can't query them directly' — é preciso fazer subquery: `SELECT Id, (SELECT Phone FROM Lead) FROM CampaignMember`.

RecordTypeId: 'You can't create or update the RecordTypeId field on the CampaignMember records. Set the CampaignMember record type using the CampaignMemberRecordTypeId field on Campaign.'

Campaign tem hierarquia (ParentId) com roll-up automático da árvore: campos como Total Actual Cost in Hierarchy, Value Opportunities in Hierarchy, Converted Leads in Hierarchy, Total Contacts in Hierarchy.

**Evidencia:** object_reference.pdf, objeto CampaignMember incluindo seção Usage (linhas 61056-61525 do extrato); objeto Campaign, campos *InHierarchy (~60098-60163)

**Licao para a Tracbel:** SIMPLIFICAR. O 'Lead XOR Contact' só existe porque o Salesforce tem duas tabelas de pessoa; se a Tracbel adotar `Pessoa` única (recomendação anterior), o problema evapora: `MembroCampanha(CampanhaId, PessoaId, Status, RespondeuEm)` com índice único em (CampanhaId, PessoaId). Mais simples e mais correto. COPIAR três coisas: (1) `Status` do membro configurável POR CAMPANHA (tabela `StatusCampanha(CampanhaId, Valor, EhResposta, EhPadrao, Ordem)`), com `Respondeu` DERIVADO do status — nunca um checkbox editável à parte. Isso mata a inconsistência 'marcado como respondido mas status = não contatado'. (2) A hierarquia de campanha com roll-up — a Tracbel roda campanhas guarda-chuva (safra, feira, linha de produto) com sub-ações, e sem hierarquia o ROI consolidado vira planilha manual. (3) O rastreio de atribuição pós-conversão: quando um prospect vira cliente, o vínculo com a campanha de origem tem que SOBREVIVER — é o único jeito de responder 'quanto de faturamento veio da campanha X'. Como a Tracbel terá Pessoa única, isso é automático (o vínculo nunca quebra), o que é uma vantagem estrutural sobre o Salesforce. NÃO copiar: o comportamento de substituir silenciosamente um status inválido pelo default — valide e rejeite com erro claro. E registre a lição geral que aparece 3 vezes neste relatório: o Salesforce tem o hábito de 'aceitar e ignorar' em vez de rejeitar, e isso gera chamados insolúveis; o CRM Tracbel deve falhar alto e explícito.

### 18. Contract, Order e Entitlement: o pós-venda e os campos calculados de vigência

CONTRACT — 'Represents a contract (a business agreement) associated with an Account.' AccountId é obrigatório. O campo mais instrutivo é EndDate: 'Read-only. Calculated end date of the contract. This value is calculated by adding the ContractTerm to the StartDate. If the Auto-calculate Contract End Date setting is DISABLED, the contract end date is editable.' — ou seja, o campo é derivado por padrão mas o comportamento é uma CONFIGURAÇÃO DA ORG, não uma decisão de código. Outros campos: ContractNumber, Status/StatusCode, ContractTerm (meses), OwnerExpirationNotice, CustomerSignedId/CustomerSignedDate/CustomerSignedTitle, CompanySignedId/CompanySignedDate, ActivatedById/ActivatedDate, SpecialTerms, IsPricingContract, HasContractCotermination.

ORDER — 'Represents an order associated with a contract or an account.' Máquina de estados explícita: 'When a client application creates an order, the Status Code must be Draft… The application can then activate an order by updating it and setting the value in its Status field to an Activated state. However, THE STATUS FIELD IS THE ONLY FIELD YOU CAN UPDATE when activating the order. After an order is activated, your client application can change the Status back to the Draft state — but only if the order doesn't have any child reduction order products. Your client application can DELETE orders when the Status is Draft but NOT when its Status is Activated.' AccountId é obrigatório e 'Only updated when the order's StatusCode value is Draft'. OriginalOrderId permite reduction/change orders. Order tem ContractId, OpportunityId, QuoteId e Pricebook2Id — é o nó que amarra tudo.

ENTITLEMENT — 'Represents the customer support an account or contact is eligible to receive… Entitlements may be based on an ASSET, PRODUCT, or SERVICE CONTRACT.' Campos: AccountId, AssetId ('Required. ID of the Asset associated with the entitlement'), AssetWarrantyID, BusinessHoursId ('Required'), ContractLineItemId ('Required'), ServiceContractId, SlaProcessId, Type, StartDate, EndDate ('The last day the entitlement is in effect'), IsPerIncident ('Indicates whether the entitlement is limited to supporting a specific number of cases'), CasesPerEntitlement ('The total number of cases the entitlement supports… only available if IsPerIncident is true'), RemainingCases, LocationID.

O encadeamento completo do pós-venda: Account → Contract → ContractLineItem → Entitlement (com BusinessHours e SlaProcess) → Case.EntitlementId, e em paralelo Asset ← Entitlement.AssetId e Asset ← Case.AssetId.

**Evidencia:** object_reference.pdf, objeto Contract (linha 85441 do extrato, campo EndDate), objeto Order incluindo seção Usage (207712 e ~210900), objeto Entitlement (116967)

**Licao para a Tracbel:** ADAPTAR seletivamente. O que COPIAR: (1) O padrão de MÁQUINA DE ESTADOS EXPLÍCITA do Order — 'só o campo Status pode mudar na ativação', 'não pode deletar depois de ativado', 'só volta para rascunho se não tiver filhos'. Implemente isso como um agregado de domínio com métodos (`Ativar()`, `Cancelar()`, `VoltarParaRascunho()`) que validam invariantes, e NUNCA como um enum livre que qualquer serviço seta. Sem isso, um CRM feito à mão vira um pântano de estados inválidos em 12 meses. Amarre com um índice/constraint no banco. (2) `Entitlement` ligado a `Equipamento` é o modelo certo para GARANTIA DE FÁBRICA e CONTRATO DE MANUTENÇÃO por máquina: `DireitoAtendimento(PessoaId, EquipamentoId, ContratoId, Tipo, Inicio, Fim, HorarioComercialId, ProcessoSlaId, LimitadoPorChamados, ChamadosContratados, ChamadosRestantes)`. A Tracbel vende contratos de manutenção John Deere e hoje não tem onde isso viva estruturado — o resultado é oficina descobrindo garantia por telefone. (3) A regra de vigência calculada (`Fim = Inicio + PrazoMeses`) COM escape configurável — o Salesforce acertou em deixar a auto-calculação ser uma configuração. O que SIMPLIFICAR: não implemente reduction orders / change orders (OriginalOrderId) — é complexidade de Revenue Cloud que 130 usuários não vão usar; se surgir, um `PedidoOrigemId` resolve. O que DESCARTAR na v1: ServiceContract separado de Contract (dois objetos para a mesma ideia).

### 19. O padrão 'read-only view object': ActivityHistory, OpenActivity e os objetos que só suportam describeSObjects()

Um padrão de modelagem pouco comentado do Salesforce: existem objetos no schema que NÃO são tabelas — são projeções read-only que só existem para alimentar related lists.

ActivityHistory: 'This READ-ONLY object is displayed in a related list of closed activities — past events and closed tasks — related to an object. It includes activities for ALL CONTACTS RELATED TO THE OBJECT.' Supported Calls: apenas describeSObjects() (e delete() opcional a partir da API 42.0, para Field History Archive). O AccountId é derivado por regra em cascata: conta do WhatId → senão conta do WhoId → senão null.

OpenActivity é o gêmeo para atividades abertas/futuras.

Outros objetos read-only no mesmo espírito: LeadStatus ('describeSObjects(), query(), retrieve()' apenas — é o catálogo de valores de picklist de status com o flag IsConverted por valor), OpportunityStage (o mapeamento estágio→categoria de forecast), Campaign (curiosamente, Campaign suporta apenas describeLayout(), describeSObjects(), getDeleted(), getUpdated(), query(), retrieve(), search() — sem create/update/delete via essa listagem).

A lição de design: o Salesforce expõe, no MESMO namespace do modelo de dados, tanto as tabelas quanto as consultas materializadas que a UI precisa — com as capacidades declaradas explicitamente por objeto ('Supported Calls'). O cliente sabe, por introspecção, o que pode fazer com cada coisa.

**Evidencia:** object_reference.pdf, objetos ActivityHistory (linha 23776 do extrato), OpenActivity (202736), LeadStatus (175727) — em cada caso a seção 'Supported Calls'

**Licao para a Tracbel:** COPIAR — é um padrão barato e de alto retorno para um time pequeno. Duas aplicações: (1) Exponha na API do CRM Tracbel recursos de LEITURA dedicados à tela, separados das entidades de escrita: `/pessoas/{id}/historico-consolidado` que agrega interações (IV_Historico), agenda realizada (IV_Agenda), OS (EXT_OS) e NF (EXT_NFS) numa timeline única e ordenada. No EF Core, mapeie como keyless entity (`.HasNoKey().ToView(...)`) sobre uma view SQL. Isso evita o antipadrão de o React fazer 5 chamadas e montar a timeline no cliente — que é lento e vira inconsistente. Com 2,44M linhas em IV_Historico, a view precisa ser paginada e indexada por (SeqPessoa, DtaRealizacao DESC). (2) COPIE a ideia de declarar CAPACIDADES por recurso: exponha um endpoint de metadados (`/metadados/entidades`) que diz, por entidade e por perfil do usuário, o que é criável/editável/deletável e quais campos são read-only. O React consome isso e desabilita controles automaticamente, em vez de ter regra de UI duplicada e divergente do backend — este é diretamente o requisito 'código que qualquer dev do time saiba estender': adicionar um campo passa a ser metadado, não código de tela. É o mecanismo que faz o Salesforce escalar para milhares de orgs com uma UI só, e é reproduzível em uma sprint.

### 20. Notas e arquivos: por que o Salesforce trocou Attachment/Note (ParentId 1:1) por ContentDocumentLink (N:N polimórfico)

O modelo legado: o objeto Note tem ParentId — um único registro pai — Title, Body ('limited to 32 KB'), IsPrivate e OwnerId. Um arquivo ou nota pertencia a exatamente UM registro. Se o mesmo documento interessava à conta e à oportunidade, você duplicava o arquivo.

O modelo atual (Salesforce Files) separa em três: ContentDocument (o documento lógico), ContentVersion (cada versão do conteúdo) e ContentDocumentLink (a JUNÇÃO). ContentDocumentLink: 'Represents the link between a Salesforce CRM Content document, Salesforce file, or ContentNote and WHERE IT'S SHARED. A file can be shared with other users, groups, records, and Salesforce CRM Content libraries.'

Campos: ContentDocumentId (Create/Filter/Group/Sort — imutável) e LinkedEntityId — 'ID of the linked object. Can include Chatter users, groups, records (ANY that support Chatter feed tracking including custom objects), and Salesforce CRM Content libraries… This is a POLYMORPHIC relationship field.' Mais ShareType e Visibility.

Regras de acesso que revelam o modelo mental: 'In API versions 21.0 and later, users with explicit Viewer access (the file has been directly shared with the user) to a file can DELETE ContentDocumentLink objects between the file and other users who have Viewer access.' E, em API 59.0+, é preciso a permissão 'Query All Files' para consultar sem filtro em id/LinkedEntityId/documentID — proteção contra varredura.

O ponto arquitetural: o Salesforce percebeu que ARQUIVO não é filho de registro, é um recurso independente COMPARTILHADO com N registros, e que a permissão de ver o arquivo é uma dimensão separada da permissão de ver o registro.

**Evidencia:** object_reference.pdf, objeto Note (linha 200291 do extrato, campos ParentId/Body) e objeto ContentDocumentLink com Special Access Rules (81530)

**Licao para a Tracbel:** COPIAR o modelo novo, e é uma decisão de arquitetura a tomar ANTES de escrever a primeira tela. O Vórtice tem `DMN_` (9 tabelas de gestão documental, `IV_ProcDocto` + `DMN_Doc`) e a memória do projeto já registra um incidente causado exatamente pelo modelo frágil: '5.302 documentos órfãos — vínculo em IV_ProcDocto sem DMN_Doc, quebrando o sync do Mobile com erro de FOREIGN KEY'. Isso é o sintoma clássico de junção sem integridade referencial. RECOMENDAÇÃO: três tabelas — `Documento` (metadados: nome, tipo MIME, tamanho, hash, chave no storage), `VersaoDocumento` (versionamento, que o Vórtice não tem), e `VinculoDocumento(DocumentoId, TipoEntidade, EntidadeId, TipoCompartilhamento)` com FK REAL para Documento e `ON DELETE CASCADE` — o que teria impedido os 5.302 órfãos por construção. Guarde o binário fora do banco (Azure Blob / S3 / file share) e só a chave no SQL Server; a Tracbel já opera FTP para o Doc Manager, então há caminho de migração. COPIE também a separação de permissão: ver o registro ≠ ver o anexo — contrato assinado e proposta com margem não devem ser visíveis a todo mundo que vê a oportunidade. E copie a proteção contra varredura: nunca exponha `GET /documentos` sem filtro obrigatório.

### 21. Padrões transversais do modelo Salesforce que valem como regra de projeto (e os antipadrões a evitar)

Consolidando padrões que aparecem repetidamente na Object Reference e que são decisões de design, não acidentes:

PADRÕES BONS:
• CAMPOS DERIVADOS SÃO READ-ONLY POR CONSTRUÇÃO. IsClosed/IsWon (de StageName), HasResponded (de Status), ExpectedRevenue (Amount×Probability), Contract.EndDate (StartDate+Term), Opportunity.ContactId (do papel primário), Asset.AssetLevel (da hierarquia), PricebookEntry.Name/ProductCode (do produto), Case.IsClosed (do Status), OpportunityLineItem.Name. O padrão é sempre: o campo governante é editável, os derivados não têm a propriedade 'Update'.
• PROPRIEDADES DE CAMPO DECLARATIVAS E INTROSPECTÁVEIS. Cada campo declara Create, Update, Filter, Group, Sort, Nillable, Defaulted on create, Restricted picklist, idLookup — e o cliente descobre isso por describeSObjects(). A ausência de 'Update' é o que torna o campo imutável; a ausência de 'Nillable' é o que o torna obrigatório.
• SOFT DELETE UNIVERSAL. IsDeleted em todo objeto, com undelete().
• MOTIVO TIPADO NO COMPARTILHAMENTO (RowCause) e NA ATRIBUIÇÃO (AssociationCause: Territory2AssignmentRule vs Territory2Manual) — sempre se sabe POR QUE aquele vínculo existe e se pode recalcular seletivamente.
• EVENTOS DE MUDANÇA POR OBJETO (AccountChangeEvent, AssetChangeEvent API 44.0, CampaignMemberChangeEvent API 46.0…) e HISTÓRICO DE CAMPO (AccountHistory, AssetHistory) como infraestrutura padrão, não add-on.
• VERSIONAMENTO DE CONFIGURAÇÃO (Territory2Model.State: Planning→Active→Archived).

ANTIPADRÕES a NÃO copiar:
• ACEITAR E IGNORAR EM SILÊNCIO. Opportunity.Amount com itens: 'Any attempt to update this field… will be ignored. The update call will not be rejected.' CampaignMember com Status inválido: grava o default em vez de rejeitar. CampaignMember com Lead+Contact: 'results in a successful insert but only the ContactId is inserted.' Isso gera a pior classe de bug: 'salvei e não gravou, e ninguém avisou'.
• UI E API COM COMPORTAMENTOS DIFERENTES. Transferência de titularidade cascateia na UI e não na API.
• CONFIGURAÇÃO COMO CONTROLE DE ACESSO. Os dois avisos 'Don't use record types / business processes as an access control mechanism'.
• REGRA DE NEGÓCIO IMPLÍCITA EM CAMPO NULO. Lead.Company nulo = vira person account.
• DEPENDER DE DETALHE DE IMPLEMENTAÇÃO. A própria doc pede para não depender da existência dos registros de share.

**Evidencia:** object_reference.pdf: campos Opportunity.Amount/IsClosed/IsWon/ExpectedRevenue/ContactId, CampaignMember.Status/HasResponded e seção Usage, Contract.EndDate, Asset.AssetLevel, AccountShare.RowCause, ObjectTerritory2Association.AssociationCause, Territory2Model.State, RecordType bloco 'Important', cap.1 'System Fields' e 'Frequently Occurring Fields' > OwnerId; api_meta.pdf, tipo BusinessProcess bloco 'Important'

**Licao para a Tracbel:** Transforme isto em um documento de convenções do repositório (ADR ou CLAUDE.md) — é o que atende diretamente ao requisito 'código que qualquer dev do time saiba estender' com 2-3 pessoas. Regras a adotar: (1) Todo campo derivado é `private set` ou sem setter, calculado no domínio, persistido para indexação — nunca aceito do cliente. Um teste automatizado que tenta setar cada derivado via API e espera 4xx. (2) Publique metadados de campo pela API (`criavel`, `editavel`, `filtravel`, `obrigatorio`) e faça o React consumir — uma fonte de verdade, não duas. (3) Soft delete + auditoria na classe base, via interceptor, sem exceção. (4) Motivo tipado em toda concessão de acesso e toda atribuição automática. (5) Change tracking desde o dia 1: uma `TrilhaAuditoria(Entidade, RegistroId, Campo, ValorAntes, ValorDepois, UsuarioId, Em)` alimentada pelo `ChangeTracker` do EF Core — o Vórtice tem `GE_LOG_*`/`GE_LgTb` e a memória mostra que a Tracbel VIVE de arqueologia de log; faça direito desde o início. (6) Versione configuração (fluxos, carteiras, mapeamentos) com vigência, nunca por UPDATE destrutivo. E as PROIBIÇÕES: nunca aceitar-e-ignorar (sempre 409/422 com erro de domínio nomeado); nunca comportamento diferente entre UI e API (a UI deve consumir a MESMA API — regra que também resolve o requisito 'APIs para integração', porque a API fica exercitada em produção por construção); nunca usar tipo de registro/configuração de tela como segurança; nunca inferir regra de negócio de campo nulo.

## Lacunas declaradas

- MAPEAMENTO DE CONVERSÃO CAMPO-A-CAMPO: não consegui abrir a tabela oficial completa de 'Lead Conversion Field Mapping' (help.salesforce.com/s/articleView?id=sales.lead_conversion_mapping.htm). O site help.salesforce.com é uma SPA que devolve 'CSS Error' ao WebFetch e 403 ao curl. Confirmei o MECANISMO (mapa configurável, campos padrão mapeados automaticamente, customizados exigem mapeamento explícito, não mapeado = perdido) por developer.salesforce.com/docs e fontes secundárias, mas NÃO a lista exata campo-por-campo de quais standard fields do Lead vão para Account vs Contact vs Opportunity.
- PÁGINA OFICIAL 'CONSIDERATIONS FOR CONVERTING LEADS' (sales.leads_notes.htm) e 'WHAT HAPPENS WHEN I CONVERT LEADS' (sales.faq_leads_what_happens_when.htm): inacessíveis pelo mesmo motivo (SPA + bloqueio de bot). O comportamento de atividades, campaign members, notas e anexos na conversão está confirmado por múltiplas fontes secundárias convergentes e pelo doc de Change Data Capture, mas NÃO pela página canônica do help. Trate os detalhes de reancoragem de Task.WhoId/WhatId como 'alta confiança, não verificado na fonte primária'.
- LIMITE DE 2 MASTER-DETAIL E 40 RELACIONAMENTOS POR OBJETO: o número 40 está confirmado INDIRETAMENTE no PDF oficial de limites ('A custom object allows up to 40 relationships'), e existe um artigo de help oficial cujo TÍTULO confirma o conceito ('Increase the maximum relationships (master-detail plus lookup) allowed per object', id=000386135) — mas não consegui abrir o corpo do artigo nem a página 'Object Relationships Overview' (platform.overview_of_custom_object_relationships.htm) para ler o '2 master-detail' na fonte primária. O valor 2 vem de fontes secundárias consistentes.
- LIMITES DE CAMPOS CUSTOMIZADOS POR EDIÇÃO (500 Enterprise / 800 Unlimited-Performance) e o teto de ~10.000 registros filhos por master-detail: apurados apenas em fontes secundárias e no título de artigos de help. A página oficial 'Custom Fields Allowed Per Object' (platform.custom_field_allocations.htm) não abriu.
- NÚMERO MÁXIMO DE RECORD TYPES POR OBJETO: NÃO CONFIRMADO. A busca retornou '200 record types por instância' apenas como recomendação de mercado em blog, não como limite de plataforma. Não use esse número.
- PÁGINAS DE ARQUITETURA architect.salesforce.com (notadamente 'Platform Sharing Architecture'): retornaram HTTP 403 tanto para WebFetch quanto para curl. Por isso NÃO consegui apurar em fonte primária: os detalhes de sharing recalculation, group membership tables, ownership skew (o limite prático de ~10.000 registros de um mesmo objeto sob um único owner) e o desenho interno das tabelas de grupo. O modelo de implicit sharing foi reconstruído a partir dos valores de RowCause em AccountShare, que é fonte primária, mas não da documentação narrativa de arquitetura.
- DIAGRAMAS ERD DA DATA MODEL GALLERY: as páginas de developer.salesforce.com/docs/platform/data-models/guide/ (sales-cloud-overview.html, product-price-book.html) abriram e confirmaram a LISTA de entidades de cada diagrama, mas o conteúdo relacional está em IMAGENS que o WebFetch não lê. As cardinalidades e nomes de FK deste relatório vieram do PDF da Object Reference (fonte primária, campo a campo), não dos diagramas.
- NÚMERO EXATO DE ROLL-UP SUMMARY FIELDS PERMITIDOS POR OBJETO (comumente citado como 25, elevável a 40): não confirmei em fonte oficial. O tipo de campo existe e está confirmado no Metadata API (FieldType 'Summary'), e a regra de que roll-up exige master-detail está confirmada; o teto numérico não.
- COMPORTAMENTO DE CASCATA NA EXCLUSÃO DE OPPORTUNITY → OPPORTUNITYLINEITEM: inferi que é cascata a partir de OpportunityId ser obrigatório, não-nulável e não-atualizável, e do fato de OpportunityLineItem não existir sem Opportunity. Não localizei a frase explícita 'deleting an Opportunity deletes its line items' na Object Reference. Trate como muito provável, não como citação.
- PERSON ACCOUNT — DOC CANÔNICA: a página 'Person Account Record Types' do SOAP API Developer Guide (para a qual a Object Reference remete em SEE ALSO) não foi aberta. Os campos Person* e o gatilho de Company nulo estão confirmados na Object Reference (fonte primária); as limitações (não entra em Account Hierarchy, person accounts não se relacionam diretamente entre si) vêm de fontes secundárias.
- LADO TRACBEL — O MAPEAMENTO É PROPOSTA, NÃO VALIDAÇÃO. O mapeamento Vórtice→Salesforce→CRM Tracbel foi construído a partir do SCHEMA_MAP.md do repositório local (c:/projetos/vortice-crm-agent/SCHEMA_MAP.md, gerado por introspecção do banco em 2026-06-03) e da memória do projeto. NÃO executei nenhuma query no banco do Vórtice nesta sessão para validar cardinalidades, domínios de Status ou volumes atuais. Antes de decidir arquitetura, valide contra o banco: domínios reais de IV_Processo.Fase/Status, se EXT_Veic tem de fato 1:N com GE_Pessoa, e o estado atual das 672 FKs em schema/fks.csv.
- NÃO COBERTO POR FALTA DE ESCOPO: Opportunity Splits, OpportunityLineItemSchedule (schedules de quantidade/receita), Forecasting (ForecastingItem/ForecastingSubmission), Customizable Campaign Influence, Sales Engagement/Cadences, Einstein Scoring, e o modelo de Revenue Cloud / Salesforce CPQ (Asset lifecycle management, AssetStatePeriod, PriceProtection). Todos aparecem no schema e alguns campos foram citados de passagem, mas não foram investigados.
