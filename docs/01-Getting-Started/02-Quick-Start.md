# ? Inicio Rápido - Quick Start

## ?? En 5 Minutos

Si ya tienes todo instalado (ver [Installation.md](01-Installation.md)), aquí está el camino más rápido.

### Paso 1: Ejecutar la API

```bash
cd BookingService.Api
dotnet run
```

Espera a que veas:
```
Now listening on: https://localhost:7001
```

### Paso 2: Abrir Swagger

En tu navegador:
```
https://localhost:7001/swagger
```

¡Deberías ver todos los endpoints! ??

### Paso 3: Registrarte

En Swagger, ve a **Auth ? POST /api/auth/register** y haz clic en "Try it out"

```json
{
  "name": "Juan",
  "email": "juan@example.com",
  "password": "Password123!"
}
```

Haz clic en "Execute"

Respuesta esperada:
```json
{
  "isSuccess": true,
  "data": {
    "id": 1,
    "name": "Juan",
    "email": "juan@example.com",
    "role": "User"
  }
}
```

### Paso 4: Iniciar Sesión

Ve a **Auth ? POST /api/auth/login**

```json
{
  "email": "juan@example.com",
  "password": "Password123!"
}
```

Copia el `token` que recibiste:
```json
{
  "isSuccess": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "..."
  }
}
```

### Paso 5: Usar el Token

En Swagger, arriba a la derecha hay un botón **"Authorize"** (??)

Haz clic e ingresa:
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

(Reemplaza con tu token)

¡Ahora puedes hacer requests autenticados! 

### Paso 6: Crear un Recurso (Admin)

?? Necesitas permisos de Admin. Para desarrollo, edita tu usuario en BD.

```json
{
  "name": "Sala de Conferencias A",
  "description": "Sala con proyector y videoconferencia",
  "isActive": true
}
```

### Paso 7: Crear una Reserva

Ve a **Reservations ? POST /api/reservations**

```json
{
  "resourceId": 1,
  "startTime": "2025-02-10T10:00:00Z",
  "endTime": "2025-02-10T11:00:00Z",
  "notes": "Reunión importante"
}
```

¡Ya creaste tu primera reserva! ??

---

## ?? Con Postman/Insomnia

### 1. Exportar desde Swagger

En Swagger, abajo a la derecha:
```
https://localhost:7001/swagger/v1/swagger.json
```

Copia esta URL.

### 2. En Postman

- File ? Import
- Paste Raw Text
- Pega el JSON de Swagger
- ¡Listo!

### 3. Configurar Token

En Postman:
- Authorization ? Bearer Token
- Pega tu token

---

## ?? Comandos Útiles

### Ejecutar en modo Debug

```bash
dotnet run --configuration Debug
```

### Ver logs detallados

En `appsettings.json`, cambia:
```json
"LogLevel": {
  "Default": "Debug"
}
```

### Recargar la Base de Datos

```bash
dotnet ef database drop
dotnet ef database update
```

### Crear un User Admin para Testing

Ejecuta esto en SQL Server Management Studio:
```sql
UPDATE Users SET Role = 'Admin' WHERE Email = 'tu@email.com'
```

---

## ?? Test Rápidos

### Test 1: Health Check

```bash
curl https://localhost:7001/health
```

(Si existe)

### Test 2: Listar Recursos

```bash
curl https://localhost:7001/api/resources
```

### Test 3: Login y capturar Token

```bash
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"juan@example.com","password":"Password123!"}'
```

Copia el `token` del response.

### Test 4: Usar el Token

```bash
curl https://localhost:7001/api/reservations/my \
  -H "Authorization: Bearer <TU_TOKEN>"
```

---

## ?? Puntos Clave

| Lo que quieres | Dónde | Cómo |
|----------------|-------|------|
| Ver todos los endpoints | Swagger | `https://localhost:7001/swagger` |
| Registrarte | Auth endpoint | POST `/api/auth/register` |
| Login | Auth endpoint | POST `/api/auth/login` |
| Usar token | Swagger Authorize | Button arriba a la derecha |
| Crear reserva | Reservations | POST `/api/reservations` |
| Ver tus reservas | Reservations | GET `/api/reservations/my` |
| Ver recursos | Resources | GET `/api/resources` |

---

## ?? Errores Comunes

### "Connection timeout"
? ¿La API está corriendo? Verifica `dotnet run`

### "401 Unauthorized"
? ¿Usaste el token? Haz clic en Authorize primero

### "Swagger no carga"
? Limpia cache: `Ctrl+Shift+Delete`

### "Bad Request"
? Verifica el JSON: mira el error en el response

---

## ?? Siguiente Paso

Ahora que funcionó, ve a:

- **[Environment-Setup.md](03-Environment-Setup.md)** - Configuración detallada
- **[../02-Architecture/](../02-Architecture/)** - Entiende la arquitectura
- **[../03-Development/](../03-Development/)** - Empieza a desarrollar
- **[../04-API-Reference/](../04-API-Reference/)** - Referencia de endpoints

---

## ?? Debugging

Si algo no funciona, abre la **Consola de Developer** (F12) y mira:

1. Network tab - ¿Qué status code retorna?
2. Console - ¿Hay errores JavaScript?
3. Aplicación - ¿Swagger se cargó?

Si la API está crasheando:
```bash
# Ver logs detallados
dotnet run
# Lee la consola de errores
```

---

**¡Felicidades! ?? Ya tienes la API funcionando**

Próximo: [../03-Development/01-Quick-Guide.md](../03-Development/01-Quick-Guide.md)
