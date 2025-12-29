using CalificacionXPuntosWeb.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CalificacionXPuntosWeb.Shared
{
    public class AuthComponentBase : ComponentBase
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
        [Inject] protected AuthService AuthService { get; set; } = null!;

        protected bool estaAutenticado = false;
        protected bool esSuperAdmin = false;
        protected string nombreUsuario = string.Empty;
        protected int? usuarioId = null;

        protected override async Task OnInitializedAsync()
        {
            await VerificarAutenticacion();
        }

        protected async Task VerificarAutenticacion()
        {
            var usuarioIdStr = await JSRuntime.InvokeAsync<string>("sessionStorage.getItem", "usuarioId");
            var rol = await JSRuntime.InvokeAsync<string>("sessionStorage.getItem", "rol");
            nombreUsuario = await JSRuntime.InvokeAsync<string>("sessionStorage.getItem", "nombreUsuario") ?? string.Empty;

            estaAutenticado = !string.IsNullOrEmpty(usuarioIdStr);
            esSuperAdmin = rol == "SuperAdmin";

            if (!string.IsNullOrEmpty(usuarioIdStr))
            {
                usuarioId = int.Parse(usuarioIdStr);
            }
        }

        protected async Task RequiereAutenticacion()
        {
            await VerificarAutenticacion();
            if (!estaAutenticado)
            {
                NavigationManager.NavigateTo("/login");
            }
        }

        protected async Task RequiereSuperAdmin()
        {
            await RequiereAutenticacion();
            if (!esSuperAdmin)
            {
                await JSRuntime.InvokeVoidAsync("Swal.fire", new
                {
                    icon = "error",
                    title = "Acceso Denegado",
                    text = "Solo los SuperAdmin pueden acceder a esta sección.",
                    confirmButtonText = "Aceptar"
                });
                NavigationManager.NavigateTo("/");
            }
        }
    }
}

