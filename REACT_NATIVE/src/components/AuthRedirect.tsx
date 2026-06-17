import { useEffect } from 'react';
import { router, useSegments } from 'expo-router';
import { useAuthStore } from '@/src/store/useAuthStore';
import { useSidebarStore } from '@/src/store/useSidebarStore';
import { CLIENT_PORTAL_SEGMENTS } from '@/src/utils/clientPortal';
import { homeHref } from '@/src/utils/authActions';

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
    const home = homeHref();
    const tabSegment = segments[0] === '(tabs)' ? segments[1] : null;
    const inClientPortal =
      Boolean(tabSegment) && CLIENT_PORTAL_SEGMENTS.includes(tabSegment as (typeof CLIENT_PORTAL_SEGMENTS)[number]);

    if (!isAuthenticated) {
      useSidebarStore.getState().setSidebarOpen(false);
      if (inAdmin || inClientPortal) {
        router.replace(home);
      }
      return;
    }

    if (userType === 'admin' && !inAdmin && !inAuth) {
      router.replace('/(admin)');
      return;
    }

    if (userType === 'cliente' && inAdmin) {
      router.replace('/mi-cuenta');
    }
  }, [sessionChecked, isAuthenticated, userType, segments]);

  return null;
}
