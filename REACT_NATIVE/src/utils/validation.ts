export const validators = {
  required: (value: string | undefined | null, label: string) => {
    if (!value?.trim()) return `${label} es requerido`;
    return null;
  },
  email: (value: string) => {
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) return 'Correo electrónico no válido';
    return null;
  },
  phone: (value: string) => {
    const digits = value.replace(/\D/g, '');
    if (digits.length < 9) return 'Teléfono no válido';
    return null;
  },
  minLength: (value: string, min: number, label: string) => {
    if (value.length < min) return `${label} debe tener al menos ${min} caracteres`;
    return null;
  },
  match: (a: string, b: string, label: string) => {
    if (a !== b) return `${label} no coinciden`;
    return null;
  },
};
