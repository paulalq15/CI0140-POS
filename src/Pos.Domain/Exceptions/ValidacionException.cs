namespace Pos.Domain.Exceptions;

/// <summary>
/// Error de validación de una regla de negocio. El mensaje debe estar en español,
/// decir qué falló e indicar qué corregir, sin detalles internos (RNF-24, Métricas 6.1 y 6.2).
/// </summary>
public class ValidacionException : Exception
{
    public ValidacionException()
    {
    }

    public ValidacionException(string mensaje)
        : base(mensaje)
    {
    }

    public ValidacionException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }

    public ValidacionException(string campo, string mensaje)
        : base(mensaje)
    {
        Campo = campo;
    }

    /// <summary>Nombre del dato que se debe corregir (por ejemplo, "Precio").</summary>
    public string? Campo { get; }
}
