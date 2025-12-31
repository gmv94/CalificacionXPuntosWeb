using Microsoft.AspNetCore.Mvc;
using CalificacionXPuntosWeb.Services;

namespace CalificacionXPuntosWeb.Controllers
{
    public class TestController : BaseController
    {
        private readonly CategoriaBDService _categoriaBDService;
        private readonly EstadoBDService _estadoBDService;
        private readonly ProcesoBDService _procesoBDService;

        public TestController(
            CategoriaBDService categoriaBDService,
            EstadoBDService estadoBDService,
            ProcesoBDService procesoBDService)
        {
            _categoriaBDService = categoriaBDService;
            _estadoBDService = estadoBDService;
            _procesoBDService = procesoBDService;
        }

        [HttpGet]
        public IActionResult CrearCalificacion()
        {
            try
            {
                ViewBag.Categorias = _categoriaBDService.GetAllCategorias().Where(c => c.Activo).Select(c => c.Nombre).Distinct().OrderBy(c => c).ToList();
                ViewBag.Estados = _estadoBDService.GetNombresEstados().Distinct().OrderBy(e => e).ToList();
                ViewBag.Procesos = _procesoBDService.GetNombresProcesos().Distinct().OrderBy(p => p).ToList();
            }
            catch
            {
                ViewBag.Categorias = new List<string>();
                ViewBag.Estados = new List<string>();
                ViewBag.Procesos = new List<string>();
            }

            return View();
        }
    }
}

