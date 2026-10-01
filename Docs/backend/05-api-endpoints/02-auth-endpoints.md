# Endpoints de Autenticación

**Base Path**: `/api/auth`  
**Controller**: `AuthController`  
**Autenticación**: Pública (excepto donde se indique)

---

## POST /api/auth/login

Inicia sesión y obtiene JWT token.

### Request
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@tupac.edu.ar",
  "password": "MiPass123"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| email | string | Sí | Email válido |
| password | string | Sí | Mín 6 caracteres |

### Response 200 OK
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJhZG1pbkB0dXBhYy5lZHUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWRtaW4gUHJpbmNpcGFsIiwicm9sZSI6IkFkbWluIiwianRpIjoiZj...",
  "expiraEn": "2026-09-26T04:30:00Z",
  "admin": {
    "id": 1,
    "nombre": "Admin",
    "apellido": "Principal",
    "email": "admin@tupac.edu.ar",
    "role": "Admin"
  }
}
```

### Response 400 Bad Request (Validation)
```json
{
  "Email": ["El email es obligatorio."],
  "Password": ["La contraseña debe tener al menos 6 caracteres."]
}
```

### Response 401 Unauthorized
```json
{ "error": "Email o contraseña incorrectos." }
```

### Response 500 Internal Server Error
```json
{ "error": "Error interno al intentar iniciar sesión." }
```

---

## POST /api/auth/verify-password  🔐 **Requiere JWT**

Verifica que la contraseña actual del usuario autenticado sea correcta.  
Usado por el frontend antes de permitir cambios sensibles (ej. editar admin, cambiar password).

### Request
```http
POST /api/auth/verify-password
Authorization: Bearer <token>
Content-Type: application/json

{
  "password": "MiPassActual123"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| password | string | Sí | Contraseña actual |

### Response 200 OK
```json
{ "mensaje": "Contraseña verificada correctamente" }
```

### Response 401 Unauthorized
```json
{ "error": "Contraseña incorrecta" }
```
```json
{ "error": "Token inválido" }
```

### Response 500 Internal Server Error
```json
{ "error": "Error interno al verificar contraseña" }
```

---

## Flujo de Autenticación

```
1. Cliente → POST /api/auth/login { email, password }
           │
           ▼
2. Server: AuthController.Login()
   ├─ ModelState.IsValid?
   │   └─ No → 400
   ├─ _authService.LoginAsync(email, password)
   │   ├─ Buscar admin en BD (email + Activo=1)
   │   ├─ BCrypt.Verify(password, hash)
   │   └─ Retorna AdminResult o null
   ├─ AdminResult == null?
   │   └─ Sí → 401 "Email o contraseña incorrectos."
   └─ GenerarToken(AdminResult) → JWT
           │
           ▼
3. Cliente recibe { token, expiraEn, admin }
   └─ Guarda token (localStorage/cookie)
   └─ Usa en headers: Authorization: Bearer <token>
```

---

## JWT Token Details

**Header**: `Authorization: Bearer <token>`

**Payload (Claims)**:
```json
{
  "sub": "1",                              // NameIdentifier (Id)
  "email": "admin@tupac.edu.ar",              // Email
  "name": "Admin Principal",               // Name (Nombre + Apellido)
  "role": "Admin",                         // Role
  "jti": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",  // JWT ID único
  "exp": 1727278200,                       // Unix timestamp (8 horas)
  "iss": "InstitutoTupacAmaru",            // Issuer
  "aud": "InstitutoTupacAmaruAdmin"        // Audience
}
```

**Configuración** (`Program.cs` + `appsettings.json`):
- **Algoritmo**: HMAC-SHA256
- **Expiración**: 8 horas
- **ClockSkew**: `TimeSpan.FromMinutes(5)` (margen de gracia 5 min)
- **Key**: `Jwt:Key` (mín 32 chars)
- **Issuer**: `Jwt:Issuer`
- **Audience**: `Jwt:Audience`

---

## Uso del Token en Requests Subsecuentes

```http
GET /api/administradores
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Middleware Pipeline** (`Program.cs`):
1. `UseAuthentication()` → Valida token, popula `HttpContext.User`
2. `UseAuthorization()` → Evalúa `[Authorize]` / policies

---

## Errores de Autenticación Comunes

| Error | Causa | Solución |
|-------|-------|----------|
| `401 { "error": "No autenticado. Iniciá sesión." }` | Token faltante/expirado/inválido | Re-login |
| `403 { "error": "No tenés permiso para realizar esta acción." }` | Token válido pero sin rol/permiso | Verificar rol del usuario |
| `401 { "error": "Email o contraseña incorrectos." }` | Credenciales inválidas | Verificar email/password |
| `401 { "error": "Contraseña incorrecta" }` | verify-password falló | Verificar password actual |
| `401 { "error": "Token inválido" }` | verify-password sin claim email | Re-login |

---

## Logout

**No hay endpoint de logout** (JWT stateless).  
Cliente debe **borrar token** localmente.  
Para revocación real → implementar **blocklist/refresh tokens** (futuro).