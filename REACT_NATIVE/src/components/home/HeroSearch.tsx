import { useEffect, useMemo, useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { bookingApi } from '@/src/api/bookingApi';
import { Button } from '@/src/components/ui/Button';
import { DateTimeSelector } from '@/src/components/ui/DateTimeSelector';
import { GradientBackground } from '@/src/components/ui/GradientBackground';
import { Select } from '@/src/components/ui/Select';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, shadows, spacing } from '@/src/theme/layout';
import { fonts, text } from '@/src/theme/typography';
import { getPayload } from '@/src/utils/bookingNormalize';

type Ciudad = { idCiudad: number; idPais?: number; nombreCiudad?: string; nombrePais?: string };
type Localizacion = { idLocalizacion: number; nombreLocalizacion?: string; idCiudad?: number };

function defaultPickup() {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  d.setHours(10, 0, 0, 0);
  return d;
}

function defaultReturn(pickup: Date) {
  const d = new Date(pickup);
  d.setDate(d.getDate() + 3);
  return d;
}

export function HeroSearch() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const userType = useAuthStore((s) => s.userType);

  const [ciudades, setCiudades] = useState<Ciudad[]>([]);
  const [localizaciones, setLocalizaciones] = useState<Localizacion[]>([]);
  const [idPais, setIdPais] = useState('');
  const [idCiudad, setIdCiudad] = useState('');
  const [idLocalizacion, setIdLocalizacion] = useState('');
  const [fechaRecogida, setFechaRecogida] = useState(defaultPickup);
  const [fechaDevolucion, setFechaDevolucion] = useState(() => defaultReturn(defaultPickup()));

  useEffect(() => {
    (async () => {
      try {
        const [locRes, ciuRes] = await Promise.allSettled([
          bookingApi.getLocalizaciones({ page: 1, limit: 200 }),
          bookingApi.getCiudades(),
        ]);
        if (locRes.status === 'fulfilled') {
          const payload = getPayload<{ localizaciones?: Localizacion[]; Localizaciones?: Localizacion[] }>(locRes.value);
          const locs = payload.localizaciones ?? payload.Localizaciones ?? [];
          setLocalizaciones(
            locs
              .map((l) => ({
                idLocalizacion: Number(l.idLocalizacion),
                nombreLocalizacion: l.nombreLocalizacion ?? (l as { nombre?: string }).nombre,
                idCiudad: l.idCiudad,
              }))
              .filter((l) => l.idLocalizacion > 0),
          );
        }
        if (ciuRes.status === 'fulfilled') {
          const payload = getPayload<{ ciudades?: Ciudad[]; Ciudades?: Ciudad[] }>(ciuRes.value);
          const cius = payload.ciudades ?? payload.Ciudades ?? [];
          setCiudades(
            cius
              .map((c) => ({
                idCiudad: Number(c.idCiudad),
                idPais: c.idPais,
                nombreCiudad: c.nombreCiudad ?? (c as { nombre?: string }).nombre,
                nombrePais: c.nombrePais,
              }))
              .filter((c) => c.idCiudad > 0),
          );
        }
      } catch {
        /* opcional */
      }
    })();
  }, []);

  const paises = useMemo(() => {
    const map = new Map<string, string>();
    ciudades.forEach((c) => {
      if (c.idPais) map.set(String(c.idPais), c.nombrePais || `País ${c.idPais}`);
    });
    return Array.from(map.entries()).map(([value, label]) => ({ value, label }));
  }, [ciudades]);

  const ciudadesFiltradas = useMemo(
    () => (idPais ? ciudades.filter((c) => String(c.idPais) === idPais) : ciudades),
    [ciudades, idPais],
  );

  const locsFiltradas = useMemo(
    () => (idCiudad ? localizaciones.filter((l) => String(l.idCiudad) === idCiudad) : localizaciones),
    [localizaciones, idCiudad],
  );

  const handleSearch = () => {
    const fr = fechaRecogida.toISOString().slice(0, 10);
    const fd = fechaDevolucion.toISOString().slice(0, 10);
    router.push({
      pathname: '/(tabs)/buscar',
      params: {
        idLocalizacion: idLocalizacion || String(locsFiltradas[0]?.idLocalizacion ?? 1),
        fechaRecogida: fr,
        fechaDevolucion: fd,
      },
    });
  };

  const accountPath = isAuthenticated
    ? userType === 'admin'
      ? '/(admin)/cuenta'
      : '/(tabs)/cuenta'
    : '/(auth)/login';

  return (
    <GradientBackground variant="hero" style={styles.heroWrap}>
      <View style={styles.heroInner}>
        <Text style={styles.badge}>🚗 La mejor experiencia en renta de vehículos</Text>
        <Text style={styles.title}>
          Renta tu vehículo{'\n'}
          <Text style={styles.titleAccent}>ideal hoy</Text>
        </Text>
        <Text style={styles.subtitle}>
          Amplia flota, precios competitivos y servicio premium. Recoge y devuelve en múltiples sucursales.
        </Text>

        <View style={styles.searchCard}>
          <Select
            label="País"
            value={idPais}
            onValueChange={(v) => { setIdPais(v); setIdCiudad(''); setIdLocalizacion(''); }}
            options={paises}
            placeholder="Todos los países"
          />
          <Select
            label="Ciudad"
            value={idCiudad}
            onValueChange={(v) => { setIdCiudad(v); setIdLocalizacion(''); }}
            options={ciudadesFiltradas.map((c) => ({
              value: String(c.idCiudad),
              label: `${c.nombreCiudad}${c.nombrePais ? ` · ${c.nombrePais}` : ''}`,
            }))}
            placeholder="Todas las ciudades"
          />
          <Select
            label="Sucursal"
            value={idLocalizacion}
            onValueChange={setIdLocalizacion}
            options={locsFiltradas.map((l) => ({
              value: String(l.idLocalizacion),
              label: l.nombreLocalizacion ?? `Sucursal ${l.idLocalizacion}`,
            }))}
            placeholder="Todas las sucursales"
          />
          <DateTimeSelector label="Recogida" value={fechaRecogida} onChange={setFechaRecogida} />
          <DateTimeSelector
            label="Devolución"
            value={fechaDevolucion}
            onChange={setFechaDevolucion}
            minimumDate={fechaRecogida}
          />
          <Button label="Buscar vehículos" onPress={handleSearch} variant="client" />
        </View>

        <View style={styles.ctas}>
          <Button label="Ver catálogo completo" variant="client" onPress={() => router.push('/(tabs)/catalogo')} />
          <Pressable style={styles.outlineBtn} onPress={() => router.push(accountPath as never)}>
            <Ionicons name="log-in-outline" size={18} color={colors.primaryLight} />
            <Text style={styles.outlineText}>Acceder a tu cuenta</Text>
          </Pressable>
        </View>
      </View>
    </GradientBackground>
  );
}

const STATS = [
  { n: '50+', l: 'Vehículos' },
  { n: '5', l: 'Sucursales' },
  { n: '24/7', l: 'Soporte' },
  { n: '4.8', l: 'Calificación' },
];

const STEPS = [
  { n: '1', icon: 'search-outline' as const, title: 'Busca', text: 'Explora el catálogo y filtra por marca, categoría o precio' },
  { n: '2', icon: 'car-sport-outline' as const, title: 'Reserva', text: 'Selecciona fechas, agrega extras y confirma tu reserva' },
  { n: '3', icon: 'shield-checkmark-outline' as const, title: 'Disfruta', text: 'Recoge tu vehículo y disfruta tu viaje con total seguridad' },
];

export function HomeStats() {
  return (
    <View style={styles.statsRow}>
      {STATS.map((s) => (
        <View key={s.l} style={styles.stat}>
          <Text style={styles.statN}>{s.n}</Text>
          <Text style={styles.statL}>{s.l}</Text>
        </View>
      ))}
    </View>
  );
}

export function HomeSteps() {
  return (
    <View style={styles.section}>
      <Text style={styles.sectionTitle}>¿Cómo funciona?</Text>
      <Text style={styles.sectionSub}>Renta tu vehículo en 3 simples pasos</Text>
      <View style={styles.steps}>
        {STEPS.map((s) => (
          <View key={s.n} style={styles.stepCard}>
            <Text style={styles.stepN}>{s.n}</Text>
            <Ionicons name={s.icon} size={28} color={colors.primaryLight} />
            <Text style={styles.stepTitle}>{s.title}</Text>
            <Text style={styles.stepText}>{s.text}</Text>
          </View>
        ))}
      </View>
    </View>
  );
}

type Categoria = { idCategoria?: number; nombre?: string; nombreCategoria?: string; descripcion?: string };

export function HomeCategories({ categorias }: { categorias: Categoria[] }) {
  if (!categorias.length) return null;
  return (
    <View style={styles.section}>
      <Text style={styles.sectionTitle}>Categorías de vehículos</Text>
      <Text style={styles.sectionSub}>El vehículo perfecto para cada necesidad</Text>
      <View style={styles.catGrid}>
        {categorias.map((cat) => {
          const name = cat.nombreCategoria ?? cat.nombre ?? 'Categoría';
          return (
            <Pressable
              key={String(cat.idCategoria ?? name)}
              style={styles.catCard}
              onPress={() => router.push('/(tabs)/catalogo')}
            >
              <Ionicons name="car-outline" size={32} color={colors.primaryLight} />
              <Text style={styles.catTitle}>{name}</Text>
              {cat.descripcion ? <Text style={styles.catDesc}>{cat.descripcion}</Text> : null}
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  heroWrap: { borderRadius: radius.xl, overflow: 'hidden', marginBottom: spacing.xl, ...shadows.md },
  heroInner: { padding: spacing.xl },
  badge: { color: colors.primaryLight, fontFamily: fonts.semiBold, fontSize: 13 },
  title: { ...text.heroTitle, color: colors.text, marginTop: spacing.md },
  titleAccent: { color: colors.primaryLight },
  subtitle: { color: colors.textSecondary, marginTop: spacing.md, lineHeight: 22, fontFamily: fonts.regular },
  searchCard: {
    marginTop: spacing.xl,
    backgroundColor: 'rgba(17,24,39,0.85)',
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.lg,
    ...shadows.sm,
  },
  ctas: { marginTop: spacing.lg, gap: spacing.sm },
  outlineBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.sm,
    borderWidth: 1,
    borderColor: colors.primary,
    borderRadius: radius.md,
    paddingVertical: 14,
  },
  outlineText: { color: colors.primaryLight, fontFamily: fonts.semiBold },
  statsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.md,
    marginBottom: spacing.xxl,
    justifyContent: 'space-between',
  },
  stat: {
    flex: 1,
    minWidth: 72,
    alignItems: 'center',
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    padding: spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
  },
  statN: { color: colors.primaryLight, fontFamily: fonts.extraBold, fontSize: 22 },
  statL: { color: colors.textMuted, fontSize: 12, marginTop: 4, fontFamily: fonts.medium },
  section: { marginBottom: spacing.xxl },
  sectionTitle: { ...text.h2, color: colors.text },
  sectionSub: { color: colors.textSecondary, marginTop: 4, marginBottom: spacing.lg, fontFamily: fonts.regular },
  steps: { gap: spacing.md },
  stepCard: {
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    padding: spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
    gap: spacing.sm,
  },
  stepN: {
    position: 'absolute',
    top: spacing.md,
    right: spacing.md,
    color: colors.primaryGhost,
    fontFamily: fonts.extraBold,
    fontSize: 28,
  },
  stepTitle: { color: colors.text, fontFamily: fonts.bold, fontSize: 17 },
  stepText: { color: colors.textSecondary, fontFamily: fonts.regular, lineHeight: 20 },
  catGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.md },
  catCard: {
    width: '47%',
    minWidth: 140,
    flexGrow: 1,
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    padding: spacing.lg,
    borderWidth: 1,
    borderColor: colors.border,
    gap: spacing.sm,
  },
  catTitle: { color: colors.text, fontFamily: fonts.bold },
  catDesc: { color: colors.textMuted, fontSize: 12 },
});
