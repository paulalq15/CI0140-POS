using Pos.Domain.Entities;
using Pos.Domain.Exceptions;
using Pos.Infrastructure.Repositories;

namespace Pos.Tests.Infrastructure;

// Datos iniciales: sucursal 1 con cajas 1 y 2; inventario Agua (1) = 50, Té (2) = 30,
// Croissant (3) = 10 y Brownie (4) = 5.
public sealed class SqliteVentaRepositoryTests : IDisposable
{
    private const int Agua = 1;
    private const int Croissant = 3;
    private const int Brownie = 4;

    private readonly BaseDeDatosDePrueba _baseDeDatos = new();
    private readonly SqliteVentaRepository _repositorio;

    public SqliteVentaRepositoryTests()
    {
        _repositorio = new SqliteVentaRepository(_baseDeDatos.Factory);
    }

    // Integración CP-08: la venta, sus detalles y el descuento de inventario quedan guardados
    [Fact]
    public void RegistrarVenta_VentaValida_GuardaVentaYDetallesYDescuentaInventario()
    {
        var venta = CrearVenta(cajaId: 1, (Agua, 2, 800m), (Croissant, 1, 1500m));

        var id = _repositorio.RegistrarVenta(venta);

        Assert.True(id > 0);
        Assert.Equal(1, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
        Assert.Equal(3100, _baseDeDatos.ConsultarNumero($"SELECT Total FROM Venta WHERE Id = {id};"));
        Assert.Equal(1900, _baseDeDatos.ConsultarNumero($"SELECT Vuelto FROM Venta WHERE Id = {id};"));
        Assert.Equal(2, _baseDeDatos.ConsultarNumero($"SELECT COUNT(*) FROM DetalleVenta WHERE VentaId = {id};"));
        Assert.Equal(48, Inventario(Agua));
        Assert.Equal(9, Inventario(Croissant));
    }

    // Integración CP-14 (RNF-08, RNF-09): si un producto no alcanza, se revierte todo
    [Fact]
    public void RegistrarVenta_SegundoProductoSinInventarioSuficiente_NoGuardaNadaYRevierteElInventario()
    {
        var venta = CrearVenta(cajaId: 1, (Agua, 2, 800m), (Brownie, 6, 1300m));

        var excepcion = Assert.Throws<ValidacionException>(() => _repositorio.RegistrarVenta(venta));

        Assert.Equal(nameof(DetalleVenta.Cantidad), excepcion.Campo);
        Assert.Equal(0, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
        Assert.Equal(0, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM DetalleVenta;"));
        Assert.Equal(50, Inventario(Agua));
        Assert.Equal(5, Inventario(Brownie));
    }

    [Fact]
    public void RegistrarVenta_ProductoSinRegistroDeInventario_LanzaValidacionYNoGuarda()
    {
        _baseDeDatos.Ejecutar("INSERT INTO Producto (Nombre, Precio, CategoriaId) VALUES ('Muffin', 1100, 2);");
        var venta = CrearVenta(cajaId: 1, (5, 1, 1100m));

        Assert.Throws<ValidacionException>(() => _repositorio.RegistrarVenta(venta));

        Assert.Equal(0, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
    }

    [Fact]
    public void RegistrarVenta_CajaDeOtraSucursal_LanzaValidacionYNoGuarda()
    {
        _baseDeDatos.Ejecutar("""
            INSERT INTO Sucursal (Nombre) VALUES ('Sucursal Este');
            INSERT INTO Caja (SucursalId, Nombre) VALUES (2, 'Caja Este');
            """);
        var venta = CrearVenta(cajaId: 3, (Agua, 1, 800m));

        var excepcion = Assert.Throws<ValidacionException>(() => _repositorio.RegistrarVenta(venta));

        Assert.Equal(nameof(Venta.CajaId), excepcion.Campo);
        Assert.Equal(0, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
        Assert.Equal(50, Inventario(Agua));
    }

    // La caja pasó a otra sucursal entre la validación del caso de uso y el guardado
    [Fact]
    public void RegistrarVenta_CajaYaNoPerteneceALaSucursal_NoGuardaLaVenta()
    {
        _baseDeDatos.Ejecutar("""
            INSERT INTO Sucursal (Nombre) VALUES ('Sucursal Este');
            UPDATE Caja SET SucursalId = 2 WHERE Id = 1;
            """);
        var venta = CrearVenta(cajaId: 1, (Agua, 1, 800m));

        var excepcion = Assert.Throws<ValidacionException>(() => _repositorio.RegistrarVenta(venta));

        Assert.Equal(nameof(Venta.CajaId), excepcion.Campo);
        Assert.Equal(0, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
        Assert.Equal(50, Inventario(Agua));
    }

    // Métrica 2.1: varias cajas venden el último inventario al mismo tiempo
    [Fact]
    public void RegistrarVenta_VentasSimultaneasDelMismoProducto_NoVendeMasDeLoQueHay()
    {
        const int intentos = 10;
        var exitosas = 0;
        var rechazadas = 0;

        Parallel.For(0, intentos, new ParallelOptions { MaxDegreeOfParallelism = intentos }, i =>
        {
            var repositorio = new SqliteVentaRepository(_baseDeDatos.Factory);
            var venta = CrearVenta(cajaId: (i % 2) + 1, (Brownie, 1, 1300m));
            try
            {
                repositorio.RegistrarVenta(venta);
                Interlocked.Increment(ref exitosas);
            }
            catch (ValidacionException)
            {
                Interlocked.Increment(ref rechazadas);
            }
        });

        Assert.Equal(5, exitosas);
        Assert.Equal(5, rechazadas);
        Assert.Equal(0, Inventario(Brownie));
        Assert.Equal(5, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
        Assert.Equal(5, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM DetalleVenta;"));
    }

    [Fact]
    public void RegistrarVenta_VentaNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _repositorio.RegistrarVenta(null!));
    }

    [Fact]
    public void Constructor_FactoryNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqliteVentaRepository(null!));
    }

    public void Dispose() => _baseDeDatos.Dispose();

    private long Inventario(int productoId) =>
        _baseDeDatos.ConsultarNumero($"SELECT Cantidad FROM Inventario WHERE SucursalId = 1 AND ProductoId = {productoId};");

    private static Venta CrearVenta(int cajaId, params (int ProductoId, int Cantidad, decimal Precio)[] lineas)
    {
        var venta = new Venta { SucursalId = 1, CajaId = cajaId, Fecha = DateTime.UtcNow, MontoRecibido = 5000m };
        foreach (var (productoId, cantidad, precio) in lineas)
        {
            venta.Detalles.Add(new DetalleVenta { ProductoId = productoId, Cantidad = cantidad, PrecioUnitario = precio });
        }

        venta.Total = venta.Detalles.Sum(d => d.Subtotal);
        venta.Vuelto = Math.Max(0, venta.MontoRecibido - venta.Total);
        return venta;
    }
}

public sealed class SqliteCajaRepositoryTests : IDisposable
{
    private readonly BaseDeDatosDePrueba _baseDeDatos = new();
    private readonly SqliteCajaRepository _repositorio;

    public SqliteCajaRepositoryTests()
    {
        _baseDeDatos.Ejecutar("""
            INSERT INTO Sucursal (Nombre) VALUES ('Sucursal Este');
            INSERT INTO Caja (SucursalId, Nombre) VALUES (2, 'Caja Este');
            """);
        _repositorio = new SqliteCajaRepository(_baseDeDatos.Factory);
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(1, 2, true)]
    [InlineData(2, 3, true)]
    [InlineData(1, 3, false)]
    [InlineData(2, 1, false)]
    [InlineData(1, 99, false)]
    public void ExisteEnSucursal_DevuelveSiLaCajaPerteneceALaSucursal(int sucursalId, int cajaId, bool esperado)
    {
        Assert.Equal(esperado, _repositorio.ExisteEnSucursal(sucursalId, cajaId));
    }

    [Fact]
    public void ObtenerPorSucursal_DevuelveSoloLasCajasDeLaSucursalEnOrden()
    {
        var cajas = _repositorio.ObtenerPorSucursal(1);

        Assert.Equal([(1, "Caja 1"), (2, "Caja 2")], cajas.Select(caja => (caja.Id, caja.Nombre)));
        Assert.All(cajas, caja => Assert.Equal(1, caja.SucursalId));
    }

    [Fact]
    public void Constructor_FactoryNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqliteCajaRepository(null!));
    }

    public void Dispose() => _baseDeDatos.Dispose();
}
