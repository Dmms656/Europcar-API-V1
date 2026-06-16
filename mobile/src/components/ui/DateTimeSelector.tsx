import { useMemo, useState } from 'react';
import {
  Modal,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import DateTimePicker from '@react-native-community/datetimepicker';
import { Ionicons } from '@expo/vector-icons';
import { Button } from '@/src/components/ui/Button';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import {
  formatDateShort,
  formatDateTimeEs,
  formatTime,
  isSameDay,
  mergeDateAndTime,
  startOfDay,
} from '@/src/utils/dateFormat';

const WEEKDAYS = ['Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sa', 'Do'];
const MESES_TITLE = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
  'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
];
const QUICK_HOURS = [8, 10, 12, 14, 18];

type Tab = 'fecha' | 'hora';

type Props = {
  label: string;
  value: Date;
  onChange: (date: Date) => void;
  minimumDate?: Date;
  accent?: 'client' | 'primary';
};

function getMonthGrid(year: number, month: number) {
  const first = new Date(year, month, 1);
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  // Lunes = 0 … Domingo = 6
  const offset = (first.getDay() + 6) % 7;
  const cells: (number | null)[] = [];
  for (let i = 0; i < offset; i++) cells.push(null);
  for (let d = 1; d <= daysInMonth; d++) cells.push(d);
  while (cells.length % 7 !== 0) cells.push(null);
  return cells;
}

export function DateTimeSelector({ label, value, onChange, minimumDate, accent = 'client' }: Props) {
  const [open, setOpen] = useState(false);
  const [tab, setTab] = useState<Tab>('fecha');
  const [draft, setDraft] = useState(value);
  const [viewYear, setViewYear] = useState(value.getFullYear());
  const [viewMonth, setViewMonth] = useState(value.getMonth());

  const accentColor = accent === 'client' ? colors.accent : colors.primary;
  const accentGhost = accent === 'client' ? colors.clientGhost : colors.primaryGhost;
  const minDay = minimumDate ? startOfDay(minimumDate) : startOfDay(new Date());

  const grid = useMemo(() => getMonthGrid(viewYear, viewMonth), [viewYear, viewMonth]);

  const openModal = () => {
    setDraft(value);
    setViewYear(value.getFullYear());
    setViewMonth(value.getMonth());
    setTab('fecha');
    setOpen(true);
  };

  const confirm = () => {
    onChange(draft);
    setOpen(false);
  };

  const selectDay = (day: number) => {
    const next = new Date(draft);
    next.setFullYear(viewYear, viewMonth, day);
    if (startOfDay(next) < minDay) return;
    setDraft(mergeDateAndTime(next, draft));
  };

  const shiftMonth = (delta: number) => {
    const d = new Date(viewYear, viewMonth + delta, 1);
    setViewYear(d.getFullYear());
    setViewMonth(d.getMonth());
  };

  const setQuickHour = (hour: number) => {
    const next = new Date(draft);
    next.setHours(hour, 0, 0, 0);
    setDraft(next);
  };

  const onTimeChange = (_: unknown, date?: Date) => {
    if (date) setDraft(mergeDateAndTime(draft, date));
  };

  return (
    <>
      <Pressable style={styles.field} onPress={openModal}>
        <View style={[styles.iconBox, { backgroundColor: accentGhost }]}>
          <Ionicons name="calendar-outline" size={22} color={accentColor} />
        </View>
        <View style={styles.fieldBody}>
          <Text style={styles.fieldLabel}>{label}</Text>
          <Text style={styles.fieldValue}>{formatDateTimeEs(value)}</Text>
        </View>
        <Ionicons name="chevron-forward" size={20} color={colors.textMuted} />
      </Pressable>

      <Modal visible={open} transparent animationType="slide" onRequestClose={() => setOpen(false)}>
        <Pressable style={styles.overlay} onPress={() => setOpen(false)}>
          <Pressable style={styles.sheet} onPress={(e) => e.stopPropagation()}>
            <View style={styles.handle} />
            <Text style={styles.sheetTitle}>{label}</Text>
            <Text style={styles.sheetPreview}>{formatDateTimeEs(draft)}</Text>

            <View style={styles.tabs}>
              <Pressable
                style={[styles.tab, tab === 'fecha' && { backgroundColor: accentColor }]}
                onPress={() => setTab('fecha')}
              >
                <Ionicons name="calendar" size={18} color={tab === 'fecha' ? colors.white : colors.textMuted} />
                <Text style={[styles.tabText, tab === 'fecha' && styles.tabTextActive]}>Calendario</Text>
              </Pressable>
              <Pressable
                style={[styles.tab, tab === 'hora' && { backgroundColor: accentColor }]}
                onPress={() => setTab('hora')}
              >
                <Ionicons name="time-outline" size={18} color={tab === 'hora' ? colors.white : colors.textMuted} />
                <Text style={[styles.tabText, tab === 'hora' && styles.tabTextActive]}>Reloj</Text>
              </Pressable>
            </View>

            {tab === 'fecha' ? (
              <View style={styles.calendar}>
                <View style={styles.monthHeader}>
                  <Pressable onPress={() => shiftMonth(-1)} style={styles.navBtn}>
                    <Ionicons name="chevron-back" size={22} color={colors.text} />
                  </Pressable>
                  <Text style={styles.monthTitle}>{MESES_TITLE[viewMonth]} {viewYear}</Text>
                  <Pressable onPress={() => shiftMonth(1)} style={styles.navBtn}>
                    <Ionicons name="chevron-forward" size={22} color={colors.text} />
                  </Pressable>
                </View>

                <View style={styles.weekRow}>
                  {WEEKDAYS.map((w) => (
                    <Text key={w} style={styles.weekDay}>{w}</Text>
                  ))}
                </View>

                <View style={styles.daysGrid}>
                  {grid.map((day, i) => {
                    if (day == null) return <View key={`e-${i}`} style={styles.dayCell} />;
                    const cellDate = new Date(viewYear, viewMonth, day);
                    const disabled = startOfDay(cellDate) < minDay;
                    const selected = isSameDay(cellDate, draft);
                    const isToday = isSameDay(cellDate, new Date());
                    return (
                      <Pressable
                        key={`d-${viewYear}-${viewMonth}-${day}`}
                        style={[
                          styles.dayCell,
                          selected && { backgroundColor: accentColor },
                          isToday && !selected && styles.dayToday,
                          disabled && styles.dayDisabled,
                        ]}
                        disabled={disabled}
                        onPress={() => selectDay(day)}
                      >
                        <Text
                          style={[
                            styles.dayText,
                            selected && styles.dayTextSelected,
                            disabled && styles.dayTextDisabled,
                          ]}
                        >
                          {day}
                        </Text>
                      </Pressable>
                    );
                  })}
                </View>
              </View>
            ) : (
              <View style={styles.timeSection}>
                <View style={styles.clockDisplay}>
                  <Ionicons name="time" size={28} color={accentColor} />
                  <Text style={styles.clockTime}>{formatTime(draft)}</Text>
                  <Text style={styles.clockDate}>{formatDateShort(draft)}</Text>
                </View>

                <Text style={styles.quickLabel}>Horarios frecuentes</Text>
                <View style={styles.quickRow}>
                  {QUICK_HOURS.map((h) => (
                    <Pressable
                      key={h}
                      style={[
                        styles.quickChip,
                        draft.getHours() === h && draft.getMinutes() === 0 && { backgroundColor: accentColor, borderColor: accentColor },
                      ]}
                      onPress={() => setQuickHour(h)}
                    >
                      <Text
                        style={[
                          styles.quickChipText,
                          draft.getHours() === h && draft.getMinutes() === 0 && { color: colors.white },
                        ]}
                      >
                        {String(h).padStart(2, '0')}:00
                      </Text>
                    </Pressable>
                  ))}
                </View>

                <View style={styles.pickerWrap}>
                  <DateTimePicker
                    value={draft}
                    mode="time"
                    display={Platform.OS === 'ios' ? 'spinner' : 'default'}
                    onChange={onTimeChange}
                    themeVariant="dark"
                    textColor={colors.text}
                  />
                </View>
              </View>
            )}

            <View style={styles.actions}>
              <Button label="Cancelar" variant="secondary" onPress={() => setOpen(false)} style={{ flex: 1 }} />
              <Button label="Confirmar" variant={accent === 'client' ? 'client' : 'primary'} onPress={confirm} style={{ flex: 1 }} />
            </View>
          </Pressable>
        </Pressable>
      </Modal>
    </>
  );
}

const styles = StyleSheet.create({
  field: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.borderLight,
    padding: spacing.md,
    marginBottom: spacing.md,
    gap: spacing.md,
  },
  iconBox: {
    width: 44,
    height: 44,
    borderRadius: radius.md,
    alignItems: 'center',
    justifyContent: 'center',
  },
  fieldBody: { flex: 1 },
  fieldLabel: { color: colors.textMuted, fontSize: 12, fontWeight: '600' },
  fieldValue: { color: colors.text, fontSize: 14, fontWeight: '600', marginTop: 4, lineHeight: 20 },
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.55)',
    justifyContent: 'flex-end',
  },
  sheet: {
    backgroundColor: colors.surfaceElevated,
    borderTopLeftRadius: radius.xl,
    borderTopRightRadius: radius.xl,
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.xxl,
    maxHeight: '92%',
  },
  handle: {
    width: 40,
    height: 4,
    backgroundColor: colors.borderLight,
    borderRadius: radius.full,
    alignSelf: 'center',
    marginTop: spacing.sm,
    marginBottom: spacing.md,
  },
  sheetTitle: { color: colors.text, fontSize: 18, fontWeight: '800' },
  sheetPreview: { color: colors.textSecondary, marginTop: 4, marginBottom: spacing.md },
  tabs: {
    flexDirection: 'row',
    backgroundColor: colors.bgSecondary,
    borderRadius: radius.md,
    padding: 4,
    marginBottom: spacing.lg,
  },
  tab: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 6,
    paddingVertical: 10,
    borderRadius: radius.sm,
  },
  tabText: { color: colors.textMuted, fontWeight: '600', fontSize: 14 },
  tabTextActive: { color: colors.white },
  calendar: { marginBottom: spacing.md },
  monthHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: spacing.md,
  },
  navBtn: {
    width: 36,
    height: 36,
    borderRadius: radius.md,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.bgSecondary,
  },
  monthTitle: { color: colors.text, fontSize: 17, fontWeight: '700' },
  weekRow: { flexDirection: 'row', marginBottom: spacing.sm },
  weekDay: {
    flex: 1,
    textAlign: 'center',
    color: colors.textMuted,
    fontSize: 12,
    fontWeight: '700',
  },
  daysGrid: { flexDirection: 'row', flexWrap: 'wrap' },
  dayCell: {
    width: '14.285714%',
    height: 44,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: radius.full,
  },
  dayToday: { borderWidth: 1, borderColor: colors.accent },
  dayDisabled: { opacity: 0.35 },
  dayText: { color: colors.text, fontSize: 15, fontWeight: '600' },
  dayTextSelected: { color: colors.white, fontWeight: '800' },
  dayTextDisabled: { color: colors.textMuted },
  timeSection: { marginBottom: spacing.md },
  clockDisplay: {
    alignItems: 'center',
    padding: spacing.lg,
    backgroundColor: colors.bgSecondary,
    borderRadius: radius.lg,
    marginBottom: spacing.md,
  },
  clockTime: { color: colors.text, fontSize: 42, fontWeight: '800', marginTop: spacing.sm },
  clockDate: { color: colors.textSecondary, marginTop: 4 },
  quickLabel: { color: colors.textSecondary, fontWeight: '600', marginBottom: spacing.sm },
  quickRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginBottom: spacing.md },
  quickChip: {
    paddingHorizontal: 14,
    paddingVertical: 8,
    borderRadius: radius.full,
    borderWidth: 1,
    borderColor: colors.borderLight,
    backgroundColor: colors.bgSecondary,
  },
  quickChipText: { color: colors.text, fontWeight: '600' },
  pickerWrap: { alignItems: 'center', overflow: 'hidden' },
  actions: { flexDirection: 'row', gap: spacing.sm, marginTop: spacing.md },
});
