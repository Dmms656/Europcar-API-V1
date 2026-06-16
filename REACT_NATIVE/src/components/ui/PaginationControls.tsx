import { Ionicons } from '@expo/vector-icons';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';

type Props = {
  page: number;
  totalPages: number;
  pageSize: number;
  totalItems: number;
  startItem: number;
  endItem: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (size: number) => void;
};

const PAGE_SIZES = [5, 10, 20];

export function PaginationControls({
  page,
  totalPages,
  pageSize,
  totalItems,
  startItem,
  endItem,
  onPageChange,
  onPageSizeChange,
}: Props) {
  if (totalItems === 0) return null;

  return (
    <View style={styles.wrap}>
      <Text style={styles.info}>
        {startItem}–{endItem} de {totalItems}
      </Text>

      <View style={styles.controls}>
        <Pressable
          style={[styles.btn, page <= 1 && styles.btnDisabled]}
          onPress={() => onPageChange(page - 1)}
          disabled={page <= 1}
        >
          <Ionicons name="chevron-back" size={18} color={page <= 1 ? colors.textMuted : colors.text} />
        </Pressable>

        <Text style={styles.pageText}>
          {page} / {totalPages}
        </Text>

        <Pressable
          style={[styles.btn, page >= totalPages && styles.btnDisabled]}
          onPress={() => onPageChange(page + 1)}
          disabled={page >= totalPages}
        >
          <Ionicons name="chevron-forward" size={18} color={page >= totalPages ? colors.textMuted : colors.text} />
        </Pressable>
      </View>

      {onPageSizeChange ? (
        <View style={styles.sizes}>
          {PAGE_SIZES.map((size) => (
            <Pressable
              key={size}
              style={[styles.sizeBtn, pageSize === size && styles.sizeBtnActive]}
              onPress={() => onPageSizeChange(size)}
            >
              <Text style={[styles.sizeText, pageSize === size && styles.sizeTextActive]}>{size}</Text>
            </Pressable>
          ))}
        </View>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    flexWrap: 'wrap',
    gap: spacing.sm,
    paddingVertical: spacing.lg,
    borderTopWidth: 1,
    borderTopColor: colors.border,
    marginTop: spacing.md,
  },
  info: { color: colors.textMuted, fontSize: 13 },
  controls: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm },
  btn: {
    width: 36,
    height: 36,
    borderRadius: radius.sm,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  btnDisabled: { opacity: 0.4 },
  pageText: { color: colors.text, fontWeight: '600', minWidth: 48, textAlign: 'center' },
  sizes: { flexDirection: 'row', gap: 4 },
  sizeBtn: {
    paddingHorizontal: spacing.sm,
    paddingVertical: 4,
    borderRadius: radius.sm,
    borderWidth: 1,
    borderColor: colors.border,
  },
  sizeBtnActive: { backgroundColor: colors.primaryGhost, borderColor: colors.primary },
  sizeText: { color: colors.textMuted, fontSize: 12 },
  sizeTextActive: { color: colors.primaryLight, fontWeight: '700' },
});
