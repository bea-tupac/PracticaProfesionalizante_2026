# Frontend - Visión General

## Descripción General

El frontend del Instituto Tupac Amaru es una **Single Page Application (SPA)** construida con **Vue 3** (Composition API + **JavaScript**) que sirve como panel de administración para la gestión de administradores, carreras, alumnos, formularios, listados e inscripción pública.

## Stack Tecnológico (Versiones Exactas)

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **Vue** | 3.5.18 | Framework reactivo (Composition API) |
| **JavaScript** | ES2023+ | Lógica de aplicación (sin TypeScript) |
| **Vite** | 7.0.6 | Bundler y dev server |
| **Vue Router** | 4.5.1 | Enrutamiento SPA |
| **Pinia** | 3.0.3 | Estado global (configurado; auth vía `useAuth` composable + sessionStorage) |
| **Element Plus** | 2.11.1 | Biblioteca de componentes UI |
| **@element-plus/icons-vue** | 1.1.4 | Iconografía |
| **ESLint** | 9.31.0 | Linting |
| **Prettier** | 3.6.2 | Formato |

## Arquitectura General (Código Real)

```
Frontend/
├── src/
│   ├── components/       # Componentes reutilizables (NavBar, WelcomeItem, TheWelcome)
│   ├── composables/      # Lógica reactiva reutilizable (useAuth)
│   ├── router/           # Configuración de rutas y guards (index.js)
│   ├── views/            # Páginas/vistas por funcionalidad (19 vistas)
│   │   ├── administradores/   # 4 vistas: List, Add, Edit, Delete
│   │   ├── carrera/           # 4 vistas: List, Add, Edit, Delete
│   │   ├── auth/              # 1 vista: Login
│   │   ├── public/            # 6 vistas: Home, Contacto, Formularios (CRUD), Inscripción
│   │   └── Listados/          # 2 vistas: Listado, Inscripción
│   ├── assets/           # Estilos, imágenes
│   │   └── css/          # CSS Modular (base/, components/, layout/)
│   ├── App.vue           # Componente raíz
│   └── main.js           # Punto de entrada
├── index.html
├── package.json
├── vite.config.js
└── .env                  # VITE_API_URL=http://localhost:5127
```

## Principios de Diseño

1. **Composition API** + `<script setup>` en todos los componentes
2. **JavaScript moderno** (ES2023+) con JSDoc para tipado de datos
3. **Lazy loading** de rutas para optimizar bundle inicial (`() => import(...)`)
4. **Guards de navegación** para autenticación/autorización (`meta.requiereAuth`, `meta.soloInvitado`)
5. **Separación de responsabilidades**: vistas (UI) ↔ composables (lógica) ↔ API (fetch directo, sin service layer centralizado)
6. **CSS Modular**: Sin `<style>` en `.vue`, clases globales en `src/assets/css/` (BEM-like)
7. **State Management**: Pinia solo para auth store; resto estado local reactivo

## Estructura de Vistas (19 Total)

| Módulo | Vistas | Rutas |
|--------|--------|-------|
| **Auth** | LoginView | `/login` |
| **Dashboard** | HomeView | `/` |
| **Administradores** | AdministradorView, AgregarAdministradorView, EditarAdministradorView, EliminarAdministradorView | `/administracion`, `/agregaradministracion`, `/editaradministrador/:id`, `/eliminaradministrador/:id` |
| **Carreras** | CarreraView, AgregarCarreraView, EditarCarreraView, EliminarCarreraView | `/carreras`, `/agregarcarreras`, `/editarcarrera/:id`, `/eliminarcarreras/:id` |
| **Formularios** | FormulariosView, AgregarFormularioView, EditarFormularioView, EliminarFormularioView | `/formularios`, `/agregarformulario`, `/editarformulario/:id`, `/eliminarformulario/:id` |
| **Listados** | ListadoView | `/listados` |
| **Inscripción Pública** | InscripciónView | `/inscripcion` |
| **Contacto** | ContactoView | `/contacto` |
| **404** | NotFound | `/:pathMatch(.*)*` |

> **Nota**: Profesores tiene backend CRUD completo pero **sin vistas frontend**.