using Atracciones.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsIdentidad.Api.Dtos;
using MsIdentidad.Api.Services;

namespace MsIdentidad.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/roles")]
public class RolesController : ControllerBase
{
    private readonly AuthService _authService;

    public RolesController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var result = await _authService.ListarRolesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RolResponse>>.Ok(result, "Consulta exitosa."));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearRolRequest request, CancellationToken cancellationToken)
    {
        var id = await _authService.CrearRolAsync(request, cancellationToken);
        return Ok(ApiResponse<int>.Ok(id, "Rol creado correctamente."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarRolRequest request, CancellationToken cancellationToken)
    {
        var ok = await _authService.ActualizarRolAsync(id, request, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(ok, "Rol actualizado correctamente."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var ok = await _authService.EliminarRolAsync(id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(ok, "Rol eliminado correctamente."));
    }
}
