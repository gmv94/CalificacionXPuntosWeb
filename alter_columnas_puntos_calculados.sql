-- Script para asegurar que las columnas de puntos calculados tengan valor por defecto 0
-- Ejecutar este script en la base de datos db36570

USE db36570;
GO

-- Primero, actualizar registros existentes que tengan NULL a 0
UPDATE dbo.RegistrosCalificacion
SET PuntosValorInversion = 0
WHERE PuntosValorInversion IS NULL;
GO

UPDATE dbo.RegistrosCalificacion
SET PuntosROI = 0
WHERE PuntosROI IS NULL;
GO

UPDATE dbo.RegistrosCalificacion
SET PuntosFacilidadImplem = 0
WHERE PuntosFacilidadImplem IS NULL;
GO

UPDATE dbo.RegistrosCalificacion
SET PuntosImpacto = 0
WHERE PuntosImpacto IS NULL;
GO

UPDATE dbo.RegistrosCalificacion
SET PuntosTotales = 0
WHERE PuntosTotales IS NULL;
GO

-- Ahora alterar las columnas para tener valor por defecto 0 y no permitir NULL
ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosValorInversion DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosValorInversion DEFAULT 0 FOR PuntosValorInversion;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosROI DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosROI DEFAULT 0 FOR PuntosROI;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosFacilidadImplem DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosFacilidadImplem DEFAULT 0 FOR PuntosFacilidadImplem;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosImpacto DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosImpacto DEFAULT 0 FOR PuntosImpacto;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosTotales DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosTotales DEFAULT 0 FOR PuntosTotales;
GO

PRINT 'Todas las columnas de puntos calculados ahora tienen valor por defecto 0 y no permiten NULL.';
GO

