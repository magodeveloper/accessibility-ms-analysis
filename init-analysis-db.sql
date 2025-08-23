-- Script de inicialización para MySQL - Microservicio Analysis
-- Este script se ejecuta automáticamente cuando se crea el contenedor

-- Crear usuario adicional para operaciones de solo lectura
CREATE USER IF NOT EXISTS 'readonly_user'@'%' IDENTIFIED BY 'ReadOnly2025AnalysisPass';
GRANT SELECT ON analysisdb.* TO 'readonly_user'@'%';

-- Crear usuario para backup
CREATE USER IF NOT EXISTS 'backup_user'@'%' IDENTIFIED BY 'BackUp2025AnalysisPass';
GRANT SELECT, LOCK TABLES, SHOW VIEW ON analysisdb.* TO 'backup_user'@'%';

-- Crear usuario para monitoreo
CREATE USER IF NOT EXISTS 'monitor_user'@'%' IDENTIFIED BY 'Monitor2025AnalysisPass';
GRANT PROCESS ON *.* TO 'monitor_user'@'%';

-- Reforzar permisos del usuario principal de la aplicación
GRANT ALL PRIVILEGES ON analysisdb.* TO 'msuser'@'%';

FLUSH PRIVILEGES;

-- Crear tabla de logs de análisis si no existe
USE analysisdb;
CREATE TABLE IF NOT EXISTS analysis_logs (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    analysis_id VARCHAR(255),
    analysis_type VARCHAR(100),
    status VARCHAR(50),
    start_time DATETIME,
    end_time DATETIME,
    duration_ms INT,
    result_summary TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
