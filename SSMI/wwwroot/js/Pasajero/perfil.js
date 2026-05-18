document.addEventListener('DOMContentLoaded', function () {
    const btnEditar = document.getElementById('btnEditarPerfil');
    const btnCancelar = document.getElementById('btnCancelarEdicion');
    const card = document.getElementById('cardEditarPerfil');

    function mostrarCard() {
        if (!card) return;
        card.style.display = 'block';
        card.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    function ocultarCard() {
        if (!card) return;
        card.style.display = 'none';
    }

    btnEditar?.addEventListener('click', function () {
        if (!card) return;
        const visible = card.style.display === 'block';
        if (visible) ocultarCard();
        else mostrarCard();
    });

    btnCancelar?.addEventListener('click', function () {
        ocultarCard();
    });
});
