using Pos.Domain.Entities;
using Pos.Domain.Exceptions;

namespace Pos.Tests.Domain;

public class ProductoTests
{
    [Fact]
    public void Validar_DatosValidos_NoLanzaExcepcion()
    {
        var producto = CrearProductoValido();

        var excepcion = Record.Exception(producto.Validar);

        Assert.Null(excepcion);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_NombreVacio_LanzaValidacionConMensajeEspecifico(string nombre)
    {
        var producto = CrearProductoValido();
        producto.Nombre = nombre;

        var excepcion = Assert.Throws<ValidacionException>(producto.Validar);

        Assert.Equal(nameof(Producto.Nombre), excepcion.Campo);
        Assert.Contains("nombre del producto es obligatorio", excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validar_NombreConLongitudMaxima_EsAceptado()
    {
        var producto = CrearProductoValido();
        producto.Nombre = new string('a', Producto.LongitudMaximaNombre);

        var excepcion = Record.Exception(producto.Validar);

        Assert.Null(excepcion);
    }

    [Fact]
    public void Validar_NombreMasLargoQueElMaximo_LanzaValidacion()
    {
        var producto = CrearProductoValido();
        producto.Nombre = new string('a', Producto.LongitudMaximaNombre + 1);

        var excepcion = Assert.Throws<ValidacionException>(producto.Validar);

        Assert.Equal(nameof(Producto.Nombre), excepcion.Campo);
    }

    [Fact]
    public void Validar_DescripcionMasLargaQueElMaximo_LanzaValidacion()
    {
        var producto = CrearProductoValido();
        producto.Descripcion = new string('d', Producto.LongitudMaximaDescripcion + 1);

        var excepcion = Assert.Throws<ValidacionException>(producto.Validar);

        Assert.Equal(nameof(Producto.Descripcion), excepcion.Campo);
    }

    public static TheoryData<decimal> PreciosInvalidos => new(0m, -1m, -1500.50m);

    [Theory]
    [MemberData(nameof(PreciosInvalidos))]
    public void Validar_PrecioCeroONegativo_LanzaValidacion(decimal precio)
    {
        var producto = CrearProductoValido();
        producto.Precio = precio;

        var excepcion = Assert.Throws<ValidacionException>(producto.Validar);

        Assert.Equal(nameof(Producto.Precio), excepcion.Campo);
        Assert.Contains("mayor que cero", excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validar_PrecioMinimoPositivo_EsAceptado()
    {
        var producto = CrearProductoValido();
        producto.Precio = 0.01m;

        var excepcion = Record.Exception(producto.Validar);

        Assert.Null(excepcion);
    }

    [Fact]
    public void Validar_SinCategoria_LanzaValidacion()
    {
        var producto = CrearProductoValido();
        producto.CategoriaId = 0;

        var excepcion = Assert.Throws<ValidacionException>(producto.Validar);

        Assert.Equal(nameof(Producto.CategoriaId), excepcion.Campo);
    }

    private static Producto CrearProductoValido() => new()
    {
        Nombre = "Galleta de avena",
        Descripcion = "Galleta artesanal",
        Precio = 900m,
        CategoriaId = 2,
    };
}
