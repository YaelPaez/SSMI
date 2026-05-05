/* =====================================================
   SSMI — Sidebar toggle (compartido entre vistas)
===================================================== */
document.addEventListener('DOMContentLoaded', () => {
    const btn = document.getElementById('btnToggleMenu');
    const sidebar = document.getElementById('sidebarMenu');

    btn?.addEventListener('click', (e) => {
        e.stopPropagation();
        sidebar.classList.toggle('sidebar-hidden');
    });

    // Cerrar sidebar al hacer click en cualquier lado
    document.addEventListener('click', (e) => {
        if (window.innerWidth <= 768) {
            const clickedInside = sidebar.contains(e.target) || btn.contains(e.target);
            if (!clickedInside) {
                sidebar.classList.add('sidebar-hidden');
            }
        }
    });
});