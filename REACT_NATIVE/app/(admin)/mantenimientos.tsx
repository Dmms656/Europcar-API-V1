import { useCallback, useMemo, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { mantenimientosApi } from '@/src/api/mantenimientosApi';
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
import { alertMessage, confirmAction } from '@/src/utils/confirm';
import { formatCurrency } from '@/src/utils/format';

type Mantenimiento = {
  idMantenimiento?: number;
  idVehiculo?: number;
  placaVehiculo?: string;
  tipoMantenimiento?: string;
  estadoMantenimiento?: string;
  costoMantenimiento?: number;
  proveedorTaller?: string;
  kilometrajeMantenimiento?: number;
};

const initialForm = {
  idVehiculo: '',
  tipoMantenimiento: 'PREVENTIVO',
  kilometrajeMantenimiento: '',
  costoMantenimiento: '',
  proveedorTaller: '',
  observaciones: '',
};

export default function AdminMantenimientosScreen() {
  const loader = useCallback(async () => {
    const res = await mantenimientosApi.getAll();
    return unwrapData<Mantenimiento[]>(res) ?? [];
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
    return items.filter((m) =>
      `${m.placaVehiculo} ${m.tipoMantenimiento} ${m.estadoMantenimiento} ${m.idVehiculo}`.toLowerCase().includes(q),
    );
  }, [items, search]);

  const pagination = useClientPagination(filtered, 10, search);

  const openCreate = () => {
    setEditingId(null);
    setForm(initialForm);
    setShowModal(true);
  };

  const handleSave = async () => {
    if (!form.idVehiculo) {
      void alertMessage('Error', 'ID de vehículo requerido');
      return;
    }
    setSaving(true);
    try {
      const payload = {
        idVehiculo: Number(form.idVehiculo),
        tipoMantenimiento: form.tipoMantenimiento,
        kilometrajeMantenimiento: Number(form.kilometrajeMantenimiento) || 0,
        costoMantenimiento: Number(form.costoMantenimiento) || 0,
        proveedorTaller: form.proveedorTaller || null,
        observaciones: form.observaciones || null,
        estadoMantenimiento: 'ABIERTO',
      };
      if (editingId) {
        await mantenimientosApi.update(editingId, payload);
      } else {
        await mantenimientosApi.create(payload);
      }
      void alertMessage('Listo', editingId ? 'Actualizado' : 'Registrado');
      setShowModal(false);
      await load();
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const cerrar = async (id?: number) => {
    if (!id) return;
    const ok = await confirmAction('Cerrar mantenimiento', '¿Confirmar cierre?', { confirmLabel: 'Cerrar' });
    if (!ok) return;
    try {
      await mantenimientosApi.cerrar(id, {});
      await load();
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    }
  };

  return (
    <View style={styles.flex}>
      <FlatList
        style={styles.list}
        contentContainerStyle={styles.content}
        data={pagination.paginatedItems}
        keyExtractor={(item, i) => String(item.idMantenimiento ?? i)}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={refresh} tintColor={colors.primary} />}
        ListHeaderComponent={
          <AdminScreen
            title="Mantenimientos"
            count={items.length}
            error={error}
            loading={loading && items.length === 0}
            search={search}
            onSearchChange={setSearch}
            actions={<Button label="+ Nuevo" onPress={openCreate} />}
          >
            {!loading && filtered.length === 0 ? <EmptyState title="No hay mantenimientos" icon="construct-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item }) => (
          <Card>
            <View style={styles.row}>
              <Text style={styles.name}>{item.tipoMantenimiento}</Text>
              <StatusBadge label={item.estadoMantenimiento || '—'} />
            </View>
            <Text style={styles.meta}>Vehículo: {item.placaVehiculo || item.idVehiculo}</Text>
            <Text style={styles.meta}>{formatCurrency(item.costoMantenimiento)} · {item.proveedorTaller || '—'}</Text>
            {item.estadoMantenimiento === 'ABIERTO' ? (
              <Button label="Cerrar" variant="secondary" onPress={() => cerrar(item.idMantenimiento)} style={styles.btn} />
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
      <Modal visible={showModal} title="Nuevo mantenimiento" onClose={() => setShowModal(false)}>
        <Input label="ID vehículo" value={form.idVehiculo} onChangeText={(v) => setForm({ ...form, idVehiculo: v })} keyboardType="number-pad" />
        <Input label="Tipo" value={form.tipoMantenimiento} onChangeText={(v) => setForm({ ...form, tipoMantenimiento: v })} />
        <Input label="Kilometraje" value={form.kilometrajeMantenimiento} onChangeText={(v) => setForm({ ...form, kilometrajeMantenimiento: v })} keyboardType="number-pad" />
        <Input label="Costo" value={form.costoMantenimiento} onChangeText={(v) => setForm({ ...form, costoMantenimiento: v })} keyboardType="decimal-pad" />
        <Input label="Taller" value={form.proveedorTaller} onChangeText={(v) => setForm({ ...form, proveedorTaller: v })} />
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
  name: { color: colors.text, fontWeight: '700', fontSize: 16 },
  meta: { color: colors.textSecondary, marginTop: 4, fontSize: 13 },
  btn: { marginTop: spacing.md, minHeight: 40 },
});
