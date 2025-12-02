# ?? Referencia de API - API Reference

## ?? Índice de Endpoints

Esta sección documenta **todos los endpoints REST** de la API.

### ?? Documentos:

1. **[00-README.md](00-README.md)** (Este)
   - Índice de endpoints
   - Cómo usar esta referencia

2. **[01-Authentication.md](01-Authentication.md)**
   - POST `/api/auth/register`
   - POST `/api/auth/login`
   - POST `/api/auth/refresh`

3. **[02-Resources.md](02-Resources.md)**
   - GET `/api/resources`
   - GET `/api/resources/{id}`
   - POST `/api/resources` (Admin)
   - PUT `/api/resources/{id}` (Admin)
   - DELETE `/api/resources/{id}` (Admin)

4. **[03-Reservations.md](03-Reservations.md)**
   - POST `/api/reservations`
   - GET `/api/reservations/my`
   - GET `/api/reservations/{id}`
   - GET `/api/reservations` (Admin)
   - DELETE `/api/reservations/{id}/cancel`
   - GET `/api/reservations/availability`

5. **[04-BlockedTimes.md](04-BlockedTimes.md)** (Admin Only)
   - GET `/api/blockedtimes`
   - POST `/api/blockedtimes`
   - DELETE `/api/blockedtimes/{id}`

6. **[05-Users.md](05-Users.md)**
   - GET `/api/users/profile`
   - GET `/api/users` (Admin)
   - PUT `/api/users/profile`

---

## ??? Mapa Rápido de Endpoints

```
/api/auth/
  ??? POST /register   ? Registrarse
  ??? POST /login       ? Iniciar sesión
  ??? POST /refresh    ? Renovar token

/api/resources/
  ??? GET  /       ? Listar todos
  ??? GET  /{id}         ? Detalle
  ??? POST /           [A]  ? Crear
  ??? PUT/{id}              [A]  ? Actualizar
  ??? DELETE /{id}            [A]  ? Eliminar

/api/reservations/
  ??? POST /      ? Crear
  ??? GET  /my        ? Mis reservas
  ??? GET  /{id}        ? Detalle
  ??? DELETE /{id}/cancel          ? Cancelar
  ??? GET  /          [A]  ? Todas (Admin)
  ??? GET  /availability           ? Consultar disponibilidad

/api/blockedtimes/              [A] Admin only
  ??? GET  /     ? Listar
  ??? POST /        ? Crear
  ??? DELETE /{id}      ? Eliminar

/api/users/
  ??? GET  /profile        ? Mi perfil
  ??? PUT  /profile        ? Actualizar perfil
  ??? GET  /          [A]  ? Listar usuarios

[A] = Requiere rol Admin
```

---

## ?? Autenticación

### Cómo autenticarse

#### 1. Registrarse
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "password": "Password123!"
}
```

#### 2. Iniciar Sesión
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "juan@example.com",
  "password": "Password123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresIn": 3600
}
```

#### 3. Usar el Token

Agrega el token a **cada request**:

```http
GET /api/reservations/my
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Expiración

- **Token:** Expira en 60 minutos
- **Refresh Token:** Expira en 7 días

Para renovar:
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "..."
}
```

---

## ?? Estructuras de Datos

### User
```json
{
  "id": 1,
  "name": "Juan Pérez",
  "email": "juan@example.com",
  "role": "User",
  "createdAt": "2025-02-01T10:00:00Z"
}
```

### Resource
```json
{
  "id": 1,
  "name": "Sala de Conferencias A",
  "description": "Sala con proyector",
  "isActive": true,
  "createdAt": "2025-02-01T10:00:00Z"
}
```

### Reservation
```json
{
  "id": 1,
  "resourceId": 1,
  "resourceName": "Sala de Conferencias A",
  "userId": 1,
  "userName": "Juan Pérez",
  "startTime": "2025-02-10T10:00:00Z",
  "endTime": "2025-02-10T11:00:00Z",
  "status": "Active",
  "notes": "Reunión importante",
  "createdAt": "2025-02-05T09:30:00Z"
}
```

### BlockedTime
```json
{
  "id": 1,
  "resourceId": 1,
  "startTime": "2025-02-15T08:00:00Z",
  "endTime": "2025-02-15T09:00:00Z",
  "reason": "Mantenimiento"
}
```

---

## ?? Códigos de Estado HTTP

| Código | Significado | Ejemplo |
|--------|-------------|---------|
| **200** | OK | GET exitoso |
| **201** | Created | Recurso creado |
| **204** | No Content | DELETE exitoso |
| **400** | Bad Request | Datos inválidos |
| **401** | Unauthorized | Falta token |
| **403** | Forbidden | Sin permisos |
| **404** | Not Found | Recurso no existe |
| **409** | Conflict | Solapamiento de reserva |
| **500** | Server Error | Error del servidor |

---

## ?? Errores Comunes

### "401 Unauthorized"
- **Causa:** Falta token o token expirado
- **Solución:** Incluir `Authorization: Bearer <token>`

### "403 Forbidden"
- **Causa:** No tienes permisos (ej: no eres Admin)
- **Solución:** Cambia tu rol en BD o usa cuenta Admin

### "400 Bad Request"
- **Causa:** Datos inválidos
- **Solución:** Revisa el error en el response

### "409 Conflict"
- **Causa:** Solapamiento de reserva
- **Solución:** Elige otro horario

### "404 Not Found"
- **Causa:** Recurso no existe
- **Solución:** Verifica el ID

---

## ?? Testing con Postman

### 1. Importar Swagger
- URL: `https://localhost:7001/swagger/v1/swagger.json`
- En Postman: File ? Import ? Paste Link

### 2. Configurar Colección

```json
{
  "info": { "name": "BookingService" },
  "item": [
  {
      "name": "Auth",
      "item": [
        { "name": "Register", "request": { "method": "POST", "url": "{{base_url}}/api/auth/register" } },
        { "name": "Login", "request": { "method": "POST", "url": "{{base_url}}/api/auth/login" } }
      ]
    }
  ]
}
```

### 3. Variables de Entorno

```json
{
  "base_url": "https://localhost:7001",
  "token": ""
}
```

---

## ?? Flujo Típico de Uso

### 1. Registrarse
```bash
POST /api/auth/register
```

### 2. Iniciar Sesión
```bash
POST /api/auth/login
```
? Guarda el token

### 3. Ver Recursos
```bash
GET /api/resources
```
(Sin token, solo para usuarios anónimos)

### 4. Crear Reserva
```bash
POST /api/reservations
Authorization: Bearer <token>
```

### 5. Ver Mis Reservas
```bash
GET /api/reservations/my
Authorization: Bearer <token>
```

### 6. Cancelar Reserva
```bash
DELETE /api/reservations/{id}/cancel
Authorization: Bearer <token>
```

---

## ?? Documentación Detallada

Elige un endpoint para ver detalles:

- ?? **[01-Authentication.md](01-Authentication.md)** - Autenticación
- ?? **[02-Resources.md](02-Resources.md)** - Gestión de recursos
- ?? **[03-Reservations.md](03-Reservations.md)** - Gestión de reservas
- ?? **[04-BlockedTimes.md](04-BlockedTimes.md)** - Bloqueos (Admin)
- ?? **[05-Users.md](05-Users.md)** - Gestión de usuarios

---

## ?? Swagger UI

La mejor forma de ver y testear endpoints es con Swagger:

```
https://localhost:7001/swagger
```

Cada endpoint tiene:
- ? Descripción
- ? Parámetros
- ? Response models
- ? Try it out (testea directamente)
- ? Autorización integrada

---

## ?? Soporte

¿No encuentras lo que buscas?

- Revisa **[Swagger UI](https://localhost:7001/swagger)**
- Lee **[../../ARCHITECTURE.md](../../ARCHITECTURE.md)**
- Lee **[../03-Development/](../03-Development/)**

---

**Selecciona un endpoint para más detalles ??**
