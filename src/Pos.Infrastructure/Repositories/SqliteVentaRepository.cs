using System.Globalization;
using Microsoft.Data.Sqlite;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class SqliteVentaRepository(SqliteConnectionFactory connectionFactory) : IVentaRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    public int RegistrarVenta(Venta venta)
    {
        ArgumentNullException.ThrowIfNull(venta);
        using var conexion = _connectionFactory.CrearConexionAbierta();
        // Al desecharse sin Commit, SQLite revierte todos los cambios de la transacción.
        using var transaccion = conexion.BeginTransaction();

        var ventaId = InsertarVenta(conexion, transaccion, venta);
        foreach (var detalle in venta.Detalles)
        {
            InsertarDetalle(conexion, transaccion, ventaId, detalle);
            DescontarInventario(conexion, transaccion, venta.SucursalId, detalle);
        }

        transaccion.Commit();
        return ventaId;
    }

    private static int InsertarVenta(SqliteConnection conexion, SqliteTransaction transaccion, Venta venta)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO Venta (SucursalId, CajaId, Fecha, Total, MontoRecibido, Vuelto, MetodoPago)
            SELECT SucursalId, Id, $fecha, $total, $montoRecibido, $vuelto, $metodoPago
            FROM Caja
            WHERE Id = $cajaId AND SucursalId = $sucursalId
            RETURNING Id;
            """;
        comando.Parameters.AddWithValue("$sucursalId", venta.SucursalId);
        comando.Parameters.AddWithValue("$cajaId", venta.CajaId);
        comando.Parameters.AddWithValue("$fecha", venta.Fecha.ToString("O", CultureInfo.InvariantCulture));
        comando.Parameters.AddWithValue("$total", venta.Total);
        comando.Parameters.AddWithValue("$montoRecibido", venta.MontoRecibido);
        comando.Parameters.AddWithValue("$vuelto", venta.Vuelto);
        comando.Parameters.AddWithValue("$metodoPago", (int)venta.MetodoPago);

        var id = comando.ExecuteScalar() ?? throw new ValidacionException(
            nameof(Venta.CajaId), "No se pudo registrar la venta en la caja indicada. Vuelva a seleccionar una caja de la sucursal.");
        return Convert.ToInt32(id, CultureInfo.InvariantCulture);
    }

    private static void InsertarDetalle(SqliteConnection conexion, SqliteTransaction transaccion, int ventaId, DetalleVenta detalle)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            INSERT INTO DetalleVenta (VentaId, ProductoId, Cantidad, PrecioUnitario)
            VALUES ($ventaId, $productoId, $cantidad, $precioUnitario);
            """;
        comando.Parameters.AddWithValue("$ventaId", ventaId);
        comando.Parameters.AddWithValue("$productoId", detalle.ProductoId);
        comando.Parameters.AddWithValue("$cantidad", detalle.Cantidad);
        comando.Parameters.AddWithValue("$precioUnitario", detalle.PrecioUnitario);
        comando.ExecuteNonQuery();
    }

    private static void DescontarInventario(SqliteConnection conexion, SqliteTransaction transaccion, int sucursalId, DetalleVenta detalle)
    {
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = """
            UPDATE Inventario
            SET Cantidad = Cantidad - $cantidad
            WHERE SucursalId = $sucursalId AND ProductoId = $productoId AND Cantidad >= $cantidad;
            """;
        comando.Parameters.AddWithValue("$sucursalId", sucursalId);
        comando.Parameters.AddWithValue("$productoId", detalle.ProductoId);
        comando.Parameters.AddWithValue("$cantidad", detalle.Cantidad);

        if (comando.ExecuteNonQuery() != 1)
        {
            throw new ValidacionException(nameof(DetalleVenta.Cantidad),
                $"Ya no hay suficiente inventario del producto {detalle.ProductoId}. Consulte el inventario y ajuste la cantidad.");
        }
    }
}
