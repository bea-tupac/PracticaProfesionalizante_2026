using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Instituto.AD;

namespace Instituto.AD;

public class AccesoDB : IDisposable
{
    private readonly string _connectionString;
    private SqlConnection? _conexion;
    private bool _isConnectionOpen;

    public AccesoDB(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _isConnectionOpen = false;
    }

    ~AccesoDB()
    {
        Dispose();
    }

    public void Dispose()
    {
        try
        {
            if (_conexion != null)
            {
                if (_isConnectionOpen)
                {
                    _conexion.Close();
                }
                _conexion.Dispose();
                _conexion = null;
                _isConnectionOpen = false;
            }
        }
        catch
        {
        }
    }

    private void AbrirConexion()
    {
        if (!_isConnectionOpen)
        {
            _conexion = new SqlConnection(_connectionString);
            _conexion.Open();
            _isConnectionOpen = true;
        }
    }

    public SqlDataReader GetData(string selectCommand)
    {
        AbrirConexion();
        var command = new SqlCommand(selectCommand, _conexion);
        return command.ExecuteReader(CommandBehavior.CloseConnection);
    }

    public SqlDataReader GetData(string selectCommand, DBParameters parametros)
    {
        AbrirConexion();
        var command = new SqlCommand(selectCommand, _conexion);

        foreach (var parametro in parametros.RetornarParametros())
        {
            command.Parameters.AddWithValue(parametro.Nombre, parametro.Valor);
        }

        return command.ExecuteReader(CommandBehavior.CloseConnection);
    }

    public int Execute(string executeCommand)
    {
        AbrirConexion();
        using var command = new SqlCommand(executeCommand, _conexion);
        return command.ExecuteNonQuery();
    }

    public int Execute(string executeCommand, DBParameters parametros)
    {
        AbrirConexion();
        using var command = new SqlCommand(executeCommand, _conexion);

        foreach (var parametro in parametros.RetornarParametros())
        {
            command.Parameters.AddWithValue(parametro.Nombre, parametro.Valor);
        }

        return command.ExecuteNonQuery();
    }

    public object ExecuteScalar(string selectCommand)
    {
        AbrirConexion();
        using var command = new SqlCommand(selectCommand, _conexion);
        return command.ExecuteScalar();
    }

    public object ExecuteScalar(string selectCommand, DBParameters parametros)
    {
        AbrirConexion();
        using var command = new SqlCommand(selectCommand, _conexion);

        foreach (var parametro in parametros.RetornarParametros())
        {
            command.Parameters.AddWithValue(parametro.Nombre, parametro.Valor);
        }

        return command.ExecuteScalar();
    }
}