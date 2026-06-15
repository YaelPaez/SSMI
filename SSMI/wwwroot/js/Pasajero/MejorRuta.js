
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

var Ruta = [];

window.onload = IniciarMapa();
/* ═══════════════════════════════════════════════════════════════════ */
/* 🗺️ INICIALIZAR MAPA                                               */
/* ═══════════════════════════════════════════════════════════════════ */

function IniciarMapa() {
    Ruta = JSON.parse(sessionStorage.getItem("rutaCalculada"));

    console.log(Ruta);

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

        PintarRuta();

    } catch (error) {
        console.error('❌ Error al inicializar mapa:', error);
    }
}

function PintarRuta() {
    try {
        var primerparada = false;
        const coordenadas = [];
        Ruta.forEach(Instruccion => {
            console.log("Punto: " + Instruccion.posicionLat + ", " + Instruccion.posicionLon);

            coordenadas.push([
                Instruccion.posicionLat,
                Instruccion.posicionLon
            ]);

            if (Instruccion.tipo == "CAMINAR") {
                L.circleMarker(
                    [Instruccion.posicionLat, Instruccion.posicionLon],
                    {
                        radius: 8,
                        fillColor: 'green',
                        color: 'black',   // borde
                        weight: 2,        // grosor del borde
                        opacity: 1,
                        fillOpacity: 1
                    }
                ).addTo(window.map);
            }

            if (Instruccion.tipo == "AUTOBUS") {
                L.circleMarker(
                    [Instruccion.posicionLat, Instruccion.posicionLon],
                    {
                        radius: 8,
                        fillColor: 'purple',
                        color: 'black',   // borde
                        weight: 2,        // grosor del borde
                        opacity: 1,
                        fillOpacity: 1
                    }
                ).addTo(window.map);
            }
            if (Instruccion.tipo == "AUTOBUS" && primerparada == false) {

                primerparada = true;

                var paradaCercanaIcon = L.icon({
                    iconUrl: '/Imagenes/paradaCercana.png',
                    iconSize: [30, 30],
                    iconAnchor: [15, 30]
                });

                L.marker(
                    [Instruccion.posicionLat, Instruccion.posicionLon],
                    {
                        icon: paradaCercanaIcon
                    }
                ).addTo(window.map);
            }

        });

        window.rutaPolilinea = L.polyline(coordenadas, {
            color: 'blue',
            weight: 5
        }).addTo(window.map);

        window.map.fitBounds(
            window.rutaPolilinea.getBounds()
        );
        
    } catch (err) {
        console.log("Error al pintar ruta: " + err)
    }

    MostrarDetalles();
}

function MostrarDetalles() {

    const contenedor = document.getElementById("card-instrucciones");
    contenedor.innerHTML = "";

    let paso = 1;
    let inicio = 0;

    for (let i = 1; i <= Ruta.length; i++) {

        const cambioTipo =
            i === Ruta.length ||
            Ruta[i].tipo !== Ruta[inicio].tipo;

        if (!cambioTipo) continue;

        const segmento = Ruta.slice(inicio, i);

        const distancia = segmento.reduce(
            (s, x) => s + (x.distancia || 0),
            0
        );

        const tiempo = segmento.reduce(
            (s, x) => s + (x.tiempo || 0),
            0
        );

        let titulo = "";
        let detalle = "";

        if (segmento[0].tipo === "CAMINAR") {

            titulo = paso === 1
                ? "Camina a la parada"
                : "Camina a tu destino";

            detalle =
                `${Math.round(distancia)} m - ${Math.ceil(tiempo / 60)} min`;
        }
        else {

            titulo = "Viaja en autobús";

            const numParadas = segmento.length - 1;

            detalle =
                `${numParadas} paradas - ${Math.ceil(tiempo / 60)} min`;
        }

        AgregarPaso(paso++, titulo, detalle);

        inicio = i;
    }
}

function AgregarPaso(numero, titulo, detalle) {

    const contenedor =
        document.getElementById("card-instrucciones");

    const div = document.createElement("div");

    div.className = "paso-ruta";

    div.innerHTML = `
        <div class="numero-paso">${numero}</div>

        <div class="contenido-paso">
            <strong>${titulo}</strong>
            <small>${detalle}</small>
        </div>
    `;

    contenedor.appendChild(div);
}


