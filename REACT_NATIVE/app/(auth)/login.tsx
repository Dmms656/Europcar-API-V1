import { useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { authApi } from '@/src/api/authApi';
import { AuthShell } from '@/src/components/ui/AuthShell';
import { Button } from '@/src/components/ui/Button';
import { Input } from '@/src/components/ui/Input';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { flatStyle } from '@/src/utils/flatStyle';
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
        idCliente?: number;
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
      router.replace(userType === 'admin' ? '/(admin)' : '/mi-cuenta');
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthShell>
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

      <View style={styles.tabs}>
        <Pressable
          style={flatStyle([styles.tab, tab === 'admin' ? styles.tabActiveAdmin : null])}
          onPress={() => { setTab('admin'); setError(''); }}
        >
          <Text style={flatStyle([styles.tabText, tab === 'admin' ? styles.tabTextActive : null])}>🛡 Administrador</Text>
        </Pressable>
        <Pressable
          style={flatStyle([styles.tab, tab === 'cliente' ? styles.tabActiveClient : null])}
          onPress={() => { setTab('cliente'); setError(''); }}
        >
          <Text style={flatStyle([styles.tabText, tab === 'cliente' ? styles.tabTextActiveClient : null])}>👤 Cliente</Text>
        </Pressable>
      </View>

      <Input
        label={tab === 'admin' ? 'Usuario' : 'Usuario / Correo'}
        placeholder={tab === 'admin' ? 'admin' : 'cliente.carlos'}
        autoCapitalize="none"
        value={username}
        onChangeText={setUsername}
      />
      <Input
        label="Contraseña"
        placeholder="Ingrese su contraseña"
        secureTextEntry
        value={password}
        onChangeText={setPassword}
      />

      {error ? <Text style={styles.error}>{error}</Text> : null}

      <Button
        label={loading ? 'Entrando…' : tab === 'admin' ? 'Acceder al Panel' : 'Acceder a Mi Cuenta'}
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
        <Text style={styles.linkMuted}>← Volver al inicio</Text>
      </Pressable>
    </AuthShell>
  );
}

const styles = StyleSheet.create({
  hero: { alignItems: 'center', marginBottom: spacing.lg },
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
  },
  brand: { color: colors.text, fontSize: 26, fontFamily: fonts.extraBold },
  subtitle: { color: colors.textSecondary, marginTop: 6, fontFamily: fonts.regular, textAlign: 'center' },
  demo: { color: colors.textMuted, fontSize: 12, marginTop: 8, textAlign: 'center', fontFamily: fonts.regular },
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
  linkMuted: { color: colors.textMuted, fontFamily: fonts.regular },
});
