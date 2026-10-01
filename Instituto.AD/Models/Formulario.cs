namespace Instituto.AD.Models;

public class Formulario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = "Borrador";
    public DateTime FechaApertura { get; set; }
    public DateTime FechaCierre { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}