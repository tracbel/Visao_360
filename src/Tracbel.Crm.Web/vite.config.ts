import react from '@vitejs/plugin-react'
// `vitest/config` é o mesmo `defineConfig` do Vite, com o bloco `test` tipado.
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

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
