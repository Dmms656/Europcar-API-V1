import { useCallback, useEffect, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  RefreshControl,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { bookingApi } from '@/src/api/bookingApi';
import { VehiculoCard } from '@/src/components/VehiculoCard';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { unwrapData } from '@/src/utils/apiResponse';

type Vehiculo = {
  idVehiculo: number;
  marca?: string;
  modelo?: string;
  codigoInterno?: string;
  precioDia?: number;
  imagenUrl?: string;
  transmision?: string;
};

export default function BuscarScreen() {
  const params = useLocalSearchParams<{
    idLocalizacion?: string;
    fechaRecogida?: string;
    fechaDevolucion?: string;
  }>();

  const idLocalizacion = Number(params.idLocalizacion || 1);
  const fechaRecogida = params.fechaRecogida || (() => {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return d.toISOString().slice(0, 10);
  })();
  const fechaDevolucion = params.fechaDevolucion || (() => {
    const d = new Date();
    d.setDate(d.getDate() + 4);
    return d.toISOString().slice(0, 10);
  })();

  const [vehiculos, setVehiculos] = useState<Vehiculo[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = useCallback(async () => {
    const res = await bookingApi.buscarVehiculos({
      idLocalizacion,
      fechaRecogida: `${fechaRecogida}T10:00:00`,
      fechaDevolucion: `${fechaDevolucion}T10:00:00`,
      page: 1,
      limit: 20,
    });
    const data = unwrapData<{ vehiculos?: Vehiculo[]; items?: Vehiculo[] }>(res);
    setVehiculos(data?.vehiculos ?? data?.items ?? []);
  }, [idLocalizacion, fechaRecogida, fechaDevolucion]);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const onRefresh = async () => {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.primary} size="large" />
      </View>
    );
  }

  return (
    <FlatList
      style={styles.list}
      contentContainerStyle={styles.content}
      data={vehiculos}
      keyExtractor={(item) => String(item.idVehiculo)}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={colors.primary} />}
      ListHeaderComponent={
        <View style={styles.header}>
          <Text style={styles.title}>{vehiculos.length} vehículos disponibles</Text>
          <Text style={styles.dates}>
            {fechaRecogida} → {fechaDevolucion}
          </Text>
        </View>
      }
      ListEmptyComponent={<Text style={styles.empty}>No hay vehículos para esta búsqueda</Text>}
      renderItem={({ item }) => (
        <VehiculoCard
          vehiculo={item}
          onPress={() =>
            router.push({
              pathname: `/reservar/${item.idVehiculo}`,
              params: {
                idLocalizacion: String(idLocalizacion),
                fechaRecogida,
                fechaDevolucion,
              },
            })
          }
        />
      )}
    />
  );
}

const styles = StyleSheet.create({
  list: { flex: 1, backgroundColor: colors.bg },
  content: { padding: spacing.lg, paddingBottom: spacing.xxl },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
  header: { marginBottom: spacing.md },
  title: { color: colors.text, fontWeight: '700', fontSize: 18 },
  dates: { color: colors.textMuted, marginTop: 4 },
  empty: { color: colors.textMuted, textAlign: 'center', marginTop: 40 },
});
