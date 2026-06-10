# ?? GUÍA DE TROUBLESHOOTING: Cálculo de Rutas

## ? Error: "Error al calcular la ruta"

### Causas Posibles y Soluciones:

---

## 1. **Error de Conexión a Base de Datos**

### Síntomas:
- Console muestra: `Error al calcular ruta: Error al calcular ruta`
- Output window muestra: `? Error SQL: ...`

### Verificar:
```csharp
// En appsettings.json
"ConnectionStrings": {
    "StringCONSQLocal": "Server=localhost;Database=SSMI;Trusted_Connection=true;"
}
```

### Soluciones:
1. Verificar que SQL Server está corriendo
2. Comprobar nombre de servidor: `localhost` o `(localdb)\MSSQLLocalDB`
3. Comprobar nombre de BD: `SSMI`
4. Ejecutar en SQL Server Management Studio:
   ```sql
   SELECT @@SERVERNAME, DB_NAME();
   ```

---

## 2. **Stored Procedure no existe**

### Síntomas:
- Output muestra: `? Error SQL: Could not find stored procedure...`

### Verificar:
```sql
SELECT * FROM sys.procedures WHERE name LIKE '%Ruta%';
```

### Soluciones:
1. Ejecutar el SP en la BD:
```sql
CREATE PROCEDURE spCalcularRutaAutobusConUnaVarianteInvolucrada
    @LatInicio FLOAT,
    @LonInicio FLOAT,
    @LatFin FLOAT,
    @LonFin FLOAT
AS
BEGIN
    -- Código del SP aquí
    ...
END;
```

2. Comprobar que existen las tablas:
```sql
SELECT * FROM sys.tables WHERE name LIKE '%Parada%' OR name LIKE '%Variante%';
```

---

## 3. **No hay datos en tablas**

### Síntomas:
- SP retorna 0 filas
- Pero no hay error SQL

### Verificar:
```sql
SELECT COUNT(*) FROM tbParadas;
SELECT COUNT(*) FROM tbVarianteParadas;
SELECT COUNT(*) FROM tbRutaVariantes;
```

### Soluciones:
1. Insertar datos de prueba:
```sql
INSERT INTO tbParadas (IdParada, Lat, Lon, Nombre, Direccion, Ubicacion)
VALUES 
    ('PAR-001', 19.4326, -99.1332, 'Parada 1', 'Calle A', geography::Point(19.4326, -99.1332, 4326)),
    ('PAR-002', 19.4345, -99.1350, 'Parada 2', 'Calle B', geography::Point(19.4345, -99.1350, 4326));
```

---

## 4. **Geolocalización no funciona**

### Síntomas:
- Alert: "Necesitamos acceso a tu ubicación"
- Console: `? Error de geolocalización: permission denied`

### Soluciones:
1. **En navegador**: Permitir acceso a ubicación
   - Chrome: Click en ?? ? "Permitir acceso"
   - Firefox: Permitir siempre

2. **En localhost**: Debe ser HTTPS o localhost
   - ? `http://localhost:5000` ? Funciona
   - ? `https://yourdomain.com` ? Funciona
   - ? `http://192.168.1.100` ? No funciona (sin HTTPS)

3. **En HTTPS**: El certificado debe ser válido

### Testing:
```javascript
// En consola del navegador
navigator.geolocation.getCurrentPosition(
    pos => console.log('? Ubicación:', pos.coords),
    err => console.error('? Error:', err)
);
```

---

## 5. **sessionStorage vacío**

### Síntomas:
- Redirige a MejorRuta pero no carga ruta
- Console: `?? No hay ruta calculada en sessionStorage`

### Verificar:
```javascript
// En consola
console.log(sessionStorage.getItem('rutaCalculada'));
console.log(sessionStorage.getItem('latOrigen'));
```

### Soluciones:
1. Verificar que API retorna datos:
   ```
   GET /api/rutas/calcular-completo?latOrigen=19.43&lonOrigen=-99.13&latDestino=19.44&lonDestino=-99.15
   ```

2. Verificar response en Network tab de DevTools

3. Usar endpoint demo para prueba:
   ```
   GET /api/rutas/demo
   ```

---

## 6. **Mapa no se renderiza**

### Síntomas:
- Página carga pero el mapa es gris o en blanco
- Console: `? Leaflet no está cargado`

### Verificar en MejorRuta.cshtml:
```html
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
```

### Soluciones:
1. Limpiar cache del navegador (Ctrl+Shift+Delete)
2. Verificar conexión a CDN unpkg
3. Usar versión local de Leaflet

---

## 7. **Ruta no se dibuja**

### Síntomas:
- Mapa carga, pero no hay polilinea
- Console: `?? No hay puntos válidos para dibujar`

### Verificar:
```javascript
// En consola de MejorRuta
console.log(window.rutaActual);
```

### Soluciones:
1. Verificar que `ruta.Instrucciones` tiene datos
2. Verificar que `PosicionLat` y `PosicionLon` son números válidos
3. Intentar con datos de demo:
   ```javascript
   cargarDatosDePrueba();
   ```

---

## 8. **Error de CORS**

### Síntomas:
- Console: `Cross-Origin Request Blocked`
- Network tab muestra request bloqueado

### Soluciones:
1. Verificar que estás en el mismo dominio
2. En Program.cs, agregar CORS si es necesario:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

---

## ?? TESTING RÁPIDO

### 1. Probar endpoint de demo:
```bash
curl "http://localhost:5000/api/rutas/demo"
```

### 2. Probar con coordenadas específicas:
```bash
curl "http://localhost:5000/api/rutas/calcular-completo?latOrigen=19.43&lonOrigen=-99.13&latDestino=19.44&lonDestino=-99.15"
```

### 3. Probar desde navegador:
```javascript
// Consola del navegador
fetch('/api/rutas/demo')
    .then(r => r.json())
    .then(data => console.log(data))
    .catch(e => console.error(e));
```

---

## ?? INFORMACIÓN DE DEBUG

### Output window muestra:
```
?? Calculando ruta completa...
   Origen: (19.4326, -99.1332)
   Destino: (19.4440, -99.1550)
? Cadena de conexión OK
?? Iniciando consulta de ruta autobús
   Inicio: (19.4326, -99.1332)
   Fin: (19.4440, -99.1550)
? Conexión abierta
? Parámetros configurados, ejecutando SP...
? SP ejecutado: 4 filas leídas
? Ruta consultada exitosamente: 4 instrucciones
? Ruta calculada: 4 instrucciones, 2950m
```

### Console del navegador muestra:
```
?? Calculando ruta...
?? Destino: (19.4440, -99.1550)
? Obteniendo tu ubicación y calculando ruta...
?? Origen: (19.4326, -99.1332)
?? Destino: (19.4440, -99.1550)
?? Calculando ruta...
?? Llamando a API: /api/rutas/calcular-completo?...
?? Respuesta HTTP: 200 OK
? Ruta calculada: {...}
?? Redirigiendo a Mejor Ruta...
? Ruta cargada desde sessionStorage
?? Instrucciones: 4
?? Distancia total: 2950m
? Tiempo total: 9.8min
?? Dibujando ruta en mapa...
? Ruta dibujada en el mapa
```

---

## ?? TIPS

1. **Abre DevTools** (F12) antes de hacer pruebas
2. **Tab Console** para ver logs
3. **Tab Network** para ver requests HTTP
4. **Tab Application** ? sessionStorage para ver datos guardados
5. **Usa `console.clear()`** para limpiar logs anteriores

---

## ?? SOPORTE

Si el problema persiste:
1. Copiar error de Output window
2. Copiar error de console del navegador
3. Verificar Network tab ? Response del API
4. Compartir logs en Issue de GitHub
