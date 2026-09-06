# Host ECS-ST-TSPLUS - coleta WMI/DCOM (somente leitura)

_Coletado em 2026-09-02 23:24._

## 1. Sistema operacional

| Item | Valor |
|---|---|
| Nome | ECS-ST-TSPLUS |
| Sistema | Microsoft Windows Server 2025 Standard |
| Versao / build | 10.0.26100 / 26100 |
| Arquitetura | 64-bit |
| Instalado em | 07/07/2026 03:36:23 |
| Ultimo boot | 08/31/2026 15:40:11 |
| Dominio | tracbel.com.br |
| Fabricante / modelo | OpenStack Foundation / OpenStack Nova |
| CPUs logicas / RAM | 4 / 16 GB |

## 2. Volumes

| Unidade | Rotulo | Total GB | Livre GB | % livre |
|---|---|---:|---:|---:|
| C: |  | 199.8 | 153.5 | 76.8% |

## 3. Servicos Windows (Vortice / Vtc / SQL / IIS / FTP)

| Nome | Estado | Inicio | Conta | Binario |
|---|---|---|---|---|
| TermService | Running | Manual | NT Authority\NetworkService | `C:\WINDOWS\System32\svchost.exe -k termsvcs` |

### Todos os servicos com binario fora de C:\Windows

| Nome | Estado | Inicio | Binario |
|---|---|---|---|
| cloudbase-init | Stopped | Auto | `"C:\Program Files\Cloudbase Solutions\Cloudbase-Init\bin\OpenStackService.exe" cloudbase-init "C:\Program Files\Cloudbase Solutions\Cloudbase-Init\Python\Scripts\cloudbase-init.exe" --config-file "C:\Program Files\Cloudbase Solutions\Cloudbase-Init\conf\cloudbase-init.conf"` |
| cloudResetPwdAgent | Stopped | Auto | `C:\CloudResetPwdAgent\bin\wrapper.exe -s C:\CloudResetPwdAgent\conf\wrapper.conf` |
| edgeupdate | Stopped | Auto | `"C:\Program Files (x86)\Microsoft\EdgeUpdate\MicrosoftEdgeUpdate.exe" /svc` |
| edgeupdatem | Stopped | Manual | `"C:\Program Files (x86)\Microsoft\EdgeUpdate\MicrosoftEdgeUpdate.exe" /medsvc` |
| EPIntegrationService | Running | Auto | `"C:\Program Files\Bitdefender\Endpoint Security\EPIntegrationService.exe" /service` |
| EPProtectedService | Running | Auto | `"C:\Program Files\Bitdefender\Endpoint Security\EPProtectedService.exe" /service` |
| EPRedline | Running | Auto | `"C:\Program Files\Bitdefender\Endpoint Security\bdredline.exe"` |
| EPSecurityService | Running | Auto | `"C:\Program Files\Bitdefender\Endpoint Security\EPSecurityService.exe" /service` |
| EPUpdateService | Running | Auto | `"C:\Program Files\Bitdefender\Endpoint Security\EPUpdateService.exe" /service` |
| GoogleChromeElevationService | Stopped | Manual | `"C:\Program Files\Google\Chrome\Application\152.0.7977.65\elevation_service.exe"` |
| GoogleUpdaterInternalService152.0.7933.0 | Stopped | Auto | `"C:\Program Files (x86)\Google\GoogleUpdater\152.0.7933.0\updater.exe" --system --windows-service --service=update-internal` |
| GoogleUpdaterService152.0.7933.0 | Stopped | Auto | `"C:\Program Files (x86)\Google\GoogleUpdater\152.0.7933.0\updater.exe" --system --windows-service --service=update` |
| icagent | Running | Auto | `c:\opt\oss\servicemgr\ICAgent\bin\manual\win\bin\nssm.exe` |
| icwatchdog | Running | Auto | `c:\opt\oss\servicemgr\ICAgent\bin\manual\win\bin\nssm.exe` |
| MDCoreSvc | Running | Auto | `"C:\ProgramData\Microsoft\Windows Defender\Platform\4.18.26070.9-0\MpDefenderCoreService.exe"` |
| MicrosoftEdgeElevationService | Stopped | Manual | `"C:\Program Files (x86)\Microsoft\Edge\Application\152.0.4191.53\elevation_service.exe"` |
| Sense | Stopped | Manual | `"C:\Program Files\Windows Defender Advanced Threat Protection\MsSense.exe"` |
| vm-agent | Running | Auto | `"c:\Program Files (x86)\virtio\monitor\vm-agent.exe" -d -l "c:\Program Files (x86)\virtio\monitor\vm-agent.log"` |
| VmAgentDaemon | Running | Auto | `"c:\Program Files (x86)\virtio\monitor\vm-agent-daemon.exe" -s` |
| WazuhSvc | Running | Auto | `"C:\Program Files (x86)\ossec-agent\wazuh-agent.exe"` |
| WdNisSvc | Stopped | Manual | `"C:\ProgramData\Microsoft\Windows Defender\Platform\4.18.26070.9-0\NisSrv.exe"` |
| WinDefend | Running | Auto | `"C:\ProgramData\Microsoft\Windows Defender\Platform\4.18.26070.9-0\MsMpEng.exe"` |
| WMPNetworkSvc | Stopped | Manual | `"C:\Program Files\Windows Media Player\wmpnetwk.exe"` |
| Zabbix Agent | Running | Auto | `"C:\Program Files\Zabbix Agent\zabbix_agentd.exe" --config "C:\Program Files\Zabbix Agent\zabbix_agentd.conf"` |

## 4. .NET Framework instalado (HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP)

| Chave | Version | Release |
|---|---|---|
| v4\Client | 4.8.09221 | 533509 |
| v4\Full | 4.8.09221 | 533509 |
| v4.0\Client | 4.0.0.0 |  |

## 5. DSNs ODBC

### 64 bits

| DSN | Driver | Server | Database |
|---|---|---|---|

Drivers instalados (64 bits):

- ODBC Core
- ODBC Translators
- SQL Server

### 32 bits (WOW6432Node)

| DSN | Driver | Server | Database |
|---|---|---|---|
| CRM | C:\WINDOWS\system32\SQLSRV32.dll | COLWCRM.coloradomaq.com.br |  |

Drivers instalados (32 bits (WOW6432Node)):

- Conversor de pagina de codigo MS
- Driver da Microsoft para arquivos texto (*.txt; *.csv)
- Driver do Microsoft Access (*.mdb)
- Driver do Microsoft dBase (*.dbf)
- Driver do Microsoft Excel(*.xls)
- Driver do Microsoft Paradox (*.db )
- Microsoft Access Driver (*.mdb)
- Microsoft Access-Treiber (*.mdb)
- Microsoft dBase Driver (*.dbf)
- Microsoft dBase-Treiber (*.dbf)
- Microsoft Excel Driver (*.xls)
- Microsoft Excel-Treiber (*.xls)
- Microsoft ODBC for Oracle
- Microsoft Paradox Driver (*.db )
- Microsoft Paradox-Treiber (*.db )
- Microsoft Text Driver (*.txt; *.csv)
- Microsoft Text-Treiber (*.txt; *.csv)
- MS Code Page Translator
- MS Code Page-Ubersetzer
- ODBC Core
- ODBC Translators
- SQL Server

## 6. RemoteApp publicados (TSAppAllowList)

fDisabledAllowList = 0

| Chave (alias) | Name | Caminho | ShortPath |
|---|---|---|---|
| vCRMAtendente | CRM | `C:\Vortice\TDev\App\vCRM Atendente.exe` | C:\Vortice\TDev\App\VCRMAT~1.EXE |

## 7. PATH do sistema

```
C:\Vortice\TDBin
C:\Vortice\TDev
C:\WINDOWS\system32
C:\WINDOWS
C:\WINDOWS\System32\Wbem
C:\WINDOWS\System32\WindowsPowerShell\v1.0\
C:\WINDOWS\System32\OpenSSH\
```

## 8. Software instalado (Uninstall)

| Produto | Versao | Fabricante | Bits |
|---|---|---|---|
| Bitdefender Endpoint Security Tools | 8.26.9.664 | Bitdefender | 64 |
| Zabbix Agent (64-bit) | 6.0.47.2400 | Zabbix SIA | 64 |
| Microsoft Windows Application Compatibility Fix Database |  |  | 64 |
| UVP VMTools for Windows 2.5.0.156 | 2.5.0.156 | Redhat | 64 |
| Microsoft Visual C++ 2019 X64 Minimum Runtime - 14.27.29016 | 14.27.29016 | Microsoft Corporation | 64 |
| Cloudbase-Init 1.1.8 | 1.1.8.0 | Cloudbase Solutions Srl | 64 |
| Microsoft Visual C++ 2019 X64 Additional Runtime - 14.27.29016 | 14.27.29016 | Microsoft Corporation | 64 |
| Google Chrome | 152.0.7977.65 | Google LLC | 32 |
| Microsoft Edge | 152.0.4191.53 | Microsoft Corporation | 32 |
| Microsoft Edge WebView2 Runtime | 152.0.4191.53 | Microsoft Corporation | 32 |
| SAP Business Explorer | 7.70 | SAP SE | 32 |
| SAP GUI for Windows 7.70  (Patch 0) | 7.70 Compilation 1 | SAP SE | 32 |
| SAP Business Client 7.70 | 7.70 PL0 | SAP SE | 32 |
| Microsoft Visual C++ 2015-2019 Redistributable (x86) - 14.27.29016 | 14.27.29016.0 | Microsoft Corporation | 32 |
| Wazuh Agent | 4.14.5 | Wazuh, Inc. | 32 |
| Microsoft Visual C++ 2015-2019 Redistributable (x64) - 14.27.29016 | 14.27.29016.0 | Microsoft Corporation | 32 |
| UVP VMTools for Windows 2.5.0.156 | 2.5.0.156 | Redhat | 32 |
| Microsoft Visual C++ 2019 X86 Additional Runtime - 14.27.29016 | 14.27.29016 | Microsoft Corporation | 32 |
| Microsoft Visual C++ 2013 Redistributable (x86) - 12.0.40660 | 12.0.40660.0 | Microsoft Corporation | 32 |
| Microsoft Visual C++ 2013 x86 Minimum Runtime - 12.0.40664 | 12.0.40664 | Microsoft Corporation | 32 |
| Microsoft Visual C++ 2013 Redistributable (x86) - 12.0.40664 | 12.0.40664.0 | Microsoft Corporation | 32 |
| Microsoft Visual C++ 2013 x86 Additional Runtime - 12.0.40664 | 12.0.40664 | Microsoft Corporation | 32 |
| Microsoft Visual C++ 2019 X86 Minimum Runtime - 14.27.29016 | 14.27.29016 | Microsoft Corporation | 32 |

## 9. Compartilhamentos

| Nome | Caminho | Descricao |
|---|---|---|
| ADMIN$ | `C:\WINDOWS` | Remote Admin |
| C$ | `C:\` | Default share |
| IPC$ | `` | Remote IPC |

## 10. Interfaces de rede com IP

| Adaptador | IP | Mascara | Gateway | DNS |
|---|---|---|---|---|
| Red Hat VirtIO Ethernet Adapter | 10.150.5.xx | 255.255.255.0 | 10.150.5.xx | 10.150.8.xx |


