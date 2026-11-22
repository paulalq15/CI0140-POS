namespace Pos.Application.Interfaces;

public interface ICajaRepository
{
    bool ExisteEnSucursal(int sucursalId, int cajaId);
}
