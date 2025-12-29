using System.Text.Json;
using CalificacionXPuntosWeb.Models;
using Microsoft.AspNetCore.Components;

namespace CalificacionXPuntosWeb.Services
{
    public class CategoriaService
    {
        private ConfiguracionCategorias? _configuracion;
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;

        public CategoriaService(HttpClient httpClient, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
        }

        public async Task CargarCategoriasAsync()
        {
            try
            {
                var baseUrl = _navigationManager.BaseUri;
                var jsonUrl = $"{baseUrl}categorias.json";
                var json = await _httpClient.GetStringAsync(jsonUrl);
                _configuracion = JsonSerializer.Deserialize<ConfiguracionCategorias>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                _configuracion = new ConfiguracionCategorias();
            }
        }

        public List<string> ObtenerNombresCategorias()
        {
            return _configuracion?.Categorias?.Select(c => c.Nombre).ToList() ?? new List<string>();
        }

        public Categoria? ObtenerPorNombre(string nombre)
        {
            return _configuracion?.Categorias?.FirstOrDefault(c => c.Nombre == nombre);
        }

        public List<Categoria> ObtenerTodas()
        {
            return _configuracion?.Categorias ?? new List<Categoria>();
        }
    }
}

