# Regras da `main`, versionadas

O GitHub guarda as regras de branch na configuração do repositório, que **não** está no Git. Este
arquivo existe para que a regra tenha história: o que está valendo, quem decidiu e como reaplicar se
alguém desligar sem querer.

Desenho e razões: [`docs/projeto/47-CI-CD.md` §4](../../docs/projeto/47-CI-CD.md) (issue #58).

## Aplicar

```powershell
gh api repos/tracbel/Visao_360/rulesets -X POST --input .github/rulesets/main-protegida.json
```

Conferir o que está valendo, e comparar com este arquivo:

```powershell
gh api repos/tracbel/Visao_360/rulesets
gh api repos/tracbel/Visao_360/rulesets/<id>
```

Desligar (rollback), sem apagar:

```powershell
gh api repos/tracbel/Visao_360/rulesets/<id> -X PUT -f enforcement=disabled
```

## As configurações do repositório que acompanham a regra

Não cabem no ruleset; são do repositório:

```powershell
gh api repos/tracbel/Visao_360 -X PATCH `
  -F allow_squash_merge=true -F allow_merge_commit=true -F allow_rebase_merge=false `
  -F delete_branch_on_merge=true -F allow_auto_merge=true `
  -f squash_merge_commit_title=PR_TITLE -f squash_merge_commit_message=PR_BODY
```

## O que a regra faz, em uma frase cada

| Regra | Efeito |
|---|---|
| `pull_request` com 0 aprovações | ninguém empurra direto na `main`; a revisão formal não é exigida porque os PRs saem da conta do Ricardo e o GitHub não deixa o autor aprovar o próprio PR (D-CI-2) |
| `required_status_checks` (`backend`, `frontend`, `seguranca`) com `strict` | sem as três checagens verdes **e com a branch atualizada com a `main`**, não há botão de merge |
| `deletion` e `non_fast_forward` | a `main` não pode ser apagada nem reescrita por force push |
| `bypass_actors` = administrador, `bypass_mode: pull_request` | a emergência passa, mas **só por PR** — fica registrada |

`actor_id: 5` é o papel de administrador do repositório. Se um dia o bypass parecer errado na tela,
é esse número que se confere primeiro.
