using CalificacionXPuntosWeb.Models;
using CalificacionXPuntosWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class EstadoBDService
    {
        private readonly ApplicationDbContext _context;

        public EstadoBDService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<EstadoBD> GetAllEstados()
        {
            try
            {
                return _context.Estados
                    .OrderBy(e => e.Nombre)
                    .ToList();
            }
            catch
            {
                return new List<EstadoBD>();
            }
        }

        public EstadoBD? GetEstadoById(int id)
        {
            try
            {
                return _context.Estados.FirstOrDefault(e => e.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public void AddEstado(EstadoBD estado)
        {
            _context.Estados.Add(estado);
            _context.SaveChanges();
        }

        public void UpdateEstado(EstadoBD estado)
        {
            var existing = _context.Estados.FirstOrDefault(e => e.Id == estado.Id);
            if (existing != null)
            {
                existing.Nombre = estado.Nombre;
                existing.Activo = estado.Activo;
                _context.SaveChanges();
            }
        }

        public void DeleteEstado(int id)
        {
            var estado = _context.Estados.FirstOrDefault(e => e.Id == id);
            if (estado != null)
            {
                _context.Estados.Remove(estado);
                _context.SaveChanges();
            }
        }

        public List<string> GetNombresEstados()
        {
            try
            {
                return _context.Estados
                    .OrderBy(e => e.Nombre)
                    .Select(e => e.Nombre)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}

