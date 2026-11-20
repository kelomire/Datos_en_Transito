using Aplicacion.Servicios;
using API.DTOs;
using Datos_en_Transito.Persistencia;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Persistencia.Interfaces;
using Persistencia.Repositorios;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "Datos en tránsito API",
    Version = "v1",
    Description = "API para administrar dispositivos y simulaciones."
}));

var database = builder.Configuration.GetSection("Database");
var host = Environment.GetEnvironmentVariable("DB_HOST") ?? database["Host"] ?? "localhost";
var port = uint.TryParse(Environment.GetEnvironmentVariable("DB_PORT") ?? database["Port"], out var configuredPort) ? configuredPort : 3306;
var name = Environment.GetEnvironmentVariable("DB_NAME") ?? database["Name"] ?? "datos_transito";
var adminUser = Environment.GetEnvironmentVariable("DB_USER_ADMIN") ?? database["AdminUser"] ?? "administrador";
var readUser = Environment.GetEnvironmentVariable("DB_USER_READONLY") ?? database["ReadUser"] ?? "desarrollo";
static string BuildConnectionString(string host, uint port, string database, string user) =>
    new MySqlConnectionStringBuilder { Server = host, Port = port, Database = database, UserID = user }.ConnectionString;

builder.Services.AddSingleton<IDbConnectionFactory>(_ => new MySqlConnectionFactory(
    BuildConnectionString(host, port, name, adminUser),
    BuildConnectionString(host, port, name, readUser)));
builder.Services.AddScoped<IDispositivoRepository, DispositivoRepository>();
builder.Services.AddScoped<ISimulacionRepository, SimulacionRepository>();
builder.Services.AddScoped<IEstadisticaRepository, EstadisticaRepository>();
builder.Services.AddScoped<DispositivoService>();
builder.Services.AddScoped<SimulacionService>();
builder.Services.AddScoped<EstadisticaService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Datos en tránsito API v1"); c.RoutePrefix = "swagger"; });
app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html"));
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

