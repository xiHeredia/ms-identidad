namespace MsIdentidad.Api.Data.Entities;

public class UsuarioRolEntity
{
    public int UsuRolId { get; set; }
    public int UsuId { get; set; }
    public int RolId { get; set; }
    public string UsuRolEstado { get; set; } = "A";
    public UsuarioEntity Usuario { get; set; } = null!;
    public RolEntity Rol { get; set; } = null!;
}
