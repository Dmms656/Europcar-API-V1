import { ReactNode } from 'react';
import { StyleSheet, View } from 'react-native';
import { ClienteSidebar } from '@/src/components/layout/ClienteSidebar';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';

type Props = {
  children: ReactNode;
};

/**
 * Layout portal cliente en web desktop: sidebar + contenido.
 * En móvil/nativo renderiza solo children (navegación por tabs).
 */
export function ClienteWebLayout({ children }: Props) {
  const { showWebSidebar } = useBreakpoint();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const userType = useAuthStore((s) => s.userType);

  const showSidebar = showWebSidebar && isAuthenticated && userType === 'cliente';

  if (!showSidebar) {
    return <>{children}</>;
  }

  return (
    <View style={styles.row}>
      <ClienteSidebar />
      <View style={styles.content}>{children}</View>
    </View>
  );
}

const styles = StyleSheet.create({
  row: { flex: 1, flexDirection: 'row', backgroundColor: colors.bg },
  content: { flex: 1, minWidth: 0 },
});
