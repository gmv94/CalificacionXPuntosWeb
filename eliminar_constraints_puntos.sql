-- Script para eliminar constraints de valores por defecto existentes (si existen)
-- Ejecutar este script ANTES de alter_columnas_puntos_calculados.sql si hay errores

USE db36570;
GO

-- Eliminar constraints si existen
IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosValorInversion')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosValorInversion;
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosROI')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosROI;
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosFacilidadImplem')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosFacilidadImplem;
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosImpacto')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosImpacto;
GO

IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_RegistrosCalificacion_PuntosTotales')
    ALTER TABLE dbo.RegistrosCalificacion DROP CONSTRAINT DF_RegistrosCalificacion_PuntosTotales;
GO

PRINT 'Constraints eliminados (si existían).';
GO

