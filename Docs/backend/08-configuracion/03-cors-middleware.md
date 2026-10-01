# CORS y Middleware

## Configuración CORS (Program.cs)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// En pipeline
app.UseCors("VueCors");
```

### Política Actual: "VueCors" (Desarrollo)

| Configuración | Valor | Efecto |
|---------------|-------|--------|
| `AllowAnyOrigin()` | `*` | Cualquier origen (frontend local, deployed, etc.) |
| `AllowAnyHeader()` | `*` | Cualquier header (Authorization, Content-Type, custom) |
| `AllowAnyMethod()` | `*` | GET, POST, PUT, DELETE, PATCH, OPTIONS |

> **Advertencia**: `AllowAnyOrigin()` **no compatible** con `AllowCredentials()` (cookies/auth). Si se usa SignalR o cookies → especificar orígenes exactos.

---

## Pipeline de Middleware Completo

```csharp
var app = builder.Build();

// 1. Exception Handler (GLOBAL - primero)
app.UseExceptionHandler(errorApp => { ... });

// 2. Routing
app.UseRouting();

// 3. CORS (después de Routing, antes de Auth)
app.UseCors("VueCors");

// 4. Authentication (JWT validation)
app.UseAuthentication();

// 5. Authorization (Policies/Roles)
app.UseAuthorization();

// 6. Endpoints (Controllers)
app.MapControllers();

// 7. Health Check (opcional)
app.MapGet("/", () => "Backend corriendo correctamente!");
app.MapHealthChecks("/health");  // Si configurado
```

### Orden Crítico

```
UseExceptionHandler
    ↓
UseRouting
    ↓
UseCors          ← Debe estar ENTRE Routing y Auth
    ↓
UseAuthentication
    ↓
UseAuthorization
    ↓
MapControllers
```

| Middleware | Posición | Por Qué |
|------------|----------|---------|
| `UseExceptionHandler` | **Primero** | Captura errores de TODO lo posterior |
| `UseRouting` | 2do | Necesario para CORS y Auth |
| `UseCors` | **Después de Routing, antes de Auth** | Requiere route data, setea headers preflight |
| `UseAuthentication` | Después de CORS | Valida token, popula `HttpContext.User` |
| `UseAuthorization` | Después de Auth | Evalúa policies sobre `User` |
| `MapControllers` | **Último** | Ejecuta endpoints |

---

## Preflight Requests (OPTIONS)

### Flujo Automático
```
Frontend (Vue)                    Backend (ASP.NET Core)
     │                                   │
     ├──── OPTIONS /api/alumnos ───────▶│
     │  Origin: http://localhost:5176   │
     │  Access-Control-Request-Method: POST
     │                                   │
     │◀─── 204 No Content ──────────────┤
     │  Access-Control-Allow-Origin: *
     │  Access-Control-Allow-Methods: GET,POST,PUT,DELETE
     │  Access-Control-Allow-Headers: *
     │                                   │
     ├──── POST /api/alumnos ──────────▶│
     │  Authorization: Bearer <token>   │
     │                                   │
     │◀─── 201 Created ─────────────────┤
```

### Headers de Respuesta CORS

| Header | Valor (Actual) | Descripción |
|--------|----------------|-------------|
| `Access-Control-Allow-Origin` | `*` | Origen permitido |
| `Access-Control-Allow-Methods` | `GET,POST,PUT,DELETE,OPTIONS` | Métodos permitidos |
| `Access-Control-Allow-Headers` | `*` | Headers permitidos en request |
| `Access-Control-Max-Age` | (default) | Cache preflight (segundos) |

---

## Configuración para Producción

### Opción A: Orígenes Específicos (Recomendado)
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCors", policy =>
    {
        policy
            .WithOrigins(
                "https://tupac.edu",
                "https://www.tupac.edu",
                "https://admin.tupac.edu"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            // .AllowCredentials()  // Solo si usas cookies/SignalR
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});
```

### Opción B: Subdominios Wildcard (Si muchos)
```csharp
policy.SetIsOriginAllowedToAllowWildcardSubdomains()
      .WithOrigins("https://*.tupac.edu")
      .AllowAnyHeader()
      .AllowAnyMethod();
```

### En Pipeline (Condicional por Ambiente)
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseCors("VueCors");
}
else
{
    app.UseCors("ProductionCors");
}
```

---

## Otros Middleware Útiles (No Configurados)

| Middleware | Paquete | Uso |
|------------|---------|-----|
| `UseHttpsRedirection` | Built-in | HTTP → HTTPS redirect |
| `UseStaticFiles` | Built-in | Servir wwwroot (imágenes, CSS) |
| `UseResponseCompression` | `Microsoft.AspNetCore.ResponseCompression` | Gzip/Brotli |
| `UseRateLimiter` | `Microsoft.AspNetCore.RateLimiting` | Rate limiting |
| `UseRequestLocalization` | Built-in | i18n / cultura |
| `UseSerilogRequestLogging` | `Serilog.AspNetCore` | Request logging estructurado |

### Ejemplo: Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

app.UseRateLimiter();
// En controller: [EnableRateLimiting("ApiPolicy")]
```

### Ejemplo: Response Compression
```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

app.UseResponseCompression();
```

---

## Testing CORS

```bash
# Preflight manual
curl -i -X OPTIONS http://localhost:5127/api/alumnos \
  -H "Origin: http://localhost:5176" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Content-Type,Authorization"

# Verificar headers
# Debe incluir:
# Access-Control-Allow-Origin: *
# Access-Control-Allow-Methods: GET,POST,PUT,DELETE,OPTIONS
# Access-Control-Allow-Headers: Content-Type,Authorization
```