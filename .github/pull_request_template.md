<!--
  O corpo deste PR vira o corpo do commit de squash na `main` (configuração do repositório, #58).
  Escreva para quem vai ler o `git log` daqui a um ano, não para quem já sabe do que se trata.
  Apague as seções que não se aplicam — menos "Closes", "O que mudou" e "Como verificar".
-->

## Resumo

Uma ou duas frases: o que este PR resolve, e por quê.

Closes #

## O que mudou

| Arquivo ou área | Mudança |
|---|---|
|  |  |

## Como verificar

O que você rodou, com o resultado — número, não adjetivo:

- [ ] `dotnet build -c Release -warnaserror`:
- [ ] `dotnet test -c Release`:
- [ ] `npm run test` / `npm run lint` / `npm run build` (se mexeu na tela):
- [ ] conferência manual (qual tela, qual rota, o que apareceu):

## Banco

- Tem migration? Qual, e ela é **reversível**?
- Precisa de **backup antes** (migration destrutiva — documento 12 §13)?
- Já foi aplicada em qual banco (local, ensaio, servidor)?

> Sem migration e sem tocar em dado: escreva "nenhum" e siga.

## Riscos e rollback

- O que pode quebrar, e como se percebe:
- Como voltar atrás:

## Dados pessoais e segredos

- [ ] Nenhuma credencial, chave, string de conexão completa ou dado pessoal real entrou no código,
      nos testes, nos documentos ou nos exemplos.
- [ ] A varredura (`scripts/seguranca/varrer-segredos.ps1`) passou — o job `seguranca` do CI confere.

## Antes de pedir o merge

- [ ] As três checagens do CI estão verdes (`backend`, `frontend`, `seguranca`).
- [ ] O título do PR está em **Conventional Commits, em português** (`feat(web): …`, `fix(ibge): …`).
- [ ] A issue tem um comentário detalhado com o que foi feito e o que ficou pendente.
- [ ] O que não foi feito está **escrito**, aqui ou na issue — nunca só omitido.
