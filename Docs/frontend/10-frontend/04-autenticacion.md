# Autenticación y Autorización (Frontend)

## Composables: `useAuth.js`

**Archivo:** `src/composables/useAuth.js`

Gestiona todo el estado de autenticación del lado del cliente usando `sessionStorage`.

### Estado Almacenado

| Clave | Tipo | Descripción |
|-------|------|-------------|
| `auth_token` | `string` | JWT token devuelto por backend |
| `auth_admin` | `string (JSON)` | Objeto `AdminSession` serializado |

### Interfaz `AdminSession`

```typescript
interface AdminSession {
  id: number
  nombre: string
  apellido: string
  email: string
  role: string
}
```

### API del Composable

| Función | Retorno | Descripción |
|---------|---------|-------------|
| `getToken()` | `string \| null` | Obtiene JWT de sessionStorage |
| `getAdmin()` | `AdminSession \| null` | Obtiene datos del admin parseados |
| `isAuthenticated()` | `boolean` | `true` si existe token válido |
| `guardarSesion(token, admin)` | `void` | Guarda token + admin en sessionStorage |
| `cerrarSesion()` | `void` | Limpia sessionStorage |
| `authHeaders()` | `Record<string, string>` | Headers para peticiones autenticadas |

### Headers de Autenticación

```typescript
const authHeaders = (): Record<string, string> => {
  const token = getToken()
  return token
    ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
    : { 'Content-Type': 'application/json' }
}
```

- **Con token:** `Authorization: Bearer <jwt>` + `Content-Type: application/json`
- **Sin token:** Solo `Content-Type: application/json`

## Flujo de Login

```
1. Usuario ingresa credenciales en LoginView.vue
       │
       ▼
2. POST /api/auth/login → Backend valida
       │
       ▼
3. Backend retorna { token, admin: { id, nombre, apellido, email, role } }
       │
       ▼
4. Frontend: useAuth().guardarSesion(token, admin)
       │
       ▼
5. Router guard permite acceso a rutas protegidas
       │
       ▼
6. Redirect a HomeView (dashboard)
```

## Flujo de Verificación de Contraseña (Re-auth)

Usado en `EditarAdministradorView.vue` antes de permitir edición:

```
1. Usuario navega a /editaradministrador/:id
       │
       ▼
2. Se abre modal de re-autenticación (showReauthDialog = true)
       │
       ▼
3. Usuario ingresa contraseña actual
       │
       ▼
4. POST /api/auth/verify-password { password } con JWT
       │
       ▼
5. Si OK → passwordVerificada se guarda en memoria (ref)
       │
       ▼
6. Carga datos del admin (GET /api/administradores/:id)
       │
       ▼
7. Usuario edita y guarda (PUT /api/administradores/:id)
       │
       ▼
8. Si quiere cambiar password → usa passwordVerificada en
   PUT /api/administradores/:id/password { passwordActual, nuevaPassword }
```

## Flujo de Logout

```
1. Usuario click "Cerrar sesión" en NavBar/HomeView
       │
       ▼
2. useAuth().cerrarSesion() → limpia sessionStorage
       │
       ▼
3. router.push({ name: 'login' })
       │
       ▼
4. Navigation guard (soloInvitado) permite acceso a /login
```

## Uso en Componentes

```typescript
// En cualquier vista/componente
const { authHeaders, isAuthenticated, getAdmin, cerrarSesion } = useAuth()

// Para fetch autenticado
const res = await fetch(`${API}/api/administradores`, { headers: authHeaders() })

// Verificar rol/permisos
const admin = getAdmin()
if (admin?.role === 'SuperAdmin') { ... }

// Cerrar sesión
const handleLogout = () => {
  cerrarSesion()
  router.push({ name: 'login' })
}
```

## Integración con Navigation Guards

El guard global en `router/index.js` usa `useAuth()`:

```typescript
router.beforeEach((to) => {
  const { isAuthenticated } = useAuth()
  const autenticado = isAuthenticated()

  if (to.meta.requiereAuth && !autenticado)
    return { name: 'login' }

  if (to.meta.soloInvitado && autenticado)
    return { name: 'home' }
})
```

### Metas de Ruta

| Meta | Rutas | Comportamiento |
|------|-------|----------------|
| `requiereAuth: true` | `/`, `/administracion`, `/carreras`, `/formularios`, `/listados`, `/agregar*`, `/editar*`, `/eliminar*` | Redirige a `/login` si no autenticado |
| `soloInvitado: true` | `/login` | Redirige a `/` si ya autenticado |
| (sin meta) | `/contacto`, `/inscripcion` | Acceso público |

## Variables de Entorno

| Variable | Uso |
|----------|-----|
| `VITE_API_URL` | Base URL para llamadas al backend (ej: `http://localhost:5127`) |

Se accede via `import.meta.env.VITE_API_URL` en componentes.

## Seguridad

- **Token en sessionStorage:** Se limpia al cerrar pestaña/navegador (más seguro que localStorage)
- **No persistencia:** No hay "recordar sesión" implementado
- **Headers automáticos:** `authHeaders()` inyecta Bearer token en cada petición
- **Validación en backend:** El frontend confía en que el backend valida expiración/firma del JWT
- **Re-autenticación sensible:** `EditarAdministradorView` exige password actual antes de cargar datos
- **Password en memoria:** Contraseña verificada se guarda solo en `ref` (se limpia en `onUnmounted`)