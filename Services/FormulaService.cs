using CalificacionXPuntosWeb.Models;

namespace CalificacionXPuntosWeb.Services
{
    public class FormulaService
    {
        // Valor base según el Excel
        private const decimal VALOR_BASE = 22000m;
        private const decimal PORCENTAJE_FIJOS = 0.10m; // 10%
        private const decimal PORCENTAJE_IMPACTO = 0.70m; // 70%

        public static void CalcularTodosLosPuntos(
            Idea idea,
            List<ImpactoConfig>? configuracionImpactos = null,
            ConfiguracionPuntosService? configuracionPuntosService = null)
        {
            // Calcular PUNTOS VALOR INVERSIÓN
            idea.PuntosValorInversion = CalcularPuntosValorInversion(idea.ValorInversion ?? 0, configuracionPuntosService);

            // Calcular PUNTOS ROI
            idea.PuntosROI = CalcularPuntosROI(idea.RoiMeses, configuracionPuntosService);

            // Calcular PUNTOS FACILIDAD IMPLEM.
            idea.PuntosFacilidadImplem = CalcularPuntosFacilidadImplem(idea.FacilidadImplem, configuracionPuntosService);

            // Obtener una copia actualizada del diccionario de impactos para el cálculo
            var impactosParaCalculo = new Dictionary<string, decimal>(idea.Impactos);
            
            // Calcular PUNTOS IMPACTO (usando diccionario dinámico con configuración)
            idea.PuntosImpacto = CalcularPuntosImpacto(impactosParaCalculo, configuracionImpactos);

            // Calcular PUNTOS TOTALES
            idea.PuntosTotales = idea.PuntosValorInversion +
                                idea.PuntosROI +
                                idea.PuntosFacilidadImplem +
                                idea.PuntosImpacto +
                                (idea.PuntosExtra ?? 0);
        }

        private static decimal CalcularPuntosValorInversion(decimal valorInversion, ConfiguracionPuntosService? configuracionPuntosService = null)
        {
            if (valorInversion == 0) return 0;
            
            // Si hay servicio de configuración, usar valores de BD
            if (configuracionPuntosService != null)
            {
                var config = configuracionPuntosService.GetConfiguracionValorInversion(valorInversion);
                if (config != null)
                {
                    return (config.Porcentaje / 100m) * config.PorcentajeFijos * config.ValorBase;
                }
            }
            
            // Fallback a valores por defecto
            if (valorInversion <= 10000000)
            {
                return 1.0m * PORCENTAJE_FIJOS * VALOR_BASE; // 100% * 10% * 22000 = 2200
            }
            else if (valorInversion <= 20000000)
            {
                return 0.60m * PORCENTAJE_FIJOS * VALOR_BASE; // 60% * 10% * 22000 = 1320
            }
            else
            {
                return 0.20m * PORCENTAJE_FIJOS * VALOR_BASE; // 20% * 10% * 22000 = 440
            }
        }

        private static decimal CalcularPuntosROI(string? roiMeses, ConfiguracionPuntosService? configuracionPuntosService = null)
        {
            if (string.IsNullOrEmpty(roiMeses)) return 0;
            
            roiMeses = roiMeses.Trim().ToUpper();
            
            // Si hay servicio de configuración, usar valores de BD
            if (configuracionPuntosService != null)
            {
                var config = configuracionPuntosService.GetConfiguracionROI(roiMeses);
                if (config != null)
                {
                    return (config.Porcentaje / 100m) * config.PorcentajeFijos * config.ValorBase;
                }
            }
            
            // Fallback a valores por defecto
            if (roiMeses == "N/A" || roiMeses == "NA")
            {
                return 0;
            }
            else if (roiMeses == "<=3" || roiMeses.Contains("<=3"))
            {
                return 1.0m * PORCENTAJE_FIJOS * VALOR_BASE; // 100% * 10% * 22000 = 2200
            }
            else if (roiMeses == ">3<=6" || (roiMeses.Contains(">3") && roiMeses.Contains("<=6")))
            {
                return 0.75m * PORCENTAJE_FIJOS * VALOR_BASE; // 75% * 10% * 22000 = 1650
            }
            else
            {
                return 0.25m * PORCENTAJE_FIJOS * VALOR_BASE; // 25% * 10% * 22000 = 550
            }
        }

        private static decimal CalcularPuntosFacilidadImplem(string? facilidad, ConfiguracionPuntosService? configuracionPuntosService = null)
        {
            if (string.IsNullOrEmpty(facilidad)) return 0;
            
            facilidad = facilidad.Trim().ToUpper();
            
            // Si hay servicio de configuración, usar valores de BD
            if (configuracionPuntosService != null)
            {
                var config = configuracionPuntosService.GetConfiguracionFacilidadImplem(facilidad);
                if (config != null)
                {
                    return (config.Porcentaje / 100m) * config.PorcentajeFijos * config.ValorBase;
                }
            }
            
            // Fallback a valores por defecto
            if (facilidad == "A")
            {
                return 1.0m * PORCENTAJE_FIJOS * VALOR_BASE; // 100% * 10% * 22000 = 2200
            }
            else if (facilidad == "B")
            {
                return 0.50m * PORCENTAJE_FIJOS * VALOR_BASE; // 50% * 10% * 22000 = 1100
            }
            else
            {
                return 0.20m * PORCENTAJE_FIJOS * VALOR_BASE; // 20% * 10% * 22000 = 440
            }
        }

        private static decimal CalcularPuntosImpacto(
            Dictionary<string, decimal> impactos,
            List<ImpactoConfig>? configuracionImpactos = null)
        {
            // Fórmula: (porcentaje_asignado / 100) * 70% * 22000
            // El total máximo posible es 15400 (70% * 22000)
            // Cada impacto puede tener un PorcentajeMaximo diferente
            // El porcentaje asignado no puede exceder el PorcentajeMaximo de cada impacto
            decimal total = 0;
            
            if (configuracionImpactos != null && configuracionImpactos.Any())
            {
                // Calcular puntos para cada impacto configurado
                foreach (var config in configuracionImpactos)
                {
                    // Buscar el valor asignado para este impacto
                    if (impactos.TryGetValue(config.Nombre, out var porcentajeAsignado))
                    {
                        // Validar que no exceda el máximo
                        if (porcentajeAsignado > config.PorcentajeMaximo)
                        {
                            porcentajeAsignado = config.PorcentajeMaximo;
                        }
                        
                        // Calcular puntos: (porcentaje_asignado / 100) * 70% * 22000
                        // Ejemplo: si asigno 25% y el máximo es 25%, obtengo (25/100) * 15400 = 3850 puntos
                        decimal puntosImpacto = (porcentajeAsignado / 100m) * PORCENTAJE_IMPACTO * VALOR_BASE;
                        total += puntosImpacto;
                    }
                }
            }
            else
            {
                // Fallback: si no hay configuración, usar fórmula simple
                foreach (var impacto in impactos.Values)
                {
                    total += (impacto / 100m) * PORCENTAJE_IMPACTO * VALOR_BASE;
                }
            }
            
            return Math.Round(total, 2);
        }

        public static string? ValidarLimitesImpacto(
            Dictionary<string, decimal> impactos,
            List<ImpactoConfig> configuracionImpactos)
        {
            // Validar que cada impacto no exceda su máximo permitido
            foreach (var impacto in impactos)
            {
                var config = configuracionImpactos.FirstOrDefault(c => c.Nombre == impacto.Key);
                if (config != null && impacto.Value > config.PorcentajeMaximo)
                {
                    return $"{impacto.Key} no puede ser mayor a {config.PorcentajeMaximo}%";
                }
            }
            
            return null;
        }
    }
}

