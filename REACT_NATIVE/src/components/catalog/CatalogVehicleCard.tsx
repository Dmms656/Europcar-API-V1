import { Image, Pressable, StyleSheet, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Button } from '@/src/components/ui/Button';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import type { VehiculoBooking } from '@/src/utils/bookingNormalize';

type Props = {
  vehiculo: VehiculoBooking;
  onPress: () => void;
};

function isValidImageUrl(url?: string) {
  return Boolean(url && (url.startsWith('http://') || url.startsWith('https://')));
}

function isEco(fuel?: string) {
  const f = (fuel ?? '').toUpperCase();
  return f === 'ELECTRICO' || f === 'HIBRIDO';
}

export function CatalogVehicleCard({ vehiculo, onPress }: Props) {
  const titulo = `${vehiculo.marca ?? ''} ${vehiculo.modelo ?? vehiculo.modeloVehiculo ?? ''}`.trim();
  const loc =
    typeof vehiculo.localizacion === 'string'
      ? vehiculo.localizacion
      : vehiculo.nombreSucursal ?? '—';
  const precio = Number(vehiculo.precioBaseDia ?? vehiculo.precioDia ?? 0);

  return (
    <Pressable style={styles.card} onPress={onPress}>
      <View style={styles.imageWrap}>
        {isValidImageUrl(vehiculo.imagenUrl) ? (
          <View style={styles.imageFrame}>
            <Image source={{ uri: vehiculo.imagenUrl }} style={styles.image} resizeMode="contain" />
          </View>
        ) : (
          <View style={[styles.imageFrame, styles.imagePlaceholder]}>
            <Ionicons name="car-sport" size={56} color={colors.primaryLight} />
            <Text style={styles.placeholderTitle}>{titulo}</Text>
          </View>
        )}
        <View style={styles.badges}>
          {vehiculo.categoria ? (
            <View style={styles.badgeCategory}>
              <Text style={styles.badgeCategoryText}>{vehiculo.categoria}</Text>
            </View>
          ) : null}
          {isEco(vehiculo.tipoCombustible) ? (
            <View style={styles.badgeEco}>
              <Ionicons name="flash" size={12} color="#34d399" />
              <Text style={styles.badgeEcoText}>Eco</Text>
            </View>
          ) : null}
        </View>
      </View>

      <View style={styles.body}>
        <View style={styles.header}>
          <Text style={styles.title} numberOfLines={2}>{titulo || 'Vehículo'}</Text>
          {vehiculo.anioFabricacion ? (
            <Text style={styles.year}>{vehiculo.anioFabricacion}</Text>
          ) : null}
        </View>

        <View style={styles.specs}>
          <Spec icon="people-outline" label={`${vehiculo.capacidadPasajeros ?? '—'} pasajeros`} />
          <Spec icon="water-outline" label={vehiculo.tipoCombustible || '—'} />
          <Spec icon="settings-outline" label={vehiculo.tipoTransmision || '—'} />
          <Spec icon="location-outline" label={loc} />
        </View>

        <View style={styles.features}>
          {vehiculo.aireAcondicionado ? (
            <View style={styles.feature}>
              <Ionicons name="shield-checkmark-outline" size={14} color={colors.textSecondary} />
              <Text style={styles.featureText}>A/C</Text>
            </View>
          ) : null}
          <View style={styles.feature}>
            <Ionicons name="star-outline" size={14} color={colors.textSecondary} />
            <Text style={styles.featureText}>{vehiculo.capacidadMaletas ?? 2} maletas</Text>
          </View>
        </View>

        <View style={styles.footer}>
          <View>
            <Text style={styles.price}>${precio.toFixed(2)}</Text>
            <Text style={styles.priceUnit}>/día</Text>
          </View>
          <Button label="Reservar →" variant="client" onPress={onPress} style={styles.reserveBtn} />
        </View>
      </View>
    </Pressable>
  );
}

function Spec({ icon, label }: { icon: keyof typeof Ionicons.glyphMap; label: string }) {
  return (
    <View style={styles.spec}>
      <Ionicons name={icon} size={15} color={colors.textMuted} />
      <Text style={styles.specText} numberOfLines={1}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    flex: 1,
    minWidth: 300,
    maxWidth: '100%',
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    overflow: 'hidden',
  },
  imageWrap: {
    position: 'relative',
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
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.sm,
    padding: spacing.lg,
  },
  placeholderTitle: {
    color: colors.textSecondary,
    fontFamily: fonts.semiBold,
    textAlign: 'center',
  },
  badges: {
    position: 'absolute',
    top: spacing.md,
    left: spacing.md,
    flexDirection: 'row',
    gap: spacing.xs,
  },
  badgeCategory: {
    backgroundColor: 'rgba(13,148,136,0.9)',
    paddingHorizontal: spacing.sm,
    paddingVertical: 4,
    borderRadius: radius.full,
  },
  badgeCategoryText: {
    color: colors.white,
    fontSize: 11,
    fontFamily: fonts.bold,
    textTransform: 'uppercase',
  },
  badgeEco: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    backgroundColor: 'rgba(16,185,129,0.2)',
    paddingHorizontal: spacing.sm,
    paddingVertical: 4,
    borderRadius: radius.full,
    borderWidth: 1,
    borderColor: 'rgba(52,211,153,0.4)',
  },
  badgeEcoText: { color: '#34d399', fontSize: 11, fontFamily: fonts.bold },
  body: { padding: spacing.lg, gap: spacing.md },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', gap: spacing.sm },
  title: { flex: 1, color: colors.text, fontFamily: fonts.bold, fontSize: 18 },
  year: { color: colors.textMuted, fontFamily: fonts.medium, fontSize: 14 },
  specs: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm },
  spec: { flexDirection: 'row', alignItems: 'center', gap: 6, width: '47%', minWidth: 130 },
  specText: { flex: 1, color: colors.textSecondary, fontSize: 13, fontFamily: fonts.regular },
  features: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm },
  feature: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  featureText: { color: colors.textSecondary, fontSize: 12, fontFamily: fonts.medium },
  footer: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: spacing.md,
    marginTop: spacing.xs,
    paddingTop: spacing.md,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  price: { color: colors.primaryLight, fontFamily: fonts.extraBold, fontSize: 22 },
  priceUnit: { color: colors.textMuted, fontSize: 13, fontFamily: fonts.medium },
  reserveBtn: { minWidth: 120 },
});
