namespace Instituto.AD.Models;

public class Carrera
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int DuracionAnios { get; set; }
    public string? Turno { get; set; }
    public string? Modalidad { get; set; }
    public string? Horario { get; set; }
    public string? Estado { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}