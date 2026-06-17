import { Alert, Platform } from 'react-native';

export function alertMessage(title: string, message?: string) {
  const text = message ? `${title}\n\n${message}` : title;
  if (Platform.OS === 'web') {
    if (typeof window !== 'undefined') window.alert(text);
    return;
  }
  Alert.alert(title, message ?? '');
}

export function confirmAction(title: string, message: string): Promise<boolean> {
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
