import { rmSync } from 'node:fs'
import { resolve } from 'node:path'
import react from '@vitejs/plugin-react'
import type { Plugin } from 'vite'
// `vitest/config` é o mesmo `defineConfig` do Vite, com o bloco `test` tipado.
import { defineConfig } from 'vitest/config'

/**
 * O JSON FICTÍCIO DO PROTÓTIPO NÃO VAI PARA O PACOTE (issue 191).
 *
 * `public/dados` guarda o que as telas do protótipo leem — o cliente 84391, o trator 1RW7250…, a carteira do CEN de
 * mentira. Essas telas só existem no `npm run dev` (ver `rotas.tsx`), mas o Vite copia a pasta `public` inteira para o
 * `dist`, e os arquivos seguiriam publicados no servidor, a um endereço de distância. O desenvolvimento continua
 * servindo a pasta normalmente; só o `build` a apaga do resultado. `npm run visual:conferir-pacote` confere.
 */
function semDadosDoPrototipoNoPacote(): Plugin {
  let saida = ''
  return {
    name: 'sem-dados-do-prototipo-no-pacote',
    apply: 'build',
    configResolved(config) {
      saida = resolve(config.root, config.build.outDir)
    },
    closeBundle() {
      rmSync(resolve(saida, 'dados'), { recursive: true, force: true })
    },
  }
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), semDadosDoPrototipoNoPacote()],

  // OS TESTES DE COMPONENTE (issue 031). O arquivo termina em `.teste.tsx`, como
  // os testes do backend terminam em `Testes.cs`: o nome diz em português o que
  // é. `globals: false` mantém `describe`/`it`/`expect` importados no arquivo —
  // nada aparece por mágica —, e por isso a limpeza da testing-library é
  // registrada à mão em `src/testes/configuracao.ts`.
  test: {
    environment: 'jsdom',
    globals: false,
    include: ['src/**/*.teste.{ts,tsx}'],
    setupFiles: ['./src/testes/configuracao.ts'],
  },
  server: {
    // A porta que `scripts/prototipo/comparar-telas.mjs` procura por padrão.
    // Declarada aqui para `npm run dev` e a comparação visual não dependerem de
    // alguém lembrar de passar `--port`.
    port: 5199,

    // O REDIRECIONAMENTO DA API, e por que ele existe: a API não publica CORS
    // (`Program.cs` não registra nada de CORS), e o navegador recusaria uma
    // chamada de http://localhost:5199 para http://localhost:5145. Redirecionar
    // aqui resolve sem ligar CORS na API só por causa do ambiente de
    // desenvolvimento — em produção o front e a API saem atrás do mesmo host, e
    // `/api` continua sendo o caminho certo.
    //
    // Outro endereço se declara em `VITE_API_URL` (ver `dados/api/http.ts`).
    proxy: {
      '/api': {
        target: 'http://localhost:5145',
        changeOrigin: true,
      },
      // A SESSÃO MORA FORA DE `/api`, e precisa do mesmo desvio. `/auth/eu` é o
      // que a tela pergunta antes de decidir entre o login e a aplicação; sem
      // este redirecionamento ele cairia no próprio Vite e voltaria o index.html.
      '/auth': {
        target: 'http://localhost:5145',
        changeOrigin: true,
      },
    },
  },
})
