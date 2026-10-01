# Convenciones de API REST

## Estándares Generales

| Aspecto | Convención |
|---------|------------|
| **Base Path** | `/api/{recurso-plural}` |
| **Verbos HTTP** | GET, POST, PUT, DELETE |
| **Status Codes** | 200, 201, 204, 400, 401, 403, 404, 409, 500 |
| **Content-Type** | `application/json` (request & response) |
| **Auth** | JWT Bearer Token en header `Authorization: Bearer <token>` |
| **CORS** | `AllowAnyOrigin()` (dev) - policy "VueCors" |
| **Response Wrapper** | `ApiResponse<T>` en todos los endpoints |
| **Base URL (Dev)** | `http://localhost:5127` |

---

## Mapeo HTTP Status Codes

| Código | Cuándo | Ejemplo |
|--------|--------|---------|
| **200 OK** | GET exitoso, PUT exitoso | `GET /api/alumnos/1` |
| **201 Created** | POST exitoso | `POST /api/carreras` |
| **204 No Content** | DELETE exitoso | `DELETE /api/profesores/1` |
| **400 Bad Request** | ModelState inválido, regla de negocio (FK no existe) | Email inválido, CarreraId inexistente |
| **401 Unauthorized** | Token faltante, expirado, inválido, credenciales incorrectas | Login fallido |
| **403 Forbidden** | Token válido pero sin permisos (rol) | Admin intenta endpoint de SuperAdmin |
| **404 Not Found** | Recurso no existe (GetById, Update, Delete) | `GET /api/alumnos/999` |
| **409 Conflict** | Setup admin cuando ya existe | `POST /api/setup/admin` (2da vez) |
| **500 Internal Server Error** | Excepción no controlada, error BD | SqlException, NullReference |

---

## Estructura de Respuestas (ApiResponse<T> Wrapper)

**Todos los endpoints retornan `ApiResponse<T>`** (definido en `Instituto.API.Models.ApiModels`):

### Éxito - Colección (GET All)
```json
{
  "isSuccess": true,
  "message": "Operación exitosa",
  "data": [
    { "id": 1, "nombre": "Juan", "apellido": "Pérez", ... },
    { "id": 2, "nombre": "María", "apellido": "Gómez", ... }
  ]
}
```

### Éxito - Elemento Único (GET ById, POST, PUT)
```json
{
  "isSuccess": true,
  "message": "Operación exitosa",
  "data": { "id": 1, "nombre": "Juan", "apellido": "Pérez", "email": "juan@test.com", ... }
}
```

### Éxito - Login (POST /api/auth/login)
```json
{
  "isSuccess": true,
  "message": "Operación exitosa",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiraEn": "2026-09-25T20:30:00Z",
    "admin": {
      "id": 1,
      "nombre": "Admin",
      "apellido": "Principal",
      "email": "admin@tupac.edu.ar",
      "role": "Admin"
    }
  }
}
```

### Error (4xx, 5xx)
```json
{
  "isSuccess": false,
  "message": "Email o contraseña incorrectos.",
  "data": null
}
```

### Error Validación (400 ModelState)
```json
{
  "isSuccess": false,
  "message": "Errores de validación",
  "data": {
    "Email": ["El email es obligatorio."],
    "Password": ["La contraseña debe tener al menos 6 caracteres."]
  }
}
```

> **Nota**: El frontend desempaqueta automáticamente con el composable `useAuth` / llamadas fetch directas.

---

## Convenciones de Nombres en JSON

- **Propiedades**: **camelCase** (configurado `JsonNamingPolicy.CamelCase` en Program.cs)
  ```json
  { "nombre": "Juan", "fechaNacimiento": "2000-01-15T00:00:00" }
  ```
- **Enums/Strings**: Valores tal cual (`"role": "Admin"`, `"estado": "Activa"`)
- **Fechas**: ISO 8601 (`"2026-09-25T15:30:00Z"`)
- **Nulls**: Omitidos (`JsonIgnoreCondition.WhenWritingNull`)

---

## Versionado

**Actual**: Sin versionado en URL (v1 implícito)  
**Futuro**: `/api/v1/...` → `/api/v2/...` si breaking changes

---

## Paginación / Filtrado (No Implementado)

> Endpoints `GetAll` retornan **todo** sin paginación.  
> Para datasets grandes, agregar:
```
GET /api/alumnos?page=1&pageSize=20&search=perez&carreraId=3
```

---

## Headers de Respuesta Útiles

| Header | Valor | Endpoint |
|--------|-------|----------|
| `Location` | `/api/alumnos/123` | POST (201 Created) |
| `WWW-Authenticate` | `Bearer` | 401 Unauthorized |

---

## Rate Limiting (No Implementado)

> Recomendado para producción:
- `POST /api/auth/login`: 5 req/min/IP
- `POST /api/setup/admin`: 1 req/hora/IP
- CRUD autenticados: 100 req/min/user

---

## Endpoints por Módulo (Actualizados - Código Real)

| Módulo | Endpoints | Auth |
|--------|-----------|------|
| **Auth** | `POST /api/auth/login` | Público |
| | `POST /api/auth/verify-password` | JWT |
| **Setup** | `POST /api/setup/admin` (solo Dev) | Público |
| **Carreras** | `GET/POST/PUT/DELETE /api/carreras` | JWT (GET público) |
| **Alumnos** | `GET/PUT/DELETE /api/alumnos` | JWT |
| | `POST /api/alumnos` | **Público** (inscripción) |
| **Administradores** | `GET/POST/PUT/DELETE /api/administradores` | JWT |
| | `PUT /api/administradores/{id}/password` | JWT |
| | `POST /api/administradores/with-password` | JWT (crear con pass) |
| **Profesores** | `GET/POST/PUT/DELETE /api/profesores` | JWT |
| **Formularios** | `GET/POST/PUT/DELETE /api/formularios` | JWT |
| **Listados** | `GET /api/listado` (join Alumno+Carrera) | JWT |

---

## Detalle de Endpoints Especiales

### POST /api/administradores/with-password
Crear administrador con password (el POST normal retorna 400 indicando usar este endpoint):
```json
// Request
{
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan@test.com",
  "role": "Admin",
  "password": "Password123"
}
```

### PUT /api/administradores/{id}/password
Cambio de password (requiere password actual):
```json
// Request
{
  "passwordActual": "Password123",
  "nuevaPassword": "NewPassword456"
}
```

### POST /api/auth/verify-password
Verificar password actual (para operaciones sensibles):
```json
// Request
{ "password": "Password123" }
// Response: { "isSuccess": true, "message": "Contraseña verificada correctamente" }
```

### POST /api/setup/admin
Solo en `Environment.IsDevelopment()`. Retorna 404 en producción. Solo funciona si no hay admins.

---

## Documentación OpenAPI/Swagger (No Configurado)

> Para agregar:
```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Instituto API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

// En pipeline (solo Dev)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```