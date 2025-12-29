namespace CalificacionXPuntosWeb.Models
{
    public class PuntosHistoricos
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int Puntos { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}

