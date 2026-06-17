import { useCallback, useEffect, useState } from 'react';
import { Modal, Pressable, StyleSheet, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Button } from '@/src/components/ui/Button';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { type DialogItem, subscribeDialog } from '@/src/utils/dialogService';

export function AppDialogHost() {
  const [queue, setQueue] = useState<DialogItem[]>([]);
  const current = queue[0] ?? null;

  useEffect(() => {
    return subscribeDialog((item) => {
      setQueue((prev) => [...prev, item]);
    });
  }, []);

  const dismissCurrent = useCallback((result?: boolean) => {
    setQueue((prev) => {
      const [head, ...rest] = prev;
      if (!head) return prev;
      if (head.kind === 'alert') head.resolve();
      else head.resolve(Boolean(result));
      return rest;
    });
  }, []);

  const handleBackdropPress = useCallback(() => {
    if (!current || current.kind !== 'confirm') return;
    dismissCurrent(false);
  }, [current, dismissCurrent]);

  if (!current) return null;

  const isConfirm = current.kind === 'confirm';
  const isError = /error/i.test(current.title);
  const iconName = isConfirm ? 'help-circle-outline' : isError ? 'alert-circle-outline' : 'information-circle-outline';
  const iconColor = isError ? colors.danger : isConfirm ? colors.accent : colors.primaryLight;

  return (
    <Modal
      visible
      transparent
      animationType="fade"
      onRequestClose={() => (isConfirm ? dismissCurrent(false) : undefined)}
    >
      <View style={styles.overlay}>
        <Pressable style={StyleSheet.absoluteFill} onPress={handleBackdropPress} />
        <View style={styles.card} accessibilityRole="alert">
          <View style={[styles.iconWrap, { backgroundColor: isError ? 'rgba(239,68,68,0.12)' : colors.clientGhost }]}>
            <Ionicons name={iconName} size={28} color={iconColor} />
          </View>
          <Text style={styles.title}>{current.title}</Text>
          {current.message ? <Text style={styles.message}>{current.message}</Text> : null}
          <View style={styles.actions}>
            {isConfirm ? (
              <>
                <Button
                  label={current.cancelLabel}
                  variant="secondary"
                  onPress={() => dismissCurrent(false)}
                  style={styles.btn}
                />
                <Button
                  label={current.confirmLabel}
                  variant={current.destructive ? 'danger' : 'client'}
                  onPress={() => dismissCurrent(true)}
                  style={styles.btn}
                />
              </>
            ) : (
              <Button label="Aceptar" variant="client" onPress={() => dismissCurrent()} style={styles.btnSingle} />
            )}
          </View>
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.65)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: spacing.xl,
  },
  card: {
    width: '100%',
    maxWidth: 400,
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.xl,
    zIndex: 1,
    alignItems: 'center',
  },
  iconWrap: {
    width: 52,
    height: 52,
    borderRadius: radius.full,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
  },
  title: {
    color: colors.text,
    fontSize: 18,
    fontFamily: fonts.bold,
    textAlign: 'center',
    marginBottom: spacing.sm,
  },
  message: {
    color: colors.textSecondary,
    fontSize: 15,
    lineHeight: 22,
    fontFamily: fonts.regular,
    textAlign: 'center',
    marginBottom: spacing.lg,
  },
  actions: {
    flexDirection: 'row',
    gap: spacing.sm,
    width: '100%',
  },
  btn: { flex: 1 },
  btnSingle: { width: '100%' },
});
