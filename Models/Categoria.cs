namespace CalificacionXPuntosWeb.Models
{
    public class ImpactoConfig
    {
        public string Nombre { get; set; } = "";
        public decimal PorcentajeMaximo { get; set; }
    }

    public class Categoria
    {
        public string Nombre { get; set; } = "";
        public List<ImpactoConfig> Impactos { get; set; } = new();
    }

    public class ConfiguracionCategorias
    {
        public List<Categoria> Categorias { get; set; } = new();
    }
}

