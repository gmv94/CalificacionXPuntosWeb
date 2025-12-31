using CalificacionXPuntosWeb.Data;
using CalificacionXPuntosWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class ConfiguracionPuntosService
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracionPuntosService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ConfiguracionPuntos> GetAllConfiguraciones()
        {
            try
            {
                return _context.ConfiguracionPuntos
                    .OrderBy(c => c.Tipo)
                    .ThenBy(c => c.Orden)
                    .ToList();
            }
            catch
            {
                return new List<ConfiguracionPuntos>();
            }
        }

        public List<ConfiguracionPuntos> GetConfiguracionesPorTipo(string tipo)
        {
            try
            {
                return _context.ConfiguracionPuntos
                    .Where(c => c.Tipo == tipo && c.Activo)
                    .OrderBy(c => c.Orden)
                    .ToList();
            }
            catch
            {
                return new List<ConfiguracionPuntos>();
            }
        }

        public ConfiguracionPuntos? GetConfiguracionById(int id)
        {
            try
            {
                return _context.ConfiguracionPuntos.Find(id);
            }
            catch
            {
                return null;
            }
        }

        public ConfiguracionPuntos? GetConfiguracionValorInversion(decimal valorInversion)
        {
            try
            {
                var configs = GetConfiguracionesPorTipo("ValorInversion");
                
                foreach (var config in configs.OrderByDescending(c => c.Orden))
                {
                    if (config.Valor == "<=10M" && valorInversion <= 10000000)
                        return config;
                    if (config.Valor == ">10M<=20M" && valorInversion > 10000000 && valorInversion <= 20000000)
                        return config;
                    if (config.Valor == ">20M" && valorInversion > 20000000)
                        return config;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public ConfiguracionPuntos? GetConfiguracionROI(string roiMeses)
        {
            try
            {
                if (string.IsNullOrEmpty(roiMeses)) return null;
                
                roiMeses = roiMeses.Trim().ToUpper();
                var configs = GetConfiguracionesPorTipo("ROI");
                
                foreach (var config in configs)
                {
                    if (config.Valor == "N/A" && (roiMeses == "N/A" || roiMeses == "NA"))
                        return config;
                    if (config.Valor == "<=3" && (roiMeses == "<=3" || roiMeses.Contains("<=3")))
                        return config;
                    if (config.Valor == ">3<=6" && (roiMeses == ">3<=6" || (roiMeses.Contains(">3") && roiMeses.Contains("<=6"))))
                        return config;
                    if (config.Valor == ">6" && roiMeses == ">6")
                        return config;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public ConfiguracionPuntos? GetConfiguracionFacilidadImplem(string facilidad)
        {
            try
            {
                if (string.IsNullOrEmpty(facilidad)) return null;
                
                facilidad = facilidad.Trim().ToUpper();
                var configs = GetConfiguracionesPorTipo("FacilidadImplem");
                
                return configs.FirstOrDefault(c => c.Valor == facilidad);
            }
            catch
            {
                return null;
            }
        }

        public void AddConfiguracion(ConfiguracionPuntos configuracion)
        {
            try
            {
                _context.ConfiguracionPuntos.Add(configuracion);
                _context.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateConfiguracion(ConfiguracionPuntos configuracion)
        {
            try
            {
                var existing = _context.ConfiguracionPuntos.Find(configuracion.Id);
                if (existing != null)
                {
                    existing.Tipo = configuracion.Tipo;
                    existing.Valor = configuracion.Valor;
                    existing.Porcentaje = configuracion.Porcentaje;
                    existing.ValorBase = configuracion.ValorBase;
                    existing.PorcentajeFijos = configuracion.PorcentajeFijos;
                    existing.Activo = configuracion.Activo;
                    existing.Orden = configuracion.Orden;
                    _context.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void DeleteConfiguracion(int id)
        {
            try
            {
                var configuracion = _context.ConfiguracionPuntos.Find(id);
                if (configuracion != null)
                {
                    _context.ConfiguracionPuntos.Remove(configuracion);
                    _context.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        public void InicializarConfiguracionesPorDefecto()
        {
            try
            {
                // Verificar si ya existen configuraciones
                if (_context.ConfiguracionPuntos.Any())
                    return;

                var configuraciones = new List<ConfiguracionPuntos>
                {
                    // Valor Inversión
                    new ConfiguracionPuntos { Tipo = "ValorInversion", Valor = "<=10M", Porcentaje = 100m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 1 },
                    new ConfiguracionPuntos { Tipo = "ValorInversion", Valor = ">10M<=20M", Porcentaje = 60m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 2 },
                    new ConfiguracionPuntos { Tipo = "ValorInversion", Valor = ">20M", Porcentaje = 20m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 3 },
                    
                    // ROI
                    new ConfiguracionPuntos { Tipo = "ROI", Valor = "<=3", Porcentaje = 100m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 1 },
                    new ConfiguracionPuntos { Tipo = "ROI", Valor = ">3<=6", Porcentaje = 75m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 2 },
                    new ConfiguracionPuntos { Tipo = "ROI", Valor = ">6", Porcentaje = 25m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 3 },
                    new ConfiguracionPuntos { Tipo = "ROI", Valor = "N/A", Porcentaje = 0m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 4 },
                    
                    // Facilidad Implem
                    new ConfiguracionPuntos { Tipo = "FacilidadImplem", Valor = "A", Porcentaje = 100m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 1 },
                    new ConfiguracionPuntos { Tipo = "FacilidadImplem", Valor = "B", Porcentaje = 50m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 2 },
                    new ConfiguracionPuntos { Tipo = "FacilidadImplem", Valor = "C", Porcentaje = 20m, ValorBase = 22000m, PorcentajeFijos = 0.10m, Activo = true, Orden = 3 }
                };

                _context.ConfiguracionPuntos.AddRange(configuraciones);
                _context.SaveChanges();
            }
            catch
            {
                // Si hay error, no hacer nada
            }
        }
    }
}

