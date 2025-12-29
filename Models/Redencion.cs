namespace CalificacionXPuntosWeb.Models
{
    public class Redencion
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public int PremioId { get; set; }
        public string NombrePremio { get; set; } = string.Empty;
        public int PuntosUtilizados { get; set; }
        public DateTime FechaRedencion { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Redimido";
    }
}

