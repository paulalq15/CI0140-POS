using Pos.Application.CasosDeUso.Inventario;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;
using Pos.Tests.Application.Fakes;

namespace Pos.Tests.Application;

public class ConsultarInventarioCasoDeUsoTests
{
    private readonly FakeInventarioRepository _inventario = new();
    private readonly FakeSucursalRepository _sucursales = new();
    private readonly ConsultarInventarioCasoDeUso _casoDeUso;

    public ConsultarInventarioCasoDeUsoTests()
    {
        _sucursales.Sucursales.Add(new Sucursal { Id = 1, Nombre = "Central" });
        _sucursales.Sucursales.Add(new Sucursal { Id = 2, Nombre = "San Pedro" });
        _inventario.Items.Add(new InventarioItem { SucursalId = 1, ProductoId = 1, NombreProducto = "Agua", Cantidad = 50 });
        _inventario.Items.Add(new InventarioItem { SucursalId = 2, ProductoId = 1, NombreProducto = "Agua", Cantidad = 7 });
        _casoDeUso = new ConsultarInventarioCasoDeUso(_inventario, _sucursales);
    }

    // CP-06: consulta de la sucursal correcta
    [Fact]
    public void Ejecutar_SucursalExistente_DevuelveSoloSuInventario()
    {
        var items = _casoDeUso.Ejecutar(2);

        var item = Assert.Single(items);
        Assert.Equal(7, item.Cantidad);
    }

    // CP-07: sucursal inexistente
    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void Ejecutar_SucursalInexistente_LanzaValidacionConMensajeEspecifico(int sucursalId)
    {
        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(sucursalId));

        Assert.Equal("SucursalId", excepcion.Campo);
        Assert.Contains("sucursal indicada no existe", excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ObtenerSucursales_DevuelveTodas()
    {
        Assert.Equal(2, _casoDeUso.ObtenerSucursales().Count);
    }

    [Fact]
    public void Constructor_RepositoriosNulos_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ConsultarInventarioCasoDeUso(null!, _sucursales));
        Assert.Throws<ArgumentNullException>(() => new ConsultarInventarioCasoDeUso(_inventario, null!));
    }
}
