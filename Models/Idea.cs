using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalificacionXPuntosWeb.Models
{
    public class Idea
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string? Celular { get; set; }
        public string Radicado { get; set; } = string.Empty;
        public DateTime FechaRadicado { get; set; }
        public string? Categoria { get; set; }
        public string? Proceso { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string DescripcionIdea { get; set; } = string.Empty;
        
        // Campos para cálculo de puntos
        public decimal? ValorInversion { get; set; }
        public string? RoiMeses { get; set; } // "<=3", ">3<=6", ">6", "N/A"
        public string? FacilidadImplem { get; set; } // "A", "B", "C"
        
        // Impactos almacenados como JSON string
        private string _impactosJson = "{}";
        public string ImpactosJson 
        { 
            get => _impactosJson;
            set => _impactosJson = value ?? "{}";
        }
        
        [NotMapped]
        public Dictionary<string, decimal> Impactos
        {
            get
            {
                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, decimal>>(_impactosJson) ?? new Dictionary<string, decimal>();
                }
                catch
                {
                    return new Dictionary<string, decimal>();
                }
            }
            set
            {
                _impactosJson = JsonSerializer.Serialize(value ?? new Dictionary<string, decimal>());
            }
        }
        
        public decimal? PuntosExtra { get; set; }
        public string? ComentariosPuntosExtra { get; set; }
        public string? Observaciones { get; set; }
        
        // Puntos calculados
        public decimal PuntosValorInversion { get; set; } = 0;
        public decimal PuntosROI { get; set; } = 0;
        public decimal PuntosFacilidadImplem { get; set; } = 0;
        public decimal PuntosImpacto { get; set; } = 0;
        public decimal PuntosTotales { get; set; } = 0;
        
        // Puntos totales como int para compatibilidad
        public int Puntos 
        { 
            get => (int)Math.Round(PuntosTotales);
            set => PuntosTotales = value;
        }
        
        public DateTime FechaRegistro { get; set; } = System.DateTime.Now;
    }
}

