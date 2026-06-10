import { Pressable, StyleSheet, Text, View } from 'react-native';
import { colors } from '@/src/theme/colors';
import type { ReservaItem } from '@/src/utils/reservas';

type Props = {
  reserva: ReservaItem;
  onPress: () => void;
  onCancel?: () => void;
  cancelable?: boolean;
};

export function ReservaCard({ reserva, onPress, onCancel, cancelable }: Props) {
  const codigo = reserva.codigoReserva ?? '—';
  const estado = reserva.estadoReserva ?? reserva.estado ?? '—';
  const vehiculo =
    reserva.vehiculo?.marca
      ? `${reserva.vehiculo.marca} ${reserva.vehiculo.modelo ?? ''}`
      : `${reserva.marcaVehiculo ?? ''} ${reserva.modeloVehiculo ?? ''}`.trim() || 'Vehículo';

  return (
    <Pressable style={styles.card} onPress={onPress}>
      <View style={styles.row}>
        <Text style={styles.codigo}>{codigo}</Text>
        <Text style={styles.estado}>{estado}</Text>
      </View>
      <Text style={styles.vehiculo}>{vehiculo}</Text>
      <Text style={styles.fechas}>
        {String(reserva.fechaHoraRecogida ?? reserva.fechaRecogida ?? '—')} →{' '}
        {String(reserva.fechaHoraDevolucion ?? reserva.fechaDevolucion ?? '—')}
      </Text>
      {reserva.total != null && <Text style={styles.total}>Total: ${reserva.total}</Text>}
      {cancelable && onCancel && (
        <Pressable style={styles.cancelBtn} onPress={onCancel}>
          <Text style={styles.cancelText}>Cancelar</Text>
        </Pressable>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    padding: 14,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: colors.border,
  },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  codigo: { color: colors.text, fontWeight: '700', fontSize: 15 },
  estado: { color: colors.primary, fontSize: 12, fontWeight: '600' },
  vehiculo: { color: colors.text, marginTop: 8 },
  fechas: { color: colors.textMuted, fontSize: 12, marginTop: 6 },
  total: { color: colors.accent, marginTop: 6, fontWeight: '600' },
  cancelBtn: {
    marginTop: 10,
    alignSelf: 'flex-start',
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 6,
    backgroundColor: '#ef444422',
  },
  cancelText: { color: colors.danger, fontWeight: '600', fontSize: 13 },
});
