using Pos.Application.CasosDeUso.Ventas;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;
using Pos.Infrastructure.Repositories;

namespace Pos.Tests.Infrastructure;

/// <summary>
/// Prueba el CU-2 completo (caso de uso + repositorios SQLite) sobre una base de datos temporal.
/// Datos iniciales de la sucursal 1: Agua (1) ₡800 con 50 unidades, Té frío (2) ₡1200 con 30,
/// Croissant (3) ₡1500 con 10 y Brownie (4) ₡1300 con 5.
/// </summary>
public sealed class CompletarVentaIntegracionTests : IDisposable
{
    private const int Agua = 1;
    private const int Te = 2;
    private const int Croissant = 3;
    private const int Brownie = 4;

    private readonly BaseDeDatosDePrueba _baseDeDatos = new();
    private readonly CompletarVentaCasoDeUso _casoDeUso;

    public CompletarVentaIntegracionTests()
    {
        var factory = _baseDeDatos.Factory;
        _casoDeUso = new CompletarVentaCasoDeUso(
            new SqliteProductoRepository(factory),
            new SqliteInventarioRepository(factory),
            new SqliteSucursalRepository(factory),
            new SqliteCajaRepository(factory),
            new SqliteVentaRepository(factory));
    }

    // CP-08 (integración): la venta se guarda y descuenta el inventario
    [Fact]
    public void Ejecutar_VentaValida_GuardaLaVentaYDescuentaElInventario()
    {
        var venta = _casoDeUso.Ejecutar(new CompletarVentaComando(1, 1, [new(Croissant, 2), new(Brownie, 1)], 5000m));

        Assert.Equal(4300m, venta.Total);
        Assert.Equal(700m, venta.Vuelto);
        Assert.Equal(1, _baseDeDatos.ConsultarNumero($"SELECT COUNT(*) FROM Venta WHERE Id = {venta.Id};"));
        Assert.Equal(8, Inventario(Croissant));
        Assert.Equal(4, Inventario(Brownie));
    }

    // CP-08 (integración): todos los importes y detalles quedan guardados tal como se calcularon
    [Fact]
    public void Ejecutar_VentaValida_GuardaImportesYDetallesEnLaBaseDeDatos()
    {
        var venta = _casoDeUso.Ejecutar(new CompletarVentaComando(1, 2, [new(Agua, 2), new(Te, 1)], 3000m));

        using var conexion = _baseDeDatos.Factory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = """
            SELECT v.CajaId, v.Total, v.MontoRecibido, v.Vuelto, v.MetodoPago, d.ProductoId, d.Cantidad, d.PrecioUnitario
            FROM Venta v JOIN DetalleVenta d ON d.VentaId = v.Id
            WHERE v.Id = $ventaId ORDER BY d.ProductoId;
            """;
        comando.Parameters.AddWithValue("$ventaId", venta.Id);
        using var lector = comando.ExecuteReader();

        Assert.True(lector.Read());
        Assert.Equal(2, lector.GetInt32(0));
        Assert.Equal(2800m, lector.GetDecimal(1));
        Assert.Equal(3000m, lector.GetDecimal(2));
        Assert.Equal(200m, lector.GetDecimal(3));
        Assert.Equal((int)MetodoPago.Efectivo, lector.GetInt32(4));
        Assert.Equal((Agua, 2, 800m), (lector.GetInt32(5), lector.GetInt32(6), lector.GetDecimal(7)));
        Assert.True(lector.Read());
        Assert.Equal((Te, 1, 1200m), (lector.GetInt32(5), lector.GetInt32(6), lector.GetDecimal(7)));
        Assert.False(lector.Read());
        Assert.Equal(48, Inventario(Agua));
        Assert.Equal(29, Inventario(Te));
    }

    // CP-14 (integración): inventario insuficiente, no se guarda la venta y el inventario queda igual
    [Fact]
    public void Ejecutar_InventarioInsuficiente_NoGuardaLaVentaYElInventarioQuedaIgual()
    {
        Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(new CompletarVentaComando(1, 1, [new(Croissant, 1), new(Brownie, 6)], 20000m)));

        Assert.Equal(0, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
        Assert.Equal(10, Inventario(Croissant));
        Assert.Equal(5, Inventario(Brownie));
    }

    // CP-15 (integración): monto insuficiente, no se guarda la venta y el inventario queda igual
    [Fact]
    public void Ejecutar_MontoInsuficiente_NoGuardaLaVentaYElInventarioQuedaIgual()
    {
        Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(new CompletarVentaComando(1, 1, [new(Croissant, 2)], 2999m)));

        Assert.Equal(0, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
        Assert.Equal(10, Inventario(Croissant));
    }

    // CP-16 (integración): caja de otra sucursal, no se guarda la venta y el inventario queda igual
    [Fact]
    public void Ejecutar_CajaDeOtraSucursal_NoGuardaLaVentaYElInventarioQuedaIgual()
    {
        _baseDeDatos.Ejecutar("""
            INSERT INTO Sucursal (Nombre) VALUES ('Sucursal Este');
            INSERT INTO Caja (SucursalId, Nombre) VALUES (2, 'Caja Este');
            """);

        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(new CompletarVentaComando(1, 3, [new(Agua, 1)], 800m)));

        Assert.Equal(nameof(Venta.CajaId), excepcion.Campo);
        Assert.Equal(0, _baseDeDatos.ConsultarNumero("SELECT COUNT(*) FROM Venta;"));
        Assert.Equal(50, Inventario(Agua));
    }

    public void Dispose() => _baseDeDatos.Dispose();

    private long Inventario(int productoId) =>
        _baseDeDatos.ConsultarNumero($"SELECT Cantidad FROM Inventario WHERE SucursalId = 1 AND ProductoId = {productoId};");
}
