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

/* ═══════════════════════════════════════════════════════════════════ */
/* 📍 CARGAR RUTA CALCULADA DESDE SESSIONSTORAGE                     */
/* ═══════════════════════════════════════════════════════════════════ */

function cargarRutaCalculada() {
    try {
        console.log('📋 Cargando ruta desde sessionStorage...');

        // Obtener datos de sessionStorage
        const rutaJSON = sessionStorage.getItem('rutaCalculada');
        const latOrigen = parseFloat(sessionStorage.getItem('latOrigen'));
        const lonOrigen = parseFloat(sessionStorage.getItem('lonOrigen'));
        const latDestino = parseFloat(sessionStorage.getItem('latDestino'));
        const lonDestino = parseFloat(sessionStorage.getItem('lonDestino'));

        if (!rutaJSON) {
            console.warn('⚠️ No hay ruta calculada en sessionStorage');
            console.log('📋 Cargando datos de demo...');
            cargarDatosDePrueba();
            return;
        }

        try {
            const ruta = JSON.parse(rutaJSON);
            window.rutaActual = ruta;

            console.log('✅ Ruta cargada desde sessionStorage');
            console.log(`📍 Instrucciones: ${ruta.Instrucciones.length}`);
            console.log(`📏 Distancia total: ${ruta.DistanciaTotal}m`);
            console.log(`⏱ Tiempo total: ${ruta.TiempoTotalMinutos.toFixed(1)}min`);

            // Validar que tenemos coordenadas
            if (isNaN(latOrigen) || isNaN(lonOrigen) || isNaN(latDestino) || isNaN(lonDestino)) {
                console.warn('⚠️ Coordenadas inválidas, usando valores por defecto');
                cargarDatosDePrueba();
                return;
            }

            // Dibujar ruta en el mapa
            dibujarRutaEnMapa(ruta, latOrigen, lonOrigen, latDestino, lonDestino);

            // Mostrar resumen
            mostrarResumenRuta(ruta);

        } catch (parseError) {
            console.error('❌ Error al parsear ruta:', parseError.message);
            cargarDatosDePrueba();
        }

    } catch (error) {
        console.error('❌ Error al cargar ruta:', error);
        cargarDatosDePrueba();
    }
}

/* ═══════════════════════════════════════════════════════════════════ */
/* 🎨 DIBUJAR RUTA EN EL MAPA                                         */
/* ═══════════════════════════════════════════════════════════════════ */

function dibujarRutaEnMapa(ruta, latOrigen, lonOrigen, latDestino, lonDestino) {
    try {
        if (!window.map) {
            console.warn('⚠️ Mapa no inicializado');
            return;
        }

        if (!ruta || !ruta.Instrucciones || ruta.Instrucciones.length === 0) {
            console.warn('⚠️ Ruta sin instrucciones válidas');
            return;
        }

        console.log('🎨 Dibujando ruta en mapa...');

        // Limpiar marcadores y rutas anteriores
        limpiarMapa();

        // Validar coordenadas
        console.log(`📍 Verificando coordenadas...`);
        console.log(`   Origen: (${latOrigen}, ${lonOrigen})`);
        console.log(`   Destino: (${latDestino}, ${lonDestino})`);

        // Crear array de puntos para la polilinea
        const puntos = ruta.Instrucciones
            .filter(instr => !isNaN(parseFloat(instr.PosicionLat)) && !isNaN(parseFloat(instr.PosicionLon)))
            .map(instr => [
                parseFloat(instr.PosicionLat),
                parseFloat(instr.PosicionLon)
            ]);

        if (puntos.length === 0) {
            console.warn('⚠️ No hay puntos válidos para dibujar');
            return;
        }

        console.log(`✅ ${puntos.length} puntos válidos para dibujar`);

        // Agregar marcador de origen
        agregarMarcador(latOrigen, lonOrigen, '📍 Inicio', 'origen');

        // Agregar marcador de destino
        agregarMarcador(latDestino, lonDestino, '🎯 Destino', 'destino');

        // Agregar marcadores para paradas de autobús
        const paradasSubida = ruta.Instrucciones.filter(i => i.Tipo === 'AUTOBUS' && i.Estado === 'SUBIR');
        const paradasBajada = ruta.Instrucciones.filter(i => i.Tipo === 'AUTOBUS' && i.Estado === 'BAJAR');

        console.log(`📍 Paradas: ${paradasSubida.length} subida, ${paradasBajada.length} bajada`);

        paradasSubida.forEach(instr => {
            agregarMarcador(
                parseFloat(instr.PosicionLat),
                parseFloat(instr.PosicionLon),
                `🚌 Sube (${instr.IdParada})`,
                'parada_subida'
            );
        });

        paradasBajada.forEach(instr => {
            agregarMarcador(
                parseFloat(instr.PosicionLat),
                parseFloat(instr.PosicionLon),
                `🚌 Baja (${instr.IdParada})`,
                'parada_bajada'
            );
        });

        // Dibujar diferentes colores según tipo de instrucción
        let puntosSeccion = [];
        let colorActual = '#4287f5'; // Azul para caminata
        let tipoActual = null;

        ruta.Instrucciones.forEach((instr, index) => {
            if (!instr.PosicionLat || !instr.PosicionLon) return;

            const punto = [parseFloat(instr.PosicionLat), parseFloat(instr.PosicionLon)];

            // Cambiar color si cambia el tipo
            if (tipoActual && tipoActual !== instr.Tipo) {
                // Dibujar sección anterior
                if (puntosSeccion.length > 1) {
                    console.log(`📍 Dibujando ${tipoActual}: ${puntosSeccion.length} puntos con color ${colorActual}`);
                    dibujarRuta(puntosSeccion, colorActual, 4);
                }
                puntosSeccion = [punto];
                tipoActual = instr.Tipo;
                colorActual = instr.Tipo === 'AUTOBUS' ? '#ef5350' : '#4287f5';
            } else {
                if (!tipoActual) {
                    tipoActual = instr.Tipo;
                    colorActual = instr.Tipo === 'AUTOBUS' ? '#ef5350' : '#4287f5';
                }
                puntosSeccion.push(punto);
            }

            // Última instrucción
            if (index === ruta.Instrucciones.length - 1 && puntosSeccion.length > 1) {
                console.log(`📍 Dibujando ${tipoActual}: ${puntosSeccion.length} puntos con color ${colorActual}`);
                dibujarRuta(puntosSeccion, colorActual, 4);
            }
        });

        console.log('✅ Ruta dibujada en el mapa');

    } catch (error) {
        console.error('❌ Error al dibujar ruta:', error);
    }
}

/* ═══════════════════════════════════════════════════════════════════ */
/* 📊 MOSTRAR RESUMEN DE LA RUTA                                      */
/* ═══════════════════════════════════════════════════════════════════ */

function mostrarResumenRuta(ruta) {
    try {
        const panelInfo = document.getElementById('panelInfo');
        const detallesRuta = document.getElementById('detallesRuta');

        if (!panelInfo || !detallesRuta) {
            console.warn('⚠️ Panel de información no encontrado');
            return;
        }

        let html = `
            <div class="ruta-item">
                <strong>📏 Distancia total:</strong> ${(ruta.DistanciaTotal / 1000).toFixed(2)} km
            </div>
            <div class="ruta-item">
                <strong>⏱ Tiempo total:</strong> ${ruta.TiempoTotalMinutos.toFixed(1)} minutos
            </div>
            <div class="ruta-item">
                <strong>📊 Resumen:</strong> ${ruta.ResumenRuta}
            </div>
            <div class="ruta-item">
                <strong>🚶 Caminata:</strong> ${ruta.Instrucciones.filter(i => i.Tipo === 'CAMINAR').length} segmentos
            </div>
            <div class="ruta-item">
                <strong>🚌 Autobús:</strong> ${ruta.Instrucciones.filter(i => i.Tipo === 'AUTOBUS').length} paradas
            </div>
        `;

        detallesRuta.innerHTML = html;
        panelInfo.style.display = 'block';

        console.log('✅ Resumen mostrado');

    } catch (error) {
        console.error('❌ Error al mostrar resumen:', error);
    }
}

/* ═══════════════════════════════════════════════════════════════════ */
/* 📍 AGREGAR MARCADOR AL MAPA                                        */
/* ═══════════════════════════════════════════════════════════════════ */

function agregarMarcador(lat, lon, titulo, tipo = 'parada') {
    try {
        if (!window.map) {
            console.warn('⚠️ Mapa no inicializado');
            return;
        }

        const iconoUrl = tipo === 'origen' 
            ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png'
            : tipo === 'destino'
            ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png'
            : tipo === 'parada_subida'
            ? 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-blue.png'
            : 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png';

        const marcador = L.marker([lat, lon], {
            title: titulo,
            icon: L.icon({
                iconUrl: iconoUrl,
                shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
                iconSize: [25, 41],
                shadowSize: [41, 41],
                iconAnchor: [12, 41],
                shadowAnchor: [12, 41],
                popupAnchor: [1, -34]
            })
        }).addTo(window.map).bindPopup(titulo);

        const id = tipo + '_' + Date.now();
        window.marcadores[id] = marcador;

        return id;

    } catch (error) {
        console.error('❌ Error al agregar marcador:', error);
    }
}

/* ═══════════════════════════════════════════════════════════════════ */
/* 🛣️ DIBUJAR RUTA (POLILINEA)                                        */
/* ═══════════════════════════════════════════════════════════════════ */

function dibujarRuta(puntos, color = '#667eea', weight = 4) {
    try {
        if (!window.map) {
            console.warn('⚠️ Mapa no inicializado');
            return;
        }

        // Crear polilinea con los puntos
        const polilinea = L.polyline(puntos, {
            color: color,
            weight: weight,
            opacity: 0.8,
            lineCap: 'round',
            lineJoin: 'round'
        }).addTo(window.map);

        if (!window.rutaPolilinea) {
            window.rutaPolilinea = polilinea;
            // Ajustar vista del mapa a la ruta
            const bounds = polilinea.getBounds();
            window.map.fitBounds(bounds, { padding: [50, 50] });
        }

    } catch (error) {
        console.error('❌ Error al dibujar ruta:', error);
    }
}

/* ═══════════════════════════════════════════════════════════════════ */
/* 🧹 LIMPIAR MARCADORES Y RUTAS                                      */
/* ═══════════════════════════════════════════════════════════════════ */

function limpiarMapa() {
    try {
        // Remover todos los marcadores
        Object.values(window.marcadores).forEach(marcador => {
            if (window.map) {
                window.map.removeLayer(marcador);
            }
        });
        window.marcadores = {};

        // Remover ruta
        if (window.rutaPolilinea && window.map) {
            window.map.removeLayer(window.rutaPolilinea);
            window.rutaPolilinea = null;
        }

        console.log('🧹 Mapa limpiado');

    } catch (error) {
        console.error('❌ Error al limpiar mapa:', error);
    }
}

/* ═══════════════════════════════════════════════════════════════════ */
/* 📋 CARGAR DATOS DE PRUEBA (DEMO)                                   */
/* ═══════════════════════════════════════════════════════════════════ */

function cargarDatosDePrueba() {
    try {
        console.log('📋 Cargando datos de prueba...');
        fetch('/api/rutas/demo')
            .then(r => r.json())
            .then(ruta => {
                window.rutaActual = ruta;
                // Usar coordenadas de prueba
                dibujarRutaEnMapa(ruta, 19.4326, -99.1332, 19.4440, -99.1550);
            })
            .catch(e => console.error('Error cargando demo:', e));
    } catch (error) {
        console.error('❌ Error:', error);
    }
}


/* ═══════════════════════════════════════════════════════════════════ */
/* 📊 MOSTRAR INFORMACIÓN DE LA RUTA                                  */
/* ═══════════════════════════════════════════════════════════════════ */

function mostrarDetallesRuta(distancia, tiempo, paradas) {
    try {
        const panelInfo = document.getElementById('panelInfo');
        const detallesRuta = document.getElementById('detallesRuta');

        if (!panelInfo || !detallesRuta) {
            console.warn('⚠️ Panel de información no encontrado');
            return;
        }

        let html = `
            <div class="ruta-item">
                <strong>📏 Distancia:</strong> ${(distancia / 1000).toFixed(2)} km
            </div>
            <div class="ruta-item">
                <strong>⏱ Tiempo:</strong> ${Math.ceil(tiempo / 60)} minutos
            </div>
        `;

        if (paradas && paradas.length > 0) {
            html += '<div class="ruta-item"><strong>🚌 Paradas:</strong><br>';
            paradas.forEach((parada, index) => {
                html += `<small>${index + 1}. ${parada}</small><br>`;
            });
            html += '</div>';
        }

        detallesRuta.innerHTML = html;
        panelInfo.style.display = 'block';

        console.log('✅ Detalles de ruta mostrados');

    } catch (error) {
        console.error('❌ Error al mostrar detalles:', error);
    }
}

/* ═══════════════════════════════════════════════════════════════════ */
/* 🔗 INTEGRACIÓN CON GRAPHHOPPER (Futuro)                           */
/* ═══════════════════════════════════════════════════════════════════ */

/*
async function calcularRutaGraphHopper(latOrigen, lonOrigen, latDestino, lonDestino) {
    try {
        const url = 'https://graphhopper.com/api/1/route';

        const params = {
            point: [
                `${latOrigen},${lonOrigen}`,
                `${latDestino},${lonDestino}`
            ],
            vehicle: 'foot',
            locale: 'es',
            key: 'YOUR_GRAPHHOPPER_API_KEY'
        };

        console.log('🚀 Calculando ruta con GraphHopper...');

        const response = await fetch(url + '?' + new URLSearchParams(params));
        const datos = await response.json();

        if (datos.paths && datos.paths[0]) {
            const ruta = datos.paths[0];

            // Decodificar polyline
            const puntos = decodificarPolyline(ruta.points);

            // Limpiar mapa
            limpiarMapa();

            // Agregar marcadores
            agregarMarcador(latOrigen, lonOrigen, 'Origen', 'origen');
            agregarMarcador(latDestino, lonDestino, 'Destino', 'destino');

            // Dibujar ruta
            dibujarRuta(puntos, '#667eea', 5);

            // Mostrar detalles
            mostrarDetallesRuta(ruta.distance, ruta.time / 1000);

            console.log('✅ Ruta calculada exitosamente');

            return ruta;
        }

    } catch (error) {
        console.error('❌ Error GraphHopper:', error);
    }
}

// Función auxiliar para decodificar polyline (formato Google)
function decodificarPolyline(encoded) {
    const precision = 5;
    const factor = Math.pow(10, precision);
    let index = 0, lat = 0, lng = 0;
    const coordinates = [];

    while (index < encoded.length) {
        let result = 0, shift = 0, b;
        do {
            b = encoded.charCodeAt(index++) - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);
        lat += (result & 1) ? ~(result >> 1) : (result >> 1);

        result = 0;
        shift = 0;
        do {
            b = encoded.charCodeAt(index++) - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);
        lng += (result & 1) ? ~(result >> 1) : (result >> 1);

        coordinates.push([lat / factor, lng / factor]);
    }
    return coordinates;
}

// Uso:
// calcularRutaGraphHopper(19.25, -99.04, 19.26, -99.05);
*/

console.log('✅ Script MejorRuta.js cargado y listo');