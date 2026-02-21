using UniversalAPIGateway.Api.Adapters;
using UniversalAPIGateway.Api.Services;
using UniversalAPIGateway.Application.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IGatewayRequestAdapter, GatewayRequestAdapter>();
builder.Services.AddApplication();
builder.Services.AddApiRuntimeDefaults();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
