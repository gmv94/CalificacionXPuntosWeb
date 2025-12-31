namespace CalificacionXPuntosWeb.Models
{
    public class ConfiguracionPuntos
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // "ValorInversion", "ROI", "FacilidadImplem"
        public string Valor { get; set; } = string.Empty; // Para ValorInversion: "<=10M", ">10M<=20M", ">20M". Para ROI: "<=3", ">3<=6", ">6", "N/A". Para FacilidadImplem: "A", "B", "C"
        public decimal Porcentaje { get; set; } // Porcentaje a aplicar (100, 75, 60, 50, 25, 20, etc.)
        public decimal ValorBase { get; set; } = 22000m; // Valor base (por defecto 22000)
        public decimal PorcentajeFijos { get; set; } = 0.10m; // Porcentaje fijos (por defecto 10%)
        public bool Activo { get; set; } = true;
        public int Orden { get; set; } // Orden de evaluación
    }
}

