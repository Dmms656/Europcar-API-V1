import Constants from 'expo-constants';

/** Misma API que el frontend web en Render (middleware /api/v1). */
export const PRODUCTION_API_URL = 'https://europcar-api-v2.onrender.com/api/v1';

export function resolveApiBaseUrl(): string {
  const extra = Constants.expoConfig?.extra as { apiUrl?: string } | undefined;
  const fromExtra = extra?.apiUrl?.trim();
  if (fromExtra) return fromExtra.replace(/\/$/, '');

  const fromEnv = process.env.EXPO_PUBLIC_API_URL?.trim();
  if (fromEnv) return fromEnv.replace(/\/$/, '');

  return PRODUCTION_API_URL;
}
