# Roles y Permisos

## Roles Actuales

| Role | Descripción | Permisos Implícitos |
|------|-------------|---------------------|
| **Admin** | Administrador estándar | CRUD completo en todas las entidades |
| **SuperAdmin** | Super administrador | Igual que Admin + gestión de admins (futuro) |

> **Actualmente**: No hay distinción real de permisos. Cualquier `[Authorize]` permite ambos roles.

---

## Claim de Rol en JWT

```json
// En token generado por AuthController.GenerarToken()
{
  "role": "Admin"  // o "SuperAdmin"
}
```

Se mapea a `ClaimTypes.Role` → accesible via `User.IsInRole("Admin")`.

---

## Matriz de Permisos (Deseada)

| Endpoint | Admin | SuperAdmin | Público |
|----------|-------|------------|---------|
| **Auth** | | | |
| POST /api/auth/login | ✅ | ✅ | ✅ |
| POST /api/setup/admin | ❌ | ❌ | ✅ (solo Dev) |
| **Administradores** | | | |
| GET /api/administradores | ✅ | ✅ | ❌ |
| GET /api/administradores/{id} | ✅ | ✅ | ❌ |
| POST /api/administradores | ❌ | ✅ | ❌ |
| PUT /api/administradores/{id} | ❌ | ✅ | ❌ |
| DELETE /api/administradores/{id} | ❌ | ✅ | ❌ |
| **Alumnos** | | | |
| GET /api/alumnos | ✅ | ✅ | ❌ |
| GET /api/alumnos/{id} | ✅ | ✅ | ❌ |
| POST /api/alumnos | ✅ | ✅ | ✅ (inscripción) |
| PUT /api/alumnos/{id} | ✅ | ✅ | ❌ |
| DELETE /api/alumnos/{id} | ✅ | ✅ | ❌ |
| **Carreras** | | | |
| GET /api/carreras | ✅ | ✅ | ✅ |
| GET /api/carreras/{id} | ✅ | ✅ | ✅ |
| POST /api/carreras | ✅ | ✅ | ❌ |
| PUT /api/carreras/{id} | ✅ | ✅ | ❌ |
| DELETE /api/carreras/{id} | ✅ | ✅ | ❌ |
| **Profesores** | | | |
| GET /api/profesores | ✅ | ✅ | ❌ |
| GET /api/profesores/{id} | ✅ | ✅ | ❌ |
| POST /api/profesores | ✅ | ✅ | ❌ |
| PUT /api/profesores/{id} | ✅ | ✅ | ❌ |
| DELETE /api/profesores/{id} | ✅ | ✅ | ❌ |
| **Listados** | | | |
| GET /api/listado | ✅ | ✅ | ❌ |

---

## Implementación Futura: Policies

### Program.cs
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));
    
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));
    
    options.AddPolicy("CanManageAdmins", policy =>
        policy.RequireRole("SuperAdmin"));
    
    options.AddPolicy("CanManageCarreras", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));
});
```

### Uso en Controllers
```csharp
[Authorize(Policy = "RequireSuperAdmin")]
[HttpPost]
public IActionResult Create([FromBody] Administrador admin) { ... }

[Authorize(Policy = "CanManageCarreras")]
[HttpDelete("{id}")]
public IActionResult Delete(int id) { ... }
```

---

## Autorización Basada en Recursos (Resource-Based)

> Para permisos granulares (ej: admin solo ve/modifica su propia carrera)

```csharp
// Policy con handler personalizado
public class SameCarreraRequirement : IAuthorizationRequirement { }

public class SameCarreraHandler : AuthorizationHandler<SameCarreraRequirement, Alumno>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameCarreraRequirement requirement,
        Alumno resource)
    {
        var userCarreraId = int.Parse(context.User.FindFirst("carreraId")?.Value ?? "0");
        if (resource.CarreraId == userCarreraId)
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

// Uso
[Authorize(Policy = "SameCarrera")]
[HttpPut("{id}")]
public IActionResult Update(int id, [FromBody] Alumno alumno) { ... }
```

---

## Claims Adicionales Útiles

| Claim | Fuente | Uso |
|-------|--------|-----|
| `carreraId` | Admin.Alumno?.CarreraId | Filtro multi-tenant |
| `permissions` | Array strings | Permisos finos |
| `sessionId` | Guid por login | Revocación granular |

---

## Testing de Roles

```csharp
// Test helper: crear principal con roles
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, "1"),
    new Claim(ClaimTypes.Email, "admin@test.com"),
    new Claim(ClaimTypes.Role, "Admin"),
    new Claim(ClaimTypes.Name, "Admin Test"),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

var identity = new ClaimsIdentity(claims, "Test");
var principal = new ClaimsPrincipal(identity);

// En test de controller
controller.ControllerContext = new ControllerContext
{
    HttpContext = new DefaultHttpContext { User = principal }
};
```