# Manejo Global de Errores

## Middleware de Excepciones (Program.cs)

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new 
        { 
            error = "Ocurrió un error interno del servidor." 
        });
        await context.Response.WriteAsync(body);
    });
});
```

### Ubicación en Pipeline
```csharp
var app = builder.Build();

// 1. PRIMERO: Exception Handler (captura TODO lo de abajo)
app.UseExceptionHandler(...);

// 2. Routing
app.UseRouting();

// 3. CORS
app.UseCors("VueCors");

// 4. Auth
app.UseAuthentication();
app.UseAuthorization();

// 5. Endpoints
app.MapControllers();
```

---

## Qué Captura

| Origen | Ejemplo | Resultado |
|--------|---------|-----------|
| Controller Action | `throw new NullReferenceException()` | 500 JSON |
| Middleware posterior | Error en `UseAuthentication` | 500 JSON |
| Filtros / Model Binding | Excepción en binder personalizado | 500 JSON |
| Background Services | Excepción no atrapada en hosted service | 500 JSON (si configurado) |

---

## Qué NO Captura (Se Maneja Antes)

| Excepción | Manejada En | Status |
|-----------|-------------|--------|
| `EntityNotFoundException` | Controller `catch` | 404 |
| `PersistenceException` | Controller `catch` | 500 (con mensaje) |
| `ModelState` inválido | Controller `if (!ModelState.IsValid)` | 400 |
| `Unauthorized` (JWT) | `JwtBearerEvents.OnChallenge` | 401 |
| `Forbidden` (Policy) | `JwtBearerEvents.OnForbidden` | 403 |

---

## Formato de Respuesta Unificado

### Errores Controlados (4xx)
```json
// 400 Validation
{ "Email": ["El email es obligatorio."] }

// 400 Business Rule
{ "error": "La carrera con Id 5 no existe." }

// 401 Auth
{ "error": "No autenticado. Iniciá sesión." }

// 403 Forbidden
{ "error": "No tenés permiso para realizar esta acción." }

// 404 Not Found
{ "error": "Alumno con Id 999 no fue encontrado/a." }

// 409 Conflict
{ "error": "Ya existe al menos un administrador..." }
```

### Errores No Controlados (5xx)
```json
// 500 Global Handler
{ "error": "Ocurrió un error interno del servidor." }

// 500 Controller catch (PersistenceException)
{ "error": "Error al crear el alumno en SQL Server." }
```

---

## Logging de Errores (Recomendado)

### Agregar Serilog
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### Program.cs
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/backend-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        // LOG estructurado
        Log.Error(exception, "Unhandled exception: {Path} {Method}", 
            context.Request.Path, context.Request.Method);
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new 
        { 
            error = "Ocurrió un error interno del servidor." 
        }));
    });
});
```

### Log Output Ejemplo
```
2026-09-25 15:30:45.123 +00:00 [ERR] Unhandled exception: /api/alumnos POST
System.NullReferenceException: Object reference not set to an instance of an object.
   at Backend.Controllers.AlumnosController.Create(Alumno alumno) in ...
```

---

## Problem Details (RFC 7807) - Futuro

```csharp
// ASP.NET Core 7+ built-in
builder.Services.AddProblemDetails();

app.UseExceptionHandler();  // Auto-genera RFC 7807 response

// Response ejemplo:
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500,
  "traceId": "00-abc123...-00"
}
```

---

## Health Checks (Monitoreo)

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("SqlServer")!);

app.MapHealthChecks("/health");
```

**Response**:
```json
// Healthy
{ "status": "Healthy", "checks": [{ "name": "sql", "status": "Healthy" }] }

// Unhealthy
{ "status": "Unhealthy", "checks": [{ "name": "sql", "status": "Unhealthy", "description": "Connection timeout" }] }
```

---

## Testing Error Handling

```bash
# Forzar 500 (si endpoint tiene bug)
curl -X POST http://localhost:5127/api/alumnos \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Test"}'  # Faltan campos requeridos → 400

# Verificar formato JSON en errores
curl -i http://localhost:5127/api/alumnos/99999 \
  -H "Authorization: Bearer <token>"
# Debe retornar 404 con { "error": "..." }

# Verificar 401 sin token
curl -i http://localhost:5127/api/administradores
# Debe retornar 401 con { "error": "No autenticado. Iniciá sesión." }
```