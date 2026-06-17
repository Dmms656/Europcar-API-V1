import { Platform } from 'react-native';
import { router } from 'expo-router';
import { useSidebarStore } from '@/src/store/useSidebarStore';

export function homeHref(): '/(tabs)' | '/' {
  return Platform.OS === 'web' ? '/' : '/(tabs)';
}

export async function logoutAndGoHome(logout: () => Promise<void>): Promise<void> {
  await logout();
  useSidebarStore.getState().setSidebarOpen(false);
  router.replace(homeHref());
}
