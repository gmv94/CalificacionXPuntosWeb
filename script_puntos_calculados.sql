USE db36570;
GO

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

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosValorInversion')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosValorInversion;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosValorInversion DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosValorInversion DEFAULT 0 FOR PuntosValorInversion;
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosROI')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosROI;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosROI DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosROI DEFAULT 0 FOR PuntosROI;
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosFacilidadImplem')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosFacilidadImplem;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosFacilidadImplem DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosFacilidadImplem DEFAULT 0 FOR PuntosFacilidadImplem;
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosImpacto')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosImpacto;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosImpacto DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosImpacto DEFAULT 0 FOR PuntosImpacto;
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosTotales')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosTotales;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosTotales DECIMAL(18,2) NOT NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ADD CONSTRAINT DF_RegistrosCalificacion_PuntosTotales DEFAULT 0 FOR PuntosTotales;
GO

