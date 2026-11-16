# Guía de ejecución de la API y conexión a MySQL

Esta guía explica cómo abrir el proyecto, iniciar la API y verificar que se conecta correctamente a la base de datos `datos_transito`.

## Requisitos

- .NET SDK 10 instalado
- MySQL 8 corriendo en localhost
- Cuenta válida del proyecto en MySQL
- Git Bash / terminal Bash o equivalente

## 1. Abrir la carpeta del proyecto

Desde la terminal:

```bash
cd /home/et12alumno/Datos_en_Transito
```

## 2. Verificar que MySQL esté disponible

Ejecutá esta comprobación:

```bash
mysql -u 5to_agbd -p -h localhost -e "SELECT USER(), CURRENT_USER(), VERSION(); SHOW DATABASES; USE datos_transito; SHOW TABLES;"
```

Cuando te pida la contraseña, ingresá:

```text
Trigg3rs!
```

Si la conexión funciona, verás la base `datos_transito` y sus tablas, por ejemplo:

- `dispositivos`
- `paquetes`
- `recorridos`
- `simulaciones`

## 3. Configurar las variables de entorno

En la misma terminal donde vas a ejecutar la API, cargá estas variables:

```bash
export DB_HOST=localhost
export DB_PORT=3306
export DB_NAME=datos_transito
export DB_USER_ADMIN=5to_agbd
export DB_USER_READONLY=5to_agbd
export DB_PASSWORD_ADMIN='Trigg3rs!'
export DB_PASSWORD_READONLY='Trigg3rs!'
```

> La API usa estas variables para crear las conexiones a MySQL. Si no están configuradas, intentará usar valores por defecto, pero en este proyecto la autenticación real necesita estas variables.

## 4. Arrancar la API

Desde la raíz del proyecto:

```bash
dotnet run --project src/API/API.csproj --urls http://localhost:8080
```

Si todo sale bien, la terminal mostrará algo similar a:

```text
Now listening on: http://localhost:8080
Application started. Press Ctrl+C to shut down.
```

## 5. Probar que la API está conectada a la base

Abrí otra terminal y ejecutá:

```bash
curl -sS -D - -o /tmp/api_response.txt http://localhost:8080/api/dispositivos
```

Luego mostrás el cuerpo:

```bash
cat /tmp/api_response.txt
```

Si la base está conectada correctamente, deberías ver una respuesta HTTP similar a:

```text
HTTP/1.1 200 OK
```

y un JSON con los dispositivos, por ejemplo:

```json
[
  {"id":1,"nombre":"router 1","tipo":"router","latenciaMs":10},
  {"id":2,"nombre":"switch 1","tipo":"switch","latenciaMs":5},
  {"id":3,"nombre":"firewall 1","tipo":"firewall","latenciaMs":15}
]
```

## 6. Abrir el proyecto en Visual Studio Code

1. Abrí Visual Studio Code.
2. En el menú superior, clickeá en File > Open Folder...
3. Seleccioná la carpeta:

```text
/home/et12alumno/Datos_en_Transito
```

4. Cuando se abra el proyecto, podés ver la estructura de carpetas y archivos del repositorio.
5. Para abrir una terminal dentro de VS Code, hacé clic en Terminal > New Terminal.
6. En esa terminal, ejecutá los mismos comandos de la guía para arrancar la API.

## 7. Acceder a Swagger desde VS Code

Una vez levantada la API, abrí el navegador o la vista previa integrada del navegador y entrá a esta URL:

```text
http://localhost:8080/swagger
```

Desde ahí podés probar los endpoints del proyecto directamente en la interfaz web.

Si querés abrir la URL desde VS Code, podés usar:

- Click derecho en el archivo HTML o abrir la URL en el navegador externo, o
- En una terminal integrada, ejecutá el navegador predeterminado con la URL, o
- Abrir la pestaña de navegador en VS Code si tenés una extensión para vista previa web.

## 8. Si aparece error 500

Si la API responde `500 Internal Server Error`, revisar esto:

1. Que MySQL esté corriendo.
2. Que la cuenta `5to_agbd` exista y tenga acceso a `datos_transito`.
3. Que las variables del entorno estén exportadas en la misma terminal donde arrancás la API.
4. Que el puerto 8080 no esté ocupado por otro proceso.

En ese caso, podés comprobar qué proceso está usando el puerto:

```bash
ss -lntp | grep 8080
```

Y si hace falta, reiniciar la API con una terminal limpia.

## 9. Comando resumido

Si querés arrancarlo todo en una sola línea:

```bash
cd /home/et12alumno/Datos_en_Transito && export DB_HOST=localhost DB_PORT=3306 DB_NAME=datos_transito DB_USER_ADMIN=5to_agbd DB_USER_READONLY=5to_agbd DB_PASSWORD_ADMIN='Trigg3rs!' DB_PASSWORD_READONLY='Trigg3rs!' && dotnet run --project src/API/API.csproj --urls http://localhost:8080
```

## 10. Archivos relevantes del proyecto

- [src/API/Program.cs](../src/API/Program.cs)
- [src/Aplicacion/Servicios](../src/Aplicacion/Servicios)
- [src/Persistencia/Repositorios](../src/Persistencia/Repositorios)
- [src/Persistencia/MySqlConnectionFactory.cs](../src/Persistencia/MySqlConnectionFactory.cs)
- [scripts/01_crear_tablas.sql](../scripts/01_crear_tablas.sql)
- [README.md](../README.md)

## 11. Resultado esperado

Cuando todo está bien, la API queda levantada en:

```text
http://localhost:8080/swagger
```

y la consulta a `/api/dispositivos` debe devolver los registros existentes en la base `datos_transito`.

### En resumen para abrirlo en VS Code

```text
1. Abrir VS Code
2. File > Open Folder > /home/et12alumno/Datos_en_Transito
3. Terminal > New Terminal
4. Ejecutar:
   export DB_HOST=localhost
   export DB_PORT=3306
   export DB_NAME=datos_transito
   export DB_USER_ADMIN=5to_agbd
   export DB_USER_READONLY=5to_agbd
   export DB_PASSWORD_ADMIN='Trigg3rs!'
   export DB_PASSWORD_READONLY='Trigg3rs!'
   dotnet run --project src/API/API.csproj --urls http://localhost:8080
5. Abrir en el navegador:
   http://localhost:8080/swagger
```
