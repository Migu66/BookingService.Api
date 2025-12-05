# ?? Comenzando - Getting Started

## ?? Índice de Documentación

Esta carpeta contiene toda la documentación para comenzar con el proyecto.

### ?? Documentos en esta sección:

1. **[00-README.md](00-README.md)** (Este archivo)
   - Índice y guía de inicio rápido

2. **[01-Installation.md](01-Installation.md)**
   - Requisitos previos
   - Instalación y configuración
   - Ejecución local
 - Docker

3. **[02-Quick-Start.md](02-Quick-Start.md)**
   - Inicio rápido en 5 minutos
   - Primer request a la API
   - Comandos útiles

4. **[03-Environment-Setup.md](03-Environment-Setup.md)**
   - Variables de entorno
   - Configuración de appsettings.json
   - User Secrets
   - Configuración de BD

5. **[04-Project-Overview.md](04-Project-Overview.md)**
   - Visión general del proyecto
   - Características principales
   - Tecnologías utilizadas
- Patrones implementados

---

## ?? ¿Por dónde empezar?

### Si es tu primer día:
1. Lee **[04-Project-Overview.md](04-Project-Overview.md)** - Entiende qué hace este proyecto
2. Lee **[01-Installation.md](01-Installation.md)** - Instala las dependencias
3. Lee **[02-Quick-Start.md](02-Quick-Start.md)** - Corre la API localmente

### Si necesitas configurar el proyecto:
? Ve a **[03-Environment-Setup.md](03-Environment-Setup.md)**

### Si necesitas entender la arquitectura:
? Ve a **[../02-Architecture/](../02-Architecture/)**

### Si necesitas guía de desarrollo:
? Ve a **[../03-Development/](../03-Development/)**

### Si necesitas referencia de API:
? Ve a **[../04-API-Reference/](../04-API-Reference/)**

---

## ? Quick Links

| Necesito... | Documento |
|------------|-----------|
| Instalar el proyecto | [Installation.md](01-Installation.md) |
| Ejecutarlo en 5 min | [Quick-Start.md](02-Quick-Start.md) |
| Configurar BD y JWT | [Environment-Setup.md](03-Environment-Setup.md) |
| Entender qué es esto | [Project-Overview.md](04-Project-Overview.md) |
| Entender la arquitectura | [../02-Architecture/](../02-Architecture/) |
| Aprender a desarrollar | [../03-Development/](../03-Development/) |
| Ver endpoints de API | [../04-API-Reference/](../04-API-Reference/) |

---

## ?? Estructura completa de docs

```
docs/
??? 01-Getting-Started/  ? TÚ ESTÁS AQUÍ
?   ??? 00-README.md
?   ??? 01-Installation.md
?   ??? 02-Quick-Start.md
?   ??? 03-Environment-Setup.md
?   ??? 04-Project-Overview.md
?
??? 02-Architecture/              ? Arquitectura y diseño
?   ??? 00-README.md
?   ??? 01-Architecture-Overview.md
?   ??? 02-Clean-Architecture.md
? ??? 03-CQRS-Pattern.md
?   ??? 04-Layers-Explained.md
?   ??? 05-Request-Flow.md
?
??? 03-Development/      ? Guía de desarrollo
?   ??? 00-README.md
?   ??? 01-Quick-Guide.md
?   ??? 02-Project-Structure.md
?   ??? 03-Adding-Features.md
?   ??? 04-Code-Examples.md
? ??? 05-Best-Practices.md
?
??? 04-API-Reference/         ? Referencia de API
    ??? 00-README.md
    ??? 01-Authentication.md
    ??? 02-Resources.md
    ??? 03-Reservations.md
    ??? 04-BlockedTimes.md
    ??? 05-Users.md
```

---

## ?? Rutas de aprendizaje recomendadas

### ?? Soy developer y quiero entender TODO
1. [Project-Overview.md](04-Project-Overview.md)
2. [Installation.md](01-Installation.md)
3. [../../ARCHITECTURE.md](../../ARCHITECTURE.md)
4. [../02-Architecture/](../02-Architecture/)
5. [../03-Development/](../03-Development/)

### ?? Quiero empezar a desarrollar AHORA
1. [Installation.md](01-Installation.md)
2. [Quick-Start.md](02-Quick-Start.md)
3. [../03-Development/01-Quick-Guide.md](../03-Development/01-Quick-Guide.md)

### ??? Necesito entender la arquitectura
1. [Project-Overview.md](04-Project-Overview.md)
2. [../02-Architecture/01-Architecture-Overview.md](../02-Architecture/01-Architecture-Overview.md)
3. [../02-Architecture/02-Clean-Architecture.md](../02-Architecture/02-Clean-Architecture.md)

### ?? Solo necesito usar la API
? Ve a [../04-API-Reference/](../04-API-Reference/)

---

## ? Preguntas frecuentes

**P: ¿Cuáles son los requisitos previos?**
R: .NET 8 SDK, SQL Server/PostgreSQL y opcionalmente Docker
?? [Installation.md](01-Installation.md)

**P: ¿Cuánto tarda en levantarse la API?**
R: ~5 minutos siguiendo el Quick Start
?? [Quick-Start.md](02-Quick-Start.md)

**P: ¿Cómo configuro JWT y la base de datos?**
R: Lee Environment-Setup.md
?? [Environment-Setup.md](03-Environment-Setup.md)

**P: ¿Dónde está la documentación de la API?**
R: En API-Reference y Swagger
?? [../04-API-Reference/](../04-API-Reference/)

**P: ¿Cómo agrego una nueva feature?**
R: Lee Development guides
?? [../03-Development/](../03-Development/)

---

## ?? Necesito ayuda

- **Error en instalación:** [Installation.md](01-Installation.md) ? Troubleshooting
- **No funciona la BD:** [Environment-Setup.md](03-Environment-Setup.md) ? Database
- **¿Cómo agregar funcionalidad?:** [../03-Development/](../03-Development/)
- **Entiendo mal la arquitectura:** [../02-Architecture/](../02-Architecture/)

---

## ?? Recursos externos

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://fluentvalidation.net/)
- [JWT.io](https://jwt.io/)

---

**¡Listo para empezar? ?? [Installation.md](01-Installation.md)**
