import { ReactNode } from 'react';
import { ActivityIndicator, StyleSheet, Text, View } from 'react-native';
import { Input } from '@/src/components/ui/Input';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';

type Props = {
  title: string;
  subtitle?: string;
  count?: number;
  error?: string;
  loading?: boolean;
  search?: string;
  onSearchChange?: (v: string) => void;
  searchPlaceholder?: string;
  actions?: ReactNode;
  children: ReactNode;
};

export function AdminScreen({
  title,
  subtitle,
  count,
  error,
  loading,
  search,
  onSearchChange,
  searchPlaceholder = 'Buscar…',
  actions,
  children,
}: Props) {
  return (
    <View style={styles.wrap}>
      <View style={styles.header}>
        <View style={{ flex: 1 }}>
          <Text style={styles.title}>{title}</Text>
          {subtitle || count != null ? (
            <Text style={styles.sub}>
              {subtitle ?? `${count} registros`}
            </Text>
          ) : null}
        </View>
        {actions}
      </View>

      {onSearchChange != null ? (
        <Input
          placeholder={searchPlaceholder}
          value={search ?? ''}
          onChangeText={onSearchChange}
        />
      ) : null}

      {error ? <Text style={styles.error}>{error}</Text> : null}

      {loading ? (
        <ActivityIndicator color={colors.primary} size="large" style={styles.loader} />
      ) : (
        children
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { flex: 1 },
  header: { flexDirection: 'row', alignItems: 'flex-start', marginBottom: spacing.md, gap: spacing.md },
  title: { color: colors.text, fontSize: 22, fontWeight: '800' },
  sub: { color: colors.textMuted, marginTop: 4 },
  error: { color: colors.danger, marginBottom: spacing.sm },
  loader: { marginTop: spacing.xxl },
});
