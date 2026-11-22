namespace Pos.Domain.Entities;

/// <summary>
/// Venta completada (RF-23, RF-45). La usa el CU-2: Completar una venta en efectivo.
/// </summary>
public class Venta
{
    public int Id { get; set; }

    public int SucursalId { get; set; }

    public int CajaId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public decimal MontoRecibido { get; set; }

    public decimal Vuelto { get; set; }

    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    public List<DetalleVenta> Detalles { get; } = [];
}
