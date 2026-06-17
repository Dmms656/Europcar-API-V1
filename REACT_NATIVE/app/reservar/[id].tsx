import { ReactNode, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { Link, router, useLocalSearchParams } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { bookingApi } from '@/src/api/bookingApi';
import { reservasApi } from '@/src/api/reservasApi';
import { WebShell } from '@/src/components/layout/WebShell';
import { ReservarVehiclePanel } from '@/src/components/reservar/ReservarVehiclePanel';
import { PaymentCardVisual } from '@/src/components/reservar/PaymentCardVisual';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { DateTimeSelector } from '@/src/components/ui/DateTimeSelector';
import { Input } from '@/src/components/ui/Input';
import { Select } from '@/src/components/ui/Select';
import { Screen } from '@/src/components/ui/Screen';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { alertMessage } from '@/src/utils/confirm';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';
import { formatDateTimeEs } from '@/src/utils/dateFormat';
import {
  defaultRentalDateTimeLocalRange,
  getPayload,
  guestFormFromUserProfile,
  normalizeContactoReserva,
  normalizeVehiculoDetalle,
  toDateTimeLocalValue,
  type VehiculoBooking,
} from '@/src/utils/bookingNormalize';
import { loadReservationClientData, principalDisplayName } from '@/src/utils/reservationClient';

const STEPS = ['Identificación', 'Fechas', 'Conductores', 'Extras', 'Resumen', 'Pago'] as const;
const IVA_RATE = 0.15;
const RECARGO_CONDUCTOR_ADICIONAL_DIA = 15;
const EXTRA_CONDUCTOR_ADICIONAL_CODE = 'COND-ADIC';

const MONTH_OPTIONS = Array.from({ length: 12 }, (_, i) => {
  const v = String(i + 1).padStart(2, '0');
  return { label: v, value: v };
});
const YEAR_OPTIONS = Array.from({ length: 10 }, (_, i) => ({
  label: String(2026 + i),
  value: String(26 + i),
}));

type ExtraItem = {
  idExtra?: number;
  id?: number;
  nombreExtra?: string;
  nombre?: string;
  descripcionExtra?: string;
  descripcion?: string;
  valorFijo?: number;
  precio?: number;
  codigoExtra?: string;
  codigo?: string;
};

type ExtraSel = { id: number; nombre: string; valorFijo: number; cantidad: number; codigo?: string };

type Conductor = {
  nombre: string;
  licencia: string;
  edad: string;
  telefono: string;
  esPrincipal: boolean;
  esCliente: boolean;
};

type Localizacion = { idLocalizacion?: number; id?: number; nombre?: string; nombreLocalizacion?: string };

function buildPrincipalConductor(nombre: string, telefono = ''): Conductor {
  return {
    nombre: nombre.trim() || 'Cliente',
    licencia: '',
    edad: '',
    telefono,
    esPrincipal: true,
    esCliente: true,
  };
}

function isConductorExtra(extra: ExtraItem) {
  return String(extra.codigoExtra ?? extra.codigo ?? '').toUpperCase() === EXTRA_CONDUCTOR_ADICIONAL_CODE;
}

function splitNombres(fullName = '', fallbackApellido = 'N/A') {
  const parts = String(fullName).trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return { nombres: '', apellidos: '' };
  if (parts.length === 1) return { nombres: parts[0], apellidos: fallbackApellido };
  return { nombres: parts.slice(0, -1).join(' '), apellidos: parts.slice(-1).join(' ') };
}

function parseDateTimeLocal(value: string) {
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? new Date() : d;
}

function resolveCliente(user: ReturnType<typeof useAuthStore.getState>['user'], guestForm: {
  nombre: string; apellido: string; cedula: string; correo: string; telefono: string;
}) {
  const contacto = normalizeContactoReserva(user, guestForm);
  const cedulaPerfil = (user?.numeroIdentificacion || '').trim();
  const cedulaForm = (guestForm.cedula || '').trim();
  const cedula = cedulaForm || cedulaPerfil;

  if (guestForm.nombre?.trim() && cedula) {
    return {
      nombre: guestForm.nombre.trim(),
      apellido: (guestForm.apellido || '').trim() || 'N/A',
      cedula,
      ...contacto,
    };
  }
  if (!user) return null;

  const nombres = (user.nombres || '').trim();
  const apellidos = (user.apellidos || '').trim();
  if (nombres && cedula) {
    return { nombre: nombres, apellido: apellidos || 'N/A', cedula, ...contacto };
  }

  const full = (user.nombreCompleto || user.username || '').trim();
  const parts = full.split(/\s+/).filter(Boolean);
  const nombre = parts[0] || user.username || 'Cliente';
  const apellido = apellidos || (parts.length > 1 ? parts.slice(1).join(' ') : 'N/A');
  return { nombre, apellido, cedula, ...contacto };
}

function FormGrid({ children, columns }: { children: ReactNode; columns: 1 | 2 }) {
  return (
    <View style={columns === 2 ? styles.formGridDesktop : styles.formGrid}>
      {children}
    </View>
  );
}

function FormField({ children, half, columns }: { children: ReactNode; half?: boolean; columns: 1 | 2 }) {
  return (
    <View style={columns === 2 && half ? styles.formFieldHalf : styles.formFieldFull}>
      {children}
    </View>
  );
}

export default function ReservarScreen() {
  const { id, idLocalizacion: locParam } = useLocalSearchParams<{ id: string; idLocalizacion?: string }>();
  const idVehiculo = Number(id);
  const { isWeb, isDesktop } = useBreakpoint();
  const formColumns: 1 | 2 = isWeb && isDesktop ? 2 : 1;
  const user = useAuthStore((s) => s.user);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const refreshProfile = useAuthStore((s) => s.refreshProfile);
  const patchUser = useAuthStore((s) => s.patchUser);

  const defaults = defaultRentalDateTimeLocalRange();

  const [guestClientId, setGuestClientId] = useState<number | null>(null);
  const [profileLoading, setProfileLoading] = useState(false);

  const skipIdentificacion = Boolean(guestClientId || isAuthenticated);

  const [step, setStep] = useState(0);
  const [vehiculo, setVehiculo] = useState<VehiculoBooking | null>(null);
  const [localizaciones, setLocalizaciones] = useState<Localizacion[]>([]);
  const [extrasCatalog, setExtrasCatalog] = useState<ExtraItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [guestProcessing, setGuestProcessing] = useState(false);
  const [checkingDisp, setCheckingDisp] = useState(false);
  const [dispBlocked, setDispBlocked] = useState(false);
  const [dispMsg, setDispMsg] = useState('');
  const [confirmada, setConfirmada] = useState<{ codigo: string; total: number } | null>(null);

  const [guestForm, setGuestForm] = useState({
    nombre: '', apellido: '', cedula: '', correo: '', telefono: '', direccion: '',
  });

  const [fechaRecogida, setFechaRecogida] = useState(parseDateTimeLocal(defaults.fechaRecogida));
  const [fechaDevolucion, setFechaDevolucion] = useState(parseDateTimeLocal(defaults.fechaDevolucion));
  const [idLocRecogida, setIdLocRecogida] = useState(String(locParam || ''));
  const [idLocDevolucion, setIdLocDevolucion] = useState(String(locParam || ''));
  const [extrasSel, setExtrasSel] = useState<ExtraSel[]>([]);
  const [conductores, setConductores] = useState<Conductor[]>([]);
  const [showAddConductor, setShowAddConductor] = useState(false);
  const [newConductor, setNewConductor] = useState({
    nombre: '', apellido: '', licencia: '', edad: '', telefono: '', esPrincipal: false,
  });

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
    if (!isAuthenticated) {
      setGuestClientId(null);
      setStep(0);
      return;
    }

    let cancelled = false;
    (async () => {
      setProfileLoading(true);
      try {
        await refreshProfile();
        if (cancelled) return;

        const currentUser = useAuthStore.getState().user;
        const { guestForm: loadedForm, user: enrichedUser } = await loadReservationClientData(currentUser);
        if (cancelled) return;

        if (enrichedUser && enrichedUser !== currentUser) {
          await patchUser(enrichedUser);
        }

        if (loadedForm) {
          setGuestForm((prev) => ({ ...prev, ...loadedForm }));
        }

        if (currentUser?.idCliente) {
          setGuestClientId(currentUser.idCliente);
        }

        const displayName = principalDisplayName(loadedForm ?? { nombre: '', apellido: '', cedula: '', correo: '', telefono: '', direccion: '' }, enrichedUser ?? currentUser);
        if (displayName) {
          setConductores([buildPrincipalConductor(displayName, loadedForm?.telefono || enrichedUser?.telefono || '')]);
        }

        const titular = displayName.toUpperCase();
        if (titular) {
          setPago((prev) => ({ ...prev, nombreTitular: prev.nombreTitular || titular }));
        }

        if (isAuthenticated) {
          setStep((s) => (s === 0 ? 1 : s));
        }
      } finally {
        if (!cancelled) setProfileLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [isAuthenticated, refreshProfile, patchUser, idVehiculo]);

  useEffect(() => {
    if (skipIdentificacion && step === 0) {
      setStep(1);
    }
  }, [skipIdentificacion, step]);

  const dias = useMemo(() => {
    const diff = Math.ceil((fechaDevolucion.getTime() - fechaRecogida.getTime()) / (1000 * 60 * 60 * 24));
    return Math.max(diff, 1);
  }, [fechaRecogida, fechaDevolucion]);

  const precioBase = Number(vehiculo?.precioBaseDia ?? vehiculo?.precioDia ?? 0);
  const subtotalVehiculo = precioBase * dias;
  const subtotalExtras = extrasSel.reduce((acc, ex) => acc + ex.valorFijo * ex.cantidad * dias, 0);
  const conductoresAdicionales = Math.max(conductores.length - 1, 0);
  const recargoConductores = conductoresAdicionales * RECARGO_CONDUCTOR_ADICIONAL_DIA * dias;
  const subtotal = subtotalVehiculo + subtotalExtras + recargoConductores;
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
    const isCondAdic = isConductorExtra(extra);
    if (isCondAdic && conductoresAdicionales <= 0) {
      alertMessage('Conductor adicional', 'Agrega un conductor adicional antes de aplicar este extra.');
      const idx = STEPS.indexOf('Conductores');
      if (idx >= 0) setStep(idx);
      return;
    }

    setExtrasSel((prev) => {
      const exists = prev.find((e) => e.id === eid);
      if (isCondAdic && exists && conductoresAdicionales > 0) return prev;
      if (exists) return prev.filter((e) => e.id !== eid);
      return [
        ...prev,
        {
          id: eid,
          nombre: extra.nombreExtra ?? extra.nombre ?? 'Extra',
          codigo: extra.codigoExtra ?? extra.codigo ?? '',
          valorFijo: Number(extra.valorFijo ?? extra.precio ?? 0),
          cantidad: isCondAdic ? conductoresAdicionales : 1,
        },
      ];
    });
  };

  const updateExtraCantidad = (extraId: number, delta: number) => {
    setExtrasSel((prev) =>
      prev.map((e) =>
        e.id === extraId ? { ...e, cantidad: Math.max(1, e.cantidad + delta) } : e,
      ),
    );
  };

  useEffect(() => {
    const condExtraCatalog = extrasCatalog.find((ex) => isConductorExtra(ex));
    if (!condExtraCatalog) return;

    setExtrasSel((prev) => {
      const already = prev.find((ex) => (ex.codigo || '').toUpperCase() === EXTRA_CONDUCTOR_ADICIONAL_CODE);
      if (conductoresAdicionales > 0) {
        if (!already) {
          return [
            ...prev,
            {
              id: Number(condExtraCatalog.idExtra ?? condExtraCatalog.id),
              nombre: condExtraCatalog.nombreExtra ?? condExtraCatalog.nombre ?? 'Conductor adicional',
              codigo: EXTRA_CONDUCTOR_ADICIONAL_CODE,
              valorFijo: Number(condExtraCatalog.valorFijo ?? condExtraCatalog.precio ?? 0),
              cantidad: conductoresAdicionales,
            },
          ];
        }
        if (already.cantidad !== conductoresAdicionales) {
          return prev.map((ex) =>
            (ex.codigo || '').toUpperCase() === EXTRA_CONDUCTOR_ADICIONAL_CODE
              ? { ...ex, cantidad: conductoresAdicionales }
              : ex,
          );
        }
        return prev;
      }
      if (already) {
        return prev.filter((ex) => (ex.codigo || '').toUpperCase() !== EXTRA_CONDUCTOR_ADICIONAL_CODE);
      }
      return prev;
    });
  }, [extrasCatalog, conductoresAdicionales]);

  const setConductorPrincipal = (idx: number) => {
    setConductores((prev) => prev.map((c, i) => ({ ...c, esPrincipal: i === idx })));
  };

  const handleAddConductor = () => {
    if (!newConductor.nombre.trim() || !newConductor.licencia.trim() || !newConductor.edad || !newConductor.telefono.trim()) {
      alertMessage('Conductor', 'Completa nombre, identificación, edad y teléfono del conductor adicional');
      return;
    }

    setConductores((prev) => {
      const next = prev.map((c) => ({
        ...c,
        esPrincipal: newConductor.esPrincipal ? false : c.esPrincipal,
      }));
      next.push({
        nombre: `${newConductor.nombre.trim()} ${newConductor.apellido.trim()}`.trim(),
        licencia: newConductor.licencia.trim().toUpperCase(),
        edad: newConductor.edad.trim(),
        telefono: newConductor.telefono.trim(),
        esPrincipal: newConductor.esPrincipal,
        esCliente: false,
      });
      if (!next.some((c) => c.esPrincipal)) next[0].esPrincipal = true;
      return next;
    });

    setNewConductor({ nombre: '', apellido: '', licencia: '', edad: '', telefono: '', esPrincipal: false });
    setShowAddConductor(false);
  };

  const handleGuestContinue = async () => {
    const nombre = guestForm.nombre.trim();
    const cedula = guestForm.cedula.trim();
    if (!nombre || !cedula) {
      alertMessage('Datos requeridos', 'Nombre y cédula son obligatorios');
      return;
    }

    if (isAuthenticated) {
      const nombre = `${guestForm.nombre} ${guestForm.apellido}`.trim() || user?.nombreCompleto || user?.username || '';
      setConductores([buildPrincipalConductor(nombre, guestForm.telefono || user?.telefono || '')]);
      setStep(1);
      return;
    }

    setGuestProcessing(true);
    try {
      const res = await reservasApi.guestClient({
        nombre,
        apellido: guestForm.apellido.trim(),
        cedula,
        correo: guestForm.correo.trim(),
        telefono: guestForm.telefono.trim(),
        direccion: guestForm.direccion.trim(),
      });
      const data = unwrapData<{ idCliente?: number; esNuevo?: boolean }>(res);
      const newId = data?.idCliente;
      if (newId) setGuestClientId(Number(newId));
      setConductores([
        buildPrincipalConductor(`${nombre} ${guestForm.apellido}`.trim(), guestForm.telefono.trim()),
      ]);
      setStep(1);
    } catch (e) {
      alertMessage('Error', getErrorMessage(e));
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
        alertMessage('Fechas inválidas', 'La devolución debe ser posterior a la recogida');
        return;
      }
      if (!idLocRecogida || !idLocDevolucion) {
        alertMessage('Sucursales', 'Selecciona sucursal de recogida y devolución');
        return;
      }
      const ok = await checkDisponibilidad();
      if (!ok) {
        alertMessage('No disponible', dispMsg || 'El vehículo no está disponible');
        return;
      }
    }
    if (current === 'Conductores') {
      if (conductores.length === 0 || !conductores.some((c) => c.esPrincipal)) {
        alertMessage('Conductores', 'Debe haber al menos un conductor principal');
        return;
      }
    }
    if (current === 'Extras') {
      const condAdic = extrasSel.find((ex) => (ex.codigo || '').toUpperCase() === EXTRA_CONDUCTOR_ADICIONAL_CODE);
      if (condAdic && conductoresAdicionales === 0) {
        alertMessage('Extras', 'No se puede aplicar el extra de conductor adicional sin conductor adicional');
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
      alertMessage('Pago', 'Completa los datos de la tarjeta');
      return;
    }
    if (!pago.mesExpiracion || !pago.anioExpiracion || pago.cvv.length < 3) {
      alertMessage('Pago', 'Completa fecha de expiración y CVV');
      return;
    }

    const cliente = resolveCliente(user, guestForm);
    if (!cliente?.cedula) {
      alertMessage(
        'Identificación',
        'No encontramos tu cédula en el perfil. Cierra sesión, vuelve a entrar o contacta soporte.',
      );
      return;
    }
    if (!cliente?.nombre) {
      alertMessage('Identificación', 'Indica tu nombre antes de pagar');
      return;
    }

    const principal = conductores.find((c) => c.esPrincipal) || conductores[0];
    if (!principal) {
      alertMessage('Conductores', 'Agrega al menos un conductor antes de pagar');
      setStep(STEPS.indexOf('Conductores'));
      return;
    }

    setProcessing(true);
    try {
      const horaInicio = `${String(fechaRecogida.getHours()).padStart(2, '0')}:${String(fechaRecogida.getMinutes()).padStart(2, '0')}:00`;
      const horaFin = `${String(fechaDevolucion.getHours()).padStart(2, '0')}:${String(fechaDevolucion.getMinutes()).padStart(2, '0')}:00`;
      const secundario = conductores.find((c) => !c.esPrincipal);
      const principalNames = splitNombres(principal.nombre, cliente.apellido);
      const secondaryNames = secundario ? splitNombres(secundario.nombre, 'N/A') : null;

      const payload = {
        idVehiculo: String(idVehiculo),
        idLocalizacionRecogida: Number(idLocRecogida),
        idLocalizacionEntrega: Number(idLocDevolucion),
        idLocalizacionDevolucion: Number(idLocDevolucion),
        fechaInicio: toDateTimeLocalValue(fechaRecogida).slice(0, 10),
        fechaFin: toDateTimeLocalValue(fechaDevolucion).slice(0, 10),
        horaInicio,
        horaFin,
        origenCanalReserva: isWeb ? 'WEB' : 'MOBILE',
        cliente: {
          nombres: cliente.nombre,
          apellidos: cliente.apellido,
          tipoIdentificacion: 'CEDULA',
          numeroIdentificacion: cliente.cedula,
          correo: cliente.correo,
          telefono: cliente.telefono,
        },
        conductorPrincipal: {
          nombres: principalNames.nombres || cliente.nombre,
          apellidos: principalNames.apellidos || cliente.apellido,
          tipoIdentificacion: 'CEDULA',
          numeroIdentificacion: principal.esCliente ? cliente.cedula : `${cliente.cedula}-A`,
          correo: cliente.correo,
          telefono: principal.telefono?.trim() || cliente.telefono,
          numeroLicencia: principal.licencia?.trim() || cliente.cedula,
          fechaVencimientoLicencia: '2035-12-31',
          edadConductor: Number(principal.edad) || 25,
        },
        conductorSecundario: secondaryNames
          ? {
              nombres: secondaryNames.nombres,
              apellidos: secondaryNames.apellidos,
              tipoIdentificacion: 'PASAPORTE',
              numeroIdentificacion: `${cliente.cedula}-B`,
              correo: cliente.correo,
              telefono: secundario?.telefono?.trim() || cliente.telefono,
              numeroLicencia: secundario?.licencia?.trim() || 'PENDIENTE',
              fechaVencimientoLicencia: '2035-12-31',
              edadConductor: Number(secundario?.edad) || 25,
            }
          : null,
        extras: extrasSel.map((ex) => ({ idExtra: ex.id, cantidad: ex.cantidad })),
      };

      const res = await bookingApi.crearReserva(payload);
      const data = unwrapData<{ codigoReserva?: string; total?: number }>(res);
      setConfirmada({
        codigo: data?.codigoReserva ?? `RES-${Date.now().toString(36).toUpperCase()}`,
        total: data?.total ?? totalFinal,
      });
    } catch (e) {
      alertMessage('Error', getErrorMessage(e));
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
        <Button label="Ver mis reservas" onPress={() => router.replace('/mis-reservas')} variant="client" style={{ marginTop: spacing.xl, width: '100%' }} />
        <Button label="Volver al catálogo" onPress={() => router.replace('/(tabs)/catalogo')} variant="ghost" style={{ marginTop: spacing.sm, width: '100%' }} />
      </Screen>
    );
  }

  const titulo = `${vehiculo?.marca ?? ''} ${vehiculo?.modelo ?? ''}`.trim();

  const vehiclePanel = (
    <ReservarVehiclePanel
      vehiculo={vehiculo}
      idVehiculo={idVehiculo}
      titulo={titulo}
      precioBase={precioBase}
      dias={dias}
      subtotalVehiculo={subtotalVehiculo}
      subtotalExtras={subtotalExtras}
      recargoConductores={recargoConductores}
      iva={iva}
      cargoOneWay={cargoOneWay}
      totalFinal={totalFinal}
      compact={!isWeb || !isDesktop}
    />
  );

  const stepContent = (
    <Card style={styles.stepCard}>
      {STEPS[step] === 'Identificación' && (
        <View>
          <Text style={styles.stepTitle}>Datos del cliente</Text>
          {profileLoading ? (
            <Text style={styles.muted}>Cargando tu perfil…</Text>
          ) : null}
          <Text style={styles.desc}>
            No necesitas crear una cuenta: ingresa tus datos y continúa como invitado.
            {'\n'}Si ya tienes cuenta, inicia sesión desde el menú superior.
          </Text>
          <FormGrid columns={formColumns}>
            <FormField half columns={formColumns}>
              <Input label="Nombres *" value={guestForm.nombre} onChangeText={(v) => setGuestForm({ ...guestForm, nombre: v })} />
            </FormField>
            <FormField half columns={formColumns}>
              <Input label="Apellidos" value={guestForm.apellido} onChangeText={(v) => setGuestForm({ ...guestForm, apellido: v })} />
            </FormField>
            <FormField half columns={formColumns}>
              <Input label="Cédula / Pasaporte *" value={guestForm.cedula} onChangeText={(v) => setGuestForm({ ...guestForm, cedula: v })} />
            </FormField>
            <FormField half columns={formColumns}>
              <Input label="Correo" value={guestForm.correo} onChangeText={(v) => setGuestForm({ ...guestForm, correo: v })} keyboardType="email-address" autoCapitalize="none" />
            </FormField>
            <FormField half columns={formColumns}>
              <Input label="Teléfono" value={guestForm.telefono} onChangeText={(v) => setGuestForm({ ...guestForm, telefono: v })} keyboardType="phone-pad" />
            </FormField>
            <FormField half columns={formColumns}>
              <Input label="Dirección" value={guestForm.direccion} onChangeText={(v) => setGuestForm({ ...guestForm, direccion: v })} />
            </FormField>
          </FormGrid>
        </View>
      )}

      {STEPS[step] === 'Fechas' && (
        <View>
          <Text style={styles.stepTitle}>Fechas y ubicación</Text>
          {isAuthenticated ? (
            <Text style={styles.desc}>
              Hola{user?.nombreCompleto ? `, ${user.nombreCompleto.split(' ')[0]}` : ''}. Confirma las fechas y sucursales de tu reserva.
            </Text>
          ) : (
            <Text style={styles.desc}>Selecciona cuándo recoges y devuelves el vehículo</Text>
          )}
          <FormGrid columns={formColumns}>
            <FormField half columns={formColumns}>
              <DateTimeSelector label="Recogida" value={fechaRecogida} onChange={setFechaRecogida} minimumDate={new Date()} />
            </FormField>
            <FormField half columns={formColumns}>
              <DateTimeSelector label="Devolución" value={fechaDevolucion} onChange={setFechaDevolucion} minimumDate={fechaRecogida} />
            </FormField>
          </FormGrid>

          <Text style={styles.section}>Sucursal recogida</Text>
          <View style={formColumns === 2 ? styles.chipGrid : undefined}>
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
          </View>

          <Text style={styles.section}>Sucursal devolución</Text>
          <View style={formColumns === 2 ? styles.chipGrid : undefined}>
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
          </View>

          {checkingDisp ? <Text style={styles.muted}>Validando disponibilidad…</Text> : null}
          {dispBlocked ? <Text style={styles.error}>{dispMsg}</Text> : null}
          <Text style={styles.muted}>{dias} día(s) · ${subtotalVehiculo.toFixed(2)} subtotal vehículo</Text>
          {isOneWay ? (
            <View style={styles.infoBox}>
              <Text style={styles.infoText}>Se aplicará un cargo de $25.00 por devolución en sucursal diferente.</Text>
            </View>
          ) : null}
        </View>
      )}

      {STEPS[step] === 'Conductores' && (
        <View>
          <Text style={styles.stepTitle}>Conductores</Text>
          <Text style={styles.desc}>
            El cliente se asigna como conductor principal. Puedes agregar conductores adicionales.
          </Text>
          <View style={formColumns === 2 ? styles.extrasGrid : undefined}>
            {conductores.map((c, idx) => (
              <View key={`${c.nombre}-${idx}`} style={[styles.extraCard, c.esPrincipal && styles.extraSelected]}>
                <View style={styles.conductorHeader}>
                  <Ionicons name={c.esPrincipal ? 'shield-checkmark' : 'person-outline'} size={18} color={colors.accent} />
                  <Text style={styles.extraName}>{c.nombre || 'Conductor'}</Text>
                  <Text style={styles.conductorBadge}>{c.esPrincipal ? 'Principal' : 'Adicional'}</Text>
                </View>
                {c.licencia ? <Text style={styles.muted}>ID: {c.licencia}</Text> : null}
                {c.telefono ? <Text style={styles.muted}>Tel: {c.telefono}</Text> : null}
                {c.esCliente ? <Text style={styles.clientTag}>Titular de la cuenta</Text> : null}
                <View style={styles.conductorActions}>
                  {!c.esPrincipal ? (
                    <Pressable onPress={() => setConductorPrincipal(idx)}>
                      <Text style={styles.linkAction}>Hacer principal</Text>
                    </Pressable>
                  ) : null}
                  {!c.esCliente ? (
                    <Pressable onPress={() => setConductores((prev) => prev.filter((_, i) => i !== idx))}>
                      <Text style={styles.dangerAction}>Quitar</Text>
                    </Pressable>
                  ) : null}
                </View>
              </View>
            ))}
          </View>

          {!showAddConductor ? (
            <Button
              label="Agregar conductor adicional"
              variant="secondary"
              onPress={() => setShowAddConductor(true)}
              style={{ marginTop: spacing.md }}
            />
          ) : (
            <Card style={styles.addConductorCard}>
              <Text style={styles.section}>Nuevo conductor</Text>
              <FormGrid columns={formColumns}>
                <FormField half columns={formColumns}>
                  <Input label="Nombre *" value={newConductor.nombre} onChangeText={(v) => setNewConductor({ ...newConductor, nombre: v })} />
                </FormField>
                <FormField half columns={formColumns}>
                  <Input label="Apellido" value={newConductor.apellido} onChangeText={(v) => setNewConductor({ ...newConductor, apellido: v })} />
                </FormField>
                <FormField half columns={formColumns}>
                  <Input label="Identificación *" value={newConductor.licencia} onChangeText={(v) => setNewConductor({ ...newConductor, licencia: v })} />
                </FormField>
                <FormField half columns={formColumns}>
                  <Input label="Edad *" value={newConductor.edad} onChangeText={(v) => setNewConductor({ ...newConductor, edad: v.replace(/\D/g, '') })} keyboardType="number-pad" />
                </FormField>
                <FormField half columns={formColumns}>
                  <Input label="Teléfono *" value={newConductor.telefono} onChangeText={(v) => setNewConductor({ ...newConductor, telefono: v })} keyboardType="phone-pad" />
                </FormField>
              </FormGrid>
              <View style={styles.conductorActions}>
                <Button label="Agregar" variant="client" onPress={handleAddConductor} />
                <Button
                  label="Cancelar"
                  variant="ghost"
                  onPress={() => {
                    setShowAddConductor(false);
                    setNewConductor({ nombre: '', apellido: '', licencia: '', edad: '', telefono: '', esPrincipal: false });
                  }}
                />
              </View>
            </Card>
          )}

          {conductoresAdicionales > 0 ? (
            <View style={styles.infoBox}>
              <Text style={styles.infoText}>
                Recargo por conductor adicional: ${RECARGO_CONDUCTOR_ADICIONAL_DIA.toFixed(2)}/día × {conductoresAdicionales}
              </Text>
            </View>
          ) : null}
        </View>
      )}

      {STEPS[step] === 'Extras' && (
        <View>
          <Text style={styles.stepTitle}>Extras y accesorios</Text>
          <Text style={styles.desc}>Personaliza tu experiencia con extras opcionales</Text>
          <View style={formColumns === 2 ? styles.extrasGrid : undefined}>
            {extrasCatalog.length === 0 ? (
              <Text style={styles.muted}>No hay extras disponibles</Text>
            ) : (
              extrasCatalog.map((extra) => {
                const eid = Number(extra.idExtra ?? extra.id);
                const selected = extrasSel.find((e) => e.id === eid);
                const isCondAdic = isConductorExtra(extra);
                const lockCondAdic = isCondAdic && conductoresAdicionales > 0;
                return (
                  <Pressable
                    key={eid}
                    style={[styles.extraCard, selected && styles.extraSelected, lockCondAdic && styles.extraLocked]}
                    onPress={() => toggleExtra(extra)}
                  >
                    <View style={styles.extraCardHeader}>
                      <Ionicons name="cube-outline" size={22} color={colors.textSecondary} />
                      {selected ? (
                        <View style={styles.extraCheckCircle}>
                          <Ionicons name="checkmark" size={14} color={colors.white} />
                        </View>
                      ) : null}
                    </View>
                    <Text style={styles.extraName}>{extra.nombreExtra ?? extra.nombre}</Text>
                    <Text style={styles.muted}>{extra.descripcionExtra ?? extra.descripcion ?? 'Servicio adicional'}</Text>
                    <Text style={styles.extraPrice}>${Number(extra.valorFijo ?? extra.precio ?? 0).toFixed(2)}/día</Text>
                    {isCondAdic ? (
                      <Text style={styles.condAdicHint}>
                        {conductoresAdicionales > 0
                          ? 'Aplicado automáticamente por conductor adicional'
                          : 'Requiere agregar conductor adicional'}
                      </Text>
                    ) : null}
                    {selected ? (
                      <View style={styles.qtyRow}>
                        <Pressable
                          style={styles.qtyBtn}
                          disabled={isCondAdic}
                          onPress={() => updateExtraCantidad(selected.id, -1)}
                        >
                          <Ionicons name="remove" size={16} color={colors.text} />
                        </Pressable>
                        <Text style={styles.qtyValue}>{selected.cantidad}</Text>
                        <Pressable
                          style={styles.qtyBtn}
                          disabled={isCondAdic}
                          onPress={() => updateExtraCantidad(selected.id, 1)}
                        >
                          <Ionicons name="add" size={16} color={colors.text} />
                        </Pressable>
                      </View>
                    ) : null}
                  </Pressable>
                );
              })
            )}
          </View>
        </View>
      )}

      {STEPS[step] === 'Resumen' && (
        <View>
          <Text style={styles.stepTitle}>Resumen de reserva</Text>
          <View style={formColumns === 2 ? styles.resumenGrid : undefined}>
            <Card style={styles.resumenSection}>
              <Text style={styles.resumenTitle}>Vehículo</Text>
              <Text style={styles.rowText}>{titulo}</Text>
            </Card>
            <Card style={styles.resumenSection}>
              <Text style={styles.resumenTitle}>Fechas</Text>
              <Text style={styles.rowText}>Recogida: {formatDateTimeEs(fechaRecogida)}</Text>
              <Text style={styles.rowText}>Devolución: {formatDateTimeEs(fechaDevolucion)}</Text>
              <Text style={styles.rowText}>{dias} {dias === 1 ? 'día' : 'días'}</Text>
            </Card>
            <Card style={styles.resumenSection}>
              <Text style={styles.resumenTitle}>Ubicaciones</Text>
              <Text style={styles.rowText}>
                Recogida: {localizaciones.find((l) => String(l.idLocalizacion ?? l.id) === idLocRecogida)?.nombre ?? idLocRecogida}
              </Text>
              <Text style={styles.rowText}>
                Devolución: {localizaciones.find((l) => String(l.idLocalizacion ?? l.id) === idLocDevolucion)?.nombre ?? idLocDevolucion}
              </Text>
            </Card>
            {extrasSel.length > 0 ? (
              <Card style={styles.resumenSection}>
                <Text style={styles.resumenTitle}>Extras</Text>
                {extrasSel.map((ex) => (
                  <Text key={ex.id} style={styles.rowText}>
                    {ex.nombre} ×{ex.cantidad} — ${(ex.valorFijo * ex.cantidad * dias).toFixed(2)}
                  </Text>
                ))}
              </Card>
            ) : null}
            {conductoresAdicionales > 0 ? (
              <Card style={styles.resumenSection}>
                <Text style={styles.resumenTitle}>Conductores adicionales</Text>
                <Text style={styles.rowText}>
                  {conductoresAdicionales} × ${RECARGO_CONDUCTOR_ADICIONAL_DIA.toFixed(2)}/día = ${recargoConductores.toFixed(2)}
                </Text>
              </Card>
            ) : null}
            <Card style={styles.resumenSection}>
              <Text style={styles.resumenTitle}>Cliente</Text>
              <Text style={styles.rowText}>
                {user?.nombreCompleto || `${guestForm.nombre} ${guestForm.apellido}`.trim() || user?.username}
              </Text>
              <Text style={styles.rowText}>{guestForm.correo || user?.correo}</Text>
              <Text style={styles.rowText}>{guestForm.cedula || user?.numeroIdentificacion}</Text>
            </Card>
          </View>
          <Text style={styles.total}>Total: ${totalFinal.toFixed(2)} USD</Text>
        </View>
      )}

      {STEPS[step] === 'Pago' && (
        <View>
          <Text style={styles.stepTitle}>Pasarela de pago</Text>
          {!guestForm.cedula && isAuthenticated ? (
            <View style={styles.infoBox}>
              <Text style={styles.infoText}>Confirma tu cédula para completar la reserva.</Text>
              <Input
                label="Cédula / Pasaporte *"
                value={guestForm.cedula}
                onChangeText={(v) => setGuestForm({ ...guestForm, cedula: v })}
              />
            </View>
          ) : null}
          <PaymentCardVisual
            numeroTarjeta={pago.numeroTarjeta}
            nombreTitular={pago.nombreTitular}
            mesExpiracion={pago.mesExpiracion}
            anioExpiracion={pago.anioExpiracion}
          />
          <FormGrid columns={formColumns}>
            <FormField half columns={formColumns}>
              <Input
                label="Nombre del titular *"
                value={pago.nombreTitular}
                onChangeText={(v) => setPago({ ...pago, nombreTitular: v.toUpperCase() })}
              />
            </FormField>
            <FormField half columns={formColumns}>
              <Input
                label="Número de tarjeta *"
                value={pago.numeroTarjeta}
                onChangeText={(v) => setPago({ ...pago, numeroTarjeta: v.replace(/\D/g, '').slice(0, 16) })}
                keyboardType="number-pad"
                placeholder="4242 4242 4242 4242"
              />
            </FormField>
          </FormGrid>
          <View style={styles.pagoRow}>
            <View style={{ flex: 1 }}>
              <Select label="Mes *" value={pago.mesExpiracion} onValueChange={(v) => setPago({ ...pago, mesExpiracion: v })} options={MONTH_OPTIONS} placeholder="MM" />
            </View>
            <View style={{ flex: 1 }}>
              <Select label="Año *" value={pago.anioExpiracion} onValueChange={(v) => setPago({ ...pago, anioExpiracion: v })} options={YEAR_OPTIONS} placeholder="AA" />
            </View>
            <View style={{ flex: 1 }}>
              <Input label="CVV *" value={pago.cvv} onChangeText={(v) => setPago({ ...pago, cvv: v.replace(/\D/g, '').slice(0, 4) })} keyboardType="number-pad" secureTextEntry />
            </View>
          </View>
          <View style={styles.pagoTotalBox}>
            <Text style={styles.pagoTotalLabel}>Total a pagar:</Text>
            <Text style={styles.pagoTotalAmount}>${totalFinal.toFixed(2)} USD</Text>
          </View>
          <Text style={styles.disclaimer}>🔒 Pago simulado — no se realizará ningún cargo real.</Text>
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
          loading={processing || guestProcessing || checkingDisp || profileLoading}
          style={{ flex: 1 }}
        />
      </View>
    </Card>
  );

  const page = (
    <Screen padded={!isWeb}>
      {isWeb ? (
        <Link href="/catalogo" asChild>
          <Pressable style={styles.backLink}>
            <Ionicons name="arrow-back" size={18} color={colors.accent} />
            <Text style={styles.backLinkText}>Volver al catálogo</Text>
          </Pressable>
        </Link>
      ) : null}

      <View style={styles.stepBar}>
        {STEPS.map((label, i) => (
          <View key={label} style={StyleSheet.flatten([styles.stepDot, i <= step ? styles.stepDotActive : null])}>
            <Text style={StyleSheet.flatten([styles.stepDotText, i <= step ? styles.stepDotTextActive : null])}>
              {i + 1}
            </Text>
          </View>
        ))}
      </View>
      <Text style={styles.stepLabel}>{STEPS[step]}</Text>

      {isWeb && isDesktop ? (
        <View style={styles.bodyDesktop}>
          <View style={styles.mainColumn}>{stepContent}</View>
          <View style={styles.sideColumn}>{vehiclePanel}</View>
        </View>
      ) : (
        <>
          {vehiclePanel}
          {stepContent}
        </>
      )}
    </Screen>
  );

  if (isWeb) {
    return (
      <WebShell padded={false} maxWidth={1200}>
        {page}
      </WebShell>
    );
  }

  return page;
}

const styles = StyleSheet.create({
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
  muted: { color: colors.textMuted, marginTop: 4 },
  error: { color: colors.danger, marginTop: 8 },
  stepBar: { flexDirection: 'row', justifyContent: 'center', flexWrap: 'wrap', gap: 6, marginBottom: spacing.sm },
  stepDot: {
    width: 24,
    height: 24,
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
  stepTitle: { color: colors.text, fontSize: 20, fontWeight: '800', marginBottom: spacing.sm },
  stepCard: { marginBottom: spacing.lg },
  backLink: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm, marginBottom: spacing.lg },
  backLinkText: { color: colors.accent, fontWeight: '600', fontSize: 15 },
  bodyDesktop: { flexDirection: 'row', gap: spacing.xl, alignItems: 'flex-start' },
  mainColumn: { flex: 1, minWidth: 0 },
  sideColumn: { width: 340, flexShrink: 0 },
  formGrid: { gap: spacing.md },
  formGridDesktop: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.md },
  formFieldFull: { width: '100%' },
  formFieldHalf: { flexGrow: 1, flexBasis: '48%', minWidth: 220 },
  chipGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm },
  extrasGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm },
  desc: { color: colors.textSecondary, marginBottom: spacing.md, lineHeight: 22 },
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
    flexGrow: 1,
    flexBasis: '48%',
    minWidth: 200,
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
    flexGrow: 1,
    flexBasis: '48%',
    minWidth: 200,
  },
  extraSelected: { borderColor: colors.accent, backgroundColor: colors.clientGhost },
  extraLocked: { opacity: 0.92 },
  extraCardHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: spacing.sm },
  extraCheckCircle: {
    width: 22,
    height: 22,
    borderRadius: radius.full,
    backgroundColor: colors.accent,
    alignItems: 'center',
    justifyContent: 'center',
  },
  condAdicHint: { color: colors.textMuted, fontSize: 12, marginTop: spacing.sm, fontWeight: '600' },
  qtyRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: spacing.md, marginTop: spacing.sm },
  qtyBtn: {
    width: 28,
    height: 28,
    borderRadius: radius.full,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.surface,
  },
  qtyValue: { color: colors.text, fontWeight: '700', minWidth: 24, textAlign: 'center' },
  extraName: { color: colors.text, fontWeight: '700' },
  extraPrice: { color: colors.accent, fontWeight: '700', marginTop: 4 },
  extraCheck: { color: colors.success, marginTop: 4, fontWeight: '600' },
  infoBox: {
    marginTop: spacing.md,
    padding: spacing.md,
    borderRadius: radius.md,
    backgroundColor: colors.clientGhost,
  },
  infoText: { color: colors.accent, fontSize: 13, lineHeight: 18 },
  conductorHeader: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm, marginBottom: spacing.xs },
  conductorBadge: { marginLeft: 'auto', color: colors.textMuted, fontSize: 12, fontWeight: '600' },
  clientTag: { color: colors.accent, fontWeight: '600', marginTop: 4, fontSize: 12 },
  conductorActions: { flexDirection: 'row', gap: spacing.md, marginTop: spacing.sm, flexWrap: 'wrap' },
  linkAction: { color: colors.accent, fontWeight: '600' },
  dangerAction: { color: colors.danger, fontWeight: '600' },
  addConductorCard: { marginTop: spacing.md },
  resumenGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.md },
  resumenSection: { flexGrow: 1, flexBasis: '48%', minWidth: 220, marginBottom: 0 },
  resumenTitle: { color: colors.accent, fontSize: 12, fontWeight: '700', textTransform: 'uppercase', marginBottom: spacing.sm },
  pagoTotalBox: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: spacing.md,
    backgroundColor: colors.clientGhost,
    borderRadius: radius.md,
    marginTop: spacing.lg,
  },
  pagoTotalLabel: { color: colors.text, fontWeight: '600' },
  pagoTotalAmount: { color: colors.accent, fontSize: 22, fontWeight: '800' },
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
  nav: { flexDirection: 'row', gap: spacing.sm, marginTop: spacing.xl, justifyContent: 'flex-end' },
  confirmWrap: { alignItems: 'center', justifyContent: 'center' },
  confirmIcon: { fontSize: 48 },
  confirmTitle: { color: colors.text, fontSize: 24, fontWeight: '800', marginTop: spacing.md },
  confirmCode: { color: colors.accent, fontSize: 22, fontWeight: '800', marginTop: spacing.sm },
});
