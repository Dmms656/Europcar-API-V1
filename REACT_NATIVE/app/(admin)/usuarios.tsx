import { useCallback, useMemo, useState } from 'react';
import { Alert, FlatList, Pressable, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { usuariosApi } from '@/src/api/usuariosApi';
import { AdminScreen } from '@/src/components/admin/AdminScreen';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { EmptyState } from '@/src/components/ui/EmptyState';
import { Input } from '@/src/components/ui/Input';
import { Modal } from '@/src/components/ui/Modal';
import { PaginationControls } from '@/src/components/ui/PaginationControls';
import { StatusBadge } from '@/src/components/ui/StatusBadge';
import { useAdminList } from '@/src/hooks/useAdminList';
import { useClientPagination } from '@/src/hooks/useClientPagination';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

type Usuario = {
  idUsuario?: number;
  username?: string;
  correo?: string;
  roles?: string[];
  activo?: boolean;
  estado?: string;
};

const ROLES = ['ADMIN', 'AGENTE_POS', 'CLIENTE_WEB'];

export default function AdminUsuariosScreen() {
  const loader = useCallback(async () => {
    const res = await usuariosApi.getAll();
    return unwrapData<Usuario[]>(res) ?? [];
  }, []);

  const { items, loading, refreshing, error, load, refresh } = useAdminList(loader);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [showRolesModal, setShowRolesModal] = useState(false);
  const [editingUser, setEditingUser] = useState<Usuario | null>(null);
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState({ username: '', correo: '', password: '', roles: [] as string[] });

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim();
    if (!q) return items;
    return items.filter((u) =>
      `${u.username} ${u.correo} ${u.roles?.join(' ')}`.toLowerCase().includes(q),
    );
  }, [items, search]);

  const pagination = useClientPagination(filtered, 10, search);

  const toggleRole = (role: string, target: 'form' | 'edit') => {
    if (target === 'form') {
      setForm((prev) => ({
        ...prev,
        roles: prev.roles.includes(role) ? prev.roles.filter((r) => r !== role) : [...prev.roles, role],
      }));
    } else {
      setSelectedRoles((prev) =>
        prev.includes(role) ? prev.filter((r) => r !== role) : [...prev, role],
      );
    }
  };

  const handleCreate = async () => {
    if (!form.username || !form.correo || !form.password || form.roles.length === 0) {
      Alert.alert('Error', 'Completa todos los campos');
      return;
    }
    setSaving(true);
    try {
      await usuariosApi.create(form);
      Alert.alert('Listo', 'Usuario creado');
      setShowModal(false);
      setForm({ username: '', correo: '', password: '', roles: [] });
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const handleSaveRoles = async () => {
    if (!editingUser?.idUsuario || selectedRoles.length === 0) {
      Alert.alert('Error', 'Selecciona al menos un rol');
      return;
    }
    setSaving(true);
    try {
      await usuariosApi.updateRoles(editingUser.idUsuario, selectedRoles);
      Alert.alert('Listo', 'Roles actualizados');
      setShowRolesModal(false);
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const toggleEstado = async (u: Usuario) => {
    if (!u.idUsuario) return;
    const activo = u.activo ?? u.estado === 'ACT';
    const nuevo = activo ? 'INA' : 'ACT';
    try {
      await usuariosApi.updateEstado(u.idUsuario, nuevo);
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    }
  };

  const handleDelete = (u: Usuario) => {
    if (!u.idUsuario) return;
    Alert.alert('Eliminar usuario', `¿Eliminar a ${u.username}?`, [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar',
        style: 'destructive',
        onPress: async () => {
          try {
            await usuariosApi.delete(u.idUsuario!);
            await load();
          } catch (e) {
            Alert.alert('Error', getErrorMessage(e));
          }
        },
      },
    ]);
  };

  const RolePicker = ({ selected, onToggle }: { selected: string[]; onToggle: (r: string) => void }) => (
    <View style={styles.roles}>
      {ROLES.map((r) => (
        <Pressable key={r} style={[styles.roleChip, selected.includes(r) && styles.roleChipOn]} onPress={() => onToggle(r)}>
          <Text style={[styles.roleText, selected.includes(r) && styles.roleTextOn]}>{r}</Text>
        </Pressable>
      ))}
    </View>
  );

  return (
    <View style={styles.flex}>
      <FlatList
        style={styles.list}
        contentContainerStyle={styles.content}
        data={pagination.paginatedItems}
        keyExtractor={(item, i) => String(item.idUsuario ?? i)}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={refresh} tintColor={colors.primary} />}
        ListHeaderComponent={
          <AdminScreen
            title="Usuarios"
            count={items.length}
            error={error}
            loading={loading && items.length === 0}
            search={search}
            onSearchChange={setSearch}
            actions={<Button label="+ Nuevo" onPress={() => setShowModal(true)} />}
          >
            {!loading && filtered.length === 0 ? <EmptyState title="No hay usuarios" icon="people-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item }) => {
          const activo = item.activo ?? item.estado === 'ACT';
          return (
            <Card>
              <View style={styles.row}>
                <Text style={styles.name}>{item.username}</Text>
                <StatusBadge label={activo ? 'ACTIVO' : 'INACTIVO'} />
              </View>
              <Text style={styles.meta}>{item.correo}</Text>
              <Text style={styles.meta}>{item.roles?.join(', ') || '—'}</Text>
              <View style={styles.actions}>
                <Button
                  label="Roles"
                  variant="secondary"
                  onPress={() => {
                    setEditingUser(item);
                    setSelectedRoles([...(item.roles || [])]);
                    setShowRolesModal(true);
                  }}
                  style={styles.btn}
                />
                <Button label={activo ? 'Desactivar' : 'Activar'} variant="ghost" onPress={() => toggleEstado(item)} style={styles.btn} />
                <Button label="Eliminar" variant="danger" onPress={() => handleDelete(item)} style={styles.btn} />
              </View>
            </Card>
          );
        }}
        ListFooterComponent={
          filtered.length > 0 ? (
            <PaginationControls
              page={pagination.page}
              totalPages={pagination.totalPages}
              pageSize={pagination.pageSize}
              totalItems={pagination.totalItems}
              startItem={pagination.startItem}
              endItem={pagination.endItem}
              onPageChange={pagination.setPage}
              onPageSizeChange={pagination.setPageSize}
            />
          ) : null
        }
      />

      <Modal visible={showModal} title="Nuevo usuario" onClose={() => setShowModal(false)}>
        <Input label="Usuario" value={form.username} onChangeText={(v) => setForm({ ...form, username: v })} autoCapitalize="none" />
        <Input label="Correo" value={form.correo} onChangeText={(v) => setForm({ ...form, correo: v })} keyboardType="email-address" autoCapitalize="none" />
        <Input label="Contraseña" value={form.password} onChangeText={(v) => setForm({ ...form, password: v })} secureTextEntry />
        <Text style={styles.roleLabel}>Roles</Text>
        <RolePicker selected={form.roles} onToggle={(r) => toggleRole(r, 'form')} />
        <Button label={saving ? 'Creando…' : 'Crear'} onPress={handleCreate} loading={saving} />
      </Modal>

      <Modal visible={showRolesModal} title={`Roles: ${editingUser?.username}`} onClose={() => setShowRolesModal(false)}>
        <RolePicker selected={selectedRoles} onToggle={(r) => toggleRole(r, 'edit')} />
        <Button label={saving ? 'Guardando…' : 'Guardar roles'} onPress={handleSaveRoles} loading={saving} />
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  flex: { flex: 1 },
  list: { flex: 1, backgroundColor: colors.bg },
  content: { padding: spacing.lg, paddingBottom: spacing.xxl },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  name: { color: colors.text, fontWeight: '700', fontSize: 16 },
  meta: { color: colors.textSecondary, marginTop: 4, fontSize: 13 },
  actions: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm, marginTop: spacing.md },
  btn: { minHeight: 36, paddingVertical: 6, flexGrow: 1 },
  roles: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm, marginBottom: spacing.md },
  roleChip: { paddingHorizontal: spacing.md, paddingVertical: spacing.sm, borderRadius: 8, borderWidth: 1, borderColor: colors.border },
  roleChipOn: { backgroundColor: colors.primaryGhost, borderColor: colors.primary },
  roleText: { color: colors.textMuted, fontSize: 12, fontWeight: '600' },
  roleTextOn: { color: colors.primaryLight },
  roleLabel: { color: colors.textSecondary, fontWeight: '600', marginBottom: spacing.sm },
});
