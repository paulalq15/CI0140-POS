using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

/// <summary>Acceso a datos de productos (CU-1). La implementación vive en Infrastructure (RNF-13).</summary>
public interface IProductoRepository
{
    int Insertar(Producto producto);

    Producto? ObtenerPorId(int id);

    IReadOnlyList<Producto> ObtenerActivos();

    bool ExisteNombre(string nombre);
}
