import { Tabs } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { Platform } from 'react-native';
import { ClienteWebLayout } from '@/src/components/layout/ClienteWebLayout';
import { WebShell } from '@/src/components/layout/WebShell';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';

export default function TabLayout() {
  const { isWeb, showWebSidebar } = useBreakpoint();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const userType = useAuthStore((s) => s.userType);
  const hideTabBar =
    isWeb || (showWebSidebar && isAuthenticated && userType === 'cliente');

  const tabs = (
    <Tabs
      screenOptions={{
        headerShown: !isWeb,
        headerStyle: { backgroundColor: colors.surface },
        headerTintColor: colors.text,
        headerTitleStyle: { fontWeight: '700' },
        tabBarStyle: hideTabBar
          ? { display: 'none' }
          : {
              backgroundColor: colors.surface,
              borderTopColor: colors.border,
              height: 60,
              paddingBottom: 6,
            },
        tabBarActiveTintColor: colors.accent,
        tabBarInactiveTintColor: colors.textMuted,
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: 'Inicio',
          href: '/',
          tabBarIcon: ({ color, size }) => <Ionicons name="home-outline" size={size} color={color} />,
        }}
      />
      <Tabs.Screen
        name="catalogo"
        options={{
          title: 'Catálogo',
          tabBarIcon: ({ color, size }) => <Ionicons name="car-sport-outline" size={size} color={color} />,
        }}
      />
      <Tabs.Screen name="buscar" options={{ href: null }} />
      <Tabs.Screen
        name="reservas"
        options={{
          title: 'Reservas',
          tabBarIcon: ({ color, size }) => <Ionicons name="calendar-outline" size={size} color={color} />,
        }}
      />
      <Tabs.Screen
        name="historial"
        options={{
          title: 'Historial',
          tabBarIcon: ({ color, size }) => <Ionicons name="time-outline" size={size} color={color} />,
        }}
      />
      <Tabs.Screen
        name="cuenta"
        options={{
          title: 'Mi cuenta',
          tabBarIcon: ({ color, size }) => <Ionicons name="person-circle-outline" size={size} color={color} />,
        }}
      />
      <Tabs.Screen name="contratos" options={{ href: null, title: 'Mis Contratos' }} />
      <Tabs.Screen name="facturas" options={{ href: null, title: 'Mis Facturas' }} />
    </Tabs>
  );

  if (Platform.OS === 'web') {
    return (
      <WebShell padded={false} maxWidth={9999}>
        <ClienteWebLayout>{tabs}</ClienteWebLayout>
      </WebShell>
    );
  }

  return tabs;
}
