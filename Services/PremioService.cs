using CalificacionXPuntosWeb.Models;
using CalificacionXPuntosWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class PremioService
    {
        private readonly ApplicationDbContext _context;

        public PremioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Premio> GetAllPremios()
        {
            try
            {
                return _context.Premios.ToList();
            }
            catch
            {
                return new List<Premio>();
            }
        }

        public Premio? GetPremioById(int id)
        {
            try
            {
                return _context.Premios.FirstOrDefault(p => p.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public void AddPremio(Premio premio)
        {
            try
            {
                // Asegurar que FechaCreacion tenga un valor
                if (premio.FechaCreacion == default(DateTime))
                {
                    premio.FechaCreacion = DateTime.UtcNow;
                }
                
                // Asegurar valores por defecto
                if (premio.Costo < 0)
                {
                    premio.Costo = 0;
                }
                if (premio.PuntosRequeridos < 0)
                {
                    premio.PuntosRequeridos = 0;
                }
                if (premio.Stock < 0)
                {
                    premio.Stock = 0;
                }
                
                // Asegurar que Descripcion no sea null
                if (premio.Descripcion == null)
                {
                    premio.Descripcion = string.Empty;
                }
                
                // Asegurar que el Id sea 0 para nuevos registros (IDENTITY)
                var premioParaGuardar = new Premio
                {
                    Id = 0, // Forzar a 0 para que SQL Server genere el IDENTITY
                    Nombre = premio.Nombre,
                    Descripcion = premio.Descripcion ?? string.Empty,
                    Costo = premio.Costo,
                    PuntosRequeridos = premio.PuntosRequeridos,
                    Stock = premio.Stock,
                    Activo = premio.Activo,
                    FechaCreacion = premio.FechaCreacion == default(DateTime) ? DateTime.UtcNow : premio.FechaCreacion
                };
                
                _context.Premios.Add(premioParaGuardar);
                _context.SaveChanges();
                
                // Actualizar el ID en el objeto original
                premio.Id = premioParaGuardar.Id;
            }
            catch (DbUpdateException dbEx)
            {
                var innerEx = dbEx.InnerException;
                var errorMessage = $"Error al agregar premio: {dbEx.Message}";
                if (innerEx != null)
                {
                    errorMessage += $" Detalles internos: {innerEx.Message}";
                }
                throw new Exception(errorMessage, dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar premio: {ex.Message}", ex);
            }
        }

        public void UpdatePremio(Premio premio)
        {
            try
            {
                var existing = _context.Premios.FirstOrDefault(p => p.Id == premio.Id);
                if (existing != null)
                {
                    existing.Nombre = premio.Nombre;
                    existing.Descripcion = premio.Descripcion;
                    existing.Costo = premio.Costo;
                    existing.PuntosRequeridos = premio.PuntosRequeridos;
                    existing.Stock = premio.Stock;
                    existing.Activo = premio.Activo;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar premio: {ex.Message}", ex);
            }
        }

        public void DeletePremio(int id)
        {
            var premio = _context.Premios.FirstOrDefault(p => p.Id == id);
            if (premio != null)
            {
                _context.Premios.Remove(premio);
                _context.SaveChanges();
            }
        }

        public List<Premio> GetPremiosDisponibles()
        {
            try
            {
                return _context.Premios.Where(p => p.Activo && p.Stock > 0).ToList();
            }
            catch
            {
                return new List<Premio>();
            }
        }

        public int EliminarTodosLosPremios()
        {
            try
            {
                // Usar SQL directo para eliminar todos los registros sin WHERE
                var registrosEliminados = _context.Database.ExecuteSqlRaw("DELETE FROM Premios");
                return registrosEliminados;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar todos los premios: {ex.Message}", ex);
            }
        }

        public int ContarPremios()
        {
            try
            {
                return _context.Premios.Count();
            }
            catch
            {
                return 0;
            }
        }
    }
}

