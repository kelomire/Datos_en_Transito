# Indagación: Minimal API, Swagger y Códigos de Estado

## 1. Cómo Minimal API vincula método HTTP, ruta, JSON y códigos de estado

En Minimal API de ASP.NET Core, la asociación entre método HTTP y ruta se hace mediante métodos como:

- `app.MapGet("/ruta", manejador)` → solicitudes **GET**
- `app.MapPost("/ruta", manejador)` → solicitudes **POST**
- `app.MapPut("/ruta", manejador)` → solicitudes **PUT**
- `app.MapDelete("/ruta", manejador)` → solicitudes **DELETE**

### Devolver JSON y establecer códigos de estado
El framework serializa automáticamente objetos a JSON (`Content-Type: application/json`) cuando devolves un tipo de dato o usas ayudas:

```csharp
// 200 OK con JSON implícito
app.MapGet("/productos", () => new { Id = 1, Nombre = "Bicicleta" });

// Con ayudas explícitas (recomendado: TypedResults)
app.MapGet("/productos/{id:int}", (int id) =>
{
    var prod = BuscarProducto(id);
    return prod is not null
        ? TypedResults.Ok(prod)          // 200 OK + JSON
        : TypedResults.NotFound();       // 404 Not Found
});

// 201 Created al crear recurso
app.MapPost("/productos", (Producto p) =>
{
    var creado = Crear(p);
    return TypedResults.Created($"/productos/{creado.Id}", creado);
});



| Columna 1 | Columna 2 | Columna 3 |
| --- | --- | --- |
| Fila 1, Dato 1 | Fila 1, Dato 2 | Fila 1, Dato 3 |
| Fila 2, Dato 1 | Fila 2, Dato 2 | Fila 2, Dato 3 |
