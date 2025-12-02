# Infrastructure Layer (Capa de Infraestructura)

## ?? Descripción
Implementa las **interfaces definidas en Application**. Contiene todo lo relacionado con tecnologías externas.

## ?? Estructura

### Persistence/

#### Configurations/
Configuraciones de Entity Framework:
- `UserConfiguration.cs` - Configuración de tabla User
- `ResourceConfiguration.cs`
- `ReservationConfiguration.cs`
- `BlockedTimeConfiguration.cs`

**Incluye:**
- Primary Keys
- Foreign Keys
- Índices
- Restricciones
- Propiedades requeridas/opcionales

#### Migrations/
Migraciones de EF Core (generadas automáticamente)

#### `ApplicationDbContext.cs`
Contexto principal de Entity Framework

### Identity/
Gestión de identidad y autenticación:
- `TokenService.cs` - Generación y validación de JWT
- `PasswordHasher.cs` - Hash de contraseñas (BCrypt)
- `CurrentUserService.cs` - Usuario del contexto HTTP

### Services/
Implementaciones de servicios:
- `DateTimeService.cs` - Servicio de fecha/hora actual
- Otros servicios de infraestructura

### `DependencyInjection.cs`
Configuración de inyección de dependencias para esta capa

## ? Principios
- Implementa interfaces de Application
- No expone detalles de implementación
- Puede cambiar tecnologías sin afectar otras capas
- Depende de Application y Domain
