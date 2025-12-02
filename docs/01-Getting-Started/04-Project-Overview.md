# ?? Visión General del Proyecto - Project Overview

## ?? ¿Qué es BookingService.Api?

**BookingService.Api** es una **API REST profesional para gestión de reservas de recursos**, construida con **ASP.NET Core 8** siguiendo las mejores prácticas empresariales.

Es como un sistema para que:
- ? **Usuarios** reserven recursos (salas, pistas, vehículos, herramientas)
- ?? **Admins** gestionen recursos y bloqueos de mantenimiento
- ?? **El sistema** valide disponibilidad y evite solapamientos

---

## ?? Características Principales

### ? Gestión de Recursos
- CRUD completo (Create, Read, Update, Delete)
- Activar/desactivar recursos
- Solo administradores pueden gestionar

### ? Sistema de Reservas
- Crear reservas con validaciones
- Consultar disponibilidad
- Cancelar reservas propias
- Ver historial personal

### ? Validaciones Inteligentes
- **Sin solapamientos:** No permite 2 reservas en el mismo horario
- **Duración:** Mínimo 30 minutos, máximo 4 horas
- **Bloqueos:** Respeta el mantenimiento programado
- **Disponibilidad:** Consulta en tiempo real

### ? Seguridad
- **JWT Bearer** - Tokens seguros
- **Roles:** User y Admin con permisos diferentes
- **Contraseñas:** Hasheadas con BCrypt
- **Autorización:** Por roles en endpoints

### ? Documentación
- **Swagger/OpenAPI** - API autodocumentada
- **Ejemplos:** Cada endpoint tiene ejemplos
- **Tipos:** Validación de tipos en requests

### ? Infraestructura
- **Docker** - Containerización
- **Logging** - Serilog estructurado
- **Tests** - Preparado para TDD

---

## ??? Arquitectura en Capas

```
???????????????????????????????????????
?   PRESENTATION LAYER  ?  Usuarios ? API
?   (Controllers, Middlewares)        ?
???????????????????????????????????????
          ?        ?
    ?  MediatR
          ?
???????????????????????????????????????
?  APPLICATION LAYER       ?  Lógica de negocio
?  (CQRS, Handlers, DTOs)      ?
???????????????????????????????????????
       ?        ?
  ?
???????????????????????????????????????
?    DOMAIN LAYER     ?  Entidades puras
?  (Entities, Rules, Enums)           ?  Sin frameworks
???????????????????????????????????????
          ?        ?
          ?
???????????????????????????????????????
?  INFRASTRUCTURE LAYER       ?  BD, JWT, servicios
?  (EF Core, Services, Identity)       ?
???????????????????????????????????????
```

### Principios

? **Clean Architecture** - Separación clara de responsabilidades  
? **CQRS** - Commands (escritura) y Queries (lectura) separados  
? **Vertical Slice** - Features autocontenidas  
? **SOLID** - Código mantenible y escalable  
? **Dependency Injection** - Inversión de dependencias  

---

## ?? Entidades Principales

### ?? User (Usuario)

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; } // User o Admin
    public DateTime CreatedAt { get; set; }
}
```

**Roles:**
- `User` - Puede hacer reservas
- `Admin` - Puede gestionar recursos y bloqueos

---

### ?? Resource (Recurso Reservable)

```csharp
public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Ejemplos:**
- Sala de conferencias
- Cancha de tenis
- Vehículo
- Herramienta de construcción

---

### ?? Reservation (Reserva)

```csharp
public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ReservationStatus Status { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Estados:**
- `Active` - Reserva vigente
- `Cancelled` - Cancelada por usuario
- `Completed` - Finalizada

**Reglas:**
- ?? Duración mínima: 30 minutos
- ?? Duración máxima: 4 horas
- ?? No puede solaparse con otras
- ?? No puede solapar con bloqueos

---

### ?? BlockedTime (Bloqueo)

```csharp
public class BlockedTime
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public DateTime StartTime { get; set; }
public DateTime EndTime { get; set; }
    public string Reason { get; set; }
}
```

**Uso:**
- Mantenimiento de recurso
- Limpieza
- Reparación
- Cualquier bloqueo administrativo

---

## ?? Flujo de Operación

### Crear una Reserva (ejemplo)

```
1. Usuario hace POST /api/reservations
   {
     "resourceId": 1,
     "startTime": "2025-02-10T10:00:00Z",
     "endTime": "2025-02-10T11:00:00Z"
   }

2. Controller recibe request
   ?
3. MediatR envía CreateReservationCommand
   ?
4. Validator valida con FluentValidation
   ?
5. Handler ejecuta lógica de negocio:
 - ¿Existe el recurso?
   - ¿Está activo?
   - ¿Hay solapamientos?
   - ¿Hay bloqueos?
   - ¿Duración correcta?
   ?
6. Si todo OK, guarda en BD con EF Core
   ?
7. AutoMapper convierte Entidad ? DTO
   ?
8. Controller retorna HTTP 201 Created
   {
     "id": 5,
     "resourceId": 1,
     "status": "Active",
     ...
   }
```

---

## ?? Endpoints Principales

### ?? Autenticación

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/auth/register` | POST | Registrar nuevo usuario |
| `/api/auth/login` | POST | Iniciar sesión (retorna JWT) |
| `/api/auth/refresh` | POST | Renovar token vencido |

### ?? Recursos

| Endpoint | Método | Rol | Descripción |
|----------|--------|-----|-------------|
| `/api/resources` | GET | User | Listar todos |
| `/api/resources/{id}` | GET | User | Detalle |
| `/api/resources` | POST | Admin | Crear |
| `/api/resources/{id}` | PUT | Admin | Actualizar |
| `/api/resources/{id}` | DELETE | Admin | Eliminar |

### ?? Reservas

| Endpoint | Método | Rol | Descripción |
|----------|--------|-----|-------------|
| `/api/reservations` | POST | User | Crear |
| `/api/reservations/my` | GET | User | Mis reservas |
| `/api/reservations/{id}` | GET | User | Detalle |
| `/api/reservations/{id}/cancel` | DELETE | User | Cancelar |
| `/api/reservations` | GET | Admin | Todas |
| `/api/reservations/availability` | GET | User | Consultar |

### ?? Bloqueos

| Endpoint | Método | Rol | Descripción |
|----------|--------|-----|-------------|
| `/api/blockedtimes` | POST | Admin | Crear bloqueo |
| `/api/blockedtimes` | GET | Admin | Listar |
| `/api/blockedtimes/{id}` | DELETE | Admin | Eliminar |

---

## ?? Tecnologías Usadas

### Backend
- **ASP.NET Core 8** - Framework web
- **Entity Framework Core** - ORM para BD
- **MediatR** - Patrón Mediator/CQRS
- **FluentValidation** - Validaciones
- **AutoMapper** - Mapeo de objetos
- **Serilog** - Logging estructurado

### Seguridad
- **JWT Bearer** - Autenticación
- **BCrypt** - Hash de contraseñas
- **Authorization** - Autorización por roles

### Documentación & Testing
- **Swagger/OpenAPI** - Documentación interactiva
- **xUnit** (opcional) - Tests unitarios
- **Moq** (opcional) - Mocking

### Deployment
- **Docker** - Containerización
- **SQL Server / PostgreSQL** - Base de datos

---

## ?? Patrones Implementados

### ? CQRS (Command Query Responsibility Segregation)
- **Commands** = Operaciones que escriben
- **Queries** = Operaciones que leen
- Separación clara de responsabilidades

### ? Repository Pattern
- Abstracción del acceso a datos
- Facilita testing y cambios de BD

### ? Result Pattern
- Manejo de errores sin excepciones
- Respuestas consistentes

### ? Dependency Injection
- IoC Container de ASP.NET
- Inyección en constructores

### ? Vertical Slice Architecture
- Features autocontenidas
- Un cambio en una feature no afecta otras

### ? Validation Pipeline
- Validación en cada capa
- FluentValidation en Application
- Authorization en Presentation

---

## ?? Seguridad Implementada

### Autenticación
```
Usuario ? Login ? JWT Token ? Autorizado ?
```

### Autorización
```
Endpoint ? [Authorize(Roles = "Admin")] ? ¿Es Admin? ? Sí ? / No ?
```

### Validación de Datos
```
Request ? Validator ? ¿Válido? ? Sí ? / No ?
```

### Hash de Contraseñas
```
Contraseña ? BCrypt ? Hash en BD
```

---

## ?? Escalabilidad

Preparado para:
- ? **Microservicios** - Cada feature podría ser un servicio
- ? **CQRS avanzado** - Separación de BD lectura/escritura
- ? **Event Sourcing** - Historial de eventos
- ? **API Gateway** - Múltiples APIs
- ? **CI/CD** - Deploy automático
- ? **Cloud** - Azure, AWS, GCP

---

## ?? Estructura de Carpetas

```
BookingService.Api/
??? Core/   ? Núcleo (sin dependencias)
?   ??? Domain/          ? Entidades puras
?   ??? Application/   ? CQRS, handlers
??? Infrastructure/      ? Implementaciones
?   ??? Persistence/     ? EF Core
?   ??? Identity/        ? JWT, contraseñas
??? Presentation/        ? API
?   ??? Controllers/
??? docs/  ? Esta documentación
```

---

## ?? Casos de Uso Reales

### Caso 1: Sistema de Reserva de Salas de Conferencias
- Recursos = Salas
- Usuarios = Empleados
- Reservas = Ocupaciones de salas

### Caso 2: Sistema de Pistas Deportivas
- Recursos = Pistas (tenis, volley, etc.)
- Usuarios = Socios
- Reservas = Horarios de uso

### Caso 3: Sistema de Alquiler de Vehículos
- Recursos = Vehículos
- Usuarios = Clientes
- Reservas = Períodos de uso

### Caso 4: Sistema de Herramientas
- Recursos = Herramientas
- Usuarios = Trabajadores
- Reservas = Préstamos

---

## ? Checklist: ¿Está Listo para Producción?

- ? Clean Architecture
- ? CQRS Pattern
- ? Validaciones robustas
- ? JWT Authentication
- ? Role-based Authorization
- ? Logging estructurado
- ? Error handling global
- ? DTOs y AutoMapper
- ? Swagger documentado
- ? Docker ready
- ? Tests (en desarrollo)
- ? CI/CD (en desarrollo)

---

## ?? Próximos Pasos

1. **[Installation.md](01-Installation.md)** - Instala el proyecto
2. **[Quick-Start.md](02-Quick-Start.md)** - Haz tu primer request
3. **[../02-Architecture/](../02-Architecture/)** - Aprende la arquitectura
4. **[../03-Development/](../03-Development/)** - Empieza a desarrollar

---

**¡Listo para comenzar? ?? [Installation.md](01-Installation.md)**
