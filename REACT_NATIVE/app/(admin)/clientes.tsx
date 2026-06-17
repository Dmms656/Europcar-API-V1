import { useCallback, useMemo, useState } from 'react';
import { Alert, FlatList, RefreshControl, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { clientesApi } from '@/src/api/clientesApi';
import { AdminScreen } from '@/src/components/admin/AdminScreen';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { EmptyState } from '@/src/components/ui/EmptyState';
import { Input } from '@/src/components/ui/Input';
import { Modal } from '@/src/components/ui/Modal';
import { PaginationControls } from '@/src/components/ui/PaginationControls';
import { Select } from '@/src/components/ui/Select';
import { StatusBadge } from '@/src/components/ui/StatusBadge';
import { useClientPagination } from '@/src/hooks/useClientPagination';
import { colors } from '@/src/theme/colors';
import { radius, shadows, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

type Cliente = {
  idCliente: number;
  nombreCompleto?: string;
  nombres?: string;
  apellidos?: string;
  nombre1?: string;
  nombre2?: string;
  apellido1?: string;
  apellido2?: string;
  correo?: string;
  telefono?: string;
  numeroIdentificacion?: string;
  tipoIdentificacion?: string;
  fechaNacimiento?: string;
  direccionPrincipal?: string;
  estadoCliente?: string;
  rowVersion?: string;
};

const INITIAL_FORM = {
  tipoIdentificacion: 'CED',
  numeroIdentificacion: '',
  nombre1: '',
  nombre2: '',
  apellido1: '',
  apellido2: '',
  fechaNacimiento: '',
  telefono: '',
  correo: '',
  direccionPrincipal: '',
};

export default function AdminClientesScreen() {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState(INITIAL_FORM);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setError('');
    try {
      const res = await clientesApi.getAll();
      setClientes(unwrapData<Cliente[]>(res) ?? []);
    } catch (e) {
      setError(getErrorMessage(e));
      setClientes([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim();
    if (!q) return clientes;
    return clientes.filter((c) =>
      `${c.nombreCompleto ?? ''} ${c.nombres ?? ''} ${c.apellidos ?? ''} ${c.numeroIdentificacion ?? ''} ${c.correo ?? ''}`
        .toLowerCase()
        .includes(q),
    );
  }, [clientes, search]);

  const pagination = useClientPagination(filtered, 10, search);

  const displayName = (c: Cliente) =>
    c.nombreCompleto?.trim() ||
    `${c.nombre1 ?? c.nombres ?? ''} ${c.apellido1 ?? c.apellidos ?? ''}`.trim() ||
    '—';

  const openCreate = () => {
    setForm(INITIAL_FORM);
    setEditingId(null);
    setShowModal(true);
  };

  const openEdit = (c: Cliente) => {
    setForm({
      tipoIdentificacion: c.tipoIdentificacion || 'CED',
      numeroIdentificacion: c.numeroIdentificacion || '',
      nombre1: c.nombre1 || c.nombres || '',
      nombre2: c.nombre2 || '',
      apellido1: c.apellido1 || c.apellidos || '',
      apellido2: c.apellido2 || '',
      fechaNacimiento: c.fechaNacimiento?.slice(0, 10) || '',
      telefono: c.telefono || '',
      correo: c.correo || '',
      direccionPrincipal: c.direccionPrincipal || '',
    });
    setEditingId(c.idCliente);
    setShowModal(true);
  };

  const handleSave = async () => {
    if (!form.numeroIdentificacion.trim() || !form.nombre1.trim() || !form.apellido1.trim()) {
      Alert.alert('Error', 'Identificación, primer nombre y primer apellido son requeridos');
      return;
    }
    setSaving(true);
    try {
      if (editingId) {
        await clientesApi.update(editingId, form);
        Alert.alert('Listo', 'Cliente actualizado');
      } else {
        await clientesApi.create(form);
        Alert.alert('Listo', 'Cliente creado');
      }
      setShowModal(false);
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = (id: number) => {
    Alert.alert('Eliminar', '¿Eliminar este cliente?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar',
        style: 'destructive',
        onPress: async () => {
          try {
            await clientesApi.delete(id);
            await load();
          } catch (e) {
            Alert.alert('Error', getErrorMessage(e));
          }
        },
      },
    ]);
  };

  return (
    <View style={styles.flex}>
      <FlatList
        style={styles.list}
        contentContainerStyle={styles.content}
        data={pagination.paginatedItems}
        keyExtractor={(item) => String(item.idCliente)}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={async () => { setRefreshing(true); await load(); setRefreshing(false); }}
            tintColor={colors.primary}
          />
        }
        ListHeaderComponent={
          <AdminScreen
            title="Clientes"
            subtitle={`${clientes.length} registrados`}
            error={error}
            loading={loading && clientes.length === 0}
            search={search}
            onSearchChange={setSearch}
            searchPlaceholder="Buscar por nombre, ID o correo…"
            actions={<Button label="+ Nuevo" onPress={openCreate} />}
          >
            {!loading && filtered.length === 0 ? <EmptyState title="No hay clientes" icon="people-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item: c }) => (
          <Card style={styles.card}>
            <View style={styles.row}>
              <View style={{ flex: 1 }}>
                <Text style={styles.name}>{displayName(c)}</Text>
                <Text style={styles.meta}>{c.correo ?? '—'}</Text>
                <Text style={styles.meta}>
                  {c.tipoIdentificacion ?? 'CED'} {c.numeroIdentificacion ?? '—'}
                  {c.telefono ? ` · ${c.telefono}` : ''}
                </Text>
              </View>
              {c.estadoCliente ? <StatusBadge label={c.estadoCliente} /> : null}
            </View>
            <View style={styles.actions}>
              <Button label="Editar" variant="secondary" onPress={() => openEdit(c)} style={styles.btn} />
              <Button label="Eliminar" variant="danger" onPress={() => handleDelete(c.idCliente)} style={styles.btn} />
            </View>
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

      <Modal visible={showModal} title={editingId ? 'Editar cliente' : 'Nuevo cliente'} onClose={() => setShowModal(false)}>
        <ScrollView showsVerticalScrollIndicator={false}>
          <Select
            label="Tipo ID"
            value={form.tipoIdentificacion}
            onValueChange={(v) => setForm({ ...form, tipoIdentificacion: v })}
            options={[
              { label: 'Cédula', value: 'CED' },
              { label: 'Pasaporte', value: 'PAS' },
              { label: 'RUC', value: 'RUC' },
            ]}
          />
          <Input label="Número identificación *" value={form.numeroIdentificacion} onChangeText={(v) => setForm({ ...form, numeroIdentificacion: v })} />
          <Input label="Primer nombre *" value={form.nombre1} onChangeText={(v) => setForm({ ...form, nombre1: v })} />
          <Input label="Segundo nombre" value={form.nombre2} onChangeText={(v) => setForm({ ...form, nombre2: v })} />
          <Input label="Primer apellido *" value={form.apellido1} onChangeText={(v) => setForm({ ...form, apellido1: v })} />
          <Input label="Segundo apellido" value={form.apellido2} onChangeText={(v) => setForm({ ...form, apellido2: v })} />
          <Input label="Fecha nacimiento (YYYY-MM-DD)" value={form.fechaNacimiento} onChangeText={(v) => setForm({ ...form, fechaNacimiento: v })} />
          <Input label="Teléfono" value={form.telefono} onChangeText={(v) => setForm({ ...form, telefono: v })} keyboardType="phone-pad" />
          <Input label="Correo" value={form.correo} onChangeText={(v) => setForm({ ...form, correo: v })} autoCapitalize="none" keyboardType="email-address" />
          <Input label="Dirección" value={form.direccionPrincipal} onChangeText={(v) => setForm({ ...form, direccionPrincipal: v })} />
          <Button label={saving ? 'Guardando…' : 'Guardar'} onPress={handleSave} loading={saving} />
        </ScrollView>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  flex: { flex: 1 },
  list: { flex: 1, backgroundColor: colors.bg },
  content: { padding: spacing.lg, paddingBottom: spacing.xxl },
  card: { ...shadows.sm },
  row: { flexDirection: 'row', gap: spacing.md, alignItems: 'flex-start' },
  name: { color: colors.text, fontFamily: fonts.bold, fontSize: 16 },
  meta: { color: colors.textSecondary, marginTop: 4, fontSize: 13 },
  actions: { flexDirection: 'row', gap: spacing.sm, marginTop: spacing.md },
  btn: { flex: 1, minHeight: 42, paddingVertical: 8 },
});
