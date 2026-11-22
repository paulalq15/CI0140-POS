namespace Pos.Application.CasosDeUso.Productos;

/// <summary>Datos que ingresa el administrador para registrar un producto (US-01).</summary>
public sealed record RegistrarProductoComando(string Nombre, string? Descripcion, decimal Precio, int CategoriaId);
