import { useCallback, useMemo, useState } from 'react';
import { Alert, FlatList, Pressable, RefreshControl, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { catalogosApi } from '@/src/api/catalogosApi';
import { vehiculosApi } from '@/src/api/vehiculosApi';
import { AdminScreen } from '@/src/components/admin/AdminScreen';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { EmptyState } from '@/src/components/ui/EmptyState';
import { ImageUploader } from '@/src/components/ui/ImageUploader';
import { Input } from '@/src/components/ui/Input';
import { Modal } from '@/src/components/ui/Modal';
import { PaginationControls } from '@/src/components/ui/PaginationControls';
import { Select, type SelectOption } from '@/src/components/ui/Select';
import { StatusBadge } from '@/src/components/ui/StatusBadge';
import { useClientPagination } from '@/src/hooks/useClientPagination';
import { colors } from '@/src/theme/colors';
import { radius, shadows, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';
import { formatCurrency } from '@/src/utils/format';

type Vehiculo = {
  idVehiculo: number;
  codigoInterno?: string;
  placa?: string;
  marca?: string;
  modelo?: string;
  anioFabricacion?: number;
  categoria?: string;
  precioBaseDia?: number;
  estadoOperativo?: string;
  localizacion?: string;
  idMarca?: number;
  idCategoria?: number;
  color?: string;
  tipoCombustible?: string;
  tipoTransmision?: string;
  capacidadPasajeros?: number;
  capacidadMaletas?: number;
  numeroPuertas?: number;
  idLocalizacion?: number;
  kilometrajeActual?: number;
  aireAcondicionado?: boolean;
  observacionesGenerales?: string;
  imagenReferencialUrl?: string;
  rowVersion?: string;
};

const ESTADOS = ['DISPONIBLE', 'RESERVADO', 'MANTENIMIENTO', 'TALLER', 'FUERA_SERVICIO'];

const INITIAL_FORM = {
  placaVehiculo: '', idMarca: '', idCategoria: '', modeloVehiculo: '', anioFabricacion: '2024',
  colorVehiculo: '', tipoCombustible: 'GASOLINA', tipoTransmision: 'AUTOMATICA',
  capacidadPasajeros: '5', capacidadMaletas: '2', numeroPuertas: '4', idLocalizacion: '',
  precioBaseDia: '', kilometrajeActual: '0', aireAcondicionado: true,
  observacionesGenerales: '', imagenReferencialUrl: '',
};

export default function AdminVehiculosScreen() {
  const [vehiculos, setVehiculos] = useState<Vehiculo[]>([]);
  const [marcas, setMarcas] = useState<SelectOption[]>([]);
  const [categorias, setCategorias] = useState<SelectOption[]>([]);
  const [localizaciones, setLocalizaciones] = useState<SelectOption[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState(INITIAL_FORM);
  const [saving, setSaving] = useState(false);

  const loadAll = useCallback(async () => {
    setError('');
    try {
      const [vRes, mRes, cRes, lRes] = await Promise.all([
        vehiculosApi.getAll(),
        catalogosApi.getMarcas(),
        catalogosApi.getCategorias(),
        catalogosApi.getLocalizaciones(),
      ]);
      setVehiculos(unwrapData<Vehiculo[]>(vRes) ?? []);
      const rawM = unwrapData<Record<string, unknown>[]>(mRes) ?? [];
      const rawC = unwrapData<Record<string, unknown>[]>(cRes) ?? [];
      const rawL = unwrapData<Record<string, unknown>[]>(lRes) ?? [];
      setMarcas(rawM.map((m) => ({
        value: String(m.idMarcaVehiculo ?? m.id),
        label: String(m.nombreMarca ?? m.nombre ?? ''),
      })));
      setCategorias(rawC.map((c) => ({
        value: String(c.idCategoriaVehiculo ?? c.id),
        label: String(c.nombreCategoria ?? c.nombre ?? ''),
      })));
      setLocalizaciones(rawL.map((l) => ({
        value: String(l.idLocalizacion ?? l.id),
        label: String(l.nombreLocalizacion ?? l.nombre ?? ''),
      })));
    } catch (e) {
      setError(getErrorMessage(e));
      setVehiculos([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { loadAll(); }, [loadAll]));

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim();
    if (!q) return vehiculos;
    return vehiculos.filter((v) =>
      `${v.placa} ${v.marca} ${v.modelo} ${v.codigoInterno}`.toLowerCase().includes(q),
    );
  }, [vehiculos, search]);

  const pagination = useClientPagination(filtered, 10, search);

  const openCreate = () => {
    setForm(INITIAL_FORM);
    setEditingId(null);
    setShowModal(true);
  };

  const openEdit = (v: Vehiculo) => {
    setForm({
      placaVehiculo: v.placa ?? '',
      idMarca: String(v.idMarca ?? ''),
      idCategoria: String(v.idCategoria ?? ''),
      modeloVehiculo: v.modelo ?? '',
      anioFabricacion: String(v.anioFabricacion ?? 2024),
      colorVehiculo: v.color ?? '',
      tipoCombustible: v.tipoCombustible ?? 'GASOLINA',
      tipoTransmision: v.tipoTransmision ?? 'AUTOMATICA',
      capacidadPasajeros: String(v.capacidadPasajeros ?? 5),
      capacidadMaletas: String(v.capacidadMaletas ?? 2),
      numeroPuertas: String(v.numeroPuertas ?? 4),
      idLocalizacion: String(v.idLocalizacion ?? ''),
      precioBaseDia: String(v.precioBaseDia ?? ''),
      kilometrajeActual: String(v.kilometrajeActual ?? 0),
      aireAcondicionado: v.aireAcondicionado ?? true,
      observacionesGenerales: v.observacionesGenerales ?? '',
      imagenReferencialUrl: v.imagenReferencialUrl ?? '',
    });
    setEditingId(v.idVehiculo);
    setShowModal(true);
  };

  const handleSave = async () => {
    if (!form.placaVehiculo.trim() || !form.idMarca || !form.idCategoria || !form.idLocalizacion) {
      Alert.alert('Error', 'Placa, marca, categoría y localización son requeridos');
      return;
    }
    setSaving(true);
    try {
      const payload: Record<string, unknown> = {
        ...form,
        idMarca: Number(form.idMarca),
        idCategoria: Number(form.idCategoria),
        idLocalizacion: Number(form.idLocalizacion),
        anioFabricacion: Number(form.anioFabricacion),
        capacidadPasajeros: Number(form.capacidadPasajeros),
        capacidadMaletas: Number(form.capacidadMaletas),
        numeroPuertas: Number(form.numeroPuertas),
        precioBaseDia: Number(form.precioBaseDia),
        kilometrajeActual: Number(form.kilometrajeActual),
      };
      if (editingId) {
        await vehiculosApi.update(editingId, payload);
        Alert.alert('Listo', 'Vehículo actualizado');
      } else {
        await vehiculosApi.create(payload);
        Alert.alert('Listo', 'Vehículo creado');
      }
      setShowModal(false);
      await loadAll();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = (id: number) => {
    Alert.alert('Eliminar', '¿Eliminar este vehículo?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar',
        style: 'destructive',
        onPress: async () => {
          try {
            await vehiculosApi.delete(id);
            await loadAll();
          } catch (e) {
            Alert.alert('Error', getErrorMessage(e));
          }
        },
      },
    ]);
  };

  const changeEstado = async (v: Vehiculo, estado: string) => {
    if (v.estadoOperativo === 'ALQUILADO') {
      Alert.alert('Error', 'ALQUILADO no es editable manualmente');
      return;
    }
    try {
      await vehiculosApi.cambiarEstadoOperativo(v.idVehiculo, estado);
      await loadAll();
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
        keyExtractor={(item) => String(item.idVehiculo)}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={async () => { setRefreshing(true); await loadAll(); setRefreshing(false); }}
            tintColor={colors.primary}
          />
        }
        ListHeaderComponent={
          <AdminScreen
            title="Vehículos"
            subtitle={`${vehiculos.length} en la flota`}
            error={error}
            loading={loading && vehiculos.length === 0}
            search={search}
            onSearchChange={setSearch}
            searchPlaceholder="Buscar placa, marca o modelo…"
            actions={<Button label="+ Nuevo" onPress={openCreate} />}
          >
            {!loading && filtered.length === 0 ? <EmptyState title="No hay vehículos" icon="car-outline" /> : null}
          </AdminScreen>
        }
        renderItem={({ item: v }) => (
          <Card style={styles.card}>
            <View style={styles.row}>
              <View style={{ flex: 1 }}>
                <Text style={styles.placa}>{v.placa}</Text>
                <Text style={styles.model}>{v.marca} {v.modelo} · {v.anioFabricacion}</Text>
                <Text style={styles.meta}>{v.codigoInterno} · {v.categoria}</Text>
              </View>
              <StatusBadge label={v.estadoOperativo ?? '—'} />
            </View>
            <View style={styles.footer}>
              <Text style={styles.price}>{formatCurrency(v.precioBaseDia)}/día</Text>
              <Text style={styles.loc}>{v.localizacion ?? '—'}</Text>
            </View>
            {v.estadoOperativo !== 'ALQUILADO' ? (
              <Select
                label="Estado operativo"
                value={v.estadoOperativo ?? ''}
                onValueChange={(est) => est && changeEstado(v, est)}
                options={ESTADOS.map((e) => ({ label: e, value: e }))}
              />
            ) : null}
            <View style={styles.actions}>
              <Button label="Editar" variant="secondary" onPress={() => openEdit(v)} style={styles.btn} />
              <Button label="Eliminar" variant="danger" onPress={() => handleDelete(v.idVehiculo)} style={styles.btn} />
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

      <Modal visible={showModal} title={editingId ? 'Editar vehículo' : 'Nuevo vehículo'} onClose={() => setShowModal(false)}>
        <ScrollView showsVerticalScrollIndicator={false}>
          <Input label="Placa *" value={form.placaVehiculo} onChangeText={(v) => setForm({ ...form, placaVehiculo: v })} autoCapitalize="characters" />
          <Select label="Marca *" value={form.idMarca} onValueChange={(v) => setForm({ ...form, idMarca: v })} options={marcas} />
          <Select label="Categoría *" value={form.idCategoria} onValueChange={(v) => setForm({ ...form, idCategoria: v })} options={categorias} />
          <Input label="Modelo *" value={form.modeloVehiculo} onChangeText={(v) => setForm({ ...form, modeloVehiculo: v })} />
          <Input label="Año" value={form.anioFabricacion} onChangeText={(v) => setForm({ ...form, anioFabricacion: v })} keyboardType="number-pad" />
          <Input label="Color" value={form.colorVehiculo} onChangeText={(v) => setForm({ ...form, colorVehiculo: v })} />
          <Select label="Combustible" value={form.tipoCombustible} onValueChange={(v) => setForm({ ...form, tipoCombustible: v })} options={[
            { label: 'Gasolina', value: 'GASOLINA' }, { label: 'Diesel', value: 'DIESEL' },
            { label: 'Híbrido', value: 'HIBRIDO' }, { label: 'Eléctrico', value: 'ELECTRICO' },
          ]} />
          <Select label="Transmisión" value={form.tipoTransmision} onValueChange={(v) => setForm({ ...form, tipoTransmision: v })} options={[
            { label: 'Automática', value: 'AUTOMATICA' }, { label: 'Manual', value: 'MANUAL' },
          ]} />
          <Select label="Localización *" value={form.idLocalizacion} onValueChange={(v) => setForm({ ...form, idLocalizacion: v })} options={localizaciones} />
          <Input label="Pasajeros" value={form.capacidadPasajeros} onChangeText={(v) => setForm({ ...form, capacidadPasajeros: v })} keyboardType="number-pad" />
          <Input label="Precio/día *" value={form.precioBaseDia} onChangeText={(v) => setForm({ ...form, precioBaseDia: v })} keyboardType="decimal-pad" />
          <Input label="Kilometraje" value={form.kilometrajeActual} onChangeText={(v) => setForm({ ...form, kilometrajeActual: v })} keyboardType="number-pad" />
          <ImageUploader value={form.imagenReferencialUrl} onChange={(url) => setForm({ ...form, imagenReferencialUrl: url })} />
          <Input label="Observaciones" value={form.observacionesGenerales} onChangeText={(v) => setForm({ ...form, observacionesGenerales: v })} />
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
  placa: { color: colors.text, fontFamily: fonts.bold, fontSize: 17 },
  model: { color: colors.text, marginTop: 4, fontFamily: fonts.medium },
  meta: { color: colors.textMuted, fontSize: 12, marginTop: 2 },
  footer: { flexDirection: 'row', justifyContent: 'space-between', marginTop: spacing.md, marginBottom: spacing.sm },
  price: { color: colors.primaryLight, fontFamily: fonts.bold },
  loc: { color: colors.textSecondary, fontSize: 13 },
  actions: { flexDirection: 'row', gap: spacing.sm, marginTop: spacing.sm },
  btn: { flex: 1, minHeight: 42, paddingVertical: 8 },
});
