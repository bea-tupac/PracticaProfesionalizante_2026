using System.Collections.Generic;

namespace Instituto.AD;

public class DBParameters
{
    private readonly List<DBParameter> _parametros = new();

    public DBParameters Agregar(string nombre, object valor)
    {
        _parametros.Add(new DBParameter(nombre, valor));
        return this;
    }

    public List<DBParameter> RetornarParametros()
    {
        return _parametros;
    }
}