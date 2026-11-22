using System.Globalization;
using Pos.Application.CasosDeUso.Inventario;
using Pos.Application.CasosDeUso.Productos;
using Pos.Domain.Exceptions;

namespace Pos.ConsoleApp;

/// <summary>Menú de consola del POS. Solo lee y muestra datos; la lógica está en Application.</summary>
public sealed class MenuPrincipal
{
    private static readonly CultureInfo FormatoMoneda = CultureInfo.GetCultureInfo("es-CR");

    private readonly RegistrarProductoCasoDeUso _registrarProducto;
    private readonly ConsultarCatalogoCasoDeUso _consultarCatalogo;
    private readonly ConsultarInventarioCasoDeUso _consultarInventario;

    public MenuPrincipal(
        RegistrarProductoCasoDeUso registrarProducto,
        ConsultarCatalogoCasoDeUso consultarCatalogo,
        ConsultarInventarioCasoDeUso consultarInventario)
    {
        _registrarProducto = registrarProducto ?? throw new ArgumentNullException(nameof(registrarProducto));
        _consultarCatalogo = consultarCatalogo ?? throw new ArgumentNullException(nameof(consultarCatalogo));
        _consultarInventario = consultarInventario ?? throw new ArgumentNullException(nameof(consultarInventario));
    }

    public void Ejecutar()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("===== POS Cafetería =====");
            Console.WriteLine("1. Registrar producto (CU-1)");
            Console.WriteLine("2. Consultar catálogo (CU-1)");
            Console.WriteLine("3. Consultar inventario de sucursal (CU-1)");
            Console.WriteLine("4. Realizar venta en efectivo (CU-2)");
            Console.WriteLine("0. Salir");

            var opcion = LectorConsola.LeerTexto("Seleccione una opción: ").Trim();
            if (opcion == "0")
            {
                return;
            }

            try
            {
                EjecutarOpcion(opcion);
            }
            catch (ValidacionException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private void EjecutarOpcion(string opcion)
    {
        switch (opcion)
        {
            case "1":
                RegistrarProducto();
                break;
            case "2":
                ConsultarCatalogo();
                break;
            case "3":
                ConsultarInventario();
                break;
            case "4":
                Console.WriteLine("Pendiente: se implementa en CU-2.");
                break;
            default:
                Console.WriteLine("Opción no válida. Digite un número del 0 al 4.");
                break;
        }
    }

    private void RegistrarProducto()
    {
        Console.WriteLine();
        Console.WriteLine("--- Registrar producto ---");

        var categorias = _registrarProducto.ObtenerCategoriasDisponibles();
        if (categorias.Count == 0)
        {
            Console.WriteLine("No hay categorías activas. Registre una categoría antes de agregar productos.");
            return;
        }

        var nombre = LectorConsola.LeerTexto("Nombre: ");
        var descripcion = LectorConsola.LeerTexto("Descripción (opcional): ");
        var precio = LectorConsola.LeerDecimal("Precio (₡): ");

        Console.WriteLine("Categorías disponibles:");
        foreach (var categoria in categorias)
        {
            Console.WriteLine($"  {categoria.Id}. {categoria.Nombre}");
        }

        var categoriaId = LectorConsola.LeerEntero("Número de categoría: ");

        var id = _registrarProducto.Ejecutar(new RegistrarProductoComando(nombre, descripcion, precio, categoriaId));
        Console.WriteLine($"Producto registrado correctamente con el código {id}.");
    }

    private void ConsultarCatalogo()
    {
        Console.WriteLine();
        Console.WriteLine("--- Catálogo de productos ---");

        var catalogo = _consultarCatalogo.Ejecutar();
        if (catalogo.Count == 0)
        {
            Console.WriteLine("No hay productos activos para la venta.");
            return;
        }

        foreach (var grupo in catalogo)
        {
            Console.WriteLine($"[{grupo.NombreCategoria}]");
            foreach (var producto in grupo.Productos)
            {
                Console.WriteLine($"  {producto.Id,3}  {producto.Nombre,-30} {producto.Precio.ToString("C0", FormatoMoneda),12}");
            }
        }
    }

    private void ConsultarInventario()
    {
        Console.WriteLine();
        Console.WriteLine("--- Inventario por sucursal ---");

        Console.WriteLine("Sucursales:");
        foreach (var sucursal in _consultarInventario.ObtenerSucursales())
        {
            Console.WriteLine($"  {sucursal.Id}. {sucursal.Nombre}");
        }

        var sucursalId = LectorConsola.LeerEntero("Número de sucursal: ");
        var items = _consultarInventario.Ejecutar(sucursalId);

        Console.WriteLine($"{"Producto",-30} {"Cantidad",8}");
        foreach (var item in items)
        {
            Console.WriteLine($"{item.NombreProducto,-30} {item.Cantidad,8}");
        }
    }
}
