import { Redirect } from 'expo-router';

/** Nativo: la home vive en (tabs)/index. Web usa index.web.tsx. */
export default function RootIndex() {
  return <Redirect href="/(tabs)" />;
}
