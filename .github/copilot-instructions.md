Quiero que generes una API de Reservas profesional y escalable usando ASP.NET Core 8. La API debe estar diseñada como si fuera para un entorno empresarial moderno, siguiendo buenas prácticas actuales. A continuación te detallo todo lo que necesito:

?? Descripción general del proyecto

Construye una API REST de gestión de reservas donde los usuarios puedan reservar recursos (por ejemplo: salas, pistas, vehículos, herramientas, etc.). El sistema debe permitir:

Crear recursos reservables

Consultar su disponibilidad

Crear reservas sin solapamientos

Cancelar reservas

Bloquear horarios para mantenimiento (solo admins)

Gestionar usuarios con roles (User/Admin)

El proyecto debe estar desarrollado con un enfoque profesional, limpio, mantenible y preparado para crecer.

?? Requisitos esenciales

Quiero que implementes la API usando las prácticas y tecnologías que se usan hoy en día en empresas de desarrollo .NET:

?? Tecnologías y librerías modernas (obligatorias)

ASP.NET Core 8

Entity Framework Core (con Migrations)

MediatR / CQRS para separar comandos y consultas

AutoMapper

FluentValidation

JWT Authentication con roles (User/Admin)

Swagger/OpenAPI bien documentado

Dependency Injection correctamente implementada

Logging estructurado (Serilog recomendado)

Docker para permitir despliegues modernos

SQL Server o PostgreSQL

Opcionales pero recomendadas:

Refresh tokens

Patrón Unit of Work

Manejo global de excepciones

?? Entidades principales

Quiero que generes entidades claras y realistas, con sus reglas de negocio incluidas:

User

Datos básicos (Name, Email, PasswordHash, Role)

Roles: User y Admin

Resource

Nombre, descripción, estado activo/inactivo

Solo administradores pueden gestionarlos

Reservation

Fecha de inicio y fin

Debe validar solapamientos

Debe validar horarios bloqueados

Duración mínima 30 minutos / máxima 4 horas

Estado de reserva (Active, Cancelled, Completed)

BlockedTime

Bloqueos administrativos para mantenimiento

Impiden crear reservas en intervalos afectados

?? Flujo de operaciones y reglas de negocio

La API debe incluir reglas sólidas que reflejen un sistema real usado en empresas:

? Validación de solapamientos

Las reservas no pueden solaparse con otras activas.
Debe usarse una consulta optimizada.

? Validación contra bloqueos

Ningún usuario puede reservar horas bloqueadas.

? Validación de duración

Rechazar reservas que duren demasiado poco o demasiado.

? Validación de disponibilidad

Debe existir un endpoint específico para consultar disponibilidad de un recurso.

? Seguridad por roles

Administradores: CRUD completo de recursos + bloquear horarios + ver todas las reservas.

Usuarios: crear/cancelar reservas propias + ver sus reservas.

?? Endpoints necesarios

Incluye endpoints REST completos:

?? Autenticación

Register

Login con JWT

?? Usuarios

Ver mis reservas

Actualizar mi perfil (opcional)

?? Recursos

Listar recursos

Ver detalle

Crear/editar/eliminar (solo admin)

?? Reservas

Crear

Cancelar

Detalle

Listar (solo admin)

Listar mis reservas (usuario)

?? Disponibilidad

Consultar si un recurso está disponible en una fecha

?? Bloqueos

Crear bloqueo

Eliminar bloqueo

?? Buenas prácticas obligatorias

Quiero que el proyecto use:

Código limpio: nombres claros, SRP, separación de capa de dominio, aplicación e infraestructura.

Validaciones robustas (DTO ? Validator ? Handler).

Handlers CQRS desacoplados.

DTOs para entrada y salida, nunca entidades.

AutoMapper para mapping consistente.

Respuestas uniformes.

Middleware para manejo global de errores.

Logging estructurado para diagnósticos reales.

Documentación Swagger con ejemplos.

Pruebas unitarias e idealmente de integración.

?? Calidad y profesionalismo

El proyecto debe verse como algo que una empresa incluiría en producción:

Código legible y consistente

Arquitectura escalable

Seguridad integrada

Capacidad de añadir nuevos módulos sin romper el sistema

Preparado para microservicios si fuera necesario

Comentarios solo cuando sean realmente necesarios

README completo explicando el proyecto

?? Objetivo final

Quiero que generes el proyecto completo como si fuera una API real de un equipo senior en una empresa moderna que trabaja con ASP.NET. El resultado debe verse muy profesional en GitHub, escalable, mantenible y con buenas prácticas actuales.