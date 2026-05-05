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
/* 🗺️ MAPA */
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

        const response = await fetch('https://prone-unsmooth-prune.ngrok-free.dev/ParadasAPI/ObtenerTodasLasParadas');
        const data = await response.json();

        paradasJson = data;

        PintarParadas();

    } catch (error) {
        console.error("Error paradas:", error);
    }
}

/* ───────────────────────────── */
/* 📍 PARADAS CERCANAS */
/* ───────────────────────────── */
async function obtenerParadasCercanas() {

    try {

        const response = await fetch(
            'https://prone-unsmooth-prune.ngrok-free.dev/ParadasAPI/ObtenerParadasCercanas',
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

        const data = await response.json();

        paradasCercanasJson = data;

        console.log("Paradas cercanas:", data);

        MarcarParadasCercanas();

        // 🔥 AQUÍ ya tienes datos → ahora sí OSRM
        await SacarDistanciasYTiemposDeParadasCercanas();

    } catch (error) {
        console.error("Error cercanas:", error);
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
/* 📍 MARCAR CERCANAS */
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

        const url = `https://router.project-osrm.org/route/v1/foot/${posicionUsuario[1]},${posicionUsuario[0]};${cerca.lon},${cerca.lat}?overview=false`;

        try {

            const response = await fetch(url);
            const data = await response.json();

            if (!data.routes || data.routes.length === 0) {
                console.warn("Sin ruta para:", cerca.idParada);
                continue;
            }

            const ruta = data.routes[0];

            // 🔥 GUARDAR EN EL OBJETO (CLAVE)
            cerca.distancia = ruta.distance;
            cerca.tiempo = ruta.duration;

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
}