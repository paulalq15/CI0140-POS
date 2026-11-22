using Microsoft.Data.Sqlite;
using Pos.Infrastructure.Persistence;

namespace Pos.Tests.Infrastructure;

public class SqliteConnectionFactoryTests
{
    [Fact]
    public void DesdeVariableDeEntorno_ConRutaDefinida_UsaEsaRuta()
    {
        var ruta = Path.Combine(Path.GetTempPath(), $"pos_env_{Guid.NewGuid():N}.db");
        Environment.SetEnvironmentVariable("POS_DB_PATH", ruta);
        try
        {
            var factory = SqliteConnectionFactory.DesdeVariableDeEntorno();
            using (factory.CrearConexionAbierta())
            {
            }

            Assert.True(File.Exists(ruta));
        }
        finally
        {
            Environment.SetEnvironmentVariable("POS_DB_PATH", null);
            SqliteConnection.ClearAllPools();
            File.Delete(ruta);
        }
    }

    [Fact]
    public void DesdeVariableDeEntorno_SinVariable_CreaFabricaConRutaPorDefecto()
    {
        Environment.SetEnvironmentVariable("POS_DB_PATH", null);

        var factory = SqliteConnectionFactory.DesdeVariableDeEntorno();

        Assert.NotNull(factory);
    }

    [Fact]
    public void CrearConexionAbierta_ActivaLlavesForaneas()
    {
        var factory = new SqliteConnectionFactory("Data Source=:memory:");

        using var conexion = factory.CrearConexionAbierta();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "PRAGMA foreign_keys;";

        Assert.Equal(1L, comando.ExecuteScalar());
    }

    [Fact]
    public void DatabaseInitializer_FactoryNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DatabaseInitializer(null!));
    }
}
