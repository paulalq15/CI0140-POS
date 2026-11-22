using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class SqliteSucursalRepository : ISucursalRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteSucursalRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public bool Existe(int id)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT EXISTS (SELECT 1 FROM Sucursal WHERE Id = $id);";
        comando.Parameters.AddWithValue("$id", id);

        return Convert.ToInt64(comando.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) == 1;
    }

    public IReadOnlyList<Sucursal> ObtenerTodas()
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT Id, Nombre FROM Sucursal ORDER BY Id;";

        using var lector = comando.ExecuteReader();
        var sucursales = new List<Sucursal>();
        while (lector.Read())
        {
            sucursales.Add(new Sucursal { Id = lector.GetInt32(0), Nombre = lector.GetString(1) });
        }

        return sucursales;
    }
}
