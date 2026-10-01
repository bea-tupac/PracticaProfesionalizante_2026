# Endpoints de Carreras

**Base Path**: `/api/carreras`  
**Controller**: `CarrerasController`  
**Autenticación**: **Mixta** - GET público, POST/PUT/DELETE requieren auth

---

## GET /api/carreras  🌐 **PÚBLICO**

Lista todas las carreras (catálogo público).

### Request
```http
GET /api/carreras
```

### Response 200 OK
```json
[
  {
    "id": 1,
    "nombre": "Tecnicatura en Programación",
    "duracionAnios": 3,
    "turno": "Mañana",
    "modalidad": "Presencial",
    "horario": "Lun-Vie 08:00-13:00",
    "estado": "Activa"
  },
  {
    "id": 2,
    "nombre": "Tecnicatura en Diseño Gráfico",
    "duracionAnios": 2,
    "turno": "Tarde",
    "modalidad": "Híbrida",
    "horario": "Lun-Mie-Vie 14:00-18:00",
    "estado": "Activa"
  }
]
```

---

## GET /api/carreras/{id}  🌐 **PÚBLICO**

Detalle de una carrera.

### Request
```http
GET /api/carreras/1
```

### Response 200 OK
```json
{
  "id": 1,
  "nombre": "Tecnicatura en Programación",
  "duracionAnios": 3,
  "turno": "Mañana",
  "modalidad": "Presencial",
  "horario": "Lun-Vie 08:00-13:00",
  "estado": "Activa"
}
```

### Response 404 Not Found
```json
{ "error": "Carrera con Id 1 no fue encontrado/a." }
```

---

## POST /api/carreras

Crea una nueva carrera.

### Request
```http
POST /api/carreras
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Tecnicatura en Data Science",
  "duracionAnios": 3,
  "turno": "Noche",
  "modalidad": "Virtual",
  "horario": "Mar-Jue 19:00-22:00",
  "estado": "Activa"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| nombre | string | Sí | Max 200 |
| duracionAnios | int | Sí | 1-10 |
| turno | string | No | Max 50 |
| modalidad | string | No | Max 50 |
| horario | string | No | Max 100 |
| estado | string | No | Default "Activa" |

### Response 201 Created
**Headers**: `Location: /api/carreras/3`
```json
{
  "id": 3,
  "nombre": "Tecnicatura en Data Science",
  "duracionAnios": 3,
  "turno": "Noche",
  "modalidad": "Virtual",
  "horario": "Mar-Jue 19:00-22:00",
  "estado": "Activa"
}
```

---

## PUT /api/carreras/{id}

Actualiza una carrera.

### Request
```http
PUT /api/carreras/1
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Tecnicatura en Programación Avanzada",
  "duracionAnios": 3,
  "turno": "Mañana",
  "modalidad": "Presencial",
  "horario": "Lun-Vie 08:00-13:00",
  "estado": "Activa"
}
```

### Response 200 OK
```json
{ ... carrera actualizada ... }
```

### Response 404 Not Found
```json
{ "error": "Carrera con Id 1 no fue encontrado/a." }
```

---

## DELETE /api/carreras/{id}

Elimina una carrera (con validación de negocio).

### Request
```http
DELETE /api/carreras/1
Authorization: Bearer <token>
```

### Response 204 No Content

### Response 400 Bad Request (Regla de Negocio)
```json
{ "error": "No se puede eliminar la carrera porque tiene alumnos inscriptos." }
```

### Response 404 Not Found
```json
{ "error": "Carrera con Id 1 no fue encontrado/a." }
```

> **Regla**: `CarrerasController.Delete()` verifica `_alumnoService.GetAll().Any(a => a.CarreraId == id)` antes de borrar.

---

## Resumen Permisos

| Endpoint | Auth | Notas |
|----------|------|-------|
| GET /api/carreras | ❌ | Catálogo público |
| GET /api/carreras/{id} | ❌ | Detalle público |
| POST /api/carreras | ✅ | Admin crea |
| PUT /api/carreras/{id} | ✅ | Admin actualiza |
| DELETE /api/carreras/{id} | ✅ | Admin elimina (si sin alumnos) |

---

## Validaciones de Negocio

| Regla | Dónde | Error |
|-------|-------|-------|
| Duración 1-10 años | DataAnnotation `[Range(1,10)]` | 400 ModelState |
| Estado default "Activa" | Service `Estado ?? "Activa"` | - |
| No borrar si hay alumnos | Controller `Delete()` | 400 BadRequest |
| Nombre único | No validado (falta UNIQUE en BD) | - |

---

## Mejoras Pendientes

- [ ] Agregar constraint UNIQUE en `Carreras.Nombre`
- [ ] Endpoint `GET /api/carreras/{id}/alumnos` (alumnos por carrera)
- [ ] Filtros: `?estado=Activa&turno=Mañana`
- [ ] Paginación en GET All