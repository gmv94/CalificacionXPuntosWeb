// Ocultar modal de reconexión de Blazor Server
(function() {
    'use strict';
    
    // Función para ocultar el modal de reconexión
    function hideReconnectModal() {
        // Ocultar por ID
        const modalById = document.getElementById('components-reconnect-modal');
        if (modalById) {
            modalById.style.display = 'none';
            modalById.style.visibility = 'hidden';
            modalById.style.opacity = '0';
            modalById.style.pointerEvents = 'none';
        }
        
        // Ocultar por clases
        const modalsByClass = document.querySelectorAll('.components-reconnect-modal, .components-reconnect-show, .components-reconnect-hide, .components-reconnect-failed, .components-reconnect-rejected');
        modalsByClass.forEach(function(modal) {
            modal.style.display = 'none';
            modal.style.visibility = 'hidden';
            modal.style.opacity = '0';
            modal.style.pointerEvents = 'none';
        });
    }
    
    // Ejecutar inmediatamente
    hideReconnectModal();
    
    // Observar cambios en el DOM para detectar cuando Blazor crea el modal
    const observer = new MutationObserver(function(mutations) {
        hideReconnectModal();
    });
    
    // Iniciar observación cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            observer.observe(document.body, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['class', 'id']
            });
            hideReconnectModal();
        });
    } else {
        observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['class', 'id']
        });
        hideReconnectModal();
    }
    
    // También ejecutar periódicamente como respaldo
    setInterval(hideReconnectModal, 100);
})();

