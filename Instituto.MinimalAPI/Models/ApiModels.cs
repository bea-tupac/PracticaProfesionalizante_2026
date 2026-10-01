using Instituto.BR.DTOs;

namespace Instituto.MinimalAPI.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public AdminResult Admin { get; set; } = null!;
}

public class SetupAdminRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
}

public class ChangePasswordRequest
{
    public string PasswordActual { get; set; } = string.Empty;
    public string NuevaPassword { get; set; } = string.Empty;
}

public class VerifyPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public static ApiResponse Ok(string message = "Operación exitosa", object? data = null)
        => new() { IsSuccess = true, Message = message, Data = data };

    public static ApiResponse Fail(string message)
        => new() { IsSuccess = false, Message = message };
}

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string message = "Operación exitosa")
        => new() { IsSuccess = true, Message = message, Data = data };

    public static ApiResponse<T> Error(string message)
        => new() { IsSuccess = false, Message = message };
}