using Microsoft.EntityFrameworkCore;
using RedCar.Catalogo.Api.Contracts;
using RedCar.Catalogo.DataAccess.Context;
using RedCar.Catalogo.DataAccess.Entities;

namespace RedCar.Catalogo.Api.Services;

public sealed class VehiculosAdminService
{
    private static readonly string[] Combustibles = ["GASOLINA", "DIESEL", "HIBRIDO", "ELECTRICO"];
    private static readonly string[] Transmisiones = ["AUTOMATICA", "MANUAL"];
    private static readonly string[] EstadosEditables = ["DISPONIBLE", "RESERVADO", "MANTENIMIENTO", "TALLER", "FUERA_SERVICIO"];

    private readonly CatalogoDbContext _db;

    public VehiculosAdminService(CatalogoDbContext db) => _db = db;

    public async Task<VehiculoAdminDto> CreateAsync(CrearVehiculoRequest req, string usuario, CancellationToken ct)
    {
        Validate(req);
        var placa = req.PlacaVehiculo.Trim().ToUpperInvariant();
        if (await _db.Vehiculos.AnyAsync(v => !v.EsEliminado && v.PlacaVehiculo == placa, ct))
            throw new InvalidOperationException($"Ya existe un vehículo con placa {placa}");

        await EnsureFkAsync(req.IdMarca, req.IdCategoria, ct);

        var entity = new Vehiculo
        {
            VehiculoGuid = Guid.NewGuid(),
            CodigoInternoVehiculo = $"VEH-{DateTime.UtcNow:yyyyMMddHHmmss}",
            PlacaVehiculo = placa,
            IdMarca = req.IdMarca,
            IdCategoria = req.IdCategoria,
            ModeloVehiculo = req.ModeloVehiculo.Trim(),
            AnioFabricacion = req.AnioFabricacion,
            ColorVehiculo = req.ColorVehiculo.Trim(),
            TipoCombustible = req.TipoCombustible.Trim().ToUpperInvariant(),
            TipoTransmision = req.TipoTransmision.Trim().ToUpperInvariant(),
            CapacidadPasajeros = req.CapacidadPasajeros,
            CapacidadMaletas = req.CapacidadMaletas,
            NumeroPuertas = req.NumeroPuertas,
            LocalizacionActual = req.IdLocalizacion,
            PrecioBaseDia = req.PrecioBaseDia,
            KilometrajeActual = req.KilometrajeActual,
            AireAcondicionado = req.AireAcondicionado,
            EstadoOperativo = "DISPONIBLE",
            ObservacionesGenerales = req.ObservacionesGenerales,
            ImagenReferencialUrl = req.ImagenReferencialUrl,
            EstadoVehiculo = "ACT",
            EsEliminado = false,
            FechaRegistroUtc = DateTimeOffset.UtcNow,
            CreadoPorUsuario = usuario,
            OrigenRegistro = "ADMIN_API",
            RowVersion = 1
        };

        _db.Vehiculos.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await MapAdminAsync(entity.IdVehiculo, ct) ?? throw new InvalidOperationException("Error al cargar vehículo creado.");
    }

    public async Task<VehiculoAdminDto> UpdateAsync(int id, ActualizarVehiculoRequest req, string usuario, CancellationToken ct)
    {
        Validate(req);
        var entity = await _db.Vehiculos.FirstOrDefaultAsync(v => v.IdVehiculo == id && !v.EsEliminado, ct)
            ?? throw new KeyNotFoundException($"Vehículo {id} no encontrado");

        if (entity.EstadoOperativo == "ALQUILADO")
            throw new InvalidOperationException("No se puede editar un vehículo ALQUILADO");

        var placa = req.PlacaVehiculo.Trim().ToUpperInvariant();
        if (!string.Equals(entity.PlacaVehiculo, placa, StringComparison.Ordinal)
            && await _db.Vehiculos.AnyAsync(v => !v.EsEliminado && v.PlacaVehiculo == placa && v.IdVehiculo != id, ct))
            throw new InvalidOperationException($"Ya existe un vehículo con placa {placa}");

        await EnsureFkAsync(req.IdMarca, req.IdCategoria, ct);

        entity.PlacaVehiculo = placa;
        entity.IdMarca = req.IdMarca;
        entity.IdCategoria = req.IdCategoria;
        entity.ModeloVehiculo = req.ModeloVehiculo.Trim();
        entity.AnioFabricacion = req.AnioFabricacion;
        entity.ColorVehiculo = req.ColorVehiculo.Trim();
        entity.TipoCombustible = req.TipoCombustible.Trim().ToUpperInvariant();
        entity.TipoTransmision = req.TipoTransmision.Trim().ToUpperInvariant();
        entity.CapacidadPasajeros = req.CapacidadPasajeros;
        entity.CapacidadMaletas = req.CapacidadMaletas;
        entity.NumeroPuertas = req.NumeroPuertas;
        entity.LocalizacionActual = req.IdLocalizacion;
        entity.PrecioBaseDia = req.PrecioBaseDia;
        entity.KilometrajeActual = req.KilometrajeActual;
        entity.AireAcondicionado = req.AireAcondicionado;
        entity.ObservacionesGenerales = req.ObservacionesGenerales;
        entity.ImagenReferencialUrl = req.ImagenReferencialUrl;
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion = req.RowVersion > 0 ? req.RowVersion : entity.RowVersion + 1;

        await _db.SaveChangesAsync(ct);
        return await MapAdminAsync(id, ct) ?? throw new InvalidOperationException("Error al cargar vehículo actualizado.");
    }

    public async Task CambiarEstadoAsync(int id, CambiarEstadoVehiculoRequest req, string usuario, CancellationToken ct)
    {
        var entity = await _db.Vehiculos.FirstOrDefaultAsync(v => v.IdVehiculo == id && !v.EsEliminado, ct)
            ?? throw new KeyNotFoundException($"Vehículo {id} no encontrado");

        if (entity.EstadoOperativo == "ALQUILADO")
            throw new InvalidOperationException("ALQUILADO no es editable manualmente");

        var estado = (req.EstadoOperativo ?? string.Empty).Trim().ToUpperInvariant();
        if (!EstadosEditables.Contains(estado))
            throw new InvalidOperationException($"Estado inválido. Permitidos: {string.Join(", ", EstadosEditables)}");
        if (estado == "ALQUILADO")
            throw new InvalidOperationException("ALQUILADO solo vía flujo operativo");

        entity.EstadoOperativo = estado;
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, string usuario, CancellationToken ct)
    {
        var entity = await _db.Vehiculos.FirstOrDefaultAsync(v => v.IdVehiculo == id && !v.EsEliminado, ct)
            ?? throw new KeyNotFoundException($"Vehículo {id} no encontrado");

        if (entity.EstadoOperativo is "ALQUILADO" or "RESERVADO")
            throw new InvalidOperationException("No se puede eliminar un vehículo alquilado o reservado");

        entity.EsEliminado = true;
        entity.EstadoVehiculo = "INA";
        entity.ModificadoPorUsuario = usuario;
        entity.FechaModificacionUtc = DateTimeOffset.UtcNow;
        entity.FechaInhabilitacionUtc = DateTimeOffset.UtcNow;
        entity.MotivoInhabilitacion = "Eliminado desde panel admin";
        entity.RowVersion++;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<VehiculoAdminDto?> MapAdminAsync(int id, CancellationToken ct) =>
        await _db.Vehiculos.AsNoTracking()
            .Where(v => v.IdVehiculo == id)
            .Select(v => new VehiculoAdminDto
            {
                IdVehiculo = v.IdVehiculo,
                VehiculoGuid = v.VehiculoGuid,
                CodigoInterno = v.CodigoInternoVehiculo,
                Placa = v.PlacaVehiculo,
                IdMarca = v.IdMarca,
                Marca = v.Marca != null ? v.Marca.NombreMarca : string.Empty,
                IdCategoria = v.IdCategoria,
                Categoria = v.Categoria != null ? v.Categoria.NombreCategoria : string.Empty,
                Modelo = v.ModeloVehiculo,
                AnioFabricacion = v.AnioFabricacion,
                Color = v.ColorVehiculo,
                TipoCombustible = v.TipoCombustible,
                TipoTransmision = v.TipoTransmision,
                CapacidadPasajeros = v.CapacidadPasajeros,
                CapacidadMaletas = v.CapacidadMaletas,
                NumeroPuertas = v.NumeroPuertas,
                PrecioBaseDia = v.PrecioBaseDia,
                KilometrajeActual = v.KilometrajeActual,
                AireAcondicionado = v.AireAcondicionado,
                EstadoOperativo = v.EstadoOperativo,
                ObservacionesGenerales = v.ObservacionesGenerales,
                ImagenReferencialUrl = v.ImagenReferencialUrl,
                IdLocalizacion = v.LocalizacionActual,
                EstadoVehiculo = v.EstadoVehiculo,
                RowVersion = v.RowVersion
            })
            .FirstOrDefaultAsync(ct);

    private async Task EnsureFkAsync(int idMarca, int idCategoria, CancellationToken ct)
    {
        if (!await _db.Marcas.AnyAsync(m => m.IdMarca == idMarca && !m.EsEliminado, ct))
            throw new InvalidOperationException($"Marca {idMarca} no encontrada");
        if (!await _db.Categorias.AnyAsync(c => c.IdCategoria == idCategoria && !c.EsEliminado, ct))
            throw new InvalidOperationException($"Categoría {idCategoria} no encontrada");
    }

    private static void Validate(CrearVehiculoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.PlacaVehiculo))
            throw new ArgumentException("La placa es obligatoria");
        if (!Combustibles.Contains(req.TipoCombustible.Trim().ToUpperInvariant()))
            throw new ArgumentException($"Combustible inválido. Permitidos: {string.Join(", ", Combustibles)}");
        if (!Transmisiones.Contains(req.TipoTransmision.Trim().ToUpperInvariant()))
            throw new ArgumentException($"Transmisión inválida. Permitidos: {string.Join(", ", Transmisiones)}");
        if (req.PrecioBaseDia <= 0)
            throw new ArgumentException("Precio base debe ser mayor a 0");
        if (req.CapacidadPasajeros <= 0 || req.NumeroPuertas <= 0)
            throw new ArgumentException("Capacidad y puertas deben ser mayores a 0");
    }
}
