USE datos_transito;

DROP PROCEDURE IF EXISTS sp_estadisticas_dispositivos;

DELIMITER //

CREATE PROCEDURE sp_estadisticas_dispositivos()
BEGIN
    START TRANSACTION;

    SELECT
        d.tipo,
        COUNT(r.id) AS paquetes_procesados,
        AVG(r.latencia) AS latencia_promedio,
        MAX(r.latencia) AS latencia_maxima,
        SUM(r.entregado) AS entregados,
        SUM(r.descartado) AS descartados
    FROM dispositivos d
    LEFT JOIN recorridos r
        ON d.id = r.dispositivo_id
    GROUP BY d.tipo;

    COMMIT;
END//

DELIMITER ;
