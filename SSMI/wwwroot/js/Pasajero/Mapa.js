window.onload = () => {
    IniciarMapa();
    ObtenerPosicion();
};

var posicionUsuario = [0, 0];
var marcadorUsuario = null;
var paradasJson = [];
var paradasCercanasJson = [];
var map;
var markersParadas = {};

/* ───────────────────────────── */
/* 🗺️ MAPAaaAA */
/* ───────────────────────────── */
function IniciarMapa() {

    map = L.map('map').setView([19.4326, -99.1332], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    console.log("Mapa inicializado");
}

/* ───────────────────────────── */
/* 📍 UBICACIÓN */
/* ───────────────────────────── */
function ObtenerPosicion() {

    navigator.geolocation.getCurrentPosition(
        async function (position) {

            posicionUsuario = [
                position.coords.latitude,
                position.coords.longitude
            ];

            console.log("Ubicación:", posicionUsuario);

            CrearMarcadorUsuario();
            map.setView(posicionUsuario, 16);

            await obtenerParadas();
            await obtenerParadasCercanas();  // 🔥 aquí ya se ejecuta OSRM

        },
        function (error) {
            console.error("Error ubicación:", error);
        },
        {
            enableHighAccuracy: true
        }
    );
}

/* ───────────────────────────── */
/* 📌 MARCADOR USUARIO */
/* ───────────────────────────── */
var usuarioUbiIcon = L.icon({
    iconUrl: '/Imagenes/focus.png',
    iconSize: [30, 30],
    iconAnchor: [15, 30]
});

function CrearMarcadorUsuario() {

    if (!marcadorUsuario) {
        marcadorUsuario = L.marker(posicionUsuario, { icon: usuarioUbiIcon })
            .addTo(map)
            .bindPopup("Estás aquí 📍")
            .openPopup();
    } else {
        marcadorUsuario.setLatLng(posicionUsuario);
    }
}

/* ───────────────────────────── */
/* 🚌 PARADAS */
/* ───────────────────────────── */
async function obtenerParadas() {

    try {
        console.log("🚀 Obteniendo todas las paradas...");

        const response = await fetch('/api/paradas/obtener-todas');

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        console.log("✅ Paradas obtenidas:", data.length);

        paradasJson = data;

        PintarParadas();

    } catch (error) {
        console.error("❌ Error paradas:", error);
    }
}

/* ───────────────────────────── */
/* 📍 PARADAS CERCANAS */
/* ───────────────────────────── */
async function obtenerParadasCercanas() {

    try {
        console.log("🚀 Obteniendo paradas cercanas...");

        const response = await fetch('/api/paradas/obtener-cercanas', 
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    Lat: posicionUsuario[0],
                    Lon: posicionUsuario[1]
                })
            }
        );

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        console.log("✅ Paradas cercanas obtenidas:", data.length);

        paradasCercanasJson = data;

        console.log("Paradas cercanas:", data);

        MarcarParadasCercanas();

        // 🔥 AQUÍ ya tienes datos → ahora sí OSRM
        await SacarDistanciasYTiemposDeParadasCercanas();

    } catch (error) {
        console.error("❌ Error cercanas:", error);
    }
}

/* ───────────────────────────── */
/* 🎨 ICONOS */
/* ───────────────────────────── */
var paradaIcon = L.icon({
    iconUrl: '/Imagenes/bus-stop.png',
    iconSize: [30, 30],
    iconAnchor: [15, 30]
});

var paradaCercanaIcon = L.icon({
    iconUrl: '/Imagenes/paradaCercana.png',
    iconSize: [30, 30],
    iconAnchor: [15, 30]
});

/* ───────────────────────────── */
/* 📍 PINTAR PARADAS */
/* ───────────────────────────── */
function PintarParadas() {

    Object.values(markersParadas).forEach(m => map.removeLayer(m));
    markersParadas = {};

    paradasJson.forEach(parada => {

        const lat = parseFloat(parada.lat);
        const lon = parseFloat(parada.lon);

        if (isNaN(lat) || isNaN(lon)) return;

        const marker = L.marker([lat, lon], { icon: paradaIcon })
            .addTo(map)
            .bindPopup("Parada");

        markersParadas[parada.idParada] = marker;
    });
}

/* ───────────────────────────── */
/* 📍  MARCAR CERCANAS           */
/* ───────────────────────────── */
function MarcarParadasCercanas() {

    paradasCercanasJson.forEach(cercana => {

        const marker = markersParadas[cercana.idParada];

        if (marker) {
            marker.setIcon(paradaCercanaIcon);
        }
    });
}

/* ───────────────────────────── */
/* 🧮 DISTANCIA Y TIEMPO (OSRM) */
/* ───────────────────────────── */
async function SacarDistanciasYTiemposDeParadasCercanas() {

    console.log("🚀 Calculando rutas OSRM...");
    console.log("Total cercanas:", paradasCercanasJson.length);

    for (const cerca of paradasCercanasJson) {

        const url = `https://ssmi.site/graphhopper/route?profile=foot&ch.disable=true&point=${posicionUsuario[0]},${posicionUsuario[1]}&point=${cerca.lat},${cerca.lon}`;
        
        try {

            const response = await fetch(url);
            const data = await response.json();
            console.log(data);
            if (!data.paths || data.paths.length === 0) {
                console.warn("Sin ruta para:", cerca.idParada);
                continue;
            }

            const ruta = data.paths[0];

            // 🔥 GUARDAR EN EL OBJETO (CLAVE)
            cerca.distancia = ruta.distance;
            cerca.tiempo = ruta.time / 1000;

            console.log("link:", url);
            console.log("🚌 Parada:", cerca.idParada);
            console.log("📏 Distancia:", ruta.distance, "m");
            console.log("⏱ Tiempo:", ruta.duration, "s");

        } catch (error) {
            console.error("Error OSRM:", error);
        }
    }

    // 🔥 ordenar por menor tiempo
    paradasCercanasJson.sort((a, b) => a.tiempo - b.tiempo);

    console.log("🏆 Mejor parada:", paradasCercanasJson[0]);

    // Renderizar paradas en el listado
    ActualizarListadoParadas();
}

/* ───────────────────────────── */
/* 📋 ACTUALIZAR LISTADO PARADAS */
/* ───────────────────────────── */
function ActualizarListadoParadas() {

    const listadoContainer = document.getElementById('listadoParadas');

    if (!listadoContainer) {
        console.error("No se encontró el elemento #listadoParadas");
        return;
    }

    // Limpiar contenido anterior
    listadoContainer.innerHTML = '';

    // Si no hay paradas cercanas
    if (!paradasCercanasJson || paradasCercanasJson.length === 0) {
        listadoContainer.innerHTML = '<div class="vacio-mensaje">No hay paradas cercanas</div>';
        return;
    }

    // Crear elemento para cada parada cercana
    paradasCercanasJson.forEach((parada, index) => {

        // Convertir metros a formato legible
        const distanciaFormato = parada.distancia >= 1000 
            ? (parada.distancia / 1000).toFixed(1) + ' km'
            : Math.round(parada.distancia) + ' m';

        // Convertir segundos a minutos
        const tiempoFormato = Math.ceil(parada.tiempo / 60) + ' min';

        // Buscar nombre de la parada en paradasJson
        const paradaInfo = paradasJson.find(p => p.idParada === parada.idParada);
        const nombreParada = paradaInfo?.nombre || 'Parada ' + parada.idParada;
        const direccion = paradaInfo?.direccion || 'Ubicación desconocida';

        const elementoParada = document.createElement('div');
        elementoParada.className = 'elemento-parada-nuevo';
        elementoParada.innerHTML = `
            <div class="etiqueta-distancia-parada" title="${tiempoFormato}">${distanciaFormato}</div>
            <div class="detalles-parada">
                <div class="icono-izquierda-parada"><i class="bi bi-geo-alt-fill"></i></div>
                <div class="texto-parada">
                    <div class="direccion-parada">Parada</div>
                    <div class="rutas-parada">${tiempoFormato} a pie</div>
                </div>
            </div>
            <div class="acciones-parada">
                <button class="circulo-accion" title="Ir a parada" onclick="IrAParada('${parada.idParada}', ${parada.lat}, ${parada.lon})">
                    <i class="bi bi-crosshair"></i>
                </button>
                <button class="circulo-accion" title="Más información" onclick="MostrarDetallesParada('${parada.idParada}')">
                    <i class="bi bi-info-circle"></i>
                </button>
            </div>
        `;

        listadoContainer.appendChild(elementoParada);
    });

    // Actualizar título con cantidad de paradas
    const titulo = document.querySelector('.titulo-panel');
    if (titulo) {
        titulo.textContent = paradasCercanasJson.length + ' Paradas Cercanas';
    }
}

/* ───────────────────────────── */
/* 🎯 IR A PARADA */
/* ───────────────────────────── */
function IrAParada(idParada, lat, lon) {
    console.log("Ir a parada:", idParada, lat, lon);

    // Centrar mapa en la parada
    map.setView([lat, lon], 18);

    // Hacer click en el marcador
    if (markersParadas[idParada]) {
        markersParadas[idParada].openPopup();
    }
}

/* ───────────────────────────── */
/* ℹ️ MOSTRAR DETALLES PARADA */
/* ───────────────────────────── */
function MostrarDetallesParada(idParada) {
    console.log("Detalles de parada:", idParada);

    const parada = paradasCercanasJson.find(p => p.idParada === idParada);
    const paradaInfo = paradasJson.find(p => p.idParada === idParada);

    if (!parada || !paradaInfo) {
        alert('Información de parada no disponible');
        return;
    }

    // Crear modal con información detallada
    const mensaje = `
📍 Parada: ${paradaInfo.nombre || idParada}
📏 Distancia: ${Math.round(parada.distancia)} m
⏱ Tiempo a pie: ${Math.ceil(parada.tiempo / 60)} minutos
📍 Coordenadas: ${parada.lat.toFixed(4)}, ${parada.lon.toFixed(4)}
    `;

    alert(mensaje);
}