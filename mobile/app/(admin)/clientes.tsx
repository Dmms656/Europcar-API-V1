import { useCallback, useState } from 'react';
import { ActivityIndicator, FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { adminClientesApi } from '@/src/api/adminApi';
import { Card } from '@/src/components/ui/Card';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

type Cliente = {
  idCliente?: number;
  nombreCompleto?: string;
  nombres?: string;
  apellidos?: string;
  correo?: string;
  telefono?: string;
  numeroIdentificacion?: string;
};

export default function AdminClientesScreen() {
  const [items, setItems] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setError('');
    try {
      const res = await adminClientesApi.getAll();
      const data = unwrapData<Cliente[]>(res);
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
      keyExtractor={(item, i) => String(item.idCliente ?? i)}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={colors.primary} />}
      ListHeaderComponent={
        <View style={styles.header}>
          <Text style={styles.title}>Clientes</Text>
          <Text style={styles.sub}>{items.length} registrados</Text>
          {error ? <Text style={styles.error}>{error}</Text> : null}
        </View>
      }
      ListEmptyComponent={<Text style={styles.empty}>No hay clientes</Text>}
      renderItem={({ item }) => (
        <Card>
          <Text style={styles.name}>
            {(item.nombreCompleto ?? `${item.nombres ?? ''} ${item.apellidos ?? ''}`.trim()) || '—'}
          </Text>
          <Text style={styles.meta}>{item.correo ?? '—'}</Text>
          <Text style={styles.meta}>
            {item.numeroIdentificacion ? `ID ${item.numeroIdentificacion}` : ''}
            {item.telefono ? ` · ${item.telefono}` : ''}
          </Text>
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
  name: { color: colors.text, fontWeight: '700', fontSize: 16 },
  meta: { color: colors.textSecondary, marginTop: 4 },
});
