CREATE DATABASE IF NOT EXISTS datos_transito;
USE datos_transito;

CREATE TABLE dispositivos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    latenciaMs INT NOT NULL
);

CREATE TABLE paquetes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    ip_origen VARCHAR(45) NOT NULL,
    ip_destino VARCHAR(45) NOT NULL,
    tamano_bytes INT NOT NULL,
    ttl_inicial INT NOT NULL
);

CREATE TABLE simulaciones (
    id INT AUTO_INCREMENT PRIMARY KEY,
    paquete_id INT,
    latenciaMs_total INT NOT NULL DEFAULT 0,
    ttl_final INT NOT NULL,
    entregado BOOLEAN NOT NULL,
    descartado BOOLEAN NOT NULL,
    fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (paquete_id) REFERENCES paquetes(id)
);

CREATE TABLE recorridos (
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

INSERT INTO dispositivos (nombre, tipo, latenciaMs) VALUES
('router-principal', 'Router', 2),
('switch-core', 'Switch', 1),
('firewall-perimetral', 'Firewall', 3);

INSERT INTO paquetes (ip_origen, ip_destino, tamano_bytes, ttl_inicial) VALUES
('10.0.0.1', '10.0.0.2', 1200, 5),
('10.0.0.3', '10.0.0.4', 2000, 3);

INSERT INTO simulaciones (paquete_id, latenciaMs_total, ttl_final, entregado, descartado) VALUES
(1, 6, 4, TRUE, FALSE),
(2, 4, 0, FALSE, TRUE);

INSERT INTO recorridos (simulacion_id, dispositivo_id, latenciaMs, ttl_final, entregado, descartado) VALUES
(1, 1, 2, 4, FALSE, FALSE),
(1, 2, 1, 4, FALSE, FALSE),
(1, 3, 3, 4, TRUE, FALSE),
(2, 1, 2, 0, FALSE, TRUE),
(2, 3, 2, 0, FALSE, TRUE);
