namespace Instituto.AD.Models;

public class ListadoItem
{
    public int AlumnoId { get; set; }
    public string? NombreCompleto { get; set; }
    public int DNI { get; set; }
    public string? Email { get; set; }
    public string? Carrera { get; set; }
    public string? Turno { get; set; }
    public int Edad { get; set; }
}