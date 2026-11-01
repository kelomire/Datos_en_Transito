# Reflexión final

## Comparación con el punto de partida

En el documento de punto de partida, la hipótesis inicial asumía un flujo más conceptual: un paquete atravesando firewall, router y switch con reglas de seguridad y enrutamiento. Esa visión era útil como base, pero la implementación posterior mostró que el diseño más firme es hacer explícita la separación entre:

- la lógica del dominio,
- la capa de servicio,
- la persistencia,
- y la API HTTP.

La diferencia principal es que el planteo inicial pensaba en topologías y decisiones de red más complejas, mientras que la solución final se centró en un modelo más claro y medible: cada dispositivo implementa una regla concreta mediante polimorfismo, y la simulación reúne los pasos y latencias sin acoplar la lógica al transporte HTTP.

## Qué decisiones cambian y por qué

1. Cambio del modelo de decisión: la validación del paquete ya no se resuelve en rutas HTTP, sino en la entidad del dominio (`PaqueteRed`) y en cada dispositivo concreto.
2. Enfoque en polimorfismo: un `DispositivoRed` define la operación central `Procesar`, y cada subclase aporta una regla distinta.
3. Separación clara de responsabilidades: los servicios coordinan la lógica de negocio y los repositorios encapsulan el almacenamiento.
4. Relevancia del backtracking del recorrido: la simulación ya no es solo un conjunto de pasos, sino un historial con latencia y estado final que puede reutilizarse para análisis.

## Dificultad resuelta

La mayor dificultad fue mantener el comportamiento de descarte y continuidad sin mezclar lógica de red con infraestructura. El punto clave fue definir reglas simples pero consistentes:

- el TTL se reduce cuando corresponde,
- la latencia se acumula por paso,
- el paquete se marca como descartado cuando la regla lo exige,
- y la simulación se detiene de forma coherente al detectar el bloqueo.

## Mejora futura

Si la API tuviera que atender más tipos de dispositivos, la mejora más natural sería introducir una estrategia configurable por tipo de dispositivo, con metadatos de comportamiento para firewall, router, switch y sensores. También convendría separar la representación de almacenamiento SQL de la entidad del dominio para facilitar extensiones sin romper el contrato de la API.

En una próxima versión, la capa de persistencia puede incorporarse con más profundidad a MySQL y dejar un modelo de DTO más estricto para la entrada/salida HTTP, más estable para clientes externos.
