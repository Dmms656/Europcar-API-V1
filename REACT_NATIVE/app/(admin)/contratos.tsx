import { useCallback, useMemo, useState } from 'react';
import { Alert, FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { contratosApi, type ContratoItem } from '@/src/api/contratosApi';
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
import { contratoCodigo, contratoEstado, formatCurrency, formatDateEs } from '@/src/utils/format';

export default function AdminContratosScreen() {
  const loader = useCallback(async () => {
    const res = await contratosApi.getAll();
    return unwrapData<ContratoItem[]>(res) ?? [];
  }, []);

  const { items, loading, refreshing, error, load, refresh } = useAdminList(loader);
  const [search, setSearch] = useState('');
  const [showCrear, setShowCrear] = useState(false);
  const [reservaRef, setReservaRef] = useState('');
  const [saving, setSaving] = useState(false);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim();
    if (!q) return items;
    return items.filter((c) =>
      `${contratoCodigo(c)} ${c.placaVehiculo} ${contratoEstado(c)}`.toLowerCase().includes(q),
    );
  }, [items, search]);

  const pagination = useClientPagination(filtered, 10, search);

  const crearContrato = async () => {
    const ref = reservaRef.trim();
    if (!ref) {
      Alert.alert('Error', 'Ingresa ID o código de reserva');
      return;
    }
    setSaving(true);
    try {
      const isNumeric = /^\d+$/.test(ref);
      const payload = isNumeric ? { idReserva: Number(ref) } : { codigoReserva: ref.toUpperCase() };
      await contratosApi.create(payload);
      Alert.alert('Listo', 'Contrato creado');
      setShowCrear(false);
      setReservaRef('');
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
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
        keyExtractor={(item, i) => String(item.idContrato ?? item.id ?? i)}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={refresh} tintColor={colors.primary} />}
        ListHeaderComponent={
          <AdminScreen
            title="Contratos"
            count={items.length}
            error={error}
            loading={loading && items.length === 0}
            search={search}
            onSearchChange={setSearch}
            actions={<Button label="+ Crear" onPress={() => setShowCrear(true)} />}
          >
            {!loading && filtered.length === 0 ? <EmptyState title="No hay contratos" icon="document-text-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item }) => (
          <Card>
            <View style={styles.row}>
              <Text style={styles.name}>{contratoCodigo(item)}</Text>
              <StatusBadge label={contratoEstado(item)} />
            </View>
            <Text style={styles.meta}>🚗 {item.placaVehiculo || item.vehiculo || '—'}</Text>
            <Text style={styles.meta}>
              {formatDateEs(item.fechaHoraSalida || item.fechaSalida)} → {formatDateEs(item.fechaHoraDevolucion || item.fechaDevolucion)}
            </Text>
            <Text style={styles.total}>{formatCurrency(item.totalContrato ?? item.total)}</Text>
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
      <Modal visible={showCrear} title="Crear contrato desde reserva" onClose={() => setShowCrear(false)}>
        <Input label="ID o código de reserva" value={reservaRef} onChangeText={setReservaRef} autoCapitalize="characters" />
        <Button label={saving ? 'Creando…' : 'Crear contrato'} onPress={crearContrato} loading={saving} />
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
  total: { color: colors.accent, fontWeight: '700', marginTop: spacing.sm },
});
