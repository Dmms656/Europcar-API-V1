import { useCallback, useMemo, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { catalogosApi } from '@/src/api/catalogosApi';
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
import { alertMessage } from '@/src/utils/confirm';
import { formatCurrency } from '@/src/utils/format';

type Extra = {
  idExtra?: number;
  codigoExtra?: string;
  nombreExtra?: string;
  descripcionExtra?: string;
  tipoExtra?: string;
  valorFijo?: number;
  estadoExtra?: string;
};

const initialForm = {
  codigoExtra: '',
  nombreExtra: '',
  descripcionExtra: '',
  tipoExtra: 'SERVICIO',
  valorFijo: '',
};

export default function AdminExtrasScreen() {
  const hasAnyRole = useAuthStore((s) => s.hasAnyRole);
  const isAdmin = hasAnyRole('ADMIN');

  const loader = useCallback(async () => {
    const res = await catalogosApi.getExtras();
    return unwrapData<Extra[]>(res) ?? [];
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
    return items.filter((e) =>
      `${e.codigoExtra} ${e.nombreExtra} ${e.tipoExtra}`.toLowerCase().includes(q),
    );
  }, [items, search]);

  const pagination = useClientPagination(filtered, 10, search);

  const openCreate = () => {
    setEditingId(null);
    setForm(initialForm);
    setShowModal(true);
  };

  const openEdit = (e: Extra) => {
    setEditingId(e.idExtra ?? null);
    setForm({
      codigoExtra: e.codigoExtra ?? '',
      nombreExtra: e.nombreExtra ?? '',
      descripcionExtra: e.descripcionExtra ?? '',
      tipoExtra: e.tipoExtra ?? 'SERVICIO',
      valorFijo: String(e.valorFijo ?? ''),
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    if (!form.codigoExtra.trim() || !form.nombreExtra.trim()) {
      void alertMessage('Error', 'Código y nombre son requeridos');
      return;
    }
    setSaving(true);
    try {
      const payload = {
        codigoExtra: form.codigoExtra.trim(),
        nombreExtra: form.nombreExtra.trim(),
        descripcionExtra: form.descripcionExtra.trim() || null,
        tipoExtra: form.tipoExtra,
        valorFijo: Number(form.valorFijo) || 0,
      };
      if (editingId) {
        await catalogosApi.updateExtra(editingId, payload);
        void alertMessage('Listo', 'Extra actualizado');
      } else {
        await catalogosApi.createExtra(payload);
        void alertMessage('Listo', 'Extra creado');
      }
      setShowModal(false);
      await load();
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const toggleEstado = async (e: Extra) => {
    if (!e.idExtra) return;
    const nuevo = e.estadoExtra === 'ACT' ? 'INA' : 'ACT';
    try {
      await catalogosApi.cambiarEstadoExtra(e.idExtra, nuevo);
      await load();
    } catch (err) {
      void alertMessage('Error', getErrorMessage(err));
    }
  };

  return (
    <View style={styles.flex}>
      <FlatList
        style={styles.list}
        contentContainerStyle={styles.content}
        data={pagination.paginatedItems}
        keyExtractor={(item, i) => String(item.idExtra ?? i)}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={refresh} tintColor={colors.primary} />}
        ListHeaderComponent={
          <AdminScreen
            title="Extras"
            count={items.length}
            error={error}
            loading={loading && items.length === 0}
            search={search}
            onSearchChange={setSearch}
            searchPlaceholder="Buscar por código o nombre…"
            actions={isAdmin ? <Button label="+ Nuevo" onPress={openCreate} /> : undefined}
          >
            {!loading && filtered.length === 0 ? <EmptyState title="No hay extras" icon="cube-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item }) => (
          <Card>
            <View style={styles.row}>
              <Text style={styles.name}>{item.nombreExtra}</Text>
              <StatusBadge label={item.estadoExtra === 'ACT' ? 'ACTIVO' : 'INACTIVO'} />
            </View>
            <Text style={styles.meta}>{item.codigoExtra} · {item.tipoExtra}</Text>
            <Text style={styles.meta}>{formatCurrency(item.valorFijo)}</Text>
            {isAdmin ? (
              <View style={styles.actions}>
                <Button label="Editar" variant="secondary" onPress={() => openEdit(item)} style={styles.btn} />
                <Button
                  label={item.estadoExtra === 'ACT' ? 'Desactivar' : 'Activar'}
                  variant="ghost"
                  onPress={() => toggleEstado(item)}
                  style={styles.btn}
                />
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

      <Modal visible={showModal} title={editingId ? 'Editar extra' : 'Nuevo extra'} onClose={() => setShowModal(false)}>
        <Input label="Código" value={form.codigoExtra} onChangeText={(v) => setForm({ ...form, codigoExtra: v })} editable={!editingId} />
        <Input label="Nombre" value={form.nombreExtra} onChangeText={(v) => setForm({ ...form, nombreExtra: v })} />
        <Input label="Descripción" value={form.descripcionExtra} onChangeText={(v) => setForm({ ...form, descripcionExtra: v })} />
        <Input label="Tipo" value={form.tipoExtra} onChangeText={(v) => setForm({ ...form, tipoExtra: v })} />
        <Input label="Valor fijo" value={form.valorFijo} onChangeText={(v) => setForm({ ...form, valorFijo: v })} keyboardType="decimal-pad" />
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
