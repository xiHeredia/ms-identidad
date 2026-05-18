using Atracciones.Shared.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using MsIdentidad.Api.Data;
using MsIdentidad.Api.GrpcServices;
using MsIdentidad.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2);
});

builder.Services.AddAtraccionesApiDefaults(builder.Configuration, "ms-identidad");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));
builder.Services.AddScoped<AuthService>();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseAtraccionesApiDefaults();
app.MapGrpcService<IdentidadGrpcService>();

app.Run();
