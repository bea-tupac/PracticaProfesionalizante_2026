# 🏛️ Sistema de Gestión Institucional — Instituto Superior Docente Túpac Amaru

> Plataforma de gestión académica para la administración de carreras, alumnos, administradores, profesores, formularios e inscripciones. Desarrollada con arquitectura **Frontend (Vue 3) + Backend (ASP.NET Core 9 + SQL Server)** en arquitectura **N-Tier (AD → BR → API)**.

---

## 📋 Descripción del Proyecto

Sistema web para el **Instituto Superior Docente Túpac Amaru** que centraliza la gestión académica y administrativa:

| Módulo | Funcionalidad | Estado Backend | Estado Frontend |
|--------|---------------|----------------|-----------------|
| **Carreras** | CRUD completo + validación integridad (no eliminar si hay alumnos) | ✅ | ✅ |
| **Alumnos** | Inscripción pública + listado con join carrera | ✅ | ✅ (inscripción) / ⚠️ (panel admin sin Edit/Delete) |
| **Administradores** | CRUD autenticado con JWT + change-password + verify-password | ✅ | ✅ |
| **Profesores** | CRUD completo autenticado | ✅ | ❌ (sin vistas) |
| **Formularios** | CRUD completo autenticado (estados: Borrador/Abierto/Cerrado) | ✅ | ✅ (UI completa) |
| **Listados** | Vista consolidada alumnos + carrera (join en memoria) | ✅ | ✅ |
| **Autenticación** | Login JWT (8h) + Route Guards + Password toggle + Verify/Change password | ✅ | ✅ |

---

## 🏗️ Arquitectura (N-Tier: AD → BR → API)

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENTE (Navegador)                      │
│  Vue 3 + JavaScript + Vite + Element Plus + Pinia + Vue Router │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTPS / REST API + JWT
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      ASP.NET CORE 9 API (Presentation)          │
│  Controllers → BR Services → AD Repositories → SQL Server       │
│  JWT Auth + BCrypt (workFactor:12) + Global Exception Handling  │
└─────────────────────────────────────────────────────────────────┘
```

**Capas del Backend:**
- **API Layer** (`Instituto.API`): 8 Controllers, Middleware, DI, JWT, CORS, Global Exception Handler
- **BR Layer** (`Instituto.BR`): 6 Servicios con lógica de negocio, validaciones, Result Pattern (`ServiceResult<T>`)
- **AD Layer** (`Instituto.AD`): 6 Repositorios tipados, `AccesoDB` (ADO.NET wrapper), Entidades de dominio

**Patrones aplicados:**
- **Backend**: Repository pattern, Template Method (`AccesoDB`), DI, Result Pattern (`ServiceResult<T>`), Exception handling tipado
- **Frontend**: Composition API, Componentes por vista, CSS modular (BEM-like), Route Guards, Composable `useAuth`

---

## 🛠️ Stack Tecnológico

### Frontend
| Tecnología | Versión | Uso |
|------------|---------|-----|
| Vue 3 | 3.5.18 | Framework reactivo (Composition API) |
| JavaScript | ES2023+ | Lógica de aplicación (sin TypeScript) |
| Vite | 7.0.6 | Bundler & Dev Server |
| Vue Router | 4.5.1 | SPA Routing + Guards |
| Pinia | 3.0.3 | Estado global (configurado; auth vía `useAuth` composable + sessionStorage) |
| Element Plus | 2.11.1 | Componentes UI |
| @element-plus/icons-vue | 1.1.4 | Iconografía |
| ESLint + Prettier | 9.31 / 3.6.2 | Linting & Formato |

### Backend
| Tecnología | Versión | Uso |
|------------|---------|-----|
| ASP.NET Core | 9.0 | Web API Framework |
| Microsoft.Data.SqlClient | 5.2+ | Driver SQL Server (ADO.NET) |
| BCrypt.Net-Next | 4.0.3 | Hash de contraseñas (workFactor: 12) |
| JWT Bearer | 9.0.5 | Autenticación stateless |
| System.Text.Json | Built-in | Serialización |
| MSTest + Moq | 3.6+ / 4.20+ | Testing unitario e integración |

### Base de Datos
- **SQL Server** (Express / LocalDB / SQL Auth)
- Esquema: `Administradores`, `Alumnos`, `Carreras`, `Profesores`, `Formularios`
- Connection Strings configurables en `appsettings.json` / `appsettings.Development.json`
- *Nota: El script DDL `CreateDatabase.sql` no está incluido en el repo; crear tablas manualmente o vía migraciones*

---

## 📁 Estructura del Proyecto (Solution)

```
Instituto.sln
├── Instituto.AD/              # Data Access Layer
│   ├── Interfaces/            # ICarreraRepository, IAlumnoRepository, etc.
│   ├── Models/                # Entidades: Persona, Administrador, Alumno, Carrera, Profesor, Formulario, ListadoItem
│   ├── Repositories/          # 6 Repositorios tipados + AccesoDB (ADO.NET wrapper)
│   ├── AccesoDB.cs            # Wrapper ADO.NET genérico (ExecuteReader/NonQuery/Scalar)
│   ├── DBParameter.cs / DBParameters.cs
│   └── Instituto.AD.csproj
│
├── Instituto.BR/              # Business Rules Layer
│   ├── DTOs/                  # ServiceResult<T>, AdminResult, LoginDto, SetupAdminDto, etc.
│   ├── Interfaces/            # ICarreraService, IAlumnoService, IAdministradorService, etc.
│   ├── Services/              # 6 Servicios: Carrera, Alumno, Administrador, Profesor, Formulario, Listado
│   └── Instituto.BR.csproj
│
├── Instituto.API/             # Presentation Layer (ASP.NET Core 9)
│   ├── Controllers/           # 8 Controllers: Auth, Setup, Administrador, Alumnos, Carrera, Profesor, Formulario, Listado
│   ├── Models/                # DTOs API: ApiModels (ApiResponse<T>, LoginRequest, etc.)
│   ├── Program.cs             # Composition root + pipeline + DI + JWT + CORS + Global Exception Handler
│   ├── appsettings.Development.json  # ConnectionString (SQL Auth) + JWT
│   ├── appsettings.json       # ConnectionString (LocalDB) + JWT
│   └── Instituto.API.csproj
│
├── Instituto.AD.Test/         # 20 tests unitarios (MSTest + Moq)
├── Instituto.BR.Test/         # 17 tests unitarios (MSTest + Moq)
├── Instituto.API.Test/        # 7 tests integración (WebApplicationFactory)
│
├── Frontend/                  # Vue 3 + Vite
│   ├── package.json
│   ├── vite.config.js
│   ├── .env                   # VITE_API_URL=http://localhost:5127
│   └── src/                   # 19 vistas, composables, components, router, CSS modular
│
└── Docs/                      # Documentación técnica completa
```

---

## 🚀 Inicio Rápido

### Prerrequisitos
- **.NET 9 SDK**
- **Node.js 20+** y **npm**
- **SQL Server** (Express, LocalDB, o contenedor Docker)

### 1. Base de Datos (SQL Server)
```bash
# Opción A: SQL Server Express / Developer Edition
# Conectar con SSMS / Azure Data Studio / VS Code
# Crear base de datos 'InstitutoDB' y ejecutar script DDL manualmente
# Tablas: Administradores, Alumnos, Carreras, Profesores, Formularios

# Opción B: LocalDB (desarrollo)
# Se crea automáticamente al ejecutar la API con appsettings.json
```

### 2. Configuración Backend
```json
// Instituto.API/appsettings.Development.json (desarrollo con SQL Auth)
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=InstitutoDB;User Id=instituto_user;Password=Instituto2026;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "Tupac@Amaru#Instituto!JWT$2026*Clave&MuySecreta=32chars",
    "Issuer": "InstitutoTupacAmaru",
    "Audience": "InstitutoTupacAmaruAdmin"
  }
}
```

```json
// Instituto.API/appsettings.json (producción / LocalDB)
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "Tupac@Amaru#Instituto!JWT$2026*Clave&MuySecreta=32chars",
    "Issuer": "InstitutoTupacAmaru",
    "Audience": "InstitutoTupacAmaruAdmin"
  }
}
```

### 3. Ejecutar Backend
```bash
cd Instituto.API
dotnet run --environment Development
# → http://localhost:5127 | https://localhost:7244
```

### 4. Configuración Frontend
```bash
# Frontend/.env (ya configurado)
VITE_API_URL=http://localhost:5127
```

### 5. Ejecutar Frontend
```bash
cd Frontend
npm install      # solo primera vez
npm run dev      # → http://localhost:5176
```

### 6. Crear Primer Admin (solo primera vez, solo en Development)
```bash
# Terminal o REST Client
curl -X POST http://localhost:5127/api/setup/admin \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Super",
    "apellido": "Admin",
    "email": "admin@tupac.edu.ar",
    "password": "Tupac123",
    "role": "SuperAdmin"
  }'
```

### 7. Login
- Abrir `http://localhost:5176/login`
- **Email**: `admin@tupac.edu.ar`
- **Password**: `Tupac123`
- Redirige a `/` (Dashboard / Panel de Administración)

---

## 🔐 Autenticación & Autorización

| Aspecto | Implementación |
|---------|----------------|
| **Login** | `POST /api/auth/login` → JWT (8h expiry) |
| **Hash** | BCrypt `workFactor: 12` |
| **Token Storage** | `sessionStorage` (expira al cerrar pestaña) |
| **Route Guards** | `meta.requiereAuth` + `meta.soloInvitado` |
| **Logout** | Limpia `sessionStorage` + redirect `/login` |
| **Roles** | `Admin` / `SuperAdmin` (claim `ClaimTypes.Role`) |
| **Password Toggle** | Botón ojo en LoginView (View/Hide icons) |
| **Verify Password** | `POST /api/auth/verify-password` (requiere JWT) |
| **Change Password** | `PUT /api/administradores/{id}/password` (requiere JWT) |

---

## 📡 API Endpoints Principales

| Módulo | Endpoints | Auth |
|--------|-----------|------|
| **Auth** | `POST /api/auth/login` | Público |
| **Auth** | `POST /api/auth/verify-password` | JWT |
| **Setup** | `POST /api/setup/admin` (solo Dev, 1 vez) | Público |
| **Carreras** | `GET/POST/PUT/DELETE /api/carreras` | JWT (GET público) |
| **Alumnos** | `GET/PUT/DELETE /api/alumnos` | JWT |
| | `POST /api/alumnos` (inscripción) | **Público** |
| **Administradores** | `GET/POST/PUT/DELETE /api/administradores` | JWT |
| | `PUT /api/administradores/{id}/password` | JWT |
| **Profesores** | `GET/POST/PUT/DELETE /api/profesores` | JWT |
| **Formularios** | `GET/POST/PUT/DELETE /api/formularios` | JWT |
| **Listados** | `GET /api/listado` (join Alumno+Carrera) | JWT |

**Códigos HTTP estándar:**
- `200 OK` — GET, PUT exitosos
- `201 Created` — POST (con `Location` header)
- `204 No Content` — DELETE
- `400 Bad Request` — Validación / FK inexistente
- `401 Unauthorized` — Token inválido/expirado / credenciales incorrectas
- `403 Forbidden` — Sin permisos
- `404 Not Found` — Recurso inexistente
- `500 Internal Server Error` — Error interno (sin stack trace)

---

## 🎨 CSS Modular (Sin `<style>` en .vue)

```
src/assets/css/
├── base/
│   ├── main.css        # @import de todo
│   └── global.css      # Variables CSS, reset, utilidades
├── components/
│   ├── buttons.css     # .btn, variants, sizes
│   ├── card.css        # .card, .card-center, .card-lg, .card-header, .card-title
│   ├── forms.css       # .form, .form-row, .field, .form-actions
│   ├── inputs.css      # Inputs, selects, .password-field, .password-toggle
│   ├── table.css       # .table, .table-header, .table-row, .table-empty-state, badges
│   ├── navbar.css      # .navbar, .menu
│   └── admin-menu.css  # Grid botones dashboard
└── layout/
    └── section.css     # .section (centrado + padding)
```

**Principio:** Las vistas solo usan clases globales. Variables en `global.css` (single source of truth).
*Nota: El archivo actual se llama `innputs.css` (typo conocido, ver deuda técnica)*

---

## 🧪 Testing

### Tests Backend (44 tests - Todos pasando)
```bash
dotnet test Instituto.sln
# Instituto.AD.Test    → 20 tests (Repositorios)
# Instituto.BR.Test    → 17 tests (Servicios + Auth)
# Instituto.API.Test   → 7 tests (Integración API + Auth)
```

### Scripts Disponibles

**Frontend:**
```bash
cd Frontend
npm run dev        # Servidor desarrollo (Vite)
npm run build      # Build producción (vite build)
npm run preview    # Preview build
npm run lint       # ESLint + fix
npm run format     # Prettier
```

**Backend:**
```bash
dotnet run --environment Development    # Dev server
dotnet build                             # Compilar
dotnet test                              # Tests (44 passing)
```

---

## 🐛 Deuda Técnica Conocida

| Prioridad | Item |
|-----------|------|
| **Crítica** | `InscripciónView.vue` tipa `CarreraId: string` vs `number` (backend) |
| **Alta** | Sin capa de servicios API centralizada (`src/services/`) |
| **Media** | `ex.Message.Contains("Carrera")` frágil en `AlumnosController.cs` |
| **Media** | Nombre archivo `InscripciónView.vue` con `ó` (riesgo Linux/CI) |
| **Media** | **Profesores**: Backend CRUD completo ✅ pero **Frontend sin vistas** |
| **Baja** | Typo `innputs.css` → `inputs.css` (archivo real: `innputs.css`) |
| **Baja** | Backend devuelve `PasswordHash` y `Role` en GET administradores (debería usar DTO sin datos sensibles) |

---

## 📄 Documentación Técnica Completa

| Archivo | Estado |
|---------|--------|
| **README.md** | ✅ Actualizado (este archivo) |
| **PROYECTO.md** | ✅ Actualizado (arquitectura N-Tier, SQL Express, JWT, flujo de datos) |
| **Docs/** | ✅ Documentación modular actualizada (backend + frontend) |

---

## 👥 Autores

Desarrollado por estudiantes de **3.º año TSAS** — Instituto Superior Docente Túpac Amaru (2026)

---

## 📄 Licencia

MIT — Uso educativo e institucional.