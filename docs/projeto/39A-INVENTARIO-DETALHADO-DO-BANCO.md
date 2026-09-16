# Inventário detalhado do banco — anexo do documento 39

> Gerado por `scripts/banco/auditoria/gerar-inventario-do-banco.ps1` em 15/09/2026 13:24, somente leitura.
> Fonte: catálogo do banco local `TracbelCrm` (mesma estrutura do banco central: 12 migrações) e o código do repositório.
> "Acesso" conta referências por DbSet, `Set<T>` e SQL em texto (`schema.Tabela`); "menção ao tipo" conta o nome da entidade
> como palavra e é evidência fraca (nomes como `Resultado`, `Fase` e `Meta` colidem com outros tipos). Linhas: banco local
> sanitizado; o banco central tem os mesmos números, salvo `frota.Familia` (22) e `seguranca.Usuario` (2).

## Migrações em ordem

| Migração | Criou | Alterou (além da restrição de domínio de entidade) |
|---|---|---|
| `20260904120040_ModeloInicial` | `auditoria.AlteracaoDeCampo`, `auditoria.CampoAuditado`, `auditoria.EventoDeAcesso`, `comercial.Alerta`, `comercial.CanalContato`, `comercial.Cliente`, `comercial.ClienteCarteira`, `comercial.ClienteContato`, `comercial.ConsentimentoComunicacao`, `comercial.Contato`, `comercial.Endereco`, `comercial.Lead`, `documento.Documento`, `documento.Vinculo`, `frota.Equipamento`, `frota.Familia`, `frota.LeituraDeHorimetro`, `frota.Marca`, `frota.Modelo`, `integracao.ChaveExterna`, `integracao.MensagemDeSaida`, `integracao.MensagemDescartada`, `integracao.PontoDeSincronismo`, `integracao.Recepcao`, `integracao.Sistema`, `metadado.CampoPersonalizado`, `metadado.Catalogo`, `metadado.CatalogoItem`, `metadado.Formulario`, `metadado.Pergunta`, `metadado.Preenchimento`, `metadado.Resposta`, `metadado.TratadorDeEvento`, `organizacao.Carteira`, `organizacao.Empresa`, `organizacao.HierarquiaComercial`, `organizacao.LinhaDeNegocio`, `organizacao.Meta`, `organizacao.Praca`, `processo.Fase`, `processo.Interacao`, `processo.InteracaoParticipante`, `processo.ItemDeProposta`, `processo.MotivoDePerda`, `processo.PassagemDeFase`, `processo.Processo`, `processo.Regra`, `processo.RegraExecucao`, `processo.Resultado`, `processo.Tarefa`, `processo.TipoProcesso`, `processo.TipoTarefa`, `relatorio.Fonte`, `relatorio.FonteCampo`, `relatorio.Relatorio`, `seguranca.CompartilhamentoDeRegistro`, `seguranca.ConjuntoDePermissao`, `seguranca.ConjuntoDePermissaoItem`, `seguranca.Equipe`, `seguranca.EquipeMembro`, `seguranca.Permissao`, `seguranca.Usuario`, `seguranca.UsuarioConjuntoDePermissao` |  |
| `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade` |  | `auditoria.AlteracaoDeCampo`, `auditoria.CampoAuditado`, `comercial.Cliente`, `comercial.ClienteContato`, `comercial.Endereco`, `comercial.Lead`, `documento.Documento`, `metadado.CampoPersonalizado`, `metadado.Catalogo`, `metadado.CatalogoItem`, `processo.ItemDeProposta`, `processo.Processo`, `relatorio.FonteCampo` |
| `20260905191622_MunicipioEAgrupamentoDeCarteira` | `organizacao.CarteiraMunicipio`, `organizacao.Municipio` | `comercial.Endereco` |
| `20260906122942_VendaPerdida` | `processo.VendaPerdida` | `metadado.Catalogo` |
| `20260906134331_NaturezaDeCarteiraEUsuario` |  | `organizacao.Carteira`, `seguranca.Usuario` |
| `20260906144125_FaturamentoEClasseDoCliente` | `comercial.FaturamentoDoCliente` | `comercial.Cliente` |
| `20260908141700_QuebraDeFaturamentoEParceiroSemCliente` | `comercial.FaturamentoSemCliente` | `comercial.FaturamentoDoCliente` |
| `20260910113323_ParticipacaoNaNegociacaoDeixaDeSerBooleanoAnulavel` |  | `processo.VendaPerdida` |
| `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial` | `organizacao.AreaPlantadaNoMunicipio`, `organizacao.MunicipioDaAreaDeAtuacao`, `organizacao.RegraDePotencial`, `organizacao.ResponsavelPeloMunicipio` |  |
| `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo` | `frota.LinhaDeProduto`, `frota.VendaDeMaquina`, `frota.VinculoDeClienteComEquipamento`, `integracao.CompradorPendente`, `integracao.CorrespondenciaDaOrigem`, `integracao.DivergenciaDeIntegracao`, `integracao.RegistroDeOrigem` | `frota.Equipamento` |
| `20260914180619_ModeloPendenteSoNaOrigemArt` |  | `frota.Equipamento` |
| `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao` | `integracao.ExecucaoDeSincronizacao` | `frota.Familia`, `frota.LinhaDeProduto`, `frota.Marca` |

## Resumo por tabela

| Tabela | Entidade | DbSet | Col. | Nulas | FK saída | FK entrada | Índices | CHECK | Identity | Linhas | Acesso (repositório/aplicação/API/carga/integração/infra) | Testes |
|---|---|---|---:|---:|---:|---:|---:|---:|:-:|---:|---|---:|
| `auditoria.AlteracaoDeCampo` | AlteracaoDeCampo | AlteracoesDeCampo | 10 | 3 | 2 | 0 | 5 | 3 | sim | 0 | 0/0/0/8/0/0 | 11 |
| `auditoria.CampoAuditado` | CampoAuditado | CamposAuditados | 5 | 0 | 0 | 0 | 2 | 3 | sim | 0 | 0/0/0/0/0/0 | 2 |
| `auditoria.EventoDeAcesso` | EventoDeAcesso | EventosDeAcesso | 10 | 7 | 1 | 0 | 4 | 3 | sim | 0 | 0/0/0/0/0/1 | 5 |
| `comercial.Alerta` | Alerta | Alertas | 16 | 6 | 2 | 0 | 4 | 2 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `comercial.CanalContato` | CanalContato | CanaisDeContato | 13 | 5 | 3 | 0 | 5 | 2 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `comercial.Cliente` | Cliente | Clientes | 27 | 15 | 6 | 18 | 11 | 6 | sim | 0 | 28/8/2/11/0/0 | 81 |
| `comercial.ClienteCarteira` | ClienteCarteira | ClienteCarteiras | 10 | 4 | 2 | 0 | 3 | 2 | sim | 0 | 7/5/0/4/0/0 | 16 |
| `comercial.ClienteContato` | ClienteContato | ClienteContatos | 8 | 2 | 3 | 0 | 5 | 2 | sim | 0 | 0/0/0/3/0/0 | 1 |
| `comercial.ConsentimentoComunicacao` | ConsentimentoComunicacao | Consentimentos | 13 | 5 | 3 | 0 | 4 | 3 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `comercial.Contato` | Contato | Contatos | 15 | 8 | 2 | 8 | 5 | 1 | sim | 0 | 0/0/0/4/0/0 | 4 |
| `comercial.Endereco` | Endereco | Enderecos | 26 | 15 | 4 | 1 | 7 | 6 | sim | 0 | 3/2/0/4/0/0 | 19 |
| `comercial.FaturamentoDoCliente` | FaturamentoDoCliente | FaturamentoDosClientes | 18 | 4 | 2 | 0 | 4 | 3 | sim | 0 | 7/4/0/3/0/0 | 17 |
| `comercial.FaturamentoSemCliente` | FaturamentoSemCliente | FaturamentoSemClientes | 20 | 4 | 1 | 0 | 5 | 3 | sim | 0 | 3/1/0/3/0/0 | 15 |
| `comercial.Lead` | Lead | Leads | 28 | 17 | 9 | 1 | 13 | 5 | sim | 0 | 0/0/0/0/0/0 | 28 |
| `dbo.__EFMigrationsHistory` |  |  | 2 | 0 | 0 | 0 | 1 | 0 |  | 0 | 0/0/0/0/0/0 | 0 |
| `documento.Documento` | Documento | Documentos | 18 | 5 | 2 | 1 | 5 | 4 | sim | 0 | 0/0/0/0/0/0 | 13 |
| `documento.Vinculo` | Vinculo | VinculosDeDocumento | 6 | 0 | 2 | 0 | 4 | 1 | sim | 0 | 0/0/0/0/0/0 | 1 |
| `frota.Equipamento` | Equipamento | Equipamentos | 27 | 19 | 7 | 8 | 10 | 6 | sim | 0 | 6/5/2/8/0/0 | 14 |
| `frota.Familia` | Familia | Familias | 5 | 0 | 1 | 2 | 2 | 0 | sim | 28 | 5/0/0/3/0/0 | 4 |
| `frota.LeituraDeHorimetro` | LeituraDeHorimetro | LeiturasDeHorimetro | 7 | 1 | 2 | 0 | 3 | 1 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `frota.LinhaDeProduto` | LinhaDeProduto | LinhasDeProduto | 6 | 1 | 1 | 2 | 3 | 1 | sim | 10 | 6/0/0/2/0/0 | 5 |
| `frota.Marca` | Marca | Marcas | 5 | 0 | 0 | 1 | 2 | 0 | sim | 13 | 4/0/0/2/0/0 | 6 |
| `frota.Modelo` | Modelo | Modelos | 7 | 2 | 1 | 3 | 3 | 2 | sim | 253 | 5/0/0/5/0/0 | 45 |
| `frota.VendaDeMaquina` | VendaDeMaquina | VendasDeMaquina | 34 | 19 | 5 | 3 | 7 | 1 | sim | 0 | 4/1/0/3/0/0 | 5 |
| `frota.VinculoDeClienteComEquipamento` | VinculoDeClienteComEquipamento | VinculosComEquipamento | 17 | 9 | 5 | 0 | 7 | 2 | sim | 0 | 2/1/0/3/0/0 | 3 |
| `integracao.ChaveExterna` | ChaveExterna | ChavesExternas | 6 | 0 | 1 | 0 | 3 | 1 | sim | 0 | 0/0/0/27/0/0 | 1 |
| `integracao.CompradorPendente` | CompradorPendente | CompradoresPendentes | 32 | 9 | 2 | 0 | 4 | 6 | sim | 0 | 0/0/0/2/0/0 | 1 |
| `integracao.CorrespondenciaDaOrigem` | CorrespondenciaDaOrigem | CorrespondenciasDaOrigem | 16 | 6 | 5 | 0 | 6 | 4 | sim | 0 | 0/0/0/2/0/0 | 2 |
| `integracao.DivergenciaDeIntegracao` | DivergenciaDeIntegracao | DivergenciasDeIntegracao | 22 | 10 | 4 | 0 | 6 | 2 | sim | 0 | 1/0/0/2/0/0 | 3 |
| `integracao.ExecucaoDeSincronizacao` | ExecucaoDeSincronizacao | ExecucoesDeSincronizacao | 13 | 2 | 1 | 0 | 2 | 2 | sim | 0 | 2/1/0/5/0/0 | 8 |
| `integracao.MensagemDeSaida` | MensagemDeSaida | MensagensDeSaida | 11 | 3 | 1 | 1 | 4 | 4 | sim | 0 | 0/0/0/0/0/0 | 1 |
| `integracao.MensagemDescartada` | MensagemDescartada | MensagensDescartadas | 10 | 4 | 2 | 0 | 4 | 3 | sim | 0 | 0/0/0/9/1/0 | 3 |
| `integracao.PontoDeSincronismo` | PontoDeSincronismo | PontosDeSincronismo | 9 | 0 | 1 | 0 | 3 | 2 | sim | 0 | 0/0/0/11/1/0 | 2 |
| `integracao.Recepcao` | Recepcao | Recepcoes | 10 | 2 | 1 | 0 | 4 | 5 | sim | 0 | 0/0/0/0/0/0 | 4 |
| `integracao.RegistroDeOrigem` | RegistroDeOrigem | RegistrosDeOrigem | 19 | 10 | 2 | 0 | 4 | 2 | sim | 0 | 0/0/0/3/0/0 | 1 |
| `integracao.Sistema` | Sistema | Sistemas | 6 | 1 | 0 | 12 | 2 | 0 | sim | 4 | 4/0/0/6/2/0 | 15 |
| `metadado.__EFMigrationsHistory` |  |  | 2 | 0 | 0 | 0 | 1 | 0 |  | 12 | 0/0/0/0/0/0 | 0 |
| `metadado.CampoPersonalizado` | CampoPersonalizado | CamposPersonalizados | 15 | 5 | 1 | 0 | 3 | 5 | sim | 0 | 0/0/0/0/0/0 | 5 |
| `metadado.Catalogo` | Catalogo | Catalogos | 6 | 1 | 0 | 3 | 2 | 0 | sim | 10 | 2/4/2/0/0/0 | 21 |
| `metadado.CatalogoItem` | CatalogoItem | CatalogoItens | 9 | 2 | 2 | 14 | 6 | 2 | sim | 227 | 5/0/0/5/0/0 | 25 |
| `metadado.Formulario` | Formulario | Formularios | 7 | 2 | 1 | 3 | 3 | 1 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `metadado.Pergunta` | Pergunta | Perguntas | 11 | 3 | 3 | 2 | 5 | 3 | sim | 0 | 0/0/0/0/0/0 | 1 |
| `metadado.Preenchimento` | Preenchimento | Preenchimentos | 11 | 4 | 7 | 1 | 8 | 3 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `metadado.Resposta` | Resposta | Respostas | 12 | 8 | 5 | 0 | 9 | 2 | sim | 0 | 0/0/0/0/0/0 | 9 |
| `metadado.TratadorDeEvento` | TratadorDeEvento | TratadoresDeEvento | 8 | 1 | 0 | 0 | 3 | 2 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `organizacao.AreaPlantadaNoMunicipio` | AreaPlantadaNoMunicipio | AreasPlantadasNosMunicipios | 8 | 1 | 2 | 0 | 4 | 3 | sim | 0 | 2/1/0/2/0/0 | 9 |
| `organizacao.Carteira` | Carteira | Carteiras | 18 | 7 | 6 | 4 | 10 | 1 | sim | 0 | 16/2/0/4/0/0 | 12 |
| `organizacao.CarteiraMunicipio` | CarteiraMunicipio | CarteiraMunicipios | 6 | 1 | 2 | 0 | 3 | 0 | sim | 0 | 2/2/0/3/0/0 | 2 |
| `organizacao.Empresa` | Empresa | Empresas | 11 | 3 | 1 | 32 | 5 | 1 | sim | 18 | 11/0/0/10/2/3 | 56 |
| `organizacao.HierarquiaComercial` | HierarquiaComercial | HierarquiaComercial | 4 | 0 | 2 | 0 | 3 | 1 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `organizacao.LinhaDeNegocio` | LinhaDeNegocio | LinhasDeNegocio | 9 | 4 | 0 | 5 | 2 | 1 | sim | 0 | 6/0/0/7/0/0 | 8 |
| `organizacao.Meta` | Meta | Metas | 18 | 8 | 4 | 0 | 6 | 3 | sim | 0 | 1/2/0/0/0/0 | 5 |
| `organizacao.Municipio` | Municipio | Municipios | 5 | 1 | 0 | 5 | 4 | 2 | sim | 5571 | 9/3/0/8/0/0 | 49 |
| `organizacao.MunicipioDaAreaDeAtuacao` | MunicipioDaAreaDeAtuacao | MunicipiosDaAreaDeAtuacao | 10 | 2 | 3 | 0 | 4 | 2 | sim | 0 | 1/1/0/2/0/0 | 9 |
| `organizacao.Praca` | Praca | Pracas | 9 | 3 | 1 | 1 | 3 | 2 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `organizacao.RegraDePotencial` | RegraDePotencial | RegrasDePotencial | 9 | 0 | 0 | 0 | 2 | 3 | sim | 0 | 1/0/0/0/0/0 | 3 |
| `organizacao.ResponsavelPeloMunicipio` | ResponsavelPeloMunicipio | ResponsaveisPelosMunicipios | 13 | 3 | 3 | 0 | 4 | 5 | sim | 0 | 1/0/0/2/0/0 | 9 |
| `processo.Fase` | Fase | Fases | 9 | 2 | 1 | 4 | 3 | 1 | sim | 0 | 4/0/0/3/0/0 | 2 |
| `processo.Interacao` | Interacao | Interacoes | 20 | 11 | 9 | 6 | 11 | 4 | sim | 0 | 1/1/0/4/0/0 | 5 |
| `processo.InteracaoParticipante` | InteracaoParticipante | InteracaoParticipantes | 6 | 3 | 3 | 0 | 4 | 2 | sim | 0 | 0/0/0/1/0/0 | 0 |
| `processo.ItemDeProposta` | ItemDeProposta | ItensDeProposta | 20 | 9 | 5 | 0 | 7 | 4 | sim | 0 | 0/0/0/1/0/0 | 0 |
| `processo.MotivoDePerda` | MotivoDePerda | MotivosDePerda | 8 | 0 | 0 | 2 | 2 | 1 | sim | 0 | 3/0/0/6/0/0 | 2 |
| `processo.PassagemDeFase` | PassagemDeFase | PassagensDeFase | 10 | 4 | 5 | 0 | 7 | 2 | sim | 0 | 0/0/0/1/0/0 | 0 |
| `processo.Processo` | Processo | Processos | 32 | 17 | 10 | 8 | 13 | 5 | sim | 0 | 9/6/0/3/2/0 | 36 |
| `processo.Regra` | Regra | Regras | 22 | 10 | 5 | 3 | 8 | 2 | sim | 0 | 0/3/0/0/0/0 | 10 |
| `processo.RegraExecucao` | ExecucaoRegra | ExecucoesRegra | 13 | 6 | 6 | 0 | 8 | 1 | sim | 0 | 0/0/0/0/0/0 | 3 |
| `processo.Resultado` | Resultado | Resultados | 9 | 2 | 2 | 3 | 3 | 1 | sim | 0 | 2/0/0/3/0/0 | 18 |
| `processo.Tarefa` | Tarefa | Tarefas | 28 | 16 | 12 | 4 | 14 | 4 | sim | 0 | 2/2/0/5/0/0 | 4 |
| `processo.TipoProcesso` | TipoProcesso | TiposDeProcesso | 6 | 1 | 1 | 5 | 3 | 1 | sim | 0 | 3/0/0/3/0/0 | 2 |
| `processo.TipoTarefa` | TipoTarefa | TiposDeTarefa | 13 | 4 | 2 | 5 | 4 | 3 | sim | 0 | 5/0/0/5/0/0 | 4 |
| `processo.VendaPerdida` | VendaPerdida | VendasPerdidas | 27 | 15 | 7 | 0 | 10 | 6 | sim | 0 | 5/2/0/1/0/0 | 8 |
| `relatorio.Fonte` | Fonte | FontesDeRelatorio | 10 | 3 | 1 | 2 | 3 | 2 | sim | 0 | 0/0/0/0/0/0 | 2 |
| `relatorio.FonteCampo` | FonteCampo | CamposDeFonte | 9 | 0 | 1 | 0 | 2 | 2 | sim | 0 | 0/0/0/0/0/0 | 4 |
| `relatorio.Relatorio` | Relatorio | Relatorios | 14 | 4 | 3 | 0 | 5 | 2 | sim | 0 | 0/0/0/0/0/0 | 1 |
| `seguranca.CompartilhamentoDeRegistro` | CompartilhamentoDeRegistro | CompartilhamentosDeRegistro | 17 | 8 | 3 | 0 | 6 | 4 | sim | 0 | 0/0/0/0/0/0 | 1 |
| `seguranca.ConjuntoDePermissao` | ConjuntoPermissao | ConjuntosPermissao | 5 | 1 | 0 | 2 | 2 | 0 | sim | 0 | 0/0/0/0/0/1 | 9 |
| `seguranca.ConjuntoDePermissaoItem` | ItemConjuntoPermissao |  | 4 | 0 | 1 | 0 | 2 | 1 | sim | 0 | 0/0/0/0/0/1 | 0 |
| `seguranca.Equipe` | Equipe | Equipes | 13 | 5 | 1 | 6 | 3 | 1 | sim | 0 | 0/0/0/0/0/0 | 8 |
| `seguranca.EquipeMembro` | EquipeMembro | EquipeMembros | 6 | 1 | 2 | 0 | 3 | 1 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `seguranca.Permissao` | Permissao | Permissoes | 5 | 0 | 0 | 0 | 2 | 2 | sim | 0 | 0/0/0/0/0/0 | 0 |
| `seguranca.Usuario` | Usuario | Usuarios | 20 | 8 | 2 | 32 | 6 | 2 | sim | 1 | 11/0/0/8/0/5 | 47 |
| `seguranca.UsuarioConjuntoDePermissao` | UsuarioConjuntoPermissao | ConcessoesPermissao | 6 | 1 | 2 | 0 | 3 | 0 | sim | 0 | 0/0/0/0/0/1 | 10 |

## `auditoria.AlteracaoDeCampo`

- **Entidade EF:** `AlteracaoDeCampo` · **DbSet:** `AlteracoesDeCampo` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/DocumentoEAuditoriaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id,AlteradoEm` · **Identity:** Id · **Particionada:** `PS_Mensal_AlteracaoDeCampo`
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint, CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `AlteradoEm` | datetime2(3) |  |  |  |
| 3 | `EmpresaId` | int |  |  |  |
| 4 | `Entidade` | varchar(40) |  |  |  |
| 5 | `RegistroId` | bigint |  |  |  |
| 6 | `Campo` | varchar(60) |  |  |  |
| 7 | `ValorAnterior` | nvarchar(400) | sim |  |  |
| 8 | `ValorNovo` | nvarchar(400) | sim |  |  |
| 9 | `AlteradoPorId` | bigint |  |  |  |
| 10 | `CorrelacaoId` | uniqueidentifier | sim |  |  |

**Referencia (FK de saída):**

- `AlteracaoDeCampo.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `AlteracaoDeCampo.AlteradoPorId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_AlteracaoDeCampo` (clustered, único): Id,AlteradoEm
- `IX_AlteracaoDeCampo_AlteradoPorId` (nonclustered): AlteradoEm,AlteradoPorId
- `IX_AlteracaoDeCampo_CorrelacaoId` (nonclustered): AlteradoEm,CorrelacaoId
- `IX_AlteracaoDeCampo_EmpresaId` (nonclustered): AlteradoEm,EmpresaId
- `IX_AlteracaoDeCampo_Entidade_RegistroId_AlteradoEm` (nonclustered): Entidade,RegistroId,AlteradoEm DESC

**Restrições CHECK:**

- `CK_AlteracaoDeCampo_Campo`: (([Campo]) collate Latin1_General_BIN2 like '[A-Z]%' AND NOT ([Campo]) collate Latin1_General_BIN2 like '%[^A-Za-z0-9]%')
- `CK_AlteracaoDeCampo_Entidade`: (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Latin1_General_BIN2='Usuari …
- `CK_AlteracaoDeCampo_Mudou`: (([ValorAnterior] IS NOT NULL OR [ValorNovo] IS NOT NULL) AND ([ValorAnterior] IS NULL OR [ValorNovo] IS NULL OR [ValorAnterior]<>[ValorNovo]))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 3·3·6 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 8·4·8 | Integracao 0·0·0 | Testes 5·4·6 | Scripts 4·2·4
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (2)`, `src/Tracbel.Crm.Carga/CargaDoArt.cs (4)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (1)`, `src/Tracbel.Crm.Carga/ConsolidacaoDeGrafiasCortadas.cs (1)`, `src/Tracbel.Crm.Dominio/Comercial/Endereco.cs (1)`, `src/Tracbel.Crm.Dominio/Integracao/Integracao.cs (1)`, `src/Tracbel.Crm.Dominio/Organizacao/Municipio.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Carga/ConsolidacaoDeGrafiasCortadasTestes.cs (2)`, `tests/Tracbel.Crm.Aplicacao.Testes/Persistencia/FronteiraDeEmpresaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/DominioDeEntidadeTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)` e mais 1

## `auditoria.CampoAuditado`

- **Entidade EF:** `CampoAuditado` · **DbSet:** `CamposAuditados` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/DocumentoEAuditoriaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint, CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Entidade` | varchar(40) |  |  |  |
| 3 | `Campo` | varchar(60) |  |  |  |
| 4 | `RetencaoMeses` | smallint |  |  |  |
| 5 | `EstaAtivo` | bit |  |  |  |

**Índices:**

- `PK_CampoAuditado` (clustered, único): Id
- `UX_CampoAuditado_Entidade_Campo` (nonclustered, único): Entidade,Campo

**Restrições CHECK:**

- `CK_CampoAuditado_Campo`: (([Campo]) collate Latin1_General_BIN2 like '[A-Z]%' AND NOT ([Campo]) collate Latin1_General_BIN2 like '%[^A-Za-z0-9]%')
- `CK_CampoAuditado_Entidade`: (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Latin1_General_BIN2='Usuari …
- `CK_CampoAuditado_Retencao`: ([RetencaoMeses]>=(1) AND [RetencaoMeses]<=(120))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·1 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 1·1·1 | Scripts 1·1·1
- Arquivos com acesso: `tests/Tracbel.Crm.Arquitetura.Testes/Banco/DominioDeEntidadeTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `auditoria.EventoDeAcesso`

- **Entidade EF:** `EventoDeAcesso` · **DbSet:** `EventosDeAcesso` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/DocumentoEAuditoriaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id,OcorreuEm` · **Identity:** Id · **Particionada:** `PS_Mensal_EventoDeAcesso`
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `OcorreuEm` | datetime2(3) |  |  |  |
| 3 | `UsuarioId` | bigint | sim |  |  |
| 4 | `NomePrincipal` | nvarchar(200) | sim |  |  |
| 5 | `Tipo` | varchar(30) |  |  |  |
| 6 | `Entidade` | varchar(40) | sim |  |  |
| 7 | `RegistroId` | bigint | sim |  |  |
| 8 | `EnderecoIp` | varchar(45) | sim |  |  |
| 9 | `AgenteUsuario` | nvarchar(400) | sim |  |  |
| 10 | `Detalhe` | nvarchar(1000) | sim |  |  |

**Referencia (FK de saída):**

- `EventoDeAcesso.UsuarioId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_EventoDeAcesso` (clustered, único): Id,OcorreuEm
- `IX_EventoDeAcesso_Entidade_RegistroId_OcorreuEm` (nonclustered): Entidade,RegistroId,OcorreuEm DESC · filtro ([Entidade] IS NOT NULL)
- `IX_EventoDeAcesso_Tipo_OcorreuEm` (nonclustered): Tipo,OcorreuEm DESC
- `IX_EventoDeAcesso_UsuarioId_OcorreuEm` (nonclustered): UsuarioId,OcorreuEm DESC

**Restrições CHECK:**

- `CK_EventoDeAcesso_Entidade`: ([Entidade] IS NULL OR (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Lati …
- `CK_EventoDeAcesso_Registro`: ([Entidade] IS NULL AND [RegistroId] IS NULL OR [Entidade] IS NOT NULL AND [RegistroId] IS NOT NULL)
- `CK_EventoDeAcesso_Tipo`: ([Tipo]='LeituraDadoSensivel' OR [Tipo]='ExportacaoDados' OR [Tipo]='AcessoNegado' OR [Tipo]='Logout' OR [Tipo]='LoginFalhou' OR [Tipo]='Login')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 1·1·1 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 2·2·3 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Infraestrutura/Multiempresa/DiarioDeAlcanceEntreEmpresasEmLog.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Persistencia/FronteiraDeEmpresaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `comercial.Alerta`

- **Entidade EF:** `Alerta` · **DbSet:** `Alertas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ClienteId` | bigint |  |  |  |
| 4 | `Severidade` | varchar(20) |  |  |  |
| 5 | `Titulo` | nvarchar(200) |  |  |  |
| 6 | `Detalhe` | nvarchar(2000) | sim |  |  |
| 7 | `VigenteDe` | date |  |  |  |
| 8 | `VigenteAte` | date | sim |  |  |
| 9 | `EstaAtivo` | bit |  |  |  |
| 10 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 11 | `CriadoEm` | datetime2(3) |  |  |  |
| 12 | `CriadoPorId` | bigint |  |  |  |
| 13 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 14 | `AlteradoPorId` | bigint | sim |  |  |
| 15 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 16 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `Alerta.ClienteId` → `comercial.Cliente.Id` (obrigatória)
- `Alerta.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Índices:**

- `PK_Alerta` (clustered, único): Id
- `IX_Alerta_ClienteId` (nonclustered): ClienteId · filtro ([EstaAtivo]=(1) AND [ExcluidoEm] IS NULL)
- `IX_Alerta_EmpresaId` (nonclustered): EmpresaId
- `UX_Alerta_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_Alerta_Severidade`: ([Severidade]='Critico' OR [Severidade]='Atencao' OR [Severidade]='Informativo')
- `CK_Alerta_Vigencia`: ([VigenteAte] IS NULL OR [VigenteAte]>=[VigenteDe])

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `comercial.CanalContato`

- **Entidade EF:** `CanalContato` · **DbSet:** `CanaisDeContato` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ClienteId` | bigint | sim |  |  |
| 4 | `ContatoId` | bigint | sim |  |  |
| 5 | `Tipo` | varchar(20) |  |  |  |
| 6 | `Valor` | nvarchar(200) |  |  |  |
| 7 | `ValorNormalizado` | varchar(200) |  |  |  |
| 8 | `Rotulo` | nvarchar(40) | sim |  |  |
| 9 | `EhPrincipal` | bit |  |  |  |
| 10 | `EhValido` | bit |  |  |  |
| 11 | `ValidadoEm` | datetime2(3) | sim |  |  |
| 12 | `CriadoEm` | datetime2(3) |  |  |  |
| 13 | `ExcluidoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `CanalContato.ClienteId` → `comercial.Cliente.Id` (opcional)
- `CanalContato.ContatoId` → `comercial.Contato.Id` (opcional)
- `CanalContato.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Índices:**

- `PK_CanalContato` (clustered, único): Id
- `IX_CanalContato_ClienteId` (nonclustered): ClienteId · filtro ([ExcluidoEm] IS NULL)
- `IX_CanalContato_ContatoId` (nonclustered): ContatoId · filtro ([ExcluidoEm] IS NULL)
- `IX_CanalContato_EmpresaId` (nonclustered): EmpresaId
- `IX_CanalContato_ValorNormalizado` (nonclustered): ValorNormalizado · filtro ([ExcluidoEm] IS NULL)

**Restrições CHECK:**

- `CK_CanalContato_Tipo`: ([Tipo]='WhatsApp' OR [Tipo]='Celular' OR [Tipo]='Telefone' OR [Tipo]='Email')
- `CK_CanalContato_UmDono`: ([ClienteId] IS NOT NULL AND [ContatoId] IS NULL OR [ClienteId] IS NULL AND [ContatoId] IS NOT NULL)

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·5 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `comercial.Cliente`

- **Entidade EF:** `Cliente` · **DbSet:** `Clientes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint x2, AddForeignKey x2, CreateIndex x2, DropForeignKey x2, DropIndex x2)`; `20260906144125_FaturamentoEClasseDoCliente (AddCheckConstraint, CreateIndex)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `NomeRazao` | nvarchar(200) |  |  |  |
| 4 | `NomeFantasia` | nvarchar(200) | sim |  |  |
| 5 | `TipoDePessoa` | varchar(10) |  |  |  |
| 6 | `Documento` | varchar(14) | sim |  |  |
| 7 | `InscricaoEstadual` | varchar(20) | sim |  |  |
| 8 | `AtividadeEconomica` | varchar(7) | sim |  |  |
| 9 | `Situacao` | varchar(20) |  |  |  |
| 10 | `SituacaoDesde` | datetime2(3) |  |  |  |
| 11 | `MotivoInativacaoId` | int | sim |  |  |
| 12 | `ProprietarioId` | bigint |  |  |  |
| 13 | `ProprietarioEquipeId` | bigint | sim |  |  |
| 14 | `ClienteMatrizId` | bigint | sim |  |  |
| 15 | `OrigemId` | int | sim |  |  |
| 16 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 17 | `CriadoEm` | datetime2(3) |  |  |  |
| 18 | `CriadoPorId` | bigint |  |  |  |
| 19 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 20 | `AlteradoPorId` | bigint | sim |  |  |
| 21 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 22 | `Versao` | timestamp | sim |  |  |
| 23 | `CatalogoDaOrigemId` | int |  |  | ((2)) |
| 24 | `CatalogoDoMotivoInativacaoId` | int |  |  | ((3)) |
| 25 | `Classe` | varchar(1) | sim |  |  |
| 26 | `ClasseApuradaEm` | datetime2(3) | sim |  |  |
| 27 | `FaturamentoApurado` | decimal(18,2) | sim |  |  |

**Referencia (FK de saída):**

- `Cliente.ClienteMatrizId` → `comercial.Cliente.Id` (opcional)
- `Cliente.CatalogoDaOrigemId,OrigemId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `Cliente.CatalogoDoMotivoInativacaoId,MotivoInativacaoId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `Cliente.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Cliente.ProprietarioEquipeId` → `seguranca.Equipe.Id` (opcional)
- `Cliente.ProprietarioId` → `seguranca.Usuario.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `comercial.Alerta.ClienteId`
- `comercial.CanalContato.ClienteId` (opcional)
- `comercial.Cliente.ClienteMatrizId` (opcional)
- `comercial.ClienteCarteira.ClienteId`
- `comercial.ClienteContato.ClienteId`
- `comercial.ConsentimentoComunicacao.ClienteId` (opcional)
- `comercial.Endereco.ClienteId`
- `comercial.FaturamentoDoCliente.ClienteId`
- `comercial.Lead.ClienteGeradoId` (opcional)
- `frota.Equipamento.ClienteId` (opcional)
- `frota.VendaDeMaquina.CompradorId`
- `frota.VinculoDeClienteComEquipamento.ClienteId`
- `metadado.Preenchimento.ClienteId` (opcional)
- `metadado.Resposta.ClienteId` (opcional)
- `processo.Interacao.ClienteId` (opcional)
- `processo.Processo.ClienteId`
- `processo.Tarefa.ClienteId` (opcional)
- `processo.VendaPerdida.ClienteId` (opcional)

**Índices:**

- `PK_Cliente` (clustered, único): Id
- `IX_Cliente_CatalogoDaOrigemId_OrigemId` (nonclustered): CatalogoDaOrigemId,OrigemId
- `IX_Cliente_CatalogoDoMotivoInativacaoId_MotivoInativacaoId` (nonclustered): CatalogoDoMotivoInativacaoId,MotivoInativacaoId
- `IX_Cliente_ClienteMatrizId` (nonclustered): ClienteMatrizId
- `IX_Cliente_EmpresaId_Classe` (nonclustered): EmpresaId,Classe · filtro ([ExcluidoEm] IS NULL)
- `IX_Cliente_EmpresaId_Situacao` (nonclustered): EmpresaId,Situacao · filtro ([ExcluidoEm] IS NULL)
- `IX_Cliente_NomeRazao` (nonclustered): NomeRazao · filtro ([ExcluidoEm] IS NULL)
- `IX_Cliente_ProprietarioEquipeId` (nonclustered): ProprietarioEquipeId
- `IX_Cliente_ProprietarioId` (nonclustered): ProprietarioId · filtro ([ExcluidoEm] IS NULL)
- `UX_Cliente_ChavePublica` (nonclustered, único): ChavePublica
- `UX_Cliente_Empresa_Documento` (nonclustered, único): EmpresaId,Documento · filtro ([Documento] IS NOT NULL AND [ExcluidoEm] IS NULL)

**Restrições CHECK:**

- `CK_Cliente_CatalogoDaOrigemId`: ([CatalogoDaOrigemId]=(2))
- `CK_Cliente_CatalogoDoMotivoInativacaoId`: ([CatalogoDoMotivoInativacaoId]=(3))
- `CK_Cliente_Classe`: ([Classe] IS NULL OR ([Classe]='D' OR [Classe]='C' OR [Classe]='B' OR [Classe]='A'))
- `CK_Cliente_Documento`: ([Documento] IS NULL OR [TipoDePessoa]='Fisica' AND len([Documento])=(11) OR [TipoDePessoa]='Juridica' AND len([Documento])=(14))
- `CK_Cliente_Situacao`: ([Situacao]='Encerrado' OR [Situacao]='ClienteInativo' OR [Situacao]='Cliente' OR [Situacao]='Prospect' OR [Situacao]='Suspect')
- `CK_Cliente_TipoDePessoa`: ([TipoDePessoa]='Juridica' OR [TipoDePessoa]='Fisica')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 4·3·32 | Aplicacao 8·5·20 | Repositorio 28·11·14 | InfraOutros 0·0·4 | Api 2·2·0 | Carga 11·5·27 | Integracao 0·0·9 | Testes 35·14·46 | Scripts 7·3·15
- Arquivos com acesso: `src/Tracbel.Crm.Api/Program.cs (1)`, `src/Tracbel.Crm.Api/Endpoints/EndpointsDeCliente.cs (1)`, `src/Tracbel.Crm.Aplicacao/Clientes/CadastroDeCliente.cs (1)`, `src/Tracbel.Crm.Aplicacao/Clientes/ConsultasDeCliente.cs (3)`, `src/Tracbel.Crm.Aplicacao/Clientes/ContratosDeCliente.cs (1)`, `src/Tracbel.Crm.Aplicacao/Comum/ComProcedencia.cs (1)`, `src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs (2)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.Faturamento.cs (4)`, `src/Tracbel.Crm.Carga/CargaDoArt.cs (1)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (4)`, `src/Tracbel.Crm.Carga/Program.cs (1)` e mais 31

## `comercial.ClienteCarteira`

- **Entidade EF:** `ClienteCarteira` · **DbSet:** `ClienteCarteiras` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `ClienteId` | bigint |  |  |  |
| 3 | `CarteiraId` | bigint |  |  |  |
| 4 | `Classe` | char(1) |  |  |  |
| 5 | `PotencialAnual` | decimal(18,2) | sim |  |  |
| 6 | `DiasCicloContato` | smallint | sim |  |  |
| 7 | `UltimaInteracaoEm` | datetime2(3) | sim |  |  |
| 8 | `VinculadoEm` | datetime2(3) |  |  |  |
| 9 | `VinculadoPorId` | bigint |  |  |  |
| 10 | `DesvinculadoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `ClienteCarteira.ClienteId` → `comercial.Cliente.Id` (obrigatória)
- `ClienteCarteira.CarteiraId` → `organizacao.Carteira.Id` (obrigatória)

**Índices:**

- `PK_ClienteCarteira` (clustered, único): Id
- `IX_ClienteCarteira_CarteiraId_Classe_UltimaInteracaoEm` (nonclustered): CarteiraId,Classe,UltimaInteracaoEm · filtro ([DesvinculadoEm] IS NULL)
- `UX_ClienteCarteira_Cliente_Carteira_Vigente` (nonclustered, único): ClienteId,CarteiraId · filtro ([DesvinculadoEm] IS NULL)

**Restrições CHECK:**

- `CK_ClienteCarteira_Ciclo`: ([DiasCicloContato] IS NULL OR [DiasCicloContato]>(0))
- `CK_ClienteCarteira_Classe`: ([Classe]='D' OR [Classe]='C' OR [Classe]='B' OR [Classe]='A')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·6 | Aplicacao 5·3·5 | Repositorio 7·4·3 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 4·2·12 | Integracao 0·0·0 | Testes 4·4·12 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs (3)`, `src/Tracbel.Crm.Aplicacao/Relacionamento/ObterIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Dominio/Organizacao/Municipio.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCarteiras.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresExecutivos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDoPainelDoCen.cs (1)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (1)` e mais 3

## `comercial.ClienteContato`

- **Entidade EF:** `ClienteContato` · **DbSet:** `ClienteContatos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint, AddForeignKey, CreateIndex, DropForeignKey, DropIndex)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `ClienteId` | bigint |  |  |  |
| 3 | `ContatoId` | bigint |  |  |  |
| 4 | `PapelId` | int |  |  |  |
| 5 | `EhPrincipal` | bit |  |  |  |
| 6 | `IniciouEm` | date | sim |  |  |
| 7 | `EncerrouEm` | date | sim |  |  |
| 8 | `CatalogoDoPapelId` | int |  |  | ((1)) |

**Referencia (FK de saída):**

- `ClienteContato.ClienteId` → `comercial.Cliente.Id` (obrigatória)
- `ClienteContato.ContatoId` → `comercial.Contato.Id` (obrigatória)
- `ClienteContato.CatalogoDoPapelId,PapelId` → `metadado.CatalogoItem.CatalogoId,Id` (obrigatória)

**Índices:**

- `PK_ClienteContato` (clustered, único): Id
- `IX_ClienteContato_CatalogoDoPapelId_PapelId` (nonclustered): CatalogoDoPapelId,PapelId
- `IX_ClienteContato_ContatoId` (nonclustered): ContatoId
- `UX_ClienteContato_Cliente_Contato_Papel` (nonclustered, único): ClienteId,ContatoId,PapelId
- `UX_ClienteContato_Principal` (nonclustered, único): ClienteId · filtro ([EhPrincipal]=(1))

**Restrições CHECK:**

- `CK_ClienteContato_CatalogoDoPapelId`: ([CatalogoDoPapelId]=(1))
- `CK_ClienteContato_Periodo`: ([EncerrouEm] IS NULL OR [IniciouEm] IS NULL OR [EncerrouEm]>=[IniciouEm])

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·2·2 | Integracao 0·0·0 | Testes 0·0·1 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `comercial.ConsentimentoComunicacao`

- **Entidade EF:** `ConsentimentoComunicacao` · **DbSet:** `Consentimentos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ClienteId` | bigint | sim |  |  |
| 4 | `ContatoId` | bigint | sim |  |  |
| 5 | `Canal` | varchar(20) |  |  |  |
| 6 | `Finalidade` | varchar(40) |  |  |  |
| 7 | `Concedido` | bit |  |  |  |
| 8 | `DecididaEm` | datetime2(3) |  |  |  |
| 9 | `OrigemEvidencia` | varchar(60) |  |  |  |
| 10 | `ReferenciaEvidencia` | nvarchar(400) | sim |  |  |
| 11 | `EnderecoIp` | varchar(45) | sim |  |  |
| 12 | `RegistradoPorId` | bigint | sim |  |  |
| 13 | `CriadoEm` | datetime2(3) |  |  |  |

**Referencia (FK de saída):**

- `ConsentimentoComunicacao.ClienteId` → `comercial.Cliente.Id` (opcional)
- `ConsentimentoComunicacao.ContatoId` → `comercial.Contato.Id` (opcional)
- `ConsentimentoComunicacao.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Índices:**

- `PK_ConsentimentoComunicacao` (clustered, único): Id
- `IX_ConsentimentoComunicacao_ClienteId_Canal_Finalidade_DecididaEm` (nonclustered): ClienteId,Canal,Finalidade,DecididaEm
- `IX_ConsentimentoComunicacao_ContatoId_Canal_Finalidade_DecididaEm` (nonclustered): ContatoId,Canal,Finalidade,DecididaEm
- `IX_ConsentimentoComunicacao_EmpresaId` (nonclustered): EmpresaId

**Restrições CHECK:**

- `CK_ConsentimentoComunicacao_Canal`: ([Canal]='Correspondencia' OR [Canal]='Telefone' OR [Canal]='WhatsApp' OR [Canal]='Sms' OR [Canal]='Email')
- `CK_ConsentimentoComunicacao_Finalidade`: ([Finalidade]='Cobranca' OR [Finalidade]='Pesquisa' OR [Finalidade]='Transacional' OR [Finalidade]='Marketing')
- `CK_ConsentimentoComunicacao_UmTitular`: ([ClienteId] IS NOT NULL AND [ContatoId] IS NULL OR [ClienteId] IS NULL AND [ContatoId] IS NOT NULL)

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `comercial.Contato`

- **Entidade EF:** `Contato` · **DbSet:** `Contatos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Sobrenome` | nvarchar(120) | sim |  |  |
| 5 | `Documento` | varchar(14) | sim |  |  |
| 6 | `Cargo` | nvarchar(80) | sim |  |  |
| 7 | `DataNascimento` | date | sim |  |  |
| 8 | `ProprietarioId` | bigint |  |  |  |
| 9 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 10 | `CriadoEm` | datetime2(3) |  |  |  |
| 11 | `CriadoPorId` | bigint |  |  |  |
| 12 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 13 | `AlteradoPorId` | bigint | sim |  |  |
| 14 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 15 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `Contato.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Contato.ProprietarioId` → `seguranca.Usuario.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `comercial.CanalContato.ContatoId` (opcional)
- `comercial.ClienteContato.ContatoId`
- `comercial.ConsentimentoComunicacao.ContatoId` (opcional)
- `comercial.Lead.ContatoGeradoId` (opcional)
- `processo.Interacao.ContatoId` (opcional)
- `processo.InteracaoParticipante.ContatoId` (opcional)
- `processo.Processo.ContatoId` (opcional)
- `processo.Tarefa.ContatoId` (opcional)

**Índices:**

- `PK_Contato` (clustered, único): Id
- `IX_Contato_EmpresaId` (nonclustered): EmpresaId
- `IX_Contato_ProprietarioId` (nonclustered): ProprietarioId · filtro ([ExcluidoEm] IS NULL)
- `UX_Contato_ChavePublica` (nonclustered, único): ChavePublica
- `UX_Contato_Empresa_Documento` (nonclustered, único): EmpresaId,Documento · filtro ([Documento] IS NOT NULL AND [ExcluidoEm] IS NULL)

**Restrições CHECK:**

- `CK_Contato_Documento`: ([Documento] IS NULL OR len([Documento])=(11))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·11 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 4·2·12 | Integracao 0·0·7 | Testes 0·0·4 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `comercial.Endereco`

- **Entidade EF:** `Endereco` · **DbSet:** `Enderecos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint, AddForeignKey, CreateIndex, DropForeignKey, DropIndex)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (AddCheckConstraint, AddForeignKey, CreateIndex)`
- **Campos a conferir (possível duplicação):** `MunicipioId + Municipio`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ClienteId` | bigint |  |  |  |
| 4 | `Tipo` | varchar(20) |  |  |  |
| 5 | `Identificacao` | nvarchar(120) | sim |  |  |
| 6 | `Logradouro` | nvarchar(200) |  |  |  |
| 7 | `Numero` | varchar(20) | sim |  |  |
| 8 | `Complemento` | nvarchar(120) | sim |  |  |
| 9 | `Bairro` | nvarchar(120) | sim |  |  |
| 10 | `Municipio` | nvarchar(120) | sim |  |  |
| 11 | `Uf` | char(2) |  |  |  |
| 12 | `Cep` | char(8) | sim |  |  |
| 13 | `Hectares` | decimal(12,2) | sim |  |  |
| 14 | `CulturaId` | int | sim |  |  |
| 15 | `Latitude` | decimal(10,7) | sim |  |  |
| 16 | `Longitude` | decimal(10,7) | sim |  |  |
| 17 | `EhPrincipal` | bit |  |  |  |
| 18 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 19 | `CriadoEm` | datetime2(3) |  |  |  |
| 20 | `CriadoPorId` | bigint |  |  |  |
| 21 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 22 | `AlteradoPorId` | bigint | sim |  |  |
| 23 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 24 | `Versao` | timestamp | sim |  |  |
| 25 | `CatalogoDaCulturaId` | int |  |  | ((5)) |
| 26 | `MunicipioId` | int | sim |  |  |

**Referencia (FK de saída):**

- `Endereco.ClienteId` → `comercial.Cliente.Id` (obrigatória)
- `Endereco.CatalogoDaCulturaId,CulturaId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `Endereco.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Endereco.MunicipioId` → `organizacao.Municipio.Id` (opcional)

**Dependem dela (FK de entrada):**

- `frota.Equipamento.EnderecoId` (opcional)

**Índices:**

- `PK_Endereco` (clustered, único): Id
- `IX_Endereco_CatalogoDaCulturaId_CulturaId` (nonclustered): CatalogoDaCulturaId,CulturaId
- `IX_Endereco_EmpresaId` (nonclustered): EmpresaId
- `IX_Endereco_MunicipioId` (nonclustered): MunicipioId · filtro ([ExcluidoEm] IS NULL)
- `IX_Endereco_Uf_Municipio` (nonclustered): Uf,Municipio · filtro ([ExcluidoEm] IS NULL)
- `UX_Endereco_ChavePublica` (nonclustered, único): ChavePublica
- `UX_Endereco_Principal` (nonclustered, único): ClienteId · filtro ([EhPrincipal]=(1) AND [ExcluidoEm] IS NULL)

**Restrições CHECK:**

- `CK_Endereco_CatalogoDaCulturaId`: ([CatalogoDaCulturaId]=(5))
- `CK_Endereco_Coordenada`: ([Latitude] IS NULL AND [Longitude] IS NULL OR [Latitude]>=(-90) AND [Latitude]<=(90) AND ([Longitude]>=(-180) AND [Longitude]<=(180)))
- `CK_Endereco_Hectares`: ([Hectares] IS NULL OR [Hectares]>(0))
- `CK_Endereco_Municipio`: ([MunicipioId] IS NOT NULL AND [Municipio] IS NULL OR [MunicipioId] IS NULL AND [Municipio] IS NOT NULL)
- `CK_Endereco_Tipo`: ([Tipo]='Fazenda' OR [Tipo]='Cobranca' OR [Tipo]='Entrega' OR [Tipo]='Fiscal')
- `CK_Endereco_Uf`: (([Uf]) collate Latin1_General_BIN2 like '[A-Z][A-Z]')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 3·3·14 | Aplicacao 2·1·0 | Repositorio 3·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 4·3·16 | Integracao 0·0·14 | Testes 5·3·14 | Scripts 3·1·5
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (2)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/ConsolidacaoDeGrafiasCortadas.cs (1)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (1)`, `src/Tracbel.Crm.Dominio/Organizacao/AreaDeAtuacao.cs (1)`, `src/Tracbel.Crm.Dominio/Organizacao/Municipio.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (3)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Carga/ConsolidacaoDeGrafiasCortadasTestes.cs (3)`, `tests/Tracbel.Crm.Aplicacao.Testes/Territorio/VisaoDaEmpresaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `comercial.FaturamentoDoCliente`

- **Entidade EF:** `FaturamentoDoCliente` · **DbSet:** `FaturamentoDosClientes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260906144125_FaturamentoEClasseDoCliente`
- **Migrações que alteraram:** `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (AddCheckConstraint)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ClienteId` | bigint |  |  |  |
| 4 | `Competencia` | date |  |  |  |
| 5 | `ValorLiquido` | decimal(18,2) |  |  |  |
| 6 | `Notas` | int |  |  |  |
| 7 | `Itens` | int |  |  |  |
| 8 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 9 | `CriadoEm` | datetime2(3) |  |  |  |
| 10 | `CriadoPorId` | bigint |  |  |  |
| 11 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 12 | `AlteradoPorId` | bigint | sim |  |  |
| 13 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 14 | `Versao` | timestamp | sim |  |  |
| 15 | `ValorEmMaquina` | decimal(18,2) |  |  | ((0.0)) |
| 16 | `ValorEmOutros` | decimal(18,2) |  |  | ((0.0)) |
| 17 | `ValorEmPeca` | decimal(18,2) |  |  | ((0.0)) |
| 18 | `ValorEmServico` | decimal(18,2) |  |  | ((0.0)) |

**Referencia (FK de saída):**

- `FaturamentoDoCliente.ClienteId` → `comercial.Cliente.Id` (obrigatória)
- `FaturamentoDoCliente.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Índices:**

- `PK_FaturamentoDoCliente` (clustered, único): Id
- `IX_FaturamentoDoCliente_EmpresaId_Competencia` (nonclustered): EmpresaId,Competencia · filtro ([ExcluidoEm] IS NULL)
- `UX_FaturamentoDoCliente_ChavePublica` (nonclustered, único): ChavePublica
- `UX_FaturamentoDoCliente_Cliente_Empresa_Competencia` (nonclustered, único): ClienteId,EmpresaId,Competencia

**Restrições CHECK:**

- `CK_FaturamentoDoCliente_Competencia`: (datepart(day,[Competencia])=(1))
- `CK_FaturamentoDoCliente_QuebraFecha`: (abs(((([ValorEmMaquina]+[ValorEmPeca])+[ValorEmServico])+[ValorEmOutros])-[ValorLiquido])<=(0.01) OR [ValorEmMaquina]=(0) AND [ValorEmPeca]=(0) AND [ValorEmServico]=(0) AND [ValorEmOutros]=(0))
- `CK_FaturamentoDoCliente_Valor`: ([ValorLiquido]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·6 | Aplicacao 4·3·4 | Repositorio 7·4·1 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·1·1 | Integracao 0·0·0 | Testes 5·5·12 | Scripts 10·3·10
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs (2)`, `src/Tracbel.Crm.Aplicacao/Relacionamento/ObterIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.Faturamento.cs (3)`, `src/Tracbel.Crm.Dominio/Portas/PortasDeIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeFaturamento.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresExecutivos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDoPainelDoCen.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Territorio/VisaoDaEmpresaTestes.cs (1)` e mais 5

## `comercial.FaturamentoSemCliente`

- **Entidade EF:** `FaturamentoSemCliente` · **DbSet:** `FaturamentoSemClientes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ComercialConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260908141700_QuebraDeFaturamentoEParceiroSemCliente`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `Competencia` | date |  |  |  |
| 4 | `Documento` | varchar(14) |  |  |  |
| 5 | `Nome` | nvarchar(120) |  |  |  |
| 6 | `Natureza` | nvarchar(24) |  |  |  |
| 7 | `ValorLiquido` | decimal(18,2) |  |  |  |
| 8 | `ValorEmMaquina` | decimal(18,2) |  |  | ((0.0)) |
| 9 | `ValorEmPeca` | decimal(18,2) |  |  | ((0.0)) |
| 10 | `ValorEmServico` | decimal(18,2) |  |  | ((0.0)) |
| 11 | `ValorEmOutros` | decimal(18,2) |  |  | ((0.0)) |
| 12 | `Notas` | int |  |  |  |
| 13 | `Itens` | int |  |  |  |
| 14 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 15 | `CriadoEm` | datetime2(3) |  |  |  |
| 16 | `CriadoPorId` | bigint |  |  |  |
| 17 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 18 | `AlteradoPorId` | bigint | sim |  |  |
| 19 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 20 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `FaturamentoSemCliente.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Índices:**

- `PK_FaturamentoSemCliente` (clustered, único): Id
- `IX_FaturamentoSemCliente_EmpresaId` (nonclustered): EmpresaId
- `IX_FaturamentoSemCliente_Natureza_Competencia` (nonclustered): Natureza,Competencia · filtro ([ExcluidoEm] IS NULL)
- `UX_FaturamentoSemCliente_ChavePublica` (nonclustered, único): ChavePublica
- `UX_FaturamentoSemCliente_Documento_Empresa_Competencia` (nonclustered, único): Documento,EmpresaId,Competencia

**Restrições CHECK:**

- `CK_FaturamentoSemCliente_Competencia`: (datepart(day,[Competencia])=(1))
- `CK_FaturamentoSemCliente_Natureza`: ([Natureza]='SemDocumento' OR [Natureza]='OutraRevenda' OR [Natureza]='EmpresaDoGrupo' OR [Natureza]='Fabrica' OR [Natureza]='ClienteNaoCadastrado' OR [Natureza]='Indefinida')
- `CK_FaturamentoSemCliente_Valor`: ([ValorLiquido]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·4 | Aplicacao 1·1·1 | Repositorio 3·2·1 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·2·1 | Integracao 0·0·0 | Testes 5·5·10 | Scripts 5·2·5
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ObterIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.Faturamento.cs (2)`, `src/Tracbel.Crm.Carga/CargaDoArt.cs (1)`, `src/Tracbel.Crm.Dominio/Portas/PortasDeIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresExecutivos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Territorio/VisaoDaEmpresaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs (1)`, `scripts/banco/conferencias/conciliacao-das-vendas-do-territorio.sql (2)` e mais 1

## `comercial.Lead`

- **Entidade EF:** `Lead` · **DbSet:** `Leads` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/LeadConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint x2, AddForeignKey x2, CreateIndex x2, DropForeignKey x2, DropIndex x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `NomeContato` | nvarchar(200) |  |  |  |
| 4 | `NomeEmpresa` | nvarchar(200) | sim |  |  |
| 5 | `Email` | nvarchar(200) | sim |  |  |
| 6 | `Telefone` | varchar(20) | sim |  |  |
| 7 | `Documento` | varchar(14) | sim |  |  |
| 8 | `Interesse` | nvarchar(400) | sim |  |  |
| 9 | `OrigemId` | int |  |  |  |
| 10 | `LinhaNegocioId` | int | sim |  |  |
| 11 | `PayloadOriginal` | nvarchar(max) | sim |  |  |
| 12 | `Situacao` | varchar(20) |  |  |  |
| 13 | `ProprietarioId` | bigint |  |  |  |
| 14 | `QualificadoEm` | datetime2(3) | sim |  |  |
| 15 | `QualificadoPorId` | bigint | sim |  |  |
| 16 | `ClienteGeradoId` | bigint | sim |  |  |
| 17 | `ContatoGeradoId` | bigint | sim |  |  |
| 18 | `ProcessoGeradoId` | bigint | sim |  |  |
| 19 | `MotivoDescarteId` | int | sim |  |  |
| 20 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 21 | `CriadoEm` | datetime2(3) |  |  |  |
| 22 | `CriadoPorId` | bigint |  |  |  |
| 23 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 24 | `AlteradoPorId` | bigint | sim |  |  |
| 25 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 26 | `Versao` | timestamp | sim |  |  |
| 27 | `CatalogoDaOrigemId` | int |  |  | ((2)) |
| 28 | `CatalogoDoMotivoDescarteId` | int |  |  | ((4)) |

**Referencia (FK de saída):**

- `Lead.ClienteGeradoId` → `comercial.Cliente.Id` (opcional)
- `Lead.ContatoGeradoId` → `comercial.Contato.Id` (opcional)
- `Lead.CatalogoDaOrigemId,OrigemId` → `metadado.CatalogoItem.CatalogoId,Id` (obrigatória)
- `Lead.CatalogoDoMotivoDescarteId,MotivoDescarteId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `Lead.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Lead.LinhaNegocioId` → `organizacao.LinhaDeNegocio.Id` (opcional)
- `Lead.ProcessoGeradoId` → `processo.Processo.Id` (opcional)
- `Lead.ProprietarioId` → `seguranca.Usuario.Id` (obrigatória)
- `Lead.QualificadoPorId` → `seguranca.Usuario.Id` (opcional)

**Dependem dela (FK de entrada):**

- `processo.Interacao.LeadId` (opcional)

**Índices:**

- `PK_Lead` (clustered, único): Id
- `IX_Lead_CatalogoDaOrigemId_OrigemId` (nonclustered): CatalogoDaOrigemId,OrigemId
- `IX_Lead_CatalogoDoMotivoDescarteId_MotivoDescarteId` (nonclustered): CatalogoDoMotivoDescarteId,MotivoDescarteId
- `IX_Lead_ClienteGeradoId` (nonclustered): ClienteGeradoId
- `IX_Lead_ContatoGeradoId` (nonclustered): ContatoGeradoId
- `IX_Lead_Documento` (nonclustered): Documento · filtro ([Documento] IS NOT NULL AND [ExcluidoEm] IS NULL)
- `IX_Lead_Email` (nonclustered): Email · filtro ([Email] IS NOT NULL AND [ExcluidoEm] IS NULL)
- `IX_Lead_EmpresaId_Situacao_CriadoEm` (nonclustered): EmpresaId,Situacao,CriadoEm · filtro ([ExcluidoEm] IS NULL)
- `IX_Lead_LinhaNegocioId` (nonclustered): LinhaNegocioId
- `IX_Lead_ProcessoGeradoId` (nonclustered): ProcessoGeradoId
- `IX_Lead_ProprietarioId_Situacao` (nonclustered): ProprietarioId,Situacao · filtro ([ExcluidoEm] IS NULL)
- `IX_Lead_QualificadoPorId` (nonclustered): QualificadoPorId
- `UX_Lead_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_Lead_CatalogoDaOrigemId`: ([CatalogoDaOrigemId]=(2))
- `CK_Lead_CatalogoDoMotivoDescarteId`: ([CatalogoDoMotivoDescarteId]=(4))
- `CK_Lead_PayloadOriginalJson`: ([PayloadOriginal] IS NULL OR isjson([PayloadOriginal])=(1))
- `CK_Lead_Qualificacao`: ([Situacao]<>'Qualificado' OR [QualificadoEm] IS NOT NULL AND [QualificadoPorId] IS NOT NULL AND [ClienteGeradoId] IS NOT NULL)
- `CK_Lead_Situacao`: ([Situacao]='Duplicado' OR [Situacao]='Descartado' OR [Situacao]='Qualificado' OR [Situacao]='EmContato' OR [Situacao]='Novo')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 2·1·17 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·1 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·4 | Testes 13·1·15 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (2)`, `tests/Tracbel.Crm.Aplicacao.Testes/Persistencia/FiltroSegurancaTestes.cs (13)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `dbo.__EFMigrationsHistory`

- **Entidade EF:** — (sem entidade) · **DbSet:** — (sem DbSet) · **Configuração:** —
- **Linhas:** 0 · **Chave primária:** `MigrationId` · **Identity:** não
- **Migração que criou:** —

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `MigrationId` | nvarchar(150) |  |  |  |
| 2 | `ProductVersion` | nvarchar(32) |  |  |  |

**Índices:**

- `PK___EFMigrationsHistory` (clustered, único): MigrationId

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·0 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 1·1·0
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `documento.Documento`

- **Entidade EF:** `Documento` · **DbSet:** `Documentos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/DocumentoEAuditoriaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint, AddForeignKey, CreateIndex, DropForeignKey, DropIndex)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `TipoDocumentoId` | int |  |  |  |
| 4 | `Nome` | nvarchar(260) |  |  |  |
| 5 | `TipoConteudo` | varchar(120) |  |  |  |
| 6 | `TamanhoBytes` | bigint |  |  |  |
| 7 | `ResumoConteudo` | char(64) |  |  |  |
| 8 | `CaminhoArmazenamento` | nvarchar(600) |  |  |  |
| 9 | `NumeroVersao` | int |  |  |  |
| 10 | `ValidoAte` | date | sim |  |  |
| 11 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 12 | `CriadoEm` | datetime2(3) |  |  |  |
| 13 | `CriadoPorId` | bigint |  |  |  |
| 14 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 15 | `AlteradoPorId` | bigint | sim |  |  |
| 16 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 17 | `Versao` | timestamp | sim |  |  |
| 18 | `CatalogoDoTipoDocumentoId` | int |  |  | ((6)) |

**Referencia (FK de saída):**

- `Documento.CatalogoDoTipoDocumentoId,TipoDocumentoId` → `metadado.CatalogoItem.CatalogoId,Id` (obrigatória)
- `Documento.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `documento.Vinculo.DocumentoId`

**Índices:**

- `PK_Documento` (clustered, único): Id
- `IX_Documento_CatalogoDoTipoDocumentoId_TipoDocumentoId` (nonclustered): CatalogoDoTipoDocumentoId,TipoDocumentoId
- `IX_Documento_EmpresaId` (nonclustered): EmpresaId
- `IX_Documento_ResumoConteudo` (nonclustered): ResumoConteudo · filtro ([ExcluidoEm] IS NULL)
- `UX_Documento_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_Documento_CatalogoDoTipoDocumentoId`: ([CatalogoDoTipoDocumentoId]=(6))
- `CK_Documento_NumeroVersao`: ([NumeroVersao]>=(1))
- `CK_Documento_Resumo`: (NOT ([ResumoConteudo]) collate Latin1_General_BIN2 like '%[^0-9a-f]%')
- `CK_Documento_Tamanho`: ([TamanhoBytes]>(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·23 | Aplicacao 0·0·18 | Repositorio 0·0·6 | InfraOutros 0·0·0 | Api 0·0·1 | Carga 0·0·40 | Integracao 0·0·30 | Testes 0·0·13 | Scripts 3·1·8
- Arquivos com acesso: `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `documento.Vinculo`

- **Entidade EF:** `Vinculo` · **DbSet:** `VinculosDeDocumento` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/DocumentoEAuditoriaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `DocumentoId` | bigint |  |  |  |
| 3 | `Entidade` | varchar(40) |  |  |  |
| 4 | `RegistroId` | bigint |  |  |  |
| 5 | `VinculadoEm` | datetime2(3) |  |  |  |
| 6 | `VinculadoPorId` | bigint |  |  |  |

**Referencia (FK de saída):**

- `Vinculo.DocumentoId` → `documento.Documento.Id` (obrigatória) · CASCADE
- `Vinculo.VinculadoPorId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_Vinculo` (clustered, único): Id
- `IX_Vinculo_Entidade_RegistroId` (nonclustered): Entidade,RegistroId
- `IX_Vinculo_VinculadoPorId` (nonclustered): VinculadoPorId
- `UX_Vinculo_Documento_Entidade_Registro` (nonclustered, único): DocumentoId,Entidade,RegistroId

**Restrições CHECK:**

- `CK_Vinculo_Entidade`: (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Latin1_General_BIN2='Usuari …

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·1 | Integracao 0·0·0 | Testes 0·0·1 | Scripts 3·1·5
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `frota.Equipamento`

- **Entidade EF:** `Equipamento` · **DbSet:** `Equipamentos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/FrotaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (AddCheckConstraint x3, AddForeignKey, CreateIndex, DropCheckConstraint x2)`; `20260914180619_ModeloPendenteSoNaOrigemArt (AddCheckConstraint, DropCheckConstraint)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ClienteId` | bigint | sim |  |  |
| 4 | `ModeloId` | int | sim |  |  |
| 5 | `Chassi` | varchar(40) |  |  |  |
| 6 | `NumeroSerie` | varchar(40) | sim |  |  |
| 7 | `Placa` | varchar(10) | sim |  |  |
| 8 | `AnoFabricacao` | smallint | sim |  |  |
| 9 | `AnoModelo` | smallint | sim |  |  |
| 10 | `HorimetroAtual` | decimal(12,2) | sim |  |  |
| 11 | `HorimetroAtualizadoEm` | datetime2(3) | sim |  |  |
| 12 | `LocalizacaoDescrita` | nvarchar(200) | sim |  |  |
| 13 | `EnderecoId` | bigint | sim |  |  |
| 14 | `EquipamentoPaiId` | bigint | sim |  |  |
| 15 | `EquipamentoSubstitutoId` | bigint | sim |  |  |
| 16 | `Situacao` | varchar(30) |  |  |  |
| 17 | `Origem` | varchar(20) |  |  |  |
| 18 | `VendidoEm` | date | sim |  |  |
| 19 | `GarantiaAte` | date | sim |  |  |
| 20 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 21 | `CriadoEm` | datetime2(3) |  |  |  |
| 22 | `CriadoPorId` | bigint |  |  |  |
| 23 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 24 | `AlteradoPorId` | bigint | sim |  |  |
| 25 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 26 | `Versao` | timestamp | sim |  |  |
| 28 | `LinhaDeProdutoId` | int | sim |  |  |

**Referencia (FK de saída):**

- `Equipamento.ClienteId` → `comercial.Cliente.Id` (opcional)
- `Equipamento.EnderecoId` → `comercial.Endereco.Id` (opcional)
- `Equipamento.EquipamentoPaiId` → `frota.Equipamento.Id` (opcional)
- `Equipamento.EquipamentoSubstitutoId` → `frota.Equipamento.Id` (opcional)
- `Equipamento.LinhaDeProdutoId` → `frota.LinhaDeProduto.Id` (opcional)
- `Equipamento.ModeloId` → `frota.Modelo.Id` (opcional)
- `Equipamento.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `frota.Equipamento.EquipamentoPaiId` (opcional)
- `frota.Equipamento.EquipamentoSubstitutoId` (opcional)
- `frota.LeituraDeHorimetro.EquipamentoId`
- `frota.VendaDeMaquina.EquipamentoId`
- `frota.VinculoDeClienteComEquipamento.EquipamentoId`
- `integracao.DivergenciaDeIntegracao.EquipamentoId` (opcional)
- `metadado.Resposta.EquipamentoId` (opcional)
- `processo.ItemDeProposta.EquipamentoId` (opcional)

**Índices:**

- `PK_Equipamento` (clustered, único): Id
- `IX_Equipamento_ClienteId_Origem` (nonclustered): ClienteId,Origem · filtro ([ExcluidoEm] IS NULL)
- `IX_Equipamento_EmpresaId` (nonclustered): EmpresaId
- `IX_Equipamento_EnderecoId` (nonclustered): EnderecoId
- `IX_Equipamento_EquipamentoPaiId` (nonclustered): EquipamentoPaiId
- `IX_Equipamento_EquipamentoSubstitutoId` (nonclustered): EquipamentoSubstitutoId
- `IX_Equipamento_LinhaDeProdutoId` (nonclustered): LinhaDeProdutoId
- `IX_Equipamento_ModeloId` (nonclustered): ModeloId
- `UX_Equipamento_Chassi` (nonclustered, único): Chassi · filtro ([ExcluidoEm] IS NULL)
- `UX_Equipamento_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_Equipamento_Ano`: (([AnoFabricacao] IS NULL OR [AnoFabricacao]>=(1900) AND [AnoFabricacao]<=(2100)) AND ([AnoModelo] IS NULL OR [AnoModelo]>=(1900) AND [AnoModelo]<=(2100)))
- `CK_Equipamento_Hierarquia`: (([EquipamentoPaiId] IS NULL OR [EquipamentoPaiId]<>[Id]) AND ([EquipamentoSubstitutoId] IS NULL OR [EquipamentoSubstitutoId]<>[Id]))
- `CK_Equipamento_Horimetro`: ([HorimetroAtual] IS NULL OR [HorimetroAtual]>=(0))
- `CK_Equipamento_ModeloPendente`: ([ModeloId] IS NOT NULL OR [Origem]='Art')
- `CK_Equipamento_Origem`: ([Origem]='Art' OR [Origem]='Crm' OR [Origem]='Protheus')
- `CK_Equipamento_Situacao`: ([Situacao]='ProprietarioNaoConfirmado' OR [Situacao]='Baixado' OR [Situacao]='Vendido' OR [Situacao]='Ativo' OR [Situacao]='Estoque')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·19 | Aplicacao 5·3·9 | Repositorio 6·2·19 | InfraOutros 0·0·4 | Api 2·2·1 | Carga 8·4·18 | Integracao 0·0·4 | Testes 6·5·8 | Scripts 8·3·10
- Arquivos com acesso: `src/Tracbel.Crm.Api/Program.cs (1)`, `src/Tracbel.Crm.Api/Endpoints/EndpointsDeEquipamento.cs (1)`, `src/Tracbel.Crm.Aplicacao/Equipamentos/CadastroDeEquipamento.cs (1)`, `src/Tracbel.Crm.Aplicacao/Equipamentos/ConsultasDeEquipamento.cs (3)`, `src/Tracbel.Crm.Aplicacao/Equipamentos/ContratosDeEquipamento.cs (1)`, `src/Tracbel.Crm.Carga/CargaDoArt.cs (3)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/CatalogoDeFrotaDaCarga.cs (1)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Dominio/Comum/Paginacao.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeEquipamentos.cs (4)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeHistoricoComercial.cs (2)` e mais 8

## `frota.Familia`

- **Entidade EF:** `Familia` · **DbSet:** `Familias` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/FrotaConfiguracao.cs`
- **Linhas:** 28 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (Sql)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `MarcaId` | int |  |  |  |
| 3 | `Codigo` | varchar(20) |  |  |  |
| 4 | `Nome` | nvarchar(80) |  |  |  |
| 5 | `EstaAtiva` | bit |  |  |  |

**Referencia (FK de saída):**

- `Familia.MarcaId` → `frota.Marca.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `frota.LinhaDeProduto.FamiliaId` (opcional)
- `frota.Modelo.FamiliaId`

**Índices:**

- `PK_Familia` (clustered, único): Id
- `UX_Familia_Marca_Codigo` (nonclustered, único): MarcaId,Codigo

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 2·2·7 | Aplicacao 0·0·3 | Repositorio 5·2·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·1·2 | Integracao 0·0·0 | Testes 2·2·2 | Scripts 8·3·18
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CatalogoDeFrotaDaCarga.cs (3)`, `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (1)`, `src/Tracbel.Crm.Dominio/Processo/VendaPerdida.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCatalogos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeEquipamentos.cs (3)`, `tests/Tracbel.Crm.Api.Testes/ApiEmMemoria.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Cadastro/BancoDeTeste.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/banco/seed/01-dados-de-referencia.sql (6)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (1)`

## `frota.LeituraDeHorimetro`

- **Entidade EF:** `LeituraDeHorimetro` · **DbSet:** `LeiturasDeHorimetro` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/FrotaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EquipamentoId` | bigint |  |  |  |
| 3 | `LidaEm` | datetime2(3) |  |  |  |
| 4 | `Horas` | decimal(12,2) |  |  |  |
| 5 | `Fonte` | varchar(40) |  |  |  |
| 6 | `RegistradoPorId` | bigint | sim |  |  |
| 7 | `CriadoEm` | datetime2(3) |  |  |  |

**Referencia (FK de saída):**

- `LeituraDeHorimetro.EquipamentoId` → `frota.Equipamento.Id` (obrigatória)
- `LeituraDeHorimetro.RegistradoPorId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_LeituraDeHorimetro` (clustered, único): Id
- `IX_LeituraDeHorimetro_RegistradoPorId` (nonclustered): RegistradoPorId
- `UX_LeituraDeHorimetro_Equipamento_Instante` (nonclustered, único): EquipamentoId,LidaEm DESC

**Restrições CHECK:**

- `CK_LeituraDeHorimetro_Horas`: ([Horas]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `frota.LinhaDeProduto`

- **Entidade EF:** `LinhaDeProduto` · **DbSet:** `LinhasDeProduto` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoDeOrigemConfiguracao.cs`
- **Linhas:** 10 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo`
- **Migrações que alteraram:** `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (Sql)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(40) |  |  |  |
| 3 | `Nome` | nvarchar(80) |  |  |  |
| 4 | `FamiliaId` | int | sim |  |  |
| 5 | `Porte` | varchar(20) |  |  |  |
| 6 | `EstaAtiva` | bit |  |  |  |

**Referencia (FK de saída):**

- `LinhaDeProduto.FamiliaId` → `frota.Familia.Id` (opcional)

**Dependem dela (FK de entrada):**

- `frota.Equipamento.LinhaDeProdutoId` (opcional)
- `integracao.CorrespondenciaDaOrigem.LinhaDeProdutoId` (opcional)

**Índices:**

- `PK_LinhaDeProduto` (clustered, único): Id
- `IX_LinhaDeProduto_FamiliaId` (nonclustered): FamiliaId
- `UX_LinhaDeProduto_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_LinhaDeProduto_Porte`: ([Porte]='Grande' OR [Porte]='Medio' OR [Porte]='Pequeno' OR [Porte]='NaoSeAplica')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·5 | Aplicacao 0·0·5 | Repositorio 6·3·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 2·1·3 | Integracao 0·0·0 | Testes 2·2·3 | Scripts 6·3·6
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDoArt.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCatalogos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeEquipamentos.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeHistoricoComercial.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IntegracaoDoArtTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/banco/seed/01-dados-de-referencia.sql (4)`, `scripts/deploy/verificar-sincronizacao.ps1 (1)`

## `frota.Marca`

- **Entidade EF:** `Marca` · **DbSet:** `Marcas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/FrotaConfiguracao.cs`
- **Linhas:** 13 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (Sql)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(20) |  |  |  |
| 3 | `Nome` | nvarchar(80) |  |  |  |
| 4 | `EhRepresentada` | bit |  |  |  |
| 5 | `EstaAtiva` | bit |  |  |  |

**Dependem dela (FK de entrada):**

- `frota.Familia.MarcaId`

**Índices:**

- `PK_Marca` (clustered, único): Id
- `UX_Marca_Codigo` (nonclustered, único): Codigo

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·12 | Aplicacao 0·0·8 | Repositorio 4·2·1 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 2·1·8 | Integracao 0·0·5 | Testes 2·2·4 | Scripts 9·3·12
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CatalogoDeFrotaDaCarga.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCatalogos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeEquipamentos.cs (2)`, `tests/Tracbel.Crm.Api.Testes/ApiEmMemoria.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Cadastro/BancoDeTeste.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/banco/seed/01-dados-de-referencia.sql (7)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (1)`

## `frota.Modelo`

- **Entidade EF:** `Modelo` · **DbSet:** `Modelos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/FrotaConfiguracao.cs`
- **Linhas:** 253 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `FamiliaId` | int |  |  |  |
| 3 | `Codigo` | varchar(40) |  |  |  |
| 4 | `Nome` | nvarchar(120) |  |  |  |
| 5 | `PotenciaCv` | smallint | sim |  |  |
| 6 | `IntervaloManutencaoHoras` | int | sim |  |  |
| 7 | `EstaAtivo` | bit |  |  |  |

**Referencia (FK de saída):**

- `Modelo.FamiliaId` → `frota.Familia.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `frota.Equipamento.ModeloId` (opcional)
- `integracao.CorrespondenciaDaOrigem.ModeloId` (opcional)
- `processo.ItemDeProposta.ModeloId` (opcional)

**Índices:**

- `PK_Modelo` (clustered, único): Id
- `IX_Modelo_FamiliaId` (nonclustered): FamiliaId
- `UX_Modelo_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_Modelo_IntervaloManutencao`: ([IntervaloManutencaoHoras] IS NULL OR [IntervaloManutencaoHoras]>(0))
- `CK_Modelo_Potencia`: ([PotenciaCv] IS NULL OR [PotenciaCv]>(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 2·2·11 | Aplicacao 0·0·16 | Repositorio 5·3·4 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 5·2·6 | Integracao 0·0·9 | Testes 3·3·42 | Scripts 8·3·10
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDoArt.cs (2)`, `src/Tracbel.Crm.Carga/CatalogoDeFrotaDaCarga.cs (3)`, `src/Tracbel.Crm.Dominio/Integracao/PonteDeLeitura.cs (1)`, `src/Tracbel.Crm.Dominio/Organizacao/AreaDeAtuacao.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCatalogos.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeEquipamentos.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeHistoricoComercial.cs (1)`, `tests/Tracbel.Crm.Api.Testes/ApiEmMemoria.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IntegracaoDoArtTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Cadastro/BancoDeTeste.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/banco/seed/01-dados-de-referencia.sql (5)` e mais 1

## `frota.VendaDeMaquina`

- **Entidade EF:** `VendaDeMaquina` · **DbSet:** `VendasDeMaquina` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoDeOrigemConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo`
- **Campos a conferir (possível duplicação):** `EmpresaId + EmpresaNaOrigem`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `EmpresaDoFaturamentoId` | int | sim |  |  |
| 4 | `EquipamentoId` | bigint |  |  |  |
| 5 | `CompradorId` | bigint |  |  |  |
| 6 | `SistemaId` | int |  |  |  |
| 7 | `ChaveOrigem` | varchar(60) |  |  |  |
| 8 | `VendidaEm` | date | sim |  |  |
| 9 | `FaturadaEm` | date | sim |  |  |
| 10 | `EntregueEm` | date | sim |  |  |
| 11 | `RegistradaNaOrigemEm` | datetime2(3) | sim |  |  |
| 12 | `NumeroDoPedido` | varchar(20) | sim |  |  |
| 13 | `NumeroDaNotaFiscal` | varchar(60) | sim |  |  |
| 14 | `SituacaoNaOrigem` | varchar(10) | sim |  |  |
| 15 | `GestaoNaOrigem` | nvarchar(30) | sim |  |  |
| 16 | `VendaDireta` | bit |  |  |  |
| 17 | `RepasseDireto` | bit |  |  |  |
| 18 | `Quantidade` | int | sim |  |  |
| 19 | `LinhaNaOrigem` | nvarchar(60) |  |  |  |
| 20 | `ProdutoNaOrigem` | nvarchar(60) |  |  |  |
| 21 | `EmpresaNaOrigem` | nvarchar(60) | sim |  |  |
| 22 | `UnidadeNaOrigem` | nvarchar(80) | sim |  |  |
| 23 | `UnidadeDoFaturamentoNaOrigem` | nvarchar(80) | sim |  |  |
| 24 | `HashDaOrigem` | varchar(64) |  |  |  |
| 25 | `Transformacoes` | nvarchar(1000) | sim |  |  |
| 26 | `ImportadaEm` | datetime2(3) |  |  |  |
| 27 | `AtualizadaPelaOrigemEm` | datetime2(3) | sim |  |  |
| 28 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 29 | `CriadoEm` | datetime2(3) |  |  |  |
| 30 | `CriadoPorId` | bigint |  |  |  |
| 31 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 32 | `AlteradoPorId` | bigint | sim |  |  |
| 33 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 34 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `VendaDeMaquina.CompradorId` → `comercial.Cliente.Id` (obrigatória)
- `VendaDeMaquina.EquipamentoId` → `frota.Equipamento.Id` (obrigatória)
- `VendaDeMaquina.SistemaId` → `integracao.Sistema.Id` (obrigatória)
- `VendaDeMaquina.EmpresaDoFaturamentoId` → `organizacao.Empresa.Id` (opcional)
- `VendaDeMaquina.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `frota.VinculoDeClienteComEquipamento.VendaDeMaquinaId` (opcional)
- `integracao.DivergenciaDeIntegracao.VendaDeMaquinaId` (opcional)
- `integracao.RegistroDeOrigem.VendaDeMaquinaId` (opcional)

**Índices:**

- `PK_VendaDeMaquina` (clustered, único): Id
- `IX_VendaDeMaquina_CompradorId` (nonclustered): CompradorId
- `IX_VendaDeMaquina_EmpresaDoFaturamentoId` (nonclustered): EmpresaDoFaturamentoId
- `IX_VendaDeMaquina_EmpresaId` (nonclustered): EmpresaId
- `IX_VendaDeMaquina_EquipamentoId_VendidaEm` (nonclustered): EquipamentoId,VendidaEm
- `UX_VendaDeMaquina_ChavePublica` (nonclustered, único): ChavePublica
- `UX_VendaDeMaquina_Sistema_ChaveOrigem` (nonclustered, único): SistemaId,ChaveOrigem

**Restrições CHECK:**

- `CK_VendaDeMaquina_Quantidade`: ([Quantidade] IS NULL OR [Quantidade]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 1·1·1 | Repositorio 4·2·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·1·3 | Integracao 0·0·0 | Testes 1·1·4 | Scripts 5·2·5
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Equipamentos/ConsultasDeEquipamento.cs (1)`, `src/Tracbel.Crm.Carga/CargaDoArt.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeEquipamentos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeHistoricoComercial.cs (2)`, `tests/Tracbel.Crm.Api.Testes/IntegracaoDoArtTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/deploy/verificar-sincronizacao.ps1 (2)`

## `frota.VinculoDeClienteComEquipamento`

- **Entidade EF:** `VinculoDeClienteComEquipamento` · **DbSet:** `VinculosComEquipamento` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoDeOrigemConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ClienteId` | bigint |  |  |  |
| 4 | `EquipamentoId` | bigint |  |  |  |
| 5 | `Natureza` | varchar(30) |  |  |  |
| 6 | `VendaDeMaquinaId` | bigint | sim |  |  |
| 7 | `SistemaId` | int | sim |  |  |
| 8 | `ReferenciaEm` | date | sim |  |  |
| 9 | `EncerradoEm` | datetime2(3) | sim |  |  |
| 10 | `MotivoDoEncerramento` | nvarchar(200) | sim |  |  |
| 11 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 12 | `CriadoEm` | datetime2(3) |  |  |  |
| 13 | `CriadoPorId` | bigint |  |  |  |
| 14 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 15 | `AlteradoPorId` | bigint | sim |  |  |
| 16 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 17 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `VinculoDeClienteComEquipamento.ClienteId` → `comercial.Cliente.Id` (obrigatória)
- `VinculoDeClienteComEquipamento.EquipamentoId` → `frota.Equipamento.Id` (obrigatória)
- `VinculoDeClienteComEquipamento.VendaDeMaquinaId` → `frota.VendaDeMaquina.Id` (opcional)
- `VinculoDeClienteComEquipamento.SistemaId` → `integracao.Sistema.Id` (opcional)
- `VinculoDeClienteComEquipamento.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Índices:**

- `PK_VinculoDeClienteComEquipamento` (clustered, único): Id
- `IX_VinculoDeClienteComEquipamento_ClienteId` (nonclustered): ClienteId
- `IX_VinculoDeClienteComEquipamento_EmpresaId` (nonclustered): EmpresaId
- `IX_VinculoDeClienteComEquipamento_EquipamentoId_Natureza` (nonclustered): EquipamentoId,Natureza
- `IX_VinculoDeClienteComEquipamento_SistemaId` (nonclustered): SistemaId
- `UX_VinculoDeClienteComEquipamento_ChavePublica` (nonclustered, único): ChavePublica
- `UX_VinculoDeClienteComEquipamento_Venda_Cliente_Natureza` (nonclustered, único): VendaDeMaquinaId,ClienteId,Natureza · filtro ([VendaDeMaquinaId] IS NOT NULL AND [EncerradoEm] IS NULL)

**Restrições CHECK:**

- `CK_VinculoDeClienteComEquipamento_Encerramento`: ([EncerradoEm] IS NULL AND [MotivoDoEncerramento] IS NULL OR [EncerradoEm] IS NOT NULL AND [MotivoDoEncerramento] IS NOT NULL)
- `CK_VinculoDeClienteComEquipamento_Natureza`: ([Natureza]='CompradorNaVenda')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 1·1·1 | Repositorio 2·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·1·2 | Integracao 0·0·0 | Testes 1·1·2 | Scripts 4·2·4
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Equipamentos/ConsultasDeEquipamento.cs (1)`, `src/Tracbel.Crm.Carga/CargaDoArt.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeHistoricoComercial.cs (2)`, `tests/Tracbel.Crm.Api.Testes/IntegracaoDoArtTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/deploy/verificar-sincronizacao.ps1 (1)`

## `integracao.ChaveExterna`

- **Entidade EF:** `ChaveExterna` · **DbSet:** `ChavesExternas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `SistemaId` | int |  |  |  |
| 3 | `Entidade` | varchar(40) |  |  |  |
| 4 | `RegistroId` | bigint |  |  |  |
| 5 | `ChaveOrigem` | varchar(200) |  |  |  |
| 6 | `SincronizadoEm` | datetime2(3) |  |  |  |

**Referencia (FK de saída):**

- `ChaveExterna.SistemaId` → `integracao.Sistema.Id` (obrigatória)

**Índices:**

- `PK_ChaveExterna` (clustered, único): Id
- `IX_ChaveExterna_Entidade_RegistroId` (nonclustered): Entidade,RegistroId
- `UX_ChaveExterna_Sistema_Entidade_Chave` (nonclustered, único): SistemaId,Entidade,ChaveOrigem

**Restrições CHECK:**

- `CK_ChaveExterna_Entidade`: (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Latin1_General_BIN2='Usuari …

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 27·5·23 | Integracao 0·0·1 | Testes 0·0·1 | Scripts 4·2·4
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (14)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.VendaPerdida.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (1)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (9)`, `src/Tracbel.Crm.Carga/Program.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (1)`

## `integracao.CompradorPendente`

- **Entidade EF:** `CompradorPendente` · **DbSet:** `CompradoresPendentes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoDeOrigemConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `SistemaId` | int |  |  |  |
| 4 | `Documento` | varchar(14) |  |  |  |
| 5 | `TipoDePessoa` | varchar(10) |  |  |  |
| 6 | `NomeNaOrigem` | nvarchar(100) |  |  |  |
| 7 | `Grupo` | varchar(30) |  |  |  |
| 8 | `Situacao` | varchar(30) |  |  |  |
| 9 | `Vendas` | int |  |  |  |
| 10 | `VendasComChassiValido` | int |  |  |  |
| 11 | `PrimeiraVendaEm` | date | sim |  |  |
| 12 | `UltimaVendaEm` | date | sim |  |  |
| 13 | `FiliaisDasVendas` | varchar(200) |  |  |  |
| 14 | `NotasNoProtheus` | int |  |  |  |
| 15 | `NaturezaNasNotas` | varchar(40) | sim |  |  |
| 16 | `PrimeiraNotaEm` | date | sim |  |  |
| 17 | `UltimaNotaEm` | date | sim |  |  |
| 18 | `NomeDaNotaCoincide` | bit |  |  |  |
| 19 | `SituacaoNoCadastroDoProtheus` | varchar(20) |  |  |  |
| 20 | `TemEnderecoNoProtheus` | bit |  |  |  |
| 21 | `TemMunicipioNoProtheus` | bit |  |  |  |
| 22 | `TemInscricaoEstadualNoProtheus` | bit |  |  |  |
| 23 | `NomeDoCadastroCoincide` | bit |  |  |  |
| 24 | `DadosQueFaltam` | nvarchar(400) |  |  |  |
| 25 | `ApuradoEm` | datetime2(3) |  |  |  |
| 26 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 27 | `CriadoEm` | datetime2(3) |  |  |  |
| 28 | `CriadoPorId` | bigint |  |  |  |
| 29 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 30 | `AlteradoPorId` | bigint | sim |  |  |
| 31 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 32 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `CompradorPendente.SistemaId` → `integracao.Sistema.Id` (obrigatória)
- `CompradorPendente.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Índices:**

- `PK_CompradorPendente` (clustered, único): Id
- `IX_CompradorPendente_EmpresaId_Grupo_Situacao` (nonclustered): EmpresaId,Grupo,Situacao
- `UX_CompradorPendente_ChavePublica` (nonclustered, único): ChavePublica
- `UX_CompradorPendente_Sistema_Documento` (nonclustered, único): SistemaId,Documento

**Restrições CHECK:**

- `CK_CompradorPendente_Contadores`: ([Vendas]>=(0) AND [VendasComChassiValido]>=(0) AND [VendasComChassiValido]<=[Vendas] AND [NotasNoProtheus]>=(0))
- `CK_CompradorPendente_Documento`: ([TipoDePessoa]='Fisica' AND len([Documento])=(11) OR [TipoDePessoa]='Juridica' AND len([Documento])=(14))
- `CK_CompradorPendente_Grupo`: ([Grupo]='SemNotaNoProtheus' OR [Grupo]='ComNotaNoProtheus')
- `CK_CompradorPendente_Situacao`: ([Situacao]='Cadastrado' OR [Situacao]='AguardandoCadastro')
- `CK_CompradorPendente_SituacaoNoCadastroDoProtheus`: ([SituacaoNoCadastroDoProtheus]='Bloqueado' OR [SituacaoNoCadastroDoProtheus]='Ativo' OR [SituacaoNoCadastroDoProtheus]='Ausente' OR [SituacaoNoCadastroDoProtheus]='NaoConferido')
- `CK_CompradorPendente_TipoDePessoa`: ([TipoDePessoa]='Juridica' OR [TipoDePessoa]='Fisica')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·5 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 2·1·1 | Integracao 0·0·0 | Testes 0·0·1 | Scripts 4·2·4
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDoArt.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/deploy/verificar-sincronizacao.ps1 (1)`

## `integracao.CorrespondenciaDaOrigem`

- **Entidade EF:** `CorrespondenciaDaOrigem` · **DbSet:** `CorrespondenciasDaOrigem` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoDeOrigemConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `SistemaId` | int |  |  |  |
| 3 | `Tipo` | varchar(20) |  |  |  |
| 4 | `CodigoNaOrigem` | varchar(80) |  |  |  |
| 5 | `TextoNaOrigem` | nvarchar(120) |  |  |  |
| 6 | `ContextoNaOrigem` | nvarchar(120) | sim |  |  |
| 7 | `LinhaDeProdutoId` | int | sim |  |  |
| 8 | `ModeloId` | int | sim |  |  |
| 9 | `EmpresaCorrespondenteId` | int | sim |  |  |
| 10 | `Situacao` | varchar(30) |  |  |  |
| 11 | `Criterio` | nvarchar(400) |  |  |  |
| 12 | `Ocorrencias` | int |  |  |  |
| 13 | `PrimeiraLeituraEm` | datetime2(3) |  |  |  |
| 14 | `UltimaLeituraEm` | datetime2(3) |  |  |  |
| 15 | `RevisadaEm` | datetime2(3) | sim |  |  |
| 16 | `RevisadaPorId` | bigint | sim |  |  |

**Referencia (FK de saída):**

- `CorrespondenciaDaOrigem.LinhaDeProdutoId` → `frota.LinhaDeProduto.Id` (opcional)
- `CorrespondenciaDaOrigem.ModeloId` → `frota.Modelo.Id` (opcional)
- `CorrespondenciaDaOrigem.SistemaId` → `integracao.Sistema.Id` (obrigatória)
- `CorrespondenciaDaOrigem.EmpresaCorrespondenteId` → `organizacao.Empresa.Id` (opcional)
- `CorrespondenciaDaOrigem.RevisadaPorId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_CorrespondenciaDaOrigem` (clustered, único): Id
- `IX_CorrespondenciaDaOrigem_EmpresaCorrespondenteId` (nonclustered): EmpresaCorrespondenteId
- `IX_CorrespondenciaDaOrigem_LinhaDeProdutoId` (nonclustered): LinhaDeProdutoId
- `IX_CorrespondenciaDaOrigem_ModeloId` (nonclustered): ModeloId
- `IX_CorrespondenciaDaOrigem_RevisadaPorId` (nonclustered): RevisadaPorId
- `UX_CorrespondenciaDaOrigem_Sistema_Tipo_Codigo` (nonclustered, único): SistemaId,Tipo,CodigoNaOrigem

**Restrições CHECK:**

- `CK_CorrespondenciaDaOrigem_Ocorrencias`: ([Ocorrencias]>=(0))
- `CK_CorrespondenciaDaOrigem_Revisao`: ([RevisadaEm] IS NULL AND [RevisadaPorId] IS NULL OR [RevisadaEm] IS NOT NULL AND [RevisadaPorId] IS NOT NULL)
- `CK_CorrespondenciaDaOrigem_Situacao`: ([Situacao]='RecusadaPorRevisao' OR [Situacao]='ConfirmadaPorRevisao' OR [Situacao]='NaoEClassificacaoDeProduto' OR [Situacao]='PendenteDeRevisao' OR [Situacao]='CorrespondenciaExata')
- `CK_CorrespondenciaDaOrigem_Tipo`: ([Tipo]='Unidade' OR [Tipo]='Produto' OR [Tipo]='LinhaDeProduto')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 2·1·8 | Integracao 0·0·0 | Testes 1·1·1 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDoArt.cs (2)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `integracao.DivergenciaDeIntegracao`

- **Entidade EF:** `DivergenciaDeIntegracao` · **DbSet:** `DivergenciasDeIntegracao` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoDeOrigemConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `SistemaId` | int |  |  |  |
| 4 | `Tipo` | varchar(50) |  |  |  |
| 5 | `ChaveOrigem` | varchar(60) |  |  |  |
| 6 | `EquipamentoId` | bigint | sim |  |  |
| 7 | `VendaDeMaquinaId` | bigint | sim |  |  |
| 8 | `Descricao` | nvarchar(400) |  |  |  |
| 9 | `ValorNoCrm` | nvarchar(200) | sim |  |  |
| 10 | `ValorNaOrigem` | nvarchar(200) | sim |  |  |
| 11 | `ValorNoProtheus` | nvarchar(200) | sim |  |  |
| 12 | `Situacao` | varchar(20) |  |  |  |
| 13 | `DetectadaEm` | datetime2(3) |  |  |  |
| 14 | `ConfirmadaEm` | datetime2(3) |  |  |  |
| 15 | `EncerradaEm` | datetime2(3) | sim |  |  |
| 16 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 17 | `CriadoEm` | datetime2(3) |  |  |  |
| 18 | `CriadoPorId` | bigint |  |  |  |
| 19 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 20 | `AlteradoPorId` | bigint | sim |  |  |
| 21 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 22 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `DivergenciaDeIntegracao.EquipamentoId` → `frota.Equipamento.Id` (opcional)
- `DivergenciaDeIntegracao.VendaDeMaquinaId` → `frota.VendaDeMaquina.Id` (opcional)
- `DivergenciaDeIntegracao.SistemaId` → `integracao.Sistema.Id` (obrigatória)
- `DivergenciaDeIntegracao.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Índices:**

- `PK_DivergenciaDeIntegracao` (clustered, único): Id
- `IX_DivergenciaDeIntegracao_EmpresaId_Situacao` (nonclustered): EmpresaId,Situacao
- `IX_DivergenciaDeIntegracao_EquipamentoId` (nonclustered): EquipamentoId
- `IX_DivergenciaDeIntegracao_VendaDeMaquinaId` (nonclustered): VendaDeMaquinaId
- `UX_DivergenciaDeIntegracao_ChavePublica` (nonclustered, único): ChavePublica
- `UX_DivergenciaDeIntegracao_Sistema_Tipo_Chave` (nonclustered, único): SistemaId,Tipo,ChaveOrigem

**Restrições CHECK:**

- `CK_DivergenciaDeIntegracao_Situacao`: ([Situacao]='DeixouDeOcorrer' OR [Situacao]='Resolvida' OR [Situacao]='Aberta')
- `CK_DivergenciaDeIntegracao_Tipo`: ([Tipo]='RegistroAusenteNaOrigem' OR [Tipo]='ChassiAlteradoNaOrigem' OR [Tipo]='CompradorAlteradoNaOrigem' OR [Tipo]='ProprietarioNoProtheusDiferenteDoComprador' OR [Tipo]='CompradorDiferenteDoProprietarioNoCrm')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 1·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 2·1·1 | Integracao 0·0·0 | Testes 1·1·2 | Scripts 4·2·4
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDoArt.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeEquipamentos.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IntegracaoDoArtTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/deploy/verificar-sincronizacao.ps1 (1)`

## `integracao.ExecucaoDeSincronizacao`

- **Entidade EF:** `ExecucaoDeSincronizacao` · **DbSet:** `ExecucoesDeSincronizacao` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoDeOrigemConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `SistemaId` | int |  |  |  |
| 3 | `Fluxo` | varchar(60) |  |  |  |
| 4 | `Maquina` | varchar(60) |  |  |  |
| 5 | `IniciadaEm` | datetime2(3) |  |  |  |
| 6 | `TerminadaEm` | datetime2(3) | sim |  |  |
| 7 | `Resultado` | varchar(20) |  |  |  |
| 8 | `Tentativas` | int |  |  |  |
| 9 | `RegistrosLidos` | int |  |  |  |
| 10 | `Incluidos` | int |  |  |  |
| 11 | `Atualizados` | int |  |  |  |
| 12 | `Pendentes` | int |  |  |  |
| 13 | `Mensagem` | nvarchar(1000) | sim |  |  |

**Referencia (FK de saída):**

- `ExecucaoDeSincronizacao.SistemaId` → `integracao.Sistema.Id` (obrigatória)

**Índices:**

- `PK_ExecucaoDeSincronizacao` (clustered, único): Id
- `IX_ExecucaoDeSincronizacao_SistemaId_Fluxo_IniciadaEm` (nonclustered): SistemaId,Fluxo,IniciadaEm DESC

**Restrições CHECK:**

- `CK_ExecucaoDeSincronizacao_Contadores`: ([Tentativas]>=(0) AND [RegistrosLidos]>=(0) AND [Incluidos]>=(0) AND [Atualizados]>=(0) AND [Pendentes]>=(0))
- `CK_ExecucaoDeSincronizacao_Resultado`: ([Resultado]='Ignorada' OR [Resultado]='Falha' OR [Resultado]='Sucesso' OR [Resultado]='EmAndamento')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 1·1·1 | Repositorio 2·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 5·3·5 | Integracao 0·0·0 | Testes 2·2·6 | Scripts 7·3·7
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Integracoes/ListarSincronizacoes.cs (1)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Carga/Sincronizacao/ExecutorDaSincronizacaoDoArt.cs (3)`, `src/Tracbel.Crm.Carga/Sincronizacao/ServicoDeSincronizacaoDoArt.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeSincronizacoes.cs (2)`, `tests/Tracbel.Crm.Api.Testes/IntegracaoDoArtTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/deploy/diagnosticar-servidor.ps1 (2)`, `scripts/deploy/verificar-sincronizacao.ps1 (2)`

## `integracao.MensagemDeSaida`

- **Entidade EF:** `MensagemDeSaida` · **DbSet:** `MensagensDeSaida` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `SistemaId` | int |  |  |  |
| 3 | `Tipo` | varchar(60) |  |  |  |
| 4 | `Conteudo` | nvarchar(max) |  |  |  |
| 5 | `CorrelacaoId` | uniqueidentifier |  |  |  |
| 6 | `Situacao` | varchar(30) |  |  |  |
| 7 | `Tentativas` | smallint |  |  |  |
| 8 | `ProximaTentativaEm` | datetime2(3) | sim |  |  |
| 9 | `UltimoErro` | nvarchar(2000) | sim |  |  |
| 10 | `CriadoEm` | datetime2(3) |  |  |  |
| 11 | `EntregueEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `MensagemDeSaida.SistemaId` → `integracao.Sistema.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `integracao.MensagemDescartada.MensagemDeSaidaId` (opcional)

**Índices:**

- `PK_MensagemDeSaida` (clustered, único): Id
- `IX_MensagemDeSaida_CorrelacaoId` (nonclustered): CorrelacaoId
- `IX_MensagemDeSaida_ProximaTentativaEm` (nonclustered): ProximaTentativaEm · filtro ([Situacao]='Pendente')
- `IX_MensagemDeSaida_SistemaId` (nonclustered): SistemaId

**Restrições CHECK:**

- `CK_MensagemDeSaida_ConteudoJson`: (isjson([Conteudo])=(1))
- `CK_MensagemDeSaida_Entrega`: ([Situacao]<>'Entregue' OR [EntregueEm] IS NOT NULL)
- `CK_MensagemDeSaida_Situacao`: ([Situacao]='DescartadaAposLimite' OR [Situacao]='Falhou' OR [Situacao]='Entregue' OR [Situacao]='Pendente')
- `CK_MensagemDeSaida_Tentativas`: ([Tentativas]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·1 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `integracao.MensagemDescartada`

- **Entidade EF:** `MensagemDescartada` · **DbSet:** `MensagensDescartadas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `MensagemDeSaidaId` | bigint | sim |  |  |
| 3 | `Fluxo` | varchar(60) |  |  |  |
| 4 | `Conteudo` | nvarchar(max) |  |  |  |
| 5 | `Erro` | nvarchar(4000) |  |  |  |
| 6 | `Tentativas` | smallint |  |  |  |
| 7 | `DescartadaEm` | datetime2(3) |  |  |  |
| 8 | `TratadaEm` | datetime2(3) | sim |  |  |
| 9 | `TratadaPorId` | bigint | sim |  |  |
| 10 | `Tratativa` | nvarchar(1000) | sim |  |  |

**Referencia (FK de saída):**

- `MensagemDescartada.MensagemDeSaidaId` → `integracao.MensagemDeSaida.Id` (opcional)
- `MensagemDescartada.TratadaPorId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_MensagemDescartada` (clustered, único): Id
- `IX_MensagemDescartada_Fluxo_DescartadaEm` (nonclustered): Fluxo,DescartadaEm · filtro ([TratadaEm] IS NULL)
- `IX_MensagemDescartada_MensagemDeSaidaId` (nonclustered): MensagemDeSaidaId
- `IX_MensagemDescartada_TratadaPorId` (nonclustered): TratadaPorId

**Restrições CHECK:**

- `CK_MensagemDescartada_ConteudoJson`: (isjson([Conteudo])=(1))
- `CK_MensagemDescartada_Tentativas`: ([Tentativas]>=(0))
- `CK_MensagemDescartada_Tratativa`: ([TratadaEm] IS NULL OR [TratadaPorId] IS NOT NULL AND [Tratativa] IS NOT NULL)

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 9·4·8 | Integracao 1·1·1 | Testes 2·1·1 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (2)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Integracao/Carga/RegistrosDaCarga.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Carga/ConsolidacaoDeGrafiasCortadasTestes.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `integracao.PontoDeSincronismo`

- **Entidade EF:** `PontoDeSincronismo` · **DbSet:** `PontosDeSincronismo` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `SistemaId` | int |  |  |  |
| 3 | `Fluxo` | varchar(60) |  |  |  |
| 4 | `UltimoValor` | varchar(100) |  |  |  |
| 5 | `ProcessadoEm` | datetime2(3) |  |  |  |
| 6 | `RegistrosLidos` | int |  |  |  |
| 7 | `RegistrosGravados` | int |  |  |  |
| 8 | `RegistrosErro` | int |  |  |  |
| 9 | `MinutosParaAlarme` | int |  |  |  |

**Referencia (FK de saída):**

- `PontoDeSincronismo.SistemaId` → `integracao.Sistema.Id` (obrigatória)

**Índices:**

- `PK_PontoDeSincronismo` (clustered, único): Id
- `IX_PontoDeSincronismo_SistemaId` (nonclustered): SistemaId
- `UX_PontoDeSincronismo_Fluxo` (nonclustered, único): Fluxo

**Restrições CHECK:**

- `CK_PontoDeSincronismo_Alarme`: ([MinutosParaAlarme]>(0))
- `CK_PontoDeSincronismo_Contadores`: ([RegistrosLidos]>=(0) AND [RegistrosGravados]>=(0) AND [RegistrosErro]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 11·6·7 | Integracao 1·1·1 | Testes 1·1·1 | Scripts 5·3·5
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (2)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Carga/Sincronizacao/ExecutorDaSincronizacaoDoArt.cs (2)`, `src/Tracbel.Crm.Carga/Sincronizacao/ServicoDeSincronizacaoDoArt.cs (1)`, `src/Tracbel.Crm.Integracao/Ibge/LeitorDoIbge.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/deploy/diagnosticar-servidor.ps1 (1)`, `scripts/deploy/verificar-sincronizacao.ps1 (1)`

## `integracao.Recepcao`

- **Entidade EF:** `Recepcao` · **DbSet:** `Recepcoes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id,RecebidaEm` · **Identity:** Id · **Particionada:** `PS_Mensal_Recepcao`
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `RecebidaEm` | datetime2(3) |  |  |  |
| 3 | `SistemaId` | int |  |  |  |
| 4 | `Entidade` | varchar(40) |  |  |  |
| 5 | `ChaveOrigem` | varchar(200) |  |  |  |
| 6 | `Conteudo` | nvarchar(max) |  |  |  |
| 7 | `Situacao` | varchar(20) |  |  |  |
| 8 | `Erro` | nvarchar(4000) | sim |  |  |
| 9 | `ProcessadaEm` | datetime2(3) | sim |  |  |
| 10 | `ExpurgarApos` | datetime2(3) |  |  |  |

**Referencia (FK de saída):**

- `Recepcao.SistemaId` → `integracao.Sistema.Id` (obrigatória)

**Índices:**

- `PK_Recepcao` (clustered, único): Id,RecebidaEm
- `IX_Recepcao_Entidade_RecebidaEm` (nonclustered): Entidade,RecebidaEm · filtro ([Situacao]='Recebida')
- `IX_Recepcao_ExpurgarApos` (nonclustered): RecebidaEm,ExpurgarApos
- `IX_Recepcao_SistemaId_ChaveOrigem` (nonclustered): RecebidaEm,SistemaId,ChaveOrigem

**Restrições CHECK:**

- `CK_Recepcao_ConteudoJson`: (isjson([Conteudo])=(1))
- `CK_Recepcao_Desfecho`: (([Situacao]<>'Falhou' OR [Erro] IS NOT NULL) AND ([Situacao]<>'Processada' OR [ProcessadaEm] IS NOT NULL))
- `CK_Recepcao_Entidade`: (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Latin1_General_BIN2='Usuari …
- `CK_Recepcao_Expurgo`: ([ExpurgarApos]>[RecebidaEm])
- `CK_Recepcao_Situacao`: ([Situacao]='Ignorada' OR [Situacao]='Falhou' OR [Situacao]='Processada' OR [Situacao]='Recebida')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 1·1·3 | Scripts 3·1·3
- Arquivos com acesso: `tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `integracao.RegistroDeOrigem`

- **Entidade EF:** `RegistroDeOrigem` · **DbSet:** `RegistrosDeOrigem` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoDeOrigemConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `SistemaId` | int |  |  |  |
| 3 | `Fluxo` | varchar(60) |  |  |  |
| 4 | `ChaveOrigem` | varchar(60) |  |  |  |
| 5 | `HashDoConteudo` | varchar(64) |  |  |  |
| 6 | `ChassiNaOrigem` | nvarchar(200) | sim |  |  |
| 7 | `LinhaNaOrigem` | nvarchar(60) | sim |  |  |
| 8 | `ProdutoNaOrigem` | nvarchar(60) | sim |  |  |
| 9 | `UnidadeNaOrigem` | nvarchar(80) | sim |  |  |
| 10 | `VendidaEm` | date | sim |  |  |
| 11 | `Transformacoes` | nvarchar(1000) | sim |  |  |
| 12 | `Decisao` | varchar(20) |  |  |  |
| 13 | `Motivos` | varchar(400) | sim |  |  |
| 14 | `VendaDeMaquinaId` | bigint | sim |  |  |
| 15 | `PrimeiraLeituraEm` | datetime2(3) |  |  |  |
| 16 | `UltimaLeituraEm` | datetime2(3) |  |  |  |
| 17 | `ConteudoAlteradoEm` | datetime2(3) | sim |  |  |
| 18 | `AusenteNaOrigemDesde` | datetime2(3) | sim |  |  |
| 19 | `Leituras` | int |  |  |  |

**Referencia (FK de saída):**

- `RegistroDeOrigem.VendaDeMaquinaId` → `frota.VendaDeMaquina.Id` (opcional)
- `RegistroDeOrigem.SistemaId` → `integracao.Sistema.Id` (obrigatória)

**Índices:**

- `PK_RegistroDeOrigem` (clustered, único): Id
- `IX_RegistroDeOrigem_Fluxo_Decisao` (nonclustered): Fluxo,Decisao
- `IX_RegistroDeOrigem_VendaDeMaquinaId` (nonclustered): VendaDeMaquinaId
- `UX_RegistroDeOrigem_Sistema_Fluxo_Chave` (nonclustered, único): SistemaId,Fluxo,ChaveOrigem

**Restrições CHECK:**

- `CK_RegistroDeOrigem_Decisao`: ([Decisao]='Pendente' OR [Decisao]='Importado')
- `CK_RegistroDeOrigem_Leituras`: ([Leituras]>=(1))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·1·3 | Integracao 0·0·0 | Testes 0·0·1 | Scripts 4·2·4
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDoArt.cs (3)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/deploy/verificar-sincronizacao.ps1 (1)`

## `integracao.Sistema`

- **Entidade EF:** `Sistema` · **DbSet:** `Sistemas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/IntegracaoConfiguracao.cs`
- **Linhas:** 4 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(20) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `MeioDeAcesso` | varchar(40) |  |  |  |
| 5 | `ResponsavelTecnico` | nvarchar(120) | sim |  |  |
| 6 | `EstaAtivo` | bit |  |  |  |

**Dependem dela (FK de entrada):**

- `frota.VendaDeMaquina.SistemaId`
- `frota.VinculoDeClienteComEquipamento.SistemaId` (opcional)
- `integracao.ChaveExterna.SistemaId`
- `integracao.CompradorPendente.SistemaId`
- `integracao.CorrespondenciaDaOrigem.SistemaId`
- `integracao.DivergenciaDeIntegracao.SistemaId`
- `integracao.ExecucaoDeSincronizacao.SistemaId`
- `integracao.MensagemDeSaida.SistemaId`
- `integracao.PontoDeSincronismo.SistemaId`
- `integracao.Recepcao.SistemaId`
- `integracao.RegistroDeOrigem.SistemaId`
- `relatorio.Fonte.SistemaId` (opcional)

**Índices:**

- `PK_Sistema` (clustered, único): Id
- `UX_Sistema_Codigo` (nonclustered, único): Codigo

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·11 | Aplicacao 0·0·1 | Repositorio 4·3·0 | InfraOutros 0·0·0 | Api 0·0·1 | Carga 6·3·4 | Integracao 2·2·4 | Testes 2·1·13 | Scripts 3·3·10
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (2)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeEquipamentos.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeHistoricoComercial.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeSincronizacoes.cs (1)`, `src/Tracbel.Crm.Integracao/Art/LeitorDoArt.cs (1)`, `src/Tracbel.Crm.Integracao/Carga/LeitorDeCargaDoVortice.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IntegracaoDoArtTestes.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/deploy/diagnosticar-servidor.ps1 (1)`, `scripts/deploy/verificar-sincronizacao.ps1 (1)`

## `metadado.__EFMigrationsHistory`

- **Entidade EF:** — (sem entidade) · **DbSet:** — (sem DbSet) · **Configuração:** —
- **Linhas:** 12 · **Chave primária:** `MigrationId` · **Identity:** não
- **Migração que criou:** —

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `MigrationId` | nvarchar(150) |  |  |  |
| 2 | `ProductVersion` | nvarchar(32) |  |  |  |

**Índices:**

- `PK___EFMigrationsHistory` (clustered, único): MigrationId

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·0 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 2·2·0
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/deploy/instalar-no-servidor.ps1 (1)`

## `metadado.CampoPersonalizado`

- **Entidade EF:** `CampoPersonalizado` · **DbSet:** `CamposPersonalizados` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint, CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Entidade` | varchar(40) |  |  |  |
| 3 | `Campo` | varchar(60) |  |  |  |
| 4 | `Rotulo` | nvarchar(80) |  |  |  |
| 5 | `TipoDeCampo` | varchar(20) |  |  |  |
| 6 | `EhPersonalizado` | bit |  |  |  |
| 7 | `EhObrigatorio` | bit |  |  |  |
| 8 | `TamanhoMaximo` | int | sim |  |  |
| 9 | `CatalogoId` | int | sim |  |  |
| 10 | `Grupo` | nvarchar(60) | sim |  |  |
| 11 | `Ordem` | smallint |  |  |  |
| 12 | `CondicaoVisibilidade` | nvarchar(1000) | sim |  |  |
| 13 | `Validacao` | nvarchar(max) | sim |  |  |
| 14 | `NoResumo` | bit |  |  |  |
| 15 | `EstaAtivo` | bit |  |  |  |

**Referencia (FK de saída):**

- `CampoPersonalizado.CatalogoId` → `metadado.Catalogo.Id` (opcional)

**Índices:**

- `PK_CampoPersonalizado` (clustered, único): Id
- `IX_CampoPersonalizado_CatalogoId` (nonclustered): CatalogoId
- `UX_CampoPersonalizado_Entidade_Campo` (nonclustered, único): Entidade,Campo

**Restrições CHECK:**

- `CK_CampoPersonalizado_Campo`: (([Campo]) collate Latin1_General_BIN2 like '[A-Z]%' AND NOT ([Campo]) collate Latin1_General_BIN2 like '%[^A-Za-z0-9]%')
- `CK_CampoPersonalizado_Entidade`: (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Latin1_General_BIN2='Usuari …
- `CK_CampoPersonalizado_Lista`: ([TipoDeCampo]<>'Lista' OR [CatalogoId] IS NOT NULL)
- `CK_CampoPersonalizado_Tipo`: ([TipoDeCampo]='Referencia' OR [TipoDeCampo]='Lista' OR [TipoDeCampo]='Booleano' OR [TipoDeCampo]='Data' OR [TipoDeCampo]='Numero' OR [TipoDeCampo]='Texto')
- `CK_CampoPersonalizado_ValidacaoJson`: ([Validacao] IS NULL OR isjson([Validacao])=(1))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 2·1·3 | Scripts 1·1·1
- Arquivos com acesso: `tests/Tracbel.Crm.Arquitetura.Testes/Banco/DominioDeEntidadeTestes.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `metadado.Catalogo`

- **Entidade EF:** `Catalogo` · **DbSet:** `Catalogos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs`
- **Linhas:** 10 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (InsertData)`; `20260906122942_VendaPerdida (InsertData)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(40) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Descricao` | nvarchar(400) | sim |  |  |
| 5 | `PermiteItemNovo` | bit |  |  |  |
| 6 | `EstaAtivo` | bit |  |  |  |

**Dependem dela (FK de entrada):**

- `metadado.CampoPersonalizado.CatalogoId` (opcional)
- `metadado.CatalogoItem.CatalogoId`
- `metadado.Pergunta.CatalogoId` (opcional)

**Índices:**

- `PK_Catalogo` (clustered, único): Id
- `UX_Catalogo_Codigo` (nonclustered, único): Codigo

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·5 | Aplicacao 4·2·3 | Repositorio 2·1·1 | InfraOutros 0·0·1 | Api 2·2·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 5·4·16 | Scripts 2·2·6
- Arquivos com acesso: `src/Tracbel.Crm.Api/Program.cs (1)`, `src/Tracbel.Crm.Api/Endpoints/EndpointsDeCatalogo.cs (1)`, `src/Tracbel.Crm.Aplicacao/Catalogos/ListarCatalogos.cs (3)`, `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCatalogos.cs (2)`, `tests/Tracbel.Crm.Aplicacao.Testes/Cadastro/BancoDeTeste.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Cadastro/CadastroDeEquipamentoTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/CatalogoDeSistemaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/ColacaoNoContainerTestes.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/banco/seed/01-dados-de-referencia.sql (1)`

## `metadado.CatalogoItem`

- **Entidade EF:** `CatalogoItem` · **DbSet:** `CatalogoItens` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs`
- **Linhas:** 227 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddForeignKey, CreateIndex, DropForeignKey, DropIndex)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `CatalogoId` | int |  |  |  |
| 3 | `Codigo` | varchar(40) |  |  |  |
| 4 | `Descricao` | nvarchar(200) |  |  |  |
| 5 | `ItemPaiId` | int | sim |  |  |
| 6 | `Ordem` | smallint |  |  |  |
| 7 | `ExigeObservacao` | bit |  |  |  |
| 8 | `EstaAtivo` | bit |  |  |  |
| 9 | `UltimoUsoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `CatalogoItem.CatalogoId` → `metadado.Catalogo.Id` (obrigatória)
- `CatalogoItem.CatalogoId,ItemPaiId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)

**Dependem dela (FK de entrada):**

- `comercial.Cliente.CatalogoDaOrigemId,OrigemId` (opcional)
- `comercial.Cliente.CatalogoDoMotivoInativacaoId,MotivoInativacaoId` (opcional)
- `comercial.ClienteContato.CatalogoDoPapelId,PapelId`
- `comercial.Endereco.CatalogoDaCulturaId,CulturaId` (opcional)
- `comercial.Lead.CatalogoDaOrigemId,OrigemId`
- `comercial.Lead.CatalogoDoMotivoDescarteId,MotivoDescarteId` (opcional)
- `documento.Documento.CatalogoDoTipoDocumentoId,TipoDocumentoId`
- `metadado.CatalogoItem.CatalogoId,ItemPaiId` (opcional)
- `metadado.Resposta.CatalogoItemId` (opcional)
- `processo.ItemDeProposta.CatalogoDaCondicaoPagamentoId,CondicaoPagamentoId` (opcional)
- `processo.Processo.CatalogoDoConcorrenteId,ConcorrenteId` (opcional)
- `processo.VendaPerdida.CatalogoDaRevendaId,RevendaDoConcorrenteId` (opcional)
- `processo.VendaPerdida.CatalogoDoConcorrenteId,ConcorrenteId` (opcional)
- `processo.VendaPerdida.CatalogoDoTipoDeEquipamentoId,TipoDeEquipamentoId` (opcional)

**Índices:**

- `PK_CatalogoItem` (clustered, único): Id
- `AK_CatalogoItem_CatalogoId` (nonclustered, único): CatalogoId,Id
- `IX_CatalogoItem_CatalogoId_ItemPaiId` (nonclustered): CatalogoId,ItemPaiId
- `IX_CatalogoItem_CatalogoId_Ordem` (nonclustered): CatalogoId,Ordem · filtro ([EstaAtivo]=(1))
- `UX_CatalogoItem_Catalogo_Codigo` (nonclustered, único): CatalogoId,Codigo
- `UX_CatalogoItem_Catalogo_Descricao` (nonclustered, único): CatalogoId,Descricao

**Restrições CHECK:**

- `CK_CatalogoItem_Ordem`: ([Ordem]>=(0))
- `CK_CatalogoItem_Pai`: ([ItemPaiId] IS NULL OR [ItemPaiId]<>[Id])

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·4 | Aplicacao 0·0·0 | Repositorio 5·3·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 5·2·2 | Integracao 0·0·0 | Testes 8·6·17 | Scripts 17·2·17
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.VendaPerdida.cs (3)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (2)`, `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCatalogos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeClientes.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeVendasPerdidas.cs (1)`, `tests/Tracbel.Crm.Api.Testes/ApiEmMemoria.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Cadastro/BancoDeTeste.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Persistencia/FiltroSegurancaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/CatalogoDeSistemaTestes.cs (3)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs (1)` e mais 2

## `metadado.Formulario`

- **Entidade EF:** `Formulario` · **DbSet:** `Formularios` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(40) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Descricao` | nvarchar(400) | sim |  |  |
| 5 | `TipoProcessoId` | int | sim |  |  |
| 6 | `VersaoPublicada` | int |  |  |  |
| 7 | `EstaAtivo` | bit |  |  |  |

**Referencia (FK de saída):**

- `Formulario.TipoProcessoId` → `processo.TipoProcesso.Id` (opcional)

**Dependem dela (FK de entrada):**

- `metadado.Pergunta.FormularioId`
- `metadado.Preenchimento.FormularioId`
- `processo.TipoTarefa.FormularioId` (opcional)

**Índices:**

- `PK_Formulario` (clustered, único): Id
- `IX_Formulario_TipoProcessoId` (nonclustered): TipoProcessoId
- `UX_Formulario_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_Formulario_Versao`: ([VersaoPublicada]>=(1))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 1·1·2
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `metadado.Pergunta`

- **Entidade EF:** `Pergunta` · **DbSet:** `Perguntas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `FormularioId` | int |  |  |  |
| 3 | `Ordem` | smallint |  |  |  |
| 4 | `Codigo` | varchar(40) |  |  |  |
| 5 | `Texto` | nvarchar(1000) |  |  |  |
| 6 | `TipoDeResposta` | varchar(20) |  |  |  |
| 7 | `CatalogoId` | int | sim |  |  |
| 8 | `EhObrigatoria` | bit |  |  |  |
| 9 | `PerguntaCondicaoId` | int | sim |  |  |
| 10 | `ValorCondicao` | nvarchar(200) | sim |  |  |
| 11 | `EstaAtiva` | bit |  |  |  |

**Referencia (FK de saída):**

- `Pergunta.CatalogoId` → `metadado.Catalogo.Id` (opcional)
- `Pergunta.FormularioId` → `metadado.Formulario.Id` (obrigatória)
- `Pergunta.PerguntaCondicaoId` → `metadado.Pergunta.Id` (opcional)

**Dependem dela (FK de entrada):**

- `metadado.Pergunta.PerguntaCondicaoId` (opcional)
- `metadado.Resposta.PerguntaId`

**Índices:**

- `PK_Pergunta` (clustered, único): Id
- `IX_Pergunta_CatalogoId` (nonclustered): CatalogoId
- `IX_Pergunta_FormularioId_Ordem` (nonclustered): FormularioId,Ordem
- `IX_Pergunta_PerguntaCondicaoId` (nonclustered): PerguntaCondicaoId
- `UX_Pergunta_Formulario_Codigo` (nonclustered, único): FormularioId,Codigo

**Restrições CHECK:**

- `CK_Pergunta_Catalogo`: (NOT ([TipoDeResposta]='CatalogoMultiplo' OR [TipoDeResposta]='Catalogo') OR [CatalogoId] IS NOT NULL)
- `CK_Pergunta_Condicao`: ([PerguntaCondicaoId] IS NULL OR [PerguntaCondicaoId]<>[Id])
- `CK_Pergunta_TipoDeResposta`: ([TipoDeResposta]='CatalogoMultiplo' OR [TipoDeResposta]='Equipamento' OR [TipoDeResposta]='Cliente' OR [TipoDeResposta]='Catalogo' OR [TipoDeResposta]='Booleano' OR [TipoDeResposta]='Data' OR [TipoDeResposta]='Numero' OR [TipoDeResposta]='Texto')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·5 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·1 | Scripts 1·1·1
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `metadado.Preenchimento`

- **Entidade EF:** `Preenchimento` · **DbSet:** `Preenchimentos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `FormularioId` | int |  |  |  |
| 4 | `VersaoFormulario` | int |  |  |  |
| 5 | `ClienteId` | bigint | sim |  |  |
| 6 | `ProcessoId` | bigint | sim |  |  |
| 7 | `TarefaId` | bigint | sim |  |  |
| 8 | `InteracaoId` | bigint | sim |  |  |
| 9 | `PreenchidoPorId` | bigint |  |  |  |
| 10 | `PreenchidoEm` | datetime2(3) |  |  |  |
| 11 | `Situacao` | varchar(20) |  |  |  |

**Referencia (FK de saída):**

- `Preenchimento.ClienteId` → `comercial.Cliente.Id` (opcional)
- `Preenchimento.FormularioId` → `metadado.Formulario.Id` (obrigatória)
- `Preenchimento.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Preenchimento.InteracaoId` → `processo.Interacao.Id` (opcional)
- `Preenchimento.ProcessoId` → `processo.Processo.Id` (opcional)
- `Preenchimento.TarefaId` → `processo.Tarefa.Id` (opcional)
- `Preenchimento.PreenchidoPorId` → `seguranca.Usuario.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `metadado.Resposta.PreenchimentoId`

**Índices:**

- `PK_Preenchimento` (clustered, único): Id
- `IX_Preenchimento_ClienteId` (nonclustered): ClienteId
- `IX_Preenchimento_EmpresaId` (nonclustered): EmpresaId
- `IX_Preenchimento_FormularioId_PreenchidoEm` (nonclustered): FormularioId,PreenchidoEm DESC
- `IX_Preenchimento_InteracaoId` (nonclustered): InteracaoId
- `IX_Preenchimento_PreenchidoPorId` (nonclustered): PreenchidoPorId
- `IX_Preenchimento_ProcessoId` (nonclustered): ProcessoId
- `IX_Preenchimento_TarefaId` (nonclustered): TarefaId

**Restrições CHECK:**

- `CK_Preenchimento_Situacao`: ([Situacao]='Cancelado' OR [Situacao]='Concluido' OR [Situacao]='EmAndamento')
- `CK_Preenchimento_TemVinculo`: ([ClienteId] IS NOT NULL OR [ProcessoId] IS NOT NULL OR [TarefaId] IS NOT NULL OR [InteracaoId] IS NOT NULL)
- `CK_Preenchimento_Versao`: ([VersaoFormulario]>=(1))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `metadado.Resposta`

- **Entidade EF:** `Resposta` · **DbSet:** `Respostas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `PreenchimentoId` | bigint |  |  |  |
| 3 | `PerguntaId` | int |  |  |  |
| 4 | `ValorTexto` | nvarchar(4000) | sim |  |  |
| 5 | `ValorNumero` | decimal(18,6) | sim |  |  |
| 6 | `ValorData` | datetime2(3) | sim |  |  |
| 7 | `ValorBooleano` | bit | sim |  |  |
| 8 | `CatalogoItemId` | int | sim |  |  |
| 9 | `ClienteId` | bigint | sim |  |  |
| 10 | `EquipamentoId` | bigint | sim |  |  |
| 11 | `ValorEstruturado` | nvarchar(max) | sim |  |  |
| 12 | `CriadoEm` | datetime2(3) |  |  |  |

**Referencia (FK de saída):**

- `Resposta.ClienteId` → `comercial.Cliente.Id` (opcional)
- `Resposta.EquipamentoId` → `frota.Equipamento.Id` (opcional)
- `Resposta.CatalogoItemId` → `metadado.CatalogoItem.Id` (opcional)
- `Resposta.PerguntaId` → `metadado.Pergunta.Id` (obrigatória)
- `Resposta.PreenchimentoId` → `metadado.Preenchimento.Id` (obrigatória)

**Índices:**

- `PK_Resposta` (clustered, único): Id
- `IX_Resposta_CatalogoItemId` (nonclustered): CatalogoItemId
- `IX_Resposta_ClienteId` (nonclustered): ClienteId
- `IX_Resposta_EquipamentoId` (nonclustered): EquipamentoId
- `IX_Resposta_PerguntaId_CatalogoItemId` (nonclustered): PerguntaId,CatalogoItemId · filtro ([CatalogoItemId] IS NOT NULL)
- `IX_Resposta_PerguntaId_ValorNumero` (nonclustered): PerguntaId,ValorNumero · filtro ([ValorNumero] IS NOT NULL)
- `IX_Resposta_PerguntaId_ValorTexto` (nonclustered): PerguntaId,ValorTexto · filtro ([ValorTexto] IS NOT NULL)
- `IX_Resposta_PreenchimentoId` (nonclustered): PreenchimentoId
- `UX_Resposta_Preenchimento_Pergunta` (nonclustered, único): PreenchimentoId,PerguntaId

**Restrições CHECK:**

- `CK_Resposta_UmValor`: ((((((((case when [ValorTexto] IS NOT NULL then (1) else (0) end+case when [ValorNumero] IS NOT NULL then (1) else (0) end)+case when [ValorData] IS NOT NULL then (1) else (0) end)+case when [ValorBooleano] IS NOT NULL then (1) else (0) end)+case when [CatalogoItemId] IS NOT NULL then (1) else (0) e …
- `CK_Resposta_ValorEstruturadoJson`: ([ValorEstruturado] IS NULL OR isjson([ValorEstruturado])=(1))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 3·2·6 | Scripts 3·1·3
- Arquivos com acesso: `tests/Tracbel.Crm.Arquitetura.Testes/Banco/CatalogoDeSistemaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/TiposDeColunaTestes.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `metadado.TratadorDeEvento`

- **Entidade EF:** `TratadorDeEvento` · **DbSet:** `TratadoresDeEvento` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/MetadadoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Evento` | varchar(60) |  |  |  |
| 3 | `TipoImplementacao` | varchar(200) |  |  |  |
| 4 | `Momento` | varchar(20) |  |  |  |
| 5 | `Ordem` | smallint |  |  |  |
| 6 | `EhAssincrono` | bit |  |  |  |
| 7 | `CamposFiltro` | varchar(400) | sim |  |  |
| 8 | `EstaAtivo` | bit |  |  |  |

**Índices:**

- `PK_TratadorDeEvento` (clustered, único): Id
- `IX_TratadorDeEvento_Evento_Momento_Ordem` (nonclustered): Evento,Momento,Ordem · filtro ([EstaAtivo]=(1))
- `UX_TratadorDeEvento_Evento_Tipo_Momento` (nonclustered, único): Evento,TipoImplementacao,Momento

**Restrições CHECK:**

- `CK_TratadorDeEvento_Assincrono`: ([EhAssincrono]=(0) OR [Momento]='PosOperacao')
- `CK_TratadorDeEvento_Momento`: ([Momento]='PosOperacao' OR [Momento]='PreOperacao' OR [Momento]='PreValidacao')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 1·1·1
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `organizacao.AreaPlantadaNoMunicipio`

- **Entidade EF:** `AreaPlantadaNoMunicipio` · **DbSet:** `AreasPlantadasNosMunicipios` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `MunicipioId` | int |  |  |  |
| 3 | `Ano` | smallint |  |  |  |
| 4 | `ProdutoCodigoIbge` | int |  |  |  |
| 5 | `ProdutoNome` | nvarchar(120) |  |  |  |
| 6 | `AreaPlantadaHectares` | decimal(14,2) | sim |  |  |
| 7 | `ImportadoEm` | datetime2(3) |  |  |  |
| 8 | `ImportadoPorId` | bigint |  |  |  |

**Referencia (FK de saída):**

- `AreaPlantadaNoMunicipio.MunicipioId` → `organizacao.Municipio.Id` (obrigatória)
- `AreaPlantadaNoMunicipio.ImportadoPorId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_AreaPlantadaNoMunicipio` (clustered, único): Id
- `IX_AreaPlantadaNoMunicipio_ImportadoPorId` (nonclustered): ImportadoPorId
- `IX_AreaPlantadaNoMunicipio_ProdutoCodigoIbge_Ano` (nonclustered): ProdutoCodigoIbge,Ano
- `UX_AreaPlantadaNoMunicipio_Municipio_Ano_Produto` (nonclustered, único): MunicipioId,Ano,ProdutoCodigoIbge

**Restrições CHECK:**

- `CK_AreaPlantadaNoMunicipio_Ano`: ([Ano]>=(1974) AND [Ano]<=(2100))
- `CK_AreaPlantadaNoMunicipio_Area`: ([AreaPlantadaHectares] IS NULL OR [AreaPlantadaHectares]>=(0))
- `CK_AreaPlantadaNoMunicipio_Produto`: ([ProdutoCodigoIbge]>(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 1·1·1 | Repositorio 2·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 2·1·2 | Integracao 0·0·0 | Testes 1·1·8 | Scripts 6·3·6
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (2)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (2)`, `scripts/deploy/carregar-territorio-no-servidor.ps1 (1)`

## `organizacao.Carteira`

- **Entidade EF:** `Carteira` · **DbSet:** `Carteiras` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260906134331_NaturezaDeCarteiraEUsuario (AddCheckConstraint, CreateIndex)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `LinhaDeNegocioId` | int |  |  |  |
| 4 | `PracaId` | int | sim |  |  |
| 5 | `Codigo` | varchar(40) |  |  |  |
| 6 | `Nome` | nvarchar(120) |  |  |  |
| 7 | `ResponsavelId` | bigint |  |  |  |
| 8 | `SupervisorId` | bigint | sim |  |  |
| 9 | `EquipeId` | bigint | sim |  |  |
| 10 | `EstaAtiva` | bit |  |  |  |
| 11 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 12 | `CriadoEm` | datetime2(3) |  |  |  |
| 13 | `CriadoPorId` | bigint |  |  |  |
| 14 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 15 | `AlteradoPorId` | bigint | sim |  |  |
| 16 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 17 | `Versao` | timestamp | sim |  |  |
| 18 | `Natureza` | varchar(20) |  |  | ('Comercial') |

**Referencia (FK de saída):**

- `Carteira.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Carteira.LinhaDeNegocioId` → `organizacao.LinhaDeNegocio.Id` (obrigatória)
- `Carteira.PracaId` → `organizacao.Praca.Id` (opcional)
- `Carteira.EquipeId` → `seguranca.Equipe.Id` (opcional)
- `Carteira.ResponsavelId` → `seguranca.Usuario.Id` (obrigatória)
- `Carteira.SupervisorId` → `seguranca.Usuario.Id` (opcional)

**Dependem dela (FK de entrada):**

- `comercial.ClienteCarteira.CarteiraId`
- `organizacao.CarteiraMunicipio.CarteiraId`
- `organizacao.Meta.CarteiraId` (opcional)
- `processo.Processo.CarteiraId` (opcional)

**Índices:**

- `PK_Carteira` (clustered, único): Id
- `IX_Carteira_EmpresaId_LinhaDeNegocioId` (nonclustered): EmpresaId,LinhaDeNegocioId · filtro ([ExcluidoEm] IS NULL)
- `IX_Carteira_EmpresaId_Natureza` (nonclustered): EmpresaId,Natureza · filtro ([ExcluidoEm] IS NULL)
- `IX_Carteira_EquipeId` (nonclustered): EquipeId
- `IX_Carteira_LinhaDeNegocioId` (nonclustered): LinhaDeNegocioId
- `IX_Carteira_PracaId` (nonclustered): PracaId
- `IX_Carteira_ResponsavelId` (nonclustered): ResponsavelId · filtro ([EstaAtiva]=(1) AND [ExcluidoEm] IS NULL)
- `IX_Carteira_SupervisorId` (nonclustered): SupervisorId
- `UX_Carteira_ChavePublica` (nonclustered, único): ChavePublica
- `UX_Carteira_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_Carteira_Natureza`: ([Natureza]='Teste' OR [Natureza]='Administrativa' OR [Natureza]='Comercial')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·10 | Aplicacao 2·2·3 | Repositorio 16·5·2 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 4·2·12 | Integracao 0·0·5 | Testes 4·4·8 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs (1)`, `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCarteiras.cs (6)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTerritorio.cs (5)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDoPainelDoCen.cs (3)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)` e mais 2

## `organizacao.CarteiraMunicipio`

- **Entidade EF:** `CarteiraMunicipio` · **DbSet:** `CarteiraMunicipios` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260905191622_MunicipioEAgrupamentoDeCarteira`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `CarteiraId` | bigint |  |  |  |
| 3 | `MunicipioId` | int |  |  |  |
| 4 | `VinculadoEm` | datetime2(3) |  |  |  |
| 5 | `VinculadoPorId` | bigint |  |  |  |
| 6 | `DesvinculadoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `CarteiraMunicipio.CarteiraId` → `organizacao.Carteira.Id` (obrigatória)
- `CarteiraMunicipio.MunicipioId` → `organizacao.Municipio.Id` (obrigatória)

**Índices:**

- `PK_CarteiraMunicipio` (clustered, único): Id
- `IX_CarteiraMunicipio_MunicipioId` (nonclustered): MunicipioId · filtro ([DesvinculadoEm] IS NULL)
- `UX_CarteiraMunicipio_Carteira_Municipio_Vigente` (nonclustered, único): CarteiraId,MunicipioId · filtro ([DesvinculadoEm] IS NULL)

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·5 | Aplicacao 2·1·2 | Repositorio 2·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·2·6 | Integracao 0·0·0 | Testes 1·1·1 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (2)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTerritorio.cs (2)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `organizacao.Empresa`

- **Entidade EF:** `Empresa` · **DbSet:** `Empresas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 18 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 3 | `Codigo` | varchar(20) |  |  |  |
| 4 | `Nome` | nvarchar(120) |  |  |  |
| 5 | `Cnpj` | varchar(14) | sim |  |  |
| 6 | `EmpresaPaiId` | int | sim |  |  |
| 7 | `Caminho` | varchar(400) |  |  |  |
| 8 | `Nivel` | smallint |  |  |  |
| 9 | `EstaAtiva` | bit |  |  |  |
| 10 | `CriadoEm` | datetime2(3) |  |  |  |
| 11 | `AlteradoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `Empresa.EmpresaPaiId` → `organizacao.Empresa.Id` (opcional)

**Dependem dela (FK de entrada):**

- `auditoria.AlteracaoDeCampo.EmpresaId`
- `comercial.Alerta.EmpresaId`
- `comercial.CanalContato.EmpresaId`
- `comercial.Cliente.EmpresaId`
- `comercial.ConsentimentoComunicacao.EmpresaId`
- `comercial.Contato.EmpresaId`
- `comercial.Endereco.EmpresaId`
- `comercial.FaturamentoDoCliente.EmpresaId`
- `comercial.FaturamentoSemCliente.EmpresaId`
- `comercial.Lead.EmpresaId`
- `documento.Documento.EmpresaId`
- `frota.Equipamento.EmpresaId`
- `frota.VendaDeMaquina.EmpresaId`
- `frota.VendaDeMaquina.EmpresaDoFaturamentoId` (opcional)
- `frota.VinculoDeClienteComEquipamento.EmpresaId`
- `integracao.CompradorPendente.EmpresaId`
- `integracao.CorrespondenciaDaOrigem.EmpresaCorrespondenteId` (opcional)
- `integracao.DivergenciaDeIntegracao.EmpresaId`
- `metadado.Preenchimento.EmpresaId`
- `organizacao.Carteira.EmpresaId`
- `organizacao.Empresa.EmpresaPaiId` (opcional)
- `organizacao.Meta.EmpresaId`
- `organizacao.MunicipioDaAreaDeAtuacao.EmpresaResponsavelId` (opcional)
- `processo.Interacao.EmpresaId`
- `processo.ItemDeProposta.EmpresaId`
- `processo.Processo.EmpresaId`
- `processo.Tarefa.EmpresaId`
- `processo.VendaPerdida.EmpresaId`
- `relatorio.Relatorio.EmpresaId`
- `seguranca.CompartilhamentoDeRegistro.EmpresaId`
- `seguranca.Equipe.EmpresaId`
- `seguranca.Usuario.EmpresaId`

**Índices:**

- `PK_Empresa` (clustered, único): Id
- `IX_Empresa_Caminho` (nonclustered): Caminho · filtro ([EstaAtiva]=(1))
- `IX_Empresa_EmpresaPaiId` (nonclustered): EmpresaPaiId
- `UX_Empresa_ChavePublica` (nonclustered, único): ChavePublica
- `UX_Empresa_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_Empresa_Nivel`: ([Nivel]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·20 | Aplicacao 0·0·3 | Repositorio 11·4·4 | InfraOutros 3·2·3 | Api 0·0·3 | Carga 10·4·7 | Integracao 2·2·7 | Testes 10·8·46 | Scripts 17·6·22
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.Faturamento.cs (4)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (1)`, `src/Tracbel.Crm.Carga/CargaDoArt.cs (2)`, `src/Tracbel.Crm.Carga/PreparacaoDaCarga.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Identidade/EscopoDeAcesso.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Identidade/ResolvedorDeContextoProvisorio.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCatalogos.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeHistoricoComercial.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTerritorio.cs (4)`, `src/Tracbel.Crm.Integracao/Carga/RegistrosDaCarga.cs (1)`, `src/Tracbel.Crm.Integracao/Protheus/LeitorDeFaturamentoDoProtheus.cs (1)` e mais 14

## `organizacao.HierarquiaComercial`

- **Entidade EF:** `HierarquiaComercial` · **DbSet:** `HierarquiaComercial` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `AncestralId` | bigint |  |  |  |
| 3 | `DescendenteId` | bigint |  |  |  |
| 4 | `Profundidade` | smallint |  |  |  |

**Referencia (FK de saída):**

- `HierarquiaComercial.AncestralId` → `seguranca.Usuario.Id` (obrigatória)
- `HierarquiaComercial.DescendenteId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_HierarquiaComercial` (clustered, único): Id
- `IX_HierarquiaComercial_DescendenteId_Profundidade` (nonclustered): DescendenteId,Profundidade
- `UX_HierarquiaComercial_Ancestral_Descendente` (nonclustered, único): AncestralId,DescendenteId

**Restrições CHECK:**

- `CK_HierarquiaComercial_Profundidade`: ([Profundidade]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `organizacao.LinhaDeNegocio`

- **Entidade EF:** `LinhaDeNegocio` · **DbSet:** `LinhasDeNegocio` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(20) |  |  |  |
| 3 | `Nome` | nvarchar(80) |  |  |  |
| 4 | `DiasCicloClasseA` | smallint | sim |  |  |
| 5 | `DiasCicloClasseB` | smallint | sim |  |  |
| 6 | `DiasCicloClasseC` | smallint | sim |  |  |
| 7 | `DiasCicloClasseD` | smallint | sim |  |  |
| 8 | `Ordem` | smallint |  |  |  |
| 9 | `EstaAtiva` | bit |  |  |  |

**Dependem dela (FK de entrada):**

- `comercial.Lead.LinhaNegocioId` (opcional)
- `organizacao.Carteira.LinhaDeNegocioId`
- `organizacao.Meta.LinhaDeNegocioId` (opcional)
- `organizacao.Praca.LinhaDeNegocioId`
- `processo.TipoProcesso.LinhaDeNegocioId` (opcional)

**Índices:**

- `PK_LinhaDeNegocio` (clustered, único): Id
- `UX_LinhaDeNegocio_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_LinhaDeNegocio_Ciclo`: (coalesce([DiasCicloClasseA],(1))>(0) AND coalesce([DiasCicloClasseB],(1))>(0) AND coalesce([DiasCicloClasseC],(1))>(0) AND coalesce([DiasCicloClasseD],(1))>(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·5 | Aplicacao 0·0·0 | Repositorio 6·5·4 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 7·2·8 | Integracao 0·0·1 | Testes 3·3·5 | Scripts 4·2·4
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (6)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCarteiras.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTerritorio.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDoPainelDoCen.cs (1)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (1)`

## `organizacao.Meta`

- **Entidade EF:** `Meta` · **DbSet:** `Metas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `LinhaDeNegocioId` | int | sim |  |  |
| 4 | `CarteiraId` | bigint | sim |  |  |
| 5 | `UsuarioId` | bigint | sim |  |  |
| 6 | `Tipo` | varchar(20) |  |  |  |
| 7 | `PeriodoInicio` | date |  |  |  |
| 8 | `PeriodoFim` | date |  |  |  |
| 9 | `Alvo` | decimal(18,2) |  |  |  |
| 10 | `Observacao` | nvarchar(400) | sim |  |  |
| 11 | `EstaAtiva` | bit |  |  |  |
| 12 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 13 | `CriadoEm` | datetime2(3) |  |  |  |
| 14 | `CriadoPorId` | bigint |  |  |  |
| 15 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 16 | `AlteradoPorId` | bigint | sim |  |  |
| 17 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 18 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `Meta.CarteiraId` → `organizacao.Carteira.Id` (opcional)
- `Meta.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Meta.LinhaDeNegocioId` → `organizacao.LinhaDeNegocio.Id` (opcional)
- `Meta.UsuarioId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_Meta` (clustered, único): Id
- `IX_Meta_CarteiraId` (nonclustered): CarteiraId
- `IX_Meta_EmpresaId_Tipo_PeriodoInicio` (nonclustered): EmpresaId,Tipo,PeriodoInicio · filtro ([ExcluidoEm] IS NULL)
- `IX_Meta_LinhaDeNegocioId` (nonclustered): LinhaDeNegocioId
- `IX_Meta_UsuarioId` (nonclustered): UsuarioId
- `UX_Meta_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_Meta_Alvo`: ([Alvo]>=(0))
- `CK_Meta_Periodo`: ([PeriodoFim]>=[PeriodoInicio])
- `CK_Meta_Tipo`: ([Tipo]='Volume' OR [Tipo]='Frequencia' OR [Tipo]='Cobertura' OR [Tipo]='Faturamento')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·5 | Aplicacao 2·1·2 | Repositorio 1·1·1 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 2·1·3 | Scripts 3·1·4
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ObterIndicadoresExecutivos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresExecutivos.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `organizacao.Municipio`

- **Entidade EF:** `Municipio` · **DbSet:** `Municipios` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 5571 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260905191622_MunicipioEAgrupamentoDeCarteira`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `CodigoIbge` | int | sim |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Uf` | char(2) |  |  |  |
| 5 | `EstaAtivo` | bit |  |  |  |

**Dependem dela (FK de entrada):**

- `comercial.Endereco.MunicipioId` (opcional)
- `organizacao.AreaPlantadaNoMunicipio.MunicipioId`
- `organizacao.CarteiraMunicipio.MunicipioId`
- `organizacao.MunicipioDaAreaDeAtuacao.MunicipioId`
- `organizacao.ResponsavelPeloMunicipio.MunicipioId`

**Índices:**

- `PK_Municipio` (clustered, único): Id
- `IX_Municipio_Nome` (nonclustered): Nome · filtro ([EstaAtivo]=(1))
- `UX_Municipio_CodigoIbge` (nonclustered, único): CodigoIbge · filtro ([CodigoIbge] IS NOT NULL)
- `UX_Municipio_Uf_Nome` (nonclustered, único): Uf,Nome

**Restrições CHECK:**

- `CK_Municipio_CodigoIbge`: ([CodigoIbge] IS NULL OR [CodigoIbge]>=(1000000) AND [CodigoIbge]<=(5999999))
- `CK_Municipio_Uf`: (([Uf]) collate Latin1_General_BIN2='TO' OR ([Uf]) collate Latin1_General_BIN2='SE' OR ([Uf]) collate Latin1_General_BIN2='SP' OR ([Uf]) collate Latin1_General_BIN2='SC' OR ([Uf]) collate Latin1_General_BIN2='RR' OR ([Uf]) collate Latin1_General_BIN2='RO' OR ([Uf]) collate Latin1_General_BIN2='RS' O …

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·13 | Aplicacao 3·1·1 | Repositorio 9·2·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 8·4·23 | Integracao 0·0·7 | Testes 6·4·43 | Scripts 11·3·16
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (3)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (4)`, `src/Tracbel.Crm.Carga/CargaDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/ConsolidacaoDeGrafiasCortadas.cs (1)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Dominio/Comercial/Endereco.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (4)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTerritorio.cs (5)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Carga/ConsolidacaoDeGrafiasCortadasTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Territorio/VisaoDaEmpresaTestes.cs (3)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)` e mais 3

## `organizacao.MunicipioDaAreaDeAtuacao`

- **Entidade EF:** `MunicipioDaAreaDeAtuacao` · **DbSet:** `MunicipiosDaAreaDeAtuacao` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `MunicipioId` | int |  |  |  |
| 3 | `PertenceAAdr` | bit |  |  |  |
| 4 | `Regiao` | varchar(20) |  |  |  |
| 5 | `EmpresaResponsavelId` | int | sim |  |  |
| 6 | `ArquivoDeOrigem` | nvarchar(200) |  |  |  |
| 7 | `LinhaNaOrigem` | int |  |  |  |
| 8 | `ImportadoEm` | datetime2(3) |  |  |  |
| 9 | `ImportadoPorId` | bigint |  |  |  |
| 10 | `EncerradoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `MunicipioDaAreaDeAtuacao.EmpresaResponsavelId` → `organizacao.Empresa.Id` (opcional)
- `MunicipioDaAreaDeAtuacao.MunicipioId` → `organizacao.Municipio.Id` (obrigatória)
- `MunicipioDaAreaDeAtuacao.ImportadoPorId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_MunicipioDaAreaDeAtuacao` (clustered, único): Id
- `IX_MunicipioDaAreaDeAtuacao_EmpresaResponsavelId` (nonclustered): EmpresaResponsavelId
- `IX_MunicipioDaAreaDeAtuacao_ImportadoPorId` (nonclustered): ImportadoPorId
- `UX_MunicipioDaAreaDeAtuacao_Municipio_Vigente` (nonclustered, único): MunicipioId · filtro ([EncerradoEm] IS NULL)

**Restrições CHECK:**

- `CK_MunicipioDaAreaDeAtuacao_LinhaNaOrigem`: ([LinhaNaOrigem]>=(2))
- `CK_MunicipioDaAreaDeAtuacao_Regiao`: ([Regiao]='Noroeste' OR [Regiao]='Norte' OR [Regiao]='NaoInformada')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 1·1·1 | Repositorio 1·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 2·1·1 | Integracao 0·0·0 | Testes 4·3·5 | Scripts 6·3·6
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (2)`, `tests/Tracbel.Crm.Aplicacao.Testes/Territorio/VisaoDaEmpresaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (2)`, `scripts/deploy/carregar-territorio-no-servidor.ps1 (1)`

## `organizacao.Praca`

- **Entidade EF:** `Praca` · **DbSet:** `Pracas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(20) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Uf` | char(2) | sim |  |  |
| 5 | `LinhaDeNegocioId` | int |  |  |  |
| 6 | `AnoReferencia` | smallint |  |  |  |
| 7 | `PotencialEstimado` | decimal(18,2) | sim |  |  |
| 8 | `MaquinasEstimadas` | int | sim |  |  |
| 9 | `EstaAtiva` | bit |  |  |  |

**Referencia (FK de saída):**

- `Praca.LinhaDeNegocioId` → `organizacao.LinhaDeNegocio.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `organizacao.Carteira.PracaId` (opcional)

**Índices:**

- `PK_Praca` (clustered, único): Id
- `IX_Praca_LinhaDeNegocioId` (nonclustered): LinhaDeNegocioId
- `UX_Praca_Codigo_Linha_Ano` (nonclustered, único): Codigo,LinhaDeNegocioId,AnoReferencia

**Restrições CHECK:**

- `CK_Praca_AnoReferencia`: ([AnoReferencia]>=(2000) AND [AnoReferencia]<=(2100))
- `CK_Praca_Uf`: ([Uf] IS NULL OR ([Uf]) collate Latin1_General_BIN2 like '[A-Z][A-Z]')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `organizacao.RegraDePotencial`

- **Entidade EF:** `RegraDePotencial` · **DbSet:** `RegrasDePotencial` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `ProdutoCodigoIbge` | int |  |  |  |
| 3 | `ProdutoNome` | nvarchar(120) |  |  |  |
| 4 | `HectaresPorMaquina` | decimal(10,2) |  |  |  |
| 5 | `ModeloDeReferencia` | nvarchar(60) |  |  |  |
| 6 | `Situacao` | varchar(20) |  |  |  |
| 7 | `Origem` | nvarchar(400) |  |  |  |
| 8 | `InformadaEm` | date |  |  |  |
| 9 | `EstaAtiva` | bit |  |  |  |

**Índices:**

- `PK_RegraDePotencial` (clustered, único): Id
- `IX_RegraDePotencial_ProdutoCodigoIbge` (nonclustered): ProdutoCodigoIbge · filtro ([EstaAtiva]=(1))

**Restrições CHECK:**

- `CK_RegraDePotencial_HectaresPorMaquina`: ([HectaresPorMaquina]>(0))
- `CK_RegraDePotencial_Produto`: ([ProdutoCodigoIbge]>(0))
- `CK_RegraDePotencial_Situacao`: ([Situacao]='Confirmada' OR [Situacao]='AConfirmar')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 1·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·3 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `organizacao.ResponsavelPeloMunicipio`

- **Entidade EF:** `ResponsavelPeloMunicipio` · **DbSet:** `ResponsaveisPelosMunicipios` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/OrganizacaoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `MunicipioId` | int |  |  |  |
| 3 | `Papel` | varchar(10) |  |  |  |
| 4 | `Fonte` | varchar(40) |  |  |  |
| 5 | `NomeNaOrigem` | nvarchar(120) |  |  |  |
| 6 | `Situacao` | varchar(30) |  |  |  |
| 7 | `UsuarioId` | bigint | sim |  |  |
| 8 | `ChaveNaOrigem` | nvarchar(120) | sim |  |  |
| 9 | `ArquivoDeOrigem` | nvarchar(200) |  |  |  |
| 10 | `LinhaNaOrigem` | int |  |  |  |
| 11 | `ImportadoEm` | datetime2(3) |  |  |  |
| 12 | `ImportadoPorId` | bigint |  |  |  |
| 13 | `EncerradoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `ResponsavelPeloMunicipio.MunicipioId` → `organizacao.Municipio.Id` (obrigatória)
- `ResponsavelPeloMunicipio.ImportadoPorId` → `seguranca.Usuario.Id` (obrigatória)
- `ResponsavelPeloMunicipio.UsuarioId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_ResponsavelPeloMunicipio` (clustered, único): Id
- `IX_ResponsavelPeloMunicipio_ImportadoPorId` (nonclustered): ImportadoPorId
- `IX_ResponsavelPeloMunicipio_UsuarioId` (nonclustered): UsuarioId
- `UX_ResponsavelPeloMunicipio_Municipio_Papel_Fonte_Vigente` (nonclustered, único): MunicipioId,Papel,Fonte · filtro ([EncerradoEm] IS NULL)

**Restrições CHECK:**

- `CK_ResponsavelPeloMunicipio_Fonte`: ([Fonte]='PlanilhaCenEGestorPorMunicipio' OR [Fonte]='PlanilhaAreaDeAtuacao')
- `CK_ResponsavelPeloMunicipio_LinhaNaOrigem`: ([LinhaNaOrigem]>=(2))
- `CK_ResponsavelPeloMunicipio_Papel`: ([Papel]='Gestor' OR [Papel]='Cen')
- `CK_ResponsavelPeloMunicipio_Situacao`: ([Situacao]='NaoIdentificado' OR [Situacao]='VagaAContratar' OR [Situacao]='UsuarioIdentificado')
- `CK_ResponsavelPeloMunicipio_UsuarioIdentificado`: ([Situacao]='UsuarioIdentificado' AND [UsuarioId] IS NOT NULL OR [Situacao]<>'UsuarioIdentificado' AND [UsuarioId] IS NULL)

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 1·1·1 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 2·1·2 | Integracao 0·0·1 | Testes 1·1·8 | Scripts 5·3·7
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (1)`, `scripts/deploy/carregar-territorio-no-servidor.ps1 (1)`

## `processo.Fase`

- **Entidade EF:** `Fase` · **DbSet:** `Fases` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `TipoProcessoId` | int |  |  |  |
| 3 | `Codigo` | varchar(40) |  |  |  |
| 4 | `Nome` | nvarchar(80) |  |  |  |
| 5 | `Ordem` | smallint |  |  |  |
| 6 | `Marco` | nvarchar(60) | sim |  |  |
| 7 | `ProbabilidadePercentual` | smallint | sim |  |  |
| 8 | `ExigeCamposObrigatorios` | bit |  |  |  |
| 9 | `EhFinal` | bit |  |  |  |

**Referencia (FK de saída):**

- `Fase.TipoProcessoId` → `processo.TipoProcesso.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `processo.PassagemDeFase.FaseId`
- `processo.Processo.FaseId`
- `processo.Regra.EfeitoFaseId` (opcional)
- `processo.Resultado.FaseDestinoId` (opcional)

**Índices:**

- `PK_Fase` (clustered, único): Id
- `IX_Fase_TipoProcessoId_Ordem` (nonclustered): TipoProcessoId,Ordem
- `UX_Fase_Tipo_Codigo` (nonclustered, único): TipoProcessoId,Codigo

**Restrições CHECK:**

- `CK_Fase_Probabilidade`: ([ProbabilidadePercentual] IS NULL OR [ProbabilidadePercentual]>=(0) AND [ProbabilidadePercentual]<=(100))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·7 | Aplicacao 0·0·0 | Repositorio 4·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·2·8 | Integracao 0·0·7 | Testes 1·1·1 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeProcessos.cs (4)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.Interacao`

- **Entidade EF:** `Interacao` · **DbSet:** `Interacoes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 3 | `EmpresaId` | int |  |  |  |
| 4 | `TipoTarefaId` | int |  |  |  |
| 5 | `ProcessoId` | bigint | sim |  |  |
| 6 | `ClienteId` | bigint | sim |  |  |
| 7 | `ContatoId` | bigint | sim |  |  |
| 8 | `LeadId` | bigint | sim |  |  |
| 9 | `TarefaId` | bigint | sim |  |  |
| 10 | `Assunto` | nvarchar(200) |  |  |  |
| 11 | `Detalhe` | nvarchar(4000) | sim |  |  |
| 12 | `ResultadoId` | int | sim |  |  |
| 13 | `ResultadoComplemento` | nvarchar(200) | sim |  |  |
| 14 | `Natureza` | varchar(12) |  |  |  |
| 15 | `OcorridaEm` | datetime2(3) |  |  |  |
| 16 | `DuracaoMinutos` | int | sim |  |  |
| 17 | `Latitude` | decimal(10,7) | sim |  |  |
| 18 | `Longitude` | decimal(10,7) | sim |  |  |
| 19 | `RegistradoPorId` | bigint |  |  |  |
| 20 | `CriadoEm` | datetime2(3) |  |  |  |

**Referencia (FK de saída):**

- `Interacao.ClienteId` → `comercial.Cliente.Id` (opcional)
- `Interacao.ContatoId` → `comercial.Contato.Id` (opcional)
- `Interacao.LeadId` → `comercial.Lead.Id` (opcional)
- `Interacao.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Interacao.ProcessoId` → `processo.Processo.Id` (opcional)
- `Interacao.ResultadoId` → `processo.Resultado.Id` (opcional)
- `Interacao.TarefaId` → `processo.Tarefa.Id` (opcional)
- `Interacao.TipoTarefaId` → `processo.TipoTarefa.Id` (obrigatória)
- `Interacao.RegistradoPorId` → `seguranca.Usuario.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `metadado.Preenchimento.InteracaoId` (opcional)
- `processo.InteracaoParticipante.InteracaoId`
- `processo.PassagemDeFase.InteracaoOrigemId` (opcional)
- `processo.RegraExecucao.InteracaoId` (opcional)
- `processo.Tarefa.InteracaoConclusaoId` (opcional)
- `processo.Tarefa.InteracaoOrigemId` (opcional)

**Índices:**

- `PK_Interacao` (clustered, único): Id
- `IX_Interacao_ClienteId_OcorridaEm` (nonclustered): ClienteId,OcorridaEm DESC
- `IX_Interacao_ContatoId` (nonclustered): ContatoId
- `IX_Interacao_EmpresaId` (nonclustered): EmpresaId
- `IX_Interacao_LeadId` (nonclustered): LeadId
- `IX_Interacao_ProcessoId_OcorridaEm` (nonclustered): ProcessoId,OcorridaEm DESC
- `IX_Interacao_RegistradoPorId` (nonclustered): RegistradoPorId
- `IX_Interacao_ResultadoId` (nonclustered): ResultadoId
- `IX_Interacao_TarefaId` (nonclustered): TarefaId
- `IX_Interacao_TipoTarefaId` (nonclustered): TipoTarefaId
- `UX_Interacao_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_Interacao_Coordenada`: ([Latitude] IS NULL AND [Longitude] IS NULL OR [Latitude]>=(-90) AND [Latitude]<=(90) AND ([Longitude]>=(-180) AND [Longitude]<=(180)))
- `CK_Interacao_Duracao`: ([DuracaoMinutos] IS NULL OR [DuracaoMinutos]>=(0))
- `CK_Interacao_Natureza`: ([Natureza]='Sistema' OR [Natureza]='Receptiva' OR [Natureza]='Ativa')
- `CK_Interacao_TemVinculo`: ([ProcessoId] IS NOT NULL OR [ClienteId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [LeadId] IS NOT NULL)

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·8 | Aplicacao 1·1·2 | Repositorio 1·1·3 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 4·2·11 | Integracao 0·0·2 | Testes 3·3·2 | Scripts 7·2·8
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Dominio/Comum/Paginacao.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeInteracoes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Persistencia/FronteiraDeEmpresaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (6)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (1)`

## `processo.InteracaoParticipante`

- **Entidade EF:** `InteracaoParticipante` · **DbSet:** `InteracaoParticipantes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `InteracaoId` | bigint |  |  |  |
| 3 | `Papel` | varchar(20) |  |  |  |
| 4 | `UsuarioId` | bigint | sim |  |  |
| 5 | `ContatoId` | bigint | sim |  |  |
| 6 | `NomeExterno` | nvarchar(200) | sim |  |  |

**Referencia (FK de saída):**

- `InteracaoParticipante.ContatoId` → `comercial.Contato.Id` (opcional)
- `InteracaoParticipante.InteracaoId` → `processo.Interacao.Id` (obrigatória)
- `InteracaoParticipante.UsuarioId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_InteracaoParticipante` (clustered, único): Id
- `IX_InteracaoParticipante_ContatoId` (nonclustered): ContatoId
- `IX_InteracaoParticipante_InteracaoId` (nonclustered): InteracaoId
- `IX_InteracaoParticipante_UsuarioId` (nonclustered): UsuarioId

**Restrições CHECK:**

- `CK_InteracaoParticipante_Identificado`: ([UsuarioId] IS NOT NULL OR [ContatoId] IS NOT NULL OR [NomeExterno] IS NOT NULL)
- `CK_InteracaoParticipante_Papel`: ([Papel]='Participante' OR [Papel]='Copia' OR [Papel]='Destinatario' OR [Papel]='Autor')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 1·1·1 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/Program.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.ItemDeProposta`

- **Entidade EF:** `ItemDeProposta` · **DbSet:** `ItensDeProposta` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint, AddForeignKey, CreateIndex, DropForeignKey, DropIndex)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ProcessoId` | bigint |  |  |  |
| 4 | `Ordem` | smallint |  |  |  |
| 5 | `ModeloId` | int | sim |  |  |
| 6 | `EquipamentoId` | bigint | sim |  |  |
| 7 | `Descricao` | nvarchar(400) |  |  |  |
| 8 | `Quantidade` | decimal(12,3) |  |  |  |
| 9 | `PrecoUnitario` | decimal(18,2) | sim |  |  |
| 10 | `DescontoPercentual` | decimal(5,2) | sim |  |  |
| 11 | `ValorTotal` | decimal(18,2) |  |  |  |
| 12 | `CondicaoPagamentoId` | int | sim |  |  |
| 13 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 14 | `CriadoEm` | datetime2(3) |  |  |  |
| 15 | `CriadoPorId` | bigint |  |  |  |
| 16 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 17 | `AlteradoPorId` | bigint | sim |  |  |
| 18 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 19 | `Versao` | timestamp | sim |  |  |
| 20 | `CatalogoDaCondicaoPagamentoId` | int |  |  | ((7)) |

**Referencia (FK de saída):**

- `ItemDeProposta.EquipamentoId` → `frota.Equipamento.Id` (opcional)
- `ItemDeProposta.ModeloId` → `frota.Modelo.Id` (opcional)
- `ItemDeProposta.CatalogoDaCondicaoPagamentoId,CondicaoPagamentoId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `ItemDeProposta.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `ItemDeProposta.ProcessoId` → `processo.Processo.Id` (obrigatória)

**Índices:**

- `PK_ItemDeProposta` (clustered, único): Id
- `IX_ItemDeProposta_CatalogoDaCondicaoPagamentoId_CondicaoPagamentoId` (nonclustered): CatalogoDaCondicaoPagamentoId,CondicaoPagamentoId
- `IX_ItemDeProposta_EmpresaId` (nonclustered): EmpresaId
- `IX_ItemDeProposta_EquipamentoId` (nonclustered): EquipamentoId
- `IX_ItemDeProposta_ModeloId` (nonclustered): ModeloId
- `UX_ItemDeProposta_ChavePublica` (nonclustered, único): ChavePublica
- `UX_ItemDeProposta_Processo_Ordem` (nonclustered, único): ProcessoId,Ordem · filtro ([ExcluidoEm] IS NULL)

**Restrições CHECK:**

- `CK_ItemDeProposta_CatalogoDaCondicaoPagamentoId`: ([CatalogoDaCondicaoPagamentoId]=(7))
- `CK_ItemDeProposta_Desconto`: ([DescontoPercentual] IS NULL OR [DescontoPercentual]>=(0) AND [DescontoPercentual]<=(100))
- `CK_ItemDeProposta_Quantidade`: ([Quantidade]>(0))
- `CK_ItemDeProposta_ValorTotal`: ([ValorTotal]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·5 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 1·1·1 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.MotivoDePerda`

- **Entidade EF:** `MotivoDePerda` · **DbSet:** `MotivosDePerda` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(40) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Categoria` | varchar(20) |  |  |  |
| 5 | `ExigeConcorrente` | bit |  |  |  |
| 6 | `ExigeObservacao` | bit |  |  |  |
| 7 | `Ordem` | smallint |  |  |  |
| 8 | `EstaAtivo` | bit |  |  |  |

**Dependem dela (FK de entrada):**

- `processo.Processo.MotivoDePerdaId` (opcional)
- `processo.VendaPerdida.MotivoDePerdaId`

**Índices:**

- `PK_MotivoDePerda` (clustered, único): Id
- `UX_MotivoDePerda_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_MotivoDePerda_Categoria`: ([Categoria]='Outro' OR [Categoria]='Concorrencia' OR [Categoria]='Desistencia' OR [Categoria]='Financiamento' OR [Categoria]='Produto' OR [Categoria]='Prazo' OR [Categoria]='Preco')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 3·2·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 6·3·3 | Integracao 0·0·0 | Testes 1·1·1 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.VendaPerdida.cs (3)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeProcessos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeVendasPerdidas.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.PassagemDeFase`

- **Entidade EF:** `PassagemDeFase` · **DbSet:** `PassagensDeFase` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `ProcessoId` | bigint |  |  |  |
| 3 | `FaseId` | int |  |  |  |
| 4 | `Ordem` | int |  |  |  |
| 5 | `EntrouEm` | datetime2(3) |  |  |  |
| 6 | `SaiuEm` | datetime2(3) | sim |  |  |
| 7 | `HorasUteis` | decimal(10,2) | sim |  |  |
| 8 | `InteracaoOrigemId` | bigint | sim |  |  |
| 9 | `RegraOrigemId` | int | sim |  |  |
| 10 | `EntrouPorId` | bigint |  |  |  |

**Referencia (FK de saída):**

- `PassagemDeFase.FaseId` → `processo.Fase.Id` (obrigatória)
- `PassagemDeFase.InteracaoOrigemId` → `processo.Interacao.Id` (opcional)
- `PassagemDeFase.ProcessoId` → `processo.Processo.Id` (obrigatória)
- `PassagemDeFase.RegraOrigemId` → `processo.Regra.Id` (opcional)
- `PassagemDeFase.EntrouPorId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_PassagemDeFase` (clustered, único): Id
- `IX_PassagemDeFase_EntrouPorId` (nonclustered): EntrouPorId
- `IX_PassagemDeFase_FaseId` (nonclustered): FaseId
- `IX_PassagemDeFase_InteracaoOrigemId` (nonclustered): InteracaoOrigemId
- `IX_PassagemDeFase_ProcessoId` (nonclustered): ProcessoId · filtro ([SaiuEm] IS NULL)
- `IX_PassagemDeFase_RegraOrigemId` (nonclustered): RegraOrigemId
- `UX_PassagemDeFase_Processo_Ordem` (nonclustered, único): ProcessoId,Ordem

**Restrições CHECK:**

- `CK_PassagemDeFase_Ordem`: ([Ordem]>=(1))
- `CK_PassagemDeFase_Periodo`: ([SaiuEm] IS NULL OR [SaiuEm]>=[EntrouEm])

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 1·1·1 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/Program.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.Processo`

- **Entidade EF:** `Processo` · **DbSet:** `Processos` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint, AddForeignKey, CreateIndex, DropForeignKey, DropIndex)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `Numero` | bigint |  |  |  |
| 3 | `EmpresaId` | int |  |  |  |
| 4 | `TipoProcessoId` | int |  |  |  |
| 5 | `ClienteId` | bigint |  |  |  |
| 6 | `ContatoId` | bigint | sim |  |  |
| 7 | `CarteiraId` | bigint | sim |  |  |
| 8 | `Titulo` | nvarchar(200) |  |  |  |
| 9 | `Descricao` | nvarchar(4000) | sim |  |  |
| 10 | `FaseId` | int |  |  |  |
| 11 | `FaseDesde` | datetime2(3) |  |  |  |
| 12 | `Situacao` | varchar(20) |  |  |  |
| 13 | `SituacaoDesde` | datetime2(3) |  |  |  |
| 14 | `MotivoDePerdaId` | int | sim |  |  |
| 15 | `ObservacaoDaPerda` | nvarchar(1000) | sim |  |  |
| 16 | `ConcorrenteId` | int | sim |  |  |
| 17 | `ValorEstimado` | decimal(18,2) | sim |  |  |
| 18 | `ValorFinal` | decimal(18,2) | sim |  |  |
| 19 | `Quantidade` | decimal(12,3) | sim |  |  |
| 20 | `PrevisaoConclusao` | date | sim |  |  |
| 21 | `PrevisaoConclusaoOriginal` | date | sim |  |  |
| 22 | `ConcluidoEm` | datetime2(3) | sim |  |  |
| 23 | `ProprietarioId` | bigint |  |  |  |
| 24 | `ProprietarioEquipeId` | bigint | sim |  |  |
| 25 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 26 | `CriadoEm` | datetime2(3) |  |  |  |
| 27 | `CriadoPorId` | bigint |  |  |  |
| 28 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 29 | `AlteradoPorId` | bigint | sim |  |  |
| 30 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 31 | `Versao` | timestamp | sim |  |  |
| 32 | `CatalogoDoConcorrenteId` | int |  |  | ((8)) |

**Referencia (FK de saída):**

- `Processo.ClienteId` → `comercial.Cliente.Id` (obrigatória)
- `Processo.ContatoId` → `comercial.Contato.Id` (opcional)
- `Processo.CatalogoDoConcorrenteId,ConcorrenteId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `Processo.CarteiraId` → `organizacao.Carteira.Id` (opcional)
- `Processo.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Processo.FaseId` → `processo.Fase.Id` (obrigatória)
- `Processo.MotivoDePerdaId` → `processo.MotivoDePerda.Id` (opcional)
- `Processo.TipoProcessoId` → `processo.TipoProcesso.Id` (obrigatória)
- `Processo.ProprietarioEquipeId` → `seguranca.Equipe.Id` (opcional)
- `Processo.ProprietarioId` → `seguranca.Usuario.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `comercial.Lead.ProcessoGeradoId` (opcional)
- `metadado.Preenchimento.ProcessoId` (opcional)
- `processo.Interacao.ProcessoId` (opcional)
- `processo.ItemDeProposta.ProcessoId`
- `processo.PassagemDeFase.ProcessoId`
- `processo.RegraExecucao.ProcessoId` (opcional)
- `processo.Tarefa.ProcessoId` (opcional)
- `processo.VendaPerdida.ProcessoId` (opcional)

**Índices:**

- `PK_Processo` (clustered, único): Id
- `IX_Processo_CarteiraId` (nonclustered): CarteiraId
- `IX_Processo_CatalogoDoConcorrenteId_ConcorrenteId` (nonclustered): CatalogoDoConcorrenteId,ConcorrenteId
- `IX_Processo_ClienteId` (nonclustered): ClienteId · filtro ([ExcluidoEm] IS NULL)
- `IX_Processo_ContatoId` (nonclustered): ContatoId
- `IX_Processo_EmpresaId_FaseId` (nonclustered): EmpresaId,FaseId · filtro ([Situacao]='Aberto' AND [ExcluidoEm] IS NULL)
- `IX_Processo_FaseId` (nonclustered): FaseId
- `IX_Processo_MotivoDePerdaId` (nonclustered): MotivoDePerdaId
- `IX_Processo_ProprietarioEquipeId` (nonclustered): ProprietarioEquipeId
- `IX_Processo_ProprietarioId_Situacao` (nonclustered): ProprietarioId,Situacao · filtro ([ExcluidoEm] IS NULL)
- `IX_Processo_TipoProcessoId` (nonclustered): TipoProcessoId
- `UX_Processo_ChavePublica` (nonclustered, único): ChavePublica
- `UX_Processo_Empresa_Numero` (nonclustered, único): EmpresaId,Numero

**Restrições CHECK:**

- `CK_Processo_CatalogoDoConcorrenteId`: ([CatalogoDoConcorrenteId]=(8))
- `CK_Processo_Encerramento`: ([Situacao]='Suspenso' OR [Situacao]='Aberto' OR [ConcluidoEm] IS NOT NULL)
- `CK_Processo_MotivoDePerda`: ([Situacao]<>'Perdido' OR [MotivoDePerdaId] IS NOT NULL)
- `CK_Processo_Situacao`: ([Situacao]='Cancelado' OR [Situacao]='Perdido' OR [Situacao]='Ganho' OR [Situacao]='Suspenso' OR [Situacao]='Aberto')
- `CK_Processo_Valores`: (([ValorEstimado] IS NULL OR [ValorEstimado]>=(0)) AND ([ValorFinal] IS NULL OR [ValorFinal]>=(0)))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 1·1·31 | Aplicacao 6·1·9 | Repositorio 9·4·9 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·2·33 | Integracao 2·1·28 | Testes 3·1·33 | Scripts 3·1·5
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs (6)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeInteracoes.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeProcessos.cs (5)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTarefas.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDoPainelDoCen.cs (1)`, `src/Tracbel.Crm.Integracao/Carga/LeitorDeCargaDoVortice.Processo.cs (2)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (3)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.Regra`

- **Entidade EF:** `Regra` · **DbSet:** `Regras` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaEWorkflowConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(60) |  |  |  |
| 3 | `Nome` | nvarchar(200) |  |  |  |
| 4 | `Descricao` | nvarchar(1000) | sim |  |  |
| 5 | `Evento` | varchar(40) |  |  |  |
| 6 | `TipoProcessoId` | int | sim |  |  |
| 7 | `TipoTarefaId` | int | sim |  |  |
| 8 | `ResultadoId` | int | sim |  |  |
| 9 | `Condicao` | nvarchar(2000) | sim |  |  |
| 10 | `Efeito` | varchar(40) |  |  |  |
| 11 | `EfeitoTipoTarefaId` | int | sim |  |  |
| 12 | `EfeitoFaseId` | int | sim |  |  |
| 13 | `EfeitoParametros` | nvarchar(max) | sim |  |  |
| 14 | `PrazoDiasUteis` | int |  |  |  |
| 15 | `ExpressaoDestinatario` | nvarchar(400) |  |  |  |
| 16 | `Ordem` | smallint |  |  |  |
| 17 | `EstaAtiva` | bit |  |  |  |
| 18 | `EhCritica` | bit |  |  |  |
| 19 | `CriadoEm` | datetime2(3) |  |  |  |
| 20 | `CriadoPorId` | bigint |  |  |  |
| 21 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 22 | `AlteradoPorId` | bigint | sim |  |  |

**Referencia (FK de saída):**

- `Regra.EfeitoFaseId` → `processo.Fase.Id` (opcional)
- `Regra.ResultadoId` → `processo.Resultado.Id` (opcional)
- `Regra.TipoProcessoId` → `processo.TipoProcesso.Id` (opcional)
- `Regra.EfeitoTipoTarefaId` → `processo.TipoTarefa.Id` (opcional)
- `Regra.TipoTarefaId` → `processo.TipoTarefa.Id` (opcional)

**Dependem dela (FK de entrada):**

- `processo.PassagemDeFase.RegraOrigemId` (opcional)
- `processo.RegraExecucao.RegraId`
- `processo.Tarefa.CriadaPorRegraId` (opcional)

**Índices:**

- `PK_Regra` (clustered, único): Id
- `IX_Regra_EfeitoFaseId` (nonclustered): EfeitoFaseId
- `IX_Regra_EfeitoTipoTarefaId` (nonclustered): EfeitoTipoTarefaId
- `IX_Regra_Evento_ResultadoId_Ordem` (nonclustered): Evento,ResultadoId,Ordem · filtro ([EstaAtiva]=(1))
- `IX_Regra_ResultadoId` (nonclustered): ResultadoId
- `IX_Regra_TipoProcessoId` (nonclustered): TipoProcessoId
- `IX_Regra_TipoTarefaId` (nonclustered): TipoTarefaId
- `UX_Regra_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_Regra_Efeito`: ([Efeito]='AtribuirCarteira' OR [Efeito]='ChamarWebhook' OR [Efeito]='Notificar' OR [Efeito]='EncerrarProcesso' OR [Efeito]='MoverFase' OR [Efeito]='CriarTarefa')
- `CK_Regra_EfeitoParametrosJson`: ([EfeitoParametros] IS NULL OR isjson([EfeitoParametros])=(1))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·21 | Aplicacao 3·1·1 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·1 | Testes 0·0·10 | Scripts 1·1·1
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Territorio/ConsultasDeTerritorio.cs (3)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `processo.RegraExecucao`

- **Entidade EF:** `ExecucaoRegra` · **DbSet:** `ExecucoesRegra` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaEWorkflowConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id,ExecutadoEm` · **Identity:** Id · **Particionada:** `PS_Mensal_RegraExecucao`
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `ExecutadoEm` | datetime2(3) |  |  |  |
| 3 | `RegraId` | int |  |  |  |
| 4 | `CorrelacaoId` | uniqueidentifier |  |  |  |
| 5 | `Evento` | varchar(40) |  |  |  |
| 6 | `ProcessoId` | bigint | sim |  |  |
| 7 | `TarefaId` | bigint | sim |  |  |
| 8 | `InteracaoId` | bigint | sim |  |  |
| 9 | `Resultado` | varchar(20) |  |  |  |
| 10 | `Motivo` | nvarchar(1000) | sim |  |  |
| 11 | `TarefaCriadaId` | bigint | sim |  |  |
| 12 | `DestinatarioResolvidoId` | bigint | sim |  |  |
| 13 | `DuracaoMs` | int |  |  |  |

**Referencia (FK de saída):**

- `RegraExecucao.InteracaoId` → `processo.Interacao.Id` (opcional)
- `RegraExecucao.ProcessoId` → `processo.Processo.Id` (opcional)
- `RegraExecucao.RegraId` → `processo.Regra.Id` (obrigatória)
- `RegraExecucao.TarefaCriadaId` → `processo.Tarefa.Id` (opcional)
- `RegraExecucao.TarefaId` → `processo.Tarefa.Id` (opcional)
- `RegraExecucao.DestinatarioResolvidoId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_RegraExecucao` (clustered, único): Id,ExecutadoEm
- `IX_RegraExecucao_CorrelacaoId` (nonclustered): ExecutadoEm,CorrelacaoId
- `IX_RegraExecucao_DestinatarioResolvidoId` (nonclustered): ExecutadoEm,DestinatarioResolvidoId
- `IX_RegraExecucao_InteracaoId` (nonclustered): ExecutadoEm,InteracaoId
- `IX_RegraExecucao_ProcessoId_ExecutadoEm` (nonclustered): ProcessoId,ExecutadoEm
- `IX_RegraExecucao_RegraId_ExecutadoEm_Resultado` (nonclustered): RegraId,ExecutadoEm,Resultado
- `IX_RegraExecucao_TarefaCriadaId` (nonclustered): ExecutadoEm,TarefaCriadaId
- `IX_RegraExecucao_TarefaId` (nonclustered): ExecutadoEm,TarefaId

**Restrições CHECK:**

- `CK_RegraExecucao_Resultado`: ([Resultado]='Suprimida' OR [Resultado]='Erro' OR [Resultado]='SemDestinatario' OR [Resultado]='RegraInativa' OR [Resultado]='CondicaoFalsa' OR [Resultado]='Disparou')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 1·1·2 | Scripts 3·1·0
- Arquivos com acesso: `tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.Resultado`

- **Entidade EF:** `Resultado` · **DbSet:** `Resultados` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `TipoTarefaId` | int |  |  |  |
| 3 | `Codigo` | varchar(40) |  |  |  |
| 4 | `Nome` | nvarchar(120) |  |  |  |
| 5 | `Classe` | varchar(20) |  |  |  |
| 6 | `FaseDestinoId` | int | sim |  |  |
| 7 | `ExigeJustificativa` | bit |  |  |  |
| 8 | `EstaAtivo` | bit |  |  |  |
| 9 | `UltimoUsoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `Resultado.FaseDestinoId` → `processo.Fase.Id` (opcional)
- `Resultado.TipoTarefaId` → `processo.TipoTarefa.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `processo.Interacao.ResultadoId` (opcional)
- `processo.Regra.ResultadoId` (opcional)
- `processo.Tarefa.ResultadoId` (opcional)

**Índices:**

- `PK_Resultado` (clustered, único): Id
- `IX_Resultado_FaseDestinoId` (nonclustered): FaseDestinoId
- `UX_Resultado_TipoTarefa_Codigo` (nonclustered, único): TipoTarefaId,Codigo

**Restrições CHECK:**

- `CK_Resultado_Classe`: ([Classe]='Cancelamento' OR [Classe]='Perda' OR [Classe]='Manutencao' OR [Classe]='Avanco')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·35 | Aplicacao 0·0·112 | Repositorio 2·2·6 | InfraOutros 0·0·17 | Api 0·0·8 | Carga 3·2·44 | Integracao 0·0·94 | Testes 1·1·17 | Scripts 3·1·11
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeInteracoes.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTarefas.cs (1)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.Tarefa`

- **Entidade EF:** `Tarefa` · **DbSet:** `Tarefas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ProcessoId` | bigint | sim |  |  |
| 4 | `ClienteId` | bigint | sim |  |  |
| 5 | `ContatoId` | bigint | sim |  |  |
| 6 | `TipoTarefaId` | int |  |  |  |
| 7 | `Assunto` | nvarchar(200) |  |  |  |
| 8 | `Detalhe` | nvarchar(4000) | sim |  |  |
| 9 | `ResponsavelId` | bigint |  |  |  |
| 10 | `ResponsavelEquipeId` | bigint | sim |  |  |
| 11 | `OrigemAtribuicao` | varchar(20) |  |  |  |
| 12 | `AgendadaPara` | datetime2(3) |  |  |  |
| 13 | `PrazoLimite` | datetime2(3) | sim |  |  |
| 14 | `Prioridade` | smallint |  |  |  |
| 15 | `Situacao` | varchar(20) |  |  |  |
| 16 | `ConcluidaEm` | datetime2(3) | sim |  |  |
| 17 | `ConcluidaPorId` | bigint | sim |  |  |
| 18 | `ResultadoId` | int | sim |  |  |
| 19 | `InteracaoConclusaoId` | bigint | sim |  |  |
| 20 | `CriadaPorRegraId` | int | sim |  |  |
| 21 | `InteracaoOrigemId` | bigint | sim |  |  |
| 22 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 23 | `CriadoEm` | datetime2(3) |  |  |  |
| 24 | `CriadoPorId` | bigint |  |  |  |
| 25 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 26 | `AlteradoPorId` | bigint | sim |  |  |
| 27 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 28 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `Tarefa.ClienteId` → `comercial.Cliente.Id` (opcional)
- `Tarefa.ContatoId` → `comercial.Contato.Id` (opcional)
- `Tarefa.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Tarefa.InteracaoConclusaoId` → `processo.Interacao.Id` (opcional)
- `Tarefa.InteracaoOrigemId` → `processo.Interacao.Id` (opcional)
- `Tarefa.ProcessoId` → `processo.Processo.Id` (opcional)
- `Tarefa.CriadaPorRegraId` → `processo.Regra.Id` (opcional)
- `Tarefa.ResultadoId` → `processo.Resultado.Id` (opcional)
- `Tarefa.TipoTarefaId` → `processo.TipoTarefa.Id` (obrigatória)
- `Tarefa.ResponsavelEquipeId` → `seguranca.Equipe.Id` (opcional)
- `Tarefa.ConcluidaPorId` → `seguranca.Usuario.Id` (opcional)
- `Tarefa.ResponsavelId` → `seguranca.Usuario.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `metadado.Preenchimento.TarefaId` (opcional)
- `processo.Interacao.TarefaId` (opcional)
- `processo.RegraExecucao.TarefaCriadaId` (opcional)
- `processo.RegraExecucao.TarefaId` (opcional)

**Índices:**

- `PK_Tarefa` (clustered, único): Id
- `IX_Tarefa_ClienteId` (nonclustered): ClienteId · filtro ([Situacao]<>'Concluida' AND [ExcluidoEm] IS NULL)
- `IX_Tarefa_ConcluidaPorId` (nonclustered): ConcluidaPorId
- `IX_Tarefa_ContatoId` (nonclustered): ContatoId
- `IX_Tarefa_CriadaPorRegraId` (nonclustered): CriadaPorRegraId
- `IX_Tarefa_EmpresaId` (nonclustered): EmpresaId
- `IX_Tarefa_InteracaoConclusaoId` (nonclustered): InteracaoConclusaoId
- `IX_Tarefa_InteracaoOrigemId` (nonclustered): InteracaoOrigemId
- `IX_Tarefa_ProcessoId` (nonclustered): ProcessoId · filtro ([ExcluidoEm] IS NULL)
- `IX_Tarefa_ResponsavelEquipeId` (nonclustered): ResponsavelEquipeId
- `IX_Tarefa_ResponsavelId_AgendadaPara` (nonclustered): ResponsavelId,AgendadaPara inclui Assunto,Prioridade,ProcessoId,ClienteId · filtro (([Situacao] IN ('Pendente', 'EmAndamento')) AND [ExcluidoEm] IS NULL)
- `IX_Tarefa_ResultadoId` (nonclustered): ResultadoId
- `IX_Tarefa_TipoTarefaId` (nonclustered): TipoTarefaId
- `UX_Tarefa_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_Tarefa_Conclusao`: ([Situacao]<>'Concluida' OR [ConcluidaEm] IS NOT NULL AND [ConcluidaPorId] IS NOT NULL AND [ResultadoId] IS NOT NULL)
- `CK_Tarefa_OrigemAtribuicao`: ([OrigemAtribuicao]='Rodizio' OR [OrigemAtribuicao]='Carteira' OR [OrigemAtribuicao]='Hierarquia' OR [OrigemAtribuicao]='Manual' OR [OrigemAtribuicao]='Regra')
- `CK_Tarefa_Prioridade`: ([Prioridade]>=(1) AND [Prioridade]<=(5))
- `CK_Tarefa_Situacao`: ([Situacao]='Reatribuida' OR [Situacao]='Cancelada' OR [Situacao]='Concluida' OR [Situacao]='EmAndamento' OR [Situacao]='Pendente')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·14 | Aplicacao 2·1·3 | Repositorio 2·1·5 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 5·2·16 | Integracao 0·0·6 | Testes 2·2·2 | Scripts 3·1·4
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs (2)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (3)`, `src/Tracbel.Crm.Carga/Program.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTarefas.cs (2)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Persistencia/FronteiraDeEmpresaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.TipoProcesso`

- **Entidade EF:** `TipoProcesso` · **DbSet:** `TiposDeProcesso` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(40) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `LinhaDeNegocioId` | int | sim |  |  |
| 5 | `Versao` | int |  |  |  |
| 6 | `EstaAtivo` | bit |  |  |  |

**Referencia (FK de saída):**

- `TipoProcesso.LinhaDeNegocioId` → `organizacao.LinhaDeNegocio.Id` (opcional)

**Dependem dela (FK de entrada):**

- `metadado.Formulario.TipoProcessoId` (opcional)
- `processo.Fase.TipoProcessoId`
- `processo.Processo.TipoProcessoId`
- `processo.Regra.TipoProcessoId` (opcional)
- `processo.TipoTarefa.TipoProcessoId` (opcional)

**Índices:**

- `PK_TipoProcesso` (clustered, único): Id
- `IX_TipoProcesso_LinhaDeNegocioId` (nonclustered): LinhaDeNegocioId
- `UX_TipoProcesso_Codigo_Versao` (nonclustered, único): Codigo,Versao

**Restrições CHECK:**

- `CK_TipoProcesso_Versao`: ([Versao]>=(1))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 3·1·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 3·2·6 | Integracao 0·0·0 | Testes 1·1·1 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (2)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeProcessos.cs (3)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `processo.TipoTarefa`

- **Entidade EF:** `TipoTarefa` · **DbSet:** `TiposDeTarefa` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(40) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `TipoProcessoId` | int | sim |  |  |
| 5 | `Categoria` | varchar(20) |  |  |  |
| 6 | `PrazoDiasUteis` | smallint |  |  |  |
| 7 | `EhAprovacao` | bit |  |  |  |
| 8 | `ExigeGeorreferencia` | bit |  |  |  |
| 9 | `ContaParaCobertura` | bit |  |  |  |
| 10 | `FormularioId` | int | sim |  |  |
| 11 | `Cor` | varchar(7) | sim |  |  |
| 12 | `EstaAtivo` | bit |  |  |  |
| 13 | `UltimoUsoEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `TipoTarefa.FormularioId` → `metadado.Formulario.Id` (opcional)
- `TipoTarefa.TipoProcessoId` → `processo.TipoProcesso.Id` (opcional)

**Dependem dela (FK de entrada):**

- `processo.Interacao.TipoTarefaId`
- `processo.Regra.EfeitoTipoTarefaId` (opcional)
- `processo.Regra.TipoTarefaId` (opcional)
- `processo.Resultado.TipoTarefaId`
- `processo.Tarefa.TipoTarefaId`

**Índices:**

- `PK_TipoTarefa` (clustered, único): Id
- `IX_TipoTarefa_FormularioId` (nonclustered): FormularioId
- `IX_TipoTarefa_TipoProcessoId` (nonclustered): TipoProcessoId
- `UX_TipoTarefa_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_TipoTarefa_Categoria`: ([Categoria]='Interna' OR [Categoria]='Remota' OR [Categoria]='Email' OR [Categoria]='WhatsApp' OR [Categoria]='Ligacao' OR [Categoria]='Visita')
- `CK_TipoTarefa_Cor`: ([Cor] IS NULL OR [Cor] like '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]' AND len([Cor])=(7))
- `CK_TipoTarefa_Prazo`: ([PrazoDiasUteis]>=(0))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 5·3·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 5·2·7 | Integracao 0·0·0 | Testes 2·2·2 | Scripts 4·2·4
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (4)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresExecutivos.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeInteracoes.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTarefas.cs (2)`, `tests/Tracbel.Crm.Api.Testes/EndpointsDeRelacionamentoTestes.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`, `scripts/coleta/extrair-pre-preenchimento.ps1 (1)`

## `processo.VendaPerdida`

- **Entidade EF:** `VendaPerdida` · **DbSet:** `VendasPerdidas` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/ProcessoConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260906122942_VendaPerdida`
- **Migrações que alteraram:** `20260910113323_ParticipacaoNaNegociacaoDeixaDeSerBooleanoAnulavel (AddCheckConstraint, DropColumn, Sql)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `ProcessoId` | bigint | sim |  |  |
| 4 | `ClienteId` | bigint | sim |  |  |
| 5 | `RegistradaEm` | datetime2(3) |  |  |  |
| 6 | `OcorridaEm` | date | sim |  |  |
| 7 | `MotivoDePerdaId` | int |  |  |  |
| 8 | `TipoDeEquipamentoId` | int | sim |  |  |
| 9 | `ConcorrenteId` | int | sim |  |  |
| 10 | `RevendaDoConcorrenteId` | int | sim |  |  |
| 11 | `ModeloDoConcorrente` | nvarchar(120) | sim |  |  |
| 12 | `ModeloOfertado` | nvarchar(120) | sim |  |  |
| 13 | `Quantidade` | int |  |  |  |
| 14 | `PrecoDoConcorrente` | decimal(18,2) | sim |  |  |
| 15 | `PrecoOfertado` | decimal(18,2) | sim |  |  |
| 17 | `RegistradaPor` | nvarchar(60) | sim |  |  |
| 18 | `CatalogoDaRevendaId` | int |  |  | ((10)) |
| 19 | `CatalogoDoConcorrenteId` | int |  |  | ((8)) |
| 20 | `CatalogoDoTipoDeEquipamentoId` | int |  |  | ((9)) |
| 21 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 22 | `CriadoEm` | datetime2(3) |  |  |  |
| 23 | `CriadoPorId` | bigint |  |  |  |
| 24 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 25 | `AlteradoPorId` | bigint | sim |  |  |
| 26 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 27 | `Versao` | timestamp | sim |  |  |
| 28 | `Participacao` | varchar(16) |  |  | ('NaoInformado') |

**Referencia (FK de saída):**

- `VendaPerdida.ClienteId` → `comercial.Cliente.Id` (opcional)
- `VendaPerdida.CatalogoDaRevendaId,RevendaDoConcorrenteId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `VendaPerdida.CatalogoDoConcorrenteId,ConcorrenteId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `VendaPerdida.CatalogoDoTipoDeEquipamentoId,TipoDeEquipamentoId` → `metadado.CatalogoItem.CatalogoId,Id` (opcional)
- `VendaPerdida.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `VendaPerdida.MotivoDePerdaId` → `processo.MotivoDePerda.Id` (obrigatória)
- `VendaPerdida.ProcessoId` → `processo.Processo.Id` (opcional)

**Índices:**

- `PK_VendaPerdida` (clustered, único): Id
- `IX_VendaPerdida_CatalogoDaRevendaId_RevendaDoConcorrenteId` (nonclustered): CatalogoDaRevendaId,RevendaDoConcorrenteId
- `IX_VendaPerdida_CatalogoDoConcorrenteId_ConcorrenteId` (nonclustered): CatalogoDoConcorrenteId,ConcorrenteId
- `IX_VendaPerdida_CatalogoDoTipoDeEquipamentoId_TipoDeEquipamentoId` (nonclustered): CatalogoDoTipoDeEquipamentoId,TipoDeEquipamentoId
- `IX_VendaPerdida_ClienteId` (nonclustered): ClienteId
- `IX_VendaPerdida_ConcorrenteId` (nonclustered): ConcorrenteId
- `IX_VendaPerdida_EmpresaId_RegistradaEm` (nonclustered): EmpresaId,RegistradaEm · filtro ([ExcluidoEm] IS NULL)
- `IX_VendaPerdida_MotivoDePerdaId` (nonclustered): MotivoDePerdaId
- `IX_VendaPerdida_ProcessoId` (nonclustered): ProcessoId
- `UX_VendaPerdida_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_VendaPerdida_CatalogoDaRevendaId`: ([CatalogoDaRevendaId]=(10))
- `CK_VendaPerdida_CatalogoDoConcorrenteId`: ([CatalogoDoConcorrenteId]=(8))
- `CK_VendaPerdida_CatalogoDoTipoDeEquipamentoId`: ([CatalogoDoTipoDeEquipamentoId]=(9))
- `CK_VendaPerdida_Participacao`: ([Participacao]='Nao' OR [Participacao]='Sim' OR [Participacao]='NaoInformado')
- `CK_VendaPerdida_Precos`: (([PrecoDoConcorrente] IS NULL OR [PrecoDoConcorrente]>(0)) AND ([PrecoOfertado] IS NULL OR [PrecoOfertado]>(0)))
- `CK_VendaPerdida_Quantidade`: ([Quantidade]>=(1))

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 3·1·6 | Aplicacao 2·2·2 | Repositorio 5·3·1 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 1·1·6 | Integracao 0·0·1 | Testes 3·3·5 | Scripts 3·1·3
- Arquivos com acesso: `src/Tracbel.Crm.Aplicacao/Relacionamento/ConsultasDeRelacionamento.cs (1)`, `src/Tracbel.Crm.Aplicacao/Relacionamento/ObterIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.VendaPerdida.cs (1)`, `src/Tracbel.Crm.Dominio/Metadado/CatalogosDeSistema.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresExecutivos.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeVendasPerdidas.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDoPainelDoCen.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresExecutivosTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/EsquemaENomenclaturaTestes.cs (1)`, `tests/Tracbel.Crm.Arquitetura.Testes/Banco/MigracaoNoContainerTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `relatorio.Fonte`

- **Entidade EF:** `Fonte` · **DbSet:** `FontesDeRelatorio` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/RelatorioConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(60) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Descricao` | nvarchar(400) | sim |  |  |
| 5 | `NomeDaVisao` | varchar(120) |  |  |  |
| 6 | `EntidadeRaiz` | varchar(40) |  |  |  |
| 7 | `SistemaId` | int | sim |  |  |
| 8 | `EhMaterializada` | bit |  |  |  |
| 9 | `AtualizadaEm` | datetime2(3) | sim |  |  |
| 10 | `EstaAtiva` | bit |  |  |  |

**Referencia (FK de saída):**

- `Fonte.SistemaId` → `integracao.Sistema.Id` (opcional)

**Dependem dela (FK de entrada):**

- `relatorio.FonteCampo.FonteId`
- `relatorio.Relatorio.FonteId`

**Índices:**

- `PK_Fonte` (clustered, único): Id
- `IX_Fonte_SistemaId` (nonclustered): SistemaId
- `UX_Fonte_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_Fonte_EntidadeRaiz`: (([EntidadeRaiz]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([EntidadeRaiz]) collate Latin1_General_BIN2='Vinculo' OR ([EntidadeRaiz]) collate Latin1_General_BIN2='VendaPerdida' OR ([EntidadeRaiz]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([EntidadeRaiz]) collate Latin1_ …
- `CK_Fonte_Materializada`: ([EhMaterializada]=(0) OR [AtualizadaEm] IS NOT NULL)

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·11 | Aplicacao 0·0·0 | Repositorio 0·0·4 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·1 | Integracao 0·0·1 | Testes 1·1·1 | Scripts 1·1·7
- Arquivos com acesso: `tests/Tracbel.Crm.Arquitetura.Testes/Banco/DominioDeEntidadeTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `relatorio.FonteCampo`

- **Entidade EF:** `FonteCampo` · **DbSet:** `CamposDeFonte` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/RelatorioConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (AddCheckConstraint)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `FonteId` | int |  |  |  |
| 3 | `Campo` | varchar(60) |  |  |  |
| 4 | `Rotulo` | nvarchar(80) |  |  |  |
| 5 | `TipoDeDado` | varchar(20) |  |  |  |
| 6 | `PermiteAgrupar` | bit |  |  |  |
| 7 | `PermiteFiltrar` | bit |  |  |  |
| 8 | `PermiteSomar` | bit |  |  |  |
| 9 | `Ordem` | smallint |  |  |  |

**Referencia (FK de saída):**

- `FonteCampo.FonteId` → `relatorio.Fonte.Id` (obrigatória)

**Índices:**

- `PK_FonteCampo` (clustered, único): Id
- `UX_FonteCampo_Fonte_Campo` (nonclustered, único): FonteId,Campo

**Restrições CHECK:**

- `CK_FonteCampo_Campo`: (([Campo]) collate Latin1_General_BIN2 like '[A-Z]%' AND NOT ([Campo]) collate Latin1_General_BIN2 like '%[^A-Za-z0-9]%')
- `CK_FonteCampo_TipoDeDado`: ([TipoDeDado]='Percentual' OR [TipoDeDado]='Moeda' OR [TipoDeDado]='Booleano' OR [TipoDeDado]='Data' OR [TipoDeDado]='Numero' OR [TipoDeDado]='Texto')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 2·1·2 | Scripts 1·1·1
- Arquivos com acesso: `tests/Tracbel.Crm.Arquitetura.Testes/Banco/DominioDeEntidadeTestes.cs (2)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `relatorio.Relatorio`

- **Entidade EF:** `Relatorio` · **DbSet:** `Relatorios` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/RelatorioConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `FonteId` | int |  |  |  |
| 4 | `Nome` | nvarchar(120) |  |  |  |
| 5 | `Definicao` | nvarchar(max) |  |  |  |
| 6 | `Visibilidade` | varchar(20) |  |  |  |
| 7 | `ProprietarioId` | bigint |  |  |  |
| 8 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 9 | `CriadoEm` | datetime2(3) |  |  |  |
| 10 | `CriadoPorId` | bigint |  |  |  |
| 11 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 12 | `AlteradoPorId` | bigint | sim |  |  |
| 13 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 14 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `Relatorio.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Relatorio.FonteId` → `relatorio.Fonte.Id` (obrigatória)
- `Relatorio.ProprietarioId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_Relatorio` (clustered, único): Id
- `IX_Relatorio_EmpresaId_Visibilidade` (nonclustered): EmpresaId,Visibilidade · filtro ([ExcluidoEm] IS NULL)
- `IX_Relatorio_FonteId` (nonclustered): FonteId
- `IX_Relatorio_ProprietarioId_Nome` (nonclustered): ProprietarioId,Nome · filtro ([ExcluidoEm] IS NULL)
- `UX_Relatorio_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_Relatorio_DefinicaoJson`: (isjson([Definicao])=(1))
- `CK_Relatorio_Visibilidade`: ([Visibilidade]='Empresa' OR [Visibilidade]='Equipe' OR [Visibilidade]='Privado')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·4 | Integracao 0·0·0 | Testes 0·0·1 | Scripts 1·1·1
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `seguranca.CompartilhamentoDeRegistro`

- **Entidade EF:** `CompartilhamentoDeRegistro` · **DbSet:** `CompartilhamentosDeRegistro` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `Entidade` | varchar(40) |  |  |  |
| 4 | `RegistroId` | bigint |  |  |  |
| 5 | `UsuarioId` | bigint | sim |  |  |
| 6 | `EquipeId` | bigint | sim |  |  |
| 7 | `Nivel` | varchar(10) |  |  |  |
| 8 | `Motivo` | varchar(20) |  |  |  |
| 9 | `RegraOrigemId` | int | sim |  |  |
| 10 | `ExpiraEm` | datetime2(3) | sim |  |  |
| 11 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 12 | `CriadoEm` | datetime2(3) |  |  |  |
| 13 | `CriadoPorId` | bigint |  |  |  |
| 14 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 15 | `AlteradoPorId` | bigint | sim |  |  |
| 16 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 17 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `CompartilhamentoDeRegistro.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `CompartilhamentoDeRegistro.EquipeId` → `seguranca.Equipe.Id` (opcional)
- `CompartilhamentoDeRegistro.UsuarioId` → `seguranca.Usuario.Id` (opcional)

**Índices:**

- `PK_CompartilhamentoDeRegistro` (clustered, único): Id
- `IX_CompartilhamentoDeRegistro_EmpresaId` (nonclustered): EmpresaId
- `IX_CompartilhamentoDeRegistro_Entidade_RegistroId` (nonclustered): Entidade,RegistroId · filtro ([ExcluidoEm] IS NULL)
- `IX_CompartilhamentoDeRegistro_EquipeId_Entidade` (nonclustered): EquipeId,Entidade · filtro ([ExcluidoEm] IS NULL)
- `IX_CompartilhamentoDeRegistro_UsuarioId_Entidade` (nonclustered): UsuarioId,Entidade · filtro ([ExcluidoEm] IS NULL)
- `UX_CompartilhamentoDeRegistro_ChavePublica` (nonclustered, único): ChavePublica

**Restrições CHECK:**

- `CK_CompartilhamentoDeRegistro_Entidade`: (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Latin1_General_BIN2='Usuari …
- `CK_CompartilhamentoDeRegistro_Motivo`: ([Motivo]='Delegacao' OR [Motivo]='Hierarquia' OR [Motivo]='Equipe' OR [Motivo]='Regra' OR [Motivo]='Manual')
- `CK_CompartilhamentoDeRegistro_Nivel`: ([Nivel]='Edicao' OR [Nivel]='Leitura')
- `CK_CompartilhamentoDeRegistro_UmSujeito`: ([UsuarioId] IS NOT NULL AND [EquipeId] IS NULL OR [UsuarioId] IS NULL AND [EquipeId] IS NOT NULL)

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·1 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `seguranca.ConjuntoDePermissao`

- **Entidade EF:** `ConjuntoPermissao` · **DbSet:** `ConjuntosPermissao` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaEWorkflowConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(60) |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Descricao` | nvarchar(400) | sim |  |  |
| 5 | `EstaAtivo` | bit |  |  |  |

**Dependem dela (FK de entrada):**

- `seguranca.ConjuntoDePermissaoItem.ConjuntoPermissaoId`
- `seguranca.UsuarioConjuntoDePermissao.ConjuntoPermissaoId`

**Índices:**

- `PK_ConjuntoDePermissao` (clustered, único): Id
- `UX_ConjuntoDePermissao_Codigo` (nonclustered, único): Codigo

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 1·1·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 2·2·7 | Scripts 5·2·0
- Arquivos com acesso: `src/Tracbel.Crm.Infraestrutura/Identidade/EscopoDeAcesso.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Seguranca/ConcessaoExplicitaNaPonteProvisoriaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/banco/validacao/perfis-de-teste-da-visao-da-empresa.sql (4)`

## `seguranca.ConjuntoDePermissaoItem`

- **Entidade EF:** `ItemConjuntoPermissao` · **DbSet:** — (sem DbSet) · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaEWorkflowConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `ConjuntoPermissaoId` | int |  |  |  |
| 3 | `CodigoPermissao` | varchar(80) |  |  |  |
| 4 | `Profundidade` | varchar(20) |  |  |  |

**Referencia (FK de saída):**

- `ConjuntoDePermissaoItem.ConjuntoPermissaoId` → `seguranca.ConjuntoDePermissao.Id` (obrigatória) · CASCADE

**Índices:**

- `PK_ConjuntoDePermissaoItem` (clustered, único): Id
- `UX_ConjuntoDePermissaoItem_Conjunto_Codigo` (nonclustered, único): ConjuntoPermissaoId,CodigoPermissao

**Restrições CHECK:**

- `CK_ConjuntoDePermissaoItem_Profundidade`: ([Profundidade]='Organizacao' OR [Profundidade]='EmpresaEAbaixo' OR [Profundidade]='Empresa' OR [Profundidade]='Equipe' OR [Profundidade]='Proprios')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·6 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 1·1·1 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 4·2·0
- Arquivos com acesso: `src/Tracbel.Crm.Infraestrutura/Identidade/EscopoDeAcesso.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/banco/validacao/perfis-de-teste-da-visao-da-empresa.sql (3)`

## `seguranca.Equipe`

- **Entidade EF:** `Equipe` · **DbSet:** `Equipes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EmpresaId` | int |  |  |  |
| 3 | `Nome` | nvarchar(120) |  |  |  |
| 4 | `Descricao` | nvarchar(400) | sim |  |  |
| 5 | `Tipo` | varchar(20) |  |  |  |
| 6 | `EstaAtiva` | bit |  |  |  |
| 7 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 8 | `CriadoEm` | datetime2(3) |  |  |  |
| 9 | `CriadoPorId` | bigint |  |  |  |
| 10 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 11 | `AlteradoPorId` | bigint | sim |  |  |
| 12 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 13 | `Versao` | timestamp | sim |  |  |

**Referencia (FK de saída):**

- `Equipe.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)

**Dependem dela (FK de entrada):**

- `comercial.Cliente.ProprietarioEquipeId` (opcional)
- `organizacao.Carteira.EquipeId` (opcional)
- `processo.Processo.ProprietarioEquipeId` (opcional)
- `processo.Tarefa.ResponsavelEquipeId` (opcional)
- `seguranca.CompartilhamentoDeRegistro.EquipeId` (opcional)
- `seguranca.EquipeMembro.EquipeId`

**Índices:**

- `PK_Equipe` (clustered, único): Id
- `UX_Equipe_ChavePublica` (nonclustered, único): ChavePublica
- `UX_Equipe_Empresa_Nome` (nonclustered, único): EmpresaId,Nome

**Restrições CHECK:**

- `CK_Equipe_Tipo`: ([Tipo]='Acesso' OR [Tipo]='Proprietaria')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·19 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·8 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `seguranca.EquipeMembro`

- **Entidade EF:** `EquipeMembro` · **DbSet:** `EquipeMembros` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `EquipeId` | bigint |  |  |  |
| 3 | `UsuarioId` | bigint |  |  |  |
| 4 | `EhLider` | bit |  |  |  |
| 5 | `EntrouEm` | datetime2(3) |  |  |  |
| 6 | `SaiuEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `EquipeMembro.EquipeId` → `seguranca.Equipe.Id` (obrigatória)
- `EquipeMembro.UsuarioId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_EquipeMembro` (clustered, único): Id
- `IX_EquipeMembro_UsuarioId` (nonclustered): UsuarioId
- `UX_EquipeMembro_Equipe_Usuario_Vigente` (nonclustered, único): EquipeId,UsuarioId · filtro ([SaiuEm] IS NULL)

**Restrições CHECK:**

- `CK_EquipeMembro_Periodo`: ([SaiuEm] IS NULL OR [SaiuEm]>=[EntrouEm])

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 3·1·3
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (3)`

## `seguranca.Permissao`

- **Entidade EF:** `Permissao` · **DbSet:** `Permissoes` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaEWorkflowConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260904191250_ChaveCompostaDeCatalogoEDominioDeEntidade (CK_Entidade)`; `20260905191622_MunicipioEAgrupamentoDeCarteira (CK_Entidade x2)`; `20260906122942_VendaPerdida (CK_Entidade x2)`; `20260906144125_FaturamentoEClasseDoCliente (CK_Entidade x2)`; `20260908141700_QuebraDeFaturamentoEParceiroSemCliente (CK_Entidade x2)`; `20260913155645_AreaDeAtuacaoResponsaveisAreaPlantadaERegraDePotencial (CK_Entidade x2)`; `20260914170927_IntegracaoDoArtLinhaDeProdutoVendaEVinculo (CK_Entidade x2)`; `20260914194518_SincronizacaoComoServicoECatalogoDeClassificacao (CK_Entidade x2)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | int |  | sim |  |
| 2 | `Codigo` | varchar(80) |  |  |  |
| 3 | `Entidade` | varchar(40) |  |  |  |
| 4 | `Verbo` | varchar(20) |  |  |  |
| 5 | `Descricao` | nvarchar(200) |  |  |  |

**Índices:**

- `PK_Permissao` (clustered, único): Id
- `UX_Permissao_Codigo` (nonclustered, único): Codigo

**Restrições CHECK:**

- `CK_Permissao_Entidade`: (([Entidade]) collate Latin1_General_BIN2='VinculoDeClienteComEquipamento' OR ([Entidade]) collate Latin1_General_BIN2='Vinculo' OR ([Entidade]) collate Latin1_General_BIN2='VendaPerdida' OR ([Entidade]) collate Latin1_General_BIN2='VendaDeMaquina' OR ([Entidade]) collate Latin1_General_BIN2='Usuari …
- `CK_Permissao_Verbo`: ([Verbo]='Compartilhar' OR [Verbo]='Atribuir' OR [Verbo]='Excluir' OR [Verbo]='Editar' OR [Verbo]='Criar' OR [Verbo]='Ler')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·3 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 0·0·0 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 0·0·0 | Scripts 1·1·1
- Arquivos com acesso: `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`

## `seguranca.Usuario`

- **Entidade EF:** `Usuario` · **DbSet:** `Usuarios` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaConfiguracao.cs`
- **Linhas:** 1 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`
- **Migrações que alteraram:** `20260906134331_NaturezaDeCarteiraEUsuario (AddCheckConstraint)`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `IdentidadeExterna` | uniqueidentifier |  |  |  |
| 3 | `NomePrincipal` | nvarchar(200) |  |  |  |
| 4 | `NomeCompleto` | nvarchar(200) |  |  |  |
| 5 | `NomeExibicao` | nvarchar(80) |  |  |  |
| 6 | `Email` | nvarchar(200) |  |  |  |
| 7 | `EmpresaId` | int |  |  |  |
| 8 | `GestorId` | bigint | sim |  |  |
| 9 | `Papel` | varchar(40) | sim |  |  |
| 10 | `EstaAtivo` | bit |  |  |  |
| 11 | `DesativadoEm` | datetime2(3) | sim |  |  |
| 12 | `UltimoLoginEm` | datetime2(3) | sim |  |  |
| 13 | `ChavePublica` | uniqueidentifier |  |  | (newid()) |
| 14 | `CriadoEm` | datetime2(3) |  |  |  |
| 15 | `CriadoPorId` | bigint |  |  |  |
| 16 | `AlteradoEm` | datetime2(3) | sim |  |  |
| 17 | `AlteradoPorId` | bigint | sim |  |  |
| 18 | `ExcluidoEm` | datetime2(3) | sim |  |  |
| 19 | `Versao` | timestamp | sim |  |  |
| 20 | `Natureza` | varchar(20) |  |  | ('Pessoa') |

**Referencia (FK de saída):**

- `Usuario.EmpresaId` → `organizacao.Empresa.Id` (obrigatória)
- `Usuario.GestorId` → `seguranca.Usuario.Id` (opcional)

**Dependem dela (FK de entrada):**

- `auditoria.AlteracaoDeCampo.AlteradoPorId`
- `auditoria.EventoDeAcesso.UsuarioId` (opcional)
- `comercial.Cliente.ProprietarioId`
- `comercial.Contato.ProprietarioId`
- `comercial.Lead.ProprietarioId`
- `comercial.Lead.QualificadoPorId` (opcional)
- `documento.Vinculo.VinculadoPorId`
- `frota.LeituraDeHorimetro.RegistradoPorId` (opcional)
- `integracao.CorrespondenciaDaOrigem.RevisadaPorId` (opcional)
- `integracao.MensagemDescartada.TratadaPorId` (opcional)
- `metadado.Preenchimento.PreenchidoPorId`
- `organizacao.AreaPlantadaNoMunicipio.ImportadoPorId`
- `organizacao.Carteira.ResponsavelId`
- `organizacao.Carteira.SupervisorId` (opcional)
- `organizacao.HierarquiaComercial.DescendenteId`
- `organizacao.HierarquiaComercial.AncestralId`
- `organizacao.Meta.UsuarioId` (opcional)
- `organizacao.MunicipioDaAreaDeAtuacao.ImportadoPorId`
- `organizacao.ResponsavelPeloMunicipio.ImportadoPorId`
- `organizacao.ResponsavelPeloMunicipio.UsuarioId` (opcional)
- `processo.Interacao.RegistradoPorId`
- `processo.InteracaoParticipante.UsuarioId` (opcional)
- `processo.PassagemDeFase.EntrouPorId`
- `processo.Processo.ProprietarioId`
- `processo.RegraExecucao.DestinatarioResolvidoId` (opcional)
- `processo.Tarefa.ConcluidaPorId` (opcional)
- `processo.Tarefa.ResponsavelId`
- `relatorio.Relatorio.ProprietarioId`
- `seguranca.CompartilhamentoDeRegistro.UsuarioId` (opcional)
- `seguranca.EquipeMembro.UsuarioId`
- `seguranca.Usuario.GestorId` (opcional)
- `seguranca.UsuarioConjuntoDePermissao.UsuarioId`

**Índices:**

- `PK_Usuario` (clustered, único): Id
- `IX_Usuario_EmpresaId` (nonclustered): EmpresaId
- `IX_Usuario_GestorId` (nonclustered): GestorId · filtro ([EstaAtivo]=(1) AND [ExcluidoEm] IS NULL)
- `UX_Usuario_ChavePublica` (nonclustered, único): ChavePublica
- `UX_Usuario_IdentidadeExterna` (nonclustered, único): IdentidadeExterna
- `UX_Usuario_NomePrincipal` (nonclustered, único): NomePrincipal

**Restrições CHECK:**

- `CK_Usuario_Desativacao`: ([EstaAtivo]=(1) OR [DesativadoEm] IS NOT NULL)
- `CK_Usuario_Natureza`: ([Natureza]='Teste' OR [Natureza]='Fornecedor' OR [Natureza]='Sistema' OR [Natureza]='Departamento' OR [Natureza]='Pessoa')

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 11·7·0 | InfraOutros 5·2·13 | Api 0·0·4 | Carga 8·4·18 | Integracao 0·0·11 | Testes 15·10·32 | Scripts 20·4·28
- Arquivos com acesso: `src/Tracbel.Crm.Carga/CargaDeProcessoDoVortice.cs (4)`, `src/Tracbel.Crm.Carga/CargaDeTerritorio.cs (1)`, `src/Tracbel.Crm.Carga/PreparacaoDaCarga.cs (2)`, `src/Tracbel.Crm.Carga/Program.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Identidade/ResolvedorDeContextoDoEntraId.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Identidade/ResolvedorDeContextoProvisorio.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeCarteiras.cs (3)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeIndicadoresTerritoriais.cs (2)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeInteracoes.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeProcessos.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTarefas.cs (1)`, `src/Tracbel.Crm.Infraestrutura/Persistencia/Repositorios/RepositorioDeTerritorio.cs (1)` e mais 15

## `seguranca.UsuarioConjuntoDePermissao`

- **Entidade EF:** `UsuarioConjuntoPermissao` · **DbSet:** `ConcessoesPermissao` · **Configuração:** `src/Tracbel.Crm.Infraestrutura/Persistencia/Configuracoes/SegurancaEWorkflowConfiguracao.cs`
- **Linhas:** 0 · **Chave primária:** `Id` · **Identity:** Id
- **Migração que criou:** `20260904120040_ModeloInicial`

| # | Coluna | Tipo | Nulo | Identity | Default |
|---:|---|---|:-:|:-:|---|
| 1 | `Id` | bigint |  | sim |  |
| 2 | `UsuarioId` | bigint |  |  |  |
| 3 | `ConjuntoPermissaoId` | int |  |  |  |
| 4 | `ConcedidoEm` | datetime2(3) |  |  |  |
| 5 | `ConcedidoPorId` | bigint |  |  |  |
| 6 | `ExpiraEm` | datetime2(3) | sim |  |  |

**Referencia (FK de saída):**

- `UsuarioConjuntoDePermissao.ConjuntoPermissaoId` → `seguranca.ConjuntoDePermissao.Id` (obrigatória)
- `UsuarioConjuntoDePermissao.UsuarioId` → `seguranca.Usuario.Id` (obrigatória)

**Índices:**

- `PK_UsuarioConjuntoDePermissao` (clustered, único): Id
- `IX_UsuarioConjuntoDePermissao_ConjuntoPermissaoId` (nonclustered): ConjuntoPermissaoId
- `UX_UsuarioConjuntoDePermissao_Usuario_Conjunto` (nonclustered, único): UsuarioId,ConjuntoPermissaoId

**Uso no código** (acesso · arquivos com acesso · menção ao tipo):

- Dominio 0·0·4 | Aplicacao 0·0·0 | Repositorio 0·0·0 | InfraOutros 1·1·4 | Api 0·0·0 | Carga 0·0·0 | Integracao 0·0·0 | Testes 2·2·8 | Scripts 5·2·0
- Arquivos com acesso: `src/Tracbel.Crm.Infraestrutura/Identidade/EscopoDeAcesso.cs (1)`, `tests/Tracbel.Crm.Api.Testes/IndicadoresTerritoriaisTestes.cs (1)`, `tests/Tracbel.Crm.Aplicacao.Testes/Seguranca/ConcessaoExplicitaNaPonteProvisoriaTestes.cs (1)`, `scripts/banco/sanitizacao/sanitizar-dados-2026-09-15.sql (1)`, `scripts/banco/validacao/perfis-de-teste-da-visao-da-empresa.sql (4)`

