namespace MsIdentidad.Api.Data.Entities;

public class RolEntity
{
    public int RolId { get; set; }
    public Guid RolGuid { get; set; }
    public string RolDescripcion { get; set; } = null!;
    public DateTimeOffset RolFechaIngreso { get; set; }
    public string RolUsuarioIngreso { get; set; } = null!;
    public string RolIpIngreso { get; set; } = null!;
    public DateTimeOffset? RolFechaEliminacion { get; set; }
    public string? RolUsuarioEliminacion { get; set; }
    public string? RolIpEliminacion { get; set; }
    public string RolEstado { get; set; } = "A";
    public ICollection<UsuarioRolEntity> UsuarioRoles { get; set; } = new List<UsuarioRolEntity>();
}
