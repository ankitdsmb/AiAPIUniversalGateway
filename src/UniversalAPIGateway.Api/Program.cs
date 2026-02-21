using UniversalAPIGateway.Api.Adapters;
using UniversalAPIGateway.Api.Services;
using UniversalAPIGateway.Application.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IGatewayRequestAdapter, GatewayRequestAdapter>();
builder.Services.AddApplication();
builder.Services.AddApiRuntimeDefaults();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Universal API Gateway v1");
});

app.MapControllers();

app.Run();
