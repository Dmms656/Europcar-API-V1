# Práctica de laboratorio: gestión de secretos y configuración segura

**Proyecto:** Europcar Rental (API .NET + frontend React/Vite + PostgreSQL)  
**Documento de referencia del curso:** `Contexto/Práctica de Laboratorio. Gestión de Secretos y Configuración Segura.docx`  
**Fecha de elaboración de este informe:** 13 de mayo de 2026  

---

## 1. Objetivo del laboratorio (según enunciado)

Aplicar buenas prácticas de ciberseguridad mediante:

- Manejo seguro de **secretos** y **variables de entorno**.
- Configuración basada en **mínimo privilegio**.
- Identificación de **vulnerabilidades** en una implementación controlada.
- **Corrección** y endurecimiento (`.env`, `.gitignore`, separación de ambientes, roles en BD).
- **Validación** y documentación de riesgos mitigados.

---

## 2. Herramientas requeridas por el curso y uso en este proyecto

| Herramienta (enunciado) | Uso en el proyecto |
|-------------------------|-------------------|
| Visual Studio Code | Edición del código y terminal integrada |
| Git y GitHub | Control de versiones del repositorio |
| Python o Node.js | **Node.js** para el frontend (`frontend/`, npm/vite) |
| Archivo `.env` | Frontend: variables `VITE_*` (archivo local ignorado por Git) |
| PostgreSQL / MySQL | **PostgreSQL** (p. ej. Supabase) vía cadena `ConnectionStrings:RentalDb` |
| Linux o Windows con terminal | PowerShell / terminal para variables de entorno y `dotnet` |

---

## 3. Fases del laboratorio y trabajo realizado

### Fase 1 — Investigación inicial

**Solicitado:** elegir aplicación con BD, herramientas y métodos seguros para secretos.

**Decisiones documentadas para este repositorio:**

| Elemento | Elección |
|----------|----------|
| API activa (orquestador) | `Middleware.RedCar/src/Middleware.RedCar.Api` (ASP.NET Core) |
| API legada (monolito, referencia) | `_legacy/EuropcarRental/src/Europcar.Rental.Api` |
| Cliente web | `frontend/` (React + Vite) |
| Base de datos | PostgreSQL (`Npgsql`, EF Core) |
| Autenticación | JWT (`JwtSettings` en configuración) |
| Secretos en desarrollo .NET | Variables de entorno con doble guion bajo (`ConnectionStrings__RentalDb`) y/o [User Secrets](https://learn.microsoft.com/es-es/aspnet/core/security/app-secrets) |
| Secretos en frontend | Prefijo `VITE_` y archivo `.env` local (no versionado) |

---

### Fase 2 — Identificación de vulnerabilidades (implementación insegura controlada)

**Solicitado:** reconocer contraseñas en código, tokens expuestos, variables sensibles sin protección, privilegios administrativos innecesarios.

En este informe las vulnerabilidades se priorizan desde la **perspectiva de un atacante externo** (sin acceso al servidor ni al repositorio): alguien que interactúa con la **API pública en Internet** y con el **cliente web** (navegador), y que puede automatizar peticiones con herramientas como Burp, Postman o scripts. Así se vincula el riesgo con **endpoints**, **datos filtrados** y **superficie del navegador**, no solo con errores internos de despliegue.

#### 2.1 Superficie explotable desde la red y el cliente

| # | Vulnerabilidad / riesgo | Cómo la usaría un atacante externo | Ubicación o evidencia | Severidad (orientativa) |
|---|-------------------------|-----------------------------------|------------------------|-------------------------|
| A | **Consulta anónima de catálogo/detalle bajo ruta “admin”** | Un bot puede llamar `GET /api/v1/admin/vehiculos/disponibles` y `GET /api/v1/admin/vehiculos/{id}` **sin JWT**, igual que un usuario legítimo no autenticado. La ruta sugiere back-office; si el DTO devuelve más campos que la API pública de Booking (`/api/v1/vehiculos`), el atacante obtiene **información comercial u operativa extra** (inventario, precios internos, identificadores) sin credenciales. | `VehiculosController`: `[AllowAnonymous]` en esas acciones; controlador en ruta `admin/vehiculos`. | Alta / media (según riqueza del JSON frente al contrato público) |
| B | **Enumeración de clientes y fuga de datos personales (PII)** | Cualquiera puede hacer `POST /api/v1/admin/reservas/guest-client` con una cédula. Si el cliente ya existe, la API responde con **idCliente, nombre, apellido, identificación, correo**; si no existe, crea registro. Sirve para **enumerar** quién es cliente, **confirmar** cédulas válidas y recolectar correos para phishing o suplantación. | `ReservasController`: `[AllowAnonymous]` en `GuestClient`. | Alta |
| C | **Token de sesión almacenado en `localStorage`** | Con una vulnerabilidad **XSS** (o una extensión del navegador maliciosa), un script lee `localStorage.getItem('token')` y envía el JWT al atacante. Con ese token actúa como el usuario (cliente o admin, según el rol en el JWT) frente a la API, sin romper TLS. | `frontend/src/store/useAuthStore.js`: `localStorage.setItem('token', ...)`. | Alta (condicionada a XSS o compromiso del cliente) |
| D | **Reconocimiento de superficie completa (Swagger/UI)** | Si Swagger está publicado en producción sin autenticación, el atacante obtiene la **lista de todos los endpoints**, parámetros y esquemas, incluidos los de administración, y planea abusos (IDOR, lógica de negocio, fuzzing). | `Program.cs`: `app.UseSwagger()` / `UseSwaggerUI()` sin condición de entorno. | Media |
| E | **Abuso de CORS + credenciales** (riesgo de diseño) | La política actual usa orígenes fijos, pero `AllowAnyMethod()` y `AllowAnyHeader()` amplían lo aceptado. Un atacante combina esto con **apps maliciosas** solo si engaña al usuario para que use un origen permitido o si hay **XSS en un origen confiable** listado. | `ServiceCollectionExtensions.AddCorsPolicy`. | Baja / media (depende del despliegue) |

#### 2.1.1 Evidencia en código (lectura anónima bajo prefijo `admin`)

El controlador de vehículos exige `[Authorize]` a nivel de clase, pero **dos acciones** permiten acceso anónimo bajo la ruta `api/v1/.../admin/vehiculos`:

```43:56:_legacy/EuropcarRental/src/Europcar.Rental.Api/Controllers/V1/Internal/VehiculosController.cs
    [HttpGet("disponibles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDisponibles([FromQuery] BuscarVehiculosRequest request)
    {
        var result = await _vehiculoService.GetDisponiblesAsync(request);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>
    /// Obtener detalle de un vehículo por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
```

#### 2.1.2 Evidencia: endpoint anónimo que devuelve datos de cliente existente

```44:62:_legacy/EuropcarRental/src/Europcar.Rental.Api/Controllers/V1/Internal/ReservasController.cs
    [HttpPost("guest-client")]
    [AllowAnonymous]
    public async Task<IActionResult> GuestClient([FromBody] GuestClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cedula) || string.IsNullOrWhiteSpace(request.Nombre))
            return BadRequest(ApiResponse<object>.Fail("Cédula y nombre son obligatorios"));

        var existing = await _clienteDataService.GetByIdentificacionAsync(request.Cedula.Trim());
        if (existing != null)
        {
            return Ok(ApiResponse<object>.Ok(new
            {
                existing.IdCliente,
                existing.Nombre1,
                existing.Apellido1,
                existing.NumeroIdentificacion,
                existing.Correo,
                esNuevo = false
            }, "Cliente existente encontrado"));
```

#### 2.1.3 Evidencia: JWT en el almacenamiento del navegador

```21:34:frontend/src/store/useAuthStore.js
  login: (loginResponse, type = 'admin') => {
    const userData = {
      username: loginResponse.username,
      correo: loginResponse.correo,
      roles: loginResponse.roles,
      expiration: loginResponse.expiration,
      // Client-specific fields
      idCliente: loginResponse.idCliente,
      nombreCompleto: loginResponse.nombreCompleto,
    };
    localStorage.setItem('token', loginResponse.token);
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.setItem('userType', type);
```

#### 2.2 Riesgos que un atacante externo explota *indirectamente* (cadena de suministro o filtración)

Estos no requieren que el atacante ya tenga shell en el servidor, pero **sí** acceso al artefacto (repo público, backup, log):

| # | Vulnerabilidad / riesgo | Cómo encaja con un atacante externo | Ubicación o evidencia | Severidad |
|---|-------------------------|-------------------------------------|------------------------|-----------|
| F | **Clave JWT o cadena de conexión en archivos versionados** | Filtración del repositorio → firma de tokens válidos o conexión directa a la BD → **compromiso total** sin tocar el front. | `appsettings.json` (`JwtSettings:SecretKey`, `ConnectionStrings`). | Alta |
| G | **Credenciales en scripts auxiliares** (historial Git) | Misma cadena F: acceso a datos y posible pivot a otros sistemas. | `scripts/KillZombies.cs` (versión anterior; **corregida**, ver sección 4). | Alta |

#### 2.3 Otros hallazgos (ámbito interno / desarrollo)

| # | Riesgo | Nota |
|---|--------|------|
| H | Contraseña débil `12345` en semilla (`Program.cs`) | Riesgo real si la API con seed se expone a Internet; el atacante externo entra con credenciales conocidas de demo. |
| I | Rol `postgres` en cadena típica | No es “explotación HTTP” directa; mitigar con usuario de aplicación y permisos mínimos. |

**Nota:** El hallazgo **G** (script con contraseña embebida) fue **corregido en código** (sección 4). Los hallazgos **A–E** siguen vigentes a nivel de diseño hasta aplicar endurecimiento en API y cliente (sección 3 ampliada). **F** requiere User Secrets / variables de entorno y rotación de claves si hubo exposición.

---

### Fase 3 — Corrección segura y endurecimiento

**Solicitado por el curso:**

- Archivo con variables de entorno (`.env`).
- Exclusión de secretos con `.gitignore`.
- Mínimo privilegio en BD y usuarios.
- Separación desarrollo / producción.

**Estado en el repositorio:**

| Requisito | Estado | Evidencia / notas |
|-----------|--------|-------------------|
| `.env` (frontend) | Parcial / convención correcta | `import.meta.env.VITE_API_URL` en `frontend/src/api/axiosClient.js`; `frontend/.env` local (no versionado). |
| `.gitignore` | Cumplido | Entrada `.env` en la raíz del repositorio. |
| Corrección “no secretos en código” en script | **Hecho** | `scripts/KillZombies.cs` (sección 4). |
| Mínimo privilegio BD | Pendiente de operación | Usuario SQL de aplicación con `GRANT` acotados. |
| Separación dev/prod | Recomendado | Secretos en User Secrets / variables del host, no en `appsettings.json` commiteado. |
| **Endurecer superficie HTTP (A–E)** | Pendiente / parcial | Deshabilitar o proteger Swagger en producción; valorar **rate limiting** y CAPTCHA o prueba de trabajo en `guest-client`; alinear **catálogo público** solo con rutas no-`admin` o mismos DTOs que Booking; valorar **cookies HttpOnly** + SameSite en lugar de JWT en `localStorage`; revisión de DTO en `GetDisponibles` / `GetById` vs API pública. |

**Mitigaciones recomendadas ligadas a un atacante externo (prioridad):**

1. **B (guest-client):** no devolver correo ni `idCliente` hasta después de verificación (OTP, enlace mágico) o exigir un secreto compartido por canal autenticado; limitar tasa por IP.
2. **A (vehículos bajo `admin`):** quitar `[AllowAnonymous]` y exponer búsqueda/detalle solo en `BookingVehiculosController` (`/api/v1/vehiculos`) con el mismo contrato reducido, o exigir JWT de rol público de solo lectura.
3. **C (JWT):** mitigar XSS (CSP, sanitización); valorar almacenar sesión en **cookie HttpOnly** firmada por el servidor.
4. **D (Swagger):** activar Swagger solo en `Development` o proteger con autenticación básica / red privada.

---

### Fase 4 — Validación, análisis y pruebas

**Solicitado:** ejecutar la aplicación sin exponer secretos; pruebas funcionales; documentar vulnerabilidades corregidas y riesgos mitigados.

**Lista de comprobación sugerida:**

- [ ] API arranca con `ConnectionStrings__RentalDb` y `JwtSettings__SecretKey` definidos en el entorno (o User Secrets), sin contraseñas en texto en archivos trackeados.
- [ ] Frontend arranca con `frontend/.env` y `VITE_API_URL` apuntando a la API.
- [ ] Login y un flujo crítico (p. ej. catálogo o reserva) funcionan.
- [ ] Capturas de pantalla **sin** contraseñas ni tokens completos visibles.
- [ ] Comprobar que Swagger no queda expuesto en producción (o está protegido).
- [ ] Revisar con Burp/Postman respuestas de `guest-client` y vehículos anónimos: qué PII o campos extra devuelve la API frente a un cliente no autenticado.

---

## 4. Cambio de código realizado: antes y después

### 4.1 Archivo `scripts/KillZombies.cs`

**Problema:** cadena de conexión Npgsql con **usuario y contraseña incrustados** en el repositorio (anti‑patrón explícito del laboratorio: “contraseñas visibles en código”).

**Mitigación aplicada:** leer la cadena desde variables de entorno, alineado con la convención de ASP.NET Core (`ConnectionStrings__RentalDb`) y un alias explícito (`RENTAL_DB_CONNECTION`).

#### Código **antes** (vulnerables: secretos en el archivo)

> **Aviso de seguridad:** en el historial real del archivo figuraba una contraseña literal. En este informe se muestra un **ejemplo redactado**; no vuelva a pegar secretos reales en documentación ni en código versionado.

```csharp
using Npgsql;

var cs = "Host=db.EJEMPLO.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<SECRETO_EN_TEXTO_PLANO>;Ssl Mode=Require;Trust Server Certificate=true;Timeout=15;Command Timeout=15";

try
{
    await using var conn = new NpgsqlConnection(cs);
    // ... resto igual ...
```

#### Código **después** (versión actual en el repositorio)

```csharp
using Npgsql;

// No incluir credenciales en el repositorio. Usar la misma cadena que la API, por ejemplo:
//   $env:ConnectionStrings__RentalDb = "Host=...;Password=...;..."
// o   set ConnectionStrings__RentalDb=...
var cs = Environment.GetEnvironmentVariable("ConnectionStrings__RentalDb")
    ?? Environment.GetEnvironmentVariable("RENTAL_DB_CONNECTION")
    ?? throw new InvalidOperationException(
        "Define ConnectionStrings__RentalDb o RENTAL_DB_CONNECTION con la cadena Npgsql.");

try
{
    await using var conn = new NpgsqlConnection(cs);
    await conn.OpenAsync();
    Console.WriteLine("Connected");

    // Test INSERT factura with id_reserva=6
    var sw = System.Diagnostics.Stopwatch.StartNew();
    await using var cmd = new NpgsqlCommand(@"
        INSERT INTO rental.facturas
        (factura_guid, numero_factura, id_cliente, id_reserva, fecha_emision,
         subtotal, valor_iva, total, estado_factura, servicio_origen, origen_canal_factura,
         observaciones_factura, creado_por_usuario, fecha_registro_utc)
        VALUES (gen_random_uuid(), 'FAC-DIRECT-TEST', 9, 6,
                CURRENT_TIMESTAMP, 73.91, 11.09, 85, 'EMITIDA', 'TEST', 'WEB',
                'Test directo', 'test', CURRENT_TIMESTAMP)
        RETURNING id_factura", conn);
    cmd.CommandTimeout = 10;
    var id = await cmd.ExecuteScalarAsync();
    Console.WriteLine($"Factura INSERT OK ({sw.ElapsedMilliseconds}ms) - id_factura: {id}");

    // Cleanup
    await using var cmdDel = new NpgsqlCommand("DELETE FROM rental.facturas WHERE numero_factura = 'FAC-DIRECT-TEST'", conn);
    await cmdDel.ExecuteNonQueryAsync();
    Console.WriteLine("Cleaned up");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Inner: {ex.InnerException?.Message}");
}
```

**Riesgo mitigado:** eliminación del secreto de BD del árbol de código fuente para ese script (sigue siendo necesario **rotar la contraseña** en el proveedor si alguna vez se publicó en Git o se compartió).

---

### 4.2 Referencia: configuración sensible actual (sin modificar en esta práctica)

El archivo `_legacy/EuropcarRental/src/Europcar.Rental.Api/appsettings.json` sigue conteniendo valores que **no deberían considerarse seguros en un repositorio público**:

```json
{
  "ConnectionStrings": {
    "RentalDb": "Host=db.ufqzdzdkcqmwvapdaajx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=********;Ssl Mode=Require;Trust Server Certificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "EuropcarRentalApi2026SuperSecretKey_MustBeAtLeast32Chars!",
    "Issuer": "Europcar.Rental.Api",
    "Audience": "Europcar.Rental.Client",
    "ExpirationMinutes": 60
  },
  ...
}
```

**Recomendación para cerrar el laboratorio al 100%:** mover `JwtSettings:SecretKey` y la cadena real de `RentalDb` a User Secrets (local) o variables del servicio de hosting (producción), dejando en `appsettings.json` solo valores no secretos o marcadores.

---

### 4.3 Referencia: semilla con contraseña débil (`Program.cs`)

Fragmento relevante (para el apartado “vulnerabilidades controladas / riesgos en desarrollo”):

```csharp
if (!LooksLikeBase64(existing.PasswordSalt) || !LooksLikeBase64(existing.PasswordHash))
{
    var (repairHash, repairSalt) = AuthService.CreatePasswordHash("12345");
    existing.PasswordHash = repairHash;
    existing.PasswordSalt = repairSalt;
    ...
}

// ...

var (hash, salt) = AuthService.CreatePasswordHash("12345");
```

**Mitigación sugerida en el informe:** documentar que es solo para **desarrollo**, deshabilitar el seed en producción o exigir cambio de contraseña al primer login (`RequiereCambioPassword`).

---

## 5. Cómo ejecutar el script corregido (validación)

PowerShell (sesión actual):

```powershell
$env:ConnectionStrings__RentalDb = "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require;Trust Server Certificate=true;"
dotnet script "c:\Users\medin\source\repos\Proyecto Desarrollo\scripts\KillZombies.cs"
```

Si el equipo no usa `dotnet script`, puede compilarse como herramienta puntual o ejecutarse el mismo enfoque desde la API ya configurada.

---

## 6. Resultados esperados por el curso (checklist)

| Resultado esperado | Cómo se demuestra con este proyecto |
|--------------------|-------------------------------------|
| Aplicación funcional y segura | Ejecución + pruebas; comprobar que endpoints sensibles no filtran más datos de los necesarios a anónimos |
| Variables protegidas | `.env` ignorado; script sin contraseña en archivo; API con override por entorno |
| Roles limitados en BD | Informe + (ideal) script SQL de usuario de aplicación con `GRANT` mínimos |
| **Entregables:** código corregido | Commit con `KillZombies.cs` sin secretos embebidos; roadmap de cambios A–D en API/cliente |
| **Entregables:** capturas | Swagger, login, variables en terminal con valores censurados; captura de respuesta **anonimizada** de `guest-client` o catálogo para el informe |
| **Entregables:** informe técnico | Este documento + tabla vulnerabilidad → vector de ataque externo → solución |
| **Entregables:** conclusiones | Sección 7 |

---

## 7. Conclusiones sobre desarrollo seguro

1. **Los secretos no deben vivir en el repositorio:** una filtración de `appsettings.json` o scripts permite a un atacante **saltarse por completo** la superficie web y firmar JWT o conectar a la BD.
2. **La seguridad no es solo “login y roles”:** un atacante externo explota **endpoints anónimos** (p. ej. bajo rutas que parecen administrativas o flujos de invitado) y **respuestas demasiado ricas en datos personales o de negocio**.
3. **El navegador es parte del perímetro:** JWT en `localStorage` es un vector clásico tras XSS; hay que endurecer el front y valorar modelos de sesión más acotados al origen.
4. **La documentación automática (Swagger) es un mapa para el atacante** si queda abierta en producción sin control.
5. **Variables de entorno y User Secrets** siguen siendo la base para separar secretos de código en el servidor; el cliente solo debe recibir **URLs públicas** y nunca claves de firma JWT.
6. Tras exposición de credenciales o PII por diseño de API, la mitigación incluye **rotación de claves**, **rediseño de contratos** y **límites de tasa** / controles anti-bots en endpoints abusables.

---

## 8. Detalle adicional: frontend y variables `VITE_*`

El cliente HTTP usa:

```javascript
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  ...
});
```

Conviene que el archivo `frontend/.env.example` documente explícitamente `VITE_API_URL` (además de las variables de Cloudinary que ya figuran), para que el laboratorio y nuevos desarrolladores tengan una plantilla clara sin commitear secretos.

---

*Fin del documento.*
