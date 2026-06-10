import { useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { bookingApi } from '@/src/api/bookingApi';
import { colors } from '@/src/theme/colors';
import { unwrapData } from '@/src/utils/apiResponse';
import { resolveApiBaseUrl } from '@/src/config/api';

type Localizacion = { idLocalizacion: number; nombre: string; codigo?: string };

export default function HomeScreen() {
  const [localizaciones, setLocalizaciones] = useState<Localizacion[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedLoc, setSelectedLoc] = useState<number>(1);

  useEffect(() => {
    (async () => {
      try {
        const res = await bookingApi.getLocalizaciones({ page: 1, limit: 20 });
        const data = unwrapData<{ items?: Localizacion[] }>(res);
        const items = data?.items ?? (Array.isArray(data) ? data : []);
        setLocalizaciones(items);
        if (items[0]?.idLocalizacion) setSelectedLoc(items[0].idLocalizacion);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <Text style={styles.hero}>Alquila tu vehículo</Text>
      <Text style={styles.sub}>Conectado a Render — {resolveApiBaseUrl()}</Text>

      {loading ? (
        <ActivityIndicator color={colors.primary} style={{ marginTop: 24 }} />
      ) : (
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Recogida</Text>
          {localizaciones.map((loc) => (
            <Pressable
              key={loc.idLocalizacion}
              style={[styles.chip, selectedLoc === loc.idLocalizacion && styles.chipActive]}
              onPress={() => setSelectedLoc(loc.idLocalizacion)}
            >
              <Text style={styles.chipText}>{loc.nombre}</Text>
            </Pressable>
          ))}
        </View>
      )}

      <Pressable
        style={styles.cta}
        onPress={() =>
          router.push({ pathname: '/(tabs)/buscar', params: { idLocalizacion: String(selectedLoc) } })
        }
      >
        <Text style={styles.ctaText}>Buscar vehículos</Text>
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.bg },
  content: { padding: 20, paddingBottom: 40 },
  hero: { color: colors.text, fontSize: 28, fontWeight: '800' },
  sub: { color: colors.textMuted, marginTop: 6, fontSize: 12 },
  section: { marginTop: 28 },
  sectionTitle: { color: colors.text, fontWeight: '600', marginBottom: 12 },
  chip: {
    backgroundColor: colors.surface,
    padding: 12,
    borderRadius: 8,
    marginBottom: 8,
    borderWidth: 1,
    borderColor: colors.border,
  },
  chipActive: { borderColor: colors.primary, backgroundColor: colors.surfaceAlt },
  chipText: { color: colors.text },
  cta: {
    marginTop: 32,
    backgroundColor: colors.primary,
    padding: 16,
    borderRadius: 12,
    alignItems: 'center',
  },
  ctaText: { color: '#fff', fontWeight: '700', fontSize: 16 },
});
