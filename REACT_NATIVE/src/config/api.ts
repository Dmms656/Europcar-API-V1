import Constants from 'expo-constants';
import { Platform } from 'react-native';

/** Misma API que el frontend web en Render (middleware /api/v1). */
export const PRODUCTION_API_URL = 'https://europcar-api-v2.onrender.com/api/v1';

const DEV_WEB_DEFAULT = 'http://localhost:5200/api/v1';

export function resolveApiBaseUrl(): string {
  const extra = Constants.expoConfig?.extra as { apiUrl?: string } | undefined;
  const fromExtra = extra?.apiUrl?.trim();
  if (fromExtra) return fromExtra.replace(/\/$/, '');

  const fromEnv = process.env.EXPO_PUBLIC_API_URL?.trim();
  if (fromEnv) return fromEnv.replace(/\/$/, '');

  // Web en desarrollo: apuntar al middleware local (requiere CORS localhost:8081)
  if (__DEV__ && Platform.OS === 'web') {
    return DEV_WEB_DEFAULT;
  }

  return PRODUCTION_API_URL;
}

export function isApiConfigured(): boolean {
  return Boolean(resolveApiBaseUrl());
}
