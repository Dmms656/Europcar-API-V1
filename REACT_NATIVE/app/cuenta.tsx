import { Redirect } from 'expo-router';
import { useAuthStore } from '@/src/store/useAuthStore';

/** Compatibilidad: /cuenta → perfil admin o mi-cuenta cliente */
export default function CuentaRedirect() {
  const userType = useAuthStore((s) => s.userType);
  if (userType === 'admin') return <Redirect href="/(admin)/perfil" />;
  return <Redirect href="/mi-cuenta" />;
}
