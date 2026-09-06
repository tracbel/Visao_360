# Capturas de referência — protótipo do gerente

Baseline visual do protótipo original (`prototipo/referencia/`), capturado em 03/09/2026.

**Para que serve:** é o critério de aceite do port em React. A tela nova só está pronta quando
fica visualmente igual à captura correspondente. Diferença de pixel só é aceitável onde estiver
registrada e justificada.

**Como foram feitas:** Playwright + Chromium, viewport 1280×900, `deviceScaleFactor: 2`,
página inteira (`fullPage`), 1,4 s de espera após `networkidle` para os gráficos e o mapa
terminarem de desenhar. O protótipo foi servido por HTTP local (arquivo `file://` quebra o
Leaflet e as fontes).

**Como refazer** (o script fica em `scripts/prototipo/capturar-referencia.mjs`):

```bash
node scripts/prototipo/capturar-referencia.mjs
```

| Arquivo | Rota | Observação |
|---|---|---|
| `visao-360.png` | `#/` | dashboard executivo com 3 perfis, 5 KPIs, gauge, donut, linha 12 meses, barras |
| `agenda.png` | `#/agenda` | agenda do CEN |
| `cobertura.png` | `#/cobertura` | mapa Leaflet com pins por status + tabela de 23 clientes com filtros |
| `pipeline.png` | `#/pipeline` | pipeline de vendas |
| `clientes.png` | `#/clientes` | lista de clientes |
| `clientes_84391.png` | `#/clientes/84391` | ficha do cliente |
| `equipamentos_1RW7250PVMR123456.png` | `#/equipamentos/...` | ficha do equipamento |
| `oportunidades_1517613.png` | `#/oportunidades/1517613` | ficha da oportunidade |
| `oportunidades_nova.png` | `#/oportunidades/nova` | formulário de nova oportunidade |
| `equipamentos.png` | `#/equipamentos` | placeholder no original |
| `relatorios_funil.png` | `#/relatorios/funil` | funil de vendas |
| `relatorios_performance.png` | `#/relatorios/performance` | performance de CEN |
| `relatorios_cobertura.png` | `#/relatorios/cobertura` | cobertura regional |
| `config.png` | `#/config` | configurações |
| `inicio-antigo.png` | `#/inicio-antigo` | home antiga, mantida no original |

Nenhum erro de página foi registrado durante a captura.
