using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class ProfesorRepository : IProfesorRepository
{
    private readonly InstitutoDbContext _context;

    public ProfesorRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static Profesor MapReaderToProfesor(SqlDataReader reader)
    {
        return new Profesor
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString(reader.GetOrdinal("Telefono")),
            Especialidad = reader.IsDBNull(reader.GetOrdinal("Especialidad")) ? null : reader.GetString(reader.GetOrdinal("Especialidad"))
        };
    }

    public async Task<List<Profesor>> GetAllAsync()
    {
        return await _context.Profesores
            .FromSqlRaw("EXEC sp_Profesores_GetAll")
            .ToListAsync();
    }

    public async Task<Profesor?> GetByIdAsync(int id)
    {
        return await _context.Profesores
            .FromSqlRaw("EXEC sp_Profesores_GetById @Id", new SqlParameter("@Id", id))
            .FirstOrDefaultAsync();
    }

    public async Task<Profesor> CreateAsync(Profesor entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Profesores_Create @Nombre, @Apellido, @Email, @Telefono, @Especialidad";
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@Apellido", entity.Apellido));
        command.Parameters.Add(new SqlParameter("@Email", entity.Email.ToLower().Trim()));
        command.Parameters.Add(new SqlParameter("@Telefono", entity.Telefono ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Especialidad", entity.Especialidad ?? (object)DBNull.Value));

        var result = await command.ExecuteScalarAsync();
        entity.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return entity;
    }

    public async Task UpdateAsync(int id, Profesor entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Profesores_Update @Id, @Nombre, @Apellido, @Email, @Telefono, @Especialidad";
        command.Parameters.Add(new SqlParameter("@Id", id));
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@Apellido", entity.Apellido));
        command.Parameters.Add(new SqlParameter("@Email", entity.Email.ToLower().Trim()));
        command.Parameters.Add(new SqlParameter("@Telefono", entity.Telefono ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Especialidad", entity.Especialidad ?? (object)DBNull.Value));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Profesores_Delete @Id";
        command.Parameters.Add(new SqlParameter("@Id", id));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Profesores_Exists @Id", new SqlParameter("@Id", id))
            .FirstOrDefaultAsync() == 1;
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Profesores_ExistsByEmail @Email", new SqlParameter("@Email", email.ToLower().Trim()))
            .FirstOrDefaultAsync() == 1;
    }
}