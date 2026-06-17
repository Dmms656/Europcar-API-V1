import { useCallback, useMemo, useState } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { pagosApi } from '@/src/api/pagosApi';
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
import { alertMessage } from '@/src/utils/confirm';
import { formatCurrency, formatDateEs } from '@/src/utils/format';

type Pago = {
  idPago?: number;
  codigoReserva?: string;
  idReserva?: number;
  idContrato?: number;
  tipoPago?: string;
  metodoPago?: string;
  estadoPago?: string;
  monto?: number;
  fechaPago?: string;
  referenciaExterna?: string;
};

const initialForm = {
  reservaRef: '',
  monto: '',
  tipoPago: 'COBRO',
  metodoPago: 'TARJETA',
  estadoPago: 'APROBADO',
};

export default function AdminPagosScreen() {
  const loader = useCallback(async () => {
    const res = await pagosApi.getAll();
    return unwrapData<Pago[]>(res) ?? [];
  }, []);

  const { items, loading, refreshing, error, load, refresh } = useAdminList(loader);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState(initialForm);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim();
    if (!q) return items;
    return items.filter((p) =>
      `${p.codigoReserva} ${p.tipoPago} ${p.metodoPago} ${p.estadoPago}`.toLowerCase().includes(q),
    );
  }, [items, search]);

  const pagination = useClientPagination(filtered, 10, search);

  const handleSave = async () => {
    if (!form.reservaRef.trim() || !form.monto) {
      void alertMessage('Error', 'Reserva y monto son requeridos');
      return;
    }
    setSaving(true);
    try {
      const ref = form.reservaRef.trim();
      const isNumeric = /^\d+$/.test(ref);
      await pagosApi.create({
        ...(isNumeric ? { idReserva: Number(ref) } : { codigoReserva: ref.toUpperCase() }),
        tipoPago: form.tipoPago,
        metodoPago: form.metodoPago,
        estadoPago: form.estadoPago,
        monto: Number(form.monto),
      });
      void alertMessage('Listo', 'Pago registrado');
      setShowModal(false);
      setForm(initialForm);
      await load();
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  return (
    <View style={styles.flex}>
      <FlatList
        style={styles.list}
        contentContainerStyle={styles.content}
        data={pagination.paginatedItems}
        keyExtractor={(item, i) => String(item.idPago ?? i)}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={refresh} tintColor={colors.primary} />}
        ListHeaderComponent={
          <AdminScreen
            title="Pagos"
            count={items.length}
            error={error}
            loading={loading && items.length === 0}
            search={search}
            onSearchChange={setSearch}
            actions={<Button label="+ Nuevo" onPress={() => setShowModal(true)} />}
          >
            {!loading && filtered.length === 0 ? <EmptyState title="No hay pagos" icon="card-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item }) => (
          <Card>
            <View style={styles.row}>
              <Text style={styles.name}>{formatCurrency(item.monto)}</Text>
              <StatusBadge label={item.estadoPago || '—'} />
            </View>
            <Text style={styles.meta}>{item.tipoPago} · {item.metodoPago}</Text>
            <Text style={styles.meta}>Reserva: {item.codigoReserva || item.idReserva || '—'}</Text>
            <Text style={styles.meta}>{formatDateEs(item.fechaPago, true)}</Text>
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
      <Modal visible={showModal} title="Registrar pago" onClose={() => setShowModal(false)}>
        <Input label="ID o código reserva" value={form.reservaRef} onChangeText={(v) => setForm({ ...form, reservaRef: v })} />
        <Input label="Monto" value={form.monto} onChangeText={(v) => setForm({ ...form, monto: v })} keyboardType="decimal-pad" />
        <Input label="Tipo" value={form.tipoPago} onChangeText={(v) => setForm({ ...form, tipoPago: v })} />
        <Input label="Método" value={form.metodoPago} onChangeText={(v) => setForm({ ...form, metodoPago: v })} />
        <Button label={saving ? 'Guardando…' : 'Registrar'} onPress={handleSave} loading={saving} />
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  flex: { flex: 1 },
  list: { flex: 1, backgroundColor: colors.bg },
  content: { padding: spacing.lg, paddingBottom: spacing.xxl },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  name: { color: colors.text, fontWeight: '700', fontSize: 18 },
  meta: { color: colors.textSecondary, marginTop: 4, fontSize: 13 },
});
