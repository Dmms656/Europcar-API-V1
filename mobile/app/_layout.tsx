import { useEffect, useState } from 'react';
import { ActivityIndicator, View } from 'react-native';
import { Stack } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import * as Updates from 'expo-updates';
import { useAuthStore } from '@/src/store/useAuthStore';
import { registerForPushNotifications } from '@/src/notifications/push';
import { colors } from '@/src/theme/colors';

SplashScreen.preventAutoHideAsync();

export default function RootLayout() {
  const restoreSession = useAuthStore((s) => s.restoreSession);
  const sessionChecked = useAuthStore((s) => s.sessionChecked);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        if (!__DEV__ && Updates.isEnabled) {
          const update = await Updates.checkForUpdateAsync();
          if (update.isAvailable) {
            await Updates.fetchUpdateAsync();
            await Updates.reloadAsync();
            return;
          }
        }
      } catch {
        /* OTA opcional; no bloquear arranque */
      }
      await restoreSession();
      setReady(true);
      await SplashScreen.hideAsync();
    })();
  }, [restoreSession]);

  useEffect(() => {
    if (isAuthenticated) {
      registerForPushNotifications().catch(() => undefined);
    }
  }, [isAuthenticated]);

  if (!ready || !sessionChecked) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg }}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  return (
    <Stack
      screenOptions={{
        headerStyle: { backgroundColor: colors.surface },
        headerTintColor: colors.text,
        contentStyle: { backgroundColor: colors.bg },
      }}
    >
      <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      <Stack.Screen name="(auth)/login" options={{ title: 'Iniciar sesión', presentation: 'modal' }} />
      <Stack.Screen name="(auth)/register" options={{ title: 'Crear cuenta', presentation: 'modal' }} />
      <Stack.Screen name="reservar/[id]" options={{ title: 'Reservar' }} />
      <Stack.Screen name="reserva/[codigo]" options={{ title: 'Detalle reserva' }} />
    </Stack>
  );
}
