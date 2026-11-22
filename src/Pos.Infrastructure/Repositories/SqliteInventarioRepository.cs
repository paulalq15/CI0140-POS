using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class SqliteInventarioRepository : IInventarioRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteInventarioRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Devuelve todos los productos activos con su cantidad en la sucursal.
    /// Un producto sin registro de inventario aparece con cantidad 0.
    /// </summary>
    public IReadOnlyList<InventarioItem> ObtenerPorSucursal(int sucursalId)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT p.Id, p.Nombre, COALESCE(i.Cantidad, 0)
            FROM Producto p
            LEFT JOIN Inventario i ON i.ProductoId = p.Id AND i.SucursalId = $sucursalId
            WHERE p.Activo = 1
            ORDER BY p.Nombre;
            """;
        comando.Parameters.AddWithValue("$sucursalId", sucursalId);

        using var lector = comando.ExecuteReader();
        var items = new List<InventarioItem>();
        while (lector.Read())
        {
            items.Add(new InventarioItem
            {
                SucursalId = sucursalId,
                ProductoId = lector.GetInt32(0),
                NombreProducto = lector.GetString(1),
                Cantidad = lector.GetInt32(2),
            });
        }

        return items;
    }

    public int ObtenerCantidad(int sucursalId, int productoId)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT COALESCE(
                (SELECT Cantidad FROM Inventario WHERE SucursalId = $sucursalId AND ProductoId = $productoId),
                0);
            """;
        comando.Parameters.AddWithValue("$sucursalId", sucursalId);
        comando.Parameters.AddWithValue("$productoId", productoId);

        return Convert.ToInt32(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
    }
}
