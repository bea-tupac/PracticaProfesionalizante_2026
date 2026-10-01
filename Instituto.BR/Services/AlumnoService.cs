using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class AlumnoService : IAlumnoService
{
    private readonly IAlumnoRepository _repository;
    private readonly ICarreraRepository _carreraRepository;

    public AlumnoService(IAlumnoRepository repository, ICarreraRepository carreraRepository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _carreraRepository = carreraRepository ?? throw new ArgumentNullException(nameof(carreraRepository));
    }

    public async Task<List<Alumno>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Alumno?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<ServiceResult<Alumno>> CreateAsync(Alumno alumno)
    {
        if (string.IsNullOrWhiteSpace(alumno.Nombre))
            return ServiceResult<Alumno>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(alumno.Apellido))
            return ServiceResult<Alumno>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(alumno.Email))
            return ServiceResult<Alumno>.Fail("El email es obligatorio.");

        if (alumno.DNI < 1000000 || alumno.DNI > 99999999)
            return ServiceResult<Alumno>.Fail("El DNI debe tener entre 7 y 8 dígitos.");

        if (alumno.FechaNacimiento == default)
            return ServiceResult<Alumno>.Fail("La fecha de nacimiento es obligatoria.");

        if (alumno.CarreraId <= 0)
            return ServiceResult<Alumno>.Fail("Debe seleccionar una carrera válida.");

        if (!_carreraRepository.ExistsAsync(alumno.CarreraId).Result)
            return ServiceResult<Alumno>.Fail($"La carrera con Id {alumno.CarreraId} no existe.");

        if (await _repository.ExistsByDNIAsync(alumno.DNI))
            return ServiceResult<Alumno>.Fail("Ya existe un alumno con ese DNI.");

        if (await _repository.ExistsByEmailAsync(alumno.Email))
            return ServiceResult<Alumno>.Fail("Ya existe un alumno con ese email.");

        alumno.FechaInscripcion ??= DateTime.Now;
        var created = await _repository.CreateAsync(alumno);
        return ServiceResult<Alumno>.Ok(created, "Alumno inscrito correctamente.");
    }

    public async Task<ServiceResult<Alumno>> UpdateAsync(int id, Alumno alumno)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult<Alumno>.Fail($"El alumno con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(alumno.Nombre))
            return ServiceResult<Alumno>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(alumno.Apellido))
            return ServiceResult<Alumno>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(alumno.Email))
            return ServiceResult<Alumno>.Fail("El email es obligatorio.");

        if (alumno.DNI < 1000000 || alumno.DNI > 99999999)
            return ServiceResult<Alumno>.Fail("El DNI debe tener entre 7 y 8 dígitos.");

        if (alumno.CarreraId <= 0)
            return ServiceResult<Alumno>.Fail("Debe seleccionar una carrera válida.");

        if (!_carreraRepository.ExistsAsync(alumno.CarreraId).Result)
            return ServiceResult<Alumno>.Fail($"La carrera con Id {alumno.CarreraId} no existe.");

        await _repository.UpdateAsync(id, alumno);
        return ServiceResult<Alumno>.Ok(alumno, "Alumno actualizado correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult.Fail($"El alumno con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Alumno eliminado correctamente.");
    }
}