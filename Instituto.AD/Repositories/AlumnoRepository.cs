using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class AlumnoRepository : IAlumnoRepository
{
    private readonly InstitutoDbContext _context;

    public AlumnoRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static Alumno MapReaderToAlumno(SqlDataReader reader)
    {
        return new Alumno
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            DNI = reader.GetInt32(reader.GetOrdinal("DNI")),
            FechaNacimiento = reader.GetDateTime(reader.GetOrdinal("FechaNacimiento")),
            Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString(reader.GetOrdinal("Direccion")),
            Nacionalidad = reader.IsDBNull(reader.GetOrdinal("Nacionalidad")) ? null : reader.GetString(reader.GetOrdinal("Nacionalidad")),
            FechaInscripcion = reader.IsDBNull(reader.GetOrdinal("FechaInscripcion")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaInscripcion")),
            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString(reader.GetOrdinal("Telefono")),
            TituloSecundario = reader.IsDBNull(reader.GetOrdinal("TituloSecundario")) ? null : reader.GetString(reader.GetOrdinal("TituloSecundario")),
            Turno = reader.IsDBNull(reader.GetOrdinal("Turno")) ? null : reader.GetString(reader.GetOrdinal("Turno")),
            CarreraId = reader.GetInt32(reader.GetOrdinal("CarreraId"))
        };
    }

    public async Task<List<Alumno>> GetAllAsync()
    {
        return await _context.Alumnos
            .FromSqlRaw("EXEC sp_Alumnos_GetAll")
            .ToListAsync();
    }

    public async Task<Alumno?> GetByIdAsync(int id)
    {
        return await _context.Alumnos
            .FromSqlRaw("EXEC sp_Alumnos_GetById @Id", new SqlParameter("@Id", id))
            .FirstOrDefaultAsync();
    }

    public async Task<Alumno> CreateAsync(Alumno entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Alumnos_Create @Nombre, @Apellido, @Email, @DNI, @FechaNacimiento, @Direccion, @Nacionalidad, @FechaInscripcion, @Telefono, @TituloSecundario, @Turno, @CarreraId";
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@Apellido", entity.Apellido));
        command.Parameters.Add(new SqlParameter("@Email", entity.Email));
        command.Parameters.Add(new SqlParameter("@DNI", entity.DNI));
        command.Parameters.Add(new SqlParameter("@FechaNacimiento", entity.FechaNacimiento));
        command.Parameters.Add(new SqlParameter("@Direccion", entity.Direccion ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Nacionalidad", entity.Nacionalidad ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@FechaInscripcion", entity.FechaInscripcion ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Telefono", entity.Telefono ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@TituloSecundario", entity.TituloSecundario ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Turno", entity.Turno ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@CarreraId", entity.CarreraId));

        var result = await command.ExecuteScalarAsync();
        entity.Id = Convert.ToInt32(result);
        return entity;
    }

    public async Task UpdateAsync(int id, Alumno entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Alumnos_Update @Id, @Nombre, @Apellido, @Email, @DNI, @FechaNacimiento, @Direccion, @Nacionalidad, @Telefono, @TituloSecundario, @Turno, @CarreraId";
        command.Parameters.Add(new SqlParameter("@Id", id));
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@Apellido", entity.Apellido));
        command.Parameters.Add(new SqlParameter("@Email", entity.Email));
        command.Parameters.Add(new SqlParameter("@DNI", entity.DNI));
        command.Parameters.Add(new SqlParameter("@FechaNacimiento", entity.FechaNacimiento));
        command.Parameters.Add(new SqlParameter("@Direccion", entity.Direccion ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Nacionalidad", entity.Nacionalidad ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Telefono", entity.Telefono ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@TituloSecundario", entity.TituloSecundario ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Turno", entity.Turno ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@CarreraId", entity.CarreraId));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Alumnos_Delete @Id";
        command.Parameters.Add(new SqlParameter("@Id", id));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Alumnos_Exists @Id", new SqlParameter("@Id", id))
            .FirstOrDefaultAsync() == 1;
    }

    public async Task<bool> ExistsByDNIAsync(int dni)
    {
        return await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Alumnos_ExistsByDNI @DNI", new SqlParameter("@DNI", dni))
            .FirstOrDefaultAsync() == 1;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Alumnos_ExistsByEmail @Email", new SqlParameter("@Email", email.ToLower().Trim()))
            .FirstOrDefaultAsync() == 1;
    }

    public async Task<bool> ExistsByCarreraIdAsync(int carreraId)
    {
        return await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Alumnos_ExistsByCarreraId @CarreraId", new SqlParameter("@CarreraId", carreraId))
            .FirstOrDefaultAsync() == 1;
    }
}