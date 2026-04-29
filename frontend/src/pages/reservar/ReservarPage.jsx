import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { vehiculosApi } from '../../api/vehiculosApi';
import { reservasApi } from '../../api/reservasApi';
import { pagosApi } from '../../api/pagosApi';
import DateTimePicker from '../../components/ui/DateTimePicker';
import { bookingApi } from '../../api/bookingApi';
import { useAuthStore } from '../../store/useAuthStore';
import {
  Car, MapPin, Calendar, Package, CreditCard, Check,
  ArrowLeft, ArrowRight, Fuel, Users, Settings2, Plus,
  Minus, ShieldCheck, Loader2, CheckCircle2, X, AlertCircle, UserPlus
} from 'lucide-react';
import { toast } from 'sonner';

const IVA_RATE = 0.15;
const RECARGO_CONDUCTOR_ADICIONAL_DIA = 15;
const EXTRA_CONDUCTOR_ADICIONAL_CODE = 'COND-ADIC';
const isValidImageUrl = (url) => url && (url.startsWith('http://') || url.startsWith('https://'));
const NAME_REGEX = /^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ' ]{2,50}$/;
const PHONE_REGEX = /^[+]?[\d\s()-]{7,20}$/;
const LICENCIA_REGEX = /^[A-Za-z0-9-]{5,20}$/;

export default function ReservarPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuthStore();

  // Guest flow: if not authenticated, add 'Identificación' as first step
  const STEPS = isAuthenticated
    ? ['Fechas', 'Conductores', 'Extras', 'Resumen', 'Pago']
    : ['Identificación', 'Fechas', 'Conductores', 'Extras', 'Resumen', 'Pago'];

  const [step, setStep] = useState(0);
  const [vehiculo, setVehiculo] = useState(null);
  const [localizaciones, setLocalizaciones] = useState([]);
  const [extras, setExtras] = useState([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [checkingDisponibilidad, setCheckingDisponibilidad] = useState(false);
  const [disponibilidadBloqueada, setDisponibilidadBloqueada] = useState(false);
  const [disponibilidadMsg, setDisponibilidadMsg] = useState('');
  const [lastAdditionalCount, setLastAdditionalCount] = useState(0);
  const [reservaConfirmada, setReservaConfirmada] = useState(null);
  const [stepErrors, setStepErrors] = useState({});
  const [shake, setShake] = useState(false);

  // Guest client form (no user account needed)
  const [guestForm, setGuestForm] = useState({
    nombre: '', apellido: '', cedula: '', correo: '',
    telefono: '', direccion: '',
  });
  const [guestProcessing, setGuestProcessing] = useState(false);
  const [guestClientId, setGuestClientId] = useState(null);

  // Form state
  const [form, setForm] = useState({
    fechaRecogida: '',
    fechaDevolucion: '',
    idLocalizacionRecogida: '',
    idLocalizacionDevolucion: '',
    extrasSeleccionados: [],
    conductores: [], // [{id, nombre, licencia, esPrincipal, esNuevo?, ...}]
  });
  const [showAddConductor, setShowAddConductor] = useState(false);
  const [newConductor, setNewConductor] = useState({
    nombre: '', apellido: '', licencia: '', edad: '', telefono: '', esPrincipal: false,
  });
  const [newConductorErrors, setNewConductorErrors] = useState({});

  // Payment form
  const [pago, setPago] = useState({
    nombreTitular: '',
    numeroTarjeta: '',
    mesExpiracion: '',
    anioExpiracion: '',
    cvv: '',
  });

  useEffect(() => {
    loadData();
  }, [id]);

  const loadData = async () => {
    setLoading(true);
    try {
      // Load vehicle from the working /disponibles endpoint
      const [vehRes, locRes, extRes] = await Promise.allSettled([
        vehiculosApi.getDisponibles(),
        bookingApi.getLocalizaciones({}),
        bookingApi.getExtras(),
      ]);

      if (vehRes.status === 'fulfilled') {
        const allVehiculos = vehRes.value.data?.data || [];
        const found = allVehiculos.find(v => String(v.idVehiculo) === String(id));
        setVehiculo(found || null);
        if (found?.idLocalizacion) {
          setForm(prev => ({
            ...prev,
            idLocalizacionRecogida: String(found.idLocalizacion),
            idLocalizacionDevolucion: String(found.idLocalizacion),
          }));
        }
      }
      if (locRes.status === 'fulfilled') {
        const ld = locRes.value.data?.data;
        setLocalizaciones(ld?.localizaciones || (Array.isArray(ld) ? ld : []));
      }
      if (extRes.status === 'fulfilled') {
        const ed = extRes.value.data?.data;
        setExtras(ed?.extras || (Array.isArray(ed) ? ed : []));
      }

      // Auto-assign client as principal conductor
      if (user) {
        setForm(prev => ({
          ...prev,
          conductores: [{
            id: null,
            nombre: user.nombreCompleto || user.username || '',
            licencia: '',
            edad: '',
            telefono: '',
            esPrincipal: true,
            esCliente: true,
          }]
        }));
      }
    } catch (e) {
      console.error(e);
      toast.error('Error al cargar el vehículo');
    } finally {
      setLoading(false);
    }
  };

  // Calculate pricing
  const calcularDias = () => {
    if (!form.fechaRecogida || !form.fechaDevolucion) return 0;
    const d1 = new Date(form.fechaRecogida);
    const d2 = new Date(form.fechaDevolucion);
    const diff = Math.ceil((d2 - d1) / (1000 * 60 * 60 * 24));
    return Math.max(diff, 1);
  };

  const dias = calcularDias();
  const precioBase = Number(vehiculo?.precioBaseDia || vehiculo?.precioDia || 0);
  const nombreVehiculo = `${vehiculo?.marca || ''} ${vehiculo?.modelo || vehiculo?.modeloVehiculo || ''}`.trim();
  const categoriaVehiculo = vehiculo?.categoria || vehiculo?.categoriaVehiculo || 'No especificada';
  const placaVehiculo = vehiculo?.placa || vehiculo?.placaVehiculo || 'No disponible';
  const puertasVehiculo = vehiculo?.numeroPuertas ?? 'N/D';
  const maletasVehiculo = vehiculo?.capacidadMaletas ?? 'N/D';
  const anioVehiculo = vehiculo?.anioFabricacion ?? 'N/D';
  const localizacionActual = localizaciones.find(
    (l) => String(l.idLocalizacion ?? l.id) === String(vehiculo?.idLocalizacion)
  );
  const subtotalVehiculo = precioBase * dias;
  const conductoresAdicionales = form.conductores.filter(c => !c.esPrincipal).length;
  const recargoConductores = conductoresAdicionales * RECARGO_CONDUCTOR_ADICIONAL_DIA * dias;
  const subtotalExtras = form.extrasSeleccionados.reduce((acc, ex) => {
    return acc + (Number(ex.valorFijo || ex.precio || 0) * ex.cantidad * dias);
  }, 0);
  const subtotal = subtotalVehiculo + subtotalExtras + recargoConductores;
  const iva = subtotal * IVA_RATE;
  const total = subtotal + iva;
  const isOneWay = form.idLocalizacionRecogida !== form.idLocalizacionDevolucion;
  const cargoOneWay = isOneWay ? 25.00 : 0;
  const totalFinal = total + cargoOneWay;
  const conductoresStepIndex = STEPS.indexOf('Conductores');

  const isConductorExtra = (extra) =>
    String(extra?.codigoExtra || extra?.codigo || '').toUpperCase() === EXTRA_CONDUCTOR_ADICIONAL_CODE;

  const getConductorExtraFromCatalog = () =>
    extras.find((ex) => isConductorExtra(ex));

  const toggleExtra = (extra) => {
    const isCondAdic = isConductorExtra(extra);
    if (isCondAdic && conductoresAdicionales <= 0) {
      toast.error('Para aplicar el extra de Conductor adicional primero debes agregar un conductor adicional.');
      if (conductoresStepIndex >= 0) setStep(conductoresStepIndex);
      return;
    }

    setForm(prev => {
      const exists = prev.extrasSeleccionados.find(e => e.id === (extra.idExtra || extra.id));
      if (isCondAdic && exists && conductoresAdicionales > 0) {
        // Mientras exista conductor adicional, este extra queda bloqueado.
        return prev;
      }
      if (exists) {
        return {
          ...prev,
          extrasSeleccionados: prev.extrasSeleccionados.filter(e => e.id !== (extra.idExtra || extra.id))
        };
      }
      return {
        ...prev,
        extrasSeleccionados: [...prev.extrasSeleccionados, {
          id: extra.idExtra || extra.id,
          nombre: extra.nombreExtra || extra.nombre,
          codigo: extra.codigoExtra || extra.codigo || '',
          valorFijo: extra.valorFijo || extra.precio || 0,
          cantidad: 1,
        }]
      };
    });
  };

  const updateExtraCantidad = (extraId, delta) => {
    setForm(prev => ({
      ...prev,
      extrasSeleccionados: prev.extrasSeleccionados.map(e =>
        e.id === extraId ? { ...e, cantidad: Math.max(1, e.cantidad + delta) } : e
      )
    }));
  };

  useEffect(() => {
    if (conductoresAdicionales > lastAdditionalCount) {
      toast.info(`Se aplicó recargo por conductor adicional: +$${RECARGO_CONDUCTOR_ADICIONAL_DIA.toFixed(2)} por día`, {
        duration: 3500,
      });
    } else if (conductoresAdicionales < lastAdditionalCount && conductoresAdicionales >= 0) {
      toast.info('Se actualizó el recargo por conductores adicionales.', { duration: 3000 });
    }
    setLastAdditionalCount(conductoresAdicionales);
  }, [conductoresAdicionales, lastAdditionalCount]);

  useEffect(() => {
    const condExtraCatalog = getConductorExtraFromCatalog();
    if (!condExtraCatalog) return;

    setForm((prev) => {
      const already = prev.extrasSeleccionados.find((ex) =>
        String(ex.codigoExtra || ex.codigo || '').toUpperCase() === EXTRA_CONDUCTOR_ADICIONAL_CODE
      );

      if (conductoresAdicionales > 0) {
        if (!already) {
          toast.info('Se agregó automáticamente el extra de Conductor adicional.');
          return {
            ...prev,
            extrasSeleccionados: [
              ...prev.extrasSeleccionados,
              {
                id: condExtraCatalog.idExtra || condExtraCatalog.id,
                nombre: condExtraCatalog.nombreExtra || condExtraCatalog.nombre,
                codigo: condExtraCatalog.codigoExtra || condExtraCatalog.codigo || EXTRA_CONDUCTOR_ADICIONAL_CODE,
                valorFijo: condExtraCatalog.valorFijo || condExtraCatalog.precio || 0,
                cantidad: conductoresAdicionales,
              },
            ],
          };
        }
        if (already.cantidad !== conductoresAdicionales) {
          return {
            ...prev,
            extrasSeleccionados: prev.extrasSeleccionados.map((ex) =>
              String(ex.codigoExtra || ex.codigo || '').toUpperCase() === EXTRA_CONDUCTOR_ADICIONAL_CODE
                ? { ...ex, cantidad: conductoresAdicionales }
                : ex
            ),
          };
        }
        return prev;
      }

      if (already) {
        return {
          ...prev,
          extrasSeleccionados: prev.extrasSeleccionados.filter((ex) =>
            String(ex.codigoExtra || ex.codigo || '').toUpperCase() !== EXTRA_CONDUCTOR_ADICIONAL_CODE
          ),
        };
      }
      return prev;
    });
  }, [extras, conductoresAdicionales]);

  useEffect(() => {
    const validateDisponibilidad = async () => {
      if (!vehiculo || !form.fechaRecogida || !form.fechaDevolucion || !form.idLocalizacionRecogida) {
        setDisponibilidadBloqueada(false);
        setDisponibilidadMsg('');
        return;
      }

      if (new Date(form.fechaDevolucion) <= new Date(form.fechaRecogida)) return;

      setCheckingDisponibilidad(true);
      try {
        const res = await bookingApi.checkDisponibilidad(String(vehiculo.codigoInterno || vehiculo.idVehiculo || id), {
          fechaRecogida: new Date(form.fechaRecogida).toISOString(),
          fechaDevolucion: new Date(form.fechaDevolucion).toISOString(),
          idLocalizacion: Number(form.idLocalizacionRecogida),
        });
        const payload = res.data?.data || res.data?.Data || {};
        const disponibilidad = payload.disponibilidad || payload.Disponibilidad || {};
        const disponible = Boolean(disponibilidad.disponible ?? disponibilidad.Disponible);

        if (disponible) {
          setDisponibilidadBloqueada(false);
          setDisponibilidadMsg('');
          setStepErrors(prev => ({ ...prev, disponibilidad: '' }));
        } else {
          const msg = 'El vehículo no está disponible para esas fechas. Ya existe una reserva o bloqueo que impide su uso.';
          setDisponibilidadBloqueada(true);
          setDisponibilidadMsg(msg);
          setStepErrors(prev => ({ ...prev, disponibilidad: msg }));
        }
      } catch {
        const msg = 'No se pudo validar disponibilidad en este momento. Intenta nuevamente.';
        setDisponibilidadBloqueada(true);
        setDisponibilidadMsg(msg);
        setStepErrors(prev => ({ ...prev, disponibilidad: msg }));
      } finally {
        setCheckingDisponibilidad(false);
      }
    };

    validateDisponibilidad();
  }, [vehiculo, form.fechaRecogida, form.fechaDevolucion, form.idLocalizacionRecogida, id]);

  const setConductorPrincipal = (idx) => {
    setForm(prev => ({
      ...prev,
      conductores: prev.conductores.map((cc, i) => ({ ...cc, esPrincipal: i === idx })),
    }));
  };

  const validateConductorDraft = (draft) => {
    const errs = {};
    const nombre = draft.nombre.trim();
    const apellido = draft.apellido.trim();
    const licencia = draft.licencia.trim().toUpperCase();
    const telefono = draft.telefono.trim();
    const edadNum = Number(draft.edad);

    if (!nombre) errs.nombre = 'Nombre requerido';
    else if (!NAME_REGEX.test(nombre)) errs.nombre = 'Nombre inválido (solo letras y espacios, 2-50)';

    if (!apellido) errs.apellido = 'Apellido requerido';
    else if (!NAME_REGEX.test(apellido)) errs.apellido = 'Apellido inválido (solo letras y espacios, 2-50)';

    if (!licencia) errs.licencia = 'Número de licencia requerido';
    else if (!LICENCIA_REGEX.test(licencia)) errs.licencia = 'Licencia inválida (5-20 caracteres alfanuméricos)';

    if (!draft.edad) errs.edad = 'Edad requerida';
    else if (!Number.isInteger(edadNum) || edadNum < 18 || edadNum > 85) errs.edad = 'Edad debe estar entre 18 y 85 años';

    if (!telefono) errs.telefono = 'Teléfono requerido';
    else if (!PHONE_REGEX.test(telefono)) errs.telefono = 'Teléfono inválido';

    return errs;
  };

  const handleAddConductor = () => {
    const errs = validateConductorDraft(newConductor);
    setNewConductorErrors(errs);
    if (Object.keys(errs).length > 0) {
      toast.error(Object.values(errs)[0]);
      return;
    }

    setForm(prev => {
      const nextConductores = prev.conductores.map((c) => ({
        ...c,
        esPrincipal: newConductor.esPrincipal ? false : c.esPrincipal,
      }));
      nextConductores.push({
        id: null,
        nombre: `${newConductor.nombre.trim()} ${newConductor.apellido.trim()}`,
        licencia: newConductor.licencia.trim().toUpperCase(),
        edad: String(newConductor.edad).trim(),
        telefono: newConductor.telefono.trim(),
        esPrincipal: newConductor.esPrincipal,
        esCliente: false,
      });
      if (!nextConductores.some(c => c.esPrincipal)) {
        nextConductores[0].esPrincipal = true;
      }
      return { ...prev, conductores: nextConductores };
    });

    setNewConductor({ nombre: '', apellido: '', licencia: '', edad: '', telefono: '', esPrincipal: false });
    setNewConductorErrors({});
    setShowAddConductor(false);
    toast.success('Conductor adicional agregado');
  };

  const canProceed = () => {
    switch (step) {
      case 0: return form.fechaRecogida && form.fechaDevolucion &&
        form.idLocalizacionRecogida && form.idLocalizacionDevolucion && dias > 0;
      case 1: return form.conductores.length > 0;
      case 2: return true;
      case 3: return true;
      case 4: return pago.nombreTitular && pago.numeroTarjeta.length >= 16 &&
        pago.mesExpiracion && pago.anioExpiracion && pago.cvv.length >= 3;
      default: return false;
    }
  };

  const getStepErrors = () => {
    const errs = {};
    const currentStep = STEPS[step];
    if (currentStep === 'Fechas') {
      if (!form.fechaRecogida) errs.fechaRecogida = 'Selecciona fecha de recogida';
      if (!form.fechaDevolucion) errs.fechaDevolucion = 'Selecciona fecha de devolución';
      if (form.fechaRecogida && form.fechaDevolucion && new Date(form.fechaDevolucion) <= new Date(form.fechaRecogida))
        errs.fechaDevolucion = 'La devolución debe ser posterior a la recogida';
      if (!form.idLocalizacionRecogida) errs.idLocalizacionRecogida = 'Selecciona sucursal de recogida';
      if (!form.idLocalizacionDevolucion) errs.idLocalizacionDevolucion = 'Selecciona sucursal de devolución';
      if (disponibilidadBloqueada) errs.disponibilidad = disponibilidadMsg || 'Vehículo no disponible para el rango seleccionado';
    }
    if (currentStep === 'Conductores') {
      if (form.conductores.length === 0) errs.conductores = 'Debe haber al menos un conductor';
      if (!form.conductores.some(c => c.esPrincipal)) errs.conductores = 'Debe haber un conductor principal';
      const conductorInvalido = form.conductores.find(c =>
        !c.esCliente && (!c.nombre?.trim() || !c.licencia?.trim() || !c.edad || !c.telefono?.trim())
      );
      if (conductorInvalido) errs.conductores = 'Completa los datos del conductor adicional';
    }
    if (currentStep === 'Pago') {
      if (!pago.nombreTitular.trim()) errs.nombreTitular = 'Nombre del titular requerido';
      if (!pago.numeroTarjeta || pago.numeroTarjeta.replace(/\s/g, '').length < 16) errs.numeroTarjeta = 'Número de tarjeta inválido (16 dígitos)';
      if (!pago.mesExpiracion) errs.mesExpiracion = 'Mes requerido';
      if (!pago.anioExpiracion) errs.anioExpiracion = 'Año requerido';
      if (!pago.cvv || pago.cvv.length < 3) errs.cvv = 'CVV inválido';
    }
    if (currentStep === 'Extras') {
      const condAdic = form.extrasSeleccionados.find((ex) =>
        String(ex.codigoExtra || ex.codigo || '').toUpperCase() === EXTRA_CONDUCTOR_ADICIONAL_CODE
      );
      if (condAdic && conductoresAdicionales === 0) {
        errs.extras = 'No se puede aplicar el extra de Conductor adicional sin conductor adicional.';
      }
    }
    return errs;
  };

  // Guest client handler (no user account required)
  const handleGuestClient = async () => {
    const { nombre, apellido, cedula, correo } = guestForm;
    if (!nombre || !cedula) {
      toast.error('Nombre y cédula son obligatorios');
      return;
    }
    setGuestProcessing(true);
    try {
      const res = await reservasApi.guestClient(guestForm);
      const data = res.data?.data;
      setGuestClientId(data.idCliente);
      toast.success(data.esNuevo ? '¡Cliente creado!' : 'Cliente encontrado. Continuemos.');
      // Assign as principal conductor
      setForm(prev => ({
        ...prev,
        conductores: [{
          id: null,
          nombre: `${nombre} ${apellido || ''}`.trim(),
          licencia: '',
          edad: '',
          telefono: guestForm.telefono || '',
          esPrincipal: true,
          esCliente: true,
        }]
      }));
      setStep(step + 1);
    } catch (e) {
      toast.error(e.response?.data?.message || 'Error al registrar cliente');
    } finally { setGuestProcessing(false); }
  };

  const handleNext = () => {
    if (STEPS[step] === 'Identificación') return;

    const errs = getStepErrors();
    if (Object.keys(errs).length > 0) {
      setStepErrors(errs);
      setShake(true);
      setTimeout(() => setShake(false), 500);
      const first = Object.values(errs)[0];
      toast.error(first);
      if (errs.extras && conductoresStepIndex >= 0) {
        setStep(conductoresStepIndex);
      }
      return;
    }
    setStepErrors({});
    if (step < STEPS.length - 1) setStep(step + 1);
  };

  const handlePagar = async () => {
    setProcessing(true);

    // Validate idCliente exists (from user account or guest client)
    const clientId = user?.idCliente || guestClientId;
    if (!clientId) {
      toast.error('No se pudo identificar el cliente. Regresa al primer paso.');
      setProcessing(false);
      return;
    }

    const lastFour = (pago.numeroTarjeta || '0000').slice(-4);
    try {
      const principal = form.conductores.find(c => c.esPrincipal) || form.conductores[0];
      const secundario = form.conductores.find(c => !c.esPrincipal);
      let codigoReservaFinal = '';
      let codigoConfirmacionFinal = '';
      let idReservaFinal = null;

      const splitNombres = (fullName = '') => {
        const trimmed = String(fullName || '').trim();
        const parts = trimmed.split(/\s+/).filter(Boolean);
        if (parts.length === 0) return { nombres: '', apellidos: '' };
        if (parts.length === 1) return { nombres: parts[0], apellidos: principal?.esCliente ? (guestForm.apellido || 'N/A') : 'N/A' };
        return { nombres: parts.slice(0, -1).join(' '), apellidos: parts.slice(-1).join(' ') };
      };

      // Public flow: use Booking API to persist selected drivers (principal/secundario).
      if (!isAuthenticated && principal) {
        const principalNames = splitNombres(principal.nombre);
        const secondaryNames = secundario ? splitNombres(secundario.nombre) : null;
        const fechaIni = new Date(form.fechaRecogida);
        const fechaFin = new Date(form.fechaDevolucion);
        const horaInicio = `${String(fechaIni.getHours()).padStart(2, '0')}:${String(fechaIni.getMinutes()).padStart(2, '0')}:00`;
        const horaFin = `${String(fechaFin.getHours()).padStart(2, '0')}:${String(fechaFin.getMinutes()).padStart(2, '0')}:00`;

        const bookingPayload = {
          idVehiculo: String(vehiculo?.codigoInterno || vehiculo?.idVehiculo || id),
          idLocalizacionRecogida: Number(form.idLocalizacionRecogida),
          idLocalizacionEntrega: Number(form.idLocalizacionDevolucion),
          fechaInicio: form.fechaRecogida.slice(0, 10),
          fechaFin: form.fechaDevolucion.slice(0, 10),
          horaInicio,
          horaFin,
          origenCanalReserva: 'WEB',
          cliente: {
            nombres: guestForm.nombre.trim(),
            apellidos: guestForm.apellido.trim() || 'N/A',
            tipoIdentificacion: 'CED',
            numeroIdentificacion: guestForm.cedula.trim(),
            correo: guestForm.correo.trim(),
            telefono: guestForm.telefono.trim(),
          },
          conductorPrincipal: {
            nombres: principalNames.nombres || guestForm.nombre.trim(),
            apellidos: principalNames.apellidos || guestForm.apellido.trim() || 'N/A',
            tipoIdentificacion: 'CED',
            numeroIdentificacion: principal.esCliente ? guestForm.cedula.trim() : `${guestForm.cedula.trim()}-A`,
            correo: guestForm.correo.trim(),
            telefono: principal.telefono?.trim() || guestForm.telefono.trim(),
            numeroLicencia: principal.licencia?.trim() || 'PENDIENTE',
            edadConductor: Number(principal.edad) || 25,
          },
          conductorSecundario: secondaryNames ? {
            nombres: secondaryNames.nombres,
            apellidos: secondaryNames.apellidos,
            tipoIdentificacion: 'PAS',
            numeroIdentificacion: `${guestForm.cedula.trim()}-B`,
            correo: guestForm.correo.trim(),
            telefono: secundario?.telefono?.trim() || guestForm.telefono.trim(),
            numeroLicencia: secundario?.licencia?.trim() || 'PENDIENTE',
            edadConductor: Number(secundario?.edad) || 25,
          } : null,
          extras: form.extrasSeleccionados.map(ex => ({ idExtra: ex.id, cantidad: ex.cantidad })),
        };

        const bookingRes = await bookingApi.crearReserva(bookingPayload);
        const bookingData = bookingRes.data?.data || bookingRes.data?.Data || {};
        codigoReservaFinal = bookingData?.codigoReserva || bookingData?.CodigoReserva || '';
        codigoConfirmacionFinal = codigoReservaFinal;
      } else {
        const reservaPayload = {
          idCliente: clientId,
          idVehiculo: Number(id),
          idLocalizacionRecogida: Number(form.idLocalizacionRecogida),
          idLocalizacionDevolucion: Number(form.idLocalizacionDevolucion),
          canalReserva: 'WEB',
          fechaHoraRecogida: new Date(form.fechaRecogida).toISOString(),
          fechaHoraDevolucion: new Date(form.fechaDevolucion).toISOString(),
          extras: form.extrasSeleccionados.map(ex => ({
            idExtra: ex.id,
            cantidad: ex.cantidad,
          })),
          conductores: form.conductores
            .filter(c => c.id)
            .map(c => ({ idConductor: c.id, esPrincipal: c.esPrincipal })),
        };

        const res = await reservasApi.create(reservaPayload);
        const reservaData = res.data?.data;
        idReservaFinal = reservaData?.idReserva ?? null;
        codigoReservaFinal = reservaData?.codigoReserva || '';
        codigoConfirmacionFinal = reservaData?.codigoConfirmacion || codigoReservaFinal;

        if (idReservaFinal) {
          await reservasApi.confirmar(idReservaFinal, {
            monto: totalFinal,
            referenciaExterna: `SIM-${lastFour}-${Date.now().toString(36).toUpperCase()}`,
          });
        }
      }

      setReservaConfirmada({
        codigoReserva: codigoReservaFinal || `RES-${Date.now().toString(36).toUpperCase()}`,
        codigoConfirmacion: codigoConfirmacionFinal || codigoReservaFinal || `RES-${Date.now().toString(36).toUpperCase()}`,
        idReserva: idReservaFinal,
        vehiculo: `${vehiculo?.marca} ${vehiculo?.modelo || vehiculo?.modeloVehiculo}`,
        fechaRecogida: form.fechaRecogida,
        fechaDevolucion: form.fechaDevolucion,
        total: totalFinal,
        estado: 'CONFIRMADA',
      });

      toast.success('¡Reserva creada exitosamente!');
      toast.success('¡Pago simulado exitosamente!');
    } catch (err) {
      const message = err?.response?.data?.message || err?.response?.data?.Mensaje || 'No se pudo generar la reserva.';
      toast.error(message);
    } finally {
      setProcessing(false);
    }
  };

  if (loading) {
    return (
      <div className="reservar-page">
        <div className="reservar-loading">
          <Loader2 size={40} className="spin" />
          <p>Cargando información del vehículo...</p>
        </div>
      </div>
    );
  }

  if (reservaConfirmada) {
    return (
      <div className="reservar-page">
        <div className="reservar-confirmacion">
          <div className="confirmacion-card">
            <div className="confirmacion-icon">
              <CheckCircle2 size={64} />
            </div>
            <h1>¡Reserva Confirmada!</h1>
            <p className="confirmacion-subtitle">Tu reserva ha sido procesada exitosamente</p>

            <div className="confirmacion-details">
              <div className="confirmacion-detail">
                <span className="confirmacion-label">Código de Reserva</span>
                <span className="confirmacion-value confirmacion-value--code">{reservaConfirmada.codigoReserva}</span>
              </div>
              <div className="confirmacion-detail">
                <span className="confirmacion-label">Confirmación</span>
                <span className="confirmacion-value">{reservaConfirmada.codigoConfirmacion}</span>
              </div>
              <div className="confirmacion-detail">
                <span className="confirmacion-label">Vehículo</span>
                <span className="confirmacion-value">{reservaConfirmada.vehiculo}</span>
              </div>
              <div className="confirmacion-detail">
                <span className="confirmacion-label">Recogida</span>
                <span className="confirmacion-value">{new Date(reservaConfirmada.fechaRecogida).toLocaleString('es-EC')}</span>
              </div>
              <div className="confirmacion-detail">
                <span className="confirmacion-label">Devolución</span>
                <span className="confirmacion-value">{new Date(reservaConfirmada.fechaDevolucion).toLocaleString('es-EC')}</span>
              </div>
              {reservaConfirmada.pago && (
                <div className="confirmacion-detail">
                  <span className="confirmacion-label">Ref. de Pago</span>
                  <span className="confirmacion-value" style={{ fontSize: '0.85rem' }}>{reservaConfirmada.pago.referencia}</span>
                </div>
              )}
              <div className="confirmacion-detail confirmacion-detail--total">
                <span className="confirmacion-label">Total Pagado</span>
                <span className="confirmacion-value">${reservaConfirmada.total.toFixed(2)}</span>
              </div>
              {reservaConfirmada.pago && (
                <div className="confirmacion-detail" style={{ justifyContent: 'center' }}>
                  <span style={{ color: 'var(--color-success)', fontWeight: 600, fontSize: '0.9rem' }}>
                    ✅ Pago {reservaConfirmada.pago.estado} — ID #{reservaConfirmada.pago.idPago}
                  </span>
                </div>
              )}
            </div>

            <div className="confirmacion-actions">
              <Link to="/mi-cuenta" className="btn btn--primary">Ir a Mi Cuenta</Link>
              <Link to="/catalogo" className="btn btn--outline">Seguir Explorando</Link>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="reservar-page">
      <nav className="home-nav">
        <div className="home-nav__inner">
          <Link to="/catalogo" className="home-nav__logo">
            <ArrowLeft size={20} />
            <span>Volver al catálogo</span>
          </Link>
        </div>
      </nav>

      <div className="reservar-content">
        {/* Steps indicator */}
        <div className="reservar-steps">
          {STEPS.map((s, i) => (
            <div key={s} className={`reservar-step ${i === step ? 'reservar-step--active' : ''} ${i < step ? 'reservar-step--done' : ''}`}>
              <div className="reservar-step__number">
                {i < step ? <Check size={16} /> : i + 1}
              </div>
              <span className="reservar-step__label">{s}</span>
            </div>
          ))}
        </div>

        <div className="reservar-body">
          {/* Vehicle summary sidebar */}
          <div className="reservar-sidebar">
            <div className="reservar-vehicle-card">
              <div className="reservar-vehicle-card__img">
                {isValidImageUrl(vehiculo?.imagenUrl) ? (
                  <img src={vehiculo.imagenUrl}
                    alt={`${vehiculo.marca} ${vehiculo.modelo}`} />
                ) : (
                  <div className="reservar-vehicle-card__placeholder">
                    <Car size={48} />
                    <span>{vehiculo?.marca} {vehiculo?.modelo}</span>
                  </div>
                )}
              </div>
              <h3>{nombreVehiculo}</h3>
              <div className="reservar-step-desc" style={{ marginBottom: '0.75rem' }}>
                {categoriaVehiculo} • Año {anioVehiculo}
              </div>
              <div className="reservar-vehicle-specs">
                <span><Users size={14} /> {vehiculo?.capacidadPasajeros} pax</span>
                <span><Fuel size={14} /> {vehiculo?.tipoCombustible}</span>
                <span><Settings2 size={14} /> {vehiculo?.tipoTransmision}</span>
              </div>
              <div className="reservar-info" style={{ marginTop: '0.75rem' }}>
                <Car size={16} />
                <span>Placa: <strong>{placaVehiculo}</strong> • Puertas: <strong>{puertasVehiculo}</strong> • Maletas: <strong>{maletasVehiculo}</strong></span>
              </div>
              <div className="reservar-info" style={{ marginTop: '0.5rem' }}>
                <MapPin size={16} />
                <span>Sucursal del vehículo: <strong>{localizacionActual?.nombreLocalizacion || localizacionActual?.nombre || 'No disponible'}</strong></span>
              </div>
              <div className="reservar-info" style={{ marginTop: '0.5rem' }}>
                <ShieldCheck size={16} />
                <span>Incluye kilometraje estándar y asistencia básica en carretera.</span>
              </div>
              <div className="reservar-price-summary">
                <div className="reservar-price-line">
                  <span>Vehículo ({dias} {dias === 1 ? 'día' : 'días'})</span>
                  <span>${subtotalVehiculo.toFixed(2)}</span>
                </div>
                {form.extrasSeleccionados.length > 0 && (
                  <div className="reservar-price-line">
                    <span>Extras</span>
                    <span>${subtotalExtras.toFixed(2)}</span>
                  </div>
                )}
                {conductoresAdicionales > 0 && (
                  <div className="reservar-price-line">
                    <span>Recargo conductores ({conductoresAdicionales})</span>
                    <span>${recargoConductores.toFixed(2)}</span>
                  </div>
                )}
                {isOneWay && (
                  <div className="reservar-price-line">
                    <span>Cargo One-Way</span>
                    <span>${cargoOneWay.toFixed(2)}</span>
                  </div>
                )}
                <div className="reservar-price-line">
                  <span>IVA (15%)</span>
                  <span>${iva.toFixed(2)}</span>
                </div>
                <div className="reservar-price-line reservar-price-total">
                  <span>Total</span>
                  <span>${totalFinal.toFixed(2)}</span>
                </div>
              </div>
            </div>
          </div>

          {/* Step Content */}
          <div className="reservar-main">
            {/* Guest Identification Step */}
            {STEPS[step] === 'Identificación' && (
              <div className="reservar-step-content">
                <h2><UserPlus size={24} /> Datos del Cliente</h2>
                <p style={{color:'var(--color-text-secondary)', marginBottom:'1.5rem'}}>
                  Ingresa tus datos para continuar. Si ya tienes una cuenta, <a href="/login" style={{color:'var(--color-primary)'}}>inicia sesión aquí</a>.
                  <br /><small>Si tu cédula ya está registrada, se reutilizará tu perfil existente.</small>
                </p>
                <div className="reservar-form-grid">
                  <div className="form-group">
                    <label className="form-label">Nombre *</label>
                    <input className="form-input" placeholder="Juan" value={guestForm.nombre}
                      onChange={e => setGuestForm({...guestForm, nombre: e.target.value})} />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Apellido</label>
                    <input className="form-input" placeholder="Pérez" value={guestForm.apellido}
                      onChange={e => setGuestForm({...guestForm, apellido: e.target.value})} />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Cédula / Identificación *</label>
                    <input className="form-input" placeholder="1712345678" value={guestForm.cedula}
                      onChange={e => setGuestForm({...guestForm, cedula: e.target.value})} />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Correo Electrónico</label>
                    <input className="form-input" type="email" placeholder="correo@ejemplo.com" value={guestForm.correo}
                      onChange={e => setGuestForm({...guestForm, correo: e.target.value})} />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Teléfono</label>
                    <input className="form-input" placeholder="0991234567" value={guestForm.telefono}
                      onChange={e => setGuestForm({...guestForm, telefono: e.target.value})} />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Dirección</label>
                    <input className="form-input" placeholder="Av. Principal 123" value={guestForm.direccion}
                      onChange={e => setGuestForm({...guestForm, direccion: e.target.value})} />
                  </div>
                </div>
                <div style={{marginTop:'1.5rem', display:'flex', justifyContent:'flex-end'}}>
                  <button className="btn btn--primary" onClick={handleGuestClient} disabled={guestProcessing}>
                    {guestProcessing ? <><Loader2 size={16} className="spin" /> Verificando...</> : <><ArrowRight size={16} /> Continuar</>}
                  </button>
                </div>
              </div>
            )}

            {/* Step: Dates */}
            {STEPS[step] === 'Fechas' && (
              <div className="reservar-step-content">
                <h2><Calendar size={24} /> Fechas y Ubicación</h2>
                <div className="reservar-form-grid">
                  <div className={stepErrors.fechaRecogida ? 'form-group--error' : ''}>
                    <DateTimePicker
                      id="fecha-recogida"
                      label="Fecha y hora de Recogida *"
                      value={form.fechaRecogida}
                      onChange={(val) => { setForm({ ...form, fechaRecogida: val }); setStepErrors(e => ({...e, fechaRecogida: '', disponibilidad: ''})); }}
                    />
                    {stepErrors.fechaRecogida && <span className="form-error"><AlertCircle size={13} /> {stepErrors.fechaRecogida}</span>}
                  </div>
                  <div className={stepErrors.fechaDevolucion ? 'form-group--error' : ''}>
                    <DateTimePicker
                      id="fecha-devolucion"
                      label="Fecha y hora de Devolución *"
                      value={form.fechaDevolucion}
                      minDate={form.fechaRecogida}
                      onChange={(val) => { setForm({ ...form, fechaDevolucion: val }); setStepErrors(e => ({...e, fechaDevolucion: '', disponibilidad: ''})); }}
                    />
                    {stepErrors.fechaDevolucion && <span className="form-error"><AlertCircle size={13} /> {stepErrors.fechaDevolucion}</span>}
                  </div>
                  <div className={`form-group ${stepErrors.idLocalizacionRecogida ? 'form-group--error' : ''}`}>
                    <label className="form-label"><MapPin size={16} /> Sucursal de Recogida *</label>
                    <select className="form-input" value={form.idLocalizacionRecogida}
                      onChange={(e) => { setForm({ ...form, idLocalizacionRecogida: e.target.value }); setStepErrors(er => ({...er, idLocalizacionRecogida: '', disponibilidad: ''})); }}>
                      <option value="">Seleccionar...</option>
                      {localizaciones.map((l) => (
                        <option key={l.idLocalizacion || l.id} value={l.idLocalizacion || l.id}>
                          {l.nombreLocalizacion || l.nombre}
                        </option>
                      ))}
                    </select>
                    {stepErrors.idLocalizacionRecogida && <span className="form-error"><AlertCircle size={13} /> {stepErrors.idLocalizacionRecogida}</span>}
                  </div>
                  <div className={`form-group ${stepErrors.idLocalizacionDevolucion ? 'form-group--error' : ''}`}>
                    <label className="form-label"><MapPin size={16} /> Sucursal de Devolución *</label>
                    <select className="form-input" value={form.idLocalizacionDevolucion}
                      onChange={(e) => { setForm({ ...form, idLocalizacionDevolucion: e.target.value }); setStepErrors(er => ({...er, idLocalizacionDevolucion: ''})); }}>
                      <option value="">Seleccionar...</option>
                      {localizaciones.map((l) => (
                        <option key={l.idLocalizacion || l.id} value={l.idLocalizacion || l.id}>
                          {l.nombreLocalizacion || l.nombre}
                        </option>
                      ))}
                    </select>
                    {stepErrors.idLocalizacionDevolucion && <span className="form-error"><AlertCircle size={13} /> {stepErrors.idLocalizacionDevolucion}</span>}
                  </div>
                </div>
                {isOneWay && (
                  <div className="reservar-info">
                    <ShieldCheck size={16} />
                    <span>Se aplicará un cargo de <strong>$25.00</strong> por devolución en diferente sucursal.</span>
                  </div>
                )}
                {checkingDisponibilidad && (
                  <div className="reservar-info">
                    <Loader2 size={16} className="spin" />
                    <span>Validando disponibilidad en tiempo real...</span>
                  </div>
                )}
                {stepErrors.disponibilidad && (
                  <div className="form-error" style={{ marginTop: '0.75rem' }}>
                    <AlertCircle size={13} /> {stepErrors.disponibilidad}
                  </div>
                )}
              </div>
            )}

            {/* Step 1: Conductores */}
            {STEPS[step] === 'Conductores' && (
              <div className="reservar-step-content">
                <h2><Users size={24} /> Conductores</h2>
                <p className="reservar-step-desc">El cliente se asigna automáticamente como conductor principal. Puedes cambiarlo o agregar conductores adicionales.</p>

                {stepErrors.conductores && <div className="form-error" style={{ marginBottom: '1rem' }}><AlertCircle size={13} /> {stepErrors.conductores}</div>}

                <div className="extras-grid">
                  {form.conductores.map((c, idx) => (
                    <div key={idx} className={`extra-card ${c.esPrincipal ? 'extra-card--selected' : ''}`}>
                      <div className="extra-card__header">
                        <div className="extra-card__name">
                          {c.esPrincipal ? <ShieldCheck size={16} /> : <Users size={16} />}
                          <span>{c.nombre || 'Conductor'}</span>
                        </div>
                        <span className="extra-card__price">{c.esPrincipal ? 'Principal' : 'Adicional'}</span>
                      </div>
                      <div className="extra-card__meta" style={{ fontSize: '0.85rem', opacity: 0.8 }}>
                        {c.licencia && <p>Licencia: {c.licencia}</p>}
                        {c.telefono && <p>Tel: {c.telefono}</p>}
                        {c.esCliente && <p style={{ color: 'var(--accent)', fontWeight: 600 }}>👤 Titular de la cuenta</p>}
                      </div>
                      <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
                        {!c.esPrincipal && (
                          <button className="btn btn--ghost btn--sm"
                            onClick={() => {
                              setConductorPrincipal(idx);
                            }}>Hacer Principal</button>
                        )}
                        {!c.esCliente && (
                          <button className="btn btn--ghost btn--sm" style={{ color: '#e74c3c' }}
                            onClick={() => setForm(prev => ({
                              ...prev,
                              conductores: prev.conductores.filter((_, i) => i !== idx)
                            }))}><X size={14} /> Quitar</button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>

                {!showAddConductor ? (
                  <button className="btn btn--outline" style={{ marginTop: '1rem' }}
                    onClick={() => setShowAddConductor(true)}>
                    <Plus size={16} /> Agregar Conductor Adicional
                  </button>
                ) : (
                  <div className="pago-card" style={{ marginTop: '1rem', padding: '1.5rem' }}>
                    <h4 style={{ marginBottom: '1rem' }}>Nuevo Conductor</h4>
                    <div className="pago-form__row" style={{ gap: '1rem', flexWrap: 'wrap' }}>
                      <div className={`form-group ${newConductorErrors.nombre ? 'form-group--error' : ''}`} style={{ flex: 1, minWidth: '200px' }}>
                        <label className="form-label">Nombre *</label>
                        <input className="form-input" placeholder="Nombre completo"
                          value={newConductor.nombre}
                          maxLength={50}
                          onChange={(e) => {
                            setNewConductor({ ...newConductor, nombre: e.target.value.replace(/\s{2,}/g, ' ') });
                            setNewConductorErrors(prev => ({ ...prev, nombre: '' }));
                          }} />
                        {newConductorErrors.nombre && <span className="form-error"><AlertCircle size={13} /> {newConductorErrors.nombre}</span>}
                      </div>
                      <div className={`form-group ${newConductorErrors.apellido ? 'form-group--error' : ''}`} style={{ flex: 1, minWidth: '200px' }}>
                        <label className="form-label">Apellido *</label>
                        <input className="form-input" placeholder="Apellido"
                          value={newConductor.apellido}
                          maxLength={50}
                          onChange={(e) => {
                            setNewConductor({ ...newConductor, apellido: e.target.value.replace(/\s{2,}/g, ' ') });
                            setNewConductorErrors(prev => ({ ...prev, apellido: '' }));
                          }} />
                        {newConductorErrors.apellido && <span className="form-error"><AlertCircle size={13} /> {newConductorErrors.apellido}</span>}
                      </div>
                    </div>
                    <div className="pago-form__row" style={{ gap: '1rem', flexWrap: 'wrap' }}>
                      <div className={`form-group ${newConductorErrors.licencia ? 'form-group--error' : ''}`} style={{ flex: 1, minWidth: '150px' }}>
                        <label className="form-label">No. Licencia *</label>
                        <input className="form-input" placeholder="Licencia de conducir"
                          value={newConductor.licencia}
                          maxLength={20}
                          onChange={(e) => {
                            setNewConductor({ ...newConductor, licencia: e.target.value.toUpperCase() });
                            setNewConductorErrors(prev => ({ ...prev, licencia: '' }));
                          }} />
                        {newConductorErrors.licencia && <span className="form-error"><AlertCircle size={13} /> {newConductorErrors.licencia}</span>}
                      </div>
                      <div className={`form-group ${newConductorErrors.edad ? 'form-group--error' : ''}`} style={{ flex: 1, minWidth: '100px' }}>
                        <label className="form-label">Edad *</label>
                        <input className="form-input" type="number" placeholder="25"
                          value={newConductor.edad}
                          min={18}
                          max={85}
                          onChange={(e) => {
                            setNewConductor({ ...newConductor, edad: e.target.value.replace(/[^\d]/g, '') });
                            setNewConductorErrors(prev => ({ ...prev, edad: '' }));
                          }} />
                        {newConductorErrors.edad && <span className="form-error"><AlertCircle size={13} /> {newConductorErrors.edad}</span>}
                      </div>
                      <div className={`form-group ${newConductorErrors.telefono ? 'form-group--error' : ''}`} style={{ flex: 1, minWidth: '150px' }}>
                        <label className="form-label">Teléfono *</label>
                        <input className="form-input" placeholder="+593..."
                          value={newConductor.telefono}
                          maxLength={20}
                          onChange={(e) => {
                            setNewConductor({ ...newConductor, telefono: e.target.value.replace(/[^\d+()\-\s]/g, '') });
                            setNewConductorErrors(prev => ({ ...prev, telefono: '' }));
                          }} />
                        {newConductorErrors.telefono && <span className="form-error"><AlertCircle size={13} /> {newConductorErrors.telefono}</span>}
                      </div>
                    </div>
                    <label style={{ display: 'inline-flex', alignItems: 'center', gap: 8, marginTop: 6 }}>
                      <input
                        type="checkbox"
                        checked={newConductor.esPrincipal}
                        onChange={(e) => setNewConductor({ ...newConductor, esPrincipal: e.target.checked })}
                      />
                      Marcar como conductor principal
                    </label>
                    <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
                      <button className="btn btn--primary btn--sm"
                        onClick={handleAddConductor}><Check size={16} /> Agregar</button>
                      <button className="btn btn--ghost btn--sm"
                        onClick={() => {
                          setShowAddConductor(false);
                          setNewConductorErrors({});
                          setNewConductor({ nombre: '', apellido: '', licencia: '', edad: '', telefono: '', esPrincipal: false });
                        }}><X size={16} /> Cancelar</button>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Step 2: Extras */}
            {STEPS[step] === 'Extras' && (
              <div className="reservar-step-content">
                <h2><Package size={24} /> Extras y Accesorios</h2>
                <p className="reservar-step-desc">Personaliza tu experiencia con extras opcionales</p>
                {extras.length === 0 ? (
                  <p className="text-muted">No hay extras disponibles.</p>
                ) : (
                  <div className="extras-grid">
                    {extras.map((extra) => {
                      const selected = form.extrasSeleccionados.find(e => e.id === (extra.idExtra || extra.id));
                      const isCondAdic = isConductorExtra(extra);
                      const lockCondAdic = isCondAdic && conductoresAdicionales > 0;
                      return (
                        <div key={extra.idExtra || extra.id}
                          className={`extra-card ${selected ? 'extra-card--selected' : ''}`}
                          style={lockCondAdic ? { cursor: 'not-allowed', opacity: 0.92 } : undefined}
                          onClick={() => toggleExtra(extra)}
                        >
                          <div className="extra-card__header">
                            <Package size={24} />
                            <div className="extra-card__check">
                              {selected && <Check size={16} />}
                            </div>
                          </div>
                          <h4 className="extra-card__name">{extra.nombreExtra || extra.nombre}</h4>
                          <p className="extra-card__desc">{extra.descripcionExtra || extra.descripcion || 'Servicio adicional'}</p>
                          <div className="extra-card__price">
                            ${Number(extra.valorFijo || extra.precio || 0).toFixed(2)}/día
                          </div>
                          {isCondAdic && (
                            <p className="extra-card__desc" style={{ marginTop: '0.4rem', fontWeight: 600 }}>
                              {conductoresAdicionales > 0
                                ? 'Aplicado automáticamente por conductor adicional'
                                : 'Requiere agregar conductor adicional'}
                            </p>
                          )}
                          {selected && (
                            <div className="extra-card__qty" onClick={(e) => e.stopPropagation()}>
                              <button
                                className="extra-card__qty-btn"
                                disabled={isCondAdic}
                                onClick={() => updateExtraCantidad(selected.id, -1)}
                              >
                                <Minus size={14} />
                              </button>
                              <span>{selected.cantidad}</span>
                              <button
                                className="extra-card__qty-btn"
                                disabled={isCondAdic}
                                onClick={() => updateExtraCantidad(selected.id, 1)}
                              >
                                <Plus size={14} />
                              </button>
                            </div>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
                {conductoresAdicionales > 0 && (
                  <div className="reservar-info" style={{ marginTop: '1rem' }}>
                    <ShieldCheck size={16} />
                    <span>
                      Recargo por conductor adicional: <strong>${RECARGO_CONDUCTOR_ADICIONAL_DIA.toFixed(2)}</strong> por día x {conductoresAdicionales} conductor(es).
                    </span>
                  </div>
                )}
              </div>
            )}

            {/* Step 3: Summary */}
            {STEPS[step] === 'Resumen' && (
              <div className="reservar-step-content">
                <h2><ShieldCheck size={24} /> Resumen de Reserva</h2>
                <div className="resumen-grid">
                  <div className="resumen-section">
                    <h4>Vehículo</h4>
                    <p>{vehiculo?.marca} {vehiculo?.modelo || vehiculo?.modeloVehiculo} ({vehiculo?.anioFabricacion})</p>
                  </div>
                  <div className="resumen-section">
                    <h4>Fechas</h4>
                    <p><strong>Recogida:</strong> {new Date(form.fechaRecogida).toLocaleString('es-EC')}</p>
                    <p><strong>Devolución:</strong> {new Date(form.fechaDevolucion).toLocaleString('es-EC')}</p>
                    <p><strong>Duración:</strong> {dias} {dias === 1 ? 'día' : 'días'}</p>
                  </div>
                  <div className="resumen-section">
                    <h4>Ubicaciones</h4>
                    <p><strong>Recogida:</strong> {(() => { const l = localizaciones.find(l => String(l.idLocalizacion ?? l.id) === String(form.idLocalizacionRecogida)); return l?.nombreLocalizacion || l?.nombre || `ID ${form.idLocalizacionRecogida}`; })()}</p>
                    <p><strong>Devolución:</strong> {(() => { const l = localizaciones.find(l => String(l.idLocalizacion ?? l.id) === String(form.idLocalizacionDevolucion)); return l?.nombreLocalizacion || l?.nombre || `ID ${form.idLocalizacionDevolucion}`; })()}</p>
                  </div>
                  {form.extrasSeleccionados.length > 0 && (
                    <div className="resumen-section">
                      <h4>Extras</h4>
                      {form.extrasSeleccionados.map(ex => (
                        <p key={ex.id}>{ex.nombre} x{ex.cantidad} — ${(Number(ex.valorFijo) * ex.cantidad * dias).toFixed(2)}</p>
                      ))}
                    </div>
                  )}
                  {conductoresAdicionales > 0 && (
                    <div className="resumen-section">
                      <h4>Recargo por Conductores</h4>
                      <p>{conductoresAdicionales} adicional(es) x ${RECARGO_CONDUCTOR_ADICIONAL_DIA.toFixed(2)}/día</p>
                      <p><strong>Total recargo:</strong> ${recargoConductores.toFixed(2)}</p>
                    </div>
                  )}
                  <div className="resumen-section">
                    <h4>Cliente</h4>
                    <p>{user?.nombreCompleto || user?.username}</p>
                    <p>{user?.correo}</p>
                  </div>
                </div>
              </div>
            )}

            {/* Step 4: Payment */}
            {STEPS[step] === 'Pago' && (
              <div className="reservar-step-content">
                <h2><CreditCard size={24} /> Pasarela de Pago</h2>
                <div className="pago-card">
                  <div className="pago-card__visual">
                    <div className="pago-card__chip" />
                    <div className="pago-card__number">
                      {pago.numeroTarjeta ? pago.numeroTarjeta.replace(/(.{4})/g, '$1 ').trim() : '•••• •••• •••• ••••'}
                    </div>
                    <div className="pago-card__details">
                      <div>
                        <span className="pago-card__label">TITULAR</span>
                        <span className="pago-card__value">{pago.nombreTitular || 'NOMBRE COMPLETO'}</span>
                      </div>
                      <div>
                        <span className="pago-card__label">EXPIRA</span>
                        <span className="pago-card__value">
                          {pago.mesExpiracion || 'MM'}/{pago.anioExpiracion || 'AA'}
                        </span>
                      </div>
                    </div>
                  </div>

                  <div className="pago-form">
                    <div className={`form-group ${stepErrors.nombreTitular ? 'form-group--error' : ''}`}>
                      <label className="form-label">Nombre del Titular *</label>
                      <input type="text" className="form-input" placeholder="Como aparece en la tarjeta"
                        value={pago.nombreTitular}
                        onChange={(e) => { setPago({ ...pago, nombreTitular: e.target.value.toUpperCase() }); setStepErrors(er => ({...er, nombreTitular: ''})); }} />
                      {stepErrors.nombreTitular && <span className="form-error"><AlertCircle size={13} /> {stepErrors.nombreTitular}</span>}
                    </div>
                    <div className={`form-group ${stepErrors.numeroTarjeta ? 'form-group--error' : ''}`}>
                      <label className="form-label">Número de Tarjeta *</label>
                      <input type="text" className="form-input" placeholder="4242 4242 4242 4242"
                        maxLength={19} value={pago.numeroTarjeta}
                        onChange={(e) => { setPago({ ...pago, numeroTarjeta: e.target.value.replace(/\D/g, '').slice(0, 16) }); setStepErrors(er => ({...er, numeroTarjeta: ''})); }} />
                      {stepErrors.numeroTarjeta && <span className="form-error"><AlertCircle size={13} /> {stepErrors.numeroTarjeta}</span>}
                    </div>
                    <div className="pago-form__row">
                      <div className={`form-group ${stepErrors.mesExpiracion ? 'form-group--error' : ''}`}>
                        <label className="form-label">Mes *</label>
                        <select className="form-input" value={pago.mesExpiracion}
                          onChange={(e) => { setPago({ ...pago, mesExpiracion: e.target.value }); setStepErrors(er => ({...er, mesExpiracion: ''})); }}>
                          <option value="">MM</option>
                          {Array.from({ length: 12 }, (_, i) => (
                            <option key={i} value={String(i + 1).padStart(2, '0')}>
                              {String(i + 1).padStart(2, '0')}
                            </option>
                          ))}
                        </select>
                        {stepErrors.mesExpiracion && <span className="form-error"><AlertCircle size={13} /> {stepErrors.mesExpiracion}</span>}
                      </div>
                      <div className={`form-group ${stepErrors.anioExpiracion ? 'form-group--error' : ''}`}>
                        <label className="form-label">Año *</label>
                        <select className="form-input" value={pago.anioExpiracion}
                          onChange={(e) => { setPago({ ...pago, anioExpiracion: e.target.value }); setStepErrors(er => ({...er, anioExpiracion: ''})); }}>
                          <option value="">AA</option>
                          {Array.from({ length: 10 }, (_, i) => (
                            <option key={i} value={String(26 + i)}>{2026 + i}</option>
                          ))}
                        </select>
                        {stepErrors.anioExpiracion && <span className="form-error"><AlertCircle size={13} /> {stepErrors.anioExpiracion}</span>}
                      </div>
                      <div className={`form-group ${stepErrors.cvv ? 'form-group--error' : ''}`}>
                        <label className="form-label">CVV *</label>
                        <input type="password" className="form-input" placeholder="•••" maxLength={4}
                          value={pago.cvv}
                          onChange={(e) => { setPago({ ...pago, cvv: e.target.value.replace(/\D/g, '').slice(0, 4) }); setStepErrors(er => ({...er, cvv: ''})); }} />
                        {stepErrors.cvv && <span className="form-error"><AlertCircle size={13} /> {stepErrors.cvv}</span>}
                      </div>
                    </div>
                  </div>

                  <div className="pago-total">
                    <span>Total a pagar:</span>
                    <span className="pago-total__amount">${totalFinal.toFixed(2)} USD</span>
                  </div>
                  <p className="pago-disclaimer">
                    🔒 Pago simulado — No se realizará ningún cargo real.
                  </p>
                </div>
              </div>
            )}

            {/* Navigation Buttons */}
            <div className={`reservar-nav ${shake ? 'form-shake' : ''}`}>
              {step > 0 && (
                <button className="btn btn--outline" onClick={() => { setStep(step - 1); setStepErrors({}); }} disabled={processing}>
                  <ArrowLeft size={16} /> Anterior
                </button>
              )}
              <div className="reservar-nav__spacer" />
              {step < 4 ? (
                <button className="btn btn--primary" onClick={handleNext}>
                  Siguiente <ArrowRight size={16} />
                </button>
              ) : (
                <button className="btn btn--primary btn--lg" onClick={() => { const errs = getStepErrors(); if (Object.keys(errs).length > 0) { setStepErrors(errs); setShake(true); setTimeout(() => setShake(false), 500); toast.error(Object.values(errs)[0]); return; } handlePagar(); }}
                  disabled={processing}>
                  {processing ? (
                    <><Loader2 size={18} className="spin" /> Procesando pago...</>
                  ) : (
                    <><CreditCard size={18} /> Confirmar y Pagar ${totalFinal.toFixed(2)}</>
                  )}
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
