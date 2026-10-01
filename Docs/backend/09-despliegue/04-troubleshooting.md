# Troubleshooting

## Errores Comunes y Soluciones

---

### 1. Base de Datos

#### `SqlException: Cannot open database "InstitutoDB" requested by the login.`
**Causa**: BD no existe o login sin permisos.
**Solución**:
```bash
# Verificar BD existe (LocalDB)
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name = 'InstitutoDB'"

# Verificar BD existe (SQL Auth - desarrollo actual)
sqlcmd -S localhost -U instituto_user -P "Instituto2026" -Q "SELECT name FROM sys.databases WHERE name = 'InstitutoDB'"

# Ejecutar script creación (usar DDL documentado en docs/)
sqlcmd -S "(localdb)\MSSQLLocalDB" -i create_instituto_db.sql

# Verificar permisos (producción)
# GRANT CONNECT, SELECT, INSERT, UPDATE, DELETE ON DATABASE::InstitutoDB TO [app_user];
```

#### `SqlException: Login failed for user '...'`
**Causa**: Credenciales incorrectas en connection string.
**Solución**:
- Verificar `User Id` y `Password` en `ConnectionStrings__SqlServer`
- Si Windows Auth: `Trusted_Connection=True` sin User/Password
- Si SQL Auth: `User Id=...;Password=...;Trusted_Connection=False;`

#### `SqlException: The DELETE statement conflicted with the REFERENCE constraint "FK_Alumnos_Carreras"`
**Causa**: Intentar borrar carrera con alumnos inscriptos.
**Solución**:
- Endpoint `DELETE /api/carreras/{id}` ya valida y retorna 400
- Si error directo en BD: mover alumnos a otra carrera antes de borrar

#### `Timeout expired. The timeout period elapsed prior to completion of the operation.`
**Causa**: Query lenta o bloqueos.
**Solución**:
- Aumentar `CommandTimeout` en `SqlCommand`: `cmd.CommandTimeout = 120;`
- Revisar índices faltantes (`IX_Alumnos_CarreraId`, `IX_Alumnos_DNI`, etc.)
- Verificar bloqueos: `sp_who2`, `sys.dm_tran_locks`

---

### 2. Autenticación / JWT

#### `401 { "error": "No autenticado. Iniciá sesión." }`
**Causas**:
- Token no enviado → Agregar header `Authorization: Bearer <token>`
- Token expirado (8hs) → Re-login
- Token firmado con key distinta → Verificar `Jwt__Key` mismo en todos los servicios
- Clock skew → `ClockSkew = TimeSpan.Zero` (estricto), sincronizar reloj servidor

#### `401 { "error": "Email o contraseña incorrectos." }`
**Causas**:
- Email inexistente / admin inactivo (`Activo=0`)
- Password incorrecto
- Email case-sensitive en BD → Login normaliza `.Trim().ToLower()`

#### `403 { "error": "No tenés permiso para realizar esta acción." }`
**Causa**: Token válido pero endpoint requiere policy/rol no presente.
**Solución**: Verificar `ClaimTypes.Role` en token vs `[Authorize(Roles="...")]`

#### `InvalidOperationException: Jwt:Key no configurado en appsettings.json.`
**Causa**: Variable `Jwt__Key` no seteada.
**Solución**: Setear variable de entorno / User Secrets / Key Vault.

---

### 3. Compilación / Build

#### `CS0246: El nombre del tipo o del espacio de nombres 'Npgsql' no se encontró`
**Causa**: Paquete `Npgsql` referenciado pero no instalado (legacy Supabase).
**Solución**: Ya resuelto - migración a `Microsoft.Data.SqlClient` completada.

#### `MSB3021: No se puede copiar ... Backend.exe ... being used by another process`
**Causa**: Proceso anterior aún corriendo.
**Solución**:
```bash
# Windows
taskkill /F /IM Backend.exe
# O cerrar Visual Studio / terminal corriendo dotnet run

# Linux
pkill -f "dotnet.*Backend"
```

#### `The type or namespace name 'IConfiguration' could not be found`
**Causa**: Falta `using Microsoft.Extensions.Configuration;`
**Solución**: Agregar using o `global using` en `Program.cs`.

---

### 4. Runtime / Endpoints

#### `Swagger/UI not found` (si agregado)
**Causa**: `UseSwagger`/`UseSwaggerUI` solo en Development.
**Solución**:
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

#### `CORS policy 'VueCors' not found`
**Causa**: `app.UseCors("VueCors")` antes de `AddCors` o nombre distinto.
**Solución**: Verificar orden en `Program.cs`:
```csharp
builder.Services.AddCors(...);  // REGISTRO primero
// ...
app.UseCors("VueCors");         // USO después de UseRouting
```

#### `404 en /api/setup/admin en Production`
**Comportamiento esperado**: Endpoint solo existe en `Development`.
**Solución**: Crear admin vía SQL directo o script de seed en producción.

---

### 5. Frontend (Vue) - Conexión Backend

#### `net::ERR_CONNECTION_REFUSED` en llamadas API
**Causa**: Backend no corriendo o puerto incorrecto.
**Solución**:
```bash
# Verificar backend
curl http://localhost:5127/
# Debe responder "Backend corriendo correctamente!"

# Verificar VITE_API_URL en Frontend/.env
cat Frontend/.env
# Debe ser: VITE_API_URL=http://localhost:5127
```

#### `CORS error en browser console`
**Causa**: Backend CORS no permite origen frontend.
**Solución**: 
- Dev: `AllowAnyOrigin()` ya configurado
- Prod: Agregar origen frontend a `Cors__AllowedOrigins`

---

## Logs y Diagnóstico

### Habilitar Logs Detallados
```json
// appsettings.Development.json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Information",
    "Backend": "Debug"
  }
}
```

### Ver Logs en Consola
```bash
dotnet run --environment Development
# Logs aparecen en terminal con timestamps
```

### SQL Profiler / Extended Events
```sql
-- Ver queries lentas
SELECT * FROM sys.dm_exec_query_stats 
ORDER BY total_elapsed_time DESC;
```

---

## Health Checks Manuales

```bash
# 1. API viva
curl http://localhost:5127/

# 2. BD conectada (si health check configurado)
curl http://localhost:5127/health

# 3. Auth funciona
curl -X POST http://localhost:5127/api/auth/login -d '{"email":"x","password":"y"}'
# Debe dar 401 (no 500)

# 4. Endpoint protegido
curl -H "Authorization: Bearer <token>" http://localhost:5127/api/administradores
# Debe dar 200 (no 401/403)
```

---

## Contacto / Escalación

| Nivel | Qué Incluir |
|-------|-------------|
| **Nivel 1** (Dev) | Error message, stack trace, pasos para reproducir, `dotnet --info` |
| **Nivel 2** (Infra) | Logs servidor, métricas BD, conectividad red, certificados TLS |
| **Nivel 3** (Vendor) | Azure/AWS support ticket, SQL Server error logs, .NET runtime version |

---

## Comandos Útiles de Diagnóstico

```bash
# Ver puertos en uso
netstat -ano | findstr :5127
# o
ss -tlnp | grep 5127

# Ver procesos .NET
dotnet-trace ps
# o
ps aux | grep dotnet

# Ver variables de entorno efectivas
dotnet run --environment Production --dry-run 2>&1 | head -20

# Test connection string (LocalDB)
sqlcmd -S "(localdb)\MSSQLLocalDB" -d InstitutoDB -Q "SELECT 1"

# Test connection string (SQL Auth - desarrollo)
sqlcmd -S localhost -U instituto_user -P "Instituto2026" -d InstitutoDB -Q "SELECT 1"

# Verificar JWT key length
echo -n "TuJwtKeyAqui" | wc -c  # Debe dar >= 32

# Decodificar JWT (header.payload.signature)
echo "<token>" | cut -d. -f1,2 | tr '.' '\n' | base64 -d | jq
```