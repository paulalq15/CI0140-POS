namespace Pos.ConsoleApp;

/// <summary>Menú de consola del POS. Solo lee y muestra datos; la lógica está en Application.</summary>
public sealed class MenuPrincipal
{
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
            Console.Write("Seleccione una opción: ");

            var opcion = Console.ReadLine()?.Trim();
            switch (opcion)
            {
                case "1":
                case "2":
                case "3":
                    Console.WriteLine("Pendiente: se implementa en CU-1.");
                    break;
                case "4":
                    Console.WriteLine("Pendiente: se implementa en CU-2.");
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opción no válida. Digite un número del 0 al 4.");
                    break;
            }
        }
    }
}
