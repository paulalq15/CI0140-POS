using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Tests.Application.Fakes;

// Repositorios en memoria para probar los casos de uso sin base de datos (RNF-17).

internal sealed class FakeProductoRepository : IProductoRepository
{
    public List<Producto> Productos { get; } = [];

    public int Insertar(Producto producto)
    {
        producto.Id = Productos.Count + 1;
        Productos.Add(producto);
        return producto.Id;
    }

    public Producto? ObtenerPorId(int id) => Productos.Find(p => p.Id == id);

    public IReadOnlyList<Producto> ObtenerActivos() => Productos.Where(p => p.Activo).ToList();

    public bool ExisteNombre(string nombre) =>
        Productos.Exists(p => string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
}

internal sealed class FakeCategoriaRepository : ICategoriaRepository
{
    public List<Categoria> Categorias { get; } = [];

    public Categoria? ObtenerPorId(int id) => Categorias.Find(c => c.Id == id);

    public IReadOnlyList<Categoria> ObtenerActivas() => Categorias.Where(c => c.Activa).ToList();
}

internal sealed class FakeSucursalRepository : ISucursalRepository
{
    public List<Sucursal> Sucursales { get; } = [];

    public bool Existe(int id) => Sucursales.Exists(s => s.Id == id);

    public IReadOnlyList<Sucursal> ObtenerTodas() => Sucursales;
}

internal sealed class FakeInventarioRepository : IInventarioRepository
{
    public List<InventarioItem> Items { get; } = [];

    public IReadOnlyList<InventarioItem> ObtenerPorSucursal(int sucursalId) =>
        Items.Where(i => i.SucursalId == sucursalId).ToList();

    public int ObtenerCantidad(int sucursalId, int productoId) =>
        Items.Find(i => i.SucursalId == sucursalId && i.ProductoId == productoId)?.Cantidad ?? 0;
}

internal sealed class FakeCajaRepository : ICajaRepository
{
    public List<Caja> Cajas { get; } = [];

    public bool ExisteEnSucursal(int sucursalId, int cajaId) =>
        Cajas.Exists(caja => caja.Id == cajaId && caja.SucursalId == sucursalId);

    public IReadOnlyList<Caja> ObtenerPorSucursal(int sucursalId) =>
        Cajas.Where(caja => caja.SucursalId == sucursalId).OrderBy(caja => caja.Id).ToList();
}

internal sealed class FakeVentaRepository : IVentaRepository
{
    public List<Venta> Ventas { get; } = [];

    public int RegistrarVenta(Venta venta)
    {
        Ventas.Add(venta);
        return Ventas.Count;
    }
}
