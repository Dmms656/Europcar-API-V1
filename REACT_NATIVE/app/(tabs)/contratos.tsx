import { useEffect, useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';
import { contratosApi, type ContratoItem } from '@/src/api/contratosApi';
import { ContratoCard } from '@/src/components/domain/ContratoCard';
import { EmptyState } from '@/src/components/ui/EmptyState';
import { Modal } from '@/src/components/ui/Modal';
import { PaginationControls } from '@/src/components/ui/PaginationControls';
import { Screen } from '@/src/components/ui/Screen';
import { StatusBadge } from '@/src/components/ui/StatusBadge';
import { useClientPagination } from '@/src/hooks/useClientPagination';
import { useAuthStore } from '@/src/store/useAuthStore';
import { colors } from '@/src/theme/colors';
import { spacing } from '@/src/theme/layout';
import { unwrapData } from '@/src/utils/apiResponse';
import {
  contratoCodigo,
  contratoEstado,
  formatCurrency,
  formatDateEs,
} from '@/src/utils/format';

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <View style={styles.detailRow}>
      <Text style={styles.detailLabel}>{label}</Text>
      <Text style={styles.detailValue}>{value}</Text>
    </View>
  );
}

export default function ContratosScreen() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const user = useAuthStore((s) => s.user);

  const [contratos, setContratos] = useState<ContratoItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<ContratoItem | null>(null);

  const pagination = useClientPagination(contratos, 10);

  useEffect(() => {
    if (!isAuthenticated) return;
    loadContratos();
  }, [isAuthenticated]);

  const loadContratos = async () => {
    setLoading(true);
    try {
      const res = await contratosApi.getMisContratos();
      const data = unwrapData<ContratoItem[]>(res);
      setContratos(Array.isArray(data) ? data : []);
    } catch {
      setContratos([]);
    } finally {
      setLoading(false);
    }
  };

  if (!isAuthenticated) {
    return (
      <Screen>
        <EmptyState
          title="Inicia sesión para ver tus contratos"
          icon="lock-closed-outline"
        />
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

  return (
    <Screen>
      <Text style={styles.title}>Mis Contratos</Text>
      <Text style={styles.subtitle}>Contratos de arrendamiento activos y cerrados</Text>

      {contratos.length === 0 ? (
        <EmptyState title="No tienes contratos registrados aún" icon="document-text-outline" />
      ) : (
        <>
          {pagination.paginatedItems.map((c) => (
            <ContratoCard
              key={String(c.idContrato ?? c.id)}
              contrato={c}
              onPress={() => setSelected(c)}
            />
          ))}
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
        </>
      )}

      <Modal
        visible={Boolean(selected)}
        title="Detalle de contrato"
        onClose={() => setSelected(null)}
      >
        {selected ? (
          <ScrollView showsVerticalScrollIndicator={false}>
            <View style={styles.modalBadge}>
              <StatusBadge label={contratoEstado(selected)} />
              <Text style={styles.modalCode}>{contratoCodigo(selected)}</Text>
            </View>
            <DetailRow
              label="Vehículo"
              value={selected.placaVehiculo || selected.vehiculo || '—'}
            />
            <DetailRow
              label="Cliente"
              value={selected.nombreCliente || user?.nombreCompleto || '—'}
            />
            <DetailRow
              label="Fecha salida"
              value={formatDateEs(selected.fechaHoraSalida || selected.fechaSalida, true)}
            />
            <DetailRow
              label="Fecha devolución"
              value={formatDateEs(selected.fechaHoraDevolucion || selected.fechaDevolucion, true)}
            />
            <DetailRow
              label="KM salida"
              value={selected.kmSalida?.toLocaleString() ?? '—'}
            />
            <DetailRow
              label="KM entrega"
              value={selected.kmEntrega?.toLocaleString() ?? 'Pendiente'}
            />
            <DetailRow
              label="Depósito"
              value={formatCurrency(selected.depositoGarantia ?? selected.deposito)}
            />
            <View style={styles.modalTotal}>
              <Text style={styles.modalTotalLabel}>Total del contrato</Text>
              <Text style={styles.modalTotalValue}>
                {formatCurrency(selected.totalContrato ?? selected.total)}
              </Text>
            </View>
          </ScrollView>
        ) : null}
      </Modal>
    </Screen>
  );
}

const styles = StyleSheet.create({
  centered: { justifyContent: 'center', alignItems: 'center' },
  title: { color: colors.text, fontSize: 26, fontWeight: '800' },
  subtitle: { color: colors.textSecondary, marginBottom: spacing.lg, marginTop: spacing.xs },
  detailRow: { marginBottom: spacing.md },
  detailLabel: { color: colors.textMuted, fontSize: 12, fontWeight: '600', marginBottom: 4 },
  detailValue: { color: colors.text, fontSize: 15 },
  modalBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    marginBottom: spacing.lg,
  },
  modalCode: { color: colors.text, fontWeight: '700', fontSize: 15 },
  modalTotal: {
    marginTop: spacing.lg,
    paddingTop: spacing.lg,
    borderTopWidth: 1,
    borderTopColor: colors.border,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  modalTotalLabel: { color: colors.textSecondary, fontWeight: '600' },
  modalTotalValue: { color: colors.accent, fontWeight: '800', fontSize: 20 },
});
