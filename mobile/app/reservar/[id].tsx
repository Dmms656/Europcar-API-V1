import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import DateTimePicker from '@react-native-community/datetimepicker';
import { bookingApi } from '@/src/api/bookingApi';
import { colors } from '@/src/theme/colors';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

export default function ReservarScreen() {
  const { id, idLocalizacion: locParam } = useLocalSearchParams<{ id: string; idLocalizacion?: string }>();
  const idVehiculo = Number(id);
  const idLocalizacion = Number(locParam || 1);

  const [vehiculo, setVehiculo] = useState<{ marca?: string; modelo?: string } | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [fechaInicio, setFechaInicio] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() + 2);
    return d;
  });
  const [fechaFin, setFechaFin] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() + 5);
    return d;
  });
  const [showPicker, setShowPicker] = useState<'inicio' | 'fin' | null>(null);

  const [nombres, setNombres] = useState('');
  const [apellidos, setApellidos] = useState('');
  const [cedula, setCedula] = useState('');
  const [correo, setCorreo] = useState('');
  const [telefono, setTelefono] = useState('');

  useEffect(() => {
    bookingApi
      .getVehiculoDetalle(idVehiculo)
      .then((res) => setVehiculo(unwrapData(res)))
      .finally(() => setLoading(false));
  }, [idVehiculo]);

  const formatDate = (d: Date) => d.toISOString().slice(0, 10);

  const confirmar = async () => {
    if (!nombres.trim() || !cedula.trim() || !correo.trim() || !telefono.trim()) {
      Alert.alert('Datos incompletos', 'Completa nombre, cédula, correo y teléfono');
      return;
    }
    setSubmitting(true);
    try {
      const payload = {
        idVehiculo: String(idVehiculo),
        idLocalizacionRecogida: idLocalizacion,
        idLocalizacionEntrega: idLocalizacion,
        idLocalizacionDevolucion: idLocalizacion,
        fechaInicio: formatDate(fechaInicio),
        fechaFin: formatDate(fechaFin),
        horaInicio: '10:00:00',
        horaFin: '10:00:00',
        origenCanalReserva: 'MOBILE',
        cliente: {
          nombres: nombres.trim(),
          apellidos: apellidos.trim() || 'N/A',
          tipoIdentificacion: 'CEDULA',
          numeroIdentificacion: cedula.trim(),
          correo: correo.trim(),
          telefono: telefono.trim(),
        },
        conductorPrincipal: {
          nombres: nombres.trim(),
          apellidos: apellidos.trim() || 'N/A',
          tipoIdentificacion: 'CEDULA',
          numeroIdentificacion: cedula.trim(),
          correo: correo.trim(),
          telefono: telefono.trim(),
          numeroLicencia: 'PENDIENTE',
          fechaVencimientoLicencia: '2035-12-31',
          edadConductor: 25,
        },
        conductorSecundario: null,
        extras: [],
      };

      const res = await bookingApi.crearReserva(payload);
      const data = unwrapData<{ codigoReserva?: string; total?: number }>(res);
      Alert.alert(
        'Reserva confirmada',
        `Código: ${data?.codigoReserva ?? '—'}`,
        [{ text: 'OK', onPress: () => router.replace('/(tabs)/reservas') }]
      );
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.primary} size="large" />
      </View>
    );
  }

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <Text style={styles.title}>
        Reservar {vehiculo?.marca} {vehiculo?.modelo}
      </Text>

      <Pressable style={styles.dateBtn} onPress={() => setShowPicker('inicio')}>
        <Text style={styles.label}>Recogida</Text>
        <Text style={styles.dateValue}>{formatDate(fechaInicio)}</Text>
      </Pressable>
      <Pressable style={styles.dateBtn} onPress={() => setShowPicker('fin')}>
        <Text style={styles.label}>Devolución</Text>
        <Text style={styles.dateValue}>{formatDate(fechaFin)}</Text>
      </Pressable>

      {showPicker && (
        <DateTimePicker
          value={showPicker === 'inicio' ? fechaInicio : fechaFin}
          mode="date"
          minimumDate={new Date()}
          onChange={(_, date) => {
            setShowPicker(null);
            if (!date) return;
            if (showPicker === 'inicio') setFechaInicio(date);
            else setFechaFin(date);
          }}
        />
      )}

      <Text style={styles.section}>Datos del cliente</Text>
      {[
        { label: 'Nombres', value: nombres, set: setNombres },
        { label: 'Apellidos', value: apellidos, set: setApellidos },
        { label: 'Cédula', value: cedula, set: setCedula },
        { label: 'Correo', value: correo, set: setCorreo, keyboard: 'email-address' as const },
        { label: 'Teléfono', value: telefono, set: setTelefono, keyboard: 'phone-pad' as const },
      ].map((f) => (
        <TextInput
          key={f.label}
          style={styles.input}
          placeholder={f.label}
          placeholderTextColor={colors.textMuted}
          value={f.value}
          onChangeText={f.set}
          keyboardType={f.keyboard}
          autoCapitalize="none"
        />
      ))}

      <Pressable style={styles.cta} onPress={confirmar} disabled={submitting}>
        {submitting ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.ctaText}>Confirmar reserva</Text>
        )}
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  content: { padding: 20, paddingBottom: 40 },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
  title: { color: colors.text, fontSize: 22, fontWeight: '700', marginBottom: 20 },
  section: { color: colors.text, fontWeight: '600', marginTop: 16, marginBottom: 8 },
  dateBtn: {
    backgroundColor: colors.surface,
    padding: 14,
    borderRadius: 10,
    marginBottom: 10,
    borderWidth: 1,
    borderColor: colors.border,
  },
  label: { color: colors.textMuted, fontSize: 12 },
  dateValue: { color: colors.text, fontSize: 16, marginTop: 4 },
  input: {
    backgroundColor: colors.surface,
    borderColor: colors.border,
    borderWidth: 1,
    borderRadius: 10,
    padding: 12,
    color: colors.text,
    marginBottom: 10,
  },
  cta: {
    marginTop: 24,
    backgroundColor: colors.accent,
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
  },
  ctaText: { color: '#fff', fontWeight: '700', fontSize: 16 },
});
