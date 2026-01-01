-- Script simple para alterar columnas opcionales en RegistrosCalificacion
-- Ejecutar este script completo en la base de datos db36570

USE db36570;
GO

-- Alterar todas las columnas opcionales para permitir NULL
ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN Categoria NVARCHAR(100) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN Proceso NVARCHAR(100) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN Celular NVARCHAR(20) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN RoiMeses NVARCHAR(20) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN FacilidadImplem NVARCHAR(10) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN ValorInversion DECIMAL(18,2) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN PuntosExtra DECIMAL(18,2) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN ComentariosPuntosExtra NVARCHAR(500) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN Observaciones NVARCHAR(1000) NULL;
GO

ALTER TABLE dbo.RegistrosCalificacion
ALTER COLUMN ImpactosJson NVARCHAR(4000) NULL;
GO

PRINT 'Todas las columnas opcionales han sido alteradas para permitir NULL.';
GO

