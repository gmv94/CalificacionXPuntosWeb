namespace CalificacionXPuntosWeb.Models
{
    public class ImpactoBD
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PorcentajeMaximo { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; } = true;
        
        // Navegación
        public CategoriaBD? Categoria { get; set; }
    }
}

