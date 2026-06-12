window.onload = IniciarMapa();
/* 
╔══════════════════════════════════════════════════════════════════╗
║          VISTA: MEJOR RUTA - CÁLCULO Y VISUALIZACIÓN             ║
║          Utiliza Leaflet, Nominatim y rutas en autobús           ║
╚══════════════════════════════════════════════════════════════════╝
*/

// Variables globales para el mapa y marcadores
window.map = null;
window.marcadores = {};
window.rutaPolilinea = null;
window.rutaActual = null;

const Ruta = JSON.parse(sessionStorage.getItem("rutaCalculada");


console.log(Ruta);


/* ═══════════════════════════════════════════════════════════════════ */
/* 🗺️ INICIALIZAR MAPA                                               */
/* ═══════════════════════════════════════════════════════════════════ */

function IniciarMapa() {
    try {
        // Verificar que Leaflet esté disponible
        if (typeof L === 'undefined') {
            console.error('❌ Leaflet no está cargado');
            return;
        }

        // Verificar que el contenedor exista
        const mapContainer = document.getElementById('map');
        if (!mapContainer) {
            console.error('❌ Contenedor #map no encontrado');
            return;
        }

        console.log('🗺️ Inicializando mapa...');

        // Crear mapa centrado en CDMX
        window.map = L.map('map').setView([19.4326, -99.1332], 13);

        // Agregar capa de OpenStreetMap
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19,
            minZoom: 2
        }).addTo(window.map);

        console.log('✅ Mapa inicializado correctamente');

        // Cargar y dibujar ruta calculada
        cargarRutaCalculada();

    } catch (error) {
        console.error('❌ Error al inicializar mapa:', error);
    }
}
