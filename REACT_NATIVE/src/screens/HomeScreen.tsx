import { useEffect, useState } from 'react';
import { Platform, ScrollView, StyleSheet, Text, View } from 'react-native';
import { Link } from 'expo-router';
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
  const isWeb = Platform.OS === 'web';

  useEffect(() => {
    bookingApi.getCategorias().then((res) => {
      const payload = getPayload<{ categorias?: Categoria[]; Categorias?: Categoria[] }>(res);
      setCategorias(payload.categorias ?? payload.Categorias ?? []);
    }).catch(() => undefined);
  }, []);

  const body = (
    <>
      <HeroSearch />
      <View style={StyleSheet.flatten([styles.body, isWeb ? styles.bodyWeb : null])}>
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
          {isWeb ? (
            <View style={styles.footerLinks}>
              <Link href="/catalogo"><Text style={styles.footerLink}>Catálogo</Text></Link>
              <Link href="/(auth)/login"><Text style={styles.footerLink}>Iniciar sesión</Text></Link>
            </View>
          ) : null}
        </View>
      </View>
    </>
  );

  if (isWeb) {
    return (
      <ScrollView style={styles.page} contentContainerStyle={styles.pageContent} showsVerticalScrollIndicator={false}>
        {body}
      </ScrollView>
    );
  }

  return (
    <Screen padded={false}>
      <View style={styles.wrap}>{body}</View>
    </Screen>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: colors.bg },
  pageContent: { flexGrow: 1 },
  wrap: { flex: 1 },
  body: { paddingHorizontal: spacing.lg, paddingBottom: spacing.xxl },
  bodyWeb: { paddingHorizontal: 0, maxWidth: 1200, alignSelf: 'center', width: '100%' },
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
  footerLinks: { flexDirection: 'row', gap: spacing.xl, marginTop: spacing.sm },
  footerLink: { color: colors.primaryLight, fontFamily: fonts.semiBold },
});
