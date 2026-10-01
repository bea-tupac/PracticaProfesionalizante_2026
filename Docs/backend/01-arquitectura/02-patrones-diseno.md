# Patrones de Diseño Utilizados

## 1. Repository Pattern

**Ubicación**: `Instituto.AD/Interfaces/IRepository.cs` + implementaciones `Instituto.AD/Repositories/*Repository.cs`

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

- Abstrae el acceso a datos detrás de una interfaz genérica
- Permite testing con mocks y desacopla servicios de BD
- Cada entidad tiene su repositorio tipado (`CarreraRepository`, `AlumnoRepository`, etc.)

## 2. Template Method Pattern (AccesoDB)

**Ubicación**: `Instituto.AD/AccesoDB.cs`

```csharp
public class AccesoDB
{
    private readonly string _connectionString;

    // Métodos template (concretos en base)
    public SqlDataReader GetData(string sql, DBParameters? parametros = null) { ... }
    public int Execute(string sql, DBParameters? parametros = null) { ... }
    public object ExecuteScalar(string sql, DBParameters? parametros = null) { ... }

    // Gestión de conexión encapsulada
    private void AbrirConexion() { ... }
    private void CerrarConexion() { ... }
}
```

- Define el esqueleto de operaciones ADO.NET en una clase wrapper
- Repositorios usan `AccesoDB` vía composición (no herencia)
- Elimina duplicación de código de conexión/comando/lectura/parámetros
- Manejo seguro de `SqlConnection`, `SqlCommand`, `SqlDataReader` con `using`

## 3. Dependency Injection (Constructor Injection)

**Ubicación**: `Program.cs` + Controllers + Services

```csharp
// Registro
builder.Services.AddScoped<ICrudJsonService<Alumno>, AlumnoSqlServerService>();

// Uso en Controller
public AlumnosController(ICrudJsonService<Alumno> alumnoService) { ... }

// Uso en Service
public AdminAuthService(IConfiguration config) { ... }
```

- Inversión de control completa
- Ciclo de vida `Scoped` (por request HTTP)
- Facilita testing y desacoplamiento

## 4. Strategy Pattern (Auth)

**Ubicación**: `Services/IAdminAuthService.cs` + `AdminAuthService.cs`

```csharp
public interface IAdminAuthService
{
    Task<AdminResult?> LoginAsync(string email, string password);
    Task<bool> HayAdminsAsync();
    Task CrearAdminAsync(...);
}
```

- Permite cambiar implementación de auth sin tocar controllers
- Útil para testing (mock) o migración a IdentityServer/OAuth futuro

## 5. DTO Pattern (Data Transfer Objects)

**Ubicación**: `Models/` + `DTOs/`

| Tipo | Uso |
|------|-----|
| `LoginDto` | Input de login (validado con DataAnnotations) |
| `SetupAdminDto` | Input de setup inicial |
| `AdminResult` | Output de login (sin password hash) |
| `AlumnoListadoDto` | Vista aplanada para listados (join Alumno+Carrera) |

- Separa modelo de dominio de contrato de API
- Evita over-posting y expone solo datos necesarios

## 6. Exception Filter Pattern (Middleware)

**Ubicación**: `Program.cs` (lines 83-93)

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(
            new { error = "Ocurrió un error interno del servidor." }));
    });
});
```

- Captura excepciones no manejadas globalmente
- Respuesta JSON consistente (no HTML)
- Logs internos vs mensajes seguros al cliente

## 7. Options Pattern (Configuration)

**Ubicación**: `Program.cs` + `appsettings.json`

```csharp
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw ...;
builder.Services.AddAuthentication(...)
    .AddJwtBearer(options => { ... });
```

- Configuración tipada vía `IConfiguration`
- Secrets fuera del código (appsettings.Development.json, env vars)
- Validación temprana (`?? throw`)

## 8. Soft Delete Pattern

**Ubicación**: `AdministradorSqlServerService.cs` (Delete method)

```csharp
// Soft delete: marcamos activo = FALSE en lugar de borrar físicamente
var rows = ExecuteNonQuery(
    "UPDATE Administradores SET Activo = 0 WHERE Id = @Id AND Activo = 1", ...);
```

- Preserva integridad referencial e historial
- Filtro `WHERE Activo = 1` en todas las consultas
- Fácil recuperación y auditoría