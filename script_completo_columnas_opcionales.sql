USE db36570;
GO

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

