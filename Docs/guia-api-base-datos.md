# Guía de uso: API y base de datos

Esta guía reúne la configuración y los pasos usados para ejecutar y demostrar Datos en Tránsito desde VS Code. La API está implementada con Minimal API en .NET 10, usa MySQL mediante Dapper y ofrece Swagger para probar las rutas.

## 1. Requisitos y terminales

- .NET SDK 10.
- MySQL 8 y el cliente de terminal `mysql`.
- Abrir una terminal **Bash** integrada en VS Code, ubicada en la raíz del repositorio.

Importante: los comandos que empiezan con `mysql ...` se escriben en Bash. Si aparece el prompt `mysql>`, ya estás dentro del cliente MySQL; ahí se escriben sentencias SQL, no comandos de shell. Para salir se usa `exit;`.

## 2. Revisar la base de datos existente

En Bash, conectarse con el usuario institucional (MySQL solicita la contraseña de forma interactiva):

```bash
mysql -u 5to_agbd -p
```

Ya en `mysql>`, comprobar usuario, bases, tablas y datos:

```sql
SELECT CURRENT_USER();
SHOW DATABASES;
USE datos_transito;
SHOW TABLES;
SELECT id, nombre, tipo, latenciaMs FROM dispositivos ORDER BY id;
SELECT id, paquete_id, entregado, descartado FROM simulaciones ORDER BY id;
```

Salir del cliente con:

```sql
exit;
```

La base de datos de este entorno ya tenía las cuatro tablas (`dispositivos`, `paquetes`, `recorridos`, `simulaciones`) y registros de ejemplo. La lectura por API se comprobó con respuesta HTTP 200: el endpoint de dispositivos devolvió seis registros y el de simulaciones dos.

## 3. Crear una base nueva (solo si hace falta)

Los scripts de inicialización no son para ejecutarlos indiscriminadamente sobre una base existente:

- `scripts/01_crear_tablas.sql` crea las tablas si faltan **y agrega datos de ejemplo**. Volver a ejecutarlo duplica esos datos.
- `backup/datos-en-transito.sql` crea tablas sin `IF NOT EXISTS`; solo debe restaurarse en una base vacía, porque falla si ya están creadas.
- `scripts/02_procedimientos_almacenados.sql` crea procedimientos almacenados auxiliares.
- `scripts/03_crear_usuarios.sql` crea usuarios. Requiere una cuenta con permisos administrativos y contiene contraseñas de ejemplo en texto plano; revisarlas y cambiarlas antes de usarlo, nunca reutilizarlas en producción.

Los scripts se ejecutan desde Bash, desde la raíz del repositorio. Por ejemplo, para inicializar tablas y datos de ejemplo en una base que aún no tiene tablas:

```bash
mysql -u 5to_agbd -p < scripts/01_crear_tablas.sql
```

Para restaurar el backup en una base nueva/vacía (no sobre las tablas existentes):

```bash
mysql -u 5to_agbd -p < backup/datos-en-transito.sql
```

Para instalar los procedimientos, si se necesitan:

```bash
mysql -u 5to_agbd -p < scripts/02_procedimientos_almacenados.sql
```

Si MySQL responde `Access denied` o `CREATE command denied`, la cuenta no tiene permisos para esa operación; debe hacerlo el administrador de la base. No se debe intentar reparar permisos con credenciales ajenas.

> En el cliente `mysql>`, no pegar `mysql -u ... < archivo.sql`: eso es un comando del shell. Salir primero con `exit;` y ejecutar el comando en Bash.

## 4. Configurar el acceso de la API

La API toma host, puerto, base y usuarios de variables de entorno si existen; de otro modo usa los valores `localhost`, `3306`, `datos_transito`, `administrador` y `desarrollo` de `src/API/appsettings.json`. Las contraseñas no se leen desde ese archivo. La fábrica acepta `DB_PASSWORD_ADMIN` / `DB_PASSWORD_READONLY` y también los nombres anteriores `DER_ADMIN_PASSWORD` / `DER_INVITADO_PASSWORD`.

Para el acceso institucional usado en las pruebas de lectura, configurar **en la misma terminal Bash donde se inicia la API**:

```bash
export DB_HOST=localhost
export DB_PORT=3306
export DB_NAME=datos_transito
export DB_USER_ADMIN=5to_agbd
export DB_USER_READONLY=5to_agbd
read -rsp 'Contraseña MySQL de 5to_agbd: ' MYSQL_PASSWORD; echo
export DB_PASSWORD_ADMIN="$MYSQL_PASSWORD"
export DB_PASSWORD_READONLY="$MYSQL_PASSWORD"
unset MYSQL_PASSWORD
```

Al ingresar la contraseña no se muestran caracteres; eso es normal. No pegar la contraseña en el código, `appsettings.json`, archivos del repositorio ni el chat.

Usar el mismo usuario para lectura y escritura es únicamente una opción práctica para el entorno local si `5to_agbd` posee ambos permisos. En producción y en una demostración con cuentas separadas, conviene configurar un usuario de escritura y otro con permiso de solo lectura. El script de usuarios actual define `administrador` y `desarrollo`; no presupongas que sus contraseñas o permisos coinciden con la cuenta institucional.

Las variables `export` duran mientras siga abierta esa terminal. Si se abre otra terminal o VS Code se reinicia, hay que volver a establecerlas. Para tomar variables recién configuradas, detener la API con `Ctrl+C` y reiniciarla desde la terminal donde se hizo el `export`.

## 5. Iniciar la API y abrir Swagger

Desde Bash, en la raíz del repo y después de configurar las variables:

```bash
dotnet run --project src/API/API.csproj --urls http://localhost:8080
```

No cerrar esa terminal mientras se usa la API. Swagger queda en:

- `http://localhost:8080/swagger`
- Endpoint OpenAPI: `http://localhost:8080/swagger/v1/swagger.json`

Si aparece `address already in use` para el puerto 8080, ya hay un proceso escuchando allí. No inicies una segunda copia: buscá la terminal de la API y detenela con `Ctrl+C`, o usa la instancia que ya está ejecutándose. Para usar otro puerto, cambiar la opción `--urls` y entrar a Swagger en el mismo puerto.

En Swagger, expandir la ruta, pulsar **Try it out**, completar parámetros y cuerpo JSON, y pulsar **Execute**. En rutas con `{id}` o `{orden}`, escribir únicamente el número en sus respectivos campos, sin llaves.

## 6. Endpoints disponibles

### Dispositivos

| Método y ruta | Uso | Respuestas típicas |
|---|---|---|
| `GET /api/dispositivos` | Lista dispositivos de MySQL | `200` |
| `POST /api/dispositivos` | Crea dispositivo | `201`, `400` |
| `PUT /api/dispositivos/{id}` | Actualiza nombre, tipo y latencia | `200`, `400`, `404` |
| `DELETE /api/dispositivos/{id}` | Borra dispositivo no referenciado | `204`, `404`, `409` |

Cuerpo de ejemplo para crear o actualizar:

```json
{
  "nombre": "router-demo",
  "tipo": "Router",
  "latenciaMs": 2
}
```

Tipos permitidos: `Router`, `Switch`, `Firewall`, `AccessPoint` (no distingue mayúsculas/minúsculas). La latencia debe ser mayor o igual a cero.

### Simulaciones

| Método y ruta | Uso | Respuestas típicas |
|---|---|---|
| `GET /api/simulaciones` | Lista simulaciones | `200` |
| `POST /api/simulaciones` | Ejecuta y guarda simulación y recorrido | `201`, `400` |
| `GET /api/simulaciones/{id}` | Consulta simulación y pasos | `200`, `404` |
| `PUT /api/simulaciones/{id}` | Recalcula y reemplaza la simulación | `200`, `400`, `404` |
| `DELETE /api/simulaciones/{id}` | Borra simulación, recorrido y paquete asociado | `204`, `404` |
| `PUT /api/simulaciones/{id}/pasos/{orden}` | Reemplaza el dispositivo del paso y recalcula | `200`, `400`, `404` |
| `DELETE /api/simulaciones/{id}/pasos/{orden}` | Quita un paso y recalcula | `200`, `400`, `404` |

Cuerpo para crear una simulación; reemplazar los IDs por dispositivos que existan en `GET /api/dispositivos`:

```json
{
  "origen": "10.0.0.1",
  "destino": "10.0.0.2",
  "tamanoBytes": 1200,
  "ttl": 5,
  "dispositivoIds": [1, 2, 3]
}
```

Para actualizar un paso, el cuerpo es:

```json
{
  "dispositivoId": 2
}
```

`orden` empieza en 1. Un recorrido debe conservar al menos un dispositivo. La modificación o eliminación de paso vuelve a ejecutar las reglas del dominio y persiste el nuevo paquete, resultado y recorrido.

## 7. Demostración guiada en Swagger

Primero ejecutar `GET /api/dispositivos` y copiar IDs reales. Para este entorno, la consulta comprobada mostró Router `1`, Switch `2`, Firewall `3`, Router `4`, Switch `5` y Firewall `6`; confirmar el listado antes de depender de esos IDs.

1. **Entrega correcta:** hacer `POST /api/simulaciones` con tamaño `1200`, TTL `5` y IDs `[1,2,3]` (o dispositivos equivalentes que existan). El estado esperado es `entregado`.
2. **Paquete descartado por Firewall:** repetir con `tamanoBytes: 2000` e incluir un Firewall en `dispositivoIds`, por ejemplo `[1,3]`. El estado esperado es `descartado`.
3. Anotar los IDs de simulación que responden en el POST. Consultar cada una con `GET /api/simulaciones/{id}` y el conjunto con `GET /api/simulaciones`.
4. **Actualizar dispositivo:** `PUT /api/dispositivos/{id}` y enviar un cuerpo válido.
5. **Cambiar paso:** `PUT /api/simulaciones/{id}/pasos/{orden}` con un ID de dispositivo existente; volver a consultar para observar el recálculo.
6. **Eliminar paso:** `DELETE /api/simulaciones/{id}/pasos/{orden}`; el último paso no se puede borrar.
7. **400:** `POST /api/dispositivos` con tipo `Modem`, o una solicitud incompleta. Se verificó `400` para el tipo no soportado.
8. **404:** `GET /api/simulaciones/999999` (confirmar que ese ID no existe).
9. **409:** intentar borrar con `DELETE /api/dispositivos/{id}` un dispositivo usado por un recorrido existente.
10. Para limpiar solo los datos de la demostración, borrar primero sus simulaciones y luego los dispositivos creados expresamente. No borrar registros preexistentes del backup.

## 8. Reglas de dominio y persistencia

La simulación instancia el tipo concreto de cada dispositivo y llama polimórficamente a `Procesar(paquete)`:

- `Router`: decrementa TTL; si llega a cero o menos, descarta el paquete.
- `AccessPoint`: también decrementa TTL y descarta cuando llega a cero o menos.
- `Switch`: deja continuar el paquete sin modificar TTL.
- `Firewall`: descarta si el paquete supera 1500 bytes.

La simulación deja de recorrer dispositivos cuando el paquete se descarta. La API usa Dapper para leer y escribir las tablas MySQL; la creación y actualización de simulaciones guardan los cambios relacionados dentro de una transacción. No usa Entity Framework ni requiere `dotnet ef migrations`. Los procedimientos almacenados del script no son necesarios para estas rutas activas.

## 9. Pruebas y estado de verificación

Desde la raíz del repositorio:

```bash
dotnet build Proyecto.slnx
dotnet test src/Tests/Tests.csproj
```

La última ejecución comprobada compiló la solución y aprobó 12 pruebas unitarias. Estas pruebas verifican lógica del dominio, no sustituyen una prueba de integración con MySQL.

También se verificó en Swagger que `GET /api/dispositivos` devolvió `200` con seis filas y `GET /api/simulaciones` devolvió `200` con dos filas. Se probó un tipo `Modem` y la API respondió `400`. No se deben dar por probadas las escrituras mientras no se ejecute una llamada de creación/actualización con un usuario que tenga permisos y se compruebe su resultado en MySQL.

## 10. Restaurar un backup

El backup del repositorio contiene DDL e inserciones y **no es idempotente** (no está preparado para recrear las tablas existentes). Para conservar datos o restaurar en una base que ya tiene tablas, primero hacer una copia de resguardo y planificar el destino; no ejecutar el backup directamente. Para una restauración de demostración en una base nueva/vacía, usar desde Bash:

```bash
mysql -u 5to_agbd -p < backup/datos-en-transito.sql
```

Luego ingresar a MySQL y comprobar con `USE datos_transito; SHOW TABLES;` y consultas `SELECT`. La restauración requiere permisos para crear base y tablas. Si no se tienen, solicitar al administrador una base vacía o que ejecute la restauración. Evitar cargar el mismo backup varias veces: las inserciones duplicarían datos y los IDs de ejemplo podrían dejar de coincidir.
