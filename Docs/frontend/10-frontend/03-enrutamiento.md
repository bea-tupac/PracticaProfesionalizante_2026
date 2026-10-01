# Enrutamiento (Vue Router)

## Configuración Principal

**Archivo:** `src/router/index.js`

```javascript
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [ ... ]
})
```

- **Modo:** `createWebHistory` (URLs limpias sin `#`)
- **Base:** `import.meta.env.BASE_URL` (configurable via `.env`)
- **Lazy loading:** Todas las rutas usan `() => import(...)` para code-splitting

## Tabla de Rutas

| Ruta | Nombre | Componente | Meta | Props |
|------|--------|------------|------|-------|
| `/` | `home` | `HomeView` | `requiereAuth: true` | - |
| `/contacto` | `contacto` | `ContactoView` | - | - |
| `/inscripcion` | `inscripcion` | `InscripciónView` | - | - |
| `/login` | `login` | `LoginView` | `soloInvitado: true` | - |
| `/administracion` | `administracion` | `AdministradorView` | `requiereAuth: true` | - |
| `/agregaradministracion` | `agregaradministracion` | `AgregarAdministradorView` | `requiereAuth: true` | - |
| `/editaradministrador/:id` | `editaradministrador` | `EditarAdministradorView` | `requiereAuth: true` | `true` |
| `/eliminaradministrador/:id` | `eliminaradministrador` | `EliminarAdministradorView` | `requiereAuth: true` | `true` |
| `/carreras` | `carreras` | `CarreraView` | `requiereAuth: true` | - |
| `/agregarcarreras` | `agregarcarreras` | `AgregarCarreraView` | `requiereAuth: true` | - |
| `/editarcarrera/:id` | `editarcarrera` | `EditarCarreraView` | `requiereAuth: true` | `true` |
| `/eliminarcarreras/:id` | `eliminarcarreras` | `EliminarCarreraView` | `requiereAuth: true` | `true` |
| `/formularios` | `formularios` | `FormulariosView` | `requiereAuth: true` | - |
| `/agregarformulario` | `agregarformulario` | `AgregarFormularioView` | `requiereAuth: true` | - |
| `/editarformulario/:id` | `editarformulario` | `EditarFormularioView` | `requiereAuth: true` | `true` |
| `/eliminarformulario/:id` | `eliminarformulario` | `EliminarFormularioView` | `requiereAuth: true` | `true` |
| `/listados` | `listados` | `ListadoView` | `requiereAuth: true` | - |
| `* (404)` | `not-found` | `NotFoundView` | - | - |

## Metadatos de Ruta (`meta`)

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `requiereAuth` | `boolean` | Ruta protegida: requiere sesión activa |
| `soloInvitado` | `boolean` | Ruta solo para no autenticados (ej: login) |

## Navigation Guards

**Guard global:** `router.beforeEach((to) => { ... })`

### Lógica de Protección

```javascript
router.beforeEach((to) => {
  const { isAuthenticated } = useAuth()
  const autenticado = isAuthenticated()

  // 1. Ruta protegida SIN sesión → redirect a login
  if (to.meta.requiereAuth && !autenticado)
    return { name: 'login' }

  // 2. Usuario logueado intenta entrar a login → redirect a home
  if (to.meta.soloInvitado && autenticado)
    return { name: 'home' }
})
```

### Flujo de Autenticación

```
Usuario accede a ruta
       │
       ▼
┌──────────────────┐
│ to.meta.requiereAuth? │──No──→ Permitir acceso
└────────┬─────────┘
         │ Sí
         ▼
┌──────────────────┐
│ isAuthenticated()? │──No──→ Redirect: { name: 'login' }
└────────┬─────────┘
         │ Sí
         ▼
┌──────────────────┐
│ to.meta.soloInvitado? │──Sí──→ Redirect: { name: 'home' }
└────────┬─────────┘
         │ No
         ▼
     Permitir acceso
```

## Props en Rutas Dinámicas

Las rutas con `:id` usan `props: true` para recibir el parámetro como prop tipado:

```typescript
// Router
{ path: '/editaradministrador/:id', props: true }

// Componente (EditarAdministradorView.vue)
defineProps<{ id: string }>()
const idNum = Number(id)  // Conversión manual si se necesita number
```

## Variables de Entorno para Routing

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `VITE_API_URL` | Base URL del backend API | `http://localhost:5127` |
| `BASE_URL` | Base path del frontend (Vite) | `/` o `/app/` |