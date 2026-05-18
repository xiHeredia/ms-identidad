namespace MsIdentidad.Api.Dtos;

public class LoginRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterRequest
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public IReadOnlyCollection<string>? Roles { get; set; }
}

public class AuthResponse
{
    public int UsuarioId { get; set; }
    public Guid UsuarioGuid { get; set; }
    public string UserName { get; set; } = null!;
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    public string? Token { get; set; }
    public DateTime? ExpirationUtc { get; set; }
}

public class UsuarioResponse
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Login { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}

public class CrearUsuarioRequest
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class ActualizarUsuarioRequest : CrearUsuarioRequest
{
}

public class RolResponse
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Descripcion { get; set; } = null!;
}

public class UsuarioRolResponse
{
    public int UsuarioRolId { get; set; }
    public int UsuarioId { get; set; }
    public Guid UsuarioGuid { get; set; }
    public string UsuarioLogin { get; set; } = null!;
    public int RolId { get; set; }
    public Guid RolGuid { get; set; }
    public string RolDescripcion { get; set; } = null!;
}

public class CrearUsuarioRolRequest
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
}

public class CrearRolRequest
{
    public string Descripcion { get; set; } = null!;
}

public class ActualizarRolRequest : CrearRolRequest
{
}
