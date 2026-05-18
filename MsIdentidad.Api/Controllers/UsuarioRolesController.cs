using Atracciones.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsIdentidad.Api.Dtos;
using MsIdentidad.Api.Services;

namespace MsIdentidad.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/usuarios-roles")]
public class UsuarioRolesController : ControllerBase
{
    private readonly AuthService _authService;

    public UsuarioRolesController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var result = await _authService.ListarUsuarioRolesAsync(null, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UsuarioRolResponse>>.Ok(result, "Consulta exitosa."));
    }

    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<IActionResult> ListarPorUsuario(int usuarioId, CancellationToken cancellationToken)
    {
        var result = await _authService.ListarUsuarioRolesAsync(usuarioId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UsuarioRolResponse>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRolRequest request, CancellationToken cancellationToken)
    {
        var id = await _authService.AsignarRolAsync(request, cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Rol asignado correctamente."));
    }
}
