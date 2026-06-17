import { ReactNode } from 'react';
import { StyleSheet, View } from 'react-native';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { PublicNavbar } from '@/src/components/layout/PublicNavbar';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';

type Props = {
  children: ReactNode;
  /** Muestra navbar pública (rutas sin auth admin) */
  showNavbar?: boolean;
  /** Ancho máximo del contenido en desktop */
  maxWidth?: number;
  /** Padding horizontal del contenido (false para home/catálogo full-bleed) */
  padded?: boolean;
};

/**
 * Contenedor responsivo para web.
 * En móvil nativo renderiza solo children (sin navbar — usa tabs del sistema).
 */
export function WebShell({ children, showNavbar = true, maxWidth = 1280, padded = true }: Props) {
  const { isWeb, showWebNavbar } = useBreakpoint();

  if (!isWeb) {
    return <>{children}</>;
  }

  return (
    <View style={styles.root}>
      {showNavbar && showWebNavbar && <PublicNavbar />}
      <View style={styles.main}>
        <View style={StyleSheet.flatten([styles.content, { maxWidth }, !padded ? styles.contentFlush : null])}>{children}</View>
      </View>
    </View>
  );
}

/** Placeholder sidebar — implementación completa en Fase 3 */
export function WebSidebarPlaceholder() {
  const { showWebSidebar } = useBreakpoint();

  if (!showWebSidebar) return null;

  return <View style={styles.sidebar} />;
}

const styles = StyleSheet.create({
  root: {
    flex: 1,
    backgroundColor: colors.bg,
    minHeight: '100%' as unknown as number,
  },
  main: {
    flex: 1,
    alignItems: 'center',
  },
  content: {
    flex: 1,
    width: '100%',
    paddingHorizontal: spacing.xl,
  },
  contentFlush: {
    paddingHorizontal: 0,
  },
  sidebar: {
    width: 260,
    backgroundColor: colors.surface,
    borderRightWidth: 1,
    borderRightColor: colors.border,
    paddingVertical: spacing.lg,
  },
});
