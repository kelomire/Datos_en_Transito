using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Datos en tránsito API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Datos en tránsito API v1");
    c.RoutePrefix = "swagger";
});

app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html"));
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.MapGet("/api/dispositivos", () =>
{
    return Results.Ok(ApiStore.Dispositivos.Select(d => new
    {
        d.Id,
        d.Nombre,
        d.Tipo,
        d.LatenciaMs
    }));
});

app.MapPost("/api/dispositivos", (DispositivoCrearActualizar request) =>
{
    if (request is null)
        return Results.BadRequest(new { error = "El cuerpo de la solicitud es obligatorio." });

    if (string.IsNullOrWhiteSpace(request.Nombre))
        return Results.BadRequest(new { error = "El nombre es obligatorio." });

    if (string.IsNullOrWhiteSpace(request.Tipo))
        return Results.BadRequest(new { error = "El tipo es obligatorio." });

    if (request.LatenciaMs < 0)
        return Results.BadRequest(new { error = "La latencia debe ser mayor o igual a cero." });

    var tipo = request.Tipo.Trim();
    if (!ApiStore.TiposValidos.Contains(tipo))
        return Results.BadRequest(new { error = $"Tipo de dispositivo no soportado: {tipo}." });

    var dispositivo = new Dispositivo
    {
        Id = ApiStore.ProximoIdDispositivo(),
        Nombre = request.Nombre.Trim(),
        Tipo = tipo,
        LatenciaMs = request.LatenciaMs
    };

    ApiStore.Dispositivos.Add(dispositivo);

    var respuesta = new
    {
        dispositivo.Id,
        dispositivo.Nombre,
        dispositivo.Tipo,
        dispositivo.LatenciaMs
    };

    return Results.Created($"/api/dispositivos/{dispositivo.Id}", respuesta);
});

app.MapPut("/api/dispositivos/{id:int}", (int id, DispositivoCrearActualizar request) =>
{
    if (request is null)
        return Results.BadRequest(new { error = "El cuerpo de la solicitud es obligatorio." });

    if (string.IsNullOrWhiteSpace(request.Nombre))
        return Results.BadRequest(new { error = "El nombre es obligatorio." });

    if (string.IsNullOrWhiteSpace(request.Tipo))
        return Results.BadRequest(new { error = "El tipo es obligatorio." });

    if (request.LatenciaMs < 0)
        return Results.BadRequest(new { error = "La latencia debe ser mayor o igual a cero." });

    var dispositivo = ApiStore.Dispositivos.FirstOrDefault(d => d.Id == id);
    if (dispositivo is null)
        return Results.NotFound(new { error = $"No existe el dispositivo con id {id}." });

    var tipo = request.Tipo.Trim();
    if (!ApiStore.TiposValidos.Contains(tipo))
        return Results.BadRequest(new { error = $"Tipo de dispositivo no soportado: {tipo}." });

    dispositivo.Nombre = request.Nombre.Trim();
    dispositivo.Tipo = tipo;
    dispositivo.LatenciaMs = request.LatenciaMs;

    return Results.Ok(new
    {
        dispositivo.Id,
        dispositivo.Nombre,
        dispositivo.Tipo,
        dispositivo.LatenciaMs
    });
});

app.MapDelete("/api/dispositivos/{id:int}", (int id) =>
{
    var dispositivo = ApiStore.Dispositivos.FirstOrDefault(d => d.Id == id);
    if (dispositivo is null)
        return Results.NotFound(new { error = $"No existe el dispositivo con id {id}." });

    var estaReferenciado = ApiStore.Simulaciones.Any(s => s.Recorrido.Any(p => p.DispositivoId == id));
    if (estaReferenciado)
        return Results.Conflict(new { error = "El dispositivo está referenciado por un recorrido guardado." });

    ApiStore.Dispositivos.Remove(dispositivo);
    return Results.NoContent();
});

app.MapPost("/api/simulaciones", (SimulacionCrearActualizar request) =>
{
    if (request is null)
        return Results.BadRequest(new { error = "El cuerpo de la solicitud es obligatorio." });

    if (string.IsNullOrWhiteSpace(request.Origen))
        return Results.BadRequest(new { error = "El origen es obligatorio." });

    if (string.IsNullOrWhiteSpace(request.Destino))
        return Results.BadRequest(new { error = "El destino es obligatorio." });

    if (request.TamanioBytes <= 0)
        return Results.BadRequest(new { error = "El tamaño del paquete debe ser mayor a cero." });

    if (request.Ttl <= 0)
        return Results.BadRequest(new { error = "El TTL debe ser mayor a cero." });

    if (request.DispositivoIds is null || request.DispositivoIds.Count == 0)
        return Results.BadRequest(new { error = "Debe indicar al menos un dispositivo en el recorrido." });

    foreach (var dispositivoId in request.DispositivoIds)
    {
        if (!ApiStore.Dispositivos.Any(d => d.Id == dispositivoId))
            return Results.BadRequest(new { error = $"No existe el dispositivo con id {dispositivoId}." });
    }

    var simulacion = ApiStore.EjecutarSimulacion(
        request.Origen.Trim(),
        request.Destino.Trim(),
        request.TamanioBytes,
        request.Ttl,
        request.DispositivoIds.ToList());

    var respuesta = new
    {
        simulacion.Id,
        simulacion.Estado,
        simulacion.TtlFinal,
        simulacion.LatenciaTotalMs
    };

    return Results.Created($"/api/simulaciones/{simulacion.Id}", respuesta);
});

app.MapGet("/api/simulaciones/{id:int}", (int id) =>
{
    var simulacion = ApiStore.Simulaciones.FirstOrDefault(s => s.Id == id);
    if (simulacion is null)
        return Results.NotFound(new { error = $"No existe la simulación con id {id}." });

    var respuesta = new
    {
        simulacion.Id,
        simulacion.Origen,
        simulacion.Destino,
        simulacion.TamanioBytes,
        simulacion.Ttl,
        simulacion.Estado,
        simulacion.TtlFinal,
        simulacion.LatenciaTotalMs,
        Recorrido = simulacion.Recorrido.Select(p => new
        {
            p.Orden,
            p.DispositivoId,
            p.Nombre,
            p.Tipo,
            p.LatenciaAplicadaMs,
            p.TtlRestante
        })
    };

    return Results.Ok(respuesta);
});

app.MapGet("/api/simulaciones", () =>
{
    var respuesta = ApiStore.Simulaciones.Select(s => new
    {
        s.Id,
        s.Estado,
        s.TtlFinal,
        s.LatenciaTotalMs
    });

    return Results.Ok(respuesta);
});

app.MapPut("/api/simulaciones/{id:int}", (int id, SimulacionCrearActualizar request) =>
{
    if (request is null)
        return Results.BadRequest(new { error = "El cuerpo de la solicitud es obligatorio." });

    if (string.IsNullOrWhiteSpace(request.Origen))
        return Results.BadRequest(new { error = "El origen es obligatorio." });

    if (string.IsNullOrWhiteSpace(request.Destino))
        return Results.BadRequest(new { error = "El destino es obligatorio." });

    if (request.TamanioBytes <= 0)
        return Results.BadRequest(new { error = "El tamaño del paquete debe ser mayor a cero." });

    if (request.Ttl <= 0)
        return Results.BadRequest(new { error = "El TTL debe ser mayor a cero." });

    if (request.DispositivoIds is null || request.DispositivoIds.Count == 0)
        return Results.BadRequest(new { error = "Debe indicar al menos un dispositivo en el recorrido." });

    var simulacion = ApiStore.Simulaciones.FirstOrDefault(s => s.Id == id);
    if (simulacion is null)
        return Results.NotFound(new { error = $"No existe la simulación con id {id}." });

    foreach (var dispositivoId in request.DispositivoIds)
    {
        if (!ApiStore.Dispositivos.Any(d => d.Id == dispositivoId))
            return Results.BadRequest(new { error = $"No existe el dispositivo con id {dispositivoId}." });
    }

    var actualizada = ApiStore.EjecutarSimulacion(
        request.Origen.Trim(),
        request.Destino.Trim(),
        request.TamanioBytes,
        request.Ttl,
        request.DispositivoIds.ToList(),
        id);

    return Results.Ok(new
    {
        actualizada.Id,
        actualizada.Origen,
        actualizada.Destino,
        actualizada.TamanioBytes,
        actualizada.Ttl,
        actualizada.Estado,
        actualizada.TtlFinal,
        actualizada.LatenciaTotalMs,
        Recorrido = actualizada.Recorrido.Select(p => new
        {
            p.Orden,
            p.DispositivoId,
            p.Nombre,
            p.Tipo,
            p.LatenciaAplicadaMs,
            p.TtlRestante
        })
    });
});

app.MapDelete("/api/simulaciones/{id:int}", (int id) =>
{
    var simulacion = ApiStore.Simulaciones.FirstOrDefault(s => s.Id == id);
    if (simulacion is null)
        return Results.NotFound(new { error = $"No existe la simulación con id {id}." });

    ApiStore.Simulaciones.Remove(simulacion);
    return Results.NoContent();
});

app.MapPut("/api/simulaciones/{id:int}/pasos/{orden:int}", (int id, int orden, PasoActualizar request) =>
{
    if (request is null)
        return Results.BadRequest(new { error = "El cuerpo de la solicitud es obligatorio." });

    if (request.DispositivoId <= 0)
        return Results.BadRequest(new { error = "El dispositivoId debe ser mayor que cero." });

    if (orden <= 0)
        return Results.BadRequest(new { error = "El orden debe ser mayor que cero." });

    var simulacion = ApiStore.Simulaciones.FirstOrDefault(s => s.Id == id);
    if (simulacion is null)
        return Results.NotFound(new { error = $"No existe la simulación con id {id}." });

    var dispositivo = ApiStore.Dispositivos.FirstOrDefault(d => d.Id == request.DispositivoId);
    if (dispositivo is null)
        return Results.NotFound(new { error = $"No existe el dispositivo con id {request.DispositivoId}." });

    if (orden > simulacion.Recorrido.Count)
        return Results.BadRequest(new { error = "El orden del paso indicado no existe en la simulación." });

    var listaIds = simulacion.Recorrido.Select(p => p.DispositivoId).ToList();
    listaIds[orden - 1] = request.DispositivoId;
    var nueva = ApiStore.EjecutarSimulacion(
        simulacion.Origen,
        simulacion.Destino,
        simulacion.TamanioBytes,
        simulacion.Ttl,
        listaIds,
        id);

    return Results.Ok(new
    {
        nueva.Id,
        nueva.Origen,
        nueva.Destino,
        nueva.TamanioBytes,
        nueva.Ttl,
        nueva.Estado,
        nueva.TtlFinal,
        nueva.LatenciaTotalMs,
        Recorrido = nueva.Recorrido.Select(p => new
        {
            p.Orden,
            p.DispositivoId,
            p.Nombre,
            p.Tipo,
            p.LatenciaAplicadaMs,
            p.TtlRestante
        })
    });
});

app.Run();

public partial class Program
{
}

static class ApiStore
{
    public static readonly HashSet<string> TiposValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Router",
        "Switch",
        "Firewall",
        "AccessPoint"
    };

    public static List<Dispositivo> Dispositivos { get; } = new();
    public static List<SimulacionPersistida> Simulaciones { get; } = new();

    public static int ProximoIdDispositivo() => Dispositivos.Count == 0 ? 1 : Dispositivos.Max(d => d.Id) + 1;
    public static int ProximoIdSimulacion() => Simulaciones.Count == 0 ? 1 : Simulaciones.Max(s => s.Id) + 1;

    public static SimulacionPersistida EjecutarSimulacion(string origen, string destino, int tamanioBytes, int ttl, List<int> dispositivoIds, int? id = null)
    {
        var simulacion = id is null
            ? new SimulacionPersistida { Id = ProximoIdSimulacion(), Origen = origen, Destino = destino, TamanioBytes = tamanioBytes, Ttl = ttl }
            : Simulaciones.First(s => s.Id == id.Value);

        simulacion.Origen = origen;
        simulacion.Destino = destino;
        simulacion.TamanioBytes = tamanioBytes;
        simulacion.Ttl = ttl;

        var ttlActual = ttl;
        var latenciaTotal = 0;
        var recorrido = new List<PasoRecorrido>();
        var descartado = false;

        for (var index = 0; index < dispositivoIds.Count; index++)
        {
            var dispositivoId = dispositivoIds[index];
            var dispositivo = Dispositivos.First(d => d.Id == dispositivoId);
            var latencia = dispositivo.LatenciaMs;
            var tipo = dispositivo.Tipo ?? string.Empty;

            if (tipo == "Firewall" && tamanioBytes > 1500)
            {
                descartado = true;
            }
            else if (tipo is "Router" or "AccessPoint")
            {
                ttlActual--;
                if (ttlActual <= 0)
                    descartado = true;
            }

            latenciaTotal += latencia;
            recorrido.Add(new PasoRecorrido
            {
                Orden = index + 1,
                DispositivoId = dispositivoId,
                Nombre = dispositivo.Nombre ?? string.Empty,
                Tipo = tipo,
                LatenciaAplicadaMs = latencia,
                TtlRestante = ttlActual
            });

            if (descartado)
                break;
        }

        simulacion.Recorrido = recorrido;
        simulacion.TtlFinal = ttlActual;
        simulacion.LatenciaTotalMs = latenciaTotal;
        simulacion.Estado = descartado ? "descartado" : "entregado";

        if (id is null)
            Simulaciones.Add(simulacion);

        return simulacion;
    }
}

class SimulacionPersistida
{
    public int Id { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public int TamanioBytes { get; set; }
    public int Ttl { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int TtlFinal { get; set; }
    public int LatenciaTotalMs { get; set; }
    public List<PasoRecorrido> Recorrido { get; set; } = new();
}

class PasoRecorrido
{
    public int Orden { get; set; }
    public int DispositivoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int LatenciaAplicadaMs { get; set; }
    public int TtlRestante { get; set; }
}
