using System;

namespace Instituto.AD.Models;

public class Alumno : Persona
{
    public int DNI { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaInscripcion { get; set; }
    public string? Telefono { get; set; }
    public string? TituloSecundario { get; set; }
    public string? Turno { get; set; }
    public int CarreraId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public int Edad => (int)((DateTime.Now - FechaNacimiento).TotalDays / 365.25);
}