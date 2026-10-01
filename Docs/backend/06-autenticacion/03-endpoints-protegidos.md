# Protección de Endpoints

## Estado Actual

| Endpoint | Atributo | Acceso |
|----------|----------|--------|
| `/api/auth/login` | *(ninguno)* | Público |
| `/api/setup/admin` | *(ninguno)* + `_env.IsDevelopment()` | Solo Dev, sin auth |
| `/api/administradores/**` | `[Authorize]` en clase | Requiere JWT válido |
| `/api/alumnos` (GET, PUT, DELETE) | `[Authorize]` en clase | Requiere JWT válido |
| `/api/alumnos` (POST) | `[AllowAnonymous]` | Público |
| `/api/carreras` (GET) | *(ninguno)* | Público |
| `/api/carreras` (POST, PUT, DELETE) | `[Authorize]` | Requiere JWT válido |
| `/api/profesores/**` | `[Authorize]` en clase | Requiere JWT válido |
| `/api/listado` | `[Authorize]` en clase | Requiere JWT válido |

---

## Middleware Pipeline (Program.cs)

```csharp
var app = builder.Build();

// 1. Exception Handler (global)
app.UseExceptionHandler(...);

// 2. Routing
app.UseRouting();

// 3. CORS
app.UseCors("VueCors");

// 4. Authentication (JWT validation)
app.UseAuthentication();

// 5. Authorization (policies/roles)
app.UseAuthorization();

// 6. Endpoints
app.MapControllers();
```

**Orden crítico**: `UseAuthentication` → `UseAuthorization` → `MapControllers`

---

## Cómo Funciona la Protección

### 1. Request llega con header
```
GET /api/administradores
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 2. UseAuthentication()
- Extrae token del header `Authorization`
- Valida firma, issuer, audience, lifetime
- Si válido → crea `ClaimsPrincipal` con claims
- Setea `HttpContext.User = principal`
- Si inválido → invoca `OnChallenge` (401 JSON)

### 3. UseAuthorization()
- Evalúa `[Authorize]` / `[AllowAnonymous]` / Policies
- Si `[Authorize]` y `User.Identity.IsAuthenticated == false` → invoca `OnForbidden` (403)
- Si policy falla → 403

### 4. Controller Action ejecuta
- `User` disponible con claims
- `User.FindFirst(ClaimTypes.Role)?.Value` → "Admin"

---

## Atributos de Autorización

### [Authorize]
- Requiere usuario autenticado (cualquier rol)
- Aplicable a clase o método
- **Herencia**: Si en clase, aplica a todos los métodos

### [AllowAnonymous]
- **Salta** autorización (pero authentication aún corre)
- Útil para endpoints públicos en controller protegido
- Ej: `AlumnosController` tiene `[Authorize]` en clase, pero `POST` tiene `[AllowAnonymous]`

### [Authorize(Roles = "X")]
- Requiere claim `role` = "X"
- Múltiples roles: `[Authorize(Roles = "Admin,SuperAdmin")]`

### [Authorize(Policy = "NombrePolicy")]
- Usa policy definida en `AddAuthorization`
- Más flexible que roles

---

## Casos Especiales

### 1. Endpoint Público en Controller Protegido
```csharp
[Authorize]  // Clase
[ApiController]
[Route("api/alumnos")]
public class AlumnosController : ControllerBase
{
    [AllowAnonymous]  // Override para este método
    [HttpPost]
    public IActionResult Create([FromBody] Alumno alumno) { ... }
}
```

### 2. Endpoint Solo Development
```csharp
[HttpPost("admin")]
public async Task<IActionResult> CrearPrimerAdmin([FromBody] SetupAdminDto dto)
{
    if (!_env.IsDevelopment())  // Guard en código
        return NotFound();       // 404 en Prod/Staging
    // ...
}
```

### 3. Validación Manual en Action
```csharp
[HttpPut("{id}")]
public IActionResult Update(int id, [FromBody] Alumno alumno)
{
    // Verificar ownership / permisos custom
    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    if (currentUserId != id && !User.IsInRole("SuperAdmin"))
        return Forbid();  // 403
    
    // ...
}
```

---

## Respuestas de Error Estándar

### 401 Unauthorized (Token inválido/ausente)
```json
{ "error": "No autenticado. Iniciá sesión." }
```
**Headers**: `WWW-Authenticate: Bearer`

### 403 Forbidden (Token válido pero sin permisos)
```json
{ "error": "No tenés permiso para realizar esta acción." }
```

---

## Testing de Protección

```bash
# Sin token → 401
curl -i http://localhost:5127/api/administradores

# Token inválido → 401
curl -i -H "Authorization: Bearer token.invalido.xxx" http://localhost:5127/api/administradores

# Token válido → 200
curl -i -H "Authorization: Bearer <token_valido>" http://localhost:5127/api/administradores

# POST público (alumnos) → 201
curl -i -X POST http://localhost:5127/api/alumnos \
  -H "Content-Type: application/json" \
  -d '{...}'
```

---

## Checklist de Seguridad por Endpoint

| Endpoint | Auth | Rate Limit | Input Validation | Output Sanitization | Audit Log |
|----------|------|------------|------------------|---------------------|-----------|
| POST /auth/login | ❌ | ✅ Requerido | ✅ ModelState | ✅ No expone hash | ✅ Login attempts |
| POST /setup/admin | ❌ (Dev) | ✅ | ✅ | ✅ | ✅ |
| GET /administradores | ✅ | | ✅ | ✅ | |
| POST /administradores | ✅ | | ✅ | ✅ | ✅ |
| GET /alumnos | ✅ | | ✅ | ✅ | |
| POST /alumnos | ❌ | ✅ Requerido | ✅ | ✅ | ✅ Inscripciones |
| GET /carreras | ❌ | | ✅ | ✅ | |
| POST /carreras | ✅ | | ✅ | ✅ | ✅ |
| GET /listado | ✅ | | ✅ | ✅ | |

---

## Mejoras Pendientes

- [ ] Rate limiting middleware (`Microsoft.AspNetCore.RateLimiting`)
- [ ] Policy-based authorization granular
- [ ] Audit logging (Serilog + seq/elastic)
- [ ] Refresh token rotation
- [ ] Token revocation (blocklist en Redis/DB)