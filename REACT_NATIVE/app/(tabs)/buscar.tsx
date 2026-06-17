import { useEffect } from 'react';
import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { colors } from '@/src/theme/colors';

/** Redirige al catálogo completo — las fechas se eligen al reservar. */
export default function BuscarScreen() {
  const params = useLocalSearchParams<{
    idLocalizacion?: string;
    localizacion?: string;
    pais?: string;
    ciudad?: string;
    categoria?: string;
  }>();

  useEffect(() => {
    const q = new URLSearchParams();
    const loc = params.localizacion ?? params.idLocalizacion;
    if (loc) q.set('localizacion', String(loc));
    if (params.pais) q.set('pais', String(params.pais));
    if (params.ciudad) q.set('ciudad', String(params.ciudad));
    if (params.categoria) q.set('categoria', String(params.categoria));
    const qs = q.toString();
    router.replace(qs ? `/catalogo?${qs}` : '/catalogo');
  }, [params]);

  return (
    <View style={styles.center}>
      <ActivityIndicator color={colors.primary} size="large" />
    </View>
  );
}

const styles = StyleSheet.create({
  center: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: colors.bg },
});
