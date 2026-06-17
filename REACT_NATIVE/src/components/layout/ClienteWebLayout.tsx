import { ReactNode, useEffect } from 'react';
import { Pressable, StyleSheet, View } from 'react-native';
import { usePathname } from 'expo-router';
import { ClienteSidebar } from '@/src/components/layout/ClienteSidebar';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { useAuthStore } from '@/src/store/useAuthStore';
import { useSidebarStore } from '@/src/store/useSidebarStore';
import { colors } from '@/src/theme/colors';
import { isClientPortalPath } from '@/src/utils/clientPortal';

type Props = {
  children: ReactNode;
};

const SIDEBAR_WIDTH = 260;

/**
 * Layout portal cliente en web: drawer lateral solo en rutas de cuenta/reservas.
 * En catálogo/home no ocupa espacio; se abre con el menú hamburguesa del navbar.
 */
export function ClienteWebLayout({ children }: Props) {
  const pathname = usePathname();
  const { isWeb } = useBreakpoint();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const userType = useAuthStore((s) => s.userType);
  const sidebarOpen = useSidebarStore((s) => s.sidebarOpen);
  const setSidebarOpen = useSidebarStore((s) => s.setSidebarOpen);

  const isPortalRoute = isClientPortalPath(pathname);
  const showDrawer = isWeb && isAuthenticated && userType === 'cliente' && isPortalRoute;

  useEffect(() => {
    if (!isPortalRoute) {
      setSidebarOpen(false);
    }
  }, [isPortalRoute, setSidebarOpen]);

  if (!showDrawer) {
    return <>{children}</>;
  }

  return (
    <View style={styles.root}>
      {sidebarOpen ? (
        <Pressable
          style={styles.overlay}
          onPress={() => setSidebarOpen(false)}
          accessibilityLabel="Cerrar menú"
        />
      ) : null}
      <View
        style={StyleSheet.flatten([
          styles.drawer,
          sidebarOpen ? styles.drawerOpen : styles.drawerClosed,
        ])}
      >
        <ClienteSidebar onClose={() => setSidebarOpen(false)} />
      </View>
      <View style={styles.content}>{children}</View>
    </View>
  );
}

const styles = StyleSheet.create({
  root: {
    flex: 1,
    position: 'relative',
    backgroundColor: colors.bg,
  },
  overlay: {
    position: 'absolute',
    left: 0,
    right: 0,
    top: 0,
    bottom: 0,
    backgroundColor: 'rgba(0,0,0,0.5)',
    zIndex: 90,
  },
  drawer: {
    position: 'absolute',
    left: 0,
    top: 0,
    bottom: 0,
    width: SIDEBAR_WIDTH,
    zIndex: 100,
    backgroundColor: colors.surface,
    borderRightWidth: 1,
    borderRightColor: colors.border,
    // @ts-expect-error RN Web transition
    transition: 'transform 0.2s ease',
  },
  drawerOpen: {
    transform: [{ translateX: 0 }],
  },
  drawerClosed: {
    transform: [{ translateX: -SIDEBAR_WIDTH }],
  },
  content: {
    flex: 1,
    minWidth: 0,
  },
});
