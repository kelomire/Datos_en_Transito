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
mysql -u root -p < scripts/DDL.sql
```

2. Crear los usuarios con permisos limitados:

```bash
mysql -u root -p < scripts/USUARIOS.sql
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
export DB_PASSWORD_ADMIN='CambiarPorUnaClaveSegura'
export DB_USER_READONLY=invitado
export DB_PASSWORD_READONLY='CambiarPorUnaClaveSegura'
```

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
  "ipOrigen": "10.0.0.1",
  "ipDestino": "10.0.0.2",
  "tamanoBytes": 1200,
  "ttl": 5,
  "dispositivos": [
    { "id": 1, "nombre": "router-principal", "tipo": "Router", "latenciaMs": 2 },
    { "id": 2, "nombre": "switch-core", "tipo": "Switch", "latenciaMs": 1 },
    { "id": 3, "nombre": "fw-perimetral", "tipo": "Firewall", "latenciaMs": 3 }
  ]
}
```

## Descripción de permisos

- `administrador`: acceso completo a la base para escribir, actualizar y administrar estructura.
- `invitado`: lectura restringida (`SELECT`) para consultas sin permisos de modificación.

## Sugerencias de seguridad

- No versionar contraseñas reales.
- Mantener la configuración sensible en variables de entorno o secrets locales.
- Usar `appsettings.Development.json` sólo con valores no productivos.

## Estado de la tarea

La solución actual incluye la lógica de dominio, pruebas unitarias y el armazón de la API con Swagger. El proyecto está listo para ampliarse con más endpoints y persistencia real en MySQL según el diseño del plan pedagógico.
