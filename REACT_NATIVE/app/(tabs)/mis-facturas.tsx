import { useEffect, useMemo, useState } from 'react';
import { ActivityIndicator, Alert, ScrollView, StyleSheet, Text, View } from 'react-native';
import { facturasApi, type FacturaItem } from '@/src/api/facturasApi';
import { FacturaCard } from '@/src/components/domain/FacturaCard';
import { EmptyState } from '@/src/components/ui/EmptyState';
import { Input } from '@/src/components/ui/Input';
import { PaginationControls } from '@/src/components/ui/PaginationControls';
import { Screen } from '@/src/components/ui/Screen';
import { useBreakpoint } from '@/src/hooks/useBreakpoint';
import { useClientPagination } from '@/src/hooks/useClientPagination';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { getErrorMessage, unwrapData } from '@/src/utils/apiResponse';

export default function FacturasScreen() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const { isDesktop } = useBreakpoint();

  const [facturas, setFacturas] = useState<FacturaItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');

  const filtered = useMemo(() => {
    const q = search.toLowerCase().trim();
    if (!q) return facturas;
    return facturas.filter((f) => {
      const text = `${f.numeroFactura || ''} ${f.codigoReserva || ''} ${f.numeroContrato || ''} ${f.estadoFactura || ''}`.toLowerCase();
      return text.includes(q);
    });
  }, [facturas, search]);

  const pagination = useClientPagination(filtered, 10, search);

  useEffect(() => {
    if (!isAuthenticated) return;
    loadFacturas();
  }, [isAuthenticated]);

  const loadFacturas = async () => {
    setLoading(true);
    try {
      const res = await facturasApi.getMyFacturas();
      const data = unwrapData<FacturaItem[]>(res);
      setFacturas(Array.isArray(data) ? data : []);
    } catch (e) {
      Alert.alert('Error', getErrorMessage(e));
      setFacturas([]);
    } finally {
      setLoading(false);
    }
  };

  if (!isAuthenticated) {
    return (
      <Screen>
        <EmptyState title="Inicia sesión para ver tus facturas" icon="lock-closed-outline" />
      </Screen>
    );
  }

  if (loading) {
    return (
      <Screen scroll={false} style={styles.centered}>
        <ActivityIndicator size="large" color={colors.primary} />
      </Screen>
    );
  }

  const showTable = isDesktop && filtered.length > 0;

  return (
    <Screen>
      <Text style={styles.title}>Mis Facturas</Text>
      <Text style={styles.subtitle}>{facturas.length} facturas emitidas</Text>

      <Input
        label="Buscar"
        placeholder="Número, reserva, contrato o estado..."
        value={search}
        onChangeText={setSearch}
      />

      {filtered.length === 0 ? (
        <EmptyState title="No tienes facturas para mostrar" icon="receipt-outline" />
      ) : showTable ? (
        <ScrollView horizontal showsHorizontalScrollIndicator={false}>
          <View style={styles.table}>
            <View style={[styles.row, styles.rowHead]}>
              <Text style={[styles.cell, styles.cellCode]}>Factura</Text>
              <Text style={styles.cell}>Fecha</Text>
              <Text style={styles.cell}>Reserva</Text>
              <Text style={styles.cell}>Contrato</Text>
              <Text style={styles.cell}>Subtotal</Text>
              <Text style={styles.cell}>IVA</Text>
              <Text style={styles.cell}>Total</Text>
              <Text style={styles.cell}>Estado</Text>
            </View>
            {pagination.paginatedItems.map((f) => (
              <FacturaCard key={f.idFactura} factura={f} compact />
            ))}
          </View>
        </ScrollView>
      ) : (
        pagination.paginatedItems.map((f) => <FacturaCard key={f.idFactura} factura={f} />)
      )}

      {filtered.length > 0 ? (
        <PaginationControls
          page={pagination.page}
          totalPages={pagination.totalPages}
          pageSize={pagination.pageSize}
          totalItems={pagination.totalItems}
          startItem={pagination.startItem}
          endItem={pagination.endItem}
          onPageChange={pagination.setPage}
          onPageSizeChange={pagination.setPageSize}
        />
      ) : null}
    </Screen>
  );
}

const styles = StyleSheet.create({
  centered: { justifyContent: 'center', alignItems: 'center' },
  title: { color: colors.text, fontSize: 26, fontWeight: '800' },
  subtitle: { color: colors.textSecondary, marginBottom: spacing.lg, marginTop: spacing.xs },
  table: { minWidth: 900 },
  row: { flexDirection: 'row' },
  rowHead: {
    backgroundColor: colors.surfaceElevated,
    paddingVertical: spacing.sm,
    paddingHorizontal: spacing.sm,
    borderRadius: 8,
    marginBottom: spacing.xs,
  },
  cell: { flex: 1, color: colors.textMuted, fontSize: 11, fontWeight: '700', minWidth: 72, textTransform: 'uppercase' },
  cellCode: { color: colors.textSecondary },
});
