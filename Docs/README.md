# Documentación del Proyecto - Instituto Tupac Amaru

## Estructura de Documentación

```
Docs/
├── backend/          # Documentación del Backend (ASP.NET Core 9)
├── frontend/         # Documentación del Frontend (Vue 3 + Vite)
├── DATABASE.md       # Documentación completa de la BD (snapshot, DDL, datos)
└── README.md         # Este archivo
```

---

## Backend (ASP.NET Core 9)

**Tecnologías**: .NET 9, SQL Server (Express / LocalDB / SQL Auth), JWT Bearer, BCrypt, ADO.NET (`Microsoft.Data.SqlClient`)

### [Arquitectura](./backend/01-arquitectura/)
- [Visión General](./backend/01-arquitectura/01-vision-general.md) — Diagramas, principios, stack tecnológico
- [Patrones de Diseño](./backend/01-arquitectura/02-patrones-diseno.md) — Repository, Template Method, DI, Strategy, DTO
- [Estructura del Proyecto](./backend/01-arquitectura/03-estructura-proyecto.md) — Árbol de directorios, responsabilidades por capa (AD, BR, API)

### [Base de Datos](./backend/02-base-de-datos/)
- [Esquema BD](./backend/02-base-de-datos/01-esquema-bd.md) — ER diagram, tablas, columnas, índices, FKs
- [Script de Creación](./backend/02-base-de-datos/02-script-creacion.md) — DDL completo (el archivo `CreateDatabase.sql` **no está en el repo**; ver este doc)
- [Migraciones](./backend/02-base-de-datos/03-migraciones.md) — Estrategia manual, patrones ALTER, versionado

### [Modelos](./backend/03-modelos/)
- [Entidades de Dominio](./backend/03-modelos/01-entidades-dominio.md) — `Persona`, `Administrador`, `Alumno`, `Carrera`, `Profesor`, `Formulario`
- [DTOs y ViewModels](./backend/03-modelos/02-dtos-viewmodels.md) — `LoginDto`, `SetupAdminDto`, `AdminResult`, `AlumnoListadoDto`, `ListadoItem`
- [Validaciones](./backend/03-modelos/03-validaciones.md) — DataAnnotations, flujo validación, reglas de negocio

### [Servicios](./backend/04-servicios/)
- [Capa de Servicios](./backend/04-servicios/01-capa-servicios.md) — DI, `AccesoDB`, servicios CRUD, `AdministradorService`
- [AccesoDB](./backend/04-servicios/01-capa-servicios.md#accesodb-data-access-layer---institutoad) — Template methods, `ExecuteReader/NonQuery/Scalar`, manejo parámetros
- [Servicios CRUD](./backend/04-servicios/03-servicios-crud.md) — Detalle 6 servicios: Admin, Alumno, Carrera, Profesor, Formulario, Listado
- [Servicio Auth](./backend/04-servicios/04-auth-service.md) — Login, BCrypt work factor 12, setup inicial, JWT claims, change-password (en `AdministradorService`)

### [API / Controladores](./backend/05-api-controladores/)
- [Convenciones API](./backend/05-api-controladores/01-convenciones-api.md) — REST standards, status codes, formatos, versionado
- [Auth Endpoints](./backend/05-api-controladores/02-auth-endpoints.md) — `POST /api/auth/login`, JWT generation, claims, verify-password
- [Administradores](./backend/05-api-controladores/03-admin-endpoints.md) — CRUD completo `/api/administradores` + change-password + verify-password
- [Alumnos](./backend/05-api-controladores/04-alumnos-endpoints.md) — CRUD + inscripción pública `/api/alumnos`
- [Carreras](./backend/05-api-controladores/05-carreras-endpoints.md) — CRUD + catálogo público `/api/carreras`
- [Profesores](./backend/05-api-controladores/06-profesores-endpoints.md) — CRUD `/api/profesores`
- [Formularios](./backend/05-api-controladores/09-formularios-endpoints.md) — CRUD `/api/formularios`
- [Listados](./backend/05-api-controladores/07-listados-endpoints.md) — Join en memoria `/api/listado`
- [Setup Inicial](./backend/05-api-controladores/08-setup-endpoint.md) — `POST /api/setup/admin` (solo Dev)

### [Autenticación y Autorización](./backend/06-autenticacion-autorizacion/)
- [JWT Authentication](./backend/06-autenticacion-autorizacion/01-jwt-auth.md) — Config, token generation, validation, claims
- [Roles y Permisos](./backend/06-autenticacion-autorizacion/02-roles-permisos.md) — Matriz permisos, policies futuras
- [Protección Endpoints](./backend/06-autenticacion-autorizacion/03-proteccion-endpoints.md) — Middleware pipeline, atributos, testing

### [Excepciones](./backend/07-excepciones/)
- [Jerarquía](./backend/07-excepciones/01-jerarquia-excepciones.md) — `PersistenceException`, `EntityNotFoundException`
- [Manejo Global](./backend/07-excepciones/02-manejo-global-errores.md) — Exception handler, formatos, logging, Problem Details

### [Configuración](./backend/08-configuracion/)
- [AppSettings](./backend/08-configuracion/01-appsettings.md) — JSON structure, ConnectionStrings (SQL Auth / LocalDB), JWT, logging
- [Variables de Entorno](./backend/08-configuracion/02-variables-entorno.md) — Dev/Staging/Prod, Docker, K8s, Azure, AWS
- [CORS y Middleware](./backend/08-configuracion/03-cors-middleware.md) — Pipeline orden, políticas, producción

### [Despliegue](./backend/09-despliegue/)
- [Requisitos Previos](./backend/09-despliegue/01-requisitos-previos.md) — .NET 9, SQL Server Express, puertos, estructura
- [Pasos de Instalación](./backend/09-despliegue/02-pasos-instalacion.md) — BD, config, build, primer admin, publicar
- [Variables Producción](./backend/09-despliegue/03-variables-produccion.md) — Secrets, connection strings, rotación
- [Troubleshooting](./backend/09-despliegue/04-troubleshooting.md) — Errores comunes BD, Auth, Build, Runtime, diagnósticos

---

## Frontend (Vue 3 + TypeScript + Vite)

**Tecnologías**: Vue 3, TypeScript, Vite, Element Plus, Pinia, Vue Router 4, Fetch API

### [Documentación Frontend](./frontend/10-frontend/)
- [Visión General](./frontend/10-frontend/01-vision-general.md) — Stack, arquitectura, principios
- [Estructura del Proyecto](./frontend/10-frontend/02-estructura-proyecto.md) — Directorios, convenciones, módulos
- [Enrutamiento](./frontend/10-frontend/03-enrutamiento.md) — Vue Router, tabla rutas, guards, lazy loading
- [Autenticación](./frontend/10-frontend/04-autenticacion.md) — `useAuth` composable, login/logout, headers, guards, re-auth modal
- [Componentes y Vistas](./frontend/10-frontend/05-componentes-vistas.md) — Componentes reutilizables, vistas por módulo, patrones
- [Integración API](./frontend/10-frontend/06-integracion-api.md) — Cliente HTTP, endpoints por módulo, tipado, errores
- [Configuración y Build](./frontend/10-frontend/07-configuracion.md) — Vite, TS, ESLint, Prettier, env vars

---

## Inicio Rápido

### Backend
```bash
# 1. Base de Datos (SQL Server)
# Opción A: SQL Server Express / Developer Edition
#   Conectar con SSMS / Azure Data Studio / VS Code
#   Crear BD 'InstitutoDB' y ejecutar DDL (ver docs/backend/02-base-de-datos/02-script-creacion.md)
#
# Opción B: LocalDB (desarrollo)
#   Se crea automáticamente al ejecutar la API con appsettings.json

# 2. Configurar connection string en appsettings.Development.json (SQL Auth)
# "Server=localhost;Database=InstitutoDB;User Id=instituto_user;Password=Instituto2026;TrustServerCertificate=True;"

# 3. Compilar y ejecutar
cd Instituto.API
dotnet run --environment Development
# → http://localhost:5127 | https://localhost:7244

# 4. Crear primer admin (una sola vez, solo en Development)
curl -X POST http://localhost:5127/api/setup/admin \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Super","apellido":"Admin","email":"admin@tupac.edu.ar","password":"Tupac123","role":"SuperAdmin"}'
```

### Frontend
```bash
cd Frontend
npm install
npm run dev  # → http://localhost:5176
```

---

## Integración Backend ↔ Frontend

| Aspecto | Backend | Frontend |
|---------|---------|----------|
| **Base URL** | `http://localhost:5127` | `VITE_API_URL=http://localhost:5127` |
| **Auth** | JWT Bearer 8h | `sessionStorage` + `useAuth` |
| **CORS** | `AllowAnyOrigin()` (dev) | Fetch directo a `VITE_API_URL` (sin proxy Vite) |
| **Endpoints** | Documentados en [05-api-controladores](./backend/05-api-controladores/) | Consumidos en [06-integracion-api](./frontend/10-frontend/06-integracion-api.md) |
| **Response Wrapper** | `ApiResponse<T>` | Se desempaqueta en composables |

---

## 📋 Notas Importantes (Estado Actual)

| Tema | Estado | Detalle |
|------|--------|---------|
| **Script DDL** | ❌ No en repo | Ver `docs/DATABASE.md` o `backend/02-base-de-datos/02-script-creacion.md` |
| **Connection String** | ✅ Configurado | `appsettings.Development.json` (SQL Auth) + `appsettings.json` (LocalDB) |
| **Soft Delete Admin** | ✅ Implementado | Índice UNIQUE parcial `WHERE Activo = 1` |
| **Swagger/OpenAPI** | ❌ No configurado | Ver [convenciones-api.md](./backend/05-api-controladores/01-convenciones-api.md#documentación-openapiswagger-no-configurado) |
| **Paginación** | ❌ No implementada | `GetAll` retorna todo |
| **Tests Backend** | ✅ 44 passing | `dotnet test Instituto.sln` |
| **Profesores Frontend** | ❌ Sin vistas | Backend CRUD completo ✅ |