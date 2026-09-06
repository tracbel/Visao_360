# Glossário e nomes — as palavras que a Tracbel usa

> Requisito do Ricardo, 03/09/2026: **"nossas tabelas com nomes legíveis que entendemos"**.
> Este documento fixa o vocabulário. A regra de nomenclatura que o obriga está no
> [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md); aqui ficam os termos e o de-para.

---

## 1. Por que isto existe

Quem abre o banco do Vórtice hoje encontra `GE_Pessoa`, `IV_Processo`, `IVS_Pes`, `EXT_Veic`,
`SeqPessoa`, `CodProcesso`, `DtaRealizacao`, `IndEstorno`. Nada disso se lê. É preciso um mapa,
e o mapa mora na cabeça de duas pessoas.

O nosso banco vai ter dezenas de tabelas hoje e centenas depois. Se o nome não for legível,
a mesma dependência se repete — só que agora por nossa causa.

**A regra:** o nome da tabela e da coluna é a **palavra que o pessoal do comercial usa**,
inteira, em português, sem abreviação e sem sigla. Se um analista comercial não entende o
nome, o nome está errado.

Três exceções, e só três:

| Exceção | Por quê |
|---|---|
| Siglas que a própria Tracbel fala | CEN, CNPJ, CPF, NF, OS, ERP, CRM. São palavras no dia a dia |
| Termos técnicos sem tradução usada | `Id`, `Lead` |
| Sem acento e sem `ç` no identificador | evita problema de collation, ferramenta e script. `Servico`, não `Serviço` |

---

## 2. Schemas — a palavra inteira, não a sigla

O [04-MODELO-DADOS](04-MODELO-DADOS.md) usou siglas (`wf`, `seg`, `intg`, `aud`, `equip`, `rel`,
`meta`). Só `crm` se lê. Passam a ser:

| Antes | Agora | O que guarda |
|---|---|---|
| `org` | `organizacao` | empresa, filial, linha de negócio, hierarquia comercial |
| `seg` | `seguranca` | usuário, equipe, permissão, compartilhamento |
| `crm` | `comercial` | cliente, contato, carteira, lead, consentimento |
| `wf` | `processo` | oportunidade e demais processos, fase, tarefa, interação, regra |
| `equip` | `frota` | equipamento do cliente, marca, modelo, família |
| `doc` | `documento` | arquivo anexado e seus vínculos |
| `aud` | `auditoria` | quem viu e quem alterou o quê |
| `intg` | `integracao` | fronteira com o ERP e com o Vórtice |
| `meta` | `metadado` | campo personalizado, extensão sem release |
| `rel` | `relatorio` | fontes curadas de relatório |

`frota` em vez de `equipamento` porque é a palavra do negócio — a tela de Cobertura já fala
"Frota mista JD/Case" — e porque evita ler `equipamento.Equipamento`.

---

## 3. O glossário do negócio

Cada termo: o que é, onde aparece na tela, como o Vórtice chama, e a nossa tabela.

### 3.1 Comercial

| Termo Tracbel | O que é | Na tela | No Vórtice | Nossa tabela |
|---|---|---|---|---|
| **Cliente** | pessoa ou empresa com quem se faz negócio, inclusive quem ainda não comprou | Clientes, Ficha do Cliente | `GE_Pessoa` | `comercial.Cliente` |
| **Contato** | a pessoa física dentro do cliente | Ficha do Cliente, aba Contatos | `GE_Contato` | `comercial.Contato` |
| **Carteira** | o conjunto de clientes sob responsabilidade de um CEN, por linha de negócio | Cobertura de Carteira | `IVS_Carteira` · `IVS_Pes` | `organizacao.Carteira` · `comercial.ClienteCarteira` |
| **CEN** | Consultor Externo de Negócios, o vendedor de campo | em toda tela | `IV_VENDEDOR` | `seguranca.Usuario` com papel de CEN |
| **Lead** | interesse ainda não qualificado | fase 1 | ação 897 | `comercial.Lead` |
| **Classe do cliente** | A, B, C, D — define a frequência de visita esperada | chips de filtro | não existe formalizado | coluna `Classe` em `comercial.ClienteCarteira` |
| **Consentimento** | autorização de contato por canal, com prova e data | — | `IV_OPTEMAIL` · `IV_OPTFONE` | `comercial.ConsentimentoComunicacao` |

> **Por que `Cliente` e não `Conta`.** O documento 04 usou `Conta`, que é o termo do Salesforce.
> Ninguém na Tracbel fala "conta"; fala cliente, e a tela do protótipo diz Clientes. A distinção
> que motivou `Conta` — abranger prospect, cliente ativo e ex-cliente na mesma tabela — continua
> valendo: é a coluna `Situacao` que diz em que estágio o cliente está, exatamente como no Vórtice,
> onde `GE_Pessoa.Status` vale `P` de prospect na maioria das linhas.

### 3.2 Processo e agenda

| Termo Tracbel | O que é | Na tela | No Vórtice | Nossa tabela |
|---|---|---|---|---|
| **Oportunidade** | um negócio em andamento — é o Processo do tipo Venda | Pipeline, Ficha de Oportunidade | `IV_Processo` com `CodProcesso` 50 | `processo.Processo` |
| **Processo** | qualquer caso que caminha por fases: venda, demonstração, cobrança, aferição | — | `IV_Processo` | `processo.Processo` |
| **Tipo de processo** | o modelo do fluxo | — | `IV_CodProcesso` (62 modelos) | `processo.TipoProcesso` |
| **Fase** | em que ponto o processo está | linha do tempo da oportunidade | `IV_ProcFase` | `processo.Fase` |
| **Tarefa** | o compromisso na agenda de alguém | Agenda do CEN | `IV_Agenda` | `processo.Tarefa` |
| **Tipo de tarefa** | visita, ligação, e-mail, aprovação | — | `IV_Acao` (980, 378 em uso) | `processo.TipoTarefa` |
| **Interação** | o contato que aconteceu, registrado e imutável | "Última interação" na Cobertura | `IV_Historico` | `processo.Interacao` |
| **Resultado** | o desfecho de uma tarefa, que move a fase e gera a próxima | — | `IV_Resultado` (4.209) | `processo.Resultado` |
| **Regra** | o que acontece depois de um resultado | — | `IV_AcaoAuto` · `IV_ProcResultado` | `processo.Regra` |
| **Motivo de perda** | por que o negócio não fechou | Visão 360, Configurações | catálogo solto | `processo.MotivoDePerda` |

> **Por que `Interacao` e não `Atividade`.** A tela de Cobertura já chama de "última interação",
> e as categorias de contato do protótipo são visita, ligação, WhatsApp, e-mail e reunião remota.
> "Atividade" é o termo do Salesforce; "interação" é o nosso.

### 3.3 Frota, documento e o resto

| Termo Tracbel | O que é | Na tela | No Vórtice | Nossa tabela |
|---|---|---|---|---|
| **Equipamento** | a máquina do cliente, identificada pelo chassi | Ficha do Equipamento | `EXT_Veic` (morto) · `IV_ClientePropr` (vivo) | `frota.Equipamento` |
| **Modelo / Marca / Família** | catálogo de máquinas | catálogo da Nova Oportunidade | `EXT_VeicMarca` | `frota.Modelo` · `frota.Marca` · `frota.Familia` |
| **Documento** | arquivo anexado a um processo | aba Documentação | `DMN_Doc` + FTP | `documento.Documento` |
| **Empresa / Filial** | a unidade da Tracbel | seletor de contexto | `GE_Empresa` (18) | `organizacao.Empresa` |
| **Linha de negócio** | tratores, colheitadeiras, peças, serviços | Mix por linha | `IVS_Depto` (29) | `organizacao.LinhaDeNegocio` |
| **Meta** | o alvo de faturamento ou de cobertura | Configurações, Performance | não existe | `organizacao.Meta` |

---

## 4. De-para completo — o que muda em relação ao documento 04

Aplicar como errata no [04-MODELO-DADOS](04-MODELO-DADOS.md) antes da primeira migration.

| Documento 04 | Passa a ser | Motivo |
|---|---|---|
| `crm.Conta` | `comercial.Cliente` | vocabulário Salesforce → vocabulário Tracbel |
| `crm.ContaContato` | `comercial.ClienteContato` | idem |
| `crm.ContaCarteira` | `comercial.ClienteCarteira` | idem |
| `crm.Contato` | `comercial.Contato` | só o schema |
| `crm.CanalContato` | `comercial.CanalContato` | só o schema |
| `crm.ConsentimentoComunicacao` | `comercial.ConsentimentoComunicacao` | só o schema |
| `crm.Lead` | `comercial.Lead` | só o schema |
| `wf.Processo` | `processo.Processo` | só o schema |
| `wf.TipoProcesso` | `processo.TipoProcesso` | só o schema |
| `wf.Estagio` | `processo.Fase` | a Tracbel e o Vórtice dizem fase |
| `wf.ProcessoEstagioTrilha` | `processo.PassagemDeFase` | nome não se lia |
| `wf.Tarefa` | `processo.Tarefa` | só o schema |
| `wf.TipoTarefa` | `processo.TipoTarefa` | só o schema |
| `wf.Atividade` | `processo.Interacao` | é o termo da tela |
| `wf.AtividadeParticipante` | `processo.InteracaoParticipante` | idem |
| `wf.Desfecho` | `processo.Resultado` | é o termo do negócio |
| `wf.Regra` · `wf.RegraExecucao` | `processo.Regra` · `processo.RegraExecucao` | só o schema |
| `org.Empresa` | `organizacao.Empresa` | só o schema |
| `org.LinhaNegocio` | `organizacao.LinhaDeNegocio` | palavra inteira |
| `org.Carteira` | `organizacao.Carteira` | só o schema |
| `org.HierarquiaVendas` | `organizacao.HierarquiaComercial` | é como o time fala |
| `seg.*` | `seguranca.*` | só o schema |
| `seg.ConjuntoPermissao` | `seguranca.ConjuntoDePermissao` | leitura |
| `seg.CompartilhamentoRegistro` | `seguranca.CompartilhamentoDeRegistro` | leitura |
| `aud.CampoAuditado` | `auditoria.CampoAuditado` | só o schema |
| `aud.AlteracaoCampo` | `auditoria.AlteracaoDeCampo` | leitura |
| `aud.EventoAcesso` | `auditoria.EventoDeAcesso` | leitura |
| `intg.ChaveExterna` | `integracao.ChaveExterna` | só o schema |
| `intg.MensagemSaida` | `integracao.MensagemDeSaida` | leitura |
| `intg.MensagemDescartada` | `integracao.MensagemDescartada` | só o schema |
| **`intg.Watermark`** | `integracao.PontoDeSincronismo` | **estava em inglês** |
| `equip.Equipamento` | `frota.Equipamento` | evita repetir a palavra |
| `equip.Marca` · `Modelo` · `Familia` | `frota.Marca` · `frota.Modelo` · `frota.Familia` | só o schema |
| `doc.Documento` | `documento.Documento` | só o schema |
| `doc.DocumentoVinculo` | `documento.Vinculo` | evita repetir a palavra |
| `meta.CampoEntidade` | `metadado.CampoPersonalizado` | diz o que é |
| `meta.ManipuladorEvento` | `metadado.TratadorDeEvento` | "manipulador" não se usa |
| `rel.FonteRelatorio` | `relatorio.Fonte` | evita repetir a palavra |
| `rel.FonteCampo` | `relatorio.FonteCampo` | só o schema |
| `rel.Relatorio` | `relatorio.Relatorio` | só o schema |

São 49 tabelas. Trinta e nove mudam só de schema; **dez mudam de nome**, e cada uma tem motivo
acima.

---

## 5. Abreviações proibidas

Nome de tabela e de coluna usa a palavra inteira. Estas aparecem no Vórtice e não vão aparecer aqui:

| Não | Sim | | Não | Sim |
|---|---|---|---|---|
| `Qtd` | `Quantidade` | | `Cod` | `Codigo` |
| `Desc` | `Descricao` | | `Seq` | `Sequencia` ou `Id` |
| `Dt`, `Dta` | `Data` | | `Proc` | `Processo` |
| `Num`, `Nro` | `Numero` | | `Usr` | `Usuario` |
| `Vlr` | `Valor` | | `Cli` | `Cliente` |
| `Ind` | `Indicador` ou o nome do que indica | | `Op` | `Oportunidade` |
| `Tp` | `Tipo` | | `Cmpl` | `Complemento` |
| `Ender` | `Endereco` | | `Obs` | `Observacao` |

O teste de nomenclatura do [14-PADRAO-DE-BANCO](14-PADRAO-DE-BANCO.md) barra o merge que
introduzir qualquer uma delas.

**Coluna booleana** não começa com `Ind`. Começa com o que ela afirma: `EstaAtivo`,
`ExigeVisitaPresencial`, `FoiEstornada`. `IndEstorno` vira `FoiEstornada`.

---

## 6. Como um termo novo entra

1. Alguém do negócio usa a palavra numa reunião. Ela é a candidata.
2. Confere-se se já existe termo para a mesma coisa neste glossário. Se existir, usa-se o que existe.
3. Acrescenta-se aqui: o termo, o que é, onde aparece, como o Vórtice chamava, e a tabela.
4. Só então vira código.

Termo que não está aqui não entra em tabela nova. É o que impede o banco de ganhar três nomes
para a mesma coisa, como aconteceu com `FINALIZADO` e `FINALIZADA` no legado.
