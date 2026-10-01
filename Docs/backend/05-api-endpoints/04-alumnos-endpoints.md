# Endpoints de Alumnos

**Base Path**: `/api/alumnos`  
**Controller**: `AlumnosController`  
**Autenticación**: **Mixta** - GET/PUT/DELETE requieren auth, POST público (inscripción)

---

## GET /api/alumnos

Lista todos los alumnos.

### Request
```http
GET /api/alumnos
Authorization: Bearer <token>
```

### Response 200 OK
```json
[
  {
    "id": 1,
    "nombre": "Juan",
    "apellido": "Pérez",
    "email": "juan@email.com",
    "dni": 12345678,
    "fechaNacimiento": "2000-05-15T00:00:00",
    "direccion": "Calle 123",
    "nacionalidad": "Argentina",
    "fechaInscripcion": "2026-03-01T10:00:00",
    "telefono": "11-1234-5678",
    "tituloSecundario": "Bachiller",
    "turno": "Mañana",
    "carreraId": 2,
    "edad": 26
  }
]
```

---

## GET /api/alumnos/{id}

Obtiene un alumno por ID.

### Request
```http
GET /api/alumnos/1
Authorization: Bearer <token>
```

### Response 200 OK
```json
{ ... alumno completo ... }
```

### Response 404 Not Found
```json
{ "error": "Alumno con Id 1 no fue encontrado/a." }
```

---

## POST /api/alumnos  🌐 **PÚBLICO** (`[AllowAnonymous]`)

Inscripción de alumno (formulario público).

### Request
```http
POST /api/alumnos
Content-Type: application/json

{
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan@email.com",
  "dni": 12345678,
  "fechaNacimiento": "2000-05-15",
  "direccion": "Calle 123",
  "nacionalidad": "Argentina",
  "telefono": "11-1234-5678",
  "tituloSecundario": "Bachiller",
  "turno": "Mañana",
  "carreraId": 2
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| nombre | string | Sí | Max 100 |
| apellido | string | Sí | Max 100 |
| email | string | Sí | Email válido |
| dni | int | Sí | 7-8 dígitos, único |
| fechaNacimiento | date | Sí | Fecha pasada |
| direccion | string | No | Max 200 |
| nacionalidad | string | No | Max 100 |
| telefono | string | No | Formato teléfono |
| tituloSecundario | string | No | - |
| turno | string | No | - |
| carreraId | int | Sí | >0, debe existir |

### Response 201 Created
**Headers**: `Location: /api/alumnos/15`
```json
{
  "id": 15,
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan@email.com",
  "dni": 12345678,
  "fechaNacimiento": "2000-05-15T00:00:00",
  "direccion": "Calle 123",
  "nacionalidad": "Argentina",
  "fechaInscripcion": "2026-09-25T15:30:00",  // Auto NOW
  "telefono": "11-1234-5678",
  "tituloSecundario": "Bachiller",
  "turno": "Mañana",
  "carreraId": 2,
  "edad": 26
}
```

### Response 400 Bad Request
```json
// Validación ModelState
{ "DNI": ["El DNI debe tener entre 7 y 8 dígitos."] }

// Regla de negocio: Carrera no existe
{ "error": "La carrera con Id 99 no existe." }
```

---

## PUT /api/alumnos/{id}

Actualiza un alumno.

### Request
```http
PUT /api/alumnos/1
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan.nuevo@email.com",
  "dni": 12345678,
  "fechaNacimiento": "2000-05-15",
  "direccion": "Nueva Calle 456",
  "nacionalidad": "Argentina",
  "telefono": "11-9876-5432",
  "tituloSecundario": "Bachiller",
  "turno": "Tarde",
  "carreraId": 3
}
```

### Response 200 OK
```json
{ ... alumno actualizado ... }
```

### Response 400 Bad Request
```json
// Carrera no existe
{ "error": "La carrera con Id 99 no existe." }
```

### Response 404 Not Found
```json
{ "error": "Alumno con Id 1 no fue encontrado/a." }
```

---

## DELETE /api/alumnos/{id}

Elimina un alumno (hard delete).

### Request
```http
DELETE /api/alumnos/1
Authorization: Bearer <token>
```

### Response 204 No Content

### Response 404 Not Found
```json
{ "error": "Alumno con Id 1 no fue encontrado/a." }
```

---

## Reglas de Negocio Específicas

| Regla | Implementación |
|-------|----------------|
| **Inscripción pública** | `[AllowAnonymous]` en POST |
| **Carrera debe existir** | Controller valida `_carreraService.GetById(carreraId)` antes de Create/Update |
| **FechaInscripcion auto** | `alumno.FechaInscripcion ??= DateTime.Now` en Create |
| **Edad calculada** | Propiedad `Edad` en modelo (no persistida) |
| **DNI único** | Constraint UNIQUE en BD → 500 si duplicado (mejorar a 409) |

---

## Resumen Permisos

| Endpoint | Auth | Notas |
|----------|------|-------|
| GET /api/alumnos | ✅ | Lista completa |
| GET /api/alumnos/{id} | ✅ | Detalle |
| POST /api/alumnos | ❌ | **Público** - Inscripción web |
| PUT /api/alumnos/{id} | ✅ | Admin actualiza datos |
| DELETE /api/alumnos/{id} | ✅ | Admin elimina |

---

## DTO de Listado (Vista Aplanada)

Ver `GET /api/listado` en [Listados Endpoints](./07-listados-endpoints.md) para vista join Alumno+Carrera optimizada para grillas.