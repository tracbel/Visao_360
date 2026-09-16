# Dado da empresa no histórico do GitHub — inventário completo e procedimento para aprovação

> **Documento 33** · Versão 2.0 · 13/09/2026 — inventário completo do histórico (credenciais,
> identificadores de usuário e dados pessoais, inclusive os logins da pesquisa e dos comentários) e a
> recomendação de limpeza.
> **Situação: PREPARADO, NÃO EXECUTADO.** Nada foi rodado contra o repositório remoto. A reescrita só
> acontece com a aprovação escrita do Ricardo, e ela **ainda não foi dada** (13/09/2026).
> Convenção: **[medido]** = verificado em 13/09/2026, só com comandos de leitura.
> **Este documento não reproduz nenhum valor encontrado** — só a categoria, o caminho e a contagem.

---

## 0. Resumo

1. **Credencial de autenticação: nenhuma exposta** no histórico inteiro [medido]. As ocorrências
   que pareciam senha ou segredo (§1.3) são nome de variável, leitura de configuração, exemplo de expressão
   regular ou cadeia de conexão **já redigida** — conferida em todas as versões do arquivo. Não há
   token, chave privada nem arquivo de credencial em nenhum commit.
2. **Identificadores de usuário: expostos em 80 arquivos**, concentrados na extração do Vórtice
   (`docs/extracao-vortice/`): logins de funcionários, contas técnicas do SQL Server e nomes de
   servidor. Também aparecem na pesquisa do legado, em documentos de projeto, em comentários do
   código, nos testes e no protótipo.
3. **Dados pessoais e de cliente: expostos em 70 arquivos com nome de funcionário**, 24 com e-mail
   corporativo, e em 4 arquivos com **documento (CPF/CNPJ) de cliente real do CRM**. A extração do
   Vórtice concentra a maior parte (e-mail e nome de 474 operadores, 19 CPFs de pessoas fora do
   cadastro de clientes).
4. **Recomendação: opção C+** — tirar do histórico `Tabelas Exemplo.xlsx` e a pasta
   `docs/extracao-vortice/` inteira, e trocar por texto fictício os identificadores e dados pessoais
   que sobram nos arquivos que precisam continuar no repositório. Justificativa no §3.
5. **Já corrigido no disco, sem commit:** os documentos de cliente real (4 arquivos) e o nome, login
   e e-mail de pessoa real nos testes (2 arquivos) foram trocados por dado fictício do mesmo formato.
   O histórico continua com eles até a reescrita.

---

## 1. O que foi verificado [medido]

### 1.1 O repositório e o que existe no GitHub além do Git

| | |
|---|---|
| Repositório | `github.com/tracbel/Visao_360`, **privado**, criado em 08/09/2026 |
| Branches · tags · releases | 1 (`main`) · 0 · 0 |
| Pull requests · issues · comentários | 0 · 0 · 0 |
| Execuções e artefatos de Actions · segredos · ambientes | 0 · 0 · 0 · 0 |
| Deploy keys · webhooks · convites pendentes | 0 · 0 · 0 |
| Forks | 0 |
| Wiki | habilitada, **nunca criada** (o repositório `.wiki` não existe) |
| Git LFS | não usado |
| Pessoas com acesso | 4, todas administradoras |
| Commits | 15 no dia do inventário; `main` local igual a `origin/main` |

Consequência: **o dado exposto está só no histórico Git**. Não há log de Actions, release, anexo de
issue nem wiki para limpar.

**O commit desta entrega** (commit e push autorizados em 13/09/2026) entra depois deste inventário.
Antes do push, o inventário é rodado de novo e comparado, arquivo a arquivo, com o deste documento; o
resultado fica no documento 32, §11.8. A reescrita, quando aprovada, vale para **todos** os commits
que existirem no dia, e não só para os 15.

### 1.2 Como a varredura foi feita

- **Todos os objetos de todos os commits**, não o `.gitignore` nem só o `HEAD`: 15 commits e 1.428
  caminhos; cada versão de cada arquivo é lida, e vale a maior contagem entre as versões.
- Ferramenta: `scripts/historico/inventariar-historico.py`. Ela **não grava valor**; para reconhecer
  login, nome e documento reais, compara com a lista de usuários do CRM e com o hash dos documentos
  do CRM, as duas em `dados-locais/`, fora do Git.
- **Mensagens de commit e nome e e-mail de autor e de quem commitou** também foram lidos (opção
  `--mensagens`); resultado no §1.6.
- **Não inspecionados por dentro:** 47 imagens `.png` (capturas do protótipo) e a planilha
  `Tabelas Exemplo.xlsx`, cujo conteúdo foi lido à parte (§1.5). Imagem não passou por leitura de texto.

### 1.3 Credenciais de autenticação

| Onde | O que é | Situação |
|---|---|---|
| `docs/extracao-vortice/binarios/INVENTARIO-BINARIOS.md` e `configs-ts172-redigido.md` | 5 cadeias de conexão do Vórtice | senha **redigida** em todas as versões dos dois arquivos (1 versão cada); expõem nome de servidor, de banco e de conta técnica — ver §1.4 |
| `scripts/deploy/instalar-no-servidor.ps1` | leitura da senha digitada (`Read-Host`) | código, sem valor |
| `src/Tracbel.Crm.Api/Seguranca/EntraId.cs` | atribuição de variável ao segredo do Entra | código, sem valor |
| `src/Tracbel.Crm.Carga/Program.cs` | leitura da senha do Protheus da configuração | código, sem valor |
| `src/Tracbel.Crm.Integracao/Protheus/PonteDoProtheus.cs` | desserialização do token de acesso | código, sem valor |
| `tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs` | leitura de variável de ambiente | código, sem valor |
| `scripts/vortice-extracao/binarios-02-configs.ps1` | comentário que explica a expressão que **redige** URL com senha | exemplo, sem valor |
| `docs/pesquisa/08-...salesforce...md` | palavra "token" em texto | texto |
| `infra/.env.exemplo` · `infra/docker-compose.yml` | texto de instrução · interpolação `${...}` | sem valor; diferente dos valores locais |
| Arquivo de credencial ou banco (`.env`, `.pfx`, `.pem`, `.key`, `.bak`, `.mdf`, `.sqlite`…) · JWT · token do GitHub · chave privada | — | **nenhum** |

**Conclusão:** a varredura não achou credencial em texto claro. **Não há senha a trocar por causa do
repositório.** A limitação é a do §1.2: imagem não foi lida.

### 1.4 Identificadores de usuário e de infraestrutura

| Categoria | Onde (arquivos · ocorrências) | Gravidade |
|---|---|---|
| Login de funcionário no formato do Vórtice (`NOME.SOBRENOME`), com prenome e sobrenome de usuário real | **80 arquivos · 1.164 logins distintos por arquivo**: extração do Vórtice 46 · 1.011; pesquisa do legado (`docs/pesquisa/`) 5 · 55; protótipo 5 · 42; front 5 · 18; documentos de projeto 4 · 11; comentários do código 4 · 10; testes 6 · 10; scripts 2 · 3; dicionário 2 · 2; outro documento 1 · 2 | média |
| Login idêntico ao de usuário do CRM | **74 arquivos · 829**: extração 39 · 720; pesquisa 5 · 51; protótipo 5 · 14; front 7 · 11; documentos de projeto 5 · 11; código 4 · 9; testes 6 · 7; scripts 2 · 4; outro documento 1 · 2 | média |
| Contas técnicas e permissões do SQL Server do Vórtice | `docs/extracao-vortice/seguranca-banco/`, **9 arquivos · 293 linhas**: 11 logins de servidor, 19 principais do banco, 234 permissões explícitas, esquemas, papéis, servidores vinculados e configuração | média — mapa de acesso da infraestrutura |
| Nome de servidor, banco e conta técnica em cadeia de conexão | `docs/extracao-vortice/binarios/` (2 arquivos) | média |
| Caminho com o perfil de usuário do Windows | `scripts/prototipo/capturar-referencia.mjs` (1) | baixa |

Os logins da pesquisa e dos comentários, que ficaram fora da versão 1.1, **estão contados acima**.

### 1.5 Dados pessoais e de cliente

| Categoria | Onde (arquivos · quantidade) | Gravidade |
|---|---|---|
| **Documento de cliente real do CRM** | `docs/projeto/24-CARGA-DE-DADOS-2026.md` (1 CPF, 4 CNPJs); `src/Tracbel.Crm.Integracao/Vortice/SaneamentoDoLegado.cs` (1 CNPJ); `tests/Tracbel.Crm.Integracao.Testes/Vortice/SaneamentoDoLegadoTestes.cs` (1 CPF, 2 CNPJs); `src/Tracbel.Crm.Integracao/Carga/RegistrosDaCarga.cs` (1 CNPJ de contraparte sem cliente) | **alta** — **corrigido no disco** (fictício); continua no histórico |
| CPF válido de pessoa fora do cadastro de clientes | `docs/extracao-vortice/catalogos-bpm/OUT_Pessoa.csv` (19) | **alta** |
| E-mail com o domínio corporativo | **24 arquivos · 551 distintos**: extração 475 em 3 arquivos (474 só em `IV_Operador.csv`, 233 deles com nome de usuário real); protótipo 36; front 16; testes 16 (parte fictícia); documentos de projeto 3; scripts 3; código 2 | **alta** na extração; média no restante |
| E-mail de qualquer domínio, fora os de exemplo | 43 arquivos · 1.784, dos quais 1.682 em 12 arquivos da extração (inclui os corporativos e endereços de sistema) | média |
| Nome e sobrenome de usuário real (pode incluir homônimo) | **70 arquivos · 349**: extração 289 em 24 arquivos; documentos de projeto 14 em 11; front 11 em 11; protótipo 10 em 7; pesquisa 8 em 7; scripts 6 em 2; código 4 em 2; testes 4 em 3; dicionário 2; dados de referência 1 | média |
| Nome, login e e-mail de pessoa real em teste | `UsuarioTestes.cs`, `TiposDeValorTestes.cs` | baixa — **corrigido no disco** |
| Telefone formatado | 14 arquivos · 37: protótipo 16; front 8; testes 4; documentos 4; pesquisa 2; código 2; extração 1 | baixa |
| CPF com dígito válido sem correspondência no CRM, fora a extração | pesquisa 1; documentos de projeto 2; testes 4 | baixa — exemplos |
| CNPJ com dígito válido sem correspondência no CRM | inventário de binários da extração 3; documentos de projeto 1; código 1; front 1; testes 5 | baixa — exemplos |
| `Tabelas Exemplo.xlsx` | 6 commits desde `6588b35` | média — razão social real de 3 empresas, primeiro nome de responsáveis e login de quem cadastrou (documentos com dígito inválido) |

### 1.6 Mensagens de commit e identidade de quem commitou

| Categoria | Resultado |
|---|---|
| Login no formato do Vórtice nas mensagens | 4, dos quais 3 idênticos a login de usuário do CRM |
| E-mail com o domínio corporativo nas mensagens | 1 |
| Credencial, token ou documento nas mensagens | nenhum |
| Identidades de autor e de quem commitou | 1 só em todos os commits: nome e sobrenome de usuário real, com e-mail fora do domínio corporativo e que não é o endereço anônimo do GitHub |

`--replace-text` **não altera** mensagem nem autor. Por isso a opção C+ usa também
`--replace-message` (§7.3). O nome e o e-mail do autor só mudam com `--mailmap`, e isso é decisão de
quem fez os commits.

---

## 2. O que já foi corrigido no disco (sem commit)

| Arquivo | Troca |
|---|---|
| `docs/projeto/24-CARGA-DE-DADOS-2026.md` | 1 CPF e 4 CNPJs de cliente real → fictícios do mesmo formato, marcados como fictícios |
| `src/Tracbel.Crm.Integracao/Vortice/SaneamentoDoLegado.cs` | 1 CNPJ de cliente real no comentário → fictício |
| `tests/Tracbel.Crm.Integracao.Testes/Vortice/SaneamentoDoLegadoTestes.cs` | documentos de cliente real → fictícios com o mesmo defeito de origem (zero à esquerda, verificador "00"); os testes passam |
| `src/Tracbel.Crm.Integracao/Carga/RegistrosDaCarga.cs` | 1 CNPJ de contraparte → fictício |
| `tests/Tracbel.Crm.Dominio.Testes/Seguranca/UsuarioTestes.cs`, `Comum/TiposDeValorTestes.cs` | nome, login e e-mail de pessoa real → fictícios |
| `Tabelas Exemplo.xlsx` | retirada do índice (continua no disco) |

A extração do Vórtice, a pesquisa do legado e o protótipo **não** foram alterados: dependem da
decisão do §3.

---

## 3. Opções e recomendação

| Opção | Remove do histórico | Troca de texto | Commits reescritos | O que sobra exposto |
|---|---|---|---|---|
| **A** | `Tabelas Exemplo.xlsx` | — | a partir de `6588b35` | quase tudo do §1.4 e do §1.5 |
| **B** | A + 5 arquivos da extração com e-mail (`IV_Operador.csv`, `GE_EMAILAGD.csv`, `GE_ParamLista.csv`, `GE_ParametroGlobal.csv`, `GE_QVCons.csv`) | — | todos | 19 CPFs de `OUT_Pessoa.csv`, logins e contas técnicas no resto da extração, documentos de cliente e nomes fora da extração |
| **B+** | B | e-mails de pessoa real nos testes e no protótipo | todos | CPFs, logins, contas técnicas, documentos de cliente |
| **C** | A + `docs/extracao-vortice/` inteira | — | todos | logins, nomes e documentos de cliente fora da extração (pesquisa, projeto, código, testes, protótipo) |
| **C+ (recomendada)** | C | identificadores e dados pessoais que sobram fora da extração | todos | nada identificado pela varredura (limite: imagens) |

**Por que C+.**

1. **A extração concentra quase tudo**: 1.011 dos 1.164 logins no formato do Vórtice, 475 dos 551
   e-mails, 289 dos 349 nomes, os 19 CPFs de pessoas e todo o mapa de contas e permissões do SQL
   Server. Tirar a **pasta inteira** é um critério que se confere por caminho; escolher arquivo por
   arquivo (opção B) deixa 41 arquivos com login e já errou uma vez — a versão 1.1 deste documento não
   tinha os CPFs de `OUT_Pessoa.csv`.
2. **A extração é refazível**: é o resultado dos scripts de `scripts/vortice-extracao/` contra um
   banco que continua existindo. Ela não precisa morar no GitHub; pode ficar num compartilhamento
   interno com acesso restrito, ou em `dados-locais/`.
3. **Sem a troca de texto, o resto continua exposto**: a pesquisa do legado, documentos de projeto,
   comentários, testes e protótipo têm logins, nomes e — até a correção do §2 — documentos de cliente.
   Esses arquivos precisam continuar no repositório; por isso a troca, e não a remoção.
4. **Minimização (LGPD)**: dado pessoal de funcionário e de terceiro não tem finalidade num
   repositório de código.

**Custo da C+.** Todos os commits mudam de identificador (a extração está no primeiro); 37 arquivos
fora da pasta citam `docs/extracao-vortice` e passam a apontar para caminho inexistente (§4); a lista
de troca de texto precisa ser montada com cuidado (§7.2).

**Antes da reescrita, com a C+,** o disco tem de ficar igual ao histórico limpo, senão o próximo
commit publica tudo de novo: mover `docs/extracao-vortice/` para fora do repositório, pôr o caminho
no `.gitignore`, aplicar a mesma troca de texto nos arquivos atuais e atualizar as 37 referências.

**Decisão fora da técnica:** a exposição de dado pessoal de funcionário e de terceiro deve ser
comunicada ao encarregado de dados (LGPD), independentemente da reescrita.

---

## 4. Arquivos e referências afetados pela C+

| Grupo | Arquivos | Efeito |
|---|---|---|
| Removidos do histórico | `Tabelas Exemplo.xlsx`; `docs/extracao-vortice/` (746 arquivos rastreados) | somem de todos os commits; continuam no disco desta estação até serem movidos |
| Com troca de texto | **até 82 arquivos a revisar** — têm ao menos um achado do §1.4 ou do §1.5, parte deles fictício: documentos de projeto 16, front 15, testes 14, pesquisa do legado 11, código 9, protótipo 9, dicionário 4, scripts 3, dados de referência 1 | continuam no repositório, sem login, nome, e-mail, telefone e documento reais |
| Referências que quebram | 37 arquivos rastreados citam `docs/extracao-vortice`: 10 em `dados-referencia/`, 12 documentos de `docs/projeto/`, 2 de `docs/dicionario/`, 11 scripts de `scripts/vortice-extracao/`, 2 scripts de `scripts/banco/` — mais os documentos 32 a 34 e `scripts/coleta/extrair-pre-preenchimento.ps1`, desta entrega | trocar o caminho pelo do compartilhamento interno, ou escrever "extração local, fora do repositório" |
| Identificadores de commit citados | documentos 32 e 33 e a memória do projeto citam `6588b35` e `1f78018` | passam a não existir no remoto; atualizar para o identificador novo |

---

## 5. Impacto para os colaboradores

- **4 pessoas com acesso, todas administradoras.** Nenhum pull request, issue ou branch além do `main`
  — não há trabalho de outra pessoa para rebasear.
- **Cada uma precisa apagar o clone antigo e clonar de novo.** `git pull` sobre clone antigo mistura os
  dois históricos; um `push` a partir dele **devolve o dado ao remoto**.
- **Clones e downloads antigos continuam com o dado.** A reescrita não os alcança: é preciso pedir a
  cada pessoa que apague a cópia antiga, inclusive arquivos `.zip` baixados pela interface.
- **Janela sem push** entre o aviso (§7.0) e a confirmação do remoto limpo (§7.5).
- **Nesta estação**, o trabalho em andamento (sem commit) é preservado pelo §7.6.

---

## 6. Limites da remoção no GitHub

- **Commit antigo continua acessível pelo identificador** (URL direta) até o GitHub rodar a coleta de
  lixo. Em repositório privado, isso só acontece com **chamado ao suporte do GitHub**, pedindo a
  remoção das referências antigas e das visões em cache.
- **Cópias fora do nosso controle não voltam:** clones, forks (hoje 0), downloads `.zip`, cópias de
  segurança do próprio GitHub e qualquer ferramenta que tenha lido o repositório.
- **A reescrita não prova que ninguém copiou.** O que ela garante é que o dado deixa de estar
  disponível a partir daqui.
- **Imagem não foi inspecionada** (§1.2): se alguma captura mostrar dado real, ela não entra na lista.
- **Mensagem de commit e identidade de autor** vão junto com o histórico: a C+ troca o texto das
  mensagens (`--replace-message`); nome e e-mail de autor só mudam com `--mailmap` (§1.6).

---

## 7. O procedimento, passo a passo

Comandos para o PowerShell desta estação, com a conta do GitHub que tem acesso ativa.

### 7.0 Antes de começar

1. **Aprovação escrita**, com a opção (A, B, B+, C ou C+).
2. Avisar as 4 pessoas: janela de reescrita, nenhum push até o fim.
3. `python -m pip install git-filter-repo` (hoje não está instalado).
4. Para C e C+: mover `docs/extracao-vortice/` para o compartilhamento interno, pôr no `.gitignore`,
   atualizar as 37 referências e aplicar a troca de texto nos arquivos atuais — **no disco, antes**.
5. Decidir se o trabalho em andamento é commitado antes (e reescrito junto) ou segue o §7.6.

### 7.1 Cópia de segurança do remoto

```powershell
$dia = Get-Date -Format yyyyMMdd
git clone --mirror https://github.com/tracbel/Visao_360.git "D:\seguro\Visao_360-antes-da-reescrita-$dia.git"
```

A cópia **contém o dado exposto**: local restrito, apagar depois do prazo de validação (sugestão: 30 dias).

### 7.2 A lista de troca de texto (B+ e C+)

A lista fica **fora do repositório** (ex.: `D:\seguro\troca.txt`), uma linha por valor, no formato
`literal:<valor real>==><valor fictício>`, e é apagada depois. Ela é montada a partir do inventário
(`dados-locais/historico/inventario.json`) e das mesmas fontes de comparação: e-mails corporativos de
pessoa, logins e nomes de usuário real e documentos de cliente real. **Revisar a lista à mão antes
de usar**: troca de texto errada altera o histórico inteiro.

### 7.3 Reescrita num clone separado

```powershell
git clone --mirror https://github.com/tracbel/Visao_360.git "$env:TEMP\Visao_360-reescrita.git"
Set-Location "$env:TEMP\Visao_360-reescrita.git"

# Opção A
git filter-repo --invert-paths --path "Tabelas Exemplo.xlsx"

# Opção B
git filter-repo --invert-paths --path "Tabelas Exemplo.xlsx" `
  --path "docs/extracao-vortice/catalogos-bpm/IV_Operador.csv" --path "docs/extracao-vortice/catalogos-bpm/GE_EMAILAGD.csv" `
  --path "docs/extracao-vortice/catalogos-bpm/GE_ParamLista.csv" --path "docs/extracao-vortice/catalogos-bpm/GE_ParametroGlobal.csv" `
  --path "docs/extracao-vortice/catalogos-bpm/GE_QVCons.csv"

# Opção C
git filter-repo --invert-paths --path "Tabelas Exemplo.xlsx" --path "docs/extracao-vortice/"

# B+ e C+ — depois da B ou da C, no mesmo clone: o conteúdo E as mensagens de commit
git filter-repo --replace-text "D:\seguro\troca.txt" --replace-message "D:\seguro\troca.txt"
```

### 7.4 Conferência antes de publicar

```powershell
git log --all --oneline -- "Tabelas Exemplo.xlsx"            # vazio
git log --all --oneline -- "docs/extracao-vortice/"           # vazio (C e C+)
git rev-list --count main                                     # o mesmo número do remoto antes da reescrita

# O inventário lê um clone NORMAL, e não o espelho: clonar o espelho reescrito numa pasta à parte.
git clone "$env:TEMP\Visao_360-reescrita.git" "$env:TEMP\Visao_360-conferencia"
python C:\projetos\tracbel-crm\scripts\historico\inventariar-historico.py `
  --repo "$env:TEMP\Visao_360-conferencia" --mensagens --saida "$env:TEMP\inventario-reescrito.json"
```

O inventário do clone reescrito tem de dar **zero** em documento de cliente real, nos CPFs de
`OUT_Pessoa.csv`, em e-mail corporativo de pessoa real e em login e nome de usuário real — nos
arquivos **e nas mensagens de commit**. Os CPF/CNPJ com dígito válido que sobrarem têm de ser só os
fictícios dos testes, conferidos um a um. **Se qualquer conferência falhar, parar aqui**: o remoto
ainda não foi tocado.

### 7.5 Publicação e GitHub

```powershell
git remote add origin https://github.com/tracbel/Visao_360.git
git push origin --force --all
git push origin --force --tags
```

Depois: conferir na interface que `main` aponta para o identificador novo; abrir o chamado ao suporte
do GitHub listando os commits antigos.

### 7.6 Esta estação, sem perder o trabalho em andamento

```powershell
Set-Location C:\projetos\tracbel-crm
git fetch origin
git rev-list --left-right --count main...origin/main   # antes: o primeiro número tem de ser 0
git reset --mixed origin/main    # alinha o ÍNDICE também; o disco fica como está
git diff --cached --stat         # tem de sair VAZIO antes de qualquer commit
git status                       # o trabalho em andamento continua no disco, como não preparado
git reflog expire --expire=now --all
git gc --prune=now
```

**Por que `--mixed`, e não `--soft`.** `--soft` mantém o índice antigo: as entradas de
`docs/extracao-vortice/` e as versões com dado real continuariam preparadas, e o próximo `git commit`
as publicaria de novo no remoto limpo — mover a pasta no disco e pô-la no `.gitignore` não tira nada
do índice. `--mixed` alinha o índice ao `main` reescrito. O `reset` só é seguro se o `main` local não
tiver commit além do remoto, e é isso que a primeira linha confere.

### 7.7 As outras pessoas e a volta atrás

- Cada pessoa apaga o clone antigo e clona de novo; nunca `git pull` sobre clone antigo.
- Volta atrás, só como último recurso: `git push --force --mirror` a partir da cópia do §7.1 — devolve o
  remoto ao estado anterior, **com o dado exposto**.

---

## 8. Critério de pronto

- inventário rodado num clone novo do remoto: zero nas categorias do §7.4;
- `main` remoto com o mesmo número de commits e a árvore igual à anterior, fora o que foi removido
  ou trocado;
- chamado ao GitHub aberto, com número registrado;
- as 4 pessoas confirmaram clone novo, ou que não tinham cópia;
- `.gitignore` e as 37 referências atualizados antes do próximo commit;
- encarregado de dados informado.
