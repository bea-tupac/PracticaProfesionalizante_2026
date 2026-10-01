# Entidades de Dominio (Models)

## Jerarquía de Herencia

```
Persona (abstract)
├── Administrador
├── Alumno
└── Profesor

Carrera (independiente)
```

## 1. Persona (Base Abstracta)

**Archivo**: `Models/Persona.cs`

```csharp
public abstract class Persona
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [Required, StringLength(100)]
    public string Apellido { get; set; } = string.Empty;
    
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;
}
```

| Propiedad | Validación | BD Column |
|-----------|------------|-----------|
| Id | Auto (PK) | `Id` |
| Nombre | Required, Max 100 | `Nombre` |
| Apellido | Required, Max 100 | `Apellido` |
| Email | Required, Email format, Max 255 | `Email` |

**Nota**: No mapea a tabla propia. Sus propiedades se incluyen en tablas hijas.

---

## 2. Administrador

**Archivo**: `Models/Administrador.cs`

```csharp
public class Administrador : Persona
{
    [Required, StringLength(50)]
    public string Role { get; set; } = string.Empty;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [MinLength(8)]
    public string? PasswordTemp { get; set; }
}
```

| Propiedad | Validación | BD Column | Notas |
|-----------|------------|-----------|-------|
| Role | Required, Max 50 | `Role` | 'Admin' / 'SuperAdmin' |
| PasswordTemp | Min 8, opcional | **No persistida** | Solo para CREATE input, se hashea |

**Tabla BD**: `Administradores`  
**Campos extra en BD**: `PasswordHash`, `Activo`, `FechaCreacion`

---

## 3. Alumno

**Archivo**: `Models/Alumno.cs`

```csharp
public class Alumno : Persona
{
    [Required, Range(1000000, 99999999)]
    public int DNI { get; set; }
    
    [Required]
    public DateTime FechaNacimiento { get; set; }
    
    [StringLength(200)]
    public string? Direccion { get; set; }
    
    [StringLength(100)]
    public string? Nacionalidad { get; set; }
    
    public DateTime? FechaInscripcion { get; set; }
    
    [Phone]
    public string? Telefono { get; set; }
    
    public string? TituloSecundario { get; set; }
    
    public string? Turno { get; set; }
    
    [Range(1, int.MaxValue)]
    public int CarreraId { get; set; }
    
    // Propiedad calculada (no persistida)
    public int Edad => (int)((DateTime.Now - FechaNacimiento).TotalDays / 365.25);
}
```

| Propiedad | Validación | BD Column | Notas |
|-----------|------------|-----------|-------|
| DNI | Required, 7-8 dígitos, **UNIQUE** | `DNI` | Índice único |
| FechaNacimiento | Required | `FechaNacimiento` | DATE |
| Direccion | Max 200 | `Direccion` | Nullable |
| Nacionalidad | Max 100 | `Nacionalidad` | Nullable |
| FechaInscripcion | Auto (Default) | `FechaInscripcion` | Default NOW en BD |
| Telefono | Phone format | `Telefono` | Nullable |
| TituloSecundario | Sin validación | `TituloSecundario` | Nullable |
| Turno | Sin validación | `Turno` | Nullable |
| CarreraId | Required, >0 | `CarreraId` | **FK → Carreras.Id** |
| Edad | **Calculada** | No persistida | Read-only, computed |

**Tabla BD**: `Alumnos`  
**Relación**: N:1 con `Carreras` (muchos alumnos por carrera)

---

## 4. Carrera

**Archivo**: `Models/Carrera.cs`

```csharp
public class Carrera
{
    public int Id { get; set; }
    
    [Required, StringLength(200)]
    public string Nombre { get; set; } = string.Empty;
    
    [Range(1, 10)]
    public int DuracionAnios { get; set; }
    
    public string? Turno { get; set; }
    public string? Modalidad { get; set; }
    public string? Horario { get; set; }
    public string? Estado { get; set; }
}
```

| Propiedad | Validación | BD Column | Notas |
|-----------|------------|-----------|-------|
| Id | Auto (PK) | `Id` | Identity |
| Nombre | Required, Max 200 | `Nombre` | |
| DuracionAnios | Required, 1-10 | `DuracionAnios` | CHECK constraint |
| Turno | Max 50 | `Turno` | Nullable |
| Modalidad | Max 50 | `Modalidad` | Nullable |
| Horario | Max 100 | `Horario` | Nullable (ej: "Lun-Vie 18-22hs") |
| Estado | Default 'Activa' | `Estado` | 'Activa'/'Inactiva'/'Cerrada' |

**Tabla BD**: `Carreras`  
**Relación**: 1:N con `Alumnos`

---

## 5. Profesor

**Archivo**: `Models/Profesor.cs`

```csharp
public class Profesor : Persona
{
    [Phone]
    public string? Telefono { get; set; }
    
    [StringLength(100)]
    public string? Especialidad { get; set; }
}
```

| Propiedad | Validación | BD Column | Notas |
|-----------|------------|-----------|-------|
| Telefono | Phone format | `Telefono` | Nullable |
| Especialidad | Max 100 | `Especialidad` | Nullable |

**Tabla BD**: `Profesores`  
**Sin relaciones FK actuales** (diseño simple)

---

## Resumen de Mapeo Modelo ↔ Tabla

| Modelo | Tabla SQL | PK | FKs | Índices Extra |
|--------|-----------|----|-----|---------------|
| Administrador | Administradores | Id | - | Email (UNIQUE) |
| Alumno | Alumnos | Id | CarreraId → Carreras.Id | DNI (UNIQUE), CarreraId |
| Carrera | Carreras | Id | - | - |
| Profesor | Profesores | Id | - | Email (UNIQUE) |

## Convenciones de Mapeo en Servicios

En `*SqlServerService.cs`, el método `MapReaderToEntity` mapea **explícitamente por nombre de columna**:

```csharp
// Ejemplo Alumno
Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
DNI = reader.GetInt32(reader.GetOrdinal("DNI")),
FechaNacimiento = reader.GetDateTime(reader.GetOrdinal("FechaNacimiento")),
Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString(reader.GetOrdinal("Direccion")),


```

---

## Buenas Prácticas de Modelos

- **Herencia**: Reutiliza campos comunes en `Persona`
- **Validaciones**: `DataAnnotations` en propiedades (server-side)
- **Nullability**: `string?` para opcionales, `string` para requeridos (non-nullable)
- **Propiedades calculadas**: `readonly` sin setter (`Edad`)
- **JSON Ignore**: `[JsonIgnore]` para campos sensibles (`PasswordTemp`)
- **Records**: Para DTOs inmutables de entrada (`LoginDto`, `SetupAdminDto`)