import { useState, useEffect, useRef } from 'react';
import { Calendar, Clock, ChevronDown } from 'lucide-react';

const HOURS = Array.from({ length: 15 }, (_, i) => i + 7);
const MINUTES = ['00', '30'];

export default function DateTimePicker({ label, value, onChange, minDate, id }) {
  const [open, setOpen] = useState(false);
  const [date, setDate] = useState('');
  const [hour, setHour] = useState('10');
  const [minute, setMinute] = useState('00');
  const ref = useRef(null);

  useEffect(() => {
    if (value) {
      const d = new Date(value);
      if (!isNaN(d.getTime())) {
        setDate(d.toISOString().slice(0, 10));
        setHour(String(d.getHours()).padStart(2, '0'));
        setMinute(d.getMinutes() >= 30 ? '30' : '00');
      }
    }
  }, []);

  useEffect(() => {
    if (date && hour) {
      const combined = `${date}T${hour.padStart(2, '0')}:${minute}`;
      if (combined !== value) onChange(combined);
    }
  }, [date, hour, minute]);

  // Close on outside click
  useEffect(() => {
    const handler = (e) => {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const today = minDate
    ? (typeof minDate === 'string' ? minDate.slice(0, 10) : new Date(minDate).toISOString().slice(0, 10))
    : new Date().toISOString().slice(0, 10);

  const displayText = date
    ? `${new Date(date + 'T12:00').toLocaleDateString('es-EC', { day: '2-digit', month: 'short' })}  ${hour.padStart(2, '0')}:${minute}`
    : 'Seleccionar...';

  return (
    <div className="dtp" id={id} ref={ref}>
      {label && <label className="form-label">{label}</label>}
      <button type="button" className={`dtp__trigger ${open ? 'dtp__trigger--open' : ''}`} onClick={() => setOpen(!open)}>
        <Calendar size={15} className="dtp__trigger-icon" />
        <span className={`dtp__trigger-text ${!date ? 'dtp__trigger-text--empty' : ''}`}>{displayText}</span>
        <ChevronDown size={14} className={`dtp__chevron ${open ? 'dtp__chevron--open' : ''}`} />
      </button>

      {open && (
        <div className="dtp__dropdown">
          <div className="dtp__section">
            <span className="dtp__section-label"><Calendar size={13} /> Fecha</span>
            <input
              type="date"
              className="dtp__date-input"
              value={date}
              min={today}
              onChange={(e) => setDate(e.target.value)}
            />
          </div>
          <div className="dtp__divider" />
          <div className="dtp__section">
            <span className="dtp__section-label"><Clock size={13} /> Hora</span>
            <div className="dtp__time-row">
              <select className="dtp__time-select" value={hour} onChange={(e) => setHour(e.target.value)}>
                {HOURS.map((h) => (
                  <option key={h} value={String(h).padStart(2, '0')}>{String(h).padStart(2, '0')}</option>
                ))}
              </select>
              <span className="dtp__time-colon">:</span>
              <select className="dtp__time-select" value={minute} onChange={(e) => setMinute(e.target.value)}>
                {MINUTES.map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>
            </div>
          </div>
          {date && (
            <div className="dtp__preview">
              {new Date(`${date}T${hour.padStart(2, '0')}:${minute}`).toLocaleDateString('es-EC', {
                weekday: 'short', year: 'numeric', month: 'short', day: 'numeric'
              })} · {hour.padStart(2, '0')}:{minute}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
