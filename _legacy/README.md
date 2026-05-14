# Código legado

Esta carpeta agrupa el **monolito ASP.NET original** (`EuropcarRental`) que antes vivía en `src/` en la raíz del repositorio.

El **stack activo** de backend es:

- `Middleware.RedCar/` — API pública/orquestador (compat `/api/v1` + contrato `/api/v2`).
- `EUROPCAR_V2/` — microservicios RedCar.

Solo abre o compila `EuropcarRental` si necesitas referencia histórica, migración puntual o funcionalidad aún no portada (por ejemplo panel **admin** y **Auth** del monolito).
