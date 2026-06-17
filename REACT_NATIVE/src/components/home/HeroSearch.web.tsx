import { useEffect, useMemo, useState } from 'react';
import { Pressable, StyleSheet, Text, View, useWindowDimensions } from 'react-native';
import { router } from 'expo-router';
import { Ionicons } from '@expo/vector-icons';
import { bookingApi } from '@/src/api/bookingApi';
import { Button } from '@/src/components/ui/Button';
import { GradientBackground } from '@/src/components/ui/GradientBackground';
import { Select } from '@/src/components/ui/Select';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts, text } from '@/src/theme/typography';
import { getPayload } from '@/src/utils/bookingNormalize';

type Ciudad = { idCiudad: number; idPais?: number; nombreCiudad?: string; nombrePais?: string };
type Localizacion = { idLocalizacion: number; nombreLocalizacion?: string; idCiudad?: number };

export function HeroSearch() {
  const { width } = useWindowDimensions();
  const isWide = width >= 1024;
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const userType = useAuthStore((s) => s.userType);

  const [ciudades, setCiudades] = useState<Ciudad[]>([]);
  const [localizaciones, setLocalizaciones] = useState<Localizacion[]>([]);
  const [idPais, setIdPais] = useState('');
  const [idCiudad, setIdCiudad] = useState('');
  const [idLocalizacion, setIdLocalizacion] = useState('');

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
    const params = new URLSearchParams();
    if (idPais) params.set('pais', idPais);
    if (idCiudad) params.set('ciudad', idCiudad);
    if (idLocalizacion) params.set('localizacion', idLocalizacion);
    const qs = params.toString();
    router.push(qs ? `/catalogo?${qs}` : '/catalogo');
  };

  const accountPath = isAuthenticated
    ? userType === 'admin'
      ? '/(admin)'
      : '/(tabs)/cuenta'
    : '/(auth)/login';

  return (
    <GradientBackground variant="hero" style={styles.hero}>
      <View style={styles.heroContent}>
        <Text style={styles.badge}>🚗 La mejor experiencia en renta de vehículos</Text>
        <Text style={styles.title}>
          Renta tu vehículo{'\n'}
          <Text style={styles.titleAccent}>ideal hoy</Text>
        </Text>
        <Text style={styles.subtitle}>
          Amplia flota de vehículos, precios competitivos y servicio premium.
          Recoge y devuelve en múltiples sucursales.
        </Text>

        <View style={[styles.searchForm, isWide && styles.searchFormWide]}>
          <View style={[styles.field, isWide && styles.fieldInline]}>
            <Select
              label="País"
              value={idPais}
              onValueChange={(v) => { setIdPais(v); setIdCiudad(''); setIdLocalizacion(''); }}
              options={paises}
              placeholder="Todos los países"
            />
          </View>
          <View style={[styles.field, isWide && styles.fieldInline]}>
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
          </View>
          <View style={[styles.field, isWide && styles.fieldInline]}>
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
          </View>
          <Text style={styles.dateHint}>Las fechas de recogida y devolución las eliges al reservar.</Text>
          <Button label="Ver vehículos" variant="client" onPress={handleSearch} style={isWide ? styles.searchBtn : undefined} />
        </View>

        <View style={[styles.ctas, isWide && styles.ctasRow]}>
          <Button label="Ver catálogo completo" variant="client" onPress={() => router.push('/catalogo')} />
          <Pressable style={styles.outlineBtn} onPress={() => router.push(accountPath as never)}>
            <Ionicons name="log-in-outline" size={20} color={colors.primaryLight} />
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
  { n: '2', icon: 'car-sport-outline' as const, title: 'Reserva', text: 'Selecciona fechas, agrega extras y realiza tu pago en línea' },
  { n: '3', icon: 'shield-checkmark-outline' as const, title: 'Disfruta', text: 'Recoge tu vehículo y disfruta tu viaje con total seguridad' },
];

export function HomeStats() {
  return (
    <View style={styles.statsSection}>
      <View style={styles.statsInner}>
        {STATS.map((s) => (
          <View key={s.l} style={styles.stat}>
            <Text style={styles.statN}>{s.n}</Text>
            <Text style={styles.statL}>{s.l}</Text>
          </View>
        ))}
      </View>
    </View>
  );
}

export function HomeSteps() {
  return (
    <View style={styles.sectionAlt}>
      <View style={styles.sectionInner}>
        <Text style={styles.sectionTitle}>¿Cómo funciona?</Text>
        <Text style={styles.sectionSub}>Renta tu vehículo en 3 simples pasos</Text>
        <View style={styles.stepsGrid}>
          {STEPS.map((s) => (
            <View key={s.n} style={styles.stepCard}>
              <Text style={styles.stepN}>{s.n}</Text>
              <Ionicons name={s.icon} size={32} color={colors.primaryLight} />
              <Text style={styles.stepTitle}>{s.title}</Text>
              <Text style={styles.stepText}>{s.text}</Text>
            </View>
          ))}
        </View>
      </View>
    </View>
  );
}

type Categoria = { idCategoria?: number; nombre?: string; nombreCategoria?: string; descripcion?: string };

export function HomeCategories({ categorias }: { categorias: Categoria[] }) {
  if (!categorias.length) return null;
  return (
    <View style={styles.section}>
      <View style={styles.sectionInner}>
        <Text style={styles.sectionTitle}>Categorías de vehículos</Text>
        <Text style={styles.sectionSub}>El vehículo perfecto para cada necesidad</Text>
        <View style={styles.catGrid}>
          {categorias.map((cat) => {
            const name = cat.nombreCategoria ?? cat.nombre ?? 'Categoría';
            return (
              <Pressable
                key={String(cat.idCategoria ?? name)}
                style={styles.catCard}
                onPress={() => router.push(`/catalogo?categoria=${encodeURIComponent(name)}`)}
              >
                <Ionicons name="car-outline" size={36} color={colors.primaryLight} />
                <Text style={styles.catTitle}>{name}</Text>
                {cat.descripcion ? <Text style={styles.catDesc}>{cat.descripcion}</Text> : null}
              </Pressable>
            );
          })}
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  hero: { borderRadius: 0, minHeight: 480, justifyContent: 'center' },
  heroContent: { paddingVertical: spacing.xxl, paddingHorizontal: spacing.xl, maxWidth: 1200, alignSelf: 'center', width: '100%' },
  badge: { color: colors.primaryLight, fontFamily: fonts.semiBold, fontSize: 14 },
  title: { ...text.heroTitle, fontSize: 48, color: colors.text, marginTop: spacing.md, lineHeight: 54 },
  titleAccent: { color: colors.primaryLight },
  subtitle: { color: colors.textSecondary, marginTop: spacing.md, fontSize: 17, lineHeight: 26, fontFamily: fonts.regular, maxWidth: 640 },
  searchForm: {
    marginTop: spacing.xl,
    backgroundColor: 'rgba(17,24,39,0.88)',
    borderRadius: radius.xl,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.lg,
    gap: spacing.md,
  },
  searchFormWide: { flexDirection: 'row', flexWrap: 'wrap', alignItems: 'flex-end' },
  field: { flex: 1, minWidth: 160, gap: spacing.xs },
  fieldInline: { minWidth: 140 },
  dateHint: { color: colors.textMuted, fontSize: 12, fontFamily: fonts.regular, width: '100%' },
  searchBtn: { minWidth: 120, alignSelf: 'flex-end' },
  ctas: { marginTop: spacing.xl, gap: spacing.md },
  ctasRow: { flexDirection: 'row', flexWrap: 'wrap', alignItems: 'center' },
  outlineBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.sm,
    borderWidth: 1,
    borderColor: colors.primary,
    borderRadius: radius.md,
    paddingVertical: 14,
    paddingHorizontal: spacing.xl,
  },
  outlineText: { color: colors.primaryLight, fontFamily: fonts.semiBold, fontSize: 15 },
  statsSection: { backgroundColor: colors.surface, borderTopWidth: 1, borderBottomWidth: 1, borderColor: colors.border },
  statsInner: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    maxWidth: 1200,
    alignSelf: 'center',
    width: '100%',
    paddingVertical: spacing.xl,
    paddingHorizontal: spacing.xl,
    justifyContent: 'space-around',
    gap: spacing.lg,
  },
  stat: { alignItems: 'center', minWidth: 100 },
  statN: { color: colors.primaryLight, fontFamily: fonts.extraBold, fontSize: 32 },
  statL: { color: colors.textMuted, fontSize: 13, marginTop: 4, fontFamily: fonts.medium },
  section: { paddingVertical: spacing.xxl },
  sectionAlt: { paddingVertical: spacing.xxl, backgroundColor: 'rgba(26,31,46,0.5)' },
  sectionInner: { maxWidth: 1200, alignSelf: 'center', width: '100%', paddingHorizontal: spacing.xl },
  sectionTitle: { ...text.h2, fontSize: 28, color: colors.text, textAlign: 'center' },
  sectionSub: { color: colors.textSecondary, marginTop: spacing.sm, marginBottom: spacing.xl, fontFamily: fonts.regular, textAlign: 'center' },
  stepsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.lg, justifyContent: 'center' },
  stepCard: {
    flex: 1,
    minWidth: 260,
    maxWidth: 360,
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    padding: spacing.xl,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    gap: spacing.sm,
  },
  stepN: { position: 'absolute', top: spacing.md, right: spacing.md, color: colors.primaryGhost, fontFamily: fonts.extraBold, fontSize: 32 },
  stepTitle: { color: colors.text, fontFamily: fonts.bold, fontSize: 20 },
  stepText: { color: colors.textSecondary, fontFamily: fonts.regular, textAlign: 'center', lineHeight: 22 },
  catGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.lg, justifyContent: 'center' },
  catCard: {
    width: 220,
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    padding: spacing.xl,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    gap: spacing.sm,
  },
  catTitle: { color: colors.text, fontFamily: fonts.bold, fontSize: 16, textAlign: 'center' },
  catDesc: { color: colors.textMuted, fontSize: 12, textAlign: 'center' },
});
