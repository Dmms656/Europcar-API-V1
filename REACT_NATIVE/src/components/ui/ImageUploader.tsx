import { useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Image,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';
import { cloudinaryConfig, uploadToCloudinary } from '@/src/utils/cloudinary';

type Props = {
  value?: string;
  onChange: (url: string) => void;
  label?: string;
};

export function ImageUploader({ value, onChange, label = 'Imagen del vehículo' }: Props) {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');
  const [preview, setPreview] = useState(value || '');

  useEffect(() => {
    setPreview(value || '');
  }, [value]);

  const pickAndUpload = async () => {
    setError('');
    if (!cloudinaryConfig.isConfigured) {
      setError('Configura EXPO_PUBLIC_CLOUDINARY_* en Render');
      return;
    }

    try {
      let file: Blob | File | null = null;

      if (Platform.OS === 'web') {
        file = await pickWebFile();
      } else {
        const perm = await ImagePicker.requestMediaLibraryPermissionsAsync();
        if (!perm.granted) {
          setError('Permiso de galería denegado');
          return;
        }
        const result = await ImagePicker.launchImageLibraryAsync({
          mediaTypes: ['images'],
          quality: 0.85,
        });
        if (result.canceled || !result.assets[0]) return;
        const asset = result.assets[0];
        setPreview(asset.uri);
        const res = await fetch(asset.uri);
        file = await res.blob();
      }

      if (!file) return;
      setUploading(true);
      const { url } = await uploadToCloudinary(file);
      setPreview(url);
      onChange(url);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al subir');
    } finally {
      setUploading(false);
    }
  };

  const clear = () => {
    setPreview('');
    onChange('');
    setError('');
  };

  return (
    <View style={styles.wrap}>
      <Text style={styles.label}>{label}</Text>
      <Pressable
        style={[styles.zone, uploading && styles.zoneBusy]}
        onPress={pickAndUpload}
        disabled={uploading}
      >
        {preview ? (
          <Image source={{ uri: preview }} style={styles.preview} resizeMode="contain" />
        ) : (
          <View style={styles.placeholder}>
            <Ionicons name="cloud-upload-outline" size={32} color={colors.textMuted} />
            <Text style={styles.hint}>Toca para subir imagen</Text>
            <Text style={styles.sub}>JPG, PNG, WebP · máx 10MB</Text>
          </View>
        )}
        {uploading ? (
          <View style={styles.overlay}>
            <ActivityIndicator color={colors.primaryLight} size="large" />
          </View>
        ) : null}
      </Pressable>
      {preview ? (
        <Pressable onPress={clear} style={styles.clearBtn}>
          <Text style={styles.clearText}>Quitar imagen</Text>
        </Pressable>
      ) : null}
      {error ? <Text style={styles.error}>{error}</Text> : null}
    </View>
  );
}

function pickWebFile(): Promise<File | null> {
  return new Promise((resolve) => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = () => resolve(input.files?.[0] ?? null);
    input.click();
  });
}

const styles = StyleSheet.create({
  wrap: { marginBottom: spacing.md },
  label: { color: colors.textSecondary, fontSize: 13, marginBottom: 6, fontFamily: fonts.semiBold },
  zone: {
    borderWidth: 2,
    borderStyle: 'dashed',
    borderColor: colors.borderLight,
    borderRadius: radius.lg,
    minHeight: Platform.OS === 'web' ? 220 : 160,
    overflow: 'hidden',
    backgroundColor: colors.bgSecondary,
  },
  zoneBusy: { opacity: 0.85 },
  placeholder: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: spacing.xl, minHeight: 160 },
  hint: { color: colors.text, marginTop: spacing.sm, fontFamily: fonts.semiBold },
  sub: { color: colors.textMuted, fontSize: 12, marginTop: 4 },
  preview: { width: '100%', height: Platform.OS === 'web' ? 220 : 180, backgroundColor: colors.bgSecondary },
  overlay: {
    ...(StyleSheet.absoluteFill as object),
    backgroundColor: 'rgba(0,0,0,0.45)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  clearBtn: { marginTop: spacing.sm, alignSelf: 'flex-start' },
  clearText: { color: colors.danger, fontFamily: fonts.semiBold, fontSize: 13 },
  error: { color: colors.danger, fontSize: 12, marginTop: 4 },
});
