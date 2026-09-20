import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:5250',
        secure: false,
      },
      '/hubs': {
        target: 'https://localhost:5250',
        secure: false,
        ws: true,
      },
    },
  },
})
