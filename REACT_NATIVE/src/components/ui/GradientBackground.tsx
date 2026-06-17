import { ReactNode } from 'react';
import { StyleSheet, View, ViewStyle } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { colors } from '@/src/theme/colors';

type Variant = 'hero' | 'auth';

type Props = {
  children: ReactNode;
  variant?: Variant;
  style?: ViewStyle;
};

/** Fondos con gradientes radiales simulados (como index.css del web). */
export function GradientBackground({ children, variant = 'hero', style }: Props) {
  const auth = variant === 'auth';

  return (
    <View style={[styles.root, style]}>
      <LinearGradient
        colors={auth
          ? ['#0a0e17', '#111827', '#0a0e17']
          : ['#0a0e17', '#0f1419', '#0a0e17']}
        style={styles.fill}
      />
      <View style={[styles.glowTeal, auth ? styles.glowAuthTeal : null]} />
      <View style={[styles.glowBlue, auth ? styles.glowAuthBlue : null]} />
      {children}
    </View>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: colors.bg },
  fill: { position: 'absolute', top: 0, left: 0, right: 0, bottom: 0 },
  glowTeal: {
    position: 'absolute',
    width: '70%',
    height: '50%',
    top: '10%',
    left: '5%',
    borderRadius: 999,
    backgroundColor: 'rgba(13,148,136,0.18)',
    transform: [{ scaleX: 1.4 }],
  },
  glowAuthTeal: { top: '5%', opacity: 0.9 },
  glowBlue: {
    position: 'absolute',
    width: '60%',
    height: '45%',
    bottom: '15%',
    right: '0%',
    borderRadius: 999,
    backgroundColor: 'rgba(59,130,246,0.1)',
  },
  glowAuthBlue: { opacity: 0.85 },
});
