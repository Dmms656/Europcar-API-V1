import { useCallback, useState } from 'react';
import { ActivityIndicator, FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { adminReservasApi } from '@/src/api/adminApi';
import { Card } from '@/src/components/ui/Card';
import { Screen } from '@/src/components/ui/Screen';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

type Reserva = {
  codigoReserva?: string;
  estadoReserva?: string;
  fechaInicio?: string;
  fechaFin?: string;
  total?: number;
  cliente?: { nombreCompleto?: string; nombres?: string };
};

export default function AdminReservasScreen() {
  const [items, setItems] = useState<Reserva[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setError('');
    try {
      const res = await adminReservasApi.getAll();
      const data = unwrapData<Reserva[]>(res);
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
      keyExtractor={(item, i) => item.codigoReserva ?? String(i)}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={colors.primary} />}
      ListHeaderComponent={
        <View style={styles.header}>
          <Text style={styles.title}>Reservas</Text>
          <Text style={styles.sub}>{items.length} registros</Text>
          {error ? <Text style={styles.error}>{error}</Text> : null}
        </View>
      }
      ListEmptyComponent={<Text style={styles.empty}>No hay reservas</Text>}
      renderItem={({ item }) => (
        <Card>
          <Text style={styles.code}>{item.codigoReserva ?? '—'}</Text>
          <Text style={styles.meta}>
            {item.cliente?.nombreCompleto ?? item.cliente?.nombres ?? 'Cliente'} · {item.estadoReserva ?? '—'}
          </Text>
          <Text style={styles.meta}>
            {item.fechaInicio?.slice(0, 10)} → {item.fechaFin?.slice(0, 10)}
          </Text>
          {item.total != null ? <Text style={styles.total}>${item.total}</Text> : null}
        </Card>
      )}
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
  code: { color: colors.primaryLight, fontWeight: '800', fontSize: 16 },
  meta: { color: colors.textSecondary, marginTop: 4 },
  total: { color: colors.text, fontWeight: '700', marginTop: 8 },
});
