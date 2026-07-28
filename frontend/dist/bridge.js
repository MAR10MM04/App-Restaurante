/* Conecta las acciones de las maquetas HTML originales con las rutas React. */
(() => {
  const file = location.pathname.split('/').pop();
  
  // Global navigation mapping (Icons and Keywords)
  const globalNav = {
    // Bottom Nav
    'home': '/home',
    'search': '/buscar',
    'receipt_long': '/historial',
    'person': '/perfil',
    'favorite': '/home', // Fallback for favorites if missing
    
    // Header actions
    'shopping_cart': '/carrito',
    'arrow_back': 'back', // Special keyword for back
  };

  const routes = {
    'splash.html': { 'comenzar': '/bienvenida' },
    'bienvenida.html': { 'crear cuenta': '/registro', 'iniciar sesi': '/login', 'cuenta': '/login' },
    'login.html': { 'ingresar': '/home', 'iniciar sesi': '/home', 'google': '/home', 'regístrate': '/registro' },
    'registro.html': { 'continuar': '/permisos', 'inicia sesi': '/login' },
    'permisos.html': { 'permitir': '/home', 'ahora no': '/home' },
    'home.html': { 'pedir ahora': '/restaurante', 'ver todo': '/buscar', 'pizza': '/categorias', 'hamburguesas': '/categorias', 'sushi': '/categorias', 'pollo': '/categorias', 'postres': '/categorias', 'bebidas': '/categorias', 'pizzeria': '/restaurante', 'sushi zen': '/restaurante', 'pollo crujiente': '/restaurante' },
    'buscar.html': { 'burger house': '/restaurante', 'smash & co': '/restaurante', 'green garden': '/restaurante', 'pueblo burger': '/restaurante', 'slider hub': '/restaurante', 'fire chicken': '/restaurante' },
    'restaurante.html': { 'agregar': '/producto', 'producto': '/producto', 'burger': '/producto' },
    'producto.html': { 'agregar': '/carrito' },
    'carrito.html': { 'continuar': '/confirmar-pedido' },
    'confirmar.html': { 'realizar pedido': '/pedido-realizado' },
    'pedido-realizado.html': { 'seguir': '/seguimiento', 'inicio': '/home', 'volver': '/home' },
    'seguimiento.html': { 'entregado': '/pedido-entregado', 'simular': '/pedido-entregado' },
    'calificacion.html': { 'enviar': '/historial', 'inicio': '/home' },
    'historial.html': { 'repetir': '/restaurante' },
    'perfil.html': { 'cerrar sesi': '/bienvenida' }
  };

  document.addEventListener('click', (event) => {
    const target = event.target.closest('button, a, article, .cursor-pointer, .group');
    if (!target) return;
    
    const text = `${target.innerText || ''} ${target.textContent || ''}`.toLowerCase();
    let navigateTo = null;
    
    // 1. Check for data-icon or text matches for global navigation
    const icons = Array.from(target.querySelectorAll('.material-symbols-outlined')).map(el => el.textContent.trim().toLowerCase());
    if (target.classList.contains('material-symbols-outlined')) {
      icons.push(target.textContent.trim().toLowerCase());
    }
    // Also check attributes like data-icon if present
    const dataIcon = target.getAttribute('data-icon') || (target.querySelector('[data-icon]')?.getAttribute('data-icon'));
    if (dataIcon) icons.push(dataIcon.toLowerCase());

    for (const icon of icons) {
      if (globalNav[icon]) {
        navigateTo = globalNav[icon];
        break;
      }
    }

    // 2. Fallback to specific screen routes
    if (!navigateTo) {
      const match = Object.entries(routes[file] || {}).find(([key]) => text.includes(key));
      if (match) {
        navigateTo = match[1];
      }
    }
    
    // 3. Fallback for restaurant cards in home and search (if it looks like a card)
    if (!navigateTo && (file === 'home.html' || file === 'buscar.html')) {
        if (target.querySelector('img') && target.querySelector('h3, h4')) {
            navigateTo = '/restaurante';
        }
    }
    
    // 4. Fallback for category clicks in home
    if (!navigateTo && file === 'home.html') {
        if (target.closest('.shrink-0')) {
            navigateTo = '/categorias';
        }
    }
    
    // 5. Fallback for product rows in restaurante
    if (!navigateTo && file === 'restaurante.html') {
        if (target.querySelector('img') && target.querySelector('h3, h4')) {
            navigateTo = '/producto';
        }
    }

    if (navigateTo) {
      event.preventDefault();
      event.stopImmediatePropagation();
      parent.postMessage({ type: 'didi-navigate', to: navigateTo }, '*');
    }
  }, true);
})();
