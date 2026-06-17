import { Alert, Platform } from 'react-native';
import { enqueueDialog, isDialogHostReady } from '@/src/utils/dialogService';

export type ConfirmOptions = {
  confirmLabel?: string;
  cancelLabel?: string;
  destructive?: boolean;
};

export function alertMessage(title: string, message?: string): Promise<void> {
  if (isDialogHostReady()) {
    return new Promise((resolve) => {
      enqueueDialog({ kind: 'alert', title, message, resolve });
    });
  }
  return fallbackAlert(title, message);
}

export function confirmAction(
  title: string,
  message: string,
  options: ConfirmOptions = {},
): Promise<boolean> {
  const confirmLabel = options.confirmLabel ?? 'Confirmar';
  const cancelLabel = options.cancelLabel ?? 'Cancelar';
  const destructive = options.destructive ?? false;

  if (isDialogHostReady()) {
    return new Promise((resolve) => {
      enqueueDialog({
        kind: 'confirm',
        title,
        message,
        confirmLabel,
        cancelLabel,
        destructive,
        resolve,
      });
    });
  }
  return fallbackConfirm(title, message);
}

function fallbackAlert(title: string, message?: string): Promise<void> {
  const text = message ? `${title}\n\n${message}` : title;
  if (Platform.OS === 'web') {
    if (typeof window !== 'undefined') window.alert(text);
    return Promise.resolve();
  }
  return new Promise((resolve) => {
    Alert.alert(title, message ?? '', [{ text: 'OK', onPress: () => resolve() }]);
  });
}

function fallbackConfirm(title: string, message: string): Promise<boolean> {
  if (Platform.OS === 'web') {
    const ok = typeof window !== 'undefined' && window.confirm(`${title}\n\n${message}`);
    return Promise.resolve(ok);
  }
  return new Promise((resolve) => {
    Alert.alert(title, message, [
      { text: 'Cancelar', style: 'cancel', onPress: () => resolve(false) },
      { text: 'Confirmar', style: 'destructive', onPress: () => resolve(true) },
    ]);
  });
}
