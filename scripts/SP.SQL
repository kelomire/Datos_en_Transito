USE datos_transito;

DROP PROCEDURE IF EXISTS sp_registrar_simulacion;

DELIMITER //

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

    INSERT INTO paquetes(
        ip_origen,
        ip_destino,
        tamano_bytes,
        ttl_inicial
    )
    VALUES(
        p_ip_origen,
        p_ip_destino,
        p_tamano,
        p_ttl
    );

    SET v_paquete = LAST_INSERT_ID();

    INSERT INTO simulaciones(
        paquete_id,
        latencia_total,
        ttl_final,
        entregado,
        descartado
    )
    VALUES(
        v_paquete,
        p_latencia,
        p_ttl,
        p_entregado,
        p_descartado
    );

    SET v_simulacion = LAST_INSERT_ID();

    COMMIT;

    SELECT v_simulacion AS id;
    
END//

DELIMITER ;


DROP PROCEDURE IF EXISTS sp_estadisticas;

DELIMITER //

CREATE PROCEDURE sp_estadisticas()
BEGIN
    START TRANSACTION;

    SELECT
        COUNT(*) AS total_simulaciones,
        SUM(entregado) AS entregadas,
        SUM(descartado) AS descartadas,
        AVG(latencia_total) AS latencia_promedio,
        MAX(latencia_total) AS latencia_maxima
    FROM simulaciones;

    COMMIT;
END//

DELIMITER ;
