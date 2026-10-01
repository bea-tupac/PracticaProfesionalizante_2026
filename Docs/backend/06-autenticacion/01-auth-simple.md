# JWT Authentication

## Visión General

El backend usa **JWT Bearer Tokens** para autenticación stateless.  
Implementación nativa ASP.NET Core (`Microsoft.AspNetCore.Authentication.JwtBearer`).

---

## Configuración

### Program.cs (Extracto)

```csharp
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurado en appsettings.json.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer           = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidateAudience         = true,
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero   // Sin margen de gracia
        };

        // Respuestas JSON en lugar de HTML para 401/403
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx => { /* 401 JSON */ },
            OnForbidden = ctx => { /* 403 JSON */ }
        };
    });

builder.Services.AddAuthorization();
```

### appsettings.json

```json
{
  "Jwt": {
    "Key": "REEMPLAZAR_CON_CLAVE_SECRETA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "InstitutoTupacAmaru",
    "Audience": "InstitutoTupacAmaruAdmin"
  }
}
```

---

## Generación de Token (AuthController)

```csharp
private string GenerarToken(AdminResult admin)
{
    var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub,   admin.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, admin.Email),
        new Claim(ClaimTypes.Name,               $"{admin.Nombre} {admin.Apellido}"),
        new Claim(ClaimTypes.Role,               admin.Role),
        new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer:            _config["Jwt:Issuer"],
        audience:          _config["Jwt:Audience"],
        claims:            claims,
        expires:           DateTime.UtcNow.AddHours(8),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Claims Incluidos

| Claim | Key | Valor | Uso |
|-------|-----|-------|-----|
| `sub` | `JwtRegisteredClaimNames.Sub` | `admin.Id` | Identificador único usuario |
| `email` | `JwtRegisteredClaimNames.Email` | `admin.Email` | Contacto / lookup |
| `name` | `ClaimTypes.Name` | `"Nombre Apellido"` | Display name |
| `role` | `ClaimTypes.Role` | `admin.Role` | Autorización (`[Authorize(Roles="Admin")]`) |
| `jti` | `JwtRegisteredClaimNames.Jti` | `Guid.NewGuid()` | Token ID único (revocación futura) |
| `exp` | Auto | `DateTime.UtcNow.AddHours(8)` | Expiración 8hs |
| `iss` | Auto | `Jwt:Issuer` | Validador |
| `aud` | Auto | `Jwt:Audience` | Validador |

---

## Validación de Token (Middleware)

`TokenValidationParameters` configurados:

| Parámetro | Valor | Descripción |
|-----------|-------|-------------|
| `ValidateIssuerSigningKey` | `true` | Verifica firma con `Jwt:Key` |
| `IssuerSigningKey` | `SymmetricSecurityKey(key)` | Clave simétrica HMAC-SHA256 |
| `ValidateIssuer` | `true` | Valida `iss` claim = `Jwt:Issuer` |
| `ValidIssuer` | `Jwt:Issuer` | "InstitutoTupacAmaru" |
| `ValidateAudience` | `true` | Valida `aud` claim = `Jwt:Audience` |
| `ValidAudience` | `Jwt:Audience` | "InstitutoTupacAmaruAdmin" |
| `ValidateLifetime` | `true` | Valida `exp` < now |
| `ClockSkew` | `TimeSpan.Zero` | **Sin tolerancia** (estricto) |

---

## Pipeline de Autenticación

```
Request → UseAuthentication() → UseAuthorization() → Controller
              │                      │
              ▼                      ▼
       Valida JWT              Evalúa [Authorize]
       Popula User             / Policies
       (ClaimsPrincipal)       
```

### HttpContext.User Resultante

```csharp
// En Controller después de auth exitosa
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  // "1"
var email  = User.FindFirst(ClaimTypes.Email)?.Value;           // "admin@test.com"
var role   = User.FindFirst(ClaimTypes.Role)?.Value;            // "Admin"
var name   = User.FindFirst(ClaimTypes.Name)?.Value;            // "Admin Principal"
var jti    = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value; // "guid..."
```

---

## Respuestas de Error Personalizadas

### 401 Unauthorized (OnChallenge)
```json
{ "error": "No autenticado. Iniciá sesión." }
```

### 403 Forbidden (OnForbidden)
```json
{ "error": "No tenés permiso para realizar esta acción." }
```

**Implementación**:
```csharp
options.Events = new JwtBearerEvents
{
    OnChallenge = ctx =>
    {
        ctx.HandleResponse();  // Evita default redirect/login page
        ctx.Response.StatusCode = 401;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = "No autenticado. Iniciá sesión." }));
    },
    OnForbidden = ctx =>
    {
        ctx.Response.StatusCode = 403;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = "No tenés permiso para realizar esta acción." }));
    }
};
```

---

## Protección de Endpoints

### Nivel Clase (Todos los métodos)
```csharp
[Authorize]
[ApiController]
[Route("api/administradores")]
public class AdministradorController : ControllerBase { ... }
```

### Nivel Método (Override)
```csharp
[AllowAnonymous]  // Público
[HttpPost]
public IActionResult Create([FromBody] Alumno alumno) { ... }

[Authorize]  // Explícito (redundante si clase tiene)
[HttpGet("{id}")]
public IActionResult GetById(int id) { ... }
```

### Por Rol (Futuro)
```csharp
[Authorize(Roles = "SuperAdmin")]
[HttpDelete("{id}")]
public IActionResult Delete(int id) { ... }

// O con Policy
[Authorize(Policy = "CanDeleteAdmins")]
```

---

## Seguridad de la Clave (Jwt:Key)

| Requisito | Implementación |
|-----------|----------------|
| **Longitud mínima** | 32 caracteres (256 bits para HS256) |
| **Entropía** | Aleatorio, no diccionario |
| **Rotación** | Cambiar periódicamente (invalida tokens existentes) |
| **Almacenamiento** | `appsettings.Development.json` (local), **Azure Key Vault / AWS Secrets Manager / env vars** (prod) |
| **Nunca en Git** | `appsettings.json` tiene placeholder, `.gitignore` excluye `appsettings.*.json` |

### Generar Clave Segura
```bash
# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))

# Bash/OpenSSL
openssl rand -base64 32

# C# (una vez)
var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
```

---

## Expiración y Refresh (Futuro)

**Actual**: Token 8hs, sin refresh → usuario debe re-loguear.

**Recomendado**:
- Access Token: 15-30 min
- Refresh Token: 7-30 días (rotating, stored in DB)
- Endpoint `/api/auth/refresh` → nuevo access + refresh

---

## Testing Tokens

```bash
# Decodificar (sin verificar firma)
echo "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IkFkbWluIFByaW5jaXBhbCIsInJvbGUiOiJBZG1pbiJ9.xxxxx" | cut -d. -f2 | base64 -d | jq

# Verificar expiración
# exp = Unix timestamp → date -d @1727278200
```