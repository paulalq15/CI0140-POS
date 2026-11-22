using Microsoft.Data.Sqlite;

namespace Pos.Infrastructure.Persistence;

/// <summary>Crea conexiones abiertas a la base de datos SQLite con llaves foráneas activadas.</summary>
public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    /// <summary>
    /// Construye la fábrica a partir de la variable de entorno POS_DB_PATH
    /// (o "pos.db" por defecto), sin guardar rutas ni secretos en el código (RNF-04).
    /// </summary>
    public static SqliteConnectionFactory DesdeVariableDeEntorno()
    {
        var ruta = Environment.GetEnvironmentVariable("POS_DB_PATH");
        if (string.IsNullOrWhiteSpace(ruta))
        {
            ruta = "pos.db";
        }

        return new SqliteConnectionFactory($"Data Source={ruta}");
    }

    public SqliteConnection CrearConexionAbierta()
    {
        var conexion = new SqliteConnection(_connectionString);
        conexion.Open();

        using var pragma = conexion.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return conexion;
    }
}
