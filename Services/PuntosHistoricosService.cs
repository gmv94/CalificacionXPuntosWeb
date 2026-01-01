using CalificacionXPuntosWeb.Data;
using CalificacionXPuntosWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class PuntosHistoricosService
    {
        private readonly ApplicationDbContext _context;

        public PuntosHistoricosService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PuntosHistoricos> GetAllPuntosHistoricos()
        {
            try
            {
                return _context.PuntosHistoricos
                    .OrderByDescending(p => p.FechaRegistro)
                    .ToList();
            }
            catch
            {
                return new List<PuntosHistoricos>();
            }
        }

        public List<PuntosHistoricos> GetPuntosHistoricosPorDocumento(string numeroDocumento)
        {
            try
            {
                return _context.PuntosHistoricos
                    .Where(p => p.NumeroDocumento == numeroDocumento)
                    .OrderByDescending(p => p.FechaRegistro)
                    .ToList();
            }
            catch
            {
                return new List<PuntosHistoricos>();
            }
        }

        public int GetTotalPuntosHistoricosPorDocumento(string numeroDocumento)
        {
            try
            {
                return _context.PuntosHistoricos
                    .Where(p => p.NumeroDocumento == numeroDocumento)
                    .Sum(p => p.Puntos);
            }
            catch
            {
                return 0;
            }
        }

        public void AddPuntosHistoricos(PuntosHistoricos puntosHistoricos)
        {
            try
            {
                puntosHistoricos.FechaRegistro = DateTime.UtcNow;
                _context.PuntosHistoricos.Add(puntosHistoricos);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdatePuntosHistoricos(PuntosHistoricos puntosHistoricos)
        {
            try
            {
                var existing = _context.PuntosHistoricos.Find(puntosHistoricos.Id);
                if (existing != null)
                {
                    existing.NumeroDocumento = puntosHistoricos.NumeroDocumento;
                    existing.NombreUsuario = puntosHistoricos.NombreUsuario;
                    existing.Puntos = puntosHistoricos.Puntos;
                    existing.Observaciones = puntosHistoricos.Observaciones;
                    _context.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void DeletePuntosHistoricos(int id)
        {
            try
            {
                var puntosHistoricos = _context.PuntosHistoricos.Find(id);
                if (puntosHistoricos != null)
                {
                    _context.PuntosHistoricos.Remove(puntosHistoricos);
                    _context.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public PuntosHistoricos? GetPuntosHistoricosById(int id)
        {
            try
            {
                return _context.PuntosHistoricos.Find(id);
            }
            catch
            {
                return null;
            }
        }
    }
}

