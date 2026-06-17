import { useState } from 'react';
import { Platform, Pressable, StyleSheet, Text, View, useWindowDimensions } from 'react-native';
import { router } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { authApi } from '@/src/api/authApi';
import { AuthShell } from '@/src/components/ui/AuthShell';
import { Button } from '@/src/components/ui/Button';
import { Input } from '@/src/components/ui/Input';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { flatStyle } from '@/src/utils/flatStyle';
import { getErrorMessage } from '@/src/utils/apiResponse';

type Mode = 'nuevo' | 'existente';

export default function RegisterScreen() {
  const { width } = useWindowDimensions();
  const isWide = Platform.OS === 'web' && width >= 520;

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
    if (form.password.length < 8) return 'La contraseña debe tener al menos 8 caracteres';
    if (form.password !== form.confirmPassword) return 'Las contraseñas no coinciden';
    if (mode === 'nuevo') {
      if (!form.nombre.trim() || !form.apellido.trim()) return 'Nombre y apellido son obligatorios';
      if (!form.cedula.trim()) return 'La cédula es obligatoria';
    } else if (!form.idClienteExistente.trim()) {
      return 'Ingresa la cédula o ID del cliente existente';
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
        correo: form.correo.trim().toLowerCase(),
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
      <AuthShell maxWidth={480}>
        <View style={styles.successWrap}>
          <Ionicons name="checkmark-circle" size={56} color={colors.accent} />
          <Text style={styles.brand}>¡Cuenta creada!</Text>
          <Text style={styles.subtitle}>
            Tu cuenta ha sido registrada. Usuario: <Text style={styles.strong}>{form.username}</Text>
          </Text>
          <Button label="Iniciar sesión" onPress={() => router.replace('/login')} variant="client" />
          <Button label="Explorar catálogo" onPress={() => router.replace('/catalogo')} variant="secondary" style={{ marginTop: spacing.sm }} />
        </View>
      </AuthShell>
    );
  }

  return (
    <AuthShell maxWidth={520}>
      <View style={styles.hero}>
        <View style={styles.logoCircle}>
          <Ionicons name="person-add" size={32} color={colors.accent} />
        </View>
        <Text style={styles.brand}>Crear cuenta</Text>
        <Text style={styles.subtitle}>Regístrate para reservar vehículos</Text>
      </View>

      <View style={styles.tabs}>
        <Pressable
          style={flatStyle([styles.tab, mode === 'nuevo' ? styles.tabActive : null])}
          onPress={() => { setMode('nuevo'); setError(''); }}
        >
          <Text style={flatStyle([styles.tabText, mode === 'nuevo' ? styles.tabTextActive : null])}>Nuevo cliente</Text>
        </Pressable>
        <Pressable
          style={flatStyle([styles.tab, mode === 'existente' ? styles.tabActive : null])}
          onPress={() => { setMode('existente'); setError(''); }}
        >
          <Text style={flatStyle([styles.tabText, mode === 'existente' ? styles.tabTextActive : null])}>Ya soy cliente</Text>
        </Pressable>
      </View>

      {mode === 'existente' ? (
        <Text style={styles.hint}>Si ya eres cliente, ingresa tu cédula o ID para vincular tu cuenta.</Text>
      ) : null}

      {mode === 'nuevo' ? (
        <>
          <View style={flatStyle([styles.row, isWide ? styles.rowWide : null])}>
            <View style={styles.rowField}>
              <Input label="Nombre *" placeholder="Juan" value={form.nombre} onChangeText={(v) => update('nombre', v)} />
            </View>
            <View style={styles.rowField}>
              <Input label="Apellido *" placeholder="Pérez" value={form.apellido} onChangeText={(v) => update('apellido', v)} />
            </View>
          </View>
          <Input
            label="Cédula / Pasaporte *"
            placeholder="1712345678"
            autoCapitalize="characters"
            value={form.cedula}
            onChangeText={(v) => update('cedula', v)}
          />
          <View style={flatStyle([styles.row, isWide ? styles.rowWide : null])}>
            <View style={styles.rowField}>
              <Input
                label="Teléfono"
                placeholder="+593 99 999 9999"
                keyboardType="phone-pad"
                value={form.telefono}
                onChangeText={(v) => update('telefono', v)}
              />
            </View>
            <View style={styles.rowField}>
              <Input label="Dirección" placeholder="Av. Principal 123" value={form.direccion} onChangeText={(v) => update('direccion', v)} />
            </View>
          </View>
          <View style={styles.divider} />
        </>
      ) : (
        <Input
          label="Cédula o ID de cliente *"
          placeholder="1712345678 o CLT-001"
          autoCapitalize="characters"
          value={form.idClienteExistente}
          onChangeText={(v) => update('idClienteExistente', v)}
        />
      )}

      <Input
        label="Usuario *"
        placeholder="juan.perez"
        autoCapitalize="none"
        value={form.username}
        onChangeText={(v) => update('username', v)}
      />
      <Input
        label="Correo *"
        placeholder="juan@ejemplo.com"
        autoCapitalize="none"
        keyboardType="email-address"
        value={form.correo}
        onChangeText={(v) => update('correo', v)}
      />
      <Input
        label="Contraseña *"
        placeholder="8+ caracteres"
        secureTextEntry
        value={form.password}
        onChangeText={(v) => update('password', v)}
      />
      <Input
        label="Confirmar contraseña *"
        placeholder="Repite tu contraseña"
        secureTextEntry
        value={form.confirmPassword}
        onChangeText={(v) => update('confirmPassword', v)}
      />

      {error ? <Text style={styles.error}>{error}</Text> : null}

      <Button label={loading ? 'Creando cuenta…' : 'Crear cuenta'} onPress={handleRegister} loading={loading} variant="client" />

      <Pressable onPress={() => router.replace('/login')} style={styles.linkWrap}>
        <Text style={styles.link}>¿Ya tienes cuenta? Inicia sesión</Text>
      </Pressable>
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
    backgroundColor: colors.clientGhost,
    borderWidth: 1,
    borderColor: 'rgba(59,130,246,0.35)',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
  },
  brand: { color: colors.text, fontSize: 24, fontFamily: fonts.extraBold, textAlign: 'center' },
  subtitle: { color: colors.textSecondary, marginTop: 6, fontFamily: fonts.regular, textAlign: 'center', lineHeight: 22 },
  strong: { color: colors.text, fontFamily: fonts.bold },
  tabs: {
    flexDirection: 'row',
    backgroundColor: colors.bgSecondary,
    borderRadius: radius.md,
    padding: 4,
    marginBottom: spacing.md,
  },
  tab: { flex: 1, paddingVertical: 10, borderRadius: radius.sm, alignItems: 'center' },
  tabActive: { backgroundColor: colors.accent },
  tabText: { color: colors.textMuted, fontSize: 13, fontFamily: fonts.semiBold },
  tabTextActive: { color: colors.white },
  hint: { color: colors.textMuted, fontSize: 13, marginBottom: spacing.md, lineHeight: 20, fontFamily: fonts.regular },
  row: { gap: spacing.sm },
  rowWide: { flexDirection: 'row' },
  rowField: { flex: 1 },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.md,
  },
  error: { color: colors.danger, marginBottom: spacing.md, fontSize: 13 },
  linkWrap: { marginTop: spacing.md, alignItems: 'center' },
  link: { color: colors.accent, fontFamily: fonts.semiBold },
  linkMuted: { color: colors.textMuted, fontFamily: fonts.regular },
  successWrap: { alignItems: 'center', gap: spacing.md },
});
