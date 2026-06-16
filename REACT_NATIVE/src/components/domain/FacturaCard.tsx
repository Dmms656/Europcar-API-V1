import { StyleSheet, Text, View } from 'react-native';
import type { FacturaItem } from '@/src/api/facturasApi';
import { StatusBadge } from '@/src/components/ui/StatusBadge';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { formatCurrency, formatDateEs } from '@/src/utils/format';

type Props = {
  factura: FacturaItem;
  compact?: boolean;
};

export function FacturaCard({ factura, compact }: Props) {
  const estado = factura.estadoFactura || '—';

  if (compact) {
    return (
      <View style={styles.row}>
        <Text style={[styles.cell, styles.cellCode]}>{factura.numeroFactura || '—'}</Text>
        <Text style={styles.cell}>{formatDateEs(factura.fechaEmision)}</Text>
        <Text style={styles.cell}>{factura.codigoReserva || '—'}</Text>
        <Text style={styles.cell}>{factura.numeroContrato || '—'}</Text>
        <Text style={styles.cell}>{formatCurrency(factura.subtotal)}</Text>
        <Text style={styles.cell}>{formatCurrency(factura.valorIva)}</Text>
        <Text style={[styles.cell, styles.cellTotal]}>{formatCurrency(factura.total)}</Text>
        <View style={styles.cellBadge}>
          <StatusBadge label={estado} />
        </View>
      </View>
    );
  }

  return (
    <View style={styles.card}>
      <View style={styles.header}>
        <Text style={styles.numero}>{factura.numeroFactura || '—'}</Text>
        <StatusBadge label={estado} />
      </View>
      <Text style={styles.meta}>Fecha: {formatDateEs(factura.fechaEmision)}</Text>
      <Text style={styles.meta}>Reserva: {factura.codigoReserva || '—'}</Text>
      <Text style={styles.meta}>Contrato: {factura.numeroContrato || '—'}</Text>
      <View style={styles.totals}>
        <Text style={styles.sub}>Subtotal {formatCurrency(factura.subtotal)}</Text>
        <Text style={styles.sub}>IVA {formatCurrency(factura.valorIva)}</Text>
        <Text style={styles.total}>Total {formatCurrency(factura.total)}</Text>
      </View>
    </View>
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
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  numero: { color: colors.text, fontWeight: '700', fontSize: 16 },
  meta: { color: colors.textSecondary, fontSize: 13, marginTop: spacing.xs },
  totals: { marginTop: spacing.md, gap: 2 },
  sub: { color: colors.textMuted, fontSize: 13 },
  total: { color: colors.accent, fontWeight: '700', fontSize: 15, marginTop: 4 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.sm,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: spacing.sm,
  },
  cell: { flex: 1, color: colors.textSecondary, fontSize: 12, minWidth: 72 },
  cellCode: { color: colors.text, fontWeight: '600' },
  cellTotal: { color: colors.text, fontWeight: '700' },
  cellBadge: { flex: 1, minWidth: 80 },
});
