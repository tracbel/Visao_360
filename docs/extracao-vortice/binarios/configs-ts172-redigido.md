# Configuracoes do Vortice - ECS-ST-TSPLUS (terminal server, 10.150.5.xx) (redigido)

> Coletado em 2026-09-02 23:19 a partir de `\\10.150.5.xx\c$\Vortice` (somente leitura).
> Senhas, tokens e chaves aparecem como `***REDIGIDO***`; o ultimo octeto dos IPs privados vira `.xx`.

## `TDev\SQL.ini`

_659 bytes ? modificado em 2026-08-23 18:36_

```
;---  SQL.INI Modelo   09/2021

[win32client]
clientname=COLWCRMBD
clientruntimedir="C:\Vortice\TDBin"  


[win32client.dll]
; order of win32client dll's is important when connecting to multiple databases. 
; sqlws32 should always be the last and sqlodb32 be 2nd last. 
;comdll=sqlora32 
; comdll=sqlsyb32 
comdll=sqlodb32
; comdll=sqlifx32 

[odbcrtr]
remotedbname=CRM,DSN=CRM
odbctrace=off
odbctracefile=sql.log
longbuffer=1600000
EnableMultipleConnections=off
; FetchRow=500
; BRSDriver=msodbcsql

;[infogtwy]
;remotedbname=IFX,DATABASE"sysmaster",-h<host_name> -s<service_name> -v<inofrmix_server>
;buffrow=0
;defconnect=off
```

## `TDev\SQLx.ini`

_823 bytes ? modificado em 2021-09-09 10:19_

```
;---  SQL.INI Modelo   09/2021

[win32client]
clientname=granada
clientruntimedir="C:\Vortice\TDBin"  

[win32client.dll]
; order of win32client dll's is important when connecting to multiple databases. 
; sqlws32 should always be the last and sqlodb32 be 2nd last. 
comdll=sqlora32 
; comdll=sqlsyb32 
comdll=sqlodb32 
; comdll=sqlifx32 

[oragtwy]
remotedbname=ora12,@ora12db
longbuffer=1024000
fetchrow=20
substitute=SYSSQL.,
remotedbname=GRANADA,@GRANADA
remotedbname=villa,@villarica


[odbcrtr]
remotedbname=ODBCDB,DSN=DSN_Name
odbctrace=off
odbctracefile=sql.log
longbuffer=1600000
EnableMultipleConnections=off
; FetchRow=500
; BRSDriver=msodbcsql

;[infogtwy]
;remotedbname=IFX,DATABASE"sysmaster",-h<host_name> -s<service_name> -v<inofrmix_server>
;buffrow=0
;defconnect=off
```

## `TDev\SQL_Modelo.ini`

_823 bytes ? modificado em 2021-09-09 10:19_

```
;---  SQL.INI Modelo   09/2021

[win32client]
clientname=granada
clientruntimedir="C:\Vortice\TDBin"  

[win32client.dll]
; order of win32client dll's is important when connecting to multiple databases. 
; sqlws32 should always be the last and sqlodb32 be 2nd last. 
comdll=sqlora32 
; comdll=sqlsyb32 
comdll=sqlodb32 
; comdll=sqlifx32 

[oragtwy]
remotedbname=ora12,@ora12db
longbuffer=1024000
fetchrow=20
substitute=SYSSQL.,
remotedbname=GRANADA,@GRANADA
remotedbname=villa,@villarica


[odbcrtr]
remotedbname=ODBCDB,DSN=DSN_Name
odbctrace=off
odbctracefile=sql.log
longbuffer=1600000
EnableMultipleConnections=off
; FetchRow=500
; BRSDriver=msodbcsql

;[infogtwy]
;remotedbname=IFX,DATABASE"sysmaster",-h<host_name> -s<service_name> -v<inofrmix_server>
;buffrow=0
;defconnect=off
```

## `TDev\Vortice_Config.INI`

_1587 bytes ? modificado em 2026-08-23 16:57_

```
;  Arquivo de configuração Modelo   09/2021
;============================================================
;-----------------------------------------------------
;---- Sistemas
;-----------------------------------------------------
;[GLOBAL] 
;defDBClone=VORTICE  
;TraceTime=0
;-----------------------------------------------------------------
;                             Conexoes
;-----------------------------------------------------------------

;[VORTICE_ORA]
;defdbname=BANCO
;defDBUser=VORTICE
;defDBPw=***REDIGIDO***
;DefDBOwner=VORTICE
;DefDbNroConexao=1
;InvalidObjectDisable=1



[GLOBAL]
defdbname=CRM
defDBUser=crm
defDBPw=***REDIGIDO***
DefDBOwner=dbo
DefDBVersion=100
DefDbNroConexao= 1
DefUSerConnect= VRTCSERVER/***REDIGIDO***
DefEmprConnect= 1
;TraceTime=1

;-----------------------------------------------------
;---- Servidor de Processo
;-----------------------------------------------------

[ServProcesso]
Servidor=1
defdbname=CRM					
defDBUser=CRM					
defDBPw=***REDIGIDO***
DefDBOwner=dbo	
DefDBVersion=100
DefDbNroConexao=1
;VerificaCNPJImp=N
DefUSerConnect= VRTCSERVER/***REDIGIDO***
DefEmprConnect=1
;Monitor=1
;DebugMode=180
;TraceTime=100
LoopSyncQtde=100

[RunImportIMP] 
defuserconnect=VRTCSERVER/***REDIGIDO***
DefEmprConnect=1

[RunMovOut]
defuserconnect=VRTCSERVER/***REDIGIDO***
DefEmprConnect=1

[RunGEPImport]
defuserconnect=VRTCSERVER/***REDIGIDO***
DefEmprConnect=1
```

## `TDev\Vortice_Config_Modelo.INI`

_860 bytes ? modificado em 2021-09-09 10:18_

```
;  Arquivo de configuração Modelo   09/2021
;============================================================
;-----------------------------------------------------
;---- Sistemas
;-----------------------------------------------------
[GLOBAL] 
defDBClone=VORTICE  
;TraceTime=0
;-----------------------------------------------------------------
;                             Conexoes
;-----------------------------------------------------------------

[VORTICE_ORA]
defdbname=BANCO
defDBUser=VORTICE
defDBPw=***REDIGIDO***
DefDBOwner=VORTICE
DefDbNroConexao=1
;InvalidObjectDisable=1

 
;%%%%%%%%%%%%%%%%%%%%%%% Vortice %%%%%%%%%%%%%
[ServProcesso]
;DefDbNroConexao=1   
;defuserconnect=SERVPROC/***SERIAL-REDIGIDO***
defemprconnect=1
servidor=1
;Monitor=0
Teste=S
TraceTime=0
;TraceTimeMax=10
;LoopSyncQtde=400
```

## `TDev\vaCONFIG.ini`

_2409 bytes ? modificado em 2022-02-24 12:11_

```
[DBMS]

;-----------------------------------------------------
;---- Sistemas
;-----------------------------------------------------


;[InterVISION]

;defdbclone=CRM

;-----------------------------------------------------
;---- Conexões SQL Server
;-----------------------------------------------------

;[colorado]
;db=SQL SERVER

;defdbname=CRM					
;defDBUser=CRM					
;defDBPw=***REDIGIDO***
;DefDBOwner=dbo	
;DefDBVersion=100
;DefDbNroConexao=1


[GLOBAL]
defdbname= CRM, treino
defDBUser=	CRM, CRM
defDBPw=***REDIGIDO***
DefDBOwner=	dbo, dbo
DefDBVersion= 100, 100
DefDbNroConexao= 1
DefUSerConnect= VTCCONS/***REDIGIDO***
DefEmprConnect= 1


[TREINO]
defdbname= treino
defDBUser= CRM
defDBPw=***REDIGIDO***
DefDBOwner=	dbo
DefDBVersion= 100
DefDbNroConexao=1


[SISTRE]
defdbname=sistre
defDBUser=informix
defDBPw=***REDIGIDO***
;***SERIAL-REDIGIDO***
DefDBOwner=informix
DefDbNroConexao=1
DefEmprConnect=1

[SISDIAXXX]
ALTERADO EM 30/07/2021 - VORTICE
defdbname=sisdia
defDBUser=informix
defDBPw=***REDIGIDO***
DefDBOwner=informix
DefDbNroConexao=1
DefEmprConnect=1


[TBL_OUT]
defdbname=CRM					
defDBUser=CRM					
defDBPw=***REDIGIDO***
DefDBOwner=dbo	
DefDBVersion=100
DefDbNroConexao=1




;-----------------------------------------------------
;---- Servidor de Processo
;-----------------------------------------------------

[ServProcesso]
Servidor=1
defdbname=CRM					
defDBUser=CRM					
defDBPw=***REDIGIDO***
DefDBOwner=dbo	
DefDBVersion=100
DefDbNroConexao=1
;VerificaCNPJImp=N
Teste=S
DefUSerConnect=MASTER/***REDIGIDO***
DefEmprConnect=1
Monitor=1


[ServProcesso_395]
Servidor=1
defdbname=TREINO					
defDBUser=CRM					
defDBPw=***REDIGIDO***
DefDBOwner=dbo	
DefDBVersion=100
DefDbNroConexao=1
;VerificaCNPJImp=N
Teste=S
;DefUSerConnect=MASTER/***SERIAL-REDIGIDO***
DefEmprConnect=1
Monitor=1

[V6]
defdbVersion = SqlServer	
defdbServer = 192.1.7.xx	
defdbName = CRM	
defDBUser = CRM	
defDBPw=***REDIGIDO***

;----------------------------------------------------
;DOCCATCHER
;----------------------------------------------------

[DOCCATCHER]
defdbname=CRM
defdbuser=CRM
defDBPw=***REDIGIDO***
defdbowner=dbo
Teste=S
DefUSerConnect=VTCSUPER/***REDIGIDO***
DefEmprConnect=1
START=2
STOP=22:00
```

## `TDev\utl\vaReplace.ini`

_1946 bytes ? modificado em 2023-06-05 15:01_

```
!=================================================
!  Arquivo de configuraçao do Vortico Replace 3.3
!=================================================

!----------------- Instruções --------------------------------------------------------------
!  [Bloco0]              ( de BLOCO0 até BLOCO20 ) Cada bloco, um conjunto de cópias
!  Origem=c:\pasta        Pasta de origem (sem a barra)
!  Destino=c:\temp\t2    Pasta Destino   (sem a barra)
!        Pode se usar . para pasta local, ex: Destino=.\arquivo\temp
!        Pode se usar [Client] para definir o local onde estão os clients Vortice.
!  Extensao=*.qrp        Extensao ou tipos de arquivo Ex: *ora*.sql
!        Se omitido = *.*
!  Recursivo=1           Se extende às pastas: 1 = Sim, 0=Não
!        Se omitido = 1
!  Opcao=NI              Opcao de cópia : T-Todos, N-Novos (mais recentes), I-Inexistentes
!        Se omitido, o padrão é NI ( Mias recente e inexistentes )
!  Monitor=T             Tipo de monitor:
!        T=Total, mostra copiados e nao copiados, S=Simples, só copiados
!        Se omitido = "S"
!----------------- Instruções ---------------------------------------------------------------

[Bloco0]                       
Origem=\\192.168.109.xx\Vortice_Atualiza\TDev
Destino=C:\Vortice\TDev
Extensao=*.*
Descricao=Arquivos do client
Recursivo=0
Monitor=S
Opcao=NI  

[Bloco1]                       
Origem=\\192.168.109.xx\Vortice_Atualiza\TDev\App
Destino=C:\Vortice\TDev\App
Extensao=*.*
Descricao=Arquivos App
Recursivo=0
Monitor=S
Opcao=NI  

[Bloco2]                       
Origem=\\192.168.109.xx\Vortice_Atualiza\TDev\UTL
Destino=C:\Vortice\TDev\UTL
Extensao=*.*
Descricao=Arquivos Utl
Recursivo=0
Monitor=S
Opcao=NI  


[Bloco3]                       
Origem=\\192.168.109.xx\Vortice_Atualiza\TDev\QRP
Destino=C:\Vortice\TDev\QRP
Extensao=*.*
Descricao=Arquivos Qrp
Recursivo=0
Monitor=S
Opcao=NI
```

## `TDBin\sql.ini`

_659 bytes ? modificado em 2026-08-23 18:36_

```
;---  SQL.INI Modelo   09/2021

[win32client]
clientname=COLWCRMBD
clientruntimedir="C:\Vortice\TDBin"  


[win32client.dll]
; order of win32client dll's is important when connecting to multiple databases. 
; sqlws32 should always be the last and sqlodb32 be 2nd last. 
;comdll=sqlora32 
; comdll=sqlsyb32 
comdll=sqlodb32
; comdll=sqlifx32 

[odbcrtr]
remotedbname=CRM,DSN=CRM
odbctrace=off
odbctracefile=sql.log
longbuffer=1600000
EnableMultipleConnections=off
; FetchRow=500
; BRSDriver=msodbcsql

;[infogtwy]
;remotedbname=IFX,DATABASE"sysmaster",-h<host_name> -s<service_name> -v<inofrmix_server>
;buffrow=0
;defconnect=off
```

## `TDBin\DCC.INI`

_1185 bytes ? modificado em 2020-12-30 05:10_

```
;----------------------------------------------------------------------------
;
; (c) Unify Corporation, 2009
; Database Connectivity Configuration Information file
;
;
;----------------------------------------------------------------------------
; SECTION:  Dump Option
;
; This section has two keywords - DUMP and PASSWORD. 
;
; Description: If DUMP is set to SUPPLEMENTARY, Unify products dump the
;              backend information from the ODBC driver for the active
;              session to this file. This information is placed in the
;              section named after the backend, its version and the
;              driver name.
;              The first section contains all the ODBC GetInfo results.
;              More sections are created depending on the number of
;              data types supported by the backend. One section is
;              created for each data type.
;
;              If DUMP is set to OFF, no changes are made to this file.
;
;              The PASSWORD keyword is reserved and should not be modified.
;----------------------------------------------------------------------------
;

[Dump Option]
DUMP=OFF
PASSWORD=***REDIGIDO***
```

## `NT\Config\app.xml`

_1858 bytes ? modificado em 2025-05-19 10:50_

```
<?xml version="1.0"?>
<!--Xml gerado automÃ¡ticamente.-->
<!--NÃ£o alterar.-->
<Root>
  <Elementos>
    <Geral>
      <Culture>pt-BR</Culture>
      <Logo01>
      </Logo01>
      <Debug>False</Debug>
      <DebugNivel>3</DebugNivel>
      <UsrLogin>VRTCSERVER</UsrLogin>
      <PasLogin>***REDIGIDO***</PasLogin>
      <EmpLogin>1</EmpLogin>
      <MaxParallelism>10</MaxParallelism>
    </Geral>
    <Conexao>
      <ConBase>CRM</ConBase>
      <ConCrm>CRM</ConCrm>
      <ConDw>
      </ConDw>
      <ConErp1>
      </ConErp1>
      <ConErp2>
      </ConErp2>
      <Sessoes>
        <Sessao alias="CRM">
          <Owner>dbo</Owner>
          <Provider>SQL Server</Provider>
          <Tipo>Nativo</Tipo>
          <StrCon>server=COLWCRM; database=CRM; UID=crm; PWD=***REDIGIDO***; Trusted_Connection=False;</StrCon>
          <QueryTimeOut>600</QueryTimeOut>
          <Observacoes>
          </Observacoes>
        </Sessao>
      </Sessoes>
    </Conexao>
    <Conexao_BD>
      <Sessoes>
        <Sessao alias="CRM">
          <Owner>dbo</Owner>
          <Provider>SQL Server</Provider>
          <Tipo>Nativo</Tipo>
          <StrCon>server=COLWCRM; database=CRM; UID=crm; PWD=***REDIGIDO***; Trusted_Connection=False;</StrCon>
          <QueryTimeOut>600</QueryTimeOut>
          <Observacoes>
          </Observacoes>
        </Sessao>
        <Sessao alias="crmteste">
          <Owner>dbo</Owner>
          <Provider>SQL Server</Provider>
          <Tipo>Nativo</Tipo>
          <StrCon>server=COLWCRM; database=CRM_TESTE; UID=crm; PWD=***REDIGIDO***; Trusted_connection=False;</StrCon>
          <QueryTimeOut>600</QueryTimeOut>
          <Observacoes>
          </Observacoes>
        </Sessao>
      </Sessoes>
      <ConBase>CRM</ConBase>
    </Conexao_BD>
  </Elementos>
</Root>
```

## `NT\Config\config.xml`

_1812 bytes ? modificado em 2025-05-22 14:47_

```
<?xml version="1.0"?>
<!--Xml gerado automÃ¡ticamente.-->
<!--NÃ£o alterar.-->
<Root>
  <Elementos>
    <VorticoServerAlerta>
      <vsEmailAlertaRemetente>
      </vsEmailAlertaRemetente>
      <vsNomeAlertaRemetente>
      </vsNomeAlertaRemetente>
      <vsEmailAlertaUsr>
      </vsEmailAlertaUsr>
      <vsEmailAlertaPasw>***REDIGIDO***</vsEmailAlertaPasw>
      <vsEmailAlertaSmtp>
      </vsEmailAlertaSmtp>
      <vsEmailAlertaDominio>
      </vsEmailAlertaDominio>
      <vnEmailAlertaPort>
      </vnEmailAlertaPort>
      <vbEmailAlertaIsHtml>
      </vbEmailAlertaIsHtml>
      <vbEmailAlertaSsl>
      </vbEmailAlertaSsl>
      <vsEmailAlertaDestinatario>
      </vsEmailAlertaDestinatario>
    </VorticoServerAlerta>
    <Qlik_Integrador>
      <PastaLeitura>***REDIGIDO***</PastaLeitura>
      <PastaLeituraOk>***REDIGIDO***</PastaLeituraOk>
      <Prefixo>
      </Prefixo>
      <PlanilhaNome>MyWorkSheet-1</PlanilhaNome>
      <ColunaNome>SEQPESSOA</ColunaNome>
      <GCQtdDias>15</GCQtdDias>
      <CsvIndexSeqPessoa>0</CsvIndexSeqPessoa>
    </Qlik_Integrador>
    <OutCRM>
      <vsLinkBaseAgenda>
      </vsLinkBaseAgenda>
      <vsLinkBasePessoa>
      </vsLinkBasePessoa>
      <vsAssuntoAgenda>
      </vsAssuntoAgenda>
      <vsAssuntoRecado>
      </vsAssuntoRecado>
      <vsAssuntoCiencia>
      </vsAssuntoCiencia>
      <vsTemplateEmailAgenda>
      </vsTemplateEmailAgenda>
      <vsTemplateEmailRecado>
      </vsTemplateEmailRecado>
      <vsTemplateEmailCiencia>
      </vsTemplateEmailCiencia>
      <vsTemplatePessoa>
      </vsTemplatePessoa>
      <vsTemplateContato>
      </vsTemplateContato>
      <vsTemplateHistorico>
      </vsTemplateHistorico>
    </OutCRM>
  </Elementos>
</Root>
```

## `NT\AppMonitorService.exe.config`

_161 bytes ? modificado em 2022-01-19 16:05_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/></startup></configuration>
```

## `NT\Vortico Server.exe.config`

_13403 bytes ? modificado em 2025-05-08 15:57_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <sectionGroup name="applicationSettings" type="System.Configuration.ApplicationSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
      <section name="VorticoServer.Properties.Settings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
    </sectionGroup>
  </configSections>
  <system.serviceModel>
    <extensions>
      <bindingElementExtensions>
        <add name="customTextMessageEncoding" type="Microsoft.ServiceModel.Samples.CustomTextMessageEncodingElement,Microsoft.ServiceModel.Samples.CustomTextEncoder" />
      </bindingElementExtensions>
    </extensions>
    <bindings>
      <basicHttpBinding>
        <binding name="wsVorticeAdmSoap" />
        <binding name="FinasaWSServiceSoapBinding" />
        <binding name="SmsSoapBinding">
          <security mode="Transport">
            <transport clientCredentialType="None" proxyCredentialType="None" realm="" />
            <message clientCredentialType="UserName" algorithmSuite="Default" />
          </security>
        </binding>
        <binding name="InventoryBinding" />
        <binding name="CreditoSoap" sendTimeout="00:02:00" receiveTimeout="00:02:00" />
        <binding name="DadosCadastraisSoap" sendTimeout="00:02:00" receiveTimeout="00:02:00" />
        <binding name="CartaoSoap" sendTimeout="00:02:00" receiveTimeout="00:02:00" />
        <binding name="serverUsbiBinding" />
        <binding name="ReluzCap Web ServiceSoap">
          <security mode="Transport">
            <transport clientCredentialType="None" proxyCredentialType="None" realm="" />
            <message clientCredentialType="Certificate" algorithmSuite="Default" />
          </security>
        </binding>
        <binding name="ReluzCap Web ServiceSoap1" />
        <binding name="ReluzCap Web ServiceSoap12" />
        <binding name="ReceiveMessagePortBinding" />
        <binding name="MPGatewaySoap" />
        <binding name="wsVorticeSoap" />
        <binding name="PODataService_Version1_1ImplSoapBinding" maxReceivedMessageSize="20971520">
          <security mode="Transport">
            <transport clientCredentialType="Basic" proxyCredentialType="None" realm="" />
            <message clientCredentialType="Certificate" algorithmSuite="Default" />
          </security>
        </binding>
        <binding name="MaintainQuote_Version6_6ImplSoapBinding" maxReceivedMessageSize="20971520">
          <security mode="Transport">
            <transport clientCredentialType="Basic" proxyCredentialType="None" realm="" />
            <message clientCredentialType="Certificate" algorithmSuite="Default" />
          </security>
        </binding>
        <binding name="Venda2Soap11Binding">
          <security mode="Transport"></security>
        </binding>
        <binding name="ISOServicePortBinding">
          <security mode="Transport">
            <transport clientCredentialType="None" proxyCredentialType="None" realm="" />
            <message clientCredentialType="Certificate" algorithmSuite="Default" />
          </security>
        </binding>
      </basicHttpBinding>
      <customBinding>
        <binding name="ReluzCap Web ServiceSoap12">
          <textMessageEncoding messageVersion="Soap12" />
          <httpsTransport />
        </binding>
        <binding name="wsVorticeAdmSoap12">
          <textMessageEncoding messageVersion="Soap12" />
          <httpTransport />
        </binding>
        <binding name="WorkflowWebServiceSoap12">
          <textMessageEncoding messageVersion="Soap12" />
          <httpsTransport maxReceivedMessageSize="2147483647" />
        </binding>
      </customBinding>
    </bindings>
    <client>
      <endpoint address="http://webservice.finasa.com.br/financiamentoAutomatico/services/FinasaWSService" binding="basicHttpBinding" bindingConfiguration="FinasaWSServiceSoapBinding" contract="BradescoSrvRef.FinasaWSService" name="FinasaWSService" />
      <endpoint address="http://10.19.254.xx:30047/Cartao.asmx" binding="basicHttpBinding" bindingConfiguration="CartaoSoap" contract="srvConductor_Cartao.CartaoSoap" name="CartaoSoap" />
      <endpoint address="http://10.19.254.xx:30047/DadosCadastrais.asmx" binding="basicHttpBinding" bindingConfiguration="DadosCadastraisSoap" contract="srvConductor_DadosCadastrais.DadosCadastraisSoap" name="DadosCadastraisSoap" />
      <endpoint address="http://10.19.254.xx:30047/Credito.asmx" binding="basicHttpBinding" bindingConfiguration="CreditoSoap" contract="srvConductor_Credito.CreditoSoap" name="CreditoSoap" />
      <endpoint address="http://10.19.252.xx:30000/Credito.asmx" binding="basicHttpBinding" bindingConfiguration="CreditoSoap" contract="srvConductor_Credito.CreditoSoap" name="CreditoSoapAlternativo" />
      <endpoint address="https://api-http.zenvia.com/GatewayIntegration/services/Sms" binding="basicHttpBinding" bindingConfiguration="SmsSoapBinding" contract="srvHumanServiceReference.Sms_BindingImpl" name="Sms" />
      <endpoint address="http://api.recuperemais.com.br/sms" binding="basicHttpBinding" bindingConfiguration="InventoryBinding" contract="srvPsServiceReference.InventoryPortType" name="InventoryPort" />
      <endpoint address="https://webservices2.twwwireless.com.br/reluzcap/wsreluzcap.asmx" binding="basicHttpBinding" bindingConfiguration="ReluzCap Web ServiceSoap" contract="srvTwwServiceReference.ReluzCapWebServiceSoap" name="ReluzCap Web ServiceSoap" />
      <endpoint address="https://webservices2.twwwireless.com.br/reluzcap/wsreluzcap.asmx" binding="customBinding" bindingConfiguration="ReluzCap Web ServiceSoap12" contract="srvTwwServiceReference.ReluzCapWebServiceSoap" name="ReluzCap Web ServiceSoap12" />
      <endpoint address="http://www.facilitamovel.com.br:80/ReceiveMessage" binding="basicHttpBinding" bindingConfiguration="ReceiveMessagePortBinding" contract="srvFacilitaMovelGetReturnReference.ReceiveMessage" name="ReceiveMessagePort" />
      <endpoint address="http://www.mpgateway.com/v_3_00/sms/service.asmx" binding="basicHttpBinding" bindingConfiguration="MPGatewaySoap" contract="srvMobiProntoReference.MPGatewaySoap" name="MPGatewaySoap" />
      <endpoint address="http://intranet.vortice.inf.br:8082/wsVortice/wsVortice.asmx" binding="basicHttpBinding" bindingConfiguration="wsVorticeSoap" contract="srvWsVortice.wsVorticeSoap" name="wsVorticeSoap" />
      <endpoint address="http://usbi.autoavaliar.com.br/webServerUsbi.php" binding="basicHttpBinding" bindingConfiguration="serverUsbiBinding" contract="srvMegaDealer_AutoAvaliar.serverUsbiPortType" name="serverUsbiPort" />
      <endpoint address="https://jdquote2.deere.com/services/PODataService_Version1_1Impl" binding="basicHttpBinding" bindingConfiguration="PODataService_Version1_1ImplSoapBinding" contract="srvPO_DataService.PODataService_Version1_1Impl" name="PODataService_Version1_1Impl" />
      <endpoint address="https://jdquote2.deere.com/services/MaintainQuote_Version6_6Impl" binding="basicHttpBinding" bindingConfiguration="MaintainQuote_Version6_6ImplSoapBinding" contract="srvJD_MaintainQuoteReference.MaintainQuote_Version6_6Impl" name="MaintainQuote_Version6_6Impl" />
      <endpoint address="http://189.2.151.226:8082/wsVorticeAdm/wsVorticeAdm.asmx" binding="basicHttpBinding" bindingConfiguration="wsVorticeAdmSoap" contract="wsVorticeAdm.wsVorticeAdmSoap" name="wsVorticeAdmSoap" />
      <endpoint address="http://189.2.151.226:8082/wsVorticeAdm/wsVorticeAdm.asmx" binding="customBinding" bindingConfiguration="wsVorticeAdmSoap12" contract="wsVorticeAdm.wsVorticeAdmSoap" name="wsVorticeAdmSoap12" />
      <!-- ProduÃ§Ã£o. -->
      <endpoint address="https://fisystem.com.br/services/services/Venda2.Venda2HttpSoap11Endpoint/" binding="basicHttpBinding" bindingConfiguration="Venda2Soap11Binding" contract="srvFIBrasilVenda2.Venda2PortType" name="Venda2HttpSoap11Endpoint" />
      <!-- HomologaÃ§Ã£o
      <endpoint address="https://fisystem.com.br/homologaservices/services/Venda2.Venda2HttpSoap11Endpoint/" binding="basicHttpBinding" bindingConfiguration="Venda2Soap11Binding" contract="srvFIBrasilVenda2.Venda2PortType" name="Venda2HttpSoap11Endpoint" />
      -->
      <endpoint address="https://dr-savegnago-prd.neurotech.com.br/services/soap/porting" binding="customBinding" bindingConfiguration="WorkflowWebServiceSoap12" contract="srvNeurotech.WorkflowWebServiceSoap" name="WorkflowWebServiceSoap12" />
      <!-- https://dr-savegnago-prd.neurotech.com.br/services/soap/porting PRODUÃ‡ÃƒO-->
      <!-- https://dr-hml.neurotech.com.br/services/soap/porting HOMOLOGAÃ‡ÃƒO-->
      <endpoint address="https://savegnagoprivate.connect.dock.tech/ISOService" binding="basicHttpBinding" bindingConfiguration="ISOServicePortBinding" contract="srvAppCardsServiceReference.ISOService" name="ISOServicePort" />
      <!-- https://savegnagoprivate.connect.dock.tech/ISOService PRODUÃ‡ÃƒO -->
    </client>
  </system.serviceModel>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
  <system.net>
    <!-- Descomente este linha caso a mÃ¡quina use proxy.
    <defaultProxy enabled="true" useDefaultCredentials="true"></defaultProxy>
    
    <defaultProxy useDefaultCredentials="true">
      <proxy usesystemdefault="False" proxyaddress="http://192.168.1.xx:3128" bypassonlocal="True" />
    </defaultProxy>

    <defaultProxy useDefaultCredentials="true">
      <proxy usesystemdefault="False" scriptLocation="http://192.168.1.xx:65480/wpad.dat" />
    </defaultProxy>
    -->
    <settings>
      <httpWebRequest useUnsafeHeaderParsing="true" />
    </settings>
  </system.net>
  <applicationSettings>
    <VorticoServer.Properties.Settings>
      <setting name="URLJupiter" serializeAs="String">
        <value>http://localhost:8083/</value>
      </setting>
      <setting name="GT_GMT" serializeAs="String">
        <value>0</value>
      </setting>
      <setting name="MonitServProc" serializeAs="String">
        <value>True</value>
      </setting>
      <setting name="UrlGTRest" serializeAs="String">
        <value>https://web.vortice.inf.br:15443/vorticehub/wsVorticeCrmApi/api/vorticeadmin/</value>
      </setting>
    </VorticoServer.Properties.Settings>
  </applicationSettings>
  <runtime>
    <gcAllowVeryLargeObjects enabled="true" />
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System" publicKeyToken="b77a5c561934e089" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Core" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.68.0.0" newVersion="1.68.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="BouncyCastle.Crypto" publicKeyToken="0e99375e54769942" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.9.0.0" newVersion="1.9.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.IdentityModel.Abstractions" culture="neutral" publicKeyToken="31bf3856ad364e35" />
        <bindingRedirect oldVersion="0.0.0.0-8.1.0.0" newVersion="8.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Formats.Asn1" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-9.0.0.0" newVersion="9.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Diagnostics.DiagnosticSource" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.1" newVersion="8.0.0.1" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Auth" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.68.0.0" newVersion="1.68.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

## `NT\Jupiter Service.exe.config`

_5820 bytes ? modificado em 2025-05-05 09:19_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <sectionGroup name="userSettings" type="System.Configuration.UserSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
      <section name="Jupiter.Properties.Settings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" allowExeDefinition="MachineToLocalUser" requirePermission="false" />
    </sectionGroup>
  </configSections>
  <system.web>
    <httpRuntime maxUrlLength="8000" />
  </system.web>
  <system.serviceModel>
    <bindings>
      <webHttpBinding>
        <binding maxBufferPoolSize="2147483647" maxReceivedMessageSize="2147483647" maxBufferSize="2147483647" transferMode="Streamed">
          <readerQuotas maxStringContentLength="2147483647" />
          <!--<security mode="Transport" />-->
        </binding>
      </webHttpBinding>
      <basicHttpBinding>
        <binding name="wsVorticeAdmSoap" />
      </basicHttpBinding>
      <customBinding>
        <binding name="wsVorticeAdmSoap12">
          <textMessageEncoding messageVersion="Soap12" />
          <httpTransport />
        </binding>
      </customBinding>
    </bindings>
    <client>
      <endpoint address="http://localhost:29526/wsVorticeAdm.asmx" binding="basicHttpBinding" bindingConfiguration="wsVorticeAdmSoap" contract="wsVorticeAdm.wsVorticeAdmSoap" name="wsVorticeAdmSoap" />
      <endpoint address="http://localhost:29526/wsVorticeAdm.asmx" binding="customBinding" bindingConfiguration="wsVorticeAdmSoap12" contract="wsVorticeAdm.wsVorticeAdmSoap" name="wsVorticeAdmSoap12" />
    </client>
    <services>
      <service name="Jupiter.Servicos" behaviorConfiguration="MetadataBehavior">
        <endpoint address="http://localhost:8083/" binding="webHttpBinding" contract="Jupiter.IServico" />
      </service>
    </services>
    <behaviors>
      <serviceBehaviors>
        <behavior name="MetadataBehavior">
          <serviceDebug includeExceptionDetailInFaults="True" httpHelpPageEnabled="True" />
          <serviceMetadata httpGetEnabled="True" />
          <dataContractSerializer maxItemsInObjectGraph="2147483646" />
        </behavior>
      </serviceBehaviors>
      <endpointBehaviors>
        <behavior name="json">
          <webHttp />
        </behavior>
        <behavior>
          <dataContractSerializer maxItemsInObjectGraph="2147483646" />
          <webHttp />
        </behavior>
      </endpointBehaviors>
    </behaviors>
  </system.serviceModel>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits>
          <headerLimits>
            <add header="Content-type" sizeLimit="10485760" />
          </headerLimits>
        </requestLimits>
      </requestFiltering>
    </security>
  </system.webServer>
  <userSettings>
    <Jupiter.Properties.Settings>
      <setting name="pgnPackageLimit" serializeAs="String">
        <value>1000</value>
      </setting>
      <setting name="pgsSocketUrl" serializeAs="String">
        <value>http://127.0.0.1:8087/</value>
      </setting>
      <setting name="pgsAddress" serializeAs="String">
        <value>http://localhost:8083/</value>
      </setting>
    </Jupiter.Properties.Settings>
  </userSettings>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Web" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System" publicKeyToken="b77a5c561934e089" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Core" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.IdentityModel.Abstractions" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.1.0.0" newVersion="8.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Diagnostics.DiagnosticSource" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.1" newVersion="8.0.0.1" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Formats.Asn1" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.0" newVersion="8.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

## `NT\Vortico Admin.exe.config`

_6944 bytes ? modificado em 2025-05-05 09:19_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <configSections>
    </configSections>
    <system.serviceModel>
      <bindings>
        <basicHttpBinding>
          <binding name="wsVorticeAdmSoap" />
          <binding name="SmsSoapBinding">
            <security mode="Transport">
              <transport clientCredentialType="None" proxyCredentialType="None" realm="" />
              <message clientCredentialType="UserName" algorithmSuite="Default" />
            </security>
          </binding>
          <binding name="InventoryBinding" />
          <binding name="ReluzCap Web ServiceSoap">
            <security mode="Transport">
              <transport clientCredentialType="None" proxyCredentialType="None" realm="" />
              <message clientCredentialType="Certificate" algorithmSuite="Default" />
            </security>
          </binding>
          <binding name="ReluzCap Web ServiceSoap1" />
          <binding name="ReluzCap Web ServiceSoap12" />
          <binding name="SendMessagePortBinding" />
          <binding name="ReceiveMessagePortBinding" />
          <binding name="MPGatewaySoap" />
        </basicHttpBinding>
        <customBinding>
          <binding name="wsVorticeAdmSoap12">
            <textMessageEncoding messageVersion="Soap12" />
            <httpTransport />
          </binding>
          <binding name="ReluzCap Web ServiceSoap12">
            <textMessageEncoding messageVersion="Soap12" />
            <httpsTransport />
          </binding>
        </customBinding>
      </bindings>
      <client>
        <endpoint address="http://tupungato:8084/wsVorticeAdm/wsVorticeAdm.asmx" binding="basicHttpBinding" bindingConfiguration="wsVorticeAdmSoap" contract="wsVorticeAdm.wsVorticeAdmSoap" name="wsVorticeAdmSoap" />

        <endpoint address="http://tupungato:8084/wsVorticeAdm/wsVorticeAdm.asmx" binding="customBinding" bindingConfiguration="wsVorticeAdmSoap12" contract="wsVorticeAdm.wsVorticeAdmSoap" name="wsVorticeAdmSoap12" />

        <endpoint address="https://api-http.zenvia.com/GatewayIntegration/services/Sms" binding="basicHttpBinding" bindingConfiguration="SmsSoapBinding" contract="srvHumanServiceReference.Sms_BindingImpl" name="Sms" />

        <endpoint address="http://api.recuperemais.com.br/sms" binding="basicHttpBinding" bindingConfiguration="InventoryBinding" contract="srvPsServiceReference.InventoryPortType" name="InventoryPort" />

        <endpoint address="https://webservices2.twwwireless.com.br/reluzcap/wsreluzcap.asmx" binding="basicHttpBinding" bindingConfiguration="ReluzCap Web ServiceSoap" contract="srvTwwServiceReference.ReluzCapWebServiceSoap" name="ReluzCap Web ServiceSoap" />

        <endpoint address="https://webservices2.twwwireless.com.br/reluzcap/wsreluzcap.asmx" binding="customBinding" bindingConfiguration="ReluzCap Web ServiceSoap12" contract="srvTwwServiceReference.ReluzCapWebServiceSoap" name="ReluzCap Web ServiceSoap12" />

        <endpoint address="http://www.facilitamovel.com.br:80/SendMessage" binding="basicHttpBinding" bindingConfiguration="SendMessagePortBinding" contract="srvFacilitaMovelReference.SendMessage" name="SendMessagePort" />

        <endpoint address="http://www.facilitamovel.com.br:80/ReceiveMessage" binding="basicHttpBinding" bindingConfiguration="ReceiveMessagePortBinding" contract="srvFacilitaMovelGetReturnReference.ReceiveMessage" name="ReceiveMessagePort" />

        <endpoint address="http://www.mpgateway.com/v_3_00/sms/service.asmx" binding="basicHttpBinding" bindingConfiguration="MPGatewaySoap" contract="srvMobiProntoReference.MPGatewaySoap" name="MPGatewaySoap" />

      </client>
    </system.serviceModel>
<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" /></startup>
  <runtime>
    <gcAllowVeryLargeObjects enabled="true" />
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System" publicKeyToken="b77a5c561934e089" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.64.0.0" newVersion="1.64.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Core" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.64.0.0" newVersion="1.64.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="SuperSocket.ClientEngine" publicKeyToken="ee9af13f57f00acc" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-0.8.0.14" newVersion="0.8.0.14" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Auth" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.IdentityModel.Abstractions" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.1.0.0" newVersion="8.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="MimeKit" publicKeyToken="bede1c8a46c66814" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.5.0.0" newVersion="4.5.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Formats.Asn1" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.0" newVersion="8.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Diagnostics.DiagnosticSource" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.1" newVersion="8.0.0.1" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

## `NT\Config Con.exe.config`

_2488 bytes ? modificado em 2025-05-05 09:19_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Web" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System" publicKeyToken="b77a5c561934e089" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Core" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.IdentityModel.Abstractions" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.1.0.0" newVersion="8.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Diagnostics.DiagnosticSource" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.1" newVersion="8.0.0.1" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Formats.Asn1" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.0" newVersion="8.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" /></startup></configuration>
```

## `NT\SQL net.exe.config`

_4778 bytes ? modificado em 2025-05-08 15:56_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System" publicKeyToken="b77a5c561934e089" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Core" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.67.0.0" newVersion="1.67.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.67.0.0" newVersion="1.67.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.PlatformServices" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Auth" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.67.0.0" newVersion="1.67.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Auth.PlatformServices" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Calendar.v3" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.68.0.3402" newVersion="1.68.0.3402" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="sapnco" publicKeyToken="50436dca5c7f7d23" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-3.0.0.42" newVersion="3.0.0.42" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="MailKit" publicKeyToken="4e064fe7c44a8f1b" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.4.0.0" newVersion="4.4.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="MimeKit" publicKeyToken="bede1c8a46c66814" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.4.0.0" newVersion="4.4.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.Identity.Client" publicKeyToken="0a613f4dd989e8ae" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.60.2.0" newVersion="4.60.2.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.IdentityModel.Abstractions" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.1.0.0" newVersion="8.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Diagnostics.DiagnosticSource" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.1" newVersion="8.0.0.1" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Formats.Asn1" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.0" newVersion="8.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="sapnco_utils" publicKeyToken="50436dca5c7f7d23" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-3.1.0.42" newVersion="3.1.0.42" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
</configuration>
```

## `NT\Bi Integrador.exe.config`

_3506 bytes ? modificado em 2025-04-16 16:58_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System" publicKeyToken="b77a5c561934e089" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Web" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.0.0" newVersion="4.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Google.Apis.Core" publicKeyToken="4b01fa6e34db77ab" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.32.0.0" newVersion="1.32.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.IdentityModel.Abstractions" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-7.5.2.0" newVersion="7.5.2.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Diagnostics.DiagnosticSource" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.1" newVersion="8.0.0.1" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Formats.Asn1" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.0" newVersion="8.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.ValueTuple" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
      </dependentAssembly>
    </assemblyBinding>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Threading.Tasks.Extensions" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.2.0.1" newVersion="4.2.0.1" />
      </dependentAssembly>
    </assemblyBinding>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Numerics.Vectors" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-4.1.4.0" newVersion="4.1.4.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

## `NT\Smtp Tester.exe.config`

_1304 bytes ? modificado em 2025-05-05 09:19_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" /></startup>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-6.0.0.0" newVersion="6.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.IdentityModel.Abstractions" publicKeyToken="31bf3856ad364e35" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.1.0.0" newVersion="8.1.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Diagnostics.DiagnosticSource" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-8.0.0.1" newVersion="8.0.0.1" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

## `NT\Database Check.exe.config`

_530 bytes ? modificado em 2025-05-08 15:21_

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

## `Install\silent.ini`

_1304 bytes ? modificado em 2021-11-07 22:45_

```
[{0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-DlgOrder]
Dlg0={0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdWelcome-0
Count=6
Dlg1={0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdLicense2-0
Dlg2={0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdAskDestPath-0
Dlg3={0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdComponentTree-0
Dlg4={0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdStartCopy-0
Dlg5={0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdFinish-0
[{0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdWelcome-0]
Result=1
[{0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdLicense2-0]
Result=1
[{0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdAskDestPath-0]
szDir=C:\Vortice\TDBin
Result=1
[{0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdComponentTree-0]
szDir=C:\Vortice\TDBin\
NativeDatabaseRouters-type=string
NativeDatabaseRouters-count=3
NativeDatabaseRouters-0=NativeDatabaseRouters\SQLRouterforOracle
NativeDatabaseRouters-1=NativeDatabaseRouters\SQLRouterforODBC
NativeDatabaseRouters-2=NativeDatabaseRouters\SQLRouterforInformix
Component-type=string
Component-count=4
Component-0=UpdatePathEnvironmentVariable
Component-1=RootFiles
Component-2=Axis2c
Component-3=NativeDatabaseRouters
Result=1
[{0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdStartCopy-0]
Result=1
[{0D9A1EE1-4126-4CE2-A3DC-6C682A8F2806}-SdFinish-0]
Result=1
bOpt1=0
bOpt2=0
```



