using Instituto.AD.Interfaces;
using Instituto.AD.Models;
using Instituto.AD.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Instituto.AD.Repositories;

public class ListadoRepository : IListadoRepository
{
    private readonly InstitutoDbContext _context;

    public ListadoRepository(InstitutoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private static ListadoItem MapReaderToListadoItem(SqlDataReader reader)
    {
        return new ListadoItem
        {
            AlumnoId = reader.GetInt32(reader.GetOrdinal("AlumnoId")),
            NombreCompleto = reader.GetString(reader.GetOrdinal("NombreCompleto")),
            DNI = reader.GetInt32(reader.GetOrdinal("DNI")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Carrera = reader.GetString(reader.GetOrdinal("Carrera")),
            Turno = reader.GetString(reader.GetOrdinal("Turno")),
            Edad = reader.GetInt32(reader.GetOrdinal("Edad"))
        };
    }

    public async Task<List<ListadoItem>> GetListadoAsync()
    {
        return await _context.Database
            .SqlQueryRaw<ListadoItem>("EXEC sp_Listado_GetAll")
            .ToListAsync();
    }
}