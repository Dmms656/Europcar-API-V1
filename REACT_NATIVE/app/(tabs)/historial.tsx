import { useCallback, useState } from 'react';
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
import { reservasApi } from '@/src/api/reservasApi';
import { ReservaCard } from '@/src/components/ReservaCard';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';
import { isReservaHistorica, type ReservaItem } from '@/src/utils/reservas';

export default function HistorialScreen() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const refreshProfile = useAuthStore((s) => s.refreshProfile);

  const [reservas, setReservas] = useState<ReservaItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const loadHistorial = useCallback(async () => {
    setError('');
    setLoading(true);
    try {
      const profile = await refreshProfile();
      const idCliente = profile?.idCliente ?? useAuthStore.getState().user?.idCliente;
      if (!idCliente) {
        setReservas([]);
        setError('Tu perfil no tiene idCliente.');
        return;
      }
      const res = await reservasApi.getByCliente(idCliente);
      const data = unwrapData<ReservaItem[]>(res);
      const all = Array.isArray(data) ? data : [];
      setReservas(all.filter((r) => isReservaHistorica(r as Record<string, unknown>)));
    } catch (e) {
      setError(getErrorMessage(e));
      setReservas([]);
    } finally {
      setLoading(false);
    }
  }, [refreshProfile]);

  useFocusEffect(
    useCallback(() => {
      if (isAuthenticated) loadHistorial();
    }, [isAuthenticated, loadHistorial]),
  );

  if (!isAuthenticated) {
    return (
      <View style={styles.center}>
        <Text style={styles.title}>Historial</Text>
        <Text style={styles.sub}>Inicia sesión para ver reservas pasadas</Text>
        <Pressable style={styles.button} onPress={() => router.push('/login')}>
          <Text style={styles.buttonText}>Iniciar sesión</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      {loading && reservas.length === 0 ? (
        <ActivityIndicator size="large" color={colors.primary} style={{ marginTop: 40 }} />
      ) : (
        <FlatList
          data={reservas}
          keyExtractor={(item) => String(item.idReserva ?? item.id ?? item.codigoReserva)}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={async () => {
                setRefreshing(true);
                await loadHistorial();
                setRefreshing(false);
              }}
              tintColor={colors.primary}
            />
          }
          ListHeaderComponent={
            <View style={styles.header}>
              <Text style={styles.title}>Historial</Text>
              <Text style={styles.sub}>Canceladas, finalizadas o ya devueltas.</Text>
              {error ? <Text style={styles.error}>{error}</Text> : null}
            </View>
          }
          ListEmptyComponent={
            !loading ? <Text style={[styles.sub, { textAlign: 'center', marginTop: 32 }]}>Sin historial aún.</Text> : null
          }
          renderItem={({ item }) => (
            <ReservaCard
              reserva={item}
              onPress={() => {
                const codigo = item.codigoReserva;
                if (codigo) router.push(`/reserva/${codigo}`);
              }}
            />
          )}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16 },
  center: { flex: 1, backgroundColor: colors.bg, padding: 24, justifyContent: 'center' },
  header: { marginBottom: 12 },
  title: { color: colors.text, fontSize: 22, fontWeight: '700' },
  sub: { color: colors.textMuted, marginTop: 8, lineHeight: 20 },
  error: { color: colors.danger, marginTop: 8 },
  button: {
    marginTop: 24,
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 10,
    alignSelf: 'center',
  },
  buttonText: { color: '#fff', fontWeight: '600' },
});
