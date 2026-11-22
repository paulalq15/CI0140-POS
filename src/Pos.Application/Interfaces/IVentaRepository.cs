using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

/// <summary>
/// Persistencia de ventas (CU-2).
/// </summary>
public interface IVentaRepository
{
    /// <summary>
    /// Guarda la venta con sus detalles y descuenta el inventario de la sucursal
    /// en UNA sola transacción: si cualquier paso falla, no debe quedar ningún
    /// cambio (RNF-07, RNF-08, RNF-09). Devuelve el Id de la venta creada.
    /// </summary>
    int RegistrarVenta(Venta venta);
}
