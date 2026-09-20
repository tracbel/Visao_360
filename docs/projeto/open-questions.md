# Questões em aberto — CRM Tracbel

Tudo o que ainda não tem resposta e que bloqueia ou condiciona uma decisão. Uma linha por item,
com o motivo pelo qual importa. Marcar `[x]` quando fechar, sem apagar a linha.

> ## ⚠️ As perguntas de infraestrutura deste arquivo estão SUPERADAS
>
> **Marcado em 20/09/2026** (contradição C-10 do documento 46A, issue #44). Este arquivo é de
> **02/09/2026** e as seções de infraestrutura abaixo — containers, Algar, Azure, VM Linux — foram
> respondidas **pelos fatos**, não por decisão de mesa:
>
> | O que o arquivo pergunta | O que foi medido depois |
> |---|---|
> | Algar opera VM Linux? Docker no contrato? Azure? | o CRM roda numa **VM OpenStack com Windows Server**, com **SQL Server nativo** e **sem Docker** — documento 35 §12 |
> | Container Linux no servidor | **impossível ali**: a VM não tem VMX, o WSL 2 não sobe (evento 41, "Hypervisor launch failed") — documento 35 §12.2 |
> | Onde hospedar | **decidido em 17/09/2026 (#60): continua no Windows Server**, com o porquê medido no documento 47 §6.1 |
> | Como o runtime chega ao servidor | publicação **self-contained**; o servidor não recebe runtime novo — documento 47 e issue #84 |
>
> **Não use este arquivo como fonte de pergunta aberta de infraestrutura.** As perguntas vivas de
> hoje estão no documento **46A §3**, e as de mercado na issue **#63**. O que está abaixo fica como
> registro do que se perguntava em 02/09 — apagar esconderia por que o documento 12 recomendou
> container antes de o servidor ser conhecido.

---

## Decisão de containers (doc 12) — 02/09/2026

**[SUPERADA — ver o aviso no topo.]** Enquanto estas estiverem abertas, a decisão do
[doc 12](12-DECISAO-CONTAINERS.md) permanece **recomendada, não travada**.

### Operação

- [ ] A Algar opera VMs Linux hoje? Qual distribuição e qual janela de patching? — **bloqueia a fase 0**: define se o degrau 1 é viável ou se a recomendação migra para Azure App Service
- [ ] Docker entra no escopo do contrato de gestão, ou o time da Tracbel opera os containers dentro de uma VM gerida? — define quem é acionado num incidente às 22h
- [ ] Quem é o plantão fora do horário comercial e qual o tempo de resposta contratado? — sem isso não há como prometer disponibilidade a partir da fase 3B
- [ ] Existe monitoramento de disco e de recursos no contrato? — a ausência disso produziu os 0,42 GB livres no terminal server (`PENDENCIAS.md`, seção I)

### Hospedagem e rede

- [ ] Datacenter da Algar, Azure ou híbrido? Há decisão corporativa de nuvem? — condiciona todas as escolhas de PaaS e o cálculo de custo
- [ ] Se houver Azure: existe ExpressRoute ou VPN até a rede da Tracbel, e a latência até o TOTVS Protheus é aceitável? — a fase 2 depende disso
- [ ] Podemos abrir 443 de entrada, e por qual caminho (WAF, balanceador, proxy da Algar)? — define a topologia de produção
- [ ] Qual o caminho de rede permitido até o banco do Vórtice (read-only) e até a `wsVorticeCrmApi`? — a camada de integração da fase 2 não sai do papel sem isso

### Banco

- [ ] VM dedicada ou Azure SQL Managed Instance? Se VM, quem faz o backup e com qual ferramenta? — o portão da fase 0 exige restauração testada com evidência
- [ ] A licença de SQL Server existente cobre a nova instância? Quantos núcleos, qual edição, há Software Assurance? — determina a mobilidade de licença e o custo
- [ ] Existe um DBA (interno ou Algar) para validar índice e plano de execução? — e para extrair os jobs do SQL Agent

### Segredos, identidade e ferramentas

- [ ] Existe Azure Key Vault no tenant, e quem administra? — define onde os segredos vivem em homolog e prod
- [ ] Repositório no GitHub ou no Azure DevOps? — decide o CI e o registry
- [ ] Qual registry de imagens: ACR, GHCR ou on-premises? — sem isso não há promoção de imagem entre ambientes
- [ ] Há política corporativa de imagem base ou de varredura de vulnerabilidade? — condiciona o critério 10 do portão da fase 0
- [ ] Qual o canal oficial de alarme (e-mail, Teams, plantão)? — um alarme sem destino é o defeito 3.4 do Vórtice repetido

### Custo

- [ ] Cotação real das VMs do degrau 1 com a Algar, e do equivalente em Azure — para substituir as faixas estimadas do doc 12, seção 16, por números com origem

---

## Programa por fases (doc 13) — 02/09/2026

### Dependências de terceiros (marco 0.5)

- [ ] **Jobs do SQL Agent do Vórtice**, hoje inacessíveis ao login de leitura (confirmado 02/09/2026) — há automação rodando em produção que não conseguimos ler. É o ponto cego declarado do programa e o risco R3; nenhum desligamento acontece sem esse inventário
- [ ] **Captura do SQL que o cliente Gupta executa** (Extended Events ou apoio do fornecedor) — é o único jeito de conhecer a regra que só existe dentro do executável
- [ ] **Documentação técnica do produto**, com o fornecedor do Vórtice — hoje é *nenhuma* (doc 01, seção 2)
- [ ] **Contrato de licenças**: os 5 módulos com `TipoAcesso = P` e sem versão instalada estão sendo cobrados? (doc 11, seção 1.2) — possível economia imediata, e ajuda a financiar a fase 0
- [ ] **Semântica dos 68 itens de política do `CRM_M001`** — é o que define o comportamento do CEN (doc 11, seção 7)

### Consolidação da investigação profunda

- [ ] Explicar a divergência de contagem de objetos: doc 01 registra **6 procedures / 1 function**; a extração de 02/09/2026 conta **85 procedures / 51 functions** — até resolver, vale o número maior, por ser o mais conservador
- [ ] Classificar os 548 objetos programáveis em *migrar / traduzir / descartar*
- [ ] Publicar o índice único que amarra `docs/dicionario/`, `docs/extracao-vortice/` e a pesquisa 16

### Decisões de negócio pendentes

- [ ] **Custo de pessoas do programa** — não está apurado em nenhum documento e é o maior item do orçamento (doc 13, seção 6)
- [ ] **Custo anual de manter o Vórtice** — não está no banco, só no contrato (doc 11, seção 7)
- [ ] Nomear **product owner de negócio** e **usuário-chave por fluxo** — é o risco R1, classificado como Alta desde o briefing
- [ ] **JD Edwards migra ou é aposentado?** — 7 tabelas com zero linhas hoje; decisão do negócio na fase 4
- [ ] **A cauda de 8 fluxos que somam 155 agendas/ano migra ou é aposentada?** (doc 10, seção 5)
- [ ] Dos **134 relatórios**, quais migram? Só 10 rodaram nos últimos 3 meses e 26 nunca rodaram (doc 01, seção 8)
- [ ] Confirmar com os donos de processo os **7 fluxos parados entre 1 e 2 anos** antes de descartá-los (doc 10, seção 6)

### Itens do sistema atual, independentes do programa

- [ ] Abrir com o fornecedor o incidente da **régua de cobrança parada desde 21/05/2025**, junto com o represamento de títulos de 22/05/2025 — é o mesmo incidente e são 15 meses (doc 10, seção 7)
- [ ] Confirmar a hipótese das **949 agendas de 2026 sem fluxo, 100% pendentes**, checando `IV_Agenda.TarefaCompromisso` (doc 10, seção 8.1)
- [ ] Backup do terminal server: a tarefa `\Algar\Backup do System State` está desabilitada e 22 GB desapareceram sem causa identificada (`PENDENCIAS.md`, seção I)
