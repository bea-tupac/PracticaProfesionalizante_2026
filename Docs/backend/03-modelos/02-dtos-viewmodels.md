# DTOs y ViewModels

## DTOs de Entrada (Input / Request)

### LoginDto
**Archivo**: `Models/LoginDto.cs`  
**Uso**: `POST /api/auth/login`

```csharp
public record LoginDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required, MinLength(6)]
    public string Password { get; init; } = string.Empty;
}
```

| Campo | Validación | Descripción |
|-------|------------|-------------|
| Email | Required, formato email | Credencial de acceso |
| Password | Required, mín 6 chars | Contraseña en claro (se hashea en server) |

**Record** → inmutable, `init` only, `with` expressions disponibles.

---

### SetupAdminDto
**Archivo**: `Models/SetupAdminDto.cs`  
**Uso**: `POST /api/setup/admin` (solo Development)

```csharp
public record SetupAdminDto
{
    [Required, StringLength(100)]
    public string Nombre { get; init; } = string.Empty;
    
    [Required, StringLength(100)]
    public string Apellido { get; init; } = string.Empty;
    
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;
    
    public string Role { get; init; } = "Admin";
}
```

| Campo | Validación | Default | Descripción |
|-------|------------|---------|-------------|
| Nombre | Required, Max 100 | - | Nombre admin |
| Apellido | Required, Max 100 | - | Apellido admin |
| Email | Required, Email | - | Login único |
| Password | Required, Min 8 | - | Contraseña inicial |
| Role | Opcional | "Admin" | "Admin" / "SuperAdmin" |

---

## DTOs de Salida (Output / Response)

### AdminResult
**Archivo**: `Models/AdminResult.cs`  
**Uso**: Response de `POST /api/auth/login`

```csharp
public record AdminResult(
    int    Id,
    string Nombre,
    string Apellido,
    string Email,
    string Role
);
```

**Sin `PasswordHash`** → nunca expone credenciales.  
**Record** → inmutable, serialización automática por posición.

---

### AlumnoListadoDto
**Archivo**: `DTOs/AlumnoListadoDTO.cs`  
**Uso**: `GET /api/listado` (vista aplanada join Alumno+Carrera)

```csharp
public class AlumnoListadoDto
{
    public int AlumnoId { get; set; }
    public string? NombreCompleto { get; set; }
    public int DNI { get; set; }
    public string? Email { get; set; }
    public string? Carrera { get; set; }
    public string? Turno { get; set; }
    public int Edad { get; set; }
}
```

| Campo | Origen | Descripción |
|-------|--------|-------------|
| AlumnoId | `Alumno.Id` | PK alumno |
| NombreCompleto | `Alumno.Apellido + ", " + Alumno.Nombre` | Formato "Apellido, Nombre" |
| DNI | `Alumno.DNI` | Documento |
| Email | `Alumno.Email` | Contacto |
| Carrera | `Carrera.Nombre` (join) | Nombre carrera o "Sin carrera" |
| Turno | `Alumno.Turno` | Turno del alumno |
| Edad | `Alumno.Edad` (calculada) | Edad en años |

**Construido en**: `ListadoController.GetListado()` vía LINQ join en memoria.

---

## Entidades como DTOs de Salida (CRUD)

Los endpoints CRUD (`GET`, `POST`, `PUT`) devuelven **la entidad completa** del dominio:

| Endpoint | Response Body |
|----------|---------------|
| `GET /api/alumnos/1` | `Alumno` (con todas sus props) |
| `POST /api/carreras` | `Carrera` (con Id asignado) |
| `PUT /api/profesores/1` | `Profesor` (actualizado) |

**Ventaja**: Simplicidad, una sola fuente de verdad.  
**Desventaja**: Over-fetching (expone todos los campos), no hay versionado de API.

---

## ViewModels de Frontend (Referencia)

> No están en el backend, pero útil para contrato frontend-backend:

```typescript
// Frontend Vue 3 (TypeScript)
interface AlumnoForm {
  nombre: string;
  apellido: string;
  email: string;
  dni: number;
  fechaNacimiento: string; // ISO date
  direccion?: string;
  nacionalidad?: string;
  telefono?: string;
  tituloSecundario?: string;
  turno?: string;
  carreraId: number;
}

interface AdminLogin {
  email: string;
  password: string;
}

interface AuthResponse {
  token: string;
  expiraEn: string; // ISO datetime
  admin: {
    id: number;
    nombre: string;
    apellido: string;
    email: string;
    role: string;
  };
}
```

---

## Patrones de DTOs en este Proyecto

| Patrón | Dónde se usa | Ventaja |
|--------|--------------|---------|
| **Record inmutable** | `LoginDto`, `SetupAdminDto`, `AdminResult` | Thread-safe, `with`, pattern matching |
| **Clase mutable** | `AlumnoListadoDto` | Compatibilidad JSON estándar, setters |
| **Entidad directa** | CRUD responses | Menos código, una sola definición |
| **Cálculo en servidor** | `Alumno.Edad`, `NombreCompleto` | Consistencia, lógica centralizada |