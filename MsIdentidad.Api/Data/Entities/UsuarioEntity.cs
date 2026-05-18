namespace MsIdentidad.Api.Data.Entities;

public class UsuarioEntity
{
    public int UsuId { get; set; }
    public Guid UsuGuid { get; set; }
    public string UsuLogin { get; set; } = null!;
    public string UsuPasswordHash { get; set; } = null!;
    public DateTimeOffset UsuFechaRegistro { get; set; }
    public string UsuUsuarioRegistro { get; set; } = null!;
    public string UsuIpRegistro { get; set; } = null!;
    public DateTimeOffset? UsuFechaMod { get; set; }
    public string? UsuUsuarioMod { get; set; }
    public string? UsuIpMod { get; set; }
    public DateTimeOffset? UsuFechaEliminacion { get; set; }
    public string? UsuUsuarioEliminacion { get; set; }
    public string? UsuIpEliminacion { get; set; }
    public string UsuEstado { get; set; } = "A";
    public ICollection<UsuarioRolEntity> UsuarioRoles { get; set; } = new List<UsuarioRolEntity>();
}
