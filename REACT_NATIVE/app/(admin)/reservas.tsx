import { useCallback, useMemo, useState } from 'react';
import { FlatList, RefreshControl, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { listAdminReservas } from '@/src/api/adminApi';
import { catalogosApi } from '@/src/api/catalogosApi';
import { clientesApi } from '@/src/api/clientesApi';
import { reservasApi } from '@/src/api/reservasApi';
import { vehiculosApi } from '@/src/api/vehiculosApi';
import { AdminScreen } from '@/src/components/admin/AdminScreen';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { DateTimeSelector } from '@/src/components/ui/DateTimeSelector';
import { EmptyState } from '@/src/components/ui/EmptyState';
import { Modal } from '@/src/components/ui/Modal';
import { PaginationControls } from '@/src/components/ui/PaginationControls';
import { Select } from '@/src/components/ui/Select';
import { StatusBadge } from '@/src/components/ui/StatusBadge';
import { useClientPagination } from '@/src/hooks/useClientPagination';
import { colors } from '@/src/theme/colors';
import { shadows, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';
import { alertMessage, confirmAction } from '@/src/utils/confirm';
import { formatCurrency } from '@/src/utils/format';

type Reserva = {
  idReserva?: number;
  codigoReserva?: string;
  estadoReserva?: string;
  fechaHoraRecogida?: string;
  fechaHoraDevolucion?: string;
  fechaInicio?: string;
  fechaFin?: string;
  total?: number;
  idCliente?: number;
  idVehiculo?: number;
  idLocalizacionRecogida?: number;
  idLocalizacionDevolucion?: number;
  canalReserva?: string;
  nombreCliente?: string;
  cliente?: string;
  vehiculo?: string;
  placaVehiculo?: string;
};

type Cliente = { idCliente: number; nombreCompleto?: string; nombres?: string; apellidos?: string };
type Vehiculo = { idVehiculo: number; placa?: string; marca?: string; modelo?: string };
type Localizacion = { idLocalizacion: number; nombreLocalizacion?: string; idCiudad?: number };
type Ciudad = { idCiudad: number; idPais?: number };

function parseIso(s?: string) {
  if (!s) return new Date();
  const d = new Date(s);
  return Number.isNaN(d.getTime()) ? new Date() : d;
}

const defaultForm = () => {
  const pickup = new Date();
  pickup.setDate(pickup.getDate() + 1);
  pickup.setHours(10, 0, 0, 0);
  const ret = new Date(pickup);
  ret.setDate(ret.getDate() + 2);
  return {
    idCliente: '',
    idVehiculo: '',
    idLocalizacionRecogida: '',
    idLocalizacionDevolucion: '',
    canalReserva: 'WEB',
    fechaRecogida: pickup,
    fechaDevolucion: ret,
  };
};

export default function AdminReservasScreen() {
  const [reservas, setReservas] = useState<Reserva[]>([]);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [vehiculos, setVehiculos] = useState<Vehiculo[]>([]);
  const [localizaciones, setLocalizaciones] = useState<Localizacion[]>([]);
  const [ciudades, setCiudades] = useState<Ciudad[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [filterCliente, setFilterCliente] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<Reserva | null>(null);
  const [form, setForm] = useState(defaultForm);
  const [saving, setSaving] = useState(false);

  const getPaisByLocalizacion = useCallback(
    (idLoc: string) => {
      const loc = localizaciones.find((l) => String(l.idLocalizacion) === idLoc);
      if (!loc?.idCiudad) return null;
      return ciudades.find((c) => c.idCiudad === loc.idCiudad)?.idPais ?? null;
    },
    [localizaciones, ciudades],
  );

  const loadAll = useCallback(async () => {
    setError('');
    try {
      const [cRes, vRes, lRes, ciRes, rList] = await Promise.allSettled([
        clientesApi.getAll({ page: 1, limit: 100 }),
        vehiculosApi.getAll(),
        catalogosApi.getLocalizaciones(),
        catalogosApi.getCiudades(),
        listAdminReservas(),
      ]);
      if (cRes.status === 'fulfilled') {
        setClientes(unwrapData<Cliente[]>(cRes.value) ?? []);
      } else {
        setClientes([]);
      }
      if (vRes.status === 'fulfilled') {
        setVehiculos(unwrapData<Vehiculo[]>(vRes.value) ?? []);
      }
      if (lRes.status === 'fulfilled') {
        const rawL = unwrapData<Record<string, unknown>[]>(lRes.value) ?? [];
        setLocalizaciones(
          rawL.map((l) => ({
            idLocalizacion: Number(l.idLocalizacion ?? l.id),
            nombreLocalizacion: String(l.nombreLocalizacion ?? l.nombre ?? ''),
            idCiudad: Number(l.idCiudad ?? 0) || undefined,
          })),
        );
      }
      if (ciRes.status === 'fulfilled') {
        const rawC = unwrapData<Record<string, unknown>[]>(ciRes.value) ?? [];
        setCiudades(rawC.map((c) => ({
          idCiudad: Number(c.idCiudad ?? c.id),
          idPais: Number(c.idPais ?? 0) || undefined,
        })));
      }
      if (rList.status === 'fulfilled') {
        setReservas(rList.value);
      } else {
        setReservas([]);
        throw rList.reason;
      }
      if (cRes.status === 'rejected') {
        setError(getErrorMessage(cRes.reason));
      }
    } catch (e) {
      setError(getErrorMessage(e));
      setReservas([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { loadAll(); }, [loadAll]));

  const filtered = useMemo(() => {
    let list = reservas;
    if (filterCliente) list = list.filter((r) => String(r.idCliente) === filterCliente);
    const q = search.trim().toLowerCase();
    if (q) list = list.filter((r) => (r.codigoReserva ?? '').toLowerCase().includes(q));
    return list;
  }, [reservas, search, filterCliente]);

  const pagination = useClientPagination(filtered, 10, `${search}-${filterCliente}`);

  const buscarCodigo = async () => {
    if (!search.trim()) { await loadAll(); return; }
    setLoading(true);
    try {
      const res = await reservasApi.getByCodigo(search.trim());
      const data = unwrapData<Reserva>(res);
      setReservas(data ? [data] : []);
    } catch {
      setReservas([]);
      void alertMessage('No encontrada', 'No se encontró la reserva');
    } finally {
      setLoading(false);
    }
  };

  const buscarPorCliente = async (id: string) => {
    setFilterCliente(id);
    if (!id) { await loadAll(); return; }
    setLoading(true);
    try {
      const res = await reservasApi.getByCliente(Number(id));
      setReservas(unwrapData<Reserva[]>(res) ?? []);
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
      setReservas([]);
    } finally {
      setLoading(false);
    }
  };

  const openCreate = () => {
    setEditing(null);
    setForm(defaultForm());
    setShowModal(true);
  };

  const openEdit = (r: Reserva) => {
    if (r.estadoReserva !== 'PENDIENTE') {
      void alertMessage('Info', 'Solo las reservas pendientes pueden editarse');
      return;
    }
    setEditing(r);
    setForm({
      idCliente: String(r.idCliente ?? ''),
      idVehiculo: String(r.idVehiculo ?? ''),
      idLocalizacionRecogida: String(r.idLocalizacionRecogida ?? ''),
      idLocalizacionDevolucion: String(r.idLocalizacionDevolucion ?? ''),
      canalReserva: r.canalReserva ?? 'WEB',
      fechaRecogida: parseIso(r.fechaHoraRecogida ?? r.fechaInicio),
      fechaDevolucion: parseIso(r.fechaHoraDevolucion ?? r.fechaFin),
    });
    setShowModal(true);
  };

  const validateForm = () => {
    if (!editing && (!form.idCliente || !form.idVehiculo)) {
      return 'Cliente y vehículo son requeridos';
    }
    if (!form.idLocalizacionRecogida || !form.idLocalizacionDevolucion) {
      return 'Selecciona localizaciones de recogida y devolución';
    }
    if (form.fechaDevolucion <= form.fechaRecogida) {
      return 'La devolución debe ser posterior a la recogida';
    }
    const p1 = getPaisByLocalizacion(form.idLocalizacionRecogida);
    const p2 = getPaisByLocalizacion(form.idLocalizacionDevolucion);
    if (p1 && p2 && p1 !== p2) return 'Recogida y devolución deben ser en el mismo país';
    return null;
  };

  const handleSave = async () => {
    const err = validateForm();
    if (err) { void alertMessage('Error', err); return; }
    setSaving(true);
    try {
      if (editing?.idReserva) {
        await reservasApi.update(editing.idReserva, {
          idVehiculo: Number(form.idVehiculo),
          idLocalizacionRecogida: Number(form.idLocalizacionRecogida),
          idLocalizacionDevolucion: Number(form.idLocalizacionDevolucion),
          fechaHoraRecogida: form.fechaRecogida.toISOString(),
          fechaHoraDevolucion: form.fechaDevolucion.toISOString(),
          canalReserva: form.canalReserva,
        });
        void alertMessage('Listo', 'Reserva actualizada');
      } else {
        const res = await reservasApi.create({
          idCliente: Number(form.idCliente),
          idVehiculo: Number(form.idVehiculo),
          idLocalizacionRecogida: Number(form.idLocalizacionRecogida),
          idLocalizacionDevolucion: Number(form.idLocalizacionDevolucion),
          canalReserva: form.canalReserva,
          fechaHoraRecogida: form.fechaRecogida.toISOString(),
          fechaHoraDevolucion: form.fechaDevolucion.toISOString(),
          extras: [],
          conductores: [{ usarClienteTitular: true, esPrincipal: true }],
        });
        const data = unwrapData<{ codigoReserva?: string }>(res);
        void alertMessage('Listo', `Reserva creada: ${data?.codigoReserva ?? 'OK'}`);
      }
      setShowModal(false);
      await loadAll();
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const confirmar = async (id: number) => {
    try {
      await reservasApi.confirmar(id);
      await loadAll();
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    }
  };

  const cancelar = async (id: number) => {
    const ok = await confirmAction('Cancelar', '¿Cancelar esta reserva?', {
      confirmLabel: 'Sí, cancelar',
      destructive: true,
    });
    if (!ok) return;
    try {
      await reservasApi.cancelar(id, 'Cancelado desde panel');
      await loadAll();
    } catch (e) {
      void alertMessage('Error', getErrorMessage(e));
    }
  };

  const locOptions = localizaciones.map((l) => ({
    value: String(l.idLocalizacion),
    label: l.nombreLocalizacion || `Sucursal ${l.idLocalizacion}`,
  }));

  return (
    <View style={styles.flex}>
      <FlatList
        style={styles.list}
        contentContainerStyle={styles.content}
        data={pagination.paginatedItems}
        keyExtractor={(item, i) => String(item.idReserva ?? item.codigoReserva ?? i)}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={async () => { setRefreshing(true); await loadAll(); setRefreshing(false); }}
            tintColor={colors.primary}
          />
        }
        ListHeaderComponent={
          <AdminScreen
            title="Reservas"
            subtitle={`${reservas.length} registros`}
            error={error}
            loading={loading && reservas.length === 0}
            search={search}
            onSearchChange={setSearch}
            searchPlaceholder="Buscar por código (Enter en web)…"
            actions={
              <View style={styles.headerActions}>
                <Button label="↻" variant="secondary" onPress={loadAll} style={styles.iconBtn} />
                <Button label="+ Nueva" onPress={openCreate} />
              </View>
            }
          >
            <Select
              label="Filtrar por cliente"
              value={filterCliente}
              onValueChange={buscarPorCliente}
              options={clientes.map((c) => ({
                value: String(c.idCliente),
                label: (c.nombreCompleto ?? `${c.nombres ?? ''} ${c.apellidos ?? ''}`.trim()) || `#${c.idCliente}`,
              }))}
              placeholder="Todos los clientes"
            />
            {search.trim() ? (
              <Button label="Buscar código" variant="secondary" onPress={buscarCodigo} style={{ marginBottom: spacing.md }} />
            ) : null}
            {!loading && filtered.length === 0 ? <EmptyState title="No hay reservas" icon="calendar-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item: r }) => (
          <Card style={styles.card}>
            <View style={styles.row}>
              <Text style={styles.code}>{r.codigoReserva ?? '—'}</Text>
              <StatusBadge label={r.estadoReserva ?? '—'} />
            </View>
            <Text style={styles.meta}>
              {r.nombreCliente ?? r.cliente ?? `Cliente #${r.idCliente ?? '—'}`}
            </Text>
            <Text style={styles.meta}>
              {r.vehiculo ?? r.placaVehiculo ?? `Vehículo #${r.idVehiculo ?? '—'}`}
            </Text>
            <Text style={styles.meta}>
              {(r.fechaHoraRecogida ?? r.fechaInicio ?? '').slice(0, 16)} → {(r.fechaHoraDevolucion ?? r.fechaFin ?? '').slice(0, 16)}
            </Text>
            {r.total != null ? <Text style={styles.total}>{formatCurrency(r.total)}</Text> : null}
            <View style={styles.actions}>
              {r.estadoReserva === 'PENDIENTE' && r.idReserva ? (
                <>
                  <Button label="Editar" variant="secondary" onPress={() => openEdit(r)} style={styles.btn} />
                  <Button label="Confirmar" onPress={() => confirmar(r.idReserva!)} style={styles.btn} />
                </>
              ) : null}
              {r.idReserva && r.estadoReserva !== 'CANCELADA' && r.estadoReserva !== 'COMPLETADA' ? (
                <Button label="Cancelar" variant="danger" onPress={() => cancelar(r.idReserva!)} style={styles.btn} />
              ) : null}
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

      <Modal
        visible={showModal}
        title={editing ? 'Editar reserva' : 'Nueva reserva'}
        onClose={() => setShowModal(false)}
      >
        <ScrollView showsVerticalScrollIndicator={false}>
          {!editing ? (
            <>
              <Select
                label="Cliente *"
                value={form.idCliente}
                onValueChange={(v) => setForm({ ...form, idCliente: v })}
                options={clientes.map((c) => ({
                  value: String(c.idCliente),
                  label: (c.nombreCompleto ?? `${c.nombres ?? ''} ${c.apellidos ?? ''}`.trim()) || `#${c.idCliente}`,
                }))}
              />
              <Select
                label="Vehículo *"
                value={form.idVehiculo}
                onValueChange={(v) => setForm({ ...form, idVehiculo: v })}
                options={vehiculos.map((v) => ({
                  value: String(v.idVehiculo),
                  label: `${v.placa ?? ''} · ${v.marca ?? ''} ${v.modelo ?? ''}`.trim(),
                }))}
              />
            </>
          ) : null}
          <Select
            label="Recogida *"
            value={form.idLocalizacionRecogida}
            onValueChange={(v) => setForm({ ...form, idLocalizacionRecogida: v })}
            options={locOptions}
          />
          <Select
            label="Devolución *"
            value={form.idLocalizacionDevolucion}
            onValueChange={(v) => setForm({ ...form, idLocalizacionDevolucion: v })}
            options={locOptions}
          />
          <DateTimeSelector label="Fecha recogida" value={form.fechaRecogida} onChange={(d) => setForm({ ...form, fechaRecogida: d })} accent="primary" />
          <DateTimeSelector label="Fecha devolución" value={form.fechaDevolucion} onChange={(d) => setForm({ ...form, fechaDevolucion: d })} minimumDate={form.fechaRecogida} accent="primary" />
          <Select
            label="Canal"
            value={form.canalReserva}
            onValueChange={(v) => setForm({ ...form, canalReserva: v })}
            options={[
              { label: 'Web', value: 'WEB' },
              { label: 'POS', value: 'POS' },
              { label: 'Teléfono', value: 'TELEFONO' },
            ]}
          />
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
  headerActions: { flexDirection: 'row', gap: spacing.sm },
  iconBtn: { minWidth: 44, paddingHorizontal: 12 },
  card: { ...shadows.sm },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', gap: spacing.md },
  code: { color: colors.primaryLight, fontFamily: fonts.bold, fontSize: 16 },
  meta: { color: colors.textSecondary, marginTop: 4, fontSize: 13 },
  total: { color: colors.text, fontFamily: fonts.bold, marginTop: spacing.sm },
  actions: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm, marginTop: spacing.md },
  btn: { flexGrow: 1, minWidth: 90, minHeight: 40, paddingVertical: 8 },
});
