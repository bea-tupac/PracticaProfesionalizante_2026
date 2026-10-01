using Instituto.AD.Models;

namespace Instituto.BR.DTOs;

public record AdminResult(
    int Id,
    string Nombre,
    string Apellido,
    string Email,
    string Role
);

public record LoginDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record SetupAdminDto
{
    public string Nombre { get; init; } = string.Empty;
    public string Apellido { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = "Admin";
}

public record ChangePasswordDto
{
    public string PasswordActual { get; init; } = string.Empty;
    public string NuevaPassword { get; init; } = string.Empty;
}

public record VerifyPasswordDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record AdminCreateDto
{
    public string Nombre { get; init; } = string.Empty;
    public string Apellido { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = "Admin";
    public string Password { get; init; } = string.Empty;
}

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

public class ServiceResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public static ServiceResult Ok(string message = "Operación exitosa", object? data = null)
        => new() { Success = true, Message = message, Data = data };

    public static ServiceResult Fail(string message)
        => new() { Success = false, Message = message };
}

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string message = "Operación exitosa")
        => new() { Success = true, Message = message, Data = data };

    public static ServiceResult<T> Fail(string message)
        => new() { Success = false, Message = message };
}