using System.Text.Json;
using Instituto.BR.Interfaces;
using Instituto.BR.Services;
using Instituto.AD.Interfaces;
using Instituto.AD.Repositories;
using Instituto.AD;
using Instituto.AD.Models;
using Instituto.MinimalAPI.Models;
using Instituto.BR.DTOs;
using Microsoft.EntityFrameworkCore;
using Instituto.AD.Data;

var builder = WebApplication.CreateBuilder(args);

// ── CORS para el front-end Vue ───────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
}

// ── JSON Options ──────────────────────────────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.SerializerOptions.WriteIndented = false;
});

// ── EF Core ───────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("ConnectionStrings:SqlServer no configurado.");

builder.Services.AddDbContext<InstitutoDbContext>(options =>
    options.UseSqlServer(connectionString));

// ── Repositories (AD Layer) ─────────────────────────────────────────────────
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IAlumnoRepository, AlumnoRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();
builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddScoped<IFormularioRepository, FormularioRepository>();
builder.Services.AddScoped<IListadoRepository, ListadoRepository>();

// ── Services (BR Layer) ─────────────────────────────────────────────────────
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IAlumnoService, AlumnoService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IFormularioService, FormularioService>();
builder.Services.AddScoped<IListadoService, ListadoService>();

// ── Swagger/OpenAPI ──────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Instituto API",
        Version = "v1",
        Description = "API del Sistema de Gestión Institucional - Instituto Superior Docente Túpac Amaru"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Global Exception Handler ────────────────────────────────────────────────
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var body = System.Text.Json.JsonSerializer.Serialize(new { error = "Ocurrió un error interno del servidor." });
        await context.Response.WriteAsync(body);
    });
}

// ── Middleware Pipeline ─────────────────────────────────────────────────────
app.UseRouting();
app.UseCors("VueCors");

// ── Swagger UI ──────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Health check ────────────────────────────────────────────────────────────
app.MapGet("/", () => "Backend corriendo correctamente!");

// ============================================================================
// MINIMAL API ENDPOINTS
// ============================================================================

// Helper functions
static IResult OkResponse<T>(T data, string message = "Operación exitosa")
    => Results.Ok(new { isSuccess = true, message = "Operación exitosa", data });

static IResult ErrorResponse(string message, int statusCode = 400)
    => Results.Json(new { isSuccess = false, message }, statusCode: statusCode);

// ============================================================================
// AUTH ENDPOINTS
// ============================================================================
var authGroup = app.MapGroup("/api/auth").WithTags("Auth");

authGroup.MapPost("/login", async (LoginRequest dto, IAdministradorService authService) =>
{
    if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest(new { isSuccess = false, message = "Email y contraseña requeridos" });

    var admin = await Task.FromResult(authService.Login(dto.Email, dto.Password));

    if (admin is null)
        return Results.Unauthorized();

    var sessionToken = Guid.NewGuid().ToString("N");
    var expira = DateTime.UtcNow.AddHours(8);

    return Results.Ok(new
    {
        isSuccess = true,
        message = "Operación exitosa",
        data = new
        {
            Token = sessionToken,
            ExpiraEn = expira,
            Admin = admin
        };
    }).WithName("Login").AllowAnonymous();

authGroup.MapPost("/verify-password", async (VerifyPasswordRequest dto, IAdministradorService authService) =>
{
    if (string.IsNullOrWhiteSpace(dto.Email))
        return Results.BadRequest(new { isSuccess = false, message = "Email requerido para verificar contraseña" });

    var admin = await Task.FromResult(authService.Login(dto.Email, dto.Password));

    if (admin is null)
        return Results.Unauthorized();

    return Results.Ok(new { isSuccess = true, message = "Contraseña verificada correctamente" });
}).WithName("VerifyPassword");

// ============================================================================
// SETUP ENDPOINT
// ============================================================================
app.MapPost("/api/setup/admin", async (SetupAdminDto dto, IServiceProvider services) =>
{
    if (!app.Environment.IsDevelopment())
        return Results.NotFound();

    var authService = services.GetRequiredService<IAdministradorService>();

    if (authService.HayAdmins())
        return Results.Conflict(new { isSuccess = false, message = "Ya existe al menos un administrador. Este endpoint está deshabilitado." });

    var admin = await authService.CrearPrimerAdminAsync(dto);

    if (admin is null)
        return Results.Json(new { isSuccess = false, message = "Error al crear el administrador inicial." }, statusCode: 500);

    return Results.Ok(new { isSuccess = true, message = $"Administrador '{admin.Email}' creado correctamente. Este endpoint ya no puede volver a usarse." })
        .WithName("CrearPrimerAdmin")
        .AllowAnonymous();
}).AllowAnonymous();

// ============================================================================
// MINIMAL API ENDPOINTS - HEALTH, STATS, SETUP
// ============================================================================

// Health check
app.MapGet("/health", async (Instituto.AD.Data.InstitutoDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "OK", timestamp = DateTime.UtcNow, database = "connected" });
    }
    catch
    {
        return Results.Ok(new { status = "error", timestamp = DateTime.UtcNow, database = "error" });
    }
}).WithName("HealthCheck").WithTags("Health");

// Stats endpoint
app.MapGet("/api/stats", async (IServiceProvider services) =>
{
    var carreraService = services.GetRequiredService<ICarreraService>();
    var alumnoService = services.GetRequiredService<IAlumnoService>();
    var adminService = services.GetRequiredService<IAdministradorService>();
    var profesorService = services.GetRequiredService<IProfesorService>();
    var formularioService = services.GetRequiredService<IFormularioService>();

    var carreras = await Task.FromResult((await carreraService.GetAllAsync()).Count);
    var alumnos = await Task.FromResult((await alumnoService.GetAllAsync()).Count);
    var admins = await Task.FromResult((await adminService.GetAllAsync()).Count);
    var profesores = await Task.FromResult((await profesorService.GetAllAsync()).Count);
    var formularios = await Task.FromResult((await formularioService.GetAllAsync()).Count);

    return Results.Ok(new
    {
        isSuccess = true,
        message = "Operación exitosa",
        data = new
        {
            totalCarreras = carreras,
            totalAlumnos = alumnos,
            totalAdmins = admins,
            totalProfesores = profesores,
            totalFormularios = formularios
        }
    });
}).WithName("GetStats").WithTags("Stats").RequireAuthorization();

// Setup status
app.MapGet("/api/setup/status", async (IAdministradorService authService) =>
{
    var hayAdmins = await authService.HayAdminsAsync();
    return Results.Ok(new { isSuccess = true, message = "Operación exitosa", data = new { hasAdmin = hayAdmins } });
}).WithName("SetupStatus").WithTags("Setup").RequireAuthorization();

// Setup first admin (moved from SetupController)
app.MapPost("/api/setup/first-admin", async (SetupAdminDto dto, IServiceProvider services) =>
{
    if (!app.Environment.IsDevelopment())
        return Results.NotFound();

    var authService = services.GetRequiredService<IAdministradorService>();

    if (authService.HayAdmins())
        return Results.Conflict(new { isSuccess = false, message = "Ya existe al menos un administrador. Este endpoint está deshabilitado." });

    var admin = await authService.CrearPrimerAdminAsync(dto);

    if (admin is null)
        return Results.Json(new { isSuccess = false, message = "Error al crear el administrador inicial." }, statusCode: 500);

    return Results.Ok(new { isSuccess = true, message = $"Administrador '{admin.Email}' creado correctamente. Este endpoint ya no puede volver a usarse." })
        .WithName("CrearPrimerAdmin")
        .AllowAnonymous();
}).AllowAnonymous();

// Run
app.Run();

// Make Program accessible for integration tests
public partial class Program { }