namespace Pos.Domain.Entities;

public class Caja
{
    public int Id { get; set; }

    public int SucursalId { get; set; }

    public string Nombre { get; set; } = string.Empty;
}
