# Componentes y Vistas

## Componentes Reutilizables (`src/components/`)

### `NavBar.vue`
**Propósito:** Barra de navegación superior fija.

**Características:**
- Logo clickeable → redirige a `/` (home)
- Enlaces: Inscripción (`/inscripcion`), Contacto (`/contacto`)
- **Se oculta automáticamente** en ruta `/inscripcion` (vía computed `ocultarNavbar`)

```typescript
const ocultarNavbar = computed(() => route.path === '/inscripcion')
```

**Estilos:** Clases CSS propias (`.navbar`, `.logo`, `.menu`) + variables CSS globales.

---

### `Banner/` (Carpeta de Assets)
Contiene imágenes estáticas usadas como fondo decorativo en vistas:
- `bannerEstudiante.jpg` - Banner para vistas de estudiantes
- `bannerProfesor.jpg` - Banner para vistas de administración/profesores

Se importan via alias `@/components/Banner/archivo.jpg`.

---

## Vistas por Módulo

### Módulo Público (`views/public/`)

#### `HomeView.vue` - `/` (Dashboard Principal)
- **Meta:** `requiereAuth: true`
- **UI:** Card centrada con banner, título, menú de navegación admin (botones router-link)
- **Funcionalidad:** 
  - Menú con enlaces a: Administradores, Carreras, Inscripción, Listados
  - Botones deshabilitados: Información académica, Descargas
  - Botón "Cerrar sesión" → navega a `/login`
  - `onMounted`: Health check a `${API}/` (test backend conectividad)

#### `ContactoView.vue` - `/contacto`
- Vista simple estática de información de contacto.

#### `FormulariosView.vue` - `/formularios`
- **Meta:** `requiereAuth: true`
- **UI:** Tabla con columnas: Nombre, Estado, Fecha apertura, Fecha cierre, Acciones
- **Fetch:** `GET ${API}/api/formularios` con `authHeaders()`
- **Acciones:** Editar (router-link) + Eliminar (router-link)
- **Estados:** loading, error, empty state
- **CTA footer:** "Agregar formulario" → `/agregarformulario`

#### `AgregarFormularioView.vue` - `/agregarformulario`
- **Meta:** `requiereAuth: true`
- **Formulario:** nombre, estado (select), fechaApertura, fechaCierre, descripcion
- **Submit:** `POST ${API}/api/formularios`

#### `EditarFormularioView.vue` - `/editarformulario/:id`
- **Meta:** `requiereAuth: true`, `props: true`
- **Carga:** `GET ${API}/api/formularios/${id}`
- **Submit:** `PUT ${API}/api/formularios/${id}`

#### `EliminarFormularioView.vue` - `/eliminarformulario/:id`
- **Meta:** `requiereAuth: true`, `props: true`
- **Carga:** `GET ${API}/api/formularios/${id}`
- **Acción:** `DELETE ${API}/api/formularios/${id}`

---

### Módulo Autenticación (`views/auth/`)

#### `LoginView.vue` - `/login`
- **Meta:** `soloInvitado: true`
- **UI:** Formulario con email + password, validación HTML5
- **Lógica:** 
  - POST a `${API}/api/auth/login`
  - En éxito: `guardarSesion(token, admin)` + `router.push({ name: 'home' })`
  - En error: muestra mensaje de error
- **Feature:** Toggle password visibility (botón ojo con icons View/Hide)

---

### Módulo Administradores (`views/administradores/`)

Patrón CRUD consistente en 4 vistas:

| Vista | Ruta | Props | Operación |
|-------|------|-------|-----------|
| `AdministradorView.vue` | `/administracion` | - | **Read** (listado) |
| `AgregarAdministradorView.vue` | `/agregaradministracion` | - | **Create** |
| `EditarAdministradorView.vue` | `/editaradministrador/:id` | `id` | **Update** |
| `EliminarAdministradorView.vue` | `/eliminaradministrador/:id` | `id` | **Delete** |

#### `AdministradorView.vue` (Listado)
- **Tabla** con columnas: Nombre, Apellido, Email, Acciones
- **Acciones:** Botones Editar (router-link) + Eliminar (router-link) con icons Element Plus
- **Fetch:** `GET ${API}/api/administradores` con `authHeaders()`
- **Estados:** loading, error, empty state
- **CTA footer:** "Agregar administrador" → `/agregaradministracion`

#### `AgregarAdministradorView.vue` (Crear)
- **Formulario reactivo:** nombre, apellido, email, role (select), passwordTemp (oculto, default)
- **Validación:** required en campos
- **Submit:** `POST ${API}/api/administradores` con body JSON
- **Éxito:** `router.push({ name: 'administracion' })`

#### `EditarAdministradorView.vue` (Editar) ⭐ **Re-autenticación**
- **Props:** `defineProps<{ id: string }>()`
- **Flujo único:** Antes de cargar datos → modal de verificación de contraseña actual
  1. Abre modal `showReauthDialog`
  2. Usuario ingresa password actual
  3. `POST ${API}/api/auth/verify-password` → si OK, guarda `passwordVerificada` en ref (memoria)
  4. Carga datos admin: `GET ${API}/api/administradores/${id}`
- **Submit datos:** `PUT ${API}/api/administradores/${id}` (nombre, apellido, email)
- **Cambio password opcional:** Si usuario ingresa nueva → `PUT ${API}/api/administradores/${id}/password` usando `passwordVerificada` guardada
- **Limpieza:** `onUnmounted` limpia `passwordVerificada`

#### `EliminarAdministradorView.vue` (Eliminar)
- **Props:** `defineProps<{ id: string }>()`
- **UI:** Confirmación con datos del admin (fetch GET previo)
- **Acción:** `DELETE ${API}/api/administradores/${id}`
- **Éxito:** redirect a listado
- ⚠️ **Deuda técnica:** Usa `http://localhost:5089` hardcoded (2 líneas)

---

### Módulo Carreras (`views/carrera/`)

Estructura idéntica a Administradores:

| Vista | Ruta | Props | Operación |
|-------|------|-------|-----------|
| `CarreraView.vue` | `/carreras` | - | **Read** |
| `AgregarCarreraView.vue` | `/agregarcarreras` | - | **Create** |
| `EditarCarreraView.vue` | `/editarcarrera/:id` | `id` | **Update** |
| `EliminarCarreraView.vue` | `/eliminarcarreras/:id` | `id` | **Delete** |

#### Diferencias clave:
- **Interfaz `Carrera`:** id, nombre, duracionAnios, turno, modalidad, horario, estado
- **Tabla columnas:** Nombre, Duración, Turno, Modalidad, Horario, Estado, Acciones
- **Fetch:** `GET ${API}/api/carreras`
- ⚠️ **Deuda técnica:** `CarreraView.vue` usa `http://localhost:5089` hardcoded (1 línea)

---

### Módulo Listados (`views/Listados/`)

#### `ListadoView.vue` - `/listados`
- **Meta:** `requiereAuth: true`
- **Propósito:** Reporte de alumnos inscriptos por carrera
- **Interfaz `AlumnoListado`:** alumnoId, nombreCompleto, dni, email, carrera, turno, edad
- **Fetch:** `GET ${API}/api/listado` con `authHeaders()`
- **Tabla:** Alumno, DNI, Edad, Carrera, Turno, (2 columnas vacías para acciones futuras)

#### `InscripciónView.vue` - `/inscripcion`
- **Acceso público** (sin auth, sin navbar)
- **Formulario extenso:** datos personales, contacto, carrera, turno, modalidad, horario
- **Carreras:** `GET ${API}/api/carreras` (público)
- **Submit:** `POST ${API}/api/alumnos` (público, `[AllowAnonymous]`)
- ⚠️ **Deuda técnica:** Usa `http://localhost:5089` hardcoded (2 líneas)

---

### Vista Transversal

#### `NotFound.vue` - `*` (404)
- Página genérica "Página no encontrada" con link a home.

---

## Patrones Comunes en Vistas

### 1. Fetch con Loading/Error
```typescript
const data = ref<T[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const cargar = async () => {
  try {
    const res = await fetch(`${API}/api/endpoint`, { headers: authHeaders() })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    data.value = await res.json()
  } catch (err) {
    error.value = 'Mensaje amigable'
  } finally {
    loading.value = false
  }
}

onMounted(cargar)
```

### 2. Props de Ruta Tipadas
```typescript
// En router: props: true
// En componente:
defineProps<{ id: string }>()
const idNum = Number(id)
```

### 3. Uso de Element Plus Icons
```vue
<el-icon><Edit /></el-icon>
<el-icon><Delete /></el-icon>
```

### 4. Estructura Visual Estándar
```vue
<main class="section">
  <div class="card card-center">
    <img class="card-media" src="@/components/Banner/bannerProfesor.jpg" alt="..." />
    <header class="text-center">
      <h1>Título</h1>
      <p class="subtitle">Descripción</p>
    </header>
    <!-- Tabla / Formulario -->
    <footer class="table-actions center">
      <router-link class="btn btn-primary" to="...">Acción principal</router-link>
    </footer>
  </div>
</main>
```

---

## Estilos Globales (`src/assets/css/base/main.css`)

Define:
- Variables CSS (colores, spacing, breakpoints)
- Clases utilitarias: `.section`, `.card`, `.card-center`, `.card-lg`, `.table`, `.table-header`, `.table-row`, `.table-cols-admin`, `.table-cols-default`, `.table-cols-formularios`, `.table-actions`, `.btn`, `.btn-primary`, `.btn-success`, `.btn-danger`, `.btn-secondary`, `.btn-menu`, `.text-center`, `.text-muted`, `.text-danger`, `.error-msg`, `.center`
- Reset básico y tipografía