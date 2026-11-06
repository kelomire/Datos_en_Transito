-- MySQL dump 10.13  Distrib 8.0.46, for Linux (x86_64)
--
-- Host: localhost    Database: datos_transito
-- ------------------------------------------------------
-- Server version	8.0.46-0ubuntu0.22.04.3

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `datos_transito`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `datos_transito` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `datos_transito`;

--
-- Table structure for table `dispositivos`
--

DROP TABLE IF EXISTS `dispositivos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dispositivos` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) NOT NULL,
  `tipo` varchar(50) NOT NULL,
  `latenciaMs` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dispositivos`
--

LOCK TABLES `dispositivos` WRITE;
/*!40000 ALTER TABLE `dispositivos` DISABLE KEYS */;
/*!40000 ALTER TABLE `dispositivos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `paquetes`
--

DROP TABLE IF EXISTS `paquetes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `paquetes` (
  `id` int NOT NULL AUTO_INCREMENT,
  `ip_origen` varchar(45) NOT NULL,
  `ip_destino` varchar(45) NOT NULL,
  `tamano_bytes` int NOT NULL,
  `ttl_inicial` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `paquetes`
--

LOCK TABLES `paquetes` WRITE;
/*!40000 ALTER TABLE `paquetes` DISABLE KEYS */;
/*!40000 ALTER TABLE `paquetes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `recorridos`
--

DROP TABLE IF EXISTS `recorridos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `recorridos` (
  `id` int NOT NULL AUTO_INCREMENT,
  `simulacion_id` int NOT NULL,
  `dispositivo_id` int NOT NULL,
  `latenciaMs` int NOT NULL,
  `ttl_final` int NOT NULL,
  `entregado` tinyint(1) NOT NULL,
  `descartado` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `simulacion_id` (`simulacion_id`),
  KEY `dispositivo_id` (`dispositivo_id`),
  CONSTRAINT `recorridos_ibfk_1` FOREIGN KEY (`simulacion_id`) REFERENCES `simulaciones` (`id`),
  CONSTRAINT `recorridos_ibfk_2` FOREIGN KEY (`dispositivo_id`) REFERENCES `dispositivos` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `recorridos`
--

LOCK TABLES `recorridos` WRITE;
/*!40000 ALTER TABLE `recorridos` DISABLE KEYS */;
/*!40000 ALTER TABLE `recorridos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `simulaciones`
--

DROP TABLE IF EXISTS `simulaciones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `simulaciones` (
  `id` int NOT NULL AUTO_INCREMENT,
  `paquete_id` int DEFAULT NULL,
  `latenciaMs_total` int NOT NULL DEFAULT '0',
  `ttl_final` int NOT NULL,
  `entregado` tinyint(1) NOT NULL,
  `descartado` tinyint(1) NOT NULL,
  `fecha` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `paquete_id` (`paquete_id`),
  CONSTRAINT `simulaciones_ibfk_1` FOREIGN KEY (`paquete_id`) REFERENCES `paquetes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `simulaciones`
--

LOCK TABLES `simulaciones` WRITE;
/*!40000 ALTER TABLE `simulaciones` DISABLE KEYS */;
/*!40000 ALTER TABLE `simulaciones` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'datos_transito'
--
/*!50003 DROP PROCEDURE IF EXISTS `sp_estadisticas` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_unicode_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`5to_agbd`@`localhost` PROCEDURE `sp_estadisticas`()
BEGIN
    START TRANSACTION;
    SELECT
        COUNT(*) AS total_simulaciones,
        SUM(entregado) AS entregadas,
        SUM(descartado) AS descartadas,
        AVG(latenciaMs_total) AS latenciaMs_promedio,
        MAX(latenciaMs_total) AS latenciaMs_maxima
    FROM simulaciones;
    COMMIT;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_estadisticas_dispositivos` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_unicode_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`5to_agbd`@`localhost` PROCEDURE `sp_estadisticas_dispositivos`()
BEGIN
    START TRANSACTION;
    SELECT
        d.tipo,
        COUNT(r.id) AS paquetes_procesados,
        AVG(r.latenciaMs) AS latenciaMs_promedio,
        MAX(r.latenciaMs) AS latenciaMs_maxima,
        SUM(r.entregado) AS entregados,
        SUM(r.descartado) AS descartados
    FROM dispositivos d
    LEFT JOIN recorridos r
        ON d.id = r.dispositivo_id
    GROUP BY d.tipo;
    COMMIT;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_registrar_simulacion` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_unicode_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`5to_agbd`@`localhost` PROCEDURE `sp_registrar_simulacion`(
    IN p_ip_origen VARCHAR(45),
    IN p_ip_destino VARCHAR(45),
    IN p_tamano INT,
    IN p_ttl INT,
    IN p_latenciaMs INT,
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
        latenciaMs_total,
        ttl_final,
        entregado,
        descartado
    )
    VALUES(
        v_paquete,
        p_latenciaMs,
        p_ttl,
        p_entregado,
        p_descartado
    );
    SET v_simulacion = LAST_INSERT_ID();
    COMMIT;
    SELECT v_simulacion AS id;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-06 10:13:40
