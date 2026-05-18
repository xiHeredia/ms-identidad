using Atracciones.Shared.Exceptions;
using Atracciones.Shared.Security;
using Microsoft.EntityFrameworkCore;
using MsIdentidad.Api.Data;
using MsIdentidad.Api.Data.Entities;
using MsIdentidad.Api.Dtos;

namespace MsIdentidad.Api.Services;

public class AuthService
{
    private readonly AuthDbContext _context;
    private readonly JwtTokenFactory _tokenFactory;

    public AuthService(AuthDbContext context, JwtTokenFactory tokenFactory)
    {
        _context = context;
        _tokenFactory = tokenFactory;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ValidateCredentials(request.UserName, request.Password);

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Include(x => x.UsuarioRoles.Where(ur => ur.UsuRolEstado == "A"))
                .ThenInclude(x => x.Rol)
            .FirstOrDefaultAsync(x => x.UsuLogin == request.UserName && x.UsuEstado == "A", cancellationToken);

        if (usuario is null || usuario.UsuPasswordHash != request.Password)
            throw new UnauthorizedBusinessException("Usuario o contrasena invalidos.");

        return CreateAuthResponse(usuario);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ValidateCredentials(request.UserName, request.Password);

        var login = request.UserName.Trim();
        var exists = await _context.Usuarios.AnyAsync(
            x => x.UsuLogin == login && x.UsuEstado == "A",
            cancellationToken);

        if (exists)
            throw new ValidationException("El usuario ya existe.");

        var usuario = new UsuarioEntity
        {
            UsuGuid = Guid.NewGuid(),
            UsuLogin = login,
            UsuPasswordHash = request.Password,
            UsuFechaRegistro = DateTimeOffset.UtcNow,
            UsuUsuarioRegistro = "api",
            UsuIpRegistro = "127.0.0.1",
            UsuEstado = "A"
        };

        var requestedRoles = (request.Roles is { Count: > 0 } ? request.Roles : new[] { "CLIENTE" })
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        foreach (var roleName in requestedRoles)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(
                x => x.RolDescripcion == roleName && x.RolEstado == "A",
                cancellationToken);

            role ??= new RolEntity
            {
                RolGuid = Guid.NewGuid(),
                RolDescripcion = roleName,
                RolFechaIngreso = DateTimeOffset.UtcNow,
                RolUsuarioIngreso = "api",
                RolIpIngreso = "127.0.0.1",
                RolEstado = "A"
            };

            usuario.UsuarioRoles.Add(new UsuarioRolEntity
            {
                Rol = role,
                UsuRolEstado = "A"
            });
        }

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(usuario);
    }

    public async Task<IReadOnlyList<UsuarioResponse>> ListarUsuariosAsync(CancellationToken cancellationToken)
    {
        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .Include(x => x.UsuarioRoles.Where(ur => ur.UsuRolEstado == "A"))
                .ThenInclude(x => x.Rol)
            .Where(x => x.UsuEstado == "A")
            .OrderBy(x => x.UsuId)
            .ToListAsync(cancellationToken);

        return usuarios.Select(ToUsuarioResponse).ToList();
    }

    public async Task<int> CrearUsuarioAsync(CrearUsuarioRequest request, CancellationToken cancellationToken)
    {
        ValidateCredentials(request.Login, request.Password);

        var login = request.Login.Trim();
        var exists = await _context.Usuarios.AnyAsync(
            x => x.UsuLogin == login && x.UsuEstado == "A",
            cancellationToken);

        if (exists)
            throw new ValidationException("El usuario ya existe.");

        var clienteRole = await _context.Roles.FirstOrDefaultAsync(
            x => x.RolDescripcion == "CLIENTE" && x.RolEstado == "A",
            cancellationToken);

        clienteRole ??= new RolEntity
        {
            RolGuid = Guid.NewGuid(),
            RolDescripcion = "CLIENTE",
            RolFechaIngreso = DateTimeOffset.UtcNow,
            RolUsuarioIngreso = "api",
            RolIpIngreso = "127.0.0.1",
            RolEstado = "A"
        };

        var usuario = new UsuarioEntity
        {
            UsuGuid = Guid.NewGuid(),
            UsuLogin = login,
            UsuPasswordHash = request.Password,
            UsuFechaRegistro = DateTimeOffset.UtcNow,
            UsuUsuarioRegistro = "api",
            UsuIpRegistro = "127.0.0.1",
            UsuEstado = "A"
        };

        usuario.UsuarioRoles.Add(new UsuarioRolEntity
        {
            Rol = clienteRole,
            UsuRolEstado = "A"
        });

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return usuario.UsuId;
    }

    public async Task<bool> ActualizarUsuarioAsync(int id, ActualizarUsuarioRequest request, CancellationToken cancellationToken)
    {
        ValidateCredentials(request.Login, request.Password);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(
            x => x.UsuId == id && x.UsuEstado == "A",
            cancellationToken);

        if (usuario is null)
            throw new NotFoundException("No se encontro el usuario.");

        var login = request.Login.Trim();
        var exists = await _context.Usuarios.AnyAsync(
            x => x.UsuId != id && x.UsuLogin == login && x.UsuEstado == "A",
            cancellationToken);

        if (exists)
            throw new ValidationException("El usuario ya existe.");

        usuario.UsuLogin = login;
        usuario.UsuPasswordHash = request.Password;
        usuario.UsuFechaMod = DateTimeOffset.UtcNow;
        usuario.UsuUsuarioMod = "api";
        usuario.UsuIpMod = "127.0.0.1";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> EliminarUsuarioAsync(int id, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(x => x.UsuarioRoles.Where(ur => ur.UsuRolEstado == "A"))
            .FirstOrDefaultAsync(x => x.UsuId == id && x.UsuEstado == "A", cancellationToken);

        if (usuario is null)
            throw new NotFoundException("No se encontro el usuario.");

        usuario.UsuEstado = "I";
        usuario.UsuFechaEliminacion = DateTimeOffset.UtcNow;
        usuario.UsuUsuarioEliminacion = "api";
        usuario.UsuIpEliminacion = "127.0.0.1";

        foreach (var usuarioRol in usuario.UsuarioRoles)
            usuarioRol.UsuRolEstado = "I";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<RolResponse>> ListarRolesAsync(CancellationToken cancellationToken)
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(x => x.RolEstado == "A")
            .OrderBy(x => x.RolDescripcion)
            .Select(x => new RolResponse
            {
                Id = x.RolId,
                Guid = x.RolGuid,
                Descripcion = x.RolDescripcion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CrearRolAsync(CrearRolRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Descripcion))
            throw new ValidationException("La descripcion del rol es obligatoria.");

        var descripcion = request.Descripcion.Trim().ToUpperInvariant();
        var exists = await _context.Roles.AnyAsync(
            x => x.RolDescripcion == descripcion && x.RolEstado == "A",
            cancellationToken);

        if (exists)
            throw new ValidationException("El rol ya existe.");

        var role = new RolEntity
        {
            RolGuid = Guid.NewGuid(),
            RolDescripcion = descripcion,
            RolFechaIngreso = DateTimeOffset.UtcNow,
            RolUsuarioIngreso = "api",
            RolIpIngreso = "127.0.0.1",
            RolEstado = "A"
        };

        await _context.Roles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return role.RolId;
    }

    public async Task<bool> ActualizarRolAsync(int id, ActualizarRolRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Descripcion))
            throw new ValidationException("La descripcion del rol es obligatoria.");

        var rol = await _context.Roles.FirstOrDefaultAsync(
            x => x.RolId == id && x.RolEstado == "A",
            cancellationToken);

        if (rol is null)
            throw new NotFoundException("No se encontro el rol.");

        var descripcion = request.Descripcion.Trim().ToUpperInvariant();
        var exists = await _context.Roles.AnyAsync(
            x => x.RolId != id && x.RolDescripcion == descripcion && x.RolEstado == "A",
            cancellationToken);

        if (exists)
            throw new ValidationException("El rol ya existe.");

        rol.RolDescripcion = descripcion;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> EliminarRolAsync(int id, CancellationToken cancellationToken)
    {
        var rol = await _context.Roles
            .Include(x => x.UsuarioRoles.Where(ur => ur.UsuRolEstado == "A"))
            .FirstOrDefaultAsync(x => x.RolId == id && x.RolEstado == "A", cancellationToken);

        if (rol is null)
            throw new NotFoundException("No se encontro el rol.");

        rol.RolEstado = "I";
        rol.RolFechaEliminacion = DateTimeOffset.UtcNow;
        rol.RolUsuarioEliminacion = "api";
        rol.RolIpEliminacion = "127.0.0.1";

        foreach (var usuarioRol in rol.UsuarioRoles)
            usuarioRol.UsuRolEstado = "I";

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<UsuarioRolResponse>> ListarUsuarioRolesAsync(int? usuarioId, CancellationToken cancellationToken)
    {
        var query = _context.UsuarioRoles
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Include(x => x.Rol)
            .Where(x => x.UsuRolEstado == "A" && x.Usuario.UsuEstado == "A" && x.Rol.RolEstado == "A");

        if (usuarioId is not null)
            query = query.Where(x => x.UsuId == usuarioId.Value);

        var rows = await query
            .OrderBy(x => x.Usuario.UsuLogin)
            .ThenBy(x => x.Rol.RolDescripcion)
            .ToListAsync(cancellationToken);

        return rows.Select(ToUsuarioRolResponse).ToList();
    }

    public async Task<int> AsignarRolAsync(CrearUsuarioRolRequest request, CancellationToken cancellationToken)
    {
        if (request.UsuarioId <= 0 || request.RolId <= 0)
            throw new ValidationException("UsuarioId y RolId son obligatorios.");

        var usuarioExists = await _context.Usuarios.AnyAsync(
            x => x.UsuId == request.UsuarioId && x.UsuEstado == "A",
            cancellationToken);

        var rolExists = await _context.Roles.AnyAsync(
            x => x.RolId == request.RolId && x.RolEstado == "A",
            cancellationToken);

        if (!usuarioExists || !rolExists)
            throw new ValidationException("El usuario o rol indicado no existe.");

        var current = await _context.UsuarioRoles.FirstOrDefaultAsync(
            x => x.UsuId == request.UsuarioId && x.RolId == request.RolId,
            cancellationToken);

        if (current is not null)
        {
            current.UsuRolEstado = "A";
            await _context.SaveChangesAsync(cancellationToken);
            return current.UsuRolId;
        }

        var entity = new UsuarioRolEntity
        {
            UsuId = request.UsuarioId,
            RolId = request.RolId,
            UsuRolEstado = "A"
        };

        await _context.UsuarioRoles.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.UsuRolId;
    }

    private AuthResponse CreateAuthResponse(UsuarioEntity usuario)
    {
        var roles = usuario.UsuarioRoles
            .Where(x => x.UsuRolEstado == "A" && x.Rol.RolEstado == "A")
            .Select(x => x.Rol.RolDescripcion)
            .Distinct()
            .ToList();

        var token = _tokenFactory.Create(usuario.UsuGuid, usuario.UsuLogin, roles);

        return new AuthResponse
        {
            UsuarioId = usuario.UsuId,
            UsuarioGuid = usuario.UsuGuid,
            UserName = usuario.UsuLogin,
            Roles = roles,
            Token = token.Token,
            ExpirationUtc = token.ExpirationUtc
        };
    }

    private static UsuarioResponse ToUsuarioResponse(UsuarioEntity usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.UsuId,
            Guid = usuario.UsuGuid,
            Login = usuario.UsuLogin,
            Estado = usuario.UsuEstado,
            Roles = usuario.UsuarioRoles
                .Where(x => x.UsuRolEstado == "A" && x.Rol.RolEstado == "A")
                .Select(x => x.Rol.RolDescripcion)
                .Distinct()
                .ToList()
        };
    }

    private static UsuarioRolResponse ToUsuarioRolResponse(UsuarioRolEntity entity)
    {
        return new UsuarioRolResponse
        {
            UsuarioRolId = entity.UsuRolId,
            UsuarioId = entity.UsuId,
            UsuarioGuid = entity.Usuario.UsuGuid,
            UsuarioLogin = entity.Usuario.UsuLogin,
            RolId = entity.RolId,
            RolGuid = entity.Rol.RolGuid,
            RolDescripcion = entity.Rol.RolDescripcion
        };
    }

    private static void ValidateCredentials(string userName, string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(userName))
            errors.Add("El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(password))
            errors.Add("La contrasena es obligatoria.");

        if (errors.Count > 0)
            throw new ValidationException("Error de validacion.", errors);
    }
}
