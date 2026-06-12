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
























/* 
????????????????????????????????????????????????????????????????????
?        BUSCADOR DE DIRECCIONES CON NOMINATIM API                ?
?        Búsqueda en tiempo real, sugerencias y selección          ?
?        Listo para integrar con Leaflet y GraphHopper             ?
????????????????????????????????????????????????????????????????????
*/

//*?? VARIABLES GLOBALES ??
const inputBuscador = document.getElementById('inputBuscador');
const listaSugerencias = document.getElementById('listaSugerencias');
const statusMensaje = document.getElementById('statusMensaje');
const btnLimpiarBuscador = document.getElementById('btnLimpiarBuscador');
const ubicacionSeleccionada = document.getElementById('ubicacionSeleccionada');

// Variables para guardar la ubicación seleccionada
let ubicacionActual = {
    nombre: '',
    latitud: null,
    longitud: null,
    displayName: ''
};

// Control de debounce para búsqueda
let timeoutBusqueda = null;
const DELAY_BUSQUEDA = 300; // ms
const MIN_CARACTERES = 3;
const API_NOMINATIM = 'https://nominatim.openstreetmap.org/search';

/* ??????????????????????????????????????????????????????????????????? */
/* ?? INICIALIZACIÓN DE EVENTOS                                       */
/* ??????????????????????????????????????????????????????????????????? */

document.addEventListener('DOMContentLoaded', () => {
    // Event listener: escribir en el input
    inputBuscador.addEventListener('input', manejarInputBuscador);

    // Event listener: limpiar búsqueda
    btnLimpiarBuscador.addEventListener('click', limpiarBuscador);

    // Event listener: cerrar sugerencias al hacer click fuera
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.buscador-contenedor')) {
            ocultarSugerencias();
        }
    });

    // Event listener: navegación con teclado (flecha arriba/abajo, enter, esc)
    inputBuscador.addEventListener('keydown', manejarTeclas);
});

/* ??????????????????????????????????????????????????????????????????? */
/* ?? MANEJO DE EVENTOS DEL INPUT                                     */
/* ??????????????????????????????????????????????????????????????????? */

function manejarInputBuscador(e) {
    const valor = e.target.value.trim();

    // Mostrar/ocultar botón limpiar
    if (valor.length > 0) {
        btnLimpiarBuscador.style.display = 'block';
    } else {
        btnLimpiarBuscador.style.display = 'none';
        ocultarSugerencias();
        return;
    }

    // Si hay menos de 3 caracteres, no hacer búsqueda
    if (valor.length < MIN_CARACTERES) {
        ocultarSugerencias();
        return;
    }

    // Cancelar búsqueda anterior si existe
    if (timeoutBusqueda) {
        clearTimeout(timeoutBusqueda);
    }

    // Debounce: esperar a que el usuario deje de escribir
    timeoutBusqueda = setTimeout(() => {
        buscarDirecciones(valor);
    }, DELAY_BUSQUEDA);
}

function manejarTeclas(e) {
    const items = Array.from(document.querySelectorAll('.item-sugerencia'));
    const activo = document.querySelector('.item-sugerencia.activa');
    let indiceActivo = activo ? items.indexOf(activo) : -1;

    switch (e.key) {
        case 'ArrowDown':
            e.preventDefault();
            indiceActivo++;
            if (indiceActivo >= items.length) indiceActivo = 0;
            marcarActivo(items[indiceActivo]);
            break;

        case 'ArrowUp':
            e.preventDefault();
            indiceActivo--;
            if (indiceActivo < 0) indiceActivo = items.length - 1;
            marcarActivo(items[indiceActivo]);
            break;

        case 'Enter':
            e.preventDefault();
            if (activo) {
                activo.click();
            }
            break;

        case 'Escape':
            e.preventDefault();
            ocultarSugerencias();
            inputBuscador.blur();
            break;
    }
}

function marcarActivo(elemento) {
    document.querySelectorAll('.item-sugerencia').forEach(el => el.classList.remove('activa'));
    if (elemento) {
        elemento.classList.add('activa');
        elemento.scrollIntoView({ block: 'nearest' });
    }
}

/* ??????????????????????????????????????????????????????????????????? */
/* ?? BÚSQUEDA CON NOMINATIM API                                      */
/* ??????????????????????????????????????????????????????????????????? */

async function buscarDirecciones(query) {
    try {
        // Mostrar estado: cargando
        mostrarEstado('cargando', '? Buscando...');

        // Construir URL con parámetros
        const url = new URL(API_NOMINATIM);
        url.searchParams.append('q', query);
        url.searchParams.append('format', 'json');
        url.searchParams.append('limit', '5');
        url.searchParams.append('addressdetails', '1');

        console.log('?? Buscando:', query);

        // Hacer request a Nominatim
        const response = await fetch(url.toString());

        if (!response.ok) {
            throw new Error(`Error HTTP: ${response.status}`);
        }

        const datos = await response.json();

        console.log('?? Resultados:', datos.length);

        // Si no hay resultados
        if (!datos || datos.length === 0) {
            mostrarEstado('info', 'Sin resultados. Intenta con otra búsqueda.');
            ocultarSugerencias();
            return;
        }

        // Renderizar sugerencias
        renderizarSugerencias(datos);
        mostrarSugerencias();

    } catch (error) {
        console.error('? Error en búsqueda:', error);
        mostrarEstado('error', 'Error en la búsqueda. Intenta de nuevo.');
        ocultarSugerencias();
    }
}

/* ??????????????????????????????????????????????????????????????????? */
/* ?? RENDERIZAR SUGERENCIAS                                          */
/* ??????????????????????????????????????????????????????????????????? */

function renderizarSugerencias(resultados) {
    // Limpiar lista anterior
    listaSugerencias.innerHTML = '';

    // Crear elemento para cada resultado
    resultados.forEach((resultado, indice) => {
        const li = document.createElement('li');
        li.className = 'item-sugerencia';
        li.textContent = resultado.display_name;

        // Guardar lat/lon como data-attributes
        li.setAttribute('data-lat', resultado.lat);
        li.setAttribute('data-lon', resultado.lon);
        li.setAttribute('data-display-name', resultado.display_name);

        // Event listener: click en sugerencia
        li.addEventListener('click', () => seleccionarSugerencia(li));

        listaSugerencias.appendChild(li);
    });

    console.log('? Sugerencias renderizadas:', resultados.length);
}

/* ??????????????????????????????????????????????????????????????????? */
/* ? SELECCIONAR SUGERENCIA                                          */
/* ??????????????????????????????????????????????????????????????????? */

function seleccionarSugerencia(elemento) {
    // Obtener datos del elemento
    const nombre = elemento.getAttribute('data-display-name');
    const latitud = parseFloat(elemento.getAttribute('data-lat'));
    const longitud = parseFloat(elemento.getAttribute('data-lon'));

    // Guardar en variable global
    ubicacionActual = {
        nombre: nombre,
        latitud: latitud,
        longitud: longitud,
        displayName: nombre
    };

    console.log('?? Ubicación seleccionada:');
    console.log('   Nombre:', nombre);
    console.log('   Latitud:', latitud);
    console.log('   Longitud:', longitud);

    // Llenar input con el nombre
    inputBuscador.value = nombre;

    // Mostrar información de ubicación
    mostrarUbicacionSeleccionada("Direccion", latitud, longitud);

    // Ocultar sugerencias
    ocultarSugerencias();

    // ?? CALCULAR RUTA Y REDIRIGIR A MEJOR RUTA ??
    calcularRutaYRedireccionar(latitud, longitud, nombre);
}

/* ??????????????????????????????????????????????????????????????????? */
/* ??? CALCULAR RUTA Y REDIRIGIR A MEJOR RUTA                         */
/* ??????????????????????????????????????????????????????????????????? */

async function calcularRutaYRedireccionar(latDestino, lonDestino, nombreDestino) {
    try {
        console.log('?? Calculando ruta...');
        console.log(`?? Destino: (${latDestino}, ${lonDestino})`);

        // Obtener ubicación actual del usuario
        if (!navigator.geolocation) {
            console.error('? Geolocalización no disponible');
            alert('Necesitamos acceso a tu ubicación para calcular la ruta');
            return;
        }

        // Mostrar loading
        mostrarEstado('cargando', '? Obteniendo tu ubicación y calculando ruta...');

        navigator.geolocation.getCurrentPosition(async function (position) {
            const latOrigen = position.coords.latitude;
            const lonOrigen = position.coords.longitude;

            console.log(`?? Origen: (${latOrigen}, ${lonOrigen})`);
            console.log(`?? Destino: (${latDestino}, ${lonDestino})`);

            try {
                mostrarEstado('cargando', '?? Calculando ruta...');

                // Construir URL con parámetros
                const url = `/RutasAutobus/ObtenerRuta`;



                console.log(`?? Llamando a API: ${url}`);

                const request = {
                    LatInicio: latOrigen,
                    LonInicio: lonOrigen,
                    LatFin: latDestino,
                    LonFin: lonDestino
                };

                // Llamar a API para calcular ruta completa
                const response = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(request)
                });

                console.log(`?? Respuesta HTTP: ${response.status} ${response.statusText}`);

                if (!response.ok) {
                    const errorData = await response.json();
                    console.error('? Error en respuesta:', errorData);
                    throw new Error(errorData.error || 'Error al calcular ruta');
                }

                const rutaData = await response.json();

                console.log('? Ruta calculada:', rutaData);

                // Guardar ruta en sessionStorage para usarla en MejorRuta
                sessionStorage.setItem('rutaCalculada', JSON.stringify(rutaData));
                sessionStorage.setItem('latOrigen', latOrigen);
                sessionStorage.setItem('lonOrigen', lonOrigen);
                sessionStorage.setItem('latDestino', latDestino);
                sessionStorage.setItem('lonDestino', lonDestino);

                // Ocultar estado antes de redirigir
                ocultarSugerencias();

                // Redirigir a página de mejor ruta
              //  console.log('?? Redirigiendo a Mejor Ruta...');
                //setTimeout(() => {
                  //  window.location.href = '/Usuario/MejorRuta';
                //     }, 500);

                const form = document.createElement("form");
                form.method = "POST";
                form.action = "/Usuario/MejorRuta"; // Controlador/Acción

                const datos = {
                    LatI: latOrigen,
                    LonI: lonOrigen,
                    nombre: nombreDestino,
                    latF: latDestino,
                    lonF: lonDestino
                };

                for (const key in datos) {
                    const input = document.createElement("input");
                    input.type = "hidden";
                    input.name = key;
                    input.value = datos[key];
                    form.appendChild(input);
                }

                localStorage.setItem(
                    "datosRuta",
                    JSON.stringify(datos)
                );

                document.body.appendChild(form);
                form.submit();

            } catch (error) {
                console.error('? Error al calcular ruta:', error);
                mostrarEstado('error', '? Error: ' + error.message);

                // Permitir reintentar
                setTimeout(() => {
                    ocultarSugerencias();
                }, 3000);
            }

        }, function (error) {
            console.error('? Error de geolocalización:', error.message);

            let mensaje = 'No se puede acceder a tu ubicación';
            if (error.code === error.PERMISSION_DENIED) {
                mensaje = 'Debes permitir acceso a tu ubicación';
            } else if (error.code === error.POSITION_UNAVAILABLE) {
                mensaje = 'Tu ubicación no está disponible';
            } else if (error.code === error.TIMEOUT) {
                mensaje = 'Tiempo de espera agotado';
            }

            mostrarEstado('error', '? ' + mensaje);

            setTimeout(() => {
                ocultarSugerencias();
            }, 3000);
        }, {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
        });

    } catch (error) {
        console.error('? Error:', error);
        mostrarEstado('error', '? Error: ' + error.message);
    }
}

/* ??????????????????????????????????????????????????????????????????? */
/* ?? MOSTRAR/OCULTAR ELEMENTOS                                       */
/* ??????????????????????????????????????????????????????????????????? */

function mostrarSugerencias() {
    listaSugerencias.style.display = 'block';
    statusMensaje.style.display = 'none';
    ubicacionSeleccionada.style.display = 'none';
}

function ocultarSugerencias() {
    listaSugerencias.style.display = 'none';
    statusMensaje.style.display = 'none';
}

function mostrarEstado(tipo, mensaje) {
    statusMensaje.textContent = mensaje;
    statusMensaje.className = 'status-mensaje ' + tipo;
    statusMensaje.style.display = 'block';
    listaSugerencias.style.display = 'none';
    ubicacionSeleccionada.style.display = 'none';
}

function mostrarUbicacionSeleccionada(nombre, lat, lon) {
    document.getElementById('ubicacionNombre').textContent = "Direccion";
    document.getElementById('ubicacionLat').textContent = lat.toFixed(6);
    document.getElementById('ubicacionLon').textContent = lon.toFixed(6);

    ubicacionSeleccionada.style.display = 'block';
    listaSugerencias.style.display = 'none';
    statusMensaje.style.display = 'none';
}

/* ??????????????????????????????????????????????????????????????????? */
/* ??? LIMPIAR BÚSQUEDA                                                */
/* ??????????????????????????????????????????????????????????????????? */

function limpiarBuscador() {
    inputBuscador.value = '';
    inputBuscador.focus();
    btnLimpiarBuscador.style.display = 'none';
    ocultarSugerencias();

    // Limpiar ubicación seleccionada
    ubicacionActual = {
        nombre: '',
        latitud: null,
        longitud: null,
        displayName: ''
    };

    console.log('???  Búsqueda limpiada');
}

/* ??????????????????????????????????????????????????????????????????? */
/* ?? FUNCIÓN PARA OBTENER UBICACIÓN ACTUAL                           */
/* ??????????????????????????????????????????????????????????????????? */

function obtenerUbicacionSeleccionada() {
    return ubicacionActual;
}

// Exportar para usar en otros scripts (si es necesario)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        obtenerUbicacionSeleccionada,
        ubicacionActual
    };
}

/* ??????????????????????????????????????????????????????????????????? */
/* ?? INTEGRACIÓN CON GRAPHHOPPER (Comentada para después)           */
/* ??????????????????????????????????????????????????????????????????? */

/*
// Cuando tengas ubicación de origen Y destino, llama a esta función:

async function calcularRutasGraphHopper(latOrigen, lonOrigen, latDestino, lonDestino) {
    try {
        const url = 'https://graphhopper.com/api/1/route';

        const params = {
            point: [
                `${latOrigen},${lonOrigen}`,
                `${latDestino},${lonDestino}`
            ],
            vehicle: 'foot',  // o 'car', 'bike', etc
            locale: 'es',
            key: 'YOUR_GRAPHHOPPER_API_KEY'
        };

        const response = await fetch(url + '?' + new URLSearchParams(params));
        const datos = await response.json();

        console.log('???  Ruta calculada:', datos);
        // Aquí mostrar la ruta en el mapa con Leaflet

    } catch (error) {
        console.error('? Error GraphHopper:', error);
    }
}

// Uso:
// calcularRutasGraphHopper(19.25, -99.04, 19.26, -99.05);
*/

console.log('? Script de buscador cargado y listo');
