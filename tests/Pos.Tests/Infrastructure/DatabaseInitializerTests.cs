using Microsoft.Data.Sqlite;
using Pos.Infrastructure.Persistence;

namespace Pos.Tests.Infrastructure;

public sealed class DatabaseInitializerTests : IDisposable
{
    private readonly string _rutaBaseDatos;
    private readonly SqliteConnectionFactory _connectionFactory;

    public DatabaseInitializerTests()
    {
        _rutaBaseDatos = Path.Combine(Path.GetTempPath(), $"pos_test_{Guid.NewGuid():N}.db");
        _connectionFactory = new SqliteConnectionFactory($"Data Source={_rutaBaseDatos}");
    }

    [Fact]
    public void Inicializar_BaseVacia_CreaTodasLasTablas()
    {
        new DatabaseInitializer(_connectionFactory).Inicializar();

        string[] esperadas = ["Caja", "Categoria", "DetalleVenta", "Inventario", "Producto", "Sucursal", "Venta"];
        Assert.Equal(esperadas, ObtenerTablas());
    }

    [Fact]
    public void Inicializar_EjecutadoDosVeces_NoDuplicaDatosIniciales()
    {
        var inicializador = new DatabaseInitializer(_connectionFactory);

        inicializador.Inicializar();
        inicializador.Inicializar();

        Assert.Equal(4, Contar("Producto"));
        Assert.Equal(2, Contar("Categoria"));
    }

    [Fact]
    public void Inventario_CantidadNegativa_EsRechazadaPorLaBaseDeDatos()
    {
        new DatabaseInitializer(_connectionFactory).Inicializar();
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "UPDATE Inventario SET Cantidad = -1 WHERE ProductoId = 1;";

        Assert.Throws<SqliteException>(() => comando.ExecuteNonQuery());
    }

    [Fact]
    public void SqliteConnectionFactory_CadenaVacia_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SqliteConnectionFactory(" "));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_rutaBaseDatos))
        {
            File.Delete(_rutaBaseDatos);
        }
    }

    private List<string> ObtenerTablas()
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
        using var lector = comando.ExecuteReader();
        var tablas = new List<string>();
        while (lector.Read())
        {
            tablas.Add(lector.GetString(0));
        }

        return tablas;
    }

    private long Contar(string tabla)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = $"SELECT COUNT(*) FROM {tabla};";
        return (long)comando.ExecuteScalar()!;
    }
}
