import { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Pressable,
  RefreshControl,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { router, useFocusEffect } from 'expo-router';
import { bookingApi } from '@/src/api/bookingApi';
import { Input } from '@/src/components/ui/Input';
import { VehiculoCard } from '@/src/components/VehiculoCard';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import {
  getPayload,
  loadCatalogFromBooking,
  type VehiculoBooking,
} from '@/src/utils/bookingNormalize';

type Categoria = { id?: number; idCategoria?: number; nombre?: string; nombreCategoria?: string };
type Localizacion = { idLocalizacion: number; nombre?: string; nombreLocalizacion?: string };

export default function CatalogoScreen() {
  const [vehiculos, setVehiculos] = useState<VehiculoBooking[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [search, setSearch] = useState('');
  const [categoriaFilter, setCategoriaFilter] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [catRes, locRes] = await Promise.all([
        bookingApi.getCategorias(),
        bookingApi.getLocalizaciones({ page: 1, limit: 30 }),
      ]);
      const catPayload = getPayload<{ categorias?: Categoria[]; Categorias?: Categoria[] }>(catRes);
      setCategorias(catPayload.categorias ?? catPayload.Categorias ?? []);

      const locPayload = getPayload<{ items?: Localizacion[]; localizaciones?: Localizacion[] }>(locRes);
      const locs =
        locPayload.items ??
        locPayload.localizaciones ??
        (Array.isArray(locPayload) ? (locPayload as Localizacion[]) : []);

      const normalizedLocs = locs
        .map((l) => ({
          idLocalizacion: Number(l.idLocalizacion),
          nombre: l.nombre ?? l.nombreLocalizacion,
        }))
        .filter((l) => l.idLocalizacion > 0);

      const fleet = await loadCatalogFromBooking(normalizedLocs, (params) => bookingApi.buscarVehiculos(params));
      setVehiculos(fleet);
    } catch {
      setVehiculos([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const onRefresh = async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  };

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    const cat = categoriaFilter.trim().toLowerCase();
    return vehiculos.filter((v) => {
      const matchSearch =
        !q ||
        (v.marca ?? '').toLowerCase().includes(q) ||
        (v.modelo ?? '').toLowerCase().includes(q) ||
        (v.categoria ?? '').toLowerCase().includes(q);
      const matchCat = !cat || (v.categoria ?? '').toLowerCase() === cat;
      return matchSearch && matchCat;
    });
  }, [vehiculos, search, categoriaFilter]);

  const reservar = (v: VehiculoBooking) => {
    const id = v.idVehiculo ?? v.id;
    if (!id) return;
    router.push({
      pathname: `/reservar/${id}`,
      params: v.idLocalizacion ? { idLocalizacion: String(v.idLocalizacion) } : {},
    });
  };

  if (loading && vehiculos.length === 0) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.accent} size="large" />
        <Text style={styles.loadingText}>Cargando catálogo…</Text>
      </View>
    );
  }

  return (
    <FlatList
      style={styles.list}
      contentContainerStyle={styles.content}
      data={filtered}
      keyExtractor={(item) => String(item.idVehiculo ?? item.id)}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={colors.accent} />}
      ListHeaderComponent={
        <View style={styles.header}>
          <View style={styles.hero}>
            <Text style={styles.heroTitle}>Encuentra tu vehículo ideal</Text>
            <Text style={styles.heroSub}>Toca un vehículo para elegir fechas, extras y pagar</Text>
          </View>

          <Input
            placeholder="Buscar marca, modelo o categoría…"
            value={search}
            onChangeText={setSearch}
          />

          <Text style={styles.filterLabel}>Categoría</Text>
          <View style={styles.chips}>
            <Pressable
              style={[styles.chip, !categoriaFilter && styles.chipActive]}
              onPress={() => setCategoriaFilter('')}
            >
              <Text style={styles.chipText}>Todas</Text>
            </Pressable>
            {categorias.map((c) => {
              const name = c.nombre ?? c.nombreCategoria ?? '';
              if (!name) return null;
              return (
                <Pressable
                  key={String(c.id ?? c.idCategoria ?? name)}
                  style={[styles.chip, categoriaFilter === name && styles.chipActive]}
                  onPress={() => setCategoriaFilter(name)}
                >
                  <Text style={styles.chipText}>{name}</Text>
                </Pressable>
              );
            })}
          </View>

          <Text style={styles.count}>{filtered.length} vehículos disponibles</Text>
        </View>
      }
      ListEmptyComponent={
        <Text style={styles.empty}>No hay vehículos con esos filtros</Text>
      }
      renderItem={({ item }) => (
        <VehiculoCard
          vehiculo={{
            idVehiculo: item.idVehiculo ?? item.id ?? 0,
            marca: item.marca,
            modelo: item.modelo,
            precioDia: item.precioDia ?? item.precioBaseDia,
            imagenUrl: item.imagenUrl,
            transmision: item.tipoTransmision,
            categoria: item.categoria,
            localizacion: typeof item.localizacion === 'string' ? item.localizacion : item.nombreSucursal,
          }}
          onPress={() => reservar(item)}
        />
      )}
    />
  );
}

const styles = StyleSheet.create({
  list: { flex: 1, backgroundColor: colors.bg },
  content: { padding: spacing.lg, paddingBottom: spacing.xxl },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
  loadingText: { color: colors.textMuted, marginTop: spacing.md },
  header: { marginBottom: spacing.sm },
  hero: {
    padding: spacing.lg,
    borderRadius: radius.lg,
    backgroundColor: colors.clientGhost,
    borderWidth: 1,
    borderColor: 'rgba(59,130,246,0.25)',
    marginBottom: spacing.lg,
  },
  heroTitle: { color: colors.text, fontSize: 22, fontWeight: '800' },
  heroSub: { color: colors.textSecondary, marginTop: 6 },
  filterLabel: { color: colors.textSecondary, fontWeight: '600', marginBottom: spacing.sm },
  chips: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginBottom: spacing.md },
  chip: {
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: radius.full,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
  },
  chipActive: { borderColor: colors.accent, backgroundColor: colors.clientGhost },
  chipText: { color: colors.text, fontSize: 13, fontWeight: '600' },
  count: { color: colors.textMuted, marginBottom: spacing.sm },
  empty: { color: colors.textMuted, textAlign: 'center', marginTop: 40 },
});
