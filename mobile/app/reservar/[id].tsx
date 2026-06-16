import { useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Image,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import DateTimePicker from '@react-native-community/datetimepicker';
import { router, useLocalSearchParams } from 'expo-router';
import { bookingApi } from '@/src/api/bookingApi';
import { reservasApi } from '@/src/api/reservasApi';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { Input } from '@/src/components/ui/Input';
import { Screen } from '@/src/components/ui/Screen';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';
import {
  defaultRentalDateTimeLocalRange,
  getPayload,
  guestFormFromUserProfile,
  normalizeContactoReserva,
  normalizeVehiculoDetalle,
  toDateTimeLocalValue,
  type VehiculoBooking,
} from '@/src/utils/bookingNormalize';

const STEPS = ['Identificación', 'Fechas', 'Extras', 'Resumen', 'Pago'] as const;
const IVA_RATE = 0.15;

type ExtraItem = {
  idExtra?: number;
  id?: number;
  nombreExtra?: string;
  nombre?: string;
  descripcionExtra?: string;
  descripcion?: string;
  valorFijo?: number;
  precio?: number;
};

type ExtraSel = { id: number; nombre: string; valorFijo: number; cantidad: number };

type Localizacion = { idLocalizacion?: number; id?: number; nombre?: string; nombreLocalizacion?: string };

function parseDateTimeLocal(value: string) {
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? new Date() : d;
}

function resolveCliente(user: ReturnType<typeof useAuthStore.getState>['user'], guestForm: {
  nombre: string; apellido: string; cedula: string; correo: string; telefono: string;
}) {
  const contacto = normalizeContactoReserva(user, guestForm);
  const cedula = guestForm.cedula.trim() || (user?.numeroIdentificacion || '').trim();
  const nombre = guestForm.nombre.trim() || user?.nombreCompleto?.split(/\s+/)[0] || user?.username || '';
  const apellido = guestForm.apellido.trim() || 'N/A';
  return { nombre, apellido, cedula, ...contacto };
}

export default function ReservarScreen() {
  const { id, idLocalizacion: locParam } = useLocalSearchParams<{ id: string; idLocalizacion?: string }>();
  const idVehiculo = Number(id);
  const user = useAuthStore((s) => s.user);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const refreshProfile = useAuthStore((s) => s.refreshProfile);

  const defaults = defaultRentalDateTimeLocalRange();
  const skipIdentificacion = Boolean((user?.numeroIdentificacion || '').trim());

  const [step, setStep] = useState(skipIdentificacion ? 1 : 0);
  const [vehiculo, setVehiculo] = useState<VehiculoBooking | null>(null);
  const [localizaciones, setLocalizaciones] = useState<Localizacion[]>([]);
  const [extrasCatalog, setExtrasCatalog] = useState<ExtraItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [guestProcessing, setGuestProcessing] = useState(false);
  const [checkingDisp, setCheckingDisp] = useState(false);
  const [dispBlocked, setDispBlocked] = useState(false);
  const [dispMsg, setDispMsg] = useState('');
  const [picker, setPicker] = useState<'recogida' | 'devolucion' | null>(null);
  const [confirmada, setConfirmada] = useState<{ codigo: string; total: number } | null>(null);

  const [guestForm, setGuestForm] = useState({
    nombre: '', apellido: '', cedula: '', correo: '', telefono: '', direccion: '',
  });

  const [fechaRecogida, setFechaRecogida] = useState(parseDateTimeLocal(defaults.fechaRecogida));
  const [fechaDevolucion, setFechaDevolucion] = useState(parseDateTimeLocal(defaults.fechaDevolucion));
  const [idLocRecogida, setIdLocRecogida] = useState(String(locParam || ''));
  const [idLocDevolucion, setIdLocDevolucion] = useState(String(locParam || ''));
  const [extrasSel, setExtrasSel] = useState<ExtraSel[]>([]);

  const [pago, setPago] = useState({
    nombreTitular: '',
    numeroTarjeta: '',
    mesExpiracion: '',
    anioExpiracion: '',
    cvv: '',
  });

  useEffect(() => {
    (async () => {
      setLoading(true);
      try {
        const [vehRes, locRes, extRes] = await Promise.allSettled([
          bookingApi.getVehiculoDetalle(idVehiculo),
          bookingApi.getLocalizaciones({ page: 1, limit: 100 }),
          bookingApi.getExtras(),
        ]);

        if (vehRes.status === 'fulfilled') {
          const payload = getPayload<{ vehiculo?: Record<string, unknown>; Vehiculo?: Record<string, unknown> }>(vehRes.value);
          const raw = payload.vehiculo ?? payload.Vehiculo ?? null;
          const found = normalizeVehiculoDetalle(raw);
          setVehiculo(found);
          if (found?.idLocalizacion && !locParam) {
            setIdLocRecogida(String(found.idLocalizacion));
            setIdLocDevolucion(String(found.idLocalizacion));
          }
        }
        if (locRes.status === 'fulfilled') {
          const ld = getPayload<{ localizaciones?: Localizacion[]; items?: Localizacion[] }>(locRes.value);
          setLocalizaciones(ld.localizaciones ?? ld.items ?? []);
        }
        if (extRes.status === 'fulfilled') {
          const ed = getPayload<{ extras?: ExtraItem[] }>(extRes.value);
          setExtrasCatalog(ed.extras ?? (Array.isArray(ed) ? (ed as ExtraItem[]) : []));
        }
      } finally {
        setLoading(false);
      }
    })();
  }, [idVehiculo, locParam]);

  useEffect(() => {
    if (!isAuthenticated) return;
    refreshProfile().then(() => {
      const u = useAuthStore.getState().user;
      const profile = guestFormFromUserProfile(u);
      if (profile) setGuestForm((p) => ({ ...p, ...profile }));
      if ((u?.numeroIdentificacion || '').trim()) setStep((s) => (s === 0 ? 1 : s));
    });
  }, [isAuthenticated, refreshProfile]);

  const dias = useMemo(() => {
    const diff = Math.ceil((fechaDevolucion.getTime() - fechaRecogida.getTime()) / (1000 * 60 * 60 * 24));
    return Math.max(diff, 1);
  }, [fechaRecogida, fechaDevolucion]);

  const precioBase = Number(vehiculo?.precioBaseDia ?? vehiculo?.precioDia ?? 0);
  const subtotalVehiculo = precioBase * dias;
  const subtotalExtras = extrasSel.reduce((acc, ex) => acc + ex.valorFijo * ex.cantidad * dias, 0);
  const subtotal = subtotalVehiculo + subtotalExtras;
  const iva = subtotal * IVA_RATE;
  const isOneWay = idLocRecogida !== idLocDevolucion;
  const cargoOneWay = isOneWay ? 25 : 0;
  const totalFinal = subtotal + iva + cargoOneWay;

  const checkDisponibilidad = async () => {
    if (!vehiculo) return true;
    setCheckingDisp(true);
    setDispBlocked(false);
    setDispMsg('');
    try {
      const fr = toDateTimeLocalValue(fechaRecogida);
      const fd = toDateTimeLocalValue(fechaDevolucion);
      const res = await bookingApi.checkDisponibilidad(String(idVehiculo), {
        idLocalizacion: Number(idLocRecogida),
        fechaRecogida: fr,
        fechaDevolucion: fd,
      });
      const payload = getPayload<{ disponibilidad?: { disponible?: boolean; mensaje?: string } }>(res);
      const disp = payload.disponibilidad ?? {};
      const ok = Boolean(disp.disponible ?? true);
      if (!ok) {
        setDispBlocked(true);
        setDispMsg(String(disp.mensaje ?? 'Vehículo no disponible en esas fechas'));
      }
      return ok;
    } catch {
      setDispBlocked(true);
      setDispMsg('No se pudo validar disponibilidad');
      return false;
    } finally {
      setCheckingDisp(false);
    }
  };

  const toggleExtra = (extra: ExtraItem) => {
    const eid = Number(extra.idExtra ?? extra.id);
    if (!eid) return;
    setExtrasSel((prev) => {
      const exists = prev.find((e) => e.id === eid);
      if (exists) return prev.filter((e) => e.id !== eid);
      return [
        ...prev,
        {
          id: eid,
          nombre: extra.nombreExtra ?? extra.nombre ?? 'Extra',
          valorFijo: Number(extra.valorFijo ?? extra.precio ?? 0),
          cantidad: 1,
        },
      ];
    });
  };

  const handleGuestContinue = async () => {
    if (!guestForm.nombre.trim() || !guestForm.cedula.trim()) {
      Alert.alert('Datos requeridos', 'Nombre y cédula son obligatorios');
      return;
    }
    setGuestProcessing(true);
    try {
      await reservasApi.guestClient(guestForm);
      setStep(1);
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setGuestProcessing(false);
    }
  };

  const handleNext = async () => {
    const current = STEPS[step];
    if (current === 'Identificación') {
      await handleGuestContinue();
      return;
    }
    if (current === 'Fechas') {
      if (fechaDevolucion <= fechaRecogida) {
        Alert.alert('Fechas inválidas', 'La devolución debe ser posterior a la recogida');
        return;
      }
      if (!idLocRecogida || !idLocDevolucion) {
        Alert.alert('Sucursales', 'Selecciona sucursal de recogida y devolución');
        return;
      }
      const ok = await checkDisponibilidad();
      if (!ok) {
        Alert.alert('No disponible', dispMsg || 'El vehículo no está disponible');
        return;
      }
    }
    if (current === 'Pago') {
      await handlePagar();
      return;
    }
    setStep((s) => Math.min(s + 1, STEPS.length - 1));
  };

  const handlePagar = async () => {
    if (!pago.nombreTitular.trim() || pago.numeroTarjeta.replace(/\D/g, '').length < 16) {
      Alert.alert('Pago', 'Completa los datos de la tarjeta');
      return;
    }
    if (!pago.mesExpiracion || !pago.anioExpiracion || pago.cvv.length < 3) {
      Alert.alert('Pago', 'Completa fecha de expiración y CVV');
      return;
    }

    const cliente = resolveCliente(user, guestForm);
    if (!cliente.nombre || !cliente.cedula) {
      Alert.alert('Identificación', 'Indica tu cédula antes de pagar');
      setStep(0);
      return;
    }

    setProcessing(true);
    try {
      const horaInicio = `${String(fechaRecogida.getHours()).padStart(2, '0')}:${String(fechaRecogida.getMinutes()).padStart(2, '0')}:00`;
      const horaFin = `${String(fechaDevolucion.getHours()).padStart(2, '0')}:${String(fechaDevolucion.getMinutes()).padStart(2, '0')}:00`;

      const payload = {
        idVehiculo: String(idVehiculo),
        idLocalizacionRecogida: Number(idLocRecogida),
        idLocalizacionEntrega: Number(idLocDevolucion),
        idLocalizacionDevolucion: Number(idLocDevolucion),
        fechaInicio: toDateTimeLocalValue(fechaRecogida).slice(0, 10),
        fechaFin: toDateTimeLocalValue(fechaDevolucion).slice(0, 10),
        horaInicio,
        horaFin,
        origenCanalReserva: 'MOBILE',
        cliente: {
          nombres: cliente.nombre,
          apellidos: cliente.apellido,
          tipoIdentificacion: 'CEDULA',
          numeroIdentificacion: cliente.cedula,
          correo: cliente.correo,
          telefono: cliente.telefono,
        },
        conductorPrincipal: {
          nombres: cliente.nombre,
          apellidos: cliente.apellido,
          tipoIdentificacion: 'CEDULA',
          numeroIdentificacion: cliente.cedula,
          correo: cliente.correo,
          telefono: cliente.telefono,
          numeroLicencia: cliente.cedula,
          fechaVencimientoLicencia: '2035-12-31',
          edadConductor: 25,
        },
        conductorSecundario: null,
        extras: extrasSel.map((ex) => ({ idExtra: ex.id, cantidad: ex.cantidad })),
      };

      const res = await bookingApi.crearReserva(payload);
      const data = unwrapData<{ codigoReserva?: string; total?: number }>(res);
      setConfirmada({
        codigo: data?.codigoReserva ?? `RES-${Date.now().toString(36).toUpperCase()}`,
        total: data?.total ?? totalFinal,
      });
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
    } finally {
      setProcessing(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.accent} size="large" />
        <Text style={styles.muted}>Cargando vehículo…</Text>
      </View>
    );
  }

  if (confirmada) {
    return (
      <Screen scroll={false} style={styles.confirmWrap}>
        <Text style={styles.confirmIcon}>✅</Text>
        <Text style={styles.confirmTitle}>¡Reserva confirmada!</Text>
        <Text style={styles.confirmCode}>{confirmada.codigo}</Text>
        <Text style={styles.muted}>Pago simulado exitoso · ${confirmada.total.toFixed(2)} USD</Text>
        <Button label="Ver mis reservas" onPress={() => router.replace('/(tabs)/reservas')} variant="client" style={{ marginTop: spacing.xl, width: '100%' }} />
        <Button label="Volver al catálogo" onPress={() => router.replace('/(tabs)/catalogo')} variant="ghost" style={{ marginTop: spacing.sm, width: '100%' }} />
      </Screen>
    );
  }

  const titulo = `${vehiculo?.marca ?? ''} ${vehiculo?.modelo ?? ''}`.trim();

  return (
    <Screen>
      <View style={styles.stepBar}>
        {STEPS.map((label, i) => (
          <View key={label} style={[styles.stepDot, i <= step && styles.stepDotActive]}>
            <Text style={[styles.stepDotText, i <= step && styles.stepDotTextActive]}>{i + 1}</Text>
          </View>
        ))}
      </View>
      <Text style={styles.stepLabel}>{STEPS[step]}</Text>

      <Card style={styles.vehicleCard}>
        {vehiculo?.imagenUrl ? (
          <Image source={{ uri: vehiculo.imagenUrl }} style={styles.vehicleImg} />
        ) : null}
        <Text style={styles.vehicleTitle}>{titulo || `Vehículo #${idVehiculo}`}</Text>
        <Text style={styles.muted}>${precioBase.toFixed(2)}/día · {vehiculo?.categoria ?? '—'}</Text>
      </Card>

      {STEPS[step] === 'Identificación' && (
        <View>
          <Text style={styles.desc}>Identifícate para continuar con la reserva</Text>
          <Input label="Nombres" value={guestForm.nombre} onChangeText={(v) => setGuestForm({ ...guestForm, nombre: v })} />
          <Input label="Apellidos" value={guestForm.apellido} onChangeText={(v) => setGuestForm({ ...guestForm, apellido: v })} />
          <Input label="Cédula / Pasaporte" value={guestForm.cedula} onChangeText={(v) => setGuestForm({ ...guestForm, cedula: v })} />
          <Input label="Correo" value={guestForm.correo} onChangeText={(v) => setGuestForm({ ...guestForm, correo: v })} keyboardType="email-address" autoCapitalize="none" />
          <Input label="Teléfono" value={guestForm.telefono} onChangeText={(v) => setGuestForm({ ...guestForm, telefono: v })} keyboardType="phone-pad" />
        </View>
      )}

      {STEPS[step] === 'Fechas' && (
        <View>
          <Text style={styles.desc}>Selecciona cuándo recoges y devuelves el vehículo</Text>
          <Pressable style={styles.dateRow} onPress={() => setPicker('recogida')}>
            <Text style={styles.dateLabel}>Recogida</Text>
            <Text style={styles.dateValue}>{toDateTimeLocalValue(fechaRecogida).replace('T', ' ')}</Text>
          </Pressable>
          <Pressable style={styles.dateRow} onPress={() => setPicker('devolucion')}>
            <Text style={styles.dateLabel}>Devolución</Text>
            <Text style={styles.dateValue}>{toDateTimeLocalValue(fechaDevolucion).replace('T', ' ')}</Text>
          </Pressable>
          {picker ? (
            <DateTimePicker
              value={picker === 'recogida' ? fechaRecogida : fechaDevolucion}
              mode="datetime"
              minimumDate={new Date()}
              display={Platform.OS === 'ios' ? 'spinner' : 'default'}
              onChange={(_, date) => {
                if (Platform.OS === 'android') setPicker(null);
                if (!date) return;
                if (picker === 'recogida') setFechaRecogida(date);
                else setFechaDevolucion(date);
                if (Platform.OS === 'ios') setPicker(null);
              }}
            />
          ) : null}

          <Text style={styles.section}>Sucursal recogida</Text>
          {localizaciones.map((loc) => {
            const lid = String(loc.idLocalizacion ?? loc.id);
            return (
              <Pressable
                key={lid}
                style={[styles.chip, idLocRecogida === lid && styles.chipActive]}
                onPress={() => setIdLocRecogida(lid)}
              >
                <Text style={styles.chipText}>{loc.nombre ?? loc.nombreLocalizacion}</Text>
              </Pressable>
            );
          })}

          <Text style={styles.section}>Sucursal devolución</Text>
          {localizaciones.map((loc) => {
            const lid = String(loc.idLocalizacion ?? loc.id);
            return (
              <Pressable
                key={`d-${lid}`}
                style={[styles.chip, idLocDevolucion === lid && styles.chipActive]}
                onPress={() => setIdLocDevolucion(lid)}
              >
                <Text style={styles.chipText}>{loc.nombre ?? loc.nombreLocalizacion}</Text>
              </Pressable>
            );
          })}

          {checkingDisp ? <Text style={styles.muted}>Validando disponibilidad…</Text> : null}
          {dispBlocked ? <Text style={styles.error}>{dispMsg}</Text> : null}
          <Text style={styles.muted}>{dias} día(s) · ${subtotalVehiculo.toFixed(2)} subtotal vehículo</Text>
        </View>
      )}

      {STEPS[step] === 'Extras' && (
        <View>
          <Text style={styles.desc}>Personaliza tu reserva con extras opcionales</Text>
          {extrasCatalog.length === 0 ? (
            <Text style={styles.muted}>No hay extras disponibles</Text>
          ) : (
            extrasCatalog.map((extra) => {
              const eid = Number(extra.idExtra ?? extra.id);
              const selected = extrasSel.find((e) => e.id === eid);
              return (
                <Pressable
                  key={eid}
                  style={[styles.extraCard, selected && styles.extraSelected]}
                  onPress={() => toggleExtra(extra)}
                >
                  <Text style={styles.extraName}>{extra.nombreExtra ?? extra.nombre}</Text>
                  <Text style={styles.muted}>{extra.descripcionExtra ?? extra.descripcion ?? 'Servicio adicional'}</Text>
                  <Text style={styles.extraPrice}>${Number(extra.valorFijo ?? extra.precio ?? 0).toFixed(2)}/día</Text>
                  {selected ? <Text style={styles.extraCheck}>✓ Seleccionado</Text> : null}
                </Pressable>
              );
            })
          )}
        </View>
      )}

      {STEPS[step] === 'Resumen' && (
        <Card>
          <Text style={styles.section}>Vehículo</Text>
          <Text style={styles.rowText}>{titulo}</Text>
          <Text style={styles.section}>Fechas</Text>
          <Text style={styles.rowText}>Recogida: {toDateTimeLocalValue(fechaRecogida).replace('T', ' ')}</Text>
          <Text style={styles.rowText}>Devolución: {toDateTimeLocalValue(fechaDevolucion).replace('T', ' ')}</Text>
          <Text style={styles.section}>Desglose</Text>
          <Text style={styles.rowText}>Vehículo ({dias} días): ${subtotalVehiculo.toFixed(2)}</Text>
          {extrasSel.map((ex) => (
            <Text key={ex.id} style={styles.rowText}>{ex.nombre}: ${(ex.valorFijo * ex.cantidad * dias).toFixed(2)}</Text>
          ))}
          {cargoOneWay > 0 ? <Text style={styles.rowText}>Cargo one-way: ${cargoOneWay.toFixed(2)}</Text> : null}
          <Text style={styles.rowText}>IVA (15%): ${iva.toFixed(2)}</Text>
          <Text style={styles.total}>Total: ${totalFinal.toFixed(2)} USD</Text>
        </Card>
      )}

      {STEPS[step] === 'Pago' && (
        <View>
          <Card style={styles.pagoCard}>
            <Text style={styles.pagoSim}>💳 Pasarela simulada</Text>
            <Text style={styles.pagoNumber}>
              {pago.numeroTarjeta ? pago.numeroTarjeta.replace(/(.{4})/g, '$1 ').trim() : '•••• •••• •••• ••••'}
            </Text>
            <Text style={styles.muted}>{pago.nombreTitular || 'TITULAR'}</Text>
          </Card>
          <Input label="Nombre del titular" value={pago.nombreTitular} onChangeText={(v) => setPago({ ...pago, nombreTitular: v.toUpperCase() })} />
          <Input label="Número de tarjeta" value={pago.numeroTarjeta} onChangeText={(v) => setPago({ ...pago, numeroTarjeta: v.replace(/\D/g, '').slice(0, 16) })} keyboardType="number-pad" />
          <View style={styles.pagoRow}>
            <View style={{ flex: 1 }}>
              <Input label="Mes (MM)" value={pago.mesExpiracion} onChangeText={(v) => setPago({ ...pago, mesExpiracion: v.replace(/\D/g, '').slice(0, 2) })} keyboardType="number-pad" />
            </View>
            <View style={{ flex: 1 }}>
              <Input label="Año (AA)" value={pago.anioExpiracion} onChangeText={(v) => setPago({ ...pago, anioExpiracion: v.replace(/\D/g, '').slice(0, 2) })} keyboardType="number-pad" />
            </View>
            <View style={{ flex: 1 }}>
              <Input label="CVV" value={pago.cvv} onChangeText={(v) => setPago({ ...pago, cvv: v.replace(/\D/g, '').slice(0, 4) })} keyboardType="number-pad" secureTextEntry />
            </View>
          </View>
          <Text style={styles.total}>Total a pagar: ${totalFinal.toFixed(2)} USD</Text>
          <Text style={styles.disclaimer}>Pago de demostración — no se realizará un cargo real.</Text>
        </View>
      )}

      <View style={styles.nav}>
        {step > (skipIdentificacion ? 1 : 0) ? (
          <Button label="Atrás" variant="secondary" onPress={() => setStep((s) => s - 1)} style={{ flex: 1 }} />
        ) : (
          <View style={{ flex: 1 }} />
        )}
        <Button
          label={STEPS[step] === 'Pago' ? (processing ? 'Procesando…' : 'Pagar y confirmar') : 'Continuar'}
          variant="client"
          onPress={handleNext}
          loading={processing || guestProcessing || checkingDisp}
          style={{ flex: 1 }}
        />
      </View>
    </Screen>
  );
}

const styles = StyleSheet.create({
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
  muted: { color: colors.textMuted, marginTop: 4 },
  error: { color: colors.danger, marginTop: 8 },
  stepBar: { flexDirection: 'row', justifyContent: 'center', gap: 8, marginBottom: spacing.sm },
  stepDot: {
    width: 28,
    height: 28,
    borderRadius: radius.full,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  stepDotActive: { backgroundColor: colors.accent, borderColor: colors.accent },
  stepDotText: { color: colors.textMuted, fontSize: 12, fontWeight: '700' },
  stepDotTextActive: { color: colors.white },
  stepLabel: { color: colors.text, fontSize: 20, fontWeight: '800', marginBottom: spacing.md, textAlign: 'center' },
  vehicleCard: { marginBottom: spacing.lg, overflow: 'hidden' },
  vehicleImg: { width: '100%', height: 140, marginBottom: spacing.sm, borderRadius: radius.md },
  vehicleTitle: { color: colors.text, fontSize: 18, fontWeight: '700' },
  desc: { color: colors.textSecondary, marginBottom: spacing.md, lineHeight: 20 },
  section: { color: colors.text, fontWeight: '700', marginTop: spacing.md, marginBottom: spacing.sm },
  dateRow: {
    backgroundColor: colors.bgSecondary,
    padding: spacing.md,
    borderRadius: radius.md,
    marginBottom: spacing.sm,
    borderWidth: 1,
    borderColor: colors.border,
  },
  dateLabel: { color: colors.textMuted, fontSize: 12 },
  dateValue: { color: colors.text, fontSize: 15, fontWeight: '600', marginTop: 4 },
  chip: {
    padding: spacing.md,
    borderRadius: radius.md,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    marginBottom: spacing.sm,
  },
  chipActive: { borderColor: colors.accent, backgroundColor: colors.clientGhost },
  chipText: { color: colors.text, fontWeight: '600' },
  extraCard: {
    padding: spacing.md,
    borderRadius: radius.md,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    marginBottom: spacing.sm,
  },
  extraSelected: { borderColor: colors.accent, backgroundColor: colors.clientGhost },
  extraName: { color: colors.text, fontWeight: '700' },
  extraPrice: { color: colors.accent, fontWeight: '700', marginTop: 4 },
  extraCheck: { color: colors.success, marginTop: 4, fontWeight: '600' },
  rowText: { color: colors.textSecondary, marginTop: 4 },
  total: { color: colors.text, fontSize: 20, fontWeight: '800', marginTop: spacing.md },
  pagoCard: {
    backgroundColor: colors.bgSecondary,
    borderColor: colors.accent,
    marginBottom: spacing.lg,
  },
  pagoSim: { color: colors.accent, fontWeight: '700', marginBottom: spacing.sm },
  pagoNumber: { color: colors.text, fontSize: 18, letterSpacing: 2, fontWeight: '600' },
  pagoRow: { flexDirection: 'row', gap: spacing.sm },
  disclaimer: { color: colors.textMuted, fontSize: 12, marginTop: spacing.sm, textAlign: 'center' },
  nav: { flexDirection: 'row', gap: spacing.sm, marginTop: spacing.xl },
  confirmWrap: { alignItems: 'center', justifyContent: 'center' },
  confirmIcon: { fontSize: 48 },
  confirmTitle: { color: colors.text, fontSize: 24, fontWeight: '800', marginTop: spacing.md },
  confirmCode: { color: colors.accent, fontSize: 22, fontWeight: '800', marginTop: spacing.sm },
});
