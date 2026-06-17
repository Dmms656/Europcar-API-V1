import { Link, usePathname } from 'expo-router';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { spacing, radius, shadows } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';

const NAV_LINKS = [
  { href: '/(tabs)/' as const, label: 'Inicio', match: ['/', '/(tabs)', '/(tabs)/index'] },
  { href: '/(tabs)/buscar' as const, label: 'Buscar', match: ['/(tabs)/buscar'] },
  { href: '/(tabs)/catalogo' as const, label: 'Catálogo', match: ['/(tabs)/catalogo'] },
] as const;

function isActive(pathname: string, match: readonly string[]) {
  return match.some((m) => pathname === m || pathname.startsWith(m + '/'));
}

/** Barra superior para web — oculta en móvil nativo (usa tabs). */
export function PublicNavbar() {
  const pathname = usePathname();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const userType = useAuthStore((s) => s.userType);

  const accountHref =
    userType === 'admin' ? '/(admin)/cuenta' : '/(tabs)/cuenta';

  return (
    <View style={styles.bar}>
      <View style={styles.inner}>
        <View style={styles.brandRow}>
          <Ionicons name="car-sport" size={22} color={colors.primaryLight} />
          <Text style={styles.brand}>Europcar</Text>
        </View>

        <View style={styles.links}>
          {NAV_LINKS.map((link) => (
            <Link key={link.href} href={link.href} asChild>
              <Pressable style={styles.linkBtn}>
                <Text
                  style={[
                    styles.linkText,
                    isActive(pathname, link.match) && styles.linkTextActive,
                  ]}
                >
                  {link.label}
                </Text>
              </Pressable>
            </Link>
          ))}
        </View>

        <View style={styles.actions}>
          {isAuthenticated ? (
            <Link href={accountHref} asChild>
              <Pressable style={styles.primaryBtn}>
                <Text style={styles.primaryBtnText}>Mi cuenta</Text>
              </Pressable>
            </Link>
          ) : (
            <>
              <Link href="/(auth)/login" asChild>
                <Pressable style={styles.ghostBtn}>
                  <Text style={styles.ghostBtnText}>Iniciar sesión</Text>
                </Pressable>
              </Link>
              <Link href="/(auth)/register" asChild>
                <Pressable style={styles.primaryBtn}>
                  <Text style={styles.primaryBtnText}>Registrarse</Text>
                </Pressable>
              </Link>
            </>
          )}
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  bar: {
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    paddingHorizontal: spacing.xl,
    paddingVertical: spacing.md,
    ...shadows.sm,
  },
  inner: {
    flexDirection: 'row',
    alignItems: 'center',
    maxWidth: 1280,
    width: '100%',
    alignSelf: 'center',
    gap: spacing.xl,
  },
  brandRow: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm },
  brand: {
    color: colors.primaryLight,
    fontSize: 20,
    fontFamily: fonts.extraBold,
    letterSpacing: 0.5,
  },
  links: {
    flex: 1,
    flexDirection: 'row',
    gap: spacing.lg,
  },
  linkBtn: {
    paddingVertical: spacing.sm,
    paddingHorizontal: spacing.md,
  },
  linkText: {
    color: colors.textSecondary,
    fontSize: 15,
    fontFamily: fonts.medium,
  },
  linkTextActive: {
    color: colors.text,
    fontFamily: fonts.bold,
  },
  actions: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  ghostBtn: {
    paddingVertical: spacing.sm,
    paddingHorizontal: spacing.lg,
    borderRadius: radius.md,
  },
  ghostBtnText: {
    color: colors.textSecondary,
    fontFamily: fonts.semiBold,
  },
  primaryBtn: {
    backgroundColor: colors.primary,
    paddingVertical: spacing.sm,
    paddingHorizontal: spacing.lg,
    borderRadius: radius.md,
  },
  primaryBtnText: {
    color: colors.white,
    fontFamily: fonts.bold,
  },
});
