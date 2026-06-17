import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
  useWindowDimensions,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { bookingApi } from '@/src/api/bookingApi';
import { CatalogVehicleCard } from '@/src/components/catalog/CatalogVehicleCard';
import { Button } from '@/src/components/ui/Button';
import { GradientBackground } from '@/src/components/ui/GradientBackground';
import { PaginationControls } from '@/src/components/ui/PaginationControls';
import { Select } from '@/src/components/ui/Select';
import { Input } from '@/src/components/ui/Input';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts, text } from '@/src/theme/typography';
import {
  getPayload,
  loadAllCatalogVehicles,
  type VehiculoBooking,
} from '@/src/utils/bookingNormalize';

type Categoria = { id?: number; idCategoria?: number; nombre?: string; nombreCategoria?: string };
type Ciudad = { idCiudad: number; idPais?: number; nombreCiudad?: string; nombrePais?: string };
type Localizacion = { idLocalizacion: number; nombreLocalizacion?: string; idCiudad?: number };

type Filtros = {
  pais: string;
  ciudad: string;
  categoria: string;
  combustible: string;
  transmision: string;
  precioMin: string;
  precioMax: string;
};

const EMPTY_FILTROS: Filtros = {
  pais: '',
  ciudad: '',
  categoria: '',
  combustible: '',
  transmision: '',
  precioMin: '',
  precioMax: '',
};

export default function CatalogWebScreen() {
  const params = useLocalSearchParams<{
    categoria?: string;
    pais?: string;
    ciudad?: string;
    localizacion?: string;
    idCategoria?: string;
  }>();
  const { width } = useWindowDimensions();

  const [vehiculos, setVehiculos] = useState<VehiculoBooking[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [ciudades, setCiudades] = useState<Ciudad[]>([]);
  const [localizaciones, setLocalizaciones] = useState<Localizacion[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filtros, setFiltros] = useState<Filtros>(EMPTY_FILTROS);
  const [showFilters, setShowFilters] = useState(false);
  const [paginaActual, setPaginaActual] = useState(1);
  const [vehiculosPorPagina, setVehiculosPorPagina] = useState(10);

  useEffect(() => {
    const cat = typeof params.categoria === 'string' ? decodeURIComponent(params.categoria) : '';
    const pais = typeof params.pais === 'string' ? params.pais : '';
    const ciudad = typeof params.ciudad === 'string' ? params.ciudad : '';
    if (cat || pais || ciudad) {
      setFiltros((prev) => ({
        ...prev,
        categoria: cat || prev.categoria,
        pais: pais || prev.pais,
        ciudad: ciudad || prev.ciudad,
      }));
    }
  }, [params.categoria, params.pais, params.ciudad]);

  useEffect(() => {
    const locParam = typeof params.localizacion === 'string' ? params.localizacion : '';
    if (!locParam || localizaciones.length === 0) return;
    const loc = localizaciones.find((l) => String(l.idLocalizacion) === locParam);
    if (loc?.idCiudad) {
      setFiltros((prev) => ({ ...prev, ciudad: String(loc.idCiudad) }));
    }
  }, [localizaciones, params.localizacion]);

  useEffect(() => {
    const idParam = typeof params.idCategoria === 'string' ? params.idCategoria : '';
    if (!idParam || categorias.length === 0) return;
    const id = Number(idParam);
    if (Number.isNaN(id)) return;
    const found = categorias.find((c) => Number(c.id ?? c.idCategoria) === id);
    const nombre = found?.nombre ?? found?.nombreCategoria ?? '';
    if (nombre) setFiltros((prev) => ({ ...prev, categoria: nombre }));
  }, [categorias, params.idCategoria]);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [catRes, ciuRes, locRes] = await Promise.allSettled([
        bookingApi.getCategorias(),
        bookingApi.getCiudades(),
        bookingApi.getLocalizaciones({ page: 1, limit: 200 }),
      ]);

      if (catRes.status === 'fulfilled') {
        const payload = getPayload<{ categorias?: Categoria[]; Categorias?: Categoria[] }>(catRes.value);
        setCategorias(payload.categorias ?? payload.Categorias ?? []);
      }
      if (ciuRes.status === 'fulfilled') {
        const payload = getPayload<{ ciudades?: Ciudad[]; Ciudades?: Ciudad[] }>(ciuRes.value);
        const items = payload.ciudades ?? payload.Ciudades ?? [];
        setCiudades(
          items
            .map((c) => ({
              idCiudad: Number(c.idCiudad),
              idPais: c.idPais,
              nombreCiudad: c.nombreCiudad ?? (c as { nombre?: string }).nombre,
              nombrePais: c.nombrePais,
            }))
            .filter((c) => c.idCiudad > 0),
        );
      }
      let locs: Localizacion[] = [];
      if (locRes.status === 'fulfilled') {
        const payload = getPayload<{ localizaciones?: Localizacion[]; Localizaciones?: Localizacion[] }>(locRes.value);
        const items = payload.localizaciones ?? payload.Localizaciones ?? [];
        locs = items
          .map((l) => ({
            idLocalizacion: Number(l.idLocalizacion),
            nombreLocalizacion: l.nombreLocalizacion ?? (l as { nombre?: string }).nombre,
            idCiudad: l.idCiudad,
          }))
          .filter((l) => l.idLocalizacion > 0);
        setLocalizaciones(locs);
      }

      if (locs.length === 0) {
        setVehiculos([]);
      } else {
        const fleet = await loadAllCatalogVehicles(locs, (p) => bookingApi.buscarVehiculos(p));
        setVehiculos(fleet);
      }
    } catch {
      setVehiculos([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const paisesOptions = useMemo(() => {
    const map = new Map<string, string>();
    ciudades.forEach((c) => {
      if (c.idPais) map.set(String(c.idPais), c.nombrePais || `País ${c.idPais}`);
    });
    return Array.from(map.entries())
      .map(([value, label]) => ({ value, label }))
      .sort((a, b) => a.label.localeCompare(b.label));
  }, [ciudades]);

  const ciudadesFiltradas = useMemo(
    () => (filtros.pais ? ciudades.filter((c) => String(c.idPais) === filtros.pais) : ciudades),
    [ciudades, filtros.pais],
  );

  const filtered = useMemo(() => {
    const search = searchTerm.toLowerCase();
    const ciudadById = new Map(ciudades.map((c) => [String(c.idCiudad), c]));
    const locationByNombre = new Map(
      localizaciones.map((l) => [(l.nombreLocalizacion ?? '').trim().toLowerCase(), l]),
    );

    return vehiculos.filter((v) => {
      const matchSearch =
        !search ||
        (v.marca ?? '').toLowerCase().includes(search) ||
        (v.modelo ?? v.modeloVehiculo ?? '').toLowerCase().includes(search) ||
        (v.categoria ?? '').toLowerCase().includes(search);

      const locName = (v.nombreSucursal ?? String(v.localizacion ?? '')).trim().toLowerCase();
      const localizacion = locationByNombre.get(locName);
      const ciudad = ciudadById.get(String(localizacion?.idCiudad ?? ''));

      const matchPais = !filtros.pais || String(ciudad?.idPais ?? '') === filtros.pais;
      const matchCiudad = !filtros.ciudad || String(localizacion?.idCiudad ?? '') === filtros.ciudad;

      const catFilter = filtros.categoria.trim().toLowerCase();
      const matchCategoria =
        !catFilter || (v.categoria ?? v.nombreCategoria ?? '').toString().trim().toLowerCase() === catFilter;

      const matchCombustible =
        !filtros.combustible ||
        (v.tipoCombustible ?? '').toLowerCase() === filtros.combustible.toLowerCase();
      const matchTransmision =
        !filtros.transmision ||
        (v.tipoTransmision ?? '').toLowerCase() === filtros.transmision.toLowerCase();

      const precio = Number(v.precioBaseDia ?? v.precioDia ?? 0);
      const matchPrecioMin = !filtros.precioMin || precio >= Number(filtros.precioMin);
      const matchPrecioMax = !filtros.precioMax || precio <= Number(filtros.precioMax);

      return (
        matchSearch &&
        matchPais &&
        matchCiudad &&
        matchCategoria &&
        matchCombustible &&
        matchTransmision &&
        matchPrecioMin &&
        matchPrecioMax
      );
    });
  }, [vehiculos, searchTerm, filtros, localizaciones, ciudades]);

  useEffect(() => {
    setPaginaActual(1);
  }, [searchTerm, filtros]);

  const totalPaginas = Math.max(1, Math.ceil(filtered.length / vehiculosPorPagina));
  const paginados = useMemo(() => {
    const start = (paginaActual - 1) * vehiculosPorPagina;
    return filtered.slice(start, start + vehiculosPorPagina);
  }, [filtered, paginaActual, vehiculosPorPagina]);

  const activeFilterCount =
    Object.values(filtros).filter(Boolean).length + (searchTerm ? 1 : 0);

  const clearFilters = () => {
    setFiltros(EMPTY_FILTROS);
    setSearchTerm('');
  };

  const reservar = (v: VehiculoBooking) => {
    const id = v.idVehiculo ?? v.id;
    if (!id) return;
    router.push({ pathname: '/reservar/[id]', params: { id: String(id) } });
  };

  const columns = width >= 1200 ? 3 : width >= 768 ? 2 : 1;

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.pageContent}>
      <GradientBackground variant="hero" style={styles.hero}>
        <View style={styles.heroInner}>
          <Text style={styles.heroTitle}>
            Encuentra tu vehículo <Text style={styles.heroAccent}>ideal</Text>
          </Text>
          <Text style={styles.heroSub}>
            Explora toda la flota — las fechas de recogida y devolución las eliges al reservar
          </Text>

          <View style={styles.searchRow}>
            <Ionicons name="search" size={20} color={colors.textMuted} style={styles.searchIcon} />
            <TextInput
              style={styles.searchInput}
              placeholder="Buscar por marca, modelo o categoría..."
              placeholderTextColor={colors.textMuted}
              value={searchTerm}
              onChangeText={setSearchTerm}
            />
            <Button
              label={`Filtros${activeFilterCount > 0 ? ` (${activeFilterCount})` : ''}`}
              variant="secondary"
              onPress={() => setShowFilters((s) => !s)}
            />
          </View>
        </View>
      </GradientBackground>

      {showFilters ? (
        <View style={styles.filters}>
          <View style={styles.filtersGrid}>
            <Select
              label="País"
              value={filtros.pais}
              onValueChange={(v) => setFiltros({ ...filtros, pais: v, ciudad: '' })}
              options={paisesOptions}
              placeholder="Todos"
            />
            <Select
              label="Ciudad"
              value={filtros.ciudad}
              onValueChange={(v) => setFiltros({ ...filtros, ciudad: v })}
              options={ciudadesFiltradas.map((c) => ({
                value: String(c.idCiudad),
                label: c.nombreCiudad ?? `Ciudad ${c.idCiudad}`,
              }))}
              placeholder="Todas"
            />
            <Select
              label="Categoría"
              value={filtros.categoria}
              onValueChange={(v) => setFiltros({ ...filtros, categoria: v })}
              options={categorias.map((c) => ({
                value: c.nombre ?? c.nombreCategoria ?? '',
                label: c.nombre ?? c.nombreCategoria ?? 'Categoría',
              }))}
              placeholder="Todas"
            />
            <Select
              label="Combustible"
              value={filtros.combustible}
              onValueChange={(v) => setFiltros({ ...filtros, combustible: v })}
              options={[
                { value: 'GASOLINA', label: 'Gasolina' },
                { value: 'DIESEL', label: 'Diésel' },
                { value: 'HIBRIDO', label: 'Híbrido' },
                { value: 'ELECTRICO', label: 'Eléctrico' },
              ]}
              placeholder="Todos"
            />
            <Select
              label="Transmisión"
              value={filtros.transmision}
              onValueChange={(v) => setFiltros({ ...filtros, transmision: v })}
              options={[
                { value: 'AUTOMATICA', label: 'Automática' },
                { value: 'MANUAL', label: 'Manual' },
              ]}
              placeholder="Todas"
            />
            <Input
              label="Precio mín."
              value={filtros.precioMin}
              onChangeText={(v) => setFiltros({ ...filtros, precioMin: v })}
              keyboardType="decimal-pad"
              placeholder="$0"
            />
            <Input
              label="Precio máx."
              value={filtros.precioMax}
              onChangeText={(v) => setFiltros({ ...filtros, precioMax: v })}
              keyboardType="decimal-pad"
              placeholder="$999"
            />
            <Button label="Limpiar" variant="ghost" onPress={clearFilters} />
          </View>
        </View>
      ) : null}

      <View style={styles.content}>
        <Text style={styles.resultsTitle}>
          {filtered.length} vehículos disponibles
          {filtered.length > 0 ? ` · Página ${paginaActual} de ${totalPaginas}` : ''}
        </Text>

        {loading ? (
          <View style={styles.center}>
            <ActivityIndicator size="large" color={colors.primary} />
            <Text style={styles.muted}>Cargando flota...</Text>
          </View>
        ) : filtered.length === 0 ? (
          <View style={styles.empty}>
            <Ionicons name="car-sport-outline" size={64} color={colors.textMuted} />
            <Text style={styles.emptyTitle}>No se encontraron vehículos</Text>
            <Text style={styles.muted}>Intenta ajustar los filtros de búsqueda</Text>
            <Button label="Limpiar filtros" variant="client" onPress={clearFilters} />
          </View>
        ) : (
          <View style={[styles.grid, { gap: spacing.lg }]}>
            {paginados.map((v) => (
              <View
                key={String(v.idVehiculo ?? v.id)}
                style={{
                  flexGrow: 1,
                  flexBasis: columns === 3 ? '31%' : columns === 2 ? '47%' : '100%',
                  minWidth: 300,
                  maxWidth: columns === 1 ? '100%' : 420,
                }}
              >
                <CatalogVehicleCard vehiculo={v} onPress={() => reservar(v)} />
              </View>
            ))}
          </View>
        )}

        {!loading && filtered.length > 0 ? (
          <PaginationControls
            page={paginaActual}
            totalPages={totalPaginas}
            pageSize={vehiculosPorPagina}
            totalItems={filtered.length}
            startItem={(paginaActual - 1) * vehiculosPorPagina + 1}
            endItem={Math.min(paginaActual * vehiculosPorPagina, filtered.length)}
            onPageChange={setPaginaActual}
            onPageSizeChange={setVehiculosPorPagina}
            pageSizes={[10, 20, 50, 100]}
          />
        ) : null}
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: colors.bg },
  pageContent: { paddingBottom: spacing.xxl },
  hero: { borderRadius: 0, marginHorizontal: -spacing.xl },
  heroInner: { paddingVertical: spacing.xxl, paddingHorizontal: spacing.xl, maxWidth: 1400, alignSelf: 'center', width: '100%' },
  heroTitle: { ...text.h1, fontSize: 36, color: colors.text, fontFamily: fonts.extraBold },
  heroAccent: { color: colors.primaryLight },
  heroSub: { color: colors.textSecondary, marginTop: spacing.sm, fontSize: 16, fontFamily: fonts.regular },
  searchRow: {
    marginTop: spacing.xl,
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    paddingLeft: spacing.md,
    paddingRight: spacing.sm,
    paddingVertical: spacing.sm,
    gap: spacing.sm,
    flexWrap: 'wrap',
  },
  searchIcon: { marginRight: spacing.xs },
  searchInput: {
    flex: 1,
    minWidth: 200,
    color: colors.text,
    fontFamily: fonts.regular,
    fontSize: 15,
    paddingVertical: spacing.sm,
  },
  filters: {
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    paddingHorizontal: spacing.xl,
    paddingVertical: spacing.lg,
  },
  filtersGrid: {
    maxWidth: 1400,
    alignSelf: 'center',
    width: '100%',
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.md,
  },
  content: {
    maxWidth: 1400,
    alignSelf: 'center',
    width: '100%',
    paddingHorizontal: spacing.xl,
    paddingTop: spacing.xl,
  },
  resultsTitle: { color: colors.text, fontFamily: fonts.bold, fontSize: 18, marginBottom: spacing.lg },
  grid: { flexDirection: 'row', flexWrap: 'wrap' },
  center: { alignItems: 'center', paddingVertical: spacing.xxl, gap: spacing.md },
  empty: { alignItems: 'center', paddingVertical: spacing.xxl, gap: spacing.md },
  emptyTitle: { color: colors.text, fontFamily: fonts.bold, fontSize: 20 },
  muted: { color: colors.textMuted, fontFamily: fonts.regular },
});
