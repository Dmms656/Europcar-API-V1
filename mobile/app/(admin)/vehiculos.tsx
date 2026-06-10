import { useCallback, useState } from 'react';
import { ActivityIndicator, FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { adminVehiculosApi } from '@/src/api/adminApi';
import { Card } from '@/src/components/ui/Card';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

type Vehiculo = {
  idVehiculo?: number;
  marca?: string;
  modelo?: string;
  codigoInterno?: string;
  estadoOperativo?: string;
  precioDia?: number;
};

const estadoColor: Record<string, string> = {
  DISPONIBLE: colors.success,
  ALQUILADO: colors.warning,
  MANTENIMIENTO: colors.info,
  TALLER: colors.info,
  FUERA_SERVICIO: colors.danger,
};

export default function AdminVehiculosScreen() {
  const [items, setItems] = useState<Vehiculo[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setError('');
    try {
      const res = await adminVehiculosApi.getAll();
      const data = unwrapData<Vehiculo[]>(res);
      setItems(Array.isArray(data) ? data : []);
    } catch (e) {
      setError(getErrorMessage(e));
      setItems([]);
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

  if (loading && items.length === 0) {
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
      data={items}
      keyExtractor={(item, i) => String(item.idVehiculo ?? i)}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={colors.primary} />}
      ListHeaderComponent={
        <View style={styles.header}>
          <Text style={styles.title}>Vehículos</Text>
          <Text style={styles.sub}>{items.length} en flota</Text>
          {error ? <Text style={styles.error}>{error}</Text> : null}
        </View>
      }
      ListEmptyComponent={<Text style={styles.empty}>No hay vehículos</Text>}
      renderItem={({ item }) => {
        const estado = item.estadoOperativo ?? '—';
        return (
          <Card>
            <View style={styles.row}>
              <Text style={styles.name}>
                {item.marca} {item.modelo}
              </Text>
              <View style={[styles.badge, { backgroundColor: estadoColor[estado] ?? colors.surfaceAlt }]}>
                <Text style={styles.badgeText}>{estado}</Text>
              </View>
            </View>
            <Text style={styles.meta}>{item.codigoInterno ?? `ID ${item.idVehiculo}`}</Text>
            {item.precioDia != null ? <Text style={styles.price}>${item.precioDia}/día</Text> : null}
          </Card>
        );
      }}
    />
  );
}

const styles = StyleSheet.create({
  list: { flex: 1, backgroundColor: colors.bg },
  content: { padding: spacing.lg, paddingBottom: spacing.xxl },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
  header: { marginBottom: spacing.md },
  title: { color: colors.text, fontSize: 22, fontWeight: '800' },
  sub: { color: colors.textMuted, marginTop: 4 },
  error: { color: colors.danger, marginTop: 8 },
  empty: { color: colors.textMuted, textAlign: 'center', marginTop: 40 },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: 8 },
  name: { color: colors.text, fontWeight: '700', fontSize: 16, flex: 1 },
  badge: { paddingHorizontal: 8, paddingVertical: 4, borderRadius: 999 },
  badgeText: { color: colors.white, fontSize: 10, fontWeight: '700' },
  meta: { color: colors.textSecondary, marginTop: 4 },
  price: { color: colors.primaryLight, fontWeight: '700', marginTop: 8 },
});
