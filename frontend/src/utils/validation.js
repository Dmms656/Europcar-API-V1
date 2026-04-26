/**
 * Reusable form validation utilities
 */

export const validators = {
  required: (val, label = 'Este campo') =>
    (!val || !String(val).trim()) ? `${label} es requerido` : '',

  email: (val) =>
    val && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val) ? 'Correo electrónico no válido' : '',

  minLength: (val, min, label = 'Este campo') =>
    val && val.length < min ? `${label} debe tener al menos ${min} caracteres` : '',

  maxLength: (val, max, label = 'Este campo') =>
    val && val.length > max ? `${label} no puede exceder ${max} caracteres` : '',

  match: (val, other, label = 'Los campos') =>
    val !== other ? `${label} no coinciden` : '',

  phone: (val) =>
    val && !/^[+]?[\d\s()-]{7,20}$/.test(val) ? 'Número de teléfono no válido' : '',

  cedula: (val) =>
    val && !/^[\dA-Z-]{6,20}$/i.test(val) ? 'Cédula o documento no válido' : '',

  username: (val) =>
    val && !/^[a-zA-Z0-9._-]{3,30}$/.test(val) ? 'Solo letras, números, puntos y guiones (3-30 caracteres)' : '',

  dateAfter: (dateA, dateB, label = 'La fecha') =>
    dateA && dateB && new Date(dateA) <= new Date(dateB) ? `${label} debe ser posterior` : '',

  dateFuture: (val, label = 'La fecha') =>
    val && new Date(val) < new Date() ? `${label} debe ser futura` : '',
};

/**
 * Validate a full form object.
 * rules = { fieldName: [(val, form) => errorString | ''] }
 * Returns { fieldName: 'error message' | '' }
 */
export function validateForm(form, rules) {
  const errors = {};
  for (const [field, fieldRules] of Object.entries(rules)) {
    for (const rule of fieldRules) {
      const error = rule(form[field], form);
      if (error) {
        errors[field] = error;
        break;
      }
    }
    if (!errors[field]) errors[field] = '';
  }
  return errors;
}

/**
 * Check if errors object has any real errors
 */
export function hasErrors(errors) {
  return Object.values(errors).some(e => e.length > 0);
}
