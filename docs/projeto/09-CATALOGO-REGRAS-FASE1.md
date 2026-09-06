# Catálogo de regras — Fase 1 (Leads e Prospecção)

> **Documento 09 de 09** · Versão 1.0 · 30/08/2026
> Este é o **conteúdo** que roda dentro do motor descrito no documento 03. Sem ele, o motor
> existe vazio e ninguém consegue começar. É a especificação do seed do banco.
> `[V]` = corrige um defeito medido no Vórtice.

---

## 1. Como ler

O motor tem cinco catálogos e um conjunto de regras:

```
TipoProcesso ──┬── Estagio        (a barra que o usuário vê no topo)
               └── TipoTarefa ──── Desfecho   (o que se escolhe ao concluir)

Regra: quando EVENTO acontece, SE condição, FAÇA efeito, PARA alguém
```

**Curadoria obrigatória.** `[V]` O Vórtice tem **980 ações (169 usadas em 2026)** e
**4.208 resultados (501 usados)**. Este catálogo nasce com **6 tipos de tarefa e 21 desfechos**.
A coluna `UltimoUsoEm` existe para que ele nunca mais apodreça sem que alguém saiba.

---

## 2. Tipo de processo

| Código | Nome | Versão |
|---|---|---|
| `PROSPECCAO` | Prospecção e Qualificação | 1 |

Um só, de propósito. Os fluxos de Venda, Demonstração, Peças, DSI e Grandes Contas entram
nas fases 3 e 4, cada um como um `TipoProcesso` novo — **sem tocar no motor**.

---

## 3. Estágios

| Ordem | Código | Nome | Marco | Trava se faltar campo | Final |
|---|---|---|---|---|---|
| 10 | `NOVO` | Lead recebido | 1. Entrada | não | não |
| 20 | `QUALIFICACAO` | Em qualificação | 1. Entrada | **sim** | não |
| 30 | `MONITORAMENTO` | Monitoramento | 2. Relacionamento | não | não |
| 40 | `INTERESSE` | Com interesse de compra | 3. Handoff | **sim** | não |
| 90 | `ENCERRADO` | Encerrado | 4. Fim | não | **sim** |

**Campos obrigatórios por estágio** (`[DYN]` *stage-gating* — o processo não avança sem eles):

| Estágio | Exige |
|---|---|
| `QUALIFICACAO` → `MONITORAMENTO` | conta vinculada, contato principal, linha de negócio |
| `MONITORAMENTO` → `INTERESSE` | carteira atribuída, interesse descrito, valor estimado |
| qualquer → `ENCERRADO` | motivo de encerramento |

> `[V]` **Entrega (90) não é o fim** no Vórtice — ainda faltam Pós-Entrega e Finalizado, e é por
> isso que 155 de 239 processos ficaram parados achando que tinham terminado. Aqui só
> `ENCERRADO` tem `EhFinal = 1`, e o nome não deixa dúvida.

---

## 4. Tipos de tarefa

| Código | Nome | Prazo (dias úteis) | Aprovação |
|---|---|---|---|
| `QUALIFICAR_LEAD` | Validar e qualificar lead | 1 | não |
| `MONITORAR_CLIENTE` | Monitorar cliente | 5 | não |
| `RETOMAR_CONTATO` | Retomar contato | 1 | não |
| `REGISTRAR_INTERESSE` | Registrar interesse de compra | 2 | não |
| `ATRIBUIR_CARTEIRA` | Atribuir carteira ao cliente | 1 | não |
| `REVISAR_DUPLICADO` | Revisar possível duplicado | 3 | não |

### Por que cada uma existe

**`QUALIFICAR_LEAD`** — a tarefa central da fase. Equivale à ação 897 do Vórtice.

> `[V]` **A 897 tem `QtdeMaxPessoa = 1`, e o limite é VITALÍCIO.** Em 193 casos medidos, 189
> (98%) foram bloqueados porque a pessoa já tivera uma 897 alguma vez — mas **só 7 ainda
> estavam abertas**. Reconversão de lead conhecido nunca vira tarefa nova para o vendedor;
> fica só no histórico, com o carimbo `[(wf) Não gerou ação 897, LIMITE]`.
>
> **Aqui não existe limite no catálogo.** A política de retrabalho é a regra `R_LEAD_RECORRENTE`
> (seção 5), com **janela temporal configurável** — 90 dias por padrão.

**`REVISAR_DUPLICADO`** — a fila de deduplicação, com dono e prazo.

> `[V]` O motor de deduplicação do Vórtice é bom, e a **fila de 124 mil pares está abandonada
> desde 2018**. Uma fila sem dono e sem SLA é igual a não ter fila. Por isso ela nasce como
> **tarefa atribuída**, não como lista que alguém deveria olhar.

**`ATRIBUIR_CARTEIRA`** — quando o lead cai numa cidade × linha de negócio sem carteira.

> `[V]` No Vórtice isso simplesmente não gera nada, e o lead fica órfão. Aqui vira tarefa
> explícita para o gestor da linha.

---

## 5. Desfechos

### `QUALIFICAR_LEAD`

| Código | Nome | Classe | Exige justificativa |
|---|---|---|---|
| `QL_QUALIFICADO` | Lead qualificado | Avanço | não |
| `QL_EM_ANDAMENTO` | Contato em andamento | Manutenção | não |
| `QL_SEM_SUCESSO` | Contato sem sucesso | Manutenção | não |
| `QL_SEM_PERFIL` | Sem perfil para nossos produtos | Perda | **sim** |
| `QL_SEM_INTERESSE` | Sem interesse no momento | Perda | **sim** |
| `QL_DUPLICADO` | Duplicado de cliente existente | Cancelamento | não |
| `QL_DADO_INVALIDO` | Dados de contato inválidos | Cancelamento | **sim** |

### `MONITORAR_CLIENTE`

Espelha os resultados 3223–3229 da ação 767 do Vórtice, curados.

| Código | Nome | Classe |
|---|---|---|
| `MC_COM_INTERESSE` | Com interesse de compra | Avanço |
| `MC_EM_ANDAMENTO` | Contato em andamento | Manutenção |
| `MC_SEM_SUCESSO` | Contato sem sucesso | Manutenção |
| `MC_INSATISFEITO` | Cliente insatisfeito | Manutenção |
| `MC_SEM_INTERESSE` | Sem interesse no período | Perda |
| `MC_CANCELADO` | Monitoramento encerrado | Cancelamento |

### `RETOMAR_CONTATO`

| Código | Nome | Classe |
|---|---|---|
| `RC_RETOMADO` | Contato retomado, com interesse | Avanço |
| `RC_SEM_SUCESSO` | Não conseguiu contato | Manutenção |
| `RC_DESISTIU` | Confirmou desistência | Perda |

### `REGISTRAR_INTERESSE`

| Código | Nome | Classe |
|---|---|---|
| `RI_ENCAMINHADO` | Encaminhado para venda | Avanço |
| `RI_AGUARDANDO` | Aguardando definição do cliente | Manutenção |
| `RI_DESISTIU` | Desistiu antes da proposta | Perda |

### `ATRIBUIR_CARTEIRA` e `REVISAR_DUPLICADO`

| Código | Nome | Classe |
|---|---|---|
| `AC_ATRIBUIDA` | Carteira atribuída | Avanço |
| `AC_SEM_COBERTURA` | Sem carteira para a região | Manutenção |
| `RD_DUPLICADO` | Confirmado duplicado — mesclar | Cancelamento |
| `RD_DISTINTO` | São clientes distintos | Avanço |

> `[V]` **Repare no que NÃO existe:** não há um "Atividade Cancelada" genérico.
> No Vórtice, cada ação tem o seu (3438 na 807, 2623 na 611, 3291 na 375, 3305 na 612), e
> **alguns estão mapeados para `Fase=Finalizado, Status=CANCELADO`** — dar esse andamento numa
> agenda duplicada **cancela o processo inteiro, sem desfazer**. É o erro mais caro do dia a dia.
>
> Aqui, cancelar uma tarefa é uma **operação sobre a tarefa** (`Tarefa.Cancelar`), nunca um
> desfecho que mexe no processo.

---

## 6. As regras

Formato: **quando** `evento` **[+ desfecho]**, **se** `condição`, **faça** `efeito`, **para** `destinatário`.

| # | Código | Gatilho | Condição | Efeito | Destinatário | Prazo |
|---|---|---|---|---|---|---|
| 1 | `R_LEAD_NOVO` | `LeadRecebido` | `lead.TemCarteira` | criar `QUALIFICAR_LEAD` | `carteira.Responsavel` | 1 |
| 2 | `R_LEAD_SEM_CARTEIRA` | `LeadRecebido` | `!lead.TemCarteira` | criar `ATRIBUIR_CARTEIRA` | `equipe:GESTAO_COMERCIAL` | 1 |
| 3 | `R_LEAD_SUSPEITA_DUPLICIDADE` | `LeadRecebido` | `lead.ScoreDuplicidade >= 80` | criar `REVISAR_DUPLICADO` | `equipe:CADASTRO` | 3 |
| 4 | `R_LEAD_RECORRENTE` | `LeadRecebido` | `lead.QualificacoesAnteriores > 0 && lead.DiasDesdeUltimaQualificacao >= 90` | criar `QUALIFICAR_LEAD` | `carteira.Responsavel` | 1 |
| 5 | `R_QUALIFICADO_MONITORA` | `TarefaConcluida` + `QL_QUALIFICADO` | — | criar `MONITORAR_CLIENTE` | `carteira.Responsavel` | 5 |
| 6 | `R_QUALIFICADO_AVANCA` | `TarefaConcluida` + `QL_QUALIFICADO` | — | mover para `MONITORAMENTO` | — | — |
| 7 | `R_QUALIF_ANDAMENTO` | `TarefaConcluida` + `QL_EM_ANDAMENTO` | — | criar `QUALIFICAR_LEAD` | `quemExecutou` | 2 |
| 8 | `R_QUALIF_SEM_SUCESSO` | `TarefaConcluida` + `QL_SEM_SUCESSO` | `tarefa.TentativasAnteriores < 3` | criar `QUALIFICAR_LEAD` | `quemExecutou` | 2 |
| 9 | `R_QUALIF_ESGOTOU` | `TarefaConcluida` + `QL_SEM_SUCESSO` | `tarefa.TentativasAnteriores >= 3` | encerrar processo | — | — |
| 10 | `R_DUPLICADO_REVISA` | `TarefaConcluida` + `QL_DUPLICADO` | — | criar `REVISAR_DUPLICADO` | `equipe:CADASTRO` | 3 |
| 11 | `R_MONITORA_CICLO` | `TarefaConcluida` + `MC_EM_ANDAMENTO` | — | criar `MONITORAR_CLIENTE` | `carteira.Responsavel` | `carteira.DiasCicloContato` |
| 12 | `R_MONITORA_INTERESSE` | `TarefaConcluida` + `MC_COM_INTERESSE` | — | criar `REGISTRAR_INTERESSE` | `carteira.Responsavel` | 2 |
| 13 | `R_INTERESSE_AVANCA` | `TarefaConcluida` + `MC_COM_INTERESSE` | — | mover para `INTERESSE` | — | — |
| 14 | `R_INTERESSE_ALTO_VALOR` | `TarefaConcluida` + `RI_ENCAMINHADO` | `processo.ValorEstimado > 500000` | notificar | `gestorDe(carteira.Responsavel)` | — |
| 15 | `R_INSATISFEITO_ESCALA` | `TarefaConcluida` + `MC_INSATISFEITO` | — | notificar | `gestorDe(carteira.Responsavel)` | — |
| 16 | `R_SEM_INTERESSE_RETOMA` | `TarefaConcluida` + `MC_SEM_INTERESSE` | — | criar `RETOMAR_CONTATO` | `carteira.Responsavel` | 90 |
| 17 | `R_PERDA_ENCERRA` | `TarefaConcluida` + desfecho classe `Perda` | — | encerrar processo | — | — |
| 18 | `R_CARTEIRA_ATRIBUIDA` | `TarefaConcluida` + `AC_ATRIBUIDA` | — | criar `QUALIFICAR_LEAD` | `carteira.Responsavel` | 1 |
| 19 | `R_SEM_COBERTURA_ESCALA` | `TarefaConcluida` + `AC_SEM_COBERTURA` | — | notificar | `equipe:DIRETORIA_COMERCIAL` | — |

**19 regras.** `[V]` O Vórtice tem **5.958**, para 1.262 pares distintos — porque, sem condição,
a única saída é replicar a regra por filial, com o destinatário fixo na linha.

### As quatro regras que corrigem defeitos específicos

**`R_LEAD_RECORRENTE` (nº 4)** — corrige o limite vitalício da ação 897. A condição tem
**janela temporal** (`DiasDesdeUltimaQualificacao >= 90`), então lead que volta depois de uma
safra gera tarefa nova; lead que volta na semana seguinte, não.

**`R_QUALIF_ESGOTOU` (nº 9)** — corrige o processo que fica em loop de tentativa.
Três tentativas sem sucesso encerram, em vez de gerar tarefa para sempre.

**`R_SEM_INTERESSE_RETOMA` (nº 16)** — o destinatário é `carteira.Responsavel`, resolvido
**no momento da execução**, 90 dias depois.

> `[V]` Este é exatamente o **caso B do suporte**: um usuário viu 7 tarefas que não eram dele.
> Seis foram geradas por ele mesmo dois meses antes, ao lançar "Desistiu da Compra" em processos
> de colegas — as regras 9067/9068 têm `SeqUsuario = 0`, que devolve o follow-up **para quem
> lançou o andamento**, não para o dono do cliente. Validado em 20 de 20 casos.
>
> Com `carteira.Responsavel` resolvido em runtime, a retomada cai em quem cuida do cliente hoje.

**`R_INTERESSE_ALTO_VALOR` (nº 14)** — a condição `processo.ValorEstimado > 500000` é o que,
no Vórtice, exigiria uma regra duplicada por filial. Aqui é uma linha.

---

## 7. Expressões de condição

O avaliador entende um subconjunto deliberadamente pequeno. **Não é uma linguagem de programação** —
é um filtro. Se uma condição precisa de mais que isto, o lugar dela é um manipulador de evento.

| Operador | Exemplo |
|---|---|
| Comparação | `processo.ValorEstimado > 500000` |
| Igualdade | `conta.Situacao == 'Cliente'` |
| Lógicos | `&&`, `\|\|`, `!` |
| Pertinência | `lead.OrigemId in [10, 11, 12]` |
| Nulo | `processo.CarteiraId != null` |

**Variáveis disponíveis:** `lead.*`, `conta.*`, `processo.*`, `tarefa.*`, `carteira.*`, `usuario.*`.

**Regra de validação:** a expressão é conferida **na gravação da regra**. Expressão inválida
não entra na tabela.

> `[V]` No Vórtice, o único mecanismo de extensão são os "Objetos Dinâmicos": **196 SQLs gravados
> soltos no banco**. Uma linguagem de condição fechada e validada é o oposto disso — e é o que
> impede que a configuração vire um segundo código-fonte sem revisão nem teste.

---

## 8. Seed

```csharp
/// <summary>
/// Popula o catálogo da fase 1. Roda uma vez, na migration inicial.
///
/// A ORDEM IMPORTA: catálogos antes das regras, porque a regra referencia o desfecho.
/// </summary>
public static class SeedFase1
{
    public static async Task ExecutarAsync(CrmDbContext db, CancellationToken ct)
    {
        if (await db.TiposProcesso.AnyAsync(ct)) return;   // idempotente

        // ---- 1. Tipo de processo e estágios ----
        var prospeccao = TipoProcesso.Criar("PROSPECCAO", "Prospecção e Qualificação");
        db.TiposProcesso.Add(prospeccao);

        var estagios = new[]
        {
            Estagio.Criar(prospeccao, "NOVO",          "Lead recebido",           10, "1. Entrada"),
            Estagio.Criar(prospeccao, "QUALIFICACAO",  "Em qualificação",         20, "1. Entrada", exigeCampos: true),
            Estagio.Criar(prospeccao, "MONITORAMENTO", "Monitoramento",           30, "2. Relacionamento"),
            Estagio.Criar(prospeccao, "INTERESSE",     "Com interesse de compra", 40, "3. Handoff", exigeCampos: true),
            Estagio.Criar(prospeccao, "ENCERRADO",     "Encerrado",               90, "4. Fim", ehFinal: true)
        };
        db.Estagios.AddRange(estagios);

        // ---- 2. Tipos de tarefa ----
        var qualificar = TipoTarefa.Criar("QUALIFICAR_LEAD",     "Validar e qualificar lead",      prazoDiasUteis: 1);
        var monitorar  = TipoTarefa.Criar("MONITORAR_CLIENTE",   "Monitorar cliente",              prazoDiasUteis: 5);
        var retomar    = TipoTarefa.Criar("RETOMAR_CONTATO",     "Retomar contato",                prazoDiasUteis: 1);
        var interesse  = TipoTarefa.Criar("REGISTRAR_INTERESSE", "Registrar interesse de compra",  prazoDiasUteis: 2);
        var carteira   = TipoTarefa.Criar("ATRIBUIR_CARTEIRA",   "Atribuir carteira ao cliente",   prazoDiasUteis: 1);
        var duplicado  = TipoTarefa.Criar("REVISAR_DUPLICADO",   "Revisar possível duplicado",     prazoDiasUteis: 3);

        db.TiposTarefa.AddRange(qualificar, monitorar, retomar, interesse, carteira, duplicado);

        // ---- 3. Desfechos ----
        // Classe alimenta o funil E a regra R_PERDA_ENCERRA, que dispara por CLASSE,
        // não por código — assim um desfecho novo de perda já encerra sem regra nova.
        db.Desfechos.AddRange(
            Desfecho.Criar(qualificar, "QL_QUALIFICADO",   "Lead qualificado",                  Classe.Avanco),
            Desfecho.Criar(qualificar, "QL_EM_ANDAMENTO",  "Contato em andamento",              Classe.Manutencao),
            Desfecho.Criar(qualificar, "QL_SEM_SUCESSO",   "Contato sem sucesso",               Classe.Manutencao),
            Desfecho.Criar(qualificar, "QL_SEM_PERFIL",    "Sem perfil para nossos produtos",   Classe.Perda,        exigeJustificativa: true),
            Desfecho.Criar(qualificar, "QL_SEM_INTERESSE", "Sem interesse no momento",          Classe.Perda,        exigeJustificativa: true),
            Desfecho.Criar(qualificar, "QL_DUPLICADO",     "Duplicado de cliente existente",    Classe.Cancelamento),
            Desfecho.Criar(qualificar, "QL_DADO_INVALIDO", "Dados de contato inválidos",        Classe.Cancelamento, exigeJustificativa: true),

            Desfecho.Criar(monitorar,  "MC_COM_INTERESSE", "Com interesse de compra",           Classe.Avanco),
            Desfecho.Criar(monitorar,  "MC_EM_ANDAMENTO",  "Contato em andamento",              Classe.Manutencao),
            Desfecho.Criar(monitorar,  "MC_SEM_SUCESSO",   "Contato sem sucesso",               Classe.Manutencao),
            Desfecho.Criar(monitorar,  "MC_INSATISFEITO",  "Cliente insatisfeito",              Classe.Manutencao),
            Desfecho.Criar(monitorar,  "MC_SEM_INTERESSE", "Sem interesse no período",          Classe.Perda),
            Desfecho.Criar(monitorar,  "MC_CANCELADO",     "Monitoramento encerrado",           Classe.Cancelamento)
            // … demais desfechos conforme a seção 5
        );

        // ---- 4. Regras ----
        db.Regras.AddRange(
            Regra.Criar(
                codigo: "R_LEAD_NOVO",
                nome: "Lead novo com carteira gera tarefa de qualificação",
                evento: EventosWorkflow.LeadRecebido,
                efeito: "CriarTarefa",
                expressaoDestinatario: "carteira.Responsavel",
                condicao: "lead.TemCarteira",
                efeitoTipoTarefaId: qualificar.Id,
                prazoDiasUteis: 1,
                ordem: 10),

            Regra.Criar(
                codigo: "R_LEAD_RECORRENTE",
                nome: "Lead conhecido volta a ser trabalhado após 90 dias",
                descricao: "[V] Corrige o limite VITALÍCIO da ação 897 do Vórtice: lá, 189 de 193 " +
                           "leads recorrentes foram bloqueados, e só 7 tinham tarefa aberta. " +
                           "Aqui a janela é temporal e configurável.",
                evento: EventosWorkflow.LeadRecebido,
                efeito: "CriarTarefa",
                expressaoDestinatario: "carteira.Responsavel",
                condicao: "lead.QualificacoesAnteriores > 0 && lead.DiasDesdeUltimaQualificacao >= 90",
                efeitoTipoTarefaId: qualificar.Id,
                prazoDiasUteis: 1,
                ordem: 15),

            Regra.Criar(
                codigo: "R_INTERESSE_ALTO_VALOR",
                nome: "Interesse acima de R$ 500 mil notifica o gestor",
                descricao: "A condição é o que, no Vórtice, exigiria uma regra duplicada por filial.",
                evento: EventosWorkflow.TarefaConcluida,
                efeito: "Notificar",
                expressaoDestinatario: "gestorDe(carteira.Responsavel)",
                condicao: "processo.ValorEstimado > 500000",
                ordem: 50)
            // … demais regras conforme a seção 6
        );

        await db.SaveChangesAsync(ct);
    }
}
```

---

## 9. Matriz de teste — o portão

O portão da fase 1 exige **cobertura de regra = 100%**: cada regra testada **disparando E não
disparando**. Isso são **38 casos** (19 regras × 2), mais os casos de erro.

```csharp
/// <summary>
/// Garante que toda regra do seed tem caso de teste nas DUAS direções.
/// Se alguém acrescentar regra e esquecer o teste, este teste falha e o merge é bloqueado.
/// </summary>
[Fact]
public async Task Toda_regra_do_catalogo_tem_teste_de_disparo_e_de_nao_disparo()
{
    var noBanco = await _db.Regras.Select(r => r.Codigo).ToListAsync();

    var comTeste = typeof(RegrasFase1Testes)
        .GetMethods()
        .SelectMany(m => m.GetCustomAttributes<RegraTestadaAttribute>())
        .GroupBy(a => a.Codigo)
        .Where(g => g.Any(a => a.Dispara) && g.Any(a => !a.Dispara))
        .Select(g => g.Key);

    noBanco.Except(comTeste).Should().BeEmpty(
        "toda regra precisa de um teste que prova que dispara E um que prova que não dispara");
}
```

**Cenários obrigatórios além das 19 regras:**

| # | Cenário | Espera |
|---|---|---|
| 1 | Lead sem carteira | `R_LEAD_SEM_CARTEIRA` dispara, `R_LEAD_NOVO` não — e as duas ficam registradas |
| 2 | Lead recorrente com 30 dias | `R_LEAD_RECORRENTE` **não** dispara, com motivo legível no log |
| 3 | Lead recorrente com 120 dias | dispara |
| 4 | Interesse de R$ 400 mil | `R_INTERESSE_ALTO_VALOR` não dispara, motivo cita o valor |
| 5 | Interesse de R$ 600 mil | dispara e notifica o gestor **atual** |
| 6 | Gestor trocado entre a criação e a execução | notifica o **novo** gestor |
| 7 | Quarta tentativa sem sucesso | `R_QUALIF_ESGOTOU` encerra em vez de gerar tarefa |
| 8 | Regra desligada | registrada como `RegraInativa`, nenhum efeito |
| 9 | Efeito inexistente na configuração | registrado como `Erro`, as outras regras seguem |
| 10 | Duas regras no mesmo gatilho | mesma `CorrelacaoId`, ordem respeitada |

O cenário **6** é o que prova a correção do defeito 2.5 do Vórtice: lá, a aprovação é gravada
para o líder do CEN **no momento da geração**, e trocar o líder **não reatribui** as agendas
já criadas.

---

## 10. Como o catálogo cresce

**Desfecho novo:** uma linha em `wf.Desfecho`. Se a classe for `Perda`, a regra
`R_PERDA_ENCERRA` já o trata — **sem regra nova**.

**Tarefa nova:** uma linha em `wf.TipoTarefa` + seus desfechos + as regras que a geram.

**Efeito novo** (ex.: "AbrirChamadoNoGLPI"): uma classe que implementa `IEfeitoRegra`.
Nenhum arquivo existente muda.

**Fluxo novo** (Venda, Demonstração, DSI): um `TipoProcesso` novo com seus estágios, tarefas,
desfechos e regras. **O motor não muda.** É essa propriedade que permite que as fases 3 e 4
sejam trabalho de configuração e tela, não de reescrita.

---

## 11. Higienização — a rotina que o Vórtice não tem

Um job mensal marca para revisão o que está morto:

```sql
-- Tipos de tarefa sem uso em 12 meses
SELECT Codigo, Nome, UltimoUsoEm
FROM wf.TipoTarefa
WHERE EstaAtivo = 1
  AND (UltimoUsoEm IS NULL OR UltimoUsoEm < DATEADD(month, -12, SYSUTCDATETIME()));

-- Regras que nunca dispararam desde que foram criadas
SELECT r.Codigo, r.Nome, r.CriadoEm
FROM wf.Regra r
WHERE r.EstaAtiva = 1
  AND NOT EXISTS (
      SELECT 1 FROM wf.RegraExecucao e
      WHERE e.RegraId = r.Id AND e.Resultado = 'Disparou');
```

> `[V]` Sem isso, o catálogo apodrece exatamente como o do Vórtice: **980 ações com 169 em uso**,
> **4.208 resultados com 501 em uso**, e **131 de 377 tabelas vazias**. Ninguém sabe o que pode
> apagar, então ninguém apaga nada, e o sistema fica cada vez mais difícil de entender.
