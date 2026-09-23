CREATE DATABASE IF NOT EXISTS datos_transito;
USE datos_transito;

CREATE TABLE IF NOT EXISTS dispositivos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    latencia INT NOT NULL
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
    latencia_total INT NOT NULL DEFAULT 0,
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
    latencia INT NOT NULL,
    ttl_final INT NOT NULL,
    entregado BOOLEAN NOT NULL,
    descartado BOOLEAN NOT NULL,
    FOREIGN KEY (simulacion_id) REFERENCES simulaciones(id),
    FOREIGN KEY (dispositivo_id) REFERENCES dispositivos(id)
);
