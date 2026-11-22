using System.Text;
using Pos.Application.CasosDeUso.Inventario;
using Pos.Application.CasosDeUso.Productos;
using Pos.ConsoleApp;
using Pos.Infrastructure.Persistence;
using Pos.Infrastructure.Repositories;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

try
{
    // Composición de dependencias: aquí se conectan las capas.
    var connectionFactory = SqliteConnectionFactory.DesdeVariableDeEntorno();
    new DatabaseInitializer(connectionFactory).Inicializar();

    var productoRepository = new SqliteProductoRepository(connectionFactory);
    var categoriaRepository = new SqliteCategoriaRepository(connectionFactory);
    var inventarioRepository = new SqliteInventarioRepository(connectionFactory);
    var sucursalRepository = new SqliteSucursalRepository(connectionFactory);

    var menu = new MenuPrincipal(
        new RegistrarProductoCasoDeUso(productoRepository, categoriaRepository),
        new ConsultarCatalogoCasoDeUso(productoRepository, categoriaRepository),
        new ConsultarInventarioCasoDeUso(inventarioRepository, sucursalRepository));

    menu.Ejecutar();
}
#pragma warning disable CA1031 // Último recurso: no mostrar detalles internos al usuario (RNF-22)
catch (Exception ex)
#pragma warning restore CA1031
{
    await Console.Error.WriteLineAsync("Ocurrió un error inesperado y la aplicación se cerrará. Intente de nuevo o contacte al administrador.");
    await Console.Error.WriteLineAsync($"Detalle técnico (para soporte): {ex.GetType().Name}");
}
