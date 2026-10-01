namespace Instituto.AD.Models;

public class Profesor : Persona
{
    public string? Telefono { get; set; }
    public string? Especialidad { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}