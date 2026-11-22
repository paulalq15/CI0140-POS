using Pos.Application.CasosDeUso.Ventas;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;
using Pos.Tests.Application.Fakes;

namespace Pos.Tests.Application;

public class CompletarVentaCasoDeUsoTests
{
    private const int Sucursal = 1;
    private const int Caja = 1;
    private const int Agua = 1;
    private const int Croissant = 2;
    private const int ProductoInactivo = 3;
    private const int ProductoCarisimo = 4;
    private const int Galleta = 5;

    private readonly FakeProductoRepository _productos = new();
    private readonly FakeInventarioRepository _inventario = new();
    private readonly FakeSucursalRepository _sucursales = new();
    private readonly FakeCajaRepository _cajas = new();
    private readonly FakeVentaRepository _ventas = new();
    private readonly CompletarVentaCasoDeUso _casoDeUso;

    public CompletarVentaCasoDeUsoTests()
    {
        _sucursales.Sucursales.Add(new Sucursal { Id = 1, Nombre = "Central" });
        _sucursales.Sucursales.Add(new Sucursal { Id = 2, Nombre = "San Pedro" });
        _cajas.Cajas.Add(new Caja { Id = 1, SucursalId = 1 });
        _cajas.Cajas.Add(new Caja { Id = 2, SucursalId = 1 });
        _cajas.Cajas.Add(new Caja { Id = 3, SucursalId = 2 });

        _productos.Productos.Add(new Producto { Id = Agua, Nombre = "Agua 600 ml", Precio = 800m, CategoriaId = 1 });
        _productos.Productos.Add(new Producto { Id = Croissant, Nombre = "Croissant", Precio = 1500m, CategoriaId = 2 });
        _productos.Productos.Add(new Producto { Id = ProductoInactivo, Nombre = "Queque de temporada", Precio = 500m, CategoriaId = 2, Activo = false });
        _productos.Productos.Add(new Producto { Id = ProductoCarisimo, Nombre = "Pastel de exhibición", Precio = decimal.MaxValue, CategoriaId = 2 });
        _productos.Productos.Add(new Producto { Id = Galleta, Nombre = "Galleta", Precio = 800.25m, CategoriaId = 2 });

        AgregarInventario(1, Agua, 50);
        AgregarInventario(1, Croissant, 10);
        AgregarInventario(1, ProductoInactivo, 5);
        AgregarInventario(1, ProductoCarisimo, 10);
        AgregarInventario(1, Galleta, 3);
        AgregarInventario(2, Agua, 3);

        _casoDeUso = new CompletarVentaCasoDeUso(_productos, _inventario, _sucursales, _cajas, _ventas);
    }

    // CP-08: venta exitosa con cálculo de total y vuelto
    [Fact]
    public void Ejecutar_VentaValida_CalculaTotalYVueltoYRegistraLaVenta()
    {
        var inicio = DateTime.UtcNow;
        var comando = Comando([new(Agua, 2), new(Croissant, 1)], montoRecibido: 5000m);

        var venta = _casoDeUso.Ejecutar(comando);

        Assert.Same(venta, Assert.Single(_ventas.Ventas));
        Assert.Equal(1, venta.Id);
        Assert.Equal(Sucursal, venta.SucursalId);
        Assert.Equal(Caja, venta.CajaId);
        Assert.Equal(MetodoPago.Efectivo, venta.MetodoPago);
        Assert.Equal(3100m, venta.Total);
        Assert.Equal(5000m, venta.MontoRecibido);
        Assert.Equal(1900m, venta.Vuelto);
        Assert.InRange(venta.Fecha, inicio, DateTime.UtcNow);
        Assert.Equal(2, venta.Detalles.Count);
        Assert.Contains(venta.Detalles, d => d.ProductoId == Agua && d.Cantidad == 2 && d.PrecioUnitario == 800m);
        Assert.Contains(venta.Detalles, d => d.ProductoId == Croissant && d.Cantidad == 1 && d.PrecioUnitario == 1500m);
    }

    // CP-09: valor límite, el efectivo alcanza exactamente
    [Fact]
    public void Ejecutar_MontoExacto_VueltoCero()
    {
        var venta = _casoDeUso.Ejecutar(Comando([new(Croissant, 2)], montoRecibido: 3000m));

        Assert.Equal(3000m, venta.Total);
        Assert.Equal(0m, venta.Vuelto);
    }

    // CP-09: precios con céntimos, sin errores de redondeo en el total ni en el vuelto
    [Theory]
    [InlineData(2400.75, 0)]
    [InlineData(2500, 99.25)]
    public void Ejecutar_PreciosConCentimos_CalculaTotalYVueltoExactos(decimal recibido, decimal vuelto)
    {
        var venta = _casoDeUso.Ejecutar(Comando([new(Galleta, 1), new(Galleta, 2)], recibido));

        Assert.Equal(2400.75m, venta.Total);
        Assert.Equal(vuelto, venta.Vuelto);
        var detalle = Assert.Single(venta.Detalles);
        Assert.Equal(3, detalle.Cantidad);
        Assert.Equal(800.25m, detalle.PrecioUnitario);
    }

    [Fact]
    public void Ejecutar_ProductoRepetido_SumaLasCantidadesEnUnSoloDetalle()
    {
        var venta = _casoDeUso.Ejecutar(Comando([new(Agua, 2), new(Agua, 3)], montoRecibido: 4000m));

        var detalle = Assert.Single(venta.Detalles);
        Assert.Equal(5, detalle.Cantidad);
        Assert.Equal(4000m, venta.Total);
    }

    // CP-10: valor límite, se vende todo el inventario disponible
    [Fact]
    public void Ejecutar_CantidadIgualAlInventario_SePermite()
    {
        var venta = _casoDeUso.Ejecutar(Comando([new(Croissant, 10)], montoRecibido: 15000m));

        Assert.Equal(10, Assert.Single(venta.Detalles).Cantidad);
    }

    // CP-11: venta vacía
    [Fact]
    public void Ejecutar_SinProductos_LanzaValidacionYNoRegistra()
    {
        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(Comando([], 1000m)));

        Assert.Equal(nameof(CompletarVentaComando.Productos), excepcion.Campo);
        Assert.Contains("vacía", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    [Fact]
    public void Ejecutar_ListaDeProductosNula_LanzaValidacion()
    {
        var comando = new CompletarVentaComando(Sucursal, Caja, null!, 1000m);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Equal(nameof(CompletarVentaComando.Productos), excepcion.Campo);
    }

    // CP-12: cantidad inválida (valor límite)
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Ejecutar_CantidadCeroONegativa_LanzaValidacionYNoRegistra(int cantidad)
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(Agua, cantidad)], 1000m)));

        Assert.Equal(nameof(ProductoParaVenta.Cantidad), excepcion.Campo);
        Assert.Contains("mayor que cero", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Ejecutar_CodigoDeProductoInvalido_LanzaValidacion(int productoId)
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(productoId, 1)], 1000m)));

        Assert.Equal(nameof(ProductoParaVenta.ProductoId), excepcion.Campo);
        Assert.Contains("inválido", excepcion.Message, StringComparison.Ordinal);
    }

    // CP-13: producto inexistente o inactivo
    [Fact]
    public void Ejecutar_ProductoInexistente_LanzaValidacionConElCodigo()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(99, 1)], 1000m)));

        Assert.Equal(nameof(ProductoParaVenta.ProductoId), excepcion.Campo);
        Assert.Contains("No existe el producto con código 99", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    [Fact]
    public void Ejecutar_ProductoInactivo_LanzaValidacionYNoRegistra()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(ProductoInactivo, 1)], 1000m)));

        Assert.Contains("está inactivo", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    // CP-14: inventario insuficiente
    [Fact]
    public void Ejecutar_InventarioInsuficiente_LanzaValidacionConLaCantidadDisponible()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(Croissant, 11)], 20000m)));

        Assert.Equal(nameof(DetalleVenta.Cantidad), excepcion.Campo);
        Assert.Contains("Hay 10 unidades de 'Croissant'", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    [Fact]
    public void Ejecutar_ProductoRepetidoQueJuntoSuperaElInventario_LanzaValidacion()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(Croissant, 6), new(Croissant, 5)], 20000m)));

        Assert.Contains("Hay 10 unidades", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    // Valor límite: la suma de las líneas pasa el máximo de int y no debe desbordarse
    [Fact]
    public void Ejecutar_CantidadesQueJuntasSuperanElMaximoDeInt_RechazaPorInventario()
    {
        var comando = Comando([new(Agua, int.MaxValue), new(Agua, 1)], decimal.MaxValue);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Equal(nameof(DetalleVenta.Cantidad), excepcion.Campo);
        Assert.Contains("Hay 50 unidades", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    [Fact]
    public void Ejecutar_UsaElInventarioDeLaSucursalDeLaVenta()
    {
        var comando = new CompletarVentaComando(2, 3, [new(Agua, 4)], 5000m);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Contains("Hay 3 unidades", excepcion.Message, StringComparison.Ordinal);
    }

    // CP-15: efectivo insuficiente (valor límite: un colón menos que el total)
    [Fact]
    public void Ejecutar_MontoInsuficiente_LanzaValidacionYNoRegistra()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(Agua, 2), new(Croissant, 1)], montoRecibido: 3099m)));

        Assert.Equal(nameof(CompletarVentaComando.MontoRecibido), excepcion.Campo);
        Assert.Contains("no cubre el total", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    [Fact]
    public void Ejecutar_MontoNegativo_LanzaValidacion()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(Agua, 1)], montoRecibido: -1m)));

        Assert.Equal(nameof(CompletarVentaComando.MontoRecibido), excepcion.Campo);
        Assert.Contains("negativo", excepcion.Message, StringComparison.Ordinal);
    }

    // CP-16: sucursal o caja inválidas
    [Fact]
    public void Ejecutar_SucursalCero_LanzaValidacionDeSucursalObligatoria()
    {
        var comando = new CompletarVentaComando(0, Caja, [new(Agua, 1)], 1000m);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Equal(nameof(Venta.SucursalId), excepcion.Campo);
        Assert.Contains("obligatoria", excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Ejecutar_SucursalInexistente_LanzaValidacion()
    {
        var comando = new CompletarVentaComando(99, Caja, [new(Agua, 1)], 1000m);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Equal(nameof(Venta.SucursalId), excepcion.Campo);
        Assert.Contains("no existe", excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Ejecutar_CajaCero_LanzaValidacionDeCajaObligatoria()
    {
        var comando = new CompletarVentaComando(Sucursal, 0, [new(Agua, 1)], 1000m);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Equal(nameof(Venta.CajaId), excepcion.Campo);
        Assert.Contains("obligatoria", excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Ejecutar_CajaDeOtraSucursal_LanzaValidacionYNoRegistra()
    {
        var comando = new CompletarVentaComando(Sucursal, 3, [new(Agua, 1)], 1000m);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Equal(nameof(Venta.CajaId), excepcion.Campo);
        Assert.Contains("no existe en esta sucursal", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    [Fact]
    public void Ejecutar_TotalQueDesbordaElTipoDecimal_LanzaValidacion()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(Comando([new(ProductoCarisimo, 2)], decimal.MaxValue)));

        Assert.Equal(nameof(CompletarVentaComando.Productos), excepcion.Campo);
        Assert.Contains("demasiado alto", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_ventas.Ventas);
    }

    [Fact]
    public void Ejecutar_ComandoNulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _casoDeUso.Ejecutar(null!));
    }

    [Fact]
    public void CalcularTotal_ProductosValidos_DevuelveLaSumaDeSubtotales()
    {
        var total = _casoDeUso.CalcularTotal(Sucursal, [new(Agua, 3), new(Croissant, 2)]);

        Assert.Equal(5400m, total);
        Assert.Empty(_ventas.Ventas);
    }

    [Fact]
    public void CalcularTotal_SinProductos_LanzaValidacion()
    {
        Assert.Throws<ValidacionException>(() => _casoDeUso.CalcularTotal(Sucursal, []));
    }

    [Fact]
    public void CalcularTotal_SucursalInexistente_LanzaValidacion()
    {
        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.CalcularTotal(99, [new(Agua, 1)]));

        Assert.Equal(nameof(Venta.SucursalId), excepcion.Campo);
    }

    [Fact]
    public void ValidarCodigoProducto_ProductoActivo_NoLanzaExcepcion()
    {
        var excepcion = Record.Exception(() => _casoDeUso.ValidarCodigoProducto(Agua));

        Assert.Null(excepcion);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(ProductoInactivo)]
    public void ValidarCodigoProducto_CodigoInvalidoInexistenteOInactivo_LanzaValidacion(int productoId)
    {
        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.ValidarCodigoProducto(productoId));

        Assert.Equal(nameof(ProductoParaVenta.ProductoId), excepcion.Campo);
    }

    [Fact]
    public void ValidarCaja_CajaDeLaSucursal_NoLanzaExcepcion()
    {
        var excepcion = Record.Exception(() => _casoDeUso.ValidarCaja(Sucursal, 2));

        Assert.Null(excepcion);
    }

    [Fact]
    public void Constructor_RepositoriosNulos_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CompletarVentaCasoDeUso(null!, _inventario, _sucursales, _cajas, _ventas));
        Assert.Throws<ArgumentNullException>(() => new CompletarVentaCasoDeUso(_productos, null!, _sucursales, _cajas, _ventas));
        Assert.Throws<ArgumentNullException>(() => new CompletarVentaCasoDeUso(_productos, _inventario, null!, _cajas, _ventas));
        Assert.Throws<ArgumentNullException>(() => new CompletarVentaCasoDeUso(_productos, _inventario, _sucursales, null!, _ventas));
        Assert.Throws<ArgumentNullException>(() => new CompletarVentaCasoDeUso(_productos, _inventario, _sucursales, _cajas, null!));
    }

    private static CompletarVentaComando Comando(List<ProductoParaVenta> productos, decimal montoRecibido) =>
        new(Sucursal, Caja, productos, montoRecibido);

    private void AgregarInventario(int sucursalId, int productoId, int cantidad) =>
        _inventario.Items.Add(new InventarioItem { SucursalId = sucursalId, ProductoId = productoId, Cantidad = cantidad });
}
