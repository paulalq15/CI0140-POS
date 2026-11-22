namespace Pos.Domain.Entities;

/// <summary>
/// Producto que se vende en la cafetería. En esta iteración todos los productos
/// se manejan por unidades (RF-12); las recetas quedan para iteraciones futuras.
/// </summary>
public class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int CategoriaId { get; set; }

    public bool Activo { get; set; } = true;
}
