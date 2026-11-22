using System.Globalization;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class SqliteCajaRepository(SqliteConnectionFactory connectionFactory) : ICajaRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    public bool ExisteEnSucursal(int sucursalId, int cajaId)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT EXISTS (SELECT 1 FROM Caja WHERE Id = $cajaId AND SucursalId = $sucursalId);";
        comando.Parameters.AddWithValue("$cajaId", cajaId);
        comando.Parameters.AddWithValue("$sucursalId", sucursalId);

        return Convert.ToInt64(comando.ExecuteScalar(), CultureInfo.InvariantCulture) == 1;
    }

    public IReadOnlyList<Caja> ObtenerPorSucursal(int sucursalId)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT Id, Nombre FROM Caja WHERE SucursalId = $sucursalId ORDER BY Id;";
        comando.Parameters.AddWithValue("$sucursalId", sucursalId);

        using var lector = comando.ExecuteReader();
        var cajas = new List<Caja>();
        while (lector.Read())
        {
            cajas.Add(new Caja { Id = lector.GetInt32(0), SucursalId = sucursalId, Nombre = lector.GetString(1) });
        }

        return cajas;
    }
}
