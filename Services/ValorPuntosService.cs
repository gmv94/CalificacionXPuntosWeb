using CalificacionXPuntosWeb.Data;
using CalificacionXPuntosWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class ValorPuntosService
    {
        private readonly ApplicationDbContext _context;

        public ValorPuntosService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ValorPuntos> GetAllValorPuntos()
        {
            try
            {
                // Obtener todos los registros sin filtrar por Activo
                var todos = _context.ValorPuntos
                    .OrderBy(v => v.CostoMinimo)
                    .ToList();
                return todos;
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error en GetAllValorPuntos: {ex.Message}");
                return new List<ValorPuntos>();
            }
        }

        public ValorPuntos? GetValorPuntosById(int id)
        {
            try
            {
                return _context.ValorPuntos.Find(id);
            }
            catch
            {
                return null;
            }
        }

        public ValorPuntos? GetValorPuntosPorCosto(decimal costo)
        {
            try
            {
                return _context.ValorPuntos
                    .Where(v => v.Activo && costo >= v.CostoMinimo && costo <= v.CostoMaximo)
                    .OrderByDescending(v => v.CostoMinimo)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public void AddValorPuntos(ValorPuntos valorPuntos)
        {
            try
            {
                valorPuntos.FechaCreacion = DateTime.Now;
                _context.ValorPuntos.Add(valorPuntos);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateValorPuntos(ValorPuntos valorPuntos)
        {
            try
            {
                var existing = _context.ValorPuntos.Find(valorPuntos.Id);
                if (existing != null)
                {
                    existing.CostoMinimo = valorPuntos.CostoMinimo;
                    existing.CostoMaximo = valorPuntos.CostoMaximo;
                    existing.ValorPorPunto = valorPuntos.ValorPorPunto;
                    existing.Activo = valorPuntos.Activo;
                    _context.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void DeleteValorPuntos(int id)
        {
            try
            {
                var valorPuntos = _context.ValorPuntos.Find(id);
                if (valorPuntos != null)
                {
                    _context.ValorPuntos.Remove(valorPuntos);
                    _context.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

