# IV-2 - Formularios, questionarios e propriedades customizadas

> Dicionario de dados do banco `CRM` (Vortice CRM / Tracbel). As contagens de linha sao do **snapshot de 03/06/2026** (`schema/*.csv`), nao do banco ao vivo.
>
> Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50. **Nao editar a mao** - reexecute o gerador.

**195 tabelas · 3.072 colunas · 602.428 linhas no snapshot.**

[Voltar ao indice](00-INDICE.md) · [Grafo de FKs](GRAFO-FK.md) · [Lacunas](LACUNAS.md)

## Tabelas neste arquivo

| Tabela | Classe | Colunas | Linhas |
|---|---|---:|---:|
| [`IV_AtribLista`](#iv_atriblista) | nucleo | 2 | 37 |
| [`IV_Atributo`](#iv_atributo) | nucleo | 11 | 10 |
| [`IV_ClienteAtrib`](#iv_clienteatrib) | isolada | 6 | 2.018 |
| [`IV_ClientePropCmpl`](#iv_clientepropcmpl) | vazia | 2 | 0 |
| [`IV_ClientePropr`](#iv_clientepropr) | nucleo | 51 | 246.684 |
| [`IV_ClientePropr_BKPJUN`](#iv_clientepropr_bkpjun) | lixo/backup | 51 | 1.000 |
| [`IV_ClientePropr_ITA`](#iv_clientepropr_ita) | lixo/backup | 51 | 157.900 |
| [`IV_Formulario`](#iv_formulario) | catalogo | 16 | 176 |
| [`IV_ListSQL`](#iv_listsql) | isolada | 5 | 360 |
| [`IV_Pcte`](#iv_pcte) | isolada | 3 | 1 |
| [`IV_PcteAtrFx`](#iv_pcteatrfx) | nucleo | 3 | 22 |
| [`IV_Propriedade`](#iv_propriedade) | catalogo | 70 | 52 |
| [`IV_Propriedade_BKPJUN`](#iv_propriedade_bkpjun) | lixo/backup | 70 | 21 |
| [`IV_Propriedade_ITA`](#iv_propriedade_ita) | lixo/backup | 70 | 32 |
| [`IV_PropriLista`](#iv_proprilista) | nucleo | 5 | 7.375 |
| [`IV_PropriListaLk`](#iv_proprilistalk) | vazia | 5 | 0 |
| [`IV_PropriLista_ITA`](#iv_proprilista_ita) | lixo/backup | 5 | 7.520 |
| [`IV_Questao`](#iv_questao) | nucleo | 19 | 2.300 |
| [`IV_QuestaoLista`](#iv_questaolista) | nucleo | 8 | 3.995 |
| [`IV_Questionario`](#iv_questionario) | nucleo | 18 | 85.393 |
| [`IV_Q_ABERTURA_OS_REVISAO`](#iv_q_abertura_os_revisao) | formulario-materializado | 3 | 10 |
| [`IV_Q_ACOMPANHAMENTO_VENDA`](#iv_q_acompanhamento_venda) | formulario-materializado | 53 | 1.715 |
| [`IV_Q_ACOMPANHAM_VENDA_IMP`](#iv_q_acompanham_venda_imp) | formulario-materializado | 39 | 3.308 |
| [`IV_Q_ACOMPANHA_COMPRA_IMP`](#iv_q_acompanha_compra_imp) | formulario-materializado | 16 | 26 |
| [`IV_Q_ACOMPANH_VENDA_JDE`](#iv_q_acompanh_venda_jde) | formulario-materializado | 104 | 6.343 |
| [`IV_Q_ACOMPAN_COMPRA_JDE`](#iv_q_acompan_compra_jde) | formulario-materializado | 17 | 153 |
| [`IV_Q_ACOMPAN_VEND_CONCESS`](#iv_q_acompan_vend_concess) | formulario-materializado | 9 | 98 |
| [`IV_Q_ACOMP_VENDA_DIRETA`](#iv_q_acomp_venda_direta) | formulario-materializado | 20 | 125 |
| [`IV_Q_ACOMP_VENDA_DIRETAJD`](#iv_q_acomp_venda_diretajd) | formulario-materializado | 94 | 270 |
| [`IV_Q_ACOMP_VENDA_FINANC`](#iv_q_acomp_venda_financ) | formulario-materializado | 9 | 2.021 |
| [`IV_Q_ACOMP_VENDA_LOCACAO`](#iv_q_acomp_venda_locacao) | formulario-materializado | 14 | 15 |
| [`IV_Q_ACOMP_VENDA_MANITOU`](#iv_q_acomp_venda_manitou) | formulario-materializado | 45 | 21 |
| [`IV_Q_ACOMP_VENDA_USADO`](#iv_q_acomp_venda_usado) | formulario-materializado | 44 | 256 |
| [`IV_Q_ACOMP_VEND_MAQUINAS`](#iv_q_acomp_vend_maquinas) | formulario-materializado | 54 | 1.535 |
| [`IV_Q_ADM_FINANCEIRO`](#iv_q_adm_financeiro) | formulario-materializado | 14 | 1.884 |
| [`IV_Q_AFERICAO_CSC`](#iv_q_afericao_csc) | formulario-materializado | 8 | 32 |
| [`IV_Q_AFERICAO_DE_PECAS`](#iv_q_afericao_de_pecas) | formulario-materializado | 8 | 2 |
| [`IV_Q_AFERICAO_IMPLEMENTO`](#iv_q_afericao_implemento) | formulario-materializado | 13 | 440 |
| [`IV_Q_AFERICAO_MAQ_1_CONT`](#iv_q_afericao_maq_1_cont) | formulario-materializado | 12 | 873 |
| [`IV_Q_AFERICAO_MAQ_1_WEB`](#iv_q_afericao_maq_1_web) | formulario-materializado | 9 | 2 |
| [`IV_Q_AFERICAO_MAQ_2_CONT`](#iv_q_afericao_maq_2_cont) | formulario-materializado | 8 | 541 |
| [`IV_Q_AFERICAO_MAQ_2_WEB`](#iv_q_afericao_maq_2_web) | formulario-materializado | 6 | 1 |
| [`IV_Q_AFERICAO_MAQ_3_CONT`](#iv_q_afericao_maq_3_cont) | formulario-materializado | 13 | 484 |
| [`IV_Q_AFERICAO_MAQ_3_WEB`](#iv_q_afericao_maq_3_web) | formulario-materializado | 12 | 3 |
| [`IV_Q_AFERICAO_PECAS`](#iv_q_afericao_pecas) | formulario-materializado | 33 | 1.719 |
| [`IV_Q_AFERICAO_POS_SOLUCAO`](#iv_q_afericao_pos_solucao) | formulario-materializado | 4 | 452 |
| [`IV_Q_AFERICAO_SERVICOS`](#iv_q_afericao_servicos) | formulario-materializado | 16 | 635 |
| [`IV_Q_AFERICAO_SERV_WEB`](#iv_q_afericao_serv_web) | formulario-materializado | 17 | 31 |
| [`IV_Q_AFERICAO_SUPORTE_INT`](#iv_q_afericao_suporte_int) | formulario-materializado | 2 | 0 |
| [`IV_Q_AFERICAO_VENDA_MAQ`](#iv_q_afericao_venda_maq) | formulario-materializado | 10 | 979 |
| [`IV_Q_AFERICAO_VENDA_MQ_IM`](#iv_q_afericao_venda_mq_im) | formulario-materializado | 32 | 604 |
| [`IV_Q_AFE_VENDA_MQ_IM_USAD`](#iv_q_afe_venda_mq_im_usad) | formulario-materializado | 26 | 1 |
| [`IV_Q_AGUARDAR_PECAS`](#iv_q_aguardar_pecas) | formulario-materializado | 2 | 31 |
| [`IV_Q_ALTERADO_PAGAMENTO`](#iv_q_alterado_pagamento) | formulario-materializado | 4 | 0 |
| [`IV_Q_APRESENTACAO`](#iv_q_apresentacao) | formulario-materializado | 8 | 211 |
| [`IV_Q_APRESENTACAO_JDE`](#iv_q_apresentacao_jde) | formulario-materializado | 8 | 123 |
| [`IV_Q_APRESENT_EQUIPAMENTO`](#iv_q_apresent_equipamento) | formulario-materializado | 1 | 0 |
| [`IV_Q_APRESENT_IMPLEMENTO`](#iv_q_apresent_implemento) | formulario-materializado | 7 | 43 |
| [`IV_Q_APROVACAO_TCSM`](#iv_q_aprovacao_tcsm) | formulario-materializado | 2 | 27 |
| [`IV_Q_ATUALIZACAO_PUK`](#iv_q_atualizacao_puk) | formulario-materializado | 10 | 0 |
| [`IV_Q_AVALIACAO_AMS_USADO`](#iv_q_avaliacao_ams_usado) | formulario-materializado | 6 | 4 |
| [`IV_Q_AVALIA_COLHEIT_USADA`](#iv_q_avalia_colheit_usada) | formulario-materializado | 51 | 1 |
| [`IV_Q_AVALIA_IMPLEM_USADO`](#iv_q_avalia_implem_usado) | formulario-materializado | 38 | 60 |
| [`IV_Q_AVALIA_USADO_ENTRADA`](#iv_q_avalia_usado_entrada) | formulario-materializado | 35 | 823 |
| [`IV_Q_AVAL_COLH_CANA_USADA`](#iv_q_aval_colh_cana_usada) | formulario-materializado | 32 | 1 |
| [`IV_Q_AVAL_TRATORES_USADOS`](#iv_q_aval_tratores_usados) | formulario-materializado | 53 | 0 |
| [`IV_Q_AVAL_USADO_ENTRADA`](#iv_q_aval_usado_entrada) | formulario-materializado | 12 | 200 |
| [`IV_Q_CADASTROS_LISTAS`](#iv_q_cadastros_listas) | formulario-materializado | 2 | 0 |
| [`IV_Q_CANCELAMENTO_SEGURO`](#iv_q_cancelamento_seguro) | formulario-materializado | 5 | 4 |
| [`IV_Q_CANCEL_RENOVACAO_SEG`](#iv_q_cancel_renovacao_seg) | formulario-materializado | 5 | 0 |
| [`IV_Q_CANHOTO_DIGITAL`](#iv_q_canhoto_digital) | formulario-materializado | 5 | 0 |
| [`IV_Q_CHASSI_ENTREGA_FISIC`](#iv_q_chassi_entrega_fisic) | formulario-materializado | 2 | 1.064 |
| [`IV_Q_CHASSI_EQUIPAMENTO`](#iv_q_chassi_equipamento) | formulario-materializado | 3 | 1.039 |
| [`IV_Q_CHASSI_PMP`](#iv_q_chassi_pmp) | formulario-materializado | 4 | 767 |
| [`IV_Q_CHEGADA_IMPLEMENTO`](#iv_q_chegada_implemento) | formulario-materializado | 2 | 61 |
| [`IV_Q_COMISSAO`](#iv_q_comissao) | formulario-materializado | 36 | 7.165 |
| [`IV_Q_COMISSAO_AMS`](#iv_q_comissao_ams) | formulario-materializado | 20 | 633 |
| [`IV_Q_COMISSAO_CONTACHAVE`](#iv_q_comissao_contachave) | formulario-materializado | 18 | 144 |
| [`IV_Q_COMISSAO_SERV_AMS`](#iv_q_comissao_serv_ams) | formulario-materializado | 15 | 448 |
| [`IV_Q_COMISSAO_USADO`](#iv_q_comissao_usado) | formulario-materializado | 5 | 240 |
| [`IV_Q_COMISSAO_VD_LOCACAO`](#iv_q_comissao_vd_locacao) | formulario-materializado | 13 | 0 |
| [`IV_Q_COMPETIDORES_NA_NEG`](#iv_q_competidores_na_neg) | formulario-materializado | 9 | 104 |
| [`IV_Q_COMPETIDORES_NA_NEGO`](#iv_q_competidores_na_nego) | formulario-materializado | 10 | 101 |
| [`IV_Q_COMPETID_NEGOC_IMPL`](#iv_q_competid_negoc_impl) | formulario-materializado | 9 | 13 |
| [`IV_Q_COM_INTERESSE_FUTURO`](#iv_q_com_interesse_futuro) | formulario-materializado | 9 | 0 |
| [`IV_Q_CONDICOES_DE_VENDAS`](#iv_q_condicoes_de_vendas) | formulario-materializado | 11 | 0 |
| [`IV_Q_CONT_COMISSAO_22`](#iv_q_cont_comissao_22) | formulario-materializado | 18 | 2.251 |
| [`IV_Q_COTA_CONSORCIO`](#iv_q_cota_consorcio) | formulario-materializado | 8 | 73 |
| [`IV_Q_DEMONSTRACAO_JD`](#iv_q_demonstracao_jd) | formulario-materializado | 33 | 215 |
| [`IV_Q_DEMONSTRACAO_LOG`](#iv_q_demonstracao_log) | formulario-materializado | 3 | 25 |
| [`IV_Q_DEMONSTRACAO_NF`](#iv_q_demonstracao_nf) | formulario-materializado | 2 | 44 |
| [`IV_Q_DEMONSTRACAO_TRATOR`](#iv_q_demonstracao_trator) | formulario-materializado | 25 | 32 |
| [`IV_Q_DEMONSTR_COLHEITAD`](#iv_q_demonstr_colheitad) | formulario-materializado | 8 | 0 |
| [`IV_Q_DEMO_EQUIP_JD`](#iv_q_demo_equip_jd) | formulario-materializado | 23 | 0 |
| [`IV_Q_DEMO_IMPLEMENTO`](#iv_q_demo_implemento) | formulario-materializado | 25 | 0 |
| [`IV_Q_DEMO_MAQUINAS`](#iv_q_demo_maquinas) | formulario-materializado | 19 | 109 |
| [`IV_Q_DEMO_TRATOR`](#iv_q_demo_trator) | formulario-materializado | 26 | 43 |
| [`IV_Q_DEVOLUCAO_PECA`](#iv_q_devolucao_peca) | formulario-materializado | 2 | 581 |
| [`IV_Q_DEVOLUCAO_PUK`](#iv_q_devolucao_puk) | formulario-materializado | 2 | 5 |
| [`IV_Q_DOC_ANALISE_CREDITO`](#iv_q_doc_analise_credito) | formulario-materializado | 28 | 21 |
| [`IV_Q_EVENTOS_AFERICAO`](#iv_q_eventos_afericao) | formulario-materializado | 12 | 67 |
| [`IV_Q_EXP_FLUXO_MODELER`](#iv_q_exp_fluxo_modeler) | formulario-materializado | 3 | 0 |
| [`IV_Q_FORA_SERVICO_PMP`](#iv_q_fora_servico_pmp) | formulario-materializado | 5 | 0 |
| [`IV_Q_FORM_TREINO`](#iv_q_form_treino) | formulario-materializado | 1 | 0 |
| [`IV_Q_GAR_DATA_SERVICO`](#iv_q_gar_data_servico) | formulario-materializado | 3 | 4.888 |
| [`IV_Q_GAR_FAB_SOL_PECA`](#iv_q_gar_fab_sol_peca) | formulario-materializado | 2 | 640 |
| [`IV_Q_GESTAO_CREDITO`](#iv_q_gestao_credito) | formulario-materializado | 54 | 1.506 |
| [`IV_Q_GESTAO_CREDITO_AMS`](#iv_q_gestao_credito_ams) | formulario-materializado | 51 | 175 |
| [`IV_Q_GESTAO_CREDITO_IMP`](#iv_q_gestao_credito_imp) | formulario-materializado | 51 | 807 |
| [`IV_Q_GESTAO_PRODUTO_IMPL`](#iv_q_gestao_produto_impl) | formulario-materializado | 23 | 813 |
| [`IV_Q_GESTAO_PRODUTO___AMS`](#iv_q_gestao_produto___ams) | formulario-materializado | 39 | 177 |
| [`IV_Q_HORIMETRO_AGREGA`](#iv_q_horimetro_agrega) | formulario-materializado | 2 | 1 |
| [`IV_Q_INCENTIVO`](#iv_q_incentivo) | formulario-materializado | 43 | 2.079 |
| [`IV_Q_INTERESSE_FUTURO_PRO`](#iv_q_interesse_futuro_pro) | formulario-materializado | 5 | 10 |
| [`IV_Q_INTERESSE_PROJETO_IR`](#iv_q_interesse_projeto_ir) | formulario-materializado | 9 | 25 |
| [`IV_Q_LIBERAR_DEMONSTRACAO`](#iv_q_liberar_demonstracao) | formulario-materializado | 5 | 44 |
| [`IV_Q_LICENCAS_PUK`](#iv_q_licencas_puk) | formulario-materializado | 12 | 41 |
| [`IV_Q_LOCACAO_COMISSAO`](#iv_q_locacao_comissao) | formulario-materializado | 13 | 15 |
| [`IV_Q_OFERECE_RENOV_SEGURO`](#iv_q_oferece_renov_seguro) | formulario-materializado | 16 | 846 |
| [`IV_Q_ORIGEM_DA_RENDA`](#iv_q_origem_da_renda) | formulario-materializado | 2 | 0 |
| [`IV_Q_OS_ABERTA`](#iv_q_os_aberta) | formulario-materializado | 3 | 771 |
| [`IV_Q_OS_CORTESIA`](#iv_q_os_cortesia) | formulario-materializado | 5 | 78 |
| [`IV_Q_OS_GARANTIA`](#iv_q_os_garantia) | formulario-materializado | 5 | 7.786 |
| [`IV_Q_OS_REVISAO_ENTREGA`](#iv_q_os_revisao_entrega) | formulario-materializado | 5 | 0 |
| [`IV_Q_PECAS_AFERICAO`](#iv_q_pecas_afericao) | formulario-materializado | 23 | 5.174 |
| [`IV_Q_PEDIDO_GC`](#iv_q_pedido_gc) | formulario-materializado | 12 | 0 |
| [`IV_Q_PEDIDO_KAM`](#iv_q_pedido_kam) | formulario-materializado | 28 | 12 |
| [`IV_Q_PEDIDO_SAM`](#iv_q_pedido_sam) | formulario-materializado | 19 | 46 |
| [`IV_Q_PERCEPCAO_JD`](#iv_q_percepcao_jd) | formulario-materializado | 17 | 32 |
| [`IV_Q_PESQUISA_NPS`](#iv_q_pesquisa_nps) | formulario-materializado | 2 | 0 |
| [`IV_Q_PESQUISA_TI`](#iv_q_pesquisa_ti) | formulario-materializado | 4 | 0 |
| [`IV_Q_PREMIO_DEMO`](#iv_q_premio_demo) | formulario-materializado | 4 | 21 |
| [`IV_Q_PREVISAO_RECEBIMENTO`](#iv_q_previsao_recebimento) | formulario-materializado | 2 | 4 |
| [`IV_Q_PRODUTO_RD`](#iv_q_produto_rd) | formulario-materializado | 8 | 135 |
| [`IV_Q_PROPOSTA_COMERCIAL`](#iv_q_proposta_comercial) | formulario-materializado | 10 | 24 |
| [`IV_Q_PROSPECCAO_SERV__JD`](#iv_q_prospeccao_serv__jd) | formulario-materializado | 6 | 104 |
| [`IV_Q_QUALIDADE_PECAS`](#iv_q_qualidade_pecas) | formulario-materializado | 6 | 642 |
| [`IV_Q_QUALIDADE_SERVICOS`](#iv_q_qualidade_servicos) | formulario-materializado | 6 | 345 |
| [`IV_Q_QUALIDADE_VENDAMAQ`](#iv_q_qualidade_vendamaq) | formulario-materializado | 5 | 141 |
| [`IV_Q_RECEBIMENTO_A_PRAZO`](#iv_q_recebimento_a_prazo) | formulario-materializado | 17 | 16 |
| [`IV_Q_RECEBIMENTO_COMISSAO`](#iv_q_recebimento_comissao) | formulario-materializado | 3 | 0 |
| [`IV_Q_RECEBIMENTO_FINANC`](#iv_q_recebimento_financ) | formulario-materializado | 2 | 4 |
| [`IV_Q_RECEB_FINAN_IMPLEM`](#iv_q_receb_finan_implem) | formulario-materializado | 2 | 0 |
| [`IV_Q_RESPONSAVEL_TECNICO`](#iv_q_responsavel_tecnico) | formulario-materializado | 2 | 2.115 |
| [`IV_Q_RESULTADO_DEMO`](#iv_q_resultado_demo) | formulario-materializado | 15 | 23 |
| [`IV_Q_RETORNADO_JD`](#iv_q_retornado_jd) | formulario-materializado | 2 | 773 |
| [`IV_Q_REVISAO_100H`](#iv_q_revisao_100h) | formulario-materializado | 3 | 1 |
| [`IV_Q_REVISAO_1100_1150H`](#iv_q_revisao_1100_1150h) | formulario-materializado | 3 | 1 |
| [`IV_Q_REVISAO_1500H`](#iv_q_revisao_1500h) | formulario-materializado | 3 | 0 |
| [`IV_Q_REVISAO_450_600H`](#iv_q_revisao_450_600h) | formulario-materializado | 3 | 1 |
| [`IV_Q_REVISAO_800H`](#iv_q_revisao_800h) | formulario-materializado | 3 | 0 |
| [`IV_Q_REVISAO_FIM_GARANTIA`](#iv_q_revisao_fim_garantia) | formulario-materializado | 3 | 0 |
| [`IV_Q_REV_DATA_SERVICO`](#iv_q_rev_data_servico) | formulario-materializado | 3 | 74 |
| [`IV_Q_ROMANEIO_DEV_PECA`](#iv_q_romaneio_dev_peca) | formulario-materializado | 4 | 1 |
| [`IV_Q_SEPARACAO_PEDIDO`](#iv_q_separacao_pedido) | formulario-materializado | 18 | 2 |
| [`IV_Q_SERVICOS_AFERICAO`](#iv_q_servicos_afericao) | formulario-materializado | 18 | 2.204 |
| [`IV_Q_SERVICO_EXTERNOS_JD`](#iv_q_servico_externos_jd) | formulario-materializado | 9 | 1.662 |
| [`IV_Q_SOLICITACAO_TCAT`](#iv_q_solicitacao_tcat) | formulario-materializado | 22 | 1 |
| [`IV_Q_TESTE1`](#iv_q_teste1) | formulario-materializado | 3 | 2 |
| [`IV_Q_TESTE2`](#iv_q_teste2) | formulario-materializado | 6 | 1 |
| [`IV_Q_TESTE_PRIMEIRO_JD`](#iv_q_teste_primeiro_jd) | formulario-materializado | 3 | 3 |
| [`IV_Q_TICKET_DSI`](#iv_q_ticket_dsi) | formulario-materializado | 8 | 1 |
| [`IV_Q_VENDA`](#iv_q_venda) | formulario-materializado | 22 | 2.806 |
| [`IV_Q_VENDAPERDIDA_SEGURO`](#iv_q_vendaperdida_seguro) | formulario-materializado | 4 | 12 |
| [`IV_Q_VENDA_AMS`](#iv_q_venda_ams) | formulario-materializado | 65 | 775 |
| [`IV_Q_VENDA_CONSORCIO`](#iv_q_venda_consorcio) | formulario-materializado | 7 | 47 |
| [`IV_Q_VENDA_DIRETA`](#iv_q_venda_direta) | formulario-materializado | 12 | 0 |
| [`IV_Q_VENDA_DSI`](#iv_q_venda_dsi) | formulario-materializado | 16 | 1 |
| [`IV_Q_VENDA_EQUIPAMENTO`](#iv_q_venda_equipamento) | formulario-materializado | 49 | 2.256 |
| [`IV_Q_VENDA_MAQUINA_FY25`](#iv_q_venda_maquina_fy25) | formulario-materializado | 45 | 0 |
| [`IV_Q_VENDA_PERDIDA`](#iv_q_venda_perdida) | formulario-materializado | 14 | 1.511 |
| [`IV_Q_VENDA_PERDIDA_FY25`](#iv_q_venda_perdida_fy25) | formulario-materializado | 13 | 198 |
| [`IV_Q_VENDA_PERDIDA_IMPL`](#iv_q_venda_perdida_impl) | formulario-materializado | 10 | 31 |
| [`IV_Q_VENDA_PERDIDA_IMPLEM`](#iv_q_venda_perdida_implem) | formulario-materializado | 13 | 125 |
| [`IV_Q_VENDA_PERDIDA_JDE`](#iv_q_venda_perdida_jde) | formulario-materializado | 11 | 296 |
| [`IV_Q_VENDA_PERDIDA_MANITO`](#iv_q_venda_perdida_manito) | formulario-materializado | 10 | 6 |
| [`IV_Q_VENDA_PERDIDA_MAQIMP`](#iv_q_venda_perdida_maqimp) | formulario-materializado | 12 | 313 |
| [`IV_Q_VENDA_PERDIDA_PROD`](#iv_q_venda_perdida_prod) | formulario-materializado | 15 | 286 |
| [`IV_Q_VENDA_PERDIDA_SEGURO`](#iv_q_venda_perdida_seguro) | formulario-materializado | 13 | 13 |
| [`IV_Q_VENDA_PERDIDA_TESTE`](#iv_q_venda_perdida_teste) | formulario-materializado | 12 | 0 |
| [`IV_Q_VENDA_PNEUS`](#iv_q_venda_pneus) | formulario-materializado | 7 | 9 |
| [`IV_Q_VENDA_PRECISION_UP`](#iv_q_venda_precision_up) | formulario-materializado | 8 | 2 |
| [`IV_Q_VENDA_SEMINOVO`](#iv_q_venda_seminovo) | formulario-materializado | 10 | 2.481 |
| [`IV_Q_VENDA_SERVICOS_AMS`](#iv_q_venda_servicos_ams) | formulario-materializado | 8 | 468 |
| [`IV_Q_VENDA_VP_PNEUS`](#iv_q_venda_vp_pneus) | formulario-materializado | 6 | 4 |
| [`IV_Q_VENDER_RENOVACAO_SEG`](#iv_q_vender_renovacao_seg) | formulario-materializado | 16 | 153 |
| [`IV_Q_VISITA_DSI`](#iv_q_visita_dsi) | formulario-materializado | 4 | 4 |
| [`IV_Q_VISITA_EXP_CLIENTE`](#iv_q_visita_exp_cliente) | formulario-materializado | 11 | 16 |
| [`IV_Q_VP_COLHEDORA`](#iv_q_vp_colhedora) | formulario-materializado | 11 | 1 |
| [`IV_Q_VP_COLHEITADEIRA`](#iv_q_vp_colheitadeira) | formulario-materializado | 12 | 3 |
| [`IV_Q_VP_PLANTADEIRA`](#iv_q_vp_plantadeira) | formulario-materializado | 13 | 3 |
| [`IV_Q_VP_PULVERIZADOR`](#iv_q_vp_pulverizador) | formulario-materializado | 10 | 0 |
| [`IV_Q_VP_RENOVACAO_SEGURO`](#iv_q_vp_renovacao_seguro) | formulario-materializado | 4 | 0 |
| [`IV_Q_VP_SEM_PARTICIPACAO`](#iv_q_vp_sem_participacao) | formulario-materializado | 12 | 74 |
| [`IV_Q_VP_TRATOR`](#iv_q_vp_trator) | formulario-materializado | 12 | 19 |

---

### IV_AtribLista

`classe: nucleo` · `2 colunas` · `37 linhas (snapshot 03/06/2026)` · `PK: Atributo, Lista`

**Funcao:** _(inferido)_ Pelo nome, e uma lista de opcoes de dominio relacionada a propriedade/atributo customizado, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Atributo` | `varchar(25)` | **nao** | - | **PK**; FK -> `IV_Atributo.Atributo` |
| 2 | `Lista` | `varchar(30)` | **nao** | - | **PK** |

---

### IV_Atributo

`classe: nucleo` · `11 colunas` · `10 linhas (snapshot 03/06/2026)` · `PK: Atributo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de propriedade/atributo customizado, no modulo `IV` (nucleo CRM/BPM).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Atributo` | `varchar(25)` | **nao** | - | **PK** |
| 2 | `UsaFeminino` | `char(1)` | **nao** | - |  |
| 3 | `UsaMasculino` | `char(1)` | **nao** | - |  |
| 4 | `UsaPessoaJuridica` | `char(1)` | **nao** | - |  |
| 5 | `UsaContato` | `char(1)` | **nao** | - |  |
| 6 | `Principal` | `char(1)` | **nao** | - |  |
| 7 | `TipoDado` | `char(1)` | **nao** | - |  |
| 8 | `Obrigatorio` | `char(1)` | **nao** | - |  |
| 9 | `Validade` | `decimal(3,0)` | sim | - |  |
| 10 | `Ordem` | `decimal(3,0)` | sim | - |  |
| 11 | `SeqAtributo` | `numeric(6,0)` | sim | - |  |

**Referenciada por:** `IV_AtribLista.Atributo`

---

### IV_ClienteAtrib

`classe: isolada` · `6 colunas` · `2.018 linhas (snapshot 03/06/2026)` · `PK: SeqPessoa, Atributo`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de propriedade/atributo customizado, no modulo `IV` (nucleo CRM/BPM). As colunas confirmam vinculo com pessoa (`SeqPessoa`).

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | **PK**; pessoa (`GE_Pessoa.SeqPessoa`) |
| 2 | `Atributo` | `varchar(25)` | **nao** | - | **PK** |
| 3 | `Lista` | `varchar(30)` | sim | - |  |
| 4 | `DtaAtualizacao` | `datetime` | sim | - |  |
| 5 | `DtaValidade` | `datetime` | sim | - |  |
| 6 | `UsuAlterou` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |

---

### IV_ClientePropCmpl

`classe: vazia` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPropPessoa`

**Funcao:** _(inferido)_ Pelo nome, e um complemento do registro pai, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPropPessoa` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_ClientePropr.SeqPropPessoa` |
| 2 | `Complemento` | `text(2147483647)` | sim | - |  |

---

### IV_ClientePropr

`classe: nucleo` · `51 colunas` · `246.684 linhas (snapshot 03/06/2026)` · `PK: SeqPropPessoa`

**Funcao:** Construtor de entidades customizadas por cliente (o análogo de 'GE_PessoaProp'). IV_Propriedade (52 tipos) define os rótulos dos slots; IV_ClientePropr (246.813 linhas / 55.544 pessoas, ativo em ago/2026) guarda as instâncias. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPropPessoa` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(10,0)` | **nao** | - | FK -> `GE_Pessoa.SeqPessoa`; pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `SeqPropriedade` | `numeric(4,0)` | **nao** | - | FK -> `IV_Propriedade.SeqPropriedade` |
| 4 | `Referencia` | `varchar(30)` | sim | - |  |
| 5 | `Identificador` | `varchar(30)` | sim | - |  |
| 6 | `Ativo` | `char(1)` | sim | - |  |
| 7 | `Notas` | `varchar(250)` | sim | - |  |
| 8 | `Campo1` | `varchar(40)` | sim | - |  |
| 9 | `Campo2` | `varchar(40)` | sim | - |  |
| 10 | `Campo3` | `varchar(40)` | sim | - |  |
| 11 | `Campo4` | `varchar(40)` | sim | - |  |
| 12 | `Campo5` | `varchar(40)` | sim | - |  |
| 13 | `Campo6` | `varchar(40)` | sim | - |  |
| 14 | `Numero1` | `decimal(15,2)` | sim | - |  |
| 15 | `Numero2` | `decimal(15,2)` | sim | - |  |
| 16 | `Numero3` | `decimal(15,2)` | sim | - |  |
| 17 | `Numero4` | `decimal(15,2)` | sim | - |  |
| 18 | `Numero5` | `decimal(15,2)` | sim | - |  |
| 19 | `Numero6` | `decimal(15,2)` | sim | - |  |
| 20 | `Data1` | `datetime` | sim | - |  |
| 21 | `Data2` | `datetime` | sim | - |  |
| 22 | `Data3` | `datetime` | sim | - |  |
| 23 | `Data4` | `datetime` | sim | - |  |
| 24 | `Data5` | `datetime` | sim | - |  |
| 25 | `Data6` | `datetime` | sim | - |  |
| 26 | `SimNao1` | `numeric(1,0)` | sim | - |  |
| 27 | `SimNao2` | `numeric(1,0)` | sim | - |  |
| 28 | `SimNao3` | `numeric(1,0)` | sim | - |  |
| 29 | `SimNao4` | `numeric(1,0)` | sim | - |  |
| 30 | `SimNao5` | `numeric(1,0)` | sim | - |  |
| 31 | `SimNao6` | `numeric(1,0)` | sim | - |  |
| 32 | `Literal1` | `varchar(40)` | sim | - |  |
| 33 | `Literal2` | `varchar(40)` | sim | - |  |
| 34 | `Literal3` | `varchar(40)` | sim | - |  |
| 35 | `Literal4` | `varchar(40)` | sim | - |  |
| 36 | `Literal5` | `varchar(40)` | sim | - |  |
| 37 | `Literal6` | `varchar(40)` | sim | - |  |
| 38 | `Literal7` | `varchar(40)` | sim | - |  |
| 39 | `Literal8` | `varchar(40)` | sim | - |  |
| 40 | `Literal9` | `varchar(40)` | sim | - |  |
| 41 | `Literal10` | `varchar(40)` | sim | - |  |
| 42 | `CodOrigem` | `varchar(20)` | sim | - |  |
| 43 | `UltOrigem` | `varchar(20)` | sim | - | sistema de origem do dado |
| 44 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 45 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 46 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 47 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 48 | `CAMPO7` | `varchar(40)` | sim | - |  |
| 49 | `CAMPO8` | `varchar(40)` | sim | - |  |
| 50 | `CAMPO7SQL` | `numeric(1,0)` | sim | - |  |
| 51 | `CAMPO8SQL` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `DMN_DocProp.SeqPropPessoa`, `IV_ClientePropCmpl.SeqPropPessoa`

---

### IV_ClientePropr_BKPJUN

`classe: lixo/backup` · `51 colunas` · `1.000 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (51 colunas) para manter o documento legivel._

---

### IV_ClientePropr_ITA

`classe: lixo/backup` · `51 colunas` · `157.900 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (51 colunas) para manter o documento legivel._

---

### IV_Formulario

`classe: catalogo` · `16 colunas` · `176 linhas (snapshot 03/06/2026)` · `PK: SeqFormulario`

**Funcao:** Construtor de formulários: 176 formulários, 2.303 perguntas tipadas com ramificação condicional e peso, 3.995 opções de lista, 85 mil questionários respondidos — materializados em 175 TABELAS FÍSICAS IV_Q_* e 175 views IV_Q$*. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFormulario` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `Pcte` | `varchar(4)` | sim | - |  |
| 3 | `Descricao` | `varchar(20)` | sim | - | descricao do registro |
| 4 | `Objetivo` | `varchar(40)` | sim | - |  |
| 5 | `Script` | `varchar(500)` | sim | - |  |
| 6 | `PrimeiraQuestao` | `decimal(2,0)` | sim | - |  |
| 7 | `EmUso` | `char(1)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 8 | `Layout` | `char(1)` | sim | - |  |
| 9 | `UsaObs` | `numeric(1,0)` | sim | - |  |
| 10 | `QuestaoGuia` | `decimal(4,0)` | sim | - |  |
| 11 | `Restricao` | `char(1)` | sim | - |  |
| 12 | `IndUsoPessoa` | `numeric(1,0)` | sim | - |  |
| 13 | `INDUSOPROJETO` | `numeric(1,0)` | sim | - |  |
| 14 | `INDUSOPROPRIEDADE` | `numeric(1,0)` | sim | - |  |
| 15 | `INDUMPORPESSOA` | `numeric(1,0)` | sim | - |  |
| 16 | `INDUSAASSINATURA` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `IV_Acao.SeqFormulario`, `IV_Questao.SeqFormulario`, `IV_Questionario.SeqFormulario`, `IV_ResMsgPapel.PesFormulario`

---

### IV_ListSQL

`classe: isolada` · `5 colunas` · `360 linhas (snapshot 03/06/2026)` · `PK: Tipo, SeqMain, SeqSub`

**Funcao:** E EVITAR o SQL guardado em text (IV_ListSQL) como fonte de combo — é acoplamento ao schema e superfície de injeção. (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Tipo` | `varchar(10)` | **nao** | - | **PK** |
| 2 | `SeqMain` | `numeric(18,0)` | **nao** | - | **PK** |
| 3 | `SeqSub` | `numeric(18,0)` | **nao** | - | **PK** |
| 4 | `Sql` | `text(2147483647)` | sim | - |  |
| 5 | `Sessao` | `varchar(20)` | sim | - |  |

---

### IV_Pcte

`classe: isolada` · `3 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: NroPcte`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `NroPcte` | `decimal(2,0)` | **nao** | - | **PK** |
| 2 | `Pcte` | `varchar(4)` | sim | - |  |
| 3 | `Descr` | `varchar(30)` | sim | - |  |

---

### IV_PcteAtrFx

`classe: nucleo` · `3 colunas` · `22 linhas (snapshot 03/06/2026)` · `PK: Pcte, Atributo, Lista`

**Funcao:** (nao documentado - apurar com acesso ao vivo)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `Pcte` | `varchar(4)` | **nao** | - | **PK** |
| 2 | `Atributo` | `varchar(12)` | **nao** | - | **PK**; FK -> `GE_AtributoFixo.Atributo` |
| 3 | `Lista` | `varchar(30)` | **nao** | - | **PK**; FK -> `GE_AtributoFixo.Lista` |

---

### IV_Propriedade

`classe: catalogo` · `70 colunas` · `52 linhas (snapshot 03/06/2026)` · `PK: SeqPropriedade`

**Funcao:** Construtor de entidades customizadas por cliente (o análogo de 'GE_PessoaProp'). IV_Propriedade (52 tipos) define os rótulos dos slots; IV_ClientePropr (246.813 linhas / 55.544 pessoas, ativo em ago/2026) guarda as instâncias. (fonte: `06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPropriedade` | `numeric(4,0)` | **nao** | - | **PK** |
| 2 | `Propriedade` | `varchar(20)` | **nao** | - |  |
| 3 | `Pcte` | `varchar(4)` | sim | - |  |
| 4 | `Nivel` | `decimal(1,0)` | sim | - |  |
| 5 | `NivelIdentificador` | `decimal(1,0)` | sim | - |  |
| 6 | `CampoListar` | `varchar(15)` | sim | - |  |
| 7 | `MostraRef` | `char(1)` | sim | - |  |
| 8 | `Usacomplemento` | `char(1)` | sim | - |  |
| 9 | `TipoRefVinculado` | `varchar(15)` | sim | - |  |
| 10 | `UmPorPessoa` | `numeric(1,0)` | sim | - |  |
| 11 | `ReferenciaSQL` | `numeric(1,0)` | sim | - |  |
| 12 | `Campo1` | `varchar(20)` | sim | - |  |
| 13 | `Campo1Sql` | `numeric(1,0)` | sim | - |  |
| 14 | `Campo2` | `varchar(20)` | sim | - |  |
| 15 | `Campo2Sql` | `numeric(1,0)` | sim | - |  |
| 16 | `Campo3` | `varchar(20)` | sim | - |  |
| 17 | `Campo3Sql` | `numeric(1,0)` | sim | - |  |
| 18 | `Campo4` | `varchar(20)` | sim | - |  |
| 19 | `Campo4Sql` | `numeric(1,0)` | sim | - |  |
| 20 | `Campo5` | `varchar(20)` | sim | - |  |
| 21 | `Campo5Sql` | `numeric(1,0)` | sim | - |  |
| 22 | `Campo6` | `varchar(20)` | sim | - |  |
| 23 | `Campo6Sql` | `numeric(1,0)` | sim | - |  |
| 24 | `Numero1` | `varchar(20)` | sim | - |  |
| 25 | `Numero2` | `varchar(20)` | sim | - |  |
| 26 | `Numero3` | `varchar(20)` | sim | - |  |
| 27 | `Numero4` | `varchar(20)` | sim | - |  |
| 28 | `Numero5` | `varchar(20)` | sim | - |  |
| 29 | `Numero6` | `varchar(20)` | sim | - |  |
| 30 | `Data1` | `varchar(20)` | sim | - |  |
| 31 | `Data2` | `varchar(20)` | sim | - |  |
| 32 | `Data3` | `varchar(20)` | sim | - |  |
| 33 | `Data4` | `varchar(20)` | sim | - |  |
| 34 | `Data5` | `varchar(20)` | sim | - |  |
| 35 | `Data6` | `varchar(20)` | sim | - |  |
| 36 | `SimNao1` | `varchar(30)` | sim | - |  |
| 37 | `SimNao2` | `varchar(30)` | sim | - |  |
| 38 | `SimNao3` | `varchar(30)` | sim | - |  |
| 39 | `SimNao4` | `varchar(30)` | sim | - |  |
| 40 | `SimNao5` | `varchar(30)` | sim | - |  |
| 41 | `SimNao6` | `varchar(30)` | sim | - |  |
| 42 | `Literal1` | `varchar(20)` | sim | - |  |
| 43 | `Literal2` | `varchar(20)` | sim | - |  |
| 44 | `Literal3` | `varchar(20)` | sim | - |  |
| 45 | `Literal4` | `varchar(20)` | sim | - |  |
| 46 | `Literal5` | `varchar(20)` | sim | - |  |
| 47 | `Literal6` | `varchar(20)` | sim | - |  |
| 48 | `Literal7` | `varchar(20)` | sim | - |  |
| 49 | `Literal8` | `varchar(20)` | sim | - |  |
| 50 | `Literal9` | `varchar(20)` | sim | - |  |
| 51 | `Literal10` | `varchar(20)` | sim | - |  |
| 52 | `DtaInclusao` | `datetime` | sim | - | auditoria de inclusao (data) |
| 53 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 54 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 55 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 56 | `ReferenciaDesc` | `varchar(20)` | sim | - |  |
| 57 | `Literal1Sql` | `numeric(1,0)` | sim | - |  |
| 58 | `Literal2Sql` | `numeric(1,0)` | sim | - |  |
| 59 | `Literal3Sql` | `numeric(1,0)` | sim | - |  |
| 60 | `Literal4Sql` | `numeric(1,0)` | sim | - |  |
| 61 | `Literal5Sql` | `numeric(1,0)` | sim | - |  |
| 62 | `Literal6Sql` | `numeric(1,0)` | sim | - |  |
| 63 | `Literal7Sql` | `numeric(1,0)` | sim | - |  |
| 64 | `Literal8Sql` | `numeric(1,0)` | sim | - |  |
| 65 | `Literal9Sql` | `numeric(1,0)` | sim | - |  |
| 66 | `Literal10Sql` | `numeric(1,0)` | sim | - |  |
| 67 | `CAMPO7` | `varchar(22)` | sim | - |  |
| 68 | `CAMPO8` | `varchar(22)` | sim | - |  |
| 69 | `CAMPO7SQL` | `numeric(1,0)` | sim | - |  |
| 70 | `CAMPO8SQL` | `numeric(1,0)` | sim | - |  |

**Referenciada por:** `EXT_VeicFam.SeqPropriedade`, `GE_MOBILEPROP.SEQPROPRIEDADE`, `GE_MobileConfig.SeqPropriedade`, `IV_ClientePropr.SeqPropriedade`, `IV_PropriLista.SeqPropriedade`

---

### IV_Propriedade_BKPJUN

`classe: lixo/backup` · `70 colunas` · `21 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de propriedade/atributo customizado, no modulo `IV` (nucleo CRM/BPM).

> **Nao usar em producao.** Motivo da classificacao: copia de backup manual (padrao `*_BKP*`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (70 colunas) para manter o documento legivel._

---

### IV_Propriedade_ITA

`classe: lixo/backup` · `70 colunas` · `32 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de propriedade/atributo customizado, no modulo `IV` (nucleo CRM/BPM).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (70 colunas) para manter o documento legivel._

---

### IV_PropriLista

`classe: nucleo` · `5 colunas` · `7.375 linhas (snapshot 03/06/2026)` · `PK: SeqPropriedade, Campo, SeqPropriLista`

**Funcao:** **Relacoes:** Listas em IV_PropriLista (7.375) e IV_ListSQL (Tipo='PROPRIED', 54). (fonte: `04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPropriedade` | `numeric(4,0)` | **nao** | - | **PK**; FK -> `IV_Propriedade.SeqPropriedade` |
| 2 | `Campo` | `varchar(2)` | **nao** | - | **PK** |
| 3 | `SeqPropriLista` | `decimal(4,0)` | **nao** | - | **PK** |
| 4 | `Lista` | `varchar(40)` | sim | - |  |
| 5 | `Ordem` | `smallint(5,0)` | sim | - |  |

**Referenciada por:** `IV_PropriListaLk.Campo`, `IV_PropriListaLk.SeqPropriLista`, `IV_PropriListaLk.SeqPropriedade`

---

### IV_PropriListaLk

`classe: vazia` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SeqPropriedade, Campo, SeqPropriLista, SeqPropListaLk`

**Funcao:** _(inferido)_ Pelo nome, guarda dados de propriedade/atributo customizado, no modulo `IV` (nucleo CRM/BPM). Sem linhas no snapshot - recurso provavelmente nao utilizado na Tracbel.

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqPropriedade` | `numeric(4,0)` | **nao** | - | **PK**; FK -> `IV_PropriLista.SeqPropriedade` |
| 2 | `Campo` | `varchar(2)` | **nao** | - | **PK**; FK -> `IV_PropriLista.Campo` |
| 3 | `SeqPropriLista` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_PropriLista.SeqPropriLista` |
| 4 | `SeqPropListaLk` | `varchar(18)` | **nao** | - | **PK** |
| 5 | `Similar` | `varchar(100)` | sim | - |  |

---

### IV_PropriLista_ITA

`classe: lixo/backup` · `5 colunas` · `7.520 linhas (snapshot 03/06/2026)` · `PK: nenhuma declarada`

**Funcao:** _(inferido)_ Pelo nome, e uma lista de opcoes de dominio relacionada a propriedade/atributo customizado, no modulo `IV` (nucleo CRM/BPM).

> **Nao usar em producao.** Motivo da classificacao: copia/variante manual (sufixo `_ITA`) (padroes do `SCHEMA_MAP.md`).

_Tabela de colunas omitida de proposito (5 colunas) para manter o documento legivel._

---

### IV_Questao

`classe: nucleo` · `19 colunas` · `2.300 linhas (snapshot 03/06/2026)` · `PK: SeqFormulario, Questao`

**Funcao:** Construtor de formulários: 176 formulários, 2.303 perguntas tipadas com ramificação condicional e peso, 3.995 opções de lista, 85 mil questionários respondidos — materializados em 175 TABELAS FÍSICAS IV_Q_* e 175 views IV_Q$*. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFormulario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Formulario.SeqFormulario`; formulario (`IV_Formulario.SeqFormulario`) |
| 2 | `Questao` | `decimal(4,0)` | **nao** | - | **PK** |
| 3 | `Descricao` | `varchar(60)` | **nao** | - | descricao do registro |
| 4 | `NroReferencia` | `varchar(8)` | sim | - |  |
| 5 | `DescReduzida` | `varchar(30)` | sim | - |  |
| 6 | `Nomecoluna` | `varchar(30)` | sim | - |  |
| 7 | `Grupo` | `varchar(30)` | sim | - |  |
| 8 | `TipoDado` | `char(1)` | **nao** | - |  |
| 9 | `NumMinimo` | `decimal(10,2)` | sim | - |  |
| 10 | `NumMaximo` | `decimal(10,2)` | sim | - |  |
| 11 | `Peso` | `decimal(6,2)` | sim | - |  |
| 12 | `ProximaQuestao` | `numeric(18,0)` | sim | - |  |
| 13 | `Script` | `varchar(500)` | sim | - |  |
| 14 | `Tamanho` | `numeric(4,0)` | sim | - |  |
| 15 | `Prefixoresposta` | `varchar(10)` | sim | - |  |
| 16 | `Permitepeso` | `char(1)` | sim | - |  |
| 17 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |
| 18 | `ExigeResposta` | `char(1)` | sim | - |  |
| 19 | `QTDEDECIMAL` | `numeric(2,0)` | sim | - |  |

**Referenciada por:** `IV_QuestaoLista.Questao`, `IV_QuestaoLista.SeqFormulario`

---

### IV_QuestaoLista

`classe: nucleo` · `8 colunas` · `3.995 linhas (snapshot 03/06/2026)` · `PK: SeqFormulario, Questao, Lista`

**Funcao:** Construtor de formulários: 176 formulários, 2.303 perguntas tipadas com ramificação condicional e peso, 3.995 opções de lista, 85 mil questionários respondidos — materializados em 175 TABELAS FÍSICAS IV_Q_* e 175 views IV_Q$*. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqFormulario` | `numeric(18,0)` | **nao** | - | **PK**; FK -> `IV_Questao.SeqFormulario`; formulario (`IV_Formulario.SeqFormulario`) |
| 2 | `Questao` | `decimal(4,0)` | **nao** | - | **PK**; FK -> `IV_Questao.Questao` |
| 3 | `Lista` | `varchar(40)` | **nao** | - | **PK** |
| 4 | `ProximaQuestao` | `decimal(2,0)` | sim | - |  |
| 5 | `Ordem` | `numeric(4,0)` | sim | - |  |
| 6 | `Nomecoluna` | `varchar(30)` | sim | - |  |
| 7 | `Peso` | `decimal(6,2)` | sim | - |  |
| 8 | `EmUso` | `numeric(1,0)` | sim | - | flag de registro/regra ativa (0 = desligada) |

---

### IV_Questionario

`classe: nucleo` · `18 colunas` · `85.393 linhas (snapshot 03/06/2026)` · `PK: SeqQuestionario`

**Funcao:** Construtor de formulários: 176 formulários, 2.303 perguntas tipadas com ramificação condicional e peso, 3.995 opções de lista, 85 mil questionários respondidos — materializados em 175 TABELAS FÍSICAS IV_Q_* e 175 views IV_Q$*. (fonte: `03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md`)

**Colunas:**

| ord | coluna | tipo | nulo? | default | observacao |
|---:|---|---|:---:|:---:|---|
| 1 | `SeqQuestionario` | `numeric(18,0)` | **nao** | - | **PK** |
| 2 | `SeqPessoa` | `numeric(8,0)` | **nao** | - | pessoa (`GE_Pessoa.SeqPessoa`) |
| 3 | `SeqFormulario` | `numeric(18,0)` | **nao** | - | FK -> `IV_Formulario.SeqFormulario`; formulario (`IV_Formulario.SeqFormulario`) |
| 4 | `SeqHistorico` | `float(53,0)` | sim | - | historico (`IV_Historico.SeqHistorico`) |
| 5 | `SeqAgenda` | `numeric(18,0)` | sim | - | agenda (`IV_Agenda.SeqAgenda`) |
| 6 | `DtaRealizacao` | `datetime` | sim | - | data de realizacao |
| 7 | `UsuInclusao` | `varchar(20)` | sim | - | auditoria de inclusao (usuario) |
| 8 | `UsuAlteracao` | `varchar(20)` | sim | - | auditoria de alteracao (usuario) |
| 9 | `DtaAlteracao` | `datetime` | sim | - | auditoria de alteracao (data) |
| 10 | `Departamento` | `varchar(12)` | sim | - | departamento |
| 11 | `Obs` | `varchar(4000)` | sim | - | texto livre |
| 12 | `Resultado` | `decimal(6,0)` | sim | - | resultado (`IV_Resultado.Resultado`) |
| 13 | `LinkDocto` | `varchar(10)` | sim | - | chave de vinculo com documento do ERP |
| 14 | `LinkNro` | `numeric(18,0)` | sim | - | chave de vinculo com documento do ERP |
| 15 | `LinkSerie` | `varchar(250)` | sim | - | chave de vinculo com documento do ERP |
| 16 | `Processo` | `decimal(15,0)` | sim | - | numero do processo (`IV_Processo.Processo`) |
| 17 | `SubProcesso` | `decimal(6,0)` | sim | - |  |
| 18 | `SEQPROJETO` | `numeric(18,0)` | sim | - | FK -> `IV_Projeto.SeqProjeto`; projeto (`IV_Projeto`) |

**Referenciada por:** `IV_Q_ABERTURA_OS_REVISAO.SEQQUESTIONARIO`, `IV_Q_ACOMPANHAMENTO_VENDA.SEQQUESTIONARIO`, `IV_Q_ACOMPANHAM_VENDA_IMP.SEQQUESTIONARIO`, `IV_Q_ACOMPANHA_COMPRA_IMP.SEQQUESTIONARIO`, `IV_Q_ACOMPANH_VENDA_JDE.SEQQUESTIONARIO`, `IV_Q_ACOMPAN_COMPRA_JDE.SEQQUESTIONARIO`, `IV_Q_ACOMPAN_VEND_CONCESS.SEQQUESTIONARIO`, `IV_Q_ACOMP_VENDA_DIRETA.SEQQUESTIONARIO`, `IV_Q_ACOMP_VENDA_DIRETAJD.SEQQUESTIONARIO`, `IV_Q_ACOMP_VENDA_FINANC.SEQQUESTIONARIO`, `IV_Q_ACOMP_VENDA_LOCACAO.SEQQUESTIONARIO`, `IV_Q_ACOMP_VENDA_MANITOU.SEQQUESTIONARIO`, `IV_Q_ACOMP_VENDA_USADO.SEQQUESTIONARIO`, `IV_Q_ACOMP_VEND_MAQUINAS.SEQQUESTIONARIO`, `IV_Q_ADM_FINANCEIRO.SEQQUESTIONARIO`, `IV_Q_AFERICAO_CSC.SEQQUESTIONARIO`, `IV_Q_AFERICAO_DE_PECAS.SEQQUESTIONARIO`, `IV_Q_AFERICAO_IMPLEMENTO.SEQQUESTIONARIO`, `IV_Q_AFERICAO_MAQ_1_CONT.SEQQUESTIONARIO`, `IV_Q_AFERICAO_MAQ_1_WEB.SEQQUESTIONARIO`, `IV_Q_AFERICAO_MAQ_2_CONT.SEQQUESTIONARIO`, `IV_Q_AFERICAO_MAQ_2_WEB.SEQQUESTIONARIO`, `IV_Q_AFERICAO_MAQ_3_CONT.SEQQUESTIONARIO`, `IV_Q_AFERICAO_MAQ_3_WEB.SEQQUESTIONARIO`, `IV_Q_AFERICAO_PECAS.SEQQUESTIONARIO`, `IV_Q_AFERICAO_POS_SOLUCAO.SEQQUESTIONARIO`, `IV_Q_AFERICAO_SERVICOS.SEQQUESTIONARIO`, `IV_Q_AFERICAO_SERV_WEB.SEQQUESTIONARIO`, `IV_Q_AFERICAO_SUPORTE_INT.SEQQUESTIONARIO`, `IV_Q_AFERICAO_VENDA_MAQ.SEQQUESTIONARIO`, `IV_Q_AFERICAO_VENDA_MQ_IM.SEQQUESTIONARIO`, `IV_Q_AFE_VENDA_MQ_IM_USAD.SEQQUESTIONARIO`, `IV_Q_AGUARDAR_PECAS.SEQQUESTIONARIO`, `IV_Q_ALTERADO_PAGAMENTO.SEQQUESTIONARIO`, `IV_Q_APRESENTACAO.SEQQUESTIONARIO`, `IV_Q_APRESENTACAO_JDE.SEQQUESTIONARIO`, `IV_Q_APRESENT_EQUIPAMENTO.SEQQUESTIONARIO`, `IV_Q_APRESENT_IMPLEMENTO.SEQQUESTIONARIO`, `IV_Q_APROVACAO_TCSM.SEQQUESTIONARIO`, `IV_Q_ATUALIZACAO_PUK.SEQQUESTIONARIO`, `IV_Q_AVALIACAO_AMS_USADO.SEQQUESTIONARIO`, `IV_Q_AVALIA_COLHEIT_USADA.SEQQUESTIONARIO`, `IV_Q_AVALIA_IMPLEM_USADO.SEQQUESTIONARIO`, `IV_Q_AVALIA_USADO_ENTRADA.SEQQUESTIONARIO`, `IV_Q_AVAL_COLH_CANA_USADA.SEQQUESTIONARIO`, `IV_Q_AVAL_TRATORES_USADOS.SEQQUESTIONARIO`, `IV_Q_AVAL_USADO_ENTRADA.SEQQUESTIONARIO`, `IV_Q_CADASTROS_LISTAS.SEQQUESTIONARIO`, `IV_Q_CANCELAMENTO_SEGURO.SEQQUESTIONARIO`, `IV_Q_CANCEL_RENOVACAO_SEG.SEQQUESTIONARIO`, `IV_Q_CANHOTO_DIGITAL.SEQQUESTIONARIO`, `IV_Q_CHASSI_ENTREGA_FISIC.SEQQUESTIONARIO`, `IV_Q_CHASSI_EQUIPAMENTO.SEQQUESTIONARIO`, `IV_Q_CHASSI_PMP.SEQQUESTIONARIO`, `IV_Q_CHEGADA_IMPLEMENTO.SEQQUESTIONARIO`, `IV_Q_COMISSAO.SEQQUESTIONARIO`, `IV_Q_COMISSAO_AMS.SEQQUESTIONARIO`, `IV_Q_COMISSAO_CONTACHAVE.SEQQUESTIONARIO`, `IV_Q_COMISSAO_SERV_AMS.SEQQUESTIONARIO`, `IV_Q_COMISSAO_USADO.SEQQUESTIONARIO`, `IV_Q_COMISSAO_VD_LOCACAO.SEQQUESTIONARIO`, `IV_Q_COMPETIDORES_NA_NEG.SEQQUESTIONARIO`, `IV_Q_COMPETIDORES_NA_NEGO.SEQQUESTIONARIO`, `IV_Q_COMPETID_NEGOC_IMPL.SEQQUESTIONARIO`, `IV_Q_COM_INTERESSE_FUTURO.SEQQUESTIONARIO`, `IV_Q_CONDICOES_DE_VENDAS.SEQQUESTIONARIO`, `IV_Q_CONT_COMISSAO_22.SEQQUESTIONARIO`, `IV_Q_COTA_CONSORCIO.SEQQUESTIONARIO`, `IV_Q_DEMONSTRACAO_JD.SEQQUESTIONARIO`, `IV_Q_DEMONSTRACAO_LOG.SEQQUESTIONARIO`, `IV_Q_DEMONSTRACAO_NF.SEQQUESTIONARIO`, `IV_Q_DEMONSTRACAO_TRATOR.SEQQUESTIONARIO`, `IV_Q_DEMONSTR_COLHEITAD.SEQQUESTIONARIO`, `IV_Q_DEMO_EQUIP_JD.SEQQUESTIONARIO`, `IV_Q_DEMO_IMPLEMENTO.SEQQUESTIONARIO`, `IV_Q_DEMO_MAQUINAS.SEQQUESTIONARIO`, `IV_Q_DEMO_TRATOR.SEQQUESTIONARIO`, `IV_Q_DEVOLUCAO_PECA.SEQQUESTIONARIO`, `IV_Q_DEVOLUCAO_PUK.SEQQUESTIONARIO`, `IV_Q_DOC_ANALISE_CREDITO.SEQQUESTIONARIO`, `IV_Q_EVENTOS_AFERICAO.SEQQUESTIONARIO`, `IV_Q_EXP_FLUXO_MODELER.SEQQUESTIONARIO`, `IV_Q_FORA_SERVICO_PMP.SEQQUESTIONARIO`, `IV_Q_FORM_TREINO.SEQQUESTIONARIO`, `IV_Q_GAR_DATA_SERVICO.SEQQUESTIONARIO`, `IV_Q_GAR_FAB_SOL_PECA.SEQQUESTIONARIO`, `IV_Q_GESTAO_CREDITO.SEQQUESTIONARIO`, `IV_Q_GESTAO_CREDITO_AMS.SEQQUESTIONARIO`, `IV_Q_GESTAO_CREDITO_IMP.SEQQUESTIONARIO`, `IV_Q_GESTAO_PRODUTO_IMPL.SEQQUESTIONARIO`, `IV_Q_GESTAO_PRODUTO___AMS.SEQQUESTIONARIO`, `IV_Q_HORIMETRO_AGREGA.SEQQUESTIONARIO`, `IV_Q_INCENTIVO.SEQQUESTIONARIO`, `IV_Q_INTERESSE_FUTURO_PRO.SEQQUESTIONARIO`, `IV_Q_INTERESSE_PROJETO_IR.SEQQUESTIONARIO`, `IV_Q_LIBERAR_DEMONSTRACAO.SEQQUESTIONARIO`, `IV_Q_LICENCAS_PUK.SEQQUESTIONARIO`, `IV_Q_LOCACAO_COMISSAO.SEQQUESTIONARIO`, `IV_Q_OFERECE_RENOV_SEGURO.SEQQUESTIONARIO`, `IV_Q_ORIGEM_DA_RENDA.SEQQUESTIONARIO`, `IV_Q_OS_ABERTA.SEQQUESTIONARIO`, `IV_Q_OS_CORTESIA.SEQQUESTIONARIO`, `IV_Q_OS_GARANTIA.SEQQUESTIONARIO`, `IV_Q_OS_REVISAO_ENTREGA.SEQQUESTIONARIO`, `IV_Q_PECAS_AFERICAO.SEQQUESTIONARIO`, `IV_Q_PEDIDO_GC.SEQQUESTIONARIO`, `IV_Q_PEDIDO_KAM.SEQQUESTIONARIO`, `IV_Q_PEDIDO_SAM.SEQQUESTIONARIO`, `IV_Q_PERCEPCAO_JD.SEQQUESTIONARIO`, `IV_Q_PESQUISA_NPS.SEQQUESTIONARIO`, `IV_Q_PESQUISA_TI.SEQQUESTIONARIO`, `IV_Q_PREMIO_DEMO.SEQQUESTIONARIO`, `IV_Q_PREVISAO_RECEBIMENTO.SEQQUESTIONARIO`, `IV_Q_PRODUTO_RD.SEQQUESTIONARIO`, `IV_Q_PROPOSTA_COMERCIAL.SEQQUESTIONARIO`, `IV_Q_PROSPECCAO_SERV__JD.SEQQUESTIONARIO`, `IV_Q_QUALIDADE_PECAS.SEQQUESTIONARIO`, `IV_Q_QUALIDADE_SERVICOS.SEQQUESTIONARIO`, `IV_Q_QUALIDADE_VENDAMAQ.SEQQUESTIONARIO`, `IV_Q_RECEBIMENTO_A_PRAZO.SEQQUESTIONARIO`, `IV_Q_RECEBIMENTO_COMISSAO.SEQQUESTIONARIO`, `IV_Q_RECEBIMENTO_FINANC.SEQQUESTIONARIO`, `IV_Q_RECEB_FINAN_IMPLEM.SEQQUESTIONARIO`, `IV_Q_RESPONSAVEL_TECNICO.SEQQUESTIONARIO`, `IV_Q_RESULTADO_DEMO.SEQQUESTIONARIO`, `IV_Q_RETORNADO_JD.SEQQUESTIONARIO`, `IV_Q_REVISAO_100H.SEQQUESTIONARIO`, `IV_Q_REVISAO_1100_1150H.SEQQUESTIONARIO`, `IV_Q_REVISAO_1500H.SEQQUESTIONARIO`, `IV_Q_REVISAO_450_600H.SEQQUESTIONARIO`, `IV_Q_REVISAO_800H.SEQQUESTIONARIO`, `IV_Q_REVISAO_FIM_GARANTIA.SEQQUESTIONARIO`, `IV_Q_REV_DATA_SERVICO.SEQQUESTIONARIO`, `IV_Q_ROMANEIO_DEV_PECA.SEQQUESTIONARIO`, `IV_Q_SEPARACAO_PEDIDO.SEQQUESTIONARIO`, `IV_Q_SERVICOS_AFERICAO.SEQQUESTIONARIO`, `IV_Q_SERVICO_EXTERNOS_JD.SEQQUESTIONARIO`, `IV_Q_SOLICITACAO_TCAT.SEQQUESTIONARIO`, `IV_Q_TESTE1.SEQQUESTIONARIO`, `IV_Q_TESTE2.SEQQUESTIONARIO`, `IV_Q_TESTE_PRIMEIRO_JD.SEQQUESTIONARIO`, `IV_Q_TICKET_DSI.SEQQUESTIONARIO`, `IV_Q_VENDA.SEQQUESTIONARIO`, `IV_Q_VENDAPERDIDA_SEGURO.SEQQUESTIONARIO`, `IV_Q_VENDA_AMS.SEQQUESTIONARIO`, `IV_Q_VENDA_CONSORCIO.SEQQUESTIONARIO`, `IV_Q_VENDA_DIRETA.SEQQUESTIONARIO`, `IV_Q_VENDA_DSI.SEQQUESTIONARIO`, `IV_Q_VENDA_EQUIPAMENTO.SEQQUESTIONARIO`, `IV_Q_VENDA_MAQUINA_FY25.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_FY25.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_IMPL.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_IMPLEM.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_JDE.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_MANITO.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_MAQIMP.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_PROD.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_SEGURO.SEQQUESTIONARIO`, `IV_Q_VENDA_PERDIDA_TESTE.SEQQUESTIONARIO`, `IV_Q_VENDA_PNEUS.SEQQUESTIONARIO`, `IV_Q_VENDA_PRECISION_UP.SEQQUESTIONARIO`, `IV_Q_VENDA_SEMINOVO.SEQQUESTIONARIO`, `IV_Q_VENDA_SERVICOS_AMS.SEQQUESTIONARIO`, `IV_Q_VENDA_VP_PNEUS.SEQQUESTIONARIO`, `IV_Q_VENDER_RENOVACAO_SEG.SEQQUESTIONARIO`, `IV_Q_VISITA_DSI.SEQQUESTIONARIO`, `IV_Q_VISITA_EXP_CLIENTE.SEQQUESTIONARIO`, `IV_Q_VP_COLHEDORA.SEQQUESTIONARIO`, `IV_Q_VP_COLHEITADEIRA.SEQQUESTIONARIO`, `IV_Q_VP_PLANTADEIRA.SEQQUESTIONARIO`, `IV_Q_VP_PULVERIZADOR.SEQQUESTIONARIO`, `IV_Q_VP_RENOVACAO_SEGURO.SEQQUESTIONARIO`, `IV_Q_VP_SEM_PARTICIPACAO.SEQQUESTIONARIO`, `IV_Q_VP_TRATOR.SEQQUESTIONARIO`, `IV_TC_PESSOA.SEQQUESTIONARIO`, `JDE_PURCHASE.SeqQuestionario`

---

### IV_Q_ABERTURA_OS_REVISAO

`classe: formulario-materializado` · `3 colunas` · `10 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ABERTURA_OS_REVISAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ABERTURA_OS_REVISAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ABERTURA_OS_REVISAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ABERTURA_OS_REVISAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMPANHAMENTO_VENDA

`classe: formulario-materializado` · `53 colunas` · `1.715 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMPANHAMENTO_VENDA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMPANHAMENTO_VENDA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMPANHAMENTO_VENDA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (53 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMPANHAMENTO_VENDA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMPANHAM_VENDA_IMP

`classe: formulario-materializado` · `39 colunas` · `3.308 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMPANHAM_VENDA_IMP`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMPANHAM_VENDA_IMP` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMPANHAM_VENDA_IMP`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (39 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMPANHAM_VENDA_IMP`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMPANHA_COMPRA_IMP

`classe: formulario-materializado` · `16 colunas` · `26 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMPANHA_COMPRA_IMP`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMPANHA_COMPRA_IMP` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMPANHA_COMPRA_IMP`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (16 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMPANHA_COMPRA_IMP`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMPANH_VENDA_JDE

`classe: formulario-materializado` · `104 colunas` · `6.343 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMPANH_VENDA_JDE`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMPANH_VENDA_JDE` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMPANH_VENDA_JDE`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (104 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMPANH_VENDA_JDE`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMPAN_COMPRA_JDE

`classe: formulario-materializado` · `17 colunas` · `153 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMPAN_COMPRA_JDE`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMPAN_COMPRA_JDE` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMPAN_COMPRA_JDE`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (17 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMPAN_COMPRA_JDE`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMPAN_VEND_CONCESS

`classe: formulario-materializado` · `9 colunas` · `98 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMPAN_VEND_CONCESS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMPAN_VEND_CONCESS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMPAN_VEND_CONCESS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (9 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMPAN_VEND_CONCESS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMP_VENDA_DIRETA

`classe: formulario-materializado` · `20 colunas` · `125 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMP_VENDA_DIRETA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMP_VENDA_DIRETA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMP_VENDA_DIRETA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (20 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMP_VENDA_DIRETA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMP_VENDA_DIRETAJD

`classe: formulario-materializado` · `94 colunas` · `270 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMP_VENDA_DIRETAJD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMP_VENDA_DIRETAJD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMP_VENDA_DIRETAJD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (94 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMP_VENDA_DIRETAJD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMP_VENDA_FINANC

`classe: formulario-materializado` · `9 colunas` · `2.021 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** A PONTE MANUAL entre o CRM e o ERP: formulário onde o vendedor DIGITA chassi, número da NF, data de faturamento, data de entrega, filial, proposta JD e COMAR. É a única fonte de 'esta oportunidade virou esta nota'. (fonte: `02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md`)

**Formulario de origem:** `ACOMP_VENDA_FINANC` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMP_VENDA_FINANC`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (9 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMP_VENDA_FINANC`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMP_VENDA_LOCACAO

`classe: formulario-materializado` · `14 colunas` · `15 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMP_VENDA_LOCACAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMP_VENDA_LOCACAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMP_VENDA_LOCACAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (14 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMP_VENDA_LOCACAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMP_VENDA_MANITOU

`classe: formulario-materializado` · `45 colunas` · `21 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMP_VENDA_MANITOU`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMP_VENDA_MANITOU` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMP_VENDA_MANITOU`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (45 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMP_VENDA_MANITOU`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMP_VENDA_USADO

`classe: formulario-materializado` · `44 colunas` · `256 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMP_VENDA_USADO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMP_VENDA_USADO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMP_VENDA_USADO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (44 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMP_VENDA_USADO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ACOMP_VEND_MAQUINAS

`classe: formulario-materializado` · `54 colunas` · `1.535 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ACOMP_VEND_MAQUINAS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ACOMP_VEND_MAQUINAS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ACOMP_VEND_MAQUINAS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (54 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ACOMP_VEND_MAQUINAS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ADM_FINANCEIRO

`classe: formulario-materializado` · `14 colunas` · `1.884 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ADM_FINANCEIRO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ADM_FINANCEIRO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ADM_FINANCEIRO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (14 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ADM_FINANCEIRO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_CSC

`classe: formulario-materializado` · `8 colunas` · `32 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_CSC`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_CSC` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_CSC`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_CSC`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_DE_PECAS

`classe: formulario-materializado` · `8 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_DE_PECAS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_DE_PECAS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_DE_PECAS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_DE_PECAS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_IMPLEMENTO

`classe: formulario-materializado` · `13 colunas` · `440 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_IMPLEMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_IMPLEMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_IMPLEMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (13 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_IMPLEMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_MAQ_1_CONT

`classe: formulario-materializado` · `12 colunas` · `873 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_MAQ_1_CONT`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_MAQ_1_CONT` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_MAQ_1_CONT`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_MAQ_1_CONT`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_MAQ_1_WEB

`classe: formulario-materializado` · `9 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_MAQ_1_WEB`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_MAQ_1_WEB` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_MAQ_1_WEB`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (9 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_MAQ_1_WEB`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_MAQ_2_CONT

`classe: formulario-materializado` · `8 colunas` · `541 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_MAQ_2_CONT`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_MAQ_2_CONT` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_MAQ_2_CONT`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_MAQ_2_CONT`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_MAQ_2_WEB

`classe: formulario-materializado` · `6 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_MAQ_2_WEB`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_MAQ_2_WEB` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_MAQ_2_WEB`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (6 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_MAQ_2_WEB`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_MAQ_3_CONT

`classe: formulario-materializado` · `13 colunas` · `484 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_MAQ_3_CONT`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_MAQ_3_CONT` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_MAQ_3_CONT`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (13 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_MAQ_3_CONT`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_MAQ_3_WEB

`classe: formulario-materializado` · `12 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_MAQ_3_WEB`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_MAQ_3_WEB` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_MAQ_3_WEB`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_MAQ_3_WEB`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_PECAS

`classe: formulario-materializado` · `33 colunas` · `1.719 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_PECAS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_PECAS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_PECAS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (33 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_PECAS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_POS_SOLUCAO

`classe: formulario-materializado` · `4 colunas` · `452 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_POS_SOLUCAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_POS_SOLUCAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_POS_SOLUCAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_POS_SOLUCAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_SERVICOS

`classe: formulario-materializado` · `16 colunas` · `635 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_SERVICOS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_SERVICOS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_SERVICOS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (16 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_SERVICOS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_SERV_WEB

`classe: formulario-materializado` · `17 colunas` · `31 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_SERV_WEB`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_SERV_WEB` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_SERV_WEB`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (17 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_SERV_WEB`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_SUPORTE_INT

`classe: formulario-materializado` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_SUPORTE_INT`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_SUPORTE_INT` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_SUPORTE_INT`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_SUPORTE_INT`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_VENDA_MAQ

`classe: formulario-materializado` · `10 colunas` · `979 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_VENDA_MAQ`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_VENDA_MAQ` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_VENDA_MAQ`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (10 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_VENDA_MAQ`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFERICAO_VENDA_MQ_IM

`classe: formulario-materializado` · `32 colunas` · `604 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFERICAO_VENDA_MQ_IM`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFERICAO_VENDA_MQ_IM` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFERICAO_VENDA_MQ_IM`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (32 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFERICAO_VENDA_MQ_IM`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AFE_VENDA_MQ_IM_USAD

`classe: formulario-materializado` · `26 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AFE_VENDA_MQ_IM_USAD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AFE_VENDA_MQ_IM_USAD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AFE_VENDA_MQ_IM_USAD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (26 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AFE_VENDA_MQ_IM_USAD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AGUARDAR_PECAS

`classe: formulario-materializado` · `2 colunas` · `31 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AGUARDAR_PECAS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AGUARDAR_PECAS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AGUARDAR_PECAS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AGUARDAR_PECAS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ALTERADO_PAGAMENTO

`classe: formulario-materializado` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ALTERADO_PAGAMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ALTERADO_PAGAMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ALTERADO_PAGAMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ALTERADO_PAGAMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_APRESENTACAO

`classe: formulario-materializado` · `8 colunas` · `211 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `APRESENTACAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `APRESENTACAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `APRESENTACAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `APRESENTACAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_APRESENTACAO_JDE

`classe: formulario-materializado` · `8 colunas` · `123 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `APRESENTACAO_JDE`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `APRESENTACAO_JDE` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `APRESENTACAO_JDE`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `APRESENTACAO_JDE`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_APRESENT_EQUIPAMENTO

`classe: formulario-materializado` · `1 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `APRESENT_EQUIPAMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `APRESENT_EQUIPAMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `APRESENT_EQUIPAMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (1 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `APRESENT_EQUIPAMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_APRESENT_IMPLEMENTO

`classe: formulario-materializado` · `7 colunas` · `43 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `APRESENT_IMPLEMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `APRESENT_IMPLEMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `APRESENT_IMPLEMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (7 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `APRESENT_IMPLEMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_APROVACAO_TCSM

`classe: formulario-materializado` · `2 colunas` · `27 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `APROVACAO_TCSM`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `APROVACAO_TCSM` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `APROVACAO_TCSM`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `APROVACAO_TCSM`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ATUALIZACAO_PUK

`classe: formulario-materializado` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ATUALIZACAO_PUK`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ATUALIZACAO_PUK` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ATUALIZACAO_PUK`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (10 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ATUALIZACAO_PUK`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AVALIACAO_AMS_USADO

`classe: formulario-materializado` · `6 colunas` · `4 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AVALIACAO_AMS_USADO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AVALIACAO_AMS_USADO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AVALIACAO_AMS_USADO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (6 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AVALIACAO_AMS_USADO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AVALIA_COLHEIT_USADA

`classe: formulario-materializado` · `51 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AVALIA_COLHEIT_USADA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AVALIA_COLHEIT_USADA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AVALIA_COLHEIT_USADA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (51 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AVALIA_COLHEIT_USADA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AVALIA_IMPLEM_USADO

`classe: formulario-materializado` · `38 colunas` · `60 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AVALIA_IMPLEM_USADO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AVALIA_IMPLEM_USADO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AVALIA_IMPLEM_USADO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (38 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AVALIA_IMPLEM_USADO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AVALIA_USADO_ENTRADA

`classe: formulario-materializado` · `35 colunas` · `823 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AVALIA_USADO_ENTRADA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AVALIA_USADO_ENTRADA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AVALIA_USADO_ENTRADA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (35 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AVALIA_USADO_ENTRADA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AVAL_COLH_CANA_USADA

`classe: formulario-materializado` · `32 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AVAL_COLH_CANA_USADA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AVAL_COLH_CANA_USADA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AVAL_COLH_CANA_USADA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (32 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AVAL_COLH_CANA_USADA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AVAL_TRATORES_USADOS

`classe: formulario-materializado` · `53 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AVAL_TRATORES_USADOS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AVAL_TRATORES_USADOS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AVAL_TRATORES_USADOS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (53 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AVAL_TRATORES_USADOS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_AVAL_USADO_ENTRADA

`classe: formulario-materializado` · `12 colunas` · `200 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `AVAL_USADO_ENTRADA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `AVAL_USADO_ENTRADA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `AVAL_USADO_ENTRADA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `AVAL_USADO_ENTRADA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CADASTROS_LISTAS

`classe: formulario-materializado` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CADASTROS_LISTAS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CADASTROS_LISTAS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CADASTROS_LISTAS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CADASTROS_LISTAS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CANCELAMENTO_SEGURO

`classe: formulario-materializado` · `5 colunas` · `4 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CANCELAMENTO_SEGURO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CANCELAMENTO_SEGURO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CANCELAMENTO_SEGURO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CANCELAMENTO_SEGURO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CANCEL_RENOVACAO_SEG

`classe: formulario-materializado` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CANCEL_RENOVACAO_SEG`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CANCEL_RENOVACAO_SEG` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CANCEL_RENOVACAO_SEG`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CANCEL_RENOVACAO_SEG`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CANHOTO_DIGITAL

`classe: formulario-materializado` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CANHOTO_DIGITAL`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CANHOTO_DIGITAL` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CANHOTO_DIGITAL`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CANHOTO_DIGITAL`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CHASSI_ENTREGA_FISIC

`classe: formulario-materializado` · `2 colunas` · `1.064 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CHASSI_ENTREGA_FISIC`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CHASSI_ENTREGA_FISIC` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CHASSI_ENTREGA_FISIC`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CHASSI_ENTREGA_FISIC`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CHASSI_EQUIPAMENTO

`classe: formulario-materializado` · `3 colunas` · `1.039 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CHASSI_EQUIPAMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CHASSI_EQUIPAMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CHASSI_EQUIPAMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CHASSI_EQUIPAMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CHASSI_PMP

`classe: formulario-materializado` · `4 colunas` · `767 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CHASSI_PMP`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CHASSI_PMP` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CHASSI_PMP`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CHASSI_PMP`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CHEGADA_IMPLEMENTO

`classe: formulario-materializado` · `2 colunas` · `61 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CHEGADA_IMPLEMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CHEGADA_IMPLEMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CHEGADA_IMPLEMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CHEGADA_IMPLEMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMISSAO

`classe: formulario-materializado` · `36 colunas` · `7.165 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMISSAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMISSAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMISSAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (36 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMISSAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMISSAO_AMS

`classe: formulario-materializado` · `20 colunas` · `633 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMISSAO_AMS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMISSAO_AMS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMISSAO_AMS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (20 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMISSAO_AMS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMISSAO_CONTACHAVE

`classe: formulario-materializado` · `18 colunas` · `144 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMISSAO_CONTACHAVE`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMISSAO_CONTACHAVE` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMISSAO_CONTACHAVE`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (18 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMISSAO_CONTACHAVE`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMISSAO_SERV_AMS

`classe: formulario-materializado` · `15 colunas` · `448 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMISSAO_SERV_AMS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMISSAO_SERV_AMS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMISSAO_SERV_AMS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (15 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMISSAO_SERV_AMS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMISSAO_USADO

`classe: formulario-materializado` · `5 colunas` · `240 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMISSAO_USADO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMISSAO_USADO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMISSAO_USADO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMISSAO_USADO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMISSAO_VD_LOCACAO

`classe: formulario-materializado` · `13 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMISSAO_VD_LOCACAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMISSAO_VD_LOCACAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMISSAO_VD_LOCACAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (13 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMISSAO_VD_LOCACAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMPETIDORES_NA_NEG

`classe: formulario-materializado` · `9 colunas` · `104 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMPETIDORES_NA_NEG`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMPETIDORES_NA_NEG` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMPETIDORES_NA_NEG`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (9 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMPETIDORES_NA_NEG`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMPETIDORES_NA_NEGO

`classe: formulario-materializado` · `10 colunas` · `101 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMPETIDORES_NA_NEGO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMPETIDORES_NA_NEGO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMPETIDORES_NA_NEGO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (10 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMPETIDORES_NA_NEGO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COMPETID_NEGOC_IMPL

`classe: formulario-materializado` · `9 colunas` · `13 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COMPETID_NEGOC_IMPL`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COMPETID_NEGOC_IMPL` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COMPETID_NEGOC_IMPL`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (9 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COMPETID_NEGOC_IMPL`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COM_INTERESSE_FUTURO

`classe: formulario-materializado` · `9 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COM_INTERESSE_FUTURO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COM_INTERESSE_FUTURO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COM_INTERESSE_FUTURO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (9 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COM_INTERESSE_FUTURO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CONDICOES_DE_VENDAS

`classe: formulario-materializado` · `11 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CONDICOES_DE_VENDAS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CONDICOES_DE_VENDAS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CONDICOES_DE_VENDAS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (11 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CONDICOES_DE_VENDAS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_CONT_COMISSAO_22

`classe: formulario-materializado` · `18 colunas` · `2.251 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `CONT_COMISSAO_22`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `CONT_COMISSAO_22` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `CONT_COMISSAO_22`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (18 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `CONT_COMISSAO_22`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_COTA_CONSORCIO

`classe: formulario-materializado` · `8 colunas` · `73 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `COTA_CONSORCIO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `COTA_CONSORCIO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `COTA_CONSORCIO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `COTA_CONSORCIO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMONSTRACAO_JD

`classe: formulario-materializado` · `33 colunas` · `215 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMONSTRACAO_JD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMONSTRACAO_JD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMONSTRACAO_JD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (33 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMONSTRACAO_JD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMONSTRACAO_LOG

`classe: formulario-materializado` · `3 colunas` · `25 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMONSTRACAO_LOG`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMONSTRACAO_LOG` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMONSTRACAO_LOG`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMONSTRACAO_LOG`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMONSTRACAO_NF

`classe: formulario-materializado` · `2 colunas` · `44 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMONSTRACAO_NF`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMONSTRACAO_NF` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMONSTRACAO_NF`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMONSTRACAO_NF`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMONSTRACAO_TRATOR

`classe: formulario-materializado` · `25 colunas` · `32 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMONSTRACAO_TRATOR`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMONSTRACAO_TRATOR` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMONSTRACAO_TRATOR`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (25 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMONSTRACAO_TRATOR`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMONSTR_COLHEITAD

`classe: formulario-materializado` · `8 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMONSTR_COLHEITAD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMONSTR_COLHEITAD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMONSTR_COLHEITAD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMONSTR_COLHEITAD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMO_EQUIP_JD

`classe: formulario-materializado` · `23 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMO_EQUIP_JD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMO_EQUIP_JD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMO_EQUIP_JD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (23 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMO_EQUIP_JD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMO_IMPLEMENTO

`classe: formulario-materializado` · `25 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMO_IMPLEMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMO_IMPLEMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMO_IMPLEMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (25 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMO_IMPLEMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMO_MAQUINAS

`classe: formulario-materializado` · `19 colunas` · `109 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMO_MAQUINAS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMO_MAQUINAS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMO_MAQUINAS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (19 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMO_MAQUINAS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEMO_TRATOR

`classe: formulario-materializado` · `26 colunas` · `43 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEMO_TRATOR`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEMO_TRATOR` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEMO_TRATOR`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (26 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEMO_TRATOR`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEVOLUCAO_PECA

`classe: formulario-materializado` · `2 colunas` · `581 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEVOLUCAO_PECA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEVOLUCAO_PECA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEVOLUCAO_PECA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEVOLUCAO_PECA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DEVOLUCAO_PUK

`classe: formulario-materializado` · `2 colunas` · `5 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DEVOLUCAO_PUK`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DEVOLUCAO_PUK` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DEVOLUCAO_PUK`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DEVOLUCAO_PUK`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_DOC_ANALISE_CREDITO

`classe: formulario-materializado` · `28 colunas` · `21 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `DOC_ANALISE_CREDITO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `DOC_ANALISE_CREDITO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `DOC_ANALISE_CREDITO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (28 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `DOC_ANALISE_CREDITO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_EVENTOS_AFERICAO

`classe: formulario-materializado` · `12 colunas` · `67 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `EVENTOS_AFERICAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `EVENTOS_AFERICAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `EVENTOS_AFERICAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `EVENTOS_AFERICAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_EXP_FLUXO_MODELER

`classe: formulario-materializado` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `EXP_FLUXO_MODELER`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `EXP_FLUXO_MODELER` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `EXP_FLUXO_MODELER`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `EXP_FLUXO_MODELER`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_FORA_SERVICO_PMP

`classe: formulario-materializado` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `FORA_SERVICO_PMP`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `FORA_SERVICO_PMP` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `FORA_SERVICO_PMP`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `FORA_SERVICO_PMP`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_FORM_TREINO

`classe: formulario-materializado` · `1 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `FORM_TREINO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `FORM_TREINO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `FORM_TREINO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (1 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `FORM_TREINO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_GAR_DATA_SERVICO

`classe: formulario-materializado` · `3 colunas` · `4.888 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `GAR_DATA_SERVICO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `GAR_DATA_SERVICO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `GAR_DATA_SERVICO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `GAR_DATA_SERVICO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_GAR_FAB_SOL_PECA

`classe: formulario-materializado` · `2 colunas` · `640 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `GAR_FAB_SOL_PECA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `GAR_FAB_SOL_PECA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `GAR_FAB_SOL_PECA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `GAR_FAB_SOL_PECA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_GESTAO_CREDITO

`classe: formulario-materializado` · `54 colunas` · `1.506 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `GESTAO_CREDITO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `GESTAO_CREDITO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `GESTAO_CREDITO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (54 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `GESTAO_CREDITO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_GESTAO_CREDITO_AMS

`classe: formulario-materializado` · `51 colunas` · `175 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `GESTAO_CREDITO_AMS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `GESTAO_CREDITO_AMS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `GESTAO_CREDITO_AMS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (51 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `GESTAO_CREDITO_AMS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_GESTAO_CREDITO_IMP

`classe: formulario-materializado` · `51 colunas` · `807 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `GESTAO_CREDITO_IMP`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `GESTAO_CREDITO_IMP` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `GESTAO_CREDITO_IMP`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (51 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `GESTAO_CREDITO_IMP`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_GESTAO_PRODUTO_IMPL

`classe: formulario-materializado` · `23 colunas` · `813 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `GESTAO_PRODUTO_IMPL`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `GESTAO_PRODUTO_IMPL` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `GESTAO_PRODUTO_IMPL`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (23 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `GESTAO_PRODUTO_IMPL`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_GESTAO_PRODUTO___AMS

`classe: formulario-materializado` · `39 colunas` · `177 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `GESTAO_PRODUTO___AMS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `GESTAO_PRODUTO___AMS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `GESTAO_PRODUTO___AMS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (39 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `GESTAO_PRODUTO___AMS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_HORIMETRO_AGREGA

`classe: formulario-materializado` · `2 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `HORIMETRO_AGREGA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `HORIMETRO_AGREGA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `HORIMETRO_AGREGA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `HORIMETRO_AGREGA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_INCENTIVO

`classe: formulario-materializado` · `43 colunas` · `2.079 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `INCENTIVO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `INCENTIVO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `INCENTIVO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (43 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `INCENTIVO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_INTERESSE_FUTURO_PRO

`classe: formulario-materializado` · `5 colunas` · `10 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `INTERESSE_FUTURO_PRO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `INTERESSE_FUTURO_PRO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `INTERESSE_FUTURO_PRO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `INTERESSE_FUTURO_PRO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_INTERESSE_PROJETO_IR

`classe: formulario-materializado` · `9 colunas` · `25 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `INTERESSE_PROJETO_IR`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `INTERESSE_PROJETO_IR` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `INTERESSE_PROJETO_IR`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (9 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `INTERESSE_PROJETO_IR`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_LIBERAR_DEMONSTRACAO

`classe: formulario-materializado` · `5 colunas` · `44 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `LIBERAR_DEMONSTRACAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `LIBERAR_DEMONSTRACAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `LIBERAR_DEMONSTRACAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `LIBERAR_DEMONSTRACAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_LICENCAS_PUK

`classe: formulario-materializado` · `12 colunas` · `41 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `LICENCAS_PUK`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `LICENCAS_PUK` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `LICENCAS_PUK`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `LICENCAS_PUK`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_LOCACAO_COMISSAO

`classe: formulario-materializado` · `13 colunas` · `15 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `LOCACAO_COMISSAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `LOCACAO_COMISSAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `LOCACAO_COMISSAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (13 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `LOCACAO_COMISSAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_OFERECE_RENOV_SEGURO

`classe: formulario-materializado` · `16 colunas` · `846 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `OFERECE_RENOV_SEGURO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `OFERECE_RENOV_SEGURO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `OFERECE_RENOV_SEGURO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (16 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `OFERECE_RENOV_SEGURO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ORIGEM_DA_RENDA

`classe: formulario-materializado` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ORIGEM_DA_RENDA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ORIGEM_DA_RENDA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ORIGEM_DA_RENDA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ORIGEM_DA_RENDA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_OS_ABERTA

`classe: formulario-materializado` · `3 colunas` · `771 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `OS_ABERTA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `OS_ABERTA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `OS_ABERTA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `OS_ABERTA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_OS_CORTESIA

`classe: formulario-materializado` · `5 colunas` · `78 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `OS_CORTESIA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `OS_CORTESIA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `OS_CORTESIA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `OS_CORTESIA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_OS_GARANTIA

`classe: formulario-materializado` · `5 colunas` · `7.786 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `OS_GARANTIA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `OS_GARANTIA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `OS_GARANTIA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `OS_GARANTIA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_OS_REVISAO_ENTREGA

`classe: formulario-materializado` · `5 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `OS_REVISAO_ENTREGA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `OS_REVISAO_ENTREGA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `OS_REVISAO_ENTREGA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `OS_REVISAO_ENTREGA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PECAS_AFERICAO

`classe: formulario-materializado` · `23 colunas` · `5.174 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PECAS_AFERICAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PECAS_AFERICAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PECAS_AFERICAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (23 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PECAS_AFERICAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PEDIDO_GC

`classe: formulario-materializado` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PEDIDO_GC`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PEDIDO_GC` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PEDIDO_GC`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PEDIDO_GC`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PEDIDO_KAM

`classe: formulario-materializado` · `28 colunas` · `12 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PEDIDO_KAM`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PEDIDO_KAM` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PEDIDO_KAM`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (28 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PEDIDO_KAM`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PEDIDO_SAM

`classe: formulario-materializado` · `19 colunas` · `46 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PEDIDO_SAM`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PEDIDO_SAM` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PEDIDO_SAM`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (19 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PEDIDO_SAM`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PERCEPCAO_JD

`classe: formulario-materializado` · `17 colunas` · `32 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PERCEPCAO_JD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PERCEPCAO_JD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PERCEPCAO_JD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (17 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PERCEPCAO_JD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PESQUISA_NPS

`classe: formulario-materializado` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PESQUISA_NPS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PESQUISA_NPS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PESQUISA_NPS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PESQUISA_NPS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PESQUISA_TI

`classe: formulario-materializado` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PESQUISA_TI`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PESQUISA_TI` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PESQUISA_TI`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PESQUISA_TI`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PREMIO_DEMO

`classe: formulario-materializado` · `4 colunas` · `21 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PREMIO_DEMO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PREMIO_DEMO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PREMIO_DEMO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PREMIO_DEMO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PREVISAO_RECEBIMENTO

`classe: formulario-materializado` · `2 colunas` · `4 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PREVISAO_RECEBIMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PREVISAO_RECEBIMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PREVISAO_RECEBIMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PREVISAO_RECEBIMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PRODUTO_RD

`classe: formulario-materializado` · `8 colunas` · `135 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PRODUTO_RD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PRODUTO_RD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PRODUTO_RD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PRODUTO_RD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PROPOSTA_COMERCIAL

`classe: formulario-materializado` · `10 colunas` · `24 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PROPOSTA_COMERCIAL`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PROPOSTA_COMERCIAL` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PROPOSTA_COMERCIAL`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (10 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PROPOSTA_COMERCIAL`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_PROSPECCAO_SERV__JD

`classe: formulario-materializado` · `6 colunas` · `104 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `PROSPECCAO_SERV__JD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `PROSPECCAO_SERV__JD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `PROSPECCAO_SERV__JD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (6 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `PROSPECCAO_SERV__JD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_QUALIDADE_PECAS

`classe: formulario-materializado` · `6 colunas` · `642 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `QUALIDADE_PECAS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `QUALIDADE_PECAS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `QUALIDADE_PECAS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (6 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `QUALIDADE_PECAS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_QUALIDADE_SERVICOS

`classe: formulario-materializado` · `6 colunas` · `345 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `QUALIDADE_SERVICOS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `QUALIDADE_SERVICOS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `QUALIDADE_SERVICOS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (6 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `QUALIDADE_SERVICOS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_QUALIDADE_VENDAMAQ

`classe: formulario-materializado` · `5 colunas` · `141 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `QUALIDADE_VENDAMAQ`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `QUALIDADE_VENDAMAQ` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `QUALIDADE_VENDAMAQ`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (5 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `QUALIDADE_VENDAMAQ`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_RECEBIMENTO_A_PRAZO

`classe: formulario-materializado` · `17 colunas` · `16 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `RECEBIMENTO_A_PRAZO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `RECEBIMENTO_A_PRAZO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `RECEBIMENTO_A_PRAZO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (17 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `RECEBIMENTO_A_PRAZO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_RECEBIMENTO_COMISSAO

`classe: formulario-materializado` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `RECEBIMENTO_COMISSAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `RECEBIMENTO_COMISSAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `RECEBIMENTO_COMISSAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `RECEBIMENTO_COMISSAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_RECEBIMENTO_FINANC

`classe: formulario-materializado` · `2 colunas` · `4 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `RECEBIMENTO_FINANC`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `RECEBIMENTO_FINANC` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `RECEBIMENTO_FINANC`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `RECEBIMENTO_FINANC`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_RECEB_FINAN_IMPLEM

`classe: formulario-materializado` · `2 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `RECEB_FINAN_IMPLEM`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `RECEB_FINAN_IMPLEM` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `RECEB_FINAN_IMPLEM`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `RECEB_FINAN_IMPLEM`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_RESPONSAVEL_TECNICO

`classe: formulario-materializado` · `2 colunas` · `2.115 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `RESPONSAVEL_TECNICO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `RESPONSAVEL_TECNICO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `RESPONSAVEL_TECNICO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `RESPONSAVEL_TECNICO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_RESULTADO_DEMO

`classe: formulario-materializado` · `15 colunas` · `23 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `RESULTADO_DEMO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `RESULTADO_DEMO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `RESULTADO_DEMO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (15 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `RESULTADO_DEMO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_RETORNADO_JD

`classe: formulario-materializado` · `2 colunas` · `773 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `RETORNADO_JD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `RETORNADO_JD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `RETORNADO_JD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (2 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `RETORNADO_JD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_REVISAO_100H

`classe: formulario-materializado` · `3 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `REVISAO_100H`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `REVISAO_100H` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `REVISAO_100H`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `REVISAO_100H`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_REVISAO_1100_1150H

`classe: formulario-materializado` · `3 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `REVISAO_1100_1150H`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `REVISAO_1100_1150H` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `REVISAO_1100_1150H`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `REVISAO_1100_1150H`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_REVISAO_1500H

`classe: formulario-materializado` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `REVISAO_1500H`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `REVISAO_1500H` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `REVISAO_1500H`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `REVISAO_1500H`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_REVISAO_450_600H

`classe: formulario-materializado` · `3 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `REVISAO_450_600H`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `REVISAO_450_600H` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `REVISAO_450_600H`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `REVISAO_450_600H`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_REVISAO_800H

`classe: formulario-materializado` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `REVISAO_800H`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `REVISAO_800H` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `REVISAO_800H`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `REVISAO_800H`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_REVISAO_FIM_GARANTIA

`classe: formulario-materializado` · `3 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `REVISAO_FIM_GARANTIA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `REVISAO_FIM_GARANTIA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `REVISAO_FIM_GARANTIA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `REVISAO_FIM_GARANTIA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_REV_DATA_SERVICO

`classe: formulario-materializado` · `3 colunas` · `74 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `REV_DATA_SERVICO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `REV_DATA_SERVICO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `REV_DATA_SERVICO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `REV_DATA_SERVICO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_ROMANEIO_DEV_PECA

`classe: formulario-materializado` · `4 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `ROMANEIO_DEV_PECA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `ROMANEIO_DEV_PECA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `ROMANEIO_DEV_PECA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `ROMANEIO_DEV_PECA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_SEPARACAO_PEDIDO

`classe: formulario-materializado` · `18 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `SEPARACAO_PEDIDO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `SEPARACAO_PEDIDO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `SEPARACAO_PEDIDO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (18 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `SEPARACAO_PEDIDO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_SERVICOS_AFERICAO

`classe: formulario-materializado` · `18 colunas` · `2.204 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `SERVICOS_AFERICAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `SERVICOS_AFERICAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `SERVICOS_AFERICAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (18 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `SERVICOS_AFERICAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_SERVICO_EXTERNOS_JD

`classe: formulario-materializado` · `9 colunas` · `1.662 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `SERVICO_EXTERNOS_JD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `SERVICO_EXTERNOS_JD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `SERVICO_EXTERNOS_JD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (9 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `SERVICO_EXTERNOS_JD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_SOLICITACAO_TCAT

`classe: formulario-materializado` · `22 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `SOLICITACAO_TCAT`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `SOLICITACAO_TCAT` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `SOLICITACAO_TCAT`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (22 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `SOLICITACAO_TCAT`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_TESTE1

`classe: formulario-materializado` · `3 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `TESTE1`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `TESTE1` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `TESTE1`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `TESTE1`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_TESTE2

`classe: formulario-materializado` · `6 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `TESTE2`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `TESTE2` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `TESTE2`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (6 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `TESTE2`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_TESTE_PRIMEIRO_JD

`classe: formulario-materializado` · `3 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `TESTE_PRIMEIRO_JD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `TESTE_PRIMEIRO_JD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `TESTE_PRIMEIRO_JD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (3 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `TESTE_PRIMEIRO_JD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_TICKET_DSI

`classe: formulario-materializado` · `8 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `TICKET_DSI`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `TICKET_DSI` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `TICKET_DSI`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `TICKET_DSI`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA

`classe: formulario-materializado` · `22 colunas` · `2.806 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (22 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDAPERDIDA_SEGURO

`classe: formulario-materializado` · `4 colunas` · `12 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDAPERDIDA_SEGURO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDAPERDIDA_SEGURO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDAPERDIDA_SEGURO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDAPERDIDA_SEGURO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_AMS

`classe: formulario-materializado` · `65 colunas` · `775 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_AMS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_AMS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_AMS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (65 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_AMS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_CONSORCIO

`classe: formulario-materializado` · `7 colunas` · `47 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_CONSORCIO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_CONSORCIO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_CONSORCIO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (7 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_CONSORCIO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_DIRETA

`classe: formulario-materializado` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_DIRETA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_DIRETA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_DIRETA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_DIRETA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_DSI

`classe: formulario-materializado` · `16 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_DSI`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_DSI` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_DSI`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (16 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_DSI`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_EQUIPAMENTO

`classe: formulario-materializado` · `49 colunas` · `2.256 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_EQUIPAMENTO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_EQUIPAMENTO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_EQUIPAMENTO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (49 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_EQUIPAMENTO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_MAQUINA_FY25

`classe: formulario-materializado` · `45 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_MAQUINA_FY25`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_MAQUINA_FY25` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_MAQUINA_FY25`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (45 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_MAQUINA_FY25`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA

`classe: formulario-materializado` · `14 colunas` · `1.511 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (14 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_FY25

`classe: formulario-materializado` · `13 colunas` · `198 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_FY25`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_FY25` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_FY25`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (13 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_FY25`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_IMPL

`classe: formulario-materializado` · `10 colunas` · `31 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_IMPL`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_IMPL` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_IMPL`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (10 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_IMPL`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_IMPLEM

`classe: formulario-materializado` · `13 colunas` · `125 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_IMPLEM`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_IMPLEM` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_IMPLEM`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (13 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_IMPLEM`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_JDE

`classe: formulario-materializado` · `11 colunas` · `296 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_JDE`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_JDE` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_JDE`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (11 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_JDE`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_MANITO

`classe: formulario-materializado` · `10 colunas` · `6 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_MANITO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_MANITO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_MANITO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (10 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_MANITO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_MAQIMP

`classe: formulario-materializado` · `12 colunas` · `313 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_MAQIMP`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_MAQIMP` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_MAQIMP`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_MAQIMP`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_PROD

`classe: formulario-materializado` · `15 colunas` · `286 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_PROD`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_PROD` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_PROD`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (15 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_PROD`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_SEGURO

`classe: formulario-materializado` · `13 colunas` · `13 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_SEGURO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_SEGURO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_SEGURO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (13 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_SEGURO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PERDIDA_TESTE

`classe: formulario-materializado` · `12 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PERDIDA_TESTE`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PERDIDA_TESTE` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PERDIDA_TESTE`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PERDIDA_TESTE`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PNEUS

`classe: formulario-materializado` · `7 colunas` · `9 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PNEUS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PNEUS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PNEUS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (7 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PNEUS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_PRECISION_UP

`classe: formulario-materializado` · `8 colunas` · `2 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_PRECISION_UP`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_PRECISION_UP` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_PRECISION_UP`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_PRECISION_UP`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_SEMINOVO

`classe: formulario-materializado` · `10 colunas` · `2.481 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_SEMINOVO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_SEMINOVO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_SEMINOVO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (10 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_SEMINOVO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_SERVICOS_AMS

`classe: formulario-materializado` · `8 colunas` · `468 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_SERVICOS_AMS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_SERVICOS_AMS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_SERVICOS_AMS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (8 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_SERVICOS_AMS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDA_VP_PNEUS

`classe: formulario-materializado` · `6 colunas` · `4 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDA_VP_PNEUS`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDA_VP_PNEUS` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDA_VP_PNEUS`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (6 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDA_VP_PNEUS`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VENDER_RENOVACAO_SEG

`classe: formulario-materializado` · `16 colunas` · `153 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VENDER_RENOVACAO_SEG`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VENDER_RENOVACAO_SEG` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VENDER_RENOVACAO_SEG`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (16 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VENDER_RENOVACAO_SEG`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VISITA_DSI

`classe: formulario-materializado` · `4 colunas` · `4 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VISITA_DSI`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VISITA_DSI` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VISITA_DSI`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VISITA_DSI`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VISITA_EXP_CLIENTE

`classe: formulario-materializado` · `11 colunas` · `16 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VISITA_EXP_CLIENTE`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VISITA_EXP_CLIENTE` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VISITA_EXP_CLIENTE`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (11 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VISITA_EXP_CLIENTE`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VP_COLHEDORA

`classe: formulario-materializado` · `11 colunas` · `1 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VP_COLHEDORA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VP_COLHEDORA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VP_COLHEDORA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (11 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VP_COLHEDORA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VP_COLHEITADEIRA

`classe: formulario-materializado` · `12 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VP_COLHEITADEIRA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VP_COLHEITADEIRA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VP_COLHEITADEIRA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VP_COLHEITADEIRA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VP_PLANTADEIRA

`classe: formulario-materializado` · `13 colunas` · `3 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VP_PLANTADEIRA`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VP_PLANTADEIRA` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VP_PLANTADEIRA`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (13 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VP_PLANTADEIRA`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VP_PULVERIZADOR

`classe: formulario-materializado` · `10 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VP_PULVERIZADOR`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VP_PULVERIZADOR` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VP_PULVERIZADOR`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (10 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VP_PULVERIZADOR`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VP_RENOVACAO_SEGURO

`classe: formulario-materializado` · `4 colunas` · `0 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VP_RENOVACAO_SEGURO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VP_RENOVACAO_SEGURO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VP_RENOVACAO_SEGURO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (4 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VP_RENOVACAO_SEGURO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VP_SEM_PARTICIPACAO

`classe: formulario-materializado` · `12 colunas` · `74 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VP_SEM_PARTICIPACAO`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VP_SEM_PARTICIPACAO` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VP_SEM_PARTICIPACAO`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VP_SEM_PARTICIPACAO`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---

### IV_Q_VP_TRATOR

`classe: formulario-materializado` · `12 colunas` · `19 linhas (snapshot 03/06/2026)` · `PK: SEQQUESTIONARIO`

**Funcao:** Materializacao fisica das respostas do formulario `VP_TRATOR`. Cada linha e um questionario respondido (`SEQQUESTIONARIO`) e cada coluna e uma questao do formulario.

**Formulario de origem:** `VP_TRATOR` - tabela fisica gerada por DDL a partir do formulario cuja `IV_Formulario.Descricao` = `VP_TRATOR`. Tem uma coluna por questao (nome vindo de `IV_Questao.Nomecoluna`) e PK `SEQQUESTIONARIO` apontando para `IV_Questionario`.

_Tabela de colunas omitida de proposito (12 colunas geradas): o rotulo legivel de cada coluna so existe nas linhas de `IV_Questao` do formulario `VP_TRATOR`, que exigem acesso ao vivo (ver [LACUNAS.md](LACUNAS.md))._

---
