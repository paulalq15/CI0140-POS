using Microsoft.Data.Sqlite;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure.Repositories;

public sealed class SqliteCategoriaRepository : ICategoriaRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteCategoriaRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public Categoria? ObtenerPorId(int id)
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT Id, Nombre, Activa FROM Categoria WHERE Id = $id;";
        comando.Parameters.AddWithValue("$id", id);

        using var lector = comando.ExecuteReader();
        return lector.Read() ? Mapear(lector) : null;
    }

    public IReadOnlyList<Categoria> ObtenerActivas()
    {
        using var conexion = _connectionFactory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT Id, Nombre, Activa FROM Categoria WHERE Activa = 1 ORDER BY Nombre;";

        using var lector = comando.ExecuteReader();
        var categorias = new List<Categoria>();
        while (lector.Read())
        {
            categorias.Add(Mapear(lector));
        }

        return categorias;
    }

    private static Categoria Mapear(SqliteDataReader lector) => new()
    {
        Id = lector.GetInt32(0),
        Nombre = lector.GetString(1),
        Activa = lector.GetBoolean(2),
    };
}
