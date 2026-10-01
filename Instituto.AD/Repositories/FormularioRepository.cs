using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class FormularioRepository : IFormularioRepository
{
    private readonly InstitutoDbContext _context;

    public FormularioRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static Formulario MapReaderToFormulario(SqlDataReader reader)
    {
        return new Formulario
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
            Estado = reader.GetString(reader.GetOrdinal("Estado")),
            FechaApertura = reader.GetDateTime(reader.GetOrdinal("FechaApertura")),
            FechaCierre = reader.GetDateTime(reader.GetOrdinal("FechaCierre")),
            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
            FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
        };
    }

    public async Task<List<Formulario>> GetAllAsync()
    {
        return await _context.Formularios
            .FromSqlRaw("EXEC sp_Formularios_GetAll")
            .ToListAsync();
    }

    public async Task<Formulario?> GetByIdAsync(int id)
    {
        return await _context.Formularios
            .FromSqlRaw("EXEC sp_Formularios_GetById @Id", new SqlParameter("@Id", id))
            .FirstOrDefaultAsync();
    }

    public async Task<Formulario> CreateAsync(Formulario entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Formularios_Create @Nombre, @Estado, @FechaApertura, @FechaCierre, @Descripcion";
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@Estado", entity.Estado));
        command.Parameters.Add(new SqlParameter("@FechaApertura", entity.FechaApertura));
        command.Parameters.Add(new SqlParameter("@FechaCierre", entity.FechaCierre));
        command.Parameters.Add(new SqlParameter("@Descripcion", entity.Descripcion ?? (object)DBNull.Value));

        var result = await command.ExecuteScalarAsync();
        entity.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return entity;
    }

    public async Task UpdateAsync(int id, Formulario entity)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Formularios_Update @Id, @Nombre, @Estado, @FechaApertura, @FechaCierre, @Descripcion";
        command.Parameters.Add(new SqlParameter("@Id", id));
        command.Parameters.Add(new SqlParameter("@Nombre", entity.Nombre));
        command.Parameters.Add(new SqlParameter("@Estado", entity.Estado));
        command.Parameters.Add(new SqlParameter("@FechaApertura", entity.FechaApertura));
        command.Parameters.Add(new SqlParameter("@FechaCierre", entity.FechaCierre));
        command.Parameters.Add(new SqlParameter("@Descripcion", entity.Descripcion ?? (object)DBNull.Value));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sp_Formularios_Delete @Id";
        command.Parameters.Add(new SqlParameter("@Id", id));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Database
            .SqlQueryRaw<int>("EXEC sp_Formularios_Exists @Id", new SqlParameter("@Id", id))
            .FirstOrDefaultAsync() == 1;
    }
}