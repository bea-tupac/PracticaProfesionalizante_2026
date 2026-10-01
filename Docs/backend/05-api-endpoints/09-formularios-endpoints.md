# Endpoints de Formularios

**Base Path**: `/api/formularios`  
**Controller**: `FormularioController`  
**Autenticación**: **Requerida** (`[Authorize]` en clase)

---

## GET /api/formularios

Lista todos los formularios.

### Request
```http
GET /api/formularios
Authorization: Bearer <token>
```

### Response 200 OK
```json
[
  {
    "id": 1,
    "nombre": "Inscripción 2026",
    "estado": "Abierto",
    "fechaApertura": "2026-01-15T00:00:00",
    "fechaCierre": "2026-03-31T23:59:59",
    "descripcion": "Formulario de inscripción para el ciclo lectivo 2026"
  },
  {
    "id": 2,
    "nombre": "Encuesta Satisfacción",
    "estado": "Cerrado",
    "fechaApertura": "2025-11-01T00:00:00",
    "fechaCierre": "2025-11-30T23:59:59",
    "descripcion": null
  }
]
```

---

## GET /api/formularios/{id}

Obtiene un formulario por ID.

### Request
```http
GET /api/formularios/1
Authorization: Bearer <token>
```

### Response 200 OK
```json
{
  "id": 1,
  "nombre": "Inscripción 2026",
  "estado": "Abierto",
  "fechaApertura": "2026-01-15T00:00:00",
  "fechaCierre": "2026-03-31T23:59:59",
  "descripcion": "Formulario de inscripción para el ciclo lectivo 2026"
}
```

### Response 404 Not Found
```json
{ "error": "Formulario con Id 1 no fue encontrado/a." }
```

---

## POST /api/formularios

Crea un nuevo formulario.

### Request
```http
POST /api/formularios
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Nuevo Formulario",
  "estado": "Borrador",
  "fechaApertura": "2026-02-01T00:00:00",
  "fechaCierre": "2026-04-30T23:59:59",
  "descripcion": "Descripción opcional"
}
```

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| nombre | string | Sí | Max 200 |
| estado | string | No | Default "Borrador" (Abierto/Cerrado/Borrador) |
| fechaApertura | datetime | Sí | Fecha válida |
| fechaCierre | datetime | Sí | Fecha válida, posterior a apertura |
| descripcion | string | No | Max 1000 |

### Response 201 Created
**Headers**: `Location: /api/formularios/3`
```json
{
  "id": 3,
  "nombre": "Nuevo Formulario",
  "estado": "Borrador",
  "fechaApertura": "2026-02-01T00:00:00",
  "fechaCierre": "2026-04-30T23:59:59",
  "descripcion": "Descripción opcional"
}
```

### Response 400 Bad Request
```json
// Validación ModelState
{ "FechaCierre": ["La fecha de cierre debe ser posterior a la de apertura."] }
```

---

## PUT /api/formularios/{id}

Actualiza un formulario existente.

### Request
```http
PUT /api/formularios/1
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Inscripción 2026 Actualizada",
  "estado": "Abierto",
  "fechaApertura": "2026-01-15T00:00:00",
  "fechaCierre": "2026-04-15T23:59:59",
  "descripcion": "Descripción actualizada"
}
```

### Response 200 OK
```json
{ ... formulario actualizado ... }
```

### Response 404 Not Found
```json
{ "error": "Formulario con Id 1 no fue encontrado/a." }
```

---

## DELETE /api/formularios/{id}

Elimina un formulario (hard delete).

### Request
```http
DELETE /api/formularios/1
Authorization: Bearer <token>
```

### Response 204 No Content
(Body vacío)

### Response 404 Not Found
```json
{ "error": "Formulario con Id 1 no fue encontrado/a." }
```

---

## Resumen Permisos

| Endpoint | Auth | Notas |
|----------|------|-------|
| GET /api/formularios | ✅ | Listado completo |
| GET /api/formularios/{id} | ✅ | Detalle |
| POST /api/formularios | ✅ | Admin crea |
| PUT /api/formularios/{id} | ✅ | Admin actualiza |
| DELETE /api/formularios/{id} | ✅ | Admin elimina |

---

## Modelo Formulario

```csharp
public class Formulario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = "Borrador"; // Borrador, Abierto, Cerrado
    public DateTime FechaApertura { get; set; }
    public DateTime FechaCierre { get; set; }
    public string? Descripcion { get; set; }
}
```

---

## Validaciones de Negocio

| Regla | Dónde | Error |
|-------|-------|-------|
| FechaCierre > FechaApertura | DataAnnotation `[Compare]` o lógica custom | 400 ModelState |
| Estado en {Borrador, Abierto, Cerrado} | Validación en frontend/backend | 400 |
| Nombre único | No validado (falta UNIQUE en BD) | - |

---

## Mejoras Pendientes

- [ ] Agregar constraint UNIQUE en `Formularios.Nombre`
- [ ] Validación `FechaCierre > FechaApertura` en backend (actualmente solo frontend)
- [ ] Filtros: `?estado=Abierto&fechaDesde=2026-01-01`
- [ ] Paginación en GET All
- [ ] Endpoint `GET /api/formularios/{id}/respuestas` (futuro)