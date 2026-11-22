using Pos.Application.CasosDeUso.Productos;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;
using Pos.Tests.Application.Fakes;

namespace Pos.Tests.Application;

public class RegistrarProductoCasoDeUsoTests
{
    private readonly FakeProductoRepository _productos = new();
    private readonly FakeCategoriaRepository _categorias = new();
    private readonly RegistrarProductoCasoDeUso _casoDeUso;

    public RegistrarProductoCasoDeUsoTests()
    {
        _categorias.Categorias.Add(new Categoria { Id = 1, Nombre = "Repostería", Activa = true });
        _categorias.Categorias.Add(new Categoria { Id = 2, Nombre = "Temporada", Activa = false });
        _casoDeUso = new RegistrarProductoCasoDeUso(_productos, _categorias);
    }

    // CP-01: registro exitoso
    [Fact]
    public void Ejecutar_DatosValidos_RegistraProductoActivoYDevuelveId()
    {
        var comando = new RegistrarProductoComando("  Galleta de avena  ", " Artesanal ", 900m, 1);

        var id = _casoDeUso.Ejecutar(comando);

        var guardado = Assert.Single(_productos.Productos);
        Assert.Equal(id, guardado.Id);
        Assert.Equal("Galleta de avena", guardado.Nombre);
        Assert.Equal("Artesanal", guardado.Descripcion);
        Assert.Equal(900m, guardado.Precio);
        Assert.True(guardado.Activo);
    }

    [Fact]
    public void Ejecutar_SinDescripcion_GuardaDescripcionVacia()
    {
        var id = _casoDeUso.Ejecutar(new RegistrarProductoComando("Muffin", null, 1100m, 1));

        Assert.Equal(string.Empty, _productos.ObtenerPorId(id)!.Descripcion);
    }

    // CP-02: precio inválido
    [Fact]
    public void Ejecutar_PrecioCero_LanzaValidacionYNoGuarda()
    {
        var comando = new RegistrarProductoComando("Muffin", null, 0m, 1);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Equal(nameof(Producto.Precio), excepcion.Campo);
        Assert.Empty(_productos.Productos);
    }

    [Fact]
    public void Ejecutar_NombreNulo_LanzaValidacion()
    {
        var comando = new RegistrarProductoComando(null!, null, 900m, 1);

        var excepcion = Assert.Throws<ValidacionException>(() => _casoDeUso.Ejecutar(comando));

        Assert.Equal(nameof(Producto.Nombre), excepcion.Campo);
    }

    // CP-03: nombre repetido (sin importar mayúsculas)
    [Fact]
    public void Ejecutar_NombreRepetido_LanzaValidacionYNoDuplica()
    {
        _casoDeUso.Ejecutar(new RegistrarProductoComando("Brownie", null, 1300m, 1));

        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(new RegistrarProductoComando("BROWNIE", null, 1500m, 1)));

        Assert.Contains("Ya existe un producto", excepcion.Message, StringComparison.Ordinal);
        Assert.Single(_productos.Productos);
    }

    [Fact]
    public void Ejecutar_CategoriaInexistente_LanzaValidacion()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(new RegistrarProductoComando("Muffin", null, 1100m, 99)));

        Assert.Equal(nameof(Producto.CategoriaId), excepcion.Campo);
        Assert.Contains("no existe", excepcion.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Ejecutar_CategoriaDeshabilitada_LanzaValidacion()
    {
        var excepcion = Assert.Throws<ValidacionException>(
            () => _casoDeUso.Ejecutar(new RegistrarProductoComando("Muffin", null, 1100m, 2)));

        Assert.Contains("deshabilitada", excepcion.Message, StringComparison.Ordinal);
        Assert.Empty(_productos.Productos);
    }

    [Fact]
    public void Ejecutar_ComandoNulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _casoDeUso.Ejecutar(null!));
    }

    [Fact]
    public void ObtenerCategoriasDisponibles_DevuelveSoloActivas()
    {
        var categorias = _casoDeUso.ObtenerCategoriasDisponibles();

        var unica = Assert.Single(categorias);
        Assert.Equal("Repostería", unica.Nombre);
    }

    [Fact]
    public void Constructor_RepositoriosNulos_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RegistrarProductoCasoDeUso(null!, _categorias));
        Assert.Throws<ArgumentNullException>(() => new RegistrarProductoCasoDeUso(_productos, null!));
    }
}
