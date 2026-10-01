# Integración con API (Backend)

## Cliente HTTP

El frontend usa **`fetch` nativo** (sin Axios ni librería adicional) para todas las peticiones al backend.

## Configuración de Base URL

```typescript
const API = import.meta.env.VITE_API_URL
// Ejemplo: http://localhost:5127
```

Se define en `.env`:
```env
VITE_API_URL=http://localhost:5127
```

## Headers Estándar

Generados por `useAuth().authHeaders()`:

```typescript
// Con sesión activa
{
  'Content-Type': 'application/json',
  'Authorization': 'Bearer <jwt-token>'
}

// Sin sesión
{
  'Content-Type': 'application/json'
}
```

## Endpoints Consumidos

### Autenticación
| Método | Endpoint | Vista | Descripción |
|--------|----------|-------|-------------|
| POST | `/api/auth/login` | `LoginView` | Login, retorna `{ token, admin }` |
| POST | `/api/auth/verify-password` | `EditarAdministradorView` | Verifica password actual |

### Administradores
| Método | Endpoint | Vista | Descripción |
|--------|----------|-------|-------------|
| GET | `/api/administradores` | `AdministradorView` | Listado completo |
| GET | `/api/administradores/:id` | `EditarAdministradorView`, `EliminarAdministradorView` | Detalle por ID |
| POST | `/api/administradores` | `AgregarAdministradorView` | Crear nuevo |
| PUT | `/api/administradores/:id` | `EditarAdministradorView` | Actualizar datos |
| PUT | `/api/administradores/:id/password` | `EditarAdministradorView` | Cambiar contraseña |
| DELETE | `/api/administradores/:id` | `EliminarAdministradorView` | Eliminar |

### Carreras
| Método | Endpoint | Vista | Descripción |
|--------|----------|-------|-------------|
| GET | `/api/carreras` | `CarreraView`, `InscripciónView` | Listado completo (público) |
| GET | `/api/carreras/:id` | `EditarCarreraView`, `EliminarCarreraView` | Detalle por ID |
| POST | `/api/carreras` | `AgregarCarreraView` | Crear nueva |
| PUT | `/api/carreras/:id` | `EditarCarreraView` | Actualizar |
| DELETE | `/api/carreras/:id` | `EliminarCarreraView` | Eliminar |

### Alumnos
| Método | Endpoint | Vista | Descripción |
|--------|----------|-------|-------------|
| GET | `/api/alumnos` | (admin) | Listado completo |
| GET | `/api/alumnos/:id` | (admin) | Detalle por ID |
| POST | `/api/alumnos` | `InscripciónView` | **Pública** - Nueva inscripción |
| PUT | `/api/alumnos/:id` | (admin) | Actualizar |
| DELETE | `/api/alumnos/:id` | (admin) | Eliminar |

### Formularios
| Método | Endpoint | Vista | Descripción |
|--------|----------|-------|-------------|
| GET | `/api/formularios` | `FormulariosView` | Listado completo |
| GET | `/api/formularios/:id` | `EditarFormularioView`, `EliminarFormularioView` | Detalle por ID |
| POST | `/api/formularios` | `AgregarFormularioView` | Crear nuevo |
| PUT | `/api/formularios/:id` | `EditarFormularioView` | Actualizar |
| DELETE | `/api/formularios/:id` | `EliminarFormularioView` | Eliminar |

### Listados / Reportes
| Método | Endpoint | Vista | Descripción |
|--------|----------|-------|-------------|
| GET | `/api/listado` | `ListadoView` | Alumnos inscriptos por carrera (join) |

## Patrones de Request

### GET Listado
```typescript
const res = await fetch(`${API}/api/administradores`, { headers: authHeaders() })
if (!res.ok) throw new Error(`Error HTTP ${res.status}`)
const data = await res.json()
```

### GET By ID
```typescript
const res = await fetch(`${API}/api/administradores/${id}`, { headers: authHeaders() })
```

### POST Crear
```typescript
const res = await fetch(`${API}/api/administradores`, {
  method: 'POST',
  headers: authHeaders(),
  body: JSON.stringify({ nombre, apellido, email, role, passwordTemp: 'Cambiar1234!' })
})
```

### PUT Actualizar (datos)
```typescript
const res = await fetch(`${API}/api/administradores/${id}`, {
  method: 'PUT',
  headers: authHeaders(),
  body: JSON.stringify({ nombre, apellido, email })
})
```

### PUT Cambiar Contraseña
```typescript
const res = await fetch(`${API}/api/administradores/${id}/password`, {
  method: 'PUT',
  headers: authHeaders(),
  body: JSON.stringify({ passwordActual: '...', nuevaPassword: '...' })
})
```

### DELETE
```typescript
const res = await fetch(`${API}/api/administradores/${id}`, {
  method: 'DELETE',
  headers: authHeaders()
})
```

## Manejo de Errores

```typescript
try {
  const res = await fetch(url, options)
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  const data = await res.json()
} catch (err) {
  console.error(err)
  error.value = 'Mensaje amigable para el usuario'
}
```

- **Errores de red/HTTP:** Capturados en `catch`
- **Errores 401/403:** No hay manejo específico (el token expira → backend rechaza → UI muestra error genérico)
- **Validación backend:** Los errores 400 con detalles no se parsean estructuralmente

## Tipado de Respuestas

Interfaces locales en cada vista:

```typescript
// AdministradorView.vue
interface AdministradorView {
  id: number
  nombre: string
  apellido: string
  email: string
}

// CarreraView.vue
interface Carrera {
  id: number
  nombre: string
  duracionAnios: number
  turno: string
  modalidad: string
  horario: string
  estado: string
}

// ListadoView.vue
interface AlumnoListado {
  alumnoId: number
  nombreCompleto: string
  dni: number
  email: string
  carrera: string
  turno: string
  edad: number
}

// FormulariosView.vue
interface Formulario {
  id: number
  nombre: string
  estado: 'Abierto' | 'Cerrado' | 'Borrador'
  fechaApertura: string
  fechaCierre: string
  descripcion: string
}
```

## Variables de Entorno Requeridas

| Variable | Descripción | Requerida |
|----------|-------------|-----------|
| `VITE_API_URL` | URL base del backend API | Sí |

Archivo `.env.example`:
```env
VITE_API_URL=http://localhost:5127
```

## CORS

El backend debe permitir:
- Origin: `http://localhost:5176` (Vite dev server)
- Headers: `Content-Type`, `Authorization`
- Methods: `GET, POST, PUT, DELETE, OPTIONS`

## Timeouts y Reintentos

No implementados actualmente. El `fetch` nativo no tiene timeout por defecto.

## Estado de Sesión y Token

- **Almacenamiento:** `sessionStorage` (se limpia al cerrar pestaña)
- **Expiración:** Manejada por backend (JWT). Frontend no valida `exp` claim.
- **Refresh token:** No implementado
- **Logout:** Limpia `sessionStorage` localmente; no notifica a backend