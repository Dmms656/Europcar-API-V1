# Servicio EUROPCAR V1

Plataforma de renta de vehiculos con arquitectura full-stack:

- `Backend` en ASP.NET Core (`net10.0`) con API versionada, JWT y Swagger.
- `Frontend` en React + Vite para experiencia web de cliente y operacion interna.
- `Base de datos` relacional con entidades de seguridad, flota, reservas, pagos y auditoria.

Este proyecto implementa el dominio de negocio descrito en `Contexto/EUROPCAR.docx`, con foco en inventario en tiempo real, motor de reservas e integracion con canales externos tipo OTA.

## Objetivo del sistema

Permitir la gestion integral del negocio de alquiler de autos:

- Consulta de disponibilidad por fechas y localizacion.
- Reserva, confirmacion, pago y seguimiento de contratos.
- Administracion de flota, clientes, mantenimientos, catalogos y localizaciones.
- Exposicion de endpoints publicos para integraciones (ej. Booking/OTA).

## Arquitectura del proyecto

Estructura principal:

- `src/Europcar.Rental.Api`: capa de presentacion (controllers, middleware, autenticacion, versionado, health checks).
- `src/Europcar.Rental.Business`: logica de negocio, DTOs, validaciones, servicios y excepciones.
- `src/Europcar.Rental.DataAccess`: entidades, configuraciones EF Core, contexto y repositorios.
- `src/Europcar.Rental.DataManagement`: servicios de datos complementarios usados por la API.
- `frontend`: aplicacion React (rutas, paginas, consumo de API, estado global).
- `scripts`: utilitarios de apoyo para el entorno.
- `Contexto`: documentacion academica/funcional del proyecto.

## Modulos funcionales implementados

Segun el repositorio y el contexto funcional, el sistema cubre:

- Autenticacion y gestion de usuarios/roles.
- Gestion de vehiculos y catalogos.
- Gestion de clientes.
- Gestion de reservas.
- Gestion de contratos.
- Gestion de pagos y facturas.
- Gestion de localizaciones y mantenimientos.
- Endpoints de integracion Booking/OTA para consulta de vehiculos y disponibilidad.

## API (resumen)

Base local por defecto:

- `http://localhost:5207`
- Swagger: `http://localhost:5207/swagger`
- Health checks:
  - `GET /health/live`
  - `GET /health/ready`

Convencion de versionado:

- `api/v{version}/...` (actualmente `v1`)

Grupos de endpoints destacados:

- `Auth`: `api/v1/Auth/login`, `api/v1/Auth/register`
- `Admin interno`: rutas bajo `api/v1/admin/...` (ej. vehiculos)
- `Booking publico`: rutas bajo `api/v1/vehiculos` y recursos de reservas para integracion externa

La API incluye autenticacion JWT y control de autorizacion por roles como `ADMIN`, `AGENTE_POS` y `CLIENTE_WEB`.

## Requisitos previos

- .NET SDK 10
- Node.js 20+ y npm
- PostgreSQL (o instancia compatible con la cadena de conexion configurada)

## Configuracion

Archivo principal backend:

- `src/Europcar.Rental.Api/appsettings.json`

Variables/valores clave:

- `ConnectionStrings:RentalDb`
- `JwtSettings:SecretKey`
- `JwtSettings:Issuer`
- `JwtSettings:Audience`
- `JwtSettings:ExpirationMinutes`

> Recomendacion: mover secretos reales a variables de entorno o User Secrets y evitar credenciales en repositorio.

## Ejecucion en desarrollo

### 1) Backend

Desde la raiz del repo:

```bash
dotnet restore
dotnet run --project src/Europcar.Rental.Api
```

La API inicializa seed basico de usuarios de desarrollo cuando corresponde.

### 2) Frontend

Desde `frontend`:

```bash
npm ci
npm run dev
```

Por defecto Vite levanta en `http://localhost:5173` (si ese puerto esta libre).

## Frontend (tecnologias)

- React 19
- Vite 8
- React Router
- React Query
- Axios
- Zustand
- React Hook Form + Zod

## Entidades de dominio (alto nivel)

El modelo de datos contempla, entre otras:

- Seguridad: usuarios, roles, permisos, sesiones.
- Operacion rental: clientes, vehiculos, categorias, marcas, localizaciones.
- Transaccional: reservas, contratos, pagos, facturas.
- Soporte operativo: mantenimientos, extras, conductores asociados.
- Auditoria: eventos e intentos de login.

## Requerimientos no funcionales (referencia de contexto)

Objetivos de calidad definidos en el documento de contexto:

- Alta disponibilidad del servicio.
- Baja latencia para consultas de disponibilidad.
- Seguridad de transporte y control de acceso.
- Modularidad para evolucion futura (incluida convivencia de versiones de API).
- Escalabilidad del inventario y operaciones.

## Despliegue (frontend estatico)

Existe `render.yaml` para despliegue del frontend en Render con:

- Build: `npm ci && npm run build`
- Publicacion: `frontend/dist`
- Regla de rewrite SPA: `/* -> /index.html`

## Estado y siguientes mejoras sugeridas

- Agregar diagramas de arquitectura y ER en `docs/`.
- Documentar contrato OpenAPI exportado por entorno.
- Incorporar coleccion de pruebas de API (Postman/Bruno).
- Automatizar CI para pruebas unitarias/integracion.
- Estandarizar variables de entorno entre local, staging y produccion.

## Licencia

Definir segun lineamientos del equipo/proyecto academico.
