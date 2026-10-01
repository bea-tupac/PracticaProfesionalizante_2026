using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class CarreraRepository : ICarreraRepository
{
    private readonly InstitutoDbContext _context;

    public CarreraRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static Carrera MapReaderToCarrera(SqlDataReader reader)
    {
        return new Carrera
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            DuracionAnios = reader.GetInt32(reader.GetOrdinal("DuracionAnios")),
            Turno = reader.IsDBNull(reader.GetOrdinal("Turno")) ? null : reader.GetString(reader.GetOrdinal("Turno")),
            Modalidad = reader.IsDBNull(reader.GetOrdinal("Modalidad")) ? null : reader.GetString(reader.GetOrdinal("Modalidad")),
            Horario = reader.IsDBNull(reader.GetOrdinal("Horario")) ? null : reader.GetString(reader.GetOrdinal("Horario")),
            Estado = reader.IsDBNull(reader.GetOrdinal("Estado")) ? null : reader.GetString(reader.GetOrdinal("Estado")),
            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
        };
    }

    public async Task<List<Carrera>> GetAllAsync()
    {
        return await _context.Carreras
            .FromSqlRaw("EXEC sp_Carreras_GetAll")
            .ToListAsync();
    }

    public async Task<Carrera?> GetByIdAsync(int id)
    {
        return await _context.Carreras
            .FromSqlRaw("EXEC sp_Carreras_GetById @Id", new SqlParameter("@Id", id))
            .FirstOrDefaultAsync();
    }

    public async Task<Carrera> CreateAsync(Carrera entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Carreras_Create @Nombre, @DuracionAnios, @Turno, @Modalidad, @Horario, @Estado";
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@DuracionAnios", entity.DuracionAnios));
        command.Parameters.Add(new SqlParameter("@Turno", entity.Turno ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Modalidad", entity.Modalidad ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Horario", entity.Horario ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Estado", entity.Estado ?? "Activa"));

        var result = await command.ExecuteScalarAsync();
        entity.Id = Convert.ToInt32(result);
        return entity;
    }

    public async Task UpdateAsync(int id, Carrera entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Carreras_Update @Id, @Nombre, @DuracionAnios, @Turno, @Modalidad, @Horario, @Estado";
        command.Parameters.Add(new SqlParameter("@Id", id));
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@DuracionAnios", entity.DuracionAnios));
        command.Parameters.Add(new SqlParameter("@Turno", entity.Turno ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Modalidad", entity.Modalidad ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Horario", entity.Horario ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Estado", entity.Estado ?? (object)DBNull.Value));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Carreras_Delete @Id";
        command.Parameters.Add(new SqlParameter("@Id", id));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Carreras_Exists @Id", new SqlParameter("@Id", id))
            .FirstOrDefaultAsync() == 1;
    }
}