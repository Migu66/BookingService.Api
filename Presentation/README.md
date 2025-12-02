# Presentation Layer (Capa de Presentación)

## ?? Descripción
Punto de entrada de la API. Contiene **Controllers**, **Middlewares** y configuración de ASP.NET Core.

## ?? Estructura

### Controllers/
Controladores REST API:
- `AuthController.cs` - Register, Login, RefreshToken
- `ResourcesController.cs` - CRUD de recursos
- `ReservationsController.cs` - Gestión de reservas
- `BlockedTimesController.cs` - Bloqueos (Admin)
- `UsersController.cs` - Perfil de usuario

**Características:**
- Delgados (solo coordinan)
- Delegan a MediatR
- Documentados con XML comments para Swagger
- Validación de autorización con `[Authorize]`

### Middlewares/
Middlewares personalizados:
- `ExceptionHandlingMiddleware.cs` - Captura excepciones globalmente
- `RequestLoggingMiddleware.cs` - Log de requests/responses

### Filters/
Filtros de acción:
- `ValidationFilter.cs` - Validación de ModelState (si no usas FluentValidation pipeline)
- `ApiExceptionFilter.cs` - Alternativa al middleware de excepciones

### Program.cs
Configuración principal:
- Servicios
- Middleware pipeline
- Swagger
- JWT Authentication
- CORS
- Serilog

## ? Principios
- Controllers delgados
- Separación de responsabilidades
- Manejo centralizado de errores
- Respuestas consistentes
