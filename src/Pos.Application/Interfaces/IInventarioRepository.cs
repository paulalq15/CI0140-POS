using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

/// <summary>Consulta de inventario por sucursal (RF-13).</summary>
public interface IInventarioRepository
{
    IReadOnlyList<InventarioItem> ObtenerPorSucursal(int sucursalId);

    int ObtenerCantidad(int sucursalId, int productoId);
}
