using Microsoft.Data.Sqlite;
using Pos.Infrastructure.Persistence;

namespace Pos.Tests.Infrastructure;

/// <summary>
/// Crea una base de datos SQLite temporal, ya inicializada con los datos de ejemplo,
/// y la elimina al terminar la prueba.
/// </summary>
internal sealed class BaseDeDatosDePrueba : IDisposable
{
    private readonly string _ruta = Path.Combine(Path.GetTempPath(), $"pos_test_{Guid.NewGuid():N}.db");

    public BaseDeDatosDePrueba()
    {
        Factory = new SqliteConnectionFactory($"Data Source={_ruta}");
        new DatabaseInitializer(Factory).Inicializar();
    }

    public SqliteConnectionFactory Factory { get; }

    /// <summary>Ejecuta una sentencia SQL directa para preparar escenarios de prueba.</summary>
    public void Ejecutar(string sql)
    {
        using var conexion = Factory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.ExecuteNonQuery();
    }

    /// <summary>Ejecuta una consulta que devuelve un número para verificar el estado de la BD.</summary>
    public long ConsultarNumero(string sql)
    {
        using var conexion = Factory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        return Convert.ToInt64(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_ruta))
        {
            File.Delete(_ruta);
        }
    }
}
