import { useCallback, useState } from 'react';
import { Alert, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from 'expo-router';
import { catalogosApi } from '@/src/api/catalogosApi';
import { AdminScreen } from '@/src/components/admin/AdminScreen';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { Input } from '@/src/components/ui/Input';
import { Modal } from '@/src/components/ui/Modal';
import { StatusBadge } from '@/src/components/ui/StatusBadge';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

type Pais = { id?: number; codigo?: string; nombre?: string; estado?: string };
type Ciudad = { idCiudad?: number; id?: number; nombreCiudad?: string; nombre?: string; nombrePais?: string; estadoCiudad?: string };

export default function AdminUbicacionesScreen() {
  const isAdmin = useAuthStore((s) => s.hasAnyRole('ADMIN'));
  const [paises, setPaises] = useState<Pais[]>([]);
  const [ciudades, setCiudades] = useState<Ciudad[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [tab, setTab] = useState<'paises' | 'ciudades'>('paises');
  const [showPaisModal, setShowPaisModal] = useState(false);
  const [showCiudadModal, setShowCiudadModal] = useState(false);
  const [saving, setSaving] = useState(false);
  const [paisForm, setPaisForm] = useState({ codigoIso2: '', nombrePais: '' });
  const [ciudadForm, setCiudadForm] = useState({ idPais: '', nombreCiudad: '' });

  const load = useCallback(async () => {
    setError('');
    setLoading(true);
    try {
      const [rP, rC] = await Promise.all([catalogosApi.getPaises(), catalogosApi.getCiudades()]);
      setPaises(unwrapData<Pais[]>(rP) ?? []);
      setCiudades(unwrapData<Ciudad[]>(rC) ?? []);
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const savePais = async () => {
    if (!paisForm.nombrePais.trim()) {
      Alert.alert('Error', 'Nombre requerido');
      return;
    }
    setSaving(true);
    try {
      await catalogosApi.createPais({
        codigoIso2: paisForm.codigoIso2.trim().toUpperCase(),
        nombrePais: paisForm.nombrePais.trim(),
      });
      setShowPaisModal(false);
      setPaisForm({ codigoIso2: '', nombrePais: '' });
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const saveCiudad = async () => {
    if (!ciudadForm.nombreCiudad.trim() || !ciudadForm.idPais) {
      Alert.alert('Error', 'País y nombre requeridos');
      return;
    }
    setSaving(true);
    try {
      await catalogosApi.createCiudad({
        idPais: Number(ciudadForm.idPais),
        nombreCiudad: ciudadForm.nombreCiudad.trim(),
      });
      setShowCiudadModal(false);
      setCiudadForm({ idPais: '', nombreCiudad: '' });
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSaving(false);
    }
  };

  const togglePais = async (p: Pais) => {
    if (!p.id) return;
    const nuevo = p.estado === 'ACT' ? 'INA' : 'ACT';
    try {
      await catalogosApi.cambiarEstadoPais(p.id, nuevo);
      await load();
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    }
  };

  return (
    <ScrollView style={styles.scroll} contentContainerStyle={styles.content}>
      <AdminScreen title="Países y Ciudades" error={error} loading={loading}>
        <View style={styles.tabs}>
          <Pressable style={[styles.tab, tab === 'paises' && styles.tabActive]} onPress={() => setTab('paises')}>
            <Text style={[styles.tabText, tab === 'paises' && styles.tabTextActive]}>Países ({paises.length})</Text>
          </Pressable>
          <Pressable style={[styles.tab, tab === 'ciudades' && styles.tabActive]} onPress={() => setTab('ciudades')}>
            <Text style={[styles.tabText, tab === 'ciudades' && styles.tabTextActive]}>Ciudades ({ciudades.length})</Text>
          </Pressable>
        </View>

        {tab === 'paises' ? (
          <>
            {isAdmin ? <Button label="+ País" onPress={() => setShowPaisModal(true)} style={styles.addBtn} /> : null}
            {paises.map((p) => (
              <Card key={p.id}>
                <View style={styles.row}>
                  <Text style={styles.name}>{p.nombre}</Text>
                  <StatusBadge label={p.estado === 'ACT' ? 'ACTIVO' : 'INACTIVO'} />
                </View>
                <Text style={styles.meta}>{p.codigo}</Text>
                {isAdmin ? <Button label="Cambiar estado" variant="ghost" onPress={() => togglePais(p)} style={styles.smallBtn} /> : null}
              </Card>
            ))}
          </>
        ) : (
          <>
            {isAdmin ? <Button label="+ Ciudad" onPress={() => setShowCiudadModal(true)} style={styles.addBtn} /> : null}
            {ciudades.map((c) => (
              <Card key={c.idCiudad ?? c.id}>
                <Text style={styles.name}>{c.nombreCiudad ?? c.nombre}</Text>
                <Text style={styles.meta}>{c.nombrePais || '—'}</Text>
                <StatusBadge label={c.estadoCiudad === 'ACT' ? 'ACTIVA' : 'INACTIVA'} />
              </Card>
            ))}
          </>
        )}
      </AdminScreen>

      <Modal visible={showPaisModal} title="Nuevo país" onClose={() => setShowPaisModal(false)}>
        <Input label="Código ISO2" value={paisForm.codigoIso2} onChangeText={(v) => setPaisForm({ ...paisForm, codigoIso2: v })} maxLength={2} autoCapitalize="characters" />
        <Input label="Nombre" value={paisForm.nombrePais} onChangeText={(v) => setPaisForm({ ...paisForm, nombrePais: v })} />
        <Button label={saving ? 'Guardando…' : 'Guardar'} onPress={savePais} loading={saving} />
      </Modal>

      <Modal visible={showCiudadModal} title="Nueva ciudad" onClose={() => setShowCiudadModal(false)}>
        <Input label="ID País" value={ciudadForm.idPais} onChangeText={(v) => setCiudadForm({ ...ciudadForm, idPais: v })} keyboardType="number-pad" />
        <Input label="Nombre ciudad" value={ciudadForm.nombreCiudad} onChangeText={(v) => setCiudadForm({ ...ciudadForm, nombreCiudad: v })} />
        <Button label={saving ? 'Guardando…' : 'Guardar'} onPress={saveCiudad} loading={saving} />
      </Modal>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  scroll: { flex: 1, backgroundColor: colors.bg },
  content: { padding: spacing.lg, paddingBottom: spacing.xxl },
  tabs: { flexDirection: 'row', gap: spacing.sm, marginBottom: spacing.lg },
  tab: { flex: 1, padding: spacing.md, borderRadius: 8, backgroundColor: colors.surface, alignItems: 'center' },
  tabActive: { backgroundColor: colors.primaryGhost, borderWidth: 1, borderColor: colors.primary },
  tabText: { color: colors.textMuted, fontWeight: '600' },
  tabTextActive: { color: colors.primaryLight },
  addBtn: { marginBottom: spacing.md },
  row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  name: { color: colors.text, fontWeight: '700', fontSize: 16 },
  meta: { color: colors.textSecondary, marginTop: 4 },
  smallBtn: { marginTop: spacing.sm, minHeight: 36 },
});
