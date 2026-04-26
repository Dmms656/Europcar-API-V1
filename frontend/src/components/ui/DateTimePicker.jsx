import { useState, useEffect } from 'react';
import { Calendar, Clock } from 'lucide-react';

const HOURS = Array.from({ length: 15 }, (_, i) => i + 7); // 07:00 - 21:00
const MINUTES = ['00', '30'];

export default function DateTimePicker({ label, value, onChange, minDate, id }) {
  const [date, setDate] = useState('');
  const [hour, setHour] = useState('10');
  const [minute, setMinute] = useState('00');

  // Parse incoming value
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

  // Emit combined value
  useEffect(() => {
    if (date && hour) {
      const combined = `${date}T${hour.padStart(2, '0')}:${minute}`;
      if (combined !== value) {
        onChange(combined);
      }
    }
  }, [date, hour, minute]);

  const today = minDate
    ? (typeof minDate === 'string' ? minDate.slice(0, 10) : new Date(minDate).toISOString().slice(0, 10))
    : new Date().toISOString().slice(0, 10);

  return (
    <div className="datetime-picker" id={id}>
      <label className="form-label">{label}</label>
      <div className="datetime-picker__row">
        <div className="datetime-picker__field datetime-picker__field--date">
          <Calendar size={16} className="datetime-picker__icon" />
          <input
            type="date"
            className="datetime-picker__input"
            value={date}
            min={today}
            onChange={(e) => setDate(e.target.value)}
          />
        </div>
        <div className="datetime-picker__field datetime-picker__field--time">
          <Clock size={16} className="datetime-picker__icon" />
          <select
            className="datetime-picker__select"
            value={hour}
            onChange={(e) => setHour(e.target.value)}
          >
            {HOURS.map((h) => (
              <option key={h} value={String(h).padStart(2, '0')}>
                {String(h).padStart(2, '0')}
              </option>
            ))}
          </select>
          <span className="datetime-picker__separator">:</span>
          <select
            className="datetime-picker__select datetime-picker__select--min"
            value={minute}
            onChange={(e) => setMinute(e.target.value)}
          >
            {MINUTES.map((m) => (
              <option key={m} value={m}>{m}</option>
            ))}
          </select>
        </div>
      </div>
      {date && (
        <div className="datetime-picker__preview">
          {new Date(`${date}T${hour.padStart(2, '0')}:${minute}`).toLocaleDateString('es-EC', {
            weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
          })} a las {hour.padStart(2, '0')}:{minute}
        </div>
      )}
    </div>
  );
}
