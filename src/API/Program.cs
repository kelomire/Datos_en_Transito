using Datos_en_Transito.Api.DTOs;
using Datos_en_Transito.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Datos en Tránsito API",
        Version = "v1",
        Description = "API para gestión de viajes y transporte"
    });
});

builder.Services.AddScoped<IViajeService, ViajeService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.MapGet("/api/viajes", (IViajeService svc) =>
{
    var viajes = svc.ListarTodos();
    return Results.Ok(viajes); // 200 OK + JSON
})
.WithName("ListarViajes")
.WithSummary("Devuelve todos los viajes registrados")
.Produces<IEnumerable<ViajeRespuesta>>(200);

app.MapGet("/api/viajes/{id:int}", (int id, IViajeService svc) =>
{
    var viaje = svc.BuscarPorId(id);
    return viaje is not null
        ? Results.Ok(viaje)      // 200 OK
        : Results.NotFound();   // 404 No encontrado
})
.WithName("ObtenerViaje")
.WithSummary("Busca un viaje por su ID")
.Produces<ViajeRespuesta>(200)
.Produces(404);

app.MapPost("/api/viajes", (CrearViajeSolicitud dto, IViajeService svc) =>
{
    if (!dto.EsValido())
        return Results.BadRequest("Datos inválidos"); // 400

    var creado = svc.Crear(dto);
    return Results.Created($"/api/viajes/{creado.Id}", creado); // 201 ✅
})
.WithName("CrearViaje")
.WithSummary("Registra un nuevo viaje")
.Produces<ViajeRespuesta>(201)
.Produces(400);

app.Run();
