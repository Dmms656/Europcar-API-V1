import { StyleSheet, Text, View } from 'react-native';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { flatStyle } from '@/src/utils/flatStyle';

type Props = {
  name: string;
  subtitle: string;
  badge: string;
  variant?: 'admin' | 'cliente';
};

export function ProfileHeader({ name, subtitle, badge, variant = 'cliente' }: Props) {
  const initial = (name || 'U').charAt(0).toUpperCase();
  const isAdmin = variant === 'admin';

  return (
    <View style={flatStyle([styles.card, isAdmin ? styles.adminCard : styles.clientCard])}>
      <View style={flatStyle([styles.avatar, isAdmin ? styles.avatarAdmin : styles.avatarClient])}>
        <Text style={styles.initial}>{initial}</Text>
      </View>
      <Text style={styles.name}>{name}</Text>
      <Text style={styles.subtitle}>{subtitle}</Text>
      <View style={flatStyle([styles.badge, isAdmin ? styles.badgeAdmin : styles.badgeClient])}>
        <Text style={styles.badgeText}>{badge}</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    alignItems: 'center',
    padding: spacing.xl,
    borderRadius: radius.lg,
    borderWidth: 1,
    marginBottom: spacing.lg,
  },
  adminCard: { backgroundColor: colors.surface, borderColor: 'rgba(13,148,136,0.25)' },
  clientCard: { backgroundColor: colors.surface, borderColor: 'rgba(59,130,246,0.25)' },
  avatar: {
    width: 72,
    height: 72,
    borderRadius: radius.full,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
  },
  avatarAdmin: { backgroundColor: colors.primary },
  avatarClient: { backgroundColor: colors.accent },
  initial: { color: colors.white, fontSize: 28, fontWeight: '800' },
  name: { color: colors.text, fontSize: 20, fontWeight: '700' },
  subtitle: { color: colors.textSecondary, marginTop: 4, textAlign: 'center' },
  badge: {
    marginTop: spacing.md,
    paddingHorizontal: 12,
    paddingVertical: 4,
    borderRadius: radius.full,
  },
  badgeAdmin: { backgroundColor: colors.primaryGhost },
  badgeClient: { backgroundColor: colors.clientGhost },
  badgeText: { color: colors.text, fontSize: 12, fontWeight: '700' },
});
