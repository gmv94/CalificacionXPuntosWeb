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
            List<ImpactoConfig>? configuracionImpactos = null)
        {
            // Calcular PUNTOS VALOR INVERSIÓN
            idea.PuntosValorInversion = CalcularPuntosValorInversion(idea.ValorInversion ?? 0);

            // Calcular PUNTOS ROI
            idea.PuntosROI = CalcularPuntosROI(idea.RoiMeses);

            // Calcular PUNTOS FACILIDAD IMPLEM.
            idea.PuntosFacilidadImplem = CalcularPuntosFacilidadImplem(idea.FacilidadImplem);

            // Obtener una copia actualizada del diccionario de impactos para el cálculo
            var impactosParaCalculo = new Dictionary<string, decimal>(idea.Impactos);
            
            // Calcular PUNTOS IMPACTO (usando diccionario dinámico con configuración)
            idea.PuntosImpacto = CalcularPuntosImpacto(impactosParaCalculo, configuracionImpactos);

            // Calcular PUNTOS TOTALES
            idea.PuntosTotales = idea.PuntosValorInversion +
                                idea.PuntosROI +
                                idea.PuntosFacilidadImplem +
                                idea.PuntosImpacto +
                                idea.PuntosExtra;
        }

        private static decimal CalcularPuntosValorInversion(decimal valorInversion)
        {
            // Fórmula: IF(VALOR <= 10M, 100%*10%*22000, IF(VALOR <= 20M, 60%*10%*22000, 20%*10%*22000))
            if (valorInversion == 0) return 0;
            
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

        private static decimal CalcularPuntosROI(string? roiMeses)
        {
            // Fórmula: IF(ROI="<=3", 100%*10%*22000, IF(ROI=">3<=6", 75%*10%*22000, IF(ROI="N/A", 0, 25%*10%*22000)))
            if (string.IsNullOrEmpty(roiMeses)) return 0;
            
            roiMeses = roiMeses.Trim().ToUpper();
            
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

        private static decimal CalcularPuntosFacilidadImplem(string? facilidad)
        {
            // Fórmula: IF(FACILIDAD="A", 100%*10%*22000, IF(FACILIDAD="B", 50%*10%*22000, 20%*10%*22000))
            if (string.IsNullOrEmpty(facilidad)) return 0;
            
            facilidad = facilidad.Trim().ToUpper();
            
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

