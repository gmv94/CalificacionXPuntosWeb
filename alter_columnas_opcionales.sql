-- Script para alterar columnas opcionales en la tabla RegistrosCalificacion
-- Ejecutar este script en la base de datos para permitir NULL en campos opcionales

USE db36570;
GO

-- Verificar si las columnas existen y alterarlas para permitir NULL
-- Categoria (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'Categoria')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN Categoria NVARCHAR(100) NULL;
    PRINT 'Columna Categoria alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna Categoria no encontrada';
END
GO

-- Proceso (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'Proceso')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN Proceso NVARCHAR(100) NULL;
    PRINT 'Columna Proceso alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna Proceso no encontrada';
END
GO

-- Celular (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'Celular')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN Celular NVARCHAR(20) NULL;
    PRINT 'Columna Celular alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna Celular no encontrada';
END
GO

-- RoiMeses (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'RoiMeses')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN RoiMeses NVARCHAR(20) NULL;
    PRINT 'Columna RoiMeses alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna RoiMeses no encontrada';
END
GO

-- FacilidadImplem (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'FacilidadImplem')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN FacilidadImplem NVARCHAR(10) NULL;
    PRINT 'Columna FacilidadImplem alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna FacilidadImplem no encontrada';
END
GO

-- ValorInversion (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'ValorInversion')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN ValorInversion DECIMAL(18,2) NULL;
    PRINT 'Columna ValorInversion alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna ValorInversion no encontrada';
END
GO

-- PuntosExtra (opcional - ya debería estar como NULL, pero por si acaso)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'PuntosExtra')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN PuntosExtra DECIMAL(18,2) NULL;
    PRINT 'Columna PuntosExtra alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna PuntosExtra no encontrada';
END
GO

-- ComentariosPuntosExtra (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'ComentariosPuntosExtra')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN ComentariosPuntosExtra NVARCHAR(500) NULL;
    PRINT 'Columna ComentariosPuntosExtra alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna ComentariosPuntosExtra no encontrada';
END
GO

-- Observaciones (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'Observaciones')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN Observaciones NVARCHAR(1000) NULL;
    PRINT 'Columna Observaciones alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna Observaciones no encontrada';
END
GO

-- ImpactosJson (opcional)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'ImpactosJson')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ALTER COLUMN ImpactosJson NVARCHAR(4000) NULL;
    PRINT 'Columna ImpactosJson alterada para permitir NULL';
END
ELSE
BEGIN
    PRINT 'Columna ImpactosJson no encontrada';
END
GO

PRINT 'Script completado. Todas las columnas opcionales ahora permiten NULL.';
GO

