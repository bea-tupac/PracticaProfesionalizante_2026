# Configuración (AppSettings)

## Archivos de Configuración

| Archivo | Propósito | Prioridad |
|---------|-----------|-----------|
| `appsettings.json` | Config base (compartida) | Baja |
| `appsettings.Development.json` | Overrides desarrollo | Alta (si `ASPNETCORE_ENVIRONMENT=Development`) |
| `appsettings.Production.json` | Overrides producción | Alta (si `ASPNETCORE_ENVIRONMENT=Production`) |
| Variables de entorno | Secrets, overrides finales | **Máxima** |
| User Secrets | Secrets desarrollo local | Alta (solo dev) |

---

## appsettings.json (Base)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },

  "Jwt": {
    "Key": "REEMPLAZAR_CON_CLAVE_SECRETA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "InstitutoTupacAmaru",
    "Audience": "InstitutoTupacAmaruAdmin"
  }
}
```

### Secciones

#### Logging
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "Backend": "Debug"  // Namespace propio
  }
}
```
- `Default`: Nivel mínimo para todos
- Por namespace: `Backend.Controllers` → `Debug`

#### AllowedHosts
```json
"AllowedHosts": "*"
```
- `*`: Acepta cualquier host header (desarrollo)
- Producción: `"AllowedHosts": "api.tupac.edu,localhost"`

#### ConnectionStrings
```json
"ConnectionStrings": {
  "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
- **Key**: `"SqlServer"` (usado en `config.GetConnectionString("SqlServer")`)
- **LocalDB**: `(localdb)\MSSQLLocalDB` (instancia automática VS/Windows)
- **Trusted_Connection**: Autenticación Windows Integrada
- **TrustServerCertificate**: Acepta certificado self-signed (dev)

#### Jwt
```json
"Jwt": {
  "Key": "REEMPLAZAR_CON_CLAVE_SECRETA_DE_AL_MENOS_32_CARACTERES",
  "Issuer": "InstitutoTupacAmaru",
  "Audience": "InstitutoTupacAmaruAdmin"
}
```
| Campo | Descripción | Requisitos |
|-------|-------------|------------|
| `Key` | Clave simétrica HMAC-SHA256 | **Mín 32 chars** (256 bits), alta entropía |
| `Issuer` | Emisor del token | Identificador único app |
| `Audience` | Destinatario esperado | Validado en cliente/servidor |

---

## appsettings.Development.json (SQL Auth - Usado Actualmente)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

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

- **Connection String**: SQL Server Authentication (usuario `instituto_user`)
- **Jwt.Key real** (solo desarrollo)
- Logging más verbose si se desea

> **Nota**: El archivo `appsettings.Development.json` **sí está en el repo** con credenciales de desarrollo. En producción usar variables de entorno / Key Vault.

---

## Variables de Entorno (Override)

Convención: `__` (doble underscore) para anidación.

```bash
# Connection String
export ConnectionStrings__SqlServer="Server=prod-sql;Database=InstitutoDB;User Id=sa;Password=StrongPass123;TrustServerCertificate=True;"

# JWT
export Jwt__Key="ClaveSuperSecretaDeProduccionCon32CharsMinimo!!"
export Jwt__Issuer="InstitutoTupacAmaru"
export Jwt__Audience="InstitutoTupacAmaruAdmin"

# Logging
export Logging__LogLevel__Default="Warning"
export Logging__LogLevel__Microsoft__AspNetCore="Error"

# ASP.NET Core
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="http://0.0.0.0:8080"
```

### En Azure App Service / AWS / Docker
- Configurar en "Configuration" / "Environment Variables" del portal
- No commitear secrets en repositorio

---

## User Secrets (Desarrollo Local)

```bash
# Inicializar
cd backend
dotnet user-secrets init

# Setear secrets
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=(localdb)\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;"
dotnet user-secrets set "Jwt:Key" "ClaveDesarrolloLocal32CharsMinimo!!!"

# Listar
dotnet user-secrets list
```

- Almacenado en `%APPDATA%\Microsoft\UserSecrets\<project-guid>\secrets.json` (Windows)
- `~/.microsoft/usersecrets/<project-guid>/secrets.json` (Linux/Mac)
- **No se commitea** (gitignored automáticamente)

---

## Acceso en Código

### IConfiguration (Inyección)
```csharp
public class AdminAuthService(IConfiguration config)
{
    private readonly string _connectionString = config.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("...");

    private readonly string _jwtKey = config["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key no configurado.");
}
```

### Options Pattern (Tipado Fuerte) - Futuro
```csharp
// Clase POCO
public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
}

// Registro
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// Uso
public class AuthController(IOptions<JwtSettings> jwtOptions)
{
    var key = jwtOptions.Value.Key;
}
```

---

## Validación Temprana (Fail Fast)

```csharp
// Program.cs
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurado en appsettings.json.");

var connStr = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer no configurado.");
```

- Falla **al iniciar** la app, no en primer request
- Mensajes claros de qué falta

---

## .gitignore Recomendado

```gitignore
# Configuración sensible
**/appsettings.*.json
!**/appsettings.json
!**/appsettings.example.json

# User Secrets
**/secrets.json

# Environment
.env
.env.local
.env.*.local
```

---

## Checklist por Ambiente

| Config | Development | Staging | Production |
|--------|-------------|---------|------------|
| `ConnectionStrings:SqlServer` | SQL Auth (`instituto_user`) | SQL Server Staging | SQL Server Prod |
| `Jwt:Key` | appsettings.Development.json (32 chars) | Env Var / Key Vault | Env Var / Key Vault |
| `Logging:LogLevel:Default` | Information | Warning | Warning/Error |
| `AllowedHosts` | `*` | `staging.tupac.edu` | `api.tupac.edu` |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Staging` | `Production` |
| `ASPNETCORE_URLS` | `http://localhost:5127` | `http://0.0.0.0:8080` | `http://0.0.0.0:8080` |