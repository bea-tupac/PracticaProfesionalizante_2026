using BCryptNet = BCrypt.Net.BCrypt;
using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.BR.DTOs;
using Instituto.BR.Interfaces;
using Microsoft.Data.SqlClient;

namespace Instituto.BR.Services;

public class AdministradorService : IAdministradorService
{
    private readonly IAdministradorRepository _repository;

    public AdministradorService(IAdministradorRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<Administrador>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Administrador?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<ServiceResult<Administrador>> CreateAsync(Administrador admin, string password)
    {
        if (string.IsNullOrWhiteSpace(admin.Nombre))
            return ServiceResult<Administrador>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(admin.Apellido))
            return ServiceResult<Administrador>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(admin.Email))
            return ServiceResult<Administrador>.Fail("El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return ServiceResult<Administrador>.Fail("La contraseña debe tener al menos 8 caracteres.");

        // ⚠️ Normalizar ANTES de chequear duplicados
        admin.Email = admin.Email.ToLower().Trim();

        if (await _repository.ExistsByEmailAsync(admin.Email))
            return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");

        admin.PasswordHash = BCryptNet.HashPassword(password, workFactor: 12);
        admin.Activo = true;
        admin.FechaCreacion = DateTime.Now;

        try
        {
            var created = await _repository.CreateAsync(admin);
            return ServiceResult<Administrador>.Ok(created, "Administrador creado correctamente.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            // 2627 = Violation of UNIQUE KEY constraint
            // 2601 = Cannot insert duplicate key row with unique index
            return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");
        }
    }

    public async Task<ServiceResult<Administrador>> UpdateAsync(int id, Administrador admin)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult<Administrador>.Fail($"El administrador con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(admin.Nombre))
            return ServiceResult<Administrador>.Fail("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(admin.Apellido))
            return ServiceResult<Administrador>.Fail("El apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(admin.Email))
            return ServiceResult<Administrador>.Fail("El email es obligatorio.");

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            return ServiceResult<Administrador>.Fail($"El administrador con Id {id} no existe.");

        // ⚠️ Normalizar ANTES de comparar
        admin.Email = admin.Email.ToLower().Trim();

        if (!existing.Email.Equals(admin.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _repository.ExistsByEmailAsync(admin.Email))
                return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");
        }

        try
        {
            await _repository.UpdateAsync(id, admin);
            var updated = await _repository.GetByIdAsync(id);
            return ServiceResult<Administrador>.Ok(updated!, "Administrador actualizado correctamente.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            return ServiceResult<Administrador>.Fail("Ya existe un administrador con ese email.");
        }
    }

    public async Task<ServiceResult> ChangePasswordAsync(int id, string passwordActual, string nuevaPassword)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult.Fail($"El administrador con Id {id} no existe.");

        if (string.IsNullOrWhiteSpace(passwordActual))
            return ServiceResult.Fail("La contraseña actual es obligatoria.");

        if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
            return ServiceResult.Fail("La nueva contraseña debe tener al menos 8 caracteres.");

        var admin = await _repository.GetByIdAsync(id);
        if (admin == null)
            return ServiceResult.Fail($"El administrador con Id {id} no existe.");

        if (!BCryptNet.Verify(passwordActual, admin.PasswordHash))
            return ServiceResult.Fail("La contraseña actual es incorrecta.");

        var newHash = BCryptNet.HashPassword(nuevaPassword, workFactor: 12);
        await _repository.UpdatePasswordHashAsync(id, newHash);

        return ServiceResult.Ok("Contraseña actualizada correctamente.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return ServiceResult.Fail($"El administrador con Id {id} no existe.");

        await _repository.DeleteAsync(id);
        return ServiceResult.Ok("Administrador eliminado correctamente.");
    }

    public async Task<AdminResult?> LoginAsync(string email, string password)
    {
        var admin = await _repository.GetByEmailAsync(email);
        if (admin == null)
            return null;

        if (!BCryptNet.Verify(password, admin.PasswordHash))
            return null;

        return new AdminResult(admin.Id, admin.Nombre, admin.Apellido, admin.Email, admin.Role);
    }

    public async Task<bool> HayAdminsAsync()
        => await _repository.CountAsync() > 0;

    public async Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto)
    {
        if (await HayAdminsAsync())
            return null;

        var admin = new Administrador
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            Role = dto.Role
        };

        var result = await CreateAsync(admin, dto.Password);
        if (!result.Success)
            return null;

        return new AdminResult(result.Data!.Id, result.Data.Nombre, result.Data.Apellido, result.Data.Email, result.Data.Role);
    }

    public async Task<ServiceResult> ChangePasswordByEmailAsync(string email, string passwordActual, string nuevaPassword)
    {
        var admin = await _repository.GetByEmailAsync(email);
        if (admin == null)
            return ServiceResult.Fail("Usuario no encontrado.");

        if (!BCryptNet.Verify(passwordActual, admin.PasswordHash))
            return ServiceResult.Fail("Contraseña actual incorrecta.");

        if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
            return ServiceResult.Fail("La nueva contraseña debe tener al menos 8 caracteres.");

        var newHash = BCryptNet.HashPassword(nuevaPassword, workFactor: 12);
        await _repository.UpdatePasswordHashAsync(admin.Id, newHash);

        return ServiceResult.Ok("Contraseña actualizada correctamente.");
    }
}