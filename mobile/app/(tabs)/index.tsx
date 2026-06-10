import { useEffect, useState } from 'react';
import { ActivityIndicator, Platform, Pressable, StyleSheet, Text, View } from 'react-native';
import DateTimePicker from '@react-native-community/datetimepicker';
import { router } from 'expo-router';
import { bookingApi } from '@/src/api/bookingApi';
import { Button } from '@/src/components/ui/Button';
import { Card } from '@/src/components/ui/Card';
import { Screen } from '@/src/components/ui/Screen';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { unwrapData } from '@/src/utils/apiResponse';

type Localizacion = { idLocalizacion: number; nombre: string; codigo?: string };

function formatDateParam(d: Date) {
  return d.toISOString().slice(0, 10);
}

export default function HomeScreen() {
  const [localizaciones, setLocalizaciones] = useState<Localizacion[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedLoc, setSelectedLoc] = useState<number>(1);
  const [fechaRecogida, setFechaRecogida] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return d;
  });
  const [fechaDevolucion, setFechaDevolucion] = useState(() => {
    const d = new Date();
    d.setDate(d.getDate() + 4);
    return d;
  });
  const [picker, setPicker] = useState<'recogida' | 'devolucion' | null>(null);

  useEffect(() => {
    (async () => {
      try {
        const res = await bookingApi.getLocalizaciones({ page: 1, limit: 20 });
        const data = unwrapData<{ items?: Localizacion[] }>(res);
        const items = data?.items ?? (Array.isArray(data) ? (data as Localizacion[]) : []);
        setLocalizaciones(items);
        if (items[0]?.idLocalizacion) setSelectedLoc(items[0].idLocalizacion);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const buscar = () => {
    router.push({
      pathname: '/(tabs)/buscar',
      params: {
        idLocalizacion: String(selectedLoc),
        fechaRecogida: formatDateParam(fechaRecogida),
        fechaDevolucion: formatDateParam(fechaDevolucion),
      },
    });
  };

  return (
    <Screen>
      <View style={styles.hero}>
        <Text style={styles.heroTitle}>Alquila tu vehículo</Text>
        <Text style={styles.heroSub}>Reserva en minutos con Europcar Rental</Text>
      </View>

      <Card>
        <Text style={styles.sectionTitle}>Recogida y devolución</Text>

        <Pressable style={styles.dateRow} onPress={() => setPicker('recogida')}>
          <Text style={styles.dateLabel}>Recogida</Text>
          <Text style={styles.dateValue}>{formatDateParam(fechaRecogida)}</Text>
        </Pressable>
        <Pressable style={styles.dateRow} onPress={() => setPicker('devolucion')}>
          <Text style={styles.dateLabel}>Devolución</Text>
          <Text style={styles.dateValue}>{formatDateParam(fechaDevolucion)}</Text>
        </Pressable>

        {picker ? (
          <DateTimePicker
            value={picker === 'recogida' ? fechaRecogida : fechaDevolucion}
            mode="date"
            minimumDate={new Date()}
            display={Platform.OS === 'ios' ? 'spinner' : 'default'}
            onChange={(_, date) => {
              if (Platform.OS === 'android') setPicker(null);
              if (!date) return;
              if (picker === 'recogida') {
                setFechaRecogida(date);
                if (date >= fechaDevolucion) {
                  const fin = new Date(date);
                  fin.setDate(fin.getDate() + 2);
                  setFechaDevolucion(fin);
                }
              } else {
                setFechaDevolucion(date);
              }
              if (Platform.OS === 'ios') setPicker(null);
            }}
          />
        ) : null}
      </Card>

      <Text style={styles.sectionTitle}>Oficina de recogida</Text>
      {loading ? (
        <ActivityIndicator color={colors.primary} style={{ marginTop: spacing.md }} />
      ) : (
        localizaciones.map((loc) => (
          <Pressable
            key={loc.idLocalizacion}
            style={[styles.chip, selectedLoc === loc.idLocalizacion && styles.chipActive]}
            onPress={() => setSelectedLoc(loc.idLocalizacion)}
          >
            <Text style={styles.chipText}>{loc.nombre}</Text>
            {loc.codigo ? <Text style={styles.chipCode}>{loc.codigo}</Text> : null}
          </Pressable>
        ))
      )}

      <Button label="Buscar vehículos" onPress={buscar} variant="client" style={{ marginTop: spacing.lg }} />
    </Screen>
  );
}

const styles = StyleSheet.create({
  hero: {
    marginBottom: spacing.lg,
    padding: spacing.lg,
    borderRadius: radius.lg,
    backgroundColor: colors.primaryGhost,
    borderWidth: 1,
    borderColor: 'rgba(13,148,136,0.2)',
  },
  heroTitle: { color: colors.text, fontSize: 28, fontWeight: '800' },
  heroSub: { color: colors.textSecondary, marginTop: 6 },
  sectionTitle: { color: colors.text, fontWeight: '700', marginBottom: spacing.sm },
  dateRow: {
    backgroundColor: colors.bgSecondary,
    padding: spacing.md,
    borderRadius: radius.md,
    marginBottom: spacing.sm,
    borderWidth: 1,
    borderColor: colors.border,
  },
  dateLabel: { color: colors.textMuted, fontSize: 12 },
  dateValue: { color: colors.text, fontSize: 16, fontWeight: '600', marginTop: 4 },
  chip: {
    backgroundColor: colors.surface,
    padding: spacing.md,
    borderRadius: radius.md,
    marginBottom: spacing.sm,
    borderWidth: 1,
    borderColor: colors.border,
  },
  chipActive: { borderColor: colors.accent, backgroundColor: colors.clientGhost },
  chipText: { color: colors.text, fontWeight: '600' },
  chipCode: { color: colors.textMuted, fontSize: 12, marginTop: 2 },
});
