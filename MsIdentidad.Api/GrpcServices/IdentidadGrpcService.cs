using System.Text.Json;
using Atracciones.Grpc;
using Atracciones.Shared.Models;
using Grpc.Core;
using MsIdentidad.Api.Dtos;
using MsIdentidad.Api.Services;

namespace MsIdentidad.Api.GrpcServices;

public class IdentidadGrpcService : IdentidadGrpc.IdentidadGrpcBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly AuthService _authService;

    public IdentidadGrpcService(AuthService authService)
    {
        _authService = authService;
    }

    public override async Task<JsonReply> Login(JsonRequest request, ServerCallContext context)
    {
        var body = JsonSerializer.Deserialize<LoginRequest>(request.Json, JsonOptions) ?? new LoginRequest();
        var result = await _authService.LoginAsync(body, context.CancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login exitoso."));
    }

    public override async Task<JsonReply> Register(JsonRequest request, ServerCallContext context)
    {
        var body = JsonSerializer.Deserialize<RegisterRequest>(request.Json, JsonOptions) ?? new RegisterRequest();
        var result = await _authService.RegisterAsync(body, context.CancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Usuario registrado correctamente."));
    }

    private static JsonReply Ok<T>(T value)
    {
        return new JsonReply
        {
            StatusCode = StatusCodes.Status200OK,
            Json = JsonSerializer.Serialize(value, JsonOptions)
        };
    }
}
