import { useState } from 'react';
import { Alert, Pressable, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { authApi } from '@/src/api/authApi';
import { ProfileHeader } from '@/src/components/ProfileHeader';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { Input } from '@/src/components/ui/Input';
import { Screen } from '@/src/components/ui/Screen';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage } from '@/src/utils/apiResponse';

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
    if (!form.correo.trim()) {
      Alert.alert('Error', 'El correo es requerido');
      return;
    }
    setSaving(true);
    try {
      await authApi.updateProfile({
        correo: form.correo.trim(),
        telefono: form.telefono.trim() || undefined,
        direccion: form.direccion.trim() || undefined,
      });
      await refreshProfile();
      setEditing(false);
      Alert.alert('Listo', 'Datos actualizados correctamente');
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const handlePasswordChange = async () => {
    if (!pwdForm.currentPassword || !pwdForm.newPassword) {
      Alert.alert('Error', 'Completa todos los campos');
      return;
    }
    if (pwdForm.newPassword.length < 6) {
      Alert.alert('Error', 'La nueva contraseña debe tener al menos 6 caracteres');
      return;
    }
    if (pwdForm.newPassword !== pwdForm.confirmPassword) {
      Alert.alert('Error', 'Las contraseñas no coinciden');
      return;
    }
    setSaving(true);
    try {
      await authApi.changePassword({
        currentPassword: pwdForm.currentPassword,
        newPassword: pwdForm.newPassword,
      });
      setChangingPassword(false);
      setPwdForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      Alert.alert('Listo', 'Contraseña actualizada');
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
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
            <Input label="Correo" value={form.correo} onChangeText={(v) => setForm({ ...form, correo: v })} keyboardType="email-address" autoCapitalize="none" />
            <Input label="Teléfono" value={form.telefono} onChangeText={(v) => setForm({ ...form, telefono: v })} keyboardType="phone-pad" />
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
            <Input label="Contraseña actual" secureTextEntry value={pwdForm.currentPassword} onChangeText={(v) => setPwdForm({ ...pwdForm, currentPassword: v })} />
            <Input label="Nueva contraseña" secureTextEntry value={pwdForm.newPassword} onChangeText={(v) => setPwdForm({ ...pwdForm, newPassword: v })} />
            <Input label="Confirmar" secureTextEntry value={pwdForm.confirmPassword} onChangeText={(v) => setPwdForm({ ...pwdForm, confirmPassword: v })} />
            <Button label="Actualizar contraseña" onPress={handlePasswordChange} loading={saving} />
          </>
        ) : (
          <Text style={styles.hint}>Protege tu cuenta con una contraseña segura.</Text>
        )}
      </Card>

      {userType === 'admin' ? (
        <Button label="Ir al panel admin" onPress={() => router.replace('/(admin)')} style={{ marginBottom: spacing.md }} />
      ) : null}

      <Button label="Cerrar sesión" variant="danger" onPress={logout} />
    </Screen>
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
});
