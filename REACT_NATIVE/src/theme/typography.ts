export const fonts = {
  regular: 'Inter_400Regular',
  medium: 'Inter_500Medium',
  semiBold: 'Inter_600SemiBold',
  bold: 'Inter_700Bold',
  extraBold: 'Inter_800ExtraBold',
} as const;

export const text = {
  heroTitle: { fontFamily: fonts.extraBold, fontSize: 32, letterSpacing: -0.5 },
  h1: { fontFamily: fonts.extraBold, fontSize: 26 },
  h2: { fontFamily: fonts.bold, fontSize: 20 },
  body: { fontFamily: fonts.regular, fontSize: 15, lineHeight: 22 },
  caption: { fontFamily: fonts.medium, fontSize: 12 },
  label: { fontFamily: fonts.semiBold, fontSize: 13 },
} as const;
