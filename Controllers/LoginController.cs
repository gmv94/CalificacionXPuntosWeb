using Microsoft.AspNetCore.Mvc;
using CalificacionXPuntosWeb.Services;
using Microsoft.AspNetCore.Http;

namespace CalificacionXPuntosWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthService _authService;

        public LoginController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Si ya está autenticado, redirigir al inicio
            var usuarioId = HttpContext.Session.GetString("usuarioId");
            if (!string.IsNullOrEmpty(usuarioId))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NombreUsuario) || string.IsNullOrWhiteSpace(model.Contrasena))
            {
                ViewBag.MensajeError = "Por favor ingrese usuario y contraseña.";
                return View(model);
            }

            var usuario = _authService.Login(model.NombreUsuario, model.Contrasena);
            if (usuario != null)
            {
                // Guardar en sesión
                HttpContext.Session.SetString("usuarioId", usuario.Id.ToString());
                HttpContext.Session.SetString("nombreUsuario", usuario.NombreUsuario);
                HttpContext.Session.SetString("rol", usuario.Rol);

                TempData["Mensaje"] = $"¡Bienvenido {usuario.NombreUsuario}!";
                TempData["TipoMensaje"] = "success";

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.MensajeError = "Usuario o contraseña incorrectos.";
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }

    public class LoginModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}



