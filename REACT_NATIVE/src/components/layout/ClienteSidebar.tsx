import { Ionicons } from '@expo/vector-icons';
import { Link, usePathname, router } from 'expo-router';
import { Alert, Pressable, StyleSheet, Text, View } from 'react-native';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { flatStyle } from '@/src/utils/flatStyle';

type NavItem = {
  href: string;
  label: string;
  icon: keyof typeof Ionicons.glyphMap;
  match: string[];
};

const NAV_ITEMS: NavItem[] = [
  { href: '/cuenta', label: 'Mi Cuenta', icon: 'person-circle-outline', match: ['cuenta'] },
  { href: '/(tabs)/reservas', label: 'Mis Reservas', icon: 'calendar-outline', match: ['reservas'] },
  { href: '/(tabs)/contratos', label: 'Mis Contratos', icon: 'document-text-outline', match: ['contratos'] },
  { href: '/(tabs)/facturas', label: 'Mis Facturas', icon: 'receipt-outline', match: ['facturas'] },
  { href: '/(tabs)/historial', label: 'Historial', icon: 'time-outline', match: ['historial'] },
];

function isActive(pathname: string, match: string[]) {
  return match.some((m) => pathname.includes(m));
}

export function ClienteSidebar() {
  const pathname = usePathname();
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);

  const handleLogout = () => {
    Alert.alert('Cerrar sesión', '¿Seguro que deseas cerrar sesión?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Salir',
        style: 'destructive',
        onPress: async () => {
          await logout();
          router.replace('/');
        },
      },
    ]);
  };

  const initial = (user?.nombreCompleto || user?.username || 'U').charAt(0).toUpperCase();

  return (
    <View style={styles.sidebar}>
      <Link href="/catalogo" asChild>
        <Pressable style={styles.cta}>
          <Ionicons name="car-sport-outline" size={18} color={colors.white} />
          <Text style={styles.ctaText}>Reservar vehículo</Text>
        </Pressable>
      </Link>

      <View style={styles.nav}>
        {NAV_ITEMS.map((item) => {
          const active = isActive(pathname, item.match);
          return (
            <Link key={item.href} href={item.href as never} asChild>
              <Pressable style={flatStyle([styles.link, active ? styles.linkActive : null])}>
                <Ionicons
                  name={item.icon}
                  size={20}
                  color={active ? colors.accent : colors.textSecondary}
                />
                <Text style={flatStyle([styles.linkText, active ? styles.linkTextActive : null])}>{item.label}</Text>
                <Ionicons name="chevron-forward" size={14} color={colors.textMuted} />
              </Pressable>
            </Link>
          );
        })}
      </View>

      <View style={styles.footer}>
        <View style={styles.user}>
          <View style={styles.avatar}>
            <Text style={styles.avatarText}>{initial}</Text>
          </View>
          <View style={styles.userInfo}>
            <Text style={styles.userName} numberOfLines={1}>
              {user?.nombreCompleto || user?.username}
            </Text>
            <Text style={styles.userRole}>Cliente</Text>
          </View>
        </View>
        <Pressable style={styles.logout} onPress={handleLogout}>
          <Ionicons name="log-out-outline" size={18} color={colors.danger} />
          <Text style={styles.logoutText}>Cerrar sesión</Text>
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  sidebar: {
    width: 260,
    backgroundColor: colors.surface,
    borderRightWidth: 1,
    borderRightColor: colors.border,
    paddingVertical: spacing.lg,
    justifyContent: 'space-between',
  },
  cta: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.sm,
    backgroundColor: colors.accent,
    marginHorizontal: spacing.lg,
    paddingVertical: spacing.md,
    borderRadius: radius.md,
    marginBottom: spacing.lg,
  },
  ctaText: { color: colors.white, fontWeight: '700' },
  nav: { flex: 1, paddingHorizontal: spacing.sm },
  link: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.md,
    borderRadius: radius.md,
    marginBottom: 2,
  },
  linkActive: { backgroundColor: colors.clientGhost },
  linkText: { flex: 1, color: colors.textSecondary, fontWeight: '500', fontSize: 15 },
  linkTextActive: { color: colors.text, fontWeight: '700' },
  footer: {
    borderTopWidth: 1,
    borderTopColor: colors.border,
    paddingHorizontal: spacing.lg,
    paddingTop: spacing.lg,
  },
  user: { flexDirection: 'row', alignItems: 'center', gap: spacing.md, marginBottom: spacing.md },
  avatar: {
    width: 40,
    height: 40,
    borderRadius: radius.full,
    backgroundColor: colors.accent,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarText: { color: colors.white, fontWeight: '800', fontSize: 16 },
  userInfo: { flex: 1 },
  userName: { color: colors.text, fontWeight: '600', fontSize: 14 },
  userRole: { color: colors.textMuted, fontSize: 12 },
  logout: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm, paddingVertical: spacing.sm },
  logoutText: { color: colors.danger, fontWeight: '600' },
});
