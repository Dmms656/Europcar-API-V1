import { ReactNode } from 'react';
import { StyleSheet, View } from 'react-native';
import { AdminSidebar } from '@/src/components/layout/AdminSidebar';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { colors } from '@/src/theme/colors';

type Props = { children: ReactNode };

/** Sidebar admin en web desktop; en móvil solo children (tabs). */
export function AdminWebLayout({ children }: Props) {
  const { showWebSidebar } = useBreakpoint();

  if (!showWebSidebar) return <>{children}</>;

  return (
    <View style={styles.row}>
      <AdminSidebar />
      <View style={styles.content}>{children}</View>
    </View>
  );
}

const styles = StyleSheet.create({
  row: { flex: 1, flexDirection: 'row', backgroundColor: colors.bg },
  content: { flex: 1, minWidth: 0 },
});
