using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Application.CasosDeUso.Productos;

/// <summary>Grupo del catálogo: una categoría con sus productos activos.</summary>
public sealed record CategoriaCatalogo(string NombreCategoria, IReadOnlyList<Producto> Productos);

/// <summary>
/// CU-1 (consulta): devuelve los productos activos organizados por categoría (US-07, RF-05).
/// </summary>
public sealed class ConsultarCatalogoCasoDeUso
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public ConsultarCatalogoCasoDeUso(IProductoRepository productoRepository, ICategoriaRepository categoriaRepository)
    {
        _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
        _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
    }

    /// <summary>
    /// Solo incluye productos activos de categorías activas. Las categorías sin
    /// productos no se muestran.
    /// </summary>
    public IReadOnlyList<CategoriaCatalogo> Ejecutar()
    {
        var productos = _productoRepository.ObtenerActivos();

        return _categoriaRepository.ObtenerActivas()
            .Select(categoria => new CategoriaCatalogo(
                categoria.Nombre,
                productos
                    .Where(producto => producto.CategoriaId == categoria.Id)
                    .OrderBy(producto => producto.Nombre, StringComparer.CurrentCulture)
                    .ToList()))
            .Where(grupo => grupo.Productos.Count > 0)
            .OrderBy(grupo => grupo.NombreCategoria, StringComparer.CurrentCulture)
            .ToList();
    }
}
