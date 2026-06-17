import { Ionicons } from '@expo/vector-icons';
import { Link, usePathname } from 'expo-router';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { flatStyle } from '@/src/utils/flatStyle';
import { confirmAction } from '@/src/utils/confirm';
import { logoutAndGoHome } from '@/src/utils/authActions';

type NavItem = {
  href: string;
  label: string;
  icon: keyof typeof Ionicons.glyphMap;
  match: string[];
  roles: string[];
};

const NAV_ITEMS: NavItem[] = [
  { href: '/(admin)/', label: 'Dashboard', icon: 'grid-outline', match: ['index'], roles: [] },
  { href: '/(admin)/clientes', label: 'Clientes', icon: 'people-outline', match: ['clientes'], roles: ['ADMIN', 'AGENTE_POS'] },
  { href: '/(admin)/vehiculos', label: 'Vehículos', icon: 'car-outline', match: ['vehiculos'], roles: [] },
  { href: '/(admin)/reservas', label: 'Reservas', icon: 'calendar-outline', match: ['reservas'], roles: [] },
  { href: '/(admin)/contratos', label: 'Contratos', icon: 'document-text-outline', match: ['contratos'], roles: ['ADMIN', 'AGENTE_POS'] },
  { href: '/(admin)/pagos', label: 'Pagos', icon: 'card-outline', match: ['pagos'], roles: ['ADMIN', 'AGENTE_POS'] },
  { href: '/(admin)/mantenimientos', label: 'Mantenimientos', icon: 'construct-outline', match: ['mantenimientos'], roles: ['ADMIN', 'AGENTE_POS'] },
  { href: '/(admin)/localizaciones', label: 'Localizaciones', icon: 'location-outline', match: ['localizaciones'], roles: ['ADMIN', 'AGENTE_POS'] },
  { href: '/(admin)/ubicaciones', label: 'Países y Ciudades', icon: 'earth-outline', match: ['ubicaciones'], roles: ['ADMIN', 'AGENTE_POS'] },
  { href: '/(admin)/extras', label: 'Extras', icon: 'cube-outline', match: ['extras'], roles: ['ADMIN', 'AGENTE_POS'] },
  { href: '/(admin)/usuarios', label: 'Usuarios', icon: 'shield-checkmark-outline', match: ['usuarios'], roles: ['ADMIN'] },
];

function isActive(pathname: string, match: string[]) {
  return match.some((m) => pathname.includes(m));
}

export function AdminSidebar() {
  const pathname = usePathname();
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const hasAnyRole = useAuthStore((s) => s.hasAnyRole);

  const visible = NAV_ITEMS.filter((item) => item.roles.length === 0 || hasAnyRole(...item.roles));

  const handleLogout = async () => {
    const ok = await confirmAction('Cerrar sesión', '¿Seguro que deseas cerrar sesión?');
    if (!ok) return;
    await logoutAndGoHome(logout);
  };

  const initial = (user?.username || 'A').charAt(0).toUpperCase();

  return (
    <View style={styles.sidebar}>
      <Text style={styles.brand}>Europcar Admin</Text>
      <ScrollView style={styles.nav} showsVerticalScrollIndicator={false}>
        {visible.map((item) => {
          const active = isActive(pathname, item.match);
          return (
            <Link key={item.href} href={item.href as never} asChild>
              <Pressable style={flatStyle([styles.link, active ? styles.linkActive : null])}>
                <Ionicons name={item.icon} size={20} color={active ? colors.primaryLight : colors.textSecondary} />
                <Text style={flatStyle([styles.linkText, active ? styles.linkTextActive : null])}>{item.label}</Text>
              </Pressable>
            </Link>
          );
        })}
      </ScrollView>
      <View style={styles.footer}>
        <View style={styles.user}>
          <View style={styles.avatar}>
            <Text style={styles.avatarText}>{initial}</Text>
          </View>
          <View style={styles.userInfo}>
            <Text style={styles.userName} numberOfLines={1}>{user?.username}</Text>
            <Text style={styles.userRole}>{user?.roles?.[0] || 'Admin'}</Text>
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
  },
  brand: {
    color: colors.primaryLight,
    fontSize: 18,
    fontWeight: '800',
    paddingHorizontal: spacing.lg,
    marginBottom: spacing.lg,
  },
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
  linkActive: { backgroundColor: colors.primaryGhost },
  linkText: { color: colors.textSecondary, fontWeight: '500', fontSize: 14, flex: 1 },
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
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarText: { color: colors.white, fontWeight: '800' },
  userInfo: { flex: 1 },
  userName: { color: colors.text, fontWeight: '600', fontSize: 14 },
  userRole: { color: colors.textMuted, fontSize: 12 },
  logout: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm, paddingVertical: spacing.sm },
  logoutText: { color: colors.danger, fontWeight: '600' },
});
