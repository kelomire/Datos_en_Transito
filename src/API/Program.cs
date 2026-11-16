using Aplicacion.Servicios;
using API.DTOs;
using Datos_en_Transito.Persistencia;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Persistencia.Interfaces;
using Persistencia.Repositorios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Datos en tránsito API", Version = "v1" });
});

var database = builder.Configuration.GetSection("Database");
var host = Environment.GetEnvironmentVariable("DB_HOST") ?? database["Host"] ?? "localhost";
var port = uint.TryParse(Environment.GetEnvironmentVariable("DB_PORT") ?? database["Port"], out var configuredPort)
    ? configuredPort
    : 3306;
var name = Environment.GetEnvironmentVariable("DB_NAME") ?? database["Name"] ?? "datos_transito";
var adminUser = Environment.GetEnvironmentVariable("DB_USER_ADMIN") ?? database["AdminUser"] ?? "administrador";
var readUser = Environment.GetEnvironmentVariable("DB_USER_READONLY") ?? database["ReadUser"] ?? "desarrollo";

static string BuildConnectionString(string host, uint port, string database, string user) =>
    new MySqlConnectionStringBuilder
    {
        Server = host,
        Port = port,
        Database = database,
        UserID = user
    }.ConnectionString;

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
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Datos en tránsito API v1");
    c.RoutePrefix = "swagger";
});

app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html"));
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.MapGet("/api/dispositivos", (DispositivoService service) =>
    Results.Ok(service.Listar().Select(dispositivo => new
    {
        dispositivo.Id,
        dispositivo.Nombre,
        Tipo = dispositivo.GetType().Name,
        LatenciaMs = dispositivo.latenciaMs
    })));

app.MapPost("/api/dispositivos", (DispositivoCrearActualizar request, DispositivoService service) =>
{
    var error = ValidarDispositivo(request);
    if (error is not null)
        return Results.BadRequest(new { error });

    var tipo = CanonicalizarTipo(request.Tipo!)!;
    var nombre = request.Nombre!.Trim();
    var id = service.Registrar(nombre, tipo, request.LatenciaMs);
    return Results.Created($"/api/dispositivos/{id}", new { Id = id, Nombre = nombre, Tipo = tipo, request.LatenciaMs });
});

app.MapPut("/api/dispositivos/{id:int}", (int id, DispositivoCrearActualizar request, DispositivoService service) =>
{
    var error = ValidarDispositivo(request);
    if (error is not null)
        return Results.BadRequest(new { error });

    var tipo = CanonicalizarTipo(request.Tipo!)!;
    var nombre = request.Nombre!.Trim();
    if (!service.Actualizar(id, nombre, tipo, request.LatenciaMs))
        return Results.NotFound(new { error = $"No existe el dispositivo con id {id}." });

    return Results.Ok(new { Id = id, Nombre = nombre, Tipo = tipo, request.LatenciaMs });
});

app.MapDelete("/api/dispositivos/{id:int}", (int id, DispositivoService service) =>
{
    var resultado = service.Eliminar(id);
    return resultado switch
    {
        ResultadoEliminacionDispositivo.Eliminado => Results.NoContent(),
        ResultadoEliminacionDispositivo.EnUso => Results.Conflict(new { error = "El dispositivo está referenciado por un recorrido guardado." }),
        _ => Results.NotFound(new { error = $"No existe el dispositivo con id {id}." })
    };
});

app.MapPost("/api/simulaciones", (SimulacionCrearActualizar request, SimulacionService service) =>
{
    var error = ValidarSimulacion(request);
    if (error is not null)
        return Results.BadRequest(new { error });

    var simulacion = service.Ejecutar(
        request.Origen!.Trim(), request.Destino!.Trim(), request.TamanioBytes, request.Ttl, request.DispositivoIds);
    if (simulacion is null)
        return Results.BadRequest(new { error = "Uno o más dispositivos del recorrido no existen." });

    return Results.Created($"/api/simulaciones/{simulacion.Id}", new
    {
        simulacion.Id,
        simulacion.Estado,
        simulacion.TtlFinal,
        simulacion.LatenciaTotalMs
    });
});

app.MapGet("/api/simulaciones/{id:int}", (int id, SimulacionService service) =>
{
    var simulacion = service.Obtener(id);
    return simulacion is null
        ? Results.NotFound(new { error = $"No existe la simulación con id {id}." })
        : Results.Ok(simulacion);
});

app.MapGet("/api/simulaciones", (SimulacionService service) => Results.Ok(service.Listar()));

app.MapPut("/api/simulaciones/{id:int}", (int id, SimulacionCrearActualizar request, SimulacionService service) =>
{
    var error = ValidarSimulacion(request);
    if (error is not null)
        return Results.BadRequest(new { error });
    if (service.Obtener(id) is null)
        return Results.NotFound(new { error = $"No existe la simulación con id {id}." });

    var simulacion = service.Actualizar(
        id, request.Origen!.Trim(), request.Destino!.Trim(), request.TamanioBytes, request.Ttl, request.DispositivoIds);
    if (simulacion is null)
        return Results.BadRequest(new { error = "Uno o más dispositivos del recorrido no existen." });
    return Results.Ok(simulacion);
});

app.MapDelete("/api/simulaciones/{id:int}", (int id, SimulacionService service) =>
    service.Eliminar(id)
        ? Results.NoContent()
        : Results.NotFound(new { error = $"No existe la simulación con id {id}." }));

app.MapPut("/api/simulaciones/{id:int}/pasos/{orden:int}",
    (int id, int orden, PasoActualizar request, DispositivoService dispositivoService, SimulacionService simulacionService) =>
{
    if (request is null)
        return Results.BadRequest(new { error = "El cuerpo de la solicitud es obligatorio." });
    if (request.DispositivoId <= 0)
        return Results.BadRequest(new { error = "El dispositivoId debe ser mayor que cero." });
    if (orden <= 0)
        return Results.BadRequest(new { error = "El orden debe ser mayor que cero." });

    var simulacion = simulacionService.Obtener(id);
    if (simulacion is null)
        return Results.NotFound(new { error = $"No existe la simulación con id {id}." });
    if (dispositivoService.Obtener(request.DispositivoId) is null)
        return Results.NotFound(new { error = $"No existe el dispositivo con id {request.DispositivoId}." });
    if (orden > simulacion.Recorrido.Count)
        return Results.BadRequest(new { error = "El orden del paso indicado no existe en la simulación." });

    var actualizada = simulacionService.ActualizarPaso(id, orden, request.DispositivoId);
    return actualizada is null
        ? Results.NotFound(new { error = $"No existe la simulación con id {id}." })
        : Results.Ok(actualizada);
});

app.MapDelete("/api/simulaciones/{id:int}/pasos/{orden:int}", (int id, int orden, SimulacionService service) =>
{
    if (orden <= 0)
        return Results.BadRequest(new { error = "El orden debe ser mayor que cero." });

    var simulacion = service.Obtener(id);
    if (simulacion is null)
        return Results.NotFound(new { error = $"No existe la simulación con id {id}." });
    if (orden > simulacion.Recorrido.Count)
        return Results.BadRequest(new { error = "El orden del paso indicado no existe en la simulación." });
    if (simulacion.Recorrido.Count == 1)
        return Results.BadRequest(new { error = "El recorrido debe conservar al menos un dispositivo." });

    var actualizada = service.EliminarPaso(id, orden);
    return actualizada is null
        ? Results.NotFound(new { error = $"No existe la simulación con id {id}." })
        : Results.Ok(actualizada);
});

app.MapGet("/api/estadisticas", (EstadisticaService service) => Results.Ok(service.Obtener()));

app.Run();

static string? ValidarDispositivo(DispositivoCrearActualizar? request)
{
    if (request is null)
        return "El cuerpo de la solicitud es obligatorio.";
    if (string.IsNullOrWhiteSpace(request.Nombre))
        return "El nombre es obligatorio.";
    if (string.IsNullOrWhiteSpace(request.Tipo))
        return "El tipo es obligatorio.";
    if (request.LatenciaMs < 0)
        return "La latencia debe ser mayor o igual a cero.";
    if (CanonicalizarTipo(request.Tipo) is null)
        return $"Tipo de dispositivo no soportado: {request.Tipo.Trim()}.";
    return null;
}

static string? ValidarSimulacion(SimulacionCrearActualizar? request)
{
    if (request is null)
        return "El cuerpo de la solicitud es obligatorio.";
    if (string.IsNullOrWhiteSpace(request.Origen))
        return "El origen es obligatorio.";
    if (string.IsNullOrWhiteSpace(request.Destino))
        return "El destino es obligatorio.";
    if (request.TamanioBytes <= 0)
        return "El tamaño del paquete debe ser mayor a cero.";
    if (request.Ttl <= 0)
        return "El TTL debe ser mayor a cero.";
    if (request.DispositivoIds is null || request.DispositivoIds.Count == 0)
        return "Debe indicar al menos un dispositivo en el recorrido.";
    return null;
}

static string? CanonicalizarTipo(string tipo) =>
    new[] { "Router", "Switch", "Firewall", "AccessPoint" }
        .FirstOrDefault(t => string.Equals(t, tipo.Trim(), StringComparison.OrdinalIgnoreCase));

public partial class Program { }