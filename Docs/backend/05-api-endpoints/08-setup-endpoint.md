# Endpoint de Setup Inicial

**Base Path**: `/api/setup`  
**Controller**: `SetupController`  
**Autenticación**: **Ninguna** (pero restringido a **Development** environment)

---

## POST /api/setup/admin

Crea el **primer administrador** del sistema. Solo disponible si no existe ningún admin.

### Request
```http
POST /api/setup/admin
Content-Type: application/json

{
  "nombre": "Super",
  "apellido": "Admin",
  "email": "admin@tupac.edu.ar",
  "password": "Tupac123",
  "role": "SuperAdmin"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| nombre | string | Sí | Max 100 |
| apellido | string | Sí | Max 100 |
| email | string | Sí | Email válido |
| password | string | Sí | Mín 8 chars |
| role | string | No | Default "Admin" |

### Response 201 Created
```json
{
  "isSuccess": true,
  "message": "Operación exitosa",
  "data": "Administrador 'admin@tupac.edu.ar' creado correctamente. Este endpoint ya no puede volver a usarse."
}
```

### Response 400 Bad Request
```json
// Validación ModelState
{ "Password": ["La contraseña debe tener al menos 8 caracteres."] }
```

### Response 404 Not Found
```json
// En producción (no Development)
{ "error": "No encontrado." }
```

### Response 409 Conflict
```json
// Ya existe al menos un admin
{ "error": "Ya existe al menos un administrador. Este endpoint está deshabilitado." }
```

### Response 500 Internal Server Error
```json
{ "error": "Error al crear el administrador inicial." }
```

---

## Guardas de Seguridad

### 1. Solo Environment Development
```csharp
if (!_env.IsDevelopment())
    return NotFound();  // 404 en Staging/Production
```

### 2. Solo Si No Hay Admins
```csharp
if (await _authService.HayAdminsAsync())
    return Conflict(new { error = "Ya existe al menos un administrador..." });
```

### 3. Validación ModelState
```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);
```

---

## Flujo de Uso Típico

```bash
# 1. Levantar backend en Development
dotnet run --environment Development

# 2. Crear primer admin (una sola vez)
curl -X POST http://localhost:5127/api/setup/admin \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Super",
    "apellido": "Admin",
    "email": "admin@tupac.edu.ar",
    "password": "Tupac123",
    "role": "SuperAdmin"
  }'

# 3. Verificar login
curl -X POST http://localhost:5127/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@tupac.edu.ar", "password": "Tupac123"}'
```

---

## Por Qué Este Diseño

| Preocupación | Solución |
|--------------|----------|
| **Bootstrapping** | Primer admin sin chicken-egg problem |
| **Seguridad** | Solo Dev environment + solo si BD vacía de admins |
| **Auditoría** | Log claro de cuándo y quién creó el primer admin |
| **Simplicidad** | Un endpoint, una vez, sin UI requerida |

---

## Alternativas para Producción

| Enfoque | Descripción |
|---------|-------------|
| **Script SQL** | `INSERT INTO Administradores ...` directo en BD |
| **Seed Data** | `HasData()` en EF Core migration (si se migra a EF) |
| **Admin CLI** | Comando `dotnet run -- seed-admin` |
| **Setup Wizard** | Frontend stepper que llama a este endpoint |

---

## Testing

```csharp
// Test: Crear primer admin en Dev
var controller = new SetupController(mockAuthService.Object, mockEnv.Object);
mockEnv.Setup(e => e.IsDevelopment()).Returns(true);
mockAuthService.Setup(a => a.HayAdminsAsync()).ReturnsAsync(false);

var result = await controller.CrearPrimerAdmin(new SetupAdminDto { ... });
Assert.IsType<OkObjectResult>(result);

// Test: 409 si ya existe admin
mockAuthService.Setup(a => a.HayAdminsAsync()).ReturnsAsync(true);
var result2 = await controller.CrearPrimerAdmin(...);
Assert.IsType<ConflictObjectResult>(result2);

// Test: 404 en Production
mockEnv.Setup(e => e.IsDevelopment()).Returns(false);
var result3 = await controller.CrearPrimerAdmin(...);
Assert.IsType<NotFoundResult>(result3);
```