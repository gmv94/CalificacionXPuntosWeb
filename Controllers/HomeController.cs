using Microsoft.AspNetCore.Mvc;
using CalificacionXPuntosWeb.Services;
using CalificacionXPuntosWeb.Models;
using CalificacionXPuntosWeb.Data;
using static CalificacionXPuntosWeb.Services.TimeHelper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace CalificacionXPuntosWeb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IdeaService _ideaService;
        private readonly RedencionService _redencionService;
        private readonly PuntosHistoricosService _puntosHistoricosService;
        private readonly PremioService _premioService;
        private readonly AuthService _authService;
        private readonly CategoriaBDService _categoriaBDService;
        private readonly EstadoBDService _estadoBDService;
        private readonly ProcesoBDService _procesoBDService;
        private readonly ValorPuntosService _valorPuntosService;
        private readonly ConfiguracionPuntosService _configuracionPuntosService;
        private readonly LogService _logService;

        public HomeController(
            IdeaService ideaService,
            RedencionService redencionService,
            PuntosHistoricosService puntosHistoricosService,
            PremioService premioService,
            AuthService authService,
            CategoriaBDService categoriaBDService,
            EstadoBDService estadoBDService,
            ProcesoBDService procesoBDService,
            ValorPuntosService valorPuntosService,
            ConfiguracionPuntosService configuracionPuntosService,
            LogService logService)
        {
            _ideaService = ideaService;
            _redencionService = redencionService;
            _puntosHistoricosService = puntosHistoricosService;
            _premioService = premioService;
            _authService = authService;
            _categoriaBDService = categoriaBDService;
            _estadoBDService = estadoBDService;
            _procesoBDService = procesoBDService;
            _valorPuntosService = valorPuntosService;
            _configuracionPuntosService = configuracionPuntosService;
            _logService = logService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ConsultarDocumento(string numeroDocumento = "")
        {
            if (!string.IsNullOrEmpty(numeroDocumento))
            {
                return ConsultarDocumentoInternal(numeroDocumento);
            }
            return View();
        }

        private IActionResult ConsultarDocumentoInternal(string numeroDocumento)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                ViewBag.Mensaje = "Por favor ingrese un número de documento.";
                ViewBag.EsExito = false;
                return View("ConsultarDocumento");
            }

            var puntosUsuario = _ideaService.GetPuntosAcumuladosPorDocumento(numeroDocumento);
            
            if (puntosUsuario == null)
            {
                ViewBag.Mensaje = "No se encontraron registros para este documento.";
                ViewBag.EsExito = false;
                return View("ConsultarDocumento");
            }

            var redencionesUsuario = _redencionService.GetRedencionesPorDocumento(numeroDocumento);
            var puntosUtilizados = redencionesUsuario?.Sum(r => r.PuntosUtilizados) ?? 0;
            var puntosDisponibles = puntosUsuario.TotalPuntos - puntosUtilizados;
            var puntosHistoricosUsuario = _puntosHistoricosService.GetPuntosHistoricosPorDocumento(numeroDocumento);

            ViewBag.PuntosUsuario = puntosUsuario;
            ViewBag.RedencionesUsuario = redencionesUsuario;
            ViewBag.PuntosHistoricosUsuario = puntosHistoricosUsuario;
            ViewBag.PuntosDisponibles = puntosDisponibles;
            ViewBag.PuntosUtilizados = puntosUtilizados;
            ViewBag.NumeroDocumento = numeroDocumento;

            return View("ConsultarDocumento");
        }

        [HttpPost]
        [ActionName("ConsultarDocumento")]
        public IActionResult ConsultarDocumentoPost(string numeroDocumento)
        {
            return ConsultarDocumentoInternal(numeroDocumento);
        }

        [HttpGet]
        public IActionResult PuntosAcumulados()
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            var puntosAcumulados = _ideaService.GetPuntosAcumulados();
            
            // Calcular puntos disponibles y utilizados
            foreach (var item in puntosAcumulados)
            {
                var redenciones = _redencionService.GetRedencionesPorDocumento(item.NumeroDocumento);
                item.PuntosUtilizados = redenciones?.Sum(r => r.PuntosUtilizados) ?? 0;
                item.PuntosDisponibles = item.TotalPuntos - item.PuntosUtilizados;
            }

            ViewBag.PuntosAcumulados = puntosAcumulados;
            return View();
        }

        [HttpGet]
        public IActionResult VerDetallePuntos(string numeroDocumento)
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            var puntosAcumulados = _ideaService.GetPuntosAcumulados();
            var detalle = puntosAcumulados?.FirstOrDefault(p => p.NumeroDocumento == numeroDocumento);
            var puntosHistoricos = _puntosHistoricosService.GetPuntosHistoricosPorDocumento(numeroDocumento);

            ViewBag.Detalle = detalle;
            ViewBag.PuntosHistoricos = puntosHistoricos;
            return View("PuntosAcumulados");
        }

        [HttpGet]
        public IActionResult RedencionPuntos(string numeroDocumento = "")
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            ViewBag.PremiosDisponibles = _premioService.GetPremiosDisponibles();
            
            if (!string.IsNullOrEmpty(numeroDocumento))
            {
                return RedencionPuntosInternal(numeroDocumento);
            }
            
            return View();
        }

        private IActionResult RedencionPuntosInternal(string numeroDocumento)
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(numeroDocumento))
            {
                ViewBag.Mensaje = "Por favor ingrese un número de documento.";
                ViewBag.EsExito = false;
                ViewBag.PremiosDisponibles = _premioService.GetPremiosDisponibles();
                return View("RedencionPuntos");
            }

            // Normalizar el número de documento antes de buscar
            var numeroDocumentoNormalizado = numeroDocumento.Trim();
            var puntosUsuario = _ideaService.GetPuntosAcumuladosPorDocumento(numeroDocumentoNormalizado);
            
            if (puntosUsuario == null)
            {
                ViewBag.Mensaje = "No se encontraron puntos para este documento.";
                ViewBag.EsExito = false;
                ViewBag.PremiosDisponibles = _premioService.GetPremiosDisponibles();
                return View("RedencionPuntos");
            }

            var redencionesUsuario = _redencionService.GetRedencionesPorDocumento(numeroDocumentoNormalizado);
            var puntosUtilizados = redencionesUsuario?.Sum(r => r.PuntosUtilizados) ?? 0;
            var puntosDisponibles = puntosUsuario.TotalPuntos - puntosUtilizados;

            ViewBag.PuntosUsuario = puntosUsuario;
            ViewBag.RedencionesUsuario = redencionesUsuario;
            ViewBag.PuntosDisponibles = puntosDisponibles;
            ViewBag.PremiosDisponibles = _premioService.GetPremiosDisponibles();
            ViewBag.NumeroDocumento = numeroDocumentoNormalizado;

            return View("RedencionPuntos");
        }

        [HttpPost]
        [ActionName("RedencionPuntos")]
        public IActionResult RedencionPuntosPost([FromForm] string numeroDocumento)
        {
            return RedencionPuntosInternal(numeroDocumento ?? "");
        }

        [HttpPost]
        public IActionResult RedimirPremio(string numeroDocumento, int premioId)
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            var resultado = _redencionService.RedimirPremio(numeroDocumento, premioId);
            TempData["Mensaje"] = resultado.Message;
            TempData["TipoMensaje"] = resultado.Success ? "success" : "error";
            
            if (resultado.Success)
            {
                _logService.RegistrarLog("Create", "Redencion", 0, NombreUsuario,
                    $"Premio redimido - Documento: {numeroDocumento}, Premio ID: {premioId}");
            }
            
            return RedirectToAction("RedencionPuntos", new { numeroDocumento });
        }

        [HttpPost]
        public IActionResult EliminarRedencion(int id, string numeroDocumento, string? returnUrl = null)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                var redencion = _redencionService.GetRedencionById(id);
                if (redencion != null)
                {
                    _redencionService.DeleteRedencion(id);
                    _logService.RegistrarLog("Delete", "Redencion", id, NombreUsuario,
                        $"Redención eliminada - Premio: {redencion.NombrePremio}, Documento: {redencion.NumeroDocumento}");
                    TempData["Mensaje"] = "Redención eliminada exitosamente.";
                    TempData["TipoMensaje"] = "success";
                }
                else
                {
                    TempData["Mensaje"] = "No se encontró la redención a eliminar.";
                    TempData["TipoMensaje"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            // Si viene de ListadoRedimidos, redirigir allí; si no, a RedencionPuntos
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("ListadoRedimidos"))
            {
                return RedirectToAction("ListadoRedimidos");
            }
            return RedirectToAction("RedencionPuntos", new { numeroDocumento });
        }

        [HttpGet]
        public IActionResult ListadoRedimidos(string filtroDocumento = "", string filtroNombre = "", string filtroPremio = "", 
            string filtroEstado = "", string filtroFechaDesde = "", string filtroFechaHasta = "", int pagina = 1)
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            var rol = HttpContext.Session.GetString("rol") ?? "";
            var esSuperAdmin = rol == "SuperAdmin";
            
            if (!esSuperAdmin && rol != "Consulta" && rol != "Usuario")
            {
                TempData["Mensaje"] = "No tiene permisos para acceder a esta sección.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Index");
            }

            var todasLasRedenciones = _redencionService.GetAllRedenciones() ?? new List<Redencion>();
            var query = todasLasRedenciones.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(filtroDocumento))
            {
                query = query.Where(r => r.NumeroDocumento.Contains(filtroDocumento, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filtroNombre))
            {
                query = query.Where(r => r.NombreUsuario.Contains(filtroNombre, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filtroPremio))
            {
                query = query.Where(r => r.NombrePremio.Contains(filtroPremio, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filtroEstado))
            {
                query = query.Where(r => r.Estado == filtroEstado);
            }

            if (!string.IsNullOrWhiteSpace(filtroFechaDesde) && DateTime.TryParse(filtroFechaDesde, out var fechaDesde))
            {
                query = query.Where(r => r.FechaRedencion.Date >= fechaDesde.Date);
            }

            if (!string.IsNullOrWhiteSpace(filtroFechaHasta) && DateTime.TryParse(filtroFechaHasta, out var fechaHasta))
            {
                query = query.Where(r => r.FechaRedencion.Date <= fechaHasta.Date);
            }

            // Ordenar por fecha más reciente primero
            var redencionesFiltradas = query.OrderByDescending(r => r.FechaRedencion).ToList();

            // Paginación
            var registrosPorPagina = 20;
            var totalPaginas = (int)Math.Ceiling((double)redencionesFiltradas.Count / registrosPorPagina);
            if (totalPaginas < 1) totalPaginas = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            var inicio = (pagina - 1) * registrosPorPagina;
            var redencionesPaginadas = redencionesFiltradas.Skip(inicio).Take(registrosPorPagina).ToList();

            ViewBag.Redenciones = redencionesPaginadas;
            ViewBag.TotalRegistros = redencionesFiltradas.Count;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.EsSuperAdmin = esSuperAdmin;
            
            // Mantener filtros en ViewBag para el formulario
            ViewBag.FiltroDocumento = filtroDocumento;
            ViewBag.FiltroNombre = filtroNombre;
            ViewBag.FiltroPremio = filtroPremio;
            ViewBag.FiltroEstado = filtroEstado;
            ViewBag.FiltroFechaDesde = filtroFechaDesde;
            ViewBag.FiltroFechaHasta = filtroFechaHasta;

            return View();
        }

        [HttpGet]
        public IActionResult TablaPremios(int? id = null)
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            ViewBag.Premios = _premioService.GetAllPremios();
            
            if (id.HasValue)
            {
                if (id.Value == 0)
                {
                    // Nuevo premio
                    ViewBag.PremioActual = new Premio 
                    { 
                        Id = 0, 
                        Activo = true,
                        FechaCreacion = TimeHelper.GetColombiaTime()
                    };
                    ViewBag.MostrarModal = true;
                }
                else
                {
                    // Editar premio existente
                    var premio = _premioService.GetPremioById(id.Value);
                    ViewBag.PremioActual = premio ?? new Premio { Id = 0, Activo = true };
                    ViewBag.MostrarModal = premio != null;
                }
            }
            else
            {
                ViewBag.MostrarModal = false;
            }

            return View();
        }

        [HttpPost]
        public IActionResult GuardarPremio(Premio premio, IFormCollection form)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            // Manejar el checkbox Activo - si el checkbox está marcado, envía "true", si no, solo envía el hidden "false"
            var activoValues = form["Activo"].ToArray();
            if (activoValues.Length > 1)
            {
                // Si hay múltiples valores, el último es el checkbox (si está marcado)
                premio.Activo = activoValues[activoValues.Length - 1] == "true";
            }
            else
            {
                // Si solo hay un valor, es el hidden "false" (checkbox no marcado)
                premio.Activo = activoValues[0] == "true";
            }

            if (string.IsNullOrWhiteSpace(premio.Nombre))
            {
                TempData["Mensaje"] = "El nombre es requerido.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("TablaPremios");
            }

            if (premio.Costo <= 0)
            {
                TempData["Mensaje"] = "El costo debe ser mayor a 0.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("TablaPremios");
            }

            // Solo calcular puntos requeridos automáticamente si no se proporcionó un valor válido
            // Si el usuario editó manualmente el campo, respetar ese valor
            if (premio.PuntosRequeridos <= 0)
            {
                // Calcular puntos requeridos automáticamente
                var valorPuntos = _valorPuntosService.GetValorPuntosPorCosto(premio.Costo);
                if (valorPuntos != null && valorPuntos.ValorPorPunto > 0)
                {
                    premio.PuntosRequeridos = (int)Math.Ceiling(premio.Costo / valorPuntos.ValorPorPunto);
                }
                else
                {
                    TempData["Mensaje"] = "No se encontró un valor por punto configurado para este rango de costo. Configure un valor por punto en Administración > Valor Puntos.";
                    TempData["TipoMensaje"] = "error";
                    return RedirectToAction("TablaPremios");
                }
            }
            // Si PuntosRequeridos > 0, significa que el usuario lo editó manualmente, respetar ese valor

            try
            {
                if (premio.Id == 0)
                {
                    if (premio.FechaCreacion == default(DateTime))
                    {
                        premio.FechaCreacion = TimeHelper.GetColombiaTime();
                    }
                    if (premio.Descripcion == null)
                    {
                        premio.Descripcion = string.Empty;
                    }
                    _premioService.AddPremio(premio);
                    _logService.RegistrarLog("Create", "Premio", premio.Id, NombreUsuario, 
                        $"Premio creado - Nombre: {premio.Nombre}, Costo: ${premio.Costo:N0}");
                    TempData["Mensaje"] = "Premio creado exitosamente.";
                }
                else
                {
                    _premioService.UpdatePremio(premio);
                    _logService.RegistrarLog("Update", "Premio", premio.Id, NombreUsuario, 
                        $"Premio actualizado - Nombre: {premio.Nombre}, Costo: ${premio.Costo:N0}");
                    TempData["Mensaje"] = "Premio actualizado exitosamente.";
                }
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("TablaPremios");
        }

        [HttpPost]
        public IActionResult EliminarPremio(int id)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                var premio = _premioService.GetPremioById(id);
                _premioService.DeletePremio(id);
                _logService.RegistrarLog("Delete", "Premio", id, NombreUsuario, 
                    $"Premio eliminado - Nombre: {premio?.Nombre ?? "N/A"}");
                TempData["Mensaje"] = "Premio eliminado exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("TablaPremios");
        }

        [HttpGet]
        public IActionResult CalcularPuntosRequeridos(decimal costo)
        {
            var valorPuntos = _valorPuntosService.GetValorPuntosPorCosto(costo);
            int puntosRequeridos = 0;
            
            if (valorPuntos != null && valorPuntos.ValorPorPunto > 0)
            {
                puntosRequeridos = (int)Math.Ceiling(costo / valorPuntos.ValorPorPunto);
            }
            
            return Json(new { puntosRequeridos });
        }

        [HttpGet]
        public IActionResult ActualizarContrasena()
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;
            return View();
        }

        [HttpPost]
        public IActionResult ActualizarContrasena(string contrasenaActual, string nuevaContrasena, string confirmarNuevaContrasena)
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(contrasenaActual))
            {
                ViewBag.MensajeError = "La contraseña actual es requerida.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(nuevaContrasena))
            {
                ViewBag.MensajeError = "La nueva contraseña es requerida.";
                return View();
            }

            if (nuevaContrasena != confirmarNuevaContrasena)
            {
                ViewBag.MensajeError = "Las contraseñas no coinciden.";
                return View();
            }

            if (nuevaContrasena.Length < 4)
            {
                ViewBag.MensajeError = "La nueva contraseña debe tener al menos 4 caracteres.";
                return View();
            }

            if (!UsuarioId.HasValue)
            {
                ViewBag.MensajeError = "Error: Usuario no identificado.";
                return View();
            }

            var resultado = _authService.ActualizarContrasena(UsuarioId.Value, contrasenaActual, nuevaContrasena);
            if (resultado)
            {
                TempData["Mensaje"] = "Contraseña actualizada exitosamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.MensajeError = "La contraseña actual es incorrecta o hubo un error al actualizar.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Registro()
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario nuevoUsuario, string contrasena, string confirmarContrasena)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(nuevoUsuario.NombreUsuario))
            {
                ViewBag.MensajeError = "El usuario es requerido.";
                return View(nuevoUsuario);
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.MensajeError = "La contraseña es requerida.";
                return View(nuevoUsuario);
            }

            if (contrasena != confirmarContrasena)
            {
                ViewBag.MensajeError = "Las contraseñas no coinciden.";
                return View(nuevoUsuario);
            }

            if (contrasena.Length < 4)
            {
                ViewBag.MensajeError = "La contraseña debe tener al menos 4 caracteres.";
                return View(nuevoUsuario);
            }

            var resultado = _authService.RegistrarUsuario(nuevoUsuario, contrasena);
            if (resultado)
            {
                TempData["Mensaje"] = "Usuario registrado exitosamente.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.MensajeError = "El usuario ya existe o hubo un error al registrar.";
                return View(nuevoUsuario);
            }
        }

        [HttpGet]
        public IActionResult ListadoCalificaciones(string filtroDocumento = "", string filtroNombre = "", string filtroCategoria = "", 
            string filtroProceso = "", string filtroEstado = "", string filtroRadicado = "", 
            string filtroFechaDesde = "", string filtroFechaHasta = "", int pagina = 1)
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            var todasLasIdeas = _ideaService.GetAllIdeas() ?? new List<Idea>();
            
            // Aplicar filtros
            var ideasFiltradas = todasLasIdeas.Where(idea =>
                (string.IsNullOrEmpty(filtroDocumento) || idea.NumeroDocumento.Contains(filtroDocumento, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(filtroNombre) || idea.NombreUsuario.Contains(filtroNombre, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(filtroCategoria) || idea.Categoria == filtroCategoria) &&
                (string.IsNullOrEmpty(filtroProceso) || (!string.IsNullOrEmpty(idea.Proceso) && idea.Proceso.Contains(filtroProceso, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrEmpty(filtroEstado) || idea.Estado == filtroEstado) &&
                (string.IsNullOrEmpty(filtroRadicado) || idea.Radicado.Contains(filtroRadicado, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Filtros de fecha
            if (DateTime.TryParse(filtroFechaDesde, out var fechaDesde))
            {
                ideasFiltradas = ideasFiltradas.Where(i => i.FechaRegistro.Date >= fechaDesde.Date).ToList();
            }
            if (DateTime.TryParse(filtroFechaHasta, out var fechaHasta))
            {
                ideasFiltradas = ideasFiltradas.Where(i => i.FechaRegistro.Date <= fechaHasta.Date).ToList();
            }

            // Paginación
            var registrosPorPagina = 20;
            var totalPaginas = (int)Math.Ceiling((double)ideasFiltradas.Count / registrosPorPagina);
            if (totalPaginas < 1) totalPaginas = 1;
            if (pagina > totalPaginas) pagina = totalPaginas;

            var ideasPaginadas = ideasFiltradas
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();

            ViewBag.Ideas = ideasPaginadas;
            ViewBag.TotalRegistros = ideasFiltradas.Count;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Categorias = _categoriaBDService.GetAllCategorias().Where(c => c.Activo).Select(c => c.Nombre).Distinct().OrderBy(c => c).ToList();
            ViewBag.Estados = _estadoBDService.GetNombresEstados().Distinct().OrderBy(e => e).ToList();
            ViewBag.Procesos = _procesoBDService.GetNombresProcesos().Distinct().OrderBy(p => p).ToList();
            ViewBag.FiltroDocumento = filtroDocumento;
            ViewBag.FiltroNombre = filtroNombre;
            ViewBag.FiltroCategoria = filtroCategoria;
            ViewBag.FiltroProceso = filtroProceso;
            ViewBag.FiltroEstado = filtroEstado;
            ViewBag.FiltroRadicado = filtroRadicado;
            ViewBag.FiltroFechaDesde = filtroFechaDesde;
            ViewBag.FiltroFechaHasta = filtroFechaHasta;

            return View();
        }

        [HttpGet]
        public IActionResult ExportarExcel(string filtroDocumento = "", string filtroNombre = "", string filtroCategoria = "", 
            string filtroProceso = "", string filtroEstado = "", string filtroRadicado = "", 
            string filtroFechaDesde = "", string filtroFechaHasta = "")
        {
            var authResult = RequiereAutenticacion();
            if (authResult != null) return authResult;

            var todasLasIdeas = _ideaService.GetAllIdeas() ?? new List<Idea>();
            
            // Aplicar los mismos filtros que en ListadoCalificaciones
            var ideasFiltradas = todasLasIdeas.Where(idea =>
                (string.IsNullOrEmpty(filtroDocumento) || idea.NumeroDocumento.Contains(filtroDocumento, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(filtroNombre) || idea.NombreUsuario.Contains(filtroNombre, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(filtroCategoria) || idea.Categoria == filtroCategoria) &&
                (string.IsNullOrEmpty(filtroProceso) || (!string.IsNullOrEmpty(idea.Proceso) && idea.Proceso.Contains(filtroProceso, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrEmpty(filtroEstado) || idea.Estado == filtroEstado) &&
                (string.IsNullOrEmpty(filtroRadicado) || idea.Radicado.Contains(filtroRadicado, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            if (DateTime.TryParse(filtroFechaDesde, out var fechaDesde))
            {
                ideasFiltradas = ideasFiltradas.Where(i => i.FechaRegistro.Date >= fechaDesde.Date).ToList();
            }
            if (DateTime.TryParse(filtroFechaHasta, out var fechaHasta))
            {
                ideasFiltradas = ideasFiltradas.Where(i => i.FechaRegistro.Date <= fechaHasta.Date).ToList();
            }

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Calificaciones");
                
                // Encabezados
                int col = 1;
                worksheet.Cell(1, col++).Value = "ID";
                worksheet.Cell(1, col++).Value = "Documento";
                worksheet.Cell(1, col++).Value = "Nombre";
                worksheet.Cell(1, col++).Value = "Celular";
                worksheet.Cell(1, col++).Value = "Radicado";
                worksheet.Cell(1, col++).Value = "Fecha Radicado";
                worksheet.Cell(1, col++).Value = "Categoría";
                worksheet.Cell(1, col++).Value = "Proceso";
                worksheet.Cell(1, col++).Value = "Estado";
                worksheet.Cell(1, col++).Value = "Resumen de la Idea";
                worksheet.Cell(1, col++).Value = "Valor Inversión";
                worksheet.Cell(1, col++).Value = "ROI Meses";
                worksheet.Cell(1, col++).Value = "Facilidad Implem";
                worksheet.Cell(1, col++).Value = "Impactos (JSON)";
                worksheet.Cell(1, col++).Value = "Puntos Extra";
                worksheet.Cell(1, col++).Value = "Comentarios Puntos Extra";
                worksheet.Cell(1, col++).Value = "Observaciones";
                worksheet.Cell(1, col++).Value = "Puntos Valor Inv.";
                worksheet.Cell(1, col++).Value = "Puntos ROI";
                worksheet.Cell(1, col++).Value = "Puntos Fac. Implem.";
                worksheet.Cell(1, col++).Value = "Puntos Impacto";
                worksheet.Cell(1, col++).Value = "Puntos Totales";
                worksheet.Cell(1, col++).Value = "Fecha Registro";
                
                int totalColumns = col - 1;
                
                // Estilo de encabezados
                var headerRange = worksheet.Range(1, 1, 1, totalColumns);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                
                // Datos
                int row = 2;
                foreach (var idea in ideasFiltradas)
                {
                    col = 1;
                    worksheet.Cell(row, col++).Value = idea.Id;
                    worksheet.Cell(row, col++).Value = idea.NumeroDocumento;
                    worksheet.Cell(row, col++).Value = idea.NombreUsuario;
                    worksheet.Cell(row, col++).Value = idea.Celular;
                    worksheet.Cell(row, col++).Value = idea.Radicado;
                    worksheet.Cell(row, col++).Value = idea.FechaRadicado != default(DateTime) ? idea.FechaRadicado.ToString("dd/MM/yyyy") : "";
                    worksheet.Cell(row, col++).Value = idea.Categoria;
                    worksheet.Cell(row, col++).Value = idea.Proceso;
                    worksheet.Cell(row, col++).Value = idea.Estado;
                    worksheet.Cell(row, col++).Value = idea.DescripcionIdea;
                    worksheet.Cell(row, col++).Value = idea.ValorInversion ?? 0;
                    worksheet.Cell(row, col++).Value = idea.RoiMeses ?? "";
                    worksheet.Cell(row, col++).Value = idea.FacilidadImplem ?? "";
                    worksheet.Cell(row, col++).Value = idea.ImpactosJson ?? "{}";
                    worksheet.Cell(row, col++).Value = idea.PuntosExtra;
                    worksheet.Cell(row, col++).Value = idea.ComentariosPuntosExtra ?? "";
                    worksheet.Cell(row, col++).Value = idea.Observaciones ?? "";
                    worksheet.Cell(row, col++).Value = (int)Math.Round(idea.PuntosValorInversion);
                    worksheet.Cell(row, col++).Value = (int)Math.Round(idea.PuntosROI);
                    worksheet.Cell(row, col++).Value = (int)Math.Round(idea.PuntosFacilidadImplem);
                    worksheet.Cell(row, col++).Value = (int)Math.Round(idea.PuntosImpacto);
                    worksheet.Cell(row, col++).Value = (int)Math.Round(idea.PuntosTotales);
                    worksheet.Cell(row, col++).Value = TimeHelper.ToColombiaTime(idea.FechaRegistro).ToString("dd/MM/yyyy HH:mm");
                    row++;
                }
                
                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();
                
                // Guardar en memoria
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"Calificaciones_{TimeHelper.GetColombiaTime():yyyyMMdd_HHmmss}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [HttpGet]
        public IActionResult Calificacion(int? id = null, string modo = "")
        {
            try
            {
                var modoLower = (modo ?? "").ToLower();
                var esModoVer = modoLower == "ver";
                var esModoEditar = modoLower == "editar";
                var esModoCrear = modoLower == "crear" || (string.IsNullOrEmpty(modo) && !id.HasValue);

                // Si no hay id ni modo, es modo crear
                if (!id.HasValue && string.IsNullOrEmpty(modo))
                {
                    modo = "crear";
                    modoLower = "crear";
                    esModoCrear = true;
                }

                // Solo SuperAdmin puede crear o editar
                if (esModoCrear || esModoEditar)
                {
                    var authResult = RequiereSuperAdmin();
                    if (authResult != null) return authResult;
                }
                else if (esModoVer)
                {
                    // Modo ver - requiere autenticación
                    var authResult = RequiereAutenticacion();
                    if (authResult != null) return authResult;
                }
                else
                {
                    // Si no se especifica modo pero hay id, asumir modo editar (requiere SuperAdmin)
                    if (id.HasValue)
                    {
                        var authResult = RequiereSuperAdmin();
                        if (authResult != null) return authResult;
                        modo = "editar";
                    }
                    else
                    {
                        // Sin id ni modo, es modo crear
                        var authResult = RequiereSuperAdmin();
                        if (authResult != null) return authResult;
                        modo = "crear";
                    }
                }

                ViewBag.Modo = modo;
                
                // Cargar datos con manejo de errores
                try
                {
                    ViewBag.Categorias = _categoriaBDService.GetAllCategorias().Where(c => c.Activo).Select(c => c.Nombre).Distinct().OrderBy(c => c).ToList();
                }
                catch
                {
                    ViewBag.Categorias = new List<string>();
                }
                
                try
                {
                    ViewBag.Estados = _estadoBDService.GetNombresEstados().Distinct().OrderBy(e => e).ToList();
                }
                catch
                {
                    ViewBag.Estados = new List<string>();
                }
                
                try
                {
                    ViewBag.Procesos = _procesoBDService.GetNombresProcesos().Distinct().OrderBy(p => p).ToList();
                }
                catch
                {
                    ViewBag.Procesos = new List<string>();
                }

                // Cargar configuraciones de puntos
                try
                {
                    ViewBag.ConfiguracionesROI = _configuracionPuntosService.GetConfiguracionesPorTipo("ROI");
                    ViewBag.ConfiguracionesFacilidadImplem = _configuracionPuntosService.GetConfiguracionesPorTipo("FacilidadImplem");
                }
                catch
                {
                    ViewBag.ConfiguracionesROI = new List<ConfiguracionPuntos>();
                    ViewBag.ConfiguracionesFacilidadImplem = new List<ConfiguracionPuntos>();
                }

                if (id.HasValue)
                {
                    var idea = _ideaService.GetIdeaById(id.Value);
                    if (idea == null)
                    {
                        TempData["Mensaje"] = "No se encontró el registro solicitado.";
                        TempData["TipoMensaje"] = "error";
                        return RedirectToAction("ListadoCalificaciones");
                    }
                    ViewBag.Idea = idea;
                }
                else
                {
                    ViewBag.Idea = null;
                }

                return View();
            }
            catch (Exception ex)
            {
                // Log del error completo para diagnóstico
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
                logger.LogError(ex, "Error en Calificacion: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                
                TempData["Mensaje"] = $"Error al cargar la página: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["Mensaje"] += $" Detalles: {ex.InnerException.Message}";
                }
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("ListadoCalificaciones");
            }
        }

        [HttpGet]
        public IActionResult GetCategoriaImpactos(string nombre)
        {
            try
            {
                var categoriaBD = _categoriaBDService.GetCategoriaPorNombre(nombre);
                if (categoriaBD != null)
                {
                    var categoria = _categoriaBDService.ConvertirACategoriaModel(categoriaBD);
                    if (categoria != null && categoria.Impactos != null)
                    {
                        var impactos = categoria.Impactos.Select(i => new
                        {
                            nombre = i.Nombre,
                            porcentajeMaximo = i.PorcentajeMaximo
                        }).ToList();
                        return Json(new { impactos });
                    }
                }
                
                // Fallback a JSON
                var categoriaService = HttpContext.RequestServices.GetRequiredService<CategoriaService>();
                categoriaService.CargarCategoriasAsync().Wait();
                var categoriaJson = categoriaService.ObtenerPorNombre(nombre);
                if (categoriaJson != null && categoriaJson.Impactos != null)
                {
                    var impactos = categoriaJson.Impactos.Select(i => new
                    {
                        nombre = i.Nombre,
                        porcentajeMaximo = i.PorcentajeMaximo
                    }).ToList();
                    return Json(new { impactos });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }

            return Json(new { impactos = new List<object>() });
        }

        [HttpGet]
        public IActionResult GetUsuarioInfo(string numeroDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroDocumento))
                {
                    return Json(new { success = false, message = "Número de documento requerido" });
                }

                // Buscar la idea más reciente con ese documento para obtener nombre y celular
                var ideaMasReciente = _ideaService.GetAllIdeas()
                    .Where(i => i.NumeroDocumento == numeroDocumento)
                    .OrderByDescending(i => i.FechaRegistro)
                    .FirstOrDefault();

                if (ideaMasReciente != null)
                {
                    return Json(new
                    {
                        success = true,
                        nombreUsuario = ideaMasReciente.NombreUsuario ?? string.Empty,
                        celular = ideaMasReciente.Celular ?? string.Empty
                    });
                }

                return Json(new { success = false, message = "No se encontró información para este documento" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult CalcularPuntos([FromBody] CalcularPuntosRequest request)
        {
            try
            {
                var idea = new Idea
                {
                    ValorInversion = request.ValorInversion,
                    RoiMeses = request.RoiMeses,
                    FacilidadImplem = request.FacilidadImplem,
                    PuntosExtra = request.PuntosExtra,
                    Impactos = request.Impactos ?? new Dictionary<string, decimal>()
                };

                List<ImpactoConfig>? impactosConfig = null;
                if (request.ImpactosConfig != null && request.ImpactosConfig.Any())
                {
                    impactosConfig = request.ImpactosConfig.Select(i => new ImpactoConfig
                    {
                        Nombre = i.Nombre,
                        PorcentajeMaximo = i.PorcentajeMaximo
                    }).ToList();
                }

                FormulaService.CalcularTodosLosPuntos(idea, impactosConfig, _configuracionPuntosService);

                return Json(new
                {
                    puntosValorInversion = idea.PuntosValorInversion,
                    puntosROI = idea.PuntosROI,
                    puntosFacilidadImplem = idea.PuntosFacilidadImplem,
                    puntosImpacto = idea.PuntosImpacto,
                    puntosTotales = idea.PuntosTotales
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GuardarCalificacion(Idea idea, string? modo = null)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                // INICIALIZAR campos calculados a 0 ANTES de cualquier operación
                // Esto previene problemas de model binding cuando los campos no se envían
                idea.PuntosValorInversion = 0;
                idea.PuntosROI = 0;
                idea.PuntosFacilidadImplem = 0;
                idea.PuntosImpacto = 0;
                idea.PuntosTotales = 0;

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(idea.NumeroDocumento))
                {
                    return Json(new { success = false, message = "El campo Documento es obligatorio." });
                }

                if (string.IsNullOrWhiteSpace(idea.NombreUsuario))
                {
                    return Json(new { success = false, message = "El campo Nombre es obligatorio." });
                }

                if (string.IsNullOrWhiteSpace(idea.DescripcionIdea))
                {
                    return Json(new { success = false, message = "El campo Resumen de la idea es obligatorio." });
                }

                if (string.IsNullOrWhiteSpace(idea.Estado))
                {
                    return Json(new { success = false, message = "El campo Estado es obligatorio." });
                }

                if (string.IsNullOrWhiteSpace(idea.Radicado))
                {
                    return Json(new { success = false, message = "El campo Radicado es obligatorio." });
                }

                if (idea.FechaRadicado == default(DateTime))
                {
                    return Json(new { success = false, message = "El campo Fecha Radicado es obligatorio." });
                }

                if (string.IsNullOrWhiteSpace(idea.Proceso))
                {
                    return Json(new { success = false, message = "El campo Proceso es obligatorio." });
                }

                // Normalizar campos obligatorios (trim)
                idea.NumeroDocumento = idea.NumeroDocumento.Trim();
                idea.NombreUsuario = idea.NombreUsuario.Trim();
                idea.DescripcionIdea = idea.DescripcionIdea.Trim();
                idea.Estado = idea.Estado.Trim();
                idea.Radicado = idea.Radicado.Trim();

                // Convertir cadenas vacías a NULL para campos opcionales
                idea.Celular = string.IsNullOrWhiteSpace(idea.Celular) ? null : idea.Celular.Trim();
                idea.Categoria = string.IsNullOrWhiteSpace(idea.Categoria) ? null : idea.Categoria.Trim();
                // Proceso: validar que no esté vacío (obligatorio en formulario), pero puede ser null en BD para registros antiguos
                idea.Proceso = string.IsNullOrWhiteSpace(idea.Proceso) ? null : idea.Proceso.Trim();
                idea.RoiMeses = string.IsNullOrWhiteSpace(idea.RoiMeses) ? null : idea.RoiMeses.Trim();
                idea.FacilidadImplem = string.IsNullOrWhiteSpace(idea.FacilidadImplem) ? null : idea.FacilidadImplem.Trim();
                idea.ComentariosPuntosExtra = string.IsNullOrWhiteSpace(idea.ComentariosPuntosExtra) ? null : idea.ComentariosPuntosExtra.Trim();
                idea.Observaciones = string.IsNullOrWhiteSpace(idea.Observaciones) ? null : idea.Observaciones.Trim();

                // Asegurar que FechaRegistro esté establecido si no viene
                if (idea.FechaRegistro == default(DateTime))
                {
                    idea.FechaRegistro = TimeHelper.GetColombiaTime();
                }

                // Cargar categoría para calcular puntos si existe
                Categoria? categoria = null;
                if (!string.IsNullOrWhiteSpace(idea.Categoria))
                {
                    var categoriaBD = _categoriaBDService.GetCategoriaPorNombre(idea.Categoria);
                    if (categoriaBD != null)
                    {
                        categoria = _categoriaBDService.ConvertirACategoriaModel(categoriaBD);
                    }
                    else
                    {
                        var categoriaService = HttpContext.RequestServices.GetRequiredService<CategoriaService>();
                        categoriaService.CargarCategoriasAsync().Wait();
                        categoria = categoriaService.ObtenerPorNombre(idea.Categoria);
                    }
                }

                // Calcular puntos (con manejo de errores)
                try
                {
                    FormulaService.CalcularTodosLosPuntos(idea, categoria?.Impactos, _configuracionPuntosService);
                }
                catch (Exception calcEx)
                {
                    // Si el cálculo falla, usar valores por defecto (0)
                    // Los valores ya están inicializados a 0 arriba
                }

                // Asegurar valores por defecto DESPUÉS de calcular puntos
                // Esto garantiza que los campos numéricos siempre tengan un valor válido (no null, no negativos)
                idea.PuntosValorInversion = Math.Max(0, idea.PuntosValorInversion);
                idea.PuntosROI = Math.Max(0, idea.PuntosROI);
                idea.PuntosFacilidadImplem = Math.Max(0, idea.PuntosFacilidadImplem);
                idea.PuntosImpacto = Math.Max(0, idea.PuntosImpacto);
                idea.PuntosTotales = Math.Max(0, idea.PuntosTotales);

                if (idea.Id > 0)
                {
                    _ideaService.UpdateIdea(idea);
                    _logService.RegistrarLog("Update", "Idea", idea.Id, NombreUsuario,
                        $"Calificación actualizada - ID: {idea.Id}, Documento: {idea.NumeroDocumento ?? "N/A"}, Puntos: {((int)Math.Round(idea.PuntosTotales))}");
                }
                else
                {
                    _ideaService.AddIdea(idea);
                    // El Id se actualiza automáticamente después de SaveChanges por Entity Framework
                    _logService.RegistrarLog("Create", "Idea", idea.Id, NombreUsuario,
                        $"Nueva calificación creada - Documento: {idea.NumeroDocumento ?? "N/A"}, Puntos: {((int)Math.Round(idea.PuntosTotales))}");
                }

                return Json(new { success = true, id = idea.Id, message = "Registro guardado exitosamente" });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Capturar excepción de base de datos y mostrar detalles internos
                var innerEx = dbEx.InnerException;
                var errorMessage = "Error al guardar en la base de datos.";
                
                if (innerEx != null)
                {
                    errorMessage += $" {innerEx.Message}";
                    
                    // Si hay más excepciones internas, agregarlas
                    var currentEx = innerEx.InnerException;
                    while (currentEx != null)
                    {
                        errorMessage += $" {currentEx.Message}";
                        currentEx = currentEx.InnerException;
                    }
                }
                else
                {
                    errorMessage += $" {dbEx.Message}";
                }
                
                return Json(new { success = false, message = errorMessage });
            }
            catch (Exception ex)
            {
                // Capturar cualquier otra excepción y mostrar detalles
                var errorMessage = $"Error al guardar: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Detalles: {ex.InnerException.Message}";
                }
                return Json(new { success = false, message = errorMessage });
            }
        }

        [HttpPost]
        public IActionResult EliminarCalificacion(int id)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                var idea = _ideaService.GetIdeaById(id);
                if (idea != null)
                {
                    _ideaService.DeleteIdea(id);
                    _logService.RegistrarLog("Delete", "Idea", id, NombreUsuario, 
                        $"Calificación eliminada - ID: {id}, Documento: {idea.NumeroDocumento}, Nombre: {idea.NombreUsuario}");
                    TempData["Mensaje"] = "Registro eliminado exitosamente.";
                    TempData["TipoMensaje"] = "success";
                }
                else
                {
                    TempData["Mensaje"] = "No se encontró el registro a eliminar.";
                    TempData["TipoMensaje"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("ListadoCalificaciones");
        }

        [HttpGet]
        public IActionResult Administracion(string tab = "categorias", string accion = "", int? id = null, int? pagina = null, string? buscar = null, string? filtroTipo = null)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            ViewBag.TabActivo = tab;
            ViewBag.Accion = accion;
            ViewBag.IdEditar = id;
            ViewBag.PaginaActual = pagina ?? 1;
            ViewBag.BuscarTexto = buscar ?? "";
            ViewBag.FiltroTipoConfig = filtroTipo ?? "";
            
            ViewBag.Categorias = _categoriaBDService.GetAllCategorias();
            ViewBag.Estados = _estadoBDService.GetAllEstados();
            ViewBag.Procesos = _procesoBDService.GetAllProcesos();
            ViewBag.ValorPuntos = _valorPuntosService.GetAllValorPuntos();
            
            // Cargar configuraciones de puntos
            var todasConfiguraciones = _configuracionPuntosService.GetAllConfiguraciones();
            if (!string.IsNullOrWhiteSpace(filtroTipo))
            {
                ViewBag.ConfiguracionesPuntos = todasConfiguraciones.Where(c => c.Tipo == filtroTipo).ToList();
            }
            else
            {
                ViewBag.ConfiguracionesPuntos = todasConfiguraciones;
            }
            
            // Cargar datos adicionales para otras pestañas
            var authService = HttpContext.RequestServices.GetRequiredService<AuthService>();
            ViewBag.Usuarios = authService.GetAllUsuarios();
            ViewBag.Logs = _logService.GetAllLogs();
            
            // Para puntos históricos - obtener usuarios únicos de ideas
            var todasLasIdeas = _ideaService.GetAllIdeas();
            ViewBag.TodosLosUsuarios = todasLasIdeas?
                .Where(i => !string.IsNullOrWhiteSpace(i.NombreUsuario))
                .Select(i => i.NombreUsuario)
                .Distinct()
                .OrderBy(u => u)
                .ToList() ?? new List<string>();

            // Cargar datos para editar
            if (accion == "editar" && id.HasValue)
            {
                if (tab == "categorias")
                {
                    ViewBag.CategoriaEditando = _categoriaBDService.GetCategoriaById(id.Value);
                }
                else if (tab == "estados")
                {
                    ViewBag.EstadoEditando = _estadoBDService.GetEstadoById(id.Value);
                }
                else if (tab == "procesos")
                {
                    ViewBag.ProcesoEditando = _procesoBDService.GetProcesoById(id.Value);
                }
                else if (tab == "valor-puntos")
                {
                    ViewBag.ValorPuntosEditando = _valorPuntosService.GetValorPuntosById(id.Value);
                }
                else if (tab == "configuracion-puntos")
                {
                    ViewBag.ConfiguracionPuntosEditando = _configuracionPuntosService.GetConfiguracionById(id.Value);
                }
                else if (tab == "usuarios")
                {
                    ViewBag.UsuarioEditando = authService.GetUsuarioById(id.Value);
                }
            }
            else if (accion == "cambiar-contrasena" && id.HasValue && tab == "usuarios")
            {
                ViewBag.UsuarioEditando = authService.GetUsuarioById(id.Value);
            }
            else if (accion == "nuevo")
            {
                if (tab == "categorias")
                {
                    ViewBag.CategoriaEditando = new CategoriaBD { Activo = true, Impactos = new List<ImpactoBD>() };
                }
                else if (tab == "estados")
                {
                    ViewBag.EstadoEditando = new EstadoBD { Activo = true };
                }
                else if (tab == "procesos")
                {
                    ViewBag.ProcesoEditando = new ProcesoBD { Activo = true };
                }
                else if (tab == "valor-puntos")
                {
                    ViewBag.ValorPuntosEditando = new ValorPuntos { Activo = true };
                }
                else if (tab == "configuracion-puntos")
                {
                    ViewBag.ConfiguracionPuntosEditando = new ConfiguracionPuntos { Activo = true, ValorBase = 22000m, PorcentajeFijos = 0.10m, Orden = 1 };
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult GuardarCategoria(CategoriaBD categoria, string Tab, IFormCollection form)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                TempData["Mensaje"] = "El nombre de la categoría es requerido.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Administracion", new { tab = Tab });
            }

            try
            {
                // Procesar impactos desde el formulario
                var impactos = new List<ImpactoBD>();
                var impactoKeys = form.Keys.Where(k => k.StartsWith("Impactos[") && k.Contains("].Nombre")).ToList();
                
                foreach (var key in impactoKeys)
                {
                    var indexMatch = System.Text.RegularExpressions.Regex.Match(key, @"\[(\d+)\]");
                    if (indexMatch.Success)
                    {
                        var index = int.Parse(indexMatch.Groups[1].Value);
                        var nombre = form[$"Impactos[{index}].Nombre"].ToString();
                        if (!string.IsNullOrWhiteSpace(nombre))
                        {
                            // Manejar checkbox: si hay múltiples valores, verificar si alguno es "true"
                            var activoValues = form[$"Impactos[{index}].Activo"];
                            bool activo = false;
                            if (activoValues.Count > 0)
                            {
                                // Si hay múltiples valores, buscar "true"
                                activo = activoValues.Any(v => v.ToString() == "true");
                            }
                            
                            var impacto = new ImpactoBD
                            {
                                Id = int.TryParse(form[$"Impactos[{index}].Id"].ToString(), out var id) ? id : 0,
                                Nombre = nombre,
                                PorcentajeMaximo = decimal.TryParse(form[$"Impactos[{index}].PorcentajeMaximo"].ToString(), out var porcentaje) ? porcentaje : 0,
                                Orden = int.TryParse(form[$"Impactos[{index}].Orden"].ToString(), out var orden) ? orden : 0,
                                Activo = activo
                            };
                            impactos.Add(impacto);
                        }
                    }
                }

                categoria.Impactos = impactos;
                if (categoria.Id > 0)
                {
                    _categoriaBDService.UpdateCategoria(categoria);
                    _logService.RegistrarLog("Update", "Categoria", categoria.Id, NombreUsuario, 
                        $"Categoría actualizada - Nombre: {categoria.Nombre}");
                    TempData["Mensaje"] = "Categoría actualizada exitosamente.";
                }
                else
                {
                    _categoriaBDService.AddCategoria(categoria);
                    _logService.RegistrarLog("Create", "Categoria", categoria.Id, NombreUsuario, 
                        $"Categoría creada - Nombre: {categoria.Nombre}");
                    TempData["Mensaje"] = "Categoría creada exitosamente.";
                }
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = Tab });
        }

        [HttpPost]
        public IActionResult GuardarPuntosHistoricos(PuntosHistoricos puntosHistoricos, string Tab)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(puntosHistoricos.NombreUsuario))
            {
                TempData["Mensaje"] = "Debe seleccionar un usuario.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Administracion", new { tab = Tab });
            }

            if (puntosHistoricos.Puntos <= 0)
            {
                TempData["Mensaje"] = "Los puntos deben ser mayores a 0.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Administracion", new { tab = Tab });
            }

            // Buscar el número de documento si no está presente
            if (string.IsNullOrWhiteSpace(puntosHistoricos.NumeroDocumento))
            {
                var idea = _ideaService.GetAllIdeas()
                    .FirstOrDefault(i => i.NombreUsuario == puntosHistoricos.NombreUsuario);
                if (idea != null)
                {
                    puntosHistoricos.NumeroDocumento = idea.NumeroDocumento;
                }
                else
                {
                    TempData["Mensaje"] = "No se encontró el número de documento para este usuario.";
                    TempData["TipoMensaje"] = "error";
                    return RedirectToAction("Administracion", new { tab = Tab });
                }
            }

            try
            {
                _puntosHistoricosService.AddPuntosHistoricos(puntosHistoricos);
                _logService.RegistrarLog("Create", "PuntosHistoricos", puntosHistoricos.Id, NombreUsuario, 
                    $"Puntos históricos asignados - Usuario: {puntosHistoricos.NombreUsuario}, Puntos: {puntosHistoricos.Puntos}");
                TempData["Mensaje"] = "Puntos históricos asignados exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = Tab });
        }

        [HttpPost]
        public IActionResult EliminarCategoria(int id)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                _categoriaBDService.DeleteCategoria(id);
                _logService.RegistrarLog("Delete", "Categoria", id, NombreUsuario, "Categoría eliminada");
                TempData["Mensaje"] = "Categoría eliminada exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = "categorias" });
        }

        [HttpPost]
        public IActionResult GuardarEstado(EstadoBD estado, string Tab, string Activo)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            estado.Activo = Activo == "true";

            if (string.IsNullOrWhiteSpace(estado.Nombre))
            {
                TempData["Mensaje"] = "El nombre del estado es requerido.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Administracion", new { tab = Tab });
            }

            try
            {
                if (estado.Id > 0)
                {
                    _estadoBDService.UpdateEstado(estado);
                    _logService.RegistrarLog("Update", "Estado", estado.Id, NombreUsuario, 
                        $"Estado actualizado - Nombre: {estado.Nombre}");
                    TempData["Mensaje"] = "Estado actualizado exitosamente.";
                }
                else
                {
                    _estadoBDService.AddEstado(estado);
                    _logService.RegistrarLog("Create", "Estado", estado.Id, NombreUsuario, 
                        $"Estado creado - Nombre: {estado.Nombre}");
                    TempData["Mensaje"] = "Estado creado exitosamente.";
                }
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = Tab });
        }

        [HttpPost]
        public IActionResult EliminarEstado(int id)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                _estadoBDService.DeleteEstado(id);
                _logService.RegistrarLog("Delete", "Estado", id, NombreUsuario, "Estado eliminado");
                TempData["Mensaje"] = "Estado eliminado exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = "estados" });
        }

        [HttpPost]
        public IActionResult GuardarProceso(ProcesoBD proceso, string Tab, string Activo)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            proceso.Activo = Activo == "true";

            if (string.IsNullOrWhiteSpace(proceso.Nombre))
            {
                TempData["Mensaje"] = "El nombre del proceso es requerido.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Administracion", new { tab = Tab });
            }

            try
            {
                if (proceso.Id > 0)
                {
                    _procesoBDService.UpdateProceso(proceso);
                    _logService.RegistrarLog("Update", "Proceso", proceso.Id, NombreUsuario, 
                        $"Proceso actualizado - Nombre: {proceso.Nombre}");
                    TempData["Mensaje"] = "Proceso actualizado exitosamente.";
                }
                else
                {
                    _procesoBDService.AddProceso(proceso);
                    _logService.RegistrarLog("Create", "Proceso", proceso.Id, NombreUsuario, 
                        $"Proceso creado - Nombre: {proceso.Nombre}");
                    TempData["Mensaje"] = "Proceso creado exitosamente.";
                }
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = Tab });
        }

        [HttpPost]
        public IActionResult EliminarProceso(int id)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                _procesoBDService.DeleteProceso(id);
                _logService.RegistrarLog("Delete", "Proceso", id, NombreUsuario, "Proceso eliminado");
                TempData["Mensaje"] = "Proceso eliminado exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = "procesos" });
        }

        [HttpPost]
        public IActionResult GuardarValorPuntos(ValorPuntos valorPuntos, string Tab, string Activo)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            valorPuntos.Activo = Activo == "true";

            if (valorPuntos.CostoMinimo < 0 || valorPuntos.CostoMaximo < 0 || valorPuntos.ValorPorPunto <= 0)
            {
                TempData["Mensaje"] = "Los valores deben ser válidos.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Administracion", new { tab = Tab });
            }

            try
            {
                if (valorPuntos.Id > 0)
                {
                    _valorPuntosService.UpdateValorPuntos(valorPuntos);
                    _logService.RegistrarLog("Update", "ValorPuntos", valorPuntos.Id, NombreUsuario, 
                        $"Valor Puntos actualizado - Rango: ${valorPuntos.CostoMinimo:N0} - ${valorPuntos.CostoMaximo:N0}");
                    TempData["Mensaje"] = "Valor Puntos actualizado exitosamente.";
                }
                else
                {
                    _valorPuntosService.AddValorPuntos(valorPuntos);
                    _logService.RegistrarLog("Create", "ValorPuntos", valorPuntos.Id, NombreUsuario, 
                        $"Valor Puntos creado - Rango: ${valorPuntos.CostoMinimo:N0} - ${valorPuntos.CostoMaximo:N0}");
                    TempData["Mensaje"] = "Valor Puntos creado exitosamente.";
                }
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = Tab });
        }

        [HttpPost]
        public IActionResult EliminarValorPuntos(int id)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                _valorPuntosService.DeleteValorPuntos(id);
                _logService.RegistrarLog("Delete", "ValorPuntos", id, NombreUsuario, "Valor Puntos eliminado");
                TempData["Mensaje"] = "Valor Puntos eliminado exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = "valor-puntos" });
        }

        [HttpPost]
        public IActionResult GuardarConfiguracionPuntos(ConfiguracionPuntos configuracion, string Tab, string Activo)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            configuracion.Activo = Activo == "true";

            if (string.IsNullOrWhiteSpace(configuracion.Tipo) || string.IsNullOrWhiteSpace(configuracion.Valor))
            {
                TempData["Mensaje"] = "El tipo y el valor son requeridos.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Administracion", new { tab = Tab });
            }

            try
            {
                if (configuracion.Id > 0)
                {
                    _configuracionPuntosService.UpdateConfiguracion(configuracion);
                    _logService.RegistrarLog("Update", "ConfiguracionPuntos", configuracion.Id, NombreUsuario, 
                        $"Configuración actualizada - Tipo: {configuracion.Tipo}, Valor: {configuracion.Valor}");
                    TempData["Mensaje"] = "Configuración actualizada exitosamente.";
                }
                else
                {
                    _configuracionPuntosService.AddConfiguracion(configuracion);
                    _logService.RegistrarLog("Create", "ConfiguracionPuntos", configuracion.Id, NombreUsuario, 
                        $"Configuración creada - Tipo: {configuracion.Tipo}, Valor: {configuracion.Valor}");
                    TempData["Mensaje"] = "Configuración creada exitosamente.";
                }
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = Tab });
        }

        [HttpPost]
        public IActionResult EliminarConfiguracionPuntos(int id)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            try
            {
                _configuracionPuntosService.DeleteConfiguracion(id);
                _logService.RegistrarLog("Delete", "ConfiguracionPuntos", id, NombreUsuario, "Configuración de puntos eliminada");
                TempData["Mensaje"] = "Configuración eliminada exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al eliminar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = "configuracion-puntos" });
        }

        [HttpPost]
        public IActionResult GuardarUsuario(Usuario usuario, string Tab)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            var authService = HttpContext.RequestServices.GetRequiredService<AuthService>();
            
            try
            {
                var usuarioExistente = authService.GetUsuarioById(usuario.Id);
                if (usuarioExistente == null)
                {
                    TempData["Mensaje"] = "Usuario no encontrado.";
                    TempData["TipoMensaje"] = "error";
                    return RedirectToAction("Administracion", new { tab = Tab });
                }

                // Actualizar solo el rol
                authService.ActualizarRol(usuario.Id, usuario.Rol, NombreUsuario, _logService);
                
                TempData["Mensaje"] = "Usuario actualizado exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al guardar: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = Tab });
        }

        [HttpPost]
        public IActionResult CambiarContrasenaUsuario(int id, string nuevaContrasena, string Tab)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(nuevaContrasena) || nuevaContrasena.Length < 6)
            {
                TempData["Mensaje"] = "La contraseña debe tener al menos 6 caracteres.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("Administracion", new { tab = Tab, accion = "cambiar-contrasena", id });
            }

            var authService = HttpContext.RequestServices.GetRequiredService<AuthService>();
            
            try
            {
                authService.CambiarContrasenaUsuario(id, nuevaContrasena, NombreUsuario, _logService);
                
                TempData["Mensaje"] = "Contraseña cambiada exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al cambiar contraseña: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = Tab });
        }

        [HttpPost]
        public IActionResult BloquearDesbloquearUsuario(int id, bool activo)
        {
            var authResult = RequiereSuperAdmin();
            if (authResult != null) return authResult;

            var authService = HttpContext.RequestServices.GetRequiredService<AuthService>();
            
            try
            {
                authService.BloquearDesbloquearUsuario(id, activo, NombreUsuario, _logService);
                
                TempData["Mensaje"] = $"Usuario {(activo ? "desbloqueado" : "bloqueado")} exitosamente.";
                TempData["TipoMensaje"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error: {ex.Message}";
                TempData["TipoMensaje"] = "error";
            }

            return RedirectToAction("Administracion", new { tab = "usuarios" });
        }
    }

    public class CalcularPuntosRequest
    {
        public decimal? ValorInversion { get; set; }
        public string? RoiMeses { get; set; }
        public string? FacilidadImplem { get; set; }
        public decimal PuntosExtra { get; set; }
        public Dictionary<string, decimal>? Impactos { get; set; }
        public List<ImpactoConfigRequest>? ImpactosConfig { get; set; }
    }

    public class ImpactoConfigRequest
    {
        public string Nombre { get; set; } = "";
        public decimal PorcentajeMaximo { get; set; }
    }
}

