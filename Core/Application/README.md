# Application Layer (Capa de Aplicación)

## ?? Descripción
Contiene la **lógica de aplicación** y orquesta el flujo de datos. Implementa **CQRS con MediatR**.

## ?? Estructura

### Common/

#### Interfaces/
Contratos que implementará Infrastructure:
- `IApplicationDbContext.cs` - Contexto de BD
- `ITokenService.cs` - Generación de JWT
- `ICurrentUserService.cs` - Usuario actual
- `IDateTime.cs` - Abstracción de fecha/hora

#### Behaviours/
Pipelines de MediatR:
- `ValidationBehaviour.cs` - Validación automática con FluentValidation
- `LoggingBehaviour.cs` - Logging de requests
- `PerformanceBehaviour.cs` - Medición de performance

#### Mappings/
Perfiles de AutoMapper:
- `MappingProfile.cs` - Mapeos entre entidades y DTOs

#### Models/
DTOs compartidos:
- `Result.cs` - Patrón Result para respuestas
- `PaginatedList.cs` - Paginación
- Request/Response DTOs

#### Exceptions/
Excepciones de aplicación:
- `ValidationException.cs`
- `NotFoundException.cs`
- `ForbiddenException.cs`

### Features/
Organización por **característica** (Vertical Slice):

#### Auth/
- `Commands/RegisterCommand.cs`
- `Commands/LoginCommand.cs`
- `Commands/RefreshTokenCommand.cs`

#### Resources/
- `Commands/CreateResourceCommand.cs`
- `Commands/UpdateResourceCommand.cs`
- `Commands/DeleteResourceCommand.cs`
- `Queries/GetResourcesQuery.cs`
- `Queries/GetResourceByIdQuery.cs`

#### Reservations/
- `Commands/CreateReservationCommand.cs`
- `Commands/CancelReservationCommand.cs`
- `Queries/GetReservationByIdQuery.cs`
- `Queries/GetMyReservationsQuery.cs`
- `Queries/GetAllReservationsQuery.cs` (Admin)
- `Queries/CheckAvailabilityQuery.cs`

#### BlockedTimes/
- `Commands/CreateBlockedTimeCommand.cs`
- `Commands/DeleteBlockedTimeCommand.cs`
- `Queries/GetBlockedTimesQuery.cs`

#### Users/
- `Queries/GetMyProfileQuery.cs`
- `Queries/GetUsersQuery.cs` (Admin)

## ? Principios
- Patrón CQRS (Command Query Responsibility Segregation)
- Un handler por operación
- Validaciones con FluentValidation
- Solo depende de Domain
