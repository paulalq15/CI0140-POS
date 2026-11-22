using Microsoft.Data.Sqlite;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;
using System.Text;

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

    /// <summary>
    /// Indica si ya existe un producto con el mismo nombre, sin distinguir mayúsculas
    /// ni la forma en que se escribió la tilde. Se compara en C# porque lower() de
    /// SQLite solo convierte letras ASCII (por ejemplo, no convierte "É" en "é").
    /// </summary>
    public bool ExisteNombre(string nombre)
    {
        var buscado = (nombre ?? string.Empty).Trim().Normalize(NormalizationForm.FormC);

        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT Nombre FROM Producto;";

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            var existente = lector.GetString(0).Normalize(NormalizationForm.FormC);
            if (string.Equals(existente, buscado, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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
