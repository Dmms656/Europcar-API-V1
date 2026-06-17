import { useState } from 'react';
import {
  Modal,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '@/src/theme/colors';
import { radius, spacing } from '@/src/theme/layout';
import { fonts } from '@/src/theme/typography';

export type SelectOption = { label: string; value: string };

type Props = {
  label?: string;
  value: string;
  onValueChange: (v: string) => void;
  options: SelectOption[];
  placeholder?: string;
};

function WebSelect({ label, value, onValueChange, options, placeholder = 'Seleccionar' }: Props) {
  const [open, setOpen] = useState(false);
  const selected = options.find((o) => o.value === value);

  const pick = (v: string) => {
    onValueChange(v);
    setOpen(false);
  };

  return (
    <View style={styles.wrap}>
      {label ? <Text style={styles.label}>{label}</Text> : null}
      <Pressable style={styles.trigger} onPress={() => setOpen(true)}>
        <Text style={[styles.triggerText, !selected && styles.placeholder]}>
          {selected?.label ?? placeholder}
        </Text>
        <Ionicons name="chevron-down" size={18} color={colors.textMuted} />
      </Pressable>

      <Modal visible={open} transparent animationType="fade" onRequestClose={() => setOpen(false)}>
        <Pressable style={styles.overlay} onPress={() => setOpen(false)}>
          <Pressable style={styles.sheet} onPress={(e) => e.stopPropagation()}>
            <Text style={styles.sheetTitle}>{label ?? placeholder}</Text>
            <ScrollView style={styles.list}>
              <Pressable style={styles.option} onPress={() => pick('')}>
                <Text style={styles.optionText}>{placeholder}</Text>
              </Pressable>
              {options.map((o) => (
                <Pressable
                  key={o.value || o.label}
                  style={[styles.option, o.value === value && styles.optionActive]}
                  onPress={() => pick(o.value)}
                >
                  <Text style={[styles.optionText, o.value === value && styles.optionTextActive]}>
                    {o.label}
                  </Text>
                </Pressable>
              ))}
            </ScrollView>
          </Pressable>
        </Pressable>
      </Modal>
    </View>
  );
}

export function Select(props: Props) {
  if (Platform.OS === 'web') {
    return <WebSelect {...props} />;
  }

  const { Picker } = require('@react-native-picker/picker') as typeof import('@react-native-picker/picker');
  const { label, value, onValueChange, options, placeholder = 'Seleccionar' } = props;

  return (
    <View style={styles.wrap}>
      {label ? <Text style={styles.label}>{label}</Text> : null}
      <View style={styles.pickerWrap}>
        <Picker
          selectedValue={value}
          onValueChange={onValueChange}
          style={styles.picker}
          dropdownIconColor={colors.textMuted}
          itemStyle={Platform.OS === 'ios' ? styles.iosItem : undefined}
        >
          <Picker.Item label={placeholder} value="" color={colors.textMuted} />
          {options.map((o) => (
            <Picker.Item key={o.value} label={o.label} value={o.value} color={colors.text} />
          ))}
        </Picker>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: { marginBottom: spacing.md },
  label: { color: colors.textSecondary, fontSize: 13, marginBottom: 6, fontFamily: fonts.semiBold },
  trigger: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.borderLight,
    borderRadius: radius.md,
    minHeight: 48,
    paddingHorizontal: spacing.md,
    paddingVertical: 12,
  },
  triggerText: { color: colors.text, fontFamily: fonts.regular, fontSize: 15, flex: 1 },
  placeholder: { color: colors.textMuted },
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.55)',
    justifyContent: 'center',
    padding: spacing.lg,
  },
  sheet: {
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    maxHeight: '70%',
    padding: spacing.lg,
  },
  sheetTitle: { color: colors.text, fontFamily: fonts.bold, fontSize: 17, marginBottom: spacing.md },
  list: { maxHeight: 360 },
  option: {
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.sm,
    borderRadius: radius.sm,
  },
  optionActive: { backgroundColor: colors.primaryGhost },
  optionText: { color: colors.text, fontFamily: fonts.regular, fontSize: 15 },
  optionTextActive: { color: colors.primaryLight, fontFamily: fonts.semiBold },
  pickerWrap: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.borderLight,
    borderRadius: radius.md,
    overflow: 'hidden',
    minHeight: 48,
    justifyContent: 'center',
  },
  picker: { color: colors.text },
  iosItem: { color: colors.text, fontSize: 15 },
});
