using CalificacionXPuntosWeb.Models;
using CalificacionXPuntosWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class RedencionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IdeaService _ideaService;
        private readonly PremioService _premioService;

        public RedencionService(IdeaService ideaService, PremioService premioService, ApplicationDbContext context)
        {
            _ideaService = ideaService;
            _premioService = premioService;
            _context = context;
        }

        public List<Redencion> GetAllRedenciones()
        {
            try
            {
                return _context.Redenciones.ToList();
            }
            catch
            {
                return new List<Redencion>();
            }
        }

        public List<Redencion> GetRedencionesPorDocumento(string numeroDocumento)
        {
            try
            {
                return _context.Redenciones.Where(r => r.NumeroDocumento == numeroDocumento).ToList();
            }
            catch
            {
                return new List<Redencion>();
            }
        }

        public Redencion? GetRedencionById(int id)
        {
            try
            {
                return _context.Redenciones.FirstOrDefault(r => r.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public (bool Success, string Message) RedimirPremio(string numeroDocumento, int premioId)
        {
            var puntosAcumulados = _ideaService.GetPuntosAcumuladosPorDocumento(numeroDocumento);
            if (puntosAcumulados == null)
            {
                return (false, "No se encontraron puntos acumulados para este documento.");
            }

            var premio = _premioService.GetPremioById(premioId);
            if (premio == null)
            {
                return (false, "El premio no existe.");
            }

            if (!premio.Activo)
            {
                return (false, "El premio no está disponible.");
            }

            if (premio.Stock <= 0)
            {
                return (false, "El premio no tiene stock disponible.");
            }

            // Calcular puntos disponibles (total - utilizados en redenciones)
            var redencionesUsuario = 0;
            try
            {
                redencionesUsuario = _context.Redenciones
                    .Where(r => r.NumeroDocumento == numeroDocumento)
                    .Sum(r => r.PuntosUtilizados);
            }
            catch
            {
                redencionesUsuario = 0;
            }

            var puntosDisponibles = puntosAcumulados.TotalPuntos - redencionesUsuario;

            if (puntosDisponibles < premio.PuntosRequeridos)
            {
                return (false, $"No tiene suficientes puntos. Necesita {premio.PuntosRequeridos} puntos y tiene {puntosDisponibles} disponibles.");
            }

            // Crear la redención
            var redencion = new Redencion
            {
                NumeroDocumento = numeroDocumento,
                NombreUsuario = puntosAcumulados.NombreUsuario,
                PremioId = premioId,
                NombrePremio = premio.Nombre,
                PuntosUtilizados = premio.PuntosRequeridos,
                FechaRedencion = DateTime.UtcNow,
                Estado = "Redimido"
            };

            _context.Redenciones.Add(redencion);

            // Reducir stock del premio
            premio.Stock--;
            _context.SaveChanges();

            return (true, "Premio redimido exitosamente.");
        }

        public void DeleteRedencion(int id)
        {
            var redencion = _context.Redenciones.FirstOrDefault(r => r.Id == id);
            if (redencion != null)
            {
                // Restaurar stock del premio
                var premio = _premioService.GetPremioById(redencion.PremioId);
                if (premio != null)
                {
                    premio.Stock++;
                    _context.SaveChanges();
                }
                _context.Redenciones.Remove(redencion);
                _context.SaveChanges();
            }
        }
    }
}

