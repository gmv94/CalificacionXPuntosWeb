namespace CalificacionXPuntosWeb.Models
{
    public class PuntosAcumulados
    {
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int TotalIdeas { get; set; }
        public int TotalPuntos { get; set; }
        public int PuntosHistoricos { get; set; }
        public int PuntosDisponibles { get; set; }
        public int PuntosUtilizados { get; set; }
        public List<Idea> Ideas { get; set; } = new List<Idea>();
    }
}

