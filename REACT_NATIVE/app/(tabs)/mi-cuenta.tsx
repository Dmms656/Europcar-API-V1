import { useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { authApi } from '@/src/api/authApi';
import { ProfileHeader } from '@/src/components/ProfileHeader';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { Input } from '@/src/components/ui/Input';
import { Screen } from '@/src/components/ui/Screen';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage } from '@/src/utils/apiResponse';
import { validators } from '@/src/utils/validation';
import { confirmAction, alertMessage } from '@/src/utils/confirm';
import { logoutAndGoHome } from '@/src/utils/authActions';

export default function CuentaScreen() {
  const { isAuthenticated, user, userType, logout, refreshProfile } = useAuthStore();
  const [editing, setEditing] = useState(false);
  const [changingPassword, setChangingPassword] = useState(false);
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    correo: user?.correo ?? '',
    telefono: user?.telefono ?? '',
    direccion: user?.direccion ?? '',
  });

  const [pwdForm, setPwdForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

  const { isWeb } = useBreakpoint();

  if (!isAuthenticated) {
    return (
      <Screen scroll={false} style={styles.guest}>
        <Text style={styles.guestTitle}>Mi cuenta</Text>
        <Text style={styles.guestSub}>Accede para gestionar reservas y tu perfil</Text>
        <Button label="Iniciar sesión" onPress={() => router.push('/login')} variant="client" />
        <Button
          label="Crear cuenta"
          onPress={() => router.push('/register')}
          variant="ghost"
          style={{ marginTop: spacing.md }}
        />
      </Screen>
    );
  }

  const handleSave = async () => {
    const errs: Record<string, string> = {};
    const emailErr = validators.required(form.correo, 'El correo') || validators.email(form.correo);
    if (emailErr) errs.correo = emailErr;
    if (form.telefono) {
      const ph = validators.phone(form.telefono);
      if (ph) errs.telefono = ph;
    }
    if (Object.keys(errs).length > 0) {
      setFieldErrors(errs);
      void alertMessage('Error', Object.values(errs)[0]);
      return;
    }
    setFieldErrors({});
    setSaving(true);
    try {
      await authApi.updateProfile({
        correo: form.correo.trim(),
        telefono: form.telefono.trim() || undefined,
        direccion: form.direccion.trim() || undefined,
      });
      await refreshProfile();
      setEditing(false);
      void alertMessage('Listo', 'Datos actualizados correctamente');
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const handlePasswordChange = async () => {
    const errs: Record<string, string> = {};
    if (!pwdForm.currentPassword) errs.currentPassword = 'Contraseña actual requerida';
    const newPwdErr =
      validators.required(pwdForm.newPassword, 'Nueva contraseña') ||
      validators.minLength(pwdForm.newPassword, 6, 'Nueva contraseña');
    if (newPwdErr) errs.newPassword = newPwdErr;
    const matchErr = validators.match(pwdForm.confirmPassword, pwdForm.newPassword, 'Las contraseñas');
    if (matchErr) errs.confirmPassword = matchErr;
    if (Object.keys(errs).length > 0) {
      setFieldErrors(errs);
      void alertMessage('Error', Object.values(errs)[0]);
      return;
    }
    setFieldErrors({});
    setSaving(true);
    try {
      await authApi.changePassword({
        currentPassword: pwdForm.currentPassword,
        newPassword: pwdForm.newPassword,
      });
      setChangingPassword(false);
      setPwdForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      void alertMessage('Listo', 'Contraseña actualizada');
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const displayName = user?.nombreCompleto || user?.username || 'Usuario';
  const roleBadge = user?.roles?.[0] || 'CLIENTE';

  return (
    <Screen>
      <ProfileHeader
        variant="cliente"
        name={displayName}
        subtitle={user?.correo || 'correo@ejemplo.com'}
        badge={roleBadge}
      />

      <Card>
        <View style={styles.cardHeader}>
          <Text style={styles.cardTitle}>Información personal</Text>
          {!editing ? (
            <Pressable onPress={() => {
              setForm({
                correo: user?.correo ?? '',
                telefono: user?.telefono ?? '',
                direccion: user?.direccion ?? '',
              });
              setEditing(true);
            }}>
              <Text style={styles.editLink}>Editar</Text>
            </Pressable>
          ) : null}
        </View>

        <Text style={styles.fieldLabel}>Nombre</Text>
        <Text style={styles.fieldValue}>{displayName}</Text>

        {editing ? (
          <>
            <Input
              label="Correo"
              value={form.correo}
              onChangeText={(v) => { setForm({ ...form, correo: v }); setFieldErrors((e) => ({ ...e, correo: '' })); }}
              keyboardType="email-address"
              autoCapitalize="none"
              error={fieldErrors.correo}
            />
            <Input
              label="Teléfono"
              value={form.telefono}
              onChangeText={(v) => { setForm({ ...form, telefono: v }); setFieldErrors((e) => ({ ...e, telefono: '' })); }}
              keyboardType="phone-pad"
              error={fieldErrors.telefono}
            />
            <Input label="Dirección" value={form.direccion} onChangeText={(v) => setForm({ ...form, direccion: v })} />
            <View style={styles.row}>
              <Button label="Guardar" onPress={handleSave} loading={saving} variant="client" style={{ flex: 1 }} />
              <Button label="Cancelar" onPress={() => setEditing(false)} variant="secondary" style={{ flex: 1 }} />
            </View>
          </>
        ) : (
          <>
            <Text style={styles.fieldLabel}>Correo</Text>
            <Text style={styles.fieldValue}>{user?.correo || '—'}</Text>
            <Text style={styles.fieldLabel}>Teléfono</Text>
            <Text style={styles.fieldValue}>{user?.telefono || '—'}</Text>
            <Text style={styles.fieldLabel}>Dirección</Text>
            <Text style={styles.fieldValue}>{user?.direccion || '—'}</Text>
          </>
        )}
      </Card>

      <Card>
        <View style={styles.cardHeader}>
          <Text style={styles.cardTitle}>Seguridad</Text>
          <Pressable onPress={() => setChangingPassword(!changingPassword)}>
            <Text style={styles.editLink}>{changingPassword ? 'Ocultar' : 'Cambiar contraseña'}</Text>
          </Pressable>
        </View>

        {changingPassword ? (
          <>
            <Input
              label="Contraseña actual"
              secureTextEntry
              value={pwdForm.currentPassword}
              onChangeText={(v) => { setPwdForm({ ...pwdForm, currentPassword: v }); setFieldErrors((e) => ({ ...e, currentPassword: '' })); }}
              error={fieldErrors.currentPassword}
            />
            <Input
              label="Nueva contraseña"
              secureTextEntry
              value={pwdForm.newPassword}
              onChangeText={(v) => { setPwdForm({ ...pwdForm, newPassword: v }); setFieldErrors((e) => ({ ...e, newPassword: '' })); }}
              error={fieldErrors.newPassword}
            />
            <Input
              label="Confirmar"
              secureTextEntry
              value={pwdForm.confirmPassword}
              onChangeText={(v) => { setPwdForm({ ...pwdForm, confirmPassword: v }); setFieldErrors((e) => ({ ...e, confirmPassword: '' })); }}
              error={fieldErrors.confirmPassword}
            />
            <Button label="Actualizar contraseña" onPress={handlePasswordChange} loading={saving} />
          </>
        ) : (
          <Text style={styles.hint}>Protege tu cuenta con una contraseña segura.</Text>
        )}
      </Card>

      {!isWeb && userType === 'cliente' ? (
        <Card>
          <Text style={styles.cardTitle}>Accesos rápidos</Text>
          <View style={styles.quickLinks}>
            <QuickLink icon="document-text-outline" label="Mis contratos" onPress={() => router.push('/mis-contratos')} />
            <QuickLink icon="receipt-outline" label="Mis facturas" onPress={() => router.push('/mis-facturas')} />
            <QuickLink icon="calendar-outline" label="Mis reservas" onPress={() => router.push('/mis-reservas')} />
          </View>
        </Card>
      ) : null}

      {userType === 'admin' ? (
        <Button label="Ir al panel admin" onPress={() => router.replace('/(admin)')} style={{ marginBottom: spacing.md }} />
      ) : null}

      <Button
        label="Cerrar sesión"
        variant="danger"
        onPress={async () => {
          const ok = await confirmAction('Cerrar sesión', '¿Seguro que deseas cerrar sesión?');
          if (!ok) return;
          await logoutAndGoHome(logout);
        }}
      />
    </Screen>
  );
}

function QuickLink({
  icon,
  label,
  onPress,
}: {
  icon: keyof typeof Ionicons.glyphMap;
  label: string;
  onPress: () => void;
}) {
  return (
    <Pressable style={styles.quickLink} onPress={onPress}>
      <Ionicons name={icon} size={20} color={colors.accent} />
      <Text style={styles.quickLinkText}>{label}</Text>
      <Ionicons name="chevron-forward" size={16} color={colors.textMuted} />
    </Pressable>
  );
}

const styles = StyleSheet.create({
  guest: { justifyContent: 'center' },
  guestTitle: { color: colors.text, fontSize: 26, fontWeight: '800', textAlign: 'center' },
  guestSub: { color: colors.textSecondary, textAlign: 'center', marginVertical: spacing.lg },
  cardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: spacing.md },
  cardTitle: { color: colors.text, fontWeight: '700', fontSize: 16 },
  editLink: { color: colors.accent, fontWeight: '600' },
  fieldLabel: { color: colors.textMuted, fontSize: 12, fontWeight: '600', marginTop: spacing.sm },
  fieldValue: { color: colors.text, fontSize: 15, marginTop: 4 },
  row: { flexDirection: 'row', gap: spacing.sm, marginTop: spacing.sm },
  hint: { color: colors.textSecondary, lineHeight: 20 },
  quickLinks: { marginTop: spacing.md, gap: spacing.xs },
  quickLink: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    paddingVertical: spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  quickLinkText: { flex: 1, color: colors.text, fontWeight: '600' },
});
