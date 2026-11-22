namespace Pos.Domain.Entities;

/// <summary>Cantidad disponible de un producto en una sucursal (RF-09, RF-13).</summary>
public class InventarioItem
{
    public int SucursalId { get; set; }

    public int ProductoId { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public int Cantidad { get; set; }
}
