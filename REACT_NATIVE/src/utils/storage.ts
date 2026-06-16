import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';

/**
 * Almacenamiento cross-platform para auth y preferencias.
 * - Nativo: expo-secure-store (cifrado en dispositivo)
 * - Web: localStorage (equivalente al sessionStorage del frontend web)
 */

const isWeb = Platform.OS === 'web';

async function webGet(key: string): Promise<string | null> {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

async function webSet(key: string, value: string): Promise<void> {
  try {
    localStorage.setItem(key, value);
  } catch {
    /* quota / private mode */
  }
}

async function webRemove(key: string): Promise<void> {
  try {
    localStorage.removeItem(key);
  } catch {
    /* ignore */
  }
}

export async function getItem(key: string): Promise<string | null> {
  if (isWeb) return webGet(key);
  return SecureStore.getItemAsync(key);
}

export async function setItem(key: string, value: string): Promise<void> {
  if (isWeb) return webSet(key, value);
  return SecureStore.setItemAsync(key, value);
}

export async function removeItem(key: string): Promise<void> {
  if (isWeb) return webRemove(key);
  return SecureStore.deleteItemAsync(key);
}

export async function multiGet(keys: string[]): Promise<(string | null)[]> {
  return Promise.all(keys.map((k) => getItem(k)));
}

export async function clearAuthKeys(keys: string[]): Promise<void> {
  await Promise.all(keys.map((k) => removeItem(k)));
}
