# Banco de desenvolvimento — CRM Tracbel

Do zero até o banco pronto na sua máquina. Regras de schema/nomenclatura/tipos ficam em
[`docs/projeto/14-PADRAO-DE-BANCO.md`](../projeto/14-PADRAO-DE-BANCO.md) e o modelo tabela a
tabela em [`docs/projeto/17-MODELO-UNIFICADO.md`](../projeto/17-MODELO-UNIFICADO.md) — este
arquivo é só o "como rodar".

> **Banco: SQL Server.** A decisão está em
> [`docs/projeto/20-DECISAO-SQL-SERVER.md`](../projeto/20-DECISAO-SQL-SERVER.md), que substitui a
> [`19-DECISAO-POSTGRESQL.md`](../projeto/19-DECISAO-POSTGRESQL.md).
>
> Em **desenvolvimento e CI** é SQL Server 2022 em container, descartável. Em **produção** é um
> banco novo dentro da instância que já hospeda o Vórtice — sem container nenhum, e com as
> condições de convivência da seção 6 daquele documento.

---

## 1. Pré-requisitos

| Ferramenta | Versão usada ao escrever isto |
|---|---|
| Docker Desktop | 29.1.2 |
| Docker Compose | v2.40.3 |
| SDK do .NET | 10.0.401, fixado no `global.json` (`dotnet --version`) |
| `dotnet-ef` | 10.0.12, fixado no `.config/dotnet-tools.json` (`dotnet tool restore`) |

Não precisa instalar SQL Server na máquina — ele roda em container, só para desenvolvimento (a
decisão e o porquê de produção ser diferente estão em
[`docs/projeto/12-DECISAO-CONTAINERS.md`](../projeto/12-DECISAO-CONTAINERS.md), seção 5).

A imagem é a **Developer Edition**, gratuita para desenvolvimento e com o conjunto de recursos da
Enterprise — o que permite exercitar em dev exatamente o que a Standard de produção suporta.

## 2. Subir o banco

Da raiz do repositório:

```powershell
./scripts/banco/subir-banco.ps1
```

Isto:

1. Cria `infra/.env` a partir de `infra/.env.exemplo`, se ainda não existir. **Abra o arquivo e
   troque as senhas** antes de usar a máquina para qualquer coisa além do seu próprio dev local.
   O SQL Server exige senha forte (8+ caracteres, com maiúscula, minúscula, dígito e símbolo):
   senha fraca faz o container morrer no boot.
2. Sobe o container do SQL Server 2022 (`docker compose -f infra/docker-compose.yml`), com a
   colação `Latin1_General_CI_AI` fixada na criação da instância — é ela que faz "Jose" encontrar
   "José" e que impede dois rótulos de catálogo que só diferem em acento de conviverem.
3. Espera o `sqlcmd` responder `healthy`.
4. **Cria o banco `TracbelCrm`** com `COLLATE Latin1_General_CI_AI`, `AUTO_SHRINK OFF` e
   crescimento de arquivo em MB fixo — documento 14, regra 9.5.
5. Cria o **login da aplicação** (`TracbelCrm`, nunca o `sa`) e o torna dono do banco.
6. Aplica as migrations do EF Core (hoje: `ModeloInicial`, que cria os 10 schemas e as 63
   tabelas, e converte quatro delas em particionadas por mês).
7. Roda o seed, **se já existir** `scripts/banco/rodar-seed.ps1` — o gancho está pronto e
   documentado; o dado semente é de outra frente (documento 14, seção 8).
8. Imprime a contagem de tabelas por schema, direto do catálogo do SQL Server. O total tem que
   ser **63**.

A senha e a porta saem de `infra/.env` e chegam ao `dotnet ef` pela variável
`ConnectionStrings__Crm`, que tem precedência sobre `appsettings.json` — por isso trocar a porta
em `infra/.env` basta, sem mexer em arquivo versionado.

> **Portas.** O banco fica em **1433** e a tela de administração em **8081**. As portas 5432 a
> 5440 e a 8080 já estão ocupadas nas máquinas do time, e por isso não são usadas. As duas são
> configuráveis (`DB_PORTA` e `ADMINER_PORTA` em `infra/.env`).

> **`user: "0:0"` no compose.** A imagem 2022 roda como o usuário `mssql` por padrão e, no Docker
> Desktop das máquinas do time, não consegue criar `/.system` na raiz do container — o servidor
> nem sobe. Rodar como root resolve, e o custo é zero: este container é descartável e não existe
> em produção.

## 3. Conferir que subiu

```powershell
docker compose -f infra/docker-compose.yml ps
```

Ou pergunte ao próprio banco:

```powershell
docker exec tracbel-crm-db /opt/mssql-tools18/bin/sqlcmd -S localhost -d TracbelCrm `
  -U sa -P "<DB_SENHA>" -C -Q "
SELECT ISNULL(s.name, '(TOTAL)') AS [Schema], COUNT(*) AS Tabelas
FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.name <> '__EFMigrationsHistory'
GROUP BY ROLLUP (s.name)
ORDER BY CASE WHEN s.name IS NULL THEN 1 ELSE 0 END, s.name;"
```

> **Por que não é preciso descontar partição:** quatro tabelas são particionadas por mês
> (`auditoria.AlteracaoDeCampo`, `auditoria.EventoDeAcesso`, `processo.RegraExecucao`,
> `integracao.Recepcao`), mas no SQL Server a tabela particionada continua sendo **uma linha em
> `sys.tables`** — a partição é uma estrutura física, não um objeto separado. Diferente do
> PostgreSQL, onde cada partição é uma tabela e a contagem precisava de `NOT relispartition`.

Ou conecte com o cliente que preferir (SSMS, Azure Data Studio, DBeaver) em `localhost,1433`,
usuário e senha de `infra/.env`. Para uma tela de administração, sem instalar nada:

```powershell
./scripts/banco/subir-banco.ps1 -ComAdministracao   # Adminer em http://localhost:8081
```

No Adminer, escolha **sistema: MS SQL**, servidor `db`, e o usuário/senha da aplicação. Ele está
num **profile separado** do compose e **não sobe por padrão**: o que o dia a dia precisa é o
banco, e uma tela de administração aberta na máquina de todo mundo é superfície a mais sem
necessidade.

## 4. Resetar do zero

Apaga o container **e o volume de dados** — todo dado local se perde — e sobe de novo:

```powershell
./scripts/banco/resetar-banco.ps1
```

Pede confirmação antes de apagar (é uma operação destrutiva por natureza). Use `-WhatIf` para ver
o que aconteceria sem executar, `-SemSubirDeNovo` para só derrubar, ou `-Confirm:$false` em
automação.

## 5. Gerar o dicionário do banco

Não precisa do container rodando — lê o **modelo do EF Core**, não o banco:

```powershell
./scripts/banco/gerar-dicionario-crm.ps1
```

Gera (e versiona, comitando junto com a mudança de modelo que o motivou):

| Arquivo | Conteúdo |
|---|---|
| [`docs/banco/DICIONARIO.md`](DICIONARIO.md) | tabela por tabela: coluna, tipo, PK/FK/único, check constraints |
| [`docs/banco/ERD.md`](ERD.md) | diagrama entidade-relacionamento em Mermaid, um por schema |
| [`docs/banco/catalogo.csv`](catalogo.csv) | a mesma informação em CSV, uma linha por coluna — para planilha ou script |
| [`docs/banco/esquema.json`](esquema.json) | a mesma informação em JSON, uma entrada por tabela — para ferramenta |

Os quatro arquivos têm o cabeçalho **"gerado automaticamente — não edite à mão"**. Rodar o gerador
de novo depois de qualquer mudança no modelo do EF Core é o que evita a documentação ficar
desatualizada — não existe passo manual de "lembrar de atualizar o dicionário".

## 6. Migrations

Toda alteração de schema nasce de migration do EF Core (documento 14, seção 9) — nunca de script
solto:

```powershell
dotnet ef migrations add <Nome> `
  --project src/Tracbel.Crm.Infraestrutura/Tracbel.Crm.Infraestrutura.csproj `
  --startup-project src/Tracbel.Crm.Api/Tracbel.Crm.Api.csproj
```

> **Atenção ao regenerar a migration inicial.** Duas partes dela são SQL escrito à mão, em
> arquivos que **sobrevivem** à regeneração — mas as **chamadas** deles no arquivo gerado, não.
> Reponha as três linhas:
>
> | Arquivo | Chamada | Onde |
> |---|---|---|
> | `Migrations/ModeloInicial.ColacaoDoBanco.cs` | `DefinirColacaoDoBanco(migrationBuilder);` | **primeira** linha do `Up(...)` |
> | `Migrations/ModeloInicial.Particionamento.cs` | `ParticionarTabelasDeLogEAuditoria(migrationBuilder);` | **última** linha do `Up(...)` |
> | `Migrations/ModeloInicial.Particionamento.cs` | `DesfazerParticionamento(migrationBuilder);` | **última** linha do `Down(...)` |
>
> O teste
> `MigracaoNoContainerTestes.As_quatro_tabelas_de_log_e_auditoria_estao_particionadas_por_data`
> falha se alguém esquecer a do particionamento. A do `Down` é o que faz a migração ser
> reaplicável depois de revertida: `DROP TABLE` não leva junto a função nem o esquema de partição.

Para provar que a migração vai e volta:

```powershell
dotnet ef database update 0 --project ... --startup-project ...   # reverte tudo
dotnet ef database update   --project ... --startup-project ...   # aplica de novo
```

## 7. Testes que tocam o banco

Os testes de padrão de banco (`tests/Tracbel.Crm.Arquitetura.Testes/Banco/`) rodam contra o
**modelo** e não precisam de container. Seis precisam: `MigracaoNoContainerTestes` cria um banco
próprio (`TracbelCrmMigracaoTeste`), roda a migration e confere o resultado no catálogo do SQL
Server — incluindo os dois comportamentos que só existem quando o motor executa: a unicidade que
recusa `'SOJA'` depois de `'Soja'`, e a concorrência otimista que recusa a segunda gravação em
cima da primeira.

- Com o container de pé, eles rodam.
- Sem container, são **ignorados com a razão escrita** — nunca falham por falta de
  infraestrutura, nunca passam em silêncio fingindo que rodaram.
- Para excluí-los explicitamente: `dotnet test --filter "Categoria!=BancoReal"`.

> O login da aplicação recebe `dbcreator` **só no container**, porque estes testes criam e
> destroem o banco deles a cada execução. Em homologação e produção ele não tem essa função: lá o
> banco é criado uma vez pelo runbook da infra.

## 8. Problemas comuns

| Sintoma | Solução |
|---|---|
| Porta 1433 já em uso | mude `DB_PORTA` em `infra/.env` e rode `subir-banco.ps1` de novo |
| Container nunca fica `healthy` | `docker compose -f infra/docker-compose.yml logs db` |
| `Password validation failed` no log | `DB_SENHA` fraca demais: o SQL Server exige 8+ caracteres com maiúscula, minúscula, dígito e símbolo |
| `The system directory [/.system] could not be created` | falta o `user: "0:0"` no serviço `db` do compose — ver a nota da seção 2 |
| `A connection was successfully established... certificate` | falta `TrustServerCertificate=True` na string de conexão; o container usa certificado autoassinado |
| `CREATE DATABASE permission denied` ao rodar os testes de `BancoReal` | o login da aplicação perdeu o `dbcreator`; rode `subir-banco.ps1` de novo |
| `docker compose` não é reconhecido | Docker Desktop não está rodando, ou é uma versão sem Compose v2 embutido |

## 9. O que ainda não existe (e por quê)

- **Seed.** O dado semente (permissões, tipos de processo, fases, catálogos, linhas de negócio) é
  de outra frente. O gancho está pronto: basta existir `scripts/banco/rodar-seed.ps1` e o
  `subir-banco.ps1` passa a chamá-lo. As três regras que ele terá de obedecer estão comentadas no
  próprio script e no documento 14, seção 8.
- **Job mensal de partição.** As partições vão do terceiro mês anterior ao décimo segundo mês
  seguinte. Criar os limites seguintes (`ALTER PARTITION FUNCTION ... SPLIT RANGE`) é trabalho de
  job mensal, que ainda não existe. Não há urgência: a função de partição do SQL Server cobre o
  domínio inteiro, então nenhuma linha é recusada se o job atrasar — a última partição só fica
  maior ([doc 20, seção 9](../projeto/20-DECISAO-SQL-SERVER.md)).
- **Filtro global de segurança nas 62 entidades novas.** Hoje só `Lead` tem `HasQueryFilter`
  (documento 14, regra 11.3). O modelo de segurança é o documento 05 e é outra frente.
