import { Tabs } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { AdminWebLayout } from '@/src/components/layout/AdminWebLayout';
import { WebShell } from '@/src/components/layout/WebShell';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { colors } from '@/src/theme/colors';

export default function AdminLayout() {
  const { showWebSidebar } = useBreakpoint();
  const hideTabBar = showWebSidebar;

  const tabs = (
    <Tabs
      screenOptions={{
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
        tabBarActiveTintColor: colors.primaryLight,
        tabBarInactiveTintColor: colors.textMuted,
      }}
    >
      <Tabs.Screen name="index" options={{ title: 'Dashboard', tabBarIcon: ({ color, size }) => <Ionicons name="grid-outline" size={size} color={color} /> }} />
      <Tabs.Screen name="reservas" options={{ title: 'Reservas', tabBarIcon: ({ color, size }) => <Ionicons name="calendar-outline" size={size} color={color} /> }} />
      <Tabs.Screen name="clientes" options={{ title: 'Clientes', tabBarIcon: ({ color, size }) => <Ionicons name="people-outline" size={size} color={color} /> }} />
      <Tabs.Screen name="vehiculos" options={{ title: 'Vehículos', tabBarIcon: ({ color, size }) => <Ionicons name="car-outline" size={size} color={color} /> }} />
      <Tabs.Screen name="mas" options={{ title: 'Más', tabBarIcon: ({ color, size }) => <Ionicons name="ellipsis-horizontal" size={size} color={color} /> }} />
      <Tabs.Screen name="perfil" options={{ href: null }} />
      <Tabs.Screen name="contratos" options={{ href: null, title: 'Contratos' }} />
      <Tabs.Screen name="pagos" options={{ href: null, title: 'Pagos' }} />
      <Tabs.Screen name="mantenimientos" options={{ href: null, title: 'Mantenimientos' }} />
      <Tabs.Screen name="localizaciones" options={{ href: null, title: 'Localizaciones' }} />
      <Tabs.Screen name="ubicaciones" options={{ href: null, title: 'Ubicaciones' }} />
      <Tabs.Screen name="extras" options={{ href: null, title: 'Extras' }} />
      <Tabs.Screen name="usuarios" options={{ href: null, title: 'Usuarios' }} />
    </Tabs>
  );

  return (
    <WebShell showNavbar={false}>
      <AdminWebLayout>{tabs}</AdminWebLayout>
    </WebShell>
  );
}
