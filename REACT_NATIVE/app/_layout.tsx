import { Platform } from 'react-native';
import { useEffect, useState } from 'react';
import { ActivityIndicator, View } from 'react-native';
import { Stack } from 'expo-router';
import * as SplashScreen from 'expo-splash-screen';
import {
  Inter_400Regular,
  Inter_500Medium,
  Inter_600SemiBold,
  Inter_700Bold,
  Inter_800ExtraBold,
  useFonts,
} from '@expo-google-fonts/inter';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { AppDialogHost } from '@/src/components/ui/AppDialogHost';
import { AuthRedirect } from '@/src/components/AuthRedirect';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { fonts } from '@/src/theme/typography';

if (Platform.OS !== 'web') {
  SplashScreen.preventAutoHideAsync();
}

export default function RootLayout() {
  const restoreSession = useAuthStore((s) => s.restoreSession);
  const sessionChecked = useAuthStore((s) => s.sessionChecked);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [ready, setReady] = useState(false);
  const [fontsLoaded, fontError] = useFonts({
    Inter_400Regular,
    Inter_500Medium,
    Inter_600SemiBold,
    Inter_700Bold,
    Inter_800ExtraBold,
  });
  const fontsReady = Platform.OS === 'web' ? true : fontsLoaded || Boolean(fontError);

  useEffect(() => {
    (async () => {
      try {
        if (!__DEV__ && Platform.OS !== 'web') {
          const Updates = await import('expo-updates');
          if (Updates.isEnabled) {
            const update = await Updates.checkForUpdateAsync();
            if (update.isAvailable) {
              await Updates.fetchUpdateAsync();
              await Updates.reloadAsync();
              return;
            }
          }
        }
      } catch {
        /* OTA opcional */
      }
      await restoreSession();
      setReady(true);
      if (Platform.OS !== 'web') {
        await SplashScreen.hideAsync();
      }
    })();
  }, [restoreSession]);

  const isBooting =
    Platform.OS !== 'web' && (!fontsReady || !ready || !sessionChecked);

  useEffect(() => {
    if (!isAuthenticated || Platform.OS === 'web') return;
    import('@/src/notifications/push')
      .then((m) => m.registerForPushNotifications())
      .catch(() => undefined);
  }, [isAuthenticated]);

  if (isBooting) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg }}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  return (
    <SafeAreaProvider>
      <AppDialogHost />
      <AuthRedirect />
      <Stack
        screenOptions={{
          headerStyle: { backgroundColor: colors.surface },
          headerTintColor: colors.text,
          headerTitleStyle: { fontWeight: '700', fontFamily: fonts.bold },
          contentStyle: { backgroundColor: colors.bg },
        }}
      >
        <Stack.Screen name="index" options={{ headerShown: false }} />
        <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
        <Stack.Screen name="(admin)" options={{ headerShown: false }} />
        <Stack.Screen name="cuenta" options={{ headerShown: false }} />
        <Stack.Screen name="(auth)/login" options={{ headerShown: Platform.OS !== 'web', title: 'Iniciar sesión', presentation: 'modal' }} />
        <Stack.Screen name="(auth)/register" options={{ headerShown: Platform.OS !== 'web', title: 'Crear cuenta', presentation: 'modal' }} />
        <Stack.Screen name="reservar/[id]" options={{ headerShown: Platform.OS !== 'web', title: 'Reservar' }} />
        <Stack.Screen name="reserva/[codigo]" options={{ title: 'Detalle reserva' }} />
      </Stack>
    </SafeAreaProvider>
  );
}
