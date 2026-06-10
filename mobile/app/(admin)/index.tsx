import { useCallback, useState } from 'react';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { router, useFocusEffect } from 'expo-router';
import { adminClientesApi, adminReservasApi, adminVehiculosApi } from '@/src/api/adminApi';
import { Card } from '@/src/components/ui/Card';
import { Screen } from '@/src/components/ui/Screen';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { unwrapData } from '@/src/utils/apiResponse';

type Stats = {
  totalVehiculos: number;
  disponibles: number;
  alquilados: number;
  totalClientes: number;
  totalReservas: number;
};

export default function AdminDashboard() {
  const user = useAuthStore((s) => s.user);
  const [stats, setStats] = useState<Stats | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [vRes, cRes, rRes] = await Promise.all([
        adminVehiculosApi.getAll(),
        adminClientesApi.getAll(),
        adminReservasApi.getAll(),
      ]);
      const vehiculos = unwrapData<Record<string, unknown>[]>(vRes) ?? [];
      const clientes = unwrapData<unknown[]>(cRes) ?? [];
      const reservas = unwrapData<unknown[]>(rRes) ?? [];
      setStats({
        totalVehiculos: vehiculos.length,
        disponibles: vehiculos.filter((v) => v.estadoOperativo === 'DISPONIBLE').length,
        alquilados: vehiculos.filter((v) => v.estadoOperativo === 'ALQUILADO').length,
        totalClientes: clientes.length,
        totalReservas: reservas.length,
      });
    } catch {
      setStats(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(
    useCallback(() => {
      load();
    }, [load]),
  );

  const cards = [
    { title: 'Vehículos', value: stats?.totalVehiculos, sub: `${stats?.disponibles ?? '—'} disponibles`, route: '/(admin)/vehiculos', color: colors.primary },
    { title: 'Clientes', value: stats?.totalClientes, sub: 'registrados', route: '/(admin)/clientes', color: colors.success },
    { title: 'Reservas', value: stats?.totalReservas, sub: 'en sistema', route: '/(admin)/reservas', color: colors.info },
    { title: 'Alquilados', value: stats?.alquilados, sub: 'en uso', route: '/(admin)/vehiculos', color: colors.warning },
  ];

  return (
    <Screen>
      <View style={styles.welcome}>
        <View style={{ flex: 1 }}>
          <Text style={styles.greeting}>Bienvenido, {user?.username ?? 'Admin'}</Text>
          <Text style={styles.sub}>Panel de administración Europcar Rental</Text>
        </View>
        <Pressable style={styles.refreshBtn} onPress={load} disabled={loading}>
          <Text style={styles.refreshText}>{loading ? '…' : '↻'}</Text>
        </Pressable>
      </View>

      {loading && !stats ? (
        <ActivityIndicator color={colors.primary} size="large" style={{ marginTop: 40 }} />
      ) : (
        <View style={styles.grid}>
          {cards.map((card) => (
            <Pressable key={card.title} onPress={() => router.push(card.route as never)} style={styles.gridItem}>
              <Card style={styles.statCard}>
                <View style={[styles.iconBox, { backgroundColor: card.color }]}>
                  <Text style={styles.iconText}>●</Text>
                </View>
                <Text style={styles.statValue}>{card.value ?? '—'}</Text>
                <Text style={styles.statTitle}>{card.title}</Text>
                <Text style={styles.statSub}>{card.sub}</Text>
              </Card>
            </Pressable>
          ))}
        </View>
      )}
    </Screen>
  );
}

const styles = StyleSheet.create({
  welcome: { flexDirection: 'row', alignItems: 'flex-start', marginBottom: spacing.lg },
  greeting: { color: colors.text, fontSize: 22, fontWeight: '800' },
  sub: { color: colors.textSecondary, marginTop: 4 },
  refreshBtn: {
    width: 40,
    height: 40,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.borderLight,
    alignItems: 'center',
    justifyContent: 'center',
  },
  refreshText: { color: colors.primaryLight, fontSize: 20 },
  grid: { flexDirection: 'row', flexWrap: 'wrap', marginHorizontal: -6 },
  gridItem: { width: '50%', padding: 6 },
  statCard: { marginBottom: 0, minHeight: 140 },
  iconBox: {
    width: 36,
    height: 36,
    borderRadius: radius.sm,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.sm,
  },
  iconText: { color: colors.white, fontSize: 10 },
  statValue: { color: colors.text, fontSize: 28, fontWeight: '800' },
  statTitle: { color: colors.text, fontWeight: '700', marginTop: 4 },
  statSub: { color: colors.textMuted, fontSize: 12, marginTop: 2 },
});
