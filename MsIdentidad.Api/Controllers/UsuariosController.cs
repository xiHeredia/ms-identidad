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
}
