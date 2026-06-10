import { Pressable, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { resolveApiBaseUrl } from '@/src/config/api';

export default function CuentaScreen() {
  const { isAuthenticated, user, userType, logout } = useAuthStore();

  if (!isAuthenticated) {
    return (
      <View style={styles.container}>
        <Text style={styles.title}>Tu cuenta</Text>
        <Text style={styles.sub}>Accede para gestionar reservas y perfil</Text>
        <Pressable style={styles.button} onPress={() => router.push('/login')}>
          <Text style={styles.buttonText}>Iniciar sesión</Text>
        </Pressable>
        <Pressable style={[styles.button, styles.outline]} onPress={() => router.push('/register')}>
          <Text style={styles.outlineText}>Crear cuenta</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>{user?.nombreCompleto || user?.username}</Text>
      <Text style={styles.sub}>{user?.correo}</Text>
      <Text style={styles.badge}>Tipo: {userType}</Text>
      {userType === 'admin' && (
        <Text style={styles.hint}>
          El panel administrativo completo sigue en la web de Render. La app móvil prioriza booking de cliente.
        </Text>
      )}
      <Pressable style={[styles.button, styles.logout]} onPress={logout}>
        <Text style={styles.buttonText}>Cerrar sesión</Text>
      </Pressable>
      <Text style={styles.api}>API: {resolveApiBaseUrl()}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 24 },
  title: { color: colors.text, fontSize: 24, fontWeight: '700' },
  sub: { color: colors.textMuted, marginTop: 8 },
  badge: { color: colors.accent, marginTop: 16, fontWeight: '600' },
  hint: { color: colors.textMuted, marginTop: 16, lineHeight: 20 },
  button: {
    marginTop: 32,
    backgroundColor: colors.primary,
    padding: 14,
    borderRadius: 10,
    alignItems: 'center',
  },
  logout: { backgroundColor: colors.danger },
  buttonText: { color: '#fff', fontWeight: '600' },
  outline: {
    marginTop: 12,
    backgroundColor: 'transparent',
    borderWidth: 1,
    borderColor: colors.primary,
  },
  outlineText: { color: colors.primary, fontWeight: '600' },
  api: { color: colors.textMuted, fontSize: 11, marginTop: 24 },
});
