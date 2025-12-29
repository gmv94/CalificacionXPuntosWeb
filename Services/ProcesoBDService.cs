using CalificacionXPuntosWeb.Models;
using CalificacionXPuntosWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class ProcesoBDService
    {
        private readonly ApplicationDbContext _context;

        public ProcesoBDService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ProcesoBD> GetAllProcesos()
        {
            try
            {
                return _context.Procesos
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }
            catch
            {
                return new List<ProcesoBD>();
            }
        }

        public ProcesoBD? GetProcesoById(int id)
        {
            try
            {
                return _context.Procesos.FirstOrDefault(p => p.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public void AddProceso(ProcesoBD proceso)
        {
            _context.Procesos.Add(proceso);
            _context.SaveChanges();
        }

        public void UpdateProceso(ProcesoBD proceso)
        {
            var existing = _context.Procesos.FirstOrDefault(p => p.Id == proceso.Id);
            if (existing != null)
            {
                existing.Nombre = proceso.Nombre;
                existing.Activo = proceso.Activo;
                _context.SaveChanges();
            }
        }

        public void DeleteProceso(int id)
        {
            var proceso = _context.Procesos.FirstOrDefault(p => p.Id == id);
            if (proceso != null)
            {
                _context.Procesos.Remove(proceso);
                _context.SaveChanges();
            }
        }

        public List<string> GetNombresProcesos()
        {
            try
            {
                return _context.Procesos
                    .OrderBy(p => p.Nombre)
                    .Select(p => p.Nombre)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}

