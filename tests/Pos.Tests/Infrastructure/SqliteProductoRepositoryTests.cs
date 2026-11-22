using Pos.Domain.Entities;
using Pos.Infrastructure.Repositories;

namespace Pos.Tests.Infrastructure;

public sealed class SqliteProductoRepositoryTests : IDisposable
{
    private readonly BaseDeDatosDePrueba _baseDeDatos = new();
    private readonly SqliteProductoRepository _repositorio;

    public SqliteProductoRepositoryTests()
    {
        _repositorio = new SqliteProductoRepository(_baseDeDatos.Factory);
    }

    // Integración CP-01: el producto registrado queda guardado y se puede consultar
    [Fact]
    public void Insertar_ProductoValido_SePuedeObtenerConLosMismosDatos()
    {
        var producto = new Producto
        {
            Nombre = "Galleta de avena",
            Descripcion = "Artesanal",
            Precio = 950.50m,
            CategoriaId = 2,
        };

        var id = _repositorio.Insertar(producto);
        var guardado = _repositorio.ObtenerPorId(id);

        Assert.NotNull(guardado);
        Assert.Equal(id, producto.Id);
        Assert.Equal("Galleta de avena", guardado.Nombre);
        Assert.Equal("Artesanal", guardado.Descripcion);
        Assert.Equal(950.50m, guardado.Precio);
        Assert.Equal(2, guardado.CategoriaId);
        Assert.True(guardado.Activo);
    }

    [Fact]
    public void ObtenerPorId_IdInexistente_DevuelveNull()
    {
        Assert.Null(_repositorio.ObtenerPorId(999));
    }

    [Fact]
    public void ObtenerActivos_ConProductoInactivo_LoExcluyeYOrdenaPorNombre()
    {
        _baseDeDatos.Ejecutar("UPDATE Producto SET Activo = 0 WHERE Nombre = 'Brownie';");

        var nombres = _repositorio.ObtenerActivos().Select(p => p.Nombre).ToList();

        Assert.Equal(new[] { "Agua 600 ml", "Croissant", "Té frío" }, nombres);
    }

    [Fact]
    public void ExisteNombre_NombreNulo_DevuelveFalse()
    {
        Assert.False(_repositorio.ExisteNombre(null!));
    }

    [Theory]
    [InlineData("Croissant")]
    [InlineData("croissant")]
    [InlineData("CROISSANT")]
    public void ExisteNombre_NombreRegistrado_DevuelveTrueSinImportarMayusculas(string nombre)
    {
        Assert.True(_repositorio.ExisteNombre(nombre));
    }

    [Fact]
    public void ExisteNombre_NombreNuevo_DevuelveFalse()
    {
        Assert.False(_repositorio.ExisteNombre("Empanada"));
    }

    [Theory]
    [InlineData("TÉ FRÍO")]
    [InlineData("té frío")]
    [InlineData("  Té Frío  ")]
    public void ExisteNombre_NombreConTildeEnOtrasMayusculas_DevuelveTrue(string nombre)
    {
        Assert.True(_repositorio.ExisteNombre(nombre));
    }

    [Fact]
    public void ExisteNombre_TildeEscritaComoCaracterSeparado_DevuelveTrue()
    {
        // "Te" + tilde combinada + " fri" + tilde combinada + "o"
        Assert.True(_repositorio.ExisteNombre("Te\u0301 fri\u0301o"));
    }

    [Theory]
    [InlineData("Te frio")]
    [InlineData("TE FRIO")]
    public void ExisteNombre_MismoNombreSinTildes_DevuelveTrue(string nombre)
    {
        Assert.True(_repositorio.ExisteNombre(nombre));
    }

    [Fact]
    public void Insertar_ProductoNulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _repositorio.Insertar(null!));
    }

    [Fact]
    public void Constructor_FactoryNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqliteProductoRepository(null!));
    }

    public void Dispose() => _baseDeDatos.Dispose();
}
