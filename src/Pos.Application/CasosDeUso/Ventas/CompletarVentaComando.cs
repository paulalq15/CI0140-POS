namespace Pos.Application.CasosDeUso.Ventas;

public sealed record CompletarVentaComando(
    int SucursalId,
    int CajaId,
    IReadOnlyList<ProductoParaVenta> Productos,
    decimal MontoRecibido);

public readonly record struct ProductoParaVenta(int ProductoId, int Cantidad);
