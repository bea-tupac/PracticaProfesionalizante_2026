# Configuración y Build

## Archivos de Configuración

### `package.json`
```json
{
  "name": "lag-comunity-frontend",
  "author": "lag-comunity",
  "license": "MIT",
  "version": "0.1.2",
  "private": true,
  "type": "module",
  "engines": { "node": "^20.19.0 || >=22.12.0" },
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview",
    "build-only": "vite build",
    "lint": "eslint . --fix",
    "format": "prettier --write src/"
  },
  "dependencies": {
    "@element-plus/icons-vue": "^1.1.4",
    "element-plus": "^2.11.1",
    "pinia": "^3.0.3",
    "vue": "^3.5.18",
    "vue-router": "^4.5.1"
  },
  "devDependencies": {
    "@vitejs/plugin-vue": "^6.0.1",
    "@vue/eslint-config-prettier": "^10.2.0",
    "@vue/eslint-config-typescript": "^14.6.0",
    "@vue/tsconfig": "^0.7.0",
    "eslint": "^9.31.0",
    "eslint-plugin-vue": "~10.3.0",
    "jiti": "^2.4.2",
    "npm-run-all2": "^8.0.4",
    "prettier": "3.6.2",
    "vite": "^7.0.6",
    "vite-plugin-vue-devtools": "^8.0.0",
    "vue-tsc": "^3.0.4"
  }
}
```

### `vite.config.js`
```javascript
import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

export default defineConfig({
  plugins: [vue(), vueDevTools()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  }
})
```
- **Alias `@`** → `src/` (usado en imports: `@/components/`, `@/composables/`)
- **Vue DevTools** habilitado en desarrollo

### `eslint.config.js`
ESLint 9 flat config con:
- `plugin:vue/vue3-recommended`
- `plugin:@typescript-eslint/recommended`
- `plugin:prettier/recommended`
- Reglas Vue + JavaScript + Prettier integradas

### `.prettierrc.json`
```json
{ "singleQuote": true, "semi": true, "tabWidth": 2, "printWidth": 100 }
```

### `.env` / `.env.example`
```env
VITE_API_URL=http://localhost:5127
```
- **`.env`** - Valores locales (no commitear secretos reales)
- **`.env.example`** - Plantilla para el equipo

---

## Variables de Entorno

| Variable | Descripción | Ejemplo Desarrollo | Ejemplo Producción |
|----------|-------------|-------------------|-------------------|
| `VITE_API_URL` | Base URL del backend | `http://localhost:5127` | `https://api.instituto.edu.ar` |
| `BASE_URL` | Base path del frontend (Vite) | `/` | `/app/` |

**Acceso en código:** `import.meta.env.VITE_API_URL`

> **Importante:** Solo variables con prefijo `VITE_` son expuestas al cliente.

---

## Scripts Disponibles

| Comando | Descripción |
|---------|-------------|
| `npm run dev` | Inicia servidor desarrollo (Vite) en `http://localhost:5176` |
| `npm run build` | Type-check + build producción en `dist/` |
| `npm run build-only` | Solo build (sin type-check) |
| `npm run preview` | Sirve build producción localmente para test |
| `npm run type-check` | `vue-tsc --build` (verifica tipos sin emitir JS) |
| `npm run lint` | ESLint con auto-fix |
| `npm run format` | Prettier formatea `src/**` |

---

## Estructura de Build Producción

```
dist/
├── index.html
├── assets/
│   ├── index-[hash].js      # Bundle principal
│   ├── index-[hash].css     # Estilos extraídos
│   ├── vendor-[hash].js     # Vendor chunk (vue, router, pinia, element-plus)
│   └── [assets]             # Imágenes, fuentes, etc.
└── .vite/                   # Manifest para SSR si aplica
```

- **Code splitting automático** por rutas (lazy loading)
- **Hash en nombres** para cache busting
- **Tree shaking** habilitado por defecto (ESM)

---

## Requisitos de Sistema

| Herramienta | Versión Mínima |
|-------------|----------------|
| Node.js | 20.19.0 / 22.12.0 |
| npm | 10.x (incluido en Node) |

---

## Instalación y Desarrollo

```bash
# 1. Instalar dependencias
npm install

# 2. Configurar variables de entorno
cp .env.example .env
# Editar .env con VITE_API_URL correcto

# 3. Desarrollo
npm run dev

# 4. Verificar tipos + lint antes de commit
npm run type-check
npm run lint
```