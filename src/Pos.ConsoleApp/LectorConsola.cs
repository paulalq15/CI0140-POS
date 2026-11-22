using System.Globalization;

namespace Pos.ConsoleApp;

/// <summary>Lee datos de la consola y repite la pregunta mientras el formato no sea válido.</summary>
internal static class LectorConsola
{
    public static string LeerTexto(string pregunta)
    {
        Console.Write(pregunta);
        return Console.ReadLine() ?? string.Empty;
    }

    public static int LeerEntero(string pregunta)
    {
        while (true)
        {
            var texto = LeerTexto(pregunta).Trim();
            if (int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var valor))
            {
                return valor;
            }

            Console.WriteLine("Valor no válido. Digite un número entero, por ejemplo: 2.");
        }
    }

    /// <summary>Acepta punto o coma como separador decimal (ej.: 1500, 1500.50 o 1500,50).</summary>
    public static decimal LeerDecimal(string pregunta)
    {
        while (true)
        {
            var texto = LeerTexto(pregunta).Trim().Replace(',', '.');
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
            {
                return valor;
            }

            Console.WriteLine("Valor no válido. Digite un monto numérico, por ejemplo: 1500 o 1500.50.");
        }
    }
}
