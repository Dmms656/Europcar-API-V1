import { StyleSheet, Text, View } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { radius, spacing } from '@/src/theme/layout';

type Props = {
  numeroTarjeta: string;
  nombreTitular: string;
  mesExpiracion: string;
  anioExpiracion: string;
};

export function PaymentCardVisual({ numeroTarjeta, nombreTitular, mesExpiracion, anioExpiracion }: Props) {
  const displayNumber = numeroTarjeta
    ? numeroTarjeta.replace(/(.{4})/g, '$1 ').trim()
    : '•••• •••• •••• ••••';

  return (
    <LinearGradient
      colors={['#1a1a2e', '#16213e', '#0f3460']}
      start={{ x: 0, y: 0 }}
      end={{ x: 1, y: 1 }}
      style={styles.visual}
    >
      <View style={styles.chip} />
      <Text style={styles.number}>{displayNumber}</Text>
      <View style={styles.detailsRow}>
        <View style={styles.detailBlock}>
          <Text style={styles.label}>TITULAR</Text>
          <Text style={styles.value} numberOfLines={1}>
            {nombreTitular || 'NOMBRE COMPLETO'}
          </Text>
        </View>
        <View style={styles.detailBlock}>
          <Text style={styles.label}>EXPIRA</Text>
          <Text style={styles.value}>
            {mesExpiracion || 'MM'}/{anioExpiracion || 'AA'}
          </Text>
        </View>
      </View>
    </LinearGradient>
  );
}

const styles = StyleSheet.create({
  visual: {
    borderRadius: radius.lg,
    padding: spacing.lg,
    minHeight: 180,
    justifyContent: 'space-between',
    marginBottom: spacing.lg,
  },
  chip: {
    width: 40,
    height: 28,
    borderRadius: 6,
    backgroundColor: '#d4af37',
  },
  number: {
    color: 'rgba(255,255,255,0.92)',
    fontSize: 20,
    letterSpacing: 2,
    fontWeight: '600',
    fontFamily: 'monospace',
    marginVertical: spacing.md,
  },
  detailsRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    gap: spacing.lg,
  },
  detailBlock: { flex: 1 },
  label: {
    color: 'rgba(255,255,255,0.5)',
    fontSize: 10,
    letterSpacing: 1,
    textTransform: 'uppercase',
    marginBottom: 4,
  },
  value: {
    color: 'rgba(255,255,255,0.88)',
    fontSize: 14,
    fontWeight: '600',
  },
});
