using Aplicacion.Servicios;
using API.DTOs;
using Datos_en_Transito.Persistencia;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Persistencia.Interfaces;
using Persistencia.Repositorios;

namespace API.Endpoints;

public static class SimulacionEndpoints
{
    public static void MapSimulacionEndpoints(this ISimulacionRepository app)
    {
        app.MapPost("/api/simulaciones", (SimulacionCrearActualizar request, SimulacionService service) =>
        {
            var error = ValidarSimulacion(request); if (error is not null) return Results.BadRequest(new { error });
            var simulacion = service.Ejecutar(request.Origen!.Trim(), request.Destino!.Trim(), request.TamanioBytes, request.Ttl, request.DispositivoIds);
            return simulacion is null
            ? Results.BadRequest(new { error = "Uno o más dispositivos del recorrido no existen." })
            : Results.Created($"/api/simulaciones/{simulacion.Id}", new { simulacion.Id, simulacion.Estado, simulacion.TtlFinal, simulacion.LatenciaTotalMs });
        })
        .WithTags("Simulaciones").WithSummary("Crear y ejecutar simulación")
        .WithDescription("Ejecuta una simulación. Cuerpo JSON:\n```json\n{\"origen\":\"PC-Cliente\",\"destino\":\"Servidor-Web\",\"tamanioBytes\":1500,\"ttl\":64,\"dispositivoIds\":[1,2,3]}\n```\nEjemplo:\n```bash\ncurl -X POST http://localhost:8080/api/simulaciones -H \"Content-Type: application/json\" -d '{\"origen\":\"PC-Cliente\",\"destino\":\"Servidor-Web\",\"tamanioBytes\":1500,\"ttl\":64,\"dispositivoIds\":[1,2,3]}'\n```");

        app.MapGet("/api/simulaciones/{id:int}", (int id, SimulacionService service) =>
        {
            var simulacion = service.Obtener(id);
            return simulacion is null ? Results.NotFound(new { error = $"No existe la simulación con id {id}." }) : Results.Ok(simulacion);
        })
        .WithTags("Simulaciones").WithSummary("Obtener simulación")
        .WithDescription("Obtiene una simulación por `id`. Ejemplo: `curl http://localhost:8080/api/simulaciones/1`");

        app.MapGet("/api/simulaciones", (SimulacionService service) => Results.Ok(service.Listar()))
            .WithTags("Simulaciones").WithSummary("Listar simulaciones")
            .WithDescription("Devuelve todas las simulaciones. Ejemplo: `curl http://localhost:8080/api/simulaciones`");

        app.MapPut("/api/simulaciones/{id:int}", (int id, SimulacionCrearActualizar request, SimulacionService service) =>
        {
            var error = ValidarSimulacion(request); if (error is not null) return Results.BadRequest(new { error });
            if (service.Obtener(id) is null) return Results.NotFound(new { error = $"No existe la simulación con id {id}." });
            var simulacion = service.Actualizar(id, request.Origen!.Trim(), request.Destino!.Trim(), request.TamanioBytes, request.Ttl, request.DispositivoIds);
            return simulacion is null ? Results.BadRequest(new { error = "Uno o más dispositivos del recorrido no existen." }) : Results.Ok(simulacion);
        })
        .WithTags("Simulaciones").WithSummary("Actualizar simulación")
        .WithDescription("Actualiza una simulación existente. Cuerpo JSON:\n```json\n{\"origen\":\"PC-Cliente\",\"destino\":\"Servidor-Actualizado\",\"tamanioBytes\":1500,\"ttl\":64,\"dispositivoIds\":[1,2]}\n```\nEjemplo:\n```bash\ncurl -X PUT http://localhost:8080/api/simulaciones/1 -H \"Content-Type: application/json\" -d '{\"origen\":\"PC-Cliente\",\"destino\":\"Servidor-Actualizado\",\"tamanioBytes\":1500,\"ttl\":64,\"dispositivoIds\":[1,2]} '\n```");

        app.MapDelete("/api/simulaciones/{id:int}", (int id, SimulacionService service) => service.Eliminar(id) ? Results.NoContent() : Results.NotFound(new { error = $"No existe la simulación con id {id}." }))
            .WithTags("Simulaciones").WithSummary("Eliminar simulación")
            .WithDescription("Elimina una simulación por `id`. Ejemplo: `curl -X DELETE -i http://localhost:8080/api/simulaciones/1`");

        app.MapPut("/api/simulaciones/{id:int}/pasos/{orden:int}", (int id, int orden, PasoActualizar request, DispositivoService dispositivoService, SimulacionService simulacionService) =>
        {
            if (request is null) return Results.BadRequest(new { error = "El cuerpo de la solicitud es obligatorio." });
            if (request.DispositivoId <= 0) return Results.BadRequest(new { error = "El dispositivoId debe ser mayor que cero." });
            if (orden <= 0) return Results.BadRequest(new { error = "El orden debe ser mayor que cero." });
            var simulacion = simulacionService.Obtener(id); if (simulacion is null) return Results.NotFound(new { error = $"No existe la simulación con id {id}." });
            if (dispositivoService.Obtener(request.DispositivoId) is null) return Results.NotFound(new { error = $"No existe el dispositivo con id {request.DispositivoId}." });
            if (orden > simulacion.Recorrido.Count) return Results.BadRequest(new { error = "El orden del paso indicado no existe en la simulación." });
            var actualizada = simulacionService.ActualizarPaso(id, orden, request.DispositivoId);
            return actualizada is null ? Results.NotFound(new { error = $"No existe la simulación con id {id}." }) : Results.Ok(actualizada);
        })
        .WithTags("Pasos").WithSummary("Actualizar paso")
        .WithDescription("Cambia el dispositivo de un paso. Cuerpo JSON: ```json\n{\"dispositivoId\":3}\n``` Ejemplo: ```bash\ncurl -X PUT http://localhost:8080/api/simulaciones/1/pasos/2 -H \"Content-Type: application/json\" -d '{\"dispositivoId\":3}'\n```");

        app.MapDelete("/api/simulaciones/{id:int}/pasos/{orden:int}", (int id, int orden, SimulacionService service) =>
        {
            if (orden <= 0) return Results.BadRequest(new { error = "El orden debe ser mayor que cero." });
            var simulacion = service.Obtener(id); if (simulacion is null) return Results.NotFound(new { error = $"No existe la simulación con id {id}." });
            if (orden > simulacion.Recorrido.Count) return Results.BadRequest(new { error = "El orden del paso indicado no existe en la simulación." });
            if (simulacion.Recorrido.Count == 1) return Results.BadRequest(new { error = "El recorrido debe conservar al menos un dispositivo." });
            var actualizada = service.EliminarPaso(id, orden);
            return actualizada is null ? Results.NotFound(new { error = $"No existe la simulación con id {id}." }) : Results.Ok(actualizada);
        })
        .WithTags("Pasos").WithSummary("Eliminar paso")
        .WithDescription("Elimina un paso del recorrido. Ejemplo: `curl -X DELETE -i http://localhost:8080/api/simulaciones/1/pasos/2`");

        app.MapGet("/api/estadisticas", (EstadisticaService service) => Results.Ok(service.Obtener()))
            .WithTags("Estadísticas").WithSummary("Obtener estadísticas")
            .WithDescription("Devuelve las estadísticas generales. Ejemplo: `curl http://localhost:8080/api/estadisticas`");

        app.Run();

        static string? ValidarSimulacion(SimulacionCrearActualizar? request)
        {
            if (request is null) return "El cuerpo de la solicitud es obligatorio.";
            if (string.IsNullOrWhiteSpace(request.Origen)) return "El origen es obligatorio.";
            if (string.IsNullOrWhiteSpace(request.Destino)) return "El destino es obligatorio.";
            if (request.TamanioBytes <= 0) return "El tamaño del paquete debe ser mayor a cero.";
            if (request.Ttl <= 0) return "El TTL debe ser mayor a cero.";
            if (request.DispositivoIds is null || request.DispositivoIds.Count == 0) return "Debe indicar al menos un dispositivo en el recorrido.";
            return null;
        }
        static string? CanonicalizarTipo(string tipo) => new[] { "Router", "Switch", "Firewall", "AccessPoint" }.FirstOrDefault(t => string.Equals(t, tipo.Trim(), StringComparison.OrdinalIgnoreCase));

    }
}