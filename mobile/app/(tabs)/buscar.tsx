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
  const params = useLocalSearchParams<{ idLocalizacion?: string }>();
  const idLocalizacion = Number(params.idLocalizacion || 1);
  const [vehiculos, setVehiculos] = useState<Vehiculo[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const load = useCallback(async () => {
    const now = new Date();
    const fin = new Date(now);
    fin.setDate(fin.getDate() + 3);
    const res = await bookingApi.buscarVehiculos({
      idLocalizacion,
      fechaRecogida: now.toISOString(),
      fechaDevolucion: fin.toISOString(),
      page: 1,
      limit: 20,
    });
    const data = unwrapData<{ vehiculos?: Vehiculo[]; items?: Vehiculo[] }>(res);
    setVehiculos(data?.vehiculos ?? data?.items ?? []);
  }, [idLocalizacion]);

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
      ListHeaderComponent={<Text style={styles.header}>{vehiculos.length} vehículos disponibles</Text>}
      ListEmptyComponent={<Text style={styles.empty}>No hay vehículos para esta búsqueda</Text>}
      renderItem={({ item }) => (
        <VehiculoCard
          vehiculo={item}
          onPress={() => router.push(`/reservar/${item.idVehiculo}?idLocalizacion=${idLocalizacion}`)}
        />
      )}
    />
  );
}

const styles = StyleSheet.create({
  list: { flex: 1, backgroundColor: colors.bg },
  content: { padding: 16 },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
  header: { color: colors.textMuted, marginBottom: 12 },
  empty: { color: colors.textMuted, textAlign: 'center', marginTop: 40 },
});
