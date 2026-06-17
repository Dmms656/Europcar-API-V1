import HomeScreen from '@/src/screens/HomeScreen';
import { WebShell } from '@/src/components/layout/WebShell';

/** Home web en `/` — fuera del grupo (tabs) para evitar crash de Slot + estilos en array. */
export default function WebHomePage() {
  return (
    <WebShell padded={false} maxWidth={9999}>
      <HomeScreen />
    </WebShell>
  );
}
