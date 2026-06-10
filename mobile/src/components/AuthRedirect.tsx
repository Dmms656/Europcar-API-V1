import { useEffect } from 'react';
import { router, useSegments } from 'expo-router';
import { useAuthStore } from '@/src/store/useAuthStore';

/** Redirige admin/cliente al stack correcto tras login o restore. */
export function AuthRedirect() {
  const segments = useSegments();
  const sessionChecked = useAuthStore((s) => s.sessionChecked);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const userType = useAuthStore((s) => s.userType);

  useEffect(() => {
    if (!sessionChecked) return;

    const inAdmin = segments[0] === '(admin)';
    const inAuth = segments[0] === '(auth)';

    if (!isAuthenticated) {
      if (inAdmin) router.replace('/(tabs)');
      return;
    }

    if (userType === 'admin' && !inAdmin && !inAuth) {
      router.replace('/(admin)');
      return;
    }

    if (userType === 'cliente' && inAdmin) {
      router.replace('/(tabs)/cuenta');
    }
  }, [sessionChecked, isAuthenticated, userType, segments]);

  return null;
}
