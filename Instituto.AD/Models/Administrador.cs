namespace Instituto.AD.Models;

public class Administrador : Persona
{
    public string Role { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PasswordTemp { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}