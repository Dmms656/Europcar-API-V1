import { StyleSheet, Text, View, ViewStyle } from 'react-native';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';

const ESTADO_COLORS: Record<string, string> = {
  ABIERTO: colors.info,
  CERRADO: colors.success,
  ANULADO: colors.danger,
  PAGADA: colors.success,
  ANULADA: colors.danger,
  PENDIENTE: colors.warning,
  EMITIDA: colors.warning,
};

type Props = {
  label: string;
  style?: ViewStyle;
};

export function StatusBadge({ label, style }: Props) {
  const bg = ESTADO_COLORS[label?.toUpperCase()] ?? colors.surfaceElevated;
  return (
    <View style={[styles.badge, { backgroundColor: `${bg}33` }, style]}>
      <Text style={[styles.text, { color: bg }]}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: {
    paddingHorizontal: spacing.sm,
    paddingVertical: 4,
    borderRadius: radius.sm,
    alignSelf: 'flex-start',
  },
  text: { fontSize: 11, fontWeight: '700', textTransform: 'uppercase' },
});
