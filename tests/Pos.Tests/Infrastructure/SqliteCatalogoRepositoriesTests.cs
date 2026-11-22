using Pos.Infrastructure.Repositories;

namespace Pos.Tests.Infrastructure;

public sealed class SqliteCategoriaRepositoryTests : IDisposable
{
    private readonly BaseDeDatosDePrueba _baseDeDatos = new();
    private readonly SqliteCategoriaRepository _repositorio;

    public SqliteCategoriaRepositoryTests()
    {
        _repositorio = new SqliteCategoriaRepository(_baseDeDatos.Factory);
    }

    [Fact]
    public void ObtenerPorId_CategoriaExistente_DevuelveSusDatos()
    {
        var categoria = _repositorio.ObtenerPorId(2);

        Assert.NotNull(categoria);
        Assert.Equal("Repostería", categoria.Nombre);
        Assert.True(categoria.Activa);
    }

    [Fact]
    public void ObtenerPorId_IdInexistente_DevuelveNull()
    {
        Assert.Null(_repositorio.ObtenerPorId(999));
    }

    [Fact]
    public void ObtenerActivas_ConCategoriaDeshabilitada_LaExcluye()
    {
        _baseDeDatos.Ejecutar("UPDATE Categoria SET Activa = 0 WHERE Id = 1;");

        var categoria = Assert.Single(_repositorio.ObtenerActivas());

        Assert.Equal("Repostería", categoria.Nombre);
    }

    [Fact]
    public void Constructor_FactoryNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqliteCategoriaRepository(null!));
    }

    public void Dispose() => _baseDeDatos.Dispose();
}

public sealed class SqliteSucursalRepositoryTests : IDisposable
{
    private readonly BaseDeDatosDePrueba _baseDeDatos = new();
    private readonly SqliteSucursalRepository _repositorio;

    public SqliteSucursalRepositoryTests()
    {
        _repositorio = new SqliteSucursalRepository(_baseDeDatos.Factory);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(99, false)]
    public void Existe_DevuelveSiLaSucursalEstaRegistrada(int id, bool esperado)
    {
        Assert.Equal(esperado, _repositorio.Existe(id));
    }

    [Fact]
    public void ObtenerTodas_DevuelveSucursalesRegistradas()
    {
        var sucursal = Assert.Single(_repositorio.ObtenerTodas());

        Assert.Equal("Sucursal Central", sucursal.Nombre);
    }

    [Fact]
    public void Constructor_FactoryNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqliteSucursalRepository(null!));
    }

    public void Dispose() => _baseDeDatos.Dispose();
}

public sealed class SqliteInventarioRepositoryTests : IDisposable
{
    private readonly BaseDeDatosDePrueba _baseDeDatos = new();
    private readonly SqliteInventarioRepository _repositorio;

    public SqliteInventarioRepositoryTests()
    {
        _repositorio = new SqliteInventarioRepository(_baseDeDatos.Factory);
    }

    // Integración CP-06: cantidades de la sucursal con el nombre del producto
    [Fact]
    public void ObtenerPorSucursal_SucursalConInventario_DevuelveCantidadesYNombres()
    {
        var items = _repositorio.ObtenerPorSucursal(1);

        Assert.Equal(4, items.Count);
        var brownie = Assert.Single(items, i => i.NombreProducto == "Brownie");
        Assert.Equal(5, brownie.Cantidad);
        Assert.All(items, i => Assert.Equal(1, i.SucursalId));
    }

    [Fact]
    public void ObtenerPorSucursal_ProductoSinInventario_ApareceConCantidadCero()
    {
        _baseDeDatos.Ejecutar("INSERT INTO Producto (Nombre, Precio, CategoriaId) VALUES ('Muffin', 1100, 2);");

        var muffin = Assert.Single(_repositorio.ObtenerPorSucursal(1), i => i.NombreProducto == "Muffin");

        Assert.Equal(0, muffin.Cantidad);
    }

    [Fact]
    public void ObtenerPorSucursal_ProductoInactivo_NoAparece()
    {
        _baseDeDatos.Ejecutar("UPDATE Producto SET Activo = 0 WHERE Nombre = 'Brownie';");

        var items = _repositorio.ObtenerPorSucursal(1);

        Assert.DoesNotContain(items, i => i.NombreProducto == "Brownie");
    }

    [Theory]
    [InlineData(1, 3, 10)]
    [InlineData(1, 999, 0)]
    public void ObtenerCantidad_DevuelveCantidadOCeroSiNoHayRegistro(int sucursalId, int productoId, int esperado)
    {
        Assert.Equal(esperado, _repositorio.ObtenerCantidad(sucursalId, productoId));
    }

    [Fact]
    public void Constructor_FactoryNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqliteInventarioRepository(null!));
    }

    public void Dispose() => _baseDeDatos.Dispose();
}
