# Endpoints de Administradores

**Base Path**: `/api/administradores`  
**Controller**: `AdministradorController`  
**Autenticación**: **Requerida** (`[Authorize]` en clase)  
**Roles**: Admin / SuperAdmin (validado por `[Authorize]` + policy futuro)

---

## GET /api/administradores

Lista todos los administradores activos.

### Request
```http
GET /api/administradores
Authorization: Bearer <token>
```

### Response 200 OK
```json
[
  {
    "id": 1,
    "nombre": "Admin",
    "apellido": "Principal",
    "email": "admin@tupac.edu.ar",
    "role": "Admin"
  },
  {
    "id": 2,
    "nombre": "Super",
    "apellido": "Admin",
    "email": "super@tupac.edu",
    "role": "SuperAdmin"
  }
]
```
> **Nota**: Solo `Activo = 1` (soft delete). No expone `PasswordHash` ni `PasswordTemp`.

### Errores
| Código | Causa |
|--------|-------|
| 401 | Token inválido/expirado |
| 403 | Sin permisos (futuro: policy) |
| 500 | Error BD |

---

## GET /api/administradores/{id}

Obtiene un administrador por ID.

### Request
```http
GET /api/administradores/1
Authorization: Bearer <token>
```

### Response 200 OK
```json
{
  "id": 1,
  "nombre": "Admin",
  "apellido": "Principal",
  "email": "admin@tupac.edu.ar",
  "role": "Admin"
}
```

### Response 404 Not Found
```json
{ "error": "Administrador con Id 1 no fue encontrado/a." }
```

---

## POST /api/administradores

Crea un nuevo administrador.

### Request
```http
POST /api/administradores
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Nuevo",
  "apellido": "Admin",
  "email": "nuevo@tupac.edu",
  "role": "Admin",
  "passwordTemp": "MiPassSeguro123"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| nombre | string | Sí | Max 100 |
| apellido | string | Sí | Max 100 |
| email | string | Sí | Email válido, único |
| role | string | No | Default "Admin", Max 50 |
| passwordTemp | string | No | Mín 8 chars (se hashea) |

### Response 201 Created
**Headers**: `Location: /api/administradores/3`
```json
{
  "id": 3,
  "nombre": "Nuevo",
  "apellido": "Admin",
  "email": "nuevo@tupac.edu",
  "role": "Admin"
}
```

### Response 400 Bad Request
```json
{ "error": "Error interno al crear el administrador." }
```
> Si email duplicado → SqlException → PersistenceException → 500 (mejorar: capturar UNIQUE violation → 409)

---

## PUT /api/administradores/{id}

Actualiza un administrador existente.

### Request
```http
PUT /api/administradores/1
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Admin",
  "apellido": "Actualizado",
  "email": "admin@tupac.edu.ar",
  "role": "SuperAdmin"
}
```
> **Nota**: `passwordTemp` **no** se usa en Update (cambio de pass = endpoint separado futuro).

### Response 200 OK
```json
{
  "id": 1,
  "nombre": "Admin",
  "apellido": "Actualizado",
  "email": "admin@tupac.edu.ar",
  "role": "SuperAdmin"
}
```

### Response 404 Not Found
```json
{ "error": "Administrador con Id 1 no fue encontrado/a." }
```

---

## PUT /api/administradores/{id}/password  🔐 **Requiere JWT**

Cambia la contraseña del administrador autenticado.  
Requiere contraseña actual verificada (vía `POST /api/auth/verify-password` en frontend).

### Request
```http
PUT /api/administradores/1/password
Authorization: Bearer <token>
Content-Type: application/json

{
  "passwordActual": "MiPassActual123",
  "nuevaPassword": "MiNuevaPass456"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| passwordActual | string | Sí | Debe coincidir con hash actual |
| nuevaPassword | string | Sí | Mín 8 caracteres |

### Response 200 OK
```json
{ "mensaje": "Contraseña actualizada correctamente" }
```

### Response 400 Bad Request
```json
{ "PasswordActual": ["La contraseña actual es obligatoria."] }
```

### Response 401 Unauthorized
```json
{ "error": "La contraseña actual es incorrecta." }
```
```json
{ "error": "Token inválido" }
```

### Response 500 Internal Server Error
```json
{ "error": "Error interno al cambiar la contraseña." }
```

---

## DELETE /api/administradores/{id}

Elimina (soft delete) un administrador.

### Request
```http
DELETE /api/administradores/1
Authorization: Bearer <token>
```

### Response 204 No Content
(Body vacío)

### Response 404 Not Found
```json
{ "error": "Administrador con Id 1 no fue encontrado/a." }
```

> **Soft Delete**: `UPDATE Administradores SET Activo = 0 WHERE Id = 1 AND Activo = 1`  
> El admin desaparece de `GetAll`/`GetById` pero persiste en BD.

---

## Resumen de Permisos

| Endpoint | Auth | Roles Permitidos |
|----------|------|------------------|
| GET /api/administradores | ✅ | Admin, SuperAdmin |
| GET /api/administradores/{id} | ✅ | Admin, SuperAdmin |
| POST /api/administradores | ✅ | SuperAdmin (futuro policy) |
| PUT /api/administradores/{id} | ✅ | SuperAdmin (futuro policy) |
| PUT /api/administradores/{id}/password | ✅ | Admin, SuperAdmin (propio usuario) |
| DELETE /api/administradores/{id} | ✅ | SuperAdmin (futuro policy) |

> **Actual**: Cualquier admin autenticado puede todo.  
> **Futuro**: Policy `RequireRole("SuperAdmin")` para escritura (excepto change-password propio).

---

## Validaciones de Negocio

| Regla | Dónde | Error |
|-------|-------|-------|
| Email único | BD (UNIQUE constraint) | 500 (mejorar a 409) |
| Password mín 8 chars (create) | DataAnnotation `PasswordTemp` | 400 ModelState |
| Password mín 8 chars (change) | DataAnnotation `ChangePasswordDto` | 400 ModelState |
| Solo admins activos | Service `WHERE Activo=1` | 404 si inactivo |
| Password actual correcta | `AdminAuthService.ChangePasswordAsync` | 401 Unauthorized |