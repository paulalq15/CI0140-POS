using System.Globalization;
using Pos.Application.Interfaces;
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
}
