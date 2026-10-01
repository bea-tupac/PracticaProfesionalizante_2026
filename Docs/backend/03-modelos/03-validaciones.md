# Validaciones

## Validaciones con DataAnnotations (Server-Side)

Todas las entidades y DTOs usan **`System.ComponentModel.DataAnnotations`** para validación declarativa.

### Atributos Utilizados

| Atributo | Aplicado A | Error Message Personalizado |
|----------|------------|----------------------------|
| `[Required]` | Nombre, Apellido, Email, DNI, FechaNacimiento, Role, Password | "El {0} es obligatorio." |
| `[StringLength(n)]` | Nombre (100), Apellido (100), Email (255), Direccion (200), ... | "El {0} no puede superar los {1} caracteres." |
| `[EmailAddress]` | Email | "El {0} no tiene un formato válido." |
| `[Range(min, max)]` | DNI (1M-99M), DuracionAnios (1-10), CarreraId (>0) | "El {0} debe estar entre {1} y {2}." |
| `[MinLength(n)]` | Password (6 login, 8 setup) | "La {0} debe tener al menos {1} caracteres." |
| `[Phone]` | Telefono | "El {0} no tiene un formato válido." |

### Ejemplos por Entidad

#### Administrador
```csharp
[Required, StringLength(50)]
public string Role { get; set; } = "Admin";

[MinLength(8)]  // Solo en PasswordTemp (input create)
public string? PasswordTemp { get; set; }
```

#### Alumno
```csharp
[Required, Range(1000000, 99999999)]
public int DNI { get; set; }  // 7-8 dígitos, UNIQUE en BD

[Required]
public DateTime FechaNacimiento { get; set; }

[StringLength(200)] public string? Direccion { get; set; }
[StringLength(100)] public string? Nacionalidad { get; set; }
[Phone] public string? Telefono { get; set; }
[Range(1, int.MaxValue)] public int CarreraId { get; set; }  // FK required
```

#### Carrera
```csharp
[Required, StringLength(200)] public string Nombre { get; set; }
[Range(1, 10)] public int DuracionAnios { get; set; }
public string? Estado { get; set; } = "Activa";  // Default en BD también
```

#### Profesor
```csharp
[Phone] public string? Telefono { get; set; }
[StringLength(100)] public string? Especialidad { get; set; }
```

#### DTOs
```csharp
// LoginDto
[Required, EmailAddress] public string Email { get; init; }
[Required, MinLength(6)] public string Password { get; init; }

// SetupAdminDto
[Required, StringLength(100)] public string Nombre { get; init; }
[Required, StringLength(100)] public string Apellido { get; init; }
[Required, EmailAddress] public string Email { get; init; }
[Required, MinLength(8)] public string Password { get; init; }
public string Role { get; init; } = "Admin";
```

---

## Flujo de Validación en Pipeline

```
1. Request llega a Controller Action
        │
        ▼
2. Model Binding (JSON → Object)
        │
        ▼
3. ModelState Validation (DataAnnotations)
        │
        ├─ Inválido ──▶ return BadRequest(ModelState)  // 400 con detalles
        │
        ▼ Válido
4. Service Layer (Business Rules)
        │
        ├─ EntityNotFoundException ──▶ return NotFound()  // 404
        ├─ PersistenceException ──▶ return StatusCode(500)
        │
        ▼ Éxito
5. Return Ok/Created/NoContent
```

### Ejemplo en Controller

```csharp
[HttpPost]
public IActionResult Create([FromBody] Alumno alumno)
{
    // 1. Validación DataAnnotations automática
    if (!ModelState.IsValid)
        return BadRequest(ModelState);  // 400 con errores por campo

    try
    {
        // 2. Validación de negocio (FK existe)
        _carreraService.GetById(alumno.CarreraId);  // Lanza EntityNotFoundException si no existe

        // 3. Default de negocio
        alumno.FechaInscripcion ??= DateTime.Now;

        // 4. Persistencia
        var creado = _alumnoService.Create(alumno);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);  // 201
    }
    catch (EntityNotFoundException)
    {
        return BadRequest(new { error = $"La carrera con Id {alumno.CarreraId} no existe." });  // 400
    }
    // ... catch PersistenceException, Exception
}
```

---

## Respuesta de Error 400 (Validation Failed)

```json
{
  "Nombre": ["El nombre es obligatorio."],
  "Email": ["El email no tiene un formato válido."],
  "DNI": ["El DNI debe tener entre 7 y 8 dígitos."],
  "CarreraId": ["Debe seleccionar una carrera válida."]
}
```

Formato: `Dictionary<string, string[]>` serializado por ASP.NET Core.

---

## Validaciones de Negocio (No DataAnnotations)

| Regla | Dónde | Excepción |
|-------|-------|-----------|
| Carrera existe al crear/actualizar Alumno | `AlumnosController.Create/Update` | `EntityNotFoundException` → 400 |
| No eliminar Carrera con alumnos inscriptos | `CarrerasController.Delete` | `BadRequest` con mensaje |
| Email único (Administradores, Profesores) | BD (UNIQUE constraint) | `PersistenceException` (SqlException) → 500 |
| DNI único (Alumnos) | BD (UNIQUE constraint) | `PersistenceException` → 500 |
| Admin existe para login | `AdminAuthService.LoginAsync` | `null` → 401 Unauthorized |
| Password correcto (BCrypt verify) | `AdminAuthService.LoginAsync` | `null` → 401 Unauthorized |

---

## Validaciones Faltantes / Mejoras Futuras

| Validación | Estado | Comentario |
|------------|--------|------------|
| Email único en Alumnos | ❌ No implementado | Solo en BD si se agrega UNIQUE |
| Formato DNI (solo números) | ⚠️ Parcial | Range valida rango, no formato |
| FechaNacimiento no futura | ❌ No implementado | Agregar `[PastDate]` custom attribute |
| Password complexity (mayúscula, número, símbolo) | ❌ No implementado | Solo `MinLength` |
| Role enum válido (Admin/SuperAdmin) | ❌ No implementado | String libre |
| Estado carrera valores permitidos | ❌ No implementado | String libre |

---

## Validaciones Frontend (Vue 3)

> Responsabilidad del frontend, pero deben coincidir con backend:

```javascript
// Ejemplo Vuelidate / Zod / Yup
const alumnoSchema = z.object({
  nombre: z.string().min(1, 'Requerido').max(100),
  apellido: z.string().min(1, 'Requerido').max(100),
  email: z.string().email('Email inválido'),
  dni: z.number().int().min(1000000).max(99999999),
  fechaNacimiento: z.string().refine(d => new Date(d) < new Date(), 'No puede ser futura'),
  carreraId: z.number().int().positive(),
  // ... campos opcionales
});
```