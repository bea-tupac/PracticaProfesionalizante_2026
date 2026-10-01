# Servicio de Autenticación (AdministradorService)

## Visión General

**Archivo**: `Instituto.BR/Services/AdministradorService.cs`  
**Interface**: `IAdministradorService`  
**Responsabilidad**: Gestión completa de administradores (CRUD + Auth + Password management).

> **Nota**: El servicio anterior `AdminAuthService` (con `IAdminAuthService`) fue refactorizado. Ahora toda la lógica está en `AdministradorService`.

---

## Interface (`IAdministradorService`)

```csharp
public interface IAdministradorService
{
    List<Administrador> GetAll();
    Administrador? GetById(int id);
    ServiceResult<Administrador> Create(Administrador admin, string password);
    ServiceResult<Administrador> Update(int id, Administrador admin);
    ServiceResult ChangePassword(int id, string passwordActual, string nuevaPassword);
    ServiceResult Delete(int id);
    AdminResult? Login(string email, string password);
    bool HayAdmins();
    Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto);
    ServiceResult ChangePasswordByEmail(string email, string passwordActual, string nuevaPassword);
}
```

---

## Métodos Principales

### 1. Login (Síncrono)

```csharp
public AdminResult? Login(string email, string password)
{
    var admin = _repository.GetByEmail(email);
    if (admin == null) return null;
    if (!BCryptNet.Verify(password, admin.PasswordHash)) return null;
    
    return new AdminResult(admin.Id, admin.Nombre, admin.Apellido, admin.Email, admin.Role);
}
```

**Flujo**:
1. Buscar admin por email (case-insensitive, solo activos - repositorio filtra `Activo = 1`)
2. Si no existe → `null`
3. Verificar password con `BCrypt.Verify(plain, hash)`
4. Si inválido → `null`
5. Retornar `AdminResult` (record inmutable sin hash)

**Seguridad**:
- **Timing attack**: BCrypt tiene tiempo constante
- **Email normalization**: `.Trim().ToLower()` en repositorio antes de query
- **Soft delete**: `AND Activo = 1` evita login de admins "borrados"
- **Síncrono**: Repositorio ADO.NET es sync, no usa `async/await`

---

### 2. Create (con Password)

```csharp
public ServiceResult<Administrador> Create(Administrador admin, string password)
{
    // Validaciones
    if (string.IsNullOrWhiteSpace(admin.Nombre))
        return ServiceResult<Administrador>.Fail("El nombre es obligatorio.");
    if (string.IsNullOrWhiteSpace(admin.Apellido))
        return ServiceResult<Administrador>.Fail("El apellido es obligatorio.");
    if (string.IsNullOrWhiteSpace(admin.Email))
        return ServiceResult<Administrador>.Fail("El email es obligatorio.");
    if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        return ServiceResult<Administrador>.Fail("La contraseña debe tener al menos 8 caracteres.");

    // Normalizar ANTES de chequear duplicados
    admin.Email = admin.Email.ToLower().Trim();

    if (_repository.ExistsByEmail(admin.Email))
        return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");

    admin.PasswordHash = BCryptNet.HashPassword(password, workFactor: 12);
    admin.Activo = true;
    admin.FechaCreacion = DateTime.Now;

    try
    {
        var created = _repository.Create(admin);
        return ServiceResult<Administrador>.Ok(created, "Administrador creado correctamente.");
    }
    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
    {
        return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");
    }
}
```

**Detalles**:
- **Work factor 12**: ~250ms en CPU moderno (2024+), seguro contra GPU cracking
- **Validación password**: Mínimo 8 caracteres
- **Email normalizado**: Lowercase + trim antes de guardar y chequear
- **Manejo race condition**: Try/catch + UNIQUE constraint BD (2627/2601)

---

### 3. Update (Datos, Sin Password)

```csharp
public ServiceResult<Administrador> Update(int id, Administrador admin)
{
    if (!_repository.Exists(id))
        return ServiceResult<Administrador>.Fail($"El administrador con Id {id} no existe.");

    if (string.IsNullOrWhiteSpace(admin.Nombre))
        return ServiceResult<Administrador>.Fail("El nombre es obligatorio.");
    if (string.IsNullOrWhiteSpace(admin.Apellido))
        return ServiceResult<Administrador>.Fail("El apellido es obligatorio.");
    if (string.IsNullOrWhiteSpace(admin.Email))
        return ServiceResult<Administrador>.Fail("El email es obligatorio.");

    var existing = _repository.GetById(id);
    if (existing == null)
        return ServiceResult<Administrador>.Fail($"El administrador con Id {id} no existe.");

    // Normalizar ANTES de comparar
    admin.Email = admin.Email.ToLower().Trim();

    if (!existing.Email.Equals(admin.Email, StringComparison.OrdinalIgnoreCase))
    {
        if (_repository.ExistsByEmail(admin.Email))
            return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");
    }

    try
    {
        _repository.Update(id, admin);
        var updated = _repository.GetById(id);
        return ServiceResult<Administrador>.Ok(updated!, "Administrador actualizado correctamente.");
    }
    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
    {
        return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");
    }
}
```

**Nota**: No permite cambiar password ni Role aquí. Solo `Nombre`, `Apellido`, `Email`.

---

### 4. ChangePassword (Por ID)

```csharp
public ServiceResult ChangePassword(int id, string passwordActual, string nuevaPassword)
{
    if (!_repository.Exists(id))
        return ServiceResult.Fail($"El administrador con Id {id} no existe.");

    if (string.IsNullOrWhiteSpace(passwordActual))
        return ServiceResult.Fail("La contraseña actual es obligatoria.");
    if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
        return ServiceResult.Fail("La nueva contraseña debe tener al menos 8 caracteres.");

    var admin = _repository.GetById(id);
    if (admin == null)
        return ServiceResult.Fail($"El administrador con Id {id} no existe.");

    if (!BCryptNet.Verify(passwordActual, admin.PasswordHash))
        return ServiceResult.Fail("La contraseña actual es incorrecta.");

    var newHash = BCryptNet.HashPassword(nuevaPassword, workFactor: 12);
    _repository.UpdatePasswordHash(id, newHash);

    return ServiceResult.Ok("Contraseña actualizada correctamente.");
}
```

**Flujo**:
1. Verificar admin existe
2. Validar passwords (actual requerida, nueva ≥8 chars)
3. Verificar `passwordActual` con `BCrypt.Verify`
4. Hash `nuevaPassword` con workFactor 12
5. UPDATE `PasswordHash` en BD

---

### 5. ChangePasswordByEmail (Por Email)

```csharp
public ServiceResult ChangePasswordByEmail(string email, string passwordActual, string nuevaPassword)
{
    var admin = _repository.GetByEmail(email);
    if (admin == null)
        return ServiceResult.Fail("Usuario no encontrado.");

    if (!BCryptNet.Verify(passwordActual, admin.PasswordHash))
        return ServiceResult.Fail("Contraseña actual incorrecta.");

    if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
        return ServiceResult.Fail("La nueva contraseña debe tener al menos 8 caracteres.");

    var newHash = BCryptNet.HashPassword(nuevaPassword, workFactor: 12);
    _repository.UpdatePasswordHash(admin.Id, newHash);

    return ServiceResult.Ok("Contraseña actualizada correctamente.");
}
```

- Usado por `AuthController.VerifyPassword` + `AdministradorController.ChangePassword`
- Busca por email (normalizado), no por ID

---

### 6. HayAdmins / CrearPrimerAdminAsync

```csharp
public bool HayAdmins() => _repository.Count() > 0;

public async Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto)
{
    if (HayAdmins())
        return null;

    var admin = new Administrador
    {
        Nombre = dto.Nombre,
        Apellido = dto.Apellido,
        Email = dto.Email,
        Role = dto.Role
    };

    var result = Create(admin, dto.Password);
    if (!result.Success)
        return null;

    return new AdminResult(result.Data!.Id, result.Data.Nombre, result.Data.Apellido, result.Data.Email, result.Data.Role);
}
```

- `HayAdmins()`: `COUNT(*)` en BD
- `CrearPrimerAdminAsync`: Solo si `HayAdmins() == false`
- **Async por convención** (setup inicial), pero `Create` es sync

---

### 7. Delete (Soft Delete)

```csharp
public ServiceResult Delete(int id)
{
    if (!_repository.Exists(id))
        return ServiceResult.Fail($"El administrador con Id {id} no existe.");

    _repository.Delete(id);
    return ServiceResult.Ok("Administrador eliminado correctamente.");
}
```

**Repositorio hace soft delete**:
```sql
UPDATE Administradores SET Activo = 0 WHERE Id = @Id AND Activo = 1
```

---

## BCrypt Configuration

```csharp
// Work factor 12 = 2^12 = 4096 iteraciones
BCryptNet.HashPassword(password, workFactor: 12);
BCryptNet.Verify(password, hash);
```

| Work Factor | Tiempo aprox (CPU 2024) | Seguridad |
|-------------|-------------------------|-----------|
| 10 | ~60ms | Mínimo aceptable |
| **12** | **~250ms** | **Recomendado 2024+** |
| 14 | ~1000ms | Alta seguridad, UX impactado |

**Versión**: `BCrypt.Net-Next 4.0.3` (paquete NuGet)

---

## AdminResult (Response DTO)

```csharp
public record AdminResult(
    int    Id,
    string Nombre,
    string Apellido,
    string Email,
    string Role
);
```

- **Inmutable** (record)
- **Sin PasswordHash** → Nunca expone credenciales
- Serializado directamente en JWT claims y response JSON

---

## Integración con AuthController

```csharp
// AuthController.Login()
var admin = _authService.Login(dto.Email, dto.Password);

if (admin is null)
    return Unauthorized(ApiResponse<string>.Error("Email o contraseña incorrectos."));

var token = GenerarToken(admin);  // JWT con claims: sub, email, name, role, jti
```

**Claims en JWT**:
| Claim | Valor | Fuente |
|-------|-------|--------|
| `sub` | `admin.Id.ToString()` | `AdminResult.Id` |
| `email` | `admin.Email` | `AdminResult.Email` |
| `name` | `"{Nombre} {Apellido}"` | `AdminResult.Nombre + Apellido` |
| `role` | `admin.Role` | `AdminResult.Role` |
| `jti` | `Guid.NewGuid()` | Único por token |

---

## Setup Inicial (Development Only)

**Endpoint**: `POST /api/setup/admin`  
**Controller**: `SetupController.CrearPrimerAdmin()`

```csharp
if (!_env.IsDevelopment()) return NotFound();  // Solo Dev

if (_authService.HayAdmins())
    return Conflict(ApiResponse<string>.Error("Ya existe al menos un administrador..."));

var admin = await _authService.CrearPrimerAdminAsync(dto);
```

- **Solo Development**: `_env.IsDevelopment()` gate
- **Idempotente**: Si ya hay admins → 409
- **Único uso**: Tras primer admin, endpoint inutilizado

---

## Seguridad Adicional Recomendada (Futuro)

| Mejora | Prioridad | Implementación |
|--------|-----------|----------------|
| Rate limiting login | Alta | `Microsoft.AspNetCore.RateLimiting` |
| Account lockout tras N fallos | Media | Tabla `LoginAttempts` + contador |
| Password history (no reusar últimas N) | Baja | Tabla `PasswordHistory` |
| 2FA (TOTP) | Media | `GoogleAuthenticator` lib |
| Refresh tokens | Alta | Tabla `RefreshTokens` + rotación |
| Auditoria login (IP, user-agent, timestamp) | Media | Tabla `LoginLogs` |

---

## Testing

```csharp
// Mock IAdministradorService
var mockAuth = new Mock<IAdministradorService>();
mockAuth.Setup(x => x.Login("admin@test.com", "password123"))
        .Returns(new AdminResult(1, "Admin", "User", "admin@test.com", "Admin"));
mockAuth.Setup(x => x.HayAdmins()).Returns(false);
```