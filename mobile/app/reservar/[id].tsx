import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import DateTimePicker from '@react-native-community/datetimepicker';
import { router, useLocalSearchParams } from 'expo-router';
import { bookingApi } from '@/src/api/bookingApi';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { Input } from '@/src/components/ui/Input';
import { Screen } from '@/src/components/ui/Screen';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

function parseDate(value?: string, fallbackDays = 2) {
  if (value) {
    const d = new Date(`${value}T12:00:00`);
    if (!Number.isNaN(d.getTime())) return d;
  }
  const d = new Date();
  d.setDate(d.getDate() + fallbackDays);
  return d;
}

export default function ReservarScreen() {
  const { id, idLocalizacion: locParam, fechaRecogida: frParam, fechaDevolucion: fdParam } =
    useLocalSearchParams<{
      id: string;
      idLocalizacion?: string;
      fechaRecogida?: string;
      fechaDevolucion?: string;
    }>();

  const user = useAuthStore((s) => s.user);
  const idVehiculo = Number(id);
  const idLocalizacion = Number(locParam || 1);

  const [vehiculo, setVehiculo] = useState<{ marca?: string; modelo?: string } | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [fechaInicio, setFechaInicio] = useState(() => parseDate(frParam, 2));
  const [fechaFin, setFechaFin] = useState(() => parseDate(fdParam, 5));
  const [showPicker, setShowPicker] = useState<'inicio' | 'fin' | null>(null);

  const splitName = (full?: string) => {
    if (!full) return { nombres: '', apellidos: '' };
    const parts = full.trim().split(/\s+/);
    return { nombres: parts[0] ?? '', apellidos: parts.slice(1).join(' ') };
  };

  const fromUser = splitName(user?.nombreCompleto);
  const [nombres, setNombres] = useState(fromUser.nombres);
  const [apellidos, setApellidos] = useState(fromUser.apellidos);
  const [cedula, setCedula] = useState(user?.numeroIdentificacion ?? '');
  const [correo, setCorreo] = useState(user?.correo ?? '');
  const [telefono, setTelefono] = useState(user?.telefono ?? '');

  useEffect(() => {
    bookingApi
      .getVehiculoDetalle(idVehiculo)
      .then((res) => setVehiculo(unwrapData(res)))
      .finally(() => setLoading(false));
  }, [idVehiculo]);

  useEffect(() => {
    if (!user) return;
    const { nombres: n, apellidos: a } = splitName(user.nombreCompleto);
    if (n) setNombres(n);
    if (a) setApellidos(a);
    if (user.numeroIdentificacion) setCedula(user.numeroIdentificacion);
    if (user.correo) setCorreo(user.correo);
    if (user.telefono) setTelefono(user.telefono);
  }, [user]);

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
          numeroLicencia: cedula.trim(),
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
        [{ text: 'OK', onPress: () => router.replace('/(tabs)/reservas') }],
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
    <Screen>
      <Text style={styles.title}>
        Reservar {vehiculo?.marca} {vehiculo?.modelo}
      </Text>

      <Card>
        <Pressable style={styles.dateBtn} onPress={() => setShowPicker('inicio')}>
          <Text style={styles.label}>Recogida</Text>
          <Text style={styles.dateValue}>{formatDate(fechaInicio)}</Text>
        </Pressable>
        <Pressable style={styles.dateBtn} onPress={() => setShowPicker('fin')}>
          <Text style={styles.label}>Devolución</Text>
          <Text style={styles.dateValue}>{formatDate(fechaFin)}</Text>
        </Pressable>

        {showPicker ? (
          <DateTimePicker
            value={showPicker === 'inicio' ? fechaInicio : fechaFin}
            mode="date"
            minimumDate={new Date()}
            display={Platform.OS === 'ios' ? 'spinner' : 'default'}
            onChange={(_, date) => {
              if (Platform.OS === 'android') setShowPicker(null);
              if (!date) return;
              if (showPicker === 'inicio') setFechaInicio(date);
              else setFechaFin(date);
              if (Platform.OS === 'ios') setShowPicker(null);
            }}
          />
        ) : null}
      </Card>

      <Text style={styles.section}>Datos del cliente</Text>
      <Input label="Nombres" value={nombres} onChangeText={setNombres} />
      <Input label="Apellidos" value={apellidos} onChangeText={setApellidos} />
      <Input label="Cédula" value={cedula} onChangeText={setCedula} />
      <Input label="Correo" value={correo} onChangeText={setCorreo} keyboardType="email-address" autoCapitalize="none" />
      <Input label="Teléfono" value={telefono} onChangeText={setTelefono} keyboardType="phone-pad" />

      <Button label="Confirmar reserva" onPress={confirmar} loading={submitting} variant="client" style={{ marginTop: spacing.md }} />
    </Screen>
  );
}

const styles = StyleSheet.create({
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
  title: { color: colors.text, fontSize: 22, fontWeight: '700', marginBottom: spacing.md },
  section: { color: colors.text, fontWeight: '600', marginBottom: spacing.sm },
  dateBtn: {
    backgroundColor: colors.bgSecondary,
    padding: spacing.md,
    borderRadius: radius.md,
    marginBottom: spacing.sm,
    borderWidth: 1,
    borderColor: colors.border,
  },
  label: { color: colors.textMuted, fontSize: 12 },
  dateValue: { color: colors.text, fontSize: 16, marginTop: 4, fontWeight: '600' },
});
