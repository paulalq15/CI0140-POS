using Pos.Domain.Exceptions;

namespace Pos.Domain.Entities;

/// <summary>
/// Producto que se vende en la cafetería. En esta iteración todos los productos
/// se manejan por unidades (RF-12); las recetas quedan para iteraciones futuras.
/// </summary>
public class Producto
{
    public const int LongitudMaximaNombre = 100;
    public const int LongitudMaximaDescripcion = 250;

    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int CategoriaId { get; set; }

    public bool Activo { get; set; } = true;

    /// <summary>
    /// Verifica las reglas propias del producto (US-01, RF-01). Las reglas que
    /// dependen de otros datos (categoría existente, nombre repetido) se validan
    /// en el caso de uso.
    /// </summary>
    /// <exception cref="ValidacionException">Si algún dato no cumple las reglas.</exception>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            throw new ValidacionException(nameof(Nombre), "El nombre del producto es obligatorio. Escriba un nombre.");
        }

        if (Nombre.Length > LongitudMaximaNombre)
        {
            throw new ValidacionException(
                nameof(Nombre),
                $"El nombre del producto no puede tener más de {LongitudMaximaNombre} caracteres. Escriba un nombre más corto.");
        }

        if (Descripcion.Length > LongitudMaximaDescripcion)
        {
            throw new ValidacionException(
                nameof(Descripcion),
                $"La descripción no puede tener más de {LongitudMaximaDescripcion} caracteres. Escriba una descripción más corta.");
        }

        if (Precio <= 0)
        {
            throw new ValidacionException(nameof(Precio), "El precio debe ser mayor que cero. Digite un precio válido.");
        }

        if (CategoriaId <= 0)
        {
            throw new ValidacionException(nameof(CategoriaId), "Debe seleccionar una categoría. Elija una categoría de la lista.");
        }
    }
}
