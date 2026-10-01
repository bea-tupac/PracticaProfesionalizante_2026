using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class CarreraService : ICarreraService
{
    private readonly ICarreraRepository _repository;
    private readonly IAlumnoRepository _alumnoRepository;

    public CarreraService(ICarreraRepository repository, IAlumnoRepository alumnoRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _alumnoRepository = alumnoRepository ?? throw new ArgumentNullException(nameof(alumnoRepository));
    }

    public async Task<List<Carrera>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Carrera?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<ServiceResult<Carrera>> CreateAsync(Carrera carrera)
    {
        if (string.IsNullOrWhiteSpace(carrera.Nombre))
            return ServiceResult<Carrera>.Fail("El nombre de la carrera es obligatorio.");

        if (carrera.DuracionAnios < 1 || carrera.DuracionAnios > 10)
            return ServiceResult<Carrera>.Fail("La duración debe estar entre 1 y 10 años.");

        carrera.Estado ??= "Activa";
        var created = await _repository.CreateAsync(carrera);
        return ServiceResult<Carrera>.Ok(created, "Carrera creada correctamente.");
    }

    public async Task<ServiceResult<Carrera>> UpdateAsync(int id, Carrera carrera)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult<Carrera>.Fail($"La carrera con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(carrera.Nombre))
            return ServiceResult<Carrera>.Fail("El nombre de la carrera es obligatorio.");

        if (carrera.DuracionAnios < 1 || carrera.DuracionAnios > 10)
            return ServiceResult<Carrera>.Fail("La duración debe estar entre 1 y 10 años.");

        await _repository.UpdateAsync(id, carrera);
        return ServiceResult<Carrera>.Ok(carrera, "Carrera actualizada correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult.Fail($"La carrera con Id {id} no existe.");

        // Validación: no eliminar si tiene alumnos inscriptos
        if (await _alumnoRepository.ExistsByCarreraIdAsync(id))
            return ServiceResult.Fail("No se puede eliminar la carrera porque tiene alumnos inscriptos.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Carrera eliminada correctamente.");
    }
}