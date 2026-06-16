import { useCallback, useMemo, useState } from 'react';
import { Alert, FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { localizacionesApi } from '@/src/api/localizacionesApi';
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
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

type Localizacion = {
  idLocalizacion?: number;
  codigoLocalizacion?: string;
  nombreLocalizacion?: string;
  nombreCiudad?: string;
  direccionLocalizacion?: string;
  telefonoContacto?: string;
  correoContacto?: string;
  estadoLocalizacion?: string;
};

const initialForm = {
  codigoLocalizacion: '',
  nombreLocalizacion: '',
  idCiudad: '',
  direccionLocalizacion: '',
  telefonoContacto: '',
  correoContacto: '',
};

export default function AdminLocalizacionesScreen() {
  const isAdmin = useAuthStore((s) => s.hasAnyRole('ADMIN'));

  const loader = useCallback(async () => {
    const res = await localizacionesApi.getAll(false);
    return unwrapData<Localizacion[]>(res) ?? [];
  }, []);

  const { items, loading, refreshing, error, load, refresh } = useAdminList(loader);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState(initialForm);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim();
    if (!q) return items;
    return items.filter((l) =>
      `${l.codigoLocalizacion} ${l.nombreLocalizacion} ${l.nombreCiudad} ${l.direccionLocalizacion}`.toLowerCase().includes(q),
    );
  }, [items, search]);

  const pagination = useClientPagination(filtered, 10, search);

  const openCreate = () => {
    setEditingId(null);
    setForm(initialForm);
    setShowModal(true);
  };

  const openEdit = (l: Localizacion) => {
    setEditingId(l.idLocalizacion ?? null);
    setForm({
      codigoLocalizacion: l.codigoLocalizacion ?? '',
      nombreLocalizacion: l.nombreLocalizacion ?? '',
      idCiudad: '',
      direccionLocalizacion: l.direccionLocalizacion ?? '',
      telefonoContacto: l.telefonoContacto ?? '',
      correoContacto: l.correoContacto ?? '',
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    if (!form.codigoLocalizacion.trim() || !form.nombreLocalizacion.trim()) {
      Alert.alert('Error', 'Código y nombre son requeridos');
      return;
    }
    setSaving(true);
    try {
      const payload = {
        codigoLocalizacion: form.codigoLocalizacion.trim(),
        nombreLocalizacion: form.nombreLocalizacion.trim(),
        idCiudad: form.idCiudad ? Number(form.idCiudad) : undefined,
        direccionLocalizacion: form.direccionLocalizacion || null,
        telefonoContacto: form.telefonoContacto || null,
        correoContacto: form.correoContacto || null,
      };
      if (editingId) {
        await localizacionesApi.update(editingId, payload);
      } else {
        await localizacionesApi.create(payload);
      }
      Alert.alert('Listo', 'Localización guardada');
      setShowModal(false);
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const toggleEstado = async (l: Localizacion) => {
    if (!l.idLocalizacion) return;
    const nuevo = l.estadoLocalizacion === 'ACT' ? 'INA' : 'ACT';
    try {
      await localizacionesApi.cambiarEstado(l.idLocalizacion, nuevo);
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    }
  };

  return (
    <View style={styles.flex}>
      <FlatList
        style={styles.list}
        contentContainerStyle={styles.content}
        data={pagination.paginatedItems}
        keyExtractor={(item, i) => String(item.idLocalizacion ?? i)}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={refresh} tintColor={colors.primary} />}
        ListHeaderComponent={
          <AdminScreen
            title="Localizaciones"
            count={items.length}
            error={error}
            loading={loading && items.length === 0}
            search={search}
            onSearchChange={setSearch}
            actions={isAdmin ? <Button label="+ Nueva" onPress={openCreate} /> : undefined}
          >
            {!loading && filtered.length === 0 ? <EmptyState title="No hay localizaciones" icon="location-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item }) => (
          <Card>
            <View style={styles.row}>
              <Text style={styles.name}>{item.nombreLocalizacion}</Text>
              <StatusBadge label={item.estadoLocalizacion === 'ACT' ? 'ACTIVA' : 'INACTIVA'} />
            </View>
            <Text style={styles.meta}>{item.codigoLocalizacion} · {item.nombreCiudad || '—'}</Text>
            <Text style={styles.meta}>{item.direccionLocalizacion || '—'}</Text>
            {isAdmin ? (
              <View style={styles.actions}>
                <Button label="Editar" variant="secondary" onPress={() => openEdit(item)} style={styles.btn} />
                <Button label="Estado" variant="ghost" onPress={() => toggleEstado(item)} style={styles.btn} />
              </View>
            ) : null}
          </Card>
        )}
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
      <Modal visible={showModal} title={editingId ? 'Editar localización' : 'Nueva localización'} onClose={() => setShowModal(false)}>
        <Input label="Código" value={form.codigoLocalizacion} onChangeText={(v) => setForm({ ...form, codigoLocalizacion: v })} editable={!editingId} />
        <Input label="Nombre" value={form.nombreLocalizacion} onChangeText={(v) => setForm({ ...form, nombreLocalizacion: v })} />
        <Input label="ID Ciudad" value={form.idCiudad} onChangeText={(v) => setForm({ ...form, idCiudad: v })} keyboardType="number-pad" />
        <Input label="Dirección" value={form.direccionLocalizacion} onChangeText={(v) => setForm({ ...form, direccionLocalizacion: v })} />
        <Input label="Teléfono" value={form.telefonoContacto} onChangeText={(v) => setForm({ ...form, telefonoContacto: v })} keyboardType="phone-pad" />
        <Input label="Correo" value={form.correoContacto} onChangeText={(v) => setForm({ ...form, correoContacto: v })} keyboardType="email-address" autoCapitalize="none" />
        <Button label={saving ? 'Guardando…' : 'Guardar'} onPress={handleSave} loading={saving} />
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  flex: { flex: 1 },
  list: { flex: 1, backgroundColor: colors.bg },
  content: { padding: spacing.lg, paddingBottom: spacing.xxl },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  name: { color: colors.text, fontWeight: '700', fontSize: 16, flex: 1 },
  meta: { color: colors.textSecondary, marginTop: 4, fontSize: 13 },
  actions: { flexDirection: 'row', gap: spacing.sm, marginTop: spacing.md },
  btn: { flex: 1, minHeight: 40, paddingVertical: 8 },
});
