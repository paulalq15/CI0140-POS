using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

public interface ISucursalRepository
{
    bool Existe(int id);

    IReadOnlyList<Sucursal> ObtenerTodas();
}
