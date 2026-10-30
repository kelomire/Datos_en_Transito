# Documento de Indagación: Diseño de API, Arquitectura y Datos

Este documento consolida la investigación técnica y las decisiones de diseño arquitectónico para el simulador de paquetes de red utilizando .NET Minimal APIs y MySQL.

---

## Actividad 1: Preguntas para diseñar la API

### 1. Vinculación de rutas y conversión a JSON en Minimal APIs
Minimal APIs utiliza métodos de extensión directamente sobre la instancia de `WebApplication` (como `.MapGet()`, `.MapPost()`, `.MapPut()`, `.MapDelete()`) para asociar un verbo HTTP y un patrón de ruta con un delegado o función handler.
* **Conversión Automática:** El middleware nativo utiliza `System.Text.Json` para deserializar el cuerpo de la solicitud entrante (Request Body) hacia objetos fuertemente tipados, y para serializar los objetos devueltos en las respuestas directamente a formato JSON.
* **Códigos de Estado:** Se controlan utilizando los métodos utilitarios de la clase estática `Results` (por ejemplo, `Results.Ok()`, `Results.Created()`, `Results.BadRequest()`).

### 2. Matriz de Decisiones para Códigos de Estado HTTP
* **`200 OK`**: Corresponde cuando una solicitud de consulta (GET) o una simulación efímera (que no persiste datos a largo plazo) se procesa correctamente y devuelve el resultado esperado.
* **`201 Created`**: Se utiliza tras una operación de escritura (POST) exitosa que crea un nuevo recurso persistente en el sistema (por ejemplo, guardar una topología de red o registrar un log histórico). Debe incluir una cabecera `Location` con la URL del nuevo recurso.
* **`400 Bad Request`**: Se devuelve cuando la solicitud del cliente tiene fallas de formato, tipos de datos incorrectos, o no supera las validaciones de negocio iniciales (ej. una dirección IP con formato inválido o campos obligatorios vacíos).
* **`404 Not Found`**: Corresponde cuando el recurso solicitado mediante un identificador único (ID) o ruta específica no existe en el sistema (ej. intentar simular un recorrido sobre un Router con un ID inexistente).

### 3. Documentación para Swagger (OpenAPI)
Para que los endpoints de Minimal API sean visibles, descriptivos y probables en la interfaz de Swagger, se utilizan metadatos acoplados mediante métodos de extensión:
* `.WithOpenApi()`: Genera la estructura base del esquema OpenAPI para el endpoint.
* `.WithName("NombreAccion")`: Asigna un identificador único a la operación.
* `.Produces<TResponse>(StatusCodes.Status200OK)`: Documenta el tipo de objeto JSON que se devuelve en caso de éxito.
* `.ProducesProblem(StatusCodes.Status400BadRequest)`: Documenta la estructura de error devuelta ante fallas de validación.

### 4. Límites de la Definición de Rutas (Responsabilidad Única)
**Fuera de la definición de rutas deben quedar:**
* Las reglas de enrutamiento y lógica de descarte de paquetes de red.
* Las consultas físicas o escrituras en la base de datos.
* Las validaciones complejas de formatos de red.
La definición de la ruta en `Program.cs` debe actuar estrictamente como un **controlador o receptor**, delegando toda la lógica de negocio a la capa de servicios.

---

## Actividad 2: Patrón DTO (Data Transfer Object) y Flujo de Datos

### 1. Diferencias conceptuales
* **DTO (Data Transfer Object):** Objeto plano (Anémico), optimizado para el transporte de datos por red. No tiene lógica de negocio ni comportamiento. Su estructura está diseñada para cumplir el contrato de la API.
* **Clase de Dominio:** Modelo rico que representa las entidades del negocio (ej. `Router`, `Firewall`, `Paquete`). Contiene el estado, las reglas de validación interna y el comportamiento del simulador de red.
* **Tabla de Base de Datos:** Estructura de almacenamiento físico (relacional) definida por columnas, claves primarias, foráneas e índices en MySQL.

### 2. Propuesta de DTOs para la Simulación

#### DTO de Entrada: `SimulationRequestDto`
* `SourceIp` (string): Dirección IP de origen del paquete (Formato IPv4).
* `DestinationIp` (string): Dirección IP de destino del paquete (Formato IPv4).
* `SourcePort` (int): Puerto de origen (1-65535).
* `DestinationPort` (int): Puerto de destino (1-65535).
* `Payload` (string): Contenido de datos del paquete.

#### DTO de Output/Salida: `SimulationResponseDto`
* `SimulationId` (Guid): Identificador único del procesamiento.
* `IsAllowed` (bool): `true` si el paquete llegó a destino; `false` si fue bloqueado/descartado.
* `DropReason` (string?): Motivo del descarte (ej. "Bloqueado por regla de Firewall").
* `TraceSteps` (List<StepDto>): Historial detallado del recorrido indicando qué dispositivo procesó el paquete y qué acción tomó.

### 3. Ubicación de la Validación de Datos
La validación se realizará en el **Endpoint (Capa de API)** antes de invocar al servicio. Se recomienda el uso de `FluentValidation` para interceptar la solicitud de forma limpia. Si el DTO no es válido, se corta la ejecución inmediatamente y se retorna un código `400 Bad Request`, protegiendo la integridad de la capa de servicios de datos mal formados.

---

## Actividad 3: Acceso, Privilegios y Recuperación de Datos (MySQL)

### 1. Contraste de Permisos y Roles de Usuario
* **Usuario Administrador (Admin/Migraciones):** Requiere permisos estructurales y globales (`CREATE`, `DROP`, `ALTER`, `INDEX`, `SELECT`, `INSERT`, `UPDATE`, `DELETE`). Se utiliza exclusivamente durante el despliegue automático, ejecución de migraciones de base de datos o mantenimiento manual.
* **Usuario Invitado/Aplicación (Runtime API):** Solo requiere permisos para interactuar con los datos de las tablas sin alterar la arquitectura. Se le otorgan estrictamente privilegios DML: `SELECT`, `INSERT`, `UPDATE` (y `DELETE` solo si el negocio lo requiere). No posee permisos de tipo DDL (`DROP` o `ALTER`).

### 2. Prueba en Base de Datos Descartable (Evidencia)
Se creó una base de prueba y un usuario limitado para verificar las restricciones:

```sql
-- 1. Crear el usuario de solo lectura
CREATE USER 'lector_test'@'localhost' IDENTIFIED BY 'PasswordSegura123!';
GRANT SELECT ON simulador_db.* TO 'lector_test'@'localhost';
FLUSH PRIVILEGES;

-- 2. Prueba de Lectura (PERMITIDA)
-- Ejecutado desde el usuario 'lector_test':
SELECT * FROM dispositivos;
-- Resultado: 3 filas devueltas (OK).

-- 3. Prueba de Escritura (RECHAZADA)
-- Ejecutado desde el usuario 'lector_test':
INSERT INTO dispositivos (nombre, tipo) VALUES ('Router Central', 'Router');
-- Resultado: Error ErrorCode: 1142. INSERT command denied to user 'lector_test'@'localhost' for table 'dispositivos'.
```

### 3. Seguridad de Credenciales y GitHub
* **Prohibición:** Está estrictamente prohibido escribir contraseñas explícitas en `Program.cs` o subir el archivo `appsettings.json` con credenciales reales a repositorios públicos.
* **Estrategia:** Se agrega `appsettings.Development.json` o archivos locales con configuraciones privadas al archivo `.gitignore`. En entornos productivos o de simulación, la cadena de conexión se inyecta dinámicamente mediante **Variables de Entorno** del sistema o herramientas de gestión de secretos (User Secrets en desarrollo de .NET).

### 4. Procedimiento Seguro de Respaldo y Restauración
Para evitar exponer la contraseña en texto plano en la terminal (`-pContraseña`), se utiliza un archivo de configuración intermedio temporal protegido (por ejemplo, `secrets.cnf`).

**Comando de Respaldo (Backup):**
```bash
mysqldump --defaults-extra-file=secrets.cnf -u root simulador_db > backups/respaldo_simulador.sql
```
*(El archivo `secrets.cnf` contiene únicamente la estructura `[client]\npassword=TuContraseña` y no se incluye en el control de versiones).*

**Comando de Restauración:**
```bash
mysql --defaults-extra-file=secrets.cnf -u root simulador_db < backups/respaldo_simulador.sql
```

---

## Actividad 4: Inversión de Control (IoC) y Responsabilidades

### 1. Delimitación de Responsabilidades
* **Servicio (`SimulationService`):** Contiene las reglas del negocio de red. Orquesta el recorrido del paquete, evalúa las políticas de los firewalls, determina las tablas de enrutamiento y genera el DTO de salida final. No sabe cómo se guardan los datos ni de dónde vienen.
* **Repositorio (`SimulationRepository`):** Encapsula el acceso de bajo nivel a la base de datos MySQL. Se encarga exclusivamente de las consultas y persistencia física de entidades. Expone una interfaz limpia al servicio.

### 2. Correcciones a las Hipótesis Iniciales
En el diseño del "Punto de Partida", se asumió que los dispositivos procesarían los paquetes directamente interactuando con controladores HTTP. Tras esta indagación, se corrige el acoplamiento: la API interactúa exclusivamente con DTOs inmutables, derivando el flujo a un servicio que trabaja con abstracciones en memoria y guardando las trazas mediante repositorios desacoplados de la infraestructura de red.
