using UniversalAPIGateway.Api.Adapters;
using UniversalAPIGateway.Application.DependencyInjection;
using UniversalAPIGateway.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IGatewayRequestAdapter, GatewayRequestAdapter>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
