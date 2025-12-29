using CalificacionXPuntosWeb.Models;
using CalificacionXPuntosWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CalificacionXPuntosWeb.Services
{
    public class CategoriaBDService
    {
        private readonly ApplicationDbContext _context;

        public CategoriaBDService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CategoriaBD> GetAllCategorias()
        {
            try
            {
                return _context.Categorias
                    .Include(c => c.Impactos)
                    .OrderBy(c => c.Nombre)
                    .ToList();
            }
            catch
            {
                return new List<CategoriaBD>();
            }
        }

        public CategoriaBD? GetCategoriaById(int id)
        {
            try
            {
                return _context.Categorias
                    .Include(c => c.Impactos)
                    .FirstOrDefault(c => c.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public CategoriaBD? GetCategoriaPorNombre(string nombre)
        {
            try
            {
                return _context.Categorias
                    .Include(c => c.Impactos)
                    .FirstOrDefault(c => c.Nombre == nombre);
            }
            catch
            {
                return null;
            }
        }

        public void AddCategoria(CategoriaBD categoria)
        {
            try
            {
                // Guardar la categoría primero
                var nuevaCategoria = new CategoriaBD
                {
                    Nombre = categoria.Nombre,
                    Activo = true, // Por defecto activo
                    FechaCreacion = DateTime.Now
                };
                
                _context.Categorias.Add(nuevaCategoria);
                _context.SaveChanges(); // Guardar para obtener el ID
                
                // Ahora agregar los impactos con el CategoriaId correcto
                if (categoria.Impactos != null && categoria.Impactos.Any())
                {
                    foreach (var impacto in categoria.Impactos)
                    {
                        // Solo agregar impactos nuevos (sin Id o Id = 0)
                        if (impacto.Id == 0)
                        {
                            var nuevoImpacto = new ImpactoBD
                            {
                                CategoriaId = nuevaCategoria.Id,
                                Nombre = impacto.Nombre ?? string.Empty,
                                PorcentajeMaximo = impacto.PorcentajeMaximo,
                                Orden = impacto.Orden,
                                Activo = true
                            };
                            _context.Impactos.Add(nuevoImpacto);
                        }
                    }
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar la categoría: {ex.Message}", ex);
            }
        }

        public void UpdateCategoria(CategoriaBD categoria)
        {
            var existing = _context.Categorias
                .Include(c => c.Impactos)
                .FirstOrDefault(c => c.Id == categoria.Id);
            
            if (existing != null)
            {
                existing.Nombre = categoria.Nombre;
                existing.Activo = categoria.Activo;
                
                // Actualizar impactos existentes y agregar nuevos
                foreach (var impacto in categoria.Impactos)
                {
                    var existingImpacto = existing.Impactos.FirstOrDefault(i => i.Id == impacto.Id);
                    if (existingImpacto != null)
                    {
                        existingImpacto.Nombre = impacto.Nombre;
                        existingImpacto.PorcentajeMaximo = impacto.PorcentajeMaximo;
                        existingImpacto.Orden = impacto.Orden;
                        existingImpacto.Activo = impacto.Activo;
                    }
                    else
                    {
                        impacto.CategoriaId = existing.Id;
                        _context.Impactos.Add(impacto);
                    }
                }
                
                // Eliminar impactos que ya no están
                var impactosAEliminar = existing.Impactos
                    .Where(i => !categoria.Impactos.Any(ci => ci.Id == i.Id))
                    .ToList();
                
                foreach (var impacto in impactosAEliminar)
                {
                    _context.Impactos.Remove(impacto);
                }
                
                _context.SaveChanges();
            }
        }

        public void DeleteCategoria(int id)
        {
            var categoria = _context.Categorias
                .Include(c => c.Impactos)
                .FirstOrDefault(c => c.Id == id);
            
            if (categoria != null)
            {
                // Eliminar impactos relacionados
                _context.Impactos.RemoveRange(categoria.Impactos);
                _context.Categorias.Remove(categoria);
                _context.SaveChanges();
            }
        }

        public List<string> GetNombresCategorias()
        {
            try
            {
                return _context.Categorias
                    .OrderBy(c => c.Nombre)
                    .Select(c => c.Nombre)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public Categoria? ConvertirACategoriaModel(CategoriaBD categoriaBD)
        {
            if (categoriaBD == null) return null;

            return new Categoria
            {
                Nombre = categoriaBD.Nombre,
                Impactos = categoriaBD.Impactos?
                    .OrderBy(i => i.Orden)
                    .Select(i => new ImpactoConfig
                    {
                        Nombre = i.Nombre,
                        PorcentajeMaximo = i.PorcentajeMaximo
                    })
                    .ToList() ?? new List<ImpactoConfig>()
            };
        }
    }
}

