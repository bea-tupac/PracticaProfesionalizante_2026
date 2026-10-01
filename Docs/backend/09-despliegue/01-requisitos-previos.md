# Requisitos Previos

## Software Requerido

| Componente | Versión Mínima | Notas |
|------------|----------------|-------|
| **.NET SDK** | 9.0 | `dotnet --version` |
| **SQL Server** | 2019+ (LocalDB/Express/Standard) | LocalDB incluido en VS |
| **Git** | 2.x | Control de versiones |
| **IDE** | VS 2022 / VS Code / Rider | Con extensiones C# |

## Verificación de Entorno

```bash
# .NET SDK
dotnet --list-sdks
# Debe mostrar 9.0.x

# SQL Server LocalDB
sqllocaldb info
# Debe mostrar "MSSQLLocalDB" v15+

# Git
git --version
```

## Permisos Necesarios

- **Desarrollo**: Usuario local con acceso a LocalDB (Windows Auth) o SQL Auth (`instituto_user`)
- **Producción**: Usuario DB con `db_owner` en `InstitutoDB` o permisos `CREATE TABLE`, `INSERT`, `UPDATE`, `DELETE`, `SELECT`
- **FS**: Escritura en carpeta logs (si file logging)
- **Puerto**: 5127 (dev) / 8080 (prod) disponible

---

## Estructura de Carpetas Esperada

```
SitioWebInstituto/
├── instituto/
│   ├── Instituto.API/         # ASP.NET Core 9 Web API
│   ├── Instituto.AD/          # Data Access Layer
│   ├── Instituto.BR/          # Business Rules Layer
│   ├── Instituto.API.Test/    # Integration Tests
│   ├── Instituto.BR.Test/     # Unit Tests BR
│   ├── Instituto.AD.Test/     # Unit Tests AD
│   ├── Frontend/              # Vue 3 + Vite
│   ├── Docs/                  # Esta documentación
│   ├── Instituto.sln          # Solution file
│   └── brana.sln              # Solution file (legacy)
```

---

## Base de Datos

### Opción A: LocalDB (Desarrollo - Alternativa)
- Ya instalado con Visual Studio / Build Tools
- Connection string: `Server=(localdb)\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;`
- Sin configuración extra (usado en `appsettings.json`)

### Opción B: SQL Server Express / Developer / Standard (Actual)
```bash
# Instalar (Windows)
# Descargar de https://www.microsoft.com/sql-server/sql-server-downloads

# Verificar instancia
sqlcmd -S localhost -Q "SELECT @@VERSION"

# Crear login para app (development actual)
CREATE LOGIN [instituto_user] WITH PASSWORD = 'Instituto2026';
CREATE USER [instituto_user] FOR LOGIN [instituto_user];
ALTER ROLE [db_owner] ADD MEMBER [instituto_user];
```
Connection string: `Server=localhost;Database=InstitutoDB;User Id=instituto_user;Password=Instituto2026;TrustServerCertificate=True;`

### Opción C: Docker (SQL Server Linux)
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```
Connection string: `Server=localhost,1433;Database=InstitutoDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;`

> **Nota**: El script `CreateDatabase.sql` **no existe en el repo**. Ver DDL en `docs/backend/02-base-de-datos/02-script-creacion.md` o `docs/DATABASE.md`.

---

## Frontend (Vue 3) - Referencia

> El frontend está en `instituto/Frontend/` - documentación separada.

**Requisitos**:
- Node.js 20.19.0+ / 22.12.0+ (según `package.json` engines)
- npm 10+ (incluido en Node)
- Vite 7+

**Comandos**:
```bash
cd instituto/Frontend
npm install
npm run dev      # http://localhost:5176
npm run build    # Dist para producción (type-check + vite build)
npm run preview  # Preview build
```

**No hay proxy Vite configurado** - El frontend usa `fetch` directo a `VITE_API_URL=http://localhost:5127`

---

## Puertos por Defecto

| Servicio | Puerto Dev | Puerto Prod | Configurable En |
|----------|------------|-------------|-----------------|
| Backend API | 5127 | 8080 | `ASPNETCORE_URLS` / `launchSettings.json` |
| Frontend Vite | 5176 | 80/443 (nginx) | `vite.config.ts` |
| SQL Server | 1433 (default instance) | 1433 | SQL Config Manager |
| LocalDB | Dinámico (named pipe) | N/A | `(localdb)\MSSQLLocalDB` |

---

## Verificación Rápida Pre-Despliegue

```bash
# 1. Compilar backend
cd Instituto.API
dotnet build --configuration Release

# 2. Ejecutar tests (44 tests - todos pasando)
dotnet test Instituto.sln

# 3. Verificar SQL script (no hay archivo físico - usar DDL documentado)
# Ver docs/backend/02-base-de-datos/02-script-creacion.md

# 4. Run backend
dotnet run --environment Development
# Debe mostrar: "Now listening on: http://localhost:5127"

# 5. Test endpoint salud
curl http://localhost:5127/
# "Backend corriendo correctamente!"

# 6. Test auth (crear admin si primera vez - solo Dev)
curl -X POST http://localhost:5127/api/setup/admin ...
curl -X POST http://localhost:5127/api/auth/login ...

# 7. Test CRUD
curl -H "Authorization: Bearer <token>" http://localhost:5127/api/carreras
```