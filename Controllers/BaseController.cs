using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace CalificacionXPuntosWeb.Controllers
{
    public class BaseController : Controller
    {
        protected bool EstaAutenticado => !string.IsNullOrEmpty(HttpContext.Session.GetString("usuarioId"));
        protected bool EsSuperAdmin => HttpContext.Session.GetString("rol") == "SuperAdmin";
        protected string NombreUsuario => HttpContext.Session.GetString("nombreUsuario") ?? string.Empty;
        protected int? UsuarioId
        {
            get
            {
                var id = HttpContext.Session.GetString("usuarioId");
                return string.IsNullOrEmpty(id) ? null : int.Parse(id);
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.EstaAutenticado = EstaAutenticado;
            ViewBag.EsSuperAdmin = EsSuperAdmin;
            ViewBag.NombreUsuario = NombreUsuario;
            base.OnActionExecuting(context);
        }

        protected IActionResult RequiereAutenticacion()
        {
            if (!EstaAutenticado)
            {
                return RedirectToAction("Index", "Login");
            }
            return null;
        }

        protected IActionResult RequiereSuperAdmin()
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            if (!EsSuperAdmin)
            {
                TempData["Mensaje"] = "No tiene permisos para acceder a esta sección.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Index", "Home");
            }
            return null;
        }
    }
}



