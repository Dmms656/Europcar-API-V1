import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Modal,
  Pressable,
  RefreshControl,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { router, useFocusEffect } from 'expo-router';
import { reservasApi } from '@/src/api/reservasApi';
import { ReservaCard } from '@/src/components/ReservaCard';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';
import {
  isReservaActiva,
  isReservaCancelable,
  type ReservaItem,
} from '@/src/utils/reservas';

export default function ReservasScreen() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const refreshProfile = useAuthStore((s) => s.refreshProfile);

  const [reservas, setReservas] = useState<ReservaItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');
  const [codigoConsulta, setCodigoConsulta] = useState('');

  const [cancelTarget, setCancelTarget] = useState<ReservaItem | null>(null);
  const [cancelMotivo, setCancelMotivo] = useState('');
  const [cancelling, setCancelling] = useState(false);

  const loadReservas = useCallback(async () => {
    setError('');
    setLoading(true);
    try {
      const profile = await refreshProfile();
      const idCliente = profile?.idCliente ?? useAuthStore.getState().user?.idCliente;
      if (!idCliente) {
        setReservas([]);
        setError('Tu perfil no tiene idCliente. Vuelve a iniciar sesión.');
        return;
      }
      const res = await reservasApi.getByCliente(idCliente);
      const data = unwrapData<ReservaItem[]>(res);
      const all = Array.isArray(data) ? data : [];
      setReservas(all.filter((r) => isReservaActiva(r as Record<string, unknown>)));
    } catch (e) {
      setError(getErrorMessage(e));
      setReservas([]);
    } finally {
      setLoading(false);
    }
  }, [refreshProfile]);

  useFocusEffect(
    useCallback(() => {
      if (isAuthenticated) loadReservas();
    }, [isAuthenticated, loadReservas]),
  );

  const onRefresh = async () => {
    setRefreshing(true);
    await loadReservas();
    setRefreshing(false);
  };

  const handleConsultaCodigo = () => {
    const codigo = codigoConsulta.trim().toUpperCase();
    if (!codigo) return;
    router.push(`/reserva/${codigo}`);
  };

  const handleCancelar = async () => {
    if (!cancelTarget || !cancelMotivo.trim()) {
      setError('Ingresa el motivo de cancelación');
      return;
    }
    const id = cancelTarget.idReserva ?? cancelTarget.id;
    if (!id) return;
    setCancelling(true);
    setError('');
    try {
      await reservasApi.cancelar(id, cancelMotivo.trim());
      setCancelTarget(null);
      setCancelMotivo('');
      await loadReservas();
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setCancelling(false);
    }
  };

  if (!isAuthenticated) {
    return (
      <View style={styles.container}>
        <Text style={styles.title}>Mis reservas</Text>
        <Text style={styles.sub}>Inicia sesión para ver tus reservas activas</Text>
        <Pressable style={styles.button} onPress={() => router.push('/login')}>
          <Text style={styles.buttonText}>Iniciar sesión</Text>
        </Pressable>

        <View style={styles.consultaBox}>
          <Text style={styles.consultaTitle}>Consultar por código</Text>
          <TextInput
            style={styles.input}
            placeholder="RES-XXXXXX-XXXX"
            placeholderTextColor={colors.textMuted}
            autoCapitalize="characters"
            value={codigoConsulta}
            onChangeText={setCodigoConsulta}
          />
          <Pressable style={styles.buttonSecondary} onPress={handleConsultaCodigo}>
            <Text style={styles.buttonSecondaryText}>Ver reserva</Text>
          </Pressable>
        </View>
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
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={colors.primary} />}
          ListHeaderComponent={
            <View style={styles.header}>
              <Text style={styles.title}>Reservas activas</Text>
              <Text style={styles.sub}>
                Próximas o en curso. Las pasadas están en la pestaña Historial.
              </Text>
              {error ? <Text style={styles.error}>{error}</Text> : null}
            </View>
          }
          ListEmptyComponent={
            !loading ? (
              <View style={styles.empty}>
                <Text style={styles.sub}>No tienes reservas activas.</Text>
                <Pressable style={styles.button} onPress={() => router.push('/buscar')}>
                  <Text style={styles.buttonText}>Buscar vehículo</Text>
                </Pressable>
              </View>
            ) : null
          }
          renderItem={({ item }) => (
            <ReservaCard
              reserva={item}
              cancelable={isReservaCancelable(item as Record<string, unknown>)}
              onPress={() => {
                const codigo = item.codigoReserva;
                if (codigo) router.push(`/reserva/${codigo}`);
              }}
              onCancel={() => setCancelTarget(item)}
            />
          )}
          contentContainerStyle={{ paddingBottom: 24 }}
        />
      )}

      <Modal visible={!!cancelTarget} transparent animationType="fade">
        <View style={styles.modalOverlay}>
          <View style={styles.modalCard}>
            <Text style={styles.modalTitle}>Cancelar reserva</Text>
            <Text style={styles.sub}>
              {cancelTarget?.codigoReserva ?? ''} — indica el motivo
            </Text>
            <TextInput
              style={[styles.input, { marginTop: 12 }]}
              placeholder="Motivo de cancelación"
              placeholderTextColor={colors.textMuted}
              value={cancelMotivo}
              onChangeText={setCancelMotivo}
              multiline
            />
            <View style={styles.modalActions}>
              <Pressable style={styles.buttonGhost} onPress={() => setCancelTarget(null)}>
                <Text style={styles.buttonGhostText}>Cerrar</Text>
              </Pressable>
              <Pressable style={styles.buttonDanger} onPress={handleCancelar} disabled={cancelling}>
                {cancelling ? (
                  <ActivityIndicator color="#fff" />
                ) : (
                  <Text style={styles.buttonText}>Confirmar</Text>
                )}
              </Pressable>
            </View>
          </View>
        </View>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 16 },
  header: { marginBottom: 12 },
  title: { color: colors.text, fontSize: 22, fontWeight: '700' },
  sub: { color: colors.textMuted, marginTop: 8, lineHeight: 20 },
  error: { color: colors.danger, marginTop: 8 },
  empty: { alignItems: 'center', paddingTop: 32 },
  button: {
    marginTop: 20,
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 10,
    alignSelf: 'center',
    minWidth: 180,
    alignItems: 'center',
  },
  buttonText: { color: '#fff', fontWeight: '600' },
  buttonSecondary: {
    marginTop: 10,
    backgroundColor: colors.surfaceAlt,
    padding: 12,
    borderRadius: 10,
    alignItems: 'center',
  },
  buttonSecondaryText: { color: colors.text, fontWeight: '600' },
  consultaBox: {
    marginTop: 40,
    padding: 16,
    backgroundColor: colors.surface,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  consultaTitle: { color: colors.text, fontWeight: '600', marginBottom: 8 },
  input: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderWidth: 1,
    borderRadius: 10,
    padding: 12,
    color: colors.text,
    minHeight: 44,
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: '#000000aa',
    justifyContent: 'center',
    padding: 24,
  },
  modalCard: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    padding: 20,
    borderWidth: 1,
    borderColor: colors.border,
  },
  modalTitle: { color: colors.text, fontSize: 18, fontWeight: '700' },
  modalActions: { flexDirection: 'row', justifyContent: 'flex-end', gap: 10, marginTop: 16 },
  buttonGhost: { padding: 12 },
  buttonGhostText: { color: colors.textMuted },
  buttonDanger: {
    backgroundColor: colors.danger,
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderRadius: 8,
    minWidth: 110,
    alignItems: 'center',
  },
});
