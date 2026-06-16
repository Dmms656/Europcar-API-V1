import { useEffect, useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useLocalSearchParams } from 'expo-router';
import { bookingApi } from '@/src/api/bookingApi';
import { reservasApi } from '@/src/api/reservasApi';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';
import type { ReservaItem } from '@/src/utils/reservas';

export default function ReservaDetalleScreen() {
  const { codigo } = useLocalSearchParams<{ codigo: string }>();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  const [reserva, setReserva] = useState<ReservaItem | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!codigo) return;
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError('');
      try {
        if (isAuthenticated) {
          try {
            const res = await reservasApi.getByCodigo(String(codigo));
            const data = unwrapData<ReservaItem>(res);
            if (!cancelled && data) {
              setReserva(data);
              return;
            }
          } catch {
            /* fallback público */
          }
        }
        const res = await bookingApi.getReservaByCodigo(String(codigo));
        const data = unwrapData<ReservaItem>(res);
        if (!cancelled) setReserva(data ?? null);
      } catch (e) {
        if (!cancelled) setError(getErrorMessage(e));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [codigo, isAuthenticated]);

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  if (error || !reserva) {
    return (
      <View style={styles.center}>
        <Text style={styles.error}>{error || 'Reserva no encontrada'}</Text>
      </View>
    );
  }

  const estado = reserva.estadoReserva ?? reserva.estado ?? '—';
  const vehiculo =
    reserva.vehiculo?.marca
      ? `${reserva.vehiculo.marca} ${reserva.vehiculo.modelo ?? ''}`
      : `${reserva.marcaVehiculo ?? ''} ${reserva.modeloVehiculo ?? ''}`.trim() || '—';

  return (
    <ScrollView style={styles.container} contentContainerStyle={{ paddingBottom: 32 }}>
      <Text style={styles.codigo}>{reserva.codigoReserva ?? codigo}</Text>
      <Text style={styles.estado}>{estado}</Text>

      <View style={styles.block}>
        <Text style={styles.label}>Vehículo</Text>
        <Text style={styles.value}>{vehiculo}</Text>
      </View>
      <View style={styles.block}>
        <Text style={styles.label}>Recogida</Text>
        <Text style={styles.value}>
          {String(reserva.fechaHoraRecogida ?? reserva.fechaRecogida ?? '—')}
        </Text>
      </View>
      <View style={styles.block}>
        <Text style={styles.label}>Devolución</Text>
        <Text style={styles.value}>
          {String(reserva.fechaHoraDevolucion ?? reserva.fechaDevolucion ?? '—')}
        </Text>
      </View>
      {reserva.total != null && (
        <View style={styles.block}>
          <Text style={styles.label}>Total</Text>
          <Text style={[styles.value, { color: colors.accent }]}>${reserva.total}</Text>
        </View>
      )}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 20 },
  center: { flex: 1, backgroundColor: colors.bg, justifyContent: 'center', alignItems: 'center', padding: 24 },
  codigo: { color: colors.text, fontSize: 24, fontWeight: '800' },
  estado: { color: colors.primary, fontWeight: '700', marginTop: 8, marginBottom: 20 },
  block: {
    backgroundColor: colors.surface,
    borderRadius: 10,
    padding: 14,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: colors.border,
  },
  label: { color: colors.textMuted, fontSize: 12, marginBottom: 4 },
  value: { color: colors.text, fontSize: 16 },
  error: { color: colors.danger, textAlign: 'center' },
});
