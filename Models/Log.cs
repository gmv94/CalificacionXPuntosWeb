namespace CalificacionXPuntosWeb.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Login", "Create", "Update", "Delete"
        public string Entidad { get; set; } = string.Empty; // "Idea", "Premio", "Usuario", etc.
        public int? EntidadId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string? Detalles { get; set; } // JSON con detalles adicionales
    }
}

