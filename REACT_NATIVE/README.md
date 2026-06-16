# Europcar — App unificada (Web + iOS + Android)

Código único en **React Native + Expo** que reemplazará progresivamente:

- `../frontend/` — web React + Vite (backup)
- `../mobile/` — app Expo original (backup)

Plan completo de migración: **[PLAN.md](./PLAN.md)**

## Requisitos

- Node.js 20+
- Para móvil: [Expo Go](https://expo.dev/go) o EAS Build
- Backend: middleware en `http://localhost:5200` (dev) o Render (prod)

## Arranque rápido

```bash
cd REACT_NATIVE
npm install
npm run web      # Web → http://localhost:8081
npm start        # QR para Expo Go (Android/iOS)
```

## Variables de entorno

Copia `.env.example` → `.env`:

```env
# Desarrollo local (middleware en puerto 5200)
EXPO_PUBLIC_API_URL=http://localhost:5200/api/v1

# Producción (por defecto en app.json → extra.apiUrl)
# EXPO_PUBLIC_API_URL=https://europcar-api-v2.onrender.com/api/v1
```

## Estructura

```
app/           → Pantallas (Expo Router)
src/api/       → Cliente HTTP y endpoints
src/components → UI y layouts
src/store/     → Zustand (auth)
src/theme/     → Colores y espaciado (alineados con web)
src/utils/     → storage cross-platform, helpers
```

## Fase actual: 3 — Panel admin completo ✅

- 11 módulos admin (dashboard + 10 secciones)
- Sidebar admin en web desktop
- Tab "Más" en móvil para acceder a todos los módulos

## Scripts

| Comando | Descripción |
|---------|-------------|
| `npm start` | Expo dev server |
| `npm run web` | Solo web |
| `npm run android` | Build nativo Android |
| `npm run ios` | Build nativo iOS |
| `npm run typecheck` | TypeScript |
| `npm run build:android` | EAS APK preview |

## API

Misma base que el frontend web:

`https://europcar-api-v2.onrender.com/api/v1`

Autenticación: `Authorization: Bearer {token}` (+ cookies en web si el middleware las envía).
