import { Platform } from 'react-native';
import CatalogWebScreen from '@/src/screens/web/CatalogWebScreen';
import CatalogoMobileScreen from '@/src/screens/CatalogoMobileScreen';

export default function CatalogoScreen() {
  if (Platform.OS === 'web') {
    return <CatalogWebScreen />;
  }
  return <CatalogoMobileScreen />;
}
