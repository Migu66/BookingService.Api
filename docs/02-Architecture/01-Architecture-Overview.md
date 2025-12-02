# ??? Visión General de la Arquitectura - Architecture Overview

## ?? Las 4 Capas

BookingService.Api está organizado en **4 capas independientes**:

```
??????????????????????????????????????????????????
? 4. PRESENTATION LAYER     ? ASP.NET Core
? Controllers, Middlewares, Swagger         ?
?    Responsabilidad: Exponer la API HTTP    ?
??????????????????????????????????????????????????
            ? (MediatR)
??????????????????????????????????????????????????
? 3. APPLICATION LAYER   ? CQRS
?    Commands, Queries, Handlers, Validators     ?
?    Responsabilidad: Lógica de casos de uso     ?
??????????????????????????????????????????????????
      ? (Entities)
??????????????????????????????????????????????????
? 2. DOMAIN LAYER           ? Pure C#
?    Entities, Enums, Business Rules             ?
?    Responsabilidad: Reglas de negocio          ?
??????????????????????????????????????????????????
        ? (Implementa)
??????????????????????????????????????????????????
? 1. INFRASTRUCTURE LAYER   ? Tecnologías
?    EF Core, JWT, Database, Services            ?
?    Responsabilidad: Detalles técnicos          ?
??????????????????????????????????????????????????
```

---

## ?? Reglas de Dependencia

### ? Las dependencias siempre apuntan hacia adentro

```
Presentation ? Application ? Domain
      ?      ?
Infrastructure ???????????????
```

### ? Domain NUNCA depende de nada

```csharp
// ? CORRECTO - Domain puro
public class Reservation
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    
    public bool OverlapsWith(Reservation other)
    {
        return StartTime < other.EndTime && 
        EndTime > other.StartTime;
    }
}
```

```csharp
// ? INCORRECTO - Domain NO debe tener referencias a EF Core
public class Reservation
{
    [Key] // ? EF Core annotation, NO en Domain
    public int Id { get; set; }
}
```

### ? Application solo depende de Domain

```csharp
// ? CORRECTO - Application usa Domain
public class CreateReservationCommandHandler 
    : IRequestHandler<CreateReservationCommand, Result<ReservationDto>>
{
    // Usa Domain entities y rules
    var reservation = new Reservation { ... };
}
```

### ? Infrastructure implementa interfaces de Application

```csharp
// Application define interfaz
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

// Infrastructure implementa
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    // ...
}
```

---

## ?? Carpetas por Capa

### Domain (Core/Domain/)
```
Core/Domain/
??? Entities/
?   ??? User.cs
?   ??? Resource.cs
?   ??? Reservation.cs
?   ??? BlockedTime.cs
??? Enums/
?   ??? UserRole.cs
?   ??? ReservationStatus.cs
?   ??? ResourceStatus.cs
??? Common/
    ??? BaseEntity.cs
    ??? AuditableEntity.cs
```

**Características:**
- ? Sin referencias a EF Core
- ? Sin referencias a HTTP
- ? Sin referencias a bases de datos
- ? Lógica pura de negocio

---

### Application (Core/Application/)
```
Core/Application/
??? Common/
?   ??? Interfaces/     ? Contratos
?   ??? Behaviours/     ? Pipelines de MediatR
?   ??? Mappings/       ? AutoMapper
?   ??? Models/         ? DTOs
?   ??? Exceptions/     ? Excepciones de negocio
??? Features/           ? CQRS por feature
    ??? Auth/
    ?   ??? Commands/
    ?   ??? Queries/
 ??? Resources/
    ?   ??? Commands/
    ?   ??? Queries/
    ??? Reservations/
  ?   ??? Commands/
    ?   ??? Queries/
  ??? ...
```

**Características:**
- ? CQRS: Commands (escribir) y Queries (leer)
- ? Validaciones con FluentValidation
- ? Mapeos con AutoMapper
- ? Handlers desacoplados
- ? DTOs para entrada/salida

---

### Infrastructure (Infrastructure/)
```
Infrastructure/
??? Persistence/
?   ??? Configurations/      ? Configuraciones EF Core
? ??? Migrations/        ? Migraciones BD
?   ??? ApplicationDbContext.cs
??? Identity/
?   ??? TokenService.cs
?   ??? PasswordHasher.cs
?   ??? CurrentUserService.cs
??? Services/
?   ??? DateTimeService.cs
??? DependencyInjection.cs
```

**Características:**
- ? Implementa interfaces de Application
- ? Entity Framework Core
- ? JWT, contraseñas
- ? Servicios específicos

---

### Presentation (Presentation/)
```
Presentation/
??? Controllers/
?   ??? AuthController.cs
?   ??? ResourcesController.cs
?   ??? ReservationsController.cs
?   ??? BlockedTimesController.cs
?   ??? UsersController.cs
??? Middlewares/
?   ??? ExceptionHandlingMiddleware.cs
?   ??? RequestLoggingMiddleware.cs
??? Filters/
    ??? ApiExceptionFilter.cs
```

**Características:**
- ? Controllers delgados
- ? Delegan a MediatR
- ? Documentados con Swagger
- ? Manejo de errores global

---

## ?? Flujo de un Request

### Ejemplo: Crear una Reserva

```
1. HTTP POST /api/reservations
   Body: {
     "resourceId": 1,
     "startTime": "2025-02-10T10:00:00Z",
     "endTime": "2025-02-10T11:00:00Z"
 }
   
   ?
   
2. ReservationsController recibe
   public async Task<IActionResult> CreateReservation(
       CreateReservationCommand command)
   {
       var result = await _mediator.Send(command);
       return CreatedAtAction(...);
   }
   
   ?
   
3. MediatR enruta a CreateReservationCommandValidator
   public class CreateReservationCommandValidator : 
       AbstractValidator<CreateReservationCommand>
   {
       RuleFor(x => x.ResourceId).GreaterThan(0);
     RuleFor(x => x.StartTime).GreaterThan(DateTime.Now);
       // ...
   }
   
   ?
   
4. MediatR enruta a CreateReservationCommandHandler
   public class CreateReservationCommandHandler : 
       IRequestHandler<CreateReservationCommand, Result<ReservationDto>>
   {
 // Ejecuta lógica de negocio
    // Valida con Domain rules
       // Guarda en BD
   }
   
   ?
   
5. Handler crea entidad Domain
   var reservation = new Reservation
   {
       UserId = _currentUser.UserId,
       ResourceId = request.ResourceId,
       StartTime = request.StartTime,
       EndTime = request.EndTime,
    Status = ReservationStatus.Active
   };
   
   ?
   
6. DbContext guarda en BD
   _context.Reservations.Add(reservation);
   await _context.SaveChangesAsync();
   
   ?
   
7. AutoMapper convierte
   Reservation entity ? ReservationDto
   
   ?
   
8. Controller retorna
   HTTP 201 Created
   {
     "id": 5,
  "resourceId": 1,
"status": "Active",
     ...
   }
```

---

## ?? Ejemplo Comparativo

### ? SIN Clean Architecture (Monolítico)

```csharp
// Todo en un Controller
public class ReservationsController : ControllerBase
{
    private readonly SqlConnection _connection;
    
    [HttpPost]
    public async Task<IActionResult> CreateReservation(
        int resourceId, DateTime start, DateTime end)
    {
     // Validación aquí
        if (start > end) return BadRequest();
    
      // Lógica de negocio aquí
        var overlaps = _connection.QueryFirstOrDefault(
     "SELECT * FROM Reservations WHERE ...");
     if (overlaps != null) return BadRequest();
     
        // Acceso a BD aquí
        _connection.Execute("INSERT INTO ...");
        
        return Ok();
    }
}
```

**Problemas:**
- ? Todo mezclado
- ? Difícil de testear
- ? Difícil de mantener
- ? No escalable

---

### ? CON Clean Architecture (Este proyecto)

```csharp
// 1. Controller (Presentation)
[HttpPost]
public async Task<IActionResult> CreateReservation(
    CreateReservationCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(...);
}

// 2. Validator (Application)
public class CreateReservationCommandValidator
{
    RuleFor(x => x.StartTime).GreaterThan(DateTime.Now);
}

// 3. Handler (Application)
public class CreateReservationCommandHandler
{
    // Validaciones de negocio
    // Llamadas a entidades Domain
}

// 4. Entity (Domain)
public class Reservation
{
    public bool OverlapsWith(Reservation other) { }
}

// 5. DbContext (Infrastructure)
public class ApplicationDbContext : DbContext
{
    public DbSet<Reservation> Reservations { get; set; }
}
```

**Beneficios:**
- ? Separación clara
- ? Testeable
- ? Mantenible
- ? Escalable

---

## ?? Responsabilidades por Capa

| Capa | Responsabilidad | ¿Quién accede a la BD? | ¿Quién valida? |
|------|-----------------|----------------------|----------------|
| **Domain** | Reglas de negocio puras | ? No | ? Sí |
| **Application** | Orquestación de casos de uso | ? No (delega) | ? Sí (FluentValidation) |
| **Infrastructure** | Detalles técnicos, acceso a BD | ? Sí (EF Core) | ? No |
| **Presentation** | Exponer API HTTP | ? No (delega) | ? Sí (Authorization) |

---

## ?? Validaciones en Cada Capa

### Domain Layer
```csharp
// Reglas de dominio puras
public class Reservation
{
    public void Cancel()
    {
    if (Status != ReservationStatus.Active)
       throw new InvalidOperationException(
     "Solo se pueden cancelar reservas activas");
    }
}
```

### Application Layer
```csharp
// Validaciones de entrada con FluentValidation
public class CancelReservationCommandValidator : 
  AbstractValidator<CancelReservationCommand>
{
    public CancelReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId)
            .GreaterThan(0)
        .WithMessage("ID requerido");
    }
}
```

### Presentation Layer
```csharp
// Autorización con roles
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteResource(int id)
{
    var command = new DeleteResourceCommand { Id = id };
    await _mediator.Send(command);
    return NoContent();
}
```

---

## ?? Beneficios de Esta Arquitectura

### ? Testeable
```csharp
// Fácil mockear en tests
var mockContext = new Mock<IApplicationDbContext>();
var handler = new CreateReservationCommandHandler(
    mockContext.Object,
    mockCurrentUser.Object,
    mockMapper.Object);

var result = await handler.Handle(command, CancellationToken.None);
```

### ? Mantenible
```
Cambiar la BD?
? Solo toca Infrastructure

Cambiar validación?
? Solo toca Application

Agregar endpoint?
? Solo toca Presentation

Cambiar regla de negocio?
? Solo toca Domain
```

### ? Escalable
```
Agregar cache?  ? Infrastructure.Services
Agregar evento? ? Application.Features
Agregar endpoint? ? Presentation.Controllers
Nueva feature? ? Application.Features.NewFeature
```

### ? Profesional
```
Código limpie y organizado
Fácil onboarding para nuevos devs
Estándar empresarial
Preparado para producción
```

---

## ?? Comparación con Otras Arquitecturas

| Aspecto | Monolítico | Layered | Clean |
|--------|-----------|--------|-------|
| **Testeable** | ? Difícil | ?? Posible | ? Fácil |
| **Mantenible** | ? No | ?? Depende | ? Sí |
| **Escalable** | ? No | ?? Limitado | ? Muy |
| **Flexible** | ? No | ?? Poco | ? Sí |
| **Profesional** | ? No | ? Sí | ?? Sí |

---

## ? Checklist: ¿Entendí la arquitectura?

- [ ] Conozco las 4 capas
- [ ] Sé que Domain no depende de nada
- [ ] Sé que Application depende solo de Domain
- [ ] Entiendo el flujo de un request
- [ ] Sé qué validaciones van en cada capa
- [ ] Entiendo por qué es testeable

---

## ?? Siguiente

?? **[02-Clean-Architecture.md](02-Clean-Architecture.md)** - Entender qué es Clean Architecture
