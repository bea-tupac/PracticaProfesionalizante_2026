# Variables de Producción

## Checklist de Variables Críticas

| Variable | Valor Requerido | Validación |
|----------|-----------------|------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Exacto |
| `ASPNETCORE_URLS` | `http://0.0.0.0:8080` | Puerto expuesto |
| `ConnectionStrings__SqlServer` | Ver abajo | Test conectividad |
| `Jwt__Key` | 32+ chars, alta entropía | `openssl rand -base64 32` |
| `Jwt__Issuer` | `InstitutoTupacAmaru` | Coincide con token |
| `Jwt__Audience` | `InstitutoTupacAmaruAdmin` | Coincide con token |

---

## Connection String Producción

```bash
# SQL Server con autenticación SQL + TLS
ConnectionStrings__SqlServer="Server=prod-sql.tupac.edu;Database=InstitutoDB;User Id=instituto_app;Password=Str0ngP@ssw0rd!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# O con Managed Identity (Azure SQL)
ConnectionStrings__SqlServer="Server=tcp:prod-sql.database.windows.net,1433;Database=InstitutoDB;Authentication=Active Directory Managed Identity;Encrypt=True;"
```

### Parámetros Clave

| Parámetro | Valor Prod | Descripción |
|-----------|------------|-------------|
| `Encrypt` | `True` | TLS obligatorio |
| `TrustServerCertificate` | `False` | Validar certificado CA |
| `Connection Timeout` | `30` | Segundos |
| `Command Timeout` | (default 30) | En código: `cmd.CommandTimeout = 60;` |

---

## JWT Key Generación

```bash
# OpenSSL (Linux/Mac/WSL/Git Bash)
openssl rand -base64 32
# Ej: K7gNuZs9xQm3pL2vR5tY8wX1zA4bC6dE9fG2hJ0kL1m=

# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))

# C# (una vez en consola)
dotnet script -c "Console.WriteLine(Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));"
```

**Guardar en**: Azure Key Vault / AWS Secrets Manager / HashiCorp Vault / `.env` (solo si Docker local)

---

## Logging Producción

```bash
Logging__LogLevel__Default=Warning
Logging__LogLevel__Microsoft=Warning
Logging__LogLevel__Microsoft__AspNetCore=Error
Logging__LogLevel__Backend=Information  # Tu namespace
```

### Serilog (Si Configurado)
```bash
Serilog__MinimumLevel__Default=Warning
Serilog__MinimumLevel__Override__Microsoft=Warning
Serilog__WriteTo__0__Name=Console
Serilog__WriteTo__1__Name=File
Serilog__WriteTo__1__Args__path=logs/backend-.log
Serilog__WriteTo__1__Args__rollingInterval=Day
Serilog__WriteTo__1__Args__retainedFileCountLimit=30
```

---

## CORS Producción

```bash
# Si se usa política con orígenes específicos
Cors__AllowedOrigins__0=https://tupac.edu
Cors__AllowedOrigins__1=https://www.tupac.edu
Cors__AllowedOrigins__2=https://admin.tupac.edu
```

En código:
```csharp
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? ["https://tupac.edu"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

## Rate Limiting (Si Configurado)

```bash
RateLimiting__ApiPolicy__PermitLimit=100
RateLimiting__ApiPolicy__WindowMinutes=1
RateLimiting__AuthPolicy__PermitLimit=5
RateLimiting__AuthPolicy__WindowMinutes=1
```

---

## Health Checks

```bash
HealthChecks__SqlServer__ConnectionString=Server=prod-sql;Database=InstitutoDB;User Id=...;Password=...;
```

---

## Archivo .env.production (Ejemplo Docker)

```env
# .env.production - NO COMMITEAR
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080

ConnectionStrings__SqlServer=Server=sqlserver;Database=InstitutoDB;User Id=sa;Password=SqlPass123!;TrustServerCertificate=True;

Jwt__Key=ProductionJwtKeyMustBeVeryLongAndRandom32CharsMinimum
Jwt__Issuer=InstitutoTupacAmaru
Jwt__Audience=InstitutoTupacAmaruAdmin

Logging__LogLevel__Default=Warning
Logging__LogLevel__Microsoft=Warning
Logging__LogLevel__Backend=Information

AllowedHosts=api.tupac.edu
```

---

## Validación Automática en Startup

```csharp
// Program.cs - Agregar al inicio
var requiredEnvVars = new[]
{
    "ConnectionStrings__SqlServer",
    "Jwt__Key",
    "Jwt__Issuer",
    "Jwt__Audience"
};

foreach (var envVar in requiredEnvVars)
{
    if (string.IsNullOrEmpty(builder.Configuration[envVar]))
    {
        var msg = $"Variable de entorno requerida no configurada: {envVar}";
        throw new InvalidOperationException(msg);
    }
}

if (builder.Configuration["Jwt__Key"]!.Length < 32)
    throw new InvalidOperationException("Jwt:Key debe tener al menos 32 caracteres en producción.");

// Test BD conectividad al inicio
using var scope = app.Services.CreateScope();
var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
var connStr = config.GetConnectionString("SqlServer");
using var testConn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
await testConn.OpenAsync();  // Falla rápido si no conecta
```

---

## Rotación de Secrets

| Secret | Frecuencia | Proceso |
|--------|------------|---------|
| `Jwt__Key` | 90 días | 1. Generar nueva key 2. Deploy 3. Tokens existentes invalidados (usuarios re-login) |
| SQL Password | 90 días | 1. Cambiar en SQL 2. Actualizar secret 3. Rolling restart pods |
| TLS Cert | 1 año | Renovación automática (Let's Encrypt / ACME) |

---

## Diferencias Dev vs Prod

| Config | Development | Production |
|--------|-------------|------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `Jwt__Key` | User Secrets / appsettings.Development.json | Key Vault / Env Var |
| `ConnectionStrings__SqlServer` | LocalDB (Trusted) | SQL Server (User/Pass + TLS) |
| `Logging` | Debug/Information | Warning/Error |
| `AllowedHosts` | `*` | `api.tupac.edu` |
| `CORS` | `AllowAnyOrigin()` | Orígenes específicos |
| `Setup /api/setup/admin` | Habilitado | **404 (deshabilitado)** |
| `UseDeveloperExceptionPage` | Sí | No |
| `TrustServerCertificate` | `True` | `False` (cert válido) |