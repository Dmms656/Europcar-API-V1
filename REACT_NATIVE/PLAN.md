# Plan de migración — Europcar unificado (Web + App)

> **Carpeta activa:** `REACT_NATIVE/`  
> **Backup (no tocar durante migración):** `frontend/` (web Vite) · `mobile/` (Expo original)

## Objetivo

Un solo código en **React Native + Expo** que funcione en:

| Plataforma | Comando / salida |
|------------|------------------|
| Web | `npm run web` → `expo export --platform web` |
| Android | `npm run android` / EAS Build |
| iOS | `npm run ios` / EAS Build |

Misma API: `https://europcar-api-v2.onrender.com/api/v1` (middleware).

---

## Arquitectura objetivo

```
REACT_NATIVE/
├── app/                    # Rutas (Expo Router, file-based)
│   ├── (public)/           # Home, buscar, catálogo (sin auth)
│   ├── (auth)/             # login, registro
│   ├── (tabs)/             # Cliente — tabs en móvil
│   ├── (cliente)/          # Cliente — sidebar en web (Fase 2)
│   ├── (admin)/            # Admin — tabs móvil / sidebar web (Fase 3)
│   ├── reservar/[id].tsx
│   └── reserva/[codigo].tsx
├── src/
│   ├── api/                # Todos los módulos API (14)
│   ├── components/
│   │   ├── layout/         # WebShell, Sidebar, Navbar
│   │   ├── ui/             # Button, Input, Card, Screen…
│   │   └── domain/         # VehiculoCard, ReservaCard…
│   ├── config/             # API URL, feature flags
│   ├── hooks/              # useBreakpoint, usePlatform
│   ├── store/              # Zustand (auth, ui)
│   ├── theme/              # colors, spacing, typography
│   └── utils/              # storage, errors, dates
├── assets/
├── app.json
├── eas.json
└── PLAN.md                 # Este documento
```

### Principios

1. **Un componente, tres plataformas** — `View`, `Text`, `Pressable` (+ `react-native-web`).
2. **Layouts adaptativos** — tabs en móvil; sidebar + navbar en web (`useBreakpoint`, `Platform.OS`).
3. **Storage cross-platform** — `SecureStore` (nativo) / `localStorage` (web).
4. **TypeScript** — tipado estricto en código nuevo; migrar JSX del web a `.tsx`.
5. **No romper backups** — `frontend/` y `mobile/` quedan como referencia hasta Fase 4.

---

## Mapa de rutas: Web actual → Expo Router

### Público

| Web (`frontend/`) | REACT_NATIVE (objetivo) | Estado |
|-------------------|-------------------------|--------|
| `/` | `/(tabs)/` o `/(public)/` | ✅ Parcial (tabs) |
| `/buscar` | `/(tabs)/buscar` | ✅ |
| `/catalogo` | `/(tabs)/catalogo` | ✅ |
| `/login` | `/(auth)/login` | ✅ |
| `/registro` | `/(auth)/register` | ✅ |
| `/reservar/:id` | `/reservar/[id]` | ✅ |
| — | `/reserva/[codigo]` | ✅ (solo mobile) |

### Portal cliente

| Web | REACT_NATIVE | Estado |
|-----|--------------|--------|
| `/mi-cuenta` | `/(tabs)/cuenta` | ✅ |
| `/mis-reservas` | `/(tabs)/reservas` | ✅ |
| `/historial` | `/(tabs)/historial` | ✅ |
| `/mis-contratos` | `/(cliente)/contratos` | ❌ Fase 2 |
| `/mis-facturas` | `/(cliente)/facturas` | ❌ Fase 2 |

### Admin

| Web | REACT_NATIVE | Estado |
|-----|--------------|--------|
| `/dashboard` | `/(admin)/` | ✅ |
| `/clientes` | `/(admin)/clientes` | ✅ |
| `/vehiculos` | `/(admin)/vehiculos` | ✅ |
| `/reservas` | `/(admin)/reservas` | ✅ |
| `/contratos` | `/(admin)/contratos` | ❌ Fase 3 |
| `/pagos` | `/(admin)/pagos` | ❌ Fase 3 |
| `/mantenimientos` | `/(admin)/mantenimientos` | ❌ Fase 3 |
| `/localizaciones` | `/(admin)/localizaciones` | ❌ Fase 3 |
| `/ubicaciones` | `/(admin)/ubicaciones` | ❌ Fase 3 |
| `/extras` | `/(admin)/extras` | ❌ Fase 3 |
| `/usuarios` | `/(admin)/usuarios` | ❌ Fase 3 |

---

## Mapa de API: módulos a unificar

| Módulo | `frontend/src/api/` | `REACT_NATIVE/src/api/` | Fase |
|--------|---------------------|-------------------------|------|
| axiosClient | ✅ axiosClient.js | ✅ axiosClient.ts | 1 (mejorar) |
| authApi | ✅ | ✅ | 1 |
| bookingApi | ✅ | ✅ | 1 |
| reservasApi | ✅ | ✅ | 1 |
| adminApi | — (disperso) | ✅ parcial | 2–3 |
| clientesApi | ✅ | → adminApi / nuevo | 3 |
| vehiculosApi | ✅ | → adminApi / nuevo | 3 |
| contratosApi | ✅ | ❌ | 3 |
| pagosApi | ✅ | ❌ | 3 |
| facturasApi | ✅ | ❌ | 2 |
| mantenimientosApi | ✅ | ❌ | 3 |
| localizacionesApi | ✅ | ❌ | 3 |
| catalogosApi | ✅ | ❌ | 3 |
| usuariosApi | ✅ | ❌ | 3 |
| conductoresApi | ✅ | ❌ | 3 |

---

## Fases detalladas

### Fase 1 — Base cross-platform ✅ (en curso)

**Objetivo:** Proyecto Expo funcional en web + móvil con auth y layouts preparados.

| Tarea | Archivo(s) | Criterio de éxito |
|-------|------------|-------------------|
| Carpeta `REACT_NATIVE` desde `mobile` | raíz | Proyecto arranca sin errores |
| Storage unificado | `src/utils/storage.ts` | Login persiste en web y nativo |
| Auth store actualizado | `src/store/useAuthStore.ts` | Misma lógica que web (roles, tipos) |
| Config API web | `src/config/api.ts`, `.env.example` | Dev apunta a middleware local |
| CORS middleware | `Middleware.RedCar/.../appsettings.json` | `localhost:8081` permitido |
| Hook breakpoints | `src/hooks/useBreakpoint.ts` | `isWeb`, `isDesktop`, `isMobile` |
| Layout web base | `src/components/layout/WebShell.tsx` | Sidebar placeholder en desktop |
| Navbar pública web | `src/components/layout/PublicNavbar.tsx` | Links home/buscar/catálogo |
| Axios mejorado | `src/api/axiosClient.ts` | `withCredentials` en web, 401 limpio |
| Documentación | `README.md`, `PLAN.md` | Equipo sabe cómo arrancar |

**Comandos de verificación Fase 1:**

```bash
cd REACT_NATIVE
npm install
npm run web          # http://localhost:8081
npm start            # Expo Go
npm run typecheck
```

---

### Fase 2 — Portal cliente completo

**Objetivo:** Paridad del área cliente con `frontend/`.

| Tarea | Origen | Notas |
|-------|--------|-------|
| Mis contratos | `MisContratosPage.jsx` | `GET /contratos/cliente/{id}` |
| Mis facturas | `MisFacturasPage.jsx` | `facturasApi.ts` nuevo |
| Mejorar cuenta | `MiCuentaPage.jsx` | Editar perfil, refresh |
| Layout cliente web | `ClienteLayout.jsx` | Sidebar en `isDesktop` |
| Grupo rutas `(cliente)/` | — | Web sidebar; móvil sigue en tabs |
| `facturasApi.ts` | `facturasApi.js` | Port a TS |
| `contratosApi.ts` | `contratosApi.js` | Port a TS |
| Toasts / errores | `errorHandler.js`, sonner | `react-native-toast-message` o similar |

**Entregable:** Cliente puede ver contratos y facturas en web y app.

---

### Fase 3 — Panel admin completo

**Objetivo:** Las 11 pantallas admin del web en REACT_NATIVE.

**Orden sugerido (de más simple a más compleja):**

1. Extras — CRUD simple
2. Ubicaciones (países/ciudades) — catálogos
3. Localizaciones — mapa de sucursales
4. Pagos — listado + filtros
5. Contratos — listado + detalle
6. Mantenimientos — CRUD + vehículo
7. Usuarios — solo ADMIN, roles

**Por pantalla (plantilla de migración):**

```
1. Copiar lógica de frontend/src/pages/{modulo}/
2. Crear src/api/{modulo}Api.ts
3. Crear app/(admin)/{modulo}.tsx
4. Extraer formularios a src/components/domain/
5. Tabla web: FlatList + header fijo; web ancho: ScrollView horizontal
6. Probar en web (desktop) y móvil (lista compacta)
```

**Componentes compartidos a crear:**

- `DataTable` — lista con paginación (`PaginationControls`)
- `FormField` — Input + label + error
- `ImageUploader` — `expo-image-picker` + Cloudinary (web: input file)
- `DateTimePicker` — unificar `DateTimeSelector` + web picker
- `ConfirmDialog` — `Alert` nativo / modal web
- `RoleGuard` — equivalente a `ProtectedRoute`

**Layout admin web:**

- Sidebar con 11 ítems (como `MainLayout.jsx`)
- Contenido en `WebShell` con `maxWidth` opcional
- Móvil: mantener tabs actuales + menú "Más" para pantallas extra

---

### Fase 4 — Producción y retiro de legacy

| Tarea | Detalle |
|-------|---------|
| Build web estático | `npx expo export --platform web` → hosting (Render, Vercel, etc.) |
| CI/CD | GitHub Action: web export + EAS Update |
| CORS producción | Añadir URL del nuevo frontend en middleware |
| Variables entorno | `EXPO_PUBLIC_API_URL`, Cloudinary, EAS secrets |
| SEO web mínimo | `app/+html.tsx`, meta tags, título por ruta |
| Deprecar `frontend/` | README en raíz apuntando a REACT_NATIVE |
| Deprecar `mobile/` | Mantener solo como backup histórico |
| Dominio | Reemplazar `europcar-frontend.onrender.com` |

---

## Decisiones técnicas

| Tema | Decisión | Alternativa descartada |
|------|----------|------------------------|
| Framework | Expo 56 + RN 0.85 | Reescribir web en Vite separado |
| Routing | Expo Router | React Navigation manual |
| Estado servidor | `useEffect` + axios (Fase 1–3) | TanStack Query (Fase 5 opcional) |
| Estilos | StyleSheet + theme tokens | NativeWind (evaluar en Fase 3) |
| Auth token web | localStorage vía `storage.ts` | Solo cookies (no funciona en RN nativo) |
| Auth token nativo | SecureStore | AsyncStorage (menos seguro) |
| Iconos | `@expo/vector-icons` | lucide-react-native |
| Formularios | estado local + validación manual | react-hook-form (portar en Fase 3) |

---

## Riesgos y mitigaciones

| Riesgo | Mitigación |
|--------|------------|
| Tablas admin complejas en móvil | Vista compacta (cards) en `< md`; tabla en web |
| CORS en dev web (puerto 8081) | Añadir origen en middleware `appsettings.json` |
| SecureStore falla en web | Abstracción `storage.ts` con fallback |
| Paridad visual con web CSS | Tokens en `src/theme/colors.ts` ya alineados |
| Subida imágenes Cloudinary | `expo-image-picker` + API web con `<input type="file">` |
| Push solo nativo | Condicionar `registerForPushNotifications` con `Platform.OS !== 'web'` |

---

## Cronograma estimado

| Fase | Duración | Dependencias |
|------|----------|--------------|
| 1 — Base | 1–2 días | — |
| 2 — Cliente | 2–3 días | Fase 1 |
| 3 — Admin | 1–2 semanas | Fase 1 |
| 4 — Producción | 1–2 días | Fases 2–3 |

---

## Checklist global

### Fase 1
- [x] Crear `REACT_NATIVE/` desde `mobile/`
- [x] `PLAN.md` y `README.md`
- [x] `src/utils/storage.ts`
- [x] Auth store cross-platform
- [x] `useBreakpoint` + layouts base web
- [x] CORS `localhost:8081`
- [ ] Verificar `npm run web` + login end-to-end

### Fase 2
- [x] `facturasApi.ts`, `contratosApi.ts`
- [x] Pantallas mis-contratos, mis-facturas
- [x] Layout cliente web con sidebar (`ClienteSidebar` + `ClienteWebLayout`)
- [x] Mejoras en Mi Cuenta (validación, accesos rápidos móvil)
- [x] Componentes: `PaginationControls`, `EmptyState`, `StatusBadge`, `Modal`

### Fase 3
- [x] 7 pantallas admin faltantes (contratos, pagos, mantenimientos, localizaciones, ubicaciones, extras, usuarios)
- [x] 6 módulos API nuevos (pagos, mantenimientos, localizaciones, catalogos, usuarios + contratos admin)
- [x] AdminSidebar + AdminWebLayout
- [x] Tab "Más" en móvil para módulos admin
- [x] hasAnyRole / hasRole en auth store

### Fase 4
- [ ] Export web + deploy
- [ ] CI/CD
- [ ] Deprecar `frontend/` y `mobile/`

---

## Referencias internas

- Web backup: `../frontend/src/`
- Mobile backup: `../mobile/`
- API middleware: `../Middleware.RedCar/`
- Paleta CSS original: `../frontend/src/index.css`
- Expo docs v56: https://docs.expo.dev/versions/v56.0.0/
