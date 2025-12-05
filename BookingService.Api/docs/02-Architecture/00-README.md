# ??? Arquitectura - Architecture Overview

## ?? Índice de Arquitectura

Esta sección explica en detalle cómo está diseñada la aplicación.

### ?? Documentos:

1. **[00-README.md](00-README.md)** (Este)
   - Índice y guía rápida

2. **[01-Architecture-Overview.md](01-Architecture-Overview.md)**
   - Visión general de 4 capas
   - Diagrama de flujo
   - Reglas de dependencia

3. **[02-Clean-Architecture.md](02-Clean-Architecture.md)**
   - Qué es Clean Architecture
   - Por qué la usamos
   - Beneficios

4. **[03-CQRS-Pattern.md](03-CQRS-Pattern.md)**
   - Commands (escritura)
   - Queries (lectura)
   - Handlers
   - Ventajas

5. **[04-Layers-Explained.md](04-Layers-Explained.md)**
   - Domain Layer detallado
   - Application Layer detallado
   - Infrastructure Layer detallado
   - Presentation Layer detallado

6. **[05-Request-Flow.md](05-Request-Flow.md)**
   - Ejemplo completo: Crear una reserva
   - Desde HTTP request hasta BD
   - Código real

---

## ?? Resumen Rápido

### 4 Capas

```
??????????????????????????????????????????
?  PRESENTATION           ?  ? Usuarios
?  Controllers, Middlewares, Swagger     ?
??????????????????????????????????????????
   ? (MediatR)    ?
??????????????????????????????????????????
?  APPLICATION      ?  ? Lógica de negocio
?  CQRS, Handlers, Validators, DTOs      ?
??????????????????????????????????????????
   ?    ?
??????????????????????????????????????????
?  DOMAIN          ?  ? Entidades puras
?  Entities, Rules, Enums    ?
??????????????????????????????????????????
    ?
              ? (Implementa)
??????????????????????????????????????????
?  INFRASTRUCTURE             ?  ? Técnico
?  EF Core, JWT, DB, Services       ?
??????????????????????????????????????????
```

### Principios Clave

? **Independencia** - Domain no depende de nada  
? **Testeable** - Cada capa se testea sola  
? **Flexible** - Cambiar BD sin tocar el negocio  
? **Escalable** - Agregar features fácilmente  

---

## ?? ¿Por dónde empezar?

### Para Entender TODO

```
1. [01-Architecture-Overview.md](01-Architecture-Overview.md)
   ?
2. [02-Clean-Architecture.md](02-Clean-Architecture.md)
   ?
3. [04-Layers-Explained.md](04-Layers-Explained.md)
   ?
4. [05-Request-Flow.md](05-Request-Flow.md)
```

### Para Entender Solo el Flujo

```
[05-Request-Flow.md](05-Request-Flow.md) - Ejemplo código real
```

### Para Entender CQRS

```
[03-CQRS-Pattern.md](03-CQRS-Pattern.md)
```

---

## ??? Estructura de Carpetas

```
Core/
??? Domain/    ? Entidades, Enums
??? Application/      ? Commands, Queries, Validators

Infrastructure/      ? EF Core, JWT, BD

Presentation/        ? Controllers, Middlewares
```

---

## ?? Flujo Simplificado

```
HTTP Request
     ?
[Controller] ? recibe
     ?
[MediatR] ? enruta a handler
     ?
[Validator] ? valida
     ?
[Handler] ? lógica de negocio
     ?
[Domain] ? reglas puras
     ?
[DbContext] ? acceso a BD
     ?
[AutoMapper] ? Entity ? DTO
     ?
[Controller] ? retorna HTTP Response
```

---

## ?? Conceptos Importantes

### CQRS (Command Query Responsibility Segregation)

- **Command** = Escribe datos (CreateReservation)
- **Query** = Lee datos (GetReservations)
- Separados = Escalable

### Clean Architecture

- Domain sin dependencias
- Application solo depende de Domain
- Infrastructure implementa interfaces
- Presentation manda todo a MediatR

### Vertical Slice

- Cada feature es autónoma
- `Features/Reservations/Commands/CreateReservationCommand`
- Cambiar una feature no afecta otras

### Repository Pattern

- DbContext es un repositorio
- Acceso a datos abstracto
- Fácil mockear para tests

---

## ?? Validaciones en Capas

### Domain Layer
```csharp
// Reglas puras
public void Cancel()
{
    if (Status != ReservationStatus.Active)
     throw new InvalidOperationException("Solo activas");
}
```

### Application Layer
```csharp
// FluentValidation
public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
  public CreateReservationValidator()
    {
     RuleFor(x => x.StartTime).GreaterThan(DateTime.Now);
  }
}
```

### Presentation Layer
```csharp
// Authorization
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteResource(int id)
```

---

## ?? Decisiones de Diseño

### ? Por qué Clean Architecture?
- Industria estándar en empresas
- Código mantenible a largo plazo
- Fácil onboarding para nuevos devs
- Testing simplificado

### ? Por qué CQRS?
- Separación de responsabilidades
- Escalable (lectura/escritura por separado)
- Código más limpio y legible
- Preparado para event sourcing

### ? Por qué MediatR?
- Desacoplamiento
- Pipelines de comportamiento
- Fácil agregar validación, logging
- Patrón mediator estándar

### ? Por qué DTOs?
- Encapsulación
- Validación en entrada
- Control de qué exponer en API
- Desacoplamiento de UI y BD

---

## ?? Escalabilidad Potencial

Con esta arquitectura puedes:

1. **Separar Lectura/Escritura**
   - BD separadas para reads/writes
   - CQRS avanzado

2. **Event Sourcing**
   - Historial de todos los eventos
   - Auditoría completa

3. **Microservicios**
   - Cada feature ? servicio
   - API Gateway

4. **Cache**
   - Redis para queries
   - Invalidación por eventos

5. **Message Brokers**
   - RabbitMQ/Kafka
   - Procesamiento async

---

## ? Checklist: ¿Entendí la arquitectura?

- [ ] Conozco las 4 capas
- [ ] Sé qué va en Domain
- [ ] Sé qué va en Application
- [ ] Entiendo CQRS (Commands vs Queries)
- [ ] Sé que Domain NO depende de EF Core
- [ ] Entiendo el flujo de un request
- [ ] Sé dónde validar en cada capa

---

## ?? Siguiente Documento

Elige por donde empezar:

- ?? **[01-Architecture-Overview.md](01-Architecture-Overview.md)** - Empezar desde cero
- ?? **[02-Clean-Architecture.md](02-Clean-Architecture.md)** - Entender Clean Arch
- ?? **[03-CQRS-Pattern.md](03-CQRS-Pattern.md)** - Entender CQRS
- ?? **[04-Layers-Explained.md](04-Layers-Explained.md)** - Detalle de capas
- ?? **[05-Request-Flow.md](05-Request-Flow.md)** - Ver código real

---

**¿Listo? ?? [01-Architecture-Overview.md](01-Architecture-Overview.md)**
