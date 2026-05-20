using SSMI.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSMI.Services;

public sealed class RuteoService
{
    private const double MaxCaminataMetros = 300d;
    private readonly SsmiRuteoDbContext _db;

    public RuteoService(SsmiRuteoDbContext db)
    {
        _db = db;
    }

    public async Task<RutaResultadoDto?> CalcularRutaAsync(
        double latOrigen,
        double lonOrigen,
        double latDestino,
        double lonDestino)
    {
        // 1) Buscar paradas cercanas a origen y destino (<= 300m)
        var paradasOrigen = await GetParadasCercanasAsync(latOrigen, lonOrigen, MaxCaminataMetros);
        if (paradasOrigen.Count == 0)
        {
            return null;
        }

        var paradasDestino = await GetParadasCercanasAsync(latDestino, lonDestino, MaxCaminataMetros);
        if (paradasDestino.Count == 0)
        {
            return null;
        }

        var origenPorId = paradasOrigen
            .GroupBy(p => p.IdParada)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DistanciaMetros).First());

        var destinoPorId = paradasDestino
            .GroupBy(p => p.IdParada)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DistanciaMetros).First());

        var idsOrigen = origenPorId.Keys.ToArray();
        var idsDestino = destinoPorId.Keys.ToArray();

        // 2) Intentar ruta DIRECTA
        var directa = await BuscarRutaDirectaAsync(idsOrigen, idsDestino, origenPorId, destinoPorId);
        if (directa is not null)
        {
            return directa;
        }

        // 3) Intentar ruta con TRANSBORDO
        return await BuscarRutaTransbordoAsync(idsOrigen, idsDestino, origenPorId, destinoPorId);
    }

    private async Task<RutaResultadoDto?> BuscarRutaDirectaAsync(
        IReadOnlyCollection<string> idsOrigen,
        IReadOnlyCollection<string> idsDestino,
        IReadOnlyDictionary<string, ParadaCercanaDto> origenPorId,
        IReadOnlyDictionary<string, ParadaCercanaDto> destinoPorId)
    {
        var directos = await (
            from o in _db.RutaVarianteParadas.AsNoTracking()
            join d in _db.RutaVarianteParadas.AsNoTracking()
                on o.IdRutaVariante equals d.IdRutaVariante
            join v in _db.RutaVariantes.AsNoTracking()
                on o.IdRutaVariante equals v.IdRutaVariante
            where idsOrigen.Contains(o.IdParada)
                && idsDestino.Contains(d.IdParada)
                && d.Orden > o.Orden
            select new DirectoRowDto
            {
                IdRutaVariante = v.IdRutaVariante,
                NombreVariante = v.Nombre,
                Sentido = v.Sentido,
                IdParadaAscenso = o.IdParada,
                OrdenAscenso = o.Orden,
                IdParadaDescenso = d.IdParada,
                OrdenDescenso = d.Orden
            }).ToListAsync();

        var mejor = directos
            .Select(r => new
            {
                Row = r,
                DistanciaCaminando = origenPorId[r.IdParadaAscenso].DistanciaMetros + destinoPorId[r.IdParadaDescenso].DistanciaMetros
            })
            .OrderBy(x => x.DistanciaCaminando)
            .ThenBy(x => x.Row.OrdenAscenso)
            .ThenBy(x => x.Row.OrdenDescenso)
            .FirstOrDefault();

        if (mejor is null)
        {
            return null;
        }

        var paradaAscenso = origenPorId[mejor.Row.IdParadaAscenso];
        var paradaDescenso = destinoPorId[mejor.Row.IdParadaDescenso];

        return new RutaResultadoDto
        {
            TipoRuta = TipoRuta.Directa,
            Variante1 = new VarianteDto(mejor.Row.IdRutaVariante, mejor.Row.NombreVariante, mejor.Row.Sentido),
            Variante2 = null,
            ParadaAscenso = new ParadaDto(paradaAscenso.IdParada, paradaAscenso.Nombre, paradaAscenso.Latitud, paradaAscenso.Longitud),
            ParadaDescenso = new ParadaDto(paradaDescenso.IdParada, paradaDescenso.Nombre, paradaDescenso.Latitud, paradaDescenso.Longitud),
            ParadaTransbordoOrigen = null,
            ParadaTransbordoDestino = null,
            DistanciaCaminando = mejor.DistanciaCaminando
        };
    }

    private async Task<RutaResultadoDto?> BuscarRutaTransbordoAsync(
        IReadOnlyCollection<string> idsOrigen,
        IReadOnlyCollection<string> idsDestino,
        IReadOnlyDictionary<string, ParadaCercanaDto> origenPorId,
        IReadOnlyDictionary<string, ParadaCercanaDto> destinoPorId)
    {
        // Variantes asociadas a paradas cercanas
        var origenVarianteParadas = await _db.RutaVarianteParadas.AsNoTracking()
            .Where(x => idsOrigen.Contains(x.IdParada))
            .Select(x => new VarianteParadaOrdenDto(x.IdRutaVariante, x.IdParada, x.Orden))
            .ToListAsync();

        var destinoVarianteParadas = await _db.RutaVarianteParadas.AsNoTracking()
            .Where(x => idsDestino.Contains(x.IdParada))
            .Select(x => new VarianteParadaOrdenDto(x.IdRutaVariante, x.IdParada, x.Orden))
            .ToListAsync();

        if (origenVarianteParadas.Count == 0 || destinoVarianteParadas.Count == 0)
        {
            return null;
        }

        // Mejor parada de ascenso por variante (min caminata desde origen)
        var mejorAscensoPorVariante = origenVarianteParadas
            .Select(x => new
            {
                x.IdRutaVariante,
                x.IdParada,
                x.Orden,
                Dist = origenPorId[x.IdParada].DistanciaMetros
            })
            .GroupBy(x => x.IdRutaVariante)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Dist).ThenBy(x => x.Orden).First());

        // Mejor parada de descenso por variante (min caminata hacia destino)
        var mejorDescensoPorVariante = destinoVarianteParadas
            .Select(x => new
            {
                x.IdRutaVariante,
                x.IdParada,
                x.Orden,
                Dist = destinoPorId[x.IdParada].DistanciaMetros
            })
            .GroupBy(x => x.IdRutaVariante)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Dist).ThenByDescending(x => x.Orden).First());

        var variantesOrigen = mejorAscensoPorVariante.Keys.ToArray();
        var variantesDestino = mejorDescensoPorVariante.Keys.ToArray();
        var variantesUnion = variantesOrigen.Concat(variantesDestino).Distinct().ToArray();

        // Cargar info de variantes
        var variantesInfo = await _db.RutaVariantes.AsNoTracking()
            .Where(v => variantesUnion.Contains(v.IdRutaVariante))
            .Select(v => new VarianteDto(v.IdRutaVariante, v.Nombre, v.Sentido))
            .ToDictionaryAsync(v => v.IdRutaVariante);

        // Cargar paradas de esas variantes (con coordenadas)
        var paradasDeVariantes = await (
            from vp in _db.RutaVarianteParadas.AsNoTracking()
            join p in _db.Paradas.AsNoTracking()
                on vp.IdParada equals p.IdParada
            where variantesUnion.Contains(vp.IdRutaVariante)
            select new VarianteParadaDetalleDto(
                vp.IdRutaVariante,
                vp.IdParada,
                vp.Orden,
                p.Nombre,
                p.Latitud,
                p.Longitud)).ToListAsync();

        var paradasPorVariante = paradasDeVariantes
            .GroupBy(x => x.IdRutaVariante)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<VarianteParadaDetalleDto>)g.OrderBy(x => x.Orden).ToList());

        TransferBestDto? mejor = null;

        // Evaluación de combinaciones VarianteOrigen -> VarianteDestino
        for (var i = 0; i < variantesOrigen.Length; i++)
        {
            var idVarianteOrigen = variantesOrigen[i];
            if (!paradasPorVariante.TryGetValue(idVarianteOrigen, out var stopsOrigenTodas))
            {
                continue;
            }

            var ascenso = mejorAscensoPorVariante[idVarianteOrigen];
            var stopsOrigen = stopsOrigenTodas.Where(s => s.Orden >= ascenso.Orden).ToArray();
            if (stopsOrigen.Length == 0)
            {
                continue;
            }

            for (var j = 0; j < variantesDestino.Length; j++)
            {
                var idVarianteDestino = variantesDestino[j];
                if (!paradasPorVariante.TryGetValue(idVarianteDestino, out var stopsDestinoTodas))
                {
                    continue;
                }

                var descenso = mejorDescensoPorVariante[idVarianteDestino];
                var stopsDestino = stopsDestinoTodas.Where(s => s.Orden <= descenso.Orden).ToArray();
                if (stopsDestino.Length == 0)
                {
                    continue;
                }

                // Buscar el mejor par de transbordo (caminando <= 300m) respetando órdenes
                var mejorTransbordo = FindBestTransferPair(stopsOrigen, stopsDestino, MaxCaminataMetros);
                if (mejorTransbordo is null)
                {
                    continue;
                }

                // Validar que en variante destino se avance del transbordo al descenso
                if (mejorTransbordo.ParadaDestino.Orden >= descenso.Orden)
                {
                    continue;
                }

                var distTotal =
                    ascenso.Dist +
                    mejorTransbordo.DistanciaMetros +
                    descenso.Dist;

                var candidato = new TransferBestDto(
                    idVarianteOrigen,
                    idVarianteDestino,
                    ascenso.IdParada,
                    descenso.IdParada,
                    mejorTransbordo.ParadaOrigen,
                    mejorTransbordo.ParadaDestino,
                    distTotal);

                if (mejor is null || candidato.DistanciaCaminando < mejor.DistanciaCaminando)
                {
                    mejor = candidato;
                }
            }
        }

        if (mejor is null)
        {
            return null;
        }

        var paradaAscenso = origenPorId[mejor.IdParadaAscenso];
        var paradaDescenso = destinoPorId[mejor.IdParadaDescenso];

        return new RutaResultadoDto
        {
            TipoRuta = TipoRuta.Transbordo,
            Variante1 = variantesInfo[mejor.IdVariante1],
            Variante2 = variantesInfo[mejor.IdVariante2],
            ParadaAscenso = new ParadaDto(paradaAscenso.IdParada, paradaAscenso.Nombre, paradaAscenso.Latitud, paradaAscenso.Longitud),
            ParadaDescenso = new ParadaDto(paradaDescenso.IdParada, paradaDescenso.Nombre, paradaDescenso.Latitud, paradaDescenso.Longitud),
            ParadaTransbordoOrigen = new ParadaDto(
                mejor.TransbordoOrigen.IdParada,
                mejor.TransbordoOrigen.Nombre,
                mejor.TransbordoOrigen.Latitud,
                mejor.TransbordoOrigen.Longitud),
            ParadaTransbordoDestino = new ParadaDto(
                mejor.TransbordoDestino.IdParada,
                mejor.TransbordoDestino.Nombre,
                mejor.TransbordoDestino.Latitud,
                mejor.TransbordoDestino.Longitud),
            DistanciaCaminando = mejor.DistanciaCaminando
        };
    }

    private async Task<List<ParadaCercanaDto>> GetParadasCercanasAsync(double lat, double lon, double maxMeters)
    {
        // Filtro por "caja" para limitar resultados en SQL; Haversine se calcula en memoria.
        var (minLat, maxLat, minLon, maxLon) = GetBoundingBox(lat, lon, maxMeters);

        var candidatas = await _db.Paradas.AsNoTracking()
            .Where(p => p.Latitud >= minLat && p.Latitud <= maxLat && p.Longitud >= minLon && p.Longitud <= maxLon)
            .Select(p => new
            {
                p.IdParada,
                p.Nombre,
                p.Latitud,
                p.Longitud
            })
            .ToListAsync();

        return candidatas
            .Select(p =>
            {
                var dist = HaversineMeters(lat, lon, p.Latitud, p.Longitud);
                return new ParadaCercanaDto(p.IdParada, p.Nombre, p.Latitud, p.Longitud, dist);
            })
            .Where(p => p.DistanciaMetros <= maxMeters)
            .OrderBy(p => p.DistanciaMetros)
            .ToList();
    }

    private static TransferPairDto? FindBestTransferPair(
        IReadOnlyList<VarianteParadaDetalleDto> stopsOrigen,
        IReadOnlyList<VarianteParadaDetalleDto> stopsDestino,
        double maxWalkMeters)
    {
        TransferPairDto? mejor = null;

        for (var i = 0; i < stopsOrigen.Count; i++)
        {
            var o = stopsOrigen[i];
            var (minLat, maxLat, minLon, maxLon) = GetBoundingBox(o.Latitud, o.Longitud, maxWalkMeters);

            for (var j = 0; j < stopsDestino.Count; j++)
            {
                var d = stopsDestino[j];
                if (d.Latitud < minLat || d.Latitud > maxLat || d.Longitud < minLon || d.Longitud > maxLon)
                {
                    continue;
                }

                var dist = HaversineMeters(o.Latitud, o.Longitud, d.Latitud, d.Longitud);
                if (dist > maxWalkMeters)
                {
                    continue;
                }

                if (mejor is null || dist < mejor.DistanciaMetros)
                {
                    mejor = new TransferPairDto(o, d, dist);
                }
            }
        }

    return mejor;
    }

    private static (double MinLat, double MaxLat, double MinLon, double MaxLon) GetBoundingBox(double lat, double lon, double radiusMeters)
    {
        // Aproximación suficiente para radios pequeños (<= 300m)
        const double metersPerDegreeLat = 111_320d;
        var latDelta = radiusMeters / metersPerDegreeLat;
        var lonDelta = radiusMeters / (metersPerDegreeLat * Math.Cos(DegToRad(lat)));

        return (lat - latDelta, lat + latDelta, lon - lonDelta, lon + lonDelta);
    }

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6_371_000d;
        var dLat = DegToRad(lat2 - lat1);
        var dLon = DegToRad(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double DegToRad(double degrees) => degrees * (Math.PI / 180d);

    // =========================
    // DTOs
    // =========================
    public enum TipoRuta
    {
        Directa = 1,
        Transbordo = 2
    }

    public sealed class RutaResultadoDto
    {
        public TipoRuta TipoRuta { get; set; }
        public VarianteDto? Variante1 { get; set; }
        public VarianteDto? Variante2 { get; set; }
        public ParadaDto? ParadaAscenso { get; set; }
        public ParadaDto? ParadaDescenso { get; set; }
        public ParadaDto? ParadaTransbordoOrigen { get; set; }
        public ParadaDto? ParadaTransbordoDestino { get; set; }
        public double DistanciaCaminando { get; set; }
    }

    public sealed record VarianteDto(int IdRutaVariante, string Nombre, string? Sentido);
    public sealed record ParadaDto(string IdParada, string? Nombre, double Latitud, double Longitud);

    private sealed record ParadaCercanaDto(string IdParada, string? Nombre, double Latitud, double Longitud, double DistanciaMetros);
    private sealed record VarianteParadaOrdenDto(int IdRutaVariante, string IdParada, int Orden);
    private sealed record VarianteParadaDetalleDto(int IdRutaVariante, string IdParada, int Orden, string? Nombre, double Latitud, double Longitud);
    private sealed record TransferPairDto(VarianteParadaDetalleDto ParadaOrigen, VarianteParadaDetalleDto ParadaDestino, double DistanciaMetros);
    private sealed record DirectoRowDto
    {
        public required int IdRutaVariante { get; init; }
        public required string NombreVariante { get; init; }
        public string? Sentido { get; init; }
        public required string IdParadaAscenso { get; init; }
        public required int OrdenAscenso { get; init; }
        public required string IdParadaDescenso { get; init; }
        public required int OrdenDescenso { get; init; }
    }

    private sealed record TransferBestDto(
        int IdVariante1,
        int IdVariante2,
        string IdParadaAscenso,
        string IdParadaDescenso,
        VarianteParadaDetalleDto TransbordoOrigen,
        VarianteParadaDetalleDto TransbordoDestino,
        double DistanciaCaminando);
}

// =========================
// EF Core (Contexto + Entidades)
// =========================
public sealed class SsmiRuteoDbContext : DbContext
{
    public SsmiRuteoDbContext(DbContextOptions<SsmiRuteoDbContext> options) : base(options) { }

    public DbSet<TbParada> Paradas => Set<TbParada>();
    public DbSet<TbRutaVariante> RutaVariantes => Set<TbRutaVariante>();
    public DbSet<TbRutaVarianteParada> RutaVarianteParadas => Set<TbRutaVarianteParada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbParada>(e =>
        {
            e.ToTable("tbParadas");
            e.HasKey(x => x.IdParada);
            e.Property(x => x.IdParada).HasColumnName("IdParada");
            e.Property(x => x.Nombre).HasColumnName("Nombre");
            e.Property(x => x.Latitud).HasColumnName("Latitud");
            e.Property(x => x.Longitud).HasColumnName("Longitud");
        });

        modelBuilder.Entity<TbRutaVariante>(e =>
        {
            e.ToTable("tbRutaVariantes");
            e.HasKey(x => x.IdRutaVariante);
            e.Property(x => x.IdRutaVariante).HasColumnName("IdRutaVariante");
            e.Property(x => x.Nombre).HasColumnName("Nombre");
            e.Property(x => x.Sentido).HasColumnName("Sentido");
        });

        modelBuilder.Entity<TbRutaVarianteParada>(e =>
        {
            e.ToTable("tbRutaVarianteParadas");
            e.HasKey(x => new { x.IdRutaVariante, x.IdParada });
            e.Property(x => x.IdRutaVariante).HasColumnName("IdRutaVariante");
            e.Property(x => x.IdParada).HasColumnName("IdParada");
            e.Property(x => x.Orden).HasColumnName("Orden");
        });
    }
}

[Table("tbParadas")]
public sealed class TbParada
{
    [Key]
    public required string IdParada { get; set; }
    public string? Nombre { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
}

[Table("tbRutaVariantes")]
public sealed class TbRutaVariante
{
    [Key]
    public int IdRutaVariante { get; set; }
    public required string Nombre { get; set; }
    public string? Sentido { get; set; }
}

[Table("tbRutaVarianteParadas")]
public sealed class TbRutaVarianteParada
{
    public int IdRutaVariante { get; set; }
    public required string IdParada { get; set; }
    public int Orden { get; set; }
}
