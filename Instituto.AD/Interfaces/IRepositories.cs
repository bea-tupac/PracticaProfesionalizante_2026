using Instituto.AD.Models;

namespace Instituto.AD.Interfaces;

public interface ICarreraRepository
{
    Task<List<Carrera>> GetAllAsync();
    Task<Carrera?> GetByIdAsync(int id);
    Task<Carrera> CreateAsync(Carrera entity);
    Task UpdateAsync(int id, Carrera entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IAlumnoRepository
{
    Task<List<Alumno>> GetAllAsync();
    Task<Alumno?> GetByIdAsync(int id);
    Task<Alumno> CreateAsync(Alumno entity);
    Task UpdateAsync(int id, Alumno entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByDNIAsync(int dni);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByCarreraIdAsync(int carreraId);
}

public interface IAdministradorRepository
{
    Task<List<Administrador>> GetAllAsync();
    Task<Administrador?> GetByIdAsync(int id);
    Task<Administrador?> GetByEmailAsync(string email);
    Task<Administrador> CreateAsync(Administrador entity);
    Task UpdateAsync(int id, Administrador entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<int> CountAsync();
    Task UpdatePasswordHashAsync(int id, string newHash);
}

public interface IProfesorRepository
{
    Task<List<Profesor>> GetAllAsync();
    Task<Profesor?> GetByIdAsync(int id);
    Task<Profesor> CreateAsync(Profesor entity);
    Task UpdateAsync(int id, Profesor entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByEmailAsync(string email);
}

public interface IFormularioRepository
{
    Task<List<Formulario>> GetAllAsync();
    Task<Formulario?> GetByIdAsync(int id);
    Task<Formulario> CreateAsync(Formulario entity);
    Task UpdateAsync(int id, Formulario entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IListadoRepository
{
    Task<List<ListadoItem>> GetListadoAsync();
}