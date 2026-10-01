using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;

namespace Instituto.BR.Services;

public class FormularioService : IFormularioService
{
    private readonly IFormularioRepository _repository;

    public FormularioService(IFormularioRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<Formulario>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Formulario?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<ServiceResult<Formulario>> CreateAsync(Formulario formulario)
    {
        if (string.IsNullOrWhiteSpace(formulario.Nombre))
            return ServiceResult<Formulario>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(formulario.Estado))
            return ServiceResult<Formulario>.Fail("El estado es obligatorio.");

        if (formulario.FechaApertura == default)
            return ServiceResult<Formulario>.Fail("La fecha de apertura es obligatoria.");

        if (formulario.FechaCierre == default)
            return ServiceResult<Formulario>.Fail("La fecha de cierre es obligatoria.");

        if (formulario.FechaCierre <= formulario.FechaApertura)
            return ServiceResult<Formulario>.Fail("La fecha de cierre debe ser posterior a la de apertura.");

        var validEstados = new[] { "Borrador", "Abierto", "Cerrado" };
        if (!new[] { "Borrador", "Abierto", "Cerrado" }.Contains(formulario.Estado))
            return ServiceResult<Formulario>.Fail("El estado debe ser: Borrador, Abierto o Cerrado.");

        var created = await _repository.CreateAsync(formulario);
        return ServiceResult<Formulario>.Ok(created, "Formulario creado correctamente.");
    }

    public async Task<ServiceResult<Formulario>> UpdateAsync(int id, Formulario formulario)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult<Formulario>.Fail($"El formulario con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(formulario.Nombre))
            return ServiceResult<Formulario>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(formulario.Estado))
            return ServiceResult<Formulario>.Fail("El estado es obligatorio.");

        if (!formulario.FechaApertura.HasValue)
            return ServiceResult<Formulario>.Fail("La fecha de apertura es obligatoria.");

        if (!formulario.FechaCierre.HasValue)
            return ServiceResult<Formulario>.Fail("La fecha de cierre es obligatoria.");

        if (formulario.FechaCierre <= formulario.FechaApertura)
            return ServiceResult<Formulario>.Fail("La fecha de cierre debe ser posterior a la de apertura.");

        var validEstados = new[] { "Borrador", "Abierto", "Cerrado" };
        if (!new[] { "Borrador", "Abierto", "Cerrado" }.Contains(formulario.Estado))
            return ServiceResult<Formulario>.Fail("El estado debe ser: Borrador, Abierto o Cerrado.");

        await _repository.UpdateAsync(id, formulario);
        return ServiceResult<Formulario>.Ok(formulario, "Formulario actualizado correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult.Fail($"El formulario con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Formulario eliminado correctamente.");
    }
}