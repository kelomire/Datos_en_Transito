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

DELIMITER //
DROP PROCEDURE IF EXISTS sp_registrar_simulacion //
CREATE PROCEDURE sp_registrar_simulacion(
    IN p_ip_origen VARCHAR(45),
    IN p_ip_destino VARCHAR(45),
    IN p_tamano INT,
    IN p_ttl INT,
    IN p_latencia INT,
    IN p_entregado BOOLEAN,
    IN p_descartado BOOLEAN
)
BEGIN
    DECLARE v_paquete INT;
    DECLARE v_simulacion INT;

    START TRANSACTION;

    INSERT INTO paquetes(ip_origen, ip_destino, tamano_bytes, ttl_inicial)
    VALUES (p_ip_origen, p_ip_destino, p_tamano, p_ttl);

    SET v_paquete = LAST_INSERT_ID();

    INSERT INTO simulaciones(paquete_id, latencia_total, ttl_final, entregado, descartado)
    VALUES (v_paquete, p_latencia, p_ttl, p_entregado, p_descartado);

    SET v_simulacion = LAST_INSERT_ID();
    COMMIT;

    SELECT v_simulacion AS id;
END //

DROP PROCEDURE IF EXISTS sp_estadisticas //
CREATE PROCEDURE sp_estadisticas()
BEGIN
    SELECT COUNT(*) AS total_simulaciones,
           SUM(entregado) AS entregadas,
           SUM(descartado) AS descartadas,
           AVG(latencia_total) AS latencia_promedio,
           MAX(latencia_total) AS latencia_maxima
    FROM simulaciones;
END //

DROP PROCEDURE IF EXISTS sp_estadisticas_dispositivos //
CREATE PROCEDURE sp_estadisticas_dispositivos()
BEGIN
    SELECT d.tipo,
           COUNT(r.id) AS paquetes_procesados,
           AVG(r.latencia) AS latencia_promedio,
           MAX(r.latencia) AS latencia_maxima,
           SUM(r.entregado) AS entregados,
           SUM(r.descartado) AS descartados
    FROM dispositivos d
    LEFT JOIN recorridos r ON d.id = r.dispositivo_id
    GROUP BY d.tipo;
END //
DELIMITER ;
