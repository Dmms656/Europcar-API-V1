import { StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { ProfileHeader } from '@/src/components/ProfileHeader';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { Screen } from '@/src/components/ui/Screen';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';

export default function AdminCuentaScreen() {
  const { user, logout } = useAuthStore();

  const handleLogout = async () => {
    await logout();
    router.replace('/(tabs)');
  };

  return (
    <Screen>
      <ProfileHeader
        variant="admin"
        name={user?.nombreCompleto || user?.username || 'Administrador'}
        subtitle={user?.correo || 'Sin correo'}
        badge={user?.roles?.[0] || 'ADMIN'}
      />

      <Card>
        <Text style={styles.sectionTitle}>Acceso administrativo</Text>
        <Text style={styles.text}>
          Gestiona reservas, clientes y vehículos desde las pestañas inferiores. Este panel replica la
          experiencia del dashboard web de Europcar.
        </Text>
      </Card>

      <Card>
        <Text style={styles.label}>Usuario</Text>
        <Text style={styles.value}>{user?.username ?? '—'}</Text>
        <Text style={[styles.label, { marginTop: spacing.md }]}>Roles</Text>
        <Text style={styles.value}>{user?.roles?.join(', ') ?? 'ADMIN'}</Text>
      </Card>

      <Button label="Cerrar sesión" variant="danger" onPress={handleLogout} style={{ marginTop: spacing.lg }} />
    </Screen>
  );
}

const styles = StyleSheet.create({
  sectionTitle: { color: colors.text, fontWeight: '700', fontSize: 16, marginBottom: 8 },
  text: { color: colors.textSecondary, lineHeight: 22 },
  label: { color: colors.textMuted, fontSize: 12, fontWeight: '600' },
  value: { color: colors.text, fontSize: 15, marginTop: 4 },
});
