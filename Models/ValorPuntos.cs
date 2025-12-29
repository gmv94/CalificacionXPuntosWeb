namespace CalificacionXPuntosWeb.Models
{
    public class ValorPuntos
    {
        public int Id { get; set; }
        public decimal CostoMinimo { get; set; }
        public decimal CostoMaximo { get; set; }
        public decimal ValorPorPunto { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}

