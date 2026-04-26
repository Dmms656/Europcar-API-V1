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
  Minus, ShieldCheck, Loader2, CheckCircle2, X, AlertCircle
} from 'lucide-react';
import { toast } from 'sonner';

const STEPS = ['Fechas', 'Conductores', 'Extras', 'Resumen', 'Pago'];
const IVA_RATE = 0.15;
const isValidImageUrl = (url) => url && (url.startsWith('http://') || url.startsWith('https://'));

export default function ReservarPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuthStore();

  const [step, setStep] = useState(0);
  const [vehiculo, setVehiculo] = useState(null);
  const [localizaciones, setLocalizaciones] = useState([]);
  const [extras, setExtras] = useState([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [reservaConfirmada, setReservaConfirmada] = useState(null);
  const [stepErrors, setStepErrors] = useState({});
  const [shake, setShake] = useState(false);

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
  const [newConductor, setNewConductor] = useState({ nombre: '', apellido: '', licencia: '', edad: '', telefono: '' });

  // Payment form
  const [pago, setPago] = useState({
    nombreTitular: '',
    numeroTarjeta: '',
    mesExpiracion: '',
    anioExpiracion: '',
    cvv: '',
  });

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login', { state: { from: { pathname: `/reservar/${id}` } } });
      return;
    }
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
  const subtotalVehiculo = precioBase * dias;
  const subtotalExtras = form.extrasSeleccionados.reduce((acc, ex) => {
    return acc + (Number(ex.valorFijo || ex.precio || 0) * ex.cantidad * dias);
  }, 0);
  const subtotal = subtotalVehiculo + subtotalExtras;
  const iva = subtotal * IVA_RATE;
  const total = subtotal + iva;
  const isOneWay = form.idLocalizacionRecogida !== form.idLocalizacionDevolucion;
  const cargoOneWay = isOneWay ? 25.00 : 0;
  const totalFinal = total + cargoOneWay;

  const toggleExtra = (extra) => {
    setForm(prev => {
      const exists = prev.extrasSeleccionados.find(e => e.id === (extra.idExtra || extra.id));
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
    if (step === 0) {
      if (!form.fechaRecogida) errs.fechaRecogida = 'Selecciona fecha de recogida';
      if (!form.fechaDevolucion) errs.fechaDevolucion = 'Selecciona fecha de devolución';
      if (form.fechaRecogida && form.fechaDevolucion && new Date(form.fechaDevolucion) <= new Date(form.fechaRecogida))
        errs.fechaDevolucion = 'La devolución debe ser posterior a la recogida';
      if (!form.idLocalizacionRecogida) errs.idLocalizacionRecogida = 'Selecciona sucursal de recogida';
      if (!form.idLocalizacionDevolucion) errs.idLocalizacionDevolucion = 'Selecciona sucursal de devolución';
    }
    if (step === 1) {
      if (form.conductores.length === 0) errs.conductores = 'Debe haber al menos un conductor';
      if (!form.conductores.some(c => c.esPrincipal)) errs.conductores = 'Debe haber un conductor principal';
    }
    if (step === 4) {
      if (!pago.nombreTitular.trim()) errs.nombreTitular = 'Nombre del titular requerido';
      if (!pago.numeroTarjeta || pago.numeroTarjeta.replace(/\s/g, '').length < 16) errs.numeroTarjeta = 'Número de tarjeta inválido (16 dígitos)';
      if (!pago.mesExpiracion) errs.mesExpiracion = 'Mes requerido';
      if (!pago.anioExpiracion) errs.anioExpiracion = 'Año requerido';
      if (!pago.cvv || pago.cvv.length < 3) errs.cvv = 'CVV inválido';
    }
    return errs;
  };

  const handleNext = () => {
    const errs = getStepErrors();
    if (Object.keys(errs).length > 0) {
      setStepErrors(errs);
      setShake(true);
      setTimeout(() => setShake(false), 500);
      const first = Object.values(errs)[0];
      toast.error(first);
      return;
    }
    setStepErrors({});
    if (step < STEPS.length - 1) setStep(step + 1);
  };

  const handlePagar = async () => {
    setProcessing(true);
    try {
      // ── STEP 1: Create Reservation ──
      const reservaPayload = {
        idCliente: user?.idCliente || 0,
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

      let reservaData = null;
      let codigoReserva, codigoConfirmacion, idReserva;

      try {
        const res = await reservasApi.create(reservaPayload);
        reservaData = res.data?.data;
        codigoReserva = reservaData?.codigoReserva;
        codigoConfirmacion = reservaData?.codigoConfirmacion;
        idReserva = reservaData?.idReserva;
        toast.success('Reserva creada correctamente');
      } catch (apiErr) {
        console.warn('Error creando reserva:', apiErr);
        const errMsg = apiErr.response?.data?.message || apiErr.response?.data?.title || 'Error al crear la reserva';
        toast.error(errMsg);
        setProcessing(false);
        return;
      }

      // ── STEP 2: Register Payment ──
      let pagoData = null;
      try {
        const pagoPayload = {
          idReserva: idReserva,
          idCliente: user?.idCliente || 0,
          tipoPago: 'COBRO',
          metodoPago: 'TARJETA',
          monto: totalFinal,
          referenciaExterna: `SIM-${pago.numeroTarjeta.slice(-4)}-${Date.now().toString(36).toUpperCase()}`,
          observaciones: `Pago web - Tarjeta terminada en ${pago.numeroTarjeta.slice(-4)} - Titular: ${pago.nombreTitular}`,
        };
        const pagoRes = await pagosApi.create(pagoPayload);
        pagoData = pagoRes.data?.data;
        toast.success('Pago registrado correctamente');
      } catch (pagoErr) {
        console.warn('Error registrando pago (reserva ya creada):', pagoErr);
        // Payment failed but reservation was created — don't block confirmation
      }

      // ── STEP 3: Show Confirmation ──
      setReservaConfirmada({
        codigoReserva: codigoReserva || `RES-${Date.now().toString(36).toUpperCase()}`,
        codigoConfirmacion: codigoConfirmacion || codigoReserva,
        idReserva,
        vehiculo: `${vehiculo?.marca} ${vehiculo?.modelo || vehiculo?.modeloVehiculo}`,
        fechaRecogida: form.fechaRecogida,
        fechaDevolucion: form.fechaDevolucion,
        total: totalFinal,
        estado: 'CONFIRMADA',
        pago: pagoData ? {
          idPago: pagoData.idPago,
          referencia: pagoData.referenciaExterna,
          estado: pagoData.estadoPago || 'COMPLETADO',
        } : null,
      });

      toast.success('¡Reserva confirmada exitosamente!');
    } catch (e) {
      console.error('Error en flujo de reserva:', e);
      toast.error('Error al procesar la reserva');
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
              <h3>{vehiculo?.marca} {vehiculo?.modelo || vehiculo?.modeloVehiculo}</h3>
              <div className="reservar-vehicle-specs">
                <span><Users size={14} /> {vehiculo?.capacidadPasajeros} pax</span>
                <span><Fuel size={14} /> {vehiculo?.tipoCombustible}</span>
                <span><Settings2 size={14} /> {vehiculo?.tipoTransmision}</span>
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
            {/* Step 0: Dates */}
            {step === 0 && (
              <div className="reservar-step-content">
                <h2><Calendar size={24} /> Fechas y Ubicación</h2>
                <div className="reservar-form-grid">
                  <div className={stepErrors.fechaRecogida ? 'form-group--error' : ''}>
                    <DateTimePicker
                      id="fecha-recogida"
                      label="Fecha y hora de Recogida *"
                      value={form.fechaRecogida}
                      onChange={(val) => { setForm({ ...form, fechaRecogida: val }); setStepErrors(e => ({...e, fechaRecogida: ''})); }}
                    />
                    {stepErrors.fechaRecogida && <span className="form-error"><AlertCircle size={13} /> {stepErrors.fechaRecogida}</span>}
                  </div>
                  <div className={stepErrors.fechaDevolucion ? 'form-group--error' : ''}>
                    <DateTimePicker
                      id="fecha-devolucion"
                      label="Fecha y hora de Devolución *"
                      value={form.fechaDevolucion}
                      minDate={form.fechaRecogida}
                      onChange={(val) => { setForm({ ...form, fechaDevolucion: val }); setStepErrors(e => ({...e, fechaDevolucion: ''})); }}
                    />
                    {stepErrors.fechaDevolucion && <span className="form-error"><AlertCircle size={13} /> {stepErrors.fechaDevolucion}</span>}
                  </div>
                  <div className={`form-group ${stepErrors.idLocalizacionRecogida ? 'form-group--error' : ''}`}>
                    <label className="form-label"><MapPin size={16} /> Sucursal de Recogida *</label>
                    <select className="form-input" value={form.idLocalizacionRecogida}
                      onChange={(e) => { setForm({ ...form, idLocalizacionRecogida: e.target.value }); setStepErrors(er => ({...er, idLocalizacionRecogida: ''})); }}>
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
              </div>
            )}

            {/* Step 1: Conductores */}
            {step === 1 && (
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
                              setForm(prev => ({
                                ...prev,
                                conductores: prev.conductores.map((cc, i) => ({ ...cc, esPrincipal: i === idx }))
                              }));
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
                      <div className="form-group" style={{ flex: 1, minWidth: '200px' }}>
                        <label className="form-label">Nombre *</label>
                        <input className="form-input" placeholder="Nombre completo"
                          value={newConductor.nombre}
                          onChange={(e) => setNewConductor({ ...newConductor, nombre: e.target.value })} />
                      </div>
                      <div className="form-group" style={{ flex: 1, minWidth: '200px' }}>
                        <label className="form-label">Apellido *</label>
                        <input className="form-input" placeholder="Apellido"
                          value={newConductor.apellido}
                          onChange={(e) => setNewConductor({ ...newConductor, apellido: e.target.value })} />
                      </div>
                    </div>
                    <div className="pago-form__row" style={{ gap: '1rem', flexWrap: 'wrap' }}>
                      <div className="form-group" style={{ flex: 1, minWidth: '150px' }}>
                        <label className="form-label">No. Licencia</label>
                        <input className="form-input" placeholder="Licencia de conducir"
                          value={newConductor.licencia}
                          onChange={(e) => setNewConductor({ ...newConductor, licencia: e.target.value })} />
                      </div>
                      <div className="form-group" style={{ flex: 1, minWidth: '100px' }}>
                        <label className="form-label">Edad</label>
                        <input className="form-input" type="number" placeholder="25"
                          value={newConductor.edad}
                          onChange={(e) => setNewConductor({ ...newConductor, edad: e.target.value })} />
                      </div>
                      <div className="form-group" style={{ flex: 1, minWidth: '150px' }}>
                        <label className="form-label">Teléfono</label>
                        <input className="form-input" placeholder="+593..."
                          value={newConductor.telefono}
                          onChange={(e) => setNewConductor({ ...newConductor, telefono: e.target.value })} />
                      </div>
                    </div>
                    <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
                      <button className="btn btn--primary btn--sm"
                        disabled={!newConductor.nombre.trim() || !newConductor.apellido.trim()}
                        onClick={() => {
                          setForm(prev => ({
                            ...prev,
                            conductores: [...prev.conductores, {
                              id: null,
                              nombre: `${newConductor.nombre} ${newConductor.apellido}`,
                              licencia: newConductor.licencia,
                              edad: newConductor.edad,
                              telefono: newConductor.telefono,
                              esPrincipal: false,
                              esCliente: false,
                            }]
                          }));
                          setNewConductor({ nombre: '', apellido: '', licencia: '', edad: '', telefono: '' });
                          setShowAddConductor(false);
                          toast.success('Conductor adicional agregado');
                        }}><Check size={16} /> Agregar</button>
                      <button className="btn btn--ghost btn--sm"
                        onClick={() => setShowAddConductor(false)}><X size={16} /> Cancelar</button>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Step 2: Extras */}
            {step === 2 && (
              <div className="reservar-step-content">
                <h2><Package size={24} /> Extras y Accesorios</h2>
                <p className="reservar-step-desc">Personaliza tu experiencia con extras opcionales</p>
                {extras.length === 0 ? (
                  <p className="text-muted">No hay extras disponibles.</p>
                ) : (
                  <div className="extras-grid">
                    {extras.map((extra) => {
                      const selected = form.extrasSeleccionados.find(e => e.id === (extra.idExtra || extra.id));
                      return (
                        <div key={extra.idExtra || extra.id}
                          className={`extra-card ${selected ? 'extra-card--selected' : ''}`}
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
                          {selected && (
                            <div className="extra-card__qty" onClick={(e) => e.stopPropagation()}>
                              <button className="extra-card__qty-btn" onClick={() => updateExtraCantidad(selected.id, -1)}>
                                <Minus size={14} />
                              </button>
                              <span>{selected.cantidad}</span>
                              <button className="extra-card__qty-btn" onClick={() => updateExtraCantidad(selected.id, 1)}>
                                <Plus size={14} />
                              </button>
                            </div>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            )}

            {/* Step 3: Summary */}
            {step === 3 && (
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
                  <div className="resumen-section">
                    <h4>Cliente</h4>
                    <p>{user?.nombreCompleto || user?.username}</p>
                    <p>{user?.correo}</p>
                  </div>
                </div>
              </div>
            )}

            {/* Step 4: Payment */}
            {step === 4 && (
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
