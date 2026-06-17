import { Link, usePathname, router } from 'expo-router';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { useAuthStore } from '@/src/store/useAuthStore';
import { confirmAction } from '@/src/utils/confirm';
import { colors } from '@/src/theme/colors';
import { spacing, radius } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';

const NAV_LINKS = [
  { href: '/' as const, label: 'Inicio', match: ['/', '/(tabs)', '/(tabs)/index'] },
  { href: '/catalogo' as const, label: 'Catálogo', match: ['/catalogo', '/(tabs)/catalogo'] },
] as const;

function isActive(pathname: string, match: readonly string[]) {
  return match.some((m) => pathname === m || pathname.startsWith(m + '/') || pathname.startsWith(m + '?'));
}

/** Barra superior web — estilo marketing del frontend Vite. */
export function PublicNavbar() {
  const pathname = usePathname();
  const { isDesktop } = useBreakpoint();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const userType = useAuthStore((s) => s.userType);
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);

  const accountHref =
    userType === 'admin' ? '/(admin)' : '/mi-cuenta';

  const handleLogout = async () => {
    const ok = await confirmAction('Cerrar sesión', '¿Seguro que deseas cerrar sesión?');
    if (!ok) return;
    await logout();
    router.replace('/');
  };

  return (
    <View style={styles.bar}>
      <View style={styles.inner}>
        <Link href="/" asChild>
          <Pressable style={styles.brandRow}>
            <Ionicons name="car-sport" size={22} color={colors.primaryLight} />
            <Text style={styles.brand}>Europcar</Text>
          </Pressable>
        </Link>

        <View style={styles.links}>
          {NAV_LINKS.map((link) => (
            <Link key={link.href} href={link.href} asChild>
              <Pressable style={StyleSheet.flatten([styles.linkBtn, isActive(pathname, link.match) ? styles.linkBtnActive : null])}>
                <Text
                  style={StyleSheet.flatten([styles.linkText, isActive(pathname, link.match) ? styles.linkTextActive : null])}
                >
                  {link.label}
                </Text>
              </Pressable>
            </Link>
          ))}
          {isAuthenticated && userType === 'admin' ? (
            <Link href="/(admin)" asChild>
              <Pressable style={styles.linkBtn}>
                <Text style={styles.linkText}>Admin</Text>
              </Pressable>
            </Link>
          ) : null}
        </View>

        <View style={styles.actions}>
          {isAuthenticated ? (
            <>
              {user?.username && isDesktop ? (
                <Text style={styles.username}>{user.username}</Text>
              ) : null}
              <Link href={accountHref} asChild>
                <Pressable style={styles.primaryBtn}>
                  <Text style={styles.primaryBtnText}>
                    {userType === 'admin' ? 'Panel' : 'Mi cuenta'}
                  </Text>
                </Pressable>
              </Link>
              <Pressable style={styles.iconBtn} onPress={handleLogout} accessibilityLabel="Cerrar sesión">
                <Ionicons name="log-out-outline" size={20} color={colors.textSecondary} />
              </Pressable>
            </>
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
    backgroundColor: 'rgba(10,14,23,0.92)',
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    paddingHorizontal: spacing.xl,
    paddingVertical: spacing.md,
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
    gap: spacing.sm,
  },
  linkBtn: {
    paddingVertical: spacing.sm,
    paddingHorizontal: spacing.md,
    borderRadius: radius.full,
  },
  linkBtnActive: {
    backgroundColor: colors.primaryGhost,
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
  username: {
    color: colors.textSecondary,
    fontFamily: fonts.medium,
    fontSize: 14,
  },
  ghostBtn: {
    paddingVertical: spacing.sm,
    paddingHorizontal: spacing.lg,
    borderRadius: radius.full,
  },
  ghostBtnText: {
    color: colors.textSecondary,
    fontFamily: fonts.semiBold,
  },
  primaryBtn: {
    backgroundColor: colors.primary,
    paddingVertical: spacing.sm,
    paddingHorizontal: spacing.lg,
    borderRadius: radius.full,
  },
  primaryBtnText: {
    color: colors.white,
    fontFamily: fonts.bold,
  },
  iconBtn: {
    padding: spacing.sm,
    borderRadius: radius.md,
  },
});
