import { useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { router } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { authApi } from '@/src/api/authApi';
import { Button } from '@/src/components/ui/Button';
import { GradientBackground } from '@/src/components/ui/GradientBackground';
import { Input } from '@/src/components/ui/Input';
import { Screen } from '@/src/components/ui/Screen';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, shadows, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

type Tab = 'admin' | 'cliente';

export default function LoginScreen() {
  const login = useAuthStore((s) => s.login);
  const [tab, setTab] = useState<Tab>('cliente');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleLogin = async () => {
    setError('');
    if (!username.trim() || !password) {
      setError('Usuario y contraseña son requeridos');
      return;
    }
    setLoading(true);
    try {
      const res = await authApi.login({ username: username.trim(), password });
      const body = res.data as { success?: boolean; message?: string };
      if (body?.success === false) {
        setError(body.message || 'Credenciales inválidas');
        return;
      }
      const data = unwrapData<{
        token: string;
        username: string;
        roles?: string[];
        correo?: string;
        nombreCompleto?: string;
      }>(res);
      if (!data?.token) {
        setError('Respuesta de login inválida');
        return;
      }

      const isAdmin = data.roles?.some((r) => ['ADMIN', 'AGENTE', 'AGENTE_POS'].includes(r));
      if (tab === 'admin' && !isAdmin) {
        setError('Este usuario no tiene permisos de administración');
        return;
      }

      const userType = tab === 'admin' && isAdmin ? 'admin' : 'cliente';
      await login({ ...data, token: data.token }, userType);
      router.replace(userType === 'admin' ? '/(admin)' : '/cuenta');
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setLoading(false);
    }
  };

  return (
    <GradientBackground variant="auth">
      <Screen scroll padded={false} style={styles.screen}>
        <KeyboardAvoidingView
          style={styles.flex}
          behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        >
          <View style={styles.hero}>
            <View style={styles.logoCircle}>
              <Ionicons name="car-sport" size={32} color={colors.primaryLight} />
            </View>
            <Text style={styles.brand}>Europcar Rental</Text>
            <Text style={styles.subtitle}>Accede a tu cuenta</Text>
            {tab === 'admin' ? (
              <Text style={styles.demo}>Demo: usuario admin · contraseña 12345</Text>
            ) : null}
          </View>

          <View style={styles.card}>
          <View style={styles.tabs}>
            <Pressable
              style={[styles.tab, tab === 'admin' && styles.tabActiveAdmin]}
              onPress={() => { setTab('admin'); setError(''); }}
            >
              <Text style={[styles.tabText, tab === 'admin' && styles.tabTextActive]}>🛡 Administrador</Text>
            </Pressable>
            <Pressable
              style={[styles.tab, tab === 'cliente' && styles.tabActiveClient]}
              onPress={() => { setTab('cliente'); setError(''); }}
            >
              <Text style={[styles.tabText, tab === 'cliente' && styles.tabTextActiveClient]}>👤 Cliente</Text>
            </Pressable>
          </View>

          <Input
            label="Usuario"
            placeholder="Tu usuario"
            autoCapitalize="none"
            value={username}
            onChangeText={setUsername}
          />
          <Input
            label="Contraseña"
            placeholder="••••••••"
            secureTextEntry
            value={password}
            onChangeText={setPassword}
          />

          {error ? <Text style={styles.error}>{error}</Text> : null}

          <Button
            label={loading ? 'Entrando…' : 'Iniciar sesión'}
            onPress={handleLogin}
            loading={loading}
            variant={tab === 'admin' ? 'primary' : 'client'}
          />

          {tab === 'cliente' ? (
            <Pressable onPress={() => router.push('/register')} style={styles.linkWrap}>
              <Text style={styles.link}>¿No tienes cuenta? Regístrate</Text>
            </Pressable>
          ) : null}

          <Pressable onPress={() => router.back()} style={styles.linkWrap}>
            <Text style={styles.linkMuted}>Continuar sin cuenta</Text>
          </Pressable>
        </View>
        </KeyboardAvoidingView>
      </Screen>
    </GradientBackground>
  );
}

const styles = StyleSheet.create({
  screen: { backgroundColor: 'transparent' },
  flex: { flex: 1 },
  hero: {
    alignItems: 'center',
    paddingTop: spacing.xxl,
    paddingHorizontal: spacing.xl,
    paddingBottom: spacing.lg,
  },
  logoCircle: {
    width: 72,
    height: 72,
    borderRadius: radius.full,
    backgroundColor: 'rgba(13,148,136,0.15)',
    borderWidth: 1,
    borderColor: 'rgba(13,148,136,0.35)',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
    ...shadows.glow,
  },
  brand: { color: colors.text, fontSize: 28, fontFamily: fonts.extraBold },
  subtitle: { color: colors.textSecondary, marginTop: 6, fontFamily: fonts.regular },
  demo: { color: colors.textMuted, fontSize: 12, marginTop: 8, textAlign: 'center', fontFamily: fonts.regular },
  card: {
    flex: 1,
    marginHorizontal: spacing.lg,
    backgroundColor: 'rgba(17,24,39,0.92)',
    borderRadius: radius.xl,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.xl,
    ...shadows.md,
  },
  tabs: {
    flexDirection: 'row',
    backgroundColor: colors.bgSecondary,
    borderRadius: radius.md,
    padding: 4,
    marginBottom: spacing.lg,
  },
  tab: {
    flex: 1,
    paddingVertical: 10,
    borderRadius: radius.sm,
    alignItems: 'center',
  },
  tabActiveAdmin: { backgroundColor: colors.primary },
  tabActiveClient: { backgroundColor: colors.accent },
  tabText: { color: colors.textMuted, fontSize: 13, fontFamily: fonts.semiBold },
  tabTextActive: { color: colors.white },
  tabTextActiveClient: { color: colors.white },
  error: { color: colors.danger, marginBottom: spacing.md, fontSize: 13 },
  linkWrap: { marginTop: spacing.lg, alignItems: 'center' },
  link: { color: colors.accent, fontFamily: fonts.semiBold },
  linkMuted: { color: colors.textMuted },
});
