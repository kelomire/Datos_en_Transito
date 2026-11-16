# Guía de uso de la API después de conectarla a la base de datos

Esta guía explica cómo usar la API ya levantada y conectada a MySQL, desde abrir Swagger hasta probar los endpoints y verificar que todo funcione correctamente.

## 1. Asegurarse de que la API esté levantada

Primero, abrí una terminal en la raíz del proyecto:

```bash
cd /home/et12alumno/Datos_en_Transito
```

Luego configurá las variables de conexión y arrancá la API:

```bash
export DB_HOST=localhost
export DB_PORT=3306
export DB_NAME=datos_transito
export DB_USER_ADMIN=5to_agbd
export DB_USER_READONLY=5to_agbd
export DB_PASSWORD_ADMIN='Trigg3rs!'
export DB_PASSWORD_READONLY='Trigg3rs!'
dotnet run --project src/API/API.csproj --urls http://localhost:8080
```

Si todo está bien, verás algo como:

```text
Now listening on: http://localhost:8080
Application started. Press Ctrl+C to shut down.
```

## 2. Abrir Swagger

En el navegador, ingresá:

```text
http://localhost:8080/swagger
```

Eso abre la interfaz Swagger, donde se muestran todos los endpoints disponibles.

## 3. Probar el endpoint principal

En Swagger, podés probar directamente el endpoint:

```text
GET /api/dispositivos
```

Esto devuelve la lista de dispositivos almacenados en la base `datos_transito`.

### Resultado esperado

Debe responder con un JSON como este:

```json
[
  {"id":1,"nombre":"router 1","tipo":"router","latenciaMs":10},
  {"id":2,"nombre":"switch 1","tipo":"switch","latenciaMs":5},
  {"id":3,"nombre":"firewall 1","tipo":"firewall","latenciaMs":15}
]
```

Si te devuelve un `200 OK`, la API está conectada y leyendo correctamente la base.

## 4. Crear un dispositivo

Usá el endpoint:

```text
POST /api/dispositivos
```

Con este cuerpo JSON:

```json
{
  "nombre": "router-demo",
  "tipo": "Router",
  "latenciaMs": 2
}
```

### Validaciones

- `nombre` no puede quedar vacío
- `tipo` debe ser uno de:
  - `Router`
  - `Switch`
  - `Firewall`
  - `AccessPoint`
- `latenciaMs` debe ser mayor o igual a 0

### Respuesta esperada

La API devuelve el dispositivo creado con su nuevo `id`.

## 5. Actualizar un dispositivo

Usá:

```text
PUT /api/dispositivos/{id}
```

Ejemplo:

```json
{
  "nombre": "router-demo-updated",
  "tipo": "Router",
  "latenciaMs": 5
}
```

Si el `id` existe, la API actualiza el registro y devuelve la información nueva.

## 6. Eliminar un dispositivo

Usá:

```text
DELETE /api/dispositivos/{id}
```

Si el dispositivo no existe, devuelve `404`.

Si está referenciado en una simulación, devuelve `409`.

## 7. Crear una simulación

Usá el endpoint:

```text
POST /api/simulaciones
```

Con un cuerpo como este:

```json
{
  "origen": "10.0.0.1",
  "destino": "10.0.0.2",
  "tamanioBytes": 1200,
  "ttl": 5,
  "dispositivoIds": [1, 2, 3]
}
```

### Validaciones

- `origen` y `destino` no pueden estar vacíos
- `tamanioBytes` debe ser positivo
- `ttl` debe ser válido
- `dispositivoIds` debe incluir dispositivos reales

La API calcula el recorrido y guarda la simulación en la base.

## 8. Ver simulaciones

Podés consultar:

```text
GET /api/simulaciones
```

para listar las simulaciones creadas.

Y también:

```text
GET /api/simulaciones/{id}
```

para consultar una simulación específica con su recorrido.

## 9. Modificar una simulación

Usá:

```text
PUT /api/simulaciones/{id}
```

con el mismo formato del `POST` anterior. Esto recalcula la simulación y vuelve a guardar el recorrido.

## 10. Modificar o eliminar un paso del recorrido

Endpoints disponibles:

```text
PUT /api/simulaciones/{id}/pasos/{orden}
DELETE /api/simulaciones/{id}/pasos/{orden}
```

Estos permiten cambiar o quitar un paso del recorrido, y la API recalcula la simulación completa.

## 11. Eliminar una simulación

Usá:

```text
DELETE /api/simulaciones/{id}
```

Esto elimina también su recorrido asociado.

## 12. Verificar que todo está funcionando

El flujo recomendado para probar la API es:

1. `GET /api/dispositivos`
2. `POST /api/dispositivos`
3. `PUT /api/dispositivos/{id}`
4. `POST /api/simulaciones`
5. `GET /api/simulaciones`
6. `GET /api/simulaciones/{id}`
7. `PUT /api/simulaciones/{id}`
8. `PUT /api/simulaciones/{id}/pasos/{orden}`
9. `DELETE /api/simulaciones/{id}/pasos/{orden}`
10. `DELETE /api/simulaciones/{id}`
11. `DELETE /api/dispositivos/{id}`

## 13. Si aparece un error

### Error 500

Esto normalmente indica que:

- la conexión a MySQL falló,
- la base no existe,
- la cuenta no tiene acceso,
- o el puerto 8080 está ocupado.

### Error 404

La entidad no existe.

### Error 409

El dispositivo está siendo usado por una simulación y no puede eliminarse.

### Error 400

Los datos enviados no tienen el formato o validación esperados.

## 14. Archivos relevantes

- [src/API/Program.cs](../src/API/Program.cs)
- [src/Aplicacion/Servicios](../src/Aplicacion/Servicios)
- [src/Persistencia/Repositorios](../src/Persistencia/Repositorios)
- [src/Persistencia/MySqlConnectionFactory.cs](../src/Persistencia/MySqlConnectionFactory.cs)
- [Docs/guia-ejecucion-api.md](./guia-ejecucion-api.md)

## 15. Resultado esperado final

Cuando la API está conectada y funcionando, podés:

- listar dispositivos,
- crear y modificar dispositivos,
- crear simulaciones,
- consultar recorridos,
- editar o eliminar pasos,
- y ver todo reflejado en la base `datos_transito`.
