import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { Screen } from '@/src/components/ui/Screen';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';

type Module = {
  route: string;
  label: string;
  icon: keyof typeof Ionicons.glyphMap;
  roles: string[];
};

const MODULES: Module[] = [
  { route: '/(admin)/contratos', label: 'Contratos', icon: 'document-text-outline', roles: ['ADMIN', 'AGENTE_POS'] },
  { route: '/(admin)/pagos', label: 'Pagos', icon: 'card-outline', roles: ['ADMIN', 'AGENTE_POS'] },
  { route: '/(admin)/mantenimientos', label: 'Mantenimientos', icon: 'construct-outline', roles: ['ADMIN', 'AGENTE_POS'] },
  { route: '/(admin)/localizaciones', label: 'Localizaciones', icon: 'location-outline', roles: ['ADMIN', 'AGENTE_POS'] },
  { route: '/(admin)/ubicaciones', label: 'Países y Ciudades', icon: 'earth-outline', roles: ['ADMIN', 'AGENTE_POS'] },
  { route: '/(admin)/extras', label: 'Extras', icon: 'cube-outline', roles: ['ADMIN', 'AGENTE_POS'] },
  { route: '/(admin)/usuarios', label: 'Usuarios', icon: 'shield-checkmark-outline', roles: ['ADMIN'] },
  { route: '/(admin)/perfil', label: 'Mi cuenta admin', icon: 'person-circle-outline', roles: [] },
];

export default function AdminMasScreen() {
  const hasAnyRole = useAuthStore((s) => s.hasAnyRole);
  const visible = MODULES.filter((m) => m.roles.length === 0 || hasAnyRole(...m.roles));

  return (
    <Screen>
      <Text style={styles.title}>Módulos admin</Text>
      <Text style={styles.sub}>Acceso a todas las secciones del panel</Text>
      <ScrollView>
        {visible.map((m) => (
          <Pressable key={m.route} style={styles.item} onPress={() => router.push(m.route as never)}>
            <Ionicons name={m.icon} size={22} color={colors.primaryLight} />
            <Text style={styles.itemText}>{m.label}</Text>
            <Ionicons name="chevron-forward" size={18} color={colors.textMuted} />
          </Pressable>
        ))}
      </ScrollView>
    </Screen>
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: 22, fontWeight: '800' },
  sub: { color: colors.textMuted, marginBottom: spacing.lg, marginTop: 4 },
  item: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    backgroundColor: colors.surface,
    padding: spacing.lg,
    borderRadius: radius.md,
    marginBottom: spacing.sm,
    borderWidth: 1,
    borderColor: colors.border,
  },
  itemText: { flex: 1, color: colors.text, fontWeight: '600', fontSize: 15 },
});
