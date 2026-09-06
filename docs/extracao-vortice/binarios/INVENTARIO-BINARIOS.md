# Inventário de binários e infraestrutura do Vórtice CRM

> **Escopo:** como o produto Vórtice está montado — executáveis, DLLs, serviços, sites IIS,
> configurações e superfície de rede. **Não houve engenharia reversa de código-fonte:** nenhum
> assembly foi carregado, nenhum IL foi lido ou reconstruído. O que se leu foram *metadados* —
> o equivalente a ler a etiqueta do arquivo.
>
> **Coleta:** 02/09/2026, via VPN, **somente leitura**. Nada foi alterado, parado, copiado por cima
> ou apagado em qualquer servidor. Nenhum binário foi copiado para a máquina local — metadados e
> hashes bastam.
>
> **Redação:** senhas, tokens e chaves aparecem como `***REDIGIDO***`. O último octeto de IPs
> privados aparece como `.xx` no material derivado; nos endereços de arquitetura deste relatório
> os IPs estão inteiros porque já constam da documentação interna do projeto.

**Números:** 4 hosts alcançados de 6 testados · **1.397 arquivos** inventariados · **696 com SHA256**
· **310 assemblies .NET** · **297 binários nativos** · **116 operações** de API mapeadas.

---

## 1. Como ler este relatório

O material está separado em **visto** e **inferido**:

| Marca | Significado |
|---|---|
| ✅ **VISTO** | Medido diretamente nesta coleta (arquivo lido, porta testada, consulta executada) |
| 📄 **INFERIDO** | Vem dos runbooks e da memória do projeto; **não** foi confirmado nesta coleta |

O grosso do relatório é ✅. Onde há 📄, está dito.

**Arquivos que acompanham este relatório:**

| Arquivo | Conteúdo |
|---|---|
| `inventario-ts172-{TDev,TDBin,NT,Install}.csv` | Inventário bruto por pasta do terminal server |
| `inventario-appserver-Vortice_Atualiza.csv` | Inventário do share de atualização no app server |
| `inventario-alcance-portas.csv` · `-shares.csv` | Varredura de portas e de shares |
| `configs-ts172-redigido.md` | Configurações, com segredos redigidos |
| `host-ECS-ST-TSPLUS.md` | SO, serviços, ODBC, .NET, RemoteApp, software instalado |
| `swagger-wsVorticeCrmApi.json` | Spec Swagger completo da API |
| `API-endpoints-completo.md` · `API-endpoints.csv` | 116 operações, com diff contra a doc existente |
| `PROPOSTA-ABRIR-CAIXA-PRETA.md` | Procedimento para capturar o SQL do cliente Gupta |

**Scripts reexecutáveis** (sem credenciais) em `c:\projetos\tracbel-crm\scripts\vortice-extracao\`:

| Script | O que faz |
|---|---|
| `binarios-01-alcance.ps1` | Varredura de portas e de shares nos 6 hosts |
| `binarios-02-configs.ps1` | Lê os arquivos de configuração e aplica a redação de segredos |
| `binarios-03-host-wmi.ps1` | Coleta remota por WMI/DCOM (SO, serviços, ODBC, .NET, RemoteApp) |
| `binarios-04-swagger.ps1` | Baixa o spec da API e gera o markdown de endpoints com diff |
| **`binarios-05-coleta-local.ps1`** | **Para rodar dentro do RDP** do agrocrm e do COLWCRM — fecha a lacuna de IIS, serviços, tarefas e `web.config` (ver §10) |
| `BinInspector/` | Utilitário C# que lê metadados PE e CLI (`MetadataReader`) e calcula SHA256 |

---

## 2. Alcance por host

### 2.1 O que respondeu

| Host | IP | Papel | Ping | Portas abertas | Share administrativo | WMI/DCOM |
|---|---|---|---|---|---|---|
| **COLWCRM** | 10.150.14.65 | Banco SQL Server + FTP Doc Manager + IIS | ✅ | 21, 80, 135, 139, 445, 1433, 3389, 5985 | ❌ acesso negado | ❌ acesso negado |
| **agrocrm** (VRTCSERVER) | 10.150.6.230 | App server: IIS (CRMWeb + API), serviços | ✅ | 80, 135, 139, **443**, 445, 3389, 5985, 8080 | ❌ acesso negado | ❌ acesso negado |
| **COLWRDP01** | 10.150.5.54 | Terminal server **antigo** | ✅ | 135, 139, 445, 3389, 5985 | ❌ acesso negado | ❌ acesso negado |
| **ECS-ST-TSPLUS** | 10.150.5.172 | Terminal server **novo** | ✅ | 135, 139, 445, 3389, 5985, 5986 | ✅ **`c$` legível** | ✅ **funciona** |
| COLWCRM (IP antigo) | 192.168.109.220 | morto desde jul/2026 | ❌ | nenhuma | — | — |
| app server (IP antigo) | 192.168.109.209 | morto desde jul/2026 | ❌ | nenhuma | — | — |

✅ Os dois IPs da rede antiga estão **confirmadamente mortos** — sem ping e sem nenhuma porta.

> 📌 **Correção de uma informação da memória do projeto.** A memória
> `vortice-acesso-remoto-terminal-server` (31/08/2026) registra que "5985 (WinRM) e 445 (SMB) estão
> fechados" para 10.150.5.54 e 10.150.5.172. **Hoje as duas portas estão abertas nos quatro hosts.**
> O que bloqueia é **autorização**, não rede: WinRM recusa a autenticação em todos, e o `c$` só abre
> no ECS-ST-TSPLUS — onde a conta `grupo_tracbel\ricardo.moretti` tem privilégio administrativo.

### 2.2 O que isso significou para a coleta

- **ECS-ST-TSPLUS (10.150.5.172)** foi a fonte principal: `c$` legível e WMI/DCOM funcionando.
  Como é uma instalação **completa e recente** do Vórtice (28/08/2026), o inventário de binários
  vale para todo o produto desktop.
- **agrocrm (10.150.6.230)** entregou o share `Vortice_Atualiza` e toda a camada HTTP (Swagger,
  cabeçalhos, certificado).
- **COLWCRM (10.150.14.65)** entregou a camada de banco (via `Invoke-Vortice`, somente leitura) e o
  banner do FTP.
- **COLWRDP01 (10.150.5.54)** não entregou nada além do teste de portas. Nem SMB, nem WMI, nem
  shares publicados. Como está saindo de operação e o servidor novo é uma cópia validada dele,
  isso não é perda relevante.

---

## 3. Arquitetura observada

```
                      ┌──────────────────────────────────────────┐
   usuário ─RDP/RemoteApp─▶ ECS-ST-TSPLUS  10.150.5.172           │
                      │  Windows Server 2025 · dominio tracbel    │
                      │  C:\Vortice\TDev\App\vCRM Atendente.exe   │
                      │  runtime Gupta TD 7.3.4 em C:\Vortice\TDBin│
                      └──────────────┬───────────────────────────┘
                                     │ DSN ODBC 32 bits "CRM"
                                     │ driver SQLSRV32.dll
                                     │ Server=COLWCRM.coloradomaq.com.br
                                     ▼
   ┌──────────────────────────┐   ┌──────────────────────────────┐
   │ agrocrm 10.150.6.230     │   │ COLWCRM 10.150.14.65         │
   │ IIS 10.0                 │──▶│ SQL Server 2019 Std 15.0.2135│
   │  /CRMWeb   (WebForms)    │   │  bases CRM e CRM_HOMO        │
   │  /wsVorticeCrmApi 4.04.01│   │ FileZilla Server 1.3.0 (FTP) │
   │ C:\Vortice\NT (27 .exe)  │   │ share DocManager             │
   │ share Vortice_Atualiza   │   │ share Backup                 │
   │ share bkpbd              │   └──────────────────────────────┘
   └──────────────────────────┘
```

Três máquinas, três papéis. ✅ Confirmado: os nomes enganam (`COLWCRMBD` aparece como
`clientname` no `SQL.INI` do terminal server, mas quem é banco é o `COLWCRM`).

---

## 4. Versões-chave

| Camada | Versão medida | Onde foi visto |
|---|---|---|
| **Gupta / Team Developer (runtime)** | **OpenText Team Developer 7.3.4, Build 64934** (21/01/2021) | `TDBin\cdlli73.dll` e 45 outras DLLs `*73.dll` |
| Runtime Gupta legado, coexistindo | **Centura Team Developer 2.1-PTF4** (2003), 2.1, 1.1.2-PTF12, 1.1.0-PTF2 | `TDBin\*i21.dll`, `cdlli21.dll` |
| Gupta SQLBase (cliente) | 12.2.1 build 12624 · 12.2.0 · 7.1.0 · 7.0.0 | `TDBin\sqlwntm.dll`, `gptconfig.exe` |
| Instalador do runtime | `TDdeploy734x86.exe` 7.3.4 (132 MB, 09/03/2021) | `C:\Vortice\Install` |
| **Aplicação Vórtice — módulos** | família **4.04.01** (r01 a r18); alguns em 4.03/4.01; um em 3.7.00 | banco, `GE_Modulo.Versao` |
| **API REST** | **4.04.01-3** | `GET /api/ApiInfo` |
| Cliente desktop principal | `vCRM Atendente.exe` FileVersion **4.03.01** (17/03/2026) | VersionInfo do PE |
| Camada .NET do Vórtice | assemblies **4.4.1.0**, `.NETFramework v4.8` | `C:\Vortice\NT` |
| **SQL Server** | **2019 Standard 15.0.2135.5 (RTM-GDR, KB5058713)** | `@@VERSION` |
| SO do servidor de banco | Windows Server 2019 Standard, build 17763 | `@@VERSION` |
| SO do terminal server novo | **Windows Server 2025 Standard**, build 26100 | WMI |
| **IIS** | **Microsoft-IIS/10.0** (nos dois servidores web) | cabeçalho `Server` |
| **ASP.NET** | **4.0.30319** (`X-AspNet-Version`), WebForms | cabeçalho da API e do CRMWeb |
| .NET Framework no terminal server | **4.8.09221** (release 533509) — única versão | registro `NDP` |
| FTP (Doc Manager) | **FileZilla Server 1.3.0** | banner da porta 21 |

### 4.1 O que é Gupta / Team Developer, e por que isso define a estratégia

O cliente desktop do Vórtice não é .NET nem C++ comum. É escrito em **SAL**, a linguagem do
**Gupta Team Developer** (hoje da OpenText; antes Centura, antes Gupta Technologies). O compilador
gera **p-code proprietário**, empacotado dentro de um `.exe` que é essencialmente o carregador do
runtime.

Isso é visível no inventário: os 28 executáveis de `TDev\App` são PE **I386 (32 bits)** sem CLI
header — nativos para o Windows, mas **sem código nativo útil dentro**: o `vCRM Atendente.exe` de
14,7 MB é p-code mais recursos. Quem executa é o `cdlli73.dll` de 8,9 MB em `C:\Vortice\TDBin`.

**Consequência prática:** não existe descompilador público de p-code Gupta. Não vale a pena tentar.
A forma correta de abrir a caixa-preta é **capturar o SQL que o cliente emite** — procedimento
desenhado em [PROPOSTA-ABRIR-CAIXA-PRETA.md](PROPOSTA-ABRIR-CAIXA-PRETA.md), com Extended Events no
SQL Server como técnica principal e o `odbctrace` do próprio runtime como alternativa.

> ✅ Medição que reforça o ponto: o banco `CRM` tem **767 tabelas, 411 views, mas só 85 stored
> procedures, 51 funções e 1 trigger**. A regra de negócio não mora no banco — mora dentro do
> p-code.

**Por que o `PATH` importa tanto** (📄 dos runbooks, ✅ confirmado no registro): o executável está em
`TDev\App` e o runtime em `TDBin`. O Windows só encontra as DLLs porque as duas pastas são as
**primeiras** entradas do `PATH` do sistema — e é exatamente isso que se vê hoje no ECS-ST-TSPLUS:

```
C:\Vortice\TDBin
C:\Vortice\TDev
C:\WINDOWS\system32
...
```

---

## 5. Inventário por pasta

| Origem | Arquivos | Tamanho | .NET | Nativos | Com SHA256 |
|---|---:|---:|---:|---:|---:|
| `ECS-ST-TSPLUS:TDev` | 454 | 582,0 MB | 0 | 136 | 170 |
| `ECS-ST-TSPLUS:TDBin` | 200 | 252,2 MB | 35 | 109 | 161 |
| `ECS-ST-TSPLUS:NT` | 513 | 1.301,6 MB | 275 | 5 | 309 |
| `ECS-ST-TSPLUS:Install` | 8 | 154,7 MB | 0 | 5 | 5 |
| `agrocrm:Vortice_Atualiza` | 222 | 290,9 MB | 0 | 42 | 51 |
| **Total** | **1.397** | **2.581,4 MB** | **310** | **297** | **696** |

> Arquivos acima de 25 MB não receberam hash (registrado no campo `Observacao` do CSV), e `.db`,
> `.pdb`, `.QRP` e `.pdf` ficaram fora da lista de hash por decisão de custo — o link VPN entrega
> cerca de 1,7 s por arquivo, e hashear 2,5 GB levaria horas sem ganho analítico.

### 5.1 `C:\Vortice\TDev\App` — o cliente desktop (28 executáveis Gupta)

Todos **PE I386, sem CLI header** (p-code Gupta). Os que carregam VersionInfo:

| Executável | MB | Modificado | FileVersion | ProductName | Company | SHA256 (12) |
|---|---:|---|---|---|---|---|
| `vCRM Atendente.exe` | 14,7 | 2026-03-17 | **4.03.01** | Vórtico CRM | Vórtice Sistemas Ltda | `51fba2747f9a` |
| `vCRM_Super.exe` | 11,6 | 2025-12-22 | 4.03.00 | Vórtico CRM | Vórtice Sistemas Ltda | `0107232b165b` |
| `vCRM_Super_OLD.exe` | 11,6 | 2025-12-22 | 4.03.00 | Vórtico CRM | Vórtice Sistemas Ltda | `0107232b165b` |
| `vCRM_Config.exe` | 10,0 | 2026-07-10 | 4.01.00 | Vórtico CRM | Vórtice Sistemas Ltda | `55a0950746ff` |
| `vCRM_Admin.exe` | 9,7 | 2026-06-15 | 4.02.01 | Vórtico CRM | Vórtice Sistemas Ltda | `820d9c5fb0e7` |
| `vWorkflow Modeler.exe` | 10,2 | 2025-11-11 | 4.03 | Vórtico Modeler | Vórtice Sistemas Ltda | `3cbff8e1fca5` |
| `vWorkflow Modeler_old.exe` | 10,2 | 2025-09-15 | 4.03 | Vórtico Modeler | Vórtice Sistemas Ltda | `401bf3fbb45f` |
| `vCRM_ConsultaTitulos.exe` | 6,9 | 2025-03-14 | 4.03.01 | Vortico CRM Consulta Títulos | Vórtice Sistemas Ltda | `9f9024b7639d` |
| `vCRM_Higienizador.exe` | 7,0 | 2025-08-13 | 4.03.01 | Vórtico Higienizador | Vórtic**eo** Sistemas | `25f624c84562` |
| `vCRM_RunGEPImport.exe` | 7,5 | 2024-10-09 | 4.01.01 | Vortico CRM Importador | Vórtice Sistemas Ltda | `ca03696c1f23` |
| `vCRM_RunImportIMP_NEW.exe` | 8,3 | 2024-09-27 | 4.01.01 | Vórtico CRM Importador | Vórtice Sistemas Ltda | `6d17b00f963a` |
| `CRM_Tool_GeraProcEstatistica.exe` | 7,3 | 2025-07-22 | 3.2.1.5 | **Métodos Comerciais Peugeot** | **Dia System** | `d38bc6fc583d` |
| `CRM_Tool_GeraProcEstatistic.exe` | 7,2 | 2024-10-11 | 3.2.1.5 | Métodos Comerciais Peugeot | Dia System | `4d0cba389c0d` |
| `CRM_Tool_AjustProcStatistica.exe` | 7,1 | 2023-08-01 | 3.2.1.5 | Métodos Comerciais Peugeot | Dia System | `b6d58cb8b221` |

⚠️ **Duas gerações inteiras de runtime obsoleto guardadas dentro do `TDev`:**

| Subpasta | Conteúdo | Datas |
|---|---|---|
| `TDev\TD21` | 30 arquivos — Centura Team Developer **2.1-PTF4**, Centura SQLBase 7.6.1, Centura Quick Report | **2001–2003** |
| `TDev\ReportBuilder 11` | 16 arquivos — Centura Team Developer **1.1.2-PTF12** e **1.1.0-PTF2**, Centura Ranger Replication | **1996–2003** |

O arquivo mais antigo de toda a instalação é `ReportBuilder 11\repc32.dll`, de **27/11/1996** — 30
anos. Somando com o `TDBin`, são **57 binários de marca Centura** ainda distribuídos com o produto.
Como são subpastas (e não o `TDev` raiz, que está no `PATH`), não são carregados por acidente — mas
seguem sendo superfície de ataque e volume de manutenção.

Sem VersionInfo (14 — os 28 do total menos os 14 da tabela acima): `CRM_NotaFiscal.exe`, `CRM_NotaFiscalVeic.exe`, `CRM_Pedido.exe`,
`CRM_PedidoConsulta.exe`, `GES_Seguranca.exe`, `IVOrdemServico.exe`, `Teste_Conexao_CnxString.exe`,
`vCRM_AjustaEmail.exe`, `vCRM_Cobranca.exe`, `vCRM_DocClear.exe`, `vCRM_QlikImport.exe`,
`vCRM_RunImportIMP.exe`, `vCRM_RunMovOUT.exe`, `Vortico Call Center.EXE`.

Três observações que saem do inventário:

1. ⚠️ **`vCRM_Super.exe` e `vCRM_Super_OLD.exe` têm o mesmo SHA256.** O "backup" é bit a bit o
   arquivo atual — não serve de rollback. Alguém copiou o novo por cima dos dois.
2. ⚠️ **`CRM_Tool_*` carrega a marca de outro produto:** "Métodos Comerciais Peugeot / Dia System",
   versão 3.2.1.5. É componente reaproveitado de outro cliente da Vórtice, empacotado junto.
3. ⚠️ **Só um programa está publicado como RemoteApp.** `TSAppAllowList` tem uma única entrada:
   alias `vCRMAtendente`, `Name` = `CRM`, apontando `C:\Vortice\TDev\App\vCRM Atendente.exe`,
   `ShortPath` = `VCRMAT~1.EXE`. 📄 O servidor antigo tinha **13 aplicativos** publicados (CRM,
   GES_Seguranca, Bi Integrador, TOTVS, Linx, BI, GLPI…). Ou o restante ainda não foi migrado, ou
   deixou de ser oferecido. `fDisabledAllowList = 0` — a lista está ativa, como deve ser.

**As três DLLs do roteador ODBC** — a causa raiz do erro 401 documentado na memória do projeto —
✅ **estão presentes** em `TDev\App`, com os tamanhos esperados:

| DLL | Bytes | Versão |
|---|---:|---|
| `Sqlodb32.dll` | 332.792 | 7.3.4 Build 64934 |
| `gptdodbu32.dll` | 300.536 | 7.3.4 Build 64934 |
| `odbsal32.dll` | 17.400 | 7.3.4 Build 64934 |

### 5.2 `C:\Vortice\TDBin` — o runtime Gupta

200 arquivos, 252 MB. 109 nativos, 35 assemblies .NET, 14 configs.

Núcleo (todos 7.3.4 Build 64934, OpenText, 21/01/2021, I386):
`cdlli73.dll` (8,9 MB — o interpretador), `rdwi73.dll` (report writer), `vti73.dll`, `qfrmi73.dll`,
`geei73.dll`, `cbawi73.dll`, `srvci73.dll`, `tabli73.dll`, `gtlsi73.dll`, `maili73.dll`,
`GptPrdLms73.dll` (licenciamento).

Roteadores de banco (7.3.4): `Sqlodb32.dll` (ODBC), `Sqlora32.dll` (Oracle), `Sqlsyb32.dll`
(Sybase), `Sqlifx32.dll` (Informix) — o Vórtice suporta os quatro; aqui só o ODBC está ativo
(`comdll=sqlodb32` no `SQL.INI`).

⚠️ **Runtime legado convivendo na mesma pasta:** **11 DLLs `*i21.dll` da Centura Team Developer 2.1
e 2.1-PTF4**, datadas de **2001 e 2003** — `cdlli21.dll` (2,6 MB), `vti21.dll`, `tabli21.dll`,
`srvci21.dll`, `gctli21.dll`, `gobji21.dll`, `csi21.dll`, `snumi21.dll`, `ssti21.dll`,
`strci21.dll`, `gtlsi21.dll`. São binários de 23 a 25 anos **na pasta que está no topo do `PATH` do
sistema**. Não há como saber por metadados se algum executável ainda os carrega.

Bibliotecas de terceiros embutidas no runtime: `PDFNetC.dll` 7.1.0 (PDFTron, 33 MB),
`ProfUIS288u*.dll` 2.8.9 (Prof-UIS), `icudt58.dll` (ICU 58), OpenSSL, libxml2, Apache Axis2/C
(12 arquivos — cliente SOAP nativo).

**Assemblies .NET dentro do runtime Gupta (35):** a ponte `Gupta.TD.Runtime.*` 7.3.4 (System,
Database, Wpf.Emulation, Wpf.Mail, Wcf.Win32, GraphicsServer), `SalCDK.dll`, mais **Telerik UI for
WPF 2014.1.331.45** (8 assemblies), **Actipro** 14.2.610, **Syncfusion XlsIO** 14.1450 (Excel),
**MailKit/MimeKit 2.5.0**, **Newtonsoft.Json 12.0**, **BouncyCastle 1.8.5**,
**Oracle.ManagedDataAccess 4.122.19.1**, **Sybase.AdoNet4.AseClient 16.0**.

⚠️ Telerik de **2014** e Newtonsoft.Json **12** (2018) são componentes com mais de uma década sem
atualização, carregados dentro do processo do cliente.

### 5.3 `C:\Vortice\NT` — a camada .NET (27 executáveis + 275 assemblies)

513 arquivos, 1,3 GB (dos quais 929 MB são 142 `.db` em `NT\Databases` — bases locais/cache).
**Toda a camada é .NET Framework**, nada de .NET Core/5+.

| Executável | KB | Modificado | AssemblyVersion | TFM | SHA256 (12) |
|---|---:|---|---|---|---|
| `Vortico Server.exe` | 890 | 2025-05-08 | 4.4.1.0 | v4.8 | `bfc32934b60f` |
| `Vortico Admin.exe` | 1.552 | 2025-05-08 | 4.4.1.0 | v4.8 | `653daeda9bc4` |
| `Jupiter Service.exe` | 628 | 2025-05-08 | 4.4.1.0 | v4.8 | `ecd5c9571664` |
| `AppMonitorService.exe` | 10 | 2025-05-08 | 1.0.0.0 | v4.8 | `9bd3183bc9a2` |
| `vCtiConnect.exe` | 3.209 | 2025-05-08 | 4.4.1.0 | v4.8 | `939d851a3037` |
| `Bi Integrador.exe` | 268 | 2025-05-08 | 4.4.1.0 | v4.8 | `b810d6cbd88b` |
| `Bi Integrador SaaS.exe` | 282 | 2025-05-08 | 4.4.1.0 | v4.8 | `e22d10a87aa8` |
| `Bi Control.exe` | 588 | 2025-05-08 | 4.4.1.0 | v4.8 | `59f6a7f8c73f` |
| `Integração Google.exe` | 736 | 2025-05-08 | 4.4.1.0 | v4.8 | `c16990812f91` |
| `Card Print.exe` | 772 | 2025-05-08 | 4.4.1.0 | v4.8 | `fe20a5053f1c` |
| `Datacard SD260 Leitor.exe` | 746 | 2025-05-08 | 4.4.1.0 | v4.8 | `19d667c08969` |
| `Importador Manual SAP.exe` | 113 | 2025-05-08 | 4.4.1.0 | v4.8 | `38907426ed1b` |
| `SQL net.exe` | 230 | 2025-05-08 | 4.4.1.0 | v4.8 | `109b44931ee7` |
| `Config Con.exe` | 26 | 2025-05-08 | 4.4.1.0 | v4.8 | `ea79219719ec` |
| `Smtp Tester.exe` | 16 | 2025-05-08 | 1.0.0.0 | v4.8 | `2b5760c13161` |
| `Database Check.exe` | 24 | 2025-05-08 | 4.4.1.0 | v4.8 | `f73bdb9aade9` |
| `Log Viewer.exe` | 30 | 2025-05-08 | 4.4.1.0 | v4.8 | `befd177af2b6` |
| `Mix Produto.exe` | 55 | 2025-05-08 | 4.4.1.0 | v4.8 | `3e11ecbef0fa` |
| `ObjetoDynTester.exe` | 13 | 2025-05-08 | 4.4.1.0 | v4.8 | `4080ec875c60` |
| `Pedido - Adm.exe` | 724 | 2022-10-29 | 4.2.0.0 | v4.8 | `3b21312f4125` |
| `Pedido.exe` | 420 | 2022-10-29 | 4.2.0.0 | v4.8 | `0ae1fd1fa93b` |
| `DW Admin.exe` | 559 | 2022-01-14 | 4.1.0.0 | v4.8 | `7a97bbd8513a` |
| `DW Config.exe` | 60 | 2022-01-14 | 4.1.0.0 | v4.8 | `3eeae272e8b1` |
| `Gerador de Chaves - Juno.exe` | 40 | 2022-01-14 | 4.1.0.0 | v4.8 | `eb3376c4a989` |
| `JupiterTester.exe` | 110 | 2022-01-14 | 1.0.0.0 | v4.8 | `950c45fde3df` |
| `JupiterAnexoTester.exe` | 20 | 2025-05-08 | 1.0.0.0 | v4.8 | `c475ca8d05e5` |
| `Event Source.exe` | 12 | 2022-01-04 | 1.0.0.0 | v4.5 | `7af661f866b3` |

#### A arquitetura interna, lida pelas referências de assembly

O Vórtice organiza sua camada .NET em cinco bibliotecas próprias, todas versão 4.4.1.0 / v4.8:

| Assembly | Tipos públicos | Referências | Papel deduzido |
|---|---:|---:|---|
| `Lib.Ent.dll` | 523 | 5 | **Entidades** — só depende do BCL |
| `Lib.Dal.dll` | 506 | 7 | **Acesso a dados** |
| `Lib.Bll.dll` | 507 | 11 | **Regras de negócio** |
| `Lib.Base.dll` / `Lib.Base_WF.dll` | 37 / 77 | 12 / 11 | Infraestrutura e base de UI (WinForms) |
| `Lib.Integracoes.dll` | **755** | **36** | **Integrações externas** — a maior e mais acoplada |

> 🔍 **O achado mais relevante desta seção:** `Lib.Dal.dll` referencia **apenas** `System.Data` e
> `System.Data.OracleClient`. **Não há ORM em lugar nenhum** — nem Entity Framework, nem NHibernate,
> nem Dapper. O acesso a dados é ADO.NET puro, com SQL montado em código. Isso explica as 767
> tabelas com só 85 procedures, e confirma que a lógica está espalhada entre o p-code Gupta e essas
> 506 classes de DAL.

**O que `Lib.Integracoes.dll` (755 tipos, 36 referências) conecta:**

| Referência | O que revela |
|---|---|
| `sapnco 3.1.0.42` | **SAP .NET Connector** — integração nativa com SAP |
| `Microsoft.Exchange.WebServices 15.0` | Exchange/Outlook via EWS |
| `MailKit` / `MimeKit` **4.8.0** | SMTP/IMAP moderno |
| `OpenPop 2.0.6` | POP3 (biblioteca abandonada desde 2013) |
| `Microsoft.Identity.Client 4.65` (MSAL) | **OAuth2 / Entra ID** — a biblioteca para Modern Auth **já está lá** |
| `Google.Apis.Calendar.v3 1.68` | Google Agenda |
| `AWSSDK.SimpleEmail 3.1` | Amazon SES |
| `Renci.SshNet 2016.0` | SFTP/SSH |
| `Telerik.Reporting 14.0.20.219` | Motor de relatórios |
| `FargoNetAPI 2.0.0.10` | Impressora de cartões HID Fargo |
| `Newtonsoft.Json 13.0` | JSON |

> ⚠️ **Consequência prática imediata:** o `PENDENCIAS.md` lista como "solução durável" pedir ao
> fornecedor a migração do envio de e-mail para OAuth2/Modern Auth, para parar de depender de senha
> de aplicativo. O inventário mostra que **MSAL 4.65 e MailKit 4.8 já estão instalados** em
> `Lib.Integracoes`. A capacidade existe no produto; o que falta é ativá-la/configurá-la — o que
> muda a natureza do pedido ao fornecedor de "desenvolver" para "habilitar".

**O que `Vortico Server.exe` (80 tipos, 25 refs) acrescenta:** `HtmlAgilityPack 1.11.67`,
`Ionic.Zip`, `Tamir.SharpSSH`, **`WebPush 1.0.11`** (notificações push web), Google Calendar.

**O que `Jupiter Service.exe` (94 tipos, 18 refs) acrescenta:** `System.Web.Http 5.2.6`
(**ASP.NET Web API self-hosted**) e `System.ServiceModel.Web` — o Jupiter expõe HTTP próprio,
declarado em `URLJupiter = http://localhost:8083/`.

Outras bibliotecas presentes em `NT`: **Telerik UI for WinForms** (47 referências — a UI dominante),
**Qlik.Engine / Qlik.Sense.JsonRpc** (BI), `Microsoft.AnalysisServices.AdomdClient` (cubos SSAS),
`AForge.Video` (captura de webcam — provavelmente a foto/biometria do fluxo de cartão).

⚠️ Também há **42 arquivos `.pdb`** e 14 `.iobj`/`.ipdb` (símbolos de depuração e artefatos de
compilação incremental, 47 MB) entregues junto com o produto em produção.

### 5.4 `C:\Vortice\Install` — o kit de instalação

8 arquivos, 154,7 MB — o pacote que instala o cliente numa máquina nova:

| Arquivo | MB | Data | O que é |
|---|---:|---|---|
| `TDdeploy734x86.exe` | 132,1 | 2021-03-09 | **Runtime Gupta Team Developer 7.3.4 (x86)** |
| `FCXsetupPro.msi` + `FCXsetup.exe` | 8,0 | 2017-02-10 | componente FCX |
| `Mtbl_redist_x86.exe` | 6,2 | 2016-04-21 | VC++ 2013 Redistributable (x86) + M!Table |
| `vcredist_x86.exe` / `_2.exe` | 8,3 | 2016 | VC++ 2008 Redistributable (x86) |
| `silent.ini`, `setup.log` | — | 2021 / 2023 | configuração e log da instalação silenciosa |

📄 Confirma o que os runbooks dizem: o cliente é **32 bits** e depende dos redistribuíveis VC++.

### 5.5 `\\agrocrm\Vortice_Atualiza` — o share de auto-atualização

222 arquivos, 290,9 MB. É a cópia-mestre de onde o `vaReplace` deveria distribuir atualizações.

⚠️ **Este share está desatualizado em relação ao terminal server.** Comparando os 26 executáveis
que existem nos dois lados por SHA256:

| Executável | No terminal server | No share | Iguais? |
|---|---|---|---|
| `vCRM Atendente.exe` | 2026-03-17 | **2025-08-05** | ❌ |
| `vCRM_Admin.exe` | 2026-06-15 | **2025-03-31** | ❌ |
| `vCRM_Config.exe` | 2026-07-10 | **2025-04-03** | ❌ |
| `GES_Seguranca.exe` | 2025-10-29 | **2025-04-09** | ❌ |
| `vCRM_Cobranca.exe` | 2025-05-15 | **2025-03-14** | ❌ |
| `vCRM_Higienizador.exe` | 2025-08-13 | **2025-03-14** | ❌ |
| `vWorkflow Modeler.exe` | 2025-11-11 | **2025-06-30** | ❌ |
| (outros 19) | — | — | ✅ idênticos |

Só no terminal server: `CRM_Tool_GeraProcEstatistica.exe`, `Teste_Conexao_CnxString.exe`,
`vCRM_Super_OLD.exe`, `vWorkflow Modeler_old.exe`.
Só no share: `InterVISION_Titulo.exe`, `vCRM_Config_old.exe`.

Os `.cll` de licença no share são de **27/07/2026**; os do terminal server, de **06/08/2026**.

> 🔴 **Se a auto-atualização fosse religada apontando para este share, ela faria *downgrade* do
> cliente principal em sete meses.** O share precisa ser ressincronizado a partir do terminal server
> **antes** de virar origem de atualização — e não o contrário.

---

## 6. Configuração (redigida)

Texto completo, com segredos mascarados, em [`configs-ts172-redigido.md`](configs-ts172-redigido.md).
54 arquivos de configuração foram localizados; 21 foram lidos integralmente.

### 6.1 A cadeia de conexão do cliente desktop

| Arquivo | Chaves relevantes | Valor |
|---|---|---|
| `TDev\SQL.ini` | `[win32client] clientname` | `COLWCRMBD` |
| | `clientruntimedir` | `"C:\Vortice\TDBin"` |
| | `[win32client.dll] comdll` | `sqlodb32` (Oracle/Sybase/Informix comentados) |
| | `[odbcrtr] remotedbname` | `CRM,DSN=CRM` |
| | `odbctrace` / `odbctracefile` | **`off`** / `sql.log` |
| | `longbuffer` | `1600000` |
| | `EnableMultipleConnections` | `off` |
| `TDBin\sql.ini` | idêntico ao acima | (mesmo conteúdo, 659 bytes) |
| `TDev\Vortice_Config.INI` | `[GLOBAL] defdbname` / `defDBUser` / `defDBPw` | `CRM` / `crm` / `***REDIGIDO***` |
| | `DefDBOwner` / `DefDBVersion` | `dbo` / `100` |
| | `DefUSerConnect` | `VRTCSERVER/***REDIGIDO***` |
| | `[ServProcesso] Servidor` / `LoopSyncQtde` | `1` / `100` |
| | seções `[RunImportIMP]`, `[RunMovOut]`, `[RunGEPImport]` | cada uma com seu `defuserconnect` |
| `NT\Config\app.xml` | `UsrLogin` / `PasLogin` | `VRTCSERVER` / `[DPT]***REDIGIDO***[/DPT]` |
| | `Sessao alias="CRM"` `StrCon` | `server=COLWCRM; database=CRM; UID=crm; PWD=***REDIGIDO***; Trusted_Connection=False;` |
| | `QueryTimeOut` / `MaxParallelism` | `600` / `10` |

✅ Confirmado o DSN: registro `HKLM\SOFTWARE\WOW6432Node\ODBC\ODBC.INI\CRM`, driver
`C:\WINDOWS\system32\SQLSRV32.dll`, `Server = COLWCRM.coloradomaq.com.br`, **sem** `Database`
(quem informa é o `remotedbname` do `SQL.INI`). ✅ Não existe nenhum DSN de 64 bits.

⚠️ **Observações de configuração:**

- A senha do banco viaja **em texto claro** em `Vortice_Config.INI`, `vaCONFIG.ini` e `app.xml` —
  três arquivos, num diretório legível por qualquer usuário do terminal server.
- `Trusted_Connection=False`: autenticação SQL, não Windows. Um único login `crm` compartilhado por
  todos os usuários — por isso qualquer trace precisa filtrar por `client_hostname`, não por login.
- `DefDBVersion=100` bate com o `compatibility_level = 100` das bases `CRM` e `CRM_HOMO` — ou seja,
  **as bases rodam em modo de compatibilidade SQL Server 2008** dentro de um SQL Server 2019.
- `app.xml` referencia uma sessão `crmteste` apontando para a base `CRM_TESTE`, que **não existe** no
  servidor (só `CRM` e `CRM_HOMO`). Configuração órfã.
- `vaCONFIG.ini` (2022) tem seções mortas de outros ambientes: `[TREINO]` (base `treino`,
  inexistente), `[SISTRE]` e `[SISDIAXXX]` com credenciais **Informix**, e `[V6]` apontando
  `defdbServer = 192.1.7.xx`.

### 6.2 Integrações e URLs externas (`NT\Vortico Server.exe.config`)

Este é o arquivo mais revelador da instalação. 20 endpoints WCF declarados:

| Endpoint | Endereço | TLS? |
|---|---|---|
| `PODataService_Version1_1Impl` | `https://jdquote2.deere.com/services/...` | ✅ |
| `MaintainQuote_Version6_6Impl` | `https://jdquote2.deere.com/services/...` | ✅ |
| `wsVorticeSoap` | `http://intranet.vortice.inf.br:8082/wsVortice/wsVortice.asmx` | ❌ |
| `wsVorticeAdmSoap` / `Soap12` | `http://189.2.151.226:8082/wsVorticeAdm/wsVorticeAdm.asmx` | ❌ |
| `Sms` (Zenvia) | `https://api-http.zenvia.com/GatewayIntegration/services/Sms` | ✅ |
| `ReluzCap Web ServiceSoap` | `https://webservices2.twwwireless.com.br/reluzcap/...` | ✅ |
| `MPGatewaySoap` | `http://www.mpgateway.com/v_3_00/sms/service.asmx` | ❌ |
| `ReceiveMessagePort` | `http://www.facilitamovel.com.br:80/ReceiveMessage` | ❌ |
| `InventoryPort` | `http://api.recuperemais.com.br/sms` | ❌ |
| `FinasaWSService` | `http://webservice.finasa.com.br/financiamentoAutomatico/...` | ❌ |
| `CartaoSoap` / `DadosCadastraisSoap` / `CreditoSoap` | `http://10.19.254.xx:30047/*.asmx` | ❌ |
| `CreditoSoapAlternativo` | `http://10.19.252.xx:30000/Credito.asmx` | ❌ |
| `serverUsbiPort` | `http://usbi.autoavaliar.com.br/webServerUsbi.php` | ❌ |
| `Venda2HttpSoap11Endpoint` | `https://fisystem.com.br/services/.../Venda2...` | ✅ |
| `WorkflowWebServiceSoap12` | `https://dr-savegnago-prd.neurotech.com.br/services/soap/porting` | ✅ |
| `ISOServicePort` | `https://savegnagoprivate.connect.dock.tech/ISOService` | ✅ |

E em `applicationSettings`:

| Chave | Valor |
|---|---|
| `URLJupiter` | `http://localhost:8083/` |
| `UrlGTRest` | `https://web.vortice.inf.br:15443/vorticehub/wsVorticeCrmApi/api/vorticeadmin/` |
| `MonitServProc` | `True` |
| `GT_GMT` | `0` |

⚠️ **Duas leituras importantes:**

1. **A maior parte desses endpoints não é da Tracbel.** `dr-savegnago-prd.neurotech.com.br` e
   `savegnagoprivate.connect.dock.tech` são de **outro cliente da Vórtice** (rede Savegnago);
   Finasa, ReluzCap, Autoavaliar e as redes `10.19.x.x` idem. É o `.config` genérico que a Vórtice
   entrega a todos os clientes, com todas as integrações que o produto já implementou. **Isso não é
   configuração — é catálogo.** Só `jdquote2.deere.com` (John Deere — cotações e pedidos) tem
   relação óbvia com o negócio da Tracbel Agro. **Confirmar com o fornecedor quais estão realmente
   ativos** antes de mapear integrações no CRM novo.
2. **Doze dos vinte endpoints são `http://` sem TLS** (8 são `https://`), incluindo o canal com a
   própria Vórtice (`intranet.vortice.inf.br:8082` e `189.2.151.226:8082`).

Há ainda proxy declarado apontando para `http://192.168.1.xx:3128` e um `wpad.dat` em
`http://192.168.1.xx:65480` — endereços de rede que não existem neste ambiente.

### 6.3 Configurações do banco com apontamento de rede

| Sistema | Módulo | Parâmetro | Valor | Alterado em |
|---|---|---|---|---|
| CRM | DOCMANAGER | `FTPWebSrv` | `10.150.14.65` | 20/07/2026 |
| CRM | MESSCENTER | `SMSURL` | `https://api-sp3.infobip.com/sms/1/text/single` | 31/05/2019 |
| CRM | OUT | `OutParLinkBaseAgenda` | 🔴 `http://192.168.109.209/CRMWeb/Forms/frmAndamento.aspx?p=` | 08/05/2025 |
| CRM | OUT | `OutParLinkBasePessoa` | 🔴 `http://192.168.109.209/CRMWeb/Forms/frmPessoa.aspx?p=` | 08/05/2025 |

---

## 7. Apontamentos mortos — lista consolidada

Varredura de todos os `.ini`, `.config`, `.xml`, `.bat` e `.ORA` do `C:\Vortice`, mais os
parâmetros do banco.

| # | Onde | Aponta para | Situação | Impacto |
|---|---|---|---|---|
| 1 | `GE_ParametroGlobal` CRM/OUT `OutParLinkBaseAgenda` | `192.168.109.209` | 🔴 host morto | Todo link de agenda enviado por e-mail pelo CRM está quebrado |
| 2 | `GE_ParametroGlobal` CRM/OUT `OutParLinkBasePessoa` | `192.168.109.209` | 🔴 host morto | Idem, para links de pessoa |
| 3–6 | `TDev\utl\vaReplace.ini` blocos 0–3 | `\\192.168.109.xx\Vortice_Atualiza\TDev{,\App,\UTL,\QRP}` | 🔴 share morto | **Auto-atualização inoperante** em todos os clientes |
| 7 | `NT\Vortico Server.exe.config` `<proxy proxyaddress>` | `http://192.168.1.xx:3128` | 🔴 rede inexistente | Proxy configurado mas inalcançável |
| 8 | `NT\Vortico Server.exe.config` `<proxy scriptLocation>` | `http://192.168.1.xx:65480/wpad.dat` | 🔴 rede inexistente | Idem |
| 9 | `NT\vCtiConnect.bat` | `#SERVER=192.168.1.xx#PORT=8000` | 🔴 rede inexistente | Conector de telefonia (XCONTACT) aponta para lugar nenhum |
| 10 | `NT\Config\app.xml` sessão `crmteste` | base `CRM_TESTE` | 🟡 base inexistente | Sessão órfã |
| 11 | `TDev\vaCONFIG.ini` `[TREINO]` | base `treino` | 🟡 base inexistente | Seção órfã |
| 12 | `TDev\vaCONFIG.ini` `[V6]` | `defdbServer = 192.1.7.xx` | 🟡 host de outro ambiente | Seção órfã |
| 13 | `TDev\vaCONFIG.ini` `[SISTRE]` / `[SISDIAXXX]` | bases Informix `sistre` / `sisdia` | 🟡 outro produto | Credenciais Informix de outro cliente |
| 14 | `NT\Vortico Server.exe.config` (14 endpoints) | Savegnago, Finasa, ReluzCap, `10.19.x.x`… | 🟡 outro cliente | Catálogo genérico do fornecedor |
| 15 | `\\agrocrm\Vortice_Atualiza` | conteúdo 7 meses atrasado | 🔴 **armadilha** | Se religado, faz downgrade do cliente |

📄 Os itens 1–6 já constavam de `PENDENCIAS.md`; ✅ os itens 7–15 são novos desta coleta.

---

## 8. Superfície de rede e segurança

### 8.1 O que está exposto por host

| Host | Portas abertas | Sem TLS |
|---|---|---|
| COLWCRM 10.150.14.65 | 21, 80, 135, 139, 445, 1433, 3389, 5985 | **FTP 21 · HTTP 80 · SQL 1433 · WinRM 5985** — sem 443 |
| agrocrm 10.150.6.230 | 80, 135, 139, **443**, 445, 3389, 5985, 8080 | HTTP 80 · HTTP 8080 · WinRM 5985 |
| COLWRDP01 10.150.5.54 | 135, 139, 445, 3389, 5985 | WinRM 5985 |
| ECS-ST-TSPLUS 10.150.5.172 | 135, 139, 445, 3389, 5985, **5986** | WinRM 5985 (5986 disponível) |

**Certificado HTTPS (agrocrm:443)** — ✅ existe e é válido:

```
Subject   : CN=*.tracbel.com.br, O=Tracbel SA, S=Minas Gerais, C=BR
Emissor   : CN=Sectigo Public Server Authentication CA OV R36, O=Sectigo Limited, C=GB
Validade  : 09/11/2025 -> 10/12/2026
Protocolo : TLS 1.2
Thumbprint: 2D94E0D2DA87AB722B9D2F5AEC5AF52B7D461087
```

`https://agrocrm.tracbel.com.br/CRMWeb/` e `.../wsVorticeCrmApi/` respondem **200 com certificado
válido**. Ou seja: **o HTTPS já funciona e não está sendo usado** — o CRMWeb e a API continuam
publicados em `http://` puro, e é por `http://` que os parâmetros do banco e as configs apontam.
Migrar é configuração, não projeto.

O COLWCRM **não tem 443 aberta** — lá o HTTPS não existe.

### 8.2 Achados de segurança

> Estes achados vieram do inventário, não de um teste de invasão. Merecem verificação e tratamento
> pela equipe de infraestrutura.

🔴 **1. Backup completo do banco CRM legível por "Todos".**
`\\10.150.6.230\bkpbd` contém `CRM_backup_2025_06_08_200006_1415257.bak` — **29,9 GB**, o dump
completo do banco de produção. A ACL NTFS da raiz do share concede `Todos = ReadAndExecute`. Qualquer conta da
rede pode copiar a base inteira: clientes, propostas, CPFs, faturamento. Foi apenas **listado**;
nada foi copiado. *(No mesmo share há `RoboCopRoboCopy.exe`, `windirstat.exe` e um `Uninstall.exe`
de 0 byte — utilitários soltos num share de backup.)*

🔴 **2. Repositório de documentos com `Todos = FullControl`.**
`\\10.150.14.65\DocManager` — o acervo do Doc Manager — tem ACL NTFS na raiz com **`Todos = FullControl`**: ler,
gravar **e apagar**. Não foi inventariado (documentos de clientes, LGPD); só se verificou a ACL e a
existência da pasta `000000`.

🔴 **3. Doc Manager trafega por FTP puro, sem TLS.**
Porta 21 aberta no COLWCRM, banner `220-FileZilla Server 1.3.0`. 📄 Segundo a memória do projeto o
Doc Manager usa FTP em **modo ativo, sem TLS** — credencial e documentos em texto claro na rede.
A porta 990 (FTPS) não está aberta.

🟡 **4. Senha do banco em texto claro em três arquivos de configuração**, num diretório legível por
qualquer usuário do terminal server (`Vortice_Config.INI`, `vaCONFIG.ini`, `app.xml`). Todos os
usuários compartilham o mesmo login SQL `crm`, o que também elimina rastreabilidade por usuário no
banco.

🟡 **5. A API é servida em HTTP puro embora o HTTPS esteja pronto.**
`POST /api/crm/login` e `/api/cartao/auth` trafegam credenciais. O certificado wildcard válido já
está instalado no mesmo IIS.

🟡 **6. O spec Swagger não declara nenhum `securityDefinitions`** — mas ✅ a API **de fato exige
autenticação**: `/api/crm/empresas`, `/api/crm/cidade/MG` e `/api/crm/paramlista` devolveram **401**
sem credencial. O problema é de documentação, não de controle de acesso.

🟡 **7. Símbolos de depuração em produção.** 42 `.pdb` e 14 `.iobj`/`.ipdb` em `C:\Vortice\NT`
(47 MB). Facilitam a análise dos assemblies por terceiros.

🟡 **8. Componentes de terceiros muito desatualizados** carregados no processo do cliente: Telerik
2014.1, Newtonsoft.Json 12 (2018), OpenPop 2.0.6 (abandonada em 2013), OpenSSL e ICU 58 embutidos no
runtime Gupta de 2021, e **57 binários Centura de 1996 a 2003** — 11 deles no `TDBin`, que está no
topo do `PATH` do sistema.

🟢 **9. O que está bem.** `fDisabledAllowList = 0` (lista de RemoteApp ativa, sem o atalho perigoso);
o terminal server novo tem **Bitdefender Endpoint Security**, **Wazuh Agent 4.14.5** e **Zabbix
Agent 6.0.47** — antivírus, SIEM e monitoramento; e o disco está saudável (153,5 GB livres de 199,8,
76,8%), sem repetir o problema crônico do servidor antigo.

---

## 9. A API `wsVorticeCrmApi`

Detalhamento completo em [`API-endpoints-completo.md`](API-endpoints-completo.md); spec bruto em
`swagger-wsVorticeCrmApi.json`.

| | |
|---|---|
| Spec | `http://10.150.6.230/wsVorticeCrmApi/swagger/docs/v2` (Swagger 2.0, Swashbuckle) |
| ⚠️ Caminho | `/swagger/docs/**v2**` — `docs/v1`, `docs/v3` e `/swagger/v1/swagger.json` dão **404** |
| Versão | `Documentação API 4.04.01-3` (bate com `GET /api/ApiInfo`) |
| Tamanho | 138.399 bytes |
| Rotas / operações / modelos | **112 / 116 / 91** |
| `securityDefinitions` | **nenhuma declarada** (mas a API exige auth — ver §8.2 item 6) |
| Plataforma | IIS 10.0 · ASP.NET 4.0.30319 · `X-Powered-By: ASP.NET` |

**Distribuição por família:**

| Família | Ops | | Família | Ops |
|---|---:|---|---|---:|
| cartão | 26 | | followize | 4 |
| crm (núcleo) | 21 | | vtcsaas | 3 |
| crm/imp (importação ERP) | 18 | | pabx | 3 |
| portal | 15 | | webhook | 3 |
| certiface | 6 | | chat · infobip · rd | 2 cada |
| acordo | 4 | | ApiInfo · ativmob · facebook · mercanet · neurotech · vorticeadmin · wifire | 1 cada |

### 9.1 Diff contra `vortice-crm-agent/docs/API-wsVorticeCrmApi.md`

A comparação mecânica acusa **22 rotas no Swagger ausentes da doc** e **4 rotas na doc ausentes do
Swagger**. Lendo caso a caso:

- **A maioria das 22 é falso positivo de formatação.** A doc existente comprime variantes numa
  linha só (`/api/cartao/certiface/challenge` · `/captcha`), e o comparador não desdobra. Os casos
  são reais mas já estavam descritos.
- **Ausências que parecem genuínas** e valem revisar na doc:
  `GET /api/cartao/promotor`, `GET /api/cartao/promotores/{psSearchText}`,
  `POST /api/certiface/facecaptcha/{appkey,captcha,result}`,
  `POST /api/vtcsaas/{evento,pesquisa}`, `POST /api/portal/senharecupera`,
  `POST /api/portal/signup`, `GET /api/portal/processo/{processo}/documento`.
- **As 4 "ausentes do Swagger"** são citações de caminho parcial na prosa da doc, não rotas.

**Conclusão:** a doc do agente está substancialmente correta. Vale complementá-la com as rotas
acima e trocar a referência de `/swagger/docs/v1` para **`/swagger/docs/v2`**.

---

## 10. Serviços, IIS e tarefas — o que ficou sem cobertura

| Item | Host | Situação |
|---|---|---|
| Serviços Windows | ECS-ST-TSPLUS | ✅ inventariados — **nenhum serviço `Vortice`/`Vtc`**, o que é correto: é terminal server, não app server |
| Serviços Windows | agrocrm, COLWCRM, COLWRDP01 | ❌ **não alcançável** (WMI e WinRM negados) |
| `VtcAppMonitorService` | agrocrm | ❌ **não verificável** — 📄 os runbooks dizem que roda ali, com binário `C:\Vortice\NT\AppMonitorService.exe`; ✅ o binário existe (10 KB, .NET v4.8, `9bd3183bc9a2`) na cópia do terminal server |
| Sites e app pools IIS | agrocrm, COLWCRM | ❌ **não alcançável** — `applicationHost.config` exige SMB/WinRM admin. Só se conhece o que os cabeçalhos HTTP revelam (IIS 10.0, ASP.NET 4.0.30319) |
| `web.config` do CRMWeb e da API | agrocrm, COLWCRM | ❌ **não alcançável** — o `scan-webconfig-ip-antigo.ps1` precisa rodar **no** servidor |
| Tarefas agendadas | todos | ❌ **não alcançável** |
| DSNs ODBC | ECS-ST-TSPLUS | ✅ inventariados (§6.1) |
| DSNs ODBC | demais hosts | ❌ **não alcançável** |
| Certificados | agrocrm | ✅ certificado de servidor lido no handshake TLS |
| .NET Framework instalado | ECS-ST-TSPLUS | ✅ **4.8.09221** (release 533509), única versão |
| .NET Framework instalado | demais hosts | ❌ **não alcançável** — 📄 inferido `4.x` pelo `X-AspNet-Version: 4.0.30319` e pelo TFM v4.8 dos assemblies |

### Como fechar essas lacunas

Há um script pronto para isso: **`scripts\vortice-extracao\binarios-05-coleta-local.ps1`**.
Copie-o para o servidor e execute **dentro de uma sessão RDP, como Administrador**:

```powershell
powershell -ExecutionPolicy Bypass -File .\binarios-05-coleta-local.ps1
```

Ele é somente leitura — não altera, para, cria nem apaga nada — e gera um `host-<NOME>.md` com
13 seções no mesmo formato do [`host-ECS-ST-TSPLUS.md`](host-ECS-ST-TSPLUS.md): sistema, volumes,
**IIS (sites, bindings, app pools com CLR/pipeline/identidade)**, **`web.config` redigidos** com
varredura de apontamentos de rede, serviços, **tarefas agendadas**, **DSNs ODBC 32 e 64 bits**,
.NET Framework, **certificados e bindings HTTPS**, `PATH`, **compartilhamentos com ACL**,
interfaces de rede e portas em escuta.

Aplica a mesma redação deste relatório (senhas, tokens, blobs `[DPT]`, seriais e último octeto de
IP) e roda uma auditoria de vazamento antes de entregar o arquivo — o console avisa se algo passar.
Basta copiar o `.md` de volta para esta pasta; nada mais precisa sair do servidor.

✅ Validado: executado ponta a ponta em 02/09/2026, sem erros e sem segredos na saída.

<details>
<summary>Equivalente manual, se preferir não copiar o script</summary>



```powershell
# IIS
Import-Module WebAdministration
Get-Website | Select-Object Name, State, PhysicalPath, @{n='Bindings';e={($_.Bindings.Collection).bindingInformation -join '; '}}
Get-IISAppPool | Select-Object Name, ManagedRuntimeVersion, ManagedPipelineMode, State,
                              @{n='Identity';e={$_.ProcessModel.IdentityType}}

# Servicos, tarefas, ODBC, .NET, certificados
Get-CimInstance Win32_Service | Where-Object { $_.PathName -match 'Vortice|Vortico|Vtc|Jupiter' } |
    Select-Object Name, State, StartMode, StartName, PathName
Get-ScheduledTask | Where-Object { $_.TaskPath -notmatch '^\\Microsoft' } | Select-Object TaskPath, TaskName, State
Get-OdbcDsn
Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP' -Recurse |
    Get-ItemProperty -Name Version, Release -ErrorAction SilentlyContinue
Get-ChildItem Cert:\LocalMachine\My | Select-Object Subject, NotAfter, Thumbprint
```

</details>

O script já existente `vortice-crm-agent\scripts\scan-webconfig-ip-antigo.ps1` continua válido para
varrer os `web.config` — embora o `binarios-05` já faça essa varredura como parte da seção 4.

---

## 11. Os dez achados mais relevantes

| # | Achado | Por que importa |
|---|---|---|
| 1 | 🔴 **Backup completo do banco (29,9 GB) legível por "Todos"** em `\\10.150.6.230\bkpbd` | Vazamento integral da base de clientes ao alcance de qualquer conta da rede |
| 2 | 🔴 **`\\10.150.14.65\DocManager` com `Todos = FullControl`** | Documentos de clientes podem ser lidos, alterados **e apagados** por qualquer um |
| 3 | 🔍 **Não existe ORM: `Lib.Dal` usa ADO.NET puro**, e o banco tem 767 tabelas para só 85 procedures e 1 trigger | A regra de negócio está no p-code Gupta e em 506 classes de DAL. Define a estratégia de descoberta do CRM novo: capturar SQL, não ler código |
| 4 | 🔴 **O share de auto-atualização está 7 meses atrasado** — `vCRM Atendente.exe` de ago/2025 contra mar/2026 no terminal server | Religar o `vaReplace` apontando para ele faria *downgrade* de todos os clientes. Ressincronizar **antes** |
| 5 | 🔧 **MSAL 4.65 e MailKit 4.8 já estão em `Lib.Integracoes`** | O OAuth2/Modern Auth para o e-mail — hoje preso a senha de aplicativo que expira — pode ser questão de habilitar, não de desenvolver. Muda o pedido ao fornecedor |
| 6 | 🔓 **HTTPS válido e pronto no agrocrm, mas CRMWeb e API rodam em `http://`** | Certificado `*.tracbel.com.br` da Sectigo válido até dez/2026 já responde. Migrar é configuração |
| 7 | 📌 **445 e 5985 estão abertas nos quatro hosts** (a memória de 31/08 dizia fechadas) | O bloqueio é de autorização, não de rede. Muda o diagnóstico de qualquer tentativa de acesso remoto |
| 8 | 🧩 **O `Vortico Server.exe.config` é catálogo, não configuração** — 14 dos 20 endpoints são de outros clientes da Vórtice (Savegnago, Finasa, ReluzCap) | Só `jdquote2.deere.com` tem relação clara com a Tracbel. Não mapear integrações a partir deste arquivo sem confirmar com o fornecedor |
| 9 | ⚠️ **57 binários Centura de 1996 a 2003 ainda distribuídos** — 11 deles misturados ao Gupta 7.3.4 no `TDBin`, que é a primeira entrada do `PATH` | Superfície antiga dentro do processo do cliente, e imprevisibilidade de qual versão ganha. O arquivo mais velho é de 27/11/1996 |
| 10 | ⚠️ **`vCRM_Super_OLD.exe` tem o mesmo SHA256 do `vCRM_Super.exe`** | O "backup" não é backup. Vale checar o mesmo padrão nos demais `_old` antes de confiar em rollback |

**Menções honrosas:** as bases `CRM` e `CRM_HOMO` rodam em **compatibility level 100 (SQL Server
2008)** dentro de um SQL Server 2019 — otimizador antigo, ganho de desempenho na mesa; existe uma
base de homologação (`CRM_HOMO`) que pode servir de campo de provas para a captura de SQL sem tocar
em dado real; e só **1 dos 13 RemoteApp** do servidor antigo foi publicado no novo.

---

## 12. Método e limites

**O que foi feito:** varredura de portas por TCP connect; listagem de shares; leitura de arquivos
por SMB; leitura de metadados PE (`FileVersionInfo`) e de metadados CLI via
`System.Reflection.Metadata.MetadataReader`; SHA256 sobre o fluxo de leitura; consultas WMI/DCOM
(`Get-*` e `StdRegProv.EnumKey`/`GetStringValue`, todas de consulta); HTTP GET; consultas SQL
somente leitura pela conta `CRM_Leitura`.

**O que não foi feito, e por quê:**

- ❌ **Nenhum `Assembly.Load`/`LoadFrom`.** A detecção de .NET e a lista de referências vêm do
  `MetadataReader`, que lê a tabela de metadados sem executar nada.
- ❌ **Nenhuma descompilação.** Sem ILSpy, sem dnSpy — é decisão contratual, tratada em
  [PROPOSTA-ABRIR-CAIXA-PRETA.md](PROPOSTA-ABRIR-CAIXA-PRETA.md) §5.
- ❌ **Nenhum binário copiado** para a máquina local.
- ❌ **Nenhuma alteração remota.** Nada criado, parado, movido ou apagado.
- ❌ **Conteúdo do `DocManager` não inventariado** — documentos de clientes, LGPD. Só se verificou a
  ACL e a existência da pasta raiz.
- ❌ **Nenhum segredo impresso ou gravado.** Toda senha, token, chave, blob `[DPT]` e serial de
  licença aparece como `***REDIGIDO***`; o material derivado passou por auditoria automática de
  vazamento antes de ser salvo.

**Limite principal:** três dos quatro hosts vivos recusaram SMB administrativo e WinRM para a conta
usada. IIS, serviços, tarefas agendadas e `web.config` do app server e do servidor de banco
**permanecem sem cobertura** — os comandos para fechar essa lacuna, a serem executados dentro de uma
sessão RDP, estão na §10.

---

*Coletado em 02/09/2026 · scripts em `c:\projetos\tracbel-crm\scripts\vortice-extracao\`*
