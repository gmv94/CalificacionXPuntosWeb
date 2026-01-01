USE db36570;
GO

-- Agregar columna TituloIdea a la tabla RegistrosCalificacion
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.RegistrosCalificacion') AND name = 'TituloIdea')
BEGIN
    ALTER TABLE dbo.RegistrosCalificacion
    ADD TituloIdea NVARCHAR(500) NULL;
    PRINT 'Columna TituloIdea agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna TituloIdea ya existe.';
END
GO

