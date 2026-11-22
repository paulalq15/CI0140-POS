using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

public interface ICategoriaRepository
{
    Categoria? ObtenerPorId(int id);

    IReadOnlyList<Categoria> ObtenerActivas();
}
