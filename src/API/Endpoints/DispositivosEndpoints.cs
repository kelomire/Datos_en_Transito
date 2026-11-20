using static API.Validacion.FiltrosValidacion;
using Aplicacion.Servicios;
using API.DTOs;
using Datos_en_Transito.Persistencia;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Persistencia.Interfaces;
using Persistencia.Repositorios;

namespace API.Endpoints;

public static class DispositivosEndpoints
{
    public static void MapDispositivosEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dispositivos", (DispositivoService service) => Results.Ok(service.Listar().Select(d =>
        new { d.Id, d.Nombre, Tipo = d.GetType().Name, LatenciaMs = d.latenciaMs })))
        .WithTags("Dispositivos").WithSummary("Listar dispositivos")
        .WithDescription("Devuelve todos los dispositivos. Ejemplo: `curl http://localhost:8080/api/dispositivos`");

        app.MapPost("/api/dispositivos", (DispositivoCrearActualizar request, DispositivoService service) =>
        {
            var error = ValidarDispositivo(request); if (error is not null) return Results.BadRequest(new { error });
            var tipo = CanonicalizarTipo(request.Tipo!)!; var nombre = request.Nombre!.Trim();
            var id = service.Registrar(nombre, tipo, request.LatenciaMs);
            return Results.Created($"/api/dispositivos/{id}", new { Id = id, Nombre = nombre, Tipo = tipo, request.LatenciaMs });
        })
        .WithTags("Dispositivos").WithSummary("Crear dispositivo")
        .WithDescription("Crea un dispositivo.\n\nCuerpo JSON:\n```json\n{\"nombre\":\"Router principal\",\"tipo\":\"Router\",\"latenciaMs\":10}\n```\n\nEjemplo:\n```bash\ncurl -X POST http://localhost:8080/api/dispositivos -H \"Content-Type: application/json\" -d '{\"nombre\":\"Router principal\",\"tipo\":\"Router\",\"latenciaMs\":10}'\n```");

        app.MapPut("/api/dispositivos/{id:int}", (int id, DispositivoCrearActualizar request, DispositivoService service) =>
        {
            var error = ValidarDispositivo(request); if (error is not null) return Results.BadRequest(new { error });
            var tipo = CanonicalizarTipo(request.Tipo!)!; var nombre = request.Nombre!.Trim();
            if (!service.Actualizar(id, nombre, tipo, request.LatenciaMs)) return Results.NotFound(new { error = $"No existe el dispositivo con id {id}." });
            return Results.Ok(new { Id = id, Nombre = nombre, Tipo = tipo, request.LatenciaMs });
        })
        .WithTags("Dispositivos").WithSummary("Actualizar dispositivo")
        .WithDescription("Actualiza el dispositivo indicado por `id`. Ejemplo:\n```bash\ncurl -X PUT http://localhost:8080/api/dispositivos/1 -H \"Content-Type: application/json\" -d '{\"nombre\":\"Router actualizado\",\"tipo\":\"Router\",\"latenciaMs\":8}'\n```\nCuerpo JSON:\n```json\n{\"nombre\":\"Router actualizado\",\"tipo\":\"Router\",\"latenciaMs\":8}\n```");

        app.MapDelete("/api/dispositivos/{id:int}", (int id, DispositivoService service) =>
        {
            var resultado = service.Eliminar(id);
            return resultado switch
            {
                ResultadoEliminacionDispositivo.Eliminado => Results.NoContent(),
                ResultadoEliminacionDispositivo.EnUso => Results.Conflict(new { error = "El dispositivo está referenciado por un recorrido guardado." }),
                _ => Results.NotFound(new { error = $"No existe el dispositivo con id {id}." })
            };
        })
        .WithTags("Dispositivos").WithSummary("Eliminar dispositivo")
        .WithDescription("Elimina el dispositivo indicado por `id`. Ejemplo:\n```bash\ncurl -X DELETE -i http://localhost:8080/api/dispositivos/1\n```");

        static string? ValidarDispositivo(DispositivoCrearActualizar? request)
        {
            if (request is null) return "El cuerpo de la solicitud es obligatorio.";
            if (string.IsNullOrWhiteSpace(request.Nombre)) return "El nombre es obligatorio.";
            if (string.IsNullOrWhiteSpace(request.Tipo)) return "El tipo es obligatorio.";
            if (request.LatenciaMs < 0) return "La latencia debe ser mayor o igual a cero.";
            if (CanonicalizarTipo(request.Tipo) is null) return $"Tipo de dispositivo no soportado: {request.Tipo.Trim()}.";
            return null;
        }
    }
}