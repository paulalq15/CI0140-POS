namespace Pos.Domain.Entities;

/// <summary>Línea de una venta: producto, cantidad y precio al momento de vender (RF-19, RF-20).</summary>
public class DetalleVenta
{
    public int Id { get; set; }

    public int VentaId { get; set; }

    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => PrecioUnitario * Cantidad;
}
