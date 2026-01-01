-- Script para alterar solo la columna Proceso
-- Ejecutar este script si solo necesitas corregir la columna Proceso

USE db36570;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN Proceso NVARCHAR(100) NULL;
GO

PRINT 'Columna Proceso alterada para permitir NULL.';
GO

