import { Platform, StyleSheet, Text, View } from 'react-native';
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

const webSelectStyle = {
  width: '100%',
  minHeight: 48,
  padding: '12px 14px',
  fontSize: 15,
  fontFamily: fonts.regular,
  color: colors.text,
  backgroundColor: colors.surface,
  border: `1px solid ${colors.borderLight}`,
  borderRadius: radius.md,
  outline: 'none',
  cursor: 'pointer',
} as const;

export function Select({ label, value, onValueChange, options, placeholder = 'Seleccionar' }: Props) {
  if (Platform.OS === 'web') {
    return (
      <View style={styles.wrap}>
        {label ? <Text style={styles.label}>{label}</Text> : null}
        <select
          value={value}
          onChange={(e) => onValueChange(e.target.value)}
          style={webSelectStyle}
        >
          <option value="">{placeholder}</option>
          {options.map((o) => (
            <option key={o.value || o.label} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
      </View>
    );
  }

  const { Picker } = require('@react-native-picker/picker') as typeof import('@react-native-picker/picker');

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
  pickerWrap: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.borderLight,
    borderRadius: radius.md,
    overflow: 'hidden',
    minHeight: 48,
    justifyContent: 'center',
  },
  picker: {
    color: colors.text,
  },
  iosItem: { color: colors.text, fontSize: 15 },
});
