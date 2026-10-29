# Punto de Partida - Simulación de Redes

Este documento contiene la propuesta inicial de diseño para el simulador de tránsito de paquetes en dispositivos de red, contemplando el recorrido de un paquete (Actividad 1) y la interfaz de comunicación mediante solicitudes (Actividad 2).

---

## 1. Recorrido del Paquete (Actividad 1)

### 1.1 Descripción del Recorrido y Comportamiento de Dispositivos

Imaginemos un paquete de datos que viaja desde una red externa hacia un servidor interno, atravesando en orden: **Firewall**, **Router** y **Switch**.

1. **Firewall**: Es la primera línea de defensa. Analiza los datos de control del paquete para determinar si cumple con las políticas de seguridad de la red.
   - *Comportamiento*: Compara el puerto de destino y la IP de origen contra una lista blanca/negra. Si pasa la validación, el paquete sigue su camino; de lo contrario, se descarta (Drop) o rechaza (Reject).
2. **Router**: Se encarga de la interconexión entre diferentes redes (Capa 3).
   - *Comportamiento*: Revisa la IP de destino. Consulta su tabla de enrutamiento interna para decidir por cuál de sus interfaces de salida debe reenviar el paquete.
3. **Switch**: Conecta dispositivos dentro de la misma red local (Capa 2).
   - *Comportamiento*: Revisa la dirección MAC de destino. Consulta su tabla MAC (CAM) para enviar el paquete exclusivamente al puerto físico donde está conectado el destinatario final, evitando inundar la red.

### 1.2 Reglas que cambian según el Dispositivo

El procesamiento del paquete varía estructuralmente en cada paso de la topología:
* **En el Firewall (Criterio de Seguridad)**: La regla evalúa si el par `(ipOrigen, puertoDestino)` está autorizado.
* **En el Router (Criterio de Enrutamiento)**: La regla evalúa la mejor coincidencia de red (Longest Prefix Match) sobre la `ipDestino`.
* **En el Switch (Criterio de Conmutación)**: La regla evalúa la existencia de la `macDestino` en la tabla física de puertos.

### 1.3 Datos a conservar de cada paso (Métricas de Simulación)
Para fines de auditoría y análisis de la simulación, se guardará un registro histórico por cada dispositivo que toque el paquete:
* ID del Dispositivo y Tipo.
* Timestamp exacto del ingreso y egreso.
* Estado final del procesamiento (`PROCESADO`, `REENVIADO`, `DENEGADO`, `ERROR`).
* Detalle de la acción tomada (ej: *"Reenviado por interfaz Eth1 hacia IP 192.168.1.1"* o *"Bloqueado por Regla #4 - Puerto 80 de origen"*).

---

## 2. Modelo de Clases Inicial

Proponemos la siguiente estructura de clases preliminar para soportar el dominio de la simulación:

* **Paquete**: Representa la entidad que viaja. Atributos: `ipOrigen`, `ipDestino`, `macOrigen`, `macDestino`, `puertoOrigen`, `puertoDestino`, `cargaUtil` (payload).
* **Dispositivo (Abstracta)**: Clase base para los componentes de red. Atributos: `id`, `nombre`. Método: `procesar(Paquete)`.
  * **Firewall (Hereda de Dispositivo)**: Posee una colección de `ReglasDeSeguridad`.
  * **Router (Hereda de Dispositivo)**: Posee una `TablaDeEnrutamiento`.
  * **Switch (Hereda de Dispositivo)**: Posee una `TablaMac`.
* **ResultadoPaso**: Almacena las métricas individuales de un paquete al cruzar un dispositivo específico.
* **Simulacion**: Coordina el flujo global. Atributos: `id`, `paqueteInicial`, `topologia` (lista ordenada de `Dispositivo`), `historialPasos` (lista de `ResultadoPaso`).

### Diagrama de Entidad Relación (DER) Conceptual

```
+------------+             1..* +-------------------+
| Simulacion |----------------->|   ResultadoPaso   |
+------------+                  +-------------------+
      | 1                                 | *
      |                                   |
      | 1                                 v 1
+------------+                  +-------------------+
|  Paquete   |                  |    Dispositivo    |
+------------+                  +-------------------+
                                          ^
                                          | (Herencia)
                         +----------------+----------------+
                         |                |                |
                   +-----------+    +-----------+    +-----------+
                   |  Firewall |    |  Router   |    |  Switch   |
                   +-----------+    +-----------+    +-----------+
```

---

## 3. Hipótesis de Diseño y Dudas Técnicas

### Hipótesis
* **Inmutabilidad del Paquete**: Consideramos que los datos esenciales del paquete (IPs, Puertos) no cambian durante el trayecto para simplificar el estado inicial de la simulación (sin NAT por el momento).
* **Topología Lineal Estática**: El orden de los dispositivos está predefinido en una lista secuencial pura dentro de la configuración del escenario.

### Decisiones no resueltas (Dudas)
1. ¿Cómo modelar el descarte de paquetes en el flujo de control? Si el Firewall bloquea el paquete, ¿la simulación debe arrojar una excepción, retornar una lista parcial de pasos o lanzar un evento de interrupción?
2. ¿Cómo manejar el dinamismo de las tablas MAC y de enrutamiento? ¿Deben inicializarse fijas en el escenario o simular el aprendizaje dinámico por paquetes (ARP / Flooding)?

---

## 4. De una Simulación a una Solicitud (Actividad 2)

### 4.1 Intercambio Cliente - Servidor (API Conceptual)

Pensando en la integración con otros sistemas independientes, el flujo de ejecución se gestionará de manera sincrónica/asincrónica mediante mensajería estructurada en formato JSON.

#### Ejemplo de Solicitud (Request)
El cliente envía los datos del paquete de prueba y la secuencia de red a validar.

```json
{
  "escenario_id": "esc-94827",
  "paquete": {
    "ip_origen": "200.45.12.3",
    "ip_destino": "192.168.1.50",
    "mac_origen": "00:1A:2B:3C:4D:5E",
    "mac_destino": "00:11:22:33:44:55",
    "puerto_origen": 49152,
    "puerto_destino": 80
  },
  "topologia_solicitada": ["firewall_perimetral", "router_core", "switch_lan_01"]
}
```

#### Ejemplo de Respuesta Exitosa (Response 200 OK)
El servidor devuelve el informe detallado del camino recorrido indicando que el paquete completó el trayecto con éxito.

```json
{
  "simulacion_id": "sim-2026-0019",
  "estado_global": "COMPLETADO",
  "total_pasos": 3,
  "cronologia": [
    {
      "paso": 1,
      "dispositivo": "firewall_perimetral",
      "tipo": "Firewall",
      "resultado": "PERMITIDO",
      "observacion": "Tráfico autorizado en puerto 80 por regla general de HTTP."
    },
    {
      "paso": 2,
      "dispositivo": "router_core",
      "tipo": "Router",
      "resultado": "REENVIADO",
      "observacion": "Ruta encontrada en interfaz GigabitEthernet0/1."
    },
    {
      "paso": 3,
      "dispositivo": "switch_lan_01",
      "tipo": "Switch",
      "resultado": "ENTREGADO",
      "observacion": "Paquete enviado al puerto físico 12 asociado a la MAC de destino."
    }
  ]
}
```

### 4.2 Situaciones de Error Propuestas

#### Caso de Error 1: Estructura del Paquete Inválida (Error de Validación 400 Bad Request)
Ocurre cuando el cliente envía datos que rompen las reglas de negocio básicas del protocolo de red (por ejemplo, una dirección IP fuera de formato o un puerto inexistente).

* **JSON de Respuesta:**
```json
{
  "codigo_error": "INVALID_PACKET_FORMAT",
  "mensaje": "La solicitud contiene datos de red con formato erróneo.",
  "detalles": {
    "paquete.ip_origen": "El valor '200.45.12.333' no es una dirección IPv4 válida.",
    "paquete.puerto_destino": "El puerto debe estar en el rango de 1 a 65355."
  }
}
```

#### Caso de Error 2: Dispositivo Inexistente en Topología (Error de Recurso 404 Not Found)
Ocurre cuando la solicitud incluye un identificador de dispositivo en la lista de `topologia_solicitada` que no está registrado en la base de datos del simulador.

* **JSON de Respuesta:**
```json
{
  "codigo_error": "DEVICE_NOT_FOUND",
  "mensaje": "No se pudo inicializar el entorno de la simulación debido a componentes desconocidos.",
  "detalles": {
    "identificador_invalido": "router_core",
    "sugerencia": "Verifique los identificadores de hardware activos en el inventario antes de lanzar la simulación."
  }
}
```