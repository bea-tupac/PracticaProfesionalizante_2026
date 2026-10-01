# Capa de Servicios (Business Rules Layer - `Instituto.BR`)

## Arquitectura de Servicios

```
┌─────────────────────────────────────────────────────────────────┐
│                      ICrudService<T>                            │
│  GetAll() | GetById(id) | Create(e) | Update(id,e) | Delete(id) │
└────────────────────────────┬────────────────────────────────────┘
                             │
          ┌──────────────────┼──────────────────┐
          ▼                  ▼                  ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│ CarreraService  │ │ AlumnoService   │ │ ProfesorService │
└────────┬────────┘ └────────┬────────┘ └────────┬────────┘
         │                   │                   │
         └───────────────────┼───────────────────┘
                             ▼
                  ┌─────────────────────┐
                  │   Repositories      │
                  │   (AD Layer)        │
                  └──────────┬──────────┘
                             │
                    ┌────────┴────────┐
                    ▼                 ▼
            ┌─────────────┐   ┌─────────────┐
            │  SqlClient  │   │  BCrypt     │
            │ (ADO.NET)   │   │ (Hashing)   │
            └─────────────┘   └─────────────┘
```

## Registro en DI Container (API Layer)

**Archivo**: `Instituto.API/Program.cs`

```csharp
// Repositories (AD Layer)
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<IFormularioRepository, FormularioRepository>();
builder.Services.AddScoped<IListadoRepository, ListadoRepository>();

// Services (BR Layer)
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IFormularioService, FormularioService>();
builder.Services.AddScoped<IListadoService, ListadoService>();
```

- **Scoped**: Una instancia por request HTTP
- **Interfaces**: Desacopla API de implementación concreta
- **Capas**: API → BR → AD (unidireccional)

---

## AccesoDB (Data Access Layer - `Instituto.AD`)

**Archivo**: `Instituto.AD/AccesoDB.cs`

Wrapper ADO.NET genérico que encapsula `Microsoft.Data.SqlClient`.

### Responsabilidades
- Gestión de conexiones (`AbrirConexion()`)
- Ejecución de queries parameterizadas
- Manejo de parámetros tipados (`DBParameter` / `DBParameters`)

### Métodos Públicos

| Método | Uso | Retorna |
|--------|-----|---------|
| `GetData(sql, params?)` | SELECT múltiple | `SqlDataReader` |
| `Execute(sql, params)` | INSERT/UPDATE/DELETE | `int` (rows affected) |
| `ExecuteScalar(sql, params)` | INSERT con SCOPE_IDENTITY / COUNT | `object` |

### Manejo de Parámetros

```csharp
var parametros = new DBParameters()
    .Agregar("@Email", email.Trim().ToLower())
    .Agregar("@Id", id)
    .Agregar("@Fecha", fecha ?? (object)DBNull.Value);
```

---

## Repositorios (AD Layer)

Interfaz base genérica:

```csharp
public interface IRepository<T>
{
    List<T> GetAll();
    T? GetById(int id);
    T Create(T entity);
    void Update(int id, T entity);
    void Delete(int id);
    bool Exists(int id);
}
```

### Repositorios Implementados

| Repositorio | Entidad | Métodos Especiales |
|-------------|---------|-------------------|
| `CarreraRepository` | Carrera | `Exists(int id)` |
| `AlumnoRepository` | Alumno | `ExistsByDNI`, `ExistsByEmail`, `ExistsByCarreraId` |
| `AdministradorRepository` | Administrador | `GetByEmail`, `ExistsByEmail`, `Count`, `UpdatePasswordHash` |
| `ProfesorRepository` | Profesor | `ExistsByEmail` |
| `FormularioRepository` | Formulario | `Exists(int id)` |
| `ListadoRepository` | ListadoItem (DTO) | `GetListado()` (join memoria) |

### Patrones en Repositorios

- **Mapeo manual**: `SqlDataReader` → Entidad (sin reflexión)
- **Parámetros**: Siempre `@Param`, nunca string interpolation
- **NULL handling**: `DBNull.Value` para opcionales
- **SCOPE_IDENTITY()**: Para obtener ID tras INSERT

---

## Servicios CRUD (BR Layer)

Interfaz base genérica:

```csharp
public interface ICrudService<T>
{
    List<T> GetAll();
    T? GetById(int id);
    ServiceResult<T> Create(T entity);
    ServiceResult<T> Update(int id, T entity);
    ServiceResult Delete(int id);
}
```

### Result Pattern

```csharp
public class ServiceResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    
    public static ServiceResult Ok(string message = "Operación exitosa") => 
        new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => 
        new() { Success = false, Message = message };
}

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    
    public static ServiceResult<T> Ok(T data, string message = "Operación exitosa") => 
        new() { Success = true, Message = message, Data = data };
    public static ServiceResult<T> Fail(string message) => 
        new() { Success = false, Message = message };
}
```

### Servicios Implementados

| Servicio | Entidad | Validaciones Especiales |
|----------|---------|------------------------|
| `CarreraService` | Carrera | Duración 1-10 años, no eliminar si tiene alumnos |
| `AlumnoService` | Alumno | DNI/Email únicos, CarreraId existe, DNI 7-8 dígitos |
| `AdministradorService` | Administrador | Email único, password ≥8 chars, BCrypt workFactor 12 |
| `ProfesorService` | Profesor | Email único |
| `FormularioService` | Formulario | Fechas válidas, estado válido, FechaCierre > FechaApertura |
| `ListadoService` | ListadoItem | Join memoria Alumno+Carrera |

### Ejemplo: CarreraService.Delete (validación cruzada)

```csharp
public ServiceResult Delete(int id)
{
    if (!_repository.Exists(id))
        return ServiceResult.Fail($"La carrera con Id {id} no existe.");

    // Validación cruzada: no eliminar si tiene alumnos
    if (_alumnoRepository.ExistsByCarreraId(id))
        return ServiceResult.Fail("No se puede eliminar la carrera porque tiene alumnos inscriptos.");

    _repository.Delete(id);
    return ServiceResult.Ok("Carrera eliminada correctamente.");
}
```

---

## AdminAuthService (Autenticación)

**Archivo**: `Instituto.BR/Services/AdministradorService.cs`  
**Interface**: `IAdministradorService`

```csharp
public interface IAdministradorService
{
    List<Administrador> GetAll();
    Administrador? GetById(int id);
    ServiceResult<Administrador> Create(Administrador admin, string password);
    ServiceResult<Administrador> Update(int id, Administrador admin);
    ServiceResult ChangePassword(int id, string passwordActual, string nuevaPassword);
    ServiceResult Delete(int id);
    AdminResult? Login(string email, string password);
    bool HayAdmins();
    Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto);
    ServiceResult ChangePasswordByEmail(string email, string passwordActual, string nuevaPassword);
}
```

> **Nota**: `Login` es **síncrono** (no `async`). Solo `CrearPrimerAdminAsync` es `async` por convención de setup inicial.

### Implementación Detallada

#### Login (Síncrono)
```csharp
public AdminResult? Login(string email, string password)
{
    var admin = _repository.GetByEmail(email);
    if (admin == null) return null;
    if (!BCryptNet.Verify(password, admin.PasswordHash)) return null;
    
    return new AdminResult(admin.Id, admin.Nombre, admin.Apellido, admin.Email, admin.Role);
}
```

- **Case-insensitive email**: `Trim().ToLower()` (normalizado en repositorio)
- **Solo activos**: Repositorio filtra `Activo = 1` en `GetByEmail`
- **BCrypt verify**: Compara password plano vs hash almacenado
- **Síncrono**: No usa `async/await` (repositorio ADO.NET es sync)

#### CrearPrimerAdminAsync
```csharp
public async Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto)
{
    if (HayAdmins()) return null;
    
    var admin = new Administrador { Nombre = dto.Nombre, ... };
    var result = Create(admin, dto.Password);
    return result.Success ? new AdminResult(...) : null;
}
```

- **Work factor 12**: Balance seguridad/performance (2024+)
- **BCrypt.HashPassword**: Genera hash seguro
- **Solo si no hay admins**: `HayAdmins()` check

#### ChangePassword
```csharp
public ServiceResult ChangePassword(int id, string passwordActual, string nuevaPassword)
{
    var admin = _repository.GetById(id);
    if (admin == null) return ServiceResult.Fail("Admin no encontrado");
    
    if (!BCryptNet.Verify(passwordActual, admin.PasswordHash))
        return ServiceResult.Fail("Contraseña actual incorrecta");
    
    var newHash = BCryptNet.HashPassword(nuevaPassword, workFactor: 12);
    _repository.UpdatePasswordHash(id, newHash);
    return ServiceResult.Ok("Contraseña actualizada correctamente.");
}
```

---

## Registro en DI Container (API Layer)

**Archivo**: `Instituto.API/Program.cs`

```csharp
// Repositories (AD Layer)
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<IFormularioRepository, FormularioRepository>();
builder.Services.AddScoped<IListadoRepository, ListadoRepository>();

// Services (BR Layer)
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IFormularioService, FormularioService>();
builder.Services.AddScoped<IListadoService, ListadoService>();
```

---

## Buenas Prácticas en Servicios

| Práctica | Ejemplo |
|----------|---------|
| **Parámetros SQL** | Siempre `@Param`, nunca string interpolation |
| **Async/Await** | Todas las operaciones I/O |
| **Using** | `SqlConnection`, `SqlCommand`, `SqlDataReader` |
| **Excepciones tipadas** | `EntityNotFoundException`, `PersistenceException` |
| **Null handling** | `reader.IsDBNull(...) ? null : reader.GetX(...)` |
| **DBNull.Value** | Para parámetros opcionales nulos |
| **SCOPE_IDENTITY()** | Para obtener ID tras INSERT |
| **Result Pattern** | `ServiceResult<T>` para éxito/fallo tipado |
| **Validación temprana** | En servicio, antes de tocar BD |

---

## Servicios Adicionales

### FormularioService
- Validación: `FechaCierre > FechaApertura`
- Estados válidos: `Borrador`, `Abierto`, `Cerrado`

### ListadoService
- Join en memoria: Alumnos + Carreras
- Retorna `List<AlumnoListadoDto>` aplanado