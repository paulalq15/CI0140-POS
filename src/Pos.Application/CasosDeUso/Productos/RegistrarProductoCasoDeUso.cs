using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;

namespace Pos.Application.CasosDeUso.Productos;

/// <summary>
/// CU-1 (inserción): registra un producto en el catálogo (US-01, RF-01).
/// </summary>
public sealed class RegistrarProductoCasoDeUso
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public RegistrarProductoCasoDeUso(IProductoRepository productoRepository, ICategoriaRepository categoriaRepository)
    {
        _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
        _categoriaRepository = categoriaRepository ?? throw new ArgumentNullException(nameof(categoriaRepository));
    }

    /// <summary>Categorías que se pueden asignar a un producto nuevo.</summary>
    public IReadOnlyList<Categoria> ObtenerCategoriasDisponibles() => _categoriaRepository.ObtenerActivas();

    /// <summary>Valida los datos y registra el producto. Devuelve el Id asignado.</summary>
    /// <exception cref="ValidacionException">Si algún dato no es válido.</exception>
    public int Ejecutar(RegistrarProductoComando comando)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var producto = new Producto
        {
            Nombre = (comando.Nombre ?? string.Empty).Trim(),
            Descripcion = (comando.Descripcion ?? string.Empty).Trim(),
            Precio = comando.Precio,
            CategoriaId = comando.CategoriaId,
            Activo = true,
        };

        producto.Validar();
        ValidarCategoria(producto.CategoriaId);

        if (_productoRepository.ExisteNombre(producto.Nombre))
        {
            throw new ValidacionException(
                nameof(Producto.Nombre),
                $"Ya existe un producto con el nombre \"{producto.Nombre}\". Use un nombre diferente.");
        }

        return _productoRepository.Insertar(producto);
    }

    private void ValidarCategoria(int categoriaId)
    {
        var categoria = _categoriaRepository.ObtenerPorId(categoriaId)
            ?? throw new ValidacionException(
                nameof(Producto.CategoriaId),
                "La categoría seleccionada no existe. Elija una categoría de la lista.");

        if (!categoria.Activa)
        {
            throw new ValidacionException(
                nameof(Producto.CategoriaId),
                "La categoría seleccionada está deshabilitada. Elija una categoría activa.");
        }
    }
}
