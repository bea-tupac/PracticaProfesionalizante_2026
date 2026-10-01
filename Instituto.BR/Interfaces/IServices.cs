using Instituto.AD.Models;
using Instituto.BR.DTOs;

namespace Instituto.BR.Interfaces;

public interface ICarreraService
{
    Task<List<Carrera>> GetAllAsync();
    Task<Carrera?> GetByIdAsync(int id);
    Task<ServiceResult<Carrera>> CreateAsync(Carrera carrera);
    Task<ServiceResult<Carrera>> UpdateAsync(int id, Carrera carrera);
    Task<ServiceResult> DeleteAsync(int id);
}

public interface IAlumnoService
{
    Task<List<Alumno>> GetAllAsync();
    Task<Alumno?> GetByIdAsync(int id);
    Task<ServiceResult<Alumno>> CreateAsync(Alumno alumno);
    Task<ServiceResult<Alumno>> UpdateAsync(int id, Alumno alumno);
    Task<ServiceResult> DeleteAsync(int id);
}

public interface IAdministradorService
{
    Task<List<Administrador>> GetAllAsync();
    Task<Administrador?> GetByIdAsync(int id);
    Task<ServiceResult<Administrador>> CreateAsync(Administrador admin, string password);
    Task<ServiceResult<Administrador>> UpdateAsync(int id, Administrador admin);
    Task<ServiceResult> ChangePasswordAsync(int id, string passwordActual, string nuevaPassword);
    Task<ServiceResult> DeleteAsync(int id);
    Task<AdminResult?> LoginAsync(string email, string password);
    Task<bool> HayAdminsAsync();
    Task<AdminResult?> CrearPrimerAdminAsync(SetupAdminDto dto);
    Task<ServiceResult> ChangePasswordByEmailAsync(string email, string passwordActual, string nuevaPassword);
}

public interface IProfesorService
{
    Task<List<Profesor>> GetAllAsync();
    Task<Profesor?> GetByIdAsync(int id);
    Task<ServiceResult<Profesor>> CreateAsync(Profesor profesor);
    Task<ServiceResult<Profesor>> UpdateAsync(int id, Profesor profesor);
    Task<ServiceResult> DeleteAsync(int id);
}

public interface IFormularioService
{
    Task<List<Formulario>> GetAllAsync();
    Task<Formulario?> GetByIdAsync(int id);
    Task<ServiceResult<Formulario>> CreateAsync(Formulario formulario);
    Task<ServiceResult<Formulario>> UpdateAsync(int id, Formulario formulario);
    Task<ServiceResult> DeleteAsync(int id);
}

public interface IListadoService
{
    Task<List<AlumnoListadoDto>> GetListadoAsync();
}