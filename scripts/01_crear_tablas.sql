CREATE DATABASE IF NOT EXISTS datos_transito;
USE datos_transito;

CREATE TABLE IF NOT EXISTS dispositivos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    latenciaMs INT NOT NULL
);

CREATE TABLE IF NOT EXISTS paquetes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    ip_origen VARCHAR(45) NOT NULL,
    ip_destino VARCHAR(45) NOT NULL,
    tamano_bytes INT NOT NULL,
    ttl_inicial INT NOT NULL
);

CREATE TABLE IF NOT EXISTS simulaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    paquete_id INT,
    latenciaMs_total INT NOT NULL DEFAULT 0,
    ttl_final INT NOT NULL,
    entregado BOOLEAN NOT NULL,
    descartado BOOLEAN NOT NULL,
    fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (paquete_id) REFERENCES paquetes(id)
);

CREATE TABLE IF NOT EXISTS recorridos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    simulacion_id INT NOT NULL,
    dispositivo_id INT NOT NULL,
    latenciaMs INT NOT NULL,
    ttl_final INT NOT NULL,
    entregado BOOLEAN NOT NULL,
    descartado BOOLEAN NOT NULL,
    FOREIGN KEY (simulacion_id) REFERENCES simulaciones(id),
    FOREIGN KEY (dispositivo_id) REFERENCES dispositivos(id)
);

insert INTO dispositivos (nombre, tipo, latenciaMs)
values
("router 1", "router", 10),
("switch 1", "switch", 5),
("firewall 1", "firewall", 15),
("router 2", "router", 12),
("switch 2", "switch", 7),
("firewall 2", "firewall", 20);

insert INTO paquetes (ip_origen, ip_destino, tamano_bytes, ttl_inicial)
values
("192.168.1.1", "192.168.1.2", 1500, 64),
("192.168.1.2", "192.168.1.5", 1500, 64);

insert INTO simulaciones (paquete_id, latenciaMs_total, ttl_final, entregado, descartado)
values
(1, 0, 64, true, false),
(2, 0, 64, true, false);

insert INTO recorridos (simulacion_id, dispositivo_id, latenciaMs, ttl_final, entregado, descartado)
values
(1, 1, 10, 64, true, false),
(1, 2, 5, 64, true, false),
(1, 3, 15, 64, true, false),
(2, 4, 12, 64, true, false),
(2, 5, 7, 64, true, false);