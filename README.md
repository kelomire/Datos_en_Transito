# Datos en tránsito

API Minimal de .NET 10 para simular paquetes a través de routers, switches, firewalls y access points, persistir resultados en MySQL y probar las operaciones desde Swagger.

## Requisitos

- .NET SDK 10.
- MySQL 8 o posterior y el cliente de terminal `mysql`.
- Bash para los comandos de esta guía.
- Una cuenta MySQL con permisos para crear tablas y usuarios al preparar una base local. Para la API se necesitan una cuenta de escritura y otra de lectura.

La API usa Dapper y MySqlConnector; no necesita Entity Framework ni `dotnet ef migrations`.

## Crear una base local

Los nombres solicitados como `DDL.sql` y `USUARIOS.sql` corresponden en este repositorio a `scripts/01_crear_tablas.sql` y `scripts/03_crear_usuarios.sql`. Ejecutá los comandos desde la raíz del repositorio y solo en una base local nueva/vacía. El primer script crea `datos_transito`, las cuatro tablas y datos de muestra (seis dispositivos y dos simulaciones). Aunque crea tablas con `IF NOT EXISTS`, volver a ejecutarlo inserta los datos de muestra otra vez.

```bash
mysql -u root -p < scripts/01_crear_tablas.sql
mysql -u root -p < scripts/02_procedimientos_almacenados.sql
mysql -u root -p < scripts/03_crear_usuarios.sql
```

Ingresá la contraseña administrativa de MySQL cuando cada comando la solicite. `02_procedimientos_almacenados.sql` instala procedimientos auxiliares y puede omitirse para ejecutar los once endpoints: la API no los invoca. `03_crear_usuarios.sql` crea `administrador` y `desarrollo` para `localhost`; requiere permisos para crear cuentas y otorgar privilegios. El script concede permisos globales a `administrador` y contiene contraseñas de ejemplo: es solo para una instalación descartable/local. Cambiá esas contraseñas y restringí los permisos antes de cualquier uso real.

Si ya existe una base con datos, no ejecutes el DDL a ciegas: los inserts de ejemplo se duplicarían. No uses la cuenta de escritura con privilegios globales en producción.

## Restaurar el backup

La restauración es una alternativa a crear las tablas con el DDL; no ejecutes ambos flujos uno después del otro. `backup/datos-en-transito.sql` contiene tablas y datos, elimina tablas existentes antes de recrearlas y también incluye procedimientos almacenados. Usalo solamente en una base local descartable cuyo contenido se pueda reemplazar:

```bash
mysql -u root -p < backup/datos-en-transito.sql
```

El dump define sus procedimientos con el usuario `5to_agbd` como `DEFINER`. En MySQL donde esa cuenta no exista o la cuenta importadora no tenga privilegios para establecer ese definidor, la importación puede fallar en esa sección; para ese entorno, importá una copia del dump con los `DEFINER` adaptados a una cuenta local autorizada. Los procedimientos no son necesarios para la API. Nunca restaures este archivo sobre una base que quieras conservar.

Comprobá la carga con:

```bash
mysql -u administrador -p datos_transito -e "SHOW TABLES; SELECT id, nombre, tipo FROM dispositivos ORDER BY id; SELECT id, paquete_id FROM simulaciones ORDER BY id;"
```

## Configuración local

La API toma estos valores del entorno si están definidos; de lo contrario, usa host `localhost`, puerto `3306`, base `datos_transito`, usuario de escritura `administrador` y usuario de lectura `desarrollo` de `src/API/appsettings.json`. Las contraseñas son obligatorias y no se leen desde el archivo de configuración. Configurá todo en la misma terminal Bash donde iniciarás la API:

```bash
export DB_HOST=localhost
export DB_PORT=3306
export DB_NAME=datos_transito
export DB_USER_ADMIN=administrador
export DB_USER_READONLY=desarrollo
read -rsp 'Contraseña MySQL de administrador: ' DB_PASSWORD_ADMIN; printf '\n'; export DB_PASSWORD_ADMIN
read -rsp 'Contraseña MySQL de desarrollo: ' DB_PASSWORD_READONLY; printf '\n'; export DB_PASSWORD_READONLY
```

Al escribir las contraseñas no se muestran caracteres. Usá las contraseñas configuradas para las cuentas; las del script son ejemplos y no deben reutilizarse en entornos compartidos. Las variables `DER_ADMIN_PASSWORD` y `DER_INVITADO_PASSWORD` se aceptan como alternativas antiguas para las dos contraseñas. Las variables `export` duran en esa terminal; si la cerrás, volvé a configurarlas. No guardes contraseñas reales en Git ni en `appsettings*.json`.

## Iniciar la API

En la misma terminal configurada, desde la raíz del repositorio:

```bash
dotnet run --project src/API/API.csproj --urls http://localhost:8080
```

Dejá esa terminal abierta mientras probás la API. La interfaz Swagger está en [http://localhost:8080/swagger](http://localhost:8080/swagger) y el documento OpenAPI en [http://localhost:8080/swagger/v1/swagger.json](http://localhost:8080/swagger/v1/swagger.json). Si el puerto 8080 está ocupado, elegí otro en `--urls` y usá ese mismo puerto en el navegador.

## Probar los once endpoints

En Swagger, expandí una operación, seleccioná **Try it out**, completá los parámetros y el cuerpo JSON y pulsá **Execute**. Los `{id}` y `{orden}` de las rutas son valores numéricos, sin llaves. Para probar de principio a fin en la base recién inicializada, seguí este orden y usá los IDs reales que respondan las operaciones:

1. `GET /api/dispositivos`: lista los dispositivos; anotá sus IDs.
2. `POST /api/dispositivos`: creá un dispositivo de prueba con el cuerpo de abajo y anotá el `Id` de la respuesta.
3. `PUT /api/dispositivos/{id}`: actualizá el dispositivo recién creado con el mismo cuerpo.
4. `POST /api/simulaciones`: creá una simulación con al menos tres IDs de dispositivo existentes; anotá el `Id` de la respuesta.
5. `GET /api/simulaciones`: lista las simulaciones.
6. `GET /api/simulaciones/{id}`: consultá la simulación creada y verificá sus pasos.
7. `PUT /api/simulaciones/{id}`: recalculá la simulación con el cuerpo de creación.
8. `PUT /api/simulaciones/{id}/pasos/{orden}`: reemplazá un paso, por ejemplo el orden `1`, con el cuerpo de actualización de paso.
9. `DELETE /api/simulaciones/{id}/pasos/{orden}`: eliminá un paso, por ejemplo el orden `3`; el recorrido debe conservar al menos uno.
10. `DELETE /api/simulaciones/{id}`: eliminá la simulación de prueba.
11. `DELETE /api/dispositivos/{id}`: eliminá el dispositivo que creaste en el paso 2, ya sin referencias.

Cuerpo para crear o actualizar un dispositivo:

```json
{
  "nombre": "router-demo",
  "tipo": "Router",
  "latenciaMs": 2
}
```

Tipos válidos: `Router`, `Switch`, `Firewall` y `AccessPoint` (no distingue mayúsculas/minúsculas). La latencia debe ser mayor o igual a cero.

Cuerpo para crear o actualizar una simulación; cambiá los IDs por tres dispositivos existentes del `GET /api/dispositivos`:

```json
{
  "origen": "10.0.0.1",
  "destino": "10.0.0.2",
  "tamanioBytes": 1200,
  "ttl": 5,
  "dispositivoIds": [1, 2, 3]
}
```

Cuerpo para actualizar un paso:

```json
{
  "dispositivoId": 2
}
```

`orden` empieza en 1. Cambiar o eliminar un paso recalcula y persiste la simulación completa. Para comprobar validaciones, probá un tipo de dispositivo no soportado (`400`) o un ID de simulación inexistente (`404`). Un dispositivo que ya figure en un recorrido no se puede borrar (`409`).

### Rutas y conexiones MySQL

Las consultas usan la cuenta `DB_USER_READONLY`; los cambios usan `DB_USER_ADMIN`. Algunos endpoints de escritura también leen datos para validar o preparar el cambio, por lo que pueden usar ambas conexiones en una misma solicitud.

| # | Método y ruta | Operación de base de datos |
|---|---|---|
| 1 | `GET /api/dispositivos` | Lectura |
| 2 | `POST /api/dispositivos` | Escritura |
| 3 | `PUT /api/dispositivos/{id}` | Escritura (incluye la comprobación de existencia) |
| 4 | `DELETE /api/dispositivos/{id}` | Lectura para comprobar; escritura para borrar |
| 5 | `GET /api/simulaciones` | Lectura |
| 6 | `POST /api/simulaciones` | Lectura de dispositivos y escritura de simulación/recorrido |
| 7 | `GET /api/simulaciones/{id}` | Lectura |
| 8 | `PUT /api/simulaciones/{id}` | Lecturas de comprobación y escritura de la simulación/recorrido |
| 9 | `DELETE /api/simulaciones/{id}` | Escritura |
| 10 | `PUT /api/simulaciones/{id}/pasos/{orden}` | Lecturas de simulación/dispositivos y escritura del resultado recalculado |
| 11 | `DELETE /api/simulaciones/{id}/pasos/{orden}` | Lectura de simulación y escritura del resultado recalculado |

## Reglas del simulador y pruebas

- `Router` y `AccessPoint` reducen el TTL; si llega a cero, el paquete se descarta.
- `Switch` no modifica el TTL.
- `Firewall` descarta paquetes de más de 1500 bytes.
- Al descartarse el paquete, se detiene el recorrido.

Desde la raíz del repositorio, compilá y ejecutá las pruebas unitarias con:

```bash
dotnet build Proyecto.slnx
dotnet test src/Tests/Tests.csproj
```

Las pruebas unitarias cubren reglas del dominio; no reemplazan la prueba de integración contra MySQL. La configuración de usuarios y la importación de SQL requieren credenciales válidas del servidor local.
