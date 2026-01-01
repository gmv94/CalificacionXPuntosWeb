using CalificacionXPuntosWeb.Data;
using CalificacionXPuntosWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class LogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void RegistrarLog(string tipo, string entidad, int? entidadId, string usuario, string descripcion, string? detalles = null)
        {
            try
            {
                var log = new Log
                {
                    Tipo = tipo,
                    Entidad = entidad,
                    EntidadId = entidadId,
                    Usuario = usuario,
                    Descripcion = descripcion,
                    Fecha = DateTime.UtcNow,
                    Detalles = detalles
                };
                _context.Logs.Add(log);
                _context.SaveChanges();
            }
            catch
            {
                // Silenciar errores de logging para no afectar la funcionalidad principal
            }
        }

        public List<Log> GetAllLogs()
        {
            try
            {
                return _context.Logs.OrderByDescending(l => l.Fecha).ToList();
            }
            catch
            {
                return new List<Log>();
            }
        }

        public List<Log> GetLogsPorTipo(string tipo)
        {
            try
            {
                return _context.Logs.Where(l => l.Tipo == tipo).OrderByDescending(l => l.Fecha).ToList();
            }
            catch
            {
                return new List<Log>();
            }
        }

        public List<Log> GetLogsPorUsuario(string usuario)
        {
            try
            {
                return _context.Logs.Where(l => l.Usuario == usuario).OrderByDescending(l => l.Fecha).ToList();
            }
            catch
            {
                return new List<Log>();
            }
        }
    }
}

