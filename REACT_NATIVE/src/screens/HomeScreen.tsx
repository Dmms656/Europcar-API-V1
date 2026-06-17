import { useEffect, useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { bookingApi } from '@/src/api/bookingApi';
import { HeroSearch, HomeCategories, HomeStats, HomeSteps } from '@/src/components/home/HeroSearch';
import { Screen } from '@/src/components/ui/Screen';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { getPayload } from '@/src/utils/bookingNormalize';

type Categoria = { idCategoria?: number; nombre?: string; nombreCategoria?: string; descripcion?: string };

export default function HomeScreen() {
  const [categorias, setCategorias] = useState<Categoria[]>([]);

  useEffect(() => {
    bookingApi.getCategorias().then((res) => {
      const payload = getPayload<{ categorias?: Categoria[]; Categorias?: Categoria[] }>(res);
      setCategorias(payload.categorias ?? payload.Categorias ?? []);
    }).catch(() => undefined);
  }, []);

  return (
    <Screen padded={false}>
      <View style={styles.wrap}>
        <HeroSearch />
        <View style={styles.body}>
          <HomeStats />
          <HomeSteps />
          <HomeCategories categorias={categorias} />
          <View style={styles.footer}>
            <View style={styles.footerBrand}>
              <Ionicons name="car-sport" size={22} color={colors.primaryLight} />
              <Text style={styles.footerTitle}>Europcar Rental</Text>
            </View>
            <Text style={styles.footerText}>
              Sistema de gestión de renta de vehículos © {new Date().getFullYear()}
            </Text>
          </View>
        </View>
      </View>
    </Screen>
  );
}

const styles = StyleSheet.create({
  wrap: { flex: 1 },
  body: { paddingHorizontal: spacing.lg, paddingBottom: spacing.xxl },
  footer: {
    marginTop: spacing.xl,
    paddingTop: spacing.xl,
    borderTopWidth: 1,
    borderTopColor: colors.border,
    alignItems: 'center',
    gap: spacing.sm,
  },
  footerBrand: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm },
  footerTitle: { color: colors.primaryLight, fontFamily: fonts.bold, fontSize: 16 },
  footerText: { color: colors.textMuted, fontSize: 12, textAlign: 'center' },
});
