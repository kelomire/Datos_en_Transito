using API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using FromBodyAttribute = Microsoft.AspNetCore.Mvc.FromBodyAttribute;

var builder = WebApplication.CreateBuilder(args);

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

var Dispositivo = new List<Dispositivo>();

app.MapGet("api/dispositivo", () =>
{
    return Results.Ok(Dispositivo);
});

app.MapPost("api/dispositivo", ([FromBody] Dispositivo dispositivo) =>
{
    Dispositivo.Add(dispositivo);
    return Results.Created($"api/dispositivo/{dispositivo.Id}", dispositivo);
});