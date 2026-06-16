import { useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { Button } from '@/src/components/ui/Button';
import { Screen } from '@/src/components/ui/Screen';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';

export default function HomeScreen() {
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const t = setTimeout(() => setReady(true), 300);
    return () => clearTimeout(t);
  }, []);

  return (
    <Screen scroll={false} style={styles.wrap}>
      <View style={styles.hero}>
        <Text style={styles.badge}>Europcar Rental</Text>
        <Text style={styles.heroTitle}>Alquila el vehículo perfecto</Text>
        <Text style={styles.heroSub}>
          Explora nuestro catálogo, elige fechas, extras y confirma tu reserva con pago simulado.
        </Text>
      </View>

      <View style={styles.features}>
        {[
          { icon: '🚗', text: 'Catálogo completo de vehículos' },
          { icon: '📅', text: 'Fechas al reservar, como en la web' },
          { icon: '🎁', text: 'Extras y accesorios opcionales' },
          { icon: '💳', text: 'Pasarela de pago simulada' },
        ].map((f) => (
          <View key={f.text} style={styles.featureRow}>
            <Text style={styles.featureIcon}>{f.icon}</Text>
            <Text style={styles.featureText}>{f.text}</Text>
          </View>
        ))}
      </View>

      <Button
        label={ready ? 'Ver catálogo' : 'Cargando…'}
        onPress={() => router.push('/(tabs)/catalogo')}
        variant="client"
        disabled={!ready}
      />
      <Pressable onPress={() => router.push('/(tabs)/reservas')} style={styles.linkWrap}>
        <Text style={styles.link}>Consultar mis reservas</Text>
      </Pressable>
    </Screen>
  );
}

const styles = StyleSheet.create({
  wrap: { justifyContent: 'center' },
  hero: {
    padding: spacing.xl,
    borderRadius: radius.xl,
    backgroundColor: colors.primaryGhost,
    borderWidth: 1,
    borderColor: 'rgba(13,148,136,0.25)',
    marginBottom: spacing.xl,
  },
  badge: { color: colors.primaryLight, fontWeight: '700', fontSize: 12, letterSpacing: 1 },
  heroTitle: { color: colors.text, fontSize: 28, fontWeight: '800', marginTop: spacing.sm },
  heroSub: { color: colors.textSecondary, marginTop: spacing.md, lineHeight: 22 },
  features: { marginBottom: spacing.xl, gap: spacing.sm },
  featureRow: { flexDirection: 'row', alignItems: 'center', gap: spacing.md },
  featureIcon: { fontSize: 20 },
  featureText: { color: colors.text, flex: 1 },
  linkWrap: { marginTop: spacing.lg, alignItems: 'center' },
  link: { color: colors.accent, fontWeight: '600' },
});
