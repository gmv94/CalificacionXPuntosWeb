-- Script para alterar la columna PuntosExtra y permitir NULL
-- Ejecutar este script directamente en la base de datos SQL Server

USE db36570;
GO

-- Alterar la columna PuntosExtra para permitir NULL
ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosExtra decimal(18,2) NULL;
GO

-- Verificar el cambio
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'RegistrosCalificacion' 
    AND COLUMN_NAME = 'PuntosExtra';
GO


