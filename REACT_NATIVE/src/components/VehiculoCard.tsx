import { Image, Pressable, StyleSheet, Text, View } from 'react-native';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';

type Props = {
  vehiculo: {
    idVehiculo: number;
    marca?: string;
    modelo?: string;
    codigoInterno?: string;
    precioDia?: number;
    imagenUrl?: string;
    transmision?: string;
    categoria?: string;
    localizacion?: string;
    disponible?: boolean;
  };
  onPress: () => void;
};

export function VehiculoCard({ vehiculo, onPress }: Props) {
  const titulo = `${vehiculo.marca ?? ''} ${vehiculo.modelo ?? ''}`.trim();
  return (
    <Pressable style={styles.card} onPress={onPress}>
      <View style={styles.imageWrap}>
        {vehiculo.imagenUrl ? (
          <View style={styles.imageFrame}>
            <Image source={{ uri: vehiculo.imagenUrl }} style={styles.image} resizeMode="contain" />
          </View>
        ) : (
          <View style={[styles.imageFrame, styles.imagePlaceholder]}>
            <Text style={styles.placeholderText}>Sin imagen</Text>
          </View>
        )}
      </View>
      <View style={styles.body}>
        <Text style={styles.title}>{titulo || vehiculo.codigoInterno}</Text>
        {vehiculo.categoria ? <Text style={styles.category}>{vehiculo.categoria}</Text> : null}
        <Text style={styles.meta}>
          {[vehiculo.transmision, vehiculo.localizacion].filter(Boolean).join(' · ') || '—'}
        </Text>
        <View style={styles.row}>
          <Text style={styles.price}>${vehiculo.precioDia ?? '—'}/día</Text>
          {vehiculo.disponible !== false && (
            <Text style={styles.badge}>Disponible</Text>
          )}
        </View>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    overflow: 'hidden',
    marginBottom: 12,
    borderWidth: 1,
    borderColor: colors.border,
  },
  imageWrap: {
    width: '100%',
    aspectRatio: 4 / 3,
    backgroundColor: colors.surfaceAlt,
    overflow: 'hidden',
  },
  imageFrame: {
    ...StyleSheet.absoluteFillObject,
    padding: spacing.md,
    alignItems: 'center',
    justifyContent: 'center',
  },
  image: { width: '100%', height: '100%' },
  imagePlaceholder: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  placeholderText: { color: colors.textMuted },
  body: { padding: 14 },
  title: { color: colors.text, fontSize: 17, fontWeight: '700' },
  category: { color: colors.accent, fontSize: 12, fontWeight: '600', marginTop: 2 },
  meta: { color: colors.textMuted, marginTop: 4, fontSize: 13 },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginTop: 10 },
  price: { color: colors.accent, fontWeight: '700', fontSize: 16 },
  badge: {
    color: colors.accent,
    fontSize: 12,
    backgroundColor: '#10b98122',
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
  },
});
