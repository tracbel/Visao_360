import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
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
    },
  },
})
