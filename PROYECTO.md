# Sistema de Gestión Institucional — Instituto Superior Docente Túpac Amaru

> Documento de referencia técnica para desarrollo y evolución del proyecto.
> Última actualización: 2026-09-28 | Versión: 1.1.0

---

## Índice

1. [Concepto del Sistema](#1-concepto-del-sistema)
2. [Arquitectura General](#2-arquitectura-general)
3. [Estructura de Directorios](#3-estructura-de-directorios)
4. [Back-end — ASP.NET Core 9 + SQL Server](#4-back-end--aspnet-core-9--sql-server)
5. [Front-end — Vue 3 + JavaScript + Vite](#5-front-end--vue-3--javascript--vite)
6. [Paradigma y Metodología de Desarrollo](#6-paradigma-y-metodología-de-desarrollo)
7. [Flujo de Datos](#7-flujo-de-datos)
8. [Requisitos Funcionales y Técnicos](#8-requisitos-funcionales-y-técnicos)
9. [Observaciones Técnicas y Deuda Técnica](#9-observaciones-técnicas-y-deuda-técnica)
10. [Hoja de Ruta — Próximos Pasos](#10-hoja-de-ruta--próximos-pasos)
11. [Historial de Versiones](#11-historial-de-versiones)

---

## 1. Concepto del Sistema

El sistema es una **plataforma de gestión académica** para el Instituto Superior Docente Túpac Amaru. Centraliza la administración de:

- **Carreras** — CRUD completo + validación de integridad (no eliminar si hay alumnos inscriptos).
- **Alumnos** — Inscripción pública (sin auth) + listado admin con join carrera.
- **Administradores** — CRUD autenticado con JWT + cambio de contraseña + re-autenticación.
- **Profesores** — CRUD completo autenticado (backend listo, frontend pendiente).
- **Formularios** — CRUD completo autenticado (estados: Borrador/Abierto/Cerrado).
- **Listados** — Vista consolidada alumnos + carrera (join en memoria).
- **Autenticación** — Login JWT (8h expiración), BCrypt workFactor 12, Route Guards, Password toggle, Verify/Change password.

El sistema expone un **panel interno** accesible por personal administrativo (JWT) y un **formulario público de inscripción** para nuevos alumnos.

---

## 2. Arquitectura General (N-Tier: AD → BR → API)

```
┌────────────────────────────────────────────────────────────────────────────┐
│                        CLIENTE (Navegador)                                 │
│  Vue 3 + JavaScript + Vite + Element Plus + Pinia + Vue Router           │
└────────────────────────────────────────────────────────────────────────────┘
                                 │ HTTPS / REST API + JWT
                                 ▼
┌────────────────────────────────────────────────────────────────────────────┐
│                      ASP.NET CORE 9 API (Presentation)                     │
│  Controllers → BR Services → AD Repositories → SQL Server                  │
│  JWT Auth + BCrypt (workFactor:12) + Global Exception Handling             │
└────────────────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌────────────────────────────────────────────────────────────────────────────┐
│                    SQL SERVER (Express / LocalDB / Docker)                 │
│         Tablas: Administradores, Alumnos, Carreras,                        │
│                 Profesores, Formularios                                    │
└────────────────────────────────────────────────────────────────────────────┘
```

**Capas del Backend (N-Tier):**
- **API Layer** (`Instituto.API`): 8 Controllers, Middleware, DI, JWT, CORS, Global Exception Handler
- **BR Layer** (`Instituto.BR`): 6 Servicios con lógica de negocio, validaciones, Result Pattern (`ServiceResult<T>`)
- **AD Layer** (`Instituto.AD`): 6 Repositorios tipados, `AccesoDB` (ADO.NET wrapper), Entidades de dominio

**Patrones aplicados:**
- **Backend**: Repository pattern, Template Method (`AccesoDB`), DI, Result Pattern (`ServiceResult<T>`), Exception handling tipado
- **Frontend**: Composition API, Componentes por vista, CSS modular (BEM-like), Route Guards, Composable `useAuth`

## 3. Estructura de Directorios (Solution)

```
Instituto.sln
├── Instituto.AD/              # Data Access Layer
├── Instituto.BR/              # Business Rules Layer
├── Instituto.API/             # Presentation Layer (ASP.NET Core 9)
├── Instituto.AD.Test/         # Unit tests AD (MSTest + Moq) — 20 tests
├── Instituto.BR.Test/         # Unit tests BR (MSTest + Moq) — 17 tests
├── Instituto.API.Test/        # Integration tests API (MSTest + WebApplicationFactory) — 7 tests
├── Frontend/                  # Vue 3 + Vite + JavaScript
├── Docs/                      # Documentación técnica
│   ├── backend/               # Docs backend (arquitectura, BD, servicios, API, etc.)
│   ├── frontend/              # Docs frontend (router, auth, componentes, build)
│   ├── DATABASE.md            # Documentación completa BD (snapshot, DDL, datos)
│   └── README.md              # Índice de documentación
└── brana.sln                  # Solution file legacy
```

> **Nota**: El script `CreateDatabase.sql` **no existe en el repositorio**. El DDL completo está documentado en `docs/backend/02-base-de-datos/02-script-creacion.md` y `docs/DATABASE.md`.

## Responsabilidades por Capa

### API Layer (`Instituto.API`)
- **Recibe** HTTP requests, validan `ModelState`
- **Delegan** a servicios BR (no contienen lógica de negocio)
- **Manejan** excepciones de dominio → HTTP status codes
- **Retornan** `IActionResult` con JSON serializado (`ApiResponse<T>`)
- **Middleware**: JWT Auth, CORS, Global Exception Handler

### BR Layer (`Instituto.BR`)
- **Contienen** lógica de negocio y reglas de validación
- **Orquestan** validaciones cruzadas (ej. Alumno valida Carrera existe)
- **Ejecutan** operaciones via Repositories (AD)
- **Manejan** `ServiceResult<T>` pattern para éxito/fallo tipado
- **Servicios**: `CarreraService`, `AlumnoService`, `AdministradorService`, `ProfesorService`, `FormularioService`, `ListadoService`
- **Auth**: `AdministradorService` con BCrypt (workFactor: 12), JWT claims

### AD Layer (`Instituto.AD`)
- **Repositorios** tipados por entidad (`ICarreraRepository`, `IAlumnoRepository`, etc.)
- **AccesoDB**: Wrapper ADO.NET genérico (`ExecuteReader/NonQuery/Scalar`, parámetros tipados)
- **Mapeo**: `SqlDataReader` → Entidades (manual mapping)
- **SQL**: Parameterizado, sin ORM, sin reflexión
- **Entidades AD**: Separadas de BR/API, sin dependencias externas

---

## Modelos de Dominio (AD Layer)

```
Persona (abstract)
├── Id, Nombre, Apellido, Email
    ├── Administrador: +Role, PasswordHash, Activo, FechaCreacion
    ├── Alumno: +DNI, FechaNacimiento, Direccion, Nacionalidad, FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, Edad (calculada)
    ├── Profesor: +Telefono, Especialidad
    └── Carrera (independiente): Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion
    └── Formulario (independiente): Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion
```

### DTOs (BR Layer)
- **Input**: `LoginDto`, `SetupAdminDto`, `ChangePasswordDto`, `VerifyPasswordDto`
- **Output**: `AdminResult` (record), `ServiceResult<T>`, `ServiceResult`, `AlumnoListadoDto`, `ListadoItem`

---

## Exceptions (Cross-Cutting)

| Excepción | HTTP Status | Uso |
|-----------|-------------|-----|
| `EntityNotFoundException` | 404 | Recurso no encontrado / inactivo |
| `PersistenceException` | 500 | Error BD / IO / Constraint violation |
| `UnauthorizedAccessException` | 401 | Credenciales inválidas / password incorrecto |

---

## Convenciones de Nombres (Actualizadas)

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| **Projects** | `Instituto.{Layer}` | `Instituto.AD`, `Instituto.BR`, `Instituto.API` |
| **Controllers** | `{Entidad}Controller` | `AlumnosController`, `AuthController` |
| **Services** | `{Entidad}Service` | `AlumnoService`, `AdministradorService` |
| **Repositories** | `{Entidad}Repository` | `AlumnoRepository`, `CarreraRepository` |
| **Interfaces** | `I{Funcionalidad}` | `IAlumnoRepository`, `IAlumnoService` |
| **DTOs Input** | `{Accion}{Entidad}Dto` | `SetupAdminDto`, `LoginDto`, `ChangePasswordDto` |
| **DTOs Output** | `{Entidad}Result` / `{Entidad}Dto` | `AdminResult`, `AlumnoListadoDto`, `ServiceResult<T>` |
| **Exceptions** | `{Contexto}Exception` | `PersistenceException`, `EntityNotFoundException` |
| **Result Pattern** | `ServiceResult<T>` | `ServiceResult<Alumno>`, `ServiceResult` |
| **SQL Tables** | Plural PascalCase | `Administradores`, `Alumnos`, `Formularios` |
| **SQL Columns** | PascalCase | `Nombre`, `FechaNacimiento`, `PasswordHash` |
| **SQL Params** | `@NombreParametro` | `@Email`, `@Id`, `@PasswordHash` |

## 4. Back-end — ASP.NET Core 9 + SQL Server Express

### 4.1 Stack y Dependencias

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| Microsoft.AspNetCore | 9.0 (SDK) | Framework web |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.5 | Autenticación JWT Bearer |
| Microsoft.AspNetCore.OpenApi | 9.0.5 | Generación de OpenAPI/Swagger |
| Microsoft.Data.SqlClient | 5.2.2 | Driver SQL Server (ADO.NET) |
| BCrypt.Net-Next | 4.0.3 | Hash de contraseñas |
| System.Text.Json | Built-in | Serialización |

**Target Framework**: net9.0  
**Características C#**: Nullable reference types, implicit usings.

---

### 4.2 Punto de Entrada: Instituto.API/Program.cs

```csharp
// CORS para el front-end Vue
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        // Respuestas 401/403 en JSON (no HTML)
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx => {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = "No autenticado. Iniciá sesión." }));
            },
            OnForbidden = ctx => {
                ctx.Response.StatusCode = 403;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = "No tenés permiso para realizar esta acción." }));
            }
        };
    });

builder.Services.AddAuthorization();

// Controllers + JSON options (camelCase)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.WriteIndented = false;
    });

// Repositories (AD Layer)
var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer no configurado.");

builder.Services.AddScoped<ICarreraRepository>(_ => new CarreraRepository(connectionString));
builder.Services.AddScoped<IAlumnoRepository>(_ => new AlumnoRepository(connectionString));
builder.Services.AddScoped<IAdministradorRepository>(_ => new AdministradorRepository(connectionString));
builder.Services.AddScoped<IProfesorRepository>(_ => new ProfesorRepository(connectionString));
builder.Services.AddScoped<IFormularioRepository>(_ => new FormularioRepository(connectionString));
builder.Services.AddScoped<IListadoRepository>(_ => new ListadoRepository(connectionString));

// Services (BR Layer)
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IFormularioService, FormularioService>();
builder.Services.AddScoped<IListadoService, ListadoService>();

// Middleware global de errores
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error = "Ocurrió un error interno del servidor." });
        await context.Response.WriteAsync(body);
    });
});

// Pipeline
app.UseRouting();
app.UseCors("VueCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Backend corriendo correctamente!");
app.Run();
```

**Puertos (launchSettings.json):**
- HTTP: http://localhost:5127
- HTTPS: https://localhost:7244

---

### 4.3 Modelos y Jerarquía de Herencia

```
Persona (abstract)
├── Id       : int
├── Nombre   : string   [Required] [StringLength(100)]
├── Apellido : string   [Required] [StringLength(100)]
└── Email    : string   [Required] [EmailAddress] [StringLength(255)]
    │
    ├── Alumno
    │   ├── DNI              [Required] [Range(1000000, 99999999)]
    │   ├── FechaNacimiento  [Required]
    │   ├── Direccion        [StringLength(200)]
    │   ├── Nacionalidad     [StringLength(100)]
    │   ├── FechaInscripcion (default DateTime.Now)
    │   ├── Telefono         [Phone] [StringLength(50)]
    │   ├── TituloSecundario [StringLength(200)]
    │   ├── Turno            [StringLength(50)]
    │   ├── CarreraId        [Required] [Range(1, int.MaxValue)]  <-- FK
    │   └── Edad             (propiedad calculada -- no persiste)
    │
    ├── Administrador
    │   ├── Role             [Required] [StringLength(50)]  // Admin | SuperAdmin
    │   ├── PasswordHash     [Required] [StringLength(255)]
    │   └── Activo           [Required] (bit, default 1)     // Soft delete
    │
    └── Profesor
        ├── Telefono         [Phone] [StringLength(50)]
        └── Especialidad     [StringLength(100)]

Carrera  (independiente)
├── Id           : int
├── Nombre       : string  [Required] [StringLength(200)]
├── DuracionAnios: int     [Range(1, 10)]
├── Turno        : string? [StringLength(50)]
├── Modalidad    : string? [StringLength(50)]
├── Horario      : string? [StringLength(100)]
└── Estado       : string? [StringLength(50)]  // Activa | Inactiva (default Activa)

Formulario (independiente)
├── Id              : int
├── Nombre          : string  [Required] [StringLength(200)]
├── Estado          : string  [Required] [StringLength(20)]  // Borrador | Abierto | Cerrado
├── FechaApertura   : DateTime [Required]
├── FechaCierre     : DateTime [Required]
└── Descripcion     : string? [StringLength(1000)]
```

---

### 4.4 Excepciones Personalizadas

Ubicadas en Instituto.AD/Exceptions/. Desacoplan el servicio de la lógica HTTP.

#### EntityNotFoundException
Lanzada cuando una entidad buscada por Id no existe (o está inactiva para Admin).

```csharp
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
    public EntityNotFoundException(string entityName, int id)
        : base(entityName + " con Id " + id + " no fue encontrado/a.") { }
}
Se lanza en: GetById, Update, Delete (y GetById de Admin filtra Activo=1).
Capturada en: controladores -> 404 Not Found.

#### PersistenceException
Lanzada ante cualquier error de BD (SqlException, timeout, constraint violation, etc.).

public class PersistenceException : Exception
{
    public PersistenceException(string message) : base(message) { }
    public PersistenceException(string message, Exception inner) : base(message, inner) { }
}
Se lanza en: operaciones ADO.NET (ExecuteReader, ExecuteNonQuery, etc.).
Capturada en: controladores -> 500 Internal Server Error.

---

### 4.5 Data Transfer Objects (DTOs)

#### AdminResult (record inmutable)
public record AdminResult(
    int    Id,
    string Nombre,
    string Apellido,
    string Email,
    string Role
);
- Sin PasswordHash -> Nunca expone credenciales.
- Serializado directamente en JWT claims y response JSON.

#### AlumnoListadoDto
Proyección plana de un alumno con información de su carrera. Usada por ListadoController.

public class AlumnoListadoDto
{
    public int AlumnoId { get; set; }
    public string NombreCompleto { get; set; }  // Apellido, Nombre
    public int DNI { get; set; }
    public string Email { get; set; }
    public string Carrera { get; set; }         // Nombre carrera o Sin carrera
    public string Turno { get; set; }
    public int Edad { get; set; }
}

Otros DTOs:
- LoginDto -- email + password
- SetupAdminDto -- nombre, apellido, email, password, role
- ChangePasswordDto -- passwordActual, nuevaPassword
- VerifyPasswordDto -- password
- ListadoItem -- Item para listados (AD layer)

---

### 4.6 Capa de Servicios (Business Rules Layer -- Instituto.BR)

#### Registro en DI Container (API Layer)

builder.Services.AddScoped<ICarreraRepository>(_ => new CarreraRepository(connectionString));
builder.Services.AddScoped<IAlumnoRepository>(_ => new AlumnoRepository(connectionString));
builder.Services.AddScoped<IAdministradorRepository>(_ => new AdministradorRepository(connectionString));
builder.Services.AddScoped<IProfesorRepository>(_ => new ProfesorRepository(connectionString));
builder.Services.AddScoped<IFormularioRepository>(_ => new FormularioRepository(connectionString));
builder.Services.AddScoped<IListadoRepository>(_ => new ListadoRepository(connectionString));

builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IFormularioService, FormularioService>();
builder.Services.AddScoped<IListadoService, ListadoService>();

- Scoped: Una instancia por request HTTP
- Interfaces: Desacopla API de implementación concreta
- Capas: API -> BR -> AD (unidireccional)

---

#### AccesoDB (Data Access Layer - Instituto.AD)

Archivo: Instituto.AD/AccesoDB.cs

Wrapper ADO.NET genérico que encapsula Microsoft.Data.SqlClient.

Métodos Públicos:
| Método | Uso | Retorna |
|--------|-----|---------|
| GetData(sql, params?) | SELECT múltiple | SqlDataReader |
| Execute(sql, params) | INSERT/UPDATE/DELETE | int (rows affected) |
| ExecuteScalar(sql, params) | INSERT con SCOPE_IDENTITY / COUNT | object |

Manejo de Parámetros:
var parametros = new DBParameters()
    .Agregar("@Email", email.Trim().ToLower())
    .Agregar("@Id", id)
    .Agregar("@Fecha", fecha ?? (object)DBNull.Value);

---

#### Repositorios (AD Layer)

Interfaces específicas por entidad (no genérica):

```csharp
public interface ICarreraRepository {
    List<Carrera> GetAll();
    Carrera? GetById(int id);
    Carrera Create(Carrera entity);
    void Update(int id, Carrera entity);
    void Delete(int id);
    bool Exists(int id);
}

public interface IAlumnoRepository {
    List<Alumno> GetAll();
    Alumno? GetById(int id);
    Alumno Create(Alumno entity);
    void Update(int id, Alumno entity);
    void Delete(int id);
    bool Exists(int id);
    bool ExistsByDNI(int dni);
    bool ExistsByEmail(string email);
    bool ExistsByCarreraId(int carreraId);
}

// ... similar para IAdministradorRepository, IProfesorRepository, IFormularioRepository, IListadoRepository
```

Repositorios Implementados:

| Repositorio | Entidad | Métodos Especiales |
|-------------|---------|-------------------|
| CarreraRepository | Carrera | Exists(int id) |
| AlumnoRepository | Alumno | ExistsByDNI, ExistsByEmail, ExistsByCarreraId |
| AdministradorRepository | Administrador | GetByEmail, ExistsByEmail, Count, UpdatePasswordHash |
| ProfesorRepository | Profesor | ExistsByEmail |
| FormularioRepository | Formulario | Exists(int id) |
| ListadoRepository | ListadoItem (DTO) | GetListado() (join memoria) |

---

### 4.7 Servicios CRUD (BR Layer)

#### Result Pattern
public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    
    public static ServiceResult Ok(string message = Operación exitosa) => 
        new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => 
        new() { Success = false, Message = message };
}

public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    
    public static ServiceResult Ok(T data, string message = Operación exitosa) => 
        new() { Success = true, Message = message, Data = data };
    public static ServiceResult Fail(string message) => 
        new() { Success = false, Message = message };
}

Servicios Implementados:

| Servicio | Entidad | Validaciones Especiales |
|----------|---------|------------------------|
| CarreraService | Carrera | Duración 1-10 años, no eliminar si tiene alumnos |
| AlumnoService | Alumno | DNI/Email únicos, CarreraId existe, DNI 7-8 dígitos |
| AdministradorService | Administrador | Email único, password >=8 chars, BCrypt workFactor 12 |
| ProfesorService | Profesor | Email único |
| FormularioService | Formulario | Fechas válidas, estado válido, FechaCierre > FechaApertura |
| ListadoService | ListadoItem | Join memoria Alumno+Carrera |

Ejemplo: CarreraService.Delete (validación cruzada)
public ServiceResult Delete(int id)
{
    if (!repository.Exists(id))
        return ServiceResult.Fail("La carrera con Id " + id + " no existe.");

    // Validación cruzada: no eliminar si tiene alumnos
    if (alumnoRepository.ExistsByCarreraId(id))
        return ServiceResult.Fail("No se puede eliminar la carrera porque tiene alumnos inscriptos.");

    repository.Delete(id);
    return ServiceResult.Ok("Carrera eliminada correctamente.");
)

---

### 4.8 AdministradorService (Autenticación + CRUD Completo)

**Archivo:** `Instituto.BR/Services/AdministradorService.cs`  
**Interface:** `IAdministradorService`

```csharp
public interface IAdministradorService
{
    List<Administrador> GetAll();
    Administrador? GetById(int id);
    ServiceResult<Administrador> Create(Administrador admin, string password);
    ServiceResult<Administrador> Update(int id, Administrador admin);
    ServiceResult ChangePassword(int id, string passwordActual, string nuevaPassword);
    ServiceResult Delete(int id);
    AdminResult? Login(string email, string password);      // SÍNCRONO
    bool HayAdmins();
    Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto);
    ServiceResult ChangePasswordByEmail(string email, string passwordActual, string nuevaPassword);
}
```

> **Nota**: `Login` es **síncrono** (no `async`). Solo `CrearPrimerAdminAsync` es `async` por convención de setup inicial.

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
- **Síncrono**: Repositorio ADO.NET es sync, no usa `async/await`

#### Create (con Password)

```csharp
public ServiceResult<Administrador> Create(Administrador admin, string password)
{
    // Validaciones: Nombre, Apellido, Email obligatorios, password >= 8 chars
    admin.Email = admin.Email.ToLower().Trim();  // Normalizar ANTES de chequear duplicados
    
    if (_repository.ExistsByEmail(admin.Email))
        return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");

    admin.PasswordHash = BCryptNet.HashPassword(password, workFactor: 12);
    admin.Activo = true;
    admin.FechaCreacion = DateTime.Now;

    try { return ServiceResult<Administrador>.Ok(_repository.Create(admin), "..."); }
    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        { return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email."); }
}
```

#### ChangePassword / ChangePasswordByEmail

Verifican `passwordActual` con `BCrypt.Verify`, hashean `nuevaPassword` (workFactor 12), actualizan via `UpdatePasswordHash`.

---

### 4.9 Controladores y Endpoints API

Todos los controladores:
- Reciben servicios por DI en constructor.
- Validan ModelState.IsValid en POST/PUT.
- Retornan 201 Created (con Location header) en POST.
- Encapsulan en try-catch tipado: EntityNotFoundException->404, PersistenceException->500, UnauthorizedAccessException->401, Exception->500.
- [Authorize] en clase (excepto Auth, Setup, Carreras GET públicos).

AuthController -- /api/auth
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | /api/auth/login | Público | Login -> JWT (8h) + admin data |
| POST | /api/auth/verify-password | JWT | Verifica password actual del usuario autenticado |

SetupController -- /api/setup
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | /api/setup/admin | Público (solo Dev) | Crea primer admin si no hay ninguno. _env.IsDevelopment() gate. |

CarreraController -- /api/carreras
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | /api/carreras | Público | Catálogo público |
| GET | /api/carreras/{id} | Público | Detalle público |
| POST | /api/carreras | JWT | Crear |
| PUT | /api/carreras/{id} | JWT | Actualizar |
| DELETE | /api/carreras/{id} | JWT | Eliminar (validación: no hay alumnos) |

AlumnosController -- /api/alumnos
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | /api/alumnos | JWT | Listado admin |
| GET | /api/alumnos/{id} | JWT | Detalle |
| POST | /api/alumnos | Público | Inscripción web ([AllowAnonymous]) |
| PUT | /api/alumnos/{id} | JWT | Actualizar |
| DELETE | /api/alumnos/{id} | JWT | Eliminar |

Validación: CarreraId debe existir (carreraService.GetById en Create/Update).

AdministradorController -- /api/administradores
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | /api/administradores | JWT | Listado (solo Activo=1) |
| GET | /api/administradores/{id} | JWT | Detalle |
| POST | /api/administradores/with-password | JWT | **Crear con password** (endpoint recomendado) |
| POST | /api/administradores | JWT | Crear (retorna 400 indicando usar with-password) |
| PUT | /api/administradores/{id} | JWT | Actualizar (no password, no role) |
| PUT | /api/administradores/{id}/password | JWT | Cambiar password (verifica actual) |
| DELETE | /api/administradores/{id} | JWT | Soft delete (Activo=0) |

**Nota**: El POST normal retorna 400 dirigiendo al endpoint `with-password` para crear admin con password.

ProfesorController -- /api/profesores
CRUD completo idéntico a Administrador (sin soft delete, sin password change). [Authorize] en clase.

FormularioController -- /api/formularios
CRUD completo. [Authorize] en clase. Estados: Borrador | Abierto | Cerrado.

ListadoController -- /api/listado
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | /api/listado | JWT | Join en memoria Alumno+Carrera -> AlumnoListadoDto[] |

---

### 4.10 Base de Datos

SQL Server (Express / LocalDB / Docker)
Script DDL: `docs/backend/02-base-de-datos/02-script-creacion.md` o `docs/DATABASE.md`
*(No hay archivo `CreateDatabase.sql` físico en el repo)*

| Tabla | Columnas clave | Índices / Constraints |
|-------|----------------|----------------------|
| Administradores | Id, Nombre, Apellido, Email, PasswordHash, **PasswordTemp**, Role, Activo, FechaCreacion | PK Id, **UNIQUE parcial** Email (WHERE Activo=1), IX_Activo |
| Carreras | Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion | PK Id, CHECK DuracionAnios 1-10 (en servicio), CHECK Estado |
| Alumnos | Id, Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad, FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion | PK Id, UNIQUE DNI, UNIQUE Email, FK CarreraId->Carreras, IX_CarreraId, IX_FechaInscripcion |
| Profesores | Id, Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion | PK Id, UNIQUE Email |
| Formularios | Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion | PK Id (estados validados en servicio) |

**Notas clave:**
- **Soft delete** en Administradores: `Activo=0` en lugar de DELETE físico
- **Índice UNIQUE parcial** permite reutilizar emails de admins inactivos
- Validaciones de CHECK (DuracionAnios, Estado formulario) están en **servicios BR**, no en BD
- `PasswordTemp` existe en modelo/tabla pero no se usa actualmente

---

### 4.11 Configuración

**appsettings.json (base - LocalDB / Windows Auth):**
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=(localdb)\\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "REEMPLAZAR_CON_CLAVE_SECRETA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "InstitutoTupacAmaru",
    "Audience": "InstitutoTupacAmaruAdmin"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

**appsettings.Development.json (SQL Auth - usado actualmente):**
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=InstitutoDB;User Id=instituto_user;Password=Instituto2026;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "Tupac@Amaru#Instituto!JWT$2026*Clave&MuySecreta=32chars",
    "Issuer": "InstitutoTupacAmaru",
    "Audience": "InstitutoTupacAmaruAdmin"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } }
}
```

> **Nota**: `appsettings.Development.json` está en el repo con credenciales de desarrollo. En producción usar variables de entorno / Key Vault.

## 5. Front-end — Vue 3 + JavaScript + Vite

### 5.1 Stack y Dependencias (Versiones Exactas)

#### Producción
| Paquete | Versión | Propósito |
|---------|---------|-----------|
| vue | **3.5.18** | Framework reactivo (Composition API) |
| vite | **7.0.6** | Bundler & Dev Server |
| vue-router | **4.5.1** | SPA Routing + Guards |
| pinia | **3.0.3** | Estado global (configurado; auth vía `useAuth` composable + sessionStorage) |
| element-plus | **2.11.1** | Componentes UI |
| @element-plus/icons-vue | **1.1.4** | Iconografía |

#### Desarrollo
| Paquete | Versión | Propósito |
|---------|---------|-----------|
| eslint | **9.31.0** | Linting |
| prettier | **3.6.2** | Formato |
| @vitejs/plugin-vue | **6.0.1** | Plugin Vue para Vite |
| vite-plugin-vue-devtools | **8.0.0** | DevTools en desarrollo |

---

### 5.2 Configuración

**.env:**
```env
VITE_API_URL=http://localhost:5127
```

**vite.config.js**: alias `@` -> `./src`, plugin Vue, devTools. **No hay proxy Vite configurado** - el frontend usa `fetch` directo a `VITE_API_URL`.

No hay `tsconfig` (proyecto en JavaScript puro).

---

### 5.3 Inicialización: main.js

```javascript
import './assets/css/base/main.css'
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import App from './App.vue'
import router from './router'
import { setRouter } from '@/composables/useApiFetch'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(ElementPlus)

// Inyectar el router en useApiFetch para manejo de 401/403
setRouter(router)

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.mount('#app')
```

---

### 5.4 Sistema de Rutas y Guards

**Archivo:** src/router/index.js

| Ruta | Nombre | Componente | Meta | Props |
|------|--------|------------|------|-------|
| / | home | HomeView | requiereAuth: true | - |
| /contacto | contacto | ContactoView | - | - |
| /inscripcion | inscripcion | InscripciónView | - | - |
| /login | login | LoginView | soloInvitado: true | - |
| /administracion | administracion | AdministradorView | requiereAuth: true | - |
| /agregaradministracion | agregaradministracion | AgregarAdministradorView | requiereAuth: true | - |
| /editaradministrador/:id | editaradministrador | EditarAdministradorView | requiereAuth: true | true |
| /eliminaradministrador/:id | eliminaradministrador | EliminarAdministradorView | requiereAuth: true | true |
| /carreras | carreras | CarreraView | requiereAuth: true | - |
| /agregarcarreras | agregarcarreras | AgregarCarreraView | requiereAuth: true | - |
| /editarcarrera/:id | editarcarrera | EditarCarreraView | requiereAuth: true | true |
| /eliminarcarreras/:id | eliminarcarreras | EliminarCarreraView | requiereAuth: true | true |
| /formularios | formularios | FormulariosView | requiereAuth: true | - |
| /agregarformulario | agregarformulario | AgregarFormularioView | requiereAuth: true | - |
| /editarformulario/:id | editarformulario | EditarFormularioView | requiereAuth: true | true |
| /eliminarformulario/:id | eliminarformulario | EliminarFormularioView | requiereAuth: true | true |
| /listados | listados | ListadoView | requiereAuth: true | - |
| * | not-found | NotFound | - | - |

**Guards globales (router.beforeEach):**
```javascript
router.beforeEach((to) => {
  const { isAuthenticated } = useAuth()
  const autenticado = isAuthenticated()

  if (to.meta.requiereAuth && !autenticado)
    return { name: 'login' }

  if (to.meta.soloInvitado && autenticado)
    return { name: 'home' }
})
```

---

### 5.5 Composable: useAuth.js

```javascript
const TOKEN_KEY = 'auth_token'
const ADMIN_KEY = 'auth_admin'

export function useAuth() {
  const getToken = () => {
    const token = sessionStorage.getItem(TOKEN_KEY)
    if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
      return null
    }
    return token
  }

  const getAdmin = () => {
    const raw = sessionStorage.getItem(ADMIN_KEY)
    if (!raw || raw === 'undefined' || raw === 'null') return null
    try {
      return JSON.parse(raw)
    } catch {
      return null
    }
  }

  const isAuthenticated = () => !!getToken()

  const guardarSesion = (token, admin) => {
    if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
      console.warn('[useAuth] Intentando guardar un token inválido:', token)
      return
    }
    sessionStorage.setItem(TOKEN_KEY, token)
    sessionStorage.setItem(ADMIN_KEY, JSON.stringify(admin))
  }

  const cerrarSesion = () => {
    sessionStorage.removeItem(TOKEN_KEY)
    sessionStorage.removeItem(ADMIN_KEY)
  }

  const authHeaders = () => {
    const token = getToken()
    return token
      ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
      : { 'Content-Type': 'application/json' }
  }

  return { getToken, getAdmin, isAuthenticated, guardarSesion, cerrarSesion, authHeaders }
}
```

- Almacenamiento: sessionStorage (expira al cerrar pestaña).
- Logout: Limpia sessionStorage + redirect /login.
- Re-autenticación sensible: EditarAdministradorView exige password actual antes de cargar datos (POST /api/auth/verify-password), guarda password verificada en ref memoria (se limpia en onUnmounted).

---

### 5.6 Vistas por Módulo

#### Módulo Público
| Vista | Ruta | Descripción |
|-------|------|-------------|
| HomeView | / | Dashboard admin (cards menú), health check ${API}/ |
| ContactoView | /contacto | Info contacto estática |
| FormulariosView | /formularios | Tabla formularios + CRUD |
| AgregarFormularioView | /agregarformulario | Formulario crear |
| EditarFormularioView | /editarformulario/:id | Formulario editar |
| EliminarFormularioView | /eliminarformulario/:id | Confirmar eliminar |

#### Módulo Auth
| Vista | Ruta | Descripción |
|-------|------|-------------|
| LoginView | /login | Login JWT + toggle password (View/Hide icons) |

#### Módulo Administradores
| Vista | Ruta | Descripción |
|-------|------|-------------|
| AdministradorView | /administracion | Listado tabla (Nombre, Apellido, Email, Acciones) |
| AgregarAdministradorView | /agregaradministracion | Formulario crear (role select, passwordTemp oculto) |
| EditarAdministradorView | /editaradministrador/:id | Re-auth modal -> carga datos -> PUT datos + opcional PUT password |
| EliminarAdministradorView | /eliminaradministrador/:id | Confirmar + DELETE |

#### Módulo Carreras
| Vista | Ruta | Descripción |
|-------|------|-------------|
| CarreraView | /carreras | Listado tabla (Nombre, Duración, Turno, Modalidad, Horario, Estado, Acciones) |
| AgregarCarreraView | /agregarcarreras | Formulario crear |
| EditarCarreraView | /editarcarrera/:id | Formulario editar |
| EliminarCarreraView | /eliminarcarreras/:id | Confirmar + DELETE (error si hay alumnos) |

#### Módulo Listados
| Vista | Ruta | Descripción |
|-------|------|-------------|
| ListadoView | /listados | Tabla Alumno+Carrera (join en memoria) |
| InscripciónView | /inscripcion | Público — Formulario extenso, carga carreras dinámicas, POST /api/alumnos |

> **Total: 19 vistas** (1 Auth + 1 Dashboard + 4 Administradores + 4 Carreras + 4 Formularios + 1 Listado + 1 Inscripción + 1 Contacto + 1 404)

> **Nota**: Profesores tiene backend CRUD completo ✅ pero **Frontend sin vistas** ❌

---

### 5.7 Estilos CSS (Modular, sin `<style>` en .vue)

```
src/assets/css/
├── base/
│   ├── main.css        # @import de todo
│   └── global.css      # Variables CSS, reset, utilidades
├── components/
│   ├── buttons.css     # .btn, variants, sizes
│   ├── card.css        # .card, .card-center, .card-lg, .card-header, .card-title
│   ├── forms.css       # .form, .form-row, .field, .form-actions
│   ├── inputs.css      # Inputs, selects, .password-field, .password-toggle
│   ├── table.css       # .table, .table-header, .table-row, .table-empty-state, badges
│   ├── navbar.css      # .navbar, .menu
│   └── admin-menu.css  # Grid botones dashboard
└── layout/
    └── section.css     # .section (centrado + padding)
```

**Principio:** Las vistas solo usan clases globales. Variables en `global.css` (single source of truth).
*Nota: El archivo actual se llama `innputs.css` (typo conocido, ver deuda técnica)*

---

### 5.8 Integración con la API

Base URL: import.meta.env.VITE_API_URL (configurado en .env)

Headers: useAuth().authHeaders() -> Bearer token automático.

Patrón estándar:
```typescript
const API = import.meta.env.VITE_API_URL
const { authHeaders } = useAuth()

const res = await fetch(${API}/api/administradores, { headers: authHeaders() })
if (!res.ok) throw new Error(HTTP ${res.status})
const data = await res.json()
```

Endpoints consumidos por módulo:
- Ver Docs/frontend/10-frontend/06-integracion-api.md para tabla completa.

---

### 5.9 Configuración y Build

**.env:**
```env
VITE_API_URL=http://localhost:5127
```

**vite.config.js**: alias @ -> ./src, plugin Vue, devTools.

**Scripts disponibles:**
```bash
npm run dev        # Servidor desarrollo (Vite)
npm run build      # Build producción (type-check + vite build)
npm run preview    # Preview build
npm run lint       # ESLint + fix
npm run format     # Prettier
npm run type-check # vue-tsc --build
```



## 6. Paradigma y Metodología de Desarrollo

### 6.1 Paradigma: Programación Orientada a Objetos (POO)

#### Encapsulamiento
- `AccesoDB` encapsula ADO.NET, conexión, parámetros, mapeo `SqlDataReader → Entidad`.
- Repositorios usan `AccesoDB` vía composición.

#### Herencia
- `Persona` abstracta concentra propiedades comunes + validaciones.
- `Alumno`, `Administrador`, `Profesor` heredan y añaden campos específicos.
- Repositorios implementan interfaces específicas (`ICarreraRepository`, `IAlumnoRepository`, etc.), no herencia de clase base.

#### Polimorfismo
- Interfaces específicas (`ICarreraRepository`, `IAlumnoRepository`, etc.) permiten DI tipada por entidad.
- Controladores dependen de abstracción (`IServicio`), no implementación concreta.

---

### 6.2 Patrones Utilizados

| Patrón | Aplicación |
|--------|------------|
| **Repository** | Interfaces específicas por entidad (`ICarreraRepository`, `IAlumnoRepository`, etc.) + implementaciones tipadas |
| **Template Method** | `AccesoDB` define esqueleto ADO.NET (`GetData`, `Execute`, `ExecuteScalar`), repositorios lo usan vía composición |
| **DTO** | `AdminResult`, `AlumnoListadoDto` desacoplan representación de dominio |
| **Scoped DI** | Servicios por request (thread-safe, conexión por request) |
| **Separación de capas** | Controllers / Services / Models / DTOs / Exceptions |
| **Inyección de dependencias** | Constructores, no `new` directo |

---

### 6.3 Manejo de Errores — Estrategia por Capas

```
Repositorio (AD)                Servicio (BR)                 Controlador                   Cliente HTTP
─────────────────────           ──────────────────             ─────────────────────────      ───────────────
SqlException       → PersistenceEx → catch(PersistenceEx)      → catch(PersistenceEx)       →   500 + { error }
Id no encontrado   → EntityNotFound → catch(EntityNotFound)     → catch(EntityNotFound)      →   404 + { error }
UnauthorizedAccess                   catch(UnauthorizedAccess) → catch(UnauthorizedAccess)  →   401 + { error }
Exception          → PersistenceEx → catch(Exception)           → catch(Exception)           →   500 + { error }
                                        ↓ si escapa todo
                                     UseExceptionHandler global   →   500 + { error }
```

**Principios:**
- Ningún stack trace llega al cliente.
- Respuestas error siempre JSON: `{ "error": "..." }`.
- Middleware global `UseExceptionHandler` = red de seguridad final.
- Endpoints 401/403 devuelven JSON (configurado en `JwtBearerEvents`).

---

### 6.4 Convenciones de Código

| Convención | Detalle |
|------------|---------|
| **Validación declarativa** | Data Annotations en modelos (`[Required]`, `[Range]`, `[EmailAddress]`, `[Phone]`, `[StringLength]`) |
| **Validación imperativa** | `ModelState.IsValid` en todos los endpoints POST/PUT |
| **Respuestas HTTP semánticas** | `200`, `201` (Location), `204`, `400`, `401`, `403`, `404`, `500` |
| **No exposición de internos** | `catch (Exception)` → mensaje controlado, nunca stack trace |
| **Null safety** | Propiedades requeridas `= string.Empty`; `?` solo donde opcional |
| **Consistencia Id en PUT** | `id` de ruta usado, no del body |
| **Work factor BCrypt** | 12 (configurado en `AdministradorService`) |

---

## 7. Flujo de Datos

### Flujo: Inscripción Pública de Alumno
```
1. Usuario abre /inscripcion (público, sin NavBar)
   ↓
2. onMounted → GET ${API}/api/carreras (público) → Popula dropdown carreras
   ↓
3. Usuario completa formulario + clic "Inscribirse"
   ↓
4. Validación client-side (Nombre, Apellido, DNI, CarreraId requeridos)
   ↓
5. fetch POST ${API}/api/alumnos (público, [AllowAnonymous])
   Body: { Nombre, Apellido, Email, DNI, FechaNacimiento, ..., CarreraId }
   ↓
6. AlumnosController.Create() valida ModelState.IsValid
   ↓
7. _carreraService.GetById(alumno.CarreraId)
   Si no existe → 400 "La carrera con Id X no existe."
   ↓
8. alumno.FechaInscripcion = DateTime.Now
   ↓
9. _alumnoService.Create(alumno) [ADO.NET + SCOPE_IDENTITY()]
   ↓
10. Controller retorna 201 Created + Location: /api/alumnos/{id}
    ↓
11. Frontend muestra confirmación + resetea formulario
```

### Flujo: Login + Acceso Panel
```
1. Usuario en /login ingresa credenciales
   ↓
2. POST ${API}/api/auth/login
   ↓
3. AuthController → AdministradorService.Login(email, password)  [SÍNCRONO]
   - Busca admin (Email + Activo=1)
   - BCrypt.Verify(password, hash)
   - Retorna AdminResult o null
   ↓
4. Si OK → GenerarToken(AdminResult) → JWT (8h, claims: sub, email, name, role, jti)
   ↓
5. Frontend: useAuth().guardarSesion(token, admin) → sessionStorage
   ↓
6. Router guard permite acceso a rutas requiereAuth
   ↓
7. Redirect a / (HomeView)
```

### Flujo: Edición de Administrador con Re-autenticación
```
1. Usuario navega a /editaradministrador/:id
   ↓
2. Se abre modal re-autenticación (showReauthDialog = true)
   ↓
3. Usuario ingresa password actual
   ↓
4. POST ${API}/api/auth/verify-password { password } con JWT
   ↓
5. Si OK → passwordVerificada (ref en memoria)
   ↓
6. GET ${API}/api/administradores/${id} → Popula formulario
   ↓
7. Usuario edita datos → PUT ${API}/api/administradores/${id}
   ↓
8. Si cambia password → PUT ${API}/api/administradores/${id}/password
   Usa passwordVerificada (memoria) como passwordActual
   ↓
9. onUnmounted limpia passwordVerificada
```

---

## 8. Requisitos Funcionales y Técnicos

### 8.1 Requisitos Funcionales

| ID | Módulo | Requisito | Estado |
|----|--------|-----------|--------|
| RF-01 | Carreras | CRUD completo + validación integridad | ✅ |
| RF-02 | Alumnos | Inscripción pública + listado admin con join | ✅ |
| RF-03 | Administradores | CRUD autenticado + change password + re-auth | ✅ |
| RF-04 | Profesores | CRUD completo autenticado (backend) | ✅ Backend / ❌ Frontend |
| RF-05 | Formularios | CRUD completo autenticado (estados) | ✅ |
| RF-06 | Listados | Vista consolidada alumnos + carrera | ✅ |
| RF-07 | Auth | Login JWT (8h) + BCrypt 12 | ✅ |
| RF-08 | Auth | Route Guards + sessionStorage | ✅ |
| RF-09 | Auth | Verify/Change password | ✅ |
| RF-10 | Setup | Primer admin solo Dev (1 vez) | ✅ |

### 8.2 Requisitos Técnicos — Back-end
- .NET 9 SDK
- SQL Server (Express / LocalDB / Docker)
- User Secrets para Jwt:Key en dev (opcional, appsettings.Development.json está en repo)
- CORS AllowAnyOrigin (solo dev)

### 8.3 Requisitos Técnicos — Front-end
- Node.js 20.19.0+ / 22.12.0+ (según `package.json` engines)
- npm 10+ (incluido en Node)
- npm install en Frontend/
- npm run dev → http://localhost:5176
- Backend en http://localhost:5127

### 8.4 Comandos de Inicio

```bash
# 1. Base de Datos (SQL Server)
# Opción A: SQL Server Express / Developer Edition
#   Conectar con SSMS / Azure Data Studio / VS Code
#   Crear BD 'InstitutoDB' y ejecutar DDL (ver docs/backend/02-base-de-datos/02-script-creacion.md)
#
# Opción B: LocalDB (desarrollo)
#   Se crea automáticamente al ejecutar la API con appsettings.json

# 2. Back-end
cd Instituto.API
dotnet run --environment Development
# → http://localhost:5127 | https://localhost:7244

# 3. Front-end (otra terminal)
cd Frontend
npm install    # solo primera vez
npm run dev
# → http://localhost:5176

# 4. Crear primer admin (una sola vez, solo Development)
curl -X POST http://localhost:5127/api/setup/admin \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Super","apellido":"Admin","email":"admin@tupac.edu.ar","password":"Tupac123","role":"SuperAdmin"}'

# 5. Login
# Abrir http://localhost:5176/login con credenciales del paso 4 (Email: admin@tupac.edu.ar, Password: Tupac123)
```

---

## 9. Observaciones Técnicas y Deuda Técnica

### Bugs Críticos Activos
| Ubicación | Problema | Severidad |
|-----------|----------|-----------|
| `InscripciónView.vue` | `CarreraId: string` vs `number` (backend) → 400 potencial | Crítica |

### Deuda Técnica — Back-end
| Item | Prioridad | Estado |
|------|-----------|--------|
| Sin capa DTO para GET administradores (expone `PasswordHash`, `Role`) | Alta | ❌ |
| `ex.Message.Contains("Carrera")` frágil en `AlumnosController.cs` | Media | ❌ |
| Sin rate limiting | Alta | ❌ |
| Sin paginación en GET All | Media | ❌ |
| Sin Swagger/OpenAPI | Media | ❌ |
| Profesores: Backend completo pero Frontend sin vistas | Media | ❌ |

### Deuda Técnica — Front-end
| Item | Prioridad | Estado |
|------|-----------|--------|
| Sin capa de servicios API centralizada (`src/services/`) | Alta | ❌ |
| Nombre archivo `InscripciónView.vue` con `ó` (riesgo Linux/CI) | Media | ❌ |
| Typo `innputs.css` → `inputs.css` (archivo real: `innputs.css`) | Baja | ❌ |
| Backend devuelve `PasswordHash` y `Role` en GET administradores (debería usar DTO) | Alta | ❌ |

### Fortalezas del Diseño Actual
- Herencia `Persona` elimina duplicación en 3 entidades.
- `AccesoDB` genérico, template methods, ADO.NET robusto.
- Separación clara: Controllers / Services / Models / DTOs / Exceptions.
- Data Annotations centralizan validaciones en modelo (single source of truth).
- Excepciones tipadas → mapeo HTTP limpio sin acoplamiento.
- JWT stateless + BCrypt 12 + claims estándar.
- Frontend: Composition API, CSS modular, Route Guards, `useAuth` composable.
- `EditarAdministradorView`: re-autenticación segura + password en memoria (limpieza `onUnmounted`).
- Tests: 44 tests passing (20 AD + 17 BR + 7 API).

---

## 10. Hoja de Ruta — Próximos Pasos

### Completado (v1.1.0)
- [x] Arquitectura N-Tier (AD → BR → API) con SQL Server (Express / LocalDB)
- [x] JWT Authentication + BCrypt workFactor 12
- [x] 8 Controladores con CRUD completo
- [x] 6 Servicios CRUD + Auth integrada en `AdministradorService`
- [x] Excepciones tipadas + middleware global errores
- [x] Route Guards + sessionStorage + re-auth modal
- [x] CSS modular sin style en componentes
- [x] Documentación modular en Docs/ + `DATABASE.md`
- [x] 44 tests passing (MSTest + Moq)
- [x] Soft delete en Administradores con índice UNIQUE parcial
- [x] Response wrapper `ApiResponse<T>` + camelCase JSON

### Prioridad Crítica
- [ ] Corregir `CarreraId: string` → `number` en `InscripciónView.vue`
- [ ] Crear vistas de Profesores (Frontend)

### Prioridad Alta
- [ ] Capa de servicios API centralizada (`src/services/`)
- [ ] DTO para administradores (no exponer `PasswordHash`/`Role` en GET)
- [ ] Rate limiting (ASP.NET Core built-in)
- [ ] Paginación en listados
- [ ] Tests xUnit + integración
- [ ] Swagger/OpenAPI

### Prioridad Media
- [ ] Renombrar `InscripciónView.vue` → `InscripcionView.vue`
- [ ] Fix `ex.Message.Contains("Carrera")` → tipar excepción FK
- [ ] Renombrar `innputs.css` → `inputs.css`
- [ ] Validación `FechaCierre > FechaApertura` en backend (Formularios)

### Prioridad Baja
- [ ] Auditoria login (IP, user-agent, timestamp)
- [ ] Refresh tokens + revocación
- [ ] 2FA (TOTP)
- [ ] Policy-based authorization (SuperAdmin vs Admin)

---

## 11. Historial de Versiones

| Versión | Fecha | Cambios Principales |
|---------|-------|---------------------|
| **1.1.0** | 2026-09-28 | **Actualización docs completa**: DDL real en docs, SQL Auth + LocalDB, soft delete con UNIQUE parcial, `Login` sync, `ApiResponse<T>` wrapper, camelCase JSON, 44 tests, `DATABASE.md` snapshot, puertos 5127/5176, sin proxy Vite, `CreateDatabase.sql` no en repo. |
| 1.0.0 | 2026-09-27 | Migración completa a SQL Server + JWT: arquitectura N-Tier (AD/BR/API), 8 controllers, 6 servicios CRUD, AuthService, excepciones tipadas, middleware global, route guards, re-autenticación, CSS modular, documentación completa en Docs/. |
| 0.1.1 | 2026-04-15 | Corrección typo Roll→Role, 201 Created en POSTs, validación CarreraId, EnsureFileExists, SemaphoreSlim, middleware errores, Data Annotations. |
| 0.1.0 | 2026-03-01 | Backend JSON files (CrudJsonService), herencia Persona, 4 controladores, frontend Vue 3 + Element Plus básico, inscripción pública, listados. |

---

*Documento mantenido por el equipo de desarrollo — Instituto Superior Docente Túpac Amaru (2026)*