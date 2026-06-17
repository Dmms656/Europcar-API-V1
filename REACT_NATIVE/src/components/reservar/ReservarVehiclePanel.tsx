import { Image, StyleSheet, Text, View } from 'react-native';
import { Card } from '@/src/components/ui/Card';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import type { VehiculoBooking } from '@/src/utils/bookingNormalize';

type Props = {
  vehiculo: VehiculoBooking | null;
  idVehiculo: number;
  titulo: string;
  precioBase: number;
  dias: number;
  subtotalVehiculo: number;
  subtotalExtras: number;
  recargoConductores?: number;
  iva: number;
  cargoOneWay: number;
  totalFinal: number;
  compact?: boolean;
};

export function ReservarVehiclePanel({
  vehiculo,
  idVehiculo,
  titulo,
  precioBase,
  dias,
  subtotalVehiculo,
  subtotalExtras,
  recargoConductores = 0,
  iva,
  cargoOneWay,
  totalFinal,
  compact,
}: Props) {
  return (
    <Card style={StyleSheet.flatten([styles.card, compact ? styles.cardCompact : null])}>
      {vehiculo?.imagenUrl ? (
        <Image source={{ uri: vehiculo.imagenUrl }} style={styles.img} resizeMode="cover" />
      ) : null}
      <Text style={styles.title}>{titulo || `Vehículo #${idVehiculo}`}</Text>
      <Text style={styles.muted}>
        ${precioBase.toFixed(2)}/día · {vehiculo?.categoria ?? '—'}
      </Text>
      {!compact ? (
        <View style={styles.summary}>
          <View style={styles.line}>
            <Text style={styles.lineLabel}>Vehículo ({dias} {dias === 1 ? 'día' : 'días'})</Text>
            <Text style={styles.lineValue}>${subtotalVehiculo.toFixed(2)}</Text>
          </View>
          {subtotalExtras > 0 ? (
            <View style={styles.line}>
              <Text style={styles.lineLabel}>Extras</Text>
              <Text style={styles.lineValue}>${subtotalExtras.toFixed(2)}</Text>
            </View>
          ) : null}
          {recargoConductores > 0 ? (
            <View style={styles.line}>
              <Text style={styles.lineLabel}>Conductores adicionales</Text>
              <Text style={styles.lineValue}>${recargoConductores.toFixed(2)}</Text>
            </View>
          ) : null}
          {cargoOneWay > 0 ? (
            <View style={styles.line}>
              <Text style={styles.lineLabel}>Cargo one-way</Text>
              <Text style={styles.lineValue}>${cargoOneWay.toFixed(2)}</Text>
            </View>
          ) : null}
          <View style={styles.line}>
            <Text style={styles.lineLabel}>IVA (15%)</Text>
            <Text style={styles.lineValue}>${iva.toFixed(2)}</Text>
          </View>
          <View style={[styles.line, styles.totalLine]}>
            <Text style={styles.totalLabel}>Total</Text>
            <Text style={styles.totalValue}>${totalFinal.toFixed(2)}</Text>
          </View>
        </View>
      ) : null}
    </Card>
  );
}

const styles = StyleSheet.create({
  card: { marginBottom: spacing.lg, overflow: 'hidden' },
  cardCompact: { marginBottom: 0 },
  img: { width: '100%', height: 160, marginBottom: spacing.sm, borderRadius: radius.md },
  title: { color: colors.text, fontSize: 18, fontWeight: '700' },
  muted: { color: colors.textMuted, marginTop: 4 },
  summary: { marginTop: spacing.lg, borderTopWidth: 1, borderTopColor: colors.border, paddingTop: spacing.md },
  line: { flexDirection: 'row', justifyContent: 'space-between', paddingVertical: 4 },
  lineLabel: { color: colors.textSecondary, fontSize: 14 },
  lineValue: { color: colors.textSecondary, fontSize: 14 },
  totalLine: { marginTop: spacing.sm, paddingTop: spacing.sm, borderTopWidth: 1, borderTopColor: colors.border },
  totalLabel: { color: colors.text, fontWeight: '700', fontSize: 16 },
  totalValue: { color: colors.accent, fontWeight: '800', fontSize: 16 },
});
