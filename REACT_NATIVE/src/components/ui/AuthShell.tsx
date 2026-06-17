import { ReactNode } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  View,
  type ViewStyle,
} from 'react-native';
import { GradientBackground } from '@/src/components/ui/GradientBackground';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { colors } from '@/src/theme/colors';
import { radius, shadows, spacing } from '@/src/theme/layout';
import { flatStyle } from '@/src/utils/flatStyle';

type Props = {
  children: ReactNode;
  /** Ancho máximo de la tarjeta en web (login ~420, registro ~520) */
  maxWidth?: number;
  style?: ViewStyle;
};

/**
 * Contenedor centrado para login/registro en web.
 * En móvil usa ancho completo con padding cómodo.
 */
export function AuthShell({ children, maxWidth = 420, style }: Props) {
  const { isWeb } = useBreakpoint();

  return (
    <GradientBackground variant="auth" style={styles.root}>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <ScrollView
          contentContainerStyle={flatStyle([styles.scroll, isWeb ? styles.scrollWeb : null])}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          <View
            style={flatStyle([
              styles.card,
              isWeb ? { maxWidth, width: '100%' } : null,
              style,
            ])}
          >
            {children}
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </GradientBackground>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1 },
  flex: { flex: 1 },
  scroll: {
    flexGrow: 1,
    padding: spacing.lg,
    paddingBottom: spacing.xxl,
  },
  scrollWeb: {
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: '100%' as unknown as number,
    paddingVertical: spacing.xxl,
  },
  card: {
    width: '100%',
    backgroundColor: 'rgba(17,24,39,0.92)',
    borderRadius: radius.xl,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.xl,
    ...shadows.md,
  },
});
