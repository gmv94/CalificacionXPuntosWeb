using CalificacionXPuntosWeb.Data;
using CalificacionXPuntosWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CalificacionXPuntosWeb.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private LogService? _logService;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void SetLogService(LogService logService)
        {
            _logService = logService;
        }

        public Usuario? Login(string nombreUsuario, string contrasena)
        {
            try
            {
                var usuario = _context.Usuarios
                    .FirstOrDefault(u => u.NombreUsuario == nombreUsuario && u.Activo);

                if (usuario == null)
                    return null;

                if (VerificarContrasena(contrasena, usuario.ContrasenaHash))
                {
                    usuario.UltimoAcceso = DateTime.UtcNow;
                    _context.SaveChanges();
                    
                    // Registrar log de inicio de sesión
                    _logService?.RegistrarLog("Login", "Usuario", usuario.Id, usuario.NombreUsuario, 
                        $"Inicio de sesión exitoso - Usuario: {usuario.NombreUsuario}, Rol: {usuario.Rol}");
                    
                    return usuario;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public bool RegistrarUsuario(Usuario usuario, string contrasena)
        {
            try
            {
                // Verificar si el usuario ya existe
                if (_context.Usuarios.Any(u => u.NombreUsuario == usuario.NombreUsuario))
                {
                    return false;
                }

                usuario.ContrasenaHash = HashContrasena(contrasena);
                usuario.FechaCreacion = DateTime.UtcNow;
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();
                
                // Registrar log de creación
                _logService?.RegistrarLog("Create", "Usuario", usuario.Id, usuario.NombreUsuario, 
                    $"Usuario creado - Nombre: {usuario.NombreUsuario}, Rol: {usuario.Rol}");
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ActualizarContrasena(int usuarioId, string contrasenaActual, string nuevaContrasena)
        {
            try
            {
                var usuario = _context.Usuarios.Find(usuarioId);
                if (usuario == null)
                    return false;

                if (!VerificarContrasena(contrasenaActual, usuario.ContrasenaHash))
                    return false;

                usuario.ContrasenaHash = HashContrasena(nuevaContrasena);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Usuario? GetUsuarioById(int id)
        {
            try
            {
                return _context.Usuarios.Find(id);
            }
            catch
            {
                return null;
            }
        }

        public List<Usuario> GetAllUsuarios()
        {
            try
            {
                return _context.Usuarios.ToList();
            }
            catch
            {
                return new List<Usuario>();
            }
        }

        public bool ActualizarRol(int usuarioId, string nuevoRol, string usuarioModificador, LogService? logService = null)
        {
            try
            {
                var usuario = _context.Usuarios.Find(usuarioId);
                if (usuario == null)
                    return false;

                var rolAnterior = usuario.Rol;
                usuario.Rol = nuevoRol;
                _context.SaveChanges();
                
                logService?.RegistrarLog("Update", "Usuario", usuarioId, usuarioModificador, 
                    $"Rol actualizado - Usuario: {usuario.NombreUsuario}, Rol anterior: {rolAnterior}, Rol nuevo: {nuevoRol}");
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CambiarContrasenaUsuario(int usuarioId, string nuevaContrasena, string usuarioModificador, LogService? logService = null)
        {
            try
            {
                var usuario = _context.Usuarios.Find(usuarioId);
                if (usuario == null)
                    return false;

                usuario.ContrasenaHash = HashContrasena(nuevaContrasena);
                _context.SaveChanges();
                
                logService?.RegistrarLog("Update", "Usuario", usuarioId, usuarioModificador, 
                    $"Contraseña cambiada - Usuario: {usuario.NombreUsuario}");
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool BloquearDesbloquearUsuario(int usuarioId, bool activo, string usuarioModificador, LogService? logService = null)
        {
            try
            {
                var usuario = _context.Usuarios.Find(usuarioId);
                if (usuario == null)
                    return false;

                usuario.Activo = activo;
                _context.SaveChanges();
                
                logService?.RegistrarLog("Update", "Usuario", usuarioId, usuarioModificador, 
                    $"Usuario {(activo ? "desbloqueado" : "bloqueado")} - Usuario: {usuario.NombreUsuario}");
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EliminarUsuario(int usuarioId, string usuarioModificador, LogService? logService = null)
        {
            try
            {
                var usuario = _context.Usuarios.Find(usuarioId);
                if (usuario == null)
                    return false;

                var nombreUsuario = usuario.NombreUsuario;
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
                
                logService?.RegistrarLog("Delete", "Usuario", usuarioId, usuarioModificador, 
                    $"Usuario eliminado - Usuario: {nombreUsuario}");
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string HashContrasena(string contrasena)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contrasena));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerificarContrasena(string contrasena, string hash)
        {
            var hashContrasena = HashContrasena(contrasena);
            return hashContrasena == hash;
        }
    }
}

