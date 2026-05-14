# Middleware.RedCar

API publica V2 que implementa **estrictamente** el contrato definido en
`Contexto/Contrato_API_Vehiculos_RedCar_V2_CORREGIDO.docx`.

Actua como BFF entre el canal web/Booking y los microservicios internos de
`EUROPCAR_V2`. Expone el contrato **V2** (`/api/v2/booking/...`) y rutas **V1**
compatibles con el SPA (`/api/v1/...`, envelope tipo monolito).

---

## 1. Arquitectura

```
Booking externo
      |
      | HTTPS + Bearer Token
      v
+-----------------------------+
| Middleware.RedCar.Api       |  <-  Controllers REST V2 (este proyecto)
+-----------------------------+
              |
              v
+-----------------------------+
| Middleware.RedCar.Business  |  <-  Orchestrators (Marketplace, Reserva, Factura)
+-----------------------------+
              |
              v
+--------------------------------+
| Middleware.RedCar.DataManagement | <- Services + Models internos + Mappers
+--------------------------------+
              |
              v
+--------------------------------+
| Middleware.RedCar.DataAccess     | <- HttpClients (Refit-less) + gRPC client
+--------------------------------+
              |
   REST  |   |   |   |   gRPC
         v   v   v   v
   MS.Seguridad  MS.Catalogo  MS.Localizaciones  MS.Clientes  MS.Reservas
```

---

## 2. Endpoints expuestos (segun contrato)

| # | Metodo | Path | Orchestrator |
|---|--------|------|--------------|
| 1 | GET   | `/api/v2/booking/vehiculos` (filtros, paginacion)       | Marketplace |
| 2 | GET   | `/api/v2/booking/vehiculos/{idVehiculo}`                | Marketplace |
| 3 | GET   | `/api/v2/booking/reservas/{idVehiculo}/disponibilidad`  | Reserva     |
| 4 | GET   | `/api/v2/booking/localizaciones`                        | Marketplace |
| 5 | GET   | `/api/v2/booking/localizaciones/{idLocalizacion}`       | Marketplace |
| 6 | GET   | `/api/v2/booking/categorias`                            | Marketplace |
| 7 | GET   | `/api/v2/booking/extras`                                | Marketplace |
| 8 | POST  | `/api/v2/booking/reservas`                              | Reserva     |
| 9 | GET   | `/api/v2/booking/reservas/{codigoReserva}`              | Reserva     |
| 10| PATCH | `/api/v2/booking/reservas/{codigoReserva}/cancelar`     | Reserva     |
| 11| GET   | `/api/v2/booking/reservas/{codigoReserva}/factura`      | Factura     |

Todas las respuestas siguen el wrapper del contrato:
```json
{ "status": 200, "mensaje": "Operación exitosa", "data": { ... } }
```

Los objetos HATEOAS usan la clave **`_links`** (con `href` en cada enlace). El enlace
`disponibilidad` de cada vehículo apunta a **`/api/v2/booking/reservas/{idVehiculo}/disponibilidad`**
(tabla del Endpoint 3 del contrato), no a `/vehiculos/.../disponibilidad`.

### Contraseñas de base de datos (`ms_*`)

Las contraseñas de los roles PostgreSQL van en **`EUROPCAR_V2/.env`** (junto a las
`ConnectionStrings__Default__*`), que está en **`.gitignore`**. No las pongas en
`Middleware.RedCar` salvo que un día el BFF acceda directo a la BD (hoy no: solo llama a los MS).

El middleware solo necesita **`Middleware.RedCar/.env`**: URLs de los cinco MS y
**el mismo `JWT__SecretKey`** que `EUROPCAR_V2/.env` para validar el Bearer que
reenvía a los microservicios.

## 3. Estructura de carpetas

```
Middleware.RedCar
├── src
│   ├── Middleware.RedCar.DataAccess        <-  HttpClients y gRPC clients
│   ├── Middleware.RedCar.DataManagement    <-  Services internos + DataModels
│   ├── Middleware.RedCar.Business          <-  Orchestrators + DTOs publicos + Validators
│   └── Middleware.RedCar.Api               <-  Controllers V2 + middlewares + JWT
└── tests
```

---

## 4. Configuracion

Copia `.env.example` a `.env` (gitignored) y rellena los valores. En
desarrollo local los MS se levantan en los puertos 5101-5105; en produccion
se sustituyen por las URLs publicas.

Para arrancar:

```powershell
# 1. levantar los 5 MS (otra terminal)
pwsh ./EUROPCAR_V2/scripts/run-all.ps1

# 2. levantar el middleware
dotnet run --project Middleware.RedCar/src/Middleware.RedCar.Api
# escucha en http://localhost:5200 (ver launchSettings.json)
```

Documentación interactiva: **`/swagger`** (OpenAPI `v1` compat frontend + `v2` contrato; Bearer donde aplique).

Smoke test (V2):
```powershell
curl http://localhost:5200/api/v2/booking/categorias -H "Authorization: Bearer <token>"
```

---

## 5. Estado actual y proximos pasos

- [x] **Estructura completa** con los 4 proyectos (DataAccess, DataManagement, Business, Api).
- [x] **Los 11 endpoints del contrato** declarados con la forma exacta de request/response.
- [x] **Wrapper de respuesta** `{ status, mensaje, data }` aplicado en todos los endpoints.
- [x] **Validators FluentValidation** para `CrearReserva` y `CancelarReserva`.
- [x] **HttpClients tipados** a los 5 MS con JWT pass-through.
- [x] **gRPC client** hacia MS.Reservas para crear/cancelar reserva (operacion transaccional).
- [ ] **Implementacion completa de los MS** (Fase 2 de EUROPCAR_V2). Mientras eso no exista,
      los HttpClients haran 404 y el middleware respondera 502 al canal de Booking.
