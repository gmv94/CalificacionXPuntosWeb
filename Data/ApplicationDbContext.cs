using Microsoft.EntityFrameworkCore;
using CalificacionXPuntosWeb.Models;

namespace CalificacionXPuntosWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Idea> Ideas { get; set; }
        public DbSet<Premio> Premios { get; set; }
        public DbSet<Redencion> Redenciones { get; set; }
        public DbSet<CategoriaBD> Categorias { get; set; }
        public DbSet<ImpactoBD> Impactos { get; set; }
        public DbSet<EstadoBD> Estados { get; set; }
        public DbSet<ProcesoBD> Procesos { get; set; }
        public DbSet<PuntosHistoricos> PuntosHistoricos { get; set; }
        public DbSet<ValorPuntos> ValorPuntos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Idea - mapear a tabla RegistrosCalificacion
            modelBuilder.Entity<Idea>(entity =>
            {
                entity.ToTable("RegistrosCalificacion");
                entity.HasKey(e => e.Id);
                
                // Mapear columnas según el esquema existente de RegistrosCalificacion
                entity.Property(e => e.NumeroDocumento).HasColumnName("Documento").HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.NombreUsuario).HasColumnName("Nombre").HasMaxLength(200).IsRequired(false);
                entity.Property(e => e.Radicado).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.Categoria).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.Proceso).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.Estado).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.ValorInversion).HasColumnName("ValorInversion").HasColumnType("decimal(18,2)").IsRequired(false);
                entity.Property(e => e.RoiMeses).HasColumnName("RoiMeses").HasMaxLength(20).IsRequired(false);
                entity.Property(e => e.FacilidadImplem).HasColumnName("FacilidadImplem").HasMaxLength(10).IsRequired(false);
                entity.Property(e => e.PuntosExtra).HasColumnName("PuntosExtra").HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                entity.Property(e => e.ComentariosPuntosExtra).HasColumnName("ComentariosPuntosExtra").HasMaxLength(500).IsRequired(false);
                entity.Property(e => e.Observaciones).HasMaxLength(1000).IsRequired(false);
                entity.Property(e => e.PuntosValorInversion).HasColumnName("PuntosValorInversion").HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                entity.Property(e => e.PuntosROI).HasColumnName("PuntosROI").HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                entity.Property(e => e.PuntosFacilidadImplem).HasColumnName("PuntosFacilidadImplem").HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                entity.Property(e => e.PuntosImpacto).HasColumnName("PuntosImpacto").HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                entity.Property(e => e.PuntosTotales).HasColumnName("PuntosTotales").HasColumnType("decimal(18,2)").HasDefaultValue(0m);
                
                // Mapear FechaRegistro a FechaCreacion
                entity.Property(e => e.FechaRegistro).HasColumnName("FechaCreacion").IsRequired();
                
                // Mapear campos adicionales
                entity.Property(e => e.Celular).HasMaxLength(20).IsRequired(false);
                entity.Property(e => e.DescripcionIdea).HasColumnName("DescripcionIdea").HasMaxLength(4000).IsRequired(false);
                entity.Property(e => e.FechaRadicado).HasColumnName("FechaRadicado").HasColumnType("datetime2").IsRequired(false);
                entity.Property(e => e.ImpactosJson).HasColumnName("ImpactosJson").HasMaxLength(4000).IsRequired(false);
                
                // Ignorar campos calculados o no mapeables
                entity.Ignore(e => e.Puntos);
                entity.Ignore(e => e.Impactos);
            });

            // Configuración de Premio - mapear a tabla existente o crear nueva
            modelBuilder.Entity<Premio>(entity =>
            {
                entity.ToTable("Premios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // IDENTITY
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(1000).IsRequired(false);
                entity.Property(e => e.Costo).HasColumnType("decimal(18,2)").IsRequired().HasDefaultValue(0m);
                entity.Property(e => e.PuntosRequeridos).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.Stock).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.FechaCreacion).IsRequired().HasDefaultValueSql("GETDATE()");
            });

            // Configuración de ValorPuntos
            modelBuilder.Entity<ValorPuntos>(entity =>
            {
                entity.ToTable("ValorPuntos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CostoMinimo).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.CostoMaximo).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.ValorPorPunto).HasColumnType("decimal(18,2)").IsRequired();
            });

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContrasenaHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Rol).IsRequired().HasMaxLength(50).HasDefaultValue("Usuario");
                entity.Property(e => e.Activo).IsRequired().HasDefaultValue(true);
            });

            // Configuración de Redencion - mapear a tabla existente o crear nueva
            modelBuilder.Entity<Redencion>(entity =>
            {
                entity.ToTable("Redenciones");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NombrePremio).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Estado).HasMaxLength(50);
            });

            // Configuración de CategoriaBD
            modelBuilder.Entity<CategoriaBD>(entity =>
            {
                entity.ToTable("Categorias");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.HasMany(e => e.Impactos)
                    .WithOne(i => i.Categoria)
                    .HasForeignKey(i => i.CategoriaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de ImpactoBD
            modelBuilder.Entity<ImpactoBD>(entity =>
            {
                entity.ToTable("Impactos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PorcentajeMaximo).HasColumnType("decimal(5,2)");
            });

            // Configuración de EstadoBD
            modelBuilder.Entity<EstadoBD>(entity =>
            {
                entity.ToTable("Estados");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            });

            // Configuración de ProcesoBD
            modelBuilder.Entity<ProcesoBD>(entity =>
            {
                entity.ToTable("Procesos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            });

            // Configuración de PuntosHistoricos
            modelBuilder.Entity<PuntosHistoricos>(entity =>
            {
                entity.ToTable("PuntosHistoricos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Puntos).IsRequired();
                entity.Property(e => e.FechaRegistro).HasColumnName("FechaCreacion").IsRequired();
            });

            // Configuración de Log
            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("Logs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Entidad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Usuario).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Fecha).IsRequired();
                entity.Property(e => e.Detalles).HasMaxLength(2000);
            });
        }
    }
}

