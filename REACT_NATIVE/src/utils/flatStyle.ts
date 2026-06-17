import { StyleSheet, type StyleProp, type ViewStyle, type TextStyle, type ImageStyle } from 'react-native';

/** Evita crash en web (CSSStyleDeclaration) con arrays de estilo condicionales. */
export function flatStyle(style: StyleProp<ViewStyle>): ViewStyle | undefined;
export function flatStyle(style: StyleProp<TextStyle>): TextStyle | undefined;
export function flatStyle(style: StyleProp<ImageStyle>): ImageStyle | undefined;
export function flatStyle(style: StyleProp<ViewStyle | TextStyle | ImageStyle>) {
  return style == null ? undefined : StyleSheet.flatten(style);
}
