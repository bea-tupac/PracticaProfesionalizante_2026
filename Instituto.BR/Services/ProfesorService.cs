using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class ProfesorService : IProfesorService
{
    private readonly IProfesorRepository _repository;

    public ProfesorService(IProfesorRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<Profesor>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Profesor?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<ServiceResult<Profesor>> CreateAsync(Profesor profesor)
    {
        if (string.IsNullOrWhiteSpace(profesor.Nombre))
            return ServiceResult<Profesor>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(profesor.Apellido))
            return ServiceResult<Profesor>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(profesor.Email))
            return ServiceResult<Profesor>.Fail("El email es obligatorio.");

        if (await _repository.ExistsByEmailAsync(profesor.Email))
            return ServiceResult<Profesor>.Fail("Ya existe un profesor con ese email.");

        var created = await _repository.CreateAsync(profesor);
        return ServiceResult<Profesor>.Ok(created, "Profesor creado correctamente.");
    }

    public async Task<ServiceResult<Profesor>> UpdateAsync(int id, Profesor profesor)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult<Profesor>.Fail($"El profesor con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(profesor.Nombre))
            return ServiceResult<Profesor>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(profesor.Apellido))
            return ServiceResult<Profesor>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(profesor.Email))
            return ServiceResult<Profesor>.Fail("El email es obligatorio.");

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<Profesor>.Fail($"El profesor con Id {id} no existe.");

        if (!existing.Email.Equals(profesor.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _repository.ExistsByEmailAsync(profesor.Email))
                return ServiceResult<Profesor>.Fail("Ya existe un profesor con ese email.");
        }

        await _repository.UpdateAsync(id, profesor);
        return ServiceResult<Profesor>.Ok(profesor, "Profesor actualizado correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult.Fail($"El profesor con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Profesor eliminado correctamente.");
    }
}