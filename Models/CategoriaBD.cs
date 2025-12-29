namespace CalificacionXPuntosWeb.Models
{
    public class CategoriaBD
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Relación con Impactos
        public List<ImpactoBD> Impactos { get; set; } = new();
    }
}

