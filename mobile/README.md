# Europcar Mobile (React Native + Expo)



App híbrida iOS/Android que consume la **misma API** que el frontend web en Render:



`https://europcar-api-v2.onrender.com/api/v1`



## Requisitos



- Node.js 20+

- [Expo Go](https://expo.dev/go) en el teléfono (desarrollo) o cuenta [Expo](https://expo.dev) para EAS Build



## Arranque



```bash

cd mobile

npm install

npm start

```



Escanea el QR con Expo Go (Android/iOS).



## Configuración API



Por defecto apunta a **Render** (`app.json` → `extra.apiUrl`).



Para override:



```env

EXPO_PUBLIC_API_URL=https://europcar-api-v2.onrender.com/api/v1

```



## Pantallas



| Ruta | Función |

|------|---------|

| `/(tabs)` | Inicio, buscar, mis reservas, historial, cuenta |

| `/login` | Auth JWT (SecureStore) |

| `/register` | Registro cliente nuevo o vincular cliente existente |

| `/reservar/[id]` | Crear reserva → `POST /api/v1/reservas` |

| `/reserva/[codigo]` | Detalle por código (público o admin) |



### Mis reservas / Historial



- Activas: `GET /admin/Reservas/cliente/{idCliente}` + filtro `isReservaActiva`

- Historial: misma API + filtro `isReservaHistorica`

- Cancelar: `PUT /admin/Reservas/{id}/cancelar` con motivo



## Push notifications



Tras login, la app solicita permiso y guarda el **Expo Push Token** en SecureStore.



El backend aún no expone endpoint para registrar tokens; cuando exista, enviar `getStoredPushToken()` al API.



## Expo + GitHub (builds y actualizaciones)

### Por qué no conecta hoy

La carpeta `mobile/` **aún no está en GitHub** (no commiteada). Expo solo puede enlazar repos que existan en tu cuenta de GitHub.

### Paso 1 — Subir `mobile/` al repo

Desde la raíz del proyecto:

```bash
git add mobile/ .github/workflows/mobile-eas-update.yml
git commit -m "feat: app móvil Expo + EAS Update"
git push origin main
```

Repo actual: `https://github.com/Dmms656/Europcar-API-V1.git`

### Paso 2 — Conectar GitHub en Expo (dashboard)

1. https://expo.dev/settings → **Connections** → **GitHub** → **Connect** → instalar la app en GitHub.
2. Proyecto **europcar** → **Project settings** → **GitHub** → conectar `Europcar-API-V1`.
3. **Base directory:** `mobile` (importante: la app no está en la raíz del repo).
4. Guardar.

Tras esto puedes lanzar builds desde expo.dev o con comentarios en commits/PRs.

### Paso 3 — Actualizaciones OTA sin nuevo APK (EAS Update)

| Qué | Cómo |
|-----|------|
| Cambios JS/UI | `cd mobile` → `npm run update:preview` |
| Automático al push a `main` | Secret `EXPO_TOKEN` en GitHub → Actions |

Crear token: https://expo.dev/settings → **Access Tokens** → copiar y en GitHub: **Settings → Secrets → Actions → New repository secret** → nombre `EXPO_TOKEN`.

**Importante:** el APK debe compilarse **después** de añadir `expo-updates` (un build nuevo con `npx eas build ...`). Luego los cambios de código se publican con `eas update` sin reinstalar el APK.

### Dos formas de “actualizar”

1. **EAS Update (OTA):** pantallas, textos, lógica JS — minutos, sin nuevo APK.
2. **EAS Build:** cambios nativos (plugins, permisos, SDK) — nuevo APK/AAB.

## Build producción (EAS)



```bash

npm install -g eas-cli

eas login

eas build:configure   # solo la primera vez

npm run build:android   # APK preview (internal)

npm run build:ios

```



Perfiles en `eas.json`: `development`, `preview` (APK Android), `production`.



## Backend



No requiere CORS en app **nativa** (HTTP + Bearer JWT). El panel admin completo sigue en la web.

