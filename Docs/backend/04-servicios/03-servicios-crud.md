# Servicios CRUD - Detalle por Entidad

## Interfaz Común

```csharp
public interface ICrudJsonService<T> where T : class
{
    List<T> GetAll();
    T GetById(int id);
    T Create(T entity);
    void Update(int id, T entity);
    void Delete(int id);
}
```

Todos los 5 servicios implementan esta interfaz.

---

## 1. AdministradorSqlServerService

**Archivo**: `Services/AdministradorSqlServerService.cs`

### Tabla: `Administradores`

### Métodos

| Método | SQL | Comportamiento Especial |
|--------|-----|------------------------|
| `GetAll()` | `SELECT ... WHERE Activo = 1 ORDER BY Id` | Solo activos (soft delete) |
| `GetById(id)` | `SELECT ... WHERE Id = @Id AND Activo = 1` | Lanza `EntityNotFoundException` si no existe/activo |
| `Create(entity)` | `INSERT ... VALUES (@Nombre, @Apellido, @Email, @PasswordHash, @Role, 1)` | Hashea `PasswordTemp` con BCrypt(12), usa `SCOPE_IDENTITY()` |
| `Update(id, entity)` | `UPDATE ... SET Nombre, Apellido, Email, Role WHERE Id=@Id AND Activo=1` | No actualiza password |
| `Delete(id)` | `UPDATE ... SET Activo=0 WHERE Id=@Id AND Activo=1` | **Soft delete** |

### Password Handling

```csharp
// En Create()
var rawPassword = entity.PasswordTemp ?? "Cambiar1234!";
var hash = BCrypt.Net.BCrypt.HashPassword(rawPassword, workFactor: 12);
// ...
cmd.Parameters.AddWithValue("@PasswordHash", hash);
```

- `PasswordTemp` viene del DTO/input (mín 8 chars por validación)
- Fallback: `"Cambiar1234!"` si null
- **Nunca** se expone en `GetAll`/`GetById` (no está en SELECT)

---

## 2. AlumnoSqlServerService

**Archivo**: `Services/AlumnoSqlServerService.cs`

### Tabla: `Alumnos`

### Métodos

| Método | SQL | Comportamiento Especial |
|--------|-----|------------------------|
| `GetAll()` | `SELECT 13 columnas ORDER BY Id` | Todos (sin filtro activo - no tiene soft delete) |
| `GetById(id)` | `SELECT 13 columnas WHERE Id = @Id` | Lanza `EntityNotFoundException` |
| `Create(entity)` | `INSERT 12 columnas (sin Id) + SCOPE_IDENTITY()` | `FechaInscripcion` default `DateTime.Now` si null |
| `Update(id, entity)` | `UPDATE 12 columnas WHERE Id = @Id` | Valida CarreraId existe (en controller) |
| `Delete(id)` | `DELETE FROM Alumnos WHERE Id = @Id` | **Hard delete** |

### NULL Handling

```csharp
cmd.Parameters.AddWithValue("@Direccion", (object?)entity.Direccion ?? DBNull.Value);
cmd.Parameters.AddWithValue("@Nacionalidad", (object?)entity.Nacionalidad ?? DBNull.Value);
cmd.Parameters.AddWithValue("@FechaInscripcion", (object?)entity.FechaInscripcion ?? (object)DateTime.Now);
// ...
```

- `(object?)` evita ambigüedad de overload
- `DBNull.Value` para NULL en BD
- `FechaInscripcion` default en servicio (no solo BD)

---

## 3. CarreraSqlServerService

**Archivo**: `Services/CarreraSqlServerService.cs`

### Tabla: `Carreras`

### Métodos

| Método | SQL | Comportamiento Especial |
|--------|-----|------------------------|
| `GetAll()` | `SELECT 7 columnas ORDER BY Id` | |
| `GetById(id)` | `SELECT 7 columnas WHERE Id = @Id` | |
| `Create(entity)` | `INSERT 6 columnas + SCOPE_IDENTITY()` | `Estado` default `'Activa'` si null |
| `Update(id, entity)` | `UPDATE 6 columnas WHERE Id = @Id` | |
| `Delete(id)` | `DELETE FROM Carreras WHERE Id = @Id` | **Hard delete** (validado en controller: no alumnos) |

### Default Estado

```csharp
// En Create y Update
cmd.Parameters.AddWithValue("@Estado", (object?)entity.Estado ?? "Activa");
```

---

## 4. ProfesorSqlServerService

**Archivo**: `Services/ProfesorSqlServerService.cs`

### Tabla: `Profesores`

### Métodos

| Método | SQL | Comportamiento Especial |
|--------|-----|------------------------|
| `GetAll()` | `SELECT 6 columnas ORDER BY Id` | |
| `GetById(id)` | `SELECT 6 columnas WHERE Id = @Id` | |
| `Create(entity)` | `INSERT 5 columnas + SCOPE_IDENTITY()` | |
| `Update(id, entity)` | `UPDATE 5 columnas WHERE Id = @Id` | |
| `Delete(id)` | `DELETE FROM Profesores WHERE Id = @Id` | **Hard delete** |

### Simplicidad

- Menos campos (5 + Id)
- Sin FKs, sin validaciones cruzadas
- Email UNIQUE en BD (constraint)

---

## 5. FormularioSqlServerService

**Archivo**: `Services/FormularioSqlServerService.cs`

### Tabla: `Formularios`

### Métodos

| Método | SQL | Comportamiento Especial |
|--------|-----|------------------------|
| `GetAll()` | `SELECT 7 columnas ORDER BY Id` | |
| `GetById(id)` | `SELECT 7 columnas WHERE Id = @Id` | |
| `Create(entity)` | `INSERT 6 columnas + SCOPE_IDENTITY()` | |
| `Update(id, entity)` | `UPDATE 6 columnas WHERE Id = @Id` | |
| `Delete(id)` | `DELETE FROM Formularios WHERE Id = @Id` | **Hard delete** |

### Campos

- `Nombre` (requerido)
- `Estado` (default 'Borrador')
- `FechaApertura` (requerido, DATETIME2)
- `FechaCierre` (requerido, DATETIME2)
- `Descripcion` (opcional, NVARCHAR(1000))

---

## Comparativa de Patrones DELETE

| Entidad | Tipo Delete | Justificación |
|---------|-------------|---------------|
| **Administrador** | Soft (`Activo=0`) | Auditoría, recuperación, login history |
| **Alumno** | Hard (`DELETE`) | RGPD/derecho al olvido, no hay dependencias fuertes |
| **Carrera** | Hard (`DELETE`) | Validación previa en controller (no alumnos) |
| **Profesor** | Hard (`DELETE`) | Simple, sin dependencias actuales |
| **Formulario** | Hard (`DELETE`) | Simple, sin dependencias actuales |

> **Nota**: Alumnos tiene FK a Carreras pero **no** ON DELETE CASCADE. El controller valida antes de borrar carrera.

---

## Validaciones Cruzadas (En Controllers, No Servicios)

| Servicio | Validación | Dónde |
|----------|------------|-------|
| Alumno | CarreraId existe | `AlumnosController.Create/Update` → `_carreraService.GetById()` |
| Carrera | No tiene alumnos al borrar | `CarrerasController.Delete` → `_alumnoService.GetAll().Any(a => a.CarreraId == id)` |

**Principio**: Servicios = operaciones atómicas single-entity. Controllers = orquestación multi-entidad.

---

## Manejo de Errores Común

```csharp
try
{
    // operación BD
}
catch (EntityNotFoundException ex)
{
    throw; // Burbujea al controller → 404
}
catch (PersistenceException ex)
{
    throw; // Burbujea al controller → 500
}
catch (Exception ex) when (ex is not EntityNotFoundException and not PersistenceException)
{
    throw new PersistenceException("Error al [operación] el [entidad].", ex); // → 500
}
```

- `EntityNotFoundException` → 404 Not Found
- `PersistenceException` → 500 Internal Server Error
- `Exception` genérico → wrap en `PersistenceException` → 500

---

## Testing de Servicios

```csharp
// Mock example (xUnit + Moq)
var mockService = new Mock<ICrudJsonService<Alumno>>();
mockService.Setup(s => s.GetById(1)).Returns(new Alumno { Id = 1, Nombre = "Test" });
mockService.Setup(s => s.GetAll()).Returns(new List<Alumno> { ... });

var controller = new AlumnosController(mockService.Object, mockCarreraService.Object);
// Act & Assert
```