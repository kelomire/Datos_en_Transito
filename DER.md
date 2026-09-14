# Modelo Relacional - Datos en tránsito

```mermaid
erDiagram
    DISPOSITIVOS {
        int id PK
        varchar tipo
        varchar nombre
        varchar direccion_ip
        decimal latencia
    }

    SIMULACIONES {
        int id PK
        datetime fecha_inicio
        datetime fecha_fin
        varchar estado
        decimal latencia_total
        int ttl_final
    }

    PAQUETES {
        int id PK
        int simulacion_id FK
        varchar ip_origen
        varchar ip_destino
        int ttl_inicial
        int ttl_final
        varchar estado
    }

    RECORRIDOS {
        int id PK
        int simulacion_id FK
        int paquete_id FK
        int dispositivo_id FK
        int orden
        decimal latencia
    }

    RESULTADOS {
        int id PK
        int recorrido_id FK
        varchar estado
        decimal latencia
        int ttl
        int bytes
    }

    SIMULACIONES ||--o{ PAQUETES : contiene
    SIMULACIONES ||--o{ RECORRIDOS : posee
    PAQUETES ||--o{ RECORRIDOS : realiza
    DISPOSITIVOS ||--o{ RECORRIDOS : participa
    RECORRIDOS ||--o{ RESULTADOS : genera