import { ReactNode } from 'react';
import {
  Modal as RNModal,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
  useWindowDimensions,
} from 'react-native';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';

type Size = 'md' | 'lg' | 'xl';

const MAX_WIDTH: Record<Size, number> = {
  md: 560,
  lg: 760,
  xl: 960,
};

type Props = {
  visible: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  size?: Size;
};

export function Modal({ visible, title, onClose, children, size = 'md' }: Props) {
  const { height } = useWindowDimensions();
  const maxSheetHeight = height * 0.92;

  return (
    <RNModal visible={visible} transparent animationType="fade" onRequestClose={onClose}>
      <View style={styles.overlay}>
        <Pressable style={StyleSheet.absoluteFill} onPress={onClose} accessibilityLabel="Cerrar" />
        <View
          style={[
            styles.sheet,
            { maxWidth: MAX_WIDTH[size], maxHeight: maxSheetHeight },
          ]}
        >
          <View style={styles.header}>
            <Text style={styles.title}>{title}</Text>
            <Pressable onPress={onClose} hitSlop={12} accessibilityLabel="Cerrar modal">
              <Text style={styles.close}>✕</Text>
            </Pressable>
          </View>
          <ScrollView
            style={styles.scroll}
            contentContainerStyle={styles.body}
            keyboardShouldPersistTaps="handled"
            showsVerticalScrollIndicator
            nestedScrollEnabled
          >
            {children}
          </ScrollView>
        </View>
      </View>
    </RNModal>
  );
}

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.65)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: spacing.lg,
  },
  sheet: {
    width: '100%',
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    overflow: 'hidden',
    zIndex: 1,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.md,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  title: { color: colors.text, fontSize: 18, fontFamily: fonts.bold, flex: 1 },
  close: { color: colors.textMuted, fontSize: 22, paddingLeft: spacing.md },
  scroll: { flexGrow: 0 },
  body: {
    padding: spacing.lg,
    paddingBottom: spacing.xxl,
  },
});
