using System;

namespace Instituto.AD;

public class DBParameter
{
    public string Nombre { get; set; }
    public object Valor { get; set; }

    public DBParameter(string nombre, object valor)
    {
        Nombre = nombre;
        Valor = valor;
    }
}