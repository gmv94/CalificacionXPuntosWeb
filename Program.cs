using CalificacionXPuntosWeb.Services;
using CalificacionXPuntosWeb.Data;
using CalificacionXPuntosWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.MaxBufferedUnacknowledgedRenderBatches = 20;
});
builder.Services.AddHttpClient();
builder.Services.Configure<HubOptions>(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024;
});

// Configurar Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configurar servicios con Entity Framework
builder.Services.AddScoped<IdeaService>(sp =>
{
    var context = sp.GetRequiredService<ApplicationDbContext>();
    var puntosHistoricosService = sp.GetRequiredService<PuntosHistoricosService>();
    return new IdeaService(context, puntosHistoricosService);
});
builder.Services.AddScoped<PremioService>();
builder.Services.AddScoped<RedencionService>(sp =>
{
    var ideaService = sp.GetRequiredService<IdeaService>();
    var premioService = sp.GetRequiredService<PremioService>();
    var context = sp.GetRequiredService<ApplicationDbContext>();
    return new RedencionService(ideaService, premioService, context);
});
builder.Services.AddScoped<CategoriaService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new CategoriaService(httpClient, navigationManager);
});
builder.Services.AddScoped<CategoriaBDService>();
builder.Services.AddScoped<EstadoBDService>();
builder.Services.AddScoped<ProcesoBDService>();
builder.Services.AddScoped<PuntosHistoricosService>();
builder.Services.AddScoped<ValorPuntosService>();
builder.Services.AddScoped<AuthService>(sp =>
{
    var context = sp.GetRequiredService<ApplicationDbContext>();
    var authService = new AuthService(context);
    var logService = sp.GetRequiredService<LogService>();
    authService.SetLogService(logService);
    return authService;
});
builder.Services.AddScoped<LogService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// En Cloud Run, no redirigir HTTPS automáticamente
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// Verificar conexión y crear tablas que no existen (Premios y Redenciones)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Verificando conexión a la base de datos...");
        
        // SIEMPRE intentar crear la base de datos y tablas si no existen
        try
        {
            logger.LogInformation("Ejecutando EnsureCreated() para crear base de datos y tablas...");
            dbContext.Database.EnsureCreated();
            logger.LogInformation("EnsureCreated() completado exitosamente.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error en EnsureCreated(): {ex.Message}");
            logger.LogError($"Stack trace: {ex.StackTrace}");
            // Continuar para intentar crear las tablas manualmente
        }
        
        // Verificar conexión después de EnsureCreated
        var canConnect = false;
        try
        {
            canConnect = dbContext.Database.CanConnect();
            if (canConnect)
            {
                logger.LogInformation("Conexión a la base de datos exitosa después de EnsureCreated().");
            }
            else
            {
                logger.LogWarning("No se pudo conectar a la base de datos después de EnsureCreated().");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error al verificar conexión después de EnsureCreated(): {ex.Message}");
        }
        
        // Continuar con la creación manual de tablas si es necesario
        if (canConnect)
        {
            logger.LogInformation("Procediendo a crear/verificar tablas individuales...");
            
            // Intentar crear solo las tablas que no existen (Premios y Redenciones)
            // RegistrosCalificacion ya existe, no la tocamos
            try
            {
                // Verificar si existe la tabla Premios usando una consulta simple
                var premiosExists = false;
                try
                {
                    dbContext.Database.ExecuteSqlRaw("SELECT TOP 1 Id FROM Premios");
                    premiosExists = true;
                }
                catch
                {
                    premiosExists = false;
                }
                
                // Si no existe, crearla
                if (!premiosExists)
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Premios')
                        BEGIN
                            CREATE TABLE Premios (
                                Id int IDENTITY(1,1) PRIMARY KEY,
                                Nombre nvarchar(200) NOT NULL,
                                Descripcion nvarchar(1000),
                                PuntosRequeridos int NOT NULL DEFAULT 0,
                                Stock int NOT NULL DEFAULT 0,
                                Activo bit NOT NULL DEFAULT 1,
                                FechaCreacion datetime2 NOT NULL DEFAULT GETDATE(),
                                Costo decimal(18,2) NOT NULL DEFAULT 0
                            )
                        END");
                    logger.LogInformation("Tabla Premios creada o ya existe.");
                }
                
                // Siempre verificar y agregar la columna Costo si no existe (incluso si la tabla ya existía)
                try
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Premios')
                        BEGIN
                            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Premios' AND COLUMN_NAME = 'Costo')
                            BEGIN
                                ALTER TABLE Premios ADD Costo decimal(18,2) NOT NULL DEFAULT 0
                            END
                        END");
                    logger.LogInformation("Columna Costo verificada/agregada en Premios.");
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"No se pudo agregar columna Costo a Premios: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla Premios: {ex.Message}");
            }
            
            try
            {
                // Verificar si existe la tabla Redenciones
                var redencionesExists = false;
                try
                {
                    dbContext.Database.ExecuteSqlRaw("SELECT TOP 1 Id FROM Redenciones");
                    redencionesExists = true;
                }
                catch
                {
                    redencionesExists = false;
                }
                
                // Si no existe, crearla
                if (!redencionesExists)
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Redenciones')
                        CREATE TABLE Redenciones (
                            Id int IDENTITY(1,1) PRIMARY KEY,
                            NumeroDocumento nvarchar(50) NOT NULL,
                            NombreUsuario nvarchar(200) NOT NULL,
                            PremioId int NOT NULL,
                            NombrePremio nvarchar(200) NOT NULL,
                            PuntosUtilizados int NOT NULL,
                            FechaRedencion datetime2 NOT NULL DEFAULT GETDATE(),
                            Estado nvarchar(50) NOT NULL DEFAULT 'Redimido'
                        )");
                    logger.LogInformation("Tabla Redenciones creada o ya existe.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla Redenciones: {ex.Message}");
            }
            
            // Agregar columnas faltantes a RegistrosCalificacion si no existen
            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RegistrosCalificacion')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RegistrosCalificacion' AND COLUMN_NAME = 'Celular')
                        BEGIN
                            ALTER TABLE RegistrosCalificacion ADD Celular nvarchar(20) NULL
                        END
                        
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RegistrosCalificacion' AND COLUMN_NAME = 'DescripcionIdea')
                        BEGIN
                            ALTER TABLE RegistrosCalificacion ADD DescripcionIdea nvarchar(4000) NULL
                        END
                        
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RegistrosCalificacion' AND COLUMN_NAME = 'FechaRadicado')
                        BEGIN
                            ALTER TABLE RegistrosCalificacion ADD FechaRadicado datetime2 NULL
                        END
                        
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RegistrosCalificacion' AND COLUMN_NAME = 'ImpactosJson')
                        BEGIN
                            ALTER TABLE RegistrosCalificacion ADD ImpactosJson nvarchar(4000) NULL
                        END
                    END");
                logger.LogInformation("Columnas adicionales verificadas/agregadas en RegistrosCalificacion.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudieron agregar columnas a RegistrosCalificacion: {ex.Message}");
            }

            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categorias')
                    CREATE TABLE Categorias (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Nombre nvarchar(200) NOT NULL,
                        Activo bit NOT NULL DEFAULT 1,
                        FechaCreacion datetime2 NOT NULL DEFAULT GETDATE()
                    )");
                
                // Verificar y agregar columna Activo si no existe
                try
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                       WHERE TABLE_NAME = 'Categorias' AND COLUMN_NAME = 'Activo')
                        ALTER TABLE Categorias ADD Activo bit NOT NULL DEFAULT 1");
                }
                catch { }
                
                // Verificar y agregar columna FechaCreacion si no existe
                try
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                       WHERE TABLE_NAME = 'Categorias' AND COLUMN_NAME = 'FechaCreacion')
                        ALTER TABLE Categorias ADD FechaCreacion datetime2 NOT NULL DEFAULT GETDATE()");
                }
                catch { }
                
                logger.LogInformation("Tabla Categorias verificada.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla Categorias: {ex.Message}");
            }

            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Impactos')
                    CREATE TABLE Impactos (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        CategoriaId int NOT NULL,
                        Nombre nvarchar(200) NOT NULL,
                        PorcentajeMaximo decimal(5,2) NOT NULL,
                        Orden int NOT NULL DEFAULT 0,
                        Activo bit NOT NULL DEFAULT 1,
                        FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id) ON DELETE CASCADE
                    )");
                logger.LogInformation("Tabla Impactos verificada.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla Impactos: {ex.Message}");
            }

            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Estados')
                    CREATE TABLE Estados (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Nombre nvarchar(100) NOT NULL,
                        Activo bit NOT NULL DEFAULT 1,
                        FechaCreacion datetime2 NOT NULL DEFAULT GETDATE()
                    )");
                logger.LogInformation("Tabla Estados verificada.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla Estados: {ex.Message}");
            }

            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Procesos')
                    CREATE TABLE Procesos (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Nombre nvarchar(100) NOT NULL,
                        Activo bit NOT NULL DEFAULT 1,
                        FechaCreacion datetime2 NOT NULL DEFAULT GETDATE()
                    )");
                logger.LogInformation("Tabla Procesos verificada.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla Procesos: {ex.Message}");
            }

            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PuntosHistoricos')
                    CREATE TABLE PuntosHistoricos (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        NumeroDocumento nvarchar(50) NOT NULL,
                        NombreUsuario nvarchar(200) NOT NULL,
                        Puntos int NOT NULL,
                        Observaciones nvarchar(1000),
                        FechaCreacion datetime2 NOT NULL DEFAULT GETDATE()
                    )");
                logger.LogInformation("Tabla PuntosHistoricos verificada.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla PuntosHistoricos: {ex.Message}");
            }

            // Crear tabla ValorPuntos si no existe
            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ValorPuntos')
                    CREATE TABLE ValorPuntos (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        CostoMinimo decimal(18,2) NOT NULL,
                        CostoMaximo decimal(18,2) NOT NULL,
                        ValorPorPunto decimal(18,2) NOT NULL,
                        Activo bit NOT NULL DEFAULT 1,
                        FechaCreacion datetime2 NOT NULL DEFAULT GETDATE()
                    )");
                logger.LogInformation("Tabla ValorPuntos verificada.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla ValorPuntos: {ex.Message}");
            }

            // Crear tabla Usuarios si no existe y crear usuario SuperAdmin inicial
            try
            {
                var usuariosExists = false;
                try
                {
                    dbContext.Database.ExecuteSqlRaw("SELECT TOP 1 Id FROM Usuarios");
                    usuariosExists = true;
                }
                catch
                {
                    usuariosExists = false;
                }

                if (!usuariosExists)
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuarios')
                        CREATE TABLE Usuarios (
                            Id int IDENTITY(1,1) PRIMARY KEY,
                            NombreUsuario nvarchar(50) NOT NULL UNIQUE,
                            ContrasenaHash nvarchar(128) NOT NULL,
                            Rol nvarchar(20) NOT NULL,
                            Activo bit NOT NULL DEFAULT 1,
                            FechaCreacion datetime2 NOT NULL DEFAULT GETDATE(),
                            UltimoAcceso datetime2 NULL
                        )");
                    logger.LogInformation("Tabla Usuarios creada.");
                }
                else
                {
                    logger.LogInformation("Tabla Usuarios ya existe.");
                }

                // Verificar si existe el usuario admin usando el servicio AuthService
                try
                {
                    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
                    var logService = scope.ServiceProvider.GetRequiredService<LogService>();
                    authService.SetLogService(logService);
                    
                    // Verificar si existe el usuario admin
                    var adminUsuario = dbContext.Usuarios.FirstOrDefault(u => u.NombreUsuario == "admin");
                    if (adminUsuario == null)
                    {
                        // Crear usuario SuperAdmin usando el servicio
                        var nuevoUsuario = new Usuario
                        {
                            NombreUsuario = "admin",
                            Rol = "SuperAdmin",
                            Activo = true
                        };
                        if (authService.RegistrarUsuario(nuevoUsuario, "admin"))
                        {
                            logger.LogInformation("Usuario SuperAdmin inicial creado (admin/admin).");
                        }
                        else
                        {
                            throw new Exception("No se pudo crear el usuario usando el servicio");
                        }
                    }
                    else
                    {
                        logger.LogInformation("Usuario SuperAdmin ya existe.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Error al crear usuario SuperAdmin con servicio: {ex.Message}");
                    // Intentar crear directamente si el servicio falla
                    try
                    {
                        var adminUsuario = dbContext.Usuarios.FirstOrDefault(u => u.NombreUsuario == "admin");
                        if (adminUsuario == null)
                        {
                            using (var sha256 = System.Security.Cryptography.SHA256.Create())
                            {
                                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("admin"));
                                var hashBase64 = Convert.ToBase64String(hashedBytes);
                                
                                // Usar FormattableStringFactory para evitar SQL injection
                                var sql = FormattableStringFactory.Create(
                                    "INSERT INTO Usuarios (NombreUsuario, ContrasenaHash, Rol, Activo, FechaCreacion) VALUES ('admin', {0}, 'SuperAdmin', 1, GETDATE())",
                                    hashBase64
                                );
                                dbContext.Database.ExecuteSql(sql);
                                logger.LogInformation("Usuario SuperAdmin creado directamente (admin/admin).");
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
                        logger.LogError($"Error al crear usuario SuperAdmin directamente: {ex2.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla Usuarios: {ex.Message}");
            }

            // Crear tabla Logs si no existe
            try
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Logs')
                    BEGIN
                        CREATE TABLE Logs (
                            Id int IDENTITY(1,1) PRIMARY KEY,
                            Tipo nvarchar(50) NOT NULL,
                            Entidad nvarchar(100) NOT NULL,
                            EntidadId int NULL,
                            Usuario nvarchar(100) NOT NULL,
                            Descripcion nvarchar(500) NOT NULL,
                            Fecha datetime2 NOT NULL DEFAULT GETDATE(),
                            Detalles nvarchar(2000) NULL
                        )
                    END");
                logger.LogInformation("Tabla Logs creada o ya existe.");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"No se pudo crear/verificar tabla Logs: {ex.Message}");
            }
        }
        else
        {
            logger.LogWarning("No se pudo conectar a la base de datos.");
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError($"Error al verificar la base de datos: {ex.Message}");
    // Continuar de todas formas para que la app pueda iniciar
}

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

