# Endpoints de Profesores

**Base Path**: `/api/profesores`  
**Controller**: `ProfesorController`  
**Autenticación**: **Requerida** (`[Authorize]` en clase)

---

## GET /api/profesores

Lista todos los profesores.

### Request
```http
GET /api/profesores
Authorization: Bearer <token>
```

### Response 200 OK
```json
[
  {
    "id": 1,
    "nombre": "Carlos",
    "apellido": "Rodríguez",
    "email": "carlos.rodriguez@tupac.edu",
    "telefono": "11-1111-2222",
    "especialidad": "Programación Web"
  },
  {
    "id": 2,
    "nombre": "Laura",
    "apellido": "Martínez",
    "email": "laura.martinez@tupac.edu",
    "telefono": "11-3333-4444",
    "especialidad": "Diseño UX/UI"
  }
]
```

---

## GET /api/profesores/{id}

Obtiene un profesor por ID.

### Request
```http
GET /api/profesores/1
Authorization: Bearer <token>
```

### Response 200 OK
```json
{
  "id": 1,
  "nombre": "Carlos",
  "apellido": "Rodríguez",
  "email": "carlos.rodriguez@tupac.edu",
  "telefono": "11-1111-2222",
  "especialidad": "Programación Web"
}
```

### Response 404 Not Found
```json
{ "error": "Profesor con Id 1 no fue encontrado/a." }
```

---

## POST /api/profesores

Crea un nuevo profesor.

### Request
```http
POST /api/profesores
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Pedro",
  "apellido": "Gómez",
  "email": "pedro.gomez@tupac.edu",
  "telefono": "11-5555-6666",
  "especialidad": "Base de Datos"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| nombre | string | Sí | Max 100 |
| apellido | string | Sí | Max 100 |
| email | string | Sí | Email válido, **único** |
| telefono | string | No | Formato teléfono |
| especialidad | string | No | Max 100 |

### Response 201 Created
**Headers**: `Location: /api/profesores/3`
```json
{
  "id": 3,
  "nombre": "Pedro",
  "apellido": "Gómez",
  "email": "pedro.gomez@tupac.edu",
  "telefono": "11-5555-6666",
  "especialidad": "Base de Datos"
}
```

---

## PUT /api/profesores/{id}

Actualiza un profesor.

### Request
```http
PUT /api/profesores/1
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Carlos",
  "apellido": "Rodríguez",
  "email": "carlos.rodriguez@tupac.edu",
  "telefono": "11-1111-2222",
  "especialidad": "Full Stack Development"
}
```

### Response 200 OK
```json
{ ... profesor actualizado ... }
```

### Response 404 Not Found
```json
{ "error": "Profesor con Id 1 no fue encontrado/a." }
```

---

## DELETE /api/profesores/{id}

Elimina un profesor (hard delete).

### Request
```http
DELETE /api/profesores/1
Authorization: Bearer <token>
```

### Response 204 No Content

### Response 404 Not Found
```json
{ "error": "Profesor con Id 1 no fue encontrado/a." }
```

---

## Resumen Permisos

| Endpoint | Auth | Notas |
|----------|------|-------|
| GET /api/profesores | ✅ | Lista |
| GET /api/profesores/{id} | ✅ | Detalle |
| POST /api/profesores | ✅ | Crear |
| PUT /api/profesores/{id} | ✅ | Actualizar |
| DELETE /api/profesores/{id} | ✅ | Eliminar |

---

## Validaciones de Negocio

| Regla | Dónde | Error |
|-------|-------|-------|
| Email único | BD (UNIQUE constraint) | 500 SqlException → PersistenceException |
| Email formato válido | DataAnnotation `[EmailAddress]` | 400 ModelState |
| Teléfono formato | DataAnnotation `[Phone]` | 400 ModelState |

---

## Relaciones Futuras (No Implementadas)

| Relación | Descripción |
|----------|-------------|
| Profesor → Materias | Un profesor dicta múltiples materias |
| Profesor → Carreras | Un profesor pertenece a una/s carrera/s |
| Profesor → Horarios | Disponibilidad horaria |

> Actualmente `Profesores` es entidad aislada sin FKs.