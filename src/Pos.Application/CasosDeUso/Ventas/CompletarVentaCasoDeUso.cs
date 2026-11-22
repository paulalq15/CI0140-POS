using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Domain.Exceptions;

namespace Pos.Application.CasosDeUso.Ventas;

public sealed class CompletarVentaCasoDeUso(
    IProductoRepository productoRepository,
    IInventarioRepository inventarioRepository,
    ISucursalRepository sucursalRepository,
    ICajaRepository cajaRepository,
    IVentaRepository ventaRepository)
{
    private readonly IProductoRepository _productoRepository = productoRepository ?? throw new ArgumentNullException(nameof(productoRepository));
    private readonly IInventarioRepository _inventarioRepository = inventarioRepository ?? throw new ArgumentNullException(nameof(inventarioRepository));
    private readonly ISucursalRepository _sucursalRepository = sucursalRepository ?? throw new ArgumentNullException(nameof(sucursalRepository));
    private readonly ICajaRepository _cajaRepository = cajaRepository ?? throw new ArgumentNullException(nameof(cajaRepository));
    private readonly IVentaRepository _ventaRepository = ventaRepository ?? throw new ArgumentNullException(nameof(ventaRepository));

    public void ValidarCodigoProducto(int productoId)
    {
        ValidarCodigoPositivo(productoId);
        _ = ObtenerProductoActivo(productoId);
    }

    public void ValidarSucursal(int sucursalId)
    {
        if (sucursalId <= 0)
        {
            throw new ValidacionException(nameof(Venta.SucursalId), "La sucursal es obligatoria. Digite el número de una sucursal válida.");
        }

        if (!_sucursalRepository.Existe(sucursalId))
        {
            throw new ValidacionException(nameof(Venta.SucursalId), "La sucursal indicada no existe. Digite el número de una sucursal válida.");
        }
    }

    public decimal CalcularTotal(int sucursalId, IReadOnlyList<ProductoParaVenta> productos)
    {
        ValidarProductos(productos);
        ValidarSucursal(sucursalId);
        return SumarSubtotales(PrepararDetalles(sucursalId, productos));
    }

    public void ValidarCaja(int sucursalId, int cajaId)
    {
        if (cajaId <= 0)
        {
            throw new ValidacionException(nameof(Venta.CajaId), "La caja es obligatoria. Digite el número de una caja de la sucursal.");
        }

        if (!_cajaRepository.ExisteEnSucursal(sucursalId, cajaId))
        {
            throw new ValidacionException(nameof(Venta.CajaId), "La caja indicada no existe en esta sucursal. Digite el número de una caja de la sucursal seleccionada.");
        }
    }

    public Venta Ejecutar(CompletarVentaComando comando)
    {
        ArgumentNullException.ThrowIfNull(comando);
        ValidarSucursal(comando.SucursalId);
        ValidarCaja(comando.SucursalId, comando.CajaId);

        if (comando.MontoRecibido < 0)
        {
            throw new ValidacionException(nameof(comando.MontoRecibido), "El efectivo recibido no puede ser negativo. Digite el monto recibido.");
        }

        ValidarProductos(comando.Productos);
        var venta = new Venta
        {
            SucursalId = comando.SucursalId,
            CajaId = comando.CajaId,
            MontoRecibido = comando.MontoRecibido,
            MetodoPago = MetodoPago.Efectivo,
        };

        venta.Detalles.AddRange(PrepararDetalles(comando.SucursalId, comando.Productos));
        venta.Total = SumarSubtotales(venta.Detalles);

        if (venta.MontoRecibido < venta.Total)
        {
            throw new ValidacionException(nameof(comando.MontoRecibido), "El efectivo recibido no cubre el total. Digite un monto igual o superior al total de la venta.");
        }

        venta.Vuelto = venta.MontoRecibido - venta.Total;
        venta.Fecha = DateTime.UtcNow;
        venta.Id = _ventaRepository.RegistrarVenta(venta);
        return venta;
    }

    private IEnumerable<DetalleVenta> PrepararDetalles(int sucursalId, IReadOnlyList<ProductoParaVenta> productos)
    {
        foreach (var grupo in productos.GroupBy(producto => producto.ProductoId))
        {
            yield return CrearDetalle(sucursalId, grupo.Key, grupo.Sum(producto => (long)producto.Cantidad));
        }
    }

    private static decimal SumarSubtotales(IEnumerable<DetalleVenta> detalles)
    {
        try
        {
            return detalles.Sum(detalle => detalle.Subtotal);
        }
        catch (OverflowException)
        {
            throw new ValidacionException(nameof(CompletarVentaComando.Productos), "El total de la venta es demasiado alto. Reduzca las cantidades o divida la compra.");
        }
    }

    private static void ValidarProductos(IReadOnlyList<ProductoParaVenta> productos)
    {
        if (productos is null || productos.Count == 0)
        {
            throw new ValidacionException(nameof(CompletarVentaComando.Productos), "La venta está vacía. Agregue al menos un producto.");
        }

        foreach (var producto in productos)
        {
            ValidarCodigoPositivo(producto.ProductoId);

            if (producto.Cantidad <= 0)
            {
                throw new ValidacionException(nameof(producto.Cantidad), "La cantidad debe ser mayor que cero. Digite al menos una unidad.");
            }
        }
    }

    private static void ValidarCodigoPositivo(int productoId)
    {
        if (productoId <= 0)
        {
            throw new ValidacionException(nameof(ProductoParaVenta.ProductoId), "El código de producto es inválido. Digite un código del catálogo.");
        }
    }

    private Producto ObtenerProductoActivo(int productoId)
    {
        var producto = _productoRepository.ObtenerPorId(productoId)
            ?? throw new ValidacionException(nameof(ProductoParaVenta.ProductoId), $"No existe el producto con código {productoId}. Revise el código en el catálogo.");

        if (!producto.Activo)
        {
            throw new ValidacionException(nameof(ProductoParaVenta.ProductoId), $"El producto '{producto.Nombre}' está inactivo. Elija un producto activo del catálogo.");
        }

        return producto;
    }

    private DetalleVenta CrearDetalle(int sucursalId, int productoId, long cantidad)
    {
        var producto = ObtenerProductoActivo(productoId);
        var disponible = _inventarioRepository.ObtenerCantidad(sucursalId, productoId);
        if (disponible < cantidad)
        {
            throw new ValidacionException(nameof(DetalleVenta.Cantidad), $"Hay {disponible} unidades de '{producto.Nombre}' en la sucursal. Reduzca la cantidad o seleccione otro producto.");
        }

        return new DetalleVenta
        {
            ProductoId = productoId,
            Cantidad = (int)cantidad,
            PrecioUnitario = producto.Precio,
        };
    }
}
