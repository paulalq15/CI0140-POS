using Pos.Application.CasosDeUso.Productos;
using Pos.Domain.Entities;
using Pos.Tests.Application.Fakes;

namespace Pos.Tests.Application;

public class ConsultarCatalogoCasoDeUsoTests
{
    private readonly FakeProductoRepository _productos = new();
    private readonly FakeCategoriaRepository _categorias = new();
    private readonly ConsultarCatalogoCasoDeUso _casoDeUso;

    public ConsultarCatalogoCasoDeUsoTests()
    {
        _casoDeUso = new ConsultarCatalogoCasoDeUso(_productos, _categorias);
    }

    // CP-04: catálogo organizado por categoría
    [Fact]
    public void Ejecutar_ProductosActivos_AgrupaPorCategoriaYOrdenaPorNombre()
    {
        _categorias.Categorias.Add(new Categoria { Id = 1, Nombre = "Repostería", Activa = true });
        _categorias.Categorias.Add(new Categoria { Id = 2, Nombre = "Bebidas", Activa = true });
        AgregarProducto("Croissant", categoriaId: 1);
        AgregarProducto("Brownie", categoriaId: 1);
        AgregarProducto("Agua", categoriaId: 2);

        var catalogo = _casoDeUso.Ejecutar();

        Assert.Equal(new[] { "Bebidas", "Repostería" }, catalogo.Select(g => g.NombreCategoria));
        Assert.Equal(new[] { "Brownie", "Croissant" }, catalogo[1].Productos.Select(p => p.Nombre));
    }

    // CP-05: no muestra productos inactivos ni categorías deshabilitadas
    [Fact]
    public void Ejecutar_ProductoInactivoYCategoriaDeshabilitada_NoSeMuestran()
    {
        _categorias.Categorias.Add(new Categoria { Id = 1, Nombre = "Repostería", Activa = true });
        _categorias.Categorias.Add(new Categoria { Id = 2, Nombre = "Temporada", Activa = false });
        AgregarProducto("Brownie", categoriaId: 1);
        AgregarProducto("Queque viejo", categoriaId: 1, activo: false);
        AgregarProducto("Rompope", categoriaId: 2);

        var catalogo = _casoDeUso.Ejecutar();

        var grupo = Assert.Single(catalogo);
        Assert.Equal("Repostería", grupo.NombreCategoria);
        Assert.Equal("Brownie", Assert.Single(grupo.Productos).Nombre);
    }

    [Fact]
    public void Ejecutar_CategoriaSinProductos_NoSeIncluye()
    {
        _categorias.Categorias.Add(new Categoria { Id = 1, Nombre = "Vacía", Activa = true });

        var catalogo = _casoDeUso.Ejecutar();

        Assert.Empty(catalogo);
    }

    [Fact]
    public void Constructor_RepositoriosNulos_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ConsultarCatalogoCasoDeUso(null!, _categorias));
        Assert.Throws<ArgumentNullException>(() => new ConsultarCatalogoCasoDeUso(_productos, null!));
    }

    private void AgregarProducto(string nombre, int categoriaId, bool activo = true) =>
        _productos.Insertar(new Producto { Nombre = nombre, Precio = 1000m, CategoriaId = categoriaId, Activo = activo });
}
