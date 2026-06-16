import { useState } from 'react';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { router } from 'expo-router';
import { authApi } from '@/src/api/authApi';
import { colors } from '@/src/theme/colors';
import { getErrorMessage } from '@/src/utils/apiResponse';

type Mode = 'nuevo' | 'existente';

export default function RegisterScreen() {
  const [mode, setMode] = useState<Mode>('nuevo');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const [form, setForm] = useState({
    username: '',
    correo: '',
    password: '',
    confirmPassword: '',
    nombre: '',
    apellido: '',
    cedula: '',
    telefono: '',
    direccion: '',
    idClienteExistente: '',
  });

  const update = (key: keyof typeof form, value: string) =>
    setForm((prev) => ({ ...prev, [key]: value }));

  const validate = () => {
    if (!form.username.trim() || !form.correo.trim() || !form.password) {
      return 'Usuario, correo y contraseña son obligatorios';
    }
    if (form.password.length < 6) return 'La contraseña debe tener al menos 6 caracteres';
    if (form.password !== form.confirmPassword) return 'Las contraseñas no coinciden';
    if (mode === 'nuevo') {
      if (!form.nombre.trim() || !form.apellido.trim()) return 'Nombre y apellido son obligatorios';
      if (!form.cedula.trim()) return 'La cédula es obligatoria';
    } else if (!form.idClienteExistente.trim()) {
      return 'Ingresa la cédula del cliente existente';
    }
    return '';
  };

  const handleRegister = async () => {
    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }
    setLoading(true);
    setError('');
    try {
      const payload: Record<string, string> = {
        username: form.username.trim(),
        correo: form.correo.trim(),
        password: form.password,
      };
      if (mode === 'nuevo') {
        payload.nombre = form.nombre.trim();
        payload.apellido = form.apellido.trim();
        payload.cedula = form.cedula.trim().toUpperCase();
        if (form.telefono.trim()) payload.telefono = form.telefono.trim();
        if (form.direccion.trim()) payload.direccion = form.direccion.trim();
      } else {
        payload.cedula = form.idClienteExistente.trim().toUpperCase();
      }
      await authApi.register(payload);
      setSuccess(true);
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <View style={styles.container}>
        <Text style={styles.brand}>¡Cuenta creada!</Text>
        <Text style={styles.subtitle}>Ya puedes iniciar sesión con tu usuario y contraseña.</Text>
        <Pressable style={styles.button} onPress={() => router.replace('/login')}>
          <Text style={styles.buttonText}>Ir a login</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={{ flex: 1, backgroundColor: colors.bg }}
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
    >
      <ScrollView contentContainerStyle={styles.scroll} keyboardShouldPersistTaps="handled">
        <Text style={styles.brand}>Crear cuenta</Text>

        <View style={styles.tabs}>
          <Pressable
            style={[styles.tab, mode === 'nuevo' && styles.tabActive]}
            onPress={() => setMode('nuevo')}
          >
            <Text style={mode === 'nuevo' ? styles.tabTextActive : styles.tabText}>Cliente nuevo</Text>
          </Pressable>
          <Pressable
            style={[styles.tab, mode === 'existente' && styles.tabActive]}
            onPress={() => setMode('existente')}
          >
            <Text style={mode === 'existente' ? styles.tabTextActive : styles.tabText}>Ya soy cliente</Text>
          </Pressable>
        </View>

        <TextInput
          style={styles.input}
          placeholder="Usuario"
          placeholderTextColor={colors.textMuted}
          autoCapitalize="none"
          value={form.username}
          onChangeText={(v) => update('username', v)}
        />
        <TextInput
          style={styles.input}
          placeholder="Correo"
          placeholderTextColor={colors.textMuted}
          autoCapitalize="none"
          keyboardType="email-address"
          value={form.correo}
          onChangeText={(v) => update('correo', v)}
        />
        <TextInput
          style={styles.input}
          placeholder="Contraseña"
          placeholderTextColor={colors.textMuted}
          secureTextEntry
          value={form.password}
          onChangeText={(v) => update('password', v)}
        />
        <TextInput
          style={styles.input}
          placeholder="Confirmar contraseña"
          placeholderTextColor={colors.textMuted}
          secureTextEntry
          value={form.confirmPassword}
          onChangeText={(v) => update('confirmPassword', v)}
        />

        {mode === 'nuevo' ? (
          <>
            <TextInput
              style={styles.input}
              placeholder="Nombre"
              placeholderTextColor={colors.textMuted}
              value={form.nombre}
              onChangeText={(v) => update('nombre', v)}
            />
            <TextInput
              style={styles.input}
              placeholder="Apellido"
              placeholderTextColor={colors.textMuted}
              value={form.apellido}
              onChangeText={(v) => update('apellido', v)}
            />
            <TextInput
              style={styles.input}
              placeholder="Cédula"
              placeholderTextColor={colors.textMuted}
              autoCapitalize="characters"
              value={form.cedula}
              onChangeText={(v) => update('cedula', v)}
            />
            <TextInput
              style={styles.input}
              placeholder="Teléfono (opcional)"
              placeholderTextColor={colors.textMuted}
              keyboardType="phone-pad"
              value={form.telefono}
              onChangeText={(v) => update('telefono', v)}
            />
            <TextInput
              style={styles.input}
              placeholder="Dirección (opcional)"
              placeholderTextColor={colors.textMuted}
              value={form.direccion}
              onChangeText={(v) => update('direccion', v)}
            />
          </>
        ) : (
          <TextInput
            style={styles.input}
            placeholder="Cédula del cliente existente"
            placeholderTextColor={colors.textMuted}
            autoCapitalize="characters"
            value={form.idClienteExistente}
            onChangeText={(v) => update('idClienteExistente', v)}
          />
        )}

        {error ? <Text style={styles.error}>{error}</Text> : null}

        <Pressable style={styles.button} onPress={handleRegister} disabled={loading}>
          {loading ? <ActivityIndicator color="#fff" /> : <Text style={styles.buttonText}>Registrarse</Text>}
        </Pressable>

        <Pressable onPress={() => router.back()}>
          <Text style={styles.link}>Ya tengo cuenta</Text>
        </Pressable>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  scroll: { padding: 24, paddingBottom: 48 },
  container: { flex: 1, backgroundColor: colors.bg, padding: 24, justifyContent: 'center' },
  brand: { color: colors.text, fontSize: 28, fontWeight: '800', textAlign: 'center' },
  subtitle: { color: colors.textMuted, textAlign: 'center', marginTop: 12, marginBottom: 24 },
  tabs: { flexDirection: 'row', gap: 8, marginVertical: 16 },
  tab: {
    flex: 1,
    padding: 10,
    borderRadius: 8,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
  },
  tabActive: { borderColor: colors.primary, backgroundColor: colors.surfaceAlt },
  tabText: { color: colors.textMuted, fontWeight: '600', fontSize: 13 },
  tabTextActive: { color: colors.primary, fontWeight: '700', fontSize: 13 },
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
