/* =====================================================
   SSMI — Accesibilidad visual (compartido entre vistas)
   Keys de localStorage: 'daltonismMode', 'darkMode', 'fontSize'
===================================================== */

function applyDaltonism(mode) {
    const classes = ['protanopia', 'deuteranopia', 'tritanopia'];
    document.documentElement.classList.remove(...classes);
    document.body.classList.remove(...classes);
    if (mode !== 'normal') {
        document.documentElement.classList.add(mode);
        document.body.classList.add(mode);
    }
}

function applyDarkMode(isDark) {
    document.body.classList.toggle('dark-mode', isDark);
    // Si existe el toggle en la vista de Configuración, sincronizarlo
    const toggle = document.getElementById('ssmiDarkToggle');
    if (toggle) toggle.checked = isDark;
}

function applyFontSize(size) {
    const map = { sm: '0.875rem', md: '1rem', lg: '1.125rem', xl: '1.25rem' };
    const sz = map[size] || '1rem';

    document.documentElement.style.setProperty('--font-size-base', sz);
    document.documentElement.style.fontSize = sz;
    document.body.style.fontSize = sz;

    const classes = ['font-size-sm', 'font-size-md', 'font-size-lg', 'font-size-xl'];
    document.documentElement.classList.remove(...classes);
    document.body.classList.remove(...classes);
    document.documentElement.classList.add('font-size-' + size);
    document.body.classList.add('font-size-' + size);
}

function applyStoredPrefs() {
    applyDaltonism(localStorage.getItem('daltonismMode') || 'normal');
    applyDarkMode(localStorage.getItem('darkMode') === 'true');
    applyFontSize(localStorage.getItem('fontSize') || 'md');
}

// Se ejecuta inmediatamente para evitar parpadeo al cargar la página
applyStoredPrefs();