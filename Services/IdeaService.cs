using CalificacionXPuntosWeb.Models;
using CalificacionXPuntosWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class IdeaService
    {
        private readonly ApplicationDbContext _context;
        private readonly PuntosHistoricosService? _puntosHistoricosService;

        public IdeaService(ApplicationDbContext context, PuntosHistoricosService? puntosHistoricosService = null)
        {
            _context = context;
            _puntosHistoricosService = puntosHistoricosService;
        }

        public List<Idea> GetAllIdeas()
        {
            try
            {
                var ideas = _context.Ideas.ToList();
                // NO normalizar aquí - dejar los valores NULL como están
                // La normalización se hace solo en la UI si es necesario para mostrar
                return ideas;
            }
            catch
            {
                return new List<Idea>();
            }
        }

        public Idea? GetIdeaById(int id)
        {
            try
            {
                var idea = _context.Ideas.FirstOrDefault(i => i.Id == id);
                // NO normalizar aquí - dejar los valores NULL como están para que se guarden correctamente
                return idea;
            }
            catch
            {
                return null;
            }
        }

        public void AddIdea(Idea idea)
        {
            // Asegurar que los campos numéricos que no permiten NULL tengan valores válidos
            // PuntosExtra ahora permite NULL, así que no se valida
            if (idea.PuntosValorInversion < 0) idea.PuntosValorInversion = 0;
            if (idea.PuntosROI < 0) idea.PuntosROI = 0;
            if (idea.PuntosFacilidadImplem < 0) idea.PuntosFacilidadImplem = 0;
            if (idea.PuntosImpacto < 0) idea.PuntosImpacto = 0;
            if (idea.PuntosTotales < 0) idea.PuntosTotales = 0;
            
            _context.Ideas.Add(idea);
            _context.SaveChanges();
        }

        public void UpdateIdea(Idea idea)
        {
            var existing = _context.Ideas.FirstOrDefault(i => i.Id == idea.Id);
            if (existing != null)
            {
                existing.NumeroDocumento = idea.NumeroDocumento;
                existing.NombreUsuario = idea.NombreUsuario;
                existing.Celular = idea.Celular;
                existing.Radicado = idea.Radicado;
                existing.FechaRadicado = idea.FechaRadicado;
                existing.Categoria = idea.Categoria;
                existing.Proceso = idea.Proceso;
                existing.Estado = idea.Estado;
                existing.DescripcionIdea = idea.DescripcionIdea;
                existing.ValorInversion = idea.ValorInversion;
                existing.RoiMeses = idea.RoiMeses;
                existing.FacilidadImplem = idea.FacilidadImplem;
                // Actualizar ImpactosJson (que es lo que se guarda en BD)
                existing.ImpactosJson = idea.ImpactosJson;
                existing.PuntosExtra = idea.PuntosExtra;
                existing.ComentariosPuntosExtra = idea.ComentariosPuntosExtra;
                existing.Observaciones = idea.Observaciones;
                existing.PuntosValorInversion = idea.PuntosValorInversion;
                existing.PuntosROI = idea.PuntosROI;
                existing.PuntosFacilidadImplem = idea.PuntosFacilidadImplem;
                existing.PuntosImpacto = idea.PuntosImpacto;
                existing.PuntosTotales = idea.PuntosTotales;
                _context.SaveChanges();
            }
        }

        public void DeleteIdea(int id)
        {
            var idea = _context.Ideas.FirstOrDefault(i => i.Id == id);
            if (idea != null)
            {
                _context.Ideas.Remove(idea);
                _context.SaveChanges();
            }
        }

        public List<PuntosAcumulados> GetPuntosAcumulados()
        {
            try
            {
                var grupos = _context.Ideas
                    .GroupBy(i => i.NumeroDocumento)
                    .Select(g => new PuntosAcumulados
                    {
                        NumeroDocumento = g.Key,
                        NombreUsuario = g.First().NombreUsuario,
                        TotalIdeas = g.Count(),
                        TotalPuntos = g.Sum(i => (int)Math.Round(i.PuntosTotales)),
                        PuntosHistoricos = 0, // Se calculará después
                        Ideas = g.ToList()
                    })
                    .ToList();

                // Agregar puntos históricos si el servicio está disponible
                if (_puntosHistoricosService != null)
                {
                    foreach (var grupo in grupos)
                    {
                        grupo.PuntosHistoricos = _puntosHistoricosService.GetTotalPuntosHistoricosPorDocumento(grupo.NumeroDocumento);
                        grupo.TotalPuntos += grupo.PuntosHistoricos;
                    }
                }

                return grupos;
            }
            catch
            {
                return new List<PuntosAcumulados>();
            }
        }

        public PuntosAcumulados? GetPuntosAcumuladosPorDocumento(string numeroDocumento)
        {
            try
            {
                var ideasUsuario = _context.Ideas.Where(i => i.NumeroDocumento == numeroDocumento).ToList();
                if (!ideasUsuario.Any())
                {
                    // Si no hay ideas pero hay puntos históricos, crear registro
                    if (_puntosHistoricosService != null)
                    {
                        var puntosHistoricos = _puntosHistoricosService.GetTotalPuntosHistoricosPorDocumento(numeroDocumento);
                        if (puntosHistoricos > 0)
                        {
                            var historico = _puntosHistoricosService.GetPuntosHistoricosPorDocumento(numeroDocumento).FirstOrDefault();
                            return new PuntosAcumulados
                            {
                                NumeroDocumento = numeroDocumento,
                                NombreUsuario = historico?.NombreUsuario ?? string.Empty,
                                TotalIdeas = 0,
                                TotalPuntos = puntosHistoricos,
                                PuntosHistoricos = puntosHistoricos,
                                Ideas = new List<Idea>()
                            };
                        }
                    }
                    return null;
                }

                var puntosAcumulados = new PuntosAcumulados
                {
                    NumeroDocumento = numeroDocumento,
                    NombreUsuario = ideasUsuario.First().NombreUsuario,
                    TotalIdeas = ideasUsuario.Count,
                    TotalPuntos = ideasUsuario.Sum(i => (int)Math.Round(i.PuntosTotales)),
                    PuntosHistoricos = 0,
                    Ideas = ideasUsuario
                };

                // Agregar puntos históricos si el servicio está disponible
                if (_puntosHistoricosService != null)
                {
                    puntosAcumulados.PuntosHistoricos = _puntosHistoricosService.GetTotalPuntosHistoricosPorDocumento(numeroDocumento);
                    puntosAcumulados.TotalPuntos += puntosAcumulados.PuntosHistoricos;
                }

                return puntosAcumulados;
            }
            catch
            {
                return null;
            }
        }
    }
}

