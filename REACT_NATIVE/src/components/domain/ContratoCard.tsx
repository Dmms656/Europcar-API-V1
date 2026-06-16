import { Pressable, StyleSheet, Text, View } from 'react-native';
import type { ContratoItem } from '@/src/api/contratosApi';
import { StatusBadge } from '@/src/components/ui/StatusBadge';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { contratoCodigo, contratoEstado, formatCurrency, formatDateEs } from '@/src/utils/format';

type Props = {
  contrato: ContratoItem;
  onPress: () => void;
};

export function ContratoCard({ contrato, onPress }: Props) {
  const codigo = contratoCodigo(contrato);
  const estado = contratoEstado(contrato);
  const salida = formatDateEs(contrato.fechaHoraSalida || contrato.fechaSalida);
  const devolucion = formatDateEs(contrato.fechaHoraDevolucion || contrato.fechaDevolucion);
  const placa = contrato.placaVehiculo || contrato.vehiculo || '—';
  const total = formatCurrency(contrato.totalContrato ?? contrato.total);

  return (
    <Pressable style={styles.card} onPress={onPress}>
      <View style={styles.header}>
        <Text style={styles.codigo}>{codigo}</Text>
        <StatusBadge label={estado} />
      </View>
      <Text style={styles.meta}>🚗 {placa}</Text>
      <Text style={styles.dates}>
        {salida} → {devolucion}
      </Text>
      <View style={styles.footer}>
        <Text style={styles.total}>{total}</Text>
        <Text style={styles.ver}>Ver detalle</Text>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderRadius: radius.md,
    padding: spacing.lg,
    marginBottom: spacing.md,
    borderWidth: 1,
    borderColor: colors.border,
  },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: spacing.sm },
  codigo: { color: colors.text, fontWeight: '700', fontSize: 16, flex: 1 },
  meta: { color: colors.text, marginTop: spacing.sm },
  dates: { color: colors.textMuted, fontSize: 13, marginTop: spacing.xs },
  footer: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: spacing.md },
  total: { color: colors.accent, fontWeight: '700', fontSize: 15 },
  ver: { color: colors.primaryLight, fontWeight: '600', fontSize: 13 },
});
