# Estructura del Proyecto Frontend

## Directorios Principales

### `src/components/`
Componentes reutilizables de UI pura (sin lógica de negocio).

| Archivo | Descripción |
|---------|-------------|
| `NavBar.vue` | Barra de navegación superior con logo y enlaces públicos. Se oculta en `/inscripcion`. |
| `TheWelcome.vue` | Componente de bienvenida (legacy, no usado actualmente). |
| `WelcomeItem.vue` | Item individual para TheWelcome (legacy). |
| `Banner/` | Carpeta con assets de imágenes (banners para estudiantes/profesores). |

### `src/composables/`
Hooks de Vue 3 con lógica reactiva reutilizable.

| Archivo | Descripción |
|---------|-------------|
| `useAuth.js` | Gestión completa de autenticación: token, sesión admin, headers, login/logout. |

### `src/router/`
Configuración de enrutamiento SPA.

| Archivo | Descripción |
|---------|-------------|
| `index.js` | Definición de rutas, lazy loading, metadatos de auth, navigation guards globales. |

### `src/views/`
Vistas organizadas por dominio/funcionalidad (cada archivo = una ruta).

#### `views/public/` - Acceso público
| Archivo | Ruta | Descripción |
|---------|------|-------------|
| `HomeView.vue` | `/` | Dashboard principal (requiere auth). Menú de navegación a módulos. |
| `ContactoView.vue` | `/contacto` | Página de contacto pública. |

#### `views/auth/` - Autenticación
| Archivo | Ruta | Descripción |
|---------|------|-------------|
| `LoginView.vue` | `/login` | Formulario de inicio de sesión. Redirige a home si ya autenticado. |

#### `views/administradores/` - CRUD Administradores
| Archivo | Ruta | Descripción |
|---------|------|-------------|
| `AdministradorView.vue` | `/administracion` | Listado con tabla, acciones editar/eliminar. |
| `AgregarAdministradorView.vue` | `/agregaradministracion` | Formulario creación. |
| `EditarAdministradorView.vue` | `/editaradministrador/:id` | Formulario edición (props route). |
| `EliminarAdministradorView.vue` | `/eliminaradministrador/:id` | Confirmación eliminación (props route). |

#### `views/carrera/` - CRUD Carreras
| Archivo | Ruta | Descripción |
|---------|------|-------------|
| `CarreraView.vue` | `/carreras` | Listado con tabla, acciones editar/eliminar. |
| `AgregarCarreraView.vue` | `/agregarcarreras` | Formulario creación. |
| `EditarCarreraView.vue` | `/editarcarrera/:id` | Formulario edición (props route). |
| `EliminarCarreraView.vue` | `/eliminarcarreras/:id` | Confirmación eliminación (props route). |

#### `views/Listados/` - Reportes
| Archivo | Ruta | Descripción |
|---------|------|-------------|
| `ListadoView.vue` | `/listados` | Tabla de alumnos inscriptos por carrera (requiere auth). |
| `InscripciónView.vue` | `/inscripcion` | Formulario público de inscripción (sin navbar). |

#### Vistas transversales
| Archivo | Ruta | Descripción |
|---------|------|-------------|
| `NotFound.vue` | `*` (404) | Página no encontrada. |

### `src/assets/`
Estilos globales y recursos estáticos.

```
assets/
└── css/
    └── base/
        └── main.css    # Estilos globales, variables CSS, utilidades
```

### `src/plugins/`
Plugins de Vue (actualmente vacío, Pinia se registra en main.ts).

## Convenciones de Nomenclatura

- **Vistas**: `PascalCase` + `View.vue` (ej: `AdministradorView.vue`)
- **Componentes**: `PascalCase` (ej: `NavBar.vue`)
- **Composables**: `camelCase` + prefijo `use` (ej: `useAuth.ts`)
- **Interfaces**: `PascalCase` (ej: `Administrador`, `Carrera`)
- **Props de ruta**: `props: true` + `:id` en path para tipado automático