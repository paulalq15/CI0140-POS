using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;

namespace Pos.Application.CasosDeUso.Inventario;

/// <summary>
/// CU-1 (consulta): muestra la cantidad disponible de cada producto en una sucursal (US-14, RF-13).
/// </summary>
public sealed class ConsultarInventarioCasoDeUso
{
    private readonly IInventarioRepository _inventarioRepository;
    private readonly ISucursalRepository _sucursalRepository;

    public ConsultarInventarioCasoDeUso(IInventarioRepository inventarioRepository, ISucursalRepository sucursalRepository)
    {
        _inventarioRepository = inventarioRepository ?? throw new ArgumentNullException(nameof(inventarioRepository));
        _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
    }

    public IReadOnlyList<Sucursal> ObtenerSucursales() => _sucursalRepository.ObtenerTodas();

    /// <exception cref="ValidacionException">Si la sucursal no existe.</exception>
    public IReadOnlyList<InventarioItem> Ejecutar(int sucursalId)
    {
        if (!_sucursalRepository.Existe(sucursalId))
        {
            throw new ValidacionException(
                "SucursalId",
                "La sucursal indicada no existe. Elija una sucursal de la lista.");
        }

        return _inventarioRepository.ObtenerPorSucursal(sucursalId);
    }
}
