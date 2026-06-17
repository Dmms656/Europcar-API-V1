import * as Device from 'expo-device';
import { Platform } from 'react-native';

const PUSH_TOKEN_KEY = 'rc_push_token';

/** Solo nativo — evita errores al importar expo-notifications en web. */
export async function registerForPushNotifications(): Promise<string | null> {
  if (Platform.OS === 'web' || !Device.isDevice) return null;

  const Notifications = await import('expo-notifications');
  const SecureStore = await import('expo-secure-store');

  Notifications.setNotificationHandler({
    handleNotification: async () => ({
      shouldShowAlert: true,
      shouldPlaySound: true,
      shouldSetBadge: false,
      shouldShowBanner: true,
      shouldShowList: true,
    }),
  });

  const { status: existing } = await Notifications.getPermissionsAsync();
  let finalStatus = existing;
  if (existing !== 'granted') {
    const { status } = await Notifications.requestPermissionsAsync();
    finalStatus = status;
  }
  if (finalStatus !== 'granted') return null;

  if (Platform.OS === 'android') {
    await Notifications.setNotificationChannelAsync('default', {
      name: 'Europcar',
      importance: Notifications.AndroidImportance.DEFAULT,
    });
  }

  const tokenData = await Notifications.getExpoPushTokenAsync();
  const token = tokenData.data;
  await SecureStore.setItemAsync(PUSH_TOKEN_KEY, token);
  return token;
}

export async function getStoredPushToken(): Promise<string | null> {
  if (Platform.OS === 'web') return null;
  const SecureStore = await import('expo-secure-store');
  return SecureStore.getItemAsync(PUSH_TOKEN_KEY);
}
