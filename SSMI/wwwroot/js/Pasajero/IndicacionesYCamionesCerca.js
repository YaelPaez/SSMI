var marcadorUsuario = null;
var posicionUsuario = [0, 0];

let camiones = [
    {
        ruta: "Ruta Z03A",
        unidad: "035",
        distancia: 550,
        tiempo: 4,
        ocupados: 25,
        pcd: 2
    },
    {
        ruta: "Ruta Z03A",
        unidad: "024",
        distancia: 800,
        tiempo: 7,
        ocupados: 50,
        pcd: 1
    },
    {
        ruta: "Ruta Z03A",
        unidad: "011",
        distancia: 1100,
        tiempo: 9,
        ocupados: 75,
        pcd: 0
    },
    {
        ruta: "Ruta Z03A",
        unidad: "067",
        distancia: 1500,
        tiempo: 10,
        ocupados: 20,
        pcd: 3
    }
];

// Botón centrar
const btnCentrar = document.getElementById('btnCentrar');

if (btnCentrar) {
    btnCentrar.addEventListener('click', function () {

        if (navigator.geolocation) {

            navigator.geolocation.getCurrentPosition(function (position) {

                posicionUsuario[0] = position.coords.latitude;
                posicionUsuario[1] = position.coords.longitude;

                if (window.map) {
                    window.map.setView(posicionUsuario, 19);
                    console.log('Mapa centrado en:', posicionUsuario);
                }

                CrearMarcadorUsuario();
            });
        }
    });
}

var usuarioUbiIcon = L.icon({
    iconUrl: '/Imagenes/focus.png',
    iconSize: [30, 30],
    iconAnchor: [15, 30]
});

function CrearMarcadorUsuario() {

    if (!marcadorUsuario) {

        marcadorUsuario = L.marker(
            posicionUsuario,
            {
                icon: usuarioUbiIcon
            }
        ).addTo(map);

    }
    else {

        marcadorUsuario.setLatLng(posicionUsuario);

    }

    ModificarContenedor();

    // Iniciar simulación solo una vez
    if (!window.simulacionIniciada) {

        window.simulacionIniciada = true;

        setInterval(() => {

            SimularCamiones();

        }, 4000);

    }
}

function SimularCamiones() {

    for (let i = camiones.length - 1; i >= 0; i--) {

        let c = camiones[i];

        // Avance aleatorio
        c.distancia -= Math.floor(Math.random() * 50) + 30;

        // Recalcular tiempo estimado
        c.tiempo = Math.max(
            1,
            Math.ceil(c.distancia / 150)
        );

        // Variación ligera de ocupación
        c.ocupados += Math.floor(Math.random() * 7) - 3;

        if (c.ocupados < 5)
            c.ocupados = 5;

        if (c.ocupados > 100)
            c.ocupados = 100;

        // Cuando llega se reemplaza por otra unidad
        if (c.distancia <= 0) {

            camiones.splice(i, 1);

            camiones.push({
                ruta: "Ruta Z03A",
                unidad: String(
                    Math.floor(Math.random() * 90) + 10
                ),
                distancia: 1800,
                tiempo: 12,
                ocupados: Math.floor(Math.random() * 80) + 10,
                pcd: Math.floor(Math.random() * 4)
            });
        }
    }

    ModificarContenedor();
}

function ModificarContenedor() {

    const contenedor =
        document.getElementById("card-instrucciones");

    const span =
        document.getElementById("span-RR-CC");

    const icono =
        document.getElementById("icono-card");

    const btnRuta =
        document.getElementById("btnCentrar");

    btnRuta.innerText = "Salir";

    btnRuta.onclick = function () {

        window.location.href =
            "/Usuario/Index";

    };

    span.innerText = "Camiones Cercanos";
    icono.className = "bi bi-bus-front";

    contenedor.innerHTML = "";

    camiones.sort((a, b) => a.distancia - b.distancia);

    camiones.forEach(c => {

        let colorOcupacion = "#10b981";

        if (c.ocupados >= 60)
            colorOcupacion = "#f59e0b";

        if (c.ocupados >= 85)
            colorOcupacion = "#ef4444";

        contenedor.innerHTML += `

        <div class="bus-card">

            <div class="bus-header">

                <div>
                    <div class="bus-route">
                        ${c.ruta}
                    </div>

                    <div class="bus-unit">
                        🚍 Unidad ${c.unidad}
                    </div>
                </div>

                <div class="eta-badge">
                    ${c.tiempo} min
                </div>

            </div>

            <div class="bus-info">

                <div class="info-row">
                    <span>📍 Distancia</span>
                    <strong>${c.distancia} m</strong>
                </div>

                <div class="info-row">
                    <span>♿ PCD</span>
                    <strong>${c.pcd} disponibles</strong>
                </div>

            </div>

            <div class="occupancy">

                <div class="occupancy-label">
                    <span>Ocupación</span>
                    <span>${c.ocupados}%</span>
                </div>

                <div class="progress">
                    <div
                        class="progress-fill"
                        style="
                            width:${c.ocupados}%;
                            background:${colorOcupacion};
                        ">
                    </div>
                </div>

            </div>

        </div>

        `;
    });
}