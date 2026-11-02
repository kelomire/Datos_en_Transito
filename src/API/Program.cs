using API.DTOs;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SimulacionRed API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// POST Dispositivos — SIN WithOpenApi
app.MapPost("/api/dispositivos", (Dispositivos solicitud) =>
{
    if (string.IsNullOrWhiteSpace(solicitud.Nombre) ||
        string.IsNullOrWhiteSpace(solicitud.Tipo) ||
        solicitud.LatenciaMs < 0)
    {
        return Results.BadRequest(new { error = "Datos inválidos" });
    }

    var nuevoId = Guid.NewGuid().ToString();

    var respuesta = new
    {
        id = nuevoId,
        nombre = solicitud.Nombre,
        tipo = solicitud.Tipo,
        latenciaMsMs = solicitud.LatenciaMs
    };

    return Results.Created($"/api/dispositivos/{nuevoId}", respuesta);
})
.WithName("CrearDispositivo");

app.MapPost("/api/simulaciones", (SimulacionCrearActualizar solicitud, Almacenamiento datos) =>
{
    // Validaciones
    if (string.IsNullOrWhiteSpace(solicitud.IpOrigen) ||
        string.IsNullOrWhiteSpace(solicitud.IpDestino) ||
        solicitud.TamanioBytes <= 0 ||
        solicitud.Ttl <= 0 ||
        solicitud.DispositivoIds == null ||
        solicitud.DispositivoIds.Count == 0)
    {
        return Results.BadRequest(new { error = "Datos inválidos" });
    }

    // Verificar dispositivos
    foreach (var idDisp in solicitud.DispositivoIds)
    {
        if (!datos.Dispositivos.ContainsKey(idDisp))
            return Results.BadRequest(new { error = $"El dispositivo '{idDisp}' no existe" });
    }

    // ✅ Variables declaradas correctamente
    int latenciaTotalMs = 0;
    int ttlActual = solicitud.Ttl;
    string estado = "Completada";
    var recorrido = new List<PasoRecorrido>();

    foreach (var idDisp in solicitud.DispositivoIds)
    {
        var disp = datos.Dispositivos[idDisp];

        // ✅ Orden correcto: sumar primero
        latenciaTotalMs += disp.LatenciaMs;
        ttlActual--;

        recorrido.Add(new PasoRecorrido
        {
            Orden = recorrido.Count + 1,
            DispositivoId = idDisp,
            LatenciaAplicadaMs = disp.LatenciaMs,
            TtlRestante = ttlActual
        });

        if (ttlActual <= 0)
        {
            estado = "Interrumpida por TTL agotado";
            ttlActual = 0;
            break;
        }
    }

    // ✅ Crear simulación con Recorrido
    var nuevoId = Guid.NewGuid().ToString();
    var simulacion = new Simulacion
    {
        Id = nuevoId,
        Origen = solicitud.IpOrigen,
        Destino = solicitud.IpDestino,
        TamanioBytes = solicitud.TamanioBytes,
        Ttl = solicitud.Ttl,
        DispositivoIds = solicitud.DispositivoIds,
        Estado = estado,
        TtlFinal = ttlActual,
        LatenciaTotalMs = latenciaTotalMs,
        Recorrido = recorrido // ✅ Ahora existe la propiedad
    };

    datos.Simulaciones[nuevoId] = simulacion;

    return Results.Created($"/api/simulaciones/{nuevoId}", new
    {
        id = nuevoId,
        estado = estado,
        ttlFinal = ttlActual,
        latenciaTotalMs = latenciaTotalMs
    });
})
.WithName("CrearSimulacion");

app.Run();