// Ocultar modal de reconexión de Blazor Server
(function() {
    'use strict';
    
    // Función para ocultar el modal de reconexión de forma agresiva
    function hideReconnectModal() {
        // Ocultar por ID
        const modalById = document.getElementById('components-reconnect-modal');
        if (modalById) {
            modalById.style.display = 'none !important';
            modalById.style.visibility = 'hidden !important';
            modalById.style.opacity = '0 !important';
            modalById.style.pointerEvents = 'none !important';
            modalById.setAttribute('style', 'display: none !important; visibility: hidden !important; opacity: 0 !important; pointer-events: none !important;');
        }
        
        // Ocultar por clases
        const modalsByClass = document.querySelectorAll('.components-reconnect-modal, .components-reconnect-show, .components-reconnect-hide, .components-reconnect-failed, .components-reconnect-rejected, [id*="reconnect"], [class*="reconnect"]');
        modalsByClass.forEach(function(modal) {
            modal.style.display = 'none';
            modal.style.visibility = 'hidden';
            modal.style.opacity = '0';
            modal.style.pointerEvents = 'none';
            modal.setAttribute('style', 'display: none !important; visibility: hidden !important; opacity: 0 !important; pointer-events: none !important;');
        });
        
        // Buscar elementos que contengan texto de reconexión
        const allElements = document.querySelectorAll('*');
        allElements.forEach(function(element) {
            const text = element.textContent || element.innerText || '';
            if (text.includes('Attempting to reconnect') || 
                text.includes('Reconnecting') || 
                text.includes('reconnect to the server') ||
                text.includes('of 8') ||
                (element.id && element.id.includes('reconnect')) ||
                (element.className && element.className.toString().includes('reconnect'))) {
                element.style.display = 'none';
                element.style.visibility = 'hidden';
                element.style.opacity = '0';
                element.style.pointerEvents = 'none';
                element.setAttribute('style', 'display: none !important; visibility: hidden !important; opacity: 0 !important; pointer-events: none !important;');
            }
        });
    }
    
    // Ejecutar inmediatamente
    hideReconnectModal();
    
    // Observar cambios en el DOM para detectar cuando Blazor crea el modal
    const observer = new MutationObserver(function(mutations) {
        hideReconnectModal();
    });
    
    // Función para iniciar observación
    function startObserving() {
        if (document.body) {
            observer.observe(document.body, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['class', 'id', 'style']
            });
        }
    }
    
    // Iniciar observación cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            startObserving();
            hideReconnectModal();
        });
    } else {
        startObserving();
        hideReconnectModal();
    }
    
    // También ejecutar periódicamente como respaldo (cada 50ms para ser más agresivo)
    setInterval(hideReconnectModal, 50);
    
    // Ejecutar después de que Blazor se haya cargado completamente
    if (window.Blazor) {
        window.addEventListener('load', function() {
            setTimeout(hideReconnectModal, 100);
        });
    }
})();


