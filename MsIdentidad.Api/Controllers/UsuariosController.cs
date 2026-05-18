using Atracciones.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsIdentidad.Api.Dtos;
using MsIdentidad.Api.Services;

namespace MsIdentidad.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly AuthService _authService;

    public UsuariosController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var result = await _authService.ListarUsuariosAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UsuarioResponse>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request, CancellationToken cancellationToken)
    {
        var id = await _authService.CrearUsuarioAsync(request, cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Usuario creado correctamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var ok = await _authService.ActualizarUsuarioAsync(id, request, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(ok, "Usuario actualizado correctamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var ok = await _authService.EliminarUsuarioAsync(id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(ok, "Usuario eliminado correctamente."));
    }
}
