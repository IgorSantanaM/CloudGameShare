import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'
import { defineConfig } from 'vite'

const catalogProxy = {
  '/api/gaming': {
    target: process.env.GAMING_SESSION_API_TARGET ?? 'http://localhost:5003',
    changeOrigin: true,
  },
  '/api/games': {
    target: process.env.CATALOG_API_TARGET ?? 'http://localhost:5001',
    changeOrigin: true,
  },
}

export default defineConfig({
  server: { proxy: catalogProxy },
  preview: { proxy: catalogProxy },
  plugins: [
    react(),
    babel({ presets: [reactCompilerPreset()] })
  ],
})
