using Pos.Domain.Exceptions;

namespace Pos.Tests.Domain;

public class ValidacionExceptionTests
{
    [Fact]
    public void Constructor_ConCampoYMensaje_GuardaAmbos()
    {
        var excepcion = new ValidacionException("Precio", "El precio debe ser mayor que cero.");

        Assert.Equal("Precio", excepcion.Campo);
        Assert.Equal("El precio debe ser mayor que cero.", excepcion.Message);
    }

    [Fact]
    public void Constructor_SoloMensaje_NoTieneCampo()
    {
        var excepcion = new ValidacionException("Mensaje");

        Assert.Null(excepcion.Campo);
        Assert.Equal("Mensaje", excepcion.Message);
    }

    [Fact]
    public void Constructor_ConExcepcionInterna_LaConserva()
    {
        var interna = new InvalidOperationException("interna");

        var excepcion = new ValidacionException("Mensaje", interna);

        Assert.Same(interna, excepcion.InnerException);
    }

    [Fact]
    public void Constructor_SinParametros_CreaExcepcion()
    {
        var excepcion = new ValidacionException();

        Assert.Null(excepcion.Campo);
    }
}
