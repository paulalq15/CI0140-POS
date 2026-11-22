namespace Pos.Infrastructure.Persistence;

/// <summary>Crea las tablas de la base de datos y carga datos iniciales si está vacía.</summary>
public sealed class DatabaseInitializer
{
    private const string Esquema = """
        CREATE TABLE IF NOT EXISTS Categoria (
            Id      INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre  TEXT    NOT NULL UNIQUE,
            Activa  INTEGER NOT NULL DEFAULT 1
        );

        CREATE TABLE IF NOT EXISTS Producto (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre      TEXT    NOT NULL UNIQUE,
            Descripcion TEXT    NOT NULL DEFAULT '',
            Precio      NUMERIC NOT NULL CHECK (Precio > 0),
            CategoriaId INTEGER NOT NULL REFERENCES Categoria(Id),
            Activo      INTEGER NOT NULL DEFAULT 1
        );

        CREATE TABLE IF NOT EXISTS Sucursal (
            Id     INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT    NOT NULL UNIQUE
        );

        CREATE TABLE IF NOT EXISTS Caja (
            Id         INTEGER PRIMARY KEY AUTOINCREMENT,
            SucursalId INTEGER NOT NULL REFERENCES Sucursal(Id),
            Nombre     TEXT    NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Inventario (
            SucursalId INTEGER NOT NULL REFERENCES Sucursal(Id),
            ProductoId INTEGER NOT NULL REFERENCES Producto(Id),
            Cantidad   INTEGER NOT NULL CHECK (Cantidad >= 0),
            PRIMARY KEY (SucursalId, ProductoId)
        );

        CREATE TABLE IF NOT EXISTS Venta (
            Id            INTEGER PRIMARY KEY AUTOINCREMENT,
            SucursalId    INTEGER NOT NULL REFERENCES Sucursal(Id),
            CajaId        INTEGER NOT NULL REFERENCES Caja(Id),
            Fecha         TEXT    NOT NULL,
            Total         NUMERIC NOT NULL CHECK (Total >= 0),
            MontoRecibido NUMERIC NOT NULL,
            Vuelto        NUMERIC NOT NULL CHECK (Vuelto >= 0),
            MetodoPago    INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS DetalleVenta (
            Id             INTEGER PRIMARY KEY AUTOINCREMENT,
            VentaId        INTEGER NOT NULL REFERENCES Venta(Id),
            ProductoId     INTEGER NOT NULL REFERENCES Producto(Id),
            Cantidad       INTEGER NOT NULL CHECK (Cantidad > 0),
            PrecioUnitario NUMERIC NOT NULL
        );
        """;

    private const string DatosIniciales = """
        INSERT INTO Categoria (Nombre) VALUES ('Bebidas embotelladas'), ('Repostería');
        INSERT INTO Sucursal (Nombre) VALUES ('Sucursal Central');
        INSERT INTO Caja (SucursalId, Nombre) VALUES (1, 'Caja 1'), (1, 'Caja 2');
        INSERT INTO Producto (Nombre, Descripcion, Precio, CategoriaId) VALUES
            ('Agua 600 ml', 'Agua embotellada', 800, 1),
            ('Té frío', 'Té frío de limón', 1200, 1),
            ('Croissant', 'Croissant de mantequilla', 1500, 2),
            ('Brownie', 'Brownie de chocolate', 1300, 2);
        INSERT INTO Inventario (SucursalId, ProductoId, Cantidad) VALUES
            (1, 1, 50), (1, 2, 30), (1, 3, 10), (1, 4, 5);
        """;

    private readonly SqliteConnectionFactory _connectionFactory;

    public DatabaseInitializer(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public void Inicializar()
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();

        using (var crearTablas = conexion.CreateCommand())
        {
            crearTablas.CommandText = Esquema;
            crearTablas.ExecuteNonQuery();
        }

        using var contar = conexion.CreateCommand();
        contar.CommandText = "SELECT COUNT(*) FROM Categoria;";
        var cantidadCategorias = Convert.ToInt64(contar.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
        if (cantidadCategorias > 0)
        {
            return;
        }

        using var transaccion = conexion.BeginTransaction();
        using var insertar = conexion.CreateCommand();
        insertar.Transaction = transaccion;
        insertar.CommandText = DatosIniciales;
        insertar.ExecuteNonQuery();
        transaccion.Commit();
    }
}
