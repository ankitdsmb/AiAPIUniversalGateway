using UniversalAPIGateway.Application.DependencyInjection;
using UniversalAPIGateway.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
