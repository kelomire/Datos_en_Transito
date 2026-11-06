# Datos en tránsito: API

Este repositorio contiene una solución .NET para simular el recorrido de un paquete de red a través de distintos dispositivos (router, switch, firewall, access point), registrar simulaciones y documentar la lógica con pruebas unitarias.

## Estructura del proyecto

- `src/API`: API web con Minimal API y Swagger.
- `src/Aplicacion`: servicios de dominio y reglas de negocio.
- `src/Persistencia`: entidades, repositorios y fábrica de conexiones.
- `src/Tests`: pruebas unitarias del comportamiento del simulador.
- `scripts`: scripts SQL para crear tablas y usuarios.
- `Docs`: documentación del punto de partida, indagación y reflexión final.

## Requisitos

- .NET SDK 10
- MySQL 8+
- Cliente MySQL (`mysql`) y herramientas de línea (`mysqldump`)

## Preparación de la base de datos

1. Crear la base y las tablas:

```bash
mysql -u root -p < scripts/01_crear_tablas.sql
```

2. Crear los usuarios:

```bash
mysql -u root -p < scripts/03_crear_usuarios.sql
```

3. Restaurar el backup de ejemplo:

```bash
mysql -u root -p datos_transito < backup/datos-en-transito.sql
```

> El backup no incluye contraseñas ni secretos. Las credenciales reales deben mantenerse fuera del repositorio y cargarse con variables de entorno o secretos locales.

## Variables de entorno recomendadas

Se recomienda definir estas variables antes de ejecutar la API o la conexión de MySQL localmente:

```bash
export DB_HOST=localhost
export DB_PORT=3306
export DB_NAME=datos_transito
export DB_USER_ADMIN=administrador
export DB_PASSWORD_ADMIN='clave-del-usuario-administrador'
export DB_USER_READONLY=desarrollo
export DB_PASSWORD_READONLY='clave-del-usuario-desarrollo'
```

La API usa `administrador` para escrituras y el usuario configurado en `DB_USER_READONLY` para consultas. Los valores predeterminados de usuarios coinciden con `scripts/03_crear_usuarios.sql`; define las contraseñas en el entorno y no las guardes en el repositorio. También se aceptan `DER_ADMIN_PASSWORD` y `DER_INVITADO_PASSWORD` por compatibilidad.

La persistencia de esta API usa MySQL con Dapper y las tablas de `scripts/01_crear_tablas.sql`; no requiere Entity Framework ni `dotnet ef migrations`.

## Ejecutar la API

Desde la raíz del proyecto:

```bash
dotnet run --project src/API/API.csproj
```

La API queda disponible en:

- Swagger UI: http://localhost:5000/swagger
- API base: http://localhost:5000

## Ejecutar pruebas

```bash
dotnet test --nologo
```

## Regla principal del dominio

La lógica de simulación se resuelve con polimorfismo:

- `Router`: reduce el TTL en 1. Si llega a 0, el paquete se descarta.
- `Switch`: no modifica el TTL y continúa.
- `Firewall`: descarta paquetes mayores a 1500 bytes.
- `AccessPoint`: comparte el mismo comportamiento de decremento del TTL que un router.
- `DispositivoRed` define el contrato común: `Procesar(PaqueteRed)`.

## Ejemplo de solicitud de simulación

```json
{
  "origen": "10.0.0.1",
  "destino": "10.0.0.2",
  "tamanoBytes": 1200,
  "ttl": 5,
  "dispositivoIds": [1, 2, 3]
}
```

## Rutas principales para la demostración

- Dispositivos: `GET` y `POST /api/dispositivos`, `PUT` y `DELETE /api/dispositivos/{id}`.
- Simulaciones: `GET` y `POST /api/simulaciones`, `GET`, `PUT` y `DELETE /api/simulaciones/{id}`.
- Pasos del recorrido: `PUT` y `DELETE /api/simulaciones/{id}/pasos/{orden}`. Al modificar o borrar un paso se vuelve a calcular y guardar el recorrido completo. No se permite borrar el último paso.

## Descripción de permisos

- `administrador`: acceso completo a la base para escribir, actualizar y administrar estructura.
- `invitado`: lectura restringida (`SELECT`) para consultas sin permisos de modificación.

## Sugerencias de seguridad

- No versionar contraseñas reales.
- Mantener la configuración sensible en variables de entorno o secrets locales.
- Usar `appsettings.Development.json` sólo con valores no productivos.

## Estado de la tarea

La API usa MySQL mediante Dapper. Las pruebas unitarias cubren las reglas de dominio; no sustituyen una prueba de integración contra un servidor MySQL configurado.
