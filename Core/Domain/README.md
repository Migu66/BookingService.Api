# Domain Layer (Capa de Dominio)

## ?? Descripción
Esta capa contiene el **núcleo del negocio**. Es la capa más importante y **no tiene dependencias** de ninguna otra capa.

## ?? Estructura

### Entities/
Contiene las entidades del dominio:
- `User.cs` - Usuario del sistema
- `Resource.cs` - Recurso reservable
- `Reservation.cs` - Reserva
- `BlockedTime.cs` - Bloqueo de horarios

**Características:**
- Propiedades del dominio
- Lógica de negocio interna
- Validaciones básicas
- Invariantes del dominio

### Enums/
Enumeraciones del dominio:
- `UserRole.cs` - Roles (User, Admin)
- `ReservationStatus.cs` - Estados de reserva (Active, Cancelled, Completed)
- `ResourceStatus.cs` - Estado del recurso

### Common/
Clases base y compartidas:
- `BaseEntity.cs` - Entidad base con Id, fechas de auditoría
- `AuditableEntity.cs` - Para auditoría (CreatedBy, ModifiedBy)

## ? Principios
- Sin dependencias externas
- Lógica de negocio pura
- Código agnóstico a infraestructura
