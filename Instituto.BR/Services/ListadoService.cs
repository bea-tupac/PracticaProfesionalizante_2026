using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class ListadoService : IListadoService
{
    private readonly IListadoRepository _repository;

    public ListadoService(IListadoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<AlumnoListadoDto>> GetListadoAsync()
    {
        var items = await _repository.GetListadoAsync();
        return items.Select(i => new AlumnoListadoDto
        {
            AlumnoId = i.AlumnoId,
            NombreCompleto = i.NombreCompleto,
            DNI = i.DNI,
            Email = i.Email,
            Carrera = i.Carrera,
            Turno = i.Turno,
            Edad = i.Edad
        }).ToList();
    }
}