using Microsoft.Data.Sqlite;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class SqliteProductoRepository : IProductoRepository
{
    private const string Columnas = "Id, Nombre, Descripcion, Precio, CategoriaId, Activo";

    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteProductoRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public int Insertar(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            INSERT INTO Producto (Nombre, Descripcion, Precio, CategoriaId, Activo)
            VALUES ($nombre, $descripcion, $precio, $categoriaId, $activo)
            RETURNING Id;
            """;
        comando.Parameters.AddWithValue("$nombre", producto.Nombre);
        comando.Parameters.AddWithValue("$descripcion", producto.Descripcion);
        comando.Parameters.AddWithValue("$precio", producto.Precio);
        comando.Parameters.AddWithValue("$categoriaId", producto.CategoriaId);
        comando.Parameters.AddWithValue("$activo", producto.Activo);

        producto.Id = Convert.ToInt32(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
        return producto.Id;
    }

    public Producto? ObtenerPorId(int id)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = $"SELECT {Columnas} FROM Producto WHERE Id = $id;";
        comando.Parameters.AddWithValue("$id", id);

        using var lector = comando.ExecuteReader();
        return lector.Read() ? Mapear(lector) : null;
    }

    public IReadOnlyList<Producto> ObtenerActivos()
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = $"SELECT {Columnas} FROM Producto WHERE Activo = 1 ORDER BY Nombre;";

        using var lector = comando.ExecuteReader();
        var productos = new List<Producto>();
        while (lector.Read())
        {
            productos.Add(Mapear(lector));
        }

        return productos;
    }

    public bool ExisteNombre(string nombre)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT EXISTS (SELECT 1 FROM Producto WHERE lower(Nombre) = lower($nombre));";
        comando.Parameters.AddWithValue("$nombre", nombre ?? string.Empty);

        return Convert.ToInt64(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) == 1;
    }

    private static Producto Mapear(SqliteDataReader lector) => new()
    {
        Id = lector.GetInt32(0),
        Nombre = lector.GetString(1),
        Descripcion = lector.GetString(2),
        Precio = lector.GetDecimal(3),
        CategoriaId = lector.GetInt32(4),
        Activo = lector.GetBoolean(5),
    };
}
