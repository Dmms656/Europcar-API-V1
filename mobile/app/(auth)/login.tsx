import { useState } from 'react';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { router } from 'expo-router';
import { authApi } from '@/src/api/authApi';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

export default function LoginScreen() {
  const login = useAuthStore((s) => s.login);
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
      await login({ ...data, token: data.token }, isAdmin ? 'admin' : 'cliente');
      router.replace('/(tabs)');
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <Text style={styles.brand}>Europcar</Text>
      <Text style={styles.subtitle}>Inicia sesión con tu cuenta</Text>

      <TextInput
        style={styles.input}
        placeholder="Usuario"
        placeholderTextColor={colors.textMuted}
        autoCapitalize="none"
        value={username}
        onChangeText={setUsername}
      />
      <TextInput
        style={styles.input}
        placeholder="Contraseña"
        placeholderTextColor={colors.textMuted}
        secureTextEntry
        value={password}
        onChangeText={setPassword}
      />

      {error ? <Text style={styles.error}>{error}</Text> : null}

      <Pressable style={styles.button} onPress={handleLogin} disabled={loading}>
        {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>Entrar</Text>}
      </Pressable>

      <Pressable onPress={() => router.push('/register')}>
        <Text style={styles.link}>Crear cuenta</Text>
      </Pressable>

      <Pressable onPress={() => router.back()}>
        <Text style={[styles.link, { marginTop: 12 }]}>Continuar sin cuenta</Text>
      </Pressable>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg, padding: 24, justifyContent: 'center' },
  brand: { color: colors.text, fontSize: 32, fontWeight: '800', textAlign: 'center' },
  subtitle: { color: colors.textMuted, textAlign: 'center', marginBottom: 32, marginTop: 8 },
  input: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderWidth: 1,
    borderRadius: 10,
    padding: 14,
    color: colors.text,
    marginBottom: 12,
  },
  button: {
    backgroundColor: colors.primary,
    borderRadius: 10,
    padding: 16,
    alignItems: 'center',
    marginTop: 8,
  },
  buttonText: { color: '#fff', fontWeight: '700', fontSize: 16 },
  error: { color: colors.danger, marginBottom: 8 },
  link: { color: colors.primary, textAlign: 'center', marginTop: 20 },
});
