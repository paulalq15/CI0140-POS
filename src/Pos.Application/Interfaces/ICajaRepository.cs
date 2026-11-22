using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

public interface ICajaRepository
{
    bool ExisteEnSucursal(int sucursalId, int cajaId);

    IReadOnlyList<Caja> ObtenerPorSucursal(int sucursalId);
}
